Imports System.Numerics
Imports System.Security.Cryptography
Imports Curve25519_ECDH.Curve
Imports System.Reflection.MethodInfo
Imports Error_Handler.Events_Handler
Namespace Crypto
    Public Class ECIES
        Public Shared G As Integer
        'Const hLen As Integer = 32
        Private Structure HKDF
            Public Secret01 As BigInteger
            Public Secret02 As BigInteger
            Public didError As Boolean
        End Structure
        Public Structure TEncryption
            Public Data() As Byte
            Public IV() As Byte
            Public publicKey As ECPoint
            Public HMAC() As Byte
            Public didError As Boolean
        End Structure

        ''' <summary>
        ''' A hash function which turns the x value of the public key into two secret keys
        ''' </summary>
        Private Function hash_KDF(ByVal Secret As ECPoint) As HKDF
            Dim hash256 As New SHA256Managed
            Dim hashValue As Byte()
            Dim tmpSec() As Byte
            Dim ret As New HKDF
            If Secret.IsNull() Then
                errorHappened(Secret, "The value of the secret is Null", 1, GetCurrentMethod.Name)
                ret.didError = True
                Return ret
            End If
            tmpSec = insert_Byte(Secret.x.ToByteArray, Convert.ToByte(1), 0)
            hashValue = hash256.ComputeHash(tmpSec)
            ret.Secret01 = New BigInteger(hashValue)
            tmpSec = insert_Byte(Secret.x.ToByteArray, Convert.ToByte(2), 0)
            hashValue = hash256.ComputeHash(tmpSec)
            ret.Secret02 = New BigInteger(hashValue)
            Return ret
        End Function
        ''' <summary>
        ''' Inserts a byte at a set location in the array
        ''' </summary>
        Private Function insert_Byte(ByVal Array() As Byte, ByVal Value As Byte, ByVal Position As Integer) As Byte()
            Dim newarr(Array.Count) As Byte
            Dim ins As Boolean
            For i = 0 To newarr.Count - 1
                If i = Position Then
                    newarr(i) = Value
                    ins = True
                Else
                    If ins = False Then
                        newarr(i) = Array(i)
                    Else
                        newarr(i) = Array(i - 1)
                    End If
                End If
            Next
            Return newarr
        End Function
        Private Function addLenghtToEnd(ByVal Array() As Byte) As Byte()
            Dim newarr(Array.Count + 3) As Byte
            Dim len() As Byte = BitConverter.GetBytes(Array.Length)
            Array.CopyTo(newarr, 0)
            len.CopyTo(newarr, Array.Length)
            Return newarr
        End Function
        ''' <summary>
        ''' Check if all the values in an array are zero
        ''' </summary>
        Private Function isArrayZeroed(ByVal byteArray() As Byte) As Boolean
            For i = 0 To byteArray.Length - 1
                If byteArray(i) <> 0 Then
                    Return False
                End If
            Next
            Return True
        End Function
        ''' <summary>
        ''' Compares two arrays of the same length
        ''' </summary>
        Private Function compareArrays(ByVal Array1() As Byte, ByVal Array2() As Byte) As Boolean
            If Array1.Count <> Array2.Count Or Array1.Count = 0 Then
                Return False
            End If
            For i = 0 To Array1.Count - 1
                If Array1(i) <> Array2(i) Then
                    Return False
                End If
            Next
            Return True
        End Function
        'Public Function Encrypt_AES_256(ByVal plaintext() As Byte, ByVal CurveName As String, ByVal CurveXMLFilepath As String, ByVal Recipient_Public_key As ECPoint) As TEncryption
        Public Function Encrypt_AES_256(ByVal plaintext() As Byte, ByVal localkey As Keys, ByVal Curve As Weierstrass_Curve, ByVal Recipient_Public_key As ECPoint) As TEncryption
            Dim ECIES_AES_Return As New TEncryption
            If IsNothing(plaintext) Or IsNothing(localkey) Or IsNothing(Curve) Or IsNothing(Recipient_Public_key) Then
                errorHappened(Me, "One of the parameters was Null", 1, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            If Recipient_Public_key.IsNull Or Recipient_Public_key.IsPointInfinity Then
                errorHappened(Recipient_Public_key, "The recipent key failed NullReference validation", 1, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            If localkey.PrivateKey.IsZero Or localkey.PublicKey.IsNull Then
                errorHappened(localkey, "The client keys failed NullReference validation", 1, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            If Curve.Curve_ID = Nothing Then
                errorHappened(Curve, "The curve failed NullReference validation", 1, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            If Recipient_Public_key.IsPointOnCurve(Curve.Parameters) = False Then
                errorHappened(Recipient_Public_key, "Provided public key is not a point on the curve", 4, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            Dim ex As errorEnum = Domain_Parameters.Validate_Params(Curve.Parameters)
            If Not ex = 0 Then
                errorHappened(Curve.Parameters, "Parameters did not pass validation", ex, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            If IsNothing(plaintext) Then
                errorHappened(plaintext, "The plaintext is empty", 1, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            Dim secretKey As New ECPoint(0, 0)
            Dim extSecret As New HKDF
            Dim AES256 As New AES_Encrypt
            Dim CBC_AES_Return As New AES.CBC_Values
            Dim CBC_MAC_Return As New AES.CBC_Values
            ECIES_AES_Return.publicKey = localkey.PublicKey
            secretKey = ECDH.generate_SharedSecret(localkey.PrivateKey, Recipient_Public_key, Curve.Parameters)
            If secretKey.isErroneous() Then
                errorHappened(secretKey, "An error occured whilst generating shared secret", 7, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            extSecret = hash_KDF(secretKey)
            If extSecret.didError = True Then
                writeDebugLog("Returning from sub, HKDF had an error", GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            CBC_AES_Return = AES256.Encrypt_Sign_AES_256_CBC(plaintext, AES.Convert_BigInteger_ToByteArray(extSecret.Secret01))
            If CBC_AES_Return.didError Then
                writeDebugLog("Returning from sub, ENCRYPT_AES_CBC had an error", GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            ECIES_AES_Return.Data = CBC_AES_Return.Data
            ECIES_AES_Return.IV = CBC_AES_Return.IV
            Dim extData() As Byte = addLenghtToEnd(CBC_AES_Return.Data)
            CBC_MAC_Return = AES256.Encrypt_Sign_AES_256_CBC(extData, AES.Convert_BigInteger_ToByteArray(extSecret.Secret02), True)
            If CBC_AES_Return.didError Then
                writeDebugLog("Returning to main, MAC generation had an error", GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            'We can ignore CBC_MAC_Return.IV because that should be 0 as the IV when calculating a MAC is zero, we can instead use it for error checking
            If isArrayZeroed(CBC_MAC_Return.IV) Then
                ECIES_AES_Return.HMAC = CBC_MAC_Return.Data
            Else
                errorHappened(CBC_MAC_Return, "IV is not zero", 5, GetCurrentMethod.Name)
                ECIES_AES_Return.didError = True
                Return ECIES_AES_Return
            End If
            ECIES_AES_Return.didError = False
            Return ECIES_AES_Return
        End Function
        Public Function Decrypt_AES_256(ByVal CBC_AES As TEncryption, ByVal localkey As Keys, ByVal Curve As Weierstrass_Curve, ByVal Recipient_Public_key As ECPoint) As Byte()
            If IsNothing(CBC_AES) Or IsNothing(localkey) Or IsNothing(Curve) Or IsNothing(Recipient_Public_key) Then
                errorHappened(Me, "One of the parameters was Null", 1, GetCurrentMethod.Name)
                Return Nothing
            End If
            If CBC_AES.Data.Count = 0 Then
                errorHappened(CBC_AES, "Ciphertext was Null", 1, GetCurrentMethod.Name)
                Return Nothing
            End If
            If localkey.PublicKey.isErroneous Or localkey.PublicKey.IsNull Or localkey.PrivateKey.IsZero Or localkey.PublicKey.IsPointInfinity Then
                errorHappened(localkey, "Provided keys were Null", 1, GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim ex As errorEnum = Domain_Parameters.Validate_Params(Curve.Parameters)
            If ex <> 0 Then
                errorHappened(Curve.Parameters, "Curve parameters failed validation", ex, GetCurrentMethod.Name)
                Return Nothing
            End If
            If Not localkey.PublicKey.IsPointOnCurve(Curve.Parameters) Then
                errorHappened(localkey, "Point was not calculated correctly, it does not exists as a point on the curve", 4, GetCurrentMethod.Name)
                Return Nothing
            End If
            If Recipient_Public_key.isErroneous Or Recipient_Public_key.IsNull Then
                errorHappened(Recipient_Public_key, "Public key of the recieving party is Null", 1, GetCurrentMethod.Name)
                Return Nothing
            End If
            If Recipient_Public_key.IsPointInfinity Then
                errorHappened(Recipient_Public_key, "Public key was the point at infinity", 3, GetCurrentMethod.Name)
                Return Nothing
            End If
            If Not Recipient_Public_key.IsPointOnCurve(Curve.Parameters) Then
                errorHappened(Recipient_Public_key, "Public key was not on the give curve", 4, GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim secretKey As New ECPoint(0, 0)
            Dim extSecret As New HKDF
            Dim AES256 As New AES_Encrypt
            Dim DAES256 As New AES_Decrypt
            Dim CBC_AES_Return As New AES.CBC_Values
            Dim CBC_MAC_Return As New AES.CBC_Values
            secretKey = ECDH.generate_SharedSecret(localkey.PrivateKey, Recipient_Public_key, Curve.Parameters)
            If secretKey.isErroneous() Then
                errorHappened(secretKey, "An error occured whilst generating shared secret", 7, GetCurrentMethod.Name)
                Return Nothing
            End If
            extSecret = hash_KDF(secretKey)
            If extSecret.didError = True Then
                writeDebugLog("Returning from sub, HKDF had an error", GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim extData() As Byte = addLenghtToEnd(CBC_AES.Data)
            CBC_MAC_Return = AES256.Encrypt_Sign_AES_256_CBC(extData, AES.Convert_BigInteger_ToByteArray(extSecret.Secret02), True)
            If CBC_AES_Return.didError Then
                writeDebugLog("Returning to main, MAC generation had an error", GetCurrentMethod.Name)
                Return Nothing
            End If
            'We can ignore CBC_MAC_Return.IV because that should be 0 as the IV when calculating a MAC is zero, we can instead use it for error checking
            If Not isArrayZeroed(CBC_MAC_Return.IV) Then
                errorHappened(CBC_MAC_Return, "IV  is not zero", 5, GetCurrentMethod.Name)
                Return Nothing
            End If
            If Not compareArrays(CBC_AES.HMAC, CBC_MAC_Return.Data) Then
                writeDebugLog("Message MAC did not equal the MAC provided by the sender, either the data has been tampered with or something happened in transit", GetCurrentMethod.Name)
                Return Nothing
            End If
            CBC_AES_Return = DAES256.Decrypt_AES_256_CBC(CBC_AES.Data, AES.Convert_BigInteger_ToByteArray(extSecret.Secret01), CBC_AES.IV)
            If CBC_AES_Return.didError Then
                writeDebugLog("Returning from sub, DECRYPT_AES_CBC had an error", GetCurrentMethod.Name)
                Return Nothing
            End If
            Return CBC_AES_Return.Data
        End Function
        Private Function HMAC_SHA256(ByVal Key As HKDF, ByVal Message As Byte()) As Byte()
            'Currently Not working, but using CBC-MAC instead
            If IsNothing(Message) Then
                errorHappened(Message, "Message is zero", 1, GetCurrentMethod.Name)
                Return Nothing
                'Throw New Exception("Message is zero")
            End If
            Dim Secret() As Byte = Key.Secret02.ToByteArray
            If Secret.Length > 32 Then
                errorHappened(Secret, "Length greater than 32", 6, GetCurrentMethod.Name)
                Return Nothing
                'Throw New Exception("The key length is greater than 256bit")
            End If
            If Secret.Length < 32 Then
                errorHappened(Secret, "Length less than 32", 6, GetCurrentMethod.Name)
                Return Nothing
                'Throw New Exception("The key length is less than 256bit")
            End If
            Dim hash256 As New SHA256Managed
            Dim o_pad(31) As Byte
            Dim i_pad(31) As Byte
            Dim FinalVal(63) As Byte

            Dim i As Integer = 0
            For i = 0 To 31
                o_pad(i) = Secret(i) Xor 92
                i_pad(i) = Secret(i) Xor 54
            Next
            i = 0
            Dim hashEntry(63) As Byte
            For i = 0 To 31 + Message.Length
                If i < 31 Then
                    hashEntry(i) = i_pad(i)
                Else
                    hashEntry(i) = Message(i Mod Message.Length)
                End If
            Next
            i = 0
            Dim hashVal() As Byte = hash256.ComputeHash(hashEntry)
            For i = 0 To 63
                If i < 31 Then
                    FinalVal(i) = o_pad(i)
                Else
                    FinalVal(i) = hashVal(i Mod 32)
                End If
            Next
            Return hash256.ComputeHash(hashVal)
        End Function
    End Class
End Namespace