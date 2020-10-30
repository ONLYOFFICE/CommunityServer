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
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ASC.Mail.Utils
{
    public static class CryptoUtil
    {
        private static readonly byte[] RsaOid = { 0x30, 0xD, 0x6, 0x9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0xD, 0x1, 0x1, 0x1, 0x5, 0x0 }; // Object ID for RSA

        private const byte INTEGER = 0x2;
        private const byte SEQUENCE = 0x30;
        private const byte BIT_STRING = 0x3;

        public static void GenerateDkimKeys(out string privateKey, out string publicKey)
        {
            var rsa = new RSACryptoServiceProvider();
            var rsaKeyInfo = rsa.ExportParameters(true);
            publicKey = "k=rsa; p=" + ConvertPublicKey(rsaKeyInfo);
            privateKey = ConvertPrivateKey(rsaKeyInfo);
        }

        private static string ConvertPublicKey(RSAParameters param)
        {
            var arrBinaryPublicKey = new List<byte>();

            arrBinaryPublicKey.InsertRange(0, param.Exponent);
            arrBinaryPublicKey.Insert(0, (byte)arrBinaryPublicKey.Count);
            arrBinaryPublicKey.Insert(0, INTEGER);

            arrBinaryPublicKey.InsertRange(0, param.Modulus);
            AppendLength(ref arrBinaryPublicKey, param.Modulus.Length);
            arrBinaryPublicKey.Insert(0, INTEGER);

            AppendLength(ref arrBinaryPublicKey, arrBinaryPublicKey.Count);
            arrBinaryPublicKey.Insert(0, SEQUENCE);

            arrBinaryPublicKey.Insert(0, 0x0); // Add NULL value

            AppendLength(ref arrBinaryPublicKey, arrBinaryPublicKey.Count);

            arrBinaryPublicKey.Insert(0, BIT_STRING);
            arrBinaryPublicKey.InsertRange(0, RsaOid);

            AppendLength(ref arrBinaryPublicKey, arrBinaryPublicKey.Count);

            arrBinaryPublicKey.Insert(0, SEQUENCE);

            return Convert.ToBase64String(arrBinaryPublicKey.ToArray());
        }

        private static string ConvertPrivateKey(RSAParameters param)
        {
            var arrBinaryPrivateKey = new List<byte>();

            arrBinaryPrivateKey.InsertRange(0, param.InverseQ);
            AppendLength(ref arrBinaryPrivateKey, param.InverseQ.Length);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            arrBinaryPrivateKey.InsertRange(0, param.DQ);
            AppendLength(ref arrBinaryPrivateKey, param.DQ.Length);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            arrBinaryPrivateKey.InsertRange(0, param.DP);
            AppendLength(ref arrBinaryPrivateKey, param.DP.Length);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            arrBinaryPrivateKey.InsertRange(0, param.Q);
            AppendLength(ref arrBinaryPrivateKey, param.Q.Length);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            arrBinaryPrivateKey.InsertRange(0, param.P);
            AppendLength(ref arrBinaryPrivateKey, param.P.Length);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            arrBinaryPrivateKey.InsertRange(0, param.D);
            AppendLength(ref arrBinaryPrivateKey, param.D.Length);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            arrBinaryPrivateKey.InsertRange(0, param.Exponent);
            AppendLength(ref arrBinaryPrivateKey, param.Exponent.Length);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            arrBinaryPrivateKey.InsertRange(0, param.Modulus);
            AppendLength(ref arrBinaryPrivateKey, param.Modulus.Length);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            arrBinaryPrivateKey.Insert(0, 0x00);
            AppendLength(ref arrBinaryPrivateKey, 1);
            arrBinaryPrivateKey.Insert(0, INTEGER);

            AppendLength(ref arrBinaryPrivateKey, arrBinaryPrivateKey.Count);
            arrBinaryPrivateKey.Insert(0, SEQUENCE);

            return Convert.ToBase64String(arrBinaryPrivateKey.ToArray());
        }

        private static void AppendLength(ref List<byte> arrBinaryData, int len)
        {
            if (len <= byte.MaxValue)
            {
                arrBinaryData.Insert(0, Convert.ToByte(len));
                arrBinaryData.Insert(0, 0x81);  //This byte means that the length fits in one byte
            }
            else
            {
                arrBinaryData.Insert(0, Convert.ToByte(len % (byte.MaxValue + 1)));
                arrBinaryData.Insert(0, Convert.ToByte(len / (byte.MaxValue + 1)));
                arrBinaryData.Insert(0, 0x82);  //This byte means that the length fits in two byte
            }

        }
    }
}
