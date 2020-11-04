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
using System.IO;
using System.Text;
using ASC.Common.Logging;
using Nest;
using Newtonsoft.Json;

namespace ASC.ElasticSearch.Core
{
    public abstract class WrapperWithDoc : Wrapper
    {
        public Document Document { get; set; }

        public const long  MaxContentLength = 2 * 1024 *1024 *1024L;

        protected abstract Stream GetDocumentStream();

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

                using (var stream = GetDocumentStream())
                {
                    if (stream == null) return;

                    Document = new Document
                    {
                        Data = Convert.ToBase64String(stream.GetCorrectBuffer())
                    };
                }
            }
            catch (FileNotFoundException e)
            {
                LogManager.GetLogger("ASC.Indexer").Error("InitDocument FileNotFoundException", e);
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC.Indexer").Error("InitDocument", e);
            }
        }
    }
}
