<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TalkNavigationItem.ascx.cs" Inherits="ASC.Web.Talk.UserControls.TalkNavigationItem" %>

<li class="top-item-box talk">
  <span id="talkMsgLabel" class="inner-text" title="<%=GetMessageStr()%>" onclick="ASC.Controls.JabberClient.extendChat()">
    <span id="talkMsgCount" class="inner-label">0</span>
  </span>
</li>
