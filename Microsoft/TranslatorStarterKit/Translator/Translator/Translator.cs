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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Class to use the cloud services to translate text from one language to another
    /// </summary>
    public class Translator : DependencyObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string property)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                }
            }
            );
        }

        #endregion

        /// <summary>
        /// From text
        /// </summary>
        private string _fromPhrase = String.Empty;

        /// <summary>
        /// To text
        /// </summary>
        private string _toPhrase = String.Empty;

        /// <summary>
        /// True if the translation (To text) is complete and valid.
        /// </summary>
        private bool _resultValid = false;

        /// <summary>
        /// Any current request in action
        /// </summary>
        private WebRequestHelper.RequestInfo _currentRequest = null;

        /// <summary>
        /// Cached away last phrase
        /// </summary>
        private string _lastPhraseString = String.Empty;

        /// <summary>
        /// Cached away from langauge
        /// </summary>
        private string _lastFromLanguage = string.Empty;

        /// <summary>
        /// Cached away to langauge
        /// </summary>
        private string _lastToLanguage = string.Empty;

        /// <summary>
        /// True if the translation is complete and valid.
        /// </summary>
        public bool ResultValid
        {
            get { return _resultValid; }
            set
            {
                _resultValid = value;
                NotifyPropertyChanged("ResultValid");
            }
        }

        /// <summary>
        /// The From text
        /// </summary>
        public string FromPhrase
        {
            get { return _fromPhrase; }
            set
            {
                _fromPhrase = value;
                NotifyPropertyChanged("FromPhrase");

                if (String.IsNullOrEmpty(value))
                {
                    ToPhrase = value;
                }
            }
        }

        /// <summary>
        /// The To text (only if ResultValid is true)
        /// </summary>
        public string ToPhrase
        {
            get { return _toPhrase; }
            set
            {
                _toPhrase = value;
                NotifyPropertyChanged("ToPhrase");
            }
        }

        /// <summary>
        /// Translates the current FromPhrase. Once the translation is successful, the ToPhrase
        /// will update.
        /// </summary>
        /// <param name="fromLang">The From Langauge</param>
        /// <param name="toLang">To To Language</param>
        /// <param name="foundSpeechInHistoryList">If true, then the speech file existed in the History List and
        /// the speech is immediately available.</param>
        public void Translate(string fromLang, string toLang, out bool foundSpeechInHistoryList)
        {
            foundSpeechInHistoryList = false;

            // Trim the leading and trailing spaces
            _fromPhrase = _fromPhrase.Trim();

            if (string.IsNullOrEmpty(_fromPhrase) == false)
            {
                string historyListTranslatedText;
                byte[] historyListTranslatedSpeech;

                // Lowercase the first letter of _fromPhrase if it is one word long 
                string[] split = _fromPhrase.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 1)
                {
                    _fromPhrase = _fromPhrase.ToLower();
                }

                App.Model.FindOrAddHistory(_fromPhrase, out historyListTranslatedText, out historyListTranslatedSpeech);

                //
                // Check values from HistoryList first
                //
                if (string.IsNullOrEmpty(historyListTranslatedText) == false)
                {
                    ToPhrase = historyListTranslatedText;
                    if (historyListTranslatedSpeech != null)
                    {
                        App.Speech.LastPhraseBytes = historyListTranslatedSpeech;
                        foundSpeechInHistoryList = true;
                    }
                    else
                    {
                        App.Speech.GetTextToSpeech(
                            _fromPhrase,
                            historyListTranslatedText,
                            fromLang,
                            toLang
                            );
                    }
                }
                else
                {
                    CallWebApi(_fromPhrase, fromLang, toLang);
                }
            }
        }

        /// <summary>
        /// Calls async web api to get the text translation
        /// </summary>
        /// <param name="fromString">From text</param>
        /// <param name="fromLang">From language</param>
        /// <param name="toLang">To langauge</param>
        /// <returns>True if a web call was made, false if the translator already had this phrase cached away</returns>
        protected bool CallWebApi(
            string fromString,
            string fromLang,
            string toLang
            )
        {
            bool callMade = false;

            if (
                (String.Compare(fromString, _lastPhraseString, StringComparison.CurrentCultureIgnoreCase) != 0)
                ||
                (fromLang != _lastFromLanguage) || (toLang != _lastToLanguage)
                ||
                (App.Model.TestingParameterForceAllWebCalls == true)
                )
            {
                _lastPhraseString = fromString;
                _lastFromLanguage = fromLang;
                _lastToLanguage = toLang;
                callMade = true;

                //
                // If there was already a translate request out on the wire, don't bother listening to it any more
                //
                if (_currentRequest != null)
                {
                    _currentRequest.Cancelled = true;
                }

                _currentRequest = InitiateRequest(GetUriRequest(fromString, fromLang, toLang), fromString, fromLang, toLang);
            }

            return callMade;
        }

        /// <summary>
        /// Builds the URI for the Web call
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="fromLang"></param>
        /// <param name="toLang"></param>
        /// <returns>The URI</returns>
        private string GetUriRequest(string phrase, string fromLang, string toLang)
        {
            string apiFormat = LocalizationStrings.Strings.TextTranslateURI;
            string uriRequest =
                  String.Format(
                    apiFormat,
                    LocalizationStrings.Strings.TextTranslateID,
                    Uri.EscapeDataString(phrase),
                    fromLang,
                    toLang
                    );
            return uriRequest;
        }

        /// <summary>
        /// Makes actual call to cloud to get translation. When result is retrieved,
        /// the HistoryList is updated, and the Speech object is set into motion to
        /// get the text to speech.
        /// </summary>
        /// <param name="uriRequest">URI</param>
        /// <param name="fromString">From text</param>
        /// <param name="fromLang">From langauge</param>
        /// <param name="toLang">To language</param>
        /// <returns>A WebRequestHelper.RequestInfo which contains information about the current request in the cloud.</returns>
        private WebRequestHelper.RequestInfo InitiateRequest(
            string uriRequest,
            string fromString,
            string fromLang,
            string toLang
            )
        {
            // The signature for SendStringRequest looks like this:
            //
            //  public static RequestInfo 
            //      SendStringRequest(
            //          string uriString,
            //          Action sent,
            //          Action<string> received,
            //          Action<string> failed );
            //
            WebRequestHelper.RequestInfo returnValue =
                    WebRequestHelper.SendStringRequest(
                        uriRequest,
                        () => // Sent()
                        {
                            // Update the UI to let the user know the request is out on the wire
                            ToPhrase = LocalizationStrings.Strings.Sending;
                            ResultValid = false;
                            App.Speech.ResultValid = false;
                        },
                        (resultXML) => // Received(string resultXML)
                        {
                            // This code is called from the WebRequest's thread, so anything that touches the UI
                            // will need to be marshalled
                            string translatedText = ParseResult(resultXML);

                            if (string.IsNullOrEmpty(translatedText))
                            {
                                ReportWebError();
                            }
                            else
                            {
                                // There are going to be cases where the translator service does not
                                // translate a word, and it returns the source text. In these cases,
                                // we don't want to save away the translation. That way, if the translator
                                // service is updated in the future to correctly translate that word,
                                // the correct translation will be stored in the history list. Otherwise,
                                // the incorrect translation will be cached away and the user will never
                                // get to benefit.
                                if (String.Compare(fromString, translatedText, StringComparison.CurrentCultureIgnoreCase) != 0)
                                {
                                    App.Model.AddTranslatedText(fromString, fromLang, translatedText, toLang);
                                }
                                else
                                {
                                    // And, if the phrase was not cached, reset the last phrase so that each subsequent call
                                    // will hit the web again (this value is used to optimize web calls at the CallApi method)
                                    _lastPhraseString = string.Empty;
                                }
                                // Get the result and update the UI
                                // Since both of these properties use the NotifyPropertyChanged
                                // method, which uses BeginInvoke to ensure the call is made on the
                                // UI thread, we do not need to marshall code here.
                                ToPhrase = translatedText;
                                ResultValid = true;

                                // Send the Speech object on its way to get the text to speech
                                App.Speech.GetTextToSpeech(
                                    fromString,
                                    translatedText,
                                    fromLang,
                                    toLang
                                    );
                            }

                        },
                        (errorMsg) => // Failed(string errorMsg)
                        {
                            ReportWebError();
                        }
                    );

            return returnValue;
        }

        /// <summary>
        /// Reports an error
        /// </summary>
        private void ReportWebError()
        {
            _lastPhraseString = String.Empty;
            ToPhrase = LocalizationStrings.Strings.ErrorTranslating;
            Speech.TestingManualResetEvent.Set();
        }

        /// <summary>
        /// Parses the XML into human readable format.
        /// Input format looks like this:
        /// 
        /// <string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">Bonjour</string>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>The translated text in human-readable form</returns>
        private string ParseResult(string xml)
        {
            string returnValue = string.Empty;

            if (string.IsNullOrEmpty(xml) == false)
            {
                try
                {
                    using (StringReader stringReader = new StringReader(xml))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(stringReader))
                        {
                            xmlReader.ReadStartElement();
                            if (xmlReader.ValueType.Name == "String")
                            {
                                returnValue = xmlReader.Value;
                            }
                        }
                    }
                }
                catch (XmlException)
                {
                    // Any XML parse errors means we let the default null return, which
                    // will put up a friendly error message
                }
            }

            // White space often comes back in the translations, lose that.
            return returnValue.Trim();
        }
    }
}
