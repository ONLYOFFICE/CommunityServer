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
using System.Web.UI;

using ASC.Core;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common.Support
{
    public partial class Support : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/Support/Support.ascx"; }
        }

        protected string SupportFeedbackLink { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.Configuration.Personal || CoreContext.Configuration.CustomMode)
                return;

            SupportFeedbackLink = CommonLinkUtility.GetFeedbackAndSupportLink();

            var quota = TenantExtra.GetTenantQuota();
            var isAdministrator = CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID);
            var showDemonstration = !CoreContext.Configuration.Standalone && quota.Trial;
            var showTrainig = !quota.Free;

            if (showTrainig)
            {
                LiveChat = !string.IsNullOrEmpty(SetupInfo.ZendeskKey);
                EmailSupport = !string.IsNullOrEmpty(SupportFeedbackLink);
                RequestTraining = isAdministrator && !quota.Trial;
            }

            ProductDemo = !string.IsNullOrEmpty(SetupInfo.DemoOrder) && isAdministrator && showDemonstration;

            BaseCondition = AdditionalWhiteLabelSettings.Instance.FeedbackAndSupportEnabled && (LiveChat || EmailSupport || RequestTraining || ProductDemo);

        }

        protected bool LiveChat;

        protected bool EmailSupport;

        protected bool RequestTraining;

        protected bool ProductDemo;

        protected bool BaseCondition;

    }
}