Imports System.Windows.Threading
Imports System.Data
Imports System.Data.SQLite
Public Class usercAgentsContrat
    Public iConnxStr As String
    Dim LocalConnxStr As String
    Dim ChangeYN As Boolean = False
    Dim RefCrt As Integer = 0
    Dim inputTimer As DispatcherTimer
    Dim NewData As Boolean = True
    Dim iFormLoaded As Boolean = False
    Dim retourDansMainWindow As Action
    Public Shared AfficheStat As Action
    Public Shared AfficheMemos As Action

    Public Sub New(callback As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        retourDansMainWindow = callback
    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        elpStatus.Visibility = Visibility.Visible
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
        ' Afficher le texte du TextBox
        Dispatcher.Invoke(Sub() SearchContrat(BtnAgentListeSearch.Text))
    End Sub

    Sub InialiseLoad()
        elpStatus.Visibility = Visibility.Visible
        ChangeYN = False
        NewData = True
        AfficheProfilUser()
        maj()
        Dim istr As String = " SELECT ag.NomPrenoms, natc.IdNature as Type,format(crt.DebutContr) as [Début], format(crt.FinContr) as [Fin],  crt.IdContr as Ref, crt.Obs,crt.TypContr as TypeExt,crt.NaturContr as Nature,crt.NumContrat as [N°]"
        istr += " FROM CONTRATSTBL crt join agents ag on ag.NumMatr=crt.NomPrenoms join contratsnature natc on crt.NaturContr=natc.NatureContr "
        istr += " where ag.actif=1 and crt.FinContr>='" & Today.ToString("yyyy-MM-dd") & "'"
        istr += " and crt.DebutContr >='" & CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")).ToString("yyyy-MM-dd") & "'"
        istr += " ORDER BY crt.DebutContr desc, ag.Nomprenoms "
        DgAgentContrat.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'", LocalConnxStr) & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")") '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        itext += Environment.NewLine
        itext += MyContratList("select Type,count(*) from (" & istr & ") group by Type")
        tblNbAgentcONTRAT.Text = itext 'MyContratList("select n.IdNature, count(*) from contratstbl c join contratsnature n on c.NaturContr=n.NatureContr group by n.IdNature")
        'tblNbAgentcONTRAT.Text = "En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & Environment.NewLine & ComptTypeContrat("select Type, count(*) from (" & istr & ") group by Type") & "ok"
        FilmyCbx("select a.NomPrenoms from agents a  where Actif=1 order by a.NomPrenoms", CbxNom, LocalConnxStr)
        FilmyCbx("select a.TypContr from contratstype a  where Actif=1 order by a.Favorit desc, a.TypContr", CbxTypCrt, LocalConnxStr)
        FilmyCbx("select a.NatureContr from contratsnature a  where Actif=1 order by a.Favorit desc, a.NatureContr", CbxNaturCrt, LocalConnxStr)
        'FilmyCbx("select a.NomPrenoms from agents a  where Actif=1 order by a.NomPrenoms", CbxNom)
        RefCrt = MayStatVal("select max(IdContr) from contratstbl ", LocalConnxStr) + 1
        dtpDebutCrt.SelectedDate = Today
        dtpDebutCrt.DisplayDateStart = DateCreationSociete
        dtpDebutCrt.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpFinCrt.SelectedDate = Today
        dtpFinCrt.DisplayDateStart = DateCreationSociete
        dtpFinCrt.DisplayDateEnd = FinDuMois(Today.AddMonths(12))

        dtpDebutSearch.SelectedDate = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy")) ' CDate("01/01/" & Today.Year)
        dtpFinSearch.SelectedDate = CDate("24/" & Today.ToString("MM/yyyy")) ' Today

        ColorDgRowsContrat(DgAgentContrat, 1, "sta", "cdd")
        Dim IList As String = ListeAgentFinContrat()
        ShowTempMsg(IList, 30, "Contrat arrivant à échance")
        ColorDgRowsFinContratAffectation(DgAgentContrat, 0, IList)
        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub
    Sub ResetValues()
        elpStatus.Visibility = Visibility.Visible
        btnSave.Content = "Enregistrer et fermer"
        btnSaveContinue.Content = "Enregistrer et ajouter un autre"
        btnDeleteAg.Visibility = Visibility.Collapsed
        AfficheProfilUser()
        NewData = True
        maj()
        AfficheStat?.Invoke()
        AfficheMemos?.Invoke()
        Dim istr As String = " SELECT ag.NomPrenoms, natc.IdNature as Type,format(crt.DebutContr) as [Début], format(crt.FinContr) as [Fin],  crt.IdContr as Ref, crt.Obs,crt.TypContr as TypeExt,crt.NaturContr as Nature,crt.NumContrat as [N°]"
        istr += " FROM CONTRATSTBL crt join agents ag on ag.NumMatr=crt.NomPrenoms join contratsnature natc on crt.NaturContr=natc.NatureContr "
        istr += " where ag.actif=1 and crt.FinContr>='" & Today.ToString("yyyy-MM-dd") & "'"
        istr += " ORDER BY crt.DebutContr desc, ag.Nomprenoms "
        Dim itext As String = "En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'", LocalConnxStr) & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'", LocalConnxStr) & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")", LocalConnxStr) '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        itext += Environment.NewLine
        tblNbAgentcONTRAT.Text = itext ' "En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid()
        DgAgentContrat.ItemsSource = LoadDataTable(istr).DefaultView
        RefCrt = MayStatVal("select max(IdContr) from contratstbl ", LocalConnxStr) + 1
        dtpDebutCrt.SelectedDate = Today
        dtpDebutCrt.DisplayDateStart = DateCreationSociete
        dtpDebutCrt.DisplayDateEnd = FinDuMois(Today.AddMonths(1))

        dtpFinCrt.SelectedDate = Today
        dtpFinCrt.DisplayDateStart = DateCreationSociete
        dtpFinCrt.DisplayDateEnd = FinDuMois(Today.AddMonths(12))

        TbxNumCrt.Text = ""
        TbxObs.Text = ""
        CbxNaturCrt.SelectedIndex = -1
        CbxNom.SelectedIndex = -1
        CbxNumMatr.SelectedIndex = -1
        CbxTypCrt.SelectedIndex = -1

        ColorDgRowsContrat(DgAgentContrat, 1, "sta", "cdd")
        Dim iList As String = ListeAgentFinContrat()
        ColorDgRowsFinContratAffectation(DgAgentContrat, 0, iList)
        ChangeYN = False
        elpStatus.Visibility = Visibility.Collapsed

    End Sub
    Sub SearchContrat(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible
        Dim sufix As String = ""
        If KeyWord = "" Then
            sufix = " and (ag.actif=1 and crt.FinContr>='" & Today.ToString("yyyy-MM-dd") & "')"
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = " "
        Else
            sufix = ""
        End If
        Dim istr As String = " SELECT ag.NomPrenoms, natc.IdNature as Type,format(crt.DebutContr) as [Début], format(crt.FinContr) as [Fin],  crt.IdContr as Ref, crt.Obs,crt.TypContr as TypeExt,crt.NaturContr as Nature,crt.NumContrat as [N°] "
        istr += " FROM CONTRATSTBL crt join agents ag on ag.NumMatr=crt.NomPrenoms join contratsnature natc on crt.NaturContr=natc.NatureContr where "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then istr += " Début>='" & dtpDebutSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and Début<='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "' and "
        istr += " (ag.NomPrenoms  like '%" & KeyWord & "%' "
        istr += " or natc.IdNature  like '%" & KeyWord & "%' "
        istr += " or crt.Obs  like '%" & KeyWord & "%' )" & sufix
        istr += " ORDER BY ag.Nomprenoms, crt.DebutContr desc "
        maj()
        DgAgentContrat.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'", LocalConnxStr) & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'", LocalConnxStr) & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")", LocalConnxStr) '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        itext += Environment.NewLine
        itext += MyContratList("select Type,count(*) from (" & istr & ") group by Type")
        tblNbAgentcONTRAT.Text = itext '"En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")
        ColorDgRowsContrat(DgAgentContrat, 1, "sta", "cdd")
        Dim iList As String = ListeAgentFinContrat()
        ColorDgRowsFinContratAffectation(DgAgentContrat, 0, iList)
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Sub ActivSaveBtn()
        Dim miDateRef As Date = dtpDebutCrt.SelectedDate.Value
        If RefCrt >= 1 AndAlso CbxNaturCrt.Items.Contains(CbxNaturCrt.SelectedItem) AndAlso CbxTypCrt.Items.Contains(CbxTypCrt.SelectedItem) AndAlso CbxNumMatr.Items.Contains(CbxNumMatr.SelectedItem) AndAlso (dtpFinCrt.SelectedDate.HasValue AndAlso dtpDebutCrt.SelectedDate.HasValue AndAlso dtpFinCrt.SelectedDate.Value > miDateRef) Then
            btnSave.Visibility = Visibility.Visible
            btnSaveContinue.Visibility = Visibility.Visible
        Else
            btnSave.Visibility = Visibility.Collapsed
            btnSaveContinue.Visibility = Visibility.Collapsed
        End If
        ChangeYN = True
        stpButtonsPad.Visibility = Visibility.Visible
    End Sub

    Private Sub CbxNom_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNom.SelectionChanged
        If CbxNom.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            CbxNumMatr.Visibility = Visibility.Visible
            TblIndicationNumMatr.Visibility = Visibility.Visible
        End If

        SaisiCbx(sender, TblIndicationNom)
        If CbxNom.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            If FilmyCbx("select a.NumMatr,a.NomPrenoms from agents a  where a.NomPrenoms like '" & Apost(CbxNom.SelectedItem) & "'  order by a.NumMatr", CbxNumMatr, LocalConnxStr) Then
                CbxNumMatr.SelectedIndex = 0
                TblIndicationNumMatr.Visibility = Visibility.Collapsed
                CbxNumMatr.Visibility = Visibility.Visible
                'TblIndicationNumMatr.Visibility = Visibility.Visible
            Else
                CbxNumMatr.SelectedIndex = -1
                TblIndicationNumMatr.Visibility = Visibility.Visible
                CbxNumMatr.Visibility = Visibility.Collapsed
                TblIndicationNumMatr.Visibility = Visibility.Collapsed
            End If
        End If
        ActivSaveBtn()
    End Sub

    Private Sub CbxNumMatr_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNumMatr.SelectionChanged
        SaisiCbx(sender, TblIndicationNumMatr)
        ActivSaveBtn()
    End Sub

    Private Sub CbxTypCrt_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxTypCrt.SelectionChanged
        SaisiCbx(sender, TblIndicationTypCrt)
        ActivSaveBtn()
    End Sub

    Private Sub CbxNaturCrt_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNaturCrt.SelectionChanged
        SaisiCbx(sender, TblIndicationNaturCrt)
        ActivSaveBtn()
    End Sub

    Private Sub TbxObs_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxObs.TextChanged
        SaisiTbx(sender, TblIndicationObs)
        ActivSaveBtn()
    End Sub


    Private Sub dtpDebutCrt_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutCrt.SelectedDateChanged
        ActivSaveBtn()
        If dtpDebutCrt.SelectedDate.HasValue Then
            Dim refDate As Date = dtpDebutCrt.SelectedDate.Value
            dtpFinCrt.DisplayDateStart = refDate
            If dtpFinCrt.SelectedDate.HasValue AndAlso dtpFinCrt.SelectedDate.Value < refDate Then
                dtpFinCrt.SelectedDate = refDate
            End If
        End If
        dtpFinCrt.DisplayDateStart = dtpDebutCrt.SelectedDate.Value
        dtpFinCrt.SelectedDate = FinDuMois(dtpDebutCrt.SelectedDate.Value.AddMonths(2))
        dtpFinCrt.Focus()
    End Sub

    Private Sub TbxNumCrt_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxNumCrt.TextChanged
        SaisiTbx(sender, TblIndicationNumCrt)
        ActivSaveBtn()
    End Sub
    Function SaveNewContrat() As Boolean
        SaveNewContrat = False
        Dim istr As String = " insert into contratstbl "
        istr += "(IdContr, NomPrenoms, DebutContr, FinContr, TypContr,NaturContr, NumContrat,Obs) values "
        istr += " ('" & RefCrt & "', "
        istr += " '" & Apost(CbxNumMatr.SelectedItem) & "', "
        istr += " '" & (dtpDebutCrt.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        istr += " '" & (dtpFinCrt.SelectedDate.Value).ToString("yyyy-MM-dd") & "', "
        istr += " '" & Apost(CbxTypCrt.SelectedItem) & "', "
        istr += " '" & Apost(CbxNaturCrt.SelectedItem) & "', "
        istr += " '" & Apost(TbxNumCrt.Text) & "', "
        istr += " '" & Apost(TbxObs.Text) & "') "
        'MsgBox(istr)
        If LocalModeBb Then MsgBox("Vous travaillez en MODE LOCAL => " & DbPath)
        SaveNewContrat = SaveNewData(istr, iConnxStr)
        If SaveNewContrat Then Joper("SaveNewContrat for " & CbxNom.SelectedItem, iConnxStr)
    End Function
    Function SaveModifCtr() As Boolean
        SaveModifCtr = False
        Dim istr As String = " update contratstbl  set "
        'istr += "(IdHS, Agent, SiteHS, DateHS, NbFaction, Obs) values "
        istr += " NomPrenoms='" & Apost(CbxNumMatr.SelectedItem) & "', "
        istr += " DebutContr='" & dtpDebutCrt.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " FinContr='" & dtpFinCrt.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        istr += " TypContr='" & Apost(CbxTypCrt.SelectedItem) & "', "
        istr += " NaturContr='" & Apost(CbxNaturCrt.SelectedItem) & "', "
        istr += " NumContrat='" & Apost(TbxNumCrt.Text) & "', "
        istr += " Obs='" & Apost(TbxObs.Text) & "'  "
        istr += " where IdContr=" & RefCrt & " "
        'MsgBox(istr)
        SaveModifCtr = SaveNewData(istr, iConnxStr)
        If SaveModifCtr Then Joper("SaveModifCtr for " & CbxNom.SelectedItem, iConnxStr)

    End Function

    Private Sub dtpFinCrt_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpFinCrt.SelectedDateChanged
        If dtpFinCrt.SelectedDate.HasValue Then
            Dim idate As Date = dtpFinCrt.SelectedDate.Value
            If dtpDebutCrt.SelectedDate.HasValue AndAlso dtpDebutCrt.SelectedDate.Value > idate Then dtpDebutCrt.SelectedDate = idate
        End If
        ActivSaveBtn()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If dtpDebutCrt.SelectedDate.HasValue AndAlso dtpDebutCrt.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDebutCrt.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDebutCrt.Focus()
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
            'btpPlus.Content = " > "

        End If
        btnDeleteAg.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnSaveContinue_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveContinue.Click
        If dtpDebutCrt.SelectedDate.HasValue AndAlso dtpDebutCrt.SelectedDate = Today Then
            If MsgBox("La date <" & dtpDebutCrt.SelectedDate & "> est elle correcte ?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                dtpDebutCrt.Focus()
                Exit Sub
            End If
        End If
        If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If NewData Then
                Try
                    If SaveNewContrat() Then
                        MsgBox("Données enregistrer avec succès")
                        ResetValues()
                    Else
                    End If
                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            Else
                Try
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
                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try

            End If

        End If

    End Sub

    Private Sub BtnAgentListeSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles BtnAgentListeSearch.TextChanged
        inputTimer.Stop()
        inputTimer.Start()
    End Sub

    Private Sub DgAgentContrat_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgAgentContrat.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgAgentContrat.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If CbxNom.Items.Contains(rowView.Row.ItemArray(0).ToString) Then
                CbxNom.SelectedItem = rowView.Row.ItemArray(0).ToString
                dtpDebutCrt.SelectedDate = rowView.Row.ItemArray(2).ToString
                dtpFinCrt.SelectedDate = rowView.Row.ItemArray(3).ToString
                CbxNaturCrt.SelectedItem = rowView.Row.ItemArray(7).ToString
                CbxTypCrt.SelectedItem = rowView.Row.ItemArray(6).ToString
                TbxObs.Text = rowView.Row.ItemArray(5).ToString
                TbxNumCrt.Text = rowView.Row.ItemArray(8).ToString
                RefCrt = rowView.Row.ItemArray(4).ToString
                stpSaisie.Visibility = Visibility.Visible
                btnDeleteAg.Visibility = Visibility.Visible
                NewData = False
                btnSave.Content = "Modifier et fermer"
                btnSaveContinue.Content = "Modifier et Continuer"
                btnDeleteAg.Visibility = Visibility.Visible
                btnNewData.Visibility = Visibility.Collapsed
            Else
                stpSaisie.Visibility = Visibility.Collapsed
                btnDeleteAg.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
                NewData = True
                btnSave.Content = "Enregistrer et fermer"
                btnSaveContinue.Content = "Enregistrer et ajouter un autre"
            End If
        End If
        AfficheProfilUser()
    End Sub
    Function DeleteCtr() As Boolean
        DeleteCtr = False
        Dim istr As String = " delete from CONTRATSTBL "
        istr += " where IdContr=" & RefCrt & " "
        DeleteCtr = SaveNewData(istr, iConnxStr)
        If DeleteCtr Then Joper("DeleteCtr for " & CbxNom.SelectedItem & " at " & RefCrt, iConnxStr)
    End Function

    Private Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgAgentContrat.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefCrt = rowView.Row.ItemArray(4).ToString
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

    Private Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & CbxNom.SelectedItem & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If DeleteCtr() Then
                MsgBox("Donnée supprimées avec succès ")
                ResetValues()
            Else
                MsgBox("Erreur de suppression")
            End If
        End If

    End Sub

    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs) Handles cmiModif.Click
        Dim rowView As DataRowView = TryCast(DgAgentContrat.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub cmiResume_Click(sender As Object, e As RoutedEventArgs) Handles cmiResume.Click
        Dim rowView As DataRowView = TryCast(DgAgentContrat.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            Dim iWrd As String = ResumeCtr(rowView.Row.ItemArray(0).ToString)
            If iWrd <> "" Then MsgBox(iWrd)
        End If

    End Sub

    Private Sub BtnAgentListeSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles BtnAgentListeSearch.KeyDown
        If e.Key = Key.Enter Then
            inputTimer.Stop()
            SearchContrat(BtnAgentListeSearch.Text)
        End If
    End Sub

    Private Sub dtpDebutSearch_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebutSearch.SelectedDateChanged, dtpFinSearch.SelectedDateChanged
        If iFormLoaded Then SearchContrat(BtnAgentListeSearch.Text)
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Contrats "
        If dtpDebutSearch.SelectedDate.HasValue And dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Contrats du " & dtpDebutSearch.SelectedDate.Value.ToString("dd/MM/yy") & " au " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        ExportDGtoExcel(DgAgentContrat, "Point_Contrats", iTitre) '"Point_Entrees"

    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As RoutedEventArgs) Handles btnRefresh.Click
        InialiseLoad()
        AfficheStat?.Invoke()
        AfficheMemos?.Invoke()

    End Sub

    Private Sub CbxNom_LostFocus(sender As Object, e As RoutedEventArgs) Handles CbxNom.LostFocus
        If CbxNumMatr.SelectedIndex >= 0 Then
            Try
                Dim DateStr As String = MyStatValStr("select date(c.FinContr,'+1 day')  from contratstbl c  where c.NomPrenoms like '" & Apost(CbxNumMatr.SelectedItem) & "' order by c.FinContr desc limit 1", LocalConnxStr)
                Dim iDate As Date = Today

                If DateStr = "" Then
                    iDate = CDate(MyStatValStr("select a.DateEntree  from agents a where a.NumMatr like '" & Apost(CbxNumMatr.SelectedItem) & "'", LocalConnxStr))
                ElseIf IsDate(DateStr) Then
                    iDate = CDate(DateStr)
                Else

                End If
                dtpDebutCrt.DisplayDateStart = iDate
                dtpDebutCrt.SelectedDate = iDate
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        End If

    End Sub

    Private Sub btnAgrandir_Click(sender As Object, e As RoutedEventArgs) Handles btnAgrandir.Click
        btnAgrandir.Visibility = Visibility.Collapsed
        ' Se retirer du parent visuel
        Dim parent = Me.Parent
        If TypeOf parent Is ContentControl Then
            CType(parent, ContentControl).Content = Nothing
        End If
        'Dim IusercAgentsEntree As New usercAgentsEntree
        Dim iDetailW As New wdwDetails("Details des CONTRATS-AGENTS", "A ce jour " & Today, Me)
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


    Private Sub btnPointStage_Click(sender As Object, e As RoutedEventArgs) Handles btnPointStage.Click
        Dim debut As Date = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy"))
        Dim Fin As Date = CDate("24/" & Today.ToString("MM/yyyy"))
        If dtpDebutSearch.SelectedDate.HasValue Then debut = dtpDebutSearch.SelectedDate
        If dtpFinSearch.SelectedDate.HasValue Then Fin = dtpFinSearch.SelectedDate
        Dim istr As String = "SELECT a.NomPrenoms, date(c.DebutContr), date(c.FinContr), a.NumMatr "
        istr &= " FROM Agents a JOIN contratstbl c ON c.NomPrenoms = a.NumMatr "
        istr &= " WHERE c.DebutContr >='" & debut.ToString("yyyy-MM-dd") & "' and c.DebutContr <='" & Fin.ToString("yyyy-MM-dd") & "'"
        istr &= " and a.DateEntree >='" & DebutDuMois(Fin.AddMonths(-6)).ToString("yyyy-MM-dd") & "'"
        istr &= " and (c.NaturContr LIKE '%stag%') "
        'istr &= " GROUP BY s.Client, a.NomPrenoms  "
        istr &= " ORDER BY a.NomPrenoms"
        'ShowTempMsg(istr)
        openPointMensuel(debut, Fin, istr, "STAGES du " & debut.ToString("dd/MM/yyyy") & " au " & Fin.ToString("dd/MM/yyyy"), "STAGES")

    End Sub

    Private Sub btnPointCD_Click(sender As Object, e As RoutedEventArgs) Handles btnPointCD.Click
        Dim debut As Date = CDate("25/" & Today.AddMonths(-1).ToString("MM/yyyy"))
        Dim Fin As Date = CDate("24/" & Today.ToString("MM/yyyy"))
        If dtpDebutSearch.SelectedDate.HasValue Then debut = dtpDebutSearch.SelectedDate
        If dtpFinSearch.SelectedDate.HasValue Then Fin = dtpFinSearch.SelectedDate
        Dim istr As String = "SELECT a.NomPrenoms, date(c.DebutContr), a.NumMatr  "
        istr &= " FROM Agents a JOIN contratstbl c ON c.NomPrenoms = a.NumMatr "
        istr &= " WHERE c.DebutContr >='" & debut.ToString("yyyy-MM-dd") & "' and c.DebutContr <='" & Fin.ToString("yyyy-MM-dd") & "'"   ' date('now','-1 month','start of month','+24 day')  "
        istr &= " and a.DateEntree >='" & DebutDuMois(Fin.AddMonths(-12)).ToString("yyyy-MM-dd") & "'"   'date('now','-1 month','start of month','+24 day')"
        istr &= " and (c.NaturContr NOT LIKE '%stag%') "
        'istr &= " GROUP BY s.Client, a.NomPrenoms  "
        istr &= " ORDER BY a.NomPrenoms"
        'ShowTempMsg(istr)
        openPointMensuel(debut, Fin, istr, "CDD ou CDI du " & debut.ToString("dd/MM/yyyy") & " au " & Fin.ToString("dd/MM/yyyy"), "CDD_CDI")

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
End Class
