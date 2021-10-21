
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace VirginMobIle
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
            App.SetSettingsBool("bShowBezwzgledna", uiMinAvg.IsOn);
            //App.SetSettingsBool("bShowNumMins", uiShowNumMins.IsOn);
            //App.SetSettingsBool("bShowNumSMS", uiShowNumSMS.IsOn);

            this.Frame.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        { // pobieranie numeru wersji tylko w Windows
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
            uiMinAvg.IsOn = App.GetSettingsBool("bShowBezwzgledna");

#if NETFX_CORE
#else
            uiBarSeparat.Visibility = Visibility.Collapsed;
            uiBarOcen.Visibility = Visibility.Collapsed;
#endif
            //uiShowNumMins.Visibility = Visibility.Collapsed
            //uiShowNumMins.Visibility = Visibility.Visible

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
        {// dziala tylko na Windows!
            Uri sUri = new Uri("ms-windows-store://review/?PFN=" + Windows.ApplicationModel.Package.Current.Id.FamilyName);
            Windows.System.Launcher.LaunchUriAsync(sUri);
        }

        private void uiPrivacy_Click(object sender, RoutedEventArgs e)
        {
            App.DialogBox("Polityka prywatnoœci aplikacji\n\n" +
                "Poniewa¿ aplikacja w ogóle nie korzysta z sieci, to nigdzie nie wysy³a ¿adnych informacji.\n\n" +
                "Dostêp do zdjêæ jest potrzebny po to, by móc rozpoznaæ tekst - komunikat od Virgin Mobile.\n\n" +
                "Aby uproœciæ korzystanie z jej funkcjonalnoœci, mo¿na wyraziæ zgodê na nawi¹zanie po³¹czenia telefonicznego - wtedy wybierze numer \"*222#\".Mo¿na te¿ nie nadawaæ jej tego uprawnienia, i robiæ to rêcznie.");
        }
    }
}