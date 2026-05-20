Imports System.Data.SQLite
Imports System.Data
Public Class wdwCongeGlobal
    Dim idxAction As Integer = 0
    Dim NbAction As Integer = 0
    Dim BaseCollect As New Collection
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        dtpFinSearch.SelectedDate = FinDuMois(Today.AddMonths(-1))
    End Sub
    Function LoadElement(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        LoadElement = False
        Dim istr As String = "select a.NomPrenoms FROM (ContratsTbl c join agents a on a.NumMatr=c.NomPrenoms) join affectation af on a.NomPrenoms=af.AgentAffect "
        istr += "WHERE    (c.NaturContr Not Like '%stage%') AND (c.FinContr>='" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "') AND (c.DebutContr<'" & dtpFinSearch.SelectedDate.Value.ToString("yyyy-MM-dd") & "') "
        istr += " and af.FinAffect>=date() "
        istr += " and (a.NomPrenoms like '%" & TbxKeyword.Text & "%' or af.SiteAffect like '%" & TbxKeyword.Text & "%') "
        istr += "order by a.NomPrenoms  "
        Try
            Using iConnx As New SQLiteConnection(iConxStr)
                iConnx.Open()
                Using iCmd As New SQLiteCommand(istr, iConnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    BaseCollect.Clear()
                    While iReader.Read
                        If iReader(0) <> "" Or Not IsDBNull(iReader(0)) Then BaseCollect.Add(iReader(0))
                        LoadElement = True
                    End While
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            LoadElement = False
        End Try
    End Function
    Function loadDg() As DataTable
        'loadGlobalDGV = False
        Dim iDataTable As New DataTable
        If LoadElement() Then
            iDataTable.Rows.Clear()
            iDataTable.Columns.Clear()

            If BaseCollect.Count > 0 Then
                iDataTable.Columns.Add("Nun")
                iDataTable.Columns.Add("Agent")
                iDataTable.Columns.Add("Site")
                iDataTable.Columns.Add("Debut contrat")
                iDataTable.Columns.Add("Nb de jours dûs")
                iDataTable.Columns.Add("Nb de jours pris")
                iDataTable.Columns.Add("Nb de jours restant")
            End If
            idxAction = 1
            NbAction = BaseCollect.Count
            'Genaral_Pgb.Visible = True
            Dim NomAgent As String
            Dim Mysite As String
            Dim DebutContrat As Date
            Dim DureeDu As Double = 0
            Dim DureePris As Double = 0
            Dim DureeNet As Double = 0

            For Each elmt In BaseCollect
                NomAgent = BaseCollect(idxAction)
                Dim iSite As String = MyStatValStr("select a.SiteAffect FROM affectation a where a.AgentAffect like '" & Apost(NomAgent) & "' order by a.DateAffect desc limit 1 ")
                Mysite = iSite
                Dim iDebut As String = MyStatValStr("select c.DebutContr FROM contratstbl c join agents a on a.NumMatr=c.NomPrenoms where a.NomPrenoms like '" & Apost(NomAgent) & "' order by c.DebutContr desc limit 1 ") 'DateDebutContratAgent(NomAgent, dtpFinSearch.SelectedDate.Value).ToString("yyyy-MM-dd")
                If IsDate(iDebut) Then
                    DebutContrat = CDate(iDebut)
                Else
                    DebutContrat = CDate("01/01/" & Today.Year)
                End If
                'MsgBox(iSite & " > " & DebutContrat)
                DureePris = JourPrisAgent(NomAgent, DebutContrat, dtpFinSearch.SelectedDate.Value)
                DureeDu = 2.5 * (DureeMois(DebutContrat, dtpFinSearch.SelectedDate.Value) - (DureePris) / 30)
                DureeNet = DureeDu - DureePris
                iDataTable.Rows.Add(idxAction, NomAgent, Mysite, Format(DebutContrat, "yyyy-MM-dd"), Format(DureeDu, "#0.##"), Format(DureePris, "#0.##"), Format(DureeNet, "#0.##"))
                'Genaral_Pgb.Value = CInt(100 * idxAction / NbAction)
                idxAction += 1
            Next
            'Genaral_Pgb.Visible = False
        End If
        loadDg = iDataTable
    End Function

    Function JourPrisAgent(NomAgent As String, DebutContrat As Date, FinPeriode As Date, Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Integer
        JourPrisAgent = 0

        Dim istr As String = "SELECT Sum(julianday(DateFin)-julianday(DateDebut)+1) AS Duree from absences "
        istr += " WHERE   (DateDebut>='" & DebutContrat.ToString("yyyy-MM-dd") & "' And DateDebut<='" & FinPeriode.ToString("yyyy-MM-dd") & "' ) " ' FinContr=#" & MaxDateFinContrat.ToString("MM/dd/yyyy") & "#  "
        istr += " AND (Motif Like '%annu%' Or Motif Like '%accor%' )  "
        istr += " AND NomPrenom Like '" & Apost(NomAgent) & "' "
        istr += " GROUP BY NomPrenom "
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    'TbxObs.Text = istr
                    Do While iReader.Read
                        JourPrisAgent = iReader(0)
                    Loop
                End Using
            End Using
        Catch ex As Exception
            MsgBox("code Error 100:" & ex.Message)
            JourPrisAgent = 0
        End Try

    End Function
    Public Function DureeMois(DebutPeriode As Date, FinPeriode As Date) As Double
        DureeMois = 0
        AttrVal(DebutPeriode.ToString("yyyy-MM-dd"), FinPeriode.ToString("yyyy-MM-dd"))
        If DateF1 = DateF2 Then
            If J1 <> J2 Then DureeMois = (J2 - J1 + 1) / DateF2.Day
            'MsgBox("cas1")
        Else
            If DateDiff(DateInterval.Month, DateD1, DateD2) > 1 Then
                DureeMois = (DateF1.Day - J1 + 1) / DateF1.Day
                DureeMois += (J2) / DateF2.Day
                DureeMois += DateDiff(DateInterval.Month, DateD1, DateD2) - 1
                'MsgBox("cas2")
            Else
                DureeMois = (DateF1.Day - J1 + 1) / DateF1.Day
                DureeMois += (J2) / DateF2.Day
                'MsgBox("cas3")
            End If
        End If

    End Function
    Public Sub AttrVal(DebutPeriode As Date, FinPeriode As Date)
        J1 = DebutPeriode.Day
        J2 = FinPeriode.Day
        M1 = DebutPeriode.Month
        M2 = FinPeriode.Month
        Y1 = DebutPeriode.Year
        Y2 = FinPeriode.Year
        DateD1 = DebutDuMois(DebutPeriode).AddMonths(1)
        DateD2 = DebutDuMois(FinPeriode).AddMonths(1)
        DateF1 = FinDuMois(DebutPeriode)
        DateF2 = FinDuMois(FinPeriode)
    End Sub

    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        lpsAction.Visibility = Visibility.Visible
        dgPointGlobal.ItemsSource = loadDg().DefaultView
        lpsAction.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click
        lpsAction.Visibility = Visibility.Visible
        Dim iTitre As String = "Congés "
        If dtpFinSearch.SelectedDate.HasValue Then
            iTitre = "Congés en fin " & dtpFinSearch.SelectedDate.Value.ToString("dd/MM/yy")
        End If
        If TbxKeyword.Text <> "" Then iTitre += " avec pour mot clé de recherche comme '" & TbxKeyword.Text & "'"
        ExportDGtoExcel(dgPointGlobal, "Point_Congés", iTitre) '"Point_Entrees"
        lpsAction.Visibility = Visibility.Collapsed

    End Sub
End Class
