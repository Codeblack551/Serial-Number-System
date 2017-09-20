Imports System.IO
Imports System.Net
Imports System.Threading
Imports System.Management
Imports System.Security

Public Class Form1

    Dim hwid As String
    Dim check As Integer

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        check = 0

        Dim hw As New clsComputerInfo
        Dim cpu As String
        Dim mb As String
        Dim mac As String
        Dim hwid As String
        cpu = hw.GetProcessorId()
        mb = hw.GetMotherboardID()
        mac = hw.GetMACAddress()
        hwid = cpu + mb + mac
        Dim hwidEncrypted As String = Strings.UCase(hw.getMD5Hash(cpu & mb & mac))
        hwid = hwidEncrypted
        TextBox2.Text = hwid

        If (My.Settings.Installed = True) Then
            TextBox1.Text = My.Settings.Serial
            Timer1.Interval = 1
            Timer1.Start()
            check = 1
        End If
    End Sub

    Private Class clsComputerInfo
        'Get processor ID
        Friend Function GetProcessorId() As String
            Dim strProcessorID As String = String.Empty
            Dim query As New SelectQuery("Win32_processor")
            Dim search As New ManagementObjectSearcher(query)
            Dim info As ManagementObject
            For Each info In search.Get()
                strProcessorID = info("processorID").ToString()
            Next
            Return strProcessorID
        End Function
        ' Get MAC Address
        Friend Function GetMACAddress() As String
            Dim mc As ManagementClass = New ManagementClass("Win32_NetworkAdapterConfiguration")
            Dim moc As ManagementObjectCollection = mc.GetInstances()
            Dim MacAddress As String = String.Empty
            For Each mo As ManagementObject In moc
                If (MacAddress.Equals(String.Empty)) Then
                    If CBool(mo("IPEnabled")) Then MacAddress = mo("MacAddress").ToString()
                    mo.Dispose()
                End If
                MacAddress = MacAddress.Replace(":", String.Empty)
            Next
            Return MacAddress
        End Function
        ' Get Motherboard ID
        Friend Function GetMotherboardID() As String
            Dim strMotherboardID As String = String.Empty
            Dim query As New SelectQuery("Win32_BaseBoard")
            Dim search As New ManagementObjectSearcher(query)
            Dim info As ManagementObject
            For Each info In search.Get()
                strMotherboardID = info("product").ToString()
            Next
            Return strMotherboardID
        End Function
        ' Encrypt HWID
        Friend Function getMD5Hash(ByVal strToHash As String) As String
            Dim md5Obj As New System.Security.Cryptography.MD5CryptoServiceProvider
            Dim bytesToHash() As Byte = System.Text.Encoding.ASCII.GetBytes(strToHash)
            bytesToHash = md5Obj.ComputeHash(bytesToHash)
            Dim strResult As String = ""
            For Each b As Byte In bytesToHash
                strResult += b.ToString("x2")
            Next
            Return strResult
        End Function
    End Class

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        check = 1
        Button1.Enabled = False
        WebBrowser1.Navigate("http://localhost/serial/serial_get.php?serial=" + TextBox1.Text + "&hwidin=" + TextBox2.Text + "&submit=Submit")
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        If (check = 1) Then
            If (WebBrowser1.DocumentText.Contains("0")) Then
                check = 0
                Button1.Enabled = True
                Timer1.Stop()
                MsgBox("Wrong HWID")
            ElseIf (WebBrowser1.DocumentText.Contains("1")) Then
                check = 0
                Button1.Enabled = True
                My.Settings.Installed = True
                My.Settings.Serial = TextBox1.Text
                My.Settings.Save()
                Timer1.Stop()
                MsgBox("All info correct!")
            ElseIf (WebBrowser1.DocumentText.Contains("2")) Then
                check = 0
                Button1.Enabled = True
                Timer1.Stop()
                MsgBox("HWID field left empty")
            ElseIf (WebBrowser1.DocumentText.Contains("3")) Then
                check = 0
                Button1.Enabled = True
                Timer1.Stop()
                MsgBox("No serial with that key")
            End If
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        WebBrowser1.Navigate("http://localhost/serial/serial_get.php?serial=" + TextBox1.Text + "&hwidin=" + TextBox2.Text + "&submit=Submit")
    End Sub
End Class
