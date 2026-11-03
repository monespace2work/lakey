Public Class usercEvenement
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim istr As String = " select format(e.DateEvt) as DateEvt, e.SiteEvent,e.AgentsEvt,e.DescEvt,e.ResolEvt,e.MemoEvt,format(e.DateResolEvt) as DateResolEvt,e.RefEvt,e.InternExtEvt"
        istr += "  from events e "
        istr += " ORDER BY e.ResolEvt, e.DateEvt "
        Dim eventData = LoadDataTable(istr)
        DgEvent.ItemsSource = eventData.DefaultView
        tblNbEvent.Text = "Nombre d'événements en attente de résolution : " & eventData.Select("ResolEvt = 0").Length

    End Sub
End Class
