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
using System.Configuration;
using System.Runtime.Serialization;

using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Notify.Jabber;
using ASC.Mail.Core;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Services.DocumentService;

namespace ASC.Api.Settings
{
    [DataContract(Name = "buildversion", Namespace = "")]
    public class BuildVersion
    {
        [DataMember]
        public string CommunityServer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DocumentServer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MailServer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string XmppServer { get; set; }

        public static BuildVersion GetCurrentBuildVersion()
        {
            return new BuildVersion
            {
                CommunityServer = GetCommunityVersion(),
                DocumentServer = GetDocumentVersion(),
                MailServer = GetMailServerVersion(),
                XmppServer = GetXmppServerVersion()
            };
        }

        private static string GetCommunityVersion()
        {
            return ConfigurationManagerExtension.AppSettings["version.number"] ?? "8.5.0";
        }

        private static string GetDocumentVersion()
        {
            if (string.IsNullOrEmpty(FilesLinkUtility.DocServiceApiUrl))
                return null;

            return DocumentServiceConnector.GetVersion();
        }

        private static string GetMailServerVersion()
        {
            try
            {
                var engineFactory = new EngineFactory(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    SecurityContext.CurrentAccount.ID.ToString());

                var version = engineFactory.ServerEngine.GetServerVersion();
                return version;
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Warn(e.Message, e);
            }

            return null;
        }

        private static string GetXmppServerVersion()
        {
            try
            {
                if (ConfigurationManagerExtension.AppSettings["web.talk"] != "true")
                    return null;

                return new JabberServiceClient().GetVersion();
            }
            catch (Exception e)
            {
                LogManager.GetLogger("ASC").Warn(e.Message, e);
            }

            return null;
        }
    }
}
