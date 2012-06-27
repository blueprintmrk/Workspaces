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
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Represents a phrase in the History List. A phrase is a list of Translation objects, each 
    /// translation represents the same meaning in a different language. For example this phrase
    /// contains two translations:
    /// 
    ///       Phrase
    ///           Translation "Good Morning", English, "en\phr2.wav"
    ///           Translation "Bon Jour", French, "fr\phr1.wav"
    /// 
    /// </summary>
    public class Phrase
    {
        /// <summary>
        /// List of Translation objects
        /// </summary>
        private List<Translation> _translations;

        /// <summary>
        /// List of Translation objects
        /// </summary>
        public List<Translation> Translations
        {
            get
            {
                if (_translations == null)
                {
                    _translations = new List<Translation>();
                }
                return _translations;
            }
        }

        /// <summary>
        /// Searches the phrase for a Translation from one language to another. In order for this
        /// to succeed, Translation objects for both languages must exist in this Phrase object.
        /// </summary>
        /// <param name="fromString">Phrase to be translated from</param>
        /// <param name="fromLanguage">Language to be translated from</param>
        /// <param name="toLanguage">Language translated to</param>
        /// <returns>The Translation if it exists, null otherwise</returns>
        public Translation FindTranslation(string fromString, string fromLanguage, string toLanguage)
        {
            Translation returnValue = null;

            if (Matches(fromString, fromLanguage))
             {
                // Getting here means we found the "from" string.
                // Now, time to see if the list contains the "to" string
                returnValue = FindTranslation(toLanguage);
            }

            return returnValue;
        }

        /// <summary>
        /// Searches the Phrase for a translation that has the indicated Language
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>The Translation if it exists, null otherwise</returns>
        public Translation FindTranslation(string language)
        {
            Translation returnValue = null;

            foreach (Translation translation in Translations)
            {
                if (translation.IsLangauge(language))
                {
                    returnValue = translation;
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Returns the text of the Translation of the app's current "From" language. Used
        /// by the HistoryList picker as the Binding field.
        /// </summary>
        public string CurrentLanguagePhrase
        {
            get
            {
                string returnValue = String.Empty;

                Translation translation = FindTranslation(App.Model.FromLanguage.TwoLetterISOLanguageName);
                if (translation != null)
                {
                    returnValue = translation.Text;
                }

                return returnValue;
            }
        }

        /// <summary>
        /// Adds a translation to the Phrase
        /// </summary>
        /// <param name="text">Text to add</param>
        /// <param name="language">Language</param>
        public void AddTranslation(string text, string language)
        {
            Translation translation = FindTranslation(language);
            if (translation == null)
            {
                translation = new Translation();
                Translations.Add(translation);
            }

            translation.Language = language.ToString();
            translation.Text = text;
        }

        /// <summary>
        /// Returns true if the Phrase contains a Translation that has the same text
        /// and langauge
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <param name="language">Language to check</param>
        /// <returns>True if the text and language match a Translatio in this Phrase</returns>
        public bool Matches(string text, string language)
        {
            bool returnValue = false;

            foreach (Translation translation in Translations)
            {
                if (translation.IsText(text) && translation.IsLangauge(language))
                {
                    returnValue = true;
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Returns true if this phrase supports a certain Language. Used by the LINQ query in the History page
        /// </summary>
        /// <param name="language"></param>
        /// <returns>True if a Translation in this Phrase is in the langauge</returns>
        public bool SupportsLangauge(string language)
        {
            bool returnValue = false;

            foreach (Translation translation in Translations)
            {
                if (translation.IsLangauge(language))
                {
                    returnValue = true;
                    break;
                }
            }

            return returnValue;
        }

        [Conditional("DEBUG")]
        public void Dump()
        {
            Debug.WriteLine("Phrase");
            foreach (Translation translation in Translations)
            {
                Debug.WriteLine("  [" + translation.Language + "] " + translation.Text + " [" + translation.WaveFilename + "]" );
            }
        }
    }
}
