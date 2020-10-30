Handling errors in the OneDrive SDK for C#
=====

Errors in the OneDrive SDK for C# behave just like errors returned from the OneDrive service. You can read more about them [here](https://github.com/OneDrive/onedrive-api-docs/blob/master/misc/errors.md).

Anytime you make a request against the service there is the potential for an error. You will see that all requests to the service can return an error. In the case of an error, the request will throw a `OneDriveException` object with an inner `Error` object that contains the service error details.

## Checking the error

There are a few different types of errors that can occur during a network call. These error codes are defined in [OneDriveErrorCodes.cs](../src/OneDriveSdk/Enums/OneDriveErrorCodes.cs).

### Checking the error code
You can easily check if an error has a specific code by calling `IsMatch` on the error code value. `IsMatch` is not case sensitive:
```csharp
if (exception.IsMatch(OneDriveErrorCode.AccessDenied.ToString())
{
        // Handle access denied error
}
```

Each error object has a `Message` property as well as code. This message is for debugging purposes and is not be meant to be displayed to the user. Common error codes are defined in [OneDriveErrorCodes.cs](../src/OneDriveSdk/Enums/OneDriveErrorCodes.cs).


### Authentication errors

There can be errors during the authentication process. Authentication errors will have the code `AuthenticationFailed`. Authentication cancelled errors will have the code `AuthenticationCancelled`.

```csharp
if (exception.IsMatch(OneDriveErrorCode.AuthenticationFailure.ToString())
{
        // Handle auth error
}
```

The `Message` property will contain more detailed error information if available.