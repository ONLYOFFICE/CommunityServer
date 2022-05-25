namespace CSharpCodeSamples
{
    using System;
    using System.Collections.Generic;
    using net.openstack.Core.Domain;
    using net.openstack.Providers.Rackspace;

    public class IdentityProviderExamples
    {
        public void CreateProvider()
        {
            #region CreateProvider
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{apiKey}" };
            var provider = new CloudIdentityProvider(identity);
            #endregion
        }

        public void CreateProviderWithPassword()
        {
            #region CreateProviderWithPassword
            var identity = new CloudIdentity { Username = "{username}", Password = "{password}" };
            var provider = new CloudIdentityProvider(identity);
            #endregion
        }

        public void CreateUser()
        {
            var identity = new CloudIdentity();
            var provider = new CloudIdentityProvider(identity);

            #region CreateUser
            NewUser user = new NewUser("{username}", "{email}", enabled: true);
            user = provider.AddUser(user, null);
            string password = user.Password;
            #endregion
        }

        public void UpdateUser()
        {
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{apiKey}" };
            var provider = new CloudIdentityProvider(identity);

            #region UpdateUser
            User user = provider.GetUserByName("{username}", null);
            user.Username = "{newUsername}";
            provider.UpdateUser(user, null);
            #endregion
        }

        public void ListUsers()
        {
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{apiKey}" };
            var provider = new CloudIdentityProvider(identity);

            #region ListUsers
            IEnumerable<User> users = provider.ListUsers(null);
            foreach (var user in users)
                Console.WriteLine("{0}: {1}", user.Id, user.Username);
            #endregion
        }

        public void AddRoleToUser()
        {
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{apiKey}" };
            var provider = new CloudIdentityProvider(identity);

            #region AddRoleToUser
            User user = provider.GetUserByName("{username}", null);
            provider.AddRoleToUser(user.Id, "{roleId}", null);
            #endregion
        }

        public void DeleteRoleFromUser()
        {
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{apiKey}" };
            var provider = new CloudIdentityProvider(identity);

            #region DeleteRoleFromUser
            User user = provider.GetUserByName("{username}", null);
            provider.DeleteRoleFromUser(user.Id, "{roleId}", null);
            #endregion
        }

        public void ResetApiKey()
        {
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{apiKey}" };
            var provider = new CloudIdentityProvider(identity);

            #region ResetApiKey
            UserCredential credential = provider.ResetApiKey("{userId}");
            string newApiKey = credential.APIKey;
            #endregion
        }

        public void ListRoles()
        {
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{apiKey}" };
            var provider = new CloudIdentityProvider(identity);

            #region ListRoles
            IEnumerable<Role> roles = provider.ListRoles();
            foreach (var role in roles)
                Console.WriteLine("{0}: {1}", role.Id, role.Name);
            #endregion
        }

        public void DeleteUser()
        {
            var identity = new CloudIdentity { Username = "{username}", APIKey = "{apiKey}" };
            var provider = new CloudIdentityProvider(identity);

            #region DeleteUser
            provider.DeleteUser("{userId}", null);
            #endregion
        }
    }
}
