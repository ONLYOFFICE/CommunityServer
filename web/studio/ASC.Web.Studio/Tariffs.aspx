<%@ Page MasterPageFile="~/Masters/BaseTemplate.master" Language="C#" AutoEventWireup="true" CodeBehind="Tariffs.aspx.cs" Inherits="ASC.Web.Studio.Tariffs" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div class="tariff-page">
        <asp:PlaceHolder runat="server" ID="pageContainer"/>
    </div>
</asp:Content>
