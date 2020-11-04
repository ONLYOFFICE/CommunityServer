<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master" Inherits="ASC.Web.CRM.Contacts"%>
<%@ MasterType TypeName="ASC.Web.CRM.BasicTemplate" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    <asp:PlaceHolder ID="TabsHolder" runat="server" Visible="False">
        <div id="ContactTabs"></div>
    </asp:PlaceHolder>
</asp:Content>

<asp:Content ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="loaderHolder" runat="server"/>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"/>
    <asp:HiddenField ID="_ctrlContactID" runat="server" />
    <div id="files_hintTypesPanel" class="hintDescriptionPanel">
        <%: CRMContactResource.TooltipTypes %>
        <% if (!string.IsNullOrEmpty(HelpLink)) { %>
        <a href="<%= HelpLink + "/tipstricks/contact-types.aspx" %>" target="_blank">
            <%=CRMCommonResource.ButtonLearnMore%>
        </a>
        <% } %>
    </div>
    <div id="files_hintCsvPanel" class="hintDescriptionPanel">
        <%: CRMContactResource.TooltipCsv %>
        <% if (!string.IsNullOrEmpty(HelpLink)) { %>
        <a href="<%= HelpLink + "/guides/create-CSV-file.aspx" %>" target="_blank">
            <%=CRMCommonResource.ButtonLearnMore%>
        </a>
        <% } %>
    </div>
</asp:Content>