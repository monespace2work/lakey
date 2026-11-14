Imports System.Windows.Threading
Imports System.Data
Imports System.Data.SQLite
Public Class usercClientsContrat
    Public iConnxStr As String
    Dim LocalConnxStr As String
    Dim ChangeYN As Boolean = False
    Dim iActif As Boolean = True
    Dim iInActif As Boolean = False
    Dim AffActif As Boolean = True
    Dim RefCrt As Integer = 0
    Dim inputTimer As DispatcherTimer
    Dim iFormLoaded As Boolean = False
    Dim iCbxClientName As New ComboBox
    Dim NewData As Boolean = True
    Dim NewNom As String = ""
    Dim retourDansMainWindow As Action
    Public Shared NomClientTemp As String
    Public Shared AfficheStat As Action
    Public Sub New(clb As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = clb
    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        LocalConnxStr = $"Data Source={DbPath};Version=3;"
        iConnxStr = LocalConnxStr

        InialiseLoad()
        ChangeYN = False
        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(3)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        inputTimer.Stop()
        Dispatcher.Invoke(Sub() SearchData(BtnDataSearch.Text))
    End Sub

    Sub InialiseLoad()
        elpStatus.Visibility = Visibility.Visible
        maj()
        Dim istr As String = " SELECT c.NomClient, c.Actif,c.TypeClient, c.MontantFacturationUnitaire as MontantUnit, c.TypeFacturation,c.Ville,c.Adresse, format(c.DateDebutCollabor) as DateDebut,format(c.DateFinCollabor) as DateFin,c.DescriptionClient,c.Obs,c.RefCtr "
        istr += " from Client c "
        istr += " where c.NomClient like '%' and c.Actif=1 "
        istr += " order by c.Actif desc, c.NomClient "
        DgClientsContrats.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbClientContratActifs.Text = "Nombre de clients actifs: " & DirectCast(DgClientsContrats.ItemsSource, System.Data.DataView).Count & " | Nombre de clients inactifs: " & MayStatVal("select count(*) from Client where Actif=0 ", LocalConnxStr)
        AfficheStat?.Invoke()

        FilmyCbx("select lower(c.NomClient) from client c", iCbxClientName)
        FilmyCbx("select c.TypeClient from ClientType c where c.Actif=1  order by c.Favorit desc,c.TypeClient", CbxTypeClient)
        FilmyCbx("select c.TypeFacutration from ClientTypeFacturation c where c.Actif=1 order by c.Favorit desc,c.TypeFacutration", CbxTyptFactur)
        RefCrt = MayStatVal("select max(c.RefCtr) from client c", iConnxStr) + 1
        dtpDebut.SelectedDate = Today
        dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))
        'MsgBox(RefCrt)
        dtpDebutSearch.SelectedDate = DateCreationSociete 'CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = Today
        iFormLoaded = True
        ChangeYN = False
        NewData = True
        btnFinCollab.Visibility = Visibility.Collapsed
        btnReActivClient.Visibility = Visibility.Collapsed
        elpStatus.Visibility = Visibility.Collapsed
        AfficheProfilUser()
    End Sub

    Sub ResetValues()
        elpStatus.Visibility = Visibility.Visible

        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed
        AfficheProfilUser()

        AfficheStat?.Invoke()
        Dim istr As String = " SELECT c.NomClient, c.Actif,c.TypeClient, c.MontantFacturationUnitaire as MontantUnit, c.TypeFacturation,c.Ville,c.Adresse, format(c.DateDebutCollabor) as DateDebut,format(c.DateFinCollabor) as DateFin,c.DescriptionClient,c.Obs,c.RefCtr "
        istr += " from Client c "
        istr += " where c.NomClient like '%' and c.Actif=1 "
        istr += " order by c.Actif desc, c.NomClient "
        DgClientsContrats.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbClientContratActifs.Text = "Nombre de clients actifs: " & DirectCast(DgClientsContrats.ItemsSource, System.Data.DataView).Count & " | Nombre de clients inactifs: " & MayStatVal("select count(*) from Client where Actif=0 ", LocalConnxStr)

        FilmyCbx("select lower(c.NomClient) from client c", iCbxClientName)
        RefCrt = MayStatVal("select max(c.RefCtr) from client c", iConnxStr) + 1
        'MsgBox(RefCrt)

        dtpDebut.SelectedDate = Today
        'dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        TbxNom.Text = ""
        TbxDescript.Text = ""
        TbxAdresse.Text = ""
        TbxVille.Text = ""
        TbxFin.Text = ""
        TbxMontant.Text = ""
        'TbxNom.Text = ""
        TbxObs.Text = ""
        CbxTypeClient.SelectedIndex = -1
        CbxTyptFactur.SelectedIndex = -1
        'CbxNumMatr.SelectedIndex = -1
        'CbxTypCrt.SelectedIndex = -1


        iFormLoaded = False
        dtpDebutSearch.SelectedDate = DateCreationSociete 'CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = Today
        BtnDataSearch.Text = ""
        iFormLoaded = True

        ChangeYN = False
        CltActif.IsChecked = True
        CltInActif.IsChecked = False
        NewData = True
        btnFinCollab.Visibility = Visibility.Collapsed
        btnReActivClient.Visibility = Visibility.Collapsed
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub SearchData(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        Dim sufix As String = ""
        If KeyWord = "" Then
            'sufix = " and (c.Actif=1 and crt.FinContr>='" & Today.ToString("yyyy-MM-dd") & "')"
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                sufix += " and DateDebut>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateDebut<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "'  "
            Else
                sufix += " and DateDebut>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "' and DateDebut<='" & Today.ToString("yyyy-MM-dd") & "'  "
            End If
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = " "
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                sufix += " and DateDebut>='" & DateCreationSociete.ToString("yyyy-MM-dd") & "' and DateDebut<='" & Today.ToString("yyyy-MM-dd") & "'  "
            Else
                sufix += " and DateDebut>='" & DateCreationSociete.ToString("yyyy-MM-dd") & "' and DateDebut<='" & Today.ToString("yyyy-MM-dd") & "'  "
            End If
        Else
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                sufix += " and DateDebut>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateDebut<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "'  "
            Else
                sufix += " and DateDebut>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "' and DateDebut<='" & Today.ToString("yyyy-MM-dd") & "'  "
            End If
        End If
        Dim istr As String = "SELECT c.NomClient, c.Actif,c.TypeClient, c.MontantFacturationUnitaire as MontantUnit, c.TypeFacturation,c.Ville,c.Adresse, format(c.DateDebutCollabor) as DateDebut,format(c.DateFinCollabor) as DateFin,c.DescriptionClient,c.Obs,c.RefCtr  "
        istr += " from Client c "
        istr += " where "
        istr += " (c.NomClient like '%" & KeyWord & "%' "
        istr += " or c.TypeClient like '%" & KeyWord & "%' "
        istr += " or c.Ville like '%" & KeyWord & "%' "
        istr += " or c.Adresse like '%" & KeyWord & "%' "
        istr += " or c.DescriptionClient like '%" & KeyWord & "%' "
        istr += " or c.TypeFacturation  like '%" & KeyWord & "%' )" & sufix
        If AffActif Then
            If iActif And Not iInActif Then
                istr += " and c.Actif=1"
            ElseIf Not iActif And iInActif Then
                istr += " and c.Actif=0"
            Else
                istr += " and c.Actif=2"
            End If

        End If
        istr += " ORDER BY c.Actif desc,c.NomClient "
        'ShowTempMsg(istr)
        maj()
        Dim searchResults = LoadDataTable(istr, LocalConnxStr)
        DgClientsContrats.ItemsSource = searchResults.DefaultView
        Dim activeCount As Integer = searchResults.Select("Actif = 1").Length
        Dim inactiveCount As Integer = searchResults.Select("Actif = 0").Length
        Dim itext As String = "Nombre de clients actifs : " & activeCount & " | Nombre de clients inactifs: " & inactiveCount
        itext += Environment.NewLine
        tblNbClientContratActifs.Text = itext '"En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")

        NewData = True
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Function SaveNewContrat() As Boolean
        SaveNewContrat = False
        Dim istr As String = " insert into Client "
        istr += "(NomClient, Actif,TypeClient, MontantFacturationUnitaire, TypeFacturation,Ville,Adresse, DateDebutCollabor,DescriptionClient,Obs,RefCtr)  "
        istr += " values ('" & Trim(Apost(TbxNom.Text)) & "', "
        istr += "1, " '& ChbActif.IsChecked & ", "
        istr += " '" & Apost(CbxTypeClient.SelectedItem) & "', "
        istr += " " & Trim(TbxMontant.Text) & ", "
        istr += " '" & Apost(CbxTyptFactur.SelectedItem) & "', "
        istr += " '" & Trim(Apost(TbxVille.Text)) & "', "
        istr += " '" & Trim(Apost(TbxAdresse.Text)) & "', "
        istr += " '" & (dtpDebut.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        istr += " '" & Trim(Apost(TbxDescript.Text)) & "', "
        istr += " '" & Trim(Apost(TbxObs.Text)) & "', "
        istr += " " & RefCrt & ")"

        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveNewContrat = SaveNewData(istr, iConnxStr)
        If SaveNewContrat Then Joper("SaveNewClient for " & TbxNom.Text & " at " & RefCrt, iConnxStr)
    End Function

    Private Async Sub btnImportCsv_Click(sender As Object, e As RoutedEventArgs) Handles btnImportCsv.Click
        Dim dialog As New Microsoft.Win32.OpenFileDialog With {
            .Title = "Importer des contrats clients depuis un fichier CSV",
            .Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
            .CheckFileExists = True,
            .Multiselect = False
        }
        If dialog.ShowDialog() <> True Then Return

        Try
            Dim table As DataTable = Nothing
            Dim hasHeader As Boolean = False
            Dim noHeaderColumns As String() = {
                "NomClient", "TypeClient", "MontantFacturationUnitaire", "TypeFacturation",
                "Ville", "Adresse", "DateDebutCollabor", "DescriptionClient", "Obs"
            }
            Try
                table = ReadCsvTable(dialog.FileName)
                hasHeader =
                    CsvHasColumn(table, "NomClient") AndAlso
                    CsvHasColumn(table, "TypeClient") AndAlso
                    CsvHasColumn(table, "MontantFacturationUnitaire") AndAlso
                    CsvHasColumn(table, "TypeFacturation") AndAlso
                    CsvHasColumn(table, "Ville") AndAlso
                    CsvHasColumn(table, "Adresse") AndAlso
                    CsvHasColumn(table, "DateDebutCollabor")
            Catch ex As FormatException
                ' Un CSV sans en-tête peut être interprété comme des en-têtes invalides.
            End Try
            If Not hasHeader Then table = ReadCsvTable(dialog.FileName, noHeaderColumns, ";")

            If Not CsvHasColumn(table, "NomClient") OrElse
               Not CsvHasColumn(table, "TypeClient") OrElse
               Not CsvHasColumn(table, "MontantFacturationUnitaire") OrElse
               Not CsvHasColumn(table, "TypeFacturation") OrElse
               Not CsvHasColumn(table, "Ville") OrElse
               Not CsvHasColumn(table, "Adresse") OrElse
               Not CsvHasColumn(table, "DateDebutCollabor") Then
                Throw New FormatException("Colonnes obligatoires : NomClient, TypeClient, MontantFacturationUnitaire, TypeFacturation, Ville, Adresse et DateDebutCollabor.")
            End If
            If table.Rows.Count = 0 Then
                MessageBox.Show("Le fichier CSV ne contient aucune donnée à importer.", "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If
            If MessageBox.Show(
                "Importer " & table.Rows.Count & " contrat(s) client ? Toute l'opération sera annulée si une ligne est invalide ou en doublon.",
                "Confirmation de l'import CSV",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) <> MessageBoxResult.Yes Then Return

            elpStatus.Visibility = Visibility.Visible
            Await Task.Run(Sub() ImportClientContratsCsv(table, hasHeader))
            If Not LocalModeBb Then NotifyGeneralDatabaseChanged()
            Await RefreshAfterDatabaseMutationAsync(Sub() ResetValues())
            MessageBox.Show(table.Rows.Count & " contrat(s) client importé(s) avec succès.", "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            MessageBox.Show("L'import CSV des contrats clients a échoué : " & ex.Message, "Import CSV", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            elpStatus.Visibility = Visibility.Collapsed
        End Try
    End Sub

    Private Sub ImportClientContratsCsv(table As DataTable, hasHeader As Boolean)
        Using connection As New SQLiteConnection(ResolveMainDatabaseConnectionString(iConnxStr, True))
            connection.Open()
            Using transaction As SQLiteTransaction = connection.BeginTransaction()
                Dim nextId As Integer
                Using command As New SQLiteCommand("SELECT COALESCE(MAX(RefCtr), 0) + 1 FROM Client", connection, transaction)
                    nextId = Convert.ToInt32(command.ExecuteScalar())
                End Using

                Dim existingClients As Dictionary(Of String, String) =
                    LoadCsvLookup(connection, transaction, "SELECT NomClient FROM Client", "NomClient", "NomClient")
                Dim activeClientTypes As Dictionary(Of String, String) =
                    LoadCsvLookup(connection, transaction, "SELECT TypeClient FROM ClientType WHERE Actif = 1", "TypeClient", "TypeClient")
                Dim activeBillingTypes As Dictionary(Of String, String) =
                    LoadCsvLookup(connection, transaction, "SELECT TypeFacutration FROM ClientTypeFacturation WHERE Actif = 1", "TypeFacutration", "TypeFacutration")
                Dim importedNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

                For index As Integer = 0 To table.Rows.Count - 1
                    Dim row As DataRow = table.Rows(index)
                    Dim rowNumber As Integer = index + If(hasHeader, 2, 1)
                    Dim clientName As String = CsvValue(row, "NomClient")
                    Dim clientType As String = CsvValue(row, "TypeClient")
                    Dim billingType As String = CsvValue(row, "TypeFacturation")
                    Dim city As String = CsvValue(row, "Ville")
                    Dim address As String = CsvValue(row, "Adresse")
                    Dim amountText As String = CsvValue(row, "MontantFacturationUnitaire")
                    Dim startDate As DateTime = CsvDateValue(CsvValue(row, "DateDebutCollabor"), rowNumber, "DateDebutCollabor")
                    Dim description As String = CsvValue(row, "DescriptionClient")
                    Dim observations As String = CsvValue(row, "Obs")

                    If clientName.Length <= 1 OrElse city.Length <= 1 OrElse address.Length <= 1 OrElse
                       String.IsNullOrWhiteSpace(clientType) OrElse String.IsNullOrWhiteSpace(billingType) Then
                        Throw New FormatException("Ligne " & rowNumber & " : nom, type, type de facturation, ville et adresse sont obligatoires.")
                    End If

                    Dim amount As Integer
                    If Not Integer.TryParse(amountText, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, amount) OrElse amount <= 5000 Then
                        Throw New FormatException("Ligne " & rowNumber & " : le montant doit être un nombre entier supérieur à 5000.")
                    End If

                    Dim canonicalClientType As String = Nothing
                    If Not activeClientTypes.TryGetValue(clientType, canonicalClientType) Then
                        Throw New InvalidOperationException("Ligne " & rowNumber & " : type de client inactif ou inconnu (« " & clientType & " »).")
                    End If
                    Dim canonicalBillingType As String = Nothing
                    If Not activeBillingTypes.TryGetValue(billingType, canonicalBillingType) Then
                        Throw New InvalidOperationException("Ligne " & rowNumber & " : type de facturation inactif ou inconnu (« " & billingType & " »).")
                    End If
                    If existingClients.ContainsKey(clientName) OrElse Not importedNames.Add(clientName) Then
                        Throw New InvalidOperationException("Ligne " & rowNumber & " : le client « " & clientName & " » existe déjà.")
                    End If

                    Using command As New SQLiteCommand(
                        "INSERT INTO Client (NomClient, Actif, TypeClient, MontantFacturationUnitaire, TypeFacturation, Ville, Adresse, DateDebutCollabor, DescriptionClient, Obs, RefCtr) " &
                        "VALUES (@name, 1, @type, @amount, @billingType, @city, @address, @start, @description, @observations, @id)",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@name", clientName)
                        command.Parameters.AddWithValue("@type", canonicalClientType)
                        command.Parameters.AddWithValue("@amount", amount)
                        command.Parameters.AddWithValue("@billingType", canonicalBillingType)
                        command.Parameters.AddWithValue("@city", city)
                        command.Parameters.AddWithValue("@address", address)
                        command.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd"))
                        command.Parameters.AddWithValue("@description", description)
                        command.Parameters.AddWithValue("@observations", observations)
                        command.Parameters.AddWithValue("@id", nextId)
                        command.ExecuteNonQuery()
                    End Using
                    Using command As New SQLiteCommand("INSERT INTO jope (operation, operateur) VALUES (@operation, @operator)", connection, transaction)
                        command.Parameters.AddWithValue("@operation", "ImportCsvClient for " & clientName)
                        command.Parameters.AddWithValue("@operator", UserName)
                        command.ExecuteNonQuery()
                    End Using
                    nextId += 1
                Next

                transaction.Commit()
            End Using
        End Using
    End Sub

    Function SaveModifCtr() As Boolean
        SaveModifCtr = False
        Dim istr As String = " update Client  set "
        'istr += "(IdHS, Agent, SiteHS, DateHS, NbFaction, Obs) values "
        istr += " NomClient='" & Trim(Apost(TbxNom.Text)) & "', "
        istr += " Actif=1," ' & ChbActif.IsChecked & ", "
        istr += " TypeClient='" & Apost(CbxTypeClient.SelectedItem) & "', "
        istr += " MontantFacturationUnitaire=" & Trim(TbxMontant.Text) & ", "
        istr += " TypeFacturation='" & Apost(CbxTyptFactur.SelectedItem) & "', "
        istr += " Ville='" & Trim(Apost(TbxVille.Text)) & "', "
        istr += " Adresse='" & Trim(Apost(TbxAdresse.Text)) & "', "
        istr += " DateDebutCollabor='" & (dtpDebut.SelectedDate.Value).ToString("yyyy-MM-dd") & "', " 'crt.FinContr
        If IsDate(TbxFin.Text) Then istr += " DateFinCollabor='" & CDate(TbxFin.Text).ToString("yyyy-MM-dd") & "', " 'crt.FinContr
        istr += " DescriptionClient='" & Trim(Apost(TbxDescript.Text)) & "', "
        istr += " Obs='" & Trim(Apost(TbxObs.Text)) & "'  "
        istr += " where RefCtr=" & RefCrt & " "
        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveModifCtr = SaveClientModification(istr)
        If SaveModifCtr Then Joper("SaveModifCtrClient for " & TbxNom.Text & " at " & RefCrt, iConnxStr)
    End Function

    Private Function SaveClientModification(clientUpdate As String) As Boolean
        Try
            Dim writeConnectionString As String = ResolveMainDatabaseConnectionString(iConnxStr, True)
            Using connection As New SQLiteConnection(writeConnectionString)
                connection.Open()
                Using transaction As SQLiteTransaction = connection.BeginTransaction()
                    Using command As New SQLiteCommand("PRAGMA defer_foreign_keys = ON", connection, transaction)
                        command.ExecuteNonQuery()
                    End Using

                    Using command As New SQLiteCommand(clientUpdate, connection, transaction)
                        command.ExecuteNonQuery()
                    End Using

                    If NewNom <> TbxNom.Text Then
                        Dim siteUpdate As String = "update SiteAffectation set Client='" & Trim(Apost(TbxNom.Text)) & "' where Client='" & Apost(NewNom) & "'"
                        Using command As New SQLiteCommand(siteUpdate, connection, transaction)
                            command.ExecuteNonQuery()
                        End Using
                    End If

                    transaction.Commit()
                End Using
            End Using
            If Not LocalModeBb Then NotifyGeneralDatabaseChanged()
            Return True
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try
    End Function

    Private Sub BtnDataSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnDataSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()
    End Sub

    Private Sub BtnDataSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnDataSearch.KeyDown
        If e.Key = Key.Enter Then
            inputTimer.Stop()
            SearchData(BtnDataSearch.Text)
        End If
    End Sub

    Private Sub btnNewData_Click(sender As Object, e As RoutedEventArgs) Handles btnNewData.Click
        stpSaisie.Visibility = Visibility.Visible
        btnNewData.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnAnnule_Click(sender As Object, e As RoutedEventArgs) Handles btnAnnule.Click
        If ChangeYN Then
            If MsgBox("ANNULER VRAIMENT ?", MsgBoxStyle.OkCancel, "ATTENTION") = MsgBoxResult.Ok Then
                ResetValues()
                stpSaisie.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
            End If
        Else
            stpSaisie.Visibility = Visibility.Collapsed
            btnNewData.Visibility = Visibility.Visible

        End If
        btnDeleteAg.Visibility = Visibility.Collapsed

    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then
            If BtnDataSearch.Text = "" Then
                SearchData("*")
            Else
                SearchData(BtnDataSearch.Text)
            End If
        End If
    End Sub
    Sub ActivSaveBtn()
        If TbxMontant.Text.Length > 3 Then
            If NewData Then
                If TbxNom.Text.Length > 1 And RefCrt > 0 And Not iCbxClientName.Items.Contains(LCase(TbxNom.Text)) And TbxAdresse.Text.Length > 1 And TbxVille.Text.Length > 1 And CbxTypeClient.SelectedIndex >= 0 And CbxTyptFactur.SelectedIndex >= 0 And TbxAdresse.Text.Length > 1 And dtpDebut.SelectedDate.HasValue And CInt(TbxMontant.Text) > 5000 Then ' AndAlso cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
                    btnSave.Visibility = Visibility.Visible
                    btnSaveContinue.Visibility = Visibility.Visible
                Else
                    btnSave.Visibility = Visibility.Collapsed
                    btnSaveContinue.Visibility = Visibility.Collapsed
                End If
            ElseIf NewNom <> TbxNom.Text Then

                If TbxNom.Text.Length > 1 And RefCrt > 0 And (Not iCbxClientName.Items.Contains(LCase(TbxNom.Text)) Or NewNom = TbxNom.Text) And TbxAdresse.Text.Length > 1 And TbxVille.Text.Length > 1 And CbxTypeClient.SelectedIndex >= 0 And CbxTyptFactur.SelectedIndex >= 0 And TbxAdresse.Text.Length > 1 And dtpDebut.SelectedDate.HasValue And CInt(TbxMontant.Text) > 5000 Then ' AndAlso cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
                    btnSave.Visibility = Visibility.Visible
                    btnSaveContinue.Visibility = Visibility.Visible
                Else
                    btnSave.Visibility = Visibility.Collapsed
                    btnSaveContinue.Visibility = Visibility.Collapsed
                End If

            Else
                If TbxNom.Text.Length > 1 And RefCrt > 0 And TbxAdresse.Text.Length > 1 And TbxVille.Text.Length > 1 And CbxTypeClient.SelectedIndex >= 0 And CbxTyptFactur.SelectedIndex >= 0 And TbxAdresse.Text.Length > 1 And dtpDebut.SelectedDate.HasValue And CInt(TbxMontant.Text) > 5000 Then ' AndAlso cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
                    btnSave.Visibility = Visibility.Visible
                    btnSaveContinue.Visibility = Visibility.Visible
                Else
                    btnSave.Visibility = Visibility.Collapsed
                    btnSaveContinue.Visibility = Visibility.Collapsed
                End If

            End If
        Else
            btnSave.Visibility = Visibility.Collapsed
            btnSaveContinue.Visibility = Visibility.Collapsed
        End If
        ChangeYN = True

    End Sub
    Private Sub TbxNom_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxNom.TextChanged
        SaisiTbx(sender, TblIndicationNom)
        ActivSaveBtn()
    End Sub

    Private Sub TbxDescript_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxDescript.TextChanged
        'SaisiTbx(sender, TblIndication)
        ActivSaveBtn()
    End Sub

    Private Sub TbxAdresse_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxAdresse.TextChanged
        'SaisiTbx(sender, TblIndication)
        SaisiePremiereLettreMajusculeTbx(sender)
        ActivSaveBtn()
    End Sub

    Private Sub TbxVille_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxVille.TextChanged
        'SaisiTbx(sender, TblIndication)
        SaisiePremiereLettreMajusculeTbx(sender)
        ActivSaveBtn()
    End Sub

    Private Sub CbxTypeClient_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxTypeClient.SelectionChanged
        SaisiCbx(sender, TblIndicationTypeClient)
        ActivSaveBtn()
    End Sub

    Private Sub CbxTyptFactur_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxTyptFactur.SelectionChanged
        SaisiCbx(sender, TblIndicationCbxTyptFactur)
        ActivSaveBtn()
    End Sub

    Private Sub TbxMontant_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxMontant.TextChanged
        SaisiTbx(sender, TblIndicationTbxMontant)
        ActivSaveBtn()
    End Sub

    Private Sub dtpDebut_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebut.SelectedDateChanged
        'SaisiTbx(sender, TblIndicationCbxTyptFactur)
        ActivSaveBtn()
    End Sub

    Private Sub TbxObs_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxObs.TextChanged
        SaisiTbx(sender, TblIndicationObs)
        ActivSaveBtn()
    End Sub

    Private Sub TbxMontant_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles TbxMontant.PreviewKeyDown
        If e.Key = Key.Back OrElse e.Key = Key.Delete OrElse e.Key = Key.Left OrElse e.Key = Key.Right Then
            e.Handled = False ' Autoriser ces touches
        End If

    End Sub

    Private Sub TbxMontant_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxMontant.PreviewTextInput
        If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If

    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDebut.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDebut.Focus()
                Exit Sub
            End If
        End If
        If NewData Then
            If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Try
                    If SaveNewContrat() Then
                        MsgBox("Données enregistrer avec succès")
                        Await RefreshAfterDatabaseMutationAsync(Sub() ResetValues())
                        stpSaisie.Visibility = Visibility.Collapsed
                        btnNewData.Visibility = Visibility.Visible
                    Else
                        stpSaisie.Visibility = Visibility.Visible
                        btnNewData.Visibility = Visibility.Collapsed
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try
            End If

        Else
            If SaveModifCtr() Then
                MsgBox("Données modifier avec succès")
                stpSaisie.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
                Await RefreshAfterDatabaseMutationAsync(Sub() ResetValues())
            Else
                MsgBox("Erreur d'enregistrement")
                stpSaisie.Visibility = Visibility.Visible
                btnNewData.Visibility = Visibility.Collapsed
            End If

        End If

    End Sub

    Private Async Sub btnSaveContinue_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveContinue.Click
        If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDebut.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDebut.Focus()
                Exit Sub
            End If
        End If
        If NewData Then
            If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Try
                    If SaveNewContrat() Then
                        MsgBox("Données enregistrer avec succès")
                        Await RefreshAfterDatabaseMutationAsync(Sub() ResetValues())
                        'stpSaisie.Visibility = Visibility.Collapsed
                        'btnNewData.Visibility = Visibility.Visible
                    Else
                        'stpSaisie.Visibility = Visibility.Visible
                        'btnNewData.Visibility = Visibility.Collapsed
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try
            End If

        Else
            If SaveModifCtr() Then
                MsgBox("Données modifier avec succès")
                'stpSaisie.Visibility = Visibility.Collapsed
                'btnNewData.Visibility = Visibility.Visible
                Await RefreshAfterDatabaseMutationAsync(Sub() ResetValues())
            Else
                MsgBox("Erreur d'enregistrement")
                'stpSaisie.Visibility = Visibility.Visible
                'btnNewData.Visibility = Visibility.Collapsed
            End If

        End If

    End Sub

    Private Sub DgClientsContrats_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgClientsContrats.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgClientsContrats.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If iCbxClientName.Items.Contains(LCase(rowView.Row.ItemArray(0).ToString)) Then

                TbxNom.Text = rowView.Row.ItemArray(0).ToString
                NewNom = rowView.Row.ItemArray(0).ToString
                ChbActif.IsChecked = rowView.Row.ItemArray(1).ToString
                CbxTypeClient.SelectedItem = rowView.Row.ItemArray(2).ToString
                TbxMontant.Text = rowView.Row.ItemArray(3).ToString
                CbxTyptFactur.SelectedItem = rowView.Row.ItemArray(4).ToString
                TbxVille.Text = rowView.Row.ItemArray(5).ToString
                TbxAdresse.Text = rowView.Row.ItemArray(6).ToString
                dtpDebut.SelectedDate = rowView.Row.ItemArray(7).ToString
                TbxFin.Text = rowView.Row.ItemArray(8).ToString
                TbxDescript.Text = rowView.Row.ItemArray(9).ToString
                TbxObs.Text = rowView.Row.ItemArray(10).ToString
                RefCrt = rowView.Row.ItemArray(11).ToString

                stpSaisie.Visibility = Visibility.Visible
                btnDeleteAg.Visibility = Visibility.Visible
                NewData = False
                btnSave.Content = "Modifier et fermer"
                btnSaveContinue.Content = "Modifier et Continuer"
                btnFinCollab.Visibility = Visibility.Visible
                btnNewData.Visibility = Visibility.Collapsed
                If Not ChbActif.IsChecked Then
                    btnReActivClient.Visibility = Visibility.Visible
                    btnFinCollab.Visibility = Visibility.Collapsed
                End If
                If ChbActif.IsChecked Then
                    btnFinCollab.Visibility = Visibility.Visible
                    btnReActivClient.Visibility = Visibility.Collapsed
                End If
            Else
                stpSaisie.Visibility = Visibility.Collapsed
                btnDeleteAg.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Collapsed
                btnFinCollab.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Collapsed
                NewData = True
                btnSave.Content = "Enregistrer et fermer"
                btnSaveContinue.Content = "Enregistrer et ajouter un autre"
            End If
        End If
        AfficheProfilUser()
    End Sub
    Function DeleteCtr() As Boolean
        DeleteCtr = False
        Dim clientName As String = Nothing
        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL ")
        Try
            Using connection As New SQLiteConnection(ResolveMainDatabaseConnectionString(iConnxStr, True))
                connection.Open()
                Using transaction As SQLiteTransaction = connection.BeginTransaction()
                    Using command As New SQLiteCommand("SELECT NomClient FROM Client WHERE RefCtr = @id", connection, transaction)
                        command.Parameters.AddWithValue("@id", RefCrt)
                        clientName = Convert.ToString(command.ExecuteScalar())
                    End Using
                    If String.IsNullOrWhiteSpace(clientName) Then
                        MessageBox.Show("Le client sélectionné est introuvable.", "Suppression du client", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Return False
                    End If

                    Using command As New SQLiteCommand(
                        "SELECT COUNT(*) FROM Client WHERE NomClient = @name AND RefCtr <> @id",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@name", clientName)
                        command.Parameters.AddWithValue("@id", RefCrt)
                        If Convert.ToInt32(command.ExecuteScalar()) > 0 Then
                            Throw New InvalidOperationException("Plusieurs clients portent le même nom. Suppression annulée car leurs sites ne peuvent pas être attribués sans ambiguïté.")
                        End If
                    End Using

                    Dim dependentDeletes As String() = {
                        "DELETE FROM PlanPoste WHERE Poste IN (SELECT p.Code FROM Postes p JOIN SiteAffectation s ON s.NomSite = p.NomSite WHERE s.Client = @client)",
                        "DELETE FROM Affectation WHERE SiteAffect IN (SELECT NomSite FROM SiteAffectation WHERE Client = @client)",
                        "DELETE FROM Events WHERE SiteEvent IN (SELECT NomSite FROM SiteAffectation WHERE Client = @client)",
                        "DELETE FROM HeureSup WHERE SiteHS IN (SELECT NomSite FROM SiteAffectation WHERE Client = @client)",
                        "DELETE FROM sortie WHERE SiteSortie IN (SELECT NomSite FROM SiteAffectation WHERE Client = @client)",
                        "DELETE FROM Postes WHERE NomSite IN (SELECT NomSite FROM SiteAffectation WHERE Client = @client)",
                        "DELETE FROM SiteAffectation WHERE Client = @client"
                    }
                    For Each deleteStatement As String In dependentDeletes
                        Using command As New SQLiteCommand(deleteStatement, connection, transaction)
                            command.Parameters.AddWithValue("@client", clientName)
                            command.ExecuteNonQuery()
                        End Using
                    Next

                    Using command As New SQLiteCommand("DELETE FROM Client WHERE RefCtr = @id", connection, transaction)
                        command.Parameters.AddWithValue("@id", RefCrt)
                        If command.ExecuteNonQuery() = 0 Then
                            Throw New InvalidOperationException("Le client sélectionné n'a pas été supprimé.")
                        End If
                    End Using

                    transaction.Commit()
                End Using
            End Using

            If Not LocalModeBb Then NotifyGeneralDatabaseChanged()
            Joper("DeleteCtrClient at " & RefCrt, iConnxStr)
            DeleteCtr = True
        Catch ex As SQLiteException
            MessageBox.Show("La suppression du client et de ses données liées a échoué : " & ex.Message,
                            "Suppression du client", MessageBoxButton.OK, MessageBoxImage.Error)
        Catch ex As InvalidOperationException
            MessageBox.Show(ex.Message, "Suppression du client", MessageBoxButton.OK, MessageBoxImage.Warning)
        Catch ex As ArgumentException
            MessageBox.Show("La suppression du client et de ses données liées a échoué : " & ex.Message,
                            "Suppression du client", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Function


    Private Async Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        If MessageBox.Show("Voulez supprimer le client « " & TbxNom.Text & " » et ses sites et données liées (postes, affectations, événements, heures supplémentaires, planifications et sorties) ?",
                           "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If Not ConfirmAdminPassword(System.Windows.Window.GetWindow(Me),
                                        "la suppression du client « " & TbxNom.Text & " » et de ses données liées",
                                        iConnxStr) Then Return
            If DeleteCtr() Then
                MsgBox("Donnée supprimées avec succès ")
                Await RefreshAfterDatabaseMutationAsync(Sub() ResetValues())
            End If
        End If
    End Sub

    Private Async Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgClientsContrats.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefCrt = rowView.Row.ItemArray(11).ToString
            If MessageBox.Show("Voulez supprimer le client « " & rowView.Row.ItemArray(0).ToString & " » et ses sites et données liées (postes, affectations, événements, heures supplémentaires, planifications et sorties) ?",
                               "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                If Not ConfirmAdminPassword(System.Windows.Window.GetWindow(Me),
                                            "la suppression du client « " & rowView.Row.ItemArray(0).ToString & " » et de ses données liées",
                                            iConnxStr) Then Return
                If DeleteCtr() Then
                    MsgBox("Donnée supprimées avec succès ")
                    Await RefreshAfterDatabaseMutationAsync(Sub() ResetValues())
                End If
            End If
        End If
    End Sub

    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs) Handles cmiModif.Click
        Dim rowView As DataRowView = TryCast(DgClientsContrats.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs) Handles cmiResume.Click
        Dim rowView As DataRowView = TryCast(DgClientsContrats.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iName As String = rowView.Row.ItemArray(0).ToString
            Dim iResum As String = ResumeClient(rowView.Row.ItemArray(0).ToString)
            MsgBox(iResum, MsgBoxStyle.OkOnly, UCase(iName))
        End If

    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Contrat clients "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Contrat clients du " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgClientsContrats, "Point_Contrat-clients", iTitre) '"Point_Entrees"

    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        InialiseLoad()
    End Sub

    Private Sub btnFinCollab_Click(sender As Object, e As RoutedEventArgs) Handles btnFinCollab.Click
        Dim iFinColl As New wdwEndClient
        iFinColl.WindowStartupLocation = WindowStartupLocation.CenterScreen
        iFinColl.ShowDialog()
        NomClientTemp = TbxNom.Text
        ' iFinColl.cbxClent.SelectedItem = TbxNom.Text
    End Sub

    Private Sub btnReActivClient_Click(sender As Object, e As RoutedEventArgs) Handles btnReActivClient.Click
        Dim iFinColl As New wdwActivOldClient
        iFinColl.WindowStartupLocation = WindowStartupLocation.CenterScreen
        iFinColl.ShowDialog()
    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des CONTRATS - CLIENTS", "A ce jour " & Today, Me)
        AddHandler iDetailW.Closed, Sub()
                                        retourDansMainWindow?.Invoke()
                                        btnAgrandir.Visibility = Visibility.Visible
                                    End Sub
        iDetailW.WindowStartupLocation = WindowStartupLocation.CenterOwner
        iDetailW.Topmost = True
        Dim iWtf As New WaitFrm
        iWtf.Show()
        iDetailW.Show()
        iWtf.Close()

    End Sub
    Sub AfficheProfilUser()
        If Not CanAddDataOrdinary Then
            btnNewData.Visibility = Visibility.Collapsed
            btnSave.Visibility = Visibility.Collapsed
            btnSaveContinue.Visibility = Visibility.Collapsed
        End If
        If Not CanModifyDataOrdinary Then
            If btnSave.Content = "Modifier et fermer" Then btnSave.Visibility = Visibility.Collapsed
            If btnSaveContinue.Content = "Modifier et Continuer" Then btnSaveContinue.Visibility = Visibility.Collapsed
            cmiModif.Visibility = Visibility.Collapsed
        End If
        If Not CanDeleteDataOrdinary And Not CanGlobalDataManage Then
            btnDeleteAg.Visibility = Visibility.Collapsed
            cmiSuppr.Visibility = Visibility.Collapsed
            btnReActivClient.Visibility = Visibility.Collapsed
            btnFinCollab.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub CltInActif_Checked(sender As Object, e As RoutedEventArgs) Handles CltInActif.Checked
        iInActif = True
        testClientActif()
    End Sub

    Private Sub CltInActif_Unchecked(sender As Object, e As RoutedEventArgs) Handles CltInActif.Unchecked
        iInActif = False
        testClientActif()
    End Sub

    Private Sub CltActif_Checked(sender As Object, e As RoutedEventArgs) Handles CltActif.Checked
        iActif = True
        testClientActif()
    End Sub

    Private Sub CltActif_Unchecked(sender As Object, e As RoutedEventArgs) Handles CltActif.Unchecked
        iActif = False
        testClientActif()
    End Sub
    Sub testClientActif()
        If iActif And iInActif Then
            AffActif = False
        Else
            AffActif = True
        End If
        If iFormLoaded Then SearchData(BtnDataSearch.Text)
    End Sub
End Class
