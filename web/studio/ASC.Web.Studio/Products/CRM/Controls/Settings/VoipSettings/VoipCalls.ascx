<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipCalls.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.VoipCalls" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div id="voip-calls-view">
    <script id="call-tmpl" type="text/x-jquery-tmpl">    
        {{if history.length}}
        <tr class="call-row">
            <td class="call-type">
                <span class="call-type-icon ${typeClass}"></span>
                {{if history[0].supportsPlaying && history[0].recordUrl}}
                <span class="call-type-icon play" data-recordUrl="${history[0].recordUrl}"></span>
                {{/if}}
            </td>
            <td class="call-date"><span>${datetime}</span></td>
            <td class="call-agent">
                ${history[0].agent && history[0].agent.displayName}
                {{if history.length > 1}}
                <span class="toggle-redirections-btn">+${history.length - 1}</span>
                {{/if}}
            </td>
            <td class="call-client">${contactTitle}</td>
            <td class="call-duration">${history[0].durationString}</td>
            <td class="call-cost">${history[0].cost}</td>
        </tr>
        {{/if}}
        {{if history.length > 1}}
        {{each history.slice(1)}}
        <tr class="redirection-row">
            <td class="call-type">
                <span class="call-type-icon hide"></span>
                {{if supportsPlaying && recordUrl}}
                <span class="call-type-icon play" data-recordUrl="${recordUrl}"></span>
                {{/if}}
            </td>
            <td class="call-date"></td>
            <td class="call-agent">${agent && agent.displayName}</td>
            <td class="call-client"></td>
            <td class="call-duration">${durationString}</td>
            <td class="call-cost">${cost}</td>
        </tr>
        {{/each}}
        {{/if}}
    </script>
    
    <script id="call-view-tmpl" type="text/x-jquery-tmpl">
        <div class="header-with-menu">
            <a href="settings.aspx?type=voip.calls" class="header-back-link"></a>
            <span><%= CRMVoipResource.Call %> ${'#' + id}</span>
        </div>
        <div id="call-info-box">
            <div class="call-info-row">
                <span class="call-info-title"><%= CRMVoipResource.CallType %>:</span>
                <span class="call-info-value">${typeString}</span>
            </div>
            <div class="call-info-row">
                <span class="call-info-title"><%= CRMVoipResource.CallDatetime %>:</span>
                <span class="call-info-value">${datetime}</span>
            </div>
            <div class="call-info-row">
                <span class="call-info-title"><%= CRMVoipResource.CallClient %>:</span>
                <span class="call-info-value">${contact.displayName}</span>
            </div>
            <div class="call-info-row">
                <span class="call-info-title"><%= CRMVoipResource.CallDuration %>:</span>
                <span class="call-info-value">${durationString}</span>
            </div>
            <div class="call-info-row">
                <span class="call-info-title"><%= CRMVoipResource.CallCost %>:</span>
                <span class="call-info-value">${cost}$</span>
            </div>
        </div>
        
        <table id="call-redirections-list" class="table-list height32">
            <thead>
                <tr>
                    <th class="call-redirection-agent"><%= CRMVoipResource.CallAgent %></th>
                    <th class="call-redirection-waiting-time"><%= CRMVoipResource.CallWaitingTime %></th>
                    <th class="call-redirection-duration"><%= CRMVoipResource.CallDuration %></th>
                </tr>
            </thead>
            <tbody>
                {{each history}}
                <tr>
                    <td class="call-redirection-agent">${agent && agent.displayName}</td>
                    <td class="call-redirection-waiting-time">${waitingTimeString}</td>
                    <td class="call-redirection-duration">${durationString}</td>
                </tr>
                {{/each}}
            </tbody>
        </table>
    </script>

    <div id="play-record-not-support-box" class="info-box excl">
        <div class="first-step">
            <div class="header-base medium bold">
                <%= CRMVoipResource.RecordingsCallsPlayNotSupportedMsg %>
            </div>
            <%= string.Format(CRMVoipResource.RecordingsCallsPlayNotSupportedTipMsg, "<a href=\"http://html5test.com/compare/feature/audio-pcm.html\" class=\"link underline blue\" target=\"_blank\">", "</a>") %>.
        </div>
    </div>
    
    <div id="calls-filter"></div>
    <table id="calls-list" class="table-list height32">
        <thead>
            <tr>
                <th class="call-type"><%= CRMVoipResource.CallType %></th>
                <th class="call-date"><%= CRMVoipResource.CallDatetime %></th>
                <th class="call-agent"><%= CRMVoipResource.CallAgent %></th>
                <th class="call-client"><%= CRMVoipResource.CallClient %></th>
                <th class="call-duration"><%= CRMVoipResource.CallDuration %></th>
                <th class="call-cost">$</th>
            </tr>
        </thead>
        <tbody>
        </tbody>
    </table>
    
    <table id="calls-paging">
        <tbody>
            <tr>
                <td>
                    <div id="calls-paging-box"></div>
                </td>
                <td id="calls-paging-stat-box">
                    <span><%= CRMCommonResource.Total %>: </span>
                    <span id="calls-paging-items-count"></span>
                    <span><%= CRMCommonResource.ShowOnPage %>: </span>
                    <select id="calls-paging-page-count">
                        <option value="10">10</option>
                        <option value="20">20</option>
                        <option value="25">25</option>
                        <option value="30">30</option>
                        <option value="40">40</option>
                        <option value="50">50</option>
                        <option value="75">75</option>
                        <option value="100">100</option>
                    </select> 
                </td>
            </tr>
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
</div>