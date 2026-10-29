Public Class Login
    Dim iConnxStr As String
    Private Async Sub btnLogin_Click(sender As Object, e As RoutedEventArgs) Handles btnLogin.Click
        btnLogin.IsEnabled = False
        grdLoginLoading.Visibility = Visibility.Visible
        Dim loadingStoryboard = TryCast(grdLoginLoading.Resources("LoginLoadingRotationStoryboard"),
                                        System.Windows.Media.Animation.Storyboard)
        loadingStoryboard?.Begin(grdLoginLoading, True)
        Await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background)

        Try
            Dim loginDatabasePath As String = DbPathGeneral
            iConnxStr = "Data Source=" & loginDatabasePath & ";Version=3;"
            Dim iLevel As Integer
            Dim generalDatabaseError As Exception = Nothing
            Dim localDatabaseErrors As New Collections.Generic.List(Of Exception)
            Dim localDatabaseChecked As Boolean = False

            If chkUseLocalMode.IsChecked.GetValueOrDefault() Then
                iLevel = TryLocalLogin(txtUsername.Text, loginDatabasePath, iConnxStr,
                                       localDatabaseChecked, localDatabaseErrors)
                If iLevel = 1976 Then
                    If localDatabaseErrors.Count > 0 Then
                        MessageBox.Show(
                            "La connexion hors ligne n'est pas disponible. Vérifiez votre installation ou contactez l'assistance.",
                            "Connexion impossible",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning)
                        Return
                    End If
                    Dim localMessage As String = If(localDatabaseChecked,
                                                    "Nom d'utilisateur ou mot de passe incorrect.",
                                                    "Aucune base hors ligne n'est disponible.")
                    MessageBox.Show(localMessage, "Connexion locale impossible",
                                    MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If
                If Not ConfirmLocalMode() Then Return
            Else
                Try
                    iLevel = UserTest(txtUsername.Text, txtPassword.Password, iConnxStr)
                Catch ex As Exception
                    Dim accessResult As Tuple(Of Boolean, String) =
                        CheckGeneralDatabaseAccessibility(loginDatabasePath)
                    If accessResult.Item1 Then
                        MessageBox.Show(
                            "La connexion n'a pas pu être vérifiée. Réessayez. Si le problème persiste, contactez l'assistance.",
                            "Échec de connexion",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error)
                        Return
                    End If

                    Dim alternativeDatabasePath As String = loginDatabasePath
                    If PromptForAlternativeGeneralDatabase(
                        loginDatabasePath,
                        alternativeDatabasePath) Then
                        loginDatabasePath = alternativeDatabasePath
                        iConnxStr = "Data Source=" & loginDatabasePath & ";Version=3;"
                        iLevel = UserTest(txtUsername.Text, txtPassword.Password, iConnxStr)
                    Else
                        generalDatabaseError = New InvalidOperationException(
                            "La base générale ne peut pas être utilisée pour la connexion." &
                            Environment.NewLine & "Base : " & loginDatabasePath &
                            Environment.NewLine & accessResult.Item2, ex)
                        iLevel = TryLocalLogin(txtUsername.Text, loginDatabasePath, iConnxStr,
                                               localDatabaseChecked, localDatabaseErrors)

                        If iLevel = 1976 AndAlso localDatabaseErrors.Count > 0 Then
                            MessageBox.Show(
                                "La base principale et la connexion hors ligne sont indisponibles. Contactez l'assistance.",
                                "Connexion impossible",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning)
                            Return
                        End If
                        If iLevel = 1976 AndAlso Not localDatabaseChecked Then
                            Dim errors As New Collections.Generic.List(Of Exception) From {generalDatabaseError}
                            Throw New InvalidOperationException("La base générale est indisponible et aucune base locale utilisable n'a pu être ouverte.",
                                                                New AggregateException(errors))
                        End If
                        If iLevel = 1976 Then
                            MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect.",
                                            "Connexion locale impossible", MessageBoxButton.OK, MessageBoxImage.Warning)
                            Return
                        End If
                        If MessageBox.Show(
                            "La base principale est indisponible." & Environment.NewLine &
                            "Voulez-vous continuer hors ligne ?" & Environment.NewLine &
                            "Les modifications hors ligne ne seront pas synchronisées automatiquement.",
                            "Connexion hors ligne",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning) <> MessageBoxResult.Yes Then Return
                    End If
                End Try
            End If

            'MsgBox(iLevel)
            If iLevel <> 1976 Then
                UserPath = IO.Path.Combine(RootPath, UserName)
                IO.Directory.CreateDirectory(UserPath)
                LocalModeBb = IsLocalDatabasePath(loginDatabasePath, txtUsername.Text)
                If LocalModeBb Then
                    DbPath = loginDatabasePath
                Else
                    DbPath = IO.Path.Combine(UserPath, "ManA.sqlite")
                End If
                DbManipulation.iConnectStr = $"Data Source={DbPath};Version=3;"

                'UserLevel = iLevel
                If Not LoadUserProfil(iLevel, iConnxStr) Then
                    MessageBox.Show("Votre profil n'a pas pu être chargé. Contactez l'assistance.",
                                    "Connexion impossible", MessageBoxButton.OK, MessageBoxImage.Error)
                    Me.Close()
                    Return
                End If

                If Not LocalModeBb AndAlso Not PrepareLocalDatabaseForConnectedMode(DbPath) Then Return

                If SessLogin(iConnxStr) Then
                    If LocalModeBb Then
                        MessageBox.Show("Vous travaillez sur la base locale ; les modifications ne seront pas synchronisées automatiquement.",
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
                    MessageBox.Show("La session n'a pas pu être ouverte. Vérifiez la disponibilité de la base et réessayez.",
                                    "Connexion impossible", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Else
                If MsgBox("Nom d'utilisateur ou mot de passe incorrect. Voulez-vous réessayer ?",
                          MsgBoxStyle.YesNo Or MsgBoxStyle.Exclamation,
                          "Connexion impossible") = MsgBoxResult.Yes Then
                    txtUsername.SelectAll()
                    txtUsername.Focus()
                Else
                    Me.Close()
                End If
            End If

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Échec de la connexion : " & ex.ToString())
            MessageBox.Show("La connexion n'a pas pu être terminée. Vérifiez la disponibilité de la base, puis réessayez.",
                            "Connexion impossible", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            loadingStoryboard?.Stop(grdLoginLoading)
            grdLoginLoading.Visibility = Visibility.Collapsed
            activBtnOk()
        End Try
    End Sub

    Private Function ConfirmLocalMode() As Boolean
        Return MessageBox.Show(
            "Vous avez choisi d'utiliser la base locale. Les modifications ne seront pas synchronisées automatiquement.",
            "Confirmer le mode local",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) = MessageBoxResult.Yes
    End Function

    Private Function TryLocalLogin(username As String,
                                   ByRef databasePath As String,
                                   ByRef connectionString As String,
                                   ByRef databaseChecked As Boolean,
                                   errors As Collections.Generic.List(Of Exception)) As Integer
        For Each localDatabasePath As String In GetLocalDatabaseCandidates(username)
            If Not IO.File.Exists(localDatabasePath) Then Continue For
            databaseChecked = True

            Try
                Dim schemaError As String = Nothing
                If Not IsLocalDatabaseSchemaUsable(localDatabasePath, schemaError) Then
                    errors.Add(New IO.InvalidDataException(
                        "Base locale incompatible (" & localDatabasePath & ") : " & schemaError))
                    Continue For
                End If

                Dim localConnectionString As String = "Data Source=" & localDatabasePath & ";Version=3;"
                Dim localLevel As Integer = UserTest(username, txtPassword.Password, localConnectionString)
                If localLevel <> 1976 Then
                    databasePath = localDatabasePath
                    connectionString = localConnectionString
                    Return localLevel
                End If
            Catch localEx As Exception
                errors.Add(localEx)
            End Try
        Next
        Return 1976
    End Function

    Private Function IsLocalDatabaseSchemaUsable(databasePath As String,
                                                 ByRef errorMessage As String) As Boolean
        errorMessage = ""
        Dim databaseError As String = Nothing
        If Not IsSQLiteDatabaseHealthy(databasePath, databaseError) Then
            errorMessage = databaseError
            Return False
        End If

        Dim requiredTables As String() = {
            "lada", "ladawet", "sesslog", "agents", "affectation",
            "siteaffectation", "contratstbl", "contratsnature", "client",
            "fonction", "rappels", "absences"
        }
        Try
            Dim connectionString As New Data.SQLite.SQLiteConnectionStringBuilder With {
                .DataSource = databasePath,
                .Version = 3,
                .ReadOnly = False
            }
            Using connection As New Data.SQLite.SQLiteConnection(connectionString.ToString())
                connection.Open()
                Using command As New Data.SQLite.SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table';", connection)
                    Using reader As Data.SQLite.SQLiteDataReader = command.ExecuteReader()
                        Dim availableTables As New Collections.Generic.HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                        While reader.Read()
                            availableTables.Add(Convert.ToString(reader("name")))
                        End While

                        Dim missingTables As String() = requiredTables.
                            Where(Function(tableName) Not availableTables.Contains(tableName)).
                            ToArray()
                        If missingTables.Length > 0 Then
                            errorMessage = "Tables manquantes : " & String.Join(", ", missingTables) & "."
                            Return False
                        End If
                    End Using
                End Using
            End Using
            Return True
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Impossible de vérifier la base hors ligne : " & ex.ToString())
            errorMessage = ex.Message
            Return False
        End Try
    End Function

    Private Function PrepareLocalDatabaseForConnectedMode(localDatabasePath As String) As Boolean
        If String.Equals(IO.Path.GetFullPath(localDatabasePath),
                         IO.Path.GetFullPath(DbPathGeneral),
                         StringComparison.OrdinalIgnoreCase) Then Return True

        Dim localDatabaseExists As Boolean = IO.File.Exists(localDatabasePath)
        If Not localDatabaseExists Then Return True

        Try
            Dim localDatabaseError As String = Nothing
            If Not IsSQLiteDatabaseHealthy(localDatabasePath, localDatabaseError) Then
                MessageBox.Show("Les données hors ligne n'ont pas pu être sécurisées. Elles ne seront pas remplacées. Contactez l'assistance.",
                                "Sauvegarde impossible", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End If

            Dim backupDirectory As String = IO.Path.Combine(UserPath, "Backups")
            IO.Directory.CreateDirectory(backupDirectory)
            Dim backupPath As String = IO.Path.Combine(
                backupDirectory,
                "ManA-local-" & DateTime.Now.ToString("yyyyMMdd-HHmmss") & "-" &
                Guid.NewGuid().ToString("N") & ".sqlite")
            CreateValidatedSQLiteSnapshot(localDatabasePath, backupPath)
            Return True
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Impossible de sauvegarder les données hors ligne : " & ex.ToString())
            MessageBox.Show("Les données hors ligne n'ont pas pu être sauvegardées. Elles ne seront pas remplacées. Contactez l'assistance.",
                            "Sauvegarde impossible", MessageBoxButton.OK, MessageBoxImage.Error)
            Return False
        End Try
    End Function

    Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        'Dim MyPath As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JPB\LKY")
        If Not IO.Directory.Exists(RootPath) Then IO.Directory.CreateDirectory(RootPath)
        txtUsername.Focus()
        Me.Title = "Login - LAKEY V" & My.Application.Info.Version.ToString

        Dim accessResult As Tuple(Of Boolean, String) =
            Await Threading.Tasks.Task.Run(
                Function() CheckGeneralDatabaseAccessibility(DbPathGeneral))

        If accessResult.Item1 Then Return

        Dim selectedDatabasePath As String = DbPathGeneral
        If PromptForAlternativeGeneralDatabase(DbPathGeneral, selectedDatabasePath) Then
            DbPathGeneral = selectedDatabasePath
            iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
            Return
        End If

        Dim localDatabasePath As String = Nothing
        Dim localDatabaseError As String = Nothing
        If FindUsableLocalDatabase(txtUsername.Text, localDatabasePath, localDatabaseError) Then
            Dim answer As MessageBoxResult = MessageBox.Show(
                Me,
                "La base principale est indisponible. Voulez-vous essayer en mode hors ligne ?" & Environment.NewLine &
                "Les modifications hors ligne ne seront pas synchronisées automatiquement.",
                "Connexion hors ligne",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No)
            If answer = MessageBoxResult.Yes Then chkUseLocalMode.IsChecked = True
        Else
            MessageBox.Show(
                "La base principale est indisponible et aucune base hors ligne n'a été trouvée." &
                Environment.NewLine & "Choisissez une autre base ou contactez l'assistance.",
                "Connexion impossible",
                MessageBoxButton.OK,
                MessageBoxImage.Error)
        End If
    End Sub

    Private Function FindUsableLocalDatabase(username As String,
                                             ByRef databasePath As String,
                                             ByRef errorMessage As String) As Boolean
        databasePath = Nothing
        errorMessage = ""
        Dim candidates As New Collections.Generic.List(Of String)(GetLocalDatabaseCandidates(username))

        If String.IsNullOrWhiteSpace(username) AndAlso IO.Directory.Exists(RootPath) Then
            Try
                For Each userDirectory As String In IO.Directory.GetDirectories(RootPath)
                    candidates.Add(IO.Path.Combine(userDirectory, "ManA.sqlite"))
                Next
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Impossible de rechercher une base hors ligne : " & ex.ToString())
                errorMessage = "Impossible de vérifier les bases locales : " & ex.Message
                Return False
            End Try
        End If

        Dim foundDatabase As Boolean = False
        For Each candidate As String In candidates.Distinct(StringComparer.OrdinalIgnoreCase)
            If Not IO.File.Exists(candidate) Then Continue For
            foundDatabase = True

            Dim schemaError As String = Nothing
            If IsLocalDatabaseSchemaUsable(candidate, schemaError) Then
                databasePath = candidate
                Return True
            End If

            errorMessage = "Base locale invalide (" & candidate & ") : " & schemaError
        Next

        If Not foundDatabase Then errorMessage = "Aucun fichier de base locale n'a été trouvé."
        Return False
    End Function

    Private Function PromptForAlternativeGeneralDatabase(currentDatabasePath As String,
                                                         ByRef selectedDatabasePath As String) As Boolean
        selectedDatabasePath = currentDatabasePath
        Dim answer As MessageBoxResult = MessageBox.Show(
            Me,
            "La base principale est indisponible. Voulez-vous en choisir une autre ?",
            "Base indisponible",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.Yes)
        If answer <> MessageBoxResult.Yes Then Return False

        Dim clientCode As String = Microsoft.VisualBasic.Interaction.InputBox(
            "Saisissez le code ID client pour choisir une autre base générale.",
            "Validation client",
            "")
        If clientCode <> "1176" Then
            If clientCode <> "" Then
                MessageBox.Show("Code incorrect.", "Accès refusé",
                                MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
            Return False
        End If

        Dim dialog As New Microsoft.Win32.OpenFileDialog With {
            .Title = "Sélectionnez une base générale LAKEY valide",
            .Filter = "Bases SQLite (*.sqlite;*.db)|*.sqlite;*.db|Tous les fichiers (*.*)|*.*",
            .CheckFileExists = True,
            .Multiselect = False
        }
        If dialog.ShowDialog(Me) <> True Then Return False

        Dim candidatePath As String = IO.Path.GetFullPath(dialog.FileName)
        Dim candidateAccessResult As Tuple(Of Boolean, String) =
            CheckGeneralDatabaseAccessibility(candidatePath)
        If Not candidateAccessResult.Item1 Then
            MessageBox.Show("Ce fichier ne peut pas être utilisé comme base. Choisissez un autre fichier.",
                            "Base invalide", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return False
        End If

        Try
            Using settingsConnection As New Data.SQLite.SQLiteConnection(
                "Data Source=C:\LkyRess\MySet.sqlite;Version=3;")
                settingsConnection.Open()
                Using command As New Data.SQLite.SQLiteCommand(
                    "UPDATE Sets SET DBpathG=@databasePath WHERE IdSet=1;", settingsConnection)
                    command.Parameters.AddWithValue("@databasePath", candidatePath)
                    If command.ExecuteNonQuery() <> 1 Then
                        Throw New InvalidOperationException("Le chemin de la base générale n'a pas pu être enregistré dans les paramètres.")
                    End If
                End Using
            End Using
            selectedDatabasePath = candidatePath
            DbPathGeneral = candidatePath
            Return True
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Impossible d'enregistrer la base sélectionnée : " & ex.ToString())
            MessageBox.Show("La base est valide, mais votre choix n'a pas pu être enregistré. Réessayez.",
                            "Enregistrement impossible", MessageBoxButton.OK, MessageBoxImage.Error)
            Return False
        End Try
    End Function

    Private Function CheckGeneralDatabaseAccessibility(databasePath As String) As Tuple(Of Boolean, String)
        If String.IsNullOrWhiteSpace(databasePath) OrElse Not IO.File.Exists(databasePath) Then
            Return Tuple.Create(False, "Le fichier de la base générale est introuvable.")
        End If

        Dim schemaError As String = Nothing
        If Not IsLocalDatabaseSchemaUsable(databasePath, schemaError) Then
            Return Tuple.Create(False, "La base ne contient pas un schéma LAKEY valide. " & schemaError)
        End If

        Try
            Dim connectionString As New Data.SQLite.SQLiteConnectionStringBuilder With {
                .DataSource = databasePath,
                .Version = 3,
                .ReadOnly = False,
                .DefaultTimeout = 10
            }
            Using connection As New Data.SQLite.SQLiteConnection(connectionString.ToString())
                connection.Open()
                Using command As New Data.SQLite.SQLiteCommand(
                    "SELECT UserName, UserPassWord, UserLever FROM lada LIMIT 0;", connection)
                    Using reader As Data.SQLite.SQLiteDataReader = command.ExecuteReader()
                    End Using
                End Using
                Using command As New Data.SQLite.SQLiteCommand(
                    "SELECT ProfilName FROM ladawet LIMIT 0;", connection)
                    Using reader As Data.SQLite.SQLiteDataReader = command.ExecuteReader()
                    End Using
                End Using
            End Using
            Return Tuple.Create(True, "")
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Impossible de vérifier la base générale : " & ex.ToString())
            Return Tuple.Create(False, "La base sélectionnée est inutilisable pour l'authentification. Détail : " & ex.Message)
        End Try
    End Function

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

    Shared Function UserTest(uName As String, uPssw As String, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Integer
        UserTest = 1976
        Dim iStr As String = "select l.UserName, l.UserPassWord, l.UserLever from lada l "
        'MsgBox(iConxStr)
        Try
            Dim authConnectionString As New Data.SQLite.SQLiteConnectionStringBuilder(iConxStr) With {
                .ReadOnly = False
            }
            Using iConx As New Data.SQLite.SQLiteConnection(authConnectionString.ToString())
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
                                                Environment.NewLine & "Requête : " & iStr &
                                                Environment.NewLine & "Détail SQLite : " & ex.Message, ex)
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
