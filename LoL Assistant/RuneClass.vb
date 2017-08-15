Public Class RuneClass
    Public Class Stats
        Public Property FlatMagicDamageMod As Double
    End Class

    Public Class Rune
        Public Property isRune As Boolean
        Public Property tier As String
        Public Property type As String
    End Class

    Public Class RuneID
        Public Property id As Integer
        Public Property name As String
        Public Property description As String
        Public Property stats As Stats
        Public Property rune As Rune
    End Class

    Public Class Data
        Public Property Rune As Rune
    End Class

    Public Class Rootobj
        Public Property type As String
        Public Property version As String
        Public Property data As Data
    End Class
End Class
