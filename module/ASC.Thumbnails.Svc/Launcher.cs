/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
                var cfg = (ConfigHandler)ConfigurationManager.GetSection("thumb");

                startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = "node",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("\"{0}\"", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cfg.Path, "index.js"))),
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
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
            catch(Exception e)
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
            catch(Exception e)
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