Imports System.Net.Sockets
Imports System.Text.Encoding
Imports System.IO
Imports Error_Handler.Events_Handler
Imports System.Reflection.MethodInfo
Imports System.Security.Cryptography
Imports Curve25519_ECDH.Curve
Imports Curve25519_ECDH.Crypto
Imports Curve25519_ECDH
Imports System.Numerics
Public Class SendRecieve
    Private stream As NetworkStream
    Private canWrite As Boolean = True
    Private canRead As Boolean = True
    Private Ended As Boolean
    Private WithEvents HeartbeatTimer As Heartbeat
    Private recBuffSize As Integer
    Private client As TcpClient
    Private _disposed As Boolean = False
    Private lookupTable As UserLookupTable
    Private killswitch As Boolean = False
    Private fileStream As FileStream
    Private linearMessageQueue As New Queue
    Public databaseConnection As DatabaseTools
    Public Event recievedPopulateValues(ByVal data() As Byte)
    Private totalMessage() As Byte
    Private localKeys As New Keys
    Private ECIES_AES As New ECIES
    Private curve As New Weierstrass_Curve("Curves.xml", "Brain-P256r1")
    Const IVBytesLen As Integer = 16
    Const MACBytesLen As Integer = 16
    Private Structure sendingParameters
        Public Data() As Byte
        Public errorBytes As Boolean
    End Structure
    Public Enum sendingErrorEnum
        RESEND = 0
        NOT_ADMIN = 1
        PASSWORD_MISMATCH = 2
        USER_EXISTS = 3
        USER_DOES_NOT_EXIST = 4
    End Enum
    Public Sub New(ByRef clientSocket As TcpClient, ByVal username As String, ByVal passwordHash() As Byte)
        If Not clientSocket.Connected Then
            Throw New Exception("The client is no longer connected")
            Return
        End If
        Me.client = clientSocket
        stream = clientSocket.GetStream
        recBuffSize = clientSocket.ReceiveBufferSize
        lookupTable = New UserLookupTable
        MainThreadUIUpdater.addInstance(FRM_Messages)
        localKeys = ECDH.generate_Keys(curve.Parameters)
        Dim publicKeyx() As Byte = Curve25519_ECDH.Crypto.AES.Convert_BigInteger_ToByteArray(localKeys.PublicKey.x)
        Dim publicKeyy() As Byte = Curve25519_ECDH.Crypto.AES.Convert_BigInteger_ToByteArray(localKeys.PublicKey.y)
        Dim byteUsername() As Byte = ASCII.GetBytes(username)
        Dim data(client.ReceiveBufferSize - 1) As Byte
        byteUsername.CopyTo(data, 0)
        data(20) = publicKeyx.Length
        data(21) = publicKeyy.Length
        publicKeyx.CopyTo(data, 22)
        publicKeyy.CopyTo(data, 22 + publicKeyx.Length)
        passwordHash.CopyTo(data, 22 + publicKeyx.Length + publicKeyy.Length)
        Try
            stream.Write(data, 0, data.Length)
            stream.Flush()
        Catch ex As Exception
            errorHappened(stream, ex.Message, errorEnum.STREAM_EXCEPTION, GetCurrentMethod.Name)
            Throw New Exception
        End Try
        writeDebugLog("Sending preliminary data to the server", GetCurrentMethod.Name)

        Dim authentication(1) As Byte
        stream.Read(authentication, 0, 1)
        Select Case CInt(authentication(0))
            Case Is = 1
                'Error in authentication
                errorHappened(authentication, "Either the username or the password was incorrect", errorEnum.FAILED_SERVER_AUTH, GetCurrentMethod.Name)
                Throw New Exception
            Case Is = 2
                'Already connected
                errorHappened(authentication, "A client is already connected with the provided details", errorEnum.CLIENT_ALREADY_CONNECTED, GetCurrentMethod.Name)
                Throw New Exception
            Case Is = 3
                'Normal
            Case Is = 4
                'Admin
        End Select
        databaseConnection = New DatabaseTools(DatabaseTools.databaseEnum.ClientDatabase, username)
        Dim clientCountByte(3) As Byte
        Try
            stream.Read(clientCountByte, 0, 4)
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
            Throw New Exception
        End Try
        Dim clientCount As Int32 = BitConverter.ToInt32(clientCountByte, 0)
        writeDebugLog("Recieved currently connected users from the server with the count of: " & clientCount, GetCurrentMethod.Name)
        '| username | connected | x len | y len |public key x | public key y |
        For i = 0 To clientCount - 1
            Dim message(recBuffSize - 1) As Byte
            Dim clientUsernameByte(19) As Byte
            Dim lastNonZeroIndex As Integer = recBuffSize - 1
            stream.Read(message, 0, recBuffSize)
            lastNonZeroIndex = getLastNonZeroIndex(message)
            If lastNonZeroIndex - 19 <= 0 Then
                writeDebugLog("Recieved preliminary data from the server was not formatted correctly", GetCurrentMethod.Name, True)
                Throw New Exception
            End If
            If Not lastNonZeroIndex > 20 Then
                writeDebugLog("Recieved preliminary data from the server was not formatted correctly", GetCurrentMethod.Name, True)
                Throw New Exception
            End If
            For c = 0 To 19
                clientUsernameByte(c) = message(c)
            Next
            Dim usernameLastIndex As Integer = getLastNonZeroIndex(clientUsernameByte)
            Dim newUsername(usernameLastIndex) As Byte
            Array.Copy(clientUsernameByte, newUsername, usernameLastIndex + 1)
            Dim pkxLen As Integer = message(21)
            Dim pkylen As Integer = message(22)
            Dim pkx(pkxLen - 1) As Byte
            Dim pky(pkylen - 1) As Byte
            Array.ConstrainedCopy(message, 23, pkx, 0, pkxLen)
            Array.ConstrainedCopy(message, 23 + pkxLen, pky, 0, pkylen)
            Dim online As Boolean = CBool(message(20))
            Dim remotePK As New ECPoint(Curve25519_ECDH.Crypto.AES.Convert_IntegerByteArray_ToBigInteger(pkx), Curve25519_ECDH.Crypto.AES.Convert_IntegerByteArray_ToBigInteger(pky), curve.Parameters.Fp.p)
            lookupTable.addClient(ASCII.GetString(newUsername), remotePK)
            MainThreadUIUpdater.addUserToList(ASCII.GetString(newUsername), CInt(message(20)))
            Dim SQL As String
            SQL = "SELECT * FROM TBL_USERS WHERE [Username] = '" & ASCII.GetString(newUsername) & "';"
            Dim reader As OleDb.OleDbDataReader = databaseConnection.sendSQLQuery(SQL)
            reader.Read()
            If Not reader.HasRows Then
                SQL = "INSERT INTO TBL_USERS VALUES ('" & ASCII.GetString(newUsername) & "');"
                databaseConnection.sendSQLNonQuery(SQL)
            End If
        Next
        recieveUnsentFiles(stream, username)
        RegistryTools.addValueToRegistry(System.Convert.ToBase64String(Curve25519_ECDH.Crypto.AES.Convert_BigInteger_ToByteArray(localKeys.PrivateKey)), username)
        writeDebugLog("Populated the lookup table with the other users", GetCurrentMethod.Name)
        HeartbeatTimer = New Heartbeat(stream)
        Dim RThread As New Threading.Thread(AddressOf pollClientRecieve)
        RThread.Start()
        Dim SThread As New Threading.Thread(AddressOf MTsend)
        SThread.Start()
    End Sub
    Private Sub recieveUnsentFiles(ByVal stream As NetworkStream, ByVal username As String)
        Dim packetsExpected As Integer
        Dim packetsExpectedBytes(3) As Byte
        Dim readLength As Integer
        readLength = stream.Read(packetsExpectedBytes, 0, 4)
        If readLength <> 4 Then
            Dim currentReadLength As Integer = readLength
            Do
                readLength = stream.Read(packetsExpectedBytes, currentReadLength, 3 - currentReadLength)
                currentReadLength += readLength
            Loop Until currentReadLength = 3
        End If
        packetsExpected = BitConverter.ToInt32(packetsExpectedBytes, 0)

        For i = 0 To packetsExpected - 1
            Dim neededData(24) As Byte
            readLength = stream.Read(neededData, 0, 25)
            If readLength <> 25 Then
                Dim currentReadLength As Integer = readLength
                Do
                    readLength = stream.Read(neededData, currentReadLength, 25 - currentReadLength)
                    currentReadLength += readLength
                Loop Until currentReadLength = 25
            End If
            Dim usernameByteZeros(19) As Byte
            Array.ConstrainedCopy(neededData, 1, usernameByteZeros, 0, 20)
            Dim lastNonZeroUsernameIndex As Integer = getLastNonZeroIndex(usernameByteZeros)
            Dim usernameBytes(lastNonZeroUsernameIndex) As Byte
            Array.Copy(usernameByteZeros, usernameBytes, lastNonZeroUsernameIndex + 1)
            Dim dataLen As Integer = BitConverter.ToInt32(neededData, 21)
            Dim data(dataLen - 1) As Byte
            Dim filenameBytes(99) As Byte
            Dim filename As String
            If neededData(0) = 2 Then
                readLength = stream.Read(filenameBytes, 0, 100)
                If readLength <> 100 Then
                    Dim currentReadLength As Integer = readLength
                    Do
                        readLength = stream.Read(filenameBytes, currentReadLength, 100 - currentReadLength)
                        currentReadLength += readLength
                    Loop Until currentReadLength = 100
                End If
                Dim lastIndex As Integer = getLastNonZeroIndex(filenameBytes)
                Dim filenameNonZero(lastIndex) As Byte
                Array.Copy(filenameBytes, filenameNonZero, lastIndex + 1)
                filename = ASCII.GetString(filenameNonZero)
            End If
            readLength = stream.Read(data, 0, dataLen)
            If readLength <> dataLen Then
                Dim currentReadLength As Integer = readLength
                Do
                    readLength = stream.Read(data, currentReadLength, dataLen - currentReadLength)
                    currentReadLength += readLength
                Loop Until currentReadLength = dataLen
            End If

            Dim ECIES_AES As New ECIES
            Dim CBC_AES As New ECIES.TEncryption
            Dim MAC(MACBytesLen - 1) As Byte
            Dim IV(IVBytesLen - 1) As Byte
            Dim tmpData(data.Length - MACBytesLen - IVBytesLen - 1) As Byte
            Dim decryptedData() As Byte
            Dim privateKey As String = RegistryTools.getRegistryValue(username)
            If privateKey = "NULL" Then
                writeDebugLog("Could not find privatekey in the registry no messages could have been sent because we had no details", GetCurrentMethod.Name)
                Return
            End If
            Dim lastLocalKeys As Keys = ECDH.regeneratePublicKey(curve.Parameters, Curve25519_ECDH.Crypto.AES.Convert_IntegerByteArray_ToBigInteger(System.Convert.FromBase64String(privateKey)))
            Array.ConstrainedCopy(data, data.Length - MACBytesLen, MAC, 0, MACBytesLen)
            Array.ConstrainedCopy(data, data.Length - MACBytesLen - IVBytesLen, IV, 0, IVBytesLen)
            Array.ConstrainedCopy(data, 0, tmpData, 0, data.Length - MACBytesLen - IVBytesLen)
            CBC_AES.HMAC = MAC
            CBC_AES.IV = IV
            CBC_AES.Data = tmpData
            decryptedData = ECIES_AES.Decrypt_AES_256(CBC_AES, lastLocalKeys, curve, lookupTable.getClientsPK(ASCII.GetString(usernameBytes)))
            If decryptedData IsNot Nothing Then
                If neededData(0) = 1 Then

                    Dim SQL As String
                    SQL = "INSERT INTO TBL_MESSAGES ([MessageData],[ReceivedDate],[IsFile],[Sender_UserID]) VALUES ('" & ASCII.GetString(decryptedData) & "','" & DateTime.Today.Date.ToShortDateString & "',False,'" & ASCII.GetString(usernameBytes) & "');"
                    databaseConnection.sendSQLNonQuery(SQL)
                    MainThreadUIUpdater.checkAndAdd(ASCII.GetString(usernameBytes), ASCII.GetString(data))

                ElseIf neededData(0) = 2 Then

                    MainThreadUIUpdater.checkAndAdd(ASCII.GetString(usernameBytes), "The user sent you a file: " & filename, True)
                    Dim SQL As String
                    SQL = "INSERT INTO TBL_FILES ([Filename],[Sender_UserID]) VALUES ('" & filename & "','" & ASCII.GetString(usernameBytes) & "');"
                    databaseConnection.sendSQLNonQuery(SQL)
                    SQL = "SELECT COUNT([FileID]) FROM TBL_FILES WHERE [Filename] = '" & filename & "' AND [Sender_UserID] = '" & ASCII.GetString(usernameBytes) & "';"
                    Dim reader As OleDb.OleDbDataReader = databaseConnection.sendSQLQuery(SQL)
                    reader.Read()
                    Dim countOfFiles As Integer = reader.GetInt32(0)
                    reader.Close()
                    SQL = "SELECT [FileID] FROM TBL_FILES WHERE [Filename] = '" & filename & "' AND [Sender_UserID] = '" & ASCII.GetString(usernameBytes) & "';"
                    reader = databaseConnection.sendSQLQuery(SQL)
                    For c = 0 To countOfFiles - 1
                        reader.Read()
                    Next
                    Dim fileID As Integer = reader.GetInt32(0)
                    reader.Close()
                    SQL = "INSERT INTO TBL_MESSAGES ([ReceivedDate],[IsFile],[Received_FileID],[Sender_UserID]) VALUES ('" & DateTime.Today.Date.ToShortDateString & "',True," & fileID & ",'" & ASCII.GetString(usernameBytes) & "');"
                    databaseConnection.sendSQLNonQuery(SQL)
                    Dim fs As New FileStream(FileIO.SpecialDirectories.MyDocuments & "/IM Application/" & ASCII.GetString(usernameBytes) & "/" & filename, FileMode.Create)
                    fs.Write(decryptedData, 0, decryptedData.Length)
                    fs.Close()

                End If
            End If
        Next

    End Sub
    Private Sub pollClientRecieve()
        Do
            If stream.DataAvailable And canRead Then
                HeartbeatTimer.Timer.Stop()
                dataRecieved(Nothing)
                HeartbeatTimer.Timer.Start()
            End If
            Threading.Thread.Sleep(1000)
        Loop Until Ended
    End Sub
    Private Sub dataRecieved(ByVal data() As Byte)
        If _disposed Then
            'Throw New ObjectDisposedException(Me.ToString)
            writeDebugLog("Object has already been disposed, reinitialise before accessing", GetCurrentMethod.Name, True)
            Return
        End If
        Dim recvBytes(recBuffSize - 1) As Byte
        Dim identifier As Integer
        Dim recvLength As Integer
        'Initiate a networkstream lock so that writer cannot access it while the stream is being read
        canWrite = False
        If data Is Nothing Then
            recvLength = stream.Read(recvBytes, 0, recBuffSize)
            If (recvBytes(0) = 1 Or recvBytes(0) = 2 Or recvBytes(0) = 206) And recvLength <> recBuffSize Then
                Dim dataCounter As Integer = recvLength
                Do
                    writeDebugLog("Got " & dataCounter & " out of " & recBuffSize, GetCurrentMethod.Name)
                    recvLength = stream.Read(recvBytes, dataCounter, recBuffSize - dataCounter)
                    dataCounter += recvLength
                Loop Until dataCounter = recBuffSize
            End If
            writeDebugLog("All data obtained", GetCurrentMethod.Name)
        Else
            recvBytes = data
        End If

        'Try and catch any chance or recieveing ACK or ERROR values from the server
        identifier = convertSetAmountToInt32(recvBytes, 2)
        writeDebugLog("Recieved data from the server", GetCurrentMethod.Name)
        If identifier = 43724 Or identifier = 61149 Then
            canWrite = True
            Return
        End If

        'Check for user connected or disconnected
        identifier = CInt(recvBytes(0))
        Select Case identifier
            Case Is = 220
                Dim usernameByte(20) As Byte
                Dim lastNonZeroIndex As Integer = 19
                For i = 0 To 19
                    usernameByte(i) = recvBytes(i + 1)
                Next
                lastNonZeroIndex = getLastNonZeroIndex(usernameByte)
                If lastNonZeroIndex = 0 Then
                    sendError()
                    canWrite = True
                    Return
                End If
                Dim correctUsername(lastNonZeroIndex) As Byte
                Array.Copy(usernameByte, correctUsername, lastNonZeroIndex + 1)
                'lookupTable.removeClient(ASCII.GetString(correctUsername))
                'MainThreadUIUpdater.removeUserFromList(ASCII.GetString(correctUsername))
                MainThreadUIUpdater.changeListIcon(ASCII.GetString(correctUsername), 0)
                writeDebugLog("Another user disconnected: " & ASCII.GetString(correctUsername), GetCurrentMethod.Name)
                sendACK()
                canWrite = True
                Return
            Case Is = 204
                '| message type | username | x len | y len | pkx | pky |
                Dim usernameByte(20) As Byte
                Dim lastNonZeroIndex As Integer = 19
                For i = 0 To 19
                    usernameByte(i) = recvBytes(i + 1)
                Next
                lastNonZeroIndex = getLastNonZeroIndex(usernameByte)
                If lastNonZeroIndex = 0 Then
                    sendError()
                    canWrite = True
                    Return
                End If
                Dim correctUsername(lastNonZeroIndex) As Byte
                Array.Copy(usernameByte, correctUsername, lastNonZeroIndex + 1)
                Dim pkxlen As Integer = recvBytes(21)
                Dim pkylen As Integer = recvBytes(22)
                Dim pkx(pkxlen - 1) As Byte
                Dim pky(pkylen - 1) As Byte
                Array.ConstrainedCopy(recvBytes, 23, pkx, 0, pkxlen)
                Array.ConstrainedCopy(recvBytes, 23 + pkxlen, pky, 0, pkylen)
                Dim remotePK As New ECPoint(Curve25519_ECDH.Crypto.AES.Convert_IntegerByteArray_ToBigInteger(pkx), Curve25519_ECDH.Crypto.AES.Convert_IntegerByteArray_ToBigInteger(pky), curve.Parameters.Fp.p)
                lookupTable.updateClientsPK(ASCII.GetString(correctUsername), remotePK)
                MainThreadUIUpdater.changeListIcon(ASCII.GetString(correctUsername), 1)
                writeDebugLog("Another user connected: " & ASCII.GetString(correctUsername), GetCurrentMethod.Name)
                sendACK()
                canWrite = True
                Return
            Case Is = 206
                sendACK()
                RaiseEvent recievedPopulateValues(recvBytes)
                canWrite = True
                Return
        End Select
        '| Message Type | x | of y | username | data hash | data |
        '|     Byte     | B |   B  | 20 bytes | 32 bytes  | Bytes|
        If recvBytes(0) <> 1 And recvBytes(0) <> 2 Then
            writeDebugLog("Recieved a packet with message type not equal to the know values", GetCurrentMethod.Name, True)
            canWrite = True
            Return
        End If
        Const offset As Integer = 3
        Const usernameLen As Integer = 20
        Const hashLen As Integer = 32
        Dim SHA256 As New SHA256Managed()
        Dim messageType As Byte = recvBytes(0)
        Dim x As Byte = recvBytes(1)
        Dim y As Byte = recvBytes(2)
        Dim lengthOfUsefulData As Integer = recBuffSize - 1
        If x = 0 Then
            writeDebugLog("Got filename", GetCurrentMethod.Name)
            Dim usernameByte(usernameLen - 1) As Byte
            For i = 0 To usernameLen - 1
                usernameByte(i) = recvBytes(i + offset)
            Next
            Dim lastNonZeroIndex As Integer = getLastNonZeroIndex(usernameByte)
            Dim finalUsername(lastNonZeroIndex) As Byte
            'Cut off the zero bytes by copying the userful data into a buffer
            Array.Copy(usernameByte, finalUsername, lastNonZeroIndex + 1)
            Dim tmpFileName(99) As Byte
            For i = 0 To 99
                tmpFileName(i) = recvBytes(i + offset + usernameLen)
            Next
            Dim fileNameLen As Integer = getLastNonZeroIndex(tmpFileName)
            Dim fileName(fileNameLen) As Byte
            If fileName.Length = 0 Then
                sendError()
                canWrite = True
                Return
            End If
            Array.Copy(tmpFileName, fileName, fileNameLen + 1)
            If Not Directory.Exists(FileIO.SpecialDirectories.MyDocuments & "/IM Application/" & ASCII.GetString(finalUsername)) Then
                Try
                    Directory.CreateDirectory(FileIO.SpecialDirectories.MyDocuments & "/IM Application/" & ASCII.GetString(finalUsername))
                Catch sysex As System.ArgumentException
                    writeDebugLog(sysex.Message, GetCurrentMethod.Name, True)
                    sendError()
                    canWrite = True
                    Return
                End Try
            End If
            'Open the filestream for the next bytes we are ready to recieve
            Try
                fileStream = New FileStream(FileIO.SpecialDirectories.MyDocuments & "/IM Application/" & ASCII.GetString(finalUsername) & "/" & ASCII.GetString(fileName), FileMode.Create)
            Catch ex As Exception
                writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
            End Try
            sendACK()
            canWrite = True
            Return
        End If
        writeDebugLog("Packet" & x & " of " & y, GetCurrentMethod.Name)
        If x = y Then
            lengthOfUsefulData = getLastNonZeroIndex(recvBytes, usernameLen + 3)
        End If
        Dim usernameBytes(usernameLen - 1) As Byte
        For i = 0 To usernameLen - 1
            usernameBytes(i) = recvBytes(i + offset)
        Next
        Dim lastNonZeroUnameIndex As Integer = getLastNonZeroIndex(usernameBytes)
        'find the last index in the username array that has a byte that is != 0
        Dim nonZeroUsername(lastNonZeroUnameIndex) As Byte
        'Cut off the zero bytes by copying the userful data into a buffer
        Array.Copy(usernameBytes, nonZeroUsername, lastNonZeroUnameIndex + 1)
        Dim hashBytes(hashLen - 1) As Byte
        For i = 0 To hashLen - 1
            hashBytes(i) = recvBytes(i + offset + usernameLen)
        Next
        Dim encryptedData(lengthOfUsefulData - offset - usernameLen - hashLen) As Byte
        encryptedData = copyToMax(recvBytes, offset + usernameLen + hashLen, lengthOfUsefulData)
        Dim genHashBytes() As Byte = SHA256.ComputeHash(encryptedData)
        If genHashBytes.Length > hashLen Then
            'Not much we can do to deal with this, its a programming error
            writeDebugLog("Hash length longer than allowed", GetCurrentMethod.Name)
            sendACK()
            canWrite = True
            Return
        ElseIf genHashBytes.Length < hashLen Then
            writeDebugLog("Hash lenght shorter than allowed", GetCurrentMethod.Name)
            sendError()
            canWrite = True
            Return
        End If
        'Compare the two hashes of the encrypted data, in order to make sure there were no errors when transmitting the file
        For i = 0 To hashLen - 1
            If hashBytes(i) <> genHashBytes(i) Then
                writeDebugLog("Hash mismatch", GetCurrentMethod.Name)
                sendError()
                canWrite = True
                Return
            End If
        Next
        'The hashes were equal so we may now send the ACK so the client sends the next set of data
        sendACK()
        'Now we can take this data we have here and update the UI, save files, and such
        If x = 1 And y <> 1 Then
            ReDim totalMessage((recvBytes.Length * y) - 1)
        ElseIf x = 1 And y = 1 Then
            ReDim totalMessage(encryptedData.Length - 1)
        End If
        If y > 1 Then
            encryptedData.CopyTo(totalMessage, (recvBytes.Length - offset - usernameLen - hashLen) * (x - 1))
        Else
            encryptedData.CopyTo(totalMessage, 0)
        End If
        If messageType = 2 Then
            writeDebugLog("Writing out data to filestream", GetCurrentMethod.Name)
            If x = y Then
                Dim totalMessageLastNonZero As Integer = getLastNonZeroIndex(totalMessage)
                Dim message(totalMessageLastNonZero) As Byte
                Array.Copy(totalMessage, message, totalMessageLastNonZero + 1)
                Dim ECIES_AES As New ECIES
                Dim CBC_AES As New ECIES.TEncryption
                Dim MAC(MACBytesLen - 1) As Byte
                Dim IV(IVBytesLen - 1) As Byte
                Dim tmpData(message.Length - MACBytesLen - IVBytesLen - 1) As Byte
                Dim decrypteddata() As Byte
                Array.ConstrainedCopy(message, message.Length - MACBytesLen, MAC, 0, MACBytesLen)
                Array.ConstrainedCopy(message, message.Length - MACBytesLen - IVBytesLen, IV, 0, IVBytesLen)
                Array.ConstrainedCopy(message, 0, tmpData, 0, message.Length - MACBytesLen - IVBytesLen)
                CBC_AES.HMAC = MAC
                CBC_AES.IV = IV
                CBC_AES.Data = tmpData
                decrypteddata = ECIES_AES.Decrypt_AES_256(CBC_AES, localKeys, curve, lookupTable.getClientsPK(ASCII.GetString(nonZeroUsername)))
                fileStream.Write(decrypteddata, 0, decrypteddata.Length)
                Dim SQL As String
                SQL = "INSERT INTO TBL_FILES ([Filename],[Sender_UserID]) VALUES ('" & Path.GetFileName(fileStream.Name) & "','" & ASCII.GetString(nonZeroUsername) & "');"
                databaseConnection.sendSQLNonQuery(SQL)
                SQL = "SELECT COUNT([FileID]) FROM TBL_FILES WHERE [Filename] = '" & Path.GetFileName(fileStream.Name) & "' AND [Sender_UserID] = '" & ASCII.GetString(nonZeroUsername) & "';"
                Dim reader As OleDb.OleDbDataReader = databaseConnection.sendSQLQuery(SQL)
                reader.Read()
                Dim countOfFiles As Integer = reader.GetInt32(0)
                reader.Close()
                SQL = "SELECT [FileID] FROM TBL_FILES WHERE [Filename] = '" & Path.GetFileName(fileStream.Name) & "' AND [Sender_UserID] = '" & ASCII.GetString(nonZeroUsername) & "';"
                reader = databaseConnection.sendSQLQuery(SQL)
                For c = 0 To countOfFiles - 1
                    reader.Read()
                Next
                Dim fileID As Integer = reader.GetInt32(0)
                reader.Close()
                SQL = "INSERT INTO TBL_MESSAGES ([ReceivedDate],[IsFile],[Received_FileID],[Sender_UserID]) VALUES ('" & DateTime.Today.Date.ToShortDateString & "',True," & fileID & ",'" & ASCII.GetString(nonZeroUsername) & "');"
                databaseConnection.sendSQLNonQuery(SQL)
                MainThreadUIUpdater.checkAndAdd(ASCII.GetString(nonZeroUsername), "The user sent you a file: " & Path.GetFileName(fileStream.Name), True)
                writeDebugLog("Closing filestream", GetCurrentMethod.Name)
                fileStream.Close()
                fileStream = Nothing
            End If
        Else
            writeDebugLog("Collecting the message", GetCurrentMethod.Name)
            If x = y Then
                Dim totalMessageLastNonZero As Integer = getLastNonZeroIndex(totalMessage)
                Dim message(totalMessageLastNonZero) As Byte
                Array.Copy(totalMessage, message, totalMessageLastNonZero + 1)
                Dim ECIES_AES As New ECIES
                Dim CBC_AES As New ECIES.TEncryption
                Dim MAC(MACBytesLen - 1) As Byte
                Dim IV(IVBytesLen - 1) As Byte
                Dim tmpData(message.Length - MACBytesLen - IVBytesLen - 1) As Byte
                Dim decrypteddata() As Byte
                Array.ConstrainedCopy(message, message.Length - MACBytesLen, MAC, 0, MACBytesLen)
                Array.ConstrainedCopy(message, message.Length - MACBytesLen - IVBytesLen, IV, 0, IVBytesLen)
                Array.ConstrainedCopy(message, 0, tmpData, 0, message.Length - MACBytesLen - IVBytesLen)
                CBC_AES.HMAC = MAC
                CBC_AES.IV = IV
                CBC_AES.Data = tmpData
                decrypteddata = ECIES_AES.Decrypt_AES_256(CBC_AES, localKeys, curve, lookupTable.getClientsPK(ASCII.GetString(nonZeroUsername)))
                Dim SQL As String
                SQL = "INSERT INTO TBL_MESSAGES ([MessageData],[ReceivedDate],[IsFile],[Sender_UserID]) VALUES ('" & ASCII.GetString(decrypteddata) & "','" & DateTime.Today.Date.ToShortDateString & "',False,'" & ASCII.GetString(nonZeroUsername) & "');"
                databaseConnection.sendSQLNonQuery(SQL)
                MainThreadUIUpdater.checkAndAdd(ASCII.GetString(nonZeroUsername), ASCII.GetString(decrypteddata))
            End If
        End If
        canWrite = True  'allow the writer to now access the stream
    End Sub
    Private Function copyToMax(ByVal array() As Byte, ByVal start As Integer, ByVal final As Integer) As Byte()
        Dim finalisedArray(final - start) As Byte
        For i = start To final
            finalisedArray(i - start) = array(i)
        Next
        Return finalisedArray
    End Function
    Public Sub sendData(ByVal data() As Byte, Optional ByVal errorBytes As Boolean = False)
        'A problem occured which didnt throw an error except just had unecessary execution
        'If closeConnection() was called without the use of _disposed then it was possible for the user to call any send function, due to error catching it would catch the stream disposed execption and then just end up close the connection again
        'This wasted execution time, to fix the problem we throw an ObjectDisposed exception which should notify the user that we can reset the object fine
        If _disposed Then
            'Throw New ObjectDisposedException(Me.ToString)
            writeDebugLog("Object has already been disposed, reinitialise before accessing", GetCurrentMethod.Name, True)
            Return
        End If
        writeDebugLog("Adding new data for sending", GetCurrentMethod.Name)
        Dim parameters As New sendingParameters
        parameters.Data = data
        parameters.errorBytes = errorBytes
        linearMessageQueue.Enqueue(parameters)
    End Sub
    Private Sub MTsend()
        'keep the thread paused until the networkstream is free
        'Do
        '    Threading.Thread.Sleep(1000)
        'Loop Until canWrite And canRead
        Do
            If linearMessageQueue.Count > 0 And canWrite Then
                Dim params As sendingParameters = linearMessageQueue.Dequeue
                canRead = False
                HeartbeatTimer.Timer.Stop()
                Try
                    stream.Write(params.Data, 0, params.Data.Length)
                Catch ex As Exception
                    writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
                    closeConnection(True)
                    canRead = True
                    Return
                End Try
                stream.Flush()
                If Not params.errorBytes Then
                    Dim serverResponseBytes(recBuffSize - 1) As Byte
                    Dim value As Integer
                    Try
                        stream.Read(serverResponseBytes, 0, recBuffSize)
                    Catch ex As Exception
                        writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
                        canRead = True
                        Return
                    End Try
                    Try
                        value = convertSetAmountToInt32(serverResponseBytes, 2)
                    Catch ex As Exception
                        'Dont worry
                    End Try
                    Dim serverResponseEnum As sendingErrorEnum = CType(serverResponseBytes(3), sendingErrorEnum)
                    If value = 61149 And serverResponseEnum = sendingErrorEnum.RESEND Then
                        writeDebugLog("Resending", GetCurrentMethod.Name, False)
                        Dim ret As Boolean = trySendAgain(params.Data)
                        If ret = False And _disposed = False And killswitch = False Then
                            closeConnection(True)
                            canRead = True
                            Return
                        ElseIf ret = False And _disposed = True Or killswitch = True Then
                            canRead = True
                            Return
                        End If
                    ElseIf value = 61149 And serverResponseEnum <> sendingErrorEnum.RESEND Then
                        errorHappened(serverResponseBytes, "Recieved message from server: " & serverResponseEnum.ToString, errorEnum.VALUE_CONDITIONS_NOT_MET, GetCurrentMethod.Name)
                    ElseIf Not value = 43724 Then
                        writeDebugLog("Recieved a message from the server we wasnt expecting", GetCurrentMethod.Name, True)
                    End If
                    writeDebugLog("ACK", GetCurrentMethod.Name, False)

                End If
                HeartbeatTimer.Timer.Start()
                canRead = True
            End If
            Threading.Thread.Sleep(1000)
        Loop Until Ended
    End Sub
    Private Function trySendAgain(ByVal data() As Byte) As Boolean
        If _disposed Then
            'Throw New ObjectDisposedException(Me.ToString)
            writeDebugLog("Object has already been disposed, reinitialise before accessing", GetCurrentMethod.Name, True)
            Return False
        End If
        'Debug.Print(data(recBuffSize - 1))
        If killswitch Then
            Return False
        End If
        writeDebugLog("Sending Again", GetCurrentMethod.Name)
        Try
            stream.Write(data, 0, data.Length)
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
            Return False
        End Try
        stream.Flush()
        Dim serverResponseBytes(recBuffSize - 1) As Byte
        Dim value As Integer
        Try
            stream.Read(serverResponseBytes, 0, recBuffSize)
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
            Return False
        End Try

        'Get the value returned as the enum
        value = convertSetAmountToInt32(serverResponseBytes, 2)
        Dim serverResponseEnum As sendingErrorEnum = CType(serverResponseBytes(3), sendingErrorEnum)
        If value = 61149 And serverResponseEnum = sendingErrorEnum.RESEND Then
            writeDebugLog("Resending", GetCurrentMethod.Name, False)
            If trySendAgain(data) = False Then
                Return False
            End If
        ElseIf value = 61149 And serverResponseEnum <> sendingErrorEnum.RESEND Then
            errorHappened(serverResponseBytes, "Recieved message from server: " & serverResponseEnum.ToString, errorEnum.VALUE_CONDITIONS_NOT_MET, GetCurrentMethod.Name)
        ElseIf Not value = 43724 Then
            writeDebugLog("Recieved a message from the server we wasnt expecting", GetCurrentMethod.Name, True)
        End If

        Return True
    End Function
    Public Function getLastNonZeroIndex(ByVal byteArray() As Byte, Optional ByVal lastAllowedIndex As Integer = 0) As Integer
        Dim lastNonZeroIndex As Integer = byteArray.Length - 1
        Do Until byteArray(lastNonZeroIndex) Or lastNonZeroIndex = lastAllowedIndex
            lastNonZeroIndex -= 1
        Loop
        Return lastNonZeroIndex
    End Function
    Private Function convertSetAmountToInt32(ByVal byteArray() As Byte, ByVal count As Integer) As Integer
        If count > 4 Then
            Return Nothing
        End If
        Dim tmpArray(4) As Byte
        Array.Copy(byteArray, tmpArray, count)
        Return BitConverter.ToInt32(tmpArray, 0)
    End Function
    Public Sub sendError()
        writeDebugLog("Sent Error", GetCurrentMethod.Name)
        Dim value As Integer = 61149
        Dim errorByte() As Byte = BitConverter.GetBytes(value)
        sendData(errorByte, True)
    End Sub
    Public Sub sendACK()
        writeDebugLog("Sent ACK", GetCurrentMethod.Name)
        Dim value As Integer = 43724
        Dim errorByte() As Byte = BitConverter.GetBytes(value)
        sendData(errorByte, True)
    End Sub
    Public Sub closeConnection(Optional serverDead As Boolean = False) Handles HeartbeatTimer.IOException
        If _disposed Then
            Return
        End If
        writeDebugLog("Closing connection to the server", GetCurrentMethod.Name)
        killswitch = True
        Dim data() As Byte = BitConverter.GetBytes(57005)
        If serverDead = False Then

            sendData(data)
            Do
                Threading.Thread.Sleep(3000)
            Loop Until canRead
        End If
        Ended = True
        HeartbeatTimer.Timer.Dispose()
        client.Close()
        stream.Dispose()
        _disposed = True
        writeDebugLog("Disconnected from the server", GetCurrentMethod.Name)
    End Sub
    Public Sub sendMessage(ByVal Username As String, ByVal Message As String, ByVal localUsername As String)
        'A problem occured which didnt throw an error except just had unecessary execution
        'If closeConnection() was called without the use of _disposed then it was possible for the user to call any send function, due to error catching it would catch the stream disposed execption and then just end up close the connection again
        'This wasted execution time, to fix the problem we throw an ObjectDisposed exception which should notify the user that we can reset the object fine
        If _disposed Then
            'Throw New ObjectDisposedException(Me.ToString)
            writeDebugLog("Object has already been disposed, reinitialise before accessing", GetCurrentMethod.Name, True)
            Return
        End If
        'The design of the messaging system should be [DEPRECIATED]:
        '[DEPRECIATED]| MessageType | Username Lenght | Username | Data |
        '[DEPRECIATED]|    Byte     |       Byte      |   Bytes  | Bytes|

        'Had to set the username to a max byte lenght of 20, due to the fact that when sending data split into 6500 byte chunks when the server changes the username theres a posibility we loose data
        '[DEPRECIATED]| MessageType | x | of y |   Username   | Data |
        '[DEPRECIATED]|    Byte     | B | Byte |Bytes (max 20)| Bytes|

        'Had to add a hash of the data in case of incorrectly sent messages
        '| Message Type | x | of y | username | data hash | data |
        '|     Byte     | B |   B  | 20 bytes | 32 bytes  | Bytes|
        If ASCII.GetByteCount(Username) > 20 Then
            writeDebugLog("Username longer than the maximum allowed of 20 bytes", GetCurrentMethod.Name, True)
            Return
        End If
        Dim UsernameBytes(19) As Byte
        Array.Copy(ASCII.GetBytes(Username), UsernameBytes, ASCII.GetByteCount(Username))
        Const usernameLength As Integer = 20
        Const messageType As Byte = 1
        Const offset As Integer = 3
        Const hashLen As Integer = 32
        Dim tmpMessageBytes() As Byte = Curve25519_ECDH.Crypto.AES.Convert_String_ToByteArray(Message)
        Dim x As Integer
        Dim y As Integer
        Dim maxMessageByteLength As Integer = recBuffSize - offset - usernameLength - hashLen
        Dim sha256 As New SHA256Managed
        Dim lastNonZeroIndex As Integer = getLastNonZeroIndex(tmpMessageBytes)
        Dim nonZeroMessageBytes(lastNonZeroIndex) As Byte

        Dim CBC_AES_Return As ECIES.TEncryption
        Dim remotePoint As ECPoint = lookupTable.getClientsPK(Username)
        Array.Copy(tmpMessageBytes, nonZeroMessageBytes, lastNonZeroIndex + 1)
        CBC_AES_Return = ECIES_AES.Encrypt_AES_256(nonZeroMessageBytes, localKeys, curve, remotePoint)
        If CBC_AES_Return.didError Then
            errorHappened(CBC_AES_Return, "Failed to encrypt data", errorEnum.ENCRYPTION_FAILED, GetCurrentMethod.Name)
            Return
        End If
        Dim encryptedData(IVBytesLen + MACBytesLen + CBC_AES_Return.Data.Length - 1) As Byte
        CBC_AES_Return.Data.CopyTo(encryptedData, 0)
        CBC_AES_Return.IV.CopyTo(encryptedData, CBC_AES_Return.Data.Length)
        CBC_AES_Return.HMAC.CopyTo(encryptedData, CBC_AES_Return.Data.Length + IVBytesLen)

        If encryptedData.Length > maxMessageByteLength Then
            y = CInt(encryptedData.Length / maxMessageByteLength) + 1
        Else
            y = 1
        End If

        For x = 1 To y
            Dim messageBytes(maxMessageByteLength - 1) As Byte
            If x = y And y <> 1 Then
                ReDim messageBytes(maxMessageByteLength - (tmpMessageBytes.Length - lastNonZeroIndex - 1))
                Array.ConstrainedCopy(encryptedData, maxMessageByteLength * (x - 1), messageBytes, 0, maxMessageByteLength - (tmpMessageBytes.Length - lastNonZeroIndex - 1) + 1)
            ElseIf x = y And y = 1 Then
                ReDim messageBytes(encryptedData.Length - 1)
                messageBytes = encryptedData
            Else
                messageBytes = encryptedData
            End If
            Dim hashBytes() As Byte = sha256.ComputeHash(messageBytes)
            If hashBytes.Length > hashLen Then
                writeDebugLog("Generated hash for the message was longer than the 32 bytes allowed for SHA256 algorithm", GetCurrentMethod.Name, True)
                Return
            End If
            Dim fullMessage(recBuffSize - 1) As Byte
            fullMessage(0) = messageType
            fullMessage(1) = CByte(x)
            fullMessage(2) = CByte(y)
            'fullMessage(3) = usernameLength
            UsernameBytes.CopyTo(fullMessage, offset)
            hashBytes.CopyTo(fullMessage, offset + usernameLength)
            messageBytes.CopyTo(fullMessage, offset + usernameLength + hashLen)
            'For i = 0 To usernameLength - 1
            '    fullMessage(i + offset) = UsernameBytes(i)
            'Next
            'i = 0
            'For i = 0 To messageBytes.Length - 1
            '    fullMessage(i + offset + usernameLength) = messageBytes(i + (maxMessageByteLength * (x - 1)))
            'Next
            Try
                writeDebugLog("Sending IM to the server", GetCurrentMethod.Name)
                sendData(fullMessage)
            Catch ex As ObjectDisposedException
                writeDebugLog("Unable to send the file, stream was disposed before we finshed", GetCurrentMethod.Name, True)
                Return
            End Try
        Next
        Dim SQL As String
        'Add our sent message to the database in non encrypted form, set the recieve date to todays and set the sender UserID to the recipients
        SQL = "INSERT INTO TBL_MESSAGES ([MessageData], [ReceivedDate], [IsFile], [Sender_UserID]) VALUES ('" & Message & "', '" & Date.Today.ToShortDateString.ToString & "', " & False & ", '" & Username & "');"
        databaseConnection.sendSQLNonQuery(SQL)
    End Sub
    Public Sub sendFile(ByVal Username As String, ByVal FilePath As String)
        'A problem occured which didnt throw an error except just had unecessary execution
        'If closeConnection() was called without the use of _disposed then it was possible for the user to call any send function, due to error catching it would catch the stream disposed execption and then just end up close the connection again
        'This wasted execution time, to fix the problem we throw an ObjectDisposed exception which should notify the user that we can reset the object fine
        If _disposed Then
            'Throw New ObjectDisposedException(Me.ToString)
            writeDebugLog("Object has already been disposed, reinitialise before accessing", GetCurrentMethod.Name, True)
            Return
        End If
        If ASCII.GetByteCount(Username) > 20 Then
            'Throw New Exception("Username longer than the maximum allowed of 20 bytes")
            errorHappened(Me, "Username longer than the maximum allowed of 20 bytes", errorEnum.USERNAME_LONGER_THAN_MAX, GetCurrentMethod.Name)
            Return
        End If
        If Not File.Exists(FilePath) Then
            writeDebugLog("Unable to find the file specified at path: " & FilePath, GetCurrentMethod.Name, True)
            Return
        End If
        '| Message Type | x | of y | username | data hash | data |
        '|     Byte     | B |   B  | 20 bytes | 32 bytes  | Bytes|
        'Newest revision of the message data where we must include the hash for the data that got sent that time, this was due to possible transmission errors
        'the hash is SHA256 so should be 32 bytes of data when the server gets the data the hash can be checked against generated hash for the data, and same on client end
        'Dim i As Integer
        Dim UsernameBytes(19) As Byte
        Array.Copy(ASCII.GetBytes(Username), UsernameBytes, ASCII.GetByteCount(Username))
        Dim usernameLength As Byte = 20
        Const messageType As Byte = 2
        Dim x As Integer
        Dim y As Integer
        Const offset As Integer = 3
        Const hashLen As Integer = 32
        Const filenameLen As Integer = 100
        Dim maxMessageByteLength As Integer = recBuffSize - offset - usernameLength - hashLen
        Dim fs As FileStream
        Dim sha256 As New SHA256Managed
        Try
            fs = New FileStream(FilePath, FileMode.Open)
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
            Return
        End Try
        If (fs.Length + MACBytesLen + IVBytesLen) > maxMessageByteLength Then
            y = CInt((fs.Length + MACBytesLen + IVBytesLen) / maxMessageByteLength) + 1
        Else
            y = 1
        End If
        If y > 256 Then
            errorHappened(fs, "File length greater than allowed", errorEnum.VALUE_LONGER_THAN_MAX, GetCurrentMethod.Name)
        End If
        Dim fileName() As Byte = ASCII.GetBytes(Path.GetFileName(FilePath))
        If fileName.Length > filenameLen Then
            writeDebugLog("Filename length was longer than allowed", GetCurrentMethod.Name, True)
            fs.Dispose()
            Return
        End If
        'Contains the filename and thats sent first, we then setup the filestream ready for storage on the other end
        Dim initalMessage(recBuffSize - 1) As Byte
        initalMessage(0) = messageType
        initalMessage(1) = 0
        initalMessage(2) = CByte(y)
        UsernameBytes.CopyTo(initalMessage, offset)
        fileName.CopyTo(initalMessage, offset + usernameLength)
        sendData(initalMessage)

        Dim fileBytes(fs.Length - 1) As Byte
        fs.Read(fileBytes, 0, fs.Length)
        fs.Close()
        Dim lastNonZeroIndex As Integer = getLastNonZeroIndex(fileBytes)
        Dim fileBytesNoZeros(lastNonZeroIndex) As Byte
        Array.Copy(fileBytes, fileBytesNoZeros, lastNonZeroIndex + 1)

        Dim CBC_AES_Return As ECIES.TEncryption
        Dim remotePoint As ECPoint = lookupTable.getClientsPK(Username)
        CBC_AES_Return = ECIES_AES.Encrypt_AES_256(fileBytesNoZeros, localKeys, curve, remotePoint)
        If CBC_AES_Return.didError Then
            errorHappened(CBC_AES_Return, "Failed to encrypt data", errorEnum.ENCRYPTION_FAILED, GetCurrentMethod.Name)
            Return
        End If
        Dim encryptedData(IVBytesLen + MACBytesLen + CBC_AES_Return.Data.Length - 1) As Byte
        CBC_AES_Return.Data.CopyTo(encryptedData, 0)
        CBC_AES_Return.IV.CopyTo(encryptedData, CBC_AES_Return.Data.Length)
        CBC_AES_Return.HMAC.CopyTo(encryptedData, CBC_AES_Return.Data.Length + IVBytesLen)

        For x = 1 To y
            Dim data(maxMessageByteLength - 1) As Byte
            If x = y AndAlso encryptedData.Length - (maxMessageByteLength * (y - 1)) < maxMessageByteLength Then
                ReDim data(encryptedData.Length - (maxMessageByteLength * (y - 1)) - 1)
                Array.ConstrainedCopy(encryptedData, maxMessageByteLength * (x - 1), data, 0, encryptedData.Length - (maxMessageByteLength * (y - 1)))
            Else
                Array.ConstrainedCopy(encryptedData, maxMessageByteLength * (x - 1), data, 0, maxMessageByteLength)
            End If
            Dim fullMessage(recBuffSize - 1) As Byte
            Dim hashBytes() As Byte = sha256.ComputeHash(data)
            If hashBytes.Length > hashLen Then
                writeDebugLog("Generated hash for the message was longer than the 32 bytes allowed for SHA256 algorithm", GetCurrentMethod.Name, True)
                Return
            End If
            fullMessage(0) = messageType
            fullMessage(1) = CByte(x)
            fullMessage(2) = CByte(y)
            UsernameBytes.CopyTo(fullMessage, offset)
            hashBytes.CopyTo(fullMessage, offset + usernameLength)
            data.CopyTo(fullMessage, offset + usernameLength + hashLen)
            Try
                writeDebugLog("Sending file to the server", GetCurrentMethod.Name)
                sendData(fullMessage)
            Catch ex As ObjectDisposedException
                writeDebugLog("Unable to send the file, stream was disposed before we finshed", GetCurrentMethod.Name, True)
                Return
            End Try
        Next
        writeDebugLog("Finished sending file to the server", GetCurrentMethod.Name)

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
    Public Sub sendNewUser(ByVal username As String, ByVal passwordHash() As Byte, ByVal isAdmin As Boolean)
        If passwordHash.Length > 32 Then
            errorHappened(passwordHash, "The length of the hash was longer than allowed", errorEnum.VALUE_LONGER_THAN_MAX, GetCurrentMethod.Name)
            Return
        End If
        Dim usernameBytes() As Byte = ASCII.GetBytes(username)
        If usernameBytes.Length > 20 Then
            errorHappened(usernameBytes, "Username was longer than allowed", errorEnum.USERNAME_LONGER_THAN_MAX, GetCurrentMethod.Name)
            Return
        End If
        Dim fullMessage(recBuffSize - 1) As Byte
        Dim messageID As Byte = 202

        fullMessage(0) = messageID
        usernameBytes.CopyTo(fullMessage, 1)
        passwordHash.CopyTo(fullMessage, 21)
        fullMessage(53) = CByte(isAdmin)
        sendData(fullMessage)
    End Sub
    Public Sub updatePasswordHash(ByVal username As String, ByVal oldPasswordHash() As Byte, ByVal newPasswordHash() As Byte)
        If oldPasswordHash.Length > 32 Or newPasswordHash.Length > 32 Then
            errorHappened(oldPasswordHash, "The length of the hash was longer than allowed", errorEnum.VALUE_LONGER_THAN_MAX, GetCurrentMethod.Name)
            Return
        End If
        Dim usernameBytes() As Byte = ASCII.GetBytes(username)
        If usernameBytes.Length > 20 Then
            errorHappened(usernameBytes, "Username was longer than allowed", errorEnum.USERNAME_LONGER_THAN_MAX, GetCurrentMethod.Name)
            Return
        End If
        Dim fullMessage(recBuffSize - 1) As Byte
        Dim messageID As Byte = 203

        fullMessage(0) = messageID
        usernameBytes.CopyTo(fullMessage, 1)
        oldPasswordHash.CopyTo(fullMessage, 21)
        newPasswordHash.CopyTo(fullMessage, 53)
        sendData(fullMessage)

    End Sub
    Public Sub updateUsersRights(ByVal username As String, ByVal isAdmin As Boolean)
        Dim usernameBytes() As Byte = ASCII.GetBytes(username)
        If usernameBytes.Length > 20 Then
            errorHappened(usernameBytes, "Username was longer than allowed", errorEnum.USERNAME_LONGER_THAN_MAX, GetCurrentMethod.Name)
            Return
        End If
        Dim fullMessage(recBuffSize - 1) As Byte
        Dim messageID As Byte = 204

        fullMessage(0) = messageID
        usernameBytes.CopyTo(fullMessage, 1)
        fullMessage(22) = CByte(isAdmin)
        sendData(fullMessage)

    End Sub
    Public Sub removeUser(ByVal username As String)
        Dim usernameBytes() As Byte = ASCII.GetBytes(username)
        If usernameBytes.Length > 20 Then
            errorHappened(usernameBytes, "Username was longer than allowed", errorEnum.USERNAME_LONGER_THAN_MAX, GetCurrentMethod.Name)
            Return
        End If
        Dim fullMessage(recBuffSize - 1) As Byte
        Dim messageID As Byte = 205

        fullMessage(0) = messageID
        usernameBytes.CopyTo(fullMessage, 1)
        sendData(fullMessage)

    End Sub
    Public Sub snycWithServer()
        Dim message(1) As Byte
        message(0) = 206
        sendData(message)
    End Sub
End Class
