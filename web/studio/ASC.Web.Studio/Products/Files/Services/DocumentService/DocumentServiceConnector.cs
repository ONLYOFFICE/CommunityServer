/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json;
using CommandMethod = ASC.Web.Core.Files.DocumentService.CommandMethod;

namespace ASC.Web.Files.Services.DocumentService
{
    public static class DocumentServiceConnector
    {
        public static string GenerateRevisionId(string expectedKey)
        {
            return Web.Core.Files.DocumentService.GenerateRevisionId(expectedKey);
        }

        public static int GetConvertedUri(string documentUri,
                                          string fromExtension,
                                          string toExtension,
                                          string documentRevisionId,
                                          string password,
                                          bool isAsync,
                                          out string convertedDocumentUri)
        {
            Global.Logger.DebugFormat("DocService convert from {0} to {1} - {2}", fromExtension, toExtension, documentUri);
            try
            {
                return Web.Core.Files.DocumentService.GetConvertedUri(
                    FilesLinkUtility.DocServiceConverterUrl,
                    documentUri,
                    fromExtension,
                    toExtension,
                    GenerateRevisionId(documentRevisionId),
                    password,
                    isAsync,
                    FileUtility.SignatureSecret,
                    out convertedDocumentUri);
            }
            catch (Exception ex)
            {
                throw CustomizeError(ex);
            }
        }

        public static bool Command(CommandMethod method,
                                   string docKeyForTrack,
                                   object fileId = null,
                                   string callbackUrl = null,
                                   string[] users = null,
                                   Web.Core.Files.DocumentService.MetaData meta = null)
        {
            Global.Logger.DebugFormat("DocService command {0} fileId '{1}' docKey '{2}' callbackUrl '{3}' users '{4}' meta '{5}'", method, fileId, docKeyForTrack, callbackUrl, users != null ? string.Join(", ", users) : null, JsonConvert.SerializeObject(meta));
            try
            {
                string version;
                var result = Web.Core.Files.DocumentService.CommandRequest(
                    FilesLinkUtility.DocServiceCommandUrl,
                    method,
                    GenerateRevisionId(docKeyForTrack),
                    callbackUrl,
                    users,
                    meta,
                    FileUtility.SignatureSecret,
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

        public static string DocbuilderRequest(string requestKey,
                                               string inputScript,
                                               bool isAsync,
                                               out Dictionary<string, string> urls)
        {
            string scriptUrl = null;
            if (!string.IsNullOrEmpty(inputScript))
            {
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(inputScript);
                    writer.Flush();
                    stream.Position = 0;
                    scriptUrl = PathProvider.GetTempUrl(stream, ".docbuilder");
                }
                scriptUrl = ReplaceCommunityAdress(scriptUrl);
                requestKey = null;
            }

            Global.Logger.DebugFormat("DocService builder requestKey {0} async {1}", requestKey, isAsync);
            try
            {
                return Web.Core.Files.DocumentService.DocbuilderRequest(
                    FilesLinkUtility.DocServiceDocbuilderUrl,
                    GenerateRevisionId(requestKey),
                    scriptUrl,
                    isAsync,
                    FileUtility.SignatureSecret,
                    out urls);
            }
            catch (Exception ex)
            {
                throw CustomizeError(ex);
            }
        }

        public static string GetVersion()
        {
            Global.Logger.DebugFormat("DocService request version");
            try
            {
                string version;
                var result = Web.Core.Files.DocumentService.CommandRequest(
                    FilesLinkUtility.DocServiceCommandUrl,
                    CommandMethod.Version,
                    GenerateRevisionId(null),
                    null,
                    null,
                    null,
                    FileUtility.SignatureSecret,
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

        public static void CheckDocServiceUrl()
        {
            if (!string.IsNullOrEmpty(FilesLinkUtility.DocServiceHealthcheckUrl))
            {
                try
                {
                    if (!Web.Core.Files.DocumentService.HealthcheckRequest(FilesLinkUtility.DocServiceHealthcheckUrl))
                    {
                        throw new Exception("bad status");
                    }
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("Healthcheck DocService check error", ex);
                    throw new Exception("Healthcheck url: " + ex.Message);
                }
            }

            if (!string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl))
            {
                string convertedFileUri;
                try
                {
                    const string fileExtension = ".docx";
                    var toExtension = FileUtility.GetInternalExtension(fileExtension);
                    var url = PathProvider.GetEmptyFileUrl(fileExtension);

                    var fileUri = ReplaceCommunityAdress(url);

                    var key = GenerateRevisionId(Guid.NewGuid().ToString());
                    Web.Core.Files.DocumentService.GetConvertedUri(FilesLinkUtility.DocServiceConverterUrl, fileUri, fileExtension, toExtension, key, null, false, FileUtility.SignatureSecret, out convertedFileUri);
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("Converter DocService check error", ex);
                    throw new Exception("Converter url: " + ex.Message);
                }

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(convertedFileUri);
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception("Converted url is not available");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("Document DocService check error", ex);
                    throw new Exception("Document server: " + ex.Message);
                }
            }

            if (!string.IsNullOrEmpty(FilesLinkUtility.DocServiceCommandUrl))
            {
                try
                {
                    var key = GenerateRevisionId(Guid.NewGuid().ToString());
                    string version;
                    Web.Core.Files.DocumentService.CommandRequest(FilesLinkUtility.DocServiceCommandUrl, CommandMethod.Version, key, null, null, null, FileUtility.SignatureSecret, out version);
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("Command DocService check error", ex);
                    throw new Exception("Command url: " + ex.Message);
                }
            }

            if (!string.IsNullOrEmpty(FilesLinkUtility.DocServiceDocbuilderUrl))
            {
                try
                {
                    var storeTemplate = Global.GetStoreTemplate();
                    var scriptUri = storeTemplate.GetUri("", "test.docbuilder");
                    var scriptUrl = CommonLinkUtility.GetFullAbsolutePath(scriptUri.ToString());
                    scriptUrl = ReplaceCommunityAdress(scriptUrl);

                    Dictionary<string, string> urls;
                    Web.Core.Files.DocumentService.DocbuilderRequest(FilesLinkUtility.DocServiceDocbuilderUrl, null, scriptUrl, false, FileUtility.SignatureSecret, out urls);
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("DocService check error", ex);
                    throw new Exception("Docbuilder url: " + ex.Message);
                }
            }
        }

        public static string ReplaceCommunityAdress(string url)
        {
            var docServicePortalUrl = FilesLinkUtility.DocServicePortalUrl;

            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            if (string.IsNullOrEmpty(docServicePortalUrl))
            {
                Tenant tenant;
                if (!TenantExtra.Saas
                    || string.IsNullOrEmpty((tenant = CoreContext.TenantManager.GetCurrentTenant()).MappedDomain)
                    || !url.StartsWith("https://" + tenant.MappedDomain))
                {
                    return url;
                }

                docServicePortalUrl = "https://" + tenant.GetTenantDomain(false);
            }

            var uri = new UriBuilder(url);
            if (new UriBuilder(CommonLinkUtility.ServerRootPath).Host != uri.Host)
            {
                return url;
            }

            var urlRewriterQuery = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;
            var query = HttpUtility.ParseQueryString(uri.Query);
            query[HttpRequestExtensions.UrlRewriterHeader] = urlRewriterQuery;
            uri.Query = query.ToString();

            var communityUrl = new UriBuilder(docServicePortalUrl);
            uri.Scheme = communityUrl.Scheme;
            uri.Host = communityUrl.Host;
            uri.Port = communityUrl.Port;

            return uri.ToString();
        }

        public static string ReplaceDocumentAdress(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            var uri = new UriBuilder(url).ToString();
            var externalUri = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.DocServiceUrl)).ToString();
            var internalUri = new UriBuilder(CommonLinkUtility.GetFullAbsolutePath(FilesLinkUtility.DocServiceUrlInternal)).ToString();
            if (uri.StartsWith(internalUri, true, CultureInfo.InvariantCulture) || !uri.StartsWith(externalUri, true, CultureInfo.InvariantCulture))
            {
                return url;
            }

            uri = uri.Replace(externalUri, FilesLinkUtility.DocServiceUrlInternal);

            return uri;
        }

        private static Exception CustomizeError(Exception ex)
        {
            var error = FilesCommonResource.ErrorMassage_DocServiceException;
            if (!string.IsNullOrEmpty(ex.Message))
                error += string.Format(" ({0})", ex.Message);

            Global.Logger.Error("DocService error", ex);
            return new Exception(error, ex);
        }
    }
}