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
using System.Web.UI;
using ASC.Web.Studio.Core.Backup;
using Resources;

namespace ASC.Web.Studio.UserControls.Common
{
    public partial class PreparationPortalCnt : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/PreparationPortal/PreparationPortal.ascx"; }
        }

        protected String HeaderPage { get; set; }
        protected String Text { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(BackupAjaxHandler), Page);

            var type = Request["type"] ?? "";

            switch (type)
            {
                //Migration
                case "0":
                    HeaderPage = String.Format(Resource.TransferPortalProcessTitle, "<br />");
                    Text = Resource.TransferPortalProcessText;
                    break;
                //Backup
                case "1":
                    HeaderPage = Resource.RestorePortalProcessTitle;
                    Text = Resource.RestorePortalProcessText;
                    break;
            }
        }
    }
}