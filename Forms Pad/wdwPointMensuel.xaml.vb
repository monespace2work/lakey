Public Class wdwPointMensuel
    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = Me.Title
        ExportDGtoExcel(dgPointGlobal, tbkNomFichier.Text, iTitre) '"Point_Entrees"

    End Sub


    Private Sub dgPointGlobal_Loaded(sender As Object, e As RoutedEventArgs) Handles dgPointGlobal.Loaded
        tbkTotal.Text = "Nombre total : " & dgPointGlobal.Items.Count
    End Sub
End Class
