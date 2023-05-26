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
using System.Globalization;

using ASC.Core.Tenants;
using ASC.Security.Cryptography;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Utils;

using Newtonsoft.Json;

namespace ASC.Files.Core.Security
{
    public class FileShareOptions
    {
        public string Title { get; set; }

        public string Password { get; set; }

        public DateTime ExpirationDate { get; set; }

        public bool AutoDelete { get; set; }


        public string GetExpirationDateStr()
        {
            return ExpirationDate == DateTime.MinValue ? null : ExpirationDate.ToString("s", CultureInfo.InvariantCulture);
        }

        public string GetPasswordKey()
        {
            return string.IsNullOrEmpty(Password) ? null : FileShareLink.CreateKey(Password);
        }

        public bool IsExpired()
        {
            return ExpirationDate != DateTime.MinValue && ExpirationDate < TenantUtil.DateTimeNow();
        }

        public static string Serialize(FileShareOptions value)
        {
            try
            {
                if (value == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(value.Password))
                {
                    value.Password = InstanceCrypto.Encrypt(value.Password);
                }

                return JsonConvert.SerializeObject(value);
            }
            catch (Exception e)
            {
                Global.Logger.Error("Error serialize ShareOptions", e);
                return null;
            }
        }

        public static FileShareOptions Deserialize(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                var result = JsonConvert.DeserializeObject<FileShareOptions>(value);

                if (!string.IsNullOrEmpty(result.Password))
                {
                    result.Password = InstanceCrypto.Decrypt(result.Password);
                }

                return result;
            }
            catch (Exception e)
            {
                Global.Logger.Error("Error parse ShareOptions: " + value, e);
                return null;
            }
        }
    }
}
