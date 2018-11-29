Imports Microsoft.Win32
Public Class RegistryTools
    Public Shared Sub addValueToRegistry(ByVal value As String, ByVal username As String)
        Dim RegKey As RegistryKey
        RegKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", True)
        RegKey = RegKey.CreateSubKey("IM Application")
        RegKey.SetValue(username, value)
        RegKey.Close()
    End Sub
    Public Shared Function getRegistryValue(ByVal username As String) As String
        Dim RegKey As RegistryKey
        Dim ret As String
        RegKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", True)
        RegKey = RegKey.CreateSubKey("IM Application")
        ret = RegKey.GetValue(username, "NULL")
        RegKey.Close()
        Return ret
    End Function
End Class
