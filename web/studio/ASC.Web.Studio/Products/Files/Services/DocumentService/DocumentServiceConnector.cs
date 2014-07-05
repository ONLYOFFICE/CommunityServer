/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.IO;
using System.Web;
using System.Web.Configuration;
using ASC.Web.Core.Files;
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
            catch (Exception e)
            {
                throw CustomizeError(e.Message);
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
            catch (Exception e)
            {
                throw CustomizeError(e.Message);
            }
        }

        public static string CommandRequest(string method,
                                            string documentRevisionId,
                                            string callbackUrl,
                                            string userId)
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
                    userId);
            }
            catch (Exception e)
            {
                throw CustomizeError(e.Message);
            }
        }

        private static Exception CustomizeError(string errorMessage)
        {
            var error = FilesCommonResource.ErrorMassage_DocServiceException;
            if (!string.IsNullOrEmpty(errorMessage))
                error += string.Format(" ({0})", errorMessage);

            return new Exception(error);
        }
    }
}