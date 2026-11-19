Imports System.Data
Public Class usercEvenement
    Private eventData As DataTable

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim istr As String = " select format(e.DateEvt) as DateEvt, e.SiteEvent,e.AgentsEvt,e.DescEvt,e.ResolEvt,e.MemoEvt,format(e.DateResolEvt) as DateResolEvt,e.RefEvt,e.InternExtEvt"
        istr += "  from events e "
        istr += " ORDER BY e.ResolEvt, e.DateEvt "
        eventData = LoadDataTable(istr)
        DgEvent.ItemsSource = eventData.DefaultView
        ApplyActifFilter()
    End Sub

    Private Sub ApplyActifFilter()
        If eventData Is Nothing Then Return

        eventData.DefaultView.RowFilter = BuildStatusRowFilter(
            "ResolEvt",
            ckbxShowActive.IsChecked.GetValueOrDefault(),
            ckbxShowInactive.IsChecked.GetValueOrDefault(),
            activeValue:=0,
            inactiveValue:=1)
        Dim pendingCount As Integer = If(
            ckbxShowActive.IsChecked.GetValueOrDefault(),
            eventData.Select("ResolEvt = 0").Length,
            0)
        tblNbEvent.Text = "Événements affichés : " & eventData.DefaultView.Count &
                          " | en attente de résolution : " & pendingCount
    End Sub

    Private Sub DisplayStatus_CheckedChanged(sender As Object, e As RoutedEventArgs) _
        Handles ckbxShowActive.Checked, ckbxShowActive.Unchecked,
                ckbxShowInactive.Checked, ckbxShowInactive.Unchecked
        ApplyActifFilter()
    End Sub
End Class
