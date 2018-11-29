<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FRM_SignUp
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TXT_Username = New System.Windows.Forms.TextBox()
        Me.TXT_Password = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TXT_ConfirmPassword = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.CMD_SubmitSignupCreds = New System.Windows.Forms.Button()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(58, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Username:"
        '
        'TXT_Username
        '
        Me.TXT_Username.Location = New System.Drawing.Point(16, 30)
        Me.TXT_Username.Name = "TXT_Username"
        Me.TXT_Username.Size = New System.Drawing.Size(256, 20)
        Me.TXT_Username.TabIndex = 1
        '
        'TXT_Password
        '
        Me.TXT_Password.Location = New System.Drawing.Point(16, 72)
        Me.TXT_Password.Name = "TXT_Password"
        Me.TXT_Password.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.TXT_Password.Size = New System.Drawing.Size(256, 20)
        Me.TXT_Password.TabIndex = 3
        Me.TXT_Password.UseSystemPasswordChar = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(13, 55)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(56, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Password:"
        '
        'TXT_ConfirmPassword
        '
        Me.TXT_ConfirmPassword.Location = New System.Drawing.Point(16, 114)
        Me.TXT_ConfirmPassword.Name = "TXT_ConfirmPassword"
        Me.TXT_ConfirmPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.TXT_ConfirmPassword.Size = New System.Drawing.Size(256, 20)
        Me.TXT_ConfirmPassword.TabIndex = 5
        Me.TXT_ConfirmPassword.UseSystemPasswordChar = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(13, 97)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(94, 13)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "Confirm Password:"
        '
        'CMD_SubmitSignupCreds
        '
        Me.CMD_SubmitSignupCreds.Location = New System.Drawing.Point(196, 139)
        Me.CMD_SubmitSignupCreds.Name = "CMD_SubmitSignupCreds"
        Me.CMD_SubmitSignupCreds.Size = New System.Drawing.Size(75, 23)
        Me.CMD_SubmitSignupCreds.TabIndex = 6
        Me.CMD_SubmitSignupCreds.Text = "Submit"
        Me.CMD_SubmitSignupCreds.UseVisualStyleBackColor = True
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = True
        Me.LinkLabel1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.LinkLabel1.Location = New System.Drawing.Point(16, 149)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(132, 13)
        Me.LinkLabel1.TabIndex = 7
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.Text = "Have an account? Sign In"
        '
        'FRM_SignUp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 174)
        Me.Controls.Add(Me.LinkLabel1)
        Me.Controls.Add(Me.CMD_SubmitSignupCreds)
        Me.Controls.Add(Me.TXT_ConfirmPassword)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.TXT_Password)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.TXT_Username)
        Me.Controls.Add(Me.Label1)
        Me.Name = "FRM_SignUp"
        Me.Text = "FRM_SignUp"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents TXT_Username As TextBox
    Friend WithEvents TXT_Password As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents TXT_ConfirmPassword As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents CMD_SubmitSignupCreds As Button
    Friend WithEvents LinkLabel1 As LinkLabel
End Class
