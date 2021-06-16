/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.ElasticSearch.Core;
using ASC.ElasticSearch.Service;

using Elasticsearch.Net;

using Nest;

namespace ASC.ElasticSearch
{
    public class BaseIndexer<T> : IIndexer where T : Wrapper, new()
    {
        private static bool IsExist;
        private static readonly object Locker = new object();
        private static readonly ILog Logger = LogManager.GetLogger("ASC.Indexer");
        private static readonly ICacheNotify Notify;
        private static CancellationTokenSource Cts;
        private static TaskScheduler Scheduler;

        private const int QueryLimit = 10000;

        private const string Alias = "_table";

        protected internal T Wrapper { get; set; }

        public string IndexName { get { return Wrapper.IndexName; } }

        static BaseIndexer()
        {
            Cts = new CancellationTokenSource();
            Notify = AscCache.Notify;
            Scheduler = new LimitedConcurrencyLevelTaskScheduler(4);
            Notify.Subscribe<BaseIndexer<T>>((a, b) =>
            {
                lock (Locker)
                {
                    IsExist = false;
                    Cts.Cancel();
                }
            });
        }

        public BaseIndexer(T wrapper)
        {
            Wrapper = wrapper;
        }

        internal void Index(T data, bool immediately = true)
        {
            BeforeIndex(data);

            Client.Instance.Index(data, idx => GetMeta(idx, data, immediately));
        }

        internal void Index(List<T> data, bool immediately = true)
        {
            CreateIfNotExist(data[0]);

            if (typeof(T).IsSubclassOf(typeof(WrapperWithDoc)))
            {
                var currentLength = 0L;
                var portion = new List<T>();
                var portionStart = 0;

                for (var i = 0; i < data.Count; i++)
                {
                    var t = data[i];
                    var runBulk = i == data.Count - 1;

                    BeforeIndex(t);

                    var wwd = t as WrapperWithDoc;

                    if (wwd == null || wwd.Document == null || string.IsNullOrEmpty(wwd.Document.Data))
                    {
                        portion.Add(t);
                    }
                    else
                    {
                        var dLength = wwd.Document.Data.Length;
                        if (dLength >= Settings.Default.MaxContentLength)
                        {
                            try
                            {
                                Index(t, immediately);
                            }
                            catch (ElasticsearchClientException e)
                            {
                                if (e.Response.HttpStatusCode == 429)
                                {
                                    throw;
                                }
                                LogManager.GetLogger("ASC.Indexer").Error(e);
                            }
                            catch (Exception e)
                            {
                                LogManager.GetLogger("ASC.Indexer").Error(e);
                            }
                            finally
                            {
                                wwd.Document.Data = null;
                                wwd.Document = null;
                                wwd = null;
                                GC.Collect();
                            }
                            continue;
                        }

                        if (currentLength + dLength < Settings.Default.MaxContentLength)
                        {
                            portion.Add(t);
                            currentLength += dLength;
                        }
                        else
                        {
                            runBulk = true;
                            i--;
                        }
                    }

                    if (runBulk)
                    {
                        var portion1 = portion.ToList();
                        Client.Instance.Bulk(r => r.IndexMany(portion1, GetMeta).SourceExcludes("attachments"));
                        for (var j = portionStart; j < i; j++)
                        {
                            var doc = data[j] as WrapperWithDoc;
                            if (doc != null && doc.Document != null)
                            {
                                doc.Document.Data = null;
                                doc.Document = null;
                            }
                            doc = null;
                        }

                        portionStart = i;
                        portion = new List<T>();
                        currentLength = 0L;
                        GC.Collect();
                    }
                }
            }
            else
            {
                foreach (var item in data)
                {
                    BeforeIndex(item);
                }

                Client.Instance.Bulk(r => r.IndexMany(data, GetMeta));
            }
        }

        internal async Task IndexAsync(List<T> data, bool immediately = true)
        {
            CreateIfNotExist(data[0]);

            if (typeof(T).IsSubclassOf(typeof(WrapperWithDoc)))
            {
                var currentLength = 0L;
                var portion = new List<T>();
                var portionStart = 0;

                for (var i = 0; i < data.Count; i++)
                {
                    var t = data[i];
                    var runBulk = i == data.Count - 1;

                    await BeforeIndexAsync(t).ConfigureAwait(false);

                    var wwd = t as WrapperWithDoc;

                    if (wwd == null || wwd.Document == null || string.IsNullOrEmpty(wwd.Document.Data))
                    {
                        portion.Add(t);
                    }
                    else
                    {
                        var dLength = wwd.Document.Data.Length;
                        if (dLength >= Settings.Default.MaxContentLength)
                        {
                            try
                            {
                                Index(t, immediately);
                            }
                            catch (ElasticsearchClientException e)
                            {
                                if (e.Response.HttpStatusCode == 429)
                                {
                                    throw;
                                }
                                LogManager.GetLogger("ASC.Indexer").Error(e);
                            }
                            catch (Exception e)
                            {
                                LogManager.GetLogger("ASC.Indexer").Error(e);
                            }
                            finally
                            {
                                wwd.Document.Data = null;
                                wwd.Document = null;
                                wwd = null;
                                GC.Collect();
                            }
                            continue;
                        }

                        if (currentLength + dLength < Settings.Default.MaxContentLength)
                        {
                            portion.Add(t);
                            currentLength += dLength;
                        }
                        else
                        {
                            runBulk = true;
                            i--;
                        }
                    }

                    if (runBulk)
                    {
                        var portion1 = portion.ToList();
                        await Client.Instance.BulkAsync(r => r.IndexMany(portion1, GetMeta).SourceExcludes("attachments"));
                        for (var j = portionStart; j < i; j++)
                        {
                            var doc = data[j] as WrapperWithDoc;
                            if (doc != null && doc.Document != null)
                            {
                                doc.Document.Data = null;
                                doc.Document = null;
                            }
                            doc = null;
                        }

                        portionStart = i;
                        portion = new List<T>();
                        currentLength = 0L;
                        GC.Collect();
                    }
                }
            }
            else
            {
                foreach (var item in data)
                {
                    BeforeIndex(item);
                }

                await Client.Instance.BulkAsync(r => r.IndexMany(data, GetMeta));
            }
        }

        internal void Update(T data, bool immediately = true, params Expression<Func<T, object>>[] fields)
        {
            CreateIfNotExist(data);
            Client.Instance.Update(DocumentPath<T>.Id(data), r => GetMetaForUpdate(r, data, immediately, fields));
        }

        internal void Update(T data, UpdateAction action, Expression<Func<T, IList>> fields, bool immediately = true)
        {
            CreateIfNotExist(data);
            Client.Instance.Update(DocumentPath<T>.Id(data), r => GetMetaForUpdate(r, data, action, fields, immediately));
        }

        internal void Update(T data, Expression<Func<Selector<T>, Selector<T>>> expression, int tenantId, bool immediately = true, params Expression<Func<T, object>>[] fields)
        {
            CreateIfNotExist(data);
            Client.Instance.UpdateByQuery(GetDescriptorForUpdate(data, expression, tenantId, immediately, fields));
        }

        internal void Update(T data, Expression<Func<Selector<T>, Selector<T>>> expression, int tenantId, UpdateAction action, Expression<Func<T, IList>> fields, bool immediately = true)
        {
            CreateIfNotExist(data);
            Client.Instance.UpdateByQuery(GetDescriptorForUpdate(data, expression, tenantId, action, fields, immediately));
        }

        internal void Delete(T data, bool immediately = true)
        {
            Client.Instance.Delete<T>(data, r => GetMetaForDelete(r, immediately));
        }

        internal void Delete(Expression<Func<Selector<T>, Selector<T>>> expression, int tenantId, bool immediately = true)
        {
            Client.Instance.DeleteByQuery(GetDescriptorForDelete(expression, tenantId, immediately));
        }

        public void Flush()
        {
            Client.Instance.Indices.Flush(new FlushRequest(IndexName));
        }

        public void Refresh()
        {
            Client.Instance.Indices.Refresh(new RefreshRequest(IndexName));
        }

        internal bool CheckExist(T data)
        {
            try
            {
                if (IsExist) return true;

                lock (Locker)
                {
                    if (IsExist) return true;

                    IsExist = Client.Instance.Indices.Exists(data.IndexName).Exists;
                    Cts = new CancellationTokenSource();
                    if (IsExist) return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error("CheckExist " + data.IndexName, e);
            }
            return false;
        }

        void IIndexer.Check()
        {
            var data = new T();
            if (!CheckExist(data)) return;

            var result = false;
            var currentMappings = Client.Instance.Indices.GetMapping<T>(r => r.Index(data.IndexName));
            var newMappings = GetMappings(data).Invoke(new CreateIndexDescriptor(data.IndexName));

            var newMappingDict = new Dictionary<string, string>();
            var props = newMappings.Mappings.Properties.ToList();
            foreach (var prop in props.Where(r => r.Key.Property != null && r.Key.Property.Name != "Document"))
            {
                var propKey = prop.Key.Property.Name.ToLowerCamelCase();
                var key = newMappings.Index.Name + "." + propKey;
                if (prop.Key.Property.CustomAttributes.Any())
                {
                    newMappingDict.Add(key, props.Any(r => r.Key == propKey && r.Value is INestedProperty) ? FieldType.Nested.GetStringValue() : prop.Value.Type);
                }

                var obj = prop.Value as ObjectProperty;

                if (obj != null)
                {
                    foreach (var objProp in obj.Properties)
                    {
                        newMappingDict.Add(key + "." + objProp.Key.Property.Name.ToLowerCamelCase(), objProp.Value.Type);
                    }
                }
            }

            foreach (var ind in currentMappings.Indices)
            {
                foreach (var prop in ind.Value.Mappings.Properties.Where(r => r.Key.Name != "document"))
                {
                    var key = ind.Key.Name + "." + prop.Key.Name.ToLowerCamelCase();

                    if (!newMappingDict.Contains(new KeyValuePair<string, string>(key, prop.Value.Type)))
                    {
                        result = true;
                        break;
                    }

                    var nested = prop.Value as NestedProperty ?? prop.Value as ObjectProperty;

                    if (nested != null)
                    {
                        if (nested.Properties.Any(nProp => !newMappingDict.Contains(new KeyValuePair<string, string>(key + "." + nProp.Key.Name.ToLowerCamelCase(), nProp.Value.Type))))
                        {
                            result = true;
                        }
                    }
                }
            }


            if (result)
            {
                Clear();
            }
        }

        async Task IIndexer.ReIndex()
        {
            while (Launcher.IsStarted && Launcher.Indexing == Wrapper.IndexName)
            {
                await Task.Delay(10000);
            }

            Clear();
            //((IIndexer) this).IndexAll();
        }

        private void Clear()
        {
            using (var db = DbManager.FromHttpContext("default"))
            {
                db.ExecuteNonQuery(new SqlDelete("webstudio_index").Where("index_name", Wrapper.IndexName));
            }

            Logger.DebugFormat("Delete {0}", Wrapper.IndexName);
            Client.Instance.Indices.Delete(Wrapper.IndexName);
            Notify.Publish(this, CacheNotifyAction.Any);
            CreateIfNotExist(new T());
        }

        void IIndexer.IndexAll()
        {
            var now = DateTime.UtcNow;

            DateTime lastIndexed;

            using (var db = DbManager.FromHttpContext("default"))
            {
                lastIndexed = db.ExecuteScalar<DateTime>(new SqlQuery("webstudio_index").Select("last_modified").Where("index_name", Wrapper.IndexName));
            }

            if (lastIndexed.Equals(DateTime.MinValue))
            {
                CreateIfNotExist(new T());
            }

            var meta = Count(lastIndexed);
            Logger.DebugFormat("Index: {0},Count {1},Max: {2},Min: {3}", IndexName, meta.Item1, meta.Item2, meta.Item3);

            var ids = new List<long>() { meta.Item3 };
            ids.AddRange(Ids(lastIndexed));
            ids.Add(meta.Item2);

            var j = 0;
            var tasks = new List<Task>();

            for (var i = 0; i < ids.Count - 1; i ++)
            {
                if (Settings.Default.Threads == 1)
                {
                    IndexAllGetData(ids[i], ids[i+1], lastIndexed);
                }
                else
                {
                    tasks.Add(IndexAllGetDataAsync(ids[i], ids[i+1], lastIndexed));
                    j++;
                    if (j >= Settings.Default.Threads)
                    {
                        Task.WaitAll(tasks.ToArray());
                        tasks = new List<Task>();
                        j = 0;
                    }
                }
            }

            using (var db = DbManager.FromHttpContext("default"))
            {
                db.ExecuteNonQuery(
                    new SqlInsert("webstudio_index", true)
                    .InColumnValue("index_name", Wrapper.IndexName)
                    .InColumnValue("last_modified", now)
                    );
            }

            Logger.DebugFormat("index completed {0}", Wrapper.IndexName);
        }

        long IIndexer.Count()
        {
            var meta = Count(DateTime.MinValue);

            Logger.DebugFormat("Index: {0}, TotalCount {1}", IndexName, meta.Item1);

            return meta.Item1;
        }

        internal IReadOnlyCollection<T> Select(Expression<Func<Selector<T>, Selector<T>>> expression, bool onlyId = false)
        {
            var func = expression.Compile();
            var selector = new Selector<T>();
            var descriptor = func(selector).Where(r => r.TenantId, CoreContext.TenantManager.GetCurrentTenant().TenantId);
            return Client.Instance.Search(descriptor.GetDescriptor(this, onlyId)).Documents;
        }

        internal IReadOnlyCollection<T> Select(Expression<Func<Selector<T>, Selector<T>>> expression, bool onlyId, out long total)
        {
            var func = expression.Compile();
            var selector = new Selector<T>();
            var descriptor = func(selector).Where(r => r.TenantId, CoreContext.TenantManager.GetCurrentTenant().TenantId);
            var result = Client.Instance.Search(descriptor.GetDescriptor(this, onlyId));
            total = result.Total;
            return result.Documents;
        }

        private void BeforeIndex(T data)
        {
            CreateIfNotExist(data);

            var wrapperWithDoc = data as WrapperWithDoc;
            if (wrapperWithDoc != null)
            {
                wrapperWithDoc.InitDocument(SearchSettings.CanIndexByContent<T>(data.TenantId));
            }
        }

        private async Task BeforeIndexAsync(T data)
        {
            CreateIfNotExist(data);

            var wrapperWithDoc = data as WrapperWithDoc;
            if (wrapperWithDoc != null)
            {
                await wrapperWithDoc.InitDocumentAsync(SearchSettings.CanIndexByContent<T>(data.TenantId)).ConfigureAwait(false);
            }
        }

        private void CreateIfNotExist(T data)
        {
            try
            {
                if (CheckExist(data)) return;

                lock (Locker)
                {
                    var columns = data.GetAnalyzers();
                    var nestedColumns = data.GetNested();

                    if (!columns.Any() && !nestedColumns.Any())
                    {
                        Client.Instance.Indices.Create(data.IndexName);
                    }
                    else
                    {
                        Client.Instance.Indices.Create(data.IndexName, GetMappings(data));
                    }

                    IsExist = true;
                }
            }
            catch (Exception e)
            {
                Logger.Error("CreateIfNotExist", e);
            }
        }

        public Func<CreateIndexDescriptor, ICreateIndexRequest> GetMappings(T data)
        {
            var columns = data.GetAnalyzers();
            var nestedColumns = data.GetNested();

            Func<AnalyzersDescriptor, IPromise<IAnalyzers>> analyzers = b =>
            {
                foreach (var c in Enum.GetNames(typeof(Analyzer)))
                {
                    var c1 = c;
                    b.Custom(c1 + "custom", ca => ca.Tokenizer(c1).Filters(Filter.lowercase.ToString()).CharFilters(CharFilter.io.ToString()));
                }

                foreach (var c in columns)
                {
                    if (c.Value.CharFilter == CharFilter.io) continue;
                    var charFilters = new List<string>();
                    foreach (var r in Enum.GetValues(typeof(CharFilter)))
                    {
                        if ((c.Value.CharFilter & (CharFilter)r) == (CharFilter)r) charFilters.Add(r.ToString());
                    }

                    var c1 = c;
                    b.Custom(c1.Key, ca => ca.Tokenizer(c1.Value.Analyzer.ToString()).Filters(c1.Value.Filter.ToString()).CharFilters(charFilters));
                }

                if (data is WrapperWithDoc)
                {
                    b.Custom("document", ca => ca.Tokenizer(Analyzer.whitespace.ToString()).Filters(Filter.lowercase.ToString()).CharFilters(CharFilter.io.ToString()));
                }

                return b;
            };

            Func<PropertiesDescriptor<T>, IPromise<IProperties>> nestedSelector = p =>
            {
                foreach (var c in nestedColumns)
                {
                    var isNested = c.Key.IsGenericType;
                    Type prop;
                    MethodInfo nested;
                    Type typeDescriptor;

                    if (isNested)
                    {
                        prop = c.Key.GenericTypeArguments[0];
                        nested = p.GetType().GetMethod("Nested");
                        typeDescriptor = typeof(NestedPropertyDescriptor<,>);
                    }
                    else
                    {
                        prop = c.Key;
                        nested = p.GetType().GetMethod("Object");
                        typeDescriptor = typeof(ObjectTypeDescriptor<,>);
                    }

                    var desc = typeDescriptor.MakeGenericType(typeof(T), prop);

                    var methods = desc.GetMethods();
                    var name = methods.FirstOrDefault(r => r.Name == "Name" && r.GetParameters().FirstOrDefault(q => q.ParameterType == typeof(PropertyName)) != null);
                    var autoMap = methods.FirstOrDefault(r => r.Name == "AutoMap" && r.GetParameters().Length == 2);
                    var props = methods.FirstOrDefault(r => r.Name == "Properties");
                    if (name == null || autoMap == null || props == null) continue;

                    var param = Expression.Parameter(desc, "a");
                    var nameFunc = Expression.Call(param, name, Expression.Constant(new PropertyName(c.Value.ToLowerCamelCase()))); //a.Name(value(Nest.PropertyName))
                    var autoMapFunc = Expression.Call(param, autoMap, Expression.Constant(null, typeof(IPropertyVisitor)), Expression.Constant(0)); //a.AutoMap()

                    var inst = (Wrapper)Activator.CreateInstance(prop);
                    var instMethods = prop.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
                    var getProperties = instMethods.First(r => r.Name == "GetProperties").MakeGenericMethod(prop);
                    var propsFunc = Expression.Call(param, props, Expression.Constant(getProperties.Invoke(inst, null))); //a.AutoMap()

                    var nestedFunc = Expression.Lambda(Expression.Block(nameFunc, autoMapFunc, propsFunc), param).Compile();
                    var fooRef = nested.MakeGenericMethod(prop);
                    fooRef.Invoke(p, new object[] { nestedFunc });//p.Nested<Wrapper>(r=> r.Name(c.Value.ToLowerCamelCase()).AutoMap().Properties(getProperties()))
                }

                return p;
            };

            return c =>
               c.Settings(r => r.Analysis(a => a.Analyzers(analyzers).CharFilters(d => d.HtmlStrip(CharFilter.html.ToString()).Mapping(CharFilter.io.ToString(), m => m.Mappings("ё => е", "Ё => Е")))))
                .Map<T>(m => m.AutoMap<T>().Properties(data.GetProperties<T>()).Properties(nestedSelector));
        }

        private IIndexRequest<T> GetMeta(IndexDescriptor<T> request, T data, bool immediately = true)
        {
            var result = request.Index(IndexName).Id(data.Id);

            if (immediately)
            {
                result.Refresh(Elasticsearch.Net.Refresh.True);
            }

            if (data is WrapperWithDoc)
            {
                result.Pipeline("attachments");
            }

            return result;
        }
        private IBulkIndexOperation<T> GetMeta(BulkIndexDescriptor<T> desc, T data)
        {
            var result = desc.Index(IndexName).Id(data.Id);

            if (data is WrapperWithDoc)
            {
                result.Pipeline("attachments");
            }

            return result;
        }

        private IUpdateRequest<T, T> GetMetaForUpdate(UpdateDescriptor<T, T> request, T data, bool immediately = true, params Expression<Func<T, object>>[] fields)
        {
            var result = request.Index(IndexName);

            if (fields.Any())
            {
                result.Script(GetScriptUpdateByQuery(data, fields));
            }
            else
            {
                result.Doc(data);
            }

            if (immediately)
            {
                result.Refresh(Elasticsearch.Net.Refresh.True);
            }

            return result;
        }

        private Func<ScriptDescriptor, IScript> GetScriptUpdateByQuery(T data, params Expression<Func<T, object>>[] fields)
        {
            var source = new StringBuilder();
            var parameters = new Dictionary<string, object>();

            for (var i = 0; i < fields.Length; i++)
            {
                var func = fields[i].Compile();
                var newValue = func(data);
                string name;

                var expression = fields[i].Body;
                var isList = expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(List<>);


                MemberExpression member;
                var sourceExprText = "";

                while (!string.IsNullOrEmpty(name = TryGetName(expression, out member)))
                {
                    sourceExprText = "." + name + sourceExprText;
                    expression = member.Expression;
                }

                if (isList)
                {
                    UpdateByAction(UpdateAction.Add, (IList)newValue, sourceExprText, parameters, source);
                }
                else
                {
                    if (newValue == default(T))
                    {
                        source.AppendFormat("ctx._source.remove('{0}');", sourceExprText.Substring(1));
                    }
                    else
                    {
                        var pkey = "p" + sourceExprText.Replace(".", "");
                        source.AppendFormat("ctx._source{0} = params.{1};", sourceExprText, pkey);
                        parameters.Add(pkey, newValue);
                    }
                }
            }

            var sourceData = source.ToString();

            return r => r.Source(sourceData).Params(parameters);
        }

        private IUpdateRequest<T, T> GetMetaForUpdate(UpdateDescriptor<T, T> request, T data, UpdateAction action, Expression<Func<T, IList>> fields, bool immediately = true)
        {
            var result = request.Index(IndexName).Script(GetScriptForUpdate(data, action, fields));

            if (immediately)
            {
                result.Refresh(Elasticsearch.Net.Refresh.True);
            }

            return result;
        }

        private Func<ScriptDescriptor, IScript> GetScriptForUpdate(T data, UpdateAction action, Expression<Func<T, IList>> fields)
        {
            var source = new StringBuilder();

            var func = fields.Compile();
            var newValue = func(data);
            string name;

            var expression = fields.Body;

            MemberExpression member;
            var sourceExprText = "";

            while (!string.IsNullOrEmpty(name = TryGetName(expression, out member)))
            {
                sourceExprText = "." + name + sourceExprText;
                expression = member.Expression;
            }

            var parameters = new Dictionary<string, object>();

            UpdateByAction(action, newValue, sourceExprText, parameters, source);

            return r => r.Source(source.ToString()).Params(parameters);
        }

        private void UpdateByAction(UpdateAction action, IList newValue, string key, Dictionary<string, object> parameters, StringBuilder source)
        {
            var paramKey = "p" + key.Replace(".", "");
            switch (action)
            {
                case UpdateAction.Add:
                    for (var i = 0; i < newValue.Count; i++)
                    {
                        parameters.Add(paramKey + i, newValue[i]);
                        source.AppendFormat("if (!ctx._source{0}.contains(params.{1})){{ctx._source{0}.add(params.{1})}}", key, paramKey + i);
                    }
                    break;
                case UpdateAction.Replace:
                    parameters.Add(paramKey, newValue);
                    source.AppendFormat("ctx._source{0} = params.{1};", key, paramKey);
                    break;
                case UpdateAction.Remove:
                    for (var i = 0; i < newValue.Count; i++)
                    {
                        parameters.Add(paramKey + i, newValue[i]);
                        source.AppendFormat("ctx._source{0}.removeIf(item -> item.id == params.{1}.id)", key, paramKey + i);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("action", action, null);
            }
        }

        private string TryGetName(Expression expr, out MemberExpression member)
        {
            member = expr as MemberExpression;
            if (member == null)
            {
                var unary = expr as UnaryExpression;
                if (unary != null)
                {
                    member = unary.Operand as MemberExpression;
                }
            }

            return member == null ? "" : member.Member.Name.ToLowerCamelCase();
        }

        private IDeleteRequest GetMetaForDelete(DeleteDescriptor<T> request, bool immediately = true)
        {
            var result = request.Index(IndexName);
            if (immediately)
            {
                result.Refresh(Elasticsearch.Net.Refresh.True);
            }
            return result;
        }

        private Func<string, string> SelectWithAlias(string alias)
        {
            return column => column.Contains('(') ? column : alias + "." + column;
        }

        private void AddJoins(Wrapper wrapper, SqlQuery dataQuery, string alias, bool sub = false)
        {
            var selectWithAliasWrapper = SelectWithAlias(alias);
            var joins = wrapper.GetJoins();
            var j = 0;

            foreach (var join in joins)
            {
                var joinWrapper = join.Key;
                var joinAttr = join.Value.GetCustomAttribute<JoinAttribute>();
                var joinAlias = alias + ++j;

                var tableWithAlias = joinWrapper.Table + " " + joinAlias;
                var selectWithAliasJoin = SelectWithAlias(joinAlias);

                Exp on = null;
                for (var k = 0; k < joinAttr.ColumnsFrom.Length; k++)
                {
                    on = on & Exp.EqColumns(selectWithAliasWrapper(joinAttr.ColumnsFrom[k]), selectWithAliasJoin(joinAttr.ColumnsTo[k]));
                }

                if (joinAttr.JoinType == JoinTypeEnum.Sub)
                {
                    var subQuery = new SqlQuery(tableWithAlias).Where(on);
                    var joinCols = joinWrapper.GetColumnNames(joinAlias);
                    if (joinCols.Any())
                    {
                        subQuery.Select("concat(\"[\", group_concat(JSON_ARRAY(" + string.Join(",", joinCols) + ") SEPARATOR ','), \"]\")");
                    }

                    joinWrapper.AddConditions(joinAlias, subQuery);

                    AddJoins(joinWrapper, subQuery, joinAlias, true);

                    dataQuery.Select(subQuery);
                }
                else
                {
                    if (sub)
                    {
                        var joinCols = joinWrapper.GetColumnNames(joinAlias);
                        if (joinCols.Any())
                        {
                            dataQuery.Select("concat(\"[\", group_concat(JSON_ARRAY(" + string.Join(",", joinCols) + ") SEPARATOR ','), \"]\")");
                        }
                    }
                    else
                    {
                        dataQuery.Select(joinWrapper.GetColumnNames(joinAlias));
                    }

                    switch (joinAttr.JoinType)
                    {
                        //case JoinTypeEnum.Left:
                        //    dataQuery.LeftOuterJoin(tableWithAlias, on);
                        //    break;
                        //case JoinTypeEnum.Right:
                        //    dataQuery.RightOuterJoin(tableWithAlias, on);
                        //    break;
                        case JoinTypeEnum.Inner:
                            dataQuery.InnerJoin(tableWithAlias, on);
                            break;
                    }

                    joinWrapper.AddConditions(joinAlias, dataQuery);
                }
            }
        }

        private Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> GetDescriptorForDelete(Expression<Func<Selector<T>, Selector<T>>> expression, int tenantId, bool immediately = true)
        {
            var func = expression.Compile();
            var selector = new Selector<T>();
            var descriptor = func(selector).Where(r => r.TenantId, tenantId);
            return descriptor.GetDescriptorForDelete(this, immediately);
        }

        private Func<UpdateByQueryDescriptor<T>, IUpdateByQueryRequest> GetDescriptorForUpdate(T data, Expression<Func<Selector<T>, Selector<T>>> expression, int tenantId, bool immediately = true, params Expression<Func<T, object>>[] fields)
        {
            var func = expression.Compile();
            var selector = new Selector<T>();
            var descriptor = func(selector).Where(r => r.TenantId, tenantId);
            return descriptor.GetDescriptorForUpdate(this, GetScriptUpdateByQuery(data, fields), immediately);
        }

        private Func<UpdateByQueryDescriptor<T>, IUpdateByQueryRequest> GetDescriptorForUpdate(T data, Expression<Func<Selector<T>, Selector<T>>> expression, int tenantId, UpdateAction action, Expression<Func<T, IList>> fields, bool immediately = true)
        {
            var func = expression.Compile();
            var selector = new Selector<T>();
            var descriptor = func(selector).Where(r => r.TenantId, tenantId);
            return descriptor.GetDescriptorForUpdate(this, GetScriptForUpdate(data, action, fields), immediately);
        }

        private void IndexAllGetData(long start, long stop, DateTime lastIndexed)
        {
            try
            {
                var convertedData = GetDataFromDb(start, stop, lastIndexed);

                FactoryIndexer<T>.Index(convertedData);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private async Task IndexAllGetDataAsync(long start, long stop, DateTime lastIndexed)
        {
            try
            {
                var convertedData = GetDataFromDb(start, stop, lastIndexed);

                await FactoryIndexer<T>.IndexAsync(convertedData).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private List<T> GetDataFromDb(long start, long stop, DateTime lastIndexed)
        {
            var idColumn = Wrapper.GetColumnName(ColumnTypeEnum.Id, Alias);

            List<object[]> data;

            var dataQuery = GetBaseQuery(lastIndexed)
                .Select(Wrapper.GetColumnNames(Alias))
                .Where(Exp.Between(idColumn, start, stop));

            AddJoins(Wrapper, dataQuery, Alias);

            using (var db = new DbManager("default", 1800000))
            {
                db.ExecuteNonQuery("SET SESSION group_concat_max_len = 4294967295;");
                data = db.ExecuteList(dataQuery);
            }

            var converter = Wrapper.GetDataConverter();
            return data.Select(r => (T)converter(r)).ToList();
        }

        private Tuple<long, long, long> Count(DateTime lastIndexed)
        {
            var idColumn = Wrapper.GetColumnName(ColumnTypeEnum.Id, Alias);

            using (var db = new DbManager("default", 1800000))
            {
                var dataQuery = GetBaseQuery(lastIndexed)
                    .Select(idColumn)
                    .OrderBy(idColumn, true)
                    .SetMaxResults(1);

                var minid = db.ExecuteList(dataQuery).Select(r => Convert.ToInt64(r[0])).FirstOrDefault();

                dataQuery = GetBaseQuery(lastIndexed)
                    .Select(idColumn)
                    .OrderBy(idColumn, false)
                    .SetMaxResults(1);

                var maxid = db.ExecuteList(dataQuery).Select(r => Convert.ToInt64(r[0])).FirstOrDefault();

                dataQuery = GetBaseQuery(lastIndexed)
                    .SelectCount();

                var count = db.ExecuteList(dataQuery).Select(r => Convert.ToInt64(r[0])).FirstOrDefault();

                return new Tuple<long, long, long>(count, maxid, minid);
            }
        }

        private List<long> Ids(DateTime lastIndexed)
        {
            var idColumn = Wrapper.GetColumnName(ColumnTypeEnum.Id, Alias);
            long start = 0;
            var result = new List<long>();
            using (var db = new DbManager("default", 1800000))
            {
                while(true)
                {
                    var dataQuery = GetBaseQuery(lastIndexed)
                        .Select(idColumn)
                        .Where(Exp.Ge(idColumn, start))
                        .OrderBy(idColumn, true)
                        .SetFirstResult(QueryLimit)
                        .SetMaxResults(1);

                    var id = db.ExecuteList(dataQuery).Select(r => Convert.ToInt64(r[0])).FirstOrDefault();
                    if (id != 0)
                    {
                        start = id;
                        result.Add(id);
                    }
                    else
                    {
                        break;
                    }
                }

                return result;
            }
        }

        private SqlQuery GetBaseQuery(DateTime lastIndexed)
        {
            var dataQuery = new SqlQuery(Wrapper.Table + " " + Alias);
            var tenantIdColumn = Wrapper.GetColumnName(ColumnTypeEnum.TenantId, Alias);
            var lastModifiedColumn = Wrapper.GetColumnName(ColumnTypeEnum.LastModified, Alias);

            if (!string.IsNullOrEmpty(tenantIdColumn))
            {
                dataQuery.InnerJoin("tenants_tenants t", Exp.EqColumns(tenantIdColumn, "t.id"))
                    .Where("t.status", TenantStatus.Active);
            }


            Wrapper.AddConditions(Alias, dataQuery);

            if (!DateTime.MinValue.Equals(lastIndexed))
            {
                dataQuery.Where(Exp.Gt(lastModifiedColumn, lastIndexed));
            }

            return dataQuery;
        }
    }

    static class CamelCaseExtension
    {
        internal static string ToLowerCamelCase(this string str)
        {
            return str.ToLowerInvariant()[0] + str.Substring(1);
        }
    }


    public enum UpdateAction
    {
        Add,
        Replace,
        Remove
    }
}