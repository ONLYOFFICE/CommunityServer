/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


window.ASC.Files.Converter = (function () {
    var isInit = false;

    var init = function () {
        if (isInit === false) {
            isInit = true;
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.ConvertCurrentFile, ASC.Files.Converter.onConvertCurrentFile);
            ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.StoreOriginalFiles, ASC.Files.Converter.onStoreOriginalFiles);
        }
    };

    /* Methods*/

    var checkCanOpenEditor = function (fileId, fileTitle, version, forEdit) {
        if (!ASC.Files.Utility.MustConvert(fileTitle) || forEdit == false) {
            if (forEdit == false) {
                var url = ASC.Files.Utility.GetFileWebViewerUrl(fileId, version);
                window.open(url, "_blank");
                return ASC.Files.Marker.removeNewIcon("file", fileId);
            }

            return ASC.Files.Actions.checkEditFile(fileId);
        }

        jq("#progressCopyConvertId").val(fileId);
        jq("#progressCopyConvertVersion").val(version);
        jq("#progressCopyConvertView").val(forEdit == false);

        if (!ASC.Files.Utility.FileIsDocument(fileTitle)
            && !ASC.Files.Utility.FileIsSpreadsheet(fileTitle)
            && !ASC.Files.Utility.FileIsPresentation(fileTitle)) {
            return true;
        }

        ASC.Files.UI.blockUI("#confirmCopyConvert", 500, 0, -120);

        PopupKeyUpActionProvider.EnterAction = "jq(\"#confirmCopyConvert .blue:visible:not(.disable):first\").click()";

        jq("#progressCopyConvert, #copyAndConvertOpen").hide();
        jq("#copyConvertDescript, #confirmCopyAndConvert").show();

        jq("#goToCopySplitter, #goToCopyFolder, #confirmCopyConvertToMyText, #confirmCopyConvertLabelText").hide();

        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        if (fileObj.is(":visible")) {
            if (!ASC.Files.UI.accessEdit()) {
                if (Teamlab.profile.isVisitor) {
                    PopupKeyUpActionProvider.CloseDialog();
                    url = ASC.Files.Utility.GetFileDownloadUrl(fileId, version);
                    window.open(url, "_blank");
                    return ASC.Files.Marker.removeNewIcon("file", fileId);
                } else {
                    jq("#confirmCopyConvertToMyText").show();
                }
            } else if (ASC.Files.UI.accessDelete(fileObj) && !ASC.Files.UI.lockedForMe(fileObj)) {
                jq("#confirmCopyConvertLabelText").show();
                jq("#confirmCopyConvertLabelText input").attr("disabled", false);
            }
        } else if (Teamlab.profile.isVisitor) {
            PopupKeyUpActionProvider.CloseDialog();
            url = ASC.Files.Utility.GetFileDownloadUrl(fileId, version);
            window.open(url, "_blank");
            return ASC.Files.Marker.removeNewIcon("file", fileId);
        }
        return true;
    };

    var convertCurrentFile = function () {
        PopupKeyUpActionProvider.CloseDialogAction = "ASC.Files.Converter.convertFileEnd(null, true);";

        jq("#copyConvertDescript, #confirmCopyAndConvert, #progressCopyConvert .convert-status").hide();
        jq("#progressCopyConvert, #copyAndConvertOpen, #progressCopyConvertRun").show();

        jq("#copyAndConvertOpen").addClass("disable");

        var fileId = jq("#progressCopyConvertId").val();
        var version = jq("#progressCopyConvertVersion").val();

        ASC.Files.UI.setProgressValue("#progressCopyConvert", 0);
        jq("#progressCopyConvert .asc-progress-percent").text("0%");

        ASC.Files.Marker.removeNewIcon("file", fileId);

        ASC.Files.Converter.convertFileStep(fileId, version, true);
    };

    var convertFileEnd = function (jsonStringData, cancel) {
        PopupKeyUpActionProvider.CloseDialogAction = "";
        jq("#progressCopyConvert .convert-status").hide();
        if (!jsonStringData) {
            if (cancel) {
                PopupKeyUpActionProvider.CloseDialog();
            } else {
                jq("#progressCopyConvertError").show();
            }
            return;
        }

        jq("#copyAndConvertOpen").removeClass("disable");

        var file = jq.parseJSON(jsonStringData);

        var fileId = file.id;
        var fileTitle = file.title;
        jq("#progressCopyConvertId").val(fileId);

        jq("#progressCopyConvertEnd").show();
        if (file.folderId != ASC.Files.Folders.currentFolder.id) {
            jq("#progressCopyConvertEndTo, #goToCopySplitter, #goToCopyFolder").show();
            ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoCrateFileIn.format(fileTitle, file.folderTitle));
            jq(".convert-end-to").html(file.folderTitle);
            jq("#goToCopyFolder").attr("data-id", file.folderId);
            return;
        }

        var stringXmlFile = file.fileXml;
        var htmlXML = ASC.Files.TemplateManager.translateFromString(stringXmlFile);

        ASC.Files.EmptyScreen.hideEmptyScreen();
        var fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        if (fileObj.length == 0) {
            jq("#filesMainContent").prepend(htmlXML);
        } else {
            fileObj.replaceWith(htmlXML);
        }

        fileObj = ASC.Files.UI.getEntryObject("file", fileId);
        var fileData = ASC.Files.UI.getObjectData(fileObj);
        fileObj = fileData.entryObject;

        fileObj.removeClass("new-file").yellowFade();

        ASC.Files.UI.addRowHandlers(fileObj);

        ASC.Files.UI.resetSelectAll();

        ASC.Files.Actions.showActionsViewPanel();

        ASC.Files.UI.displayInfoPanel(ASC.Files.FilesJSResources.InfoCrateFile.format(fileTitle));
    };

    var convertFileOpen = function () {
        PopupKeyUpActionProvider.CloseDialog();

        var fileId = jq("#progressCopyConvertId").val();

        if (jq("#progressCopyConvertView").val() == "true") {
            var url = ASC.Files.Utility.GetFileWebViewerUrl(fileId);
            window.open(url, "_blank");
        } else {
            ASC.Files.Actions.checkEditFile(fileId);
        }
    };

    var showToConvert = function (selectedElements) {
        selectedElements = selectedElements || jq("#filesMainContent .file-row:has(.checkbox input:checked)");

        var selectedFiles = {
            documents: [],
            spreadsheets: [],
            presentations: [],
            other: []
        };

        jq("#convertFileZip").toggle(jq(selectedElements).length > 1);

        jq(selectedElements).each(function () {
            var entryObj = ASC.Files.UI.getObjectData(this);
            var entryTitle = entryObj.title;
            var entryId = entryObj.id;
            var formats;
            var ftClass;
            if (entryObj.entryType == "file") {
                formats =
                    [{
                        name: ASC.Files.FilesJSResources.OriginalFormat,
                        value: ASC.Files.Utility.GetFileExtension(entryTitle)
                    }];
                var convertFormats = ASC.Files.Utility.GetConvertFormats(entryTitle);
                if (convertFormats) {
                    for (var i = 0; i < convertFormats.length; i++) {
                        formats.push({ name: convertFormats[i], value: convertFormats[i] });
                    }
                }
                ftClass = ASC.Files.Utility.getCssClassByFileTitle(entryTitle);
                entryId = "file_" + entryId;
            } else {
                formats = [{ name: "", value: "" }];
                ftClass = ASC.Files.Utility.getFolderCssClass();
                entryId = "folder_" + entryId;
            }
            var curData = {
                fileConvertFormats: { format: formats },
                fileCssClass: ftClass,
                fileTitle: entryTitle,
                fileId: entryId,
            };
            if (formats.length == 1) {
                selectedFiles.other.push(curData);
            } else if (ASC.Files.Utility.FileIsDocument(entryTitle)) {
                selectedFiles.documents.push(curData);
            } else if (ASC.Files.Utility.FileIsSpreadsheet(entryTitle)) {
                selectedFiles.spreadsheets.push(curData);
            } else if (ASC.Files.Utility.FileIsPresentation(entryTitle)) {
                selectedFiles.presentations.push(curData);
            } else {
                selectedFiles.other.push(curData);
            }
        });

        var blockList =
            [
                {
                    blockCssClass: ASC.Files.Utility.getCssClassByFileTitle(ASC.Files.Utility.Resource.InternalFormats.Document),
                    blockTitle: ASC.Files.FilesJSResources.Documents,
                    blockFormats: { format: getFileBlockConvertFormats(selectedFiles.documents) },
                    files: { file: selectedFiles.documents }
                },
                {
                    blockCssClass: ASC.Files.Utility.getCssClassByFileTitle(ASC.Files.Utility.Resource.InternalFormats.Spreadsheet),
                    blockTitle: ASC.Files.FilesJSResources.Spreedsheets,
                    blockFormats: { format: getFileBlockConvertFormats(selectedFiles.spreadsheets) },
                    files: { file: selectedFiles.spreadsheets }
                },
                {
                    blockCssClass: ASC.Files.Utility.getCssClassByFileTitle(ASC.Files.Utility.Resource.InternalFormats.Presentation),
                    blockTitle: ASC.Files.FilesJSResources.Presentations,
                    blockFormats: { format: getFileBlockConvertFormats(selectedFiles.presentations) },
                    files: { file: selectedFiles.presentations }
                },
                {
                    blockCssClass: ASC.Files.Utility.getCssClassByFileTitle(),
                    blockTitle: ASC.Files.FilesJSResources.Other,
                    blockFormats: { format: [] },
                    files: { file: selectedFiles.other }
                }
            ];

        var stringData = ASC.Files.Common.jsonToXml({ entryData: { fileTypeBlock: blockList } });
        var htmlXml = ASC.Files.TemplateManager.translateFromString(stringData);

        jq("#convertFileList").html(htmlXml);

        jq("#convertFileList select").each(function () {
            jq(this).tlcombobox();
        });

        jq(".cnvrt-file-block-body .cnvrt-title-content").each(function () {
            var rowLink = jq(this);
            var fileTitle = rowLink.text();
            ASC.Files.UI.highlightExtension(rowLink, fileTitle);
        });

        ASC.Files.UI.blockUI("#convertAndDownload", 600, 0, -150);
    };

    var getFileBlockConvertFormats = function (selectedFiles) {
        var result = [];
        if (selectedFiles.length == 0) {
            return result;
        }
        result.push({ name: ASC.Files.FilesJSResources.OriginalFormat, value: "original" });
        var minCount = selectedFiles[0].fileConvertFormats.format.length;
        var minIndex = 0;
        for (var i = 1; i < selectedFiles.length; i++) {
            if (selectedFiles[i].fileConvertFormats.format.length < minCount) {
                minIndex = i;
            }
        }
        for (i = 0; i < selectedFiles[minIndex].fileConvertFormats.format.length; i++) {
            var ext = selectedFiles[minIndex].fileConvertFormats.format[i].value;
            var isTotalFormat = true;
            for (var j = 0; j < selectedFiles.length; j++) {
                var hasFormat = false;
                for (var k = 0; k < selectedFiles[j].fileConvertFormats.format.length; k++) {
                    if (selectedFiles[j].fileConvertFormats.format[k].value == ext) {
                        hasFormat = true;
                    }
                }
                if (!hasFormat) {
                    isTotalFormat = false;
                }
            }
            if (isTotalFormat) {
                result.push({ name: ext, value: ext });
            }
        }
        result.push({ name: ASC.Files.FilesJSResources.CustomFormat, value: "custom" });
        return result;
    };

    var changeFormat = function (select) {

        var name = jq(select).find("option:selected").text();
        jq(select).parents(".cnvrt-format-title-content").find(".cnvrt-format-title").text(name);

        if (jq(select).attr("file-id")) {
            var header = jq(select).parents(".cnvrt-file-block").find(".cnvrt-file-block-head");
            var headerSelect = jq(header).find("select.select-format");
            jq(headerSelect).val("custom");
            jq(headerSelect).tlcombobox();
            var selectedItem = jq(headerSelect).find("option:selected").text();
            jq(header).find(".cnvrt-format-title").text(selectedItem);
        } else {
            var block = jq(select).parents(".cnvrt-file-block");
            var body = jq(block).find(".cnvrt-file-block-body");
            var val = jq(select).val();
            if (val == "original") {
                jq(block).removeClass("cnvrt-file-block-open").addClass("cnvrt-file-block-closed");
                jq(body).find("select.select-format").each(function () {
                    var defaultFormatValue = jq(this).find("option:first").val();
                    var defaultFormatName = jq(this).find("option:first").text();
                    jq(this).val(defaultFormatValue);
                    jq(this).tlcombobox();
                    jq(this).parents(".cnvrt-format-title-content").find(".cnvrt-format-title").text(defaultFormatName);
                });
            } else if (val == "custom") {
                jq(block).removeClass("cnvrt-file-block-closed").addClass("cnvrt-file-block-open");
            } else {
                jq(block).removeClass("cnvrt-file-block-open").addClass("cnvrt-file-block-closed");
                jq(body).find("select.select-format").each(function () {
                    jq(this).val(val);
                    jq(this).tlcombobox();
                    var formatName = jq(this).find("option:selected").text();
                    jq(this).parents(".cnvrt-format-title-content").find(".cnvrt-format-title").text(formatName);
                });
            }
        }
    };

    //request

    var storeOriginalFiles = function (target) {
        var value = jq(target).prop("checked");

        ASC.Files.ServiceManager.storeOriginalFiles(ASC.Files.ServiceManager.events.StoreOriginalFiles, { value: value });
    };

    var convertFileStep = function (fileId, version, isStart) {
        var data = {};
        data.entry = new Array();

        var entry = { entry: [fileId, version, isStart === true] };
        data.entry.push([entry]);

        ASC.Files.ServiceManager.checkConversion(
            ASC.Files.ServiceManager.events.ConvertCurrentFile,
            { fileId: fileId, version: version },
            { stringListList: data });
    };

    //event handler

    var onConvertCurrentFile = function (jsonData, params, errorMessage) {
        if (typeof jsonData !== "object" && typeof errorMessage != "undefined" || jsonData == null) {
            errorMessage = errorMessage || ASC.Files.FilesJSResources.ErrorMassage_ErrorConvert;
        } else if (!jsonData.length || !jsonData[0]) {
            errorMessage = ASC.Files.FilesJSResources.ErrorMassage_ErrorConvert;
        } else if (jsonData[0].error) {
            errorMessage = jsonData[0].error;
        }

        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            ASC.Files.Converter.convertFileEnd();
            return;
        }

        var data = jsonData[0];

        var progress = data.progress || 0;
        ASC.Files.UI.setProgressValue("#progressCopyConvert", progress);
        jq("#progressCopyConvert .asc-progress-percent").text(progress + "%");

        if (progress >= 100) {
            ASC.Files.Converter.convertFileEnd(data.result);
            return;
        }

        setTimeout(function () {
            ASC.Files.Converter.convertFileStep(params.fileId, params.version);
        }, ASC.Files.Constants.REQUEST_CONVERT_DELAY);
    };

    var onStoreOriginalFiles = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.UI.displayInfoPanel(errorMessage, true);
            return;
        }
        ASC.Files.Common.storeOriginal = (jsonData === true);

        jq(".store-original").prop("checked", ASC.Files.Common.storeOriginal);
    };

    return {
        init: init,

        checkCanOpenEditor: checkCanOpenEditor,
        convertCurrentFile: convertCurrentFile,
        convertFileStep: convertFileStep,
        convertFileEnd: convertFileEnd,
        convertFileOpen: convertFileOpen,
        storeOriginalFiles: storeOriginalFiles,

        showToConvert: showToConvert,
        changeFormat: changeFormat,

        onConvertCurrentFile: onConvertCurrentFile,
        onStoreOriginalFiles: onStoreOriginalFiles
    };
})();

(function ($) {
    $(function () {
        ASC.Files.Converter.init();

        jq("#studioPageContent").on("click", "#buttonConvert, #mainConvert.unlockAction", function () {
            ASC.Files.Actions.hideAllActionPanels();
            ASC.Files.Converter.showToConvert();
        });

        jq("#filesMainContent").on("click", ".pencil.file-edit, .can-coauthoring .pencil.file-editing, .pencil.convert-action", function () {
            ASC.Files.Actions.hideAllActionPanels();
            var fileData = ASC.Files.UI.getObjectData(this);
            var fileId = fileData.entryId;
            var fileTitle = fileData.title;
            var fileVersion = fileData.version;

            ASC.Files.Converter.checkCanOpenEditor(fileId, fileTitle, fileVersion);
            return false;
        });

        jq("#convertFileList").on("click", ".cnvrt-status-content, .cnvrt-file-block-head .cnvrt-title-content, .cnvrt-file-block-head .cnvrt-format-icon-content", function () {
            var parent = jq(this).parents(".cnvrt-file-block");

            var isOpen = jq(parent).hasClass("cnvrt-file-block-open");
            jq(parent)
                .toggleClass("cnvrt-file-block-open", !isOpen)
                .toggleClass("cnvrt-file-block-closed", isOpen);
        });

        jq("#convertFileList").on("change", "select.select-format", function () {
            ASC.Files.Converter.changeFormat(this);
        });

        jq("#convertFileList").on("click", ".cnvrt-file-block-head input[type=checkbox]", function () {
            var parent = jq(this).parents(".cnvrt-file-block");

            var isChecked = jq(this).prop("checked");
            jq(parent).toggleClass("cnvrt-file-block-active", isChecked);
            jq(parent).find("input[type=checkbox]").prop("checked", isChecked);
            jq(parent).find(".cnvrt-file-row").toggleClass("cnvrt-file-row-active", isChecked);
        });

        jq("#convertFileList").on("click", ".cnvrt-file-block-body input[type=checkbox]", function () {
            var parentBlock = jq(this).parents(".cnvrt-file-block");
            var parentRow = jq(this).parents(".cnvrt-file-row");
            var count = jq(this).parents(".cnvrt-file-block-body").find("input[type=checkbox]").length;
            var checkedCount = jq(this).parents(".cnvrt-file-block-body").find("input[type=checkbox]:checked").length;

            var isChecked = jq(this).prop("checked");
            jq(parentRow).toggleClass("cnvrt-file-row-active", isChecked);
            if (isChecked) {
                jq(parentBlock).addClass("cnvrt-file-block-active");
                if (checkedCount == count) {
                    jq(parentBlock).find(".cnvrt-file-block-head input[type=checkbox]").prop("checked", true).prop("indeterminate", false);
                } else {
                    jq(parentBlock).find(".cnvrt-file-block-head input[type=checkbox]").prop("indeterminate", true);
                }
            } else {
                jq(parentBlock).find(".cnvrt-file-block-head input[type=checkbox]").prop("checked", false).prop("indeterminate", false);
                if (checkedCount == 0) {
                    jq(parentBlock).removeClass("cnvrt-file-block-active");
                } else {
                    jq(parentBlock).find(".cnvrt-file-block-head input[type=checkbox]").prop("indeterminate", true);
                }
            }
        });

        jq("#buttonStartConvert").click(function () {
            var data = new Array();

            jq("#convertFileList .cnvrt-file-block-body select.select-format").each(function () {
                var parentRow = jq(this).parents(".cnvrt-file-row");
                if (jq(parentRow).hasClass("cnvrt-file-row-active")) {
                    var fileFormat = jq(this).val();
                    var fileId = jq(this).attr("file-id");
                    data.push({ Key: fileId, Value: fileFormat });
                }
            });

            jq("#convertFileList .cnvrt-file-block-body input[type=hidden]").each(function () {
                var parentRow = jq(this).parents(".cnvrt-file-row");
                if (jq(parentRow).hasClass("cnvrt-file-row-active")) {
                    var fileFormat = jq(this).val();
                    fileId = jq(this).attr("file-id");
                    data.push({ Key: fileId, Value: fileFormat });
                }
            });

            if (data.length == 0) {
                return;
            }

            PopupKeyUpActionProvider.CloseDialog();

            if (data.length == 1) {
                var itemId = ASC.Files.UI.parseItemId(data[0].Key);
                if (itemId.entryType == "file") {
                    fileId = itemId.entryId;
                    var url = ASC.Files.Utility.GetFileDownloadUrl(fileId, 0, data[0].Value);
                    window.open(url, "_blank");
                    return;
                }
            }

            ASC.Files.Folders.bulkDownload(data);
        });

        jq("#confirmCopyAndConvert").click(ASC.Files.Converter.convertCurrentFile);

        jq("#confirmCopyConvert").on("click", "#copyAndConvertOpen:not(.disable)", function () {
            ASC.Files.Converter.convertFileOpen();
        });

        jq(".store-original").change(function () {
            ASC.Files.Converter.storeOriginalFiles(this);
        });

        jq("#confirmCopyConvert").on("click", "#goToCopyFolder", function () {
            var folderId = jq(this).attr("data-id");
            ASC.Files.Anchor.navigationSet(folderId);
            PopupKeyUpActionProvider.CloseDialog();
        });

    });
})(jQuery);