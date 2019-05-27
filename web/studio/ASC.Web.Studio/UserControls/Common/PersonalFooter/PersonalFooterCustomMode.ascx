<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PersonalFooter.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.PersonalFooter.PersonalFooter" %>

<%@ Import Namespace="ASC.Web.Studio.PublicResources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div class="personal-footer">
    <div class="personal-footer_w clearFix">
        <ul class="personal-footer-links">
            <li><a href="<%= HelpLink + "/pdf/Terms/cloud.pdf" %>" target="_blank"><%=CustomModeResource.TermsServiceCustomMode %></a></li>
        </ul>
        <div class="personal-footer_rights"><%= String.Format(CustomModeResource.AllRightsReservedCustomMode,DateTime.UtcNow.Year.ToString())%></div>
    </div>
</div>