OneDrive SDK for CSharp overview
=====

The OneDrive C# SDK is designed to look just like the [OneDrive API](https://github.com/onedrive/onedrive-api-docs/).  

## OneDriveClient

When accessing the OneDrive APIs, all requests will be made through a **OneDriveClient** object. For a more detailed explanation, see [Authentication](/docs/auth.md).

## Resource model


Resources, like [items](/docs/items.md) or drives, are represented by `Item` and `Drive`. These objects contain properties that represent the properties of a resource. These objects are property bags and cannot make calls against the service.

To get the name of an item you would address the `Name` property. It is possible for any of these properties to be null at any time. To check if an item is a folder you can address the `Folder` property of the item. If the item is a folder, a `Folder` object that contains all of the properties described by the [folder](https://github.com/OneDrive/onedrive-api-docs/blob/master/facets/folder_facet.md) facet will be returned.

See [Resource model](https://github.com/onedrive/onedrive-api-docs/#resource-model) for more information.

## Requests

To make requests against the service, you construct request objects using a request builder object. The type of the request builder will depend on the type of the object you are addressing. This is meant to mimic creating the URL for any of the OneDrive APIs.

### 1. Request builders

To generate a request you chain together calls on request builder objects. You get the first request builder from the `OneDriveClient` object. To get a drive request builder you call:

|Task            | SDK                   | URL                             |
|:---------------|:---------------------:|:--------------------------------|
|Get a drive     | oneDriveClient.Drive  | GET api.onedrive.com/v1.0/drive/|
 
The call will return an `IDriveRequestBuilder` object. From drive you can continue to chain the requests to get everything else in the API, like an item.

|Task            | SDK                                  | URL                                       |
|:---------------|:------------------------------------:|:------------------------------------------|
|Get an item     | oneDriveClient.Drive.Items["1234"]   | GET api.onedrive.com/v1.0/drive/items/1234|


Here `oneDriveClient.Drive` returns an `IDriveRequestBuilder` that contains a property `Items` of type `IItemsCollectionRequestBuilder`. That builder has an accessor for the item ID and Items["1234"] returns an `IItemRequestBuilder`.

Similarly to get thumbnails:

|Task            | SDK                            | URL                      |
|----------------|--------------------------------|--------------------------|
| Get thumbnails | ... Items["1234"].Thumbnails   | .../items/1234/thumbnails|


Here, `oneDriveClient.Drive.Items["1234"]` returns an `IItemRequestBuilder` that contains the property Thumbnails of type `IThumbnailsCollectionRequestBuilder`.

This returns a collection of [thumbnail sets](https://github.com/OneDrive/onedrive-api-docs/blob/master/resources/thumbnailSet.md). To index the collection directly you can call:

|Task               | SDK                                 | URL                        |
|-------------------|-------------------------------------|----------------------------|
| Get thumbnail Set | ... Items["1234"].Thumbnails["0"]   | ...items/1234/thumbnails/0 |

To return a thumbnail set, and to get a specific [thumbnail](https://github.com/OneDrive/onedrive-api-docs/blob/master/resources/thumbnail.md), you can add the name of the thumbnail to the URL like this:

|Task             | SDK                         | URL                    |
|-----------------|-----------------------------|------------------------|
| Get a thumbnail | ... Thumbnails["0"].Small   | .../thumbnails/0/small |


### 2. Request calls

After you build the request you call the `Request` method on the request builder. This will construct the request object needed to make calls against the service.

For an item you call:

```csharp
var itemRequest = oneDriveClient
                      .Drive
					  .Items[itemId]
					  .Request();
```

All request builders have a `Request` method that can generate a request object. Request objects may have different methods on them depending on the type of request. To get an item you call:

```csharp
var item = await oneDriveClient
                     .Drive
					 .Items[itemId]
					 .Request()
					 .GetAsync();
```

For more info, see [items](/docs/items.md) and [errors](/docs/errors.md).

## Query options

If you only want to retrieve certain properties of a resource you can select them. Here's how to get only the names and IDs of an item:

```csharp
var item = await oneDriveClient
                     .Drive
					 .Items[itemId]
					 .Request()
					 .Select("name,id")
					 .GetAsync();
```

All properties other than `Name` and `Id` will be null on the item.

To expand certain properties on resources you can call a similar expand method, like this:

```csharp
var item = await oneDriveClient
                     .Drive
					 .Items[itemId]
					 .Request()
					 .Expand("thumbnails,children(expand=thumbnails)")
					 .GetAsync();
```

The above call will expand thumbnails and children for the item, as well as thumbnails for all of the children.
