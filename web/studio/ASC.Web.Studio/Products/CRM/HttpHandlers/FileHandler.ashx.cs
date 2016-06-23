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


#region Import

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using System.Linq;
using ASC.Collections;
using ASC.Common.Threading.Workers;
using ASC.Data.Storage;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Configuration;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.Web.CRM.HttpHandlers
{
    public class FileHandler : IHttpHandler
    {
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
            context.Response.End();
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
            context.Response.End();
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