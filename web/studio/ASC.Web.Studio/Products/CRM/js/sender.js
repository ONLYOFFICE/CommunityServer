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

            var data = {
                contactIds: contacts,
                subject: subj,
                body: ASC.CRM.SmtpSender.editor.getData() + (jq("#watermarkInfo").length ? watermark : ""),
                fileIDs: ASC.CRM.FileUploader.fileIDs,
                storeInHistory: jq("#storeInHistory").is(":checked")
            };

            Teamlab.sendSMTPMailToContacts({}, data, {
                success: function (params, response) {
                    ASC.CRM.SmtpSender.checkSendStatus(true);
                },
                error: function (params, errors) {
                    var err = errors[0];
                    if (err != null) {
                        toastr.error(err);
                    }
                }
            });
        };
    };
    
    var initCkeditor = function() {
        var callbackFunction = function () {
            ASC.CRM.SmtpSender.showSendEmailPanel();
        };
        var configData = { toolbar: 'CrmEmail' };
        ckeditorConnector.load(function () {
            ASC.CRM.SmtpSender.editor = jq('#ckEditor').ckeditor(callbackFunction, configData).editor;
        });
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
            console.log("Empty field in smtp settings.");
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
                initFileUploaderCallback();
                initCkeditor();
                activateFileUploader();
            }
        },

        showSendEmailPanel: function () {

            Teamlab.getStatusSMTPMailToContacts({}, {
                success: function (params, response) {
                    if (response == null || jQuery.isEmptyObject(response) || response.isCompleted) {
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
                },
                error: function (params, errors) {
                    var err = errors[0];
                    if (err != null) {
                        toastr.error(err);
                    }
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

            Teamlab.getPreviewSMTPMailToContacts({},
                { template: mess, contactId: ASC.CRM.SmtpSender.selectedItems[0].id },
                {
                    before: function(params){ LoadingBanner.showLoaderBtn("#sendEmailPanel"); },
                    after: function(params){ LoadingBanner.hideLoaderBtn("#sendEmailPanel"); },
                    success: function(params, response) {
                        jq("#previewSubject").text(subj);

                        var watermark = jq.format("<div style='color:#787878;font-size:12px;margin-top:10px'>{0}</div>",
                                                    jq.format(ASC.CRM.Resources.CRMJSResource.TeamlabWatermark,
                                                    jq.format("<a style='color:#787878;font-size:12px;' href='http://www.onlyoffice.com'>{0}</a>", "ONLYOFFICE.com"))
                        );

                        jq("#previewMessage").html(response + (jq("#watermarkInfo").length ? watermark : ""));

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

                        jq("#sendProcessPanel").hide();
                        jq("#createContent").hide();
                        jq("#previewContent").show();
                        jq("#backButton").show();
                        jq("#sendButton").text(ASC.CRM.Resources.CRMJSResource.Send).unbind("click").bind("click", function () {
                            ASC.CRM.SmtpSender.sendEmail();
                        });
                        jq("#sendButton").trackEvent(ga_Categories.contacts, ga_Actions.actionClick, 'mass_email');



                    },
                    error: function(params, errors) {
                        var err = errors[0];
                        if (err != null) {
                            toastr.error(err);
                            LoadingBanner.hideLoaderBtn("#sendEmailPanel");
                        }
                    }
                });

        },

        sendEmail: function () {
            AjaxPro.onLoading = function (b) { };

            if (ASC.CRM.FileUploader.getUploadFileCount() > 0) {
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

                var letterBody = "";

                if (!jq.browser.mobile) {
                    letterBody = ASC.CRM.SmtpSender.editor.getData();
                } else {
                    letterBody = jq("#ckEditor").val().trim();
                }

                var data = {
                    contactIds: contacts,
                    subject: subj,
                    body: letterBody + (jq("#watermarkInfo").length ? watermark : ""),
                    fileIDs: [],
                    storeInHistory: jq("#storeInHistory").is(":checked")
                };

                Teamlab.sendSMTPMailToContacts({}, data, {
                    success: function (params, response) {
                        ASC.CRM.SmtpSender.checkSendStatus(true);
                    },
                    error: function (params, errors) {
                        var err = errors[0];
                        if (err != null) {
                            toastr.error(err);
                        }
                    }
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

            Teamlab.getStatusSMTPMailToContacts({}, {
                success: function (params, response) {
                    if (response == null || jQuery.isEmptyObject(response)) {
                        jq("#sendProcessProgress .progress").css("width", "100%");
                        jq("#sendProcessProgress .percent").text("100%");
                        jq("#sendProcessPanel #abortButton").hide();
                        jq("#sendProcessPanel #okButton").show();
                        return;
                    }

                    ASC.CRM.SmtpSender.displayProgress(response);

                    if (response.error != null && response.error != "") {
                        ASC.CRM.SmtpSender.buildErrorList(response);
                        jq("#sendProcessPanel #abortButton").hide();
                        jq("#sendProcessPanel #okButton").show();
                    } else {
                        if (response.isCompleted) {
                            jq("#sendProcessPanel #abortButton").hide();
                            jq("#sendProcessPanel #okButton").show();
                        } else {
                            setTimeout("ASC.CRM.SmtpSender.checkSendStatus(false)", 3000);
                        }
                    }
                },
                error: function (params, errors) {
                    var err = errors[0];
                    if (err != null) {
                        toastr.error(err);
                    }
                }
            })
        },

        buildErrorList: function (res) {
            var mess = "error";
            switch (typeof res.error) {
                case "object":
                    mess = res.error.Message + "<br/>";
                    break;
                case "string":
                    mess = res.error;
                    break;
            }

            jq("#emailsErrorsCount")
            .html(jq("<div></div>").addClass("red-text").html(mess))
            .append(jq("<a></a>").attr("href", "settings.aspx?type=common").text(ASC.CRM.Resources.CRMJSResource.GoToSettings));
        },

        abortMassSend: function () {
            Teamlab.cancelSMTPMailToContacts({}, {
                success: function (params, response) {
                    if (response != null && !jQuery.isEmptyObject(response)) {
                        ASC.CRM.SmtpSender.displayProgress(response);
                    }
                    jq("#sendProcessPanel #abortButton").hide();
                },
                error: function (params, errors) {
                    var err = errors[0];
                    if (err != null) {
                        toastr.error(err);
                    }
                }
            })
        },

        displayProgress: function (progressItem) {
            jq("#sendProcessProgress .progress").css("width", progressItem.percentage + "%");
            jq("#sendProcessProgress .percent").text(progressItem.percentage + "%");
            jq("#emailsAlreadySentCount").html(progressItem.status.deliveryCount);
            jq("#emailsEstimatedTime").html(progressItem.status.estimatedTime);
            jq("#emailsTotalCount").html(progressItem.status.recipientCount);
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
            localStorageManager.setItem("senderTargets", targets);
        },
        
        getItems: function () {
            var s = localStorageManager.getItem("senderTargets") || "[]";
            localStorageManager.removeItem("senderTargets");
            return s;
        },
    };
})();