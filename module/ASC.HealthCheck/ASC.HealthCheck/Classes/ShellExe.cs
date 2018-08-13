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
