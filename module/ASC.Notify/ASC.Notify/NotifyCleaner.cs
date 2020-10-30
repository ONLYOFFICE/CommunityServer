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
using System.Threading;

using ASC.Common.Data;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.Notify.Config;

namespace ASC.Notify
{
    public class NotifyCleaner : IServiceController
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Notify");
        private readonly ManualResetEvent stop = new ManualResetEvent(false);
        private Thread worker;


        public void Start()
        {
            worker = new Thread(Clear)
            {
                Name = "Notify Cleaner",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
            worker.Start();
        }

        public void Stop()
        {
            stop.Set();
            worker.Join(TimeSpan.FromSeconds(5));
        }


        private void Clear()
        {
            while (true)
            {
                try
                {
                    var date = DateTime.UtcNow.AddDays(-NotifyServiceCfg.StoreMessagesDays);
                    using (var db = new DbManager(NotifyServiceCfg.ConnectionStringName))
                    using (var d1 = db.Connection.CreateCommand("delete from notify_info where modify_date < ? and state = 4", date))
                    using (var d2 = db.Connection.CreateCommand("delete from notify_queue where creation_date < ?", date))
                    {
                        d1.CommandTimeout = 60 * 60; // hour
                        d2.CommandTimeout = 60 * 60; // hour

                        var affected1 = d1.ExecuteNonQuery();
                        var affected2 = d2.ExecuteNonQuery();

                        log.InfoFormat("Clear notify messages: notify_info({0}), notify_queue ({1})", affected1, affected2);
                    }
                }
                catch (ThreadAbortException)
                {
                    // ignore
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
                if (stop.WaitOne(TimeSpan.FromHours(8)))
                {
                    break;
                }
            }
        }
    }
}
