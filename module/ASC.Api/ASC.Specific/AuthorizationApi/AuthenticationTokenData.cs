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


using System.Runtime.Serialization;

namespace ASC.Specific.AuthorizationApi
{
    [DataContract(Name = "token", Namespace = "")]
    public class AuthenticationTokenData
    {
        /// <example>abcde12345</example>
        /// <order>1</order>
        [DataMember(Order = 1)]
        public string Token { get; set; }

        /// <example>2020-11-24T05:36:20.4206897Z</example>
        /// <order>2</order>
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public ApiDateTime Expires { get; set; }

        /// <example>true</example>
        /// <order>3</order>
        [DataMember(Order = 3, EmitDefaultValue = false)]
        public bool Sms { get; set; }

        /// <example>8(999)999-99-99</example>
        /// <order>4</order>
        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string PhoneNoise { get; set; }

        /// <example>true</example>
        /// <order>5</order>
        [DataMember(Order = 5, EmitDefaultValue = false)]
        public bool Tfa { get; set; }

        /// <example>123dwa</example>
        /// <order>6</order>
        [DataMember(Order = 6, EmitDefaultValue = false)]
        public string TfaKey { get; set; }

        public static AuthenticationTokenData GetSample()
        {
            return new AuthenticationTokenData
            {
                Expires = ApiDateTime.GetSample(),
                Token = "abcde12345",
                Sms = false,
                PhoneNoise = null,
                Tfa = false,
                TfaKey = null
            };
        }
    }
}