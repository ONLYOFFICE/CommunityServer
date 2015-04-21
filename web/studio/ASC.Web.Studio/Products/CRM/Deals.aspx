<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master"
    CodeBehind="Deals.aspx.cs"  Inherits="ASC.Web.CRM.Deals" %>
<%@ MasterType TypeName="ASC.Web.CRM.BasicTemplate" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<asp:Content ID="PageContentWithoutCommonContainer" ContentPlaceHolderID="BTPageContentWithoutCommonContainer" runat="server">
    <asp:PlaceHolder ID="_navigationPanelContent" runat="server"></asp:PlaceHolder>
</asp:Content>
<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="loaderHolder" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="CommonContainerHolder" runat="server"></asp:PlaceHolder>
    <div id="files_hintStagesPanel" class="hintDescriptionPanel">
        <%=CRMDealResource.TooltipStages%>
        <a href="http://www.onlyoffice.com/help/tipstricks/opportunity-stages.aspx" target="_blank"><%=CRMCommonResource.ButtonLearnMore%></a>
    </div>
</asp:Content>