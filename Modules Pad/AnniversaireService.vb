Imports System.Data.SQLite
Imports System.Globalization
Imports System.IO
Imports System.Media
Imports System.Threading.Tasks

Public Class AnniversaireAgent
    Public Property RefAgent As Integer
    Public Property NomPrenoms As String
    Public Property NumMatr As String
    Public Property VilleResidence As String
    Public Property DateNaiss As Date
    Public Property DateAnniversaire As Date
    Public Property JoursRestants As Integer
    Public Property Echeance As String
End Class

Public Module AnniversaireService
    Public Function GetAnniversairesAVenir(connectionString As String, dateLimite As Date) As List(Of AnniversaireAgent)
        Dim result As New List(Of AnniversaireAgent)
        Dim today As Date = Date.Today

        Using connection As New SQLiteConnection(connectionString)
            connection.Open()
            Using command As New SQLiteCommand(
                "SELECT RefAgent, NomPrenoms, NumMatr, VilleResidence, DateNaiss FROM agents " &
                "WHERE Actif = 1 AND DateNaiss IS NOT NULL AND TRIM(DateNaiss) <> ''",
                connection)
                Using reader As SQLiteDataReader = command.ExecuteReader()
                    While reader.Read()
                        Dim agentName As String = Convert.ToString(reader("NomPrenoms"))
                        Dim birthDate As Date
                        If Not TryParseBirthDate(Convert.ToString(reader("DateNaiss")), birthDate) Then
                            Throw New InvalidDataException(
                                "La date de naissance de l'agent « " & agentName & " » est invalide.")
                        End If

                        Dim anniversary As Date = GetAnniversaryDate(birthDate, today.Year)
                        If anniversary < today Then anniversary = GetAnniversaryDate(birthDate, today.Year + 1)

                        If anniversary <= dateLimite.Date Then
                            result.Add(CreateAnniversaireAgent(
                                Convert.ToInt32(reader("RefAgent")), agentName,
                                Convert.ToString(reader("NumMatr")), Convert.ToString(reader("VilleResidence")),
                                birthDate, anniversary))
                        End If
                    End While
                End Using
            End Using
        End Using

        Return result.OrderBy(Function(agent) agent.DateAnniversaire).
            ThenBy(Function(agent) agent.NomPrenoms).ToList()
    End Function

    Public Function GetAnniversairesMoisPrecedent(connectionString As String) As List(Of AnniversaireAgent)
        Dim result As New List(Of AnniversaireAgent)
        Dim today As Date = Date.Today
        Dim monthStart As Date = New Date(today.Year, today.Month, 1).AddMonths(-1)
        Dim monthEnd As Date = monthStart.AddMonths(1).AddDays(-1)

        Using connection As New SQLiteConnection(connectionString)
            connection.Open()
            Using command As New SQLiteCommand(
                "SELECT RefAgent, NomPrenoms, NumMatr, VilleResidence, DateNaiss FROM agents " &
                "WHERE Actif = 1 AND DateNaiss IS NOT NULL AND TRIM(DateNaiss) <> ''",
                connection)
                Using reader As SQLiteDataReader = command.ExecuteReader()
                    While reader.Read()
                        Dim agentName As String = Convert.ToString(reader("NomPrenoms"))
                        Dim birthDate As Date
                        If Not TryParseBirthDate(Convert.ToString(reader("DateNaiss")), birthDate) Then
                            Throw New InvalidDataException(
                                "La date de naissance de l'agent « " & agentName & " » est invalide.")
                        End If
                        If birthDate.Month <> monthStart.Month Then Continue While

                        Dim anniversary As Date = GetAnniversaryDate(birthDate, monthStart.Year)
                        If anniversary < monthStart OrElse anniversary > monthEnd Then Continue While
                        result.Add(CreateAnniversaireAgent(
                            Convert.ToInt32(reader("RefAgent")), agentName,
                            Convert.ToString(reader("NumMatr")), Convert.ToString(reader("VilleResidence")),
                            birthDate, anniversary))
                    End While
                End Using
            End Using
        End Using

        Return result.OrderBy(Function(agent) agent.DateAnniversaire).
            ThenBy(Function(agent) agent.NomPrenoms).ToList()
    End Function

    Public Function RechercherAnniversaires(connectionString As String, recherche As String) As List(Of AnniversaireAgent)
        Dim result As New List(Of AnniversaireAgent)
        Dim today As Date = Date.Today

        Using connection As New SQLiteConnection(connectionString)
            connection.Open()
            Using command As New SQLiteCommand(
                "SELECT RefAgent, NomPrenoms, NumMatr, VilleResidence, DateNaiss FROM agents " &
                "WHERE Actif = 1 AND DateNaiss IS NOT NULL AND TRIM(DateNaiss) <> '' " &
                "AND (NomPrenoms LIKE @recherche OR NumMatr LIKE @recherche " &
                "OR VilleResidence LIKE @recherche)",
                connection)
                command.Parameters.AddWithValue("@recherche", "%" & recherche.Trim() & "%")
                Using reader As SQLiteDataReader = command.ExecuteReader()
                    While reader.Read()
                        Dim agentName As String = Convert.ToString(reader("NomPrenoms"))
                        Dim birthDate As Date
                        If Not TryParseBirthDate(Convert.ToString(reader("DateNaiss")), birthDate) Then
                            Throw New InvalidDataException(
                                "La date de naissance de l'agent « " & agentName & " » est invalide.")
                        End If

                        Dim anniversary As Date = GetAnniversaryDate(birthDate, today.Year)
                        If anniversary < today Then anniversary = GetAnniversaryDate(birthDate, today.Year + 1)
                        result.Add(CreateAnniversaireAgent(
                            Convert.ToInt32(reader("RefAgent")), agentName,
                            Convert.ToString(reader("NumMatr")), Convert.ToString(reader("VilleResidence")),
                            birthDate, anniversary))
                    End While
                End Using
            End Using
        End Using

        Return result.OrderBy(Function(agent) agent.DateAnniversaire).
            ThenBy(Function(agent) agent.NomPrenoms).ToList()
    End Function

    Public Function GetAnniversairesDuJourNonVus(connectionString As String) As List(Of AnniversaireAgent)
        EnsureAcknowledgementTable(connectionString)
        Dim today = Date.Today
        Dim result = GetAnniversairesAVenir(connectionString, today)

        Using connection As New SQLiteConnection(connectionString)
            connection.Open()
            Using command As New SQLiteCommand(
                "SELECT AgentRef FROM AnniversaireVu WHERE Annee = @year",
                connection)
                command.Parameters.AddWithValue("@year", today.Year)
                Using reader As SQLiteDataReader = command.ExecuteReader()
                    Dim seenAgents As New HashSet(Of Integer)
                    While reader.Read()
                        seenAgents.Add(Convert.ToInt32(reader("AgentRef")))
                    End While
                    Return result.Where(Function(agent) Not seenAgents.Contains(agent.RefAgent)).ToList()
                End Using
            End Using
        End Using
    End Function

    Public Sub MarquerAnniversaireVu(connectionString As String, agentRef As Integer, year As Integer)
        EnsureAcknowledgementTable(connectionString)
        Using connection As New SQLiteConnection(connectionString)
            connection.Open()
            Using command As New SQLiteCommand(
                "INSERT OR REPLACE INTO AnniversaireVu (AgentRef, Annee, DateVu) " &
                "VALUES (@agentRef, @year, @dateSeen)",
                connection)
                command.Parameters.AddWithValue("@agentRef", agentRef)
                command.Parameters.AddWithValue("@year", year)
                command.Parameters.AddWithValue("@dateSeen", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Async Function JouerJingleAnniversaireAsync() As Task
        Await Task.Run(
            Sub()
                Const sampleRate As Integer = 22050
                Dim notes As Integer() = {392, 392, 440, 392, 523, 494}
                Const noteDuration As Double = 0.28
                Const silenceDuration As Double = 0.04
                Dim samplesPerNote As Integer = CInt(sampleRate * noteDuration)
                Dim silenceSamples As Integer = CInt(sampleRate * silenceDuration)
                Dim dataLength As Integer = notes.Length * (samplesPerNote + silenceSamples) * 2

                Using stream As New MemoryStream()
                    Using writer As New BinaryWriter(stream)
                        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"))
                        writer.Write(36 + dataLength)
                        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"))
                        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "))
                        writer.Write(16)
                        writer.Write(CShort(1))
                        writer.Write(CShort(1))
                        writer.Write(sampleRate)
                        writer.Write(sampleRate * 2)
                        writer.Write(CShort(2))
                        writer.Write(CShort(16))
                        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"))
                        writer.Write(dataLength)

                        For Each frequency As Integer In notes
                            For sample As Integer = 0 To samplesPerNote - 1
                                Dim envelope As Double = Math.Min(1.0, Math.Min(sample / 220.0, (samplesPerNote - sample) / 220.0))
                                Dim value As Short = CShort(Math.Sin(2.0 * Math.PI * frequency * sample / sampleRate) * 9000 * envelope)
                                writer.Write(value)
                            Next
                            For sample As Integer = 1 To silenceSamples
                                writer.Write(CShort(0))
                            Next
                        Next

                        stream.Position = 0
                        Using player As New SoundPlayer(stream)
                            player.PlaySync()
                        End Using
                    End Using
                End Using
            End Sub)
    End Function

    Private Sub EnsureAcknowledgementTable(connectionString As String)
        Using connection As New SQLiteConnection(connectionString)
            connection.Open()
            Using command As New SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS AnniversaireVu (" &
                "AgentRef INTEGER NOT NULL, Annee INTEGER NOT NULL, DateVu TEXT NOT NULL, " &
                "PRIMARY KEY (AgentRef, Annee))",
                connection)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Function TryParseBirthDate(value As String, ByRef birthDate As Date) As Boolean
        Dim formats As String() = {
            "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss",
            "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy"
        }
        Return Date.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture,
                                  DateTimeStyles.AllowWhiteSpaces, birthDate) OrElse
               Date.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, birthDate) OrElse
               Date.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, birthDate)
    End Function

    Private Function GetAnniversaryDate(birthDate As Date, year As Integer) As Date
        Dim day As Integer = birthDate.Day
        If birthDate.Month = 2 AndAlso day = 29 AndAlso Not Date.IsLeapYear(year) Then day = 28
        Return New Date(year, birthDate.Month, day)
    End Function

    Private Function CreateAnniversaireAgent(agentRef As Integer,
                                              agentName As String,
                                              matricule As String,
                                              villeResidence As String,
                                              birthDate As Date,
                                              anniversary As Date) As AnniversaireAgent
        Dim daysRemaining As Integer = CInt((anniversary.Date - Date.Today).TotalDays)
        Dim due As String
        If daysRemaining < 0 Then
            due = If(daysRemaining = -1, "Hier", "Il y a " & Math.Abs(daysRemaining) & " jours")
        ElseIf daysRemaining = 0 Then
            due = "Aujourd'hui"
        ElseIf daysRemaining = 1 Then
            due = "Demain"
        Else
            due = "Dans " & daysRemaining & " jours"
        End If

        Return New AnniversaireAgent With {
            .RefAgent = agentRef,
            .NomPrenoms = agentName,
            .NumMatr = matricule,
            .VilleResidence = villeResidence,
            .DateNaiss = birthDate,
            .DateAnniversaire = anniversary,
            .JoursRestants = daysRemaining,
            .Echeance = due
        }
    End Function
End Module
