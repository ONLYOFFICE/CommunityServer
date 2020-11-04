<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.CRM" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/CRM/Masters/BasicTemplate.Master" CodeBehind="MailViewer.aspx.cs" Inherits="ASC.Web.CRM.MailViewer" %>

<%@ MasterType TypeName="ASC.Web.CRM.BasicTemplate" %>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <div id="mailHistoryEventContainer">
        <div class="messageHeader"></div>
        <div class="messageContent mobile-overflow"></div>
    </div>
</asp:Content>
