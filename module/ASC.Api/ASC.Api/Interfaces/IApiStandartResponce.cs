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


#region usings

using System;
using System.Collections;
using System.Runtime.Serialization;
using ASC.Api.Enums;
using ASC.Api.Impl;

#endregion

namespace ASC.Api.Interfaces
{
    public interface IApiStandartResponce
    {
        object Response { get; set; }
        ErrorWrapper Error { get; set; }
        ApiStatus Status { get; set; }
        long Code { get; set; }
        long Count { get; set; }
        long StartIndex { get; set; }
        long? NextPage { get; set; }
        long? TotalCount { get; set; }
        ApiContext ApiContext { get; set; }
    }

    [DataContract(Name = "error", Namespace = "")]
    public class ErrorWrapper
    {
        public ErrorWrapper()
        {
        }

        public ErrorWrapper(Exception exception)
        {
            //Unwrap
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            Message = exception.Message;
#if (DEBUG)
            Type = exception.GetType().ToString();
            Stack = exception.StackTrace;
#endif

            HResult = exception.HResult;
            Data = exception.Data;

        }

        [DataMember(Name = "message", EmitDefaultValue = false, Order = 2)]
        public string Message { get; set; }

        [DataMember(Name = "type", EmitDefaultValue = false, Order = 3)]
        public string Type { get; set; }

        [DataMember(Name = "stack", EmitDefaultValue = false, Order = 3)]
        public string Stack { get; set; }

        [DataMember(Name = "hresult", EmitDefaultValue = false, Order = 3)]
        public int HResult { get; set; }

        [DataMember(Name = "data", EmitDefaultValue = false, Order = 3)]
        public IDictionary Data { get; set; }
    }
}