Imports System.Net.Sockets
Public Class Heartbeat
    Public WithEvents Timer As System.Timers.Timer
    Public Event IOException(ByVal serverDead As Boolean)
    Private stream As NetworkStream
    'Public paused As Boolean
    Public Sub New(ByRef stream As NetworkStream)
        Timer = New System.Timers.Timer()
        Timer.Interval = 90000 '1.5 minutes
        Timer.Enabled = True
        Timer.Start()
        Me.stream = stream
    End Sub
    Private Sub timerFired() Handles Timer.Elapsed
        'If Not paused Then
        Dim value As Integer = 43775
        Dim heartbeat As Byte() = BitConverter.GetBytes(value)
        Try
            stream.Write(heartbeat, 0, heartbeat.Length)
        Catch ex As System.IO.IOException
            RaiseEvent IOException(True)
            Return
        End Try
        Timer.Stop()
        Timer.Start()
        'End If
    End Sub
End Class
