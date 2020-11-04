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
using System.Text;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.ElasticSearch.Service;

using Elasticsearch.Net;

using Nest;

namespace ASC.ElasticSearch
{
    class Client
    {
        private static volatile ElasticClient client;
        private static readonly object Locker = new object();
        private static readonly ILog Log = LogManager.GetLogger("ASC.Indexer");

        private Client()
        {

        }

        public static ElasticClient Instance
        {
            get
            {
                if (client != null) return client;

                lock (Locker)
                {
                    if (client != null) return client;

                    var launchSettings = CoreContext.Configuration.GetSection<Settings>(Tenant.DEFAULT_TENANT) ??
                                         Settings.Default;

                    var uri = new Uri(string.Format("{0}://{1}:{2}", launchSettings.Scheme, launchSettings.Host, launchSettings.Port));
                    var settings = new ConnectionSettings(new SingleNodeConnectionPool(uri))
                        .RequestTimeout(TimeSpan.FromMinutes(5))
                        .MaximumRetries(10)
                        .ThrowExceptions();

                    if (Log.IsTraceEnabled)
                    {
                        settings.DisableDirectStreaming().PrettyJson().EnableDebugMode(r =>
                        {
                            //Log.Trace(r.DebugInformation);

                            //if (r.RequestBodyInBytes != null)
                            //{
                            //    Log.TraceFormat("Request: {0}", Encoding.UTF8.GetString(r.RequestBodyInBytes));
                            //}

                            if (r.HttpStatusCode != null && (r.HttpStatusCode == 403 || r.HttpStatusCode == 500) && r.ResponseBodyInBytes != null)
                            {
                                Log.TraceFormat("Response: {0}", Encoding.UTF8.GetString(r.ResponseBodyInBytes));
                            }
                        });
                    }

                    return client = new ElasticClient(settings);
                }
            }
        }
    }
}
