Imports System.Data
Public Class ProfilUser
    Dim RefProfil As Integer = -1
    Dim iConnxStr As String
    Dim iCbxProf As New ComboBox
    Dim iCbxLevel As New ComboBox
    Dim iFormLoaded As Boolean = False
    Private Sub CheckBox_Checked(sender As Object, e As RoutedEventArgs)
        chbRead.IsChecked = True
        chbAdd.IsChecked = True
        chbModif.IsChecked = True
        chbDel.IsChecked = True
    End Sub

    Private Sub chbAllParam_Unchecked(sender As Object, e As RoutedEventArgs) Handles chbAllParam.Unchecked
        chbRead.IsChecked = False
        chbAdd.IsChecked = False
        chbModif.IsChecked = False
        chbDel.IsChecked = False
    End Sub

    Private Sub chbRead_Checked(sender As Object, e As RoutedEventArgs) Handles chbRead.Checked
        chbReadUser.IsChecked = True
        chbReadGlobal.IsChecked = True
        chbReadSociety.IsChecked = True
        chbReadSpecial.IsChecked = True
        chbReadOrdinary.IsChecked = True
    End Sub

    Private Sub chbRead_Unchecked(sender As Object, e As RoutedEventArgs) Handles chbRead.Unchecked
        chbReadUser.IsChecked = False
        chbReadGlobal.IsChecked = False
        chbReadSociety.IsChecked = False
        chbReadSpecial.IsChecked = False
        chbReadOrdinary.IsChecked = False
    End Sub

    Private Sub chbAdd_Checked(sender As Object, e As RoutedEventArgs) Handles chbAdd.Checked
        chbAddUser.IsChecked = True
        chbAddGlobal.IsChecked = True
        chbAddSociety.IsChecked = True
        chbAddSpecial.IsChecked = True
        chbAddOrdinary.IsChecked = True
    End Sub

    Private Sub chbAdd_Unchecked(sender As Object, e As RoutedEventArgs) Handles chbAdd.Unchecked
        chbAddUser.IsChecked = False
        chbAddGlobal.IsChecked = False
        chbAddSociety.IsChecked = False
        chbAddSpecial.IsChecked = False
        chbAddOrdinary.IsChecked = False
    End Sub

    Private Sub chbModif_Checked(sender As Object, e As RoutedEventArgs) Handles chbModif.Checked
        chbModifUser.IsChecked = True
        chbModifGlobal.IsChecked = True
        chbModifSociety.IsChecked = True
        chbModifSpecial.IsChecked = True
        chbModifOrdinary.IsChecked = True
    End Sub

    Private Sub chbModif_Unchecked(sender As Object, e As RoutedEventArgs) Handles chbModif.Unchecked
        chbModifUser.IsChecked = False
        chbModifGlobal.IsChecked = False
        chbModifSociety.IsChecked = False
        chbModifSpecial.IsChecked = False
        chbModifOrdinary.IsChecked = False
    End Sub

    Private Sub chbDel_Checked(sender As Object, e As RoutedEventArgs) Handles chbDel.Checked
        chbDelUser.IsChecked = True
        chbDelGlobal.IsChecked = True
        chbDelSociety.IsChecked = True
        chbDelSpecial.IsChecked = True
        chbDelOrdinary.IsChecked = True
    End Sub

    Private Sub chbDel_Unchecked(sender As Object, e As RoutedEventArgs) Handles chbDel.Unchecked
        chbDelUser.IsChecked = False
        chbDelGlobal.IsChecked = False
        chbDelSociety.IsChecked = False
        chbDelSpecial.IsChecked = False
        chbDelOrdinary.IsChecked = False

    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = (PaphBackUp())
        iConnxStr = $"Data Source={ DbPathGeneral};Version=3;"
        initalLoad()
    End Sub

    Private Sub tbxLevel_GotFocus(sender As Object, e As RoutedEventArgs) Handles tbxLevel.GotFocus
        tbxLevel.SelectAll()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        ResetValues()
    End Sub

    Private Sub tbxLevel_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles tbxLevel.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub
    Sub initalLoad()
        maj()
        dgProfil.ItemsSource = LoadDataTable("select * from LadaWet").DefaultView
        If Not FilmyCbx("select TRIM(LOWER(ProfilName)) from LadaWet ", iCbxProf) Or Not FilmyCbx("select Id from LadaWet ", iCbxLevel) Then MsgBox("Erreur d'accès aux données")
        ResetValues()
        iFormLoaded = True
    End Sub
    Sub ResetValues()
        chbAllParam.IsChecked = True
        chbAllParam.IsChecked = False
        chbRead.IsChecked = False
        chbRead.IsChecked = True
        tbxLevel.Text = 90
        tbxProfildESCRIPT.Text = ""
        tbxProfilnAME.Text = ""
        btnValidNewFontion.Visibility = Visibility.Collapsed
        btnDelete.Visibility = Visibility.Collapsed
        RefProfil = -6
    End Sub
    Function SaveNewProfil() As Boolean
        SaveNewProfil = False
        Dim MyStr = "INSERT INTO LadaWet(Id,ProfilName,Description, "
        MyStr += " CanUserManage, CanSocieteDataManage,CanGlobalDataManage,"
        MyStr += " CanAddDattaOrdinary,CanAddDattaSpecial,"
        MyStr += " CanModifyDattaOrdinary,CanModifyDattaSpecial,"
        MyStr += " CanDeleteDattaOrdinary,CanDeleteDattaSpecial,"
        MyStr += " CanReadDattaOrdinary,CanReadDattaSpecial)"
        MyStr += " VALUES"
        MyStr += "  (" & tbxLevel.Text & ", '" & Apost(tbxProfilnAME.Text) & "','" & Apost(tbxProfildESCRIPT.Text) & "',"
        MyStr += " " & chbAddUser.IsChecked & ", " & chbAddSociety.IsChecked & "," & chbAddGlobal.IsChecked & ","
        MyStr += " " & chbAddOrdinary.IsChecked & ", " & chbAddSpecial.IsChecked & "," ' & chbAddGlobal.IsChecked & ","
        MyStr += " " & chbModifOrdinary.IsChecked & ", " & chbModifSpecial.IsChecked & "," '& chbAddGlobal.IsChecked & ","
        MyStr += " " & chbDelOrdinary.IsChecked & ", " & chbDelSpecial.IsChecked & "," '& chbAddGlobal.IsChecked & ","
        MyStr += " " & chbReadOrdinary.IsChecked & ", " & chbReadSpecial.IsChecked & "" '& chbAddGlobal.IsChecked & ","
        MyStr += " )"
        'ShowTempMsg(MyStr)
        SaveNewProfil = SaveNewData(MyStr, iConnxStr)
    End Function
    Function SaveModifProfil() As Boolean
        SaveModifProfil = False
        Dim MyStr = "UPDATE LadaWet SET " '(Id,ProfilName,Description, "
        ' MyStr += " CanUserManage, CanSocieteDataManage,CanGlobalDataManage,"
        'MyStr += " CanAddDattaOrdinary,CanAddDattaSpecial,"
        'MyStr += " CanModifyDattaOrdinary,CanModifyDattaSpecial,"
        ' MyStr += " CanDeleteDattaOrdinary,CanDeleteDattaSpecial,"
        ' MyStr += " CanReadDattaOrdinary,CanReadDattaSpecial)"
        'MyStr += " VALUES"
        MyStr += "  Id=" & tbxLevel.Text & ", ProfilName='" & Apost(tbxProfilnAME.Text) & "',Description='" & Apost(tbxProfildESCRIPT.Text) & "',"
        MyStr += " CanUserManage=" & chbAddUser.IsChecked & ", CanSocieteDataManage=" & chbAddSociety.IsChecked & ",CanGlobalDataManage=" & chbAddGlobal.IsChecked & ","
        MyStr += " CanAddDattaOrdinary=" & chbAddOrdinary.IsChecked & ", CanAddDattaSpecial=" & chbAddSpecial.IsChecked & "," ' & chbAddGlobal.IsChecked & ","
        MyStr += " CanModifyDattaOrdinary=" & chbModifOrdinary.IsChecked & ", CanModifyDattaSpecial=" & chbModifSpecial.IsChecked & "," '& chbAddGlobal.IsChecked & ","
        MyStr += " CanDeleteDattaOrdinary=" & chbDelOrdinary.IsChecked & ", CanDeleteDattaSpecial=" & chbDelSpecial.IsChecked & "," '& chbAddGlobal.IsChecked & ","
        MyStr += " CanReadDattaOrdinary=" & chbReadOrdinary.IsChecked & ", CanReadDattaSpecial=" & chbReadSpecial.IsChecked & "" '& chbAddGlobal.IsChecked & ","
        MyStr += " where Id=" & RefProfil
        'ShowTempMsg(MyStr)
        SaveModifProfil = SaveNewData(MyStr, iConnxStr)
    End Function
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If Not IsNothing(rowView.Row.ItemArray(0).ToString) Then
                tbxLevel.Text = rowView.Row.ItemArray(0).ToString
                tbxProfilnAME.Text = rowView.Row.ItemArray(1).ToString
                chbAddUser.IsChecked = rowView.Row.ItemArray(2).ToString
                chbAddGlobal.IsChecked = rowView.Row.ItemArray(3).ToString
                chbAddSociety.IsChecked = rowView.Row.ItemArray(4).ToString
                chbAddOrdinary.IsChecked = rowView.Row.ItemArray(5).ToString
                chbModifOrdinary.IsChecked = rowView.Row.ItemArray(6).ToString
                chbDelOrdinary.IsChecked = rowView.Row.ItemArray(7).ToString
                chbReadOrdinary.IsChecked = rowView.Row.ItemArray(8).ToString
                chbAddSpecial.IsChecked = rowView.Row.ItemArray(9).ToString
                chbModifSpecial.IsChecked = rowView.Row.ItemArray(10).ToString
                chbDelSpecial.IsChecked = rowView.Row.ItemArray(11).ToString
                chbReadSpecial.IsChecked = rowView.Row.ItemArray(12).ToString
                tbxProfildESCRIPT.Text = rowView.Row.ItemArray(13).ToString
                RefProfil = rowView.Row.ItemArray(0).ToString
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
        'AfficheProfilUser()

    End Sub

    Sub ActivValidBtn()
        If iFormLoaded Then
            If CasValidation = 0 Then
                If tbxLevel.Text.Length >= 1 And tbxProfilnAME.Text.Length >= 3 And Not iCbxProf.Items.Contains(Trim(tbxProfilnAME.Text.ToLower)) And Not iCbxLevel.Items.Contains(tbxProfilnAME.Text) Then
                    btnValidNewFontion.Visibility = Visibility.Visible
                    btnDelete.Visibility = Visibility.Collapsed
                Else
                    btnValidNewFontion.Visibility = Visibility.Collapsed
                    btnDelete.Visibility = Visibility.Collapsed
                End If

            Else
                If tbxLevel.Text.Length >= 1 And tbxProfilnAME.Text.Length >= 3 And RefProfil >= 0 Then
                    btnValidNewFontion.Visibility = Visibility.Visible
                    btnDelete.Visibility = Visibility.Visible
                Else
                    btnValidNewFontion.Visibility = Visibility.Collapsed
                    btnDelete.Visibility = Visibility.Collapsed
                End If

            End If
        End If
        'AfficheProfilUser()
    End Sub

    Private Sub btnValidNewFontion_Click(sender As Object, e As RoutedEventArgs) Handles btnValidNewFontion.Click
        If MsgBox("Enregistrer les données?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If CasValidation = 0 Then
                If SaveNewProfil() Then
                    MessageBox.Show("Données enregistrées avec succès")
                    initalLoad()
                Else
                    MessageBox.Show("Erreur les données non pas été enregistrée", "Error", MessageBoxButton.OK, MessageBoxImage.Error)

                End If
            Else
                If SaveModifProfil() Then
                    MessageBox.Show("Données enregistrées avec succès")
                    initalLoad()
                Else
                    MessageBox.Show("Erreur les données non pas été enregistrée", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If

            End If
        End If

    End Sub

    Private Sub dgProfil_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles dgProfil.MouseDoubleClick
        chbAllParam.IsChecked = True
        chbAdd.IsChecked = True
        chbAllParam.IsChecked = False
        'MsgBox("ok")
        chbAdd.IsChecked = False
        Dim rowView As DataRowView = TryCast(dgProfil.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub tbxProfilnAME_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxProfilnAME.TextChanged, tbxLevel.TextChanged, tbxProfildESCRIPT.TextChanged
        ActivValidBtn()
    End Sub

    Private Sub chbReadUser_Checked(sender As Object, e As RoutedEventArgs) Handles chbReadUser.Checked, chbReadUser.Unchecked, chbReadGlobal.Checked, chbReadGlobal.Unchecked, chbReadOrdinary.Checked, chbReadOrdinary.Unchecked, chbReadSociety.Checked, chbReadSociety.Unchecked, chbReadSpecial.Checked, chbReadSpecial.Unchecked
        ActivValidBtn()
    End Sub

    Private Sub chbAddUser_Checked(sender As Object, e As RoutedEventArgs) Handles chbAddUser.Checked, chbAddUser.Unchecked, chbAddGlobal.Checked, chbAddGlobal.Unchecked, chbAddOrdinary.Checked, chbAddOrdinary.Unchecked, chbAddSociety.Checked, chbAddSociety.Unchecked, chbAddSpecial.Checked, chbAddSpecial.Unchecked
        ActivValidBtn()
    End Sub

    Private Sub chbModifUser_Checked(sender As Object, e As RoutedEventArgs) Handles chbModifUser.Checked, chbModifUser.Unchecked, chbModifGlobal.Checked, chbModifGlobal.Unchecked, chbModifOrdinary.Checked, chbModifOrdinary.Unchecked, chbModifSociety.Checked, chbModifSociety.Unchecked, chbModifSpecial.Checked, chbModifSpecial.Unchecked
        ActivValidBtn()
    End Sub

    Private Sub chbDelUser_Checked(sender As Object, e As RoutedEventArgs) Handles chbDelUser.Checked, chbDelUser.Unchecked, chbDelGlobal.Checked, chbDelGlobal.Unchecked, chbDelOrdinary.Checked, chbDelOrdinary.Unchecked, chbDelSociety.Checked, chbDelSociety.Unchecked, chbDelSpecial.Checked, chbDelSpecial.Unchecked
        ActivValidBtn()
    End Sub
End Class
