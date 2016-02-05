<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Assembly Name="ASC.Data.Storage" %>

<!--common -->
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("jq_projects_extensions.js") %>"></script>  
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("common.js") %>"></script> 
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("base.js") %>"></script> 
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("navsidepanel.js") %>"></script>
<script type="text/javascript" src="<%=ResolveUrl("~/js/third-party/jquery/jquery.autosize.js") %>"></script>
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("taskaction.js") %>"></script>
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("milestoneaction.js") %>"></script>
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("projectnavpanel.js") %>"></script>  
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("common_filter_projects.js") %>"></script>
<script type="text/javascript" src="<%=ResolveUrl("~/js/uploader/ajaxupload.js") %>"></script>
<!--tasks -->
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("subtasks.js") %>"></script>
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("tasks.js") %>"></script>  
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("taskdescription.js") %>"></script>
<!--projects -->
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("projects.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("projecttemplates.js") %>"></script>
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("projectteam.js") %>"></script>
<!--milestones -->
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("milestones.js") %>"></script>
<!--discussions -->
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("discussions.js") %>"></script>
<!--timetracking -->
<script type="text/javascript" src="<%=PathProvider.GetFileStaticRelativePath("timetracking.js") %>"></script>  
<script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("apitimetraking.js") %>"></script>