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
using System.Web;
using ASC.Api.Interfaces;
using ASC.Api.Interfaces.ResponseTypes;
using ASC.Api.Utils;

namespace ASC.Api.Impl.Responders
{
    public class ContentResponder : IApiResponder
    {
        #region IApiResponder Members

        public string Name
        {
            get { return "content"; }
        }

        public IEnumerable<string> GetSupportedExtensions()
        {
            return new string[0];
        }

        public bool CanSerializeType(Type type)
        {
            return false;
        }

        public string Serialize(IApiStandartResponce obj, ApiContext context)
        {
            throw new NotSupportedException();
        }

        public bool CanRespondTo(IApiStandartResponce responce, HttpContextBase context)
        {
            return responce.Response is IApiContentResponce;
        }

        public void RespondTo(IApiStandartResponce responce, HttpContextBase context)
        {
            var contentResponce = (IApiContentResponce) responce.Response;
            if (contentResponce.ContentDisposition != null)
            {
                context.Response.AddHeader("Content-Disposition", contentResponce.ContentDisposition.ToString());
            }
            if (contentResponce.ContentType != null)
            {
                context.Response.ContentType = contentResponce.ContentType.ToString();
            }
            if (contentResponce.ContentEncoding != null)
            {
                context.Response.ContentEncoding = contentResponce.ContentEncoding;
            }
            context.Response.WriteStreamToResponce(contentResponce.ContentStream);
        }

        #endregion
    }
}