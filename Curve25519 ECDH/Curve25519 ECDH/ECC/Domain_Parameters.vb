Imports System.Numerics
Imports Error_Handler.Events_Handler
Namespace Curve
    Public Class Domain_Parameters
        Public Bitlength As Integer
        Public Fp As Fp_Operations.Fp
        Public a As BigInteger
        Public b As BigInteger
        'Public s As BigInteger
        Public Base As ECPoint
        Public n As BigInteger
        Public h As Integer
        'Infinity point has the homogeneous coords (0:1:0), or negative y (0:-1:0)

        Public Sub New(ByVal Bitlenght As Integer, ByVal p As Fp_Operations.Fp, ByVal a As BigInteger, ByVal b As BigInteger, ByVal Base As ECPoint, ByVal n As BigInteger, ByVal h As Integer)
            Me.Bitlength = Bitlength
            Me.Fp = p
            Me.a = a
            Me.b = b
            'Me.s = s Removed the need for a seed, might be needed if we do our own curve generation
            Me.Base = Base
            Me.n = n
            Me.h = h
        End Sub
        Public Shared Function Validate_Params(ByVal Params As Domain_Parameters) As errorEnum
            'We dont need to validate Fp as that is done uppon creation
            'Dim p As Fp_Operations.Fp = Params.Fp
            Dim bitLength As Integer = Params.Bitlength
            Dim a As BigInteger = Params.a
            Dim b As BigInteger = Params.b
            Dim Base As ECPoint = Params.Base
            Dim n As BigInteger = Params.n
            Dim h As BigInteger = Params.h
            'If New BitArray(Params.Fp.p.ToByteArray).Length - 1 <> bitLength Then
            '    System.Diagnostics.Debug.Print(New BitArray(Params.Fp.p.ToByteArray).Length - 1)
            '    Return New Exception("Prime integer is not 256 bits")
            'End If
            'If New BitArray(a.ToByteArray).Length <> bitLength Or New BitArray(b.ToByteArray).Length <> bitLength Then
            '    'System.Diagnostics.Debug.Print(New BitArray(a.ToByteArray).Length - 1, ", ", New BitArray(b.ToByteArray).Length - 1)
            '    Return 10
            'End If
            If b = 0 Then
                Return 11
            End If
            If 4 * BigInteger.Pow(a, 3) + 27 * BigInteger.Pow(b, 2) = 0 Then
                Return 12
            End If
            If Base.IsPointInfinity Then
                Return 3
            End If
            If h <> 1 Then
                Return 13
            End If
            'If New BitArray(Base.x.ToByteArray).Length <> 256 Or New BitArray(Base.y.ToByteArray).Length <> 256 Then
            '    System.Diagnostics.Debug.Print(New BitArray(Base.x.ToByteArray).Length)
            '    Return New Exception("The base point is not of the correct bitlenght")
            'End If
            Return Nothing
        End Function
    End Class
End Namespace
