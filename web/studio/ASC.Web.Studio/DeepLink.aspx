<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" EnableViewState="false" CodeBehind="DeepLink.aspx.cs" Inherits="ASC.Web.Studio.DeepLink" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <div class="deeplink-form-page">
        <asp:PlaceHolder runat="server" ID="DeepLinkingHolder"/>
    </div>
</asp:Content>