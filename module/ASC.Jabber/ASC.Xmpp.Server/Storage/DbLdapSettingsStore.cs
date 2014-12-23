/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Common.Data.Sql;
using ASC.Core;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using LDAPProtocols = System.DirectoryServices.Protocols;

namespace ASC.Xmpp.Server.Storage
{
    class DbLdapSettingsStore : DbStoreBase
    {
        private readonly static ILog _log = LogManager.GetLogger(typeof(DbLdapSettingsStore));

        private const string LDAP_SETTINGS_ID = "197149b3-fbc9-44c2-b42a-232f7e729c16";
        private const int STANDART_LDAP_PORT = 389;
        private const int SSL_LDAP_PORT = 636;
        private const int LDAP_ERROR_INVALID_CREDENTIALS = 0x31;

        private Dictionary<string, string> properties = new Dictionary<string, string>(1);

        public bool EnableLdapAuthentication
        {
            get;
            private set;
        }

        private string Server
        {
            get;
            set;
        }

        private int PortNumber
        {
            get;
            set;
        }

        public string UserDN
        {
            get;
            private set;
        }

        public string UserAttribute
        {
            get;
            private set;
        }

        public DbLdapSettingsStore()
        {
            properties["connectionStringName"] = "core";
            base.Configure(properties);
        }

        public void GetLdapSettings(string domain)
        {
            var s = Stopwatch.StartNew();
            try
            {
                var tenant = CoreContext.TenantManager.GetTenant(domain);
                if (tenant != null)
                {
                    var q = new SqlQuery("webstudio_settings")
                        .Select("Data")
                        .Where("TenantID", tenant.TenantId)
                        .Where("ID", LDAP_SETTINGS_ID);
                    
                    var settings = ExecuteList(q);
                    if (settings != null && settings.Count > 0 && settings[0] != null)
                    {
                        var stringSettings = (string)settings[0][0];
                        if (stringSettings != null)
                        {
                            var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            var settingsDictionary = (Dictionary<string, object>)jsSerializer.DeserializeObject(stringSettings);
                            EnableLdapAuthentication = Convert.ToBoolean(settingsDictionary["EnableLdapAuthentication"]);
                            Server = Convert.ToString(settingsDictionary["Server"]);
                            PortNumber = Convert.ToInt32(settingsDictionary["PortNumber"]);
                            UserDN = Convert.ToString(settingsDictionary["UserDN"]);
                            UserAttribute = Convert.ToString(settingsDictionary["UserAttribute"]);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _log.ErrorFormat("{0}, {1}", ex, ex.StackTrace);
            }
            s.Stop();
        }

        public string getAccountNameBySid(string sid)
        {
            var s = Stopwatch.StartNew();
            DirectorySearcher dsSearcher = null;
            SearchResult result = null;
            string accountName = null;
            try
            {
                using (var entry = new DirectoryEntry(Server + ":" + PortNumber + "/" + UserDN))
                {
                    using (dsSearcher = new DirectorySearcher(entry, "(objectSid=" + sid + ")", new string[] { "SAMAccountName" }))
                    {
                        dsSearcher.SearchScope = SearchScope.Subtree;
                        dsSearcher.ReferralChasing = ReferralChasingOption.All;
                        result = dsSearcher.FindOne();
                        if (result != null)
                        {
                            try
                            {
                                accountName = result.Properties["SAMAccountName"][0].ToString();
                            }
                            catch
                            {
                                // no such user in directory
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("{0}, {1}", ex, ex.StackTrace);
            }
            finally
            {
                if (dsSearcher != null)
                {
                    dsSearcher.Dispose();
                }
            }
            s.Stop();
            return accountName;
        }

        public bool CheckCredentials(string login, string password)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (password == null) throw new ArgumentNullException("password");
            if (Server == null) throw new ArgumentNullException("Server");

            try
            {
                var domainName = Server.Split('/').Last() + ":" + PortNumber;
                // if login with domain
                login = login.Split('@')[0];

                using (var ldap = new LDAPProtocols.LdapConnection(domainName))
                {
                    var networkCredential = new NetworkCredential(login, password, domainName);
                    ldap.SessionOptions.VerifyServerCertificate = new LDAPProtocols.VerifyServerCertificateCallback((con, cer) => true);
                    ldap.SessionOptions.SecureSocketLayer = (PortNumber == SSL_LDAP_PORT);
                    ldap.SessionOptions.ProtocolVersion = 3;
                    ldap.AuthType = LDAPProtocols.AuthType.Negotiate;
                    ldap.Bind(networkCredential);
                }
                return true;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Internal LDAP authentication error: {0}. {1}", e, e.StackTrace);
            }
            return false;
        }
    }
}
