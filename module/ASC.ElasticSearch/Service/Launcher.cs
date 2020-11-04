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
