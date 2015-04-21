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


using log4net;
using System;
using System.Diagnostics;
using System.IO;
using ASC.FullTextIndex.Service.Config;

namespace ASC.FullTextIndex.Service
{
    class TextIndexer
    {
        private readonly ModuleInfo module;
        private readonly ILog log = LogManager.GetLogger(typeof(TextIndexer));

        public TextIndexer(ModuleInfo module)
        {
            this.module = module;

            var names = module.Name.Split('_');
            var path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TextIndexCfg.DataPath), names[0], names[1]);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void FirstTimeIndex()
        {
            var exitCode = IndexMain();
            if (exitCode == 0)
                IndexDelta();
        }

        public void Rotate()
        {
            var exitCode = IndexDelta();
            if (!(exitCode == 0 && TextSearcher.Instance.CheckDeltaIndexNotEmpty(module))) return;

            exitCode = Merge();
            if (exitCode != 0)
            {
                IndexMain();
                return;
            }

            DbProvider.UpdateLastDeltaIndexDate(module);
        }

        private int IndexMain()
        {
            return StartIndexer(string.Format("--rotate {0} --verbose", module.Main));
        }

        private int IndexDelta()
        {
            return StartIndexer(string.Format("--rotate {0} --verbose", module.Delta));
        }

        private int Merge()
        {
            return StartIndexer(string.Format("--merge {0} {1} --rotate --verbose", module.Main, module.Delta));
        }

        private int StartIndexer(string command)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = TextIndexCfg.IndexerName,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("{0} --config \"{1}\"", command, TextIndexCfg.ConfPath),
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                using (var exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                    return exeProcess.ExitCode;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error: {0}, {1}", e, e.StackTrace);
                return 1;
            }
        }

    }
}
