/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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