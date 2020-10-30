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


#region Import

using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Reports;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using ASC.Web.Studio.Utility;

#endregion

namespace ASC.Web.CRM
{
    public partial class Reports : BasePage
    {
        protected override void PageLoad()
        {
            if (!Global.CanCreateReports)
                Response.Redirect(PathProvider.StartURL());

            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? CRMReportResource.Reports);

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));

            AdditionalContainerHolder.Controls.Add(LoadControl(ReportsNavigation.Location));

            CommonContainerHolder.Controls.Add(LoadControl(ReportsView.Location));
        }
    }
}