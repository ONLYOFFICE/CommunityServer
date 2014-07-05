<%@ Assembly Name="ASC.Web.Community.Wiki" %>
<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="ListPages.ascx.cs" Inherits="ASC.Web.UserControls.Wiki.UC.ListPages" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Data" %>
<div class="wikiList">
    <asp:Repeater ID="rptListPages" runat="Server">
        <ItemTemplate>
            <div>
                <a class = " linkMedium baseLinkAction" href="<%#GetPageLink((Container.DataItem as Pages).PageName)%>">
                    <%#(Container.DataItem as Pages).PageName%></a>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
