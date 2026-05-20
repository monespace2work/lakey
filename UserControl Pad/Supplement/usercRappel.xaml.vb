Imports System.Data
Public Class usercRappel
    Dim mDate As Date = Today
    Dim RefMemo As Integer = -6
    Dim CasValidation As Integer = 0
    Dim iFrmLoaded As Boolean = False
    Public iConnxStr As String
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = (PaphBackUp())
        iConnxStr = $"Data Source={ DbPathGeneral};Version=3;"
        DtpDebut.SelectedDate = Today
        DtpFin.SelectedDate = Today
        Initialization()

    End Sub
    Sub Initialization()
        'DtpDebut.SelectedDate = Today
        'DtpFin.SelectedDate = Today
        maj()
        Dim istr As String = " select r.NoteRappel,format(r.Debut) as Debut,format(r.Fin) as Fin,r.Obs,r.Actif,r.Id from rappels r "
        istr += " ORDER BY r.Actif desc, r.Debut "
        DgRappels.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbRappels.Text = "Nombre de rappels actifs : " & MayStatVal("select count(*) from rappels  where Actif=1") ' & " | Nombre d'agents actifs sans un N° CNSSS : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.Nsecu like '' and a.Nsecu not null)")
        CasValidation = 0
        iFrmLoaded = True
        ActivValidBtn()
        AfficheProfilUser()
    End Sub

    Sub resetvalues()
        CasValidation = 0
        tbxMemoContent.Text = ""
        tbxMemoObs.Text = ""
        btnValidNewMemo.Content = "AJOUTER"
        ckbxMemoActif.IsChecked = True
        DtpDebut.SelectedDate = Today
        DtpFin.SelectedDate = Today

        mDate = Today
        RefMemo = -6
        AfficheProfilUser()
    End Sub
    Sub ActivValidBtn()
        If iFrmLoaded Then
            If tbxMemoContent.Text.Trim.Length > 3 AndAlso DtpDebut.SelectedDate.HasValue AndAlso DtpFin.SelectedDate.HasValue Then
                btnValidNewMemo.Visibility = Visibility.Visible
                If CasValidation = 1 And RefMemo > 0 Then
                    btnDelete.Visibility = Visibility.Visible
                    If mDate <> DtpDebut.SelectedDate.Value Then
                        btnCopyMemo.Visibility = Visibility.Visible
                    Else
                        btnCopyMemo.Visibility = Visibility.Collapsed
                    End If
                Else
                    btnDelete.Visibility = Visibility.Collapsed
                End If
            Else
                btnValidNewMemo.Visibility = Visibility.Collapsed
                btnDelete.Visibility = Visibility.Collapsed
                btnCopyMemo.Visibility = Visibility.Collapsed
            End If
            AfficheProfilUser()
        End If
    End Sub

    Private Sub tbxMemoContent_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxMemoContent.TextChanged
        ActivValidBtn()
    End Sub

    Private Sub DtpDebut_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DtpDebut.SelectedDateChanged
        If DtpDebut.SelectedDate.HasValue AndAlso DtpFin.SelectedDate.HasValue Then
            If DtpDebut.SelectedDate.Value > DtpFin.SelectedDate.Value Then
                DtpFin.SelectedDate = DtpDebut.SelectedDate.Value
            End If
        End If
        ActivValidBtn()
    End Sub

    Private Sub DtpFin_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DtpFin.SelectedDateChanged
        If DtpDebut.SelectedDate.HasValue AndAlso DtpFin.SelectedDate.HasValue Then
            If DtpDebut.SelectedDate.Value > DtpFin.SelectedDate.Value Then
                DtpDebut.SelectedDate = DtpFin.SelectedDate.Value
            End If
        End If
        ActivValidBtn()

    End Sub

    Private Sub tbxMemoObs_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxMemoObs.TextChanged
        ActivValidBtn()
    End Sub

    Private Sub btnValidNewMemo_Click(sender As Object, e As RoutedEventArgs) Handles btnValidNewMemo.Click
        Dim iStr As String = ""
        If CasValidation = 0 Then
            iStr = "insert into rappels (NoteRappel,Debut,Fin,Obs,Actif) values "
            iStr &= $"('{Apost(tbxMemoContent.Text.Trim)}','{DtpDebut.SelectedDate.Value.ToString("yyyy-MM-dd")}','{DtpFin.SelectedDate.Value.ToString("yyyy-MM-dd")}','{Apost(tbxMemoObs.Text.Trim)}',{ckbxMemoActif.IsChecked})"
            If SaveNewData(iStr, iConnxStr) Then
                Joper("Add new Rappels ", iConnxStr)
                MsgBox("Données enregistrées")
                Initialization()
                resetvalues()
            Else
                MsgBox("Echec d'enregistrement")
            End If
        Else
            iStr = "update rappels set "
            iStr &= $"NoteRappel='{Apost(tbxMemoContent.Text.Trim)}',Debut='{DtpDebut.SelectedDate.Value.ToString("yyyy-MM-dd")}',Fin='{DtpFin.SelectedDate.Value.ToString("yyyy-MM-dd")}',Obs='{Apost(tbxMemoObs.Text.Trim)}',Actif={ckbxMemoActif.IsChecked}"
            iStr &= $" where Id={RefMemo}"
            If SaveNewData(iStr, iConnxStr) Then
                Joper("Mofif Rappels at id=" & RefMemo, iConnxStr)
                MsgBox("Données modifiée avec succès")
                Initialization()
                resetvalues()
            Else
                MsgBox("Echec d'enregistrement")
            End If

        End If

    End Sub

    Private Sub DgRappels_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgRappels.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgRappels.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If Not IsNothing(rowView.Row.ItemArray(0).ToString) Then
                tbxMemoContent.Text = rowView.Row.ItemArray(0).ToString
                DtpDebut.SelectedDate = CDate(rowView.Row.ItemArray(1).ToString)
                mDate = CDate(rowView.Row.ItemArray(1).ToString)
                DtpFin.SelectedDate = CDate(rowView.Row.ItemArray(2).ToString)
                tbxMemoObs.Text = rowView.Row.ItemArray(3).ToString
                ckbxMemoActif.IsChecked = rowView.Row.ItemArray(4).ToString
                RefMemo = rowView.Row.ItemArray(5).ToString
                CasValidation = 1
                btnValidNewMemo.Content = "MODIFIER"
            Else
                CasValidation = 0
                btnValidNewMemo.Content = "AJOUTER"
            End If
        End If
        ActivValidBtn()
        AfficheProfilUser()

    End Sub

    Private Sub btnDelete_Click(sender As Object, e As RoutedEventArgs) Handles btnDelete.Click
        If SaveNewData($"delete from rappels where id={RefMemo}", iConnxStr) Then
            Joper("Delete Rappels at id=" & RefMemo, iConnxStr)
            MsgBox("Données suppriméees avec succès")
            Initialization()
            resetvalues()
        Else
            MsgBox("Echec de suppression")
        End If

    End Sub
    Sub AfficheProfilUser()
        If Not CanAddDataOrdinary Then
            btnCopyMemo.Visibility = Visibility.Collapsed
            btnValidNewMemo.Visibility = Visibility.Collapsed
        End If
        If Not CanModifyDataOrdinary Then
            btnCopyMemo.Visibility = Visibility.Collapsed
            If btnValidNewMemo.Content = "MODIFIER" Then btnValidNewMemo.Visibility = Visibility.Collapsed
        End If
        If Not CanDeleteDataOrdinary And Not CanGlobalDataManage Then
            btnDelete.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub btnCopyMemo_Click(sender As Object, e As RoutedEventArgs) Handles btnCopyMemo.Click
        If MessageBox.Show("Créer une COPIE ?", "COPIE...", MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            Dim iStr As String = ""
            iStr = "insert into rappels (NoteRappel,Debut,Fin,Obs,Actif) values "
            iStr &= $"('{Apost(tbxMemoContent.Text.Trim)}','{DtpDebut.SelectedDate.Value.ToString("yyyy-MM-dd")}','{DtpFin.SelectedDate.Value.ToString("yyyy-MM-dd")}','{Apost(tbxMemoObs.Text.Trim)}',{ckbxMemoActif.IsChecked})"
            If SaveNewData(iStr, iConnxStr) Then
                Joper("Add new Rappels ", iConnxStr)
                MsgBox("Copie réalisée avec succès")
                Initialization()
                resetvalues()
            Else
                MsgBox("Echec d'enregistrement")
            End If
        End If
    End Sub

    Private Sub DtpDebut_LostFocus(sender As Object, e As RoutedEventArgs) Handles DtpDebut.LostFocus
        If DtpDebut.SelectedDate.HasValue AndAlso DtpFin.SelectedDate.HasValue Then
            If DtpDebut.SelectedDate.Value > DtpFin.SelectedDate.Value Then
                DtpFin.SelectedDate = DtpDebut.SelectedDate.Value
            End If
        End If
        ActivValidBtn()
    End Sub
End Class
