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


using System.Text;

namespace ASC.FederatedLogin.Helpers
{
    public class HashHelper
    {
        public static int CombineHashCodes(int hash1, int hash2)
        {
            if (hash2 == 0)
                return hash1;
            return (((hash1 << 5) + hash1) ^ hash2);
        }

        //Use this luke!!!
        public static int StringHash(string text)
        {
            return text.GetHashCode();
        }

        public static string MD5(string text)
        {
            return MD5(text, Encoding.Default);
        }

        public static string MD5(string text, Encoding encoding)
        {
            return MD5String(encoding.GetBytes(text));
        }

        public static string MD5String(byte[] data)
        {
            var hash = MD5(data);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static byte[] MD5(byte[] data)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            return md5.ComputeHash(data);
        }
    }
}