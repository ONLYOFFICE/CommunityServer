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
using System.IO;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Xml.Linq;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using Newtonsoft.Json;

namespace ASC.Api.Impl.Serializers
{
    public class JsonNetSerializer : IApiSerializer
    {

        public JsonNetSerializer()
        {
            
        }

        #region IApiSerializer Members

        public IEnumerable<string> GetSupportedExtensions()
        {
            return new[] {".json", ".xml",".tml"};
        }

        public bool CanSerializeType(Type type)
        {
            return true;
        }

        public ContentType RespondTo(IApiStandartResponce responce, TextWriter output, string path, string contentType,
                                     bool prettify, bool async)
        {
            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new SerializerContractResolver(responce.Response,responce.ApiContext),
                Converters = new[] { new JsonStringConverter() }
            };

            string responseJson = JsonConvert.SerializeObject(responce, prettify ? Formatting.Indented : Formatting.None,settings);
            if (string.IsNullOrEmpty(responseJson))
                throw new InvalidOperationException("Failed to serialize object");

            ContentType type;
            if (IsXmlRequest(path, contentType))
            {
                settings.DateParseHandling = DateParseHandling.None;

                type = new ContentType(Constants.XmlContentType) { CharSet = "UTF-8" };
                responseJson = JsonConvert.DeserializeObject<XDocument>("{\"result\":" + responseJson + "}", settings).ToString(prettify
                                                                                      ? SaveOptions.None
                                                                                      : SaveOptions.DisableFormatting);
            }
            else
            {
                //Just write
                type = new ContentType(Constants.JsonContentType) { CharSet = "UTF-8" };
            }

            try
            {
                output.Write(responseJson);
            }
            catch (Exception e)
            {
                throw new SerializationException("Failed to write:"+responseJson,e);
            }
            return type;
        }


        private static bool IsXmlRequest(string request, string contentType)
        {
            var ext = StringUtils.GetExtension(request);
            return ( ext == ".tml" || ext == ".xml") &&
                !StringUtils.IsContentType(Constants.JsonContentType, contentType);
        }

        #endregion
    }
}