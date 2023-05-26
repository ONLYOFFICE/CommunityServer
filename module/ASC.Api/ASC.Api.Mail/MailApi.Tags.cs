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
        /// <returns type="ASC.Mail.Data.Contracts.MailTagData, ASC.Mail">List of tags represented as JSON</returns>
        /// <short>Get tags</short> 
        /// <category>Tags</category>
        /// <path>api/2.0/mail/tags</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"tags")]
        public IEnumerable<MailTagData> GetTags()
        {
            return MailEngineFactory.TagEngine.GetTags().ToTagData();
        }

        /// <summary>
        /// Creates a new tag with the parameters specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="name">Tag name</param>
        /// <param type="System.String, System" name="style">Style identifier: a postfix which represents the CSS style (tag color)</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="addresses">List of addresses associated with a tag</param>
        /// <returns type="ASC.Mail.Data.Contracts.MailTagData, ASC.Mail">Mail tag</returns>
        /// <short>Create a tag</short> 
        /// <category>Tags</category>
        /// <path>api/2.0/mail/tags</path>
        /// <httpMethod>POST</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
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
        /// <param type="System.Int32, System" method="url" name="id">Tag ID</param>
        /// <param type="System.String, System" name="name">New tag name</param>
        /// <param type="System.String, System" name="style">New style identifier: a postfix which represents the CSS style (tag color)</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.String}, System.Collections.Generic" name="addresses">New list of addresses associated with a tag</param>
        /// <returns>Updated mail tag</returns>
        /// <short>Update a tag</short> 
        /// <category>Tags</category>
        /// <path>api/2.0/mail/tags/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
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
        /// Deletes a tag with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Tag ID</param>
        /// <returns>Deleted mail tag</returns>
        /// <short>Delete a tag</short> 
        /// <category>Tags</category>
        /// <path>api/2.0/mail/tags/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
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
        /// <param type="System.Int32, System" method="url" name="id">Tag ID</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="messages">List of message IDs</param>
        /// <returns>Added mail tag ID</returns>
        /// <short>Set a tag to the messages</short> 
        /// <category>Tags</category>
        /// <path>api/2.0/mail/tags/{id}/set</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
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
        /// <param type="System.Int32, System" method="url" name="id">Tag ID</param>
        /// <param type="System.Collections.Generic.List{System.Int32}, System.Collections.Generic" name="messages">List of message IDs</param>
        /// <returns>Removed mail tag ID</returns>
        /// <short>Remove a tag from the messages</short> 
        /// <category>Tags</category>
        /// <path>api/2.0/mail/tags/{id}/unset</path>
        /// <httpMethod>PUT</httpMethod>
        /// <exception cref="ArgumentException">An exception occurs when the parameters are invalid. The text description contains the parameter name and the text description.</exception>
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
