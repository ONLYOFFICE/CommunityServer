/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Web.UI;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using System.Web;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("NamingPeopleContentController")]
    public partial class NamingPeopleSettingsContent : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Management/NamingPeopleSettings/NamingPeopleSettingsContent.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/namingpeoplesettings/js/namingpeoplecontent.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/namingpeoplesettings/css/namingpeople.less"));

            var schemas = new List<object>();
            var currentSchemaId = CustomNamingPeople.Current.Id;

            foreach (var schema in CustomNamingPeople.GetSchemas())
            {
                schemas.Add(new
                    {
                        Id = schema.Key,
                        Name = schema.Value,
                        Current = string.Equals(schema.Key, currentSchemaId, StringComparison.InvariantCultureIgnoreCase)
                    });
            }

            namingSchemaRepeater.DataSource = schemas;
            namingSchemaRepeater.DataBind();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object GetPeopleNames(string schemaId)
        {
            var names = CustomNamingPeople.GetPeopleNames(schemaId);

            return new
                {
                    names.Id,
                    names.UserCaption,
                    names.UsersCaption,
                    names.GroupCaption,
                    names.GroupsCaption,
                    names.UserPostCaption,
                    names.RegDateCaption,
                    names.GroupHeadCaption,
                    names.GuestCaption,
                    names.GuestsCaption,
                };
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveNamingSettings(string schemaId)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                CustomNamingPeople.SetPeopleNames(schemaId);

                CoreContext.TenantManager.SaveTenant(CoreContext.TenantManager.GetCurrentTenant());

                MessageService.Send(HttpContext.Current.Request, MessageAction.TeamTemplateChanged);

                return new {Status = 1, Message = Resource.SuccessfullySaveSettingsMessage};
            }
            catch(Exception e)
            {
                return new {Status = 0, e.Message};
            }
        }


        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveCustomNamingSettings(string usrCaption, string usrsCaption, string grpCaption, string grpsCaption,
                                               string usrStatusCaption, string regDateCaption,
                                               string grpHeadCaption,
                                               string guestCaption, string guestsCaption)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                usrCaption = (usrCaption ?? "").Trim();
                usrsCaption = (usrsCaption ?? "").Trim();
                grpCaption = (grpCaption ?? "").Trim();
                grpsCaption = (grpsCaption ?? "").Trim();
                usrStatusCaption = (usrStatusCaption ?? "").Trim();
                regDateCaption = (regDateCaption ?? "").Trim();
                grpHeadCaption = (grpHeadCaption ?? "").Trim();
                guestCaption = (guestCaption ?? "").Trim();
                guestsCaption = (guestsCaption ?? "").Trim();

                if (String.IsNullOrEmpty(usrCaption)
                    || String.IsNullOrEmpty(usrsCaption)
                    || String.IsNullOrEmpty(grpCaption)
                    || String.IsNullOrEmpty(grpsCaption)
                    || String.IsNullOrEmpty(usrStatusCaption)
                    || String.IsNullOrEmpty(regDateCaption)
                    || String.IsNullOrEmpty(grpHeadCaption)
                    || String.IsNullOrEmpty(guestCaption)
                    || String.IsNullOrEmpty(guestsCaption))
                {
                    throw new Exception(Resource.ErrorEmptyFields);
                }

                var names = new PeopleNamesItem
                    {
                        Id = PeopleNamesItem.CustomID,
                        UserCaption = usrCaption.Substring(0, Math.Min(30, usrCaption.Length)),
                        UsersCaption = usrsCaption.Substring(0, Math.Min(30, usrsCaption.Length)),
                        GroupCaption = grpCaption.Substring(0, Math.Min(30, grpCaption.Length)),
                        GroupsCaption = grpsCaption.Substring(0, Math.Min(30, grpsCaption.Length)),
                        UserPostCaption = usrStatusCaption.Substring(0, Math.Min(30, usrStatusCaption.Length)),
                        RegDateCaption = regDateCaption.Substring(0, Math.Min(30, regDateCaption.Length)),
                        GroupHeadCaption = grpHeadCaption.Substring(0, Math.Min(30, grpHeadCaption.Length)),
                        GuestCaption = guestCaption.Substring(0, Math.Min(30, guestCaption.Length)),
                        GuestsCaption = guestsCaption.Substring(0, Math.Min(30, guestsCaption.Length)),
                    };

                CustomNamingPeople.SetPeopleNames(names);

                CoreContext.TenantManager.SaveTenant(CoreContext.TenantManager.GetCurrentTenant());

                MessageService.Send(HttpContext.Current.Request, MessageAction.TeamTemplateChanged);

                return new {Status = 1, Message = Resource.SuccessfullySaveSettingsMessage};
            }
            catch(Exception e)
            {
                return new {Status = 0, e.Message};
            }
        }
    }
}