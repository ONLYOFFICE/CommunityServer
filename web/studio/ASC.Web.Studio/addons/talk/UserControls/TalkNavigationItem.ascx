<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TalkNavigationItem.ascx.cs" Inherits="ASC.Web.Talk.UserControls.TalkNavigationItem" %>
<%@ Import Namespace="ASC.Data.Storage" %>

<li class="top-item-box talk">
  <span id="talkMsgLabel" class="inner-text" title="<%=GetMessageStr()%>" onclick="ASC.Controls.JabberClient.extendChat()">
    <svg><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenuTalk"></use></svg>
    <span id="talkMsgCount" class="inner-label">0</span>
  </span>
</li>
