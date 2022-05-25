namespace net.openstack.Core.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using net.openstack.Core.Collections;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Domain.Queues;
    using Newtonsoft.Json.Linq;
    using CancellationToken = System.Threading.CancellationToken;
    using CloudQueuesProvider = net.openstack.Providers.Rackspace.CloudQueuesProvider;
    using JsonSerializationException = Newtonsoft.Json.JsonSerializationException;
    using WebException = System.Net.WebException;

    /// <summary>
    /// Represents a provider for asynchronous operations on the OpenStack Marconi (Cloud Queues) Service.
    /// </summary>
    /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1&amp;oldid=30943">OpenStack Marconi API v1 Blueprint</seealso>
    /// <preliminary/>
    public interface IQueueingService
    {
        #region Base endpoints

        /// <summary>
        /// Gets the home document describing the operations supported by the service.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="HomeDocument"/> object describing the operations supported by the service.</returns>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudQueuesProvider"/>
        /// implementation of the <see cref="IQueueingService"/>. For more information about creating the provider, see
        /// <see cref="CloudQueuesProvider.CloudQueuesProvider(CloudIdentity, string, Guid, bool, IIdentityProvider)"/>.</para>
        /// <token>AsyncAwaitExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="GetHomeAsync (await)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="GetHomeAsync (await)" language="vbnet"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="GetHomeAsync (await)" language="fs"/>
        /// <token>TplExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="GetHomeAsync (TPL)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="GetHomeAsync (TPL)" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\QueueingServiceExamples.cpp" region="GetHomeAsync (TPL)" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="GetHomeAsync (TPL)" language="fs"/>
        /// </example>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Get_Home_Document">Get Home Document (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<HomeDocument> GetHomeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Checks the queueing service node status.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. If the service
        /// is available, the task will complete successfully. If the service is unavailable due
        /// to a storage driver failure or some other error, the task will fail and the
        /// <see cref="Task.Exception"/> property will contain the reason for the failure.
        /// </returns>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudQueuesProvider"/>
        /// implementation of the <see cref="IQueueingService"/>. For more information about creating the provider, see
        /// <see cref="CloudQueuesProvider.CloudQueuesProvider(CloudIdentity, string, Guid, bool, IIdentityProvider)"/>.</para>
        /// <token>AsyncAwaitExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="GetNodeHealthAsync (await)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="GetNodeHealthAsync (await)" language="vbnet"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="GetNodeHealthAsync (await)" language="fs"/>
        /// <token>TplExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="GetNodeHealthAsync (TPL)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="GetNodeHealthAsync (TPL)" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\QueueingServiceExamples.cpp" region="GetNodeHealthAsync (TPL)" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="GetNodeHealthAsync (TPL)" language="fs"/>
        /// </example>
        /// <seealso href="https://wiki.openstack.org/wiki/Marconi/specs/api/v1#Check_Node_Health">Check Node Health (OpenStack Marconi API v1 Blueprint)</seealso>
        Task GetNodeHealthAsync(CancellationToken cancellationToken);

        #endregion Base endpoints

        #region Queues

        /// <summary>
        /// Creates a queue, if it does not already exist.
        /// </summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain <see langword="true"/> if the queue was created by the call, or <see langword="false"/> if the queue already existed.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudQueuesProvider"/>
        /// implementation of the <see cref="IQueueingService"/>. For more information about creating the provider, see
        /// <see cref="CloudQueuesProvider.CloudQueuesProvider(CloudIdentity, string, Guid, bool, IIdentityProvider)"/>.</para>
        /// <token>AsyncAwaitExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="CreateQueueAsync (await)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="CreateQueueAsync (await)" language="vbnet"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="CreateQueueAsync (await)" language="fs"/>
        /// <token>TplExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="CreateQueueAsync (TPL)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="CreateQueueAsync (TPL)" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\QueueingServiceExamples.cpp" region="CreateQueueAsync (TPL)" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="CreateQueueAsync (TPL)" language="fs"/>
        /// </example>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Create_Queue">Create Queue (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<bool> CreateQueueAsync(QueueName queueName, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a list of queues.
        /// </summary>
        /// <param name="marker">The name of the last queue in the previous list. The resulting collection of queues will start with the first queue <em>after</em> this value, when sorted using <see cref="StringComparer.Ordinal"/>. If this value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of queues to return. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="detailed"><see langword="true"/> to return detailed information about each queue; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain <placeholder>placeholder</placeholder>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudQueuesProvider"/>
        /// implementation of the <see cref="IQueueingService"/>. For more information about creating the provider, see
        /// <see cref="CloudQueuesProvider.CloudQueuesProvider(CloudIdentity, string, Guid, bool, IIdentityProvider)"/>.</para>
        /// <token>AsyncAwaitExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="ListQueuesAsync (await)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="ListQueuesAsync (await)" language="vbnet"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="ListQueuesAsync (await)" language="fs"/>
        /// <token>TplExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="ListQueuesAsync (TPL)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="ListQueuesAsync (TPL)" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\QueueingServiceExamples.cpp" region="ListQueuesAsync (TPL)" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="ListQueuesAsync (TPL)" language="fs"/>
        /// </example>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#List_Queues">List Queues (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<ReadOnlyCollectionPage<CloudQueue>> ListQueuesAsync(QueueName marker, int? limit, bool detailed, CancellationToken cancellationToken);

        /// <summary>
        /// Checks for the existence of a queue with a particular name.
        /// </summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain <see langword="true"/> if queue with the specified name exists; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudQueuesProvider"/>
        /// implementation of the <see cref="IQueueingService"/>. For more information about creating the provider, see
        /// <see cref="CloudQueuesProvider.CloudQueuesProvider(CloudIdentity, string, Guid, bool, IIdentityProvider)"/>.</para>
        /// <token>AsyncAwaitExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="QueueExistsAsync (await)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="QueueExistsAsync (await)" language="vbnet"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="QueueExistsAsync (await)" language="fs"/>
        /// <token>TplExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="QueueExistsAsync (TPL)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="QueueExistsAsync (TPL)" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\QueueingServiceExamples.cpp" region="QueueExistsAsync (TPL)" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="QueueExistsAsync (TPL)" language="fs"/>
        /// </example>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Checking_Queue_Existence">Checking Queue Existence (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<bool> QueueExistsAsync(QueueName queueName, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a queue.
        /// </summary>
        /// <remarks>
        /// The queue will be deleted whether or not it is empty, even if one or more messages in the queue is currently claimed.
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <example>
        /// <para>The following example demonstrates the use of this method using the <see cref="CloudQueuesProvider"/>
        /// implementation of the <see cref="IQueueingService"/>. For more information about creating the provider, see
        /// <see cref="CloudQueuesProvider.CloudQueuesProvider(CloudIdentity, string, Guid, bool, IIdentityProvider)"/>.</para>
        /// <token>AsyncAwaitExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="DeleteQueueAsync (await)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="DeleteQueueAsync (await)" language="vbnet"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="DeleteQueueAsync (await)" language="fs"/>
        /// <token>TplExample</token>
        /// <code source="..\Samples\CSharpCodeSamples\QueueingServiceExamples.cs" region="DeleteQueueAsync (TPL)" language="cs"/>
        /// <code source="..\Samples\VBCodeSamples\QueueingServiceExamples.vb" region="DeleteQueueAsync (TPL)" language="vbnet"/>
        /// <code source="..\Samples\CPPCodeSamples\QueueingServiceExamples.cpp" region="DeleteQueueAsync (TPL)" language="cpp"/>
        /// <code source="..\Samples\FSharpCodeSamples\QueueingServiceExamples.fs" region="DeleteQueueAsync (TPL)" language="fs"/>
        /// </example>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Delete_Queue">Delete Queue (OpenStack Marconi API v1 Blueprint)</seealso>
        Task DeleteQueueAsync(QueueName queueName, CancellationToken cancellationToken);

        #endregion

        #region Queue metadata

        /// <summary>
        /// Sets the metadata associated with a queue.
        /// </summary>
        /// <typeparam name="T">The type of data to associate with the queue.</typeparam>
        /// <param name="queueName">The queue name.</param>
        /// <param name="metadata">The metadata to associate with the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Set_Queue_Metadata">Set Queue Metadata (OpenStack Marconi API v1 Blueprint)</seealso>
        Task SetQueueMetadataAsync<T>(QueueName queueName, T metadata, CancellationToken cancellationToken)
            where T : class;

        /// <summary>
        /// Gets the metadata associated with a queue.
        /// </summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="JObject"/> object containing the metadata associated with the queue.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Get_Queue_Metadata">Get Queue Metadata (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<JObject> GetQueueMetadataAsync(QueueName queueName, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the metadata associated with a queue, as a strongly-typed object.
        /// </summary>
        /// <typeparam name="T">The type of metadata associated with the queue.</typeparam>
        /// <param name="queueName">The queue name.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a deserialized object of type <typeparamref name="T"/> representing the metadata associated with the queue.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="JsonSerializationException">If an error occurs while deserializing the metadata.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Get_Queue_Metadata">Get Queue Metadata (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<T> GetQueueMetadataAsync<T>(QueueName queueName, CancellationToken cancellationToken)
            where T : class;

        /// <summary>
        /// Gets statistics for a queue.
        /// </summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="QueueStatistics"/> object containing statistics for the queue.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Get_Queue_Stats">Get Queue Stats (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<QueueStatistics> GetQueueStatisticsAsync(QueueName queueName, CancellationToken cancellationToken);

        #endregion Queue metadata

        #region Messages

        /// <summary>
        /// Gets a list of messages currently in a queue.
        /// </summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="marker">The identifier of the message list page to return. This is obtained from <see cref="QueuedMessageList.NextPageId"/>. If this value is <see langword="null"/>, the list starts at the beginning.</param>
        /// <param name="limit">The maximum number of messages to return. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="echo"><see langword="true"/> to include messages created by the current client; otherwise, <see langword="false"/>.</param>
        /// <param name="includeClaimed"><see langword="true"/> to include claimed messages; otherwise <see langword="false"/> to return only unclaimed messages.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a collection of <see cref="QueuedMessage"/> objects describing the messages in the queue.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="limit"/> is less than or equal to 0.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#List_Messages">List Messages (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<QueuedMessageList> ListMessagesAsync(QueueName queueName, QueuedMessageListId marker, int? limit, bool echo, bool includeClaimed, CancellationToken cancellationToken);

        /// <summary>
        /// Gets detailed information about a specific queued message.
        /// </summary>
        /// <remarks>
        /// This method will return information for the specified message regardless of the
        /// <literal>Client-ID</literal> or claim associated with the message.
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="messageId">The message ID. This is obtained from <see cref="QueuedMessage.Id">QueuedMessage.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="QueuedMessage"/> object containing detailed information about the specified message.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="messageId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Get_a_Specific_Message">Get a Specific Message (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<QueuedMessage> GetMessageAsync(QueueName queueName, MessageId messageId, CancellationToken cancellationToken);

        /// <summary>
        /// Get messages from a queue.
        /// </summary>
        /// <remarks>
        /// This method will return information for the specified message regardless of the
        /// <literal>Client-ID</literal> or claim associated with the message.
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="messageIds">The message IDs of messages to get.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a collection of <see cref="QueuedMessage"/> objects containing detailed information about the specified messages.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="messageIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="messageIds"/> contains a <see langword="null"/> value.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Get_a_Set_of_Messages_by_ID">Get a Set of Messages by ID (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<ReadOnlyCollection<QueuedMessage>> GetMessagesAsync(QueueName queueName, IEnumerable<MessageId> messageIds, CancellationToken cancellationToken);

        /// <summary>
        /// Posts messages to a queue.
        /// </summary>
        /// <remarks>
        /// If <paramref name="messages"/> is empty, this call is equivalent to <see cref="QueueExistsAsync"/>,
        /// and the <see cref="Task{TResult}.Result"/> property of the returned task contains
        /// <see cref="MessagesEnqueued.Empty"/>.
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="messages">The messages to post.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the task completes
        /// successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="MessagesEnqueued"/> object representing the result of the operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="messages"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="messages"/> contains a <see langword="null"/> value.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Post_Message.28s.29">Post Message(s) (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<MessagesEnqueued> PostMessagesAsync(QueueName queueName, IEnumerable<Message> messages, CancellationToken cancellationToken);

        /// <summary>
        /// Posts messages to a queue.
        /// </summary>
        /// <remarks>
        /// If <paramref name="messages"/> is empty, this call is equivalent to <see cref="QueueExistsAsync"/>,
        /// and upon success the <see cref="Task{TResult}.Result"/> property of the returned task contains
        /// <see cref="MessagesEnqueued.Empty"/>.
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="messages">The messages to post.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the task completes
        /// successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="MessagesEnqueued"/> object representing the result of the operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="messages"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="messages"/> contains a <see langword="null"/> value.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Post_Message.28s.29">Post Message(s) (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<MessagesEnqueued> PostMessagesAsync(QueueName queueName, CancellationToken cancellationToken, params Message[] messages);

        /// <summary>
        /// Posts messages to a queue.
        /// </summary>
        /// <remarks>
        /// If <paramref name="messages"/> is empty, this call is equivalent to <see cref="QueueExistsAsync"/>,
        /// and the <see cref="Task{TResult}.Result"/> property of the returned task contains
        /// <see cref="MessagesEnqueued.Empty"/>.
        /// </remarks>
        /// <typeparam name="T">The class modeling the JSON representation of the messages to post in the queue.</typeparam>
        /// <param name="queueName">The queue name.</param>
        /// <param name="messages">The messages to post.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the task completes
        /// successfully, the <see cref="Task{TResult}.Result"/> property will contain a
        /// <see cref="MessagesEnqueued"/> object representing the result of the operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="messages"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="messages"/> contains a <see langword="null"/> value.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Post_Message.28s.29">Post Message(s) (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<MessagesEnqueued> PostMessagesAsync<T>(QueueName queueName, IEnumerable<Message<T>> messages, CancellationToken cancellationToken);

        /// <summary>
        /// Posts messages to a queue.
        /// </summary>
        /// <remarks>
        /// If <paramref name="messages"/> is empty, this call is equivalent to <see cref="QueueExistsAsync"/>,
        /// and upon success the <see cref="Task{TResult}.Result"/> property of the returned task contains
        /// <see cref="MessagesEnqueued.Empty"/>.
        /// </remarks>
        /// <typeparam name="T">The class modeling the JSON representation of the messages to post in the queue.</typeparam>
        /// <param name="queueName">The queue name.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="messages">The messages to post.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="messages"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="messages"/> contains a <see langword="null"/> value.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Post_Message.28s.29">Post Message(s) (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<MessagesEnqueued> PostMessagesAsync<T>(QueueName queueName, CancellationToken cancellationToken, params Message<T>[] messages);

        /// <summary>
        /// Deletes a message from a queue.
        /// </summary>
        /// <param name="queueName">The queue name.</param>
        /// <param name="messageId">The ID of the message to delete. This is obtained from <see cref="QueuedMessage.Id">QueuedMessage.Id</see>.</param>
        /// <param name="claim">The claim for the message. If this value is <see langword="null"/>, the delete operation will fail if the message is claimed. If this value is non-<see langword="null"/>, the delete operation will fail if the message is not claimed by the specified claim.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="messageId"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Delete_Message">Delete Message (OpenStack Marconi API v1 Blueprint)</seealso>
        Task DeleteMessageAsync(QueueName queueName, MessageId messageId, Claim claim, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes messages from a queue.
        /// </summary>
        /// <remarks>
        /// <note type="warning">
        /// This method deletes messages from a queue whether or not they are currently claimed.
        /// </note>
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="messageIds">The IDs of messages to delete. These are obtained from <see cref="QueuedMessage.Id">QueuedMessage.Id</see>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="messageIds"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="messageIds"/> contains a <see langword="null"/> value.
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Delete_a_Set_of_Messages_by_ID">Delete a Set of Messages by ID (OpenStack Marconi API v1 Blueprint)</seealso>
        Task DeleteMessagesAsync(QueueName queueName, IEnumerable<MessageId> messageIds, CancellationToken cancellationToken);

        #endregion Messages

        #region Claims

        /// <summary>
        /// Claim messages from a queue.
        /// </summary>
        /// <remarks>
        /// <para>When the claim is no longer required, the code should call <see cref="Claim.DisposeAsync"/>
        /// or <see cref="Claim.Dispose()"/> to ensure the following actions are taken.</para>
        /// <list type="bullet">
        /// <item>Messages which are part of this claim which were not processed are made available to other nodes.</item>
        /// <item>The claim resource is cleaned up without waiting for the time-to-live to expire.</item>
        /// </list>
        ///
        /// <para>Messages which are not deleted before the claim is released will be eligible for
        /// reclaiming by another process.</para>
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="limit">The maximum number of messages to claim. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="timeToLive">The time to wait before the server automatically releases the claim.</param>
        /// <param name="gracePeriod">The time to wait, after the time-to-live for the claim expires, before the server allows the claimed messages to be deleted due to the individual message's time-to-live expiring.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain <see cref="Claim"/> object representing the claim.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="queueName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="limit"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="timeToLive"/> is negative or <see cref="TimeSpan.Zero"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="gracePeriod"/> is negative.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Claim_Messages">Claim Messages (OpenStack Marconi API v1 Blueprint)</seealso>
        Task<Claim> ClaimMessageAsync(QueueName queueName, int? limit, TimeSpan timeToLive, TimeSpan gracePeriod, CancellationToken cancellationToken);

        /// <summary>
        /// Gets detailed information about the current state of a claim.
        /// </summary>
        /// <remarks>
        /// <note type="caller">Use <see cref="Claim.RefreshAsync"/> instead of calling this method directly.</note>
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="claim">The claim to query.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/> property will contain a <see cref="Claim"/> object representing the claim.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="claim"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Query_Claim">Query Claim (OpenStack Marconi API v1 Blueprint)</seealso>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Task<Claim> QueryClaimAsync(QueueName queueName, Claim claim, CancellationToken cancellationToken);

        /// <summary>
        /// Renews a claim, by updating the time-to-live and resetting the age of the claim to zero.
        /// </summary>
        /// <remarks>
        /// <note type="caller">Use <see cref="Claim.RenewAsync"/> instead of calling this method directly.</note>
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="claim">The claim to renew.</param>
        /// <param name="timeToLive">The updated time-to-live for the claim.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="claim"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="timeToLive"/> is negative.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Update_Claim">Update Claim (OpenStack Marconi API v1 Blueprint)</seealso>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Task UpdateClaimAsync(QueueName queueName, Claim claim, TimeSpan timeToLive, CancellationToken cancellationToken);

        /// <summary>
        /// Immediately release a claim, making any (remaining, non-deleted) messages associated
        /// with the claim available to other workers.
        /// </summary>
        /// <remarks>
        /// <note type="caller">Use <see cref="Claim.DisposeAsync"/> instead of calling this method directly.</note>
        /// </remarks>
        /// <param name="queueName">The queue name.</param>
        /// <param name="claim">The claim to release.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="queueName"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="claim"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Release_Claim">Release Claim (OpenStack Marconi API v1 Blueprint)</seealso>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Task ReleaseClaimAsync(QueueName queueName, Claim claim, CancellationToken cancellationToken);

        #endregion
    }
}
