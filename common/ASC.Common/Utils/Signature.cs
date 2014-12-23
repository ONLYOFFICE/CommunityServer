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

using ASC.Security.Cryptography;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace ASC.Common.Utils
{
    public static class Signature
    {
        public static string Create<T>(T obj)
        {
            return Create(obj, Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant()));
        }

        public static string Create<T>(T obj, string secret)
        {
            var serializer = new JavaScriptSerializer();
            var str = serializer.Serialize(obj);
            var payload = GetHashBase64(str + secret) + "?" + str;
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(payload));
        }

        public static T Read<T>(string signature)
        {
            return Read<T>(signature, Encoding.UTF8.GetString(MachinePseudoKeys.GetMachineConstant()));
        }

        public static T Read<T>(string signature, string secret)
        {
            return Read<T>(signature, secret, true);
        }

        public static T Read<T>(string signature, string secret, bool useSecret)
        {
            try
            {
                var payloadParts = Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(signature)).Split('?');
                if (!useSecret || GetHashBase64(payloadParts[1] + secret) == payloadParts[0]
                    || GetHashBase64MD5(payloadParts[1] + secret) == payloadParts[0] //todo: delete
                    )
                {
                    //Sig correct
                    return new JavaScriptSerializer().Deserialize<T>(payloadParts[1]);
                }
            }
            catch (Exception)
            {
            }
            return default(T);
        }

        private static string GetHashBase64(string str)
        {
            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(str)));
        }

        private static string GetHashBase64MD5(string str)
        {
            return Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str)));
        }
    }
}