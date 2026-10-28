Imports System.Windows.Threading
Imports System.Data
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
        iConnxStr = If(LocalModeBb, LocalConnxStr, "Data Source=" & DbPathGeneral & ";Version=3;")

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
        tblNbClientContratActifs.Text = "Nombre de clients actifs: " & MayStatVal("select count(*) from Client where Actif=1 ") & " | Nombre de clients inactifs: " & MayStatVal("select count(*) from Client where Actif=0 ", LocalConnxStr)
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

        maj()
        AfficheStat?.Invoke()
        Dim istr As String = " SELECT c.NomClient, c.Actif,c.TypeClient, c.MontantFacturationUnitaire as MontantUnit, c.TypeFacturation,c.Ville,c.Adresse, format(c.DateDebutCollabor) as DateDebut,format(c.DateFinCollabor) as DateFin,c.DescriptionClient,c.Obs,c.RefCtr "
        istr += " from Client c "
        istr += " where c.NomClient like '%' and c.Actif=1 "
        istr += " order by c.Actif desc, c.NomClient "
        DgClientsContrats.ItemsSource = LoadDataTable(istr).DefaultView
        tblNbClientContratActifs.Text = "Nombre de clients actifs: " & MayStatVal("select count(*) from Client where Actif=1 ") & " | Nombre de clients inactifs: " & MayStatVal("select count(*) from Client where Actif=0 ", LocalConnxStr)

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
        DgClientsContrats.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de clients actifs : " & MayStatVal("select count(*) from (" & istr & ") where Actif=1 ", LocalConnxStr) & " | Nombre de clients inactifs: " & MayStatVal("select count(*) from  (" & istr & ") where Actif=0 ", LocalConnxStr) 'where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")") '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
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
        SaveModifCtr = SaveNewData(istr, iConnxStr)
        If SaveModifCtr And NewNom <> TbxNom.Text Then
            SaveModifCtr = SaveNewData("update SiteAffectation set Client='" & Trim(Apost(TbxNom.Text)) & "' where Client='" & Apost(NewNom) & "'", iConnxStr) '113
            If SaveModifCtr Then Joper("SaveModifCtrClient for " & TbxNom.Text & " at " & RefCrt, iConnxStr)
        End If
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
                    If SaveNewContrat() Then
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
                    If SaveNewContrat() Then
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
        Dim istr As String = " delete from Client "
        istr += " where RefCtr=" & RefCrt & " "
        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL ")
        DeleteCtr = SaveNewData(istr, iConnxStr)
        If DeleteCtr Then Joper("DeleteCtrClient at " & RefCrt, iConnxStr)
    End Function


    Private Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & TbxNom.Text & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If DeleteCtr() Then
                MsgBox("Donnée supprimées avec succès ")
                InialiseLoad()
            Else
                MsgBox("Erreur de suppression")
            End If
        End If
    End Sub

    Private Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgClientsContrats.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefCrt = rowView.Row.ItemArray(11).ToString
            If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & rowView.Row.ItemArray(0).ToString & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                If DeleteCtr() Then
                    MsgBox("Donnée supprimées avec succès ")
                    ResetValues()
                Else
                    MsgBox("Erreur de suppression")
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
