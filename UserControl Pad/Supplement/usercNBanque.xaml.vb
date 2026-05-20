Imports System.Windows.Threading
Public Class usercNBanque
    Dim inputTimer As DispatcherTimer
    Dim iConnxStr As String
    Dim iCbxRIB As New ComboBox
    Dim iFormLoaded As Boolean = False
    Dim iRIB As String = ""
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
        DbPathGeneral = (PaphBackUp())
        iConnxStr = $"Data Source={ DbPathGeneral};Version=3;"

        Initilisation()
        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(3)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub resetvalues()
        cbxAgent.SelectedIndex = -1
        cbxMatric.SelectedIndex = -1
        cbxBank.SelectedIndex = -1
        tbxFontion.Text = ""
        tbxSite.Text = ""
        tbxContact.Text = ""
        tbxNumCompt.Text = ""
    End Sub
    Sub Initilisation()
        Dim istr As String = " select a.NomPrenoms,a.NumMatr , a.Banque, a.NumCompte, a.Contacts,a.Actif from agents a where a.Actif=1 "
        istr += " order by a.Actif desc, a.NomPrenoms "
        'istr += " ORDER BY crt.DebutContr desc, ag.Nomprenoms "
        DgBanque.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbBanque.Text = "Nombre d'agents actifs ayant un compte bancaire : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.NumCompte not like '' and a.NumCompte not null)") & " | Nombre d'agents actifs n'ayant pas de compte bancaire : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.NumCompte like '' and a.NumCompte not null)")
        If Not FilmyCbx("select a.RefBank,a.Favorit from comptbank a where a.Actif=1 order by a.Favorit desc,a.RefBank ", cbxBank) Then cbxBank.Items.Clear()
        If Not FilmyCbx("select a.NomPrenoms from agents a order by a.Actif desc, a.NomPrenoms, a.NumMatr ", cbxAgent) Then cbxAgent.Items.Clear()
        If cbxBank.Items.Count > 0 Then cbxBank.Items.Insert(0, "")
        If Not fillRIB() Then iCbxRIB.Items.Clear()
        btnValidNewData.Visibility = Visibility.Collapsed
        'MsgBox(iCbxRIB.Items.Item(0))
        'select a.Banque||a.NumCompte from agents a where a.Banque not like'' and a.NumCompte not like'' order by a.Banque 
    End Sub
    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimer.Stop()
        Dispatcher.Invoke(Sub() SearchNumBanque(BtnAgentListeSearch.Text))
    End Sub
    Sub SearchNumBanque(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible

        Dim sufix As String = ""
        If KeyWord = "" Then
            sufix = " and  a.Actif=1"
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = ""
        Else
            sufix = ""
        End If
        Dim istr As String = " select a.NomPrenoms,a.NumMatr , a.Banque, a.NumCompte, a.Contacts,a.Actif from agents a where "
        istr += " (a.NomPrenoms  like '%" & KeyWord & "%' "
        istr += " or a.Banque  like '%" & KeyWord & "%' "
        istr += " or a.NumMatr  like '%" & KeyWord & "%' "
        istr += " or a.Contacts  like '%" & KeyWord & "%' "
        istr += " or a.NumCompte  like '%" & KeyWord & "%' )" & sufix
        istr += " order by a.Actif desc, a.NomPrenoms "
        DgBanque.ItemsSource = LoadDataTable(istr).DefaultView
        'ShowTempMsg(istr)
        tblNbBanque.Text = "Nombre d'agents ayant un compte bancaire : " & MayStatVal("select count(*) from (" & istr & ")  where (NumCompte not like '' and NumCompte not null)") & " | Nombre d'agents n'ayant pas de compte bancaire : " & MayStatVal("select count(*) from (" & istr & ") where (NumCompte like '' and NumCompte not null)")

        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Sub BtnAgentListeSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentListeSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()
    End Sub

    Private Sub BtnAgentListeSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentListeSearch.KeyDown
        If e.Key = Key.Enter Then
            inputTimer.Stop()
            SearchNumBanque(BtnAgentListeSearch.Text)
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
            Dim Istr As String = $"select a.Fonction, f.SiteAffect,a.Contacts,a.Banque,a.NumCompte from agents a join affectation f on a.NomPrenoms=f.AgentAffect where a.NumMatr ='{cbxMatric.SelectedItem}' order by f.FinAffect desc limit 1"
            Try
                Using iconxx As New Data.SQLite.SQLiteConnection("Data Source=C:\LkyRess\ManA.sqlite;Version=3;")
                    iconxx.Open()
                    Using icmdd As New Data.SQLite.SQLiteCommand(Istr, iconxx)
                        Dim iReader As Data.SQLite.SQLiteDataReader = icmdd.ExecuteReader
                        cbxBank.SelectedIndex = -1
                        tbxNumCompt.Text = ""
                        'iFormLoaded = False
                        While iReader.Read
                            If Not IsDBNull(iReader(0)) Then tbxFontion.Text = iReader(0)
                            If Not IsDBNull(iReader(1)) Then tbxSite.Text = iReader(1)
                            If Not IsDBNull(iReader(2)) Then tbxContact.Text = iReader(2)
                            If Not IsDBNull(iReader(3)) Then cbxBank.SelectedItem = iReader(3)
                            If Not IsDBNull(iReader(4)) Then tbxNumCompt.Text = iReader(4)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
            iFormLoaded = True
            'ActivValidBtn()
        End If
    End Sub
    Function fillRIB() As Boolean
        fillRIB = False
        Dim Istr As String = $"select a.Banque||a.NumCompte from agents a where a.Banque not like'' and a.NumCompte not like'' order by a.Banque"
        Try
            Using iconxx As New Data.SQLite.SQLiteConnection("Data Source=C:\LkyRess\ManA.sqlite;Version=3;")
                iconxx.Open()
                Using icmdd As New Data.SQLite.SQLiteCommand(Istr, iconxx)
                    Dim iReader As Data.SQLite.SQLiteDataReader = icmdd.ExecuteReader
                    iCbxRIB.Items.Clear()

                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then iCbxRIB.Items.Add(EffaceToutEspace(iReader(0)))
                    End While
                    If iCbxRIB.Items.Count >= 1 Then fillRIB = True
                End Using
            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Function

    Private Sub cbxBank_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxBank.SelectionChanged
        If iFormLoaded Then
            If cbxBank.SelectedIndex <= 0 Then
                tbxNumCompt.Text = ""
                tbxNumCompt.IsEnabled = False
            Else
                tbxNumCompt.IsEnabled = True
            End If
            MyRIB()
        End If
        ActivValidBtn()
    End Sub

    Private Sub DgBanque_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgBanque.MouseDoubleClick
        Dim rowView As Data.DataRowView = TryCast(DgBanque.CurrentItem, Data.DataRowView)
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
    Sub MyRIB()
        If iFormLoaded Then iRIB = EffaceToutEspace(cbxBank.Text & tbxNumCompt.Text)
        'MsgBox(iRIB)
    End Sub
    Sub ActivValidBtn()
        MyRIB()
        If cbxBank.SelectedIndex >= 0 AndAlso cbxMatric.SelectedIndex >= 0 AndAlso iCbxRIB.Items.Count > 0 Then
            btnValidNewData.Visibility = Visibility.Visible
            If cbxBank.SelectedIndex > 0 And tbxNumCompt.Text.Trim.Length <= 2 Then btnValidNewData.Visibility = Visibility.Collapsed
        Else
            btnValidNewData.Visibility = Visibility.Collapsed
        End If
        AfficheProfilUser()
    End Sub

    Private Sub tbxNumCompt_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxNumCompt.TextChanged
        ActivValidBtn()
    End Sub

    Private Sub btnValidNewData_Click(sender As Object, e As RoutedEventArgs) Handles btnValidNewData.Click
        If MessageBox.Show("Modifier ce compte ?", "MODIFICATION...", MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.Yes Then
            Dim iStr As String = ""
            iStr = "update agents  set "
            iStr &= $"Banque='{Apost(cbxBank.SelectedItem)}', NumCompte='{ Apost(tbxNumCompt.Text)}' where NumMatr='{Apost(cbxMatric.SelectedItem)}'"
            If SaveNewData(iStr, iConnxStr) Then
                Joper("Modif new Agent Compte de " & cbxAgent.SelectedItem, iConnxStr)
                MsgBox("MODIFICATION réalisée avec succès")
                Initilisation()
                resetvalues()
            Else
                ShowTempMsg("Echec d'enregistrement " & iStr)
            End If
        End If

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        resetvalues()
    End Sub
    Sub AfficheProfilUser()
        If Not CanAddDataSpecial Then
            btnValidNewData.Visibility = Visibility.Collapsed
        End If
        If Not CanModifyDataSpecial Then
            btnValidNewData.Visibility = Visibility.Collapsed
        End If
    End Sub

End Class
