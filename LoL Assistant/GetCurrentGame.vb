Public Class GetCurrentGame
    Public Class Rune
        Public Property count As Integer
        Public Property runeId As Integer
    End Class

    Public Class Mastery
        Public Property rank As Integer
        Public Property masteryId As Integer
    End Class

    Public Class Participant
        Public Property teamId As Integer
        Public Property spell1Id As Integer
        Public Property spell2Id As Integer
        Public Property championId As Integer
        Public Property profileIconId As Integer
        Public Property summonerName As String
        Public Property bot As Boolean
        Public Property summonerId As Integer
        Public Property runes As Rune()
        Public Property masteries As Mastery()
    End Class

    Public Class Observers
        Public Property encryptionKey As String
    End Class

    Public Class CurrentGame
        Public Property gameId As Long
        Public Property mapId As Integer
        Public Property gameMode As String
        Public Property gameType As String
        Public Property gameQueueConfigId As Integer
        Public Property participants As Participant()
        Public Property observers As Observers
        Public Property platformId As String
        Public Property bannedChampions As Object()
        Public Property gameStartTime As Long
        Public Property gameLength As Integer
    End Class
End Class
