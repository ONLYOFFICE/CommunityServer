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


using System;
using System.Threading;
using System.Linq;

using ASC.FullTextIndex.Service.Config;

using log4net;

namespace ASC.FullTextIndex.Service
{
    enum TextIndexAction
    {
        None,
        Index,
        Merge,
        Remove
    }

    class TextIndexerParams
    {
        public TextIndexAction Action { get; private set; }
        public TimeSpan Period { get; private set; }
        public DateTime LastIndexDate { get; private set; }

        public TextIndexerParams(TextIndexAction action, TimeSpan period)
        {
            Action = action;
            Period = period;
            LastIndexDate = DateTime.UtcNow;
        }

        public void InitNext()
        {
            var now = DateTime.UtcNow;
            var indexDateTime = TextIndexCfg.ChangedCron.GetTimeAfter(now) ?? DateTime.MaxValue;
            var removeDateTime = TextIndexCfg.RemovedCron.GetTimeAfter(now) ?? DateTime.MaxValue;
            var mergeDateTime = TextIndexCfg.MergeCron.GetTimeAfter(now) ?? DateTime.MaxValue;

            Action = TextIndexAction.None;
            DateTime period;
            if (indexDateTime < mergeDateTime)
            {
                Action = TextIndexAction.Index;
                period = indexDateTime;
                LastIndexDate = period;
            }
            else
            {
                if (mergeDateTime < removeDateTime)
                {
                    Action = TextIndexAction.Merge;
                    period = mergeDateTime;
                }
                else
                {
                    Action = TextIndexAction.Remove;
                    period = removeDateTime;
                }
            }

            Period = (period - now).Add(TimeSpan.FromSeconds(1));

        }
    }

    class TextIndexerService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TextIndexerService));

        private readonly Thread worker;
        private readonly ManualResetEvent stop;
        private static TextIndexerService instance;
        public static TextIndexerService Instance
        {
            get
            {
                return instance ?? (instance = new TextIndexerService());
            }
        }

        private TextIndexerService()
        {
            worker = new Thread(DoWork) { Priority = ThreadPriority.Lowest, Name = "Full Text Indexer", IsBackground = true};
            stop = new ManualResetEvent(false);
        }

        public void Start()
        {
            worker.Start();
        }

        public void Stop()
        {
            stop.Set();
            worker.Join(TimeSpan.FromSeconds(10));
            stop.Close();
        }        

        private void DoWork()
        {
            var parameters = new TextIndexerParams(TextIndexAction.None, TimeSpan.FromSeconds(1));

            do
            {
                try
                {
                    if (stop.WaitOne(parameters.Period))
                    {
                        return;
                    }


                    DoIndex(parameters);

                    TextSearcher.Instance.Start();

                    parameters.InitNext();

                    log.DebugFormat("Next action '{0}' over {1}", parameters.Action, parameters.Period);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error in DoIndex: {0}", ex);
                }
            }
            while (true);
        }

        private void DoIndex(TextIndexerParams parameters)
        {
            var indexed = DbProvider.GetIndexedModules().ToList();

            foreach (var module in TextIndexCfg.Modules)
            {
                if (stop.WaitOne(TimeSpan.Zero))
                {
                    return;
                }
                
                var indexer = TextIndexCfg.Chunks > 1 ? new TextIndexerDistributed(module) : new TextIndexer(module);

                try
                {
                    if (TextIndexAction.None == parameters.Action  && !indexed.Contains(module.Main) ||
                        TextIndexAction.Remove == parameters.Action)
                    {
                        indexer.RotateMain();
                        DbProvider.UpdateLastIndexDate(module.Main, DateTime.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error first time index {0}, module {1}", ex, module.Name);
                }

                try
                {
                    if (TextIndexAction.Merge == parameters.Action)
                    {
                        var exitCode = indexer.Merge();
                        if (exitCode == 0)
                            DbProvider.UpdateLastIndexDate(module.Delta, parameters.LastIndexDate);
                    }

                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error rotate {0}, module {1}", ex, module.Name);
                }

                try
                {
                    indexer.RotateDelta();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error rotate delta {0}, module {1}", ex, module.Name);
                }
            }
        }
    }
}
