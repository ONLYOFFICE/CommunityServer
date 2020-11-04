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
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Data.Backup.Tasks;
using ASC.Notify.Cron;

namespace ASC.Data.Backup.RestoreDemo
{
    public class Service : IServiceController
    {
        private readonly static ILog log = LogManager.GetLogger("ASC");
        private readonly ManualResetEvent stop = new ManualResetEvent(false);
        private Thread worker;


        public void Start()
        {
            worker = new Thread(Execute);
            worker.Start();
        }

        public void Stop()
        {
            stop.Set();
            worker.Join();
            stop.Close();
        }

        private void Execute(object _)
        {
            try
            {
                var cronstr = ConfigurationManager.AppSettings["bill"];
                var now = cronstr == "now";

                do
                {
                    if (!now)
                    {
                        if (WaitNextValidTime(cronstr))
                        {
                            return;
                        }
                    }

                    DeleteAll();

                    Restore();

                } while (!now);

                log.Info("Stop restore");
            }
            catch (Exception error)
            {
                log.Error(error);
            }
        }

        private bool WaitNextValidTime(string cronstr)
        {
            var cron = new CronExpression(cronstr ?? "0 0 * 1 * ?");
            while (true)
            {
                var date = cron.GetNextValidTimeAfter(DateTime.UtcNow);
                log.InfoFormat("Next restore at {0}", date);
                if (!date.HasValue)
                {
                    log.Info("Stop restore.");
                    return true;
                }

                var diff = date.Value - DateTime.UtcNow;
                if (int.MaxValue < (long)diff.TotalMilliseconds)
                {
                    diff = TimeSpan.FromMilliseconds(int.MaxValue);
                }

                if (stop.WaitOne(diff))
                {
                    log.Info("Stop restore.");
                    return true;
                }

                if ((long)diff.TotalMilliseconds < int.MaxValue)
                {
                    break;
                }
            }

            return false;
        }

        private static void DeleteAll()
        {
            using (var manager = new DbManager("core"))
            {
                var tables = manager.ExecuteList("show tables").ConvertAll(r=> (string)r[0]);
                tables.RemoveAll(r => r == "tenants_quota" || r.Contains("template_") || r.Contains("res_") || r == "core_acl" || r == "core_settings" || r == "core_subscription" || r == "core_subscriptionmethod");
                
                tables.ForEach(r=> manager.ExecuteNonQuery(new SqlDelete(r)));

                manager.ExecuteNonQuery(new SqlDelete("core_acl").Where(!Exp.Eq("tenant", -1)));
                manager.ExecuteNonQuery(new SqlDelete("core_settings").Where(!Exp.Eq("tenant", -1)));
                manager.ExecuteNonQuery(new SqlDelete("core_subscription").Where(!Exp.Eq("tenant", -1)));
                manager.ExecuteNonQuery(new SqlDelete("core_subscriptionmethod").Where(!Exp.Eq("tenant", -1)));
            }
        }

        private static void Restore()
        {
            var files = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).GetFiles("*.zip");

            foreach (var file in files)
            {
                try
                {
                    var task = new RestorePortalTask(LogManager.GetLogger("ASC"), ToAbsolute("TeamLabSvc.exe.config"), file.FullName);
                    task.ProgressChanged += (sender, args) => log.Info(args.Progress);
                    task.UnblockPortalAfterCompleted = true;
                    task.ReplaceDate = true;
                    task.RunJob();
                }
                catch (Exception error)
                {
                    log.Error(error);
                }
            }
        }

        private static string ToAbsolute(string basePath)
        {
            return Path.IsPathRooted(basePath)
                       ? basePath
                       : Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), basePath);
        }
    }
}
