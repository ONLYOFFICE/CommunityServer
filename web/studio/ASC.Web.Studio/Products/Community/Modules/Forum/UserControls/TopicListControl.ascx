<%@ Assembly Name="ASC.Web.Community"%>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopicListControl.ascx.cs" Inherits="ASC.Web.UserControls.Forum.TopicListControl" %>
<%@ Import Namespace="ASC.Web.UserControls.Forum.Resources" %>
<div class="forum_columnHeader">
<table width="100%" style="height: 100%" border="0" cellpadding="0" cellspacing="0">
    <tr>
        <td align="left" valign="bottom">
            <%=ForumUCResource.Topic%>
        </td>
        <td style="width: 100px;" align="center" valign="bottom">
            <%=ForumUCResource.ViewCount%>
        </td>
        <td style="width: 100px;"align="center" valign="bottom">
            <%=ForumUCResource.PostCount%>
        </td>
        <td style="width: 180px;" valign="middle">
            <%=ForumUCResource.RecentUpdate%>
        </td>
    </tr>
</table>
</div>
<div class="clearFix">
    <asp:PlaceHolder ID="topicListHolder" runat="server"></asp:PlaceHolder>
</div>
<%if (TopicPagesCount > 20){ %>
<div class="navigationLinkBox forums">
            <table id="tableForNavigation" cellpadding="4" cellspacing="0">
                <tbody>
                <tr>
                    <td>
                        <div style="clear: right; display: inline-block;">    
                            <asp:PlaceHolder ID="bottomPageNavigatorHolder" runat="server"></asp:PlaceHolder>
                    </div>
                    </td>
                    <td style="text-align:right;">
                        <span class="gray-text"><%=ForumUCResource.TotalPages%>: </span>
                        <span class="gray-text" style="margin-right: 20px;"><%=TopicPagesCount%></span>
                        <span class="gray-text"><%=ForumUCResource.ShowOnPage%>: </span>
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