/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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