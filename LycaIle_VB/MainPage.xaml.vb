
'Imports Windows.Graphics.Imaging
'Imports Windows.Media.Ocr
'Imports Windows.Storage
'Imports Windows.Storage.Streams

' *** PRZENIESIONE DO UNO ***

' 2020.01.21
' * jakby nie bylo pakietu, to jest inny obrazek - obsługa tegoż

' 2019.05.15
' * rozszerzenie Page Limity do Settings
' * dodanie na niej numeru wersji App
' * migracja do niej autokasowania obrazka oraz RateIt
' * dodanie włączania pokazywania liczby SMS/minut przy ikonce

Public NotInheritable Class MainPage
    Inherits Page


    Private Async Function ZnajdzFolder() As Task(Of Windows.Storage.StorageFolder)
        Dim oFold As Windows.Storage.StorageFolder = Nothing
        Try
            oFold = Windows.Storage.KnownFolders.PicturesLibrary
            oFold = Await oFold.GetFolderAsync("Screenshots")
        Catch ex As Exception
        End Try

        Return oFold

    End Function

    Private Async Function ZnajdzPliczek(oFold As Windows.Storage.StorageFolder) As Task(Of String)

        ' 2. iteracja plikow
        Dim oDateDt As Date = New Date(2000, 1, 1)
        Dim sFile As String = ""
        Dim oDate As DateTimeOffset = oDateDt

        Try
            For Each oFile As Windows.Storage.StorageFile In Await oFold.GetFilesAsync
                If oFile.DateCreated > oDate Then
                    oDate = oFile.DateCreated
                    sFile = oFile.Name
                End If
            Next
        Catch ex As Exception
        End Try

        Return sFile

    End Function

    Private Async Function ZrobOCR(oFile As Windows.Storage.StorageFile) As Task(Of Boolean)
        Dim oStream As Windows.Storage.Streams.IRandomAccessStream =
            Await oFile.OpenAsync(Windows.Storage.FileAccessMode.Read)
        Dim oDecoder As Windows.Graphics.Imaging.BitmapDecoder =
            Await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(oStream)
        Dim oBmp As Windows.Graphics.Imaging.SoftwareBitmap =
            Await oDecoder.GetSoftwareBitmapAsync()

        Dim oTmp = Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages()

        Dim rOCR As Windows.Media.Ocr.OcrResult =
            Await Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages().RecognizeAsync(oBmp)

        Dim sTxt As String = rOCR.Text
        uiOCR.Text = sTxt

        Dim iInd As Integer
        If Not sTxt.Contains("Lycamobile") Then
            App.DialogBox("Nie widzę tekstu 'Lycamobile', to chyba nie ten screenshot")
            Return False
        End If

        If sTxt.Contains("nie masz zadnego aktywnego pakietu") Then
            App.DialogBox("Ponoć nie masz żadnego aktywnego pakietu")
            Return False
        End If

        iInd = sTxt.IndexOf("pozostalo")
        If iInd < 0 Then iInd = sTxt.IndexOf("pozostało")
        If iInd < 0 Then
            App.DialogBox("Nie widzę tekstu 'pozostało', to chyba nie ten screenshot")
            Return False
        End If
        sTxt = sTxt.Substring(iInd + 10)
        iInd = sTxt.IndexOf(" ")
        If iInd < 0 Then
            App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot")
            Return False
        End If
        Dim iMin As Integer = 0
        Try
            iMin = sTxt.Substring(0, iInd)
        Catch ex As Exception
        End Try

        iInd = sTxt.IndexOf(",")
        If iInd < 0 Then
            App.DialogBox("Nie widzę przecinka przed liczbą SMS, to chyba nie ten screenshot")
            Return False
        End If
        sTxt = sTxt.Substring(iInd + 2)
        iInd = sTxt.IndexOf(" ")
        If iInd < 0 Then
            App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot")
            Return False
        End If
        Dim iSMS As Integer = 0
        Try
            iSMS = sTxt.Substring(0, iInd)
        Catch ex As Exception
        End Try

        iInd = sTxt.IndexOf("&")
        If iInd < 0 Then
            App.DialogBox("Nie widzę '&' przed liczbą megabajtów, to chyba nie ten screenshot")
            Return False
        End If
        sTxt = sTxt.Substring(iInd + 2)
        iInd = sTxt.IndexOf(" ")
        If iInd < 0 Then
            App.DialogBox("Błąd rozpoznawania zawartości tekstu, to chyba nie ten screenshot")
            Return False
        End If
        Dim sInet As String = sTxt.Substring(0, iInd)
        sInet = sInet.Replace("MB", "")
        Dim iInet As Integer = 0

        Dim bError As Boolean = False
        Try
            iInet = sInet
        Catch ex As Exception
            bError = True
        End Try
        If bError Then
            bError = False
            sInet = sInet.Replace(".", ",")
            Try
                iInet = sInet
            Catch ex As Exception
                bError = True
            End Try
        End If

        iInd = sTxt.IndexOf(" do ")
        If iInd < 0 Then
            App.DialogBox("Nie widzę ' do ' przed datą końca okresu, to chyba nie ten screenshot")
            Return False
        End If
        sTxt = sTxt.Substring(iInd + 4)
        iInd = sTxt.IndexOf(".")
        If iInd < 0 Then
            App.DialogBox("Nie widzę kropki po dacie, to chyba nie ten screenshot")
            Return False
        End If
        Dim sData As String = sTxt.Substring(0, iInd)

        Dim oDataEnd As Date
        Dim oFileTime As Date

        Try
            oDataEnd = New Date(sData.Substring(6, 4), sData.Substring(3, 2), sData.Substring(0, 2))
            ' wp_ss_20180527_0001
            ' 01234567890123 - moglby byc z daty pliku, ale jakby byla jakas edycja czy cos takiego...

            oFileTime = New Date(oFile.Name.Substring(6, 4), oFile.Name.Substring(10, 2), oFile.Name.Substring(12, 2))
        Catch ex As Exception
            Return False
        End Try

        Dim iDni As Integer = 0
        iDni = CInt((oDataEnd - oFileTime).TotalDays + 1)
        If iDni < 1 Then
            App.DialogBox("Zła data? Zrzut ekranu z zeszłego okresu?")
            Return False
        End If

        sTxt = OpisStanLimit("Minut", iMin, iDni, "bShowNumMins") & vbCrLf &
                OpisStanLimit("SMS", iSMS, iDni, "bShowNumSMS") & vbCrLf &
            "Inet: " & iInet & vbCrLf
        '    "Minut: " & iMin & " (" & (iMin / iDni).ToString("###.#") & "/d"


        '")" & vbCrLf &
        '    "SMS: " & iSMS & " (" & (iSMS / iDni).ToString("###.#") & "/d)" & vbCrLf &


        App.SetSettingsString("lastrunstat", sTxt)
        App.SetSettingsInt("Minut", iMin)
        App.SetSettingsInt("Dni", iDni)
        App.SetSettingsInt("Sms", iSMS)
        App.SetSettingsInt("Inet", iInet)

        Return True
    End Function

    Private Function OpisStanLimit(sTyp As String, iCurr As Integer, iDays As Integer, sSettName As String)
        Dim sTxt As String
        sTxt = sTyp & ": " & iCurr & " (" & (iCurr / iDays).ToString("###.#") & "/d"

        Dim dDbl As Double
        dDbl = App.GetSettingsInt("limit" & sTyp, 100)
        If dDbl > 0 Then
            dDbl = iCurr - (iDays * dDbl / 30)
            sTxt = sTxt & ", " & dDbl.ToString("+###;-###")
        End If

        sTxt = sTxt & ")"

        If App.GetSettingsBool(sSettName) Then App.SetBadgeNo(dDbl)

        Return sTxt
    End Function

    Private Async Sub uiZrobOCR_Click(sender As Object, e As RoutedEventArgs)
        ' ostatni screenshot

        ' 1. folder
        Dim oFold As Windows.Storage.StorageFolder = Nothing
        oFold = Await ZnajdzFolder()

        If oFold Is Nothing Then
            App.DialogBox("Niedostępny katalog zrzutów ekranu?")
            Exit Sub
        End If

        ' 2. iteracja plikow
        Dim sFile As String = Await ZnajdzPliczek(oFold)

        If sFile = "" Then
            App.DialogBox("Nie ma zrzutu ekranu? Żadnego?")
            Exit Sub
        End If
        ' wp_ss_20180527_0001

        ' 3. rozpoznanie
        Dim oFileOne As Windows.Storage.StorageFile = Await oFold.GetFileAsync(sFile)
        If Not Await ZrobOCR(oFileOne) Then Exit Sub

        ' jesli sie udalo rozpoznanie, to mozna
        ' 99. usunięcie obrazka

        Dim oAsync As IAsyncAction = Nothing
        If App.GetSettingsBool("AutoDel", True) Then oAsync = oFileOne.DeleteAsync
        ' w trakcie usuwania moze sobie przerysowac, a co :)
        uiPage_Loaded(Nothing, Nothing)
        If App.GetSettingsBool("AutoDel", True) Then Await oAsync

    End Sub

    Private Sub SetUiBar(ByRef oBar As ProgressBar, sSetting As String, Optional iGranica As Integer = 10)
        Dim iVal As Integer = App.GetSettingsInt(sSetting)
        oBar.Value = iVal
        If iVal < iGranica Then
            oBar.Foreground = New SolidColorBrush(Windows.UI.Colors.Red)
        Else
            oBar.Foreground = New SolidColorBrush(Windows.UI.Colors.Blue)
        End If
    End Sub
    Private Sub SetUiSlider(ByRef oSld As Slider, sSetting As String)
        Dim iDni As Integer = App.GetSettingsInt("Dni")
        If iDni < 1 Then Exit Sub  ' było: =0

        Dim dDaily As Double = App.GetSettingsInt(sSetting) / iDni
        dDaily = Math.Log(dDaily, 3.3)
        dDaily = Math.Min(dDaily, oSld.Maximum)
        dDaily = Math.Max(dDaily, oSld.Minimum)

        oSld.Value = dDaily
        If dDaily < 0.5 Then
            oSld.Foreground = New SolidColorBrush(Windows.UI.Colors.Red)
        ElseIf dDaily < 1 Then
            oSld.Foreground = New SolidColorBrush(Windows.UI.Colors.Yellow)
        ElseIf dDaily < 2 Then
            oSld.Foreground = New SolidColorBrush(Windows.UI.Colors.Blue)
        Else
            oSld.Foreground = New SolidColorBrush(Windows.UI.Colors.Green)
        End If
    End Sub
    Private Sub uiPage_Loaded(sender As Object, e As RoutedEventArgs)
        uiStat.Text = App.GetSettingsString("lastrunstat")

        If App.GetSettingsInt("Dni") = 0 Then Exit Sub

        SetUiBar(uiMinBar, "Minut")
        SetUiBar(uiSmsBar, "SMS")
        SetUiBar(uiInetBar, "Inet", 50)

        SetUiSlider(uiMinSld, "Minut")
        SetUiSlider(uiSmsSld, "SMS")

        'uiAutoDel.IsChecked = App.GetSettingsBool("AutoDel", True)

    End Sub

    Private Sub uiCallnij_Click(sender As Object, e As RoutedEventArgs)
        ' ułatwienie w szukaniu
        Calls.PhoneCallManager.ShowPhoneCallUI("*137#", "Lyca ile")
    End Sub

    Private Async Sub uiCallnij1_Click(sender As Object, e As RoutedEventArgs)
        Dim bError As Boolean = False
        Try
            Dim oPhoneCallStore As Calls.PhoneCallStore = Await Calls.PhoneCallManager.RequestStoreAsync()
            Dim LineGuid As Guid = Await oPhoneCallStore.GetDefaultLineAsync()

            Dim oPhoneLine As Calls.PhoneLine = Await Calls.PhoneLine.FromIdAsync(LineGuid)
            oPhoneLine.Dial("*137#", "Lyca ile")
        Catch ex As Exception
            bError = True
        End Try

        If bError Then
            App.DialogBox("To nie jest telefon?")
        End If
    End Sub

    Private Sub uiLimit_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Settings))
    End Sub
End Class
