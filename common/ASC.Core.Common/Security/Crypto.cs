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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ASC.Common.Security;

namespace ASC.Core
{
    public class Crypto
    {
        private static byte[] GetSK1(bool rewrite)
        {
            return GetSK(rewrite.GetType().Name.Length);
        }

        private static byte[] GetSK2(bool rewrite)
        {
            return GetSK(rewrite.GetType().Name.Length * 2);
        }

        private static byte[] GetSK(int seed)
        {
            var random = new AscRandom(seed);
            var randomKey = new byte[32];
            for (var i = 0; i < randomKey.Length; i++)
            {
                randomKey[i] = (byte)random.Next(byte.MaxValue);
            }
            return randomKey;
        }

        public static string GetV(string data, int keyno, bool reverse)
        {
            var hasher = Rijndael.Create();
            hasher.Key = keyno == 1 ? GetSK1(false) : GetSK2(false);
            hasher.IV = new byte[hasher.BlockSize >> 3];

            string result;
            if (reverse)
            {
                using (var ms = new MemoryStream())
                using (var ss = new CryptoStream(ms, hasher.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var buffer = Encoding.Unicode.GetBytes(data);
                    ss.Write(buffer, 0, buffer.Length);
                    ss.FlushFinalBlock();
                    hasher.Clear();
                    result = Convert.ToBase64String(ms.ToArray());
                }
            }
            else
            {
                var bytes = Convert.FromBase64String(data);
                using (var ms = new MemoryStream(bytes))
                using (var ss = new CryptoStream(ms, hasher.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    var buffer = new byte[bytes.Length];
                    var size = ss.Read(buffer, 0, buffer.Length);
                    hasher.Clear();
                    var newBuffer = new byte[size];
                    Array.Copy(buffer, newBuffer, size);
                    result = Encoding.Unicode.GetString(newBuffer);
                }
            }

            return result;
        }

        internal static byte[] GetV(byte[] data, int keyno, bool reverse)
        {
            var hasher = Rijndael.Create();
            hasher.Key = keyno == 1 ? GetSK1(false) : GetSK2(false);
            hasher.IV = new byte[hasher.BlockSize >> 3];

            byte[] result;
            if (reverse)
            {
                using (var ms = new MemoryStream())
                using (var ss = new CryptoStream(ms, hasher.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var buffer = data;
                    ss.Write(buffer, 0, buffer.Length);
                    ss.FlushFinalBlock();
                    hasher.Clear();
                    result = ms.ToArray();
                }
            }
            else
            {
                var bytes = data;
                using (var ms = new MemoryStream(bytes))
                using (var ss = new CryptoStream(ms, hasher.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    var buffer = new byte[bytes.Length];
                    var size = ss.Read(buffer, 0, buffer.Length);
                    hasher.Clear();
                    var newBuffer = new byte[size];
                    Array.Copy(buffer, newBuffer, size);
                    result = newBuffer;
                }
            }

            return result;
        }
    }
}