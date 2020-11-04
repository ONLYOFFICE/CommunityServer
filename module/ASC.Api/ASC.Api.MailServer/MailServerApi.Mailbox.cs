/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

// ReSharper disable InconsistentNaming

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Create mailbox
        /// </summary>
        /// <param name="name"></param>
        /// <param name="local_part"></param>
        /// <param name="domain_id"></param>
        /// <param name="user_id"></param>
        /// <param name="notifyCurrent">Send message to creating mailbox's address</param>
        /// <param name="notifyProfile">Send message to email from user profile</param>
        /// <returns>MailboxData associated with tenant</returns>
        /// <short>Create mailbox</short> 
        /// <category>Mailboxes</category>
        [Create(@"mailboxes/add")]
        public ServerMailboxData CreateMailbox(string name, string local_part, int domain_id, string user_id,
            bool notifyCurrent = false, bool notifyProfile = false)
        {
            var serverMailbox = MailEngineFactory.ServerMailboxEngine.CreateMailbox(name, local_part, domain_id, user_id);

            SendMailboxCreated(serverMailbox, notifyCurrent, notifyProfile);

            return serverMailbox;
        }

        /// <summary>
        ///    Create my mailbox
        /// </summary>
        /// <param name="name"></param>
        /// <returns>MailboxData associated with tenant</returns>
        /// <short>Create mailbox</short> 
        /// <category>Mailboxes</category>
        [Create(@"mailboxes/addmy")]
        public ServerMailboxData CreateMyMailbox(string name)
        {
            var serverMailbox = MailEngineFactory.ServerMailboxEngine.CreateMyCommonDomainMailbox(name);
            return serverMailbox;
        }

        /// <summary>
        ///    Returns list of the mailboxes associated with tenant
        /// </summary>
        /// <returns>List of MailboxData for current tenant</returns>
        /// <short>Get mailboxes list</short> 
        /// <category>Mailboxes</category>
        [Read(@"mailboxes/get")]
        public List<ServerMailboxData> GetMailboxes()
        {
            var mailboxes = MailEngineFactory.ServerMailboxEngine.GetMailboxes();
            return mailboxes;
        }

        /// <summary>
        ///    Deletes the selected mailbox
        /// </summary>
        /// <param name="id">id of mailbox</param>
        /// <returns>MailOperationResult object</returns>
        /// <exception cref="ArgumentException">Exception happens when some parameters are invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="ItemNotFoundException">Exception happens when mailbox wasn't found.</exception>
        /// <short>Remove mailbox from mail server</short> 
        /// <category>Mailboxes</category>
        [Delete(@"mailboxes/remove/{id}")]
        public MailOperationStatus RemoveMailbox(int id)
        {
            var status = MailEngineFactory.ServerMailboxEngine.RemoveMailbox(id);
            return status;
        }

        /// <summary>
        ///    Update mailbox
        /// </summary>
        /// <param name="mailbox_id">id of mailbox</param>
        /// <param name="name">sender name</param>
        /// <returns>Updated MailboxData</returns>
        /// <short>Update mailbox</short>
        /// <category>Mailboxes</category>
        [Update(@"mailboxes/update")]
        public ServerMailboxData UpdateMailbox(int mailbox_id, string name)
        {
            var mailbox = MailEngineFactory.ServerMailboxEngine.UpdateMailboxDisplayName(mailbox_id, name);
            return mailbox;
        }

        /// <summary>
        ///    Add alias to mailbox
        /// </summary>
        /// <param name="mailbox_id">id of mailbox</param>
        /// <param name="alias_name">name of alias</param>
        /// <returns>MailboxData associated with tenant</returns>
        /// <short>Add mailbox's aliases</short>
        /// <category>AddressData</category>
        [Update(@"mailboxes/alias/add")]
        public ServerDomainAddressData AddMailboxAlias(int mailbox_id, string alias_name)
        {
            var serverAlias = MailEngineFactory.ServerMailboxEngine.AddAlias(mailbox_id, alias_name);
            return serverAlias;
        }

        /// <summary>
        ///    Remove alias from mailbox
        /// </summary>
        /// <param name="mailbox_id">id of mailbox</param>
        /// <param name="address_id"></param>
        /// <returns>id of mailbox</returns>
        /// <short>Remove mailbox's aliases</short>
        /// <category>Mailboxes</category>
        [Update(@"mailboxes/alias/remove")]
        public int RemoveMailboxAlias(int mailbox_id, int address_id)
        {
            MailEngineFactory.ServerMailboxEngine.RemoveAlias(mailbox_id, address_id);
            return mailbox_id;
        }

        /// <summary>
        ///    Change mailbox password
        /// </summary>
        /// <param name="mailbox_id"></param>
        /// <param name="password"></param>
        /// <short>Change mailbox password</short> 
        /// <category>Mailboxes</category>
        [Update(@"mailboxes/changepwd")]
        public void ChangeMailboxPassword(int mailbox_id, string password)
        {
            MailEngineFactory.ServerMailboxEngine.ChangePassword(mailbox_id, password);

            SendMailboxPasswordChanged(mailbox_id);
        }

        /// <summary>
        ///    Check existence of mailbox address
        /// </summary>
        /// <param name="local_part"></param>
        /// <param name="domain_id"></param>
        /// <short>Is server mailbox address exists</short>
        /// <returns>True - address exists, False - not exists</returns>
        /// <category>Mailboxes</category>
        [Read(@"mailboxes/alias/exists")]
        public bool IsAddressAlreadyRegistered(string local_part, int domain_id)
        {
            return MailEngineFactory.ServerMailboxEngine.IsAddressAlreadyRegistered(local_part, domain_id);
        }

        /// <summary>
        ///    Validate mailbox address
        /// </summary>
        /// <param name="local_part"></param>
        /// <param name="domain_id"></param>
        /// <short>Is server mailbox address valid</short>
        /// <returns>True - address valid, False - not valid</returns>
        /// <category>Mailboxes</category>
        [Read(@"mailboxes/alias/valid")]
        public bool IsAddressValid(string local_part, int domain_id)
        {
            return MailEngineFactory.ServerMailboxEngine.IsAddressValid(local_part, domain_id);
        }

        private void SendMailboxCreated(ServerMailboxData serverMailbox, bool toMailboxUser, bool toUserProfile)
        {
            try
            {
                if (serverMailbox == null)
                    throw new ArgumentNullException("serverMailbox");

                if((!toMailboxUser && !toUserProfile))
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

                if (CoreContext.Configuration.Standalone)
                {
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

                    StudioNotifyService.Instance.SendMailboxCreated(emails, userInfo.DisplayUserName(),
                        mailbox.EMail.Address,
                        string.IsNullOrEmpty(mxHost) ? mailbox.Server : mxHost, encType.ToUpper(), mailbox.Port,
                        mailbox.SmtpPort, mailbox.Account);
                }
                else
                {
                    StudioNotifyService.Instance.SendMailboxCreated(emails, userInfo.DisplayUserName(),
                        mailbox.EMail.Address);
                }

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
                if (!CoreContext.Configuration.Standalone)
                    return;

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
