<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master" EnableViewState="false" EnableViewStateMac="false" AutoEventWireup="true" CodeBehind="FileChoice.aspx.cs" Inherits="ASC.Web.Files.FileChoice" %>

<%@ Import Namespace="ASC.Files.Core" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>

<asp:Content runat="server" ContentPlaceHolderID="BTPageContent">
    <% if (!string.IsNullOrEmpty(RequestExt))
       { %>
    <span><%= string.Format(FilesUCResource.FileChoiceFormat, RequestExt.ToUpper()) %></span>
    <br />
    <br />
    <% }
       else if (RequestType != FilterType.None && FromEditor)
       { %>
    <span><%= string.Format(FilesUCResource.FileChoiceType, GetTypeString(RequestType).ToLower()) %></span>
    <br />
    <br />
    <% } %>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
