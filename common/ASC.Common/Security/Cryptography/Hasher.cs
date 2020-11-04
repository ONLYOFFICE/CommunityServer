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
using System.Security.Cryptography;
using System.Text;

namespace ASC.Security.Cryptography
{
    public sealed class Hasher
    {
        private const HashAlg DefaultAlg = HashAlg.SHA256;


        public static byte[] Hash(string data, HashAlg hashAlg)
        {
            return ComputeHash(data, hashAlg);
        }

        public static byte[] Hash(string data)
        {
            return Hash(data, DefaultAlg);
        }

        public static byte[] Hash(byte[] data, HashAlg hashAlg)
        {
            return ComputeHash(data, hashAlg);
        }

        public static byte[] Hash(byte[] data)
        {
            return Hash(data, DefaultAlg);
        }

        public static string Base64Hash(string data, HashAlg hashAlg)
        {
            return ComputeHash64(data, hashAlg);
        }

        public static string Base64Hash(string data)
        {
            return Base64Hash(data, DefaultAlg);
        }

        public static string Base64Hash(byte[] data, HashAlg hashAlg)
        {
            return ComputeHash64(data, hashAlg);
        }

        public static string Base64Hash(byte[] data)
        {
            return Base64Hash(data, DefaultAlg);
        }

        public static bool EqualHash(byte[] dataToCompare, byte[] hash, HashAlg hashAlg)
        {
            return String.Equals(
                ComputeHash64(dataToCompare, hashAlg),
                B2S64(hash)
                );
        }

        public static bool EqualHash(byte[] dataToCompare, byte[] hash)
        {
            return EqualHash(dataToCompare, hash, DefaultAlg);
        }

        public static bool EqualHash(string dataToCompare, string hash, HashAlg hashAlg)
        {
            return EqualHash(S2B(dataToCompare), S642B(hash), hashAlg);
        }

        public static bool EqualHash(string dataToCompare, string hash)
        {
            return EqualHash(dataToCompare, hash, DefaultAlg);
        }


        private static HashAlgorithm GetAlg(HashAlg hashAlg)
        {
            switch (hashAlg)
            {
                case HashAlg.MD5:
                    return MD5.Create();
                case HashAlg.SHA1:
                    return SHA1.Create();
                case HashAlg.SHA256:
                    return SHA256.Create();
                case HashAlg.SHA512:
                    return SHA512.Create();
                default:
                    return SHA256.Create();
            }
        }

        private static byte[] S2B(string str)
        {
            if (str == null) throw new ArgumentNullException("str");
            return Encoding.UTF8.GetBytes(str);
        }

        private static string B2S(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return Encoding.UTF8.GetString(data);
        }

        private static byte[] S642B(string str)
        {
            if (str == null) throw new ArgumentNullException("str");
            return Convert.FromBase64String(str);
        }

        private static string B2S64(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            return Convert.ToBase64String(data);
        }

        private static byte[] ComputeHash(byte[] data, HashAlg hashAlg)
        {
            return GetAlg(hashAlg).ComputeHash(data);
        }

        private static byte[] ComputeHash(string data, HashAlg hashAlg)
        {
            return ComputeHash(S2B(data), hashAlg);
        }

        private static string ComputeHash64(byte[] data, HashAlg hashAlg)
        {
            return B2S64(ComputeHash(data, hashAlg));
        }

        private static string ComputeHash64(string data, HashAlg hashAlg)
        {
            return ComputeHash64(S2B(data), hashAlg);
        }
    }
}