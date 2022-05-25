module QueueingServiceExamples

open System
open System.Threading
open System.Threading.Tasks
open net.openstack.Core
open net.openstack.Core.Collections
open net.openstack.Core.Domain
open net.openstack.Core.Domain.Queues
open net.openstack.Core.Providers
open net.openstack.Providers.Rackspace

let identity = new CloudIdentity (Username = "MyUser", APIKey = "API_KEY_HERE")
let region : string = null
let clientId = Guid.NewGuid()
let internalUrl = false
let identityProvider : IIdentityProvider = null

let getHomeAsyncAwait =
    async {
        //#region GetHomeAsync (await)
        let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        let! homeDocument = queueingService.GetHomeAsync(CancellationToken.None) |> Async.AwaitTask
        //#endregion
        ()
    }

let getHome =
    //#region GetHomeAsync (TPL)
    let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
    let task = queueingService.GetHomeAsync(CancellationToken.None)
    //#endregion
    ()

let getNodeHealthAsyncAwait =
    async {
        //#region GetNodeHealthAsync (await)
        let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        queueingService.GetNodeHealthAsync(CancellationToken.None) |> Async.AwaitIAsyncResult |> ignore
        //#endregion
        ()
    }

let getNodeHealth =
    //#region GetNodeHealthAsync (TPL)
    let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
    let task = queueingService.GetNodeHealthAsync(CancellationToken.None)
    //#endregion
    ()

let createQueueAsyncAwait =
    async {
        //#region CreateQueueAsync (await)
        let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        let queueName = new QueueName("ExampleQueue")
        let! createdQueue = queueingService.CreateQueueAsync(queueName, CancellationToken.None) |> Async.AwaitTask
        //#endregion
        ()
    }

let createQueue =
    //#region CreateQueueAsync (TPL)
    let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
    let queueName = new QueueName("ExampleQueue")
    let task = queueingService.CreateQueueAsync(queueName, CancellationToken.None)
    //#endregion
    ()

let deleteQueueAsyncAwait =
    async {
        //#region DeleteQueueAsync (await)
        let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        let queueName = new QueueName("ExampleQueue")
        queueingService.DeleteQueueAsync(queueName, CancellationToken.None) |> Async.AwaitIAsyncResult |> ignore
        //#endregion
        ()
    }

let deleteQueue =
    //#region DeleteQueueAsync (TPL)
    let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
    let queueName = new QueueName("ExampleQueue")
    let task = queueingService.DeleteQueueAsync(queueName, CancellationToken.None)
    //#endregion
    ()

let listQueuesAsyncAwait =
    async {
        //#region ListQueuesAsync (await)
        let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        let! queuesPage = queueingService.ListQueuesAsync(null, Nullable(), true, CancellationToken.None) |> Async.AwaitTask
        let! queues = queuesPage.GetAllPagesAsync(CancellationToken.None, null) |> Async.AwaitTask
        //#endregion
        ()
    }

let listQueues =
    //#region ListQueuesAsync (TPL)
    let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
    let queuesPageTask = queueingService.ListQueuesAsync(null, Nullable(), true, CancellationToken.None)
    let queuesTask = queuesPageTask.ContinueWith(fun (task:Task<ReadOnlyCollectionPage<CloudQueue>>) -> task.Result.GetAllPagesAsync(CancellationToken.None, null)) |> TaskExtensions.Unwrap
    //#endregion
    ()

let queueExistsAsyncAwait =
    async {
        //#region QueueExistsAsync (await)
        let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        let queueName = new QueueName("ExampleQueue")
        let! exists = queueingService.QueueExistsAsync(queueName, CancellationToken.None) |> Async.AwaitTask
        //#endregion
        ()
    }

let queueExists =
    //#region QueueExistsAsync (TPL)
    let queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
    let queueName = new QueueName("ExampleQueue")
    let task = queueingService.QueueExistsAsync(queueName, CancellationToken.None)
    //#endregion
    ()
