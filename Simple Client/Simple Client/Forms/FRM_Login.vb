Imports System.Security.Cryptography
Imports System.Text.Encoding
Public Class FRM_Login
    Private Sub CMD_SubmitLoginCreds_Click(sender As Object, e As EventArgs) Handles CMD_SubmitLoginCreds.Click
        Dim sha256 As New SHA256Managed
        If TXT_Username.Text = Nothing Or TXT_Password.Text = Nothing Then
            MsgBox("Username or password field empty", MsgBoxStyle.Critical)
            Exit Sub
        End If
        If Not FRM_Messages.SetupUserAfterLogin(sha256.ComputeHash(ASCII.GetBytes(TXT_Password.Text)), TXT_Username.Text) Then
            Return
        End If
        TXT_Username.Clear()
        TXT_Password.Clear()
        Me.Close()
    End Sub
End Class