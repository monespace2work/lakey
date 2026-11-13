Imports ClosedXML.Excel
Imports System.Windows.Threading
Imports System.IO
Imports System.Data
Imports System.Threading.Tasks
Imports System.Globalization
Imports System.Text
Imports Microsoft.VisualBasic.FileIO
Module ObjectsManaging
    Public DocsDir As String = My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\LkyDocs"
    Dim iTimer As New DispatcherTimer
    Public iDureeTempMsg As Integer = 10
    Dim iTempMsg As New TempMsg
    Private generalDatabaseWatcher As FileSystemWatcher
    Private ReadOnly generalDatabaseSyncLock As New Object
    Private ReadOnly generalDatabaseSyncTimer As New DispatcherTimer With {
        .Interval = TimeSpan.FromMilliseconds(500)
    }
    Private ReadOnly generalDatabasePollTimer As New DispatcherTimer With {
        .Interval = TimeSpan.FromSeconds(30)
    }
    Private lastSyncedLocalPath As String = ""
    Private lastSyncedFingerprint As String = ""
    Private generalDatabaseSyncWarningShown As Boolean
    Private Const FinContratAffectationRowColorHandlerKey As String = "FinContratAffectationRowColorHandler"

    Public Async Function RefreshAfterDatabaseMutationAsync(refreshAction As Action) As Task
        Dim mainWindow As MainWindow = TryCast(Application.Current?.MainWindow, MainWindow)
        mainWindow?.BeginDatabaseLoading()
        Try
            Try
                Dim synchronized As Boolean = Await Task.Run(
                    Function()
                        Return LocalModeBb OrElse maj()
                    End Function)
                If Not synchronized Then
                    MessageBox.Show(
                        "L'opération est enregistrée, mais la copie locale n'a pas pu être actualisée. Les données affichées peuvent être anciennes.",
                        "Actualisation en attente",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning)
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Échec de l'actualisation après une modification : " & ex.ToString())
                MessageBox.Show(
                    "L'opération est enregistrée, mais l'actualisation de la base locale a échoué : " & ex.Message,
                    "Actualisation impossible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning)
            End Try

            Try
                refreshAction()
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Échec du rechargement de l'interface après une modification : " & ex.ToString())
                MessageBox.Show(
                    "L'opération est enregistrée, mais l'actualisation de l'écran a échoué : " & ex.Message,
                    "Actualisation impossible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning)
            End Try
        Finally
            mainWindow?.EndDatabaseLoading()
        End Try
    End Function

    Public Sub SaisiTbx(iTbx As TextBox, iTbl As TextBlock)
        Try
            If iTbx.Text <> String.Empty Then
                If Not IsNothing(iTbx) Then
                    iTbl.Visibility = Visibility.Hidden
                End If
            Else
                If Not IsNothing(iTbx) Then
                    iTbl.Visibility = Visibility.Visible
                End If
            End If
        Catch ex As Exception

        End Try


    End Sub
    Public Sub SaisiCbx(iCbx As ComboBox, iTbl As TextBlock)
        Try
            If iCbx.SelectedIndex <> -1 Then
                If Not IsNothing(iCbx) Then
                    iTbl.Visibility = Visibility.Hidden
                End If
            Else
                If Not IsNothing(iCbx) Then
                    iTbl.Visibility = Visibility.Visible
                End If
            End If
        Catch ex As Exception

        End Try


    End Sub
    Public Sub ExportDGtoExcel(iDataGrid As DataGrid, filename As String, Optional Titre As String = "Données exportées")
        Dim rowsToExport As New List(Of Object)
        Dim FicherNom As String
        If Not System.IO.Directory.Exists(DocsDir) Then
            Try
                System.IO.Directory.CreateDirectory(DocsDir)
            Catch ex As Exception
                MsgBox(ex.Message & Chr(13) & "L'exportation vers EXCEL va s'arrêter ! ")
                Exit Sub
            End Try
        End If
        If iDataGrid.SelectedItems.Count = 0 Then
            FicherNom = DocsDir & "\" & filename & "_" & Now.ToString("yyyy-MM-dd_HHmmfff") & ".xlsx"
        Else
            FicherNom = DocsDir & "\" & filename & "_Partiel_" & Now.ToString("yyyy-MM-dd_HHmmfff") & ".xlsx"
        End If
        Try
            Dim originalDataView As DataView = TryCast(iDataGrid.ItemsSource, DataView)
            If originalDataView Is Nothing Then Exit Sub
            Dim exportTable As New DataTable

            If iDataGrid.SelectedItems.Count > 0 Then
                exportTable = originalDataView.Table.Clone
                For Each elt In iDataGrid.SelectedItems
                    Dim iRowDataRowView As DataRowView = TryCast(elt, DataRowView)
                    If iRowDataRowView IsNot Nothing Then
                        exportTable.ImportRow(iRowDataRowView.Row)
                    End If
                Next
            Else
                exportTable = originalDataView.ToTable
            End If
            If exportTable.Rows.Count = 0 Then
                MessageBox.Show("Aucune donnée à exporter.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If
            Using wb As New XLWorkbook()
                Dim iFeuille = wb.Worksheets.Add("Feuil1")
                Dim colCount As Integer = exportTable.Columns.Count
                iFeuille.Cell(1, 1).Value = Titre
                iFeuille.Range(1, 1, 1, colCount).Merge()
                With iFeuille.Range(1, 1, 1, colCount).Style
                    .Font.FontSize = 14
                    .Font.Bold = True
                    .Alignment.Horizontal = XLAlignmentHorizontalValues.Center
                End With
                iFeuille.Cell(3, 1).InsertTable(exportTable)
                iFeuille.Columns().AdjustToContents()
                wb.SaveAs(FicherNom)
            End Using

            MessageBox.Show($"Exporté avec succès : {FicherNom}", "Export", MessageBoxButton.OK, MessageBoxImage.Information)
            Process.Start(New ProcessStartInfo(FicherNom) With {.UseShellExecute = True})


        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        '=======================================

    End Sub
    Public Sub ShowTempMsg(MsgText As String, Optional Duree As Integer = 10, Optional iTitle As String = "IMPORTANT!!!", Optional iColorCode As Char = "W")
        Dim iTempMsg As New TempMsg
        Try
            iDureeTempMsg = Duree
            Select Case iColorCode
                Case "W", "w"
                    iTempMsg.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFFFFF"))
                Case "R", "r"
                    iTempMsg.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFE2E2"))
                Case "B", "b"
                    iTempMsg.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFE2F4FF"))
                Case "G", "g"
                    iTempMsg.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFE2FFE4"))
                Case Else
                    iTempMsg.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFF8B1"))
            End Select
            iTempMsg.Title = iTitle
            iTempMsg.tbxMsg.Text = MsgText
            iTempMsg.tbxMsg.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FF263238"))
            iTempMsg.tbxMsg.FontFamily = New FontFamily("Segoe UI")
            iTempMsg.tbxMsg.FontSize = 13
            iTempMsg.tbxMsg.Padding = New Thickness(10)
            iTempMsg.Show()
            iTimer.Start()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Public Function ColorGbColor(iRowView As DataGridRow) As DataGridRow
        Dim iRow As New DataGridRow
        'Dim MiRow As DataGridRow = typecast()
        If iRowView IsNot Nothing Then
            iRow.Background = Brushes.Red
        End If
        ColorGbColor = iRow
    End Function
    Private Sub SetThemeDataGridRow(row As DataGridRow, backgroundResource As String, foregroundResource As String)
        row.SetResourceReference(DataGridRow.BackgroundProperty, backgroundResource)
        row.SetResourceReference(DataGridRow.ForegroundProperty, foregroundResource)
    End Sub

    Public Sub ColorDgRowsAge(iDataGrid As DataGrid, Optional iRowOfAge As Integer = 6, Optional AgeLimit1 As Integer = 30, Optional AgeLimit2 As Integer = 45)
        Dim iTable As DataTable = TryCast(iDataGrid.ItemsSource, DataView).ToTable
        If IsNothing(iTable) Then
            Exit Sub
        Else
        End If
        For eTdx As Integer = 0 To iTable.Rows.Count - 1
            Try
                Dim iAge As Integer = CInt(iTable.Rows(eTdx).Item(iRowOfAge))
                If iAge < AgeLimit1 Then
                    Dim targetItem = iDataGrid.Items(eTdx)
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)

                    If targetRow IsNot Nothing Then
                        SetThemeDataGridRow(targetRow, "ThemeTablePositiveRowBrush", "ThemeTablePositiveRowTextBrush")
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            SetThemeDataGridRow(targetRow, "ThemeTablePositiveRowBrush", "ThemeTablePositiveRowTextBrush")
                        End If
                    End If
                ElseIf iAge < AgeLimit2 Then
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        SetThemeDataGridRow(targetRow, "ThemeTableWarningRowBrush", "ThemeTableWarningRowTextBrush")
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            SetThemeDataGridRow(targetRow, "ThemeTableWarningRowBrush", "ThemeTableWarningRowTextBrush")
                        End If
                    End If
                Else
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        SetThemeDataGridRow(targetRow, "ThemeTableDangerRowBrush", "ThemeTableDangerRowTextBrush")
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            SetThemeDataGridRow(targetRow, "ThemeTableDangerRowBrush", "ThemeTableDangerRowTextBrush")
                        End If
                    End If
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        Next

    End Sub
    Public Sub ColorDgRowsContrat(iDataGrid As DataGrid, Optional iRowOfType As Integer = 6, Optional ContratKey1 As String = "cdd", Optional ContratKey2 As String = "stg")
        Dim iTable As DataTable = TryCast(iDataGrid.ItemsSource, DataView).ToTable
        If IsNothing(iTable) Then
            Exit Sub
        Else
        End If
        For eTdx As Integer = 0 To iTable.Rows.Count - 1
            Try
                Dim iContrat As String = LCase((iTable.Rows(eTdx).Item(iRowOfType)))
                If iContrat.Contains(LCase(ContratKey1)) Then
                    Dim targetItem = iDataGrid.Items(eTdx)
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)

                    If targetRow IsNot Nothing Then
                        SetThemeDataGridRow(targetRow, "ThemeTablePositiveRowBrush", "ThemeTablePositiveRowTextBrush")
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            SetThemeDataGridRow(targetRow, "ThemeTablePositiveRowBrush", "ThemeTablePositiveRowTextBrush")
                        End If
                    End If
                ElseIf iContrat.Contains(LCase(ContratKey2)) Then
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        SetThemeDataGridRow(targetRow, "ThemeTableWarningRowBrush", "ThemeTableWarningRowTextBrush")
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            SetThemeDataGridRow(targetRow, "ThemeTableWarningRowBrush", "ThemeTableWarningRowTextBrush")
                        End If
                    End If
                Else
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        SetThemeDataGridRow(targetRow, "ThemeTableDangerRowBrush", "ThemeTableDangerRowTextBrush")
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            SetThemeDataGridRow(targetRow, "ThemeTableDangerRowBrush", "ThemeTableDangerRowTextBrush")
                        End If
                    End If
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        Next

    End Sub
    Public Sub ColorDgRowsFinContratAffectation(iDataGrid As DataGrid, Optional iRowOfType As Integer = 6, Optional ListAgentFin As String = "fin", Optional ListAgentAffectation As String = "aff")
        If iDataGrid Is Nothing OrElse Not TypeOf iDataGrid.ItemsSource Is DataView Then Return

        If iDataGrid.Resources.Contains(FinContratAffectationRowColorHandlerKey) Then
            Dim previousHandler = TryCast(iDataGrid.Resources(FinContratAffectationRowColorHandlerKey), EventHandler(Of DataGridRowEventArgs))
            If previousHandler IsNot Nothing Then RemoveHandler iDataGrid.LoadingRow, previousHandler
        End If

        Dim rowColorHandler As EventHandler(Of DataGridRowEventArgs) =
            Sub(sender, e)
                ApplyFinContratAffectationRowColor(e.Row, iRowOfType, ListAgentFin, ListAgentAffectation)
            End Sub
        iDataGrid.Resources(FinContratAffectationRowColorHandlerKey) = rowColorHandler
        AddHandler iDataGrid.LoadingRow, rowColorHandler

        For rowIndex As Integer = 0 To iDataGrid.Items.Count - 1
            Dim row As DataGridRow = TryCast(iDataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex), DataGridRow)
            If row IsNot Nothing Then
                ApplyFinContratAffectationRowColor(row, iRowOfType, ListAgentFin, ListAgentAffectation)
            End If
        Next
    End Sub

    Private Sub ApplyFinContratAffectationRowColor(row As DataGridRow, rowIndex As Integer, ListAgentFin As String, ListAgentAffectation As String)
        Dim rowView As DataRowView = TryCast(row.Item, DataRowView)
        If rowView Is Nothing OrElse rowIndex < 0 OrElse rowIndex >= rowView.Row.ItemArray.Length Then Return

        row.ClearValue(DataGridRow.BackgroundProperty)
        row.ClearValue(DataGridRow.ForegroundProperty)

        Dim agentName As String = Convert.ToString(rowView.Row.Item(rowIndex))
        If Not String.IsNullOrEmpty(ListAgentFin) AndAlso
           ListAgentFin.IndexOf(agentName, StringComparison.OrdinalIgnoreCase) >= 0 Then
            row.SetResourceReference(DataGridRow.ForegroundProperty, "ThemeTableAlertTextBrush")
        End If
        If Not String.IsNullOrEmpty(ListAgentAffectation) AndAlso
           ListAgentAffectation.IndexOf(agentName, StringComparison.OrdinalIgnoreCase) >= 0 Then
            SetThemeDataGridRow(row, "ThemeTableWarningRowBrush", "ThemeTableWarningRowTextBrush")
        End If
    End Sub
    Function maj() As Boolean
        maj = False
        If LocalModeBb Then Return False
        Dim userDatabasePath As String = Path.Combine(UserPath, "ManA.sqlite")
        If File.Exists(userDatabasePath) Then DbPath = userDatabasePath
        SyncLock generalDatabaseSyncLock
            Try
                Dim generalPath As String = Path.GetFullPath(DbPathGeneral)
                Dim localPath As String = Path.GetFullPath(DbPath)
                If String.Equals(generalPath, localPath, StringComparison.OrdinalIgnoreCase) Then Return True
                If Not File.Exists(generalPath) Then
                    Throw New FileNotFoundException("La base centrale est introuvable.", generalPath)
                End If

                If Not String.Equals(lastSyncedLocalPath, localPath, StringComparison.OrdinalIgnoreCase) Then
                    Dim statePath As String = localPath & ".sync"
                    Try
                        lastSyncedFingerprint = If(File.Exists(statePath), File.ReadAllText(statePath), "")
                    Catch stateException As Exception
                        System.Diagnostics.Debug.WriteLine("Impossible de lire l'état de synchronisation : " & stateException.ToString())
                        lastSyncedFingerprint = ""
                    End Try
                    lastSyncedLocalPath = localPath
                End If

                Dim sourceFingerprint As String = GetGeneralDatabaseFingerprint(generalPath)
                If File.Exists(localPath) AndAlso
                   String.Equals(sourceFingerprint, lastSyncedFingerprint, StringComparison.Ordinal) Then
                    generalDatabaseSyncWarningShown = False
                    Return True
                End If

                If File.Exists(localPath) Then
                    Dim backupDirectory As String = GetConfiguredBackupDirectory()
                    Directory.CreateDirectory(backupDirectory)
                    Dim backupPath As String = Path.Combine(backupDirectory, Path.GetFileName(localPath))
                    If Not String.Equals(Path.GetFullPath(backupPath), localPath, StringComparison.OrdinalIgnoreCase) Then
                        Dim localValidationError As String = Nothing
                        If IsSQLiteDatabaseHealthy(localPath, localValidationError) Then
                            CreateValidatedSQLiteSnapshot(localPath, backupPath)
                        End If
                    End If
                End If

                CreateValidatedSQLiteSnapshot(generalPath, localPath)
                File.SetAttributes(localPath, File.GetAttributes(localPath) Or FileAttributes.Hidden)
                lastSyncedFingerprint = sourceFingerprint
                Try
                    File.WriteAllText(localPath & ".sync", sourceFingerprint)
                Catch stateException As Exception
                    System.Diagnostics.Debug.WriteLine("Impossible d'enregistrer l'état de synchronisation : " & stateException.ToString())
                End Try
                generalDatabaseSyncWarningShown = False
                maj = True
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Échec de la synchronisation SQLite : " & ex.ToString())
            End Try
        End SyncLock
    End Function

    Private Function GetGeneralDatabaseFingerprint(databasePath As String) As String
        Return Path.GetFullPath(databasePath) & "|" &
               GetDatabaseFileState(databasePath) & "|" &
               GetDatabaseFileState(databasePath & "-wal")
    End Function

    Private Function GetDatabaseFileState(filePath As String) As String
        If Not File.Exists(filePath) Then Return "missing"
        Dim fileInfo As New FileInfo(filePath)
        Return fileInfo.Length.ToString(Globalization.CultureInfo.InvariantCulture) & ":" &
               fileInfo.LastWriteTimeUtc.Ticks.ToString(Globalization.CultureInfo.InvariantCulture)
    End Function

    Public Sub StartGeneralDatabaseListener()
        If LocalModeBb Then Return
        StopGeneralDatabaseListener()

        Dim generalPath As String = Path.GetFullPath(DbPathGeneral)
        Dim directoryPath As String = Path.GetDirectoryName(generalPath)
        If String.IsNullOrWhiteSpace(directoryPath) OrElse Not Directory.Exists(directoryPath) Then
            Throw New DirectoryNotFoundException("Le dossier de la base générale est introuvable : " & directoryPath)
        End If

        generalDatabaseWatcher = New FileSystemWatcher(directoryPath) With {
            .Filter = Path.GetFileName(generalPath) & "*",
            .NotifyFilter = NotifyFilters.FileName Or NotifyFilters.LastWrite Or NotifyFilters.Size,
            .IncludeSubdirectories = False,
            .EnableRaisingEvents = False
        }
        AddHandler generalDatabaseWatcher.Changed, AddressOf GeneralDatabaseWatcher_Changed
        AddHandler generalDatabaseWatcher.Created, AddressOf GeneralDatabaseWatcher_Changed
        AddHandler generalDatabaseWatcher.Deleted, AddressOf GeneralDatabaseWatcher_Changed
        AddHandler generalDatabaseWatcher.Renamed, AddressOf GeneralDatabaseWatcher_Renamed
        AddHandler generalDatabaseWatcher.Error, AddressOf GeneralDatabaseWatcher_Error
        AddHandler generalDatabaseSyncTimer.Tick, AddressOf GeneralDatabaseSyncTimer_Tick
        AddHandler generalDatabasePollTimer.Tick, AddressOf GeneralDatabasePollTimer_Tick
        AddHandler DbManipulation.GeneralDatabaseChanged, AddressOf QueueGeneralDatabaseSync
        generalDatabaseWatcher.EnableRaisingEvents = True
        generalDatabasePollTimer.Start()
    End Sub

    Public Sub StopGeneralDatabaseListener()
        generalDatabaseSyncTimer.Stop()
        generalDatabasePollTimer.Stop()
        RemoveHandler generalDatabaseSyncTimer.Tick, AddressOf GeneralDatabaseSyncTimer_Tick
        RemoveHandler generalDatabasePollTimer.Tick, AddressOf GeneralDatabasePollTimer_Tick
        RemoveHandler DbManipulation.GeneralDatabaseChanged, AddressOf QueueGeneralDatabaseSync
        If generalDatabaseWatcher IsNot Nothing Then
            RemoveHandler generalDatabaseWatcher.Changed, AddressOf GeneralDatabaseWatcher_Changed
            RemoveHandler generalDatabaseWatcher.Created, AddressOf GeneralDatabaseWatcher_Changed
            RemoveHandler generalDatabaseWatcher.Deleted, AddressOf GeneralDatabaseWatcher_Changed
            RemoveHandler generalDatabaseWatcher.Renamed, AddressOf GeneralDatabaseWatcher_Renamed
            RemoveHandler generalDatabaseWatcher.Error, AddressOf GeneralDatabaseWatcher_Error
            generalDatabaseWatcher.Dispose()
            generalDatabaseWatcher = Nothing
        End If
    End Sub

    Private Sub GeneralDatabaseWatcher_Changed(sender As Object, e As FileSystemEventArgs)
        If IsGeneralDatabaseFile(e.FullPath) Then QueueGeneralDatabaseSync(sender, EventArgs.Empty)
    End Sub

    Private Sub GeneralDatabaseWatcher_Renamed(sender As Object, e As RenamedEventArgs)
        If IsGeneralDatabaseFile(e.FullPath) OrElse IsGeneralDatabaseFile(e.OldFullPath) Then
            QueueGeneralDatabaseSync(sender, EventArgs.Empty)
        End If
    End Sub

    Private Sub GeneralDatabaseWatcher_Error(sender As Object, e As ErrorEventArgs)
        System.Diagnostics.Debug.WriteLine("Le suivi de la base générale a rencontré une erreur : " & e.GetException().ToString())
        QueueGeneralDatabaseSync(sender, EventArgs.Empty)
    End Sub

    Private Function IsGeneralDatabaseFile(filePath As String) As Boolean
        Dim basePath As String = Path.GetFullPath(DbPathGeneral)
        Dim changedPath As String = Path.GetFullPath(filePath)
        Return String.Equals(changedPath, basePath, StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(changedPath, basePath & "-wal", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub QueueGeneralDatabaseSync(sender As Object, e As EventArgs)
        Dim dispatcher = Application.Current?.Dispatcher
        If dispatcher Is Nothing OrElse dispatcher.HasShutdownStarted OrElse dispatcher.HasShutdownFinished Then Return

        Dim queueAction As Action =
            Sub()
                generalDatabaseSyncTimer.Stop()
                generalDatabaseSyncTimer.Start()
            End Sub
        If dispatcher.CheckAccess() Then
            queueAction()
        Else
            dispatcher.BeginInvoke(queueAction)
        End If
    End Sub

    Private Sub GeneralDatabaseSyncTimer_Tick(sender As Object, e As EventArgs)
        generalDatabaseSyncTimer.Stop()
        SynchronizeGeneralDatabaseIfNeeded()
    End Sub

    Private Sub GeneralDatabasePollTimer_Tick(sender As Object, e As EventArgs)
        SynchronizeGeneralDatabaseIfNeeded()
    End Sub

    Private Async Sub SynchronizeGeneralDatabaseIfNeeded()
        If LocalModeBb Then Return

        Dim synchronized As Boolean
        Try
            synchronized = Await Task.Run(Function() maj())
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Échec de la synchronisation SQLite en arrière-plan : " & ex.ToString())
            synchronized = False
        End Try

        If Not synchronized AndAlso Not generalDatabaseSyncWarningShown Then
            generalDatabaseSyncWarningShown = True
            MessageBox.Show(
                "La base locale n'a pas pu être actualisée depuis la base générale. Les dernières données locales restent disponibles.",
                "Synchronisation en attente",
                MessageBoxButton.OK,
                MessageBoxImage.Warning)
        End If
    End Sub

    Public Sub StartProcess(path As String)
        If File.Exists(path) Then
            Process.Start(path)
        Else
            MessageBox.Show("Fichier introuvable : " & path)
        End If
    End Sub
    Public Sub StopProcess(processName As String)
        For Each p As Process In Process.GetProcessesByName(processName)
            Try
                p.Kill()
                p.WaitForExit()
            Catch ex As Exception
                MessageBox.Show("Erreur lors de l'arrêt de " & processName & " : " & ex.Message)
            End Try
        Next
    End Sub
    Public Sub openZoomWdw(iObjet As Object)
        Try
            Dim IwdwZ As New wdwZoom
            IwdwZ.grdPrincipal.Children.Add(iObjet)
            IwdwZ.Show()

        Catch ex As Exception
            ShowTempMsg(ex.Message)
        End Try
    End Sub
    Public Sub openPointMensuel(Debut As Date, Fin As Date, MotCle As String, Titre As String, Optional NomFichier As String = "DATA_EXPORTED")
        Dim iPointMens As New wdwPointMensuel
        iPointMens.dtpDebutSearch.SelectedDate = Debut
        iPointMens.dtpFinSearch.SelectedDate = Fin
        iPointMens.tbkNomFichier.Text = NomFichier
        Try
            iPointMens.Title = Titre

            iPointMens.dgPointGlobal.ItemsSource = LoadDataTable(MotCle, $"Data Source={UserPath}\ManA.sqlite;Version=3;").DefaultView
            iPointMens.Show()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Function ReadCsvTable(filePath As String) As DataTable
        Dim delimiter As String
        Using reader As New StreamReader(filePath, Encoding.UTF8, True)
            Dim firstLine As String = reader.ReadLine()
            If firstLine Is Nothing Then Return New DataTable()
            delimiter = If(CountCsvDelimiters(firstLine, ";"c) > CountCsvDelimiters(firstLine, ","c), ";", ",")
        End Using

        Dim table As New DataTable()
        Using parser As New TextFieldParser(filePath, Encoding.UTF8, True)
            parser.TextFieldType = FieldType.Delimited
            parser.SetDelimiters(delimiter)
            parser.HasFieldsEnclosedInQuotes = True
            parser.TrimWhiteSpace = False

            If parser.EndOfData Then Return table
            Dim headers As String() = parser.ReadFields()
            For Each header As String In headers
                Dim normalizedHeader As String = NormalizeCsvHeader(header)
                If normalizedHeader.Length = 0 Then Throw New FormatException("Une colonne du CSV ne possède pas d'en-tête.")
                If table.Columns.Contains(normalizedHeader) Then Throw New FormatException("L'en-tête CSV contient plusieurs colonnes « " & header & " ».")
                table.Columns.Add(normalizedHeader, GetType(String))
            Next

            Dim recordNumber As Integer = 1
            While Not parser.EndOfData
                Dim fields As String() = parser.ReadFields()
                recordNumber += 1
                If fields Is Nothing OrElse fields.All(Function(field) String.IsNullOrWhiteSpace(field)) Then Continue While
                If fields.Length <> headers.Length Then
                    Throw New FormatException("L'enregistrement " & recordNumber & " contient " & fields.Length & " champ(s), au lieu de " & headers.Length & ".")
                End If
                table.Rows.Add(fields.Select(Function(field) field.Trim()).ToArray())
            End While
        End Using
        Return table
    End Function

    Public Function CsvHasColumn(table As DataTable, ParamArray headerNames As String()) As Boolean
        Return table.Columns.Cast(Of DataColumn)().Any(
            Function(column) headerNames.Any(Function(header) column.ColumnName = NormalizeCsvHeader(header)))
    End Function

    Public Function CsvValue(row As DataRow, ParamArray headerNames As String()) As String
        For Each header As String In headerNames
            Dim columnName As String = NormalizeCsvHeader(header)
            If row.Table.Columns.Contains(columnName) AndAlso Not row.IsNull(columnName) Then
                Return Convert.ToString(row(columnName)).Trim()
            End If
        Next
        Return ""
    End Function

    Public Function CsvDateValue(value As String, rowNumber As Integer, fieldName As String) As DateTime
        Dim parsedDate As DateTime
        Dim formats As String() = {"yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy"}
        If DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate) OrElse
           DateTime.TryParse(value, CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None, parsedDate) Then
            Return parsedDate
        End If
        Throw New FormatException("Ligne " & rowNumber & " : la date « " & fieldName & " » est invalide (« " & value & " »).")
    End Function

    Private Function NormalizeCsvHeader(value As String) As String
        Dim normalized As String = value.Normalize(NormalizationForm.FormD)
        Dim result As New StringBuilder()
        For Each character As Char In normalized
            If CharUnicodeInfo.GetUnicodeCategory(character) <> UnicodeCategory.NonSpacingMark AndAlso Char.IsLetterOrDigit(character) Then
                result.Append(Char.ToLowerInvariant(character))
            End If
        Next
        Return result.ToString()
    End Function

    Private Function CountCsvDelimiters(value As String, delimiter As Char) As Integer
        Dim count As Integer = 0
        Dim insideQuotes As Boolean = False
        Dim index As Integer = 0
        While index < value.Length
            If value(index) = """"c Then
                If insideQuotes AndAlso index + 1 < value.Length AndAlso value(index + 1) = """"c Then
                    index += 1
                Else
                    insideQuotes = Not insideQuotes
                End If
            ElseIf value(index) = delimiter AndAlso Not insideQuotes Then
                count += 1
            End If
            index += 1
        End While
        Return count
    End Function
End Module
