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
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASC.ActiveDirectory;
using ASC.ActiveDirectory.BuiltIn;
using ASC.ActiveDirectory.DirectoryServices;
using ASC.ActiveDirectory.Novell;
using ASC.Common.Security.Authorizing;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;
using ASC.Security.Cryptography;
using log4net;
using Resources;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Studio.Core.Import.LDAP
{
    public abstract class LDAPOperation
    {
        public const string OWNER = "LDAPOwner";
        public const string OPERATION_TYPE = "LDAPOperationType";
        public const string SOURCE = "LDAPSource";
        public const string PROGRESS = "LDAPProgress";
        public const string RESULT = "LDAPResult";
        public const string ERROR = "LDAPError";
        public const string CERT_ALLOW = "SaveLdapSettingTaskAcceptCertificate";
        public const string CERT_REQUEST = "LDAPCertRequest";
        //public const string PROCESSED = "LDAPProcessed";
        public const string FINISHED = "LDAPFinished";

        private readonly string _culture;
        
        private readonly LdapSettingsChecker _ldapSettingsChecker;

        public LDAPSupportSettings LDAPSettings { get; private set; }

        public LDAPUserImporter Importer { get; private set; }

        protected DistributedTask TaskInfo { get; private set; }

        protected int Progress { get; private set; }

        protected string Source { get; private set; }

        protected string Status { get; set; }

        protected string Error { get; set; }

        protected Tenant CurrentTenant { get; private set; }

        protected ILog Logger { get; private set; }

        protected CancellationToken CancellationToken { get; private set; }

        public abstract LDAPOperationType OperationType { get; }

        protected LDAPOperation(LDAPSupportSettings settings, Tenant tenant = null, bool? acceptCertificate = null)
        {
            CurrentTenant = tenant ?? CoreContext.TenantManager.GetCurrentTenant();

            _culture = Thread.CurrentThread.CurrentCulture.Name;

            LDAPSettings = settings;

            _ldapSettingsChecker = !WorkContext.IsMono
                                       ? (LdapSettingsChecker)new SystemLdapSettingsChecker()
                                       : new NovellLdapSettingsChecker();

            Source = "";
            Progress = 0;
            Status = "";
            Error = "";
            Source = "";

            TaskInfo = new DistributedTask();

            if(acceptCertificate.HasValue)
                TaskInfo.SetProperty(CERT_ALLOW, acceptCertificate.Value);
        }

        public void RunJob(DistributedTask _, CancellationToken cancellationToken)
        {
            try
            {
                CancellationToken = cancellationToken;

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(_culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(_culture);

                Logger = LogManager.GetLogger(typeof(LDAPOperation));

                if (LDAPSettings == null)
                {                   
                    Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                    Logger.Error("Can't save default LDAP settings.");
                    return;
                }

                if (OperationType == LDAPOperationType.Save)
                {
                    SetProgress(1, Resource.LdapSettingsStatusCheckingLdapSettings);

                    PrepareSettings(LDAPSettings);

                    if(!string.IsNullOrEmpty(Error))
                        return;

                    Importer = new LDAPUserImporter(LDAPSettings);

                    SetProgress(5, Resource.LdapSettingsStatusLoadingBaseInfo);

                    var acceptCertificate = TaskInfo.GetProperty<bool>(CERT_ALLOW);

                    var result = _ldapSettingsChecker.CheckSettings(Importer, acceptCertificate);

                    if (result == LdapSettingsChecker.CERTIFICATE_REQUEST)
                    {
                        TaskInfo.SetProperty(FINISHED, true);

                        TaskInfo.SetProperty(CERT_REQUEST, ((NovellLdapSettingsChecker)_ldapSettingsChecker).CertificateConfirmRequest);

                        SetProgress(0, Resource.LdapSettingsStatusCertificateVerification);

                        return;
                    }

                    var error = GetError(result);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Error = error;
                        return;
                    }

                    SetProgress(10, Resource.LdapSettingsStatusSavingSettings);

                    LDAPSettings.IsDefault = LDAPSettings.Equals(LDAPSettings.GetDefault());

                    if (!SettingsManager.Instance.SaveSettings(LDAPSettings, CurrentTenant.TenantId))
                    {
                        Logger.Error("Can't save LDAP settings.");
                        Error = Resource.LdapSettingsErrorCantSaveLdapSettings;
                        return;
                    }
                }
                else if(OperationType == LDAPOperationType.Sync)
                {
                    Importer = new LDAPUserImporter(LDAPSettings);
                }

                Do();
            }
            catch (AuthorizingException authError)
            {
                Error = Resource.ErrorAccessDenied;
                Logger.Error(Error, new SecurityException(Error, authError));
            }
            catch (AggregateException ae)
            {
                ae.Flatten().Handle(e => e is TaskCanceledException || e is OperationCanceledException);
            }
            catch (TenantQuotaException e)
            {
                Error = Resource.LdapSettingsTenantQuotaSettled;
                Logger.ErrorFormat("TenantQuotaException. {0}", e);
            }
            catch (FormatException e)
            {
                Error = Resource.LdapSettingsErrorCantCreateUsers;
                Logger.ErrorFormat("FormatException error. {0}", e);
            }
            catch (Exception e)
            {
                Error = Resource.LdapSettingsInternalServerError;
                Logger.ErrorFormat("Internal server error. {0}", e);
            }
            finally
            {
                try
                {
                    TaskInfo.SetProperty(FINISHED, true);
                    PublishTaskInfo();
                }
                catch { /* ignore */ }
            }
        }

        public virtual DistributedTask GetDistributedTask()
        {
            FillDistributedTask();
            return TaskInfo;
        }

        protected virtual void FillDistributedTask()
        {
            TaskInfo.SetProperty(SOURCE, Source);
            TaskInfo.SetProperty(OPERATION_TYPE, OperationType);
            TaskInfo.SetProperty(OWNER, CurrentTenant.TenantId);
            TaskInfo.SetProperty(PROGRESS, Progress < 100 ? Progress : 100);
            TaskInfo.SetProperty(RESULT, Status);
            TaskInfo.SetProperty(ERROR, Error);
            //TaskInfo.SetProperty(PROCESSED, successProcessed);
        }

        protected int GetProgress()
        {
           return Progress;
        }

        public void SetProgress(int? currentPercent = null, string currentStatus = null, string currentSource = null)
        {
            if(!currentPercent.HasValue && currentStatus == null && currentSource == null)
                return;

            if(currentPercent.HasValue)
                Progress = currentPercent.Value;

            if (currentStatus != null)
                Status = currentStatus;

            if (currentSource != null)
                Source = currentSource;

            PublishTaskInfo();
        }

        protected void PublishTaskInfo()
        {
            FillDistributedTask();
            TaskInfo.PublishChanges();
        }

        protected abstract void Do();

        private void PrepareSettings(LDAPSupportSettings settings)
        {
            if (settings == null)
            {
                Logger.Error("Wrong LDAP settings were received from client.");
                Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                return;
            }

            if (!settings.EnableLdapAuthentication)
            {
                settings.Password = string.Empty;
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.Server))
                settings.Server = settings.Server.Trim();
            else
            {
                Logger.Error("settings.Server is null or empty.");
                Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                return;
            }

            if (!settings.Server.StartsWith("LDAP://"))
                settings.Server = "LDAP://" + settings.Server;

            if (!string.IsNullOrWhiteSpace(settings.UserDN))
                settings.UserDN = settings.UserDN.Trim();
            else
            {
                Logger.Error("settings.UserDN is null or empty.");
                Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.LoginAttribute))
                settings.LoginAttribute = settings.LoginAttribute.Trim();
            else
            {
                Logger.Error("settings.LoginAttribute is null or empty.");
                Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.UserFilter))
                settings.UserFilter = settings.UserFilter.Trim();

            if (!string.IsNullOrWhiteSpace(settings.FirstNameAttribute))
                settings.FirstNameAttribute = settings.FirstNameAttribute.Trim();

            if (!string.IsNullOrWhiteSpace(settings.SecondNameAttribute))
                settings.SecondNameAttribute = settings.SecondNameAttribute.Trim();

            if (!string.IsNullOrWhiteSpace(settings.MailAttribute))
                settings.MailAttribute = settings.MailAttribute.Trim();

            if (!string.IsNullOrWhiteSpace(settings.TitleAttribute))
                settings.TitleAttribute = settings.TitleAttribute.Trim();

            if (!string.IsNullOrWhiteSpace(settings.MobilePhoneAttribute))
                settings.MobilePhoneAttribute = settings.MobilePhoneAttribute.Trim();

            if (settings.GroupMembership)
            {
                if (!string.IsNullOrWhiteSpace(settings.GroupDN))
                    settings.GroupDN = settings.GroupDN.Trim();
                else
                {
                    Logger.Error("settings.GroupDN is null or empty.");
                    Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(settings.GroupFilter))
                    settings.GroupFilter = settings.GroupFilter.Trim();

                if (!string.IsNullOrWhiteSpace(settings.GroupAttribute))
                    settings.GroupAttribute = settings.GroupAttribute.Trim();
                else
                {
                    Logger.Error("settings.GroupAttribute is null or empty.");
                    Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(settings.UserAttribute))
                    settings.UserAttribute = settings.UserAttribute.Trim();
                else
                {
                    Logger.Error("settings.UserAttribute is null or empty.");
                    Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                    return;
                }
            }

            if (WorkContext.IsMono)
                settings.Authentication = true;

            if (!settings.Authentication)
            {
                settings.Password = string.Empty;
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.Login))
                settings.Login = settings.Login.Trim();
            else
            {
                Logger.Error("settings.Login is null or empty.");
                Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                return;
            }

            if (settings.PasswordBytes == null || !settings.PasswordBytes.Any())
            {
                if (!string.IsNullOrEmpty(settings.Password))
                {
                    settings.PasswordBytes =
                        InstanceCrypto.Encrypt(new UnicodeEncoding().GetBytes(settings.Password));
                }
                else
                {
                    Logger.Error("settings.Password is null or empty.");
                    Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                    return;
                }
            }

            settings.Password = string.Empty;
        }

        private static string GetError(byte result)
        {
            switch (result)
            {
                case LdapSettingsChecker.OPERATION_OK:
                    return string.Empty;
                case LdapSettingsChecker.WRONG_SERVER_OR_PORT:
                    return Resource.LdapSettingsErrorWrongServerOrPort;
                case LdapSettingsChecker.WRONG_USER_DN:
                    return Resource.LdapSettingsErrorWrongUserDN;
                case LdapSettingsChecker.INCORRECT_LDAP_FILTER:
                    return Resource.LdapSettingsErrorIncorrectLdapFilter;
                case LdapSettingsChecker.USERS_NOT_FOUND:
                    return Resource.LdapSettingsErrorUsersNotFound;
                case LdapSettingsChecker.WRONG_LOGIN_ATTRIBUTE:
                    return Resource.LdapSettingsErrorWrongLoginAttribute;
                case LdapSettingsChecker.WRONG_GROUP_DN:
                    return Resource.LdapSettingsErrorWrongGroupDN;
                case LdapSettingsChecker.INCORRECT_GROUP_LDAP_FILTER:
                    return Resource.LdapSettingsErrorWrongGroupFilter;
                case LdapSettingsChecker.GROUPS_NOT_FOUND:
                    return Resource.LdapSettingsErrorGroupsNotFound;
                case LdapSettingsChecker.WRONG_GROUP_ATTRIBUTE:
                    return Resource.LdapSettingsErrorWrongGroupAttribute;
                case LdapSettingsChecker.WRONG_USER_ATTRIBUTE:
                    return Resource.LdapSettingsErrorWrongUserAttribute;
                case LdapSettingsChecker.WRONG_GROUP_NAME_ATTRIBUTE:
                    return Resource.LdapSettingsErrorWrongGroupNameAttribute;
                case LdapSettingsChecker.CREDENTIALS_NOT_VALID:
                    return Resource.LdapSettingsErrorCredentialsNotValid;
                case LdapSettingsChecker.CONNECT_ERROR:
                    return Resource.LdapSettingsConnectError;
                case LdapSettingsChecker.STRONG_AUTH_REQUIRED:
                    return Resource.LdapSettingsStrongAuthRequired;
                case LdapSettingsChecker.WRONG_SID_ATTRIBUTE:
                    return Resource.LdapSettingsWrongSidAttribute;
                case LdapSettingsChecker.TLS_NOT_SUPPORTED:
                    return Resource.LdapSettingsTlsNotSupported;
                case LdapSettingsChecker.DOMAIN_NOT_FOUND:
                    return Resource.LdapSettingsErrorDomainNotFound;
                default:
                    return Resource.LdapSettingsErrorUnknownError;
            }
        }
    }
}