Public Class CurrentGameTeamData

    Public Class Rune
        Public Property count As Integer
        Public Property runeId As Integer
    End Class

    Public Class Mastery
        Public Property rank As Integer
        Public Property masteryId As Integer
    End Class

    Public Class Participant
        Public Tier As String
        Public LeaguePoints As Integer
        Public teamId As Integer
        Public spell1Id As Integer
        Public spell2Id As Integer
        Public championId As Integer
        Public profileIconId As Integer
        Public summonerName As String
        Public summonerId As Integer
        Public runes As Rune()
        Public masteries As Mastery()
    End Class

End Class
