/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Core;
using ASC.Core.Users;
using ASC.Mail;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Web.Studio.Core.Notify;

using ASC.Web.Studio.PublicResources;

// ReSharper disable InconsistentNaming

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        /// Creates a mailbox with the parameters specified in the request.
        /// </summary>
        /// <param name="name">Mailbox name</param>
        /// <param name="local_part">Mailbox local part</param>
        /// <param name="domain_id">Mailbox domain ID</param>
        /// <param name="user_id">User ID</param>
        /// <param name="notifyCurrent">Specifies if the notifications will be sent to the email address from which this mailbox was created or not</param>
        /// <param name="notifyProfile">Specifies if the notifications will be sent to the email address from the current user's profile or not</param>
        /// <returns>Mailbox data associated with the tenant</returns>
        /// <short>Create a mailbox</short> 
        /// <category>Mailboxes</category>
        [Create(@"mailboxes/add")]
        public ServerMailboxData CreateMailbox(string name, string local_part, int domain_id, string user_id,
            bool notifyCurrent = false, bool notifyProfile = false)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);

            var serverMailbox = MailEngineFactory.ServerMailboxEngine.CreateMailbox(name, local_part, domain_id, user_id);

            SendMailboxCreated(serverMailbox, notifyCurrent, notifyProfile);

            return serverMailbox;
        }

        /// <summary>
        /// Create my common domain mailbox with the name specified in the request.
        /// </summary>
        /// <param name="name">Sender name</param>
        /// <returns>Mailbox data associated with the tenant</returns>
        /// <short>Create my mailbox</short> 
        /// <category>Mailboxes</category>
        [Create(@"mailboxes/addmy")]
        public ServerMailboxData CreateMyMailbox(string name)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var serverMailbox = MailEngineFactory.ServerMailboxEngine.CreateMyCommonDomainMailbox(name);
            return serverMailbox;
        }

        /// <summary>
        /// Returns a list of all the mailboxes associated with the tenant.
        /// </summary>
        /// <returns>List of mailbox data for the current tenant</returns>
        /// <short>Get mailboxes</short> 
        /// <category>Mailboxes</category>
        [Read(@"mailboxes/get")]
        public List<ServerMailboxData> GetMailboxes()
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var mailboxes = MailEngineFactory.ServerMailboxEngine.GetMailboxes();
            return mailboxes;
        }

        /// <summary>
        /// Deletes a mailbox with the ID specified in the request.
        /// </summary>
        /// <param name="id">Mailbox ID</param>
        /// <returns>Operation status</returns>
        /// <exception cref="ArgumentException">Exception happens when some parameters are invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="ItemNotFoundException">Exception happens when mailbox wasn't found.</exception>
        /// <short>Remove a mailbox from the mail server</short> 
        /// <category>Mailboxes</category>
        [Delete(@"mailboxes/remove/{id}")]
        public MailOperationStatus RemoveMailbox(int id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var status = MailEngineFactory.ServerMailboxEngine.RemoveMailbox(id);
            return status;
        }

        /// <summary>
        /// Updates a mailbox with the ID specified in the request.
        /// </summary>
        /// <param name="mailbox_id">Mailbox ID</param>
        /// <param name="name">New sender name</param>
        /// <returns>Updated mailbox data</returns>
        /// <short>Update a mailbox</short>
        /// <category>Mailboxes</category>
        [Update(@"mailboxes/update")]
        public ServerMailboxData UpdateMailbox(int mailbox_id, string name)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var mailbox = MailEngineFactory.ServerMailboxEngine.UpdateMailboxDisplayName(mailbox_id, name);
            return mailbox;
        }

        /// <summary>
        /// Adds an alias to the mailbox with the ID specified in the request.
        /// </summary>
        /// <param name="mailbox_id">Mailbox ID</param>
        /// <param name="alias_name">Mailbox alias</param>
        /// <returns>Mailbox data associated with the tenant</returns>
        /// <short>Add a mailbox alias</short>
        /// <category>Address data</category>
        [Update(@"mailboxes/alias/add")]
        public ServerDomainAddressData AddMailboxAlias(int mailbox_id, string alias_name)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var serverAlias = MailEngineFactory.ServerMailboxEngine.AddAlias(mailbox_id, alias_name);
            return serverAlias;
        }

        /// <summary>
        /// Removes an alias from the mailbox with the ID specified in the request.
        /// </summary>
        /// <param name="mailbox_id">Mailbox ID</param>
        /// <param name="address_id">Mailbox address ID</param>
        /// <returns>Mailbox ID</returns>
        /// <short>Remove a mailbox alias</short>
        /// <category>Mailboxes</category>
        [Update(@"mailboxes/alias/remove")]
        public int RemoveMailboxAlias(int mailbox_id, int address_id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            MailEngineFactory.ServerMailboxEngine.RemoveAlias(mailbox_id, address_id);
            return mailbox_id;
        }

        /// <summary>
        /// Changes a password of the mailbox with the ID specified in the request.
        /// </summary>
        /// <param name="mailbox_id">Mailbox ID</param>
        /// <param name="password">New password</param>
        /// <short>Change a mailbox password</short> 
        /// <category>Mailboxes</category>
        [Update(@"mailboxes/changepwd")]
        public void ChangeMailboxPassword(int mailbox_id, string password)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            MailEngineFactory.ServerMailboxEngine.ChangePassword(mailbox_id, password);

            SendMailboxPasswordChanged(mailbox_id);
        }

        /// <summary>
        /// Checks if the mailbox address is already registered or not.
        /// </summary>
        /// <param name="local_part">Mailbox local part</param>
        /// <param name="domain_id">Mailbox domain ID</param>
        /// <short>Check the mailbox address existence</short>
        /// <returns>Boolean value: True - address exists, False - address does not exist</returns>
        /// <category>Mailboxes</category>
        [Read(@"mailboxes/alias/exists")]
        public bool IsAddressAlreadyRegistered(string local_part, int domain_id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            return MailEngineFactory.ServerMailboxEngine.IsAddressAlreadyRegistered(local_part, domain_id);
        }

        /// <summary>
        /// Checks if the mailbox address is valid or not.
        /// </summary>
        /// <param name="local_part">Mailbox local part</param>
        /// <param name="domain_id">Mailbox domain ID</param>
        /// <short>Validate the mailbox address</short>
        /// <returns>Boolean value: True - address is valid, False - address is not valid</returns>
        /// <category>Mailboxes</category>
        [Read(@"mailboxes/alias/valid")]
        public bool IsAddressValid(string local_part, int domain_id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            return MailEngineFactory.ServerMailboxEngine.IsAddressValid(local_part, domain_id);
        }

        private void SendMailboxCreated(ServerMailboxData serverMailbox, bool toMailboxUser, bool toUserProfile)
        {
            try
            {
                if (serverMailbox == null)
                    throw new ArgumentNullException("serverMailbox");

                if ((!toMailboxUser && !toUserProfile))
                    return;

                var emails = new List<string>();

                if (toMailboxUser)
                {
                    emails.Add(serverMailbox.Address.Email);
                }

                var userInfo = CoreContext.UserManager.GetUsers(new Guid(serverMailbox.UserId));

                if (userInfo == null || userInfo.Equals(Core.Users.Constants.LostUser))
                    throw new Exception(string.Format("SendMailboxCreated(mailboxId={0}): user not found",
                        serverMailbox.Id));

                if (toUserProfile)
                {
                    if (userInfo != null && !userInfo.Equals(Core.Users.Constants.LostUser))
                    {
                        if (!emails.Contains(userInfo.Email) &&
                            userInfo.ActivationStatus == EmployeeActivationStatus.Activated)
                        {
                            emails.Add(userInfo.Email);
                        }
                    }
                }

                var mailbox =
                    MailEngineFactory.MailboxEngine.GetMailboxData(
                        new ConcreteUserServerMailboxExp(serverMailbox.Id, TenantId, serverMailbox.UserId));

                if (mailbox == null)
                    throw new Exception(string.Format("SendMailboxCreated(mailboxId={0}): mailbox not found",
                        serverMailbox.Id));

                var encType = Enum.GetName(typeof(EncryptionType), mailbox.Encryption) ?? Defines.START_TLS;

                string mxHost = null;

                try
                {
                    mxHost = MailEngineFactory.ServerEngine.GetMailServerMxDomain();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("GetMailServerMxDomain() failed. Exception: {0}", ex.ToString());
                }

                var skipSettings = string.IsNullOrEmpty(mxHost);

                try
                {
                    var domain = MailEngineFactory.ServerDomainEngine.GetCommonDomain();

                    skipSettings = domain.Id == serverMailbox.Address.DomainId;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("GetCommonDomain() failed. Exception: {0}", ex.ToString());
                }

                StudioNotifyService.Instance.SendMailboxCreated(emails, userInfo.DisplayUserName(),
                    mailbox.EMail.Address,
                    string.IsNullOrEmpty(mxHost) ? mailbox.Server : mxHost, encType.ToUpper(), mailbox.Port,
                    mailbox.SmtpPort, mailbox.Account, skipSettings);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private void SendMailboxPasswordChanged(int mailboxId)
        {
            try
            {
                if (mailboxId < 0)
                    throw new ArgumentNullException("mailboxId");

                var mailbox =
                    MailEngineFactory.MailboxEngine.GetMailboxData(
                        new ConcreteTenantServerMailboxExp(mailboxId, TenantId, false));

                if (mailbox == null)
                    throw new Exception(string.Format("SendMailboxPasswordChanged(mailboxId={0}): mailbox not found",
                        mailboxId));

                var userInfo = CoreContext.UserManager.GetUsers(new Guid(mailbox.UserId));

                if (userInfo == null || userInfo.Equals(Core.Users.Constants.LostUser))
                    throw new Exception(string.Format("SendMailboxPasswordChanged(mailboxId={0}): user not found",
                        mailboxId));

                var toEmails = new List<string>
                {
                    userInfo.ActivationStatus == EmployeeActivationStatus.Activated
                        ? userInfo.Email
                        : mailbox.EMail.Address
                };

                StudioNotifyService.Instance.SendMailboxPasswordChanged(toEmails,
                    userInfo.DisplayUserName(), mailbox.EMail.Address);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }
    }
}
