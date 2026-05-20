Public Class wdwActivOldClient

    Dim iConnxStr As String
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        LoadData()
    End Sub


    Sub LoadData()
        FilmyCbx("select c.NomClient,c.Actif from client c where c.Actif=0 order by c.NomClient", cbxClent, iConnxStr)
        dtpDateActiv.SelectedDate = DebutDuMois(Today)
        cbxClent.SelectedIndex = -1
        maj()
        ActivBtnValid()
    End Sub

    Private Sub btnValid_Click(sender As Object, e As RoutedEventArgs) Handles btnValid.Click
        If MsgBox("Réactiver le client #" & cbxClent.SelectedItem, MsgBoxStyle.YesNo) = MsgBoxResult.No Then Exit Sub
        If SaveNewData("update client set Actif=1 , DateFinCollabor='" & dtpDateActiv.SelectedDate.Value.ToString("yyyy-MM-dd") & "' where NomClient='" & Apost(cbxClent.SelectedItem) & "'", iConnxStr) Then
            Joper("ReactivationConctratClient #" & cbxClent.SelectedItem, iConnxStr)
            LoadData()
            End If

    End Sub
    Sub ActivBtnValid()
        If cbxClent.SelectedIndex >= 0 AndAlso dtpDateActiv.SelectedDate.HasValue Then
            btnValid.IsEnabled = True
        Else
            btnValid.IsEnabled = False
        End If
    End Sub
    Private Sub cbxClent_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxClent.SelectionChanged
        ActivBtnValid()
    End Sub

    Private Sub dtpDateActiv_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDateActiv.SelectedDateChanged
        ActivBtnValid()
    End Sub
End Class
