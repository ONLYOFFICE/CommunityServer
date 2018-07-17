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
using System.Diagnostics;
using System.IO;
using System.Linq;

using ASC.FullTextIndex.Service.Config;
using log4net;

namespace ASC.FullTextIndex.Service
{
    internal class TextSearcher
    {
        private static readonly ILog log = LogManager.GetLogger("ASC");

        private static TextSearcher instance = new TextSearcher();

        public static TextSearcher Instance
        {
            get { return instance; }
        }


        public void Start()
        {
            if (CheckIsStarted()) return;
            ClearBinlogFiles();

            try
            {
                Process.Start(GetDefaultProcessStartInfo());
            }
            catch (Exception e)
            {
                log.Error("Searchd failed start", e);
            }
        }

        public void Stop()
        {
            if (!CheckIsStarted()) return;

            try
            {
                var startInfo = GetDefaultProcessStartInfo();
                startInfo.Arguments += " --stopwait";
                var searchd = Process.Start(startInfo);
                if (searchd != null)
                {
                    searchd.WaitForExit(10000);
                }

                ClearBinlogFiles();
            }
            catch (Exception e)
            {
                log.Error("Searchd failed stop", e);
            }
        }

        private static ProcessStartInfo GetDefaultProcessStartInfo()
        {
            return new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = TextIndexCfg.SearcherName,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = string.Format("--config \"{0}\"", TextIndexCfg.ConfPath),
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };
        }

        private static bool CheckIsStarted()
        {
            try
            {
                var startInfo = GetDefaultProcessStartInfo();
                startInfo.Arguments += " --status";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                using(var searchd = Process.Start(startInfo))
                {
                    if (searchd != null && searchd.WaitForExit(1000))
                    {
                        var output = searchd.StandardOutput.ReadToEnd();
                        return !string.IsNullOrEmpty(output) && output.IndexOf("uptime:", StringComparison.Ordinal) > 0;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Searchd failed checkStatus", e);
            }
            return false;
        }

        public int[] Search(string[] modules, int tenantID)
        {
            var result = new Dictionary<string, int[]>();
            foreach (var module in modules)
            {
                try
                {
                    var ids = new List<int>();

                    var name = module.Substring(0, module.IndexOf("|"));
                    var sql = module.Substring(module.IndexOf("|") + 1);

                    var mainname = name + "_main";
                    if (1 < TextIndexCfg.Chunks)
                    {
                        var index = tenantID/TextIndexCfg.Dimension;
                        if (index >= TextIndexCfg.Chunks)
                        {
                            index = TextIndexCfg.Chunks - 1;
                        }
                        mainname = ModuleInfo.GetChunk(name, index + 1);
                    }
                    var replacedSql = sql.Replace(name, mainname);
                    ids.AddRange(DbProvider.Search(replacedSql));

                    replacedSql = sql.Replace(name, name + "_delta");
                    ids.AddRange(DbProvider.Search(replacedSql));

                    result.Add(name, ids.ToArray());
                }
                catch (Exception e)
                {
                    Start();
                    log.ErrorFormat("Searchd: search failed, module :{0}, exception:{1}", module, e.Message);
                }
            }

            return result.SelectMany(r => r.Value).Distinct().ToArray();
        }

        private static void ClearBinlogFiles()
        {
            try
            {
                foreach (var file in Directory.GetFiles(TextIndexCfg.DataPath, "binlog.*"))
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Searchd: ClearBinlogFiles failed, module :{0}", e);
            }
        }
    }
}