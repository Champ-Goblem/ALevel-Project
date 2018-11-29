Imports System.IO
Imports System.Environment
Imports Error_Handler.Events_Handler
Imports System.Reflection.MethodInfo
Imports System.Data
Imports System.Data.OleDb
Public Class DatabaseTools
    Private dbWorkingDirectory As String = (CurrentDirectory & "\")
    Private Const dbConnectString As String = "Provider=Microsoft.Jet.OLEDB.4.0;"
    Private DBConnection As OleDbConnection = Nothing
    Public Enum databaseEnum
        Client
        Server
    End Enum
    Public Sub New(ByVal databaseFile As databaseEnum, Optional ByVal username As String = Nothing)
        If databaseFile = databaseEnum.Client And username = Nothing Then
            errorHappened(username, "The username was NULL when checking for the users database", errorEnum.NULL_REFERECE_EXCEPTION, GetCurrentMethod.Name)
            Return
        End If
        If Not File.Exists(dbWorkingDirectory & databaseFile.ToString & ".accdb") Then
            writeDebugLog("Database " & databaseFile.ToString & " could not be found", GetCurrentMethod.Name)
            Return
        End If
        DBConnection = New OleDbConnection(dbConnectString & dbWorkingDirectory)
        DBConnection.Open()
    End Sub
    'Public Sub createDatabase(ByVal databaseFile As databaseEnum)
    '    File.Create(dbWorkingDirectory & databaseFile.ToString & ".accdb")
    '    DBConnection = New OleDbConnection(dbConnectString & dbWorkingDirectory)
    '    DBConnection.Open()
    '    Dim SQL As String
    '    If databaseFile = databaseEnum.Client Then
    '        SQL = "CREATE TABLE TBL_USERS ([UserID] uniqueidentifier primary key,[Username] text)"
    '        sendSQL(SQL)
    '        SQL = "CREATE TABLE TBL_FILES ([FileID] uniqueidentifier primary key,[Filename] text,[Sender_UserID] long)"
    '        sendSQL(SQL)
    '        SQL = "CREATE TABLE TBL_MESSAGES (MessageID,MessageData,ReceivedDate,IsFile,Sender_UserID)"
    '        sendSQL(SQL)
    '    Else
    '        SQL = "CREATE TABLE TBL_USERS"
    '    End If
    'End Sub
    Public Sub sendSQLNonQuey(ByVal SQL As String)
        Dim cmd As New OleDbCommand(SQL, DBConnection)
        cmd.ExecuteNonQuery()
    End Sub
    Public Function sendSQLQuery(ByVal SQL As String) As OleDbDataReader
        Dim cmd As New OleDbCommand(SQL, DBConnection)
        Dim reader As OleDbDataReader = cmd.ExecuteReader()
        Return reader
    End Function
End Class
