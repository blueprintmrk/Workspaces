// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Serialization;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Maintains the list of items in the history list. The list is an array of phrases, each phrase
    /// containing a list of translations:
    /// 
    /// HistoryList (1x)
    ///     Phrase (n instances)
    ///         Translation (n instances)
    ///         Translation
    ///         Translation
    ///         Translation
    ///         ...
    ///     Phrase
    ///         Translation
    ///         Translation
    ///         Translation
    ///         Translation
    ///         ...
    ///     ...
    ///         
    /// All methods to find and update Phrase and Translations are done through this class.
    /// When the user requests a translation to be performed, this History List is checked first,
    /// using the phrase and the "from" and "to" language via the FindOrAddHistoryItemSourceText method.
    /// If the translation already exists in the History List, the cached values of the translated
    /// text and the speech wave file are returned, and the user will have their translation without
    /// ever hitting the web.
    /// 
    /// In the case a new phrases, the information is built as the translation takes place. For example,
    /// let's say the user asks to translte "Good morning" from English to French, and the HistoryList is
    /// empty. There are multiple times this HistoryList will be called:
    /// 
    /// 1. When the user first presses the Translate button. Since the translation is not found, the
    ///    HistoryList is initialized with the "from" language, the web is called asynchronously,
    ///    and the data structure will look like this:
    ///    
    ///    HistoryList
    ///       Phrase
    ///           Translation "Good Morning", English, (no associated wave file)
    ///        
    /// 2. When the async web call from #1 is received with the translated text, another asynchronous
    ///    call is made to the web to get the wave file for text to speech. The data strcuture is updated to
    ///    look like this:
    ///    
    ///    HistoryList
    ///       Phrase
    ///           Translation "Good Morning", English, (no associated wave file)
    ///           Translation "Bon Jour", French, (no associated wave file)
    /// 
    /// 3. When the async web call from #2 is received with the wave file, this class is called again to get that
    ///    wave file into the list. The SoundFileNameGenerator helper class generates a unique filename
    ///    for the sound file, the wave bytes are saved in the Isolated Storage,
    ///    and the data structure is updated to look like this:
    ///    
    ///    HistoryList
    ///       Phrase
    ///           Translation "Good Morning", English, (no associated wave file)
    ///           Translation "Bon Jour", French, "fr\phr1.wav"
    ///    
    /// 4. In the event the user changed the From/To langauges and asked to translate "Bon Jour" to "Good Morning",
    ///    the "Good Morning" would have been found in the history list, but the Translator class would still
    ///    make the async call to get the english speech to text since it was not in the data structure. When
    ///    that async call returns, the data structure is updated again to look like this:
    ///    
    ///    HistoryList
    ///       Phrase
    ///           Translation "Good Morning", English, "en\phr2.wav"
    ///           Translation "Bon Jour", French, "fr\phr1.wav"
    /// 
    /// </summary>
    public class HistoryList
    {
        /// <summary>
        /// Data file name
        /// </summary>
        const string Filename = "history.xml";

        /// <summary>
        /// Used to generate filenames for text-to-speech
        /// </summary>
        private SoundFilenameGenerator _soundFilenameGenerator = new SoundFilenameGenerator();

        /// <summary>
        /// List of Phrase objects
        /// </summary>
        private ObservableCollection<Phrase> _phrases;

        /// <summary>
        /// SoundFileNameGenerator needs a number to use for uniquely naming files, and
        /// it is saved here
        /// </summary>
        [XmlAttribute] 
        public uint NextFilenameNumber 
        {
            get
            {
                return _soundFilenameGenerator.NextFileNumber;
            }
            set
            {
                _soundFilenameGenerator.NextFileNumber = value;
            }
        }

        /// <summary>
        /// List of Phrase objects
        /// </summary>
        public ObservableCollection<Phrase> Phrases 
        {
            get
            {
                if (_phrases == null)
                {
                    _phrases = new ObservableCollection<Phrase>();
                }
                return _phrases;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public HistoryList()
        {
            _soundFilenameGenerator = new SoundFilenameGenerator();
        }

        /// <summary>
        /// Generates a new history list object from the files in the Content Folder. This is the OOBE
        /// experience where the pre-translated phrases from the Content folder are pulled into the HistoryList.
        /// They are moved into the History list so that the user can delete the ones they dont care about,
        /// as the Content Folder is read-only.
        /// 
        /// The Content folder contains an XML file of the exact same format as the HistoryList saves itself in,
        /// with the exception that the "Content=true" flag is set to true in each Translation object. That flag
        /// allows the Translation object to get the wave file from the Content folder instead of the Isolated
        /// Storage.
        /// </summary>
        /// <returns>A pre-populated HistoryList</returns>
        private static HistoryList GetDefaultHistoryList()
        {
            HistoryList returnValue = null;

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = null;

            // Load from the content folder
            StreamResourceInfo sri = Application.GetResourceStream(new Uri("Content/DefaultPhrases.xml", UriKind.Relative));

            if (sri != null)
            {
                stream = sri.Stream;
            }

            if (stream != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(HistoryList));
                returnValue = (HistoryList)serializer.Deserialize(stream);
                stream.Close();
            }

            if (returnValue == null)
            {
                // Should never happen, but just in case it's better to return an empty list than nothing.
                returnValue = new HistoryList();
            }

            return returnValue;
        }

        /// <summary>
        /// Loads the HistoryList from Isolated Storage. If there is not file in Isolated Storage (OOBE),
        /// then the list if prepopulated with canned phrases.
        /// </summary>
        /// <returns>A HistoryList that's ready to use</returns>
        public static HistoryList Load()
        {
            HistoryList returnValue = null;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(Filename))
                {
                    IsolatedStorageFileStream stream = store.OpenFile(Filename, System.IO.FileMode.Open);
                    XmlSerializer serializer = new XmlSerializer(typeof(HistoryList));
                    try
                    {
                        returnValue = (HistoryList)serializer.Deserialize(stream);
                        stream.Close();
                    }
                    catch (SerializationException se)
                    {
#if DEBUG
                        Debug.WriteLine(se.ToString());
#endif
                    }
                }
            }

            if (returnValue == null)
            {
                returnValue = GetDefaultHistoryList();
            }

            return returnValue;
        }

        /// <summary>
        /// Saves the HistoryList to Isolated Storage
        /// </summary>
        public void Save()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(Filename))
                {
                    store.DeleteFile(Filename);
                }

                IsolatedStorageFileStream stream = store.CreateFile(Filename);
                XmlSerializer serializer = new XmlSerializer(typeof(HistoryList));
                serializer.Serialize(stream, this);
                stream.Close();
            }
        }

        /// <summary>
        /// Returns the the item at the "top" of the History List in the current
        /// langauge. Used when rehydrating, or when the History Page navigates back
        /// to the main page.
        /// </summary>
        public string MostRecentHistoryPhraseText
        {
            get
            {
                string returnValue = String.Empty;

                if (Phrases.Count > 0)
                {
                    returnValue = Phrases[0].CurrentLanguagePhrase;
                }

                return returnValue;
            }
        }

        /// <summary>
        /// Searches the HistoryList for an already existing translation. If one is found, populates
        /// the out parameters with the cached values. If one is not found, adds a new entry for the source
        /// language and text.
        /// </summary>
        /// <param name="fromString">Phrase to be translated from</param>
        /// <param name="fromLanguage">Language to be translated from</param>
        /// <param name="toLanguage">Language to be translated to</param>
        /// <param name="translatedText">Resultant translation if exists, otherwise String.Empty</param>
        /// <param name="translatedSpeech">Bytes of text to speech if exists, otherwise null</param>
        public void FindOrAddHistoryItemSourceText(
            string fromString,
            string fromLanguage,
            string toLanguage,
            out string translatedText,
            out byte[] translatedSpeech
            )
        {
            translatedText = String.Empty;
            translatedSpeech = null;

            Translation translation = FindTranslation(fromString, fromLanguage, toLanguage);
            if (translation != null)
            {
                if (String.IsNullOrEmpty(translation.Text) == false)
                {
                    translatedText = translation.Text;
                    translatedSpeech = translation.GetTranslatedSpeech();
                }
            }
            Dump();
        }

        /// <summary>
        /// Adds a translation (text) to the HistoryList.
        /// </summary>
        /// <param name="fromString">Phrase to be translated from</param>
        /// <param name="fromLanguage">Language to be translated from</param>
        /// <param name="toString">Phrase after being translated</param> 
        /// <param name="toLanguage">Language translated to</param>
        public void AddHistoryItemTranslatedString(
            string fromString, string fromLanguage,
            string toString, string toLanguage
            )
        {
            Phrase phrase = FindPhrase(fromString, fromLanguage);
            if (phrase == null)
            {
                phrase = AddSingleTranslation(fromString, fromLanguage);
            }

            phrase.AddTranslation(toString, toLanguage);
            BubbleToTop(phrase);
            Dump();
        }

        /// <summary>
        /// Saves the wave of a text to speech translation in the HistoryList
        /// </summary>
        /// <param name="fromString">Phrase to be translated from</param>
        /// <param name="fromLanguage">Language to be translated from</param>
        /// <param name="toBytes">byte array of the text to speech wave</param>
        /// <param name="toLanguage">Language translated to</param>
        public void AddHistoryItemTranslatedSpeech(
            string fromString, string fromLanguage,
            byte[] toBytes, string toLanguage
            )
        {
            Translation translation = FindTranslation(fromString, fromLanguage, toLanguage);
            // If the resulting text translation was not saved in the history list
            // (because the translated text was the same as the original text), then
            // there is nowhere to save away the wave file either, so skip this step
            // for that case.
            if (translation != null)
            {
                // Generate a unique filename for this file
                string waveFilename = _soundFilenameGenerator.GetNextFilename(toLanguage);
                translation.SaveWaveFile(toBytes, waveFilename);
                Dump();
            }
        }

        /// <summary>
        /// Adds a single translation which consists of the phrase and the langauge
        /// </summary>
        /// <param name="fromString">The text</param>
        /// <param name="fromLanguage">The Language</param>
        /// <returns>The Phrase which contains the translation</returns>
        private Phrase AddSingleTranslation(string fromString, string fromLanguage)
        {
            Phrase phrase = FindPhrase(fromString, fromLanguage);
            if (phrase == null)
            {
                phrase = new Phrase();
                Phrases.Add(phrase);
            }
            phrase.AddTranslation(fromString, fromLanguage);
            return phrase;
        }

        /// <summary>
        /// Searches the HistoryList for an existing translation
        /// </summary>
        /// <param name="fromString">Phrase to be translated from</param>
        /// <param name="fromLanguage">Language to be translated from</param>
        /// <param name="toLanguage">Language translated to</param>
        /// <returns>The Translation if it exists, null otherwise</returns>
        private Translation FindTranslation(string fromString, string fromLanguage, string toLanguage)
        {
            Translation returnValue = null;

            foreach (Phrase phrase in Phrases)
            {
                returnValue = phrase.FindTranslation(fromString, fromLanguage, toLanguage);
                if (returnValue != null)
                {
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Searches the HistoryList for a Phrase
        /// </summary>
        /// <param name="fromString">Phrase to be translated from</param>
        /// <param name="fromLanguage">Language to be translated from</param>
        /// <returns>The Phrase if it exists, null otherwise</returns>
        private Phrase FindPhrase(string fromString, string fromLanguage)
        {
            Phrase returnValue = null;

            foreach (Phrase phrase in Phrases)
            {
                if (phrase.Matches(fromString, fromLanguage))
                {
                    returnValue = phrase;
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Call this to just bubble a history item to the top
        /// </summary>
        /// <param name="hi"></param>
        public void BubbleToTop(Phrase hi)
        {
            if (Phrases.Count > 1)
            {
                int index = Phrases.IndexOf(hi);
                if (index != -1)
                {
                    Phrase temp = Phrases[index];
                    Phrases.RemoveAt(index);
                    Phrases.Insert(0, temp);
                }
            }
        }

        [Conditional("DEBUG")]
        public void Dump()
        {
            //
            // Turn this on to get a ton of spew showing the list. It slows down the emulator sessions horribly
            //
            //Debug.WriteLine("BuiltInList");
            //Debug.WriteLine("-----------");
            //foreach (Phrase phrase in Phrases)
            //{
            //    phrase.Dump();
            //}
            //Debug.WriteLine("");
        }

    }
}
