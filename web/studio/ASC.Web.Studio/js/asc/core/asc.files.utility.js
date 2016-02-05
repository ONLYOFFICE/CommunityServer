/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


if (typeof ASC === 'undefined') {
    ASC = {};
}
if (typeof ASC.Files === 'undefined') {
    ASC.Files = (function () {
        return {};
    })();
}
if (typeof ASC.Files.Utility === 'undefined') {
    ASC.Files.Utility = {};
}

ASC.Files.Utility.FileExtensionLibrary = {
    ArchiveExts: [".zip", ".rar", ".ace", ".arc", ".arj", ".bh", ".cab", ".enc", ".gz", ".ha", ".jar", ".lha", ".lzh", ".pak", ".pk3", ".tar", ".tgz", ".uu", ".uue", ".xxe", ".z", ".zoo"],
    AviExts: [".avi"],
    CsvExts: [".csv"],
    DjvuExts: [".djvu"],
    DocExts: [".doc", ".docx"],
    DoctExts: [".doct"],
    EbookExts: [".epub", ".fb2"],
    FlvExts: [".flv", ".fla"],
    HtmlExts: [".html", ".htm", ".mht"],
    IafExts: [".iaf"],
    ImgExts: [".bmp", ".cod", ".gif", ".ief", ".jpe", ".jpeg", ".jpg", ".jfif", ".tiff", ".tif", ".cmx", ".ico", ".png", ".pnm", ".pbm", ".ppm", ".rgb", ".xbm", ".xpm", ".xwd"],
    M2tsExts: [".m2ts"],
    MkvExts: [".mkv"],
    MovExts: [".mov"],
    Mp4Exts: [".mp4"],
    MpgExts: [".mpg"],
    OdpExts: [".odp"],
    OdsExts: [".ods"],
    OdtExts: [".odt"],
    PdfExts: [".pdf"],
    PpsExts: [".pps", ".ppsx"],
    PptExts: [".ppt", ".pptx"],
    PpttExts: [".pptt"],
    RtfExts: [".rtf"],
    SoundExts: [".mp3", ".wav", ".pcm", ".aiff", ".3gp", ".flac", ".fla", ".cda"],
    SvgExts: [".svg"],
    TxtExts: [".txt"],
    DvdExts: [".vob"],
    XlsExts: [".xls", ".xlsx"],
    XlstExts: [".xlst"],
    XmlExts: [".xml"],
    XpsExts: [".xps"],
    GdocExts: [".gdoc"],
    GsheetExts: [".gsheet"],
    GslidesExts: [".gslides"]
};

ASC.Files.Utility.getCssClassByFileTitle = function (fileTitle, compact) {
    var fileExt = ASC.Files.Utility.GetFileExtension(fileTitle);

    var ext = "file";

    if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.ArchiveExts) != -1)
        ext = "Archive";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.AviExts) != -1)
        ext = "Avi";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.CsvExts) != -1)
        ext = "Csv";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.DjvuExts) != -1)
        ext = "Djvu";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.DocExts) != -1)
        ext = "Doc";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.DoctExts) != -1)
        ext = "Doct";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.EbookExts) != -1)
        ext = "Ebook";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.FlvExts) != -1)
        ext = "Flv";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.HtmlExts) != -1)
        ext = "Html";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.IafExts) != -1)
        ext = "Iaf";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.ImgExts) != -1)
        ext = "Image";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.M2tsExts) != -1)
        ext = "M2ts";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.MkvExts) != -1)
        ext = "Mkv";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.MovExts) != -1)
        ext = "Mov";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.Mp4Exts) != -1)
        ext = "Mp4";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.MpgExts) != -1)
        ext = "Mpg";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.OdpExts) != -1)
        ext = "Odp";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.OdtExts) != -1)
        ext = "Odt";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.OdsExts) != -1)
        ext = "Ods";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.PdfExts) != -1)
        ext = "Pdf";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.PpsExts) != -1)
        ext = "Pps";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.PptExts) != -1)
        ext = "Ppt";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.PpttExts) != -1)
        ext = "Pptt";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.RtfExts) != -1)
        ext = "Rtf";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.SoundExts) != -1)
        ext = "Sound";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.SvgExts) != -1)
        ext = "Svg";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.TxtExts) != -1)
        ext = "Txt";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.DvdExts) != -1)
        ext = "Dvd";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.XlsExts) != -1)
        ext = "Xls";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.XlstExts) != -1)
        ext = "Xlst";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.XmlExts) != -1)
        ext = "Xml";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.XpsExts) != -1)
        ext = "Xps";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.GdocExts) != -1)
        ext = "Gdoc";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.GsheetExts) != -1)
        ext = "Gsheet";
    else if (jq.inArray(fileExt, ASC.Files.Utility.FileExtensionLibrary.GslidesExts) != -1)
        ext = "Gslides";

    return "ftFile_" + (compact === true ? 21 : 32) + " ft_" + ext;
};

ASC.Files.Utility.getFolderCssClass = function (compact) {
    return "ftFolder_" + (compact === true ? 21 : 32);
};

ASC.Files.Utility.CanWebEditBrowser =
    true === (jq.browser.msie && jq.browser.versionCorrect >= 9
        || jq.browser.safari && jq.browser.versionCorrect >= 5
        || jq.browser.mozilla && jq.browser.versionCorrect >= 4
        || jq.browser.chrome && jq.browser.versionCorrect >= 7
        || jq.browser.opera && jq.browser.versionCorrect >= 10.5);

ASC.Files.Utility.GetFileExtension = function (fileTitle) {
    if (typeof fileTitle == "undefined" || fileTitle == null) {
        return "";
    }
    fileTitle = fileTitle.trim();
    var posExt = fileTitle.lastIndexOf(".");
    return 0 <= posExt ? fileTitle.substring(posExt).trim().toLowerCase() : "";
};

ASC.Files.Utility.FileInAnInternalFormat = function (fileTitle) {
    var fileExt = ASC.Files.Utility.GetFileExtension(fileTitle);
    for (var i in ASC.Files.Utility.Resource.InternalFormats) {
        if (ASC.Files.Utility.Resource.InternalFormats[i] === fileExt) {
            return true;
        }
    }
    return false;
};

ASC.Files.Utility.CanImageView = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsImagePreviewed) != -1;
};

ASC.Files.Utility.CanWebView = function (fileTitle) {
    return ASC.Resources.Master.TenantTariffDocsEdition &&
        jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebPreviewed) != -1;
};

ASC.Files.Utility.CanWebEdit = function (fileTitle, withoutMobileDetect) {
    return (
        ASC.Resources.Master.TenantTariffDocsEdition &&
        ASC.Files.Utility.CanWebEditBrowser && Teamlab.profile.isVisitor !== true
            ? jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebEdited) != -1
                && (!jq.browser.mobile || withoutMobileDetect === true)
            : false
    );
};

ASC.Files.Utility.CanCoAuhtoring = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsCoAuthoring) != -1;
};

ASC.Files.Utility.MustConvert = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsMustConvert) != -1;
};

ASC.Files.Utility.GetConvertFormats = function (fileTitle) {
    var curExt = ASC.Files.Utility.GetFileExtension(fileTitle);
    return ASC.Files.Utility.Resource.ExtsConvertible[curExt] || [];
};

ASC.Files.Utility.FileIsArchive = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsArchive) != -1;
};

ASC.Files.Utility.FileIsVideo = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsVideo) != -1;
};

ASC.Files.Utility.FileIsAudio = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsAudio) != -1;
};

ASC.Files.Utility.FileIsImage = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsImage) != -1;
};

ASC.Files.Utility.FileIsSpreadsheet = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsSpreadsheet) != -1;
};

ASC.Files.Utility.FileIsPresentation = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsPresentation) != -1;
};

ASC.Files.Utility.FileIsDocument = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsDocument) != -1;
};

ASC.Files.Utility.GetFileViewUrl = function (fileId, fileVersion) {
    var url = ASC.Files.Utility.Resource.FileViewUrlString.format(encodeURIComponent(fileId));
    if (fileVersion) {
        return url + "&" + ASC.Files.Utility.Resource.ParamVersion + "=" + fileVersion;
    }
    return url;
};

ASC.Files.Utility.GetFileDownloadUrl = function (fileId, fileVersion, convertToExtension) {
    var url = ASC.Files.Utility.Resource.FileDownloadUrlString.format(encodeURIComponent(fileId));
    if (fileVersion) {
        return url + "&" + ASC.Files.Utility.Resource.ParamVersion + "=" + fileVersion;
    }
    if (convertToExtension) {
        return url + "&" + ASC.Files.Utility.Resource.ParamOutType + "=" + convertToExtension;
    }
    return url;
};

ASC.Files.Utility.GetFileWebViewerUrl = function (fileId, fileVersion) {
    var url = ASC.Files.Utility.Resource.FileWebViewerUrlString.format(encodeURIComponent(fileId));
    if (fileVersion) {
        return url + "&" + ASC.Files.Utility.Resource.ParamVersion + "=" + fileVersion;
    }
    return url;
};

ASC.Files.Utility.GetFileWebViewerExternalUrl = function (fileUri, fileTitle, refererUrl) {
    return ASC.Files.Utility.Resource.FileWebViewerExternalUrlString.format(encodeURIComponent(fileUri), encodeURIComponent(fileTitle || ""), encodeURIComponent(refererUrl || ""));
};

ASC.Files.Utility.GetFileWebEditorUrl = function (fileId) {
    return ASC.Files.Utility.Resource.FileWebEditorUrlString.format(encodeURIComponent(fileId));
};

ASC.Files.Utility.GetFileWebEditorExternalUrl = function (fileUri, fileTitle, folderId) {
    return ASC.Files.Utility.Resource.FileWebEditorExternalUrlString.format(encodeURIComponent(fileUri), encodeURIComponent(fileTitle || "")) +
        ((folderId || "") ? ("&folderid=" + folderId) : "");
};