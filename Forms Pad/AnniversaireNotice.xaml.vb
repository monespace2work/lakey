Imports System.Windows
Imports System.Windows.Controls

Public Class AnniversaireNotice
    Private ReadOnly connectionString As String
    Private ReadOnly agentAcknowledged As Action
    Private ReadOnly currentYear As Integer = Date.Today.Year

    Public Sub New(agents As IEnumerable(Of AnniversaireAgent),
                   connectionString As String,
                   agentAcknowledged As Action)
        InitializeComponent()
        Me.connectionString = connectionString
        Me.agentAcknowledged = agentAcknowledged
        icAnniversaires.ItemsSource = agents.ToList()
    End Sub

    Private Sub MarkSeen_Click(sender As Object, e As RoutedEventArgs)
        Dim button As Button = TryCast(sender, Button)
        If button Is Nothing Then Return

        Try
            AnniversaireService.MarquerAnniversaireVu(
                connectionString, Convert.ToInt32(button.Tag), currentYear)
            Dim agents As List(Of AnniversaireAgent) =
                TryCast(icAnniversaires.ItemsSource, List(Of AnniversaireAgent))
            If agents IsNot Nothing Then
                agents.RemoveAll(Function(agent) agent.RefAgent = Convert.ToInt32(button.Tag))
                icAnniversaires.ItemsSource = Nothing
                icAnniversaires.ItemsSource = agents
            End If

            agentAcknowledged()
            If agents IsNot Nothing AndAlso agents.Count = 0 Then Close()
        Catch ex As Exception
            MessageBox.Show("Impossible d'enregistrer l'anniversaire comme vu : " & ex.Message,
                            "Anniversaires", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub Close_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub
End Class
