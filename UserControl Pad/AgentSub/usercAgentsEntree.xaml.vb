Imports System.Windows.Threading
Imports System.Data.SQLite
Imports System.Data

Public Class usercAgentsEntree
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
        iConnxStr = If(LocalModeBb, LocalConnxStr, "Data Source=" & DbPathGeneral & ";Version=3;")
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
        DgAgentListe.ItemsSource = LoadDataTable(istr, iConnxStr).DefaultView
        tblNbAgent.Text = "Agents actifs: " & MayStatVal("select count(*) from (" & istr & ") where actif=1", iConnxStr) & " | Agents inactifs: " & MayStatVal("select count(*) from (" & istr & ")  where actif=0", iConnxStr) & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")", iConnxStr)

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

        maj()
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

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
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
                        InitializeMyFields()
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
                        InitializeMyFields()
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
        SaveNewAgent = SaveNewData(istr, iConnxStr)
        If SaveNewAgent Then Joper("SaveNewAgent As " & NomPrenomAg, iConnxStr)
    End Function
    Function SaveModifAgent() As Boolean
        Dim istr As String = " update Agents  set "
        istr += " NomPrenoms='" & Trim(Apost(NomPrenomAg)) & "',NomAgent='" & Trim(Apost(TbxNom.Text)) & "',PrenomAgent='" & Trim(Apost(TbxPrenom.Text)) & "',NumMatr='" & Trim(Apost(TbxNumMaticule.Text)) & "',Contacts='"
        istr += Trim(Apost(TbxContact.Text)) & "',DateNaiss='" & dtpDateNaiss.SelectedDate.Value.ToString("yyyy-MM-dd") & "',Sexe='" & Apost(cbxSexe.SelectedItem) & "',CatEch='" & Trim(Apost(TbxCSP.Text)) & "',NbEnf=" & TbxNbEnf.Text & ", Actif="
        istr += ChbActif.IsChecked & ",DateEntree='" & dtpDateEmbauche.SelectedDate.Value.ToString("yyyy-MM-dd") & "',VilleResidence='" & Apost(TbxVille.Text) & "',Quartier='" & Apost(TbxAdresse.Text) & "',Fonction='" & Apost(CbxFonction.SelectedItem) & "',OpTime='" & Now.ToString("yyyy-MM-dd HH:mm:ss") & "',Pays='"
        istr += Trim(Apost(TbxPays.Text)) & "',MatrimoSituation='" & Apost(CbxSituationMatrim.SelectedItem) & "',Nsecu='" & Trim(Apost(TbxNCNSS.Text)) & "',CarteType='" & Trim(Apost(CartValue)) & "',CarteNumero='" & Trim(Apost(TbxNunCarte.Text)) & "',Banque='" & Trim(Apost(BankValue)) & "',NumCompte='" & Trim(Apost(TbxNunBank.Text)) & "',"
        istr += "LieuNaiss='" & Trim(Apost(TbxLieuNaiss.Text)) & "',PrefNaiss='" & Trim(Apost(TbxPrefNaiss.Text)) & "'"
        If ChbActif.IsChecked And DateSortie <> "" Then
            If MsgBox("Vous êtes sur le point de ACTIVER L'AGENT. " & DateSortie & Chr(13) & " CONTINUER ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then istr += ", DateSortie=null "
        End If
        istr += " where RefAgent=" & RefAg & " "
        'MsgBox(istr)
        If isLocalModeDd() Then MsgBox("Vous travaillez en MODE LOCAL => " & iConnxStr)
        SaveModifAgent = SaveNewData(istr, iConnxStr) 'NewNomPremn = NomPrenomAg
        If SaveModifAgent And NewNutricule <> TbxNumMaticule.Text Then
            SaveModifAgent = SaveNewData("update contratstbl set NomPrenoms='" & Apost(TbxNumMaticule.Text) & "' where NomPrenoms='" & NewNutricule & "'", iConnxStr)
        End If
        If SaveModifAgent And NewNomPremn <> NomPrenomAg Then
            'SaveModifAgent = SaveNewData("update Affectation set AgentAffect='" & Apost(TbxNumMaticule.Text) & "' where AgentAffect='" & Apost(NewNomPremn) & "'")
            SaveModifAgent = SaveNewData("update PlanPoste set Agent='" & Apost(NomPrenomAg) & "' where Agent='" & Apost(NewNomPremn) & "'", iConnxStr)
            'SaveModifAgent = SaveNewData("update Postes set NomSite='" & Apost(TbxSite.Text) & "' where NomSite='" & Apost(NewNom) & "'")
            SaveModifAgent = SaveNewData("update sortie set NomsPrenoms='" & Apost(NomPrenomAg) & "' where NomsPrenoms='" & Apost(NewNomPremn) & "'", iConnxStr)
            SaveModifAgent = SaveNewData("update HeureSup set Agent='" & Apost(NomPrenomAg) & "' where Agent='" & Apost(NewNomPremn) & "'", iConnxStr)
            SaveModifAgent = SaveNewData("update Affectation set AgentAffect='" & Apost(NomPrenomAg) & "' where AgentAffect='" & Apost(NewNomPremn) & "'", iConnxStr)
            SaveModifAgent = SaveNewData("update Absences set NomPrenom='" & Apost(NomPrenomAg) & "' where NomPrenom='" & Apost(NewNomPremn) & "'", iConnxStr)
            'SaveModifAgent = SaveNewData("update Affectation set AgentAffect='" & Apost(NomPrenomAg) & "' where SiteAffect='" & Apost(NewNomPremn) & "'")
            If SaveModifAgent Then Joper("SaveModifAgent As " & NewNomPremn & " at " & RefAg, iConnxStr)
        End If
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
        istr += " (NomPrenoms  like '%" & KeyWord & "%' "
        istr += " or NumMatr  like '%" & KeyWord & "%' "
        istr += " or PrenomAgent  like '%" & KeyWord & "%' "
        istr += " or NomAgent  like '%" & KeyWord & "%' "
        istr += " or Contacts  like '%" & KeyWord & "%' "
        istr += " or VilleResidence  like '%" & KeyWord & "%' "
        istr += " or Quartier  like '%" & KeyWord & "%' "
        istr += " or Fonction  like '%" & KeyWord & "%' "
        istr += " or CatEch  like '%" & KeyWord & "%' "
        istr += " or LieuNaiss  like '%" & KeyWord & "%' "
        istr += " or PrefNaiss  like '%" & KeyWord & "%' "
        istr += " or RefAgent  like '%" & KeyWord & "%') " & sufix ' BtnAgentListeSearch.Text
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
        DgAgentListe.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        tblNbAgent.Text = "Agents actifs: " & MayStatVal("select count(*) from (" & istr & ") where actif=1", LocalConnxStr) & " | Agents inactifs: " & MayStatVal("select count(*) from (" & istr & ") where actif=0", LocalConnxStr) & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")", LocalConnxStr)
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnSaveContinue_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveContinue.Click
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
                        InitializeMyFields()
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
                        InitializeMyFields()
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
                iFormLoaded = True
                TbxNom.Text = rowView.Row.ItemArray(0).ToString
                TbxPrenom.Text = rowView.Row.ItemArray(1).ToString
                NewNomPremn = NomPrenomAg
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
                RefAg = rowView.Row.ItemArray(16).ToString
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

    Private Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgAgentListe.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefAg = rowView.Row.ItemArray(14).ToString
            If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & rowView.Row.ItemArray(0).ToString & " " & rowView.Row.ItemArray(1).ToString & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                If DeleteAgent() Then
                    MsgBox("Donnée supprimées avec succès ")
                    InitializeMyFields()
                Else
                    MsgBox("Erreur de suppression")
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

    Private Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        'If SaveNewData("delete from Agents  where RefAgent=" & RefAg) Then InitialLoad()
        If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & NomPrenomAg & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If DeleteAgent() Then
                MsgBox("Donnée supprimées avec succès ")
                AfficheStat?.Invoke()
                InitializeMyFields()
            Else
                MsgBox("Erreur de suppression")
            End If
        End If

    End Sub
    Function DeleteAgent() As Boolean
        DeleteAgent = False
        Dim istr As String = " delete from Agents "
        istr += " where RefAgent=" & RefAg & " "
        DeleteAgent = SaveNewData(istr, iConnxStr)
        If DeleteAgent Then Joper("DeleteAgent at " & RefAg, iConnxStr)

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
