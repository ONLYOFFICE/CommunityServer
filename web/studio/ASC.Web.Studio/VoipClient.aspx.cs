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
                return SetupInfo.VoipEnabled == "true" &&
                       VoipDao.ConfigSettingsExist &&
                       !WebItemManager.Instance[WebItemManager.CRMProductID].IsDisabled();
            }
        }
    }
}