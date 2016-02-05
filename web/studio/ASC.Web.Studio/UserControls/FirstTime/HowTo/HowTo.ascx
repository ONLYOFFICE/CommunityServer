<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HowTo.ascx.cs" Inherits="ASC.Web.Studio.UserControls.FirstTime.HowTo" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<div class="howtoBlock clearFix">
    <div class="video">
        <div class="howToVideoBlock title"><%=Resources.Resource.HowToVideo %></div>
         <a class="basecampImportLink" href="http://www.onlyoffice.com/help/tipstricks/basecamp-import.aspx" target="_blank">
         <img src='<%=ASC.Data.Storage.WebPath.GetPath("usercontrols/firsttime/howto/images/free_proj_manag.png")%>' width="201px" height="109px"  />
         </a>
    </div>
    <div class="articles">
        <div class="title">
            <%: Resources.Resource.HowToArticles %></div>
        <a href="http://www.onlyoffice.com/help/tipstricks/basecamp-import.aspx" target="_blank">
            <%: Resources.Resource.HowToImportBaseCamp %></a><br />
        <a href="http://www.onlyoffice.com/help/tipstricks/importing-files.aspx" target="_blank">
            <%: Resources.Resource.HowToImportFiles %></a><br />
        <a href="http://www.onlyoffice.com/help/video/import-google-docs.aspx" target="_blank">
            <%: Resources.Resource.HowToImportGoogle %></a><br />
        <a href="http://www.onlyoffice.com/help/video/import-boxnet-docs.aspx" target="_blank">
            <%: Resources.Resource.HowToImportBox %></a><br />
        <a href="http://www.onlyoffice.com/help/video/import-zoho-docs.aspx" target="_blank">
            <%: Resources.Resource.HowToImportZoho %></a><br />
        <a href="http://www.onlyoffice.com/help/administratorguides/import-contacts-from-web.aspx"
            target="_blank">H<%: Resources.Resource.HowToImportWorld %></a><br />
        <a href="http://www.onlyoffice.com/help/administratorguides/import-contacts-from-csv.aspx"
            target="_blank">
            <%: Resources.Resource.HowToImportCSV %></a><br />
        <a href="http://www.onlyoffice.com/help/administratorguides/import-contacts-from-mail-client.aspx"
            target="_blank">
            <%: Resources.Resource.HowToImportMailClient %></a><br />
    </div>
</div>
