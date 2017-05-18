<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectDocumentsPopup.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.ProjectDocumentsPopup.ProjectDocumentsPopup" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="Resources" %>

<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<sc:Container id="_documentUploader" runat="server">
    <header>
    <%=PopupName %>    
    </header>
    <body>
        <div class="popupContainerBreadCrumbs">
                
        </div>
        <p class="containerCheckAll display-none"><input type="checkbox" title="<%=UserControlsCommonResource.CheckAll%>" id="checkAll"/><label for="checkAll"><%=UserControlsCommonResource.CheckAll%></label></p>
        <div class="fileContainer">
            <img class="loader" src="<%= WebImageSupplier.GetAbsoluteWebPath("loader_32.gif")%>"/>
            <div id="emptyFileList" class="display-none" data-imgSrc="<%= VirtualPathUtility.ToAbsolute("~/UserControls/Common/ProjectDocumentsPopup/Images/project-documents.png") %>">
            </div>
            <ul class='fileList'>
            </ul>
        </div>
        <div class="buttonContainer">
            <a class="button blue disable"><%=UserControlsCommonResource.AttachFiles %></a>
            <span class="splitter-buttons"></span>
            <a class="button gray"><%=UserControlsCommonResource.CancelButton %></a>
        </div>
    </body>
    </sc:Container>