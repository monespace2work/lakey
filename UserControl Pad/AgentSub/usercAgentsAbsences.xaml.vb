Imports System.Windows.Threading
Imports System.Data

Public Class usercAgentsAbsences
    Public iConnxStr As String
    Public LocalConnxStr As String
    Dim ChangeYN As Boolean = False
    Dim ChangeYNsbb As Boolean = False
    Dim RefAbs As Integer = 0
    Dim inputTimer As DispatcherTimer
    Dim NewData As Boolean = True
    Dim iFormLoaded As Boolean = False
    Dim iTrue As Boolean = True
    Dim retourDansMainWindow As Action
    Public Shared AfficheMemos As Action
    Public Sub New(cllb As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = cllb
    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
        DbPathGeneral = (PaphBackUp())
        iConnxStr = $"Data Source={DbPathGeneral};Version=3;"
        LocalConnxStr = $"Data Source={DbPath};Version=3;"
        'MsgBox(iConnxStr)
        InialiseLoad()
        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(3)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimer.Stop()
        Dispatcher.Invoke(Sub() SearchAbsence(BtnAgentAbsenceSearch.Text))
    End Sub

    Sub InialiseLoad()
        elpStatus.Visibility = Visibility.Visible
        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed
        NewData = True
        AfficheProfilUser()
        maj()
        AfficheMemos?.Invoke()

        Dim istr As String = " select ab.NomPrenom, date(ab.DateDebut) as Début,date(ab.DateFin) as Fin, (julianday(ab.DateFin) - julianday(ab.DateDebut) + 1) as Durée,ab.Motif,ab.Obs,ab.IdOp  "
        istr += " from absences ab join agents ag on ag.NomPrenoms=ab.NomPrenom where ab.DateDebut>='" & DebutDuMois(Today.AddMonths(-1)).ToString("yyyy-MM-dd") & "'"
        istr += " and ab.DateDebut >='" & CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")).ToString("yyyy-MM-dd") & "'"
        istr += " order by ag.Actif desc, ab.DateDebut desc, ab.NomPrenom "
        DgAgentAbsence.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de ligne: " & MayStatVal("select count(*) from absences ab where ab.DateDebut>='" & DebutDuMois(Today.AddMonths(-1)).ToString("yyyy-MM-dd") & "'") '") '& " | Nombre d'agents actif: " & NbAgentInactif()
        itext += Environment.NewLine
        'itext += MyContratList("select Type,count(*) from (" & istr & ") group by Type")
        tblNbAgentAbsence.Text = itext 'MyContratList("select n.IdNature, count(*) from contratstbl c join contratsnature n on c.NaturContr=n.NatureContr group by n.IdNature")
        'tblNbAgentcONTRAT.Text = "En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & Environment.NewLine & ComptTypeContrat("select Type, count(*) from (" & istr & ") group by Type") & "ok"
        istr = "SELECT A.NomPrenom,A.Motif,date(A.DebutDmd) Debut,date(A.FinDmd) Fin ,date(A.DateDmd) DateDmd,A.Obs,A.IdOp Ref FROM ABSENCES A WHERE A.StdBy=1 "
        istr += " ORDER BY A.Motif,A.DateDmd,  A.NomPrenom"
        dgEnAttente.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        FilmyCbx("select a.NomPrenoms from agents a  where Actif=1 order by a.NomPrenoms", CbxNom, LocalConnxStr)
        FilmyCbx("select a.Motif from AbsencesMotifs a where a.Actif=1 order by a.Favorit desc,a.Motif", CbxMotif, LocalConnxStr)
        FilmyCbx("select a.NomPrenoms from agents a  where Actif=1 order by a.NomPrenoms", CbxNomSdb, LocalConnxStr)
        FilmyCbx("select a.Motif from AbsencesMotifs a where a.Actif=1 order by a.Favorit desc,a.Motif", CbxMotifSdb, LocalConnxStr)
        'FilmyCbx("select a.NomPrenoms from agents a  where Actif=1 order by a.NomPrenoms", CbxNom)
        RefAbs = MayStatVal("select max(a.IdOp) from Absences a ", LocalConnxStr) + 1
        dtpDebut.SelectedDate = Today
        dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpDateDmdSdb.SelectedDate = Today
        dtpDateDmd.SelectedDate = Today

        dtpDebutSdb.SelectedDate = Today
        dtpDebutSdb.DisplayDateStart = DateCreationSociete
        dtpDebutSdb.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpFin.SelectedDate = Today
        dtpFin.DisplayDateStart = DateCreationSociete
        dtpFin.DisplayDateEnd = FinDuMois(Today.AddMonths(12))

        dtpFinSdb.SelectedDate = Today
        dtpFinSdb.DisplayDateStart = DateCreationSociete
        dtpFinSdb.DisplayDateEnd = FinDuMois(Today.AddMonths(12))

        ChangeYNsbb = False
        ChangeYN = False
        dtpDebutSearch.SelectedDate = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")) 'DebutDuMois(Today.AddMonths(-1)) 'CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = CDate("24/" & Today.ToString("MM/yyyy")) 'Today
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub ResetValues()
        elpStatus.Visibility = Visibility.Visible
        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed
        NewData = True
        AfficheProfilUser()

        dtpDateDmd.SelectedDate = Today
        dtpDebut.SelectedDate = Today
        dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpFin.SelectedDate = Today
        dtpFin.DisplayDateStart = DateCreationSociete
        dtpFin.DisplayDateEnd = FinDuMois(Today.AddMonths(12))

        TbxObs.Text = ""
        TbxSite.Text = ""
        CbxMotif.SelectedIndex = -1
        'CbxNumMatr.SelectedIndex = -1
        CbxNom.SelectedIndex = -1
        'CbxNom.Text = ""
        AfficheMemos?.Invoke()
        maj()
        Dim istr As String = " select ab.NomPrenom, format(ab.DateDebut) as Début,format(ab.DateFin) as Fin, (julianday(ab.DateFin) - julianday(ab.DateDebut) + 1) as Durée,ab.Motif,ab.Obs,ab.IdOp  "
        istr += " from absences ab join agents ag on ag.NomPrenoms=ab.NomPrenom where ab.DateDebut>='" & DebutDuMois(Today.AddMonths(-1)).ToString("yyyy-MM-dd") & "'"
        istr += " order by ag.Actif desc, ab.DateDebut desc, ab.NomPrenom "
        DgAgentAbsence.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de ligne: " & MayStatVal("select count(*) from absences ab where ab.DateDebut>='" & DebutDuMois(Today.AddMonths(-1)).ToString("yyyy-MM-dd") & "'", LocalConnxStr) '& " | Nombre d'agents actif: " & NbAgentInactif()
        itext += Environment.NewLine
        'itext += MyContratList("select Type,count(*) from (" & istr & ") group by Type")
        tblNbAgentAbsence.Text = itext 'MyContratList("select n.IdNature, count(*) from contratstbl c join contratsnature n on c.NaturContr=n.NatureContr group by n.IdNature")
        RefAbs = MayStatVal("select max(a.IdOp) from Absences a ", LocalConnxStr) + 1
        ResetValuesSdb()
        ChangeYN = False
        elpStatus.Visibility = Visibility.Collapsed
        iFormLoaded = True

    End Sub
    Sub ResetValuesSdb()
        elpStatus.Visibility = Visibility.Visible
        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed
        NewData = True
        iFormLoaded = False

        dtpDateDmdSdb.SelectedDate = Today
        dtpDebutSdb.SelectedDate = Today
        dtpDebutSdb.DisplayDateStart = DateCreationSociete
        dtpDebutSdb.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpFinSdb.SelectedDate = Today
        dtpFinSdb.DisplayDateStart = DateCreationSociete
        dtpFinSdb.DisplayDateEnd = FinDuMois(Today.AddMonths(12))

        TbxObsSdb.Text = ""
        CbxMotifSdb.SelectedIndex = -1
        CbxNomSdb.SelectedIndex = -1

        AfficheMemos?.Invoke()

        maj()
        Dim istr As String = "SELECT A.NomPrenom,A.Motif,date(A.DebutDmd) Debut,date(A.FinDmd) Fin ,date(A.DateDmd) DateDmd,A.Obs,A.IdOp Ref FROM ABSENCES A WHERE A.StdBy=1 "
        istr += " ORDER BY A.Motif,A.DateDmd,  A.NomPrenom"
        dgEnAttente.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView

        RefAbs = MayStatVal("select max(a.IdOp) from Absences a ", LocalConnxStr) + 1
        btnExtend.Content = ">"
        stpDecision.Visibility = Visibility.Collapsed
        expDecisionSdb.Visibility = Visibility.Collapsed
        ChangeYNsbb = False
        elpStatus.Visibility = Visibility.Collapsed
        iFormLoaded = True

    End Sub

    Sub SearchAbsence(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        Dim myw As New WaitFrm
        myw.Show()
        Dim sufix As String = ""
        If KeyWord = "" Then
            sufix = " and ab.DateDebut>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "'"
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = " "
        Else
            sufix = ""
        End If
        maj()
        Dim istr As String = " select ab.NomPrenom, format(ab.DateDebut) as Début,format(ab.DateFin) as Fin, (julianday(ab.DateFin) - julianday(ab.DateDebut) + 1) as Durée,ab.Motif,ab.Obs,ab.IdOp  "
        istr += " from absences ab join agents ag on ag.NomPrenoms=ab.NomPrenom where "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then istr += " Début>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and Début<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and "
        istr += " (ab.NomPrenom  like '%" & KeyWord & "%' "
        istr += " or ab.Motif  like '%" & KeyWord & "%' "
        istr += " or ab.Obs  like '%" & KeyWord & "%' )" & sufix
        istr += " order by ag.Actif desc, ab.DateDebut desc, ab.NomPrenom "
        DgAgentAbsence.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de ligne: " & MayStatVal("select count(*) from (" & istr & ") ", LocalConnxStr) 'where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")") '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        itext += Environment.NewLine
        'itext += MyContratList("select Type,count(*) from (" & istr & ") group by Type")
        tblNbAgentAbsence.Text = itext ' "Nombre de ligne:  : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")
        myw.Close()
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Sub CbxNom_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNom.SelectionChanged
        If CbxNom.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            CbxNumMatr.Visibility = Visibility.Visible
            TblIndicationNumMatr.Visibility = Visibility.Visible
            TbxSite.Text = MyStatValStr("select a.SiteAffect from  affectation a where a.AgentAffect like '" & Apost(CbxNom.SelectedItem) & "' and a.FinAffect>='" & Today.AddDays(-21).ToString("yyyy-MM-dd") & "' order by a.DateAffect desc,a.IdAffec desc limit 1", LocalConnxStr)
        End If
        SaisiCbx(sender, TblIndicationNom)
        If CbxNom.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            If FilmyCbx("select a.NumMatr,a.NomPrenoms from agents a  where a.NomPrenoms like '" & Apost(CbxNom.SelectedItem) & "'  order by a.NumMatr", CbxNumMatr, LocalConnxStr) Then
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
        If RefAbs >= 1 And CbxMotif.Items.Contains(CbxMotif.SelectedItem) And CbxNom.Items.Contains(CbxNom.SelectedItem) And CbxNumMatr.Items.Contains(CbxNumMatr.SelectedItem) And (dtpFin.SelectedDate.HasValue AndAlso dtpFin.SelectedDate.Value >= miDateRef) Then
            btnSave.Visibility = Visibility.Visible
            btnSaveContinue.Visibility = Visibility.Visible
            'btnValideDecision.Visibility = Visibility.Visible
        Else
            btnSave.Visibility = Visibility.Collapsed
            btnSaveContinue.Visibility = Visibility.Collapsed
            'btnValideDecision.Visibility = Visibility.Collapsed
        End If
        ChangeYN = True
        stpButtonsPad.Visibility = Visibility.Visible

    End Sub
    Sub ActivSaveBtnSdb()
        Dim miDateRef As Date = dtpDebutSdb.SelectedDate.Value
        If RefAbs >= 1 And CbxMotifSdb.Items.Contains(CbxMotifSdb.SelectedItem) And CbxNomSdb.Items.Contains(CbxNomSdb.SelectedItem) And CbxNumMatrSdb.Items.Contains(CbxNumMatrSdb.SelectedItem) And (dtpFinSdb.SelectedDate.HasValue AndAlso dtpFinSdb.SelectedDate.Value >= miDateRef) Then
            chbEnAttente.Visibility = Visibility.Visible
            btnSaveSdb.Visibility = Visibility.Visible
            'expDecisionSdb.Visibility = Visibility.Visible
        Else
            chbEnAttente.Visibility = Visibility.Collapsed
            btnSaveSdb.Visibility = Visibility.Collapsed
            'expDecisionSdb.Visibility = Visibility.Collapsed
        End If
        ChangeYNsbb = True
        'stpButtonsPad.Visibility = Visibility.Visible

    End Sub

    Private Sub CbxMotif_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxMotif.SelectionChanged
        SaisiCbx(sender, TblIndicationMotif)
        ActivSaveBtn()
    End Sub

    Private Sub dtpDebut_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebut.SelectedDateChanged
        If dtpDebut.SelectedDate.HasValue Then
            Dim refDate As Date = dtpDebut.SelectedDate.Value
            dtpFin.DisplayDateStart = refDate
            If dtpFin.SelectedDate.HasValue AndAlso dtpFin.SelectedDate.Value < refDate Then
                dtpFin.SelectedDate = refDate
            End If
        End If
        dtpFin.DisplayDateStart = dtpDebut.SelectedDate.Value
        dtpFin.DisplayDate = dtpDebut.SelectedDate.Value
        dtpFin.Focus()
        TbxDuree.Text = NbJrs(dtpDebut, dtpFin)
        ActivSaveBtn()
    End Sub
    Function NbJrs(DebutDpk As DatePicker, finDpk As DatePicker) As Integer
        NbJrs = 0
        If DebutDpk.SelectedDate.HasValue AndAlso finDpk.SelectedDate.HasValue Then
            Dim nbj As Integer = DateDiff(DateInterval.Day, DebutDpk.SelectedDate.Value, finDpk.SelectedDate.Value) + 1
            NbJrs = nbj
        End If
    End Function
    Private Sub dtpFin_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpFin.SelectedDateChanged
        If dtpFin.SelectedDate.HasValue Then
            Dim idate As Date = dtpFin.SelectedDate.Value
            If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate.Value > idate Then dtpDebut.SelectedDate = idate
        End If
        ActivSaveBtn()
        TbxDuree.Text = NbJrs(dtpDebut, dtpFin)
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
                    If SaveNewAbsences() Then
                        MsgBox("Données enregistrer avec succès")
                        ResetValues()
                        stpSaisie.Visibility = Visibility.Collapsed
                        btnNewData.Visibility = Visibility.Visible
                        btnReduir.Visibility = Visibility.Collapsed
                    Else
                        stpSaisie.Visibility = Visibility.Visible
                        btnNewData.Visibility = Visibility.Collapsed
                        btnReduir.Visibility = Visibility.Visible
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            Else
                Try
                    If SaveModifAbs() Then
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
    Function SaveNewAbsences() As Boolean
        SaveNewAbsences = False
        Dim istr As String = " insert into Absences "
        istr += "(IdOp, NomPrenom, DateDebut, DateFin, Motif,OpTime,StdBy, Obs) values "
        istr += " ('" & RefAbs & "', "
        istr += " '" & Apost(CbxNom.SelectedItem) & "', "
        istr += " '" & (dtpDebut.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        istr += " '" & (dtpFin.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        istr += " '" & Apost(CbxMotif.SelectedItem) & "', "
        istr += " '" & Now.ToString("yyyy-MM-dd HH:mm:ss") & "', "
        istr += "0, " '" & Apost(TbxNumCrt.Text) & "', "
        istr += " '" & Apost(TbxObs.Text) & "') "
        'MsgBox(istr)
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveNewAbsences = SaveNewData(istr, iConnxStr)
        If SaveNewAbsences Then Joper("SaveNewAbsences for " & CbxNom.SelectedItem, iConnxStr)
    End Function

    Function SaveNewAbsencesStb() As Boolean
        SaveNewAbsencesStb = False
        Dim istr As String = " insert into Absences "
        istr += "(IdOp, NomPrenom, DebutDmd, FinDmd, Motif,DateDmd,StdBy, Obs) values "
        istr += " ('" & RefAbs & "', "
        istr += " '" & Apost(CbxNomSdb.SelectedItem) & "', "
        istr += " '" & (dtpDebutSdb.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        istr += " '" & (dtpFinSdb.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        istr += " '" & Apost(CbxMotifSdb.SelectedItem) & "', "
        istr += " '" & dtpDateDmdSdb.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += "1, " '" & Apost(TbxNumCrt.Text) & "', "
        istr += " '" & Apost(TbxObsSdb.Text) & "') "
        'MsgBox(istr)
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveNewAbsencesStb = SaveNewData(istr, iConnxStr)
        If SaveNewAbsencesStb Then Joper("SaveNewAbsencesStb for " & CbxNom.SelectedItem, iConnxStr)
    End Function

    Function SaveModifAbs() As Boolean
        SaveModifAbs = False
        Dim istr As String = " update Absences  set "
        'istr += "(IdHS, Agent, SiteHS, DateHS, NbFaction, Obs) values "
        istr += " NomPrenom='" & Apost(CbxNom.SelectedItem) & "', "
        istr += " DateDebut='" & dtpDebut.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " DateFin='" & dtpFin.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " Motif='" & Apost(CbxMotif.SelectedItem) & "', "
        'istr += " DateDmd='" & dtpDateDmdSdb.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " Obs='" & Apost(TbxObs.Text) & "', StdBy=0 "
        istr += " where IdOp=" & RefAbs & " "
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveModifAbs = SaveNewData(istr, iConnxStr)
        If SaveModifAbs Then Joper("SaveModifAbs for " & CbxNom.SelectedItem & " at " & RefAbs, iConnxStr)

    End Function
    Function SaveModifAbsStb() As Boolean
        SaveModifAbsStb = False
        Dim istr As String = " update Absences  set "
        'istr += "(IdHS, Agent, SiteHS, DateHS, NbFaction, Obs) values "
        istr += " NomPrenom='" & Apost(CbxNomSdb.SelectedItem) & "', "
        istr += " DebutDmd='" & dtpDebutSdb.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " FinDmd='" & dtpFinSdb.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " DateDmd='" & dtpDateDmdSdb.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " Motif='" & Apost(CbxMotifSdb.SelectedItem) & "', "
        istr += " Obs='" & Apost(TbxObsSdb.Text) & "'  "
        istr += " where IdOp=" & RefAbs & " "
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveModifAbsStb = SaveNewData(istr, iConnxStr)
        If SaveModifAbsStb Then Joper("SaveModifAbsStb for " & CbxNom.SelectedItem & " at " & RefAbs, iConnxStr)

    End Function


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
                    If SaveNewAbsences() Then
                        MsgBox("Données enregistrer avec succès")
                        ResetValues()
                        'stpSaisie.Visibility = Visibility.Collapsed
                        'btnNewData.Visibility = Visibility.Visible
                    Else
                        ' stpSaisie.Visibility = Visibility.Visible
                        ' btnNewData.Visibility = Visibility.Collapsed
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            Else
                Try
                    If SaveModifAbs() Then
                        MsgBox("Données modifier avec succès")
                        'stpSaisie.Visibility = Visibility.Collapsed
                        'btnNewData.Visibility = Visibility.Visible
                        ResetValues()
                    Else
                        MsgBox("Erreur d'enregistrement")
                        'stpSaisie.Visibility = Visibility.Visible
                        'btnNewData.Visibility = Visibility.Collapsed
                    End If
                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            End If


        End If

    End Sub

    Private Sub TbxObs_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxObs.TextChanged
        SaisiTbx(sender, TblIndicationObs)
        ActivSaveBtn()
    End Sub

    Private Sub btnNewData_Click(sender As Object, e As RoutedEventArgs) Handles btnNewData.Click
        stpSaisie.Visibility = Visibility.Visible
        btnNewData.Visibility = Visibility.Collapsed
        btnReduir.Visibility = Visibility.Visible

    End Sub

    Private Sub btnAnnule_Click(sender As Object, e As RoutedEventArgs) Handles btnAnnule.Click
        If ChangeYN Then
            If MsgBox("ANNULER VRAIMENT ?", MsgBoxStyle.OkCancel, "ATTENTION") = MsgBoxResult.Ok Then
                ResetValues()
                stpSaisie.Visibility = Visibility.Collapsed
                stpSaisie.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
            End If
        Else
            stpSaisie.Visibility = Visibility.Collapsed
            stpSaisie.Visibility = Visibility.Collapsed
            btnNewData.Visibility = Visibility.Visible
            ResetValues()
            NewData = True
            'btpPlus.Content = " > "

        End If
        btnDeleteAg.Visibility = Visibility.Collapsed
    End Sub

    Private Sub BtnAgentAbsenceSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentAbsenceSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()
    End Sub

    Private Sub DgAgentAbsence_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgAgentAbsence.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgAgentAbsence.CurrentItem, DataRowView)
        LoadRowItems(rowView)

    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If CbxNom.Items.Contains(rowView.Row.ItemArray(0).ToString) Then
                CbxNom.SelectedItem = rowView.Row.ItemArray(0).ToString
                dtpDebut.SelectedDate = rowView.Row.ItemArray(1).ToString
                dtpFin.SelectedDate = rowView.Row.ItemArray(2).ToString
                CbxMotif.SelectedItem = rowView.Row.ItemArray(4).ToString
                TbxObs.Text = rowView.Row.ItemArray(5).ToString
                RefAbs = rowView.Row.ItemArray(6).ToString
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
    Sub LoadRowItemsSdb(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If CbxNomSdb.Items.Contains(rowView.Row.ItemArray(0).ToString) Then
                CbxNomSdb.SelectedItem = rowView.Row.ItemArray(0).ToString
                dtpDebutSdb.SelectedDate = rowView.Row.ItemArray(2).ToString
                dtpFinSdb.SelectedDate = rowView.Row.ItemArray(3).ToString
                dtpDateDmdSdb.SelectedDate = rowView.Row.ItemArray(4).ToString
                CbxMotifSdb.SelectedItem = rowView.Row.ItemArray(1).ToString
                'cbxSite.SelectedItem = rowView.Row.ItemArray(3).ToString
                TbxObsSdb.Text = rowView.Row.ItemArray(5).ToString
                RefAbs = rowView.Row.ItemArray(6).ToString
                stpSaisie.Visibility = Visibility.Visible
                btnDeleteAg.Visibility = Visibility.Visible
                NewData = False

                chbEnAttente.IsChecked = True
                chbNonAccord.IsChecked = True

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

    Private Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & CbxNom.SelectedItem & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If DeleteAbs() Then
                MsgBox("Donnée supprimées avec succès ")
                ResetValues()
            Else
                MsgBox("Erreur de suppression")
            End If
        End If

    End Sub
    Function DeleteAbs() As Boolean
        DeleteAbs = False
        Dim istr As String = " delete from Absences "
        istr += " where IdOp=" & RefAbs & " "
        DeleteAbs = SaveNewData(istr, iConnxStr)
        If DeleteAbs Then Joper("DeleteAbs for " & CbxNom.SelectedItem & " at " & RefAbs, iConnxStr)
    End Function

    Private Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgAgentAbsence.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefAbs = rowView.Row.ItemArray(5).ToString
            If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & rowView.Row.ItemArray(0).ToString & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                If DeleteAbs() Then
                    MsgBox("Donnée supprimées avec succès ")
                    ResetValues()
                Else
                    MsgBox("Erreur de suppression")
                End If
            End If

        End If

    End Sub

    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs) Handles cmiResume.Click
        Dim rowView As DataRowView = TryCast(DgAgentAbsence.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAbs(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd)
        End If

    End Sub

    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs) Handles cmiModif.Click
        Dim rowView As DataRowView = TryCast(DgAgentAbsence.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub BtnAgentAbsenceSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentAbsenceSearch.KeyDown
        If e.Key = Key.Enter Then
            inputTimer.Stop()
            SearchAbsence(BtnAgentAbsenceSearch.Text)
        End If
    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then SearchAbsence(BtnAgentAbsenceSearch.Text)
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Absences/premissions"
        If dtpDebutSearch.SelectedDate.HasValue And dtpFin.SelectedDate.HasValue Then
            iTitre = "Absences/premissions du " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgAgentAbsence, "Point_Entrée-permissions", iTitre) '"Point_Entrees"

    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        InialiseLoad()
    End Sub

    Private Sub CbxNomSdb_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNomSdb.SelectionChanged
        If CbxNom.SelectedIndex = -1 Then
            CbxNumMatrSdb.Visibility = Visibility.Collapsed
            TblIndicationNumMatrSdb.Visibility = Visibility.Collapsed
        Else
            CbxNumMatrSdb.Visibility = Visibility.Visible
            TblIndicationNumMatrSdb.Visibility = Visibility.Visible
            'TbxSite.Text = MyStatValStr("select a.SiteAffect from  affectation a where a.AgentAffect like '" & Apost(CbxNom.SelectedItem) & "' and a.FinAffect>='" & Today.AddDays(-21).ToString("yyyy-MM-dd") & "' order by a.DateAffect desc,a.IdAffec desc limit 1")
        End If
        SaisiCbx(sender, TblIndicationNomSdb)
        If CbxNomSdb.SelectedIndex = -1 Then
            CbxNumMatrSdb.Visibility = Visibility.Collapsed
            TblIndicationNumMatrSdb.Visibility = Visibility.Collapsed
        Else
            If FilmyCbx("select a.NumMatr,a.NomPrenoms from agents a  where a.NomPrenoms like '" & Apost(CbxNomSdb.SelectedItem) & "'  order by a.NumMatr", CbxNumMatrSdb) Then
                CbxNumMatrSdb.SelectedIndex = 0
                TblIndicationNumMatrSdb.Visibility = Visibility.Collapsed
                CbxNumMatrSdb.Visibility = Visibility.Visible
                'TblIndicationNumMatr.Visibility = Visibility.Visible
            Else
                CbxNumMatrSdb.SelectedIndex = -1
                TblIndicationNumMatrSdb.Visibility = Visibility.Visible
                CbxNumMatrSdb.Visibility = Visibility.Collapsed
                TblIndicationNumMatrSdb.Visibility = Visibility.Collapsed
            End If
        End If
        ActivSaveBtnSdb()

    End Sub

    Private Sub CbxMotifSdb_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxMotifSdb.SelectionChanged
        SaisiCbx(sender, TblIndicationMotifSdb)
        ActivSaveBtnSdb()

    End Sub

    Private Sub btnExtend_Click(sender As Object, e As RoutedEventArgs) Handles btnExtend.Click
        If btnExtend.Content = ">" Then
            stpDecision.Visibility = Visibility.Visible
            btnExtend.Content = "<"
        Else
            stpDecision.Visibility = Visibility.Collapsed
            btnExtend.Content = ">"
        End If
    End Sub

    Private Sub chbEnAttente_Checked(sender As Object, e As RoutedEventArgs) Handles chbEnAttente.Checked
        If iFormLoaded Then
            expDecisionSdb.Visibility = Visibility.Collapsed
            chbAccord.IsChecked = False
            btnSaveSdb.Visibility = Visibility.Visible
        End If
        'DeactivAccord()
    End Sub

    Private Sub chbEnAttente_Unchecked(sender As Object, e As RoutedEventArgs) Handles chbEnAttente.Unchecked
        If iFormLoaded Then
            expDecisionSdb.Visibility = Visibility.Visible
            btnSaveSdb.Visibility = Visibility.Collapsed
        End If
    End Sub


    Private Sub chbAccord_Checked(sender As Object, e As RoutedEventArgs) Handles chbAccord.Checked
        ActivAccord()
        chbNonAccord.IsChecked = False
        'ActivSaveSdb()
    End Sub

    Private Sub chbAccord_Unchecked(sender As Object, e As RoutedEventArgs) Handles chbAccord.Unchecked
        DeactivAccord()
        chbNonAccord.IsChecked = True
        'ActivSaveSdb()
    End Sub

    Private Sub TbxObsSdb_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxObsSdb.TextChanged
        SaisiTbx(sender, TblIndicationObsSdb)
        ActivSaveBtnSdb()
    End Sub
    Sub DeactivAccord()
        If iFormLoaded Then
            TbxSite.Text = ""
            CbxNom.SelectedIndex = -1
            CbxMotif.SelectedIndex = -1
            dtpDebut.SelectedDate = Today
            dtpFin.SelectedDate = Today
            TbxObs.Text = ""
            btnExtend.Content = ">"
            stpDecision.Visibility = Visibility.Collapsed
        End If
    End Sub
    Sub ActivAccord()
        If iFormLoaded Then
            Dim MyVal As Integer = CbxNomSdb.SelectedIndex
            CbxNom.SelectedIndex = MyVal
            MyVal = CbxMotifSdb.SelectedIndex
            CbxMotif.SelectedIndex = MyVal
            If dtpDateDmdSdb.SelectedDate.HasValue Then dtpDateDmd.SelectedDate = dtpDateDmdSdb.SelectedDate.Value
            If dtpDebutSdb.SelectedDate.HasValue Then dtpDebut.SelectedDate = dtpDebutSdb.SelectedDate.Value
            If dtpFinSdb.SelectedDate.HasValue Then dtpFin.SelectedDate = dtpFinSdb.SelectedDate.Value
            TbxObs.Text = TbxObsSdb.Text
            btnExtend.Content = "<"
            stpDecision.Visibility = Visibility.Visible
        End If

    End Sub

    Private Sub dtpDebutSdb_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSdb.SelectedDateChanged
        If dtpDebutSdb.SelectedDate.HasValue Then
            Dim refDate As Date = dtpDebutSdb.SelectedDate.Value
            dtpFinSdb.DisplayDateStart = refDate
            If dtpFinSdb.SelectedDate.HasValue AndAlso dtpFinSdb.SelectedDate.Value < refDate Then
                dtpFinSdb.SelectedDate = refDate
            End If
        End If
        dtpFinSdb.DisplayDateStart = dtpDebut.SelectedDate.Value
        dtpFinSdb.DisplayDate = dtpDebut.SelectedDate.Value
        dtpFinSdb.Focus()
        TbxDureeSdb.Text = NbJrs(dtpDebutSdb, dtpFinSdb)
        ActivSaveBtn()

    End Sub

    Private Sub dtpFinSdb_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpFinSdb.SelectedDateChanged
        If dtpFinSdb.SelectedDate.HasValue Then
            Dim idate As Date = dtpFinSdb.SelectedDate.Value
            If dtpDebutSdb.SelectedDate.HasValue AndAlso dtpDebutSdb.SelectedDate.Value > idate Then dtpDebutSdb.SelectedDate = idate
        End If
        ActivSaveBtnSdb()
        TbxDureeSdb.Text = NbJrs(dtpDebutSdb, dtpFinSdb)
    End Sub

    Private Sub dgEnAttente_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles dgEnAttente.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(dgEnAttente.CurrentItem, DataRowView)
        LoadRowItemsSdb(rowView)
    End Sub

    Private Sub btnAnnuleSdb_Click(sender As Object, e As RoutedEventArgs) Handles btnAnnuleSdb.Click
        If ChangeYNsbb Then
            If MsgBox("ANNULER VRAIMENT ?", MsgBoxStyle.OkCancel, "ATTENTION") = MsgBoxResult.Ok Then
                ResetValuesSdb()
                DeactivAccord()
                expDecisionSdb.Visibility = Visibility.Collapsed
                chbEnAttente.Visibility = Visibility.Collapsed
                btnSaveSdb.Visibility = Visibility.Collapsed
            End If
        Else
            'ResetValuesSdb()
        End If
    End Sub

    Private Sub btnReduir_Click(sender As Object, e As RoutedEventArgs) Handles btnReduir.Click
        stpSaisie.Visibility = Visibility.Collapsed
        btnNewData.Visibility = Visibility.Visible
        btnReduir.Visibility = Visibility.Collapsed
    End Sub

    Private Sub chbNonAccord_Checked(sender As Object, e As RoutedEventArgs) Handles chbNonAccord.Checked
        chbAccord.IsChecked = False
        'ActivSaveSdb()
    End Sub

    Private Sub chbNonAccord_Unchecked(sender As Object, e As RoutedEventArgs) Handles chbNonAccord.Unchecked
        chbAccord.IsChecked = True
        'ActivSaveSdb()
    End Sub
    Sub ActivSaveSdb()
        If chbAccord.IsChecked Xor chbNonAccord.IsChecked Then
            btnSaveSdb.Visibility = Visibility.Visible
        Else
            btnSaveSdb.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub btnSaveSdb_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveSdb.Click
        If chbEnAttente.IsChecked And NewData Then
            If SaveNewAbsencesStb() Then
                MsgBox("Succès d'enregistrement ")
                ResetValuesSdb()
            End If
        ElseIf chbEnAttente.IsChecked And Not NewData Then
            If SaveModifAbsStb() Then
                MsgBox("Succès de modification ")
                ResetValuesSdb()
            End If

        End If
    End Sub

    Private Sub btnValideDecision_Click(sender As Object, e As RoutedEventArgs) Handles btnValideDecision.Click
        If chbAccord.IsChecked Then
            If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate = Today Then
                If MsgBox("La date <" & dtpDebut.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                    dtpDebut.Focus()
                    Exit Sub
                End If
            End If

            If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                If NewData Then
                    Try
                        If SaveNewAbsences() Then
                            MsgBox("Données enregistrer avec succès")
                            ResetValues()
                            stpSaisie.Visibility = Visibility.Collapsed
                            btnNewData.Visibility = Visibility.Visible
                            btnReduir.Visibility = Visibility.Collapsed
                        Else
                            stpSaisie.Visibility = Visibility.Visible
                            btnNewData.Visibility = Visibility.Collapsed
                            btnReduir.Visibility = Visibility.Visible
                        End If

                    Catch ex As Exception
                        MsgBox(ex.Message)
                    End Try

                Else
                    Try
                        If SaveModifAbs() Then
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
        ElseIf chbNonAccord.IsChecked And Not NewData Then
            If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate = Today Then
                If MsgBox("La date de début <" & dtpDebutSdb.SelectedDate.Value & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                    dtpDebut.Focus()
                    Exit Sub
                End If
                If MsgBox("La date de demande <" & dtpDateDmd.SelectedDate.Value & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                    dtpDateDmd.Focus()
                    Exit Sub
                End If
            End If
            If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                If SaveNewData("update Absences  set  StdBy=0 where IdOp=" & RefAbs, iConnxStr) Then
                    MsgBox("Succès d'enregistrement")
                    ResetValuesSdb()
                Else
                    MsgBox("Echec d'enregistrement")
                End If
            End If
        End If
    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des ABSENCES/PERMISSIONS/CONGES", "A ce jour " & Today, Me)
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
        Dim istr As String = "SELECT s.Client, ab.NomPrenom, date(ab.DateDebut) as DateDebut, date(ab.DateFin) as DateFin, (julianday(ab.DateFin) - julianday(ab.DateDebut) + 1) Durée, ab.Motif, ab.Obs  "
        istr &= " FROM affectation aff JOIN SiteAffectation s ON s.NomSite = aff.SiteAffect JOIN Absences ab ON ab.NomPrenom = aff.AgentAffect  "
        istr &= " WHERE ab.DateDebut >='" & debut.ToString("yyyy-MM-dd") & "' and ab.DateDebut <='" & Fin.ToString("yyyy-MM-dd") & "'"   ' date('now','-1 month','start of month','+24 day')  "
        istr &= " and aff.FinAffect >='" & DebutDuMois(Fin).ToString("yyyy-MM-dd") & "'"  'date('now','-1 month','start of month','+24 day')"
        istr &= " and (ab.Motif LIKE '%sans%' or ab.Motif LIKE '%malad%' )"
        'istr &= " GROUP BY s.Client, a.NomPrenoms  "
        istr &= " ORDER BY s.Client, ab.NomPrenom, ab.DateDebut"
        'ShowTempMsg(istr)
        openPointMensuel(debut, Fin, istr, "ABSENCES du " & debut.ToString("dd/MM/yyyy") & " au " & Fin.ToString("dd/MM/yyyy"), "ABENCES")

    End Sub

    Private Sub btnPointConges_Click(sender As Object, e As RoutedEventArgs) Handles btnPointConges.Click
        Dim debut As Date = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy"))
        Dim Fin As Date = CDate("24/" & Today.ToString("MM/yyyy"))
        If dtpDebutSearch.SelectedDate.HasValue Then debut = dtpDebutSearch.SelectedDate
        If dtpFinSearch.SelectedDate.HasValue Then Fin = dtpFinSearch.SelectedDate
        Dim istr As String = "SELECT s.Client, ab.NomPrenom, date(ab.DateDebut), date(ab.DateFin), (julianday(ab.DateFin) - julianday(ab.DateDebut) + 1) AS Durée, ab.Motif, ab.Obs "
        istr &= " FROM affectation aff JOIN SiteAffectation s ON s.NomSite = aff.SiteAffect JOIN Absences ab ON ab.NomPrenom = aff.AgentAffect "
        istr &= " WHERE ab.DateFin >='" & debut.ToString("yyyy-MM-dd") & "'" ' and ab.DateDebut <='" & Fin.ToString("yyyy-MM-dd") & "'"   ' date('now','-1 month','start of month','+24 day')  "
        istr &= " and aff.FinAffect >='" & DebutDuMois(Fin).ToString("yyyy-MM-dd") & "'"  'date('now','-1 month','start of month','+24 day')"
        istr &= " and (ab.Motif NOT LIKE '%sans%') and (ab.Motif NOT LIKE '%malad%') "
        'istr &= " GROUP BY s.Client, a.NomPrenoms  "
        istr &= " ORDER BY s.Client, ab.NomPrenom, ab.DateDebut"
        'ShowTempMsg(istr)
        openPointMensuel(debut, Fin, istr, "CONGES du " & debut.ToString("dd/MM/yyyy") & " au " & Fin.ToString("dd/MM/yyyy"), "CONGES")

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
