Module TextManip

    Public Function Apost(ByVal Text As String) As String
        Dim NewTxt As String
        NewTxt = ""
        For Each Charac As Char In Text
            If Charac = "'" Then
                NewTxt += "''"
            Else
                NewTxt += Charac
            End If
        Next
        Apost = NewTxt
    End Function
    Public Function ApostX(ByVal Text As String) As String
        Dim NewTxt As String
        NewTxt = ""
        For Each Charac As Char In Text
            If Charac = "''" Then
                NewTxt += "'"
            Else
                NewTxt += Charac
            End If
        Next
        ApostX = NewTxt

    End Function
    Public Function TextPointille(ByVal TextGauche As String, ByVal TextDroite As String, Optional ByVal LenghtTotal As Integer = 256) As String
        TextPointille = ""
        Dim LenghtText As Integer = TextGauche.Length + TextDroite.Length
        Dim CasLenght As Int16 = 0
        If LenghtTotal <= 3 Or LenghtTotal >= 225 Then CasLenght = 0
        If LenghtTotal <= TextDroite.Length Or LenghtTotal <= TextGauche.Length Then CasLenght = 0
        If LenghtTotal < (TextDroite.Length + 6) And LenghtTotal < LenghtText Then CasLenght = 0
        If LenghtTotal >= (TextDroite.Length + 6) And LenghtTotal < LenghtText Then CasLenght = 1
        If LenghtTotal >= LenghtText Then CasLenght = 2

        Dim MyWord As String = ""
        Select Case CasLenght
            Case 0
                TextPointille = "..."
            Case 1
                MyWord = Left(TextGauche, LenghtTotal - TextDroite.Length - 3)
                For Ix As Integer = 0 To 2 '(LenghtTotal - LenghtText - 1)
                    MyWord += "."
                Next
                TextPointille = MyWord + TextDroite

            Case 2
                MyWord = TextGauche
                For Ix As Integer = 0 To (LenghtTotal - LenghtText - 1)
                    MyWord += "."
                Next
                TextPointille = MyWord + TextDroite
        End Select

    End Function

    Public Sub SaisieMajusculeTbx(iTbx As TextBox)
        Dim position As Integer = iTbx.SelectionStart
        iTbx.Text = iTbx.Text.ToUpper
        iTbx.SelectionStart = position
    End Sub
    Public Sub SaisiePremiereLettreMajusculeTbx(iTbx As TextBox)
        Dim txtBox As TextBox = DirectCast(iTbx, TextBox)
        Dim position As Integer = txtBox.SelectionStart ' Sauvegarde la position du curseur

        ' Transformer le texte en "Title Case" (Première lettre en majuscule)
        Dim culture As System.Globalization.CultureInfo = System.Globalization.CultureInfo.CurrentCulture
        Dim textTransform As String = culture.TextInfo.ToTitleCase(txtBox.Text.ToLower())

        ' Appliquer le texte transformé et restaurer le curseur
        If txtBox.Text <> textTransform Then
            txtBox.Text = textTransform
            txtBox.SelectionStart = position ' Restaurer la position du curseur
        End If
    End Sub
    Public Function EffaceToutEspace(Mot As String) As String
        Dim iMot As String = ""
        For Each iChar As Char In Mot
            If iChar <> " " Then iMot += iChar
        Next
        EffaceToutEspace = iMot
    End Function
    Public Function MajusculeEnDebutDeMot(Mot As String) As String

        Dim iMot As String = ""
        Dim iCharP As Char = ""
        If Mot <> "" Then
            Try
                For idx As Integer = 0 To Mot.Length - 1
                    'MsgBox(Mot.Length)
                    If idx = 0 Then
                        If Mot.Chars(1) <> " " Then
                            iMot += UCase(Mot.Chars(idx))
                            iCharP = Mot.Chars(idx)
                        Else
                            iMot += Mot.Chars(idx)
                            iCharP = Mot.Chars(idx)
                        End If
                    Else
                        If iCharP = " " And Mot.Chars(idx) <> " " Then
                            iMot += UCase(Mot.Chars(idx))
                            iCharP = Mot.Chars(idx)
                        Else
                            iMot += Mot.Chars(idx)
                            iCharP = Mot.Chars(idx)
                        End If
                    End If
                Next
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        End If
        MajusculeEnDebutDeMot = iMot
    End Function
    Public Function MajusculeEnDebutDuTexte(Mot As String) As String
        Dim EstActif As Boolean = True
        Dim iMot As String = ""
        Dim iCharP As Char = ""
        If Mot <> "" Then
            Try
                For idx As Integer = 0 To Mot.Length - 1
                    If Mot.Chars(idx) <> " " And EstActif Then
                        iMot += UCase(Mot.Chars(idx))
                        EstActif = False
                    Else
                        iMot += Mot.Chars(idx)
                    End If
                Next
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        End If
        MajusculeEnDebutDuTexte = iMot
    End Function

End Module

