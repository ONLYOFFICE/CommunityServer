<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<script type="text/javascript" language="javascript">
    (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){(i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)})(window,document,'script','//www.google-analytics.com/analytics.js','ga');

    ga('create', 'UA-12442749-29', 'auto', { 'name': 'www', 'allowLinker': true });
    ga('require', 'linker');
    ga('www.linker:autoLink', ['onlyoffice.com', 'onlyoffice.eu', 'onlyoffice.sg', 'avangate.com'], false, true);
    ga('www.set', 'userId', '<%= CoreContext.TenantManager.GetCurrentTenant().OwnerId %>');
    ga('www.set', 'userIdCurrent', '<%= SecurityContext.CurrentAccount.ID %>');
    ga('www.set', 'tenantId', '<%= TenantProvider.CurrentTenantID %>');
    ga('www.send', 'pageview');
</script>
