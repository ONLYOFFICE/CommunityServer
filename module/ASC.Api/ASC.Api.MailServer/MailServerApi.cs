/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Core;
using ASC.Web.Studio.Utility;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi : Interfaces.IApiEntryPoint
    {
        private EngineFactory _engineFactory;

        private EngineFactory MailEngineFactory
        {
            get
            {
                return _engineFactory ??
                       (_engineFactory =
                           new EngineFactory(
                               CoreContext.TenantManager.GetCurrentTenant().TenantId,
                               SecurityContext.CurrentAccount.ID.ToString()));
            }
        }

        ///<summary>
        /// Api name entry
        ///</summary>
        public string Name
        {
            get { return "mailserver"; }
        }

        public bool IsEnableMailServer
        {
            get
            {
                if (CoreContext.Configuration.Standalone)
                {
                    return true;
                }
                else
                {
                    return TenantExtra.GetTenantQuota().EnableMailServer;
                }
            }
        }

        private ILog _log;

        private ILog Logger
        {
            get { return _log ?? (_log = LogManager.GetLogger("ASC.Api")); }
        }

        private static int TenantId
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private static string Username
        {
            get { return SecurityContext.CurrentAccount.ID.ToString(); }
        }
    }
}
