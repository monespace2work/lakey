Imports ClosedXML.Excel
Imports System.Windows.Threading
Imports System.IO
Imports System.Data
Module ObjectsManaging
    Public DocsDir As String = My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\LkyDocs"
    Dim iTimer As New DispatcherTimer
    Public iDureeTempMsg As Integer = 10
    Dim iTempMsg As New TempMsg

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
                        targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFDEFFE2"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFDEFFE2"))
                        End If
                    End If
                ElseIf iAge < AgeLimit2 Then
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFFFA6"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFFFA6"))
                        End If
                    End If
                Else
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFB4B4"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFB4B4"))
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
                        targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFDEFFE2"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFDEFFE2"))
                        End If
                    End If
                ElseIf iContrat.Contains(LCase(ContratKey2)) Then
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFF3FFBF"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFF3FFBF"))
                        End If
                    End If
                Else
                    Dim targetRow As DataGridRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                    Dim targetItem = iDataGrid.Items(eTdx)
                    If targetRow IsNot Nothing Then
                        targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFB4B4"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFB4B4"))
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
                        targetRow.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF0000"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF0000"))
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
                        targetRow.Background = Brushes.Yellow 'New SolidColorBrush(ColorConverter.ConvertFromString("#FFF3FFBF"))
                    Else
                        iDataGrid.ScrollIntoView(targetItem)
                        iDataGrid.UpdateLayout()
                        targetRow = CType(iDataGrid.ItemContainerGenerator.ContainerFromIndex(eTdx), DataGridRow)
                        If targetRow IsNot Nothing Then
                            targetRow.Background = Brushes.Yellow 'New SolidColorBrush(ColorConverter.ConvertFromString("#FFF3FFBF"))
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
        If Directory.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then DbPath = Path.Combine(UserPath, "ManA.sqlite") 'MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")}\ManA.sqlite;Version=3;"
        If (DbPathGeneral = DbPath) Or (Not File.Exists(DbPathGeneral)) Then
            'MainFrm.Connexion_Pbx.Visible = True
            'MsgBox(DbPathGeneral)
            maj = False
        Else
            Try
                Try
                    File.Copy(DbPath, BUDirPath & "\" & Path.GetFileName(DbPath), True)
                Catch ex As Exception
                End Try
                Try
                    If File.Exists(DbPath) Then File.Delete(DbPath)
                    File.Copy(DbPathGeneral, DbPath, True)
                    Dim Fi As New FileInfo(DbPath)
                    Fi.Attributes = FileAttributes.Hidden
                    maj = True
                Catch ex As Exception
                    'MainFrm.Connexion_Pbx.Visible = True
                    maj = False
                End Try
            Catch ex As Exception
                'MsgBox("Code 100 : " & ex.Message)
                maj = False
            End Try

        End If

    End Function

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
