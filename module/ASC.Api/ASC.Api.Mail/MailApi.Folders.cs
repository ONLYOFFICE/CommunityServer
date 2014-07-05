/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System.Collections.Generic;
using ASC.Api.Attributes;
using ASC.Mail.Aggregator;
using ASC.Specific;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns the list of all folders
        /// </summary>
        /// <param name="last_check_time" optional="true"> Filter folders for last_check_time. Get folders with greater date time.</param>
        /// <returns>Folders list</returns>
        /// <short>Get folders</short> 
        /// <category>Folders</category>
        [Read(@"folders")]
        public IEnumerable<MailBoxManager.MailFolderInfo> GetFolders(ApiDateTime last_check_time)
        {
            if (null != last_check_time)
            {
                var api_date = new ApiDateTime(MailBoxManager.GetMessagesModifyDate(TenantId, Username));
                var compare_rez = api_date.CompareTo(last_check_time);

                if (compare_rez < 1 && System.DateTime.MinValue != api_date) // if api_date == DateTime.MinValue then there are no folders in mail_folder
                {
                    return null;
                }
            }
            return FoldersList;
        }

        /// <summary>
        ///    Returns change date of folderid.
        /// </summary>
        /// <param name="folderid">Selected folder id.</param>
        /// <returns>Last modify folder DateTime</returns>
        /// <short>Get folder change date</short> 
        /// <category>Folders</category>
        [Read(@"folders/{folderid:[0-9]+}/modify_date")]
        public ApiDateTime GetFolderModifyDate(int folderid)
        {
            return new ApiDateTime(MailBoxManager.GetFolderModifyDate(TenantId, Username, folderid));
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

        private IEnumerable<MailBoxManager.MailFolderInfo> FoldersList
        {
            get { return MailBoxManager.GetFoldersList(TenantId, Username, true); }
        }
    }
}
