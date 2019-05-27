<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="Settings.aspx.cs" Inherits="ASC.Web.Projects.Settings" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Assembly Name="ASC.Web.Projects" %>

<asp:Content runat="server" ID="PageContent" ContentPlaceHolderID="BTPageContent">
<div id="settings"></div>
<asp:PlaceHolder runat="server" ID="FolderSelectorHolder"></asp:PlaceHolder>
</asp:Content>