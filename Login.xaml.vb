Public Class Login
    Dim iConnxStr As String
    Private Sub btnLogin_Click(sender As Object, e As RoutedEventArgs) Handles btnLogin.Click
        Try
            Dim loginDatabasePath As String = DbPathGeneral
            Dim iLevel As Integer
            Dim generalDatabaseError As Exception = Nothing
            Try
                iLevel = UserTest(txtUsername.Text, txtPassword.Password, iConnxStr)
            Catch ex As Exception
                generalDatabaseError = ex
                iLevel = 1976

                Dim localDatabaseChecked As Boolean = False
                Dim localDatabaseErrors As New Collections.Generic.List(Of Exception)
                For Each localDatabasePath As String In GetLocalDatabaseCandidates(txtUsername.Text)
                    If Not IO.File.Exists(localDatabasePath) Then Continue For

                    Try
                        Dim localConnectionString As String = "Data Source=" & localDatabasePath & ";Version=3;"
                        iLevel = UserTest(txtUsername.Text, txtPassword.Password, localConnectionString)
                        localDatabaseChecked = True
                        If iLevel <> 1976 Then
                            loginDatabasePath = localDatabasePath
                            iConnxStr = localConnectionString
                            Exit For
                        End If
                    Catch localEx As Exception
                        localDatabaseErrors.Add(localEx)
                    End Try
                Next

                If iLevel = 1976 AndAlso localDatabaseChecked Then
                    MessageBox.Show("La base générale est indisponible et ces identifiants ne sont pas présents dans la base locale.",
                                    "Connexion locale impossible", MessageBoxButton.OK, MessageBoxImage.Warning)
                ElseIf iLevel = 1976 Then
                    Dim errors As New Collections.Generic.List(Of Exception) From {generalDatabaseError}
                    errors.AddRange(localDatabaseErrors)
                    Throw New InvalidOperationException("La base générale est indisponible et aucune base locale utilisable n'a pu être ouverte.",
                                                        New AggregateException(errors))
                End If
            End Try

            'MsgBox(iLevel)
            If iLevel <> 1976 Then
                UserPath = IO.Path.Combine(RootPath, UserName)
                IO.Directory.CreateDirectory(UserPath)
                LocalModeBb = IsLocalDatabasePath(loginDatabasePath, txtUsername.Text)
                If LocalModeBb Then
                    DbPath = loginDatabasePath
                Else
                    DbPath = IO.Path.Combine(UserPath, IO.Path.GetFileName(DbPathGeneral))
                End If

                'UserLevel = iLevel
                If Not LoadUserProfil(iLevel, iConnxStr) Then
                    MessageBox.Show("Erreur de chargement du profil utilisateur", "ERREUR CRITIQUE", MessageBoxButton.OK, MessageBoxImage.Error)
                    Me.Close()
                    Return
                End If

                If SessLogin(iConnxStr) Then
                    If LocalModeBb Then
                        MessageBox.Show("La base générale n'est pas accessible. Vous travaillez sur la base locale ; les modifications ne seront pas synchronisées automatiquement.",
                                        "Mode local", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
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

    Private Function GetLocalDatabaseCandidates(username As String) As Collections.Generic.List(Of String)
        Dim candidates As New Collections.Generic.List(Of String)
        Dim safeUsername As String = username.Trim()
        If safeUsername <> "" AndAlso
           String.Equals(IO.Path.GetFileName(safeUsername), safeUsername, StringComparison.Ordinal) AndAlso
           safeUsername <> "." AndAlso safeUsername <> ".." Then
            candidates.Add(IO.Path.Combine(RootPath, safeUsername, "ManA.sqlite"))
        End If
        candidates.Add("C:\LkyRess\ManA.sqlite")
        Return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
    End Function

    Private Function IsLocalDatabasePath(databasePath As String, username As String) As Boolean
        Return GetLocalDatabaseCandidates(username).Any(
            Function(localPath) String.Equals(IO.Path.GetFullPath(databasePath),
                                                IO.Path.GetFullPath(localPath),
                                                StringComparison.OrdinalIgnoreCase))
    End Function

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
