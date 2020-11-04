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
using System.Threading;
using System.Web;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.WhiteLabel;

namespace ASC.Web.Core.Client.HttpHandlers
{
    public abstract class ClientScriptLocalization : ClientScript
    {
        protected override bool CheckAuth { get { return false; } }

        protected internal sealed override string GetCacheHash()
        {
            var result = ClientSettings.ResetCacheKey + Thread.CurrentThread.CurrentCulture.Name;

            try
            {
                var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
                var whiteLabelSettings = TenantWhiteLabelSettings.Load();

                if (!string.IsNullOrEmpty(whiteLabelSettings.LogoText))
                {
                    result += tenantId.ToString() + whiteLabelSettings.LogoText;
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Error(e);
            }

            return result;
        }
    }

    public abstract class ClientScriptTemplate : ClientScript
    {
        protected override bool CheckAuth { get { return false; } }

        protected abstract string[] Links { get; }

        protected sealed override string BaseNamespace { get { return ""; } }

        protected sealed override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return RegisterClientTemplatesPath(context, Links);
        }

        protected internal sealed override string GetCacheHash()
        {
            return ClientSettings.ResetCacheKey;
        }

        public string[] GetLinks()
        {
            if (ClientSettings.BundlingEnabled)
            {
                return Links;
            }

            return new[] { new ClientScriptReference().AddScript(this).GetLink() };
        }
    }
}
