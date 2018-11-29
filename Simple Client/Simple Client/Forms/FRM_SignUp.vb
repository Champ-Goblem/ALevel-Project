Public Class FRM_SignUp
    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Me.Hide()
        FRM_Login.Show()
    End Sub

    Private Sub CMD_SubmitSignupCreds_Click(sender As Object, e As EventArgs) Handles CMD_SubmitSignupCreds.Click
        If TXT_Password.Text <> TXT_ConfirmPassword.Text Or TXT_Password.Text = Nothing Or TXT_ConfirmPassword.Text = Nothing Then
            MsgBox("Passwords do not match", MsgBoxStyle.Critical)
            Exit Sub
        End If
        If TXT_Username.Text = Nothing Then
            MsgBox("Username is not allowed", MsgBoxStyle.Critical)
        End If

        TXT_Username.Clear()
        TXT_Password.Clear()
        Me.Hide()
    End Sub
End Class