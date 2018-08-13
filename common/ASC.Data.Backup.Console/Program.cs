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
using System.IO;
using System.Linq;
using System.Reflection;
using ASC.Core;
using ASC.Data.Backup.Logging;
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
                var webconfig = ToAbsolute(options.WebConfigPath ?? Path.Combine("..", "..", "WebStudio", "web.config"));
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

                var log = LogFactory.Create();
                if (!Path.HasExtension(options.WebConfigPath))
                {
                    options.WebConfigPath = Path.Combine(options.WebConfigPath, "web.config");
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
                    var task = new BackupPortalTask(log, tenant.TenantId, options.WebConfigPath, backupFilePath);
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
