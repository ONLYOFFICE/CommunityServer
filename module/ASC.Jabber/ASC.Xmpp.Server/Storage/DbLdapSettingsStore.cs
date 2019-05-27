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
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

using ASC.Common.Data.Sql;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Security.Cryptography;

namespace ASC.Xmpp.Server.Storage
{
    class DbLdapSettingsStore : DbStoreBase
    {
        private readonly ILog log = LogManager.GetLogger("ASC");
        private readonly Dictionary<string, string> properties = new Dictionary<string, string>(1);
        private const string LDAP_SETTINGS_ID = "197149b3-fbc9-44c2-b42a-232f7e729c16";

        public bool EnableLdapAuthentication { get; private set; }

        public string Server { get; private set; }

        public int PortNumber { get; private set; }

        public string UserDN { get; private set; }

        public string LoginAttribute { get; private set; }

        public bool Authentication { get; private set; }

        public string Login { get; private set; }

        public string Password { get; private set; }

        public bool StartTls { get; private set; }

        public DbLdapSettingsStore()
        {
            properties["connectionStringName"] = "core";
            base.Configure(properties);
        }

        public void GetLdapSettings(string domain)
        {
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
                            var jsSerializer = new JavaScriptSerializer();
                            var settingsDictionary = (Dictionary<string, object>)jsSerializer.DeserializeObject(stringSettings);
                            EnableLdapAuthentication = Convert.ToBoolean(settingsDictionary["EnableLdapAuthentication"]);
                            Server = Convert.ToString(settingsDictionary["Server"]);
                            PortNumber = Convert.ToInt32(settingsDictionary["PortNumber"]);
                            UserDN = Convert.ToString(settingsDictionary["UserDN"]);
                            LoginAttribute = Convert.ToString(settingsDictionary["LoginAttribute"]);
                            Authentication = Convert.ToBoolean(settingsDictionary["Authentication"]);
                            Login = Convert.ToString(settingsDictionary["Login"]);
                            Password = GetPassword(settingsDictionary["PasswordBytes"]);
                            StartTls = Convert.ToBoolean(settingsDictionary["StartTls"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private string GetPassword(object passwordBytesObject)
        {
            string password = string.Empty;
            try
            {
                if (passwordBytesObject != null)
                {
                    object[] passwordBytesObjects = (object[])passwordBytesObject;
                    byte[] passwordBytes = new byte[passwordBytesObjects.Length];
                    for (int i = 0; i < passwordBytesObjects.Length; i++)
                    {
                        passwordBytes[i] = Convert.ToByte(passwordBytesObjects[i]);
                    }
                    if (passwordBytes.Length != 0)
                    {
                        password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(passwordBytes));
                    }
                }
            }
            catch (Exception ex)
            {
                password = string.Empty;
                log.ErrorFormat("Can't decrypt password {0}, {1}", ex.ToString(), ex.StackTrace);
            }
            return password;
        }
    }
}
