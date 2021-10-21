/* 
 
STORE 10.2001

 2020.01.21
 * jakby nie bylo pakietu, to jest inny obrazek - obsługa tegoż

STORE 10.1910

 2019.10.01
 * gdy nie ma OCR Engine dla języka, próbuje jeszcze dla angielskiego
 * nie wypisuje "+0", tylko zwykłe "0".
 * ProgressRing podczas OCRowania (i pierwsze w życiu użycie try/finally)
 
 2019.09.30
 * poprawka Manifest:Capabilities - nie miało prawa działać!
 * sprawdzanie czy OCR Engine się udało uzyskać

 STORE 10.1909

 2019.08.24
* migracja do Platform Uno

== LAST VERSION VC ==
2019.08.22
* jeśli jest błąd w OCR, to OCR jest pokazywany w DialogBox, ale nie jest pakowany na ekran
* możliwe pokazywanie i minut i SMS (ale wtedy nie ma ikonki, tylko same teksty)

2019.08.20
 * migracja do C# - przygotowanie do Project Uno / Xamarin
 * pamięta stan ostatniego OCR (żeby była dostępna data)

 
 STORE 1.1906, 2019.05.31 (== LAST VERSION VB ==)
 

 2019.05.15
 * rozszerzenie Page Limity do Settings
 * dodanie na niej numeru wersji App
 * migracja do niej autokasowania obrazka oraz RateIt
 * dodanie włączania pokazywania liczby SMS/minut przy ikonce





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



namespace LycaIle
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
            if (!sTxt.Contains("Lycamobile"))
            {
                App.DialogBox("Nie widzę tekstu 'Lycamobile', to chyba nie ten screenshot\n" + sTxt);
                return false;
            }

            if (sTxt.Contains("nie masz zadnego aktywnego pakietu"))
            {
                App.DialogBox("Ponoć nie masz żadnego aktywnego pakietu");
                return false;
            }

            iInd = sTxt.IndexOf("pozostalo");
            if (iInd < 0)
                iInd = sTxt.IndexOf("pozostało");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę tekstu 'pozostało', to chyba nie ten screenshot\n" + sTxt);
                return false;
            }
            sTxt = sTxt.Substring(iInd + 10);
            iInd = sTxt.IndexOf(" ");
            if (iInd < 0)
            {
                App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }
            int iMin = 0;
            int.TryParse(sTxt.Substring(0, iInd), out iMin);

            iInd = sTxt.IndexOf(",");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę przecinka przed liczbą SMS, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }
            sTxt = sTxt.Substring(iInd + 2);
            iInd = sTxt.IndexOf(" ");
            if (iInd < 0)
            {
                App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }
            int iSMS = 0;
            int.TryParse(sTxt.Substring(0, iInd), out iSMS);
            //try
            //{
            //    iSMS = int.Parse(sTxt.Substring(0, iInd));
            //}
            //catch (Exception ex)
            //{
            //}

            iInd = sTxt.IndexOf("&");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę '&' przed liczbą megabajtów, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }
            sTxt = sTxt.Substring(iInd + 2);
            iInd = sTxt.IndexOf(" ");
            if (iInd < 0)
            {
                App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }
            string sInet = sTxt.Substring(0, iInd);
            sInet = sInet.Replace("MB", "");
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
            iInd = sTxt.IndexOf(".");
            if (iInd < 0)
            {
                App.DialogBox("Nie widzę kropki po dacie, to chyba nie ten screenshot\n" + sTxt);
                return false;
            }
            string sData = sTxt.Substring(0, iInd);

            DateTime oDataEnd;
            DateTime oFileTime;

            try
            {
                oDataEnd = new DateTime(int.Parse(sData.Substring(6, 4)),
                                        int.Parse(sData.Substring(3, 2)),
                                        int.Parse(sData.Substring(0, 2)));
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

            double dDbl;
            dDbl = App.GetSettingsInt("limitMinut", 100);
            if (dDbl > 0.0)
            {
                dDbl = (double)iMin - (((double)iDni * dDbl) / 30.0);
                sAddMin = dDbl.ToString("+###;-###;0");
            }

            dDbl = App.GetSettingsInt("limitSMS", 100);
            if (dDbl > 0.0)
            {
                dDbl = (double)iSMS - (((double)iDni * dDbl) / 30.0);
                sAddSMS = dDbl.ToString("+###;-###;0");
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
                oPhoneLine.Dial("*137#", "Lyca ile");
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

