Imports System.Net.Sockets
Imports System.Net
Imports System.Text.Encoding
Public Class MTClientReciever
    Private stream As NetworkStream
    Private client As TcpClient
    Private canRead As Boolean = True
    Private canWrite As Boolean = True
    Private Ended As Boolean
    Private recvBuffLenght As Integer
    Private WithEvents HeartBeatTimer As Heartbeat
    Private clNumber As Integer
    Private endpointIP As String
    Private linearMessageQueue As New Queue
    Public Event clientDisconnect(ByVal IPaddr As String, ByVal clNumber As Integer)
    Public Event ReceivedData(ByVal Data() As Byte, ByVal IPaddr As String, ByVal clNumber As Integer)
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
    Public Sub New(ByRef client As TcpClient, ByVal clNumber As Integer)
        If Not client.Connected Then
            Throw New NullReferenceException("The provided client is no longer connected")
        End If
        Dim endpoint As IPEndPoint = client.Client.RemoteEndPoint
        endpointIP = endpoint.Address.ToString
        'Create the network stream to get data in and out
        stream = client.GetStream
        'store the tcpClient reference and the clients unique number
        Me.client = client
        Me.clNumber = clNumber
        'start the thread that polls for sent messages
        Dim RThread As Threading.Thread = New Threading.Thread(AddressOf pollClientSend)
        RThread.Start()
        'Start the thread that polls for new data in the queue
        Dim SThread As Threading.Thread = New Threading.Thread(AddressOf MTSend)
        SThread.Start()
        'set the recieve buffer size and start the heartbeat timer to check if the client has timed out
        recvBuffLenght = client.ReceiveBufferSize
        HeartBeatTimer = New Heartbeat()
    End Sub
    Private Sub pollClientSend()
        Do
            If stream.DataAvailable And canRead Then
                'we need to raise the event of client has sent data, we must also reset the timer due to the fact that the client is not idle but alive
                HeartBeatTimer.Timer.Stop()
                dataReciever(Nothing)
                Try
                    HeartBeatTimer.Timer.Start()
                Catch ex As Exception
                    'No need to worry by raising an event we need to return from the event at some point, if the client disconnected we dispose of the timer, therefore meaning when we return from the event the timer is disposed of and we crash
                    'Now we dont crash and everything is okay anyway
                    'The thread should now end anyway
                End Try
                Threading.Thread.Sleep(1000)
            End If
        Loop Until Ended
    End Sub
    Private Sub dataReciever(ByVal data() As Byte)
        Dim recvBytes(recvBuffLenght - 1) As Byte
        Dim recvLength As Integer
        Dim endpoint As IPEndPoint = client.Client.RemoteEndPoint
        'lock out the network stream so that a deadlock isnt created, might be a bit pedantic but to be safe
        canWrite = False
        'read the data in
        If data Is Nothing Then
            recvLength = stream.Read(recvBytes, 0, recvBuffLenght)
            If (recvBytes(0) = 1 Or recvBytes(0) = 2) And recvLength <> recvBuffLenght Then
                Dim dataCounter As Integer = recvLength
                Do
                    MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: Got " & dataCounter & " out of " & recvBuffLenght & vbNewLine)
                    recvLength = stream.Read(recvBytes, dataCounter, recvBuffLenght - dataCounter)
                    dataCounter += recvLength
                Loop Until dataCounter = recvBuffLenght
            End If
            MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "All data obtained" & vbNewLine)
        Else
            recvBytes = data
        End If
        'check if the vlaue is hearbeat, and change the output to the log
        If BitConverter.ToInt32(recvBytes, 0) = 43775 Then
            MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Heartbeat, client still alive" & vbNewLine)
            canWrite = True
            Return
        Else
            MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Received data from client" & vbNewLine)
        End If
        'set it so that we can write to the stream again
        canWrite = True
        'raise the event for the socketMT class to deal with and decode what the data means
        RaiseEvent ReceivedData(recvBytes, endpoint.Address.ToString, clNumber)
    End Sub
    Public Sub sendData(ByVal Data() As Byte, Optional ByVal errorBytes As Boolean = False)
        'start new thread for sending messages and pass the data to it
        Dim tmpArray(recvBuffLenght - 1) As Byte
        If Data.Length <> recvBuffLenght Then
            Array.Copy(Data, tmpArray, Data.Length)
        Else
            tmpArray = Data
        End If
        Dim parameters As New sendingParameters
        parameters.Data = tmpArray
        parameters.errorBytes = errorBytes
        linearMessageQueue.Enqueue(parameters)
        'Dim SThread As New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf MTSend))
        'SThread.Start(parameters)
    End Sub
    'Private Sub MTSend(ByVal params As sendingParameters)
    Private Sub MTSend()
        Do
            If linearMessageQueue.Count > 0 And canWrite Then
                Dim params As sendingParameters = linearMessageQueue.Dequeue()
                canRead = False
                'reset the timer like before, if we fail to write the client is already disconnected and the heartbeat wont help us here, possible it could write data to the stream the same time as we do, invalidating the data
                HeartBeatTimer.Timer.Stop()
                MTMainThreadWriter.writeLog("[Client: " & clNumber & "]: " & "Sending data to client" & vbNewLine)
                Try
                    stream.Write(params.Data, 0, params.Data.Length)
                Catch ex As Exception
                    'shutdown the clients connection, its dead
                    closeClientConnection(True)
                    canRead = True
                    Return
                End Try
                'flush the stream, start the timer and allow the stream to be read
                stream.Flush()
                If Not params.errorBytes Then
                    Dim serverResponseBytes(recvBuffLenght - 1) As Byte
                    Dim value As Integer
                    Try
                        stream.Read(serverResponseBytes, 0, recvBuffLenght)
                    Catch ex As Exception
                        canRead = True
                        Return
                    End Try
                    Try
                        value = BitConverter.ToInt32(serverResponseBytes, 0)
                    Catch ex As Exception
                        'Dont worry
                    End Try
                    If value = 61149 Then
                        Dim ret As Boolean = trySendAgain(params.Data)
                        If ret = False Then
                            closeClientConnection(True)
                            canRead = True
                            Return
                        End If
                    ElseIf Not value = 43724 Then
                        'Throw New Exception
                        dataReciever(serverResponseBytes)
                        'trySendAgain(params.Data)
                    End If
                    HeartBeatTimer.Timer.Start()
                End If

                canRead = True
            End If
            Threading.Thread.Sleep(1000)
        Loop Until Ended
    End Sub
    Private Function trySendAgain(ByVal data() As Byte) As Boolean
        Try
            stream.Write(data, 0, data.Length)
        Catch ex As Exception
            Return False
        End Try
        stream.Flush()
        Dim serverResponseBytes(recvBuffLenght - 1) As Byte
        Dim value As Integer
        Try
            stream.Read(serverResponseBytes, 0, recvBuffLenght)
        Catch ex As Exception
            Return False
        End Try
        Try
            value = BitConverter.ToInt32(serverResponseBytes, 0)
        Catch ex As Exception
            'Dont worry
        End Try
        If value = 61149 Then
            If trySendAgain(data) = False Then
                Return False
            End If
        End If
        Return True
    End Function
    Public Sub sendError(Optional ByVal errorCode As sendingErrorEnum = 0)
        Dim value As Integer = 61149
        Dim errorByte(3) As Byte
        BitConverter.GetBytes(value).CopyTo(errorByte, 0)
        errorByte(3) = CInt(errorCode)
        sendData(errorByte, True)
    End Sub
    Public Sub sendACK()
        Dim value As Integer = 43724
        Dim errorByte() As Byte = BitConverter.GetBytes(value)
        sendData(errorByte, True)
    End Sub
    Private Sub NoHeartbeat() Handles HeartBeatTimer.timerFiredNoResponse
        'correctly stop the thread
        Ended = True
        Try
            HeartBeatTimer.Timer.Dispose()
        Catch ex As Exception
            'No need to worry, timer is already stopped
        End Try
        'dispose of the stream and close the tcp connection
        stream.Dispose()
        client.Close()
        'write to the log, remove the client from the DataGridViewer in the UI and finally remove the client from the lookupTable
        MTMainThreadWriter.writeLog("[Client: " & clNumber & "]:" & "No heartbeat packet was Received, or an error happened when writing to the stream, client disconnected" & vbNewLine)
        MTMainThreadWriter.RemoveClientFromDGV(clNumber)
        RaiseEvent clientDisconnect(endpointIP, clNumber)
    End Sub
    Public Sub closeClientConnection(ByVal raisingevent As Boolean)
        If Ended = True Then
            Return
        End If
        Ended = True
        Try
            HeartBeatTimer.Timer.Dispose()
        Catch ex As Exception
            'No need to worry, timer is already stopped
        End Try
        stream.Dispose()
        client.Close()
        MTMainThreadWriter.writeLog("[Client: " & clNumber & "]:" & " Client disconnected" & vbNewLine)
        MTMainThreadWriter.RemoveClientFromDGV(clNumber)
        'We need to raise an event so that socketMT (which has accesss to the lookuptable) can remove us from logged on users
        'Should be boolean because if clearing out table then we dont need unecessary raiseEvent
        If raisingevent Then
            RaiseEvent clientDisconnect(endpointIP, clNumber)
        End If
    End Sub
    Public Sub removeOneFromClNumber()
        clNumber -= 1
    End Sub
    Public ReadOnly Property cliNumber() As Integer
        Get
            Return clNumber
        End Get
    End Property
End Class
