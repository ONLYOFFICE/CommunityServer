<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipCalls.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.VoipCalls" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.CRM.Core" %>

<div id="voip-calls-view">
    <div id="play-record-not-support-box" class="info-box excl">
        <div class="first-step">
            <div class="header-base medium bold">
                <%= CRMVoipResource.RecordingsCallsPlayNotSupportedMsg %>
            </div>
            <%= string.Format(CRMVoipResource.RecordingsCallsPlayNotSupportedTipMsg, "<a href=\"http://html5test.com/compare/feature/audio-pcm.html\" class=\"link underline blue\" target=\"_blank\">", "</a>") %>.
        </div>
    </div>

    <table id="calls-list" class="table-list height32">
        <thead>
            <tr>
                <th class="call-type"><%= CRMVoipResource.CallType %></th>
                <th class="call-date"><%= CRMVoipResource.CallDatetime %></th>
                <th class="call-agent"><%= CRMVoipResource.CallAgent %></th>
                <th class="call-client"><%= CRMVoipResource.CallClient %></th>
                <th class="call-duration"><%= CRMVoipResource.CallDuration %></th>
                <% if (CRMSecurity.IsAdmin)
                   { %>
                <th class="call-cost">$</th>
                <% } %>
            </tr>
        </thead>
        <tbody>
        </tbody>
    </table>

    <div id="call-record-play-panel" class="studio-action-panel">
        <div id="call-record-play-panel-loader" class="loader16"></div>

        <div class="call-type-icon pause"></div>
        <div class="call-type-icon play"></div>

        <div id="call-record-play-panel-progress">
            <div id="call-record-play-panel-progress-percentage"></div>
        </div>

        <div class="clear-icon stop"></div>
        <div id="call-record-play-panel-timer">00:00</div>

        <audio id="call-record-player"></audio>
    </div>

    <asp:PlaceHolder ID="controlsHolder" runat="server"></asp:PlaceHolder>

    <div id="hiddenBlockForCallsContactSelector" style="display:none;">
        <span id="callsContactSelectorForFilter" class="custom-value">
            <span class="inner-text">
                <span class="value"><%= CRMCommonResource.Select %></span>
            </span>
        </span>
    </div>
</div>