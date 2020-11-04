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