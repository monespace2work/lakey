Public Class wdwEndClient
    Dim iConnxStr As String
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        LoadData()
    End Sub
    Sub LoadData()
        FilmyCbx("select c.NomClient,c.Actif from client c where c.Actif=1 order by c.NomClient", cbxClent, iConnxStr)
        'cbxClent.SelectedItem=usercClients.nom
        dtpDateFin.SelectedDate = FinDuMois(Today)
        cbxClent.SelectedIndex = -1
        maj()
        ActivBtnValid()
    End Sub
    Private Sub btnValid_Click(sender As Object, e As RoutedEventArgs) Handles btnValid.Click
        If MsgBox("Déactiver le client #" & cbxClent.SelectedItem, MsgBoxStyle.YesNo) = MsgBoxResult.No Then Exit Sub
        If SaveNewData("update client set Actif=0 , DateFinCollabor='" & dtpDateFin.SelectedDate.Value.ToString("yyyy-MM-dd") & "' where NomClient='" & Apost(cbxClent.SelectedItem) & "'", iConnxStr) AndAlso SaveNewData("update siteaffectation set Actif=0 , DateFermeture='" & dtpDateFin.SelectedDate.Value.ToString("yyyy-MM-dd") & "' where Client='" & Apost(cbxClent.SelectedItem) & "' and  Actif=1", iConnxStr) Then
            Joper("FinConctratClient et Site #" & cbxClent.SelectedItem, iConnxStr)
            If SaveNewData("update  postes set Actif=0  where postes.NomSite like'%" & Apost(cbxClent.SelectedItem) & "%' and postes.Actif=1", iConnxStr) Then
                Joper("FinPosteClient chez #" & cbxClent.SelectedItem, iConnxStr)
                MsgBox("Client désactivé avec succès")
                LoadData()
            Else
                MsgBox("Erreur de désactivation")
            End If
        End If
    End Sub
    Sub ActivBtnValid()
        If cbxClent.SelectedIndex >= 0 AndAlso dtpDateFin.SelectedDate.HasValue Then
            btnValid.IsEnabled = True
        Else
            btnValid.IsEnabled = False
        End If
    End Sub

    Private Sub cbxClent_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxClent.SelectionChanged
        ActivBtnValid()
    End Sub

    Private Sub dtpDateFin_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDateFin.SelectedDateChanged
        ActivBtnValid()
    End Sub
End Class
