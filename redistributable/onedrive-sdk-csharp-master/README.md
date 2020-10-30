# OneDrive SDK for CSharp

[![Build status](https://ci.appveyor.com/api/projects/status/fs9ddrmdev37v012/branch/master?svg=true)](https://ci.appveyor.com/project/OneDrive/onedrive-sdk-csharp/branch/master)

Integrate the [OneDrive API](https://dev.onedrive.com/README.htm) into your C#
project!

The OneDrive SDK is built as a Portable Class Library and targets the following
frameworks: 

* .NET 4.5.1 
* .NET for Windows Store apps 
* Windows Phone 8.1 and higher

Azure Active Directory authentication is available for:

* Windows Forms apps
* UWP apps
* Windows 8.1 apps

## Installation via Nuget

To install the OneDrive SDK via NuGet

* Search for `Microsoft.OneDriveSDK` in the NuGet Library, or
* Type `Install-Package Microsoft.OneDriveSDK` into the Package Manager Console.

## Getting started

### 1. Register your application

Register your application for OneDrive by following [these](https://dev.onedrive.com/app-registration.htm) steps.

### 2. Setting your application Id and scopes

Your app must requests permissions in order to access a user's OneDrive. To do this, specify your app ID and scopes, or permission level.
For more information, see [Authentication scopes](https://dev.onedrive.com/auth/msa_oauth.htm#authentication-scopes).

### 3. Getting an authenticated OneDriveClient object

You must get a **OneDriveClient** object in order for your app to make requests to the service, but first you must have an instance of an object that implements `IAuthenticationProvider` in Microsoft.Graph.Core.
An example of such an imlementation can be found [MSA Auth Adapter repository](https://github.com/OneDrive/onedrive-sdk-dotnet-msa-auth-adapter). You should create the `IAuthenticationProvider`, authenticate
using `AuthenticateUserAsync()`, and then create a `OneDriveClient` using the auth provider as a constructor argument. You must also provide the ClientId of your app, the return URL you have specified for your app,
and the base URL for the API. Below is a sample of that pattern for authentication on the OneDrive service.

```csharp
var msaAuthProvider = new myAuthProvider(
    myClientId,
    "https://login.live.com/oauth20_desktop.srf",
    { "onedrive.readonly", "wl.signin" });
await msaAuthProvider.AuthenticateUserAsync();
var oneDriveClient = new OneDriveClient("https://api.onedrive.com/v1.0", msaAuthProvider);
```

After that, you will be able to use the `oneDriveClient` object to make calls to the service. For more information, see [Authenticate your C# app for OneDrive](docs/auth.md).

### 4. Making requests to the service

Once you have a OneDriveClient that is authenticated you can begin to make calls against the service. The requests against the service look like OneDrive's [REST API](https://dev.onedrive.com/README.htm).

To retrieve a user's drive:

```csharp
    var drive = await oneDriveClient
                          .Drive
                          .Request()
                          .GetAsync();
```

`GetAsync` will return a `Drive` object on success and throw a `Microsoft.Graph.ServiceException` on error.

To get the current user's root folder of their drive:

```csharp
    var rootItem = await oneDriveClient
                             .Drive
                             .Root
                             .Request()
                             .GetAsync();
```

`GetAsync` will return an `Item` object on success and throw a `Microsoft.Graph.ServiceException` on error.

For a general overview of how the SDK is designed, see [overview](docs/overview.md).

The following sample applications are also available:
* [OneDrive API Browser](https://github.com/OneDrive/onedrive-sample-apibrowser-dotnet) - Windows Forms app
* [OneDrive Photo Browser](https://github.com/OneDrive/onedrive-sample-photobrowser-uwp) - Windows Universal app
* [OneDrive Webhooks](https://github.com/OneDrive/onedrive-webhooks-aspnet) - ASP.NET MVC app

To run the OneDrivePhotoBrowser sample app your machine will need to be configured for [UWP app development](https://msdn.microsoft.com/en-us/library/windows/apps/dn609832.aspx) and the project must be associated with the Windows Store.

## Documentation and resources

* [Overview](docs/overview.md)
* [Auth](docs/auth.md)
* [Items](docs/items.md)
* [Chunked uploads](docs/chunked-uploads.md)
* [Collections](docs/collections.md)
* [Errors](docs/errors.md)
* [OneDrive API](http://dev.onedrive.com)

## Issues

To view or log issues, see [issues](https://github.com/OneDrive/onedrive-sdk-csharp/issues).

## Other resources

* NuGet Package: [https://www.nuget.org/packages/Microsoft.OneDriveSDK](https://www.nuget.org/packages/Microsoft.OneDriveSDK)


## License

[License](LICENSE.txt)

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
