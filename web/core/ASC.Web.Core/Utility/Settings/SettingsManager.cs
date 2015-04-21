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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Core.Caching;
using ASC.Web.Studio.Utility;
using log4net;

namespace ASC.Web.Core.Utility.Settings
{
    public class SettingsManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SettingsManager));

        private readonly ICache cache;
        private readonly TimeSpan expirationTimeout;
        private readonly IDictionary<Type, DataContractJsonSerializer> jsonSerializers;
        private readonly string dbId = "webstudio";


        public static SettingsManager Instance
        {
            get;
            private set;
        }


        static SettingsManager()
        {
            Instance = new SettingsManager();
        }

        private SettingsManager()
        {
            cache = AscCache.Default;
            expirationTimeout = TimeSpan.FromMinutes(5);
            jsonSerializers = new Dictionary<Type, DataContractJsonSerializer>();
        }


        public bool SaveSettings<T>(T settings, int tenantID) where T : ISettings
        {
            return SaveSettingsFor<T>(settings, tenantID, Guid.Empty);
        }

        public bool SaveSettingsFor<T>(T settings, Guid userID) where T : ISettings
        {
            return SaveSettingsFor<T>(settings, TenantProvider.CurrentTenantID, userID);
        }

        public T LoadSettings<T>(int tenantID) where T : ISettings
        {
            return LoadSettingsFor<T>(tenantID, Guid.Empty);
        }

        public T LoadSettingsFor<T>(Guid userID) where T : ISettings
        {
            return LoadSettingsFor<T>(TenantProvider.CurrentTenantID, userID);
        }


        private bool SaveSettingsFor<T>(T settings, int tenantID, Guid userID) where T : ISettings
        {
            if (settings == null) throw new ArgumentNullException("settings");
            try
            {
                using (var db = GetDbManager())
                {
                    var data = Serialize(settings);
                    var defaultData = Serialize(settings.GetDefault());

                    ISqlInstruction i;
                    if (data.SequenceEqual(defaultData))
                    {
                        // remove default settings
                        i = new SqlDelete("webstudio_settings")
                            .Where("id", settings.ID.ToString())
                            .Where("tenantid", tenantID)
                            .Where("userid", userID.ToString());
                    }
                    else
                    {
                        i = new SqlInsert("webstudio_settings", true)
                            .InColumnValue("id", settings.ID.ToString())
                            .InColumnValue("userid", userID.ToString())
                            .InColumnValue("tenantid", tenantID)
                            .InColumnValue("data", data);
                    }
                    db.ExecuteNonQuery(i);
                }
                ToCache(tenantID, userID, settings);
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        private T LoadSettingsFor<T>(int tenantID, Guid userID) where T : ISettings
        {
            var settingsInstance = (ISettings)Activator.CreateInstance<T>();
            var settings = FromCache(settingsInstance.ID, tenantID, userID);
            
            if (settings != null)
            {
                return (T)settings;
            }

            try
            {
                using (var db = GetDbManager())
                {
                    var q = new SqlQuery("webstudio_settings")
                        .Select("data")
                        .Where("id", settingsInstance.ID.ToString())
                        .Where("tenantid", tenantID)
                        .Where("userid", userID.ToString());
                    
                    var result = db.ExecuteScalar<object>(q);
                    if (result != null)
                    {
                        var data = result is string ? Encoding.UTF8.GetBytes((string)result) : (byte[])result;
                        settings = Deserialize<T>(data);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            if (settings == null)
            {
                settings = settingsInstance.GetDefault();
            }
            if (settings == null)
            {
                throw new InvalidOperationException(string.Format("Default settings of type '{0}' can not be null.", typeof(T)));
            }

            ToCache(tenantID, userID, settings);
            return (T)settings;
        }

        private ISettings Deserialize<T>(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var settings = data[0] == 0 ?
                    new BinaryFormatter().Deserialize(stream) :
                    GetJsonSerializer(typeof(T)).ReadObject(stream);
                return (ISettings)settings;
            }
        }

        private byte[] Serialize(ISettings settings)
        {
            using (var stream = new MemoryStream())
            {
                GetJsonSerializer(settings.GetType()).WriteObject(stream, settings);
                return stream.ToArray();
            }
        }

        private void ToCache(int tenantID, Guid userID, ISettings settings)
        {
            var key = settings.ID.ToString() + tenantID.ToString() + userID.ToString();
            cache.Insert(key, settings, DateTime.UtcNow.Add(expirationTimeout));
        }

        private ISettings FromCache(Guid settingsID, int tenantID, Guid userID)
        {
            var key = settingsID.ToString() + tenantID.ToString() + userID.ToString();
            return cache.Get(key) as ISettings;
        }

        private DbManager GetDbManager()
        {
            return DbManager.FromHttpContext(dbId);
        }

        private DataContractJsonSerializer GetJsonSerializer(Type type)
        {
            lock (jsonSerializers)
            {
                if (!jsonSerializers.ContainsKey(type))
                {
                    jsonSerializers[type] = new DataContractJsonSerializer(type);
                }
                return jsonSerializers[type];
            }
        }
    }
}
