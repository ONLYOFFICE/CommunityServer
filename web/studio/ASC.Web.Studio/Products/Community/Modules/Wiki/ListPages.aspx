<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListPages.aspx.cs" Inherits="ASC.Web.Community.Wiki.ListPages"
    MasterPageFile="~/Products/Community/Modules/Wiki/Wiki.Master" %>


<asp:Content ContentPlaceHolderID="WikiContents" runat="Server">
    <div id="listWikiPages"></div>

    <asp:PlaceHolder ID="phListEmptyScreen" runat="Server"/>
</asp:Content>
