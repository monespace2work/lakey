Imports ClosedXML.Excel
Imports System.Windows.Threading
Imports System.IO
Imports System.Data
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
        Dim iTable As DataTable = TryCast(iDataGrid.ItemsSource, DataView).ToTable
        If IsNothing(iTable) Then
            Exit Sub
        Else
        End If
        For eTdx As Integer = 0 To iTable.Rows.Count - 1
            Try
                Dim iContrat As String = LCase((iTable.Rows(eTdx).Item(iRowOfType)))
                If LCase(ListAgentFin).Contains(LCase(iContrat)) Then
                    Dim targetItem = iDataGrid.Items(eTdx)
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    If targetRow IsNot Nothing Then
                        targetRow.SetResourceReference(DataGridRow.ForegroundProperty, "ThemeTableAlertTextBrush")
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.SetResourceReference(DataGridRow.ForegroundProperty, "ThemeTableAlertTextBrush")
                        End If
                    End If
                Else
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        'targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFB4B4"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            'targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFB4B4"))
                        End If
                    End If
                End If

                If LCase(ListAgentAffectation).Contains(LCase(iContrat)) Then
                    'MsgBox("ok")
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
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        Next

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

                If File.Exists(localPath) AndAlso Not String.IsNullOrWhiteSpace(BUDirPath) Then
                    Dim backupPath As String = Path.Combine(BUDirPath, Path.GetFileName(localPath))
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

    Private Sub SynchronizeGeneralDatabaseIfNeeded()
        If LocalModeBb Then Return

        If Not maj() AndAlso Not generalDatabaseSyncWarningShown Then
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
End Module
