<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FRM_MessageHistory
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
        Me.LSV_Files = New System.Windows.Forms.ListView()
        Me.Filename = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.OpenInExplorerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TXT_UsersHistory = New System.Windows.Forms.TextBox()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'LSV_Files
        '
        Me.LSV_Files.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.Filename})
        Me.LSV_Files.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LSV_Files.Location = New System.Drawing.Point(0, 37)
        Me.LSV_Files.Name = "LSV_Files"
        Me.LSV_Files.Size = New System.Drawing.Size(284, 224)
        Me.LSV_Files.TabIndex = 0
        Me.LSV_Files.UseCompatibleStateImageBehavior = False
        Me.LSV_Files.View = System.Windows.Forms.View.Details
        '
        'Filename
        '
        Me.Filename.Text = "Filename"
        Me.Filename.Width = 279
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenInExplorerToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(284, 24)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'OpenInExplorerToolStripMenuItem
        '
        Me.OpenInExplorerToolStripMenuItem.Name = "OpenInExplorerToolStripMenuItem"
        Me.OpenInExplorerToolStripMenuItem.Size = New System.Drawing.Size(106, 20)
        Me.OpenInExplorerToolStripMenuItem.Text = "&Open In Explorer"
        '
        'TXT_UsersHistory
        '
        Me.TXT_UsersHistory.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.TXT_UsersHistory.Dock = System.Windows.Forms.DockStyle.Top
        Me.TXT_UsersHistory.Enabled = False
        Me.TXT_UsersHistory.Location = New System.Drawing.Point(0, 24)
        Me.TXT_UsersHistory.Name = "TXT_UsersHistory"
        Me.TXT_UsersHistory.ReadOnly = True
        Me.TXT_UsersHistory.Size = New System.Drawing.Size(284, 13)
        Me.TXT_UsersHistory.TabIndex = 2
        '
        'FRM_MessageHistory
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 261)
        Me.Controls.Add(Me.LSV_Files)
        Me.Controls.Add(Me.TXT_UsersHistory)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "FRM_MessageHistory"
        Me.Text = "FRM_MessageHistory"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LSV_Files As ListView
    Friend WithEvents Filename As ColumnHeader
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents OpenInExplorerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents TXT_UsersHistory As TextBox
End Class
