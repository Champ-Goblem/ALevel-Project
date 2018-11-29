Imports System.Net.Sockets
Public Class Heartbeat
    Public WithEvents Timer As System.Timers.Timer
    Public Event timerFiredNoResponse()
    Public Sub New()
        Timer = New System.Timers.Timer()
        Timer.Interval = 120000 '2 minutes
        'Timer.Interval = 3000 'used in testing
        Timer.Start()
    End Sub
    Private Sub timerFired() Handles Timer.Elapsed
        Timer.Stop()
        RaiseEvent timerFiredNoResponse()
    End Sub
End Class
