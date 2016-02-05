/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Common.Data;
using ASC.Common.Module;
using ASC.Notify.Config;
using log4net;
using System;
using System.Threading;

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
