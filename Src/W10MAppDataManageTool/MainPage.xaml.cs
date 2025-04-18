﻿using MahdiGhiasi.AppListManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace W10MAppDataManageTool
{
    public sealed partial class MainPage : Page
    {
        Dictionary<string, AppData> AppNames = new Dictionary<string, AppData>();
        BackupManager.BackupLoader backupLoader = new BackupManager.BackupLoader();
        static LoadAppData lad = new LoadAppData();

        bool loadAppsList = true;

        public MainPage()
        {
            this.InitializeComponent();

            //backupLoader.LoadBackupsProgress += BackupLoader_LoadBackupsProgress;

            Windows.UI.Core.SystemNavigationManager
               .GetForCurrentView().AppViewBackButtonVisibility
               = AppViewBackButtonVisibility.Visible;

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
            };



            InitSettings();

            versionNum.Text = "v2.5.0";// + UpdateChecker.GetAppVersionString(false);
        }

        private async void InitSettings()
        {
            ApplicationDataContainer localSettings = 
                Windows.Storage.ApplicationData.Current.LocalSettings;

            if ((localSettings.Values["allowCompress"] != null) 
                && (localSettings.Values["allowCompress"].GetType() == typeof(bool)))
            {
                App.AllowCompress = (bool)localSettings.Values["allowCompress"];
            }
            else
            {
                localSettings.Values["allowCompress"] = App.AllowCompress;
            }

            if ((localSettings.Values["loadAppsEveryTime"] != null) 
                && (localSettings.Values["loadAppsEveryTime"].GetType() == typeof(bool)))
            {
                loadAppsList = (bool)localSettings.Values["loadAppsEveryTime"];
            }
            else
            {
                localSettings.Values["loadAppsEveryTime"] = true;
            }

            if ((localSettings.Values["hiddenMode"] != null) 
                && (localSettings.Values["hiddenMode"].GetType() == typeof(bool)))
            {
                App.hiddenMode = (bool)localSettings.Values["hiddenMode"];
            }
            else
            {
                localSettings.Values["hiddenMode"] = false;
                App.hiddenMode = false;
            }

            if ((localSettings.Values["backupDest"] != null) 
                && (localSettings.Values["backupDest"].GetType() == typeof(string)))
            {
                App.BackupDestination = (string)localSettings.Values["backupDest"];
            }
            else
            {
                localSettings.Values["backupDest"] = App.BackupDestination;
            }

            try
            {
                await FileOperations.CreateDirectoryIfNotExists(App.BackupDestination);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] FileOperations.CreateDirector Exception: " + ex.Message);
            }
        }//InitSettings


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //backupLoader.LoadBackupsProgress -= BackupLoader_LoadBackupsProgress;

            base.OnNavigatingFrom(e);
        }

        /**
        private void BackupLoader_LoadBackupsProgress(object sender, LoadingEventArgs e)
        {
                        
            if (e.Current == 0)
            {
                progressStatus.Text = "Loading current backups...";
            }
            else
            {
                int percent = (int)Math.Round((100.0 * e.Current) / e.Total);
                progressStatus.Text = "Loading current backups " + percent.ToString() + "%";
            }
            
        }
        /**/


        //string sVPN;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                StorageFile ff = await 
                    StorageFile.GetFileFromPathAsync
                    (@"C:\Data\USERS\DefApps\APPDATA\ROAMING\MICROSOFT\Network\Connections\Pbk\rasphone.pbk");

                sVPN = await FileIO.ReadTextAsync(ff);
            }
            catch
            {
                //
            }
            */

            //await new MessageDialog(sVPN).ShowAsync();

            //sVPN = sVPN.Replace("IpPrioritizeRemote=0", "IpPrioritizeRemote=1");

            if (LoadAppData.appsData.Count == 0)
            {
                //UpdateChecker.CheckForUpdates();

                progress.Visibility = Visibility.Visible;
                progressRing.IsActive = true;

                progressStatus.Text = "Loading cache...";
                bool isThereCache = await LoadAppData.LoadCachedAppList();
                
                bool appsBg = true;
                if (!isThereCache)
                {
                    lad.LoadingProgress += LoadAppData_LoadingProgress_2;

                    await lad.LoadApps();
                    
                    lad.LoadingProgress -= LoadAppData_LoadingProgress_2;

                    appsBg = false;
                }

                progressStatus.Text = "Loading current backups...";
                await backupLoader.LoadCurrentBackups();

                if (appsBg)
                {
                    AppListCacheUpdater.LoadAppsInBackground(lad);
                }

                progress.Visibility = Visibility.Collapsed;
                progressRing.IsActive = false;

                Frame.Background = Header.Background;
            }

            AppDataView.PageStatus_CurrentApp = null;
            AppDataView.PageStatus_IsShowingDetails = false;
        }


        private void LoadAppData_LoadingProgress_2(object sender, LoadingEventArgs e)
        {
            int percent = (int)Math.Round((100.0 * e.Current) / e.Total);
            progressStatus.Text = "Loading apps " + percent.ToString() + "%";
        }


        private void appDataViewButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AppDataView));
        }

        private void appDataBackupsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Backups));
        }

        private void appDataSettingsButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings));
        }

        /*
        private async void appDataAboutButton_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            StorageFile ff = await StorageFile.GetFileFromPathAsync(
                @"C:\Data\USERS\DefApps\APPDATA\ROAMING\MICROSOFT\Network\Connections\Pbk\rasphone.pbk");
            //await FileIO.WriteTextAsync(ff, sVPN);

            //Frame.Navigate(typeof(About));
            Frame.Navigate(typeof(About));
        }*/

        private void Secret3_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if ((App.secretCodeCounter == 12) || (App.secretCodeCounter == 120))
            {
                App.secretCodeCounter *= 10;
            }
            else
            {
                App.secretCodeCounter = 0;
            }

            System.Diagnostics.Debug.WriteLine("SECRET3");
        }

        private async void appDataAboutButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /* 
            try
            {
                StorageFile ff = await StorageFile.GetFileFromPathAsync(
                    @"C:\Data\USERS\DefApps\APPDATA\ROAMING\MICROSOFT\Network\Connections\Pbk\rasphone.pbk");
                //await FileIO.WriteTextAsync(ff, sVPN);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] Exception : " + ex.Message);
            }*/

            Frame.Navigate(typeof(About));
        }
    }
}
