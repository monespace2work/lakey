Public Class usercEvenement
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        Dim istr As String = " select format(e.DateEvt) as DateEvt, e.SiteEvent,e.AgentsEvt,e.DescEvt,e.ResolEvt,e.MemoEvt,format(e.DateResolEvt) as DateResolEvt,e.RefEvt,e.InternExtEvt"
        istr += "  from events e "
        istr += " ORDER BY e.ResolEvt, e.DateEvt "
        DgEvent.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbEvent.Text = "Nombre d'événements en attente de résolution : " & MayStatVal("select count(*) from events  where ResolEvt=0") ' & " | Nombre d'agents actifs sans un N° CNSSS : " & MayStatVal("select count(*) from agents a where a.Actif=1 and  (a.Nsecu like '' and a.Nsecu not null)")

    End Sub
End Class
