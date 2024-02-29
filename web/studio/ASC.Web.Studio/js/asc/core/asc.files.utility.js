/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
    CalendarExts: [".ical", ".ifb", ".icalendar"],
    CsvExts: [".csv"],
    DjvuExts: [".djvu"],
    DocExts: [".doc"],
    DocmExts: [".docm"],
    DoctExts: [".doct"],
    DocxExts: [".docx"],
    DocxfExts: [".docxf"],
    DotExts: [".dot"],
    DotmExts: [".dotm"],
    DotxExts: [".dotx"],
    DpsExts: [".dps"],
    DptExts: [".dpt"],
    DvdExts: [".dvd"],
    //EbookExts: [".epub", ".fb2"],
    EpubExts: [".epub"],
    EtExts: [".et"],
    EttExts: [".ett"],
    Fb2Exts: [".fb2"],
    FlvExts: [".flv"],
    FodpExts: [".fodp"],
    FodsExts: [".fods"],
    FodtExts: [".fodt"],
    GdocExts: [".gdoc"],
    GsheetExts: [".gsheet"],
    GslidesExts: [".gslides"],
    HtmExts: [".htm"],
    HtmlExts: [".html"],
    IafExts: [".iaf"],
    IcsExts: [".ics"],
    ImgExts: [".bmp", ".cod", ".gif", ".ief", ".jpe", ".jpeg", ".jpg", ".jfif", ".tiff", ".tif", ".cmx", ".ico", ".png", ".pnm", ".pbm", ".ppm", ".rgb", ".xbm", ".xpm", ".xwd", ".webp"],
    M2tsExts: [".m2ts"],
    MhtExts: [".mht"],
    MhtmlExts: [".mhtml"],
    MkvExts: [".mkv"],
    MovExts: [".mov"],
    Mp4Exts: [".mp4"],
    MpgExts: [".mpg"],
    OdpExts: [".odp"],
    OdsExts: [".ods"],
    OdtExts: [".odt"],
    OformExts: [".oform"],
    OtpExts: [".otp"],
    OtsExts: [".ots"],
    OttExts: [".ott"],
    PdfExts: [".pdf"],
    PotExts: [".pot"],
    PotmExts: [".potm"],
    PotxExts: [".potx"],
    PpsExts: [".pps"],
    PpsmExts: [".ppsm"],
    PpsxExts: [".ppsx"],
    PptExts: [".ppt"],
    PptmExts: [".pptm"],
    PpttExts: [".pptt"],
    PptxExts: [".pptx"],
    RtfExts: [".rtf"],
    SoundExts: [".aac", ".flac", ".m4a", ".mp3", ".oga", ".ogg", ".wav"],
    SoundUnkExts: [".ac3", ".aiff", ".amr", ".ape", ".cda", ".mid", ".mka", ".mpc", ".pcm", ".ra", ".raw", ".wma"],
    StwExts: [".stw"],
    SvgExts: [".svg"],
    SxcExts: [".sxc"],
    SxiExts: [".sxi"],
    SxwExts: [".sxw"],
    TxtExts: [".txt"],
    VideoExts: [".f4v", ".m4v", ".mpeg", ".ogv", ".webm", ".wmv" ],
    VideoUnkExts: [".3gp", ".asf", ".fla", ".mts", ".svi", ".vob"],
    WpsExts: [".wps"],
    WptExts: [".wpt"],
    XlsExts: [".xls", ".xlsb"],
    XlsmExts: [".xlsm"],
    XlstExts: [".xlst"],
    XlsxExts: [".xlsx"],
    XltExts: [".xlt"],
    XltmExts: [".xltm"],
    XltxExts: [".xltx"],
    XmlExts: [".xml"],
    XpsExts: [".xps", ".oxps"]
};

ASC.Files.Utility.ExternalFolderShareKey = 'share';

ASC.Files.Utility.getCssClassByFileTitle = function (fileTitle, compact) {
    var utility = ASC.Files.Utility,
        fileExtensionLibrary = utility.FileExtensionLibrary,
        fileExt = utility.GetFileExtension(fileTitle),
        ext = "file",
        checkInArray = function(extsArray) {
            return jq.inArray(fileExt, extsArray) != -1;
        };


    if (checkInArray(fileExtensionLibrary.ArchiveExts))
        ext = "Archive";
    else if (checkInArray(fileExtensionLibrary.AviExts))
        ext = "Avi";
    else if (checkInArray(fileExtensionLibrary.CalendarExts))
        ext = "Cal";
    else if (checkInArray(fileExtensionLibrary.CsvExts))
        ext = "Csv";
    else if (checkInArray(fileExtensionLibrary.DjvuExts))
        ext = "Djvu";
    else if (checkInArray(fileExtensionLibrary.DocExts))
        ext = "Doc";
    else if (checkInArray(fileExtensionLibrary.DocmExts))
        ext = "Docm";
    else if (checkInArray(fileExtensionLibrary.DoctExts))
        ext = "Doct";
    else if (checkInArray(fileExtensionLibrary.DocxExts))
        ext = "Docx";
    else if (checkInArray(fileExtensionLibrary.DocxfExts))
        ext = "Docxf";
    else if (checkInArray(fileExtensionLibrary.DotExts))
        ext = "Dot";
    else if (checkInArray(fileExtensionLibrary.DotmExts))
        ext = "Dotm";
    else if (checkInArray(fileExtensionLibrary.DotxExts))
        ext = "Dotx";
    else if (checkInArray(fileExtensionLibrary.DpsExts))
        ext = "Dps";
    else if (checkInArray(fileExtensionLibrary.DptExts))
        ext = "Dpt";
    else if (checkInArray(fileExtensionLibrary.DvdExts))
        ext = "Dvd";
    //else if (checkInArray(fileExtensionLibrary.EbookExts))
    //    ext = "Ebook";
    else if (checkInArray(fileExtensionLibrary.EpubExts))
        ext = "Epub";
    else if (checkInArray(fileExtensionLibrary.EtExts))
        ext = "Et";
    else if (checkInArray(fileExtensionLibrary.EttExts))
        ext = "Ett";
    else if (checkInArray(fileExtensionLibrary.Fb2Exts))
        ext = "Fb2";
    else if (checkInArray(fileExtensionLibrary.FlvExts))
        ext = "Flv";
    else if (checkInArray(fileExtensionLibrary.FodpExts))
        ext = "Fodp";
    else if (checkInArray(fileExtensionLibrary.FodsExts))
        ext = "Fods";
    else if (checkInArray(fileExtensionLibrary.FodtExts))
        ext = "Fodt";
    else if (checkInArray(fileExtensionLibrary.GdocExts))
        ext = "Gdoc";
    else if (checkInArray(fileExtensionLibrary.GsheetExts))
        ext = "Gsheet";
    else if (checkInArray(fileExtensionLibrary.GslidesExts))
        ext = "Gslides";
    else if (checkInArray(fileExtensionLibrary.HtmExts))
        ext = "Htm";
    else if (checkInArray(fileExtensionLibrary.HtmlExts))
        ext = "Html";
    else if (checkInArray(fileExtensionLibrary.IafExts))
        ext = "Iaf";
    else if (checkInArray(fileExtensionLibrary.IcsExts))
        ext = "Ics";
    else if (checkInArray(fileExtensionLibrary.ImgExts))
        ext = "Image";
    else if (checkInArray(fileExtensionLibrary.M2tsExts))
        ext = "M2ts";
    else if (checkInArray(fileExtensionLibrary.MhtExts))
        ext = "Mht";
    else if (checkInArray(fileExtensionLibrary.MhtmlExts))
        ext = "Mhtml";
    else if (checkInArray(fileExtensionLibrary.MkvExts))
        ext = "Mkv";
    else if (checkInArray(fileExtensionLibrary.MovExts))
        ext = "Mov";
    else if (checkInArray(fileExtensionLibrary.Mp4Exts))
        ext = "Mp4";
    else if (checkInArray(fileExtensionLibrary.MpgExts))
        ext = "Mpg";
    else if (checkInArray(fileExtensionLibrary.OdpExts))
        ext = "Odp";
    else if (checkInArray(fileExtensionLibrary.OdsExts))
        ext = "Ods";
    else if (checkInArray(fileExtensionLibrary.OdtExts))
        ext = "Odt";
    else if (checkInArray(fileExtensionLibrary.OformExts))
        ext = "Oform";
    else if (checkInArray(fileExtensionLibrary.OtpExts))
        ext = "Otp";
    else if (checkInArray(fileExtensionLibrary.OtsExts))
        ext = "Ots";
    else if (checkInArray(fileExtensionLibrary.OttExts))
        ext = "Ott";
    else if (checkInArray(fileExtensionLibrary.PdfExts))
        ext = "Pdf";
    else if (checkInArray(fileExtensionLibrary.PotExts))
        ext = "Pot";
    else if (checkInArray(fileExtensionLibrary.PotmExts))
        ext = "Potm";
    else if (checkInArray(fileExtensionLibrary.PotxExts))
        ext = "Potx";
    else if (checkInArray(fileExtensionLibrary.PpsExts))
        ext = "Pps";
    else if (checkInArray(fileExtensionLibrary.PpsmExts))
        ext = "Ppsm";
    else if (checkInArray(fileExtensionLibrary.PpsxExts))
        ext = "Ppsx";
    else if (checkInArray(fileExtensionLibrary.PptExts))
        ext = "Ppt";
    else if (checkInArray(fileExtensionLibrary.PptmExts))
        ext = "Pptm";
    else if (checkInArray(fileExtensionLibrary.PpttExts))
        ext = "Pptt";
    else if (checkInArray(fileExtensionLibrary.PptxExts))
        ext = "Pptx";
    else if (checkInArray(fileExtensionLibrary.RtfExts))
        ext = "Rtf";
    else if (checkInArray(fileExtensionLibrary.SoundExts))
        ext = "Sound";
    else if (checkInArray(fileExtensionLibrary.SoundUnkExts))
        ext = "SoundUnk";
    else if (checkInArray(fileExtensionLibrary.StwExts))
        ext = "Stw";
    else if (checkInArray(fileExtensionLibrary.SvgExts))
        ext = "Svg";
    else if (checkInArray(fileExtensionLibrary.SxcExts))
        ext = "Sxc";
    else if (checkInArray(fileExtensionLibrary.SxiExts))
        ext = "Sxi";
    else if (checkInArray(fileExtensionLibrary.SxwExts))
        ext = "Sxw";
    else if (checkInArray(fileExtensionLibrary.TxtExts))
        ext = "Txt";
    else if (checkInArray(fileExtensionLibrary.VideoExts))
        ext = "Video";
    else if (checkInArray(fileExtensionLibrary.VideoUnkExts))
        ext = "VideoUnk";
    else if (checkInArray(fileExtensionLibrary.WpsExts))
        ext = "Wps";
    else if (checkInArray(fileExtensionLibrary.WptExts))
        ext = "Wpt";
    else if (checkInArray(fileExtensionLibrary.XlsExts))
        ext = "Xls";
    else if (checkInArray(fileExtensionLibrary.XlsmExts))
        ext = "Xlsm";
    else if (checkInArray(fileExtensionLibrary.XlstExts))
        ext = "Xlst";
    else if (checkInArray(fileExtensionLibrary.XlsxExts))
        ext = "Xlsx";
    else if (checkInArray(fileExtensionLibrary.XltExts))
        ext = "Xlt";
    else if (checkInArray(fileExtensionLibrary.XltmExts))
        ext = "Xltm";
    else if (checkInArray(fileExtensionLibrary.XltxExts))
        ext = "Xltx";
    else if (checkInArray(fileExtensionLibrary.XmlExts))
        ext = "Xml";
    else if (checkInArray(fileExtensionLibrary.XpsExts))
        ext = "Xps";

    return "ftFile_" + (compact === true ? 21 : 32) + " ft_" + ext;
};

ASC.Files.Utility.getFolderCssClass = function (compact) {
    return "ftFolder_" + (compact === true ? 21 : 32);
};

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
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebPreviewed) != -1;
};

ASC.Files.Utility.CanWebEdit = function (fileTitle) {
    return (
        Teamlab.profile.isVisitor !== true
            ? jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebEdited) != -1
            : false
    );
};

ASC.Files.Utility.CanWebEncrypt = function (fileTitle) {
    return (
        Teamlab.profile.isVisitor !== true
            ? jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebEncrypt) != -1
            : false
    );
};

ASC.Files.Utility.CanWebReview = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebReviewed) != -1;
};

ASC.Files.Utility.CanWebCustomFilterEditing = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebCustomFilterEditing) != -1;
};

ASC.Files.Utility.CanWebRestrictedEditing = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebRestrictedEditing) != -1;
};

ASC.Files.Utility.CanWebComment = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebCommented) != -1;
};

ASC.Files.Utility.CanBeTemplate = function (fileTitle) {
    return jq.inArray(ASC.Files.Utility.GetFileExtension(fileTitle), ASC.Files.Utility.Resource.ExtsWebTemplate) != -1;
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

ASC.Files.Utility.FileIsMasterForm = function (fileTitle) {
    return ASC.Files.Utility.GetFileExtension(fileTitle) === ASC.Files.Utility.Resource.MasterFormExtension;
};

ASC.Files.Utility.GetFileDownloadUrl = function (fileId, fileVersion, convertToExtension) {
    var url = ASC.Files.Utility.Resource.FileDownloadUrlString.format(encodeURIComponent(fileId));

    url = ASC.Files.Utility.AddExternalShareKey(url);
    
    if (fileVersion) {
        return url + "&" + ASC.Files.Utility.Resource.ParamVersion + "=" + fileVersion;
    }
    if (convertToExtension) {
        return url + "&" + ASC.Files.Utility.Resource.ParamOutType + "=" + convertToExtension;
    }
    
    return url;
};

ASC.Files.Utility.GetFileViewUrl = function (fileId, fileVersion, convertToExtension) {
    var url = ASC.Files.Utility.Resource.FileViewUrlString.format(encodeURIComponent(fileId));
    if (fileVersion) {
        return url + "&" + ASC.Files.Utility.Resource.ParamVersion + "=" + fileVersion;
    }
    if (convertToExtension) {
        return url + "&" + ASC.Files.Utility.Resource.ParamOutType + "=" + convertToExtension;
    }

    url = ASC.Files.Utility.AddExternalShareKey(url);
    
    return url;
};

ASC.Files.Utility.GetFileRedirectPreviewUrl = function (fileId, orFolderId) {
    return ASC.Files.Utility.Resource.FileRedirectPreviewUrlString + (!!orFolderId ? ("&folderid=" + encodeURIComponent(orFolderId)) : ("&fileid=" + encodeURIComponent(fileId)));
};

ASC.Files.Utility.GetFileWebViewerUrl = function (fileId, fileVersion) {
    var url = ASC.Files.Utility.Resource.FileWebViewerUrlString.format(encodeURIComponent(fileId));

    url = ASC.Files.Utility.AddExternalShareKey(url);
    
    if (fileVersion) {
        return url + "&" + ASC.Files.Utility.Resource.ParamVersion + "=" + fileVersion;
    }
    
    return url;
};

ASC.Files.Utility.GetFileWebViewerExternalUrl = function (fileUri, fileTitle, refererUrl) {
    return ASC.Files.Utility.Resource.FileWebViewerExternalUrlString.format(encodeURIComponent(fileUri), encodeURIComponent(fileTitle || ""), encodeURIComponent(refererUrl || ""));
};

ASC.Files.Utility.GetFileWebEditorUrl = function (fileId) {
    var url = ASC.Files.Utility.Resource.FileWebEditorUrlString.format(encodeURIComponent(fileId));

    url = ASC.Files.Utility.AddExternalShareKey(url);
    
    return url;
};

ASC.Files.Utility.GetFileCustomProtocolEditorUrl = function (fileId) {
    return ASC.Files.Utility.Resource.FileCustomProtocolEditorUrlString.format(encodeURIComponent(fileId));
};

ASC.Files.Utility.GetOpenPrivate = function (fileId) {
    return ASC.Files.Utility.Resource.OpenPrivateString.format(encodeURIComponent(fileId));
};

ASC.Files.Utility.GetFileWebEditorExternalUrl = function (fileUri, fileTitle, folderId) {
    return ASC.Files.Utility.Resource.FileWebEditorExternalUrlString.format(encodeURIComponent(fileUri), encodeURIComponent(fileTitle || "")) +
        (folderId ? ("&folderid=" + folderId) : "");
};

ASC.Files.Utility.GetFileThumbnailUrl = function (fileId, fileVersion) {
    var url = ASC.Files.Utility.Resource.FileThumbnailUrlString.format(encodeURIComponent(fileId));

    url = ASC.Files.Utility.AddExternalShareKey(url);
    
    if (fileVersion) {
        return url + "&" + ASC.Files.Utility.Resource.ParamVersion + "=" + fileVersion;
    }
    return url;
};

ASC.Files.Utility.AddExternalShareKey = function (url) {
    if (new URLSearchParams(url).get(ASC.Files.Utility.ExternalFolderShareKey)) {
        return url;
    }
    
    var key = new URLSearchParams(window.location.search).get(ASC.Files.Utility.ExternalFolderShareKey);

    if (key !== null) {
        url += (url.indexOf('?') === -1 ? '?' : '&') + ASC.Files.Utility.ExternalFolderShareKey + '=' + key;
    }
    
    return url;
}