Public Class ChampionsClass

    Public Class Info
        Public Property attack As Integer
        Public Property defense As Integer
        Public Property magic As Integer
        Public Property difficulty As Integer
    End Class

    Public Class Image
        Public Property full As String
        Public Property sprite As String
        Public Property group As String
        Public Property x As Integer
        Public Property y As Integer
        Public Property w As Integer
        Public Property h As Integer
    End Class

    Public Class Stats
        Public Property hp As Double
        Public Property hpperlevel As Double
        Public Property mp As Double
        Public Property mpperlevel As Double
        Public Property movespeed As Double
        Public Property armor As Double
        Public Property armorperlevel As Double
        Public Property spellblock As Double
        Public Property spellblockperlevel As Double
        Public Property attackrange As Double
        Public Property hpregen As Double
        Public Property hpregenperlevel As Double
        Public Property mpregen As Double
        Public Property mpregenperlevel As Double
        Public Property crit As Double
        Public Property critperlevel As Double
        Public Property attackdamage As Double
        Public Property attackdamageperlevel As Double
        Public Property attackspeedoffset As Double
        Public Property attackspeedperlevel As Double
    End Class

    Public Class Champion
        Public Property id As Integer
        Public Property key As String
        Public Property name As String
        Public Property title As String
        Public Property image As Image
    End Class

    Public Class Example
        Public Property Champion As Champion
    End Class
End Class