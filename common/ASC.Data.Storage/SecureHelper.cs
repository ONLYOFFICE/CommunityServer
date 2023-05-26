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
using System.IO;
using System.Web;

using ASC.Common.Logging;
using ASC.Security.Cryptography;

namespace ASC.Data.Storage
{
    public static class SecureHelper
    {
        public static bool IsSecure()
        {
            try
            {
                return HttpContext.Current != null && Uri.UriSchemeHttps.Equals(HttpContext.Current.Request.GetUrlRewriter().Scheme, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.Data.Storage.SecureHelper").Error(err);
                return false;
            }
        }

        public static string GenerateSecureKeyHeader(string path)
        {
            var ticks = DateTime.UtcNow.Ticks;
            var data = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) + "." + ticks;
            var key = EmailValidationKeyProvider.GetEmailKey(data);

            return Constants.SECUREKEY_HEADER + ":" + ticks + "-" + key;
        }

        public static bool CheckSecureKeyHeader(string header, string path)
        {
            if (string.IsNullOrEmpty(header))
            {
                return false;
            }

            header = header.Replace(Constants.SECUREKEY_HEADER + ":", string.Empty);

            var separatorPosition = header.IndexOf('-');
            var ticks = header.Substring(0, separatorPosition);
            var key = header.Substring(separatorPosition + 1);

            var validateResult = EmailValidationKeyProvider.ValidateEmailKey(path + "." + ticks, key);

            return validateResult == EmailValidationKeyProvider.ValidationResult.Ok;
        }
    }
}