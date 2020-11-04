/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Data.Backup.Tasks;
using CommandLine;

namespace ASC.Data.Backup.Console
{
    class Program
    {
        class Options
        {
            [OptionArray('t', "tenants", HelpText = "Identifiers of portals to backup")]
            public int[] TenantIds { get; set; }

            [Option('c', "config", HelpText = "Path to Teamlab configuration file")]
            public string WebConfigPath { get; set; }

            [Option('d', "dir", HelpText = "Folder in which backup files are stored")]
            public string BackupDirectory { get; set; }

            [Option('r', "restore", HelpText = "File to restore backup.")]
            public string Restore { get; set; }
        }

        static void Main(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            var options = new Options();
            Parser.Default.ParseArgumentsStrict(args, options);

            if (!string.IsNullOrWhiteSpace(options.Restore))
            {
                var webconfig = ToAbsolute(options.WebConfigPath ?? Path.Combine("..", "..", "WebStudio", "Web.config"));
                var backupfile = ToAbsolute(options.Restore);
                var backuper = new BackupManager(backupfile, webconfig);
                backuper.ProgressChanged += (s, e) =>
                {
                    var pos = System.Console.CursorTop;
                    System.Console.SetCursorPosition(0, System.Console.CursorTop);
                    System.Console.Write(new string(' ', System.Console.WindowWidth));
                    System.Console.SetCursorPosition(0, pos);
                    System.Console.Write("{0}... {1}%", e.Status, e.Progress);
                };

                System.Console.WriteLine();
                
                backuper.Load();

                System.Console.WriteLine();
            }
            else
            {
                options.WebConfigPath = ToAbsolute(options.WebConfigPath);
                options.BackupDirectory = ToAbsolute(options.BackupDirectory);

                var log = LogManager.GetLogger("ASC");
                if (!Path.HasExtension(options.WebConfigPath))
                {
                    options.WebConfigPath = Path.Combine(options.WebConfigPath, "Web.config");
                }
                if (!File.Exists(options.WebConfigPath))
                {
                    log.Error("Configuration file not found.");
                    return;
                }
                if (!Directory.Exists(options.BackupDirectory))
                {
                    Directory.CreateDirectory(options.BackupDirectory);
                }

                foreach (var tenant in options.TenantIds.Select(tenantId => CoreContext.TenantManager.GetTenant(tenantId)).Where(tenant => tenant != null))
                {
                    var backupFileName = string.Format("{0}-{1:yyyyMMddHHmmss}.zip", tenant.TenantAlias, DateTime.UtcNow);
                    var backupFilePath = Path.Combine(options.BackupDirectory, backupFileName);
                    var task = new BackupPortalTask(log, tenant.TenantId, options.WebConfigPath, backupFilePath, 100);
                    task.RunJob();
                }
            }
        }

        private static string ToAbsolute(string basePath)
        {
            if (!Path.IsPathRooted(basePath))
            {
                basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), basePath);
            }
            return basePath;
        }
    }
}
