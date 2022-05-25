Imports net.openstack.Core.Domain
Imports net.openstack.Providers.Rackspace

Public Class IdentityProviderExamples

    Public Sub CreateProvider()
        ' #Region "CreateProvider"
        Dim identity = New CloudIdentity With {.Username = "{username}", .APIKey = "{apiKey}"}
        Dim provider = New CloudIdentityProvider(identity)
        ' #End Region
    End Sub

    Public Sub CreateProviderWithPassword()
        ' #Region "CreateProviderWithPassword"
        Dim identity = New CloudIdentity With {.Username = "{username}", .Password = "{password}"}
        Dim provider = New CloudIdentityProvider(identity)
        ' #End Region
    End Sub

    Public Sub CreateUser()
        Dim identity = New CloudIdentity
        Dim provider = New CloudIdentityProvider(identity)

        ' #Region "CreateUser"
        Dim User = New NewUser("{username}", "{email}", enabled:=True)
        User = provider.AddUser(User, Nothing)
        Dim password = User.Password
        ' #End Region
    End Sub

    Public Sub UpdateUser()
        Dim identity = New CloudIdentity
        Dim provider = New CloudIdentityProvider(identity)

        ' #Region "UpdateUser"
        Dim user = provider.GetUserByName("{username}", Nothing)
        user.Username = "{newUsername}"
        provider.UpdateUser(user, Nothing)
        ' #End Region
    End Sub

    Public Sub ListUsers()
        Dim identity = New CloudIdentity
        Dim provider = New CloudIdentityProvider(identity)

        ' #Region "ListUsers"
        Dim users = provider.ListUsers(Nothing)
        For Each user As User In users
            Console.WriteLine("{0}: {1}", user.Id, user.Username)
        Next
        ' #End Region
    End Sub

    Public Sub AddRoleToUser()
        Dim identity = New CloudIdentity
        Dim provider = New CloudIdentityProvider(identity)

        ' #Region "AddRoleToUser"
        Dim user = provider.GetUserByName("{username}", Nothing)
        provider.AddRoleToUser(user.Id, "{roleId}", Nothing)
        ' #End Region
    End Sub

    Public Sub DeleteRoleFromUser()
        Dim identity = New CloudIdentity
        Dim provider = New CloudIdentityProvider(identity)

        ' #Region "DeleteRoleFromUser"
        Dim user = provider.GetUserByName("{username}", Nothing)
        provider.DeleteRoleFromUser(user.Id, "{roleId}", Nothing)
        ' #End Region
    End Sub

    Public Sub ResetApiKey()
        Dim identity = New CloudIdentity
        Dim provider = New CloudIdentityProvider(identity)

        ' #Region "ResetApiKey"
        Dim credential = provider.ResetApiKey("{userId}")
        Dim newApiKey As String = credential.APIKey
        ' #End Region
    End Sub

    Public Sub ListRoles()
        Dim identity = New CloudIdentity
        Dim provider = New CloudIdentityProvider(identity)

        ' #Region "ListRoles"
        Dim roles = provider.ListRoles
        For Each role As Role In roles
            Console.WriteLine("{0}: {1}", role.Id, role.Name)
        Next
        ' #End Region
    End Sub

    Public Sub DeleteUser()
        Dim identity = New CloudIdentity
        Dim provider = New CloudIdentityProvider(identity)

        ' #Region "DeleteUser"
        provider.DeleteUser("{userId}", Nothing)
        ' #End Region
    End Sub

End Class
