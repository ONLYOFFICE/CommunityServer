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


#region usings

using System.Web;
using ASC.Api.Interfaces;

#endregion

namespace ASC.Api.Impl
{
    public class ApiAllowAllAuthorization : IApiAuthorization
    {
        #region IApiAuthorization Members

        public bool Authorize(HttpContextBase context)
        {
            return true;
        }

        public bool OnAuthorizationFailed(HttpContextBase context)
        {
            return false;
        }

        #endregion
    }
}