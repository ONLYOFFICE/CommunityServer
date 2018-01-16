<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<script type="text/javascript" language="javascript">
    ga('create', 'UA-12442749-29', 'auto', { 'name': 'www', 'allowLinker': true });
    ga('require', 'linker');
    ga('www.linker:autoLink', ['onlyoffice.com', 'onlyoffice.eu', 'onlyoffice.sg', 'avangate.com'], false, true);
    ga('www.set', 'userId', '<%= SecurityContext.CurrentAccount.ID %>');
    ga('www.set', 'tenantId', '<%= TenantProvider.CurrentTenantID %>');
    ga('www.send', 'pageview');
</script>
