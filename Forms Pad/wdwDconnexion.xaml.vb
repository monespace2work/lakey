Public Class wdwDconnexion
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        btnValider.Visibility = Visibility.Collapsed
        tbkUSer.Text = UserName
        pswUser.Focus()

    End Sub

    Private Sub btnSortie_Click(sender As Object, e As RoutedEventArgs) Handles btnSortie.Click
        If MsgBox("Vous allez sortir de cette session et FERMER L'APPLICATION" & Chr(13) & "SORTIR ?", MsgBoxStyle.YesNo, "ATTENTION...") = MsgBoxResult.Yes Then
            Me.DialogResult = False
            Me.Close()
            'Me.DialogResult = False
            'Application.Current.Shutdown()
            'Me.Close()
        End If

    End Sub

    Private Sub pswUser_PasswordChanged(sender As Object, e As RoutedEventArgs) Handles pswUser.PasswordChanged
        If pswUser.Password.Length >= 3 Then
            btnValider.Visibility = Visibility.Visible
            btnValider.IsDefault = True
        Else
            btnValider.Visibility = Visibility.Collapsed
        End If

    End Sub

    Private Sub btnValider_Click(sender As Object, e As RoutedEventArgs) Handles btnValider.Click
        If pswUser.Password = UserPW Then
            'pswUser.Password = ""
            Me.DialogResult = True
            'Me.Close()
        Else
            MsgBox("Mot de passe incorrect")
            pswUser.Focus()
            pswUser.SelectAll()
            'Me.DialogResult = False
        End If

    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If pswUser.Password = UserPW Then
            pswUser.Password = ""
            Me.DialogResult = True
            'Me.Close()
        Else
            Me.DialogResult = False
        End If
        'Me.DialogResult = False
        'Application.Current.Shutdown()
    End Sub

End Class
