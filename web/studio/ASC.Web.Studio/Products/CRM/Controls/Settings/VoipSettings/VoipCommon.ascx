<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipCommon.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.VoipCommon" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<script id="header-tmpl" type="text/x-jquery-tmpl">
    {{if $data > 0}}
    <div>
        <%= CRMVoipResource.NumbersCountMsg %>: <strong>${$data}</strong>. 
        <%= string.Format(CRMVoipResource.NumbersCountDscrMsg, "<a href=\"settings.aspx?type=voip.numbers\" class=\"link underline blue\" target=\"_blank\">", "</a>") %>
    </div>
    {{else}}
     <div>
         <%= string.Format(CRMVoipResource.NoNumbersMsg, "<a href=\"settings.aspx?type=voip.numbers\" class=\"link underline blue\" target=\"_blank\">", "</a>") %>
    </div>
    {{/if}}
</script>

<script id="settings-tmpl" type="text/x-jquery-tmpl">
    <div class="common-setting-item">
        <div class="header-base-small"><%= CRMVoipResource.IncomingCallsQueueSize %></div>
        <select id="queue-size-selector" class="comboBox">
            <option value="5" {{if queue.size == 5}} selected="selected" {{/if}}>5</option>
            <option value="10" {{if queue.size == 10}} selected="selected" {{/if}}>10</option>
            <option value="15" {{if queue.size == 15}} selected="selected" {{/if}}>15</option>
        </select>
    </div>
            
    <div class="common-setting-item">
        <div class="header-base-small"><%= CRMVoipResource.WaitingTimeout %></div>
        <select id="queue-wait-time-selector" class="comboBox">
            <option value="5" {{if queue.waitTime == 5}} selected="selected" {{/if}}><%= CRMVoipResource.WaitingTimeout5Minutes %></option>
            <option value="10" {{if queue.waitTime == 10}} selected="selected" {{/if}}><%= CRMVoipResource.WaitingTimeout10Minutes %></option>
            <option value="15" {{if queue.waitTime == 15}} selected="selected" {{/if}}><%= CRMVoipResource.WaitingTimeout15Minutes %></option>
        </select>
    </div>
    
    <div class="common-setting-item">
        <div class="header-base-small"><%= CRMVoipResource.OperatorPause %></div>
        <select id="operator-pause-selector" class="comboBox">
            <option value="0" {{if !pause}} selected="selected" {{/if}}><%= CRMVoipResource.OperatorPauseDisabled %></option>
            <option value="1" {{if pause}} selected="selected" {{/if}}><%= CRMVoipResource.OperatorPauseEnabled %></option>
        </select>
    </div>
</script>

<script id="ringtones-tmpl" type="text/x-jquery-tmpl">
    <div id="ringtone-group-${audioType}" class="ringtone-group" data-audiotype="${audioType}">
        <div class="ringtone-group-box with-entity-menu clearFix">
            <div class="cell switcher">
                <div class="expander-icon"></div>
            </div>   
            <div class="cell title">
                ${name}
            </div>
            <div class="cell actions entity-menu">
                <div class="studio-action-panel">
                    <ul class="dropdown-content">
                        <li id="add-ringtone-${audioType}-btn" class="dropdown-item"><%= CRMVoipResource.AddRingtoneBtn %></li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="ringtones-box">
            {{tmpl(ringtones) "#ringtone-tmpl"}}
        </div>
    </div>
</script>

<script id="ringtone-tmpl" type="text/x-jquery-tmpl">
    <div class="ringtone with-entity-menu clearFix" data-filename="${name}">
        <div class="cell title">
            <div class="loader16"></div>
            <button class="button gray btn-icon ringtone-play-btn __play" data-path="${path}"></button>
            ${name}
        </div>
        <div class="cell actions entity-menu">
            <div class="studio-action-panel">
                <ul class="dropdown-content">
                    <li class="dropdown-item delete-ringtone-btn"><%= CRMVoipResource.DeleteRingtoneBtn %></li>
                </ul>
            </div>
        </div>
    </div>
</script>

<div id="voip-common-view">
    <div id="header-settings-box" class="clearFix">
        <div class="settings-block">
            <div class="header-base"><%= CRMVoipResource.VirtualNumbers %></div>
            <div id="header-message"></div>
        </div>
        <div class="settings-help-block">
            <p><%= string.Format(CRMVoipResource.VirtualNumbersDscrMsg, "<strong>", "</strong>") %></p>
        </div>
    </div>
    
    <div id="settings-box" class="clearFix">
        <div class="settings-block">
            <div class="header-base"><%= CRMVoipResource.GeneralSettings %></div>
            <div id="settings-list"></div>
        </div>
        <div class="settings-help-block">
            <p><%= string.Format(CRMVoipResource.IncomingCallsQueueDscrMsg, "<strong>", "</strong>") %></p>
            <p><%= string.Format(CRMVoipResource.WaitingTimeoutDscrMsg, "<strong>", "</strong>") %></p>
            <p><%= string.Format(CRMVoipResource.OperatorPauseDscrMsg, "<strong>", "</strong>") %></p>
        </div>
    </div>
    
    <div id="ringtones-box" class="clearFix">
        <audio id="ringtone-player"></audio>
        <div class="settings-block">
            <div class="header-base"><%= CRMVoipResource.RingtonesTuning %></div>
            <div id="ringtones-list">
                <div class="ringtone-group header">
                    <div class="ringtone-group-box clearFix">
                        <div class="cell">
                            <%= CRMVoipResource.RingtonesList %>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="settings-help-block">
            <p>
                <%= string.Format(CRMVoipResource.RingtonesUploadingRecomendationsMsg, "<strong>", "</strong>") %>
            </p>
        </div>
    </div>
</div>