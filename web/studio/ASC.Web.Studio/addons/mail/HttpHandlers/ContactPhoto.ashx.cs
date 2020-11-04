/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Web;
using System.Web.Services;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Data.Storage;
using ASC.Web.Mail.Resources;

namespace ASC.Web.Mail.HttpHandlers
{

    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class ContactPhotoHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var log = LogManager.GetLogger("ASC.Mail.ContactPhotoHandler");

            try
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    throw new HttpException(403, "Access denied.");
                }

                var contactId = Convert.ToInt32(context.Request.QueryString["cid"]);
                var photoSize = Convert.ToInt32(context.Request.QueryString["ps"]);

                string photoUrl;

                switch (photoSize)
                {
                    case 1:
                        photoUrl = ContactPhotoManager.GetSmallSizePhoto(contactId);
                        break;
                    case 2:
                        photoUrl = ContactPhotoManager.GetMediumSizePhoto(contactId);
                        break;
                    case 3:
                        photoUrl = ContactPhotoManager.GetBigSizePhoto(contactId);
                        break;
                    default:
                        throw new HttpException("Unknown photo size");
                }

                context.Response.Clear();
                context.Response.Write(photoUrl);
            }
            catch (HttpException he)
            {
                log.Error("ContactPhoto handler failed", he);

                context.Response.StatusCode = he.GetHttpCode();
                context.Response.Write(he.Message != null ? HttpUtility.HtmlEncode(he.Message) : MailApiErrorsResource.ErrorInternalServer);
            }
            catch (Exception ex)
            {
                log.Error("ContactPhoto handler failed", ex);

                context.Response.StatusCode = 404;
                context.Response.Redirect("404.html");
            }
            finally
            {
                try
                {
                    context.Response.Flush();
                    context.Response.SuppressContent = true;
                    context.ApplicationInstance.CompleteRequest();
                }
                catch (HttpException ex)
                {
                    LogManager.GetLogger("ASC").Error("ResponceContactPhotoUrl", ex);
                }
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