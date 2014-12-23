<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipNumbers.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.VoipNumbers" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<asp:PlaceHolder ID="quickSettingsHolder" runat="server"></asp:PlaceHolder>

<script id="number-selector-option-tmpl" type="text/x-jquery-tmpl">
    <option value="${id}">${number}</option>
</script>

<script id="settings-tmpl" type="text/x-jquery-tmpl">
    <div id="alias-setting-box" class="clearFix">
        <div class="settings-block">
            <div class="header-base"><%= CRMVoipResource.Alias %></div>
            <input type="text" value="${alias}" id="number-alias-input" class="textEdit" placeholder="<%= CRMVoipResource.AliasPlaceholder %>"/>
        </div>
        <div class="settings-help-block">
            <p><%= string.Format(CRMVoipResource.AliasDscrMsg, "<strong>", "</strong>") %></p>
        </div>
    </div>
    
    <div class="clearFix">
        <div id="quick-setting" class="settings-block">
            <div class="header-base"><%= CRMVoipResource.QuickTuning %></div>
            <div class="quick-setting-item">
                <a id="outgoing-calls-setting-btn" class="on_off_button ${settings.allowOutgoingCalls ? "on" : "off"}"></a>
                <%= CRMVoipResource.OutgoingCalls %>
            </div>
            <!--<div class="quick-setting-item">
                <a id="voicemail-setting-btn" class="on_off_button ${settings.voiceMail && settings.voiceMail.enabled ? "on" : "off"}"></a>
                <%= CRMVoipResource.Voicemail %>
            </div>-->
            <div class="quick-setting-item">
                <a id="record-incoming-setting-btn" class="on_off_button ${settings.record ? "on" : "off"}"></a>
                <%= CRMVoipResource.RecordingCalls %>
            </div>
            <div class="quick-setting-item">
                <a id="working-hours-setting-btn" class="on_off_button ${settings.workingHours && settings.workingHours.enabled ? "on" : "off"}"></a>
                <%= CRMVoipResource.WorkingHours %>
                <div id="working-hours-inputs-box">
                    <div>
                        <input type="text" class="textEdit" id="working-hours-from-input" value="${settings.workingHours ? settings.workingHours.from.slice(0, 5) : ""}" /> -
                        <input type="text" class="textEdit" id="working-hours-to-input" value="${settings.workingHours ? settings.workingHours.to.slice(0, 5) : ""}"/>
                    </div>
                    <span id="working-hours-invalid-format-error" class="working-hours-error"><%= CRMVoipResource.WorkingHoursFormatTip %></span>
                    <span id="working-hours-invalid-interval-error" class="working-hours-error"><%= CRMVoipResource.WorkingHoursFormatErrorMsg %></span>
                </div>
            </div>
        </div>
        <div class="settings-help-block">
            <p><%= string.Format(CRMVoipResource.QuickTuningDscrMsg, "<strong>", "</strong>") %></p>
        </div>
    </div>
    
    <div id="ringtones-setting-box" class="clearFix">
        <div  class="settings-block">
            <div class="header-base"><%= CRMVoipResource.RingtonesTuning %></div>
            
            <div class="ringtone-setting-item">
                <div class="header-base-small">
                    <%= CRMVoipResource.GreetingRingtones %>
                </div>
                <select id="greeting-ringtone-selector" class="comboBox ringtone-selector">
                    <option value="" {{if settings.greetingAudio === ""}} selected="selected" {{/if}}><%= CRMVoipResource.UnspecifiedOption %></option>
                    {{each settings.ringtones.greeting}}
                    <option value="${path}" {{if settings.greetingAudio === path}} selected="selected" {{/if}}>${name}</option>
                    {{/each}}
                </select>

                <button class="button gray btn-icon ringtone-play-btn __play {{if !settings.greetingAudio}} disable {{/if}}" id="greeting-ringtone-play-btn" data-src="${settings.greetingAudio}" data-type="greeting"></button>
                <button class="button gray btn-icon __upload" id="greeting-ringtone-load-btn" onclick=" return false; "></button>
            </div>
            
            <div class="ringtone-setting-item">
                <div class="header-base-small">
                    <%= CRMVoipResource.QueueRingtones %>
                </div>
                <select id="queue-wait-ringtone-selector" class="comboBox ringtone-selector">
                    {{each settings.ringtones.queue}}
                    <option value="${path}" {{if settings.queue && settings.queue.waitUrl === path}} selected="selected" {{/if}}>${name}</option>
                    {{/each}}
                </select>

                <button class="button gray btn-icon ringtone-play-btn __play" id="queue-wait-ringtone-play-btn" data-src="settings.queue.waitUrl" data-type="queue"></button>
                <button class="button gray btn-icon __upload" id="queue-wait-ringtone-load-btn" onclick=" return false; "></button>
            </div>
        
            <!--<div class="ringtone-setting-item">
                <div class="header-base-small">
                    <%= CRMVoipResource.WaitingRingtones %>
                </div>
                <select id="hold-ringtone-selector" class="comboBox ringtone-selector">
                    <option value="" {{if settings.holdAudio === ""}} selected="selected" {{/if}}><%= CRMVoipResource.UnspecifiedOption %></option>
                    {{each settings.ringtones.hold}}
                    <option value="${path}" {{if settings.holdAudio === path}} selected="selected" {{/if}}>${name}</option>
                    {{/each}}
                </select>

                <button class="button gray btn-icon ringtone-play-btn __play {{if !settings.holdAudio }} disable {{/if}}" id="hold-ringtone-play-btn" data-src="${settings.holdAudio}" data-type="hold"></button>
                <button class="button gray btn-icon __upload" id="hold-ringtone-load-btn" onclick=" return false; "></button>
            </div>-->
    
            <div class="ringtone-setting-item">
                <div class="header-base-small">
                    <%= CRMVoipResource.VoicemailRingtones %>
                </div>
                <select id="voicemail-ringtone-selector" class="comboBox ringtone-selector">
                    {{each settings.ringtones.voicemail}}
                    <option value="${path}" {{if settings.voiceMail && settings.voiceMail.url === path}} selected="selected" {{/if}}>${name}</option>
                    {{/each}}
                </select>

                <button class="button gray btn-icon ringtone-play-btn __play" id="voicemail-ringtone-play-btn" data-src="settings.voiceMail.url" data-type="voice"></button>
                <button class="button gray btn-icon __upload" id="voicemail-ringtone-load-btn" onclick=" return false; "></button>
            </div>
        </div>
        <div class="settings-help-block">
            <p><%= string.Format(CRMVoipResource.RingtonesTuningDscrMsg, "<strong>", "</strong>") %></p>
        </div>
    </div>
</script>

<script id="operator-tmpl" type="text/x-jquery-tmpl">
    {{each settings.operators}}
    <div class="operator with-entity-menu" data-operatorid="${id}">
        <div class="cell code"></div>    
        
        <div class="cell title">
            <img src="${userInfo.avatarSmall}" alt="${userInfo.displayName}"/>
            ${userInfo.displayName}
        </div>
        
        <div class="cell outgoing-calls">
            <a class="on_off_button ${allowOutgoingCalls ? "on" : "off"} ${$data.settings.allowOutgoingCalls ? "" : "disable"}"></a>
        </div>
        
        <div class="cell incoming-recording">
            <a class="on_off_button ${record ? "on" : "off"} ${$data.settings.record ? "" : "disable"}"></a>
        </div>
        
        <div class="cell actions entity-menu">
            <div class="studio-action-panel">
                <ul class="dropdown-content">
                    <li class="dropdown-item delete-operator-btn"><%= CRMVoipResource.DeleteBtn %></li>
                </ul>
            </div>
        </div>
    </div>
    {{/each}}
</script>

<script id="ringtone-selector-option-tmpl" type="text/x-jquery-tmpl">
    <option value="${path}">${name}</option>
</script>

<div id="voip-numbers-view">
    <div id="empty-numbers-list-msg">
        <%= string.Format(CRMVoipResource.NoNumbersMsg, "<a href=\"settings.aspx?type=voip.numbers\" class=\"link underline blue\" target=\"_blank\">", "</a>") %>
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
        <a id="delete-number-btn" class="button gray big"><%= CRMVoipResource.DeleteNumberBtn %></a>
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