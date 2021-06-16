﻿<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Diff.aspx.cs" Inherits="ASC.Web.Community.Wiki.Diff"
    MasterPageFile="~/Products/Community/Modules/Wiki/Wiki.Master" %>

<%@ Import Namespace="ASC.Web.Community.Modules.Wiki.Resources" %>

<asp:Content ContentPlaceHolderID="WikiTitleContent" runat="Server">
    <div class="header-with-menu"><%=string.Format(WikiResource.wikiDiffDescriptionFormat, OldVer, NewVer) %></div>
</asp:Content>

<asp:Content ContentPlaceHolderID="WikiContents" runat="Server">
    <asp:Panel ID="pDiff" class="wikiDiff" runat="Server">
        <ol>
            <asp:Literal ID="litDiff" runat="Server" />
        </ol>
    </asp:Panel>
    <div class="big-button-container">
        <asp:LinkButton ID="cmdCancel" CssClass="button gray big" runat="Server" OnClick="cmdCancel_Click"/>
    </div>
</asp:Content>
