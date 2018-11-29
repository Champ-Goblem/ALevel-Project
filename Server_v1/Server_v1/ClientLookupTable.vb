Public Class ClientLookupTable
    Private clientHashTable As Hashtable
    Public Structure tableValues
        Public username As String
        Public publicKey As Curve25519_ECDH.ECPoint
        Public clientReference As MTClientReciever
    End Structure
    'An enum of errors depending on the outcome of certain actions
    Public Enum clientLookupTableErrors
        CLIENT_DOES_NOT_EXIST
        TABLE_CONTAINS_CLIENT
        NO_ERROR
        CLIENT_CONNECTED
        CLIENT_NOT_CONNECTED
    End Enum
    Public Sub New()
        clientHashTable = New Hashtable()
    End Sub
    Public Function AddNewClient(ByVal IPAddr As String, ByVal value As tableValues) As clientLookupTableErrors
        'return an error if the table already contains the client
        If clientHashTable.ContainsKey(IPAddr) Then
            Return clientLookupTableErrors.TABLE_CONTAINS_CLIENT
        End If
        'else add the new client
        clientHashTable.Add(IPAddr, value)
        Return clientLookupTableErrors.NO_ERROR
    End Function
    Public Function CheckClientConnected(ByVal IPAddr As String) As clientLookupTableErrors
        'return that the client is connected
        If clientHashTable.ContainsKey(IPAddr) Then
            Return clientLookupTableErrors.CLIENT_CONNECTED
        End If
        'Client probably not connected
        Return clientLookupTableErrors.CLIENT_NOT_CONNECTED
    End Function
    Public Function RemoveClient(ByVal IPAddr As String, ByVal clNumber As Integer) As clientLookupTableErrors
        'Check if the client doesnt exist
        If Not clientHashTable.ContainsKey(IPAddr) Then
            Return clientLookupTableErrors.CLIENT_DOES_NOT_EXIST
        End If
        'We need to update the clNumber of the client as a row has now been removed from the table it could change the position of the client in the table without updating the clNumber
        For Each entries As DictionaryEntry In clientHashTable
            Dim values As tableValues = entries.Value
            Dim ref As MTClientReciever = values.clientReference
            If ref.cliNumber > clNumber Then
                ref.removeOneFromClNumber()
            End If
        Next
        'else remove the client
        clientHashTable.Remove(IPAddr)
        Return clientLookupTableErrors.NO_ERROR
    End Function
    Public Function GetClientDetails(ByVal IPAddr As String) As Object
        If Not clientHashTable.ContainsKey(IPAddr) Then
            Return clientLookupTableErrors.CLIENT_DOES_NOT_EXIST
        End If
        'Get the values provided by looking up the IP address in the hashtable
        Dim values As tableValues = clientHashTable(IPAddr)
        Return values
    End Function
    Public Function GetClientPublicKey(ByVal IPAddr As String) As Object
        If Not clientHashTable.ContainsKey(IPAddr) Then
            Return clientLookupTableErrors.CLIENT_DOES_NOT_EXIST
        End If
        'return the public key
        Dim values As tableValues = clientHashTable(IPAddr)
        Return values.publicKey
    End Function
    Public Function GetClientUname(ByVal IPAddr As String) As Object
        If Not clientHashTable.ContainsKey(IPAddr) Then
            Return clientLookupTableErrors.CLIENT_DOES_NOT_EXIST
        End If
        'returns the username of the client
        Dim values As tableValues = clientHashTable(IPAddr)
        Return values.username
    End Function
    Public Function GetClientReference(ByVal IPAddr As String) As Object
        If Not clientHashTable.ContainsKey(IPAddr) Then
            Return clientLookupTableErrors.CLIENT_DOES_NOT_EXIST
        End If
        'returns the reference to the MTClientReciever
        Dim values As tableValues = clientHashTable(IPAddr)
        Return values.clientReference
    End Function
    Public Sub CloseAllClientConnections()
        'loop through all entrieds and close the connections
        For Each entries As DictionaryEntry In clientHashTable
            Dim values As tableValues = entries.Value
            values.clientReference.closeClientConnection(False)
        Next
        clientHashTable.Clear()
    End Sub
    Public Function GetConnectedClients() As tableValues()
        'Get all of the clients that are connected
        Dim loggenOnUsers(clientHashTable.Count - 1) As tableValues
        Dim counter As Integer = 0
        For Each entries As DictionaryEntry In clientHashTable
            loggenOnUsers(0) = entries.Value
            counter += 1
        Next
        Return loggenOnUsers
    End Function
    Public Function ResolveUsernameToIP(ByVal username As String) As String
        'Loop through all of the entries until we find the value for username that equals the one provided
        For Each entries As DictionaryEntry In clientHashTable
            Dim values As tableValues = entries.Value
            If values.username = username Then
                'return the corresponding IP address when we find the username
                Return entries.Key
            End If
        Next
        'return nothing if nothing is found
        Return Nothing
    End Function
    Public Sub sendDisconnectedClientBroadcast(ByVal disconnectedClientUsername() As Byte)
        '| message type | username |
        'This goes through all of the currently connected clients, gets their reference and sends the username of the client that disconnected
        Const usernameLen As Integer = 20
        Const value As Integer = 220 'DC in hex
        Dim message(usernameLen) As Byte
        message(0) = CByte(value)
        disconnectedClientUsername.CopyTo(message, 1)
        For Each entries As DictionaryEntry In clientHashTable
            Dim values As tableValues = entries.Value
            Dim ref As MTClientReciever = values.clientReference
            ref.sendData(message)
        Next
    End Sub
    Public Sub sendConnectedClientBroadcast(ByVal connectedClientUsername() As Byte, ByVal pkx() As Byte, ByVal pky() As Byte)
        '| message type | username | x len | y len | pkx | pky |
        'This goes through all of the currently connected clients, gets their reference and sends the username of the client that connected
        Const usernameLen As Integer = 20
        Const value As Integer = 204 'CC in hex
        For Each entries As DictionaryEntry In clientHashTable
            Dim values As tableValues = entries.Value
            Dim ref As MTClientReciever = values.clientReference
            Dim message(usernameLen + pkx.Length + pky.Length + 2) As Byte
            message(0) = CByte(value)
            connectedClientUsername.CopyTo(message, 1)
            message(21) = pkx.Length
            message(22) = pky.Length
            pkx.CopyTo(message, 23)
            pky.CopyTo(message, 23 + pkx.Length)
            ref.sendData(message)
        Next
    End Sub
End Class
