Imports System.Net.Sockets
Imports System.Net
Imports System.Threading
Public Class NewTcpListener
    'We are extending the functionality of the TcpListener to make it multithreaded
    Inherits System.Net.Sockets.TcpListener
    Public Event clientConnecting()
    Private ended As Boolean
    'Public frmInstance As Form1
    'Private WithEvents cl As New ClientConnection
    Public Sub New(ByVal port As Integer, ByVal localIP As IPAddress)
        'create a new intstance of TcpListener
        MyBase.New(localIP, port)
    End Sub
    Public Sub startClientPoll(ByRef Sender As TcpListener)
        'start the poll for new client connetions
        Dim TThread As New Thread(New ParameterizedThreadStart(AddressOf pollClientConnect))
        TThread.Start(Sender)
    End Sub
    Private Sub pollClientConnect(tcplistener As TcpListener)
        Do
            'If a client is pending connection then raise the event and let socketMT deal with it
            If tcplistener.Pending() Then
                RaiseEvent clientConnecting()
            End If
        Loop Until ended = True
    End Sub
    Public Sub stopListener()
        'stop the thread and stop the instance of TcpListener
        ended = True
        MyBase.Stop()
    End Sub
End Class