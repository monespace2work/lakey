Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Threading

Public Class usercAnniversaires
    Private ReadOnly searchTimer As New DispatcherTimer With {
        .Interval = TimeSpan.FromSeconds(2)
    }

    Public Sub New()
        InitializeComponent()
        AddHandler searchTimer.Tick, AddressOf SearchTimer_Tick
    End Sub

    Private Sub usercAnniversaires_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ActualiserAnniversaires()
    End Sub

    Private Sub usercAnniversaires_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
        searchTimer.Stop()
    End Sub

    Private Sub Period_Checked(sender As Object, e As RoutedEventArgs)
        If IsLoaded Then ActualiserAnniversaires()
    End Sub

    Private Sub Search_TextChanged(sender As Object, e As TextChangedEventArgs)
        If Not IsLoaded Then Return
        searchTimer.Stop()
        searchTimer.Start()
    End Sub

    Private Sub Search_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key <> Key.Enter Then Return
        e.Handled = True
        searchTimer.Stop()
        ActualiserAnniversaires()
    End Sub

    Private Sub SearchTimer_Tick(sender As Object, e As EventArgs)
        searchTimer.Stop()
        If IsLoaded Then ActualiserAnniversaires()
    End Sub

    Private Sub ActualiserAnniversaires()
        Try
            Dim connectionString As String = $"Data Source={DbPath};Version=3;"
            Dim recherche As String = tbxRecherche.Text.Trim()
            If recherche.Length > 0 Then
                dgAnniversaires.ItemsSource =
                    AnniversaireService.RechercherAnniversaires(connectionString, recherche)
            ElseIf rdoMoisPrecedent.IsChecked.GetValueOrDefault() Then
                dgAnniversaires.ItemsSource =
                    AnniversaireService.GetAnniversairesMoisPrecedent(connectionString)
            Else
                Dim dateLimite As Date = If(rdoMois.IsChecked.GetValueOrDefault(),
                                            Date.Today.AddMonths(1), Date.Today.AddDays(7))
                dgAnniversaires.ItemsSource =
                    AnniversaireService.GetAnniversairesAVenir(connectionString, dateLimite)
            End If
        Catch ex As Exception
            MessageBox.Show("Impossible de charger les anniversaires : " & ex.Message,
                            "Anniversaires", MessageBoxButton.OK, MessageBoxImage.Error)
            dgAnniversaires.ItemsSource = Nothing
        End Try
    End Sub
End Class
