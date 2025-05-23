﻿using MahdiGhiasi.AppListManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace W10MAppDataManageTool
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private static ObservableCollection<AppDataExtension> appsSizeData = new ObservableCollection<AppDataExtension>();

        //public static string BackupDestination = @"C:\Users\Admin\Downloads\Backups";    //Win10/11
        public static string BackupDestination = @"C:\Data\Users\Public\Downloads\Backups"; // W10M

        public static bool AllowCompress = false;
        public static int secretCodeCounter = 0;
        public static bool hiddenMode = false;
        public static bool updateCacheInProgress = false;
        
        
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            
            //Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
            //    Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
            //    Microsoft.ApplicationInsights.WindowsCollectors.Session);
            
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
        }

        private void OnResuming(object sender, object e)
        {
            Frame rootFrame = (Frame)Window.Current.Content;

            /**
            if (rootFrame.CurrentSourcePageType == typeof(Backups))
            {
                Backups thePage = (Backups)rootFrame.Content;
                thePage.RefreshCurrentBackupDataIfNecessary();
            }
            /**/
            AppListCacheUpdater.LoadAppsInBackground();

            FileOperations.ClearGetContentsCache();
            AppDataExtension.ResetAppSizes();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;


                // Register a handler for BackRequested events and set the
                // visibility of the Back button
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ?
                    AppViewBackButtonVisibility.Visible :
                    AppViewBackButtonVisibility.Collapsed;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        public event EventHandler<BackRequestedEventArgs> BackRequested;

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            // Raise child event
            var eventHandler = this.BackRequested;

            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }

            if (!e.Handled)
            {
                var _rootFrame = Window.Current.Content as Frame;

                if (_rootFrame != null && _rootFrame.CanGoBack)
                {
                    e.Handled = true;
                    _rootFrame.GoBack();

                }
            }
        }

        public static AppDataExtension GetAppDataEx(AppData app)
        {
            AppDataExtension ade = appsSizeData.FirstOrDefault(x => x.familyName == app.FamilyName);
            if (ade == null)
            {
                ade = new AppDataExtension()
                {
                    familyName = app.FamilyName,
                    TheApp = app
                };
                appsSizeData.Add(ade);
            }
            return ade;
        }
    }
}
