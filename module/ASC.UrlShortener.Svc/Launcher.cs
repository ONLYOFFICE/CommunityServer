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
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using ASC.Common.Logging;
using ASC.Common.Module;

namespace ASC.UrlShortener.Svc
{
    public class Launcher : IServiceController
    {
        private ProcessStartInfo startInfo;
        private Process proc;
        private static readonly ILog Logger = LogManager.GetLogger("ASC");

        public void Start()
        {
            try
            {
                var cfg = (ConfigHandler)ConfigurationManagerExtension.GetSection("urlshortener");

                startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = "node",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("\"{0}\"", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), cfg.Path, "index.js"))),
                    WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                };

                var appSettings = ConfigurationManagerExtension.AppSettings;

                startInfo.EnvironmentVariables.Add("core.machinekey", appSettings["core.machinekey"]);
                startInfo.EnvironmentVariables.Add("port", cfg.Port);

                var conString = ConfigurationManager.ConnectionStrings["default"].ConnectionString;

                var dict = new Dictionary<string, string>
                {
                    {"Server", "host"},
                    {"Database", "database"},
                    {"User ID", "user"},
                    {"Password", "password"}
                };

                foreach (var conf in conString.Split(';'))
                {
                    var splited = conf.Split('=');
                    if (splited.Length < 2) continue;

                    if (dict.ContainsKey(splited[0]))
                    {
                        startInfo.EnvironmentVariables.Add("sql:" + dict[splited[0]], splited[1]);
                    }
                }

                startInfo.EnvironmentVariables.Add("logPath", Path.Combine(Logger.LogDirectory, "web.urlshortener.log"));

                StartNode();
            }
            catch (Exception e)
            {
                Logger.Fatal("Start", e);
            }
        }

        public void Stop()
        {
            try
            {
                if (proc != null && !proc.HasExited)
                {
                    proc.Kill();
                    proc.WaitForExit(10000);

                    proc.Close();
                    proc.Dispose();
                    proc = null;
                }
            }
            catch (Exception e)
            {
                Logger.Error("stop", e);
            }
        }

        private void StartNode()
        {
            Stop();
            proc = Process.Start(startInfo);
        }
    }
}
