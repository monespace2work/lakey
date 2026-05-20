Imports System.Windows.Threading
Imports System.Data.SQLite
Public Class usercAgentsSortie
    Public iConnxStr As String
    Public LocalConnxStr As String
    Dim RefSortie As Integer = 0
    Dim ChangeYN As Boolean = False
    Dim inputTimer As DispatcherTimer
    Dim iFormLoaded As Boolean = False
    Dim retourDansMainWindow As Action
    Public Shared AfficheStat As Action 'AfficheMemos
    Public Shared AfficheMemos As Action 'AfficheMemos

    Public Sub New(clb As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = clb
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;Pooling=False;"
        LocalConnxStr = $"Data Source={DbPath};Version=3;"
        InialiseLoad()
        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(5)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed
        elpStatus.Visibility = Visibility.Collapsed

    End Sub
    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimer.Stop()
        ' Afficher le texte du TextBox
        Dispatcher.Invoke(Sub() SearchAffectation(BtnAgentListeSearch.Text))
    End Sub

    Sub InialiseLoad()
        elpStatus.Visibility = Visibility.Visible
        maj()
        Dim istr As String = " select ag.NomsPrenoms,format(ag.DateSortie) as DateSortie, ag.SiteSortie, ag.MotifSortie,ag.Obs, format(ag.DateEntreeRappel) as DateEntree, ag.IdSortie  "
        istr += " from sortie ag "
        istr += " where DateSortie>='" & CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")).ToString("yyyy-MM-dd") & "'"
        istr += " order by ag.DateSortie desc, ag.NomsPrenoms "
        DgAgentSortie.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "En cours de validité : " & MayStatVal("select count(*) from (" & istr & ")")
        itext += Environment.NewLine
        'itext += MyAffectationVilleList("select s.Localisation as ville, count(*) as nbr from (" & istr & ") a join siteaffectation s on a.Site=s.NomSite where a.Fin>='" & Today.ToString("yyyy-MM-dd") & "' group by s.Localisation Order by nbr desc,ville")
        tblNbAgentSortie.Text = itext
        'FilmyCbx("select s.NomSite from  siteaffectation s where s.Actif=1 order by s.NomSite ", CbxNouveauSite)
        FilmyCbx("select s.Motif from SortieMotifs s where s.Actif=1 order by s.Favorit desc,s.NumOrdre", CbxMotif, LocalConnxStr)
        FilmyCbx("select a.NomPrenoms from contratstbl c join agents a on c.NomPrenoms=a.NumMatr where a.Actif=1 group by a.NomPrenoms  order by a.NomPrenoms", CbxNom, LocalConnxStr)

        RefSortie = MayStatVal("select max(IdSortie) from sortie ", LocalConnxStr) + 1
        dtpDateSortie.SelectedDate = Today
        dtpDateSortie.DisplayDateStart = DateCreationSociete
        dtpDateSortie.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpDebutSearch.SelectedDate = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy"))
        dtpFinSearch.SelectedDate = CDate("24/" & Today.ToString("MM/yyyy"))
        iFormLoaded = True
        ChangeYN = False
        AfficheProfilUser()
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub ResetValues()
        elpStatus.Visibility = Visibility.Visible
        maj()
        AfficheStat?.Invoke()
        AfficheMemos?.Invoke()
        Dim istr As String = " select ag.NomsPrenoms,format(ag.DateSortie) as DateSortie, ag.SiteSortie, ag.MotifSortie,ag.Obs, format(ag.DateEntreeRappel) as DateEntree, ag.IdSortie  "
        istr += " from sortie ag "
        istr += " where DateSortie>='" & CDate("01/01/" & Today.ToString("yyyy")).ToString("yyyy-MM-dd") & "'"
        istr += " order by ag.DateSortie desc, ag.NomsPrenoms "
        DgAgentSortie.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "En cours de validité : " & MayStatVal("select count(*) from (" & istr & ")", LocalConnxStr)
        itext += Environment.NewLine
        'itext += MyAffectationVilleList("select s.Localisation as ville, count(*) as nbr from (" & istr & ") a join siteaffectation s on a.Site=s.NomSite where a.Fin>='" & Today.ToString("yyyy-MM-dd") & "' group by s.Localisation Order by nbr desc,ville")
        tblNbAgentSortie.Text = itext
        FilmyCbx("select a.NomPrenoms from contratstbl c join agents a on c.NomPrenoms=a.NumMatr where a.Actif=1 group by a.NomPrenoms  order by a.NomPrenoms", CbxNom, LocalConnxStr)

        RefSortie = MayStatVal("select max(IdSortie) from sortie ", LocalConnxStr) + 1
        dtpDateSortie.SelectedDate = Today
        dtpDateSortie.DisplayDateStart = DateCreationSociete
        dtpDateSortie.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        CbxMotif.SelectedIndex = -1
        CbxNom.SelectedIndex = -1
        CbxNumMatr.SelectedIndex = -1

        TbxDernierSite.Text = ""
        tbxDebutContrat.Text = ""
        tbxFinContrat.Text = ""
        TbxObs.Text = ""
        'dtpFinAffect.DisplayDateEnd = FinDuMois(Today.AddMonths(12))
        ChangeYN = False
        iFormLoaded = True
        AfficheProfilUser()

        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub SearchAffectation(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        maj()
        Dim myw As New WaitFrm
        myw.Show()
        Dim sufix As String = ""
        If KeyWord = "" Then
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                sufix += " and  DateSortie>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateSortie<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "'  "
            Else
                sufix += " and DateSortie>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "' and DateSortie<='" & Today.ToString("yyyy-MM-dd") & "'  "
                'istr = " (DateSortie>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "') and "

            End If
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = " and DateSortie>='" & DateCreationSociete.ToString("yyyy-MM-dd") & "' and DateSortie<='" & Today.ToString("yyyy-MM-dd") & "' "
        Else
            sufix = ""
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                sufix += " and  DateSortie>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateSortie<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "'  "
            Else
                sufix += " and DateSortie>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "' and DateSortie<='" & Today.ToString("yyyy-MM-dd") & "'  "
                'istr = " (DateSortie>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "') and "

            End If
        End If
        Dim istr As String = " select ag.NomsPrenoms,format(ag.DateSortie) as DateSortie, ag.SiteSortie, ag.MotifSortie,ag.Obs, format(ag.DateEntreeRappel) as DateEntree, ag.IdSortie  "
        istr += " from sortie ag where "
        istr += " (ag.NomsPrenoms  like '%" & KeyWord & "%' " 'af.SiteAffectPrecedent
        istr += " or ag.SiteSortie  like '%" & KeyWord & "%' "
        istr += " or ag.MotifSortie  like '%" & KeyWord & "%' "
        istr += " or ag.Obs  like '%" & KeyWord & "%' )" & sufix
        istr += " order by ag.DateSortie desc, ag.NomsPrenoms  "
        DgAgentSortie.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "En cours de validité : " & MayStatVal("select count(*) from (" & istr & ")", LocalConnxStr)
        itext += Environment.NewLine
        'itext += MyAffectationVilleList("select s.Localisation as ville, count(*) as nbr from (" & istr & ") a join siteaffectation s on a.Site=s.NomSite  group by s.Localisation Order by nbr desc,ville")
        tblNbAgentSortie.Text = itext '"En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")
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
        End If
        SaisiCbx(sender, TblIndicationNom)
        If CbxNom.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            If FilmyCbx("select a.NumMatr,a.NomPrenoms from agents a  where a.NomPrenoms like '" & Apost(CbxNom.SelectedItem) & "'  order by a.NumMatr", CbxNumMatr, LocalConnxStr) Then
                CbxNumMatr.SelectedIndex = 0
                If SiteActuElmt() Then
                    'TbxDernierSite.Text = "" 'SiteDernierAffectation

                End If
                'TbxAncienSite.Text = MyStatValStr("select a.SiteAffect from  affectation a where a.AgentAffect like '" & Apost(CbxNom.SelectedItem) & "' order by a.DateAffect desc,a.IdAffec desc limit 1")

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
        If RefSortie >= 1 And TbxDernierSite.Text <> "" And CbxMotif.Items.Contains(CbxMotif.SelectedItem) And CbxNumMatr.Items.Contains(CbxNumMatr.SelectedItem) And (dtpDateSortie.SelectedDate.HasValue) Then
            btnSave.Visibility = Visibility.Visible
            btnSaveContinue.Visibility = Visibility.Visible
        Else
            btnSave.Visibility = Visibility.Collapsed
            btnSaveContinue.Visibility = Visibility.Collapsed
        End If
        ChangeYN = True
        stpButtonsPad.Visibility = Visibility.Visible

    End Sub
    Function SiteActuElmt(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        SiteActuElmt = False
        If IO.Directory.Exists(IO.Path.Combine(UserPath, "ManA.sqlite")) Then iConxStr = $"Data Source={IO.Path.Combine(UserPath, "ManA.sqlite")}\ManA.sqlite;Version=3;"
        If CbxNom.SelectedIndex >= 0 Then
            Dim istr As String = "select a.SiteAffect from  affectation a where a.AgentAffect like '" & Apost(CbxNom.SelectedItem) & "' and a.FinAffect>='" & Today.AddDays(-21).ToString("yyyy-MM-dd") & "' order by a.DateAffect desc,a.IdAffec desc limit 1"
            TbxDernierSite.Text = MyStatValStr(istr, LocalConnxStr)
            Try
                If CbxNumMatr.SelectedIndex >= 0 Then
                    istr = "select format(c.DebutContr) as debut,format(c.FinContr) as fin from contratstbl c where c.NomPrenoms like '" & Apost(CbxNumMatr.SelectedItem) & "' "
                    Try
                        Using iconnx As New SQLiteConnection(iConxStr)
                            iconnx.Open()
                            Using icmd As New SQLiteCommand(istr, iconnx)
                                Dim ireader As SQLiteDataReader = icmd.ExecuteReader
                                While ireader.Read
                                    If Not IsDBNull(ireader(0)) AndAlso IsDate(ireader(0)) Then
                                        tbxDebutContrat.Text = CDate(ireader(0)).ToString("dd/MM/yyyy")
                                        dtpDateSortie.DisplayDateStart = CDate(ireader(0))
                                    End If
                                    If Not IsDBNull(ireader(1)) AndAlso IsDate(ireader(1)) Then
                                        Dim idate As Date = CDate(ireader(1))
                                        tbxFinContrat.Text = idate.ToString("dd/MM/yyyy")
                                        dtpDateSortie.DisplayDateEnd = idate
                                        If idate < Today Then
                                            dtpDateSortie.SelectedDate = idate
                                        Else
                                            dtpDateSortie.SelectedDate = Today
                                        End If
                                    End If
                                    SiteActuElmt = True
                                End While
                            End Using
                        End Using

                    Catch ex As Exception
                        MsgBox(ex.Message)
                        SiteActuElmt = False
                    End Try

                End If
            Catch ex As Exception
                MsgBox(ex.Message)
                SiteActuElmt = False

            End Try
        End If
    End Function

    Private Sub CbxMotif_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxMotif.SelectionChanged
        SaisiCbx(sender, TblIndicationMotif)
        ActivSaveBtn()
    End Sub

    Private Sub TbxObs_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxObs.TextChanged
        SaisiTbx(sender, TblIndicationObs)
        ActivSaveBtn()

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
            'btpPlus.Content = " > "

        End If

    End Sub

    Private Sub btnNewData_Click(sender As Object, e As RoutedEventArgs) Handles btnNewData.Click
        stpSaisie.Visibility = Visibility.Visible
        btnNewData.Visibility = Visibility.Collapsed

    End Sub

    Private Sub BtnAgentListeSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentListeSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()

    End Sub
    Function SaveNewSortie() As Boolean
        'SaveNewAffectation = False
        Dim istr As String = " insert into sortie "
        istr += "(IdSortie, NomsPrenoms, DateSortie, DateEntreeRappel, SiteSortie,MotifSortie, Obs) values "
        istr += " ('" & RefSortie & "', "
        istr += " '" & Apost(CbxNom.SelectedItem) & "', "
        istr += " '" & dtpDateSortie.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " '" & CDate(tbxDebutContrat.Text).ToString("yyyy-MM-dd") & "', "
        istr += " '" & Apost(TbxDernierSite.Text) & "', "
        istr += " '" & Apost(CbxMotif.SelectedItem) & "', "
        'istr += " '" & Apost(TbxNumCrt.Text) & "', "
        istr += " '" & Apost(TbxObs.Text) & "') "
        'MsgBox(istr)

        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveNewSortie = SaveNewData(istr, iConnxStr)
        If SaveNewSortie Then Joper("SaveNewSortie for " & CbxNom.SelectedItem, iConnxStr)
    End Function

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Try
                If MsgBox(CbxNom.SelectedItem & "// sera effacé.e du PLANNING" & Chr(13) & "ABANDONNER ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                    If MsgBox("Continuer sans désactiver dans le planning", MsgBoxStyle.YesNo, "OPTION SANS DESACTIVATION DANS LE PLANNING") = MsgBoxResult.Yes Then

                    Else
                        If FinPlan() Then
                            MsgBox(CbxNom.SelectedItem & "// désactivé.e du planning")
                        Else
                            MsgBox("Erreur au niveau du planning!")
                        End If
                    End If
                    If MsgBox(CbxNom.SelectedItem & "// sera désactivé.e des AFFECTATIONS" & Chr(13) & "ABANDONNER ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                        If FinAffectation() Then
                            MsgBox(CbxNom.SelectedItem & "// désactivé.e des AFFECTATIONS")
                            If MsgBox(CbxNom.SelectedItem & "// sera désactivé.e des CONTRATS" & Chr(13) & "ABANDONNER ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                                If FinContrat() Then
                                    MsgBox(CbxNom.SelectedItem & "// désactivé.e des CONTRATS")
                                    If MsgBox(CbxNom.SelectedItem & "// sera désactivé.e de la liste des AGENTS" & Chr(13) & "ABANDONNER ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                                        If FinAgent() Then
                                            If SaveNewSortie() Then
                                                MsgBox("Données enregistrer avec succès")
                                                ResetValues()
                                                stpSaisie.Visibility = Visibility.Collapsed
                                                btnNewData.Visibility = Visibility.Visible
                                            Else
                                                MsgBox("Echec d'enregistrement de données")
                                                stpSaisie.Visibility = Visibility.Visible
                                                btnNewData.Visibility = Visibility.Collapsed
                                            End If

                                        Else
                                            MsgBox("Erreur au niveau la désactivation de l'AGENT !")
                                        End If

                                    End If

                                Else
                                    MsgBox("Erreur au niveau des CONTRATS !")
                                End If
                            End If
                        Else
                            MsgBox("Erreur au niveau des AFFECTATIONS !")

                        End If

                    End If
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        End If

    End Sub

    Private Sub btnSaveContinue_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveContinue.Click
        If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Try
                If MsgBox(CbxNom.SelectedItem & "// sera effacé.e du PLANNING" & Chr(13) & "ABANDONNER ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                    If FinPlan() Then
                        MsgBox(CbxNom.SelectedItem & "// désactivé.e du planning")
                        If MsgBox(CbxNom.SelectedItem & "// sera désactivé.e des AFFECTATIONS" & Chr(13) & "ABANDONNER ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                            If FinAffectation() Then
                                MsgBox(CbxNom.SelectedItem & "// désactivé.e des AFFECTATIONS")
                                If MsgBox(CbxNom.SelectedItem & "// sera désactivé.e des CONTRATS" & Chr(13) & "ABANDONNER ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                                    If FinContrat() Then
                                        MsgBox(CbxNom.SelectedItem & "// désactivé.e des CONTRATS")
                                        If MsgBox(CbxNom.SelectedItem & "// sera désactivé.e de la liste des AGENTS" & Chr(13) & "ABANDONNER ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                                            If FinAgent() Then
                                                If SaveNewSortie() Then
                                                    MsgBox("Données enregistrer avec succès")
                                                    ResetValues()
                                                    'stpSaisie.Visibility = Visibility.Collapsed
                                                    'btnNewData.Visibility = Visibility.Visible
                                                Else
                                                    MsgBox("Echec d'enregistrement de données")
                                                    'stpSaisie.Visibility = Visibility.Visible
                                                    'btnNewData.Visibility = Visibility.Collapsed
                                                End If

                                            Else
                                                MsgBox("Erreur au niveau la désactivation de l'AGENT !")
                                            End If

                                        End If

                                    Else
                                        MsgBox("Erreur au niveau des CONTRATS !")
                                    End If
                                End If
                            Else
                                MsgBox("Erreur au niveau des AFFECTATIONS !")

                            End If


                        End If

                    Else
                            MsgBox("Erreur au niveau du planning!")
                    End If
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        End If

    End Sub
    Function FinPlan() As Boolean
        FinPlan = False
        If dtpDateSortie.SelectedDate.HasValue And CbxNom.Items.Contains(CbxNom.SelectedItem) Then
            Dim istr As String = "delete from PlanPoste  where Agent like '" & Apost(CbxNom.SelectedItem) & "' and DatePlan >'" & dtpDateSortie.SelectedDate.Value.ToString("yyyy-MM-dd") & "'"
            FinPlan = DeleteData(istr, iConnxStr)
        End If
    End Function
    Function FinAffectation() As Boolean
        FinAffectation = False
        Dim IdAff As Integer = MayStatVal("select a.IdAffec from affectation a where a.AgentAffect like '" & Apost(CbxNom.SelectedItem) & "' order by a.IdAffec desc limit 1", LocalConnxStr)
        FinAffectation = SaveNewData("update affectation set FinAffect='" & dtpDateSortie.SelectedDate.Value.ToString("yyyy-MM-dd") & "' where IdAffec=" & IdAff & "", iConnxStr)
    End Function
    Function FinContrat() As Boolean
        FinContrat = False
        Dim IdCtr As Integer = MayStatVal("select c.IdContr from contratstbl c where c.NomPrenoms like '" & Apost(CbxNumMatr.SelectedItem) & "' order by c.DebutContr desc limit 1", LocalConnxStr)
        'MsgBox("update contratstbl set FinContr='" & dtpDateSortie.SelectedDate.Value.ToString("yyyy-MM-dd") & "' where IdAffec=" & IdCtr & "")
        FinContrat = SaveNewData("update contratstbl set FinContr='" & dtpDateSortie.SelectedDate.Value.ToString("yyyy-MM-dd") & "' where IdContr=" & IdCtr & "", iConnxStr)
    End Function
    Function FinAgent() As Boolean
        FinAgent = False
        FinAgent = SaveNewData("update agents set actif=0, DateSortie='" & dtpDateSortie.SelectedDate.Value.ToString("yyyy-MM-dd") & "' where NumMatr='" & Apost(CbxNumMatr.SelectedItem) & "' ", iConnxStr) 'where IdContr=" & IdCtr & "")
    End Function


    Private Sub BtnAgentListeSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentListeSearch.KeyDown
        If e.Key = Key.Enter Then
            SearchAffectation(BtnAgentListeSearch.Text)
            inputTimer.Stop()
        End If
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Sorties "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Sorties du " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgAgentSortie, "Point_Sorties", iTitre) '"Point_Entrees"

    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        InialiseLoad()
        AfficheStat?.Invoke()
        AfficheMemos?.Invoke()

    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des SORTIES - AGENTS", "A ce jour " & Today, Me)
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
        Dim istr As String = "SELECT s.Client, a.NomPrenoms, date(so.DateSortie) as DateSortie, so.MotifSortie, so.Obs  "
        istr &= " FROM (Agents a JOIN sortie so ON a.NomPrenoms = so.NomsPrenoms) JOIN affectation aff ON aff.AgentAffect = a.NomPrenoms JOIN SiteAffectation s ON s.NomSite = aff.SiteAffect "
        istr &= "  WHERE so.DateSortie >='" & debut.ToString("yyyy-MM-dd") & "' and so.DateSortie <='" & Fin.ToString("yyyy-MM-dd") & "'"   ' date('now','-1 month','start of month','+24 day')  "
        istr &= " and aff.FinAffect >='" & debut.ToString("yyyy-MM-dd") & "'"  'date('now','-1 month','start of month','+24 day')"
        istr &= " and a.Actif=0"
        istr &= " GROUP BY s.Client, a.NomPrenoms  "
        istr &= " ORDER BY s.Client, a.NomPrenoms, aff.SiteAffect"
        'ShowTempMsg(istr)
        openPointMensuel(debut, Fin, istr, "SORTIES du " & debut.ToString("dd/MM/yyyy") & " au " & Fin.ToString("dd/MM/yyyy"), "SORTIES")
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
            'cmiModif.Visibility = Visibility.Collapsed
        End If
        If Not CanDeleteDataOrdinary And Not CanGlobalDataManage Then
            btnDeleteAg.Visibility = Visibility.Collapsed
            'cmiSuppr.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged
        If iFormLoaded Then
            SearchAffectation(BtnAgentListeSearch.Text)
            inputTimer.Stop()
        End If
    End Sub

    Private Sub dtpFinSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then
            SearchAffectation(BtnAgentListeSearch.Text)
            inputTimer.Stop()
        End If
    End Sub
End Class
