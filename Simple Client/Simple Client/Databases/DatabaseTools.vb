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
        ClientDatabase
        ServerDatabase
    End Enum
    Public Sub New(ByVal databaseFile As databaseEnum, Optional ByVal username As String = Nothing)
        If databaseFile = databaseEnum.ClientDatabase And username = Nothing Then
            errorHappened(username, "The username was NULL when checking for the users database", errorEnum.NULL_REFERECE_EXCEPTION, GetCurrentMethod.Name)
            Return
        End If
        If databaseFile = databaseEnum.ServerDatabase And Not File.Exists(dbWorkingDirectory & databaseFile.ToString & ".mdb") Then
            writeDebugLog("Database " & databaseFile.ToString & " could not be found", GetCurrentMethod.Name)
            createDatabase(dbWorkingDirectory & databaseFile.ToString & ".mdb", databaseEnum.ServerDatabase)
        ElseIf databaseFile = databaseEnum.ClientDatabase And Not File.Exists(dbWorkingDirectory & username & "_" & databaseFile.ToString & ".mdb") Then
            writeDebugLog("Database " & username & "_" & databaseFile.ToString & " could not be found", GetCurrentMethod.Name)
            createDatabase(dbWorkingDirectory & username & "_" & databaseFile.ToString & ".mdb", databaseEnum.ClientDatabase)
        End If
        If databaseFile = databaseEnum.ServerDatabase Then
            DBConnection = New OleDbConnection(dbConnectString & "Data Source=" & dbWorkingDirectory & databaseFile.ToString & ".mdb")
            DBConnection.Open()
        Else
            DBConnection = New OleDbConnection(dbConnectString & "Data Source=" & dbWorkingDirectory & username & "_" & databaseFile.ToString & ".mdb")
            DBConnection.Open()
        End If
    End Sub
    Public Sub createDatabase(ByVal filePath As String, ByVal databaseFile As databaseEnum)
        Dim cat As New ADOX.Catalog
        cat.Create(dbConnectString & "Data Source=" & filePath)
        DBConnection = New OleDbConnection(dbConnectString & "Data Source=" & filePath)
        DBConnection.Open()
        Dim SQL As String
        If databaseFile = databaseEnum.ClientDatabase Then
            'SQL = "CREATE DATABASE " & Path.GetFileNameWithoutExtension(filePath) & ";"
            'sendSQLNonQuery(SQL)
            SQL = "CREATE TABLE TBL_USERS ([Username] TEXT NOT NULL PRIMARY KEY);"
            sendSQLNonQuery(SQL)
            SQL = "CREATE TABLE TBL_FILES ([FileID] COUNTER NOT NULL UNIQUE PRIMARY KEY, [Filename] TEXT, [Sender_UserID] TEXT, CONSTRAINT FK_SenderUsername FOREIGN KEY ([Sender_UserID]) REFERENCES TBL_USERS([Username]));"
            sendSQLNonQuery(SQL)
            SQL = "CREATE TABLE TBL_MESSAGES ([MessageID] COUNTER NOT NULL UNIQUE PRIMARY KEY, [MessageData] LONGTEXT, [ReceivedDate] LONGTEXT, [IsFile] YESNO, [Received_FileID] LONG, [Sender_UserID] LONGTEXT, CONSTRAINT FK_SenderUsername2 FOREIGN KEY ([Sender_UserID]) REFERENCES TBL_USERS([Username]), CONSTRAINT FK_FileID FOREIGN KEY ([Received_FileID]) REFERENCES TBL_FILES([FileID]));"
            sendSQLNonQuery(SQL)
        Else
            'SQL = "CREATE DATABASE " & Path.GetFileNameWithoutExtension(filePath) & ";"
            'sendSQLNonQuery(SQL)
            SQL = "CREATE TABLE TBL_USER ([Username] TEXT NOT NULL PRIMARY KEY, [Password_Hash] LONGTEXT NOT NULL, [Is_Administrator] BOOLEAN, [Last_Known_PublicKey] LONGTEXT);"
            sendSQLNonQuery(SQL)
            SQL = "INSET INTO TBL_USER ([Username], [Password_Hash], [Is_Administrator]) VALUES ('Administrator', 'CvsAE42OczSOwf5B/T06j8vZAVayY7+leRug4JX0LPw=', TRUE);"
            sendSQLNonQuery(SQL)
            SQL = "CREATE TABLE TBL_UNSENT_FILES ([FileID] COUNTER NOT NULL UNIQUE PRIMARY KEY, [Filepath] LONGTEXT, [Sender_UserID] TEXT, [Recipient_UserID] TEXT, CONSTRAINT FK_SenderUsername FOREIGN KEY ([Sender_UserID]) REFERENCES TBL_USER(Username), CONSTRAIN FK_RecipientUsername FOREIGN KEY ([Recipient_UserId]) REFERENCES TBL_USER([Username]));"
            sendSQLNonQuery(SQL)
            SQL = "CREATE TABLE TBL_UNSENT_MESSAGES ([MessageID] COUNTER NOT NULL UNIQUE PRIMARY KEY, [MessageData] LONGTEXT, [Sender_UserID] TEXT, [Recipient_UserID] TEXT, CONSTRAINT FK_SenderUsername2 FOREIGN KEY ([Sender_UserID]) REFERENCES TBL_USER(Username), CONSTRAIN FK_RecipientUsername2 FOREIGN KEY ([Recipient_UserId]) REFERENCES TBL_USER([Username]));"
            sendSQLNonQuery(SQL)
        End If
    End Sub
    Public Sub sendSQLNonQuery(ByVal SQL As String)
        Dim cmd As New OleDbCommand(SQL, DBConnection)
        cmd.ExecuteNonQuery()
    End Sub
    Public Function sendSQLQuery(ByVal SQL As String) As OleDbDataReader
        Dim cmd As New OleDbCommand(SQL, DBConnection)
        Dim reader As OleDbDataReader = cmd.ExecuteReader()
        Return reader
    End Function
End Class
