Imports System.Net
Imports System.Net.Sockets
'Imports System.Data.SQLite

Module IpTraitement
    Public Function ConvertPathIp(iPath As String) As String
        If iPath.StartsWith("\\") Then
            ConvertPathIp = ResolveUncHostToIP(iPath)
        ElseIf System.IO.Directory.Exists(iPath) Then
            ConvertPathIp = iPath
        Else
            ConvertPathIp = ""
        End If
    End Function

    Function ResolveUncHostToIP(uncPath As String) As String
        Try
            ' Extraire le nom d'hôte depuis le chemin UNC
            Dim hostName As String = uncPath.Replace("\\", "").Split("\"c)(0)

            ' Résolution DNS du nom NetBIOS ou nom DNS
            Dim hostEntry As IPHostEntry = Dns.GetHostEntry(hostName)

            For Each ip As IPAddress In hostEntry.AddressList
                If ip.AddressFamily = AddressFamily.InterNetwork Then
                    Return ip.ToString()
                End If
            Next

            Return "Aucune adresse IPv4 trouvée."
        Catch ex As Exception
            Return "Erreur de résolution : " & ex.Message
        End Try
    End Function
    Public Sub EnableWALMode(connectionString As String)
        Try
            'Dim connectionString As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;"
            Using conn As New System.Data.SQLite.SQLiteConnection(connectionString)
                conn.Open()
                Dim cmd As New System.Data.SQLite.SQLiteCommand("PRAGMA journal_mode = WAL;", conn)
                Dim result = cmd.ExecuteScalar()
                If Not result.ToString().ToUpper().Equals("WAL") Then
                    MessageBox.Show("Échec de l'activation du mode WAL.")
                End If
            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Public Function CheckWALMode(connectionString As String) As Boolean
        CheckWALMode = False
        Try
            Using conn As New Data.SQLite.SQLiteConnection(connectionString)
                conn.Open()
                Dim cmd As New Data.SQLite.SQLiteCommand("PRAGMA journal_mode;", conn)
                Dim result = cmd.ExecuteScalar()?.ToString()

                If result Is Nothing OrElse Not result.Equals("wal", StringComparison.OrdinalIgnoreCase) Then
                    MessageBox.Show($"⚠️ Mode WAL inactif. Mode actuel : {result}", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning)
                    CheckWALMode = False
                Else
                    CheckWALMode = True
                End If
            End Using
        Catch ex As Exception
            CheckWALMode = False
            MessageBox.Show($"Erreur lors de la vérification WAL: {ex.Message}")
        End Try
    End Function
    Public Function MonteMapDrive(networkPath As String, driveLetter As String) As Boolean
        'Dim networkPath As String = PathTextBox.Text.Trim()
        'Dim driveLetter As String = DriveLetterTextBox.Text.Trim().ToUpper()
        MonteMapDrive = False
        If String.IsNullOrEmpty(networkPath) OrElse Not networkPath.StartsWith("\\") Then
            MsgBox("Veuillez entrer un chemin réseau valide (ex: \\serveur\partage).")
            'Return
        End If

        If Not driveLetter.EndsWith(":") Then
            driveLetter &= ":"
        End If

        Try
            Dim psi As New ProcessStartInfo("cmd.exe", $"/c net use {driveLetter} ""{networkPath}"" /persistent:yes")
            psi.CreateNoWindow = True
            psi.UseShellExecute = False
            psi.RedirectStandardOutput = True
            psi.RedirectStandardError = True

            Dim process As Process = Process.Start(psi)
            process.WaitForExit()

            Dim output As String = process.StandardOutput.ReadToEnd()
            Dim errorOutput As String = process.StandardError.ReadToEnd()

            If process.ExitCode = 0 Then
                MsgBox($"Lecteur {driveLetter} mappé avec succès sur {networkPath}")
                MonteMapDrive = True
            Else
                MsgBox($"Erreur : {errorOutput}")
                MonteMapDrive = False
            End If
        Catch ex As Exception
            MsgBox($"Exception : {ex.Message}")
            MonteMapDrive = False
        End Try
    End Function

End Module
