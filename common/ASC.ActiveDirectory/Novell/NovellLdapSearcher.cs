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


using ASC.ActiveDirectory.Expressions;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Studio.Utility;
using log4net;
using Mono.Security.Cryptography;
using Mono.Security.X509;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Syscert = System.Security.Cryptography.X509Certificates;

namespace ASC.ActiveDirectory.Novell
{
    public class NovellLdapSearcher
    {
        private readonly ILog log = LogManager.GetLogger(typeof(LdapSettingsChecker));
        private readonly int currentTenantID = TenantProvider.CurrentTenantID;
        private readonly NovellLdapCertificateConfirmRequest certificateConfirmRequest = new NovellLdapCertificateConfirmRequest();
        private static readonly ICache cache = AscCache.Default;
        private static readonly object rootSync = new object();

        public NovellLdapSearcher(bool acceptCertificate)
        {
            if (acceptCertificate)
            {
                certificateConfirmRequest.Approved = true;
                certificateConfirmRequest.Requested = false;
            }
        }

        public List<LDAPObject> Search(string login, string password, string server, int portNumber,
            int scope, bool startTls, Criteria criteria, string userFilter = null, string disnguishedName = null, string[] attributes = null)
        {
            if (!string.IsNullOrEmpty(userFilter) && !userFilter.StartsWith("(") && !userFilter.EndsWith(")"))
            {
                userFilter = "(" + userFilter + ")";
            }
            string searchFilter = criteria != null ? "(&" + criteria.ToString() + userFilter + ")" : userFilter;
            return Search(login, password, server, portNumber, scope, startTls, searchFilter, disnguishedName, attributes);
        }

        public List<LDAPObject> Search(string login, string password, string server, int portNumber,
            int scope, bool startTls, string searchFilter = null, string disnguishedName = null, string[] attributes = null)
        {
            if (portNumber != LdapConnection.DEFAULT_PORT && portNumber != LdapConnection.DEFAULT_SSL_PORT)
            {
                throw new SystemException("Wrong port");
            }
            if (server.StartsWith("LDAP://"))
            {
                server = server.Substring("LDAP://".Length);
            }
            var factory = new LDAPObjectFactory();
            var entries = new List<LdapEntry>();
            var ldapConnection = new LdapConnection();
            if (startTls || portNumber == LdapConnection.DEFAULT_SSL_PORT)
            {
                ldapConnection.UserDefinedServerCertValidationDelegate += ServerCertValidationHandler;
            }
            
            ldapConnection.Connect(server, portNumber);
            if (portNumber == LdapConnection.DEFAULT_SSL_PORT)
            {
                ldapConnection.SecureSocketLayer = true;
            }
            if (startTls)
            {
                // does not call stopTLS because it does not work
                ldapConnection.startTLS();
            }
            ldapConnection.Bind(LdapConnection.Ldap_V3, login, password);

            if (startTls)
            {
                string errorMessage = ServerCertValidate();
                // error in ServerCertValidationHandler
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    ldapConnection.Disconnect();
                    throw new Exception(errorMessage);
                }
            }

            // certificate confirmation requested
            if ((startTls && certificateConfirmRequest != null &&
                certificateConfirmRequest.Requested && !certificateConfirmRequest.Approved))
            {
                log.Debug("LDAP certificate confirmation requested.");
                ldapConnection.Disconnect();
                var exception = new NovellLdapTlsCertificateRequestedException
                {
                    CertificateConfirmRequest = certificateConfirmRequest
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
                string ldapUniqueIdAttribute = ConfigurationManager.AppSettings["ldap.unique.id"];
                if (ldapUniqueIdAttribute == null)
                {
                    attributes = new [] { "*", Constants.RFCLDAPAttributes.EntryDN, Constants.RFCLDAPAttributes.EntryUUID,
                        Constants.RFCLDAPAttributes.NSUniqueId, Constants.RFCLDAPAttributes.GUID };
                }
                else
                {
                    attributes = new [] { "*", ldapUniqueIdAttribute };
                }
            }
            LdapSearchConstraints ldapSearchConstraints = new LdapSearchConstraints
            {
                MaxResults = int.MaxValue,
                HopLimit = 0
            };
            LdapSearchResults ldapSearchResults = ldapConnection.Search(disnguishedName,
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
                {
                    entries.Add(nextEntry);
                }
            }
            var result = factory.CreateObjects(entries);
            ldapConnection.Disconnect();
            return result;
        }

        private bool ServerCertValidationHandler(Syscert.X509Certificate certificate, int[] certificateErrors)
        {
            foreach (int error in certificateErrors)
            {
                log.DebugFormat("ServerCertValidationHandler CertificateError: {0}", error);
            }
            lock (rootSync)
            {
                cache.Insert("ldapCertificate", certificate, DateTime.MaxValue);
                cache.Insert("ldapCertificateErrors", certificateErrors, DateTime.MaxValue);
                certificateConfirmRequest.CertificateErrors = certificateErrors;
            }
            return true;
        }

        private string ServerCertValidate()
        {
            string errorMessage = String.Empty;
            X509Store store = WorkContext.IsMono ? X509StoreManager.CurrentUser.TrustedRoot :
                X509StoreManager.LocalMachine.TrustedRoot;
            var storage = StorageFactory.GetStorage("-1", "certs");
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(currentTenantID);

                // Import the details of the certificate from the server.
                lock (rootSync)
                {
                    var certificate = cache.Get<Syscert.X509Certificate>("ldapCertificate");
                    if (certificate != null)
                    {
                        byte[] data = certificate.GetRawCertData();
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
                            if (certificateConfirmRequest.Approved)
                            {
                                AddCertificateToStorage(storage, x509);
                                // Add the certificate to the store.
                                store.Import(x509);
                                store.Certificates.Add(x509);
                                return String.Empty;
                            }
                            if (!certificateConfirmRequest.Requested)
                            {
                                certificateConfirmRequest.SerialNumber = CryptoConvert.ToHex(x509.SerialNumber);
                                certificateConfirmRequest.IssuerName = x509.IssuerName;
                                certificateConfirmRequest.SubjectName = x509.SubjectName;
                                certificateConfirmRequest.ValidFrom = x509.ValidFrom;
                                certificateConfirmRequest.ValidUntil = x509.ValidUntil;
                                certificateConfirmRequest.Hash = CryptoConvert.ToHex(x509.Hash);
                                var certificateErrors = cache.Get<int[]>("ldapCertificateErrors");
                                certificateConfirmRequest.CertificateErrors = certificateErrors.ToArray();
                                certificateConfirmRequest.Requested = true;
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
                        else
                        {
                            errorMessage = "LDAP TlsHandler. Certificate not found in certificate store.";
                            log.Error(errorMessage);
                            return errorMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = String.Format("LDAP TlsHandler error: {0}. {1}. store path = {2}",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.ToString() : string.Empty, store.Name);
                log.ErrorFormat(errorMessage);
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

        private void AddCertificateToStorage(IDataStore storage, X509Certificate x509)
        {
            var stream = new MemoryStream(x509.RawData);
            storage.DeleteDirectory("ldap");
            storage.Save("ldap/ldap.cer", stream);
        }

        private byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[4 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private bool CompareHash(byte[] hash1, byte[] hash2)
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
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
