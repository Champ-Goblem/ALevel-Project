<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FRM_UserSettings
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
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.SyncToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ResetPasswordToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ChangePrivilegeToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AddNewUserToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RemoveUserToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DGV_Users = New System.Windows.Forms.DataGridView()
        Me.Username = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.IsAdministrator = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.DGV_Users, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SyncToolStripMenuItem, Me.ResetPasswordToolStripMenuItem, Me.ChangePrivilegeToolStripMenuItem, Me.AddNewUserToolStripMenuItem, Me.RemoveUserToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(469, 24)
        Me.MenuStrip1.TabIndex = 0
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'SyncToolStripMenuItem
        '
        Me.SyncToolStripMenuItem.Name = "SyncToolStripMenuItem"
        Me.SyncToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
        Me.SyncToolStripMenuItem.Text = "&Sync"
        '
        'ResetPasswordToolStripMenuItem
        '
        Me.ResetPasswordToolStripMenuItem.Name = "ResetPasswordToolStripMenuItem"
        Me.ResetPasswordToolStripMenuItem.Size = New System.Drawing.Size(100, 20)
        Me.ResetPasswordToolStripMenuItem.Text = "&Reset Password"
        '
        'ChangePrivilegeToolStripMenuItem
        '
        Me.ChangePrivilegeToolStripMenuItem.Name = "ChangePrivilegeToolStripMenuItem"
        Me.ChangePrivilegeToolStripMenuItem.Size = New System.Drawing.Size(108, 20)
        Me.ChangePrivilegeToolStripMenuItem.Text = "&Change Privilege"
        '
        'AddNewUserToolStripMenuItem
        '
        Me.AddNewUserToolStripMenuItem.Name = "AddNewUserToolStripMenuItem"
        Me.AddNewUserToolStripMenuItem.Size = New System.Drawing.Size(94, 20)
        Me.AddNewUserToolStripMenuItem.Text = "&Add New User"
        '
        'RemoveUserToolStripMenuItem
        '
        Me.RemoveUserToolStripMenuItem.Name = "RemoveUserToolStripMenuItem"
        Me.RemoveUserToolStripMenuItem.Size = New System.Drawing.Size(88, 20)
        Me.RemoveUserToolStripMenuItem.Text = "&Remove User"
        '
        'DGV_Users
        '
        Me.DGV_Users.AllowUserToAddRows = False
        Me.DGV_Users.AllowUserToDeleteRows = False
        Me.DGV_Users.AllowUserToResizeRows = False
        Me.DGV_Users.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DGV_Users.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Username, Me.IsAdministrator})
        Me.DGV_Users.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DGV_Users.Location = New System.Drawing.Point(0, 24)
        Me.DGV_Users.MultiSelect = False
        Me.DGV_Users.Name = "DGV_Users"
        Me.DGV_Users.ReadOnly = True
        Me.DGV_Users.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.DGV_Users.Size = New System.Drawing.Size(469, 342)
        Me.DGV_Users.TabIndex = 1
        '
        'Username
        '
        Me.Username.HeaderText = "Usename"
        Me.Username.Name = "Username"
        Me.Username.ReadOnly = True
        '
        'IsAdministrator
        '
        Me.IsAdministrator.HeaderText = "IsAdministrator"
        Me.IsAdministrator.Name = "IsAdministrator"
        Me.IsAdministrator.ReadOnly = True
        '
        'FRM_UserSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(469, 366)
        Me.Controls.Add(Me.DGV_Users)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "FRM_UserSettings"
        Me.Text = "FRM_UserSettings"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.DGV_Users, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents SyncToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ResetPasswordToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DGV_Users As DataGridView
    Friend WithEvents Username As DataGridViewTextBoxColumn
    Friend WithEvents IsAdministrator As DataGridViewCheckBoxColumn
    Friend WithEvents AddNewUserToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RemoveUserToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ChangePrivilegeToolStripMenuItem As ToolStripMenuItem
End Class
