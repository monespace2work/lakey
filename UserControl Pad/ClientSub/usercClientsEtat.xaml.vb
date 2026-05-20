Imports System.Windows.Threading
Public Class usercClientsEtat
    Dim inputTimer As DispatcherTimer
    Dim iFormLoaded As Boolean = False
    Public iConnxStr As String
    Dim LocalConnxStr As String

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        LocalConnxStr = $"Data Source={DbPath};Version=3;"


        Dim istr As String = " SELECT s.Client, a.AgentAffect FROM SiteAffectation s JOIN affectation a ON a.SiteAffect = s.NomSite   "
        istr += " WHERE s.Actif = 1 and a.FinAffect >= date() GROUP BY s.Client, a.AgentAffect "
        istr += " ORDER BY s.Client, a.AgentAffect "
        DgClientsEtat.ItemsSource = LoadDataTable(istr, $"Data Source={DbPath};Version=3;").DefaultView
        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(3)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed
        iFormLoaded = True
    End Sub
    Sub SearchData(KeyWord As String)
        'elpStatus.Visibility = Visibility.Visible
        Dim sufix As String = ""
        If KeyWord = "" Then
            'sufix = " and (c.Actif=1 and crt.FinContr>='" & Today.ToString("yyyy-MM-dd") & "')"
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = ""
        Else

        End If

        Dim istr As String = " SELECT s.Client, a.AgentAffect FROM SiteAffectation s JOIN affectation a ON a.SiteAffect = s.NomSite   "
        istr += " WHERE (s.Actif = 1 and a.FinAffect >= date()) and"
        istr += " (s.Client like '%" & KeyWord & "%' "
        istr += " or a.AgentAffect like '%" & KeyWord & "%' ) " & sufix
        istr += " GROUP BY s.Client, a.AgentAffect "
        istr += " ORDER BY s.Client, a.AgentAffect "
        maj()
        DgClientsEtat.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre d'éléments : " & MayStatVal("select count(*) from (" & istr & ")", LocalConnxStr) '& " | Nombre de clients inactifs: " & MayStatVal("select count(*) from  (" & istr & ") where Actif=0 ", LocalConnxStr) 'where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")") '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        'itext += Environment.NewLine
        tblNbClientEtatsActifs.Text = itext '"En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")

        'NewData = True
        iFormLoaded = True
        'elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnPointG_Click(sender As Object, e As RoutedEventArgs) Handles btnPointG.Click
        Dim istr As String = "SELECT s.Client, a.AgentAffect "
        istr &= " FROM SiteAffectation s JOIN affectation a ON a.SiteAffect = s.NomSite "
        istr += " WHERE s.Actif = 1 and a.FinAffect >= date() "
        istr &= " GROUP BY s.Client, a.AgentAffect  "
        istr &= " ORDER BY s.Client, a.AgentAffect"
        'ShowTempMsg(istr)
        openPointMensuel(Today, Today, istr, "DETAILS DES AGENTS au " & Today.ToString("dd/MM/yyyy"), "DETAILS-AGENTS_PAR_CLIENT")

    End Sub

    Private Sub BtnClientEtatSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnClientEtatSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()
    End Sub
    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimer.Stop()
        Dispatcher.Invoke(Sub() SearchData(BtnClientEtatSearch.Text))
    End Sub

    Private Sub BtnClientEtatSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnClientEtatSearch.KeyDown
        If e.Key = Key.Enter Then
            inputTimer.Stop()
            SearchData(BtnClientEtatSearch.Text)
        End If

    End Sub
End Class
