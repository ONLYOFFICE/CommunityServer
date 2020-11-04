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


ASC.Projects.Import = (function($) {
    var quotaEndFlag = false;
    var resources = ASC.Projects.Resources.ImportResource;
    var $chooseProjects,
        $importClosed,
        $sendInvitations,
        $agreement,
        $importAsCollaborators,
        $startImportButton,
        $importTools,
        $popupPanelBodyError,
        $popupImportErrorContainer,
        $popupImportErrorContainerHeader,
        $popupImportErrorContainerMessage,
        $importPeopleStatus,
        $importProjectsStatus,
        $importFilesStatus,
        $importProgress,
        $basecampProjectsContainer,
        $tbxURL,
        $tbxUserName,
        $tbxPassword;

    var disabledAttr = 'disabled',
        readonlyAttr = "readonly",
        disableClass = "disable",
        uncheckedClass = "unchecked",
        checkedClass = "checked",
        importStatusClosed = 'importStatusClosed',
        importStatus = 'importStatus';

    var loadingBanner = LoadingBanner;

    var init = function() {

        $chooseProjects = $("#chooseProjects");
        $importClosed = $("#importClosed");
        $sendInvitations = $("#sendInvitations");
        $agreement = $("#agreement");
        $importAsCollaborators = $("#importAsCollaborators");
        $startImportButton = $("#startImportButton");
        $importTools = $("#importTools");
        $popupPanelBodyError = $("#popupPanelBodyError");
        $popupImportErrorContainer = $("#popupImportErrorContainer");
        $popupImportErrorContainerHeader = $popupImportErrorContainer.find(".popup-header");
        $popupImportErrorContainerMessage = $popupImportErrorContainer.find(".error-message");
        $importPeopleStatus = $("#importPeopleStatus");
        $importProjectsStatus = $("#importProjectsStatus");
        $importFilesStatus = $("#importFilesStatus");
        $importProgress = $("#importProgress");
        $basecampProjectsContainer = $(".basecamp-projects-container");
        $tbxURL = jq("#tbxURL");
        $tbxUserName = jq("#tbxUserName");
        $tbxPassword = jq("#tbxPassword");

        $chooseProjects.removeAttr(checkedClass);
        $importClosed.removeAttr(checkedClass);
        $sendInvitations.removeAttr(checkedClass);
        $agreement.removeAttr(checkedClass);

        if ($importAsCollaborators.attr(disabledAttr) != disabledAttr) {
            $importAsCollaborators.removeAttr(checkedClass);
        } else {
            quotaEndFlag = true;
        }
        $tbxURL.focus();

        checkImportStatus(true);

        $startImportButton.click(function () {
            if (jq(this).hasClass(disableClass)) return;

            if (!validateData()) return;

            if ($importAsCollaborators.is(":checked"))
                beforeStartImport();
            else
                Teamlab.checkPrjImportQuota({}, getDataForImport(), { success: onCheckQuota, error: showOverLimitPopup });
        });

        $agreement.click(function () {
            changeAgreementCheckBox(this);
        });

        $importTools.on('click', ".view-import", function () {
            viewImportInfoPanel(jq("#import_info_popup"));
        });

        $basecampProjectsContainer.on("click", "input", function (event) {
            var row = jq(this).parents("li");
            onProjectClick(row);
            event.stopPropagation();
        });

        $basecampProjectsContainer.on("click", "label", function (event) {
            event.stopPropagation();
        });

        $basecampProjectsContainer.on("click", "li", function () {
            var input = jq(this).children("input");
            if (input.is(":checked")) {
                input.removeAttr(checkedClass);
            } else {
                input.attr(checkedClass, checkedClass);
            }
            onProjectClick(this);
        });
        jq("#checkArchivedProj").change(function() {
            var archivedProjCont = jq("#archivedProjects");
            if (!jq(this).is(":checked")) {
                archivedProjCont.find("input").removeAttr(checkedClass);
                archivedProjCont.find("li").addClass(uncheckedClass);
            } else {
                archivedProjCont.find("input").attr(checkedClass, checkedClass);
                archivedProjCont.find("li").removeClass(uncheckedClass);
            }
        });
        jq("#checkActiveProj").change(function() {
            var activeProjCont = jq("#activeProjects");
            if (!jq(this).is(":checked")) {
                activeProjCont.find("input").removeAttr(checkedClass);
                activeProjCont.find("li").addClass(uncheckedClass);
            } else {
                activeProjCont.find("input").attr(checkedClass, checkedClass);
                activeProjCont.find("li").removeClass(uncheckedClass);
            }
        });
        jq("#importCheckedProjects").click(function() {
            if (jq(this).hasClass(disableClass)) return;

            var projects = $basecampProjectsContainer.find("li input");
            importCheckedProjects(projects);
        });
        jq("#continueImport").click(function() {
            beforeStartImport();
            jq.unblockUI();
        });
    };

    function beforeStartImport() {
        if ($chooseProjects.is(":checked")) {
            getBasecampProjects();
        } else {
            startImport();
        }
    };

    function onCheckQuota(params, response) {
        if (response < 0) {
            jq("#userLimit").text(-response);
            viewImportInfoPanel(jq("#popupUsersQuotaEnds"));
            return;
        }
        beforeStartImport();
    };

    function showOverLimitPopup(params, response) {
        if (response[0] != "empty response") {
            showErrorPopup(response);
            return;
        }

        var limitHeaderContainer = jq(".tariff-limitexceed-users .header-base-medium");
        limitHeaderContainer.text(limitHeaderContainer.attr("data-zero-users-text"));

        var limitReasonContainer = jq("#limitReasonText");
        limitReasonContainer.text(limitReasonContainer.attr("data-zero-users-text"));

        viewImportInfoPanel(jq("#popupUsersQuotaEnds"));
    }

    function onProjectClick(row) {
        if (jq(row).hasClass(uncheckedClass)) {
            jq(row).removeClass(uncheckedClass);
        } else {
            jq(row).addClass(uncheckedClass);
        }
        var importButton = jq("#importCheckedProjects");
        var checkedFlag = false;
        var inputs = $basecampProjectsContainer.find("input");
        for (var i = 0; i < inputs.length; i++) {
            if (!checkedFlag) {
                checkedFlag = jq(inputs[i]).is(":checked");
            }
        };

        if (checkedFlag) {
            importButton.removeClass(disableClass);
        } else {
            importButton.addClass(disableClass);
        }
    };

    function importCheckedProjects(projects) {
        var projIds = [];
        for (var i = 0; i < projects.length; i++) {
            if (jq(projects[i]).is(":checked")) {
                projIds.push(parseInt(jq(projects[i]).attr("id")));
            }
        }
        startImport(projIds);
    };

    function getBasecampProjects() {
        var data = getDataForImport();
        if (!data) return;
        loadingBanner.displayLoading();
        Teamlab.getPrjImportProjects({ getProjects: true, isInit: true }, data, { success: onGetImportedProjects, error: onGetImportStatus });
    };

    function onGetImportedProjects(params, data) {
        var activeProjCont = jq("#activeProjects");
        var archivedProjCont = jq("#archivedProjects");
        var template = "<li><input type='checkbox' id='${id}' checked='checked'/><label for='${id}' title='${title}'>${title}</label></li>";

        var activeProjs = [];
        var archevedProjs = [];

        for (var i = 0; i < data.length; i++) {
            if (data[i].status == 0) {
                activeProjs.push(data[i]);
            } else {
                archevedProjs.push(data[i]);
            }
        }

        activeProjCont.empty();
        archivedProjCont.empty();
        jq.tmpl(template, activeProjs).appendTo(activeProjCont);
        jq.tmpl(template, archevedProjs).appendTo(archivedProjCont);

        loadingBanner.hideLoading();
        jq("#checkArchivedProj").removeAttr(disabledAttr);
        jq("#checkActiveProj").attr(checkedClass, checkedClass);
        archivedProjCont.find("input").removeAttr(checkedClass);
        archivedProjCont.find("li").addClass(uncheckedClass);
        viewImportInfoPanel(jq("#chooseProjectsPopup"));
    };

    function validateData() {
        var $urlTbContainer = jq("#companyUrl"), $emailTbContainer = jq("#companyEmail"), $passwordTbContainer = jq("#companyPassword");
        var requiredFieldErrorClass = "requiredFieldError";
        var requiredErrorTextClass = ".requiredErrorText";

        if ($tbxURL.val().trim() == "") {
            $urlTbContainer.addClass(requiredFieldErrorClass);
            $urlTbContainer.find(requiredErrorTextClass).text(resources.EmptyURL);
            return false;
        } else {
            $urlTbContainer.removeClass(requiredFieldErrorClass);
        }
        if ($tbxUserName.val().trim() == "") {
            $emailTbContainer.addClass(requiredFieldErrorClass);
            $emailTbContainer.find(requiredErrorTextClass).text(resources.EmptyEmail);
            return false;
        } else {
            $emailTbContainer.removeClass(requiredFieldErrorClass);
        }
        if ($tbxPassword.val().trim() == "") {
            $passwordTbContainer.addClass(requiredFieldErrorClass);
            return false;
        } else {
            $passwordTbContainer.removeClass(requiredFieldErrorClass);
        }

        var regExpForCompanyUrl = /^(https:\/\/basecamp.com)\/([0-9]{6,8})(\/?)$/;
        var regExpForEmail = /^([a-z0-9_\.-]+)@([a-z0-9_\.-]+)\.([a-z\.]{2,6})$/;

        // company url
        var url = $tbxURL.val().trim();
        if (!regExpForCompanyUrl.test(url)) {
            $urlTbContainer.addClass(requiredFieldErrorClass);
            $urlTbContainer.find(requiredErrorTextClass).text(resources.InvalidCompaniUrl);
            return false;
        }
        // company url
        var email = $tbxUserName.val().trim();
        if (!regExpForEmail.test(email)) {
            $emailTbContainer.addClass(requiredFieldErrorClass);
            $emailTbContainer.find(requiredErrorTextClass).text(resources.InvalidEmail);
            return false;
        }
        return true;
    };

    function getDataForImport() {
        if (validateData()) {
            $importPeopleStatus.removeClass(importStatusClosed);
            $importProjectsStatus.removeClass(importStatusClosed);
            $importFilesStatus.removeClass(importStatusClosed);

            return {
                url: $tbxURL.val(),
                userName: $tbxUserName.val(),
                password: $tbxPassword.val(),
                importClosed: jq("#importClosed").is(':checked'),
                importUsersAsCollaborators: $importAsCollaborators.is(":checked"),
                disableNotifications: !$sendInvitations.is(':checked')
            }
        }
    
        return false;
    };

    function startImport(projects) {
        var data = getDataForImport();
        if (!data) return;
        if (projects) data.projects = projects;
        Teamlab.addPrjImport({}, data, { success: onStartImport });
    };

    function showErrorPopup(response) {
        var errorText = "";
        if (!response.error) {
            errorText = response[0];
        } else {
            errorText = response.error.Message;
        }
        if (errorText.indexOf("404") < 0) {
            errorText = resources.ImportFailed + ":" + errorText;
            $popupImportErrorContainerHeader.text(resources.ImportFailed);
            $popupImportErrorContainerMessage.text(errorText);
        }
        viewImportInfoPanel($popupImportErrorContainer);
    };

    function onStartImport(params, status) {
        if (status.error != null) {
            showErrorPopup(status);
            return;
        }
        else {
            lockImportTools();
            viewImportInfoPanel(jq("#import_info_popup"));
            var spanStatusAwaiting = "<span class='gray-text'>" + resources.StatusAwaiting + " </span>";
            $importPeopleStatus.html(spanStatusAwaiting);
            $importProjectsStatus.html(spanStatusAwaiting);
            $importFilesStatus.html(spanStatusAwaiting);
            $importProgress.html('0');
            $popupPanelBodyError.hide();
            setTimeout(checkImportStatus, 5000);
        }
    };

    function onGetImportStatus(params, status) {
        if (!status.started) return;
        if (params.getProjects) {
            loadingBanner.hideLoading();
            showErrorPopup(status);
            unlockImportTools();
            return;
        }
        if (status.error != null) {
            if (!params.isInit && !jq("#import_info_popup").is(":visible")) {
                showErrorPopup(status);
                $popupPanelBodyError.hide();
            }
            unlockImportTools();
            buildErrorList(status);
        }
        else if (status != null && status.completed && status.error == null) {
            $importPeopleStatus.html('').removeClass(importStatus).addClass(importStatusClosed);
            $importProjectsStatus.html('').removeClass(importStatus).addClass(importStatusClosed);
            $importFilesStatus.html('').removeClass(importStatus).addClass(importStatusClosed);;
            $importProgress.html('3');
            $popupPanelBodyError.hide();
            buildErrorList(status);
            unlockImportTools();
            if (!jq("#import_info_popup").is(":visible")) {
                $popupImportErrorContainerHeader.text(resources.PopupPanelHeader);
                $popupImportErrorContainerMessage.text(resources.ImportCompleted);
                viewImportInfoPanel($popupImportErrorContainer);
            }
        }
        else if (status != null && status.error != null) {
            buildErrorList(status);
            unlockImportTools();
        }
        else {
            lockImportTools();
            setTimeout(checkImportStatus, 5000);
            buildErrorList(status);
            if (status != null) {
                if (status.userProgress > 0) {
                    $importPeopleStatus.html(Math.round(status.userProgress) + '%');
                    if (status.userProgress == 100) {
                        $importPeopleStatus.html('').removeClass(importStatus).addClass(importStatusClosed);
                        $importProgress.html('1');
                    }
                }
                if (status.projectProgress > 0) {
                    $importProjectsStatus.html(Math.round(status.projectProgress) + '%');
                    if (status.projectProgress == 100) {
                        $importProjectsStatus.html('').removeClass(importStatus).addClass(importStatusClosed);
                        $importProgress.html('2');
                    }
                }
                if (status.fileProgress > 0) {
                    $importFilesStatus.html(Math.round(status.fileProgress) + '%');
                    if (status.fileProgress == 100) {
                        $importFilesStatus.html('').removeClass(importStatus).addClass(importStatusClosed);;
                        $importProgress.html('3');
                    }
                }
            }
        }
    };

    function checkImportStatus(isInit) {
        Teamlab.getPrjImport({ isInit: isInit }, { success: onGetImportStatus });
    };

    function buildErrorList(res) {
        if (!res || !res.log || !res.log.length)
            return;

        var statusStr = "";
        if (res.error != null && res.error.Message != "") {
            statusStr = statusStr + '<div class="errorBox fatal">' + res.error.Message + '</div>';
        }
        for (var i = res.log.length - 1; i >= 0; i--) {

            var messageClass = "ok";
            if (res.log[i].Type == "warn" || res.log[i].type == "error")
                messageClass = "warn";

            statusStr = statusStr + '<div class="' + messageClass + 'Box">' + res.log[i].message + '</div>';
        }

        if (statusStr != "") {
            $popupPanelBodyError.html(statusStr);
            $popupPanelBodyError.show();
        }
        else {
            $popupPanelBodyError.hide();
        }
    };

    function lockImportTools() {
        $tbxURL.attr(readonlyAttr, readonlyAttr).addClass(disabledAttr);
        $tbxUserName.attr(readonlyAttr, readonlyAttr).addClass(disabledAttr);
        $tbxPassword.attr(readonlyAttr, readonlyAttr).addClass(disabledAttr);

        $importClosed.attr(disabledAttr, disabledAttr);
        $sendInvitations.attr(disabledAttr, disabledAttr);
        $chooseProjects.attr(disabledAttr, disabledAttr);
        $importAsCollaborators.attr(disabledAttr, disabledAttr);
        loadingBanner.showLoaderBtn($importTools);

        if (! $importTools.find(".loader-container a").length) {
            $importTools.find(".loader-container").append(jq("#viewDetailsImport").html());
        }

        jq('#importCompletedContent').hide();
    };

    function unlockImportTools() {
        $tbxURL.removeAttr(readonlyAttr).removeClass(disabledAttr);
        $tbxUserName.removeAttr(readonlyAttr).removeClass(disabledAttr);
        $tbxPassword.removeAttr(readonlyAttr).removeClass(disabledAttr);

        $importClosed.removeAttr(disabledAttr);
        $sendInvitations.removeAttr(disabledAttr);
        $chooseProjects.removeAttr(disabledAttr);
        if (!quotaEndFlag) {
            $importAsCollaborators.removeAttr(disabledAttr);
        }

        loadingBanner.hideLoaderBtn($importTools);

        jq('#importCompletedContent').show();
    };

    function viewImportInfoPanel(popup) {
        jq("#import_info_attention_popup").hide();
        jq("#importCheckedProjects").removeClass(disableClass);
        StudioBlockUIManager.blockUI(popup, 480);
    };

    function changeAgreementCheckBox(obj) {
        if (jq(obj).is(':checked')) {
            $startImportButton.removeClass(disableClass);
        }
        else {
            $startImportButton.addClass(disableClass);
        }
    };

    return {
        init: init
    };
})(jQuery);