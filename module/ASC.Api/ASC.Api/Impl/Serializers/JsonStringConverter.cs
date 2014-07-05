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

using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace ASC.Api.Impl.Serializers
{
    public class JsonStringConverter : JsonConverter
    {
        private string GetEscapedJsonString(string str)
        {
            //THIS MAGIC FUNCTION IS REAPED FROM:
            //System.Runtime.Serialization.Json.XmlJsonWriter

            var writer = new StringWriter();
            var chars = str.ToCharArray();
            writer.Write('"');

            int num = 0;
            int index;
            for (index = 0; index < str.Length; ++index)
            {
                char ch = chars[index];
                if ((int)ch <= 47)
                {
                    if ((int)ch == 47 || (int)ch == 34)
                    {
                        writer.Write(chars, num, index - num);
                        writer.Write((char)92);
                        writer.Write(ch);
                        num = index + 1;
                    }
                    else if ((int)ch < 32)
                    {
                        writer.Write(chars, num, index - num);
                        writer.Write((char)92);
                        writer.Write((char)117);
                        writer.Write(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[1]
                                                                                               {
                                                                                                   (int) ch
                                                                                               }));
                        num = index + 1;
                    }
                }
                else if ((int)ch == 92)
                {
                    writer.Write(chars, num, index - num);
                    writer.Write((char)92);
                    writer.Write(ch);
                    num = index + 1;
                }
                else if ((int)ch >= 55296 && ((int)ch <= 57343 || (int)ch >= 65534))
                {
                    writer.Write(chars, num, index - num);
                    writer.Write((char)92);
                    writer.Write((char)117);
                    writer.Write(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[1]
                                                                                           {
                                                                                               (int) ch
                                                                                           }));
                    num = index + 1;
                }
            }
            if (num < index)
                writer.Write(chars, num, index - num);
            writer.Write('"');
            return writer.GetStringBuilder().ToString();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueString = value as string;
            if (valueString != null)
            {
                writer.WriteRawValue(GetEscapedJsonString((string)value));//Write raw here, so no aditional encoding done by Json.net
            }
            else
            {
                writer.WriteValue(value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Convert.ToString(existingValue);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}