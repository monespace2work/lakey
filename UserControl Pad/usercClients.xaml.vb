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
            tbxNbClientInactifs.Text = "Nombre de clients inactifs : " & NbClientInactif()
            tbxNbClientActifs.Text = "Nombre de clients actifs : " & ClientDTable.Rows.Count
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
