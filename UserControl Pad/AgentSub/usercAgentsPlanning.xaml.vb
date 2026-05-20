Imports System.Windows.Threading
Imports System.Data.SQLite
Imports System.Data

Public Class usercAgentsPlanning
    Public iConnxStr As String
    Public LocalConnxStr As String
    Dim is24Hloaded As Boolean = False
    Dim is12Hloaded As Boolean = False
    Dim isHebdoloaded As Boolean = False
    Dim isSubstitloaded As Boolean = False
    Dim isPlanloaded As Boolean = False
    Dim is12H24Hloaded As Boolean = False
    Dim idxJrRepos As Integer = 7
    Dim J0, J1, J2, J3, J4, J5, J6, valJr As Integer
    Dim NbOp As Integer = 0
    Dim idxop As Integer = 0
    Dim inputTimer As DispatcherTimer
    Dim MsgDelete As String = ""
    Dim retourDansMainWindow As Action
    Public Sub New(clb As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = clb
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        LocalConnxStr = $"Data Source={DbPath};Version=3;"

        initializ()

        elpStatus.Visibility = Visibility.Collapsed

    End Sub
    Sub initializ()
        elpStatus.Visibility = Visibility.Visible
        maj()
        Dim istr As String = " select format(pl.DatePlan) as [Date],pl.Agent,po.Designation,pl.Poste from planposte pl join postes po on pl.Poste=po.Code"
        istr += " where pl.DatePlan='" & Today.ToString("yyyy-MM-dd") & "'"
        'istr += " ORDER BY crt.DebutContr desc, ag.Nomprenoms "
        DgAgentPlanning.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        tblNbAgentPlanning.Text = "Nombre d'agent au poste aujourd'hui : " & MayStatVal("select count(*) from planposte  where DatePlan='" & Today.ToString("yyyy-MM-dd") & "'", LocalConnxStr) '& " | Nombre d'agents actifs sans un N° CNSSS : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.Nsecu like '' and a.Nsecu not null)")

        dtpDebutSearch.SelectedDate = (Today)
        dtpFinSearch.SelectedDate = (Today)
        Try
            LoadPlan12H24H()
            LoadPlanHebdo()
            initializHebdo()
            LoadPlanSubst()

        Catch ex As Exception
            'MsgBox(ex.Message)
        End Try
        xpdPlan24H.Visibility = Visibility.Collapsed
        xpdPlan12H.Visibility = Visibility.Collapsed
        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(5)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed
        isPlanloaded = True

        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub SearchPlanning(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        maj()
        Try
            Dim istr As String = " select format(pl.DatePlan) as [Date],pl.Agent,po.Designation,pl.Poste from planposte pl join postes po on pl.Poste=po.Code"
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                istr += " where (pl.DatePlan>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and pl.DatePlan<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "')  "
            Else
                istr += " where pl.DatePlan='" & Today.ToString("yyyy-MM-dd") & "'"
            End If
            istr += " and("
            istr += " pl.Agent like '%" & Apost(KeyWord) & "%' "
            istr += " or po.Designation like '%" & Apost(KeyWord) & "%' "
            istr += " or pl.Poste like '%" & Apost(KeyWord) & "%' "
            'istr += " "
            istr += ") "
            istr += " ORDER BY pl.DatePlan, po.Designation "
            DgAgentPlanning.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
            tblNbAgentPlanning.Text = "Nombre d'entrées : " & DgAgentPlanning.Items.Count 'MayStatVal("select count(*) from planposte  where DatePlan='" & Today.ToString("yyyy-MM-dd") & "'") '& " | Nombre d'agents actifs sans un N° CNSSS : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.Nsecu like '' and a.Nsecu not null)")

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimer.Stop()
        ' Afficher le texte du TextBox
        Dispatcher.Invoke(Sub() SearchPlanning(BtnAgentListeSearch.Text))
    End Sub

    Function LoadPlan12h() As Boolean
        LoadPlan12h = False
        maj()

        If cbxSite.SelectedIndex >= 0 And Not is12Hloaded Then
            If FilmyCbx("select a.AgentAffect,a.SiteAffect from affectation a where a.FinAffect>=date() and a.SiteAffect ='" & Apost(cbxSite.SelectedItem) & "' order by a.AgentAffect", CbxNom12H, LocalConnxStr) Then
                If FilmyCbx("select a.AgentAffect,a.SiteAffect from affectation a where a.FinAffect>=date() and (a.SiteAffect like '%g2s%' or a.SiteAffect like '%rond%' ) and  a.SiteAffect not like '%direct%'  and  a.SiteAffect not like '%contr%'  order by a.AgentAffect", CbxNomRemplace12H, LocalConnxStr) Then

                End If
                FilmyCbx("select d.JourRepos from PosteRepos d where d.Actif=1 order by d.Ordre", CbxJrRepos, LocalConnxStr)
                is12Hloaded = True
            End If
        End If
        btnValider12H.Visibility = Visibility.Collapsed
    End Function
    Function LoadPlan24h() As Boolean
        maj()
        LoadPlan24h = False
        If cbxSite.SelectedIndex >= 0 And Not is24Hloaded Then
            If FilmyCbx("select a.AgentAffect,a.SiteAffect from affectation a where a.FinAffect>=date() and a.SiteAffect ='" & Apost(cbxSite.SelectedItem) & "' order by a.AgentAffect", CbxNom24H1, LocalConnxStr) And FilmyCbx("select a.AgentAffect,a.SiteAffect from affectation a where a.FinAffect>=date() and a.SiteAffect ='" & cbxSite.SelectedItem & "' order by a.AgentAffect", CbxNom24H2, LocalConnxStr) Then
                is24Hloaded = True
            Else
                is24Hloaded = False
            End If
        End If
        btnValider24H.Visibility = Visibility.Collapsed
    End Function
    Function LoadPlanHebdo() As Boolean
        maj()
        LoadPlanHebdo = False
        If Not isHebdoloaded Then
            If FilmyCbx("select a.AgentAffect,a.SiteAffect from affectation a where a.FinAffect>=date() order by a.AgentAffect", CbxNomHebdo, LocalConnxStr) Then
                dtpDebutHebdo.SelectedDate = DebutDuMois(Today)
                dtpFinHebdo.SelectedDate = CDate("31/12/" & Today.Year)
                isHebdoloaded = True
            Else
                isHebdoloaded = False
            End If
        End If
        'btnValiderHebdo.Visibility = Visibility.Collapsed
    End Function

    Function LoadPlanSubst() As Boolean
        elpStatus.Visibility = Visibility.Visible
        maj()
        LoadPlanSubst = False
        If Not isSubstitloaded Then
            Dim iStrSortant As String = "select p.Agent from  planposte p where p.DatePlan>='" & dtpDebutSubst.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and  p.DatePlan<='" & dtpFinSubst.SelectedDate.Value.ToString("yyyy-MM-dd") & "' group by p.Agent  order by p.Agent"
            Dim iStrEntrant As String = "select a.AgentAffect as IdPosteCompt from affectation a where a.FinAffect>=date() order by a.AgentAffect"
            Dim iStrPoste As String = "select p.Poste||'# '||po.Designation as IdPosteCompt from  planposte p join postes po on po.Code=p.Poste where p.DatePlan>='" & dtpDebutSubst.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and  p.DatePlan<='" & dtpFinSubst.SelectedDate.Value.ToString("yyyy-MM-dd") & "' group by IdPosteCompt  order by po.Designation"
            If FilmyCbx(iStrSortant, CbxAgentSortant, LocalConnxStr) And FilmyCbx(iStrPoste, CbxPosteRemplace, LocalConnxStr) And FilmyCbx(iStrEntrant, CbxAgentEntrant, LocalConnxStr) Then
                CbxAgentSortant.Items.Insert(0, "*")
                CbxPosteRemplace.Items.Insert(0, "*")
                'MsgBox(CbxPosteRemplace.Items.Count)
                dtpFinSubst.SelectedDate = CDate("31/12/" & Today.Year)

                LoadPlanSubst = True
            Else
                LoadPlanSubst = False
            End If
        End If
        btnValiderSubstitution.Visibility = Visibility.Collapsed
    End Function
    Function LoadPlan12H24H() As Boolean
        elpStatus.Visibility = Visibility.Visible
        maj()
        LoadPlan12H24H = False
        If Not isSubstitloaded Then
            If FilmyCbx("select s.NomSite from siteaffectation s join postes p on p.NomSite=s.NomSite where s.Actif and p.Actif group by s.NomSite order by s.NomSite ", cbxSite, LocalConnxStr) Then
                dtpDebut12H24H.SelectedDate = DebutDuMois(Today)
                dtpFin12H24H.SelectedDate = CDate("31/12/" & Today.Year)
                is12H24Hloaded = True
            Else
                is12H24Hloaded = False
            End If
        End If
        elpStatus.Visibility = Visibility.Collapsed
    End Function

    Private Sub cbxSite_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxSite.SelectionChanged
        'lbxNumPoste.Items.Clear()
        ' CbxNom12H.Items.Clear()
        xpdPlan24H.Visibility = Visibility.Collapsed
        xpdPlan12H.Visibility = Visibility.Collapsed
        If cbxSite.SelectedIndex >= 0 Then
            If FilmyLbx("select p.Code||'# '|| p.Designation from postes p where p.Actif and p.NomSite='" & Apost(cbxSite.SelectedItem) & "' order by p.Designation ", lbxNumPoste, LocalConnxStr) Then
                lbxNumPoste.Visibility = Visibility.Visible
            Else
                lbxNumPoste.Visibility = Visibility.Collapsed
            End If
        Else
            lbxNumPoste.Visibility = Visibility.Collapsed
        End If
    End Sub
    Function ExtractPosteCode(TextDesignaation As String) As String
        ExtractPosteCode = ""
        For Each elt As Char In TextDesignaation
            If elt <> "#" Then
                ExtractPosteCode += elt
            Else
                Exit For
            End If
        Next
    End Function

    Private Sub lbxNumPoste_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lbxNumPoste.SelectionChanged
        If lbxNumPoste.SelectedIndex >= 0 Then
            Dim iCodePoste As String = ExtractPosteCode(Apost(lbxNumPoste.SelectedItem))
            'ClearPlan24H()
            If iCodePoste <> "" Then
                If idFaction(iCodePoste) = "24 heures" Then
                    is24Hloaded = False
                    xpdPlan24H.Visibility = Visibility.Visible
                    xpdPlan24H.IsExpanded = True
                    xpdPlan12H.Visibility = Visibility.Collapsed
                    LoadPlan24h()
                Else
                    ClearPlan12H()
                    is12Hloaded = False
                    xpdPlan24H.Visibility = Visibility.Collapsed
                    xpdPlan12H.Visibility = Visibility.Visible
                    xpdPlan12H.IsExpanded = True
                    LoadPlan12h()
                End If
            Else
                xpdPlan24H.Visibility = Visibility.Collapsed
                xpdPlan12H.Visibility = Visibility.Collapsed
            End If
        End If
    End Sub

    Function idFaction(CodePoste As String) As String
        idFaction = MyStatValStr("select p.Duree from postes p where p.Code='" & CodePoste & "'", LocalConnxStr)
    End Function


    Private Sub CbxNomHebdo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNomHebdo.SelectionChanged
        Try
            If CbxNomHebdo.SelectedIndex >= 0 Then
                Dim iSite As String = MyStatValStr("select p.NomSite from affectation a join postes p on a.SiteAffect=p.NomSite  where a.FinAffect>=date() and a.AgentAffect like '" & CbxNomHebdo.SelectedItem & "'  and p.Actif=1 group by a.AgentAffect order by a.AgentAffect", LocalConnxStr)
                If iSite <> "" Then
                    filHebdoJour(iSite)
                Else
                    ClearHebdoJour()
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        ActivValidHebo()
    End Sub
    Sub ClearHebdoJour()
        cbxLundiPst.Items.Clear()
        cbxMardiPst.Items.Clear()
        cbxMercrediPst.Items.Clear()
        cbxJeudiPst.Items.Clear()
        cbxVendrediPst.Items.Clear()
        cbxSamediPst.Items.Clear()
        cbxDimanchePst.Items.Clear()

    End Sub
    Sub filHebdoJour(Poste As String)
        If LCase(Poste).Contains("rondier") Then
            FilmyCbx("select p.Code||'# '||p.Designation from  postes p where p.Duree not like '%24%'  and p.Actif=1 order by p.Designation", cbxLundiPst, LocalConnxStr)
            cbxLundiPst.Items.Insert(0, "REPOS")
        ElseIf Poste <> "" Then
            FilmyCbx("select p.Code||'# '||p.Designation from  postes p where p.NomSite like '" & Poste & "' and p.Actif=1 order by p.Designation", cbxLundiPst, LocalConnxStr)
            cbxLundiPst.Items.Insert(0, "REPOS")
        Else
            ClearHebdoJour()
        End If
        If cbxLundiPst.Items.Count > 0 Then
            cbxMardiPst.ItemsSource = cbxLundiPst.Items
            cbxMercrediPst.ItemsSource = cbxLundiPst.Items
            cbxJeudiPst.ItemsSource = cbxLundiPst.Items
            cbxVendrediPst.ItemsSource = cbxLundiPst.Items
            cbxSamediPst.ItemsSource = cbxLundiPst.Items
            cbxDimanchePst.ItemsSource = cbxLundiPst.Items
        End If
    End Sub

    Private Sub btnValider12H_Click(sender As Object, e As RoutedEventArgs) Handles btnValider12H.Click
        elpStatus.Visibility = Visibility.Visible
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        If PlanSemaine(dtpDebut12H24H.SelectedDate.Value, dtpFin12H24H.SelectedDate.Value) = True Then
            CbxNom12H.SelectedIndex = -1
            CbxNomRemplace12H.SelectedIndex = -1
            CbxJrRepos.SelectedIndex = -1
            dtpDebut12H24H.SelectedDate = DebutDuMois(Today)
            dtpFin12H24H.SelectedDate = CDate("31/12/" & Today.Year) ' FinDuMois(Today)
            cbxSite.SelectedIndex = -1
            lbxNumPoste.Items.Clear()
            initializ()

        End If
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnValider24H_Click(sender As Object, e As RoutedEventArgs) Handles btnValider24H.Click
        elpStatus.Visibility = Visibility.Visible
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        If PlanF24(dtpDebut12H24H.SelectedDate.Value, dtpFin12H24H.SelectedDate.Value) Then
            CbxNom24H1.SelectedIndex = -1
            CbxNom24H2.SelectedIndex = -1
            'CbxJrRepos.SelectedIndex = -1
            dtpDebut12H24H.SelectedDate = DebutDuMois(Today)
            dtpFin12H24H.SelectedDate = CDate("31/12/" & Today.Year) 'FinDuMois(Today)
            cbxSite.SelectedIndex = -1
            lbxNumPoste.Items.Clear()
            initializ()
        End If
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnValiderHebdo_Click(sender As Object, e As RoutedEventArgs) Handles btnValiderHebdo.Click
        elpStatus.Visibility = Visibility.Visible
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        If PlanSemaineLibre(dtpDebutHebdo.SelectedDate.Value, dtpFinHebdo.SelectedDate.Value) = True Then
            initializ()
            initializHebdo()
        End If
        elpStatus.Visibility = Visibility.Collapsed

    End Sub
    Sub initializHebdo()
        Try
            CbxNomHebdo.SelectedIndex = -1
            cbxLundiPst.SelectedIndex = -1
            cbxMardiPst.SelectedIndex = -1
            cbxMercrediPst.SelectedIndex = -1
            cbxJeudiPst.SelectedIndex = -1
            cbxVendrediPst.SelectedIndex = -1
            cbxSamediPst.SelectedIndex = -1
            cbxDimanchePst.SelectedIndex = -1
            dtpDebutHebdo.SelectedDate = DebutDuMois(Today)
            dtpFinHebdo.SelectedDate = CDate("31/12/" & Today.Year) 'FinDuMois(Today)
            cbxSite.SelectedIndex = -1
            lbxNumPoste.Items.Clear()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Private Sub xpdPlanHebdo_GotFocus(sender As Object, e As RoutedEventArgs) Handles xpdPlanHebdo.GotFocus
        If Not isHebdoloaded Then
            If LoadPlanHebdo() Then

            End If
        End If
        ' btnValiderHebdo.Visibility = Visibility.Collapsed
    End Sub

    Private Sub CbxNom12H_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNom12H.SelectionChanged
        ActivValid12H()
    End Sub
    Sub ActivValid12H()
        If dtpDebut12H24H.SelectedDate.HasValue And dtpFin12H24H.SelectedDate.HasValue And cbxSite.SelectedIndex >= 0 And lbxNumPoste.SelectedIndex >= 0 Then
            If CbxJrRepos.SelectedIndex >= 0 Then
                If CbxNom12H.SelectedIndex >= 0 And CbxNomRemplace12H.SelectedIndex >= 0 Then
                    btnValider12H.Visibility = Visibility.Visible
                ElseIf CbxNom12H.SelectedIndex >= 0 And CbxJrRepos.SelectedIndex = 7 Then
                    btnValider12H.Visibility = Visibility.Visible
                Else
                    btnValider12H.Visibility = Visibility.Collapsed
                End If
            Else
                btnValider12H.Visibility = Visibility.Collapsed
            End If
        Else
            btnValider12H.Visibility = Visibility.Collapsed
        End If
    End Sub
    Sub ActivValid24H()
        If dtpDebut12H24H.SelectedDate.HasValue And dtpFin12H24H.SelectedDate.HasValue And cbxSite.SelectedIndex >= 0 And lbxNumPoste.SelectedIndex >= 0 Then
            If CbxNom24H1.SelectedIndex >= 0 And CbxNom24H2.SelectedIndex >= 0 And CbxNom24H2.SelectedItem <> CbxNom24H1.SelectedItem Then
                btnValider24H.Visibility = Visibility.Visible
            Else
                btnValider24H.Visibility = Visibility.Collapsed

            End If
        Else
            btnValider24H.Visibility = Visibility.Collapsed
        End If

    End Sub
    Private Sub CbxJrRepos_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxJrRepos.SelectionChanged
        If CbxJrRepos.SelectedItem = "NON DEFINI" Then
            spnRemplace.Visibility = Visibility.Hidden
        Else
            spnRemplace.Visibility = Visibility.Visible
        End If
        ActivValid12H()
    End Sub

    Private Sub CbxNomRemplace12H_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNomRemplace12H.SelectionChanged
        ActivValid12H()
    End Sub

    Private Sub CbxNom24H1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNom24H1.SelectionChanged
        ActivValid24H()
    End Sub

    Private Sub CbxNom24H2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNom24H2.SelectionChanged
        ActivValid24H()
    End Sub
    Sub ActivValidSubst()
        If dtpDebutSubst.SelectedDate.HasValue And dtpFinSubst.SelectedDate.HasValue AndAlso (dtpDebutSubst.SelectedDate.Value <= dtpFinSubst.SelectedDate.Value) Then
            Dim ActivCase As Integer = 0
            If CbxAgentSortant.SelectedIndex = -1 And CbxPosteRemplace.SelectedIndex = -1 Then ActivCase = 1
            If CbxAgentSortant.SelectedIndex = -1 And CbxPosteRemplace.SelectedIndex = 0 Then ActivCase = 2
            If CbxAgentSortant.SelectedIndex = -1 And CbxPosteRemplace.SelectedIndex > 0 Then ActivCase = 3
            If CbxAgentSortant.SelectedIndex = 0 And CbxPosteRemplace.SelectedIndex = -1 Then ActivCase = 4
            If CbxAgentSortant.SelectedIndex = 0 And CbxPosteRemplace.SelectedIndex = 0 Then ActivCase = 5
            If CbxAgentSortant.SelectedIndex = 0 And CbxPosteRemplace.SelectedIndex > 0 Then ActivCase = 6
            If CbxAgentSortant.SelectedIndex > 0 And CbxPosteRemplace.SelectedIndex = -1 Then ActivCase = 7
            If CbxAgentSortant.SelectedIndex > 0 And CbxPosteRemplace.SelectedIndex = 0 Then ActivCase = 8
            If CbxAgentSortant.SelectedIndex > 0 And CbxPosteRemplace.SelectedIndex > 0 Then ActivCase = 9

            Select Case ActivCase
                Case 1, 2, 3, 4, 7
                    btnValiderSubstitution.Visibility = Visibility.Hidden
                    btnValiderSuppr.Visibility = Visibility.Collapsed

                Case 5
                    btnValiderSubstitution.Visibility = Visibility.Hidden
                    MsgDelete = "TOUTES LES DONNEES DE LA PERIODE DU " & dtpDebutSubst.SelectedDate.Value & " AU " & dtpFinSubst.SelectedDate.Value & " SERONT DEFINITIVEMENT SUPPRIMES !!!"
                    MsgDelete += Environment.NewLine & " ABANDONNER ? "
                    btnValiderSuppr.Visibility = Visibility.Visible
                Case 6, 8
                    If CbxAgentEntrant.SelectedIndex >= 0 Then
                        btnValiderSubstitution.Visibility = Visibility.Visible
                    Else
                        btnValiderSubstitution.Visibility = Visibility.Hidden
                    End If
                    If CbxAgentSortant.SelectedIndex = 0 Then
                        MsgDelete = "TOUTES LES DONNEES DE LA PERIODE DU " & dtpDebutSubst.SelectedDate.Value & " AU " & dtpFinSubst.SelectedDate.Value & " SERONT DEFINITIVEMENT SUPPRIMES !!!"
                        MsgDelete += Environment.NewLine & " AU POSTE DE <" & UCase(CbxPosteRemplace.SelectedItem) & ">. "
                        MsgDelete += Environment.NewLine & " ABANDONNER ? "
                        btnValiderSuppr.Visibility = Visibility.Visible
                    Else
                        MsgDelete = "TOUTES LES DONNEES DE LA PERIODE DU " & dtpDebutSubst.SelectedDate.Value & " AU " & dtpFinSubst.SelectedDate.Value & " SERONT DEFINITIVEMENT SUPPRIMES !!!"
                        MsgDelete += Environment.NewLine & " POUR <" & UCase(CbxAgentSortant.SelectedItem) & ">. "
                        MsgDelete += Environment.NewLine & " ABANDONNER ? "
                        btnValiderSuppr.Visibility = Visibility.Visible
                    End If
                Case 9
                    If CbxAgentEntrant.SelectedIndex >= 0 Then
                        btnValiderSubstitution.Visibility = Visibility.Visible
                    Else
                        btnValiderSubstitution.Visibility = Visibility.Hidden
                    End If
                    MsgDelete = "TOUTES LES DONNEES DE LA PERIODE DU " & dtpDebutSubst.SelectedDate.Value & " AU " & dtpFinSubst.SelectedDate.Value & " SERONT DEFINITIVEMENT SUPPRIMES !!!"
                    MsgDelete += Environment.NewLine & " POUR <" & UCase(CbxAgentSortant.SelectedItem) & "> AU POSTE DE <" & UCase(CbxPosteRemplace.SelectedItem) & "> "
                    MsgDelete += Environment.NewLine & " ABANDONNER ? "
                    btnValiderSuppr.Visibility = Visibility.Visible
                Case Else
                    btnValiderSubstitution.Visibility = Visibility.Hidden
                    btnValiderSuppr.Visibility = Visibility.Collapsed
            End Select

        Else
            btnValiderSubstitution.Visibility = Visibility.Hidden
        End If

    End Sub
    Sub ActivValidHebo()
        If (dtpDebutHebdo.SelectedDate.HasValue And dtpFinHebdo.SelectedDate.HasValue) AndAlso (dtpFinHebdo.SelectedDate.Value > dtpDebutHebdo.SelectedDate.Value) Then
            btnValiderHebdo.Visibility = Visibility.Visible
            If CbxNomHebdo.SelectedIndex >= 0 And cbxLundiPst.SelectedIndex >= 0 And cbxMardiPst.SelectedIndex >= 0 And cbxMercrediPst.SelectedIndex >= 0 And cbxMercrediPst.SelectedIndex >= 0 And cbxJeudiPst.SelectedIndex >= 0 And cbxVendrediPst.SelectedIndex >= 0 And cbxSamediPst.SelectedIndex >= 0 And cbxDimanchePst.SelectedIndex >= 0 Then
                btnValiderHebdo.Visibility = Visibility.Visible
            Else
                btnValiderHebdo.Visibility = Visibility.Collapsed
            End If
        Else
            btnValiderHebdo.Visibility = Visibility.Collapsed
        End If
        'btnValiderHebdo.Visibility = Visibility.Visible
    End Sub

    Private Sub CbxAgentSortant_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxAgentSortant.SelectionChanged
        If CbxAgentSortant.SelectedIndex > 0 Then

            If CbxPosteRemplace.SelectedIndex < 0 Then
                Dim iStrPoste As String = "select p.Poste||'# '||po.Designation as IdPosteCompt from  planposte p join postes po on po.Code=p.Poste where p.Agent='" & Apost(CbxAgentSortant.SelectedItem) & "' and p.DatePlan>='" & dtpDebutSubst.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and  p.DatePlan<='" & dtpFinSubst.SelectedDate.Value.ToString("yyyy-MM-dd") & "' group by IdPosteCompt  order by IdPosteCompt"
                If FilmyCbx(iStrPoste, CbxPosteRemplace, LocalConnxStr) Then
                    CbxPosteRemplace.Items.Insert(0, "*")
                End If
            ElseIf CbxPosteRemplace.SelectedIndex = 0 Then
                'Dim iStrPoste As String = "select p.Poste||'# '||po.Designation as IdPosteCompt from  planposte p join postes po on po.Code=p.Poste where  p.DatePlan>='" & dtpDebutSubst.SelectedDate.Value & "' and  p.DatePlan<='" & dtpFinSubst.SelectedDate.Value & "' group by IdPosteCompt  order by IdPosteCompt"
                'If FilmyCbx(iStrPoste, CbxPosteRemplace) Then
                'CbxPosteRemplace.Items.Insert(0, "*")

                'End If


            End If
        ElseIf CbxAgentSortant.SelectedIndex = 0 Then

        End If
        ActivValidSubst()
    End Sub

    Private Sub CbxPosteRemplace_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxPosteRemplace.SelectionChanged
        Try
            If CbxPosteRemplace.SelectedIndex > 0 Then
                If CbxAgentSortant.SelectedIndex < 0 Then
                    Dim iStrSortant As String = "select p.Agent from  planposte p where p.Poste='" & ExtractPosteCode(Apost(CbxPosteRemplace.SelectedItem)) & "' and p.DatePlan>='" & dtpDebutSubst.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and  p.DatePlan<='" & dtpFinSubst.SelectedDate.Value.ToString("yyyy-MM-dd") & "' group by p.Agent order by p.Agent"
                    If FilmyCbx(iStrSortant, CbxAgentSortant, LocalConnxStr) Then
                        CbxAgentSortant.Items.Insert(0, "*")
                    End If
                ElseIf CbxAgentSortant.SelectedIndex = 0 Then

                End If

            ElseIf CbxPosteRemplace.SelectedIndex = 0 Then

            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        ActivValidSubst()
    End Sub



    Private Sub btnCancelSubstitution_Click(sender As Object, e As RoutedEventArgs) Handles btnCancelSubstitution.Click
        'MsgBox("OK")
        isSubstitloaded = False
        LoadPlanSubst()
        ActivValidSubst()

    End Sub

    Private Sub CbxAgentEntrant_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxAgentEntrant.SelectionChanged
        ActivValidSubst()
    End Sub

    Private Sub cbxLundiPst_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxLundiPst.SelectionChanged
        If cbxLundiPst.SelectedIndex >= 0 Then
            Dim iIndex As Integer = cbxLundiPst.SelectedIndex
            cbxMardiPst.SelectedIndex = iIndex
            cbxMercrediPst.SelectedIndex = iIndex
            cbxJeudiPst.SelectedIndex = iIndex
            cbxVendrediPst.SelectedIndex = iIndex
            cbxSamediPst.SelectedIndex = iIndex
            cbxDimanchePst.SelectedIndex = iIndex
        End If
        ActivValidHebo()
    End Sub
    Sub ClearPlan12H()
        CbxNom12H.Items.Clear()
        CbxNomRemplace12H.Items.Clear()
        CbxJrRepos.SelectedIndex = -1
    End Sub

    Private Sub xpdPlanSubst_GotFocus(sender As Object, e As RoutedEventArgs) Handles xpdPlanSubst.GotFocus
        'stpSubstContent.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnValiderSubstitution_Click(sender As Object, e As RoutedEventArgs) Handles btnValiderSubstitution.Click
        elpStatus.Visibility = Visibility.Visible
        If MessageBox.Show("Procéder au remplacement ?", "REMPLACEMENT", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If CbxPosteRemplace.SelectedIndex = 0 And CbxAgentSortant.SelectedIndex > 0 Then
                If RemplaceAgentInPlanning(dtpDebutSubst.SelectedDate.Value, dtpFinSubst.SelectedDate.Value) Then
                    MsgBox("DONNEES MISE A JOUR AVEC SUCCES")
                    initializ()
                    stpSubstContent.Visibility = Visibility.Collapsed
                End If
            End If
            If CbxPosteRemplace.SelectedIndex > 0 And CbxAgentSortant.SelectedIndex > 0 Then
                If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
                If RemplaceAgentPlan(dtpDebutSubst.SelectedDate.Value, dtpFinSubst.SelectedDate.Value, ExtractPosteCode(CbxPosteRemplace.SelectedItem)) Then
                    MsgBox("DONNEES MISE A JOUR AVEC SUCCES")
                    initializ()
                    stpSubstContent.Visibility = Visibility.Collapsed
                End If
            End If
            isSubstitloaded = False
            LoadPlanSubst()
            ActivValidSubst()
        End If
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Sub dtpDebutHebdo_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutHebdo.SelectedDateChanged, dtpFinHebdo.SelectedDateChanged
        ActivValidHebo()
    End Sub

    Private Sub cbxMardiPst_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxMardiPst.SelectionChanged, cbxMercrediPst.SelectionChanged, cbxJeudiPst.SelectionChanged, cbxVendrediPst.SelectionChanged, cbxSamediPst.SelectionChanged, cbxDimanchePst.SelectionChanged
        If isPlanloaded Then ActivValidHebo()
    End Sub

    Private Sub BtnAgentListeSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentListeSearch.KeyDown
        If e.Key = Key.Enter Then SearchPlanning(BtnAgentListeSearch.Text)
    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If isPlanloaded Then SearchPlanning(BtnAgentListeSearch.Text)
    End Sub

    Private Sub BtnAgentListeSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentListeSearch.TextChanged
        If isPlanloaded Then
            inputTimer.Stop()
            inputTimer.Start()
        End If
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "PLANNING"
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            If dtpDebutSearch.SelectedDate.Value = dtpFinSearch.SelectedDate.Value Then
                iTitre = "PLANNING DU " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") ' & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
            Else
                iTitre = "PLANNING DU " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
            End If
        End If
        ExportDGtoExcel(DgAgentPlanning, "PLANNING ", iTitre) '"Point_Entrees"

    End Sub

    Private Sub btnValiderSuppr_Click(sender As Object, e As RoutedEventArgs) Handles btnValiderSuppr.Click
        elpStatus.Visibility = Visibility.Visible
        If MessageBox.Show(MsgDelete, "ATTENTION", MessageBoxButton.YesNo, MessageBoxImage.Question) = MessageBoxResult.No Then
            If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
            If DeletePlanning(dtpDebutSubst.SelectedDate.Value, dtpFinSubst.SelectedDate.Value, CbxAgentSortant.SelectedItem, CbxPosteRemplace.SelectedItem) Then
                MsgBox("Données supprimées avec succès")
                initializ()
                CbxAgentSortant.SelectedIndex = -1
                CbxPosteRemplace.SelectedIndex = -1
                CbxAgentEntrant.SelectedIndex = -1
                isSubstitloaded = False
                If LoadPlanSubst() Then
                    stpSubstContent.Visibility = Visibility.Visible
                Else
                    stpSubstContent.Visibility = Visibility.Collapsed
                End If
                ActivValidSubst()
                dtpDebutSubst.SelectedDate = DebutDuMois(Today)
                dtpFinSubst.SelectedDate = CDate("31/12/" & Today.Year)
                stpSubstContent.Visibility = Visibility.Collapsed
            End If
        End If
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    '===========================================
    Function PlanSemaine(ByVal DateDebut As Date, ByVal DateFin As Date) As Boolean
        ' Wait_Frm.Show()
        'Loading_Pgb.Visible = True
        'PlanSemaine = False
        Dim Ixdate As Date = DateDebut
        Dim DateCollect As New Collection
        Try
            Do While Ixdate <= DateFin
                DateCollect.Add(Ixdate)
                Ixdate = Ixdate.AddDays(1)
            Loop
            NbOp = DateCollect.Count
            idxop = 0
            For Each elmt As Date In DateCollect
                'If NbOp > 0 Then Loading_Pgb.Value = CInt(100 * idxop / NbOp)
                Select Case CbxJrRepos.SelectedIndex
                    Case 0 To 5
                        idxJrRepos = CbxJrRepos.SelectedIndex + 1
                    Case 6
                        idxJrRepos = 0
                    Case Else
                        idxJrRepos = 7

                End Select
                If elmt.DayOfWeek = idxJrRepos Then
                    'MsgBox(elmt)
                    SaveNewPlanPosteData(elmt, ExtractPosteCode(lbxNumPoste.SelectedItem), CbxNomRemplace12H.SelectedItem)
                Else
                    SaveNewPlanPosteData(elmt, ExtractPosteCode(lbxNumPoste.SelectedItem), CbxNom12H.SelectedItem)
                End If
                If idxJrRepos <> 7 Then

                End If
                idxop += 1
            Next
            PlanSemaine = True
        Catch ex As Exception
            MsgBox(ex.Message)
            PlanSemaine = False
        End Try
        If PlanSemaine Then Joper("PlanSemaine on Poste " & lbxNumPoste.SelectedItem, iConnxStr)
        'Loading_Pgb.Visible = False
        'Wait_Frm.Close()
    End Function

    Private Sub btnOkDate_Click(sender As Object, e As RoutedEventArgs) Handles btnOkDate.Click
        'stpSubstContent.Visibility = Visibility.Visible
        If LoadPlanSubst() Then
            stpSubstContent.Visibility = Visibility.Visible
        Else
            stpSubstContent.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub cmiThisW_Click(sender As Object, e As RoutedEventArgs) Handles cmiThisW.Click
        Dim rowView As DataRowView = TryCast(DgAgentPlanning.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            'cmiPoste.Header = rowView.Row.ItemArray(1).ToString
            Dim iWrd As String = ResumePlanning(rowView.Row.ItemArray(1).ToString, DebutDeSemaine(Today), finDeSemaine(Today))
            If iWrd <> "" Then ShowTempMsg(iWrd,, "CETTE SEMAINE => " & UCase(rowView.Row.ItemArray(1).ToString))
        End If
    End Sub

    Private Sub cmiNextW_Click(sender As Object, e As RoutedEventArgs) Handles cmiNextW.Click
        Dim rowView As DataRowView = TryCast(DgAgentPlanning.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumePlanning(rowView.Row.ItemArray(1).ToString, DebutDeSemaine(Today.AddDays(7)), finDeSemaine(Today).AddDays(7))
            If iWrd <> "" Then ShowTempMsg(iWrd,, "SEMAINE PROCHAINE => " & UCase(rowView.Row.ItemArray(1).ToString))
        End If
    End Sub

    Private Sub cmiThisWp_Click(sender As Object, e As RoutedEventArgs) Handles cmiThisWp.Click
        Dim rowView As DataRowView = TryCast(DgAgentPlanning.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumePlanningPoste(rowView.Row.ItemArray(2).ToString, DebutDeSemaine(Today), finDeSemaine(Today))
            If iWrd <> "" Then ShowTempMsg(iWrd,, "CETTE SEMAINE => " & UCase(rowView.Row.ItemArray(2).ToString))
        End If
    End Sub

    Private Sub cmiNextWp_Click(sender As Object, e As RoutedEventArgs) Handles cmiNextWp.Click
        Dim rowView As DataRowView = TryCast(DgAgentPlanning.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumePlanningPoste(rowView.Row.ItemArray(2).ToString, DebutDeSemaine(Today.AddDays(7)), finDeSemaine(Today.AddDays(7)))
            If iWrd <> "" Then ShowTempMsg(iWrd,, "SEMAINE PROCHAINE => " & UCase(rowView.Row.ItemArray(2).ToString))
        End If
    End Sub

    Private Sub xpdPlanSubst_Expanded(sender As Object, e As RoutedEventArgs) Handles xpdPlanSubst.Expanded
        dtpDebutSubst.SelectedDate = DebutDuMois(Today)
        dtpFinSubst.SelectedDate = CDate("31/12/" & Today.Year)

    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des PLANNINGS", "A ce jour " & Today, Me)
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

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        If MsgBox("Réinitialiser toutes les données de planning?" & Environment.NewLine & "CONTINUER ?", MsgBoxStyle.YesNo, "ACTUALISATION DES DONNEES") = MsgBoxResult.Yes Then initializ()
    End Sub

    Function PlanF24(ByVal DateDebut As Date, ByVal DateFin As Date) As Boolean
        ' Wait_Frm.Show()
        'Loading_Pgb.Visible = True
        'PlanF24 = False
        Dim Ixdate As Date = DateDebut
        Dim DateCollect As New Collection
        Try
            Do While Ixdate <= DateFin
                DateCollect.Add(Ixdate)
                Ixdate = Ixdate.AddDays(1)
            Loop
            NbOp = DateCollect.Count
            idxop = 0
            For cont As Integer = 1 To DateCollect.Count
                'Dim MyVal As Integer = cont Mod 2
                'If NbOp > 0 Then Loading_Pgb.Value = CInt(100 * IdxOp / NbOp)
                If cont Mod 2 = 0 Then
                    SaveNewPlanPosteData(DateCollect.Item(cont), ExtractPosteCode(lbxNumPoste.SelectedItem), CbxNom24H2.Text)
                Else
                    SaveNewPlanPosteData(DateCollect.Item(cont), ExtractPosteCode(lbxNumPoste.SelectedItem), CbxNom24H1.Text)
                End If
                idxop += 1
            Next
            PlanF24 = True
        Catch ex As Exception
            MsgBox(ex.Message)
            PlanF24 = False
        End Try
        If PlanF24 Then Joper("PlanF24 on " & lbxNumPoste.SelectedItem & " for " & CbxNom24H1.Text & " and " & CbxNom24H2.Text, iConnxStr)
        'Loading_Pgb.Visible = False
        'Wait_Frm.Close()
    End Function
    Function PlanSemaineLibre(ByVal DateDebut As Date, ByVal DateFin As Date) As Boolean
        'MsgBox("ok")
        'Wait_Frm.Show()
        'PlanSemaineLibre = False
        'Loading_Pgb.Visible = True
        Dim Ixdate As Date = DateDebut
        Dim DateCollect As New Collection
        Try
            Do While Ixdate <= DateFin
                DateCollect.Add(Ixdate)
                Ixdate = Ixdate.AddDays(1)
            Loop
            NbOp = DateCollect.Count
            idxop = 0
            For Each elmt As Date In DateCollect
                'If NbOp > 0 Then Loading_Pgb.Value = CInt(100 * IdxOp / NbOp)
                Select Case elmt.DayOfWeek
                    Case 0
                        If cbxDimanchePst.SelectedIndex >= 0 And cbxDimanchePst.SelectedItem <> "REPOS" Then SaveNewPlanPosteData(elmt, ExtractPosteCode(cbxDimanchePst.SelectedItem), ExtractPosteCode(CbxNomHebdo.SelectedItem))
                    Case 1
                        If cbxLundiPst.SelectedIndex >= 0 And cbxLundiPst.SelectedItem <> "REPOS" Then SaveNewPlanPosteData(elmt, ExtractPosteCode(cbxLundiPst.SelectedItem), ExtractPosteCode(CbxNomHebdo.SelectedItem))
                    Case 2
                        If cbxMardiPst.SelectedIndex >= 0 And cbxMardiPst.SelectedItem <> "REPOS" Then SaveNewPlanPosteData(elmt, ExtractPosteCode(cbxMardiPst.SelectedItem), ExtractPosteCode(CbxNomHebdo.SelectedItem))
                    Case 3
                        If cbxMercrediPst.SelectedIndex >= 0 And cbxMercrediPst.SelectedItem <> "REPOS" Then SaveNewPlanPosteData(elmt, ExtractPosteCode(cbxMercrediPst.SelectedItem), ExtractPosteCode(CbxNomHebdo.SelectedItem))
                    Case 4
                        If cbxJeudiPst.SelectedIndex >= 0 And cbxJeudiPst.SelectedItem <> "REPOS" Then SaveNewPlanPosteData(elmt, ExtractPosteCode(cbxJeudiPst.SelectedItem), ExtractPosteCode(CbxNomHebdo.SelectedItem))
                    Case 5
                        If cbxVendrediPst.SelectedIndex >= 0 And cbxVendrediPst.SelectedItem <> "REPOS" Then SaveNewPlanPosteData(elmt, ExtractPosteCode(cbxVendrediPst.SelectedItem), ExtractPosteCode(CbxNomHebdo.SelectedItem))
                    Case 6
                        If cbxSamediPst.SelectedIndex >= 0 And cbxSamediPst.SelectedItem <> "REPOS" Then SaveNewPlanPosteData(elmt, ExtractPosteCode(cbxSamediPst.SelectedItem), ExtractPosteCode(CbxNomHebdo.SelectedItem))
                    Case Else
                        'idxJrRepos = 7
                End Select
                idxop += 1
            Next
            PlanSemaineLibre = True
        Catch ex As Exception
            MsgBox(ex.Message)
            PlanSemaineLibre = False
        End Try
        If PlanSemaineLibre Then Joper("PlanSemaineLibre for " & CbxNomHebdo.SelectedItem, iConnxStr)
        'Loading_Pgb.Visible = False
        'Wait_Frm.Close()
    End Function
    Public Function SaveNewPlanPosteData(ByVal DatePlan As Date, ByVal idPoste As String, ByVal NomAgent As String) As Boolean
        'Wait_Frm.Show()
        Dim Istr As String = "INSERT INTO PlanPoste (Agent, DatePlan, Poste) "
        Istr += " VALUES  ('" & Apost(NomAgent) & "',"
        Istr += "'" & DatePlan.ToString("yyyy-MM-dd") & "', "
        Istr += "'" & Apost(idPoste) & "') "
        'If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL => " & DbPath)
        SaveNewPlanPosteData = SaveNewData(Istr, iConnxStr)
        'Wait_Frm.Close()
    End Function
    Public Function SaveMdifPlanPosteData(ByVal idPoste As String, ByVal NomNouvelAgent As String) As Boolean
        Try
            Dim iCmdStr As String = "UPDATE PlanPoste  SET Agent='" & Apost(NomNouvelAgent) & "' "
            iCmdStr += " where IdPlan =" & idPoste
            'If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL => " & DbPath)
            SaveMdifPlanPosteData = SaveNewData(iCmdStr, iConnxStr)
        Catch ex As Exception
            MsgBox(ex.Message)
            SaveMdifPlanPosteData = False
        End Try
        'Wait_Frm.Close()
    End Function


    Function RemplaceAgentInPlanning(ByVal DateDebut As Date, ByVal DateFin As Date, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        'Wait_Frm.Show()
        RemplaceAgentInPlanning = False
        Dim Ixdate As Date = DateDebut
        Dim DateCollect As New Collection
        Dim IdPosteCollection As New Collection
        Try
            'Loading_Pgb.Visible = True

            Do While Ixdate <= DateFin
                DateCollect.Add(Ixdate)
                Ixdate = Ixdate.AddDays(1)
            Loop

            If DateCollect.Count = 0 Then
                RemplaceAgentInPlanning = False
                Exit Function
            End If
            IdPosteCollection.Clear()
            idxop = 1
            'Loading_Pgb.Visible = True
            For Each elmt As Date In DateCollect
                Dim myCbx As New ComboBox
                Dim iCmdStr As String = "SELECT   p.IdPlan FROM  PlanPoste p  WHERE p.Agent like " '" & Apost(CbxAgentSortant.SelectedItem) & "' "
                If CbxAgentSortant.SelectedItem = "*" Then
                    iCmdStr += "'%'"
                Else
                    iCmdStr += "'" & Apost(CbxAgentSortant.SelectedItem) & "' "
                End If
                iCmdStr += " And DatePlan ='" & elmt.ToString("yyyy-MM-dd") & "'"
                Try
                    Using iconx As New SQLiteConnection(iConxStr)
                        iconx.Open()
                        Using iCmnd As New SQLiteCommand(iCmdStr, iconx)
                            Dim iReader As SQLiteDataReader = iCmnd.ExecuteReader
                            While iReader.Read
                                IdPosteCollection.Add(iReader(0))
                            End While
                            RemplaceAgentInPlanning = True
                        End Using
                    End Using
                Catch ex As Exception
                    MsgBox(ex.Message)
                    RemplaceAgentInPlanning = False
                End Try
                idxop += 1
                'myCbx.Dispose()
            Next
            If RemplaceAgentInPlanning And IdPosteCollection.Count > 0 Then
                idxop = 1
                ' Loading_Pgb.Visible = True
                For Each IdPost As Integer In IdPosteCollection
                    Try
                        If Not SaveMdifPlanPosteData(IdPost, Apost(CbxAgentEntrant.SelectedItem)) Then RemplaceAgentInPlanning = False
                        'Loading_Pgb.Value = CInt(100 * idxop / IdPosteCollection.Count)
                        idxop += 1
                    Catch ex As Exception
                        RemplaceAgentInPlanning = False
                        Exit For
                    End Try
                Next
                'Loading_Pgb.Visible = False
            Else
                RemplaceAgentInPlanning = False
            End If
            RemplaceAgentInPlanning = True
        Catch ex As Exception
            MsgBox(ex.Message)
            RemplaceAgentInPlanning = False
        End Try
        'Loading_Pgb.Value = 0
        'Loading_Pgb.Visible = False

        If RemplaceAgentInPlanning Then
            If RemplaceAgentInPlanning Then Joper("RemplaceInPlanning " & CbxAgentSortant.SelectedItem & " by " & CbxAgentEntrant.SelectedItem, iConnxStr)
        End If
        'Wait_Frm.Close()
    End Function
    Function RemplaceAgentPlan(ByVal DateDebut As Date, ByVal DateFin As Date, ByVal IdPoste As String, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        'Wait_Frm.Show()
        RemplaceAgentPlan = False
        Dim Ixdate As Date = DateDebut
        Dim DateCollect As New Collection
        Dim IdPosteCollection As New Collection
        Try
            Do While Ixdate <= DateFin
                DateCollect.Add(Ixdate)
                Ixdate = Ixdate.AddDays(1)
            Loop
            'MsgBox(DateCollect.Count) '<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            If DateCollect.Count = 0 Then
                RemplaceAgentPlan = False
                Exit Function
            End If
            IdPosteCollection.Clear()
            For Each elmt As Date In DateCollect
                Dim myCbx As New ComboBox
                Dim iCmdStr As String = "SELECT   IdPlan "
                iCmdStr += " FROM  PlanPoste "
                If CbxAgentSortant.SelectedItem = "*" Then
                    iCmdStr += " WHERE Agent like '%'" ' & Apost(CbxAgentSortant.SelectedItem) & "' "
                Else
                    iCmdStr += " WHERE Agent like '" & Apost(CbxAgentSortant.SelectedItem) & "' "
                End If
                iCmdStr += " and Poste like '" & Apost(IdPoste) & "' "
                iCmdStr += "and DatePlan ='" & elmt.ToString("yyyy-MM-dd") & "'"
                '********************************

                Try
                    Using iconx As New SQLiteConnection(iConxStr)
                        iconx.Open()
                        Using iCmnd As New SQLiteCommand(iCmdStr, iconx)
                            Dim iReader As SQLiteDataReader = iCmnd.ExecuteReader
                            While iReader.Read
                                IdPosteCollection.Add(iReader(0))
                            End While
                            RemplaceAgentPlan = True
                        End Using
                    End Using
                Catch ex As Exception
                    MsgBox(ex.Message)
                    RemplaceAgentPlan = False
                End Try
            Next
            If RemplaceAgentPlan And IdPosteCollection.Count > 0 Then
                For Each IdPost As Integer In IdPosteCollection
                    Try
                        'MsgBox(IdPost)
                        If Not SaveMdifPlanPosteData(IdPost, Apost(CbxAgentEntrant.SelectedItem)) Then RemplaceAgentPlan = False
                    Catch ex As Exception
                        RemplaceAgentPlan = False
                        Exit For
                    End Try
                Next
            Else
                RemplaceAgentPlan = False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            RemplaceAgentPlan = False
        End Try
        If RemplaceAgentPlan Then Joper("RemplaceAgentPlan " & CbxAgentSortant.SelectedItem & " by " & CbxAgentEntrant.SelectedItem, iConnxStr)
        'Wait_Frm.Close()
    End Function
    Function DeletePlanning(Debut As Date, Fin As Date, Agent As String, Poste As String) As Boolean
        DeletePlanning = False
        If Agent = "*" Then Agent = "%"
        If Poste = "*" Then Poste = "%"
        If Debut <= Fin Then
            Try
                Dim iStr As String = "Delete  from planposte "
                iStr += " where DatePlan>='" & Debut.ToString("yyyy-MM-dd") & "' and DatePlan<='" & Fin.ToString("yyyy-MM-dd") & "' "
                iStr += " and Agent like '" & Apost(Agent) & "' "
                iStr += " and Poste like '" & Apost(ExtractPosteCode(Poste)) & "' "
                'MsgBox(iStr)
                'If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL => " & DbPath)
                DeletePlanning = SaveNewData(iStr, iConnxStr)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
        If DeletePlanning Then Joper("SaveModifHS for " & Agent & " at " & Poste, iConnxStr)
    End Function

End Class
