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
                hintDefaultDisable: false,
                maxlength: ASC.Files.Constants.MAX_NAME_LENGTH,
                sorters: getSorterItems()
            };

            ASC.Files.Filter.advansedFilter =
                jq(".files-filter div")
                    .advansedFilter(filterOptions)
                    .on("setfilter", ASC.Files.Filter.setFilter)
                    .on("resetfilter", ASC.Files.Filter.setFilter);
        }
    };

    var getFilterItems = function () {
        var isFavorites = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_FAVORITES;
        var isRecent = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_RECENT;
        var isTemplates = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TEMPLATES;

        var filterItems = [
            {
                id: "selected-type-folder",
                filtertitle: ASC.Files.FilesJSResource.ButtonFilterType,
                group: ASC.Files.FilesJSResource.ButtonFilterType,
                groupby: "type",
                options: getTypeOptions(ASC.Files.Constants.FilterType.FoldersOnly),
                type: "combobox",
                title: ASC.Files.FilesJSResource.ButtonFilterFolder,
                visible: !isRecent && !isTemplates
            },
            {
                id: "selected-type-text",
                filtertitle: ASC.Files.FilesJSResource.ButtonFilterType,
                group: ASC.Files.FilesJSResource.ButtonFilterType,
                groupby: "type",
                options: getTypeOptions(ASC.Files.Constants.FilterType.DocumentsOnly),
                type: "combobox",
                title: ASC.Files.FilesJSResource.ButtonFilterDocument
            },
            {
                id: "selected-type-presentation",
                filtertitle: ASC.Files.FilesJSResource.ButtonFilterType,
                group: ASC.Files.FilesJSResource.ButtonFilterType,
                groupby: "type",
                options: getTypeOptions(ASC.Files.Constants.FilterType.PresentationsOnly),
                type: "combobox",
                title: ASC.Files.FilesJSResource.ButtonFilterPresentation
            },
            {
                id: "selected-type-spreadsheet",
                filtertitle: ASC.Files.FilesJSResource.ButtonFilterType,
                group: ASC.Files.FilesJSResource.ButtonFilterType,
                groupby: "type",
                options: getTypeOptions(ASC.Files.Constants.FilterType.SpreadsheetsOnly),
                type: "combobox",
                title: ASC.Files.FilesJSResource.ButtonFilterSpreadsheet
            },
            {
                id: "selected-type-image",
                filtertitle: ASC.Files.FilesJSResource.ButtonFilterType,
                group: ASC.Files.FilesJSResource.ButtonFilterType,
                groupby: "type",
                options: getTypeOptions(ASC.Files.Constants.FilterType.ImagesOnly),
                type: "combobox",
                title: ASC.Files.FilesJSResource.ButtonFilterImage,
                visible: !isRecent && !isTemplates
            },
            {
                id: "selected-type-media",
                filtertitle: ASC.Files.FilesJSResource.ButtonFilterType,
                group: ASC.Files.FilesJSResource.ButtonFilterType,
                groupby: "type",
                options: getTypeOptions(ASC.Files.Constants.FilterType.MediaOnly),
                type: "combobox",
                title: ASC.Files.FilesJSResource.ButtonFilterMedia,
                visible: !isRecent && !isTemplates
            },
            {
                id: "selected-type-archive",
                filtertitle: ASC.Files.FilesJSResource.ButtonFilterType,
                group: ASC.Files.FilesJSResource.ButtonFilterType,
                groupby: "type",
                options: getTypeOptions(ASC.Files.Constants.FilterType.ArchiveOnly),
                type: "combobox",
                title: ASC.Files.FilesJSResource.ButtonFilterArchive,
                visible: !isRecent && !isTemplates
            },
            {
                id: "selected-type-file",
                filtertitle: ASC.Files.FilesJSResource.ButtonFilterType,
                group: ASC.Files.FilesJSResource.ButtonFilterType,
                groupby: "type",
                options: getTypeOptions(ASC.Files.Constants.FilterType.FilesOnly),
                type: "combobox",
                title: ASC.Files.FilesJSResource.ButtonFilterFiles,
                visible: !isFavorites && !isRecent && !isTemplates
            }
        ];

        if (ASC.Resources.Master.IsAuthenticated && !ASC.Resources.Master.Personal) {
            filterItems.push({
                id: "selected-person",
                group: ASC.Files.FilesJSResource.ButtonFilterAuthor,
                groupby: "owner",
                title: ASC.Files.FilesJSResource.Users,
                type: "person"
            });

            filterItems.push({
                id: "selected-group",
                group: ASC.Files.FilesJSResource.ButtonFilterAuthor,
                groupby: "owner",
                title: ASC.Files.FilesJSResource.Departments,
                type: "group"
            });
        }

        var isRootTrash = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TRASH;

        if (jq(".files-filter").data("content")) {
            filterItems.push({
                id: "with-content",
                group: ASC.Files.FilesJSResource.ButtonFilterOther,
                title: ASC.Files.FilesJSResource.ButtonFilterInContent,
                type: "flag",
                visible: !isRootTrash
            });
        }

        filterItems.push({
            id: "current-folder",
            group: ASC.Files.FilesJSResource.ButtonFilterOther,
            title: ASC.Files.FilesJSResource.ButtonFilterCurrentFolderOnly,
            type: "flag",
            visible: !isRootTrash && !isFavorites && !isRecent && !isTemplates
                && (!ASC.Files.ThirdParty || !ASC.Files.ThirdParty.isThirdParty())
        });

        return filterItems;
    };

    var getTypeOptions = function (defaultType) {
        var isFavorites = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_FAVORITES;
        var isRecent = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_RECENT;
        var isTemplates = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TEMPLATES;

        var options = [];
        if (!isRecent && !isTemplates) {
            options = [{
                def: defaultType == ASC.Files.Constants.FilterType.FoldersOnly,
                title: ASC.Files.FilesJSResource.ButtonFilterFolder,
                value: ASC.Files.Constants.FilterType.FoldersOnly
            }];
        }

        options.push({
                def: defaultType == ASC.Files.Constants.FilterType.DocumentsOnly,
                title: ASC.Files.FilesJSResource.ButtonFilterDocument,
                value: ASC.Files.Constants.FilterType.DocumentsOnly
            },
            {
                def: defaultType == ASC.Files.Constants.FilterType.PresentationsOnly,
                title: ASC.Files.FilesJSResource.ButtonFilterPresentation,
                value: ASC.Files.Constants.FilterType.PresentationsOnly
            },
            {
                def: defaultType == ASC.Files.Constants.FilterType.SpreadsheetsOnly,
                title: ASC.Files.FilesJSResource.ButtonFilterSpreadsheet,
                value: ASC.Files.Constants.FilterType.SpreadsheetsOnly
            });

        if (!isRecent && !isTemplates) {
            options.push({
                    def: defaultType == ASC.Files.Constants.FilterType.ImagesOnly,
                    title: ASC.Files.FilesJSResource.ButtonFilterImage,
                    value: ASC.Files.Constants.FilterType.ImagesOnly
                },
                {
                    def: defaultType == ASC.Files.Constants.FilterType.MediaOnly,
                    title: ASC.Files.FilesJSResource.ButtonFilterMedia,
                    value: ASC.Files.Constants.FilterType.MediaOnly
                },
                {
                    def: defaultType == ASC.Files.Constants.FilterType.ArchiveOnly,
                    title: ASC.Files.FilesJSResource.ButtonFilterArchive,
                    value: ASC.Files.Constants.FilterType.ArchiveOnly
                });
        }

        if (!isFavorites && !isRecent && !isTemplates) {
            options.push({
                def: defaultType == ASC.Files.Constants.FilterType.FilesOnly,
                title: ASC.Files.FilesJSResource.ButtonFilterFiles,
                value: ASC.Files.Constants.FilterType.FilesOnly
            });
        }

        return options;
    };

    var getSorterItems = function () {
        var startOrderBy = getOrderDefault();
        
        var sorterItems = [
            {
                def: (startOrderBy.property == "DateAndTime"),
                dsc: (startOrderBy.property == "DateAndTime" ? !startOrderBy.is_asc : false),
                id: "DateAndTime",
                title: ASC.Files.FilesJSResource.ButtonSortModifiedNew
            },
            {
                def: (startOrderBy.property == "DateAndTimeCreation"),
                dsc: (startOrderBy.property == "DateAndTimeCreation" ? !startOrderBy.is_asc : false),
                id: "DateAndTimeCreation",
                title: ASC.Files.FilesJSResource.ButtonSortCreated
            },
            {
                def: (startOrderBy.property == "AZ"),
                dsc: (startOrderBy.property == "AZ" ? !startOrderBy.is_asc : true),
                id: "AZ",
                title: ASC.Files.FilesJSResource.ButtonSortName
            },
            {
                def: (startOrderBy.property == "Type"),
                dsc: (startOrderBy.property == "Type" ? !startOrderBy.is_asc : true),
                id: "Type",
                title: ASC.Files.FilesJSResource.ButtonSortType
            },
            {
                def: (startOrderBy.property == "Size"),
                dsc: (startOrderBy.property == "Size" ? !startOrderBy.is_asc : false),
                id: "Size",
                title: ASC.Files.FilesJSResource.ButtonSortSize
            }
        ];

        if (ASC.Resources.Master.Personal != true) {
            sorterItems.push({
                def: (startOrderBy.property == "Author"),
                dsc: (startOrderBy.property == "Author" ? !startOrderBy.is_asc : false),
                id: "Author",
                title: ASC.Files.FilesJSResource.ButtonSortAuthor
            });
        }

        return sorterItems;
    };

    var disableFilter = function () {
        ASC.Files.Filter.advansedFilter.advansedFilter({
            filters: getFilterItems()
        });

        var isRecent = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_RECENT;
        ASC.Files.Filter.advansedFilter.advansedFilter("sort", !isRecent);
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

        var currentFolderOnly = ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TRASH
            || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_FAVORITES
            || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_RECENT
            || ASC.Files.Folders.currentFolder.id == ASC.Files.Constants.FOLDER_ID_TEMPLATES;
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

        jq(".files-clear-filter").on("click", function () {
            ASC.Files.Filter.clearFilter();
            return false;
        });

        jq("#filesWithoutSubfolders").on("click", function () {
            ASC.Files.Filter.setWithoutSubfolders();
            return false;
        });
    });
})(jQuery);
