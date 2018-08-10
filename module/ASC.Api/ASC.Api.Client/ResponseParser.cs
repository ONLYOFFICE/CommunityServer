/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
