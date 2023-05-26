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


#region Import

using System;
using System.IO;
using System.Web;

using ASC.Common.Logging;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.Web.CRM.HttpHandlers
{
    public class FileHandler : IHttpHandler
    {
        private ILog Log = LogManager.GetLogger("ASC");

        public void ProcessRequest(HttpContext context)
        {
            var action = context.Request["action"];

            switch (action)
            {
                case "contactphotoulr":
                    ResponceContactPhotoUrl(context);
                    break;
                case "mailmessage":
                    ResponceMailMessageContent(context);
                    break;
                default:
                    throw new ArgumentException(String.Format("action='{0}' is not defined", action));
            }
        }

        private void ResponceContactPhotoUrl(HttpContext context)
        {
            var contactId = Convert.ToInt32(context.Request["cid"]);
            var isCompany = Convert.ToBoolean(context.Request["isc"]);
            var photoSize = Convert.ToInt32(context.Request["ps"]);

            String photoUrl = String.Empty;

            switch (photoSize)
            {
                case 1:
                    photoUrl = ContactPhotoManager.GetSmallSizePhoto(contactId, isCompany);
                    break;
                case 2:
                    photoUrl = ContactPhotoManager.GetMediumSizePhoto(contactId, isCompany);
                    break;
                case 3:
                    photoUrl = ContactPhotoManager.GetBigSizePhoto(contactId, isCompany);
                    break;
                default:
                    throw new Exception(CRMErrorsResource.ContactPhotoSizeUnknown);
            }

            context.Response.Clear();
            context.Response.Write(photoUrl);

            try
            {
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException ex)
            {
                Log.Error("ResponceContactPhotoUrl", ex);
            }
        }

        private void ResponceMailMessageContent(HttpContext context)
        {
            var messageId = Convert.ToInt32(context.Request["message_id"]);

            var filePath = String.Format("folder_{0}/message_{1}.html", (messageId / 1000 + 1) * 1000, messageId);

            String messageContent = String.Empty;

            using (var streamReader = new StreamReader(Global.GetStore().GetReadStream("mail_messages", filePath)))
            {
                messageContent = streamReader.ReadToEnd();
            }


            context.Response.Clear();
            context.Response.Write(messageContent);
            try
            {
                context.Response.Flush();
                context.Response.SuppressContent = true;
                context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException ex)
            {
                Log.Error("ResponceMailMessageContent", ex);
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}