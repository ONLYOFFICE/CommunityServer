<%@ Assembly Name="ASC.Web.Core" %>
<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConvertFile.ascx.cs" Inherits="ASC.Web.Files.Controls.ConvertFile" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register TagPrefix="sc" Namespace="ASC.Web.Studio.Controls.Common" Assembly="ASC.Web.Studio" %>

<%--dialog window--%>
<div id="confirmCopyConvert" class="popup-modal">
    <sc:Container runat="server" ID="confirmCopyConvertDialog">
        <header>
            <%= FilesUCResource.CaptionCopyConvertOpen %>
        </header>
        <body>
            <div id="copyConvertDescript" class="confirm-convert-panel">
                <%= FilesUCResource.ConfirmStoreOriginalOpenTitle.HtmlEncode() %>
                <br/>
                <br/>
                <label id="confirmCopyConvertLabelText">
                    <input type="checkbox" class="store-original checkbox" <%= FilesSettings.StoreOriginalFiles ? "checked=\"checked\"" : string.Empty %> />
                    <%= FilesUCResource.ConfirmStoreOriginalOpenCbxLabelText %>
                </label>
                <div id="confirmCopyConvertToMyText">
                    <%= string.Format(FilesUCResource.ConfirmStoreOriginalOpenToMyText, "<b>", "</b>")%>
                </div>
            </div>

            <div id="convertPasswordPanel" class="confirm-convert-panel">
                <%= FilesUCResource.CaptionConvertPassword %>
                <br/>
                <br/>
                <input type="password" id="convertPassword" class="textEdit" name="convert-field" autocomplete="new-password" />
            </div>

            <div id="progressCopyConvert">
                <div class="files-progress-box">
                    <input type="hidden" id="progressCopyConvertId" />
                    <input type="hidden" id="progressCopyConvertVersion" />
                    <input type="hidden" id="progressCopyConvertView" />
                    <progress max="100" value="0" >
                        <div class="asc-progress-wrapper">
                            <div class="asc-progress-value"></div>
                        </div>
                    </progress>
                    <div class="asc-progress-percent">0</div>
                </div>
                <span id="progressCopyConvertRun" class="convert-status"><%= FilesUCResource.CopyConvertExec %></span>
                <span id="progressCopyConvertError" class="convert-status red-text"><%= FilesUCResource.CopyConvertError %></span>
                <span id="progressCopyConvertEnd" class="convert-status"><%= FilesUCResource.CopyConvertEnd %></span>
                <span id="progressCopyConvertEndTo" class="convert-status">
                    <%= string.Format(FilesUCResource.CopyConvertEndTo, "<span class=\"convert-end-to\"></span>") %>
                </span>
            </div>

            <div class="middle-button-container">
                <a id="confirmCopyAndConvert" class="button blue middle">
                    <%= FilesUCResource.Convert %>
                </a>
                <a id="copyAndConvertOpen" class="button blue middle disable">
                    <%= FilesUCResource.ButtonOpenFile %>
                </a>
                <span id="goToCopySplitter" class="splitter-buttons"></span>
                <a id="goToCopyFolder" class="button blue middle">
                    <%= string.Format(FilesUCResource.ButtonGotoFolder, "<span class=\"convert-end-to\"></span>") %>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%= FilesUCResource.ButtonClose %>
                </a>
            </div>

        </body>
    </sc:Container>
</div>

<div id="convertAndDownload" class="popup-modal">
    <sc:Container id="convertAndDownloadDialog" runat="server">
        <header><%=FilesUCResource.ConvertAndDownload%></header>
        <body>
            <div><%=FilesUCResource.ChooseFormatToDownload%></div>
            <div id="convertFileList" class="compact cnvrt-list-file-block"></div>
            <div id="convertFileZip"><%= String.Format(FilesUCResource.FilesWillBeCompressed, "<b>", "</b>")%></div>
            <div class="text-medium-describe"><%= FilesUCResource.LostDataConvert %></div>
            <div class="middle-button-container">
                <a id="buttonStartConvert" class="button blue middle">
                    <%=FilesUCResource.ButtonDownload%>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button gray middle" href="" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                    <%=FilesUCResource.ButtonCancel%>
                </a>
            </div>
        </body>
    </sc:Container>
</div>
