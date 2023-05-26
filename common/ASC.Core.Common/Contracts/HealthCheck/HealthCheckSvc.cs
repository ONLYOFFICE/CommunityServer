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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ASC.Common.Logging;

namespace ASC.Core.Common.Contracts
{
    public class HealthCheckSvc
    {
        private readonly string port;
        private readonly string needResult;
        private readonly string path;
        private readonly int pingInterval;
        private readonly string url;
        private CancellationTokenSource PingCancellationTokenSource = new CancellationTokenSource();
        private ILog Log;

        public HealthCheckSvc(string port, string needResult, ILog log, string path = "")
        {
            this.port = port;
            this.needResult = needResult;
            this.path = path;
            Log = log;

            var appSettings = ConfigurationManagerExtension.AppSettings;
            pingInterval = int.Parse(appSettings["ping.interval"]);
            url = appSettings["ping.url"];
        }

        private bool Ping()
        {
            using (var client = new WebClient { Encoding = Encoding.UTF8 })
            {
                try
                {
                    var result = client.DownloadString(new Uri(url + ":" + port + path));
                    return result == needResult;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void StartPing()
        {
            var task = new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(pingInterval);
                    Log.Debug("Ping");
                    if (!Ping())
                    {
                        Log.Error("Error pong");
                        Process.GetCurrentProcess().Kill();
                    }
                    else 
                    {
                        Log.Debug("Pong");
                    }
                }
            }, PingCancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            task.Start(TaskScheduler.Default);
        }

        public void StopPing()
        {
            PingCancellationTokenSource.Cancel();
            PingCancellationTokenSource.Dispose();
        }
    }
}
