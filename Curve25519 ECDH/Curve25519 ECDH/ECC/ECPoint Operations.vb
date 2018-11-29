Imports System.Numerics
Imports Curve25519_ECDH.Curve
Public Class Point_Operations
    ''' <summary>
    ''' A structure that contains data returned from Extended_GCD function
    ''' </summary>
    Friend Structure RetGCD
        Public x As BigInteger
        Public y As BigInteger
        Public GCD As BigInteger
    End Structure
    ''' <summary>
    ''' Performs addition of two points
    ''' </summary>
    Public Shared Function Addition(ByVal P1 As ECPoint, ByVal P2 As ECPoint) As ECPoint
        'Details about the maths was found on https://en.wikipedia.org/wiki/Elliptic_curve_point_multiplication
        'Tested with the site http://christelbach.com/ECCalculator.aspx
        If P1.IsPointInfinity() Then
            Return P2
        End If
        If P2.IsPointInfinity() Then
            Return P1
        End If
        If P1.p <> P2.p Then
            Return New ECPoint(-99, -99, -99) 'Add custom errors with ENUM
        End If
        If P1.y = -P2.y Then
            Return New ECPoint(-99, -99, -99)
        End If
        Dim Y As BigInteger = -P2.y
        If Y < 0 Then
            Y += P1.p
        End If
        Dim prime As BigInteger
        If P1.x = P2.x And P1.y = Y Then
            Dim ret As New ECPoint(0, 1, P1.p)
            Return ret
        End If
        Dim P3 As New ECPoint(P1)
        Dim l, z As BigInteger
        Dim D As BigInteger = P2.x - P1.x
        If D < 0 Then
            D += P1.p
        End If
        z = Inverse(D, P1.p)
        l = ((P2.y - P1.y) * z) Mod P1.p
        P3.x = ((l * l) Mod P1.p - P1.x - P2.x) Mod P1.p
        P3.y = ((l * (P1.x - P3.x)) Mod P1.p - P1.y) Mod P1.p
        'Tested the code without the peice below and found that the input (10,10,47) and (20,20,47) returned wrong values found out that the difference between each was the prime number so if its less than zero we add the prime whcih seems to work
        If P3.x < 0 Then
            P3.x += P1.p
        End If
        If P3.y < 0 Then
            P3.y += P1.p
        End If
        Return P3
    End Function
    ''' <summary>
    ''' A way of doubling a EC Point
    ''' </summary>
    Public Shared Function PDouble(ByVal P As ECPoint, ByVal a As Domain_Parameters) As ECPoint
        'Details about the maths was found on https://en.wikipedia.org/wiki/Elliptic_curve_point_multiplication
        'Tested with the site http://christelbach.com/ECCalculator.aspx
        If P.p = 0 And a.Fp.p = 0 Then
            Return New ECPoint(-99, -99, -99)
        End If
        'If a.a = 0 Then
        '    Return New ECPoint(-99, -99, -99)
        'End If
        If P.IsPointInfinity() Then
            Return P
        End If
        Dim Y As BigInteger = -P.y
        If Y < 0 Then
            Y += P.p
        End If
        If P.y = Y Then
            Dim ret As New ECPoint(0, 1, P.p)
            Return ret
        End If
        Dim prime As BigInteger
        If P.p = 0 Then
            prime = a.Fp.p
        Else
            prime = P.p
        End If
        Dim Q As New ECPoint(P)
        Dim l, z As BigInteger
        z = Inverse(2 * P.y, prime)
        l = ((((3 * P.x * P.x) Mod prime + a.a) Mod prime) * z) Mod prime
        Q.x = ((l * l) Mod prime - 2 * P.x) Mod prime     'accidental set (2 * p.x) to (2 * prime)
        Q.y = ((l * (P.x - Q.x)) Mod prime - P.y) Mod prime
        'This code was tested and the same problem again with negative values for x and y so we must add the prime to p to correct that
        If Q.x < 0 Then
            Q.x += prime
        End If
        If Q.y < 0 Then
            Q.y += prime
        End If
        Return Q
    End Function
    ''' <summary>
    ''' Performs point multiplication with a scalar
    ''' </summary>
    Public Shared Function Scalar_Mult(ByVal P As ECPoint, ByVal a As Domain_Parameters, ByVal Scalar As BigInteger) As ECPoint
        'Details about the maths was found on https://en.wikipedia.org/wiki/Elliptic_curve_point_multiplication
        'Tested with the site http://christelbach.com/ECCalculator.aspx
        If P.p = 0 And a.Fp.p = 0 Then
            Return New ECPoint(-99, -99, -99)
        End If
        'If a.a = 0 Then
        '    Return New ECPoint(-99, -99, -99)
        'End If
        If P.IsPointInfinity Then
            Return P
        End If
        Dim prime As BigInteger
        If P.p = 0 Then
            prime = a.Fp.p
        Else
            prime = P.p
        End If

        Dim N, S As New ECPoint(P)
        S = New ECPoint(0, 1, prime)
        Dim bytearr(a.Bitlength - 1) As Byte
        Dim k As New BigInteger(Scalar.ToByteArray)
        For i = bytearr.Count - 1 To 0 Step -1
            bytearr(i) = k Mod 2
            k /= 2
        Next
        If bytearr(bytearr.Count - 1) = 1 Then
            S = N
        End If
        For i = bytearr.Count - 2 To 0 Step -1
            N = PDouble(N, a)
            If bytearr(i) = 1 Then
                S = Addition(N, S)
            End If
        Next
        Return S
    End Function
    ''' <summary>
    ''' Calcualtes the Greatest Common Divisor of two numbers
    ''' </summary>
    Private Shared Function Extended_GCD(ByVal a As BigInteger, b As BigInteger) As RetGCD
        'The pseudocode was found on https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm#Pseudocode
        'Tested with the site http://planetcalc.com/3298/
        Dim r, o_r As BigInteger
        Dim t, o_t As BigInteger
        Dim s, o_s As BigInteger
        Dim p, q As BigInteger
        Dim ret As RetGCD
        s = 0
        t = 1
        r = b
        o_s = 1
        o_t = 0
        o_r = a
        While r <> 0
            q = o_r / r
            p = r
            r = o_r - q * p
            o_r = p
            p = s
            s = o_s - q * p
            o_s = p
            p = t
            t = o_t - q * p
            o_t = p
        End While
        ret.x = o_s
        ret.y = o_t
        ret.GCD = o_r
        Return ret
    End Function
    ''' <summary>
    ''' Performs the modular multiplitcative inverse of a number so we can use multiplication instead of division in our arithmetic
    ''' </summary>
    Public Shared Function Inverse(ByVal a As BigInteger, ByVal p As BigInteger) As BigInteger
        'Not explicitely tested but works
        Dim ret As RetGCD
        ret = Extended_GCD(a, p)
        If ret.GCD <> 1 Then
            Return 0
        End If
        Return ret.x Mod p
    End Function
End Class
