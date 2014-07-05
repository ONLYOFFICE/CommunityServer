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

using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace ASC.Api.Client
{
    internal static class ResponseParser
    {
        public static ApiResponse ParseXmlResponse(Stream stream)
        {
            using (var xmlReader = XmlReader.Create(stream))
            {
                var apiResponse = new ApiResponse();

                xmlReader.ReadToNextElement(); // Initial read
                xmlReader.ReadToNextElement(); // Move to first descendant

                while (!xmlReader.EOF)
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "statusCode":
                                apiResponse.StatusCode = xmlReader.ReadElementContentAsInt();
                                break;

                            case "count":
                                apiResponse.Count = xmlReader.ReadElementContentAsInt();
                                break;

                            case "response":
                                apiResponse.Response = xmlReader.ReadOuterXml();
                                break;

                            case "error":
                                throw ParseError(xmlReader);

                            default:
                                xmlReader.Skip();
                                break;
                        }
                    }
                    else
                    {
                        xmlReader.Read();
                    }
                }

                return apiResponse;
            }
        }

        private static ApiErrorException ParseError(XmlReader xmlReader)
        {
            var error = new ApiErrorException();

            xmlReader.ReadToNextElement();

            while (!xmlReader.EOF)
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {
                        case "message":
                            error.ErrorMessage = xmlReader.ReadElementContentAsString();
                            break;

                        case "stack":
                            error.ErrorStackTrace = xmlReader.ReadElementContentAsString();
                            break;

                        case "type":
                            error.ErrorType = xmlReader.ReadElementContentAsString();
                            break;

                        default:
                            xmlReader.Skip();
                            break;
                    }
                }
                else
                {
                    xmlReader.Read();
                }
            }

            return error;
        }

        public static ApiResponse ParseJsonResponse(Stream stream)
        {
            using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                var apiResponse = new ApiResponse();

                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.PropertyName)
                    {
                        switch ((string)jsonReader.Value)
                        {
                            case "statusCode":
                                apiResponse.StatusCode = jsonReader.ReadAsInt32() ?? 0;
                                break;

                            case "count":
                                apiResponse.Count = jsonReader.ReadAsInt32() ?? 0;
                                break;

                            case "response":
                                apiResponse.Response = jsonReader.GetInnerJson();
                                break;

                            case "error":
                                jsonReader.Read();
                                throw ParseError(jsonReader);

                            default:
                                jsonReader.Skip();
                                break;
                        }
                    }
                }

                return apiResponse;
            }
        }

        private static ApiErrorException ParseError(JsonReader jsonReader)
        {
            var error = new ApiErrorException();

            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.PropertyName)
                {
                    switch ((string)jsonReader.Value)
                    {
                        case "message":
                            error.ErrorMessage = jsonReader.ReadAsString();
                            break;

                        case "stack":
                            error.ErrorStackTrace = jsonReader.ReadAsString();
                            break;

                        case "type":
                            error.ErrorType = jsonReader.ReadAsString();
                            break;

                        default:
                            jsonReader.Skip();
                            break;
                    }
                }
            }

            return error;
        }
    }
}
