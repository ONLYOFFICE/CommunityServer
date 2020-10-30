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