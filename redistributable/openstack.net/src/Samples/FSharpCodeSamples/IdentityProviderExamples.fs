module IdentityProviderExamples

open System
open net.openstack.Core.Domain
open net.openstack.Providers.Rackspace

let createProvider =
    //#region CreateProvider
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#endregion
    ()

let createProviderWithPassword =
    //#region CreateProviderWithPassword
    let identity = new CloudIdentity (Username = "{username}", Password = "{password}")
    let provider = new CloudIdentityProvider(identity)
    //#endregion
    ()

let createUser =
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#region CreateUser
    let user = new NewUser("{username}", "{email}", enabled=true)
    let user = provider.AddUser(user, null)
    let password = user.Password
    //#endregion
    ()

let updateUser =
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#region UpdateUser
    let user = provider.GetUserByName("{username}", null)
    user.Username <- "{newUsername}"
    provider.UpdateUser(user, null) |> ignore
    //#endregion
    ()

let listUsers =
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#region ListUsers
    let users = provider.ListUsers(null)
    for user in users do
        Console.WriteLine("{0}: {1}", user.Id, user.Username)
    //#endregion
    ()

let addRoleToUser =
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#region AddRoleToUser
    let user = provider.GetUserByName("{username}", null)
    provider.AddRoleToUser(user.Id, "{roleId}", null) |> ignore
    //#endregion
    ()

let deleteRoleFromUser =
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#region DeleteRoleFromUser
    let user = provider.GetUserByName("{username}", null)
    provider.DeleteRoleFromUser(user.Id, "{roleId}", null) |> ignore
    //#endregion
    ()

let resetApiKey =
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#region ResetApiKey
    let credential = provider.ResetApiKey("{userId}")
    let newApiKey = credential.APIKey
    //#endregion
    ()

let listRoles =
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#region ListRoles
    let roles = provider.ListRoles()
    for role in roles do
        Console.WriteLine("{0}: {1}", role.Id, role.Name)
    //#endregion
    ()

let deleteUser =
    let identity = new CloudIdentity (Username = "{username}", APIKey = "{apiKey}")
    let provider = new CloudIdentityProvider(identity)
    //#region DeleteUser
    provider.DeleteUser("{userId}", null) |> ignore
    //#endregion
    ()
