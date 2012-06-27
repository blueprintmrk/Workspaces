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
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;
using System.Windows;
using System.Threading;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Class to use the cloud to get a wave file of text translated to speech, and play it back.
    /// </summary>
    public class Speech : DependencyObject, INotifyPropertyChanged, IDisposable
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
        /// The bytes at the start of the wave file
        /// </summary>
        const int cHeaderSize = 128;

        /// <summary>
        /// Event fired when the wave file has loaded.
        /// </summary>
        public event EventHandler ReadyToSpeak;

        /// <summary>
        /// Set to true when there is something to say
        /// </summary>
        private bool _resultValid = false;

        /// <summary>
        /// The web request helper class
        /// </summary>
        private WebRequestHelper.RequestInfo _currentRequest = null;

        /// <summary>
        /// Used by test classes to track the asyn nature of this class
        /// </summary>
        private static ManualResetEvent _testingManualResetEvent;

        /// <summary>
        /// XNA Sound Player
        /// </summary>
        private DynamicSoundEffectInstance _player;

        /// <summary>
        /// Cached away wave file from last time played
        /// </summary>
        private byte[] _lastPhraseBytes;

        /// <summary>
        /// XNA needs to have FrameworkDispatcher.Update() called while playing
        /// sound effects, and this timer is how we keep calling it.
        /// </summary>
        private DispatcherTimer _xnaUpdateTimer;

        /// <summary>
        /// In order to know when a wave file is DONE playing, we need to listen for
        /// the second BufferNeeded event. This bool is used to ignore the first one.
        /// </summary>
        private bool _ignoreNextBufferNeeded;

        /// <summary>
        /// Set to true when there is something to say
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
        /// Used by test classes to track the async nature of this class
        /// </summary>
        public static ManualResetEvent TestingManualResetEvent
        {
            get
            {
                if (_testingManualResetEvent == null)
                {
                    _testingManualResetEvent = new ManualResetEvent(false);
                }
                return _testingManualResetEvent;
            }
        }

        /// <summary>
        /// Cached away wave file from last time played
        /// </summary>
        public byte[] LastPhraseBytes
        {
            get { return _lastPhraseBytes; }
            set 
            {
                _lastPhraseBytes = value;
                if (value != null)
                {
                    ResultValid = true;
                }
                else
                {
                    ResultValid = false;
                }
            }
        }

        /// <summary>
        /// Set to true while the wave file is playing (used for things such as graying out the
        /// speak button while it's speaking)
        /// </summary>
        public bool IsSpeaking
        {
            get { return (bool)GetValue(IsSpeakingProperty); }
            set { SetValue(IsSpeakingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSpeaking.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSpeakingProperty =
            DependencyProperty.Register("IsSpeaking", typeof(bool), typeof(Speech), new PropertyMetadata(false));
        
        /// <summary>
        /// Constructor
        /// </summary>
        public Speech()
        {
        }

        /// <summary>
        /// Gets the text to speech wave asynch from the cloud
        /// </summary>
        /// <param name="fromString">Text in "from" langauge.</param>
        /// <param name="toString">Text in "to" langauge</param>
        /// <param name="fromLang">From Langauge</param>
        /// <param name="toLang">To langauge</param>
        public void GetTextToSpeech(string fromString, string toString, string fromLang, string toLang)
        {
            // The reason for the fromString getting passed onto InitiateRequest is that the
            // Historylist needs to know the source phrase so that it can put the wave file in the
            // correct place

            //
            // If there was already a translate request out on the wire, don't bother listening to it any more
            //
            if (_currentRequest != null)
            {
                _currentRequest.Cancelled = true;
            }

            _currentRequest = InitiateRequest(GetUriRequest(toString, fromLang, toLang), fromString, fromLang, toString, toLang);
        }

        /// <summary>
        /// Builds the uri for the cloud API call
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="fromLang">From Langauge</param>
        /// <param name="toLang">To Language</param>
        /// <returns>The URI</returns>
        private string GetUriRequest(string text, string fromLang, string toLang)
        {
            string apiFormat = LocalizationStrings.Strings.TextToSpeechURI;
            string uriRequest =
                  String.Format(
                    apiFormat,
                    LocalizationStrings.Strings.TextToSpeechID,
                    Uri.EscapeDataString(text),
                    fromLang.ToString(),
                    toLang.ToString()
                    );

            return uriRequest;
        }

        /// <summary>
        /// Makes the call to the cloud API
        /// </summary>
        /// <param name="uriRequest">The URI</param>
        /// <param name="fromString">The "from" text</param>
        /// <param name="fromLang">The "from" language</param>
        /// <param name="toString">Text in "to" langauge</param>
        /// <param name="toLang">The "to" langauge</param>
        /// <returns>A WebRequestHelper.RequestInfo which contains information about the current request in the cloud.</returns>
        private WebRequestHelper.RequestInfo InitiateRequest(
            string uriRequest, 
            string fromString, 
            string fromLang, 
            string toString,
            string toLang
            )
        {
            //
            // Clear the cached speech
            //
            LastPhraseBytes = null;

            // The signature of WebRequestHelper.SendByteRequest is
            //
            //      public static RequestInfo SendByteRequest(
            //          string uriString,
            //          Action sent,
            //          Action<byte[]> received,
            //          Action<string> failed
            //
            // So we are able to put all the actions for sent/received/failed 
            // down in this one section of code.

            return WebRequestHelper.SendByteRequest(
                                    uriRequest,
                                    () => // Sent()
                                    {
                                        ResultValid = false;
                                    },
                                    (resultBytes) => // Received(byte[] resultBytes)
                                    {
                                        _lastPhraseBytes = resultBytes;

                                        // There are going to be cases where the translator service does not
                                        // translate a word, and it returns the source text. In these cases,
                                        // we don't want to save away the translation. That way, if the translator
                                        // service is updated in the future to correctly translate that word,
                                        // the correct translation will be stored in the history list. Otherwise,
                                        // the incorrect translation will be cached away and the user will never
                                        // get to benefit.
                                        if (String.Compare(fromString, toString, StringComparison.CurrentCultureIgnoreCase) != 0)
                                        {
                                            App.Model.AddTranslatedSpeech(fromString, fromLang, resultBytes, toLang);
                                        }

                                        ResultValid = true;
                                        if (ReadyToSpeak != null)
                                        {
                                            ReadyToSpeak(this, EventArgs.Empty);
                                        }
                                        TestingManualResetEvent.Set();
                                    },
                                    (errorMsg) => // Failed(string errorMsg)
                                    {
                                        TestingManualResetEvent.Set();
                                    }
                                );
        }

        /// <summary>
        /// Call this method to make the device speak
        /// </summary>
        public void Speak()
        {
            if (!IsSpeaking && (_lastPhraseBytes != null) )
            {
                SpeakResult(_lastPhraseBytes);
            }
        }

        /// <summary>
        /// Converts the wave into a format playable by the XNA player and plays it
        /// </summary>
        /// <param name="soundBytes"></param>
        private void SpeakResult(byte[] soundBytes)
        {
            byte[] upscaled = ConvertTo16BitAudio(soundBytes, cHeaderSize, soundBytes.Length - cHeaderSize);

            if (upscaled.Length > 0)
            {
                if (_xnaUpdateTimer == null)
                {
                    //
                    // The XNA framework has some code that assumes a game loop is running.
                    // Playback of wave files is one of them, and without calls to FrameworkDispatcher.Update(),
                    // the wave files could get garbled, and the BufferNeeded event will not fire. Since this class
                    // needs the BufferNeeded event, and since the user wants a clean wave file playback, this
                    // snippet of code will call the FrameworkDispatcher.Update() method at a high frequency.
                    // The timer is turned on when the _player starts to play, and it is turned off when the
                    // BufferNeeded is called. This preserves battery life.
                    //
                    _xnaUpdateTimer = new DispatcherTimer();
                    _xnaUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
                    _xnaUpdateTimer.Tick += (s, e) =>
                    {
                        FrameworkDispatcher.Update();
                    };
                }

                // Initialize the player
                if (_player == null)
                {
                    _player = new DynamicSoundEffectInstance(8000, AudioChannels.Mono);
                    _player.BufferNeeded += OnPlayerBufferNeeded;
                }
                // Need to send the first Update immediately, as the timer wont send one for a period
                FrameworkDispatcher.Update();
                _ignoreNextBufferNeeded = true;
                _player.SubmitBuffer(upscaled);
                _player.Play();
                FrameworkDispatcher.Update();
                // Turn on the XNA timer to ensure the wave file is ungarbled.
                _xnaUpdateTimer.Start();

                IsSpeaking = true;
            }
        }

        private void OnPlayerBufferNeeded(object sender, EventArgs e)
        {
            if (_ignoreNextBufferNeeded)
            {
                _ignoreNextBufferNeeded = false;
                return;
            }
            else
            {
                IsSpeaking = false;
                _xnaUpdateTimer.Stop();
                _player.Stop();
                _ignoreNextBufferNeeded = true;
            }
        }

        /// <summary>
        /// Converts the byte to a 16 bit signed
        /// </summary>
        /// <param name="b"></param>
        /// <returns>16 bit signed integer</returns>
        private static short ConvertTo16BitSigned(byte b)
        {
            float val = b - 127F;
            if (val == 0)
                return 0;
            else if (val == 128)
                return short.MaxValue;
            else if (val == -127)
                return short.MinValue;
            else if (val > 0)
            {
                val = ((val / 128F));
                val = short.MaxValue * val;
            }
            else
            {
                val = ((val / -128F));
                val = short.MinValue * val;
            }

            short newVal = (short)val;
            return newVal;
        }

        /// <summary>
        /// Low range of 8 bit audio is 0, high 16
        /// Low range of 16 bit audio is int>MinValue, high int.MaxValue
        /// </summary>
        /// <param name="eightBitValue"></param>
        /// <returns>16 bit signed array</returns>
        private static short[] ConvertTo16BitSignedArray(byte[] bytes, int offset, int length)
        {

            short[] outArray = new short[bytes.Length];
            for (int i = offset; i < length; i++)
            {
                outArray[i] = ConvertTo16BitSigned(bytes[i]);
            }
            return outArray;
        }

        /// <summary>
        /// Convert 8 bit byte[] to 16 bit byte[]
        /// output will have twice the data length
        /// </summary>
        /// <param name="eightBitData">8 bit input</param>
        /// <param name="offset">Start index for input stream</param>
        /// <param name="length">length from start index to process</param>
        /// <returns>16bit audio stream</returns>
        private static byte[] ConvertTo16BitAudio(byte[] eightBitData, int offset, int length)
        {
            short[] sixteenBitData = ConvertTo16BitSignedArray(eightBitData, offset, length);
            return ConvertTo16BitAudio(sixteenBitData, offset, length);
        }

        /// <summary>
        /// Convert an array of 16bit audio in the form of short[]
        /// to 16 bit byte[]. There will be twice the length of input stream.
        /// </summary>
        /// <param name="sixteenBitData">Data with int16.MinValue as lowest, int16.MinValue highest</param>
        /// <param name="offset">Start index for input stream</param>
        /// <param name="length">length from start index to process</param>
        /// <returns>16bit audio stream</returns>
        private static byte[] ConvertTo16BitAudio(short[] sixteenBitData, int offset, int length)
        {
            List<byte> outputList = new List<byte>();

            for (int i = offset; i < length; i++)
            {
                outputList.AddRange(BitConverter.GetBytes(sixteenBitData[i]));
            }

            return outputList.ToArray();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public virtual void Dispose()
        {
            if (_player != null)
            {
                IsSpeaking = false;
                _player.BufferNeeded -= OnPlayerBufferNeeded;
                _player.Stop();
                _player.Dispose();
                _player = null;
                _xnaUpdateTimer.Stop();
                _xnaUpdateTimer = null;
                FrameworkDispatcher.Update();
                _ignoreNextBufferNeeded = true;
            }
        }
    }
}
