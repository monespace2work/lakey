Public Class usercStructure
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim MyString As String
        MyString = "SELECT format(DateNaiss)  "
        MyString += " FROM agents "
        MyString += " WHERE Actif =1 order by DateNaiss desc limit 1"

        'MsgBox(AgeMoyAactif())
        If SturctureAffiche() Then
            DgStructure.ItemsSource = StructureDTable.DefaultView
        Else
            MsgBox("erreur de chargement")
        End If
        DgFonction.ItemsSource = LoadDataTable("select Fonction, Actif from Fonction order by Actif desc, Fonction ").DefaultView
        DgFonctionNbre.ItemsSource = LoadDataTable("select a.Fonction, count(*) as Nombre FROM Fonction f join agents a on f.Fonction=a.Fonction where a.Actif=1 group by f.Fonction  ").DefaultView
    End Sub
End Class
