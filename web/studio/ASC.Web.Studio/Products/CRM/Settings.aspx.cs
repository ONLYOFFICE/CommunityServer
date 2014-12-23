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
using System.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Settings;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.CRM
{
    public partial class Settings : BasePage
    {
        protected override void PageLoad()
        {
            if (!CRMSecurity.IsAdmin)
                Response.Redirect(PathProvider.StartURL());

            Page.RegisterBodyScripts(LoadControl(VirtualPathUtility.ToAbsolute("~/products/crm/masters/SettingsBodyScripts.ascx")));

            var typeValue = (HttpContext.Current.Request["type"] ?? "common").ToLower();
            ListItemView listItemViewControl;

            string titlePage;
            switch (typeValue)
            {
                case "common":
                    CommonContainerHolder.Controls.Add(LoadControl(CommonSettingsView.Location));

                    titlePage = CRMSettingResource.CommonSettings;
                    break;

                case "deal_milestone":
                    var dealMilestoneViewControl = (DealMilestoneView)LoadControl(DealMilestoneView.Location);
                    CommonContainerHolder.Controls.Add(dealMilestoneViewControl);

                    titlePage = CRMDealResource.DealMilestone;
                    break;

                case "task_category":
                    listItemViewControl = (ListItemView)LoadControl(ListItemView.Location);
                    listItemViewControl.CurrentTypeValue = ListType.TaskCategory;
                    listItemViewControl.AddButtonText = CRMSettingResource.AddThisCategory;
                    listItemViewControl.AddPopupWindowText = CRMSettingResource.CreateNewCategory;
                    listItemViewControl.AddListButtonText = CRMSettingResource.CreateNewCategoryListButton;

                    listItemViewControl.AjaxProgressText = CRMSettingResource.CreateCategoryInProgressing;
                    listItemViewControl.DeleteText = CRMSettingResource.DeleteCategory;
                    listItemViewControl.EditText = CRMSettingResource.EditCategory;
                    listItemViewControl.EditPopupWindowText = CRMSettingResource.EditSelectedCategory;
                    listItemViewControl.DescriptionText = CRMSettingResource.DescriptionTextTaskCategory;
                    listItemViewControl.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextTaskCategoryEditDelete;
                    CommonContainerHolder.Controls.Add(listItemViewControl);
                    titlePage = CRMTaskResource.TaskCategories;
                    break;

                case "history_category":
                    listItemViewControl = (ListItemView)LoadControl(ListItemView.Location);
                    listItemViewControl.CurrentTypeValue = ListType.HistoryCategory;
                    listItemViewControl.AddButtonText = CRMSettingResource.AddThisCategory;
                    listItemViewControl.AddPopupWindowText = CRMSettingResource.CreateNewCategory;
                    listItemViewControl.AddListButtonText = CRMSettingResource.CreateNewCategoryListButton;
                    listItemViewControl.AjaxProgressText = CRMSettingResource.CreateCategoryInProgressing;
                    listItemViewControl.DeleteText = CRMSettingResource.DeleteCategory;
                    listItemViewControl.EditText = CRMSettingResource.EditCategory;
                    listItemViewControl.EditPopupWindowText = CRMSettingResource.EditSelectedCategory;
                    listItemViewControl.DescriptionText = CRMSettingResource.DescriptionTextHistoryCategory;
                    listItemViewControl.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextHistoryCategoryEditDelete;
                    CommonContainerHolder.Controls.Add(listItemViewControl);
                    titlePage = CRMSettingResource.HistoryCategories;
                    break;

                case "contact_stage":
                    listItemViewControl = (ListItemView)LoadControl(ListItemView.Location);
                    listItemViewControl.CurrentTypeValue = ListType.ContactStatus;
                    listItemViewControl.AddButtonText = CRMSettingResource.AddThisStage;
                    listItemViewControl.AddPopupWindowText = CRMSettingResource.CreateNewStage;
                    listItemViewControl.AddListButtonText = CRMSettingResource.CreateNewStageListButton;

                    listItemViewControl.AjaxProgressText = CRMSettingResource.CreateContactStageInProgressing;
                    listItemViewControl.DeleteText = CRMSettingResource.DeleteContactStage;
                    listItemViewControl.EditText = CRMSettingResource.EditContactStage;
                    listItemViewControl.EditPopupWindowText = CRMSettingResource.EditSelectedContactStage;
                    listItemViewControl.DescriptionText = CRMSettingResource.DescriptionTextContactStage;
                    listItemViewControl.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextContactStageEditDelete;
                    CommonContainerHolder.Controls.Add(listItemViewControl);
                    titlePage = CRMContactResource.ContactStages;
                    break;

                case "contact_type":
                    listItemViewControl = (ListItemView)LoadControl(ListItemView.Location);
                    listItemViewControl.CurrentTypeValue = ListType.ContactType;
                    listItemViewControl.AddButtonText = CRMSettingResource.AddThisContactType;
                    listItemViewControl.AddPopupWindowText = CRMSettingResource.CreateNewContactType;
                    listItemViewControl.AddListButtonText = CRMSettingResource.CreateNewContactTypeListButton;

                    listItemViewControl.AjaxProgressText = CRMSettingResource.CreateContactTypeInProgressing;
                    listItemViewControl.DeleteText = CRMSettingResource.DeleteContactType;
                    listItemViewControl.EditText = CRMSettingResource.EditContactType;
                    listItemViewControl.EditPopupWindowText = CRMSettingResource.EditSelectedContactType;
                    listItemViewControl.DescriptionText = CRMSettingResource.DescriptionTextContactType;
                    listItemViewControl.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextContactTypeEditDelete;
                    CommonContainerHolder.Controls.Add(listItemViewControl);
                    titlePage = CRMSettingResource.ContactTypes;
                    break;

                case "tag":
                    var tagSettingsViewControl = (TagSettingsView)LoadControl(TagSettingsView.Location);
                    CommonContainerHolder.Controls.Add(tagSettingsViewControl);

                    titlePage = CRMCommonResource.Tags;
                    break;

                case "web_to_lead_form":
                    CommonContainerHolder.Controls.Add(LoadControl(WebToLeadFormView.Location));
                    titlePage = CRMSettingResource.WebToLeadsForm;
                    break;
                case "task_template":
                    CommonContainerHolder.Controls.Add(LoadControl(TaskTemplateView.Location));

                    titlePage = CRMSettingResource.TaskTemplates;
                    break;

                case "invoice_items":

                    var actionValue = (HttpContext.Current.Request["action"] ?? "").ToLower();
                    if (!String.IsNullOrEmpty(actionValue) && actionValue == "manage")
                    {
                        var idParam = HttpContext.Current.Request["id"];
                        InvoiceItem targetInvoiceItem = null;

                        if (!String.IsNullOrEmpty(idParam))
                        {
                            targetInvoiceItem = Global.DaoFactory.GetInvoiceItemDao().GetByID(Convert.ToInt32(idParam));
                            if (targetInvoiceItem == null)
                            {
                                Response.Redirect(PathProvider.StartURL() + "settings.aspx?type=invoice_items");
                            }
                        }


                        var invoiceProductsViewControl = (InvoiceItemActionView)LoadControl(InvoiceItemActionView.Location);
                        invoiceProductsViewControl.TargetInvoiceItem = targetInvoiceItem;
                        CommonContainerHolder.Controls.Add(invoiceProductsViewControl);

                        titlePage = CRMCommonResource.ProductsAndServices;

                        var headerTitle = targetInvoiceItem == null ?
                                              CRMInvoiceResource.CreateNewInvoiceItem :
                                              String.Format(CRMInvoiceResource.UpdateInvoiceItem, targetInvoiceItem.Title);
                        Master.CurrentPageCaption = headerTitle;
                        Title = HeaderStringHelper.GetPageTitle(headerTitle);
                    }
                    else
                    {
                        var invoiceProductsViewControl = (InvoiceItemsView)LoadControl(InvoiceItemsView.Location);
                        CommonContainerHolder.Controls.Add(invoiceProductsViewControl);

                        titlePage = CRMCommonResource.ProductsAndServices;
                    }
                    break;

                case "invoice_tax":
                    var invoiceTaxesViewControl = (InvoiceTaxesView)LoadControl(InvoiceTaxesView.Location);
                    CommonContainerHolder.Controls.Add(invoiceTaxesViewControl);

                    titlePage = CRMCommonResource.InvoiceTaxes;

                    break;

                case "organisation_profile":
                    var organisationProfileControl = (OrganisationProfile)LoadControl(OrganisationProfile.Location);
                    CommonContainerHolder.Controls.Add(organisationProfileControl);

                    titlePage = CRMCommonResource.OrganisationProfile;

                    break;

                case "voip.common":
                    var voIPCommon = (VoipCommon)LoadControl(VoipCommon.Location);
                    CommonContainerHolder.Controls.Add(voIPCommon);

                    titlePage = CRMCommonResource.VoIPCommonSettings;

                    break;

                case "voip.numbers":
                    var voIPNumbers = (VoipNumbers)LoadControl(VoipNumbers.Location);
                    CommonContainerHolder.Controls.Add(voIPNumbers);

                    titlePage = CRMCommonResource.VoIPNumbersSettings;

                    break;

                case "voip.calls":
                    var voIPCalls = (VoipCalls)LoadControl(VoipCalls.Location);
                    CommonContainerHolder.Controls.Add(voIPCalls);

                    titlePage = CRMCommonResource.VoIPCallsSettings;

                    break;

                default:
                    CommonContainerHolder.Controls.Add(LoadControl(CustomFieldsView.Location));

                    titlePage = CRMSettingResource.CustomFields;
                    break;
            }

            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? titlePage);
        }
    }
}