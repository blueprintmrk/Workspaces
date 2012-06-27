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

using System.Collections.Generic;
using System.Linq;
using System;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// The Data Model for this application. Consists of the AppSettings (which is nothing but the
    /// "from" and "to" languages), and the History List, and the list of available langauges.
    /// 
    /// Saved in Isolated Storage App Settings:
    ///   From Langauge
    ///   To Langauge
    ///   
    /// Saved in normal Isolated Storage:
    ///   History List
    ///   
    /// Only kept in Memory:
    ///   Available languages
    ///   
    /// </summary>
    public class Model
    {
        /// <summary>
        /// Application Settings.
        /// </summary>
        private AppSettings _appSettings;
        /// <summary>
        /// The Hsitory List
        /// </summary>
        private HistoryList _historyList;

        /// <summary>
        /// The list of available languages
        /// </summary>
        private List<LanguageInformation> _availableLangauges;
        /// <summary>
        /// The "from" language (in memory cache)
        /// </summary>
        private LanguageInformation _fromLanguage;
        /// <summary>
        /// The "to" language  (in memory cache)
        /// </summary>
        private LanguageInformation _toLanguage;

        /// <summary>
        /// The list of available languages
        /// </summary>
        public List<LanguageInformation> AvailableLangauges
        {
            get 
            {
                if (_availableLangauges == null)
                {
                    _availableLangauges = new List<LanguageInformation>();
                    string[] split = LocalizationStrings.Strings.SupportedLanguages.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string lang in split)
                    {
                        _availableLangauges.Add(new LanguageInformation(lang));
                    }

                }
                return _availableLangauges; 
            }
        }

        /// <summary>
        /// Setting this to true will bypass all checks to see if a call to a web API is required.
        /// Used by test methods.
        /// </summary>
        public bool TestingParameterForceAllWebCalls = false;

        /// <summary>
        /// The current source language
        /// </summary>
        public LanguageInformation FromLanguage
        {
            get
            {
                if (_fromLanguage == null)
                {
                    _fromLanguage = FindLanguage(_appSettings.FromLangaugeCode);
                }
                return _fromLanguage;
            }
            set
            {
                _fromLanguage = value;
                if (_appSettings.FromLangaugeCode != _fromLanguage.TwoLetterISOLanguageName)
                {
                    _appSettings.FromLangaugeCode = _fromLanguage.TwoLetterISOLanguageName;
                }
            }
        }
        /// <summary>
        /// The current destination language
        /// </summary>
        public LanguageInformation ToLanguage
        {
            get
            {
                if (_toLanguage == null)
                {
                    _toLanguage = FindLanguage(_appSettings.ToLangaugeCode);
                }
                return _toLanguage;
            }
            set
            {
                _toLanguage = value;
                if (_appSettings.ToLangaugeCode != _toLanguage.TwoLetterISOLanguageName)
                {
                    _appSettings.ToLangaugeCode = _toLanguage.TwoLetterISOLanguageName;
                }
            }
        }

        /// <summary>
        /// The History List of translations
        /// </summary>
        public HistoryList HistoryList
        {
            get
            {
                if (_historyList == null)
                {
                    _historyList = HistoryList.Load();
                }
                return _historyList;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Model()
        {
            _appSettings = new AppSettings();
        }

        /// <summary>
        /// Saves any Isolated Storage data
        /// </summary>
        public void Save()
        {
            HistoryList.Save();
        }

        /// <summary>
        /// Given a TwoLetterISOLanguageName return the Translator LanguageInformation for the same language
        /// </summary>
        /// <param name="twoLetterISOLanguageName"></param>
        /// <returns>The matching LanguageInformation, or the first one in the list if the code is not found</returns>
        public LanguageInformation FindLanguage(string twoLetterISOLanguageName)
        {
            return (from found in AvailableLangauges
                    where String.Compare(found.TwoLetterISOLanguageName, twoLetterISOLanguageName, StringComparison.CurrentCultureIgnoreCase) == 0
                    select found).FirstOrDefault<LanguageInformation>();
        }


        /// <summary>
        /// Given one LanguageInformation, find the first different one in the availible ones.
        /// </summary>
        /// <param name="avoid"></param>
        /// <returns>The LanguageInforation object that does not match the "avoid" specimen</returns>
        public LanguageInformation FindDifferentLanguage(LanguageInformation avoid)
        {
            return (from found in AvailableLangauges
                    where !found.Equals(avoid)
                    select found).FirstOrDefault<LanguageInformation>();
        }

        /// <summary>
        /// Swaps the langauges (source and dest)
        /// </summary>
        public void SwapLangauges()
        {
            LanguageInformation temp = ToLanguage;
            ToLanguage = FromLanguage;
            FromLanguage = temp;
            App.Translator.FromPhrase = String.Empty;
            App.Translator.ToPhrase = String.Empty;
        }


        /// <summary>
        /// Entry point into app settings to check the history. Given in incoming phrase, and checking
        /// the current langauge, the History class will set itself up for the incoming values later on.
        /// if there are already values in the History cache, they are retrieved and placed in the out
        /// parameters
        /// </summary>
        /// <param name="text">String to translate from</param>
        /// <param name="translatedText">Translated String if found in History List</param>
        /// <param name="translatedSpeech">Text to Speech if found in History List</param>
        public void FindOrAddHistory(string text, out string translatedText, out byte[] translatedSpeech)
        {
            translatedText = string.Empty;
            translatedSpeech = null;

            HistoryList.FindOrAddHistoryItemSourceText(
                text,
                App.Model.FromLanguage.TwoLetterISOLanguageName,
                App.Model.ToLanguage.TwoLetterISOLanguageName,
                out translatedText, out translatedSpeech
                );
        }

        /// <summary>
        /// Adds translated text to the History List
        /// </summary>
        /// <param name="fromString">Phrase to be translated from</param>
        /// <param name="fromLanguage">Language to be translated from</param>
        /// <param name="toString">Phrase after being translated</param> 
        /// <param name="toLanguage">Language translated to</param>
        public void AddTranslatedText(
            string fromString, string fromLanguage,
            string toString, string toLanguage
            )
        {
            HistoryList.AddHistoryItemTranslatedString(
                fromString, fromLanguage,
                toString, toLanguage
                );
        }

        /// <summary>
        /// Entry point into app settings for when some new translated wave files comes back from the cloud
        /// </summary>
        /// <param name="fromString">Phrase to be translated from</param>
        /// <param name="fromLanguage">Language to be translated from</param>
        /// <param name="toBytes">byte array of the text to speech wave</param>
        /// <param name="toLanguage">Language translated to</param>
        public void AddTranslatedSpeech(
            string fromString, string fromLanguage,
            byte[] toBytes, string toLanguage
            )
        {
            HistoryList.AddHistoryItemTranslatedSpeech(
                fromString, fromLanguage,
                toBytes, toLanguage
                );
        }

        /// <summary>
        /// When the History Picker page selects something, this will bubble it to the top
        /// and set the dirty bit so that the main page can reflect the changes.
        /// </summary>
        /// <param name="hi"></param>
        public void ChooseFromHistory(Phrase phrase)
        {
            HistoryList.BubbleToTop(phrase);
        }
    }
}
