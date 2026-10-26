Imports System.Windows.Threading
Public Class usercNCNSS
    Dim inputTimer As DispatcherTimer
    Dim iConnxStr As String
    Dim iCbxNsecu As New ComboBox
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
        DbPathGeneral = (PaphBackUp())
        iConnxStr = $"Data Source={DbPathGeneral};Version=3;"

        Initilisation()

        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(3)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed
        elpStatus.Visibility = Visibility.Collapsed

    End Sub
    Sub Initilisation()
        Dim istr As String = " select a.NomPrenoms,a.NumMatr, a.Nsecu,  a.Contacts,a.Actif from agents a where a.Actif=1 "
        istr += " order by a.Actif desc, a.NomPrenoms "
        Dgcnss.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbcnss.Text = "Nombre d'agents actifs ayant un N° CNSSS : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.Nsecu not like '' and a.Nsecu not null)") & " | Nombre d'agents actifs sans un N° CNSSS : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.Nsecu like '' and a.Nsecu not null)")
        If Not FilmyCbx("select a.NomPrenoms from agents a order by a.Actif desc, a.NomPrenoms, a.NumMatr ", cbxAgent) Then cbxAgent.Items.Clear()
        If Not FilmyCbx("select a.Nsecu from agents a", iCbxNsecu) Then iCbxNsecu.Items.Clear()
        btnValidNewData.Visibility = Visibility.Collapsed
        'MsgBox(iCbxNsecu.Items.Item(0))
        'select a.Banque||a.NumCompte from agents a where a.Banque not like'' and a.NumCompte not like'' order by a.Banque 
    End Sub

    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimer.Stop()
        Dispatcher.Invoke(Sub() SearchNumCNSS(BtnAgentListeSearch.Text))
    End Sub
    Sub SearchNumCNSS(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible

        Dim sufix As String = ""
        If KeyWord = "" Then
            sufix = " and a.Actif=1 "
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = ""
        Else
            sufix = ""
        End If
        Dim istr As String = " select a.NomPrenoms,a.NumMatr, a.Nsecu,  a.Contacts,a.Actif from agents a where "
        istr += " (a.NomPrenoms  like '%" & KeyWord & "%' "
        istr += " or a.Nsecu  like '%" & KeyWord & "%' "
        istr += " or a.NumMatr  like '%" & KeyWord & "%' "
        istr += " or a.Contacts  like '%" & KeyWord & "%' )" & sufix
        istr += " order by a.Actif desc, a.NomPrenoms "
        Dgcnss.ItemsSource = LoadDataTable(istr).DefaultView
        ShowTempMsg(istr)
        tblNbcnss.Text = "Nombre d'agents ayant un N° CNSSS : " & MayStatVal("select count(*) from (" & istr & ")  where (Nsecu not like '' and Nsecu not null)") & " | Nombre d'agents sans un N° CNSSS : " & MayStatVal("select count(*) from (" & istr & ") where (Nsecu like '')")

        elpStatus.Visibility = Visibility.Collapsed

    End Sub
    Sub resetvalues()
        cbxAgent.SelectedIndex = -1
        cbxMatric.SelectedIndex = -1
        tbxFontion.Text = ""
        tbxSite.Text = ""
        tbxContact.Text = ""
        tbxNumSecu.Text = ""
    End Sub

    Private Sub BtnAgentListeSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentListeSearch.KeyDown
        If e.Key = Key.Enter Then
            inputTimer.Stop()
            SearchNumCNSS(BtnAgentListeSearch.Text)
        End If

    End Sub

    Private Sub cbxAgent_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxAgent.SelectionChanged
        If cbxAgent.SelectedIndex >= 0 Then
            If FilmyCbx($"select a.NumMatr from agents a where a.NomPrenoms ='{Apost(cbxAgent.SelectedItem)}' order by a.NumMatr ", cbxMatric) AndAlso cbxMatric.Items.Count >= 1 Then
                cbxMatric.SelectedIndex = 0
            End If
        End If
    End Sub

    Private Sub cbxMatric_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxMatric.SelectionChanged
        If cbxMatric.SelectedIndex >= 0 Then
            Dim Istr As String = $"select a.Fonction, f.SiteAffect,a.Contacts,a.Nsecu from agents a join affectation f on a.NomPrenoms=f.AgentAffect where a.NumMatr ='{cbxMatric.SelectedItem}' order by f.FinAffect desc limit 1"
            Try
                Using iconxx As New Data.SQLite.SQLiteConnection("Data Source=C:\LkyRess\ManA.sqlite;Version=3;")
                    iconxx.Open()
                    Using icmdd As New Data.SQLite.SQLiteCommand(Istr, iconxx)
                        Dim iReader As Data.SQLite.SQLiteDataReader = icmdd.ExecuteReader
                        'cbxBank.SelectedIndex = -1
                        tbxNumSecu.Text = ""
                        'iFormLoaded = False
                        While iReader.Read
                            If Not IsDBNull(iReader(0)) Then tbxFontion.Text = iReader(0)
                            If Not IsDBNull(iReader(1)) Then tbxSite.Text = iReader(1)
                            If Not IsDBNull(iReader(2)) Then tbxContact.Text = iReader(2)
                            'If Not IsDBNull(iReader(3)) Then cbxBank.SelectedItem = iReader(3)
                            If Not IsDBNull(iReader(3)) Then tbxNumSecu.Text = iReader(3)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
            'iFormLoaded = True
            'ActivValidBtn()
        End If

    End Sub

    Private Sub Dgcnss_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Dgcnss.MouseDoubleClick
        Dim rowView As Data.DataRowView = TryCast(Dgcnss.CurrentItem, Data.DataRowView)
        LoadRowItems(rowView)
    End Sub
    Sub LoadRowItems(rowView As Data.DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If Not IsNothing(rowView.Row.ItemArray(0).ToString) Then
                cbxAgent.SelectedItem = rowView.Row.ItemArray(0).ToString
                cbxMatric.SelectedItem = rowView.Row.ItemArray(1).ToString
                'tbxFontion.Text = rowView.Row.ItemArray(2).ToString
                'tbxSite.Text = rowView.Row.ItemArray(3).ToString
                'tbxContact.Text = rowView.Row.ItemArray(4).ToString
                'cbxBank.SelectedItem = rowView.Row.ItemArray(5).ToString
                'tbxNumCompt.Text = rowView.Row.ItemArray(6).ToString
                'CasValidation = 1
                btnValidNewData.Content = "MODIFIER"
            Else
                'CasValidation = 0
                btnValidNewData.Content = "AJOUTER"
            End If
        End If

        'MsgBox(iRIB)

        ActivValidBtn()
        'AfficheProfilUser()

    End Sub
    Sub ActivValidBtn()
        If cbxAgent.SelectedIndex >= 0 AndAlso cbxMatric.SelectedIndex >= 0 AndAlso iCbxNsecu.Items.Count > 0 Then
            If tbxNumSecu.Text.Trim.Length = 0 Then
                btnValidNewData.Visibility = Visibility.Visible
            ElseIf iCbxNsecu.Items.Contains(tbxNumSecu.Text.Trim) Or tbxNumSecu.Text.Trim.Length <= 5 Then
                btnValidNewData.Visibility = Visibility.Collapsed
            Else
                btnValidNewData.Visibility = Visibility.Visible
            End If
        Else
            btnValidNewData.Visibility = Visibility.Collapsed
        End If
        AfficheProfilUser()
    End Sub
    Sub AfficheProfilUser()
        If Not CanAddDataOrdinary Then
            btnValidNewData.Visibility = Visibility.Collapsed
        End If
        If Not CanModifyDataOrdinary Then
            btnValidNewData.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub tbxNumSecu_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxNumSecu.TextChanged
        ActivValidBtn()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        resetvalues()
    End Sub

    Private Sub btnValidNewData_Click(sender As Object, e As RoutedEventArgs) Handles btnValidNewData.Click
        If MessageBox.Show("Modifier ce Numéro ?", "MODIFICATION...", MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            Dim iStr As String = ""
            iStr = "update agents  set "
            iStr &= $" Nsecu='{ Apost(tbxNumSecu.Text.Trim)}' where NumMatr='{Apost(cbxMatric.SelectedItem)}'"
            If SaveNewData(iStr, iConnxStr) Then
                Joper("Modif Num CNSS de " & cbxAgent.SelectedItem, iConnxStr)
                MsgBox("MODIFICATION réalisée avec succès")
                Initilisation()
                resetvalues()
            Else
                ShowTempMsg("Echec d'enregistrement " & iStr)
            End If
        End If

    End Sub
End Class
