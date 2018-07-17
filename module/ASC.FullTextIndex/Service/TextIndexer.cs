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


using ASC.FullTextIndex.Service.Config;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ASC.FullTextIndex.Service
{
    class TextIndexer
    {
        protected readonly ModuleInfo Module;
        protected readonly TextIndexerCommand Indexer;

        public TextIndexer(ModuleInfo module)
        {
            Module = module;
            Indexer = new TextIndexerCommand();

            var names = module.Name.Split('_');
            var path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TextIndexCfg.DataPath), names[0], names[1]);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public virtual int RotateMain()
        {
            return Indexer.Rotate(Module.Main);
        }

        public int RotateDelta()
        {
            return Indexer.Rotate(Module.Delta);
        }

        public virtual int Merge()
        {
            var tenantids = DbProvider.GetDeltaTenantId(Module);
            if (!tenantids.Any()) return 100;

            var exitCode = Indexer.Merge(Module.Main, Module.Delta);
            if (exitCode != 0)
            {
                exitCode = Indexer.Rotate(Module.Main);
            }
            return exitCode;
        }
    }

    class TextIndexerDistributed : TextIndexer
    {
        public TextIndexerDistributed(ModuleInfo module)
            : base(module)
        {
        }


        public override int RotateMain()
        {
            var result = 0;
            for (var part = 1; part <= TextIndexCfg.Chunks; part ++)
            {
                result += Indexer.Rotate(Module.GetChunk(part));
                if (result == 1) break;
            }
            return result;
        }

        public override int Merge()
        {
            var chunks = GetChunksForMerge();
            if (!chunks.Any()) return 100;

            var result = 0;
            foreach (var part in chunks)
            {
                var exitCode = Indexer.Merge(part, Module.Delta);
                if (exitCode != 0)
                {
                    exitCode = Indexer.Rotate(part);
                }
                result += exitCode;
            }

            return result;
        }

        private IEnumerable<string> GetChunksForMerge()
        {
            var result = new HashSet<string>();

            for (var i = 1; i <= TextIndexCfg.Chunks; i++)
            {
                result.Add(Module.GetChunk(i));
            }

            return result;
        }
    }

    class TextIndexerCommand
    {
        private readonly ILog log = LogManager.GetLogger(typeof(TextIndexerCommand));

        public int Merge(string main, string delta)
        {
            log.Debug("Merge");
            var exitCode = Start(string.Format("--merge {0} {1} --rotate --verbose --sighup-each", main, delta));
            log.DebugFormat("Merge {0} and {1}, exitCode: {2}", main, delta, exitCode);
            return exitCode;
        }

        public int Rotate(string indexName)
        {
            log.Debug("Rotate");
            var exitCode = Start(string.Format("--rotate {0} --verbose --sighup-each", indexName));
            log.DebugFormat("Rotate {0}, exitCode: {1}", indexName, exitCode);
            return exitCode;
        }

        public int Start(string command)
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
