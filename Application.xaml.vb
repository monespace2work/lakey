Class Application
    Shared Sub New()
        EventManager.RegisterClassHandler(GetType(UserControl),
                                          FrameworkElement.LoadedEvent,
                                          New RoutedEventHandler(AddressOf OnUserControlLoaded),
                                          True)
    End Sub

    Private Shared Sub OnUserControlLoaded(sender As Object, e As RoutedEventArgs)
        Dim mainWindow = TryCast(Current.MainWindow, MainWindow)
        If mainWindow IsNot Nothing Then mainWindow.NotifyUserControlLoading()
    End Sub
End Class
