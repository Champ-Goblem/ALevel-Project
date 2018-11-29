Imports Error_Handler.Events_Handler
Imports System.Reflection.MethodInfo
Public Class MainThreadUIUpdater
    Private Shared frmInstance As FRM_Messages = Nothing
    Public Shared Sub addInstance(ByRef instance As Form)
        frmInstance = instance
    End Sub
    Public Shared Sub addUserToList(ByVal username As String, ByVal connected As Integer)
        If frmInstance Is Nothing Then
            writeDebugLog("Set an instance of the form with addInstance", GetCurrentMethod.Name, True)
            Return
        End If
        Try
            frmInstance.addUserToListView(username, connected)
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
        End Try
    End Sub
    Public Shared Sub removeUserFromList(ByVal username As String)
        If frmInstance Is Nothing Then
            writeDebugLog("Set an instance of the form with addInstance", GetCurrentMethod.Name, True)
            Return
        End If
        Try
            frmInstance.removeUserFromListView(username)
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
        End Try
    End Sub
    Public Shared Sub changeListIcon(ByVal username As String, ByVal connected As Integer)
        If frmInstance Is Nothing Then
            writeDebugLog("Set an instance of the form with addInstance", GetCurrentMethod.Name, True)
            Return
        End If
        Try
            frmInstance.changeUserIcon(username, connected)
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
        End Try
    End Sub
    Public Shared Sub checkAndAdd(ByVal username As String, ByVal message As String, ByVal Optional file As Boolean = False)
        If frmInstance Is Nothing Then
            writeDebugLog("Set an instance of the form with addInstance", GetCurrentMethod.Name, True)
            Return
        End If
        Try
            frmInstance.checkAndAdd(username, message, file)
        Catch ex As Exception
            writeDebugLog(ex.Message, GetCurrentMethod.Name, True)
        End Try
    End Sub
End Class
