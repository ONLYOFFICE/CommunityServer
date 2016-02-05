/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Web;
using System.Web.UI;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Core;
using ASC.Web.Studio.Core;
using Resources;

namespace ASC.Web.Studio.UserControls.Users
{
    [AjaxNamespace("AjaxPro.UserLangController")]
    public partial class UserLanguage : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Users/UserProfile/UserLanguage.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterStyle(ResolveUrl("~/usercontrols/users/userprofile/css/userlanguages.less"));
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/users/userprofile/js/userlanguage.js"));
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveUserLanguageSettings(string lng)
        {
            try
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var curLng = user.CultureName;
                
                var changelng = false;
                if (SetupInfo.EnabledCultures.Find(c => String.Equals(c.Name, lng, StringComparison.InvariantCultureIgnoreCase)) != null)
                {
                    if (curLng != lng)
                    {
                        user.CultureName = lng;
                        changelng = true;

                        try
                        {
                            CoreContext.UserManager.SaveUserInfo(user);
                        }
                        catch (Exception ex)
                        {
                            user.CultureName = curLng;
                            throw ex;
                        }

                        MessageService.Send(HttpContext.Current.Request, MessageAction.UserUpdatedLanguage);
                    }
                }
                
                return new {Status = changelng ? 1 : 2, Message = Resource.SuccessfullySaveSettingsMessage};
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }
}