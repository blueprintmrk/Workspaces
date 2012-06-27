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
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Code-behind the for Languages page
    /// </summary>
    public partial class LanguagePickerPage : PhoneApplicationPage
    {
        /// <summary>
        /// How to get to this page
        /// </summary>
        public const string NavigateString = "/LanguagePickerPage/LanguagePickerPage.xaml";

        /// <summary>
        /// Generates a URI to get to this page with parameters. This code keeps all the
        /// knowledge of the paramters inside the page class
        /// </summary>
        /// <param name="from">True if the language picker page is for the "From" langauge, false for the "To" language</param>
        /// <returns>The URI that specifies the appropriate parameter</returns>
        public static string GetNavigateString(bool from)
        {
            return
                String.Format(
                    NavigateString + "?{0}={1}",
                    LangaugeQuery,
                    (from ? ParameterStringFrom : ParameterStringTo)
                    );
        }

        /// <summary>
        /// Parameter for "To"
        /// </summary>
        private const string LangaugeQuery = "FromOrTo";
        /// <summary>
        /// Parameter for "From"
        /// </summary>
        private const string ParameterStringFrom = "From";
        /// <summary>
        /// Parameter for "To"
        /// </summary>
        private const string ParameterStringTo = "To";

        /// <summary>
        /// True if "From", false if "To"
        /// </summary>
        private bool _modifyingFromLangauge;

        /// <summary>
        /// Constructor
        /// </summary>
        public LanguagePickerPage()
        {
            InitializeComponent();
            // Since this list of languages is generated in memory and not in Isolated Storage or
            // the result of a web call, it's OK to data bind here.
            _languagesListBox.ItemsSource = App.Model.AvailableLangauges;
            // When this page returns to the Main Page, the Main Page needs to react accordingly.
            // Default behavior, i.e. user just presses the back button, is to do nothing.
            MainPage.OnPageNavigatedToBehavior = MainPage.OnPageNavigatedToBehaviors.DoNothing;
        }

        /// <summary>
        /// Event handler for when the user selects a langauge from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_modifyingFromLangauge)
            {
                // Update the model, and clear the fields, since changing the "from" language
                // means the translated "to" text is irrelevant.
                App.Model.FromLanguage = (LanguageInformation)(_languagesListBox.SelectedItem);
                MainPage.OnPageNavigatedToBehavior = MainPage.OnPageNavigatedToBehaviors.ClearFields;
            }
            else
            {
                // Upadte the model and let the MainPage know the To langauge changed. This will
                // cause the Main Page to re-run a translation based on the new language.
                App.Model.ToLanguage = (LanguageInformation)(_languagesListBox.SelectedItem);
                MainPage.OnPageNavigatedToBehavior = MainPage.OnPageNavigatedToBehaviors.ToLanguageChanged;
            }

            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        /// <summary>
        /// The only state this picker page needs to know is if the "from" or "to" language is being
        /// changed. That information is preserved in the NavigationContext, so even if your app
        /// tombstones and them rehydrates, when you come into this page, the From or To will be
        /// set correctly.
        /// </summary>
        /// <param name="e"></param>
        protected override void  OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _modifyingFromLangauge = false;

            if (this.NavigationContext.QueryString.ContainsKey(LangaugeQuery))
            {
                string parameter = this.NavigationContext.QueryString[LangaugeQuery];
                if (parameter == ParameterStringFrom)
                {
                    _modifyingFromLangauge = true;
                }
                else
                {
                    _modifyingFromLangauge = false;
                }
            }

            if (_modifyingFromLangauge)
            {
                ListName.Text = LocalizationStrings.Strings.LanguageFrom;
            }
            else
            {
                ListName.Text = LocalizationStrings.Strings.LanguageTo;
            }
        }

    }


}
