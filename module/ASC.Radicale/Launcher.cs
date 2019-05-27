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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using ASC.Common.Module;
using ASC.Core;
using log4net;
namespace ASC.Radicale
{
    public class Launcher: IServiceController
    {
        private static Process proc;
        private static ProcessStartInfo startInfo;
        private static readonly ILog Logger = LogManager.GetLogger("ASC");
        public void Start()
        {
            try
            {
                var cfg = (RadicaleCfgSectionHandler)ConfigurationManager.GetSection("radicale");
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
                    Arguments = string.Format("-m radicale --config \"{0}\" --logging-config \"{1}\"", 
                                                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "radicale.config")),
                                                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "radicale.log.config"))),
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
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
