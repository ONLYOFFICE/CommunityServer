<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Data.Storage" %>

<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("common.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("navsidepanel.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("fileUploader.js") %>"></script>
<script type="text/javascript" src="<%= ResolveUrl("~/js/uploader/ajaxupload.js") %>"></script>

<script type="text/javascript" src="<%= ResolveUrl("~/js/third-party/jquery/jquery.autosize.js") %>"></script>

<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("tasks.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("contacts.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("cases.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("deals.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("invoices.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("socialmedia.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("sender.js") %>"></script>
