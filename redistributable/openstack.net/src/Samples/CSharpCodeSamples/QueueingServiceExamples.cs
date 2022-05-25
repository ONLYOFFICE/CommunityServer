namespace CSharpCodeSamples
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using net.openstack.Core;
    using net.openstack.Core.Collections;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Domain.Queues;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace;

    public class QueueingServiceExamples
    {
        CloudIdentity identity =
            new CloudIdentity()
            {
                Username = "MyUser",
                APIKey = "API_KEY_HERE"
            };
        // use the default region for the account
        string region = null;
        // create a new client ID for this instance
        Guid clientId = Guid.NewGuid();
        // access Cloud Queues over the public Internet
        bool internalUrl = false;
        // use a default CloudIdentityProvider for authentication
        IIdentityProvider identityProvider = null;

        public async Task GetHomeAsyncAwait()
        {
            #region GetHomeAsync (await)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            HomeDocument createdQueue = await queueingService.GetHomeAsync(CancellationToken.None);
            #endregion
        }

        public void GetHome()
        {
            #region GetHomeAsync (TPL)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            Task<HomeDocument> task = queueingService.GetHomeAsync(CancellationToken.None);
            #endregion
        }

        public async Task GetNodeHealthAsyncAwait()
        {
            #region GetNodeHealthAsync (await)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            await queueingService.GetNodeHealthAsync(CancellationToken.None);
            #endregion
        }

        public void GetNodeHealth()
        {
            #region GetNodeHealthAsync (TPL)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            Task task = queueingService.GetNodeHealthAsync(CancellationToken.None);
            #endregion
        }

        public async Task CreateQueueAsyncAwait()
        {
            #region CreateQueueAsync (await)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            QueueName queueName = new QueueName("ExampleQueue");
            bool createdQueue = await queueingService.CreateQueueAsync(queueName, CancellationToken.None);
            #endregion
        }

        public void CreateQueue()
        {
            #region CreateQueueAsync (TPL)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            QueueName queueName = new QueueName("ExampleQueue");
            Task<bool> task = queueingService.CreateQueueAsync(queueName, CancellationToken.None);
            #endregion
        }

        public async Task DeleteQueueAsyncAwait()
        {
            #region DeleteQueueAsync (await)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            QueueName queueName = new QueueName("ExampleQueue");
            await queueingService.DeleteQueueAsync(queueName, CancellationToken.None);
            #endregion
        }

        public void DeleteQueue()
        {
            #region DeleteQueueAsync (TPL)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            QueueName queueName = new QueueName("ExampleQueue");
            Task task = queueingService.DeleteQueueAsync(queueName, CancellationToken.None);
            #endregion
        }

        public async Task ListQueuesAsyncAwait()
        {
            #region ListQueuesAsync (await)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            ReadOnlyCollectionPage<CloudQueue> queuesPage = await queueingService.ListQueuesAsync(null, null, true, CancellationToken.None);
            ReadOnlyCollection<CloudQueue> queues = await queuesPage.GetAllPagesAsync(CancellationToken.None, null);
            #endregion
        }

        public void ListQueues()
        {
            #region ListQueuesAsync (TPL)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            Task<ReadOnlyCollectionPage<CloudQueue>> queuesPageTask = queueingService.ListQueuesAsync(null, null, true, CancellationToken.None);
            Task<ReadOnlyCollection<CloudQueue>> queuesTask =
                queuesPageTask
                .ContinueWith(task => task.Result.GetAllPagesAsync(CancellationToken.None, null))
                .Unwrap();
            #endregion
        }

        public async Task QueueExistsAsyncAwait()
        {
            #region QueueExistsAsync (await)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            QueueName queueName = new QueueName("ExampleQueue");
            bool exists = await queueingService.QueueExistsAsync(queueName, CancellationToken.None);
            #endregion
        }

        public void QueueExists()
        {
            #region QueueExistsAsync (TPL)
            IQueueingService queueingService = new CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
            QueueName queueName = new QueueName("ExampleQueue");
            Task<bool> task = queueingService.QueueExistsAsync(queueName, CancellationToken.None);
            #endregion
        }
    }
}
