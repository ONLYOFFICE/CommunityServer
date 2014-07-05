<%@ Assembly Name="ASC.Web.Community.Wiki" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Diff.aspx.cs" Inherits="ASC.Web.Community.Wiki.Diff"
    MasterPageFile="~/Products/Community/Modules/Wiki/Wiki.Master" %>

<asp:Content ContentPlaceHolderID="WikiContents" runat="Server">
    <asp:Panel ID="pDiff" class="wikiDiff" runat="Server">
        <ol>
                <asp:Literal ID="litDiff" runat="Server" />
        </ol>
    </asp:Panel>
</asp:Content>
