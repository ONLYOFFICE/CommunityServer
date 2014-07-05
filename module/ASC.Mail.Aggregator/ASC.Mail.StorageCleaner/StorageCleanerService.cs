/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Web;
using ASC.Common.Data.Sql;
using ASC.Data.Storage;
using ASC.Common.Data;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.StorageCleaner.Configuration;
using NLog;

namespace ASC.Mail.StorageCleaner
{
    class StorageCleanerService : ServiceBase
    {
        #region - Declaration -

        private const string DbId = "mailAggregator";
        private const string ModuleName = "mailaggregator";
        private readonly string _uniqueId = Guid.NewGuid().ToString();
        private readonly AutoResetEvent _waiter = new AutoResetEvent(true);
        private int _threadsCount;
        readonly ManualResetEvent _mreStop = new ManualResetEvent(false);
        private readonly Logger _log = LogManager.GetLogger("StorageCleaner");
        Thread _mainThread;
        Thread _watchdogThread;

        private struct Task
        {
            public int garbage_id;
            public int tenant;
            public string path;
        }

        private readonly Queue<Task> _tasks = new Queue<Task>();

        #endregion

        #region - Constructor -

        public StorageCleanerService()
        {
            CanStop = true;
            AutoLog = true;

            if (!DbRegistry.IsDatabaseRegistered(DbId))
            {
                DbRegistry.RegisterDatabase(DbId, StorageCleanerCfg.ConnectionString);
            }
        }

        #endregion

        #region - Start / Stop -

        public void StartConsole()
        {
            OnStart(null);
            Console.CancelKeyPress += (sender, e) => OnStop();
        }

        protected override void OnStart(string[] args)
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif
            try
            {
                //Start service
                _mreStop.Reset();
                var mpts = new ThreadStart(MainThread);
                var wpts = new ThreadStart(WatchDogThread);

                // Setup thread.
                _mainThread = new Thread(mpts);
                _watchdogThread = new Thread(wpts);

                // Start it.
                _mainThread.Start();
                _watchdogThread.Start();
            }
            catch (Exception)
            {
                OnStop();
            }
        }

        protected override void OnStop()
        {
            // Service will be stoped in future 30 seconds!
            // signal to tell the worker process to stop
            _mreStop.Set();
            _log.Info("Administrator stopped the service");
            if (_mainThread == null) return;

            // give it a little time to finish any pending work
            var timeout = new TimeSpan(0, 0, 30);
            _mainThread.Join(timeout);
            _watchdogThread.Join(timeout);
            _mainThread.Abort();
            _watchdogThread.Abort();
        }

        #endregion

        #region - Threads -

        // mail thread, wich dispatchs tasks
        private void MainThread()
        {
            try
            {
                _log.Info("MainThread");

                while (true)
                {
                    try
                    {
                        _waiter.WaitOne(TimeSpan.FromSeconds(5));

                        var count = Thread.VolatileRead(ref _threadsCount);
                        if (count < StorageCleanerCfg.MaxThreads)
                        {
                            try
                            {
                                var task = GetTask();
                                var t = new Thread(WorkerThread)
                                    {
                                        Priority = ThreadPriority.Lowest,
                                        Name = "StorageCleanerThread " + DateTime.UtcNow.ToString("o"),
                                        IsBackground = true,
                                    };
                                t.Start(task);
                                Interlocked.Increment(ref _threadsCount);
                                _waiter.Set();

                                _log.Debug("{0} started.", t.Name);
                            }
                            catch (InvalidOperationException)
                            {
                                _log.Debug("No garbage.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Fatal("Unable to kill tasks. Exception:\r\n{0}", ex.ToString());
                    }

                    if (_mreStop.WaitOne(0))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            finally
            {
                OnStop();
            }
        }

        // deletes data from storage and deletes appropreate row from garbage table
        private void WorkerThread(object data)
        {
            try
            {
                var task = (Task) data;
                _log.Debug("WorkerThread deleting " + task.path);
                GetDataStore(task.tenant).Delete(string.Empty, task.path);
                using (var db = GetDb())
                {
                    db.ExecuteNonQuery(new SqlDelete(Aggregator.DbSchema.Garbage.table).Where(Aggregator.DbSchema.Garbage.Columns.id, task.garbage_id));
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            finally
            {
                Interlocked.Decrement(ref _threadsCount);
                try
                {
                    _waiter.Set();
                }
                catch (ObjectDisposedException) { }
                _log.Debug(string.Format("{0} stopped", Thread.CurrentThread.Name));
            }
        }

        // watches for tasks which are in work for too long
        private void WatchDogThread()
        {
            try
            {
                _log.Info("WatchDog Thread");

                while (true)
                {
                    using (var db = GetDb())
                    {
                        db.ExecuteNonQuery(new SqlUpdate(Aggregator.DbSchema.Garbage.table)
                                               .Set(Aggregator.DbSchema.Garbage.Columns.is_processed, "")
                                               .Where(!Exp.Eq(Aggregator.DbSchema.Garbage.Columns.is_processed, ""))
                                               .Where(Exp.Lt(Aggregator.DbSchema.Garbage.Columns.time_modified,
                                                             DateTime.Now.AddSeconds(-StorageCleanerCfg.WatchdogTimeout))));
                    }

                    if (_mreStop.WaitOne(TimeSpan.FromSeconds(StorageCleanerCfg.WatchdogTimeout)))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            finally
            {
                _log.Info("WatchDog Thread stopped");
            }
        }

        #endregion

        #region - Private methods -

        // returns task from pool and generates new tasks if pool is empty
        private Task GetTask()
        {
            if(!_tasks.Any())
                GenTasks();
            return _tasks.Dequeue();
        }

        // tryes to gen new garbage records from attachments
        private void GenTasks()
        {
            using (var db = GetDb())
            {
                if (0 == MarkGarbage(db))
                {
                    if (0 == GenGarbage(db))
                        return;
                    if(0 == MarkGarbage(db))
                        return;
                }

                db.ExecuteList(new SqlQuery(Aggregator.DbSchema.Garbage.table)
                    .Select(new[] { Aggregator.DbSchema.Garbage.Columns.id, Aggregator.DbSchema.Garbage.Columns.tenant, Aggregator.DbSchema.Garbage.Columns.path })
                    .Where(Aggregator.DbSchema.Garbage.Columns.is_processed, _uniqueId))
                    .ForEach(r => _tasks.Enqueue(new Task{garbage_id = Convert.ToInt32(r[0]), tenant = Convert.ToInt32(r[1]), path = (string)r[2]}));
            }
        }

        // mark rows in garbage table as taked by this service instance
        // returns count of marked rows
        private int MarkGarbage(IDbManager db)
        {
            return db.ExecuteNonQuery(new SqlUpdate(Aggregator.DbSchema.Garbage.table)
                                             .Where(Aggregator.DbSchema.Garbage.Columns.is_processed, "")
                                             .Set(Aggregator.DbSchema.Garbage.Columns.is_processed, _uniqueId));
        }

        // generates rows in garbage table
        // returns count of generated rows
        private static int GenGarbage(IDbManager db)
        {
            var locked = false;
            try
            {
                if (1 != db.ExecuteScalar<int>(String.Format("SELECT GET_LOCK('{0}',{1})", StorageCleanerCfg.DbLockName, StorageCleanerCfg.DbLockTimeot)))
                    return 0;

                locked = true;

                var res = GenGarbageFromAttachments(db);
                return 0 < res ? res : GenGarbageFromMails(db);
            }
            finally
            {
                if (locked)
                    db.ExecuteScalar<int>(String.Format("SELECT RELEASE_LOCK('{0}')", StorageCleanerCfg.DbLockName));
            }
        }

        // generates garbge from removed mails attachments
        // return number of generated items
        private static int GenGarbageFromAttachments(IDbManager db)
        {
            var attachements_to_remove = db.ExecuteList(new SqlQuery(Aggregator.DbSchema.AttachmentTable.name)
                .Select(new[] {Aggregator.DbSchema.AttachmentTable.Columns.id,
                        Aggregator.DbSchema.AttachmentTable.Columns.id_mail,
                        Aggregator.DbSchema.AttachmentTable.Columns.id_tenant,
                        Aggregator.DbSchema.AttachmentTable.Columns.file_number,
                        Aggregator.DbSchema.AttachmentTable.Columns.name})
                .Where(Aggregator.DbSchema.AttachmentTable.Columns.need_remove, 1)
                .SetMaxResults(StorageCleanerCfg.TasksChunkSize * StorageCleanerCfg.TasksGenChunkCount))
                .ConvertAll(i => new
                {
                    id = Convert.ToInt32(i[0]),
                    id_mail = Convert.ToInt32(i[1]),
                    tenant = Convert.ToInt32(i[2]),
                    file_number = Convert.ToInt32(i[3]),
                    name = (string)i[4],
                    key = ""
                });

            if (!attachements_to_remove.Any())
                return 0;

            var mail_ids = attachements_to_remove.ConvertAll(i => i.id_mail).Distinct().ToArray();

            var related_mails = db.ExecuteList(new SqlQuery(Aggregator.DbSchema.MailTable.name)
                .Select(new[]{Aggregator.DbSchema.MailTable.Columns.id,
                    Aggregator.DbSchema.MailTable.Columns.stream,
                    Aggregator.DbSchema.MailTable.Columns.id_user})
                .Where(Exp.In(Aggregator.DbSchema.MailTable.Columns.id, mail_ids)))
                .ConvertAll(i => new
                {
                    id = Convert.ToInt32(i[0]),
                    stream = (string)i[1],
                    id_user = (string)i[2]
                });

            var data = (from attachment in attachements_to_remove
                        let mail = related_mails.Find(i => i.id == attachment.id_mail)
                        select new
                        {
                            path = MakeStorageAttachmentKey(mail.id_user, mail.stream, attachment.file_number, attachment.name),
                            attachment.tenant
                        });

            var insert_garbage_querys = data.Select(item => new SqlInsert(Aggregator.DbSchema.Garbage.table)
                .InColumnValue(Aggregator.DbSchema.Garbage.Columns.path, item.path)
                .InColumnValue(Aggregator.DbSchema.Garbage.Columns.tenant, item.tenant))
                .Cast<ISqlInstruction>().ToList();

            db.ExecuteBatch(insert_garbage_querys);

            db.ExecuteNonQuery(new SqlDelete(Aggregator.DbSchema.AttachmentTable.name)
                                   .Where(Exp.In(Aggregator.DbSchema.AttachmentTable.Columns.id,
                                                 attachements_to_remove.ConvertAll(item => item.id).ToArray())));

            return insert_garbage_querys.Count;
        }

        // generates garbge from removed mails bodyes
        // return number of generated items
        private static int GenGarbageFromMails(IDbManager db)
        {
            var bodyes_to_remove = db.ExecuteList(new SqlQuery(Aggregator.DbSchema.MailTable.name)
                .Select(new[]{Aggregator.DbSchema.MailTable.Columns.id,
                    Aggregator.DbSchema.MailTable.Columns.stream,
                    Aggregator.DbSchema.MailTable.Columns.id_user,
                    Aggregator.DbSchema.MailTable.Columns.id_tenant})
                .Where(Aggregator.DbSchema.MailTable.Columns.is_removed, 1)
                .Where(!Exp.Eq(Aggregator.DbSchema.MailTable.Columns.stream, ""))
                .SetMaxResults(StorageCleanerCfg.TasksChunkSize * StorageCleanerCfg.TasksGenChunkCount))
                .ConvertAll(i => new
                {
                    mail_id = Convert.ToInt32(i[0]),
                    path = MakeStorageBodyKey((string)i[2], (string)i[1]),
                    tenant = Convert.ToInt32(i[3])
                });

            if (!bodyes_to_remove.Any())
                return 0;

            var insert_garbage_querys = bodyes_to_remove.Select(item => new SqlInsert(Aggregator.DbSchema.Garbage.table)
                .InColumnValue(Aggregator.DbSchema.Garbage.Columns.path, item.path)
                .InColumnValue(Aggregator.DbSchema.Garbage.Columns.tenant, item.tenant))
                .Cast<ISqlInstruction>().ToList();

            db.ExecuteBatch(insert_garbage_querys);

            db.ExecuteNonQuery(new SqlUpdate(Aggregator.DbSchema.MailTable.name)
                .Set(Aggregator.DbSchema.MailTable.Columns.stream, "")
                .Where(Exp.In(Aggregator.DbSchema.MailTable.Columns.id, bodyes_to_remove.ConvertAll(item => item.mail_id).ToArray())));

            return insert_garbage_querys.Count;
        }

        // returns new db manager instance
        private static IDbManager GetDb()
        {
            return new DbManager(DbId);
        }

        // return attachment storage key
        private static string MakeStorageAttachmentKey(string id_user, string stream, int file_number, string name)
        {
            return String.Format("{0}|{1}",
                ModuleName,
                HttpUtility.UrlPathEncode(id_user + "/" + stream + "/attachments/" + file_number + "/" + name));
        }

        // return body storage key
        private static string MakeStorageBodyKey(string id_user, string stream)
        {
            return String.Format("{0}|{1}",
                ModuleName,
                HttpUtility.UrlPathEncode(id_user + "/" + stream + "/body.html"));
        }

        // returns new data store object
        private static IDataStore GetDataStore(int tenant)
        {
            return StorageFactory.GetStorage(tenant.ToString(CultureInfo.InvariantCulture), ModuleName);
        }

        #endregion
    }
}
