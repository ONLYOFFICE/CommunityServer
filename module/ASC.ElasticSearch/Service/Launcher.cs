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
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Common.Module;
using ASC.ElasticSearch.Service;
using Autofac;

namespace ASC.ElasticSearch
{
    public class Launcher : IServiceController
    {
        private static readonly ICacheNotify Notify = AscCache.Notify;
        private static Timer timer;
        private static TimeSpan Period { get { return TimeSpan.FromMinutes(Settings.Default.Period); } }
        private static ILog logger;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        internal static ServiceHost Searcher { get; private set; }
        internal static bool IsStarted { get; private set; }
        internal static string Indexing { get; private set; }
        internal static DateTime? LastIndexed { get; private set; }

        public void Start()
        {
            Searcher = new ServiceHost(typeof(Service.Service));
            Searcher.Open();

            logger = LogManager.GetLogger("ASC.Indexer");

            try
            {
                Notify.Subscribe<AscCacheItem>(async (item, action) =>
                {
                    while (IsStarted)
                    {
                        await Task.Delay(10000);
                    }
                    IndexAll(true);
                });
            }
            catch (Exception e)
            {
                logger.Error("Subscribe on start", e);
            }

            var task = new Task(() =>
            {
                while (!FactoryIndexer.CheckState(false))
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    Thread.Sleep(10000);
                }

                CheckIfChange();
            }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

            task.Start();
        }

        private static void CheckIfChange()
        {
            IsStarted = true;

            var generic = typeof(BaseIndexer<>);
            var products = FactoryIndexer.Builder.Resolve<IEnumerable<Wrapper>>()
                .Select(r => (IIndexer)Activator.CreateInstance(generic.MakeGenericType(r.GetType()), r))
                .ToList();

            products.ForEach(product =>
            {
                try
                {
                    if (!IsStarted) return;

                    logger.DebugFormat("Product check {0}", product.IndexName);
                    Indexing = product.IndexName;
                    product.Check();
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    logger.ErrorFormat("Product check {0}", product.IndexName);
                }
            });

            IsStarted = false;
            Indexing = null;

            timer = new Timer(_ => IndexAll(), null, TimeSpan.Zero, TimeSpan.Zero);
        }

        private static void IndexAll(bool reindex = false)
        {
            timer.Change(-1, -1);
            IsStarted = true;

            var generic = typeof(BaseIndexer<>);
            var products = FactoryIndexer.Builder.Resolve<IEnumerable<Wrapper>>()
                .Select(r => (IIndexer)Activator.CreateInstance(generic.MakeGenericType(r.GetType()), r))
                .ToList();

            if (reindex)
            {
                products.ForEach(product =>
                {
                    try
                    {
                        if (!IsStarted) return;

                        logger.DebugFormat("Product reindex {0}", product.IndexName);
                        product.ReIndex();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                        logger.ErrorFormat("Product reindex {0}", product.IndexName);
                    }
                });
            }

            products.ForEach(product =>
            {
                try
                {
                    if (!IsStarted) return;

                    logger.DebugFormat("Product {0}", product.IndexName);
                    Indexing = product.IndexName;
                    product.IndexAll();
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    logger.ErrorFormat("Product {0}", product.IndexName);
                }
            });

            timer.Change(Period, Period);
            LastIndexed = DateTime.UtcNow;
            IsStarted = false;
            Indexing = null;
        }

        public void Stop()
        {
            if (Searcher != null)
            {
                Searcher.Close();
                Searcher = null;
            }

            IsStarted = false;

            if (timer != null)
            {
                timer.Dispose();
            }

            cancellationTokenSource.Cancel();
        }
    }
}
