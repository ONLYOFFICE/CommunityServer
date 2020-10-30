<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PortalEncryption.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.PortalEncryption.PortalEncryption" %>
<%@ Import Namespace="Resources" %>

<div id="wrapper">
    <div id="container">
        <img src="/TenantLogo.ashx?logotype=2&defifnoco=true" id="logo" alt="" title="" />
        <div id="content">
            <p><%= Resource.PortalEncryptionProcessTitle %></p>
            <div id="progressbar_container">
                <div id="progress-line">
                    <div class="asc-progress-wrapper">
                        <div class="asc-progress-value"></div>
                    </div>
                    <span class="asc-progress_percent"></span>
                </div>
                <div id="progress-error"></div>
            </div>
        </div>
        <p id="comment"><%= Resource.PortalEncryptionText %></p>
    </div>
</div>