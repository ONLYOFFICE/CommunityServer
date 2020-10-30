<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CurrencySettingsView.ascx.cs"
    Inherits="ASC.Web.CRM.Controls.Settings.CurrencySettingsView" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div id="currencySettingsContent">
    <div class="clearFix">
        <div id="currencySettingsPannel" class="float-left">
            <div class="header-base-small headerPanelSmall"><%= CRMSettingResource.DefaultCurrency %>:</div>
            <select id="defaultCurrency" class="comboBox">
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
            <div class="header-base-small headerPanelSmall"><%= CRMSettingResource.CurrencyRate %>:</div>
            <select id="currencySelector" class="comboBox">
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
            <span class="splitter-buttons"></span>
            <a id="addCurrencyRate" class="button gray middle disable"><%= CRMSettingResource.AddCurrencyRate %></a>
            <div id="currencyRateList"></div>
        </div>
        <div class="settings-help-block">
            <p><%= String.Format(CRMSettingResource.CurrencySettingsHelp.HtmlEncode(), "<br/><br/>")%></p>
        </div>
    </div>
    <div class="middle-button-container">
        <a id="saveCurrencySettings" class="button blue middle">
            <%=CRMCommonResource.Save%>
        </a>
        <span class="splitter-buttons"></span>
        <a id="cancelCurrencySettings" class="button gray middle">
            <%=CRMCommonResource.Cancel%>
        </a>
    </div>
</div>