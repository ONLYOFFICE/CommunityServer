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
using System.Web;
using System.Web.Configuration;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using CommandMethod = ASC.Web.Core.Files.DocumentService.CommandMethod;

namespace ASC.Web.Files.Services.DocumentService
{
    public static class DocumentServiceConnector
    {
        private static Web.Core.Files.DocumentService GetDocumentService()
        {
            int timeout;
            Int32.TryParse(WebConfigurationManager.AppSettings["files.docservice.timeout"], out timeout);

            var documentService = new Web.Core.Files.DocumentService(
                StudioKeySettings.GetKey(),
                StudioKeySettings.GetSKey(),
                TenantStatisticsProvider.GetUsersCount());
            if (timeout > 0)
            {
                documentService.Timeout = timeout;
            }
            return documentService;
        }

        public static string GenerateRevisionId(string expectedKey)
        {
            return Web.Core.Files.DocumentService.GenerateRevisionId(expectedKey);
        }

        public static string GenerateValidateKey(string documentRevisionId)
        {
            var docServiceConnector = new Web.Core.Files.DocumentService(
                StudioKeySettings.GetKey(),
                StudioKeySettings.GetSKey(),
                TenantStatisticsProvider.GetUsersCount());

            string userIp = null;
            try
            {
                if (HttpContext.Current != null) userIp = HttpContext.Current.Request.UserHostAddress;
            }
            catch
            {
                userIp = string.Empty;
            }

            return docServiceConnector.GenerateValidateKey(GenerateRevisionId(documentRevisionId), userIp);
        }

        public static int GetConvertedUri(string documentUri,
                                          string fromExtension,
                                          string toExtension,
                                          string documentRevisionId,
                                          bool isAsync,
                                          out string convertedDocumentUri)
        {
            Global.Logger.DebugFormat("DocService convert from {0} to {1} - {2}", fromExtension, toExtension, documentUri);
            try
            {
                return GetDocumentService().GetConvertedUri(
                    FilesLinkUtility.DocServiceConverterUrl,
                    documentUri,
                    fromExtension,
                    toExtension,
                    GenerateRevisionId(documentRevisionId),
                    isAsync,
                    out convertedDocumentUri);
            }
            catch (Exception ex)
            {
                throw CustomizeError(ex);
            }
        }

        public static string GetExternalUri(Stream fileStream, string contentType, string documentRevisionId)
        {
            try
            {
                documentRevisionId = GenerateRevisionId(documentRevisionId);
                var response = GetDocumentService().GetExternalUri(
                    FilesLinkUtility.DocServiceStorageUrl,
                    fileStream,
                    contentType,
                    documentRevisionId);
                Global.Logger.Info("DocService GetExternalUri for " + documentRevisionId + " response " + response);
                return response;
            }
            catch (Exception ex)
            {
                throw CustomizeError(ex);
            }
        }

        public static bool Command(CommandMethod method, string docKeyForTrack, object fileId = null, string callbackUrl = null, string users = null, string status = null)
        {
            Global.Logger.DebugFormat("DocService command {0} fileId '{1}' docKey '{2}' callbackUrl '{3}' users '{4}' status '{5}'", method, fileId, docKeyForTrack, callbackUrl, users, status);
            try
            {
                string version;
                var result = GetDocumentService().CommandRequest(
                    FilesLinkUtility.DocServiceCommandUrl,
                    method,
                    GenerateRevisionId(docKeyForTrack),
                    callbackUrl,
                    users,
                    status,
                    out version);

                if (result == Web.Core.Files.DocumentService.CommandResultTypes.NoError)
                {
                    return true;
                }

                Global.Logger.ErrorFormat("DocService command response: '{0}'", result);
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocService command error", e);
            }
            return false;
        }

        public static string GetVersion()
        {
            Global.Logger.DebugFormat("DocService request version"); 
            try
            {
                string version;
                var result = GetDocumentService().CommandRequest(
                    FilesLinkUtility.DocServiceCommandUrl,
                    CommandMethod.Version,
                    GenerateRevisionId(null),
                    null,
                    null,
                    null,
                    out version);

                if (result == Web.Core.Files.DocumentService.CommandResultTypes.NoError)
                {
                    return version;
                }

                Global.Logger.ErrorFormat("DocService command response: '{0}'", result);
            }
            catch (Exception e)
            {
                Global.Logger.Error("DocService command error", e);
            }
            return "4.1.5.1";
        }

        public static bool CheckDocServiceUrl(string docServiceUrlCommand, string docServiceUrlStorage, string docServiceUrlConverter, string docServiceUrlPortal)
        {
            var documentService = GetDocumentService();

            var key = GenerateRevisionId(Guid.NewGuid().ToString());
            var fileUri = string.Empty;
            const string toExtension = ".docx";
            var fileExtension = FileUtility.GetInternalExtension(toExtension);
            var path = FileConstant.NewDocPath + "default/new" + fileExtension;

            try
            {
                var storeTemplate = Global.GetStoreTemplate();
                using (var stream = storeTemplate.GetReadStream("", path))
                {
                    fileUri = documentService.GetExternalUri(docServiceUrlStorage, stream, MimeMapping.GetMimeMapping(toExtension), key);
                }
            }
            catch (Exception ex)
            {
                Global.Logger.Error("DocService check error", ex);
                throw new Exception("Storage url: " + ex.Message);
            }

            try
            {
                string tmp;
                documentService.GetConvertedUri(docServiceUrlConverter, fileUri, fileExtension, toExtension, key, false, out tmp);
            }
            catch (Exception ex)
            {
                Global.Logger.Error("DocService check error", ex);
                throw new Exception("Converter url: " + ex.Message);
            }

            try
            {
                var storeTemplate = Global.GetStoreTemplate();
                var uri = storeTemplate.GetUri("", path);
                var url = CommonLinkUtility.GetFullAbsolutePath(uri.ToString());

                fileUri = ReplaceCommunityAdress(url, docServiceUrlPortal);

                key = GenerateRevisionId(Guid.NewGuid().ToString());
                string tmp;
                documentService.GetConvertedUri(docServiceUrlConverter, fileUri, fileExtension, toExtension, key, false, out tmp);
            }
            catch (Exception ex)
            {
                Global.Logger.Error("DocService check error", ex);
                throw new Exception("Community server url: " + ex.Message);
            }

            try
            {
                string version;
                var response = documentService.CommandRequest(docServiceUrlCommand, CommandMethod.Version, key, null, null, null, out version);

                return response == Web.Core.Files.DocumentService.CommandResultTypes.NoError
                       || response == Web.Core.Files.DocumentService.CommandResultTypes.CommandError;
            }
            catch (Exception ex)
            {
                Global.Logger.Error("DocService check error", ex);
                throw new Exception("Command url: " + ex.Message);
            }
        }

        public static string ReplaceCommunityAdress(string url)
        {
            return ReplaceCommunityAdress(url, FilesLinkUtility.DocServicePortalUrl);
        }

        private static string ReplaceCommunityAdress(string url, string docServicePortalUrl)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(docServicePortalUrl))
            {
                return url;
            }

            var uri = new UriBuilder(url);
            if (new UriBuilder(CommonLinkUtility.ServerRootPath).Host != uri.Host)
            {
                return url;
            }

            var communityUrl = new UriBuilder(docServicePortalUrl);
            uri.Host = communityUrl.Host;
            uri.Scheme = communityUrl.Scheme;
            uri.Port = communityUrl.Port;

            return uri.ToString();
        }

        private static Exception CustomizeError(Exception ex)
        {
            var error = FilesCommonResource.ErrorMassage_DocServiceException;
            if (!string.IsNullOrEmpty(ex.Message))
                error += string.Format(" ({0})", ex.Message);

            Global.Logger.Error("DocService error", ex);
            return new Exception(error);
        }
    }
}