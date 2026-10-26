Public Class Login
    Dim iConnxStr As String
    Private Sub btnLogin_Click(sender As Object, e As RoutedEventArgs) Handles btnLogin.Click
        Try
            ' Keep the first SQLite call inside this Try so native-provider load
            ' failures are reported with their full exception details.
            Dim iLevel = UserTest(txtUsername.Text, txtPassword.Password, iConnxStr)
            'MsgBox(iLevel)
            If iLevel <> 1976 Then
                'UserLevel = iLevel
                If Not LoadUserProfil(iLevel, iConnxStr) Then
                    MessageBox.Show("Erreur de chargement du profil utilisateur", "ERREUR CRITIQUE", MessageBoxButton.OK, MessageBoxImage.Error)
                    Me.Close()
                End If

                If SessLogin(iConnxStr) Then
                    If Not IO.Directory.Exists(IO.Path.Combine(RootPath, UserName)) Then
                        Try
                            IO.Directory.CreateDirectory(IO.Path.Combine(RootPath, UserName))
                        Catch ex As Exception

                        End Try
                    End If
                    Try
                        UserPath = IO.Path.Combine(RootPath, UserName)
                        'MsgBox(UserPath)
                        If IO.File.Exists(DbPathGeneral) Then
                            Try
                                Dim iBaseName As String = IO.Path.GetFileName(DbPathGeneral)
                                DbPath = IO.Path.Combine(UserPath, iBaseName)
                                'MsgBox(DbPath)
                            Catch ex As Exception

                            End Try
                        End If
                    Catch ex As Exception

                    End Try
                    txtPassword.Password = ""
                    Me.Hide()
                    Dim iWnd As New MainWindow
                    iWnd.WindowState = WindowState.Maximized
                    Dim result = iWnd.ShowDialog()
                    If result = True Then
                        SessLogout(iConnxStr)
                        Me.Close()
                    Else
                        Me.Show()
                    End If
                Else
                    MsgBox("Errer acces au données de la base")
                    Me.Close()
                End If
            Else
                If MsgBox("REPRENDRE ?", MsgBoxStyle.YesNo, "Indentifiants incorrects") = MsgBoxResult.Yes Then
                    txtUsername.SelectAll()
                    txtUsername.Focus()
                Else
                    Me.Close()
                End If
            End If

        Catch ex As Exception
            MessageBox.Show(ex.ToString(), "Erreur pendant la connexion", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        'Dim MyPath As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JPB\LKY")
        If Not IO.Directory.Exists(RootPath) Then IO.Directory.CreateDirectory(RootPath)
        txtUsername.Focus()
        Me.Title = "Login - LAKEY V" & My.Application.Info.Version.ToString
    End Sub
    Function UserTest(uName As String, uPssw As String, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Integer
        UserTest = 1976
        Dim iStr As String = "select l.UserName, l.UserPassWord, l.UserLever from lada l "
        'MsgBox(iConxStr)
        Try
            Using iConx As New Data.SQLite.SQLiteConnection(iConxStr)
                iConx.Open()
                Using iCmd As New Data.SQLite.SQLiteCommand(iStr, iConx)
                    Dim iReader As Data.SQLite.SQLiteDataReader = iCmd.ExecuteReader
                    While iReader.Read
                        'MsgBox(iReader(2))
                        If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) AndAlso Not IsDBNull(iReader(2)) Then
                            'MsgBox(iReader(2))
                            If iReader(0) = uName And iReader(1) = uPssw Then
                                UserName = iReader(0)
                                UserPW = iReader(1)
                                UserLevel = iReader(2)
                                UserTest = iReader(2)
                                'MsgBox(iReader(2))
                            Else
                                ' UserTest = 1976
                            End If
                            'MsgBox(iReader(0))
                        Else
                            'UserTest = 1976
                        End If
                    End While
                End Using
            End Using

        Catch ex As Exception
            ' Do not turn a SQLite/native DLL load failure into the same return
            ' value as invalid credentials; let the click handler show its trace.
            Throw New InvalidOperationException("Échec de la vérification des identifiants dans SQLite. " &
                                                "Base utilisée : " & iConxStr &
                                                Environment.NewLine & "Requête : " & iStr, ex)
        End Try
        'MsgBox(UserTest)
        'UserTest = MayStatVal(iStr, iConxStr)
    End Function

    Private Sub txtUsername_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtUsername.TextChanged
        If txtUsername.Text <> "" Then
            tblUsername.Visibility = Visibility.Hidden
        Else
            tblUsername.Visibility = Visibility.Visible
        End If
        activBtnOk()
    End Sub
    Sub activBtnOk()
        If txtUsername.Text.Length >= 2 And txtPassword.Password.Length >= 2 Then
            btnLogin.IsEnabled = True
            btnLogin.IsDefault = True
        Else
            btnLogin.IsEnabled = False
            btnLogin.IsDefault = False
        End If
    End Sub

    Private Sub txtPassword_PasswordChanged(sender As Object, e As RoutedEventArgs) Handles txtPassword.PasswordChanged
        If txtPassword.Password <> "" Then
            tblPassword.Visibility = Visibility.Hidden
        Else
            tblPassword.Visibility = Visibility.Visible
        End If
        activBtnOk()
    End Sub
End Class
