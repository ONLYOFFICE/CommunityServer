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


if (typeof ASC === "undefined") {
    ASC = {};
}
if (typeof ASC.Mail === "undefined") {
    ASC.Mail = (function () {
        return {};
    })();
}
if (typeof ASC.Mail.Filter === "undefined") {
    ASC.Mail.Filter = function(id, name, position, enabled, conditions, actions, options) {
        this.id = id || 0;
        this.name = name || "";
        this.position = position || 0;
        this.enabled = enabled || true;

        this.conditions = conditions || [];
        this.actions = actions || [];

        this.options = options || new ASC.Mail.Filter.Options();

        this.ToData = function() {
            var json = JSON.stringify(this);
            var data = JSON.parse(json);
            return data;
        };
    };

    ASC.Mail.Filter.Constants = (function() {
        var condition = {
                KeyType:
                {
                    None: -1,
                    From: 0,
                    ToOrCc: 1,
                    To: 2,
                    Cc: 3,
                    Subject: 4
                },
                OperationType:
                {
                    None: -1,
                    Matches: 0,
                    NotMatches: 1,
                    Contains: 2,
                    NotContains: 3
                },
                MatchMultiType:
                {
                    None: 0,
                    MatchAll: 1,
                    MatchAtLeastOne: 2
                }
            },
            actionType = {
                None: -1,
                DeleteForever: 0,
                MoveTo: 1,
                MarkTag: 2,
                MarkAsImportant: 3,
                MarkAsRead: 4
            },
            applyTo = {
                AttachmentsType:
                {
                    WithAndWithoutAttachments: 0,
                    WithAttachments: 1,
                    WithoutAttachments: 2
                },
                MessagesType:
                {
                    AllExceptSpam: 0,
                    AllIncludeSpam: 1,
                    SpamOnly: 2
                }
            };

        return {
            Condition: condition,
            ActionType: actionType,
            ApplyTo: applyTo
        };
    })();

    ASC.Mail.Filter.Options = function(folders) {
        this.matchMultiConditions = ASC.Mail.Filter.Constants.Condition.MatchMultiType.None;
        this.applyTo = {
            folders: folders || [],
            mailboxes: [],
            withAttachments: ASC.Mail.Filter.Constants.ApplyTo.AttachmentsType.WithAndWithoutAttachments
        };
        this.ignoreOther = false;
    };

    ASC.Mail.Filter.Action = function (action, data) {
        this.action = action === null || action === undefined ? ASC.Mail.Filter.Constants.ActionType.None : action;
        this.data = data || null;
    };

    ASC.Mail.Filter.Condition = function (key, operation, value) {
        this.id = "condition" + TMMail.getRandomId();
        this.key = key;
        this.operation = operation;
        this.value = value || null;
    };
}