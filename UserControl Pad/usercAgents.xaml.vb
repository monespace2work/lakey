
Imports System.ComponentModel
Imports System.Data
Imports System.Windows.Threading

Public Class usercAgents
    Dim itable As New DataTable
    Dim Ptt, Moy, Grd As New Collection
    Dim iFormLoaded As Boolean = False
    Dim inputTimerAgent As DispatcherTimer
    Dim retourDansMainWindow As Action
    Public Sub New(cllb As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = cllb

    End Sub
    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs) Handles cmiResume.Click
        Dim rowView As DataRowView = TryCast(DgEmployes.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAgentActif(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "INFOS - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiContrat_Click(sender As Object, e As RoutedEventArgs) Handles cmiContrat.Click
        Dim rowView As DataRowView = TryCast(DgEmployes.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeCtr(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "CONTRAT - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If

    End Sub

    Private Sub cmiAffectation_Click(sender As Object, e As RoutedEventArgs) Handles cmiAffectation.Click
        Dim rowView As DataRowView = TryCast(DgEmployes.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAffect(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "AFFFECTATIONS - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub

    Private Sub cmiAbsence_Click(sender As Object, e As RoutedEventArgs) Handles cmiAbsence.Click
        Dim rowView As DataRowView = TryCast(DgEmployes.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAbs(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd, MsgBoxStyle.OkOnly, "ABSENCES/PERMISSIONS/CONGES - " & UCase(rowView.Row.ItemArray(0).ToString))
        End If
    End Sub
    Private Sub AgentActifOnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimerAgent.Stop()
        Dispatcher.Invoke(Sub() SeachAgentActif(BtnAgentSearch.Text))
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpGlobale.Visibility = Visibility.Visible
        inputTimerAgent = New DispatcherTimer()
        inputTimerAgent.Interval = TimeSpan.FromMilliseconds(3000)
        AddHandler inputTimerAgent.Tick, AddressOf AgentActifOnInputTimerElapsed

        Dim DateStr As String = MyStatValStr("select min( a.DateEntree) from agents a ")
        If IsDate(DateStr) Then
            Try
                dtpDebutSearch.SelectedDate = CDate(DateStr)
                dtpFinSearch.SelectedDate = Today
                Dim iStr As String = "select ag.NomPrenoms,ag.Sexe,ag.Fonction,ag.Contacts, sit.NomSite,sit.Localisation as Ville,cast(strftime('%Y','now')-strftime('%Y',ag.DateNaiss) as integer) as Age,date(ag.DateNaiss) as DateNaiss,date(ag.DateEntree) as DateEntree,cast((julianday('now')-julianday(ag.DateEntree))/30.4375 as integer) as [Ancienneté(mois)] "
                iStr += " from agents ag join affectation af on ag.NomPrenoms =af.AgentAffect join siteaffectation sit on sit.NomSite=af.SiteAffect "
                iStr += " where af.FinAffect >='" & Today.ToString("yyyy-MM-dd") & "' order by ag.NomPrenoms"
                itable = LoadDataTable(iStr)
                DgEmployes.ItemsSource = itable.DefaultView
                tblNbAgents.Text = "Nombre d'agents : " & DgEmployes.Items.Count
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
            ColorDgRowsAge(DgEmployes,, 35, 50)
            'MsgBox(MajusculeEnDebutDuTexte("   mon ciel est plus grand avec dieu "))
            iFormLoaded = True
        End If
        elpGlobale.Visibility = Visibility.Collapsed
    End Sub

    Private Sub BtnAgentAbsenceSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentSearch.KeyDown
        If e.Key = Key.Enter And iFormLoaded Then SeachAgentActif(BtnAgentSearch.Text)

    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then SeachAgentActif(BtnAgentSearch.Text)
    End Sub

    Private Sub BtnAgentSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentSearch.TextChanged
        inputTimerAgent.Stop()
        inputTimerAgent.Start()
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Etat des employés actuels"
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Etat des employés actuels" ' entre " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " et " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgEmployes, "Point_Employés-actifs", iTitre) '"Point_Entrees"


    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des AGENTS EN ACTIVITE", "A  ce jour :" & Today, Me)
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

    Private Sub DgEmployes_Sorting(sender As Object, e As DataGridSortingEventArgs) Handles DgEmployes.Sorting
        'If iFormLoaded Then colorDgRows()
        ColorDgRowsAge(DgEmployes,, 35, 50)
    End Sub

    Sub SeachAgentActif(KeyWord As String)
        elpGlobale.Visibility = Visibility.Visible
        Dim iStr As String = "select ag.NomPrenoms,ag.Sexe,ag.Fonction,ag.Contacts, sit.NomSite,sit.Localisation as Ville,cast(strftime('%Y','now')-strftime('%Y',ag.DateNaiss) as integer) as Age,date(ag.DateNaiss) as DateNaiss,date(ag.DateEntree) as DateEntree,cast((julianday('now')-julianday(ag.DateEntree))/30.4375 as integer) as [Ancienneté(mois)] "
        iStr += " from agents ag join affectation af on ag.NomPrenoms =af.AgentAffect join siteaffectation sit on sit.NomSite=af.SiteAffect "
        If dtpDebutSearch.SelectedDate.HasValue AndAlso dtpFinSearch.SelectedDate.HasValue Then
            iStr += " where (ag.DateEntree >='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "') and ag.DateEntree <='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and af.FinAffect >=date() "
        Else
            iStr += " where (af.FinAffect >='" & Today.ToString("yyyy-MM-dd") & "') "
        End If
        iStr += " and ("
        iStr += " ag.NomPrenoms like '%" & Apost(KeyWord) & "%'"
        iStr += " or  ag.Fonction like '%" & Apost(KeyWord) & "%'"
        iStr += " or  ag.Contacts like '%" & Apost(KeyWord) & "%'"
        iStr += " or  sit.NomSite like '%" & Apost(KeyWord) & "%'"
        iStr += " or  sit.Localisation like '%" & Apost(KeyWord) & "%'"
        iStr += ") "
        iStr += " order by ag.NomPrenoms"
        itable = LoadDataTable(iStr)
        DgEmployes.ItemsSource = itable.DefaultView
        tblNbAgents.Text = "Nombre d'agents : " & DgEmployes.Items.Count
        ColorDgRowsAge(DgEmployes,, 35, 50)
        elpGlobale.Visibility = Visibility.Collapsed

    End Sub

End Class

