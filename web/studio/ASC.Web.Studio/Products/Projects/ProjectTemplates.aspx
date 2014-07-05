<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="ProjectTemplates.aspx.cs" Inherits="ASC.Web.Projects.ProjectTemplates" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>
    
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>


<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    
<asp:PlaceHolder runat="server" ID="_content"></asp:PlaceHolder>

<div class="projects-templates-container">   
    <table id="listTemplates">
    </table>
        
    <div id="templateActionPanel" class="studio-action-panel" target="">
	    <div class="corner-top right"></div>
	    <ul class="actionList dropdown-content">
	        <li id="editTmpl" title="<%= ProjectTemplatesResource.Edit %>" class="dropdown-item"><%= ProjectTemplatesResource.Edit %></li>
	        <li id="createProj" title="<%= ProjectTemplatesResource.CreateProjFromTmpl %>" class="dropdown-item"><%= ProjectTemplatesResource.CreateProjFromTmpl %></li>
	        <li id="deleteTmpl" title="<%= ProjectTemplatesResource.Delete %>" class="dropdown-item"><%= ProjectTemplatesResource.Delete %></li>
	    </ul>        
    </div>
    
    <div id="questionWindow" style="display: none">
        <sc:Container ID="_hintPopup" runat="server">
            <Header>
                <%= ProjectTemplatesResource.RemoveTemplateTitlePopup%>
            </Header>
            <Body>        
                <p><%= ProjectTemplatesResource.RemoveQuestion%> </p>
                <p><%= ProjectsCommonResource.PopupNoteUndone %></p>
                <div class="middle-button-container">
                    <a class="button blue middle remove"><%= ProjectTemplatesResource.RemoveTemplateTitlePopup%></a>
                    <span class="splitter-buttons"></span>
                    <a class="button gray middle cancel"><%= ProjectsCommonResource.Cancel%></a>
                </div>     
            </Body>
        </sc:Container>
    </div>
</div>
</asp:Content>
<asp:Content ID="projectsClientTemplatesResourcesPlaceHolder" ContentPlaceHolderID="projectsClientTemplatesResourcesPlaceHolder" runat="server">
</asp:Content>