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


window.mailCalendar = (function ($) {

    function composeFromCalendar(email, name) {
        if (!email) return;
        window.popup.hide();
        var address = new ASC.Mail.Address(name, email);
        messagePage.setToEmailAddresses([address.ToString()]);
        messagePage.composeTo();
    }

    function fixCalendarDescription(text) {
        if (!text)
            return text;

        var options = {
            defaultProtocol: 'http',
            events: null,
            format: function (value) {
                return value.length > 100 ? value.substring(0, 100) + "..." : value;
            },
            formatHref: function (href, type) {
                return type === 'email' ? "javascript:mailCalendar.composeFromCalendar(\"{0}\");".format(href.toLowerCase().replace("mailto:", "")) : href;
            },
            linkAttributes: null,
            linkClass: "link underline",
            nl2br: false,
            tagName: 'a',
            target: function (href, type) {
                return type === 'url' ? '_blank' : null;
            }
        };

        var newDescription = window.linkifyStr(text, options).trim();

        newDescription = newDescription
            .replace(/\\r\\n/g, "<br>")
            .replace(/\r\n/g, "<br>")
            .replace(/\\n/g, "<br>")
            .replace(/\n/g, "<br>")
            .replace(/\\r/g, "<br>")
            .replace(/\r/g, "<br>");

        return newDescription;
    }

    function parseIcs(data, icsFileName, calendarUrl, srcEmail, fromEmail, calendarEventInfo) {
        var jCalData = window.ICAL.parse(data);
        var comp = new window.ICAL.Component(jCalData);
        if (comp.name !== "vcalendar")
            throw "Unsupported ical type (Only VCALENDAR)";

        var vtimezones = comp.getAllSubcomponents("vtimezone");
        //Add all timezones in iCalendar object to TimezonService
        //if they are not already registered.
        vtimezones.forEach(function (vtimezone) {

            if (!(window.ICAL.TimezoneService.has(
                vtimezone.getFirstPropertyValue("tzid")))) {
                window.ICAL.TimezoneService.register(vtimezone);
            }
        });

        var vevent = comp.getFirstSubcomponent("vevent");
        if (!vevent)
            throw "VEVENT not found";

        var event = new window.ICAL.Event(vevent);

        if (!event.organizer && comp.getFirstPropertyValue("method") === "REQUEST")
            comp.updatePropertyWithValue("method", "REPLY");

        if (!event.summary)
            event.summary = window.MailScriptResource.NoSubject;

        if (!event.startDate ||
            !event.endDate ||
            (!event.organizer && comp.getFirstPropertyValue("method") !== "REPLY") ||
            !event.attendees)
            throw "Some property is undefined";

        var srcAccount;

        if (srcEmail)
            srcAccount = accountsManager.getAccountByAddress(srcEmail);

        var icalInfo = {
            method: comp.getFirstPropertyValue("method"),
            summary: event.summary,
            location: event.location,
            eventUid: event.uid,
            orgName: "",
            orgEmail: "",
            description: fixCalendarDescription(event.description),
            mapUrl: TMMail.getMapUrl(event.location),
            showButtons: false,
            needAgenda: false,
            showDetail: "mailCalendar.showCalendarInfo(\"{0}\", \"{1}\")".format(calendarUrl, icsFileName),
            attendees: jq.map(event.attendees, function(p) {
                return {
                    name: p.getParameter("cn"),
                    email: p.getFirstValue().toLowerCase().replace("mailto:", "")
                };
            }),
            ics: data,
            eventSequence: event.sequence,
            eventSummaryChanged: false,
            eventDateEventChanged: false,
            eventLocationChanged: false,
            eventOrganizerChanged: false,
            eventRRuleEventChanged: false,
            fromOrganizer: false,
            fromAttendee: false,
            alienEvent: true,
            timeZone: calendarEventInfo && calendarEventInfo.timeZone ? calendarEventInfo.timeZone : {
                offset: ASC.Resources.Master.CurrentTenantTimeZone.UtcOffset
            },
            calendarName: calendarEventInfo && calendarEventInfo.calendarName ? calendarEventInfo.calendarName : null,
            calendarId: calendarEventInfo && calendarEventInfo.calendarId ? calendarEventInfo.calendarId : -1,
            eventId: calendarEventInfo && calendarEventInfo.eventId ? calendarEventInfo.eventId : null
        };

        var rrule = vevent.getFirstPropertyValue("rrule");

        if (rrule !== null) {
            icalInfo.recurrence = ASC.Mail.Utility.ToRRuleRfc2445String(rrule);
        }

        if (event.organizer) {
            icalInfo.orgName = Encoder.htmlDecode(vevent.getFirstProperty("organizer").getParameter("cn") || "");
            icalInfo.orgEmail = event.organizer.toLowerCase().replace("mailto:", "");

            if (srcAccount) {
                icalInfo.fromOrganizer = ASC.Mail.Utility.IsEqualEmail(srcAccount.email, icalInfo.orgEmail);
            }
        }

        window.moment.locale(ASC.Resources.Master.TwoLetterISOLanguageName);
        var start = window.moment(new Date(event.startDate.toUnixTime() * 1000));
        var end = window.moment(new Date(event.endDate.toUnixTime() * 1000));

        icalInfo.dtStartAllDay = event.startDate.isDate;
        icalInfo.dtStart = icalInfo.timeZone && !icalInfo.dtStartAllDay ? start.utcOffset(icalInfo.timeZone.offset) : start;
        icalInfo.dtEndAllDay = event.endDate.isDate;
        icalInfo.dtEnd = icalInfo.timeZone && !icalInfo.dtEndAllDay ? end.utcOffset(icalInfo.timeZone.offset) : end;

        if (icalInfo.dtStartAllDay &&
            icalInfo.dtEndAllDay &&
            start.isBefore(end, 'day')) {
            // Correct end date
            end = end.clone().startOf('day').add(-1, 'day');
            icalInfo.dtEnd = end;
        }

        icalInfo.agendaDate = icalInfo.dtStart.format("ddd, DD MMM YYYY");
        icalInfo.timeDay = icalInfo.dtStart.clone().startOf('day').valueOf();

        if (srcEmail) {
            var curAttendee = jq.grep(event.attendees, function(a) {
                return ASC.Mail.Utility.IsEqualEmail(a.getFirstValue().toLowerCase().replace("mailto:", ""), srcEmail);
            });

            if (curAttendee.length > 0) {
                icalInfo.curAttendee = curAttendee[0];
                if (srcAccount) {
                    icalInfo.fromAttendee = ASC.Mail.Utility.IsEqualEmail(srcAccount.email, icalInfo.curAttendee.getFirstValue().toLowerCase().replace("mailto:", ""));
                }
            } else {
                curAttendee = jq.grep(event.attendees, function(a) {
                    return ASC.Mail.Utility.IsEqualEmail(a.getFirstValue().toLowerCase().replace("mailto:", ""), fromEmail);
                });

                if (curAttendee.length > 0) {
                    icalInfo.curAttendee = curAttendee[0];
                }
            }
        }

        if (icalInfo.curAttendee) {
            icalInfo.curAttendeeAddress = new ASC.Mail.Address(Encoder.htmlDecode(icalInfo.curAttendee.getParameter("cn") || ""),
                icalInfo.curAttendee.getFirstValue().toLowerCase().replace("mailto:", ""));
        }

        icalInfo.alienEvent = !icalInfo.fromOrganizer && !icalInfo.fromAttendee;

        if (icalInfo.method === "REQUEST") {
            icalInfo.action = !icalInfo.alienEvent && !icalInfo.fromOrganizer && icalInfo.fromAttendee
                    ? ASC.Resources.Master.Resource.MailIcsRequestDescription.format(icalInfo.orgName || icalInfo.orgEmail)
                    : undefined;

        } else if (icalInfo.method === "REPLY") {
            if (!icalInfo.alienEvent && icalInfo.curAttendee) {
                var action;
                switch (icalInfo.curAttendee.getParameter("partstat")) {
                case "ACCEPTED":
                    action = icalInfo.fromAttendee
                        ? ASC.Resources.Master.Resource.MailIcsYouReplyYesDescription
                        : ASC.Resources.Master.Resource.MailIcsReplyYesDescription;
                    break;
                case "TENTATIVE":
                    action = icalInfo.fromAttendee
                        ? ASC.Resources.Master.Resource.MailIcsYouReplyMaybeDescription
                        : ASC.Resources.Master.Resource.MailIcsReplyMaybeDescription;
                    break;
                case "DECLINED":
                    action = icalInfo.fromAttendee
                        ? ASC.Resources.Master.Resource.MailIcsYouReplyNoDescription
                        : ASC.Resources.Master.Resource.MailIcsReplyNoDescription;
                    break;
                default:
                    throw "Unsupported attendee partstart";
                }

                icalInfo.action = action.format(icalInfo.curAttendeeAddress.name || icalInfo.curAttendeeAddress.email);
            }
            icalInfo.attendees = [];

        } else if (icalInfo.method === "CANCEL") {
            icalInfo.action = !icalInfo.alienEvent ? ASC.Resources.Master.Resource.MailIcsCancelDescription : undefined;
        }

        return icalInfo;
    }

    function loadCalendarInfo(calendarUrl, calendarFilename, srcEmail, fromEmail) {
        var d = jq.Deferred();

        jq.get(calendarUrl)
            .then(function(data) {
                try {
                    var icalInfo = parseIcs(data, calendarFilename, calendarUrl, srcEmail, fromEmail);
                    d.resolve(icalInfo);
                } catch (err) {
                    d.reject(this, err);
                }
            }, d.reject)
            .then(d.resolve, d.reject);

        return d.promise();
    }

    function loadCalendarAgenda(message, icalInfo) {
        var dt = icalInfo.dtStart.clone().startOf('day'),
            dtStart = dt.clone().format("YYYY-MM-DD[T]HH-mm-ss"),
            dtEnd = dt.clone().hours(23).minutes(59).seconds(59).milliseconds(0).format("YYYY-MM-DD[T]HH-mm-ss");

        Teamlab.getCalendars({},
            dtStart,
            dtEnd,
            {
                success: function (p, calendars) {

                    var allCalendarsEvents = jq.map(calendars, function (c) {
                        return c.events;
                    });

                    jq.each(allCalendarsEvents, function(i, e) {
                        e.start = icalInfo.timeZone && !e.allDay
                            ? window.moment(e.start).utcOffset(icalInfo.timeZone.offset)
                            : window.moment(e.start);
                    });

                    if (allCalendarsEvents.length > 1) {
                        var prevDayEvents = jq.grep(allCalendarsEvents, function (e) {
                            return e.start.isBefore(icalInfo.dtStart, 'day');
                        });

                        var allDayEvents = jq.grep(allCalendarsEvents, function (e) {
                            return e.allDay && e.start.isSame(icalInfo.dtStart, 'day');
                        });

                        var currentDayEvents = jq.grep(allCalendarsEvents, function (e) {
                            return !e.allDay && e.start.isSame(icalInfo.dtStart, 'day');
                        });

                        currentDayEvents.sort(function (a, b) {
                            if (a.start.isBefore(b.start))
                                return -1;
                            if (a.start.isAfter(b.start))
                                return 1;
                            return 0;
                        });

                        allCalendarsEvents = prevDayEvents.concat(allDayEvents).concat(currentDayEvents);

                    } else if (!allCalendarsEvents.length) {
                        allCalendarsEvents.push({
                            start: icalInfo.dtStart,
                            title: icalInfo.summary,
                            allDay: icalInfo.dtStartAllDay,
                            uniqueId: icalInfo.eventUid
                        });
                    }

                    var events = [],
                        monthTs = icalInfo.dtStart.clone().startOf("month").valueOf(),
                        curPos = 0;

                    for (var i = 0, len = allCalendarsEvents.length; i < len; i++) {
                        var e = allCalendarsEvents[i],
                            isCurrent = e.uniqueId === icalInfo.eventUid;

                        if (isCurrent) {
                            if ((len === 1 || i === 0) && !e.allDay) {
                                events.push({
                                    isNotifyItem: true,
                                    title: MailScriptResource.CalendarAgendaNoEventsBeforeLabel
                                });
                            }
                            curPos = i;
                        }

                        events.push({
                            time: e.start.isSame(icalInfo.dtStart, 'day')
                                ? (e.allDay  
                                    ? window.MailScriptResource.CalendarEventAllDayLabel
                                    : e.start.format("LT"))
                                : window.MailScriptResource.CalendarEventPrevDayLabel,
                            date: e.start,
                            title: e.title,
                            isCurrent: isCurrent,
                            isNotifyItem: false,
                            href: e.objectId ? "/addons/calendar/#month/{0}/{1}".format(monthTs, e.objectId) : null
                        });

                        if (isCurrent) {
                            if (len === 1 || i + 1 === len && !e.allDay) {
                                events.push({
                                    isNotifyItem: true,
                                    title: MailScriptResource.CalendarAgendaNoEventsAfterLabel
                                });
                            }
                        }
                    }

                    var html = jq.tmpl('calendarEventsAgendaTmpl', { events: events });
                    var $messageBody = $('#itemContainer .itemWrapper .head[message_id=' + message.id + ']');
                    $messageBody.find(".calendarView .agenda_loading").replaceWith(html);
                    $messageBody.find('.calendarView .agenda_row .title').dotdotdot({ wrap: 'letter', height: 25, watch: 'window' });

                    var offsetTopCur,
                        agendaRows = $messageBody.find(".calendarView .agenda_card .agenda_row"),
                        agendaCnt = $messageBody.find(".calendarView .agenda_card");
                    //Scroll to current event
                    if (agendaRows.length > 0 && agendaCnt && agendaCnt.get(0).scrollHeight > agendaCnt.height()) {
                        if (curPos !== 0 && curPos !== allCalendarsEvents.length - 1) {
                            if (curPos === 1)
                                curPos = 0;
                            else if (curPos >= 2)
                                curPos -= 2;
                            offsetTopCur = $(agendaRows[curPos]).position().top;
                        } else {
                            offsetTopCur = $(agendaRows[curPos]).position().top;
                        }
                        var offsetTopBase = agendaCnt.position().top;
                        $messageBody.find(".calendarView .agenda_card").scrollTop(offsetTopCur - offsetTopBase);
                    }
                },
                error: function (p, e) {
                    var $messageBody = $('#itemContainer .itemWrapper .head[message_id=' + message.id + ']');
                    $messageBody.find(".calendarView .agenda_loading").hide();
                    $messageBody.find(".calendarView .cal_agenda").hide('slide');
                    console.error(e);
                    toastr.error(window.MailScriptResource.ErrorCalendarIsUnreachable);
                },
                max_request_attempts: 1
            });
    }

    function getCalendarEventByUid(params, calendarEventUid) {
        var d = jq.Deferred();

        if (ASC.Mail.Constants.CALENDAR_AVAILABLE) {
            Teamlab.getCalendarEventByUid(params,
                calendarEventUid,
                {
                    success: d.resolve,
                    error: d.reject,
                    max_request_attempts: 1
                });
        } else {
            d.resolve(params, null);
        }

        return d.promise();
    }

    function loadCalendarBody(message) {
        if (message.attachments.length > 0 && message.htmlBody) {
            var calendars = $.grep(message.attachments, function (attach) {
                return TMMail.canViewAsCalendar(attach.fileName);
            });

            if (calendars.length > 0) {
                var headCalendar = $('#itemContainer .itemWrapper .head[message_id=' + message.id + '] .row.calendar');

                function showCalendarError(e, error) {
                    console.error(error);
                    headCalendar.find(".loader-fx").hide();
                    toastr.error(window.MailScriptResource.ErrorUnsupportedFileFormat);
                    headCalendar.find(".error").show();
                }

                headCalendar.removeClass("hidden");

                var calendarUrl = window.TMMail.getAttachmentDownloadUrl(calendars[0].fileId);
                var calendarFilename = calendars[0].fileName;

                function showCalendarHeader(icalInfo) {
                    icalInfo.messageId = message.id;
                    var html = $.tmpl('calendarHeaderTmpl', icalInfo);
                    var $messageBody = $('#itemContainer .itemWrapper .head[message_id=' + icalInfo.messageId + ']');
                    $messageBody.data('icalInfo', icalInfo);

                    headCalendar.find(".loader-fx").hide();
                    headCalendar.find("label").show();
                    $messageBody.find(".calendarView").remove();
                    $messageBody.find(".row.calendar").remove();
                    $messageBody.append(html);

                    var calendarView = $messageBody.find(".calendarView");

                    calendarView.find('.goToWriter').off("click").click(function () {
                        var $this = $(this);
                        return mailCalendar.composeFromCalendar($this.attr("title"), $this.attr("name"));
                    });

                    calendarView.slideToggle('slow');
                    calendarView.find('.cal_summary').dotdotdot({ wrap: 'letter', height: 26, watch: 'window' });

                    calendarView.find('.card_when .card_value').dotdotdot({
                        wrap: 'letter', height: 25, watch: 'window'});

                    var mapEl = $.tmpl("mapLinkTmpl", icalInfo);
                    calendarView.find('.card_location .card_value').dotdotdot({ wrap: 'letter', height: 25, watch: 'window', after: mapEl });

                    if (icalInfo.showButtons) {
                        html.find(".cal_buttons_row").show();

                        var idCheckedButton = undefined;

                        if (icalInfo.curAttendee) {
                            switch (icalInfo.curAttendee.getParameter("partstat")) {
                                case "ACCEPTED":
                                    idCheckedButton = "#request-accept";
                                    break;
                                case "TENTATIVE":
                                    idCheckedButton = "#request-maybe";
                                    break;
                                case "DECLINED":
                                    idCheckedButton = "#request-decline";
                                    break;
                                default:
                                    break;
                            }
                        }

                        var buttons = html.find(".cal_buttons");
                        if (buttons.length > 0) {
                            buttons.controlgroup();

                            buttons.find("> input:radio").checkboxradio("option", "icon", false);

                            function checkButton(idButton) {
                                if (idCheckedButton) {
                                    var checkedButton = buttons.find(idButton);
                                    checkedButton.prop('checked', true);
                                    buttons.controlgroup("refresh");
                                } else {
                                    buttons.find("> input:radio").prop('checked', false);
                                    buttons.controlgroup("refresh");
                                }

                                return null;
                            }

                            if (idCheckedButton) {
                                checkButton(idCheckedButton);
                            }

                            function doReply(decision) {
                                window.LoadingBanner.displayMailLoading();

                                html.find(".cal_buttons > input:radio").button({ disabled: true });

                                function loadMessageIcal() {
                                    var $messageBody = $('#itemContainer .itemWrapper .head[message_id=' + message.id + ']');
                                     return $messageBody.data('icalInfo');
                                }

                                var icalInfo;
                                if (!ASC.Mail.Constants.CALENDAR_AVAILABLE) {
                                    icalInfo = loadMessageIcal();
                                }

                                (icalInfo
                                        ? ASC.Mail.Utility.SendCalendarReplyIcs(message.calendarUid, icalInfo.ics, message.address, decision)
                                        : ASC.Mail.Utility.SendCalendarReply(-1, message.calendarUid, message.address, decision))
                                    .done(function() {
                                        idCheckedButton = buttons.find(":checked").prop("id");
                                    })
                                    .fail(function(p, e) {
                                        if (e === "Empty ical") {
                                            if (!icalInfo) {
                                                icalInfo = loadMessageIcal();
                                            }

                                            Teamlab.importCalendarEventIcs({}, -1, icalInfo.ics, {
                                                success: function() {
                                                    doReply(decision);
                                                },
                                                error: function(p, e) {
                                                    toastr.error(e);
                                                },
                                                max_request_attempts: 1
                                            });
                                        } else {
                                            switch (e) {
                                            case "Account is disabled.":
                                                toastr.error(window.MailResource.MessageFromWarning);
                                                var $messageBody = $('#itemContainer .itemWrapper .head[message_id=' + message.id + ']');
                                                $messageBody.find(".from-disabled-warning").show();
                                                break;
                                            default:
                                                toastr.error(window.MailScriptResource.ErrorNotification);
                                            }

                                            console.error("SendCalendarReply Error", arguments);
                                            checkButton(idCheckedButton);
                                        }
                                    })
                                    .always(function() {
                                        window.LoadingBanner.hideLoading();
                                        html.find(".cal_buttons > input:radio").button({ disabled: false });
                                    });
                            }

                            buttons.find("#request-accept").change(function () {
                                if ($(this).is(':checked')) {
                                    doReply("ACCEPTED");
                                }
                            });
                            buttons.find("#request-maybe").change(function () {
                                if ($(this).is(':checked')) {
                                    doReply("TENTATIVE");
                                }
                            });
                            buttons.find("#request-decline").change(function () {
                                if ($(this).is(':checked')) {
                                    doReply("DECLINED");
                                }
                            });
                        }
                    }

                    if (icalInfo.needAgenda) {
                        loadCalendarAgenda(message, icalInfo);
                    }
                }

                var pGetCalEvent = getCalendarEventByUid({}, message.calendarUid)
                    .then(function (p, eventInfo) {
                        var d = jq.Deferred();
                        if (eventInfo && eventInfo.eventUid === message.calendarUid && eventInfo.mergedIcs) {
                            try {
                                var icalInfo = parseIcs(eventInfo.mergedIcs, calendarFilename, calendarUrl, message.address, message.from, eventInfo);
                                d.resolve(icalInfo);
                            } catch (err) {
                                d.reject(this, err);
                            }
                        } else {
                            d.resolve(null);
                        }

                        return d.promise();
                    });

                var pGetMailEvent = loadCalendarInfo(calendarUrl, calendarFilename, message.address, message.from);

                $.when(pGetCalEvent, pGetMailEvent)
                    .done(function(calEventInfo, mailEventInfo) {
                        var iCal = mailEventInfo;
                        var now;
                        if (calEventInfo) {
                            switch (mailEventInfo.method) {
                                case "REQUEST":
                                    switch (calEventInfo.method) {
                                        case "REQUEST":
                                        case "REPLY":
                                            now = calEventInfo.timeZone ? window.moment().utcOffset(calEventInfo.timeZone.offset) : window.moment();

                                            var eventSummaryChanged = calEventInfo.summary !== mailEventInfo.summary,
                                                eventDateEventChanged = !calEventInfo.dtStart.isSame(
                                                        calEventInfo.timeZone && !calEventInfo.dtStartAllDay
                                                        ? mailEventInfo.dtStart.clone().utcOffset(calEventInfo.timeZone.offset)
                                                        : mailEventInfo.dtStart)
                                                    || !calEventInfo.dtEnd.isSame(
                                                        calEventInfo.timeZone && !calEventInfo.dtEndAllDay
                                                        ? mailEventInfo.dtEnd.clone().utcOffset(calEventInfo.timeZone.offset)
                                                        : mailEventInfo.dtEnd),
                                                eventLocationChanged = calEventInfo.location !== mailEventInfo.location,
                                                eventOrganizerChanged = calEventInfo.orgEmail !== mailEventInfo.orgEmail || calEventInfo.orgName !== mailEventInfo.orgName,
                                                eventRRuleEventChanged = calEventInfo.recurrence !== mailEventInfo.recurrence,
                                                hasChanges = eventSummaryChanged || eventDateEventChanged || eventLocationChanged || eventOrganizerChanged || eventRRuleEventChanged;

                                            if (calEventInfo.eventSequence !== mailEventInfo.eventSequence || hasChanges) {
                                                iCal.eventDisplayInfo = ASC.Resources.Master.Resource.MailIcsUpdateDescription;
                                                iCal.eventDisplayInfoClass = "info-region";
                                                iCal.eventSummaryChanged = eventSummaryChanged;
                                                iCal.eventDateEventChanged = eventDateEventChanged;
                                                iCal.eventLocationChanged = eventLocationChanged;
                                                iCal.eventOrganizerChanged = eventOrganizerChanged;
                                                iCal.eventRRuleEventChanged = eventRRuleEventChanged;
                                                iCal.showButtons = false;
                                                iCal.needAgenda = false;
                                            }
                                            else if (!iCal.recurrence &&
                                                (iCal.dtEndAllDay && iCal.dtEnd.isBefore(now, 'day')) ||
                                                (!iCal.dtEndAllDay && iCal.dtEnd.isBefore(now))) {
                                                iCal.eventDisplayInfo = ASC.Resources.Master.Resource.MailIcsFinishDescription;
                                                iCal.eventDisplayInfoClass = "success-region";
                                                iCal.showButtons = false;
                                                iCal.needAgenda = false;
                                            } else {
                                                iCal = parseInt(MailFilter.getFolder()) === TMMail.sysfolders.inbox.id
                                                    ? calEventInfo
                                                    : mailEventInfo;
                                                iCal.eventDisplayInfo = iCal.fromOrganizer
                                                        ? ASC.Resources.Master.Resource.MailIcsYouSentRequestDescription
                                                        : ASC.Resources.Master.Resource.MailIcsRequestDescription.format(
                                                            iCal.orgName || iCal.orgEmail);
                                                iCal.eventDisplayInfoClass = "info-region";

                                                if (!iCal.alienEvent) {
                                                    iCal.showButtons = 
                                                        !iCal.fromOrganizer &&
                                                        iCal.fromAttendee &&
                                                        (!!iCal.recurrence ||
                                                        (iCal.dtEndAllDay && iCal.dtEnd.isSameOrAfter(now, 'day')) ||
                                                        (!iCal.dtEndAllDay && iCal.dtEnd.isSameOrAfter(now)));

                                                    iCal.needAgenda = ASC.Mail.Constants.CALENDAR_AVAILABLE && iCal.showButtons;
                                                }
                                            }
                                            break;
                                        case "CANCEL":
                                            iCal.eventDisplayInfo = ASC.Resources.Master.Resource.MailIcsCancelDescription;
                                            iCal.eventDisplayInfoClass = "error-region";
                                            iCal.showButtons = false;
                                            iCal.needAgenda = false;
                                            break;
                                    }
                                    iCal.timeZone = calEventInfo.timeZone;
                                    iCal.calendarName = calEventInfo.calendarName;
                                    iCal.calendarId = calEventInfo.calendarId;
                                    iCal.eventId = calEventInfo.eventId;
                                    break;
                                case "REPLY":
                                    iCal = calEventInfo;

                                    if (!mailEventInfo.curAttendee)
                                        break;

                                    iCal.eventDisplayInfo = mailEventInfo.action;
                                    switch (mailEventInfo.curAttendee.getParameter("partstat")) {
                                        case "ACCEPTED":
                                            iCal.eventDisplayInfoClass = "success-region";
                                            break;
                                        case "TENTATIVE":
                                            iCal.eventDisplayInfoClass = "info-region";
                                            break;
                                        case "DECLINED":
                                            iCal.eventDisplayInfoClass = "error-region";
                                            break;
                                    }
                                    iCal.showButtons = false;
                                    iCal.needAgenda = false;
                                    break;
                                case "CANCEL":
                                    iCal = calEventInfo;
                                    iCal.eventDisplayInfo = ASC.Resources.Master.Resource.MailIcsCancelDescription;
                                    iCal.eventDisplayInfoClass = "error-region";
                                    break;
                            }
                            
                        } else {
                            switch (iCal.method) {
                                case "REQUEST":
                                    now = window.moment();
                                    if (!iCal.recurrence &&
                                        (iCal.dtEndAllDay && iCal.dtEnd.isBefore(now, 'day')) ||
                                        (!iCal.dtEndAllDay && iCal.dtEnd.isBefore(now))) {
                                        iCal.eventDisplayInfo = ASC.Resources.Master.Resource.MailIcsFinishDescription;
                                        iCal.eventDisplayInfoClass = "success-region";
                                    } else {
                                        iCal.eventDisplayInfo = iCal.fromOrganizer
                                                        ? ASC.Resources.Master.Resource.MailIcsYouSentRequestDescription
                                                        : ASC.Resources.Master.Resource.MailIcsRequestDescription.format(
                                                            iCal.orgName || iCal.orgEmail);
                                        iCal.eventDisplayInfoClass = "info-region";

                                        if (!iCal.alienEvent) {
                                            iCal.showButtons =
                                                !iCal.fromOrganizer &&
                                                iCal.fromAttendee &&
                                                (!!iCal.recurrence ||
                                                (iCal.dtEndAllDay && iCal.dtEnd.isSameOrAfter(now, 'day')) ||
                                                (!iCal.dtEndAllDay && iCal.dtEnd.isSameOrAfter(now)));

                                            iCal.needAgenda = false;
                                        }
                                    }
                                    break;
                                case "REPLY":
                                    if (!iCal.curAttendee)
                                        break;

                                    iCal.eventDisplayInfo = iCal.action;
                                    switch (iCal.curAttendee.getParameter("partstat")) {
                                        case "ACCEPTED":
                                            iCal.eventDisplayInfoClass = "success-region";
                                            break;
                                        case "TENTATIVE":
                                            iCal.eventDisplayInfoClass = "info-region";
                                            break;
                                        case "DECLINED":
                                            iCal.eventDisplayInfoClass = "error-region";
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case "CANCEL":
                                    iCal.eventDisplayInfo = iCal.action;
                                    iCal.eventDisplayInfoClass = "error-region";
                                    break;
                            }
                        }

                        if (iCal.alienEvent) {
                            iCal.eventDisplayInfo = undefined;
                            iCal.eventDisplayInfoClass = undefined;
                        }

                        iCal.dateEvent = ASC.Mail.Utility.ToCalendarDateString(iCal.dtStart, iCal.dtEnd, iCal.dtStartAllDay, iCal.dtEndAllDay);
                        if (iCal.recurrence)
                            iCal.rruleText = ASC.Mail.Utility.ToCalendarRRuleString(iCal.recurrence, iCal.dtStart);

                        if (iCal.eventId) {
                            var monthTs = iCal.dtStart.clone().startOf("month").valueOf();
                            iCal.eventUrl = "/addons/calendar/#month/{0}/{1}".format(monthTs, iCal.eventId);
                        }

                        return showCalendarHeader(iCal);
                    })
                    .fail(showCalendarError);
            }
        }
    }

    function showCalendarPopup(calendarUrl, filename) {
        loadCalendarInfo(calendarUrl, filename)
            .then(function(icalInfo) {
                    icalInfo.dateEvent = ASC.Mail.Utility.ToCalendarDateString(icalInfo.dtStart, icalInfo.dtEnd, icalInfo.dtStartAllDay, icalInfo.dtEndAllDay);
                    if (icalInfo.recurrence)
                        icalInfo.rruleText = ASC.Mail.Utility.ToCalendarRRuleString(icalInfo.recurrence, icalInfo.dtStart);

                    var html = $.tmpl('calendarPopupTmpl', icalInfo);
                    window.popup.addBig(filename, html);
                    var popup = $(".calendarPopView.popup");
                    popup.find('.cal_summary').dotdotdot({
                        ellipsis: '... ',
                        wrap: 'word',
                        fallbackToLetter: true,
                        height: 26
                    });

                    var mapEl = $.tmpl("mapLinkTmpl", icalInfo);
                    popup.find('.card_location .card_value').dotdotdot({ wrap: 'word', height: 18, fallbackToLetter: true, after: mapEl });

                    popup.find('.goToWriter').off("click").click(function() {
                        var $this = $(this);
                        return mailCalendar.composeFromCalendar($this.attr("title"), $this.attr("name"));
                    });
                },
                function(err) {
                    console.error(err);
                    toastr.error(window.MailScriptResource.ErrorUnsupportedFileFormat);
                });
    }

    function saveAttachedIcsToCalendar(fileId, filename) {
        var calendarUrl = window.TMMail.getAttachmentDownloadUrl(fileId);

        loadCalendarInfo(calendarUrl, filename)
            .then(function(icalInfo) {
                Teamlab.importCalendarEventIcs({}, -1, icalInfo.ics, {
                        success: function() {
                            toastr.success(window.MailScriptResource.SaveAttachmentToCalendarSuccess.replace('%file_name%', filename));
                        },
                        error: function(p, e) {
                            toastr.error(e);
                        },
                        max_request_attempts: 1
                    });
                },
                function(err) {
                    console.error(err);
                    window.toastr.error(window.MailScriptResource.SaveAttachmentsToDocumentsFailure);
                });
    }

    return {
        composeFromCalendar: composeFromCalendar,
        loadAttachedCalendar: loadCalendarBody,
        showCalendarInfo: showCalendarPopup,
        exportAttachmentToCalendar: saveAttachedIcsToCalendar
    };

})(jQuery);