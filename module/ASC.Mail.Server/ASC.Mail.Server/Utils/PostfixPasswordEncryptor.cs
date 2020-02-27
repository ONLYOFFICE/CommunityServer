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
