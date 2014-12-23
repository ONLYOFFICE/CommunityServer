/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Mail.Aggregator;
using ASC.Api.Mail.Resources;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        ///    Returns list of the tags used in Mail
        /// </summary>
        /// <returns>Tags list. Tags represented as JSON.</returns>
        /// <short>Get tags list</short> 
        /// <category>Tags</category>
        [Read(@"tags")]
        public IEnumerable<MailTag> GetTags()
        {
            return MailBoxManager.GetTagsList(TenantId, Username, false);
        }

        /// <summary>
        ///    Creates a new tag
        /// </summary>
        /// <param name="name">Tag name represented as string</param>
        /// <param name="style">Style identificator. With postfix will be added to tag css style whe it will represent. Specifies color of tag.</param>
        /// <param name="addresses">Specifies list of addresses tag associated with.</param>
        /// <returns>MailTag</returns>
        /// <short>Create tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Create(@"tags")]
        public MailTag CreateTag(string name, string style, IEnumerable<string> addresses)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException(MailApiResource.ErrorTagNameCantBeEmpty);

            //Todo: Add format string to resource
            if(MailBoxManager.TagExists(TenantId, Username, name))
                throw new ArgumentException(MailApiResource.ErrorTagNameAlreadyExists.Replace("%1", "\"" + name + "\""));

            return MailBoxManager.SaveMailTag(TenantId, Username, new MailTag(0, name, addresses.ToList(), style, 0));

        }

        /// <summary>
        ///    Updates the selected tag
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name">Tag name represented as string</param>
        /// <param name="style">Style identificator. With postfix will be added to tag css style whe it will represent. Specifies color of tag.</param>
        /// <param name="addresses">Specifies list of addresses tag associated with.</param>
        /// <returns>Updated MailTag</returns>
        /// <short>Update tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"tags/{id}")]
        public MailTag UpdateTag(int id, string name, string style, IEnumerable<string> addresses)
        {
            if (id < 0)
                throw new ArgumentException("Invalid tag id", "id");

            if (String.IsNullOrEmpty(name))
                throw new ArgumentException(MailApiResource.ErrorTagNameCantBeEmpty);

            var tag = MailBoxManager.GetMailTag(TenantId, Username, id);
            if (tag == null)
                throw new ArgumentException();

            //Check exsisting label
            var t = MailBoxManager.GetMailTag(TenantId, Username, name);
            if(t != null && t.Id != id) throw new ArgumentException(MailApiResource.ErrorTagNameAlreadyExists.Replace("%1", "\"" + name + "\""));
                          

            tag.Name = name;
            tag.Style = style;
            tag.Addresses = new MailTag.AddressesList<string>(addresses);
            MailBoxManager.SaveMailTag(TenantId, Username, tag);

            return tag;
        }

        /// <summary>
        ///    Deletes the selected tag from TLMail
        /// </summary>
        /// <param name="id">Tag for deleting id</param>
        /// <returns>Deleted MailTag</returns>
        /// <short>Delete tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Delete(@"tags/{id}")]
        public int DeleteTag(int id)
        {
            if (id < 0)
                throw new ArgumentException("Invalid tag id", "id");

            MailBoxManager.DeleteTag(TenantId, Username, id);
            return id;
        }

        /// <summary>
        ///    Adds the selected tag to the messages
        /// </summary>
        /// <param name="id">Tag for setting id</param>
        /// <param name="messages">Messages id for setting.</param>
        /// <returns>Setted MailTag</returns>
        /// <short>Set tag to messages</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"tags/{id}/set")]
        public int SetTag(int id, IEnumerable<int> messages)
        {
            //Todo: Fix transformations
            var messages_ids = messages as IList<int> ?? messages.ToList();
            if (!messages_ids.Any())
                throw new ArgumentException("Messages are empty", "messages");

            MailBoxManager.SetMessagesTag(TenantId, Username, id, messages_ids);
            return id;
        }

        //Todo: Fix english in that comments
        /// <summary>
        ///    Removes the specified tag from messages
        /// </summary>
        /// <param name="id">Tag for removing id</param>
        /// <param name="messages">Messages id for removing.</param>
        /// <returns>Removed mail tag</returns>
        /// <short>Remove tag from messages</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        [Update(@"tags/{id}/unset")]
        public int UnsetTag(int id, IEnumerable<int> messages)
        {
            //Todo: Fix transformations
            var messages_ids = messages as IList<int> ?? messages.ToList();
            if (!messages_ids.Any())
                throw new ArgumentException("Messages are empty", "messages");

            MailBoxManager.UnsetMessagesTag(TenantId, Username, id, messages_ids);
            return id;
        }
    }
}
