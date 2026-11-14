Imports System.Windows.Threading
Imports System.Data.SQLite
Imports System.Data
Imports System.Globalization
Imports System.Text
Imports Microsoft.VisualBasic.FileIO

Public Class usercAgentsEntree
    Private Class AgentCsvRecord
        Public Property NomAgent As String
        Public Property PrenomAgent As String
        Public Property Sexe As String
        Public Property NumMatr As String
        Public Property Contacts As String
        Public Property DateNaiss As DateTime
        Public Property CatEch As String
        Public Property NbEnf As Integer
        Public Property Actif As Boolean
        Public Property DateEntree As DateTime
        Public Property VilleResidence As String
        Public Property Quartier As String
        Public Property Fonction As String
        Public Property Pays As String
        Public Property MatrimoSituation As String
        Public Property Nsecu As String
        Public Property CarteType As String
        Public Property CarteNumero As String
        Public Property Banque As String
        Public Property NumCompte As String
        Public Property LieuNaiss As String
        Public Property PrefNaiss As String

        Public ReadOnly Property NomPrenoms As String
            Get
                Return NomAgent.Trim().ToUpperInvariant() & " " & PrenomAgent.Trim()
            End Get
        End Property
    End Class

    Public iConnxStr As String
    Public LocalConnxStr As String
    Dim RefAg As Integer = 0
    Dim IdxAg As Integer = 0
    Dim NomPrenomAg As String = "Julio BAG2"
    Dim icbxNumMatricul, icbxRef As New ComboBox
    Dim inputTimer As DispatcherTimer
    Dim ChangeYN As Boolean = False
    Dim iActif As Boolean = True
    Dim iInActif As Boolean = False
    Dim AffActif As Boolean = True
    Dim NewData As Boolean = True
    Dim iFormLoaded As Boolean = False
    Dim BankValue, CartValue As String
    Dim NewNutricule As String
    Dim NewNomPremn As String
    Dim DateSortie As String = ""
    Dim idxFonctDefault As Integer = -1
    Dim retourDansMainWindow As Action
    Public Shared AfficheStat As Action
    'Dim iAction As Action
    Public Sub New(callback As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()
        'iAction = MyAsction
        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = callback

    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
        DbPathGeneral = PaphBackUp()
        LocalConnxStr = $"Data Source={DbPath};Version=3;"
        iConnxStr = LocalConnxStr
        InitialLoad()
        inputTimer = New DispatcherTimer()
        inputTimer.Interval = TimeSpan.FromSeconds(3)
        AddHandler inputTimer.Tick, AddressOf OnInputTimerElapsed
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Private Sub OnInputTimerElapsed(sender As Object, e As EventArgs)
        elpStatus.Visibility = Visibility.Visible
        inputTimer.Stop()
        ' Afficher le texte du TextBox
        Dispatcher.Invoke(Sub() SearchAgent(BtnAgentListeSearch.Text))
    End Sub
    Sub InitialLoad()
        elpStatus.Visibility = Visibility.Visible
        maj()
        AfficheStat?.Invoke()
        NewData = True
        Dim istr As String = " SELECT NomAgent, PrenomAgent, sexe, NumMatr, Contacts, format(DateEntree) as DateEntree, format(DateSortie) as DateSortie, actif, format(DateNaiss) as [Né(e) le],LieuNaiss,PrefNaiss, VilleResidence as Ville, Quartier, Fonction, CatEch,NbEnf, RefAgent as Ref,Nsecu, Banque,NumCompte,MatrimoSituation, Pays,CarteType,CarteNumero "
        istr += " FROM  agents  where actif=1"
        istr += " and DateEntree>='" & CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")).ToString("yyyy-MM-dd") & "'"
        istr += " ORDER BY actif desc, Nomprenoms "
        Dim agentData = LoadDataTable(istr, iConnxStr)
        DgAgentListe.ItemsSource = agentData.DefaultView
        tblNbAgent.Text = "Agents actifs: " & agentData.Select("actif = 1").Length & " | Agents inactifs: " & agentData.Select("actif = 0").Length & " | TOTAL: " & agentData.Rows.Count

        If FilmyCbx("select a.NumMatr from agents a ", icbxNumMatricul, iConnxStr) And FilmyCbx("select a.RefAgent  from agents a order by a.RefAgent desc ", icbxRef, iConnxStr) Then
            If FilmyCbx("select f.Fonction  from fonction f where f.Actif=1 order by f.Fonction", CbxFonction, iConnxStr) Then
                CbxFonction.Text = FonctionDefault
                idxFonctDefault = CbxFonction.SelectedIndex
                If iFormLoaded Then CbxFonction.SelectedIndex = -1
                btnDeleteAg.Visibility = Visibility.Collapsed
            End If
        Else
            btnSave.Visibility = Visibility.Hidden
            btnSaveContinue.Visibility = Visibility.Hidden
        End If
        If Not FilmyCbx("select a.SituationMatri from SituationMatrim a where a.Actif=1 order by a.Favorit desc,a.SituationMatri", CbxSituationMatrim, iConnxStr) Then
            CbxSituationMatrim.Items.Clear()
        Else
            CbxSituationMatrim.SelectedIndex = 0
        End If
        If Not FilmyCbx("select a.CodeCarte from typecarteid a where a.Actif=1 order by a.Favorit desc,a.TypeCatre", CbxTypeCarte, iConnxStr) Then CbxTypeCarte.Items.Clear()
        If Not FilmyCbx("select a.RefBank,a.Favorit from comptbank a where a.Actif=1 order by a.Favorit desc,a.RefBank ", CbxBanque, iConnxStr) Then CbxBanque.Items.Clear()
        If Not FilmyCbx("select sexe from sexe ", cbxSexe, iConnxStr) Then cbxSexe.Items.Clear()
        RefAg = MayStatVal("select a.RefAgent  from agents a order by a.RefAgent desc limit 1", iConnxStr) + 1
        'MsgBox(RefAg)
        TbxNbEnf.Text = 0
        TbxPays.Text = "TOGO"
        CartValue = ""
        BankValue = ""
        CbxFonction.SelectedItem = FonctionDefault

        dtpDateEmbauche.SelectedDate = Today
        dtpDateEmbauche.Text = Today
        dtpDateEmbauche.DisplayDateEnd = Today
        dtpDateEmbauche.DisplayDateStart = DateCreationSociete
        dtpDateNaiss.SelectedDate = Today.AddYears(-17)
        dtpDateNaiss.DisplayDateEnd = Today.AddYears(-17)
        dtpDateNaiss.DisplayDateStart = Today.AddYears(-65)

        stpSaisie.Visibility = Visibility.Collapsed
        stpSaisieDetails.Visibility = Visibility.Collapsed
        AgActif.IsChecked = True
        AgInActif.IsChecked = False
        ChangeYN = False
        dtpDebutSearch.SelectedDate = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")) 'DebutDuMois(Today.AddMonths(-1)) 'CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = CDate("24/" & Today.ToString("MM/yyyy")) 'Today
        elpStatus.Visibility = Visibility.Collapsed
        iFormLoaded = True
        AfficheProfilUser()
        'iAction.Invoke()
    End Sub
    Sub InitializeMyFields()
        elpStatus.Visibility = Visibility.Visible
        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnNewAg.Visibility = Visibility.Visible
        btnDeleteAg.Visibility = Visibility.Collapsed
        AfficheProfilUser()
        NewData = True
        iFormLoaded = False

        AfficheStat?.Invoke()

        FilmyCbx("select a.NumMatr from agents a ", icbxNumMatricul, iConnxStr)
        RefAg = MayStatVal("select a.RefAgent  from agents a order by a.RefAgent desc limit 1", iConnxStr) + 1

        TbxContact.Clear()
        TbxCSP.Clear()
        TbxNCNSS.Clear()
        TbxNom.Clear()
        TbxNumMaticule.Clear()
        TbxPrenom.Clear()
        TbxVille.Clear()
        TbxAdresse.Clear()
        TbxNunBank.Clear()
        TbxNunCarte.Clear()
        TbxLieuNaiss.Clear()
        TbxPrefNaiss.Clear()

        TbxNbEnf.Text = 0
        TbxPays.Text = "TOGO"
        cbxSexe.SelectedIndex = -1
        'CbxFonction.SelectedItem = FonctionDefault
        'idxFonctDefault = CbxFonction.SelectedIndex
        CbxFonction.SelectedIndex = -1
        CbxSituationMatrim.SelectedIndex = 0
        CbxBanque.SelectedIndex = -1
        CbxTypeCarte.SelectedIndex = -1

        dtpDateEmbauche.SelectedDate = Today
        dtpDateEmbauche.DisplayDateEnd = Today
        dtpDateEmbauche.DisplayDateStart = DateCreationSociete
        dtpDateNaiss.SelectedDate = Today.AddYears(-17)
        dtpDateNaiss.DisplayDateEnd = Today.AddYears(-17)
        dtpDateNaiss.DisplayDateStart = Today.AddYears(-65)


        Dim istr As String = " SELECT NomAgent, PrenomAgent, sexe, NumMatr, Contacts, format(DateEntree) as DateEntree,format(DateSortie) as DateSortie, actif, format(DateNaiss) as [Né(e) le],LieuNaiss,PrefNaiss, VilleResidence as Ville, Quartier, Fonction, CatEch,NbEnf, RefAgent as Ref,Nsecu, Banque,NumCompte,MatrimoSituation, Pays,CarteType,CarteNumero,LieuNaiss,PrefNaiss "
        istr += " FROM  agents where actif=1 "
        istr += " ORDER BY actif desc, DateEntree desc, RefAgent desc, Nomprenoms "
        DgAgentListe.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        tblNbAgent.Text = "Nombre d'agents actifs: " & NbAgentActif(LocalConnxStr) & " | Nombre d'agents inactifs: " & NbAgentInactif(LocalConnxStr)
        ChangeYN = False
        AgActif.IsChecked = True
        AgInActif.IsChecked = False
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If dtpDateEmbauche.SelectedDate.HasValue AndAlso dtpDateEmbauche.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDateEmbauche.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDateEmbauche.Focus()
                Exit Sub
            End If
        End If
        If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If NewData Then
                Try
                    If SaveNewAgent() Then
                        MsgBox("Données enregistrer avec succès")
                        Await RefreshAfterDatabaseMutationAsync(Sub()
                                                                    InitializeMyFields()
                                                                    SearchAgent(BtnAgentListeSearch.Text)
                                                                End Sub)
                        stpSaisie.Visibility = Visibility.Collapsed
                        btnNewAg.Visibility = Visibility.Visible
                    Else
                        MsgBox("Echec d'enregistrement de données")
                        stpSaisie.Visibility = Visibility.Visible
                        btnNewAg.Visibility = Visibility.Collapsed
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            Else
                Try
                    If SaveModifAgent() Then
                        MsgBox("Données modifiées avec succès")
                        Await RefreshAfterDatabaseMutationAsync(Sub()
                                                                    InitializeMyFields()
                                                                    SearchAgent(BtnAgentListeSearch.Text)
                                                                End Sub)
                        stpSaisie.Visibility = Visibility.Collapsed
                        btnNewAg.Visibility = Visibility.Visible
                    Else
                        MsgBox("Echec d'enregistrement de données")
                        stpSaisie.Visibility = Visibility.Visible
                        btnNewAg.Visibility = Visibility.Collapsed
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            End If

        End If
    End Sub
    Function SaveNewAgent() As Boolean
        Dim istr As String = " insert into agents "
        istr += " (RefAgent,NomPrenoms,NomAgent,PrenomAgent,NumMatr,Contacts,DateNaiss,Sexe,CatEch,NbEnf,Actif,DateEntree,VilleResidence,Quartier,Fonction,OpTime,Pays,MatrimoSituation,Nsecu,CarteType,CarteNumero,Banque,NumCompte,LieuNaiss,PrefNaiss ) "
        istr += " values "
        istr += "(" & RefAg & ",'" & Trim(Apost(NomPrenomAg)) & "','" & Trim(Apost(TbxNom.Text)) & "','" & Trim(Apost(TbxPrenom.Text)) & "','" & Trim(Apost(TbxNumMaticule.Text)) & "','"
        istr += Trim(Apost(TbxContact.Text)) & "','" & dtpDateNaiss.SelectedDate.Value.ToString("yyyy-MM-dd") & "','" & Apost(cbxSexe.SelectedItem) & "','" & Apost(TbxCSP.Text) & "'," & TbxNbEnf.Text & ","
        istr += ChbActif.IsChecked & ",'" & dtpDateEmbauche.SelectedDate.Value.ToString("yyyy-MM-dd") & "','" & Apost(TbxVille.Text) & "','" & Trim(Apost(TbxAdresse.Text)) & "','" & Apost(CbxFonction.SelectedItem) & "','" & Now.ToString("yyyy-MM-dd HH:mm:ss") & "','"
        istr += Trim(Apost(TbxPays.Text)) & "','" & Apost(CbxSituationMatrim.SelectedItem) & "','" & Trim(Apost(TbxNCNSS.Text)) & "','" & Trim(Apost(CartValue)) & "','" & Trim(Apost(TbxNunCarte.Text)) & "','" & Apost(BankValue) & "','" & Trim(Apost(TbxNunBank.Text)) & "','" & Apost(TbxLieuNaiss.Text) & "','" & Trim(Apost(TbxPrefNaiss.Text)) & "'"
        istr += ")"
        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL => " & DbPath)
        'MsgBox("new - " & istr,)
        SaveNewAgent = SaveNewData(istr, iConnxStr)
        If SaveNewAgent Then Joper("SaveNewAgent As " & NomPrenomAg, iConnxStr)
    End Function

    Private Async Sub btnImportCsv_Click(sender As Object, e As RoutedEventArgs) Handles btnImportCsv.Click
        Dim dialog As New Microsoft.Win32.OpenFileDialog With {
            .Title = "Importer des agents depuis un fichier CSV",
            .Filter = "Fichiers CSV (*.csv)|*.csv|Tous les fichiers (*.*)|*.*",
            .CheckFileExists = True,
            .Multiselect = False
        }
        If dialog.ShowDialog() <> True Then Return

        Try
            Dim records As List(Of AgentCsvRecord) = ReadAgentCsv(dialog.FileName)
            If records.Count = 0 Then
                MessageBox.Show("Le fichier CSV ne contient aucune donnée à importer.", "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            If MessageBox.Show(
                "Importer " & records.Count & " agent(s) depuis ce fichier ? L'import sera annulé si une donnée est invalide ou existe déjà.",
                "Confirmation de l'import CSV",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) <> MessageBoxResult.Yes Then Return

            elpStatus.Visibility = Visibility.Visible
            ImportAgentRecords(records)
            If Not LocalModeBb Then NotifyGeneralDatabaseChanged()
            Await RefreshAfterDatabaseMutationAsync(Sub()
                                                        SearchAgent(BtnAgentListeSearch.Text)
                                                        AfficheStat?.Invoke()
                                                    End Sub)
            MessageBox.Show(records.Count & " agent(s) importé(s) avec succès.", "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            MessageBox.Show("L'import CSV a échoué : " & ex.Message, "Import CSV", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            elpStatus.Visibility = Visibility.Collapsed
        End Try
    End Sub

    Private Function ReadAgentCsv(filePath As String) As List(Of AgentCsvRecord)
        Try
            Return ReadAgentCsv(filePath, New UTF8Encoding(False, True))
        Catch ex As DecoderFallbackException
            Return ReadAgentCsv(filePath, Encoding.GetEncoding(1252))
        End Try
    End Function

    Private Function ReadAgentCsv(filePath As String, fileEncoding As Encoding) As List(Of AgentCsvRecord)
        Dim delimiter As String
        Using reader As New IO.StreamReader(filePath, fileEncoding, True)
            Dim firstLine As String = reader.ReadLine()
            If firstLine Is Nothing Then Return New List(Of AgentCsvRecord)()
            Dim semicolonCount As Integer = CountUnquotedDelimiters(firstLine, ";"c)
            Dim commaCount As Integer = CountUnquotedDelimiters(firstLine, ","c)
            delimiter = If(semicolonCount > commaCount, ";", ",")
        End Using

        Dim aliases As Dictionary(Of String, String) = CreateCsvHeaderAliases()
        Dim records As New List(Of AgentCsvRecord)()
        Dim errors As New List(Of String)()
        Dim matricules As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim noms As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Using parser As New TextFieldParser(filePath, fileEncoding, True)
            parser.TextFieldType = FieldType.Delimited
            parser.SetDelimiters(delimiter)
            parser.HasFieldsEnclosedInQuotes = True
            parser.TrimWhiteSpace = False

            If parser.EndOfData Then Return records
            Dim headers As String() = parser.ReadFields()
            Dim columnIndexes As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            For index As Integer = 0 To headers.Length - 1
                Dim normalizedHeader As String = NormalizeCsvHeader(headers(index))
                Dim targetColumn As String = Nothing
                If aliases.TryGetValue(normalizedHeader, targetColumn) Then
                    If columnIndexes.ContainsKey(targetColumn) Then
                        Throw New FormatException("L'en-tête CSV contient plusieurs colonnes pour « " & targetColumn & " ».")
                    End If
                    columnIndexes.Add(targetColumn, index)
                End If
            Next

            Dim requiredColumns As String() = {
                "NomAgent", "PrenomAgent", "NumMatr", "Contacts", "DateNaiss", "Sexe",
                "CatEch", "DateEntree", "VilleResidence", "Quartier", "Fonction"
            }
            Dim missingColumns = requiredColumns.Where(Function(column) Not columnIndexes.ContainsKey(column)).ToArray()
            If missingColumns.Length > 0 Then
                Throw New FormatException("Colonnes obligatoires manquantes : " & String.Join(", ", missingColumns) & ".")
            End If

            Dim recordNumber As Integer = 1
            While Not parser.EndOfData
                Dim fields As String() = parser.ReadFields()
                recordNumber += 1
                If fields Is Nothing OrElse fields.All(Function(field) String.IsNullOrWhiteSpace(field)) Then Continue While

                Dim rowErrorCount As Integer = errors.Count
                If fields.Length > headers.Length Then
                    errors.Add("Enregistrement " & recordNumber & " : il contient plus de champs que l'en-tête.")
                    Continue While
                End If

                Dim record As New AgentCsvRecord With {
                    .NomAgent = GetCsvValue(fields, columnIndexes, "NomAgent"),
                    .PrenomAgent = GetCsvValue(fields, columnIndexes, "PrenomAgent"),
                    .NumMatr = GetCsvValue(fields, columnIndexes, "NumMatr"),
                    .Contacts = GetCsvValue(fields, columnIndexes, "Contacts"),
                    .CatEch = GetCsvValue(fields, columnIndexes, "CatEch"),
                    .VilleResidence = GetCsvValue(fields, columnIndexes, "VilleResidence"),
                    .Quartier = GetCsvValue(fields, columnIndexes, "Quartier"),
                    .Fonction = GetCsvValue(fields, columnIndexes, "Fonction"),
                    .Pays = If(String.IsNullOrWhiteSpace(GetCsvValue(fields, columnIndexes, "Pays")), "TOGO", GetCsvValue(fields, columnIndexes, "Pays")),
                    .MatrimoSituation = If(String.IsNullOrWhiteSpace(GetCsvValue(fields, columnIndexes, "MatrimoSituation")), "Celibataire", GetCsvValue(fields, columnIndexes, "MatrimoSituation")),
                    .Nsecu = GetCsvValue(fields, columnIndexes, "Nsecu"),
                    .CarteType = GetCsvValue(fields, columnIndexes, "CarteType"),
                    .CarteNumero = GetCsvValue(fields, columnIndexes, "CarteNumero"),
                    .Banque = GetCsvValue(fields, columnIndexes, "Banque"),
                    .NumCompte = GetCsvValue(fields, columnIndexes, "NumCompte"),
                    .LieuNaiss = GetCsvValue(fields, columnIndexes, "LieuNaiss"),
                    .PrefNaiss = GetCsvValue(fields, columnIndexes, "PrefNaiss")
                }

                If record.NomAgent.Length = 0 OrElse record.PrenomAgent.Length = 0 Then
                    errors.Add("Enregistrement " & recordNumber & " : le nom et le prénom sont obligatoires.")
                End If
                If record.NumMatr.Length < 5 Then errors.Add("Enregistrement " & recordNumber & " : le matricule doit contenir au moins 5 caractères.")
                If String.IsNullOrWhiteSpace(record.Contacts) OrElse String.IsNullOrWhiteSpace(record.CatEch) OrElse
                   String.IsNullOrWhiteSpace(record.VilleResidence) OrElse String.IsNullOrWhiteSpace(record.Quartier) OrElse
                   String.IsNullOrWhiteSpace(record.Fonction) Then
                    errors.Add("Enregistrement " & recordNumber & " : contact, catégorie, ville, quartier et fonction sont obligatoires.")
                End If

                Dim rawDate As String = GetCsvValue(fields, columnIndexes, "DateNaiss")
                If Not TryParseCsvDate(rawDate, record.DateNaiss) Then
                    errors.Add("Enregistrement " & recordNumber & " : date de naissance invalide (« " & rawDate & " »).")
                End If
                rawDate = GetCsvValue(fields, columnIndexes, "DateEntree")
                If Not TryParseCsvDate(rawDate, record.DateEntree) Then
                    errors.Add("Enregistrement " & recordNumber & " : date d'embauche invalide (« " & rawDate & " »).")
                End If

                Dim rawSexe As String = GetCsvValue(fields, columnIndexes, "Sexe").ToUpperInvariant()
                Select Case rawSexe
                    Case "M", "H", "HOMME"
                        record.Sexe = "M"
                    Case "F", "FEMME"
                        record.Sexe = "F"
                    Case Else
                        errors.Add("Enregistrement " & recordNumber & " : le sexe doit être M ou F.")
                End Select

                Dim rawNbEnf As String = GetCsvValue(fields, columnIndexes, "NbEnf")
                If String.IsNullOrWhiteSpace(rawNbEnf) Then
                    record.NbEnf = 0
                ElseIf Not Integer.TryParse(rawNbEnf, record.NbEnf) OrElse record.NbEnf < 0 Then
                    errors.Add("Enregistrement " & recordNumber & " : le nombre de personnes à charge doit être un entier positif.")
                End If

                Dim rawActif As String = GetCsvValue(fields, columnIndexes, "Actif").ToLowerInvariant()
                Select Case rawActif
                    Case "", "1", "oui", "true", "actif"
                        record.Actif = True
                    Case "0", "non", "false", "inactif"
                        record.Actif = False
                    Case Else
                        errors.Add("Enregistrement " & recordNumber & " : la colonne Actif doit valoir 1/0, oui/non ou actif/inactif.")
                End Select

                If errors.Count = rowErrorCount Then
                    If Not matricules.Add(record.NumMatr) Then
                        errors.Add("Enregistrement " & recordNumber & " : le matricule « " & record.NumMatr & " » apparaît plusieurs fois dans le fichier.")
                    ElseIf Not noms.Add(record.NomPrenoms) Then
                        errors.Add("Enregistrement " & recordNumber & " : le nom complet « " & record.NomPrenoms & " » apparaît plusieurs fois dans le fichier.")
                    Else
                        records.Add(record)
                    End If
                End If
            End While
        End Using

        If errors.Count > 0 Then
            Throw New FormatException("Corrigez les erreurs suivantes avant de relancer l'import :" & Environment.NewLine & String.Join(Environment.NewLine, errors))
        End If
        Return records
    End Function

    Private Function CreateCsvHeaderAliases() As Dictionary(Of String, String)
        Dim aliases As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        AddCsvHeaderAliases(aliases, "NomAgent", "NomAgent", "Nom")
        AddCsvHeaderAliases(aliases, "PrenomAgent", "PrenomAgent", "Prenom", "Prénom")
        AddCsvHeaderAliases(aliases, "NumMatr", "NumMatr", "Matricule", "Numero matricule")
        AddCsvHeaderAliases(aliases, "Contacts", "Contacts", "Contact", "Telephone", "Téléphone")
        AddCsvHeaderAliases(aliases, "DateNaiss", "DateNaiss", "Date naissance", "Date de naissance")
        AddCsvHeaderAliases(aliases, "Sexe", "Sexe")
        AddCsvHeaderAliases(aliases, "CatEch", "CatEch", "Categorie", "Catégorie")
        AddCsvHeaderAliases(aliases, "DateEntree", "DateEntree", "Date entree", "Date embauche", "Date d'embauche")
        AddCsvHeaderAliases(aliases, "VilleResidence", "VilleResidence", "Ville", "Ville residence")
        AddCsvHeaderAliases(aliases, "Quartier", "Quartier", "Adresse")
        AddCsvHeaderAliases(aliases, "Fonction", "Fonction")
        AddCsvHeaderAliases(aliases, "NbEnf", "NbEnf", "Nombre enfants", "Enfants a charge")
        AddCsvHeaderAliases(aliases, "Actif", "Actif", "Statut")
        AddCsvHeaderAliases(aliases, "Pays", "Pays")
        AddCsvHeaderAliases(aliases, "MatrimoSituation", "MatrimoSituation", "Situation matrimoniale")
        AddCsvHeaderAliases(aliases, "Nsecu", "Nsecu", "CNSS", "Numero CNSS")
        AddCsvHeaderAliases(aliases, "CarteType", "CarteType", "Type carte")
        AddCsvHeaderAliases(aliases, "CarteNumero", "CarteNumero", "Numero carte")
        AddCsvHeaderAliases(aliases, "Banque", "Banque")
        AddCsvHeaderAliases(aliases, "NumCompte", "NumCompte", "Numero compte")
        AddCsvHeaderAliases(aliases, "LieuNaiss", "LieuNaiss", "Lieu naissance")
        AddCsvHeaderAliases(aliases, "PrefNaiss", "PrefNaiss", "Prefecture naissance")
        Return aliases
    End Function

    Private Sub AddCsvHeaderAliases(aliases As Dictionary(Of String, String), targetColumn As String, ParamArray headerNames As String())
        For Each headerName As String In headerNames
            aliases(NormalizeCsvHeader(headerName)) = targetColumn
        Next
    End Sub

    Private Function NormalizeCsvHeader(value As String) As String
        Dim normalized As String = value.Normalize(NormalizationForm.FormD)
        Dim result As New StringBuilder()
        For Each character As Char In normalized
            If CharUnicodeInfo.GetUnicodeCategory(character) <> UnicodeCategory.NonSpacingMark AndAlso Char.IsLetterOrDigit(character) Then
                result.Append(Char.ToLowerInvariant(character))
            End If
        Next
        Return result.ToString()
    End Function

    Private Function GetCsvValue(fields As String(), columnIndexes As Dictionary(Of String, Integer), columnName As String) As String
        Dim index As Integer
        If columnIndexes.TryGetValue(columnName, index) AndAlso index < fields.Length Then
            Return NormalizeCsvValue(fields(index))
        End If
        Return ""
    End Function

    Private Function TryParseCsvDate(value As String, ByRef parsedDate As DateTime) As Boolean
        Dim formats As String() = {"yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy"}
        Return DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate) OrElse
               DateTime.TryParse(value, CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None, parsedDate)
    End Function

    Private Function CountUnquotedDelimiters(value As String, delimiter As Char) As Integer
        Dim count As Integer = 0
        Dim insideQuotes As Boolean = False
        Dim index As Integer = 0
        While index < value.Length
            If value(index) = """"c Then
                If insideQuotes AndAlso index + 1 < value.Length AndAlso value(index + 1) = """"c Then
                    index += 1
                Else
                    insideQuotes = Not insideQuotes
                End If
            ElseIf value(index) = delimiter AndAlso Not insideQuotes Then
                count += 1
            End If
            index += 1
        End While
        Return count
    End Function

    Private Sub ImportAgentRecords(records As List(Of AgentCsvRecord))
        Dim writeConnectionString As String = ResolveMainDatabaseConnectionString(iConnxStr, True)
        Using connection As New SQLiteConnection(writeConnectionString)
            connection.Open()
            Using transaction As SQLiteTransaction = connection.BeginTransaction()
                Dim nextRef As Integer
                Using command As New SQLiteCommand("SELECT COALESCE(MAX(RefAgent), 0) + 1 FROM agents", connection, transaction)
                    nextRef = Convert.ToInt32(command.ExecuteScalar())
                End Using

                Dim existingAgents As Dictionary(Of String, String) =
                    LoadCsvLookup(connection, transaction, "SELECT NomPrenoms, NumMatr FROM agents", "NomPrenoms", "NomPrenoms", "NumMatr")
                Dim activeFunctions As Dictionary(Of String, String) =
                    LoadCsvLookup(connection, transaction, "SELECT Fonction FROM fonction WHERE Actif = 1", "Fonction", "Fonction")
                For Each record As AgentCsvRecord In records
                    Dim canonicalFunction As String = Nothing
                    If Not activeFunctions.TryGetValue(record.Fonction, canonicalFunction) Then
                        Throw New InvalidOperationException("La fonction « " & record.Fonction & " » de l'agent « " & record.NomPrenoms & " » n'existe pas ou n'est pas active.")
                    End If
                    record.Fonction = canonicalFunction

                    Dim matriculeKey As String = NormalizeCsvValue(record.NumMatr)
                    Dim nameKey As String = NormalizeCsvValue(record.NomPrenoms)
                    If existingAgents.ContainsKey(matriculeKey) OrElse existingAgents.ContainsKey(nameKey) Then
                        Throw New InvalidOperationException("Le matricule ou le nom complet « " & record.NomPrenoms & " / " & record.NumMatr & " » existe déjà dans la base.")
                    End If

                    Using insertCommand As New SQLiteCommand(
                        "INSERT INTO agents (RefAgent, NomPrenoms, NomAgent, PrenomAgent, NumMatr, Contacts, DateNaiss, Sexe, CatEch, NbEnf, Actif, DateEntree, VilleResidence, Quartier, Fonction, OpTime, Pays, MatrimoSituation, Nsecu, CarteType, CarteNumero, Banque, NumCompte, LieuNaiss, PrefNaiss) " &
                        "VALUES (@RefAgent, @NomPrenoms, @NomAgent, @PrenomAgent, @NumMatr, @Contacts, @DateNaiss, @Sexe, @CatEch, @NbEnf, @Actif, @DateEntree, @VilleResidence, @Quartier, @Fonction, @OpTime, @Pays, @MatrimoSituation, @Nsecu, @CarteType, @CarteNumero, @Banque, @NumCompte, @LieuNaiss, @PrefNaiss)",
                        connection,
                        transaction)
                        insertCommand.Parameters.AddWithValue("@RefAgent", nextRef)
                        insertCommand.Parameters.AddWithValue("@NomPrenoms", record.NomPrenoms)
                        insertCommand.Parameters.AddWithValue("@NomAgent", record.NomAgent)
                        insertCommand.Parameters.AddWithValue("@PrenomAgent", record.PrenomAgent)
                        insertCommand.Parameters.AddWithValue("@NumMatr", record.NumMatr)
                        insertCommand.Parameters.AddWithValue("@Contacts", record.Contacts)
                        insertCommand.Parameters.AddWithValue("@DateNaiss", record.DateNaiss.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                        insertCommand.Parameters.AddWithValue("@Sexe", record.Sexe)
                        insertCommand.Parameters.AddWithValue("@CatEch", record.CatEch)
                        insertCommand.Parameters.AddWithValue("@NbEnf", record.NbEnf)
                        insertCommand.Parameters.AddWithValue("@Actif", If(record.Actif, 1, 0))
                        insertCommand.Parameters.AddWithValue("@DateEntree", record.DateEntree.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                        insertCommand.Parameters.AddWithValue("@VilleResidence", record.VilleResidence)
                        insertCommand.Parameters.AddWithValue("@Quartier", record.Quartier)
                        insertCommand.Parameters.AddWithValue("@Fonction", record.Fonction)
                        insertCommand.Parameters.AddWithValue("@OpTime", Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                        insertCommand.Parameters.AddWithValue("@Pays", record.Pays)
                        insertCommand.Parameters.AddWithValue("@MatrimoSituation", record.MatrimoSituation)
                        insertCommand.Parameters.AddWithValue("@Nsecu", record.Nsecu)
                        insertCommand.Parameters.AddWithValue("@CarteType", record.CarteType)
                        insertCommand.Parameters.AddWithValue("@CarteNumero", record.CarteNumero)
                        insertCommand.Parameters.AddWithValue("@Banque", record.Banque)
                        insertCommand.Parameters.AddWithValue("@NumCompte", record.NumCompte)
                        insertCommand.Parameters.AddWithValue("@LieuNaiss", record.LieuNaiss)
                        insertCommand.Parameters.AddWithValue("@PrefNaiss", record.PrefNaiss)
                        insertCommand.ExecuteNonQuery()
                    End Using
                    If Not existingAgents.ContainsKey(matriculeKey) Then existingAgents.Add(matriculeKey, record.NomPrenoms)
                    If Not existingAgents.ContainsKey(nameKey) Then existingAgents.Add(nameKey, record.NomPrenoms)
                    nextRef += 1
                Next

                transaction.Commit()
            End Using
        End Using
    End Sub

    Function SaveModifAgent() As Boolean
        Dim istr As String = " update Agents set "
        If NewNomPremn <> NomPrenomAg Then istr += " NomPrenoms='" & Trim(Apost(NomPrenomAg)) & "',NomAgent='" & Trim(Apost(TbxNom.Text)) & "',PrenomAgent='" & Trim(Apost(TbxPrenom.Text)) & "'"
        If NewNutricule <> TbxNumMaticule.Text Then
            If istr = " update Agents set " Then
                istr += "NumMatr='" & Trim(Apost(TbxNumMaticule.Text)) & "'"
            Else
                istr += ",NumMatr='" & Trim(Apost(TbxNumMaticule.Text)) & "'"
            End If
        End If

        If istr = " update Agents set " Then
            istr += "Contacts='" & Trim(Apost(TbxContact.Text)) & "',DateNaiss='" & dtpDateNaiss.SelectedDate.Value.ToString("yyyy-MM-dd") & "',Sexe='" & Apost(cbxSexe.SelectedItem) & "',CatEch='" & Trim(Apost(TbxCSP.Text)) & "',NbEnf=" & TbxNbEnf.Text & ", Actif="
        Else
            istr += ",Contacts='" & Trim(Apost(TbxContact.Text)) & "',DateNaiss='" & dtpDateNaiss.SelectedDate.Value.ToString("yyyy-MM-dd") & "',Sexe='" & Apost(cbxSexe.SelectedItem) & "',CatEch='" & Trim(Apost(TbxCSP.Text)) & "',NbEnf=" & TbxNbEnf.Text & ", Actif="
        End If
        istr += ChbActif.IsChecked & ",DateEntree='" & dtpDateEmbauche.SelectedDate.Value.ToString("yyyy-MM-dd") & "',VilleResidence='" & Apost(TbxVille.Text) & "',Quartier='" & Apost(TbxAdresse.Text) & "',Fonction='" & Apost(CbxFonction.SelectedItem) & "',OpTime='" & Now.ToString("yyyy-MM-dd HH:mm:ss") & "',Pays='"
        istr += Trim(Apost(TbxPays.Text)) & "',MatrimoSituation='" & Apost(CbxSituationMatrim.SelectedItem) & "',Nsecu='" & Trim(Apost(TbxNCNSS.Text)) & "',CarteType='" & Trim(Apost(CartValue)) & "',CarteNumero='" & Trim(Apost(TbxNunCarte.Text)) & "',Banque='" & Trim(Apost(BankValue)) & "',NumCompte='" & Trim(Apost(TbxNunBank.Text)) & "',"
        istr += "LieuNaiss='" & Trim(Apost(TbxLieuNaiss.Text)) & "',PrefNaiss='" & Trim(Apost(TbxPrefNaiss.Text)) & "'"
        'MsgBox("modif - " & istr,)
        If ChbActif.IsChecked And DateSortie <> "" Then
            If MsgBox("Vous êtes sur le point de ACTIVER L'AGENT. " & DateSortie & Chr(13) & " CONTINUER ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then istr += ", DateSortie=null "
        End If
        istr += " where RefAgent=" & RefAg & " "
        'MsgBox(istr)
        If isLocalModeDd() Then MsgBox("Vous travaillez en MODE LOCAL => " & iConnxStr)
        SaveModifAgent = SaveAgentModification(istr)
        If SaveModifAgent And NewNomPremn <> NomPrenomAg Then Joper("SaveModifAgent As " & NewNomPremn & " at " & RefAg, iConnxStr)
    End Function

    Private Function SaveAgentModification(agentUpdate As String) As Boolean
        Try
            Dim writeConnectionString As String = ResolveMainDatabaseConnectionString(iConnxStr, True)
            Using connection As New SQLiteConnection(writeConnectionString)
                connection.Open()
                Using transaction As SQLiteTransaction = connection.BeginTransaction()
                    Using command As New SQLiteCommand("PRAGMA defer_foreign_keys = ON", connection, transaction)
                        command.ExecuteNonQuery()
                    End Using

                    Using command As New SQLiteCommand(agentUpdate, connection, transaction)
                        command.ExecuteNonQuery()
                    End Using

                    If NewNutricule <> TbxNumMaticule.Text Then
                        Dim matriculeUpdate As String = "update contratstbl set NomPrenoms='" & Apost(TbxNumMaticule.Text) & "' where NomPrenoms='" & Apost(NewNutricule) & "'"
                        Using command As New SQLiteCommand(matriculeUpdate, connection, transaction)
                            command.ExecuteNonQuery()
                        End Using
                    End If

                    If NewNomPremn <> NomPrenomAg Then
                        Dim nameUpdates As String() = {
                            "update PlanPoste set Agent='" & Apost(NomPrenomAg) & "' where Agent='" & Apost(NewNomPremn) & "'",
                            "update sortie set NomsPrenoms='" & Apost(NomPrenomAg) & "' where NomsPrenoms='" & Apost(NewNomPremn) & "'",
                            "update HeureSup set Agent='" & Apost(NomPrenomAg) & "' where Agent='" & Apost(NewNomPremn) & "'",
                            "update Affectation set AgentAffect='" & Apost(NomPrenomAg) & "' where AgentAffect='" & Apost(NewNomPremn) & "'",
                            "update Absences set NomPrenom='" & Apost(NomPrenomAg) & "' where NomPrenom='" & Apost(NewNomPremn) & "'"
                        }
                        For Each nameUpdate As String In nameUpdates
                            Using command As New SQLiteCommand(nameUpdate, connection, transaction)
                                command.ExecuteNonQuery()
                            End Using
                        Next

                        Using command As New SQLiteCommand("select count(*) from sqlite_master where type='table' and lower(name)='agences'", connection, transaction)
                            If Convert.ToInt32(command.ExecuteScalar()) > 0 Then
                                Using updateCommand As New SQLiteCommand("update Agences set Responsable='" & Apost(NomPrenomAg) & "' where Responsable='" & Apost(NewNomPremn) & "'", connection, transaction)
                                    updateCommand.ExecuteNonQuery()
                                End Using
                            End If
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

    Private Sub TbxNumMaticule_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxNumMaticule.TextChanged
        If iFormLoaded Then
            SaisieMajusculeTbx(sender)
            SaisiTbx(sender, TblIndicationNumMaticule)
            ActivSaveBtn()
        End If

    End Sub

    Private Sub TbxPrenom_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxPrenom.TextChanged
        If iFormLoaded Then
            SaisiePremiereLettreMajusculeTbx(sender)
            SaisiTbx(sender, TblIndicationPrenom)
            NomPrenomAg = UCase(TbxNom.Text) & " " & TbxPrenom.Text
            If TbxNom.Text.Length > 0 And TbxPrenom.Text.Length > 0 Then
                CbxFonction.SelectedIndex = idxFonctDefault
            Else
                CbxFonction.SelectedIndex = -1
            End If
            ActivSaveBtn()
        End If

        'MsgBox(NomPrenomAg)
    End Sub

    Private Sub cbxSexe_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cbxSexe.SelectionChanged
        If iFormLoaded Then
            ActivSaveBtn()
        End If
    End Sub

    Private Sub TbxContact_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxContact.TextChanged
        If iFormLoaded Then
            SaisiTbx(sender, TblIndicationContact)
            ActivSaveBtn()
        End If
    End Sub

    Private Sub TbxNom_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxNom.TextChanged
        If iFormLoaded Then
            SaisieMajusculeTbx(sender)
            SaisiTbx(sender, TblIndicationNom)
            NomPrenomAg = UCase(TbxNom.Text).Trim & " " & TbxPrenom.Text.Trim
            If TbxNom.Text.Length > 0 And TbxPrenom.Text.Length > 0 Then
                CbxFonction.SelectedIndex = idxFonctDefault
            Else
                CbxFonction.SelectedIndex = -1
            End If
            ActivSaveBtn()
        End If
    End Sub

    Private Sub TbxAdresse_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxAdresse.TextChanged
        If iFormLoaded Then
            SaisiePremiereLettreMajusculeTbx(sender)
            SaisiTbx(sender, TblIndicationAdresse)
            ActivSaveBtn()
        End If
    End Sub

    Private Sub TbxNbEnf_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxNbEnf.TextChanged
        If iFormLoaded Then
            SaisiTbx(sender, TblIndicationNbEnf)
            ActivSaveBtn()
        End If
    End Sub

    Private Sub TbxVille_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxVille.TextChanged
        If iFormLoaded Then
            SaisiePremiereLettreMajusculeTbx(sender)
            SaisiTbx(sender, TblIndicationVille)
            ActivSaveBtn()
        End If
    End Sub

    Private Sub TbxCSP_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxCSP.TextChanged
        If iFormLoaded Then
            'SaisiePremiereLettreMajusculeTbx(sender)
            SaisiTbx(sender, TblIndicationCSP)
            ActivSaveBtn()
        End If
    End Sub

    Private Sub CbxFonction_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxFonction.SelectionChanged
        If iFormLoaded Then
            SaisiCbx(CbxFonction, TblIndicationFonction)
            If CbxFonction.SelectedItem <> "" Then
                TblIndicationFonction.Visibility = Visibility.Hidden
            Else
                TblIndicationFonction.Visibility = Visibility.Visible
            End If

            ActivSaveBtn()
        End If
        If iFormLoaded And CbxFonction.SelectedItem <> "" Then TbxNumMaticule.Text = NumMatrDefault(CbxFonction.SelectedItem)
    End Sub

    Private Sub dtpDateEmbauche_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDateEmbauche.SelectedDateChanged, dtpDateNaiss.SelectedDateChanged
        If iFormLoaded Then
            ActivSaveBtn()
        End If
    End Sub

    Private Sub TbxContact_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxContact.PreviewTextInput
        If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9+\-*/]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxContact_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles TbxContact.PreviewKeyDown
        ' Autoriser les touches de contrôle (Backspace, Delete, flèches)
        If e.Key = Key.Back OrElse e.Key = Key.Delete OrElse e.Key = Key.Left OrElse e.Key = Key.Right Then
            e.Handled = False ' Autoriser ces touches
        End If
    End Sub

    Private Sub TbxNbEnf_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles TbxNbEnf.PreviewKeyDown
        If e.Key = Key.Back OrElse e.Key = Key.Delete OrElse e.Key = Key.Left OrElse e.Key = Key.Right Then
            e.Handled = False ' Autoriser ces touches
        End If

    End Sub

    Private Sub TbxNbEnf_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxNbEnf.PreviewTextInput
        If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9+\-*/]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub btnNewAg_Click(sender As Object, e As RoutedEventArgs) Handles btnNewAg.Click
        stpSaisie.Visibility = Visibility.Visible
        btnNewAg.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnAnnule_Click(sender As Object, e As RoutedEventArgs) Handles btnAnnule.Click
        If ChangeYN Then
            If MsgBox("ANNULER VRAIMENT ?", MsgBoxStyle.OkCancel, "ATTENTION") = MsgBoxResult.Ok Then
                InitializeMyFields()
                stpSaisie.Visibility = Visibility.Collapsed
                stpSaisieDetails.Visibility = Visibility.Collapsed
                btnNewAg.Visibility = Visibility.Visible
                btpPlus.Content = " > "

            End If
        Else
            stpSaisie.Visibility = Visibility.Collapsed
            stpSaisieDetails.Visibility = Visibility.Collapsed
            btnNewAg.Visibility = Visibility.Visible
            btpPlus.Content = " > "
        End If
        AfficheProfilUser()
    End Sub

    Private Sub BtnAgentListeSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentListeSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()
        'SearchAgent()
    End Sub
    Sub SearchAgent(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        Dim sufix As String = ""
        If KeyWord = "" Then
            sufix = " "
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = ""
        Else
            sufix = ""
        End If
        Dim istr As String = " SELECT NomAgent, PrenomAgent, sexe, NumMatr, Contacts, format(DateEntree) as DateEntree,Format(DateSortie) as DateSortie, actif, format(DateNaiss) as [Né(e) le],LieuNaiss,PrefNaiss, VilleResidence as Ville, Quartier, Fonction, CatEch,NbEnf, RefAgent as Ref,Nsecu, Banque,NumCompte,MatrimoSituation, Pays,CarteType,CarteNumero,LieuNaiss,PrefNaiss "
        istr += " FROM  agents "
        istr += " where  "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then istr += " DateEntree>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateEntree<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and "
        istr += " (NomPrenoms  like '%" & Apost(KeyWord) & "%' "
        istr += " or NumMatr  like '%" & Apost(KeyWord) & "%' "
        istr += " or PrenomAgent  like '%" & Apost(KeyWord) & "%' "
        istr += " or NomAgent  like '%" & Apost(KeyWord) & "%' "
        istr += " or Contacts  like '%" & Apost(KeyWord) & "%' "
        istr += " or VilleResidence  like '%" & Apost(KeyWord) & "%' "
        istr += " or Quartier  like '%" & Apost(KeyWord) & "%' "
        istr += " or Fonction  like '%" & Apost(KeyWord) & "%' "
        istr += " or CatEch  like '%" & Apost(KeyWord) & "%' "
        istr += " or LieuNaiss  like '%" & Apost(KeyWord) & "%' "
        istr += " or PrefNaiss  like '%" & Apost(KeyWord) & "%' "
        istr += " or RefAgent  like '%" & Apost(KeyWord) & "%') " & sufix ' BtnAgentListeSearch.Text

        If AffActif Then
            If iActif And Not iInActif Then
                istr += " and Actif=1"
            ElseIf Not iActif And iInActif Then
                istr += " and Actif=0"
            Else
                istr += " and Actif=2"
            End If
        End If
        istr += " ORDER BY actif desc, Nomprenoms " '& sufix
        Dim agentData = LoadDataTable(istr, LocalConnxStr)
        DgAgentListe.ItemsSource = agentData.DefaultView
        tblNbAgent.Text = "Agents actifs: " & agentData.Select("actif = 1").Length & " | Agents inactifs: " & agentData.Select("actif = 0").Length & " | TOTAL: " & agentData.Rows.Count
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Async Sub btnSaveContinue_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveContinue.Click
        If dtpDateEmbauche.SelectedDate.HasValue AndAlso dtpDateEmbauche.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDateEmbauche.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDateEmbauche.Focus()
                Exit Sub
            End If
        End If
        If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If NewData Then
                Try
                    If SaveNewAgent() Then
                        MsgBox("Données enregistrer avec succès")
                        Await RefreshAfterDatabaseMutationAsync(Sub()
                                                                    InitializeMyFields()
                                                                    SearchAgent(BtnAgentListeSearch.Text)
                                                                End Sub)
                        'stpSaisie.Visibility = Visibility.Collapsed
                        AfficheStat?.Invoke()
                    Else
                        MsgBox("Echec d'enregistrement de données")
                        'stpSaisie.Visibility = Visibility.Visible
                        'btnNewAg.Visibility = Visibility.Collapsed
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            Else
                Try
                    If SaveModifAgent() Then
                        MsgBox("Données modifiées avec succès")
                        Await RefreshAfterDatabaseMutationAsync(Sub()
                                                                    InitializeMyFields()
                                                                    SearchAgent(BtnAgentListeSearch.Text)
                                                                End Sub)
                        'stpSaisie.Visibility = Visibility.Collapsed
                        'btnNewAg.Visibility = Visibility.Visible
                    Else
                        MsgBox("Echec d'enregistrement de données")
                        'stpSaisie.Visibility = Visibility.Visible
                        'btnNewAg.Visibility = Visibility.Collapsed
                    End If

                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            End If

        End If

    End Sub

    Private Sub ChbActif_Checked(sender As Object, e As RoutedEventArgs) Handles ChbActif.Checked
        ActivSaveBtn()
    End Sub

    Private Sub btpPlus_Click(sender As Object, e As RoutedEventArgs) Handles btpPlus.Click
        If btpPlus.Content = " > " Then
            btpPlus.Content = " < "
            stpSaisieDetails.Visibility = Visibility.Visible
        Else
            btpPlus.Content = " > "
            stpSaisieDetails.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub TbxPays_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxPays.TextChanged
        SaisieMajusculeTbx(sender)
        SaisiTbx(sender, TblIndicationPays)
    End Sub

    Private Sub TbxNCNSS_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxNCNSS.TextChanged
        SaisiTbx(sender, TblIndicationNCNSS)
    End Sub

    Private Sub CbxTypeCarte_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxTypeCarte.SelectionChanged
        SaisiCbx(sender, TblIndicationTypeCarte)
        If CbxTypeCarte.SelectedIndex = -1 Then
            CartValue = ""
        Else
            CartValue = CbxTypeCarte.SelectedItem
        End If
    End Sub

    Private Sub TbxNunCarte_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxNunCarte.TextChanged
        SaisiTbx(sender, TblIndicationNunCarte)
    End Sub

    Private Sub CbxBanque_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxBanque.SelectionChanged
        SaisiCbx(sender, TblIndicationBanque)
        If CbxBanque.SelectedIndex = -1 Then
            BankValue = ""
        Else
            BankValue = CbxBanque.SelectedItem
        End If
    End Sub

    Private Sub CbxSituationMatrim_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxSituationMatrim.SelectionChanged
        SaisiCbx(sender, TblIndicationSitua)
    End Sub

    Private Sub TbxNunBank_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxNunBank.TextChanged
        SaisiTbx(sender, TblIndicationNunBank)
    End Sub

    Private Sub DgAgentListe_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgAgentListe.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgAgentListe.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If rowView.Row.ItemArray(0).ToString <> "" Then
                RefAg = Convert.ToInt32(rowView.Row.ItemArray(16))
                NewNomPremn = MyStatValStr("select NomPrenoms from agents where RefAgent=" & RefAg, iConnxStr)
                If String.IsNullOrWhiteSpace(NewNomPremn) Then
                    MsgBox("Impossible de retrouver le nom de référence de cet agent.")
                    Return
                End If
                iFormLoaded = True
                TbxNom.Text = rowView.Row.ItemArray(0).ToString
                TbxPrenom.Text = rowView.Row.ItemArray(1).ToString
                'MsgBox(rowView.Row.ItemArray(2).ToString)
                cbxSexe.SelectedItem = rowView.Row.ItemArray(2).ToString
                TbxContact.Text = rowView.Row.ItemArray(4).ToString
                'TbxObs.Text = rowView.Row.ItemArray(5).ToString
                dtpDateEmbauche.SelectedDate = rowView.Row.ItemArray(5).ToString
                ChbActif.IsChecked = rowView.Row.ItemArray(7).ToString
                dtpDateNaiss.SelectedDate = rowView.Row.ItemArray(8).ToString
                '-------------------------------
                TbxLieuNaiss.Text = rowView.Row.ItemArray(9).ToString
                TbxPrefNaiss.Text = rowView.Row.ItemArray(10).ToString
                '-------------------------------
                TbxVille.Text = rowView.Row.ItemArray(11).ToString
                TbxAdresse.Text = rowView.Row.ItemArray(12).ToString
                CbxFonction.SelectedItem = rowView.Row.ItemArray(13).ToString
                TbxCSP.Text = rowView.Row.ItemArray(14).ToString
                TbxNbEnf.Text = rowView.Row.ItemArray(15).ToString
                TbxNCNSS.Text = rowView.Row.ItemArray(17).ToString
                TbxNumMaticule.Text = rowView.Row.ItemArray(3).ToString
                NewNutricule = rowView.Row.ItemArray(3).ToString
                DateSortie = MyStatValStr("select a.DateSortie from agents a  where a.NumMatr='" & TbxNumMaticule.Text & "'")
                If rowView.Row.ItemArray(18).ToString <> "" Then
                    CbxBanque.SelectedItem = rowView.Row.ItemArray(18).ToString
                Else
                    CbxBanque.SelectedIndex = -1
                End If
                TbxNunBank.Text = rowView.Row.ItemArray(19).ToString
                If rowView.Row.ItemArray(20).ToString <> "" Then
                    CbxSituationMatrim.SelectedItem = rowView.Row.ItemArray(20).ToString
                Else
                    CbxSituationMatrim.SelectedIndex = -1
                End If
                TbxPays.Text = rowView.Row.ItemArray(21).ToString
                If rowView.Row.ItemArray(22).ToString <> "" Then
                    CbxTypeCarte.SelectedItem = rowView.Row.ItemArray(22).ToString
                Else
                    CbxTypeCarte.SelectedIndex = -1
                End If
                TbxNunCarte.Text = rowView.Row.ItemArray(23).ToString

                stpSaisie.Visibility = Visibility.Visible
                btnDeleteAg.Visibility = Visibility.Visible

                NewData = False
                btnSave.Content = "Modifier et fermer"
                btnSaveContinue.Content = "Modifier et Continuer"
                btnDeleteAg.Visibility = Visibility.Visible
                btnNewAg.Visibility = Visibility.Collapsed
                iFormLoaded = True
                AfficheProfilUser()
            Else
                stpSaisie.Visibility = Visibility.Collapsed
                btnDeleteAg.Visibility = Visibility.Collapsed
                btnNewAg.Visibility = Visibility.Visible
                NewData = True
                btnSave.Content = "Enregistrer et fermer"
                btnSaveContinue.Content = "Enregistrer et ajouter un autre"
                AfficheProfilUser()
            End If
        End If

    End Sub

    Private Async Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgAgentListe.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefAg = rowView.Row.ItemArray(14).ToString
            If MessageBox.Show("Voulez supprimer définitivement l'agent « " & rowView.Row.ItemArray(0).ToString & " " & rowView.Row.ItemArray(1).ToString & " » ainsi que ses contrats, affectations, absences, heures supplémentaires, sorties, plannings et références d'événements ? Une agence dont il est responsable devra d'abord être réaffectée.",
                               "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                If Not ConfirmAdminPassword(System.Windows.Window.GetWindow(Me),
                                            "la suppression de l'agent « " & rowView.Row.ItemArray(0).ToString & " " & rowView.Row.ItemArray(1).ToString & " » et de ses données liées",
                                            iConnxStr) Then Return
                If DeleteAgent() Then
                    MsgBox("L'agent et ses données liées ont été supprimés avec succès.")
                    Await RefreshAfterDatabaseMutationAsync(Sub()
                                                                InitializeMyFields()
                                                                SearchAgent(BtnAgentListeSearch.Text)
                                                            End Sub)
                End If
            End If

        End If
    End Sub

    Private Sub BtnAgentListeSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentListeSearch.KeyDown
        If e.Key = Key.Enter Then
            elpStatus.Visibility = Visibility.Visible
            inputTimer.Stop()
            SearchAgent(BtnAgentListeSearch.Text)
        End If
    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then
            inputTimer.Stop()
            elpStatus.Visibility = Visibility.Visible
            SearchAgent(BtnAgentListeSearch.Text)
        End If
    End Sub

    Private Async Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        'If SaveNewData("delete from Agents  where RefAgent=" & RefAg) Then InitialLoad()
        If MessageBox.Show("Voulez supprimer définitivement l'agent « " & NomPrenomAg & " » ainsi que ses contrats, affectations, absences, heures supplémentaires, sorties, plannings et références d'événements ? Une agence dont il est responsable devra d'abord être réaffectée.",
                           "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If Not ConfirmAdminPassword(System.Windows.Window.GetWindow(Me),
                                        "la suppression de l'agent « " & NomPrenomAg & " » et de ses données liées",
                                        iConnxStr) Then Return
            If DeleteAgent() Then
                MsgBox("L'agent et ses données liées ont été supprimés avec succès.")
                AfficheStat?.Invoke()
                Await RefreshAfterDatabaseMutationAsync(Sub()
                                                            InitializeMyFields()
                                                            SearchAgent(BtnAgentListeSearch.Text)
                                                        End Sub)
            End If
        End If

    End Sub
    Function DeleteAgent() As Boolean
        DeleteAgent = False
        Dim agentName As String = Nothing
        Dim matricule As String = Nothing
        Try
            Using connection As New SQLiteConnection(ResolveMainDatabaseConnectionString(iConnxStr, True))
                connection.Open()
                Using transaction As SQLiteTransaction = connection.BeginTransaction()
                    Using command As New SQLiteCommand("SELECT NomPrenoms, NumMatr FROM Agents WHERE RefAgent = @id", connection, transaction)
                        command.Parameters.AddWithValue("@id", RefAg)
                        Using reader As SQLiteDataReader = command.ExecuteReader()
                            If reader.Read() Then
                                agentName = reader.GetString(0)
                                matricule = reader.GetString(1)
                            End If
                        End Using
                    End Using
                    If String.IsNullOrWhiteSpace(agentName) OrElse String.IsNullOrWhiteSpace(matricule) Then
                        MessageBox.Show("L'agent sélectionné est introuvable.", "Suppression de l'agent", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Return False
                    End If

                    Using command As New SQLiteCommand(
                        "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND lower(name) = 'agences'",
                        connection,
                        transaction)
                        If Convert.ToInt32(command.ExecuteScalar()) > 0 Then
                            Using agencyCommand As New SQLiteCommand("SELECT COUNT(*) FROM Agences WHERE Responsable = @agent", connection, transaction)
                                agencyCommand.Parameters.AddWithValue("@agent", agentName)
                                If Convert.ToInt32(agencyCommand.ExecuteScalar()) > 0 Then
                                    Throw New InvalidOperationException("Cet agent est responsable d'une ou plusieurs agences. Réaffectez d'abord le responsable dans la gestion des agences, puis réessayez. Les agences et leurs sites ne seront pas supprimés.")
                                End If
                            End Using
                        End If
                    End Using

                    Using command As New SQLiteCommand(
                        "SELECT COUNT(*) FROM Events WHERE instr(AgentsEvt, @agent) > 0 AND trim(AgentsEvt) <> @agent",
                        connection,
                        transaction)
                        command.Parameters.AddWithValue("@agent", agentName)
                        If Convert.ToInt32(command.ExecuteScalar()) > 0 Then
                            Throw New InvalidOperationException("Un événement contient le nom de cet agent avec d'autres informations. Corrigez cette référence manuellement avant de relancer la suppression.")
                        End If
                    End Using

                    Dim dependentDeletes As String() = {
                        "DELETE FROM Absences WHERE NomPrenom = @agent",
                        "DELETE FROM Affectation WHERE AgentAffect = @agent",
                        "DELETE FROM ContratsTbl WHERE NomPrenoms = @matricule",
                        "DELETE FROM HeureSup WHERE Agent = @agent",
                        "DELETE FROM sortie WHERE NomsPrenoms = @agent",
                        "DELETE FROM PlanPoste WHERE Agent = @agent"
                    }
                    For Each deleteStatement As String In dependentDeletes
                        Using command As New SQLiteCommand(deleteStatement, connection, transaction)
                            If deleteStatement.Contains("@agent") Then command.Parameters.AddWithValue("@agent", agentName)
                            If deleteStatement.Contains("@matricule") Then command.Parameters.AddWithValue("@matricule", matricule)
                            command.ExecuteNonQuery()
                        End Using
                    Next

                    Using command As New SQLiteCommand("UPDATE Events SET AgentsEvt = '' WHERE trim(AgentsEvt) = @agent", connection, transaction)
                        command.Parameters.AddWithValue("@agent", agentName)
                        command.ExecuteNonQuery()
                    End Using

                    Using command As New SQLiteCommand("DELETE FROM Agents WHERE RefAgent = @id", connection, transaction)
                        command.Parameters.AddWithValue("@id", RefAg)
                        If command.ExecuteNonQuery() = 0 Then
                            Throw New InvalidOperationException("L'agent sélectionné n'a pas été supprimé.")
                        End If
                    End Using

                    transaction.Commit()
                End Using
            End Using
            If Not LocalModeBb Then NotifyGeneralDatabaseChanged()
            Joper("DeleteAgent at " & RefAg, iConnxStr)
            DeleteAgent = True
        Catch ex As SQLiteException
            MessageBox.Show("La suppression de l’agent et de ses données liées a échoué : " & ex.Message,
                            "Suppression de l’agent", MessageBoxButton.OK, MessageBoxImage.Error)
        Catch ex As InvalidOperationException
            MessageBox.Show(ex.Message, "Suppression de l’agent", MessageBoxButton.OK, MessageBoxImage.Warning)
        Catch ex As ArgumentException
            MessageBox.Show("La suppression de l’agent et de ses données liées a échoué : " & ex.Message,
                            "Suppression de l’agent", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try

    End Function

    Sub ActivSaveBtn()
        If Not IsNothing(TbxCSP) Then
            If NomPrenomAg.Length > 3 And RefAg > 0 And IsNumeric(TbxNbEnf.Text) And TbxNom.Text.Trim.Length > 1 And TbxPrenom.Text.Trim.Length > 1 And TbxNumMaticule.Text.Trim.Length > 4 And TbxVille.Text.Trim.Length > 1 And TbxAdresse.Text.Trim.Length > 1 And TbxCSP.Text.Trim.Length > 1 And cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
                btnSave.Visibility = Visibility.Visible
                btnSaveContinue.Visibility = Visibility.Visible
            Else
                btnSave.Visibility = Visibility.Collapsed
                btnSaveContinue.Visibility = Visibility.Collapsed
            End If
        End If
        'AfficheProfilUser()
        ChangeYN = True
    End Sub

    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs) Handles cmiModif.Click
        Dim rowView As DataRowView = TryCast(DgAgentListe.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Entrées/embauches "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Entrées/embauches du " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgAgentListe, "Point_Entrées", iTitre) '"Point_Entrees"

    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        Try
            InitialLoad()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        btnNewAg.Visibility = Visibility.Visible
    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des ENTREES-AGENTS", "A ce jour " & Today, Me)
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

    Private Sub btnPointG_Click(sender As Object, e As RoutedEventArgs) Handles btnPointG.Click
        Dim debut As Date = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy"))
        Dim Fin As Date = CDate("24/" & Today.ToString("MM/yyyy"))
        If dtpDebutSearch.SelectedDate.HasValue Then debut = dtpDebutSearch.SelectedDate
        If dtpFinSearch.SelectedDate.HasValue Then Fin = dtpFinSearch.SelectedDate
        Dim istr As String = "SELECT s.Client, format(c.DebutContr) DebutContr, format(c.FinContr) FinContr, a.NumMatr, a.Nsecu, a.NomPrenoms, format(a.DateNaiss) DateNaiss, a.Sexe, a.CatEch, a.Fonction, a.NbEnf, aff.ObsAffect "
        istr &= " FROM (Agents a JOIN contratstbl c ON a.NumMatr = c.NomPrenoms) JOIN affectation aff ON aff.AgentAffect = a.NomPrenoms JOIN SiteAffectation s ON s.NomSite = aff.SiteAffect "
        istr &= " WHERE c.DebutContr >='" & debut.ToString("yyyy-MM-dd") & "' and c.DebutContr <='" & Fin.ToString("yyyy-MM-dd") & "'"   ' date('now','-1 month','start of month','+24 day')  "
        istr &= " and c.FinContr >='" & DebutDuMois(Today).ToString("yyyy-MM-dd") & "'"  'date('now','-1 month','start of month','+24 day')"
        istr &= " and aff.DateAffect >='" & debut.ToString("yyyy-MM-dd") & "'"
        istr &= " and  aff.FinAffect >='" & debut.ToString("yyyy-MM-dd") & "'"
        istr &= " ORDER BY s.Client, a.NomPrenoms, aff.SiteAffect"
        'ShowTempMsg(istr)
        openPointMensuel(debut, Fin, istr, "ENTREES du " & debut.ToString("dd/MM/yyyy") & " au " & Fin.ToString("dd/MM/yyyy"), "ENTREES")

    End Sub

    Public Function MaxIndexNumMatricul(TypeContrat As String) As Integer
        MaxIndexNumMatricul = 0
        Dim iStr As String = ""

        If TypeContrat.Contains("stag") Then
            iStr += "select max( substr(a.NumMatr,12,4)) from agents a where a.NumMatr like '%stag%'"
        Else
            iStr += "select max( substr(a.NumMatr,9,4)) from agents a where a.NumMatr like '%gs%'"
        End If
        MaxIndexNumMatricul = MayStatVal(iStr)
    End Function

    Private Sub AgActif_Checked(sender As Object, e As RoutedEventArgs) Handles AgActif.Checked
        iActif = True
        testClientActif()
    End Sub

    Private Sub AgActif_Unchecked(sender As Object, e As RoutedEventArgs) Handles AgActif.Unchecked
        iActif = False
        testClientActif()
    End Sub

    Private Sub AgInActif_Checked(sender As Object, e As RoutedEventArgs) Handles AgInActif.Checked
        iInActif = True
        testClientActif()
    End Sub

    Private Sub AgInActif_Unchecked(sender As Object, e As RoutedEventArgs) Handles AgInActif.Unchecked
        iInActif = False
        testClientActif()
    End Sub
    Sub testClientActif()
        If iActif And iInActif Then
            AffActif = False
        Else
            AffActif = True
        End If
        If iFormLoaded Then SearchAgent(BtnAgentListeSearch.Text)
    End Sub

    Private Sub TbxLieuNaiss_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxLieuNaiss.TextChanged
        SaisiTbx(sender, TblIndicationLieuNaiss)
    End Sub

    Private Sub TbxNunPrefNaiss_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxPrefNaiss.TextChanged
        SaisiTbx(sender, TblIndicationPrefNais)
    End Sub

    Public Function NumMatrDefault(TypeContrat As String) As String
        NumMatrDefault = ""
        Dim iStr As String = ""
        Dim iIndex As Integer = 0
        maj()

        If LCase(TypeContrat).Contains("stag") And iFormLoaded Then
            iStr += "select max( substr(a.NumMatr,12,4)) from agents a where a.NumMatr like '%stag%'"
            iIndex = MayStatVal(iStr, iConnxStr) + 1
            NumMatrDefault = Today.Year & "/STAGE-" & Right("0000" & iIndex, 3)
        Else
            iStr += "select max( substr(a.NumMatr,9,4)) from agents a where a.NumMatr like '%gs%'"
            iIndex = MayStatVal(iStr, iConnxStr) + 1
            NumMatrDefault = Today.Year & "/GS-" & Right("0000" & iIndex, 3)
        End If

    End Function
    Sub AfficheProfilUser()
        If Not CanAddDataOrdinary Then
            btnNewAg.Visibility = Visibility.Collapsed
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
        End If
    End Sub

End Class
