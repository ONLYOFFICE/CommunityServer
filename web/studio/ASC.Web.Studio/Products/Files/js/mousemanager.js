/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/
window.ASC.Files.Mouse = (function () {
    var isInit = false;

    var moveToX = 0;
    var moveToY = 0;
    var moveToFolder = "";

    var mainContentArea = null;

    var mouseBtn = false;

    var timeoutUpdate = null;

    var mouseSelector =
        {
            startX: 0,
            startY: 0,
            entryItems: new Array()
        };

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        ASC.Files.Mouse.mouseBtn = false;
    };

    var handleMove = function (e) {
        if (ASC.Files.Mouse.mouseBtn
            || jq("#filesSelector, #filesActionsPanel:visible, #filesActionPanel:visible").length != 0) {
            return true;
        }

        e = ASC.Files.Common.fixEvent(e);
        jq(this).toggleClass("row-hover", e.type == "mouseenter");

        return true;
    };

    var forbiddenTarget = function (target) {
        if (target.nodeName == "HTML") {
            return true;
        }

        if (jq(".popup-modal:visible").length != 0) {
            return true;
        }

        if (ASC.Files.ImageViewer && ASC.Files.ImageViewer.isView()) {
            return true;
        }

        if (!jq(target).is(
            ".mainPageLayout:not(.studio-top-panel),\
             .mainPageTableSidePanel,\
             .mainPageContent,\
             #contentPanel,\
             #mainContent,\
             .file-row:not(.row-rename):not(.row-selected),\
             .file-row:not(.row-rename):not(.row-selected) *")) {
            return true;
        }

        return jq(target).is(".textEdit");
    };

    var collectEntryItems = function () {
        if (jq("#filesSelector").length == 0) {
            return;
        }

        ASC.Files.Mouse.mouseSelector.entryItems = new Array();
        jq("#filesMainContent .file-row:not(.checkloading):not(.new-folder):not(.new-file)").each(function () {
            var entryObj = jq(this).offset();
            entryObj.right = entryObj.left + this.offsetWidth;
            entryObj.bottom = entryObj.top + this.offsetHeight;
            entryObj.entryObj = jq(this);

            ASC.Files.Mouse.mouseSelector.entryItems.push(entryObj);
        });
    };

    var updateMainContentArea = function () {
        mainContentArea = jq("#filesMainContent").offset();
        mainContentArea.right = mainContentArea.left + jq("#filesMainContent")[0].offsetWidth;
        mainContentArea.bottom = mainContentArea.top + jq("#filesMainContent")[0].offsetHeight;
        mainContentArea.documentWidth = jq(document).width();
        mainContentArea.documentHeight = jq(document).height();
    };

    var intersectionLines = function (line1, line2) {
        var left = line1.x < line2.x;
        return (left ? line1 : line2).y > (!left ? line1 : line2).x;
    };

    var intersectionRectangles = function (rect1, rect2) {
        return intersectionLines({ x: rect1.top, y: rect1.bottom },
            { x: rect2.top, y: rect2.bottom }) &&
            intersectionLines({ x: rect1.left, y: rect1.right },
                { x: rect2.left, y: rect2.right });
    };

    var beginSelecting = function (e) {
        e = ASC.Files.Common.fixEvent(e);
        ASC.Files.Mouse.mouseBtn = e.target.nodeName != "HTML";

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1))) {
            return true;
        }

        var target = e.target || e.srcElement;

        try {
            if (forbiddenTarget(target)) {
                return true;
            }
        } catch (e) {
            return true;
        }

        ASC.Files.Mouse.mouseSelector.startX = e.pageX;
        ASC.Files.Mouse.mouseSelector.startY = e.pageY;
        ASC.Files.Mouse.updateMainContentArea();

        var windowFix = (jq.browser.msie && jq.browser.version < 9 ? jq("body") : jq(window));
        windowFix
            .unbind("mousemove.MouseSelect mouseup.MouseSelect")
            .bind("mousemove.MouseSelect", ASC.Files.Mouse.continueSelecting)
            .bind("mouseup.MouseSelect", ASC.Files.Mouse.finishSelecting);

        if (typeof SmallChat != "undefined" && SmallChat.minimizeAllWindowsIfLoseFocus) {
            SmallChat.minimizeAllWindowsIfLoseFocus(e, target);
        }

        return false;
    };

    var continueSelecting = function (event) {
        event = ASC.Files.Common.fixEvent(event);

        var targetMove = event.target || event.srcElement;
        if (typeof targetMove == "undefined") {
            return true;
        }

        var selectDelta = 2;
        var posXnew = Math.min(event.pageX, mainContentArea.documentWidth - selectDelta);
        var posYnew = Math.min(event.pageY, mainContentArea.documentHeight - selectDelta);

        var width = Math.abs(posXnew - ASC.Files.Mouse.mouseSelector.startX);
        var height = Math.abs(posYnew - ASC.Files.Mouse.mouseSelector.startY);

        if (width < 5 && height < 5) {
            return true;
        }

        if (jq("#filesSelector").length == 0) {
            ASC.Files.Actions.hideAllActionPanels();

            jq("#studioPageContent").append("<div id=\"filesSelector\"></div>");
            ASC.Files.Mouse.collectEntryItems();
            jq("body").addClass("select-action");
        }

        var selectObj = {
            left: Math.min(ASC.Files.Mouse.mouseSelector.startX, posXnew),
            top: Math.min(ASC.Files.Mouse.mouseSelector.startY, posYnew),
            right: Math.max(ASC.Files.Mouse.mouseSelector.startX, posXnew),
            bottom: Math.max(ASC.Files.Mouse.mouseSelector.startY, posYnew)
        };

        jq("#filesSelector").css({
            "width": width + "px",
            "height": height + "px",
            "left": selectObj.left + "px",
            "top": selectObj.top + "px"
        });

        var itemObj = mainContentArea;

        if (!event.ctrlKey && !intersectionRectangles(itemObj, selectObj)) {
            ASC.Files.UI.checkSelectAll(false);
        } else {
            var selectionChanged = false;

            var itemsCount = ASC.Files.Mouse.mouseSelector.entryItems.length;
            for (var i = itemsCount; i; i--) {
                itemObj = ASC.Files.Mouse.mouseSelector.entryItems[itemsCount - i];
                if (intersectionRectangles(itemObj, selectObj)) {
                    selectionChanged = ASC.Files.UI.selectRow(itemObj.entryObj, true) || selectionChanged;
                } else {
                    if (!event.ctrlKey) {
                        selectionChanged = ASC.Files.UI.selectRow(itemObj.entryObj, false) || selectionChanged;
                    }
                }
            }

            if (selectionChanged) {
                clearTimeout(timeoutUpdate);
                timeoutUpdate = setTimeout(ASC.Files.UI.updateMainContentHeader, 10);
            }
        }

        return false;
    };

    var finishSelecting = function () {
        jq("#filesSelector").remove();
        ASC.Files.Mouse.mouseSelector.startX = 0;
        ASC.Files.Mouse.mouseSelector.startY = 0;
        ASC.Files.Mouse.mouseSelector.entryItems = new Array();
        ASC.Files.Mouse.mouseBtn = false;

        var windowFix = (jq.browser.msie && jq.browser.version < 9 ? jq("body") : jq(window));
        windowFix.unbind("mousemove.MouseSelect mouseup.MouseSelect");

        jq("body").removeClass("select-action");
    };

    var highlightFolderTo = function (cssClass, withoutTrash) {
        var listArea =
            [
                "#treeViewContainer li:not(.access-read) > a",
                ".to-parent-folder"
            ];
        if (ASC.Files.Folders.currentFolder.id != ASC.Files.Constants.FOLDER_ID_TRASH) {
            listArea.push("#filesMainContent .folder-row:not(.checkloading):not(.new-folder):not(.error-entry):not(.row-selected)");
        }

        var searchArea = listArea.join(",");
        jq(searchArea).each(function () {
            var folderToId;
            var entryData = ASC.Files.UI.getObjectData(this);

            if (entryData) {
                folderToId = entryData.entryId;
            } else {
                folderToId = jq(this).attr("data-id");
                if (withoutTrash && folderToId == ASC.Files.Constants.FOLDER_ID_TRASH) {
                    folderToId = null;
                }
            }

            if (ASC.Files.Common.isCorrectId(folderToId)) {
                var folderToObj = entryData ? entryData.entryObject : ASC.Files.UI.getEntryObject("folder", folderToId);

                if (folderToId != ASC.Files.Folders.currentFolder.id
                    && ASC.Files.UI.accessibleItem(entryData, folderToObj)
                    && (folderToId != ASC.Files.Constants.FOLDER_ID_TRASH
                        || ASC.Files.UI.accessibleItem())) {
                    jq(this).addClass(cssClass);
                }
            }
        });
    };

    var getOverFolderId = function (e, entry, addCssClass) {
        e = ASC.Files.Common.fixEvent(e);
        if (e.type == "mouseleave") {
            jq("." + addCssClass).removeClass(addCssClass);
            return null;
        }

        var treeLeft = jq("#treeViewContainer").offset().left;
        var treeRight = treeLeft + jq("#treeViewContainer")[0].offsetWidth;
        //it check long folder title in tree
        if (jq(this).is("#treeViewContainer *") && (treeLeft > e.pageX || treeRight < e.pageX)) {
            return null;
        }

        var folderToId;
        var entryObj = ASC.Files.UI.getObjectData(entry);

        if (entryObj) {
            folderToId = entryObj.entryId;
        } else {
            folderToId = jq(entry).attr("data-id");
        }

        if (ASC.Files.Common.isCorrectId(folderToId)) {
            jq(entry).addClass(addCssClass);
            return folderToId;
        }
        return null;
    };

    var preparingMoveTo = function (event) {
        var e = ASC.Files.Common.fixEvent(event);

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1))) {
            return false;
        }

        if (ASC.Files.Folders.folderContainer == "trash") {
            return false;
        }

        if (jq("#treeViewPanelSelector").length == 0) {
            return false;
        }

        if (jq("#promptRename").length != 0) {
            return true;
        }

        ASC.Files.Mouse.moveToX = e.pageX;
        ASC.Files.Mouse.moveToY = e.pageY;

        jq("body")
            .unbind("mouseout.MouseMove mousemove.MouseMove")
            .bind("mouseout.MouseMove mousemove.MouseMove", ASC.Files.Mouse.beginMoveTo);
        return true;
    };

    var beginMoveTo = function (e) {
        e = ASC.Files.Common.fixEvent(e);

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1))
            || ASC.Files.Mouse.mouseBtn == false) {
            jq("body").unbind("mouseout.MouseMove mousemove.MouseMove");
            return false;
        }

        if (Math.abs(e.pageX - ASC.Files.Mouse.moveToX) < 5
            && Math.abs(e.pageY - ASC.Files.Mouse.moveToY) < 5) {
            return false;
        }

        ASC.Files.Actions.hideAllActionPanels();

        jq("#filesMainContent .row-hover").removeClass("row-hover");
        jq(".may-row-to").removeClass("may-row-to");

        jq("body").addClass("user-select-none file-mouse-move");

        ASC.Files.Mouse.highlightFolderTo("may-row-to");

        jq("body")
            .unbind("mouseout.MouseMove mousemove.MouseMove mouseup.MouseMove")
            .bind("mouseout.MouseMove mousemove.MouseMove", ASC.Files.Mouse.continueMoveTo)
            .bind("mouseup.MouseMove", ASC.Files.Mouse.finishMoveTo)
            .on("mouseenter.MouseMove mouseleave.MouseMove", ".may-row-to", ASC.Files.Mouse.checkMoveTo);
        return true;
    };

    var continueMoveTo = function (e) {
        e = ASC.Files.Common.fixEvent(e);

        if (ASC.Files.Mouse.mouseBtn == false) {
            ASC.Files.Mouse.finishMoveTo(e);
            return true;
        }

        if (jq("#filesMovingTooltip").length == 0) {
            var list = jq("#filesMainContent .file-row:has(.checkbox input:checked)");

            var textInfo;
            if (list.length == 1) {
                textInfo = ASC.Files.UI.getObjectTitle(list[0]);
            } else {
                textInfo = ASC.Files.FilesJSResources.InfoSelectCount.format(list.length);
            }
            textInfo = ASC.Files.FilesJSResources.InfoSelectingDescribe.format("<b>" + textInfo + "</b><br/>");

            jq("#filesMainContent").parent().after("<div id=\"filesMovingTooltip\"></div>");
            jq("#filesMovingTooltip").html(textInfo);
        }
        jq("#filesMovingTooltip").css({ "left": e.pageX + "px", "top": e.pageY + "px" });

        if (!ASC.Files.UI.accessibleItem() || e.ctrlKey) {
            jq("body").addClass("file-mouse-copy");
        } else {
            jq("body").removeClass("file-mouse-copy");
        }

        if (jq.browser.opera) {
            var target = e.target || e.srcElement;
            target = jq(target);
            if (!target.hasClass("file-row")) {
                target = jq(target).closest(".file-row");
            }

            var nameFix = "fix_select_text";
            var el = target.children("#" + nameFix);
            if (el.length == 0) {
                jq("#" + nameFix).remove();
                el = document.createElement("INPUT");
                el.style.width = 0;
                el.style.height = 0;
                el.style.border = 0;
                el.style.margin = 0;
                el.style.padding = 0;
                el.id = nameFix;
                el.disabled = true;

                target.prepend(el);
                el = jq("#" + nameFix);
            }

            try {
                el.focus();
            } catch (e) {
                el.disabled = false;
                el.focus();
                el.disabled = true;
            }
        }
        return true;
    };

    var checkMoveTo = function (e) {
        ASC.Files.Mouse.moveToFolder = ASC.Files.Mouse.getOverFolderId(e, this, "row-to");
    };

    var finishMoveTo = function (e) {
        e = ASC.Files.Common.fixEvent(e);

        jq(".row-to").removeClass("row-to");
        jq(".may-row-to").removeClass("may-row-to");
        jq("body").removeClass("user-select-none file-mouse-move file-mouse-copy");

        var folderToId = ASC.Files.Mouse.moveToFolder;
        ASC.Files.Mouse.moveToFolder = null;
        if (ASC.Files.Common.isCorrectId(folderToId)) {
            if (folderToId == ASC.Files.Constants.FOLDER_ID_TRASH) {
                ASC.Files.Folders.deleteItem();
            } else {
                ASC.Files.Folders.isCopyTo = !ASC.Files.UI.accessibleItem() || e && e.ctrlKey === true;

                var folderToTitle = ASC.Files.UI.getEntryTitle("folder", folderToId);

                ASC.Files.Folders.curItemFolderMoveTo(folderToId, folderToTitle);
            }
        }

        ASC.Files.Folders.isCopyTo = false;
        jq("body").unbind("mouseout.MouseMove mousemove.MouseMove mouseup.MouseMove mouseenter.MouseMove mouseleave.MouseMove");

        jq("#filesMovingTooltip").remove();
    };

    var overCompactTitle = function () {
        var entryObj = jq(this);

        ASC.Files.UI.hideEntryTooltip();
        entryObj.one("mouseleave", function () {
            ASC.Files.UI.hideEntryTooltip();
        });

        var entryData = ASC.Files.UI.getObjectData(entryObj);
        var entryId = entryData.entryId;
        var entryType = entryData.entryType;

        entryObj.attr("data-title", true);
        entryObj.removeAttr("title");

        ASC.Files.UI.timeTooltip = setTimeout(function () {
            ASC.Files.UI.displayEntryTooltip(entryData.entryObject, entryType, entryId);
        }, 750);
    };

    return {
        init: init,

        mouseBtn: mouseBtn,
        mouseSelector: mouseSelector,
        moveToX: moveToX,
        moveToY: moveToY,
        moveToFolder: moveToFolder,
        updateMainContentArea: updateMainContentArea,

        handleMove: handleMove,

        collectEntryItems: collectEntryItems,
        beginSelecting: beginSelecting,
        continueSelecting: continueSelecting,
        finishSelecting: finishSelecting,

        preparingMoveTo: preparingMoveTo,
        beginMoveTo: beginMoveTo,
        continueMoveTo: continueMoveTo,
        checkMoveTo: checkMoveTo,
        finishMoveTo: finishMoveTo,

        overCompactTitle: overCompactTitle,

        highlightFolderTo: highlightFolderTo,
        getOverFolderId: getOverFolderId,
    };
})();

(function ($) {
    ASC.Files.Mouse.init();

    $(function () {
        jq("#filesMainContent").on("mousedown", ".file-row:not(.checkloading):not(.new-folder):not(.new-file):not(.error-entry):has(.checkbox input:checked)", ASC.Files.Mouse.preparingMoveTo);

        jq("#mainContent").on("mouseover", "#filesMainContent.compact .file-row:not(.checkloading):not(.new-folder):not(.new-file) .entry-title .name a", ASC.Files.Mouse.overCompactTitle);

        jq(document).on("mousedown.MouseSelect", "#studioPageContent:has(#filesMainContent .file-row:visible:not(.checkloading):not(.new-folder):not(.new-file))", ASC.Files.Mouse.beginSelecting);

        jq(document).bind("mouseup.Mouse", function () {
            ASC.Files.Mouse.mouseBtn = false;
        });

        jq("#filesMainContent").on("mouseenter mouseleave", ".file-row:not(.checkloading)", ASC.Files.Mouse.handleMove);
    });
})(jQuery);