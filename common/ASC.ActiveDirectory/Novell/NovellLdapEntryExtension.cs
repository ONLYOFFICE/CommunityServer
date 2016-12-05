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


using System.Globalization;
using Novell.Directory.Ldap;
using System;
using System.Text;

namespace ASC.ActiveDirectory.Novell
{
    public static class NovellLdapEntryExtension
    {
        public static object GetAttributeValue(this LdapEntry ldapEntry, string attributeName)
        {
            var attribute = ldapEntry.getAttribute(attributeName);
            if (attribute == null)
                return null;

            if (string.Equals(attributeName, Constants.ADSchemaAttributes.OBJECT_SID, StringComparison.OrdinalIgnoreCase))
            {
                if (attribute.ByteValue == null)
                {
                    return null;
                }

                var value = new byte[attribute.ByteValue.Length];

                Buffer.BlockCopy(attribute.ByteValue, 0, value, 0, attribute.ByteValue.Length);

                return DecodeSID(value);
            }

            return attribute.StringValue;
        }

        public static string[] GetAttributeArrayValue(this LdapEntry ldapEntry, string attributeName)
        {
            var attribute = ldapEntry.getAttribute(attributeName);
            return attribute == null ? null : attribute.StringValueArray;
        }

        private static string DecodeSID(byte[] sid)
        {
            var strSid = new StringBuilder("S-");

            // get version
            int revision = sid[0];
            strSid.Append(revision.ToString(CultureInfo.InvariantCulture));

            //next byte is the count of sub-authorities
            int countSubAuths = sid[1] & 0xFF;

            //get the authority
            long authority = 0;

            //String rid = "";
            for (int i = 2; i <= 7; i++)
            {
                authority |= ((long)sid[i]) << (8 * (5 - (i - 2)));
            }

            strSid.Append("-");
            strSid.Append(authority.ToString("X4"));

            //iterate all the sub-auths
            int offset = 8;
            const int size = 4; //4 bytes for each sub auth

            for (int j = 0; j < countSubAuths; j++)
            {
                long subAuthority = 0;
                for (int k = 0; k < size; k++)
                {
                    subAuthority |= (long)(sid[offset + k] & 0xFF) << (8 * k);
                }

                strSid.Append("-");
                strSid.Append(subAuthority);

                offset += size;
            }

            return strSid.ToString();
        }
    }
}
