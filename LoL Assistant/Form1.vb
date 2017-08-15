Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Net
Imports System.ComponentModel
Imports Newtonsoft.Json

Imports L_Stats.GetCurrentGame
Imports System.IO

Public Class Form1

#Region "OVERLAY"

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
    Public Shared Function GetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As GWL) As Integer
    End Function

    '<DllImport("user32.dll", EntryPoint:="SetWindowLong")>
    'Public Shared Function SetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As GWL, ByVal dwNewLong As WS_EX) As Integer
    'End Function

    <DllImport("user32.dll", EntryPoint:="SetLayeredWindowAttributes")>
    Public Shared Function SetLayeredWindowAttributes(ByVal hWnd As IntPtr, ByVal crKey As Integer, ByVal alpha As Byte, ByVal dwFlags As LWA) As Boolean
    End Function

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmExtendFrameIntoClientArea(ByVal hwnd As IntPtr, ByRef margins As Margins) As Integer
    End Function

    Friend Structure Margins
        Public Left As Integer, Right As Integer, Top As Integer, Bottom As Integer
    End Structure

    Private marg As Margins
    Private _InitialStyle As Integer
#End Region

#Region "Windows imports"
    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    End Function

    Private Declare Function SetParent Lib "user32" (ByVal hWndChild As IntPtr, ByVal _
     hWndNewParent As IntPtr) As Integer

    Private Declare Auto Function SetWindowLong Lib "User32.Dll" (ByVal hWnd As IntPtr, ByVal nIndex As Integer, ByVal dwNewLong As Integer) As Integer

    Public Enum GWLParameter
        GWL_EXSTYLE = -20
        GWL_HINSTANCE = -6
        GWL_HWNDPARENT = -8
        GWL_ID = -12
        GWL_STYLE = -16
        GWL_USERDATA = -21
        GWL_WNDPROC = -4
    End Enum

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetForegroundWindow() As IntPtr
    End Function

    Private Declare Function GetWindowLong Lib "user32" Alias "GetWindowLongA" (ByVal hWnd As Integer, ByVal nIndex As Integer) As Integer
    Public Declare Function MoveWindow Lib "user32" (ByVal hwnd As Int32, ByVal x As Long, ByVal y As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal bRepaint As Boolean) As Boolean

    Private Declare Function SetForegroundWindow Lib "user32" (ByVal hWnd As IntPtr) As IntPtr
    Private Declare Function SetFocus Lib "user32.dll" (ByVal hWnd As IntPtr) As Integer

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetActiveWindow(ByVal hWnd As IntPtr) As IntPtr
    End Function
    <DllImport("user32.dll")>
    Shared Function GetAsyncKeyState(ByVal vKey As System.Windows.Forms.Keys) As Short
    End Function
    Private Declare Function GetKeyState Lib "user32" Alias "GetKeyState" (ByVal nVirtKey As Long) As Integer

    Private Const KEY_TOGGLED As Integer = &H1
    Private Const KEY_PRESSED As Integer = &H1000

#End Region

#Region "Variables"
    'Rendering/thread/web/Data variables below
    Public GameFound As Boolean = False
    Public GameHandle As IntPtr

    Private MeasureFont As New System.Drawing.Font("Verdana", 9, FontStyle.Regular)  'Just to measure the width of the text
    Private ApiLink As String = "https://dl.dropboxusercontent.com/s/g1cna0v4bc6kg6p/ApiKey.txt?dl=0"
    Private ApiKey As String = Nothing
    Private ServerRegion As String = Nothing
    Private WebClient As New WebClient
    Private MaxSummonerNameWidth As Integer = 0
    Public MaxSummonerWidth As Integer = 0
    Private MaxSummonerHeight As Integer = 0
    Private PlayersList As New List(Of NewPlayer.Player)()
    Private CurrentName As String = Nothing
    Dim tempName As String = Nothing

    'Summoner variables below
    Private SummonersIDS As String = Nothing
    Private SummonerId As Integer = Nothing
    Private Summoner As SummonerByName
    Private GameMapID As String = Nothing
    Private GameType As String = Nothing

    ' Private ErrorPosition As String = Nothing
    Private NotifyText As String
    Private BlueTeamFirst As Boolean
    Private BMasteryY As Integer
    Private RMasteryY As Integer
    Private MasteryY As Integer
    Dim SkipContraction As Boolean = False

    'Classes used to deserialize the api calls
    Private CurrentGameReply As New GetCurrentGame.CurrentGame
    Private LeagueReply As New GetLeagueInfo.SummonerInfo
    Private ApiReply As String = Nothing
    Private RuneReply As String = Nothing
    Private reply As String = Nothing

#End Region

    Private Sub GetChampionData()

        Dim reply As String = Nothing
        reply = WebClient.DownloadString("https://global.api.pvp.net/api/lol/static-data/" & ServerRegion & "/v1.2/champion?champData=image&api_key=" & ApiKey)
        reply = reply.Substring(reply.IndexOf("data") + 6)
        reply = reply.Substring(0, reply.Length - 1)
        reply = "[" & reply & "]"

        Dim VersionReply = WebClient.DownloadString("https://global.api.pvp.net/api/lol/static-data/" & ServerRegion & "/v1.2/versions?api_key=" & ApiKey)
        VersionReply = VersionReply.Substring(2, VersionReply.Length - 2)
        VersionReply = VersionReply.Substring(0, VersionReply.IndexOf(""""))


        Dim res = JsonConvert.DeserializeObject(Of Dictionary(Of String, ChampionsClass.Champion)())(reply)
        Dim players As Dictionary(Of String, ChampionsClass.Champion) = res(0)
        Try


            For Each p As NewPlayer.Player In PlayersList
                For Each kvp As KeyValuePair(Of String, ChampionsClass.Champion) In players

                    If kvp.Value.id = p.ChampionID Then
                        p.ChampionName = kvp.Value.name
                        ' Clipboard.SetText("http://ddragon.leagueoflegends.com/cdn/" & VersionReply & "/img/champion/" & kvp.Value.image.full)

                        Dim i As New Bitmap(New MemoryStream(WebClient.DownloadData("http://ddragon.leagueoflegends.com/cdn/" & VersionReply & "/img/champion/" & kvp.Value.image.full)))
                        p.ChampionImage = New Bitmap(i, 60, 60)
                        Exit For
                    End If

                Next
            Next
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Rl()
        Dim HeightPos As Integer = 55
        Dim TeamSpacer As Integer = 0
        '''   OverlayForm.MetroLabel1.Text = GameType & " - " & GameMapID

        For Each p As NewPlayer.Player In PlayersList

            If p.TeamID = 100 Then 'Blue team
                Dim l As New Label
                l.Text = p.SummonerName
                l.AutoSize = False
                l.Size = New Size(MaxSummonerWidth, MaxSummonerHeight)
                l.Location = New Point(33, HeightPos + 7)
                l.ForeColor = Color.DeepSkyBlue
                ' l.BackColor = Color.Transparent
                l.Font = New System.Drawing.Font("Verdana", 12)

                Dim pc As New PictureBox
                pc.Size = New Size(30, 30)
                pc.BackgroundImage = p.ChampionImage
                pc.Location = New Point(3, HeightPos)
                pc.BorderStyle = BorderStyle.None

                Dim pcb As New PictureBox
                pcb.Size = New Size(30, 30)
                pcb.BackgroundImage = p.TierImage
                pcb.Location = New Point(MaxSummonerWidth + 30, HeightPos)
                pcb.BorderStyle = BorderStyle.None

                Dim l2 As New Label
                l2.Text = p.Tier & " " & p.Division
                l2.Location = New Point(3 + MaxSummonerWidth + 58, HeightPos + 9)
                l2.ForeColor = Color.DeepSkyBlue
                l2.BackColor = Color.Transparent
                l2.Font = New System.Drawing.Font("Verdana", 9)

                OverlayForm.Controls.Add(l2)
                OverlayForm.Controls.Add(pc)
                OverlayForm.Controls.Add(pcb)
                OverlayForm.Controls.Add(l)
                HeightPos += 33
            Else
                If TeamSpacer = 0 Then
                    TeamSpacer = 10
                    HeightPos += 10
                End If
                Dim l As New Label
                l.Text = p.SummonerName
                l.AutoSize = False
                l.Size = New Size(MaxSummonerWidth, MaxSummonerHeight)
                l.Location = New Point(33, HeightPos + 7)
                l.ForeColor = Color.Red
                l.BackColor = Color.Transparent
                l.Font = New System.Drawing.Font("Verdana", 12)

                Dim pc As New PictureBox
                pc.Size = New Size(30, 30)
                pc.BackgroundImage = p.ChampionImage
                pc.Location = New Point(3, HeightPos)
                pc.BorderStyle = BorderStyle.None

                Dim pcb As New PictureBox
                pcb.Size = New Size(30, 30)
                pcb.BackgroundImage = p.TierImage
                pcb.Location = New Point(MaxSummonerWidth + 30, HeightPos)
                pcb.BorderStyle = BorderStyle.None

                Dim l2 As New Label
                l2.Text = p.Tier & " " & p.Division
                l2.Location = New Point(3 + MaxSummonerWidth + 58, HeightPos + 9)
                l2.ForeColor = Color.Red
                l2.BackColor = Color.Transparent
                l2.Font = New System.Drawing.Font("Verdana", 9)

                OverlayForm.Controls.Add(l2)
                OverlayForm.Controls.Add(pc)
                OverlayForm.Controls.Add(pcb)
                OverlayForm.Controls.Add(l)
                HeightPos += 33

            End If
        Next

        OverlayForm.Show()
        OverlayForm.Size = New Point(3 + MaxSummonerWidth + 55 + 100, HeightPos + 15)
        OverlayForm.Location = New Point(Screen.PrimaryScreen.Bounds.Width - OverlayForm.Width - 10, 100)
        OverlayForm.TopMost = True

        SetWindowLong(OverlayForm.Handle, GWLParameter.GWL_HWNDPARENT, GameHandle)
        SetForegroundWindow(OverlayForm.Handle)
        SetForegroundWindow(GameHandle)

        TimerShorcut.Start()
    End Sub
    Private Sub RenderLoop()
        OverlayForm.SuspendLayout()

        Dim BlueTeamX As Integer = 50
        Dim BlueTeamY As Integer
        Dim RedTeamY As Integer
        Dim RedTeamX As Integer = 50
        Dim F, C, R As Boolean

        If BlueTeamFirst = True Then
            BlueTeamY = 50
            RedTeamY = 300
        Else
            BlueTeamY = 300
            RedTeamY = 50
        End If

        For Each pl As NewPlayer.Player In PlayersList
            If pl.TeamID = 100 Then
                'BLUE TEAM!

                'ChampionImage picturebox
                Dim ChampionImgBox As New PictureBox
                ChampionImgBox.Size = New Size(60, 60)
                ChampionImgBox.Location = New Point(BlueTeamX, BlueTeamY)
                ChampionImgBox.Image = pl.ChampionImage

                'Summoner name next to the champion image
                Dim SummonerLabel As New Label
                SummonerLabel.Text = pl.SummonerName
                SummonerLabel.ForeColor = Color.DeepSkyBlue
                SummonerLabel.Location = New Point(BlueTeamX + 65, BlueTeamY + 5)
                SummonerLabel.Font = New System.Drawing.Font("Verdana", 10, FontStyle.Regular)
                SummonerLabel.BackColor = Color.Transparent

                'Tier image below the name
                Dim TierPictureBox As New PictureBox
                TierPictureBox.Size = New Size(30, 30)
                TierPictureBox.Location = New Point(BlueTeamX + 65, BlueTeamY + 5 + MaxSummonerHeight)
                TierPictureBox.Image = pl.TierImage
                TierPictureBox.BackColor = Color.Transparent

                'Then Tier and division next to the tier image
                Dim TierLabel As New Label
                TierLabel.Text = pl.Tier & " " & pl.Division
                TierLabel.Location = New Point(BlueTeamX + 100, BlueTeamY + 10 + MaxSummonerHeight)
                TierLabel.ForeColor = Color.GhostWhite
                TierLabel.Width = 90
                TierLabel.BackColor = Color.Transparent

                'Getting the masteries json file and edit to deserialize later 
                Dim MasteryReply = WebClient.DownloadString("https://global.api.pvp.net/api/lol/static-data/" & ServerRegion & "/v1.2/mastery?masteryListData=masteryTree&api_key=" & ApiKey)
                MasteryReply = MasteryReply.Substring(MasteryReply.IndexOf("data") + 6)
                MasteryReply = MasteryReply.Substring(0, MasteryReply.Length - 1)
                MasteryReply = "[" & MasteryReply & "]"

                Dim res = JsonConvert.DeserializeObject(Of Dictionary(Of String, MasteryClass.Mastery)())(MasteryReply)
                Dim Masteries As Dictionary(Of String, MasteryClass.Mastery) = res(0)

                MasteryY = BlueTeamY + 45
                Dim FXpoint = -55
                Dim CXPoint = -55
                Dim RXPoint = -55

                Dim FMasteryLabel As New Label
                FMasteryLabel.Text = "Ferocity"
                FMasteryLabel.ForeColor = Color.GhostWhite
                FMasteryLabel.BackColor = Color.Transparent

                Dim CMasteryLabel As New Label
                CMasteryLabel.Text = "Cunning"
                CMasteryLabel.ForeColor = Color.GhostWhite
                CMasteryLabel.BackColor = Color.Transparent

                Dim RMasteryLabel As New Label
                RMasteryLabel.Text = "Resolve"
                RMasteryLabel.ForeColor = Color.GhostWhite
                RMasteryLabel.BackColor = Color.Transparent

                For Each p As Participant In CurrentGameReply.participants
                    If p.summonerName = pl.SummonerName Then
                        For Each M As Mastery In p.masteries

                            For Each kvp As KeyValuePair(Of String, MasteryClass.Mastery) In Masteries
                                If M.masteryId = kvp.Value.id Then
                                    If kvp.Value.masteryTree = "Ferocity" Then

                                        FMasteryLabel.Location = New Point(BlueTeamX, MasteryY)
                                        If FMasteryLabel.Bottom > BMasteryY Then
                                            BMasteryY = FMasteryLabel.Bottom
                                        End If
                                        If F = False Then
                                            MasteryY += 23
                                        End If

                                        F = True
                                        Dim MasteryPictureBox As New PictureBox
                                        MasteryPictureBox.Image = My.Resources.ResourceManager.GetObject("_" & kvp.Value.id)
                                        MasteryPictureBox.Size = New Size(20, 20)
                                        ' MasteryPictureBox.Location = New Point(FMasteryLabel.Location.X + FMasteryLabel.Width + FXpoint, FMasteryLabel.Top)
                                        MasteryPictureBox.Location = New Point(FMasteryLabel.Location.X + FMasteryLabel.Width + FXpoint, MasteryY)

                                        OverlayForm.Controls.Add(MasteryPictureBox)
                                        FXpoint += 22
                                    ElseIf kvp.Value.masteryTree = "Cunning" Then

                                        CMasteryLabel.Location = New Point(BlueTeamX, MasteryY)
                                        If CMasteryLabel.Bottom > BMasteryY Then
                                            BMasteryY = CMasteryLabel.Bottom
                                        End If
                                        If C = False Then
                                            MasteryY += 23
                                        End If

                                        C = True
                                        Dim MasteryPictureBox As New PictureBox
                                        MasteryPictureBox.Image = My.Resources.ResourceManager.GetObject("_" & kvp.Value.id)
                                        MasteryPictureBox.Size = New Size(20, 20)
                                        MasteryPictureBox.Location = New Point(CMasteryLabel.Location.X + CMasteryLabel.Width + CXPoint, MasteryY)

                                        OverlayForm.Controls.Add(MasteryPictureBox)
                                        CXPoint += 22
                                    ElseIf kvp.Value.masteryTree = "Resolve" Then
                                        RMasteryLabel.Location = New Point(BlueTeamX, MasteryY)
                                        If RMasteryLabel.Bottom > BMasteryY Then
                                            BMasteryY = RMasteryLabel.Bottom
                                        End If
                                        If R = False Then
                                            MasteryY += 23
                                        End If

                                        R = True
                                        Dim MasteryPictureBox As New PictureBox
                                        MasteryPictureBox.Image = My.Resources.ResourceManager.GetObject("_" & kvp.Value.id)
                                        MasteryPictureBox.Size = New Size(20, 20)
                                        MasteryPictureBox.Location = New Point(RMasteryLabel.Location.X + RMasteryLabel.Width + RXPoint, MasteryY)

                                        OverlayForm.Controls.Add(MasteryPictureBox)
                                        RXPoint += 22
                                    End If
                                    Exit For
                                End If
                            Next

                        Next
                        Exit For
                    End If
                Next

                If F = True Then OverlayForm.Controls.Add(FMasteryLabel)
                If C = True Then OverlayForm.Controls.Add(CMasteryLabel)
                If R = True Then OverlayForm.Controls.Add(RMasteryLabel)

                F = False
                C = False
                R = False

                OverlayForm.Controls.Add(TierLabel)
                OverlayForm.Controls.Add(ChampionImgBox)
                OverlayForm.Controls.Add(SummonerLabel)
                OverlayForm.Controls.Add(TierPictureBox)
                BlueTeamX += 60 + MaxSummonerWidth + 50
            Else
                'RED TEAM!

                'ChampionImage picturebox
                Dim ChampionImgBox As New PictureBox
                ChampionImgBox.Size = New Size(60, 60)
                ChampionImgBox.Location = New Point(RedTeamX, RedTeamY)
                ChampionImgBox.Image = pl.ChampionImage


                'Summoner name next to the champion image
                Dim SummonerLabel As New Label
                SummonerLabel.Text = pl.SummonerName
                SummonerLabel.ForeColor = Color.Red
                SummonerLabel.Location = New Point(RedTeamX + 65, RedTeamY + 5)
                SummonerLabel.Font = New System.Drawing.Font("Verdana", 10, FontStyle.Regular)
                SummonerLabel.BackColor = Color.Transparent

                'Tier image below the name
                Dim TierPictureBox As New PictureBox
                TierPictureBox.Size = New Size(30, 30)
                TierPictureBox.Location = New Point(RedTeamX + 65, RedTeamY + 5 + MaxSummonerHeight)
                TierPictureBox.Image = pl.TierImage
                TierPictureBox.BackColor = Color.Transparent

                'Then Tier and division next to the tier image
                Dim TierLabel As New Label
                TierLabel.Text = pl.Tier & " " & pl.Division
                TierLabel.Location = New Point(RedTeamX + 100, RedTeamY + 10 + MaxSummonerHeight)
                TierLabel.ForeColor = Color.GhostWhite
                TierLabel.Width = 90
                TierLabel.BackColor = Color.Transparent

                'Getting the masteries json file and edit to deserialize later 
                Dim MasteryReply = WebClient.DownloadString("https://global.api.pvp.net/api/lol/static-data/" & ServerRegion & "/v1.2/mastery?masteryListData=masteryTree&api_key=" & ApiKey)
                MasteryReply = MasteryReply.Substring(MasteryReply.IndexOf("data") + 6)
                MasteryReply = MasteryReply.Substring(0, MasteryReply.Length - 1)
                MasteryReply = "[" & MasteryReply & "]"

                Dim res = JsonConvert.DeserializeObject(Of Dictionary(Of String, MasteryClass.Mastery)())(MasteryReply)
                Dim Masteries As Dictionary(Of String, MasteryClass.Mastery) = res(0)

                MasteryY = RedTeamY + 45

                Dim FXpoint = -55
                Dim CXPoint = -55
                Dim RXPoint = -55

                Dim FMasteryLabel As New Label
                FMasteryLabel.Text = "Ferocity"
                FMasteryLabel.ForeColor = Color.GhostWhite
                FMasteryLabel.BackColor = Color.Transparent

                Dim CMasteryLabel As New Label
                CMasteryLabel.Text = "Cunning"
                CMasteryLabel.ForeColor = Color.GhostWhite
                CMasteryLabel.BackColor = Color.Transparent

                Dim RMasteryLabel As New Label
                RMasteryLabel.Text = "Resolve"
                RMasteryLabel.ForeColor = Color.GhostWhite
                RMasteryLabel.BackColor = Color.Transparent

                For Each p As Participant In CurrentGameReply.participants
                    If p.summonerName = pl.SummonerName Then
                        For Each M As Mastery In p.masteries

                            For Each kvp As KeyValuePair(Of String, MasteryClass.Mastery) In Masteries
                                If M.masteryId = kvp.Value.id Then
                                    If kvp.Value.masteryTree = "Ferocity" Then

                                        FMasteryLabel.Location = New Point(RedTeamX, MasteryY)
                                        If FMasteryLabel.Bottom > RMasteryY Then
                                            RMasteryY = FMasteryLabel.Bottom
                                        End If
                                        If F = False Then
                                            MasteryY += 23
                                        End If

                                        F = True
                                        Dim MasteryPictureBox As New PictureBox
                                        MasteryPictureBox.Image = My.Resources.ResourceManager.GetObject("_" & kvp.Value.id)
                                        MasteryPictureBox.Size = New Size(20, 20)
                                        MasteryPictureBox.Location = New Point(FMasteryLabel.Location.X + FMasteryLabel.Width + FXpoint, MasteryY)

                                        OverlayForm.Controls.Add(MasteryPictureBox)
                                        FXpoint += 22
                                    ElseIf kvp.Value.masteryTree = "Cunning" Then

                                        CMasteryLabel.Location = New Point(RedTeamX, MasteryY)
                                        If CMasteryLabel.Bottom > RMasteryY Then
                                            RMasteryY = CMasteryLabel.Bottom
                                        End If
                                        If C = False Then
                                            MasteryY += 23
                                        End If

                                        C = True
                                        Dim MasteryPictureBox As New PictureBox
                                        MasteryPictureBox.Image = My.Resources.ResourceManager.GetObject("_" & kvp.Value.id)
                                        MasteryPictureBox.Size = New Size(20, 20)
                                        MasteryPictureBox.Location = New Point(CMasteryLabel.Location.X + CMasteryLabel.Width + CXPoint, MasteryY)

                                        OverlayForm.Controls.Add(MasteryPictureBox)
                                        CXPoint += 22
                                    ElseIf kvp.Value.masteryTree = "Resolve" Then
                                        RMasteryLabel.Location = New Point(RedTeamX, MasteryY)
                                        If RMasteryLabel.Bottom > RMasteryY Then
                                            RMasteryY = RMasteryLabel.Bottom
                                        End If
                                        If R = False Then
                                            MasteryY += 23
                                        End If

                                        R = True
                                        Dim MasteryPictureBox As New PictureBox
                                        MasteryPictureBox.Image = My.Resources.ResourceManager.GetObject("_" & kvp.Value.id)
                                        MasteryPictureBox.Size = New Size(20, 20)
                                        MasteryPictureBox.Location = New Point(RMasteryLabel.Location.X + RMasteryLabel.Width + RXPoint, MasteryY)

                                        OverlayForm.Controls.Add(MasteryPictureBox)
                                        RXPoint += 22
                                    End If
                                    Exit For
                                End If
                            Next

                        Next
                        Exit For
                    End If
                Next

                If F = True Then OverlayForm.Controls.Add(FMasteryLabel)
                If C = True Then OverlayForm.Controls.Add(CMasteryLabel)
                If R = True Then OverlayForm.Controls.Add(RMasteryLabel)

                F = False
                C = False
                R = False

                OverlayForm.Controls.Add(TierLabel)
                OverlayForm.Controls.Add(ChampionImgBox)
                OverlayForm.Controls.Add(SummonerLabel)
                OverlayForm.Controls.Add(TierPictureBox)
                RedTeamX += 60 + MaxSummonerWidth + 50

            End If
        Next

        If BlueTeamX >= RedTeamX Then
            OverlayForm.Size = New Size(BlueTeamX + 50, RedTeamY + 200 + 90)
        Else
            OverlayForm.Size = New Size(RedTeamX + 50, RedTeamY + 200 + 90)
        End If
    End Sub

    Private Sub GetRunes()
        Dim BX, BY, RX, RY As Integer

        If BlueTeamFirst = True Then
            BX = 50
            BY = 190

            RX = 50
            RY = 440
        Else
            BX = 50
            BY = 440

            RX = 50
            RY = 190
        End If

        'Editing the Json to deserialize into a dictionary
        RuneReply = RuneReply.Substring(RuneReply.IndexOf("data") + 6)
        RuneReply = RuneReply.Substring(0, RuneReply.Length - 1)
        RuneReply = "[" & RuneReply & "]"

        Dim response = JsonConvert.DeserializeObject(Of Dictionary(Of String, RuneClass.RuneID)())(RuneReply)
        Dim Runes As Dictionary(Of String, RuneClass.RuneID) = response(0)

        For Each Pl As NewPlayer.Player In PlayersList
            Dim RunesText As String = Nothing

            For Each part As Participant In CurrentGameReply.participants
                If Pl.SummonerName = part.summonerName Then

                    Dim symbol As String = Nothing
                    Dim RuneValue As String = Nothing
                    Dim RuneDesc As String = Nothing
                    Dim PercentRune As Boolean = False

                    For Each PlayerRune As Rune In part.runes
                        For Each r As KeyValuePair(Of String, RuneClass.RuneID) In Runes
                            If PlayerRune.runeId = r.Value.id Then
                                'Getting the symbol (+/-)
                                symbol = r.Value.description.Chars(0)

                                'Getting the value of the rune
                                RuneValue = r.Value.description.Substring(0, r.Value.description.IndexOf(" "))
                                If RuneValue.Contains("%") Then
                                    PercentRune = True
                                    Dim tempvalue As String = Nothing
                                    For i = 0 To RuneValue.Length - 2
                                        tempvalue += RuneValue.Chars(i)
                                    Next
                                    RuneValue = tempvalue
                                End If

                                'Getting the description and then removing ( if exist
                                RuneDesc = r.Value.description.Substring(r.Value.description.IndexOf(" "))
                                If RuneDesc.Contains("(") Then
                                    RuneDesc = RuneDesc.Substring(RuneDesc.IndexOf(" "), RuneDesc.IndexOf("("))
                                End If

                                'Getting the Sum for each rune id and then add it on since variable (each rune on different line)
                                Dim b As Double
                                If Double.TryParse(RuneValue, b) Then
                                    If PercentRune = True Then
                                        RunesText += symbol & b * PlayerRune.count & "% " & RuneDesc & vbNewLine
                                    Else
                                        RunesText += symbol & " " & b * PlayerRune.count & " " & RuneDesc & vbNewLine
                                    End If
                                End If
                                'Setting the PercentRune variable back to false
                                PercentRune = False
                            End If
                        Next
                    Next
                    ' MsgBox(RunesText)
                End If
            Next
            'last code before the next player in list (Adding the runes as a label using multiple lines)
            If Pl.TeamID = 100 Then
                Dim l As New Label
                l.Text = RunesText
                l.ForeColor = Color.GreenYellow
                l.Location = New Point(BX, BMasteryY)
                l.Size = New Size(60 + MaxSummonerWidth + 50, 90)
                l.BackColor = Color.Transparent
                OverlayForm.Controls.Add(l)

                BX += 60 + MaxSummonerWidth + 50
            Else
                Dim l As New Label
                l.Text = RunesText
                l.ForeColor = Color.GreenYellow
                l.Location = New Point(RX, RMasteryY)
                l.Size = New Size(60 + MaxSummonerWidth + 50, 90)
                l.BackColor = Color.Transparent
                OverlayForm.Controls.Add(l)

                RX += 60 + MaxSummonerWidth + 50
            End If
        Next


        OverlayForm.Show()
        OverlayForm.Location = New Point((Screen.PrimaryScreen.Bounds.Width / 2) - (OverlayForm.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - (OverlayForm.Height / 2))
        OverlayForm.ResumeLayout()
        OverlayForm.TopMost = True

        SetWindowLong(OverlayForm.Handle, GWLParameter.GWL_HWNDPARENT, GameHandle)
        SetForegroundWindow(GameHandle)
        SetForegroundWindow(OverlayForm.Handle)
    End Sub

    Private Sub GetApiKey()
        'Getting the api key first 
        Try
            ApiKey = WebClient.DownloadString("https://dl.dropboxusercontent.com/s/g1cna0v4bc6kg6p/ApiKey.txt?dl=0")
            NotifyText = "Acquired api key"
            NotifyIcon1.Text = NotifyText

            GetSummonerId()
        Catch ex As Exception
            NotifyIcon1.BalloonTipText = "Could not acquire the api key! "
            NotifyIcon1.ShowBalloonTip(5)
        End Try
    End Sub
    Private Sub GetSummonerId()
        Try
            Dim reply As String = WebClient.DownloadString("https://" & ServerRegion & "1.api.riotgames.com/lol/summoner/v3/summoners/by-name/" & My.Settings.SummonerName & "?api_key=" & ApiKey)

            Summoner = New SummonerByName
            Summoner = JsonConvert.DeserializeObject(Of SummonerByName)(reply)
            SummonerId = Summoner.id

            My.Settings.Save()

            NotifyText = NotifyText & vbNewLine & "Summoner: " & My.Settings.SummonerName & vbNewLine & "Summoner ID: " & SummonerId
            NotifyIcon1.Text = NotifyText

            'Starting the timer in order to check if the game is running 
            Timer1.Start()
        Catch ex As Exception
            If ex.Message.Contains("(404) Not Found") Then
                NotifyIcon1.BalloonTipText = ("No summoner data found for any specified inputs. Make sure that your summoner name & region is corrent.")
                My.Settings.SummonerName = Nothing
                My.Settings.SummonerID = Nothing
                My.Settings.ServerRegion = Nothing

                'Me.Opacity = 100
                Me.Show()
            ElseIf ex.Message.Contains("(403) Forbidden") Then
                NotifyIcon1.BalloonTipText = ("The remote server returned an error: (403) Forbidden. Make sure that your summoner name & region is correct")
                My.Settings.SummonerName = Nothing
                My.Settings.SummonerID = Nothing
                My.Settings.ServerRegion = Nothing
                NotifyIcon1.ShowBalloonTip(5)
                ' Me.Opacity = 100
                Me.Show()
            ElseIf ex.Message.Contains("Internal server error") Then
                NotifyIcon1.BalloonTipText = ("Internal server error. League Of Legends server is unavailable")
                NotifyIcon1.ShowBalloonTip(5)
            ElseIf ex.Message.Contains("Service unavailable") Then
                NotifyIcon1.BalloonTipText = ("League Of Legends server is unavailable")
                NotifyIcon1.ShowBalloonTip(5)
            Else
                'If for whatever reason throw an error display it via notify icon & restart the timer1 (active game checker)
                NotifyIcon1.BalloonTipText = "Could not acquire summoners ID."
                NotifyIcon1.ShowBalloonTip(5)
                Timer1.Start()
            End If
        End Try

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load


        OverlayForm.Show()
        OverlayForm.Hide()
        'Adding server regions
        ComboBox1.Items.Add("Brazil")
        ComboBox1.Items.Add("EU Nordic & East")
        ComboBox1.Items.Add("Eu West")
        ComboBox1.Items.Add("Korea")
        ComboBox1.Items.Add("Latin America North")
        ComboBox1.Items.Add("Latin America South")
        ComboBox1.Items.Add("North America")
        ComboBox1.Items.Add("Oceania")
        ComboBox1.Items.Add("Russia")
        ComboBox1.Items.Add("Turkey")

        'Check if My.settings.username exist else ask for user and start timers (timer1 game process checker)
        If My.Settings.SummonerName = Nothing Or My.Settings.ServerRegion = Nothing Then
            'Show the form so the user can enter username
        Else
            WatermarkTextBox1.Text = My.Settings.SummonerName
            WatermarkTextBox1.ReadOnly = True
            ' Button1.Enabled = False
            ServerRegion = My.Settings.ServerRegion
            ComboBox1.Text = My.Settings.ServerRegion
            '  ComboBox1.Enabled = False
            SummonerId = My.Settings.SummonerID
            GetApiKey()
        End If
    End Sub

    Private Sub ThreadCall()
        GetGameData()
    End Sub

    Private Sub GetGameType()
        Select Case CurrentGameReply.gameQueueConfigId
            Case 0
                GameType = "Custom game"
            Case 8
                GameType = "Normal"
            Case 2
                GameType = "Normal Blind Pick"
            Case 14
                GameType = "Normal Draft Pick"
            Case 4
                GameType = "Ranked Solo"
            Case 6
                GameType = "Ranked Premade"
            Case 9
                GameType = "Ranked Premade 3v3"
            Case 41
                GameType = "Ranked Team 3v3"
            Case 42
                GameType = "Ranked Team 5v5"
            Case 16
                GameType = "Dominion Blind Pick"
            Case 17
                GameType = "Dominion Draft Pick"
            Case 7
                GameType = "Coop vs AI"
            Case 25
                GameType = "Coop vs AI"
            Case 31
                GameType = "Coop vs AI Intro Bot"
            Case 32
                GameType = "Coop vs AI Beginner Bot"
            Case 33
                GameType = "Coop vs AI Intermediate Bot"
            Case 52
                GameType = "Coop vs AI "
            Case 61
                GameType = "Team Builder"
            Case 65
                GameType = "ARAM"
            Case 70
                GameType = "One for All"
            Case 72
                GameType = "Snowdown Showdown 1v1"
            Case 73
                GameType = "Snowdown Showdown 2v2"
            Case 75
                GameType = "Hexakill "
            Case 76
                GameType = "Ultra Rapid Fire"
            Case 83
                GameType = "Ultra Rapid Fire vs AI"
            Case 91
                GameType = "Doom Bots Rank 1"
            Case 92
                GameType = "Doom Bots Rank 2 "
            Case 93
                GameType = "Doom Bots Rank 5"
            Case 96
                GameType = "Ascension"
            Case 98
                GameType = "Hexakill "
            Case 100
                GameType = "Butcher's Bridge"
            Case 300
                GameType = "King Poro"
            Case 310
                GameType = "Nemesis"
            Case 313
                GameType = "Black Market Brawlers"
            Case 400
                GameType = "Team Builder Draft"
            Case 410
                GameType = "Team Builder Draft ranked"
        End Select
    End Sub

    Private Sub GetMapID()
        Select Case CurrentGameReply.mapId
            Case 1
                GameMapID = "Summoner's Rift Summer"
            Case 2
                GameMapID = "Summoner's Rift Autumn"
            Case 3
                GameMapID = "The Proving Grounds"
            Case 4
                GameMapID = "Twisted Treeline"
            Case 8
                GameMapID = "The Crystal Scar"
            Case 10
                GameMapID = "Twisted Treeline"
            Case 11
                GameMapID = "Summoner's Rift"
            Case 12
                GameMapID = "Howling Abyss"
            Case 14
                GameMapID = "Butcher's Bridge"
        End Select
    End Sub

    Private Sub EncodeTheNames()
        Try
            Dim sz As SizeF = Nothing
            Dim testString As String = Nothing
            Dim g As Graphics = Graphics.FromHwnd(Me.Handle)
            Dim tempwidth As Integer = 0

            testString = GameMapID & " " & GameType
            sz = g.MeasureString(testString, MeasureFont)
            MaxSummonerNameWidth = Int(sz.Width.ToString)

            SummonersIDS = Nothing

            For Each p As Participant In CurrentGameReply.participants
                If CurrentName = p.summonerName Then
                Else
                    tempName = p.summonerName

                    SummonersIDS += p.summonerId & ","

                    Dim ch() As Char = p.summonerName.ToCharArray()
                    Dim bytes(ch.Length) As Byte
                    For i = 0 To (ch.Length - 1)
                        bytes(i) = System.Convert.ToByte(ch(i))
                    Next
                    Dim str As String = System.Text.Encoding.UTF8.GetString(bytes)
                    p.summonerName = str

                    testString = p.summonerName
                    sz = g.MeasureString(testString, MeasureFont)
                    If Int(sz.Width.ToString) > MaxSummonerWidth Then
                        MaxSummonerWidth = Int(sz.Width.ToString)
                        MaxSummonerHeight = Int(sz.Height.ToString)
                    End If
                End If
            Next

        Catch ex As Exception
            CurrentName = tempName
            EncodeTheNames()
        End Try
    End Sub

    Private Sub GetGameData()
        Try
            'Check the selected region to choose the proper link for the current game info request.

            If ServerRegion = "ru" Then
                reply = WebClient.DownloadString("https://" & ServerRegion & ".api.pvp.net/observer-mode/rest/consumer/getSpectatorGameInfo/" & ServerRegion.ToUpper & "/" & SummonerId & "?api_key=" & ApiKey)
            ElseIf ServerRegion = "kr" Then
                reply = WebClient.DownloadString("https://" & ServerRegion & ".api.pvp.net/observer-mode/rest/consumer/getSpectatorGameInfo/" & ServerRegion.ToUpper & "/" & SummonerId & "?api_key=" & ApiKey)
            ElseIf ServerRegion = "eune" Then
                reply = WebClient.DownloadString("https://" & ServerRegion & ".api.pvp.net/observer-mode/rest/consumer/getSpectatorGameInfo/EUN1/" & SummonerId & "?api_key=" & ApiKey)
            Else
                reply = WebClient.DownloadString("https://" & ServerRegion & "1.api.riotgames.com/lol/spectator/v3/active-games/by-summoner/" & SummonerId & "?api_key=" & ApiKey)
            End If

        Catch ex As Exception
            If ex.Message.Contains("(404) Not Found.") Then
                NotifyIcon1.BalloonTipText = "No active game found for " & My.Settings.SummonerName & vbNewLine & "League Stats will stop until your next game"
                NotifyIcon1.ShowBalloonTip(5)
                Exit Sub
            Else
                NotifyIcon1.BalloonTipText = ex.Message
                NotifyIcon1.ShowBalloonTip(5)
            End If
            Timer2.Stop()
            Timer1.Start()
        End Try

        'Deserializing the current game info
        CurrentGameReply = JsonConvert.DeserializeObject(Of GetCurrentGame.CurrentGame)(reply)
        NotifyIcon1.BalloonTipText = "Active game found!" & vbNewLine & "Game id: " & CurrentGameReply.gameId
        NotifyIcon1.ShowBalloonTip(3)


        Try
            'Api call to get league info for the summoners
            ApiReply = WebClient.DownloadString("https://" & ServerRegion & ".api.pvp.net/api/lol/" & ServerRegion & "/v2.5/league/by-summoner/" & SummonersIDS & "/entry?api_key=" & ApiKey)
        Catch ex As Exception
            ' LeagueNotFound()
            SkipContraction = True
        End Try

        GetGameType()
        GetMapID()

        'Encode the names taken by the api and encode to utf-8
        EncodeTheNames()

        For Each p As Participant In CurrentGameReply.participants
            If p.teamId = 100 Then
                BlueTeamFirst = True
                Exit For
            End If
        Next

        'Then constract the teams
        ConstractTheTeams()

        'Pass the image for the tier on each participant
        GetTierImage()
        GetChampionData()

        'Creating the controls for each summoner and add the on overlay form
        RenderLoop()
        RuneReply = WebClient.DownloadString("https://global.api.pvp.net/api/lol/static-data/" & ServerRegion & "/v1.2/rune?runeListData=stats&api_key=" & ApiKey)

        GetRunes()
        TimerShorcut.Start()

        '  PlayersHistory("read")
        PlayersHistory("write")
    End Sub

    Private Sub PlayersHistory(ByVal task As String)
        If task = "read" Then

            If File.Exists(Application.StartupPath & "\PlayerHistory.txt") Then
                Dim PlayerHistoryList As String()
                Dim Players As New List(Of String)
                PlayerHistoryList = File.ReadAllLines(Application.StartupPath & "\PlayerHistory.txt")

                If PlayerHistoryList.Count > 0 Then
                    For Each n As String In PlayerHistoryList
                        For Each P As NewPlayer.Player In PlayersList
                            If n.Contains(P.SummonerName) Then
                                Dim t As String() = n.Split(",")
                                If t(3) <> CurrentGameReply.gameId Then

                                    Players.Add(P.SummonerName)
                                End If
                            End If
                        Next
                    Next

                    If Players.Count > 0 Then
                        MsgBox("There are players that you have played with in past!")
                        For Each i As String In Players
                            MsgBox(i)
                        Next
                    End If
                End If
            End If


        ElseIf task = "write" Then
            If File.Exists(Application.StartupPath & "\PlayerHistory.txt") Then

                Dim PlayerHistoryList As String()
                PlayerHistoryList = File.ReadAllLines(Application.StartupPath & "\PlayerHistory.txt")

                Dim ch() As Char = My.Settings.SummonerName.ToCharArray()
                Dim bytes(ch.Length) As Byte
                For i = 0 To (ch.Length - 1)
                    bytes(i) = System.Convert.ToByte(ch(i))
                Next
                Dim str As String = System.Text.Encoding.UTF8.GetString(bytes)

                For Each p As NewPlayer.Player In PlayersList
                    If Not PlayerHistoryList.Contains(p.SummonerName) And Not p.SummonerName = str Then
                        My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\PlayerHistory.txt", p.SummonerName & "," & p.ChampionName & "," & p.Tier & " " & p.Division & "," & CurrentGameReply.gameId & vbNewLine, True)
                    End If
                Next
            Else

                Dim fs As FileStream = File.Create(Application.StartupPath & "\PlayerHistory.txt")

                fs.Close()
                Dim ch() As Char = My.Settings.SummonerName.ToCharArray()
                Dim bytes(ch.Length) As Byte
                For i = 0 To (ch.Length - 1)
                    bytes(i) = System.Convert.ToByte(ch(i))
                Next
                Dim str As String = System.Text.Encoding.UTF8.GetString(bytes)

                For Each p As NewPlayer.Player In PlayersList
                    If p.SummonerName <> str Then
                        My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\PlayerHistory.txt", p.SummonerName & vbNewLine, True)
                    End If
                Next
            End If
        End If

    End Sub

    Private Sub GetTierImage()
        Try
            'The league's tier. (Legal values: CHALLENGER, MASTER, DIAMOND, PLATINUM, GOLD, SILVER, BRONZE)
            For Each p As NewPlayer.Player In PlayersList
                If p.Tier = "GOLD" Then
                    p.TierImage = New Bitmap(My.Resources.gold, 30, 30)
                ElseIf p.Tier = "BRONZE" Then
                    p.TierImage = New Bitmap(My.Resources.bronze, 30, 30)
                ElseIf p.Tier = "SILVER" Then
                    p.TierImage = New Bitmap(My.Resources.silver, 30, 30)
                ElseIf p.Tier = "PLATINUM" Then
                    p.TierImage = New Bitmap(My.Resources.platinum, 30, 30)
                ElseIf p.Tier = "DIAMOND" Then
                    p.TierImage = New Bitmap(My.Resources.diamond, 30, 30)
                ElseIf p.Tier = "MASTER" Then
                    p.TierImage = New Bitmap(My.Resources.master, 30, 30)
                ElseIf p.Tier = "CHALLENGER" Then
                    p.TierImage = New Bitmap(My.Resources.challenger, 30, 30)
                Else
                    p.TierImage = New Bitmap(My.Resources.provisional, 30, 30)
                End If
            Next
        Catch ex As Exception
        End Try
    End Sub

    Private Sub LeagueNotFound()
        PlayersList.Clear()
        Try
            For Each p As Participant In CurrentGameReply.participants
                Dim pl As New NewPlayer.Player
                pl.TeamID = p.teamId
                pl.SummonerName = p.summonerName
                pl.SummonerID = p.summonerId
                pl.Tier = "N/A"
                pl.ChampionID = p.championId

                PlayersList.Add(pl)
            Next
        Catch ex As Exception
        End Try
    End Sub

    Private Sub ConstractTheTeams()
        PlayersList.Clear()

        Try
            Dim result = JsonConvert.DeserializeObject(Of Dictionary(Of String, GetLeagueInfo.SummonerInfo()))(ApiReply)

            For Each p As Participant In CurrentGameReply.participants
                'If p.teamId = 100 Then
                Dim pl As New NewPlayer.Player
                pl.TeamID = p.teamId
                pl.SummonerName = p.summonerName
                pl.SummonerID = p.summonerId
                pl.Tier = "Unranked"
                pl.LeaguePoints = 0
                pl.Wins = 0
                pl.Losses = 0
                pl.ChampionID = p.championId

                For Each kvp As KeyValuePair(Of String, GetLeagueInfo.SummonerInfo()) In result

                    For Each sum As GetLeagueInfo.SummonerInfo In kvp.Value
                        For Each en As GetLeagueInfo.Entry In sum.entries
                            If p.summonerId.ToString = en.playerOrTeamId Then
                                pl.Tier = sum.tier
                                pl.Division = en.division
                                pl.Wins = en.wins
                                pl.Losses = en.losses
                                If PlayersList.Contains(pl) Then
                                Else
                                    PlayersList.Add(pl)
                                End If
                            Else

                                If PlayersList.Contains(pl) Then
                                Else
                                    PlayersList.Add(pl)
                                End If
                            End If
                        Next
                    Next
                Next
            Next
        Catch ex As Exception
            Try
                If ex.Message.Contains("be null") Then '404 no league
                    'Adding all the players as unranked
                    For Each p As Participant In CurrentGameReply.participants
                        Dim pl As New NewPlayer.Player
                        pl.TeamID = p.teamId
                        pl.SummonerName = p.summonerName
                        pl.SummonerID = p.summonerId
                        pl.Tier = "Unranked"
                        pl.LeaguePoints = 0
                        pl.Wins = 0
                        pl.Losses = 0
                        PlayersList.Add(pl)
                    Next
                End If
            Catch exe As Exception
                Timer2.Stop()
                Timer1.Start()
            End Try
        End Try
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        GameHandle = FindWindow(vbNullString, "League of Legends (TM) Client")
        If GameHandle.ToString <> "0" Then
            GameFound = True
            Timer2.Start()
            Timer1.Stop()

            'Redim OperatingThread and start render loop

            ThreadCall()
        End If
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        GameHandle = FindWindow(vbNullString, "League of Legends (TM) Client")
        'Clearing the screen if GameHandle = 0 & restart timer1
        If GameHandle.ToString = "0" Then
            GameFound = False
            Timer1.Start()
            Timer2.Stop()
            OverlayForm.Close()
            OverlayForm.Show()
            OverlayForm.Hide()
            TimerShorcut.Stop()
        End If
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        NotifyIcon1.Icon.Dispose()
        NotifyIcon1.Dispose()

        My.Settings.Save()
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If My.Settings.SummonerName = Nothing Then
            Me.Show()
        Else
            Me.Hide()
        End If
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Close()
    End Sub

    Private Sub ChangeSummonerNameToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ChangeSummonerNameToolStripMenuItem.Click
        Me.Show()
        WatermarkTextBox1.Text = ""
        WatermarkTextBox1.ReadOnly = False
        ComboBox1.Text = ""
        ComboBox1.Enabled = True
        Button1.Enabled = True
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim SelText As String = ComboBox1.SelectedItem

        Select Case SelText
            Case "Brazil"
                ServerRegion = "br"
                ComboBox1.Text = ServerRegion
            Case "EU Nordic & East"
                ServerRegion = "eune"
                ComboBox1.Text = ServerRegion
            Case "Eu West"
                ServerRegion = "euw"
                ComboBox1.Text = ServerRegion
            Case "Korea"
                ServerRegion = "kr"
                ComboBox1.Text = ServerRegion
            Case "Latin America North"
                ServerRegion = "lan"
                ComboBox1.Text = ServerRegion
            Case "Latin America South"
                ServerRegion = "las"
                ComboBox1.Text = ServerRegion
            Case "North America"
                ServerRegion = "na"
                ComboBox1.Text = ServerRegion
            Case "Oceania"
                ServerRegion = "oce"
                ComboBox1.Text = ServerRegion
            Case "Russia"
                ServerRegion = "ru"
                ComboBox1.Text = ServerRegion
            Case "Turkey"
                ServerRegion = "tr"
                ComboBox1.Text = ServerRegion
        End Select
        My.Settings.Save()
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click
        Me.Close()
    End Sub

    Private Sub Label5_MouseEnter(sender As Object, e As EventArgs) Handles Label5.MouseEnter
        Label5.ForeColor = Color.Red
    End Sub

    Private Sub Label5_MouseLeave(sender As Object, e As EventArgs) Handles Label5.MouseLeave
        Label5.ForeColor = Color.White
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If WatermarkTextBox1.Text.Length > 0 And ComboBox1.Text.Length > 0 Then
            My.Settings.SummonerName = WatermarkTextBox1.Text
            My.Settings.ServerRegion = ServerRegion
            My.Settings.SummonerID = Nothing
            Me.Hide()
            My.Settings.Save()
            GetApiKey()
        Else
            MsgBox("Please enter you summoner name & region.")
        End If
    End Sub

    Private Sub TimerShorcut_Tick(sender As Object, e As EventArgs) Handles TimerShorcut.Tick
        If (GetKeyState(Keys.ShiftKey) And KEY_PRESSED) AndAlso (GetKeyState(Keys.Tab) And KEY_PRESSED) > 0 Then
            If OverlayForm.Visible = True Then
                SetWindowLong(OverlayForm.Handle, GWLParameter.GWL_HWNDPARENT, Handle)
                OverlayForm.TopMost = False
                OverlayForm.Hide()
                Thread.Sleep(400)
                Exit Sub
            Else
                OverlayForm.Show()
                OverlayForm.TopMost = True
                SetWindowLong(OverlayForm.Handle, GWLParameter.GWL_HWNDPARENT, GameHandle)
                SetForegroundWindow(GameHandle)
                SetForegroundWindow(OverlayForm.Handle)
                SetForegroundWindow(GameHandle)

                Thread.Sleep(400)
                Exit Sub
            End If
        End If

    End Sub
End Class

Public Class WatermarkTextBox
    Inherits TextBox

    Private NotInheritable Class NativeMethods
        Private Sub New()
        End Sub

        Private Const ECM_FIRST As UInteger = &H1500
        Friend Const EM_SETCUEBANNER As UInteger = ECM_FIRST + 1

        <DllImport("user32.dll", EntryPoint:="SendMessageW")>
        Public Shared Function SendMessageW(hWnd As IntPtr, Msg As UInt32, wParam As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> lParam As String) As IntPtr
        End Function
    End Class

    Private _watermark As String
    Public Property Watermark() As String
        Get
            Return _watermark
        End Get
        Set(value As String)
            _watermark = value
            UpdateWatermark()
        End Set
    End Property

    Private Sub UpdateWatermark()
        If IsHandleCreated AndAlso _watermark IsNot Nothing Then
            NativeMethods.SendMessageW(Handle, NativeMethods.EM_SETCUEBANNER, CType(1, IntPtr), _watermark)
        End If
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        UpdateWatermark()
    End Sub

End Class


