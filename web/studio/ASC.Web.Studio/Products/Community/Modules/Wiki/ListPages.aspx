<%@ Assembly Name="ASC.Web.Community" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListPages.aspx.cs" Inherits="ASC.Web.Community.Wiki.ListPages"
    MasterPageFile="~/Products/Community/Modules/Wiki/Wiki.Master" %>


<%@ Import Namespace="ASC.Web.Community.Wiki" %>
<asp:Content ContentPlaceHolderID="WikiContents" runat="Server">
    <div>
        <asp:PlaceHolder ID="phListResult" runat="Server">
            <asp:Repeater ID="rptPageList" runat="server">
                <HeaderTemplate>
                    <table class="tableBase" cellpadding="10" cellspacing="0">
                        <colgroup>
                            <col/>
                            <col style="width: 150px;"/>
                            <col style="width: 140px;"/>
                        </colgroup>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr class="row">
                        <td class="borderBase">
                            <asp:HyperLink runat="server" ID="hlPageLink" CssClass = "linkHeaderMedium" Text='<%#GetPageName(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>'
                                NavigateUrl='<%#GetPageViewLink(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>' />
                        </td>
                        <td class="borderBase">
                            <%#GetAuthor(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>
                        </td>
                        <td class="borderBase gray-text" style="text-align: right">
                            <%#GetDate(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </asp:PlaceHolder>
    </div>
    <asp:PlaceHolder ID="phTableResult" runat="Server">
        <asp:Repeater ID="rptMainPageList" runat="Server">
            <HeaderTemplate>
                <table class="catDict" border="0" cellspacing="0" cellpadding="0" width="100%">
                    <tr>
            </HeaderTemplate>
            <ItemTemplate>
                <td class="catLetter">
                    <asp:Repeater ID="rptPageList" runat="server" DataSource='<%#Container.DataItem %>'>
                        <ItemTemplate>
                            <div class="block">
                                <div class="letter">
                                    <%#(Container.DataItem as PageDictionary).HeadName%>
                                </div>
                                <asp:Repeater runat="Server" ID="rptPages" DataSource='<%#(Container.DataItem as PageDictionary).Pages%>'>
                                    <HeaderTemplate>
                                        <div class="catList">
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="category">
                                            <a class = "linkAction" href="<%#GetPageViewLink(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page)%>"
                                                title="<%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).PageName%>">
                                                <%#(Container.DataItem as ASC.Web.UserControls.Wiki.Data.Page).PageName%></a>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </div>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </td>
            </ItemTemplate>
            <FooterTemplate>
                </tr> </table>
            </FooterTemplate>
        </asp:Repeater>
    </asp:PlaceHolder>
</asp:Content>
