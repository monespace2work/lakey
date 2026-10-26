Imports System.Data

Public Class UserManager
    Dim iUsersCbx As New ComboBox
    Dim iConnxStr As String
    Dim RefPst As Integer = -1
    Dim isNew As Boolean = True
    Dim NewUserName As String
    Dim uLevel As Integer = -1

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        Initializing()
    End Sub
    Sub Initializing()
        Dim iStr As String = "select l.UserName,w.ProfilName,l.Agence,l.UserPassWord,l.NomOperateur,l.IdUser from lada l join ladawet w on l.UserLever=w.Id where l.IdUser>1"
        dgUsers.ItemsSource = LoadDataTable(iStr, iConnxStr).DefaultView
        iStr = "select a.NomPrenoms from (agents a join affectation af on a.NomPrenoms=af.AgentAffect join siteaffectation s on s.NomSite=af.SiteAffect) join client c on c.NomClient=s.Client where a.Actif=1 and af.FinAffect>=date() and c.NomClient like '%admin%' order by a.Fonction desc,a.NomPrenoms"
        FilmyCbx(iStr, CbxOperateur)
        FilmyCbx("select trim(lower(l.UserName)) from lada l ", iUsersCbx)
        FilmyCbx("select ProfilName from LadaWet order by Id desc ", cbxLevel)
        isNew = True
        ActivSaveBtn()
    End Sub
    Sub ActivSaveBtn()
        If isNew Then
            Try
                If CbxOperateur.SelectedIndex >= 0 AndAlso TbxUserName.Text.Trim.Length >= 3 AndAlso Not iUsersCbx.Items.Contains(TbxUserName.Text.Trim.ToLower) AndAlso TbxPassW.Text.Length >= 3 AndAlso cbxLevel.SelectedIndex >= 0 AndAlso uLevel >= 0 Then
                    BtnSave.Visibility = Visibility.Visible
                Else
                    BtnSave.Visibility = Visibility.Collapsed
                End If

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        ElseIf NewUserName = TbxUserName.Text Then
            Try
                If CbxOperateur.SelectedIndex >= 0 AndAlso TbxUserName.Text.Trim.Length >= 3 AndAlso TbxPassW.Text.Length >= 3 AndAlso cbxLevel.SelectedIndex >= 0 AndAlso uLevel >= 0 AndAlso RefPst > 0 Then
                    BtnSave.Visibility = Visibility.Visible
                Else
                    BtnSave.Visibility = Visibility.Collapsed
                End If

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else
            Try
                If CbxOperateur.SelectedIndex >= 0 AndAlso TbxUserName.Text.Trim.Length >= 3 AndAlso Not iUsersCbx.Items.Contains(TbxUserName.Text.Trim.ToLower) AndAlso TbxPassW.Text.Length >= 3 AndAlso cbxLevel.SelectedIndex >= 0 AndAlso uLevel >= 0 AndAlso RefPst > 0 Then
                    BtnSave.Visibility = Visibility.Visible
                Else
                    BtnSave.Visibility = Visibility.Collapsed
                End If

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        End If
    End Sub
    Private Sub TbxUserName_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxUserName.TextChanged, TbxPassW.TextChanged
        ActivSaveBtn()
    End Sub

    Private Sub CbxOperateur_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxOperateur.SelectionChanged
        ActivSaveBtn()
    End Sub

    Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs) Handles BtnSave.Click
        If LocalModeBb Then MsgBox("Vous êtes en mode LOCAL")
        If isNew Then
            If SaveNewUser(iConnxStr) Then
                MsgBox("Nouvel utilisateur créé avec succès")
                Initializing()
                resetvalues()
            Else
                MsgBox("Erreur d'enregistrement")
            End If
        Else
            If SaveModifUser(iConnxStr) Then
                MsgBox("Utilisateur modifié avec succès")
                Initializing()
                resetvalues()
            Else
                MsgBox("Erreur d'enregistrement")
            End If

        End If
    End Sub
    Sub resetvalues()
        CbxOperateur.SelectedIndex = -1
        TbxUserName.Text = ""
        TbxPassW.Text = ""
        cbxLevel.SelectedIndex = -1
        BtnSave.Content = "Enregistrer"
        isNew = True
    End Sub
    Function SaveNewUser(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        Dim iStr As String = "insert into lada (UserName,UserPassWord,UserLever,NomOperateur) values "
        iStr &= " ("
        iStr &= " '" & Apost(TbxUserName.Text.Trim) & "', "
        iStr &= " '" & Apost(TbxPassW.Text.Trim) & "', "
        iStr &= " " & uLevel & ", "
        iStr &= " '" & Apost(CbxOperateur.SelectedItem) & "' "
        iStr &= ")"
        SaveNewUser = SaveNewData(iStr, iConxStr)
    End Function
    Function SaveModifUser(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        Dim iStr As String = "update lada  "
        iStr &= " set "
        iStr &= " UserName='" & Apost(TbxUserName.Text.Trim) & "', "
        iStr &= " UserPassWord='" & Apost(TbxPassW.Text.Trim) & "', "
        iStr &= " UserLever=" & uLevel & ", "
        iStr &= " NomOperateur='" & Apost(CbxOperateur.SelectedItem) & "' "
        iStr &= " where IdUser=" & RefPst
        SaveModifUser = SaveNewData(iStr, iConxStr)
    End Function

    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            TbxUserName.Text = rowView.Row.ItemArray(0).ToString
            NewUserName = rowView.Row.ItemArray(0).ToString
            TbxPassW.Text = rowView.Row.ItemArray(2).ToString
            cbxLevel.SelectedItem = rowView.Row.ItemArray(1).ToString
            If CbxOperateur.Items.Contains(rowView.Row.ItemArray(4).ToString) Then
                CbxOperateur.SelectedItem = rowView.Row.ItemArray(4).ToString
            Else
                CbxOperateur.SelectedIndex = -1
            End If
            RefPst = rowView.Row.ItemArray(5).ToString
            BtnSave.Content = "Modifier"
            isNew = False
            ActivSaveBtn()
        Else
            RefPst = 0
            BtnSave.Content = "Enregistrer"
            'ActivSaveBtn()
            isNew = True
            ActivSaveBtn()
        End If

    End Sub

    Private Sub dgUsers_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles dgUsers.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(dgUsers.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs) Handles BtnCancel.Click
        resetvalues()
    End Sub

    Private Sub cbxLevel_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxLevel.SelectionChanged
        If cbxLevel.SelectedIndex >= 0 Then
            Dim iLevel As String = MyStatValStr($"select Id from LadaWet where ProfilName='{Apost(cbxLevel.SelectedItem)}'")
            If IsNumeric(iLevel) Then uLevel = CInt(iLevel)
            'MsgBox(iLevel)
        End If
    End Sub
End Class
