Imports System.Numerics
Imports System.Diagnostics
Imports System.Reflection.MethodInfo
Imports Error_Handler.Events_Handler
Namespace Crypto
    Public MustInherit Class AES
        ''' <summary>
        ''' A structure that contains the two values returned from CBC encryption
        ''' </summary>
        Public Structure CBC_Values
            Public Data() As Byte 'The plaintext or chiphertext
            Public IV() As Byte 'The Initialisation Vector that was used or generated
            Public didError As Boolean
        End Structure
        ''' <summary>
        ''' Function that converts a string to a byte array
        ''' </summary>
        Public Shared Function Convert_String_ToByteArray(ByVal Str As String) As Byte()
            Dim chars() As Char = Str.ToCharArray
            Dim returnByte(chars.Length - 1) As Byte
            'Converts the character to byte in the index location determined by i and store the byte in the byte array at the location i
            For i = 0 To chars.Length - 1
                returnByte(i) = Convert.ToByte(chars(i))
            Next
            Return returnByte
        End Function
        ''' <summary>
        ''' Converts a byte array to string
        ''' </summary>
        Public Shared Function Convert_ByteArray_ToString(ByVal bArr() As Byte) As String
            Dim str As String
            For i = 0 To bArr.Length - 1
                str += Convert.ToChar(bArr(i))
            Next
            Return str
        End Function
        ''' <summary>
        ''' This function converts big integers to byte arrays rather than using .tobytearray due to the fact that .tobytearray could possibly use the wrong endianess
        ''' </summary>
        Public Shared Function Convert_BigInteger_ToByteArray(ByVal IntegerValue As BigInteger) As Byte()
            Dim values() As Byte = IntegerValue.ToByteArray
            If BitConverter.IsLittleEndian Then 'checks if the computer processor works in little endian as this program must be run in big endian
                Array.Reverse(values) 'reverses the byte array
            End If
            Return values
        End Function
        ''' <summary>
        ''' Converts a byte array converted using the method above back to BigInteger
        ''' </summary>
        Public Shared Function Convert_IntegerByteArray_ToBigInteger(ByVal bArr() As Byte) As BigInteger
            If BitConverter.IsLittleEndian Then
                Array.Reverse(bArr)
            End If
            Return New BigInteger(bArr)
        End Function
        ''' <summary>
        ''' Converts the 2 dimensional state array back into a 1 dimensional array
        ''' </summary>
        Friend Function ConvertFromState(ByVal State(,) As Byte) As Byte()
            Dim Ret(State.Length - 1) As Byte
            Dim i As Integer = Ret.Count - 1
            For c = 0 To 3 'column counter
                For r = 0 To 3 'row counter
                    Ret(r + 4 * c) = State(r, c)
                Next
            Next
            Return Ret
        End Function
        ''' <summary>
        ''' Converts a byte array of 16 values to a 2 dimensional state rather than a free
        ''' </summary>
        Friend Function convertSingleState(ByVal byteArrOf16 As Byte()) As Byte(,)
            If byteArrOf16.Count > 16 Then
                'Throw New Exception("Array larger than allowed") 'Stop the array provided from being cut off
                errorHappened(byteArrOf16, "Array larger than allowed", 6, GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim state(3, 3) As Byte
            For c = 0 To 3
                For r = 0 To 3
                    state(r, c) = byteArrOf16(r + c * 4)
                Next
            Next
            Return state
        End Function
        ''' <summary>
        ''' Debugging function
        ''' </summary>
        Friend Sub check_state_Val(ByVal State(,) As Byte)
            For r = 0 To 3
                Dim col(4) As Byte
                For c = 0 To 3
                    col(c) = State(r, c)
                Next
                Debug.Print(Hex(col(0)) & " " & Hex(col(1)) & " " & Hex(col(2)) & " " & Hex(col(3))) 'prints the hex values in the form FF FF FF FF

            Next
            Debug.Print(" ")
        End Sub
        ''' <summary>
        ''' This converts a byte array that is greater than 16 bytes to a state of 3 dimensions
        ''' </summary>
        Friend Function convertState(ByVal Plaintext As Byte()) As Byte(,,)
            Dim Nb, Nk As Integer
            Nk = 4 'row count
            Nb = 4 'column count
            Dim SCount As Integer
            Dim tmp((Nb * Nk) - 1) As Byte
            If Plaintext.Count = 0 Then 'checks for empty array
                'Throw New Exception("Plaintext is empty")
                errorHappened(Plaintext, "Plaintext is Null", 1, GetCurrentMethod.Name)
            End If
            'Calculates the number of States needed each state is a 4x4 array
            If Plaintext.Length * 8 > (4 * Nb * Nk) Then
                SCount = Math.Ceiling(Plaintext.Length / (Nb * Nk))
            End If
            'If we need 0 States we need to increase than number to 1 otherwise the algorithm next fails
            If SCount = 0 Then
                SCount += 1
            End If
            'We define the new 3 dimensional State array
            Dim State(SCount - 1, Nb - 1, Nk - 1) As Byte
            'And fill the state array
            For i = 0 To SCount - 1
                Populate(tmp, Plaintext, (Nb * Nk), (Nb * Nk) * i)
                For c = 0 To (Nb * Nk) - 1
                    State(i, c Mod Nk, Math.Floor(c / Nk)) = tmp(c)
                Next
                Array.Clear(tmp, 0, tmp.Count - 1)
            Next
            Return State
        End Function
        ''' <summary>
        ''' This function is used to copy as set of specific values from startarray into the finalarray
        ''' </summary>
        Friend Sub Populate(ByRef finalarr() As Byte, ByVal startarr() As Byte, ByVal count As Integer, ByVal start As Integer)
            Dim max As Integer
            If start + count < startarr.Length Then
                max = start + count
            Else
                max = startarr.Length
            End If
            For i = start To max - 1
                finalarr(i - start) = startarr(i)
            Next
        End Sub
        ''' <summary>
        ''' Copies a selected state from the 3 dimensional State array into the 2 dimensional one for use with AES algorithms
        ''' </summary>
        Friend Sub Copy_3to2(ByVal start(,,) As Byte, ByRef final(,) As Byte, ByVal start3rdPos As Integer, ByVal rowCount As Integer, ByVal colCount As Integer)
            For r = 0 To rowCount - 1
                For c = 0 To colCount - 1
                    final(r, c) = start(start3rdPos, r, c)
                Next
            Next
        End Sub
        ''' <summary>
        ''' Function used to extend the provided key for AES into something longer
        ''' </summary>
        Friend Function Rij_KeySchedule(ByVal Key() As Byte, ByVal keyLength As Integer) As Byte()
            Dim n As Integer
            Dim b As Integer
            'This is depreciated as we are only allowing 256bit curves
            If keyLength = 128 Then
                n = 16
                b = 176
            ElseIf keyLength = 192 Then
                n = 24
                b = 208
            ElseIf keyLength = 256 Then
                n = 32
                b = 240
            Else
                Throw New Exception("Key Lenght not valid")
            End If
            'ExpKey is the extended key
            Dim ExpKey(b - 1) As Byte
            Array.Copy(Key, 0, ExpKey, 0, n)
            Dim t(3) As Byte
            Dim c, i, a As Integer
            c = n
            i = 1
            'This function extends the key as seen on https://en.wikipedia.org/wiki/Rijndael_key_schedule
            While c < b
                For a = 0 To 3
                    t(a) = ExpKey(a + c - 4)
                Next
                If c Mod n = 0 Then
                    Core(t, i)
                    i += 1
                End If
                If c Mod n = 16 Then
                    For a = 0 To 3
                        t(a) = SBox(t(a))
                    Next
                End If
                For a = 0 To 3
                    ExpKey(c) = ExpKey(c - n) Xor t(a)
                    c += 1
                Next
            End While
            Return ExpKey
        End Function
        ''' <summary>
        ''' This perfroms the exponentation of 2 to a user specified value
        ''' </summary>
        Friend Function Get_RCON(ByVal Val As Integer) As Byte
            Dim c As Byte = 1
            If IsNothing(Val) Then
                errorHappened(Val, "RCON parameter is Null", 1, GetCurrentMethod.Name)
                Return Nothing
            End If
            While Val <> 1
                Dim b As Byte
                b = c And 128
                c <<= 1
                If b = 128 Then
                    c = c Xor 27
                End If
                'gmul(one, 2)
                Val -= 1
            End While
            Return c
        End Function
        ''' <summary>
        ''' This shifts a 4 byte array left by 1 and wraps the end value round to the start like so:
        ''' Before: 32 AA FF 10
        ''' After:  AA FF 10 32
        ''' </summary>
        Friend Sub Rotate(ByRef input As Byte())
            Dim a As Byte
            a = input(0)
            For b = 0 To 2
                input(b) = input(b + 1)
            Next
            input(3) = a
        End Sub
        ''' <summary>
        ''' This is a lookup table of multiplicative inverses for a number inside the Rijndaels finite field
        ''' </summary>
        Friend Function SBox(ByVal input As Byte) As Byte
            Dim sboxvals(255) As Byte
            sboxvals = {99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22}
            Return sboxvals(input)
        End Function
        ''' <summary>
        ''' This performs the inverse of Galios Field multiplication
        ''' </summary>
        Friend Function gmul_inverse(ByVal input As Byte) As Byte
            If IsNothing(input) Then
                errorHappened(input, "Parameter is Null", 1, GetCurrentMethod.Name)
                Return Nothing
            End If
            Dim altable(255), ltable(255) As Byte
            Dim ret As Byte
            ltable = {0, 255, 200, 8, 145, 16, 208, 54, 90, 62, 216, 67, 153, 119, 254, 24, 35, 32, 7, 112, 161, 108, 12, 127, 98, 139, 64, 70, 199, 75, 224, 14, 235, 22, 232, 173, 207, 205, 57, 83, 106, 39, 53, 147, 212, 78, 72, 195, 43, 121, 84, 40, 9, 120, 15, 33, 144, 135, 20, 42, 169, 156, 214, 116, 180, 124, 222, 237, 177, 134, 118, 164, 152, 226, 150, 143, 2, 50, 28, 193, 51, 238, 239, 129, 253, 48, 92, 19, 157, 41, 23, 196, 17, 68, 140, 128, 243, 115, 66, 30, 29, 181, 240, 18, 209, 91, 65, 162, 215, 44, 233, 213, 89, 203, 80, 168, 220, 252, 242, 86, 114, 166, 101, 47, 159, 155, 61, 186, 125, 194, 69, 130, 167, 87, 182, 163, 122, 117, 79, 174, 63, 55, 109, 71, 97, 190, 171, 211, 95, 176, 88, 175, 202, 94, 250, 133, 228, 77, 138, 5, 251, 96, 183, 123, 184, 38, 74, 103, 198, 26, 248, 105, 37, 179, 219, 189, 102, 221, 241, 210, 223, 3, 141, 52, 217, 146, 13, 99, 85, 170, 73, 236, 188, 149, 60, 132, 11, 245, 230, 231, 229, 172, 126, 110, 185, 249, 218, 142, 154, 201, 36, 225, 10, 21, 107, 58, 160, 81, 244, 234, 178, 151, 158, 93, 34, 136, 148, 206, 25, 1, 113, 76, 165, 227, 197, 49, 187, 204, 31, 45, 59, 82, 111, 246, 46, 137, 247, 192, 104, 27, 100, 4, 6, 191, 131, 56}
            altable = {1, 229, 76, 181, 251, 159, 252, 18, 3, 52, 212, 196, 22, 186, 31, 54, 5, 92, 103, 87, 58, 213, 33, 90, 15, 228, 169, 249, 78, 100, 99, 238, 17, 55, 224, 16, 210, 172, 165, 41, 51, 89, 59, 48, 109, 239, 244, 123, 85, 235, 77, 80, 183, 42, 7, 141, 255, 38, 215, 240, 194, 126, 9, 140, 26, 106, 98, 11, 93, 130, 27, 143, 46, 190, 166, 29, 231, 157, 45, 138, 114, 217, 241, 39, 50, 188, 119, 133, 150, 112, 8, 105, 86, 223, 153, 148, 161, 144, 24, 187, 250, 122, 176, 167, 248, 171, 40, 214, 21, 142, 203, 242, 19, 230, 120, 97, 63, 137, 70, 13, 53, 49, 136, 163, 65, 128, 202, 23, 95, 83, 131, 254, 195, 155, 69, 57, 225, 245, 158, 25, 94, 182, 207, 75, 56, 4, 185, 43, 226, 193, 74, 221, 72, 12, 208, 125, 61, 88, 222, 124, 216, 20, 107, 135, 71, 232, 121, 132, 115, 60, 189, 146, 201, 35, 139, 151, 149, 68, 220, 173, 64, 101, 134, 162, 164, 204, 127, 236, 192, 175, 145, 253, 247, 79, 129, 47, 91, 234, 168, 28, 2, 209, 152, 113, 237, 37, 227, 36, 6, 104, 179, 147, 44, 111, 62, 108, 10, 184, 206, 174, 116, 177, 66, 180, 30, 211, 73, 233, 156, 200, 198, 199, 34, 110, 219, 32, 191, 67, 81, 82, 102, 178, 118, 96, 218, 197, 243, 246, 170, 205, 154, 160, 117, 84, 14, 1}
            ret = Convert.ToByte(altable((256 - ltable(input))))
            Return ret
        End Function
        ''' <summary>
        ''' This is the inner loop of the key schedule
        ''' </summary>
        Friend Sub Core(ByRef input As Byte(), ByVal iteration As Integer)
            Rotate(input)
            For i = 0 To 3
                input(i) = SBox(input(i))
            Next
            input(0) = input(0) Xor Get_RCON(iteration)
        End Sub
        ''' <summary>
        ''' perfoms galios field multiplication using lookup tables
        ''' </summary>
        Friend Function gmul(ByVal input1 As Byte, ByVal input2 As Byte)
            If input1 = 0 Or input2 = 0 Then
                Return 0
            End If
            Dim altable(255), ltable(255) As Byte
            'log table
            ltable = {0, 255, 200, 8, 145, 16, 208, 54, 90, 62, 216, 67, 153, 119, 254, 24, 35, 32, 7, 112, 161, 108, 12, 127, 98, 139, 64, 70, 199, 75, 224, 14, 235, 22, 232, 173, 207, 205, 57, 83, 106, 39, 53, 147, 212, 78, 72, 195, 43, 121, 84, 40, 9, 120, 15, 33, 144, 135, 20, 42, 169, 156, 214, 116, 180, 124, 222, 237, 177, 134, 118, 164, 152, 226, 150, 143, 2, 50, 28, 193, 51, 238, 239, 129, 253, 48, 92, 19, 157, 41, 23, 196, 17, 68, 140, 128, 243, 115, 66, 30, 29, 181, 240, 18, 209, 91, 65, 162, 215, 44, 233, 213, 89, 203, 80, 168, 220, 252, 242, 86, 114, 166, 101, 47, 159, 155, 61, 186, 125, 194, 69, 130, 167, 87, 182, 163, 122, 117, 79, 174, 63, 55, 109, 71, 97, 190, 171, 211, 95, 176, 88, 175, 202, 94, 250, 133, 228, 77, 138, 5, 251, 96, 183, 123, 184, 38, 74, 103, 198, 26, 248, 105, 37, 179, 219, 189, 102, 221, 241, 210, 223, 3, 141, 52, 217, 146, 13, 99, 85, 170, 73, 236, 188, 149, 60, 132, 11, 245, 230, 231, 229, 172, 126, 110, 185, 249, 218, 142, 154, 201, 36, 225, 10, 21, 107, 58, 160, 81, 244, 234, 178, 151, 158, 93, 34, 136, 148, 206, 25, 1, 113, 76, 165, 227, 197, 49, 187, 204, 31, 45, 59, 82, 111, 246, 46, 137, 247, 192, 104, 27, 100, 4, 6, 191, 131, 56}
            'antilog table
            altable = {1, 229, 76, 181, 251, 159, 252, 18, 3, 52, 212, 196, 22, 186, 31, 54, 5, 92, 103, 87, 58, 213, 33, 90, 15, 228, 169, 249, 78, 100, 99, 238, 17, 55, 224, 16, 210, 172, 165, 41, 51, 89, 59, 48, 109, 239, 244, 123, 85, 235, 77, 80, 183, 42, 7, 141, 255, 38, 215, 240, 194, 126, 9, 140, 26, 106, 98, 11, 93, 130, 27, 143, 46, 190, 166, 29, 231, 157, 45, 138, 114, 217, 241, 39, 50, 188, 119, 133, 150, 112, 8, 105, 86, 223, 153, 148, 161, 144, 24, 187, 250, 122, 176, 167, 248, 171, 40, 214, 21, 142, 203, 242, 19, 230, 120, 97, 63, 137, 70, 13, 53, 49, 136, 163, 65, 128, 202, 23, 95, 83, 131, 254, 195, 155, 69, 57, 225, 245, 158, 25, 94, 182, 207, 75, 56, 4, 185, 43, 226, 193, 74, 221, 72, 12, 208, 125, 61, 88, 222, 124, 216, 20, 107, 135, 71, 232, 121, 132, 115, 60, 189, 146, 201, 35, 139, 151, 149, 68, 220, 173, 64, 101, 134, 162, 164, 204, 127, 236, 192, 175, 145, 253, 247, 79, 129, 47, 91, 234, 168, 28, 2, 209, 152, 113, 237, 37, 227, 36, 6, 104, 179, 147, 44, 111, 62, 108, 10, 184, 206, 174, 116, 177, 66, 180, 30, 211, 73, 233, 156, 200, 198, 199, 34, 110, 219, 32, 191, 67, 81, 82, 102, 178, 118, 96, 218, 197, 243, 246, 170, 205, 154, 160, 117, 84, 14, 1}
            Dim a As Integer
            a = (Convert.ToInt32(ltable(input1)) + Convert.ToInt32(ltable(input2))) Mod 255
            a = altable(a)
            Return a
        End Function
    End Class
    Public Class AES_Encrypt
        Inherits AES
        ''' <summary>
        ''' Performs AES Cipher Block Chaining, or by specifiying CBC_MAC as True we can perform CBC_MAC for calculating a Message Authentication Code
        ''' </summary>
        Public Function Encrypt_Sign_AES_256_CBC(ByVal Plaintext As Byte(), ByVal Key As Byte(), Optional ByVal CBC_MAC As Boolean = False) As CBC_Values
            Dim ret As New CBC_Values
            If IsNothing(Plaintext) Or Plaintext.Count = 0 Then
                errorHappened(Plaintext, "Plaintext is Null", 1, GetCurrentMethod.Name)
                ret.didError = True
                Return ret
            End If
            If IsNothing(Key) Or Key.Length < 32 Then
                errorHappened(Key, "Key failed validation checks", 6, GetCurrentMethod.Name)
                ret.didError = True
                Return ret
            End If
            Dim IV() As Byte
            If CBC_MAC = False Then
                IV = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}
                'We can get current total cpu usage in order to help derrive a new cryptographically secure IV
                Dim cpu As New PerformanceCounter()
                Dim rand As New Random
                With cpu
                    .CategoryName = "Processor"
                    .CounterName = "% Processor Time"
                    .InstanceName = "_Total"
                End With
                'We repeat the generation 50 times to be secure
                For c = 0 To 50
                    'we populate the 16 array values using this secure method
                    For i = 0 To 15
                        'We can use cpu usage as a percentage, cursor positions, a random number and ram usage as a percentage to generate an cryptographic IV that is secure
                        Dim ramusage As Integer = (My.Computer.Info.AvailablePhysicalMemory / My.Computer.Info.TotalPhysicalMemory) * 100
                        Dim tmpNum As Integer = IV(i) Xor Cursor.Position.X Xor Cursor.Position.Y Xor cpu.NextValue Xor ramusage Xor rand.Next
                        IV(i) = Math.Abs(tmpNum Mod 255)
                    Next
                Next
                writeDebugLog("Finished generating IV for session", GetCurrentMethod.Name)
            Else
                'If siging the message using MAC we can set IV to zero
                IV = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                writeDebugLog("Set IV to zero for MAC signature generation", GetCurrentMethod.Name)
            End If
            Dim State3(,,) As Byte
            'Converts the plaintext into the states possible
            State3 = convertState(Plaintext)
            Dim State2(3, 3) As Byte
            'Get the first state from the three dimensional holding
            Copy_3to2(State3, State2, 0, 4, 4)
            Dim operationCount As Integer = Math.Ceiling(Plaintext.Length / 16) 'calculates the number of states that plaintext was converted into
            Dim stateIV(,) As Byte = convertSingleState(IV) 'converts the IV into a single state as the IV is 16 counts long
            Dim AES256 As New AES_Encrypt
            Dim cipherState2(3, 3) As Byte
            Dim ciphertext((Math.Ceiling(Plaintext.Count / 16) * 16) - 1) As Byte 'We need to calculate the number of states that the plaintext has used up so that when we copy the encrypted State to the output we have the right number indexes
            For i = 0 To operationCount - 1
                If i = 0 Then
                    'function reused from AES as it XORs two states together
                    AddRoundKey(stateIV, State2)
                Else
                    Copy_3to2(State3, State2, i, 4, 4)
                    AddRoundKey(cipherState2, State2)
                End If
                'Encryption
                cipherState2 = AES256.Encrypt_AES(State2, Key)
                'convert the ciphertext into a 1D byte array and copy that two the return variable
                If Not CBC_MAC Then
                    'if encrypting then we need all the data back
                    Array.Copy(ConvertFromState(cipherState2), 0, ciphertext, i * 16, 16)
                End If
            Next
            If CBC_MAC Then
                'if generating a MAC then all we need is the last block of the mix
                ReDim ciphertext(15)
                Array.Copy(ConvertFromState(cipherState2), 0, ciphertext, 0, 16)
            End If
            If Not CBC_MAC Then
                writeDebugLog("Finished encrypting the plaintext with CBC, returning.", GetCurrentMethod.Name)
            Else
                writeDebugLog("Finished generating the MAC for the message, returning.", GetCurrentMethod.Name)
            End If
            ret.Data = ciphertext
            ret.IV = IV
            Return ret
        End Function
        ''' <summary>
        ''' AES encryption algorithm
        ''' </summary>
        Private Function Encrypt_AES(ByVal State(,) As Byte, ByVal Key() As Byte) As Byte(,)
            Dim Rounds As Integer
            Dim IntState(3, 3) As Byte
            Dim IntKey(31) As Byte
            Array.Copy(State, IntState, 16)
            Array.Copy(Key, IntKey, 32)
            Dim RoundKey(,,) As Byte
            Dim Curkey(3, 3) As Byte
            IntKey = Rij_KeySchedule(IntKey, 256)
            RoundKey = convertState(IntKey)
            Copy_3to2(RoundKey, Curkey, 0, 4, 4)
            AddRoundKey(Curkey, IntState)
            For i = 1 To Rounds - 1
                Sub_Bytes(IntState)
                ShiftRows(IntState)
                Mix_Column(IntState)
                Copy_3to2(RoundKey, Curkey, i, 4, 4)
                AddRoundKey(Curkey, IntState)
            Next
            Sub_Bytes(IntState)
            ShiftRows(IntState)
            Copy_3to2(RoundKey, Curkey, Rounds, 4, 4)
            AddRoundKey(Curkey, IntState)
            Return IntState
        End Function
        Private Sub AddRoundKey(ByVal RoundKey(,) As Byte, ByRef State(,) As Byte)
            For c = 0 To 3
                For r = 0 To 3
                    State(r, c) = RoundKey(r, c) Xor State(r, c)
                Next
            Next
        End Sub
        Private Sub Sub_Bytes(ByRef State(,) As Byte)
            For c = 0 To 3
                For r = 0 To 3
                    State(r, c) = SBox(State(r, c))
                Next
            Next
        End Sub
        Private Sub Mix_Column(ByRef State(,) As Byte)
            Dim tmpState(3, 3) As Byte
            Dim DoubleState(3, 3) As Byte
            For c = 0 To 3
                For r = 0 To 3
                    Dim highBit As Byte
                    tmpState(r, c) = State(r, c)
                    highBit = State(r, c) And 128
                    DoubleState(r, c) = State(r, c) << 1
                    If highBit = 128 Then
                        DoubleState(r, c) = DoubleState(r, c) Xor 27
                    End If
                Next
            Next
            For c = 0 To 3
                State(0, c) = DoubleState(0, c) Xor tmpState(3, c) Xor tmpState(2, c) Xor DoubleState(1, c) Xor tmpState(1, c)
                State(1, c) = DoubleState(1, c) Xor tmpState(0, c) Xor tmpState(3, c) Xor DoubleState(2, c) Xor tmpState(2, c)
                State(2, c) = DoubleState(2, c) Xor tmpState(1, c) Xor tmpState(0, c) Xor DoubleState(3, c) Xor tmpState(3, c)
                State(3, c) = DoubleState(3, c) Xor tmpState(2, c) Xor tmpState(1, c) Xor DoubleState(0, c) Xor tmpState(0, c)
            Next

        End Sub
        Private Sub Mix_Columns(ByRef State(,) As Byte)
            Dim Col(3) As Byte
            Dim tmp_state(3, 3) As Byte
            For c = 0 To 3
                For r = 0 To 3
                    Col(r) = State(r, c)
                Next
                Mix_One(Col)
                For r = 0 To 3
                    tmp_state(r, c) = Col(r)
                Next
            Next
            State = tmp_state
        End Sub
        Private Sub Mix_One(ByRef Col() As Byte)
            Dim tmpCol(3) As Byte
            Dim DCol(3) As Byte
            For i = 0 To 3
                Dim h As Byte
                tmpCol(i) = Col(i)
                h = Col(i) And 128
                DCol(i) = Col(i) << 1
                If h = 128 Then
                    DCol(i) = DCol(i) Xor 27
                End If
            Next
            Col(0) = DCol(0) Xor tmpCol(3) Xor tmpCol(2) Xor DCol(1) Xor tmpCol(1)
            Col(1) = DCol(1) Xor tmpCol(0) Xor tmpCol(3) Xor DCol(2) Xor tmpCol(2)
            Col(2) = DCol(2) Xor tmpCol(1) Xor tmpCol(0) Xor DCol(3) Xor tmpCol(3)
            Col(3) = DCol(3) Xor tmpCol(2) Xor tmpCol(1) Xor DCol(0) Xor tmpCol(0)
        End Sub
        Private Sub ShiftRows(ByRef State(,) As Byte)
            Dim temp As Byte
            For i = 1 To 3
                For c = i - 1 To 0 Step -1
                    temp = State(i, 0)
                    State(i, 0) = State(i, 1)
                    State(i, 1) = State(i, 2)
                    State(i, 2) = State(i, 3)
                    State(i, 3) = temp
                Next
            Next
        End Sub
    End Class
    Public Class AES_Decrypt
        Inherits AES
        ''' <summary>
        ''' Decrypts a byte array encrypted with AES and Cipher Block Chaining
        ''' </summary>
        Public Function Decrypt_AES_256_CBC(ByVal Ciphertext As Byte(), ByVal key As Byte(), ByVal IV() As Byte) As CBC_Values
            Dim ret As New CBC_Values
            If IsNothing(Ciphertext) Or Ciphertext.Count = 0 Then
                errorHappened(Ciphertext, "Ciphertext is Null", 1, GetCurrentMethod.Name)
                ret.didError = True
                Return ret
            End If
            If IsNothing(key) Or key.Length < 32 Then
                errorHappened(key, "Key failed validation checks", 6, GetCurrentMethod.Name)
                ret.didError = True
                Return ret
            End If
            Dim Aes256 As New AES_Decrypt
            Dim oldState2(3, 3) As Byte
            Dim State3(,,) As Byte = convertState(Ciphertext)
            Dim State2(3, 3) As Byte
            Copy_3to2(State3, State2, 0, 4, 4)
            Dim operationCount As Integer = Math.Ceiling(Ciphertext.Length / 16)
            Dim stateIV(,) As Byte = convertSingleState(IV)
            Dim plainState2(3, 3) As Byte
            Dim Plaintext(Ciphertext.Count - 1) As Byte
            For i = 0 To operationCount - 1
                Copy_3to2(State3, State2, i, 4, 4)
                plainState2 = Aes256.Decrypt_AES(State2, key)
                If i = 0 Then
                    AddRoundKey(stateIV, plainState2)
                Else
                    AddRoundKey(oldState2, plainState2)
                End If
                Array.Copy(ConvertFromState(plainState2), 0, Plaintext, i * 16, 16)
                oldState2 = State2.Clone()
            Next
            writeDebugLog("Finished decrypting the text", GetCurrentMethod.Name)
            ret.Data = removeZeros(Plaintext)
            ret.IV = IV
            Return ret
        End Function
        ''' <summary>
        ''' Removes the excess zeros from the end of a message
        ''' </summary>
        Private Function removeZeros(ByRef plaintext() As Byte) As Byte()
            Dim number As Integer
            Dim i As Integer = plaintext.Length - 1
            writeDebugLog("Removing zero values from the end of the decrypted message", GetCurrentMethod.Name)
            Do Until plaintext(i) <> 0 Or i = 0
                number += 1
                i -= 1
            Loop
            Dim deZeroed(plaintext.Count - 1 - number) As Byte
            i = 0
            For i = 0 To plaintext.Length - 1 - number
                deZeroed(i) = plaintext(i)
            Next
            writeDebugLog("Removed " & number & " zeros from the message", GetCurrentMethod.Name)
            Return deZeroed
        End Function
        ''' <summary>
        ''' AES decryption algorithm
        ''' </summary>
        Private Function Decrypt_AES(ByVal State(,) As Byte, ByVal key() As Byte) As Byte(,)
            Dim Rounds As Integer
            Dim IntState(3, 3) As Byte
            Dim IntKey(31) As Byte
            Array.Copy(State, IntState, 16)
            Array.Copy(key, IntKey, 32)
            Dim RoundKey(,,) As Byte
            Dim Curkey(3, 3) As Byte
            IntKey = Rij_KeySchedule(IntKey, 256)
            RoundKey = convertState(IntKey)
            Copy_3to2(RoundKey, Curkey, Rounds, 4, 4)
            AddRoundKey(Curkey, IntState)
            Inv_ShiftRows(IntState)
            Inv_Sub_Bytes(IntState)
            For i = Rounds - 1 To 1 Step -1
                Copy_3to2(RoundKey, Curkey, i, 4, 4)
                AddRoundKey(Curkey, IntState)
                Inv_Mix_Column(IntState)
                Inv_ShiftRows(IntState)
                Inv_Sub_Bytes(IntState)
            Next
            Copy_3to2(RoundKey, Curkey, 0, 4, 4)
            AddRoundKey(Curkey, IntState)
            Return IntState
        End Function
        ''' <summary>
        ''' Perfroms the inverse of SBox
        ''' </summary>
        Private Function Inv_SBox(ByVal input As Byte) As Byte
            Dim invsboxvals(255) As Byte
            invsboxvals = {82, 9, 106, 213, 48, 54, 165, 56, 191, 64, 163, 158, 129, 243, 215, 251, 124, 227, 57, 130, 155, 47, 255, 135, 52, 142, 67, 68, 196, 222, 233, 203, 84, 123, 148, 50, 166, 194, 35, 61, 238, 76, 149, 11, 66, 250, 195, 78, 8, 46, 161, 102, 40, 217, 36, 178, 118, 91, 162, 73, 109, 139, 209, 37, 114, 248, 246, 100, 134, 104, 152, 22, 212, 164, 92, 204, 93, 101, 182, 146, 108, 112, 72, 80, 253, 237, 185, 218, 94, 21, 70, 87, 167, 141, 157, 132, 144, 216, 171, 0, 140, 188, 211, 10, 247, 228, 88, 5, 184, 179, 69, 6, 208, 44, 30, 143, 202, 63, 15, 2, 193, 175, 189, 3, 1, 19, 138, 107, 58, 145, 17, 65, 79, 103, 220, 234, 151, 242, 207, 206, 240, 180, 230, 115, 150, 172, 116, 34, 231, 173, 53, 133, 226, 249, 55, 232, 28, 117, 223, 110, 71, 241, 26, 113, 29, 41, 197, 137, 111, 183, 98, 14, 170, 24, 190, 27, 252, 86, 62, 75, 198, 210, 121, 32, 154, 219, 192, 254, 120, 205, 90, 244, 31, 221, 168, 51, 136, 7, 199, 49, 177, 18, 16, 89, 39, 128, 236, 95, 96, 81, 127, 169, 25, 181, 74, 13, 45, 229, 122, 159, 147, 201, 156, 239, 160, 224, 59, 77, 174, 42, 245, 176, 200, 235, 187, 60, 131, 83, 153, 97, 23, 43, 4, 126, 186, 119, 214, 38, 225, 105, 20, 99, 85, 33, 12, 125}
            Return invsboxvals(input)
        End Function
        ''' <summary>
        ''' XORs the key with the state
        ''' </summary>
        Private Sub AddRoundKey(ByVal RoundKey(,) As Byte, ByRef State(,) As Byte)
            For c = 0 To 3
                For r = 0 To 3
                    State(r, c) = RoundKey(r, c) Xor State(r, c)
                Next
            Next
        End Sub
        ''' <summary>
        ''' Does the inverse of Sub_Bytes by using inverse SBox to look up the data
        ''' </summary>
        Private Sub Inv_Sub_Bytes(ByRef State(,) As Byte)
            For c = 0 To 3
                For r = 0 To 3
                    State(r, c) = Inv_SBox(State(r, c))
                Next
            Next
        End Sub
        ''' <summary>
        ''' Does the inverse of Mix_Column
        ''' </summary>
        Private Sub Inv_Mix_Column(ByRef State(,) As Byte)
            Dim tmpState(3, 3) As Byte
            For c = 0 To 3
                For r = 0 To 3
                    tmpState(r, c) = State(r, c)
                Next
            Next
            For c = 0 To 3
                State(0, c) = gmul(tmpState(0, c), 14) Xor gmul(tmpState(3, c), 9) Xor gmul(tmpState(2, c), 13) Xor gmul(tmpState(1, c), 11)
                State(1, c) = gmul(tmpState(1, c), 14) Xor gmul(tmpState(0, c), 9) Xor gmul(tmpState(3, c), 13) Xor gmul(tmpState(2, c), 11)
                State(2, c) = gmul(tmpState(2, c), 14) Xor gmul(tmpState(1, c), 9) Xor gmul(tmpState(0, c), 13) Xor gmul(tmpState(3, c), 11)
                State(3, c) = gmul(tmpState(3, c), 14) Xor gmul(tmpState(2, c), 9) Xor gmul(tmpState(1, c), 13) Xor gmul(tmpState(0, c), 11)
            Next
        End Sub
        ''' <summary>
        ''' Does the inverse of shift rows
        ''' </summary>
        Private Sub Inv_ShiftRows(ByRef State(,) As Byte)
            Dim temp As Byte
            For i = 1 To 3
                For c = i - 1 To 0 Step -1
                    temp = State(i, 3)
                    State(i, 3) = State(i, 2)
                    State(i, 2) = State(i, 1)
                    State(i, 1) = State(i, 0)
                    State(i, 0) = temp
                Next
            Next
        End Sub
    End Class
End Namespace