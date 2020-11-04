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

namespace ASC.Api.Impl.Responders
{
    public class DirectResponder : IApiResponder
    {
        #region IApiResponder Members

        public string Name
        {
            get { return "direct"; }
        }

        public IEnumerable<string> GetSupportedExtensions()
        {
            return new string[0];
        }

        public bool CanSerializeType(Type type)
        {
            return false;
        }


        public bool CanRespondTo(IApiStandartResponce responce, HttpContextBase context)
        {
            return responce.Response is IApiDirectResponce;
        }

        public void RespondTo(IApiStandartResponce responce, HttpContextBase context)
        {
            ((IApiDirectResponce) responce.Response).WriteResponce(context.Response);
        }

        #endregion
    }
}