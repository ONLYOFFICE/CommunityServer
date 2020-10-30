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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Configuration;
using ASC.Data.Storage;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ASC.Web.Studio.Utility
{
    public class WhiteLabelHelper
    {
        private readonly static ILog Log = LogManager.GetLogger("ASC");

        private const string Base64StartPng = "data:image/png;base64,";

        private const string PngExt = ".png";

        private const string JsonDataFilePath = "json-data.txt";

        private const string LogoPattern = "logo_*";

        public static void ApplyPartnerWhiteLableSettings()
        {
            if (!TenantExtra.Enterprise && !CoreContext.Configuration.CustomMode) return;

            var firstVisit = CompanyWhiteLabelSettings.Instance.IsDefault &&
                             AdditionalWhiteLabelSettings.Instance.IsDefault &&
                             MailWhiteLabelSettings.Instance.IsDefault;

            try
            {
                var partnerdataStorage = StorageFactory.GetStorage(string.Empty, "static_partnerdata");

                if (partnerdataStorage == null) return;

                if (!partnerdataStorage.IsFile(JsonDataFilePath)) return;

                JObject jsonObject;

                using (var stream = partnerdataStorage.GetReadStream(JsonDataFilePath))
                using (var reader = new StreamReader(stream))
                {
                    jsonObject = JObject.Parse(reader.ReadToEnd());
                }

                if(jsonObject == null) return;

                SaveSettings<CompanyWhiteLabelSettings>(jsonObject, "CompanyWhiteLabelSettings");
                SaveSettings<AdditionalWhiteLabelSettings>(jsonObject, "AdditionalWhiteLabelSettings");
                SaveSettings<MailWhiteLabelSettings>(jsonObject, "MailWhiteLabelSettings");
                SaveSettings<TenantWhiteLabelSettings>(jsonObject, "TenantWhiteLabelSettings");

                var smtpSettingsStr = (jsonObject["SmtpSettings"] ?? "").ToString();
                var defaultCultureName = (jsonObject["DefaultCulture"] ?? "").ToString();

                if (!String.IsNullOrEmpty(smtpSettingsStr))
                {
                    try
                    {
                        SmtpSettings.Deserialize(smtpSettingsStr);
                        CoreContext.Configuration.SaveSetting("SmtpSettings", smtpSettingsStr);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message, e);
                    }
                }

                if (!String.IsNullOrEmpty(defaultCultureName))
                {
                    var defaultCulture = CultureInfo.GetCultureInfo(defaultCultureName);

                    if (SetupInfo.EnabledCultures.Find(culture => String.Equals(culture.Name, defaultCulture.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
                    {
                        var tenant = CoreContext.TenantManager.GetCurrentTenant();
                        tenant.Language = defaultCulture.Name;
                        CoreContext.TenantManager.SaveTenant(tenant);
                    }
                }

                var logoImages = partnerdataStorage.ListFiles(String.Empty, LogoPattern, false);

                if (!logoImages.Any())
                {
                    MakeLogoFiles(partnerdataStorage, jsonObject);
                }

                if (!firstVisit) return;

                var tenantInfoSettings = TenantInfoSettings.Load();
                tenantInfoSettings.RestoreDefaultTenantName();
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
        }

        private static void SaveSettings<T>(JObject jsonObject, string prop) where T : class, ISettings
        {
            var jsonObjectToken = jsonObject[prop];

            if (jsonObjectToken == null) return;

            var settings = JsonConvert.DeserializeObject<T>(jsonObjectToken.ToString()) as BaseSettings<T>;

            if (settings == null) return;

            settings.SaveForDefaultTenant();
        }

        private static void MakeLogoFiles(IDataStore store, JObject jObject)
        {
            var settings = jObject["WhiteLabelLogos"];

            if (settings == null) return;

            var propNames = new[]
                {
                    "LogoDarkGeneral",
                    "LogoDark",
                    "LogoDocseditorGeneral",
                    "LogoDocseditor",
                    "LogoFaviconGeneral",
                    "LogoFavicon",
                    "LogoLightSmallGeneral",
                    "LogoLightSmall"
                };

            foreach (var prop in propNames)
            {
                MakeLogoFile(store, prop, settings[prop].ToString());
            }
        }

        private static void MakeLogoFile(IDataStore store, String prop, String value)
        {
            if(String.IsNullOrEmpty(value)) return;
            
            var fileName = prop.Replace("Logo", "logo_").Replace("General", "_general").ToLower() + PngExt;

            Uri validUri;
            var isValidUri = Uri.TryCreate(value, UriKind.Absolute, out validUri) && (validUri.Scheme == Uri.UriSchemeHttp || validUri.Scheme == Uri.UriSchemeHttps);

            if (isValidUri)
            {
                var request = WebRequest.Create(validUri);

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    store.Save(fileName, stream);
                }
            }
            else
            {
                if (!value.StartsWith(Base64StartPng)) return;

                var bytes = Convert.FromBase64String(value.Substring(Base64StartPng.Length));

                using (var stream = new MemoryStream(bytes))
                {
                    store.Save(fileName, stream);
                }
            }
        }
    }
}
