﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using AzureConferenceLib.Models;
using AzureConferenceWP.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace AzureConferenceWP
{
    public partial class App : Application
    {
        private static MainViewModel viewModel = null;
        public static Speaker CurrentSpeaker;
        public static Session CurrentSession;
        public static ObservableCollection<Session> Sessions;
        public static ObservableCollection<Speaker> Speakers;
        public static ObservableCollection<Session> SavedSessions;
        public static List<int> SavedSessionIds; 
        public static string sessionData = string.Empty;
        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            ThemeManager.SetAccentColor(AccentColor.Blue);

            
            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            IEnumerable<Session> savedSessionsListFromIsoStorage = null;
            //IsolatedStorageSettings.ApplicationSettings.TryGetValue<IEnumerable<Session>>("SavedSessions", out savedSessionsListFromIsoStorage);
            //if (savedSessionsListFromIsoStorage != null)
            //    SavedSessions = savedSessionsListFromIsoStorage.OrderBy(p => p.Begins).ToObservableCollection();
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("SavedSessionIds", out SavedSessionIds);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("SessionData", out sessionData);
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
                //IsolatedStorageSettings.ApplicationSettings.TryGetValue<ObservableCollection<Session>>("SavedSessions", out SavedSessions);
                IsolatedStorageSettings.ApplicationSettings.TryGetValue("SavedSessionIds", out SavedSessionIds);
                IsolatedStorageSettings.ApplicationSettings.TryGetValue("SessionData", out sessionData);
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
            SaveAppSettings();
        }

        private static void SaveAppSettings()
        {
            try
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("SavedSessionIds"))
                    IsolatedStorageSettings.ApplicationSettings.Remove("SavedSessionIds");
                IsolatedStorageSettings.ApplicationSettings.Add("SavedSessionIds", SavedSessionIds);
                if (IsolatedStorageSettings.ApplicationSettings.Contains("SessionData"))
                    IsolatedStorageSettings.ApplicationSettings.Remove("SessionData");
                IsolatedStorageSettings.ApplicationSettings.Add("SessionData", sessionData);
                //IsolatedStorageSettings.ApplicationSettings["SavedSessions"] = SavedSessions.ToList();
            IsolatedStorageSettings.ApplicationSettings.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveAppSettings();
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

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
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

        #endregion
    }
}