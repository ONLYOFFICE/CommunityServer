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
using ASC.Mail.Server.Exceptions;

namespace ASC.Mail.Server.Utils
{
    public enum HashType { Md5 = 1 }

    public class PostfixPasswordEncryptor
    {
        public static string EncryptString(HashType ht, string str, string salt = null)
        {
            switch (ht)
            {
                case HashType.Md5:
                    var md5 = SaltMd5.Init(str, salt);
                    return md5.ToBfFormat();
            }
            throw new PostfixEncryptorException("This hash type doesn't implemented.");
        }
    }

    internal class SaltMd5
    {
        private SaltMd5()
        {
        }

        private bool _isCalculated;
        private const int SALT_LENGTH = 8;
        private const string REQUIRED_MD5_SALT_PREFIX = "$1$";

        private string _stringToHash;
        private string _salt;
        private string _hash;


        public static SaltMd5 Init(string stringToHash, string salt)
        {
            if (salt != null && !salt.StartsWith(REQUIRED_MD5_SALT_PREFIX))
                throw new ArgumentException("Invalid salt format. Should be $1$********");

            var hash = new SaltMd5 { _stringToHash = stringToHash, _salt = salt };
            return hash;
        }

        //http://pentestmonkey.net/cheat-sheet/john-the-ripper-hash-formats
        public string ToBfFormat()
        {
            if (!_isCalculated) CalculateCryptMd5();
            return _hash;
        }

        private void CalculateCryptMd5()
        {
            var saltInString = _salt == null ? GenerateRandomString() : _salt.Substring(REQUIRED_MD5_SALT_PREFIX.Length).Replace('$', ',');
            _hash = Md5Crypt.crypt(_stringToHash, saltInString);
            _isCalculated = true;
        }


        private static string GenerateRandomString()
        {
            const string chars = "23456789abcdefghjkmnpqrstuvwxyz23456789ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var t = new Random();
            var randomString = "";
            for (var i = 0; i < SALT_LENGTH; i++)
                randomString += chars[t.Next(0, chars.Length - 1)];

            return randomString;
        }
    }
}
