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

using System.Runtime.Serialization;
using ASC.Api.Enums;
using ASC.Api.Interfaces;

#endregion

namespace ASC.Api.Impl
{
    [DataContract(Name = "result", Namespace = "")]
    internal class ApiStandartResponce : IApiStandartResponce
    {
        #region IApiStandartResponce Members

        [DataMember(Name = "response", EmitDefaultValue = false, Order = 200)]
        public object Response { get; set; }

        [DataMember(Name = "error", EmitDefaultValue = false, Order = 210)]
        public ErrorWrapper Error { get; set; }

        [DataMember(Name = "status", EmitDefaultValue = true, Order = 100)]
        public ApiStatus Status { get; set; }

        [DataMember(Name = "statusCode", EmitDefaultValue = false, Order = 101)]
        public long Code { get; set; }

        [DataMember(Name = "count", EmitDefaultValue = false, Order = 10)]
        public long Count { get; set; }

        [DataMember(Name = "startIndex", EmitDefaultValue = false, Order = 11)]
        public long StartIndex { get; set; }

        [DataMember(Name = "nextIndex", EmitDefaultValue = false, Order = 12)]
        public long? NextPage { get; set; }

        [DataMember(Name = "total", EmitDefaultValue = false, Order = 13)]
        public long? TotalCount { get; set; }


        public ApiContext ApiContext { get; set; }
        #endregion
    }
}