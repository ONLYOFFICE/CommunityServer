<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SupportChat.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Support.SupportChat" %>

<% if (!string.IsNullOrEmpty(SupportKey))
   { %>
<!--Start of Zendesk Chat Script-->
<script type="text/javascript">
window.$zopim||(function(d,s){var z=$zopim=function(c){z._.push(c)},$=z.s=
d.createElement(s),e=d.getElementsByTagName(s)[0];z.set=function(o){z.set.
_.push(o)};z._=[];z.set._=[];$.async=!0;$.setAttribute("charset","utf-8");
$.src="https://v2.zopim.com/?<%: SupportKey %>";z.t=+new Date;$.
type="text/javascript";e.parentNode.insertBefore($,e)})(document,"script");
</script>
<!--End of Zendesk Chat Script-->
<% } %>
