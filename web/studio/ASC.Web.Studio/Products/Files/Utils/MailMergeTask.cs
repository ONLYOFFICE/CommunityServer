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

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ASC.Api;
using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;

namespace ASC.Web.Files.Utils
{
    public class MailMergeTask : IDisposable
    {
        private const string MessageBodyFormat = "id={0}&from={1}&subject={2}&to%5B%5D={3}&body={4}&mimeReplyToId=";

        public string From;
        public string Subject;
        public string To;
        public string Message;
        public string AttachTitle;
        public Stream Attach;
        public int MessageId;
        public string StreamId;

        private ApiServer _apiServer;

        protected ApiServer Api
        {
            get { return _apiServer ?? (_apiServer = new ApiServer()); }
        }


        public MailMergeTask()
        {
            MessageId = 0;
        }

        public string Run()
        {
            if (string.IsNullOrEmpty(From)) throw new ArgumentException("From is null");
            if (string.IsNullOrEmpty(To)) throw new ArgumentException("To is null");

            CreateDraftMail();

            var bodySendAttach = AttachToMail();

            return SendMail(bodySendAttach);
        }

        private void CreateDraftMail()
        {
            var apiUrlCreate = String.Format("{0}mail/messages/save.json", SetupInfo.WebApiBaseUrl);
            var bodyCreate =
                string.Format(
                    MessageBodyFormat,
                    MessageId,
                    HttpUtility.UrlEncode(From),
                    HttpUtility.UrlEncode(Subject),
                    HttpUtility.UrlEncode(To),
                    HttpUtility.UrlEncode(Message));

            var responseCreateString = Encoding.UTF8.GetString(Convert.FromBase64String(Api.GetApiResponse(apiUrlCreate, "PUT", bodyCreate)));
            var responseCreate = JObject.Parse(responseCreateString);

            if (responseCreate["statusCode"].Value<int>() != (int) HttpStatusCode.OK)
            {
                throw new Exception("Create draft failed: " + responseCreate["error"]["message"].Value<string>());
            }

            MessageId = responseCreate["response"]["id"].Value<int>();
            StreamId = responseCreate["response"]["streamId"].Value<string>();
        }

        private string AttachToMail()
        {
            if (Attach == null) return string.Empty;

            if (string.IsNullOrEmpty(AttachTitle)) AttachTitle = "attach.pdf";

            var apiUrlAttach = string.Format("{0}mail/messages/attachment/add?id_message={1}&name={2}",
                                             SetupInfo.WebApiBaseUrl,
                                             MessageId,
                                             AttachTitle);

            var request = (HttpWebRequest) WebRequest.Create(CommonLinkUtility.GetFullAbsolutePath(apiUrlAttach));
            request.Method = "POST";
            request.ContentType = MimeMapping.GetMimeMapping(AttachTitle);
            request.ContentLength = Attach.Length;
            request.Headers.Add("Authorization", SecurityContext.AuthenticateMe(SecurityContext.CurrentAccount.ID));

            const int bufferSize = 2048;
            var buffer = new byte[bufferSize];
            int readed;
            while ((readed = Attach.Read(buffer, 0, bufferSize)) > 0)
            {
                request.GetRequestStream().Write(buffer, 0, readed);
            }

            // hack. http://ubuntuforums.org/showthread.php?t=1841740
            if (WorkContext.IsMono)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            }

            string responseAttachString;
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) throw new WebException("Could not get an answer");
                using (var reader = new StreamReader(stream))
                {
                    responseAttachString = reader.ReadToEnd();
                }
            }

            var responseAttach = JObject.Parse(responseAttachString);

            if (responseAttach["statusCode"].Value<int>() != (int) HttpStatusCode.Created)
            {
                throw new Exception("Attach failed: " + responseAttach["error"]["message"].Value<string>());
            }

            var bodySendAttach =
                "&attachments%5B0%5D%5BfileId%5D=" + HttpUtility.UrlEncode(responseAttach["response"]["fileId"].Value<string>())
                + "&attachments%5B0%5D%5BfileName%5D=" + HttpUtility.UrlEncode(responseAttach["response"]["fileName"].Value<string>())
                + "&attachments%5B0%5D%5Bsize%5D=" + HttpUtility.UrlEncode(responseAttach["response"]["size"].Value<string>())
                + "&attachments%5B0%5D%5BcontentType%5D=" + HttpUtility.UrlEncode(responseAttach["response"]["contentType"].Value<string>())
                + "&attachments%5B0%5D%5BfileNumber%5D=" + HttpUtility.UrlEncode(responseAttach["response"]["fileNumber"].Value<string>())
                + "&attachments%5B0%5D%5BstoredName%5D=" + HttpUtility.UrlEncode(responseAttach["response"]["storedName"].Value<string>())
                + "&attachments%5B0%5D%5BstreamId%5D=" + HttpUtility.UrlEncode(responseAttach["response"]["streamId"].Value<string>())
                ;

            return bodySendAttach;
        }

        private string SendMail(string bodySendAttach)
        {
            var apiUrlSend = String.Format("{0}mail/messages/send.json", SetupInfo.WebApiBaseUrl);

            var bodySend =
                string.Format(
                    MessageBodyFormat,
                    MessageId,
                    HttpUtility.UrlEncode(From),
                    HttpUtility.UrlEncode(Subject),
                    HttpUtility.UrlEncode(To),
                    HttpUtility.UrlEncode(Message));

            bodySend += bodySendAttach;

            var responseSendString = Encoding.UTF8.GetString(Convert.FromBase64String(Api.GetApiResponse(apiUrlSend, "PUT", bodySend)));
            var responseSend = JObject.Parse(responseSendString);

            if (responseSend["statusCode"].Value<int>() != (int)HttpStatusCode.OK)
            {
                throw new Exception("Create draft failed: " + responseSend["error"]["message"].Value<string>());
            }
            return responseSend["response"].Value<string>();
        }

        public void Dispose()
        {
            if (Attach != null)
                Attach.Dispose();
        }
    }
}