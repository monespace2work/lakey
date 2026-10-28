Imports System.Data
Imports System.Data.SQLite
Imports System.IO
'Imports System.Windows.Forms
Module DbManipulation
    Public MyConnection As SQLiteConnection
    Public MyCommand As SQLiteCommand
    Public iConnectStr As String = "Data Source=" & DbPath & ";Version=3;"
    Public MyCmdStr As String = ""
    Public CmdStrTemp As String = ""
    Public TempTable As New DataTable
    Public iMinValue As Integer = 120
    Public iMaxValue As Integer = 250
    Public CasValidation As Integer = 0

    Public Enum FonctonStat
        Max
        Min
        Somme
        Moyenne
    End Enum
    Public Function LoadPrimSettings() As Boolean
        LoadPrimSettings = False
        DbPathGeneral = PaphBackUp()
        Dim iStrConx As String = "Data Source=C:\LkyRess\MySet.sqlite;Version=3;"
        'iConnectStr = "Data Source=C:\LkyRess\MySet.sqlite;Version=3;"
        MyCmdStr = " Select s.NomSociete, s.Sigle, s.Adresse, s.VilleSiege,s.PaysSiege, s.Telephone, s.CodePostal, s.NumFiscal,s.NumRCom,s.NumSocial,s.MailAdress,s.WebSite,s.LogoPath"
        MyCmdStr += " from society s "
        MyCmdStr += " where s.idx=1"
        'MyCmdStr += " order by ag.NomPrenoms ; "
        Try
            Using iConnex As New SQLiteConnection(iStrConx)
                iConnex.Open()
                Using iCmd As New SQLiteCommand(MyCmdStr, iConnex)
                    Using iReader As SQLiteDataReader = iCmd.ExecuteReader
                        Do While iReader.Read
                            If Not IsDBNull(iReader(0)) Then
                                SocieteName = iReader(0)
                            Else
                                SocieteName = ""
                            End If
                            If Not IsDBNull(iReader(1)) Then
                                SocieteSigle = iReader(1)
                            Else
                                SocieteSigle = ""
                            End If
                            If Not IsDBNull(iReader(2)) Then
                                SocieteAdress = iReader(2)
                            Else
                                SocieteAdress = ""
                            End If
                            If Not IsDBNull(iReader(3)) Then
                                SocieteVille = iReader(3)
                            Else
                                SocieteVille = ""
                            End If
                            If Not IsDBNull(iReader(4)) Then
                                SocietePays = iReader(4)
                            Else
                                SocietePays = ""
                            End If
                            If Not IsDBNull(iReader(5)) Then
                                SocieteTelephone = iReader(5)
                            Else
                                SocieteTelephone = ""
                            End If
                            If Not IsDBNull(iReader(6)) Then
                                SocieteCodePostal = iReader(6)
                            Else
                                SocieteCodePostal = ""
                            End If
                            If Not IsDBNull(iReader(7)) Then
                                SocieteNIF = iReader(7)
                            Else
                                SocieteNIF = ""
                            End If
                            If Not IsDBNull(iReader(8)) Then
                                SocieteNCNSS = iReader(8)
                            Else
                                SocieteNCNSS = ""
                            End If
                            If Not IsDBNull(iReader(9)) Then
                                SocieteNRCCM = iReader(9)
                            Else
                                SocieteNRCCM = ""
                            End If
                            If Not IsDBNull(iReader(10)) Then
                                SocieteMail = iReader(10)
                            Else
                                SocieteMail = ""
                            End If
                            If Not IsDBNull(iReader(11)) Then
                                SocieteWebSite = iReader(11)
                            Else
                                SocieteWebSite = ""
                            End If
                            If Not IsDBNull(iReader(12)) Then
                                If File.Exists(iReader(12)) Then
                                    SocieteLogoPath = iReader(12)
                                Else
                                    SocieteLogoPath = My.Application.Info.DirectoryPath & "\MyIco.png"
                                End If
                            Else
                                'SocieteLogoPath = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
                            End If

                            LoadPrimSettings = True
                        Loop

                    End Using
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            LoadPrimSettings = False
        End Try
    End Function

    Public Function LoadDataSettings() As Boolean
        LoadDataSettings = False
        DbPathGeneral = PaphBackUp()
        Dim iStrConx As String = "Data Source=C:\LkyRess\MySet.sqlite;Version=3;"
        'iConnectStr = "Data Source=C:\LkyRess\MySet.sqlite;Version=3;"
        MyCmdStr = " Select DBpathG, BckUpDir, AgeRetraite, AgeMinEmbauche,"
        MyCmdStr += " date(DateCreationSociete),DateDebutEtatFinancierMensuel,DateFinEtatFinancierMensuel,NbJrDeCongeMensuel, "
        MyCmdStr += " MontantSalaireBase,MontantSMIG,MontantSalaireNet, "
        MyCmdStr += " TauxCNSSpatronal,TauxCNSSemploye,TauxIRPP, "
        MyCmdStr += " FacturePUsociete,FacturePUparticurier,FacturePUminimum, "
        MyCmdStr += " TauxTVA,TauxReduction" 'TauxIRPP "
        MyCmdStr += " from Sets s "
        MyCmdStr += " where IdSet=1"
        'MyCmdStr += " order by ag.NomPrenoms ; "
        Try
            Using iConnex As New SQLiteConnection(iStrConx)
                iConnex.Open()
                Using iCmd As New SQLiteCommand(MyCmdStr, iConnex)
                    Using iReader As SQLiteDataReader = iCmd.ExecuteReader
                        Do While iReader.Read
                            If Not IsDBNull(iReader(0)) Then
                                DbPathGeneral = iReader(0)
                            Else
                                ' DbPathGeneral = ""
                            End If
                            If Not IsDBNull(iReader(1)) Then
                                BUDirPath = iReader(1)
                            Else
                                ' SocieteSigle = ""
                            End If
                            If Not IsDBNull(iReader(2)) Then
                                AgeRetraite = iReader(2)
                            Else
                                'SocieteAdress = ""
                            End If
                            If Not IsDBNull(iReader(3)) Then
                                AgeMinEmbauche = iReader(3)
                            Else
                                'SocieteVille = ""
                            End If
                            If Not IsDBNull(iReader(4)) Then
                                DateCreationSociete = CDate(iReader(4))
                            Else
                                'SocietePays = ""
                            End If
                            If Not IsDBNull(iReader(5)) Then
                                DateDebutEtatFinancierMensuel = iReader(5)
                            Else
                                'SocieteTelephone = ""
                            End If
                            If Not IsDBNull(iReader(6)) Then
                                DateFinEtatFinancierMensuel = iReader(6)
                            Else
                                'SocieteCodePostal = ""
                            End If
                            If Not IsDBNull(iReader(7)) Then
                                NbJrDeCongeMensuel = iReader(7)
                            Else
                                'SocieteNIF = ""
                            End If
                            If Not IsDBNull(iReader(8)) Then
                                MontantSalaireBase = iReader(8)
                            Else
                                'SocieteNCNSS = ""
                            End If
                            If Not IsDBNull(iReader(9)) Then
                                MontantSMIG = iReader(9)
                            Else
                                'SocieteNRCCM = ""
                            End If
                            If Not IsDBNull(iReader(10)) Then
                                MontantSalaireNet = iReader(10)
                            Else
                                'SocieteMail = ""
                            End If
                            If Not IsDBNull(iReader(11)) Then
                                TauxCNSSpatronal = iReader(11)
                            Else
                                'SocieteWebSite = ""
                            End If
                            If Not IsDBNull(iReader(12)) Then
                                TauxCNSSemploye = iReader(12)
                            Else
                                'SocieteLogoPath = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
                            End If
                            If Not IsDBNull(iReader(13)) Then
                                TauxIRPP = iReader(13)
                            Else
                                'SocieteLogoPath = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
                            End If
                            If Not IsDBNull(iReader(14)) Then
                                FacturePUsociete = iReader(14)
                            Else
                                'SocieteLogoPath = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
                            End If
                            If Not IsDBNull(iReader(15)) Then
                                FacturePUparticurier = iReader(15)
                            Else
                                'SocieteLogoPath = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
                            End If
                            If Not IsDBNull(iReader(16)) Then
                                FacturePUminimum = iReader(16)
                            Else
                                'SocieteLogoPath = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
                            End If
                            If Not IsDBNull(iReader(17)) Then
                                TauxTVA = iReader(17)
                            Else
                                'SocieteLogoPath = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
                            End If
                            If Not IsDBNull(iReader(18)) Then
                                TauxReduction = iReader(18)
                            Else
                                'SocieteLogoPath = My.Application.Info.DirectoryPath & "\JPB\ManAps\ManAps_JPG.jpg"
                            End If

                            LoadDataSettings = True
                        Loop

                    End Using
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            LoadDataSettings = False
        End Try
    End Function

    Public Function PaphBackUp(Optional iConnectStr As String = "Data Source=C:\LkyRess\MySet.sqlite;Version=3;") As String
        Dim iCmdStr As String = "SELECT DBpathG "
        iCmdStr += " FROM Sets "
        iCmdStr += " WHERE IdSet=1 "
        Dim localDatabasePath As String = Path.Combine(UserPath, "ManA.sqlite")
        Dim configuredDatabasePath As String = Nothing
        PaphBackUp = "C:\LkyRess\ManA.sqlite"
        If File.Exists(localDatabasePath) Then PaphBackUp = localDatabasePath
        Try
            Using iconexion As New SQLiteConnection(iConnectStr)
                iconexion.Open()
                Using icmd As New SQLiteCommand(iCmdStr, iconexion)
                    Using iReader As SQLiteDataReader = icmd.ExecuteReader
                        If iReader.Read() AndAlso Not IsDBNull(iReader(0)) Then
                            configuredDatabasePath = Convert.ToString(iReader(0)).Trim()
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Impossible de lire le chemin de la base générale dans MySet.sqlite : " & ex.Message)
        End Try
        If Not String.IsNullOrWhiteSpace(configuredDatabasePath) Then PaphBackUp = configuredDatabasePath
    End Function

    Public Function IsSQLiteDatabaseHealthy(databasePath As String, ByRef errorMessage As String) As Boolean
        errorMessage = ""
        If String.IsNullOrWhiteSpace(databasePath) OrElse Not File.Exists(databasePath) Then
            errorMessage = "Le fichier de base de données est introuvable."
            Return False
        End If

        Try
            Dim connectionString As New SQLiteConnectionStringBuilder With {
                .DataSource = databasePath,
                .Version = 3,
                .ReadOnly = True
            }
            Using connection As New SQLiteConnection(connectionString.ToString())
                connection.Open()
                Using command As New SQLiteCommand("PRAGMA quick_check;", connection)
                    Dim result As String = Convert.ToString(command.ExecuteScalar())
                    If Not String.Equals(result, "ok", StringComparison.OrdinalIgnoreCase) Then
                        errorMessage = If(String.IsNullOrWhiteSpace(result), "Le contrôle SQLite n'a renvoyé aucun résultat.", result)
                        Return False
                    End If
                End Using
            End Using
            Return True
        Catch ex As Exception
            errorMessage = ex.Message
            Return False
        End Try
    End Function

    Public Sub CreateValidatedSQLiteSnapshot(sourcePath As String, destinationPath As String)
        Dim fullSourcePath As String = Path.GetFullPath(sourcePath)
        Dim fullDestinationPath As String = Path.GetFullPath(destinationPath)
        If String.Equals(fullSourcePath, fullDestinationPath, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("La source et la destination de la copie SQLite sont identiques.")
        End If

        Dim destinationDirectory As String = Path.GetDirectoryName(fullDestinationPath)
        If String.IsNullOrWhiteSpace(destinationDirectory) Then
            Throw New InvalidOperationException("Le dossier de destination de la base SQLite est invalide.")
        End If
        Directory.CreateDirectory(destinationDirectory)

        Dim temporaryPath As String = Path.Combine(
            destinationDirectory,
            Path.GetFileName(fullDestinationPath) & "." & Guid.NewGuid().ToString("N") & ".tmp")

        Try
            SQLiteConnection.CreateFile(temporaryPath)
            Dim sourceConnectionString As New SQLiteConnectionStringBuilder With {
                .DataSource = fullSourcePath,
                .Version = 3,
                .ReadOnly = True
            }
            Using source As New SQLiteConnection(sourceConnectionString.ToString())
                source.Open()
                Using checkCommand As New SQLiteCommand("PRAGMA quick_check;", source)
                    Dim checkResult As String = Convert.ToString(checkCommand.ExecuteScalar())
                    If Not String.Equals(checkResult, "ok", StringComparison.OrdinalIgnoreCase) Then
                        Throw New InvalidDataException("La base SQLite source est invalide : " & checkResult)
                    End If
                End Using

                Using destination As New SQLiteConnection("Data Source=" & temporaryPath & ";Version=3;")
                    destination.Open()
                    source.BackupDatabase(destination, "main", "main", -1, Nothing, 1000)
                End Using
            End Using

            Dim validationError As String = Nothing
            If Not IsSQLiteDatabaseHealthy(temporaryPath, validationError) Then
                Throw New InvalidDataException("La copie SQLite temporaire est invalide : " & validationError)
            End If

            If File.Exists(fullDestinationPath) Then
                File.Replace(temporaryPath, fullDestinationPath, Nothing)
            Else
                File.Move(temporaryPath, fullDestinationPath)
            End If
        Finally
            If File.Exists(temporaryPath) Then
                Try
                    File.Delete(temporaryPath)
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine("Impossible de supprimer la copie SQLite temporaire : " & ex.Message)
                End Try
            End If
        End Try
    End Sub

    Public Function isLocalModeDd() As Boolean
        Return LocalModeBb
    End Function
    Public Function AgentAffiche(Optional MyDateFormat As String = "yyyy-MM-dd") As DataTable
        'MyDtable.Columns.Clear()
        Dim MyDtable As New DataTable
        iConnectStr = "Data Source=" & DbPath & ";Version=3;"
        MyCmdStr = " Select ag.NomPrenoms As [Nom & prénons(s)], ag.Sexe As Sexe , af.SiteAffect As Site , ag.Contacts , ag.NumMatr [N° Matricule], sit.Localisation As Ville, ag.Fonction, format(af.DateAffect) DebutAffectation"
        MyCmdStr += " from agents ag join affectation af On ag.NomPrenoms =af.AgentAffect join siteaffectation sit On sit.NomSite=af.SiteAffect "
        MyCmdStr += " where af.FinAffect >='" & Today.ToString("yyyy-MM-dd") & "' "
        MyCmdStr += " order by ag.NomPrenoms ; "

        Try
            Using MyConnect As New SQLiteConnection(iConnectStr)
                MyConnect.Open()
                Using myCommand As New SQLiteCommand(MyCmdStr, MyConnect)
                    Using myReader As SQLiteDataReader = myCommand.ExecuteReader
                        MyDtable.Columns.Add("Nom & prénons(s)")
                        MyDtable.Columns.Add("Sexe")
                        MyDtable.Columns.Add("Site")
                        MyDtable.Columns.Add("Contacts")
                        MyDtable.Columns.Add("N° Matricule")
                        MyDtable.Columns.Add("Ville")
                        MyDtable.Columns.Add("Fonction")
                        MyDtable.Columns.Add("Début affect")
                        While myReader.Read
                            Dim myvalue As Object
                            If Not IsDBNull(myReader(7)) Then
                                Try
                                    myvalue = Convert.ToDateTime(myReader(7)).ToString(MyDateFormat)
                                Catch ex As Exception
                                    myvalue = "1900/01/01"
                                End Try
                            Else
                                myvalue = "1900/01/01"
                            End If
                            MyDtable.Rows.Add(myReader(0), myReader(1), myReader(2), myReader(3), myReader(4), myReader(5), myReader(7), myvalue)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            'AgentAffiche = False
        End Try
        Return MyDtable
    End Function
    Public Function EvolTrimEffectif(DateReference As Date, Optional NbMois As Integer = 3, Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As DataTable
        Dim MyDtable As New DataTable
        MyDtable.Columns.Add("ELEMENTS")
        For idx As Integer = (NbMois - 1) To 0 Step -1
            MyDtable.Columns.Add(UCase(DateReference.AddMonths(-idx).ToString("MMMM yyyy")))
        Next
        iMinValue = 0
        iMaxValue = 0

        Try
            Dim N, N1, N2 As Integer
            MyDtable.Rows.Add("Nb d'agents")
            MyDtable.Rows.Add("Nb d'entrées")
            MyDtable.Rows.Add("Nb de sorties")
            Dim iidx As Integer = 0
            For ixd As Integer = (NbMois - 1) To 0 Step -1
                'MsgBox(ixd)
                N = NbAgentAyantTravaillePeriode(DebutDuMois(DateReference.AddMonths(-iidx)), FinDuMois(DateReference.AddMonths(-iidx)), ConnexStr)
                N1 = NbAgentEntreePeriode(DebutDuMois(DateReference.AddMonths(-iidx)), FinDuMois(DateReference.AddMonths(-iidx)), ConnexStr)
                N2 = NbAgentSortiePeriode(DebutDuMois(DateReference.AddMonths(-iidx)), FinDuMois(DateReference.AddMonths(-iidx)), ConnexStr)
                'MsgBox(ixd)
                MyDtable.Rows(0).Item(1 + ixd) = N
                MyDtable.Rows(1).Item(1 + ixd) = N1
                MyDtable.Rows(2).Item(1 + ixd) = N2

                iidx += 1

                If iMinValue >= N Then iMinValue = N
                If iMaxValue <= N Then iMaxValue = N

            Next
            'MsgBox(iMinValue & " < " & iMaxValue)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        EvolTrimEffectif = MyDtable

    End Function

    Public Function ClientAffiche() As Boolean

        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then DbPath = Path.Combine(UserPath, "ManA.sqlite") 'MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        iConnectStr = "Data Source=" & DbPath & ";Version=3;"

        MyCmdStr = "select sit.Client, count(*) as [Nb d'Agent], format(sit.DateCreation) as [début collab] "
        MyCmdStr += " from siteaffectation  sit join affectation af on sit.NomSite=af.SiteAffect "
        MyCmdStr += " where af.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "' "
        MyCmdStr += " group by sit.Client  "
        MyCmdStr += " order by sit.Client  "

        Try
            ClientDTable = LoadDataTable(MyCmdStr)
            ClientAffiche = True

        Catch ex As Exception
            MsgBox(ex.Message)
            ClientAffiche = False
        End Try
    End Function

    Public Function ClientInactifAffiche() As Boolean

        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then DbPath = Path.Combine(UserPath, "ManA.sqlite") 'MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        iConnectStr = "Data Source=" & DbPath & ";Version=3;"

        MyCmdStr = " select nomclient as Client, format(DateDebutCollabor) as Début, format(DateFinCollabor) as Fin, Ville "
        MyCmdStr += " FROM client "
        MyCmdStr += " WHERE actif =0 "
        MyCmdStr += " ORDER BY nomclient  "

        Try
            TempTable = LoadDataTable(MyCmdStr)
            ClientInactifAffiche = True

        Catch ex As Exception
            MsgBox(ex.Message)
            ClientInactifAffiche = False
        End Try
    End Function

    Public Function ClientSearchAffiche(ByVal SearchKey As String) As DataTable
        Dim iTable As New DataTable

        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then DbPath = Path.Combine(UserPath, "ManA.sqlite") 'MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        iConnectStr = "Data Source=" & DbPath & ";Version=3;"

        MyCmdStr = "select sit.Client, count(*) as [Nb d'Agent], format(sit.DateCreation) as [début collab] "
        MyCmdStr += " from siteaffectation  sit join affectation af on sit.NomSite=af.SiteAffect "
        MyCmdStr += " where af.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "' "
        MyCmdStr += "  And  sit.Client like  '%" & SearchKey & "%'"
        MyCmdStr += " group by sit.Client "
        MyCmdStr += " order by sit.Client  "
        Try
            iTable = LoadDataTable(MyCmdStr)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        ClientSearchAffiche = iTable
    End Function
    Public Function ClientInactifSearchTable(ByVal SearchKey As String) As DataTable
        Dim iTable As New DataTable
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then DbPath = Path.Combine(UserPath, "ManA.sqlite") 'MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        iConnectStr = "Data Source=" & DbPath & ";Version=3;"

        MyCmdStr = "select c.NomClient,format(c.DateDebutCollabor) as [début collab] ,format(c.DateFinCollabor) as [fin collab] ,c.Ville  from client c "
        MyCmdStr += " where c.actif=0" ' & Today.ToString("yyyy-MM-dd") & "' "
        MyCmdStr += "  And  (c.NomClient like  '%" & Apost(SearchKey) & "%'"
        MyCmdStr += "  or  c.Ville like  '%" & Apost(SearchKey) & "%')"
        MyCmdStr += " order by c.NomClient  "
        Try
            iTable = LoadDataTable(MyCmdStr)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        ClientInactifSearchTable = iTable
    End Function

    Public Function SturctureAffiche() As Boolean
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then DbPath = Path.Combine(UserPath, "ManA.sqlite") 'MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        iConnectStr = "Data Source=" & DbPath & ";Version=3;"

        MyCmdStr = "select sit.Localisation ,  count (* ) as NbrAgent "
        MyCmdStr += " from affectation af  join siteaffectation sit on sit.NomSite=af.SiteAffect  "
        MyCmdStr += " where af.FinAffect>='" & Today.ToString("yyyy-MM-dd") & "' "
        MyCmdStr += " group by sit.Localisation  "
        MyCmdStr += " ORDER BY sit.Localisation  "

        Try
            StructureDTable = LoadDataTable(MyCmdStr)
            SturctureAffiche = True
        Catch ex As Exception
            MsgBox(ex.Message)
            SturctureAffiche = False
        End Try
    End Function


    Public Function LoadDataTable(ByVal MyCommandString As String, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As DataTable
        'Dim myw As New WaitFrm
        'myw.Show()
        Dim MyDAdapter As SQLiteDataAdapter
        MyCmdStr = MyCommandString
        Dim TempTable As New DataTable
        Try
            Dim TempConnection As New SQLiteConnection
            Dim TempCommand As SQLiteCommand
            TempConnection.ConnectionString = MyConxStr
            TempConnection.Open()
            TempCommand = TempConnection.CreateCommand
            TempCommand.CommandText = MyCmdStr
            MyDAdapter = New SQLiteDataAdapter(TempCommand)
            TempCommand.Connection = TempConnection
            Dim MyDSet As New DataSet
            MyDAdapter.Fill(MyDSet, "MyP_table")
            TempTable = MyDSet.Tables("MyP_table")
            TempCommand.Dispose()
            TempConnection.Close()
            'LoadDataTable = True
            MyDAdapter.Dispose()
            MyDAdapter.Dispose()
        Catch ex As Exception
            MsgBox(ex.Message) ' & " => " & MyCommandString)
            'LoadDataTable = False
        End Try
        LoadDataTable = TempTable
        'myw.Close()
    End Function

    Public Function SaveNewData(ByVal iCommandString As String, Optional iConnectStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        SaveNewData = False
        Dim myw As New WaitFrm
        myw.Show()
        Try
            Using iconnx As New SQLiteConnection(iConnectStr)
                iconnx.Open()
                Using icmd As New SQLiteCommand(iCommandString, iconnx)
                    icmd.ExecuteNonQuery()
                    SaveNewData = True
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            'MsgBox(ex.Message)
            SaveNewData = False
        End Try
        myw.Close()
    End Function


    Public Function MayStatVal(ByVal MyCommandString As String, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        'iConnectStr = "Data Source=" & DbPath & ";Version=3;"
        Try
            Using iconexion As New SQLiteConnection(MyConxStr)
                iconexion.Open()
                Using icmd As New SQLiteCommand(MyCommandString, iconexion)
                    MayStatVal = icmd.ExecuteScalar()
                End Using
            End Using

        Catch ex As Exception
            MayStatVal = 0
        End Try
    End Function
    Public Function MyStatValStr(ByVal MyCommandString As String, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        'iConnectStr = "Data Source=" & DbPath & ";Version=3;"
        Try
            Using iconexion As New SQLiteConnection(MyConxStr)
                iconexion.Open()
                Using icmd As New SQLiteCommand(MyCommandString, iconexion)
                    MyStatValStr = icmd.ExecuteScalar()
                End Using
            End Using

        Catch ex As Exception
            MyStatValStr = ""
        End Try
    End Function
    Public Function AgeMax(Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        AgeMax = 0
        Dim MyString As String
        MyString = "select format(DateNaiss) as DateNaiss"
        MyString += " FROM agents "
        MyString += " WHERE Actif=1 order by DateNaiss limit 1 "
        Try
            Using iconexion As New SQLiteConnection(MyConxStr)
                iconexion.Open()
                Using icmd As New SQLiteCommand(MyString, iconexion)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader

                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            Dim idate As Date = Convert.ToDateTime(iReader(0))
                            AgeMax = Today.Year - idate.Year
                        End If
                    End While
                End Using
            End Using
        Catch ex As Exception
            AgeMax = 0
            MsgBox(ex.Message)
        End Try

        'MyMax = MayStatVal(MyString)
    End Function
    Public Function AgeMin(Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        AgeMin = 0
        Dim MyString As String
        MyString = "select format(DateNaiss) as DateNaiss"
        MyString += " FROM agents "
        MyString += " WHERE Actif=1 order by DateNaiss desc limit 1 "
        Try
            Using iconexion As New SQLiteConnection(MyConxStr)
                iconexion.Open()
                Using icmd As New SQLiteCommand(MyString, iconexion)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader

                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            Dim idate As Date = Convert.ToDateTime(iReader(0))
                            AgeMin = Today.Year - idate.Year
                        End If
                    End While
                End Using
            End Using
        Catch ex As Exception
            AgeMin = 0
            MsgBox(ex.Message)
        End Try

        'MyMax = MayStatVal(MyString)
    End Function
    Public Function AgeMoyAactif(Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        AgeMoyAactif = 0
        Dim MyString As String
        MyString = "select format(DateNaiss) as DateNaiss"
        MyString += " FROM agents "
        MyString += " WHERE Actif=1 "
        Try
            Using iconexion As New SQLiteConnection(MyConxStr)
                iconexion.Open()
                Using icmd As New SQLiteCommand(MyString, iconexion)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                    Dim NbElmt As Integer = 0
                    Dim Tages As Integer = 0
                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            Dim idate As Date = Convert.ToDateTime(iReader(0))
                            'MsgBox()
                            Tages += (Today.Year - idate.Year)
                            NbElmt += 1
                        End If
                    End While
                    If NbElmt > 0 Then
                        AgeMoyAactif = Tages / NbElmt
                    Else
                        AgeMoyAactif = 0
                    End If
                End Using
            End Using
        Catch ex As Exception
            AgeMoyAactif = 0
            MsgBox(ex.Message)
        End Try

        'MyMax = MayStatVal(MyString)
    End Function

    Public Function NbClientInactif() As Decimal
        Dim MyString As String
        MyString = "select count (* )  "
        MyString += " from client where actif=0 "
        'MyString += " where af.FinAffect <'" & Today.ToString("yyyy-MM-dd") & "' "
        NbClientInactif = MayStatVal(MyString)
    End Function
    Public Function ListeSiteClient(NomClient As String, Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListeSiteClient = ""
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then ConnexStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Dim iStr As String = "select c.NomSite,c.Actif from siteaffectation c where c.Client like '" & Apost(NomClient) & "' order by c.Actif desc, c.NomSite"
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then ConnexStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Using iconnx As New SQLiteConnection(ConnexStr)
            iconnx.Open()
            Using icmd As New SQLiteCommand(iStr, iconnx)
                Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                While iReader.Read
                    If Not IsDBNull(iReader(0)) Then
                        If iReader(1) Then
                            ListeSiteClient += iReader(0) & "   √" & Environment.NewLine
                        Else
                            ListeSiteClient += iReader(0) & Environment.NewLine
                        End If
                    End If
                End While
            End Using
        End Using
    End Function

    Public Function ListePosteSite(NomSite As String, Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListePosteSite = ""
        Dim iStr As String = "select c.Designation,c.Actif,c.Code from Postes c where c.NomSite like '" & Apost(NomSite) & "' order by c.Actif desc, c.Designation"
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then ConnexStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Using iconnx As New SQLiteConnection(ConnexStr)
            iconnx.Open()
            Using icmd As New SQLiteCommand(iStr, iconnx)
                Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                While iReader.Read
                    If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(2)) Then
                        If iReader(1) Then
                            ListePosteSite += iReader(0) & " > " & iReader(2) & "   √" & Environment.NewLine
                        Else
                            ListePosteSite += iReader(0) & " > " & iReader(2) & Environment.NewLine
                        End If
                    End If
                End While
            End Using
        End Using
    End Function

    Public Function ListePosteClient(NomClient As String, Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListePosteClient = ""
        Dim iStr As String = "select c.Designation||'  ('||c.Code||')',c.Actif from Postes c join siteaffectation s on s.NomSite=c.NomSite where s.Client='" & Apost(NomClient) & "' order by c.Actif desc, c.Designation"
        Try
            If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then ConnexStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
            Using iconnx As New SQLiteConnection(ConnexStr)
                iconnx.Open()
                Using icmd As New SQLiteCommand(iStr, iconnx)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                    Dim iNb As Integer = 1
                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            If iReader(1) Then
                                ListePosteClient += Right("000" & iNb, 3) & ". " & iReader(0) & "   √" & Environment.NewLine
                                iNb += 1
                            Else
                                ListePosteClient += Right("000" & iNb, 3) & ". " & iReader(0) & Environment.NewLine
                                iNb += 1
                            End If
                        End If
                    End While
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Function

    Public Function NbClientActif(Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        Dim MyString As String
        MyString = "SELECT Count(SiteAffectation_Tbl.Client) "
        MyString += " FROM SiteAffectation_Tbl "
        MyString += " WHERE (((SiteAffectation_Tbl.[Actif])=Yes)) "
        NbClientActif = MayStatVal(MyString, ConnexStr)
    End Function
    Public Function NbAgentActif(Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        Dim MyString As String
        MyString = "select count(*) from agents where Actif=1 "
        NbAgentActif = MayStatVal(MyString, ConnexStr)
    End Function
    Public Function NbAgentInactif(Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        Dim MyString As String
        MyString = "select count(*) from agents where Actif=0 "
        NbAgentInactif = MayStatVal(MyString, ConnexStr)
    End Function
    Public Function NbAgentFemmeActif(Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        Dim MyString As String
        MyString = "SELECT Count(*) "
        MyString += " FROM agents "
        MyString += " WHERE (Actif=1) AND (Sexe LIKE 'f') "
        NbAgentFemmeActif = MayStatVal(MyString, ConnexStr)
    End Function
    Public Function NbContatEnCoursValid(Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        Dim MyString As String
        MyString = "select count(*) from contratstbl where FinContr>='" & Today.ToString("yyyy-MM-dd") & "' "
        NbContatEnCoursValid = MayStatVal(MyString, ConnexStr)
    End Function
    Public Function NbAffectationEnCoursValid(Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        Dim MyString As String
        MyString = "select count(*) from affectation where FinAffect>='" & Today.ToString("yyyy-MM-dd") & "' "
        NbAffectationEnCoursValid = MayStatVal(MyString, ConnexStr)
    End Function

    Public Function NbContatEnCoursNonValid(Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        Dim MyString As String
        MyString = "select count(*) from contratstbl where FinContr<'" & Today.ToString("yyyy-MM-dd") & "' "
        NbContatEnCoursNonValid = MayStatVal(MyString, ConnexStr)
    End Function

    Public Function NbContatEnCoursJusquenFinDuMoisValid(Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Decimal
        Dim MyString As String
        MyString = "SELECT Count(*) "
        MyString += " FROM contratstbl  c"
        MyString += " WHERE (c.FinContr >=#" & Today.ToString("yyyy-MM-dd") & "# and c.FinContr <=#" & FinDuMois(Today).ToString("yyyy-MM-dd") & "#)"
        MsgBox(MyString)
        NbContatEnCoursJusquenFinDuMoisValid = MayStatVal(MyString, ConnexStr)
    End Function

    Public Function NbAgentAyantTravaillePeriode(Datedebut As Date, Datefin As Date, Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Integer
        NbAgentAyantTravaillePeriode = 0
        Dim NbEt, NbSt, NbSm As Integer
        Try
            Dim icmdstr As String = "select count(*) from agents a where a.DateEntree<='" & Datefin.ToString("yyyy-MM-dd") & "' and a.DateEntree not like '' "
            Using connx As New SQLiteConnection(ConnexStr)
                connx.Open()
                NbAgentAyantTravaillePeriode = 0
                Using mycmd As New SQLiteCommand(icmdstr, connx)
                    Try
                        NbEt = mycmd.ExecuteScalar()
                    Catch ex As Exception
                        MsgBox(ex.Message)
                        NbEt = 0
                    End Try

                End Using
            End Using
            icmdstr = "select count(*) from agents a where a.DateSortie<='" & Datefin.ToString("yyyy-MM-dd") & "' and a.DateSortie not like '' "
            Using connx As New SQLiteConnection(ConnexStr)
                connx.Open()
                Using mycmd As New SQLiteCommand(icmdstr, connx)
                    Try
                        NbSt = mycmd.ExecuteScalar()
                    Catch ex As Exception
                        MsgBox(ex.Message)
                        NbSt = 0
                    End Try
                End Using
            End Using
            icmdstr = "select count(*) from agents a  where a.DateSortie <'" & Datefin.ToString("yyyy-MM-dd") & "' and a.DateSortie >='" & Datedebut.ToString("yyyy-MM-dd") & "'"
            'MsgBox(icmdstr)
            Using connx As New SQLiteConnection(ConnexStr)
                connx.Open()
                Using mycmd As New SQLiteCommand(icmdstr, connx)
                    Try
                        NbSm = mycmd.ExecuteScalar()
                    Catch ex As Exception
                        'MsgBox(ex.Message)
                        NbSm = 0
                    End Try
                End Using
            End Using
            NbAgentAyantTravaillePeriode = NbEt - NbSt + NbSm
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try


    End Function
    Public Function NbAgentEntreePeriode(Datedebut As Date, Datefin As Date, Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Integer
        NbAgentEntreePeriode = 0
        Try
            Dim icmdstr As String = "select count(*) from agents a where a.DateEntree<='" & Datefin.ToString("yyyy-MM-dd") & "' and a.DateEntree >='" & Datedebut.ToString("yyyy-MM-dd") & "'"
            'MsgBox(icmdstr)
            Using connx As New SQLiteConnection(ConnexStr)
                connx.Open()
                Using mycmd As New SQLiteCommand(icmdstr, connx)
                    Try
                        NbAgentEntreePeriode = mycmd.ExecuteScalar()
                    Catch ex As Exception
                        MsgBox(ex.Message)
                        NbAgentEntreePeriode = 0
                    End Try

                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try


    End Function
    Public Function NbAgentSortiePeriode(Datedebut As Date, Datefin As Date, Optional ConnexStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Integer
        NbAgentSortiePeriode = 0
        Try
            Dim icmdstr = "select count(*) from agents a  where a.DateSortie <='" & Datefin.ToString("yyyy-MM-dd") & "' and a.DateSortie >='" & Datedebut.ToString("yyyy-MM-dd") & "'"
            ' MsgBox(icmdstr)
            Using connx As New SQLiteConnection(ConnexStr)
                connx.Open()
                Using mycmd As New SQLiteCommand(icmdstr, connx)
                    Try
                        NbAgentSortiePeriode += mycmd.ExecuteScalar()
                    Catch ex As Exception
                        MsgBox(ex.Message)
                        NbAgentSortiePeriode = 0
                    End Try
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try


    End Function

    Public Function FilmyCbx(MyCommandString As String, MyCbx As ComboBox, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        FilmyCbx = False
        Try
            Using iconn As New SQLiteConnection(MyConxStr)
                iconn.Open()
                Using icmd As New SQLiteCommand(MyCommandString, iconn)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                    MyCbx.Items.Clear()
                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            MyCbx.Items.Add(iReader(0))
                        End If
                    End While
                    FilmyCbx = True
                End Using

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
            FilmyCbx = False
        End Try
    End Function
    Public Function FilmyLbx(MyCommandString As String, MyCbx As ListBox, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        FilmyLbx = False
        Try
            Using iconn As New SQLiteConnection(MyConxStr)
                iconn.Open()
                Using icmd As New SQLiteCommand(MyCommandString, iconn)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                    MyCbx.Items.Clear()
                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            MyCbx.Items.Add(iReader(0))
                        End If
                    End While
                    FilmyLbx = True
                End Using

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
            FilmyLbx = False
        End Try
    End Function
    Public Function ComptTypeContrat(icmdst As String, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ComptTypeContrat = ""
        'MyConxStr = "select n.IdNature, count(IdContr) from contratstbl c join contratsnature n on c.NaturContr=n.NatureContr group by n.IdNature"
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Dim mytext As String = ""
        MsgBox(icmdst)
        Try
            Using iconn As New SQLiteConnection(MyConxStr)
                iconn.Open()
                Using icmd As New SQLiteCommand(icmdst, iconn)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                    'MyCbx.Items.Clear()
                    While iReader.Read
                        If Not IsDBNull(iReader(0)) Then
                            mytext += iReader(0)
                            MsgBox(mytext)
                        End If
                    End While
                    ComptTypeContrat = mytext
                    'FilmyCbx() = True
                End Using

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Function
    Public Function MyContratList(MyCommandString As String, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        MyContratList = "| "
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Try
            Using iconn As New SQLiteConnection(MyConxStr)
                iconn.Open()
                Using icmd As New SQLiteCommand(MyCommandString, iconn)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                    While iReader.Read
                        If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) Then
                            MyContratList += iReader(0) & " : " & iReader(1) & " | " 'Environment.NewLine
                        End If
                    End While
                End Using

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Function
    Public Function MyAffectationVilleList(MyCommandString As String, Optional MyConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        MyAffectationVilleList = "| "
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then MyConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Try
            Using iconn As New SQLiteConnection(MyConxStr)
                iconn.Open()
                Using icmd As New SQLiteCommand(MyCommandString, iconn)
                    Dim iReader As SQLiteDataReader = icmd.ExecuteReader
                    While iReader.Read
                        If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) Then
                            MyAffectationVilleList += iReader(0) & " : " & iReader(1) & " | " 'Environment.NewLine
                        End If
                    End While
                End Using

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Function

    Public Function DeleteData(iCommandString As String, Optional iConnectStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        DeleteData = False
        Dim myw As New WaitFrm
        myw.Show()
        Try
            Using iconnx As New SQLiteConnection(iConnectStr)
                iconnx.Open()
                Using icmd As New SQLiteCommand(iCommandString, iconnx)
                    icmd.ExecuteNonQuery()
                    DeleteData = True
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            DeleteData = False
        End Try
        myw.Close()
    End Function
    Public Function ListeAgentFinContrat(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListeAgentFinContrat = "=== CONTRATS A ECHEANCE ==="
        Dim istr As String = "select a.NomPrenoms||' => '|| n.IdNature ||' (' ||strftime('%d-%m-%Y', c.DebutContr)|| ' - '||strftime('%d-%m-%Y',c.FinContr)||')' as [Liste des Agents] "
        istr += " from ((contratstbl c join agents a on c.NomPrenoms=a.NumMatr) join contratsnature n on n.NatureContr=c.NaturContr) join affectation af on af.AgentAffect=a.NomPrenoms  " ' FinContr=#" & MaxDateFinContrat.ToString("MM/dd/yyyy") & "#  "
        istr += " where a.Actif=1 and c.FinContr>=date() and c.FinContr<=date('now','+1 month','-1 day') and af.FinAffect>=date() order by a.NomPrenoms "
        Dim iWord As String = ""
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    Dim iNb As Integer = 1
                    Do While iReader.Read
                        iWord += Right("000" & iNb, 3) & ". " & iReader(0) & Environment.NewLine
                        iNb += 1
                    Loop
                End Using
            End Using
            If iWord.Length > 0 Then
                Dim iLength As Integer = iWord.Length - 2
                If iLength > 0 Then iWord = Left(iWord, iLength)
            End If
        Catch ex As Exception
            MsgBox("code Error 200:" & ex.Message)
            iWord = ""
        End Try
        If iWord <> "" Then
            ListeAgentFinContrat += Environment.NewLine & iWord
        Else
            ListeAgentFinContrat = ""
        End If
    End Function
    Public Function ListePosteActuel(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListePosteActuel = ""
        Dim istr As String = "select a.Fonction, count(*) from agents a join fonction f on a.Fonction=f.Fonction where a.Actif=1 group by a.Fonction order by f.Niveau,f.Fonction "
        Dim iWord As String = ""
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    Dim iNb As Integer = 1
                    Do While iReader.Read
                        iWord += iNb & ". " & iReader(0) & " : " & iReader(1) & Environment.NewLine
                        iNb += 1
                    Loop
                End Using
            End Using
            If iWord.Length > 0 Then
                Dim iLength As Integer = iWord.Length - 2
                If iLength > 0 Then iWord = Left(iWord, iLength)
            End If
        Catch ex As Exception
            MsgBox("code Error 200:" & ex.Message)
            iWord = ""
        End Try
        If iWord <> "" Then
            ListePosteActuel += Environment.NewLine & iWord
        Else
            ListePosteActuel = ""
        End If
    End Function
    Public Function ListeAgentSortisActif(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListeAgentSortisActif = "=== AGENT ACTIF MAIS AYANT UNE DATE DE SORTIE ==="
        Dim istr As String = "select a.NomPrenoms||' => '|| a.DateSortie as [Liste des Agents] "
        istr += " from agents a  " ' FinContr=#" & MaxDateFinContrat.ToString("MM/dd/yyyy") & "#  "
        istr += " where a.actif =1 and ( a.DateSortie not null or a.DateSortie not like '' ) order by a.NomPrenoms "
        Dim iWord As String = ""
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    Dim iNb As Integer = 1
                    Do While iReader.Read
                        iWord += Right("000" & iNb, 3) & ". " & iReader(0) & Environment.NewLine
                        iNb += 1
                    Loop
                End Using
            End Using
            If iWord.Length > 0 Then
                Dim iLength As Integer = iWord.Length - 2
                If iLength > 0 Then iWord = Left(iWord, iLength)
            End If
        Catch ex As Exception
            MsgBox("code Error 200:" & ex.Message)
            iWord = ""
        End Try
        If iWord <> "" Then
            ListeAgentSortisActif += Environment.NewLine & iWord
        Else
            ListeAgentSortisActif = ""
        End If
    End Function
    Public Function ListeAgentInActifSansDateSortie(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListeAgentInActifSansDateSortie = "=== AGENT INACTIF SANS DATE DE SORTIE ==="
        Dim istr As String = "select a.NomPrenoms as [Liste des Agents] "
        istr += " from agents a  " ' FinContr=#" & MaxDateFinContrat.ToString("MM/dd/yyyy") & "#  "
        istr += " where a.actif =0 and ( a.DateSortie is null or a.DateSortie like '' ) order by a.NomPrenoms "
        Dim iWord As String = ""
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    Dim iNb As Integer = 1
                    Do While iReader.Read
                        iWord += Right("000" & iNb, 3) & ". " & iReader(0) & Environment.NewLine
                        iNb += 1
                    Loop
                End Using
            End Using
            If iWord.Length > 0 Then
                Dim iLength As Integer = iWord.Length - 2
                If iLength > 0 Then iWord = Left(iWord, iLength)
            End If
        Catch ex As Exception
            MsgBox("code Error 200:" & ex.Message)
            iWord = ""
        End Try
        If iWord <> "" Then
            ListeAgentInActifSansDateSortie += Environment.NewLine & iWord
        Else
            ListeAgentInActifSansDateSortie = ""
        End If
    End Function

    Public Function StatAgent(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        StatAgent = ""
        Dim iText As String = ""
        iText = MyStatValStr("SELECT count (*) from agents a where a.Actif=1", iConxStr)
        If iText <> "" Then StatAgent &= "Agents actifs : " & iText
        iText = MyStatValStr("SELECT count (*) from contratstbl c where c.FinContr>=date()", iConxStr)
        If iText <> "" Then StatAgent &= Environment.NewLine & "Contrats : " & iText
        iText = MyStatValStr("SELECT count (*) from affectation a where a.FinAffect>=date()", iConxStr)
        If iText <> "" Then StatAgent &= Environment.NewLine & "Affectations : " & iText
        iText = MyStatValStr("select count(*) from agents a where (a.Actif=0 and a.DateSortie>=date('now','start of month'))", iConxStr)
        If iText <> "" Then StatAgent &= Environment.NewLine & "Sortis ce mois : " & iText
        iText = MyStatValStr("select count(*) from agents a where (a.Actif=0 and a.DateSortie>=date('now','start of month')) or a.Actif=1", iConxStr)
        If iText <> "" Then StatAgent &= Environment.NewLine & "Agents à payer : " & iText
        Dim Tagent As Integer = MyStatValStr("SELECT count (*) from agents a where a.Actif=1", iConxStr)
        Dim TFagent As Integer = MyStatValStr("SELECT count (*) from agents a where a.Actif=1 and Sexe like 'f%'", iConxStr)
        Dim TMagent As Integer = MyStatValStr("SELECT count (*) from agents a where a.Actif=1 and Sexe like 'm%'", iConxStr)
        If Tagent > 0 Then
            If StatAgent <> "" Then StatAgent &= Environment.NewLine & "----"
            'iText = (MyStatValStr("SELECT count (*) from agents a where a.Actif=1 and Sexe like 'f%'", iConxStr) * 100 / Tagent)
            StatAgent &= Environment.NewLine & "F : " & TFagent & " (" & Math.Round(TFagent * 100 / Tagent, 0) & "%)"
            StatAgent &= Environment.NewLine & "M : " & TMagent & " (" & Math.Round(TMagent * 100 / Tagent, 0) & "%)"
        End If
        If StatAgent <> "" Then StatAgent &= Environment.NewLine & "----"
        StatAgent &= ListePosteActuel(iConxStr)
        If StatAgent <> "" Then StatAgent &= Environment.NewLine & "----"
        iText = MyStatValStr("SELECT min( cast ((julianday('now') -julianday(a.DateNaiss))/365.25 as integer) ) from agents a where a.Actif=1", iConxStr)
        If iText <> "" Then StatAgent &= Environment.NewLine & "Age Min : " & iText
        iText = MyStatValStr("SELECT cast ( avg( cast ((julianday('now') -julianday(a.DateNaiss))/365.25 as integer) ) as integer) from agents a where a.Actif=1", iConxStr)
        If iText <> "" Then StatAgent &= Environment.NewLine & "Age Moy : " & iText
        iText = MyStatValStr("SELECT Max( cast ((julianday('now') -julianday(a.DateNaiss))/365.25 as integer) ) from agents a where a.Actif=1", iConxStr)
        If iText <> "" Then StatAgent &= Environment.NewLine & "Age Max : " & iText

    End Function
    Public Function StatClient(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        StatClient = ""
        Dim iText As String = ""
        iText = MyStatValStr("SELECT count (*) from client c where c.Actif=1", iConxStr)
        If iText <> "" Then StatClient &= "Clients actifs : " & iText
        iText = MyStatValStr("SELECT count (*) from siteaffectation c where c.Actif=1", iConxStr)
        If iText <> "" Then StatClient &= Environment.NewLine & "Site actifs : " & iText
        iText = MyStatValStr("select count(*) from  (SELECT count (*) from siteaffectation c join affectation a on c.NomSite=a.SiteAffect where c.Actif=1 and a.FinAffect>=date() group by c.Localisation)", iConxStr)
        If iText <> "" Then StatClient &= Environment.NewLine & "Nb de ville : " & iText

    End Function
    Public Function ListeAgentFinAffectation(Optional DureeLimite As Integer = 15, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListeAgentFinAffectation = "=== AFFECTATION A ECHEANCE ==="
        Dim istr As String = "select c.AgentAffect||' => (' ||strftime('%d/%m/%Y', c.DateAffect)|| ' - '||strftime('%d/%m/%Y',c.FinAffect)||')' as [Liste des Agents] "
        istr += " from AFFECTATION c  "
        istr += " where cast(-julianday('now') +julianday(c.FinAffect)+1 as integer) <" & DureeLimite
        istr += " and c.FinAffect>=date() "
        'istr += " GROUP BY NomPrenom "
        Dim iWord As String = ""
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    Dim iNb As Integer = 1
                    Do While iReader.Read
                        iWord += Right("000" & iNb, 3) & ". " & iReader(0) & Environment.NewLine
                        iNb += 1
                    Loop
                End Using
            End Using
            If iWord.Length > 0 Then
                Dim iLength As Integer = iWord.Length - 2
                If iLength > 0 Then iWord = Left(iWord, iLength)
            End If
        Catch ex As Exception
            MsgBox("code Error 200:" & ex.Message)
            iWord = ""
        End Try
        If iWord <> "" Then
            ListeAgentFinAffectation += Environment.NewLine & iWord
        Else
            ListeAgentFinAffectation = ""
        End If
    End Function


    Public Function RappelActifs(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        RappelActifs = "=== MEMOS ==="
        Dim istr As String = "Select r.NoteRappel from rappels r "
        istr += " where r.Actif=1 and r.Debut<=date()  "
        Dim iWord As String = ""
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    Dim iNb As Integer = 1
                    Do While iReader.Read
                        iWord += Right("000" & iNb, 3) & ". " & iReader(0) & Environment.NewLine
                        iNb += 1
                    Loop
                End Using
            End Using
            If iWord.Length > 0 Then
                Dim iLength As Integer = iWord.Length - 2
                If iLength > 0 Then iWord = Left(iWord, iLength)
            End If
        Catch ex As Exception
            MsgBox("code Error 300:" & ex.Message)
            iWord = ""
        End Try
        If iWord <> "" Then
            RappelActifs += Environment.NewLine & iWord
        Else
            RappelActifs = ""
        End If
    End Function
    Public Function ResumeAgentActif(NomAgent As String) As String
        Dim iText As String = ""
        Dim iStr As String = "select a.NomPrenoms,a.Contacts,af.SiteAffect,n.IdNature,c.FinContr,a.Sexe,a.Fonction, cast((julianday('now')-julianday(a.DateNaiss))/365.25 as integer) as age, cast((julianday('now')-julianday(a.DateEntree))/30.4375 as integer) as anciennete, "
        iStr += " a.DateEntree,a.VilleResidence,a.Quartier "
        iStr += " from ((agents a join contratstbl c on a.NumMatr=c.NomPrenoms) join affectation af on af.AgentAffect=a.NomPrenoms) join contratsnature n on n.NatureContr=c.NaturContr   "
        iStr += " where a.Actif and c.FinContr>= date() and af.FinAffect>date() "
        iStr += " and a.NomPrenoms='" & Apost(NomAgent) & "' "
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then iConnectStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Try

            Using iconnx As New SQLiteConnection(iConnectStr)
                iconnx.Open()
                Using iCmd As New SQLiteCommand(iStr, iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    While iReader.Read
                        iText += "Contacts : " & iReader(1) & Environment.NewLine
                        iText += "Site actuel : " & iReader(2) & Environment.NewLine
                        iText += iReader(3) & " a fin " & iReader(4) & Environment.NewLine
                        iText += "Sexe : " & iReader(5) & " / Fonction : " & iReader(6) & Environment.NewLine
                        iText += "Age : " & iReader(7) & " / Ancienneté : " & iReader(8) & " mois" & Environment.NewLine
                        iText += "Date d'entrée : " & iReader(9) & Environment.NewLine
                        iText += "Ville : " & iReader(10) & " / Quartier : " & iReader(11) & Environment.NewLine
                        'iText += iReader(0) & " (" & iReader(2) & " - " & iReader(1) & ")" & Environment.NewLine
                    End While
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        If iText.Length >= 2 Then
            Dim iLengh As Integer = iText.Length - 2
            iText = Left(iText, iLengh)
        End If
        ResumeAgentActif = iText
        'MsgBox(ResumeAgentActif, MsgBoxStyle.OkOnly, UCase(NomAgent))
    End Function
    Public Function ResumeAffect(NomAgent As String) As String
        ResumeAffect = "" 'UCase(NomAgent) & Environment.NewLine
        Dim iCount As Integer = 0
        Dim iCount2 As Integer = 0
        Dim iStr As String = "select SiteAffect, FinAffect,DateAffect from affectation  where "
        iStr += " AgentAffect='" & Apost(NomAgent) & "'  order by DateAffect desc"
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then iConnectStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"

        Using iconnx As New SQLiteConnection(iConnectStr)
            iconnx.Open()
            Using iCmd As New SQLiteCommand(iStr, iconnx)
                Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                While iReader.Read
                    iCount += 1
                    If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) AndAlso Not IsDBNull(iReader(2)) Then
                        If iCount <= 30 Then
                            ResumeAffect += iCount & ". " & iReader(0) & " (" & iReader(2) & " - " & iReader(1) & ")" & Environment.NewLine
                        Else
                            iCount2 += 1
                        End If
                    End If

                End While
                If iCount2 > 0 Then ResumeAffect += "... + " & iCount2 & " autre(s) élement(s)"
            End Using

        End Using
    End Function
    Public Function ResumeCtr(NomAgent As String) As String
        ResumeCtr = "" ' UCase(NomAgent) & Environment.NewLine
        Dim iCount As Integer = 0
        Dim iCount2 As Integer = 0
        Dim iStr As String = "SELECT natc.IdNature as Type,crt.DebutContr , crt.FinContr  "
        iStr += " FROM CONTRATSTBL crt join agents ag on ag.NumMatr=crt.NomPrenoms join contratsnature natc on crt.NaturContr=natc.NatureContr " '& Apost(NomAgent) & "'  order by DateAffect desc"
        iStr += " where ag.NomPrenoms='" & Apost(NomAgent) & "'  order by crt.DebutContr desc"
        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then iConnectStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"

        Using iconnx As New SQLiteConnection(iConnectStr)
            iconnx.Open()
            Using iCmd As New SQLiteCommand(iStr, iconnx)
                Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                While iReader.Read
                    iCount += 1
                    If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) AndAlso Not IsDBNull(iReader(2)) Then
                        If iCount <= 30 Then
                            ResumeCtr += iCount & ". " & iReader(0) & " (" & iReader(1) & " - " & iReader(2) & ")" & Environment.NewLine
                        Else
                            iCount2 += 1
                        End If
                    End If

                End While
                If iCount2 > 0 Then ResumeCtr += "... + " & iCount2 & " autre(s) élement(s)"
            End Using

        End Using
    End Function
    Public Function ResumeAbs(NomAgent As String) As String
        ResumeAbs = "" 'UCase(NomAgent) & Environment.NewLine
        Dim iCount As Integer = 0
        Dim iCount2 As Integer = 0
        Dim iStr As String = "select DateDebut, DateFin, Motif from Absences where "
        iStr += " NomPrenom='" & Apost(NomAgent) & "'  order by DateDebut desc"

        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then iConnectStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Using iconnx As New SQLiteConnection(iConnectStr)
            iconnx.Open()
            Using iCmd As New SQLiteCommand(iStr, iconnx)
                Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                While iReader.Read
                    iCount += 1
                    If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) AndAlso Not IsDBNull(iReader(2)) Then
                        If iCount <= 30 Then
                            ResumeAbs += iCount & ". " & iReader(2) & " (" & iReader(0) & " - " & iReader(1) & ")" & Environment.NewLine
                        Else
                            iCount2 += 1
                        End If
                    End If

                End While
                If iCount2 > 0 Then ResumeAbs += "... + " & iCount2 & " autre(s) élement(s)"
            End Using

        End Using
    End Function
    Public Function ResumeHS(NomAgent As String) As String
        ResumeHS = "" 'UCase(NomAgent) & Environment.NewLine
        Dim iCount As Integer = 0
        Dim iCount2 As Integer = 0
        Dim iStr As String = "select DateHS, NbFaction, Obs from heuresup where "
        iStr += " Agent='" & Apost(NomAgent) & "'  order by DateHS desc"

        If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then iConnectStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Using iconnx As New SQLiteConnection(iConnectStr)
            iconnx.Open()
            Using iCmd As New SQLiteCommand(iStr, iconnx)
                Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                While iReader.Read
                    iCount += 1
                    If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) AndAlso Not IsDBNull(iReader(2)) Then
                        If iCount <= 30 Then
                            ResumeHS += iCount & ". " & iReader(0) & " | " & iReader(2) & " | " & iReader(1) & "" & Environment.NewLine
                        Else
                            iCount2 += 1
                        End If
                    End If

                End While
                If iCount2 > 0 Then ResumeHS += "... + " & iCount2 & " autre(s) élement(s)"
            End Using

        End Using
    End Function

    Public Function ResumePlanning(NomAgent As String, debut As Date, fin As Date) As String
        ResumePlanning = "" 'UCase(NomAgent) & Environment.NewLine
        Try
            Dim iCount As Integer = 0
            Dim iCount2 As Integer = 0
            Dim iStr As String = "select case strftime('%u', p.DatePlan) when '1' then 'Lun.'  when '2' then 'Mar.'  when '3' then 'Mer.'  when '4' then 'Jeu.'  when '5' then 'Ven.'  when '6' then 'Sam.'  when '7' then 'Dim.'   end  "
            iStr += "  ||' '||strftime('%d/%m', p.DatePlan) as Jour , "
            iStr += " po.Designation from planposte p join postes po on p.Poste=po.Code "
            iStr += " where p.DatePlan>='" & debut.ToString("yyyy-MM-dd") & "' and p.DatePlan<='" & fin.ToString("yyyy-MM-dd") & "'"
            iStr += "  and (p.Agent like '%" & Apost(NomAgent) & "%' or po.Designation like '%" & Apost(NomAgent) & "%') "
            iStr += " order by p.DatePlan, p.Agent,po.Designation"
            'iStr += " "
            'iStr += " "
            'iStr += " Agent='" & Apost(NomAgent) & "'  order by DateHS desc"

            If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then iConnectStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
            Using iconnx As New SQLiteConnection(iConnectStr)
                iconnx.Open()
                Using iCmd As New SQLiteCommand(iStr, iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    While iReader.Read
                        iCount += 1
                        If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) Then
                            If iCount <= 30 Then
                                ResumePlanning += iCount & ". " & iReader(0) & " >> " & iReader(1) & "" & Environment.NewLine
                            Else
                                iCount2 += 1
                            End If
                        End If

                    End While
                    If iCount2 > 0 Then ResumePlanning += "... + " & iCount2 & " autre(s) élement(s)"
                End Using

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Function
    Public Function ResumePlanningPoste(NomPoste As String, debut As Date, fin As Date) As String
        ResumePlanningPoste = "" 'UCase(NomAgent) & Environment.NewLine
        Try
            Dim iCount As Integer = 0
            Dim iCount2 As Integer = 0
            Dim iStr As String = "select case strftime('%u', p.DatePlan) when '1' then 'Lun.'  when '2' then 'Mar.'  when '3' then 'Mer.'  when '4' then 'Jeu.'  when '5' then 'Ven.'  when '6' then 'Sam.'  when '7' then 'Dim.'   end  "
            iStr += "  ||' '||strftime('%d/%m', p.DatePlan) as Jour , "
            iStr += " p.Agent from planposte p join postes po on p.Poste=po.Code "
            iStr += " where p.DatePlan>='" & debut.ToString("yyyy-MM-dd") & "' and p.DatePlan<='" & fin.ToString("yyyy-MM-dd") & "'"
            iStr += " and (po.Designation like '%" & Apost(NomPoste) & "%') " 'or po.Designation like '%" & Apost(NomAgent) & "%') "
            iStr += " order by p.DatePlan, p.Agent,po.Designation"
            'iStr += " "
            'iStr += " "
            'iStr += " Agent='" & Apost(NomAgent) & "'  order by DateHS desc"

            If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then iConnectStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
            Using iconnx As New SQLiteConnection(iConnectStr)
                iconnx.Open()
                Using iCmd As New SQLiteCommand(iStr, iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    While iReader.Read
                        iCount += 1
                        If Not IsDBNull(iReader(0)) AndAlso Not IsDBNull(iReader(1)) Then
                            If iCount <= 30 Then
                                ResumePlanningPoste += iCount & ". " & iReader(0) & " >> " & iReader(1) & "" & Environment.NewLine
                            Else
                                iCount2 += 1
                            End If
                        End If

                    End While
                    If iCount2 > 0 Then ResumePlanningPoste += "... + " & iCount2 & " autre(s) élement(s)"
                End Using

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Function

    Public Function ResumeClient(iName As String) As String
        Dim iResum As String = "" 'UCase(iName) & Environment.NewLine
        iResum += "=== Montant facturation : " & Format(MayStatVal("select c.MontantFacturationUnitaire from client c where c.NomClient like '" & Apost(iName) & "'"), "# ###") & Environment.NewLine
        iResum += "=== Date Début : " & MyStatValStr("select c.DateDebutCollabor from client c where c.NomClient like '" & Apost(iName) & "'") & Environment.NewLine
        iResum += "=== Date fin : " & MyStatValStr("select c.DateFinCollabor from client c where c.NomClient like '" & Apost(iName) & "'") & Environment.NewLine
        iResum += "=== Total d'agents : " & MyStatValStr("select count(*) as NbAgent from siteaffectation s  join affectation a on a.SiteAffect=s.NomSite where  s.Actif=1 and a.FinAffect>=date() and  s.Client not like '%g2s%' and s.Client='" & Apost(iName) & "'") & Environment.NewLine
        iResum += "=== Site ====" & Environment.NewLine & ListeSiteClient(iName)
        ResumeClient = iResum
    End Function
    Public Function ListeAgentClient(iName As String, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListeAgentClient = ""
        Dim istr As String = "select a.AgentAffect||' ('|| a.SiteAffect ||')' as NbAgent from siteaffectation s  join affectation a on a.SiteAffect=s.NomSite "
        'istr += " from ((contratstbl c join agents a on c.NomPrenoms=a.NumMatr) join contratsnature n on n.NatureContr=c.NaturContr) join affectation af on af.AgentAffect=a.NomPrenoms  " ' FinContr=#" & MaxDateFinContrat.ToString("MM/dd/yyyy") & "#  "
        istr += " where  s.Actif=1 and a.FinAffect>=date() and s.Client='" & Apost(iName) & "'  order by a.AgentAffect"
        'istr += " AND NomPrenom Like '" & Apost(NomAgent) & "' "
        'istr += " GROUP BY NomPrenom "
        Dim iWord As String = ""
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    Dim iNb As Integer = 1
                    Do While iReader.Read
                        iWord += Right("000" & iNb, 3) & ". " & iReader(0) & Environment.NewLine
                        iNb += 1
                    Loop
                End Using
            End Using
            If iWord.Length > 0 Then
                Dim iLength As Integer = iWord.Length - 2
                If iLength > 0 Then iWord = Left(iWord, iLength)
            End If
        Catch ex As Exception
            MsgBox("code Error 200:" & ex.Message)
            iWord = ""
        End Try
        If iWord <> "" Then
            ListeAgentClient += Environment.NewLine & iWord
        Else
            ListeAgentClient = ""
        End If
    End Function
    Public Function ListePermissionCongeEnAttente(Optional TypeConge As String = "%", Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As String
        ListePermissionCongeEnAttente = ""
        Dim istr As String = "SELECT A.NomPrenom||' ('||strftime('%d/%m/%Y',A.DebutDmd)||' - '||strftime('%d/%m/%Y',A.FinDmd)||') > Durée : '|| CAST (JULIANDAY(A.FinDmd)-JULIANDAY (A.DebutDmd)+1 AS INTEGER) AS DELAI "
        istr += " from ABSENCES A "
        istr += " WHERE A.StdBy=1 AND  CAST (JULIANDAY(A.DebutDmd)-JULIANDAY ('now') AS INTEGER)<15 "
        istr += " AND A.Motif LIKE '%" & Apost(TypeConge) & "%' "
        If LCase(TypeConge).Contains("permis") Then
            istr += " ORDER BY A.DebutDmd,A.NomPrenom"
        ElseIf LCase(TypeConge).Contains("cong") Then
            istr += " ORDER BY A.DateDmd,A.NomPrenom  "
        Else
            istr += " ORDER BY A.NomPrenom  "
        End If
        Dim iWord As String = ""
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    Dim iNb As Integer = 1
                    Do While iReader.Read
                        iWord += Right("000" & iNb, 3) & ". " & iReader(0) & Environment.NewLine
                        iNb += 1
                    Loop
                End Using
            End Using
            If iWord.Length > 0 Then
                Dim iLength As Integer = iWord.Length - 2
                If iLength > 0 Then iWord = Left(iWord, iLength)
            End If
        Catch ex As Exception
            MsgBox("code Error 200:" & ex.Message)
            iWord = ""
        End Try
        If iWord <> "" Then
            ListePermissionCongeEnAttente += Environment.NewLine & iWord
        Else
            ListePermissionCongeEnAttente = ""
        End If
    End Function
    Public Function Joper(ChaineDeMots As String, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        Joper = False
        'Dim iConxStr As String = $"Data Source={PaphBackUp()};Version=3;"
        Dim iStr As String = "insert into jope (operation,operateur) values ('" & Apost(ChaineDeMots) & "','" & Apost(UserName) & "') "
        Joper = SaveNewData(iStr, iConxStr)
    End Function
    Public Function SessLogin(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        If Not LocalModeBb AndAlso Not maj() Then Return False
        'Dim iConxStr As String = $"Data Source={PaphBackUp()};Version=3;"
        Dim iUserName As String = UCase(UserName & "@" & My.Computer.Name)
        IdSession = MayStatVal("select  max (SesId) from sesslog", iConxStr) + 1
        Dim iStr As String = "insert into SessLog (UserName,SesId) values ('" & Apost(iUserName) & "'," & IdSession & ") "
        'ShowTempMsg(iConxStr)
        SessLogin = SaveNewData(iStr, iConxStr)
    End Function
    Public Function SessLogout(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        'Dim iConxStr As String = $"Data Source={PaphBackUp()};Version=3;"
        'Dim iUserName As String = UCase(UserName & "@" & My.Computer.Name)
        'IdSession = MayStatVal("select  max (SesId) from sesslog", iConxStr) + 1
        Dim iStr As String = "update SessLog  set DateFinSession=datetime() where SesId=" & IdSession & " "
        'ShowTempMsg(iStr)
        SessLogout = SaveNewData(iStr, iConxStr)
    End Function
    Public Function LoadUserProfil(LevelUser As Integer, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        LoadUserProfil = False
        If LevelUser = 1976 Then Exit Function
        Dim iStr As String = "select l.ProfilName,l.CanUserManage,l.CanGlobalDataManage,CanSocieteDataManage, "
        iStr &= " l.CanAddDattaOrdinary,l.CanModifyDattaOrdinary,l.CanDeleteDattaOrdinary,l.CanReadDattaOrdinary,"
        iStr &= " l.CanAddDattaSpecial,l.CanModifyDattaSpecial,l.CanDeleteDattaSpecial,l.CanReadDattaSpecial,"
        iStr &= " l.Description"
        iStr &= "  from ladawet l where l.Id=" & LevelUser
        iStr &= " "
        ' If File.Exists(Path.Combine(UserPath, "ManA.sqlite")) Then iConxStr = $"Data Source={Path.Combine(UserPath, "ManA.sqlite")};Version=3;"
        Try
            Using iConnex As New SQLiteConnection(iConxStr)
                iConnex.Open()
                Using iCmd As New SQLiteCommand(iStr, iConnex)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    While iReader.Read
                        ProfilName = iReader(0)
                        CanUserManage = iReader(1)
                        CanGlobalDataManage = iReader(2)
                        CanSocieteDataManage = iReader(3)
                        CanAddDataOrdinary = iReader(4)
                        CanModifyDataOrdinary = iReader(5)
                        CanDeleteDataOrdinary = iReader(6)
                        CanReadDataOrdinary = iReader(7)
                        CanAddDataSpecial = iReader(8)
                        CanModifyDataSpecial = iReader(9)
                        CanDeleteDataSpecial = iReader(10)
                        CanReadDataSpecial = iReader(11)
                        DescriptionProfil = iReader(12)
                        LoadUserProfil = True
                    End While
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            LoadUserProfil = False
        End Try
    End Function

End Module
