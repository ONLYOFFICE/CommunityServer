Items in the OneDrive SDK for C#
=====

Items in the OneDrive SDK for C# behave just like items through the OneDrive API. All actions on items described in the OneDrive API are available through the SDK. For more information, see the [Items Reference](https://dev.onedrive.com/README.htm#item-resource). 

The examples below assume that you have [Authenticated](/docs/auth.md) your app with a **OneDriveClient** object.

* [Get an Item](#get-an-item)
* [Delete an Item](#delete-an-item)
* [Get Children for an Item](#get-children-for-an-item)
* [Create a folder](#create-a-folder)
* [Uploading contents](#uploading-contents)
* [Downloading contents](#downloading-contents)
* [Moving and updating an Item](#moving-and-updating-an-item)
* [Copy an Item](#copy-an-item)

Get an Item
---------------
### 1. By ID

```csharp
var item = await oneDriveClient
                     .Items[itemId]
                     .Request()
                     .GetAsync();
```

### 2. By path

```csharp
var item = await oneDriveClient
                     .Drive
                     .Root
                     .ItemWithPath("path/to/file/txt")
                     .Request()
                     .GetAsync();
```

Access an item by parent reference path:
```csharp
var item = await oneDriveClient
                     .ItemWithPath(parentItem.ParentReference.Path + "/" + parentItem.Name + "/relative/path")
                     .Request()
                     .GetAsync();
```

Delete an Item
---------------
```csharp
await oneDriveClient
          .Drive
          .Items[itemId]
          .Request()
          .DeleteAsync();
```

Get children for an Item
-------------------------

More info about collections [here](/docs/collections.md).

```csharp
await oneDriveClient
          .Drive
          .Items[itemId]
          .Children
          .Request()
          .GetAsync();
```

Create a folder
-------------------------

### 1. By POST to a known folder ID

```csharp
var folderToCreate = new Item { Folder = new Folder(), Name = "folder name" };
var createdFolder = await oneDriveClient
          .Drive
          .Items[itemId]
          .Children
          .Request()
          .AddAsync(folderToCreate);
```

### 2. By PUT to a known folder ID

```csharp
var folderToCreate = new Item { Folder = new Folder() };
var createdFolder = await oneDriveClient
          .Drive
          .Items[itemId]
          .ItemWithPath("folder name")
          .Request()
          .CreateAsync(folderToCreate);
```

### 3. By PUT with a given path

```csharp
var folderToCreate = new Item { Folder = new Folder() };
var createdFolder = await oneDriveClient
          .Drive
          .Root
          .ItemWithPath("folder/subfolder")
          .Request()
          .CreateAsync(folderToCreate);
```

Note: If the specified path does not exist, every folder name in the path will also be created.

Uploading contents
------------------------------

```csharp
using (contentStream)
{
    var uploadedItem = await oneDriveClient
                                 .Drive
                                 .Root
                                 .ItemWithPath("path/to/file.txt")
                                 .Content
                                 .Request()
                                 .PutAsync<Item>(contentStream);
}
```

Downloading contents
------------------------------

```csharp
var contentStream = await oneDriveClient
                              .Drive
                              .Items[itemId]
                              .Content
                              .Request()
                              .GetAsync();
```

Moving and updating an Item
--------------
To [move](https://dev.onedrive.com/items/move.htm) an item you must update its parent reference.

```csharp
var updateItem = new Item { ParentReference = new ItemReference { Id = newParentId } };
var itemWithUpdates = await oneDriveClient
                                .Drive
                                .Items[itemId]
                                .Request()
                                .UpdateAsync(updateItem);
```

To change an item's name you could:

```csharp
var updateItem = new Item { Name = "New name!" };
var itemWithUpdates = await oneDriveClient
                                .Drive
                                .Items[itemId]
                                .Request()
                                .UpdateAsync(updateItem);

```

Copy an Item
---------------
Copying an item is an async action described [here](https://dev.onedrive.com/items/copy.htm).

```csharp
var asyncStatus = await oneDriveClient
                            .Drive
                            .Items[itemId]
                            .Copy(newItemName, new ItemReference { Id = copyLocationId })
                            .Request()
                            .PostAsync();  
```

The `Copy` action returns an `IItemCopyAsyncMonitor` instance that has a method to poll the monitor URL for completion. The poll method returns the created item on completion.

To poll until the copy action completes:

```csharp
var newItem = await asyncStatus.CompleteOperationAsync(null, CancellationToken.None);
```

`CompleteOperationAsync` takes in an `IProgress<AsyncOperationStatus>` for reporting back progress status and a `CancellationToken` for action cancellation. The method will poll until completion unless cancelled.
