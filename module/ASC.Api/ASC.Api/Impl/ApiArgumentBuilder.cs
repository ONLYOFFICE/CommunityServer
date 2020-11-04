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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using System.Xml.Linq;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using Newtonsoft.Json;
using Binder = ASC.Api.Utils.Binder;

namespace ASC.Api.Impl
{
    public class ApiArgumentBuilder : IApiArgumentBuilder
    {
        public IEnumerable<object> BuildCallingArguments(RequestContext context, IApiMethodCall methodToCall)
        {
            var callArg = new List<object>();
            var requestParams = GetRequestParams(context);

            var methodParams = methodToCall.GetParams().Where(x => !x.IsRetval).OrderBy(x => x.Position);


            foreach (var parameterInfo in methodParams)
            {
                if (requestParams[parameterInfo.Name] != null)
                {
                    //convert
                    var values = requestParams.GetValues(parameterInfo.Name);
                    if (values != null && values.Any())
                    {
                        if (Binder.IsCollection(parameterInfo.ParameterType))
                        {
                            callArg.Add(Binder.Bind(parameterInfo.ParameterType, requestParams, parameterInfo.Name));
                            continue; //Go to next loop
                        }

                        try
                        {
                            callArg.Add(ConvertUtils.GetConverted(values.First(), parameterInfo)); //NOTE; Get first value!
                        }
                        catch (ApiArgumentMismatchException)
                        {
                            //Failed to convert. Try bind
                            callArg.Add(Binder.Bind(parameterInfo.ParameterType, requestParams, parameterInfo.Name));
                        }
                    }
                }
                else
                {
                    //try get request param first. It may be form\url-encoded
                    if (!"GET".Equals(context.HttpContext.Request.HttpMethod, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(context.HttpContext.Request[parameterInfo.Name]))
                    {
                        //Drop to
                        callArg.Add(ConvertUtils.GetConverted(context.HttpContext.Request[parameterInfo.Name], parameterInfo));
                    }
                    else if (parameterInfo.ParameterType == typeof(ContentType) && !string.IsNullOrEmpty(context.HttpContext.Request.ContentType))
                    {
                        callArg.Add(new ContentType(context.HttpContext.Request.ContentType));
                    }
                    else if (parameterInfo.ParameterType == typeof(ContentDisposition) && !string.IsNullOrEmpty(context.HttpContext.Request.Headers["Content-Disposition"]))
                    {
                        var disposition = new ContentDisposition(context.HttpContext.Request.Headers["Content-Disposition"]);
                        disposition.FileName = HttpUtility.UrlDecode(disposition.FileName); //Decode uri name
                        callArg.Add(disposition);
                    }
                    else if (parameterInfo.ParameterType.IsSubclassOf(typeof(HttpPostedFile)) && context.HttpContext.Request.Files[parameterInfo.Name] != null)
                    {
                        callArg.Add(context.HttpContext.Request.Files[parameterInfo.Name]);
                    }
                    else if (Binder.IsCollection(parameterInfo.ParameterType) && parameterInfo.ParameterType.IsGenericType && parameterInfo.ParameterType.GetGenericArguments().First() == typeof(HttpPostedFileBase))
                    {
                        //File catcher
                        var files = new List<HttpPostedFileBase>(context.HttpContext.Request.Files.Count);
                        files.AddRange(from string key in context.HttpContext.Request.Files select context.HttpContext.Request.Files[key]);
                        callArg.Add(files);
                    }
                    else
                    {
                        if (parameterInfo.ParameterType.IsSubclassOf(typeof(Stream)) || parameterInfo.ParameterType == typeof(Stream))
                        {
                            //First try get files
                            var file = context.HttpContext.Request.Files[parameterInfo.Name];
                            callArg.Add(file != null ? file.InputStream : context.HttpContext.Request.InputStream);
                        }
                        else
                        {
                            //Try bind
                            //Note: binding moved here
                            if (IsTypeBindable(parameterInfo.ParameterType))
                            {
                                //Custom type
                                var binded = Binder.Bind(parameterInfo.ParameterType,
                                                         requestParams,
                                                         parameterInfo.Name);

                                if (binded != null)
                                {
                                    callArg.Add(binded);
                                    continue; //Go to next loop
                                }
                            }
                            //Create null
                            var obj = parameterInfo.ParameterType.IsValueType ? Activator.CreateInstance(parameterInfo.ParameterType) : null;
                            callArg.Add(obj);
                        }


                    }
                }
            }
            return callArg;
        }

        private static bool IsTypeBindable(Type parameterInfo)
        {
            if (Binder.IsCollection(parameterInfo))
            {
                return true;
            }
            return parameterInfo.Namespace != null && !parameterInfo.Namespace.StartsWith("System", StringComparison.Ordinal);
        }

        private static NameValueCollection GetRequestParams(RequestContext context)
        {
            var collection = context.RouteData.Values.ToNameValueCollection();
            var request = context.HttpContext.Request;

            if (request == null) return collection;
            collection.Add(request.QueryString);

            var contentType = string.IsNullOrEmpty(request.ContentType) ? new ContentType("text/plain") : new ContentType(request.ContentType);

            switch (contentType.MediaType)
            {
                case Constants.XmlContentType:
                    {
                        using (var stream = request.InputStream)
                        {
                            if (stream != null)
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    var root = XDocument.Load(reader).Root;
                                    if (root != null)
                                    {
                                        FillCollectionFromXElement(root.Elements(), string.Empty, collection);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case Constants.JsonContentType:
                    {
                        using (var stream = request.InputStream)
                        {
                            if (stream != null)
                            {
                                using (var reader = new StreamReader(request.InputStream))
                                {
                                    var value = reader.ReadToEnd();
                                    XDocument xdoc;
                                    try
                                    {
                                        xdoc = JsonConvert.DeserializeXNode(value, "request", false);
                                    }
                                    catch (Exception e)
                                    {
                                        throw new TargetInvocationException(new ArgumentException("Unable to deserialize json", e));
                                    }

                                    XElement root;
                                    if (xdoc != null && (root = xdoc.Root) != null)
                                    {
                                        FillCollectionFromXElement(root.Elements(), string.Empty, collection);
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    if (!"GET".Equals(request.HttpMethod, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var form = request.Form;
                        if (form != null)
                        {
                            collection.Add(request.Form);
                        }
                    }
                    break;
            }
            return collection;
        }

        private static void FillCollectionFromXElement(IEnumerable<XElement> elements, string prefix, NameValueCollection collection)
        {
            foreach (var grouping in elements.GroupBy(x => x.Name))
            {
                if (grouping.Count() < 2)
                {
                    //Single element
                    var element = grouping.SingleOrDefault();
                    if (element != null)
                    {
                        if (!element.HasElements)
                        {
                            //Last one in tree
                            AddElement(prefix, collection, element);
                        }
                        else
                        {
                            FillCollectionFromXElement(element.Elements(), prefix + "." + element.Name.LocalName, collection);
                        }
                    }

                }
                else
                {
                    //Grouping has more than one
                    if (grouping.All(x => !x.HasElements))
                    {
                        //Simple collection
                        foreach (var element in grouping)
                        {
                            AddElement(prefix, collection, element);
                        }
                    }
                    else
                    {
                        var groupList = grouping.ToList();
                        for (var i = 0; i < groupList.Count; i++)
                        {
                            FillCollectionFromXElement(groupList[i].Elements(), prefix + "." + grouping.Key + "[" + i + "]", collection);
                        }
                    }
                }
            }
        }

        private static void AddElement(string prefix, NameValueCollection collection, XElement element)
        {
            if (string.IsNullOrEmpty(prefix))
                collection.Add(element.Name.LocalName, element.Value);
            else
            {
                var prefixes = prefix.TrimStart('.').Split('.');
                var additional = string.Empty;
                if (prefixes.Length > 1)
                {
                    additional = string.Join("", prefix.Skip(1).Select(x => "[" + x + "]").ToArray());
                }
                collection.Add(prefixes[0] + additional + "[" + element.Name.LocalName + "]", element.Value);
            }
        }
    }
}