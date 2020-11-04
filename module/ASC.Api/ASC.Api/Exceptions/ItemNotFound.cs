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
using System.Web;

namespace ASC.Api.Exceptions
{
    [Serializable]
    public class ItemNotFoundException : HttpException
    {

        public ItemNotFoundException():base(404,"Not found")
        {
        }

        public ItemNotFoundException(string message) : base(404,message)
        {
        }

        public ItemNotFoundException(string message, Exception inner) : base(404, message, inner)
        {
        }

        protected ItemNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}