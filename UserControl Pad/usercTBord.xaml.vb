Imports System.Data
Imports LiveCharts
Imports LiveCharts.Wpf
Imports System.Data.SQLite
Imports System.Windows.Threading
Public Class usercTBord
    Public Property NbSeries As New ChartValues(Of Double)()
    Public Property NbSiteSeries As New ChartValues(Of Double)()
    Public Property NbTypeSiteSeries As New ChartValues(Of Double)()
    Public Property NbTrancheSeries As New ChartValues(Of Double)()
    Public Property SotieSeries As New ChartValues(Of Double)()
    Public Property MoisLabels As New List(Of String)()
    Public Property AgesLabels As New List(Of String)()
    Public Property VilleLabels As New List(Of String)()
    Public Property TypeLabels As New List(Of String)()
    Public PieCharClientSiteSeries As New SeriesCollection()
    Dim myDtbl, myDtblCv, myDtblType, myDtblClientActif, myDtblAgentsActif, myDtblContratEch, myDtblClientGrand10, myDtblClientAncien20, myDtblNewAgent, myDtblClientNbAgent, myDtblTrancheAgeAgent As New DataTable
    Dim inputTimerAgent, inputTimerClent As DispatcherTimer
    Dim iMinValue, iMaxValue As Integer
    Dim iMinAge, iMaxAge As Integer
    Dim retourDansMainWindow As Action
    Dim LocalConnxStr As String
    Public iConnxStr As String

    Public Sub New(cllb As Action)
        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = cllb

    End Sub


    Private Sub usercTBord_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        LocalConnxStr = $"Data Source={DbPath};Version=3;"
        ResetLoad()
    End Sub
    Sub ResetLoad()
        elpGlobale.Visibility = Visibility.Visible
        maj()
        incorrection()

        Try
            LoadEvolEffectif()
            LoadClientSite()
            LoadClientType()
            LoadAgentActifs()
            LoadClientActifs()
            LoadContratEcheance()
            LoadClientGrandT10()
            LoadClientAnciensT20()
            LoadNewAgents()
            LoadClientNbAgent()
            LoadTtrancheAgeAgent()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        inputTimerAgent = New DispatcherTimer()
        inputTimerAgent.Interval = TimeSpan.FromMilliseconds(1000)
        AddHandler inputTimerAgent.Tick, AddressOf AgentActifOnInputTimerElapsed

        inputTimerClent = New DispatcherTimer()
        inputTimerClent.Interval = TimeSpan.FromMilliseconds(1000)
        AddHandler inputTimerClent.Tick, AddressOf ClientActifOnInputTimerElapsed
        '=========================
        Try
            Dim irappel As String = RappelActifs()
            If irappel <> "" Then
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        '========================
        isLoadedTbord = True
        elpGlobale.Visibility = Visibility.Collapsed

    End Sub
    Sub incorrection()
        Try
            Dim iMsg As String = ""
            Dim msgSortiActif As String = ListeAgentSortisActif()
            Dim msgInActif As String = ListeAgentInActifSansDateSortie()
            If msgSortiActif <> "" And msgInActif <> "" Then
                iMsg = msgSortiActif & Environment.NewLine & Environment.NewLine & msgInActif
            ElseIf msgSortiActif <> "" Then
                iMsg = msgSortiActif
            Else
                If msgInActif <> "" Then
                    iMsg = msgInActif
                Else
                    iMsg = ""
                End If
            End If
            If iMsg <> "" Then MsgBox(iMsg,, "ATTENTION")

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Private Sub AgentActifOnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimerAgent.Stop()
        ' Afficher le texte du TextBox
        Dispatcher.Invoke(Sub() SeachAgentActif(BtnAgentActifSearch.Text))
    End Sub
    Private Sub ClientActifOnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimerClent.Stop()
        Dispatcher.Invoke(Sub() SeachClientActif(BtnClientActifSearch.Text))
    End Sub

    Sub LoadEvolEffectif()
        Try
            myDtbl = EvolTrimEffectif(Today.AddMonths(-0), 12) ', iConnxStr)
            If myDtbl.Columns.Count > 0 Then
                For iIdx As Integer = 0 To myDtbl.Columns.Count - 1
                    If iIdx > 0 Then
                        Dim ival As Integer = myDtbl.Rows(0).Item(iIdx)
                        If iIdx = 1 Then
                            iMinValue = ival
                        Else
                            If iMinValue >= ival Then iMinValue = ival
                        End If
                        If iMaxValue <= ival Then iMaxValue = ival

                    End If
                Next
                dgNbEvol.ItemsSource = myDtbl.DefaultView ' EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
                'MsgBox()
                If iMinValue < iMaxValue Then UpDateChartEffectif()
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub LoadClientSite()
        Try
            myDtblCv = LoadDataTable("select s.Localisation, count(s.NomSite) as Nbr from siteaffectation s where  s.Actif=1 group by s.Localisation") ', LocalConnxStr)
            dgSiteVile.ItemsSource = myDtblCv.DefaultView ' EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
            UpDateChartClientVile()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub LoadClientType()
        Try
            myDtblType = LoadDataTable("select s.TypeSite, count(s.NomSite) as Nombre from siteaffectation s where  s.Actif=1 group by s.TypeSite") ', LocalConnxStr)
            dgSiteType.ItemsSource = myDtblType.DefaultView ' EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
            UpDateChartClientType()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub LoadClientNbAgent()
        Try
            Dim iDt As New DataTable
            Dim iStr As String = "select s.Client, count(s.Client) as NbAgent from siteaffectation s  join affectation a on a.SiteAffect=s.NomSite "
            iStr += " where  s.Actif=1 and a.FinAffect>=date() and  s.Client not like '%g2s%' " ' c.FinContr<='" & FinDuMois(Today).AddDays(15).ToString("yyyy-MM-dd") & "' and a.actif=1"
            iStr += " group by s.Client  order by NbAgent desc"
            iDt.Columns.Add("Regroup")
            iDt.Columns.Add("Nb de clients")

            Dim Agent1 As Integer = 0
            Dim Agent2_4 As Integer = 0
            Dim Agent5_9 As Integer = 0
            Dim Agent10plus As Integer = 0
            Using iconn As New SQLiteConnection(iConnectStr)
                iconn.Open()
                Using icmd As New SQLiteCommand(iStr, iconn)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader

                    While iReader.Read
                        If Not IsDBNull(iReader(1)) Then
                            If iReader(1) <= 1 Then
                                Agent1 += 1
                            ElseIf iReader(1) <= 4 Then
                                Agent2_4 += 1
                            ElseIf iReader(1) <= 9 Then
                                Agent5_9 += 1
                            ElseIf iReader(1) >= 10 Then
                                Agent10plus += 1
                            End If
                        End If
                    End While
                End Using
            End Using
            iDt.Rows.Add("01 Agent", Agent1)
            iDt.Rows.Add("2 - 4 Agents", Agent2_4)
            iDt.Rows.Add("5 - 9 Agents", Agent5_9)
            'iDt.Rows.Add("7 - 9", Agent7_9)
            iDt.Rows.Add("10 Agents et plus", Agent10plus)
            myDtblClientNbAgent = iDt 'LoadDataTable(iStr)
            dgClientNbAgent.ItemsSource = iDt.DefaultView 'EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
            UpDateChartClientNbAgent()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub LoadTtrancheAgeAgent()
        Try
            Dim iDt As New DataTable
            Dim iStr As String = "SELECT (strftime('%Y','now') -strftime('%Y',a.DateNaiss)) age from agents a where a.Actif=1 order by age "
            'iStr += " where  s.Actif=1 and a.FinAffect>=date() and  s.Client not like '%g2s%' " ' c.FinContr<='" & FinDuMois(Today).AddDays(15).ToString("yyyy-MM-dd") & "' and a.actif=1"
            'iStr += " group by s.Client  order by NbAgent desc"
            iDt.Columns.Add("Age")
            'iDt.Columns.Add("Nbre")
            iDt.Columns.Add("<=20ans")
            iDt.Columns.Add("21-25ans")
            iDt.Columns.Add("26-30ans")
            iDt.Columns.Add("31-35ans")
            iDt.Columns.Add("26-40ans")
            iDt.Columns.Add("31-45ans")
            iDt.Columns.Add("26-50ans")
            iDt.Columns.Add("31-55ans")
            iDt.Columns.Add("31-60ans")
            iDt.Columns.Add(">60ans")

            'MsgBox(iStr)
            Dim MoinsDe20ans As Integer = 0
            Dim De21a25ans As Integer = 0
            Dim De26a30ans As Integer = 0
            Dim De31a35ans As Integer = 0
            Dim De36a40ans As Integer = 0
            Dim De41a45ans As Integer = 0
            Dim De46a50ans As Integer = 0
            Dim De51a55ans As Integer = 0
            Dim De56a60ans As Integer = 0
            Dim PusDe60ans As Integer = 0
            Using iconn As New SQLiteConnection(iConnectStr)
                iconn.Open()
                Using icmd As New SQLiteCommand(iStr, iconn)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader

                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            If iReader(0) <= 20 Then MoinsDe20ans += 1
                            If iReader(0) > 20 And iReader(0) <= 25 Then De21a25ans += 1
                            If iReader(0) > 25 And iReader(0) <= 30 Then De26a30ans += 1
                            If iReader(0) > 30 And iReader(0) <= 35 Then De31a35ans += 1
                            If iReader(0) > 35 And iReader(0) <= 40 Then De36a40ans += 1
                            If iReader(0) > 40 And iReader(0) <= 45 Then De41a45ans += 1
                            If iReader(0) > 45 And iReader(0) <= 50 Then De46a50ans += 1
                            If iReader(0) > 50 And iReader(0) <= 55 Then De51a55ans += 1
                            If iReader(0) > 55 And iReader(0) <= 60 Then De56a60ans += 1
                            If iReader(0) > 60 Then PusDe60ans += 1
                            'If iReader(1) > 25 And iReader(1) <= 30 Then De26a30ans += 1
                        End If
                    End While
                End Using
            End Using
            Dim nombres As New List(Of Integer) From {MoinsDe20ans, De21a25ans, De26a30ans, De31a35ans, De36a40ans, De41a45ans, De46a50ans, De51a55ans, De56a60ans, PusDe60ans}
            iDt.Rows.Add("Nbre", MoinsDe20ans, De21a25ans, De26a30ans, De31a35ans, De36a40ans, De41a45ans, De46a50ans, De51a55ans, De56a60ans, PusDe60ans)
            iMinAge = nombres.Min()
            iMaxAge = nombres.Max()
            If iMinAge < 5 Then iMinAge = 0
            myDtblTrancheAgeAgent = iDt 'LoadDataTable(iStr)

            dgTrancheAge.ItemsSource = iDt.DefaultView 'EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
            UpDateChartTrancheAgeAgent()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Sub LoadClientActifs()
        Try
            Dim iStr As String = "select c.NomClient,s.NomSite, s.RespSite, s.Localisation,s.TypeSite from client c join siteaffectation s on c.NomClient =s.Client "
            iStr += " where c.Actif=1 and s.Actif=1"
            iStr += " order by c.NomClient,s.NomSite"
            myDtblClientActif = LoadDataTable(iStr) ', LocalConnxStr)
            dgClientActifs.ItemsSource = myDtblClientActif.DefaultView ' EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
            lblNbClentActif.Content = myDtblClientActif.Rows.Count
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub LoadAgentActifs()
        Try
            Dim iStr As String = "select a.NomPrenoms,a.Contacts,a.NumMatr,af.SiteAffect,s.Localisation,date(a.DateEntree) as DateEntree   from agents a join contratstbl c on c.NomPrenoms=a.NumMatr join affectation af on af.AgentAffect=a.NomPrenoms join siteaffectation s on s.NomSite=af.SiteAffect "
            iStr += " where c.FinContr>=date() and af.FinAffect>=date() and s.Actif=1"
            iStr += " order by a.NomPrenoms"
            myDtblAgentsActif = LoadDataTable(iStr) ', LocalConnxStr)
            dgAgentActifs.ItemsSource = myDtblAgentsActif.DefaultView ' EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
            Dim iList As String = ListeAgentFinContrat()
            Dim ilistAff As String = ListeAgentFinAffectation()
            ColorDgRowsFinContratAffectation(dgAgentActifs, 0, iList, ilistAff)
            lblNbAgentActif.Content = myDtblAgentsActif.Rows.Count
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        'dgNbEvol.Visibility = Visibility.Collapsed
        'UpDateChartEffectif()
        'UpDateChartClientType()
    End Sub

    Private Sub BtnAgentActifSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentActifSearch.TextChanged
        inputTimerAgent.Stop()
        inputTimerAgent.Start()
        'SeachAgentActif(BtnAgentActifSearch.Text)

    End Sub
    Sub SeachAgentActif(KeyWord As String)
        Try
            Dim iStr As String = "select a.NomPrenoms,a.Contacts,a.NumMatr,af.SiteAffect,s.Localisation,date(a.DateEntree) as DateEntree from agents a join contratstbl c on c.NomPrenoms=a.NumMatr join affectation af on af.AgentAffect=a.NomPrenoms join siteaffectation s on s.NomSite=af.SiteAffect "
            iStr += " where (c.FinContr>='" & Today.ToString("yyyy-MM-dd") & "' and af.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "' and s.Actif=1)"
            'iStr += " where (c.FinContr>='" & Today.ToString("yyyy-MM-dd") & "' and a.Actif=1)"
            iStr += " and (a.NomPrenoms like '%" & Apost(KeyWord) & "%'"
            iStr += " or a.Contacts like '%" & Apost(KeyWord) & "%'"
            iStr += " or a.NumMatr like '%" & Apost(KeyWord) & "%'"
            iStr += " or af.SiteAffect like '%" & Apost(KeyWord) & "%'"
            ' iStr += " or a.NumMatr like %" & Apost(KeyWord) & "%"
            ' iStr += " or a.NumMatr like %" & Apost(KeyWord) & "%"
            ' iStr += " or a.NumMatr like %" & Apost(KeyWord) & "%"
            iStr += " or s.Localisation like '%" & Apost(KeyWord) & "%' )"
            iStr += " order by a.NomPrenoms"
            myDtblAgentsActif = LoadDataTable(iStr) ', LocalConnxStr)
            dgAgentActifs.ItemsSource = myDtblAgentsActif.DefaultView ' EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
            lblNbAgentActif.Content = myDtblAgentsActif.Rows.Count
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs) Handles cmiResume.Click
        Dim rowView As DataRowView = TryCast(dgAgentActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAgentActif(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "INFOS - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiContrat_Click(sender As Object, e As RoutedEventArgs) Handles cmiContrat.Click
        Dim rowView As DataRowView = TryCast(dgAgentActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeCtr(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "CONTRAT - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiAffectation_Click(sender As Object, e As RoutedEventArgs) Handles cmiAffectation.Click
        Dim rowView As DataRowView = TryCast(dgAgentActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAffect(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "AFFFECTATIONS - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiAbsence_Click(sender As Object, e As RoutedEventArgs) Handles cmiAbsence.Click
        Dim rowView As DataRowView = TryCast(dgAgentActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAbs(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "ABSENCES/PERMISSIONS/CONGES - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiHeureSup_Click(sender As Object, e As RoutedEventArgs) Handles cmiHeureSup.Click
        Dim rowView As DataRowView = TryCast(dgAgentActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeHS(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "HEURES SUPPLEMENTAIRES - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiResumeClient_Click(sender As Object, e As RoutedEventArgs) Handles cmiResumeClient.Click
        Dim rowView As DataRowView = TryCast(dgClientActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeClient(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "INFOS - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiAgentt_Click(sender As Object, e As RoutedEventArgs) Handles cmiAgentt.Click
        Dim rowView As DataRowView = TryCast(dgClientActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ListeAgentClient(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "AGENTS AFFECTES - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiSites_Click(sender As Object, e As RoutedEventArgs) Handles cmiSites.Click
        Dim rowView As DataRowView = TryCast(dgClientActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ListeSiteClient(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "SITES - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiPostes_Click(sender As Object, e As RoutedEventArgs) Handles cmiPostes.Click
        Dim rowView As DataRowView = TryCast(dgClientActifs.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ListePosteClient(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "POSTES - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub chartVentes_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles chartVentes.MouseDoubleClick
        '  openZoomWdw(sender)
    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("TABLEAU DE BORD GENERAL", "A ce jour " & Today, Me)
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

    Sub SeachClientActif(KeyWord As String)
        Try
            Dim iStr As String = "select c.NomClient, s.NomSite,  s.RespSite, s.Localisation,s.TypeSite  from client c join siteaffectation s on c.NomClient =s.Client  "
            iStr += " where (c.Actif=1 and s.Actif=1) "
            iStr += " and (c.NomClient like '%" & Apost(KeyWord) & "%'"
            iStr += " or s.NomSite like '%" & Apost(KeyWord) & "%'"
            iStr += " or s.RespSite like '%" & Apost(KeyWord) & "%'"
            iStr += " or s.Localisation like '%" & Apost(KeyWord) & "%'"
            iStr += " or s.TypeSite like '%" & Apost(KeyWord) & "%' )"
            iStr += " order by s.NomSite, s.Localisation"
            myDtblClientActif = LoadDataTable(iStr) ', LocalConnxStr)
            dgClientActifs.ItemsSource = myDtblClientActif.DefaultView ' EvolTrimEffectif(Today.AddMonths(-0)).DefaultView
            lblNbClentActif.Content = myDtblClientActif.Rows.Count
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub BtnClientActifSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnClientActifSearch.TextChanged
        inputTimerClent.Stop()
        inputTimerClent.Start()
        'SeachClientActif(BtnClientActifSearch.Text)
    End Sub

    Sub LoadContratEcheance()
        Try
            Dim iStr As String = "select a.NomPrenoms,format(c.FinContr) as FinContr , n.IdNature as Nature,format(c.DebutContr) as DebutContr from contratstbl c  join agents a on a.NumMatr=c.NomPrenoms join contratsnature n on n.NatureContr=c.NaturContr "
            iStr += " where c.FinContr>='" & DebutDuMois(Today).ToString("yyyy-MM-dd") & "' and c.FinContr<='" & FinDuMois(Today).AddDays(15).ToString("yyyy-MM-dd") & "' and a.actif=1"
            iStr += " order by c.FinContr,a.NomPrenoms"
            myDtblContratEch = LoadDataTable(iStr) ', LocalConnxStr)
            dgContratEcheant.ItemsSource = myDtblContratEch.DefaultView
            lblNbContratEcheance.Content = myDtblContratEch.Rows.Count
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub


    Sub LoadNewAgents()
        Try
            Dim iStr As String = "select a.NomPrenoms, format(c.DebutContr) as Début from agents a join contratstbl c on a.NumMatr=c.NomPrenoms "
            iStr += " where c.FinContr>='" & DebutDuMois(Today).ToString("yyyy-MM-dd") & "' and a.Actif=1 and c.DebutContr>='" & DebutDuMois(Today).ToString("yyyy-MM-dd") & "'"  ' c.FinContr<='" & FinDuMois(Today).AddDays(15).ToString("yyyy-MM-dd") & "' and a.actif=1"
            iStr += " order by a.DateEntree desc, a.NomPrenoms "
            myDtblNewAgent = LoadDataTable(iStr) ', LocalConnxStr)
            dgNewAgent.ItemsSource = myDtblNewAgent.DefaultView
            lblNbContratNouveau.Content = myDtblNewAgent.Rows.Count
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Sub LoadClientGrandT10()
        Try
            Dim iStr As String = "select s.Client, count(s.Client) as NbAgent from siteaffectation s  join affectation a on a.SiteAffect=s.NomSite "
            iStr += " where s.Actif=1 and a.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "' and s.Client not like '%g2s%'"
            iStr += " group by s.Client  order by NbAgent desc limit 10"
            myDtblClientGrand10 = LoadDataTable(iStr) ', LocalConnxStr)
            dgGrandClients.ItemsSource = myDtblClientGrand10.DefaultView
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub LoadClientAnciensT20()
        Try
            Dim iStr As String = "select c.NomClient, format(c.DateDebutCollabor) as Debut,c.MontantFacturationUnitaire as PrixUnit from client c "
            iStr += " where c.Actif=1 and  c.NomClient not like '%g2s%' and c.NomClient not like '%TOGO INTERIM%' "
            iStr += " order by c.DateDebutCollabor limit 20 "
            myDtblClientAncien20 = LoadDataTable(iStr) ', LocalConnxStr)
            dgAnciensClients.ItemsSource = myDtblClientAncien20.DefaultView
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Sub UpDateChartEffectif()
        If myDtbl.Columns.Count - 1 > 0 Then
            Try
                NbSeries.Clear()
                MoisLabels.Clear()
                chartVentes.AxisY(0).MinValue = iMinValue
                chartVentes.AxisY(0).MaxValue = iMaxValue
                For idx As Integer = 0 To myDtbl.Columns.Count - 1
                    If idx <> 0 Then
                        MoisLabels.Add(myDtbl.Columns.Item(idx).ToString)
                        NbSeries.Add(myDtbl.Rows(0).Item(idx))
                        SotieSeries.Add(myDtbl.Rows(2).Item(idx))
                    Else

                    End If
                    DataContext = Me
                Next

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Sub UpDateChartClientVile()
        Try
            If myDtblCv.Rows.Count > 0 Then
                NbSiteSeries.Clear()
                VilleLabels.Clear()
                For idx As Integer = 0 To myDtblCv.Rows.Count - 1
                    If idx >= 0 Then
                        VilleLabels.Add(Left(myDtblCv.Rows(idx).Item(0).ToString, 5))
                        'PieCharClientSiteSeries.Add(New PieChart() With {.Title = "rrrr"})
                        NbSiteSeries.Add(myDtblCv.Rows(idx).Item(1))
                    Else

                    End If
                    DataContext = Me
                Next
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub UpDateChartClientType()
        Try
            If myDtblType.Rows.Count > 0 Then
                NbTypeSiteSeries.Clear()
                TypeLabels.Clear()
                Dim PieChartSeries As New SeriesCollection

                For idx As Integer = 0 To myDtblType.Rows.Count - 1
                    If idx >= 0 Then
                        TypeLabels.Add(Left(myDtblType.Rows(idx).Item(0), 3) & ".")
                        'PieCharClientSiteSeries.Add(New PieChart() With {.Title = "rrrr"})
                        NbTypeSiteSeries.Add(myDtblType.Rows(idx).Item(1))
                        PieChartSeries.Add(New PieSeries With {
                                       .Title = myDtblType.Rows(idx).Item(0),
                                       .Values = New ChartValues(Of Integer)({myDtblType.Rows(idx).Item(1)}),
                                       .DataLabels = True
                    })
                    Else

                    End If
                    DataContext = Me
                Next
                PiechartClientType.Series = PieChartSeries
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub UpDateChartClientNbAgent()
        Try
            If myDtblType.Rows.Count > 0 Then
                Dim PieChartSeries As New SeriesCollection
                For idx As Integer = 0 To myDtblType.Rows.Count - 1
                    If idx >= 0 Then
                        PieChartSeries.Add(New PieSeries With {
                                       .Title = myDtblClientNbAgent.Rows(idx).Item(0),
                                       .Values = New ChartValues(Of Integer)({myDtblClientNbAgent.Rows(idx).Item(1)}),
                                       .DataLabels = True
                    })
                    Else

                    End If
                    DataContext = Me
                Next
                PiechartClientType.Series = PieChartSeries
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Sub UpDateChartTrancheAgeAgent()
        If myDtblTrancheAgeAgent.Columns.Count - 1 > 0 Then
            Try
                NbTrancheSeries.Clear()
                AgesLabels.Clear()
                chartTAges.AxisY(0).MinValue = iMinAge
                chartTAges.AxisY(0).MaxValue = iMaxAge
                For idx As Integer = 0 To myDtblTrancheAgeAgent.Columns.Count - 1
                    If idx = 0 Then
                        AgesLabels.Add("-")
                        NbTrancheSeries.Add(0)
                    Else
                        AgesLabels.Add(myDtblTrancheAgeAgent.Columns.Item(idx).ToString)
                        NbTrancheSeries.Add(myDtblTrancheAgeAgent.Rows(0).Item(idx))
                        'SotieSeries.Add(myDtblTrancheAgeAgent.Rows(2).Item(idx))

                    End If
                    DataContext = Me
                Next

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If

    End Sub

    Sub VisibityControl(ByVal MyVal As Decimal, ByVal MyBotton As Button, ByVal MyText As String)
        'Dim ButtonControl As New Button
        Try
            If MyVal <> 0 Then
                MyBotton.Content = MyText & " " & MyVal
                MyBotton.Visibility = Visibility.Visible
                MyBotton.ToolTip = MyBotton.Content
            Else
                MyBotton.Content = "0"
                MyBotton.Visibility = Visibility.Collapsed
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub


End Class
