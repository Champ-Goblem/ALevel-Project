Imports System.Net.Sockets
Imports System.Net
Imports System.Text.Encoding
Imports System.Security.Cryptography
Imports System.IO
Imports System.Numerics
Public Class SocketMT
    Private WithEvents listenerSocket As NewTcpListener
    Private lookupTable As ClientLookupTable = Nothing
    Private databaseConnections As DatabaseTools = Nothing
    Private FileStream As FileStream = Nothing
    Private totalMessage() As Byte = Nothing
    Private recvBufferLen As Integer = Nothing
    Private Structure databaseUserReturn
        Public username As String
        Public publicKey As Curve25519_ECDH.ECPoint
    End Structure
    Private Structure database206Return
        Public username As String
        Public isAdministrator As Boolean
    End Structure
    Public Function TcpListener_Start(ByVal port As Integer, ByVal LocalIP As IPAddress) As Boolean
        'Check if the port is invalid or if the IP address is invalid
        If port = Nothing Or port = 0 Or LocalIP.Equals(Nothing) Then
            Form1.RTB_Log.AppendText("Failed to start socket, 1 or more parameters were invalid" & vbNewLine)
            Return False
        End If
        If port > 65535 Then
            Form1.RTB_Log.AppendText("[ERROR]: Port was larger than the maximum allowed" & vbNewLine)
            Return False
        End If
        databaseConnections = New DatabaseTools(DatabaseTools.databaseEnum.ServerDatabase)
        'Set the instance of the UI updater
        MTMainThreadWriter.setInstance(Form1)
        'Create a new instance of NewTcpListener
        listenerSocket = New NewTcpListener(port, LocalIP)
        Try
            'try creating the socket and catch any errors thrown
            listenerSocket.Start()
        Catch ex As Exception
            Form1.RTB_Log.AppendText(ex.Message & vbNewLine)
            Return False
        End Try
        'Now start polling for a client using our new multithreaded version of TcpClient
        listenerSocket.startClientPoll(listenerSocket)
        Form1.RTB_Log.AppendText("Created TCP listener on: " & LocalIP.ToString & ":" & port & vbNewLine)
        'listenerSocket.Stop()
        'Add a new handler that handles when the pollClientConnetion fires and a new user wants to connect
        AddHandler listenerSocket.clientConnecting, AddressOf Me.client_Connect
        Return True
    End Function
    Private Sub client_Connect()
        'Design of the intitial connection message:
        'start | UNAMELENGHT | UNAME | PKLENGHT | PK  | end
        '      |     BYTE    | BYTES |   BYTE   |BYTES|
        Dim username As String
        Dim IPaddr As String
        Dim endpoint As IPEndPoint
        Dim netStream As NetworkStream
        Dim pkxLength, pkylength As Integer
        Dim passwordHash(31) As Byte
        MTMainThreadWriter.writeLog("New user awaiting acept" & vbNewLine)
        'If there isnt an instance of the lookup table create one
        If lookupTable Is Nothing Then
            lookupTable = New ClientLookupTable
        End If
        'Accept the clients connection 
        Dim client As TcpClient = listenerSocket.AcceptTcpClient()
        If recvBufferLen = Nothing Then
            recvBufferLen = client.ReceiveBufferSize
        End If
        'Get the remote end point and then the IP address of the client
        endpoint = client.Client.RemoteEndPoint
        IPaddr = endpoint.Address.ToString
        'sync the networkstream with the client stream
        netStream = client.GetStream
        Dim recvBytes(client.ReceiveBufferSize) As Byte
        'Read the data from the stream in order to get the username, public key from the client
        Dim readlen As Integer
        Try
            readlen = netStream.Read(recvBytes, 0, client.ReceiveBufferSize)
        Catch ex As Exception
            MTMainThreadWriter.writeLog(ex.Message)
            Return
        End Try
        If readlen <> client.ReceiveBufferSize Then
            Dim total As Integer = readlen
            Do
                Try
                    readlen = netStream.Read(recvBytes, 0, client.ReceiveBufferSize - total)
                Catch ex As Exception
                    MTMainThreadWriter.writeLog(ex.Message)
                    Return
                End Try
                total += readlen
            Loop Until total = client.ReceiveBufferSize
        End If
        'loop through the values from point 1 in the Received data array to the end of username, we dont need to minus 1 because we have offset the data by 1
        Dim usernameBytes(19) As Byte
        Array.ConstrainedCopy(recvBytes, 0, usernameBytes, 0, 20)
        Dim indexNotZero As Integer = getLastNonZeroIndex(usernameBytes)
        Dim usernameBytesNoZeros(indexNotZero) As Byte
        Array.Copy(usernameBytes, usernameBytesNoZeros, indexNotZero + 1)
        username = ASCII.GetString(usernameBytesNoZeros)
        'Now get the public key lenght that is right after the username as shown at the start
        pkxLength = CInt(recvBytes(20))
        pkylength = CInt(recvBytes(21))
        Dim publicKeyx(pkxLength - 1) As Byte
        Dim publicKeyy(pkylength - 1) As Byte
        'Now just loop through as you normally would and offest the recvBytes array by usernamelength and a value of 2 because of the 2 bytes used for data lengths
        Array.ConstrainedCopy(recvBytes, 22, publicKeyx, 0, pkxLength)
        Array.ConstrainedCopy(recvBytes, 22 + pkxLength, publicKeyy, 0, pkylength)
        'Get the password hash to check with the database
        Array.ConstrainedCopy(recvBytes, 22 + pkxLength + pkylength, passwordHash, 0, 32)
        'Check if the details are null
        If publicKeyx Is Nothing Or publicKeyy Is Nothing Or username = Nothing Or passwordHash Is Nothing Then
            MTMainThreadWriter.writeLog("Client connection details are NULL" & vbNewLine)
            client.Close()
            netStream.Dispose()
            Return
        End If

        'Get the password hash from the database
        Dim SQL As String
        SQL = "SELECT Password_Hash, Is_Administrator FROM TBL_USER WHERE Username = '" & username & "';"
        Dim dbPWHashReturn As OleDb.OleDbDataReader = databaseConnections.sendSQLQuery(SQL)
        dbPWHashReturn.Read()
        If Not dbPWHashReturn.HasRows Then
            dbPWHashReturn.Close()
            MTMainThreadWriter.writeLog("Connecting client didnt not provide correct username, password combo, the connection was denied" & vbNewLine)
            Try
                netStream.WriteByte(CByte(1))
            Catch ex As Exception
                MTMainThreadWriter.writeLog(ex.Message)
                Return
            End Try
            client.Close()
            netStream.Dispose()
            Return
        End If
        Dim dbPWHash() As Byte = System.Convert.FromBase64String(dbPWHashReturn(0).ToString)
        Dim isAdmin As Boolean = dbPWHashReturn.GetBoolean(1)
        dbPWHashReturn.Close()
        'Check if the password hashes are equal
        For i = 0 To 31
            If dbPWHash(i) <> passwordHash(i) Then
                MTMainThreadWriter.writeLog("Connecting client didnt not provide correct username, password combo, the connection was denied" & vbNewLine)
                Try
                    netStream.WriteByte(CByte(1))
                Catch ex As Exception
                    MTMainThreadWriter.writeLog(ex.Message)
                    Return
                End Try
                client.Close()
                netStream.Dispose()
                Return
            End If
        Next
        MTMainThreadWriter.writeLog("User was succesfully authenticated" & vbNewLine)
        If lookupTable.CheckClientConnected(IPaddr) = ClientLookupTable.clientLookupTableErrors.CLIENT_CONNECTED Then
            Try
                netStream.WriteByte(CByte(2))
            Catch ex As Exception
                MTMainThreadWriter.writeLog(ex.Message)
                Return
            End Try
            client.Close()
            Return
        End If
        If isAdmin Then
            Try
                netStream.WriteByte(CByte(4))
            Catch ex As Exception
                MTMainThreadWriter.writeLog(ex.Message)
                Return
            End Try
        Else
            Try
                netStream.WriteByte(CByte(3))
            Catch ex As Exception
                MTMainThreadWriter.writeLog(ex.Message)
                Return
            End Try
        End If

        'Get all of the currently connected clients
        Dim connectedClientsValues() As ClientLookupTable.tableValues = lookupTable.GetConnectedClients()
        Dim totalClientsReader As OleDb.OleDbDataReader = databaseConnections.sendSQLQuery("SELECT COUNT(Username) FROM TBL_USER WHERE Username <> '" & username & "' AND Last_Known_PublicKey <> 'NULL';")
        totalClientsReader.Read()
        Dim totalClientCount As Integer = totalClientsReader.GetInt32(0)
        totalClientsReader.Close()

        Dim totalClientsInDB(totalClientCount - 1) As databaseUserReturn
        If totalClientCount <> 0 Then
            totalClientsReader = databaseConnections.sendSQLQuery("SELECT [Username],[Last_Known_PublicKey] FROM TBL_USER WHERE Username <> '" & username & "' AND Last_Known_PublicKey <> 'NULL';")
            For i = 0 To totalClientCount - 1
                totalClientsReader.Read()
                If totalClientsReader.GetString(1) = "NULL" Then
                    totalClientsInDB(i).publicKey = Nothing
                    totalClientsInDB(i).username = totalClientsReader.GetString(0)
                Else
                    Dim pkXY() As String = totalClientsReader.GetString(1).Split(",")
                    Dim pkx As BigInteger = Curve25519_ECDH.Crypto.AES.Convert_IntegerByteArray_ToBigInteger(System.Convert.FromBase64String(pkXY(0)))
                    Dim pky As BigInteger = Curve25519_ECDH.Crypto.AES.Convert_IntegerByteArray_ToBigInteger(System.Convert.FromBase64String(pkXY(1)))
                    totalClientsInDB(i).username = totalClientsReader.GetString(0)
                    totalClientsInDB(i).publicKey = New Curve25519_ECDH.ECPoint(pkx, pky)
                End If
            Next
        End If
        totalClientsReader.Close()

        Dim numberOfClientsBytes() As Byte = BitConverter.GetBytes(totalClientsInDB.Length)
        'send the count of packets that we will send, and transmit that first
        Try
            netStream.Write(numberOfClientsBytes, 0, numberOfClientsBytes.Length)
            netStream.Flush()
        Catch ex As Exception
            MTMainThreadWriter.writeLog(ex.Message)
            Return
        End Try
        Const usernameLen As Integer = 20
        'send:
        '| username | connected | x len | y len |public key x | public key y |
        For i = 0 To totalClientsInDB.Length - 1
            Dim usernameBytes2() As Byte = ASCII.GetBytes(totalClientsInDB(i).username)

            If totalClientsInDB(i).publicKey IsNot Nothing Then
                Dim publicKeyxBytes() As Byte = Curve25519_ECDH.Crypto.AES.Convert_BigInteger_ToByteArray(totalClientsInDB(i).publicKey.x)
                Dim publicKeyyBytes() As Byte = Curve25519_ECDH.Crypto.AES.Convert_BigInteger_ToByteArray(totalClientsInDB(i).publicKey.y)
                Dim message(client.ReceiveBufferSize - 1) As Byte
                usernameBytes2.CopyTo(message, 0)
                message(usernameLen + 1) = publicKeyxBytes.Length
                message(usernameLen + 2) = publicKeyyBytes.Length
                publicKeyxBytes.CopyTo(message, usernameLen + 3)
                publicKeyyBytes.CopyTo(message, usernameLen + 3 + publicKeyxBytes.Length)
                'See if the username we have is equal to the ones already connected
                For c = 0 To connectedClientsValues.Length - 1
                    If connectedClientsValues(c).username = totalClientsInDB(i).username Then
                        message(usernameLen) = 1
                    Else
                        message(usernameLen) = 0
                    End If
                Next
                Try
                    netStream.Write(message, 0, message.Length)
                    netStream.Flush()
                Catch ex As Exception
                    MTMainThreadWriter.writeLog(ex.Message)
                    Return
                End Try
            End If
        Next

        'Sends the files and messages that were stored on the server for when the user connected, i.e. now
        sendUnsentData(netStream, username)

        'Get the unique client number based off of the number of clients in the DataGridView element
        Dim clientCount As Integer = MTMainThreadWriter.GetDGVCount
        'Start the instance of the multithreaded client sender and reciever
        Dim clientHandler As New MTClientReciever(client, clientCount)
        'Add 2 handlers, 1 to deal with removing the client from lookupTable and one to deal with the data once Received
        AddHandler clientHandler.clientDisconnect, AddressOf clientDisconnected
        AddHandler clientHandler.ReceivedData, AddressOf processClientData
        Dim lookupTableValues As New ClientLookupTable.tableValues
        lookupTableValues.clientReference = clientHandler
        lookupTableValues.publicKey = New Curve25519_ECDH.ECPoint(New BigInteger(publicKeyx), New BigInteger(publicKeyy))
        lookupTableValues.username = username
        lookupTable.sendConnectedClientBroadcast(ASCII.GetBytes(username), publicKeyx, publicKeyy)
        SQL = "UPDATE TBL_USER SET [Last_Known_PublicKey] = '" & System.Convert.ToBase64String(publicKeyx) & "," & System.Convert.ToBase64String(publicKeyy) & "' WHERE [Username] = '" & username & "';"
        databaseConnections.sendSQLNonQuery(SQL)
        'try and populate the table with the values above and the IP address as the lookup key
        If lookupTable.AddNewClient(IPaddr, lookupTableValues) = ClientLookupTable.clientLookupTableErrors.TABLE_CONTAINS_CLIENT Then
            MTMainThreadWriter.writeLog("Client already connected" & vbNewLine)
            clientHandler.closeClientConnection(False)
            Return
        End If
        MTMainThreadWriter.AddClientToDGV(username, IPaddr, publicKeyx)
        MTMainThreadWriter.writeLog("[Client: " & clientCount & "]: " & "Created new client with connection number: " & clientCount & vbNewLine)
    End Sub
    Private Sub sendUnsentData(ByRef stream As NetworkStream, ByVal username As String)
        Dim SQL As String

        'Get the number of messages to send
        SQL = "SELECT COUNT(MessageID) FROM TBL_UNSENT_MESSAGES WHERE [Recipient_UserID] = '" & username & "';"
        Dim reader As OleDb.OleDbDataReader = databaseConnections.sendSQLQuery(SQL)
        reader.Read()
        Dim messageCount As Integer = reader.GetInt32(0)
        reader.Close()

        'Get the number of files to send
        SQL = "SELECT COUNT(FileID) FROM TBL_UNSENT_FILES WHERE [Recipient_UserID] = '" & username & "';"
        reader = databaseConnections.sendSQLQuery(SQL)
        reader.Read()
        Dim fileCount As Integer = reader.GetInt32(0)
        reader.Close()

        'Send the number of seperate sets of data being sent over the network for recieve
        Dim packetCount() As Byte = BitConverter.GetBytes(messageCount + fileCount)
        Try
            stream.Write(packetCount, 0, packetCount.Length)
            stream.Flush()
        Catch ex As Exception
            MTMainThreadWriter.writeLog(ex.Message)
            Return
        End Try

        'Send the IMs
        SQL = "SELECT MessageData, Sender_UserID FROM TBL_UNSENT_MESSAGES WHERE [Recipient_UserID] = '" & username & "';"
        reader = databaseConnections.sendSQLQuery(SQL)
        reader.Read()
        For i = 0 To messageCount - 1
            '| Message ID | Username | Lenght | Data ... |
            '| Byte       | 20 Bytes |4 Bytes | Bytes    |
            Dim senderUsername As String = reader.GetString(1)
            Dim senderUsernameBytes() As Byte = ASCII.GetBytes(senderUsername)

            Dim message() As Byte = System.Convert.FromBase64String(reader.GetString(0))
            Dim messageLen() As Byte = BitConverter.GetBytes(message.Length)

            Dim fullMessage(1 + 20 + 4 + message.Length - 1) As Byte

            fullMessage(0) = 1
            senderUsernameBytes.CopyTo(fullMessage, 1)
            messageLen.CopyTo(fullMessage, 21)
            message.CopyTo(fullMessage, 25)
            Try
                stream.Write(fullMessage, 0, fullMessage.Length)
                stream.Flush()
            Catch ex As Exception
                MTMainThreadWriter.writeLog(ex.Message)
                Return
            End Try
            reader.Read()
        Next
        reader.Close()
        SQL = "DELETE FROM TBL_UNSENT_MESSAGES WHERE [Recipient_UserID] = '" & username & "';"
        databaseConnections.sendSQLNonQuery(SQL)

        'send the files
        SQL = "SELECT [Filepath], [Sender_UserID] FROM TBL_UNSENT_FILES WHERE [Recipient_UserID] = '" & username & "';"
        reader = databaseConnections.sendSQLQuery(SQL)
        reader.Read()
        For i = 0 To fileCount - 1
            '| Message ID | Username | Lenght | Filename | Data ... |
            '| Byte       | 20 Bytes |4 Bytes |100 Bytes | Bytes    |
            Dim senderUsername As String = reader.GetString(1)
            Dim senderUsernameBytes() As Byte = ASCII.GetBytes(senderUsername)

            Dim filepath As String = Path.GetFileName(reader.GetString(0))
            Dim filepathBytes() As Byte = ASCII.GetBytes(filepath)
            Dim fs As New FileStream(reader.GetString(0), FileMode.Open)
            Dim message(fs.Length - 1) As Byte
            fs.Read(message, 0, fs.Length)
            fs.Dispose()

            Dim messageLen() As Byte = BitConverter.GetBytes(message.Length)
            Dim fullMessage(1 + 20 + 4 + 100 + message.Length - 1) As Byte

            fullMessage(0) = 2
            senderUsernameBytes.CopyTo(fullMessage, 1)
            messageLen.CopyTo(fullMessage, 21)
            filepathBytes.CopyTo(fullMessage, 25)
            message.CopyTo(fullMessage, 125)
            Try
                stream.Write(fullMessage, 0, fullMessage.Length)
                stream.Flush()
            Catch ex As Exception
                MTMainThreadWriter.writeLog(ex.Message)
                Return
            End Try
            reader.Read()
        Next
        reader.Close()
        SQL = "DELETE FROM TBL_UNSENT_FILES WHERE [Recipient_UserID] = '" & username & "';"
        databaseConnections.sendSQLNonQuery(SQL)
    End Sub

    Private Sub clientDisconnected(ByVal IPAddr As String, ByVal clNumber As Integer)
        'Remove the client from the lookuptable
        Dim username As String = lookupTable.GetClientUname(IPAddr)
        If lookupTable.RemoveClient(IPAddr, clNumber) = ClientLookupTable.clientLookupTableErrors.CLIENT_NOT_CONNECTED Then
            MTMainThreadWriter.writeLog("Tried removing the client " & IPAddr & " but it did not exist")
            Return
        End If
        lookupTable.sendDisconnectedClientBroadcast(ASCII.GetBytes(username))
    End Sub

    Private Sub processClientData(ByVal data() As Byte, ByVal IPAddr As String, ByVal clNumber As Integer)
        Dim client As MTClientReciever = lookupTable.GetClientReference(IPAddr)
        Dim dataValue As Integer
        Try
            'try converting the data to an integer value, but possible that it overflows, so catch this
            dataValue = BitConverter.ToInt32(data, 0)
        Catch ex As Exception
            'value overflowed 32bits, is okay because then we know its not hearbeat or a dead signal
        End Try
        If dataValue = 57005 Then
            'Check for client disconnect value (0xDEAD) and remove any instances of that client
            client.closeClientConnection(True)
        End If
        Select Case CInt(data(0))
            Case Is = 202
                '| Message ID | Username | Password Hash | Administrator |
                '| Byte       |20 Bytes  | 32 Bytes      | Byte          |
                'The decimal value 202 is equal to the hex value CA, used when adding a new user
                Dim usernameBytes(19) As Byte
                Dim passwordHash(31) As Byte
                Dim admin As Byte
                Dim SQL As String
                Dim reader As OleDb.OleDbDataReader
                Array.ConstrainedCopy(data, 1, usernameBytes, 0, 20)
                Array.ConstrainedCopy(data, 21, passwordHash, 0, 32)
                admin = data(53)

                Dim lastNonZeroIndex As Integer = getLastNonZeroIndex(usernameBytes)
                Dim nonZeroUsername(lastNonZeroIndex) As Byte
                Array.Copy(usernameBytes, nonZeroUsername, lastNonZeroIndex + 1)

                'Check if the username who wanted the new user to be created is an admin
                SQL = "SELECT [Is_Administrator] FROM TBL_USER WHERE [Username] = '" & lookupTable.GetClientUname(IPAddr) & "';"
                reader = databaseConnections.sendSQLQuery(SQL)
                reader.Read()

                If Not reader.GetBoolean(0) Then
                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to add a new user, but was not administrator" & vbNewLine)
                    client.sendError(MTClientReciever.sendingErrorEnum.NOT_ADMIN)
                    Return
                End If
                reader.Close()

                'Check if the user already exists
                SQL = "SELECT [Username] FROM TBL_USER WHERE [Username] = '" & ASCII.GetString(nonZeroUsername) & "';"
                reader = databaseConnections.sendSQLQuery(SQL)
                reader.Read()

                If reader.HasRows Then
                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to add a new user, but the username already exists" & vbNewLine)
                    client.sendError(MTClientReciever.sendingErrorEnum.USER_EXISTS)
                    Return
                End If
                reader.Close()

                'Add the new user
                SQL = "INSERT INTO TBL_USER (Username,Password_Hash,Is_Administrator,Last_Known_PublicKey) VALUES ('" & ASCII.GetString(nonZeroUsername) & "','" & System.Convert.ToBase64String(passwordHash) & "'," & CBool(admin).ToString & ",'NULL');"
                databaseConnections.sendSQLNonQuery(SQL)
                client.sendACK()
            Case Is = 203
                'Update password
                '| Message ID | Username | old Password Hash | new Password Hash |
                Dim usernameBytes(19) As Byte
                Dim oldPasswordHash(31) As Byte
                Dim newPasswordHash(31) As Byte
                Dim oldPasswordHashInt As Integer = -99
                Dim SQL As String
                Dim reader As OleDb.OleDbDataReader
                Array.ConstrainedCopy(data, 1, usernameBytes, 0, 20)
                Array.ConstrainedCopy(data, 21, oldPasswordHash, 0, 32)
                Array.ConstrainedCopy(data, 53, newPasswordHash, 0, 32)
                Try
                    oldPasswordHashInt = BitConverter.ToInt32(oldPasswordHash, 0)
                Catch ex As Exception
                End Try

                'Get the username without zero bytes
                Dim lastNonZeroIndex As Integer = getLastNonZeroIndex(usernameBytes)
                Dim nonZeroUsername(lastNonZeroIndex) As Byte
                Array.Copy(usernameBytes, nonZeroUsername, lastNonZeroIndex + 1)

                If oldPasswordHashInt = -99 Then
                    'Check if the users given password is correct
                    SQL = "SELECT [Password_Hash] FROM TBL_USER WHERE [Username] = '" & ASCII.GetString(nonZeroUsername) & "';"
                    reader = databaseConnections.sendSQLQuery(SQL)
                    reader.Read()

                    If Not reader.HasRows Then
                        MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to change user password but user did not exist" & vbNewLine)
                        client.sendError(MTClientReciever.sendingErrorEnum.USER_DOES_NOT_EXIST)
                        Return
                    End If
                    Dim passwordHash() As Byte = System.Convert.FromBase64String(reader.GetString(0))
                    For i = 0 To 31
                        If passwordHash(i) <> oldPasswordHash(i) Then
                            MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to change password, but old password did not match new password " & vbNewLine)
                            client.sendError(MTClientReciever.sendingErrorEnum.PASSWORD_MISMATCH)
                            Return
                        End If
                    Next
                    reader.Close()
                Else
                    'Administrator tried to change password, check if the person was admin
                    SQL = "SELECT [Is_Administrator] FROM TBL_USER WHERE [Username] = '" & lookupTable.GetClientUname(IPAddr) & "';"
                    reader = databaseConnections.sendSQLQuery(SQL)
                    reader.Read()

                    If Not reader.GetBoolean(0) Then
                        MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to change password, but was not administrator" & vbNewLine)
                        client.sendError(MTClientReciever.sendingErrorEnum.NOT_ADMIN)
                        Return
                    End If
                    reader.Close()
                End If
                'Update the password
                SQL = "UPDATE TBL_USER SET [Password_Hash] = '" & System.Convert.ToBase64String(newPasswordHash) & "' WHERE [Username] = '" & ASCII.GetString(nonZeroUsername) & "';"
                databaseConnections.sendSQLNonQuery(SQL)
                client.sendACK()
            Case Is = 204
                'Set the users rights
                '| Message ID | Username | Admin |
                Dim usernameBytes(19) As Byte
                Dim admin As Byte
                Dim SQL As String
                Dim reader As OleDb.OleDbDataReader
                Array.ConstrainedCopy(data, 1, usernameBytes, 0, 20)
                admin = data(22)

                'Get the username without zero bytes
                Dim lastNonZeroIndex As Integer = getLastNonZeroIndex(usernameBytes)
                Dim nonZeroUsername(lastNonZeroIndex) As Byte
                Array.Copy(usernameBytes, nonZeroUsername, lastNonZeroIndex + 1)

                'Check if the username who wanted the user to be set admin, is admin
                SQL = "SELECT [Is_Administrator] FROM TBL_USER WHERE [Username] = '" & lookupTable.GetClientUname(IPAddr) & "';"
                reader = databaseConnections.sendSQLQuery(SQL)
                reader.Read()

                If Not reader.GetBoolean(0) Then
                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to add a new user, but was not administrator" & vbNewLine)
                    client.sendError(MTClientReciever.sendingErrorEnum.NOT_ADMIN)
                    Return
                End If
                reader.Close()

                'Check the user exists
                SQL = "SELECT [Username] FROM TBL_USER WHERE [Username] = '" & ASCII.GetString(nonZeroUsername) & "';"
                reader = databaseConnections.sendSQLQuery(SQL)
                reader.Read()
                If Not reader.HasRows Then
                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to change user password but user did not exist" & vbNewLine)
                    client.sendError(MTClientReciever.sendingErrorEnum.USER_DOES_NOT_EXIST)
                    Return
                End If
                reader.Close()

                'Set the user as an admin
                SQL = "UPDATE TBL_USER SET [Is_Administrator] = " & CBool(admin) & " WHERE [Username] = '" & ASCII.GetString(nonZeroUsername) & "';"
                databaseConnections.sendSQLNonQuery(SQL)
                client.sendACK()
            Case Is = 205
                'Removing a user
                '| Message ID | Username |
                '| Byte       |20 Bytes  |
                Dim usernameBytes(19) As Byte
                Dim SQL As String
                Dim reader As OleDb.OleDbDataReader
                Array.ConstrainedCopy(data, 1, usernameBytes, 0, 20)

                Dim lastNonZeroIndex As Integer = getLastNonZeroIndex(usernameBytes)
                Dim nonZeroUsername(lastNonZeroIndex) As Byte
                Array.Copy(usernameBytes, nonZeroUsername, lastNonZeroIndex + 1)

                'Check if the username who wanted the new user to be removed is an admin
                SQL = "SELECT [Is_Administrator] FROM TBL_USER WHERE [Username] = '" & lookupTable.GetClientUname(IPAddr) & "';"
                reader = databaseConnections.sendSQLQuery(SQL)
                reader.Read()
                If Not reader.GetBoolean(0) Then
                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to remove a user, but was not administrator" & vbNewLine)
                    client.sendError(MTClientReciever.sendingErrorEnum.NOT_ADMIN)
                    Return
                End If
                reader.Close()

                'Check the user exists
                SQL = "SELECT [Username] FROM TBL_USER WHERE [Username] = '" & ASCII.GetString(nonZeroUsername) & "';"
                reader = databaseConnections.sendSQLQuery(SQL)
                reader.Read()
                If Not reader.HasRows Then
                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Tried to change user password but user did not exist" & vbNewLine)
                    client.sendError(MTClientReciever.sendingErrorEnum.USER_DOES_NOT_EXIST)
                    Return
                End If
                reader.Close()

                'Add the new user
                SQL = "DELETE FROM TBL_USER WHERE [Username] = '" & ASCII.GetString(nonZeroUsername) & "';"
                databaseConnections.sendSQLNonQuery(SQL)
                client.sendACK()
            Case Is = 206
                'Send back all of the users in the database and whether they are an administrator
                Dim connectedClientsValues() As ClientLookupTable.tableValues = lookupTable.GetConnectedClients()
                Dim totalClientsReader As OleDb.OleDbDataReader = databaseConnections.sendSQLQuery("SELECT COUNT(Username) FROM TBL_USER;")
                totalClientsReader.Read()
                Dim totalClientCount As Integer = totalClientsReader.GetInt32(0)
                totalClientsReader.Close()

                Dim totalClientsInDB(totalClientCount - 1) As database206Return
                If totalClientCount <> 0 Then
                    totalClientsReader = databaseConnections.sendSQLQuery("SELECT [Username], [Is_Administrator] FROM TBL_USER;")
                    For i = 0 To totalClientCount - 1
                        totalClientsReader.Read()
                        totalClientsInDB(i).username = totalClientsReader.GetString(0)
                        totalClientsInDB(i).isAdministrator = totalClientsReader.GetBoolean(1)
                    Next
                End If
                totalClientsReader.Close()
                client.sendACK()
                'send the count of packets that we will send, and transmit that first
                Const usernameLen As Integer = 20
                'send:
                '| MessageID | username | isAdmin |
                For i = 0 To totalClientsInDB.Length - 1
                    Dim usernameBytes() As Byte = ASCII.GetBytes(totalClientsInDB(i).username)
                    Dim message(21) As Byte
                    message(0) = 206
                    usernameBytes.CopyTo(message, 1)
                    message(usernameLen + 1) = CByte(totalClientsInDB(i).isAdministrator)
                    client.sendData(message)
                Next
            Case Else
                Dim messageType As Byte = data(0)
                Dim lenghtOfUsefulData As Integer = data.Length - 1
                Dim senderUsername(19) As Byte
                Const usernameLength As Integer = 20
                Const offset As Integer = 3
                Const hashLen As Integer = 32
                Dim sha256 As New SHA256Managed
                Array.Copy(ASCII.GetBytes(lookupTable.GetClientUname(IPAddr)), senderUsername, ASCII.GetByteCount(lookupTable.GetClientUname(IPAddr)))
                'We need to reprocess the data for the end user, which means changing the username from the recipients to the senders and therefore altering the username length
                '| Message Type | x | of y | username | data hash | data |
                '|     Byte     | B |   B  | 20 bytes | 32 bytes  | Bytes|
                If data(0) <> 1 And data(0) <> 2 Then
                    client.sendError()
                    Return
                End If
                If data(1) = data(2) Then
                    lenghtOfUsefulData = getLastNonZeroIndex(data, usernameLength + 3)
                End If
                Dim x As Byte = data(1)
                Dim y As Byte = data(2)
                'Dim usernameLength As Byte = data(3)
                'retrieve the full 20 bytes of the username from the message
                Dim username(usernameLength - 1) As Byte
                For i = 0 To usernameLength - 1
                    username(i) = data(i + offset)
                Next
                Dim lastNonZeroIndex As Integer = getLastNonZeroIndex(username)
                Dim nonZeroUsername(lastNonZeroIndex) As Byte
                'Cut off the zero bytes by copying the userful data into a buffer
                Array.Copy(username, nonZeroUsername, lastNonZeroIndex + 1)
                If x = 0 Then

                    Const filenameLen As Integer = 100
                    Dim initalMessage(data.Length - 1) As Byte
                    Dim fileNameZero(filenameLen - 1) As Byte

                    For i = 0 To filenameLen - 1
                        fileNameZero(i) = data(i + offset + usernameLength)
                    Next
                    Dim lastNonZeroFilenameIndex As Integer = getLastNonZeroIndex(fileNameZero)
                    Dim fileName(lastNonZeroFilenameIndex) As Byte
                    Array.Copy(fileNameZero, fileName, lastNonZeroFilenameIndex + 1)
                    Dim recp As String = lookupTable.ResolveUsernameToIP(ASCII.GetString(nonZeroUsername))

                    If recp = Nothing Then

                        'Got to now store the message for later, that user is not currently logged on
                        MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Storing message for later for the user " & recp & vbNewLine)
                        If Not Directory.Exists(Environment.CurrentDirectory & "/TEMP/" & ASCII.GetString(nonZeroUsername)) Then
                            Try
                                Directory.CreateDirectory(Environment.CurrentDirectory & "/TEMP/" & ASCII.GetString(nonZeroUsername))
                            Catch sysex As System.ArgumentException
                                MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & sysex.Message & vbNewLine)
                                client.sendError()
                                Return
                            End Try
                        End If

                        'Open the filestream for the next bytes we are ready to recieve
                        Try
                            FileStream = New FileStream(Environment.CurrentDirectory & "\TEMP\" & ASCII.GetString(nonZeroUsername) & "\" & ASCII.GetString(fileName), FileMode.Create)
                        Catch ex As Exception
                            MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & ex.Message & vbNewLine)
                        End Try

                        client.sendACK()
                        Return
                    End If

                    client.sendACK()

                    'Here we get that clients reference
                    Dim reference As MTClientReciever = lookupTable.GetClientReference(recp)

                    initalMessage(0) = messageType
                    initalMessage(1) = x
                    initalMessage(2) = y

                    senderUsername.CopyTo(initalMessage, offset)
                    fileName.CopyTo(initalMessage, offset + usernameLength)
                    reference.sendData(initalMessage)

                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Forwarding data " & IPAddr & " >> " & recp & vbNewLine)
                    Return
                End If
                'printArray(data, 10)
                Dim hashBytes(hashLen - 1) As Byte
                For i = 0 To hashLen - 1
                    hashBytes(i) = data(i + offset + usernameLength)
                Next
                Dim encryptedData(lenghtOfUsefulData - offset - usernameLength - hashLen) As Byte
                'encryptedData = copyToMax(data, offset + usernameLength + hashLen, lenghtOfUsefulData - 1)
                Array.ConstrainedCopy(data, offset + usernameLength + hashLen, encryptedData, 0, lenghtOfUsefulData - offset - usernameLength - hashLen + 1)

                Dim genHashBytes() As Byte = sha256.ComputeHash(encryptedData)
                If genHashBytes.Length > hashLen Then
                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "[ERROR] Generated hash for the Received data longer than max allowed" & vbNewLine)
                    'Not much we can do to deal with this, its a programming error
                    client.sendACK()
                    Return
                End If
                'Compare the two hashes of the encrypted data, in order to make sure there were no errors when transmitting the file
                For i = 0 To hashLen - 1
                    If hashBytes(i) <> genHashBytes(i) Then
                        MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "[ERROR] Generated hash did not match the hash Received from the client" & vbNewLine)
                        client.sendError()
                        Debug.Print("hash")
                        Return
                    End If
                Next
                'The hashes were equal so we may now send the ACK so the client sends the next set of data
                client.sendACK()
                'We need to get the recipients IP address from our lookup table so we can get the reference of the instance of MTClientReciever for that client
                Dim recpIPAddr As String = lookupTable.ResolveUsernameToIP(ASCII.GetString(nonZeroUsername))

                If recpIPAddr = Nothing Then
                    'Got to now store the message for later, that user is not currently logged on
                    If messageType = 2 Then
                        MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Writing encrypted file to /TEMP/ for user " & recpIPAddr & vbNewLine)
                        FileStream.Write(encryptedData, 0, encryptedData.Length)

                        If x = y Then
                            Dim SQL As String
                            SQL = "INSERT INTO TBL_UNSENT_FILES ([Filepath],[Sender_UserID],[Recipient_UserID]) VALUES ('" & FileStream.Name & "','" & lookupTable.GetClientUname(IPAddr) & "','" & ASCII.GetString(nonZeroUsername) & "');"
                            databaseConnections.sendSQLNonQuery(SQL)
                            FileStream.Close()
                            FileStream = Nothing
                            MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Finished writing the file out" & vbNewLine)
                        End If

                    ElseIf messageType = 1 Then
                        MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Storing message for later for the user " & recpIPAddr & vbNewLine)

                        If x = 1 And y <> 1 Then
                            ReDim totalMessage((data.Length * y) - 1)
                        ElseIf x = 1 And y = 1 Then
                            ReDim totalMessage(encryptedData.Length - 1)
                        End If
                        If y > 1 Then
                            encryptedData.CopyTo(totalMessage, encryptedData.Length * (x - 1))
                        Else
                            encryptedData.CopyTo(totalMessage, 0)
                        End If
                        If x = y Then
                            Dim SQL As String
                            SQL = "INSERT INTO TBL_UNSENT_MESSAGES ([MessageData],[Sender_UserID],[Recipient_UserID]) VALUES ('" & System.Convert.ToBase64String(totalMessage) & "','" & lookupTable.GetClientUname(IPAddr) & "','" & ASCII.GetString(nonZeroUsername) & "');"
                            databaseConnections.sendSQLNonQuery(SQL)
                        End If

                    End If
                    Return
                End If

                'Here we get that clients reference
                Dim recpientReference As MTClientReciever = lookupTable.GetClientReference(recpIPAddr)
                'Send a full frame of bytes, therefore we know the max amount to recieve upon intercept, and therefore no more TCP problems, this also makes hashing now redundant, but keeping it as a backup
                Dim newData(data.Length - 1) As Byte
                newData(0) = messageType
                newData(1) = x
                newData(2) = y
                'newData(3) = senderUsername.Length
                senderUsername.CopyTo(newData, offset)
                hashBytes.CopyTo(newData, offset + usernameLength)
                encryptedData.CopyTo(newData, offset + usernameLength + hashLen)
                recpientReference.sendData(newData)
                MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Forwarding data " & IPAddr & " >> " & recpIPAddr & vbNewLine)
        End Select
    End Sub
    Private Sub printArray(ByVal array() As Byte, ByVal charPerLine As Integer)
        Dim charactercounter As Integer
        For i = 0 To 100
            If charactercounter = charPerLine Then
                charactercounter = 0
                Debug.Write(vbNewLine)
            End If
            Debug.Write(array(i) & Space(1))
            charactercounter += 1
        Next
    End Sub
    Private Function getLastNonZeroIndex(ByVal byteArray() As Byte, Optional ByVal lastAllowedIndex As Integer = 0) As Integer
        Dim lastNonZeroIndex As Integer = byteArray.Length - 1
        Do
            lastNonZeroIndex -= 1
        Loop Until byteArray(lastNonZeroIndex) Or lastNonZeroIndex = lastAllowedIndex
        Return lastNonZeroIndex
    End Function
    Private Function copyToMax(ByVal array() As Byte, ByVal start As Integer, ByVal final As Integer) As Byte()
        '| offset | username | hash | data |
        '|    3   |    20    |  32  |   ?  |
        '0        2          22     54     ?
        Dim finalisedArray(final - start) As Byte
        Dim i As Integer
        For i = start To final
            finalisedArray(i - start) = array(i)
        Next
        Debug.Print(i)
        Return finalisedArray
    End Function
    Public Sub TcpListener_Stop()
        'Kill off everything to stop the server running
        If listenerSocket Is Nothing Then
            Return
        End If
        listenerSocket.stopListener()
        If lookupTable Is Nothing Then
            Return
        End If
        lookupTable.CloseAllClientConnections()
    End Sub
    Public Shared Function Convert_IP_DecimalNotated(ByVal IP As String) As Byte()
        '0.0.0.0
        'This converts the decimal notated ip like so above to bytes
        If IP = Nothing Then
            Form1.RTB_Log.AppendText("Provided IP was null" & vbNewLine)
            Return Nothing
        End If
        Dim s(3) As String
        Dim IPBytes(3) As Byte
        s = IP.Split(".")
        If s.Count <> 4 Then
            Form1.RTB_Log.AppendText("Tried converting IP to byte array, the number of decimals was not equal to 3" & vbNewLine)
            Return Nothing
        End If
        For i = 0 To s.Length - 1
            Try
                IPBytes(i) = System.Convert.ToByte(s(i))
            Catch ex As Exception
                Form1.RTB_Log.AppendText("Failed to convert to byte, invalid IP address" & vbNewLine)
                Return Nothing
            End Try
        Next
        Return IPBytes
    End Function
End Class
