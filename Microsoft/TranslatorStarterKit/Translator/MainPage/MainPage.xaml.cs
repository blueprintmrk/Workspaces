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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Logging;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Code-Behind for the Main Page
    /// </summary>
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        #region Enums and Constants

        //
        // Strings for storing the page state, these dientify the Keys. See the SaveState and RestoreFromState methods
        //
        const string TranslatorFromPhraseStateName = "translatorFromPhrase";
        const string TranslatorToPhraseStateName = "translatorToPhrase";
        const string AppSettingsFromLanguageStateName = "AppSettingsFromLanguage";
        const string AppSettingsToLanguageStateName = "AppSettingsToLanguage";

        /// <summary>
        /// Indicates the behavior the MainPage should exhibit when navigated to
        /// </summary>
        public enum OnPageNavigatedToBehaviors
        {
            /// <summary>
            /// Clears the fields.
            /// </summary>
            ClearFields,
            /// <summary>
            /// Takes the top item from the History List, fills in the source text
            /// and requests a translate.
            /// </summary>
            PopulateWithHistory,
            /// <summary>
            /// The destination langauge changed, re-translate
            /// </summary>
            ToLanguageChanged,
            /// <summary>
            /// Leaves the page as-is
            /// </summary>
            DoNothing,
        }

        #endregion

        #region Fields, Properties, Events

        /// <summary>
        /// Static: Indicates the behavior the MainPage should exhibit when navigated to.
        /// </summary>
        private static OnPageNavigatedToBehaviors _sOnPageNavigatedToBehavior = OnPageNavigatedToBehaviors.DoNothing;

        /// <summary>
        /// Static: Set to true when the app is rehydrating, set in App.xaml.cs. Since this is the main page,
        /// which interacts with the model, it needs to know when the app is rehydrating even if this main page
        /// is not the page returned to. For example, if the app rehydrates to the History Page, this page still
        /// needs to know to do its work when naviagted to.
        /// </summary>
        private static bool _sIsRehydrating = false;

        /// <summary>
        /// Current background color
        /// </summary>
        private Color _currentBackgroundColor;

        /// <summary>
        /// Set to true when the OnNavigatedTo method is called. If this is true when the LayoutUpdated
        /// event handler is called, that implies that the page is visible for the first time.
        /// </summary>
        private bool _onNavigatedToCalled = false;

        /// <summary>
        /// Set to true once the page has been initialized (for example, data binding set up)
        /// </summary>
        private bool _pageInitialized = false;

        // Using a DependencyProperty as the backing store for IsSpeaking.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSpeakingProperty =
            DependencyProperty.Register(
                "IsSpeaking", // Name of the property in this class
                typeof(bool),   // Property type
                typeof(MainPage), // Owner of the property (i.e. MainPage.IsSpeaking)
                new PropertyMetadata(false, IsSpeaking_PropertyChangedCallback)  // Defaults to false, and the IsSpeaking_PropertyChangedCallback is called when the prop changes
                );

        /// <summary>
        /// Static: Set to true when the app is rehydrating, set in App.xaml.cs. Since this is the main page,
        /// which interacts with the model, it needs to know when the app is rehydrating even if this main page
        /// is not the page returned to. For example, if the app rehydrates to the History Page, this page still
        /// needs to know to do its work when naviagted to.
        /// </summary>
        public static bool IsRehydrating
        {
            get { return _sIsRehydrating; }
            set { _sIsRehydrating = value; }
        }

        /// <summary>
        /// Set to true while speaking
        /// </summary>
        public bool IsSpeaking
        {
            get { return (bool)GetValue(IsSpeakingProperty); }
            set { SetValue(IsSpeakingProperty, value); }
        }


        /// <summary>
        /// Indicates the behavior the MainPage should exhibit when navigated to.
        /// Set this value before navigating to the MainPage.
        /// </summary>
        public static OnPageNavigatedToBehaviors OnPageNavigatedToBehavior
        {
            get { return _sOnPageNavigatedToBehavior; }
            set { _sOnPageNavigatedToBehavior = value; }
        }

        /// <summary>
        /// Current background color
        /// </summary>
        public Color CurrentBackgroundColor
        {
            get
            {
                return _currentBackgroundColor;
            }
            set
            {
                _currentBackgroundColor = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentBackgroundColor"));
                }
            }
        }
        #endregion

        #region Constrcutor
        /// <summary>
        /// Constructor
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            LayoutUpdated += new EventHandler(MainPage_LayoutUpdated);
            SupportedOrientations = SupportedPageOrientation.Portrait;

            _textBlockFromLanguage.Text = " ";
            _textBlockToLanguage.Text = " ";

            CurrentBackgroundColor = (Color)App.Current.Resources["PhoneBackgroundColor"];
        }

        #endregion

        #region Navigation (this page) and Page Setup

        /// <summary>
        /// Once this is called, the *next* LayoutUpdated event will be called once the page is visible.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _onNavigatedToCalled = true;
        }

        /// <summary>
        /// Event handler for LayoutUpdated. Ignores event until the _onNavigatedToCalled is true,
        /// which means the page is visible for the first time. This is where you want to do your work
        /// of setting up the page if needed (i.e. data bindings), and where you handle dehydration or
        /// other time-intensive tasks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_LayoutUpdated(object sender, EventArgs e)
        {
            if (_onNavigatedToCalled == true)
            {
                _onNavigatedToCalled = false;
                InitPage();
                OnPageMadeVisible();
            }
        }

        /// <summary>
        /// Sets up the page's data bindings
        /// </summary>
        private void InitPage()
        {
            if (_pageInitialized == false)
            {
                _pageInitialized = true;
                // The BeginInvoke causes this code to be run after SilverLight has completed any internal
                // work needed to make the page visible.
                Dispatcher.BeginInvoke(() =>
                    {
                        //
                        // This code is done after the page is visible, trying to get the page to start up as fast as possible
                        //
                        App.Speech.ReadyToSpeak += new EventHandler(OnReadyToSpeak);
                        _txtBoxFrom.DataContext = App.Translator;
                        _txtBoxTo.DataContext = App.Translator;
                        _btnTranslate.DataContext = this;
                        _btnToSpeech.DataContext = App.Speech;

                        // Bind IsSpeaking in speech to ToSpeech.IsChecked 
                        // so that when the Speech class sets its IsSpeaking
                        // property, this class' IsSpeakingProperty also is
                        // affected, and this property's IsSpeaking_PropertyChangedCallback
                        // will be triggered.
                        //
                        // This demonstrates a way to have a property on one page
                        // cause something to happen on another page. It can also be
                        // used to bind things in the xaml (but not in this example).
                        Binding bindIsSpeaking = new Binding();
                        bindIsSpeaking.Path = new PropertyPath("IsSpeaking");
                        bindIsSpeaking.Mode = BindingMode.OneWay;
                        bindIsSpeaking.Source = App.Speech; // From Speech class
                        this.SetBinding(IsSpeakingProperty, bindIsSpeaking); // To this class

                        _btnFromLanguage.DataContext = App.Model.FromLanguage;
                        _btnToLanguage.DataContext = App.Model.ToLanguage;
                    }
                );
            }
        }

        /// <summary>
        /// Called when this mainpage is made visible for the first time. Do the work
        /// "in the future" to allow the UI stack to unwind for fastest start up performance
        /// </summary>
        /// <param name="e"></param>
        private void OnPageMadeVisible()
        {
            // The BeginInvoke causes the code to be run after SilverLight has completed any internal
            // work to make the page visible
            Dispatcher.BeginInvoke(() =>
            {
                bool translateNeeded = false;

                if (IsRehydrating)
                {
                    RestoreFromState(OnPageNavigatedToBehavior);
                    translateNeeded = true;
                }

                // Behave according to the context on which this page was navigated to.
                switch (OnPageNavigatedToBehavior)
                {
                    default:
                    case OnPageNavigatedToBehaviors.DoNothing:

                        break;

                    case OnPageNavigatedToBehaviors.ClearFields:

                        App.Translator.FromPhrase = string.Empty;
                        break;

                    case OnPageNavigatedToBehaviors.ToLanguageChanged:

                        translateNeeded = true;
                        break;

                    case OnPageNavigatedToBehaviors.PopulateWithHistory:

                        App.Translator.FromPhrase = App.Model.HistoryList.MostRecentHistoryPhraseText;
                        translateNeeded = true;
                        break;
                }

                ApplyLanguageButtonText();
                UpdateInputScopeForFromLanguage();

                if (translateNeeded)
                {
                    TranslateText();
                }

                // Reset the behavior
                OnPageNavigatedToBehavior = OnPageNavigatedToBehaviors.DoNothing;

            }
            );

        }

        /// <summary>
        /// On the way out, save the state
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            TextBox focused = FocusManager.GetFocusedElement() as TextBox;
            if (focused != null)
            {
                BindingExpression binding = focused.GetBindingExpression(TextBox.TextProperty);
                if (binding != null)
                {
                    binding.UpdateSource();
                }
            }

            SaveState();
        }

        #endregion

        #region Navigating to Other Pages

        /// <summary>
        /// Navigates to the History Page
        /// </summary>
        private void ShowHistoryPage()
        {
            NavigationService.Navigate(new Uri(HistoryPage.NavigateString, UriKind.Relative));
        }

        /// <summary>
        /// Navigates to the Language picker page
        /// </summary>
        /// <param name="fromLang">True if the "From" language is to be picked, false if the "To" language is to be picked</param>
        private void ShowLanguagePickerPage(bool fromLang)
        {
            NavigationService.Navigate(
                new Uri(LanguagePickerPage.GetNavigateString(fromLang), UriKind.Relative)
                );
        }

        #endregion

        #region Button Event Handlers

        /// <summary>
        /// Translate Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTranslateBtnClick(object sender, RoutedEventArgs e)
        {
            TranslateText();
        }

        /// <summary>
        /// "Say It" button (it's an icon)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnToSpeechBtnClick(object sender, RoutedEventArgs e)
        {
            App.Speech.Speak();
        }

        /// <summary>
        /// From language button (the button is the actual word of the from langauge)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFromLanguageBtnClick(object sender, RoutedEventArgs e)
        {
            ShowLanguagePickerPage(true);
        }

        /// <summary>
        /// To language button (the button is the actual word of the to langauge)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnToLanguageBtnClick(object sender, RoutedEventArgs e)
        {
            ShowLanguagePickerPage(false);
        }

        /// <summary>
        /// History button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHistoryBtnClick(object sender, RoutedEventArgs e)
        {
            ShowHistoryPage();
        }

        /// <summary>
        /// Swap button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSwapBtnClick(object sender, RoutedEventArgs e)
        {
            SwapLangauges();
        }

        #endregion

        #region State

        /// <summary>
        /// Saves the stuff on the page we care about into the built-in State dictionary. This
        /// will be automatically saved by Silverlight when the app tombstones and will automatically
        /// be restored when the app rehydrates.
        /// </summary>
        private void SaveState()
        {
            State[TranslatorFromPhraseStateName] = App.Translator.FromPhrase;
            State[TranslatorToPhraseStateName] = App.Translator.ToPhrase;
            State[AppSettingsFromLanguageStateName] = App.Model.FromLanguage.TwoLetterISOLanguageName;
            State[AppSettingsToLanguageStateName] = App.Model.ToLanguage.TwoLetterISOLanguageName;
        }

        /// <summary>
        /// This method rehydrates the main page by checking against the saved state
        /// </summary>
        /// <param name="behavior"></param>
        private void RestoreFromState(OnPageNavigatedToBehaviors behavior)
        {
            IsRehydrating = false;

            // Check to make sure this is not a super-quick rehydration where the app
            // thought it was being decativated but in reality it is not.
            if (App.Deactivated == false)
            {
                App.Translator.FromPhrase =
                    PhoneUtils.TryGetValue<string>(
                        State,
                        TranslatorFromPhraseStateName,
                        App.Model.HistoryList.MostRecentHistoryPhraseText
                        );
                App.Translator.ToPhrase =
                    PhoneUtils.TryGetValue<string>(
                        State,
                        TranslatorToPhraseStateName,
                        ""
                        );

                // When rehydrating, there is the case where the app rehydrates in the language picker
                // page. In that case, this page will need to "partially" hydrate, where the phrases are
                // restored, but the languages should NOT be restored to the state because the language
                // picker page changed them. This if statement prevents that case from happening.
                if (behavior != OnPageNavigatedToBehaviors.ToLanguageChanged)
                {
                    App.Model.FromLanguage = App.Model.FindLanguage(
                        PhoneUtils.TryGetValue<string>(
                            State,
                            AppSettingsFromLanguageStateName,
                            CultureInfo.CurrentCulture.TwoLetterISOLanguageName
                            ));

                    App.Model.ToLanguage = App.Model.FindLanguage(
                        PhoneUtils.TryGetValue<string>(
                            State,
                            AppSettingsToLanguageStateName,
                            CultureInfo.CurrentCulture.TwoLetterISOLanguageName
                            ));
                }
            }
        }

        #endregion

        #region "From" Textbox Code

        /// <summary>
        /// As the user modifies the text box, check to see if the Translate button should
        /// light up, as well as clear out the translated text when applicable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFromTextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = _txtBoxFrom.Text.Trim().Length != 0;
            _btnTranslate.IsEnabled = isValid;
            if ((_txtBoxFrom.Text != App.Translator.FromPhrase) || (!isValid))
            {
                App.Translator.ToPhrase = string.Empty;
                App.Speech.ResultValid = false;
            }
        }

        /// <summary>
        /// This is how we can get the Enter key to act as "Translate"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFromTextKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
                // Needs to wait for this OnFromTextKeyUp message to finish up
                // before calling TranslateText or else the properties of the Translator
                // could get changed at indeterminate times.
                Dispatcher.BeginInvoke(() =>
                {
                    TranslateText();
                });
            }
        }

        /// <summary>
        /// When the user taps on the source text box, select everything
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFromTextGotFocus(object sender, RoutedEventArgs e)
        {
            // In order to select the text after the user touches the text box,
            // BeginInvoke() must be called to cause the selection to happen "in the future"
            // versus right now. Otherwise, the selection will immediately be removed.
            Dispatcher.BeginInvoke(() =>
            {
                _txtBoxFrom.SelectAll();
            });
        }


        /// <summary>
        /// Call this method to set the input scope correctly for the source text box.
        /// If the currently selected langauge is the phone owner's default language, set the
        /// input scope to text to turn on auto correct. Otherwise, leave the input scope
        /// default, which will not have auto-correct, since typing "Hola" in a Spanish source box
        /// on an English phone would not give useful auto correct suggestions
        /// </summary>
        private void UpdateInputScopeForFromLanguage()
        {
            _txtBoxFrom.InputScope.Names.Clear();
            InputScopeName name = new InputScopeName();
            if (string.Compare(
                                    CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                                    App.Model.FromLanguage.TwoLetterISOLanguageName,
                                    StringComparison.CurrentCultureIgnoreCase
                               ) == 0
               )
            {
                name.NameValue = InputScopeNameValue.Text;
            }
            else
            {
                name.NameValue = InputScopeNameValue.Default;
            }
            _txtBoxFrom.InputScope.Names.Add(name);
        }

        /// <summary>
        /// Handler for the back key. If they are in an "edit" control, this will
        /// dismiss the SIP otherwise it will continue to go back. These two
        /// extra attirbutes are set in the xaml:
        /// 
        ///     BackKeyPress="OnBackKeyPress"
        ///     IsTabStop="true"
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            object element = FocusManager.GetFocusedElement();
            if (element is TextBox || element is PasswordBox)
            {
                this.Focus(); // requires the IsTabStop property to be set for the main page to work
                e.Cancel = true;
            }
        }

        #endregion

        #region Speak Button

        /// <summary>
        /// When the speaking changes, update the visuals
        /// </summary>
        /// <param name="dependancyObject"></param>
        /// <param name="e"></param>
        static void IsSpeaking_PropertyChangedCallback(DependencyObject dependancyObject, DependencyPropertyChangedEventArgs e)
        {
            MainPage mainPage = dependancyObject as MainPage;
            mainPage._btnToSpeech.IsChecked = (bool)e.NewValue;
            mainPage._btnToSpeech.IsEnabled = !(bool)e.NewValue;
        }

        /// <summary>
        /// Event handler called when the speech object finishes loading a wave file.
        /// Lights up the speak button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReadyToSpeak(object sender, EventArgs e)
        {
            // This event comes from a non-UI thread, so we need to marshall the UI code
            Dispatcher.BeginInvoke(() =>
                {
                    _btnToSpeech.IsEnabled = true;
                }
            );
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Reacts to the user swapping the From and To langauges
        /// </summary>
        private void SwapLangauges()
        {
            App.Model.SwapLangauges();
            ApplyLanguageButtonText();
            _txtBoxFrom.Focus();
            UpdateInputScopeForFromLanguage();
        }

        /// <summary>
        /// Makes the call to the Translator class to start a translation
        /// </summary>
        private void TranslateText()
        {
            bool hasSpeech;

            App.Translator.Translate(
                App.Model.FromLanguage.TwoLetterISOLanguageName, 
                App.Model.ToLanguage.TwoLetterISOLanguageName, out hasSpeech
                );

            _btnToSpeech.IsEnabled = hasSpeech;
        }

        /// <summary>
        /// Updates the "buttons" for the To and From languages to reflect the model.
        /// </summary>
        private void ApplyLanguageButtonText()
        {
            _textBlockFromLanguage.Text = App.Model.FromLanguage.ShortNativeName;
            _textBlockToLanguage.Text = App.Model.ToLanguage.ShortNativeName;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }
}