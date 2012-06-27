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
using System.Windows;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Logging;
using System.Windows.Navigation;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Entry point for Application
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The Data Model
        /// </summary>
        public static bool Deactivated { get; private set; }

        /// <summary>
        /// The Data Model
        /// </summary>
        private static Model _model;

        /// <summary>
        /// Translator class for text to text
        /// </summary>
        private static Translator _translator;

        /// <summary>
        /// Speech class for Text to Speech
        /// </summary>
        private static Speech _speech;

        // Easy access to the root frame
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Translator class for text to text
        /// </summary>
        public static Translator Translator 
        { 
            get 
            {
                if (_translator == null)
                {
                    _translator = new Translator();
                }
                return _translator; 
            } 
        }

        /// <summary>
        /// Speech class for Text to Speech
        /// </summary>
        public static Speech Speech 
        { 
            get 
            {
                if (_speech == null)
                {
                    _speech = new Speech();
                }
                return _speech; 
            } 
        }

        /// <summary>
        /// The Data Model
        /// </summary>
        public static Model Model
        {
            get
            {
                if (_model == null)
                {
                    _model = new Model();
                }
                return _model;
            }
        }

        /// <summary>
        /// Entry point constructor
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        /// <summary>
        /// Method called when the app launches (fresh start)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            Translator_LaunchOrActivate(true);
        }

        /// <summary>
        /// Method called when the app rehydrates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            MainPage.IsRehydrating = true;
            Translator_LaunchOrActivate(false);
        }

        /// <summary>
        /// Method called when the app tombstones
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            Translator_CloseOrDeactivate(false);
        }

        /// <summary>
        /// Method called when the app is closed down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            Translator_CloseOrDeactivate(true);
        }

        /// <summary>
        /// Called when the app starts to run
        /// </summary>
        /// <param name="isLaunch">True for a launch, false for a rehydrate</param>
        private void Translator_LaunchOrActivate(bool isLaunch)
        {
            Microsoft.Phone.Applications.Common.Controls.TiltEffect.SetIsTiltEnabled(RootFrame, true);
        }

        /// <summary>
        /// Called when the app is closing
        /// </summary>
        /// <param name="isClose"></param>
        private void Translator_CloseOrDeactivate(bool isClose)
        {
            Deactivated = true;
            // No diff in this app for close or tombstone
            Model.Save();
            Speech.Dispose();
        }

        /// <summary>
        /// Unhandled Exception handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }


        #region Phone application initialization

        // Avoid double-initialization-
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #endregion
    }
}
