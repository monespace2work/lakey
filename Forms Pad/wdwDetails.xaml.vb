Public Class wdwDetails
    Public Sub New(Titre As String, SsTitre As String, iUserC As UserControl)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        Me.Title = Titre
        tbkTitre.Text = Titre
        tbkSousTitre.Text = SsTitre
        usrcCentralD.Content = iUserC

        Dim iMain = Application.Current.MainWindow
        Me.Width = CInt(My.Computer.Screen.Bounds.Width * 0.6)
        Me.Height = CInt(My.Computer.Screen.Bounds.Height * 0.6)
        'Me.Width = CInt(iMain.RenderSize.Width * 0.6)
        'Me.Height = CInt(iMain.RenderSize.Height * 0.6)
        Me.Left = iMain.Left + CInt((iMain.Width - Me.Width) / 2)
        Me.Top = iMain.Top + CInt((iMain.Height - Me.Height) / 2)
        'Me.WindowState = WindowState.Maximized
    End Sub

End Class
