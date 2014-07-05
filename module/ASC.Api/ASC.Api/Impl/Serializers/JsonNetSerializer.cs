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
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
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

        public bool CanRespondTo(IApiStandartResponce responce, string path, string contentType)
        {
            return IsJsonRequest(path, contentType) || IsXmlRequest(path, contentType);
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
            if (IsJsonRequest(path, contentType))
            {
                //Just write
                type = new ContentType(Constants.JsonContentType) {CharSet = "UTF-8"};
            }
            else
            {
                type = new ContentType(Constants.XmlContentType) {CharSet = "UTF-8"};
                responseJson =
                    JsonConvert.DeserializeXNode(responseJson, "result").ToString(prettify
                                                                                      ? SaveOptions.None
                                                                                      : SaveOptions.DisableFormatting);
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

        #endregion

        private static bool IsJsonRequest(string request, string contentType)
        {
            return StringUtils.GetExtension(request) == ".json" ||
                   StringUtils.IsContentType(Constants.JsonContentType, contentType);
        }

        private static bool IsXmlRequest(string request, string contentType)
        {
            return !IsJsonRequest(request, contentType);
        }
    }
}