<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>

<link rel="stylesheet" href="<%= PathProvider.GetFileStaticRelativePath("common.css") %>" />
<link rel="stylesheet" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/maincontent/maincontent.css" %>" />
<link rel="stylesheet" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/emptyfolder/emptyfolder.css" %>" />
<link rel="stylesheet" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/accessrights/accessrights.css" %>" />
<link rel="stylesheet" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/fileviewer/fileviewer.css" %>" />
<link rel="stylesheet" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/importcontrol/importcontrol.css" %>" />
<link rel="stylesheet" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/thirdparty/thirdparty.css" %>" />
<link rel="stylesheet" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/convertfile/convertfile.css" %>" />
<link rel="stylesheet" href="<%= FilesLinkUtility.FilesBaseAbsolutePath + "controls/chunkuploaddialog/chunkuploaddialog.css" %>" />
