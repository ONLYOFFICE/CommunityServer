<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VoipQuick.ascx.cs" Inherits="ASC.Web.CRM.Controls.Settings.VoipQuick" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<div id="voip-quick-view">
    <span id="show-buy-phone-popup-btn" class="link dotline plus display-none"><%= CRMVoipResource.BuyNumberBtn %></span>
    <span id="show-link-phone-popup-btn" class="link dotline plus display-none"><%= CRMVoipResource.LinkNumberBtn %></span>
    
    <div id="buy-phone-popup" class="phone-popup">
        <sc:Container ID="buyNumberContainer" runat="server">
            <Header>
                <%= CRMVoipResource.BuyNumberHeader %>
            </Header>
            <Body>
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

                <div class="numbers-box">
                    <div class="numbers-loader"><%= CRMVoipResource.LoadingMsg %></div>
                    <div class="numbers-empty-msg"><%= CRMVoipResource.NoAvailableNumbersMsg %></div>
                    <div id="available-numbers-empty-search-msg"><%= CRMVoipResource.NoSearchingNumbersMsg %></div>
                    <div class="numbers"></div>
                </div>
                <div class="middle-button-container">
                    <a class="button disable phone-btn"><%= CRMVoipResource.BuyNumberBtn %></a>
                    <a class="button gray cancel-btn"><%= CRMVoipResource.CancelBtn %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
    
    <div id="link-phone-popup" class="phone-popup">
        <sc:Container ID="linkNumberContainer" runat="server">
            <Header>
                <%= CRMVoipResource.LinkNumberHeader %>
            </Header>
            <Body>
                <div class="header-base-small"><%= CRMVoipResource.LinkNumberBody %></div>
                <div class="numbers-box">
                    <div class="numbers-loader"><%= CRMVoipResource.LoadingMsg %></div>
                    <div class="numbers-empty-msg"><%= CRMVoipResource.NoExistingNumbersMsg %></div>
                    <div class="numbers"></div>
                </div>
                <div class="middle-button-container">
                    <a class="button disable phone-btn"><%= CRMVoipResource.LinkNumberBtn %></a>
                    <a class="button gray cancel-btn"><%= CRMVoipResource.CancelBtn %></a>
                </div>
            </Body>
        </sc:Container>
    </div>

    <div id="remove-number-popup" class="phone-popup">
        <sc:Container ID="deleteNumberContainer" runat="server">
            <Header>
                <%= CRMVoipResource.DeleteNumberHeader %>
            </Header>
            <Body>                
                <div class="header-base-small"><%= CRMVoipResource.DeleteNumberBody %></div>
                <div class="middle-button-container">
                    <a id="remove-number-btn" class="button blue"><%= CRMVoipResource.DeleteBtn %></a>
                    <span class="splitter-buttons"></span>
                    <a id="cancel-remove-phone-btn" class="button gray"><%= CRMVoipResource.CancelBtn %></a>
                </div>
            </Body>
        </sc:Container>
    </div>
    
    <div id="existing-numbers-empty-box" class="display-none"></div>
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