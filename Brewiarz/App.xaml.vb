﻿Imports Windows.Storage
Imports Windows.UI.Popups
''' <summary>
''' Provides application-specific behavior to supplement the default Application class.
''' </summary>
NotInheritable Class App
    Inherits Application

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active


        If rootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

            If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                ' TODO: Load state from previously suspended application
            End If
            ' Place the frame in the current Window
            Window.Current.Content = rootFrame
        End If

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub

    Public Shared Async Sub DialogBox(sMsg As String)
        Dim oMsg As New MessageDialog(sMsg)
        Await oMsg.ShowAsync
    End Sub

    Public Shared Function GetSettingsInt(sName As String, Optional iDefault As Integer = 0) As Integer
        Dim sTmp As Integer

        sTmp = iDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = CInt(ApplicationData.Current.RoamingSettings.Values(sName).ToString)
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = CInt(ApplicationData.Current.LocalSettings.Values(sName).ToString)
        End If

        Return sTmp

    End Function

    Public Shared Sub SetSettingsInt(sName As String, sValue As Integer, Optional bRoam As Boolean = False)
        If bRoam Then ApplicationData.Current.RoamingSettings.Values(sName) = sValue.ToString
        ApplicationData.Current.LocalSettings.Values(sName) = sValue.ToString
    End Sub

    Public Sub SetCiemneJasneThemeMain(bNight As Boolean)
        If bNight Then
            MyBase.RequestedTheme = ApplicationTheme.Dark
        Else
            MyBase.RequestedTheme = ApplicationTheme.Light
        End If
    End Sub

    Public Shared Sub SetCiemneJasneTheme(bNight As Boolean)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, FrameworkElement)
        If rootFrame Is Nothing Then Exit Sub

        If GetJasneCiemneMode() Then
            rootFrame.RequestedTheme = ElementTheme.Dark
        Else
            rootFrame.RequestedTheme = ElementTheme.Light
        End If
        'ale to nie moze byc gdy app is running, wiec nie tak... w OnLaunched tez nie dziala
        ' próba zmiany theme
        'Dim iMode As Integer = GetJasneCiemneMode()
        'SetCiemneJasneThemeMain(iMode)

        ' TryCast(App.Current, App).SetCiemneJasneThemeMain(bNight)
    End Sub

    Public Shared Function GetJasneCiemneMode()
        Dim iMode As Integer = App.GetSettingsInt("modeTheme", 1)
        If iMode = 2 Then
            If Date.Now.Hour > 17 OrElse Date.Now.Hour < 8 Then
                iMode = 0
            Else
                iMode = 1
            End If
        End If
        Return iMode
    End Function

    Public Shared Function IsMobile() As Boolean
        Return (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily = "Windows.Mobile")
    End Function

End Class
