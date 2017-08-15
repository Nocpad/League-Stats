Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports L_Stats.User32Wrappers
Public Class OverlayForm
    Private _InitialStyle As Integer
    Private Sub OverlayForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        'Me.Location = New Point(Screen.PrimaryScreen.Bounds.Width - Me.Width - 10, 100)

        _InitialStyle = GetWindowLong(Me.Handle, GWL.ExStyle)
        SetFormToTransparent()
    End Sub

    'Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
    '    If m.Msg = Hotkey.WM_HOTKEY Then
    '        Hotkey.handleHotKeyEvent(m.WParam)
    '    End If
    '    MyBase.WndProc(m)
    'End Sub

    Private Sub SetFormToTransparent()
        ' This creates a new Extended Style
        ' for our window, which takes effect
        ' immediately upon being set, that
        ' combines the initial style of our window
        ' (saved in Form.Load) and adds the ability
        ' to be Transparent to the mouse.
        ' Both Layered and Transparent must be
        ' turned on for this to work AND have
        '  the window render properly!
        SetWindowLong(Me.Handle, GWL.ExStyle,
            _InitialStyle Or WS_EX.Layered Or WS_EX.Transparent)

        ' Don't forget to set the Alpha
        ' for the window or else you won't be able
        ' to see the window! Possible values
        ' are 0 (visibly transparent)
        ' to 255 (visibly opaque). I'll set
        ' it to 70% visible here for show.
        ' The second parameter is 0, because
        ' we're not using a ColorKey!
        SetLayeredWindowAttributes(Me.Handle, 0,
                           255 * My.Settings.OpacityPercentage, LWA.Alpha)
    End Sub
End Class
Public Class Hotkey

#Region "Declarations - WinAPI, Hotkey constant and Modifier Enum"
    ''' <summary>
    ''' Declaration of winAPI function wrappers. The winAPI functions are used to register / unregister a hotkey
    ''' </summary>
    Public Declare Function RegisterHotKey Lib "user32" _
        (ByVal hwnd As IntPtr, ByVal id As Integer, ByVal fsModifiers As Integer, ByVal vk As Integer) As Integer

    Public Declare Function UnregisterHotKey Lib "user32" (ByVal hwnd As IntPtr, ByVal id As Integer) As Integer

    Public Const WM_HOTKEY As Integer = &H312

    Private Declare Auto Function SetWindowLong Lib "User32.Dll" (ByVal hWnd As IntPtr, ByVal nIndex As Integer, ByVal dwNewLong As Integer) As Integer
    Private Declare Function SetForegroundWindow Lib "user32" (ByVal hWnd As IntPtr) As IntPtr

    Public Enum GWLParameter
        GWL_EXSTYLE = -20
        GWL_HINSTANCE = -6
        GWL_HWNDPARENT = -8
        GWL_ID = -12
        GWL_STYLE = -16
        GWL_USERDATA = -21
        GWL_WNDPROC = -4
    End Enum

    ' Hotkey.RegisterHotKey(Me.Handle, 1, Hotkey.KeyModifier.Shift, 9)


    Enum KeyModifier
        None = 0
        Alt = &H1
        Control = &H2
        Shift = &H4
        Winkey = &H8
    End Enum 'This enum is just to make it easier to call the registerHotKey function: The modifier integer codes are replaced by a friendly "Alt","Shift" etc.
#End Region


#Region "Hotkey registration, unregistration and handling"
    Public Shared Sub registerHotkey(ByRef sourceForm As Form, ByVal triggerKey As String, ByVal modifier As KeyModifier)
        RegisterHotKey(sourceForm.Handle, 1, modifier, Asc(triggerKey.ToUpper))
    End Sub
    Public Shared Sub unregisterHotkeys(ByRef sourceForm As Form)
        UnregisterHotKey(sourceForm.Handle, 1)  'Remember to call unregisterHotkeys() when closing your application.
    End Sub
    Public Shared Sub handleHotKeyEvent(ByVal hotkeyID As IntPtr)

        If OverlayForm.Visible = True Then
            SetWindowLong(OverlayForm.Handle, GWLParameter.GWL_HWNDPARENT, Form1.Handle)
            OverlayForm.Hide()
        Else
            OverlayForm.Show()
            SetWindowLong(OverlayForm.Handle, GWLParameter.GWL_HWNDPARENT, Form1.GameHandle)
            SetForegroundWindow(Form1.GameHandle)
        End If

    End Sub
#End Region

End Class

Public Class User32Wrappers

    Public Enum GWL As Integer
        ExStyle = -20
    End Enum

    Public Enum WS_EX As Integer
        Transparent = &H20
        Layered = &H80000
    End Enum

    Public Enum LWA As Integer
        ColorKey = &H1
        Alpha = &H2
    End Enum

    <DllImport("user32.dll", EntryPoint:="GetWindowLong")>
    Public Shared Function GetWindowLong(
        ByVal hWnd As IntPtr,
        ByVal nIndex As GWL
            ) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLong")>
    Public Shared Function SetWindowLong(
        ByVal hWnd As IntPtr,
        ByVal nIndex As GWL,
        ByVal dwNewLong As WS_EX
            ) As Integer
    End Function

    <DllImport("user32.dll",
      EntryPoint:="SetLayeredWindowAttributes")>
    Public Shared Function SetLayeredWindowAttributes(
        ByVal hWnd As IntPtr,
        ByVal crKey As Integer,
        ByVal alpha As Byte,
        ByVal dwFlags As LWA
            ) As Boolean
    End Function
End Class