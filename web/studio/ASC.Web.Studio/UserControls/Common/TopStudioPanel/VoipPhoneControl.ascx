<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipPhoneControl.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.VoipPhoneControl" %>
<%@ Import Namespace="Resources" %>
<script type="text/javascript" src="//media.twiliocdn.com/sdk/js/client/v1.9/twilio.min.js"></script>

<div id="studio_dropVoipPopupPanel">
    <div id="voip-phone-view">
        <div id="service-unavailable-box">
            <%= Resource.VoipServiceUnavailableMsg %>
        </div>

        <div id="service-already-running-box">
            <%= Resource.VoipServiceAlreadyRunningMsg %>
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
                
            <a id="call-btn" class="button green disable"><%= Resource.CallBtn %></a>
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
</div>
