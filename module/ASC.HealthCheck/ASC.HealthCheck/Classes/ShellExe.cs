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


using log4net;
using System;
using System.Diagnostics;

namespace ASC.HealthCheck.Classes
{
    public class ShellExe
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShellExe));
        private const int MonoWaitTimeout = 15000;

        public Process RunBinFile(string command, string arguments)
        {
            try
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = command,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForInputIdle();
                process.WaitForExit(MonoWaitTimeout);
                if (process.HasExited == false)
                {
                    if (process.Responding)
                    {
                        process.CloseMainWindow();
                    }
                    else
                    {
                        process.Kill();
                    }
                }
                return process;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while executing command \"{0}\" : {1}", command + " " + arguments, ex);
                throw;
            }
        }

        public string ExecuteCommand(string programName, params string[] parameters)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = programName,
                    Arguments = string.Join(" ", parameters),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var proc = Process.Start(startInfo))
                {
                    proc.WaitForExit(MonoWaitTimeout);
                    var error = proc.StandardError.ReadToEnd();
                    return !string.IsNullOrEmpty(error) ? error : proc.StandardOutput.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while executing command \"{0}\" : {1}", programName + " " + parameters, ex);
                throw;
            }
        }
    }
}
