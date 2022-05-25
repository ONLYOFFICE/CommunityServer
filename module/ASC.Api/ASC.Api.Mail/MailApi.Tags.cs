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
        /// Returns a list of all the tags used in the Mail module.
        /// </summary>
        /// <returns>List of tags represented as JSON</returns>
        /// <short>Get tags</short> 
        /// <category>Tags</category>
        [Read(@"tags")]
        public IEnumerable<MailTagData> GetTags()
        {
            return MailEngineFactory.TagEngine.GetTags().ToTagData();
        }

        /// <summary>
        /// Creates a new tag with the parameters specified in  the request.
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <param name="style">Style identificator: a postfix  which represents the css style (tag color)</param>
        /// <param name="addresses">List of addresses which tag is associated with</param>
        /// <returns>Mail tag</returns>
        /// <short>Create a tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when the parameters are invalid. Text description contains parameter name and text description.</exception>
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
        /// Updates a tag with the ID specified in the request.
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <param name="name">New tag name</param>
        /// <param name="style">New style identificator: a postfix  which represents the css style (tag color)</param>
        /// <param name="addresses">New list of addresses which tag is associated with</param>
        /// <returns>Updated mail tag</returns>
        /// <short>Update a tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when the parameters are invalid. Text description contains parameter name and text description.</exception>
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
        /// Deletes a tag with the ID specified in the request from TLMail.
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <returns>Deleted mail tag</returns>
        /// <short>Delete a tag</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when the parameters are invalid. Text description contains parameter name and text description.</exception>
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
        /// Adds a tag with the ID specified in the request to the messages.
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <param name="messages">List of message IDs</param>
        /// <returns>Added mail tag</returns>
        /// <short>Set a tag to the messages</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when the parameters are invalid. Text description contains parameter name and text description.</exception>
        [Update(@"tags/{id}/set")]
        public int SetTag(int id, List<int> messages)
        {
            if (!messages.Any())
                throw new ArgumentException(@"Messages are empty", "messages");

            MailEngineFactory.TagEngine.SetMessagesTag(messages, id);

            return id;
        }

        /// <summary>
        /// Removes a tag with the ID specified in the request from the messages.
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <param name="messages">List of message IDs</param>
        /// <returns>Removed mail tag</returns>
        /// <short>Remove a tag from the messages</short> 
        /// <category>Tags</category>
        /// <exception cref="ArgumentException">Exception happens when the parameters are invalid. Text description contains parameter name and text description.</exception>
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
