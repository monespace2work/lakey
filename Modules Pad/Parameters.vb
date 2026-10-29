Imports System.Data

Module Parameters
    Public SocieteName As String = "JULIO BAG CORP"
    Public SocieteSigle As String = "JPBC"
    Public SocieteAdress As String = "Kara"
    Public SocieteVille As String = "Kara"
    Public SocietePays As String = "TOGO"
    Public SocieteTelephone As String = "+228 00 00 00 00"
    Public SocieteCodePostal As String = "00 BP 0000"
    Public SocieteNIF As String = "0000000"
    Public SocieteNCNSS As String = "0000000"
    Public SocieteNRCCM As String = "0000000"
    Public SocieteMail As String = "contact@juliobag.jpb"
    Public SocieteWebSite As String = "juliobag.jpb"
    Public SocieteLogoPath As String = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
    Public DbPath As String = "C:\LkyRess\ManA.sqlite"
    Public DbPathGeneral As String = ""
    Public LocalModeBb As Boolean = True
    'Public DbPath As String = "C:\Users\Public\MYDO\RM\GestAPS.accdb"
    Public RootPath As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JPB\LKY")
    Public UserPath As String = "C:\LkyRess\" 'IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JPB\LKY")
    Public UserName As String = "JuioBag"
    Public UserLevel As Integer = 0
    Public UserPW As String = "JPB"
    Public SessionConnxStr As String = ""
    Public IdxSession As Integer = 0

    Public isLoadedTbord As Boolean = False
    Public isLoadedAgent As Boolean = False
    Public isLoadedTraitement As Boolean = False
    Public isLoadedClient As Boolean = False
    Public isLoadedStructure As Boolean = False
    Public isLoadedSupplement As Boolean = False
    Public isLoadedSettings As Boolean = False
    Public IdSession As Integer = 0

    Public DGVloadingLink As String = ""

    Public Priority1 As String
    Public Priority2 As String
    Public Priority3 As String
    Public Priority4 As String
    Public Priority5 As String
    Public Priority As String

    Public MssTxt As String = ""

    Public SearchCase As Integer = 0
    Public ApsCbx As ComboBox
    Public SiteCbx As ComboBox
    Public MotifCbx As ComboBox
    Public MNatureContratCbx As ComboBox
    Public TypeContratCbx As ComboBox
    Public CNSSFrmTemp As Boolean = False
    Public ComptaFrmTemp As Boolean = False


    Public IdRow As Integer = 0
    Public IdColumn As Integer = 0
    Public DureeLimitRenouvellementContrat As Integer = 30
    Public ErrAffiche As String = ""

    Public IdxOp As Integer = 0
    Public MaxOp As Integer = 100

    Public AgeRetraite As Integer = 55
    Public AgeMinEmbauche As Integer = 18 'age légale de travail autorisé
    Public DateCreationSociete As Date = #04/01/2020#

    Public DateDebutEtatFinancierMensuel As Integer = -25
    Public DateFinEtatFinancierMensuel As Integer = 24
    Public NbJrDeCongeMensuel As Double = 2.5

    Public MontantSalaireBase As Integer = 52500
    Public MontantSMIG As Integer = 52500
    Public MontantSalaireNet As Integer = 52500

    Public TauxCNSSpatronal As Double = 22.5
    Public TauxCNSSemploye As Double = 9
    Public TauxIRPP As Double = 0

    Public FacturePUsociete As Integer = 115000
    Public FacturePUparticurier As Integer = 105000
    Public FacturePUminimum As Integer = 100000

    Public TauxTVA As Double = 18
    Public TauxReduction As Double = 0

    Public VilleSiege As String = "Lomé"
    Public StatMsg As String = "Statistiques"
    Public FonctionDefault As String = "APS - Stagiaire"
    Public DureeActualisationAffichage As Integer = 600000
    Public DureeBackUpDB As Integer = 900000
    Public DureeRefreshDB As Integer = 3000

    Public ToDayDTable As New DataTable
    Public WeekDTable As DataTable
    Public ContratDTable As DataTable
    Public AgentsDTable As New DataTable
    Public ClientDTable As New DataTable
    Public AbsenceDTable As DataTable
    Public StructureDTable As DataTable

    Public RappelNotesCbx As New ComboBox
    Public BUfilesCbx As New ComboBox
    Public ReplaceYN As Boolean
    Public BUDirPath As String

    Public TxtStatAgent As String = ""

    Public Id As Integer = 0
    Public ProfilName As String = ""
    Public CanUserManage As Boolean = False
    Public CanGlobalDataManage As Boolean = False
    Public CanSocieteDataManage As Boolean = False
    Public CanAddDataOrdinary As Boolean = False
    Public CanModifyDataOrdinary As Boolean = False
    Public CanDeleteDataOrdinary As Boolean = False
    Public CanReadDataOrdinary As Boolean = False
    Public CanAddDataSpecial As Boolean = False
    Public CanModifyDataSpecial As Boolean = False
    Public CanDeleteDataSpecial As Boolean = False
    Public CanReadDataSpecial As Boolean = False
    Public DescriptionProfil As String = ""

    Public ReadOnly Property TxtStatClient As String
        Get
            Return StatAgent()
        End Get
    End Property
End Module
Public Class DtAgent
    Public Nomp As String
    Public Contact As String
    Public Sexe As String
End Class
