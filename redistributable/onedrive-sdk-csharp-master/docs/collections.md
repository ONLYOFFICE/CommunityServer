Collections in the OneDrive SDK for C#
=====

You can use the OneDrive SDK for C# to work with item collections in OneDrive.

## Getting a collection

To retrieve a collection, like a folder's children, you call `GetAsync`:

```csharp
await oneDriveClient
          .Drive
		  .Items[itemId]
		  .Children
		  .Request()
		  .GetAsync();
```

`GetAsync` returns an `ICollectionPage<T>` implementation on success and throws a `OneDriveException` on error. For children collections, the type returned is `IChildrenCollectionPage`, which inherits `ICollectionPage<Item>`.

`IChildrenCollectionPage` contains three properties: 

|Name                |Description                                                                                                                                  |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
|**CurrentPage**     |An `IList<Item>`.                                                                                                                            |
|**NextPageRequest** |An `IChildrenPageRequest` used to get to the next page of items, if another page exists. This value will be null if there is not a next page.|
|**AdditionData**    |An `IDictionary<string, object>` to any additional values returned by the service. In this case, none.                                       |

## Adding to a collection

Some collections, like the children of a folder, can be changed. To add a folder to the children of an item, you can call the `AddAsync` method:

```csharp
var folderToCreate = new Item { Name = "New folder", Folder = new Folder() };
var newFolder = await oneDriveClient
                          .Drive
						  .Items[itemId]
						  .Children
						  .Request()
						  .AddAsync(folderToCreate);
```

`AddAsync` returns the created item on success and throws a `OneDriveException` on error.

## Expanding a collection

To expand a collection, you call `Expand` on the collection request object with the string value of the expand:

```csharp
var children = await oneDriveClient
                         .Drive
						 .Items[itemId]
						 .Children
						 .Request()
						 .Expand("thumbnails")
						 .GetAsync();
```

## Special collections

Some API calls will return collections with added properties. These properties will always be in the additional data dictionary. These collections are also their own objects (subclasses of `ICollectionPage<T>`) that will have these properties attached to them.  

To get the delta of an item you call:

```csharp
var deltaCollectionPage = await oneDriveClient
                                    .Drive
									.Items[itemId]
									.Delta(deltaToken)
									.Request()
									.GetAsync();
```

`IItemDeltaCollectionPage` is an `ICollectionPage<Item>` object with a `Token` property and a `DeltaLink` property. The token link can be used to pass into `Delta:` when you want to check for more changes. You can also construct a delta request with the `DeltaLink` property. The `NextPageRequest` is an `IItemDeltaRequest` to be used for paging purposes and will be null when there are no more changes.