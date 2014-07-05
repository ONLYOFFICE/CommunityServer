<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BlankModal.ascx.cs" Inherits="ASC.Web.Mail.Controls.BlankModal" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<div class="backdrop" blank-page=""></div>

<div id="content" blank-page="" class="dashboard-center-box mail">
    <div class="header">
        <span class="close" onclick="blankModal.close();"></span><%=MailResource.BlankModalHeader%>
    </div>
    <div class="content clearFix">
       <div class="module-block">
           <div class="img contacts"></div>
           <div class="title"><%=MailResource.BlankModalAccountsTitle%></div>
           <ul>
               <li><%=MailResource.BlankModalAccountsTip1%></li>
               <li><%=MailResource.BlankModalAccountsTip2%></li>
               <li><%=MailResource.BlankModalAccountsTip3%></li>
           </ul>
       </div>
       <div class="module-block">
           <div class="img tags"></div>
           <div class="title"><%=MailResource.BlankModalTagsTitle%></div>
           <ul>
               <li><%=MailResource.BlankModalTagsTip1%></li>
               <li><%=MailResource.BlankModalTagsTip2%></li>
               <li><%=MailResource.BlankModalTagsTip3%></li>
           </ul>
       </div>
       <div class="module-block">
           <div class="img crm"></div>
           <div class="title"><%=MailResource.BlankModalCRMTitle%></div>
           <ul>
               <li><%=MailResource.BlankModalCRMTip1%></li>
               <li><%=MailResource.BlankModalCRMTip2%></li>
               <li><%=MailResource.BlankModalCRMTip3%></li>
           </ul>
       </div>
    </div>
    <div class="dashboard-buttons">
        <a class="button huge create-button" href="#" onclick="blankModal.addAccount();"><%=MailResource.BlankModalCreateBtn%></a>
    </div>
</div>