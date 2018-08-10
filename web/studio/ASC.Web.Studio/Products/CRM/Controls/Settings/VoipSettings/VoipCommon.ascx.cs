/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
                Response.Redirect(PathProvider.StartURL() + "settings.aspx");
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