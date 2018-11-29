Imports System.Net.Sockets
Imports Error_Handler.Events_Handler
Imports System.Reflection.MethodInfo
Public Class FRM_Messages
    Private WithEvents srFunction As SendRecieve
    Private clientSocket As TcpClient = New TcpClient
    Private remoteUser As String
    Private localUsername As String
    Private WithEvents FormUserSettings As FRM_UserSettings = Nothing
    Private Delegate Sub Delegate_removeUserFromListView(ByVal username As String)
    Private Delegate Sub Delegate_addUserToListView(ByVal username As String, ByVal connected As Integer)
    Private Delegate Sub Delegate_chageUserIcon(ByVal username As String, ByVal connected As Integer)
    Private Delegate Sub Delegate_checkAndAdd(ByVal username As String, ByVal message As String)
    Private Structure databaseMessageReturn
        Public MessageData As String
        Public ReceivedDate As DateTime
        Public username As String
    End Structure
    Private Sub TXT_MessageEnter_KeyPress(sender As Object, e As KeyEventArgs) Handles TXT_MessageEnter.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If localUsername = Nothing Or remoteUser = Nothing Then
                MsgBox("You are either currently not logged in or you have not selected a user to chat to.", MsgBoxStyle.Critical)
                Return
            End If
            Try
                srFunction.sendMessage(remoteUser, TXT_MessageEnter.Text, localUsername)
                WriteMsgToRTX(TXT_MessageEnter.Text, localUsername, True)
            Catch ex As ObjectDisposedException
                srFunction = Nothing
                clientSocket.Close()
                clientSocket = New TcpClient
            End Try
            TXT_MessageEnter.Clear()
        End If
    End Sub

    Private Sub WriteMsgToRTX(sMessage As String, sUsername As String, bIslocalUsername As Boolean, ByVal Optional file As Boolean = False)
        Dim fBold As New Font(RTXT_MessagePanel.Font.FontFamily, RTXT_MessagePanel.Font.Size, FontStyle.Bold)
        Dim sOutString As String
        If Not file Then
            sOutString = sUsername & ": " & sMessage
            RTXT_MessagePanel.AppendText(sOutString)
            If bIslocalUsername Then
                RTXT_MessagePanel.Select(RTXT_MessagePanel.TextLength - sOutString.Length, sOutString.Length)
                RTXT_MessagePanel.SelectionFont = fBold
            End If
        Else
            RTXT_MessagePanel.AppendText(sMessage)
            RTXT_MessagePanel.Select(RTXT_MessagePanel.TextLength - sMessage.Length, sMessage.Length)
            RTXT_MessagePanel.SelectionFont = fBold
        End If
        RTXT_MessagePanel.AppendText(vbNewLine)
    End Sub

    Public Sub setLSVBackColorOrange(ByVal username As String, ByVal color As Color)
        Dim i As Integer
        Do
            If LSV_UserContacts.Items.Item(i).Text = username Then
                LSV_UserContacts.Items.Item(i).BackColor = color
            End If
            i += 1
        Loop Until i > LSV_UserContacts.Items.Count - 1
    End Sub

    Private Sub LSV_UserContacts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LSV_UserContacts.SelectedIndexChanged
        Try
            remoteUser = LSV_UserContacts.SelectedItems(0).Text
        Catch ex As Exception
            errorHappened(LSV_UserContacts, "Make sure you select a user", errorEnum.NULL_REFERECE_EXCEPTION, GetCurrentMethod.Name)
            Return
        End Try
        If localUsername = Nothing Or srFunction Is Nothing Then
            MsgBox("You are either currently not logged in or you have not selected a user to chat to.", MsgBoxStyle.Critical)
            Return
        End If
        LSV_UserContacts.SelectedItems(0).BackColor = System.Drawing.SystemColors.ButtonFace
        TXT_ChatTo.Text = "Currently Chatting To: " + LSV_UserContacts.SelectedItems(0).Text
        RTXT_MessagePanel.Clear()
        If LSV_UserContacts.SelectedItems(0).ImageIndex = 0 Then
            RTXT_MessagePanel.AppendText("The user you are currently chatting to is offline, you may send messages, but the user will not be able to reply until they log on again" & vbNewLine)
        End If
        Dim SQL As String
        SQL = "SELECT COUNT([MessageData]) FROM TBL_MESSAGES WHERE IsFile = False AND Sender_UserID = '" & remoteUser & "';"
        Dim reader As OleDb.OleDbDataReader = srFunction.databaseConnection.sendSQLQuery(SQL)
        reader.Read()
        Dim messageCount As Integer = reader.GetInt32(0)
        If messageCount > 0 Then
            Dim dbValues(messageCount - 1) As databaseMessageReturn
            SQL = "SELECT [MessageData], [ReceivedDate], [Sender_UserID] FROM TBL_MESSAGES WHERE IsFile = False AND Sender_UserID = '" & remoteUser & "';"
            reader = srFunction.databaseConnection.sendSQLQuery(SQL)
            For i = 0 To messageCount - 1
                reader.Read()
                dbValues(i).MessageData = reader.GetString(0)
                dbValues(i).ReceivedDate = DateTime.Parse(reader.GetString(1))
                dbValues(i).username = reader.GetString(2)
            Next
            For i = 0 To messageCount - 1
                Dim difference As TimeSpan = DateTime.Today.Date - dbValues(i).ReceivedDate
                Dim lastTotalDays As Double
                'check that years and months are equal
                If difference.TotalDays <= 2 Then
                    If i = 0 Then
                        lastTotalDays = difference.TotalDays
                        RTXT_MessagePanel.AppendText(dbValues(i).ReceivedDate.ToShortDateString & vbNewLine)
                    ElseIf lastTotalDays <> difference.TotalDays Then
                        RTXT_MessagePanel.AppendText(dbValues(i).ReceivedDate.ToShortDateString & vbNewLine)
                    End If
                    WriteMsgToRTX(dbValues(i).MessageData, dbValues(i).username, False)

                End If
            Next
        End If
        RTXT_MessagePanel.AppendText(DateTime.Today.Date.ToShortDateString & vbNewLine)
        'add in here the editing of the box when we get details from the server
    End Sub

    Private Sub LoginToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoginToolStripMenuItem.Click
        FRM_Login.Show()
    End Sub

    Private Sub FRM_Messages_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RTXT_MessagePanel.Enabled = False
        RTXT_MessagePanel.ForeColor = Color.Black
        LSV_UserContacts.FullRowSelect = True
        Dim IL As New ImageList
        IL.ImageSize = New Size(15, 15)
        IL.Images.Add(GetEmbeddedIcon("offline.ico"))
        IL.Images.Add(GetEmbeddedIcon("online.ico"))
        LSV_UserContacts.SmallImageList = IL
        FRM_Login.Show()
    End Sub

    Private Sub FRM_Messages_Closing(sender As Object, e As EventArgs) Handles MyBase.Closing
        If Not srFunction Is Nothing Then
            srFunction.closeConnection()
        End If
        My.Settings.Default.Save()
    End Sub

    Private Sub LogoutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LogoutToolStripMenuItem.Click
        If localUsername = Nothing Then
            MsgBox("Already logged out", MsgBoxStyle.Critical)
            Exit Sub
        End If
        If Not srFunction Is Nothing Then
            srFunction.closeConnection()
            srFunction = Nothing
        End If
        TLS_Username.Text = "Not Logged In"
        TXT_ChatTo.Clear()
        RTXT_MessagePanel.Clear()
        LSV_UserContacts.Items.Clear()
        LSV_UserContacts.Items.Add("Not Logged In")
        localUsername = Nothing
        remoteUser = Nothing
        LoginToolStripMenuItem.Visible = True
        LogoutToolStripMenuItem.Visible = False
        FilesToolStripMenuItem.Visible = False
    End Sub

    Public Function SetupUserAfterLogin(ByVal passwordHash() As Byte, ByVal username As String) As Boolean
        LSV_UserContacts.Items.Clear()
        Dim IPAddr As String = My.Settings.Default.serverIPAddress
        If IPAddr = "" Then
            IPAddr = Interaction.InputBox("Enter the server's IP address", "Input IP Address")
        End If
        If My.Settings.Default.serverPort = 0 Then
            Try
                My.Settings.Default.serverPort = CInt(Interaction.InputBox("Enter the Servers port", "Input Port"))
            Catch ex As Exception
                Dim port As String
                Do
                    port = Interaction.InputBox("Enter the Servers port", "Input Port")
                Loop Until IsNumeric(port)
                My.Settings.Default.serverPort = CInt(port)
            End Try
        End If
        My.Settings.Default.serverIPAddress = IPAddr
        Try
            clientSocket.Connect(IPAddr, My.Settings.Default.serverPort)
        Catch ex As ObjectDisposedException
            clientSocket.Close()
            clientSocket = New TcpClient
            clientSocket.Connect(IPAddr, My.Settings.Default.serverPort)
        Catch ex As Exception
            errorHappened(clientSocket, ex.Message, errorEnum.SOCKET_EXCEPTION, GetCurrentMethod.Name)
            LSV_UserContacts.Items.Add("Not Logged In")
            Return False
        End Try
        Try
            srFunction = New SendRecieve(clientSocket, username, passwordHash)
        Catch ex As Exception
            'We cant return a form of failure as sub new cant be a function, therefore when it fails we just throw an error
            MsgBox(ex.Message)
            Try
                srFunction.closeConnection()
            Catch exx As Exception
            End Try
            srFunction = Nothing
            clientSocket.Close()
            clientSocket = New TcpClient
            LSV_UserContacts.Items.Add("Not Logged In")
            Return False
        End Try
        localUsername = username
        TLS_Username.Text = "Logged in as: " + localUsername
        LoginToolStripMenuItem.Visible = False
        LogoutToolStripMenuItem.Visible = True
        FilesToolStripMenuItem.Visible = True
        Return True
    End Function

    Private Sub UserSettingsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UserSettingsToolStripMenuItem.Click
        If localUsername = Nothing Or srFunction Is Nothing Then
            Return
        End If
        If FormUserSettings Is Nothing Then
            FormUserSettings = New FRM_UserSettings(srFunction)
            FormUserSettings.ShowDialog()
        End If
    End Sub
    Private Sub FormUserSettings_Closing(sender As Object, e As EventArgs) Handles FormUserSettings.Closed
        FormUserSettings = Nothing
    End Sub
    Private Sub updateFormUserSettings(ByVal data() As Byte) Handles srFunction.recievedPopulateValues
        FormUserSettings.getUserDetails(data)
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Close()
    End Sub

    Public Sub addUserToListView(ByVal username As String, ByVal connected As Integer)
        If LSV_UserContacts.InvokeRequired Then
            Dim Dgate As New Delegate_addUserToListView(AddressOf addUserToListView)
            LSV_UserContacts.BeginInvoke(Dgate, New Object() {username})
        Else
            Dim lvi As New ListViewItem
            lvi.ImageIndex = connected
            lvi.Text = username
            LSV_UserContacts.Items.Add(lvi)
        End If
    End Sub
    Public Sub removeUserFromListView(ByVal username As String)
        If LSV_UserContacts.InvokeRequired Then
            Dim Dgate As New Delegate_removeUserFromListView(AddressOf removeUserFromListView)
            LSV_UserContacts.BeginInvoke(Dgate, New Object() {username})
        Else
            For i = 0 To LSV_UserContacts.Items.Count - 1
                If LSV_UserContacts.Items(i).Text = username Then
                    LSV_UserContacts.Items(i).Remove()
                    Return
                End If
            Next
        End If
    End Sub
    Public Sub changeUserIcon(ByVal username As String, ByVal connected As Integer)
        If LSV_UserContacts.InvokeRequired Then
            Dim Dgate As New Delegate_chageUserIcon(AddressOf changeUserIcon)
            LSV_UserContacts.BeginInvoke(Dgate, New Object() {username, connected})
        Else
            For i = 0 To LSV_UserContacts.Items.Count - 1
                If LSV_UserContacts.Items.Item(i).Text = username Then
                    LSV_UserContacts.Items.Item(i).ImageIndex = connected
                End If
            Next
        End If
    End Sub
    Public Sub checkAndAdd(ByVal username As String, ByVal message As String, ByVal Optional file As Boolean = False)
        If Me.InvokeRequired Then
            Dim dgate As New Delegate_checkAndAdd(AddressOf checkAndAdd)
            Me.BeginInvoke(dgate, New Object() {username, message})
        Else
            If remoteUser = username And file Then
                WriteMsgToRTX(message, username, False, True)
            ElseIf remoteUser = username And Not file Then
                WriteMsgToRTX(message, username, False)
            Else
                If file Then
                    setLSVBackColorOrange(username, Color.LightBlue)
                Else
                    setLSVBackColorOrange(username, Color.Orange)

                End If
            End If
        End If
    End Sub

    Private Function GetEmbeddedIcon(ByVal strName As String) As Icon
        'Gets the icons for the program stored as an embedded resource in the final executable
        Dim p As System.Reflection.Assembly
        p = System.Reflection.Assembly.GetExecutingAssembly()
        Dim ic As Icon
        ic = New System.Drawing.Icon(p.GetManifestResourceStream(Me.GetType(), strName))
        Return ic
    End Function

    Private Sub HistoryToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HistoryToolStripMenuItem.Click
        Dim imageList As New ImageList
        imageList.ImageSize = New Size(15, 15)
        imageList.Images.Add(GetEmbeddedIcon("no.ico"))
        imageList.Images.Add(GetEmbeddedIcon("yes.ico"))
        If remoteUser = Nothing Then
            MsgBox("Select a user before preceding")
            Return
        End If
        Dim history As New FRM_MessageHistory(imageList, remoteUser, srFunction.databaseConnection)
        history.ShowDialog()
    End Sub

    Private Sub SendToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SendToolStripMenuItem.Click
        If remoteUser = Nothing Then
            MsgBox("Select a user before preceding")
            Return
        End If
        OpenFileDialog1.InitialDirectory = System.Environment.SpecialFolder.MyDocuments
        OpenFileDialog1.ShowDialog()
        Dim filepath As String = OpenFileDialog1.FileName
        If filepath = Nothing Then
            Return
        End If
        Try
            srFunction.sendFile(remoteUser, filepath)
        Catch ex As ObjectDisposedException
            srFunction = Nothing
            clientSocket = New TcpClient
            Return
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
            Return
        End Try
    End Sub
End Class
