Imports System.Windows.Threading
Imports System.Data.SQLite
Imports System.Data
Imports System.Threading.Tasks

Public Class usercAgentsAffectation
    Public iConnxStr As String
    Public LocalConnxStr As String
    Dim DateFinDernierAffectation As Date = DateCreationSociete
    Dim IdDernierAffectation As Integer = 0
    Dim SiteDernierAffectation As String = "(Nouveau)"
    Dim NewData As Boolean = True
    Dim iFormLoaded As Boolean = False
    Dim RefAffect As Integer = 0
    Dim ChangeYN As Boolean = False
    Dim IsSaving As Boolean = False
    Dim inputTimer As DispatcherTimer
    Dim retourDansMainWindow As Action
    Public Shared AfficheStat As Action
    Public Sub New(callback As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = callback
    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
        DbPathGeneral = PaphBackUp()
        LocalConnxStr = $"Data Source={DbPath};Version=3;"
        iConnxStr = LocalConnxStr
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

    Sub ActivSaveBtn()
        Dim miDateRef As Date = dtpDebutAffect.SelectedDate.Value
        If RefAffect >= 1 And TbxAncienSite.Text <> "" And CbxNouveauSite.Items.Contains(CbxNouveauSite.SelectedItem) And CbxNumMatr.Items.Contains(CbxNumMatr.SelectedItem) And (dtpFinAffect.SelectedDate.HasValue AndAlso dtpFinAffect.SelectedDate.Value > miDateRef) Then
            btnSave.Visibility = Visibility.Visible
            btnSaveContinue.Visibility = Visibility.Visible
        Else
            btnSave.Visibility = Visibility.Collapsed
            btnSaveContinue.Visibility = Visibility.Collapsed
        End If
        ChangeYN = True
        stpButtonsPad.Visibility = Visibility.Visible

    End Sub

    Sub InialiseLoad()
        elpStatus.Visibility = Visibility.Visible
        NewData = True
        AfficheProfilUser()

        maj()
        Dim istr As String = " select af.AgentAffect, format(af.DateAffect) as Début, format(af.FinAffect) as Fin, af.SiteAffect as Site, af.SiteAffectPrecedent, af.ObsAffect, af.IdAffec "
        istr += " from affectation af join agents ag on ag.NomPrenoms=af.AgentAffect "
        istr += " where ag.Actif=1 and af.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "'"
        istr += " and af.DateAffect>='" & CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")).ToString("yyyy-MM-dd") & "'"
        istr += " order by  ag.Actif desc, af.AgentAffect, af.DateAffect desc  "
        DgAgentAffectation.ItemsSource = LoadDataTable(istr, iConnxStr).DefaultView
        Dim itext As String = "En cours de validité : " & DirectCast(DgAgentAffectation.ItemsSource, System.Data.DataView).Count
        itext += Environment.NewLine
        itext += MyAffectationVilleList("select s.Localisation as ville, count(*) as nbr from (" & istr & ") a join siteaffectation s on a.Site=s.NomSite where a.Fin>='" & Today.ToString("yyyy-MM-dd") & "' group by s.Localisation Order by nbr desc,ville", LocalConnxStr)
        tblNbAgentAffectation.Text = itext
        FilmyCbx("select s.NomSite from  siteaffectation s where s.Actif=1 order by s.NomSite ", CbxNouveauSite, iConnxStr)
        FilmyCbx("select a.NomPrenoms from contratstbl c join agents a on c.NomPrenoms=a.NumMatr where c.FinContr>='" & Today.ToString("yyyy-MM-dd") & "' and a.Actif group by a.NomPrenoms  order by a.NomPrenoms", CbxNom, iConnxStr)

        RefAffect = MayStatVal("select max(IdAffec) from Affectation ", iConnxStr) + 1
        dtpDebutAffect.SelectedDate = Today
        dtpDebutAffect.DisplayDateStart = DateCreationSociete
        dtpDebutAffect.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpFinAffect.SelectedDate = Today
        dtpFinAffect.DisplayDateStart = DateCreationSociete
        dtpFinAffect.DisplayDateEnd = FinDuMois(Today.AddMonths(12))
        ChangeYN = False
        dtpDebutSearch.SelectedDate = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")) ' CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = CDate("24/" & Today.ToString("MM/yyyy")) ' Today
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub SearchAffectation(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        Dim myw As New WaitFrm
        myw.Show()
        Dim sufix As String = ""
        If KeyWord = "" Then
            sufix = " and (ag.actif=1)" ' and af.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "')"
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = " "
        Else
            sufix = ""
        End If
        maj()
        Dim istr As String = " select af.AgentAffect, format(af.DateAffect) as Début, format(af.FinAffect) as Fin, af.SiteAffect as Site, af.SiteAffectPrecedent, af.ObsAffect, af.IdAffec "
        istr += " from affectation af join agents ag on ag.NomPrenoms=af.AgentAffect where "
        'istr += " where (ag.Actif=1 and af.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "')"
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then istr += " Début>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and Début<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and "
        istr += " (af.AgentAffect  like '%" & Apost(KeyWord) & "%' " 'af.SiteAffectPrecedent
        istr += " or af.SiteAffect  like '%" & Apost(KeyWord) & "%' "
        istr += " or af.SiteAffectPrecedent  like '%" & Apost(KeyWord) & "%' "
        istr += " or af.ObsAffect  like '%" & Apost(KeyWord) & "%' )" & Apost(sufix)
        istr += " order by  ag.Actif desc, af.AgentAffect, af.DateAffect desc  "
        DgAgentAffectation.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'", LocalConnxStr) & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'", LocalConnxStr) & " | TOTAL: " & DirectCast(DgAgentAffectation.ItemsSource, System.Data.DataView).Count '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        itext += Environment.NewLine
        itext += MyAffectationVilleList("select s.Localisation as ville, count(*) as nbr from (" & istr & ") a join siteaffectation s on a.Site=s.NomSite  group by s.Localisation Order by nbr desc,ville", LocalConnxStr)
        tblNbAgentAffectation.Text = itext '"En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")
        myw.Close()
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed

    End Sub
    Sub ResetValues()
        elpStatus.Visibility = Visibility.Visible
        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed
        AfficheProfilUser()
        NewData = True
        AfficheStat?.Invoke()
        Dim istr As String = " select af.AgentAffect, format(af.DateAffect) as Début, format(af.FinAffect) as Fin, af.SiteAffect as Site, af.SiteAffectPrecedent, af.ObsAffect, af.IdAffec "
        istr += " from affectation af join agents ag on ag.NomPrenoms=af.AgentAffect "
        istr += " where ag.Actif=1 and af.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "'"
        istr += " order by  ag.Actif desc, af.AgentAffect, af.DateAffect desc  "
        DgAgentAffectation.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "En cours de validité : " & DirectCast(DgAgentAffectation.ItemsSource, System.Data.DataView).Count
        itext += Environment.NewLine
        itext += MyAffectationVilleList("select s.Localisation as ville, count(*) as nbr from (" & istr & ") a join siteaffectation s on a.Site=s.NomSite where a.Fin>='2025/03/20' group by s.Localisation Order by nbr desc,ville", LocalConnxStr)
        tblNbAgentAffectation.Text = itext

        RefAffect = MayStatVal("select max(IdAffec) from Affectation ", iConnxStr) + 1
        dtpDebutAffect.SelectedDate = Today
        dtpDebutAffect.DisplayDateStart = DateCreationSociete
        dtpDebutAffect.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpFinAffect.SelectedDate = Today
        dtpFinAffect.DisplayDateStart = DateCreationSociete
        dtpFinAffect.DisplayDateEnd = FinDuMois(Today.AddMonths(12))

        TbxAncienSite.Text = ""
        TbxObs.Text = ""
        CbxNouveauSite.SelectedIndex = -1
        CbxNom.SelectedIndex = -1
        CbxNumMatr.SelectedIndex = -1
        'CbxTypCrt.SelectedIndex = -1
        iFormLoaded = True
        ChangeYN = False
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Private Sub SaveAffectationTransaction(isNew As Boolean,
                                           affectationId As Integer,
                                           agentName As String,
                                           startDate As Date,
                                           endDate As Date,
                                           previousSite As String,
                                           newSite As String,
                                           observations As String,
                                           previousAffectationId As Integer,
                                           closePreviousAffectation As Boolean)
        Dim writeConnectionString As String = ResolveMainDatabaseConnectionString(iConnxStr, True)
        Using connection As New SQLiteConnection(writeConnectionString)
            connection.Open()
            Using transaction As SQLiteTransaction = connection.BeginTransaction()
                If isNew AndAlso closePreviousAffectation AndAlso previousAffectationId > 0 Then
                    Using command As New SQLiteCommand(
                        "UPDATE Affectation SET FinAffect = @endDate WHERE IdAffec = @id",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@endDate", startDate.AddDays(-1).ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@id", previousAffectationId)
                        command.ExecuteNonQuery()
                    End Using
                End If

                If isNew Then
                    Using command As New SQLiteCommand(
                        "INSERT INTO Affectation (IdAffec, AgentAffect, DateAffect, FinAffect, SiteAffectPrecedent, SiteAffect, ObsAffect) " &
                        "VALUES (@id, @agent, @startDate, @endDate, @previousSite, @newSite, @observations)",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@id", affectationId)
                        command.Parameters.AddWithValue("@agent", agentName)
                        command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@previousSite", previousSite)
                        command.Parameters.AddWithValue("@newSite", newSite)
                        command.Parameters.AddWithValue("@observations", observations)
                        command.ExecuteNonQuery()
                    End Using
                Else
                    Using command As New SQLiteCommand(
                        "UPDATE Affectation SET AgentAffect = @agent, DateAffect = @startDate, FinAffect = @endDate, " &
                        "SiteAffectPrecedent = @previousSite, SiteAffect = @newSite, ObsAffect = @observations WHERE IdAffec = @id",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@agent", agentName)
                        command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@previousSite", previousSite)
                        command.Parameters.AddWithValue("@newSite", newSite)
                        command.Parameters.AddWithValue("@observations", observations)
                        command.Parameters.AddWithValue("@id", affectationId)
                        command.ExecuteNonQuery()
                    End Using
                End If

                Dim operation As String = If(isNew,
                                             "SaveNewAffectation for " & agentName,
                                             "SaveModifAffect for " & agentName & " at " & affectationId)
                Using command As New SQLiteCommand(
                    "INSERT INTO jope (operation, operateur) VALUES (@operation, @operator)",
                    connection,
                    transaction)
                    command.Parameters.AddWithValue("@operation", operation)
                    command.Parameters.AddWithValue("@operator", UserName)
                    command.ExecuteNonQuery()
                End Using

                transaction.Commit()
            End Using
        End Using

        If Not LocalModeBb Then NotifyGeneralDatabaseChanged()
    End Sub

    Private Sub DeleteAffectationTransaction(affectationId As Integer)
        Dim writeConnectionString As String = ResolveMainDatabaseConnectionString(iConnxStr, True)
        Using connection As New SQLiteConnection(writeConnectionString)
            connection.Open()
            Using transaction As SQLiteTransaction = connection.BeginTransaction()
                Using command As New SQLiteCommand(
                    "DELETE FROM Affectation WHERE IdAffec = @id",
                    connection,
                    transaction)
                    command.Parameters.AddWithValue("@id", affectationId)
                    command.ExecuteNonQuery()
                End Using

                Using command As New SQLiteCommand(
                    "INSERT INTO jope (operation, operateur) VALUES (@operation, @operator)",
                    connection,
                    transaction)
                    command.Parameters.AddWithValue("@operation", "DeleteAff at " & affectationId)
                    command.Parameters.AddWithValue("@operator", UserName)
                    command.ExecuteNonQuery()
                End Using

                transaction.Commit()
            End Using
        End Using

        If Not LocalModeBb Then NotifyGeneralDatabaseChanged()
    End Sub

    Private Async Function RefreshAfterMutationAsync() As Task
        Dim mainWindow As MainWindow = TryCast(Application.Current?.MainWindow, MainWindow)
        mainWindow?.BeginDatabaseLoading()
        Try
            Try
                Dim synchronized As Boolean = Await Task.Run(
                    Function()
                        If LocalModeBb Then Return True
                        Return maj()
                    End Function)
                If Not synchronized Then
                    MessageBox.Show(
                        "L'opération est enregistrée, mais la copie locale n'a pas pu être actualisée. Les données affichées peuvent être anciennes.",
                        "Actualisation en attente",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning)
                End If
            Catch ex As Exception
                MessageBox.Show(
                    "L'opération est enregistrée, mais l'actualisation de la base locale a échoué : " & ex.Message,
                    "Actualisation impossible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning)
            End Try

            ResetValues()
        Finally
            mainWindow?.EndDatabaseLoading()
        End Try
    End Function


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
                If SiteActuElmt(CbxNom.SelectedItem) Then
                    TbxAncienSite.Text = SiteDernierAffectation

                End If

                TblIndicationNumMatr.Visibility = Visibility.Collapsed
                CbxNumMatr.Visibility = Visibility.Visible
            Else
                CbxNumMatr.SelectedIndex = -1
                TblIndicationNumMatr.Visibility = Visibility.Visible
                CbxNumMatr.Visibility = Visibility.Collapsed
                TblIndicationNumMatr.Visibility = Visibility.Collapsed
            End If
        End If
        ActivSaveBtn()

    End Sub

    Private Sub CbxNouveauSite_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNouveauSite.SelectionChanged
        SaisiCbx(sender, TblIndicationNouveauSite)
        ActivSaveBtn()

    End Sub

    Private Sub TbxAncienSite_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxAncienSite.TextChanged
        If TbxAncienSite.Text = "" Then
            TbxAncienSite.Text = "(Nouveau)"
        End If
        SaisiTbx(sender, TblIndicationAncienSite)
        ActivSaveBtn()
    End Sub

    Private Sub TbxObs_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxObs.TextChanged
        SaisiTbx(sender, TblIndicationObs)
        ActivSaveBtn()
    End Sub

    Private Sub dtpDebutAffect_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutAffect.SelectedDateChanged
        ActivSaveBtn()
        If dtpDebutAffect.SelectedDate.HasValue Then
            Dim refDate As Date = dtpDebutAffect.SelectedDate.Value
            dtpFinAffect.DisplayDateStart = refDate
            If dtpFinAffect.SelectedDate.HasValue AndAlso dtpFinAffect.SelectedDate.Value < refDate Then
                dtpFinAffect.SelectedDate = refDate
            End If
        End If
        dtpFinAffect.DisplayDateStart = dtpDebutAffect.SelectedDate.Value
        dtpFinAffect.DisplayDate = dtpDebutAffect.SelectedDate.Value
        dtpFinAffect.Focus()

    End Sub

    Private Sub dtpFinAffect_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpFinAffect.SelectedDateChanged
        ActivSaveBtn()
    End Sub

    Private Sub CbxNumMatr_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNumMatr.SelectionChanged
        ActivSaveBtn()
    End Sub


    Function SiteActuElmt(Name As String) ', Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        SiteActuElmt = False
        Dim iConxStr = LocalConnxStr
        Dim istr As String = "select a.IdAffec,a.SiteAffect, format(a.FinAffect),format(a.DateAffect) from  affectation a where a.AgentAffect like '" & Apost(Name) & "' and a.FinAffect>='" & Today.AddDays(-21).ToString("yyyy-MM-dd") & "' order by a.DateAffect desc,a.IdAffec desc limit 1"
        IdDernierAffectation = 0
        SiteDernierAffectation = "(Nouveau)"
        DateFinDernierAffectation = DateCreationSociete
        Try
            Using iconn As New SQLiteConnection(ResolveMainDatabaseConnectionString(iConxStr, False))
                iconn.Open()
                Using icmdstr As New SQLiteCommand(istr, iconn)
                    Dim iReader As SQLiteDataReader = icmdstr.ExecuteReader
                    Do While iReader.Read
                        If Not IsDBNull(iReader(0)) And IsNumeric(iReader(0)) Then IdDernierAffectation = iReader(0)
                        If Not IsDBNull(iReader(1)) Then SiteDernierAffectation = iReader(1)
                        If Not IsDBNull(iReader(2)) And IsDate(iReader(2)) Then DateFinDernierAffectation = iReader(2)
                    Loop
                    SiteActuElmt = True
                End Using
            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Function
    Private Sub btnNewData_Click(sender As Object, e As RoutedEventArgs) Handles btnNewData.Click
        stpSaisie.Visibility = Visibility.Visible
        btnNewData.Visibility = Visibility.Collapsed
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

    Private Async Sub btnSaveContinue_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveContinue.Click
        Await SaveAffectationAsync(False)
    End Sub

    Private Async Function SaveAffectationAsync(closeAfterSave As Boolean) As Task
        If IsSaving Then Return

        If Not dtpDebutAffect.SelectedDate.HasValue OrElse Not dtpFinAffect.SelectedDate.HasValue OrElse
           CbxNom.SelectedItem Is Nothing OrElse CbxNumMatr.SelectedItem Is Nothing OrElse
           CbxNouveauSite.SelectedItem Is Nothing OrElse String.IsNullOrWhiteSpace(TbxAncienSite.Text) Then
            MessageBox.Show("Veuillez renseigner tous les champs obligatoires.")
            Return
        End If

        Dim startDate As Date = dtpDebutAffect.SelectedDate.Value
        Dim endDate As Date = dtpFinAffect.SelectedDate.Value
        If endDate <= startDate Then
            MessageBox.Show("La date de fin doit être postérieure à la date de début.")
            Return
        End If
        If startDate = Today AndAlso
           MsgBox("La date <" & startDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
            dtpDebutAffect.Focus()
            Return
        End If
        If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) <> MsgBoxResult.Yes Then Return
        If LocalModeBb Then MessageBox.Show("Vous travaillez en MODE LOCAL.")

        Dim isNew As Boolean = NewData
        Dim affectationId As Integer = RefAffect
        Dim agentName As String = CbxNom.SelectedItem.ToString()
        Dim previousSite As String = TbxAncienSite.Text
        Dim newSite As String = CbxNouveauSite.SelectedItem.ToString()
        Dim observations As String = TbxObs.Text
        Dim previousAffectationId As Integer = IdDernierAffectation
        Dim closePreviousAffectation As Boolean =
            isNew AndAlso previousAffectationId > 0 AndAlso startDate < DateFinDernierAffectation
        Dim mainWindow As MainWindow = TryCast(Application.Current?.MainWindow, MainWindow)

        IsSaving = True
        btnSave.IsEnabled = False
        btnSaveContinue.IsEnabled = False
        Try
            Try
                mainWindow?.BeginDatabaseLoading()
                Await Task.Run(
                    Sub()
                        SaveAffectationTransaction(
                            isNew,
                            affectationId,
                            agentName,
                            startDate,
                            endDate,
                            previousSite,
                            newSite,
                            observations,
                            previousAffectationId,
                            closePreviousAffectation)
                    End Sub)
            Catch ex As Exception
                MessageBox.Show("Échec de l'enregistrement de l'affectation : " & ex.Message)
                Return
            Finally
                mainWindow?.EndDatabaseLoading()
            End Try

            MessageBox.Show(If(isNew, "Données enregistrées avec succès.", "Données modifiées avec succès."))
            Try
                Await RefreshAfterMutationAsync()
                If closeAfterSave Then
                    stpSaisie.Visibility = Visibility.Collapsed
                    btnNewData.Visibility = Visibility.Visible
                End If
            Catch ex As Exception
                MessageBox.Show("L'affectation a été enregistrée, mais l'actualisation de l'écran a échoué : " & ex.Message)
            End Try
        Finally
            btnSave.IsEnabled = True
            btnSaveContinue.IsEnabled = True
            IsSaving = False
        End Try
    End Function

    Private Sub BtnAgentListeSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentListeSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()

    End Sub

    Private Sub DgAgentAffectation_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgAgentAffectation.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgAgentAffectation.CurrentItem, DataRowView)
        LoadRowItems(rowView)

    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If CbxNom.Items.Contains(rowView.Row.ItemArray(0).ToString) Then
                CbxNom.SelectedItem = rowView.Row.ItemArray(0).ToString
                dtpDebutAffect.SelectedDate = rowView.Row.ItemArray(1).ToString
                dtpFinAffect.SelectedDate = rowView.Row.ItemArray(2).ToString
                CbxNouveauSite.SelectedItem = rowView.Row.ItemArray(3).ToString
                TbxAncienSite.Text = rowView.Row.ItemArray(4).ToString
                TbxObs.Text = rowView.Row.ItemArray(5).ToString
                RefAffect = rowView.Row.ItemArray(6).ToString
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


    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs) Handles cmiModif.Click
        Dim rowView As DataRowView = TryCast(DgAgentAffectation.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs) Handles cmiResume.Click
        Dim rowView As DataRowView = TryCast(DgAgentAffectation.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeAffect(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd)
        End If

    End Sub

    Private Async Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & CbxNom.SelectedItem & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            Await DeleteAffectationAsync(RefAffect)
        End If
    End Sub

    Private Async Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgAgentAffectation.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim affectationId As Integer = Convert.ToInt32(rowView.Row.ItemArray(6))
            If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & rowView.Row.ItemArray(0).ToString & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                Await DeleteAffectationAsync(affectationId)
            End If
        End If
    End Sub

    Private Async Function DeleteAffectationAsync(affectationId As Integer) As Task
        If IsSaving Then Return

        IsSaving = True
        btnSave.IsEnabled = False
        btnSaveContinue.IsEnabled = False
        btnDeleteAg.IsEnabled = False
        Try
            Await Task.Run(Sub() DeleteAffectationTransaction(affectationId))
            MessageBox.Show("Donnée supprimée avec succès.")
            Await RefreshAfterMutationAsync()
        Catch ex As Exception
            MessageBox.Show("Échec de la suppression de l'affectation : " & ex.Message)
        Finally
            btnSave.IsEnabled = True
            btnSaveContinue.IsEnabled = True
            btnDeleteAg.IsEnabled = True
            IsSaving = False
        End Try
    End Function

    Private Sub BtnAgentListeSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentListeSearch.KeyDown
        If e.Key = Key.Enter Then
            inputTimer.Stop()
            SearchAffectation(BtnAgentListeSearch.Text)
        End If
    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then SearchAffectation(BtnAgentListeSearch.Text)
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Affectation"
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Affectation du " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgAgentAffectation, "Point_Affectation", iTitre) '"Point_Entrees"
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        InialiseLoad()
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Await SaveAffectationAsync(True)
    End Sub

    Private Sub CbxNom_LostFocus(sender As Object, e As RoutedEventArgs) Handles CbxNom.LostFocus
        If CbxNumMatr.SelectedIndex >= 0 Then
            Dim iDate As Date = CDate(MyStatValStr("select c.DebutContr  from contratstbl c where c.NomPrenoms like '" & Apost(CbxNumMatr.SelectedItem) & "' and c.FinContr>=date()", LocalConnxStr))
            Dim iFin As Date = CDate(MyStatValStr("select c.FinContr  from contratstbl c where c.NomPrenoms like '" & Apost(CbxNumMatr.SelectedItem) & "' and c.FinContr>=date()", LocalConnxStr))
            dtpDebutAffect.DisplayDateStart = iDate
            If TbxAncienSite.Text = "(Nouveau)" Then
                dtpDebutAffect.SelectedDate = iDate
            Else
                dtpDebutAffect.SelectedDate = DebutDuMois(Today)
            End If
            'dtpFinAffect.DisplayDateEnd = iFin
            dtpFinAffect.SelectedDate = iFin
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
        Dim iDetailW As New wdwDetails("Details des AFFECTATIONS", "A ce jour " & Today, Me)
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
        Dim istr As String = "SELECT s.Client, aff.AgentAffect, date(aff.DateAffect), aff.SiteAffectPrecedent, aff.SiteAffect, aff.ObsAffect   "
        istr &= " FROM affectation aff JOIN SiteAffectation s ON s.NomSite = aff.SiteAffect "
        istr &= "  WHERE aff.DateAffect >='" & debut.ToString("yyyy-MM-dd") & "' and aff.DateAffect <='" & Fin.ToString("yyyy-MM-dd") & "'"   ' date('now','-1 month','start of month','+24 day')  "
        istr &= " and aff.FinAffect >='" & DebutDuMois(Fin).ToString("yyyy-MM-dd") & "'"  'date('now','-1 month','start of month','+24 day')"
        istr &= " and not aff.SiteAffect=aff.SiteAffectPrecedent"
        'istr &= " GROUP BY s.Client, a.NomPrenoms  "
        istr &= " ORDER BY s.Client, aff.AgentAffect, aff.SiteAffect"
        'ShowTempMsg(istr)
        openPointMensuel(debut, Fin, istr, "AFFECTATIONS du " & debut.ToString("dd/MM/yyyy") & " au " & Fin.ToString("dd/MM/yyyy"), "AFFECTATIONS")

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
