<%@ Assembly Name="ASC.Web.Community" %>
<%@ Page Language="C#" MasterPageFile="~/Products/Community/Master/Community.master" AutoEventWireup="true" EnableViewState="false" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Community.Blogs.Default" Title="Untitled Page" %>


<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<asp:Content ID="SettingsHeaderContent" ContentPlaceHolderID="CommunityPageHeader"
    runat="server">
</asp:Content>
<asp:Content ID="SettingsPageContent" ContentPlaceHolderID="CommunityPageContent"
    runat="server">
                    <sc:Container ID="mainContainer" runat="server">
                        <header></header>
                        <body>
                            <asp:PlaceHolder ID="placeContent" runat="server"></asp:PlaceHolder>
                            <asp:Literal ID="ltrBody" runat="server"></asp:Literal>
                        <%if (blogsCount>20) {%>
                        <div class="navigationLinkBox news">
                            <table id="tableForNavigation" cellpadding="4" cellspacing="0">
                                <tbody>
                                    <tr>
                                        <td>
                                            <div style="clear: right; display: inline-block;">
                                                <asp:PlaceHolder ID="pageNavigatorHolder" runat="server"></asp:PlaceHolder>
                                             </div>
                                        </td>
                                        <td style="text-align:right;">
                                            <span class="gray-text"><%=ASC.Blogs.Core.Resources.BlogsResource.TotalPages%>: </span>
                                            <span class="gray-text" style="margin-right: 20px;"><%=blogsCount%></span>
                                            <span class="gray-text"><%=ASC.Blogs.Core.Resources.BlogsResource.ShowOnPage%>: </span>
                                            <select class="top-align">
                                                <option value="20">20</option>
                                                <option value="50">50</option>
                                                <option value="75">75</option>
                                                <option value="100">100</option>
                                            </select>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                        <%} %>                        
                        </body>
                    </sc:Container>
                        
</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="CommunitySidePanel" runat="server">
</asp:Content>

