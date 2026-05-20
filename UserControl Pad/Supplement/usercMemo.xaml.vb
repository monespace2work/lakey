Imports System.Data
Public Class usercMemo
    Dim mDate As Date = Today
    Dim RefMemo As Integer = -6
    Dim CasValidation As Integer = 0
    Dim iFrmLoaded As Boolean = False
    Public iConnxStr As String

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = (PaphBackUp())
        iConnxStr = $"Data Source={ DbPathGeneral};Version=3;"
        Initialization()
    End Sub
    Sub Initialization()
        Dim istr As String = "Select m.MemContent,date(m.DateMem) as Date,m.Actif,m.IdMem as Id from memos m "
        'istr += "  from events e "
        istr += " ORDER BY m.Actif desc, m.DateMem "
        DgMemo.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbMemo.Text = "Nombre de memos en attente : " & MayStatVal("select count(*) from memos  where Actif=1") ' & " | Nombre d'agents actifs sans un N° CNSSS : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.Nsecu like '' and a.Nsecu not null)")
        CasValidation = 0
        iFrmLoaded = True
        ActivValidBtn()
    End Sub
    Private Sub DgMemo_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgMemo.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgMemo.CurrentItem, DataRowView)
        LoadRowItems(rowView)

    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If Not IsNothing(rowView.Row.ItemArray(0).ToString) Then
                tbxMemoContent.Text = rowView.Row.ItemArray(0).ToString
                mDate = CDate(rowView.Row.ItemArray(1).ToString)
                ckbxMemoActif.IsChecked = rowView.Row.ItemArray(2).ToString
                RefMemo = rowView.Row.ItemArray(3).ToString
                CasValidation = 1
                btnValidNewMemo.Content = "MODIFIER"
            Else
                CasValidation = 0
                btnValidNewMemo.Content = "AJOUTER"
            End If
        End If
        ActivValidBtn()
        'AfficheProfilUser()

    End Sub
    Sub ActivValidBtn()
        If iFrmLoaded Then
            If tbxMemoContent.Text.Trim.Length > 3 Then
                btnValidNewMemo.Visibility = Visibility.Visible
                If CasValidation = 1 And RefMemo > 0 Then
                    btnDelete.Visibility = Visibility.Visible
                Else
                    btnDelete.Visibility = Visibility.Collapsed
                End If
            Else
                btnValidNewMemo.Visibility = Visibility.Collapsed
                btnDelete.Visibility = Visibility.Collapsed
            End If
        End If
    End Sub

    Private Sub tbxMemoContent_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxMemoContent.TextChanged
        ActivValidBtn()
    End Sub

    Private Sub ckbxMemoActif_Checked(sender As Object, e As RoutedEventArgs) Handles ckbxMemoActif.Checked
        ActivValidBtn()
    End Sub

    Private Sub ckbxMemoActif_Unchecked(sender As Object, e As RoutedEventArgs) Handles ckbxMemoActif.Unchecked
        ActivValidBtn()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        resetvalues()
    End Sub
    Sub resetvalues()
        CasValidation = 0
        tbxMemoContent.Text = ""
        btnValidNewMemo.Content = "AJOUTER"
        ckbxMemoActif.IsChecked = True
        mDate = Today
        RefMemo = -6
    End Sub

    Private Sub btnValidNewMemo_Click(sender As Object, e As RoutedEventArgs) Handles btnValidNewMemo.Click
        If CasValidation = 0 Then
            If SaveNewData($"insert into memos (MemContent,Actif) values ('{Apost(tbxMemoContent.Text.Trim)}',{ckbxMemoActif.IsChecked})", iConnxStr) Then
                Joper("Add new memo ", iConnxStr)
                MsgBox("Données enregistrées")
                Initialization()
                resetvalues()
            Else
                MsgBox("Echec d'enregistrement")
            End If
        Else
            If SaveNewData($"update memos set MemContent='{Apost(tbxMemoContent.Text.Trim)}',Actif={ckbxMemoActif.IsChecked}, DateMem='{Now.ToString("yyyy-MM-dd HH:mm:ss")}' where IdMem={RefMemo}", iConnxStr) Then
                Joper("Mofif memo at id=" & RefMemo, iConnxStr)
                MsgBox("Données modifiée avec succès")
                Initialization()
                resetvalues()
            Else
                MsgBox("Echec d'enregistrement")
            End If

        End If
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As RoutedEventArgs) Handles btnDelete.Click
        If SaveNewData($"delete from memos where IdMem={RefMemo}", iConnxStr) Then
            Joper("Delete memo at id=" & RefMemo, iConnxStr)
            MsgBox("Données suppriméees avec succès")
            Initialization()
            resetvalues()
        Else
            MsgBox("Echec de suppression")
        End If
    End Sub
End Class
