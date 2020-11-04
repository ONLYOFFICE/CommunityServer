<%@ Assembly Name="ASC.Web.Community"%>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopicControl.ascx.cs" Inherits="ASC.Web.UserControls.Forum.TopicControl" %>

<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>


<div id="forum_topic_<%=Topic.ID%>" class="borderBase clearFix forums-row" style="padding:5px 0px; margin-top:-1px; border-right:none; border-left:none; overflow:hidden;">    
    <table cellpadding="0" cellspacing="0" style="width:100%;">
        <tr valign="top">
        <td align="center" style="width:35px; padding:0px 10px;">
            <img alt="<%=(Topic.IsNew()?"new":"") %>" src="<%=_imageURL%>"/>
         </td>
         <td style="padding-top:8px;">
         <div class="clearFix">            
              <%="<a class=\"link bold\" href=\"Posts.aspx?&t=" + Topic.ID + "\">" + HttpUtility.HtmlEncode(Topic.Title) + "</a>"%>
                  <%=RenderPages()%>             
                  <%=RenderModeratorFunctions()%>
                </div>                              
               <%if (IsShowThreadName)
                     Response.Write("<div style=\"margin-top:3px;\"><a class=\"link\" href=\"" + _settings.LinkProvider.TopicList(Topic.ThreadID) + "\">" + HttpUtility.HtmlEncode(Topic.ThreadTitle) + "</a></div>");
               %>
              <div style="padding:5px 0px;" class="clearFix">
                <%= ASC.Core.Users.StudioUserInfoExtension.RenderCustomProfileLink(ASC.Core.CoreContext.UserManager.GetUsers(Topic.PosterID), "describe-text", "link gray")%>             
             </div>
             <% %>
             <asp:Panel runat="server" ID="_tagsPanel">
               <asp:Repeater ID="tagRepeater" runat="server">
                 <HeaderTemplate>
                     <div class="text-medium-describe clearFix" style="padding-top: 2px;">
                     <img alt='' style="margin-right:3px;" src="<%=WebImageSupplier.GetAbsoluteWebPath("tags.png", _settings.ImageItemID)%>" />
                 </HeaderTemplate>
                 <ItemTemplate>
                     <a class="link" href="<%#_settings.LinkProvider.SearchByTag((int)Eval("ID"))%>">
                         <%# HttpUtility.HtmlEncode((string)Eval("Name")) %>
                     </a>
                 </ItemTemplate>
                 <SeparatorTemplate>, </SeparatorTemplate>
                 <FooterTemplate></div></FooterTemplate>
             </asp:Repeater> 
             </asp:Panel>
         </td>        
         <td class="header-base-medium" style="width:100px; padding-top:8px; text-align:center;">
            <%=Topic.ViewCount%>            
         </td>
         <td class="header-base-medium" style="width:100px; padding-top:8px; text-align:center;">
            <%=Topic.PostCount%>              
         </td>
         <td style="width:180px; padding-top:8px;">
            <%=RenderLastUpdates()%>
         </td>         
         </tr>
        </table>
</div>