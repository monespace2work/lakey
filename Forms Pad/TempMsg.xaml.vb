Imports System.Windows.Threading
Public Class TempMsg

    Dim iTimer As New DispatcherTimer

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        iTimer.Interval = TimeSpan.FromSeconds(iDureeTempMsg)
        AddHandler iTimer.Tick, AddressOf OniTimerElapsed
        iTimer.Start()
    End Sub
    Public Sub OniTimerElapsed()
        Me.Close()
    End Sub

    Private Sub tbxMsg_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles tbxMsg.MouseDoubleClick
        iTimer.Stop()
    End Sub
End Class
