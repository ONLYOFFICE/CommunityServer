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


using ASC.Core.Tenants;
using ASC.Core.Users;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ASC.Core.Billing
{
    public static class LicenseClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LicenseClient));
        private const string PathRefreshLicense = "/<customer_id>/users";
        private const string PathAttrSign = "sign";
        public const string CustomerIdKey = "CustomerId";
        public const string CustomerSecretKey = "CustomerSecret";

        public static string CustomerId
        {
            get { return CoreContext.Configuration.GetSetting(CustomerIdKey); }
            private set { CoreContext.Configuration.SaveSetting(CustomerIdKey, value); }
        }

        private static string CustomerSecret
        {
            get { return CoreContext.Configuration.GetSetting(CustomerSecretKey); }
            set { CoreContext.Configuration.SaveSetting(CustomerSecretKey, value); }
        }

        private static string LicenseServerUrl
        {
            get { return ConfigurationManager.AppSettings["license.service.url"]; }
        }

        public static void SetLicenseKeys(string customerId, string customerSecret)
        {
            RefreshLicense(customerId, customerSecret);

            CustomerId = customerId;
            CustomerSecret = customerSecret;
        }

        public static void RefreshLicense(string customerId = null, string customerSecret = null)
        {
            if (string.IsNullOrEmpty(LicenseServerUrl)) return;

            if (string.IsNullOrEmpty(customerId))
            {
                var q = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
                if (!q.ControlPanel)
                {
                    return;
                }
            }

            var addrGetLicense = PathRefreshLicense.Replace("<customer_id>", customerId ?? CustomerId);

            var usersHash = CoreContext.UserManager.GetUsers(EmployeeStatus.Default, EmployeeType.User).Select(GetUserHash);

            var data = JsonConvert.SerializeObject(new { users = usersHash });

            using(var license = SendRequest(addrGetLicense, "POST", data, customerSecret))
            {
                LicenseReader.UpdateLicense(license, customerId);
            }
        }

        private static string GetUserHash(UserInfo userInfo)
        {
            return GetHash(userInfo.ID + userInfo.FirstName + userInfo.LastName);
        }

        private static Stream SendRequest(string address, string method = "GET", string data = null, string customerSecret = null)
        {
            if (string.IsNullOrEmpty(LicenseServerUrl) || string.IsNullOrEmpty(address)) throw new ArgumentNullException("address");

            var uriBuilder = new UriBuilder(LicenseServerUrl + address);
            var query = uriBuilder.Query;

            var hash = GetHash(uriBuilder.Path + (data ?? "") + (customerSecret ?? CustomerSecret));
            query += PathAttrSign + "=" + hash;

            var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri + "?" + query);
            request.Method = method;
            request.AllowAutoRedirect = false;
            request.ContentType = "application/json";
            request.Timeout = 120000;

            var bytes = Encoding.UTF8.GetBytes(data ?? "");
            if (request.Method != "GET" && bytes.Length > 0)
            {
                request.ContentLength = bytes.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            // hack. http://ubuntuforums.org/showthread.php?t=1841740
            if (WorkContext.IsMono)
            {
                ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            }

            try
            {
                using (var response = request.GetResponse())
                using (var tmp = response.GetResponseStream())
                {
                    if (tmp == null) throw new Exception("ResponseStream is null");

                    var stream = new MemoryStream();
                    tmp.CopyTo(stream);
                    return stream;
                }
            }
            catch (WebException e)
            {
                using (var response = (HttpWebResponse)e.Response)
                {
                    if (response == null)
                    {
                        Log.Error("LicenseServer request " + uriBuilder.Uri, e);
                        throw;
                    }

                    Log.ErrorFormat("LicenseServer response {0} : {1}", response.StatusCode, uriBuilder.Uri);
                    return ProcessResponceError(response);
                }
            }
        }

        private static string GetHash(string text)
        {
            return ToHexString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(text ?? "")));
        }

        private static string ToHexString(ICollection<byte> hex)
        {
            if (hex == null) return null;
            if (hex.Count == 0) return string.Empty;
            var s = new StringBuilder();
            foreach (var b in hex)
            {
                s.Append(b.ToString("x2"));
            }
            return s.ToString();
        }

        private enum ErrorCode
        {
            InternalServerError = 50000,
            InvalidParameters = 40000,
            IncorrectSignature = 40101,
            UnknownCustomerId = 40401,
            ExpiredPayment = 40201,
            NoPayments = 40202,
            EndUserQuota = 40301,
            UserHashAlreadyExist = 40302,
        }

        private static Stream ProcessResponceError(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) throw new ArgumentNullException("response");

                string body;
                using (var reader = new StreamReader(stream))
                {
                    body = reader.ReadToEnd();
                }
                Log.ErrorFormat("LicenseServer response body {0}", body);

                JObject jsonBody;
                try
                {
                    jsonBody = JObject.Parse(body);
                }
                catch (Exception e)
                {
                    Log.Error("LicenseServer response json parse", e);
                    throw new BillingNotConfiguredException(body);
                }

                var error = jsonBody.Value<string>("errorCode");
                ErrorCode errorCode;
                if (!Enum.TryParse(error, true, out errorCode))
                {
                    throw new BillingNotConfiguredException(body);
                }

                var errorDescription = jsonBody.Value<string>("description");

                switch (errorCode)
                {
                    case ErrorCode.NoPayments:
                    case ErrorCode.UnknownCustomerId:
                        throw new BillingNotFoundException(errorDescription);

                    case ErrorCode.EndUserQuota:
                    case ErrorCode.ExpiredPayment:
                        throw new BillingException(errorDescription);

                    default:
                        throw new BillingNotConfiguredException(body);
                }
            }
        }
    }
}