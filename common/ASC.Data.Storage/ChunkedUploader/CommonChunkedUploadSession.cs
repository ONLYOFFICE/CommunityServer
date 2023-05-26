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
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace ASC.Core.ChunkedUploader
{
    [Serializable]
    public class CommonChunkedUploadSession : ICloneable
    {
        public string Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Expired { get; set; }

        public string Location { get; set; }

        public long BytesUploaded { get; set; }

        public long BytesTotal { get; set; }
        public bool LastChunk { get; set; }

        public int TenantId { get; set; }

        public Guid UserId { get; set; }

        public string LinkId { get; set; }

        public bool UseChunks { get; set; }

        public string CultureName { get; set; }

        public readonly Dictionary<string, object> Items = new Dictionary<string, object>();

        private const string TempPathKey = "TempPath";
        public string TempPath
        {
            get { return GetItemOrDefault<string>(TempPathKey); }
            set { Items[TempPathKey] = value; }
        }

        private const string UploadIdKey = "UploadId";
        public string UploadId
        {
            get { return GetItemOrDefault<string>(UploadIdKey); }
            set { Items[UploadIdKey] = value; }
        }

        private const string ChunksBufferKey = "ChunksBuffer";
        public string ChunksBuffer
        {
            get { return GetItemOrDefault<string>(ChunksBufferKey); }
            set { Items[ChunksBufferKey] = value; }
        }

        public CommonChunkedUploadSession(long bytesTotal)
        {
            Id = Guid.NewGuid().ToString("N");
            Created = DateTime.UtcNow;
            BytesUploaded = 0;
            BytesTotal = bytesTotal;
            UseChunks = true;
            LastChunk = false;
        }


        public T GetItemOrDefault<T>(string key)
        {
            if (Items.ContainsKey(key) && Items[key] != null)
            {
                if (Items[key] is T)
                {
                    return (T)Items[key];
                }

                var jToken = Items[key] as Newtonsoft.Json.Linq.JToken;
                if (jToken != null)
                {
                    var item = jToken.ToObject<T>();
                    Items[key] = item;
                    return item;
                }
            }
            return default(T);
        }

        public Stream Serialize()
        {
            var str = JsonConvert.SerializeObject(this);
            var res = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(str));
            return res;
        }

        public static T Deserialize<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                var str = reader.ReadToEnd();
                var res = JsonConvert.DeserializeObject<T>(str);
                return res;
            }
        }

        public virtual object Clone()
        {
            return (CommonChunkedUploadSession)MemberwiseClone();
        }
    }
}
