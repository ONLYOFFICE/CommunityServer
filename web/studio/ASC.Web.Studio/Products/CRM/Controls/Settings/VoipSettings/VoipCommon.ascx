<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipCommon.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.VoipCommon" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

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