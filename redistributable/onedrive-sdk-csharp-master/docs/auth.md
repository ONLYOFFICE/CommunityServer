Authenticate your C# app for OneDrive
=====

To authenticate your app to use OneDrive, you need to instantiate an object that implements `IAuthenticationProvider` from Microsoft.Graph and call `AuthenticateAsync` on it. Then, you must create a `OneDriveClient` object and pass in your auth provider as an argument. Note that if the user changes their password, your app must re-authenticate.  If you see `401` error codes, this is most likely the case. See [Error codes for the OneDrive C# SDK](errors.md) for more info.

**Note** This topic assumes that you are familiar with app authentication. For more info about authentication in OneDrive, see [Authentication for the OneDrive API](https://dev.onedrive.com/auth/readme.htm).

## Standard authentication components

When implementing `IAuthenticationProvider`, a standard set of parameters will  be required:

| Parameter | Description |
|:----------|:------------|
| _clientId_ | The client ID of the app. Required. |
| _returnUrl_ | A redirect URL. Required. |
| _baseUrl_ | URL where the target OneDrive service is found. Required. |
| _scopes_ | Permissions that your app requires from the user. Required. |
| _client\_secret_ | The client secret created for your app. Optional. Not available for Windows Store 8.1, Windows Phone 8.1, and Universal Windows Platform (UWP) apps. |

In addition to _clientId_, _returnURL_, _scopes_, and _client\_secret_ the method takes in implementations for a client type, credential cache, HTTP provider, and a service info provider or web authentication UI. If not provided, the default implementations of each item will be used.

### ClientType
A single client can only call OneDrive for Consumer or OneDrive for Business, not both. The target service is configured implicitly by the `IAuthenticationProvider` and the _baseUrl_ passed into the `OneDriveClient` constructor.

If the application would like to interact with both OneDrive for Consumer and OneDrive for Business, a client should be created for each.


## More Information
More information, and a fuller example of authentication, can be found at the [MSA Auth Adapter repository](https://github.com/OneDrive/onedrive-sdk-dotnet-msa-auth-adapter).