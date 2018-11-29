'Public Sub New(ByRef clientSocket As TcpClient, ByVal username As String, ByVal publicKey() As Byte)
'    Me.client = clientSocket
'    stream = clientSocket.GetStream
'    recBuffSize = clientSocket.ReceiveBufferSize
'    lookupTable = New UserLookupTable
'    MainThreadUIUpdater.addInstance(Form1)
'    'Dim username As String = "Test2"
'    Dim byteUsername() As Byte = ASCII.GetBytes(username)
'    'Dim publicKey() As Byte = BitConverter.GetBytes(600000000000000000)
'    Dim data(byteUsername.Length + publicKey.Length + 1) As Byte
'    data(0) = CByte(byteUsername.Length)
'    For i = 1 To byteUsername.Length
'        data(i) = byteUsername(i - 1)
'    Next
'    data(byteUsername.Length + 1) = CByte(publicKey.Length)
'    For i = 0 To publicKey.Length - 1
'        data(i + byteUsername.Length + 2) = publicKey(i)
'    Next
'    Try
'        stream.Write(data, 0, data.Length)
'        stream.Flush()
'    Catch ex As Exception
'        Return
'    End Try
'    writeDebugLog("Sending preliminary data to the server", GetCurrentMethod.Name)
'    Dim clientCountByte(recBuffSize - 1) As Byte
'    stream.Read(clientCountByte, 0, recBuffSize)
'    Dim clientCount As Int64 = BitConverter.ToInt64(clientCountByte, 0)
'    writeDebugLog("Recieved currently connected users from the server with the count of: " & clientCount, GetCurrentMethod.Name)
'    For i = 0 To clientCount - 1
'        Dim message(recBuffSize - 1) As Byte
'        Dim clientUsernameByte(20) As Byte
'        Dim lastNonZeroIndex As Integer = recBuffSize - 1
'        stream.Read(message, 0, recBuffSize)
'        For c = recBuffSize - 1 To 0 Step -1
'            If message(c) <> 0 Then
'                Exit For
'            End If
'            lastNonZeroIndex -= 1
'        Next
'        Dim clientPKByte(lastNonZeroIndex - 20) As Byte
'        If Not lastNonZeroIndex > 20 Then
'            writeDebugLog("Recieved preliminary data from the server was not formatted correctly", GetCurrentMethod.Name, True)
'            Return
'        End If
'        For c = 0 To 19
'            clientUsernameByte(c) = message(c)
'        Next
'        Dim usernameLastIndex As Integer = 19
'        For c = 19 To 0 Step -1
'            If clientUsernameByte(c) <> 0 Then
'                Exit For
'            End If
'            usernameLastIndex -= 1
'        Next
'        Dim newUsername(usernameLastIndex) As Byte
'        Array.Copy(clientUsernameByte, newUsername, usernameLastIndex)
'        For c = 0 To lastNonZeroIndex - 20
'            clientPKByte(c) = message(c + 20)
'        Next
'        lookupTable.addClient(ASCII.GetString(newUsername), clientPKByte)
'        MainThreadUIUpdater.addUserToList(ASCII.GetString(newUsername))
'    Next
'    writeDebugLog("Populated the lookup table with the other users", GetCurrentMethod.Name)
'    HeartbeatTimer = New Heartbeat(stream)
'    Dim RThread As New Threading.Thread(AddressOf pollClientRecieve)
'    RThread.Start()
'End Sub
'Public Sub sendData(ByVal data() As Byte, Optional ByVal errorBytes As Boolean = False)
'    'A problem occured which didnt throw an error except just had unecessary execution
'    'If closeConnection() was called without the use of _disposed then it was possible for the user to call any send function, due to error catching it would catch the stream disposed execption and then just end up close the connection again
'    'This wasted execution time, to fix the problem we throw an ObjectDisposed exception which should notify the user that we can reset the object fine
'    If _disposed Then
'        'Throw New ObjectDisposedException(Me.ToString)
'        writeDebugLog("Object has already been disposed, reinitialise before accessing", GetCurrentMethod.Name, True)
'        Return
'    End If
'    Dim parameters As New sendingParameters
'    parameters.Data = data
'    parameters.errorBytes = errorBytes
'    Dim SThread As New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf MTsend))
'    SThread.Start(parameters)
'End Sub
'Private Sub MTsend(ByVal params As sendingParameters)
'    'keep the thread paused until the networkstream is free
'    Do
'        Threading.Thread.Sleep(1000)
'    Loop Until canWrite And canRead
'    canRead = False
'    HeartbeatTimer.Timer.Enabled = False
'    HeartbeatTimer.Timer.Stop()
'    Try
'        stream.Write(params.Data, 0, params.Data.Length)
'    Catch ex As Exception
'        closeConnection(True)
'        canRead = True
'        Return
'    End Try
'    stream.Flush()
'    If params.errorBytes Then
'        canRead = True
'        Return
'    End If
'    Dim serverResponseBytes(recBuffSize - 1) As Byte
'    Dim value As Integer
'    Try
'        stream.Read(serverResponseBytes, 0, recBuffSize)
'    Catch ex As Exception
'        canRead = True
'        Return
'    End Try
'    Try
'        value = BitConverter.ToInt32(serverResponseBytes, 0)
'    Catch ex As Exception
'        'Dont worry
'    End Try
'    If value = 61149 Then
'        writeDebugLog("Resending", GetCurrentMethod.Name, False)
'        Dim ret As Boolean = trySendAgain(params.Data)
'        If ret = False And _disposed = False And killswitch = False Then
'            closeConnection(True)
'            canRead = True
'            Return
'        ElseIf ret = False And _disposed = True Or killswitch = True Then
'            canRead = True
'            Return
'        End If
'    ElseIf Not value = 43724 Then
'        'Throw New Exception
'        RaiseEvent EdataRecieved(serverResponseBytes)
'        'trySendAgain(params.Data)
'    End If
'    writeDebugLog("ACK", GetCurrentMethod.Name, False)
'    HeartbeatTimer.Timer.Start()
'    HeartbeatTimer.Timer.Enabled = True
'    canRead = True
'End Sub