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

            Page.RegisterBodyScripts("~/UserControls/Management/NamingPeopleSettings/js/namingpeoplecontent.js")
                .RegisterStyle("~/UserControls/Management/NamingPeopleSettings/css/namingpeople.less");

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