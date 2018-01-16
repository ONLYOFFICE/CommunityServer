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


using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using ASC.Api.Attributes;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Settings;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.SingleSignOn.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using log4net;
using Newtonsoft.Json;
using Resources;

namespace ASC.Api.Settings
{
    public partial class SettingsApi
    {
        /// <summary>
        /// Returns current portal SSO settings
        /// </summary>
        /// <short>
        /// Get SSO settings
        /// </short>
        /// <returns>SsoSettings object</returns>
        [Read("sso")]
        public SsoSettings GetSsoSettings()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var settings = SsoSettings.Load();
            settings.ClientPassword = null;
            return settings;
        }

        /// <summary>
        /// Check that server is standalone (Obsolete)
        /// </summary>
        /// <returns>bool</returns>
        /// <visible>false</visible>
        [Read("sso/isstandalone")]
        public bool IsStandalone()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            return CoreContext.Configuration.Standalone;
        }

        /// <summary>
        /// Save SSO settings for current portal
        /// </summary>
        /// <short>
        /// Save SSO settings
        /// </short>
        /// <param name="serializeSettings">SsoSettings serialized string</param>
        /// <returns>SaveSettingsResult object</returns>
        [Create("sso")]
        public SaveSettingsResult SaveSettings(string serializeSettings)
        {
            CheckSsoPermissions();

            var saveSettingsResult = new SaveSettingsResult();

            var log = LogManager.GetLogger(typeof(SettingsApi));

            try
            {
                if (string.IsNullOrWhiteSpace(serializeSettings))
                {
                    log.Error("SSO settings are null or empty.");
                    saveSettingsResult.Status = Resource.SsoSettingsAreEmpty;
                    return saveSettingsResult;
                }

                var settings = JsonConvert.DeserializeObject<SsoSettings>(serializeSettings);
                if (settings == null)
                {
                    log.Error("Wrong SSO settings were received from client.");
                    saveSettingsResult.Status = Resource.SsoSettingsWrongSerialization;
                    return saveSettingsResult;
                }

                if (settings.EnableSso)
                {
                    if (!(string.IsNullOrWhiteSpace(settings.Issuer) || CheckUri(settings.Issuer)))
                    {
                        log.ErrorFormat("Wrong Issuer URL: {0}", settings.Issuer);
                        saveSettingsResult.Status = string.Format(Resource.SsoSettingsWrongURL, settings.Issuer);
                        return saveSettingsResult;
                    }
                    settings.Issuer = settings.Issuer.Trim();

                    if (!(string.IsNullOrWhiteSpace(settings.SsoEndPoint) || CheckUri(settings.SsoEndPoint)))
                    {
                        log.ErrorFormat("Wrong SsoEndPoint URL: {0}", settings.SsoEndPoint);
                        saveSettingsResult.Status = string.Format(Resource.SsoSettingsWrongURL, settings.SsoEndPoint);
                        return saveSettingsResult;
                    }

                    settings.SsoEndPoint = settings.SsoEndPoint.Trim();

                    if (!string.IsNullOrWhiteSpace(settings.SloEndPoint) && !CheckUri(settings.SloEndPoint))
                    {
                        log.ErrorFormat("Wrong SloEndPoint URL: {0}", settings.SloEndPoint);
                        saveSettingsResult.Status = string.Format(Resource.SsoSettingsWrongURL, settings.SloEndPoint);
                        return saveSettingsResult;
                    }

                    settings.SloEndPoint = (settings.SloEndPoint ?? "").Trim();

                    if (string.IsNullOrWhiteSpace(settings.PublicKey))
                    {
                        log.ErrorFormat("Wrong PublicKey: {0}", settings.PublicKey);
                        saveSettingsResult.Status = Resource.SsoSettingsWrongPublicKey;
                        return saveSettingsResult;
                    }

                    settings.PublicKey = settings.PublicKey.Trim();

                    if (settings.TokenType != TokenTypes.SAML && settings.TokenType != TokenTypes.JWT)
                    {
                        log.ErrorFormat("Wrong token type: {0}", settings.TokenType);
                        saveSettingsResult.Status = Resource.SsoSettingsWrongTokenType;
                        return saveSettingsResult;
                    }

                    if (settings.ValidationType != ValidationTypes.HMAC_SHA256 &&
                         settings.ValidationType != ValidationTypes.RSA_SHA256 && 
                         settings.ValidationType != ValidationTypes.X509)
                    {
                        log.ErrorFormat("Wrong validaion type: {0}", settings.ValidationType);
                        saveSettingsResult.Status = Resource.SsoSettingsWrongValidationType;
                        return saveSettingsResult;
                    }

                    //TODO: Why is it necessary?
                    if (settings.TokenType == TokenTypes.SAML && settings.ValidationType != ValidationTypes.X509)
                    {
                        settings.ValidationType = ValidationTypes.X509;
                    }
                }

                var base64ExportCertificate = FillBase64ExportCertificate(settings, log);

                if (!string.IsNullOrEmpty(settings.ClientCertificateFileName) && string.IsNullOrEmpty(base64ExportCertificate))
                {
                    log.ErrorFormat("Wrong certificate or password: {0}", settings.ClientCertificateFileName);
                    saveSettingsResult.Status = Resource.WrongPasswordOrIncorrectCertitificate;
                    return saveSettingsResult;
                }

                if (!string.IsNullOrEmpty(settings.ClientPassword))
                {
                    settings.ClientPassword = InstanceCrypto.Encrypt(settings.ClientPassword);
                }

                if (!settings.Save())
                {
                    log.Error("Can't save SSO settings.");
                    saveSettingsResult.Status = Resource.SsoSettingsCantSaveSettings;
                    return saveSettingsResult;
                }

                var messageAction = settings.EnableSso ? MessageAction.SSOEnabled : MessageAction.SSODisabled;

                MessageService.Send(HttpContext.Current.Request, messageAction);

                saveSettingsResult.PublicKey = base64ExportCertificate;
                saveSettingsResult.Status = string.Empty;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Save SSO setting, error: {0}", e);
                saveSettingsResult.Status = Resource.SsoSettingsUnexpectedError;
            }

            return saveSettingsResult;
        }

        /// <summary>
        /// Check SSO certificate existance
        /// </summary>
        /// <returns>object like {success: true, exist: true}</returns>
        /// <visible>false</visible>
        [Read("sso/cert/check")]
        public object CheckSsoCertificate()
        {
            CheckSsoPermissions();

            var log = LogManager.GetLogger(typeof(SettingsApi));
            try
            {
                var settings = SsoSettings.Load();

                if (!string.IsNullOrEmpty(settings.ClientPassword))
                    settings.ClientPassword = InstanceCrypto.Decrypt(settings.ClientPassword);

                return new
                {
                    success = true,
                    exist = GetCertificate(settings.ClientCertificateFileName, settings.ClientPassword, log) != null
                };
            }
            catch (Exception exception)
            {
                log.Error(exception);

                return new
                {
                    success = false,
                    message = exception.Message
                };
            }
        }

        /// <summary>
        /// Upload SSO certificate
        /// </summary>
        /// <param name="certPassword">Certificate password</param>
        /// <param name="files">List of HttpPostedFileBase objects with Certificate file</param>
        /// <returns>object like {success: true} or {success: false, message: "some error"}</returns>
        /// <visible>false</visible>
        [Create("sso/cert/upload")]
        public object UploadSsoCertificate(string certPassword, List<HttpPostedFileBase> files)
        {
            CheckSsoPermissions();

            var log = LogManager.GetLogger(typeof(SettingsApi));

            try
            {
                if (!CoreContext.Configuration.Standalone)
                {
                    log.Error("Impossible to upload sso client certificate. Allowed in standalone version only.");
                    return new
                    {
                        success = false,
                        message = Resource.ErrorAccessDenied
                    };
                }

                if (!files.Any())
                {
                    log.Error("Impossible to upload sso client certificate. Required admin privileges.");
                    return new
                    {
                        success = false,
                        message = Resource.ErrorEmptyUploadFileSelected
                    };
                }

                var userId = SecurityContext.CurrentAccount.ID;

                var userInfo = CoreContext.UserManager.GetUsers(userId);
                if (Equals(userInfo, Core.Users.Constants.LostUser) || !userInfo.IsAdmin())
                {
                    log.Error("Impossible to upload sso client certificate. Required admin privileges.");
                    return new
                    {
                        success = false,
                        message = Resource.ErrorAccessDenied
                    };
                }

                var certificate = files[0];
                var cert = GetCertificate(null, certificate, certPassword, log);

                if (cert != null)
                {
                    var storage = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "certs");
                    storage.DeleteDirectory("sso");
                    storage.Save(Path.Combine("sso", certificate.FileName), certificate.InputStream);
                }
                else
                {
                    return new
                    {
                        success = false,
                        message = Resource.WrongPasswordOrIncorrectCertitificate
                    };
                }

                return new
                {
                    success = true
                };
            }
            catch (Exception exception)
            {
                log.Error(exception);

                return new
                {
                    success = false,
                    message = exception.Message
                };
            }
        }

        /// <summary>
        /// Get SSO certificate public key
        /// </summary>
        /// <short>
        /// Get SSO certificate key
        /// </short>
        /// <returns>object like {success: true, publicKey: "[key string]"} or {success: false, message: "some error"}</returns>
        /// <visible>false</visible>
        [Read("sso/cert/key")]
        public object GetSsoCertificateKey()
        {
            CheckSsoPermissions();

            var log = LogManager.GetLogger(typeof(SettingsApi));

            try
            {
                var settings = SsoSettings.Load();

                if (string.IsNullOrEmpty(settings.ClientCertificateFileName))
                    throw new Exception("Certificate is not found");

                if (!string.IsNullOrEmpty(settings.ClientPassword))
                    settings.ClientPassword = InstanceCrypto.Decrypt(settings.ClientPassword);

                var base64ExportCertificate = FillBase64ExportCertificate(settings, log);

                if (string.IsNullOrEmpty(base64ExportCertificate))
                    throw new Exception("Certificate is unaccessable");

                return new
                {
                    success = true,
                    publicKey = base64ExportCertificate
                };

            }
            catch (Exception exception)
            {
                log.Error(exception);

                return new
                {
                    success = false,
                    message = exception.Message
                };
            }
        }

        /// <summary>
        /// Reset SSO settings for current portal
        /// </summary>
        /// <short>
        /// Reset SSO settings
        /// </short>
        /// <returns>object like {success: true} or {success: false, message: "some error"}</returns>
        [Delete("sso")]
        public object ResetSettings()
        {
            CheckSsoPermissions();

            var log = LogManager.GetLogger(typeof(SettingsApi));

            try
            {
                var settings = SsoSettings.Load();

                var defaultSettings = settings.GetDefault() as SsoSettings;

                if (Equals(settings, defaultSettings))
                {
                    return new
                    {
                        success = true
                    };
                }

                defaultSettings.Save();

                if (string.IsNullOrEmpty(settings.ClientCertificateFileName))
                {
                    return new
                    {
                        success = true
                    };
                }

                try
                {
                    var storage = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "certs");

                    if(storage.IsDirectory("sso"))
                        storage.DeleteDirectory("sso");
                }
                catch (Exception e)
                {
                    log.Error(e);
                }

                return new
                {
                    success = true
                };

            }
            catch (Exception exception)
            {
                log.Error(exception);

                return new
                {
                    success = false,
                    message = exception.Message
                };
            }
        }

        private static bool CheckUri(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static string FillBase64ExportCertificate(SsoSettings settings, ILog log)
        {
            var cert = GetCertificate(settings.ClientCertificateFileName, settings.ClientPassword, log);
            if (cert != null)
            {
                var builder = new StringBuilder();
                builder.AppendLine("-----BEGIN CERTIFICATE-----");
                builder.AppendLine(Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
                builder.Append("-----END CERTIFICATE-----");
                return builder.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private static X509Certificate2 GetCertificate(string clientCrtFilename, string clientCrtPassword, ILog log)
        {
            X509Certificate2 certificate;
            try
            {
                var certificatePassword = ConfigurationManager.AppSettings["saml.request.certificate.password"];

                if (CoreContext.Configuration.Standalone)
                {
                    var storage = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "certs");

                    using (var stream = storage.GetReadStream(Path.Combine("sso", clientCrtFilename)))
                    {
                        var rawData = ReadFullyStream(stream);

                        certificate = new X509Certificate2(rawData,
                            (clientCrtPassword ?? certificatePassword) ?? string.Empty);
                    }
                }
                else
                {
                    var currentDir = HttpContext.Current.Request.PhysicalApplicationPath;

                    if (string.IsNullOrEmpty(currentDir))
                        throw new Exception("HttpContext->PhysicalApplicationPath is empty");

                    var crtName = string.IsNullOrEmpty(clientCrtFilename) ? "sp.pfx" : clientCrtFilename;
                    var crtPath = Path.Combine(currentDir, "Certificates\\" + crtName);

                    certificate = new X509Certificate2(crtPath,
                        (clientCrtPassword ?? certificatePassword) ?? string.Empty);
                }
            }
            catch (CryptographicException ex)
            {
                log.DebugFormat("Using SAML requests without certificate. {0}", ex);
                certificate = null;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Certification error. {0}", ex);
                certificate = null;
            }
            return certificate;
        }

        private static X509Certificate2 GetCertificate(string clientCrtFilename, HttpPostedFileBase crtFile, string clientCrtPassword, ILog log)
        {
            X509Certificate2 certificate = null;
            try
            {
                var certificatePassword = ConfigurationManager.AppSettings["saml.request.certificate.password"];

                if (CoreContext.Configuration.Standalone)
                {
                    Stream stream;
                    if (clientCrtFilename != null)
                    {
                        var storage = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "certs");
                        stream = storage.GetReadStream(Path.Combine("sso", clientCrtFilename));
                    }
                    else
                    {
                        stream = crtFile.InputStream;
                    }

                    var rawData = ReadFullyStream(stream);
                    certificate = new X509Certificate2(rawData,
                        (clientCrtPassword ?? certificatePassword) ?? string.Empty);
                }
                else
                {
                    var currentDir = HttpContext.Current.Request.PhysicalApplicationPath;

                    if (string.IsNullOrEmpty(currentDir))
                        throw new Exception("HttpContext->PhysicalApplicationPath is empty");

                    var crtPath = Path.Combine(currentDir, "Certificates\\sp.pfx");

                    certificate = new X509Certificate2(crtPath,
                        (clientCrtPassword ?? certificatePassword) ?? string.Empty);
                }
            }
            catch (CryptographicException ex)
            {
                log.DebugFormat("Using SAML requests without certificate. {0}", ex);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Certification error. {0}", ex);
            }
            return certificate;
        }

        private static byte[] ReadFullyStream(Stream input)
        {
            var buffer = new byte[4 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private static void CheckSsoPermissions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!SetupInfo.IsVisibleSettings(ManagementType.SingleSignOnSettings.ToString()) ||
                (CoreContext.Configuration.Standalone &&
                 !CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Sso))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Sso");
            }
        }
    }
}
