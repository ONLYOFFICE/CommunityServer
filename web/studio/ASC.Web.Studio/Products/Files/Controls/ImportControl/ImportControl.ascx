<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportControl.ascx.cs" Inherits="ASC.Web.Files.Controls.ImportControl" %>
<%@ Import Namespace="ASC.Web.Files.Import" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<% if (ImportConfiguration.SupportImport)%>
<%
   { %>
<div id="importLoginDialog" class="popup-modal">
    <sc:Container id="LoginDialogTemp" runat="server">
        <header><%=FilesUCResource.ImportFromZoho%></header>
        <body>
            <div class="field-panel">
                <div><%=FilesUCResource.Login%></div>
                <input type="text" id="filesImportLogin" class="textEdit" />
            </div>
            <div>
                <div><%=FilesUCResource.Password%></div>
                <input type="password" id="filesImportPass" class="textEdit" />
            </div>
            <div class="middle-button-container">
                <a id="filesSubmitLoginDialog" class="button blue middle">
                    <%=FilesUCResource.ButtonOk%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog();return false;">
                    <%=FilesUCResource.ButtonCancel%>
                </a>
            </div>
        </body>
    </sc:Container>
</div>

<div id="importDialog" class="popup-modal">
    <sc:Container id="ImportDialogTemp" runat="server">
        <header>
            <span id="importDialogHeader" class="header-content"></span>
        </header>
        <body>
            <div id="importData">
            </div>
            <div id="importToFolder">
                <%=FilesUCResource.ImportToFolder%>:
                <a id="importToFolderBtn" class="import-tree"></a>
                <span id="importToFolderTitle"><%=FilesUCResource.MyFiles%></span>
                <span>&nbsp;>&nbsp;</span>
                <span id="importToFolderName"></span>
                <input type="hidden" id="files_importToFolderId" value="" />
            </div>
            <div>
                <%=FilesUCResource.IfFileNameConflict%>:
                <label>
                    <input type="radio" name="file_conflict" value="true" checked="checked" />
                    <%=FilesUCResource.Overwrite%>
                </label>
                <label>
                    <input type="radio" name="file_conflict" value="false" />
                    <%=FilesUCResource.Ignore%>
                </label>
            </div>
            <div class="middle-button-container">
                <a id="startImportData" class="button blue middle">
                    <%=FilesUCResource.ButtonImport%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%=FilesUCResource.ButtonCancel%>
                </a>
            </div>
        </body>
    </sc:Container>
</div>
<% } %>