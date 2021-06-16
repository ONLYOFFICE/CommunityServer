﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StepContainer.ascx.cs" Inherits="ASC.Web.Studio.UserControls.FirstTime.StepContainer" %>
<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<div class="step">

        <div class="stepBody">
            <asp:PlaceHolder ID="content1" runat="server"></asp:PlaceHolder>
        </div>
        <div class="footer clearFix">
                <div class="big-button-container">
                    <a class="button blue big" id="saveSettingsBtn" href="javascript:void(0)" onclick="<%= this.SaveButtonEvent %>">
                        <%= Resource.ContinueButton %></a>
                </div>
        </div>
</div>
