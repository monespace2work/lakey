Imports System.Windows.Threading
Imports System.Data.SQLite
Imports System.Data
Imports System.Threading.Tasks
Imports System.Globalization

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

    Private Async Sub btnImportCsv_Click(sender As Object, e As RoutedEventArgs) Handles btnImportCsv.Click
        Dim dialog As New Microsoft.Win32.OpenFileDialog With {
            .Title = "Importer des affectations depuis un fichier CSV",
            .Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
            .CheckFileExists = True,
            .Multiselect = False
        }
        If dialog.ShowDialog() <> True Then Return

        Try
            Dim table As DataTable = Nothing
            Dim hasHeader As Boolean = False
            Dim noHeaderColumns As String() = {"DateAffect", "FinAffect", "AgentAffect", "SiteAffect", "SiteAffectPrecedent", "ObsAffect"}
            Try
                table = ReadCsvTable(dialog.FileName)
                hasHeader =
                    CsvHasColumn(table, "Agent", "AgentAffect", "NomPrenoms", "Matricule") AndAlso
                    CsvHasColumn(table, "Nouveau site", "Site", "SiteAffect") AndAlso
                    CsvHasColumn(table, "Début", "DateAffect") AndAlso
                    CsvHasColumn(table, "Fin", "FinAffect")
            Catch ex As FormatException
                ' Un CSV sans en-tête peut être interprété comme des en-têtes invalides.
            End Try
            If Not hasHeader Then
                table = ReadCsvTable(dialog.FileName, noHeaderColumns, ";")
            End If
            If Not CsvHasColumn(table, "Agent", "AgentAffect", "NomPrenoms", "Matricule") OrElse
               Not CsvHasColumn(table, "Nouveau site", "Site", "SiteAffect") OrElse
               Not CsvHasColumn(table, "Début", "DateAffect") OrElse
               Not CsvHasColumn(table, "Fin", "FinAffect") Then
                Throw New FormatException("Colonnes obligatoires : Agent, Nouveau site, Début et Fin.")
            End If
            If table.Rows.Count = 0 Then
                MessageBox.Show("Le fichier CSV ne contient aucune donnée à importer.", "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If
            If MessageBox.Show(
                "Importer " & table.Rows.Count & " affectation(s) ? Toute l'opération sera annulée si une ligne est invalide, en conflit ou en doublon.",
                "Confirmation de l'import CSV",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) <> MessageBoxResult.Yes Then Return

            elpStatus.Visibility = Visibility.Visible
            Await Task.Run(Sub() ImportAffectationsCsv(table, hasHeader))
            Await RefreshAfterMutationAsync()
            MessageBox.Show(table.Rows.Count & " affectation(s) importée(s) avec succès.", "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            MessageBox.Show("L'import CSV des affectations a échoué : " & ex.Message, "Import CSV", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            elpStatus.Visibility = Visibility.Collapsed
        End Try
    End Sub

    Private Sub ImportAffectationsCsv(table As DataTable, hasHeader As Boolean)
        Dim writeConnectionString As String = ResolveMainDatabaseConnectionString(iConnxStr, True)
        Using connection As New SQLiteConnection(writeConnectionString)
            connection.Open()
            Using transaction As SQLiteTransaction = connection.BeginTransaction()
                Dim nextId As Integer
                Using command As New SQLiteCommand("SELECT COALESCE(MAX(IdAffec), 0) + 1 FROM Affectation", connection, transaction)
                    nextId = Convert.ToInt32(command.ExecuteScalar())
                End Using

                Dim activeAgents As Dictionary(Of String, String) =
                    LoadCsvLookup(connection, transaction, "SELECT NomPrenoms, NumMatr FROM agents WHERE Actif = 1", "NomPrenoms", "NomPrenoms", "NumMatr")
                Dim activeAgentMatricules As Dictionary(Of String, String) =
                    LoadCsvLookup(connection, transaction, "SELECT NomPrenoms, NumMatr FROM agents WHERE Actif = 1", "NumMatr", "NomPrenoms", "NumMatr")
                Dim activeSites As Dictionary(Of String, String) =
                    LoadCsvLookup(connection, transaction, "SELECT NomSite FROM siteaffectation WHERE Actif = 1", "NomSite", "NomSite")
                Dim seenEntries As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                For index As Integer = 0 To table.Rows.Count - 1
                    Dim row As DataRow = table.Rows(index)
                    Dim rowNumber As Integer = index + If(hasHeader, 2, 1)
                    Dim agentInput As String = CsvValue(row, "Agent", "AgentAffect", "NomPrenoms", "Matricule")
                    Dim newSite As String = CsvValue(row, "Nouveau site", "Site", "SiteAffect")
                    Dim startDate As DateTime = CsvDateValue(CsvValue(row, "Début", "DateAffect"), rowNumber, "Début")
                    Dim endDate As DateTime = CsvDateValue(CsvValue(row, "Fin", "FinAffect"), rowNumber, "Fin")
                    If String.IsNullOrWhiteSpace(agentInput) OrElse String.IsNullOrWhiteSpace(newSite) Then
                        Throw New FormatException("Ligne " & rowNumber & " : l'agent et le nouveau site sont obligatoires.")
                    End If
                    If endDate <= startDate Then Throw New FormatException("Ligne " & rowNumber & " : la fin doit être postérieure au début.")

                    Dim agentName As String = Nothing
                    Dim agentMatricule As String = Nothing
                    If Not activeAgents.TryGetValue(agentInput, agentName) OrElse
                       Not activeAgentMatricules.TryGetValue(agentInput, agentMatricule) Then
                        Throw New InvalidOperationException("Ligne " & rowNumber & " : agent actif ayant un contrat valide introuvable (« " & agentInput & " »).")
                    End If

                    Using command As New SQLiteCommand(
                        "SELECT COUNT(*) FROM contratstbl WHERE NomPrenoms = @agent AND FinContr >= @start",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@agent", agentMatricule)
                        command.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd"))
                        If Convert.ToInt32(command.ExecuteScalar()) = 0 Then
                            Throw New InvalidOperationException("Ligne " & rowNumber & " : agent actif ayant un contrat valide introuvable (« " & agentInput & " »).")
                        End If
                    End Using

                    Dim canonicalSite As String = Nothing
                    If Not activeSites.TryGetValue(newSite, canonicalSite) Then
                        Throw New InvalidOperationException("Ligne " & rowNumber & " : nouveau site inactif ou inconnu (« " & newSite & " »).")
                    End If
                    newSite = canonicalSite

                    Dim duplicateKey As String = String.Join("|", agentName, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), newSite)
                    If Not seenEntries.Add(duplicateKey) Then
                        Throw New InvalidOperationException("Ligne " & rowNumber & " : affectation répétée dans le fichier CSV.")
                    End If

                    Dim previousId As Integer = 0
                    Dim previousStart As DateTime = Date.MinValue
                    Dim previousEnd As DateTime = Date.MinValue
                    Dim previousSite As String = ""
                    Using command As New SQLiteCommand(
                        "SELECT IdAffec, DateAffect, FinAffect, SiteAffect FROM Affectation " &
                        "WHERE AgentAffect = @agent AND DateAffect <= @end AND FinAffect >= @start " &
                        "ORDER BY DateAffect DESC, IdAffec DESC",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@agent", agentName)
                        command.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@end", endDate.ToString("yyyy-MM-dd"))
                        Using reader As SQLiteDataReader = command.ExecuteReader()
                            If reader.Read() Then
                                previousId = Convert.ToInt32(reader("IdAffec"))
                                previousStart = DateTime.Parse(Convert.ToString(reader("DateAffect"), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
                                previousEnd = DateTime.Parse(Convert.ToString(reader("FinAffect"), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
                                previousSite = Convert.ToString(reader("SiteAffect"))
                                If reader.Read() Then
                                    Throw New InvalidOperationException("Ligne " & rowNumber & " : plusieurs affectations existantes chevauchent la période de « " & agentName & " ».")
                                End If
                            End If
                        End Using
                    End Using

                    If previousId > 0 AndAlso previousStart >= startDate Then
                        Throw New InvalidOperationException("Ligne " & rowNumber & " : une affectation existante de « " & agentName & " » commence pendant ou après la date de début.")
                    End If

                    If previousId = 0 Then
                        Using command As New SQLiteCommand(
                            "SELECT SiteAffect FROM Affectation WHERE AgentAffect = @agent AND DateAffect < @start ORDER BY DateAffect DESC, IdAffec DESC LIMIT 1",
                            connection,
                            transaction)
                            command.Parameters.AddWithValue("@agent", agentName)
                            command.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd"))
                            previousSite = Convert.ToString(command.ExecuteScalar())
                        End Using
                    End If
                    If String.IsNullOrWhiteSpace(previousSite) Then previousSite = "(Nouveau)"
                    Dim suppliedPreviousSite As String = CsvValue(row, "Ancien site", "SiteAffectPrecedent")
                    If Not String.IsNullOrWhiteSpace(suppliedPreviousSite) Then previousSite = suppliedPreviousSite

                    If previousId > 0 AndAlso previousEnd >= startDate Then
                        Using command As New SQLiteCommand("UPDATE Affectation SET FinAffect = @end WHERE IdAffec = @id", connection, transaction)
                            command.Parameters.AddWithValue("@end", startDate.AddDays(-1).ToString("yyyy-MM-dd"))
                            command.Parameters.AddWithValue("@id", previousId)
                            command.ExecuteNonQuery()
                        End Using
                    End If

                    Using command As New SQLiteCommand(
                        "INSERT INTO Affectation (IdAffec, AgentAffect, DateAffect, FinAffect, SiteAffectPrecedent, SiteAffect, ObsAffect) " &
                        "VALUES (@id, @agent, @start, @end, @previousSite, @newSite, @observations)",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@id", nextId)
                        command.Parameters.AddWithValue("@agent", agentName)
                        command.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@end", endDate.ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@previousSite", previousSite)
                        command.Parameters.AddWithValue("@newSite", newSite)
                        command.Parameters.AddWithValue("@observations", CsvValue(row, "Observations", "ObsAffect", "Obs"))
                        command.ExecuteNonQuery()
                    End Using
                    Using command As New SQLiteCommand("INSERT INTO jope (operation, operateur) VALUES (@operation, @operator)", connection, transaction)
                        command.Parameters.AddWithValue("@operation", "ImportCsvAffectation for " & agentName)
                        command.Parameters.AddWithValue("@operator", UserName)
                        command.ExecuteNonQuery()
                    End Using
                    nextId += 1
                Next

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
