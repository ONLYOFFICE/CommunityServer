/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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