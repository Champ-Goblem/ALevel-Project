<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FRM_Login
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TXT_Username = New System.Windows.Forms.TextBox()
        Me.TXT_Password = New System.Windows.Forms.TextBox()
        Me.CMD_SubmitLoginCreds = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(55, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Username"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(16, 61)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(53, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Password"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TXT_Username
        '
        Me.TXT_Username.Location = New System.Drawing.Point(13, 30)
        Me.TXT_Username.Name = "TXT_Username"
        Me.TXT_Username.Size = New System.Drawing.Size(302, 20)
        Me.TXT_Username.TabIndex = 2
        '
        'TXT_Password
        '
        Me.TXT_Password.Location = New System.Drawing.Point(13, 77)
        Me.TXT_Password.Name = "TXT_Password"
        Me.TXT_Password.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.TXT_Password.Size = New System.Drawing.Size(302, 20)
        Me.TXT_Password.TabIndex = 3
        Me.TXT_Password.UseSystemPasswordChar = True
        '
        'CMD_SubmitLoginCreds
        '
        Me.CMD_SubmitLoginCreds.Location = New System.Drawing.Point(240, 103)
        Me.CMD_SubmitLoginCreds.Name = "CMD_SubmitLoginCreds"
        Me.CMD_SubmitLoginCreds.Size = New System.Drawing.Size(75, 23)
        Me.CMD_SubmitLoginCreds.TabIndex = 4
        Me.CMD_SubmitLoginCreds.Text = "Login"
        Me.CMD_SubmitLoginCreds.UseVisualStyleBackColor = True
        '
        'FRM_Login
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(327, 135)
        Me.Controls.Add(Me.CMD_SubmitLoginCreds)
        Me.Controls.Add(Me.TXT_Password)
        Me.Controls.Add(Me.TXT_Username)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(343, 174)
        Me.MinimumSize = New System.Drawing.Size(343, 174)
        Me.Name = "FRM_Login"
        Me.Text = "FRM_Login"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents TXT_Username As TextBox
    Friend WithEvents TXT_Password As TextBox
    Friend WithEvents CMD_SubmitLoginCreds As Button
End Class
