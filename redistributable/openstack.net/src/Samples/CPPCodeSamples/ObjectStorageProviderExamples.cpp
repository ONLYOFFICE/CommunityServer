#include "Stdafx.h"

using namespace net::openstack::Core;
using namespace net::openstack::Core::Domain;
using namespace net::openstack::Core::Providers;
using namespace System;
using namespace System::Collections::Generic;

ref class ObjectStorageProviderExamples
{
public:
#pragma region ListObjectsInContainer
	void ListObjects(IObjectStorageProvider^ provider, String^ containerName)
	{
		Console::WriteLine("Objects in container {0}", containerName);
		for each (ContainerObject^ containerObject in ListAllObjects(provider, containerName))
			Console::WriteLine("    {0}", containerObject->Name);
	}

	static IEnumerable<ContainerObject^>^ ListAllObjects(IObjectStorageProvider^ provider, String^ containerName)
	{
		return ListAllObjects(provider, containerName, Nullable<int>(), nullptr, nullptr, false, nullptr);
	}

	static IEnumerable<ContainerObject^>^ ListAllObjects(
		IObjectStorageProvider^ provider,
		String^ containerName,
		Nullable<int> blockSize,
		String^ prefix,
		String^ region,
		bool useInternalUrl,
		CloudIdentity^ identity)
	{
		if (blockSize.HasValue && blockSize.Value <= 0)
			throw gcnew ArgumentOutOfRangeException("blockSize");

		List<ContainerObject^>^ result = gcnew List<ContainerObject^>();
		ContainerObject^ lastContainerObject = nullptr;
		do
		{
			String^ marker = lastContainerObject ? lastContainerObject->Name : nullptr;
			IEnumerable<ContainerObject^>^ containerObjects =
				provider->ListObjects(containerName, blockSize, marker, nullptr, prefix, region, useInternalUrl, identity);
			int previousCount = result->Count;
			result->AddRange(containerObjects);
			if (result->Count > previousCount)
				lastContainerObject = result[result->Count - 1];
			else
				lastContainerObject = nullptr;
		} while (lastContainerObject);

		return result;
	}
#pragma endregion
};
