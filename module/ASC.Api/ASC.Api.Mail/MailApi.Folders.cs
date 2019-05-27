/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ASC.Api.Attributes;
using ASC.Mail;
using ASC.Mail.Core.Engine.Operations.Base;
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
        ///    Returns the list of default folders
        /// </summary>
        /// <returns>Folders list</returns>
        /// <short>Get folders</short> 
        /// <category>Folders</category>
        [Read(@"folders")]
        public IEnumerable<MailFolderData> GetFolders()
        {
            if (!Defines.IsSignalRAvailable)
                MailEngineFactory.AccountEngine.SetAccountsActivity();

            return MailEngineFactory.FolderEngine.GetFolders()
                                 .Where(f => f.id != FolderType.Sending)
                                 .ToList()
                                 .ToFolderData();
        }

        /// <summary>
        ///    Removes all the messages from the folder. Trash or Spam.
        /// </summary>
        /// <param name="folderid">Selected folder id. Trash - 4, Spam 5.</param>
        /// <short>Remove all messages from folder</short> 
        /// <category>Folders</category>
        [Delete(@"folders/{folderid:[0-9]+}/messages")]
        public int RemoveFolderMessages(int folderid)
        {
            var folderType = (FolderType) folderid;

            if (folderType == FolderType.Trash || folderType == FolderType.Spam)
            {
                MailEngineFactory.MessageEngine.SetRemoved(folderType);
            }

            return folderid;
        }


        /// <summary>
        ///    Recalculate folders counters
        /// </summary>
        /// <returns>MailOperationResult object</returns>
        /// <short>Get folders</short> 
        /// <category>Folders</category>
        /// <visible>false</visible>
        [Read(@"folders/recalculate")]
        public MailOperationStatus RecalculateFolders()
        {
            return MailEngineFactory.OperationEngine.RecalculateFolders(TranslateMailOperationStatus);
        }

        /// <summary>
        ///    Returns the list of user folders
        /// </summary>
        /// <param name="ids" optional="true">List of folder's id</param>
        /// <param name="parentId" optional="true">Selected parent folder id (root level equals 0)</param>
        /// <returns>Folders list</returns>
        /// <short>Get folders</short> 
        /// <category>Folders</category>
        [Read(@"userfolders")]
        public IEnumerable<MailUserFolderData> GetUserFolders(List<uint> ids, uint? parentId)
        {
            var list = MailEngineFactory.UserFolderEngine.GetList(ids, parentId);
            return list;
        }

        /// <summary>
        ///    Create user folder
        /// </summary>
        /// <param name="name">Folder name</param>
        /// <param name="parentId">Parent folder id (default = 0)</param>
        /// <returns>Folders list</returns>
        /// <short>Create folder</short> 
        /// <category>Folders</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Create(@"userfolders")]
        public MailUserFolderData CreateUserFolder(string name, uint parentId = 0)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                var userFolder = MailEngineFactory.UserFolderEngine.Create(name, parentId);
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
        ///    Update user folder
        /// </summary>
        /// <param name="id">Folder id</param>
        /// <param name="name">new Folder name</param>
        /// <param name="parentId">new Parent folder id (default = 0)</param>
        /// <returns>Folders list</returns>
        /// <short>Update folder</short> 
        /// <category>Folders</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"userfolders/{id}")]
        public MailUserFolderData UpdateUserFolder(uint id, string name, uint? parentId = null)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                var userFolder = MailEngineFactory.UserFolderEngine.Update(id, name, parentId);
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
        ///    Delete user folder
        /// </summary>
        /// <param name="id">Folder id</param>
        /// <short>Delete folder</short> 
        /// <category>Folders</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <returns>MailOperationResult object</returns>
        [Delete(@"userfolders/{id}")]
        public MailOperationStatus DeleteUserFolder(uint id)
        {
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            try
            {
                return MailEngineFactory.OperationEngine.RemoveUserFolder(id, TranslateMailOperationStatus);
            }
            catch (Exception)
            {
                throw new Exception(MailApiErrorsResource.ErrorInternalServer);
            }
        }

        /// <summary>
        ///    Returns the user folders by mail id
        /// </summary>
        /// <param name="mailId">List of folder's id</param>
        /// <returns>User Folder</returns>
        /// <short>Get folder by mail id</short> 
        /// <category>Folders</category>
        [Read(@"userfolders/bymail")]
        public MailUserFolderData GetUserFolderByMailId(uint mailId)
        {
            var folder = MailEngineFactory.UserFolderEngine.GetByMail(mailId);
            return folder;
        }
    }
}
