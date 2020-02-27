/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


window.ASC.Files.Filter = (function () {
    var isInit = false;
    var advansedFilter = null;

    var init = function () {
        if (!isInit) {

            var filterOptions = {
                anykey: true,
                anykeytimeout: 800,
                colcount: 3,
                filters: getFilterItems(),
                hintDefaultDisable: true,
                maxlength: ASC.Files.Constants.MAX_NAME_LENGTH,
                sorters: getSorterItems()
            };

            ASC.Files.Filter.advansedFilter =
                jq(".files-filter div")
                    .advansedFilter(filterOptions)
                    .bind("setfilter", ASC.Files.Filter.setFilter)
                    .bind("resetfilter", ASC.Files.Filter.setFilter);
        }
    };

    var getFilterItems = function () {
        var filterItems = [
            {
                id: "selected-type-folder",
                filtertitle: ASC.Files.FilesJSResources.ButtonFilterType,
                group: ASC.Files.FilesJSResources.ButtonFilterType,
                groupby: "type",
                options: [
                    {
                        def: true,
                        title: ASC.Files.FilesJSResources.ButtonFilterFolder,
                        value: ASC.Files.Constants.FilterType.FoldersOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterDocument,
                        value: ASC.Files.Constants.FilterType.DocumentsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterPresentation,
                        value: ASC.Files.Constants.FilterType.PresentationsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet,
                        value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterImage,
                        value: ASC.Files.Constants.FilterType.ImagesOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterMedia,
                        value: ASC.Files.Constants.FilterType.MediaOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterArchive,
                        value: ASC.Files.Constants.FilterType.ArchiveOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFiles,
                        value: ASC.Files.Constants.FilterType.FilesOnly
                    }],
                type: "combobox",
                title: ASC.Files.FilesJSResources.ButtonFilterFolder
            },
            {
                id: "selected-type-text",
                filtertitle: ASC.Files.FilesJSResources.ButtonFilterType,
                group: ASC.Files.FilesJSResources.ButtonFilterType,
                groupby: "type",
                options: [
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFolder,
                        value: ASC.Files.Constants.FilterType.FoldersOnly
                    },
                    {
                        def: true,
                        title: ASC.Files.FilesJSResources.ButtonFilterDocument,
                        value: ASC.Files.Constants.FilterType.DocumentsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterPresentation,
                        value: ASC.Files.Constants.FilterType.PresentationsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet,
                        value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterImage,
                        value: ASC.Files.Constants.FilterType.ImagesOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterMedia,
                        value: ASC.Files.Constants.FilterType.MediaOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterArchive,
                        value: ASC.Files.Constants.FilterType.ArchiveOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFiles,
                        value: ASC.Files.Constants.FilterType.FilesOnly
                    }],
                type: "combobox",
                title: ASC.Files.FilesJSResources.ButtonFilterDocument
            },
            {
                id: "selected-type-presentation",
                filtertitle: ASC.Files.FilesJSResources.ButtonFilterType,
                group: ASC.Files.FilesJSResources.ButtonFilterType,
                groupby: "type",
                options: [
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFolder,
                        value: ASC.Files.Constants.FilterType.FoldersOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterDocument,
                        value: ASC.Files.Constants.FilterType.DocumentsOnly
                    },
                    {
                        def: true,
                        title: ASC.Files.FilesJSResources.ButtonFilterPresentation,
                        value: ASC.Files.Constants.FilterType.PresentationsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet,
                        value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterImage,
                        value: ASC.Files.Constants.FilterType.ImagesOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterMedia,
                        value: ASC.Files.Constants.FilterType.MediaOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterArchive,
                        value: ASC.Files.Constants.FilterType.ArchiveOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFiles,
                        value: ASC.Files.Constants.FilterType.FilesOnly
                    }],
                type: "combobox",
                title: ASC.Files.FilesJSResources.ButtonFilterPresentation
            },
            {
                id: "selected-type-spreadsheet",
                filtertitle: ASC.Files.FilesJSResources.ButtonFilterType,
                group: ASC.Files.FilesJSResources.ButtonFilterType,
                groupby: "type",
                options: [
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFolder,
                        value: ASC.Files.Constants.FilterType.FoldersOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterDocument,
                        value: ASC.Files.Constants.FilterType.DocumentsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterPresentation,
                        value: ASC.Files.Constants.FilterType.PresentationsOnly
                    },
                    {
                        def: true,
                        title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet,
                        value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterImage,
                        value: ASC.Files.Constants.FilterType.ImagesOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterMedia,
                        value: ASC.Files.Constants.FilterType.MediaOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterArchive,
                        value: ASC.Files.Constants.FilterType.ArchiveOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFiles,
                        value: ASC.Files.Constants.FilterType.FilesOnly
                    }],
                type: "combobox",
                title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet
            },
            {
                id: "selected-type-image",
                filtertitle: ASC.Files.FilesJSResources.ButtonFilterType,
                group: ASC.Files.FilesJSResources.ButtonFilterType,
                groupby: "type",
                options: [
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFolder,
                        value: ASC.Files.Constants.FilterType.FoldersOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterDocument,
                        value: ASC.Files.Constants.FilterType.DocumentsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterPresentation,
                        value: ASC.Files.Constants.FilterType.PresentationsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet,
                        value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
                    },
                    {
                        def: true,
                        title: ASC.Files.FilesJSResources.ButtonFilterImage,
                        value: ASC.Files.Constants.FilterType.ImagesOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterMedia,
                        value: ASC.Files.Constants.FilterType.MediaOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterArchive,
                        value: ASC.Files.Constants.FilterType.ArchiveOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFiles,
                        value: ASC.Files.Constants.FilterType.FilesOnly
                    }],
                type: "combobox",
                title: ASC.Files.FilesJSResources.ButtonFilterImage
            },
            {
                id: "selected-type-media",
                filtertitle: ASC.Files.FilesJSResources.ButtonFilterType,
                group: ASC.Files.FilesJSResources.ButtonFilterType,
                groupby: "type",
                options: [
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFolder,
                        value: ASC.Files.Constants.FilterType.FoldersOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterDocument,
                        value: ASC.Files.Constants.FilterType.DocumentsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterPresentation,
                        value: ASC.Files.Constants.FilterType.PresentationsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet,
                        value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterImage,
                        value: ASC.Files.Constants.FilterType.ImagesOnly
                    },
                    {
                        def: true,
                        title: ASC.Files.FilesJSResources.ButtonFilterMedia,
                        value: ASC.Files.Constants.FilterType.MediaOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterArchive,
                        value: ASC.Files.Constants.FilterType.ArchiveOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFiles,
                        value: ASC.Files.Constants.FilterType.FilesOnly
                    }],
                type: "combobox",
                title: ASC.Files.FilesJSResources.ButtonFilterMedia
            },
            {
                id: "selected-type-archive",
                filtertitle: ASC.Files.FilesJSResources.ButtonFilterType,
                group: ASC.Files.FilesJSResources.ButtonFilterType,
                groupby: "type",
                options: [
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFolder,
                        value: ASC.Files.Constants.FilterType.FoldersOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterDocument,
                        value: ASC.Files.Constants.FilterType.DocumentsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterPresentation,
                        value: ASC.Files.Constants.FilterType.PresentationsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet,
                        value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterImage,
                        value: ASC.Files.Constants.FilterType.ImagesOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterMedia,
                        value: ASC.Files.Constants.FilterType.MediaOnly
                    },
                    {
                        def: true,
                        title: ASC.Files.FilesJSResources.ButtonFilterArchive,
                        value: ASC.Files.Constants.FilterType.ArchiveOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFiles,
                        value: ASC.Files.Constants.FilterType.FilesOnly
                    }],
                type: "combobox",
                title: ASC.Files.FilesJSResources.ButtonFilterArchive
            },
            {
                id: "selected-type-file",
                filtertitle: ASC.Files.FilesJSResources.ButtonFilterType,
                group: ASC.Files.FilesJSResources.ButtonFilterType,
                groupby: "type",
                options: [
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterFolder,
                        value: ASC.Files.Constants.FilterType.FoldersOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterDocument,
                        value: ASC.Files.Constants.FilterType.DocumentsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterPresentation,
                        value: ASC.Files.Constants.FilterType.PresentationsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterSpreadsheet,
                        value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterImage,
                        value: ASC.Files.Constants.FilterType.ImagesOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterMedia,
                        value: ASC.Files.Constants.FilterType.MediaOnly
                    },
                    {
                        title: ASC.Files.FilesJSResources.ButtonFilterArchive,
                        value: ASC.Files.Constants.FilterType.ArchiveOnly
                    },
                    {
                        def: true,
                        title: ASC.Files.FilesJSResources.ButtonFilterFiles,
                        value: ASC.Files.Constants.FilterType.FilesOnly
                    }],
                type: "combobox",
                title: ASC.Files.FilesJSResources.ButtonFilterFiles
            }
        ];

        if (ASC.Resources.Master.Personal != true) {
            filterItems.push({
                id: "selected-person",
                group: ASC.Files.FilesJSResources.ButtonFilterAuthor,
                groupby: "owner",
                title: ASC.Files.FilesJSResources.Users,
                type: "person"
            });

            filterItems.push({
                id: "selected-group",
                group: ASC.Files.FilesJSResources.ButtonFilterAuthor,
                groupby: "owner",
                title: ASC.Files.FilesJSResources.Departments,
                type: "group"
            });
        }

        var isRootTrash = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TRASH;

        if (jq(".files-filter").data("content")) {
            filterItems.push({
                id: "with-content",
                group: ASC.Files.FilesJSResources.ButtonFilterOther,
                title: ASC.Files.FilesJSResources.ButtonFilterInContent,
                type: "flag",
                visible: !isRootTrash
            });
        }

        filterItems.push({
            id: "current-folder",
            group: ASC.Files.FilesJSResources.ButtonFilterOther,
            title: ASC.Files.FilesJSResources.ButtonFilterCurrentFolderOnly,
            type: "flag",
            visible: !isRootTrash && (!ASC.Files.ThirdParty || !ASC.Files.ThirdParty.isThirdParty())
        });

        return filterItems;
    };

    var getSorterItems = function () {
        var startOrderBy = getOrderDefault();
        
        var sorterItems = [
            {
                def: (startOrderBy.property == "DateAndTime"),
                dsc: (startOrderBy.property == "DateAndTime" ? !startOrderBy.is_asc : false),
                id: "DateAndTime",
                title: ASC.Files.FilesJSResources.ButtonSortModifiedNew
            },
            {
                def: (startOrderBy.property == "DateAndTimeCreation"),
                dsc: (startOrderBy.property == "DateAndTimeCreation" ? !startOrderBy.is_asc : false),
                id: "DateAndTimeCreation",
                title: ASC.Files.FilesJSResources.ButtonSortCreated
            },
            {
                def: (startOrderBy.property == "AZ"),
                dsc: (startOrderBy.property == "AZ" ? !startOrderBy.is_asc : true),
                id: "AZ",
                title: ASC.Files.FilesJSResources.ButtonSortName
            },
            {
                def: (startOrderBy.property == "Type"),
                dsc: (startOrderBy.property == "Type" ? !startOrderBy.is_asc : true),
                id: "Type",
                title: ASC.Files.FilesJSResources.ButtonSortType
            },
            {
                def: (startOrderBy.property == "Size"),
                dsc: (startOrderBy.property == "Size" ? !startOrderBy.is_asc : false),
                id: "Size",
                title: ASC.Files.FilesJSResources.ButtonSortSize
            }
        ];

        if (ASC.Resources.Master.Personal != true) {
            sorterItems.push({
                def: (startOrderBy.property == "Author"),
                dsc: (startOrderBy.property == "Author" ? !startOrderBy.is_asc : false),
                id: "Author",
                title: ASC.Files.FilesJSResources.ButtonSortAuthor
            });
        }

        return sorterItems;
    };

    var disableFilter = function () {
        ASC.Files.Filter.advansedFilter.advansedFilter({
            filters: getFilterItems()
        });
    };

    var setFilter = function (evt, $container, filter, params, selectedfilters) {
        if (!isInit) {
            isInit = true;
            return;
        }

        ASC.Files.Anchor.navigationSet(ASC.Files.Folders.currentFolder.id, false);
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

    var setWithoutSubfolders = function () {
        ASC.Files.Filter.advansedFilter.advansedFilter({
            filters: [
                { id: "current-folder", params: {}}
            ]
        });
    };

    var resize = function () {
        ASC.Files.Filter.advansedFilter.advansedFilter("resize");
    };

    var getDefaultFilterSettings = function () {
        var settings =
            {
                sorter: getOrderDefault(),
                text: "",
                filter: ASC.Files.Constants.FilterType.None,
                subject: "",
                isSet: false,
                withSubfolders: false,
                searchInContent: false
            };
        return settings;
    };

    var getFilterSettings = function () {
        var settings = ASC.Files.Filter.getDefaultFilterSettings();

        if (ASC.Files.Filter.advansedFilter == null) {
            return settings;
        }

        var currentFolderOnly = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TRASH;
        var param = ASC.Files.Filter.advansedFilter.advansedFilter();
        jq(param).each(function (i, item) {
            switch (item.id) {
                case "sorter":
                    var curOrderBy = getOrderBy(item.params.id, !item.params.dsc);
                    settings.sorter = curOrderBy;
                    break;
                case "text":
                    settings.text = item.params.value;
                    settings.isSet = true;
                    break;
                case "selected-person":
                    settings.subject = item.params.id;
                    settings.isSet = true;
                    break;
                case "selected-group":
                    settings.subjectGroup = true;
                    settings.subject = item.params.id;
                    settings.isSet = true;
                    break;
                case "selected-type-folder":
                case "selected-type-text":
                case "selected-type-presentation":
                case "selected-type-spreadsheet":
                case "selected-type-image":
                case "selected-type-media":
                case "selected-type-archive":
                case "selected-type-file":
                    settings.filter = parseInt(item.params.value || ASC.Files.Constants.FilterType.None);
                    settings.isSet = settings.filter != ASC.Files.Constants.FilterType.None;
                    break;
                case "current-folder":
                    currentFolderOnly = true;
                    break;
                case "with-content":
                    settings.searchInContent = true;
                    break;
            }
        });

        if (!currentFolderOnly && settings.isSet) {
            settings.withSubfolders = true;
        }

        if (ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TRASH) {
            settings.searchInContent = false;
        }

        return settings;
    };

    var getOrderDefault = function () {
        return getOrderBy(jq(".files-filter").data("sort"), jq(".files-filter").data("asc"));
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

        getDefaultFilterSettings: getDefaultFilterSettings,
        getFilterSettings: getFilterSettings,

        setFilter: setFilter,
        clearFilter: clearFilter,
        setWithoutSubfolders: setWithoutSubfolders,
        resize: resize
    };
})();

(function ($) {
    $(function () {
        ASC.Files.Filter.init();

        jq(".files-clear-filter").click(function () {
            ASC.Files.Filter.clearFilter();
            return false;
        });

        jq("#filesWithoutSubfolders").click(function () {
            ASC.Files.Filter.setWithoutSubfolders();
            return false;
        });
    });
})(jQuery);
