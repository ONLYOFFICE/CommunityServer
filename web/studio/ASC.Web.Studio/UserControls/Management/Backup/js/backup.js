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

BackupManager = new function () {
    this.Init = function() {
        jq('#startBackupBtn').on("click", BackupManager.StartBackup);

        jq('#sendDeactivateInstructionsBtn').on("click", BackupManager.SendDeactivateInstructions);
        jq('#sendDeleteInstructionsBtn').on("click", BackupManager.SendDeleteInstructions);

        jq("#startRestoreBtn").on("click", BackupManager.StartRestore);

        jq(".restore-setting-file").on("change", function() {
            var filePath = jq(this).val();
            jq(".restore-setting-field").val(filePath);
        });

        jq("#restoreChooseFile").on("click", function(event) {
            event.preventDefault();
            jq(".restore-setting-file").click();
        });
    };

    this.StartBackup = function () {
        var $button = jq("#startBackupBtn");
        if ($button.hasClass("disable")) {
            return;
        }
        $button.addClass("disable");
        
        showBackupProgress(0);
        AjaxPro.Backup.StartBackup(processBackupResponse);
    };
    
    function showBackupProgress(progress) {
        var $progressContainer = jq("#progressbar_container"),
            $progressValue = jq(".asc-progress-value"),
            $progressText = jq("#backup_percent");

        $progressValue.animate({ "width": progress + "%" });
        $progressText.text(progress + "% ");

        if ($progressContainer.is(":hidden")) {
            $progressContainer.show();
        }
    }

    function processBackupResponse(response) {
        if (response.error) {
            var $error = jq('#backup_error');
            $error.text(response.error.Message);
            $error.show();
            jq('#startBackupBtn').removeClass('disable');
            return;
        }

        if (response.value && response.value.IsCompleted) {
            jq('#progressbar_container').hide();
            jq('#backup_link').html("<a href='" + response.value.Link + "' target='_blank'>" + response.value.Link + "</a>");
            jq('#backup_ready').show();
            jq('#startBackupBtn').removeClass('disable');
            return;
        }

        showBackupProgress(response.value.Progress);
        setTimeout(function() {
            AjaxPro.Backup.GetBackupStatus(processBackupResponse);
        }, 1000);
    }

    this.StartRestore = function () {
        var $button = jq("#startRestoreBtn");
        if ($button.hasClass("disable")) {
            return;
        }
        $button.addClass("disable");
        
        alert('test!!!');
    };

    this.SendDeactivateInstructions = function () {
        if (jq("#sendDeactivateInstructionsBtn").hasClass("disable")) {
            return;
        }
        
        LoadingBanner.showLoaderBtn("#accountDeactivationBlock");
        AjaxPro.Backup.SendDeactivateInstructions(function (response) {
            if (response.value) {
                var $status = jq('#deativate_sent');
                $status.html(response.value);
                $status.show();
            }
            LoadingBanner.hideLoaderBtn("#accountDeactivationBlock");
        });
    };

    this.SendDeleteInstructions = function () {
        if (jq("#sendDeleteInstructionsBtn").hasClass("disable")) {
            return;
        }

        LoadingBanner.showLoaderBtn("#accountDeletionBlock");
        AjaxPro.Backup.SendDeleteInstructions(function (response) {
            if (response.value) {
                var $status = jq('#delete_sent');
                $status.html(response.value);
                $status.show();
            }
            LoadingBanner.hideLoaderBtn("#accountDeletionBlock");
        });
    };
};

jq(function() {
    BackupManager.Init();
});
