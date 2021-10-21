/* 
 

 2020.01.24
 * Settings: editbox minus/SMS jako dwie kolumny (by było miejsce na nowy ToggleSwitch)
 * Settings: nowy ToggleSwitch: liczba bezwzględna/średnia
 * jeśli pokazuje względem średniej, to jest znak (+-) albo "=0"
 
 Start: 2020.01.22 - kopia LycaIle



*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
//using System.Threading.Tasks;
//using Windows.Foundation;
using Windows.Foundation.Collections;
//using Windows.UI.Xaml;
//using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
//using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



namespace VirginMobIle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async System.Threading.Tasks.Task<Windows.Storage.StorageFolder> ZnajdzFolder()
        {
            Windows.Storage.StorageFolder oFold = null;
            try
            {
                oFold = Windows.Storage.KnownFolders.PicturesLibrary;
                oFold = await oFold.GetFolderAsync("Screenshots");
            }
            catch { }

            return oFold;
        }

        private async System.Threading.Tasks.Task<string> ZnajdzPliczek(Windows.Storage.StorageFolder oFold)
        {

            // 2. iteracja plikow
            DateTime oDateDt = new DateTime(2000, 1, 1);
            string sFile = "";
            DateTimeOffset oDate = oDateDt;

            try
            {
                foreach (Windows.Storage.StorageFile oFile in await oFold.GetFilesAsync())
                {
                    if (oFile.DateCreated > oDate)
                    {
                        oDate = oFile.DateCreated;
                        sFile = oFile.Name;
                    }
                }
            }
            catch { }

            return sFile;
        }

        private void ProgresywnyRing(bool sStart)
        {
            if (sStart)
            {
                double dVal;
                dVal = Math.Min(uiGrid.ActualHeight, uiGrid.ActualWidth) / 2;
                uiProcesuje.Width = dVal;
                uiProcesuje.Height = dVal;

                uiProcesuje.Visibility = Windows.UI.Xaml.Visibility.Visible;
                uiProcesuje.IsActive = true;
            }
            else
            {
                uiProcesuje.IsActive = false;
                uiProcesuje.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }


        private async System.Threading.Tasks.Task<bool> ZrobOCR(Windows.Storage.StorageFile oFile)
        {
            Windows.Storage.Streams.IRandomAccessStream oStream = await oFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            Windows.Graphics.Imaging.BitmapDecoder oDecoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(oStream);
            Windows.Graphics.Imaging.SoftwareBitmap oBmp = await oDecoder.GetSoftwareBitmapAsync();

            Windows.Media.Ocr.OcrEngine oEng = Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages();
            if(oEng is null ) // jesli nie ma takiego, to spróbuj angielskiego (dowolnego) - to raczej jest
                oEng = Windows.Media.Ocr.OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
            if (oEng is null)
            { 
                App.DialogBox("Nie udało się dostać do OCR Engine");
                return false;
            }

            Windows.Media.Ocr.OcrResult rOCR = await oEng.RecognizeAsync(oBmp);

            string sTxt = rOCR.Text;
            // uiOCR.Text = sTxt;

            int iInd;
            if (!sTxt.Contains("Virgin mobile"))
            {
                App.DialogBox("Nie widzę tekstu 'Virgin mobile', to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            //if (sTxt.Contains("nie masz zadnego aktywnego pakietu"))
            //{
            //    App.DialogBox("Ponoć nie masz żadnego aktywnego pakietu");
            //    return false;
            //}

            // "Teraz korzystasz z pakietu <nazwa>. Masz <x> SMS, <x> min i <x> MB. Pakiet ważny do <x>"
            iInd = sTxt.IndexOf(". Masz ");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę tekstu 'Masz', to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            sTxt = sTxt.Substring(iInd + 5);
            // "z <x> SMS, <x> min i <x> MB. Pakiet ważny do <x>"

            iInd = sTxt.IndexOf(" ");
            if (iInd < 0)
            {
                App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            sTxt = sTxt.Substring(iInd + 1);
            // "<x> SMS, <x> min i <x> MB. Pakiet ważny do <x>"

            iInd = sTxt.IndexOf("SMS");
            if (iInd < 0)
            {
                App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            int iSMS = 0;
            int.TryParse(sTxt.Substring(0, iInd).Trim(), out iSMS);
            //try
            //{
            //    iSMS = int.Parse(sTxt.Substring(0, iInd));
            //}
            //catch (Exception ex)
            //{
            //}

            iInd = sTxt.IndexOf(",");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę przecinka przed liczbą minut, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            sTxt = sTxt.Substring(iInd + 2);
            // <x> min i <x> MB. Pakiet ważny do <x>"

            iInd = sTxt.IndexOf(" min");
            if (iInd < 0)
            {
                App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            int iMin = 0;
            int.TryParse(sTxt.Substring(0, iInd).Trim(), out iMin);

            iInd = sTxt.IndexOf(" i ");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę 'i' przed liczbą megabajtów, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            sTxt = sTxt.Substring(iInd + 2);
            // " <x> MB. Pakiet ważny do <dd-mm-yyyy>"
            iInd = sTxt.IndexOf("MB");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę 'MB', to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            string sInet = sTxt.Substring(0, iInd).Trim();
            int iInet = 0;

            bool bError = false;
            try
            {
                iInet = (int)double.Parse(sInet);
            }
            catch 
            {
                bError = true;
            }
            if (bError)
            {
                bError = false;
                sInet = sInet.Replace(".", ",");
                try
                {
                    iInet = (int)double.Parse(sInet);
                }
                catch 
                {
                    //bError = true;
                }
            }

            iInd = sTxt.IndexOf(" do ");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę ' do ' przed datą końca okresu, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }
            sTxt = sTxt.Substring(iInd + 4);
            // "<dd-mm-yyyy>"

            DateTime oDataEnd;
            DateTime oFileTime;

            try
            {
                oDataEnd = new DateTime(int.Parse(sTxt.Substring(6, 4)),
                                        int.Parse(sTxt.Substring(3, 2)),
                                        int.Parse(sTxt.Substring(0, 2)));
                // wp_ss_20180527_0001
                // 01234567890123 - moglby byc z daty pliku, ale jakby byla jakas edycja czy cos takiego...

                oFileTime = new DateTime(int.Parse(oFile.Name.Substring(6, 4)),
                                        int.Parse(oFile.Name.Substring(10, 2)),
                                        int.Parse(oFile.Name.Substring(12, 2)));
            }
            catch
            {
                return false;
            }

            int iDni = 0;
            iDni = (int)(oDataEnd - oFileTime).TotalDays + 1;
            if (iDni < 1)
            {
                App.DialogBox("Zła data? Zrzut ekranu z zeszłego okresu?");
                return false;
            }

            sTxt = OpisStanLimit("Minut", iMin, iDni, "bShowNumMins") + "\n" + OpisStanLimit("SMS", iSMS, iDni, "bShowNumSMS") + "\nInet: " + iInet + "\n";
            // "Minut: " & iMin & " (" & (iMin / iDni).ToString("###.#") & "/d"

            // powyższe ustawi ikonkę jako Badge, ale jeśli trzeba zmienić całkiem...
            if(App.GetSettingsBool("bShowBothNum"))
                {
                // zmień ikonkę na tekst
                UstawTextLiveTile(iMin, iSMS, iDni);
                }

           // ")" & vbCrLf &
           // "SMS: " & iSMS & " (" & (iSMS / iDni).ToString("###.#") & "/d)" & vbCrLf &

           uiOCR.Text = sTxt;  // przeniesione tutaj - tylko gdy jest poprawny tekst
            App.SetSettingsString("lastrunstat", sTxt);
            App.SetSettingsInt("Minut", iMin);
            App.SetSettingsInt("Dni", iDni);
            App.SetSettingsInt("Sms", iSMS);
            App.SetSettingsInt("Inet", iInet);
            App.SetSettingsString("lastOCR", rOCR.Text);
            return true;
        }

        private void UstawTextLiveTile(int iMin, int iSMS, int iDni)
        {
            string sAddMin = " ";
            string sAddSMS = " ";

            if (App.GetSettingsBool("bShowBezwzgledna"))
            {
                // liczby bezwzględne
                sAddMin = iMin.ToString();
                sAddSMS = iSMS.ToString();
            }
            else
            {
                // względem średniej

                double dDbl;
                dDbl = App.GetSettingsInt("limitMinut", 300);
                if (dDbl > 0.0)
                {
                    dDbl = (double)iMin - (((double)iDni * dDbl) / 30.0);
                    sAddMin = dDbl.ToString("+###;-###;=0");
                }
            
                dDbl = App.GetSettingsInt("limitSMS", 300);
                if (dDbl > 0.0)
                {
                    dDbl = (double)iSMS - (((double)iDni * dDbl) / 30.0);
                    sAddSMS = dDbl.ToString("+###;-###;=0");
                }
            }

            //' caption   12 regular
            //' body      15 regular
            //' base      15 semibold
            //' subtitle  20 regular
            //' title     24 semilight <- to jest w dailyIti, chyba za duże żeby dwa się zmieściły
            //' subheader 34 light
            //' header    46 light

            sAddSMS = "<text hint-style='subtitle' hint-align='center'>" + sAddSMS + "</text>";
            sAddMin = "<text hint-style='subtitle' hint-align='center'>" + sAddMin + "</text>";

            string sTmp;
            sTmp = "<tile><visual>";

            sTmp += "<binding template ='TileSmall' branding='none' hint-textStacking='center'>";
            sTmp = sTmp + sAddMin + sAddSMS;
            sTmp += "</binding>";

            sTmp += "<binding template ='TileMedium' branding='none' hint-textStacking='center'>";
            sTmp = sTmp + sAddMin + sAddSMS;
            sTmp += "</binding>";

            sTmp += "<binding template ='TileWide' branding='none' hint-textStacking='center'>";
            sTmp = sTmp + sAddMin + sAddSMS;
            sTmp += "</binding>";

            sTmp += "<binding template ='TileLarge' branding='none' hint-textStacking='center'>";
            sTmp = sTmp + sAddMin + sAddSMS;
            sTmp += "</binding>";

            sTmp += "</visual></tile>";

            Windows.UI.Notifications.TileNotification oTile;

            Windows.Data.Xml.Dom.XmlDocument oXml = new Windows.Data.Xml.Dom.XmlDocument();
            oXml.LoadXml(sTmp);
            oTile = new Windows.UI.Notifications.TileNotification(oXml);

            Windows.UI.Notifications.TileUpdater oTUPS;
            oTUPS = Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication();
            oTUPS.Clear();

            oTUPS.Update(oTile);
            // oraz usuń Badge
            Windows.UI.Notifications.BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            
        }

        private string OpisStanLimit(string sTyp, int iCurr, int iDays, string sSettName)
        {
            string sTxt;
            sTxt = sTyp + ": " + iCurr + " (" + ((double)iCurr / (double)iDays).ToString("###.#") + "/d";

            double dDbl;
            dDbl = App.GetSettingsInt("limit" + sTyp, 100);
            if (dDbl > 0.0)
            {
                dDbl = (double)iCurr - (((double)iDays * dDbl) / 30.0);
                sTxt = sTxt + ", " + dDbl.ToString("+###;-###");
            }

            sTxt = sTxt + ")";

            if (App.GetSettingsBool(sSettName))
                App.SetBadgeNo((int)dDbl);

            return sTxt;
        }

        private async void uiZrobOCR_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ProgresywnyRing(true);
            try
            {
                // ostatni screenshot

                // 1. folder
                Windows.Storage.StorageFolder oFold = null;
                oFold = await ZnajdzFolder();

                if (oFold == null)
                {
                    App.DialogBox("Niedostępny katalog zrzutów ekranu?");
                    return;
                }

                // 2. iteracja plikow
                string sFile = await ZnajdzPliczek(oFold);

                if (string.IsNullOrEmpty(sFile))
                {
                    App.DialogBox("Nie ma zrzutu ekranu? Żadnego?");
                    return;
                }
                // wp_ss_20180527_0001

                // 3. rozpoznanie (opakowane ProgresywnymRingiem)
                Windows.Storage.StorageFile oFileOne = await oFold.GetFileAsync(sFile);
                if (!await ZrobOCR(oFileOne)) return;

                // jesli sie udalo rozpoznanie, to mozna
                // 99. usunięcie obrazka

                Windows.Foundation.IAsyncAction oAsync = null;
                if (App.GetSettingsBool("AutoDel", true))
                    oAsync = oFileOne.DeleteAsync();

                // w trakcie usuwania moze sobie przerysowac, a co :)
                App.gbAfterOCR = true;

                uiPage_Loaded(null, null);
                if (App.GetSettingsBool("AutoDel", true))
                    await oAsync;

            }
            finally
            {
                ProgresywnyRing(false);
            }
        }

        private void SetUiBar(ref Windows.UI.Xaml.Controls.ProgressBar oBar, string sSetting, int iGranica = 10)
        {
            int iVal = App.GetSettingsInt(sSetting);
            oBar.Value = iVal;
            if (iVal < iGranica)
                oBar.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            else
                oBar.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Blue);
        }
        private void SetUiSlider(ref Windows.UI.Xaml.Controls.Slider oSld, string sSetting)
        {
            int iDni = App.GetSettingsInt("Dni");
            if (iDni < 1)
                return;  // było: =0

            double dDaily = App.GetSettingsInt(sSetting);
            dDaily /= iDni;
            dDaily = Math.Log(dDaily, 3.3);
            dDaily = Math.Min(dDaily, oSld.Maximum);
            dDaily = Math.Max(dDaily, oSld.Minimum);

            oSld.Value = dDaily;
            if (dDaily < 0.5)
                oSld.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            else if (dDaily < 1.0)
                oSld.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Yellow);
            else if (dDaily < 2.0)
                oSld.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Blue);
            else
                oSld.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);
        }

        private void uiPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!App.gbAfterOCR)
                uiOCR.Text = "(arch) " + App.GetSettingsString("lastOCR");
            else
                uiOCR.Text = App.GetSettingsString("lastOCR");

            uiStat.Text = App.GetSettingsString("lastrunstat");

            if (App.GetSettingsInt("Dni") == 0)
                return;

            SetUiBar(ref uiMinBar, "Minut");
            SetUiBar(ref uiSmsBar, "SMS");
            SetUiBar(ref uiInetBar, "Inet", 50);

            SetUiSlider(ref uiMinSld, "Minut");
            SetUiSlider(ref uiSmsSld, "SMS");


        }

        //private void uiCallnij_Click(object sender, RoutedEventArgs e)
        //{
        //    // ułatwienie w szukaniu
        //    Windows.ApplicationModel.Calls.PhoneCallManager.ShowPhoneCallUI("*137#", "Lyca ile");
        //}

        private async void uiCallnij1_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            bool bError = false;
            try
            {
                Windows.ApplicationModel.Calls.PhoneCallStore oPhoneCallStore = await Windows.ApplicationModel.Calls.PhoneCallManager.RequestStoreAsync();
                if (oPhoneCallStore is null)
                    App.DialogBox("brak dostępu do funkcji telefonu");
                else
                {
                Guid LineGuid = await oPhoneCallStore.GetDefaultLineAsync();

                Windows.ApplicationModel.Calls.PhoneLine oPhoneLine = await Windows.ApplicationModel.Calls.PhoneLine.FromIdAsync(LineGuid);
                oPhoneLine.Dial("*222#", "VirginMobile ile");
                }
            }
            catch
            {
                bError = true;
            }

            if (bError)
                App.DialogBox("To nie jest telefon?");
        }

        private void uiLimit_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }
    }
}

