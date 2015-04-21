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