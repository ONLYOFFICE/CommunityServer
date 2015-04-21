<%@ Assembly Name="ASC.Web.Community" %>
<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="ListPages.ascx.cs" Inherits="ASC.Web.UserControls.Wiki.UC.ListPages" %>

<div class="wikiList">
    <asp:Repeater ID="rptListPages" runat="Server">
        <ItemTemplate>
            <div>
                <a class = " linkMedium baseLinkAction" href="<%#GetPageLink((Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).PageName)%>">
                    <%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).PageName%></a>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
