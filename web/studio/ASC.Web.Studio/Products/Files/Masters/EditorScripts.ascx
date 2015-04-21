<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="ASC.Web.Core.Client.Bundling.ResourceBundleControl" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%--Third party--%>
<script type="text/javascript" language="javascript" src="<%= ResolveUrl("~/js/third-party/jquery/jquery.core.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= ResolveUrl("~/js/asc/core/localstorage.js") %>"></script>
<%--Common--%>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("common.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("servicemanager.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("editor.js") %>"></script>
