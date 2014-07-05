<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Assembly Name="ASC.Data.Storage" %>

<script type="text/javascript" src="<%=ResolveUrl("~/js/third-party/jquery/jquery.autosize.js") %>"></script>
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("jq_projects_extensions.js") %>"></script>  
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("common.js") %>"></script> 
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("taskaction.js") %>"></script> 
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("milestoneaction.js") %>"></script> 
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("ganttchart_min.js") %>"></script>
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("ganttchartpage.js") %>"></script>  