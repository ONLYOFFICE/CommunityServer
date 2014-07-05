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
if (typeof ASC === "undefined") {
    ASC = {};
}

if (typeof ASC.CRM === "undefined") {
    ASC.CRM = (function () { return {}; })();
}

ASC.CRM.SmtpSender = (function () {
    var activateFileUploader = function () {
        ASC.CRM.FileUploader.activateUploader();
        ASC.CRM.FileUploader.fileIDs.clear();
    };

    var initFileUploaderCallback = function () {
        ASC.CRM.FileUploader.OnAllUploadCompleteCallback_function = function () {
            jq('#sendEmailPanel').hide();
            jq("#sendProcessPanel").show();

            var contacts = ASC.CRM.SmtpSender.selectedItems.map(function (item) { return item.id; }),
                watermark = jq.format("<div style='color:#787878;font-size:12px;margin-top:10px'>{0}</div>",
                                        jq.format(ASC.CRM.Resources.CRMJSResource.TeamlabWatermark,
                                        jq.format("<a style='color:#787878;font-size:12px;' href='http://www.onlyoffice.com'>{0}</a>", "ONLYOFFICE.com"))
            );

            var subj = jq("#sendEmailPanel #tbxEmailSubject").val().trim();
            if (subj == "") {
                 subj = ASC.CRM.Resources.CRMJSResource.NoSubject;
            }

            var storeInHistory = jq("#storeInHistory").is(":checked");

            AjaxPro.SmtpSender.SendEmail(ASC.CRM.FileUploader.fileIDs,
                    contacts,
                    subj,
                    ASC.CRM.SmtpSender.editor.getData() + watermark,
                    storeInHistory,
                    function (res) {
                        if (res.error != null) {
                            alert(res.error.Message);
                            return;
                        }
                        ASC.CRM.SmtpSender.checkSendStatus(true);
                    });
        };
    };
    
    var initCkeditor = function() {
        var callbackFunction = function () {
            ASC.CRM.SmtpSender.editor = window.CKEDITOR.instances.ckEditor;
            ASC.CRM.SmtpSender.showSendEmailPanel();
        };
        var configData = { toolbar: 'CrmEmail' };
        jq('#ckEditor').ckeditor(callbackFunction, configData);
    };

    var checkValidation = function () {
        if (ASC.CRM.SmtpSender.selectedItems.length > ASC.CRM.SmtpSender.emailQuotas) {
            console.log(jq.format(ASC.CRM.Resources.CRMJSResource.ErrorEmailRecipientsCount, ASC.CRM.SmtpSender.emailQuotas));
            return false;
        }

        if (!ASC.CRM.SmtpSender.selectedItems.length) {
            console.log("Empty recipient list.");
            return false;
        }

        if (!ASC.CRM.ListContactView.checkSMTPSettings()) {
            console.log("Eempty field in smtp settings.");
            return false;
        }

        return true;
    };

    return {
        selectedItems: [],
        emailQuotas: 0,
        editor: null,

        init: function (emailQuotas) {
            ASC.CRM.SmtpSender.selectedItems = ASC.CRM.SmtpSender.getItems();
            ASC.CRM.SmtpSender.emailQuotas = emailQuotas;

            if (!checkValidation()) {
                window.location.href = "default.aspx";
            } else {
                if (!jq.browser.mobile) {
                    initFileUploaderCallback();
                    initCkeditor();
                    activateFileUploader();
                } else {
                    ASC.CRM.SmtpSender.showSendEmailPanel();
                }
            }
        },

        showSendEmailPanel: function () {
            AjaxPro.SmtpSender.GetStatus(function (res) {
                if (res.error != null) {
                    alert(res.error.Message);
                    return;
                }
                if (res.value == null || res.value.IsCompleted) {
                    jq("#sendEmailPanel #emailFromLabel").text(jq.format("{0} ({1})", ASC.CRM.Data.smtpSettings.SenderDisplayName, ASC.CRM.Data.smtpSettings.SenderEmailAddress));
                    jq("#sendEmailPanel #previewEmailFromLabel").text(jq.format("{0} ({1})", ASC.CRM.Data.smtpSettings.SenderDisplayName, ASC.CRM.Data.smtpSettings.SenderEmailAddress));
                    jq("#tbxEmailSubject").val("");

                    if (!jq.browser.mobile) {
                        ASC.CRM.SmtpSender.editor.setData("");
                    } else {
                        jq("#ckEditor").val("");
                    }

                    jq("#storeInHistory").prop("checked", false);

                    jq("#sendButton").text(ASC.CRM.Resources.CRMJSResource.NextPreview).unbind("click").bind("click", function () {
                        ASC.CRM.SmtpSender.showSendEmailPanelPreview();
                    });

                    jq("#backButton a.button.blue.middle").unbind("click").bind("click", function () {
                        ASC.CRM.SmtpSender.showSendEmailPanelCreate();
                    });

                    var count = ASC.CRM.SmtpSender.selectedItems.length,
                        countString =
                                count == 1 ?
                                ASC.CRM.Resources.CRMJSResource.AddressGenitiveSingular :
                                ASC.CRM.Resources.CRMJSResource.AddressGenitivePlural;

                    jq("#emailAddresses").html([count, countString].join(" "));
                    jq("#previewEmailAddresses").html([count, countString].join(" "));

                    jq("#sendEmailPanel").show();
                    jq("#sendEmailPanel #createContent").show();
                    jq("#sendEmailPanel #previewContent").hide();
                    
                    jq("#sendProcessPanel").hide();
                } else {
                    ASC.CRM.SmtpSender.checkSendStatus(true);
                }
            });
        },

        showSendEmailPanelCreate: function () {
            jq("#createContent").show();
            jq("#previewContent").hide();
            jq("#sendProcessPanel").hide();
            jq("#backButton").hide();
            jq("#sendButton").text(ASC.CRM.Resources.CRMJSResource.NextPreview).unbind("click").bind("click", function () {
                ASC.CRM.SmtpSender.showSendEmailPanelPreview();
            });
        },

        showSendEmailPanelPreview: function () {
            AjaxPro.onLoading = function (b) {
                if (b) {
                    LoadingBanner.showLoaderBtn("#sendEmailPanel");
                } else {
                    LoadingBanner.hideLoaderBtn("#sendEmailPanel");
                }
            };

            var mess = "";

            if (!jq.browser.mobile) {
                mess = ASC.CRM.SmtpSender.editor.getData();
            } else {
                mess = jq("#ckEditor").val().trim();
            }
            
            if (mess == "") {
                AddRequiredErrorText(jq("#requiredMessageBody"), ASC.CRM.Resources.CRMJSResource.EmptyLetterBodyContent);
                ShowRequiredError(jq("#requiredMessageBody"), true);
                return false;
            } else {
                RemoveRequiredErrorClass(jq("#requiredMessageBody"));
            }

            if (jq.browser.mobile) {
                mess = mess.replace(/\n/g, "<br />");
            }

            var subj = jq("#sendEmailPanel #tbxEmailSubject").val().trim();
            if (subj == "") {
                subj = ASC.CRM.Resources.CRMJSResource.NoSubject;
            }

            AjaxPro.SmtpSender.GetMessagePreview(mess, ASC.CRM.SmtpSender.selectedItems[0].id, function (res) {
                if (res.error != null) {
                    alert(res.error.Message);
                    LoadingBanner.hideLoaderBtn("#sendEmailPanel");
                    return false;
                }

                jq("#previewSubject").text(subj);

                var watermark = jq.format("<div style='color:#787878;font-size:12px;margin-top:10px'>{0}</div>",
                                            jq.format(ASC.CRM.Resources.CRMJSResource.TeamlabWatermark,
                                            jq.format("<a style='color:#787878;font-size:12px;' href='http://www.onlyoffice.com'>{0}</a>", "ONLYOFFICE.com"))
                );

                jq("#previewMessage").html(res.value + watermark);

                if (!jq.browser.mobile) {
                    var attachments = ASC.CRM.FileUploader.fileNames();
                    jq("#previewAttachments span").html("");
                    if (attachments.length > 0) {
                        attachments.each(function (index) {
                            jq("#previewAttachments span").append(this);
                            if (index != attachments.length - 1)
                                jq("#previewAttachments span").append(", ");
                        });
                        jq("#previewAttachments").show();
                    } else {
                        jq("#previewAttachments").hide();
                    }
                }

                jq("#sendProcessPanel").hide();
                jq("#createContent").hide();
                jq("#previewContent").show();
                jq("#backButton").show();
                jq("#sendButton").text(ASC.CRM.Resources.CRMJSResource.Send).unbind("click").bind("click", function () {
                    ASC.CRM.SmtpSender.sendEmail();
                });
                jq("#sendButton").trackEvent(ga_Categories.contacts, ga_Actions.actionClick, 'mass_email');
            });
        },

        sendEmail: function () {
            AjaxPro.onLoading = function (b) { };

            if (!jq.browser.mobile && ASC.CRM.FileUploader.getUploadFileCount() > 0) {
                ASC.CRM.FileUploader.start();
            } else {
                var contacts = ASC.CRM.SmtpSender.selectedItems.map(function (item) { return item.id; }),
                    watermark = jq.format("<div style='color:#787878;font-size:12px;margin-top:10px'>{0}</div>",
                                            jq.format(ASC.CRM.Resources.CRMJSResource.TeamlabWatermark,
                                            jq.format("<a style='color:#787878;font-size:12px;' href='http://www.onlyoffice.com'>{0}</a>", "ONLYOFFICE.com"))
                );

                var subj = jq("#sendEmailPanel #tbxEmailSubject").val().trim();
                if (subj == "") {
                    subj = ASC.CRM.Resources.CRMJSResource.NoSubject;
                }

                var storeInHistory = jq("#storeInHistory").is(":checked");
                var letterBody = "";

                if (!jq.browser.mobile) {
                    letterBody = ASC.CRM.SmtpSender.editor.getData();
                } else {
                    letterBody = jq("#ckEditor").val().trim();
                }

                AjaxPro.SmtpSender.SendEmail(new Array(),
                        contacts,
                        subj,
                        letterBody + watermark,
                        storeInHistory,
                        function (res) {
                            if (res.error != null) {
                                alert(res.error.Message);
                                return;
                            }
                            ASC.CRM.SmtpSender.checkSendStatus(true);
                        });
            }
        },

        checkSendStatus: function (isFirstVisit) {
            jq("#sendEmailPanel").hide();
            jq("#sendProcessPanel").show();
            jq("#sendProcessPanel #abortButton").show();
            jq("#sendProcessPanel #okButton").hide();

            if (isFirstVisit) {
                jq("#sendProcessProgress .progress").css("width", "0%");
                jq("#sendProcessProgress .percent").text("0%");
                jq("#emailsTotalCount").html("");
                jq("#emailsAlreadySentCount").html("");
                jq("#emailsEstimatedTime").html("");
                jq("#emailsErrorsCount").html("");
            }

            AjaxPro.SmtpSender.GetStatus(function (res) {
                if (res.error != null) {
                    alert(res.error.Message);
                    return;
                }

                if (res.value == null) {
                    jq("#sendProcessProgress .progress").css("width", "100%");
                    jq("#sendProcessProgress .percent").text("100%");
                    jq("#sendProcessPanel #abortButton").hide();
                    jq("#sendProcessPanel #okButton").show();
                    return;
                } else {
                    ASC.CRM.SmtpSender.displayProgress(res.value);
                }

                if (res.value.Error != null && res.value.Error != "") {
                    ASC.CRM.SmtpSender.buildErrorList(res);
                    jq("#sendProcessPanel #abortButton").hide();
                    jq("#sendProcessPanel #okButton").show();
                } else {
                    if (res.value.IsCompleted) {
                        jq("#sendProcessPanel #abortButton").hide();
                        jq("#sendProcessPanel #okButton").show();
                    } else {
                        setTimeout("ASC.CRM.SmtpSender.checkSendStatus(false)", 3000);
                    }
                }
            });
        },

        buildErrorList: function (res) {
            var mess = "error";
            switch (typeof res.value.Error) {
                case "object":
                    mess = res.value.Error.Message + "<br/>";
                    break;
                case "string":
                    mess = res.value.Error;
                    break;
            }

            jq("#emailsErrorsCount")
            .html(jq("<div></div>").addClass("red-text").html(mess))
            .append(jq("<a></a>").attr("href", "settings.aspx?type=common").text(ASC.CRM.Resources.CRMJSResource.GoToSettings));
        },

        abortMassSend: function () {
            AjaxPro.onLoading = function (b) { };
            AjaxPro.SmtpSender.Cancel(function (res) {
                if (res.error != null) {
                    alert(res.error.Message);
                    return;
                }
                if (res.value != null) {
                    ASC.CRM.SmtpSender.displayProgress(res.value);
                }
                jq("#sendProcessPanel #abortButton").hide();
            });
        },

        displayProgress: function (progressItem) {
            jq("#sendProcessProgress .progress").css("width", progressItem.Percentage + "%");
            jq("#sendProcessProgress .percent").text(progressItem.Percentage + "%");
            jq("#emailsAlreadySentCount").html(progressItem.Status.DeliveryCount);
            jq("#emailsEstimatedTime").html(progressItem.Status.EstimatedTime);
            jq("#emailsTotalCount").html(progressItem.Status.RecipientCount);
            jq("#emailsErrorsCount").html("0");
        },

        emailInsertTag: function () {
            var isCompany = jq("#emailTagTypeSelector option:selected").val() == "company",
                tagName = isCompany ? jq('#emailCompanyTagSelector option:selected').val() : jq('#emailPersonTagSelector option:selected').val();

            if (!jq.browser.mobile) {
                ASC.CRM.SmtpSender.editor.insertHtml(tagName);
            } else {
                var caretPos = jq("#ckEditor").caret().start,
                    oldText = jq("#ckEditor").val(),
                    newText = oldText.slice(0, caretPos) + tagName + oldText.slice(caretPos);
                jq("#ckEditor").val(newText);
            }
        },

        renderTagSelector: function () {
            var isCompany = jq("#emailTagTypeSelector option:selected").val() == "company";
            if (isCompany) {
                jq("#emailPersonTagSelector").hide();
                jq("#emailCompanyTagSelector").show();
            } else {
                jq("#emailCompanyTagSelector").hide();
                jq("#emailPersonTagSelector").show();
            }
        },

        setItems: function (targets) {
            var s = JSON.stringify(targets);
            if (!localStorage.hasOwnProperty("senderTargets")) {
                localStorage.setItem("senderTargets", s);
            } else {
                localStorage.senderTargets = s;
            }
        },
        
        getItems: function () {
            var s = "[]";
            if (localStorage.hasOwnProperty("senderTargets")) {
                s = localStorage.senderTargets;
                localStorage.removeItem("senderTargets");
            }
            return JSON.parse(s);
        },
    };
})();