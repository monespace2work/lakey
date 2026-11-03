Public Class usercAngence
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim istr As String = " SELECT * FROM AGENCES ORDER BY NOM "
        'istr += " from siteaffectation s "
        Dim agencyData = LoadDataTable(istr, "Data Source=C:\LkyRess\MySet.sqlite;Version=3;")
        DgAgency.ItemsSource = agencyData.DefaultView
        tblNbAgence.Text = "Nombre d'agences actives: " & agencyData.Select("Actif = 1").Length & " | Nombre d'agences fermées: " & agencyData.Select("Actif = 0").Length
        grdAddNewAgence.Visibility = Visibility.Collapsed
    End Sub
End Class
