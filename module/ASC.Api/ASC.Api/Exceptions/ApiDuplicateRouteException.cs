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
using ASC.Api.Interfaces;

namespace ASC.Api.Exceptions
{
    [Serializable]
    public class ApiDuplicateRouteException : Exception
    {
        public ApiDuplicateRouteException(IApiMethodCall currentMethod, IApiMethodCall registeredMethod)
            : base(string.Format("route '{0}' is already registered to '{1}'", currentMethod, registeredMethod))
        {
        }

        public ApiDuplicateRouteException()
        {
        }


        public ApiDuplicateRouteException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ApiDuplicateRouteException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}