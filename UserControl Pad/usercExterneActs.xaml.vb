Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Linq

Public Enum SupplementView
    Memo
    Events
    Rappel
    Compta
    Cnss
End Enum

Public Class SupplementPreviewItem
    Public Property DateText As String
    Public Property Title As String
    Public Property Details As String
End Class

Public Class usercExterneActs
    Private connectionString As String

    Public Property MemoPreview As New ObservableCollection(Of SupplementPreviewItem)
    Public Property EventPreview As New ObservableCollection(Of SupplementPreviewItem)
    Public Property RappelPreview As New ObservableCollection(Of SupplementPreviewItem)
    Public Property ComptaPreview As New ObservableCollection(Of SupplementPreviewItem)
    Public Property CnssPreview As New ObservableCollection(Of SupplementPreviewItem)

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        connectionString = $"Data Source={DbPath};Version=3;"

        MemoPreview = LoadPreview(
            "SELECT m.DateMem AS Date, m.MemContent AS Title, m.Actif " &
            "FROM memos m ORDER BY datetime(m.DateMem) DESC, m.IdMem DESC LIMIT 3",
            Function(row) New SupplementPreviewItem With {
                .DateText = Value(row, "Date"),
                .Title = Value(row, "Title"),
                .Details = StatusText(Value(row, "Actif"))
            })
        EventPreview = LoadPreview(
            "SELECT e.DateEvt AS Date, " &
            "COALESCE(NULLIF(trim(e.SiteEvent), ''), 'Événement') AS Title, " &
            "e.DescEvt AS Description, e.ResolEvt " &
            "FROM events e ORDER BY date(e.DateEvt) DESC, e.RefEvt DESC LIMIT 3",
            Function(row) New SupplementPreviewItem With {
                .DateText = Value(row, "Date"),
                .Title = Value(row, "Title"),
                .Details = String.Join(" | ", {
                    Value(row, "Description"),
                    If(IsTrue(Value(row, "ResolEvt")), "Résolu", "En attente")
                }.Where(Function(value) Not String.IsNullOrWhiteSpace(value)))
            })
        RappelPreview = LoadPreview(
            "SELECT r.Debut AS Date, r.NoteRappel AS Title, r.Fin AS Fin, r.Actif " &
            "FROM rappels r ORDER BY r.Id DESC LIMIT 3",
            Function(row) New SupplementPreviewItem With {
                .DateText = Value(row, "Date"),
                .Title = Value(row, "Title"),
                .Details = "Fin : " & Value(row, "Fin") & " | " & StatusText(Value(row, "Actif"))
            })

        If CanAddDataSpecial Then
            cardCompta.Visibility = Visibility.Visible
            cardCnss.Visibility = Visibility.Visible
            ComptaPreview = LoadPreview(
                "SELECT a.OpTime AS Date, a.NomPrenoms AS Title, a.Banque, a.NumCompte " &
                "FROM agents a " &
                "WHERE trim(coalesce(a.Banque, '')) <> '' OR trim(coalesce(a.NumCompte, '')) <> '' " &
                "ORDER BY datetime(a.OpTime) DESC, a.RefAgent DESC LIMIT 3",
                Function(row) New SupplementPreviewItem With {
                    .DateText = Value(row, "Date"),
                    .Title = Value(row, "Title"),
                    .Details = String.Join(" | ", {
                        Value(row, "Banque"),
                        Value(row, "NumCompte")
                    }.Where(Function(value) Not String.IsNullOrWhiteSpace(value)))
                })
            CnssPreview = LoadPreview(
                "SELECT a.OpTime AS Date, a.NomPrenoms AS Title, a.Nsecu " &
                "FROM agents a " &
                "WHERE trim(coalesce(a.Nsecu, '')) <> '' " &
                "ORDER BY datetime(a.OpTime) DESC, a.RefAgent DESC LIMIT 3",
                Function(row) New SupplementPreviewItem With {
                    .DateText = Value(row, "Date"),
                    .Title = Value(row, "Title"),
                    .Details = "N° CNSS : " & Value(row, "Nsecu")
                })
        Else
            cardCompta.Visibility = Visibility.Collapsed
            cardCnss.Visibility = Visibility.Collapsed
        End If

        MemoPreview = EnsurePreviewItems(MemoPreview)
        EventPreview = EnsurePreviewItems(EventPreview)
        RappelPreview = EnsurePreviewItems(RappelPreview)
        ComptaPreview = EnsurePreviewItems(ComptaPreview)
        CnssPreview = EnsurePreviewItems(CnssPreview)
        DataContext = Me
    End Sub

    Private Function LoadPreview(query As String,
                                 createItem As Func(Of DataRow, SupplementPreviewItem)) As ObservableCollection(Of SupplementPreviewItem)
        Dim rows As DataTable = LoadDataTable(query, connectionString)
        Dim preview As New ObservableCollection(Of SupplementPreviewItem)
        For Each row As DataRow In rows.Rows
            preview.Add(createItem(row))
        Next
        Return preview
    End Function

    Private Shared Function EnsurePreviewItems(preview As ObservableCollection(Of SupplementPreviewItem)) As ObservableCollection(Of SupplementPreviewItem)
        If preview.Count = 0 Then
            preview.Add(New SupplementPreviewItem With {.Title = "Aucune donnée récente"})
        End If
        Return preview
    End Function

    Private Shared Function Value(row As DataRow, columnName As String) As String
        If row.IsNull(columnName) Then Return String.Empty
        Return Convert.ToString(row(columnName))
    End Function

    Private Shared Function StatusText(value As String) As String
        Return If(IsTrue(value), "Actif", "Inactif")
    End Function

    Private Shared Function IsTrue(value As String) As Boolean
        Return String.Equals(value, "1", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(value, "True", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub RecentOptions_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs)
        Dim current As DependencyObject = TryCast(sender, ScrollViewer)
        While current IsNot Nothing
            Dim scrollViewer As ScrollViewer = TryCast(current, ScrollViewer)
            If scrollViewer IsNot Nothing AndAlso CanScrollInDirection(scrollViewer, e.Delta) Then
                Dim lineCount As Integer = Math.Max(1, CInt(Math.Round(Math.Abs(e.Delta) / 120.0 * 3)))
                For index = 1 To lineCount
                    If e.Delta < 0 Then
                        scrollViewer.LineDown()
                    Else
                        scrollViewer.LineUp()
                    End If
                Next
                e.Handled = True
                Return
            End If
            current = VisualTreeHelper.GetParent(current)
        End While
    End Sub

    Private Shared Function CanScrollInDirection(scrollViewer As ScrollViewer, delta As Integer) As Boolean
        If delta < 0 Then Return scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight
        If delta > 0 Then Return scrollViewer.VerticalOffset > 0
        Return False
    End Function

    Private Async Sub BtnVoirPlus_Click(sender As Object, e As RoutedEventArgs)
        Dim button As Button = TryCast(sender, Button)
        Dim view As SupplementView
        If button Is Nothing OrElse
           Not [Enum].TryParse(Of SupplementView)(Convert.ToString(button.Tag), True, view) Then
            MessageBox.Show("L'option supplémentaire sélectionnée n'est pas reconnue.",
                            "Outils supplémentaires", MessageBoxButton.OK, MessageBoxImage.Error)
            Return
        End If

        Dim mainWindow As MainWindow = MainWindow.FindOpenMainWindow()
        If mainWindow Is Nothing Then
            MessageBox.Show("La fenêtre principale est introuvable.",
                            "Outils supplémentaires", MessageBoxButton.OK, MessageBoxImage.Error)
            Return
        End If

        Await mainWindow.OpenSupplementView(view)
    End Sub
End Class
