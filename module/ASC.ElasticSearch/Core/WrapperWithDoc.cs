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
using System.IO;
using System.Text;
using System.Threading.Tasks;

using ASC.Common.Logging;
using ASC.ElasticSearch.Service;

using Nest;

using Newtonsoft.Json;

namespace ASC.ElasticSearch.Core
{
    public abstract class WrapperWithDoc : Wrapper
    {
        public Document Document { get; set; }

        private ILog Log = LogManager.GetLogger("ASC.Indexer");

        public static readonly long MaxFileSize = Settings.Default.MaxFileSize;

        protected virtual Task<string> GetDocumentDataAsync()
        {
            return Task.FromResult(GetDocumentData());
        }

        protected virtual string GetDocumentData()
        {
            return "";
        }

        protected abstract Stream GetDocumentStream();
        protected abstract Task<Stream> GetDocumentStreamAsync();

        [Ignore, JsonIgnore]
        public abstract string SettingsTitle { get; }

        internal void InitDocument(bool index)
        {
            Document = new Document
            {
                Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(""))
            };

            try
            {
                if (!index) return;

                var data = GetDocumentData();

                if (!string.IsNullOrEmpty(data))
                {
                    Document.Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
                    return;
                }

                using (var stream = GetDocumentStream())
                {
                    if (stream == null) return;

                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        Document.Data = Convert.ToBase64String(ms.GetBuffer());
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Log.Error("InitDocument FileNotFoundException", e);
            }
            catch (Exception e)
            {
                Log.Error("InitDocument", e);
            }
        }

        internal async Task InitDocumentAsync(bool index)
        {
            Document = new Document
            {
                Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(""))
            };

            try
            {
                if (!index) return;

                var data = await GetDocumentDataAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(data))
                {
                    Document.Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
                    return;
                }

                using (var stream = await GetDocumentStreamAsync().ConfigureAwait(false))
                {
                    if (stream == null) return;

                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        Document.Data = Convert.ToBase64String(ms.GetBuffer());
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Log.Error("InitDocument FileNotFoundException", e);
            }
            catch (Exception e)
            {
                Log.Error("InitDocument", e);
            }
        }
    }
}
