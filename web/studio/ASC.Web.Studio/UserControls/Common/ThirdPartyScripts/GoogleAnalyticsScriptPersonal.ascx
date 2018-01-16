<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="ASC.Core" %>

<script type="text/javascript" language="javascript">
    ga('create', 'UA-12442749-17', 'auto', { 'name': 'www', 'allowLinker': true });
    ga('require', 'linker');
    ga('www.linker:autoLink', ['onlyoffice.com', 'onlyoffice.eu', 'onlyoffice.sg', 'avangate.com'], false, true);
    <% if (SecurityContext.IsAuthenticated)
       { %>
    ga('www.set', 'userId', '<%= SecurityContext.CurrentAccount.ID %>');
    <% } %>
    ga('www.send', 'pageview');
</script>
