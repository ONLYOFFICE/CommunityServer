<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipNumbers.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.VoipNumbers" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<asp:PlaceHolder ID="quickSettingsHolder" runat="server"></asp:PlaceHolder>

<div id="voip-numbers-view">
    <div id="empty-numbers-list-msg">
        <%= string.Format(CRMVoipResource.NoNumbersMsg, "<a href=\"Settings.aspx?type=voip.numbers\" class=\"link underline blue\" target=\"_blank\">", "</a>") %>
    </div>
    
    <div id="number-selector-box" class="clearFix">
        <div class="settings-block">
            <div class="header-base"><%= CRMVoipResource.TuningNumber %></div>
            <select id="number-selector" class="comboBox"></select>
        </div>
    </div>
    
    <div class="voip-divider"></div>
    <audio id="ringtone-player" class="ringtone-player"></audio>
    <div id="number-settings-box"></div>
    <div class="voip-divider"></div>
    
    <div id="buttons-box">
        <a id="save-settings-btn" class="button blue big"><%= CRMVoipResource.UpdateSettingsBtn %></a>
        <a id="show-remove-number-btn" class="button gray big"><%= CRMVoipResource.DeleteNumberBtn %></a>
    </div>
    
    <div id="operators-box">
        <div class="header-base">
            <%= CRMVoipResource.NumberOperators %>
        </div>
    
        <span id="add-operators-btn" class="link dotline plus"><%= CRMVoipResource.AddOperatorBtn %></span>
        <div id="operators-list">
            <div id="operators-list-header" class="operator">
                <div class="cell code"></div>    
                <div class="cell title"><%= CRMVoipResource.Operator %></div>
                <div class="cell outgoing-calls"><%= CRMVoipResource.OutgoingCalls %></div>
                <div class="cell incoming-recording"><%= CRMVoipResource.RecordingCalls %></div>
                <div class="cell actions"></div>
            </div>
            <div id="operators-list-body"></div>
        </div>
    </div>
</div>