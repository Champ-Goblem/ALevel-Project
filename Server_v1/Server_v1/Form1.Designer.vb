<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.TBC_ServerProperties = New System.Windows.Forms.TabControl()
        Me.TP_Settings = New System.Windows.Forms.TabPage()
        Me.CB_LocalAddress = New System.Windows.Forms.ComboBox()
        Me.BTN_StartServer = New System.Windows.Forms.Button()
        Me.BTN_StopServer = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TXT_Port = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TP_Users = New System.Windows.Forms.TabPage()
        Me.DGV_Users = New System.Windows.Forms.DataGridView()
        Me.Username = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Address = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Status = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TP_Log = New System.Windows.Forms.TabPage()
        Me.RTB_Log = New System.Windows.Forms.RichTextBox()
        Me.LB_Inetfaces = New System.Windows.Forms.ListBox()
        Me.TBC_ServerProperties.SuspendLayout()
        Me.TP_Settings.SuspendLayout()
        Me.TP_Users.SuspendLayout()
        CType(Me.DGV_Users, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TP_Log.SuspendLayout()
        Me.SuspendLayout()
        '
        'TBC_ServerProperties
        '
        Me.TBC_ServerProperties.Controls.Add(Me.TP_Settings)
        Me.TBC_ServerProperties.Controls.Add(Me.TP_Users)
        Me.TBC_ServerProperties.Controls.Add(Me.TP_Log)
        Me.TBC_ServerProperties.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TBC_ServerProperties.Location = New System.Drawing.Point(0, 0)
        Me.TBC_ServerProperties.Name = "TBC_ServerProperties"
        Me.TBC_ServerProperties.SelectedIndex = 0
        Me.TBC_ServerProperties.Size = New System.Drawing.Size(621, 460)
        Me.TBC_ServerProperties.TabIndex = 0
        '
        'TP_Settings
        '
        Me.TP_Settings.Controls.Add(Me.LB_Inetfaces)
        Me.TP_Settings.Controls.Add(Me.CB_LocalAddress)
        Me.TP_Settings.Controls.Add(Me.BTN_StartServer)
        Me.TP_Settings.Controls.Add(Me.BTN_StopServer)
        Me.TP_Settings.Controls.Add(Me.Label2)
        Me.TP_Settings.Controls.Add(Me.TXT_Port)
        Me.TP_Settings.Controls.Add(Me.Label1)
        Me.TP_Settings.Location = New System.Drawing.Point(4, 22)
        Me.TP_Settings.Name = "TP_Settings"
        Me.TP_Settings.Padding = New System.Windows.Forms.Padding(3)
        Me.TP_Settings.Size = New System.Drawing.Size(613, 434)
        Me.TP_Settings.TabIndex = 0
        Me.TP_Settings.Text = "Settings"
        Me.TP_Settings.UseVisualStyleBackColor = True
        '
        'CB_LocalAddress
        '
        Me.CB_LocalAddress.FormattingEnabled = True
        Me.CB_LocalAddress.Location = New System.Drawing.Point(11, 31)
        Me.CB_LocalAddress.Name = "CB_LocalAddress"
        Me.CB_LocalAddress.Size = New System.Drawing.Size(97, 21)
        Me.CB_LocalAddress.TabIndex = 11
        '
        'BTN_StartServer
        '
        Me.BTN_StartServer.Location = New System.Drawing.Point(449, 405)
        Me.BTN_StartServer.Name = "BTN_StartServer"
        Me.BTN_StartServer.Size = New System.Drawing.Size(75, 23)
        Me.BTN_StartServer.TabIndex = 9
        Me.BTN_StartServer.Text = "Start"
        Me.BTN_StartServer.UseVisualStyleBackColor = True
        '
        'BTN_StopServer
        '
        Me.BTN_StopServer.Enabled = False
        Me.BTN_StopServer.Location = New System.Drawing.Point(530, 405)
        Me.BTN_StopServer.Name = "BTN_StopServer"
        Me.BTN_StopServer.Size = New System.Drawing.Size(75, 23)
        Me.BTN_StopServer.TabIndex = 8
        Me.BTN_StopServer.Text = "Stop"
        Me.BTN_StopServer.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(8, 59)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(26, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Port"
        '
        'TXT_Port
        '
        Me.TXT_Port.Location = New System.Drawing.Point(8, 75)
        Me.TXT_Port.Name = "TXT_Port"
        Me.TXT_Port.Size = New System.Drawing.Size(100, 20)
        Me.TXT_Port.TabIndex = 2
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(8, 15)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(79, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Server Address"
        '
        'TP_Users
        '
        Me.TP_Users.Controls.Add(Me.DGV_Users)
        Me.TP_Users.Location = New System.Drawing.Point(4, 22)
        Me.TP_Users.Name = "TP_Users"
        Me.TP_Users.Padding = New System.Windows.Forms.Padding(3)
        Me.TP_Users.Size = New System.Drawing.Size(613, 434)
        Me.TP_Users.TabIndex = 1
        Me.TP_Users.Text = "Users"
        Me.TP_Users.UseVisualStyleBackColor = True
        '
        'DGV_Users
        '
        Me.DGV_Users.AllowUserToAddRows = False
        Me.DGV_Users.AllowUserToDeleteRows = False
        Me.DGV_Users.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DGV_Users.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Username, Me.Address, Me.Status})
        Me.DGV_Users.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DGV_Users.Location = New System.Drawing.Point(3, 3)
        Me.DGV_Users.Name = "DGV_Users"
        Me.DGV_Users.ReadOnly = True
        Me.DGV_Users.Size = New System.Drawing.Size(607, 428)
        Me.DGV_Users.TabIndex = 0
        '
        'Username
        '
        Me.Username.HeaderText = "Username"
        Me.Username.Name = "Username"
        Me.Username.ReadOnly = True
        '
        'Address
        '
        Me.Address.HeaderText = "IP Address"
        Me.Address.Name = "Address"
        Me.Address.ReadOnly = True
        '
        'Status
        '
        Me.Status.HeaderText = "Public Key"
        Me.Status.Name = "Status"
        Me.Status.ReadOnly = True
        Me.Status.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'TP_Log
        '
        Me.TP_Log.Controls.Add(Me.RTB_Log)
        Me.TP_Log.Location = New System.Drawing.Point(4, 22)
        Me.TP_Log.Name = "TP_Log"
        Me.TP_Log.Padding = New System.Windows.Forms.Padding(3)
        Me.TP_Log.Size = New System.Drawing.Size(613, 434)
        Me.TP_Log.TabIndex = 2
        Me.TP_Log.Text = "Console Log"
        Me.TP_Log.UseVisualStyleBackColor = True
        '
        'RTB_Log
        '
        Me.RTB_Log.Cursor = System.Windows.Forms.Cursors.No
        Me.RTB_Log.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RTB_Log.Location = New System.Drawing.Point(3, 3)
        Me.RTB_Log.Name = "RTB_Log"
        Me.RTB_Log.ReadOnly = True
        Me.RTB_Log.Size = New System.Drawing.Size(607, 428)
        Me.RTB_Log.TabIndex = 0
        Me.RTB_Log.Text = "Console Log"
        '
        'LB_Inetfaces
        '
        Me.LB_Inetfaces.FormattingEnabled = True
        Me.LB_Inetfaces.Location = New System.Drawing.Point(261, 15)
        Me.LB_Inetfaces.Name = "LB_Inetfaces"
        Me.LB_Inetfaces.Size = New System.Drawing.Size(344, 95)
        Me.LB_Inetfaces.TabIndex = 12
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(621, 460)
        Me.Controls.Add(Me.TBC_ServerProperties)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form1"
        Me.Text = "Server - Suspended"
        Me.TBC_ServerProperties.ResumeLayout(False)
        Me.TP_Settings.ResumeLayout(False)
        Me.TP_Settings.PerformLayout()
        Me.TP_Users.ResumeLayout(False)
        CType(Me.DGV_Users, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TP_Log.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TBC_ServerProperties As System.Windows.Forms.TabControl
    Friend WithEvents TP_Settings As System.Windows.Forms.TabPage
    Friend WithEvents BTN_StartServer As System.Windows.Forms.Button
    Friend WithEvents BTN_StopServer As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TXT_Port As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TP_Users As System.Windows.Forms.TabPage
    Friend WithEvents DGV_Users As System.Windows.Forms.DataGridView
    Friend WithEvents TP_Log As System.Windows.Forms.TabPage
    Friend WithEvents CB_LocalAddress As System.Windows.Forms.ComboBox
    Friend WithEvents RTB_Log As RichTextBox
    Friend WithEvents Username As DataGridViewTextBoxColumn
    Friend WithEvents Address As DataGridViewTextBoxColumn
    Friend WithEvents Status As DataGridViewTextBoxColumn
    Friend WithEvents LB_Inetfaces As ListBox
End Class
