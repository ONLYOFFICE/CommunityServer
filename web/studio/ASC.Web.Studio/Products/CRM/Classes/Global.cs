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


#region Import

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Data.Storage;
using ASC.Thrdparty.Configuration;
using ASC.VoipService;
using ASC.VoipService.Dao;
using ASC.VoipService.Twilio;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Web.Configuration;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ASC.Web.Core.Files;
using ASC.Web.Core;
using Newtonsoft.Json;

#endregion

namespace ASC.Web.CRM.Classes
{
    public class Global
    {
        public static readonly int EntryCountOnPage = 25;
        public static readonly int VisiblePageCount = 10;

        public static readonly int MaxCustomFieldSize = 150;
        public static readonly int MaxCustomFieldRows = 25;
        public static readonly int MaxCustomFieldCols = 150;

        public static readonly int DefaultCustomFieldSize = 40;
        public static readonly int DefaultCustomFieldRows = 2;
        public static readonly int DefaultCustomFieldCols = 40;

        public static readonly int MaxHistoryEventCharacters = 65000;

        public static DaoFactory DaoFactory
        {
            get { return new DaoFactory(TenantProvider.CurrentTenantID, CRMConstants.DatabaseId); }
        }

        public static CRMSettings TenantSettings
        {
            get { return SettingsManager.Instance.LoadSettings<CRMSettings>(TenantProvider.CurrentTenantID); }
        }

        public static IDataStore GetStore()
        {
            return StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "crm");
        }

        public static bool CanCreateProjects()
        {
            try
            {
                var apiUrl = String.Format("{0}project/securityinfo.json", SetupInfo.WebApiBaseUrl);

                var cacheKey = String.Format("{0}-{1}", ASC.Core.SecurityContext.CurrentAccount.ID, apiUrl);

                bool canCreateProject;

                if (HttpRuntime.Cache[cacheKey] != null)
                    return Convert.ToBoolean(HttpRuntime.Cache[cacheKey]);

                var apiServer = new Api.ApiServer();

                var responseApi = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(apiServer.GetApiResponse(apiUrl, "GET"))))["response"];

                if (responseApi.HasValues)
                    canCreateProject = Convert.ToBoolean(responseApi["canCreateProject"].Value<String>());
                else
                    canCreateProject = false;
                HttpRuntime.Cache.Remove(cacheKey);
                HttpRuntime.Cache.Insert(cacheKey, canCreateProject, null, System.Web.Caching.Cache.NoAbsoluteExpiration,
                                  TimeSpan.FromMinutes(5));

                return canCreateProject;

            }
            catch 
            {
                return false;
            }

        }

        public static bool CanDownloadInvoices
        {
            get
            {
                var canDownloadFiles = false;

                var value = WebConfigurationManager.AppSettings["crm.invoice.download.enable"];
                if (string.IsNullOrEmpty(value)) return false;

                canDownloadFiles = Convert.ToBoolean(value);

                if (canDownloadFiles && (string.IsNullOrEmpty(FilesLinkUtility.DocServiceStorageUrl) || string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl)))
                {
                    canDownloadFiles = false;
                }

                return canDownloadFiles;
            }
        }

        #region CRM Settings

        public static void SaveSMTPSettings(string host, int port, bool authentication, string hostLogin, string hostPassword, string senderDisplayName, string senderEmailAddress, bool enableSSL)
        {
            var crmSettings = Global.TenantSettings;

            crmSettings.SMTPServerSetting = new SMTPServerSetting
            {
                Host = host,
                Port = port,
                RequiredHostAuthentication = authentication,
                HostLogin = hostLogin,
                HostPassword = hostPassword,
                SenderDisplayName = senderDisplayName,
                SenderEmailAddress = senderEmailAddress,
                EnableSSL = enableSSL
            };

            SettingsManager.Instance.SaveSettings(crmSettings, TenantProvider.CurrentTenantID);
        }

        public static void SaveDefaultCurrencySettings(CurrencyInfo currency)
        {
            var tenantSettings = Global.TenantSettings;
            tenantSettings.DefaultCurrency = currency;

            SettingsManager.Instance.SaveSettings(tenantSettings, TenantProvider.CurrentTenantID);
        }

        #endregion

        #region Invoice PDF

        public static ASC.Files.Core.File GetInvoicePdfExistingOrCreate(ASC.CRM.Core.Entities.Invoice invoice)
        {
            var existingFile = invoice.GetInvoiceFile();
            if (existingFile != null)
            {
                return existingFile;
            }
            else
            {
                var newFile = PdfCreator.CreateFile(invoice);
                invoice.FileID = Int32.Parse(newFile.ID.ToString());
                Global.DaoFactory.GetInvoiceDao().UpdateInvoiceFileID(invoice.ID, invoice.FileID);
                Global.DaoFactory.GetRelationshipEventDao().AttachFiles(invoice.ContactID, invoice.EntityType, invoice.EntityID, new[] { invoice.FileID });
                return newFile;
            }
        }

        #endregion

        //Code snippet

        /// <summary>
        /// method for generating a country list, say for populating
        /// a ComboBox, with country options. We return the
        /// values in a Generic List<T>
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<int, string>> GetCountryListBase()
        {
            var cultureList = CultureInfo.GetCultures(CultureTypes.AllCultures)
                            .Where(culture => !culture.IsNeutralCulture && culture.LCID != 127)
                            .Select(culture => new KeyValuePair<int,string>(culture.LCID, new RegionInfo(culture.Name).EnglishName))
                            .ToList();

            return cultureList;
        }

        public static List<String> GetCountryListExt() { 
            var country = new List<string> ();
            
            var enUs = CultureInfo.GetCultureInfo("en-US");
            var additionalCountries = new List<KeyValuePair<int,string>>
                                        {
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_Gambia", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_Ghana", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_RepublicOfCyprus", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_SierraLeone", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_Tanzania", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_Zambia", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_RepublicOfMadagascar", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_SolomonIslands", enUs)),
                                            new KeyValuePair<int,string>(2072,CRMCommonResource.ResourceManager.GetString("Country_RepublicOfMoldova", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_RepublicOfMauritius", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_BurkinaFaso", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_RepublicOfMozambique", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_RepublicOfMalawi", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_Benin", enUs)),
                                            new KeyValuePair<int,string>(12300,CRMCommonResource.ResourceManager.GetString("Country_IvoryCoast", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_Bahamas", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_Andorra", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_BritishVirginIslands", enUs)),
                                            new KeyValuePair<int,string>(9228,CRMCommonResource.ResourceManager.GetString("Country_RepublicOfCongo", enUs)),
                                            new KeyValuePair<int,string>(0,CRMCommonResource.ResourceManager.GetString("Country_RepublicOfCuba", enUs))
                                        };//https://msdn.microsoft.com/en-us/goglobal/bb964664

            var additionalCountriesCodes = additionalCountries.Select(s => s.Key).Where(s => s != 0).ToList();


            var standardCountries = Global.GetCountryListBase()
                                .Where(s => !additionalCountriesCodes.Contains(s.Key)).ToList();

            country.AddRange(additionalCountries.Select(s => s.Value).ToList());
            country.AddRange(standardCountries.Select(s => s.Value).Distinct().ToList());
            country = country.Distinct().OrderBy(c => c).ToList();

            return country;
        }


        public static String GetUpButtonHTML(Uri requestUrlReferrer)
        {
            return String.Format(@"<a title='{0}' {1} class='studio-level-up{2}' style='margin-top: 2px;'></a>",
                                  CRMCommonResource.Up,
                                  requestUrlReferrer != null ? "href='" + requestUrlReferrer.OriginalString + "'" : "",
                                  requestUrlReferrer != null ? "" : " disable");
        }

        public static String RenderItemHeaderWithMenu(String title, EntityType entityType, Boolean isPrivate, Boolean canEdit)
        {
            var sbIcon = new StringBuilder();
            var sbPrivateMark = new StringBuilder();

            string titleIconClass;
            switch (entityType)
            {
                case EntityType.Contact:
                    titleIconClass = "group";
                    break;
                case EntityType.Person:
                    titleIconClass = "people";
                    break;
                case EntityType.Company:
                    titleIconClass = "company";
                    break;
                case EntityType.Case:
                    titleIconClass = "cases";
                    break;
                case EntityType.Opportunity:
                    titleIconClass = "opportunities";
                    break;
                case EntityType.Invoice:
                    titleIconClass = "documents";
                    break;
                default:
                    titleIconClass = string.Empty;
                    break;
            }
            if (!String.IsNullOrEmpty(titleIconClass))
            {
                if (isPrivate)
                {
                    sbPrivateMark.AppendFormat("<div class='privateMark' title='{0}'></div>", CRMCommonResource.Private);
                }
                sbIcon.AppendFormat("<span class='main-title-icon {0}'>{1}</span>", titleIconClass, sbPrivateMark);
            }

            return String.Format(@" <div class='header-with-menu crm-pageHeader'>
                                        {0}<span class='crm-pageHeaderText text-overflow'>{1}</span>
                                        {2}
                                    </div>
                                  ",
                                        sbIcon,
                                        title,
                                        canEdit ? "<span class='menu-small'></span>" : ""
                                      );
        }


        /// <summary>
        /// The method to Decode your Base64 strings.
        /// </summary>
        /// <param name="encodedData">The String containing the characters to decode.</param>
        /// <returns>A String containing the results of decoding the specified sequence of bytes.</returns>
        public static string DecodeFrom64(string encodedData)
        {
            var encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);
        }

        /// <summary>
        /// The method create a Base64 encoded string from a normal string.
        /// </summary>
        /// <param name="toEncode">The String containing the characters to encode.</param>
        /// <returns>The Base64 encoded string.</returns>
        public static string EncodeTo64(string toEncode)
        {
            var toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(toEncode);

            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static byte[] ToByteArray(Stream inputStream)
        {
            var br = new MemoryStream();
            var data = new byte[1024];
            var readed = 0;

            while ((readed = inputStream.Read(data, 0, data.Length)) > 0)
            {
                br.Write(data, 0, readed);
            }
            br.Close();
            return br.ToArray();
        }

        #region CRM Images download

        public static string GetImgFormatName(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Bmp)) return "bmp";
            if (format.Equals(ImageFormat.Emf)) return "emf";
            if (format.Equals(ImageFormat.Exif)) return "exif";
            if (format.Equals(ImageFormat.Gif)) return "gif";
            if (format.Equals(ImageFormat.Icon)) return "icon";
            if (format.Equals(ImageFormat.Jpeg)) return "jpeg";
            if (format.Equals(ImageFormat.MemoryBmp)) return "MemoryBMP";
            if (format.Equals(ImageFormat.Png)) return "png";
            if (format.Equals(ImageFormat.Tiff)) return "tiff";
            if (format.Equals(ImageFormat.Wmf)) return "wmf";

            return "jpg";
        }

        public static byte[] SaveToBytes(Image img)
        {
            return CommonPhotoManager.SaveToBytes(img, GetImgFormatName(img.RawFormat));
        }

        #endregion

        #region Parse JOjbect with ApiDateTime

        private static readonly string[] Formats = new[]
                                                       {
                                                           "o",
                                                           "yyyy'-'MM'-'dd'T'HH'-'mm'-'ss'.'fffK",
                                                           "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",
                                                           "yyyy-MM-ddTHH:mm:ss"
                                                       };

        public static DateTime ApiDateTimeParse(string data)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException("data");

            if (data.Length < 7) throw new ArgumentException(CRMErrorsResource.DateTimeFormatInvalid);

            DateTime dateTime;
            if (DateTime.TryParseExact(data, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dateTime))
            {
                return new DateTime(dateTime.Ticks, DateTimeKind.Unspecified);
            }
            throw new ArgumentException(CRMErrorsResource.DateTimeFormatInvalid);
        }

        public static JObject JObjectParseWithDateAsString(string data)
        {
            JsonReader reader = new JsonTextReader(
                          new StringReader(data)
                          );
            reader.DateParseHandling = DateParseHandling.None;
            return JObject.Load(reader);
        }

        #endregion
    }
}