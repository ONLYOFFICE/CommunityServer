/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ARSoft.Tools.Net.Dns;
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Server.DnsChecker
{
    public class DnsChecker
    {
        #region .Constants

        private static readonly byte[] RsaOid = { 0x30, 0xD, 0x6, 0x9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0xD, 0x1, 0x1, 0x1, 0x5, 0x0 }; // Object ID for RSA

        // Corresponding ASN identification bytes
        const byte INTEGER = 0x2;
        const byte SEQUENCE = 0x30;
        const byte BIT_STRING = 0x3;
#pragma warning disable 169
        const byte OCTET_STRING = 0x4;
#pragma warning restore 169

        #endregion

        #region .Public

        public static bool IsMxRecordCorrect(string domainName, string mxRecord, ILogger logger = null)
        {
            try
            {
                var records = DnsResolve<MxRecord>(domainName, RecordType.Mx);
                return records.Any(mx => mx.ExchangeDomainName == mxRecord);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Error("IsMxRecordCorrect: domain: '{0}' mx: '{1}'\r\nException: {2}",
                                domainName, mxRecord, ex.ToString());

                return false;
            }
        }

        public static bool IsDkimRecordCorrect(string domainName, string selector, string dkimRecord, ILogger logger = null)
        {
            var dkimRecordName = selector + "._domainkey." + domainName;

            try
            {
                var records = DnsResolve<TxtRecord>(dkimRecordName, RecordType.Txt);
                return records.Any(dkim => dkim.TextData.Trim('\"') == dkimRecord);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Error("IsDkimRecordCorrect: '{0}' dkim_pk: '{1}'\r\nException: {2}",
                                dkimRecordName, dkimRecord, ex.ToString());
                return false;
            }
        }

        public static bool IsTxtRecordCorrect(string domainName, string txtRecord, ILogger logger = null)
        {
            try
            {
                var records = DnsResolve<TxtRecord>(domainName, RecordType.Txt);
                return records.Any(mx => mx.TextData.Trim('\"') == txtRecord);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Error("IsTxtRecordCorrect: domain: '{0}' txt: '{1}'\r\nException: {2}",
                                domainName, txtRecord, ex.ToString());
                return false;
            }
        }

        public static void GenerateKeys(out string privateKey, out string publicKey)
        {
            var rsa = new RSACryptoServiceProvider();
            var rsaKeyInfo = rsa.ExportParameters(true);
            publicKey = "k=rsa; p=" + ConvertPublicKey(rsaKeyInfo);
            privateKey = ConvertPrivateKey(rsaKeyInfo);
        }

        #endregion

        #region .Private

        private static List<T> DnsResolve<T>(string domainName, RecordType type)
        {
            var dnsMessage = DnsClient.Default.Resolve(domainName, type);

            if ((dnsMessage == null) ||
                ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
            {
                throw new ArgumentException("DNS request failed");
            }

            return dnsMessage.AnswerRecords.Where(r => r.RecordType == type).Cast<T>().ToList();
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

        #endregion
    }
}
