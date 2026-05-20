Module TimeDateManip
    Public Date1, Date2, DateD1, DateD2, DateF1, DateF2 As Date
    Public J1, J2, M1, M2, Y1, Y2 As Integer

    Public Function FinDuMois(ByVal MyDate As Date) As Date
        Dim dateval As Date = "01/01/1978"
        If IsDate(MyDate) Then
            Try
                Dim Mois As Integer = MyDate.ToString("MM")
                Dim AnneeDate As Integer = MyDate.ToString("yyyy")
                Dim Fin As Integer = Date.DaysInMonth(AnneeDate, Mois)
                dateval = Fin & "/" & Mois & "/" & AnneeDate

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
        FinDuMois = dateval
    End Function

    Public Function DebutDuMois(ByVal MyDate As Date) As Date
        Dim dateval As Date = "01/01/1978"
        If IsDate(MyDate) Then
            Try
                Dim Mois As Integer = MyDate.ToString("MM")
                Dim AnneeDate As Integer = MyDate.ToString("yyyy")
                Dim Fin As Integer = Date.DaysInMonth(AnneeDate, Mois)
                dateval = "01/" & Mois & "/" & AnneeDate

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
        DebutDuMois = dateval
    End Function

    Public Function DebutDeSemaine(ByVal MyDate As Date) As Date
        Dim dateval As Date = "01/01/1978"
        If IsDate(MyDate) Then
            Try
                'Dim Mois As Integer = MyDate.ToString("MM")
                'Dim AnneeDate As Integer = MyDate.ToString("yyyy")
                Dim Jr As Integer = MyDate.DayOfWeek
                'Dim ddate As Integer = MyDate.Day

                Select Case Jr
                    Case 0
                        dateval = MyDate.AddDays(-6)

                    Case 1
                        dateval = MyDate
                    Case 2
                        dateval = MyDate.AddDays(-1)
                    Case 3
                        dateval = MyDate.AddDays(-2)
                    Case 4
                        dateval = MyDate.AddDays(-3)
                    Case 5
                        dateval = MyDate.AddDays(-4)
                    Case 6
                        dateval = MyDate.AddDays(-5)
                End Select
                'dateval = "01/" & Mois & "/" & AnneeDate

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
        DebutDeSemaine = dateval
    End Function

    Public Function finDeSemaine(ByVal MyDate As Date) As Date
        Dim dateval As Date = "01/01/1978"
        If IsDate(MyDate) Then
            Try
                Dim Jr As Integer = MyDate.DayOfWeek
                Select Case Jr
                    Case 0
                        dateval = MyDate
                    Case 1
                        dateval = MyDate.AddDays(+6)
                    Case 2
                        dateval = MyDate.AddDays(+5)
                    Case 3
                        dateval = MyDate.AddDays(+4)
                    Case 4
                        dateval = MyDate.AddDays(3)
                    Case 5
                        dateval = MyDate.AddDays(2)
                    Case 6
                        dateval = MyDate.AddDays(+1)
                End Select
                'dateval = "01/" & Mois & "/" & AnneeDate

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
        finDeSemaine = dateval
    End Function
    Public Function FinEmbaucheCalcule(ByVal DateNaissance As Date) As Date
        Dim MyDate As Date = CDate("31/12/" & (DateCreationSociete.Year + 99))
        Dim AnneeActuel As Integer = CInt(Today.Year)
        Dim AgeActuel As Integer = CInt(Today.Year) - CInt(DateNaissance.Year)
        Dim AnneeRetraite As Integer = CInt(Today.Year) + (AgeRetraite - AgeActuel)
        'MsgBox(AnneeRetraite)
        If MyDate.Year >= AnneeRetraite Then
            MyDate = CDate("31/12/" & AnneeRetraite)
        Else
            MyDate = CDate("31/12/" & (DateCreationSociete.Year + 99))
        End If
        FinEmbaucheCalcule = MyDate
    End Function
    Public Function ConfirmToday(DateSaisie As Date, oDate As Object, Optional ByVal DateMsg As String = "LA DATE") As Boolean
        ConfirmToday = False
        If DateSaisie.ToString("dd/MM/yyyy") = Today.ToString("dd/MM/yyyy") Then
            If MsgBox(DateMsg & " " & Today.ToString("dd/MM/yyyy") & " EST-ELLE CORRECTE ?", MsgBoxStyle.YesNo, "Confirmation de date...") = MsgBoxResult.Yes Then
                ConfirmToday = True
            Else
                oDate.Focus()
                ConfirmToday = False
            End If
        Else
            ConfirmToday = True
        End If
    End Function
End Module
