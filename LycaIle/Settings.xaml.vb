' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Settings
    Inherits Page

    Private Sub uiSave_Click(sender As Object, e As RoutedEventArgs)
        App.SetSettingsInt("limitMinut", uiMins.Text)
        App.SetSettingsInt("limitSMS", uiSMS.Text)

        App.SetSettingsBool("AutoDel", uiDelPic.IsOn)
        App.SetSettingsBool("bShowNumMins", uiShowNumMins.IsOn)
        App.SetSettingsBool("bShowNumSMS", uiShowNumSMS.IsOn)

        Me.Frame.GoBack()
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiVersion.Text = "wersja " & Package.Current.Id.Version.Major & "." &
            Package.Current.Id.Version.Minor & "." & Package.Current.Id.Version.Build

        uiMins.Text = App.GetSettingsInt("limitMinut", 100)
        uiSMS.Text = App.GetSettingsInt("limitSMS", 100)

        uiDelPic.IsOn = App.GetSettingsBool("AutoDel", True)
        uiShowNumMins.IsOn = App.GetSettingsBool("bShowNumMins")
        uiShowNumSMS.IsOn = App.GetSettingsBool("bShowNumSMS")


    End Sub

    Private Sub uiNumMins_Toggled(sender As Object, e As RoutedEventArgs) Handles uiShowNumMins.Toggled
        If uiShowNumMins.IsOn Then uiShowNumSMS.IsOn = False
    End Sub

    Private Sub uiNumSMS_Toggled(sender As Object, e As RoutedEventArgs) Handles uiShowNumSMS.Toggled
        If uiShowNumSMS.IsOn Then uiShowNumMins.IsOn = False
    End Sub

    Private Sub uiRateIt_Click(sender As Object, e As RoutedEventArgs)
        Dim sUri As New Uri("ms-windows-store://review/?PFN=" & Package.Current.Id.FamilyName)
        Windows.System.Launcher.LaunchUriAsync(sUri)
    End Sub
End Class
