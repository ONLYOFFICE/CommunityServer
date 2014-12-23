/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Text;
using System.Security.Cryptography;

namespace ASC.Mail.Server.PostfixAdministration
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
        private const int SaltLength = 8;
        private const string RequiredMd5SaltPrefix = "$1$";

        private string _stringToHash;
        private string _salt;
        private string _hash;


        public static SaltMd5 Init(string string_to_hash, string salt)
        {
            if (salt != null && !salt.StartsWith(RequiredMd5SaltPrefix))
                throw new ArgumentException("Invalid salt format. Should be $1$********");

            var hash = new SaltMd5 { _stringToHash = string_to_hash, _salt = salt };
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
            var salt_in_string = _salt == null ? GenerateRandomString() : _salt.Substring(RequiredMd5SaltPrefix.Length).Replace('$', ',');
            _hash = Unix_MD5Crypt.MD5Crypt.crypt(_stringToHash, salt_in_string);
            _isCalculated = true;
        }


        private static string GenerateRandomString()
        {
            const string chars = "23456789abcdefghjkmnpqrstuvwxyz23456789ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var t = new Random();
            var random_string = "";
            for (var i = 0; i < SaltLength; i++)
                random_string += chars[t.Next(0, chars.Length - 1)];

            return random_string;
        }
    }
}
