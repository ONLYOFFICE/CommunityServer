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
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public static readonly decimal MaxInvoiceItemPrice = (decimal)99999999.99;

        public static CRMSettings TenantSettings
        {
            get { return CRMSettings.Load(); }
        }

        public static IDataStore GetStore()
        {
            return StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "crm");
        }

        public static IDataStore GetStoreTemplate()
        {
            return StorageFactory.GetStorage(String.Empty, "crm_template");
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

                var value = ConfigurationManagerExtension.AppSettings["crm.invoice.download.enable"];
                if (string.IsNullOrEmpty(value)) return false;

                canDownloadFiles = Convert.ToBoolean(value);

                if (canDownloadFiles && string.IsNullOrEmpty(FilesLinkUtility.DocServiceConverterUrl))
                {
                    canDownloadFiles = false;
                }

                return canDownloadFiles;
            }
        }

        public static bool CanCreateReports
        {
            get
            {
                return !string.IsNullOrEmpty(FilesLinkUtility.DocServiceDocbuilderUrl) && CRMSecurity.IsAdmin;
            }
        }

        #region CRM Settings

        public static void SaveDefaultCurrencySettings(CurrencyInfo currency)
        {
            var tenantSettings = TenantSettings;
            tenantSettings.DefaultCurrency = currency;
            tenantSettings.Save();
        }

        #endregion

        #region Invoice PDF

        public static ASC.Files.Core.File GetInvoicePdfExistingOrCreate(ASC.CRM.Core.Entities.Invoice invoice, DaoFactory factory)
        {
            var existingFile = invoice.GetInvoiceFile(factory);
            if (existingFile != null)
            {
                return existingFile;
            }
            else
            {
                var newFile = PdfCreator.CreateFile(invoice, factory);
                invoice.FileID = Int32.Parse(newFile.ID.ToString());
                factory.InvoiceDao.UpdateInvoiceFileID(invoice.ID, invoice.FileID);
                factory.RelationshipEventDao.AttachFiles(invoice.ContactID, invoice.EntityType, invoice.EntityID, new[] { invoice.FileID });
                return newFile;
            }
        }

        #endregion

        //Code snippet

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