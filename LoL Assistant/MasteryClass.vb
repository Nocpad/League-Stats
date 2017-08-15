Public Class MasteryClass

    Public Class Mastery
        Public Property id As Integer
        Public Property name As String
        Public Property description As String()
        Public Property masteryTree As String
    End Class

    Public Class Data
        Public Property Mastery As Mastery
    End Class

    Public Class Example
        Public Property type As String
        Public Property version As String
        Public Property data As Data
    End Class
End Class
