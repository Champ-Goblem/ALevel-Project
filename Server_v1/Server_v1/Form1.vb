Imports System.Net
Public Class Form1
    Private localIP As System.Net.IPAddress
    Private Delegate Sub writeLogDelegate(ByVal message As String)
    Private Delegate Sub appendClient(ByVal Username As String, ByVal IPAddr As String, ByVal PublicKey() As Byte)
    Private Delegate Sub removeClient(ByVal Row As Integer)
    Dim serverSocket As SocketMT = Nothing
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'If Form1.instance Is Nothing Then
        '    Form1.instance = Me
        'End If
        'Get the current time and date so we can datestamp the start of the log
        Dim timeDate As DateTime = DateTime.Now
        'Add the datestamp to the start of the log in the UI
        RTB_Log.Text = timeDate.Date.ToShortDateString & Space(1) & timeDate.TimeOfDay.ToString & vbNewLine
        'Set the style of the machine IP address selection box, there was a problem if a user entered their own value because we get the selected value rather than the text converting the string to IP address would return null error, so to stop this change the box design to drop down instead
        CB_LocalAddress.DropDownStyle = ComboBoxStyle.DropDownList
        'Get all of the network interfaces on the system such as Ethernet, wireless, bluetooth, loopback address, etc.
        Dim NICs() As System.Net.NetworkInformation.NetworkInterface = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        RTB_Log.AppendText("A list of interfaces were found with these IP Addresses: ")
        For i = 0 To NICs.Length - 1
            Dim IP As String
            Dim gotIP As Boolean
            Dim l As Integer
            'Get the interfaces that are currently active and not deactivated
            If NICs(i).OperationalStatus = Net.NetworkInformation.OperationalStatus.Up Then
                l = NICs(i).GetIPProperties.UnicastAddresses.Count - 1
                Do
                    'loop through the addresses, some versions of this list the MACs first and some list the IP first so we need to make sure we are getting the IPs not the MACs
                    Dim address As New IPAddress({0, 0, 0, 0})
                    System.Net.IPAddress.TryParse(NICs(i).GetIPProperties.UnicastAddresses(l).Address.ToString, address)
                    If address.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                        IP = NICs(i).GetIPProperties.UnicastAddresses(l).Address.ToString()
                        gotIP = True
                    End If
                Loop Until gotIP
                'Add the IP to the selection box
                CB_LocalAddress.Items.Add(IP)
                'Add the interface name and the corresponding IP to the list box
                LB_Inetfaces.Items.Add(NICs(i).Name & " : " & IP)
                RTB_Log.AppendText(IP & Space(1))
            End If
        Next
        RTB_Log.AppendText(vbNewLine)
        'set the default port
        TXT_Port.Text = "3434"

    End Sub
    Private Sub BTN_StartServer_Click(sender As Object, e As EventArgs) Handles BTN_StartServer.Click
        'Check if the server is currently running and cancel the setup if it already is
        If serverSocket IsNot Nothing Then
            RTB_Log.AppendText("Instance of server already running" & vbNewLine)
            MessageBox.Show("Check server log for detials", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        'Get the selected item from the local IP box
        Dim IPByte() As Byte = SocketMT.Convert_IP_DecimalNotated(CB_LocalAddress.SelectedItem)
        'Just some error checking
        If IsNothing(IPByte) Then
            MessageBox.Show("Check server log for detials", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        'Check if the port is numeric, error if its not
        If Not IsNumeric(TXT_Port.Text) Then
            RTB_Log.AppendText("Value for port is not allowed" & vbNewLine)
            MessageBox.Show("Check server log for detials", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        'Parse the IP string to IP
        localIP = New System.Net.IPAddress(IPByte)
        'New instance of SocketMT
        serverSocket = New SocketMT
        'Got to make sure that the listener is started correctly before continuing
        If Not serverSocket.TcpListener_Start(TXT_Port.Text, localIP) Then
            MessageBox.Show("Check server log for detials", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            serverSocket = Nothing
            Return
        End If
        'Change the desgin so the user cant start the server again
        BTN_StartServer.Enabled = False
        BTN_StopServer.Enabled = True
        Me.Icon = GetEmbeddedIcon("Running.ico")
        Me.Text = "Server - Running"
    End Sub
    Private Function GetEmbeddedIcon(ByVal strName As String) As Icon
        'Gets the icons for the program stored as an embedded resource in the final executable
        Dim p As System.Reflection.Assembly
        p = System.Reflection.Assembly.GetExecutingAssembly()
        Dim ic As Icon
        ic = New System.Drawing.Icon(p.GetManifestResourceStream(Me.GetType(), strName))
        Return ic
    End Function
    Public Sub writeLog(ByVal Message As String)
        'Used for default form interactions from other threads, this uses the main thread to update the form for saftey reasons
        If RTB_Log.InvokeRequired = True Then
            Dim Dgate As writeLogDelegate = New writeLogDelegate(AddressOf writeLog)
            RTB_Log.BeginInvoke(Dgate, New Object() {Message})
        Else
            RTB_Log.AppendText(Message)
        End If
    End Sub
    Public Sub addNewClient(ByVal Username As String, ByVal IPAddr As String, ByVal PublicKey() As Byte)
        'Adds a new client to the data grid viewer in the User tab
        If DGV_Users.InvokeRequired Then
            Dim Dgate As appendClient = New appendClient(AddressOf addNewClient)
            DGV_Users.BeginInvoke(Dgate, New Object() {Username, IPAddr, PublicKey})
        Else
            Dim key As Long = BitConverter.ToInt64(PublicKey, 0)
            DGV_Users.Rows.Add(New String() {Username, IPAddr, key})
        End If
    End Sub
    Public Sub removeClientDGV(ByVal Row As Integer)
        'Removes a client from the data grid viewer in the User tab
        If DGV_Users.InvokeRequired Then
            Dim Dgate As removeClient = New removeClient(AddressOf removeClientDGV)
            DGV_Users.BeginInvoke(Dgate, New Object() {Row})
        Else
            Try
                DGV_Users.Rows.RemoveAt(Row)
            Catch ex As Exception
                'all is well client wasnt added in the first place
                Return
            End Try
        End If
    End Sub
    Private Sub Form1_Closing(ByVal Sender As Object, ByVal e As EventArgs) Handles Me.Closing
        'check if the server is already running, if not we dont need to shut anything down
        If serverSocket Is Nothing Then
            Return
        End If
        'shutdown the server before closing
        serverSocket.TcpListener_Stop()
    End Sub
    Private Sub BTN_StopServer_Click(sender As Object, e As EventArgs) Handles BTN_StopServer.Click
        'stop the server and reset UI stuff
        If serverSocket Is Nothing Then
            RTB_Log.AppendText("Server instance has not been set, server is not running" & vbNewLine)
            MessageBox.Show("Check server log for detials", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        serverSocket.TcpListener_Stop()
        serverSocket = Nothing
        BTN_StartServer.Enabled = True
        BTN_StopServer.Enabled = False
        Me.Icon = GetEmbeddedIcon("Stopped.ico")
        Me.Text = "Server - Suspended"
    End Sub
End Class
