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
window.ASC.Files.Filter = (function () {
    var isInit = false;
    var advansedFilter = null;

    var init = function () {
        if (!isInit) {

            var startOrderBy = ASC.Files.Filter.getOrderDefault();

            ASC.Files.Filter.advansedFilter =
                jq("#files_advansedFilter")
                    .advansedFilter({
                        anykey: true,
                        anykeytimeout: 800,
                        hintDefaultDisable: true,
                        maxfilters: 1,
                        maxlength: ASC.Files.Constants.MAX_NAME_LENGTH,
                        filters: [
                            {
                                type: "combobox",
                                id: "selected-type",
                                title: ASC.Files.FilesJSResources.Types,
                                options: [
                                    {value: ASC.Files.Constants.FilterType.FilesOnly, title: ASC.Files.FilesJSResources.ButtonFilterFile},
                                    {value: ASC.Files.Constants.FilterType.FoldersOnly, title: ASC.Files.FilesJSResources.ButtonFilterFolder, def: true},
                                    {value: ASC.Files.Constants.FilterType.DocumentsOnly, title: ASC.Files.FilesJSResources.ButtonFilterDocument},
                                    {value: ASC.Files.Constants.FilterType.PresentationsOnly, title: ASC.Files.FilesJSResources.ButtonFilterPresentation},
                                    {value: ASC.Files.Constants.FilterType.SpreadsheetsOnly, title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet},
                                    {value: ASC.Files.Constants.FilterType.ImagesOnly, title: ASC.Files.FilesJSResources.ButtonFilterImage}]
                            },
                            {type: "person", id: "selected-person", title: ASC.Files.FilesJSResources.Users},
                            {type: "group", id: "selected-group", title: ASC.Files.FilesJSResources.Departments}
                        ],
                        sorters: [
                            {id: "DateAndTime", title: ASC.Files.FilesJSResources.ButtonSortModified, def: (startOrderBy.property == "DateAndTime"), dsc: !startOrderBy.is_asc},
                            {id: "AZ", title: ASC.Files.FilesJSResources.ButtonSortName, def: (startOrderBy.property == "AZ"), dsc: !startOrderBy.is_asc},
                            {id: "Type", title: ASC.Files.FilesJSResources.ButtonSortType, def: (startOrderBy.property == "Type"), dsc: !startOrderBy.is_asc},
                            {id: "Size", title: ASC.Files.FilesJSResources.ButtonSortSize, def: (startOrderBy.property == "Size"), dsc: !startOrderBy.is_asc},
                            {id: "Author", title: ASC.Files.FilesJSResources.ButtonSortAuthor, def: (startOrderBy.property == "Author"), dsc: !startOrderBy.is_asc},
                            {id: "New", title: ASC.Files.FilesJSResources.ButtonSortNew, def: (startOrderBy.property == "New"), dsc: !startOrderBy.is_asc}
                        ]
                    })
                    .bind("setfilter", ASC.Files.Filter.setFilter)
                    .bind("resetfilter", ASC.Files.Filter.setFilter);
        }
    };

    var disableFilter = function () {
        var isMy = ASC.Files.Folders.folderContainer == "my";
        var isForMe = ASC.Files.Folders.folderContainer == "forme";
        var isRootProject = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_PROJECT;
        var isPersonal = ASC.Resources.Master.Personal == true;

        ASC.Files.Filter.advansedFilter.advansedfilter("sort", !isForMe && !isRootProject);

        ASC.Files.Filter.advansedFilter.advansedFilter({
            filters: [
                {id: "selected-type", visible: !isRootProject},
                {id: "selected-person", visible: !isMy && !isRootProject && !isPersonal},
                {id: "selected-group", visible: !isMy && !isRootProject && !isPersonal}
            ],
            sorters: [
                {id: "Author", visible: !isMy}
            ]
        });

        ASC.Files.Filter.advansedFilter.advansedFilter({
            nonetrigger: true,
            hasButton: !isRootProject,
            sorters: [
                {id: "New", visible: isForMe}
            ]
        });
    };

    var setFilter = function (evt, $container, filter, params, selectedfilters) {
        if (!isInit) {
            isInit = true;
            return;
        }

        ASC.Files.Anchor.navigationSet(ASC.Files.Folders.currentFolder.id, false, true);
    };

    var clearFilter = function (safeMode) {
        safeMode = safeMode === true;
        var mustTrue = false;
        if (safeMode && isInit) {
            mustTrue = true;
            isInit = false;
        }
        ASC.Files.Filter.advansedFilter.advansedFilter(null);
        if (safeMode && mustTrue) {
            isInit = true;
        }
    };

    var resize = function () {
        ASC.Files.Filter.advansedFilter.advansedFilter("resize");
    };

    var getFilterSettings = function (currentFolderId) {
        var settings =
            {
                sorter: ASC.Files.Filter.getOrderDefault(),
                text: "",
                filter: 0,
                subject: ""
            };

        if (ASC.Files.Filter.advansedFilter == null) {
            return settings;
        }

        var param = ASC.Files.Filter.advansedFilter.advansedFilter();
        jq(param).each(function (i, item) {
            switch (item.id) {
                case "sorter":
                    var curOrderBy = getOrderBy(item.params.id, !item.params.dsc);
                    settings.sorter = curOrderBy;
                    break;
                case "text":
                    settings.text = item.params.value;
                    break;
                case "selected-person":
                    settings.filter = 8;
                    settings.subject = item.params.id;
                    break;
                case "selected-group":
                    settings.filter = 9;
                    settings.subject = item.params.id;
                    break;
                case "selected-type":
                    settings.filter = parseInt(item.params.value || 0);
                    break;
            }
        });

        currentFolderId = currentFolderId || ASC.Files.Folders.currentFolder.id;

        if (currentFolderId == ASC.Files.Constants.FOLDER_ID_SHARE || ASC.Files.Folders.folderContainer == "forme") {
            settings.sorter = ASC.Files.Filter.getOrderByNew(false);
        }

        if (currentFolderId == ASC.Files.Constants.FOLDER_ID_PROJECT) {
            settings.sorter = ASC.Files.Filter.getOrderByAZ(true);
        }

        return settings;
    };

    var getOrderDefault = function () {
        return ASC.Files.Filter.getOrderByDateAndTime(false);
    };

    var getOrderByDateAndTime = function (asc) {
        return getOrderBy("DateAndTime", asc);
    };

    var getOrderByAZ = function (asc) {
        return getOrderBy("AZ", asc);
    };

    var getOrderByNew = function (asc) {
        return getOrderBy("New", asc);
    };

    var getOrderBy = function (name, asc) {
        name = (typeof name != "undefined" && name != "" ? name : "DateAndTime");
        asc = asc === true;
        return {
            is_asc: asc,
            property: name
        };
    };

    return {
        init: init,

        advansedFilter: advansedFilter,
        disableFilter: disableFilter,

        getFilterSettings: getFilterSettings,

        getOrderDefault: getOrderDefault,
        getOrderByDateAndTime: getOrderByDateAndTime,
        getOrderByAZ: getOrderByAZ,
        getOrderByNew: getOrderByNew,

        setFilter: setFilter,
        clearFilter: clearFilter,
        resize: resize
    };
})();

(function ($) {
    $(function () {
        ASC.Files.Filter.init();

        jq("#files_clearFilter").click(function () {
            ASC.Files.Filter.clearFilter();
            return false;
        });
    });
})(jQuery);
