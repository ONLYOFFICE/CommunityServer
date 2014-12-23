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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ASC.Mail.Aggregator.Common.Logging;
using JHSoftware;

namespace DnsChecker
{
    public class DnsChecker
    {
        #region .Constants

        private static readonly byte[] RsaOid = { 0x30, 0xD, 0x6, 0x9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0xD, 0x1, 0x1, 0x1, 0x5, 0x0 }; // Object ID for RSA

        // Corresponding ASN identification bytes
        const byte Integer = 0x2;
        const byte Sequence = 0x30;
        const byte BitString = 0x3;
        const byte OctetString = 0x4;

        #endregion

        #region .Public

        public static bool IsMxSettedUpCorrectForDomain(string domain_name, string mx_record, ILogger logger = null)
        {
            try
            {
                var domain_mx_records = DnsClient.LookupMX(domain_name);

                if (domain_mx_records == null)
                    throw new ArgumentException("MX record on your domain is missing.");

                return domain_mx_records.Any(re => re.HostName == mx_record);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Debug("DnsClient.LookupMX: domain: '{0}' mx: '{1}'\r\nException: {2}",
                                domain_name, mx_record, ex.ToString());

                return false;
            }
        }

        public static bool IsCnameSettedUpCorrectForDomain(string domain_name, string cname, string alias_name, ILogger logger = null)
        {
            try
            {
                var domain_alias = String.Format("{0}.{1}", cname, domain_name);

                var alias_host = DnsClient.LookupHost(domain_alias);
                var needed_host = DnsClient.LookupHost(alias_name);

                if (alias_host == null || needed_host == null)
                    throw new ArgumentException("Can't find host. Wait for dns updating.");

                return alias_host[0].Equals(needed_host[0]) && alias_host.Length == needed_host.Length;
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Debug("DnsClient.LookupHost: domain: '{0}' cname: '{1}' alias: '{2}'\r\nException: {3}",
                                domain_name, cname, alias_name, ex.ToString());
                return false;
            }
        }

        public static bool IsDkimSettedUpCorrectForDomain(string domain_name, string selector, string dkim_record, ILogger logger = null)
        {
            try
            {
                var domain_txt_records = DnsClient.Lookup(selector + "._domainkey."+ domain_name, DnsClient.RecordType.TXT);

                if (domain_txt_records == null)
                    throw new ArgumentException("DKIM record on your domain is missing.");

                return domain_txt_records.AnswerRecords.Any(re => re.Data.Trim('\"') == dkim_record);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Debug("DnsClient.Lookup: domain: '{0}' selector: '{1}._domainkey.' dkim_pk: '{2}'\r\nException: {3}",
                                domain_name, selector, dkim_record, ex.ToString());
                return false;
            }
        }

        public static bool IsTxtRecordCorrect(string domain_name, string txt_record, ILogger logger = null)
        {
            try
            {
                var domain_txt_records = DnsClient.Lookup(domain_name, DnsClient.RecordType.TXT);

                if (domain_txt_records == null)
                    throw new ArgumentException("TXT record on your domain is missing.");

                return domain_txt_records.AnswerRecords.Any(re => re.Data.Trim('\"') == txt_record);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Debug("DnsClient.LookupMX: domain: '{0}' txt: '{1}'\r\nException: {2}",
                                domain_name, txt_record, ex.ToString());
                return false;
            }
        }

        public static void GenerateKeys(out string private_key, out string public_key)
        {
            var rsa = new RSACryptoServiceProvider();
            var rsa_key_info = rsa.ExportParameters(true);
            public_key = "k=rsa; p=" + ConvertPublicKey(rsa_key_info);
            private_key = ConvertPrivateKey(rsa_key_info);
        }

        #endregion

        #region .Private

        private static string ConvertPublicKey(RSAParameters param)
        {
            var arr_binary_public_key = new List<byte>();

            arr_binary_public_key.InsertRange(0, param.Exponent);
            arr_binary_public_key.Insert(0, (byte)arr_binary_public_key.Count);
            arr_binary_public_key.Insert(0, Integer);

            arr_binary_public_key.InsertRange(0, param.Modulus);
            AppendLength(ref arr_binary_public_key, param.Modulus.Length);
            arr_binary_public_key.Insert(0, Integer);

            AppendLength(ref arr_binary_public_key, arr_binary_public_key.Count);
            arr_binary_public_key.Insert(0, Sequence);

            arr_binary_public_key.Insert(0, 0x0); // Add NULL value

            AppendLength(ref arr_binary_public_key, arr_binary_public_key.Count);

            arr_binary_public_key.Insert(0, BitString);
            arr_binary_public_key.InsertRange(0, RsaOid);

            AppendLength(ref arr_binary_public_key, arr_binary_public_key.Count);

            arr_binary_public_key.Insert(0, Sequence);

            return Convert.ToBase64String(arr_binary_public_key.ToArray());
        }

        private static string ConvertPrivateKey(RSAParameters param)
        {
            var arr_binary_private_key = new List<byte>();

            arr_binary_private_key.InsertRange(0, param.InverseQ);
            AppendLength(ref arr_binary_private_key, param.InverseQ.Length);
            arr_binary_private_key.Insert(0, Integer);

            arr_binary_private_key.InsertRange(0, param.DQ);
            AppendLength(ref arr_binary_private_key, param.DQ.Length);
            arr_binary_private_key.Insert(0, Integer);

            arr_binary_private_key.InsertRange(0, param.DP);
            AppendLength(ref arr_binary_private_key, param.DP.Length);
            arr_binary_private_key.Insert(0, Integer);

            arr_binary_private_key.InsertRange(0, param.Q);
            AppendLength(ref arr_binary_private_key, param.Q.Length);
            arr_binary_private_key.Insert(0, Integer);

            arr_binary_private_key.InsertRange(0, param.P);
            AppendLength(ref arr_binary_private_key, param.P.Length);
            arr_binary_private_key.Insert(0, Integer);

            arr_binary_private_key.InsertRange(0, param.D);
            AppendLength(ref arr_binary_private_key, param.D.Length);
            arr_binary_private_key.Insert(0, Integer);

            arr_binary_private_key.InsertRange(0, param.Exponent);
            AppendLength(ref arr_binary_private_key, param.Exponent.Length);
            arr_binary_private_key.Insert(0, Integer);

            arr_binary_private_key.InsertRange(0, param.Modulus);
            AppendLength(ref arr_binary_private_key, param.Modulus.Length);
            arr_binary_private_key.Insert(0, Integer);

            arr_binary_private_key.Insert(0, 0x00);
            AppendLength(ref arr_binary_private_key, 1);
            arr_binary_private_key.Insert(0, Integer);

            AppendLength(ref arr_binary_private_key, arr_binary_private_key.Count);
            arr_binary_private_key.Insert(0, Sequence);

            return Convert.ToBase64String(arr_binary_private_key.ToArray());
        }

        private static void AppendLength(ref List<byte> arr_binary_data, int len)
        {
            if (len <= byte.MaxValue)
            {
                arr_binary_data.Insert(0, Convert.ToByte(len));
                arr_binary_data.Insert(0, 0x81);  //This byte means that the length fits in one byte
            }
            else
            {
                arr_binary_data.Insert(0, Convert.ToByte(len % (byte.MaxValue + 1)));
                arr_binary_data.Insert(0, Convert.ToByte(len / (byte.MaxValue + 1)));
                arr_binary_data.Insert(0, 0x82);  //This byte means that the length fits in two byte
            }

        }

        #endregion
    }
}
