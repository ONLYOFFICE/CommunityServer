
<%@ Assembly Name="ASC.Web.Core"%>
<%@ Assembly Name="ASC.Web.Community"%>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ThreadCategoryListControl.ascx.cs" Inherits="ASC.Web.UserControls.Forum.ThreadCategoryListControl" %>

<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>


<asp:Repeater ID="_categoryRepeater" runat="server">
    <ItemTemplate>
       <div class="clearFix">
            <div class="header-base" style="padding-top: 25px; max-width:600px; overflow: hidden;"
                title="<%#HttpUtility.HtmlEncode((string)Eval("Title"))%>">
                <%#HttpUtility.HtmlEncode((string)Eval("Title"))%>
            </div>
            <div class="describe-text" style="padding: 2px 0px 25px; width: 285px; word-wrap: break-word;"><%#HttpUtility.HtmlEncode((string)Eval("Description")).Replace("\r\n", "</br>").Replace("\n", "</br>").Replace("  ", " &nbsp;")%></div>

            <asp:Repeater ID="_threadRepeater" runat="server">
                <ItemTemplate>                
                    <div class="clearFix borderBase forums-row" style="padding:5px 0px; margin-top:-1px; border-right:none; border-left:none;width: 100%;overflow:hidden;">
                        <table cellpadding="0" cellspacing="0" style="width:100%;">
                            
                            <tr valign="top">
                                <td align="center" style="width:35px; padding:0px 10px;">
                                <%# ((ASC.Forum.Thread)Container.DataItem).IsNew()?                                    
                                    ("<img alt=\"new\"  align='absmiddle' src=\"" + WebImageSupplier.GetAbsoluteWebPath("forum_unread.png", _settings.ImageItemID) + "\"/>"):
                                    ("<img alt=\"\" align='absmiddle' src=\"" + WebImageSupplier.GetAbsoluteWebPath("forum_read.png", _settings.ImageItemID) + "\"/>")
                                %> 
                                </td> 
                                <td style="padding-top:8px;">
                                    <div style="padding-right: 5px; width: 285px; word-wrap: break-word;">
                                        <a class="link bold" href="<%#_settings.LinkProvider.TopicList((int)Eval("ID"))%>"
                                            title="<%#HttpUtility.HtmlEncode((string)Eval("Title"))%>"><%#HttpUtility.HtmlEncode((string)Eval("Title"))%></a>
                                    </div>
                                    <div class="describe-text" style="padding: 2px 0px; width: 285px; word-wrap: break-word;"><%#HttpUtility.HtmlEncode((string)Eval("Description")).Replace("\r\n", "</br>").Replace("\n", "</br>").Replace("  ", " &nbsp;")%></div>
                                </td>
                                <td class="header-base-medium" style="width:100px; padding-top:8px; text-align:center;">
                                    <%#Eval("TopicCount")%>
                                </td>
                                <td class="header-base-medium" style="width:100px; padding-top:8px; text-align:center;">                                    
                                    <%#Eval("PostCount")%>
                                </td>
                                <td style="width:180px; padding-top:8px;">
                                <%#RenderRecentUpdate((Container.DataItem as ASC.Forum.Thread))%>                                
                                </td>          
                            </tr>
                        </table>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
       </div>        
    </ItemTemplate>
</asp:Repeater>
