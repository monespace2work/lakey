Class HelpWindow
    Private Sub btnExportPdf_Click(sender As Object, e As RoutedEventArgs)
        Dim printDialog As New PrintDialog()
        If printDialog.ShowDialog() <> True Then Return

        HelpDocument.PageWidth = printDialog.PrintableAreaWidth
        HelpDocument.PageHeight = printDialog.PrintableAreaHeight
        HelpDocument.ColumnWidth = Math.Max(1, printDialog.PrintableAreaWidth - HelpDocument.PagePadding.Left - HelpDocument.PagePadding.Right)
        Dim documentSource = DirectCast(HelpDocument, IDocumentPaginatorSource)
        printDialog.PrintDocument(documentSource.DocumentPaginator, "Guide d'utilisation LAKEY")
    End Sub
End Class
