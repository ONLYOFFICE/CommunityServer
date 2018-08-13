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
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.Novell;
using ASC.Common.Security.Authorizing;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Security.Cryptography;
using log4net;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.ActiveDirectory.ComplexOperations
{
    public abstract class LdapOperation: IDisposable
    {
        public const string OWNER = "LDAPOwner";
        public const string OPERATION_TYPE = "LDAPOperationType";
        public const string SOURCE = "LDAPSource";
        public const string PROGRESS = "LDAPProgress";
        public const string RESULT = "LDAPResult";
        public const string ERROR = "LDAPError";
        public const string CERT_REQUEST = "LDAPCertRequest";
        public const string FINISHED = "LDAPFinished";

        private readonly string _culture;

        public LdapSettings LDAPSettings { get; private set; }

        public LdapUserImporter Importer { get; private set; }

        public LdapUserManager LDAPUserManager { get; private set; }

        protected DistributedTask TaskInfo { get; private set; }

        protected int Progress { get; private set; }

        protected string Source { get; private set; }

        protected string Status { get; set; }

        protected string Error { get; set; }

        protected Tenant CurrentTenant { get; private set; }

        protected ILog Logger { get; private set; }

        protected CancellationToken CancellationToken { get; private set; }

        public LdapOperationType OperationType { get; private set; }

        public static LdapLocalization Resource { get; private set; }

        protected LdapOperation(LdapSettings settings, Tenant tenant, LdapOperationType operationType, LdapLocalization resource = null)
        {
            CurrentTenant = tenant;

            OperationType = operationType;

            _culture = Thread.CurrentThread.CurrentCulture.Name;

            LDAPSettings = settings;

            Source = "";
            Progress = 0;
            Status = "";
            Error = "";
            Source = "";

            TaskInfo = new DistributedTask();

            Resource = resource ?? new LdapLocalization();

            LDAPUserManager = new LdapUserManager(Resource);
        }

        public void RunJob(DistributedTask _, CancellationToken cancellationToken)
        {
            try
            {
                CancellationToken = cancellationToken;

                CoreContext.TenantManager.SetCurrentTenant(CurrentTenant);

                SecurityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);

                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(_culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(_culture);

                Logger = LogManager.GetLogger(typeof(LdapOperation));

                if (LDAPSettings == null)
                {
                    Error = Resource.LdapSettingsErrorCantGetLdapSettings;
                    Logger.Error("Can't save default LDAP settings.");
                    return;
                }

                switch (OperationType)
                {
                    case LdapOperationType.Save:
                    case LdapOperationType.SaveTest:

                        Logger.InfoFormat("Start '{0}' operation",
                            Enum.GetName(typeof(LdapOperationType), OperationType));

                        SetProgress(1, Resource.LdapSettingsStatusCheckingLdapSettings);

                        Logger.Debug("PrepareSettings()");

                        PrepareSettings(LDAPSettings);

                        if (!string.IsNullOrEmpty(Error))
                        {
                            Logger.DebugFormat("PrepareSettings() Error: {0}", Error);
                            return;
                        }

                        Importer = new NovellLdapUserImporter(LDAPSettings, Resource);

                        if (LDAPSettings.EnableLdapAuthentication)
                        {
                            var ldapSettingsChecker = new NovellLdapSettingsChecker(Importer);

                            SetProgress(5, Resource.LdapSettingsStatusLoadingBaseInfo);

                            var result = ldapSettingsChecker.CheckSettings();

                            if (result != LdapSettingsStatus.Ok)
                            {
                                if (result == LdapSettingsStatus.CertificateRequest)
                                {
                                    TaskInfo.SetProperty(CERT_REQUEST,
                                        ldapSettingsChecker.CertificateConfirmRequest);
                                }

                                Error = GetError(result);

                                Logger.DebugFormat("ldapSettingsChecker.CheckSettings() Error: {0}", Error);

                                return;
                            }
                        }

                        break;
                    case LdapOperationType.Sync:
                    case LdapOperationType.SyncTest:
                        Logger.InfoFormat("Start '{0}' operation",
                            Enum.GetName(typeof(LdapOperationType), OperationType));

                        Importer = new NovellLdapUserImporter(LDAPSettings, Resource);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
                    Dispose();
                    SecurityContext.Logout();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("LdapOperation finalization problem. {0}", ex);
                }
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

        const string PROGRESS_STRING = "Progress: {0}% {1} {2}";

        public void SetProgress(int? currentPercent = null, string currentStatus = null, string currentSource = null)
        {
            if (!currentPercent.HasValue && currentStatus == null && currentSource == null)
                return;

            if (currentPercent.HasValue)
                Progress = currentPercent.Value;

            if (currentStatus != null)
                Status = currentStatus;

            if (currentSource != null)
                Source = currentSource;

            Logger.InfoFormat(PROGRESS_STRING, Progress, Status, Source);

            PublishTaskInfo();
        }

        protected void PublishTaskInfo()
        {
            FillDistributedTask();
            TaskInfo.PublishChanges();
        }

        protected abstract void Do();

        private void PrepareSettings(LdapSettings settings)
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
                settings.Server = "LDAP://" + settings.Server.Trim();

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
                    settings.PasswordBytes = InstanceCrypto.Encrypt(new UnicodeEncoding().GetBytes(settings.Password));
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

        private static string GetError(LdapSettingsStatus result)
        {
            switch (result)
            {
                case LdapSettingsStatus.Ok:
                    return string.Empty;
                case LdapSettingsStatus.WrongServerOrPort:
                    return Resource.LdapSettingsErrorWrongServerOrPort;
                case LdapSettingsStatus.WrongUserDn:
                    return Resource.LdapSettingsErrorWrongUserDn;
                case LdapSettingsStatus.IncorrectLDAPFilter:
                    return Resource.LdapSettingsErrorIncorrectLdapFilter;
                case LdapSettingsStatus.UsersNotFound:
                    return Resource.LdapSettingsErrorUsersNotFound;
                case LdapSettingsStatus.WrongLoginAttribute:
                    return Resource.LdapSettingsErrorWrongLoginAttribute;
                case LdapSettingsStatus.WrongGroupDn:
                    return Resource.LdapSettingsErrorWrongGroupDn;
                case LdapSettingsStatus.IncorrectGroupLDAPFilter:
                    return Resource.LdapSettingsErrorWrongGroupFilter;
                case LdapSettingsStatus.GroupsNotFound:
                    return Resource.LdapSettingsErrorGroupsNotFound;
                case LdapSettingsStatus.WrongGroupAttribute:
                    return Resource.LdapSettingsErrorWrongGroupAttribute;
                case LdapSettingsStatus.WrongUserAttribute:
                    return Resource.LdapSettingsErrorWrongUserAttribute;
                case LdapSettingsStatus.WrongGroupNameAttribute:
                    return Resource.LdapSettingsErrorWrongGroupNameAttribute;
                case LdapSettingsStatus.CredentialsNotValid:
                    return Resource.LdapSettingsErrorCredentialsNotValid;
                case LdapSettingsStatus.ConnectError:
                    return Resource.LdapSettingsConnectError;
                case LdapSettingsStatus.StrongAuthRequired:
                    return Resource.LdapSettingsStrongAuthRequired;
                case LdapSettingsStatus.WrongSidAttribute:
                    return Resource.LdapSettingsWrongSidAttribute;
                case LdapSettingsStatus.TlsNotSupported:
                    return Resource.LdapSettingsTlsNotSupported;
                case LdapSettingsStatus.DomainNotFound:
                    return Resource.LdapSettingsErrorDomainNotFound;
                case LdapSettingsStatus.CertificateRequest:
                    return Resource.LdapSettingsStatusCertificateVerification;
                default:
                    return Resource.LdapSettingsErrorUnknownError;
            }
        }

        public void Dispose()
        {
            if (Importer != null)
                Importer.Dispose();
        }
    }
}