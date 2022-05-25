Imports net.openstack.Core
Imports net.openstack.Core.Domain
Imports net.openstack.Core.Providers
Imports net.openstack.Core.Domain.Queues
Imports net.openstack.Providers.Rackspace
Imports System.Threading
Imports System.Threading.Tasks

Public Class QueueingServiceExamples

    Dim identity = New CloudIdentity With
                   {
                       .Username = "MyUser",
                       .APIKey = "API_KEY_HERE"
                   }
    Dim region As String = Nothing
    Dim clientId = Guid.NewGuid
    Dim internalUrl = False
    Dim identityProvider As IIdentityProvider = Nothing

    Public Async Function GetHomeAsyncAwait() As Task
        ' #Region "GetHomeAsync (await)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim homeDocument = Await queueingService.GetHomeAsync(CancellationToken.None)
        ' #End Region
    End Function

    Public Sub GetHome()
        ' #Region "GetHomeAsync (TPL)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim task = queueingService.GetHomeAsync(CancellationToken.None)
        ' #End Region
    End Sub

    Public Async Function GetNodeHealthAsyncAwait() As Task
        ' #Region "GetNodeHealthAsync (await)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Await queueingService.GetNodeHealthAsync(CancellationToken.None)
        ' #End Region
    End Function

    Public Sub GetNodeHealth()
        ' #Region "GetNodeHealthAsync (TPL)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim task = queueingService.GetNodeHealthAsync(CancellationToken.None)
        ' #End Region
    End Sub

    Public Async Function CreateQueueAsyncAwait() As Task
        ' #Region "CreateQueueAsync (await)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim queueName = New QueueName("ExampleQueue")
        Dim createdQueue = Await queueingService.CreateQueueAsync(queueName, CancellationToken.None)
        ' #End Region
    End Function

    Public Sub CreateQueue()
        ' #Region "CreateQueueAsync (TPL)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim queueName = New QueueName("ExampleQueue")
        Dim task = queueingService.CreateQueueAsync(queueName, CancellationToken.None)
        ' #End Region
    End Sub

    Public Async Function DeleteQueueAsyncAwait() As Task
        ' #Region "DeleteQueueAsync (await)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim queueName = New QueueName("ExampleQueue")
        Await queueingService.DeleteQueueAsync(queueName, CancellationToken.None)
        ' #End Region
    End Function

    Public Sub DeleteQueue()
        ' #Region "DeleteQueueAsync (TPL)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim queueName = New QueueName("ExampleQueue")
        Dim task = queueingService.DeleteQueueAsync(queueName, CancellationToken.None)
        ' #End Region
    End Sub

    Public Async Function ListQueuesAsyncAwait() As Task
        ' #Region "ListQueuesAsync (await)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim queuesPage = Await queueingService.ListQueuesAsync(Nothing, Nothing, True, CancellationToken.None)
        Dim queues = Await queuesPage.GetAllPagesAsync(CancellationToken.None, Nothing)
        ' #End Region
    End Function

    Public Sub ListQueues()
        ' #Region "ListQueuesAsync (TPL)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim queuesPageTask = queueingService.ListQueuesAsync(Nothing, Nothing, True, CancellationToken.None)
        Dim queuesTask = queuesPageTask _
            .ContinueWith(Function(task) task.Result.GetAllPagesAsync(CancellationToken.None, Nothing)) _
            .Unwrap()
        ' #End Region
    End Sub

    Public Async Function QueueExistsAsyncAwait() As Task
        ' #Region "QueueExistsAsync (await)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim queueName = New QueueName("ExampleQueue")
        Dim exists = Await queueingService.QueueExistsAsync(queueName, CancellationToken.None)
        ' #End Region
    End Function

    Public Sub QueueExists()
        ' #Region "QueueExistsAsync (TPL)"
        Dim queueingService As IQueueingService = New CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider)
        Dim queueName = New QueueName("ExampleQueue")
        Dim task = queueingService.QueueExistsAsync(queueName, CancellationToken.None)
        ' #End Region
    End Sub

End Class
