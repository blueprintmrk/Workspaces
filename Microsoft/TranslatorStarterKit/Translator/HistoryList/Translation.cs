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
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Xml.Serialization;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Contains information about one Translation. A translation has a text string, a filename of the wave file
    /// for the text to speech, and the langauge of the text. A Phrase (the parent owner of this class) is a list
    /// of Translations one different langauges, i.e.:
    /// 
    ///       Phrase
    ///           Translation "Good Morning", English, "en\phr2.wav"
    ///           Translation "Bon Jour", French, "fr\phr1.wav"
    /// 
    /// </summary>
    public class Translation
    {
        /// <summary>
        /// The text of the translations
        /// </summary>
        [XmlText]
        public string Text { get; set; }

        /// <summary>
        /// The langauge, in the two-letter ISO format, i.e. "en" or "fr", etc
        /// </summary>
        [XmlAttribute]
        public string Language { get; set; }

        /// <summary>
        /// The name of the wave file that contains the text to speech sound
        /// </summary>
        [XmlAttribute]
        public string WaveFilename { get; set; }

        /// <summary>
        /// Set to true if the wave file is in the Content folder (built-in, was deployed from the
        /// XAP file), or false if the wave file was saved in isolated storage.
        /// </summary>
        [XmlAttribute]
        public bool FromContent { get; set; }

        /// <summary>
        /// When the translator class finds cached away translations, it will end up calling this
        /// method, which will load the wave file from the ISO store given the filename generated in
        /// SetTranslatedSpeech
        /// </summary>
        /// <returns>Byte array of the waveform for the Text to Speech, null if not found in the HistoryList file cache</returns>
        public byte[] GetTranslatedSpeech()
        {
            byte[] returnValue = null;

            if (FromContent == true)
            {
                //
                // This is how the default phrases are loaded, since those wave files are not in the ISO
                // store and instead are content files
                //
                Stream stream = Application.GetResourceStream(new Uri("Content/" + WaveFilename, UriKind.Relative)).Stream;
                returnValue = GetBytesFromStream(stream);
                stream.Close();
            }
            else
            {
                //
                // This is how the custom phrases are loaded, from the ISO store file
                //
                if (string.IsNullOrEmpty(WaveFilename) == false)
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.FileExists(WaveFilename))
                        {
                            IsolatedStorageFileStream stream = store.OpenFile(WaveFilename, System.IO.FileMode.Open);
                            returnValue = GetBytesFromStream(stream);
                        }
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Does exactly what its name implies
        /// </summary>
        /// <param name="stream">A stream</param>
        /// <returns>A byte[] array</returns>
        private static byte[] GetBytesFromStream(Stream stream)
        {
            long length = stream.Seek(0, System.IO.SeekOrigin.End);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] returnValue = new byte[length];
            stream.Read(returnValue, 0, (int)length);
            stream.Close();
            return returnValue;
        }

        /// <summary>
        /// Returns true if the Translation is the specified langauge
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>True if the Translation is the specified langauge</returns>
        public bool IsLangauge(string language)
        {
            return ((String.Compare(Language, language, CultureInfo.CurrentUICulture, CompareOptions.IgnoreCase) == 0));
        }

        /// <summary>
        /// Returns true if the Translation is the specified text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>True if the Translation's text matches the text passed in</returns>
        public bool IsText(string text)
        {
            return ((String.Compare(Text, text, CultureInfo.CurrentUICulture, CompareOptions.IgnoreCase) == 0));
        }

        /// <summary>
        /// Saves the byte array of the wave file to the specified filename
        /// </summary>
        /// <param name="translatedSpeech">Byte array of the waveform</param>
        /// <param name="waveFilename">Filename</param>
        public void SaveWaveFile(byte[] translatedSpeech, string waveFilename)
        {
            if (FromContent == false)
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(waveFilename))
                    {
                        store.DeleteFile(waveFilename);
                    }

                    IsolatedStorageFileStream stream = store.CreateFile(waveFilename);
                    stream.Write(translatedSpeech, 0, translatedSpeech.Length);
                    stream.Close();

                    WaveFilename = waveFilename;
                }
            }
        }
    }
}
