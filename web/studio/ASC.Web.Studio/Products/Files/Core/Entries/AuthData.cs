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


using System.Diagnostics;
using System.Runtime.Serialization;

namespace ASC.Files.Core
{
    [DataContract(Name = "AuthData", Namespace = "")]
    [DebuggerDisplay("{Login} {Password} {Token} {Url}")]
    public class AuthData
    {
        ///<example name="login">login</example>
        [DataMember(Name = "login")]
        public string Login { get; set; }

        ///<example name="password">password</example>
        [DataMember(Name = "password")]
        public string Password { get; set; }

        ///<example name="token">token</example>
        [DataMember(Name = "token")]
        public string Token { get; set; }

        ///<example name="url">url</example>
        [DataMember(Name = "url")]
        public string Url { get; set; }

        public AuthData(string url = null, string login = null, string password = null, string token = null)
        {
            Url = url ?? "";
            Login = login ?? "";
            Password = password ?? "";
            Token = token ?? "";
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty((Url ?? "") + (Login ?? "") + (Password ?? "") + (Token ?? ""));
        }
    }
}