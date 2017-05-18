/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Mail.DataContracts;
using ASC.Api.Mail.Extensions;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.ComplexOperations.Base;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns the list of all folders
        /// </summary>
        /// <returns>Folders list</returns>
        /// <short>Get folders</short> 
        /// <category>Folders</category>
        [Read(@"folders")]
        public IEnumerable<MailFolderData> GetFolders()
        {
            if (!IsSignalRAvailable)
                MailBoxManager.UpdateUserActivity(TenantId, Username);

            return MailBoxManager.GetFolders(TenantId, Username)
                                 .Where(f => f.id != MailFolder.Ids.temp)
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
            if (folderid == MailFolder.Ids.trash || folderid == MailFolder.Ids.spam)
            {
                MailBoxManager.DeleteFoldersMessages(TenantId, Username, folderid);
            }

            return folderid;
        }


        /// <summary>
        ///    Returns the list of all folders
        /// </summary>
        /// <returns>Folders list</returns>
        /// <short>Get folders</short> 
        /// <category>Folders</category>
        /// <visible>false</visible>
        [Read(@"folders/recalculate")]
        public MailOperationStatus RecalculateFolders()
        {
            return MailBoxManager.RecalculateFolders(TranslateMailOperationStatus);
        }
    }
}
