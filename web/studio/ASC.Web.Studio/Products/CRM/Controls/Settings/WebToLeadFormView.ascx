<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebToLeadFormView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Settings.WebToLeadFormView" %>

<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Users" %>

<div>
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td>
                <img src="<%= WebImageSupplier.GetAbsoluteWebPath("web_to_leads.png", ProductEntryPoint.ID)%>" />
            </td>
            <td style="padding-left:8px;">
                <%= CRMSettingResource.WebToLeadsFormHeader%>
            </td>
        </tr>
    </table>
</div>
<div class="settingsHeaderWithDigit">
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td class="digits">
                1
            </td>
            <td style="padding-top: 8px;">
                <span class="header-base">
                    <%= CRMSettingResource.FormProperties %>
                </span>
                <div class="describe-text">
                    <%= CRMSettingResource.FormPropertiesDescription %>
                </div>
            </td>
        </tr>
    </table>
</div>
<div>
    <div id="properties_url_panel" class="requiredField">
        <span class="requiredErrorText">
            <%= CRMSettingResource.EmptyField %>
        </span>
        <div>
            <b><%= CRMSettingResource.ReturnURL %>:</b>
            <span class="crm-requiredField">*</span>
        </div>
        <div style="margin: 5px 0">
            <input type="text" maxlength="255" value="" style="width: 100%;" id="returnURL" name="returnURL" class="textEdit">
        </div>
        <div class="describe-text">
            <%= CRMSettingResource.ReturnURLDescription %>
        </div>
    </div>
    <br />
    <div id="properties_webFormKey" class="requiredField">
        <span class="requiredErrorText">
            <%= CRMSettingResource.EmptyField %></span>
        <div>
            <b><%= CRMSettingResource.WebFormKey%>:</b>
            <span class="crm-requiredField">*</span>
        </div>
        <div style="margin: 5px 0">
            <input type="hidden" value="<%= _webFormKey %>" />
            <div class="clearFix">
                <div id="webFormKeyContainer" style="float: left;"><%=_webFormKey %></div>
                <label class="refreshBtn" title="<%= CRMCommonResource.Change %>" onclick="ASC.CRM.SettingsPage.WebToLeadFormView.changeWebFormKey();" ></label>
            </div>
        </div>
        <div class="describe-text">
            <%= CRMSettingResource.WebFormKeyDescription %>
        </div>
    </div>
</div>
<div class="settingsHeaderWithDigit">
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td class="digits">
                2
            </td>
            <td style="padding-top: 8px;">
                <span class="header-base">
                    <%= CRMSettingResource.FieldsSelection %>
                </span>
                <div class="describe-text">
                    <%= CRMSettingResource.FieldsSelectionDescription %>
                </div>
                <div style="margin-top: 6px;">
                    <input type="radio" id="radioCompany" value="company" name="radio" style="margin-left: 0px;"
                        onchange="ASC.CRM.SettingsPage.WebToLeadFormView.changeContactType()"/>
                    <label for="radioCompany"><%= CRMContactResource.Company %></label>
                    <input type="radio" id="radioPerson" value="person" name="radio" checked="checked"
                        onchange="ASC.CRM.SettingsPage.WebToLeadFormView.changeContactType()"/>
                    <label for="radioPerson"><%= CRMContactResource.Person %></label>
                </div>
            </td>
        </tr>
    </table>
</div>
<div>
    <div style="margin: 7px;">
        <b><%= CRMSettingResource.FieldList%>:</b>
    </div>
    <table width="100%" id="tblFieldList">
        <tbody>
        </tbody>
    </table>
</div>
<div class="settingsHeaderWithDigit">
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td class="digits">
                3
            </td>
            <td style="padding-top: 8px;">
                <span class="header-base">
                    <%= CRMSettingResource.AccessRightsAndTags%>
                </span>
                <div class="describe-text">
                    <%= CRMSettingResource.AccessRightsAndTagsDescription%>
                </div>
            </td>
        </tr>
    </table>
</div>
<div>
    <div id="wtlfAccessRights"></div>
      <div style="margin-top:23px;" id="wtlfMakePublicPanel">
        <input id="isPublicContact" type="hidden" value="" name="isPublicContact" />
    </div>

    <div style="margin: 18px 0 10px 0; font-weight: bold"><%=CRMCommonResource.Tags%>:</div>
    <div id="wtlfTags"></div>
</div>

<div class="settingsHeaderWithDigit">
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td class="digits">
                4
            </td>
            <td style="padding-top: 8px;">
                <span class="header-base">
                    <%= CustomNamingPeople.Substitute<CRMSettingResource>("NotifyUsers").HtmlEncode() %>
                </span>
                <div class="describe-text">
                    <%= CustomNamingPeople.Substitute<CRMSettingResource>("NotifyUsersDescription").HtmlEncode() %>
                </div>
            </td>
        </tr>
    </table>
</div>
<div id="userSelectorListViewContainer">
</div>

<div class="middle-button-container">
    <a class="button blue middle" onclick="javascript: ASC.CRM.SettingsPage.WebToLeadFormView.generateSampleForm();">
        <%= CRMSettingResource.GenerateForm %>
    </a>
</div>
<div id="resultContainer" class="panelSplitter" style="display: none;">
    <br/><div class="panelSplitter">
        <div class="header-base">
            <%= CRMSettingResource.FormCode %>
        </div>
        <div class="describe-text">
            <%= CRMSettingResource.FormCodeDescription%>
        </div>
    </div>
    <textarea onclick="this.select()" style="width: 100%; resize: none;" rows="10"></textarea>
</div>
<div class="panelSplitter" id="previewHeader" style="display: none;">
    <br/><div class="header-base">
        <%= CRMSettingResource.FormPreview%>
    </div>
    <div class="describe-text">
        <%= CRMSettingResource.FormPreviewDescription%>
    </div>
    <br />
    <div class="content">
    </div>
</div>