Imports System.Windows.Threading
Imports System.Data.SQLite
Imports System.Data
Public Class usercClientsPoste
    Public iConnxStr As String
    Public LocalConnxStr As String
    Dim ChangeYN As Boolean = False
    Dim iActif As Boolean = True
    Dim iInActif As Boolean = False
    Dim AffActif As Boolean = True
    Dim RefPst As Integer = 0
    Dim inputTimer As DispatcherTimer
    Dim iFormLoaded As Boolean = False
    Dim iCbxPosteCode As New ComboBox
    Dim iCbxPosteSite As New ComboBox
    Dim iCbxPosteDesignation As New ComboBox
    Dim NewData As Boolean = True
    Dim NewNom As String = ""
    Dim NewDesign As String = ""
    Dim retourDansMainWindow As Action
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
        Dim istr As String = " SELECT p.Designation,p.Code,p.Actif,p.Duree,p.ReposHebdo,p.NomSite,p.IdPoste "
        istr += " from postes p where actif=1 "
        istr += " order by p.Actif desc, p.Designation "
        DgClientsPoste.ItemsSource = LoadDataTable(istr, iConnxStr).DefaultView
        tblNbClientPostesActifs.Text = "Nombre de postes actifs: " & MayStatVal("select count(*) from postes where Actif=1 ", iConnxStr) '& " | Nombre de postes fermés: " & MayStatVal("select count(*) from postes where Actif=0 ")

        FilmyCbx("SELECT s.NomSite from siteaffectation s where s.Actif=1 order by s.Actif desc,s.NomSite ", CbxSite, iConnxStr)
        FilmyCbx("select lower(s.Code) from postes s ", iCbxPosteCode, iConnxStr)
        FilmyCbx("select lower(s.Designation) from postes s ", iCbxPosteDesignation, iConnxStr)
        FilmyCbx("select d.JourRepos from PosteRepos d where d.Actif=1 order by d.Ordre", CbxRepos, iConnxStr)
        FilmyCbx("select d.Duree from PosteDuree d where d.Actif=1 order by d.Favorit desc,d.Duree ", CbxDuree, iConnxStr)
        RefPst = MayStatVal("select max(IdPoste) from postes", iConnxStr) + 1

        'dtpDebutSearch.SelectedDate = CDate("01/01/" & Today.Year)
        'dtpFinSearch.SelectedDate = Today
        iFormLoaded = True
        ChangeYN = False
        AfficheProfilUser()
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Sub ResetValues()
        elpStatus.Visibility = Visibility.Visible
        maj()

        Try
            btnSave.Content = "Enregistrer et fermer"
            btnSaveContinue.Content = "Enregistrer et ajouter un autre"
            btnDeleteAg.Visibility = Visibility.Collapsed

            Dim istr As String = " SELECT p.Designation,p.Code,p.Actif,p.Duree,p.ReposHebdo,p.NomSite,p.IdPoste "
            istr += " from postes p "
            istr += " order by p.Actif desc, p.Designation "
            DgClientsPoste.ItemsSource = LoadDataTable(istr, iConnxStr).DefaultView
            tblNbClientPostesActifs.Text = "Nombre de postes actifs: " & MayStatVal("select count(*) from postes where Actif=1 ", iConnxStr) '& " | Nombre de postes fermés: " & MayStatVal("select count(*) from postes where Actif=0 ")

            FilmyCbx("select lower(s.Code) from postes s ", iCbxPosteCode, iConnxStr)
            FilmyCbx("select lower(s.Designation) from postes s ", iCbxPosteDesignation, iConnxStr)
            RefPst = MayStatVal("select max(IdPoste) from postes", iConnxStr) + 1

            CbxDuree.SelectedIndex = -1
            CbxRepos.SelectedIndex = -1
            CbxSite.SelectedIndex = -1
            tbxListePoste.Text = ""
            tbxListePoste.Visibility = Visibility.Collapsed
            TbxDesignation.Text = ""
            TbxCode.IsReadOnly = False
            TbxDesignation.IsReadOnly = False
            TbxCode.Text = ""
            'CbxTypCrt.SelectedIndex = -1

            NewData = True

            ChangeYN = False
            CltActif.IsChecked = True
            CltInActif.IsChecked = False
            iFormLoaded = True
            elpStatus.Visibility = Visibility.Collapsed
            iFormLoaded = True

        Catch ex As Exception
            MsgBox(ex.Message)

        End Try
        AfficheProfilUser()

    End Sub
    Sub CancelValues()
        elpStatus.Visibility = Visibility.Visible
        Try
            btnSave.Content = "Enregistrer et fermer"
            btnSaveContinue.Content = "Enregistrer et ajouter un autre"
            btnDeleteAg.Visibility = Visibility.Collapsed

            RefPst = MayStatVal("select max(c.IdPoste) from postes c") + 1

            tbxListePoste.Text = ""
            tbxListePoste.Visibility = Visibility.Collapsed
            TbxDesignation.Text = ""
            TbxCode.Text = ""
            CbxSite.SelectedIndex = -1
            CbxRepos.SelectedIndex = -1
            CbxDuree.SelectedIndex = -1

            NewData = True
            ChangeYN = False
            CltActif.IsChecked = True
            CltInActif.IsChecked = False
            iFormLoaded = True
            elpStatus.Visibility = Visibility.Collapsed
            iFormLoaded = True
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Sub SearchData(KeyWord As String)
        elpStatus.Visibility = Visibility.Visible

        maj()
        Dim sufix As String = ""
        If KeyWord = "" Then
            sufix = "  " ' and (c.Actif=1 and crt.FinContr>='" & Today.ToString("yyyy-MM-dd") & "')"
        ElseIf KeyWord = "*" Or KeyWord = "%" Then
            KeyWord = "%"
            sufix = " "
        Else

        End If
        Dim istr As String = "SELECT Designation,Code,Actif,Duree,ReposHebdo,NomSite,IdPoste "
        istr += " from postes s "
        istr += " where "
        istr += " (Designation like '%" & KeyWord & "%' "
        istr += " or Code like '%" & KeyWord & "%' "
        istr += " or Duree like '%" & KeyWord & "%' "
        istr += " or ReposHebdo like '%" & KeyWord & "%' "
        istr += " or NomSite like '%" & KeyWord & "%' "
        'istr += " or IdPoste like '%" & KeyWord & "%' "
        'istr += " or s.Observations like '%" & KeyWord & "%' "
        istr += " or IdPoste  like '%" & KeyWord & "%' )" & sufix
        If AffActif Then
            If iActif And Not iInActif Then
                istr += " and Actif=1"
            ElseIf Not iActif And iInActif Then
                istr += " and Actif=0"
            Else
                istr += " and Actif=2"
            End If

        End If
        istr += " ORDER BY Actif desc,Designation "
        DgClientsPoste.ItemsSource = LoadDataTable(istr, LocalConnxStr).DefaultView
        Dim itext As String = "Nombre de postes actifs : " & MayStatVal("select count(*) from (" & istr & ") where Actif=1 ", LocalConnxStr) & " | Nombre de postes inactifs: " & MayStatVal("select count(*) from  (" & istr & ") where Actif=0 ", LocalConnxStr) 'where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")") '"En cours de validité : " & NbContatEnCoursValid() & " | Hors de validité : " & NbContatEnCoursNonValid() & " | Total : " & MayStatVal("select count (*) from CONTRATSTBL") ' (" & istr & ")")
        itext += Environment.NewLine
        tblNbClientPostesActifs.Text = itext '"En cours de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin>='" & Today.ToString("yyyy-MM-dd") & "'") & " | Hors de validité : " & MayStatVal("select count(*) from (" & istr & ") where Fin<'" & Today.ToString("yyyy-MM-dd") & "'") & " | TOTAL: " & MayStatVal("select count(*) from (" & istr & ")")


        iFormLoaded = True
        elpStatus.Visibility = Visibility.Collapsed
    End Sub

    Function SaveNewPoste() As Boolean
        SaveNewPoste = False
        Dim istr As String = " insert into postes "
        istr += "(Designation,Code,Actif,Duree,ReposHebdo,NomSite,IdPoste)  "
        istr += " values ('" & Trim(Apost(TbxDesignation.Text)) & "', "
        istr += " '" & Trim(Apost(TbxCode.Text)) & "', "
        istr += " " & ChbActif.IsChecked & ", "
        istr += " '" & Apost(CbxDuree.SelectedItem) & "', "
        istr += " '" & Apost(CbxRepos.SelectedItem) & "', "
        istr += " '" & Apost(CbxSite.SelectedItem) & "', "
        istr += " " & RefPst & ")"
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveNewPoste = SaveNewData(istr, iConnxStr)
        If SaveNewPoste Then Joper("SaveNewPoste for " & TbxDesignation.Text, iConnxStr)
    End Function
    Function SaveModifPoste() As Boolean
        SaveModifPoste = False
        Dim istr As String = " update postes  set "
        istr += " Designation='" & Trim(Apost(TbxDesignation.Text)) & "', "
        istr += " Actif=" & ChbActif.IsChecked & ", "
        istr += " Code='" & Trim(Apost(TbxCode.Text)) & "', "
        istr += " Duree='" & Apost(CbxDuree.SelectedItem) & "', "
        istr += " ReposHebdo='" & Apost(CbxRepos.SelectedItem) & "', "
        istr += " NomSite='" & Apost(CbxSite.SelectedItem) & "' "
        istr += " where IdPoste=" & RefPst & " "
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        SaveModifPoste = SaveNewData(istr, iConnxStr)
        If SaveModifPoste And NewNom <> TbxCode.Text Then
            SaveModifPoste = SaveNewData("update PlanPoste set Poste='" & Apost(TbxCode.Text) & "' where Poste='" & Apost(NewNom) & "'", iConnxStr) '113
            If SaveModifPoste Then Joper("SaveModifPoste for " & TbxDesignation.Text & " at " & RefPst, iConnxStr)
        End If

    End Function
    Function DeletePoste() As Boolean
        DeletePoste = False
        Dim istr As String = " delete from postes "
        istr += " where IdPoste=" & RefPst & " "
        If DbPath = DbPathGeneral Then MsgBox("Vous travaillez en MODE LOCAL ")
        DeletePoste = SaveNewData(istr, iConnxStr)
        If DeletePoste Then Joper("DeletePoste at " & RefPst, iConnxStr)
    End Function

    Sub ActivSaveBtn()
        If NewData Then
            'MsgBox(NewData)
            If CbxSite.SelectedIndex >= 0 And TbxDesignation.Text.Length > 3 And TbxCode.Text.Length > 3 And RefPst > 0 And Not iCbxPosteCode.Items.Contains(LCase(TbxCode.Text)) And Not iCbxPosteDesignation.Items.Contains(LCase(TbxDesignation.Text)) And CbxRepos.SelectedIndex >= 0 And CbxDuree.SelectedIndex >= 0 Then
                btnSave.Visibility = Visibility.Visible
                btnSaveContinue.Visibility = Visibility.Visible
            Else
                btnSave.Visibility = Visibility.Collapsed
                btnSaveContinue.Visibility = Visibility.Collapsed
            End If
            'MsgBox("New")
        ElseIf NewNom <> TbxCode.Text Then
            If CbxSite.SelectedIndex >= 0 And TbxDesignation.Text.Length > 3 And TbxCode.Text.Length > 4 And RefPst > 0 And (Not iCbxPosteCode.Items.Contains(LCase(TbxCode.Text)) Or NewNom = TbxCode.Text) And CbxRepos.SelectedIndex >= 0 And CbxDuree.SelectedIndex >= 0 Then ' AndAlso cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
                btnSave.Visibility = Visibility.Visible
                btnSaveContinue.Visibility = Visibility.Visible
                If NewDesign <> TbxDesignation.Text Then
                    If Not iCbxPosteDesignation.Items.Contains(LCase(TbxDesignation.Text)) Or NewDesign = TbxDesignation.Text Then
                        btnSave.Visibility = Visibility.Visible
                        btnSaveContinue.Visibility = Visibility.Visible
                    Else
                        btnSave.Visibility = Visibility.Collapsed
                        btnSaveContinue.Visibility = Visibility.Collapsed
                    End If
                End If
                'MsgBox("modif")
            Else
                btnSave.Visibility = Visibility.Collapsed
                btnSaveContinue.Visibility = Visibility.Collapsed
            End If
        Else
            'MsgBox(NewData)

            If CbxSite.SelectedIndex >= 0 And TbxDesignation.Text.Length > 3 And TbxCode.Text.Length > 4 And RefPst > 0 And CbxRepos.SelectedIndex >= 0 And CbxDuree.SelectedIndex >= 0 Then ' AndAlso cbxSexe.Items.Contains(cbxSexe.SelectedItem) And CbxFonction.Items.Contains(CbxFonction.SelectedItem) And IsDate(dtpDateEmbauche.Text) And IsDate(dtpDateNaiss.Text) Then
                btnSave.Visibility = Visibility.Visible
                btnSaveContinue.Visibility = Visibility.Visible
                'MsgBox("modif")
            Else
                btnSave.Visibility = Visibility.Collapsed
                btnSaveContinue.Visibility = Visibility.Collapsed
            End If
        End If

        ChangeYN = True
    End Sub
    Function DesignationDefaut() As String
        DesignationDefaut = ""
        If CbxSite.Text <> "" And CbxSite.SelectedIndex >= 0 And CbxSite.Text <> "" And CbxDuree.SelectedIndex >= 0 Then
            DesignationDefaut = CbxSite.SelectedItem & " - " & ExtractFaction(CbxDuree.SelectedItem)
        End If
    End Function

    Function ExtractFaction(ByVal Faction As String) As String
        ExtractFaction = "NonSpé"
        If Faction = "" Then
            Exit Function
        End If
        'Dim extrtxt As String = "NonSpé" Cyclique
        If Faction.Contains("Nuit") Then ExtractFaction = "Nuit"
        If Faction.Contains("24") Then ExtractFaction = "24"
        If Faction.Contains("Jour") Then ExtractFaction = "Jour"
        If Faction.Contains("Rotatif") Then ExtractFaction = "Rotatif"
        If Faction.Contains("Cyclique") Then ExtractFaction = "Cyclique"

        'If Faction.Contains("12 heures") Then ExtractFaction = "Rotatif"
        If Faction.Contains("Non") Then ExtractFaction = "NonSpé"
        '12 heures
    End Function

    Function ExtractPosteNumPrefix(ByVal IdPoste As String) As Integer
        ExtractPosteNumPrefix = 0
        If IdPoste = "" Then
            Exit Function
        End If
        Dim extrNum As String = ""
        For Each Elmt As String In Left(Trim(IdPoste), 4)
            If IsNumeric(Elmt) Then extrNum += Elmt
        Next
        If IsNumeric(extrNum) Then ExtractPosteNumPrefix = CInt(extrNum)
    End Function
    Function ExtractPosteNum(ByVal IdPoste As String) As Integer
        ExtractPosteNum = 0
        If IdPoste = "" Then
            Exit Function
        End If
        Dim extrNum As String = ""
        For Each Elmt As String In Right(Trim(IdPoste), 4)
            If IsNumeric(Elmt) Then extrNum += Elmt
        Next
        If IsNumeric(extrNum) Then ExtractPosteNum = CInt(extrNum)
    End Function

    Function idPosteDefault() As String
        idPosteDefault = "P00000"
        Try
            Dim myNum As Integer = 0
            For Each elmt As Integer In iCbxPosteSite.Items
                If myNum <= elmt Then myNum = elmt
            Next
            If myNum > 0 Then
                idPosteDefault = "P" & Right("00000" & (myNum + 1), 5)
                'MsgBox(idPosteDefault)
            Else
                Dim numposte As Integer = 0
                For Each elmt As String In iCbxPosteCode.Items
                    numposte = ExtractPosteNumPrefix(elmt)
                    If myNum <= numposte Then myNum = numposte
                Next
                idPosteDefault = "P" & Right("000" & (numposte + 1), 3) & "01"
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Function

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

    Private Sub TbxDesignation_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxDesignation.TextChanged
        SaisiTbx(sender, TblIndicationDesignation)
        ActivSaveBtn()
        If iCbxPosteDesignation.Items.Contains(LCase(TbxDesignation.Text)) Then
            TbxDesignation.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF8E8E"))
        Else
            TbxDesignation.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFFFFF"))
        End If
    End Sub

    Private Sub TbxCode_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxCode.TextChanged
        SaisiTbx(sender, TblIndicationCode)
        TbxCode.Text = UCase(TbxCode.Text)
        ActivSaveBtn()
        If iCbxPosteCode.Items.Contains(LCase(TbxCode.Text)) Then
            TbxCode.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF8E8E"))
        Else
            TbxCode.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFFFFF"))
        End If
    End Sub
    Public Function FiliCbx(MyCommandString As String, MyCbx As ComboBox, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        FiliCbx = False
        Try
            Using iconn As New SQLiteConnection(MyConxStr)
                iconn.Open()
                Using icmd As New SQLiteCommand(MyCommandString, iconn)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                    MyCbx.Items.Clear()
                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            MyCbx.Items.Add(ExtractPosteNum(iReader(0)))
                        End If
                    End While
                    FiliCbx = True
                End Using

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
            FiliCbx = False
        End Try
    End Function

    Private Sub CbxSite_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxSite.SelectionChanged
        SaisiCbx(sender, TblIndicationCbxSite)
        ActivSaveBtn()
        If CbxSite.SelectedIndex >= 0 Then FiliCbx("select c.Code from Postes c where c.NomSite like '" & Apost(CbxSite.SelectedItem) & "' order by c.Code", iCbxPosteSite, LocalConnxStr)
        TbxCode.Text = idPosteDefault()

        If CbxSite.SelectedIndex = -1 Then Exit Sub
        Dim itext As String = ListePosteSite(CbxSite.SelectedItem, LocalConnxStr)
        Dim iNb As Integer = itext.Length - 2
        If iNb > 0 Then tbxListePoste.Text = Left(itext, iNb)
        If itext = "" Then
            tbxListePoste.Text = ""
            tbxListePoste.Visibility = Visibility.Collapsed
        Else
            tbxListePoste.Visibility = Visibility.Visible
        End If
        TbxDesignation.Text = DesignationDefaut() 'CbxSite.SelectedItem
    End Sub

    Private Sub CbxDuree_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxDuree.SelectionChanged
        SaisiCbx(sender, TblIndicationDuree)
        ActivSaveBtn()
        TbxDesignation.Text = DesignationDefaut() 'CbxSite.SelectedItem
    End Sub

    Private Sub CbxRepos_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxRepos.SelectionChanged
        SaisiCbx(sender, TblIndicationRepos)
        ActivSaveBtn()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        If NewData Then
            If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Try
                    If SaveNewPoste() Then
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
            If SaveModifPoste() Then
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
        If NewData Then
            If MsgBox("Enregister ces données ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Try
                    If SaveNewPoste() Then
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
            If SaveModifPoste() Then
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

    Private Sub DgClientsPoste_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles DgClientsPoste.MouseDoubleClick
        Dim rowView As DataRowView = TryCast(DgClientsPoste.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub
    Sub LoadRowItems(rowView As DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            If iCbxPosteDesignation.Items.Contains(LCase(rowView.Row.ItemArray(0).ToString)) Then
                'p.Designation,p.Code,p.Actif,p.Duree,p.ReposHebdo,p.NomSite,p.IdPoste
                'TbxSite.IsReadOnly = True
                ChbActif.IsChecked = rowView.Row.ItemArray(2).ToString
                CbxDuree.SelectedItem = rowView.Row.ItemArray(3).ToString
                CbxRepos.SelectedItem = rowView.Row.ItemArray(4).ToString
                CbxSite.SelectedItem = rowView.Row.ItemArray(5).ToString
                TbxDesignation.Text = rowView.Row.ItemArray(0).ToString
                NewDesign = rowView.Row.ItemArray(0).ToString
                TbxCode.Text = rowView.Row.ItemArray(1).ToString
                NewNom = rowView.Row.ItemArray(1).ToString
                RefPst = rowView.Row.ItemArray(6).ToString

                stpSaisie.Visibility = Visibility.Visible
                btnDeleteAg.Visibility = Visibility.Visible
                NewData = False
                btnSave.Content = "Modifier et fermer"
                btnSaveContinue.Content = "Modifier et Continuer"
                btnDeleteAg.Visibility = Visibility.Visible
                btnNewData.Visibility = Visibility.Collapsed
                'MsgBox(NewData)
                'TbxCode.IsReadOnly = True
                'TbxDesignation.IsReadOnly = True
            Else
                'TbxSite.IsReadOnly = False
                'CbxClient.IsEnabled = False
                stpSaisie.Visibility = Visibility.Collapsed
                btnDeleteAg.Visibility = Visibility.Collapsed
                btnNewData.Visibility = Visibility.Visible
                NewData = True
                btnSave.Content = "Enregistrer et fermer"
                btnSaveContinue.Content = "Enregistrer et ajouter un autre"
                TbxDesignation.IsReadOnly = False
                TbxCode.IsReadOnly = False
                'MsgBox("new")
            End If
            'NewData = False
            AfficheProfilUser()

        End If

    End Sub

    Private Sub cmiModif_Click(sender As Object, e As RoutedEventArgs) Handles cmiModif.Click
        Dim rowView As DataRowView = TryCast(DgClientsPoste.CurrentItem, DataRowView)
        LoadRowItems(rowView)
    End Sub

    Private Sub cmiSuppr_Click(sender As Object, e As RoutedEventArgs) Handles cmiSuppr.Click
        Dim rowView As DataRowView = TryCast(DgClientsPoste.CurrentItem, DataRowView)
        If rowView IsNot Nothing AndAlso rowView.Row.ItemArray.Length > 1 Then
            RefPst = rowView.Row.ItemArray(6).ToString
            If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & rowView.Row.ItemArray(0).ToString & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                If DeletePoste() Then
                    MsgBox("Donnée supprimées avec succès ")
                    ResetValues()
                Else
                    MsgBox("Erreur de suppression")
                End If
            End If
        End If

    End Sub

    Private Sub btnDeleteAg_Click(sender As Object, e As RoutedEventArgs) Handles btnDeleteAg.Click
        If MessageBox.Show("Voulez supprimer cette donnée de " & Environment.NewLine & TbxDesignation.Text & " ? " & Environment.NewLine & "Continuer", "SUPPRESSION ", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
            If DeletePoste() Then
                MsgBox("Donnée supprimées avec succès ")
                ResetValues()
            Else
                MsgBox("Erreur de suppression")
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

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        Dim iTitre As String = "Postes "
        ExportDGtoExcel(DgClientsPoste, "Point_Postes", iTitre) '"Point_Entrees"

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
        Dim iDetailW As New wdwDetails("Details des Postes", "A ce jour : " & Today, Me)
        AddHandler iDetailW.Closed, Sub()
                                        retourDansMainWindow?.Invoke()
                                        btnAgrandir.Visibility = Visibility.Visible
                                    End Sub
        iDetailW.Height = CInt(My.Computer.Screen.Bounds.Height * 0.6)
        iDetailW.Width = CInt(My.Computer.Screen.Bounds.Width * 0.6)
        iDetailW.WindowStartupLocation = WindowStartupLocation.CenterOwner
        iDetailW.Topmost = True
        Dim iWtf As New WaitFrm
        iWtf.Show()
        iDetailW.Show()
        iWtf.Close()

    End Sub

    Private Sub ChbActif_Checked(sender As Object, e As RoutedEventArgs) Handles ChbActif.Checked
        If iFormLoaded Then ActivSaveBtn()
    End Sub

    Private Sub ChbActif_Unchecked(sender As Object, e As RoutedEventArgs) Handles ChbActif.Unchecked
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
