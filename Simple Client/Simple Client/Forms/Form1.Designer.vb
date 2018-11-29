<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FRM_Messages
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
        Dim ListViewItem2 As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem("Not Logged In", "(none)")
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ApplicationToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AppSettingsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UserSettingsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LoginToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LogoutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FilesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HistoryToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SendToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.TLS_Username = New System.Windows.Forms.ToolStripStatusLabel()
        Me.LSV_UserContacts = New System.Windows.Forms.ListView()
        Me.H_Users = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.RTXT_MessagePanel = New System.Windows.Forms.RichTextBox()
        Me.TXT_MessageEnter = New System.Windows.Forms.TextBox()
        Me.TXT_ChatTo = New System.Windows.Forms.TextBox()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.ApplicationToolStripMenuItem, Me.LoginToolStripMenuItem, Me.LogoutToolStripMenuItem, Me.FilesToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(675, 24)
        Me.MenuStrip1.TabIndex = 0
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "&File"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(92, 22)
        Me.ExitToolStripMenuItem.Text = "&Exit"
        '
        'ApplicationToolStripMenuItem
        '
        Me.ApplicationToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AppSettingsToolStripMenuItem, Me.UserSettingsToolStripMenuItem})
        Me.ApplicationToolStripMenuItem.Name = "ApplicationToolStripMenuItem"
        Me.ApplicationToolStripMenuItem.Size = New System.Drawing.Size(80, 20)
        Me.ApplicationToolStripMenuItem.Text = "&Application"
        '
        'AppSettingsToolStripMenuItem
        '
        Me.AppSettingsToolStripMenuItem.Name = "AppSettingsToolStripMenuItem"
        Me.AppSettingsToolStripMenuItem.Size = New System.Drawing.Size(142, 22)
        Me.AppSettingsToolStripMenuItem.Text = "&App Settings"
        '
        'UserSettingsToolStripMenuItem
        '
        Me.UserSettingsToolStripMenuItem.Name = "UserSettingsToolStripMenuItem"
        Me.UserSettingsToolStripMenuItem.Size = New System.Drawing.Size(142, 22)
        Me.UserSettingsToolStripMenuItem.Text = "&User Settings"
        '
        'LoginToolStripMenuItem
        '
        Me.LoginToolStripMenuItem.Name = "LoginToolStripMenuItem"
        Me.LoginToolStripMenuItem.Size = New System.Drawing.Size(49, 20)
        Me.LoginToolStripMenuItem.Text = "&Login"
        '
        'LogoutToolStripMenuItem
        '
        Me.LogoutToolStripMenuItem.Name = "LogoutToolStripMenuItem"
        Me.LogoutToolStripMenuItem.Size = New System.Drawing.Size(57, 20)
        Me.LogoutToolStripMenuItem.Text = "&Logout"
        Me.LogoutToolStripMenuItem.Visible = False
        '
        'FilesToolStripMenuItem
        '
        Me.FilesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.HistoryToolStripMenuItem, Me.SendToolStripMenuItem})
        Me.FilesToolStripMenuItem.Name = "FilesToolStripMenuItem"
        Me.FilesToolStripMenuItem.Size = New System.Drawing.Size(42, 20)
        Me.FilesToolStripMenuItem.Text = "&Files"
        Me.FilesToolStripMenuItem.Visible = False
        '
        'HistoryToolStripMenuItem
        '
        Me.HistoryToolStripMenuItem.Name = "HistoryToolStripMenuItem"
        Me.HistoryToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.HistoryToolStripMenuItem.Text = "&History"
        '
        'SendToolStripMenuItem
        '
        Me.SendToolStripMenuItem.Name = "SendToolStripMenuItem"
        Me.SendToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.SendToolStripMenuItem.Text = "&Send"
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.SplitContainer1.IsSplitterFixed = True
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 24)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.StatusStrip1)
        Me.SplitContainer1.Panel1.Controls.Add(Me.LSV_UserContacts)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.RTXT_MessagePanel)
        Me.SplitContainer1.Panel2.Controls.Add(Me.TXT_MessageEnter)
        Me.SplitContainer1.Panel2.Controls.Add(Me.TXT_ChatTo)
        Me.SplitContainer1.Size = New System.Drawing.Size(675, 428)
        Me.SplitContainer1.SplitterDistance = 225
        Me.SplitContainer1.TabIndex = 1
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TLS_Username})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 406)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(225, 22)
        Me.StatusStrip1.TabIndex = 1
        '
        'TLS_Username
        '
        Me.TLS_Username.Name = "TLS_Username"
        Me.TLS_Username.Size = New System.Drawing.Size(83, 17)
        Me.TLS_Username.Text = "Not Logged In"
        '
        'LSV_UserContacts
        '
        Me.LSV_UserContacts.AllowColumnReorder = True
        Me.LSV_UserContacts.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.LSV_UserContacts.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.H_Users})
        Me.LSV_UserContacts.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LSV_UserContacts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.LSV_UserContacts.Items.AddRange(New System.Windows.Forms.ListViewItem() {ListViewItem2})
        Me.LSV_UserContacts.Location = New System.Drawing.Point(0, 0)
        Me.LSV_UserContacts.MultiSelect = False
        Me.LSV_UserContacts.Name = "LSV_UserContacts"
        Me.LSV_UserContacts.Size = New System.Drawing.Size(225, 428)
        Me.LSV_UserContacts.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.LSV_UserContacts.TabIndex = 0
        Me.LSV_UserContacts.UseCompatibleStateImageBehavior = False
        Me.LSV_UserContacts.View = System.Windows.Forms.View.Details
        '
        'H_Users
        '
        Me.H_Users.Text = "User Name"
        Me.H_Users.Width = 221
        '
        'RTXT_MessagePanel
        '
        Me.RTXT_MessagePanel.Cursor = System.Windows.Forms.Cursors.Default
        Me.RTXT_MessagePanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RTXT_MessagePanel.Location = New System.Drawing.Point(0, 13)
        Me.RTXT_MessagePanel.Name = "RTXT_MessagePanel"
        Me.RTXT_MessagePanel.ReadOnly = True
        Me.RTXT_MessagePanel.Size = New System.Drawing.Size(446, 395)
        Me.RTXT_MessagePanel.TabIndex = 1
        Me.RTXT_MessagePanel.Text = ""
        '
        'TXT_MessageEnter
        '
        Me.TXT_MessageEnter.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.TXT_MessageEnter.Location = New System.Drawing.Point(0, 408)
        Me.TXT_MessageEnter.Name = "TXT_MessageEnter"
        Me.TXT_MessageEnter.Size = New System.Drawing.Size(446, 20)
        Me.TXT_MessageEnter.TabIndex = 0
        '
        'TXT_ChatTo
        '
        Me.TXT_ChatTo.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.TXT_ChatTo.Dock = System.Windows.Forms.DockStyle.Top
        Me.TXT_ChatTo.Enabled = False
        Me.TXT_ChatTo.Location = New System.Drawing.Point(0, 0)
        Me.TXT_ChatTo.Name = "TXT_ChatTo"
        Me.TXT_ChatTo.ReadOnly = True
        Me.TXT_ChatTo.Size = New System.Drawing.Size(446, 13)
        Me.TXT_ChatTo.TabIndex = 2
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'FRM_Messages
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(675, 452)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "FRM_Messages"
        Me.Text = "Messages"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel1.PerformLayout()
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.Panel2.PerformLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ApplicationToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AppSettingsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents UserSettingsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents RTXT_MessagePanel As RichTextBox
    Friend WithEvents TXT_MessageEnter As TextBox
    Friend WithEvents LSV_UserContacts As ListView
    Protected WithEvents H_Users As ColumnHeader
    Friend WithEvents LoginToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LogoutToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents TLS_Username As ToolStripStatusLabel
    Friend WithEvents SplitContainer2 As SplitContainer
    Friend WithEvents TXT_ChatTo As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents FilesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents HistoryToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SendToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
End Class
