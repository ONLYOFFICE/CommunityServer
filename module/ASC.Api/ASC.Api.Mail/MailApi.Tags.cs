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
using ASC.Mail.Data.Contracts;
using ASC.Mail.Extensions;
using ASC.Web.Mail.Resources;

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
        public IEnumerable<MailTagData> GetTags()
        {
            return MailEngineFactory.TagEngine.GetTags().ToTagData();
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
        public MailTagData CreateTag(string name, string style, IEnumerable<string> addresses)
        {
            //TODO: Is it necessary?
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(MailApiResource.ErrorTagNameCantBeEmpty);

            if (MailEngineFactory.TagEngine.IsTagExists(name))
                throw new ArgumentException(MailApiResource.ErrorTagNameAlreadyExists.Replace("%1", "\"" + name + "\""));

            return MailEngineFactory.TagEngine.CreateTag(name, style, addresses).ToTagData();

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
        public MailTagData UpdateTag(int id, string name, string style, IEnumerable<string> addresses)
        {
            if (id < 0)
                throw new ArgumentException(@"Invalid tag id", "id");

            //TODO: Is it necessary?
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(MailApiResource.ErrorTagNameCantBeEmpty);

            try
            {
                var tag = MailEngineFactory.TagEngine.UpdateTag(id, name, style, addresses);

                return tag.ToTagData();
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Equals("Tag name already exists"))
                    throw new ArgumentException(MailApiResource.ErrorTagNameAlreadyExists.Replace("%1",
                        "\"" + name + "\""));

                throw;
            }
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
                throw new ArgumentException(@"Invalid tag id", "id");

            if (!MailEngineFactory.TagEngine.DeleteTag(id))
                throw new Exception("DeleteTag failed");

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
        public int SetTag(int id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Messages are empty", "messages");

            MailEngineFactory.TagEngine.SetMessagesTag(messages, id);

            return id;
        }

        /// <summary>
        ///    Removes the specified tag from messages
        /// </summary>
        /// <param name="id">Tag for removing id</param>
        /// <param name="messages">Messages id for removing.</param>
        /// <returns>Removed mail tag</returns>
        /// <short>Remove tag from messages</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when parameters are invalid. Text description contains parameter name and text description.</exception>
        [Update(@"tags/{id}/unset")]
        public int UnsetTag(int id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Messages are empty", "messages");

            MailEngineFactory.TagEngine.UnsetMessagesTag(messages, id);

            return id;
        }
    }
}
