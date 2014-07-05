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
using System.Runtime.Serialization;
using ASC.Mail.Aggregator.Collection;
using ASC.Mail.Aggregator.Filter;
using ASC.Mail.Service.DAO;
using ASC.Mail.Aggregator;

namespace ASC.Mail.Service
{
    [DataContract(Name = "CommonHash", Namespace = "")]
    public class CommonHash
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int FolderId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int Page { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PrecisedTimeAll { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string PrecisedTimeFolder { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ItemList<MailMessageItem> Messages { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long? TotalMessagesFiltered { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<MailFolder> Folders { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public ItemList<MailTag> Tags { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<MailAccount> Accounts { get; set; }

        public CommonHash(ItemList<MailMessageItem> messages,
            List<MailFolder> folders, ItemList<MailTag> tags, List<MailAccount> accounts,
            int folder_id, int page, long? total_messages,
            string precised_time_all, string precised_time_folder)
        {
            PrecisedTimeAll = precised_time_all;
            PrecisedTimeFolder = precised_time_folder;
            Messages = messages;
            Folders = folders;
            Tags = tags;
            FolderId = folder_id;
            Page = page;
            TotalMessagesFiltered = total_messages;
            Accounts = accounts;
        }

        private CommonHash()
        {
        }

        public static CommonHash FoldersResponse(List<MailFolder> folders, string precised_time_all)
        {
            return new CommonHash() { Folders = folders, PrecisedTimeAll = precised_time_all };
        }

        public static CommonHash TagsResponse(ItemList<MailTag> tags)
        {
            return new CommonHash() { Tags = tags };
        }

        public static CommonHash MessagesResponse(ItemList<MailMessageItem> messages, long total_messages, int page, string precised_time_folder)
        {
            return new CommonHash() { 
                Messages = messages, 
                TotalMessagesFiltered = total_messages, 
                Page = page,
                PrecisedTimeFolder = precised_time_folder };
        }

    }
}
