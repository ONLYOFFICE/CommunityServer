<%@ Control Language="C#" AutoEventWireup="true" %>
<script type="text/javascript" language="javascript">
    ga('create', 'UA-12442749-21', 'auto', { 'name': 'testTracker', 'userId': '<%=ASC.Core.SecurityContext.CurrentAccount.ID %>' });
    ga('testTracker.send', 'pageview');
</script>
