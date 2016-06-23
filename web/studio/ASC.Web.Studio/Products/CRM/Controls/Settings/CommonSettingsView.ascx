<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommonSettingsView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Settings.CommonSettingsView" %>
<%@ Import Namespace="ASC.Web.CRM.Configuration" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<div class="header-base settingsHeader" style="margin-top:5px;"><%= CRMSettingResource.SMTPSettings%></div>
<div id="smtpSettingsContent">
    <p><%: CRMSettingResource.SMTPSettingsDescription%></p>
    <div class="clearFix">
        <div id="SMTPSettingsPannel" class="float-left">
        </div>
        <div class="settings-help-block">
            <p><%= String.Format(CRMSettingResource.SMTPSettingsHelp.HtmlEncode(), "<br/><br/>")%></p>
            <!--<a href="<%= CommonLinkUtility.GetHelpLink(true) + "administratorguides/mass-mailing.aspx" %>" target="_blank">
                <%=CRMSettingResource.LearnMore%>
            </a>-->
        </div>
    </div>
    <div style="margin: 23px 0 0 1px;">
        <a href="javascript:void(0);" onclick="ASC.CRM.SettingsPage.saveSMTPSettings();" class="button blue middle">
            <%=CRMCommonResource.Save%>
        </a>
        <span class="splitter-buttons"></span>
        <a id="showSendTestMailPanelBtn" href="javascript:void(0);" onclick="ASC.CRM.SettingsPage.showSendTestMailPanel();" class="button gray middle disable">
            <%=CRMSettingResource.SendTestMail%>
        </a>
    </div>
</div>

<div class="header-base settingsHeader"><%=CRMSettingResource.CurrencySettings%></div>
<div>
    <p><%: CRMSettingResource.CurrencySettingsDescription%></p>
    <p><%: CRMSettingResource.CurrencySettingsInvoiceDescription%></p>
    <div class="header-base-small headerTitle">
        <%= CRMSettingResource.DefaultCurrency%>:
    </div>
    <div>
        <select id="defaultCurrency" name="defaultCurrency" onchange="ASC.CRM.SettingsPage.changeDefaultCurrency()" class="comboBox">
            <optgroup label="<%= CRMCommonResource.Currency_Basic %>">
                <% foreach (var keyValuePair in BasicCurrencyRates)%>
                <% { %>
                <option value="<%=keyValuePair.Abbreviation%>" <%=IsSelectedBidCurrency(keyValuePair.Abbreviation) ? "selected=selected" : String.Empty%>>
                    <%=String.Format("{0} - {1}", keyValuePair.Abbreviation, keyValuePair.Title)%></option>
                <% } %>
            </optgroup>
            <optgroup label="<%= CRMCommonResource.Currency_Other %>">
                <% foreach (var keyValuePair in OtherCurrencyRates)%>
                <% { %>
                <option value="<%=keyValuePair.Abbreviation%>" <%=IsSelectedBidCurrency(keyValuePair.Abbreviation) ? "selected=selected" : String.Empty%>>
                    <%=String.Format("{0} - {1}", keyValuePair.Abbreviation, keyValuePair.Title)%></option>
                <% } %>
            </optgroup>
        </select>
        <img style="display:none;" src="<%= WebImageSupplier.GetAbsoluteWebPath("loader_12.gif") %>" width="12px" height="12px"
            alt="<%= CRMCommonResource.LoadingWait %>"/>
        <span style="display:none;" class="text-medium-describe"><%= CRMSettingResource.SaveCompleted%></span>
    </div>
</div>

<div class="header-base settingsHeader"><%= CRMSettingResource.ExportData %></div>
<div id="exportDataContent">
    <table width="100%" cellpadding="0" cellspacing="0">
        <colgroup>
            <col style="width: 100px;"/>
            <col/>
        </colgroup>
        <tbody>
            <tr valign="top">
                <td>
                    <img src="<%= WebImageSupplier.GetAbsoluteWebPath("export_data.png", ProductEntryPoint.ID) %>"
                        alt="<%= CRMSettingResource.ExportData %>" />
                </td>
                <td>
                    <p style="margin-top: 5px;">
                        <%= CRMSettingResource.ExportDataSettingsInfo %>
                    </p>
                    <a class="button blue middle" onclick="ASC.CRM.SettingsPage.startExportData()">
                        <%= CRMSettingResource.DownloadAllDataInZipArchive %>
                    </a>
                    <div class="clearFix progress-container" style="display:none;">
                        <div class="percent">0%</div>
                        <div class="progress-wrapper">
                            <div class="progress" style="width: 0%"></div>
                        </div>
                    </div>
                    <p style="display: none;" class="header-base-small">
                        <%= CRMSettingResource.DownloadingAllDataInZipArchive %>
                    </p>
                    <div id="exportErrorBox" class="clearFix" style="margin-top:10px; display:none;">
                        <div style="float:left"><%= CRMContactResource.MassSendErrors %>:</div>
                        <div class="progressErrorBox" style="float: left; margin-left: 10px;"></div>
                    </div>
                    <div id="exportLinkBox" class="clearFix" style="margin-top:10px; display:none;">
                        <div style="float:left"><%= CRMSettingResource.DownloadLinkText %>:</div>
                        <span style="float: left; margin-left: 10px;"></span>
                    </div>
                    <div class="middle-button-container" style="display:none;">
                        <a class="button gray middle" onclick="ASC.CRM.SettingsPage.abortExport()" id="abortButton">
                            <%= CRMSettingResource.AbortExport %>
                        </a>
                        <a class="button gray middle" onclick="ASC.CRM.SettingsPage.closeExportProgressPanel()" id="okButton" style="display: none;">
                            <%= CRMCommonResource.OK %>
                        </a>
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</div>


<div id="sendTestMailPanelBody" style="display: none;">
    <table class="testMailFieldsTable" cellpadding="0" cellspacing="0">
        <colgroup>
            <col style="width: 1%;"/>
            <col style="width: 20px;"/>
            <col/>
        </colgroup>
        <tbody>
            <tr>
                <td><span class="bold"><%= CRMCommonResource.MailFrom%>:</span></td>
                <td></td>
                <td><span class="testMailFromLabel"></span></td>
            </tr>
            <tr>
                <td>
                    <div class="requiredField">
                    <div class="headerPanelSmall bold">
                        <%= CRMCommonResource.MailTo%>:
                    </div>
                    </div>
                </td>
                <td></td>
                <td><input type="text" class="textEdit testMailToField" /></td>
            </tr>
            <tr>
                <td><span class="bold"><%= CRMCommonResource.MailSubject%>:</span></td>
                <td></td>
                <td><input type="text" class="textEdit testMailSubjectField" /></td>
            </tr>
        </tbody>
    </table>

    <div class="headerPanelSmall-splitter requiredField">
        <div class="headerPanelSmall headerPanelSmall-splitter bold">
            <%= CRMCommonResource.MailBody%>:
        </div>
        <textarea cols="10" rows="6" class="testMailBodyField"></textarea>
    </div>
</div>