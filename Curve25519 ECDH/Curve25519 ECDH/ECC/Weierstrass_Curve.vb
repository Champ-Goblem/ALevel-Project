Imports System.Numerics
Namespace Curve
    '''<summary>
    '''Deffines a cruve with the general weistrass formula y^2 = x^3 + ax + b
    ''' </summary>
    Public Class Weierstrass_Curve
        Public Parameters As New Curve.Domain_Parameters(0, New Fp_Operations.Fp(1), 0, 0, New ECPoint(0, 0), 0, 0)
        Public Curve_ID As String
        Public Sub New(ByVal Filepath As String, ByVal Curve_Name As String)
            If Not IO.File.Exists(Filepath) Then
                Throw New IO.FileNotFoundException
            End If
            Dim found As Boolean
            Dim xmlr As New Xml.XmlDocument()
            Dim xmlnode As Xml.XmlNodeList
            Dim fs As New IO.FileStream(Filepath, IO.FileMode.Open)
            xmlr.Load(fs)
            xmlnode = xmlr.GetElementsByTagName("Curve-ID")
            For i = 0 To xmlnode.Count - 1
                If xmlnode(i).Attributes(0).InnerText = Curve_Name Then
                    If xmlnode(i).ChildNodes.Count <> 8 Then
                        fs.Close()
                        Throw New Exception("The current file is corrupted or the curve entry is corrupted, rebuild the curves.xml file from the menu")
                    End If
                    Me.Curve_ID = Curve_Name
                    Me.Parameters.Bitlength = Convert.ToInt32(xmlnode(i).ChildNodes(0).InnerText)
                    Me.Parameters.Fp = New Fp_Operations.Fp(BigInteger.Parse(xmlnode(i).ChildNodes(1).InnerText))
                    Me.Parameters.a = BigInteger.Parse(xmlnode(i).ChildNodes(2).InnerText)
                    Me.Parameters.b = BigInteger.Parse(xmlnode(i).ChildNodes(3).InnerText)
                    Me.Parameters.Base.x = BigInteger.Parse(xmlnode(i).ChildNodes(4).InnerText)
                    Me.Parameters.Base.y = BigInteger.Parse(xmlnode(i).ChildNodes(5).InnerText)
                    Me.Parameters.Base.p = Me.Parameters.Fp.p
                    Me.Parameters.n = BigInteger.Parse(xmlnode(i).ChildNodes(6).InnerText)
                    Me.Parameters.h = BigInteger.Parse(xmlnode(i).ChildNodes(7).InnerText)
                    found = True
                End If
            Next
            If found = False Then
                Throw New Exception("Curve could not be found in the xml file")
            End If
            Dim ex As Error_Handler.Events_Handler.errorEnum = Domain_Parameters.Validate_Params(Me.Parameters)
            If ex <> 0 Then
                'Error_Handler.Events_Handler.errorHappened(Me.Parameters, "Parameters imported from the xml file failed to pass validation", ex, System.Reflection.MethodInfo.GetCurrentMethod.Name)
                Throw New Exception("Curve failed parameter validation: " & ex.ToString)
            End If
            fs.Close()
        End Sub
    End Class
End Namespace
