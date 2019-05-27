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
using System.Security;
using System.Linq;
using System.Net.Sockets;
using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.Novell.Exceptions;
using Novell.Directory.Ldap.Rfc2251;

namespace ASC.ActiveDirectory.Novell
{
    public class NovellLdapSettingsChecker : LdapSettingsChecker
    {
        public LdapCertificateConfirmRequest CertificateConfirmRequest { get; set; }

        public LdapHelper LdapHelper {
            get { return LdapImporter.LdapHelper; }
        }

        public NovellLdapSettingsChecker(LdapUserImporter importer) :
            base(importer)
        {
        }

        public override LdapSettingsStatus CheckSettings()
        {
            if (!Settings.EnableLdapAuthentication)
                return LdapSettingsStatus.Ok;

            if (Settings.Server.Equals("LDAP://", StringComparison.InvariantCultureIgnoreCase))
                return LdapSettingsStatus.WrongServerOrPort;

            if (!LdapHelper.IsConnected)
            {
                try
                {
                    LdapHelper.Connect();
                }
                catch (NovellLdapTlsCertificateRequestedException ex)
                {
                    log.ErrorFormat("CheckSettings(acceptCertificate={0}): NovellLdapTlsCertificateRequestedException: {1}", Settings.AcceptCertificate, ex);
                    CertificateConfirmRequest = ex.CertificateConfirmRequest;
                    return LdapSettingsStatus.CertificateRequest;
                }
                catch (NotSupportedException ex)
                {
                    log.ErrorFormat("CheckSettings(): NotSupportedException: {0}", ex);
                    return LdapSettingsStatus.TlsNotSupported;
                }
                catch (SocketException ex)
                {
                    log.ErrorFormat("CheckSettings(): SocketException: {0}", ex);
                    return LdapSettingsStatus.ConnectError;
                }
                catch (ArgumentException ex)
                {
                    log.ErrorFormat("CheckSettings(): ArgumentException: {0}", ex);
                    return LdapSettingsStatus.WrongServerOrPort;
                }
                catch (SecurityException ex)
                {
                    log.ErrorFormat("CheckSettings(): SecurityException: {0}", ex);
                    return LdapSettingsStatus.StrongAuthRequired;
                }
                catch (SystemException ex)
                {
                    log.ErrorFormat("CheckSettings(): SystemException: {0}", ex);
                    return LdapSettingsStatus.WrongServerOrPort;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("CheckSettings(): Exception: {0}", ex);
                    return LdapSettingsStatus.CredentialsNotValid;
                }
            }

            if (!CheckUserDn(Settings.UserDN))
            {
                return LdapSettingsStatus.WrongUserDn;
            }

            if (Settings.GroupMembership)
            {
                if (!CheckGroupDn(Settings.GroupDN))
                {
                    return LdapSettingsStatus.WrongGroupDn;
                }

                try
                {
                    new RfcFilter(Settings.GroupFilter);
                }
                catch
                {
                    return LdapSettingsStatus.IncorrectGroupLDAPFilter;
                }

                if (!LdapImporter.TryLoadLDAPGroups())
                {
                    if (!LdapImporter.AllSkipedDomainGroups.Any())
                        return LdapSettingsStatus.IncorrectGroupLDAPFilter;

                    if (LdapImporter.AllSkipedDomainGroups.All(kv => kv.Value == LdapSettingsStatus.WrongSidAttribute))
                        return LdapSettingsStatus.WrongSidAttribute;

                    if (LdapImporter.AllSkipedDomainGroups.All(kv => kv.Value == LdapSettingsStatus.WrongGroupAttribute))
                        return LdapSettingsStatus.WrongGroupAttribute;

                    if (LdapImporter.AllSkipedDomainGroups.All(kv => kv.Value == LdapSettingsStatus.WrongGroupNameAttribute))
                        return LdapSettingsStatus.WrongGroupNameAttribute;
                }

                if (!LdapImporter.AllDomainGroups.Any())
                    return LdapSettingsStatus.GroupsNotFound;
            }

            try
            {
                new RfcFilter(Settings.UserFilter);
            }
            catch
            {
                return LdapSettingsStatus.IncorrectLDAPFilter;
            }

            if (!LdapImporter.TryLoadLDAPUsers())
            {
                if (!LdapImporter.AllSkipedDomainUsers.Any())
                    return LdapSettingsStatus.IncorrectLDAPFilter;

                if (LdapImporter.AllSkipedDomainUsers.All(kv => kv.Value == LdapSettingsStatus.WrongSidAttribute))
                    return LdapSettingsStatus.WrongSidAttribute;

                if (LdapImporter.AllSkipedDomainUsers.All(kv => kv.Value == LdapSettingsStatus.WrongLoginAttribute))
                    return LdapSettingsStatus.WrongLoginAttribute;

                if (LdapImporter.AllSkipedDomainUsers.All(kv => kv.Value == LdapSettingsStatus.WrongUserAttribute))
                    return LdapSettingsStatus.WrongUserAttribute;
            }

            if (!LdapImporter.AllDomainUsers.Any())
                return LdapSettingsStatus.UsersNotFound;

            return string.IsNullOrEmpty(LdapImporter.LDAPDomain)
                ? LdapSettingsStatus.DomainNotFound
                : LdapSettingsStatus.Ok;
        }

        private bool CheckUserDn(string userDn)
        {
            try
            {
                return LdapHelper.CheckUserDn(userDn);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong User DN parameter: {0}. {1}", userDn, e);
                return false;
            }
        }

        private bool CheckGroupDn(string groupDn)
        {
            try
            {
                return LdapHelper.CheckGroupDn(groupDn);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Wrong Group DN parameter: {0}. {1}", groupDn, e);
                return false;
            }
        }
    }
}
