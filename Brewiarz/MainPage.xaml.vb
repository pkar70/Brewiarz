' nie dziala powiekszanie tekstu?

' 2019.08.30
' * dodanie ProgressBar podczas wczytywania całego dnia do cache

Imports System.Net.Http
Imports System.Net.NetworkInformation
Imports System.Text

Public NotInheritable Class MainPage
    Inherits Page

    Private Function Num2Roman(iNo As Integer)
        Select Case iNo
            Case 1
                Return "i"
            Case 2
                Return "ii"
            Case 3
                Return "iii"
            Case 4
                Return "iv"
            Case 5
                Return "v"
            Case 6
                Return "vi"
            Case 7
                Return "vii"
            Case 8
                Return "viii"
            Case 9
                Return "ix"
            Case 10
                Return "x"
            Case 11
                Return "xi"
            Case 12
                Return "xii"
            Case Else
                Return "??"
        End Select
    End Function

    Private Function Latin1ToLatin2(ByRef sTxt As String) As String
        sTxt = sTxt.Replace(ChrW(177), "ą")
        sTxt = sTxt.Replace(ChrW(230), "ć")
        sTxt = sTxt.Replace(ChrW(234), "ę")
        sTxt = sTxt.Replace(ChrW(179), "ł")
        sTxt = sTxt.Replace(ChrW(241), "ń")
        'sTxt = sTxt.Replace(ChrW(191), "ó")
        sTxt = sTxt.Replace(ChrW(182), "ś")
        ' sTxt = sTxt.Replace("�", "ś")
        sTxt = sTxt.Replace(ChrW(191), "ż")
        sTxt = sTxt.Replace(ChrW(188), "ź")

        sTxt = sTxt.Replace(ChrW(161), "Ą")
        sTxt = sTxt.Replace(ChrW(198), "Ć")
        sTxt = sTxt.Replace(ChrW(202), "Ę")
        sTxt = sTxt.Replace(ChrW(163), "Ł")
        sTxt = sTxt.Replace(ChrW(209), "Ń")
        ' sTxt = sTxt.Replace(ChrW(191), "Ó")
        sTxt = sTxt.Replace(ChrW(166), "Ś")
        sTxt = sTxt.Replace(ChrW(175), "Ż")
        sTxt = sTxt.Replace(ChrW(172), "Ź")

        Return sTxt
    End Function

    Private Function Date2Link(Optional iDay As Integer = 0, Optional iMonth As Integer = 0, Optional iYear As Integer = 0) As String
        If iYear = 0 Then iYear = Date.Now.Year
        If iMonth = 0 Then iMonth = Date.Now.Month
        If iDay = 0 Then iDay = Date.Now.Day

        iYear = iYear Mod 100
        Return Num2Roman(Date.Now.Month) & "_" & iYear & "/" & iDay.ToString("d2") & iMonth.ToString("d2")
    End Function

    Private Sub ZmianaCiemneJasneMenu(bNight As Boolean)

        ' wazne tylko przy ForceTheme black na jasnym (czyli PC)
        If Not bNight Then Exit Sub
        If App.IsMobile Then Exit Sub

        Dim oFore As Brush
        oFore = New SolidColorBrush(Windows.UI.Colors.Black)

        uiThemeJasne.Background = oFore
        uiThemeCiemne.Background = oFore
        uiThemeClock.Background = oFore
        ' uiMsza.RequestedTheme = ElementTheme.Dark
        uiInfo.Background = oFore
        uiMsza.Background = oFore
        uiGCzyt.Background = oFore
        uiJutrznia.Background = oFore
        uiTercja.Background = oFore
        uiSeksta.Background = oFore
        uiNona.Background = oFore
        uiNieszport.Background = oFore
        uiKompleta.Background = oFore

        ' to nic nie daje - dalej jest bialy tekst na szarym tle

        'Dim oMode As ElementTheme
        ''If bNight Then
        ''    oMode = ElementTheme.Dark
        ''Else
        'oMode = ElementTheme.Light
        ''End If
        'uiThemeJasne.RequestedTheme = oMode
        'uiThemeCiemne.RequestedTheme = oMode
        'uiThemeClock.RequestedTheme = oMode

        'uiInfo.RequestedTheme = oMode
        'uiMsza.RequestedTheme = oMode
        'uiGCzyt.RequestedTheme = oMode
        'uiJutrznia.RequestedTheme = oMode
        'uiTercja.RequestedTheme = oMode
        'uiSeksta.RequestedTheme = oMode
        'uiNona.RequestedTheme = oMode
        'uiNieszport.RequestedTheme = oMode
        'uiKompleta.RequestedTheme = oMode




    End Sub

    Private Function MakeBodyTag() As String
        Dim iMode As Integer = App.GetJasneCiemneMode()

        Dim oBiale As Brush = New SolidColorBrush(Windows.UI.Colors.White)

        Select Case iMode
            Case 0  ' jasne
                App.SetCiemneJasneTheme(False)
                ZmianaCiemneJasneMenu(False)
                'uiGrid.Background = oBiale
                'uiKiedyCo.Foreground = oCzarne
                'uiTitle.Foreground = oCzarne
                Return "<body>"
            Case 1 ' ciemne
                App.SetCiemneJasneTheme(True)
                ZmianaCiemneJasneMenu(True)
                'uiGrid.Background = oCzarne
                'uiKiedyCo.Foreground = oBiale
                'uiTitle.Foreground = oBiale
                Return "<body bgcolor='#000000' style='color:#eeeeee'>"
        End Select

        Return "<body>"
    End Function


    Private Async Function CacheLoad(sFileName As String) As Task(Of String)
        Dim oObj As Windows.Storage.StorageFile
        oObj = Await Windows.Storage.ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(sFileName)
        If oObj Is Nothing Then Return ""

        Dim oFile As Windows.Storage.StorageFile = TryCast(oObj, Windows.Storage.StorageFile)
        If oFile.DateCreated.AddDays(30) < Date.Now Then
            Await oFile.DeleteAsync
            Return ""    ' za stare, pewnie zeszloroczne
        End If

        Return Await Windows.Storage.FileIO.ReadTextAsync(oFile)
    End Function

    Private Async Function CacheSave(sFileName As String, sContent As String) As Task
        Dim oFile As Windows.Storage.StorageFile

        ' najpierw kasujemy, bo inaczej CreateDate jest z poprzedniego miesiaca
        Try
            Await Windows.Storage.ApplicationData.Current.LocalCacheFolder.DeleteAsync(sFileName)
        Catch ex As Exception
            ' wszak moze nie byc
        End Try

        oFile = Await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFileAsync(
                sFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting)
        Await Windows.Storage.FileIO.WriteTextAsync(oFile, sContent)

    End Function

    Private Async Function DownloadWebPage(sLink As String, sLinkFallback As String) As Task(Of String)
        Dim sTmp As String = ""

        'sTmp = Await CacheLoad(sFileName)
        'If sTmp <> "" Then Return sTmp

        Dim oHttp As New HttpClient()
        Dim oStream As Stream = Nothing ' musi byc poprzez stream, bo inaczej psuje politki

        Dim bError As Boolean = False
        oHttp.Timeout = TimeSpan.FromSeconds(8)

        Try
            oStream = Await oHttp.GetStreamAsync(sLink)
            ' sTmp = Await oHttp.GetStringAsync(sLink)
        Catch ex As Exception
            bError = True
        End Try

        If bError Then
            bError = False
            Try
                oStream = Await oHttp.GetStreamAsync(sLinkFallback)
                'sTmp = Await oHttp.GetStringAsync(sLinkFallback)
            Catch ex As Exception
                bError = True
            End Try
        End If

        If bError Then
            App.DialogBox("resErrorGetHttp")
            Return ""
        End If

        Dim oRdr = New StreamReader(oStream, Encoding.GetEncoding("ISO-8859-1"))

        sTmp = oRdr.ReadToEnd

        Return sTmp
    End Function


    Private Function KonwersjaStrony(sPage As String, sModl As String) As String
        Dim iInd As Integer

        sPage = Latin1ToLatin2(sPage)

        If sModl = "index" Then
            iInd = sPage.IndexOf("Dzisiejsi")
            If iInd < 10 Then Return ""
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("</table")
            If iInd < 10 Then Return ""
            iInd = sPage.LastIndexOf("</div", iInd)
            sPage = sPage.Substring(0, iInd)

            sPage = sPage.Replace("font-size:8pt; ", "")
        Else

            If sModl = "czyt" Then
                iInd = sPage.IndexOf("Teksty liturgii Mszy")
                If iInd < 10 Then Return ""
                iInd = sPage.LastIndexOf("<div", iInd)
                If iInd < 10 Then Return ""
                sPage = sPage.Substring(iInd)

                iInd = sPage.IndexOf("Polecamy komentarze")
                If iInd < 10 Then Return ""
                iInd = sPage.LastIndexOf("</table", iInd)
                If iInd < 10 Then Return ""
                sPage = sPage.Substring(0, iInd)

                Dim iInd1, iInd2 As Integer
                iInd1 = sPage.IndexOf(" width")
                While iInd1 > 0
                    iInd2 = sPage.IndexOf(" ", iInd1 + 2)
                    sPage = sPage.Remove(iInd1, iInd2 - iInd1 + 1)
                    iInd1 = sPage.IndexOf(" width")
                End While

            Else
                iInd = sPage.IndexOf("e, wejrzyj ku wspomo")
                If iInd < 10 Then Return ""
                iInd = sPage.LastIndexOf("<div class", iInd)
                If iInd < 10 Then Return ""
                sPage = sPage.Substring(iInd)
                iInd = sPage.IndexOf("Wydawnictwo Pallottinum")
                If iInd < 10 Then Return ""
                iInd = sPage.LastIndexOf("</table", iInd)
                If iInd < 10 Then Return ""
                sPage = sPage.Substring(0, iInd)
            End If

            ' 2019.01.27
            iInd = sPage.IndexOf("premium.brewiarz.pl")
            While iInd > 0
                Dim iInd1, iInd2 As Integer
                iInd1 = sPage.LastIndexOf("<div", iInd)
                iInd2 = sPage.IndexOf("</div", iInd)
                iInd2 = sPage.IndexOf(">", iInd2)
                sPage = sPage.Remove(iInd1, iInd2 - iInd1 + 1)
                iInd = sPage.IndexOf("premium.brewiarz.pl")
            End While

            ' 2019.02.02 - takze access.php3
            iInd = sPage.IndexOf("access.php3")
            While iInd > 0
                Dim iInd1, iInd2 As Integer
                iInd1 = sPage.LastIndexOf("<div", iInd)
                iInd2 = sPage.IndexOf("</div", iInd)
                iInd2 = sPage.IndexOf(">", iInd2)
                sPage = sPage.Remove(iInd1, iInd2 - iInd1 + 1)
                iInd = sPage.IndexOf("access.php3")
            End While
        End If


        ' 2018.08.09
        'sPage = sPage.Replace("<td>", " ")
        'sPage = sPage.Replace("</td>", " ")
        'sPage = sPage.Replace("<tr>", " ")
        'sPage = sPage.Replace("</tr>", " ")
        'sPage = sPage.Replace("<table>", " ")
        'sPage = sPage.Replace("</table>", " ")
        'sPage = sPage.Replace("<div>", " ")
        'sPage = sPage.Replace("</div>", " ") ' ale co z: <div class=c></div>

        ' bo to psuje 'darktheme'
        sPage = sPage.Replace("color:black", " ")
        sPage = sPage.Replace("color: black", " ") ' 
        sPage = sPage.Replace("font-size:10pt", " ")
        sPage = sPage.Replace("font-size:7.5pt", "font-size:9pt")
        sPage = sPage.Replace("font-size: 10pt", " ")     ' Ciebie Boże wychwalamy...
        sPage = sPage.Replace("font-size:8pt;", "font-size:10pt; ")      ' wprowadzenie ze mozna opuscic drugą część w Ciebie Boże; tam jeszcze jest zmiana fontu na Verdana

        Return sPage

    End Function

    Private Async Function WczytajModlitwe_Main(sModl As String) As Task(Of String)

        Dim sTmp As String = ""

        Dim sFilename As String = Date.Now.ToString("MMdd") & "-" & sModl & ".htm"
        sTmp = Await CacheLoad(sFilename)
        If sTmp = "" Then

            If Not NetworkInterface.GetIsNetworkAvailable() Then
                App.DialogBox("resErrorNoNetwork")
                Return ""
            End If

            uiNext.IsEnabled = False
            uiPrev.IsEnabled = False
            uiLoadDay.IsEnabled = False

            Dim sLinkBase As String

            sLinkBase = "http://brewiarz.pl/" & Date2Link()
            If sModl = "index" Then
                sTmp = Await DownloadWebPage(sLinkBase & "p/index.php3", sLinkBase & "/index.php3")
            Else
                sTmp = Await DownloadWebPage(
                     sLinkBase & "/" & sModl & ".php3?off=1", sLinkBase & "p/" & sModl & ".php3?off=1")
            End If

            Await CacheSave(sFilename, sTmp)
        End If

        sTmp = KonwersjaStrony(sTmp, sModl)
        sTmp = "<html><head>" & MetaViewport() & "</head>" & vbCrLf &
            MakeBodyTag() & vbCrLf &
            sTmp & vbCrLf &
            "</table></body></html>"

        uiNext.IsEnabled = True
        uiPrev.IsEnabled = True
        uiLoadDay.IsEnabled = True

        Return sTmp

    End Function

    Private Function MetaViewport() As String
        Dim dScale As Double = App.GetSettingsInt("fontSize", 100) / 100
        Dim sScale As String = "initial-scale=" & dScale.ToString("0.##")
        Return "<meta name=""viewport"" content=""width=device-width, " & sScale & """>"
    End Function

    Private Async Sub WczytajModlitwe(sModl As String)
        Dim sTmp As String = Await WczytajModlitwe_Main(sModl)
        If sTmp = "" Then Exit Sub
        uiWeb.NavigateToString(sTmp)
        uiKiedyCo.Text = sModl
    End Sub

    Private Sub uiGCzyt_Click(sender As Object, e As RoutedEventArgs) Handles uiGCzyt.Click
        WczytajModlitwe("godzczyt")
    End Sub

    Private Sub uiMsza_Click(sender As Object, e As RoutedEventArgs) Handles uiMsza.Click
        WczytajModlitwe("czyt")
    End Sub

    Private Sub uiJutrznia_Click(sender As Object, e As RoutedEventArgs) Handles uiJutrznia.Click
        WczytajModlitwe("jutrznia")
    End Sub

    Private Sub uiTercja_Click(sender As Object, e As RoutedEventArgs) Handles uiTercja.Click
        WczytajModlitwe("modlitwa1")
    End Sub

    Private Sub uiSeksta_Click(sender As Object, e As RoutedEventArgs) Handles uiSeksta.Click
        WczytajModlitwe("modlitwa2")
    End Sub

    Private Sub uiNona_Click(sender As Object, e As RoutedEventArgs) Handles uiNona.Click
        WczytajModlitwe("modlitwa3")
    End Sub

    Private Sub uiNieszpory_Click(sender As Object, e As RoutedEventArgs) Handles uiNieszport.Click
        WczytajModlitwe("nieszpory")
    End Sub

    Private Sub uiKompleta_Click(sender As Object, e As RoutedEventArgs) Handles uiKompleta.Click
        WczytajModlitwe("kompleta")
    End Sub

    Private Sub uiInfo_Click(sender As Object, e As RoutedEventArgs) Handles uiInfo.Click
        WczytajModlitwe("index")
    End Sub

    Private Sub uiPrev_Click(sender As Object, e As RoutedEventArgs) Handles uiPrev.Click
        Select Case uiKiedyCo.Text
            Case "czyt"
                ' nic
            Case "godzczyt"
                WczytajModlitwe("czyt")
            Case "jutrznia"
                WczytajModlitwe("godzczyt")
            Case "modlitwa1"
                WczytajModlitwe("jutrznia")
            Case "modlitwa2"
                WczytajModlitwe("modlitwa1")
            Case "modlitwa3"
                WczytajModlitwe("modlitwa2")
            Case "nieszpory"
                WczytajModlitwe("modlitwa3")
            Case "kompleta"
                WczytajModlitwe("nieszpory")
        End Select
    End Sub

    Private Sub uiNext_Click(sender As Object, e As RoutedEventArgs) Handles uiNext.Click
        Select Case uiKiedyCo.Text
            Case "czyt"
                WczytajModlitwe("godzczyt")
            Case "godzczyt"
                WczytajModlitwe("jutrznia")
            Case "jutrznia"
                WczytajModlitwe("modlitwa1")
            Case "modlitwa1"
                WczytajModlitwe("modlitwa2")
            Case "modlitwa2"
                WczytajModlitwe("modlitwa3")
            Case "modlitwa3"
                WczytajModlitwe("nieszpory")
            Case "nieszpory"
                WczytajModlitwe("kompleta")
            Case "kompleta"
                ' nie ma next
        End Select


    End Sub

    Private Sub InitModeSwitch(Optional iMode As Integer = -1)
        If iMode = -1 Then iMode = App.GetSettingsInt("modeTheme", 1)

        uiThemeCiemne.IsChecked = False
        uiThemeJasne.IsChecked = False
        uiThemeClock.IsChecked = False

        Select Case iMode
            Case 0
                uiThemeJasne.IsChecked = True
                uiTheme.Icon = New SymbolIcon(Symbol.SolidStar)
            Case 1
                uiThemeCiemne.IsChecked = True
                uiTheme.Icon = New SymbolIcon(Symbol.OutlineStar)
            Case 2
                uiThemeClock.IsChecked = True
                uiTheme.Icon = New SymbolIcon(Symbol.Clock)
        End Select

    End Sub

    Private Sub uiThemeJasne_Click(sender As Object, e As RoutedEventArgs) Handles uiThemeJasne.Click, uiThemeCiemne.Click, uiThemeClock.Click
        Dim sName As String = TryCast(sender, ToggleMenuFlyoutItem).Name

        Dim iMode As Integer = 1
        Select Case sName
            Case "uiThemeJasne"
                iMode = 0
            Case "uiThemeCiemne"
                iMode = 1
            Case "uiThemeClock"
                iMode = 2
        End Select

        InitModeSwitch(iMode)
        App.SetSettingsInt("modeTheme", iMode)
    End Sub

    Private Async Function RegisterTrigger() As Task

        If Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily <> "Windows.Mobile" Then Exit Function
        ' tylko dla mobile idziemy dalej

        ' na pozniej: bo wymaga np. cogodzinnego az trafi na siec, wtedy download...

        Dim oBAS As Background.BackgroundAccessStatus
        oBAS = Await Windows.ApplicationModel.Background.BackgroundExecutionManager.RequestAccessAsync()

        If oBAS = Windows.ApplicationModel.Background.BackgroundAccessStatus.AlwaysAllowed Or oBAS = Windows.ApplicationModel.Background.BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then
            '    ' https://docs.microsoft.com/en-us/windows/uwp/launch-resume/create-And-register-an-inproc-background-task
            Dim builder As Background.BackgroundTaskBuilder = New Background.BackgroundTaskBuilder


            ' dla UniversalApiContract 6.0, a to oznacza SDK 17134
            'builder.AddCondition(New Windows.ApplicationModel.Background.SystemConditionType.FreeNetworkAvailable)
            'builder.AddCondition(New Windows.ApplicationModel.Background.SystemConditionType.InternetAvailable)

            Dim oDateMew As Date
            oDateMew = New Date(Date.Now.Year, Date.Now.Month, Date.Now.Day, 0, 40, 0).AddDays(1)

            Dim iMin As Integer = (oDateMew - Date.Now).TotalMinutes

            builder.SetTrigger(New Background.TimeTrigger(iMin, True))
            builder.Name = "PKARbrewiarz_Daily"
            Dim oRet As Background.BackgroundTaskRegistration
            oRet = builder.Register()
        End If

    End Function

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        InitModeSwitch()

        ' dla ciemnego - zrob tło ciemne
        Dim sTmp As String = "<html><head>" & MetaViewport() & "</head>" & vbCrLf &
            MakeBodyTag() & vbCrLf &
            "</body></html>"
        uiWeb.NavigateToString(sTmp)

    End Sub

    Private Async Sub UiLoadDay_Click(sender As Object, e As RoutedEventArgs) Handles uiLoadDay.Click

        If Not NetworkInterface.GetIsNetworkAvailable() Then
            App.DialogBox("resErrorNoNetwork")
            Exit Sub
        End If

        ' ewentualnie: download DAY/WEEK/MONTH

        uiLoadProgress.Maximum = 9
        uiLoadProgress.Minimum = 0
        uiLoadProgress.Value = 0
        uiLoadProgress.Visibility = Visibility.Visible

        Await WczytajModlitwe_Main("index")
        uiLoadProgress.Value = 1
        Await WczytajModlitwe_Main("czyt")
        uiLoadProgress.Value = 2
        Await WczytajModlitwe_Main("godzczyt")
        uiLoadProgress.Value = 3
        Await WczytajModlitwe_Main("jutrznia")
        uiLoadProgress.Value = 4
        Await WczytajModlitwe_Main("modlitwa1")
        uiLoadProgress.Value = 5
        Await WczytajModlitwe_Main("modlitwa2")
        uiLoadProgress.Value = 6
        Await WczytajModlitwe_Main("modlitwa3")
        uiLoadProgress.Value = 7
        Await WczytajModlitwe_Main("nieszpory")
        uiLoadProgress.Value = 8
        Await WczytajModlitwe_Main("kompleta")

        uiLoadProgress.Visibility = Visibility.Collapsed

    End Sub

    Private bInSizeChange As Boolean = False
    Private Sub uiSizeChange_Click(sender As Object, e As RoutedEventArgs)
        Dim oTmfi As ToggleMenuFlyoutItem = TryCast(sender, ToggleMenuFlyoutItem)
        If oTmfi Is Nothing Then Return
        'otmfi = uiSize100
        If bInSizeChange Then Return
        Dim iSize As Integer = oTmfi.Name.Replace("uiSize", "")
        App.SetSettingsInt("fontSize", iSize)

        ' przełącz pokazywane
        bInSizeChange = True
        uiSize100.IsChecked = (iSize = 100)
        uiSize110.IsChecked = (iSize = 110)
        uiSize125.IsChecked = (iSize = 125)
        uiSize150.IsChecked = (iSize = 150)
        uiSize175.IsChecked = (iSize = 175)
        uiSize200.IsChecked = (iSize = 200)
        bInSizeChange = False

    End Sub
End Class
