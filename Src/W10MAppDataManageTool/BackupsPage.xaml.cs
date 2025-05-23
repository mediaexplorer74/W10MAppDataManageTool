﻿using MahdiGhiasi.AppListManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace W10MAppDataManageTool
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Backups : Page
    {
        Backup currentBackup = null;
        bool isJustForDetails = false;

        public Backups()
        {
            this.InitializeComponent();

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


            BackupDetails.Visibility = Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            backupsList.ItemsSource = BackupManager.currentBackups;
            noBackups.Visibility = BackupManager.currentBackups.Count == 0 ? Visibility.Visible : Visibility.Collapsed;         

            ((App)App.Current).BackRequested += Backups_BackRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.Parameter != null) && (e.Parameter.GetType() == typeof(Backup)))
            {
                isJustForDetails = true;

                ShowBackup((Backup)e.Parameter);
            }

            base.OnNavigatedTo(e);
        }

        private async void ShowBackup(Backup backup)
        {
            currentBackup = backup;

            BackupDetails.DataContext = backup;
            BackupDetails.Visibility = Visibility.Visible;

            LoadBackupSize((Backup)backup);

            bool isThereAtLeastOneNotInstalled;
            bool isThereAtLeastOneInstalled;
            LoadBackupAppsList(backup, out isThereAtLeastOneNotInstalled, out isThereAtLeastOneInstalled);

            if ((isThereAtLeastOneNotInstalled) && (App.updateCacheInProgress))
            {
                WaitingForCacheMessage.Visibility = Visibility.Visible;
                RestoreAppBarButton.IsEnabled = false;
                appsList.ItemsSource = null;

                while (App.updateCacheInProgress)
                    await Task.Delay(500);

                LoadBackupAppsList(backup, out isThereAtLeastOneNotInstalled, out isThereAtLeastOneInstalled);
                WaitingForCacheMessage.Visibility = Visibility.Collapsed;
            }

            NotInstalledNotice.Visibility = isThereAtLeastOneNotInstalled ? Visibility.Visible : Visibility.Collapsed;

            RestoreAppBarButton.IsEnabled = isThereAtLeastOneInstalled;
        }

        private void LoadBackupAppsList(Backup backup, out bool isThereAtLeastOneNotInstalled, out bool isThereAtLeastOneInstalled)
        {
            isThereAtLeastOneNotInstalled = false;
            isThereAtLeastOneInstalled = false;
            List<BackupListOfApps> listOfApps = new List<BackupListOfApps>();
            foreach (var item in backup.Apps)
            {
                BackupListOfApps b = new BackupListOfApps();
                b.AppName = item.DisplayName;
                b.cAppData = item;

                AppData appd = AppDataExtension.GetAppDataFromCompactAppData(item);

                if (appd == null)
                {
                    b.IsInstalled = false;
                    isThereAtLeastOneNotInstalled = true;
                }
                else
                {
                    b.Publisher = appd.Publisher;
                    b.IsInstalled = true;
                    isThereAtLeastOneInstalled = true;
                }

                listOfApps.Add(b);
            }

            appsList.ItemsSource = listOfApps;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ((App)App.Current).BackRequested -= Backups_BackRequested;

            base.OnNavigatingFrom(e);
        }

        private void Backups_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            if ((BackupDetails.Visibility == Visibility.Visible) && (!isJustForDetails))
            {
                BackupDetails.Visibility = Visibility.Collapsed;
                backupsList.SelectedItem = null;
                BackupSizeText.Text = "Checking...";
                e.Handled = true;
            }
        }

        private void backupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (backupsList.SelectedItem == null)
                return;

            ShowBackup((Backup)backupsList.SelectedItem);
        }

        private async void LoadBackupSize(Backup item)
        {
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.Combine(App.BackupDestination, item.Name));
                StorageFile dataFile = (StorageFile)await folder.GetItemAsync("data.zip");
                BackupSizeText.Text = FileOperations.GetFileSizeString((await dataFile.GetBasicPropertiesAsync()).Size);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] Exception: " + ex.Message);

                MessageDialog md = new MessageDialog("Item "+ item.Name + " has a wrong name \n (get right name from metadata.json \n and apply to folder name!!!) ");
                md.Commands.Add(new UICommand("OK") { Id = 1 });
                //md.Commands.Add(new UICommand("No") { Id = 0 });
                md.DefaultCommandIndex = 1;
                //md.CancelCommandIndex = 0;

                var result = await md.ShowAsync();
            }
        }

        private void RestoreButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(BackupProgress), Newtonsoft.Json.JsonConvert.SerializeObject(new BackupProgressMessage() { backup = currentBackup, IsRestore = true }));
        }

        private async void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MessageDialog md = new MessageDialog("Are you sure you want to permanently delete this backup?");
            md.Commands.Add(new UICommand("Yes") { Id = 1 });
            md.Commands.Add(new UICommand("No") { Id = 0 });
            md.DefaultCommandIndex = 1;
            md.CancelCommandIndex = 0;

            var result = await md.ShowAsync();

            if (((int)result.Id) == 1)
            {
                RestoreAppBarButton.IsEnabled = false;
                DeleteAppBarButton.IsEnabled = false;

                await BackupManager.DeleteBackup(currentBackup);

                backupsList.ItemsSource = null;
                await Task.Delay(100);
                BackupDetails.Visibility = Visibility.Collapsed;
                backupsList.ItemsSource = BackupManager.currentBackups;

                RestoreAppBarButton.IsEnabled = true;
                DeleteAppBarButton.IsEnabled = true;

                if (isJustForDetails)
                    Frame.GoBack();
            }
        }

        private async void appsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (appsList.SelectedItem == null)
                return;

            BackupListOfApps selectedApp = (BackupListOfApps)appsList.SelectedItem;
            appsList.SelectedItem = null;

            if (!selectedApp.IsInstalled)
            {
                CompactAppData appd = selectedApp.cAppData;

                Uri storeUri;
                if (appd.FamilyName[0] == ('{'))
                    storeUri = new Uri("ms-windows-store://pdp/?PhoneAppId=" + appd.FamilyName.Substring(1, appd.FamilyName.Length - 2).ToLower());
                else
                    storeUri = new Uri("ms-windows-store://pdp/?PFN=" + appd.FamilyName);

                await Windows.System.Launcher.LaunchUriAsync(storeUri);
            }
        }
    }

    internal class BackupListOfApps
    {
        public string AppName { get; set; }
        public string Publisher { get; set; } = "";
        public bool IsInstalled { get; set; }
        public CompactAppData cAppData { get; set; }
    }
}
