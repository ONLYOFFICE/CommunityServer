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


using System.Collections.Generic;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Mail.Autoreply.ParameterResolvers;

namespace ASC.Mail.Autoreply
{
    internal class ApiRequest
    {
        public string Method { get; set; }

        public string Url { get; set; }

        public List<RequestParameter> Parameters { get; set; } 

        public Tenant Tenant { get; set; }

        public UserInfo User { get; set; }

        public List<RequestFileInfo> FilesToPost { get; set; }

        public ApiRequest(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Url = url.Trim('/');
            }
        }

        public override string ToString()
        {
            return string.Format("t:{0}; u:{1}; {2} {3}", Tenant.TenantId, User.ID, Method, Url);
        }
    }

    internal class RequestParameter
    {
        public string Name { get; private set; }
        public object Value { get; set; }
        public IParameterResolver ValueResolver { get; private set; }

        public RequestParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public RequestParameter(string name, IParameterResolver valueResolver)
        {
            Name = name;
            ValueResolver = valueResolver;
        }
    }

    internal class RequestFileInfo
    {
        public byte[] Body { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
    }
}