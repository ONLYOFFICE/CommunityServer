<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>

<%--Third party--%>

<script type="text/javascript" src="<%= ResolveUrl("~/js/third-party/jquery/jquery.mousewheel.js") %>"></script>
<script type="text/javascript" src="<%= ResolveUrl("~/js/third-party/jquery/jquery.uri.js") %>"></script>

<script type="text/javascript" src="<%= ResolveUrl("~/js/third-party/sorttable.js") %>"></script>

<%--Common--%>

<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("auth.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("common.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("filter.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("templatemanager.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("servicemanager.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("ui.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("mousemanager.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("markernew.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("actionmanager.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("anchormanager.js") %>"></script>
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("foldermanager.js") %>"></script>

<%--Controls--%>

<script type="text/javascript" src="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/createmenu/createmenu.js" %>"></script>
<script type="text/javascript" src="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/fileviewer/fileviewer.js" %>"></script>
<script type="text/javascript" src="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/convertfile/convertfile.js" %>"></script>
<script type="text/javascript" src="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/chunkuploaddialog/chunkuploadmanager.js" %>"></script>