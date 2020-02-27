/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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