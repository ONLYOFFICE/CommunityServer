<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<script type="text/javascript" language="javascript">
    ga('create', 'UA-12442749-5', 'auto', { 'name': 'www', 'allowLinker': true });
    ga('require', 'linker');
    ga('www.linker:autoLink', ['onlyoffice.com', 'onlyoffice.eu', 'onlyoffice.sg', 'avangate.com'], false, true);
    ga('www.set', 'userId', '<%= CoreContext.TenantManager.GetCurrentTenant().OwnerId %>');
    <% if (SecurityContext.IsAuthenticated)
       { %>
    ga('www.set', 'userIdCurrent', '<%= SecurityContext.CurrentAccount.ID %>');
    <% } %>
    ga('www.set', 'tenantId', '<%= TenantProvider.CurrentTenantID %>');
    ga('www.send', 'pageview');

    ga('create', 'UA-12442749-21', 'auto', { 'name': 'testTracker', 'allowLinker': true });
    ga('require', 'linker');
    ga('testTracker.linker:autoLink', ['onlyoffice.com', 'onlyoffice.eu', 'onlyoffice.sg', 'avangate.com'], false, true);
    ga('testTracker.set', 'userId', '<%= CoreContext.TenantManager.GetCurrentTenant().OwnerId %>');
    <% if (SecurityContext.IsAuthenticated)
       { %>
    ga('testTracker.set', 'userIdCurrent', '<%= SecurityContext.CurrentAccount.ID %>');
    <% } %>
    ga('testTracker.set', 'tenantId', '<%= TenantProvider.CurrentTenantID %>');
    ga('testTracker.send', 'pageview');
</script>
