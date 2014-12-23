<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipPhoneControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.VoipPhoneControl" %>
<%@ Import Namespace="Resources" %>
<script type="text/javascript" src="//static.twilio.com/libs/twiliojs/1.2/twilio.min.js"></script>
<div id="voip-phone-view">
    
    <script id="operator-status-switcher-options-tmpl" type="text/x-jquery-tmpl">  
        <ul class="dropdown-content ">
            <li id="operator-online-btn" class="dropdown-item">${ASC.Resources.Master.Resource.OnlineStatus}</li>
            <li id="operator-offline-btn" class="dropdown-item">${ASC.Resources.Master.Resource.OfflineStatus}</li>
        </ul>
    </script>

    <script id="options-box-tmpl" type="text/x-jquery-tmpl">
        <select id="call-type-selector" class="comboBox">
            <option value="2" class="browser-type-call" {{if answer == 2}} selected="selected" {{/if}}><%= Resource.Browser %></option>
            {{each phones}}
            <option value="0" class="phone-type-call" data-number="${title}" {{if answer == 0 && redirectToNumber == title}} selected="selected" {{/if}}>${title}</option>
            {{/each}}
        </select>
        <div id="toggle-phone-box-btn">
            <div class="voip-tastatura"></div>
        </div>
    </script>
        
    <script id="countries-panel-tmpl" type="text/x-jquery-tmpl">
        <div id="countries-panel" class="studio-action-panel">
            <ul class="dropdown-content">
                {{each countries}}
                <li class="dropdown-item">
                    <span class="voip-flag ${iso}" data-iso="${iso}" data-code="${code}"></span>${title}
                </li>
                {{/each}}
            </ul>
        </div>
    </script>
        
    <script id="contact-phone-switcher-item-tmpl" type="text/x-jquery-tmpl">
        <li class="dropdown-item">
            <div class="data">${data}</div>
            <div class="category">${categoryName}</div>
        </li>
    </script>
        
    <script id="missed-call-tmpl" type="text/x-jquery-tmpl">
        <div class="missed-call" data-number="${lastCall.from}">
            <div class="missed-call-avatar">
                <a href="${'/products/crm/default.aspx?id=' + lastCall.contact.id}" target="_blank">
                    <img src="${lastCall.contact.smallFotoUrl}" alt="${lastCall.contact.displayName}"/>
                </a>
                {{if callsCount > 1}}
                <div class="missed-call-group-count">${callsCount}</div>
                {{/if}}
            </div>
            <div class="missed-call-contact">
                {{if lastCall.contact.displayName}}
                <a href="${'/products/crm/default.aspx?id=' + lastCall.contact.id}" target="_blank" class="missed-call-name" title="${lastCall.contact.displayName}">
                    ${lastCall.contact.displayName}
                </a>
                <div class="missed-call-number">${lastCall.from}</div>
                {{else}}
                <div class="missed-call-number-single">${lastCall.from}</div>
                {{/if}}
            
            </div>
            <div class="recall-btn" data-number="${lastCall.from}">
                <div></div>
            </div>
            <div class="missed-call-date">${lastCall.time}</div>
            <div class="missed-call-icon"></div>
        </div>
    </script>
        
    <script id="operators-redirect-option-tmpl" type="text/x-jquery-tmpl">
        <option value="${id}">${displayName}</option>
    </script>

    <script id="call-tmpl" type="text/x-jquery-tmpl">
        <div id="call-avatar">
            <a href="${'/products/crm/default.aspx?id=' + contact.id}" target="_blank">
                <img src="${contact.mediumFotoUrl}" alt="${contact.displayName}"/>
            </a>
        </div>    
        <div id="call-dude">
            <a href="${'/products/crm/default.aspx?id=' + contact.id}" target="_blank" id="dude-name">
                ${contact.displayName}
            </a>
            <div id="dude-company"></div>
        </div>
        <div id="call-number">
            <div id="number-value">${to}</div>
            <div id="number-location"></div>
        </div>
    </script>
    
    <div id="service-unavailable-box">
        <%= Resource.VoipServiceUnavailableMsg %>
    </div>
    
    <div id="view-init-loader-box" class="loader-text-block">
        <%= Resource.LoadingMsg %>
    </div>
    
    <audio id="incoming-player" loop></audio>
           
    <div id="operator-box" class="clearFix">
        <div id="operator-status"></div>
        <div id="operator-name"></div>
        <div id="operator-status-switcher" class="link arrow-down"></div>
        <div id="operator-status-switcher-options" class="studio-action-panel">

        </div>
    </div>   
    <div id="options-box" class="clearFix"></div>
            
    <div id="missed-calls-box">
        <div id="missed-calls-list"></div>
        <div id="missed-calls-empty-msg"><%= Resource.NoMissedCallsMsg %></div>
    </div>
            
    <div id="phone-box">
        <div id="selected-contact"></div>
        <div class="phone-selector-block">
            <div id="phone-selector-box">
                <div class="clearFix">
                    <div id="country-selector" class="voip-flag link arrow-down"></div>
                    <input id="phone-input" type="text" autocomplete="off" />
                    <div id="contact-phone-switcher-btn" class="link gray arrow-down"></div>
                    <div id="phone-clear-btn" class="clear-icon"></div>
                </div>
                <div id="contact-phone-switcher-panel" class="studio-action-panel">
                    <ul class="dropdown-content"></ul>
                </div>
            </div>
            <div id="select-contact-btn">
                <div class="voip-add-abonent"></div>
            </div>
        </div>

        <div id="panel-btns">
            <div class="panel-row clearFix">
                <div class="panel-btn" data-value="1">1</div>
                <div class="panel-btn" data-value="2">2<span>abc</span></div>
                <div class="panel-btn" data-value="3">3<span>def</span></div>
            </div>
            <div class="panel-row clearFix">
                <div class="panel-btn" data-value="4">4<span>ghi</span></div>
                <div class="panel-btn" data-value="5">5<span>jkl</span></div>
                <div class="panel-btn" data-value="6">6<span>mno</span></div>
            </div>
            <div class="panel-row clearFix">
                <div class="panel-btn" data-value="7">7<span>pqrs</span></div>
                <div class="panel-btn" data-value="8">8<span>tuv</span></div>
                <div class="panel-btn" data-value="9">9<span>wxyz</span></div>
            </div>
            <div class="panel-row clearFix">
                <div class="panel-btn" data-value="*">*</div>
                <div class="panel-btn" data-value="0">0</div>
                <div class="panel-btn" data-value="#">#</div>
            </div>
        </div>
                
        <a id="call-btn" class="button green"><%= Resource.CallBtn %></a>
    </div>
            
    <div id="call-box">
        <div id="call-status-box">
            <div class="call-status" data-status="0"><%= Resource.CallConnectStatus %></div>
            <div class="call-status" data-status="1"><%= Resource.CallGoingStatus %></div>
            <div class="call-status" data-status="2"><%= Resource.CallCompleteStatus %></div>
            <div class="call-status" data-status="3"><%= Resource.CallIncomingStatus %></div>
            <div class="call-status" data-status="4"><%= Resource.CallGoingStatus %></div>
            <div class="call-status" data-status="5"><%= Resource.CallCompleteStatus %></div>
            <div id="call-timer"></div>
        </div>
        
        <div id="call-main-box"></div>

        <div id="call-btns-box" class="clearFix">
            <a class="button call-btn red single complete-btn" data-status="0"><%= Resource.CompleteCallBtn %></a>
            <a class="button call-btn red single complete-btn" data-status="1"><%= Resource.CompleteCallBtn %></a>
            <a class="button call-btn green answer-btn" data-status="3"><%= Resource.AnswerCallBtn %></a>
            <a class="button call-btn gray reject-btn" data-status="3"><%= Resource.RejectCallBtn %></a>
            <a class="button call-btn red complete-btn" data-status="4"><%= Resource.CompleteCallBtn %></a>
            <a class="button call-btn gray redirect-btn" data-status="4"><%= Resource.RedirectCallBtn %></a>
                    
            <div class="clearFix"></div>
                    
            <div id="operators-redirect-box">
                <select id="operators-redirect-selector" class="comboBox"></select>
                <div id="operators-redirect-empty-msg"><%= Resource.NoRedirectOperatorsMsg %></div>
            </div>
        </div>
    </div>
</div>