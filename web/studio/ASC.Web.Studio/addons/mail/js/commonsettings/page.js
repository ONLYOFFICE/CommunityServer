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


window.commonSettingsPage = (function($) {
    var isInit = false,
        page,
        header,
        cbxEnableConversations,
        cbxDisplayAllImages,
        cbxCacheUnreadMessages,
        cbxGoNextAfterMove,
        cbxReplaceMessageBody;

    function init() {
        if (isInit === false) {
            isInit = true;
            page = $('#mailCommonSettings');
            header = $('#pageActionContainer');
            cbxEnableConversations = jq("#cbxEnableConversations"),
            cbxDisplayAllImages = jq("#cbxDisplayAllImages"),
            cbxCacheUnreadMessages = jq("#cbxCacheUnreadMessages"),
            cbxGoNextAfterMove = jq("#cbxGoNextAfterMove"),
            cbxReplaceMessageBody = jq("#cbxReplaceMessageBody");

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
                            window.userFoldersPanel.refresh();
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

            cbxReplaceMessageBody.on("change",
                function () {
                    var self = jq(this);

                    var enabled = self.prop("checked");

                    ASC.Mail.Presets.CommonSettings.ReplaceMessageBody = enabled;
                    serviceManager.setEnableReplaceMessageBody(enabled, { enabled: enabled },
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
        header.html($.tmpl('commonSettingsHeaderTmpl'));
    }

    function hide() {
        page.hide();
        header.empty();
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

    function replaceMessageBodyEnabled() {
        return ASC.Mail.Presets.CommonSettings.ReplaceMessageBody;
    }

    return {
        init: init,
        show: show,
        hide: hide,

        isConversationsEnabled: isConversationsEnabled,
        AlwaysDisplayImages: alwaysDisplayImages,
        AutocacheMessagesEnabled: autocacheMessagesEnabled,
        GoNextAfterMoveEnabled: goNextAfterMoveEnabled,
        ReplaceMessageBodyEnabled: replaceMessageBodyEnabled
    };
})(jQuery);