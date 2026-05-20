Public Class usercAngence
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim istr As String = " SELECT * FROM AGENCES ORDER BY NOM "
        'istr += " from siteaffectation s "
        DgAgency.ItemsSource = LoadDataTable(istr, "Data Source=C:\LkyRess\MySet.sqlite;Version=3;").DefaultView
        tblNbAgence.Text = "Nombre d'agences actives: " & MayStatVal("select count(*) from AGENCES where Actif=1 ", "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") & " | Nombre d'agences fermées: " & MayStatVal("select count(*) from AGENCES where Actif=0 ", "Data Source=C:\LkyRess\MySet.sqlite;Version=3;")
        grdAddNewAgence.Visibility = Visibility.Collapsed
    End Sub
End Class
