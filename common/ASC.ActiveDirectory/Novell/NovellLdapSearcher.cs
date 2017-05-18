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
using ASC.ActiveDirectory.DirectoryServices;
using ASC.ActiveDirectory.Expressions;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Data.Storage;
using log4net;
using Mono.Security.Cryptography;
using Mono.Security.X509;
using Novell.Directory.Ldap;
using Syscert = System.Security.Cryptography.X509Certificates;

namespace ASC.ActiveDirectory.Novell
{
    public class NovellLdapSearcher
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(LdapSettingsChecker));
        private readonly int _currentTenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
        private readonly NovellLdapCertificateConfirmRequest _certificateConfirmRequest = new NovellLdapCertificateConfirmRequest();
        private static readonly ICache Cache = AscCache.Default;
        private static readonly object RootSync = new object();

        public NovellLdapSearcher(bool acceptCertificate)
        {
            if (!acceptCertificate)
                return;

            _certificateConfirmRequest.Approved = true;
            _certificateConfirmRequest.Requested = false;
        }

        public List<LDAPObject> Search(string login, string password, string server, int portNumber,
                                       int scope, bool startTls, Criteria criteria, string userFilter = null,
                                       string disnguishedName = null, string[] attributes = null)
        {
            if (!string.IsNullOrEmpty(userFilter) && !userFilter.StartsWith("(") && !userFilter.EndsWith(")"))
                userFilter = "(" + userFilter + ")";

            var searchFilter = criteria != null ? "(&" + criteria + userFilter + ")" : userFilter;
            return Search(login, password, server, portNumber, scope, startTls, searchFilter, disnguishedName,
                          attributes);
        }

        public List<LDAPObject> Search(string login, string password, string server, int portNumber,
                                       int scope, bool startTls, string searchFilter = null,
                                       string disnguishedName = null, string[] attributes = null)
        {
            if (portNumber != LdapConnection.DEFAULT_PORT && portNumber != LdapConnection.DEFAULT_SSL_PORT)
                throw new SystemException("Wrong port");

            if (server.StartsWith("LDAP://"))
                server = server.Substring("LDAP://".Length);

            var entries = new List<LdapEntry>();
            var ldapConnection = new LdapConnection();

            if (startTls || portNumber == LdapConnection.DEFAULT_SSL_PORT)
                ldapConnection.UserDefinedServerCertValidationDelegate += ServerCertValidationHandler;

            ldapConnection.Connect(server, portNumber);

            if (portNumber == LdapConnection.DEFAULT_SSL_PORT)
                ldapConnection.SecureSocketLayer = true;

            if (startTls) // does not call stopTLS because it does not work
                ldapConnection.startTLS();

            ldapConnection.Bind(LdapConnection.Ldap_V3, login, password);

            if (startTls)
            {
                var errorMessage = ServerCertValidate();
                // error in ServerCertValidationHandler
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ldapConnection.Disconnect();
                    throw new Exception(errorMessage);
                }
            }

            // certificate confirmation requested
            if ((startTls && _certificateConfirmRequest != null &&
                 _certificateConfirmRequest.Requested && !_certificateConfirmRequest.Approved))
            {

                _log.Debug("LDAP certificate confirmation requested.");

                ldapConnection.Disconnect();

                var exception = new NovellLdapTlsCertificateRequestedException
                    {
                        CertificateConfirmRequest = _certificateConfirmRequest
                    };

                throw exception;
            }

            if (searchFilter == null)
            {
                ldapConnection.Disconnect();
                return null;
            }

            if (attributes == null)
            {
                var ldapUniqueIdAttribute = ConfigurationManager.AppSettings["ldap.unique.id"];

                if (ldapUniqueIdAttribute == null)
                {
                    attributes = new[]
                        {
                            "*", Constants.RfcLDAPAttributes.ENTRY_DN, Constants.RfcLDAPAttributes.ENTRY_UUID,
                            Constants.RfcLDAPAttributes.NS_UNIQUE_ID, Constants.RfcLDAPAttributes.GUID
                        };
                }
                else
                {
                    attributes = new[] { "*", ldapUniqueIdAttribute };
                }
            }

            var ldapSearchConstraints = new LdapSearchConstraints
                {
                    MaxResults = int.MaxValue,
                    HopLimit = 0
                };

            var ldapSearchResults = ldapConnection.Search(disnguishedName,
                                                          scope, searchFilter, attributes, false, ldapSearchConstraints);

            while (ldapSearchResults.hasMore())
            {
                LdapEntry nextEntry;
                try
                {
                    nextEntry = ldapSearchResults.next();
                }
                catch (LdapException)
                {
                    continue;
                }

                if (nextEntry != null)
                    entries.Add(nextEntry);
            }

            var result = LDAPObjectFactory.CreateObjects(entries);

            ldapConnection.Disconnect();

            return result;
        }

        private bool ServerCertValidationHandler(Syscert.X509Certificate certificate, int[] certificateErrors)
        {
            foreach (var error in certificateErrors)
            {
                _log.DebugFormat("ServerCertValidationHandler CertificateError: {0}", error);
            }

            lock (RootSync)
            {
                Cache.Insert("ldapCertificate", certificate, DateTime.MaxValue);
                Cache.Insert("ldapCertificateErrors", certificateErrors, DateTime.MaxValue);
                _certificateConfirmRequest.CertificateErrors = certificateErrors;
            }

            return true;
        }

        private string ServerCertValidate()
        {
            var errorMessage = String.Empty;

            var store = WorkContext.IsMono
                            ? X509StoreManager.CurrentUser.TrustedRoot
                            : X509StoreManager.LocalMachine.TrustedRoot;

            var storage = StorageFactory.GetStorage("-1", "certs");

            try
            {
                CoreContext.TenantManager.SetCurrentTenant(_currentTenantId);

                // Import the details of the certificate from the server.
                lock (RootSync)
                {
                    var certificate = Cache.Get<Syscert.X509Certificate>("ldapCertificate");
                    if (certificate != null)
                    {
                        var data = certificate.GetRawCertData();
                        var x509 = new X509Certificate(data);
                        // Check for ceritficate in store.
                        if (!store.Certificates.Contains(x509))
                        {
                            if (storage.IsFile("ldap/ldap.cer"))
                            {
                                var storageData = GetCertificateFromStorage(storage);
                                var storageX509 = new X509Certificate(storageData);
                                if (CompareHash(storageX509.Hash, x509.Hash))
                                {
                                    // Add the certificate to the store.
                                    store.Import(storageX509);
                                    store.Certificates.Add(storageX509);
                                    return String.Empty;
                                }
                            }
                            if (_certificateConfirmRequest.Approved)
                            {
                                AddCertificateToStorage(storage, x509);
                                // Add the certificate to the store.
                                store.Import(x509);
                                store.Certificates.Add(x509);
                                return String.Empty;
                            }
                            if (!_certificateConfirmRequest.Requested)
                            {
                                _certificateConfirmRequest.SerialNumber = CryptoConvert.ToHex(x509.SerialNumber);
                                _certificateConfirmRequest.IssuerName = x509.IssuerName;
                                _certificateConfirmRequest.SubjectName = x509.SubjectName;
                                _certificateConfirmRequest.ValidFrom = x509.ValidFrom;
                                _certificateConfirmRequest.ValidUntil = x509.ValidUntil;
                                _certificateConfirmRequest.Hash = CryptoConvert.ToHex(x509.Hash);
                                var certificateErrors = Cache.Get<int[]>("ldapCertificateErrors");
                                _certificateConfirmRequest.CertificateErrors = certificateErrors.ToArray();
                                _certificateConfirmRequest.Requested = true;
                            }
                        }
                    }
                    else
                    {
                        // for AD
                        if (storage.IsFile("ldap/ldap.cer"))
                        {
                            var storageData = GetCertificateFromStorage(storage);
                            var storageX509 = new X509Certificate(storageData);
                            // Add the certificate to the store.
                            store.Import(storageX509);
                            store.Certificates.Add(storageX509);
                            return String.Empty;
                        }

                        errorMessage = "LDAP TlsHandler. Certificate not found in certificate store.";

                        _log.Error(errorMessage);

                        return errorMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = String.Format("LDAP TlsHandler error: {0}. {1}. store path = {2}",
                                             ex, ex.InnerException != null ? ex.InnerException.ToString() : string.Empty,
                                             store.Name);
                _log.ErrorFormat(errorMessage);
            }

            return errorMessage;
        }

        private byte[] GetCertificateFromStorage(IDataStore storage)
        {
            using (var stream = storage.GetReadStream("ldap/ldap.cer"))
            {
                return ReadFully(stream);
            }
        }

        private static void AddCertificateToStorage(IDataStore storage, X509Certificate x509)
        {
            var stream = new MemoryStream(x509.RawData);
            storage.DeleteDirectory("ldap");
            storage.Save("ldap/ldap.cer", stream);
        }

        private static byte[] ReadFully(Stream input)
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

        private static bool CompareHash(byte[] hash1, byte[] hash2)
        {
            if ((hash1 == null) && (hash2 == null))
            {
                return true;
            }
            if ((hash1 == null) || (hash2 == null))
            {
                return false;
            }
            if (hash1.Length != hash2.Length)
            {
                return false;
            }
            return !hash1.Where((t, i) => t != hash2[i]).Any();
        }
    }
}
