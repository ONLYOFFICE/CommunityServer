<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" CodeBehind="ServerError.aspx.cs" EnableViewState="false" Inherits="ASC.Web.Studio.ServerError" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="Resources" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <div class="header-base-big">
        <span><%= ErrorCaption %></span>
    </div>
    <br />
    <%= ErrorText %>
    <div style="margin-top: 20px;">
        <a href="<%= VirtualPathUtility.ToAbsolute("~/") %>"><%= Resource.BackToHomeLink %></a>
    </div>
</asp:Content>
