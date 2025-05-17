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
using System.Web;

using ASC.Common.Radicale;
using ASC.Common.Radicale.Core;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.Masters.MasterManagement;

namespace ASC.Web.People
{
    public partial class CardDavSettings : MainPage
    {
        protected bool IsDisabled { get; set; }
        protected bool IsEnabledLink { get; set; }

        protected string CardDavLink { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {

            IsDisabled = WebItemManager.Instance[WebItemManager.PeopleProductID].IsDisabled();
            if (IsDisabled) return;

            RenderScripts();
            var dao = new DbRadicale();

            IsEnabledLink = dao.IsExistCardDavUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, SecurityContext.CurrentAccount.ID.ToString());

            if (IsEnabledLink)
            {
                var myUri = (HttpContext.Current != null) ? HttpContext.Current.Request.GetUrlRewriter().ToString() : "http://localhost";
                var cardDavAddBook = new CardDavAddressbook();
                CardDavLink = cardDavAddBook.GetRadicaleUrl(myUri, CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email, true, true, true);
            }


        }

        protected void RenderScripts()
        {
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/Products/People/App_Themes/dark/dark-carddav.less");
            }
            else
            {
                Page.RegisterStyle("~/Products/People/App_Themes/default/css/carddav.less");
            }
            Page.RegisterBodyScripts("~/Products/People/js/carddav.js");
        }
    }
}