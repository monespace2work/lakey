Imports System.Data
Public Class usercFonction
    Public iConnxStr As String
    Dim RefFonction As Integer = 0
    Dim iFormLoaded As Boolean = False
    Dim CasValidation As Integer = 0
    Dim iCbxFonction As New ComboBox
    Private Sub btnValidNewFontion_Click(sender As Object, e As RoutedEventArgs) Handles btnValidNewFontion.Click
        If MsgBox("Enregistrer les données?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Select Case CasValidation
                Case 0
                    If SaveNewFonctionData() Then
                        MessageBox.Show("Données enregistrées avec succès")
                        If ckbxFonctionDefaut.IsChecked Then
                            If SaveNewData($"update Sets set FonctionDefaut='{Apost(tbxFonctionName.Text)}'", "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") Then FonctionDefault = tbxFonctionName.Text
                        End If
                        InitialLoad()
                    Else
                        MessageBox.Show("Erreur les donnée non pas été enregistrée", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    End If
                Case 1
                    If SaveModifFonctionData() Then
                        MessageBox.Show("Modification effectuée avec succès")
                        If ckbxFonctionDefaut.IsChecked Then
                            If SaveNewData($"update Sets set FonctionDefaut='{Apost(tbxFonctionName.Text)}'", "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") Then FonctionDefault = tbxFonctionName.Text
                        End If
                        InitialLoad()
                    Else
                        MessageBox.Show("Erreur les Modifications non pas été effectuées", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    End If
            End Select
        End If
    End Sub
    Sub ActivValidBtn()
        If iFormLoaded Then
            If CasValidation = 0 Then
                If tbxFonctionName.Text.Length >= 2 And tbxFonctionDescription.Text.Length >= 2 And Not iCbxFonction.Items.Contains(Trim(tbxFonctionName.Text.ToLower)) And IsNumeric(tbxFonctionNiveau.Text.Trim) Then
                    btnValidNewFontion.Visibility = Visibility.Visible
                    btnDelete.Visibility = Visibility.Collapsed
                Else
                    btnValidNewFontion.Visibility = Visibility.Collapsed
                    btnDelete.Visibility = Visibility.Collapsed
                End If

            Else
                If tbxFonctionName.Text.Length >= 2 And tbxFonctionDescription.Text.Length >= 2 And RefFonction > 0 And IsNumeric(tbxFonctionNiveau.Text.Trim) Then
                    btnValidNewFontion.Visibility = Visibility.Visible
                    btnDelete.Visibility = Visibility.Visible
                Else
                    btnValidNewFontion.Visibility = Visibility.Collapsed
                    btnDelete.Visibility = Visibility.Collapsed
                End If

            End If
            AfficheProfilUser()
        End If
    End Sub

    Private Sub tbxFonctionName_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxFonctionName.TextChanged
        ActivValidBtn()
        tbxFonctionDescription.Text = tbxFonctionName.Text
    End Sub

    Private Sub tbxFonctionDescription_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxFonctionDescription.TextChanged
        ActivValidBtn()
    End Sub

    Private Sub ckbxFonctionActif_TouchEnter(sender As Object, e As TouchEventArgs) Handles ckbxFonctionActif.TouchEnter
        ActivValidBtn()
    End Sub

    Function SaveNewFonctionData() As Boolean
        Dim MyStr = "INSERT INTO Fonction (Fonction,Niveau, Actif, Description) "
        MyStr += " VALUES  ('" & Apost(tbxFonctionName.Text.Trim) & "',"
        MyStr += " " & tbxFonctionNiveau.Text.Trim & ", "
        MyStr += " " & ckbxFonctionActif.IsChecked & ", "
        MyStr += "'" & Apost(tbxFonctionDescription.Text.Trim) & "') "
        SaveNewFonctionData = SaveNewData(MyStr, iConnxStr)
    End Function
    Function SaveModifFonctionData() As Boolean
        Dim MyStr = "UPDATE Fonction SET Fonction = '" & Apost(tbxFonctionName.Text.Trim) & "',"
        MyStr += " Niveau = " & tbxFonctionNiveau.Text.Trim & ", "
        MyStr += " Actif = " & ckbxFonctionActif.IsChecked & ", "
        MyStr += " Description = '" & Apost(tbxFonctionDescription.Text.Trim) & "' "
        MyStr += "WHERE Id =" & RefFonction & " "
        SaveModifFonctionData = SaveNewData(MyStr, iConnxStr)
    End Function

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = (PaphBackUp())
        iConnxStr = $"Data Source={ DbPathGeneral};Version=3;"
        InitialLoad()
        stpAddNewFonction.Visibility = Visibility.Visible
    End Sub
    Sub InitialLoad()
        Dim istr As String = " SELECT Fonction, Niveau, Actif, Description,Id FROM Fonction ORDER BY Actif desc, Niveau, Fonction "
        maj()
        DgFonction.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbFonction.Text = "Nombre de fonctions actives: " & MayStatVal("select count(*) from Fonction where Actif=1 ") & " | Nombre de fonctions désactivées: " & MayStatVal("select count(*) from Fonction where Actif=0 ")
        FilmyCbx("SELECT TRIM(LOWER(Fonction)) FROM Fonction ORDER BY Fonction", iCbxFonction) 'Then MsgBox(iCbxFonction.Items.Count)
        tbxFonctionName.Text = ""
        tbxFonctionDescription.Text = ""
        ckbxFonctionActif.IsChecked = True
        ckbxFonctionDefaut.IsChecked = False
        ActivValidBtn()
        AfficheProfilUser()
        btnValidNewFontion.Visibility = Visibility.Collapsed
        btnDelete.Visibility = Visibility.Collapsed
        iFormLoaded = True
    End Sub
    Sub ResetValues()
        tbxFonctionName.Text = ""
        tbxFonctionDescription.Text = ""
        ckbxFonctionActif.IsChecked = True
        ckbxFonctionDefaut.IsChecked = False
        btnValidNewFontion.Visibility = Visibility.Collapsed
        btnDelete.Visibility = Visibility.Collapsed
        CasValidation = 0
        AfficheProfilUser()
    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If Not IsNothing(rowView.Row.ItemArray(0).ToString) Then
                tbxFonctionName.Text = rowView.Row.ItemArray(0).ToString
                tbxFonctionNiveau.Text = rowView.Row.ItemArray(1).ToString
                ckbxFonctionActif.IsChecked = rowView.Row.ItemArray(2).ToString
                tbxFonctionDescription.Text = rowView.Row.ItemArray(3).ToString
                RefFonction = rowView.Row.ItemArray(4).ToString
                CasValidation = 1
                btnValidNewFontion.Content = "MODIFIER"
                'btnSaveContinue.Content = "Modifier et Continuer"
            Else
                CasValidation = 0
                btnValidNewFontion.Content = "AJOUTER"
                'btnSaveContinue.Content = "Enregistrer et ajouter un autre"
            End If
        End If
        ActivValidBtn()
        AfficheProfilUser()

    End Sub

    Private Sub DgFonction_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgFonction.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgFonction.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        ResetValues()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As RoutedEventArgs) Handles btnDelete.Click
        If MsgBox("SUPPRIMER DEFINITIVEMENT CET ELEMENT ? ", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Dim ISTR As String = "DELETE FROM Fonction WHERE Id=" & RefFonction
            'ShowTempMsg(ISTR)
            If SaveNewData(ISTR, iConnxStr) Then
                MsgBox("Données suprrimées avec succès")
                InitialLoad()
            End If
        End If
    End Sub
    Sub AfficheProfilUser()
        If Not CanAddDataOrdinary Then
            btnDelete.Visibility = Visibility.Collapsed
        End If
        If Not CanModifyDataOrdinary Then
            If btnValidNewFontion.Content = "MODIFIER" Then btnValidNewFontion.Visibility = Visibility.Collapsed
        End If
        If Not CanDeleteDataOrdinary And Not CanGlobalDataManage Then
            btnValidNewFontion.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub tbxFonctionNiveau_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxFonctionNiveau.TextChanged
        ActivValidBtn()
    End Sub
End Class
