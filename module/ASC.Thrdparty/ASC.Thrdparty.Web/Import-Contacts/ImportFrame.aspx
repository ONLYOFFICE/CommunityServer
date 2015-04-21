<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Popup.Master"  ValidateRequest="false" CodeBehind="ImportFrame.aspx.cs" Inherits="ASC.Thrdparty.Web.ImportFrame" %>
<%@ Import Namespace="ASC.Thrdparty.Web" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
<script src="//ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js" type="text/javascript"></script>
<meta charset="utf-8" />
    <!--[if IE]><meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1"><![endif]-->
    
    
    <link rel="stylesheet" href="<%=ResolveUrl("~/content/popup.css") %>" type="text/css" />
    <title></title>
</asp:Content>	
<asp:Content ID="Content1" ContentPlaceHolderID="bodyContent" runat="server">
    <div class="clearFix buttons <%= Mobile()?"buttonsPad":"" %>">
        <div class="google" onclick="window.master.open('<%=ResolveUrl("~/Google/GoogleImportContacts.aspx")%>');"
            title="<%=Resources.ContactsServiceResource.ImportFromGoogle%>">
        </div>
      
          <div class="yahoo" onclick="window.master.open('<%=ResolveUrl("~/Yahoo/YahooImport.aspx") %>');"
            title="<%=Resources.ContactsServiceResource.ImportFromYahoo%>">
        </div>
        <%--<div class="live" onclick="window.master.open('<%=ResolveUrl("~/Live/WindowsLive.aspx") %>');"
            title="<%=Resources.ContactsServiceResource.ImportFromWindowsLive%>">
            Windows Live
        </div>--%>
    </div>
    
     <script type="text/javascript">
         document.domain = '<%=SetupInfo.Domain%>';
         window.master = {
             winName: 'ImportContacts',
             params: 'width=800,height=500,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no',
             init: function() {
                 if (window.addEventListener) {
                     window.addEventListener('message', master.listener, false);
                 } else {
                     window.attachEvent('onmessage', master.listener);
                 }
             },
             callback: function (msg) {
                 var data = JSON.parse(msg);
                 var obj = { "Tpr": "Importer", "Data": data.msg, "error": data.error };
                 window.parent.postMessage(JSON.stringify(obj), '<%=Request.UrlReferrer%>');
             },

             open: function(url) {
                 window.open(url, this.winName, this.params);
             }
         };
         master.init();

  </script>
</asp:Content>
