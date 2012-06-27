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
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Code-behind for the Hsitory Page
    /// </summary>
    public partial class HistoryPage : PhoneApplicationPage
    {
        /// <summary>
        /// How to get to this page
        /// </summary>
        public const string NavigateString = "/HistoryPage/HistoryPage.xaml";
        /// <summary>
        /// Set to true when the SelectionChanged event should NOT cause this page to return
        /// to the MainPage
        /// </summary>
        private bool _ignoreSelectionChanged = false;

        /// <summary>
        /// Set to true when the OnNavigatedTo method is called. If this is true when the LayoutUpdated
        /// event handler is called, that implies that the page is visible for the first time.
        /// </summary>
        private bool _onNavigatedToCalled = false;

        /// <summary>
        /// Constructor. Looks for LayoutUpdated events, and will keep doing so until that _onNaviagtedTo is
        /// set to true
        /// </summary>
        public HistoryPage()
        {
            InitializeComponent();
            LayoutUpdated += HistoryPage_LayoutUpdated;
        }

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
        /// which means the page is visible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HistoryPage_LayoutUpdated(object sender, EventArgs e)
        {
            if (_onNavigatedToCalled == true)
            {
                LayoutUpdated -= HistoryPage_LayoutUpdated;
                // Once the page is visible, this code will be run, which will fill the list.
                // That ensures the History Page comes up as fast as possible for the user.
                // Running this in a BeginInvoke() block will put this call at the end of any
                // remaining internal work Silverlight must do.
                Dispatcher.BeginInvoke(() =>
                {
                    IndicateVisiblePhrases();
                }
                );
            }
        }

        /// <summary>
        /// Since LINQ is not an ObservableCollection, we need to fake it. Make an ObservableCollection
        /// that will get re-filtered whenever the HistoryList changes. use LINQ for the filtering, and
        /// pass this return value to the data binding for the listbox.
        /// </summary>
        /// <returns>An ObservableCollection that is filtered by Linq automatically when the source collection changes.</returns>
        private void IndicateVisiblePhrases()
        {
            _phrasesListbox.ItemsSource = 
                        from Phrase phrase in App.Model.HistoryList.Phrases
                        where phrase.SupportsLangauge(App.Model.FromLanguage.TwoLetterISOLanguageName)
                        select phrase;
        }

        /// <summary>
        /// Event handler for when the user selects an item in the listbox. If they are
        /// tapping on an item to select it, then the HistoryList will need to update to
        /// move the choice to the top, and this page will need to close and navigate back
        /// to the Main page.
        /// 
        /// However, if the user is enableing the context menu, this method will also get
        /// called when the Context Menu highlights the selection. Also, when the HistoryList
        /// changes from the ChooseFromHistory re-ordering the list, this method will be called.
        /// 
        /// Therefore, the _ignoreSelectionChanged is set to true here to avoid this method getting
        /// run multiple times, and it is also set to true when the Context Menu displays
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPhrasesListboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ignoreSelectionChanged == false)
            {
                _ignoreSelectionChanged = true;
                Phrase phrase = _phrasesListbox.SelectedItem as Phrase;
                // Ensure the navigate will update the field
                MainPage.OnPageNavigatedToBehavior = MainPage.OnPageNavigatedToBehaviors.PopulateWithHistory;
                // The BeginInvoke() prevents some "flicker" where the listbox may update as the page navigates away.
                Dispatcher.BeginInvoke(() =>
                {
                    App.Model.ChooseFromHistory(phrase);
                }
                );
                this.NavigationService.GoBack();
            }
        }

        /// <summary>
        /// Prevents the page from acting like the user tapped on an item when in reality they
        /// are bringing up the Context Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContextMenuDisplaying(object sender, System.EventArgs e)
        {
            _ignoreSelectionChanged = true;
        }

        /// <summary>
        /// Event Handler for the "Delete" Context Menu item being tapped. Does as advertised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleteContextMenuTapped(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Model.HistoryList.Phrases.RemoveAt(HistoryListContextMenu.LastSelectedIndex);
            IndicateVisiblePhrases();
        }

        /// <summary>
        /// Once the Context Menu is out of play, start listening again for the user tapping
        /// on an item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContextMenuClosing(object sender, EventArgs e)
        {
            _phrasesListbox.SelectedItem = null;
            _ignoreSelectionChanged = false;
        }

        /// <summary>
        /// When the pack key is pressed, close the context menu if it is open
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            HistoryListContextMenu.OnBackKeyPress(e);
        }
    }
}
