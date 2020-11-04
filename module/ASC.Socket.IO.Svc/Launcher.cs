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
using System.Threading;
using System.Threading.Tasks;

using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Core;
using ASC.Core.Notify.Signalr;

using WebSocketSharp;

namespace ASC.Socket.IO.Svc
{
    public class Launcher : IServiceController
    {
        private static Process proc;
        private static ProcessStartInfo startInfo;
        private static WebSocket webSocket;
        private static CancellationTokenSource cancellationTokenSource;
        private const int PingInterval = 10000;
        private static readonly ILog Logger = LogManager.GetLogger("ASC");
        private static string LogDir;

        public void Start()
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();

                var cfg = (SocketIOCfgSectionHandler)ConfigurationManagerExtension.GetSection("socketio");

                startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = "node",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("\"{0}\"", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), cfg.Path, "app.js"))),
                    WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                };

                var appSettings = ConfigurationManagerExtension.AppSettings;

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

                LogDir = Logger.LogDirectory;
                startInfo.EnvironmentVariables.Add("logPath", Path.Combine(LogDir, "web.socketio.log"));
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

            var task = new Task(StartPing, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            task.Start(TaskScheduler.Default);
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

            var error = false;
            webSocket = new WebSocket(string.Format("ws://127.0.0.1:{0}/socket.io/?EIO=3&transport=websocket", startInfo.EnvironmentVariables["port"]));
            webSocket.SetCookie(new WebSocketSharp.Net.Cookie("authorization", SignalrServiceClient.CreateAuthToken()));
            webSocket.EmitOnPing = true;

            webSocket.Log.Level = LogLevel.Trace;

            webSocket.Log.Output = (logData, filePath) =>
            {
                if (logData.Message.Contains("SocketException"))
                {
                    error = true;
                }

                Logger.Debug(logData.Message);
            };

            webSocket.OnOpen += (sender, e) =>
            {
                Logger.Info("Open");
                error = false;

                Thread.Sleep(PingInterval);

                Task.Run(() =>
                {
                    while (webSocket.Ping())
                    {
                        Logger.Debug("Ping " + webSocket.ReadyState);
                        Thread.Sleep(PingInterval);
                    }

                    Logger.Debug("Reconnect" + webSocket.ReadyState);

                }, cancellationTokenSource.Token);
            };

            webSocket.OnClose += (sender, e) =>
            {
                Logger.Info("Close");

                if (cancellationTokenSource.IsCancellationRequested) return;

                if (error)
                {
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    webSocket.Connect();
                }

            };

            webSocket.OnMessage += (sender, e) =>
            {
                if (e.Data.Contains("error"))
                {
                    Logger.Error("Auth error");
                    cancellationTokenSource.Cancel();
                }
            };

            webSocket.OnError += (sender, e) =>
            {
                Logger.Error("Error", e.Exception);
            };

            webSocket.Connect();
        }

        private static void StopPing()
        {
            try
            {
                cancellationTokenSource.Cancel();
                if (webSocket.IsAlive)
                {
                    webSocket.Close();
                    webSocket = null;
                }
            }
            catch (Exception)
            {
                Logger.Error("Ping failed stop");
            }
        }
    }
}