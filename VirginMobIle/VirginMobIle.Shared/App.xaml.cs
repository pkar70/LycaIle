using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VirginMobIle
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // ConfigureFilters(global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);

            this.InitializeComponent();
            this.Suspending += OnSuspending;
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
				// this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
            Frame rootFrame = Windows.UI.Xaml.Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
                rootFrame.Navigated += OnNavigatedAddBackButton;
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackButtonPressed;

                // PKAR: komentuje, bo i tak nie uzywam tego
                //if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                //{
                //    //TODO: Load state from previously suspended application
                //}

                // Place the frame in the current Window
                Windows.UI.Xaml.Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Windows.UI.Xaml.Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
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

        public static bool gbAfterOCR = false;


        // PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
        private void OnNavigatedAddBackButton(object sender, NavigationEventArgs e)
        {
            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            Windows.UI.Xaml.Controls.Frame oFrame = sender as Windows.UI.Xaml.Controls.Frame;
            if (oFrame == null)
                return;

            Windows.UI.Core.SystemNavigationManager oNavig = Windows.UI.Core.SystemNavigationManager.GetForCurrentView();

            if (oFrame.CanGoBack)
                oNavig.AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            else
                oNavig.AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
        }


        private void OnBackButtonPressed(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            try
            {
                (Window.Current.Content as Windows.UI.Xaml.Controls.Frame).GoBack();
                e.Handled = true;
            }
            catch
            {
            }
        }


        public static string GetSettingsString(string sName, string sDefault = "")
        {
            string sTmp;

            sTmp = sDefault;

            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName))
                sTmp = Windows.Storage.ApplicationData.Current.RoamingSettings.Values[sName].ToString();
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(sName))
                sTmp = Windows.Storage.ApplicationData.Current.LocalSettings.Values[sName].ToString();

            return sTmp;
        }

        public static void SetSettingsString(string sName, string sValue, bool bRoam = false)
        {
            if (bRoam)
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values[sName] = sValue;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[sName] = sValue;
        }

        public static int GetSettingsInt(string sName, int iDefault = 0)
        {
            int sTmp;

            sTmp = iDefault;

            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName))
                sTmp = System.Convert.ToInt32(Windows.Storage.ApplicationData.Current.RoamingSettings.Values[sName].ToString());
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(sName))
                sTmp = System.Convert.ToInt32(Windows.Storage.ApplicationData.Current.LocalSettings.Values[sName].ToString());

            return sTmp;
        }
        public static void SetSettingsInt(string sName, int sValue, bool bRoam = false)
        {
            if (bRoam)
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values[sName] = sValue.ToString();
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[sName] = sValue.ToString();
        }

        public static bool GetSettingsBool(string sName, bool iDefault = false)
        {
            bool sTmp;

            sTmp = iDefault;

            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName))
                sTmp = System.Convert.ToBoolean(Windows.Storage.ApplicationData.Current.RoamingSettings.Values[sName].ToString());
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(sName))
                sTmp = System.Convert.ToBoolean(Windows.Storage.ApplicationData.Current.LocalSettings.Values[sName].ToString());

            return sTmp;
        }

        public static void SetSettingsBool(string sName, bool? sValue, bool bRoam = false)
        {
            if (sValue.HasValue && sValue.Value)
                SetSettingsBool(sName, true, bRoam);
            else
                SetSettingsBool(sName, false, bRoam);
        }

        public static void SetSettingsBool(string sName, bool sValue, bool bRoam = false)
        {
            if (bRoam)
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values[sName] = sValue.ToString();
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[sName] = sValue.ToString();
        }

        public static async void DialogBox(string sMsg)
        {
            Windows.UI.Popups.MessageDialog oMsg = new Windows.UI.Popups.MessageDialog(sMsg);
            await oMsg.ShowAsync();
        }

        public static void SetBadgeNo(int iInt)
        {
            // https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-badges
            Windows.Data.Xml.Dom.XmlDocument oXmlBadge;
            oXmlBadge = Windows.UI.Notifications.BadgeUpdateManager.GetTemplateContent(Windows.UI.Notifications.BadgeTemplateType.BadgeNumber);
            Windows.Data.Xml.Dom.XmlElement oXmlNum;
            oXmlNum = (Windows.Data.Xml.Dom.XmlElement)oXmlBadge.SelectSingleNode("/badge");
            oXmlNum.SetAttribute("value", iInt.ToString());

            Windows.UI.Notifications.BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new Windows.UI.Notifications.BadgeNotification(oXmlBadge));
        }


        /// <summary>
        /// Configures global logging
        /// </summary>
        /// <param name="factory"></param>
//        static void ConfigureFilters(ILoggerFactory factory)
//        {
//            factory
//                .WithFilter(new FilterLoggerSettings
//                    {
//                        { "Uno", LogLevel.Warning },
//                        { "Windows", LogLevel.Warning },

//						// Debug JS interop
//						// { "Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug },

//						// Generic Xaml events
//						// { "Windows.UI.Xaml", LogLevel.Debug },
//						// { "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
//						// { "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
//						// { "Windows.UI.Xaml.UIElement", LogLevel.Debug },

//						// Layouter specific messages
//						// { "Windows.UI.Xaml.Controls", LogLevel.Debug },
//						// { "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
//						// { "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
//						// { "Windows.Storage", LogLevel.Debug },

//						// Binding related messages
//						// { "Windows.UI.Xaml.Data", LogLevel.Debug },

//						// DependencyObject memory references tracking
//						// { "ReferenceHolder", LogLevel.Debug },
//					}
//                )
//#if DEBUG
//				.AddConsole(LogLevel.Debug);
//#else
//                .AddConsole(LogLevel.Information);
//#endif
//        }
    }
}
