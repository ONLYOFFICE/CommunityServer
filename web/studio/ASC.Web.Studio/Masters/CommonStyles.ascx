<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>

<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/jquery_style.css") %>"/>
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/common_style.css") %>" />
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/filetype_style.css") %>" />
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/magnific-popup.css") %>" />
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/magnific-popup.less") %>" />
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/toastr.css") %>" />
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/groupselector.css") %>" />
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/advuserselector.css") %>" />
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/jquery-advansedfilter.css") %>"/>
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/jquery-advansedselector.css") %>"/>
<link type="text/css" rel="stylesheet" href="<%= ResolveUrl("~/skins/default/codestyle.css") %>" />

<% if (WebSkin.HasCurrentCultureCssFile) { %>
<link type="text/css" rel="stylesheet"  href="<%= ResolveUrl(("~/skins/default/common_style.css").Replace("css", CultureInfo.CurrentCulture.Name.ToLower() + ".css")) %>"/>
<% } %>