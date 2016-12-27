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
if (typeof ASC.Mail === "undefined") {
    ASC.Mail = (function () {
        return {};
    })();
}
if (typeof ASC.Mail.Utility === "undefined") {
    ASC.Mail.Utility = (function () {
        var parseErrorTypes = {
                None: 0,
                EmptyRecipients: 1,
                IncorrectEmail: 2
            },
            lastSentMessageId = 0;


        function checkCalendarRequirements() {
            if (!window.ICAL)
                throw "Required ICAL module (~/js/third-party/ical.min.js)";

            if (!window.moment)
                throw "Required moment.js module (~/js/third-party/moment.min.js, ~/js/third-party/moment-timezone.min.js)";

            if (!window.RRule)
                throw "Required moment.js module (~/js/third-party/rrule.js, ~/js/third-party/nlp.js)";
        }

        function getAccounts(params) {
            var d = jq.Deferred();

            if (!params.skipAccountsCheck)
                window.Teamlab.getAccounts(params, { success: d.resolve, error: d.reject });
            else
                d.resolve(params, []);

            return d.promise();
        }

        function checkAccounts(params, accounts) {
            var d = jq.Deferred();

            try {
                if (params.skipAccountsCheck) {
                    d.resolve(params);
                    return d.promise();
                }

                var message = params.message;

                if (accounts.length === 0) {
                    throw "No accounts.";
                }

                function getDefaultAccount(accs) {
                    var def = jq.grep(accs, function(a) { return a.is_default === true }),
                        acc = (def.length > 0 ? def[0] : accs[0]);

                    return new ASC.Mail.Address(
                        acc.name,
                        acc.email,
                        true);
                }

                if (!message.from) {
                    message.from = getDefaultAccount(accounts);
                } else {
                    var found = jq.grep(accounts, function (a) {
                        return  message.from.EqualsByEmail(a.email);
                    });

                    if (found.length === 0) {
                        if (!params.origCalendarEvent.canNotify)
                            throw "From not found in your accounts.";

                        message.from = getDefaultAccount(accounts);
                    }
                    else if (found.length > 1) {
                        throw "Found more then one account by From.";
                    } else if (!found[0].enabled) {
                        throw "Account is disabled.";
                    }
                }

                d.resolve(params);

            } catch (e) {
                d.reject(params, e);
            }

            return d.promise();
        }

        function saveDraft(params) {
            var d = jq.Deferred();

            if (params.skipSave) {
                d.resolve(params, { messageUrl: null, saveSkipped: true });
                return d.promise();
            }

            var message = params.message;

            window.Teamlab.saveMailMessage(
                params,
                message,
                {
                    success: d.resolve,
                    error: d.reject
                });

            return d.promise();
        }

        function afterSave(params, savedMessage) {
            var d = jq.Deferred();

            if (params.skipSave) {
                d.resolve(params, { messageUrl: null, saveSkipped: true });
                return d.promise();
            }

            var message = params.message;
            message.id = savedMessage.id;

            d.resolve(params, { messageUrl: "/addons/mail/#draftitem/" + message.id });

            return d.promise();
        }

        function addDocuments(params, saveResult) {
            var d = jq.Deferred();

            if (params.skipSave) {
                d.resolve(params, saveResult);
                return d.promise();
            }

            var message = params.message;

            if (message.HasDocumentsForSave()) {
                var documentIds = message.GetDocumentsForSave();

                function addDoc(fileId) {
                    var dfd = jq.Deferred();

                    var data = {
                        fileId: fileId,
                        version: "",
                        shareLink: ""
                    };

                    window.Teamlab.addMailDocument(
                        {
                            message: message,
                            documentId: fileId,
                            sendImmediately: params.sendImmediately
                        },
                        message.id,
                        data,
                        {
                            success: function (params, attachedDocument) {
                                message.attachments.push(attachedDocument);
                                message.RemoveDocumentAfterSave(params.documentId);
                                dfd.resolve(params, saveResult);
                            },
                            error: dfd.reject
                        });

                    return dfd.promise();
                }

                var addarray = [];
                for (var i = 0, len = documentIds.length; i < len; i++) {
                    var documentId = documentIds[i];
                    addarray.push(addDoc(documentId));
                }

                jq.when.apply(jq, addarray).done(function () {
                    if (!message.HasDocumentsForSave())
                        d.resolve(params, saveResult);
                    else
                        d.reject(params, "Something goes wrong.");
                })
                .fail(d.reject);

            } else {
                d.resolve(params, saveResult);
            }

            return d.promise();
        }

        function sendDraft(params) {
            var d = jq.Deferred();

            if (params.skipSend) {
                d.resolve(params, -1);
                return d.promise();
            }

            var message = params.message;

            window.Teamlab.sendMailMessage(
                params,
                message,
                {
                    success: d.resolve,
                    error: d.reject
                });

            return d.promise();
        }

        function afterSend(params, messageId) {
            var d = jq.Deferred();

            var sendResult = params.skipSend ?
            { messageUrl: null, sendSkipped: true } :
            { messageUrl: "/addons/mail/#conversation/" + messageId };

            if (!params.skipSend && (!jq.connection || jq.connection.hub && jq.connection.hub.state !== jq.connection.connectionState.connected)) {
                var state = 0;
                if (params.method === "REQUEST")
                    state = 1;
                else if (params.method === "REPLY")
                    state = 2;
                else if (params.method === "CANCEL")
                    state = 3;

                lastSentMessageId = messageId;

                setTimeout(function () {
                    ASC.Mail.Utility._showSignalRMailNotification(state);
                }, 3000);
            }

            if (params.method === "REPLY" && !params.skipCalendarImport) {
                Teamlab.importCalendarEventIcs({}, params.calendarId, params.message.calendarIcs, {
                    success: function() {
                        d.resolve(params, sendResult);
                    },
                    error: d.reject,
                    max_request_attempts: 1
                });
            } else {
                d.resolve(params, sendResult);
            }

            return d.promise();
        }

        function getCalendarEventByUid(params) {
            var d = jq.Deferred();

            Teamlab.getCalendarEventByUid(
                params,
                params.calendarEventUid,
                {
                    success: d.resolve,
                    error: d.reject,
                    max_request_attempts: 1
                });

            return d.promise();
        }

        function checkCalendarValid(params, ical) {
            var d = jq.Deferred();

            try {
                params.origCalendarEvent = ical;

                if (!ical || !ical.mergedIcs)
                    throw "Empty ical";

                var jCalData = window.ICAL.parse(ical.mergedIcs);

                var comp = new window.ICAL.Component(jCalData);
                if (comp.name !== "vcalendar")
                    throw "Unsupported ical type (only vcalendar)";

                var vevent = comp.getFirstSubcomponent("vevent");
                if (!vevent)
                    throw "VEVENT not found";

                var event = new window.ICAL.Event(vevent);

                if (event.uid !== params.calendarEventUid) {
                    throw "No found event by calendarEventUid=\"{0}\"".format(params.calendarEventUid);
                }

                var iCalInfo = {
                    comp: comp,
                    vevent: vevent,
                    event: event,
                    organizerAddress: new ASC.Mail.Address(Encoder.htmlDecode(vevent.getFirstProperty("organizer").getParameter("cn") || ""),
                        event.organizer.toLowerCase().replace("mailto:", "")),
                    calendarTimeZone: ical.timeZone
                };

                var rrule = vevent.getFirstPropertyValue("rrule");

                if (rrule !== null) {
                    iCalInfo.recurrence = ASC.Mail.Utility.ToRRuleRfc2445String(rrule);
                }

                if (!ASC.Mail.Utility.IsValidEmail(iCalInfo.organizerAddress.email)) {
                    throw "Organizer email \"{0}\" is invalid".format(iCalInfo.organizerAddress.email, event.organizer);
                }

                iCalInfo.organizerAddress.isValid = true;

                if (!event.attendees ||
                    event.attendees.length === 0)
                    throw "Unable to find attendees";

                switch (params.method) {
                    case "REQUEST":
                    case "REPLY":
                        if (comp.getFirstPropertyValue("method") !== "REQUEST" && 
                            comp.getFirstPropertyValue("method") !== "REPLY" &&
                            comp.getFirstPropertyValue("method") !== "PUBLISH")
                            throw "Allow only REQUEST";
                    case "CANCEL":
                        break;

                    default:
                        throw "Unsupported send method \"{0}\"".format(params.method);
                }

                if (params.method === "REPLY") {
                    var curAttendee = jq.grep(event.attendees, function (a) {
                        return ASC.Mail.Utility.IsEqualEmail(a.getFirstValue().toLowerCase().replace("mailto:", ""), params.attendeeEmail);
                    })[0];

                    if (!curAttendee) {
                        throw "Attendee with email \"{0}\" not found".format(params.attendeeEmail);
                    }

                    if (curAttendee.getParameter("partstat") === params.replyDecision) {
                        throw "Attendee with email \"{0}\" already answered \"{1}\"".format(params.attendeeEmail, params.replyDecision);
                    }

                    iCalInfo.currentAttendee = curAttendee;
                    iCalInfo.currentAttendeeAddress = new ASC.Mail.Address(Encoder.htmlDecode(curAttendee.getParameter("cn") || ""),
                        curAttendee.getFirstValue().toLowerCase().replace("mailto:", ""));

                    if (!ASC.Mail.Utility.IsValidEmail(iCalInfo.currentAttendeeAddress.email)) {
                        throw "Attendee email \"{0}\" is invalid".format(iCalInfo.currentAttendeeAddress.email, iCalInfo.currentAttendee);
                    }

                    iCalInfo.currentAttendeeAddress.isValid = true;
                    params.skipSend = iCalInfo.currentAttendeeAddress.EqualsByEmail(iCalInfo.organizerAddress);
                    if (params.skipSend)
                        params.skipSave = true;

                } else if (params.method === "CANCEL" && params.attendeesEmails) {
                    var cancelAttendees = jq.grep(event.attendees, function (a) {
                        var exists = false;
                        for (var i = 0, len = params.attendeesEmails.length; i < len; i++) {
                            if (ASC.Mail.Utility.IsEqualEmail(a.getFirstValue().toLowerCase().replace("mailto:", ""), params.attendeesEmails[i])) {
                                exists = true;
                                break;
                            }
                        }

                        return exists;
                    });

                    if (!cancelAttendees)
                        throw "Attendees for cancel not found in event.";

                    iCalInfo.cancelAttendees = cancelAttendees;
                }

                d.resolve(params, iCalInfo);

            } catch (e) {
                d.reject(params, e);
            }

            return d.promise();
        }

        function createBoby(params, iCalInfo) {
            window.moment.locale(ASC.Resources.Master.TwoLetterISOLanguageName);

            var start = window.moment(iCalInfo.event.startDate.toJSDate()),
                end = window.moment(iCalInfo.event.endDate.toJSDate()),
                dateStartAllDay = iCalInfo.event.startDate.isDate,
                dateEndAllDay = iCalInfo.event.endDate.isDate,
                dtStart = iCalInfo.calendarTimeZone && !dateStartAllDay ? start.utcOffset(iCalInfo.calendarTimeZone.offset) : start,
                dtEnd = iCalInfo.calendarTimeZone && !dateEndAllDay ? end.utcOffset(iCalInfo.calendarTimeZone.offset) : end;

            if (dateStartAllDay &&
                dateEndAllDay &&
                start.isBefore(end, 'day')) {
                // Correct end date
                end = end.clone().startOf('day').add(-1, 'day');
                dtEnd = end;
            }

            function getMapUrl(location) {
                if (!location)
                    return '';

                return "https://maps.google.com/maps?q={0}".format(location.split(/[,\s]/).join('+'));
            }

            var info = {
                summary: iCalInfo.event.summary,
                location: iCalInfo.event.location,
                allDay: false,
                orgName: iCalInfo.organizerAddress.name,
                orgEmail: iCalInfo.organizerAddress.email,
                mailToHref: "mailto:" + iCalInfo.organizerAddress.email,
                mapUrl: iCalInfo.event.location ? getMapUrl(iCalInfo.event.location) : null
            };

            info.dateEvent = ASC.Mail.Utility.ToCalendarDateString(dtStart, dtEnd, dateStartAllDay, dateEndAllDay);
            if (iCalInfo.recurrence)
                info.rruleText = ASC.Mail.Utility.ToCalendarRRuleString(iCalInfo.recurrence, dtStart);

            switch (params.method) {
                case "REQUEST":
                    info.action = params.isUpdate ?
                        ASC.Resources.Master.Resource.MailIcsUpdateDescription :
                        ASC.Resources.Master.Resource.MailIcsRequestDescription.format(
                        (iCalInfo.organizerAddress.name || iCalInfo.organizerAddress.email) ||
                        (iCalInfo.currentAttendeeAddress.name || iCalInfo.currentAttendeeAddress.email));
                    break;
                case "REPLY":
                    var res;
                    switch (params.replyDecision) {
                        case "ACCEPTED":
                            res = ASC.Resources.Master.Resource.MailIcsReplyYesDescription;
                            break;
                        case "TENTATIVE":
                            res = ASC.Resources.Master.Resource.MailIcsReplyMaybeDescription;
                            break;
                        case "DECLINED":
                            res = ASC.Resources.Master.Resource.MailIcsReplyNoDescription;
                            break;
                        default:
                            throw "Unsupported attendee partstart";
                    }
                    info.action = res.format(iCalInfo.currentAttendeeAddress.name ||
                        iCalInfo.currentAttendeeAddress.email);
                    break;
                case "CANCEL":
                    info.action = ASC.Resources.Master.Resource.MailIcsCancelDescription;
                    break;
                default:
                    break;
            }

            var body = jq("<div/>").html(
                jq.tmpl('template-mailCalendar', info))
                .prop('outerHTML');

            body = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\
                    <html xmlns=\"http://www.w3.org/1999/xhtml\">\
                        <head>\
                            <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\
                            <link href=\"https://fonts.googleapis.com/css?family=Open+Sans:900,800,700,600,500,400,300&subset=latin,cyrillic-ext,cyrillic,latin-ext\" rel=\"stylesheet\" type=\"text/css\" />\
                            <meta name=\"viewport\" content=\"width=device-width\" />\
                        </head>\
                        <body style=\"margin: 0; padding: 0; text-align: center; width: 100%; font-family: 'Open Sans', Tahoma, Arial; font-size: 14px; color: #333;\">" + body + "</body>\
                    </html>";

            return body;
        }

        function createMessageWithCalendarRequest(params, iCalInfo) {
            var d = jq.Deferred(),
            message = new ASC.Mail.Message();

            var toAddresses = [];
            jq.each(iCalInfo.event.attendees, function (i, a) {
                var email = a.getFirstValue().toLowerCase().replace("mailto:", "");
                var name = a.getParameter("cn") || "";
                if (ASC.Mail.Utility.IsValidEmail(email)) {

                    if (iCalInfo.organizerAddress.EqualsByEmail(email))
                        return true; // skip org email

                    var toAddress = new ASC.Mail.Address(name, email, true);

                    if (params.attendeesEmails) {
                        var found = jq.grep(params.attendeesEmails, function (aEmail) {
                            return toAddress.EqualsByEmail(aEmail);
                        });

                        if (found.length === 0)
                            return true; //continue
                    }

                    toAddresses.push(toAddress);
                }
                return true;
            });

            if (toAddresses.length > 0) {
                var fromAddress = iCalInfo.organizerAddress;

                var subject = params.isUpdate ?
                    ASC.Resources.Master.Resource.MailIcsUpdateSubject.format(iCalInfo.event.summary) :
                    ASC.Resources.Master.Resource.MailIcsRequestSubject.format(iCalInfo.event.summary);

                message.from = fromAddress;
                message.to = toAddresses;
                message.subject = subject;
                message.body = createBoby(params, iCalInfo);
                message.calendarIcs = iCalInfo.comp.toString();

                d.resolve(message, params);
            } else {
                d.reject(params, "Empty recipients list (toAddresses)");
            }

            return d.promise();
        }

        function createMessageWithCalendarReply(params, iCalInfo) {
            var d = jq.Deferred();
            var message = new ASC.Mail.Message();
            try {
                iCalInfo.comp.updatePropertyWithValue("prodid", "-//Ascensio System//OnlyOffice Calendar//EN");
                iCalInfo.comp.updatePropertyWithValue("method", params.method);

                iCalInfo.currentAttendee.removeParameter("rsvp");
                iCalInfo.currentAttendee.setParameter("partstat", params.replyDecision);

                var t = window.ICAL.Time.now();

                iCalInfo.vevent.updatePropertyWithValue("dtstamp", t);
                iCalInfo.vevent.updatePropertyWithValue("created", t);
                iCalInfo.vevent.updatePropertyWithValue("last-modified", t);

                iCalInfo.vevent.removeAllProperties("attendee");
                iCalInfo.vevent.addProperty(iCalInfo.currentAttendee);

                var toAddress = iCalInfo.organizerAddress;

                var subject;
                switch (params.replyDecision) {
                    case "ACCEPTED":
                        subject = ASC.Resources.Master.Resource.MailIcsReplyYesDescription;
                        break;
                    case "TENTATIVE":
                        subject = ASC.Resources.Master.Resource.MailIcsReplyMaybeDescription;
                        break;
                    case "DECLINED":
                        subject = ASC.Resources.Master.Resource.MailIcsReplyNoDescription;
                        break;
                    default:
                        throw "Unsupported attendee partstart";
                }
                subject = "{0} \"{1}\"".format(
                    subject.format(iCalInfo.currentAttendeeAddress.name ||
                        iCalInfo.currentAttendeeAddress.email),
                    iCalInfo.event.summary);

                var fromAddress = new ASC.Mail.Address(
                    "",
                    params.attendeeEmail,
                    true);

                message.from = fromAddress;
                message.to = [toAddress];
                message.subject = ASC.Resources.Master.Resource.MailIcsReplySubject.format(subject);
                message.body = createBoby(params, iCalInfo);
                message.calendarIcs = iCalInfo.comp.toString();

                d.resolve(message, params);
            } catch (e) {
                d.reject(params, e);
            }

            return d.promise();
        }

        function createMessageWithCalendarCancel(params, iCalInfo) {
            var d = jq.Deferred();
            var message = new ASC.Mail.Message();

            iCalInfo.comp.updatePropertyWithValue("prodid", "-//Ascensio System//OnlyOffice Calendar//EN");
            iCalInfo.comp.updatePropertyWithValue("method", params.method);
            iCalInfo.comp.updatePropertyWithValue("status", "CANCELLED");

            var toAddresses = [];
            if (iCalInfo.cancelAttendees) {
                iCalInfo.vevent.removeAllProperties("attendee");
                for (var i = 0, len = iCalInfo.cancelAttendees.length; i < len; i++) {
                    iCalInfo.cancelAttendees[i].removeParameter("rsvp");
                    iCalInfo.cancelAttendees[i].setParameter("partstat", "DECLINED");
                    iCalInfo.vevent.addProperty(iCalInfo.cancelAttendees[i]);
                    toAddresses.push(new ASC.Mail.Address(
                                            iCalInfo.cancelAttendees[i].getParameter("cn"),
                                            iCalInfo.cancelAttendees[i].getFirstValue().toLowerCase().replace("mailto:", ""),
                                            true));
                }
            } else {
                jq.each(iCalInfo.event.attendees, function (i, a) {
                    var email = a.getFirstValue().toLowerCase().replace("mailto:", "");
                    var name = a.getParameter("cn") || "";
                    if (ASC.Mail.Utility.IsValidEmail(email)) {
                        toAddresses.push((new ASC.Mail.Address(name, email, true)));
                    }
                });
            }

            var t = window.ICAL.Time.now();

            iCalInfo.vevent.updatePropertyWithValue("dtstamp", t);
            iCalInfo.vevent.updatePropertyWithValue("created", t);
            iCalInfo.vevent.updatePropertyWithValue("last-modified", t);

            var fromAddress = iCalInfo.organizerAddress;

            message.from = fromAddress;
            message.to = toAddresses;
            message.subject = ASC.Resources.Master.Resource.MailIcsCancelSubject.format(iCalInfo.event.summary);
            message.body = createBoby(params, iCalInfo);
            message.calendarIcs = iCalInfo.comp.toString();

            d.resolve(message, params);

            return d.promise();
        }

        function sendCalendarRequest(params) {
            var d = jq.Deferred();

            try {
                checkCalendarRequirements();

                if (!params.calendarEventUid)
                    throw "Param 'calendarEventUid' value is empty";

                getCalendarEventByUid(params)
                    .then(checkCalendarValid, d.reject)
                    .then(createMessageWithCalendarRequest, d.reject)
                    .then(ASC.Mail.Utility.SendMessage, d.reject)
                    .then(d.resolve, d.reject);

            } catch (e) {
                d.reject(params, e);
            }

            return d.promise();
        }

        function sendCalendarCancel(params) {
            var d = jq.Deferred();

            try {
                checkCalendarRequirements();

                if (!params.calendarEventUid)
                    throw "Param 'calendarEventUid' value is empty";

                getCalendarEventByUid(params)
                    .then(checkCalendarValid, d.reject)
                    .then(createMessageWithCalendarCancel, d.reject)
                    .then(ASC.Mail.Utility.SendMessage, d.reject)
                    .then(d.resolve, d.reject);

            } catch (e) {
                d.reject(params, e);
            }

            return d.promise();
        }

        return {
            ParseErrorTypes: parseErrorTypes,
            /**
             * Parse addresses from string
             * @param {String} str
             * @return {Object} result with array of addresses and array of errors
             */
            ParseAddresses: function (str) {
                var parsedObjs = {
                    addresses: [],
                    errors: []
                };

                if ("string" !== typeof str || str.length === 0) {
                    parsedObjs.errors.push({ message: "Empty recipients", type: parseErrorTypes.EmptyRecipients, errorItem: null });
                    return parsedObjs;
                }

                function contact2Obj(e) {
                    var t = /^"(.*)"\s*<([^>]+)>$/,
                        n = /^(.*)<([^>]+)>$/,
                        i = e.match(t) || e.match(n);
                    return i ? {
                        name: jq.trim(i[1].replace(/\\"/g, '"').replace(/\\\\/g, "\\")),
                        email: jq.trim(i[2])
                    } : {
                        email: e
                    }
                };

                function obj2Contact(e) {
                    var t = undefined;
                    if (e.email) {
                        t = e.email;
                        e.name && (t = '"' + e.name.replace(/\\/g, "\\\\").replace(/"/g, '\\"') + '" <' + t + ">");
                    }
                    return t;
                };

                function parseAndAppend(s) {
                    s = obj2Contact(contact2Obj(s));
                    var parsed = emailAddresses.parseOneAddress(s);
                    if (parsed) {
                        var isValid = true;
                        if (parsed.domain.indexOf(".") === -1) {
                            isValid = false;
                            parsedObjs.errors.push({ message: "Incorrect domain", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }

                        if (!/^[\x00-\x7F]+$/.test(punycode.toUnicode(parsed.domain))) {
                            isValid = false;
                            parsedObjs.errors.push({ message: "Punycode domains are not suppoted", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }

                        if (!/^[\x00-\x7F]+$/.test(parsed.local))
                        {
                            isValid = false;
                            parsedObjs.errors.push({ message: "Incorrect localpart", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }

                        parsedObjs.addresses.push(new ASC.Mail.Address(parsed.name || "", parsed.address, isValid));
                    } else {
                        var invalidEmail = s.trim();
                        if (invalidEmail) {
                            parsedObjs.addresses.push(new ASC.Mail.Address("", invalidEmail, false));
                            parsedObjs.errors.push({ message: "Incorrect email", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }
                    }
                }

                var e = str.replace(/[\s,;]*$/, ",");
                for (var t, i = false, o = 0, a = 0, s = e.length; s > a; a += 1) {
                    switch (e.charAt(a)) {
                        case ",":
                        case ";":
                            if (!i) {
                                t = e.substring(o, a);
                                t = jq.trim(t);
                                if (t) {
                                    parseAndAppend(t);
                                }
                                o = a + 1;
                            }
                            break;
                        case '"':
                            "\\" !== e.charAt(a - 1) && '"' !== e.charAt(a + 1) && (i = !i);
                    }
                }
                if (!parsedObjs.addresses.length) {
                    parseAndAppend(e.replace(/,\s*$/, ""));
                }
                return parsedObjs;
            },
            /**
             * Parse address from string
             * @param {String} str
             * @return {ASC.Mail.Address} result
             */
            ParseAddress: function (str) {
                var parsed = ASC.Mail.Utility.ParseAddresses(str);

                if (parsed.errors.length > 0 ||
                    parsed.addresses.length !== 1) {
                    return new ASC.Mail.Address("", str, false);
                }

                return parsed.addresses[0];
            },
            /**
             * Check email validity
             * @param {String}/{ASC.Mail.Address} email
             * @return {Bool} result
             */
            IsValidEmail: function (email) {
                return ASC.Mail.Utility.ParseAddress(email).isValid;
            },
            /**
             * Check domain validity
             * @param {String} domain
             * @return {Bool} result
             */
            IsValidDomainName: function (domain) {
                var parsed = emailAddresses.parseOneAddress("test@" + domain);
                return !!parsed && parsed.domain === domain && domain.indexOf(".") !== -1;
            },
            /**
             * Compare emails
             * @param {String}/{ASC.Mail.Address} email1
             * @param {String}/{ASC.Mail.Address} email2
             * @return {Bool} result
             */
            IsEqualEmail: function(email1, email2) {
                var parsed1 = ASC.Mail.Utility.ParseAddress(email1);
                var parsed2 = ASC.Mail.Utility.ParseAddress(email2);

                if (!parsed1.isValid ||
                    !parsed2.isValid) {
                    return false;
                }

                return parsed1.email === parsed2.email;
            },
            /**
             * Save message to Mail Drafts
             * @param {ASC.Mail.Message} message
             * @param {Object} params (Example: { skipAccountsCheck: true, skipSave: true });
             * @return {Object} result with messageUrl;
             */
            SaveMessageInDrafts: function(message, params) {
                var d = jq.Deferred();

                params = params || { skipAccountsCheck: true };

                try {

                    if (!(message instanceof ASC.Mail.Message)) {
                        throw "Unsupported message format";
                    }

                    if (!params.hasOwnProperty("skipAccountsCheck") || !message.from)
                        params.skipAccountsCheck = false;

                    params.message = message;

                    getAccounts(params)
                        .then(checkAccounts, d.reject)
                        .then(saveDraft, d.reject)
                        .then(afterSave, d.reject)
                        .then(addDocuments, d.reject)
                        .then(d.resolve, d.reject);

                } catch (e) {
                    d.reject(params, e);
                }

                return d.promise();
            },
            /**
             * Send message
             * @param {ASC.Mail.Message} message
             * @param {Object} params (Example: { skipAccountsCheck: true, skipSave: true });
             * @return {Object} result with messageUrl;
             */
            SendMessage: function (message, params) {
                var d = jq.Deferred();

                params = params || { skipAccountsCheck: true };

                try {

                    if (!(message instanceof ASC.Mail.Message)) {
                        throw "Unsupported message format";
                    }

                    if (!params.hasOwnProperty("skipAccountsCheck") || !message.from)
                        params.skipAccountsCheck = false;

                    function improveAddresses(addresses) {
                        var result = { addresses: [], hasBad: false };

                        if (!addresses || !addresses.length)
                            return result;

                        if (!jq.isArray(addresses) && ("string" === typeof addresses)) {
                            var p = ASC.Mail.Utility.ParseAddresses(addresses);
                            result.addresses = p.addresses;
                            result.hasBad = p.errors.length > 0;
                            return result;
                        }

                        for (var i = 0, len = addresses.length; i < len; i++) {
                            var a = !(addresses[i] instanceof ASC.Mail.Address)
                                ? ASC.Mail.Utility.ParseAddress(addresses[i])
                                : addresses[i];

                            result.addresses.push(a);

                            if (!a.isValid)
                                result.hasBad = true;
                        }

                        return result;
                    }

                    var t = improveAddresses(message.to);
                    message.to = t.addresses;

                    if (message.to.length === 0) {
                        throw "To field is empty";
                    } else if (t.hasBad) {
                        throw "To field contains invalid recipients.";
                    }

                    t = improveAddresses(message.cc);
                    message.cc = t.addresses;

                    if (t.hasBad) {
                        throw "Cc field contains invalid recipients.";
                    }

                    t = improveAddresses(message.bcc);
                    message.bcc = t.addresses;

                    if (t.hasBad) {
                        throw "Bcc field contains invalid recipients.";
                    }

                    ASC.Mail.Utility.SaveMessageInDrafts(message, params)
                        .then(sendDraft, d.reject)
                        .then(afterSend, d.reject)
                        .then(d.resolve, d.reject);

                } catch (e) {
                    d.reject(params, e);
                }

                return d.promise();
            },
            /**
             * Send message with Calendar Request
             * @param {Int} calendarId
             * @param {String} calendarEventUid
             * @param {Array} attendeesEmails
             * @param {Object} params (Example: { skipAccountsCheck: true, skipSave: true });
             * @return {Object} result with messageUrl;
             */
            SendCalendarRequest: function (calendarId, calendarEventUid, attendeesEmails, params) {
                params = params || { skipAccountsCheck: true };
                params.calendarId = calendarId || -1;
                params.calendarEventUid = calendarEventUid;
                params.method = "REQUEST";
                params.skipSave = params.skipSave || true;
                if (attendeesEmails)
                    params.attendeesEmails = attendeesEmails;

                return sendCalendarRequest(params);
            },
            /**
             * Send message with Calendar Update
             * @param {Int} calendarId
             * @param {String} calendarEventUid
             * @param {Array} attendeesEmails
             * @param {Object} params (Example: { skipAccountsCheck: true, skipSave: true });
             * @return {Object} result with messageUrl;
             */
            SendCalendarUpdate: function (calendarId, calendarEventUid, attendeesEmails, params) {
                params = params || { skipAccountsCheck: false };
                params.calendarId = calendarId || -1;
                params.calendarEventUid = calendarEventUid;
                params.method = "REQUEST";
                params.isUpdate = true;
                params.skipSave = params.skipSave || true;
                if (attendeesEmails)
                    params.attendeesEmails = attendeesEmails;

                return sendCalendarRequest(params);
            },
            /**
             * Send message with Calendar Reply
             * @param {Int} calendarId
             * @param {String} calendarEventUid
             * @param {String} attendeeEmail
             * @param {String} attendeeDecision (Support only: ACCEPTED/DECLINED/TENTATIVE)
             * @param {Object} params (Example: { skipAccountsCheck: true, skipSave: true });
             * @return {Object} result with messageUrl;
             */
            SendCalendarReply: function (calendarId, calendarEventUid, attendeeEmail, attendeeDecision, params) {
                var d = jq.Deferred();
                params = params || { skipAccountsCheck: true };
                params.calendarId = calendarId || -1;
                params.calendarEventUid = calendarEventUid;
                params.method = "REPLY";
                params.attendeeEmail = attendeeEmail;
                params.replyDecision = attendeeDecision;
                params.skipSave = params.skipSave || true;

                try {
                    checkCalendarRequirements();

                    if (!calendarEventUid)
                        throw "Param 'calendarEventUid' value is empty", calendarEventUid;

                    if (!attendeeEmail)
                        throw "Param 'attendeeEmail' value is empty", attendeeEmail;

                    if (!attendeeDecision)
                        throw "Param 'attendeeDecision' value is empty", attendeeDecision;

                    if (attendeeDecision !== "ACCEPTED" &&
                        attendeeDecision !== "DECLINED" &&
                        attendeeDecision !== "TENTATIVE")
                        throw "Param 'attendeeDecision' value is invalid (ACCEPTED, DECLINED, TENTATIVE)", attendeeDecision;

                    if (!ASC.Mail.Utility.IsValidEmail(attendeeEmail))
                        throw "Param 'attendeeEmail' value is invalid", attendeeEmail;

                    getCalendarEventByUid(params)
                        .then(checkCalendarValid, d.reject)
                        .then(createMessageWithCalendarReply, d.reject)
                        .then(ASC.Mail.Utility.SendMessage, d.reject)
                        .then(d.resolve, d.reject);

                } catch (e) {
                    d.reject(params, e);
                }

                return d.promise();
            },
            /**
             * Send message with Calendar Reply ics
             * @param {String} calendarEventUid
             * @param {String} calendarEventIcs
             * @param {String} attendeeEmail
             * @param {String} attendeeDecision (Support only: ACCEPTED/DECLINED/TENTATIVE)
             * @param {Object} params (Example: { skipAccountsCheck: true, skipSave: true });
             * @return {Object} result with messageUrl;
             */
            SendCalendarReplyIcs: function (calendarEventUid, calendarEventIcs, attendeeEmail, attendeeDecision, params) {
                var d = jq.Deferred();
                params = params || { skipAccountsCheck: true };
                params.calendarId = -1;
                params.calendarEventUid = calendarEventUid;
                params.method = "REPLY";
                params.attendeeEmail = attendeeEmail;
                params.replyDecision = attendeeDecision;
                params.skipSave = params.skipSave || true;
                params.skipCalendarImport = params.skipCalendarImport || true;

                try {
                    checkCalendarRequirements();

                    if (!calendarEventUid)
                        throw "Param 'calendarEventUid' value is empty", calendarEventUid;

                    if (!attendeeEmail)
                        throw "Param 'attendeeEmail' value is empty", attendeeEmail;

                    if (!attendeeDecision)
                        throw "Param 'attendeeDecision' value is empty", attendeeDecision;

                    if (attendeeDecision !== "ACCEPTED" &&
                        attendeeDecision !== "DECLINED" &&
                        attendeeDecision !== "TENTATIVE")
                        throw "Param 'attendeeDecision' value is invalid (ACCEPTED, DECLINED, TENTATIVE)", attendeeDecision;

                    if (!ASC.Mail.Utility.IsValidEmail(attendeeEmail))
                        throw "Param 'attendeeEmail' value is invalid", attendeeEmail;

                    checkCalendarValid(params, { mergedIcs: calendarEventIcs })
                        .then(createMessageWithCalendarReply, d.reject)
                        .then(ASC.Mail.Utility.SendMessage, d.reject)
                        .then(d.resolve, d.reject);

                } catch (e) {
                    d.reject(params, e);
                }

                return d.promise();
            },
            /**
             * Send message with Calendar Cancel
             * @param {Int} calendarId
             * @param {String} calendarEventUid
             * @param {Array} attendeesEmails
             * @param {Object} params (Example: { skipAccountsCheck: true, skipSave: true });
             * @return {Object} result with messageUrl;
             */
            SendCalendarCancel: function (calendarId, calendarEventUid, attendeesEmails, params) {
                params = params || { skipAccountsCheck: true };
                params.calendarId = calendarId || -1;
                params.calendarEventUid = calendarEventUid;
                params.method = "CANCEL";
                params.skipSave = params.skipSave || true;
                if (attendeesEmails)
                    params.attendeesEmails = attendeesEmails;
                return sendCalendarCancel(params);
            },
            _showSignalRMailNotification: function (state) {
                var module = StudioManager.getCurrentModule();
                if (module !== "mail" && module !== "calendar")
                    return;

                switch (state) {
                    case -1:
                        toastr.error(ASC.Resources.Master.Resource.MailSendMessageError);

                        if (module === "mail" && window.mailAlerts) { // mail hook
                            window.mailAlerts.check(lastSentMessageId > 0 ? { showFailureOnlyMessageId: lastSentMessageId } : {});
                        }
                        break;
                    case 0:
                        toastr.success(ASC.Resources.Master.Resource.MailSentMessageText);
                        if (module === "mail" && window.mailAlerts) { // mail hook
                            if (!ASC.Resources.Master.Hub.Url ||
                            (jq.connection && jq.connection.hub.state !== jq.connection.connectionState.connected)) {
                                setTimeout(function() {
                                    window.mailAlerts.check(lastSentMessageId > 0 ? { showFailureOnlyMessageId: lastSentMessageId } : {});
                                }, 5000);
                            }
                        }
                        break;
                    case 1:
                        toastr.success(ASC.Resources.Master.Resource.MailSentIcalRequestText);
                        break;
                    case 2:
                        toastr.success(ASC.Resources.Master.Resource.MailSentIcalResponseText);
                        break;
                    case 3:
                        toastr.success(ASC.Resources.Master.Resource.MailSentIcalCancelText);
                        break;
                    default:
                        break;
                }

                if (module === "mail" && window.messagePage) {
                    window.messagePage.refreshMailAfterSent(state);
                }
            },
            /**
             * Convert Moment DateTimes of calendar event to formated string 
             * @param {Moment DateTime} dtStart
             * @param {Moment DateTime} dtEnd
             * @return {String} formated string;
             */
            ToCalendarDateString: function (dtStart, dtEnd, allDayStart, allDayEnd) {
                checkCalendarRequirements();

                var dateEvent;
                var strStart = dtStart.format("ddd, DD MMM YYYY"),
                    strEnd = dtEnd.format("ddd, DD MMM YYYY"),
                    strStartTime = allDayStart ? "" : dtStart.format("LT"),
                    strEndTime = allDayEnd ? "" : dtEnd.format("LT"),
                    strTz = "(GMT{0})".format(dtStart.format("Z"));

                if (dtStart.isSame(dtEnd, 'day')) {
                    if (allDayStart)
                        dateEvent = "{0}, {1}".format(strStart, ASC.Resources.Master.Resource.MailIcsCalendarAllDayEventLabel);
                    else
                        dateEvent = "{0}, {1} - {2} {3}".format(strStart, strStartTime, strEndTime, strTz);
                } else {
                    if (allDayStart && allDayEnd) {
                        dateEvent = "{0}, {1} - {2}, {1}".format(strStart, ASC.Resources.Master.Resource.MailIcsCalendarAllDayEventLabel, strEnd);
                    } else if (allDayStart && !allDayEnd) {
                        dateEvent = "{0}, {1} - {2}, {3} {4}".format(strStart, ASC.Resources.Master.Resource.MailIcsCalendarAllDayEventLabel, strEnd, strEndTime, strTz);
                    } else if (!allDayStart && allDayEnd) {
                        dateEvent = "{0}, {1} {2} - {3}, {4}".format(strStart, strStartTime, strTz, strEnd, ASC.Resources.Master.Resource.MailIcsCalendarAllDayEventLabel);
                    } else {
                        dateEvent = "{0}, {1} - {2}, {3} {4}".format(strStart, strStartTime, strEnd, strEndTime, strTz);
                    }
                }

                return dateEvent;
            },
            /**
             * Convert RRule of calendar event to user-friedndly string 
             * @param {String} rruleRfc2445
             * @param {Moment DateTime} dtStart
             * @return {String} user-friedndly string;
             */
            ToCalendarRRuleString: function(rruleRfc2445, dtStart, notLocalize) {
                checkCalendarRequirements();

                if (!dtStart)
                    dtStart = window.moment();

                try {
                    var options = RRule.parseString(rruleRfc2445);
                    options.dtstart = dtStart.toDate();
                    var rule = new RRule(options);

                    var language = {
                        dayNames: ASC.Resources.Master.DayNames,
                        monthNames: ASC.Resources.Master.MonthNames,
                        tokens: {
                            'SKIP': /^[ \r\n\t]+|^\.$/,
                            'number': /^[1-9][0-9]*/,
                            'numberAsText': /^(one|two|three)/i,
                            'every': /^every/i,
                            'day(s)': /^days?/i,
                            'weekday(s)': /^weekdays?/i,
                            'week(s)': /^weeks?/i,
                            'hour(s)': /^hours?/i,
                            'month(s)': /^months?/i,
                            'year(s)': /^years?/i,
                            'on': /^(on|in)/i,
                            'at': /^(at)/i,
                            'the': /^the/i,
                            'first': /^first/i,
                            'second': /^second/i,
                            'third': /^third/i,
                            'nth': /^([1-9][0-9]*)(\.|th|nd|rd|st)/i,
                            'last': /^last/i,
                            'for': /^for/i,
                            'time(s)': /^times?/i,
                            'until': /^(un)?til/i,
                            'monday': /^mo(n(day)?)?/i,
                            'tuesday': /^tu(e(s(day)?)?)?/i,
                            'wednesday': /^we(d(n(esday)?)?)?/i,
                            'thursday': /^th(u(r(sday)?)?)?/i,
                            'friday': /^fr(i(day)?)?/i,
                            'saturday': /^sa(t(urday)?)?/i,
                            'sunday': /^su(n(day)?)?/i,
                            'january': /^jan(uary)?/i,
                            'february': /^feb(ruary)?/i,
                            'march': /^mar(ch)?/i,
                            'april': /^apr(il)?/i,
                            'may': /^may/i,
                            'june': /^june?/i,
                            'july': /^july?/i,
                            'august': /^aug(ust)?/i,
                            'september': /^sep(t(ember)?)?/i,
                            'october': /^oct(ober)?/i,
                            'november': /^nov(ember)?/i,
                            'december': /^dec(ember)?/i,
                            'comma': /^(,\s*|(and|or)\s*)+/i
                        }
                    };

                    function getText(id) {
                        switch (id.toLowerCase()) {
                            case "every":
                                return ASC.Resources.Master.Resource.MailIcsRRuleEveryLabel;
                            case "until":
                                return ASC.Resources.Master.Resource.MailIcsRRuleUntilLabel;
                            case "for":
                                return ASC.Resources.Master.Resource.MailIcsRRuleForLabel;
                            case "times":
                                return ASC.Resources.Master.Resource.MailIcsRRuleTimesLabel;
                            case "time":
                                return ASC.Resources.Master.Resource.MailIcsRRuleTimeLabel;
                            case "(~ approximate)":
                                return "(~ {0})".format(ASC.Resources.Master.Resource.MailIcsRRuleApproximateLabel);
                            case "hours":
                                return ASC.Resources.Master.Resource.MailIcsRRuleHoursLabel;
                            case "hour":
                                return ASC.Resources.Master.Resource.MailIcsRRuleHourLabel;
                            case "weekdays":
                                return ASC.Resources.Master.Resource.MailIcsRRuleWeekdaysLabel;
                            case "weekday":
                                return ASC.Resources.Master.Resource.MailIcsRRuleWeekdayLabel;
                            case "days":
                                return ASC.Resources.Master.Resource.MailIcsRRuleDaysLabel;
                            case "day":
                                return ASC.Resources.Master.Resource.MailIcsRRuleDayLabel;
                            case "weeks":
                                return ASC.Resources.Master.Resource.MailIcsRRuleWeeksLabel;
                            case "week":
                                return ASC.Resources.Master.Resource.MailIcsRRuleWeekLabel;
                            case "months":
                                return ASC.Resources.Master.Resource.MailIcsRRuleMonthsLabel;
                            case "month":
                                return ASC.Resources.Master.Resource.MailIcsRRuleMonthLabel;
                            case "years":
                                return ASC.Resources.Master.Resource.MailIcsRRuleYearsLabel;
                            case "year":
                                return ASC.Resources.Master.Resource.MailIcsRRuleYearLabel;
                            case "on":
                                return ASC.Resources.Master.Resource.MailIcsRRuleOnLabel;
                            case "on the":
                                return ASC.Resources.Master.Resource.MailIcsRRuleOnTheLabel;
                            case "in":
                                return ASC.Resources.Master.Resource.MailIcsRRuleInLabel;
                            case "at":
                                return ASC.Resources.Master.Resource.MailIcsRRuleAtLabel;
                            case "the":
                                return ASC.Resources.Master.Resource.MailIcsRRuleTheLabel;
                            case "and":
                                return ASC.Resources.Master.Resource.MailIcsRRuleAndLabel;
                            case "or":
                                return ASC.Resources.Master.Resource.MailIcsRRuleOrLabel;
                            case "last":
                                return ASC.Resources.Master.Resource.MailIcsRRuleLastLabel;
                            case "st":
                                return ASC.Resources.Master.Resource.MailIcsRRuleStLabel;
                            case "nd":
                                return ASC.Resources.Master.Resource.MailIcsRRuleNdLabel;
                            case "rd":
                                return ASC.Resources.Master.Resource.MailIcsRRuleRdLabel;
                            case "th":
                                return ASC.Resources.Master.Resource.MailIcsRRuleThLabel;
                            case "rrule error: unable to fully convert this rrule to text":
                                return ASC.Resources.Master.Resource.MailIcsRRuleParseErrorLabel;
                            default:
                                return id;
                        }
                    }

                    return notLocalize ? rule.toText() : rule.toText(getText, language);
                } catch (e) {
                    console.warn(e);
                }

                return "";
            },
            /**
             * Convert ICAL.RRule to Rfc2445 string 
             * @param {ICAL.RRule} rrule
             * @return {String} rruleRfc2445 string;
             */
            ToRRuleRfc2445String: function (rrule) {
                if (typeof (rrule) !== "object" && !rrule.icaltype && rrule.icaltype !== "recur")
                    throw "not supported rrule param";

                var str = "FREQ=" + rrule.freq;
                if (rrule.count) {
                    str += ";COUNT=" + rrule.count;
                }
                if (rrule.interval > 1) {
                    str += ";INTERVAL=" + rrule.interval;
                }
                for (var k in rrule.parts) {
                    if (rrule.parts.hasOwnProperty(k)) {
                        str += ";" + k + "=" + rrule.parts[k];
                    }
                }
                if (rrule.until) {
                    str += ';UNTIL=' + (rrule.until.isDate
                        ? window.moment(rrule.until.toJSDate()).startOf('day')
                        : window.moment(rrule.until.toJSDate()).utc()).format("YYYYMMDD[T]HHmmss[Z]");
                }
                if ('wkst' in rrule && rrule.wkst !== ICAL.Time.DEFAULT_WEEK_START) {
                    str += ';WKST=' + ICAL.Recur.numericDayToIcalDay(rrule.wkst);
                }
                return str;
            }

        };
    })(jQuery);
}

if (typeof ASC.Mail.Message === "undefined") {
    ASC.Mail.Message = function () {
        var docIds = [];

        this.id = 0;
        this.from = "";
        this.to = [];
        this.cc = [];
        this.bcc = [];
        this.subject = "";
        this.body = "";
        this.attachments = [];
        this.mimeReplyToId = "";
        this.importance = false;
        this.tags = [];
        this.fileLinksShareMode = 2; // ASC.Files.Constants.AceStatusEnum.Read;
        this.calendarIcs = "";

        this.HasDocumentsForSave = function () {
            return docIds.length > 0;
        };
        this.AddDocumentsForSave = function (documentIds) {
            jq.merge(docIds, documentIds);
        };
        this.GetDocumentsForSave = function () {
            return docIds;
        };
        this.RemoveDocumentAfterSave = function (documentId) {
            var pos = -1, i, len = docIds.length;
            for (i = 0; i < len; i++) {
                var docId = docIds[i];
                if (docId === documentId) {
                    pos = i;
                    break;
                }
            }
            if (pos > -1)
                docIds.splice(pos, 1);
        };
        this.ToData = function () {

            function convertAddress(addr) {
                if (typeof (addr) === "object" && !(addr instanceof ASC.Mail.Address) && addr.hasOwnProperty("name") && addr.hasOwnProperty("email") && addr.hasOwnProperty("isValid")) {
                    addr = new ASC.Mail.Address(addr.name, addr.email, addr.isValid);
                }

                return (addr instanceof ASC.Mail.Address) ?
                    addr.ToString() :
                    addr;
            }

            function convertAddresses(addresses) {
                return addresses.map(convertAddress);
            }

            var json = JSON.stringify(this);
            var data = JSON.parse(json);

            data.from = convertAddress(data.from);
            data.to = convertAddresses(data.to);
            data.cc = convertAddresses(data.cc);
            data.bcc = convertAddresses(data.bcc);

            return data;
        };
    };

    if (typeof ASC.Mail.Address === "undefined") {
        ASC.Mail.Address = function (name, email, isValid) {
            this.name = name || "";
            this.email = email;
            this.isValid = isValid;

            function quote(text) {
                if (!text)
                    return "";

                var quoted = "\"";
                for (var i = 0, len = text.length; i < len; i++) {
                    var t = text[i];
                    if (t === '\\' || t === '"')
                        quoted += '\\';
                    quoted += t;
                }
                quoted += "\"";
                return quoted;
            }

            this.ToString = function(skipQuotes) {
                var s = !this.name
                    ? this.email
                    : !skipQuotes 
                        ? quote(this.name) + " <" + this.email + ">"
                        : this.name + " <" + this.email + ">";
                return s;
            };

            this.Equals = function(addr) {
                if (typeof (addr) === "object" && (addr instanceof ASC.Mail.Address)) {
                    return this === addr;
                }
                else if (typeof (addr) === "string") {
                    var parsed = ASC.Mail.Utility.ParseAddress(addr);
                    return this.email === parsed;
                }

                return false;
            }

            this.EqualsByEmail = function (addr) {
                if (typeof (addr) === "object" && (addr instanceof ASC.Mail.Address)) {
                    return this.email.toLowerCase() === addr.email.toLowerCase();
                }
                else if (typeof (addr) === "string") {
                    var parsed = ASC.Mail.Utility.ParseAddress(addr);
                    return this.email.toLowerCase() === parsed.email.toLowerCase();
                }

                return false;
            }
        }
    };
}

if (typeof ASC.Mail.Sanitizer === "undefined") {
    ASC.Mail.Sanitizer = (function() {
        var tagWhitelist = {
            'A': true,
            'ABBR': true,
            'ACRONYM': true,
            'ADDRESS': true,
            'APPLET': true,
            'AREA': true,
            'ARTICLE': true,
            'ASIDE': true,
            'AUDIO': true,
            'B': true,
            'BDI': true,
            'BDO': true,
            'BGSOUND': true,
            'BLOCKQUOTE': true,
            'BIG': true,
            'BODY': true,
            'BLINK': true,
            'BR': true,
            'CANVAS': true,
            'CAPTION': true,
            'CENTER': true,
            'CITE': true,
            'CODE': true,
            'COL': true,
            'COLGROUP': true,
            'COMMENT': true,
            'DATALIST': true,
            'DD': true,
            'DEL': true,
            'DETAILS': true,
            'DFN': true,
            'DIR': true,
            'DIV': true,
            'DL': true,
            'DT': true,
            'EM': true,
            'FIGCAPTION': true,
            'FIGURE': true,
            'FONT': true,
            'FOOTER': true,
            'H1': true,
            'H2': true,
            'H3': true,
            'H4': true,
            'H5': true,
            'H6': true,
            'HEAD': true,
            'HEADER': true,
            'HGROUP': true,
            'HR': true,
            'HTML': true,
            'I': true,
            'IMG': true,
            'INS': true,
            'ISINDEX': true,
            'KBD': true,
            'LABEL': true,
            'LEGEND': true,
            'LI': true,
            'MAP': true,
            'MARQUEE': true,
            'MARK': true,
            'META': false,
            'METER': true,
            'NAV': true,
            'NOBR': true,
            'NOEMBED': true,
            'NOFRAMES': true,
            'NOSCRIPT': true,
            'OL': true,
            'OPTGROUP': true,
            'OPTION': true,
            'P': true,
            'PLAINTEXT': true,
            'PRE': true,
            'Q': true,
            'RP': true,
            'RT': true,
            'RUBY': true,
            'S': true,
            'SAMP': true,
            'SECTION': true,
            'SMALL': true,
            'SPAN': true,
            'SOURCE': true,
            'STRIKE': true,
            'STRONG': true,
            'STYLE': false,
            'SUB': true,
            'SUMMARY': true,
            'SUP': true,
            'TABLE': true,
            'TBODY': true,
            'TD': true,
            'TFOOT': true,
            'TH': true,
            'THEAD': true,
            'TIME': true,
            'TITLE': false,
            'TR': true,
            'TT': true,
            'U': true,
            'UL': true,
            'VAR': true,
            'VIDEO': true,
            'WBR': true,
            'XMP': true
        },
        regexps = {
            styleEmbeddedImage: /^data:([\w/]+);(\w+),([^\"^)\s]+)/,
            styleItems: /^([^\s^:]+)\s*:\s*([^;]+);?/g,
            styleUrl: /^.*\b\s*url\s*\(([^)]*)\)/i,
            styleForbiddenValue: /^(?:(expression|eval|javascript|vbscript))\s*(\(|:)/,  // expression(....)
            quotes: /['"]+/g
        };

        function checkOptions(options) {
            options = options || {
                urlProxyHandler: "",
                needProxyHttp: false,
                loadImages: true,
                baseHref: window.location.origin
            }

            if (!options.hasOwnProperty("urlProxyHandler") || options.urlProxyHandler.length === 0) {
                options.urlProxyHandler = "";
                options.needProxyHttp = false;
            }

            if (!options.hasOwnProperty("needProxyHttp")) {
                options.needProxyHttp = false;
            }

            if (!options.hasOwnProperty("loadImages")) {
                options.loadImages = true;
            }

            if (!options.hasOwnProperty("baseHref")) {
                options.baseHref = window.location.origin;
            }

            return options;
        }

        function changeUrlToProxy(url, options) {
            checkOptions(options);

            var newUrl = options.needProxyHttp && url.indexOf("http://") === 0
                                ? "{0}?url={1}".format(options.urlProxyHandler, jq.base64.encode(url))
                                : url;

            return newUrl;
        }

        function parseUrl(url) {
            var a = document.createElement('a');
            a.href = url;
            return a;
        }

        function isWellFormedUrl(url) {
            try {
                var uri = parseUrl(url);
                return uri.protocol === "http:" || uri.protocol === "https:";
            } catch (e) {
                return false;
            }
        }

        function fixBaseLink(foundedUrl, options) {
            if (foundedUrl.indexOf("/") !== 0)
                return foundedUrl;

            return options.baseHref ? options.baseHref + foundedUrl : foundedUrl;
        }

        function fixStyles(node, result, options) {
            checkOptions(options);
            
            var styles = node.style,
                cleanStyle = "",
                needChangeStyle = false;

            var i, len;
            for (i = 0, len = styles.length; i < len; i++) {
                var styleName = (styles[i] || "").trim().toLowerCase();
                var styleValue = (styles[styles[i]] || "").trim().toLowerCase();
                try {
                    if (regexps.styleForbiddenValue.test(styleValue) ||
                        styleName === "position" ||
                        styleName === "margin-left" && styleValue.indexOf("-") !== -1 ||
                        styleName === "margin-top" && styleValue.indexOf("-") !== -1 ||
                        styleName === "margin-rigth" && styleValue.indexOf("-") !== -1 ||
                        styleName === "margin-bottom" && styleValue.indexOf("-") !== -1 ||
                        styleName === "left" ||
                        styleName === "top" ||
                        styleName === "rigth" ||
                        styleName === "bottom") {
                        needChangeStyle = true;
                        continue;
                    }

                    // check if valid url 
                    var urlStyleMatcher = regexps.styleUrl.exec(styleValue);
                    if (!urlStyleMatcher) {
                        cleanStyle = "{0}{1}:{2};".format(cleanStyle, styleName, styleValue);
                        continue;
                    }

                    var urlString = urlStyleMatcher[1].replace(regexps.quotes, "");
                    if (!regexps.styleEmbeddedImage.test(urlString)) {
                        var val = urlString.indexOf("//") === 0
                            ? "http:" + urlString
                            : urlString;

                        if (!isWellFormedUrl(val)) {
                            needChangeStyle = true;
                            continue;
                        }
                    }

                    var newUrl = fixBaseLink(urlString, options);

                    if (options.needProxyHttp) {
                        if (newUrl.length > 0) {
                            newUrl = changeUrlToProxy(newUrl, options);
                        } else {
                            var t = changeUrlToProxy(urlString, options);
                            if (!t.Equals(urlString))
                                newUrl = t;
                        }
                    }

                    if (newUrl.length > 0 && newUrl !== urlString) {
                        styleValue = styleValue.replace(urlString, newUrl);
                        needChangeStyle = true;
                    }

                    if ((styleName === "background-image" ||
                        (styleName === "background" &&
                            styleValue.indexOf("url(") !== -1)) &&
                        !options.loadImages) {
                        styleName = "tl_disabled_" + styleName;
                        result.imagesBlocked = true;
                        needChangeStyle = true;
                    }
                } catch (e) {
                    needChangeStyle = true;
                    continue;
                }

                cleanStyle = "{0}{2};".format(cleanStyle, styleName, styleValue.length === 0 ? "" : ":" + styleValue);
            }

            if (needChangeStyle) {
                console.log("Sanitizer: tag '%s' style changed:\n\told: '%s'\n\n\tnew: '%s'\n", node.tagName, styles.cssText, cleanStyle);
            }

            return needChangeStyle ? cleanStyle : styles.cssText;
        }

        var COMMENT_PSEUDO_COMMENT_OR_LT_BANG = new RegExp(
            '<!--[\\s\\S]*?(?:-->)?' +
            '<!---+>?' + // A comment with no body
            '|<!(?![dD][oO][cC][tT][yY][pP][eE]|\\[CDATA\\[)[^>]*>?' +
            '|<[?][^>]*>?', // A pseudo-comment
            'g');

        function sanitize(html, options) {
            checkOptions(options);

            var result = {
                html: html,
                imagesBlocked: false,
                sanitized: false,
                httpProxied: false
            };

            if (!html)
                return result;

            var iframe = document.createElement("iframe");
            if (iframe["sandbox"] === undefined) {
                var error = "Sorry, but your browser does not support sandboxed iframes. Please upgrade to a modern browser.";
                alert(error);
                throw error;
            }
            iframe["sandbox"] = "allow-same-origin";
            iframe.style.display = "none";
            iframe.src = "data:text/html;base64,PHNwYW4+ZmFrZSBodG1sPC9zcGFuPg==";
            document.body.appendChild(iframe); // necessary so the iframe contains a document

            function insertHtmlToSandbox(htmlStr) {
                iframe.contentDocument.open();
                iframe.contentDocument.write(htmlStr);
                iframe.contentDocument.close();
            }

            try {
                var temp = html
                    .replace(/<\/?script\b.*?>/g, "")
                    .replace(/<\/?link\b.*?>/g, "")
                    .replace(/ on\w+=".*?"/g, "")
                    .replace(COMMENT_PSEUDO_COMMENT_OR_LT_BANG, "");
                insertHtmlToSandbox(temp);

            } catch (e) {
                insertHtmlToSandbox(html);
            } 

            var styleSheets = iframe.contentDocument.styleSheets;
            if (styleSheets.length > 0) {
                for (var i = 0, n = styleSheets.length; i < n; i++) {
                    var rules = styleSheets[i].cssRules || [];
                    for (var j = 0, m = rules.length; j < m; j++) {
                        if (rules[j].hasOwnProperty("media"))
                            continue; // Skips media queries
                        var ruleSelector = "";
                        try {
                            ruleSelector = rules[j].selectorText;
                            if (!ruleSelector)
                                continue;

                            var collection = iframe.contentDocument.querySelectorAll(ruleSelector);
                            for (var k = 0, l = collection.length; k < l; k++) {
                                collection[k].style.cssText += rules[j].style.cssText;
                            }
                        } catch (ex) {
                            console.error("Failed rewrite style's rule (%s): ", ruleSelector, ex);
                        }
                    }
                }
            }

            function makeSanitizedCopy(node) {
                var newNode;
                switch (node.nodeType) {
                case Node.TEXT_NODE:
                    newNode = node.cloneNode(true);
                    break;
                case Node.ELEMENT_NODE:
                    var tagName = node.tagName.toUpperCase();
                    if (tagName.indexOf("O:") === 0) // fix MS tags
                        tagName = tagName.replace("O:", "");

                    if (!tagWhitelist[tagName]) {
                        newNode = document.createDocumentFragment();
                        break;
                    }

                    newNode = iframe.contentDocument.createElement(tagName);
                    var i, n;
                    for (i = 0, n = node.attributes.length; i < n; i++) {
                        var attr = node.attributes[i];
                        try {
                            var newValue;
                            switch (attr.name) {
                            case "background":
                            case "src":
                                newValue = attr.value;
                                if (!regexps.styleEmbeddedImage.test(newValue)) {
                                    newValue = fixBaseLink(attr.value, options);
                                }

                                if (!options.loadImages) {
                                    newNode.setAttribute("tl_disabled_" + attr.name, changeUrlToProxy(newValue, options));
                                    result.imagesBlocked = true;
                                } else {
                                    newNode.setAttribute(attr.name, changeUrlToProxy(newValue, options));
                                }
                                break;
                           case "style":
                                newValue = fixStyles(node, result, options);
                                newNode.setAttribute(attr.name, newValue);
                                break;
                            case "class":
                                // Skips any classes
                                break;
                            default:
                                if (attr.name.indexOf("on") !== 0 && attr.value.length > 0) // skip all javascript events and attributes with empty value 
                                    newNode.setAttribute(attr.name, attr.value);
                                break;
                            }
                        } catch (ex) {
                            console.log("sanitize: ", ex);
                        }
                    }
                    for (i = 0, n = node.childNodes.length; i < n; i++) {
                        try {
                            var subCopy = makeSanitizedCopy(node.childNodes[i]);
                            newNode.appendChild(subCopy, false);
                        } catch (er) {
                            console.log("sanitize: ", er);
                        }
                    }

                    break;
                default:
                    newNode = document.createDocumentFragment();
                    break;
                }
                return newNode;
            };

            var resultElement = makeSanitizedCopy(iframe.contentDocument.body);
            document.body.removeChild(iframe);

            result.html = resultElement.innerHTML;
            result.sanitized = true;

            return result;
        }

        return {
            Sanitize: function(html, options) {
                try {
                    return sanitize(html, options);
                } catch (e) {
                    var error = "Failed ASC.Mail.Sanitizer.Sanitize: {0}".format(e);
                    console.error(error, e);
                    html = error;
                }
                return {
                    html: html,
                    imagesBlocked: false,
                    sanitized: false,
                    httpProxied: false
                };
            }
        };
    })(jQuery);
}