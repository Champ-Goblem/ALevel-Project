Imports System.IO
Public Class FRM_MessageHistory
    Private imageList As ImageList
    Private username As String
    Public Sub New(ByRef imageList As ImageList, ByVal username As String, ByRef databaseConnection As DatabaseTools)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        LSV_Files.SmallImageList = imageList
        TXT_UsersHistory.Text = "Currently viewing history for: " & username
        Me.imageList = imageList
        Me.username = username
        Dim SQL As String
        SQL = "SELECT COUNT([Filename]) FROM TBL_FILES WHERE [Sender_UserID] = '" & username & "';"
        Dim reader As OleDb.OleDbDataReader = databaseConnection.sendSQLQuery(SQL)
        reader.Read()
        Dim fileCount As Integer = reader.GetInt32(0)
        If fileCount = 0 Then
            LSV_Files.Items.Add("No Files")
        Else
            SQL = "SELECT [Filename] FROM TBL_FILES WHERE [Sender_UserID] = '" & username & "';"
            reader = databaseConnection.sendSQLQuery(SQL)
            reader.Read()
            For i = 0 To fileCount - 1
                Dim filename As String = reader.GetString(0)
                Dim lvi As New ListViewItem
                lvi.Text = filename
                If File.Exists(FileIO.SpecialDirectories.MyDocuments & "/IM Application/" & username & "/" & filename) Then
                    lvi.ImageIndex = 1
                Else
                    lvi.ImageIndex = 0
                End If
                LSV_Files.Items.Add(lvi)
                reader.Read()
            Next
        End If
    End Sub

    Private Sub OpenInExplorerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenInExplorerToolStripMenuItem.Click
        If Not Directory.Exists(FileIO.SpecialDirectories.MyDocuments & "/IM Application/" & username & "/") Then
            Directory.CreateDirectory(FileIO.SpecialDirectories.MyDocuments & "/IM Application/" & username & "/")
        End If
        Process.Start(ControlChars.Quote & FileIO.SpecialDirectories.MyDocuments & "/IM Application/" & username & "/" & ControlChars.Quote)
    End Sub
End Class