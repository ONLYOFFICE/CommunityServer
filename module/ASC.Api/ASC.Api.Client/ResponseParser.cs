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
