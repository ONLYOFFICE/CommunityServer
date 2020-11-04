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
using ASC.Web.Studio;
using ASC.Web.Studio.Utility;
using ASC.Web.Calendar.UserControls;

namespace ASC.Web.Calendar
{
    public partial class Default : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Title = HeaderStringHelper.GetPageTitle(Resources.CalendarAddonResource.AddonName);

            CreateButtonContent.Controls.Add(LoadControl(ButtonSidePanel.Location));
            SidePanelContent.Controls.Add(LoadControl(SidePanel.Location));
            SidePanelContent.Controls.Add(LoadControl(CalendarSidePanel.Location));
            CalendarPageContent.Controls.Add(LoadControl(CalendarControl.Location));
        }
    }
}