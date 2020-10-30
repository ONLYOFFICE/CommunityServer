<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master"  AutoEventWireup="true" CodeBehind="VoipClient.aspx.cs" Inherits="ASC.Web.Studio.VoipClient" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>

<asp:Content ContentPlaceHolderID="PageContent" runat="server">
    <style type="text/css">
        body {
            overflow:hidden;
        }
    </style>
    <asp:PlaceHolder runat="server" ID="PhoneControl"/>
</asp:Content>