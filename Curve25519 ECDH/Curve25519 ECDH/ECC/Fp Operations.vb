Imports System.Numerics
Imports System.Security.Cryptography
Imports System.Threading
Namespace Fp_Operations
    Public Class Fp
        Public p As BigInteger
        Private __a_gen As BigInteger
        Private __max As BigInteger
        Private __prime As BigInteger
        Public Sub New(ByVal p As BigInteger)
            If Test_prime(p, 20) = Errors.MR_P_prime Then
                Me.p = p
            Else
                Throw New Exception("Specified parameter should be prime")
            End If
        End Sub
        Public Sub New(Num_Bytes As Integer)
            Generate_Params(Me.p, Num_Bytes)
        End Sub

        ''' <summary>
        ''' A list of custom errors used in the private methods
        ''' </summary>
        Private Enum Errors
            MR_P_LESS_2 = -100
            MR_P_MOD_2_ZERO = -101
            MR_P_NOT_prime = -103
            MR_P_prime = 100
        End Enum
        ''' <summary>
        ''' So far only generates the domain p which is specifically a prime number for which a prime filed will later operate over
        ''' </summary>
        Public Sub Generate_Params(ByRef p As BigInteger, ByVal Num_Bytes As Integer)
            Dim P_ret As Boolean
            Do
                P_ret = Generate_P(p, 40, Num_Bytes)
            Loop While P_ret = False
        End Sub
        Private Function Generate_P(ByRef p As BigInteger, ByVal k As Integer, ByVal Num_Bytes As Integer) As Boolean
            Dim Temp__prime As BigInteger
            Do
                Temp__prime = New BigInteger(SecureRandom(Num_Bytes))
                If Temp__prime.IsEven = False Then
                    If Test_prime(Temp__prime, k) = Errors.MR_P_prime Then
                        p = Temp__prime
                        Return True
                    End If
                    Return False
                End If
            Loop While Temp__prime Mod 2 = 0
            Return False
        End Function
        ''' <summary>
        ''' Generates a random binary value of lenght Num_Bytes in a byte array
        ''' </summary>
        Private Function SecureRandom(ByVal Num_Bytes As Integer) As Byte()
            Dim RND = New RNGCryptoServiceProvider()
            Dim RawRandomBytes(Num_Bytes) As Byte
            RND.GetBytes(RawRandomBytes)
            Return RawRandomBytes
        End Function
        '''<summary>
        ''' This function uses the Miller-Rabin prime tests as stated on wikipedia where p is the odd number to be tested and k is the accuracy of the test
        ''' </summary>
        Private Function Test_prime(ByVal p As BigInteger, ByVal k As Integer) As Errors
            Dim __Thread As Thread
            Dim s As BigInteger
            Dim d As Integer
            Dim a As BigInteger
            If p < 2 Then
                Return Errors.MR_P_LESS_2
            End If
            If p <> 2 And p Mod 2 = 0 Then
                Return Errors.MR_P_MOD_2_ZERO
            End If
            __max = p - 1
            __prime = p
            __Thread = New Thread(AddressOf Gen_Random_BigInt)
            __Thread.Start()
            s = p - 1
            d = 0
            While s Mod 2 = 0
                s = s / 2
                d += 1
            End While

            For i = 0 To k - 1
                Dim x As BigInteger
                If __Thread.IsAlive Then
                    __Thread.Join()
                End If
                a = __a_gen
                __Thread = New Thread(AddressOf Gen_Random_BigInt)
                __Thread.Start()
                'a = Gen_Random_BigInt(p.ToByteArray.Length - 1, p)
                x = BigInteger.ModPow(a, s, p)
                If x = 1 Or x = p - 1 Then
                    Continue For
                End If
                For q = 1 To d - 1
                    x = BigInteger.ModPow(x, 2, p) 'ModPow (a * b) mod c
                    If x = 1 Then
                        __Thread.Abort()
                        Return Errors.MR_P_NOT_prime
                    End If
                    If x = p - 1 Then
                        Exit For
                    End If
                Next
                If x <> p - 1 Then
                    __Thread.Abort()
                    Return Errors.MR_P_NOT_prime
                End If
            Next
            __Thread.Abort()
            Return Errors.MR_P_prime

        End Function
        ''' <summary>
        ''' Genetares a random BigInteger value that should be positive and less than or equal to the value of prime being passed
        ''' </summary>
        Private Sub Gen_Random_BigInt()
            Dim int As BigInteger
            Dim rng As New RNGCryptoServiceProvider
            Dim bytes(__max.ToByteArray.LongLength) As Byte
            Do
                rng.GetBytes(bytes)
                int = New BigInteger(bytes)
            Loop While int < 2 Or int >= __prime
            __a_gen = int
        End Sub
        ''' <summary>
        ''' Calculates field addition
        ''' </summary>
        Public Function Field_Addition(ByVal A As BigInteger, ByVal B As BigInteger, ByVal P As BigInteger) As BigInteger
            If P = 0 Then
                Return New BigInteger(-999)
            End If
            Dim Q As BigInteger
            Q = New BigInteger(((A + B) Mod P).ToByteArray)
            Return Q
        End Function

        Public Function Field_Subtraction(ByVal A As BigInteger, ByVal B As BigInteger, ByVal P As BigInteger) As BigInteger
            If P = 0 Then
                Return New BigInteger(-999)
            End If
            Dim Q As BigInteger
            Q = New BigInteger(((A - B) Mod P).ToByteArray)
            Return Q
        End Function

        Public Function Field_Mult(ByVal A As BigInteger, ByVal B As BigInteger, ByVal P As BigInteger) As BigInteger
            If P = 0 Then
                Return New BigInteger(-999)
            End If
            Dim Q As BigInteger
            Q = New BigInteger(((A * B) Mod P).ToByteArray)
            Return Q
        End Function

        Public Function Field_Div(ByVal A As BigInteger, ByVal B As BigInteger, ByVal P As BigInteger) As BigInteger
            If P = 0 Then
                Return New BigInteger(-999)
            End If
            Dim Q As BigInteger
            Q = New BigInteger(((A * Point_Operations.Inverse(B, P)) Mod P).ToByteArray)
            Return Q
        End Function
    End Class

End Namespace
