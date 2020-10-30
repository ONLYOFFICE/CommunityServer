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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Novell.Exceptions;
using ASC.ActiveDirectory.Novell.Extensions;
using ASC.Common.Logging;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;
using Novell.Directory.Ldap.Utilclass;

namespace ASC.ActiveDirectory.Novell
{
    public class NovellLdapSearcher: IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger("ASC");
        private LdapCertificateConfirmRequest _certificateConfirmRequest;
        private static readonly object RootSync = new object();

        private LdapConnection _ldapConnection;

        public string Login { get; private set; }
        public string Password { get; private set; }
        public string Server { get; private set; }
        public int PortNumber { get; private set; }
        public bool StartTls { get; private set; }
        public bool Ssl { get; private set; }
        public bool AcceptCertificate { get; private set; }
        public string AcceptCertificateHash { get; private set; }

        public string LdapUniqueIdAttribute { get; set; }

        private Dictionary<string, string[]> _capabilities;

        public bool IsConnected {
            get { return _ldapConnection != null && _ldapConnection.Connected; }
        }

        public NovellLdapSearcher(string login, string password, string server, int portNumber, bool startTls, bool ssl,
            bool acceptCertificate, string acceptCertificateHash = null)
        {
            Login = login;
            Password = password;
            Server = server;
            PortNumber = portNumber;
            StartTls = startTls;
            Ssl = ssl;
            AcceptCertificate = acceptCertificate;
            AcceptCertificateHash = acceptCertificateHash;

            LdapUniqueIdAttribute = ConfigurationManagerExtension.AppSettings["ldap.unique.id"];
        }

        public void Connect()
        {
            if (Server.StartsWith("LDAP://"))
                Server = Server.Substring("LDAP://".Length);

            var ldapConnection = new LdapConnection();

            if (StartTls || Ssl)
                ldapConnection.UserDefinedServerCertValidationDelegate += ServerCertValidationHandler;

            if (Ssl)
                ldapConnection.SecureSocketLayer = true;

            try
            {
                ldapConnection.ConnectionTimeout = 30000; // 30 seconds

                _log.DebugFormat("ldapConnection.Connect(Server='{0}', PortNumber='{1}');", Server, PortNumber);

                ldapConnection.Connect(Server, PortNumber);

                if (StartTls)
                {
                    _log.Debug("ldapConnection.StartTls();");
                    ldapConnection.StartTls();
                }
            }
            catch (Exception ex)
            {
                if (_certificateConfirmRequest == null)
                {
                    if (ex.Message.StartsWith("Connect Error"))
                    {
                        throw new SocketException();
                    }

                    if (ex.Message.StartsWith("Unavailable"))
                    {
                        throw new NotSupportedException(ex.Message);
                    }

                    throw;
                }

                _log.Debug("LDAP certificate confirmation requested.");

                ldapConnection.Disconnect();

                var exception = new NovellLdapTlsCertificateRequestedException
                {
                    CertificateConfirmRequest = _certificateConfirmRequest
                };

                throw exception;
            }

            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                _log.Debug("ldapConnection.Bind(Anonymous)");

                ldapConnection.Bind(null, null); 
            }
            else
            {
                _log.DebugFormat("ldapConnection.Bind(Login: '{0}')", Login);

                ldapConnection.Bind(Login, Password);
            }

            if (!ldapConnection.Bound)
            {
                throw new Exception("Bind operation wasn't completed successfully.");
            }

            _ldapConnection = ldapConnection;
        }

        private bool ServerCertValidationHandler(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            lock (RootSync)
            {
                var certHash = certificate.GetCertHashString();

                if (LdapUtils.IsCertInstalled(certificate, _log))
                {
                    AcceptCertificate = true;
                    AcceptCertificateHash = certHash;
                    return true;
                }

                if (AcceptCertificate)
                {
                    if (AcceptCertificateHash == null || AcceptCertificateHash.Equals(certHash))
                    {
                        if (LdapUtils.TryInstallCert(certificate, _log))
                        {
                            AcceptCertificateHash = certHash;
                        }

                        return true;
                    }

                    AcceptCertificate = false;
                    AcceptCertificateHash = null;
                }

                _log.WarnFormat("ServerCertValidationHandler: sslPolicyErrors = {0}", sslPolicyErrors);

                _certificateConfirmRequest = LdapCertificateConfirmRequest.FromCert(certificate, chain, sslPolicyErrors, false, true, _log);
            }

            return false;
        }

        public enum LdapScope
        {
            Base = LdapConnection.SCOPE_BASE,
            One = LdapConnection.SCOPE_ONE,
            Sub = LdapConnection.SCOPE_SUB
        }

        public List<LdapObject> Search(LdapScope scope, string searchFilter,
            string[] attributes = null, int limit = -1, LdapSearchConstraints searchConstraints = null)
        {
            return Search("", scope, searchFilter, attributes, limit, searchConstraints);
        }

        public List<LdapObject> Search(string searchBase, LdapScope scope, string searchFilter,
            string[] attributes = null, int limit = -1, LdapSearchConstraints searchConstraints = null)
        {
            if (!IsConnected)
                Connect();

            if (searchBase == null)
                searchBase = "";

            var entries = new List<LdapEntry>();

            if (string.IsNullOrEmpty(searchFilter))
                return new List<LdapObject>();

            if (attributes == null)
            {
                if (string.IsNullOrEmpty(LdapUniqueIdAttribute))
                {
                    attributes = new[]
                    {
                        "*", LdapConstants.RfcLDAPAttributes.ENTRY_DN, LdapConstants.RfcLDAPAttributes.ENTRY_UUID,
                        LdapConstants.RfcLDAPAttributes.NS_UNIQUE_ID, LdapConstants.RfcLDAPAttributes.GUID
                    };
                }
                else
                {
                    attributes = new[] { "*", LdapUniqueIdAttribute };
                }
            }

            var ldapSearchConstraints = searchConstraints ?? new LdapSearchConstraints
            {
                // Maximum number of search results to return.
                // The value 0 means no limit. The default is 1000.
                MaxResults = limit == -1 ? 0 : limit,
                // Returns the number of results to block on during receipt of search results. 
                // This should be 0 if intermediate results are not needed, and 1 if results are to be processed as they come in.
                //BatchSize = 0,
                // The maximum number of referrals to follow in a sequence during automatic referral following. 
                // The default value is 10. A value of 0 means no limit.
                HopLimit = 0,
                // Specifies whether referrals are followed automatically
                // Referrals of any type other than to an LDAP server (for example, a referral URL other than ldap://something) are ignored on automatic referral following.
                // The default is false.
                ReferralFollowing = true,
                // The number of seconds to wait for search results.
                // Sets the maximum number of seconds that the server is to wait when returning search results.
                //ServerTimeLimit = 600000, // 10 minutes
                // Sets the maximum number of milliseconds the client waits for any operation under these constraints to complete.
                // If the value is 0, there is no maximum time limit enforced by the API on waiting for the operation results.
                //TimeLimit = 600000 // 10 minutes
            };

            var queue = _ldapConnection.Search(searchBase,
                (int)scope, searchFilter, attributes, false, ldapSearchConstraints);

            while (queue.hasMore())
            {
                LdapEntry nextEntry;
                try
                {
                    nextEntry = queue.next();

                    if (nextEntry == null)
                        continue;
                }
                catch (LdapException ex)
                {
                    if (!string.IsNullOrEmpty(ex.Message) && ex.Message.Contains("Sizelimit Exceeded"))
                    {
                        if (!string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Password) && limit == -1)
                        {
                            _log.Warn("The size of the search results is limited. Start TrySearchSimple()");

                            List<LdapObject> simpleResults;

                            if (TrySearchSimple(searchBase, scope, searchFilter, out simpleResults, attributes, limit,
                                searchConstraints))
                            {
                                if (entries.Count >= simpleResults.Count)
                                    break;

                                return simpleResults;
                            }
                        }

                        break;
                    }

                    _log.ErrorFormat("Search({0}) error: {1}", searchFilter, ex);
                    continue;
                }

                entries.Add(nextEntry);

                if (string.IsNullOrEmpty(LdapUniqueIdAttribute))
                {
                    LdapUniqueIdAttribute = GetLdapUniqueId(nextEntry);
                }
            }

            var result = entries.ToLdapObjects(LdapUniqueIdAttribute);

            return result;
        }

        private bool TrySearchSimple(string searchBase, LdapScope scope, string searchFilter, out List<LdapObject> results,
            string[] attributes = null, int limit = -1, LdapSearchConstraints searchConstraints = null)
        {

            try
            {
                results = SearchSimple(searchBase, scope, searchFilter, attributes, limit, searchConstraints);

                return true;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("TrySearchSimple() failed. Error: {0}", ex);
            }

            results = null;
            return false;
        }

        public List<LdapObject> SearchSimple(string searchBase, LdapScope scope, string searchFilter,
            string[] attributes = null, int limit = -1, LdapSearchConstraints searchConstraints = null)
        {
            if (!IsConnected)
                Connect();

            if (searchBase == null)
                searchBase = "";

            var entries = new List<LdapEntry>();

            if (string.IsNullOrEmpty(searchFilter))
                return new List<LdapObject>();

            if (attributes == null)
            {
                if (string.IsNullOrEmpty(LdapUniqueIdAttribute))
                {
                    attributes = new[]
                    {
                        "*", LdapConstants.RfcLDAPAttributes.ENTRY_DN, LdapConstants.RfcLDAPAttributes.ENTRY_UUID,
                        LdapConstants.RfcLDAPAttributes.NS_UNIQUE_ID, LdapConstants.RfcLDAPAttributes.GUID
                    };
                }
                else
                {
                    attributes = new[] {"*", LdapUniqueIdAttribute};
                }
            }

            var ldapSearchConstraints = searchConstraints ?? new LdapSearchConstraints
            {
                // Maximum number of search results to return.
                // The value 0 means no limit. The default is 1000.
                MaxResults = limit == -1 ? 0 : limit,
                // Returns the number of results to block on during receipt of search results. 
                // This should be 0 if intermediate results are not needed, and 1 if results are to be processed as they come in.
                //BatchSize = 0,
                // The maximum number of referrals to follow in a sequence during automatic referral following. 
                // The default value is 10. A value of 0 means no limit.
                HopLimit = 0,
                // Specifies whether referrals are followed automatically
                // Referrals of any type other than to an LDAP server (for example, a referral URL other than ldap://something) are ignored on automatic referral following.
                // The default is false.
                ReferralFollowing = true,
                // The number of seconds to wait for search results.
                // Sets the maximum number of seconds that the server is to wait when returning search results.
                //ServerTimeLimit = 600000, // 10 minutes
                // Sets the maximum number of milliseconds the client waits for any operation under these constraints to complete.
                // If the value is 0, there is no maximum time limit enforced by the API on waiting for the operation results.
                //TimeLimit = 600000 // 10 minutes
            };

            // initially, cookie must be set to an empty string
            var pageSize = 2;
            sbyte[] cookie = Array.ConvertAll(Encoding.ASCII.GetBytes(""), b => unchecked((sbyte)b));
            var i = 0;

            do
            {
                var requestControls = new LdapControl[1];
                requestControls[0] = new LdapPagedResultsControl(pageSize, cookie);
                ldapSearchConstraints.setControls(requestControls);
                _ldapConnection.Constraints = ldapSearchConstraints;

                var res = _ldapConnection.Search(searchBase,
                    (int)scope, searchFilter, attributes, false, (LdapSearchConstraints)null);

                while (res.hasMore())
                {
                    LdapEntry nextEntry;
                    try
                    {
                        nextEntry = res.next();

                        if (nextEntry == null)
                            continue;
                    }
                    catch (LdapException ex)
                    {
                        if (ex is LdapReferralException)
                            continue;

                        if (!string.IsNullOrEmpty(ex.Message) && ex.Message.Contains("Sizelimit Exceeded"))
                            break;

                        _log.ErrorFormat("SearchSimple({0}) error: {1}", searchFilter, ex);
                        continue;
                    }

                    _log.DebugFormat("{0}. DN: {1}", ++i, nextEntry.DN);

                    entries.Add(nextEntry);

                    if (string.IsNullOrEmpty(LdapUniqueIdAttribute))
                    {
                        LdapUniqueIdAttribute = GetLdapUniqueId(nextEntry);
                    }
                }

                // Server should send back a control irrespective of the 
                // status of the search request
                var controls = res.ResponseControls;
                if (controls == null)
                {
                    _log.Debug("No controls returned");
                    cookie = null;
                }
                else
                {
                    // Multiple controls could have been returned
                    foreach (LdapControl control in controls)
                    {
                        /* Is this the LdapPagedResultsResponse control? */
                        if (!(control is LdapPagedResultsResponse)) 
                            continue;

                        var response = new LdapPagedResultsResponse(control.ID,
                            control.Critical, control.getValue());

                        cookie = response.Cookie;
                    }
                }
                // if cookie is empty, we are done.
            } while (cookie != null && cookie.Length > 0);

            var result = entries.ToLdapObjects(LdapUniqueIdAttribute);

            return result;
        }

        public Dictionary<string, string[]> GetCapabilities()
        {
            if (_capabilities != null)
                return _capabilities;

            _capabilities = new Dictionary<string, string[]>();

            try
            {
                var ldapSearchConstraints = new LdapSearchConstraints
                {
                    MaxResults = int.MaxValue,
                    HopLimit = 0,
                    ReferralFollowing = true
                };

                var ldapSearchResults = _ldapConnection.Search("", LdapConnection.SCOPE_BASE, LdapConstants.OBJECT_FILTER,
                    new[] {"*", "supportedControls", "supportedCapabilities"}, false, ldapSearchConstraints);

                while (ldapSearchResults.hasMore())
                {
                    LdapEntry nextEntry;
                    try
                    {
                        nextEntry = ldapSearchResults.next();

                        if (nextEntry == null)
                            continue;
                    }
                    catch (LdapException ex)
                    {
                        _log.ErrorFormat("GetCapabilities()->LoopResults failed. Error: {0}", ex);
                        continue;
                    }

                    var attributeSet = nextEntry.getAttributeSet();

                    var ienum = attributeSet.GetEnumerator();

                    while (ienum.MoveNext())
                    {
                        var attribute = (LdapAttribute) ienum.Current;
                        if (attribute == null) 
                            continue;

                        var attributeName = attribute.Name;
                        var attributeVals = attribute.StringValueArray
                            .ToList()
                            .Select(s =>
                            {
                                if (Base64.isLDIFSafe(s)) return s;
                                var tbyte = SupportClass.ToByteArray(s);
                                s = Base64.encode(SupportClass.ToSByteArray(tbyte));

                                return s;
                            }).ToArray();

                        _capabilities.Add(attributeName, attributeVals);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("GetCapabilities() failed. Error: {0}", ex);
            }

            return _capabilities;
        }

        private string GetLdapUniqueId(LdapEntry ldapEntry)
        {
            try
            {
                var ldapUniqueIdAttribute = ConfigurationManagerExtension.AppSettings["ldap.unique.id"];

                if (ldapUniqueIdAttribute != null)
                    return ldapUniqueIdAttribute;

                if (!string.IsNullOrEmpty(
                    ldapEntry.GetAttributeValue(LdapConstants.ADSchemaAttributes.OBJECT_SID) as string))
                {
                    ldapUniqueIdAttribute = LdapConstants.ADSchemaAttributes.OBJECT_SID;
                }
                else if (!string.IsNullOrEmpty(
                    ldapEntry.GetAttributeValue(LdapConstants.RfcLDAPAttributes.ENTRY_UUID) as string))
                {
                    ldapUniqueIdAttribute = LdapConstants.RfcLDAPAttributes.ENTRY_UUID;
                }
                else if(!string.IsNullOrEmpty(
                    ldapEntry.GetAttributeValue(LdapConstants.RfcLDAPAttributes.NS_UNIQUE_ID) as string))
                {
                    ldapUniqueIdAttribute = LdapConstants.RfcLDAPAttributes.NS_UNIQUE_ID;
                }
                else if (!string.IsNullOrEmpty(
                    ldapEntry.GetAttributeValue(LdapConstants.RfcLDAPAttributes.GUID) as string))
                {
                    ldapUniqueIdAttribute = LdapConstants.RfcLDAPAttributes.GUID;
                }

                return ldapUniqueIdAttribute;
            }
            catch (Exception ex)
            {
                _log.Error("GetLdapUniqueId()", ex);
            }

            return null;
        }

        public void Dispose()
        {
            if (!IsConnected)
                return;

            try
            {
                _ldapConnection.Constraints.TimeLimit = 10000;
                _ldapConnection.SearchConstraints.ServerTimeLimit = 10000;
                _ldapConnection.SearchConstraints.TimeLimit = 10000;
                _ldapConnection.ConnectionTimeout = 10000;

                if (_ldapConnection.TLS)
                {
                    _log.Debug("ldapConnection.StopTls();");
                    _ldapConnection.StopTls();
                }

                _log.Debug("ldapConnection.Disconnect();");
                _ldapConnection.Disconnect();

                _log.Debug("ldapConnection.Dispose();");
                _ldapConnection.Dispose();

                _ldapConnection = null;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("LDAP->Dispose() failed. Error: {0}", ex);
            }
        }
    }
}
