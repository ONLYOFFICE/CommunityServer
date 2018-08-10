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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Module;
using ASC.Core;
using ASC.Core.Notify.Signalr;
using log4net;
using WebSocketSharp;

namespace ASC.Socket.IO.Svc
{
    public class Launcher : IServiceController
    {
        private static int retries;
        private static readonly int maxretries = 10;
        private static Process proc;
        private static ProcessStartInfo startInfo;
        private static WebSocket webSocket;
        private static CancellationTokenSource cancellationTokenSource;
        private const int PingInterval = 10000;
        private static readonly ILog Logger = LogManager.GetLogger("ASC");

        public void Start()
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();

                var cfg = (SocketIOCfgSectionHandler) ConfigurationManager.GetSection("socketio");

                startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = "node",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("\"{0}\"", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cfg.Path, "app.js"))),
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                var appSettings = ConfigurationManager.AppSettings;

                startInfo.EnvironmentVariables.Add("core.machinekey", appSettings["core.machinekey"]);
                startInfo.EnvironmentVariables.Add("port", cfg.Port);

                if (cfg.Redis != null && !string.IsNullOrEmpty(cfg.Redis.Host) && !string.IsNullOrEmpty(cfg.Redis.Port))
                {
                    startInfo.EnvironmentVariables.Add("redis:host", cfg.Redis.Host);
                    startInfo.EnvironmentVariables.Add("redis:port", cfg.Redis.Port);
                }

                if (CoreContext.Configuration.Standalone)
                {
                    startInfo.EnvironmentVariables.Add("portal.internal.url", "http://localhost");
                }

                var appender = LogManager.GetRepository().GetAppenders().Where(r => r.Name == "File")
                    .Cast<log4net.Appender.RollingFileAppender>()
                    .FirstOrDefault();
                if (appender != null)
                {
                    startInfo.EnvironmentVariables.Add("logPath",
                        Path.Combine(Path.GetDirectoryName(appender.File), "web.socketio.%DATE%.log"));
                }

                StartNode();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Stop()
        {
            StopPing();
            StopNode();
        }

        private static void StartNode()
        {
            StopNode();
            proc = Process.Start(startInfo);

            StartPing();
        }

        private static void StopNode()
        {
            try
            {
                if (proc != null && !proc.HasExited)
                {
                    proc.Kill();
                    if (!proc.WaitForExit(10000)) /* wait 10 seconds */
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
                Logger.Error("SocketIO failed stop", e);
            }
        }

        private static void StartPing()
        {
            Thread.Sleep(PingInterval);

            webSocket = new WebSocket(string.Format("ws://127.0.0.1:{0}/socket.io/?EIO=3&transport=websocket", startInfo.EnvironmentVariables["port"]));
            webSocket.SetCookie(new WebSocketSharp.Net.Cookie("authorization", SignalrServiceClient.CreateAuthToken()));
            webSocket.OnMessage += (sender, e) =>
            {
                if (e.Data.Contains("error"))
                {
                    Logger.Error("Auth error");
                    cancellationTokenSource.Cancel();
                }
            };
            webSocket.Connect();

            Task.Run(() =>
            {
                while (webSocket.Ping())
                {
                    Logger.Debug("Ping");
                    Thread.Sleep(PingInterval);
                }

                Logger.Debug("Reconnect");
                if (retries < maxretries)
                {
                    StartNode();
                    retries++;
                }
            }, cancellationTokenSource.Token);
        }

        private static void StopPing()
        {
            try
            {
                cancellationTokenSource.Cancel();
                if (webSocket.IsAlive)
                {
                    webSocket.Close();
                }
            }
            catch (Exception)
            {
                Logger.Error("Ping failed stop");
            }
        }
    }
}