<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactActionView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Contacts.ContactActionView" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.CRM.Core.Entities" %>
<%@ Import Namespace="ASC.Web.CRM" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>


<div id="crm_contactMakerDialog">
    <input type="hidden" name="typeAddedContact" id="typeAddedContact" value="<%= TypeAddedContact %>" />
    <input type="hidden" name="linkMessageId" id="linkMessageId" value="<%= LinkMessageId %>" />

    <table border="0" cellpadding="0" cellspacing="0" id="contactProfileEdit">
        <colgroup>
            <col style="width:720px;"/>
            <col/>
        </colgroup>
        <tbody>
        <tr>
            <td class="contactInfo">
            <% if (TargetContact != null && TargetContact is Person || TargetContact == null && UrlParameters.Type == "people") %>
            <% { %>
            <div class="info_for_person headerPanelSmall-splitter clearFix">
                <table class="info_for_person_table" cellpadding="0" cellspacing="0">
                <colgroup>
                    <col style="width: 50%;"/>
                    <col style="width: 50%;"/>
                </colgroup>
                <tbody style="width: 100%;">
                <tr>
                    <td style="padding-right:10px;">
                        <div class="requiredField">
                            <span class="requiredErrorText"><%= CRMContactResource.ErrorEmptyContactFirstName%></span>
                            <div class="headerPanelSmall"><%= CRMContactResource.FirstName%></div>
                            <input type="text" class="textEdit generalField" name="baseInfo_firstName" maxlength="255"
                               value="<%= GetFirstName() %>"/>
                        </div>
                    </td>
                    <td style="padding-left:10px;">
                        <div class="bold"><%= CRMContactResource.LastName%></div>
                        <input type="text" class="textEdit generalField" name="baseInfo_lastName" maxlength="255"
                            value="<%= GetLastName() %>"/>
                    </td>
                </tr>
                <tr>
                    <td style="padding-right:5px;">
                        <div>
                            <div class="bold"><%= CRMContactResource.CompanyName%></div>
                            <div id="companySelectorsContainer">
                                <div>
                                    <input type="hidden" name="baseInfo_compID" value="<%= GetCompanyIDforPerson() %>" />
                                    <input type="hidden" name="baseInfo_compName"/>
                                </div>
                            </div>
                        </div>
                    </td>
                    <td style="padding-left:10px;">
                        <div>
                            <div class="bold"><%= CRMContactResource.PersonPosition%></div>
                            <input type="text" class="textEdit generalField" name="baseInfo_personPosition"
                                maxlength="255" value="<%= GetTitle()%>" />
                        </div>
                    </td>
                </tr>
                </tbody>
                </table>
            </div>
            <% } else { %>
                <div class="info_for_company headerPanelSmall-splitter clearFix">
                    <div class="requiredField">
                        <span class="requiredErrorText"><%= CRMContactResource.ErrorEmptyCompanyName%></span>
                        <div class="headerPanelSmall"><%= CRMContactResource.CompanyName%></div>
                        <input type="text" class="textEdit generalField" name="baseInfo_companyName" maxlength="255"
                            value="<%= GetCompanyName()%>" />
                    </div>
                </div>
            <% } %>

                <dl id="generalListEdit" class="headerPanelSmall-splitter clearFix">
                    <dt class="bold crm-headerHiddenToggledBlock"><%= CRMContactResource.ContactType %></dt>
                    <dd id="contactTypeContainer">
                        <div class="display-none">
                            <select class="contactTypeSelect comboBox" name="baseInfo_contactType">
                                <option value="0"><%= CRMCommonResource.NoSet %></option>
                            </select>
                        </div>
                    </dd>



                    <dt class="bold crm-headerHiddenToggledBlock"><%= ContactInfoType.Email.ToLocalizedString()%></dt>
                    <dd id="emailContainer" onclick="ASC.CRM.ContactActionView.editCommunicationsEvent(event, jq(this).attr('id'))">
                        <div style="display:none !important;">
                            <table class="borderBase input_with_type" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td><input type="text" class="textEdit" name="contactInfo_Email_0_<%= (int)ContactInfoBaseCategory.Work %>_0" value="" maxlength="100"/></td>
                                    <td style="width:1%;"><a onclick="ASC.CRM.ContactActionView.showBaseCategoriesPanel(this);"><%= ContactInfoBaseCategory.Work.ToLocalizedString()%></a></td>
                                </tr>
                            </table>
                            <div class="actions_for_item">
                                <a class="crm-deleteLink" title="<%=CRMCommonResource.Delete%>"></a>
                                <a class="is_primary not_primary_field" title="<%=CRMJSResource.CheckAsPrimary%>"></a>
                                <a class="crm-addNewLink" title="<%=CRMJSResource.AddNewEmail%>"></a>
                            </div>
                            <span class="requiredErrorText" style="float:right;"><%=CRMContactResource.ErrorInvalidEmail%></span>
                        </div>
                    </dd>


                    <dt class="bold crm-headerHiddenToggledBlock"><%= ContactInfoType.Phone.ToLocalizedString()%></dt>
                    <dd id="phoneContainer" onclick="ASC.CRM.ContactActionView.editCommunicationsEvent(event, jq(this).attr('id'))">
                        <div style="display:none !important;">
                            <table class="borderBase input_with_type" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td><input type="text" class="textEdit" name="contactInfo_Phone_0_<%= (int)PhoneCategory.Work %>_0" value="" maxlength="100"/></td>
                                    <td style="width:1%;"><a onclick="ASC.CRM.ContactActionView.showPhoneCategoriesPanel(this);"><%= PhoneCategory.Work.ToLocalizedString()%></a></td>
                                </tr>
                            </table>
                            <div class="actions_for_item">
                                <a class="crm-deleteLink" title="<%=CRMCommonResource.Delete%>"></a>
                                <a class="is_primary not_primary_field" title="<%=CRMJSResource.CheckAsPrimary%>"></a>
                                <a class="crm-addNewLink" title="<%=CRMJSResource.AddNewPhone%>"></a>
                            </div>
                            <span class="requiredErrorText" style="float:right;"><%=CRMContactResource.ErrorInvalidPhone%></span>
                        </div>
                    </dd>


                    <dt class="bold crm-headerHiddenToggledBlock"><%= CRMContactResource.ContactWebSiteAndSocialProfiles%></dt>
                    <dd id="websiteAndSocialProfilesContainer" onclick="ASC.CRM.ContactActionView.editCommunicationsEvent(event, jq(this).attr('id'))">
                        <div style="display: none !important;">
                            <table class="borderBase input_with_type" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td><input type="text" name="contactInfo_<%= ContactInfoType.Website + "_0_" + (int)ContactInfoBaseCategory.Work %>_0" class="textEdit" value="" maxlength="255"/></td>
                                    <td style="width:1%;">
                                         <a class="social_profile_type" onclick="ASC.CRM.ContactActionView.showSocialProfileCategoriesPanel(this);">
                                            <%= ContactInfoType.Website.ToLocalizedString()%>
                                        </a>
                                    </td>
                                    <td style="width:1%;">
                                        <a class="social_profile_category" onclick="ASC.CRM.ContactActionView.showBaseCategoriesPanel(this);">
                                            <%= ContactInfoBaseCategory.Work.ToLocalizedString()%>
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <%--<div class="text-medium-describe" style="min-height:14px;"> </div>
                            --%>
                            <div class="actions_for_item">
                                <a class="crm-deleteLink" title="<%=CRMCommonResource.Delete%>"></a>
                                <a class="find_profile" title="" style="display: none;" onclick=""></a>
                                <a class="crm-addNewLink" title="<%=CRMCommonResource.Add%>"></a>
                            </div>
                        </div>
                    </dd>


                    <dt class="bold crm-headerHiddenToggledBlock"><%= ContactInfoType.Address.ToLocalizedString()%></dt>
                    <dd id="addressContainer" onclick="ASC.CRM.ContactActionView.editAddressEvent(event)">
                        <div style="display:none !important;" selectname="contactInfo_Address_0_<%= (int)AddressCategory.Billing + "_"  + AddressPart.Country%>_0">
                            <table class="address-tbl" cellpadding="0" cellspacing="0">
                                <colgroup>
                                    <col style="width: 50%;"/>
                                    <col style="width: 50%;"/>
                                </colgroup>
                                <tbody>
                                    <tr>
                                        <td class="cell select-cell">
                                            <select class="address_category comboBox" onchange="ASC.CRM.ContactActionView.changeAddressCategory(this)">
                                                <% foreach (AddressCategory item in Enum.GetValues(typeof(AddressCategory))) %>
                                                <% { %>
                                                    <option category="<%=(int)item%>" <%= item == AddressCategory.Billing ? "selected='selected'" : "" %> ><%=item.ToLocalizedString()%></option>
                                                <% } %>
                                            </select>
                                        </td>
                                        <td class="cell input-cell textarea-cell" rowspan="4">
                                            <textarea class="contact_street" maxlength="255"
                                                name="contactInfo_Address_0_<%=(int)AddressCategory.Billing + "_" + AddressPart.Street %>_0"
                                                placeholder="<%= CRMJSResource.AddressWatermark %>"></textarea>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="cell input-cell">
                                            <input type="text" class="contact_city textEdit" maxlength="255"
                                                name="contactInfo_Address_0_<%=(int)AddressCategory.Billing + "_" + AddressPart.City %>_0"
                                                placeholder="<%= CRMJSResource.CityWatermark %>"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="cell input-cell">
                                            <input type="text" class="contact_state textEdit" maxlength="255"
                                                name="contactInfo_Address_0_<%=(int)AddressCategory.Billing + "_" + AddressPart.State %>_0"
                                                placeholder="<%= CRMJSResource.StateWatermark %>"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="cell input-cell">
                                            <input type="text" class="contact_zip textEdit" maxlength="255"
                                                name="contactInfo_Address_0_<%=(int)AddressCategory.Billing + "_" + AddressPart.Zip %>_0"
                                                placeholder="<%= CRMJSResource.ZipCodeWatermark %>"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="input-cell" colspan="2">
                                            <input type="text" class="contact_country textEdit" maxlength="255"
                                                name="contactInfo_Address_0_<%=(int)AddressCategory.Billing + "_" + AddressPart.Country %>_0"
                                                placeholder="<%= CRMJSResource.CountryWatermark %>"/>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            <div class="actions_for_item">
                                <a class="crm-deleteLink" title="<%=CRMCommonResource.Delete%>"></a>
                                <a class="is_primary not_primary_field" title="<%=CRMJSResource.CheckAsPrimary%>"></a>
                                <a class="crm-addNewLink" title="<%=CRMContactResource.AddNewAddress%>"></a>
                            </div>
                        </div>
                    </dd>

                    <dt class="bold crm-headerHiddenToggledBlock"><%= CRMCommonResource.Tags %></dt>
                    <dd id="tagsContainer">
                        <div class="display-none">
                            <input type="hidden" name="baseInfo_assignedTags" />
                        </div>
                    </dd>


                    <dt class="bold crm-headerHiddenToggledBlock"><%= CRMCommonResource.Currency %></dt>
                    <dd id="currencyContainer">
                        <div class="display-none">
                            <select class="currencySelect comboBox" name="baseInfo_currency">
                                <option value=""><%= CRMCommonResource.NoSet %></option>
                            </select>
                        </div>
                    </dd>


                    <dt <%= (TargetContact != null && !String.IsNullOrEmpty(TargetContact.About)) ? "class='bold'": "class='bold crm-headerHiddenToggledBlock'"%> ><%= CRMContactResource.Overview%></dt>
                    <dd id="overviewContainer" onclick="ASC.CRM.ContactActionView.editCommunicationsEvent(event, jq(this).attr('id'))">
                        <div style="display:none !important;">
                            <textarea type="text" rows="4" name="baseInfo_contactOverview" class="textEdit baseInfo_contactOverview"></textarea>
                            <div class="actions_for_item">
                                <a class="crm-deleteLink" title="<%=CRMCommonResource.Delete%>"></a>
                            </div>
                        </div>

                        <% if (TargetContact != null && !String.IsNullOrEmpty(TargetContact.About)) { %>
                        <div>
                            <textarea type="text" rows="4" name="baseInfo_contactOverview"
                                class="textEdit baseInfo_contactOverview"><%= TargetContact.About.HtmlEncode()%></textarea>
                            <div class="actions_for_item">
                                <a class="crm-deleteLink" title="<%=CRMCommonResource.Delete%>"></a>
                            </div>
                        </div>
                        <% } %>
                    </dd>
                </dl>

                <% if (CRMSecurity.IsAdmin) { %>
                <div id="otherContactCustomFieldPanel" class="headerPanelSmall-splitter">
                    <div class="bold" style="margin: 16px 0 10px;"><%= CRMSettingResource.OtherFields %></div>
                    <a onclick="ASC.CRM.ContactActionView.showGotoAddSettingsPanel();" style="text-decoration: underline" class="linkMedium">
                        <%= CRMSettingResource.SettingCustomFields %>
                    </a>
                </div>
                <% } %>

                <% if (UrlParameters.Type != "people") { %>
                <dl id="assignedContactsListEdit" class="headerPanelSmall-splitter">
                    <dt class="headerToggleBlock clearFix">
                        <span class="headerToggle header-base"><%=CRMContactResource.AllPersons%></span>
                        <span class="openBlockLink"><%= CRMCommonResource.Show %></span>
                        <span class="closeBlockLink"><%= CRMCommonResource.Hide %></span>
                    </dt>
                    <dd class="underHeaderBase clearFix"></dd>

                    <dt class="assignedContactsLink"></dt>
                    <dd class="assignedContactsLink">
                        <a class="link-with-entity baseLinkAction linkMedium" onclick="ASC.CRM.ContactActionView.showAssignedContactPanel()" >
                            <%=CRMContactResource.AssignContact%>
                        </a>
                    </dd>

                    <dt class="bold assignedContacts hiddenFields"><%= CRMContactResource.AssignPersonFromExisting%></dt>
                    <dd class="assignedContacts hiddenFields">
                    </dd>

                    <dt style="margin-top:20px;">
                        <div id="contactListBox">
                            <table id="contactTable" class="table-list padding4" cellpadding="0" cellspacing="0">
                                <tbody>
                                </tbody>
                            </table>
                        </div>
                    </dt>
                    <dd>
                        <input type="hidden" name="baseInfo_assignedContactsIDs" />
                        <input type="hidden" name="baseInfo_assignedNewContactsIDs" />
                    </dd>
                </dl>
                <% } %>

                <div class="contactManagerPanel">
                    <div class="header-base"><%= CRMCommonResource.PrivatePanelHeader %></div>
                    <div class="contactManager-selectorContent">
                            <div class="headerPanelSmall"><%= CRMContactResource.AssignContactManager %></div>
                            <div id="contactActionViewManager">
                            </div>
                            <input id="selectedContactManagers" type="hidden" value="" name="selectedContactManagers" />
                            <input id="notifyContactManagers" type="hidden" value="" name="notifyContactManagers" />
                    </div>
                    <div style="margin-top:23px;" id="makePublicPanel">
                        <input id="isPublicContact" type="hidden" value="" name="isPublicContact" />
                    </div>
                </div>
            </td>
            <td style="padding-left: 20px;">
                <div class="additionInfo">
                    <div id="contactPhoto" class="contact-photo">
                        <div class="contact-photo-img">
                            <img class="contact_photo" alt="" src="<%= String.Format("{0}?{1}",ContactPhotoManager.GetBigSizePhoto(0, UrlParameters.Type != "people"), new DateTime().Ticks) %>"
                                data-avatarurl="<%= TargetContact != null ? String.Format("{0}HttpHandlers/filehandler.ashx?action=contactphotoulr&cid={1}&isc={2}&ps=3", PathProvider.BaseAbsolutePath, TargetContact.ID, TargetContact is Company) : "" %>" />
                        </div>

                        <div class="under_logo">
                            <a onclick="ASC.CRM.ContactActionView.prepareSocialNetworks(); ASC.CRM.SocialMedia.OpenLoadPhotoWindow(); return false;" class="linkChangePhoto grey-phone">
                                 <span class="bold"><%= CRMContactResource.ChangePhoto %></span>
                            </a>
                        </div>
                    </div>

                    <input type="hidden" value="null" id="uploadPhotoPath" name="uploadPhotoPath"/>
                </div>
            </td>
        </tr>
        </tbody>
    </table>
    <div class="middle-button-container">
        <asp:LinkButton runat="server" CommandName="SaveContact" CommandArgument="0" ID="saveContactButton"
            OnCommand="SaveOrUpdateContact" CssClass="button blue big" />
        <span class="splitter-buttons"></span>
        <% if (TargetContact == null) %>
        <% { %>
        <asp:LinkButton runat="server" CommandName="SaveContact" CommandArgument="1" ID="saveAndCreateContactButton"
            OnCommand="SaveOrUpdateContact" CssClass="button gray big" />
        <span class="splitter-buttons"></span>
         <% } %>
        <asp:LinkButton runat="server" ID="cancelButton" class="button gray big cancelSbmtFormBtn"><%= CRMCommonResource.Cancel%></asp:LinkButton>

        <% if (TargetContact != null && CRMSecurity.CanDelete(TargetContact)) %>
        <% { %>
        <span class="splitter-buttons"></span>
        <a id="deleteContactButton" class="button gray big"
            isCompany="<%= TargetContact is Company ? "true" : "false" %>" contactName="<%= TargetContact.GetTitle().HtmlEncode().ReplaceSingleQuote() %>" >
            <%= TargetContact is Company ? CRMContactResource.DeleteThisCompany : CRMContactResource.DeleteThisPerson %>
        </a>
        <% } %>
    </div>
</div>


<div id="phoneCategoriesPanel" class="studio-action-panel">
    <ul class="dropdown-content">
    <% foreach (PhoneCategory item in Enum.GetValues(typeof(PhoneCategory))) %>
    <% { %>
        <li><a category="<%=(int)item%>" class="dropdown-item"><%=item.ToLocalizedString()%></a></li>
    <% } %>
    </ul>
</div>

<div id="baseCategoriesPanel" class="studio-action-panel">
    <ul class="dropdown-content">
    <% foreach (ContactInfoBaseCategory item in Enum.GetValues(typeof(ContactInfoBaseCategory))) %>
    <% { %>
        <li><a category="<%=(int)item%>" class="dropdown-item"><%=item.ToLocalizedString()%></a></li>
    <% } %>
    </ul>
</div>

<div id="socialProfileCategoriesPanel" class="studio-action-panel">
    <ul class="dropdown-content">
    <% foreach (var item in ContactInfoTypes) %>
    <% { %>
        <li>
            <a category="<%=(int)item%>" categoryName="<%=item.ToString()%>" class="dropdown-item">
                <%=item.ToLocalizedString()%>
            </a>
        </li>
    <% } %>
    </ul>
</div>

<div id="divSMProfilesWindow" class="borderBase">
    <div class="header-base-medium divHeader">
        <span></span>
        <label class="cancel_cross" title="<%= CRMCommonResource.CloseWindow%>" onclick="jq('#divSMProfilesWindow').hide();"></label>
    </div>
    <div class="divSMProfilesWindowBody mobile-overflow">
        <table id="sm_tbl_UserList">
        </table>
        <div class="divWait">
            <span class="loader-text-block"><%= CRMSocialMediaResource.PleaseWait%></span>            
        </div>
        <div class="divNoProfiles">
            <%= UrlParameters.Type == "people" ? CRMSocialMediaResource.NoAccountsHasBeenFound : CRMSocialMediaResource.NoCompaniesHasBeenFound%>
        </div>
    </div>
</div>