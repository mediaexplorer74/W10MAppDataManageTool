using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
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


            VersionNameText.Text = "v2.5.0";//UpdateChecker.GetAppVersionString(false);
        }

        private void Secret1_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            App.secretCodeCounter++;
            if (App.secretCodeCounter > 3)
                App.secretCodeCounter = 0;
            System.Diagnostics.Debug.WriteLine("SECRET1");
        }

        private void Secret2_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if ((App.secretCodeCounter == 3) || (App.secretCodeCounter == 6))
                App.secretCodeCounter *= 2;
            else
                App.secretCodeCounter = 0;

            System.Diagnostics.Debug.WriteLine("SECRET2");
        }
    }
}
