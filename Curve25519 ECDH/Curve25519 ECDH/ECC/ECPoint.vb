Imports System.Numerics
''' <summary>
''' 
''' </summary>
Public Class ECPoint
    'Any variables defined with the double underscore (__<variable name>) defines an internal variable
    '__x defines the point x
    '__y defines the point y
    '__p defines the prime number
    Private __x, __y, __p As BigInteger
    Public Sub New(Point As ECPoint)
        Me.x = Point.x
        Me.y = Point.y
        Me.p = Point.p
    End Sub
    Public Sub New(X As BigInteger, Y As BigInteger, P As BigInteger)
        Me.x = X
        Me.y = Y
        Me.p = P 'Could add in prime testing from Fp Operations
    End Sub
    Public Sub New(X As BigInteger, Y As BigInteger)
        Me.x = X
        Me.y = Y
    End Sub
    Property x() As BigInteger
        Get
            Return __x
        End Get
        Set(value As BigInteger)
            __x = value
        End Set
    End Property

    Property y() As BigInteger
        Get
            Return __y
        End Get
        Set(value As BigInteger)
            __y = value
        End Set
    End Property

    Property p() As BigInteger
        Get
            Return __p
        End Get
        Set(value As BigInteger)
            __p = value
        End Set
    End Property

    ''' <summary>
    ''' Gets if the point is an infinity point
    ''' </summary>
    Public Function IsPointInfinity() As Boolean
        If x = 0 And y = 1 Then
            Return True
        End If
        Return False
    End Function
    ''' <summary>
    ''' Zero out an ECPoint for security Reasons
    ''' </summary>
    Public Sub Zero()
        x = BigInteger.Zero
        y = BigInteger.Zero
        p = BigInteger.Zero
    End Sub
    ''' <summary>
    ''' Checks that the x or y values are set to zero
    ''' </summary>
    ''' <returns></returns>
    Public Function IsNull() As Boolean
        If Me.x = 0 Or Me.y = 0 Then
            Return True
        End If
        Return False
    End Function
    ''' <summary>
    ''' Check if the point lies on a specific curve, checked by the general Weistrass Equation (x^3 + ax + b - y^2) % p == 0
    ''' </summary>
    Public Function IsPointOnCurve(ByVal Param As Curve.Domain_Parameters) As Boolean
        Return ((x * x * x) + (Param.a * x) + Param.b - (y * y)) Mod Param.Fp.p = 0
    End Function
    ''' <summary>
    ''' Returns a boolean value if the x y and p values are set to the value -99
    ''' </summary>
    ''' <returns></returns>
    Public Function isErroneous() As Boolean
        If x = -99 And y = -99 And p = -99 Then
            Return True
        End If
        Return False
    End Function
End Class
