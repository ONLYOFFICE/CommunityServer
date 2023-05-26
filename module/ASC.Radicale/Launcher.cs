/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Core;
using ASC.Core.Common.Contracts;

using LogManager = ASC.Common.Logging.BaseLogManager;

namespace ASC.Radicale
{
    public class Launcher : IServiceController
    {
        private static Process proc;
        private static ProcessStartInfo startInfo;
        private static readonly ILog Logger = LogManager.GetLogger("ASC");
        private const string ResultOfPing = "Radicale works!";
        private HealthCheckSvc HealthCheckSvc;
        public void Start()
        {
            try
            {
                var cfg = (RadicaleCfgSectionHandler)ConfigurationManagerExtension.GetSection("radicale");
                var pythonName = "python";

                if (WorkContext.IsMono)
                {
                    pythonName = "python3";
                }

                startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = pythonName,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("-m radicale --config \"{0}\"",
                                                Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "radicale.config"))),
                    WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                };

                StartRedicale();
                var appSettings = ConfigurationManagerExtension.AppSettings;
                HealthCheckSvc = new HealthCheckSvc (cfg.Port, ResultOfPing, Logger);
                HealthCheckSvc.StartPing();


                var checkRadicaleVersion = CheckRadicaleVersion();

                if (!checkRadicaleVersion) Stop();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private static bool CheckRadicaleVersion()
        {
            ProcessStartInfo start = new ProcessStartInfo();

            var pythonName = "pip";

            if (WorkContext.IsMono)
            {
                pythonName = "pip";
            }
            var isSuccess = true;

            start.FileName = pythonName;
            start.Arguments = "show radicale app_store_plugin app_auth_plugin app_right_plugin";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        if (result != null)
                        {
                            string[] plugins = result.Split(new string[] { "---" }, StringSplitOptions.None);
                            foreach (string plugin in plugins)
                            {
                                const string namePattern = "Name: (.*)\n";
                                const string versionPattern = "Version: (.*)\n";
                                Regex rgxName = new Regex(namePattern);
                                Regex rgxVersion = new Regex(versionPattern);
                                Match matchName = rgxName.Match(plugin);

                                if (matchName.Groups.Count > 1)
                                {
                                    Match matchVersion = rgxVersion.Match(plugin);
                                    switch (matchName.Groups[1].Value.Trim())
                                    {
                                        case "Radicale":
                                            if (matchVersion.Groups[1].Value.Split('.')[0] != "3")
                                            {
                                                Logger.Error("Only Radicale 3 version supported");
                                                isSuccess = false;
                                            }
                                            break;
                                        case "app-store-plugin":
                                        case "app-auth-plugin":
                                        case "app-right-plugin":
                                           
                                            if (matchVersion.Groups[1].Value.Trim() != "1.0.0")
                                            {
                                                Logger.Error("Required " + matchName.Groups[1].Value.ToLower() + " plugin version 1.0.0");
                                                isSuccess = false;
                                            }
                                            break;
                                    }
                                }
                            }
                            return isSuccess;
                        }
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return false;
            }
        }

        public void Stop()
        {
            StopRadicale();
            HealthCheckSvc.StopPing();
        }

        private static void StartRedicale()
        {
            StopRadicale();
            proc = Process.Start(startInfo);
        }

        private static void StopRadicale()
        {
            try
            {
                if (proc != null && !proc.HasExited)
                {
                    proc.Kill();
                    if (!proc.WaitForExit(10000))
                    {
                        Logger.Warn("The process does not wait for completion.");
                    }
                    proc.Close();
                    proc.Dispose();
                    proc = null;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Radicale failed stop", e);
            }
        }
    }
}
