/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.UI;
using AjaxPro;
using Resources;
using System;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Studio
{
    public partial class PreparationPortal : Page
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            InitInlineScript();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            PreparationPortalContent.Controls.Add(LoadControl(PreparationPortalCnt.Location));

            var type = Request["type"] ?? "";

            switch (type){ 
               //Migration
                case "0" :
                    Title = Resource.TransferPortalTitle;
                    break;
                //Backup
                case "1":
                    Title = Resource.RestoreTitle;
                    break;
                default:
                    Title = Resource.MainPageTitle;
                    break;
            }
        }
        private void InitInlineScript()
        {
            var scripts = HttpContext.Current.Items[Constant.AjaxID + ".pagescripts"] as ListDictionary;

            if (scripts == null) return;

            var sb = new StringBuilder();

            foreach (var key in scripts.Keys)
            {
                sb.Append(scripts[key]);
            }

            InlineScript.Scripts.Add(new Tuple<string, bool>(sb.ToString(), false));
        }
    }
}