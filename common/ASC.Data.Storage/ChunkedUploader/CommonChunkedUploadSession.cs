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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

        public int TenantId { get; set; }

        public Guid UserId { get; set; }

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
        }

        public T GetItemOrDefault<T>(string key)
        {
            return Items.ContainsKey(key) && Items[key] is T ? (T)Items[key] : default(T);
        }

        public Stream Serialize()
        {
            var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, this);
            return stream;
        }

        public static CommonChunkedUploadSession Deserialize(Stream stream)
        {
            return (CommonChunkedUploadSession)new BinaryFormatter().Deserialize(stream);
        }

        public virtual object Clone()
        {
            return (CommonChunkedUploadSession) MemberwiseClone();
        }
    }
}
