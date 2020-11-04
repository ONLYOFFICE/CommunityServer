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
using System.Diagnostics;
using System.IO;
using System.Reflection;

using ASC.Common.Logging;
using ASC.Common.Module;

namespace ASC.Thumbnails.Svc
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
                var cfg = (ConfigHandler)ConfigurationManagerExtension.GetSection("thumb");

                startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = "node",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("\"{0}\"", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), cfg.Path, "index.js"))),
                    WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                };

                var savePath = cfg.SavePath;
                if (!savePath.EndsWith("/"))
                {
                    savePath += "/";
                }
                startInfo.EnvironmentVariables.Add("port", cfg.Port);
                startInfo.EnvironmentVariables.Add("logPath", Path.Combine(Logger.LogDirectory, "web.thumbnails.log"));
                startInfo.EnvironmentVariables.Add("savePath", Path.GetFullPath(savePath));

                StartNode();
            }
            catch (Exception e)
            {
                Logger.Error("Start", e);
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
                Logger.Error("Stop", e);
            }
        }

        private void StartNode()
        {
            Stop();
            proc = Process.Start(startInfo);
        }
    }
}