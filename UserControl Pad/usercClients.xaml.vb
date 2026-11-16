Imports System.Data

Public Class usercClients
    Dim retourDansMainWindow As Action
    Public Sub New(cllb As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = cllb
    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)

        If ClientAffiche() And ClientInactifAffiche() Then
            DgClients.ItemsSource = ClientDTable.DefaultView
            DgClientsInactifs.ItemsSource = TempTable.DefaultView
            tbxNbClientInactifs.Text = "Nombre de clients inactifs : " & TempTable.Rows.Count
            tbxNbClientActifs.Text = "Nombre de clients actifs : " & ClientDTable.Rows.Count
            cmiModifClientActif.Visibility = If(CanModifyDataOrdinary, Visibility.Visible, Visibility.Collapsed)
            cmiModifClientInactif.Visibility = If(CanModifyDataOrdinary, Visibility.Visible, Visibility.Collapsed)
            Dim canDelete As Boolean = CanDeleteDataOrdinary Or CanGlobalDataManage
            'cmiSupprClientActif.Visibility = If(canDelete, Visibility.Visible, Visibility.Collapsed)
            'cmiSupprClientInactif.Visibility = If(canDelete, Visibility.Visible, Visibility.Collapsed)
        Else
            MsgBox("erreur de chargement")
        End If

    End Sub

    Private Sub BtnClientSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnClientSearch.KeyDown
        If e.Key = Key.Enter Then
            'MsgBox(ClientDTable.Rows.Count)
            DgClients.ItemsSource = ClientSearchAffiche(BtnClientSearch.Text).DefaultView
            DgClientsInactifs.ItemsSource = ClientInactifSearchTable(BtnClientSearch.Text).DefaultView
        End If
    End Sub

    Private Sub btnExportExcelClientActif_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcelClientActif.Click
        ExportDGtoExcel(DgClients, "Point_Client_Actif", "Clients actifs ") '"Point_Entrees"

    End Sub

    Private Sub btnExportExcelClientInactif_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcelClientInactif.Click
        ExportDGtoExcel(DgClients, "Point_Client_Inactif", "Clients Inactifs ") '"Point_Entrees"
    End Sub

    Private Sub DgClients_PreviewMouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        MainWindow.SelectContextMenuRow(DirectCast(sender, DataGrid), e)
    End Sub

    Private Function ContextMenuDataGrid(sender As Object) As DataGrid
        Dim menuItem As MenuItem = TryCast(sender, MenuItem)
        If menuItem Is Nothing Then Return Nothing

        Dim contextMenu As ContextMenu = TryCast(menuItem.Parent, ContextMenu)
        If contextMenu Is Nothing Then Return Nothing
        Return TryCast(contextMenu.PlacementTarget, DataGrid)
    End Function

    Private Function SelectedClientName(dataGrid As DataGrid) As String
        Dim rowView As DataRowView = TryCast(dataGrid?.CurrentItem, DataRowView)
        If rowView Is Nothing Then Return Nothing

        Dim nameColumn As String = If(rowView.Row.Table.Columns.Contains("Client"), "Client", "NomClient")
        If Not rowView.Row.Table.Columns.Contains(nameColumn) Then Return Nothing
        Return Convert.ToString(rowView(nameColumn))
    End Function

    Private Sub cmiCopier_Click(sender As Object, e As RoutedEventArgs)
        Dim dataGrid As DataGrid = ContextMenuDataGrid(sender)
        If dataGrid IsNot Nothing Then MainWindow.CopyContextMenuRowToClipboard(dataGrid)
    End Sub

    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs)
        ShowClientContextSummary(sender, "INFOS", Function(clientName) ResumeClient(clientName, iConnectStr))
    End Sub

    Private Sub cmiSites_Click(sender As Object, e As RoutedEventArgs)
        ShowClientContextSummary(sender, "SITES", Function(clientName) ListeSiteClient(clientName, iConnectStr))
    End Sub

    Private Sub cmiPostes_Click(sender As Object, e As RoutedEventArgs)
        ShowClientContextSummary(sender, "POSTES", Function(clientName) ListePosteClient(clientName, iConnectStr))
    End Sub

    Private Sub cmiAgents_Click(sender As Object, e As RoutedEventArgs)
        ShowClientContextSummary(sender, "AGENTS AFFECTÉS", Function(clientName) ListeAgentClient(clientName, iConnectStr))
    End Sub

    Private Sub ShowClientContextSummary(sender As Object, title As String, getSummary As Func(Of String, String))
        Dim clientName As String = SelectedClientName(ContextMenuDataGrid(sender))
        If String.IsNullOrWhiteSpace(clientName) Then Return

        Dim summary As String = getSummary(clientName)
        If String.IsNullOrWhiteSpace(summary) Then summary = "Aucun élément affecté."
        MsgBox(summary, MsgBoxStyle.OkOnly, title & " - " & UCase(clientName))
    End Sub

    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs)
        OpenClientContractFromContextMenu(sender, False)
    End Sub

    Private Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs)
        OpenClientContractFromContextMenu(sender, True)
    End Sub

    Private Sub OpenClientContractFromContextMenu(sender As Object, deleteClient As Boolean)
        Dim dataGrid As DataGrid = ContextMenuDataGrid(sender)
        Dim clientName As String = SelectedClientName(dataGrid)
        If String.IsNullOrWhiteSpace(clientName) Then Return

        If deleteClient AndAlso Not (CanDeleteDataOrdinary Or CanGlobalDataManage) Then Return
        If Not deleteClient AndAlso Not CanModifyDataOrdinary Then Return

        Dim mainWindow As MainWindow = TryCast(Window.GetWindow(Me), MainWindow)
        If mainWindow Is Nothing Then
            MessageBox.Show("Impossible d'ouvrir la fiche contrat du client sélectionné.",
                            "Gestion du client", MessageBoxButton.OK, MessageBoxImage.Error)
            Return
        End If

        mainWindow.LoadClientContrat(clientName, deleteClient)
    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("CLIENTS-POINT GLOBAL", "A ce jour " & Today, Me)
        AddHandler iDetailW.Closed, Sub()
                                        retourDansMainWindow?.Invoke()
                                        btnAgrandir.Visibility = Visibility.Visible
                                    End Sub
        iDetailW.WindowStartupLocation = WindowStartupLocation.CenterOwner
        iDetailW.Topmost = True
        Dim iWtf As New WaitFrm
        iWtf.Show()
        iDetailW.Show()
        iWtf.Close()

    End Sub
End Class
