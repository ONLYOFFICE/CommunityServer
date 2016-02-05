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
using System.Diagnostics;
using System.Linq;
using ASC.FullTextIndex.Service.Config;
using log4net;

namespace ASC.FullTextIndex.Service
{
    internal class TextSearcher
    {
        private static TextSearcher instance;
        public static TextSearcher Instance
        {
            get
            {
                return instance ?? (instance = new TextSearcher());
            }
        }

        private Process searchd;
        private readonly ILog log = LogManager.GetLogger("ASC");

        public void Start()
        {
            if(searchd != null && !searchd.HasExited) return;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = TextIndexCfg.SearcherName,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("--config \"{0}\"", TextIndexCfg.ConfPath),
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                searchd = Process.Start(startInfo);
            }
            catch (Exception e)
            {
                log.Error("Searchd failed stop", e);
            }
        }

        public void Stop()
        {
            if (searchd == null) return;

            try
            {
                searchd.Kill();
                searchd.WaitForExit();
                searchd.Close();
                searchd.Dispose();
                searchd = null;
            }
            catch (Exception e)
            {
                log.Error("Searchd failed stop", e);
            }
        }

        public Dictionary<string, IEnumerable<int>> Search(IEnumerable<ModuleInfo> modules, int tenantID)
        {
            var result = new Dictionary<string, IEnumerable<int>>();
            foreach (var module in modules)
            {
                try
                {
                    var ids = new List<int>();

                    var temp = module.SqlQuery;

                    module.SqlQuery = temp.Replace(module.Name, TextIndexCfg.Chunks > 1 ? module.GetChunkByTenantId(tenantID, TextIndexCfg.Chunks, TextIndexCfg.Dimension) : module.Main);                    
                    ids.AddRange(DbProvider.Search(module));

                    module.SqlQuery = temp.Replace(module.Name, module.Delta);
                    ids.AddRange(DbProvider.Search(module));

                    result.Add(module.Name, ids);
                }
                catch (Exception e)
                {
                    Start();
                    log.ErrorFormat("Searchd: search failed, module :{0}, exception:{1}", module.Name, e.Message);
                }
            }

            return result;
        }
    }
}
