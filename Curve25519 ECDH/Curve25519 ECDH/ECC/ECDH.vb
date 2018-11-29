Imports Curve25519_ECDH.Curve
Imports System.Numerics
Imports System.Security.Cryptography
Imports Error_Handler.Events_Handler
Imports System.Reflection.MethodInfo
Namespace Crypto
    Public Structure Keys
        Public PublicKey As ECPoint
        Public PrivateKey As BigInteger
    End Structure
    Public Class ECDH
        Public Shared Function generate_Keys(ByVal Param As Domain_Parameters) As Keys
            Dim validateError As errorEnum = Domain_Parameters.Validate_Params(Param)
            If validateError <> Nothing Then
                errorHappened(Param, "Parameters are not a valid curve", validateError, GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim __PrivateKey As BigInteger
            Dim __PublicKey As ECPoint
            Dim d As BigInteger
            Dim rng As New RNGCryptoServiceProvider
            Dim bytes(Param.n.ToByteArray.Length) As Byte
            Do
                rng.GetBytes(bytes)
                d = New BigInteger(bytes)
                If d.Sign = -1 Then
                    d = d * -1
                End If
            Loop While d >= Param.n
            __PrivateKey = d
            __PublicKey = Point_Operations.Scalar_Mult(Param.Base, Param, d)
            If __PublicKey.IsPointOnCurve(Param) = False Then
                errorHappened(__PublicKey, "The generated public key was not valid for the provided parameters", errorEnum.POINT_NOT_ON_CURVE, GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim Ret As New Keys
            Ret.PublicKey = __PublicKey
            Ret.PrivateKey = __PrivateKey
            __PrivateKey = BigInteger.Zero()
            d = BigInteger.Zero()
            bytes = {0}
            Return Ret
        End Function
        Public Shared Function regeneratePublicKey(ByVal Param As Domain_Parameters, ByVal privateKey As BigInteger) As Keys
            Dim validateError As errorEnum = Domain_Parameters.Validate_Params(Param)
            If validateError <> Nothing Then
                errorHappened(Param, "Parameters are not a valid curve", validateError, GetCurrentMethod.Name)
                Return Nothing
            End If
            If privateKey = Nothing Then
                errorHappened(privateKey, "Private key was null", errorEnum.NULL_REFERECE_EXCEPTION, GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim __PrivateKey As BigInteger
            Dim __PublicKey As ECPoint
            Dim d As BigInteger = privateKey
            Dim rng As New RNGCryptoServiceProvider
            Dim bytes(Param.n.ToByteArray.Length) As Byte
            __PrivateKey = d
            __PublicKey = Point_Operations.Scalar_Mult(Param.Base, Param, d)
            If __PublicKey.IsPointOnCurve(Param) = False Then
                errorHappened(__PublicKey, "The generated public key was not valid for the provided parameters", errorEnum.POINT_NOT_ON_CURVE, GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim Ret As New Keys
            Ret.PublicKey = __PublicKey
            Ret.PrivateKey = __PrivateKey
            __PrivateKey = BigInteger.Zero()
            d = BigInteger.Zero()
            bytes = {0}
            Return Ret
        End Function
        Public Shared Function generate_SharedSecret(ByVal PrivateKey As BigInteger, ByVal PublicKey As ECPoint, ByVal Curve_Parameters As Domain_Parameters) As ECPoint
            Dim Secret As ECPoint
            If PrivateKey = 0 Or PrivateKey = Nothing Then
                errorHappened(PrivateKey, "Private key was null or 0", errorEnum.NULL_REFERECE_EXCEPTION, GetCurrentMethod.Name)
                Return New ECPoint(-99, -99, -99)
            End If
            'If PublicKey.IsPointInfinity Then
            '    errorHappened(PublicKey, "Public key point was infinity", errorEnum.POINT_INFINITY, GetCurrentMethod.Name)
            '    Return New ECPoint(-99, -99, -99)
            'End If
            Dim validateParamsError As errorEnum = Domain_Parameters.Validate_Params(Curve_Parameters)
            If validateParamsError <> Nothing Then
                errorHappened(Curve_Parameters, "Provided parameters not valid as a curve", validateParamsError, GetCurrentMethod.Name)
                Return New ECPoint(-99, -99, -99)
            End If
            If PublicKey.IsPointOnCurve(Curve_Parameters) = False Then
                errorHappened(PublicKey, "Point was not found on provided curve", errorEnum.POINT_NOT_ON_CURVE, GetCurrentMethod.Name)
                Return New ECPoint(-99, -99, -99)
            End If
            Secret = Point_Operations.Scalar_Mult(PublicKey, Curve_Parameters, PrivateKey)
            If Secret.IsPointInfinity Then
                errorHappened(Secret, "Generated secret key was at point of infinity", errorEnum.POINT_INFINITY, GetCurrentMethod.Name)
                Return New ECPoint(-99, -99, -99)
            End If
            Return Secret
        End Function
    End Class
End Namespace
