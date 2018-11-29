Imports Curve25519_ECDH.Curve
Imports Curve25519_ECDH.Crypto
Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'Testing out my ECIES class
        Dim ECIES_AES As New ECIES
        Dim K, R As New Keys
        Dim Curve As New Weierstrass_Curve("Curves.xml", "Brain-P256r1")
        Dim p As String = "hello"
        Dim CBC_AES_Return As New ECIES.TEncryption
        Dim Rt() As Byte
        K = ECDH.generate_Keys(Curve.Parameters)
        R = ECDH.generate_Keys(Curve.Parameters)
        CBC_AES_Return = ECIES_AES.Encrypt_AES_256(AES.Convert_String_ToByteArray(p), K, Curve, R.PublicKey)
        Rt = ECIES_AES.Decrypt_AES_256(CBC_AES_Return, K, Curve, R.PublicKey)
        Dim s As String = AES.Convert_ByteArray_ToString(Rt)
        ''Testing out my AES class and testing out private and public key validation
        'Dim Curve As New Weistrass_Curve("Curves.xml", "Brain-P256r1")
        'Dim keyA As Keys
        'Dim keyB As Keys
        'Dim SecretA As ECPoint
        'Dim SecretB As ECPoint
        'Dim Secret As ECIES
        ''Dim State3(,,) As Byte
        'Dim State2(3, 3) As Byte
        ''Dim OutState(,) As Byte
        'keyA = ECDH.generate_Keys(Curve.Parameters)
        'keyB = ECDH.generate_Keys(Curve.Parameters)
        'SecretA = ECDH.generate_SharedSecret(keyA.PrivateKey, keyB.PublicKey, Curve.Parameters)
        'SecretB = ECDH.generate_SharedSecret(keyB.PrivateKey, keyA.PublicKey, Curve.Parameters)
        'APriv.Text = keyA.PrivateKey.ToString
        'APubx.Text = keyA.PublicKey.x.ToString
        'APuby.Text = keyA.PublicKey.y.ToString
        'BPriv.Text = keyB.PrivateKey.ToString
        'BPubx.Text = keyB.PublicKey.x.ToString
        'BPuby.Text = keyB.PublicKey.y.ToString
        'SecAx.Text = SecretA.x.ToString
        'SecAy.Text = SecretA.y.ToString
        'SecBx.Text = SecretB.x.ToString
        'SecBy.Text = SecretB.y.ToString
        'Secret = ECIES.hash_KDF(SecretA)
        'Sec01.Text = Secret.Secret01.ToString
        'Sec02.Text = Secret.Secret02.ToString
        'Dim AES256 As New AES_Encrypt
        'Dim DAES256 As New AES_Decrypt
        'Dim ret As Aes.CBC_Values
        'Dim ret2 As Aes.CBC_Values
        'ret = AES256.Encrypt_AES_256_CBC(AES256.Convert_String_ToByteArray(Plaintext.Text), AES256.Convert_BigInteger_ToByteArray(Secret.Secret01))
        'ret2 = DAES256.Decrypt_AES_256_CBC(ret.Data, AES256.Convert_BigInteger_ToByteArray(Secret.Secret01), ret.IV)
        'Chiphertext.Text = AES256.Convert_ByteArray_ToString(ret.Data)
        'Decrypttext.Text = AES256.Convert_ByteArray_ToString(ret2.Data)
        ''State3 = AES256.convertState(plaintext.ToByteArray)
        ''AES256.Copy_3to2(State3, State2, 0, 4, 4)
        ''OutState = AES256.Encrypt_AES(State2, key.ToByteArray, 256)
        ''ret = AES256.Encrypt_AES_256_CBC(AES256.Convert_BigInteger_ToByteArray(plaintext), AES256.Convert_BigInteger_ToByteArray(key))
        ''ret = AES256.Encrypt_AES_256_CBC(plaintext, key)
        ''Array.Reverse(ret.Data)
        ''ret2 = DAES256.Decrypt_AES_256_CBC(ret.Data, AES256.Convert_BigInteger_ToByteArray(key), ret.IV)
        ''ret2 = DAES256.Decrypt_AES_256_CBC(ret.Data, key, ret.IV)
    End Sub
End Class
