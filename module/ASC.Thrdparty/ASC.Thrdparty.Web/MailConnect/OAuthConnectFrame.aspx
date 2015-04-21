<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Popup.Master"  ValidateRequest="false" CodeBehind="OAuthConnectFrame.aspx.cs" Inherits="ASC.Thrdparty.Web.MailConnect.OAuthConnectFrame" %>
<%@ Import Namespace="ASC.Thrdparty.Web" %>


<asp:Content ID="Content1" ContentPlaceHolderID="bodyContent" runat="server">
    <div class="oAuthBlock"onclick="window.master.open('<%=ResolveUrl("~/Google/GoogleOAuthConnect.aspx?access_type=offline")%>');">
        <div class="googleOAuthImage"></div>
        <div>
            <span class="oAuuthLabel"><%= Resources.ContactsServiceResource.GoogleLabel%></span>
            <span class="oAuuthConnectLabel"><%= Resources.ContactsServiceResource.ConnectLabel%></span>
        </div>
    </div>
    
     <script type="text/javascript">
         document.domain = '<%=SetupInfo.Domain%>';
         window.master = {
             winName: 'ImportMails',
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
                 var obj = {"Tpr": "OAuthImporter", "Data": data.msg, "error": data.error};
                 window.parent.postMessage(JSON.stringify(obj), '<%=Request.UrlReferrer%>');
             },

             open: function(url) {
                 window.open(url, this.winName, this.params);
             }
         };
         master.init();

  </script>
</asp:Content>
