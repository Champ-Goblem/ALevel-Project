Imports System.Text.Encoding
Imports System.Security.Cryptography
Public Class FRM_UserSettings
    Private srFunction As SendRecieve = Nothing
    Private numberOfUsers As Integer
    Private Delegate Sub delegateAddRows(ByVal items As Object())
    Public Sub New(ByRef srFunction As SendRecieve)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.srFunction = srFunction
        srFunction.snycWithServer()
    End Sub
    Private Sub SyncToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SyncToolStripMenuItem.Click
        DGV_Users.Rows.Clear()
        srFunction.snycWithServer()
    End Sub
    Public Sub getUserDetails(ByVal data() As Byte)
        numberOfUsers = BitConverter.ToInt32(data, 1)
        Dim usernameBytes(19) As Byte
        Dim username As String
        Dim isAdmin As Boolean = CBool(data(21))
        Array.ConstrainedCopy(data, 1, usernameBytes, 0, 20)
        Dim usernameLastNonZero As Integer = srFunction.getLastNonZeroIndex(usernameBytes)
        Dim tmpUsername(usernameLastNonZero) As Byte
        Array.Copy(usernameBytes, tmpUsername, usernameLastNonZero + 1)
        username = ASCII.GetString(tmpUsername)
        addRowsToDGV(New Object() {username, isAdmin})

    End Sub
    Private Sub addRowsToDGV(ByVal items As Object())
        If DGV_Users.InvokeRequired Then
            Dim Dgate As New delegateAddRows(AddressOf addRowsToDGV)
            DGV_Users.BeginInvoke(Dgate, New Object() {items})
        Else
            DGV_Users.Rows.Add(items)
        End If
    End Sub
    Private Sub ResetPasswordToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResetPasswordToolStripMenuItem.Click
        Dim sha256 As New SHA256Managed
        Dim selectedRows As DataGridViewSelectedRowCollection = DGV_Users.SelectedRows
        If selectedRows.Count = 0 Then
            MsgBox("Select a user from the box below", MsgBoxStyle.Critical)
            Return
        End If
        Dim username As String = selectedRows.Item(0).Cells.Item(0).Value
        Dim password As String = Interaction.InputBox("Provide a password for the user, its possible for a user to change the password later", "Set password")
        If password = Nothing Then
            Return
        End If
        Dim passwordHash() As Byte = SHA256.ComputeHash(ASCII.GetBytes(password))
        Dim oldPasswordHash(31) As Byte
        password = ""
        srFunction.updatePasswordHash(username, oldPasswordHash, passwordHash)
    End Sub

    Private Sub ChangePrivilegeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ChangePrivilegeToolStripMenuItem.Click
        Dim selectedRows As DataGridViewSelectedRowCollection = DGV_Users.SelectedRows
        If selectedRows.Count = 0 Then
            MsgBox("Select a user from the box below", MsgBoxStyle.Critical)
            Return
        End If
        Dim username As String = selectedRows.Item(0).Cells.Item(0).Value
        Dim value As String = Interaction.InputBox("Type 1 for Administrator and 0 for normal user", "Set privilege")
        If value = Nothing Then
            Return
        End If
        If Not IsNumeric(value) Or (CInt(value) <> 0 And CInt(value) <> 1) Then
            Do
                value = Interaction.InputBox("Type 1 for Administrator and 0 for normal user", "Set privilege")
            Loop Until IsNumeric(value) AndAlso (CInt(value) = 0 Or CInt(value) = 1)
        End If
        srFunction.updateUsersRights(username, CBool(value))
    End Sub

    Private Sub AddNewUserToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AddNewUserToolStripMenuItem.Click
        Dim username As String = Interaction.InputBox("Provide a username less than 20 characters long", "Set username")
        Dim sha256 As New SHA256Managed
        If username = Nothing Then
            Return
        End If
        If username.Length > 20 Then
            Do
                username = Interaction.InputBox("Provide a username less than 20 characters long", "Set username")
            Loop Until username.Length <= 20
        End If
        Dim password As String = Interaction.InputBox("Provide a password for the user, its possible for a user to change the password later", "Set password")
        If password = Nothing Then
            Return
        End If
        Dim passwordHash() As Byte = sha256.ComputeHash(ASCII.GetBytes(password))
        password = ""
        Dim value As String = Interaction.InputBox("Type 1 for Administrator and 0 for normal user", "Set privilege")
        If value = Nothing Then
            Return
        End If
        If value = Nothing Or Not IsNumeric(value) Or (CInt(value) <> 0 And CInt(value) <> 1) Then
            Do
                value = Interaction.InputBox("Type 1 for Administrator and 0 for normal user", "Set privilege")
                If value = Nothing Then
                    Return
                End If
            Loop Until value <> Nothing AndAlso IsNumeric(value) AndAlso (CInt(value) = 0 Or CInt(value) = 1)
        End If
        srFunction.sendNewUser(username, passwordHash, CBool(value))
    End Sub

    Private Sub RemoveUserToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RemoveUserToolStripMenuItem.Click
        Dim selectedRows As DataGridViewSelectedRowCollection = DGV_Users.SelectedRows
        If selectedRows.Count = 0 Then
            MsgBox("Select a user from the box below", MsgBoxStyle.Critical)
            Return
        End If
        Dim username As String = selectedRows.Item(0).Cells.Item(0).Value
        srFunction.removeUser(username)
    End Sub
End Class