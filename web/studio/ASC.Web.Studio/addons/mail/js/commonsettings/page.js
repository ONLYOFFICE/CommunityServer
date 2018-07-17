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


window.commonSettingsPage = (function($) {
    var isInit = false,
        page,
        cbxEnableConversations,
        cbxDisplayAllImages,
        cbxCacheUnreadMessages,
        cbxGoNextAfterMove;

    function init() {
        if (isInit === false) {
            isInit = true;
            page = $('#mailCommonSettings');
            cbxEnableConversations = jq("#cbxEnableConversations"),
            cbxDisplayAllImages = jq("#cbxDisplayAllImages"),
            cbxCacheUnreadMessages = jq("#cbxCacheUnreadMessages"),
            cbxGoNextAfterMove = jq("#cbxGoNextAfterMove");

            cbxEnableConversations.on("change",
                function () {
                    var self = jq(this);

                    var enabled = self.prop("checked");

                    ASC.Mail.Presets.CommonSettings.EnableConversations = enabled;
                    serviceManager.setConversationEnabledFlag(enabled, { enabled: enabled },
                    {
                        success: function() {
                            mailBox.markFolderAsChanged(TMMail.sysfolders.inbox.id);
                            mailBox.markFolderAsChanged(TMMail.sysfolders.sent.id);
                            mailBox.markFolderAsChanged(TMMail.sysfolders.trash.id);
                            mailBox.markFolderAsChanged(TMMail.sysfolders.spam.id);
                            mailCache.clear();
                            serviceManager.updateFolders({}, {}, ASC.Resources.Master.Resource.LoadingProcessing);
                        },
                        error: function (e, error) {
                            window.toastr.error(window.MailApiErrorsResource.ErrorInternalServer);
                            console.error(e, error);
                            self.prop("checked", !enabled);
                        }
                    });
                    return false;
                });

            cbxDisplayAllImages.on("change",
                function () {
                    var self = jq(this);

                    var enabled = self.prop("checked");

                    ASC.Mail.Presets.CommonSettings.AlwaysDisplayImages = enabled;
                    serviceManager.setAlwaysDisplayImagesFlag(enabled, { enabled: enabled },
                    {
                        success: function () {},
                        error: function (e, error) {
                            window.toastr.error(window.MailApiErrorsResource.ErrorInternalServer);
                            console.error(e, error);
                            self.prop("checked", !enabled);
                        }
                    });
                    return false;
                });

            cbxCacheUnreadMessages.on("change",
                function () {
                    var self = jq(this);

                    var enabled = self.prop("checked");

                    ASC.Mail.Presets.CommonSettings.CacheUnreadMessages = enabled;
                    serviceManager.setCacheUnreadMessagesFlag(enabled, { enabled: enabled },
                    {
                        success: function () {},
                        error: function (e, error) {
                            window.toastr.error(window.MailApiErrorsResource.ErrorInternalServer);
                            console.error(e, error);
                            self.prop("checked", !enabled);
                        }
                    });
                    return false;
                });

            cbxGoNextAfterMove.on("change",
                function () {
                    var self = jq(this);

                    var enabled = self.prop("checked");

                    ASC.Mail.Presets.CommonSettings.EnableGoNextAfterMove = enabled;
                    serviceManager.setEnableGoNextAfterMove(enabled, { enabled: enabled },
                    {
                        success: function () {},
                        error: function (e, error) {
                            window.toastr.error(window.MailApiErrorsResource.ErrorInternalServer);
                            console.error(e, error);
                            self.prop("checked", !enabled);
                        }
                    });
                    return false;
                });
        }
    }

    function show() {
        page.show();
    }

    function hide() {
        page.hide();
    }

    function isConversationsEnabled() {
        return ASC.Mail.Presets.CommonSettings.EnableConversations;
    }

    function alwaysDisplayImages() {
        return ASC.Mail.Presets.CommonSettings.AlwaysDisplayImages;
    }

    function autocacheMessagesEnabled() {
        return ASC.Mail.Presets.CommonSettings.CacheUnreadMessages;
    }

    function goNextAfterMoveEnabled() {
        return ASC.Mail.Presets.CommonSettings.EnableGoNextAfterMove;
    }

    return {
        init: init,
        show: show,
        hide: hide,

        isConversationsEnabled: isConversationsEnabled,
        AlwaysDisplayImages: alwaysDisplayImages,
        AutocacheMessagesEnabled: autocacheMessagesEnabled,
        GoNextAfterMoveEnabled: goNextAfterMoveEnabled
    };
})(jQuery);