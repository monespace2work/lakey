Public Class usercSociete
    Public Shared AfficheInfosStructure As Action
    'Dim retourDansMainWindow As Action
    Public Sub New(callback As Action)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        'retourDansMainWindow = callback
    End Sub
    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        InitialFrm()
    End Sub
    Sub InitialFrm()
        LoadPrimSettings()
        ' AfficheInfosStructure?.Invoke()
        AfficheInfosStructure?.Invoke()
        Try
            If IO.File.Exists(SocieteLogoPath) Then
                'Dim iUri uri 
                imgLogo.Source = New BitmapImage(New Uri(SocieteLogoPath))
            Else
                imgLogo.Source = New BitmapImage(New Uri("/MyIco.png"))
                'E:\repos\Lakey\Ressources Pad\hors_connexion2.png
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        tbxSteNom.Text = SocieteName
        tbxSteAdress.Text = SocieteAdress
        tbxSteSigle.Text = SocieteSigle
        tbxSteVille.Text = SocieteVille
        tbxStePays.Text = SocietePays
        tbxSteTelephone.Text = SocieteTelephone
        tbxSteBP.Text = SocieteCodePostal
        tbxSteMail.Text = SocieteMail
        tbxSteWeb.Text = SocieteWebSite
        tbxSteWeb.Text = SocieteWebSite
        tbxSteNif.Text = SocieteNIF
        tbxSteCnss.Text = SocieteNCNSS
        tbxSteRccm.Text = SocieteNRCCM
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        InitialFrm()
    End Sub

    Private Sub tbxSteNom_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteNom.TextChanged
        If tbxSteNom.Text.Length > 1 Then SocieteName = tbxSteNom.Text
    End Sub

    Private Sub tbxSteSigle_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteSigle.TextChanged
        SocieteSigle = tbxSteSigle.Text.Trim 'If tbxSteSigle.Text.Length > 0 Then SocieteSigle = tbxSteSigle.Text
    End Sub

    Private Sub tbxSteAdress_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteAdress.TextChanged
        If tbxSteAdress.Text.Length > 1 Then SocieteAdress = tbxSteAdress.Text
    End Sub

    Private Sub tbxSteVille_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteVille.TextChanged
        If tbxSteVille.Text.Length > 1 Then SocieteVille = tbxSteVille.Text
    End Sub

    Private Sub tbxStePays_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxStePays.TextChanged
        If tbxStePays.Text.Length > 1 Then SocietePays = tbxStePays.Text
    End Sub

    Private Sub tbxSteTelephone_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteTelephone.TextChanged
        If tbxSteTelephone.Text.Length >= 0 Then SocieteTelephone = tbxSteTelephone.Text
    End Sub

    Private Sub tbxSteBP_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteBP.TextChanged
        If tbxSteBP.Text.Length >= 0 Then SocieteCodePostal = tbxSteBP.Text
    End Sub

    Private Sub tbxSteNif_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteNif.TextChanged
        If tbxSteNif.Text.Length >= 0 Then SocieteNIF = tbxSteNif.Text
    End Sub

    Private Sub tbxSteRccm_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteRccm.TextChanged
        If tbxSteRccm.Text.Length >= 0 Then SocieteNRCCM = tbxSteRccm.Text
    End Sub

    Private Sub tbxSteCnss_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteCnss.TextChanged
        If tbxSteCnss.Text.Length >= 0 Then SocieteNCNSS = tbxSteCnss.Text
    End Sub

    Private Sub tbxSteWeb_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteWeb.TextChanged
        If tbxSteWeb.Text.Length >= 0 Then SocieteWebSite = tbxSteWeb.Text
    End Sub

    Private Sub tbxSteMail_TextChanged(sender As Object, e As TextChangedEventArgs) Handles tbxSteMail.TextChanged
        If tbxSteMail.Text.Length >= 0 Then SocieteMail = tbxSteMail.Text
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Dim iStr As String = ""
        iStr &= "update society  "
        iStr &= " set "
        iStr &= " "
        iStr &= " NomSociete='" & Apost(SocieteName) & "', Sigle='" & Apost(SocieteSigle) & "', Adresse='" & Apost(SocieteAdress) & "', "
        iStr &= " VilleSiege='" & Apost(SocieteVille) & "', PaysSiege='" & Apost(SocietePays) & "', Telephone='" & Apost(SocieteTelephone) & "', CodePostal='" & Apost(SocieteCodePostal) & "', "
        iStr &= " NumFiscal='" & Apost(SocieteNIF) & "', NumRCom='" & Apost(SocieteNRCCM) & "', NumSocial='" & Apost(SocieteNCNSS) & "', "
        iStr &= " MailAdress='" & Apost(SocieteMail) & "', WebSite='" & Apost(SocieteWebSite) & "', "
        iStr &= " LogoPath='" & Apost(SocieteLogoPath) & "' "
        iStr &= " where idx=1 "
        'iStr &= " "
        'iStr &= " "
        If SaveNewData(iStr, "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") Then
            Dim iLogoPath As String = IO.Path.GetDirectoryName(DbPathGeneral) & "\iLgo" & IO.Path.GetExtension(SocieteLogoPath)
            Try
                IO.File.Copy(SocieteLogoPath, iLogoPath, True)
            Catch ex As Exception
                MsgBox(ex.Message & " => " & iLogoPath)
            End Try

            'AfficheInfosStructure?.Invoke()
            MsgBox("Données enregistées avec succès")

            InitialFrm()
        Else
            MsgBox("Ereeur d'enregistrement")
        End If
    End Sub

    Private Sub imgLogo_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles imgLogo.MouseDown
        Dim dlg As New Microsoft.Win32.OpenFileDialog()
        dlg.Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
        If dlg.ShowDialog() = True Then
            Dim userImage = New BitmapImage(New Uri(dlg.FileName))
            imgLogo.Source = userImage
            SocieteLogoPath = dlg.FileName ' Appliquer comme arrière-plan
        End If
    End Sub
End Class
