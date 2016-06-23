<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipQuick.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.VoipQuick" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<script id="existing-number-tmpl" type="text/x-jquery-tmpl">
    <div id="enumber${id}" class="number" data-numberid="${id}">
        
        <div class="number-box with-entity-menu clearFix">
            <div class="cell switcher">
                <div class="expander-icon"></div>
            </div>

            <div class="cell title">
                ${number}<span class="support-level">${alias}</span>
            </div>
    
            <div class="cell outgoing-calls">
                <a class="on_off_button ${settings.allowOutgoingCalls ? "on" : "off"}" data-operator-btn-selector=".outgoing-calls"></a>
            </div>
            
            <!--
            <div class="cell voicemail">
                <a class="on_off_button ${settings.voiceMail && settings.voiceMail.enabled ? "on" : "off"}"></a>
            </div>
            -->
        
            <div class="cell recording-calls">
                <a class="on_off_button ${settings.record ? "on" : "off"}" data-operator-btn-selector=".recording-calls"></a>
            </div>
            
            <div class="cell actions entity-menu">
                <div class="studio-action-panel">
                    <ul class="dropdown-content">
                        <li class="dropdown-item">
                            <a href="settings.aspx?type=voip.numbers#${id}" target="_blank"><%= CRMVoipResource.EditBtn %></a>
                        </li>
                        <li class="dropdown-item"><%= CRMVoipResource.DeleteBtn %></li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="operators-box">
            <div class="add-operators-box">
                <span class="add-operators-btn link dotline plus"><%= CRMVoipResource.AddOperatorsBtn %></span>
            </div>
            {{tmpl($data) "#operators-tmpl"}}
        </div>
    </div>
</script>

<script id="operators-tmpl" type="text/x-jquery-tmpl">
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
        
        <!--<div class="cell voicemail"></div>-->
        
        <div class="cell recording-calls">
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

<script id="countries-list-tmpl" type="text/x-jquery-tmpl">
    <div class="studio-action-panel">
        <ul class="dropdown-content">
            {{each countries}}
            <li class="dropdown-item">
                <span class="voip-flag ${iso}" data-iso="${iso}" data-code="${code}"></span>${title}
            </li>
            {{/each}}
        </ul>
    </div>
</script>

<script id="available-number-tmpl" type="text/x-jquery-tmpl">
    <div id="anumber${number}" class="number">
        <input type="radio" name="buy-number-radio" data-number="${number}"/>
        <div class="number-value">${number}</div>
    </div>
</script>

<div id="voip-quick-view">
    <span id="show-buy-phone-popup-btn" class="link dotline plus"><%= CRMVoipResource.BuyNumberBtn %></span>
    
    <div id="buy-phone-popup">
        <sc:Container ID="buyNumberContainer" runat="server">
            <Header>
                <%= CRMVoipResource.BuyNumberHeader %>
            </Header>
            <Body>
                <!--<div class="header-base-small">Выберите тип номера</div>
                <select id="number-type-selector" class="comboBox">
                    <option>Toll-free</option>
                    <option>Другой</option>       
                </select>-->
                
                <div class="header-base-small"><%= CRMVoipResource.SearchLabel %></div>
                <div class="clearFix">
                    <div id="country-selector-box" class="clearFix">
                        <div id="country-selector" class="voip-flag arrow-down"></div>
                        <div id="country-code"></div>
                        <input id="country-input" type="text" />
                        <div id="country-input-clear-btn" class="clear-icon"></div>
                    </div>
                    <div id="country-selector-search-btn" class="search-icon"></div>
                </div>

                <div id="available-numbers-box">
                    <div id="available-numbers-loader"><%= CRMVoipResource.LoadingMsg %></div>
                    <div id="available-numbers-empty-msg"><%= CRMVoipResource.NoAvailableNumbersMsg %></div>
                    <div id="available-numbers-empty-search-msg"><%= CRMVoipResource.NoSearchingNumbersMsg %></div>
                    <div id="available-numbers"></div>
                </div>

                <a id="buy-phone-btn" class="button disable"><%= CRMVoipResource.BuyNumberBtn %></a>
                <a id="cancel-buy-phone-btn" class="button gray"><%= CRMVoipResource.CancelBtn %></a>
                <span id="buy-phone-loader" class="loader-middle"></span>
            </Body>
        </sc:Container>
    </div>
    
    <div id="existing-numbers-empty-box"><%= CRMVoipResource.NoExistingNumbersMsg %></div>
    <div id="existing-numbers-list">
        <div id="existing-numbers-header" class="number">
            <div class="number-box clearFix">
                <div class="cell title">
                    <%= CRMVoipResource.VirtualNumbersAndOperators %>
                </div>
    
                <div class="cell outgoing-calls">
                    <%= CRMVoipResource.OutgoingCalls %>
                </div>
                
                <!--
                <div class="cell voicemail">
                    <%= CRMVoipResource.Voicemail %>
                </div>
                -->
        
                <div class="cell recording-calls">
                    <%= CRMVoipResource.RecordingCalls %>
                </div>
            </div>
        </div>
    </div>
</div>