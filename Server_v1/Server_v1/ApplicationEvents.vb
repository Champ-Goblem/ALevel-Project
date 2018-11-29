Namespace My
    ' The following events are available for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication
        Private Sub UnhandledExceptionLogSaver(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Form1.RTB_Log.AppendText("[!Unhandled Exception!]:" & e.Exception.Message & vbNewLine)
            Dim fs As New IO.FileStream(Environment.CurrentDirectory + "\Crash Log.txt", IO.FileMode.CreateNew)
            fs.Write(System.Text.Encoding.ASCII.GetBytes(Form1.RTB_Log.Text), 0, System.Text.Encoding.ASCII.GetByteCount(Form1.RTB_Log.Text))
            fs.Close()
        End Sub
    End Class


End Namespace

