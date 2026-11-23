Imports System.Data
Imports System.Data.SQLite
Imports System.Threading.Tasks

Public Class usercJournalOperations
    Private _isLoading As Boolean = False

    Private ReadOnly Property CanViewAllJournals As Boolean
        Get
            Return CanUserManage OrElse CanGlobalDataManage
        End Get
    End Property

    Private Async Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Try
            ConfigureAccessUi()
            ' Chargement léger par défaut : opérations du jour uniquement
            DtpFrom.SelectedDate = Date.Today
            DtpTo.SelectedDate = Date.Today
            Await LoadJournalAsync()
        Catch ex As Exception
            MessageBox.Show("Impossible de charger le journal des opérations : " & ex.Message, "Journal des opérations", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub ConfigureAccessUi()
        If CanViewAllJournals Then
            SpOperateur.Visibility = Visibility.Visible
            TbkSubtitle.Text = "Consultation de l'ensemble des journaux (administration)."
            TbkScope.Text = "Vue : tous les utilisateurs"
        Else
            SpOperateur.Visibility = Visibility.Collapsed
            TbkSubtitle.Text = "Consultation de votre journal des opérations."
            TbkScope.Text = "Vue : " & If(String.IsNullOrWhiteSpace(UserName), "utilisateur courant", UserName)
        End If
    End Sub

    Private Async Sub BtnRefresh_Click(sender As Object, e As RoutedEventArgs)
        Await LoadJournalAsync()
    End Sub

    Private Async Sub BtnClear_Click(sender As Object, e As RoutedEventArgs)
        ' Réinitialiser sur la plage légère par défaut (aujourd'hui)
        DtpFrom.SelectedDate = Date.Today
        DtpTo.SelectedDate = Date.Today
        If CbxAction.Items.Count > 0 Then CbxAction.SelectedIndex = 0
        TbxOperateur.Text = String.Empty
        TbxSearch.Text = String.Empty
        Await LoadJournalAsync()
    End Sub

    Private Async Function LoadJournalAsync() As Task
        If _isLoading Then Return
        _isLoading = True
        SetLoadingState(True)
        Await Task.Delay(1)

        Try
            Dim filters As JournalFilter = CaptureFilters()

            Dim table As DataTable = Await Task.Run(Function()
                                                        Return QueryJournal(filters)
                                                    End Function)

            DgvJournal.ItemsSource = table.DefaultView
            TbkCount.Text = table.Rows.Count.ToString() & " opération(s)"
            If filters.FromDate.HasValue AndAlso filters.ToDate.HasValue AndAlso filters.FromDate.Value.Date = Date.Today AndAlso filters.ToDate.Value.Date = Date.Today Then
                TbkCount.Text &= " (aujourd'hui)"
            End If
        Catch ex As Exception
            MessageBox.Show("Erreur lors du chargement du journal : " & ex.Message, "Journal des opérations", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            SetLoadingState(False)
            _isLoading = False
        End Try
    End Function

    Private Function CaptureFilters() As JournalFilter
        Dim f As New JournalFilter()
        f.FromDate = DtpFrom.SelectedDate
        f.ToDate = DtpTo.SelectedDate
        f.ActionType = GetSelectedAction()
        f.Search = If(TbxSearch.Text, String.Empty).Trim()
        f.CanViewAll = CanViewAllJournals
        f.CurrentUser = If(UserName, String.Empty)
        f.OperateurFilter = If(CanViewAllJournals, If(TbxOperateur.Text, String.Empty).Trim(), String.Empty)
        Return f
    End Function

    Private Function QueryJournal(filters As JournalFilter) As DataTable
        Dim connStr As String = GetGeneralDatabaseConnectionString()
        EnsureJopeSchema(connStr)

        ' Action effective : utilise ActionType si renseigné, sinon déduit depuis Operation (compatibilité anciennes entrées)
        Dim effectiveActionExpr As String =
            "CASE " &
            "WHEN TRIM(COALESCE(ActionType,'')) <> '' THEN UPPER(TRIM(ActionType)) " &
            "WHEN UPPER(COALESCE(Operation,'')) LIKE 'IMPORT%' OR UPPER(COALESCE(Operation,'')) LIKE '%IMPORT %' OR UPPER(COALESCE(Operation,'')) LIKE '% IMPORT%' THEN 'IMPORT' " &
            "WHEN UPPER(COALESCE(Operation,'')) LIKE 'EXPORT%' OR UPPER(COALESCE(Operation,'')) LIKE '%EXPORT %' OR UPPER(COALESCE(Operation,'')) LIKE '% EXPORT%' THEN 'EXPORT' " &
            "WHEN UPPER(COALESCE(Operation,'')) LIKE 'DELETE%' OR UPPER(COALESCE(Operation,'')) LIKE '%DELETE%' OR UPPER(COALESCE(Operation,'')) LIKE 'SUPPR%' OR UPPER(COALESCE(Operation,'')) LIKE '%SUPPR%' THEN 'DELETE' " &
            "WHEN UPPER(COALESCE(Operation,'')) LIKE 'SAVENEW%' OR UPPER(COALESCE(Operation,'')) LIKE '%SAVENEW%' OR UPPER(COALESCE(Operation,'')) LIKE 'INSERT%' OR UPPER(COALESCE(Operation,'')) LIKE '%AJOUT%' OR UPPER(COALESCE(Operation,'')) LIKE '%CRÉAT%' OR UPPER(COALESCE(Operation,'')) LIKE '%CREAT%' THEN 'CREATE' " &
            "WHEN UPPER(COALESCE(Operation,'')) LIKE 'SAVEMODIF%' OR UPPER(COALESCE(Operation,'')) LIKE '%SAVEMODIF%' OR UPPER(COALESCE(Operation,'')) LIKE '%MODIF%' OR UPPER(COALESCE(Operation,'')) LIKE 'UPDATE%' OR UPPER(COALESCE(Operation,'')) LIKE '%UPDATE%' OR UPPER(COALESCE(Operation,'')) LIKE '%MISE À JOUR%' OR UPPER(COALESCE(Operation,'')) LIKE '%MISE A JOUR%' THEN 'UPDATE' " &
            "ELSE '' END"

        Dim sql As String =
            "SELECT DateOperation, Operateur, " &
            "COALESCE(Operation,'') AS Operation, " &
            effectiveActionExpr & " AS ActionType, " &
            "COALESCE(Entite,'') AS Entite, " &
            "COALESCE(Cible,'') AS Cible, " &
            "COALESCE(Details,'') AS Details " &
            "FROM JOPE WHERE 1=1"

        Using connection As New SQLiteConnection(connStr)
            connection.Open()
            Using command As New SQLiteCommand()
                command.Connection = connection

                If filters.FromDate.HasValue Then
                    sql &= " AND date(DateOperation) >= date(@from)"
                    command.Parameters.AddWithValue("@from", filters.FromDate.Value.ToString("yyyy-MM-dd"))
                End If
                If filters.ToDate.HasValue Then
                    sql &= " AND date(DateOperation) <= date(@to)"
                    command.Parameters.AddWithValue("@to", filters.ToDate.Value.ToString("yyyy-MM-dd"))
                End If

                If Not String.IsNullOrWhiteSpace(filters.ActionType) Then
                    sql &= " AND (" & effectiveActionExpr & ") = @action"
                    command.Parameters.AddWithValue("@action", filters.ActionType.ToUpperInvariant())
                End If

                If filters.CanViewAll Then
                    If Not String.IsNullOrWhiteSpace(filters.OperateurFilter) Then
                        sql &= " AND Operateur LIKE @operateur"
                        command.Parameters.AddWithValue("@operateur", "%" & filters.OperateurFilter & "%")
                    End If
                Else
                    sql &= " AND Operateur = @operateur"
                    command.Parameters.AddWithValue("@operateur", filters.CurrentUser)
                End If

                If Not String.IsNullOrWhiteSpace(filters.Search) Then
                    sql &= " AND (Operation LIKE @search OR COALESCE(Entite,'') LIKE @search OR COALESCE(Cible,'') LIKE @search OR COALESCE(Details,'') LIKE @search OR (" & effectiveActionExpr & ") LIKE @search OR Operateur LIKE @search)"
                    command.Parameters.AddWithValue("@search", "%" & filters.Search & "%")
                End If

                sql &= " ORDER BY datetime(DateOperation) DESC, OrdrOperation DESC LIMIT 2000"
                command.CommandText = sql

                Dim table As New DataTable()
                Using adapter As New SQLiteDataAdapter(command)
                    adapter.Fill(table)
                End Using
                EnrichJournalRows(table)
                Return table
            End Using
        End Using
    End Function

    Private Sub SetLoadingState(isLoading As Boolean)
        If LoadingOverlay IsNot Nothing Then
            LoadingOverlay.Visibility = If(isLoading, Visibility.Visible, Visibility.Collapsed)
        End If
        BtnRefresh.IsEnabled = Not isLoading
        BtnClear.IsEnabled = Not isLoading
        DtpFrom.IsEnabled = Not isLoading
        DtpTo.IsEnabled = Not isLoading
        CbxAction.IsEnabled = Not isLoading
        TbxSearch.IsEnabled = Not isLoading
        TbxOperateur.IsEnabled = Not isLoading
        If isLoading Then
            TbkCount.Text = "Chargement..."
        End If
    End Sub

    Private Function GetSelectedAction() As String
        If CbxAction Is Nothing OrElse CbxAction.SelectedItem Is Nothing Then Return String.Empty
        Dim item = TryCast(CbxAction.SelectedItem, ComboBoxItem)
        If item Is Nothing Then Return String.Empty
        Dim content As String = Convert.ToString(item.Content)
        If String.IsNullOrWhiteSpace(content) OrElse content.Equals("Toutes", StringComparison.OrdinalIgnoreCase) Then
            Return String.Empty
        End If
        Return content
    End Function

    Private Sub EnrichJournalRows(table As DataTable)
        If table Is Nothing Then Return
        For Each row As DataRow In table.Rows
            Dim operation As String = Convert.ToString(row("Operation"))
            Dim action As String = Convert.ToString(row("ActionType"))
            Dim entite As String = Convert.ToString(row("Entite"))
            Dim cible As String = Convert.ToString(row("Cible"))
            Dim details As String = Convert.ToString(row("Details"))

            If String.IsNullOrWhiteSpace(entite) Then
                entite = InferEntity(operation, action)
            End If
            If String.IsNullOrWhiteSpace(cible) Then
                cible = InferTarget(operation)
            End If
            If String.IsNullOrWhiteSpace(details) Then
                details = InferDetails(operation)
            End If

            row("ActionType") = If(action, String.Empty).Trim()
            row("Entite") = If(entite, String.Empty).Trim()
            row("Cible") = If(cible, String.Empty).Trim()
            row("Details") = If(details, String.Empty).Trim()
        Next
        If table.Rows.Count > 0 Then table.AcceptChanges()
    End Sub

    Private Function InferEntity(operation As String, action As String) As String
        If String.IsNullOrWhiteSpace(operation) Then Return String.Empty
        Dim op As String = operation.Trim()

        ' Ex: "Import CSV absences : ..." / "Import CSV clients/contrats : ..."
        Dim importPrefix As String = "Import CSV "
        If op.StartsWith(importPrefix, StringComparison.OrdinalIgnoreCase) Then
            Dim rest As String = op.Substring(importPrefix.Length).Trim()
            Dim sepIdx As Integer = rest.IndexOf(" :")
            If sepIdx < 0 Then sepIdx = rest.IndexOf(":"c)
            If sepIdx > 0 Then Return rest.Substring(0, sepIdx).Trim()
            Return rest
        End If

        If op.StartsWith("ImportCsv", StringComparison.OrdinalIgnoreCase) Then
            Dim rest As String = op.Substring("ImportCsv".Length)
            Dim forIdx As Integer = rest.IndexOf(" for ", StringComparison.OrdinalIgnoreCase)
            If forIdx > 0 Then rest = rest.Substring(0, forIdx)
            Dim parIdx As Integer = rest.IndexOf("("c)
            If parIdx > 0 Then rest = rest.Substring(0, parIdx)
            rest = rest.Trim(" "c, ":"c, "-"c)
            If rest <> String.Empty Then Return rest
        End If

        If op.StartsWith("Export Excel", StringComparison.OrdinalIgnoreCase) Then
            Return "Export"
        End If

        Return String.Empty
    End Function

    Private Function InferTarget(operation As String) As String
        If String.IsNullOrWhiteSpace(operation) Then Return String.Empty
        Dim op As String = operation.Trim()

        Dim depuisIdx As Integer = op.IndexOf(" depuis ", StringComparison.OrdinalIgnoreCase)
        If depuisIdx >= 0 AndAlso depuisIdx + 8 < op.Length Then
            Return op.Substring(depuisIdx + 8).Trim()
        End If

        Dim atIdx As Integer = op.LastIndexOf(" at ", StringComparison.OrdinalIgnoreCase)
        If atIdx >= 0 AndAlso atIdx + 4 < op.Length Then
            Return op.Substring(atIdx + 4).Trim()
        End If

        Dim forIdx As Integer = op.IndexOf(" for ", StringComparison.OrdinalIgnoreCase)
        If forIdx >= 0 AndAlso forIdx + 5 < op.Length Then
            Dim target As String = op.Substring(forIdx + 5).Trim()
            Dim atInTarget As Integer = target.LastIndexOf(" at ", StringComparison.OrdinalIgnoreCase)
            If atInTarget > 0 Then target = target.Substring(0, atInTarget).Trim()
            Return target
        End If

        Return String.Empty
    End Function

    Private Function InferDetails(operation As String) As String
        If String.IsNullOrWhiteSpace(operation) Then Return String.Empty
        Dim op As String = operation.Trim()
        Dim lower As String = op.ToLowerInvariant()

        If lower.Contains(" depuis ") Then
            Dim idx As Integer = op.IndexOf(" depuis ", StringComparison.OrdinalIgnoreCase)
            If idx >= 0 Then Return "Source: " & op.Substring(idx + 8).Trim()
        End If
        If lower.Contains(" ligne") OrElse lower.Contains("entrée") OrElse lower.Contains("entree") Then
            ' Garder une indication utile issue du libellé d'opération
            Dim colonIdx As Integer = op.IndexOf(":"c)
            If colonIdx >= 0 AndAlso colonIdx + 1 < op.Length Then
                Return op.Substring(colonIdx + 1).Trim()
            End If
        End If
        Return String.Empty
    End Function

    Private Class JournalFilter
        Public Property FromDate As Date?
        Public Property ToDate As Date?
        Public Property ActionType As String
        Public Property Search As String
        Public Property CanViewAll As Boolean
        Public Property CurrentUser As String
        Public Property OperateurFilter As String
    End Class
End Class