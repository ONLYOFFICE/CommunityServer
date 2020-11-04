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
if (typeof ASC.Mail.Utility === "undefined") {
    ASC.Mail.Utility = (function () {
        var resources = ASC.Resources.Master.Resource;
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
            function getDefaultAccount(accs) {
                var def = jq.grep(accs, function (a) { return a.is_default === true }),
                    acc = (def.length > 0 ? def[0] : accs[0]);

                return new ASC.Mail.Address(
                    acc.name,
                    acc.email,
                    true);
            }

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

        function saveTemplate(params) {
            var d = jq.Deferred();

            if (params.skipSave) {
                d.resolve(params, { messageUrl: null, saveSkipped: true });
                return d.promise();
            }

            var message = params.message;

            window.Teamlab.saveMailTemplate(
                params,
                message,
                {
                    success: d.resolve,
                    error: d.reject
                });

            return d.promise();
        }

        function afterTemplateSave(params, savedMessage) {
            var d = jq.Deferred();

            if (params.skipSave) {
                d.resolve(params, { messageUrl: null, saveSkipped: true });
                return d.promise();
            }

            var message = params.message;
            message.id = savedMessage.id;

            d.resolve(params, { messageUrl: "/addons/mail/#templateitem/" + message.id });

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
            function addDoc(fileId) {
                var dfd = jq.Deferred();

                var data = {
                    fileId: fileId,
                    version: ""
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

            var d = jq.Deferred();

            if (params.skipSave) {
                d.resolve(params, saveResult);
                return d.promise();
            }

            var message = params.message;

            if (message.HasDocumentsForSave()) {
                var documentIds = message.GetDocumentsForSave();

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

            var socket = ASC.SocketIO && !ASC.SocketIO.disabled() ? ASC.SocketIO.Factory.counters : null;

            if (!params.skipSend && (!socket || !socket.connected())) {
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
                        break;
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
                            if (ASC.Mail.Utility.IsEqualEmail(a.getFirstValue().toLowerCase().replace("mailto:", ""), params.attendeesEmails[i].toLowerCase())) {
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
                mapUrl: iCalInfo.event.location ? getMapUrl(iCalInfo.event.location) : null,
                description: iCalInfo.event.description
            };

            info.dateEvent = ASC.Mail.Utility.ToCalendarDateString(dtStart, dtEnd, dateStartAllDay, dateEndAllDay);
            if (iCalInfo.recurrence)
                info.rruleText = ASC.Mail.Utility.ToCalendarRRuleString(iCalInfo.recurrence, dtStart);

            switch (params.method) {
                case "REQUEST":
                    info.action = params.isUpdate ?
                        resources.MailIcsUpdateDescription :
                        resources.MailIcsRequestDescription.format(
                        (iCalInfo.organizerAddress.name || iCalInfo.organizerAddress.email) ||
                        (iCalInfo.currentAttendeeAddress.name || iCalInfo.currentAttendeeAddress.email));
                    break;
                case "REPLY":
                    var res;
                    switch (params.replyDecision) {
                        case "ACCEPTED":
                            res = resources.MailIcsReplyYesDescription;
                            break;
                        case "TENTATIVE":
                            res = resources.MailIcsReplyMaybeDescription;
                            break;
                        case "DECLINED":
                            res = resources.MailIcsReplyNoDescription;
                            break;
                        default:
                            throw "Unsupported attendee partstart";
                    }
                    info.action = res.format(iCalInfo.currentAttendeeAddress.name ||
                        iCalInfo.currentAttendeeAddress.email);
                    break;
                case "CANCEL":
                    info.action = resources.MailIcsCancelDescription;
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
                    resources.MailIcsUpdateSubject.format(iCalInfo.event.summary) :
                    resources.MailIcsRequestSubject.format(iCalInfo.event.summary);

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
                        subject = resources.MailIcsReplyYesDescription;
                        break;
                    case "TENTATIVE":
                        subject = resources.MailIcsReplyMaybeDescription;
                        break;
                    case "DECLINED":
                        subject = resources.MailIcsReplyNoDescription;
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
                message.subject = resources.MailIcsReplySubject.format(subject);
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
            message.subject = resources.MailIcsCancelSubject.format(iCalInfo.event.summary);
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
                    if (!s)
                        return;
                    var parsed = emailAddresses.parseOneAddress(s);
                    if (parsed) {
                        var isValid = true;
                        if (parsed.domain.indexOf(".") === -1 || !/(^((?!-)[a-zA-Z0-9-]{1,63}\.)+[a-zA-Z]{2,63}\.?$)/.test(parsed.domain)) {
                            isValid = false;
                            parsedObjs.errors.push({ message: "Incorrect domain", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }

                        if (parsed.domain.indexOf('[') === 0 && parsed.domain.indexOf(']') === parsed.domain.length - 1) {
                            parsedObjs.errors.push({ message: "Domains as ip adress are not suppoted", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }

                        if (!/^[\x00-\x7F]+$/.test(punycode.toUnicode(parsed.domain))) {
                            isValid = false;
                            parsedObjs.errors.push({ message: "Punycode domains are not suppoted", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }

                        if (!/^[\x00-\x7F]+$/.test(parsed.local) || !/^([a-zA-Z0-9]+)([_\-\.\+][a-zA-Z0-9]+)*$/.test(parsed.local)) {
                            isValid = false;
                            parsedObjs.errors.push({ message: "Incorrect localpart", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }

                        if (/\s+/.test(parsed.local) || parsed.local !== parsed.parts.local.tokens) {
                            isValid = false;
                            parsedObjs.errors.push({ message: "Incorrect, localpart contains spaces", type: parseErrorTypes.IncorrectEmail, errorItem: s });
                        }

                        if (/\s+/.test(parsed.domain) || parsed.domain !== parsed.parts.domain.tokens) {
                            isValid = false;
                            parsedObjs.errors.push({ message: "Incorrect, domain contains spaces", type: parseErrorTypes.IncorrectEmail, errorItem: s });
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
            IsValidEmail: function (email, options) {
                options = options || {
                    nameExistance: false
                }

                if (!options.hasOwnProperty('nameExistance')) {
                    options.nameExistance = false;
                }

                var parsed = ASC.Mail.Utility.ParseAddress(email);

                if (!options.nameExistance && parsed.name)
                    return false;
                    

                return parsed.isValid;
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

                    if (message.body.length > ASC.Resources.Master.MailMaximumMessageBodySize) {
                        throw "Message body exceeded limit";
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
            * Save message to Mail Templates
            * @param {ASC.Mail.Message} message
            * @param {Object} params (Example: { skipAccountsCheck: true, skipSave: true });
            * @return {Object} result with messageUrl;
            */
            SaveMessageInTemplates: function (message, params) {
                var d = jq.Deferred();

                params = params || { skipAccountsCheck: true };

                try {

                    if (!(message instanceof ASC.Mail.Message)) {
                        throw "Unsupported message format";
                    }

                    if (message.body.length > ASC.Resources.Master.MailMaximumMessageBodySize) {
                        throw "Message body exceeded limit";
                    }

                    if (!params.hasOwnProperty("skipAccountsCheck") || !message.from)
                        params.skipAccountsCheck = false;

                    params.message = message;

                    getAccounts(params)
                        .then(checkAccounts, d.reject)
                        .then(saveTemplate, d.reject)
                        .then(afterTemplateSave, d.reject)
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

                var d = jq.Deferred();

                params = params || { skipAccountsCheck: true };

                try {

                    if (!(message instanceof ASC.Mail.Message)) {
                        throw "Unsupported message format";
                    }

                    if (!params.hasOwnProperty("skipAccountsCheck") || !message.from)
                        params.skipAccountsCheck = false;

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
                        toastr.error(resources.MailSendMessageError);

                        if (module === "mail" && window.mailAlerts) { // mail hook
                            window.mailAlerts.check(lastSentMessageId > 0 ? { showFailureOnlyMessageId: lastSentMessageId } : {});
                        }
                        break;
                    case 0:
                        toastr.success(resources.MailSentMessageText);
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
                        toastr.success(resources.MailSentIcalRequestText);
                        break;
                    case 2:
                        toastr.success(resources.MailSentIcalResponseText);
                        break;
                    case 3:
                        toastr.success(resources.MailSentIcalCancelText);
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
                    strTz = "(UTC{0})".format(dtStart.format("Z"));

                if (dtStart.isSame(dtEnd, 'day')) {
                    if (allDayStart)
                        dateEvent = "{0}, {1}".format(strStart, resources.MailIcsCalendarAllDayEventLabel);
                    else
                        dateEvent = "{0}, {1} - {2} {3}".format(strStart, strStartTime, strEndTime, strTz);
                } else {
                    if (allDayStart && allDayEnd) {
                        dateEvent = "{0}, {1} - {2}, {1}".format(strStart, resources.MailIcsCalendarAllDayEventLabel, strEnd);
                    } else if (allDayStart && !allDayEnd) {
                        dateEvent = "{0}, {1} - {2}, {3} {4}".format(strStart, resources.MailIcsCalendarAllDayEventLabel, strEnd, strEndTime, strTz);
                    } else if (!allDayStart && allDayEnd) {
                        dateEvent = "{0}, {1} {2} - {3}, {4}".format(strStart, strStartTime, strTz, strEnd, resources.MailIcsCalendarAllDayEventLabel);
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
            ToCalendarRRuleString: function (rruleRfc2445, dtStart, notLocalize) {
                function getText(id) {
                    switch (id.toLowerCase()) {
                        case "every":
                            return resources.MailIcsRRuleEveryLabel;
                        case "until":
                            return resources.MailIcsRRuleUntilLabel;
                        case "for":
                            return resources.MailIcsRRuleForLabel;
                        case "times":
                            return resources.MailIcsRRuleTimesLabel;
                        case "time":
                            return resources.MailIcsRRuleTimeLabel;
                        case "(~ approximate)":
                            return "(~ {0})".format(resources.MailIcsRRuleApproximateLabel);
                        case "hours":
                            return resources.MailIcsRRuleHoursLabel;
                        case "hour":
                            return resources.MailIcsRRuleHourLabel;
                        case "weekdays":
                            return resources.MailIcsRRuleWeekdaysLabel;
                        case "weekday":
                            return resources.MailIcsRRuleWeekdayLabel;
                        case "days":
                            return resources.MailIcsRRuleDaysLabel;
                        case "day":
                            return resources.MailIcsRRuleDayLabel;
                        case "weeks":
                            return resources.MailIcsRRuleWeeksLabel;
                        case "week":
                            return resources.MailIcsRRuleWeekLabel;
                        case "months":
                            return resources.MailIcsRRuleMonthsLabel;
                        case "month":
                            return resources.MailIcsRRuleMonthLabel;
                        case "years":
                            return resources.MailIcsRRuleYearsLabel;
                        case "year":
                            return resources.MailIcsRRuleYearLabel;
                        case "on":
                            return resources.MailIcsRRuleOnLabel;
                        case "on the":
                            return resources.MailIcsRRuleOnTheLabel;
                        case "in":
                            return resources.MailIcsRRuleInLabel;
                        case "at":
                            return resources.MailIcsRRuleAtLabel;
                        case "the":
                            return resources.MailIcsRRuleTheLabel;
                        case "and":
                            return resources.MailIcsRRuleAndLabel;
                        case "or":
                            return resources.MailIcsRRuleOrLabel;
                        case "last":
                            return resources.MailIcsRRuleLastLabel;
                        case "st":
                            return resources.MailIcsRRuleStLabel;
                        case "nd":
                            return resources.MailIcsRRuleNdLabel;
                        case "rd":
                            return resources.MailIcsRRuleRdLabel;
                        case "th":
                            return resources.MailIcsRRuleThLabel;
                        case "rrule error: unable to fully convert this rrule to text":
                            return resources.MailIcsRRuleParseErrorLabel;
                        default:
                            return id;
                    }
                }

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
            },

            GetDraftUrl: function(fileIds) {
                var url = "/addons/mail/#compose";

                if (!fileIds) return url;

                if (!(fileIds instanceof Array)) {
                    fileIds = [fileIds];
                }

                url += "?files={0}".format(encodeURIComponent(JSON.stringify(fileIds)));

                return url;
            }
        };
    })();
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
        this.requestReceipt = false;
        this.requestRead = false;

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
                if (typeof (addr) === "object" &&
                    !(addr instanceof ASC.Mail.Address) &&
                    addr.hasOwnProperty("name") &&
                    addr.hasOwnProperty("email") &&
                    addr.hasOwnProperty("isValid"))
                {
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

        // specify the regex to detect external content
        var regex = /(url\("?)(?!data:)/gim;

        /**
         *  Take CSS property-value pairs and proxy URLs in values,
         *  then add the styles to an array of property-value pairs
         */
        function addStyles(output, styles, options) {
            for (var prop = styles.length - 1; prop >= 0; prop--) {
                if (styles[styles[prop]] && options.needProxyHttp) {
                    var url = styles[styles[prop]].replace(regex, '$1' + options.urlProxyHandler);
                    styles[styles[prop]] = url;
                }
                if (styles[styles[prop]]) {
                    output.push(styles[prop] + ':' + styles[styles[prop]] + ';');
                }
            }
        }

        /**
         * Take CSS rules and analyze them, proxy URLs via addStyles(),
         * then create matching CSS text for later application to the DOM
         */
        function addCssRules(output, cssRules, options) {
            for (var index = cssRules.length - 1; index >= 0; index--) {
                var rule = cssRules[index];
                // check for rules with selector
                if (rule.type === 1 && rule.selectorText) {
                    output.push(rule.selectorText + '{');
                    if (rule.style) {
                        addStyles(output, rule.style, options);
                    }
                    output.push('}');
                    // check for @media rules
                } else if (rule.type === rule.MEDIA_RULE) {
                    output.push('@media ' + rule.media.mediaText + '{');
                    addCssRules(output, rule.cssRules, options);
                    output.push('}');
                    // check for @font-face rules
                } else if (rule.type === rule.FONT_FACE_RULE) {
                    output.push('@font-face {');
                    if (rule.style) {
                        addStyles(output, rule.style, options);
                    }
                    output.push('}');
                    // check for @keyframes rules
                } else if (rule.type === rule.KEYFRAMES_RULE) {
                    output.push('@keyframes ' + rule.name + '{');
                    for (var i = rule.cssRules.length - 1; i >= 0; i--) {
                        var frame = rule.cssRules[i];
                        if (frame.type === 8 && frame.keyText) {
                            output.push(frame.keyText + '{');
                            if (frame.style) {
                                addStyles(output, frame.style, options);
                            }
                            output.push('}');
                        }
                    }
                    output.push('}');
                }
            }
        }

        function isForbiddenStyle(styleName, styleValue) {
            styleName = (styleName || "").trim().toLowerCase();
            styleValue = (styleValue || "").trim().toLowerCase();

            switch (styleName) {
                case "position":
                case "left":
                case "top":
                case "right":
                case "bottom":
                    return true;
                case "margin-left":
                case "margin-top":
                case "margin-rigth":
                case "margin-bottom":
                    return styleValue.indexOf("-") !== -1;
                default:
                    return false;
            }
        }

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

            // Proxy a URL in case it's not a Data URI
            function proxyAttribute(url) {
                if (!options.needProxyHttp)
                    return url;

                if (/^data:image\//.test(url)) {
                    return url;
                } else {
                    result.httpProxied = true;
                    return options.urlProxyHandler + encodeURIComponent(url);
                }
            }

            // Add a hook to enforce proxy for leaky CSS rules
            window.DOMPurify.addHook("uponSanitizeElement", function (node, data) {
                if (data.tagName === "style") {
                    var output = [];
                    if (node && node.sheet && node.sheet.cssRules) {
                        addCssRules(output, node.sheet.cssRules, options);
                    }
                    node.textContent = output.join("\n");
                }
                else if (node.className === "MsoNormal") {
                    node.setAttribute("style", "margin: 0cm; margin-bottom: .0001pt");
                }
            });

            window.DOMPurify.addHook("afterSanitizeAttributes", function (node) {
                // set all elements owning target to target=_blank
                if (node.hasOwnProperty("target")) {
                    node.setAttribute("target", "_blank");
                }

                // Check all style attribute values and proxy them
                if (node.hasAttribute("style")) {
                    var styles = node.style;
                    var output = [];
                    for (var prop = styles.length - 1; prop >= 0; prop--) {
                        var styleName = styles[prop],
                            styleValue = node.style[styleName];

                        // we re-write each property-value pair to remove invalid CSS
                        if (styleValue && regex.test(styleValue)) {
                            if (options.needProxyHttp) {
                                var url = styleValue.replace(regex, '$1' + options.urlProxyHandler);
                                result.httpProxied = true;
                                node.style[styleName] = url;
                            }
                        }

                        if (isForbiddenStyle(styleName, styleValue)) {
                            continue;
                        }

                        if (!options.loadImages &&
                        (styleName === "background-image" || styleName === "background")) {
                            styleName = "tl_disabled_" + styleName;
                            result.imagesBlocked = true;
                        }

                        output.push(styleName + ':' + styleValue + ';');
                    }
                    // re-add styles in case any are left
                    if (output.length) {
                        node.setAttribute('style', output.join(""));
                    } else {
                        node.removeAttribute('style');
                    }
                }
                var newSrcVal;
                if (node.hasAttribute("src")) {
                    if (!options.loadImages) {
                        newSrcVal = proxyAttribute(node.getAttribute("src"));

                        if (newSrcVal) {
                            node.removeAttribute("src");
                            node.setAttribute("tl_disabled_src", newSrcVal);
                            result.imagesBlocked = true;

                        }
                    }
                }

                if (node.hasAttribute("background")) {
                    if (!options.loadImages) {
                        newSrcVal = proxyAttribute(node.getAttribute("background"));

                        if (newSrcVal) {
                            node.removeAttribute("background");
                            node.setAttribute("tl_disabled_background", newSrcVal);
                            result.imagesBlocked = true;
                        }
                    }
                }
            });

            var clean = window.DOMPurify.sanitize(html,
            {
                ALLOWED_URI_REGEXP:
                    /^(?:(?:(?:f|ht)tps?|mailto|tel|callto|cid|blob|xmpp|data):|[^a-z]|[a-z+.\-]+(?:[^a-z+.\-:]|$))/i,// eslint-disable-line no-useless-escape
                FORBID_TAGS: ['style', 'input', 'form', 'title', 'iframe', 'meta'],
                FORBID_ATTR: ['srcset', 'action']
            });

            result.html = clean;
            result.sanitized = true;

            window.DOMPurify.removeHook("afterSanitizeAttributes");
            window.DOMPurify.removeHook("uponSanitizeElement");

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
    })();
}