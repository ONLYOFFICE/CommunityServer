<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TabsContainerPart.ascx.cs" Inherits="ASC.Web.Talk.UserControls.TabsContainerPart" %>

<%@ Import Namespace="ASC.Web.Talk.Resources" %>

<div id="talkTabContainer" class="studio-top-panel">
    <div class ="studio-top-logo">
        <a class="top-logo" title="" onclick="ASC.Controls.JabberClient.redirectMain()">
                <img alt="" src="<%= GetAbsoluteCompanyTopLogoPath() %>" />
        </a> 
        <a title="" href=""><div class="talk-top-logo-label" ><%=TalkResource.Chat%></div></a>
    </div>
  <ul class="tabs">
   <!--<li class ="studio-top-logo" </li>
    <a class="top-logo" title="ONLYOFFICE" href="#">
       <img alt="" src="https://d3b5653v7ash7c.cloudfront.net/studio/tag/nct.8.9.0/skins/default/images/onlyoffice_logo/light_small_general.png">
    </a> -->
    <li class="tab default" unselectable="on">
      <div class="background" unselectable="on"><div class="helper" unselectable="on"></div></div>
      <div class="left-side" unselectable="on"></div>
      <div class="right-side" unselectable="on"></div>
      <div class="container" unselectable="on">
        <div class="button-talk close" unselectable="on" style="font-size: 20px;" >×</div>     
        <div class="state" unselectable="on"></div>   
        <div class="tab-title" unselectable="on"></div>
      </div>
    </li>
  </ul>
  <div class="navigation" unselectable="on">
    <div class="size studio-top-button-hidden-tabs">
        <div class="hiddennewmessage"></div>
        <div class="pointer-down"></div>
        <div class="countHiddenTabs"><span class="all">0</span></div>
        
        
    </div>
       <div class="popupContainerClass hiddenTabs" style="display:none">
           <ul class="contactlist listhiddencontacts" unselectable="on">
            <li class="contact default" unselectable="on">
                <div class="state" unselectable="on"></div>
                <div class="toolbar" unselectable="on">
                </div>
                <div class="title" unselectable="on"></div>
            </li>
        </ul>
       </div>
   <!-- <div class="button-container move-to-left" title="<%=TalkResource.HintShowLeftTab%>" unselectable="on"><div class="button-talk move-to-left"></div></div>
    <div class="button-container move-to-right" title="<%=TalkResource.HintShowRightTab%>" unselectable="on"><div class="button-talk move-to-right"></div></div> -->
  </div>
  
</div>
