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

using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Billing;

using static ASC.Web.Core.Files.DocumentService;

namespace ASC.Web.Core.Files
{
    public static class DocumentServiceLicense
    {
        private readonly static ICache cache = AscCache.Memory;
        private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

        private static CommandResponse GetDocumentServiceLicense()
        {
            if (!CoreContext.Configuration.Standalone) return null;
            if (string.IsNullOrEmpty(FilesLinkUtility.DocServiceCommandUrl)) return null;

            var cacheKey = "DocumentServiceLicense";
            var commandResponse = cache.Get<CommandResponse>(cacheKey);
            if (commandResponse == null)
            {
                commandResponse = DocumentService.CommandRequest(
                       FilesLinkUtility.DocServiceCommandUrl,
                       DocumentService.CommandMethod.License,
                       null,
                       null,
                       null,
                       null,
                       FileUtility.SignatureSecret);
                cache.Insert(cacheKey, commandResponse, DateTime.UtcNow.Add(CACHE_EXPIRATION));
            }

            return commandResponse;
        }

        public static Dictionary<string, DateTime> GetLicenseQuota()
        {
            var commandResponse = GetDocumentServiceLicense();
            if (commandResponse == null
                || commandResponse.Quota == null
                || commandResponse.Quota.Users == null)
                return null;

            var result = new Dictionary<string, DateTime>();
            commandResponse.Quota.Users.ForEach(user => result.Add(user.UserId, user.Expire));
            return result;
        }

        public static License GetLicense()
        {
            var commandResponse = GetDocumentServiceLicense();
            if (commandResponse == null)
                return null;

            return commandResponse.License;
        }
    }
}