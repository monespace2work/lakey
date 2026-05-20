Public Class GeneralSets
    Dim iDbPath As String = ""
    Dim iFormLoaded As Boolean = False
    Dim iDateDebut As Integer = 0
    Dim iDateFin As Integer = 0
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        AfficheData()
    End Sub

    Sub AfficheData()
        LoadDataSettings()
        TbxDBpath.Text = DbPathGeneral
        TbxRecupFolder.Text = BUDirPath 'MyStatValStr("select DirPath from DirOthersSets where  NuRef=1", "Data Source=C:\LkyRess\gfil.sqlite;Version=3;")
        btnSaveBase.Visibility = Visibility.Collapsed

        AfficheDataDateAge()

        AfficheDataSalaires()

        AfficheDataFacturation()

        iFormLoaded = True
    End Sub
    Sub AfficheDataDateAge()
        DtpDateCreationSociete.SelectedDate = DateCreationSociete
        TbxAgeRetraite.Text = AgeRetraite
        TbxAgeMinEmbauche.Text = AgeMinEmbauche
        TbxDatDebutEtatFinancierMensuel.Text = Math.Abs(DateDebutEtatFinancierMensuel)
        If DateDebutEtatFinancierMensuel < 0 Then
            cbxMoisDébut.SelectedIndex = 0
        Else
            cbxMoisDébut.SelectedIndex = 1
        End If
        TbxDateFinEtatFinancierMensuel.Text = Math.Abs(DateFinEtatFinancierMensuel)
        If DateFinEtatFinancierMensuel < 0 Then
            cbxMoisFin.SelectedIndex = 0
        Else
            cbxMoisFin.SelectedIndex = 1
        End If
        TbxNbJrDeCongeMensuel.Text = NbJrDeCongeMensuel
        DtpDateCreationSociete.DisplayDateEnd = Today
        btnSaveDateAge.Visibility = Visibility.Collapsed

    End Sub
    Sub AfficheDataSalaires()
        TbxMontantSalaireBase.Text = MontantSalaireBase
        TbxMontantSMIG.Text = MontantSMIG
        TbxMontantSalaireNet.Text = MontantSalaireNet
        TbxTauxCNSSpatronal.Text = TauxCNSSpatronal
        TbxTauxCNSSemploye.Text = TauxCNSSemploye
        tbxTauxIRPP.Text = TauxIRPP
        btnSaveSalaires.Visibility = Visibility.Collapsed
    End Sub
    Sub AfficheDataFacturation()
        TbxFacturePUsociete.Text = FacturePUsociete
        TbxFacturePUparticurier.Text = FacturePUparticurier
        TbxFacturePUminimum.Text = FacturePUminimum
        TbxTauxTVA.Text = TauxTVA
        TbxTauxReduction.Text = TauxReduction
        btnSaveFacturation.Visibility = Visibility.Collapsed
    End Sub
    Private Sub TbxDBpath_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles TbxDBpath.MouseDoubleClick
        Dim dlg As New Microsoft.Win32.OpenFileDialog()
        dlg.Filter = "Sqlite base (*.sqlite)|*.sqlite"
        If dlg.ShowDialog() = True Then
            TbxDBpath.Text = dlg.FileName
        End If
    End Sub

    Private Sub btnSaveBase_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles btnSaveBase.MouseDoubleClick
        Dim iStr As String = ""
        iStr &= "update Sets  "
        iStr &= " set "
        iStr &= " DBpathG='" & Apost(TbxDBpath.Text) & "', "
        iStr &= " BckUpDir='" & Apost(TbxRecupFolder.Text) & "' "
        iStr &= " where IdSet=1 "

        Try
            If SaveNewData(iStr, "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") Then
                DbPathGeneral = TbxDBpath.Text
                BUDirPath = TbxRecupFolder.Text
                Try
                    iStr = "update DirOthersSets  "
                    iStr &= " set "
                    iStr &= " DirPath='" & Apost(BUDirPath) & "' "
                    iStr &= " where NuRef=1 "
                    If Not SaveNewData(iStr, "Data Source=C:\LkyRess\gfil.sqlite;Version=3;") Then MsgBox("Erreur inattendue => " & iStr)
                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try
                MsgBox("Données enregistées avec succès")
                btnSaveBase.Visibility = Visibility.Collapsed
            Else
                MsgBox("Ereeur d'enregistrement")
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try


    End Sub

    Private Sub TbxRecupFolder_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles TbxRecupFolder.MouseDoubleClick
        Dim dlg As New Forms.FolderBrowserDialog
        If Not IO.Directory.Exists("C:\MyRescu\") Then
            Try
                IO.Directory.CreateDirectory("C:\MyRescu\")
            Catch ex As Exception

            End Try
        End If
        If IO.Directory.Exists("C:\MyRescu\") Then dlg.SelectedPath = "C:\MyRescu\"
        'dlg.Filter = "Sqlite base (*.sqlite)|*.sqlite"
        If dlg.ShowDialog() = Forms.DialogResult.OK Then
            TbxRecupFolder.Text = dlg.SelectedPath
        End If
        dlg.SelectedPath = "C:\MyRescu\"
    End Sub

    Private Sub TbxDBpath_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxDBpath.TextChanged, TbxRecupFolder.TextChanged
        If IO.File.Exists(TbxDBpath.Text) And IO.Directory.Exists(TbxRecupFolder.Text) Then
            btnSaveBase.Visibility = Visibility.Visible
        Else
            btnSaveBase.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub TbxAgeMinEmbauche_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxAgeMinEmbauche.PreviewTextInput
        If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxAgeRetraite_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxAgeRetraite.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxDatDebutEtatFinancierMensuel_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxDatDebutEtatFinancierMensuel.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxDateFinEtatFinancierMensuel_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxDateFinEtatFinancierMensuel.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxNbJrDeCongeMensuel_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxNbJrDeCongeMensuel.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9.]$") Then
            e.Handled = True ' Bloque la saisie
        End If

    End Sub

    Private Sub DtpDateCreationSociete_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles DtpDateCreationSociete.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9/]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxMontantSalaireBase_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxMontantSalaireBase.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxMontantSMIG_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxMontantSMIG.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxMontantSalaireNet_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxMontantSalaireNet.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxTauxCNSSpatronal_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxTauxCNSSpatronal.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9.]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxTauxCNSSemploye_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxTauxCNSSemploye.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9.]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub tbxTauxIRPP_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles tbxTauxIRPP.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9.]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxFacturePUsociete_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxFacturePUsociete.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxFacturePUparticurier_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxFacturePUparticurier.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxFacturePUminimum_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxFacturePUminimum.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxTauxTVA_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxTauxTVA.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9.]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxTauxReduction_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TbxTauxReduction.PreviewTextInput
        If Not Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9.]$") Then
            e.Handled = True ' Bloque la saisie
        End If
    End Sub

    Private Sub TbxFacturePUsociete_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxFacturePUsociete.TextChanged, TbxFacturePUparticurier.TextChanged, TbxFacturePUminimum.TextChanged, TbxTauxTVA.TextChanged, TbxTauxReduction.TextChanged
        Try
            If iFormLoaded Then
                If (IsNumeric(TbxFacturePUsociete.Text) And IsNumeric(TbxFacturePUparticurier.Text) And IsNumeric(TbxFacturePUminimum.Text) And IsNumeric(TbxTauxTVA.Text.Replace(".", ",")) And IsNumeric(TbxTauxReduction.Text.Replace(".", ","))) AndAlso (TbxFacturePUsociete.Text >= 5000 And TbxFacturePUparticurier.Text >= 5000 And TbxFacturePUminimum.Text >= 5000 And TbxTauxTVA.Text.Replace(".", ",") >= 0 And TbxTauxReduction.Text.Replace(".", ",") >= 0) Then
                    btnSaveFacturation.Visibility = Visibility.Visible
                Else
                    btnSaveFacturation.Visibility = Visibility.Collapsed
                End If
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub TbxMontantSalaireBase_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxMontantSalaireBase.TextChanged, TbxMontantSMIG.TextChanged, TbxMontantSalaireNet.TextChanged, TbxTauxCNSSpatronal.TextChanged, TbxTauxCNSSemploye.TextChanged, tbxTauxIRPP.TextChanged
        Try
            If iFormLoaded Then
                If (IsNumeric(TbxMontantSalaireBase.Text) And IsNumeric(TbxMontantSMIG.Text) And IsNumeric(TbxMontantSalaireNet.Text) And IsNumeric(TbxTauxCNSSpatronal.Text.Replace(".", ",")) And IsNumeric(TbxTauxCNSSemploye.Text.Replace(".", ",")) And IsNumeric(tbxTauxIRPP.Text.Replace(".", ","))) AndAlso (TbxMontantSalaireBase.Text >= 5000 And TbxMontantSMIG.Text >= 5000 And TbxMontantSalaireNet.Text >= 5000 And TbxTauxCNSSpatronal.Text.Replace(".", ",") >= 0 And TbxTauxCNSSemploye.Text.Replace(".", ",") >= 0 And tbxTauxIRPP.Text.Replace(".", ",") >= 0) Then
                    btnSaveSalaires.Visibility = Visibility.Visible
                Else
                    btnSaveSalaires.Visibility = Visibility.Collapsed
                End If
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
    Sub ActivBntSaveDates()
        Try
            If iFormLoaded Then
                If (DtpDateCreationSociete.SelectedDate.HasValue And IsNumeric(TbxAgeMinEmbauche.Text) And IsNumeric(TbxAgeRetraite.Text) And IsNumeric(TbxDatDebutEtatFinancierMensuel.Text) And IsNumeric(TbxDateFinEtatFinancierMensuel.Text) And IsNumeric(TbxNbJrDeCongeMensuel.Text.Replace(".", ","))) AndAlso (DtpDateCreationSociete.SelectedDate.Value <= Today And TbxAgeMinEmbauche.Text >= 7 And TbxAgeRetraite.Text <= 100 And TbxDatDebutEtatFinancierMensuel.Text >= 0 And TbxDateFinEtatFinancierMensuel.Text >= 0 And TbxNbJrDeCongeMensuel.Text.Replace(".", ",") >= 0) AndAlso (TbxDatDebutEtatFinancierMensuel.Text <= 28 And TbxDateFinEtatFinancierMensuel.Text <= 28 And TbxNbJrDeCongeMensuel.Text.Replace(".", ",") <= 15) Then
                    btnSaveDateAge.Visibility = Visibility.Visible
                Else
                    btnSaveDateAge.Visibility = Visibility.Collapsed
                End If
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub DtpDateCreationSociete_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DtpDateCreationSociete.SelectedDateChanged
        ActivBntSaveDates()
    End Sub

    Private Sub TbxAgeMinEmbauche_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxAgeMinEmbauche.TextChanged, TbxAgeRetraite.TextChanged, TbxDatDebutEtatFinancierMensuel.TextChanged, TbxDateFinEtatFinancierMensuel.TextChanged, TbxNbJrDeCongeMensuel.TextChanged
        ActivBntSaveDates()
    End Sub
    Sub AffectDates()
        Try
            If iFormLoaded Then
                If IsNumeric(TbxDateFinEtatFinancierMensuel.Text) Then
                    If cbxMoisFin.SelectedIndex = 0 Then
                        iDateFin = -CInt(TbxDateFinEtatFinancierMensuel.Text)
                    Else
                        iDateFin = CInt(TbxDateFinEtatFinancierMensuel.Text)
                    End If
                End If
            End If

            If iFormLoaded Then
                If IsNumeric(TbxDateFinEtatFinancierMensuel.Text) Then
                    If cbxMoisDébut.SelectedIndex = 0 Then
                        iDateDebut = -CInt(TbxDatDebutEtatFinancierMensuel.Text)
                    Else
                        iDateDebut = CInt(TbxDatDebutEtatFinancierMensuel.Text)
                    End If
                End If
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub btnSaveDateAge_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveDateAge.Click
        AffectDates()
        Dim iStr As String = ""
        iStr &= "update Sets  "
        iStr &= " set "
        iStr &= " DateCreationSociete='" & DtpDateCreationSociete.SelectedDate.Value.ToString("yyyy-MM-dd") & "', "
        iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        iStr &= " AgeRetraite=" & TbxAgeRetraite.Text & ", "
        iStr &= " DateDebutEtatFinancierMensuel=" & iDateDebut & ", "
        iStr &= " DateFinEtatFinancierMensuel=" & iDateFin & ", "
        iStr &= " NbJrDeCongeMensuel=" & TbxNbJrDeCongeMensuel.Text & " "
        'iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        'iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        'iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        'iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        iStr &= " where IdSet=1 "
        Try
            If SaveNewData(iStr, "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") Then
                btnSaveDateAge.Visibility = Visibility.Collapsed
                LoadDataSettings()
            End If
        Catch ex As Exception
            MsgBox("Errer d'enregistrement des données")
        End Try
    End Sub

    Private Sub btnSaveSalaires_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveSalaires.Click
        Dim iStr As String = ""
        iStr &= "update Sets  "
        iStr &= " set "
        iStr &= " MontantSalaireBase=" & TbxMontantSalaireBase.Text & ", "
        iStr &= " MontantSMIG=" & TbxMontantSMIG.Text & ", "
        iStr &= " MontantSalaireNet=" & TbxMontantSalaireNet.Text & ", "
        iStr &= " TauxCNSSpatronal=" & TbxTauxCNSSpatronal.Text & ", "
        iStr &= " TauxCNSSemploye=" & TbxTauxCNSSemploye.Text & ", "
        iStr &= " TauxIRPP=" & tbxTauxIRPP.Text & " "
        'iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        'iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        'iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        'iStr &= " AgeMinEmbauche='" & TbxAgeMinEmbauche.Text & "', "
        iStr &= " where IdSet=1 "
        Try
            If SaveNewData(iStr, "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") Then
                btnSaveSalaires.Visibility = Visibility.Collapsed
                LoadDataSettings()
            End If
        Catch ex As Exception
            MsgBox("Errer d'enregistrement des données")
        End Try
    End Sub

    Private Sub btnSaveFacturation_Click(sender As Object, e As RoutedEventArgs) Handles btnSaveFacturation.Click
        Dim iStr As String = ""
        iStr &= "update Sets  "
        iStr &= " set "
        iStr &= " FacturePUsociete=" & TbxFacturePUsociete.Text & ", "
        iStr &= " FacturePUparticurier=" & TbxFacturePUparticurier.Text & ", "
        iStr &= " FacturePUminimum=" & TbxFacturePUminimum.Text & ", "
        iStr &= " TauxTVA=" & TbxTauxTVA.Text & ", "
        iStr &= " TauxReduction=" & TbxTauxReduction.Text & " "
        iStr &= " where IdSet=1 "
        Try
            If SaveNewData(iStr, "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") Then
                btnSaveFacturation.Visibility = Visibility.Collapsed
                LoadDataSettings()
            End If
        Catch ex As Exception
            MsgBox("Errer d'enregistrement des données")
        End Try

    End Sub
End Class
