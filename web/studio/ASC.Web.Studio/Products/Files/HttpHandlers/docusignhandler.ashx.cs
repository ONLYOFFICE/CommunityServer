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
using System.Net;
using System.Web;
using System.Xml;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Resources;
using ASC.Web.Files.Services.NotifyService;
using ASC.Web.Studio.UserControls.Statistics;

namespace ASC.Web.Files.HttpHandlers
{
    public class DocuSignHandler : IHttpHandler
    {
        public static string Path
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "httphandlers/docusignhandler.ashx"; }
        }

        private static ILog Log
        {
            get { return Global.Logger; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (TenantStatisticsProvider.IsNotPaid())
            {
                context.Response.StatusCode = (int) HttpStatusCode.PaymentRequired;
                context.Response.StatusDescription = "Payment Required.";
                return;
            }

            try
            {
                switch ((context.Request[FilesLinkUtility.Action] ?? "").ToLower())
                {
                    case "redirect":
                        Redirect(context);
                        break;
                    case "webhook":
                        Webhook(context);
                        break;
                    default:
                        throw new HttpException((int) HttpStatusCode.BadRequest, FilesCommonResource.ErrorMassage_BadRequest);
                }
            }
            catch (InvalidOperationException e)
            {
                throw new HttpException((int) HttpStatusCode.InternalServerError, FilesCommonResource.ErrorMassage_BadRequest, e);
            }
        }

        private static void Redirect(HttpContext context)
        {
            Log.Info("DocuSign redirect query: " + context.Request.QueryString);

            var eventRedirect = context.Request["event"];
            switch (eventRedirect.ToLower())
            {
                case "send":
                    context.Response.Redirect(PathProvider.StartURL + "#message/" + HttpUtility.UrlEncode(FilesCommonResource.DocuSignStatusSended), true);
                    break;
                case "save":
                case "cancel":
                    context.Response.Redirect(PathProvider.StartURL + "#error/" + HttpUtility.UrlEncode(FilesCommonResource.DocuSignStatusNotSended), true);
                    break;
                case "error":
                case "sessionend":
                    context.Response.Redirect(PathProvider.StartURL + "#error/" + HttpUtility.UrlEncode(FilesCommonResource.DocuSignStatusError), true);
                    break;
            }
            context.Response.Redirect(PathProvider.StartURL, true);
        }

        private const string XmlPrefix = "docusign";

        private static void Webhook(HttpContext context)
        {
            Global.Logger.Info("DocuSign webhook: " + context.Request.QueryString);
            try
            {
                var xmldoc = new XmlDocument();
                xmldoc.Load(context.Request.InputStream);
                Global.Logger.Info("DocuSign webhook outerXml: " + xmldoc.OuterXml);

                var mgr = new XmlNamespaceManager(xmldoc.NameTable);
                mgr.AddNamespace(XmlPrefix, "http://www.docusign.net/API/3.0");

                var envelopeStatusNode = GetSingleNode(xmldoc, "DocuSignEnvelopeInformation/" + XmlPrefix + ":EnvelopeStatus", mgr);
                var envelopeId = GetSingleNode(envelopeStatusNode, "EnvelopeID", mgr).InnerText;
                var subject = GetSingleNode(envelopeStatusNode, "Subject", mgr).InnerText;

                var statusString = GetSingleNode(envelopeStatusNode, "Status", mgr).InnerText;
                DocuSignStatus status;
                if (!Enum.TryParse(statusString, true, out status))
                {
                    throw new Exception("DocuSign webhook unknown status: " + statusString);
                }

                Global.Logger.Info("DocuSign webhook: " + envelopeId + " " + subject + " " + status);

                var customFieldUserIdNode = GetSingleNode(envelopeStatusNode, "CustomFields/" + XmlPrefix + ":CustomField[" + XmlPrefix + ":Name='" + DocuSignHelper.UserField + "']", mgr);
                var userIdString = GetSingleNode(customFieldUserIdNode, "Value", mgr).InnerText;
                Auth(userIdString);

                switch (status)
                {
                    case DocuSignStatus.Completed:

                        var documentStatuses = GetSingleNode(envelopeStatusNode, "DocumentStatuses", mgr);
                        foreach (XmlNode documentStatus in documentStatuses.ChildNodes)
                        {
                            try
                            {
                                var documentId = GetSingleNode(documentStatus, "ID", mgr).InnerText;
                                var documentName = GetSingleNode(documentStatus, "Name", mgr).InnerText;

                                string folderId = null;
                                string sourceTitle = null;

                                var documentFiels = GetSingleNode(documentStatus, "DocumentFields", mgr, true);
                                if (documentFiels != null)
                                {
                                    var documentFieldFolderNode = GetSingleNode(documentFiels, "DocumentField[" + XmlPrefix + ":Name='" + FilesLinkUtility.FolderId + "']", mgr, true);
                                    if (documentFieldFolderNode != null)
                                    {
                                        folderId = GetSingleNode(documentFieldFolderNode, "Value", mgr).InnerText;
                                    }
                                    var documentFieldTitleNode = GetSingleNode(documentFiels, "DocumentField[" + XmlPrefix + ":Name='" + FilesLinkUtility.FileTitle + "']", mgr, true);
                                    if (documentFieldTitleNode != null)
                                    {
                                        sourceTitle = GetSingleNode(documentFieldTitleNode, "Value", mgr).InnerText;
                                    }
                                }

                                var file = DocuSignHelper.SaveDocument(envelopeId, documentId, documentName, folderId);

                                NotifyClient.SendDocuSignComplete(file, sourceTitle ?? documentName);
                            }
                            catch (Exception ex)
                            {
                                Global.Logger.Error("DocuSign webhook save document: " + documentStatus.InnerText, ex);
                            }
                        }
                        break;
                    case DocuSignStatus.Declined:
                    case DocuSignStatus.Voided:
                        var statusFromResource = status == DocuSignStatus.Declined
                                                     ? FilesCommonResource.DocuSignStatusDeclined
                                                     : FilesCommonResource.DocuSignStatusVoided;
                        NotifyClient.SendDocuSignStatus(subject, statusFromResource);
                        break;
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocuSign webhook", e);

                throw new HttpException((int) HttpStatusCode.BadRequest, e.Message);
            }
        }

        private static void Auth(string userIdString)
        {
            Guid userId;
            if (!Guid.TryParse(userIdString ?? "", out userId))
            {
                throw new Exception("DocuSign incorrect User ID: " + userIdString);
            }

            SecurityContext.AuthenticateMe(userId);
        }

        private static XmlNode GetSingleNode(XmlNode node, string xpath, XmlNamespaceManager mgr, bool canMiss = false)
        {
            var result = node.SelectSingleNode(XmlPrefix + ":" + xpath, mgr);
            if (!canMiss && result == null) throw new Exception(xpath + " is null");
            return result;
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}