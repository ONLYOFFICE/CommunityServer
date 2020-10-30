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
using System.Globalization;
using System.Web;
using ASC.Core;
using ASC.VoipService;
using ASC.VoipService.Dao;
using ASC.Web.Core;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Studio
{
    public partial class VoipClient : MainPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Master.DisabledSidePanel = true;
            Master.DisabledTopStudioPanel = true;

            if (VoipNumberData.CanMakeOrReceiveCall)
            {
                PhoneControl.Controls.Add(LoadControl(VoipPhoneControl.Location));
            }
        }
    }

    public class VoipNumberData : ClientScript
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Resources.Master"; }
        }

        protected override string GetCacheHash()
        {
            return SecurityContext.CurrentAccount.ID + CurrentNumber.Number +
                   (SecurityContext.IsAuthenticated && !CoreContext.Configuration.Personal
                        ? (CoreContext.UserManager.GetMaxUsersLastModified().Ticks.ToString(CultureInfo.InvariantCulture) +
                           CoreContext.UserManager.GetMaxGroupsLastModified().Ticks.ToString(CultureInfo.InvariantCulture))
                        : string.Empty);
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterObject(new { numberId = CurrentNumber.Number });
        }

        public static VoipPhone CurrentNumber
        {
            get { return new CachedVoipDao(CoreContext.TenantManager.GetCurrentTenant().TenantId).GetCurrentNumber(); }
        }

        public static bool CanMakeOrReceiveCall
        {
            get { return Allowed && CurrentNumber != null; }
        }

        public static bool Allowed
        {
            get
            {
                return SetupInfo.VoipEnabled &&
                       VoipDao.ConfigSettingsExist &&
                       !WebItemManager.Instance[WebItemManager.CRMProductID].IsDisabled();
            }
        }
    }
}