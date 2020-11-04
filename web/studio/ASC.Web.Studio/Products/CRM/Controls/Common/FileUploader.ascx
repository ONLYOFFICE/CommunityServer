<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileUploader.ascx.cs" Inherits="ASC.Web.CRM.Controls.Common.FileUploader" %>

<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<div id='pm_DragDropHolder' style="border:1px solid #d1d1d1; padding:15px 15px 25px;"> 
    <table cellpadding="0" cellspacing="0" border="0">
        <tr valign="middle">
            <td style="width:50px; padding:5px 0 0 10px;">
                <div class="pm_uploadIcon"></div>
            </td>
            <td height="20"> 
                <div class="describeUpload">
                    <%=ASC.Web.Studio.Core.FileSizeComment.GetFileSizeNote()%>
                </div>
            </td>
        </tr>
    </table>
    <div id="pm_overallprocessHolder">
    </div>
    <div id="history_uploadContainer" class="history_uploadContainer">
    </div>
    <div id="pm_upload_pnl" style="padding:15px 0 10px;">
        <div class="clearFix" id="pm_swf_button_container" style="position:relative;">
            <a id="pm_upload_btn" class="button gray middle pm_upload_btn"><%= CRMCommonResource.UploadFile%></a>
        </div>
    </div>
</div>


