<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrganisationProfile.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.OrganisationProfile" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.CRM.Classes" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>


<div id="settings_organisation_profile" class="clearFix">
        <div class="header-base settingsHeader settingsHeaderBase"><%= CRMSettingResource.BasicInformation%></div>
        <div class="headerPanelSmall-splitter">
            <div class="header-base-small headerPanelSmall"><%= CRMContactResource.CompanyName %>:</div>
            <input type="text" class="textEdit settingsOrganisationProfileName"  maxlength="255" value="" /> 
        </div>
        <div class="middle-button-container">
            <a class="button blue middle save_base_info" href="javascript:void(0);">
                <%= CRMCommonResource.Save %>
            </a>
        </div>

        <div class="header-base settingsHeader settingsHeaderLogo"><%= CRMSettingResource.OrganisationLogo %></div>
        <div class="headerPanelSmall-splitter clearFix">
            <div class="float-left companyLogoComtainer">
               <div class="contact-photo">
                    <div class="contact-photo-img">
                        <img class="contact_photo" src="<%= ASC.Web.CRM.Classes.OrganisationLogoManager.GetDefaultLogoUrl() %>"
                            title="<%= CRMSettingResource.OrganisationLogo %>" alt="<%= CRMSettingResource.OrganisationLogo %>" />
                    </div>
                    <div class="under_logo">
                        <a id="changeOrganisationLogo" class="linkChangePhoto grey-phone">
                             <span class="bold"><%= CRMSettingResource.ChangeLogo%></span>
                        </a>
                    </div>
                </div>
            </div>
            <div class="settings-help-block">
                <p><%=String.Format(CRMSettingResource.SettingsCompanyLogoHelp,
                   "<strong>",
                   "</strong>",
                   FileSizeComment.FilesSizeToString(SetupInfo.MaxImageUploadSize),
                   "<br/>",
                   OrganisationLogoManager.OrganisationLogoSize.Width,
                   OrganisationLogoManager.OrganisationLogoSize.Height)%></p>
                <% if (!string.IsNullOrEmpty(HelpLink)) { %>
                <a target="_blank" href="<%= HelpLink + "/gettingstarted/crm.aspx#MakingInvoices_block" %>" class="linkAction"><%= CRMCommonResource.LearnMore %></a>
                <% } %>
            </div>
        </div>
        <span class="fileUploadError"></span>
        <input type="hidden" value="null" id="uploadOrganisationLogoPath" />
        <div class="middle-button-container">
            <a class="button blue middle save_logo disable" href="javascript:void(0);">
                <%= CRMCommonResource.Save %>
            </a>
            <span class="splitter-buttons"></span>
            <a class="button gray middle restore_default_logo disable" href="javascript:void(0);">
                <%= CRMSettingResource.RestoreLogoToDeafult %>
            </a>
        </div>


        <div class="header-base settingsHeader settingsHeaderAddress"><%= CRMSettingResource.Address %></div>
        <div class="headerPanelSmall-splitter clearFix">
            <table class="address-tbl" cellpadding="0" cellspacing="0">
                <colgroup>
                    <col style="width: 50%;"/>
                    <col style="width: 50%;"/>
                </colgroup>
                <tbody>
                    <tr>
                        <td class="cell select-cell">
                            <select class="address_category comboBox disable" onchange="ASC.CRM.ContactActionView.changeAddressCategory(this)">
                                <% foreach (AddressCategory item in Enum.GetValues(typeof(AddressCategory))) %>
                                <% { %>
                                    <option category="<%=(int)item%>" <%= item == AddressCategory.Billing ? "selected='selected' class='default'" : "" %> ><%=item.ToLocalizedString()%></option>
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
            <div class="settings-help-block">
                <p><%=String.Format(CRMSettingResource.SettingsCompanyAddressHelp,
                   "<strong>",
                   "</strong>")%></p>
                <% if (!string.IsNullOrEmpty(HelpLink)) { %>
                <a target="_blank" href="<%= HelpLink + "/gettingstarted/crm.aspx#MakingInvoices_block" %>" class="linkAction"><%= CRMCommonResource.LearnMore %></a>
                <% } %>
            </div>
        </div>

        <div class="middle-button-container">
            <a class="button blue middle save_addresses" href="javascript:void(0);">
                <%= CRMCommonResource.Save %>
            </a>
        </div>



    <div id="invoiceItemActionProgress" class="display-none">       
            <%= CRMCommonResource.SaveChangesProggress%>       
    </div>
</div>