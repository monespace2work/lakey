Imports System.IO
Public Class ReseauSelect
    Dim iCbxDrive As New ComboBox
    Dim iFilePath As String = Path.GetDirectoryName(DbPathGeneral)
    Dim iFileName As String = Path.GetFileName(DbPathGeneral)


    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        LoadDriveActif()
        'tbxPathDrive.Text = iFilePath
        cbxLetterDrive.Items.Clear()

        For Each iLetter As Char In "ZYXWVUTSRQPONMLKJIHGFEDCBA"
            If Not iCbxDrive.Items.Contains(iLetter & ":\") Then cbxLetterDrive.Items.Add(iLetter & ":")
        Next
        If cbxLetterDrive.Items.Count > 0 Then cbxLetterDrive.SelectedIndex = 0

    End Sub
    Sub LoadPathes()

    End Sub
    Sub LoadDriveActif()
        iCbxDrive.Items.Clear()
        'Dim Itext As String = ""
        For Each iDrive As DriveInfo In DriveInfo.GetDrives
            If iDrive.IsReady Then
                iCbxDrive.Items.Add(iDrive.Name)
                'Itext &= iDrive.Name & Environment.NewLine
            End If
        Next
        'MsgBox(Itext)
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As RoutedEventArgs) Handles btnOpen.Click
        Dim iOfd As New Forms.OpenFileDialog
        'iFbwsD.RootFolder = Environment.SpecialFolder.NetworkShortcuts 'Environment.SpecialFolder.MyComputer
        iOfd.Title = "Sélectionnez un dossier dans le RESEAU"
        'iOfd.ShowNewFolderButton = False
        iOfd.ShowDialog()
        Dim iFile As String = iOfd.FileName
        Dim Idirpath As String = Path.GetDirectoryName(iFile)
        iFileName = Path.GetFileName(iFile)
        If Not Idirpath.StartsWith("\\") Or Idirpath = "" Then
            MsgBox("Le dossier selectionné n'est pas un DOSSIER RESEAU")
        Else
            tbxPathDrive.Text = Idirpath
        End If
    End Sub

    Private Sub cbxLetterDrive_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxLetterDrive.SelectionChanged
        activSaveBtn()
    End Sub
    Sub activSaveBtn()
        If cbxLetterDrive.SelectedIndex >= 0 And tbxPathDrive.Text.StartsWith("\\") Then
            btnSave.Visibility = Visibility.Visible
        Else
            btnSave.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub tbxPathDrive_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxPathDrive.TextChanged
        activSaveBtn()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If MonteMapDrive(tbxPathDrive.Text, cbxLetterDrive.SelectedItem) Then
            DbPathGeneral = Path.Combine(cbxLetterDrive.SelectedItem & "\", iFileName)
            MsgBox(DbPathGeneral)
            If File.Exists(DbPathGeneral) Then
                If SaveNewData("update Sets set DBpathG='" & DbPathGeneral & "' where IdSet=1", "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") Then Me.Close()
            End If
        End If
        DbPathGeneral = PaphBackUp()
    End Sub
End Class
