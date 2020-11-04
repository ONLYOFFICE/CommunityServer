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
using ASC.Core;
using ASC.CRM.Core;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.CRM.Configuration;
using ASC.Web.Studio;

namespace ASC.Web.CRM.Controls.Settings
{
    public partial class VoipCommon : BaseUserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/VoIPSettings/VoipCommon.ascx"); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!VoipNumberData.Allowed || !CRMSecurity.IsAdmin)
            {
                Response.Redirect(PathProvider.StartURL() + "Settings.aspx");
            }

            Page.RegisterBodyScripts("~/js/uploader/jquery.fileupload.js");
            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath("voip.common.js"));
            Page.RegisterClientScript(new VoipCommonData());
        }
    }

    public class VoipCommonData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Resources.Master.Voip"; }
        }

        protected override string GetCacheHash()
        {
            return CoreContext.TenantManager.GetCurrentTenant().TenantId + VoipNumberData.Allowed.ToString();
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterObject(new { enabled = VoipNumberData.Allowed});
        }
    }
}