Imports System.Windows.Threading
Imports System.Data
Public Class usercClientsSite
    Public iConnxStr As String
    Public LocalConnxStr As String
    Dim ChangeYN As Boolean = False
    Dim iActif As Boolean = True
    Dim iInActif As Boolean = False
    Dim AffActif As Boolean = True
    Dim RefCrt As Integer = 0
    Dim inputTimer As DispatcherTimer
    Dim iFormLoaded As Boolean = False
    Dim iCbxSiteName As New ComboBox
    Dim NewData As Boolean = True
    Dim NewNom As String = ""
    Dim retourDansMainWindow As Action
    Public Shared AfficheStat As Action
    Public Sub New(clb As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = clb
    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        DbPathGeneral = PaphBackUp()
        iConnxStr = "Data Source=" & DbPathGeneral & ";Version=3;"
        LocalConnxStr = $"Data Source={DbPath};Version=3;"
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
        Dim istr As String = " SELECT s.NomSite, s.Actif, s.Localisation,s.AdressSite as Adresse, s.Duree, s.TypeSite as Type, s.RespSite as Responsable, format(s.DateCreation) as DateCreation, format(s.DateFermeture) as DateFermeture, s.Client, s.Observations,s.IdSite  "
        istr += " from siteaffectation s "
        istr += " where S.NomSite like '%' and s.Actif=1"
        istr += " order by s.Actif desc,S.NomSite "
        DgClientSite.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        tblNbClientSiteActifs.Text = "Nombre de sites actifs: " & MayStatVal("select count(*) from siteaffectation where Actif=1 ", LocalConnxStr) & " | Nombre de sites fermés: " & MayStatVal("select count(*) from siteaffectation where Actif=0 ", LocalConnxStr)

        FilmyCbx("select c.NomClient from client c where c.Actif=1 order by c.Favorit desc,c.NomClient", CbxClient, LocalConnxStr)
        FilmyCbx("select lower(s.NomSite) from siteaffectation s ", iCbxSiteName, LocalConnxStr)
        FilmyCbx("select t.TypeSite from typesite t where t.Actif=1 order by t.Favorit desc,t.TypeSite desc", CbxTypeSite, LocalConnxStr)
        FilmyCbx("select d.Duree from dureerotation d where d.Actif=1 order by d.Favorit desc,d.Duree ", CbxDuree, LocalConnxStr)
        RefCrt = MayStatVal("select max(c.IdSite) from siteaffectation c", LocalConnxStr) + 1
        'MsgBox(RefCrt)
        dtpDebut.SelectedDate = Today
        dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpDebutSearch.SelectedDate = DateCreationSociete 'CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = Today
        iFormLoaded = True
        ChangeYN = False
        AfficheProfilUser()
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Sub ResetValues()
        elpStatus.Visibility = Visibility.Visible

        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed
        maj()
        AfficheStat?.Invoke()
        Dim istr As String = " SELECT s.NomSite, s.Actif, s.Localisation,s.AdressSite as Adresse, s.Duree, s.TypeSite as Type, s.RespSite as Responsable, format(s.DateCreation) as DateCreation, format(s.DateFermeture) as DateFermeture, s.Client, s.Observations,s.IdSite  "
        istr += " from siteaffectation s "
        istr += " where S.NomSite like '%' "
        istr += " order by s.Actif desc,S.NomSite "
        DgClientSite.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        tblNbClientSiteActifs.Text = "Nombre de sites actifs: " & MayStatVal("select count(*) from siteaffectation where Actif=1 ", LocalConnxStr) & " | Nombre de sites fermés: " & MayStatVal("select count(*) from siteaffectation where Actif=0 ", LocalConnxStr)

        FilmyCbx("select lower(s.NomSite) from siteaffectation s ", iCbxSiteName, LocalConnxStr)
        RefCrt = MayStatVal("select max(c.IdSite) from siteaffectation c", LocalConnxStr) + 1

        dtpDebut.SelectedDate = Today
        'dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        tbxListeSite.Text = ""
        tbxListeSite.Visibility = Visibility.Collapsed
        TbxSite.Text = ""
        TbxSite.IsReadOnly = False
        CbxClient.IsReadOnly = False
        TbxReponsable.Text = ""
        TbxAdresse.Text = ""
        TbxVille.Text = ""
        TbxFin.Text = ""
        'TbxMontant.Text = ""
        'TbxNom.Text = ""
        TbxObs.Text = ""
        CbxTypeSite.SelectedIndex = -1
        CbxDuree.SelectedIndex = -1
        CbxClient.SelectedIndex = -1
        'CbxTypCrt.SelectedIndex = -1
        CltActif.IsChecked = True
        CltInActif.IsChecked = False
        iFormLoaded = False
        dtpDebutSearch.SelectedDate = DateCreationSociete 'CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = Today


        ChangeYN = False
        iFormLoaded = True
        NewData = True
        AfficheProfilUser()
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub CancelValues()
        elpStatus.Visibility = Visibility.Visible

        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed

        RefCrt = MayStatVal("select max(c.IdSite) from siteaffectation c", LocalConnxStr) + 1

        dtpDebut.SelectedDate = Today
        'dtpDebut.DisplayDateStart = DateCreationSociete
        dtpDebut.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        tbxListeSite.Text = ""
        tbxListeSite.Visibility = Visibility.Collapsed
        TbxSite.Text = ""
        TbxSite.IsReadOnly = False
        CbxClient.IsReadOnly = False
        TbxReponsable.Text = ""
        TbxAdresse.Text = ""
        TbxVille.Text = ""
        TbxFin.Text = ""
        'TbxMontant.Text = ""
        'TbxNom.Text = ""
        TbxObs.Text = ""
        CbxTypeSite.SelectedIndex = -1
        CbxDuree.SelectedIndex = -1
        CbxClient.SelectedIndex = -1
        'CbxTypCrt.SelectedIndex = -1
        AfficheProfilUser()

        NewData = True
        ChangeYN = False
        CltActif.IsChecked = True
        CltInActif.IsChecked = False
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Sub SearchData(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        maj()
        Dim sufix As String = ""
        If KeyWord = "" Then
            'sufix = " and (c.Actif=1 and crt.FinContr>='" & Today.ToString("yyyy-MM-dd") & "')"
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                sufix += " and DateCreation>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateCreation<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "'  "
            Else
                sufix += " and DateCreation>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "' and DateCreation<='" & Today.ToString("yyyy-MM-dd") & "'  "
            End If
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = " "
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                sufix += " and DateCreation>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateCreation<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "'  "
            Else
                sufix += " and DateCreation>='" & DateCreationSociete.ToString("yyyy-MM-dd") & "' and DateCreation<='" & Today.ToString("yyyy-MM-dd") & "'  "
            End If
        Else
            If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
                sufix += " and DateCreation>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and DateCreation<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "'  "
            Else
                sufix += " and DateCreation>='" & CDate("01/01/" & Today.Year).ToString("yyyy-MM-dd") & "' and DateCreation<='" & Today.ToString("yyyy-MM-dd") & "'  "
            End If
        End If
        Dim istr As String = "SELECT s.NomSite, s.Actif, s.Localisation,s.AdressSite as Adresse, s.Duree, s.TypeSite as Type, s.RespSite as Responsable, format(s.DateCreation) as DateCreation, format(s.DateFermeture) as DateFermeture, s.Client, s.Observations,s.IdSite  "
        istr += " from siteaffectation s "
        istr += " where "
        istr += " (s.NomSite like '%" & KeyWord & "%' "
        istr += " or s.Localisation like '%" & KeyWord & "%' "
        istr += " or s.AdressSite like '%" & KeyWord & "%' "
        istr += " or s.Duree like '%" & KeyWord & "%' "
        istr += " or s.TypeSite like '%" & KeyWord & "%' "
        istr += " or s.Client like '%" & KeyWord & "%' "
        istr += " or s.Observations like '%" & KeyWord & "%' "
        istr += " Or s.RespSite Like '%" & KeyWord & "%' )" & sufix
        If AffActif Then
            If iActif And Not iInActif Then
                istr += " and s.Actif=1"
            ElseIf Not iActif And iInActif Then
                istr += " and s.Actif=0"
            Else
                istr += " and s.Actif=2"
            End If

        End If
        istr += " ORDER BY s.Actif desc,S.NomSite "
        DgClientSite.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de sites actifs : " & MayStatVal("select count(*) from (" & istr & ") where Actif=1 ", LocalConnxStr) & " | Nombre de sites inactifs: " & MayStatVal("select count(*) from  (" & istr & ") where Actif=0 ", LocalConnxStr) 'where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")") '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        itext += Environment.NewLine
        tblNbClientSiteActifs.Text = itext '"En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")

        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Function SaveNewSite() As Boolean
        SaveNewSite = False
        Dim istr As String = " insert into siteaffectation "
        istr += "(NomSite, Actif,Localisation,AdressSite, Duree, TypeSite, RespSite,DateCreation,DateFermeture, Client, Observations,IdSite)  "
        istr += " values ('" & Trim(Apost(TbxSite.Text)) & "', "
        istr += " " & ChbActif.IsChecked & ", "
        istr += " '" & Trim(Apost(TbxVille.Text)) & "', "
        istr += " '" & Trim(Apost(TbxAdresse.Text)) & "', "
        istr += " '" & Apost(CbxDuree.SelectedItem) & "', "
        istr += " '" & Apost(CbxTypeSite.SelectedItem) & "', "
        istr += " '" & Trim(Apost(TbxReponsable.Text)) & "', "
        istr += " '" & (dtpDebut.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        istr += " '" & Trim(TbxFin.Text) & "', "
        istr += " '" & Apost(CbxClient.SelectedItem) & "', "
        istr += " '" & Trim(Apost(TbxObs.Text)) & "', "
        istr += " " & RefCrt & ")"
        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveNewSite = SaveNewData(istr, iConnxStr)
        If SaveNewSite Then Joper("SaveNewSite for " & TbxSite.Text & " at " & RefCrt, iConnxStr)
    End Function
    Function SaveModifCtr() As Boolean
        SaveModifCtr = False
        Dim istr As String = " update siteaffectation  set "
        'istr += "(IdHS, Agent, SiteHS, DateHS, NbFaction, Obs) values "
        istr += " NomSite='" & Trim(Apost(TbxSite.Text)) & "', "
        istr += " Actif=" & ChbActif.IsChecked & ", "
        istr += " Localisation='" & Trim(Apost(TbxVille.Text)) & "', "
        istr += " AdressSite='" & Trim(Apost(TbxAdresse.Text)) & "', "
        istr += " Duree='" & Apost(CbxDuree.SelectedItem) & "', "
        istr += " TypeSite='" & Apost(CbxTypeSite.SelectedItem) & "', "
        istr += " RespSite='" & Trim(Apost(TbxReponsable.Text)) & "', "
        istr += " DateCreation='" & (dtpDebut.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        If IsDate(TbxFin.Text) Then istr += " DateFermeture='" & CDate(TbxFin.Text).ToString("yyyy-MM-dd") & "', "
        istr += " Client='" & Apost(CbxClient.SelectedItem) & "', "
        istr += " Observations='" & Trim(Apost(TbxObs.Text)) & "'  "
        istr += " where IdSite=" & RefCrt & " "
        'MsgBox(istr)
        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveModifCtr = SaveNewData(istr, iConnxStr)
        If SaveModifCtr And NewNom <> TbxSite.Text Then
            If SaveNewData("update Affectation set SiteAffect='" & Apost(TbxSite.Text) & "' where SiteAffect='" & Apost(NewNom) & "'", iConnxStr) And SaveNewData("update Affectation set SiteAffectPrecedent='" & Apost(TbxSite.Text) & "' where SiteAffectPrecedent='" & Apost(NewNom) & "'", iConnxStr) Then
                SaveModifCtr = SaveNewData("update Postes set NomSite='" & Apost(TbxSite.Text) & "' where NomSite='" & Apost(NewNom) & "'", iConnxStr)
                SaveModifCtr = SaveNewData("update sortie set SiteSortie='" & Apost(TbxSite.Text) & "' where SiteSortie='" & Apost(NewNom) & "'", iConnxStr)
                SaveModifCtr = SaveNewData("update HeureSup set SiteHS='" & Apost(TbxSite.Text) & "' where SiteHS='" & Apost(NewNom) & "'", iConnxStr)
                'SaveModifCtr = SaveNewData("update HeureSup set SiteHS='" & Apost(TbxSite.Text) & "' where SiteHS='" & Apost(NewNom) & "'")
            End If
        End If
        If ChbActif.IsChecked = False Then
            'MsgBox("update Postes set Actif=0 where NomSite='" & Apost(TbxSite.Text) & "'")
            SaveModifCtr = SaveNewData("update Postes set Actif=0 where NomSite='" & Apost(TbxSite.Text) & "'", iConnxStr)
        End If
        If SaveModifCtr Then Joper("SaveModifSite for " & TbxSite.Text & " at " & RefCrt, iConnxStr)


    End Function

    Sub ActivSaveBtn()
        If NewData Then
            If CbxClient.SelectedIndex >= 0 And TbxSite.Text.Length > 3 Then
                If TbxReponsable.Text.Length > 2 And RefCrt > 0 And Not iCbxSiteName.Items.Contains(LCase(TbxSite.Text)) And TbxAdresse.Text.Length > 1 And TbxVille.Text.Length > 1 And CbxTypeSite.SelectedIndex >= 0 And CbxDuree.SelectedIndex >= 0 And dtpDebut.SelectedDate.HasValue Then ' AndAlso cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
                    btnSave.Visibility = Visibility.Visible
                    btnSaveContinue.Visibility = Visibility.Visible
                    'MsgBox("New")
                Else
                    btnSave.Visibility = Visibility.Collapsed
                    btnSaveContinue.Visibility = Visibility.Collapsed
                End If
            Else
                btnSave.Visibility = Visibility.Collapsed
                btnSaveContinue.Visibility = Visibility.Collapsed
            End If
            'MsgBox("New")
        Else

            'If TbxSite.Text.Length > 3 And TbxReponsable.Text.Length > 2 And RefCrt > 0 And TbxAdresse.Text.Length > 1 And TbxVille.Text.Length > 1 And CbxDuree.Items.Contains(CbxDuree.Text) And dtpDebut.SelectedDate.HasValue Then ' AndAlso cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
            'If CbxClient.Items.Contains(CbxClient.Text) And TbxSite.Text.Length > 3 And TbxReponsable.Text.Length > 2 And RefCrt > 0 And iCbxSiteName.Items.Contains(LCase(TbxSite.Text)) And TbxAdresse.Text.Length > 1 And TbxVille.Text.Length > 1 And CbxTypeSite.Items.Contains(CbxTypeSite.Text) And CbxDuree.Items.Contains(CbxDuree.Text) And dtpDebut.SelectedDate.HasValue Then ' AndAlso cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
            If NewNom <> TbxSite.Text Then
                If CbxClient.Items.Contains(CbxClient.Text) And TbxSite.Text.Length > 3 And TbxReponsable.Text.Length > 2 And RefCrt > 0 And (Not iCbxSiteName.Items.Contains(LCase(TbxSite.Text)) Or NewNom = TbxSite.Text) And TbxAdresse.Text.Length > 1 And TbxVille.Text.Length > 1 And CbxTypeSite.Items.Contains(CbxTypeSite.Text) And CbxDuree.Items.Contains(CbxDuree.Text) And dtpDebut.SelectedDate.HasValue Then
                    btnSave.Visibility = Visibility.Visible
                    btnSaveContinue.Visibility = Visibility.Visible
                    'MsgBox("modif")
                Else
                    btnSave.Visibility = Visibility.Collapsed
                    btnSaveContinue.Visibility = Visibility.Collapsed
                End If

            Else
                If CbxClient.Items.Contains(CbxClient.Text) And TbxSite.Text.Length > 3 And TbxReponsable.Text.Length > 2 And RefCrt > 0 And TbxAdresse.Text.Length > 1 And TbxVille.Text.Length > 1 And CbxTypeSite.Items.Contains(CbxTypeSite.Text) And CbxDuree.Items.Contains(CbxDuree.Text) And dtpDebut.SelectedDate.HasValue Then
                    btnSave.Visibility = Visibility.Visible
                    btnSaveContinue.Visibility = Visibility.Visible
                    'MsgBox("modif")
                Else
                    btnSave.Visibility = Visibility.Collapsed
                    btnSaveContinue.Visibility = Visibility.Collapsed
                End If

            End If
        End If
        ChangeYN = True

    End Sub
    Private Sub CbxClient_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxClient.SelectionChanged
        SaisiCbx(sender, TblIndicationCbxClient)
        ActivSaveBtn()
        If CbxClient.SelectedIndex = -1 Then Exit Sub
        Dim itext As String = ListeSiteClient(CbxClient.SelectedItem)
        Dim iNb As Integer = itext.Length - 2
        If iNb > 0 Then tbxListeSite.Text = Left(itext, iNb)
        If itext = "" Then
            tbxListeSite.Text = ""
            tbxListeSite.Visibility = Visibility.Collapsed
        Else
            tbxListeSite.Visibility = Visibility.Visible
        End If
        TbxSite.Text = CbxClient.SelectedItem
    End Sub

    Private Sub btnNewData_Click(sender As Object, e As RoutedEventArgs) Handles btnNewData.Click
        stpSaisie.Visibility = Visibility.Visible
        btnNewData.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnAnnule_Click(sender As Object, e As RoutedEventArgs) Handles btnAnnule.Click
        If ChangeYN Then
            If MsgBox("ANNULER VRAIMENT ?", MsgBoxStyle.OkCancel, "ATTENTION") = MsgBoxResult.Ok Then
                CancelValues()
                stpSaisie.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
            End If
        Else
            CancelValues()
            stpSaisie.Visibility = Visibility.Collapsed
            btnNewData.Visibility = Visibility.Visible
        End If
        btnDeleteAg.Visibility = Visibility.Collapsed
    End Sub

    Private Sub CbxTypeSite_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxTypeSite.SelectionChanged
        SaisiCbx(sender, TblIndicationTypeSite)
        ActivSaveBtn()
    End Sub

    Private Sub CbxDuree_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxDuree.SelectionChanged
        SaisiCbx(sender, TblIndicationDuree)
        ActivSaveBtn()
    End Sub

    Private Sub TbxSite_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxSite.TextChanged
        'SaisiePremiereLettreMajusculeTbx(sender)
        SaisiTbx(sender, TblIndicationSite)
        ActivSaveBtn()
    End Sub

    Private Sub TbxReponsable_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxReponsable.TextChanged
        SaisiePremiereLettreMajusculeTbx(sender)
        SaisiTbx(sender, TblIndicationResponsable)
        ActivSaveBtn()
    End Sub

    Private Sub TbxAdresse_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxAdresse.TextChanged
        SaisiePremiereLettreMajusculeTbx(sender)
        SaisiTbx(sender, TblIndicationAdresse)
        ActivSaveBtn()
    End Sub

    Private Sub TbxVille_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxVille.TextChanged
        SaisiePremiereLettreMajusculeTbx(sender)
        SaisiTbx(sender, TblIndicationVille)
        ActivSaveBtn()
    End Sub

    Private Sub TbxObs_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxObs.TextChanged
        SaisiTbx(sender, TblIndicationObs)
        ActivSaveBtn()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDebut.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDebut.Focus()
                Exit Sub
            End If
        End If
        If NewData Then
            If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Try
                    If SaveNewSite() Then
                        MsgBox("Données enregistrer avec succès")
                        ResetValues()
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
                ResetValues()
            Else
                MsgBox("Erreur d'enregistrement")
                stpSaisie.Visibility = Visibility.Visible
                btnNewData.Visibility = Visibility.Collapsed
            End If
        End If
    End Sub

    Private Sub btnSaveContinue_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveContinue.Click
        If dtpDebut.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDebut.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDebut.Focus()
                Exit Sub
            End If
        End If
        If NewData Then
            If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Try
                    If SaveNewSite() Then
                        MsgBox("Données enregistrer avec succès")
                        ResetValues()
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
                ResetValues()
            Else
                MsgBox("Erreur d'enregistrement")
                'stpSaisie.Visibility = Visibility.Visible
                'btnNewData.Visibility = Visibility.Collapsed
            End If

        End If
    End Sub

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

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then
            If BtnDataSearch.Text = "" Then
                SearchData("*")
            Else
                SearchData(BtnDataSearch.Text)
            End If
        End If

    End Sub

    Private Sub DgClientSite_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgClientSite.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgClientSite.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If iCbxSiteName.Items.Contains(LCase(rowView.Row.ItemArray(0).ToString)) Then
                'NomSite, Actif,Localisation,AdressSite, Duree, TypeSite, RespSite,DateCreation, Client, Observations,IdSite
                CbxClient.Text = rowView.Row.ItemArray(9).ToString
                TbxSite.Text = rowView.Row.ItemArray(0).ToString
                NewNom = rowView.Row.ItemArray(0).ToString
                'TbxSite.IsReadOnly = True
                ChbActif.IsChecked = rowView.Row.ItemArray(1).ToString
                TbxVille.Text = rowView.Row.ItemArray(2).ToString
                TbxAdresse.Text = rowView.Row.ItemArray(3).ToString
                CbxDuree.SelectedItem = rowView.Row.ItemArray(4).ToString
                CbxTypeSite.SelectedItem = rowView.Row.ItemArray(5).ToString
                TbxReponsable.Text = rowView.Row.ItemArray(6).ToString
                dtpDebut.SelectedDate = rowView.Row.ItemArray(7).ToString
                TbxFin.Text = rowView.Row.ItemArray(8).ToString
                'TbxDescript.Text = rowView.Row.ItemArray(9).ToString
                TbxObs.Text = rowView.Row.ItemArray(10).ToString
                RefCrt = rowView.Row.ItemArray(11).ToString

                stpSaisie.Visibility = Visibility.Visible
                btnDeleteAg.Visibility = Visibility.Visible
                NewData = False
                btnSave.Content = "Modifier et fermer"
                btnSaveContinue.Content = "Modifier et Continuer"
                btnDeleteAg.Visibility = Visibility.Visible
                btnNewData.Visibility = Visibility.Collapsed
                'MsgBox(NewData)
                CbxClient.IsEnabled = True
            Else
                TbxSite.IsReadOnly = False
                CbxClient.IsEnabled = False
                stpSaisie.Visibility = Visibility.Collapsed
                btnDeleteAg.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
                NewData = True
                btnSave.Content = "Enregistrer et fermer"
                btnSaveContinue.Content = "Enregistrer et ajouter un autre"
                'MsgBox("new")
            End If
            'NewData = False

        End If
        AfficheProfilUser()
    End Sub

    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs) Handles cmiModif.Click
        Dim rowView As DataRowView = TryCast(DgClientSite.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub dtpDebut_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebut.SelectedDateChanged
        ActivSaveBtn()
    End Sub
    Function DeleteSite() As Boolean
        DeleteSite = False
        Dim istr As String = " delete from siteaffectation "
        istr += " where IdSite=" & RefCrt & " "
        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL ")
        DeleteSite = SaveNewData(istr, iConnxStr)
        If DeleteSite Then Joper("DeleteSite at " & RefCrt, iConnxStr)
    End Function

    Private Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & TbxSite.Text & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If DeleteSite() Then
                MsgBox("Donnée supprimées avec succès ")
                InialiseLoad()
            Else
                MsgBox("Erreur de suppression")
            End If
        End If
    End Sub

    Private Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgClientSite.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefCrt = rowView.Row.ItemArray(11).ToString
            If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & rowView.Row.ItemArray(0).ToString & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                If DeleteSite() Then
                    MsgBox("Donnée supprimées avec succès ")
                    ResetValues()
                Else
                    MsgBox("Erreur de suppression")
                End If
            End If
        End If

    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Sites "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Sites  du " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgClientSite, "Point_Sites", iTitre) '"Point_Entrees"

    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        InialiseLoad()
    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des Sites", "A ce jour " & Today, Me)
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

    Private Sub ChbActif_Unloaded(sender As Object, e As RoutedEventArgs) Handles ChbActif.Unloaded
        If iFormLoaded Then ActivSaveBtn()
    End Sub

    Private Sub ChbActif_Checked(sender As Object, e As RoutedEventArgs) Handles ChbActif.Checked
        If iFormLoaded Then ActivSaveBtn()
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
