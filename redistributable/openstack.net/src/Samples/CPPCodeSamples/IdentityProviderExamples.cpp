#include "Stdafx.h"

using namespace net::openstack::Core::Domain;
using namespace net::openstack::Providers::Rackspace;
using namespace System;

ref class IdentityProviderExamples
{
public:
	void CreateProvider()
	{
#pragma region CreateProvider
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);
#pragma endregion
	}

	void CreateProviderWithPassword()
	{
#pragma region CreateProviderWithPassword
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->Password = "{password}";
		auto provider = gcnew CloudIdentityProvider(identity);
#pragma endregion
	}

	void CreateUser()
	{
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);

#pragma region CreateUser
		auto user = gcnew NewUser("{username}", "{email}", nullptr, true);
		user = provider->AddUser(user, nullptr);
		auto password = user->Password;
#pragma endregion
	}

	void UpdateUser()
	{
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);

#pragma region UpdateUser
		auto user = provider->GetUserByName("{username}", nullptr);
		user->Username = "{newUsername}";
		provider->UpdateUser(user, nullptr);
#pragma endregion
	}

	void ListUsers()
	{
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);

#pragma region ListUsers
		auto users = provider->ListUsers(nullptr);
		for each (User^ user in users)
			Console::WriteLine("{0}: {1}", user->Id, user->Username);
#pragma endregion
	}

	void AddRoleToUser()
	{
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);

#pragma region AddRoleToUser
		auto user = provider->GetUserByName("{username}", nullptr);
		provider->AddRoleToUser(user->Id, "{roleId}", nullptr);
#pragma endregion
	}

	void DeleteRoleFromUser()
	{
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);

#pragma region DeleteRoleFromUser
		auto user = provider->GetUserByName("{username}", nullptr);
		provider->DeleteRoleFromUser(user->Id, "{roleId}", nullptr);
#pragma endregion
	}

	void ResetApiKey()
	{
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);

#pragma region ResetApiKey
		auto credential = provider->ResetApiKey("{userId}", nullptr);
		auto newApiKey = credential->APIKey;
#pragma endregion
	}

	void ListRoles()
	{
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);

#pragma region ListRoles
		auto roles = provider->ListRoles(nullptr, Nullable<int>(), Nullable<int>(), nullptr);
		for each (Role^ role in roles)
			Console::WriteLine("{0}: {1}", role->Id, role->Name);
#pragma endregion
	}

	void DeleteUser()
	{
		auto identity = gcnew CloudIdentity();
		identity->Username = "{username}";
		identity->APIKey = "{apiKey}";
		auto provider = gcnew CloudIdentityProvider(identity);

#pragma region DeleteUser
		provider->DeleteUser("{userId}", nullptr);
#pragma endregion
	}
};
