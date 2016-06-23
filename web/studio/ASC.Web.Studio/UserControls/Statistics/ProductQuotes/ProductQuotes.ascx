<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductQuotes.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Statistics.ProductQuotes" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="Resources" %>

<%-- Quotes --%>
<div class="quotesBlock">
    <div class="information">
        <table class="information">
            <tr>
                <td class="name"><%= Resource.TenantCreated %>:</td>
                <td class="value"><% =RenderCreatedDate() %></td>
            </tr>
            <tr>
                <td class="name"><%= Resource.TenantUsersTotal %>:</td>
                <td class="value"><%= RenderUsersTotal() %></td>
            </tr>
        </table>
    </div>
    <div class="header-base storeUsage">
        <%= Resource.TenantUsedSpace %>: <span class="diskUsage"><%= RenderUsedSpace() %></span>
        <% if (!CoreContext.Configuration.Standalone)
           { %>
        <div class="description">
            <%= String.Format(Resource.TenantUsedSpacePremiumDescription, GetMaxTotalSpace()) %>
        </div>
        <% } %>
    </div>
    
    <asp:Repeater ID="_itemsRepeater" runat="server">
        <ItemTemplate>
            <div class="header-base header">
                <img align="absmiddle" src="<%# ((ASC.Web.Studio.UserControls.Statistics.ProductQuotes.Product)Container.DataItem).Icon %>" alt="" /> <%# ((ASC.Web.Studio.UserControls.Statistics.ProductQuotes.Product)Container.DataItem).Name.HtmlEncode() %>
            </div>
            <div class="contentBlock">
                <asp:Repeater ID="_usageSpaceRepeater" runat="server">
                    <HeaderTemplate>
                        <table class="quotes">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr class="borderBase <%# Container.ItemIndex >= 10 ? "display-none" : "" %>" >
                            <td class="icon">
                                <%# String.IsNullOrEmpty((string)Eval("Icon")) ? "" : "<img src=\"" + Eval("Icon") + "\" alt=\"" + ((string)Eval("Name")).HtmlEncode() + "\" />"%>
                            </td>
                            <td class="name">
                                <div>
                                    <%# String.IsNullOrEmpty((string)Eval("Url"))
                                        ? "<span class=\"header-base-small\" title=\"" + ((string)Eval("Name")).HtmlEncode() + "\" >" + ((string)Eval("Name")).HtmlEncode() + "</span>"
                                        : "<a href=\"" + Eval("Url") + "\" class=\"link bold\" title=\"" + ((string)Eval("Name")).HtmlEncode() + "\" >" + ((string)Eval("Name")).HtmlEncode() + "</a>" %>
                                </div>
                            </td>
                            <td class="value">
                                <%# Eval("Size") %>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                
                <asp:PlaceHolder ID="_showMorePanel" runat="server">
                    <div class="moreBox">
                        <%= Resource.Top10Title %>
                        <a class="link dotted gray topTitleLink"><%= Resource.ShowAllButton %></a>
                    </div>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="_emptyUsageSpace" runat="server">
                    <div class="emptySpace">
                        <%= Resource.EmptyUsageSpace %>
                    </div>
                </asp:PlaceHolder>
                
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>