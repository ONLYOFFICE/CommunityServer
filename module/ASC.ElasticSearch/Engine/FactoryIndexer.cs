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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using ASC.Common.Caching;
using ASC.Common.DependencyInjection;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.ElasticSearch.Core;
using ASC.ElasticSearch.Service;

using Autofac;

using Elasticsearch.Net;

using Nest;

namespace ASC.ElasticSearch
{
    public class FactoryIndexer<T> where T : Wrapper, new()
    {
        private static readonly ICache Cache = AscCache.Memory;
        private static readonly TaskScheduler Scheduler = new LimitedConcurrencyLevelTaskScheduler(10);
        private static readonly ILog Logger = LogManager.GetLogger("ASC.Indexer");

        public static bool Support
        {
            get
            {
                if (!FactoryIndexer.CheckState()) return false;
                var t = new T();

                var cacheTime = DateTime.UtcNow.AddMinutes(15);
                var key = "elasticsearch " + t.IndexName;
                try
                {
                    var cacheValue = Cache.Get<string>(key);
                    if (!string.IsNullOrEmpty(cacheValue))
                    {
                        return Convert.ToBoolean(cacheValue);
                    }

                    var service = new Service.Service();

                    var result = service.Support(t.IndexName);

                    Cache.Insert(key, result.ToString(CultureInfo.InvariantCulture).ToLower(), cacheTime);

                    return result;
                }
                catch (Exception e)
                {
                    Cache.Insert(key, "false", cacheTime);
                    Logger.Error("FactoryIndexer CheckState", e);
                    return false;
                }
            }
        }

        public static bool TrySelect(Expression<Func<Selector<T>, Selector<T>>> expression, out IReadOnlyCollection<T> result)
        {
            if (!Support || !Indexer.CheckExist(new T()))
            {
                result = new List<T>();
                return false;
            }

            try
            {
                result = Indexer.Select(expression);
            }
            catch (Exception e)
            {
                Logger.Error("Select", e);
                result = new List<T>();
                return false;
            }
            return true;
        }

        public static bool TrySelectIds(Expression<Func<Selector<T>, Selector<T>>> expression, out List<int> result)
        {
            if (!Support || !Indexer.CheckExist(new T()))
            {
                result = new List<int>();
                return false;
            }

            try
            {
                result = Indexer.Select(expression, true).Select(r => r.Id).ToList();
            }
            catch (Exception e)
            {
                Logger.Error("Select", e);
                result = new List<int>();
                return false;
            }

            return true;
        }

        public static bool TrySelectIds(Expression<Func<Selector<T>, Selector<T>>> expression, out List<int> result, out long total)
        {
            if (!Support || !Indexer.CheckExist(new T()))
            {
                result = new List<int>();
                total = 0;
                return false;
            }

            try
            {
                result = Indexer.Select(expression, true, out total).Select(r => r.Id).ToList();
            }
            catch (Exception e)
            {
                Logger.Error("Select", e);
                total = 0;
                result = new List<int>();
                return false;
            }

            return true;
        }

        public static bool Index(T data, bool immediately = true)
        {
            if (!Support) return false;

            try
            {
                Indexer.Index(data, immediately);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("Index", e);
            }
            return false;
        }

        public static void Index(List<T> data, bool immediately = true, int retry = 0)
        {
            if (!Support || !data.Any()) return;

            try
            {
                Indexer.Index(data, immediately);
            }
            catch (ElasticsearchClientException e)
            {
                Logger.Error(e);

                if (e.Response != null)
                {
                    IndexRetry(e, data, immediately, retry);
                }
            }
            catch (AggregateException e) //ElasticsearchClientException
            {
                if (e.InnerExceptions.Count == 0) throw;

                var inner = e.InnerExceptions.OfType<ElasticsearchClientException>().FirstOrDefault();

                if (inner != null)
                {
                    IndexRetry(inner, data, immediately, retry);
                }
                else
                {
                    throw;
                }
            }
        }

        private static void IndexRetry(ElasticsearchClientException e, List<T> data, bool immediately = true, int retry = 0)
        {
            Logger.Error(e);

            if (e.Response.HttpStatusCode == 413 || e.Response.HttpStatusCode == 403 || e.Response.HttpStatusCode == 408)
            {
                data.Where(r => r != null).ToList().ForEach(r => Index(r, immediately));
            }
            else if (e.Response.HttpStatusCode == 429)
            {
                Thread.Sleep(60000);
                if (retry < 10)
                {
                    Index(data.Where(r => r != null).ToList(), immediately, retry++);
                    return;
                }

                throw e;
            }
        }

        public static async Task IndexAsync(List<T> data, bool immediately = true, int retry = 0)
        {
            if (!Support || !data.Any()) return;

            try
            {
                await Indexer.IndexAsync(data, immediately).ConfigureAwait(false);
            }
            catch (ElasticsearchClientException e)
            {
                Logger.Error(e);

                if (e.Response != null)
                {
                    await IndexRetryAsync(e, data, immediately, retry);
                }
            }
            catch (AggregateException e) //ElasticsearchClientException
            {
                if (e.InnerExceptions.Count == 0) throw;

                var inner = e.InnerExceptions.OfType<ElasticsearchClientException>().FirstOrDefault();

                if (inner != null)
                {
                    await IndexRetryAsync(inner, data, immediately, retry);
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task IndexRetryAsync(ElasticsearchClientException e, List<T> data, bool immediately = true, int retry = 0)
        {
            Logger.Error(e);

            if (e.Response.HttpStatusCode == 413 || e.Response.HttpStatusCode == 403 || e.Response.HttpStatusCode == 408)
            {
                foreach (var r in data.Where(r => r != null))
                {
                    await IndexAsync(r, immediately);
                }
            }
            else if (e.Response.HttpStatusCode == 429)
            {
                await Task.Delay(60000);
                if (retry < 10)
                {
                    await IndexAsync(data.Where(r => r != null).ToList(), immediately, retry++);
                    return;
                }

                throw e;
            }
        }

        public static void Update(T data, bool immediately = true, params Expression<Func<T, object>>[] fields)
        {
            if (!Support) return;

            try
            {
                Indexer.Update(data, immediately, fields);
            }
            catch (Exception e)
            {
                Logger.Error("Update", e);
            }
        }

        public static void Update(T data, UpdateAction action, Expression<Func<T, IList>> field, bool immediately = true)
        {
            if (!Support) return;
            try
            {
                Indexer.Update(data, action, field, immediately);
            }
            catch (Exception e)
            {
                Logger.Error("Update", e);
            }
        }

        public static void Update(T data, Expression<Func<Selector<T>, Selector<T>>> expression, bool immediately = true, params Expression<Func<T, object>>[] fields)
        {
            if (!Support) return;
            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;
                Indexer.Update(data, expression, tenant, immediately, fields);
            }
            catch (Exception e)
            {
                Logger.Error("Update", e);
            }
        }

        public static void Update(T data, Expression<Func<Selector<T>, Selector<T>>> expression, UpdateAction action, Expression<Func<T, IList>> fields, bool immediately = true)
        {
            if (!Support) return;
            try
            {
                var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;
                Indexer.Update(data, expression, tenant, action, fields, immediately);
            }
            catch (Exception e)
            {
                Logger.Error("Update", e);
            }
        }

        public static void Delete(T data, bool immediately = true)
        {
            if (!Support) return;
            try
            {
                Indexer.Delete(data, immediately);
            }
            catch (Exception e)
            {
                Logger.Error("Delete", e);
            }
        }

        public static void Delete(Expression<Func<Selector<T>, Selector<T>>> expression, bool immediately = true)
        {
            if (!Support) return;
            var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;

            try
            {
                Indexer.Delete(expression, tenant, immediately);
            }
            catch (Exception e)
            {
                Logger.Error("Index", e);
            }
        }

        public static Task<bool> IndexAsync(T data, bool immediately = true)
        {
            if (!Support) return Task.FromResult(false);
            return Queue(() => Indexer.Index(data, immediately));
        }

        //public static Task<bool> IndexAsync(List<T> data, bool immediately = true)
        //{
        //    if (!Support) return Task.FromResult(false);
        //    return Queue(() => Indexer.Index(data, immediately));
        //}

        public static Task<bool> UpdateAsync(T data, bool immediately = true, params Expression<Func<T, object>>[] fields)
        {
            if (!Support) return Task.FromResult(false);
            return Queue(() => Indexer.Update(data, immediately, fields));
        }

        public static Task<bool> DeleteAsync(T data, bool immediately = true)
        {
            if (!Support) return Task.FromResult(false);
            return Queue(() => Indexer.Delete(data, immediately));
        }

        public static Task<bool> DeleteAsync(Expression<Func<Selector<T>, Selector<T>>> expression, bool immediately = true)
        {
            if (!Support) return Task.FromResult(false);
            var tenant = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            return Queue(() => Indexer.Delete(expression, tenant, immediately));
        }


        public static void Flush()
        {
            if (!Support) return;
            Indexer.Flush();
        }

        public static void Refresh()
        {
            if (!Support) return;
            Indexer.Refresh();
        }

        private static BaseIndexer<T> Indexer
        {
            get { return new BaseIndexer<T>(FactoryIndexer.Builder.Resolve<T>()); }
        }

        private static Task<bool> Queue(Action actionData)
        {
            var task = new Task<bool>(() =>
            {
                try
                {
                    actionData();
                    return true;
                }
                catch (AggregateException agg)
                {
                    foreach (var e in agg.InnerExceptions)
                    {
                        Logger.Error(e);
                    }
                    throw;
                }

            }, TaskCreationOptions.LongRunning);

            task.ConfigureAwait(false);
            task.Start(Scheduler);
            return task;
        }
    }

    public class FactoryIndexer
    {
        private static readonly ICache cache = AscCache.Memory;
        private static readonly ILog Logger = LogManager.GetLogger("ASC.Indexer");
        internal static IContainer Builder { get; set; }
        internal static bool Init { get; set; }

        static FactoryIndexer()
        {
            try
            {
                var container = AutofacConfigLoader.Load("search");
                if (container != null)
                {
                    Builder = container.Build();
                    Init = true;
                }
                else
                {
                    Logger.Fatal("FactoryIndexer container == null");
                    return;
                }

                if (CheckState())
                {
                    Client.Instance.Ingest.PutPipeline("attachments", p => p
                        .Processors(pp => pp
                            .Attachment<Attachment>(a => a.Field("document.data").TargetField("document.attachment"))
                        ));
                }
            }
            catch (Exception e)
            {
                Logger.Fatal("FactoryIndexer", e);
            }
        }

        public static bool CheckState(bool cacheState = true)
        {
            if (!Init) return false;

            const string key = "elasticsearch";

            if (cacheState)
            {
                var cacheValue = cache.Get<string>(key);
                if (!string.IsNullOrEmpty(cacheValue))
                {
                    return Convert.ToBoolean(cacheValue);
                }
            }

            var cacheTime = DateTime.UtcNow.AddMinutes(15);

            try
            {
                var result = Client.Instance.Ping(new PingRequest());

                var isValid = result.IsValid;

                Logger.DebugFormat("CheckState ping {0}", result.DebugInformation);

                if (cacheState)
                {
                    cache.Insert(key, isValid.ToString(CultureInfo.InvariantCulture).ToLower(), cacheTime);
                }

                return isValid;
            }
            catch (Exception e)
            {
                if (cacheState)
                {
                    cache.Insert(key, "false", cacheTime);
                }

                Logger.Error("Ping false", e);
                return false;
            }
        }

        public static object GetState()
        {
            State state = null;
            IEnumerable<object> indices = null;
            Dictionary<string, long> count = null;

            if (!CoreContext.Configuration.Standalone)
            {
                return new
                {
                    state,
                    indices,
                    status = CheckState()
                };
            }

            using (var service = new ServiceClient())
            {
                state = service.GetState();
                count = service.GetCount();
            }

            if (state.LastIndexed.HasValue)
            {
                state.LastIndexed = TenantUtil.DateTimeFromUtc(state.LastIndexed.Value);
            }

            indices = Client.Instance.Cat.Indices(new CatIndicesRequest { SortByColumns = new[] { "index" } }).Records
                .Select(r => new
                {
                    r.Index,
                    Count = count.ContainsKey(r.Index) ? count[r.Index] : 0,
                    DocsCount = Client.Instance.Count(new CountRequest(r.Index)).Count,
                    r.StoreSize
                })
                .Where(r =>
                {
                    return r.Count > 0;
                });

            return new
            {
                state,
                indices,
                status = CheckState()
            };
        }

        public static void Reindex(string name)
        {
            if (!CoreContext.Configuration.Standalone) return;

            var generic = typeof(BaseIndexer<>);
            var indexers = Builder.Resolve<IEnumerable<Wrapper>>()
                .Where(r => string.IsNullOrEmpty(name) || r.IndexName == name)
                .Select(r => (IIndexer)Activator.CreateInstance(generic.MakeGenericType(r.GetType()), r));

            foreach (var indexer in indexers)
            {
                indexer.ReIndex();
            }
        }

    }
}