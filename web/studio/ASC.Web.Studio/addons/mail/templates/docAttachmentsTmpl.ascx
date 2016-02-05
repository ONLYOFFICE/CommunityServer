<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<script id="docAttachTmpl" type="text/x-jquery-tmpl">
    {{if type=='folder'}}
        <li title='${title}' onclick='DocumentsPopup.openFolder("${id}");' id='${id}'>
            <span class='ftFolder_21 folder'>${title}</span>
        </li>
    {{else}}
            <li title='${title} (${size_string})' onclick='DocumentsPopup.selectFile("${id}");'>
                <input type='checkbox' onclick='DocumentsPopup.selectFile("${id}");' version='${version}' access='${access}' id='${id}' size='${size}'/>
                <label class='${exttype}'>${title}</label>
                {{if access == ASC.Files.Constants.AceStatusEnum.None || access == ASC.Files.Constants.AceStatusEnum.ReadWrite }}
                    <span class="share-can"></span>
                {{/if}}
            </li>
    {{/if}}
</script>

<script id="messageFileLink" type="text/x-jquery-tmpl">
    <div dir="ltr">
        <div contenteditable="false" data-fileid="${id}" class="mailmessage-filelink" style="width: 350px; height: 27px; max-height: 27px; 
            background-color: #f2f2f2; color: #333; font-family: arial; font-style:normal; font-weight: normal; font-size: 12px; cursor: default; margin-bottom: 10px;">
            <a href="${webUrl}" title="${fileName + ext}" target="_blank" data-fileid="${id}" class="mailmessage-filelink-link" style="display: inline-block; 
                overflow: hidden; text-overflow: ellipsis; white-space: nowrap; text-decoration: none; border: none; width: 310px; 
                cursor: pointer; height: 25px; color: #333;">
                <img style="vertical-align: bottom; border: none; cursor: pointer; margin: 3px 0 0 3px; width: 21px; height: 21px; display: inline-block; float: left;"
                {{if jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.ArchiveExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_archive_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.AviExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_avi_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.CsvExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_csv_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.DjvuExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_djvu_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.DocExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_doc_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.DoctExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_doct_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.EbookExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_ebook_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.FlvExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_flv_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.HtmlExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_html_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.IafExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_iaf_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.ImgExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_image_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.M2tsExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_m2ts_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.MkvExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_mkv_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.MovExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_mov_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.Mp4Exts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_mp4_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.MpgExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_mpg_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.OdpExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_odp_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.OdtExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_odt_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.OdsExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_ods_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.PdfExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_pdf_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.PpsExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_pps_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.PptExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_ppt_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.PpttExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_pptt_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.RtfExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_rtf_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.SoundExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_sound_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.SvgExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_svg_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.TxtExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_txt_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.DvdExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_dvd_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.XlsExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_xls_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.XlstExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_xlst_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.XmlExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_xml_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.XpsExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_xps_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.GdocExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_gdoc_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.GsheetExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_gsheet_21.png") %>"
                {{else jq.inArray(ext, ASC.Files.Utility.FileExtensionLibrary.GslidesExts) != -1 }}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_gslides_21.png") %>"
                {{else}}
                src="<%= CommonLinkUtility.GetFullAbsolutePath("~/usercontrols/common/ckeditor/plugins/filetype/images/file_21.png") %>"
                {{/if}}
                ></img>
                <span style="display: inline-block; margin: 7px 0 0 3px;">
                    <span style="max-width: 260px; text-decoration:none; display:inline-block;" class="file-name">${fileName}</span>
                    <span style="text-decoration:none; color:#a2a2a2; margin-left:-3px; display:inline-block;">${ext}</span>
                </span>
            </a>
            <div class="delete-btn"></div>
        </div>
    </div>
</script>

<script id="filesCannotBeAttachedAsLinksTmpl" type="text/x-jquery-tmpl">
    <div id="filesCannotBeAttachedAsLinksBox">
        
        <div class="warning-box"></div>
        <div class="description-box">
            <div class="header"><%= MailResource.FilesCannotBeAttachedAsLinks_Header %></div>
            <div><%= string.Format(MailResource.FilesCannotBeAttachedAsLinks_Body, "<br/>") %></div>
        </div>
        
        <div class="buttons">
            <button class="button middle blue" type="button" onclick="window.AttachmentManager.CopyFilesToMyDocumentsAndInsertFileLinksToMessage();">
                <%= MailResource.CopyFilesToMyDocumentsBtn %>
            </button>
            <button class="button middle gray cancel" type="button" onclick="window.AttachmentManager.CancelCopyingFilesToMyDocuments()">
                <%= MailScriptResource.CancelBtnLabel %>
            </button>
        </div>
    </div>
</script>

<script id="sharingSettingForFileLinksTmpl" type="text/x-jquery-tmpl">
    <div id="sharingSettingForFileLinksTmplBox">
        <div class="header"><%= MailResource.SharingSettingForFileLinks_Header %></div>
        
        <div class="radiobox-switcher">
            <label for="shareReadFileLinks" class="radiobox">
                <input id="shareReadFileLinks" name="shareFileLinksAccessSelector" type="radio" checked="checked" value="${ASC.Files.Constants.AceStatusEnum.Read}" />
                <%= MailResource.ReadOnly %>
            </label>
            <label for="shareReadWriteFileLinks" class="radiobox">
                <input id="shareReadWriteFileLinks" name="shareFileLinksAccessSelector" type="radio" value="${ASC.Files.Constants.AceStatusEnum.ReadWrite}" />
                <%= MailResource.FullAccess %>
            </label>
        </div>

        <div class="description"><%= MailResource.SharingSettingForFileLinks_Body %></div>
        
        <div class="buttons">
            <button class="button middle blue" type="button" onclick="window.messagePage.sendAction(null, true); window.popup.hide();">
                <%= MailResource.SaveAndSendMessageBtn %>
            </button>
            <button class="button middle gray cancel" type="button">
                <%= MailScriptResource.CancelBtnLabel %>
            </button>
        </div>
    </div>
</script>