
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace LycaIle
{

    public sealed partial class Settings : Page
    {


        public Settings()
        {
            this.InitializeComponent();
        }


        private void uiSave_Click(object sender, RoutedEventArgs e)
        {
            App.SetSettingsInt("limitMinut", int.Parse(uiMins.Text));
            App.SetSettingsInt("limitSMS", int.Parse(uiSMS.Text));

            App.SetSettingsBool("AutoDel", uiDelPic.IsOn);

            App.SetSettingsBool("bShowNumMins", uiRadioMin.IsChecked);
            App.SetSettingsBool("bShowNumSMS", uiRadioSMS.IsChecked);
            App.SetSettingsBool("bShowBothNum", uiRadioText.IsChecked);
            //App.SetSettingsBool("bShowNumMins", uiShowNumMins.IsOn);
            //App.SetSettingsBool("bShowNumSMS", uiShowNumSMS.IsOn);

            this.Frame.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            uiVersion.Text = "wersja " + Windows.ApplicationModel.Package.Current.Id.Version.Major + "." +
                Windows.ApplicationModel.Package.Current.Id.Version.Minor + "." +
                Windows.ApplicationModel.Package.Current.Id.Version.Build;

            uiMins.Text = App.GetSettingsInt("limitMinut", 100).ToString();
            uiSMS.Text = App.GetSettingsInt("limitSMS", 100).ToString();

            uiDelPic.IsOn = App.GetSettingsBool("AutoDel", true);
            //uiShowNumMins.IsOn = App.GetSettingsBool("bShowNumMins");
            //uiShowNumSMS.IsOn = App.GetSettingsBool("bShowNumSMS");
            uiRadioMin.IsChecked = App.GetSettingsBool("bShowNumMins");
            uiRadioSMS.IsChecked = App.GetSettingsBool("bShowNumSMS");
            uiRadioText.IsChecked = App.GetSettingsBool("bShowBothNum");
            if (!(App.GetSettingsBool("bShowNumMins") || App.GetSettingsBool("bShowNumSMS") || App.GetSettingsBool("bShowBothNum")))
                uiRadioNone.IsChecked = true;

        }

        //private void uiNumMins_Toggled(object sender, RoutedEventArgs e)
        //{
        //    if (uiShowNumMins.IsOn)
        //        uiShowNumSMS.IsOn = false;
        //}

        //private void uiNumSMS_Toggled(object sender, RoutedEventArgs e)
        //{
        //    if (uiShowNumSMS.IsOn)
        //        uiShowNumMins.IsOn = false;
        //}

        private void uiRateIt_Click(object sender, RoutedEventArgs e)
        {
            Uri sUri = new Uri("ms-windows-store://review/?PFN=" + Windows.ApplicationModel.Package.Current.Id.FamilyName);
            Windows.System.Launcher.LaunchUriAsync(sUri);
        }
    }
}