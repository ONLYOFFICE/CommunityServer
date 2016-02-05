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


using log4net;
using System;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;

namespace ASC.Xmpp.Server.Authorization
{
    public class SystemLdapHelper : ILdapHelper
    {
        private const int SSL_LDAP_PORT = 636;
        private const int LDAP_V3 = 3;
        private readonly ILog log = LogManager.GetLogger(typeof(SystemLdapHelper));

        public string GetAccountNameBySid(string sid, bool authentication, string login,
            string password, string server, int portNumber, string userDN, string loginAttribute, bool startTls)
        {
            DirectorySearcher dsSearcher = null;
            SearchResult result = null;
            string accountName = null;
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
            if (portNumber == SSL_LDAP_PORT)
            {
                type |= AuthenticationTypes.SecureSocketsLayer;
            }
            try
            {
                string dn = server + ":" + portNumber + "/" + userDN;
                string searchFilter = "(objectSid=" + sid + ")";
                var attributes = new string[] { loginAttribute };

                using (var entry = CreateDirectoryEntry(authentication, dn, login, password, type))
                {
                    using (dsSearcher = new DirectorySearcher(entry, searchFilter, attributes))
                    {
                        dsSearcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;
                        dsSearcher.ReferralChasing = ReferralChasingOption.All;
                        result = dsSearcher.FindOne();
                        if (result != null)
                        {
                            accountName = result.Properties[loginAttribute][0].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "Can't access to objectSid: {0}. login = {1}, server = {2}, port = {3}, userDN = {4}, loginAttribute = {5}, {6}",
                    sid, login, server, portNumber, userDN, loginAttribute, ex);
            }
            finally
            {
                if (dsSearcher != null)
                {
                    dsSearcher.Dispose();
                }
            }
            return accountName;
        }

        public bool CheckCredentials(string login, string password, string server, int portNumber, string settingsLogin, bool startTls)
        {
            try
            {
                var domainName = server.Split('/').Last() + ":" + portNumber;
                // if login with domain
                login = login.Split('@')[0];

                using (var ldap = new LdapConnection(domainName))
                {
                    var networkCredential = new NetworkCredential(login, password, domainName);
                    ldap.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback((con, cer) => true);
                    ldap.SessionOptions.SecureSocketLayer = (portNumber == SSL_LDAP_PORT);
                    ldap.SessionOptions.ProtocolVersion = LDAP_V3;
                    ldap.AuthType = AuthType.Negotiate;
                    ldap.Bind(networkCredential);
                }
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Check credentials problem. login = {0}, server = {1}, portNumber = {2} {3}",
                    login, server, portNumber, ex);
            }
            return false;
        }

        private DirectoryEntry CreateDirectoryEntry(
            bool authentication, string dn, string login, string password, AuthenticationTypes type)
        {
            return authentication ? new DirectoryEntry(dn, login, password, type) : new DirectoryEntry(dn);
        }
    }
}
