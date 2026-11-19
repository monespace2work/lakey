Imports System.IO
Imports System.Diagnostics
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Xml.Serialization
Imports Drawing = System.Drawing
Imports Forms = System.Windows.Forms

Public NotInheritable Class IdBuder
    Private Const DefaultBackupDirectory As String = "C:\MyResq"
    Private Const MaximumCustomIntervalSeconds As Long = 2592000

    Private Shared ReadOnly SettingsPath As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "Lakey\idbuder.xml")
    Private Shared ReadOnly SettingsSerializer As New XmlSerializer(GetType(IdBuderSettings))
    Private Shared settings As IdBuderSettings
    Private Shared backupInterval As TimeSpan
    Private Shared sourcePathProvider As Func(Of String)
    Private Shared backupTimer As Timer
    Private Shared notifyIcon As Forms.NotifyIcon
    Private Shared backupInProgress As Integer

    Private Sub New()
    End Sub

    Public Shared Sub Start(getSourcePath As Func(Of String))
        If getSourcePath Is Nothing Then Throw New ArgumentNullException(NameOf(getSourcePath))
        If notifyIcon IsNot Nothing Then Return

        sourcePathProvider = getSourcePath
        settings = New IdBuderSettings()
        Dim settingsWarning As String = LoadSettings()
        settings.IntervalSeconds = GetIntervalSeconds(settings)
        settings.IntervalMinutes = CInt(Math.Ceiling(settings.IntervalSeconds / 60.0))
        backupInterval = TimeSpan.FromSeconds(settings.IntervalSeconds)
        CreateNotifyIcon()
        backupTimer = New Timer(AddressOf BackupTimer_Tick,
                                Nothing,
                                backupInterval,
                                backupInterval)

        If Not String.IsNullOrWhiteSpace(settingsWarning) Then
            ShowNotification("Réglages idbuder", settingsWarning, Forms.ToolTipIcon.Warning)
        End If
    End Sub

    Public Shared Sub [Stop]()
        If backupTimer IsNot Nothing Then
            backupTimer.Dispose()
            backupTimer = Nothing
        End If
        If notifyIcon IsNot Nothing Then
            notifyIcon.Visible = False
            notifyIcon.Dispose()
            notifyIcon = Nothing
        End If
        sourcePathProvider = Nothing
    End Sub

    Private Shared Function LoadSettings() As String
        If Not File.Exists(SettingsPath) Then Return Nothing

        Try
            Using settingsFile As FileStream = File.OpenRead(SettingsPath)
                Dim loadedSettings As IdBuderSettings =
                    TryCast(SettingsSerializer.Deserialize(settingsFile), IdBuderSettings)
                If loadedSettings Is Nothing Then Throw New InvalidDataException("Le fichier de réglages est vide.")
                loadedSettings.IntervalSeconds = GetIntervalSeconds(loadedSettings)
                loadedSettings.IntervalMinutes = CInt(Math.Ceiling(loadedSettings.IntervalSeconds / 60.0))
                If String.IsNullOrWhiteSpace(loadedSettings.BackupDirectory) Then
                    Throw New InvalidDataException("Le dossier de sauvegarde enregistré n'est pas valide.")
                End If
                settings = loadedSettings
            End Using
            Return Nothing
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Chargement des réglages idbuder impossible : " & ex.ToString())
            settings = New IdBuderSettings()
            Return "Les réglages enregistrés n'ont pas pu être lus. Les valeurs par défaut sont utilisées : " & ex.Message
        End Try
    End Function

    Private Shared Sub SaveSettings()
        Dim settingsDirectory As String = Path.GetDirectoryName(SettingsPath)
        Directory.CreateDirectory(settingsDirectory)
        Dim temporaryPath As String = SettingsPath & "." & Guid.NewGuid().ToString("N") & ".tmp"

        Try
            Using settingsFile As New FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                SettingsSerializer.Serialize(settingsFile, settings)
            End Using
            If File.Exists(SettingsPath) Then
                File.Replace(temporaryPath, SettingsPath, Nothing)
            Else
                File.Move(temporaryPath, SettingsPath)
            End If
        Finally
            If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
        End Try
    End Sub

    Private Shared Sub CreateNotifyIcon()
        Dim menu As New Forms.ContextMenuStrip()
        Dim scheduleMenu As New Forms.ToolStripMenuItem("Fréquence des sauvegardes")
        AddScheduleOption(scheduleMenu, "1 minute", 1)
        AddScheduleOption(scheduleMenu, "3 minutes", 3)
        AddScheduleOption(scheduleMenu, "5 minutes", 5)
        AddScheduleOption(scheduleMenu, "15 minutes", 15)
        AddScheduleOption(scheduleMenu, "30 minutes", 30)
        AddScheduleOption(scheduleMenu, "1 heure (par défaut)", 60)
        AddScheduleOption(scheduleMenu, "3 heures", 180)
        scheduleMenu.DropDownItems.Add(New Forms.ToolStripSeparator())
        Dim customInterval As New Forms.ToolStripMenuItem("Durée personnalisée...")
        AddHandler customInterval.Click, AddressOf CustomInterval_Click
        scheduleMenu.DropDownItems.Add(customInterval)

        Dim overwriteItem As New Forms.ToolStripMenuItem("Écraser le fichier précédent") With {
            .CheckOnClick = True,
            .Checked = settings.OverwritePrevious
        }
        AddHandler overwriteItem.CheckedChanged, AddressOf Overwrite_CheckedChanged

        Dim chooseFolderItem As New Forms.ToolStripMenuItem("Choisir le dossier de sauvegarde...")
        AddHandler chooseFolderItem.Click, AddressOf ChooseFolder_Click
        Dim openFolderItem As New Forms.ToolStripMenuItem("Ouvrir le dossier de sauvegarde")
        AddHandler openFolderItem.Click, AddressOf OpenFolder_Click
        Dim backupNowItem As New Forms.ToolStripMenuItem("Sauvegarder maintenant")
        AddHandler backupNowItem.Click, AddressOf BackupNow_Click

        menu.Items.Add(scheduleMenu)
        menu.Items.Add(overwriteItem)
        menu.Items.Add(New Forms.ToolStripSeparator())
        menu.Items.Add(chooseFolderItem)
        menu.Items.Add(openFolderItem)
        menu.Items.Add(New Forms.ToolStripSeparator())
        menu.Items.Add(backupNowItem)

        Dim appIcon As Drawing.Icon = Nothing
        Try
            appIcon = Drawing.Icon.ExtractAssociatedIcon(System.Windows.Application.ResourceAssembly.Location)
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("L'icône Lakey n'a pas pu être chargée pour idbuder : " & ex.Message)
        End Try

        notifyIcon = New Forms.NotifyIcon With {
            .Icon = If(appIcon, Drawing.SystemIcons.Application),
            .Text = "idbuder - sauvegarde automatique de la base générale",
            .ContextMenuStrip = menu,
            .Visible = True
        }
        AddHandler notifyIcon.DoubleClick, AddressOf BackupNow_Click
    End Sub

    Private Shared Sub AddScheduleOption(parent As Forms.ToolStripMenuItem, label As String, minutes As Integer)
        Dim optionItem As New Forms.ToolStripMenuItem(label) With {
            .CheckOnClick = True,
            .Checked = settings.IntervalSeconds = CLng(minutes) * 60,
            .Tag = CLng(minutes) * 60
        }
        AddHandler optionItem.Click, AddressOf ScheduleOption_Click
        parent.DropDownItems.Add(optionItem)
    End Sub

    Private Shared Sub ScheduleOption_Click(sender As Object, e As EventArgs)
        Dim optionItem As Forms.ToolStripMenuItem = DirectCast(sender, Forms.ToolStripMenuItem)
        SetInterval(CLng(optionItem.Tag))
    End Sub

    Private Shared Sub CustomInterval_Click(sender As Object, e As EventArgs)
        Dim currentInterval As TimeSpan = TimeSpan.FromSeconds(settings.IntervalSeconds)
        Dim dialog As New Forms.Form With {
            .Text = "Fréquence personnalisée",
            .FormBorderStyle = Forms.FormBorderStyle.FixedDialog,
            .StartPosition = Forms.FormStartPosition.CenterScreen,
            .ClientSize = New Drawing.Size(360, 170),
            .MaximizeBox = False,
            .MinimizeBox = False,
            .ShowInTaskbar = False
        }

        Dim hoursBox As New Forms.TextBox With {
            .Location = New Drawing.Point(120, 22),
            .Size = New Drawing.Size(80, 23),
            .MaxLength = 3,
            .TextAlign = Forms.HorizontalAlignment.Right,
            .Text = CInt(Math.Floor(currentInterval.TotalHours)).ToString(),
            .TabIndex = 0
        }
        Dim minutesBox As New Forms.TextBox With {
            .Location = New Drawing.Point(120, 57),
            .Size = New Drawing.Size(80, 23),
            .MaxLength = 2,
            .TextAlign = Forms.HorizontalAlignment.Right,
            .Text = currentInterval.Minutes.ToString(),
            .TabIndex = 1
        }
        Dim secondsBox As New Forms.TextBox With {
            .Location = New Drawing.Point(120, 92),
            .Size = New Drawing.Size(80, 23),
            .MaxLength = 2,
            .TextAlign = Forms.HorizontalAlignment.Right,
            .Text = currentInterval.Seconds.ToString(),
            .TabIndex = 2
        }
        dialog.Controls.Add(New Forms.Label With {
            .Text = "Heures (0 à 720) :",
            .Location = New Drawing.Point(16, 25),
            .AutoSize = True
        })
        dialog.Controls.Add(New Forms.Label With {
            .Text = "Minutes (0 à 59) :",
            .Location = New Drawing.Point(16, 60),
            .AutoSize = True
        })
        dialog.Controls.Add(New Forms.Label With {
            .Text = "Secondes (0 à 59) :",
            .Location = New Drawing.Point(16, 95),
            .AutoSize = True
        })
        dialog.Controls.Add(hoursBox)
        dialog.Controls.Add(minutesBox)
        dialog.Controls.Add(secondsBox)

        Dim validateButton As New Forms.Button With {
            .Text = "Valider",
            .Location = New Drawing.Point(172, 132),
            .Size = New Drawing.Size(80, 26),
            .TabIndex = 3
        }
        Dim cancelButton As New Forms.Button With {
            .Text = "Annuler",
            .Location = New Drawing.Point(260, 132),
            .Size = New Drawing.Size(80, 26),
            .DialogResult = Forms.DialogResult.Cancel,
            .TabIndex = 4
        }
        AddHandler validateButton.Click,
            Sub()
                Dim totalSeconds As Long
                If Not TryConvertInterval(hoursBox, minutesBox, secondsBox, totalSeconds) Then Return
                dialog.Tag = totalSeconds
                dialog.DialogResult = Forms.DialogResult.OK
                dialog.Close()
            End Sub
        dialog.Controls.Add(validateButton)
        dialog.Controls.Add(cancelButton)
        dialog.AcceptButton = validateButton
        dialog.CancelButton = cancelButton

        Using dialog
            If dialog.ShowDialog() = Forms.DialogResult.OK Then
                SetInterval(CLng(dialog.Tag))
            End If
        End Using
    End Sub

    Private Shared Function TryConvertInterval(hoursBox As Forms.TextBox,
                                               minutesBox As Forms.TextBox,
                                               secondsBox As Forms.TextBox,
                                               ByRef totalSeconds As Long) As Boolean
        totalSeconds = 0
        Dim hours As Integer
        Dim minutes As Integer
        Dim seconds As Integer
        If Not Integer.TryParse(If(String.IsNullOrWhiteSpace(hoursBox.Text), "0", hoursBox.Text), hours) OrElse
           Not Integer.TryParse(If(String.IsNullOrWhiteSpace(minutesBox.Text), "0", minutesBox.Text), minutes) OrElse
           Not Integer.TryParse(If(String.IsNullOrWhiteSpace(secondsBox.Text), "0", secondsBox.Text), seconds) OrElse
           hours < 0 OrElse minutes < 0 OrElse seconds < 0 Then
            Forms.MessageBox.Show("Saisissez uniquement des nombres entiers positifs ou nuls dans les trois champs.",
                                  "Fréquence invalide",
                                  Forms.MessageBoxButtons.OK,
                                  Forms.MessageBoxIcon.Warning)
            Return False
        End If
        If minutes > 59 OrElse seconds > 59 Then
            Forms.MessageBox.Show("Les minutes et les secondes doivent être comprises entre 0 et 59.",
                                  "Fréquence invalide",
                                  Forms.MessageBoxButtons.OK,
                                  Forms.MessageBoxIcon.Warning)
            Return False
        End If

        totalSeconds = CLng(hours) * 3600 + CLng(minutes) * 60 + seconds
        If totalSeconds < 1 OrElse totalSeconds > MaximumCustomIntervalSeconds Then
            Forms.MessageBox.Show("La durée totale doit être comprise entre 1 seconde et 30 jours.",
                                  "Fréquence invalide",
                                  Forms.MessageBoxButtons.OK,
                                  Forms.MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Private Shared Function GetIntervalSeconds(configuredSettings As IdBuderSettings) As Long
        Dim intervalSeconds As Long = configuredSettings.IntervalSeconds
        If intervalSeconds <= 0 Then
            If configuredSettings.IntervalMinutes < 1 OrElse configuredSettings.IntervalMinutes > 43200 Then
                Throw New InvalidDataException("La fréquence enregistrée n'est pas valide.")
            End If
            intervalSeconds = CLng(configuredSettings.IntervalMinutes) * 60
        End If
        If intervalSeconds > MaximumCustomIntervalSeconds Then
            Throw New InvalidDataException("La fréquence enregistrée dépasse 30 jours.")
        End If
        Return intervalSeconds
    End Function

    Private Shared Sub SetInterval(intervalSeconds As Long)
        Dim previousIntervalSeconds As Long = settings.IntervalSeconds
        Dim previousIntervalMinutes As Integer = settings.IntervalMinutes
        settings.IntervalSeconds = intervalSeconds
        settings.IntervalMinutes = CInt(Math.Ceiling(intervalSeconds / 60.0))
        Try
            SaveSettings()
            backupInterval = TimeSpan.FromSeconds(intervalSeconds)
            If backupTimer IsNot Nothing Then backupTimer.Change(backupInterval, backupInterval)
            UpdateScheduleChecks()
            ShowNotification("Fréquence modifiée",
                             "La prochaine sauvegarde sera effectuée dans " & FormatInterval(backupInterval) & ".",
                             Forms.ToolTipIcon.Info)
        Catch ex As Exception
            settings.IntervalSeconds = previousIntervalSeconds
            settings.IntervalMinutes = previousIntervalMinutes
            UpdateScheduleChecks()
            ShowNotification("Réglage non enregistré", ex.Message, Forms.ToolTipIcon.Error)
        End Try
    End Sub

    Private Shared Sub UpdateScheduleChecks()
        If notifyIcon Is Nothing OrElse notifyIcon.ContextMenuStrip Is Nothing Then Return
        Dim scheduleMenu As Forms.ToolStripMenuItem =
            TryCast(notifyIcon.ContextMenuStrip.Items(0), Forms.ToolStripMenuItem)
        If scheduleMenu Is Nothing Then Return
        For Each item As Forms.ToolStripItem In scheduleMenu.DropDownItems
            Dim optionItem As Forms.ToolStripMenuItem = TryCast(item, Forms.ToolStripMenuItem)
            If optionItem IsNot Nothing AndAlso TypeOf optionItem.Tag Is Long Then
                optionItem.Checked = CLng(optionItem.Tag) = settings.IntervalSeconds
            End If
        Next
    End Sub

    Private Shared Sub Overwrite_CheckedChanged(sender As Object, e As EventArgs)
        Dim menuItem As Forms.ToolStripMenuItem = DirectCast(sender, Forms.ToolStripMenuItem)
        Dim previousValue As Boolean = settings.OverwritePrevious
        settings.OverwritePrevious = menuItem.Checked
        Try
            SaveSettings()
        Catch ex As Exception
            settings.OverwritePrevious = previousValue
            menuItem.Checked = previousValue
            ShowNotification("Réglage non enregistré", ex.Message, Forms.ToolTipIcon.Error)
        End Try
    End Sub

    Private Shared Sub ChooseFolder_Click(sender As Object, e As EventArgs)
        Using dialog As New Forms.FolderBrowserDialog With {
            .Description = "Choisissez le dossier local des sauvegardes idbuder.",
            .SelectedPath = If(Directory.Exists(settings.BackupDirectory),
                               settings.BackupDirectory,
                               DefaultBackupDirectory),
            .ShowNewFolderButton = True
        }
            If dialog.ShowDialog() <> Forms.DialogResult.OK Then Return

            Dim previousDirectory As String = settings.BackupDirectory
            settings.BackupDirectory = dialog.SelectedPath
            Try
                SaveSettings()
                ShowNotification("Dossier modifié", settings.BackupDirectory, Forms.ToolTipIcon.Info)
            Catch ex As Exception
                settings.BackupDirectory = previousDirectory
                ShowNotification("Dossier non enregistré", ex.Message, Forms.ToolTipIcon.Error)
            End Try
        End Using
    End Sub

    Private Shared Sub OpenFolder_Click(sender As Object, e As EventArgs)
        Try
            Directory.CreateDirectory(settings.BackupDirectory)
            Process.Start(New ProcessStartInfo With {
                .FileName = settings.BackupDirectory,
                .UseShellExecute = True
            })
        Catch ex As Exception
            ShowNotification("Dossier inaccessible", ex.Message, Forms.ToolTipIcon.Error)
        End Try
    End Sub

    Private Shared Sub BackupNow_Click(sender As Object, e As EventArgs)
        BeginBackup()
    End Sub

    Private Shared Sub BackupTimer_Tick(state As Object)
        BeginBackup()
    End Sub

    Private Shared Sub BeginBackup()
        If Interlocked.CompareExchange(backupInProgress, 1, 0) <> 0 Then Return
        Dim getSourcePath As Func(Of String) = sourcePathProvider
        If getSourcePath Is Nothing Then
            Interlocked.Exchange(backupInProgress, 0)
            Return
        End If
        Dim destinationDirectory As String = settings.BackupDirectory
        Dim overwritePrevious As Boolean = settings.OverwritePrevious

        Task.Run(
            Sub()
                Try
                    Dim sourcePath As String = getSourcePath()
                    If String.IsNullOrWhiteSpace(sourcePath) OrElse Not File.Exists(sourcePath) Then
                        Throw New FileNotFoundException("La base générale de Lakey est introuvable.", sourcePath)
                    End If

                    Directory.CreateDirectory(destinationDirectory)
                    Dim fileName As String =
                        If(overwritePrevious,
                           "Lakey_General.sqlite",
                           "Lakey_General_" & DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") & ".sqlite")
                    Dim destinationPath As String = Path.Combine(destinationDirectory, fileName)
                    CreateValidatedSQLiteSnapshot(sourcePath, destinationPath)
                    ShowNotification("Sauvegarde terminée", destinationPath, Forms.ToolTipIcon.Info)
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine("La sauvegarde automatique idbuder a échoué : " & ex.ToString())
                    ShowNotification("Échec de la sauvegarde", ex.Message, Forms.ToolTipIcon.Error)
                Finally
                    Interlocked.Exchange(backupInProgress, 0)
                End Try
            End Sub)
    End Sub

    Private Shared Sub ShowNotification(title As String, message As String, icon As Forms.ToolTipIcon)
        Dim dispatcher As System.Windows.Threading.Dispatcher =
            If(System.Windows.Application.Current Is Nothing,
               Nothing,
               System.Windows.Application.Current.Dispatcher)
        If dispatcher IsNot Nothing AndAlso Not dispatcher.CheckAccess() Then
            dispatcher.BeginInvoke(New Action(Of String, String, Forms.ToolTipIcon)(AddressOf ShowNotification),
                                  title, message, icon)
            Return
        End If

        If notifyIcon IsNot Nothing Then
            notifyIcon.BalloonTipTitle = title
            notifyIcon.BalloonTipText = If(message.Length > 250, message.Substring(0, 247) & "...", message)
            notifyIcon.BalloonTipIcon = icon
            notifyIcon.ShowBalloonTip(5000)
        End If
    End Sub

    Private Shared Function FormatInterval(interval As TimeSpan) As String
        Return CInt(Math.Floor(interval.TotalHours)).ToString() & " h " &
               interval.Minutes.ToString() & " min " &
               interval.Seconds.ToString() & " s"
    End Function
End Class

Public Class IdBuderSettings
    Public Property IntervalMinutes As Integer = 60
    Public Property IntervalSeconds As Long
    Public Property BackupDirectory As String = "C:\MyResq"
    Public Property OverwritePrevious As Boolean = False
End Class
