/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


jq(document).ready(function () {
    var teamlab = Teamlab,
        projects = ASC.Projects,
        common = projects.Common,
        masterResource = ASC.Resources.Master.Resource,
        fileSelector = ASC.Files.FileSelector,
        folder,
        baseFolder = ASC.Files.Constants.FOLDER_ID_MY_FILES;

    var $folderSelectorBox,
        $box,
        $button;

    function init() {
        teamlab.getPrjSettings({
            success: function (params, data) {
                var modules = projects.Resources.StartModules;
                var currentModule = modules.find(function (item) {
                    return item.StartModuleType === data.startModuleType;
                }) || modules[0];

                data.startModuleTitle = currentModule.Title;

                jq("#settings").html(jq.tmpl("projects_settings", data));

                var $cbxEnableSettings = jq("#cbxEnableSettings"),
                    $cbxHideSettings = jq("#cbxHideSettings"),
                    $startModule = jq("#startModule");

                $cbxEnableSettings.on("change", function () {
                    updateSettings({ everebodyCanCreate: $cbxEnableSettings.is(":checked") });
                });

                $cbxHideSettings.on("change", function () {
                    updateSettings({ hideEntitiesInPausedProjects: $cbxHideSettings.is(":checked") });
                });


                $startModule.advancedSelector({
                    height: 26 * 4, //magic: itemsCount*itemHeight
                    itemsSelectedIds: [currentModule.StartModuleType],
                    onechosen: true,
                    showSearch: false,
                    itemsChoose: modules.map(function (item) { return { id: item.StartModuleType, title: item.Title } }),
                    sortMethod: function () { return 0; }
                })
                    .on("showList",
                        function (event, item) {
                            $startModule.html(item.title).attr("title", item.title);
                            updateSettings({ startModule: item.id });
                        });

                folder = data.folderId;

                initFolderSelector();
            }
        });
    }


    function updateSettings(data) {
        teamlab.updatePrjSettings(data,
        {
            success: function() {
                common.displayInfoPanel(masterResource.ChangesSuccessfullyAppliedMsg);
            },
            error: function() {
                common.displayInfoPanel(masterResource.CommonJSErrorMsg, true);
            }
        });
    }

    function initFolderSelector() {
        $folderSelectorBox = jq(".folderSelectorBox");
        $box = $folderSelectorBox.find("input");
        $button = $folderSelectorBox.find(".button");

        $button.on("click", showFolderPop);

        jq('#fileSelectorTree > ul > li.tree-node:not([data-id=\'' + baseFolder + '\'])').remove();

        fileSelector.onInit = function () {
            fileSelector.toggleThirdParty(true);

            fileSelector.onThirdPartyTreeCreated = function () {
                $('*:not(.disabled) > .settings-block .thirdPartyStorageSelectorBox')
                    .removeAttr('title')
                    .removeClass('disabled')
                    .find(':radio')
                    .attr('disabled', false);
            };
            fileSelector.createThirdPartyTree();
        };

        fileSelector.onSubmit = function (folderId) {
            displayFolderPath(folderId);
            updateSettings({ folderId: folderId });
        }

        fileSelector.setTitle(masterResource.SelectFolder);

        displayFolderPath();
    }
    
    function displayFolderPath(folderId) {
        if (typeof folderId != "undefined") {
            folder = folderId;
        }

        teamlab.getFolderPath(folder,
        {
            success: function (params, data) {
                var path = data.map(function (item) { return item.title; }),
                    pathTitle = path.join(' > ');
                $box.val(pathTitle);
            }
        });
    }

    function showFolderPop() {
        if ($button.is('.disable')) {
            return;
        }

        fileSelector.openDialog(folder, true, false);
    }

    init();
});