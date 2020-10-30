# Chunked Uploads
## Uploading large files and pausing/resuming uploads

Chunked uploads are easy using `Helpers.ChunkedUploadProvider`. The easiest way to upload a large file
looks like this:

```csharp
var session = await oneDriveClient.ItemWithPath(uploadPath).CreateSession().Request().PostAsync();
var provider = new ChunkedUploadProvider(session, oneDriveClient, fileStream);

var uploadedItem = await provider.UploadAsync();
```

Your file will upload in default-sized chunks (5MiB). You can also adjust chunk size by adding a parameter to the `ChunkedUploadProvider`
constructor: `new ChunkedUploadProvider(session, oneDriveClient, fileStream, 1024*1024); // 1 MiB`.

## Controlling and Monitoring Large Uploads

You may want to monitor the progress of an upload (perhaps to show a progress bar to the user). You can get finer control of uploading each chunk using
the skeleton below. You will certainly need to make a few modifications. Also, you can check out how the [ChunkedUploadProvider](../src/OneDriveSdk/Helpers/ChunkedUploadProvider.cs)

```csharp
// Get the provider
var myMaxChunkSize = 5*1024*1024; // 5MB
var session = await oneDriveClient.ItemWithPath(uploadPath).CreateSession().Request().PostAsync();
var provider = new ChunkedUploadProvider(session, oneDriveClient, fileStream, myMaxChunkSize);

// Setup the chunk request necessities
var chunkRequests = provider.GetUploadChunkRequests();
var readBuffer = new byte[myMaxChunkSize];
var trackedExceptions = new List<Exception>();
Item itemResult = null;

//upload the chunks
foreach(var request in chunkRequests)
{
    // Do your updates here: update progress bar, etc.
    // ...
    // Send chunk request
    var result = await provider.GetChunkRequestResponseAsync(request, readBuffer, trackedExceptions);
    
    if(result.UploadSucceeded)
    {
        itemResult = result.ItemResponse;
    }
}

// Check that upload succeeded
if (itemResult == null)
{
    // Retry the upload
    // ...
}
```

