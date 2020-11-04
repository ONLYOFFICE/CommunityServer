/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


window.ASC.Files.Mouse = (function () {
    var isInit = false;

    var moveToX = 0;
    var moveToY = 0;
    var moveToFolder = "";

    var mainContentArea = null;

    var mouseBtn = false;
    var disableHover = false;

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
        if (ASC.Files.Mouse.mouseBtn || ASC.Files.Mouse.disableHover) {
            return true;
        }

        e = jq.fixEvent(e);
        this.classList.toggle("row-hover", e.type == "mouseenter");

        return true;
    };

    var forbiddenTarget = function (target) {
        if (target.nodeName == "HTML") {
            return true;
        }

        if (jq(".popup-modal:visible").length != 0) {
            return true;
        }

        if (ASC.Files.MediaPlayer && ASC.Files.MediaPlayer.isView) {
            return true;
        }

        if (!jq(target).is(
            "nav,\
             .nav-content,\
             .page-menu,\
             main,\
             .filter-content,\
             .mainPageTableSidePanel,\
             .mainPageContent,\
             .files-content-panel,\
             #mainContentHeader,\
             #mainContent,\
             .file-row:not(.row-rename):not(.row-selected),\
             .file-row:not(.row-rename):not(.row-selected) *")) {
            return true;
        }

        return jq(target).is(".textEdit, .checkbox, input");
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
        e = jq.fixEvent(e);
        ASC.Files.Mouse.mouseBtn = e.target.nodeName != "HTML";

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1))) {
            return true;
        }

        var target = e.target || e.srcElement;

        try {
            if (forbiddenTarget(target)) {
                return true;
            }
        } catch (ex) {
            return true;
        }

        if (jq(target)
            .is(".folder-row:not(.error-entry) .thumb-folder," +
                ".file-row:not(.error-entry) .thumb-file," +
                ".file-row:not(.error-entry) .entry-title .name a," +
                ".file-row:not(.error-entry) .entry-title .name span")) {
            if (ASC.Files.Folders.folderContainer !== "trash") {
                ASC.Files.UI.clickRow(e, jq(target).closest(".file-row"));

                ASC.Files.Mouse.preparingMoveTo(e);
            }

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

    var continueSelecting = function (e) {
        e = jq.fixEvent(e);

        var targetMove = e.target || e.srcElement;
        if (typeof targetMove == "undefined") {
            return true;
        }

        var selectDelta = 2;
        var posXnew = Math.min(e.pageX, mainContentArea.documentWidth - selectDelta);
        var posYnew = Math.min(e.pageY, mainContentArea.documentHeight - selectDelta);

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
            ASC.Files.Mouse.disableHover = true;
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

        if (!e.ctrlKey && !intersectionRectangles(itemObj, selectObj)) {
            ASC.Files.UI.checkSelectAll(false);
        } else {
            var selectionChanged = false;

            var itemsCount = ASC.Files.Mouse.mouseSelector.entryItems.length;
            for (var i = itemsCount; i; i--) {
                itemObj = ASC.Files.Mouse.mouseSelector.entryItems[itemsCount - i];
                if (intersectionRectangles(itemObj, selectObj)) {
                    selectionChanged = ASC.Files.UI.selectRow(itemObj.entryObj, true) || selectionChanged;
                } else {
                    if (!e.ctrlKey) {
                        selectionChanged = ASC.Files.UI.selectRow(itemObj.entryObj, false) || selectionChanged;
                    }
                }
            }

            if (selectionChanged) {
                clearTimeout(timeoutUpdate);
                timeoutUpdate = setTimeout(ASC.Files.UI.updateMainContentHeader, 10);
            }
        }

        autoScroll(e);

        return false;
    };

    var autoScroll = function (e) {
        var scrollZone = 50;
        var deltaScroll = 15;
        if (scrollZone > Math.abs(e.pageY - document.documentElement.scrollTop)) {
            window.scrollBy(0, -deltaScroll);
        }
        if (scrollZone > Math.abs(e.pageY - (document.documentElement.scrollTop + document.documentElement.clientHeight))) {
            window.scrollBy(0, deltaScroll);
        }
    };

    var finishSelecting = function () {
        jq("#filesSelector").remove();
        ASC.Files.Mouse.mouseSelector.startX = 0;
        ASC.Files.Mouse.mouseSelector.startY = 0;
        ASC.Files.Mouse.mouseSelector.entryItems = new Array();
        ASC.Files.Mouse.mouseBtn = false;
        ASC.Files.Mouse.disableHover = false;

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
                    && ASC.Files.UI.accessEdit(entryData, folderToObj)
                    && (folderToId != ASC.Files.Constants.FOLDER_ID_TRASH
                        || ASC.Files.UI.accessEdit())) {
                    jq(this).addClass(cssClass);
                }
            }
        });
    };

    var getOverFolderId = function (e, entry, addCssClass) {
        e = jq.fixEvent(e);
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
        var entryData = ASC.Files.UI.getObjectData(entry);

        if (entryData) {
            folderToId = entryData.entryId;
        } else {
            folderToId = jq(entry).attr("data-id");
        }

        if (ASC.Files.Common.isCorrectId(folderToId)) {
            jq(entry).addClass(addCssClass);
            return folderToId;
        }
        return null;
    };

    var preparingMoveTo = function (e) {
        e = jq.fixEvent(e);

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1))) {
            return false;
        }

        if (ASC.Files.Folders.folderContainer == "trash"
            || ASC.Files.Folders.folderContainer == "privacy") {
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
            .unbind("mouseout.MouseMove mousemove.MouseMove keyup.MouseMove keydown.MouseMove")
            .bind("mouseout.MouseMove mousemove.MouseMove", ASC.Files.Mouse.beginMoveTo);
        return true;
    };

    var beginMoveTo = function (e) {
        e = jq.fixEvent(e);

        if (!(e.button == 0 || (jq.browser.msie && e.button == 1))
            || ASC.Files.Mouse.mouseBtn == false) {
            jq("body").unbind("mouseout.MouseMove mousemove.MouseMove keyup.MouseMove keydown.MouseMove");
            return false;
        }

        if (Math.abs(e.pageX - ASC.Files.Mouse.moveToX) < 5
            && Math.abs(e.pageY - ASC.Files.Mouse.moveToY) < 5) {
            return false;
        }

        ASC.Files.Actions.hideAllActionPanels();

        jq("#contentVersions").removeClass("version-highlight");
        jq("#filesMainContent .row-hover").removeClass("row-hover");
        jq(".may-row-to").removeClass("may-row-to");

        jq("body").addClass("user-select-none file-mouse-move");

        ASC.Files.Mouse.highlightFolderTo("may-row-to");

        jq("body")
            .unbind("mouseout.MouseMove mousemove.MouseMove mouseup.MouseMove keyup.MouseMove keydown.MouseMove")
            .bind("mouseout.MouseMove mousemove.MouseMove", ASC.Files.Mouse.continueMoveTo)
            .bind("mouseup.MouseMove", ASC.Files.Mouse.finishMoveTo)
            .bind("keyup.MouseMove keydown.MouseMove", function (ev) {
                ev = jq.fixEvent(ev);
                var code = ev.keyCode || ev.which;
                if (code == ASC.Files.Common.keyCode.ctrl) {
                    return ASC.Files.Mouse.continueMoveTo(ev);
                }
                return true;
            })
            .on("mouseenter.MouseMove mouseleave.MouseMove", ".may-row-to", ASC.Files.Mouse.checkMoveTo);
        return true;
    };

    var continueMoveTo = function (e) {
        e = jq.fixEvent(e);

        if (ASC.Files.Mouse.mouseBtn == false) {
            ASC.Files.Mouse.finishMoveTo(e);
            return true;
        }

        if (!ASC.Files.UI.accessEdit() || !ASC.Files.UI.accessDelete() || e.ctrlKey) {
            var textFormat = ASC.Files.FilesJSResources.InfoCopyDescribe;
            jq("body").addClass("file-mouse-copy");
        } else {
            textFormat = ASC.Files.FilesJSResources.InfoMoveDescribe;
            jq("body").removeClass("file-mouse-copy");
        }

        if (jq("#filesMovingTooltip").length == 0) {
            jq("#filesMainContent").parent().after("<div id=\"filesMovingTooltip\" class=\"studio-action-panel\"></div>");
        }

        var textInfo;
        var list = jq("#filesMainContent .file-row:has(.checkbox input:checked)");
        if (list.length == 1) {
            textInfo = ASC.Files.UI.getObjectTitle(list[0]);
        } else {
            textInfo = ASC.Files.FilesJSResources.InfoCountDescribe.format(list.length);
        }
        textInfo = textFormat.format(textInfo, "<b>", "</b>", "<br/>");
        jq("#filesMovingTooltip").html(textInfo);
        var target = e.target || e.srcElement;
        if (!jq(target).is("body")) {
            jq("#filesMovingTooltip").css({"left": e.pageX + "px", "top": e.pageY + "px"});
        }

        if (jq.browser.opera) {
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

        autoScroll(e);

        return true;
    };

    var checkMoveTo = function (e) {
        ASC.Files.Mouse.moveToFolder = ASC.Files.Mouse.getOverFolderId(e, this, "row-to");
    };

    var finishMoveTo = function (e) {
        e = jq.fixEvent(e);

        jq("#contentVersions").addClass("version-highlight");
        jq(".row-to").removeClass("row-to");
        jq(".may-row-to").removeClass("may-row-to");
        jq("body").removeClass("user-select-none file-mouse-move file-mouse-copy");

        var folderToId = ASC.Files.Mouse.moveToFolder;
        ASC.Files.Mouse.moveToFolder = null;
        if (ASC.Files.Common.isCorrectId(folderToId)) {
            if (folderToId == ASC.Files.Constants.FOLDER_ID_TRASH) {
                ASC.Files.Folders.deleteItem();
            } else {
                ASC.Files.Folders.isCopyTo = !ASC.Files.UI.accessEdit() || !ASC.Files.UI.accessDelete() || e && e.ctrlKey === true;

                var folderToTitle = ASC.Files.UI.getEntryTitle("folder", folderToId);

                ASC.Files.Folders.curItemFolderMoveTo(folderToId, folderToTitle);
            }
        }

        ASC.Files.Folders.isCopyTo = false;
        jq("body").unbind("mouseout.MouseMove mousemove.MouseMove mouseup.MouseMove mouseenter.MouseMove mouseleave.MouseMove keyup.MouseMove keydown.MouseMove");

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
        disableHover: disableHover,
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

        jq(document).on("dragstart", ".row-selected a", function (e) {
            e.preventDefault();
        });
    });
})(jQuery);