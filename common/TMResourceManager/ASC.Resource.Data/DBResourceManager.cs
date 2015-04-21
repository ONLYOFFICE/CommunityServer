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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Web;

namespace TMResourceData
{
    public class DBResourceManager : ResourceManager
    {
        static object lockObject = new object();
        static DateTime _updateDate = DateTime.UtcNow;
        static Hashtable _resData;
        static Hashtable _resDataForTrans;

        // settings
        readonly static int updateSeconds;
        readonly static string getPagePortal;
        readonly static List<string> updatePortals;


        readonly string _fileName;
        readonly ResourceManager _resManager;

        protected ConcurrentDictionary<string, DBResourceSet> ResourceSetsTable;

        // not beforfieldInit
        static DBResourceManager()
        {
            updateSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["resources.cache-timeout"] ?? "10");
            updatePortals = (ConfigurationManager.AppSettings["resources.trans-portals"] ?? string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            getPagePortal = ConfigurationManager.AppSettings["resources.pageinfo-portal"];
        }

        public DBResourceManager(string fileName, ResourceManager resManager)
        {
            ResourceSetsTable = new ConcurrentDictionary<string, DBResourceSet>();
            _fileName = fileName;
            _resManager = resManager;
        }

        public override ResourceSet GetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            var baseCulture = culture;

            DBResourceSet databaseResourceSet;

            while (true)
            {
                if (!ResourceSetsTable.TryGetValue(culture.Name, out databaseResourceSet))
                {
                    databaseResourceSet = new DBResourceSet(_fileName, culture);
                    ResourceSetsTable.TryAdd(culture.Name, databaseResourceSet);
                }

                if (!databaseResourceSet.DataIsEmpty)
                    break;

                if (culture.Equals(CultureInfo.InvariantCulture))
                    return _resManager.GetResourceSet(baseCulture, createIfNotExists, tryParents);

                culture = culture.Parent;
            }

            if (0 < updateSeconds && DateTime.UtcNow > _updateDate.AddSeconds(2))
            {
                GetResource.UpdateDBRS(databaseResourceSet, _fileName, culture.Name, _updateDate);
                _updateDate = DateTime.UtcNow;
            }

            return databaseResourceSet;

        }

        public override string GetString(string name, CultureInfo culture)
        {
            try
            {
                var pageLink = string.Empty;
                var resDataTable = LoadData();

                try
                {
                    if (0 < updateSeconds && DateTime.UtcNow > _updateDate.AddSeconds(updateSeconds) &&
                        HttpContext.Current != null && HttpContext.Current.Request != null)
                    {
                        _updateDate = DateTime.UtcNow;

                        var uri = HttpContext.Current.Request.Url;

                        if (uri.Host.Contains("-translator") || uri.Host.Contains("we-translate") || updatePortals.Contains(uri.Host, StringComparer.InvariantCultureIgnoreCase))
                        {
                            resDataTable = LoadDataTrans();
                            GetResource.UpdateHashTable(ref resDataTable, _updateDate);
                        }

                        if (uri.Host == getPagePortal)
                        {
                            pageLink = uri.AbsolutePath;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    // ignore System.ArgumentException: Value does not fall within the expected range.
                }
                catch (Exception err)
                {
                    log4net.LogManager.GetLogger("ASC.DbRes").Error(err);
                }

                var ci = culture ?? CultureInfo.CurrentUICulture;
                while (true)
                {
                    var language = !string.IsNullOrEmpty(ci.Name) ? ci.Name : "Neutral";

                    var resdata = resDataTable[name + _fileName + language];
                    if (resdata != null)
                    {
                        if (!string.IsNullOrEmpty(pageLink))
                        {
                            GetResource.AddLink(name, _fileName, pageLink);
                        }
                        return resdata.ToString();
                    }

                    if (ci.Equals(CultureInfo.InvariantCulture))
                    {
                        break;
                    }
                    ci = ci.Parent;
                }
            }
            catch (Exception err)
            {
                log4net.LogManager.GetLogger("ASC.DbRes").Error(err);
            }

            return _resManager.GetString(name, culture);
        }


        private Hashtable LoadData()
        {
            if (_resData == null)
            {
                lock (lockObject)
                {
                    if (_resData == null)
                    {
                        _resData = GetResource.GetAllData("tmresource");
                    }
                }
            }
            return _resData;
        }

        public ResourceSet GetBaseNeutralResourceSet()
        {
            return _resManager.GetResourceSet(CultureInfo.InvariantCulture, true, false);
        }

        private Hashtable LoadDataTrans()
        {
            if (_resDataForTrans == null)
            {
                lock (lockObject)
                {
                    if (_resDataForTrans == null)
                    {
                        _resDataForTrans = GetResource.GetAllData("tmresourceTrans");
                    }
                }
            }
            return _resDataForTrans;
        }
    }
}
