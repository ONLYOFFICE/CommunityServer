/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Web.CRM.Resources;
using ASC.CRM.Core;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Configuration;
using System.Text;
using ASC.Web.Studio.Controls.Users;
using ASC.Web.Studio.Core;
using ASC.Core.Users;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.CRM.Controls.Settings
{
    public partial class TaskTemplateView : BaseUserControl
    {
        #region Properties

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Settings/TaskTemplateView.ascx"); }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            _templateConatainerPanel.Options.IsPopup = true;
            _templatePanel.Options.IsPopup = true;

            var entityType = StringToEntityType(Request["view"]);

            RegisterClientScript(entityType);

            //var cntrlCategorySelector = (CategorySelector)LoadControl(CategorySelector.Location);
            //cntrlCategorySelector.Categories = Global.DaoFactory.ListItemDao.GetItems(ListType.TaskCategory);
            //cntrlCategorySelector.ID = "taskTemplateCategorySelector";
            //phCategorySelector.Controls.Add(cntrlCategorySelector);

            var emptyScreenControl = new EmptyScreenControl
                {
                    ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_screen_tasks.png", ProductEntryPoint.ID),
                    Header = CRMSettingResource.EmptyContentTaskTemplates,
                    Describe = CRMSettingResource.EmptyContentTaskTemplatesDescript,
                    ButtonHTML = String.Format("<a id='addTag' class='link dotline plus' onclick='ASC.CRM.TaskTemplateView.showTemplateConatainerPanel();'>{0}</a>",
                                               CRMSettingResource.AddTaskTemplateContainer)
                };

            _phEmptyContent.Controls.Add(emptyScreenControl);

            //init task responsible
            //there should be user selector with id = "taskTemplateResponsibleSelector"

            RegisterScript();
        }

        #endregion

        #region Methods

        private static EntityType StringToEntityType(string type)
        {
            switch (type)
            {
                case "person":
                    return EntityType.Person;
                case "company":
                    return EntityType.Company;
                case "opportunity":
                    return EntityType.Opportunity;
                case "case":
                    return EntityType.Case;
                default:
                    return EntityType.Contact;
            }
        }

        private void RegisterClientScript(EntityType entityType)
        {
            var apiServer = new Api.ApiServer();
            var templateConatainerList = apiServer.GetApiResponse(
                String.Format("{0}crm/{1}/tasktemplatecontainer.json", SetupInfo.WebApiBaseUrl, entityType.ToString().ToLower()), "GET");

            Page.JsonPublisher(templateConatainerList, "templateConatainerList");

        }

        protected string InitHoursSelect()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 24; i++)
            {
                sb.AppendFormat("<option value='{0}' id='optDeadlineHours_{0}'>{1}</option>", i, i < 10 ? "0" + i.ToString() : i.ToString());
            }

            return sb.ToString();
        }

        protected string InitMinutesSelect()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 60; i++)
            {
                sb.AppendFormat("<option value='{0}' id='optDeadlineMinutes_{0}'>{1}</option>", i, i < 10 ? "0" + i.ToString() : i.ToString());
            }

            return sb.ToString();
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"
                    ASC.CRM.TaskTemplateView.init();

                    ASC.CRM.TaskTemplateView.ConatainerPanel_AddHeaderText = '{0}';
                    ASC.CRM.TaskTemplateView.ConatainerPanel_AddButtonText = '{1}';
                    ASC.CRM.TaskTemplateView.ConatainerPanel_EditHeaderText = '{2}';

                    ASC.CRM.TaskTemplateView.TemplatePanel_AddHeaderText = '{3}';
                    ASC.CRM.TaskTemplateView.TemplatePanel_AddButtonText = '{4}';
                    ASC.CRM.TaskTemplateView.TemplatePanel_EditHeaderText = '{5}';",
                CRMSettingResource.AddNewTaskTemplateContainer.ReplaceSingleQuote(),
                CRMSettingResource.AddThisTaskTemplateContainer.ReplaceSingleQuote(),
                CRMSettingResource.EditSelectedTaskTemplateContainer.ReplaceSingleQuote(),
                CRMSettingResource.AddNewTaskTemplate.ReplaceSingleQuote(),
                CRMSettingResource.AddThisTaskTemplate.ReplaceSingleQuote(),
                CRMSettingResource.EditSelectedTaskTemplate.ReplaceSingleQuote()
            );

            Page.RegisterInlineScript(sb.ToString());
        }

        #endregion
    }
}