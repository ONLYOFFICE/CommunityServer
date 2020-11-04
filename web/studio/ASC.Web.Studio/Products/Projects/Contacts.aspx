<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Data.Storage" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Page Language="C#" MasterPageFile="Masters/BasicTemplate.Master"
    AutoEventWireup="true" CodeBehind="Contacts.aspx.cs" Inherits="ASC.Web.Projects.Contacts" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>


<asp:Content runat="server" ID="PageContent" ContentPlaceHolderID="BTPageContent">
    <%if (CanLinkContact) { %>
    <div id="contactsForProjectPanel">
        <div class="bold" style="margin-bottom:5px;">
            <%= ProjectsCommonResource.SelectOrAddCRMContact %>
        </div>
        <div id="projContactSelectorParent"></div>
    </div>
    <% } %>

    <div id="contactListBox">
        <table id="contactTable" class="table-list padding4">
            <tbody>
            </tbody>
        </table>
    </div>
    <asp:PlaceHolder ID="emptyScreen" runat="server"></asp:PlaceHolder>
</asp:Content>