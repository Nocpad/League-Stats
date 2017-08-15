Imports System.Runtime.InteropServices
Imports L_Stats.User32Wrappers

Public Class InfoForm
    Private _InitialStyle As Integer
    Private Sub InfoForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Location = New Point(Screen.PrimaryScreen.Bounds.Width - Me.Width - 10, 70)
        _InitialStyle = GetWindowLong(Me.Handle, GWL.ExStyle)
        SetFormToTransparent()
    End Sub

    Private Sub SetFormToTransparent()

        SetWindowLong(Me.Handle, GWL.ExStyle,
            _InitialStyle Or WS_EX.Layered Or WS_EX.Transparent)


        SetLayeredWindowAttributes(Me.Handle, 0,
                           255 * My.Settings.OpacityPercentage, LWA.Alpha)
    End Sub


End Class