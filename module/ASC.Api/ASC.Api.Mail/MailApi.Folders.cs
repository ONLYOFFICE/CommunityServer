/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;
using System.Threading;

using ASC.Api.Attributes;
using ASC.Mail;
using ASC.Mail.Core.Engine.Operations.Base;
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Extensions;
using ASC.Web.Mail.Resources;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Returns a list of default folders.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.MailFolderData, ASC.Mail">List of default folders</returns>
        /// <short>Get the default folders</short> 
        /// <category>Folders</category>
        /// <path>api/2.0/mail/folders</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"folders")]
        public IEnumerable<MailFolderData> GetFolders()
        {
            if (!Defines.IsSignalRAvailable)
                MailEngineFactory.AccountEngine.SetAccountsActivity();

            ActionEngine.SendUserAlive(-1, null);

            return MailEngineFactory.FolderEngine.GetFolders()
                                 .Where(f => f.id != FolderType.Sending)
                                 .ToList()
                                 .ToFolderData();
        }

        /// <summary>
        /// Removes all the messages from the trash or spam folder.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="folderid">Folder ID: 4 - Trash, 5 - Spam</param>
        /// <short>Remove folder messages</short> 
        /// <category>Folders</category>
        /// <returns>Folder ID</returns>
        /// <path>api/2.0/mail/folders/{folderid}/messages</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"folders/{folderid:[0-9]+}/messages")]
        public int RemoveFolderMessages(int folderid)
        {
            var folderType = (FolderType)folderid;

            if (folderType == FolderType.Trash || folderType == FolderType.Spam)
            {
                var idsToImapSync = MailEngineFactory.MessageEngine.SetRemoved(folderType);

                ActionEngine.SendUserActivity(idsToImapSync, MailUserAction.SetAsDeleted);
            }

            return folderid;
        }


        /// <summary>
        /// Recalculates folder counters.
        /// </summary>
        /// <returns>Operation status</returns>
        /// <short>Recalculate folders</short> 
        /// <category>Folders</category>
        /// <path>api/2.0/mail/folders/recalculate</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read(@"folders/recalculate")]
        public MailOperationStatus RecalculateFolders()
        {
            return MailEngineFactory.OperationEngine.RecalculateFolders(TranslateMailOperationStatus);
        }

        /// <summary>
        /// Returns a list of user folders with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.List{System.UInt32}, System.Collections.Generic" method="url" name="ids" optional="true">List of folder IDs</param>
        /// <param type="System.Nullable{System.UInt32}, System" method="url" name="parentId" optional="true">Parent folder ID (root level is equal to 0)</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailUserFolderData, ASC.Mail">List of folders</returns>
        /// <short>Get the user folders</short> 
        /// <category>Folders</category>
        /// <path>api/2.0/mail/userfolders</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"userfolders")]
        public IEnumerable<MailUserFolderData> GetUserFolders(List<uint> ids, uint? parentId)
        {
            var list = MailEngineFactory.UserFolderEngine.GetList(ids, parentId);
            return list;
        }

        /// <summary>
        /// Creates a user folder with the name specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="name">Folder name</param>
        /// <param type="System.UInt32, System" name="parentId">Parent folder ID (root level is equal to 0)</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailUserFolderData, ASC.Mail">Folder information</returns>
        /// <short>Create a folder</short> 
        /// <category>Folders</category>
        /// <path>api/2.0/mail/userfolders</path>
        /// <httpMethod>POST</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Create(@"userfolders")]
        public MailUserFolderData CreateUserFolder(string name, uint parentId = 0)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                var userFolder = MailEngineFactory.UserFolderEngine.Create(name, parentId);

                ActionEngine.SendUserActivity(new List<int>() { (int)userFolder.Id }, MailUserAction.CreateFolder);

                return userFolder;
            }
            catch (AlreadyExistsFolderException)
            {
                throw new ArgumentException(MailApiResource.ErrorUserFolderNameAlreadyExists
                    .Replace("%1", "\"" + name + "\""));
            }
            catch (EmptyFolderException)
            {
                throw new ArgumentException(MailApiResource.ErrorUserFoldeNameCantBeEmpty);
            }
            catch (Exception)
            {
                throw new Exception(MailApiErrorsResource.ErrorInternalServer);
            }
        }

        /// <summary>
        /// Updates a user folder with the parameters specified in the request.
        /// </summary>
        /// <param type="System.UInt32, System" method="url" name="id">Folder ID</param>
        /// <param type="System.String, System" name="name">New folder name</param>
        /// <param type="System.Nullable{System.UInt32}, System" name="parentId">New parent folder ID (root level is equal to 0)</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailUserFolderData, ASC.Mail">Folder information</returns>
        /// <short>Update a folder</short> 
        /// <category>Folders</category>
        /// <path>api/2.0/mail/userfolders/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        [Update(@"userfolders/{id}")]
        public MailUserFolderData UpdateUserFolder(uint id, string name, uint? parentId = null)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                var userFolder = MailEngineFactory.UserFolderEngine.Update(id, name, parentId);

                ActionEngine.SendActivityUserFolderUpdate(id, name, parentId);

                return userFolder;
            }
            catch (AlreadyExistsFolderException)
            {
                throw new ArgumentException(MailApiResource.ErrorUserFolderNameAlreadyExists
                    .Replace("%1", "\"" + name + "\""));
            }
            catch (EmptyFolderException)
            {
                throw new ArgumentException(MailApiResource.ErrorUserFoldeNameCantBeEmpty);
            }
            catch (Exception)
            {
                throw new Exception(MailApiErrorsResource.ErrorInternalServer);
            }
        }

        /// <summary>
        /// Deletes a user folder with the ID specified in the request.
        /// </summary>
        /// <param type="System.UInt32, System" method="url" name="id">Folder ID</param>
        /// <short>Delete a folder</short> 
        /// <category>Folders</category>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
        /// <returns type="ASC.Mail.Core.Engine.Operations.Base.MailOperationStatus, ASC.Mail">Operation status</returns>
        /// <path>api/2.0/mail/userfolders/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"userfolders/{id}")]
        public MailOperationStatus DeleteUserFolder(uint id)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                ActionEngine.SendUserActivity(new List<int>(), MailUserAction.DeleteUserFolder, userFolderId: id);

                return MailEngineFactory.OperationEngine.RemoveUserFolder(id, TranslateMailOperationStatus);
            }
            catch (Exception)
            {
                throw new Exception(MailApiErrorsResource.ErrorInternalServer);
            }
        }

        /// <summary>
        /// Returns a user folder by the mail ID specified in the request.
        /// </summary>
        /// <param type="System.UInt32, System" method="url" name="mailId">Mail ID</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailUserFolderData, ASC.Mail">User folder</returns>
        /// <short>Get a folder by mail ID</short> 
        /// <category>Folders</category>
        /// <path>api/2.0/mail/userfolders/bymail</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"userfolders/bymail")]
        public MailUserFolderData GetUserFolderByMailId(uint mailId)
        {
            var folder = MailEngineFactory.UserFolderEngine.GetByMail(mailId);
            return folder;
        }
    }
}
