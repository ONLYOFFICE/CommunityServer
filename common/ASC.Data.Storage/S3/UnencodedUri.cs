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
using System.Runtime.Serialization;

namespace ASC.Data.Storage.S3
{
    public class UnencodedUri : Uri
    {
        public UnencodedUri(string uriString)
            : base(uriString)
        {
        }

        public UnencodedUri(string uriString, UriKind uriKind)
            : base(uriString, uriKind)
        {
        }

        public UnencodedUri(Uri baseUri, string relativeUri)
            : base(baseUri, relativeUri)
        {
        }

        public UnencodedUri(Uri baseUri, Uri relativeUri)
            : base(baseUri, relativeUri)
        {
        }

        protected UnencodedUri(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public override string ToString()
        {
            return OriginalString;
        }
    }
}
