/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
