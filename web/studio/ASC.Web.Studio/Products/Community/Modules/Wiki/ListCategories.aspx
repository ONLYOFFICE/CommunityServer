<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListCategories.aspx.cs"
    Inherits="ASC.Web.Community.Wiki.ListCategories" MasterPageFile="~/Products/Community/Modules/Wiki/Wiki.Master" %>

<%@ Import Namespace="ASC.Web.Community.Wiki" %>
<asp:Content ContentPlaceHolderID="WikiContents" runat="Server">
    <% if (HasCategories) %>
    <% { %>
    <asp:Repeater ID="rptCategoryList" runat="Server">
        <HeaderTemplate>
            <table class="catDict" border="0" cellspacing="0" cellpadding="0" width="100%">
                <tr>
        </HeaderTemplate>
        <ItemTemplate>
            <td class="catLetter">
                <asp:Repeater ID="rptCategoryList" runat="server" DataSource='<%#Container.DataItem %>'>
                    <ItemTemplate>
                        <div class="block">
                            <div class="letter">
                                <%#(Container.DataItem as CategoryDictionary).HeadName%>
                            </div>
                            <asp:Repeater runat="Server" ID="rptCatList" DataSource='<%#(Container.DataItem as CategoryDictionary).Categories%>'>
                                <HeaderTemplate>
                                    <div class="catList">
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <div class="category">
                                        <a class = "linkAction" href="<%#(Container.DataItem as CategoryInfo).CategoryUrl%>" title="<%#(Container.DataItem as CategoryInfo).CategoryName%>">
                                            <%#(Container.DataItem as CategoryInfo).CategoryName%></a>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </div>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </ItemTemplate>
    </asp:Repeater>
    </td> </ItemTemplate>
    <footertemplate>
            </tr> </table>
        </footertemplate>
    </asp:Repeater>
    <% } else { %>
        <asp:PlaceHolder ID="EmptyContent" runat="server"/>
    <% } %>
</asp:Content>
