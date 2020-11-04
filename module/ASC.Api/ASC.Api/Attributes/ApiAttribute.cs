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

namespace ASC.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ApiAttribute : Attribute
    {
        public ApiAttribute(string httpMethod, string path, bool requiresAuthorization = true, bool checkPayment = true)
        {
            Method = httpMethod;
            Path = path;
            RequiresAuthorization = requiresAuthorization;
            CheckPayment = checkPayment;
        }

        public string Method { get; set; }
        public string Path { get; set; }
        public bool RequiresAuthorization { get; set; }
        public bool CheckPayment { get; set; }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CreateAttribute : ApiAttribute
    {
        public CreateAttribute(string path, bool requiresAuthorization = true, bool checkPayment = true)
            : base("POST", path, requiresAuthorization, checkPayment)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class UpdateAttribute : ApiAttribute
    {
        public UpdateAttribute(string path, bool requiresAuthorization = true, bool checkPayment = true)
            : base("PUT", path, requiresAuthorization, checkPayment)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DeleteAttribute : ApiAttribute
    {
        public DeleteAttribute(string path, bool requiresAuthorization = true, bool checkPayment = true)
            : base("DELETE", path, requiresAuthorization, checkPayment)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ReadAttribute : ApiAttribute
    {
        public ReadAttribute(string path, bool requiresAuthorization = true, bool checkPayment = true)
            : base("GET", path, requiresAuthorization, checkPayment)
        {
        }
    }
}