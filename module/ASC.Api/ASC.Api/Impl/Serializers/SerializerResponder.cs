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
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Routing;
using ASC.Api.Interfaces;
using Microsoft.Practices.Unity;

namespace ASC.Api.Impl.Serializers
{
    public class SerializerResponder : IApiResponder
    {
        private readonly ICollection<IApiSerializer> _serializers;

        public SerializerResponder(IUnityContainer container)
        {
            var serializers = container.ResolveAll<IApiSerializer>();
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