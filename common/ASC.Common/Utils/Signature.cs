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


using ASC.Security.Cryptography;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace ASC.Common.Utils
{
    public static class Signature
    {
        public static string Create<T>(T obj)
        {
            return Create(obj, Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant()));
        }

        public static string Create<T>(T obj, string secret)
        {
            var serializer = new JavaScriptSerializer();
            var str = serializer.Serialize(obj);
            var payload = GetHashBase64(str + secret) + "?" + str;
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(payload));
        }

        public static T Read<T>(string signature)
        {
            return Read<T>(signature, Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant()));
        }

        public static T Read<T>(string signature, string secret)
        {
            try
            {
                var payloadParts = Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(signature)).Split('?');
                if (GetHashBase64(payloadParts[1] + secret) == payloadParts[0])
                {
                    //Sig correct
                    return new JavaScriptSerializer().Deserialize<T>(payloadParts[1]);
                }
            }
            catch (Exception)
            {
            }
            return default(T);
        }

        private static string GetHashBase64(string str)
        {
            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str)));
        }

        private static string GetHashBase64MD5(string str)
        {
            return Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str)));
        }
    }
}