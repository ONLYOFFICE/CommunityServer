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
using System.Linq;
using System.Web;
using ASC.Api.Interfaces;
using Autofac;

namespace ASC.Api.Impl.Serializers
{
    public class SerializerResponder : IApiResponder
    {
        private readonly ICollection<IApiSerializer> _serializers;


        public SerializerResponder(IComponentContext container)
        {
            var serializers = container.Resolve<IEnumerable<IApiSerializer>>();
            if (serializers==null)
                throw new ArgumentException("No serializers resolved");

            _serializers = new List<IApiSerializer>(serializers);
            if (!_serializers.Any())
                throw new ArgumentException("No serializers defined");
        }

        public string Name
        {
            get { return "serializer"; }
        }

        public IEnumerable<string> GetSupportedExtensions()
        {
            return _serializers.SelectMany(x => x.GetSupportedExtensions());
        }

        public bool CanSerializeType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return _serializers.Any(x => x.CanSerializeType(type));
        }

        public bool CanRespondTo(IApiStandartResponce responce, HttpContextBase context)
        {
            if (responce == null) throw new ArgumentNullException("responce");
            if (context == null) throw new ArgumentNullException("context");
            return true;
        }

        public void RespondTo(IApiStandartResponce responce, HttpContextBase httpContext)
        {
            if (responce == null) throw new ArgumentNullException("responce");
            if (httpContext == null) throw new ArgumentNullException("httpContext");

            foreach (var apiSerializer in _serializers)
            {
                var contentType = apiSerializer.RespondTo(responce, httpContext.Response.Output,
                                                                  httpContext.Request.Path, httpContext.Request.ContentType, false, false);
                if (contentType != null)
                {
                    httpContext.Response.ContentType = contentType.ToString();
#if (DEBUG)
                    httpContext.Response.AddHeader("X-Responded", string.Format("{0}", contentType));
#endif
                    return;
                }
            }
#if (DEBUG)
            httpContext.Response.AddHeader("X-Responded", "No");
#endif

        }
    }
}