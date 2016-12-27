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


extern alias MonoAlias;
using ASC.Core;
using ASC.Data.Storage;
using log4net;
using MonoAlias.Mono.Security.X509;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Events.Edir;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Syscert = System.Security.Cryptography.X509Certificates;

namespace ASC.Xmpp.Server.Authorization
{
    public class NovellLdapHelper : ILdapHelper
    {
        private readonly ILog log = LogManager.GetLogger(typeof(NovellLdapHelper));
        private static readonly object rootSync = new object();
        private static Syscert.X509Certificate certificate;

        public string GetAccountNameBySid(string sid, bool authentication, string login,
            string password, string server, int portNumber, string userDN, string loginAttribute, bool startTls)
        {
            try
            {
                // call static constructor of MonitorEventRequest class
                MonitorEventRequest.RegisterResponseTypes = true;

                string ldapUniqueIdAttribute = ConfigurationManager.AppSettings["ldap.unique.id"];
                List<LdapEntry> entryList = null;
                string searchFilter;
                var attributes = new string[] { loginAttribute };
                if (ldapUniqueIdAttribute == null)
                {
                    searchFilter = "(entryUUID=" + sid + ")";
                    entryList = Search(login, password, server, portNumber, startTls,
                        LdapConnection.SCOPE_SUB, searchFilter, userDN, attributes);
                    if (entryList.Count == 0)
                    {
                        searchFilter = "(nsuniqueid=" + sid + ")";
                        entryList = Search(login, password, server, portNumber, startTls,
                            LdapConnection.SCOPE_SUB, searchFilter, userDN, attributes);
                        if (entryList.Count == 0)
                        {
                            searchFilter = "(GUID=" + sid + ")";
                            entryList = Search(login, password, server, portNumber, startTls,
                                LdapConnection.SCOPE_SUB, searchFilter, userDN, attributes);
                            if (entryList.Count == 0)
                            {
                                searchFilter = "(objectSid=" + sid + ")";
                                entryList = Search(login, password, server, portNumber, startTls,
                                    LdapConnection.SCOPE_SUB, searchFilter, userDN, attributes);
                            }
                        }
                    }
                }
                else
                {
                    searchFilter = "(" + ldapUniqueIdAttribute + "=" + sid + ")";
                    entryList = Search(login, password, server, portNumber, startTls,
                                    LdapConnection.SCOPE_SUB, searchFilter, userDN, attributes);
                }

                if (entryList.Count != 0)
                {
                    foreach (var entry in entryList)
                    {
                        if (entry != null)
                        {
                            return entry.DN;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Can not access to objectSid: {0}. login = {1}, server = {2}, portNumber = {3}, userDN = {4}, {5}",
                    sid, login, server, portNumber, userDN, ex);
            }
            return null;
        }

        public bool CheckCredentials(string login, string password, string server, int portNumber, string settingsLogin, bool startTls)
        {
            try
            {
                if (settingsLogin.Contains("\\"))
                {
                    string shortDomain = settingsLogin.Split('\\')[0];
                    login = shortDomain + "\\" + login;
                }
                Search(login, password, server, portNumber, startTls, LdapConnection.SCOPE_BASE);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Check credentials problem. login = {0}, server = {1}, portNumber = {2} {3}",
                    login, server, portNumber, ex);
            }
            return false;
        }

        private List<LdapEntry> Search(string login, string password, string server, int portNumber, bool startTls,
            int scope, string searchFilter = null, string disnguishedName = null, string[] attributes = null)
        {
            if (server.StartsWith("LDAP://"))
            {
                server = server.Substring("LDAP://".Length);
            }
            var entries = new List<LdapEntry>();
            var ldapConnection = new LdapConnection();
            if (startTls)
            {
                ldapConnection.UserDefinedServerCertValidationDelegate += TlsHandler;
            }
            ldapConnection.Connect(server, portNumber);
            if (startTls)
            {
                // does not call stopTLS because it does not work
                ldapConnection.startTLS();
            }
            ldapConnection.Bind(LdapConnection.Ldap_V3, login, password);
            if (startTls)
            {
                string errorMessage = ServerCertValidate();
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    ldapConnection.Disconnect();
                    throw new Exception(errorMessage);
                }
            }
            if (searchFilter == null)
            {
                ldapConnection.Disconnect();
                return null;
            }
            LdapSearchResults ldapSearchResults =
                ldapConnection.Search(disnguishedName, scope, searchFilter, attributes, false);
            while (ldapSearchResults.hasMore())
            {
                LdapEntry nextEntry = null;
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
            ldapConnection.Disconnect();
            return entries;
        }

        private bool TlsHandler(Syscert.X509Certificate crt, int[] certificateErrors)
        {
            lock (rootSync)
            {
                certificate = crt;
            }
            return true;
        }

        private string ServerCertValidate()
        {
            string errorMessage = String.Empty;
            X509Store store = WorkContext.IsMono ? X509StoreManager.CurrentUser.TrustedRoot :
                    X509StoreManager.LocalMachine.TrustedRoot;
            try
            {
                var storage = StorageFactory.GetStorage("-1", "certs");
                // Import the details of the certificate from the server.
                X509Certificate x509 = null;
                lock (rootSync)
                {
                    if (certificate != null)
                    {
                        var data = certificate.GetRawCertData();
                        x509 = new X509Certificate(data);
                    }
                }
                if (x509 != null)
                {
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
                        errorMessage = String.Format("LDAP TlsHandler. Certificate not found in certificate store. {0}.", x509.IssuerName);
                        log.Error(errorMessage);
                        return errorMessage;
                    }
                    log.DebugFormat("LDAP TlsHandler. Certificate found in certificate store. {0}.", x509.IssuerName);
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
                return String.Empty;
            }
            catch (Exception ex)
            {
                errorMessage = String.Format("LDAP TlsHandler. Error: {0}. {1}.",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.ToString() : string.Empty);
                log.Error(errorMessage);
                return errorMessage;
            }
        }

        private byte[] GetCertificateFromStorage(IDataStore storage)
        {
            var stream = storage.GetReadStream("ldap/ldap.cer");
            return ReadFully(stream);
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
