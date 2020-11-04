<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Search.aspx.cs" Inherits="ASC.Web.Studio.Search" %>

<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <div class="header-base-big">
        <span><%= HeaderStringHelper.GetHTMLSearchHeader(SearchText) %></span>
        <input type="hidden" id="searchTextHidden" value="<%= SearchText.HtmlEncode() %>" />
    </div>
    <br />
    <asp:PlaceHolder ID="SearchContent" runat="server"/>
</asp:Content>
