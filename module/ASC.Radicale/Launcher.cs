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
using ASC.Core;


namespace ASC.Radicale
{
    public class Launcher : IServiceController
    {
        private static Process proc;
        private static ProcessStartInfo startInfo;
        private static readonly ILog Logger = LogManager.GetLogger("ASC");
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
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Stop()
        {
            StopRadicale();
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
