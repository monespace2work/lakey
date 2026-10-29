Imports System.Windows.Threading
Imports System.Data

Class MainWindow
    'Dim iUsrcEnntree As New usercAgentsEntree(AddressOf LoadEntree)
    Dim iUserC As New UserControl
    Public LocalConnxStr As String
    Private isDarkTheme As Boolean = True
    Private isLeftPanelCollapsed As Boolean
    Private isRightPanelCollapsed As Boolean
    Private ReadOnly userControlLoadingTimer As New DispatcherTimer With {
        .Interval = TimeSpan.FromMilliseconds(500)
    }
    Private userControlLoadingPending As Boolean
    Private loadingOperationCount As Integer

    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        AddHandler userControlLoadingTimer.Tick, AddressOf UserControlLoadingTimer_Tick

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
    End Sub

    Private Sub btnToggleLeftPanel_Click(sender As Object, e As RoutedEventArgs)
        isLeftPanelCollapsed = Not isLeftPanelCollapsed
        If isLeftPanelCollapsed Then
            colLeftPanel.MinWidth = 42
            colLeftPanel.Width = New GridLength(42)
            bdrColonneGauche.Padding = New Thickness(5, 0, 5, 0)
            stpLeftPanelContent.Visibility = Visibility.Collapsed
            tbkLeftPanelCollapsedTitle.Visibility = Visibility.Visible
            btnToggleLeftPanel.Content = "▶"
            btnToggleLeftPanel.ToolTip = "Développer le panneau"
        Else
            colLeftPanel.MinWidth = 165
            colLeftPanel.Width = New GridLength(190)
            bdrColonneGauche.Padding = New Thickness(8, 10, 8, 10)
            stpLeftPanelContent.Visibility = Visibility.Visible
            tbkLeftPanelCollapsedTitle.Visibility = Visibility.Collapsed
            btnToggleLeftPanel.Content = "◀"
            btnToggleLeftPanel.ToolTip = "Réduire le panneau"
        End If
    End Sub

    Private Sub btnToggleRightPanel_Click(sender As Object, e As RoutedEventArgs)
        isRightPanelCollapsed = Not isRightPanelCollapsed
        If isRightPanelCollapsed Then
            colRightPanel.MinWidth = 42
            colRightPanel.Width = New GridLength(42)
            bdrColonneDroit.Padding = New Thickness(5, 0, 5, 0)
            stpRightPanelContent.Visibility = Visibility.Collapsed
            tbkRightPanelCollapsedTitle.Visibility = Visibility.Visible
            btnToggleRightPanel.Content = "▶"
            btnToggleRightPanel.ToolTip = "Développer le panneau"
        Else
            colRightPanel.MinWidth = 210
            colRightPanel.Width = New GridLength(245)
            bdrColonneDroit.Padding = New Thickness(10)
            stpRightPanelContent.Visibility = Visibility.Visible
            tbkRightPanelCollapsedTitle.Visibility = Visibility.Collapsed
            btnToggleRightPanel.Content = "◀"
            btnToggleRightPanel.ToolTip = "Réduire le panneau"
        End If
    End Sub

    Private Sub bdrColonneCentre_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs)
        Dim dataGrid = FindAncestorDataGrid(TryCast(e.OriginalSource, DependencyObject))
        If dataGrid Is Nothing Then Return

        Dim gridScrollViewer = FindVisualScrollViewer(dataGrid)
        If gridScrollViewer IsNot Nothing AndAlso CanScrollVertically(gridScrollViewer, e.Delta) Then Return

        Dim ancestor = GetParentElement(dataGrid)
        While ancestor IsNot Nothing
            Dim scrollViewer = TryCast(ancestor, ScrollViewer)
            If scrollViewer IsNot Nothing AndAlso CanScrollVertically(scrollViewer, e.Delta) Then
                Dim lineCount = Math.Max(1, CInt(Math.Round(Math.Abs(e.Delta) / 120.0 * 3)))
                For index = 1 To lineCount
                    If e.Delta < 0 Then
                        scrollViewer.LineDown()
                    Else
                        scrollViewer.LineUp()
                    End If
                Next
                e.Handled = True
                Return
            End If
            ancestor = GetParentElement(ancestor)
        End While
    End Sub

    Private Function FindAncestorDataGrid(element As DependencyObject) As DataGrid
        Dim current = element
        While current IsNot Nothing
            Dim dataGrid = TryCast(current, DataGrid)
            If dataGrid IsNot Nothing Then Return dataGrid
            current = GetParentElement(current)
        End While
        Return Nothing
    End Function

    Private Function FindVisualScrollViewer(root As DependencyObject) As ScrollViewer
        For index = 0 To VisualTreeHelper.GetChildrenCount(root) - 1
            Dim child = VisualTreeHelper.GetChild(root, index)
            Dim scrollViewer = TryCast(child, ScrollViewer)
            If scrollViewer IsNot Nothing Then Return scrollViewer

            scrollViewer = FindVisualScrollViewer(child)
            If scrollViewer IsNot Nothing Then Return scrollViewer
        Next
        Return Nothing
    End Function

    Private Function GetParentElement(element As DependencyObject) As DependencyObject
        Dim contentElement = TryCast(element, ContentElement)
        If contentElement IsNot Nothing Then
            Dim parent = ContentOperations.GetParent(contentElement)
            If parent IsNot Nothing Then Return parent
            Return LogicalTreeHelper.GetParent(element)
        End If
        Return VisualTreeHelper.GetParent(element)
    End Function

    Private Function CanScrollVertically(scrollViewer As ScrollViewer, delta As Integer) As Boolean
        If delta < 0 Then Return scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight
        If delta > 0 Then Return scrollViewer.VerticalOffset > 0
        Return False
    End Function

    Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Await RunWithLoadingAsync(
        Sub()
            tbxVersion.Text = "V" & My.Application.Info.Version.ToString
            tbxUserName.Text = $"@{UserName} #{UserLevel}_{ProfilName}"
            DbPathGeneral = PaphBackUp()
            If Not LocalModeBb AndAlso Not maj() Then DbPath = DbPathGeneral
            LocalConnxStr = $"Data Source={DbPath};Version=3;"
            InitializeGlobal()
        End Sub,
        "Ouverture de la fenêtre principale...")
        'If Not SessLogin() Then Me.Close()
    End Sub

    Private Async Function RunWithLoadingAsync(loadAction As Action,
                                               Optional loadingMessage As String = "Chargement des données...",
                                               Optional minimumDisplayMilliseconds As Integer = 0) As Task
        BeginLoadingOperation(loadingMessage)
        Me.Cursor = Cursors.Wait
        Try
            Await Dispatcher.Yield(DispatcherPriority.Background)
            If minimumDisplayMilliseconds > 0 Then
                Await System.Threading.Tasks.Task.Delay(minimumDisplayMilliseconds)
            End If
            loadAction()
        Finally
            EndLoadingOperation()
            Me.Cursor = Cursors.Arrow
        End Try
    End Function

    Friend Sub NotifyUserControlLoading()
        userControlLoadingPending = True
        userControlLoadingTimer.Stop()
        userControlLoadingTimer.Start()
        ShowLoadingOverlay("Chargement des données...")
    End Sub

    Friend Sub BeginDatabaseLoading()
        Dim beginOnUiThread As Action =
            Sub()
                Dim wasVisible = grdLoadingOverlay.Visibility = Visibility.Visible
                BeginLoadingOperation("Chargement des données...")
                If Not wasVisible AndAlso IsLoaded AndAlso IsVisible Then
                    Dispatcher.Invoke(
                        New Action(
                            Sub()
                            End Sub),
                        DispatcherPriority.Background)
                End If
            End Sub

        If Dispatcher.CheckAccess() Then
            beginOnUiThread()
        Else
            Dispatcher.Invoke(beginOnUiThread)
        End If
    End Sub

    Friend Sub EndDatabaseLoading()
        If Dispatcher.CheckAccess() Then
            EndLoadingOperation()
        Else
            Dispatcher.Invoke(Sub() EndLoadingOperation())
        End If
    End Sub

    Private Sub BeginLoadingOperation(message As String)
        loadingOperationCount += 1
        ShowLoadingOverlay(message)
    End Sub

    Private Sub EndLoadingOperation()
        If loadingOperationCount > 0 Then loadingOperationCount -= 1
        HideLoadingOverlayIfIdle()
    End Sub

    Private Sub ShowLoadingOverlay(message As String)
        tbkLoadingMessage.Text = message
        If grdLoadingOverlay.Visibility = Visibility.Visible Then Return

        grdLoadingOverlay.Visibility = Visibility.Visible
        Dim loadingStoryboard = TryCast(grdLoadingOverlay.Resources("LoadingRotationStoryboard"),
                                        System.Windows.Media.Animation.Storyboard)
        loadingStoryboard?.Begin(grdLoadingOverlay, True)
    End Sub

    Private Sub HideLoadingOverlayIfIdle()
        If loadingOperationCount > 0 OrElse userControlLoadingPending Then Return

        Dim loadingStoryboard = TryCast(grdLoadingOverlay.Resources("LoadingRotationStoryboard"),
                                        System.Windows.Media.Animation.Storyboard)
        loadingStoryboard?.Stop(grdLoadingOverlay)
        grdLoadingOverlay.Visibility = Visibility.Collapsed
        tbkLoadingMessage.Text = "Chargement des données..."
    End Sub

    Private Sub UserControlLoadingTimer_Tick(sender As Object, e As EventArgs)
        userControlLoadingTimer.Stop()
        userControlLoadingPending = False
        HideLoadingOverlayIfIdle()
    End Sub

    Private Async Sub btnThemeToggle_Click(sender As Object, e As RoutedEventArgs)
        Dim requestedDarkTheme = Not isDarkTheme
        Await RunWithLoadingAsync(
            Sub()
                isDarkTheme = requestedDarkTheme
                ApplyTheme(isDarkTheme)
            End Sub,
            "Application du thème...",
            250)
    End Sub

    Private Sub ApplyTheme(useDarkTheme As Boolean)
        If useDarkTheme Then
            SetThemeColor("ThemeSurfaceBrush", "#17212F")
            SetThemeColor("ThemeCardBrush", "#222F40")
            SetThemeColor("ThemeContentSurfaceBrush", "#101A28")
            SetThemeColor("ThemeContentCardBrush", "#2A3A4E")
            SetThemeColor("ThemeContentBorderBrush", "#62778D")
            SetThemeColor("ThemeDashboardTitleBrush", "#0B1630")
            SetThemeColor("ThemeDashboardTitleTextBrush", "#FFFFFF")
            SetThemeColor("ThemeSearchPlanningBrush", "#082744")
            SetThemeColor("ThemeSearchAgentBrush", "#0C365E")
            SetThemeColor("ThemeSearchContractBrush", "#124A78")
            SetThemeColor("ThemeSearchLabelBrush", "#FFFFFF")
            SetThemeColor("ThemeSearchTitleBrush", "#123A5A")
            SetThemeColor("ThemeInkBrush", "#F1F5F9")
            SetThemeColor("ThemeInputBrush", "#26384B")
            SetThemeColor("ThemeInputTextBrush", "#F8FAFC")
            SetThemeColor("ThemeInputPlaceholderBrush", "#AAB8C8")
            SetThemeColor("ThemeInputDisabledBrush", "#34465A")
            SetThemeColor("ThemeInputDisabledTextBrush", "#D2DAE4")
            SetThemeColor("ThemeComboItemHoverBrush", "#34536F")
            SetThemeColor("ThemeComboItemSelectedBrush", "#1676A5")
            SetThemeColor("ThemeComboItemSelectedTextBrush", "#FFFFFF")
            SetThemeColor("ThemeCalendarInactiveTextBrush", "#93A4B7")
            SetThemeColor("ThemeCalendarTodayBorderBrush", "#4DD0E1")
            SetThemeColor("ThemeTextSelectionBrush", "#2679A8")
            SetThemeColor("ThemeHeadingBrush", "#FFFFFF")
            SetThemeColor("ThemeNavyBrush", "#0B1630")
            SetThemeColor("ThemeHeaderBrush", "#35556A")
            SetThemeColor("ThemeSectionHeaderBrush", "#0B1630")
            SetThemeColor("ThemeSectionHeaderTextBrush", "#FFFFFF")
            SetThemeColor("ThemeSectionHeaderAccentBrush", "#A7C9FF")
            SetThemeColor("ThemeTableHeaderBrush", "#0B1630")
            SetThemeColor("ThemeTableHeaderTextBrush", "#FFFFFF")
            SetThemeColor("ThemeSuccessButtonBrush", "#18794E")
            SetThemeColor("ThemeSuccessButtonTextBrush", "#FFFFFF")
            SetThemeColor("ThemeActionSuccessBrush", "#22C55E")
            SetThemeColor("ThemeActionSuccessTextBrush", "#052E16")
            SetThemeColor("ThemeActionDangerBrush", "#B42332")
            SetThemeColor("ThemeActionDangerTextBrush", "#FFF1F2")
            SetThemeColor("ThemeActionWarningBrush", "#FBBF24")
            SetThemeColor("ThemeActionWarningTextBrush", "#422006")
            SetThemeColor("ThemeActionPositiveBrush", "#16A34A")
            SetThemeColor("ThemeActionPositiveTextBrush", "#052E16")
            SetThemeColor("ThemeActionBrightSuccessBrush", "#A3E635")
            SetThemeColor("ThemeActionBrightSuccessTextBrush", "#1A2E05")
            SetThemeColor("ThemeActionPointBrush", "#8B5CF6")
            SetThemeColor("ThemeActionPointTextBrush", "#FFFFFF")
            SetThemeColor("ThemeActionNeutralTextBrush", "#E2E8F0")
            SetThemeColor("ThemeLoginButtonBrush", "#00B8A9")
            SetThemeColor("ThemeLoginButtonTextBrush", "#062A30")
            SetThemeColor("ThemeAgentButtonBrush", "#1C6664")
            SetThemeColor("ThemeAgentButtonTextBrush", "#FFFFFF")
            SetThemeColor("ThemeAgentSelectedBrush", "#14756E")
            SetThemeColor("ThemeAgentSelectedTextBrush", "#FFFFFF")
            SetThemeColor("ThemeAgentHoverBrush", "#23847C")
            SetThemeColor("ThemeAgentGroupBrush", "#123337")
            SetThemeColor("ThemeAgentGroupBorderBrush", "#26615D")
            SetThemeColor("ThemeClientButtonBrush", "#315F9A")
            SetThemeColor("ThemeClientButtonTextBrush", "#FFFFFF")
            SetThemeColor("ThemeClientSelectedBrush", "#416FAE")
            SetThemeColor("ThemeClientSelectedTextBrush", "#FFFFFF")
            SetThemeColor("ThemeClientHoverBrush", "#477FC0")
            SetThemeColor("ThemeClientGroupBrush", "#172A45")
            SetThemeColor("ThemeClientGroupBorderBrush", "#365D8B")
            SetThemeColor("ThemeCongesButtonBrush", "#F2C14E")
            SetThemeColor("ThemeCongesButtonTextBrush", "#392A00")
            SetThemeColor("ThemeCongesSelectedBrush", "#D99000")
            SetThemeColor("ThemeCongesSelectedTextBrush", "#241700")
            SetThemeColor("ThemeCongesHoverBrush", "#FFE08A")
            SetThemeColor("ThemeAgentGroupLabelBrush", "#75E4CE")
            SetThemeColor("ThemeClientGroupLabelBrush", "#A7C9FF")
            SetThemeColor("ThemeAccentBrush", "#00CFCB")
            SetThemeColor("ThemeAccentSoftBrush", "#29434B")
            SetThemeColor("ThemeBorderBrush", "#60758A")
            SetThemeColor("ThemeRowBrush", "#26364A")
            SetThemeColor("ThemeAlternateRowBrush", "#35485E")
            SetThemeColor("ThemeTablePositiveRowBrush", "#174A3A")
            SetThemeColor("ThemeTablePositiveRowTextBrush", "#D1FAE5")
            SetThemeColor("ThemeTableWarningRowBrush", "#665315")
            SetThemeColor("ThemeTableWarningRowTextBrush", "#FFF2B3")
            SetThemeColor("ThemeTableDangerRowBrush", "#641F2D")
            SetThemeColor("ThemeTableDangerRowTextBrush", "#FFD7DE")
            SetThemeColor("ThemeTableAlertTextBrush", "#FF9AA9")
            SetThemeColor("ThemePendingRowBrush", "#244866")
            SetThemeColor("ThemePendingAlternateRowBrush", "#19364F")
            SetThemeColor("ThemePendingRowTextBrush", "#E8F4FF")
            SetThemeColor("ThemeSpecialTableBrush", "#1D2B47")
            SetThemeColor("ThemeHeaderTextBrush", "#FFFFFF")
            SetThemeColor("ThemeReminderHeaderBrush", "#5A3038")
            SetThemeColor("ThemeReminderHeaderTextBrush", "#FBEAEC")
            SetThemeColor("ThemeReminderSurfaceBrush", "#302329")
            SetThemeColor("ThemeReminderTextBrush", "#F3E8EA")
            SetThemeColor("ThemeReminderBorderBrush", "#80535C")
            btnThemeToggle.Header = "Mode clair"
        Else
            SetThemeColor("ThemeSurfaceBrush", "#E7EEF2")
            SetThemeColor("ThemeCardBrush", "#FFFFFF")
            SetThemeColor("ThemeContentSurfaceBrush", "#D7E2EA")
            SetThemeColor("ThemeContentCardBrush", "#FFFFFF")
            SetThemeColor("ThemeContentBorderBrush", "#8EA4B3")
            SetThemeColor("ThemeDashboardTitleBrush", "#D7E7F4")
            SetThemeColor("ThemeDashboardTitleTextBrush", "#0B2440")
            SetThemeColor("ThemeSearchPlanningBrush", "#DCEAF5")
            SetThemeColor("ThemeSearchAgentBrush", "#D3E5F4")
            SetThemeColor("ThemeSearchContractBrush", "#C7DFF0")
            SetThemeColor("ThemeSearchLabelBrush", "#102A43")
            SetThemeColor("ThemeSearchTitleBrush", "#B4D2E8")
            SetThemeColor("ThemeInkBrush", "#172B3F")
            SetThemeColor("ThemeInputBrush", "#FFFFFF")
            SetThemeColor("ThemeInputTextBrush", "#172B3F")
            SetThemeColor("ThemeInputPlaceholderBrush", "#60758A")
            SetThemeColor("ThemeInputDisabledBrush", "#E1E8EE")
            SetThemeColor("ThemeInputDisabledTextBrush", "#526779")
            SetThemeColor("ThemeComboItemHoverBrush", "#EAF3FA")
            SetThemeColor("ThemeComboItemSelectedBrush", "#D1E6F4")
            SetThemeColor("ThemeComboItemSelectedTextBrush", "#102A43")
            SetThemeColor("ThemeCalendarInactiveTextBrush", "#8593A0")
            SetThemeColor("ThemeCalendarTodayBorderBrush", "#2679A8")
            SetThemeColor("ThemeTextSelectionBrush", "#2679A8")
            SetThemeColor("ThemeHeadingBrush", "#061A2F")
            SetThemeColor("ThemeNavyBrush", "#00113E")
            SetThemeColor("ThemeHeaderBrush", "#426A82")
            SetThemeColor("ThemeSectionHeaderBrush", "#DCEAF5")
            SetThemeColor("ThemeSectionHeaderTextBrush", "#102A43")
            SetThemeColor("ThemeSectionHeaderAccentBrush", "#245B8F")
            SetThemeColor("ThemeTableHeaderBrush", "#C9DDED")
            SetThemeColor("ThemeTableHeaderTextBrush", "#102A43")
            SetThemeColor("ThemeSuccessButtonBrush", "#D2F0DF")
            SetThemeColor("ThemeSuccessButtonTextBrush", "#164B2C")
            SetThemeColor("ThemeActionSuccessBrush", "#004700")
            SetThemeColor("ThemeActionSuccessTextBrush", "#FFFFFF")
            SetThemeColor("ThemeActionDangerBrush", "#700101")
            SetThemeColor("ThemeActionDangerTextBrush", "#FFFFFF")
            SetThemeColor("ThemeActionWarningBrush", "#614F00")
            SetThemeColor("ThemeActionWarningTextBrush", "#FFFFFF")
            SetThemeColor("ThemeActionPositiveBrush", "#0D4700")
            SetThemeColor("ThemeActionPositiveTextBrush", "#FFFFFF")
            SetThemeColor("ThemeActionBrightSuccessBrush", "#3AD600")
            SetThemeColor("ThemeActionBrightSuccessTextBrush", "#000000")
            SetThemeColor("ThemeActionPointBrush", "#FFF3E0FF")
            SetThemeColor("ThemeActionPointTextBrush", "#392A4A")
            SetThemeColor("ThemeActionNeutralTextBrush", "#013924")
            SetThemeColor("ThemeLoginButtonBrush", "#00113E")
            SetThemeColor("ThemeLoginButtonTextBrush", "#FFFFFF")
            SetThemeColor("ThemeAgentButtonBrush", "#D0F0E9")
            SetThemeColor("ThemeAgentButtonTextBrush", "#123B3A")
            SetThemeColor("ThemeAgentSelectedBrush", "#14756E")
            SetThemeColor("ThemeAgentSelectedTextBrush", "#FFFFFF")
            SetThemeColor("ThemeAgentHoverBrush", "#B6E5DC")
            SetThemeColor("ThemeAgentGroupBrush", "#E7F5F1")
            SetThemeColor("ThemeAgentGroupBorderBrush", "#9ACFC4")
            SetThemeColor("ThemeClientButtonBrush", "#D6E6FA")
            SetThemeColor("ThemeClientButtonTextBrush", "#18365C")
            SetThemeColor("ThemeClientSelectedBrush", "#315B91")
            SetThemeColor("ThemeClientSelectedTextBrush", "#FFFFFF")
            SetThemeColor("ThemeClientHoverBrush", "#BDD4F3")
            SetThemeColor("ThemeClientGroupBrush", "#EAF1FB")
            SetThemeColor("ThemeClientGroupBorderBrush", "#A8C3E4")
            SetThemeColor("ThemeCongesButtonBrush", "#FFF0BF")
            SetThemeColor("ThemeCongesButtonTextBrush", "#654500")
            SetThemeColor("ThemeCongesSelectedBrush", "#B7791F")
            SetThemeColor("ThemeCongesSelectedTextBrush", "#241700")
            SetThemeColor("ThemeCongesHoverBrush", "#FFE39A")
            SetThemeColor("ThemeAgentGroupLabelBrush", "#12665E")
            SetThemeColor("ThemeClientGroupLabelBrush", "#244F83")
            SetThemeColor("ThemeAccentBrush", "#00CFCB")
            SetThemeColor("ThemeAccentSoftBrush", "#D1F9F6")
            SetThemeColor("ThemeBorderBrush", "#8DA2B1")
            SetThemeColor("ThemeRowBrush", "#FFFFFF")
            SetThemeColor("ThemeAlternateRowBrush", "#D9E4EC")
            SetThemeColor("ThemeTablePositiveRowBrush", "#FFDEFFE2")
            SetThemeColor("ThemeTablePositiveRowTextBrush", "#1E4620")
            SetThemeColor("ThemeTableWarningRowBrush", "#FFFFFFA6")
            SetThemeColor("ThemeTableWarningRowTextBrush", "#574100")
            SetThemeColor("ThemeTableDangerRowBrush", "#FFFFB4B4")
            SetThemeColor("ThemeTableDangerRowTextBrush", "#7A1E1E")
            SetThemeColor("ThemeTableAlertTextBrush", "#9B1C31")
            SetThemeColor("ThemePendingRowBrush", "#FFA5EAFF")
            SetThemeColor("ThemePendingAlternateRowBrush", "#FFFFFFFF")
            SetThemeColor("ThemePendingRowTextBrush", "#172B3F")
            SetThemeColor("ThemeSpecialTableBrush", "#FF444781")
            SetThemeColor("ThemeHeaderTextBrush", "#FFFFFF")
            SetThemeColor("ThemeReminderHeaderBrush", "#F4DDE0")
            SetThemeColor("ThemeReminderHeaderTextBrush", "#713B43")
            SetThemeColor("ThemeReminderSurfaceBrush", "#FFF7F7")
            SetThemeColor("ThemeReminderTextBrush", "#493238")
            SetThemeColor("ThemeReminderBorderBrush", "#D8AEB4")
            btnThemeToggle.Header = "Mode sombre"
        End If

        TestConnexionDbIcon()
    End Sub

    Private Sub SetThemeColor(resourceKey As String, colorValue As String)
        If Not System.Windows.Application.Current.Resources.Contains(resourceKey) Then
            Throw New InvalidOperationException("Ressource de thème introuvable : " & resourceKey)
        End If
        Dim themeColor = CType(ColorConverter.ConvertFromString(colorValue), Color)
        System.Windows.Application.Current.Resources(resourceKey) = New SolidColorBrush(themeColor)
    End Sub

    Private Sub mitContact_Click(sender As Object, e As RoutedEventArgs)
        MessageBox.Show("Développeur : Julio P. Bag" & Environment.NewLine &
                        "Téléphone : +228 91 17 03 07" & Environment.NewLine &
                        "E-mail : juliobag@live.fr" & Environment.NewLine &
                        "Support : juliobag@hotmail.fr",
                        "Contact", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Private Sub mitAideUtilisation_Click(sender As Object, e As RoutedEventArgs)
        Dim helpWindow As New HelpWindow()
        helpWindow.Owner = Me
        helpWindow.ShowDialog()
    End Sub

    Private Sub mitAPropos_Click(sender As Object, e As RoutedEventArgs)
        MessageBox.Show("LAKEY" & Environment.NewLine &
                        "Version " & My.Application.Info.Version.ToString() & Environment.NewLine &
                        "Développé par Julio P. Bag.",
                        "À propos de LAKEY", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Sub InitializeGlobal()
        TestConnexionDbIcon()
        LoadPrimSettings()
        iUserC = New usercTBord(Sub()
                                    cctrlCentral.Content = iUserC
                                End Sub)
        cctrlCentral.Content = iUserC
        'cctrlCentral.Content = New usercTBord
        Try
            InitStpVisibility()
            SelectPrincipalBtn(btnTBord)
            AfficheStatValue()
            ReDirectStaval()
            RappelsMemos()
            ReDirectRappel()
            AfficheProfilUser()
            AfficheInfosStructure()
            RedirectSociete()
            BtnAgentSearch.Text = ""
            BtnPlanningSearch.Text = ""
            BtnContratSearch.Text = ""
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Try
            Dim iIdber As String = "C:\Program Files\JPBA\idbuder\idbuder.exe"
            StartProcess(iIdber)
        Catch ex As Exception
            MsgBox(ex.Message)
            Me.Close()
        End Try

    End Sub

    Private Async Sub btnTBord_Click(sender As Object, e As RoutedEventArgs)
        Await RunWithLoadingAsync(
            Sub()
                iUserC = New usercTBord(Sub()
                                            cctrlCentral.Content = iUserC
                                        End Sub)
                cctrlCentral.Content = iUserC
                SelectPrincipalBtn(btnTBord)
                SelecStp(stpTBord)
            End Sub)
    End Sub


    Private Async Sub btnAgents_Click(sender As Object, e As RoutedEventArgs)
        Await RunWithLoadingAsync(
            Sub()
                iUserC = New usercAgents(Sub()
                                             cctrlCentral.Content = iUserC
                                         End Sub)
                cctrlCentral.Content = iUserC
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
            End Sub)
    End Sub

    Private Async Sub btnTraitement_Click(sender As Object, e As RoutedEventArgs) Handles btnTraitement.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New usercTraitements
                SelecStp(stpTraitement)
                SelectPrincipalBtn(btnTraitement)
            End Sub)
    End Sub

    Private Async Sub btnClient_Click(sender As Object, e As RoutedEventArgs) Handles btnClient.Click
        Await RunWithLoadingAsync(
            Sub()
                iUserC = New usercClients(Sub()
                                              cctrlCentral.Content = iUserC
                                          End Sub)
                cctrlCentral.Content = iUserC
                SelecStp(stpClient)
                SelectPrincipalBtn(btnClient)
            End Sub)
    End Sub

    Private Async Sub btnStructure_Click(sender As Object, e As RoutedEventArgs) Handles btnStructure.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New usercStructure
                SelecStp(stpStructure)
                SelectPrincipalBtn(btnStructure)
            End Sub)
    End Sub


    Sub AfficheInfosStructure()
        maj()
        'MsgBox("ok")
        Try
            If IO.File.Exists(SocieteLogoPath) Then
                Dim iUri = New Uri((SocieteLogoPath))
                imgLoogo.Source = New BitmapImage(iUri)
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Dim SteName As String = ""
        If SocieteSigle <> "" Then

            SteName = $"{SocieteName} ({SocieteSigle})"
        Else
            SteName = SocieteName
        End If
        tbkSteName.Text = SteName
        Me.Title = $"{My.Application.Info.ProductName} V{My.Application.Info.Version}  ~ {SteName}"
        'MsgBox("ok")

    End Sub
    Sub ReDirectRappel()
        usercAgentsAbsences.AfficheMemos = AddressOf RappelsMemos
        usercAgentsContrat.AfficheMemos = AddressOf RappelsMemos
        usercAgentsSortie.AfficheMemos = AddressOf RappelsMemos
    End Sub
    Sub ReDirectStaval()
        usercAgentsEntree.AfficheStat = AddressOf AfficheStatValue
        usercAgentsContrat.AfficheStat = AddressOf AfficheStatValue
        usercAgentsAffectation.AfficheStat = AddressOf AfficheStatValue
        usercAgentsSortie.AfficheStat = AddressOf AfficheStatValue
        usercClientsContrat.AfficheStat = AddressOf AfficheStatValue
        usercClientsSite.AfficheStat = AddressOf AfficheStatValue
    End Sub
    Sub RedirectSociete()
        usercSociete.AfficheInfosStructure = AddressOf AfficheInfosStructure
    End Sub
    Sub RappelsMemos()
        tbxRappel.Text = ListeAgentFinContrat(LocalConnxStr)
        If tbxRappel.Text <> "" Then
            Dim iText As String = tbxRappel.Text
            Dim irappel As String = RappelActifs(LocalConnxStr)
            Dim CongeRapel As String = ListePermissionCongeEnAttente("permis", LocalConnxStr)
            If CongeRapel <> "" Then irappel += Environment.NewLine & Environment.NewLine & "=== PERMISSIONS en attente ===" & CongeRapel
            CongeRapel = ListePermissionCongeEnAttente("cong", LocalConnxStr)
            If CongeRapel <> "" Then irappel += Environment.NewLine & Environment.NewLine & "=== CONGES en attente ===" & CongeRapel
            If irappel <> "" Then
                iText += Environment.NewLine & Environment.NewLine & irappel
                Try
                    ShowTempMsg(irappel, 60, "RAPPELS IMPORTANTS", "r")

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try
            End If
            tbxRappel.Text = iText
        End If
    End Sub
    Sub AfficheStatValue()
        tbxAgentStat.Text = StatAgent(LocalConnxStr)
        tbxClientStat.Text = StatClient(LocalConnxStr)
    End Sub



    Private Async Sub btnFonction_Click(sender As Object, e As RoutedEventArgs) Handles btnFonction.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New usercFonction
                SelectStructure(btnFonction)
            End Sub)
    End Sub
    Sub SelecStp(MyStp As StackPanel)
        InitStpVisibility()
        MyStp.Visibility = Visibility.Visible
    End Sub
    Sub InitStpVisibility()
        stpClient.Visibility = Visibility.Collapsed
        stpTraitement.Visibility = Visibility.Collapsed
        stpStructure.Visibility = Visibility.Collapsed
        stpSupplement.Visibility = Visibility.Collapsed
        stpEmployes.Visibility = Visibility.Collapsed
        stpSetting.Visibility = Visibility.Collapsed
    End Sub
    Sub InitLoadStpVisibility()
        stpClient.Visibility = Visibility.Visible
        stpTraitement.Visibility = Visibility.Visible
        stpStructure.Visibility = Visibility.Visible
        stpSupplement.Visibility = Visibility.Visible
        stpEmployes.Visibility = Visibility.Visible
        stpSetting.Visibility = Visibility.Collapsed
    End Sub

    Sub initPrincipalBtn()
        btnTraitement.Height = 30
        btnAgents.Height = 30
        btnClient.Height = 30
        btnStructure.Height = 30
        btnSupplement.Height = 30
        btnSeting.Height = 30
        btnTBord.Height = 30

        btnTraitement.IsEnabled = True
        btnAgents.IsEnabled = True
        btnClient.IsEnabled = True
        btnStructure.IsEnabled = True
        btnSupplement.IsEnabled = True
        btnSeting.IsEnabled = True
        btnTBord.IsEnabled = True

        btnTraitement.FontWeight = FontWeights.Normal
        btnAgents.FontWeight = FontWeights.Normal
        btnClient.FontWeight = FontWeights.Normal
        btnStructure.FontWeight = FontWeights.Normal
        btnSupplement.FontWeight = FontWeights.Normal
        btnSeting.FontWeight = FontWeights.Normal
        btnTBord.FontWeight = FontWeights.Normal

        btnTraitement.ClearValue(Button.BackgroundProperty)
        btnAgents.ClearValue(Button.BackgroundProperty)
        btnClient.ClearValue(Button.BackgroundProperty)
        btnStructure.ClearValue(Button.BackgroundProperty)
        btnSupplement.ClearValue(Button.BackgroundProperty)
        btnSeting.ClearValue(Button.BackgroundProperty)
        btnTBord.ClearValue(Button.BackgroundProperty)

    End Sub

    Sub initRacourcisBtn()
        'initRacourcisClientBtn()
        btnRacAbsenceAg.IsEnabled = True
        btnRacAffectationAg.IsEnabled = True
        btnRacContratAg.IsEnabled = True
        btnRacEntreeAg.IsEnabled = True
        btnRacHeureSupAg.IsEnabled = True
        btnRacPlanningAg.IsEnabled = True
        btnRacSortieAg.IsEnabled = True
        btnRacCalCong.IsEnabled = True

    End Sub
    Sub initRacourcisClientBtn()
        'initRacourcisBtn()
        btnRacEtatCl.IsEnabled = True
        btnRacContratCl.IsEnabled = True
        btnRacSiteCl.IsEnabled = True
        btnRacPosteCl.IsEnabled = True
        btnRacAgentListeCl.IsEnabled = True

    End Sub

    Sub SelectSetingBtn(myBtn As Button)
        initSetingsBtn()
        myBtn.Height = 40
        myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF58B1FF"))
        myBtn.FontWeight = FontWeights.Bold
    End Sub
    Sub SelectRacoucourcilBtn(myBtn As Button)
        TestConnexionDbIcon()
        bdrColonneCentre.ScrollToTop()
        initialzTousBtnRcourcis()
        myBtn.Height = 25
        'myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF58B1FF"))
        myBtn.FontWeight = FontWeights.Bold
        myBtn.IsEnabled = False
    End Sub
    Sub initialzTousBtnRcourcis()
        initRacourcisBtn()
        initRacourcisClientBtn()
    End Sub
    Sub SelectRacoucourciClientBtn(myBtn As Button)
        TestConnexionDbIcon()
        bdrColonneCentre.ScrollToTop()
        initRacourcisClientBtn()
        initRacourcisBtn()
        myBtn.Height = 25
        'myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF58B1FF"))
        myBtn.FontWeight = FontWeights.Bold
        myBtn.IsEnabled = False
    End Sub

    Sub SelectPrincipalBtn(myBtn As Button)
        initialzTousBtnRcourcis()
        TestConnexionDbIcon()
        initPrincipalBtn()
        bdrColonneCentre.ScrollToTop()
        initAgentBtn()
        initClientBtn()
        initSetingsBtn()
        initStructureBtn()
        initSupplementBtn()
        initTraitementBtn()

        'myBtn.IsEnabled = False

        myBtn.Height = 35
        myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF58B1FF"))
        myBtn.FontWeight = FontWeights.Bold
    End Sub

    Private Async Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New usercExterneActs
                SelecStp(stpSupplement)
                SelectPrincipalBtn(btnSupplement)
            End Sub)
    End Sub

    Private Async Sub btnSeting_Click(sender As Object, e As RoutedEventArgs) Handles btnSeting.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New usercSettings
                SelectPrincipalBtn(btnSeting)
                SelecStp(stpSetting)
            End Sub)
    End Sub
    Sub initSetingsBtn()
        btnGeneral.Height = 20
        btnUser.Height = 20
        btnDataSets.Height = 20
        btnProfilUser.Height = 20

        btnGeneral.FontWeight = FontWeights.Normal
        btnUser.FontWeight = FontWeights.Normal
        btnDataSets.FontWeight = FontWeights.Normal
        btnProfilUser.FontWeight = FontWeights.Normal

        btnGeneral.ClearValue(Button.BackgroundProperty)
        btnUser.ClearValue(Button.BackgroundProperty)
        btnDataSets.ClearValue(Button.BackgroundProperty)
        btnProfilUser.ClearValue(Button.BackgroundProperty)

        btnGeneral.Foreground = Brushes.Black
        btnUser.Foreground = Brushes.Black
        btnDataSets.Foreground = Brushes.Black
        btnProfilUser.Foreground = Brushes.Black
        'btnSortie.Foreground = Brushes.Black
    End Sub

    Sub initAgentBtn()
        TestConnexionDbIcon()
        btnEntree.Height = 20
        btnContrat.Height = 20
        btnAffectation.Height = 20
        btnPlanning.Height = 20
        btnSortie.FontWeight = FontWeights.Normal
        btnEntree.FontWeight = FontWeights.Normal
        btnContrat.FontWeight = FontWeights.Normal
        btnAffectation.FontWeight = FontWeights.Normal
        btnPlanning.FontWeight = FontWeights.Normal
        'btnSortie.FontWeight = FontWeights.Normal

        btnEntree.ClearValue(Button.BackgroundProperty)
        btnContrat.ClearValue(Button.BackgroundProperty)
        btnAffectation.ClearValue(Button.BackgroundProperty)
        btnPlanning.ClearValue(Button.BackgroundProperty)
        btnSortie.ClearValue(Button.BackgroundProperty)

        btnEntree.Foreground = Brushes.Black
        btnContrat.Foreground = Brushes.Black
        btnAffectation.Foreground = Brushes.Black
        btnPlanning.Foreground = Brushes.Black
        btnSortie.Foreground = Brushes.Black
    End Sub
    Sub initTraitementBtn()

        btnAbsence.Height = 20
        btnHeureSupp.Height = 20
        btnConges.Height = 20
        btnEvenement.Height = 20

        btnAbsence.FontWeight = FontWeights.Normal
        btnHeureSupp.FontWeight = FontWeights.Normal
        btnConges.FontWeight = FontWeights.Normal
        btnEvenement.FontWeight = FontWeights.Normal
        'btnPlanning.FontWeight = FontWeights.Normal
        'btnSortie.FontWeight = FontWeights.Normal

        btnAbsence.ClearValue(Button.BackgroundProperty)
        btnHeureSupp.ClearValue(Button.BackgroundProperty)
        btnConges.ClearValue(Button.BackgroundProperty)
        btnEvenement.ClearValue(Button.BackgroundProperty)
        'btnSortie.ClearValue(Button.BackgroundProperty)

        btnAbsence.Foreground = Brushes.Black
        btnHeureSupp.Foreground = Brushes.Black
        btnConges.Foreground = Brushes.Black
        btnEvenement.Foreground = Brushes.Black
        'btnSortie.Foreground = Brushes.Black
    End Sub
    Sub initClientBtn()

        btnClientEtat.Height = 20
        btnSite.Height = 20
        btnPoste.Height = 20
        btnClientContrat.Height = 20

        btnClientEtat.FontWeight = FontWeights.Normal
        btnSite.FontWeight = FontWeights.Normal
        btnPoste.FontWeight = FontWeights.Normal
        btnClientContrat.FontWeight = FontWeights.Normal
        'btnPlanning.FontWeight = FontWeights.Normal
        'btnSortie.FontWeight = FontWeights.Normal

        btnClientEtat.ClearValue(Button.BackgroundProperty)
        btnSite.ClearValue(Button.BackgroundProperty)
        btnPoste.ClearValue(Button.BackgroundProperty)
        btnClientContrat.ClearValue(Button.BackgroundProperty)
        'btnSortie.ClearValue(Button.BackgroundProperty)

        btnClientEtat.Foreground = Brushes.Black
        btnSite.Foreground = Brushes.Black
        btnPoste.Foreground = Brushes.Black
        btnClientContrat.Foreground = Brushes.Black
        'btnSortie.Foreground = Brushes.Black
    End Sub
    Sub initSupplementBtn()

        btnCompta.Height = 20
        btnCnss.Height = 20
        btnRappel.Height = 20
        btnMemo.Height = 20
        btnEvenement.Height = 20

        btnCompta.FontWeight = FontWeights.Normal
        btnCnss.FontWeight = FontWeights.Normal
        btnRappel.FontWeight = FontWeights.Normal
        btnMemo.FontWeight = FontWeights.Normal
        btnEvenement.FontWeight = FontWeights.Normal

        btnCompta.ClearValue(Button.BackgroundProperty)
        btnCnss.ClearValue(Button.BackgroundProperty)
        btnRappel.ClearValue(Button.BackgroundProperty)
        btnMemo.ClearValue(Button.BackgroundProperty)
        btnEvenement.ClearValue(Button.BackgroundProperty)

        btnCompta.Foreground = Brushes.Black
        btnCnss.Foreground = Brushes.Black
        btnRappel.Foreground = Brushes.Black
        btnMemo.Foreground = Brushes.Black
        btnEvenement.Foreground = Brushes.Black
    End Sub
    Sub initStructureBtn()

        btnFonction.Height = 20
        btnSociety.Height = 20
        btnAgency.Height = 20

        btnFonction.FontWeight = FontWeights.Normal
        btnSociety.FontWeight = FontWeights.Normal
        btnAgency.FontWeight = FontWeights.Normal

        btnFonction.ClearValue(Button.BackgroundProperty)
        btnSociety.ClearValue(Button.BackgroundProperty)
        btnAgency.ClearValue(Button.BackgroundProperty)

        btnFonction.Foreground = Brushes.Black
        btnSociety.Foreground = Brushes.Black
        btnAgency.Foreground = Brushes.Black

    End Sub

    Sub SelectAgentBtn(myBtn As Button)
        initAgentBtn()
        bdrColonneCentre.ScrollToTop()
        initRacourcisBtn()
        initRacourcisClientBtn()
        myBtn.Height = 30
        myBtn.FontWeight = FontWeights.Bold
        myBtn.Foreground = Brushes.White
        myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00549C"))
    End Sub
    Sub SelectTraitementBtn(myBtn As Button)
        initRacourcisBtn()
        initRacourcisClientBtn()
        initTraitementBtn()
        myBtn.Height = 30
        myBtn.FontWeight = FontWeights.Bold
        myBtn.Foreground = Brushes.White
        myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00549C"))
    End Sub
    Sub SelectClientBtn(myBtn As Button)
        initialzTousBtnRcourcis()
        initClientBtn()
        myBtn.Height = 30
        myBtn.FontWeight = FontWeights.Bold
        myBtn.Foreground = Brushes.White
        myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00549C"))
    End Sub
    Sub SelectSupplement(myBtn As Button)
        initialzTousBtnRcourcis()
        initSupplementBtn()
        myBtn.Height = 30
        myBtn.FontWeight = FontWeights.Bold
        myBtn.Foreground = Brushes.White
        myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00549C"))
    End Sub
    Sub SelectStructure(myBtn As Button)
        initialzTousBtnRcourcis()
        initStructureBtn()
        myBtn.Height = 30
        myBtn.FontWeight = FontWeights.Bold
        myBtn.Foreground = Brushes.White
        myBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF00549C"))
    End Sub

    Private Async Sub btnEntree_Click(sender As Object, e As RoutedEventArgs) Handles btnEntree.Click
        Await RunWithLoadingAsync(Sub() LoadEntree())
    End Sub
    Sub LoadEntree()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnEntree)
        iUserC = New usercAgentsEntree(Sub()
                                           cctrlCentral.Content = iUserC
                                       End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub
    Private Async Sub btnContrat_Click(sender As Object, e As RoutedEventArgs) Handles btnContrat.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectAgentBtn(btnContrat)
                LoadContrat()
                cctrlCentral.Content = iUserC
            End Sub)
    End Sub
    Sub LoadContrat()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnContrat)
        iUserC = New usercAgentsContrat(Sub()
                                            cctrlCentral.Content = iUserC
                                        End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub


    Private Async Sub btnAffectation_Click(sender As Object, e As RoutedEventArgs) Handles btnAffectation.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectAgentBtn(btnAffectation)
                LoadAffectation()
            End Sub)
    End Sub
    Sub LoadAffectation()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnAffectation)
        iUserC = New usercAgentsAffectation(Sub()
                                                cctrlCentral.Content = iUserC
                                            End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub


    Private Async Sub btnPlanning_Click(sender As Object, e As RoutedEventArgs) Handles btnPlanning.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectAgentBtn(btnPlanning)
                LoadPlanning()
            End Sub)
    End Sub
    Sub LoadPlanning()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnPlanning)
        iUserC = New usercAgentsPlanning(Sub()
                                             cctrlCentral.Content = iUserC
                                         End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub


    Private Async Sub btnSortie_Click(sender As Object, e As RoutedEventArgs) Handles btnSortie.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectAgentBtn(btnSortie)
                LoadSortie()
            End Sub)
    End Sub
    Sub LoadSortie()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnSortie)
        iUserC = New usercAgentsSortie(Sub()
                                           cctrlCentral.Content = iUserC
                                       End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub

    Private Async Sub btnAbsence_Click(sender As Object, e As RoutedEventArgs) Handles btnAbsence.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectTraitementBtn(btnAbsence)
                LoadAbsence()
            End Sub)
    End Sub
    Sub LoadAbsence()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnAbsence)
        iUserC = New usercAgentsAbsences(Sub()
                                             cctrlCentral.Content = iUserC
                                         End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub

    Private Async Sub btnHeureSupp_Click(sender As Object, e As RoutedEventArgs) Handles btnHeureSupp.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectTraitementBtn(btnHeureSupp)
                LoadHeureS()
            End Sub)
    End Sub
    Sub LoadHeureS()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnHeureSupp)
        iUserC = New usercAgentsHeureSupp(Sub()
                                              cctrlCentral.Content = iUserC
                                          End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub


    Private Async Sub btnConges_Click(sender As Object, e As RoutedEventArgs) Handles btnConges.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectTraitementBtn(btnConges)
                cctrlCentral.Content = New usercAgentsConges
            End Sub)
    End Sub

    Private Sub btnEvenement_Click(sender As Object, e As RoutedEventArgs)
        SelectTraitementBtn(btnEvenement)
    End Sub

    Private Async Sub btnClientEtat_Click(sender As Object, e As RoutedEventArgs) Handles btnClientEtat.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New usercClientsEtat
                SelectClientBtn(btnClientEtat)
            End Sub)
    End Sub

    Private Async Sub btnSite_Click(sender As Object, e As RoutedEventArgs) Handles btnSite.Click
        Await RunWithLoadingAsync(
            Sub()
                LoadSite()
                SelectClientBtn(btnSite)
            End Sub)
    End Sub
    Sub LoadSite()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnEntree)
        iUserC = New usercClientsSite(Sub()
                                          cctrlCentral.Content = iUserC
                                      End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub
    Sub LoadPoste()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnEntree)
        iUserC = New usercClientsPoste(Sub()
                                           cctrlCentral.Content = iUserC
                                       End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub

    Private Async Sub btnPoste_Click(sender As Object, e As RoutedEventArgs) Handles btnPoste.Click
        Await RunWithLoadingAsync(
            Sub()
                LoadPoste()
                SelectClientBtn(btnPoste)
            End Sub)
    End Sub

    Private Async Sub btnClientContrat_Click(sender As Object, e As RoutedEventArgs) Handles btnClientContrat.Click
        Await RunWithLoadingAsync(
            Sub()
                LoadClientContrat()
                SelectClientBtn(btnClientContrat)
            End Sub)
    End Sub
    Sub LoadClientContrat()
        elpsLoading.Visibility = Visibility.Visible
        SelectAgentBtn(btnEntree)
        iUserC = New usercClientsContrat(Sub()
                                             cctrlCentral.Content = iUserC
                                         End Sub)
        cctrlCentral.Content = iUserC
        elpsLoading.Visibility = Visibility.Collapsed
    End Sub
    Sub LoadSociete()
        iUserC = New usercSociete(Sub()
                                      cctrlCentral.Content = iUserC
                                  End Sub)

        cctrlCentral.Content = iUserC

    End Sub

    Private Async Sub btnSociety_Click(sender As Object, e As RoutedEventArgs) Handles btnSociety.Click
        Await RunWithLoadingAsync(
            Sub()
                LoadSociete()
                SelectStructure(btnSociety)
            End Sub)
    End Sub

    Private Async Sub btnAgency_Click(sender As Object, e As RoutedEventArgs) Handles btnAgency.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectStructure(btnAgency)
                cctrlCentral.Content = New usercAngence
            End Sub)
    End Sub

    Private Async Sub btnCompta_Click(sender As Object, e As RoutedEventArgs) Handles btnCompta.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectSupplement(btnCompta)
                cctrlCentral.Content = New usercNBanque
            End Sub)
    End Sub

    Private Async Sub btnCnss_Click(sender As Object, e As RoutedEventArgs) Handles btnCnss.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectSupplement(btnCnss)
                cctrlCentral.Content = New usercNCNSS
            End Sub)
    End Sub

    Private Async Sub btnRappel_Click(sender As Object, e As RoutedEventArgs) Handles btnRappel.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectSupplement(btnRappel)
                cctrlCentral.Content = New usercRappel
            End Sub)
    End Sub

    Private Async Sub btnMemo_Click(sender As Object, e As RoutedEventArgs) Handles btnMemo.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectSupplement(btnMemo)
                cctrlCentral.Content = New usercMemo
            End Sub)
    End Sub

    Private Async Sub btnEvenement_Click_1(sender As Object, e As RoutedEventArgs) Handles btnEvenement.Click
        Await RunWithLoadingAsync(
            Sub()
                SelectSupplement(btnEvenement)
                cctrlCentral.Content = New usercEvenement
            End Sub)
    End Sub

    Private Async Sub btnGeneral_Click(sender As Object, e As RoutedEventArgs) Handles btnGeneral.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New GeneralSets
                SelectSetingBtn(btnGeneral)
            End Sub)
    End Sub

    Private Async Sub btnUser_Click(sender As Object, e As RoutedEventArgs) Handles btnUser.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New UserManager
                SelectSetingBtn(btnUser)
            End Sub)
    End Sub

    Private Sub btnCalCong_Click(sender As Object, e As RoutedEventArgs) Handles btnCalCong.Click
        Dim iCalCon As New wdwConges
        iCalCon.WindowStartupLocation = WindowStartupLocation.CenterScreen
        iCalCon.Show()

    End Sub

    Private Sub tbxRappel_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxRappel.TextChanged
        If tbxRappel.Text = "" Then
            wrpRappel.Visibility = Visibility.Collapsed
        Else
            wrpRappel.Visibility = Visibility.Visible
        End If
    End Sub

    Private Async Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        If MsgBox("Réinitialiser toutes les données de planning?" & Environment.NewLine & "CONTINUER ?", MsgBoxStyle.YesNo, "ACTUALISATION DES DONNEES") <> MsgBoxResult.Yes Then Return

        btnRefresh.IsEnabled = False
        Try
            Await RunWithLoadingAsync(Sub() InitializeGlobal(), "Actualisation des données...")
        Finally
            btnRefresh.IsEnabled = True
        End Try
    End Sub
    Sub ResetData()
        If tbxRappel.Text <> "" Then
            Dim irappel As String = RappelActifs()
            If irappel <> "" Then
                ShowTempMsg(irappel, 60, "RAPPELS")
            End If
        End If

    End Sub

    Private Async Sub btnRacEntreeAg_Click(sender As Object, e As RoutedEventArgs) Handles btnRacEntreeAg.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
                LoadEntree()
                SelectRacoucourcilBtn(btnRacEntreeAg)
            End Sub)
    End Sub

    Private Async Sub btnRacContratAg_Click(sender As Object, e As RoutedEventArgs) Handles btnRacContratAg.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
                SelectAgentBtn(btnContrat)
                LoadContrat()
                SelectRacoucourcilBtn(btnRacContratAg)
            End Sub)
    End Sub

    Private Async Sub btnRacAffectationAg_Click(sender As Object, e As RoutedEventArgs) Handles btnRacAffectationAg.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
                SelectAgentBtn(btnAffectation)
                LoadAffectation()
                SelectRacoucourcilBtn(btnRacAffectationAg)
            End Sub)
    End Sub

    Private Async Sub btnRacPlanningAg_Click(sender As Object, e As RoutedEventArgs) Handles btnRacPlanningAg.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
                SelectAgentBtn(btnPlanning)
                LoadPlanning()
                SelectRacoucourcilBtn(btnRacPlanningAg)
            End Sub)
    End Sub

    Private Async Sub btnRacAbsenceAg_Click(sender As Object, e As RoutedEventArgs) Handles btnRacAbsenceAg.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpTraitement)
                SelectPrincipalBtn(btnTraitement)
                SelectTraitementBtn(btnAbsence)
                LoadAbsence()
                SelectRacoucourcilBtn(btnRacAbsenceAg)
            End Sub)
    End Sub

    Private Async Sub btnRacSortieAg_Click(sender As Object, e As RoutedEventArgs) Handles btnRacSortieAg.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
                SelectAgentBtn(btnSortie)
                LoadSortie()
                SelectRacoucourcilBtn(btnRacSortieAg)
            End Sub)
    End Sub

    Private Async Sub btnRacHeureSupAg_Click(sender As Object, e As RoutedEventArgs) Handles btnRacHeureSupAg.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpTraitement)
                SelectPrincipalBtn(btnTraitement)
                SelectTraitementBtn(btnHeureSupp)
                LoadHeureS()
                SelectRacoucourcilBtn(btnRacHeureSupAg)
            End Sub)
    End Sub

    Private Sub btnRacCalCong_Click(sender As Object, e As RoutedEventArgs) Handles btnRacCalCong.Click
        Dim iCalCon As New wdwCongeGlobal
        iCalCon.WindowStartupLocation = WindowStartupLocation.CenterScreen
        iCalCon.Show()
        'SelectRacoucourcilBtn(btnRacCalCong)
    End Sub

    Private Async Sub btnRacPosteCl_Click(sender As Object, e As RoutedEventArgs) Handles btnRacPosteCl.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpClient)
                SelectPrincipalBtn(btnClient)
                LoadPoste()
                SelectClientBtn(btnPoste)
                SelectRacoucourciClientBtn(btnRacPosteCl)
            End Sub)
    End Sub

    Private Async Sub btnRacSiteCl_Click(sender As Object, e As RoutedEventArgs) Handles btnRacSiteCl.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpClient)
                SelectPrincipalBtn(btnClient)
                LoadSite()
                SelectClientBtn(btnSite)
                SelectRacoucourciClientBtn(btnRacSiteCl)
            End Sub)
    End Sub

    Private Async Sub btnRacContratCl_Click(sender As Object, e As RoutedEventArgs) Handles btnRacContratCl.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpClient)
                SelectPrincipalBtn(btnClient)
                LoadClientContrat()
                SelectClientBtn(btnClientContrat)
                SelectRacoucourciClientBtn(btnRacContratCl)
            End Sub)
    End Sub

    Private Async Sub btnRacEtatCl_Click(sender As Object, e As RoutedEventArgs) Handles btnRacEtatCl.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpClient)
                SelectPrincipalBtn(btnClient)
                iUserC = New usercClients(Sub()
                                              cctrlCentral.Content = iUserC
                                          End Sub)
                cctrlCentral.Content = iUserC
                SelectClientBtn(btnClientEtat)
                SelectRacoucourciClientBtn(btnRacEtatCl)
            End Sub)
    End Sub

    Private Async Sub btnRacAgentListeCl_Click(sender As Object, e As RoutedEventArgs) Handles btnRacAgentListeCl.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpClient)
                SelectPrincipalBtn(btnClient)
                cctrlCentral.Content = New usercClientsEtat
                SelectClientBtn(btnClientEtat)
                SelectRacoucourciClientBtn(btnRacAgentListeCl)
            End Sub)
    End Sub

    Private Sub BtnAgentSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentSearch.KeyDown
        If e.Key = Key.Enter Then
            BtnAgentSearch.SelectAll()
            maj()
            dgAgent.ItemsSource = LoadDataTable("SELECT a.AgentAffect,a.SiteAffect,ag.Contacts,ag.NumMatr from affectation a join agents ag on ag.NomPrenoms=a.AgentAffect where a.FinAffect>=date() and (a.AgentAffect like '%" & Apost(BtnAgentSearch.Text) & "%' or a.SiteAffect like '%" & Apost(BtnAgentSearch.Text) & "%' )order by a.SiteAffect, a.AgentAffect", LocalConnxStr).DefaultView
            lblAgentSearch.Content = "Agents N=" & dgAgent.Items.Count
        End If
    End Sub

    Private Sub BtnContratSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnContratSearch.KeyDown
        If e.Key = Key.Enter Then
            BtnContratSearch.SelectAll()
            maj()
            dgContrat.ItemsSource = LoadDataTable("SELECT ag.NomPrenoms,date(c.DebutContr),date(c.FinContr),n.IdNature from contratstbl c join agents ag on ag.NumMatr=c.NomPrenoms join contratsnature n on n.NatureContr=c.NaturContr where c.FinContr>=date() and (ag.NomPrenoms like '%" & Apost(BtnContratSearch.Text) & "%' or n.IdNature like '%" & Apost(BtnContratSearch.Text) & "%' )order by ag.NomPrenoms,c.DebutContr desc", LocalConnxStr).DefaultView
            lblContratSearch.Content = "Contrat N=" & dgContrat.Items.Count
        End If

    End Sub

    Private Sub BtnPlanningSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnPlanningSearch.KeyDown
        If e.Key = Key.Enter Then
            BtnPlanningSearch.SelectAll()
            maj()
            dgPlanning.ItemsSource = LoadDataTable("SELECT p.Agent, po.Designation from planposte p join postes po on p.Poste=po.Code where p.DatePlan=date() and (p.Agent like '%" & Apost(BtnPlanningSearch.Text) & "%' or po.Designation like '%" & Apost(BtnPlanningSearch.Text) & "%' ) order by p.Agent,po.Designation", LocalConnxStr).DefaultView
            lblPlanningSearch.Content = "En poste N=" & dgPlanning.Items.Count
        End If

    End Sub

    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs) Handles cmiResume.Click
        Dim rowView As DataRowView = TryCast(dgAgent.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAgentActif(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then ShowTempMsg(iWrd,, "RESUME - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiContrat_Click(sender As Object, e As RoutedEventArgs) Handles cmiContrat.Click
        Dim rowView As DataRowView = TryCast(dgAgent.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeCtr(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then ShowTempMsg(iWrd,, "CONTRAT - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiAffectation_Click(sender As Object, e As RoutedEventArgs) Handles cmiAffectation.Click
        Dim rowView As DataRowView = TryCast(dgAgent.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAffect(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then ShowTempMsg(iWrd,, "AFFFECTATIONS - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiAbsence_Click(sender As Object, e As RoutedEventArgs) Handles cmiAbsence.Click
        Dim rowView As DataRowView = TryCast(dgAgent.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAbs(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then ShowTempMsg(iWrd,, "ABSENCES/PERMISSIONS/CONGES - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiHeureSup_Click(sender As Object, e As RoutedEventArgs) Handles cmiHeureSup.Click
        Dim rowView As DataRowView = TryCast(dgAgent.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeHS(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then ShowTempMsg(iWrd,, "HEURES SUPPLEMENTAIRES - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiResumeC_Click(sender As Object, e As RoutedEventArgs) Handles cmiResumeC.Click
        Dim rowView As DataRowView = TryCast(dgContrat.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeCtr(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then ShowTempMsg(iWrd,, "CONTRATS - " & UCase(rowView.Row.ItemArray(0).ToString)) 'MsgBox(iWrd, MsgBoxStyle.OkOnly, "CONTRATS - " & UCase(rowView.Row.ItemArray(0).ToString))
            'ShowTempMsg(iWrd,, "CONTRATS - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiThisW_Click(sender As Object, e As RoutedEventArgs) Handles cmiThisW.Click
        Dim rowView As DataRowView = TryCast(dgPlanning.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            'cmiPoste.Header = rowView.Row.ItemArray(1).ToString
            Dim iWrd As String = ResumePlanning(rowView.Row.ItemArray(0).ToString, DebutDeSemaine(Today), finDeSemaine(Today))
            If iWrd <> "" Then ShowTempMsg(iWrd,, "CETTE SEMAINE => " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiNextW_Click(sender As Object, e As RoutedEventArgs) Handles cmiNextW.Click
        Dim rowView As DataRowView = TryCast(dgPlanning.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            'cmiPoste.Header = rowView.Row.ItemArray(1).ToString
            'MsgBox((Today.AddDays(7)))
            Dim iWrd As String = ResumePlanning(rowView.Row.ItemArray(0).ToString, DebutDeSemaine(Today.AddDays(7)), finDeSemaine(Today).AddDays(7))
            If iWrd <> "" Then ShowTempMsg(iWrd,, "SEMAINE PROCHAINE => " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiThisWp_Click(sender As Object, e As RoutedEventArgs) Handles cmiThisWp.Click
        Dim rowView As DataRowView = TryCast(dgPlanning.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumePlanningPoste(rowView.Row.ItemArray(1).ToString, DebutDeSemaine(Today), finDeSemaine(Today))
            If iWrd <> "" Then ShowTempMsg(iWrd,, "CETTE SEMAINE => " & UCase(rowView.Row.ItemArray(1).ToString))
        End If
    End Sub

    Private Sub cmiNextWp_Click(sender As Object, e As RoutedEventArgs) Handles cmiNextWp.Click
        Dim rowView As DataRowView = TryCast(dgPlanning.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumePlanningPoste(rowView.Row.ItemArray(1).ToString, DebutDeSemaine(Today.AddDays(7)), finDeSemaine(Today.AddDays(7)))
            If iWrd <> "" Then ShowTempMsg(iWrd,, "SEMAINE PROCHAINE => " & UCase(rowView.Row.ItemArray(1).ToString))
        End If
    End Sub

    Sub TestConnexionDbIcon()
        mitModeConnecte.IsChecked = Not LocalModeBb
        mitModeLocal.IsChecked = LocalModeBb
        If LocalModeBb Then
            imgHorsConnex.Visibility = Visibility.Visible
            tbModeConnexion.Visibility = Visibility.Visible
            btnReconnecter.Visibility = Visibility.Visible
            bdrTitreCentre.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF6C0000"))
        Else
            imgHorsConnex.Visibility = Visibility.Collapsed
            tbModeConnexion.Visibility = Visibility.Collapsed
            btnReconnecter.Visibility = Visibility.Collapsed
            bdrTitreCentre.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF426A82"))
        End If

    End Sub

    Private Async Sub btnReconnecter_Click(sender As Object, e As RoutedEventArgs)
        Await SwitchToConnectedModeFromUIAsync()
    End Sub

    Private Async Sub mitModeConnecte_Click(sender As Object, e As RoutedEventArgs)
        Await SwitchToConnectedModeFromUIAsync()
    End Sub

    Private Async Function SwitchToConnectedModeFromUIAsync() As Task
        If Not LocalModeBb OrElse Not btnReconnecter.IsEnabled Then Return
        If MessageBox.Show(
            "La base locale sera sauvegardée et conservée, mais ses modifications ne seront pas fusionnées dans la base centrale." &
            Environment.NewLine & "L'application passera ensuite en mode connecté et rechargera les données de la centrale." &
            Environment.NewLine & Environment.NewLine &
            "Continuer ?",
            "Passer en mode connecté",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question) <> MessageBoxResult.Yes Then
            TestConnexionDbIcon()
            Return
        End If

        btnReconnecter.IsEnabled = False
        mitModeConnecte.IsEnabled = False
        mitModeLocal.IsEnabled = False
        Try
            Await RunWithLoadingAsync(AddressOf SwitchToConnectedMode,
                                      "Sauvegarde locale et connexion à la base centrale...")
        Catch ex As Exception
            MessageBox.Show("Le passage en mode connecté a échoué. Vérifiez que la session locale est toujours active." &
                            Environment.NewLine & ex.Message,
                            "Erreur de bascule", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            btnReconnecter.IsEnabled = LocalModeBb
            mitModeConnecte.IsEnabled = True
            mitModeLocal.IsEnabled = True
            TestConnexionDbIcon()
        End Try
    End Function

    Private Async Sub mitModeLocal_Click(sender As Object, e As RoutedEventArgs)
        If LocalModeBb OrElse Not mitModeLocal.IsEnabled Then Return
        If MessageBox.Show(
            "La copie de travail sera sauvegardée puis utilisée en mode local. Les modifications faites en mode local ne seront pas fusionnées dans la base centrale." &
            Environment.NewLine & Environment.NewLine & "Continuer ?",
            "Passer en mode local",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question) <> MessageBoxResult.Yes Then
            TestConnexionDbIcon()
            Return
        End If

        btnReconnecter.IsEnabled = False
        mitModeConnecte.IsEnabled = False
        mitModeLocal.IsEnabled = False
        Try
            Await RunWithLoadingAsync(AddressOf SwitchToLocalMode,
                                      "Sauvegarde et passage en mode local...")
        Catch ex As Exception
            MessageBox.Show("Le passage en mode local a échoué." & Environment.NewLine & ex.Message,
                            "Erreur de bascule", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            btnReconnecter.IsEnabled = LocalModeBb
            mitModeConnecte.IsEnabled = True
            mitModeLocal.IsEnabled = True
            TestConnexionDbIcon()
        End Try
    End Sub

    Private Sub SwitchToLocalMode()
        Dim connectedDatabasePath As String = DbPath
        Dim localConnectionString As String = LocalConnxStr
        Dim connectedConnectionString As String =
            If(String.IsNullOrWhiteSpace(SessionConnxStr),
               "Data Source=" & DbPathGeneral & ";Version=3;",
               SessionConnxStr)
        Dim connectedUserName As String = UserName
        Dim connectedPassword As String = UserPW
        Dim connectedUserLevel As Integer = UserLevel
        Dim connectedUserPath As String = connectedDatabasePath
        Dim centralDatabasePath As String = DbPathGeneral

        If String.IsNullOrWhiteSpace(localConnectionString) Then
            localConnectionString = "Data Source=" & connectedDatabasePath & ";Version=3;"
        End If
        If String.Equals(IO.Path.GetFullPath(connectedDatabasePath),
                         IO.Path.GetFullPath(centralDatabasePath),
                         StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("La copie de travail désigne la base centrale ; le mode local ne peut pas être activé.")
        End If

        Dim localDatabaseError As String = Nothing
        If Not IsSQLiteDatabaseHealthy(connectedDatabasePath, localDatabaseError) Then
            Throw New InvalidOperationException("La copie de travail locale n'est pas exploitable : " & localDatabaseError)
        End If

        Dim backupDirectory As String = IO.Path.Combine(UserPath, "Backups")
        IO.Directory.CreateDirectory(backupDirectory)
        Dim backupPath As String = IO.Path.Combine(
            backupDirectory,
            "ManA-avant-mode-local-" & DateTime.Now.ToString("yyyyMMdd-HHmmss") & "-" &
            Guid.NewGuid().ToString("N") & ".sqlite")
        CreateValidatedSQLiteSnapshot(connectedDatabasePath, backupPath)

        Dim localUserLevel As Integer
        Try
            localUserLevel = Login.UserTest(connectedUserName, connectedPassword, localConnectionString)
        Finally
            UserName = connectedUserName
            UserPW = connectedPassword
            UserLevel = connectedUserLevel
        End Try
        If localUserLevel = 1976 Then
            Throw New InvalidOperationException("Cet utilisateur n'est pas reconnu dans la copie locale.")
        End If
        If Not LoadUserProfil(localUserLevel, localConnectionString) Then
            LoadUserProfil(connectedUserLevel, connectedConnectionString)
            Throw New InvalidOperationException("Le profil utilisateur n'est pas disponible dans la copie locale.")
        End If

        If Not SessLogout(connectedConnectionString) Then
            LoadUserProfil(connectedUserLevel, connectedConnectionString)
            Throw New InvalidOperationException("La session connectée n'a pas pu être clôturée.")
        End If

        Try
            LocalModeBb = True
            UserLevel = localUserLevel
            DbPath = connectedDatabasePath
            LocalConnxStr = localConnectionString
            DbManipulation.iConnectStr = localConnectionString
            If Not SessLogin(localConnectionString) Then
                Throw New InvalidOperationException("La session locale n'a pas pu être ouverte.")
            End If
            SessionConnxStr = localConnectionString
            tbxUserName.Text = $"@{UserName} #{UserLevel}_{ProfilName}"
            InitializeGlobal()
        Catch ex As Exception
            Dim rollbackIssues As New Collections.Generic.List(Of String)
            If LocalModeBb AndAlso Not String.IsNullOrWhiteSpace(SessionConnxStr) AndAlso
               Not String.Equals(SessionConnxStr, connectedConnectionString, StringComparison.OrdinalIgnoreCase) Then
                If Not SessLogout(SessionConnxStr) Then
                    rollbackIssues.Add("La session locale n'a pas pu être clôturée.")
                End If
            End If

            LocalModeBb = False
            UserName = connectedUserName
            UserPW = connectedPassword
            UserLevel = connectedUserLevel
            DbPath = connectedUserPath
            LocalConnxStr = "Data Source=" & connectedDatabasePath & ";Version=3;"
            DbManipulation.iConnectStr = LocalConnxStr
            SessionConnxStr = connectedConnectionString
            If Not LoadUserProfil(connectedUserLevel, connectedConnectionString) Then
                rollbackIssues.Add("Le profil connecté n'a pas pu être restauré.")
            End If

            If Not SessLogin(connectedConnectionString) Then
                rollbackIssues.Add("La session connectée n'a pas pu être réouverte.")
            Else
                SessionConnxStr = connectedConnectionString
                tbxUserName.Text = $"@{UserName} #{UserLevel}_{ProfilName}"
                Try
                    InitializeGlobal()
                Catch refreshException As Exception
                    rollbackIssues.Add("L'écran n'a pas pu être actualisé après restauration : " & refreshException.Message)
                End Try
            End If

            Dim rollbackMessage As String =
                "La bascule a échoué ; le mode connecté a été rétabli. " & ex.Message
            If rollbackIssues.Count > 0 Then
                rollbackMessage &= Environment.NewLine & String.Join(Environment.NewLine, rollbackIssues)
            End If
            Throw New InvalidOperationException(rollbackMessage, ex)
        End Try
    End Sub

    Private Sub SwitchToConnectedMode()
        Dim originalDatabasePath As String = DbPath
        Dim originalLocalConnectionString As String = LocalConnxStr
        Dim originalSessionConnectionString As String =
            If(String.IsNullOrWhiteSpace(SessionConnxStr), LocalConnxStr, SessionConnxStr)
        Dim originalSessionId As Integer = IdSession
        Dim originalUserName As String = UserName
        Dim originalPassword As String = UserPW
        Dim originalUserLevel As Integer = UserLevel
        Dim centralDatabasePath As String = ReadConfiguredGeneralDatabasePath()
        DbPathGeneral = centralDatabasePath
        Dim centralConnectionString As String = "Data Source=" & centralDatabasePath & ";Version=3;"
        Dim connectedDatabasePath As String = IO.Path.Combine(UserPath, "ManA.sqlite")
        Dim localBackupPath As String = Nothing
        Dim localSessionClosed As Boolean = False
        Dim connectedSessionOpened As Boolean = False
        Dim centralUserLevel As Integer
        Dim connectedUserName As String = originalUserName
        Dim connectedPassword As String = originalPassword

        If String.Equals(IO.Path.GetFullPath(originalDatabasePath),
                         IO.Path.GetFullPath(centralDatabasePath),
                         StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("La base locale et la base centrale désignent le même fichier ; la bascule est annulée.")
        End If
        If String.Equals(IO.Path.GetFullPath(connectedDatabasePath),
                         IO.Path.GetFullPath(centralDatabasePath),
                         StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Le dossier utilisateur désigne la base centrale ; la bascule est annulée.")
        End If

        Dim centralDatabaseError As String = Nothing
        If Not IsSQLiteDatabaseHealthy(centralDatabasePath, centralDatabaseError) Then
            Throw New InvalidOperationException("La base centrale n'est pas disponible : " & centralDatabaseError)
        End If

        Try
            centralUserLevel = Login.UserTest(UserName, UserPW, centralConnectionString)
            connectedUserName = UserName
            connectedPassword = UserPW
        Finally
            UserName = originalUserName
            UserPW = originalPassword
            UserLevel = originalUserLevel
        End Try
        If centralUserLevel = 1976 Then
            Throw New InvalidOperationException("Cet utilisateur n'est pas reconnu dans la base centrale.")
        End If

        Dim localDatabaseError As String = Nothing
        If Not IsSQLiteDatabaseHealthy(originalDatabasePath, localDatabaseError) Then
            Throw New InvalidOperationException("La base locale ne peut pas être sauvegardée en toute sécurité : " & localDatabaseError)
        End If

        Dim backupDirectory As String = IO.Path.Combine(UserPath, "Backups")
        IO.Directory.CreateDirectory(backupDirectory)
        localBackupPath = IO.Path.Combine(
            backupDirectory,
            "ManA-local-" & DateTime.Now.ToString("yyyyMMdd-HHmmss") & "-" &
            Guid.NewGuid().ToString("N") & ".sqlite")
        CreateValidatedSQLiteSnapshot(originalDatabasePath, localBackupPath)

        If Not LoadUserProfil(centralUserLevel, centralConnectionString) Then
            LoadUserProfil(originalUserLevel, originalLocalConnectionString)
            Throw New InvalidOperationException("Le profil de cet utilisateur n'est pas disponible dans la base centrale.")
        End If

        If Not SessLogout(originalSessionConnectionString) Then
            LoadUserProfil(originalUserLevel, originalLocalConnectionString)
            Throw New InvalidOperationException("La session locale n'a pas pu être clôturée ; elle reste active.")
        End If
        localSessionClosed = True

        Try
            LocalModeBb = False
            UserName = connectedUserName
            UserPW = connectedPassword
            UserLevel = centralUserLevel
            DbPath = connectedDatabasePath
            LocalConnxStr = "Data Source=" & DbPath & ";Version=3;"
            DbManipulation.iConnectStr = LocalConnxStr

            If Not SessLogin(centralConnectionString) Then
                Throw New InvalidOperationException("La session connectée n'a pas pu être ouverte.")
            End If
            connectedSessionOpened = True
            SessionConnxStr = centralConnectionString
            tbxUserName.Text = $"@{UserName} #{UserLevel}_{ProfilName}"

            InitializeGlobal()
        Catch ex As Exception
            Dim rollbackIssues As New Collections.Generic.List(Of String)
            If connectedSessionOpened AndAlso Not SessLogout(centralConnectionString) Then
                rollbackIssues.Add("La session centrale n'a pas pu être clôturée.")
            End If
            Dim sameLocalAndConnectedPath As Boolean =
                String.Equals(IO.Path.GetFullPath(originalDatabasePath),
                              IO.Path.GetFullPath(connectedDatabasePath),
                              StringComparison.OrdinalIgnoreCase)
            Dim rollbackDatabasePath As String = originalDatabasePath
            If sameLocalAndConnectedPath AndAlso Not String.IsNullOrWhiteSpace(localBackupPath) Then
                Try
                    CreateValidatedSQLiteSnapshot(localBackupPath, originalDatabasePath)
                Catch restoreException As Exception
                    rollbackDatabasePath = localBackupPath
                    rollbackIssues.Add("La base locale a été conservée à l'emplacement de sauvegarde : " & restoreException.Message)
                End Try
            End If

            LocalModeBb = True
            UserName = originalUserName
            UserPW = originalPassword
            UserLevel = originalUserLevel
            DbPath = rollbackDatabasePath
            LocalConnxStr = If(String.Equals(rollbackDatabasePath, originalDatabasePath, StringComparison.OrdinalIgnoreCase),
                               originalLocalConnectionString,
                               "Data Source=" & rollbackDatabasePath & ";Version=3;")
            DbManipulation.iConnectStr = LocalConnxStr
            SessionConnxStr = If(String.Equals(rollbackDatabasePath, originalDatabasePath, StringComparison.OrdinalIgnoreCase),
                                 originalSessionConnectionString,
                                 LocalConnxStr)
            If Not LoadUserProfil(originalUserLevel, LocalConnxStr) Then
                rollbackIssues.Add("Le profil local n'a pas pu être rechargé.")
            End If

            If localSessionClosed Then
                If sameLocalAndConnectedPath Then
                    IdSession = originalSessionId
                    If Not SessLogout(SessionConnxStr) Then
                        rollbackIssues.Add("L'ancienne session locale restaurée n'a pas pu être clôturée.")
                    End If
                End If
                If Not SessLogin(SessionConnxStr) Then
                    rollbackIssues.Add("La session locale n'a pas pu être réouverte.")
                End If
            End If

            Dim rollbackMessage As String =
                "La connexion centrale a échoué ; la session locale a été rétablie. " & ex.Message
            If rollbackIssues.Count > 0 Then
                rollbackMessage &= Environment.NewLine & String.Join(Environment.NewLine, rollbackIssues)
            End If
            Throw New InvalidOperationException(
                rollbackMessage,
                ex)
        End Try
    End Sub
    Sub AfficheProfilUser()
        btnGeneral.Visibility = Visibility.Visible
        mitPgeneraux.Visibility = Visibility.Visible
        btnSeting.Visibility = Visibility.Visible
        btnUser.Visibility = Visibility.Visible
        btnProfilUser.Visibility = Visibility.Visible
        mitUtilisateur.Visibility = Visibility.Visible
        btnStructure.Visibility = Visibility.Visible
        mitSociete.Visibility = Visibility.Visible
        btnCompta.Visibility = Visibility.Visible
        btnCnss.Visibility = Visibility.Visible
        btnAgents.Visibility = Visibility.Visible
        btnTraitement.Visibility = Visibility.Visible
        btnClient.Visibility = Visibility.Visible
        btnSupplement.Visibility = Visibility.Visible
        btnTBord.Visibility = Visibility.Visible
        xpdAgentRaccourcis.Visibility = Visibility.Visible
        xpdClientRaccourcis.Visibility = Visibility.Visible
        mitParametres.Visibility = Visibility.Visible
        mitMouvement.Visibility = Visibility.Visible
        mitStructure.Visibility = Visibility.Visible
        xpdPlusOp.Visibility = Visibility.Visible

        If Not CanGlobalDataManage Then
            btnGeneral.Visibility = Visibility.Collapsed
            mitPgeneraux.Visibility = Visibility.Collapsed
            btnSeting.Visibility = Visibility.Collapsed
        End If
        If Not CanUserManage Then
            btnUser.Visibility = Visibility.Collapsed
            btnProfilUser.Visibility = Visibility.Collapsed
            mitUtilisateur.Visibility = Visibility.Collapsed
        End If
        If Not CanSocieteDataManage Then
            btnStructure.Visibility = Visibility.Collapsed
            mitSociete.Visibility = Visibility.Collapsed
        End If
        If Not CanAddDataSpecial Then
            btnCompta.Visibility = Visibility.Collapsed
            btnCnss.Visibility = Visibility.Collapsed
        End If
        If UserLevel = 99 Then
            btnAgents.Visibility = Visibility.Collapsed
            btnTraitement.Visibility = Visibility.Collapsed
            btnClient.Visibility = Visibility.Collapsed
            btnSupplement.Visibility = Visibility.Collapsed
            btnStructure.Visibility = Visibility.Collapsed
            btnSeting.Visibility = Visibility.Collapsed
            btnTBord.Visibility = Visibility.Collapsed

            xpdAgentRaccourcis.Visibility = Visibility.Collapsed
            xpdClientRaccourcis.Visibility = Visibility.Collapsed

            mitParametres.Visibility = Visibility.Collapsed
            mitMouvement.Visibility = Visibility.Collapsed
            mitStructure.Visibility = Visibility.Collapsed
            xpdPlusOp.Visibility = Visibility.Collapsed
        End If

    End Sub

    Private Async Sub btnProfilUser_Click(sender As Object, e As RoutedEventArgs) Handles btnProfilUser.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New ProfilUser
                SelectSetingBtn(btnProfilUser)
            End Sub)
    End Sub

    Private Sub mitDConexion_Click(sender As Object, e As RoutedEventArgs) Handles mitDConexion.Click
        oDeconect()
    End Sub
    Sub oDeconect()
        Dim iwDecnx As New wdwDconnexion
        Dim Resultat As Boolean? = iwDecnx.ShowDialog()
        If Resultat.HasValue AndAlso Resultat = True Then
        ElseIf Resultat.HasValue AndAlso Resultat = False Then
            Me.Close()
        End If

    End Sub
    Private Sub mitQuit_Click(sender As Object, e As RoutedEventArgs) Handles mitQuit.Click
        Me.Close()
    End Sub
    Protected Overrides Sub OnClosing(e As ComponentModel.CancelEventArgs)
        MyBase.OnClosing(e)
        If e.Cancel Then Return

        If MessageBox.Show("Voulez-vous vraiment fermer l'application ?",
                           "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) <> MessageBoxResult.Yes Then
            e.Cancel = True
            Return
        End If

        Me.DialogResult = True
        StopProcess("idbuder")
    End Sub

    Private Async Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.X AndAlso Keyboard.Modifiers = (ModifierKeys.Shift Or ModifierKeys.Control) Then
            oDeconect()
        End If
        If e.Key = Key.Q AndAlso Keyboard.Modifiers = (ModifierKeys.Shift Or ModifierKeys.Control) Then
            Me.Close()
        End If
        If e.Key = Key.A AndAlso Keyboard.Modifiers = (ModifierKeys.Shift Or ModifierKeys.Control) Then
            Await RunWithLoadingAsync(Sub() InitializeGlobal())
        End If
    End Sub

    Private Async Sub mitEntree_Click(sender As Object, e As RoutedEventArgs) Handles mitEntree.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
                LoadEntree()
                SelectRacoucourcilBtn(btnRacEntreeAg)
            End Sub)
    End Sub

    Private Async Sub mitAffectation_Click(sender As Object, e As RoutedEventArgs) Handles mitAffectation.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
                SelectAgentBtn(btnAffectation)
                LoadAffectation()
                SelectRacoucourcilBtn(btnRacAffectationAg)
            End Sub)
    End Sub

    Private Async Sub mitSortie_Click(sender As Object, e As RoutedEventArgs) Handles mitSortie.Click
        Await RunWithLoadingAsync(
            Sub()
                SelecStp(stpEmployes)
                SelectPrincipalBtn(btnAgents)
                SelectAgentBtn(btnSortie)
                LoadSortie()
                SelectRacoucourcilBtn(btnRacSortieAg)
            End Sub)
    End Sub

    Private Async Sub mitFonction_Click(sender As Object, e As RoutedEventArgs) Handles mitFonction.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New usercFonction
                SelectStructure(btnFonction)
            End Sub)
    End Sub

    Private Async Sub mitSociete_Click(sender As Object, e As RoutedEventArgs) Handles mitSociete.Click
        Await RunWithLoadingAsync(
            Sub()
                LoadSociete()
                SelectStructure(btnSociety)
            End Sub)
    End Sub

    Private Async Sub mitPgeneraux_Click(sender As Object, e As RoutedEventArgs) Handles mitPgeneraux.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New GeneralSets
                SelectSetingBtn(btnGeneral)
            End Sub)
    End Sub

    Private Async Sub mitUtilisateur_Click(sender As Object, e As RoutedEventArgs) Handles mitUtilisateur.Click
        Await RunWithLoadingAsync(
            Sub()
                cctrlCentral.Content = New UserManager
                SelectSetingBtn(btnUser)
            End Sub)
    End Sub
End Class
