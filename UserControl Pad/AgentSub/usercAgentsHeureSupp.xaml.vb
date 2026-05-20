Imports System.Windows.Threading
Imports System.Data
Public Class usercAgentsHeureSupp
    Public iConnxStr As String
    Public LocalConnxStr As String
    Dim ChangeYN As Boolean = False
    Dim RefHS As Integer = 0
    Dim inputTimer As DispatcherTimer
    Dim NewData As Boolean = True
    Dim iFormLoaded As Boolean = False
    Dim retourDansMainWindow As Action
    Public Sub New(cllb As Action)
        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = cllb

    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        LocalConnxStr = $"Data Source={DbPath};Version=3;"
        Me.Cursor = Cursors.Wait
        InialiseLoad()
        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(3)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed

        Me.Cursor = Cursors.Arrow
        elpStatus.Visibility = Visibility.Collapsed

    End Sub
    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimer.Stop()
        ' Afficher le texte du TextBox
        Dispatcher.Invoke(Sub() SearchHS(BtnAgentAbsenceSearch.Text))
    End Sub

    Sub InialiseLoad()
        elpStatus.Visibility = Visibility.Visible
        NewData = True
        AfficheProfilUser()
        maj()
        Dim istr As String = " select h.Agent,format(h.DateHS) as DateHS ,h.NbFaction,SiteHS,h.Obs,h.IdHS  "
        istr += " from heuresup h join agents ag on ag.NomPrenoms=h.Agent "
        istr += " where ag.Actif=1 and h.DateHS>='" & CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")).ToString("yyyy-MM-dd") & "' "
        istr += " order by ag.Actif desc, h.DateHS desc,  h.Agent "
        DgAgentHeureSupp.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de ligne: " & MayStatVal("select count(*) from (" & istr & ")") ' & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "'") '") '& " | Nombre d'agents actif: " & NbAgentInactif()
        itext += Environment.NewLine
        'itext += MyContratList("select Type,count(*) from (" & istr & ") group by Type")
        Dim iMin, iMax, TxtResult As String
        iMax = MyStatValStr("select DateHS from (" & istr & ") order by DateHS  desc limit 1", LocalConnxStr)
        iMin = MyStatValStr("select DateHS from (" & istr & ") order by DateHS  limit 1", LocalConnxStr)
        If IsDate(iMax) AndAlso IsDate(iMin) Then
            TxtResult = "(Période du " & CDate(iMin).ToString("dd/MM/yyyy") & " au " & CDate(iMax).ToString("dd/MM/yyyy") & ")"
        Else
            TxtResult = "(Période du " & iMin & " au " & iMax & ")"
        End If
        tblNbAgentHeureSupp.Text = itext & TxtResult ' "Nombre de ligne:  : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")

        FilmyCbx("select a.NomPrenoms from agents a  where Actif=1 order by a.NomPrenoms", CbxNom, LocalConnxStr)
        FilmyCbx("select s.NomSite from siteaffectation s  where s.Actif=1 order by s.NomSite ", cbxSite, LocalConnxStr)

        CbxNbrFaction.Items.Add("1")
        CbxNbrFaction.Items.Add("2")

        RefHS = MayStatVal("select max (h.IdHS) from heuresup h  ") + 1
        dtpDebut.SelectedDate = Today
        dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        ChangeYN = False
        dtpDebutSearch.SelectedDate = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")) ' DebutDuMois(Today.AddMonths(-1)) ' CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = CDate("24/" & Today.ToString("MM/yyyy")) ' Today
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub ResetValues()
        elpStatus.Visibility = Visibility.Visible
        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed
        AfficheProfilUser()

        dtpDebut.SelectedDate = Today
        dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))


        TbxObs.Text = ""
        cbxSite.SelectedIndex = -1
        CbxNbrFaction.SelectedIndex = -1
        CbxNom.SelectedIndex = -1
        maj()
        Dim istr As String = " select h.Agent,format(h.DateHS)  ,h.NbFaction,SiteHS,h.Obs,h.IdHS  "
        istr += " from heuresup h join agents ag on ag.NomPrenoms=h.Agent "
        istr += " where ag.Actif=1 and h.DateHS>='" & DebutDuMois(Today.AddMonths(-1)).ToString("yyyy-MM-dd") & "' "
        istr += " order by ag.Actif desc, h.DateHS desc,  h.Agent "
        DgAgentHeureSupp.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de ligne: " & MayStatVal("select count(*) from (" & istr & ")", LocalConnxStr) ' & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "'") '") '& " | Nombre d'agents actif: " & NbAgentInactif()
        itext += Environment.NewLine
        'itext += MyContratList("select Type,count(*) from (" & istr & ") group by Type")
        Dim iMin, iMax, TxtResult As String
        iMax = MyStatValStr("select DateHS from (" & istr & ") order by DateHS  desc limit 1", LocalConnxStr)
        iMin = MyStatValStr("select DateHS from (" & istr & ") order by DateHS  limit 1", LocalConnxStr)
        If IsDate(iMax) AndAlso IsDate(iMin) Then
            TxtResult = "(Période du " & CDate(iMin).ToString("dd/MM/yyyy") & " au " & CDate(iMax).ToString("dd/MM/yyyy") & ")"
        Else
            TxtResult = "(Période du " & iMin & " au " & iMax & ")"
        End If

        tblNbAgentHeureSupp.Text = itext & TxtResult 'MyContratList("select n.IdNature, count(*) from contratstbl c join contratsnature n on c.NaturContr=n.NatureContr group by n.IdNature")
        RefHS = MayStatVal("select max(a.IdHS) from HeureSup a ", LocalConnxStr) + 1


        ChangeYN = False
        NewData = True
        iFormLoaded = True
        'MsgBox(RefHS)
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub SearchHS(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        Dim myw As New WaitFrm
        myw.Show()

        Dim sufix As String = ""
        If KeyWord = "" Then
            sufix = " and h.DateHS>='" & DebutDuMois(Today.AddMonths(-1)).ToString("yyyy-MM-dd") & "'"
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = " "
        Else
            sufix = ""
        End If
        maj()
        Dim istr As String = " select h.Agent, format(h.DateHS) as DateHS ,h.NbFaction, h.SiteHS,h.Obs,h.IdHS "
        istr += " from heuresup h join agents ag on ag.NomPrenoms=h.Agent where "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then istr += " DateHS>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateHS<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and "
        istr += "  (h.Agent like '%" & KeyWord & "%' "
        istr += " or h.IdHS  like '%" & KeyWord & "%' "
        istr += " or h.SiteHS  like '%" & KeyWord & "%' "
        istr += " or h.Obs  like '%" & KeyWord & "%' )" & sufix
        istr += " order by ag.Actif desc, h.DateHS desc,  h.Agent "
        DgAgentHeureSupp.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de ligne: " & MayStatVal("select count(*) from (" & istr & ") ") 'where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")") '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        itext += Environment.NewLine
        'itext += MyContratList("select Type,count(*) from (" & istr & ") group by Type")
        Dim iMin, iMax, TxtResult As String
        iMax = MyStatValStr("select DateHS from (" & istr & ") order by DateHS  desc limit 1", LocalConnxStr)
        iMin = MyStatValStr("select DateHS from (" & istr & ") order by DateHS  limit 1", LocalConnxStr)
        If IsDate(iMax) AndAlso IsDate(iMin) Then
            TxtResult = "(Recherche du " & CDate(iMin).ToString("dd/MM/yyyy") & " au " & CDate(iMax).ToString("dd/MM/yyyy") & ")"
        Else
            TxtResult = "(Recherche du " & iMin & " au " & iMax & ")"
        End If
        tblNbAgentHeureSupp.Text = itext & TxtResult ' " ( du " & iMin & " au " & iMax & ")" ' "Nombre de ligne:  : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")
        iFormLoaded = True
        myw.Close()
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Function SaveNewHS() As Boolean
        SaveNewHS = False
        Dim istr As String = " insert into heuresup "
        istr += "(IdHS, Agent, SiteHS, DateHS, NbFaction, Obs) values "
        istr += " ('" & RefHS & "', "
        istr += " '" & Apost(CbxNom.SelectedItem) & "', "
        istr += " '" & Apost(cbxSite.SelectedItem) & "', "
        istr += " '" & dtpDebut.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " " & CbxNbrFaction.SelectedItem & ", "
        istr += " '" & Apost(TbxObs.Text) & "') "
        'MsgBox(istr)
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveNewHS = SaveNewData(istr, iConnxStr)
        If SaveNewHS Then Joper("SaveNewHS for " & CbxNom.SelectedItem, iConnxStr)

    End Function

    Function SaveModifHS() As Boolean
        SaveModifHS = False
        Dim istr As String = " update heuresup  set "
        'istr += "(IdHS, Agent, SiteHS, DateHS, NbFaction, Obs) values "
        istr += " Agent='" & Apost(CbxNom.SelectedItem) & "', "
        istr += " SiteHS='" & Apost(cbxSite.SelectedItem) & "', "
        istr += " DateHS='" & dtpDebut.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " NbFaction=" & CbxNbrFaction.SelectedItem & ", "
        istr += " Obs='" & Apost(TbxObs.Text) & "'  "
        istr += " where IdHS=" & RefHS & " "
        'MsgBox(istr)
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveModifHS = SaveNewData(istr, iConnxStr)
        If SaveModifHS Then Joper("SaveModifHS for " & CbxNom.SelectedItem & " at " & RefHS, iConnxStr)

    End Function
    Function DeleteHS() As Boolean
        DeleteHS = False
        Dim istr As String = " delete from HeureSup "
        istr += " where IdHS=" & RefHS & " "
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        DeleteHS = SaveNewData(istr, iConnxStr)
        If DeleteHS Then Joper("DeleteAgent at " & RefHS, iConnxStr)

    End Function

    Private Sub CbxNom_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNom.SelectionChanged
        If CbxNom.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            CbxNumMatr.Visibility = Visibility.Visible
            TblIndicationNumMatr.Visibility = Visibility.Visible
            Dim iElmt As String = MyStatValStr("select a.SiteAffect from  affectation a where a.AgentAffect like '" & Apost(CbxNom.SelectedItem) & "' and a.FinAffect>='" & Today.AddDays(-21).ToString("yyyy-MM-dd") & "' order by a.DateAffect desc,a.IdAffec desc limit 1")
            If cbxSite.Items.Contains(iElmt) Then
                cbxSite.SelectedItem = iElmt
            Else
                cbxSite.SelectedIndex = -1
            End If
            'cbxSite.Text = MyStatValStr("select a.SiteAffect from  affectation a where a.AgentAffect like '" & Apost(CbxNom.SelectedItem) & "' and a.FinAffect>='" & Today.AddDays(-21).ToString("yyyy-MM-dd") & "' order by a.DateAffect desc,a.IdAffec desc limit 1")
        End If
        SaisiCbx(sender, TblIndicationNom)
        If CbxNom.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            If FilmyCbx("select a.NumMatr,a.NomPrenoms from agents a  where a.NomPrenoms like '" & Apost(CbxNom.SelectedItem) & "'  order by a.NumMatr", CbxNumMatr) Then
                CbxNumMatr.SelectedIndex = 0
                TblIndicationNumMatr.Visibility = Visibility.Collapsed
                CbxNumMatr.Visibility = Visibility.Visible
                'TblIndicationNumMatr.Visibility = Visibility.Visible
            Else
                CbxNumMatr.SelectedIndex = -1
                TblIndicationNumMatr.Visibility = Visibility.Visible
                CbxNumMatr.Visibility = Visibility.Collapsed
                TblIndicationNumMatr.Visibility = Visibility.Collapsed
            End If
        End If
        ActivSaveBtn()


    End Sub
    Sub ActivSaveBtn()
        Dim miDateRef As Date = dtpDebut.SelectedDate.Value
        If RefHS >= 1 And cbxSite.Items.Contains(cbxSite.SelectedItem) And CbxNom.Items.Contains(CbxNom.SelectedItem) And CbxNumMatr.Items.Contains(CbxNumMatr.SelectedItem) And CbxNbrFaction.Items.Contains(CbxNbrFaction.SelectedItem) Then
            btnSave.Visibility = Visibility.Visible
            btnSaveContinue.Visibility = Visibility.Visible
        Else
            btnSave.Visibility = Visibility.Collapsed
            btnSaveContinue.Visibility = Visibility.Collapsed
        End If
        ChangeYN = True
        stpButtonsPad.Visibility = Visibility.Visible

    End Sub

    Private Sub cbxSite_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxSite.SelectionChanged
        ActivSaveBtn()
    End Sub

    Private Sub CbxNbrFaction_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNbrFaction.SelectionChanged
        ActivSaveBtn()
    End Sub

    Private Sub TbxObs_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxObs.TextChanged
        SaisiTbx(sender, TblIndicationObs)
        ActivSaveBtn()
    End Sub

    Private Sub dtpDebut_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebut.SelectedDateChanged
        ActivSaveBtn()

    End Sub

    Private Sub btnNewData_Click(sender As Object, e As RoutedEventArgs) Handles btnNewData.Click
        stpSaisie.Visibility = Visibility.Visible
        btnNewData.Visibility = Visibility.Collapsed
        NewData = True

    End Sub

    Private Sub btnAnnule_Click(sender As Object, e As RoutedEventArgs) Handles btnAnnule.Click
        If ChangeYN Then
            If MsgBox("ANNULER VRAIMENT ?", MsgBoxStyle.YesNo, "ATTENTION") = MsgBoxResult.Yes Then
                ResetValues()
                stpSaisie.Visibility = Visibility.Collapsed
                stpSaisie.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
                'btnDeleteAg.Visibility = Visibility.Collapsed
            End If
        Else
            stpSaisie.Visibility = Visibility.Collapsed
            stpSaisie.Visibility = Visibility.Collapsed
            btnNewData.Visibility = Visibility.Visible
            'btnDeleteAg.Visibility = Visibility.Collapsed
            ResetValues()
            NewData = True

        End If

    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDebut.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDebut.Focus()
                Exit Sub
            End If
        End If
        If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If NewData Then
                Try
                    If SaveNewHS() Then
                        MsgBox("Données enregistrer avec succès")
                        stpSaisie.Visibility = Visibility.Collapsed
                        btnNewData.Visibility = Visibility.Visible
                        ResetValues()
                    Else
                        MsgBox("Erreur d'enregistrement")
                        stpSaisie.Visibility = Visibility.Visible
                        btnNewData.Visibility = Visibility.Collapsed
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            Else
                Try
                    If SaveModifHS() Then
                        MsgBox("Données modifier avec succès")
                        stpSaisie.Visibility = Visibility.Collapsed
                        btnNewData.Visibility = Visibility.Visible
                        ResetValues()
                    Else
                        MsgBox("Erreur d'enregistrement")
                        stpSaisie.Visibility = Visibility.Visible
                        btnNewData.Visibility = Visibility.Collapsed
                    End If
                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            End If

        End If

    End Sub

    Private Sub BtnAgentAbsenceSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentAbsenceSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()
    End Sub

    Private Sub btnSaveContinue_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveContinue.Click
        If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDebut.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDebut.Focus()
                Exit Sub
            End If
        End If
        If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If NewData Then
                Try
                    If SaveNewHS() Then
                        MsgBox("Données enregistrer avec succès")
                        ResetValues()
                    Else
                        MsgBox("Erreur d'enregistrement")
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            Else
                Try
                    If SaveModifHS() Then
                        MsgBox("Données modifier avec succès")
                        ResetValues()
                    Else
                        MsgBox("Erreur d'enregistrement")
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            End If

        End If
    End Sub

    Private Sub DgAgentHeureSupp_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgAgentHeureSupp.MouseDoubleClick

        Dim rowView As DataRowView = TryCast(DgAgentHeureSupp.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & CbxNom.SelectedItem & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If DeleteHS() Then
                MsgBox("Donnée supprimées avec succès ")
                ResetValues()
            Else
                MsgBox("Erreur de suppression")
            End If
        End If
    End Sub

    Private Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click

        Dim rowView As DataRowView = TryCast(DgAgentHeureSupp.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefHS = rowView.Row.ItemArray(5).ToString
            If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & rowView.Row.ItemArray(0).ToString & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                If DeleteHS() Then
                    MsgBox("Donnée supprimées avec succès ")
                    ResetValues()
                Else
                    MsgBox("Erreur de suppression")
                End If
            End If

        End If

    End Sub

    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs) Handles cmiModif.Click
        Dim rowView As DataRowView = TryCast(DgAgentHeureSupp.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If CbxNom.Items.Contains(rowView.Row.ItemArray(0).ToString) Then
                CbxNom.SelectedItem = rowView.Row.ItemArray(0).ToString
                dtpDebut.SelectedDate = rowView.Row.ItemArray(1).ToString
                CbxNbrFaction.SelectedItem = rowView.Row.ItemArray(2).ToString
                cbxSite.SelectedItem = rowView.Row.ItemArray(3).ToString
                TbxObs.Text = rowView.Row.ItemArray(4).ToString
                RefHS = rowView.Row.ItemArray(5).ToString
                stpSaisie.Visibility = Visibility.Visible
                btnDeleteAg.Visibility = Visibility.Visible
                NewData = False
                btnSave.Content = "Modifier et fermer"
                btnSaveContinue.Content = "Modifier et Continuer"
                btnDeleteAg.Visibility = Visibility.Visible
                btnNewData.Visibility = Visibility.Collapsed
            Else
                stpSaisie.Visibility = Visibility.Collapsed
                btnDeleteAg.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
                NewData = True
                btnSave.Content = "Enregistrer et fermer"
                btnSaveContinue.Content = "Enregistrer et ajouter un autre"
            End If
        End If
        AfficheProfilUser()

    End Sub

    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs) Handles cmiResume.Click
        Dim rowView As DataRowView = TryCast(DgAgentHeureSupp.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeHS(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd)
        End If

    End Sub

    Private Sub BtnAgentAbsenceSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentAbsenceSearch.KeyDown
        If e.Key = Key.Enter Then
            inputTimer.Stop()
            SearchHS(BtnAgentAbsenceSearch.Text)
        End If
    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then SearchHS(BtnAgentAbsenceSearch.Text)
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Heures supplémentaires "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Heures supplémentaires du " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgAgentHeureSupp, "Point_HeuresSuppl", iTitre) '"Point_Entrees"

    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        InialiseLoad()
    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des HEURES SUPPLEMENTAIRES", "A ce jour " & Today, Me)
        AddHandler iDetailW.Closed, Sub()
                                        retourDansMainWindow?.Invoke()
                                        btnAgrandir.Visibility = Visibility.Visible
                                    End Sub
        iDetailW.WindowStartupLocation = WindowStartupLocation.CenterOwner
        iDetailW.Topmost = True
        Dim iWtf As New WaitFrm
        iWtf.Show()
        iDetailW.Show()
        iWtf.Close()

    End Sub

    Private Sub btnPointG_Click(sender As Object, e As RoutedEventArgs) Handles btnPointG.Click
        Dim debut As Date = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy"))
        Dim Fin As Date = CDate("24/" & Today.ToString("MM/yyyy"))
        If dtpDebutSearch.SelectedDate.HasValue Then debut = dtpDebutSearch.SelectedDate
        If dtpFinSearch.SelectedDate.HasValue Then Fin = dtpFinSearch.SelectedDate
        Dim istr As String = "SELECT s.Client, h.Agent, date(h.DateHS) AS Debut, date(h.DateHS) AS Fin, h.NbFaction AS Durée, h.Obs "
        istr &= " FROM SiteAffectation s JOIN heuresup h ON h.SiteHS = s.NomSite "
        istr &= "  WHERE h.DateHS >='" & debut.ToString("yyyy-MM-dd") & "' and h.DateHS <='" & Fin.ToString("yyyy-MM-dd") & "'"   ' date('now','-1 month','start of month','+24 day')  "
        'istr &= " and aff.FinAffect >='" & debut.ToString("yyyy-MM-dd") & "'"  'date('now','-1 month','start of month','+24 day')"
        'istr &= " and a.Actif=0"
        'istr &= " GROUP BY s.Client, a.NomPrenoms  "
        istr &= "  ORDER BY s.Client, h.Agent, h.DateHS"
        'ShowTempMsg(istr)
        openPointMensuel(debut, Fin, istr, "Heures Supp. du " & debut.ToString("dd/MM/yyyy") & " au " & Fin.ToString("dd/MM/yyyy"), "HEURES_SUP")

    End Sub
    Sub AfficheProfilUser()
        If Not CanAddDataOrdinary Then
            btnNewData.Visibility = Visibility.Collapsed
            btnSave.Visibility = Visibility.Collapsed
            btnSaveContinue.Visibility = Visibility.Collapsed
        End If
        If Not CanModifyDataOrdinary Then
            If btnSave.Content = "Modifier et fermer" Then btnSave.Visibility = Visibility.Collapsed
            If btnSaveContinue.Content = "Modifier et Continuer" Then btnSaveContinue.Visibility = Visibility.Collapsed
            cmiModif.Visibility = Visibility.Collapsed
        End If
        If Not CanDeleteDataOrdinary And Not CanGlobalDataManage Then
            btnDeleteAg.Visibility = Visibility.Collapsed
            cmiSuppr.Visibility = Visibility.Collapsed
        End If
    End Sub

End Class
