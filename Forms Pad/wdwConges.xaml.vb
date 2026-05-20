Imports System.Data.SQLite
Public Class wdwConges
    Dim iLoaded As Boolean = False
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        dtpDebut.SelectedDate = Today
        dtpFin.SelectedDate = FinDuMois(Today.AddMonths(-1))
        FilmyCbx("select NomPrenoms from agents where actif=1 order by NomPrenoms", CbxAgent)
        iLoaded = True
    End Sub

    Private Sub CbxAgent_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxAgent.SelectionChanged
        InitialVal()
        SaisiCbx(sender, TblIndicationCbxAgent)
        If CbxAgent.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            CbxNumMatr.Visibility = Visibility.Visible
            TblIndicationNumMatr.Visibility = Visibility.Visible
        End If
        If CbxAgent.SelectedIndex = -1 Then
            CbxNumMatr.Visibility = Visibility.Collapsed
            TblIndicationNumMatr.Visibility = Visibility.Collapsed
        Else
            If FilmyCbx("select a.NumMatr,a.NomPrenoms from agents a  where a.NomPrenoms like '" & Apost(CbxAgent.SelectedItem) & "'  order by a.NumMatr", CbxNumMatr) Then
                CbxNumMatr.SelectedIndex = 0
                If SiteActuElmt() Then
                End If
                TblIndicationNumMatr.Visibility = Visibility.Collapsed
                CbxNumMatr.Visibility = Visibility.Visible
            Else
                CbxNumMatr.SelectedIndex = -1
                TblIndicationNumMatr.Visibility = Visibility.Visible
                CbxNumMatr.Visibility = Visibility.Collapsed
                TblIndicationNumMatr.Visibility = Visibility.Collapsed
            End If
        End If

        TbxJrPris.Text = JourPris()

        ActivSaveBtn()
    End Sub
    Sub ActivSaveBtn()
        If isLoaded Then
            If dtpDebut.SelectedDate.HasValue AndAlso dtpFin.SelectedDate.HasValue AndAlso isLoaded Then
                If (IsNumeric(TbxJrPris.Text)) And (IsNumeric(TbxFranchise.Text)) And (IsNumeric(TbxMensuelCong.Text)) And (Not LCase(TbxTypCtr.Text).Contains("stage")) And (dtpDebut.SelectedDate.Value < dtpFin.SelectedDate.Value) Then
                    btnValider.Visibility = Visibility.Visible
                    btnValider.IsEnabled = True
                Else
                    btnValider.IsEnabled = False
                    btnValider.Visibility = Visibility.Collapsed
                End If
            Else
                btnValider.Visibility = Visibility.Collapsed
                btnValider.IsEnabled = False
            End If
        End If
    End Sub
    Sub InitialVal()
        If isLoaded Then
            TbxJrPris.Text = "0"
            TbxJrsDuree.Text = "0"
            TbxMoisDuree.Text = "0"
            TbxDureeNet.Text = "0"
        End If
        'CbxAgent.SelectedIndex = -1
    End Sub
    Function SiteActuElmt(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Boolean
        SiteActuElmt = False
        If CbxAgent.SelectedIndex >= 0 Then
            Dim istr As String = "select a.SiteAffect from  affectation a where a.AgentAffect like '" & Apost(CbxAgent.SelectedItem) & "' and a.FinAffect>='" & Today.AddDays(-21).ToString("yyyy-MM-dd") & "' order by a.DateAffect desc,a.IdAffec desc limit 1"
            TbxSite.Text = MyStatValStr(istr)
            Try
                istr = "select format(c.DebutContr) as debut,format(c.FinContr) as fin from contratstbl c where c.NomPrenoms like '" & Apost(CbxNumMatr.SelectedItem) & "' "
                Try
                    Using iconnx As New SQLiteConnection(iConxStr)
                        iconnx.Open()
                        Using icmd As New SQLiteCommand(istr, iconnx)
                            Dim ireader As SQLiteDataReader = icmd.ExecuteReader
                            While ireader.Read
                                If Not IsDBNull(ireader(0)) AndAlso IsDate(ireader(0)) Then
                                    dtpDebut.SelectedDate = CDate(ireader(0)).ToString("dd/MM/yyyy")
                                    dtpFin.DisplayDateStart = CDate(ireader(0))
                                End If
                                If Not IsDBNull(ireader(1)) AndAlso IsDate(ireader(1)) Then
                                    Dim idate As Date = CDate(ireader(1))
                                    dtpFin.SelectedDate = idate.ToString("dd/MM/yyyy")
                                    'dtpDateSortie.DisplayDateEnd = idate
                                    If idate < Today Then
                                        dtpFin.SelectedDate = idate
                                    Else
                                        dtpFin.SelectedDate = FinDuMois(Today.AddMonths(-1))
                                        If dtpFin.SelectedDate.HasValue AndAlso dtpDebut.SelectedDate.HasValue AndAlso dtpFin.SelectedDate.Value < dtpDebut.SelectedDate.Value Then
                                            dtpFin.SelectedDate = dtpDebut.SelectedDate.Value
                                        End If
                                    End If

                                End If
                                SiteActuElmt = True
                            End While
                        End Using
                    End Using

                Catch ex As Exception
                    MsgBox(ex.Message)
                    SiteActuElmt = False
                End Try

            Catch ex As Exception
                MsgBox(ex.Message)
                SiteActuElmt = False

            End Try
        End If
    End Function

    Function JourPris(Optional iConxStr As String = "Data Source=C:\LkyRess\ManA.sqlite;Version=3;") As Integer
        JourPris = 0
        Dim istr As String = "SELECT Sum(julianday(DateFin)-julianday(DateDebut)+1) AS Duree from absences "
        istr += " WHERE   (DateDebut>='" & dtpDebut.SelectedDate.Value.ToString("yyyy-MM-dd") & "' And DateDebut<='" & dtpFin.SelectedDate.Value.ToString("yyyy-MM-dd") & "' ) " ' FinContr=#" & MaxDateFinContrat.ToString("MM/dd/yyyy") & "#  "
        istr += " AND (Motif Like '%annu%' Or Motif Like '%accor%' )  "
        istr += " AND NomPrenom Like '" & Apost(CbxAgent.SelectedItem) & "' "
        istr += " GROUP BY NomPrenom "
        Try
            Using Iconnx As New SQLiteConnection(iConxStr)
                Iconnx.Open()
                Using iCmd As New SQLiteCommand(istr, Iconnx)
                    Dim iReader As SQLiteDataReader = iCmd.ExecuteReader
                    TbxObs.Text = istr
                    Do While iReader.Read
                        JourPris = iReader(0)
                    Loop
                End Using
            End Using
        Catch ex As Exception
            MsgBox("code Error 100:" & ex.Message)
            JourPris = 0
        End Try

    End Function
    Sub AffichResut()
        Try
            Dim moisBrut As Double = DureeMois() - (TbxJrPris.Text) / 30
            Dim jourBrut As Double = (moisBrut * (TbxMensuelCong.Text)) - TbxFranchise.Text
            Dim jourNet As Double = jourBrut - (TbxJrPris.Text)
            Me.TbxMoisDuree.Text = Math.Round(moisBrut, 2)
            Me.TbxJrsDuree.Text = Math.Round(jourBrut, 2)
            Me.TbxDureeNet.Text = Math.Round(jourNet, 2)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Public Function DureeMois() As Double
        DureeMois = 0
        AttrVal()
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

    Sub AttrVal()
        J1 = dtpDebut.SelectedDate.Value.Day
        J2 = dtpFin.SelectedDate.Value.Day
        M1 = dtpDebut.SelectedDate.Value.Month
        M2 = dtpFin.SelectedDate.Value.Month
        Y1 = dtpDebut.SelectedDate.Value.Year
        Y2 = dtpFin.SelectedDate.Value.Year
        DateD1 = DebutDuMois(dtpDebut.SelectedDate.Value).AddMonths(1)
        DateD2 = DebutDuMois(dtpFin.SelectedDate.Value).AddMonths(1)
        DateF1 = FinDuMois(dtpDebut.SelectedDate.Value)
        DateF2 = FinDuMois(dtpFin.SelectedDate.Value)
    End Sub

    Private Sub btnValider_Click(sender As Object, e As RoutedEventArgs) Handles btnValider.Click
        AffichResut()
    End Sub

    Private Sub CbxNumMatr_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbxNumMatr.SelectionChanged
        If CbxNumMatr.SelectedIndex >= 0 Then
            Dim iStr As String = "SELECT n.IdNature,c.DebutContr from contratstbl c join ContratsNature n on c.NaturContr=n.NatureContr"
            iStr += " where c.NomPrenoms like '" & CbxNumMatr.SelectedItem & "' "
            iStr += " order by c.DebutContr desc limit 1 "
            TbxTypCtr.Text = MyStatValStr(iStr)
        Else
            TbxTypCtr.Text = ""
        End If
        If LCase(TbxTypCtr.Text).Contains("stage") Then
            TbxTypCtr.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF8E8E"))
            btnValider.Visibility = Visibility.Collapsed
        Else
            TbxTypCtr.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFFFFF"))
            btnValider.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub dtpDebut_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpDebut.SelectedDateChanged, dtpFin.SelectedDateChanged
        InitialVal()
        ActivSaveBtn()

    End Sub

    Private Sub btnPtGal_Click(sender As Object, e As RoutedEventArgs) Handles btnPtGal.Click
        Dim iVuGlobal As New wdwCongeGlobal
        iVuGlobal.WindowState = WindowState.Normal
        iVuGlobal.Topmost = True
        iVuGlobal.Show()
    End Sub

    Private Sub TbxMensuelCong_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TbxMensuelCong.TextChanged, TbxMensuelCong.TextChanged, TbxFranchise.TextChanged, TbxJrPris.TextChanged
        'InitialVal()
        ActivSaveBtn()
        If isLoaded Then
            If Not IsNumeric(sender.Text) Then
                sender.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF8E8E"))
                'btnValider.Visibility = Visibility.Collapsed
            Else
                sender.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFFFFF"))
                'btnValider.Visibility = Visibility.Visible
            End If
        End If
    End Sub
End Class
