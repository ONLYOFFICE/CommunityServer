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
using System.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Controls.Settings;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.CRM
{
    public partial class Settings : BasePage
    {
        protected string PageTitle { get; private set; }

        protected bool IsInvoiceItemsList { get; private set; }

        protected override void PageLoad()
        {
            if (!CRMSecurity.IsAdmin)
                Response.Redirect(PathProvider.StartURL());

            Page.RegisterBodyScripts(PathProvider.GetFileStaticRelativePath,
                                         "settings.js",
                                         "settings.invoices.js"
                                     );

            var typeValue = (HttpContext.Current.Request["type"] ?? "common").ToLower();
            ListItemView listItemViewControl;

            var headerView = (SettingsHeaderView) LoadControl(SettingsHeaderView.Location);

            switch (typeValue)
            {
                case "common":
                    PageTitle = CRMSettingResource.CommonSettings;
                    
                    headerView.HeaderText = CRMSettingResource.ExportData;
                    TitleContentHolder.Controls.Add(headerView);

                    CommonContainerHolder.Controls.Add(LoadControl(CommonSettingsView.Location));
                    break;

                case "currency":
                    PageTitle = CRMSettingResource.CurrencySettings;

                    headerView.HeaderText = PageTitle;
                    TitleContentHolder.Controls.Add(headerView);

                    CommonContainerHolder.Controls.Add(LoadControl(CurrencySettingsView.Location));
                    break;

                case "deal_milestone":
                    PageTitle = CRMDealResource.DealMilestone;

                    headerView.HeaderText = PageTitle;
                    headerView.DescriptionText = CRMSettingResource.DescriptionTextDealMilestone;
                    headerView.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextDealMilestoneEditDelete;
                    headerView.AddListButtonId = "createNewDealMilestone";
                    headerView.AddListButtonText = CRMSettingResource.CreateNewDealMilestoneListButton;
                    TitleContentHolder.Controls.Add(headerView);

                    CommonContainerHolder.Controls.Add(LoadControl(DealMilestoneView.Location));
                    break;

                case "task_category":
                    PageTitle = CRMTaskResource.TaskCategories;

                    headerView.HeaderText = PageTitle;
                    headerView.DescriptionText = CRMSettingResource.DescriptionTextTaskCategory;
                    headerView.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextTaskCategoryEditDelete;
                    headerView.AddListButtonText = CRMSettingResource.CreateNewCategoryListButton;
                    TitleContentHolder.Controls.Add(headerView);

                    listItemViewControl = (ListItemView)LoadControl(ListItemView.Location);
                    listItemViewControl.CurrentTypeValue = ListType.TaskCategory;
                    listItemViewControl.AddButtonText = CRMSettingResource.AddThisCategory;
                    listItemViewControl.AddPopupWindowText = CRMSettingResource.CreateNewCategory;
                    listItemViewControl.AjaxProgressText = CRMSettingResource.CreateCategoryInProgressing;
                    listItemViewControl.DeleteText = CRMSettingResource.DeleteCategory;
                    listItemViewControl.EditText = CRMSettingResource.EditCategory;
                    listItemViewControl.EditPopupWindowText = CRMSettingResource.EditSelectedCategory;
                    CommonContainerHolder.Controls.Add(listItemViewControl);
                    break;

                case "history_category":
                    PageTitle = CRMSettingResource.HistoryCategories;

                    headerView.HeaderText = PageTitle;
                    headerView.DescriptionText = CRMSettingResource.DescriptionTextHistoryCategory;
                    headerView.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextHistoryCategoryEditDelete;
                    headerView.AddListButtonText = CRMSettingResource.CreateNewCategoryListButton;
                    TitleContentHolder.Controls.Add(headerView);

                    listItemViewControl = (ListItemView)LoadControl(ListItemView.Location);
                    listItemViewControl.CurrentTypeValue = ListType.HistoryCategory;
                    listItemViewControl.AddButtonText = CRMSettingResource.AddThisCategory;
                    listItemViewControl.AddPopupWindowText = CRMSettingResource.CreateNewCategory;
                    listItemViewControl.AjaxProgressText = CRMSettingResource.CreateCategoryInProgressing;
                    listItemViewControl.DeleteText = CRMSettingResource.DeleteCategory;
                    listItemViewControl.EditText = CRMSettingResource.EditCategory;
                    listItemViewControl.EditPopupWindowText = CRMSettingResource.EditSelectedCategory;
                    CommonContainerHolder.Controls.Add(listItemViewControl);
                    break;

                case "contact_stage":
                    PageTitle = CRMContactResource.ContactStages;

                    headerView.HeaderText = PageTitle;
                    headerView.DescriptionText = CRMSettingResource.DescriptionTextContactStage;
                    headerView.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextContactStageEditDelete;
                    headerView.AddListButtonText = CRMSettingResource.CreateNewStageListButton;
                    headerView.ShowContactStatusAskingDialog = true;
                    TitleContentHolder.Controls.Add(headerView);

                    listItemViewControl = (ListItemView)LoadControl(ListItemView.Location);
                    listItemViewControl.CurrentTypeValue = ListType.ContactStatus;
                    listItemViewControl.AddButtonText = CRMSettingResource.AddThisStage;
                    listItemViewControl.AddPopupWindowText = CRMSettingResource.CreateNewStage;
                    listItemViewControl.AjaxProgressText = CRMSettingResource.CreateContactStageInProgressing;
                    listItemViewControl.DeleteText = CRMSettingResource.DeleteContactStage;
                    listItemViewControl.EditText = CRMSettingResource.EditContactStage;
                    listItemViewControl.EditPopupWindowText = CRMSettingResource.EditSelectedContactStage;
                    CommonContainerHolder.Controls.Add(listItemViewControl);
                    break;

                case "contact_type":
                    PageTitle = CRMSettingResource.ContactTypes;

                    headerView.HeaderText = PageTitle;
                    headerView.DescriptionText = CRMSettingResource.DescriptionTextContactType;
                    headerView.DescriptionTextEditDelete = CRMSettingResource.DescriptionTextContactTypeEditDelete;
                    headerView.AddListButtonText = CRMSettingResource.CreateNewContactTypeListButton;
                    TitleContentHolder.Controls.Add(headerView);

                    listItemViewControl = (ListItemView)LoadControl(ListItemView.Location);
                    listItemViewControl.CurrentTypeValue = ListType.ContactType;
                    listItemViewControl.AddButtonText = CRMSettingResource.AddThisContactType;
                    listItemViewControl.AddPopupWindowText = CRMSettingResource.CreateNewContactType;
                    listItemViewControl.AjaxProgressText = CRMSettingResource.CreateContactTypeInProgressing;
                    listItemViewControl.DeleteText = CRMSettingResource.DeleteContactType;
                    listItemViewControl.EditText = CRMSettingResource.EditContactType;
                    listItemViewControl.EditPopupWindowText = CRMSettingResource.EditSelectedContactType;
                    CommonContainerHolder.Controls.Add(listItemViewControl);
                    break;

                case "tag":
                    PageTitle = CRMCommonResource.Tags;

                    headerView.HeaderText = PageTitle;
                    headerView.TabsContainerId = "TagSettingsTabs";
                    headerView.AddListButtonId = "createNewTagSettings";
                    headerView.AddListButtonText = CRMSettingResource.CreateNewTagListButton;
                    headerView.ShowTagAskingDialog = true;
                    TitleContentHolder.Controls.Add(headerView);

                    CommonContainerHolder.Controls.Add(LoadControl(TagSettingsView.Location));
                    break;

                case "web_to_lead_form":
                    PageTitle = CRMSettingResource.WebToLeadsForm;

                    CommonContainerHolder.Controls.Add(LoadControl(WebToLeadFormView.Location));
                    break;

                //case "task_template":
                //    PageTitle = CRMSettingResource.TaskTemplates;

                //    CommonContainerHolder.Controls.Add(LoadControl(TaskTemplateView.Location));
                //    break;

                case "invoice_items":

                    var actionValue = (HttpContext.Current.Request["action"] ?? "").ToLower();
                    if (!String.IsNullOrEmpty(actionValue) && actionValue == "manage")
                    {
                        var idParam = HttpContext.Current.Request["id"];
                        InvoiceItem targetInvoiceItem = null;

                        if (!String.IsNullOrEmpty(idParam))
                        {
                            targetInvoiceItem = DaoFactory.InvoiceItemDao.GetByID(Convert.ToInt32(idParam));
                            if (targetInvoiceItem == null)
                            {
                                Response.Redirect(PathProvider.StartURL() + "Settings.aspx?type=invoice_items");
                            }
                        }

                        PageTitle = targetInvoiceItem == null ?
                                              CRMInvoiceResource.CreateNewInvoiceItem :
                                              String.Format(CRMInvoiceResource.UpdateInvoiceItem, targetInvoiceItem.Title);

                        headerView.HeaderText = PageTitle;
                        TitleContentHolder.Controls.Add(headerView);

                        var invoiceProductsViewControl = (InvoiceItemActionView)LoadControl(InvoiceItemActionView.Location);
                        invoiceProductsViewControl.TargetInvoiceItem = targetInvoiceItem;
                        CommonContainerHolder.Controls.Add(invoiceProductsViewControl);
                    }
                    else
                    {
                        PageTitle = CRMCommonResource.ProductsAndServices;

                        headerView.HeaderText = PageTitle;
                        TitleContentHolder.Controls.Add(headerView);
                        
                        IsInvoiceItemsList = true;
                        
                        CommonContainerHolder.Controls.Add(LoadControl(InvoiceItemsView.Location));
                    }
                    break;

                case "invoice_tax":
                    PageTitle = CRMCommonResource.InvoiceTaxes;
                    
                    headerView.HeaderText = PageTitle;
                    headerView.DescriptionText = CRMInvoiceResource.InvoiceTaxesDescriptionText;
                    headerView.DescriptionTextEditDelete = CRMInvoiceResource.InvoiceTaxesDescriptionTextEditDelete;
                    headerView.AddListButtonId = "createNewTax";
                    headerView.AddListButtonText = CRMInvoiceResource.CreateInvoiceTax;
                    TitleContentHolder.Controls.Add(headerView);

                    CommonContainerHolder.Controls.Add(LoadControl(InvoiceTaxesView.Location));
                    break;

                case "organisation_profile":
                    PageTitle = CRMCommonResource.OrganisationProfile;

                    CommonContainerHolder.Controls.Add(LoadControl(OrganisationProfile.Location));
                    break;

                case "voip.common":
                    PageTitle = CRMCommonResource.VoIPCommonSettings;

                    CommonContainerHolder.Controls.Add(LoadControl(VoipCommon.Location));
                    break;

                case "voip.numbers":
                    PageTitle = CRMCommonResource.VoIPNumbersSettings;

                    CommonContainerHolder.Controls.Add(LoadControl(VoipNumbers.Location));
                    break;

                default:
                    PageTitle = CRMSettingResource.CustomFields;

                    headerView.HeaderText = PageTitle;
                    headerView.TabsContainerId = "CustomFieldsTabs";
                    headerView.AddListButtonId = "createNewField";
                    headerView.AddListButtonText = CRMSettingResource.CreateNewFieldListButton;
                    TitleContentHolder.Controls.Add(headerView);

                    CommonContainerHolder.Controls.Add(LoadControl(CustomFieldsView.Location));
                    break;
            }

            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? PageTitle);
        }
    }
}