Imports Newtonsoft.Json

Public Class GetLeagueInfo

    Public Class Entry
        Public Property playerOrTeamId As String
        Public Property playerOrTeamName As String
        Public Property division As String
        Public Property leaguePoints As Integer
        Public Property wins As Integer
        Public Property losses As Integer
        Public Property isHotStreak As Boolean
        Public Property isVeteran As Boolean
        Public Property isFreshBlood As Boolean
        Public Property isInactive As Boolean
    End Class

    Public Class SummonerInfo
        Public Property name As String
        Public Property tier As String
        Public Property queue As String
        Public Property entries As Entry()
    End Class

    Public Class LeagueInfo

        Public Property SummonerList() As List(Of SummonerInfo)
    End Class
End Class