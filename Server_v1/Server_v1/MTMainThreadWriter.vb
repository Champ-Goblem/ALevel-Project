Public Class MTMainThreadWriter
    Private Shared frmInstance As Form1 = Nothing
    Public Enum MTMTWErrors
        NO_ERROR
        FRMINSTANCE_IS_NULL
        EXCEPTION_HAPPENED
    End Enum
    Public Shared Sub setInstance(ByRef MainFormUIInstance As Form1)
        'must be called first so that we have the instance of the UI
        frmInstance = MainFormUIInstance
    End Sub
    Public Shared Function writeLog(ByVal message As String) As MTMTWErrors
        'check that the instance has been set then try adding the message to the log
        If frmInstance Is Nothing Then
            Return MTMTWErrors.FRMINSTANCE_IS_NULL
        End If
        Try
            frmInstance.writeLog(message)
        Catch ex As Exception
            Return MTMTWErrors.EXCEPTION_HAPPENED
        End Try
        Return MTMTWErrors.NO_ERROR
    End Function
    Public Shared Function AddClientToDGV(ByVal Username As String, ByVal IPAddr As String, ByVal PublicKey() As Byte) As MTMTWErrors
        'check that the instance has been set then try adding the client to the DataGridViewer
        If frmInstance Is Nothing Then
            Return MTMTWErrors.FRMINSTANCE_IS_NULL
        End If
        Try
            frmInstance.addNewClient(Username, IPAddr, PublicKey)
        Catch ex As Exception
            Return MTMTWErrors.EXCEPTION_HAPPENED
        End Try
        Return MTMTWErrors.NO_ERROR
    End Function
    Public Shared Function RemoveClientFromDGV(ByVal Row As Integer) As MTMTWErrors
        'check that the instance has been set then try removing the client from the DataGridView
        If frmInstance Is Nothing Then
            Return MTMTWErrors.FRMINSTANCE_IS_NULL
        End If
        Try
            frmInstance.removeClientDGV(Row)
        Catch ex As Exception
            Return MTMTWErrors.EXCEPTION_HAPPENED
        End Try
        Return MTMTWErrors.NO_ERROR
    End Function
    Public Shared Function GetDGVCount() As Object
        'check that the instance has been set then try getting the number of rows in the DataGridView
        If frmInstance Is Nothing Then
            Return MTMTWErrors.FRMINSTANCE_IS_NULL
        End If
        Try
            Return frmInstance.DGV_Users.Rows.Count
        Catch ex As Exception
        End Try
        Return MTMTWErrors.EXCEPTION_HAPPENED
    End Function
End Class
