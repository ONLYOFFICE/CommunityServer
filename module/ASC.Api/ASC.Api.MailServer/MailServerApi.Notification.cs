using System;
using System.Security;
using ASC.Api.Attributes;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;
using ASC.Mail.Server.Dal;
using ASC.Mail.Server.Utils;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Create address for tenant notifications
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="domain_id"></param>
        /// <returns>NotificationAddressData associated with tenant</returns>
        /// <short>Create notification address</short> 
        /// <category>Notifications</category>
        [Create(@"notification/address/add")]
        public NotificationAddressData CreateNotificationAddress(string name, string password, int domain_id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", @"Invalid address username.");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password", @"Invalid password.");

            if (name.Length > 64)
                throw new ArgumentException(@"Local part of address exceed limitation of 64 characters.", "name");

            if (!Parser.IsEmailLocalPartValid(name))
                throw new ArgumentException(@"Incorrect address username.", "name");

            if (!Parser.IsPasswordValid(password))
                throw new ArgumentException(
                    @"Incorrect password. The password's first character must be a letter," +
                    @" it must contain at least 6 characters and no more than 15 characters " +
                    @"and no characters other than letters, numbers and the underscore may be used",
                    "password");

            var localPart = name.ToLowerInvariant();

            if (domain_id < 0)
                throw new ArgumentException(@"Invalid domain id.", "domain_id");

            var domain = MailServer.GetWebDomain(domain_id, MailServerFactory);

            var notificationAddress = MailServer.CreateNotificationAddress(localPart, password, domain, MailServerFactory);

            return notificationAddress.ToNotificationAddressData();

        }

        /// <summary>
        ///    Deletes address for notification 
        /// </summary>
        /// <short>Remove mailbox from mail server</short> 
        /// <category>Notifications</category>
        [Delete(@"notification/address/remove")]
        public void RemoveNotificationAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException(@"Invalid mailbox address.", "address");

            MailServer.DeleteNotificationAddress(address);
        }
    }
}
