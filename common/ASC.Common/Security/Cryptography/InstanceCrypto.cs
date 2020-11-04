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
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace ASC.Security.Cryptography
{
    public static class InstanceCrypto
    {
        public static string Encrypt(string data)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data)));
        }

        public static byte[] Encrypt(byte[] data)
        {
            Rijndael hasher = Rijndael.Create();
            hasher.Key = EKey();
            hasher.IV = new byte[hasher.BlockSize >> 3];
            using (var ms = new MemoryStream())
            using (var ss = new CryptoStream(ms, hasher.CreateEncryptor(), CryptoStreamMode.Write))
            {
                ss.Write(data, 0, data.Length);
                ss.FlushFinalBlock();
                hasher.Clear();
                return ms.ToArray();
            }
        }

        public static string Decrypt(string data)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(data)));
        }

        public static byte[] Decrypt(byte[] data)
        {
            Rijndael hasher = Rijndael.Create();
            hasher.Key = EKey();
            hasher.IV = new byte[hasher.BlockSize >> 3];

            using (var ms = new MemoryStream(data))
            using (var ss = new CryptoStream(ms, hasher.CreateDecryptor(), CryptoStreamMode.Read))
            {
                var buffer = new byte[data.Length];
                int size = ss.Read(buffer, 0, buffer.Length);
                hasher.Clear();
                var newBuffer = new byte[size];
                Array.Copy(buffer, newBuffer, size);
                return newBuffer;
            }
        }

        private static byte[] EKey()
        {
            return MachinePseudoKeys.GetMachineConstant(32);
        }
    }
}