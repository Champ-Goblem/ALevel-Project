Public Class UserLookupTable
    Private lookupTable As Hashtable
    Public Enum clientLookupTableErrors
        CLIENT_DOES_NOT_EXIST
        TABLE_CONTAINS_CLIENT
        NO_ERROR
        CLIENT_CONNECTED
        CLIENT_NOT_CONNECTED
    End Enum
    Public Sub New()
        lookupTable = New Hashtable
    End Sub
    Public Function addClient(ByVal username As String, ByVal publicKey As Curve25519_ECDH.ECPoint) As clientLookupTableErrors
        If lookupTable.ContainsKey(username) Then
            Return clientLookupTableErrors.TABLE_CONTAINS_CLIENT
        End If
        lookupTable.Add(username, publicKey)
        Return clientLookupTableErrors.NO_ERROR
    End Function
    Public Function removeClient(ByVal username As String) As clientLookupTableErrors
        If Not lookupTable.ContainsKey(username) Then
            Return clientLookupTableErrors.CLIENT_DOES_NOT_EXIST
        End If
        lookupTable.Remove(username)
        Return clientLookupTableErrors.NO_ERROR
    End Function
    Public Function getClientsPK(ByVal username As String) As Curve25519_ECDH.ECPoint
        If Not lookupTable.ContainsKey(username) Then
            Return Nothing
        End If
        Return lookupTable(username)
    End Function
    Public Function updateClientsPK(ByVal username As String, ByVal publicKey As Curve25519_ECDH.ECPoint) As clientLookupTableErrors
        If Not lookupTable.ContainsKey(username) Then
            Return clientLookupTableErrors.CLIENT_DOES_NOT_EXIST
        End If
        lookupTable(username) = publicKey
        Return clientLookupTableErrors.NO_ERROR
    End Function
End Class
