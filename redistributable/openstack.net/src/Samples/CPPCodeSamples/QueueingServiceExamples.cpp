#include "Stdafx.h"

using namespace net::openstack::Core;
using namespace net::openstack::Core::Collections;
using namespace net::openstack::Core::Domain;
using namespace net::openstack::Core::Providers;
using namespace net::openstack::Core::Domain::Queues;
using namespace net::openstack::Providers::Rackspace;
using namespace System;
using namespace System::Collections::ObjectModel;
using namespace System::Threading;
using namespace System::Threading::Tasks;

ref class QueueingServiceExamples
{
private:
	static CloudIdentity^ identity = gcnew CloudIdentity();
	static String^ region = nullptr;
	static Guid clientId = Guid::NewGuid();
	static bool internalUrl = false;
	static IIdentityProvider^ identityProvider = nullptr;

public:
	void GetHome()
	{
#pragma region GetHomeAsync (TPL)
		IQueueingService^ queueingService = gcnew CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
		Task<HomeDocument^>^ task = queueingService->GetHomeAsync(CancellationToken::None);
#pragma endregion
	}

	void GetNodeHealth()
	{
#pragma region GetNodeHealthAsync (TPL)
		IQueueingService^ queueingService = gcnew CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
		Task^ task = queueingService->GetNodeHealthAsync(CancellationToken::None);
#pragma endregion
	}

	void CreateQueue()
	{
#pragma region CreateQueueAsync (TPL)
		IQueueingService^ queueingService = gcnew CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
		QueueName^ queueName = gcnew QueueName("ExampleQueue");
		Task<bool>^ task = queueingService->CreateQueueAsync(queueName, CancellationToken::None);
#pragma endregion
	}

	void DeleteQueue()
	{
#pragma region DeleteQueueAsync (TPL)
		IQueueingService^ queueingService = gcnew CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
		QueueName^ queueName = gcnew QueueName("ExampleQueue");
		Task^ task = queueingService->DeleteQueueAsync(queueName, CancellationToken::None);
#pragma endregion
	}

#pragma region ListQueuesAsync (TPL)
	void ListQueues()
	{
		IQueueingService^ queueingService = gcnew CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
		Task<ReadOnlyCollectionPage<CloudQueue^>^>^ queuesPageTask = queueingService->ListQueuesAsync(nullptr, Nullable<int>(), true, CancellationToken::None);
		auto func = gcnew Func<Task<ReadOnlyCollectionPage<CloudQueue^>^>^, Task<ReadOnlyCollection<CloudQueue^>^>^>(GetAllPagesContinuationAsync<CloudQueue^>);
		Task<ReadOnlyCollection<CloudQueue^>^>^ queuesTask = TaskExtensions::Unwrap(queuesPageTask->ContinueWith(func));
	}

	generic<class T>
	static Task<ReadOnlyCollection<T>^>^ GetAllPagesContinuationAsync(Task<ReadOnlyCollectionPage<T>^>^ pageTask)
	{
		return ReadOnlyCollectionPageExtensions::GetAllPagesAsync(pageTask->Result, CancellationToken::None, static_cast<IProgress<ReadOnlyCollectionPage<T>^>^>(nullptr));
	}
#pragma endregion

	void QueueExists()
	{
#pragma region QueueExistsAsync (TPL)
		IQueueingService^ queueingService = gcnew CloudQueuesProvider(identity, region, clientId, internalUrl, identityProvider);
		QueueName^ queueName = gcnew QueueName("ExampleQueue");
		Task<bool>^ task = queueingService->QueueExistsAsync(queueName, CancellationToken::None);
#pragma endregion
	}
};
