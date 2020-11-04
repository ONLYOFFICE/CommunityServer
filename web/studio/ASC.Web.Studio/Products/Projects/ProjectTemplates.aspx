<%@ Assembly Name="ASC.Web.Projects" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="ProjectTemplates.aspx.cs" Inherits="ASC.Web.Projects.ProjectTemplates" %>
    
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>


<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    
<asp:PlaceHolder runat="server" ID="_content"></asp:PlaceHolder>

<div class="projects-templates-container">
    <table id="listTemplates">
    </table>
</div>
</asp:Content>