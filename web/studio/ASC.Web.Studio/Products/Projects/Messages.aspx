<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#"  AutoEventWireup="true" MasterPageFile="Masters/BasicTemplate.Master"
            CodeBehind="Messages.aspx.cs" Inherits="ASC.Web.Projects.Messages" %>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
    
<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="contentHolder" runat="server"></asp:PlaceHolder>
</asp:Content>


<asp:Content ID="projectsClientTemplatesResourcesPlaceHolder" ContentPlaceHolderID="projectsClientTemplatesResourcesPlaceHolder" runat="server">
</asp:Content>