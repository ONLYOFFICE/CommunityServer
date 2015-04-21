/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Statistics;

namespace ASC.Web.Files.Services.DocumentService
{
    public static class DocumentServiceConnector
    {
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

            return docServiceConnector.GenerateValidateKey(documentRevisionId, userIp);
        }

        public static int GetConvertedUri(string documentUri,
                                          string fromExtension,
                                          string toExtension,
                                          string documentRevisionId,
                                          bool isAsync,
                                          out string convertedDocumentUri)
        {
            int timeout;
            Int32.TryParse(WebConfigurationManager.AppSettings["files.docservice.timeout"], out timeout);

            var docServiceConnector = new Web.Core.Files.DocumentService(
                StudioKeySettings.GetKey(),
                StudioKeySettings.GetSKey(),
                TenantStatisticsProvider.GetUsersCount());
            if (timeout > 0)
            {
                docServiceConnector.Timeout = timeout;
            }
            try
            {
                return docServiceConnector.GetConvertedUri(
                    FilesLinkUtility.DocServiceConverterUrl,
                    documentUri,
                    fromExtension,
                    toExtension,
                    documentRevisionId,
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
            int timeout;
            Int32.TryParse(WebConfigurationManager.AppSettings["files.docservice.timeout"], out timeout);

            var docServiceConnector = new Web.Core.Files.DocumentService(
                StudioKeySettings.GetKey(),
                StudioKeySettings.GetSKey(),
                TenantStatisticsProvider.GetUsersCount());
            if (timeout > 0)
            {
                docServiceConnector.Timeout = timeout;
            }
            try
            {
                return docServiceConnector.GetExternalUri(
                    FilesLinkUtility.DocServiceStorageUrl,
                    fileStream,
                    contentType,
                    documentRevisionId);
            }
            catch (Exception ex)
            {
                throw CustomizeError(ex);
            }
        }

        public static string CommandRequest(string method,
                                            string documentRevisionId,
                                            string callbackUrl,
                                            string users,
                                            string status)
        {
            int timeout;
            Int32.TryParse(WebConfigurationManager.AppSettings["files.docservice.timeout"], out timeout);

            var docServiceConnector = new Web.Core.Files.DocumentService(
                StudioKeySettings.GetKey(),
                StudioKeySettings.GetSKey(),
                TenantStatisticsProvider.GetUsersCount());
            if (timeout > 0)
            {
                docServiceConnector.Timeout = timeout;
            }
            try
            {
                return docServiceConnector.CommandRequest(
                    FilesLinkUtility.DocServiceCommandUrl,
                    method,
                    documentRevisionId,
                    callbackUrl,
                    users,
                    status);
            }
            catch (Exception e)
            {
                throw CustomizeError(e);
            }
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