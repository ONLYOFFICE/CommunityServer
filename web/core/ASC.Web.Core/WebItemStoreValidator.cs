/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Core;
using ASC.Data.Storage;

namespace ASC.Web.Core
{
    public class WebItemStoreValidator : IDataStoreValidator
    {
        internal Guid WebItemID;

        public WebItemStoreValidator(string webItemID)
        {
            if (!Guid.TryParse(webItemID, out WebItemID))
            {
                throw new ArgumentException();
            }
        }

        public virtual bool Validate(string path)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                return false;
            }

            var product = WebItemManager.Instance[WebItemID];

            return product != null && !product.IsDisabled();
        }
    }

    public class WebItemStoreAdminValidator : WebItemStoreValidator
    {
        public WebItemStoreAdminValidator(string webItemID) : base(webItemID) { }

        public override bool Validate(string path)
        {
            return base.Validate(path) && WebItemSecurity.IsProductAdministrator(WebItemID, SecurityContext.CurrentAccount.ID);
        }
    }
}