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


if (typeof ASC === 'undefined')
    ASC = {};

jq.ajaxSetup({
    cache: false
});

if (typeof ASC.Api === 'undefined')
    ASC.Api = {};

ASC.Api.TypeConverter = new function() {

    this.ClientTimeToServer = function(clientTime, offset) {

        var date = new Date();
        var minutesOffset = (new Date(clientTime)).getTimezoneOffset();

        if (offset != undefined)
            minutesOffset = (-1) * offset;

        date.setTime(clientTime.getTime() + (minutesOffset * 60 * 1000));

        var m = (date.getMonth() + 1);
        var d = date.getDate();
        var h = date.getHours();
        var min = date.getMinutes();

        var str = date.getFullYear() + '-' + (m > 9 ? m : ('0' + m)) + '-' + (d > 9 ? d : ('0' + d))
            + 'T' + (h > 9 ? h : ('0' + h)) + '-' + (min > 9 ? min : ('0' + min)) + "-00.000Z";

        return str;
    }

    this.ServerTimeToClient = function(serverTime) {

        var str = serverTime.replace(/\..*/gi, '');
        var date = str.split('T')[0].split('-');
        var time = str.split('T')[1].split(':');
        return new Date(date[0], date[1] - 1, date[2], time[0], time[1], time[2]);
    }
}

ASC.CalendarSizeManager = new function() {

    var cache = {
        topPanelHeight: 48,
        fcHeaderHeight: 36,
        fcMonthContentTheadHeight: 18,

        fcMonthContentCellVPaddings: 0,
        fcMonthContentCellVBorders: 2,
        fcMonthContentCellVMargins: 0,
        fcMonthContentCellVSides: 2,

        fcMonthContentCellNumberHeight: 22,
        fcMonthContentCellNumberVPaddings: 3,
        fcMonthContentCellNumberVBorders: 0,
        fcMonthContentCellNumberVMargins: 0,
        fcMonthContentCellNumberVSides: 3,

        fcMonthContentCellContentMaxHeight: 150,
        fcMonthContentCellContentVPaddings: 3,
        fcMonthContentCellContentVBorders: 0,
        fcMonthContentCellContentVMargins: 0,
        fcMonthContentCellContentVSides: 3,

        fcMonthContentCellContentSegmentHeight: 18,
        fcMonthContentCellContentSegmentVPaddings: 0,
        fcMonthContentCellContentSegmentVBorders: 2,
        fcMonthContentCellContentSegmentVMargins: 3,
        fcMonthContentCellContentSegmentVSides: 5,
        fcMonthContentCellContentSegmentHPaddings: 0,
        fcMonthContentCellContentSegmentHBorders: 0,
        fcMonthContentCellContentSegmentHMargins: 2,
        fcMonthContentCellContentSegmentHSides: 2,

        fcMainPadding: 26
    };

    function vpadding(styles) {
        return (parseFloat(styles.paddingTop) || 0) + (parseFloat(styles.paddingBottom) || 0);
    }

    function vborders(styles) {
        return (parseFloat(styles.borderTopWidth) || 0) + (parseFloat(styles.borderBottomWidth) || 0);
    }

    function vmargins(styles) {
        return (parseFloat(styles.marginTop) || 0) + (parseFloat(styles.marginBottom) || 0);
    }

    function hpadding(styles) {
        return (parseFloat(styles.paddingLeft) || 0) + (parseFloat(styles.paddingRight) || 0);
    }

    function hborders(styles) {
        return (parseFloat(styles.borderLeftWidth) || 0) + (parseFloat(styles.borderRightWidth) || 0);
    }

    function hmargins(styles) {
        return (parseFloat(styles.marginLeft) || 0) + (parseFloat(styles.marginRight) || 0);
    }

    function refresh() {
        cache = {};

        cache.topPanelHeight = document.querySelector("#studioPageContent .studio-top-panel").clientHeight;
        cache.fcHeaderHeight = document.querySelector(".fc-header-outer").clientHeight;
        cache.fcMonthContentTheadHeight = document.querySelector(".fc-border-separate thead").clientHeight;

        var cell = document.querySelector(".fc-border-separate tbody td.fc-widget-content.fc-day0");
        var cellStyles = window.getComputedStyle(cell);
        cache.fcMonthContentCellVPaddings = vpadding(cellStyles);
        cache.fcMonthContentCellVBorders = vborders(cellStyles);
        cache.fcMonthContentCellVMargins = vmargins(cellStyles);
        cache.fcMonthContentCellVSides = cache.fcMonthContentCellVPaddings + cache.fcMonthContentCellVBorders + cache.fcMonthContentCellVMargins;

        var cellNumber = cell.querySelector(".fc-day-number");
        cache.fcMonthContentCellNumberHeight = cell.clientHeight;
        var cellNumberStyles = window.getComputedStyle(cellNumber);
        cache.fcMonthContentCellNumberVPaddings = vpadding(cellNumberStyles);
        cache.fcMonthContentCellNumberVBorders = vborders(cellNumberStyles);
        cache.fcMonthContentCellNumberVMargins = vmargins(cellNumberStyles);
        cache.fcMonthContentCellNumberVSides = cache.fcMonthContentCellNumberVPaddings + cache.fcMonthContentCellNumberVBorders + cache.fcMonthContentCellNumberVMargins;

        var cellContent = cell.querySelector(".fc-day-number");
        cache.fcMonthContentCellContentMaxHeight = 150;
        var cellContentStyles = window.getComputedStyle(cellContent);
        cache.fcMonthContentCellContentVPaddings = vpadding(cellContentStyles);
        cache.fcMonthContentCellContentVBorders = vborders(cellContentStyles);
        cache.fcMonthContentCellContentVMargins = vmargins(cellContentStyles);
        cache.fcMonthContentCellContentVSides = cache.fcMonthContentCellContentVPaddings + cache.fcMonthContentCellContentVBorders + cache.fcMonthContentCellContentVMargins;

        var segment = document.querySelector(".fc-event-skin-day");
        cache.fcMonthContentCellContentSegmentHeight = segment.clientHeight;
        var segmentStyles = window.getComputedStyle(segment);
        cache.fcMonthContentCellContentSegmentVPaddings = vpadding(segmentStyles);
        cache.fcMonthContentCellContentSegmentVBorders = vborders(segmentStyles);
        cache.fcMonthContentCellContentSegmentVMargins = vmargins(segmentStyles);
        cache.fcMonthContentCellContentSegmentVSides = cache.fcMonthContentCellContentSegmentVPaddings + cache.fcMonthContentCellContentSegmentVBorders + cache.fcMonthContentCellContentSegmentVMargins;
        cache.fcMonthContentCellContentSegmentHPaddings = hpadding(segmentStyles);
        cache.fcMonthContentCellContentSegmentHBorders = hborders(segmentStyles);
        cache.fcMonthContentCellContentSegmentHMargins = hmargins(segmentStyles);
        cache.fcMonthContentCellContentSegmentHSides = cache.fcMonthContentCellContentSegmentHPaddings + cache.fcMonthContentCellContentSegmentHBorders + cache.fcMonthContentCellContentSegmentHMargins;
    }

    return {
        refresh: refresh,
        cache: cache
    };
};

ASC.CalendarController = new function() {

    var clone = function(o) {
        if (!o || 'object' !== typeof o) {
            return o;
        }
        var c = 'function' === typeof o.pop ? [] : {};
        var p, v;
        for (p in o) {
            if (o.hasOwnProperty(p)) {
                v = o[p];
                if (v && 'object' === typeof v) {
                    c[p] = clone(v);
                }
                else {
                    c[p] = v;
                }
            }
        }
        return c;
    }

    this.ApiUrl = '';
    this.Busy = false;
    this.Search = false;

    var _controller = this;
    var callbackFunc = null;
    var edited = null;

    var sharingManager;

    this.characterString = "@&<>";
    this.characterRegExp = new RegExp("[<>@&]", "gim");

    var AlertError = function(message) {
            alert(message);
    };

    var createProperty = function(jCal) {
        if (!jCal || jCal.length < 4)
            return null;
        
        var prop = new ICAL.Property(jCal[0]);
        
        for (var paramName in jCal[1]) {
            prop.setParameter(paramName, jCal[1][paramName]);
        }

        prop.setValue(jCal[3]);

        return prop;
    };

    var getEventData = function (calendarId, name, description, sDate, eDate, repeatType, alertType, isAllDayLong, timeZone, shareOptions, location, organizer, attendees, status) {

        var startDate = new Date(sDate.getTime());
        var endDate = new Date(sDate.getTime());

        if (eDate && eDate != 'null')
            endDate = new Date(eDate.getTime());

        var comp = new ICAL.Component(['vcalendar', [], []]);
        var vevent = new ICAL.Component('vevent');

        var alarm = new ICAL.Component('valarm');
        if (alertType > 0) {
            var valarmObj = {};
            switch (alertType) {
                case 1: valarmObj.minutes = 5; valarmObj.isNegative = true; break;
                case 2: valarmObj.minutes = 15; valarmObj.isNegative = true; break;
                case 3: valarmObj.minutes = 30; valarmObj.isNegative = true; break;
                case 4: valarmObj.hours = 1; valarmObj.isNegative = true; break;
                case 5: valarmObj.hours = 2; valarmObj.isNegative = true; break;
                case 6: valarmObj.days = 1; valarmObj.isNegative = true; break;
            }
            
            alarm.addPropertyWithValue("TRIGGER", new ICAL.Duration(valarmObj).toICALString());
            alarm.addPropertyWithValue("ACTION", "DISPLAY");
            alarm.addPropertyWithValue("DESCRIPTION", "Reminder");
        }
        
        vevent.addPropertyWithValue("DTSTAMP", ICAL.Time.fromJSDate(new Date(), true).toICALString());
        if (repeatType)
            vevent.addPropertyWithValue("RRULE", repeatType);

        var statusStr = "TENTATIVE";
        if (status == 1) statusStr = "CONFIRMED";
        else if (status == 2) statusStr = "CANCELLED";

        vevent.addPropertyWithValue("STATUS", statusStr);

        if (organizer) {
            var organizerProperty = createProperty(organizer);
            if (organizerProperty)
                vevent.addProperty(organizerProperty);
        }

        if (attendees && attendees.length) {
            for (var i = 0; i < attendees.length; i++) {
                var attendeeProperty = createProperty(attendees[i]);
                if (attendeeProperty)
                    vevent.addProperty(attendeeProperty);
            }
        }

        var event = new ICAL.Event(vevent);

        event.summary = name;
        event.description = description;
        event.location = location || "";

        if (isAllDayLong) {
            endDate.setDate(endDate.getDate() + 1);
        } else {
            startDate = new Date(startDate.getTime() + ((-1) * timeZone.offset * 60 * 1000));
            endDate = new Date(endDate.getTime() + ((-1) * timeZone.offset * 60 * 1000));
        }

        var dtstart = ICAL.Time.fromJSDate(startDate, false);
        dtstart.zone = ICAL.Timezone.utcTimezone;

        var dtend = ICAL.Time.fromJSDate(endDate, false);
        dtend.zone = ICAL.Timezone.utcTimezone;

        dtstart.isDate = isAllDayLong;
        dtend.isDate = isAllDayLong;

        event.startDate = dtstart;
        event.endDate = dtend;

        vevent.addSubcomponent(alarm);
        comp.addSubcomponent(vevent);

        var ics = comp.toString();

        return {
            calendarId: calendarId,
            alertType: alertType,
            sharingOptions: shareOptions,
            ics: ics
        };
    };

    //==================================================
    
    var getTodoData = function (calendarId, name, description, sDate, priority, completed) {

        var startDate = sDate ? sDate.getTime() != new Date(1, 0, 1, 23, 59, 59).getTime() ? new Date(sDate.getTime()) : null : null;
        
        var comp = new ICAL.Component(['vcalendar', [], []]);
        var vtodo = new ICAL.Component('vtodo');
        
        var todo = new ICAL.Event(vtodo);
        
        todo.summary = name;
        todo.description = description ? description : "";
       
        var dtstart = ICAL.Time.fromJSDate(new Date());
        dtstart.zone = ICAL.Timezone.utcTimezone;

        var now = new Date();
        var dtcompleted = ICAL.Time.fromJSDate(new Date(now.getTime() + (now.getTimezoneOffset() * 60 * 1000)));
        dtcompleted.zone = ICAL.Timezone.utcTimezone;
        
        if (startDate != null) {
            startDate = new Date(startDate.getTime() + (startDate.getTimezoneOffset() * 60 * 1000));
            dtstart = ICAL.Time.fromJSDate(startDate, false);
            dtstart.zone = ICAL.Timezone.utcTimezone;
            todo.startDate = dtstart;
        }
        
        if (completed) todo._setProp('completed', dtcompleted);
       
        comp.addSubcomponent(vtodo);

        var ics = comp.toString();
        
        
        return {
            calendarId: calendarId,
            ics: ics
        };
        
    }

    this.init = function (timeZones, editorUrl) {

        jq('.mainPageTable.with-mainPageTableSidePanel .mainPageContent').addClass('calendar');
        var $icon = jq("link[rel*=icon][type^='image']:last");
        if ($icon.attr('href').indexOf('logo_favicon_general.ico') !== -1) {//not default
            $icon.attr('href', $icon.attr('href'));
        }
        if (window.moment) {
            window.moment.locale(ASC.Resources.Master.TwoLetterISOLanguageName);
        }

        jq(document).ajaxStart(function() {
            if (!ASC.CalendarController.Search)
                LoadingBanner.displayLoading();
        });

        jq(document).ajaxStop(function() {
            if (!ASC.CalendarController.Busy)
                LoadingBanner.hideLoading();
        });


        var viewName = 'month';
        var today = new Date();
        var targetEventId = null;

        try{
            ASC.Controls.AnchorController.init();

            var anchorParams = ASC.Controls.AnchorController.getAnchor().split('/');

            viewName = anchorParams[0];
            var date = anchorParams[1];

            today = (date == '' || date == undefined || isNaN(date)) ? new Date() : new Date(parseInt(date));
            if ('Invalid Date' == today || today == 'NaN')
                today = new Date();

            targetEventId = anchorParams[2] || null;
        }
        catch(e)
        {
            today = new Date();
            viewName = 'month';
        }

        ASC.CalendarController.ApiUrl = ASC.Resources.Master.ApiPath + 'calendar';

        var calHeight = jq(window).height() -
            (jq("#studioPageContent .mainContainer").length ? jq("#studioPageContent .mainContainer").outerHeight(true) : 0);

        var defTimeZone = null;

        jq(timeZones).each(function (i, el) {
            if (el.id == ASC.Resources.Master.CurrentTenantTimeZone.Id || el.name == ASC.Resources.Master.CurrentTenantTimeZone.DisplayName) {
                defTimeZone = el;
                return;
            }
        });

        if (defTimeZone == null) {

            var curDate = new Date();
            var clientOffset = (-1) * curDate.getTimezoneOffset();
            var neighbourTimeZones = new Array();
            
            jq(timeZones).each(function (i, el) {
                if (el.offset == clientOffset)
                    neighbourTimeZones.push(el);
            });

            jq(neighbourTimeZones).each(function (i, el) {
                if (curDate.toString().indexOf(el.id) >= 0) {
                    defTimeZone = el;
                    return false;
                }
            });

            if (neighbourTimeZones.length > 0 && defTimeZone == null)
                defTimeZone = neighbourTimeZones[0];
        }

        if (defTimeZone == null) {
            defTimeZone = {id:"UTC", name : "UTC", offset : 0};
        }

        jq("#asc_calendar").fullCalendar({

            defaultView: viewName,
            targetEventId: targetEventId,
            year: today.getFullYear(),
            month: today.getMonth(),
            date: today.getDate(),

            notifications: {
                editorUrl: editorUrl
            },

            selectable: true,
            selectHelper: true,
            editable: true,

            padding: 0,
            height: calHeight,

            onHeightChange: function() {
                var sizeManager = ASC.CalendarSizeManager.cache;
                this.height = window.innerHeight - sizeManager.topPanelHeight - sizeManager.fcHeaderHeight - sizeManager.fcMainPadding;
            },

            characterRegExp: ASC.CalendarController.characterRegExp,

            loadEventSources: ASC.CalendarController.LoadCalendars,
            editCalendar: ASC.CalendarController.DoRequestToCalendar,
            editEvent: ASC.CalendarController.DoRequestToEvent,
            editTodo: ASC.CalendarController.DoRequestToTodo,
            editPermissions: ASC.CalendarController.EditPermissions,
            getPermissions: ASC.CalendarController.GetPermissions,
            removePermissions: ASC.CalendarController.RemovePermissions,
            loadSubscriptions: ASC.CalendarController.LoadSubscriptions,
            manageSubscriptions: ASC.CalendarController.ManageSubscriptions,
            viewChanged: ASC.CalendarController.ViewChangedHandler,
            getiCalUrl: ASC.CalendarController.GetiCalUrl,
            getCaldavUrl: ASC.CalendarController.getCaldavUrl,
            defaultTimeZone: defTimeZone,
            timeZones : timeZones,
            getMonthEvents: ASC.CalendarController.GetEventDays,
            replaceSpecCharacter: ASC.CalendarController.ReplaceSpecCharacter,
            displayInfoPanel: ASC.CalendarController.displayInfoPanel,
            characterString: ASC.CalendarController.characterString
        });
        
        ASC.Mail.Enabled = true;
        ASC.Mail.Accounts = [];
        ASC.Mail.DefaultAccount = null;
        ASC.Mail.Initialized = false;

        if (Teamlab.profile.isVisitor) {
            ASC.Mail.Enabled = false;
            return;
        }

        window.Teamlab.getAccounts({}, {
            success: function(params, res) {
                if (res && res.length) {

                    var enabledAccounts = jq.grep(res, function (item) {
                        return item.enabled && !item.isGroup;
                    });

                    jq.each(enabledAccounts, function (index, item) {
                        if (index == 0) {
                            ASC.Mail.DefaultAccount = { email: item.email, name: item.name || Teamlab.profile.displayName };
                        }

                        if (item.isDefault) {
                            ASC.Mail.DefaultAccount = { email: item.email, name: item.name || Teamlab.profile.displayName };
                            return false;
                        }
                        return true;
                    });
                    
                    ASC.Mail.Accounts = enabledAccounts;

                    var selector = jq("#asc_event .editor .owner select");

                    jq.each(enabledAccounts, function (index, item) {
                        var name = item.name ? Encoder.htmlEncode(item.name) : Teamlab.profile.displayName;
                        var option = jq("<option/>").attr("value", item.email).attr("data-name", Encoder.htmlDecode(name)).html("{0} &lt;{1}&gt;".format(name, item.email));
                        selector.append(option);
                    });

                    selector.tlcombobox();

                    ASC.Mail.Initialized = true;
                }
            },
            error: function (params, err) {
                ASC.Mail.Enabled = false;
                ASC.Mail.Initialized = true;
                console.log(err);
            }
        });
    };

    this.LoadCalendars = function(startDate, endDate, callback) {

        callbackFunc = callback;

        var start = encodeURIComponent(ASC.Api.TypeConverter.ClientTimeToServer(startDate));
        var end = encodeURIComponent(ASC.Api.TypeConverter.ClientTimeToServer(endDate));

        jq.ajax({ type: 'get',
            url: _controller.ApiUrl + "/calendars/" + start + "/" + end + ".json",
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {

                    for (var i = 0; i < data.response.length; i++) {

                        for (var j = 0; j < data.response[i].events.length; j++) {

                            data.response[i].events[j].start = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].events[j].start);
                            data.response[i].events[j].end = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].events[j].end);
                            data.response[i].events[j].repeatRule = ASC.Api.iCal.ParseRRuleFromString(data.response[i].events[j].repeatRule);
                        }
                        if (data.response[i].todos && data.response[i].todos.length > 0) {
                           
                            for (var k = 0; k < data.response[i].todos.length; k++) {

                                var completed;
                                try {
                                    completed = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].todos[k].completed);
                                } catch(e) {
                                    completed = false;
                                }
                                data.response[i].todos[k].completed = completed.getTime() == (new Date(1, 0, 1)).getTime() ? false : true;
                                data.response[i].todos[k].start = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].todos[k].start);
                            }
                        }
                       
                    }

                    data.response.sort(function (a, b) {
                        if (a.isSubscription == b.isSubscription) {
                            return (a.objectId < b.objectId) ? -1 : (a.objectId > b.objectId) ? 1 : 0;
                        } else {
                            return a.isSubscription ? 1 : -1;
                        }
                    });

                    callback({ result: true, eventSources: data.response });
                    return true;
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        })
    }
    this.getCaldavUrl = function (calendarId, callback) {
        callbackFunc = callback;
        jq.ajax({
            type: "get",
            url: _controller.ApiUrl + "/" + calendarId + "/caldavurl.json",
            complete: function (d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true, url: data.response });
                }
                else {
                    callbackFunc({ result: false, url: '' });
                    AlertError(data.error.message);
                }
            }
        });
    }
    this.GetiCalUrl = function(calendarId, callback) {
        callbackFunc = callback;
        jq.ajax({ type: "get",
            url: _controller.ApiUrl + "/" + calendarId + "/icalurl.json",
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true, url: data.response });
                }
                else {
                    callbackFunc({ result: false, url: '' });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.CreateiCalStream = function(url, name, textColor, backgroundColor) {

        jq.ajax({ type: "post",
            url: _controller.ApiUrl + "/calendarUrl.json",
            data: {
                iCalUrl: url,
                name: name,
                textColor: textColor,
                backgroundColor: backgroundColor
            },
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true, source: data.response });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }
    this.CreateCalendar = function (name, description, textColor, backgroundColor, timeZone, eventAlertType, sharingOptions, iCalUrl) {

        jq.ajax({ type: "post",
            url: _controller.ApiUrl + ".json",
            data: { name: name,
                description: description,
                textColor: textColor,
                backgroundColor: backgroundColor,
                timeZone: timeZone,
                alertType: eventAlertType,
                sharingOptions: sharingOptions,
                iCalUrl: iCalUrl ? iCalUrl:null
            },
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true, source: data.response,
                        importUrl: _controller.ApiUrl + "/" + data.response.objectId + "/import"
                    });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.UpdateCalendar = function(calendarId, name, description, textColor, backgroundColor, timeZone, eventAlertType, hideEvents, sharingOptions, iCalUrl) {

        jq.ajax({ type: "put",
            url: _controller.ApiUrl + "/" + calendarId + ".json",
            data: {
                name: name,
                description: description,
                textColor: textColor,
                backgroundColor: backgroundColor,
                timeZone: timeZone,
                alertType: eventAlertType,
                hideEvents: hideEvents,
                sharingOptions: sharingOptions,
                iCalUrl: iCalUrl ? iCalUrl:null
            },
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true, source: data.response,
                        importUrl: _controller.ApiUrl + "/" + data.response.objectId + "/import"
                    });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.RemoveCalendar = function(calendarId) {
        jq.ajax({ type: "delete",
            url: _controller.ApiUrl + "/" + calendarId + ".json",
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true, source: undefined });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.UpdateCalendarView = function(calendarId, name, textColor, backgroundColor, timeZone, eventAlertType, hideEvents) {
        jq.ajax({ type: "put",
            url: _controller.ApiUrl + "/" + calendarId + "/view.json",
            data: {
                calendarId: calendarId,
                name: name,
                textColor: textColor,
                backgroundColor: backgroundColor,
                timeZone: timeZone,
                alertType: eventAlertType,
                hideEvents: hideEvents
            },
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true, source: data.response });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.DoRequestToCalendar = function(params, callback) {
        callbackFunc = callback;

        //permissions
        var sharingOptions = new Array();
        if (params.action === 1 || params.action === 2) {
            if (params.permissions != undefined && params.permissions.data != undefined) {
                for (var i = 0; i < params.permissions.data.items.length; i++) {
                    var item = params.permissions.data.items[i];
                    if (item.selectedAction.id == 'owner')
                        continue;

                    sharingOptions.push({ actionId: item.selectedAction.id, itemId: item.id, isGroup: item.isGroup });
                }
            }

        }
        var timeZone = params.timeZone ? params.timeZone.id : 'UTC';
        //create
        if (params.action === 1 && (params.iCalUrl == undefined || params.iCalUrl == null || params.iCalUrl == ''))
            _controller.CreateCalendar(params.title, params.description, params.textColor, params.backgroundColor, timeZone, params.defaultAlert.type, sharingOptions);
        //create url without sync
        else if (params.action === 1 && params.iCalUrl != undefined && params.iCalUrl != null && params.iCalUrl != '' && params.withoutSync == true)
            _controller.CreateCalendar(params.title, params.description, params.textColor, params.backgroundColor, timeZone, params.defaultAlert.type, sharingOptions, params.iCalUrl);
        //create stream
        else if (params.action === 1 && params.iCalUrl != undefined && params.iCalUrl != null && params.iCalUrl != '')
            _controller.CreateiCalStream(params.iCalUrl, params.title, params.textColor, params.backgroundColor);

        //update url
        else if (params.action === 2 && params.iCalUrl != undefined && params.iCalUrl != null && params.iCalUrl != '')
            _controller.UpdateCalendar(params.objectId, params.title, params.description, params.textColor, params.backgroundColor, timeZone, params.defaultAlert.type, params.isHidden, sharingOptions, params.iCalUrl);
        //update
        else if (params.action === 2)
            _controller.UpdateCalendar(params.objectId, params.title, params.description, params.textColor, params.backgroundColor, timeZone, params.defaultAlert.type, params.isHidden, sharingOptions);

        //delete
        else if (params.action === 3)
            _controller.RemoveCalendar(params.objectId);

        //show - hide events
        else if (params.action === 4)
            _controller.UpdateCalendarView(params.objectId, params.title, params.textColor, params.backgroundColor, timeZone, params.defaultAlert.type, params.isHidden);

        //cancel
        else if (params.action === 5)
            _controller.CancelEditDialog(params.permissions);
    }

    this.GetEventDays = function(params, callback) {
        callbackFunc = callback;

        var start = encodeURIComponent(ASC.Api.TypeConverter.ClientTimeToServer(params.startDate));
        var end = encodeURIComponent(ASC.Api.TypeConverter.ClientTimeToServer(params.endDate));

        jq.ajax({ type: 'get',
            url: _controller.ApiUrl + "/eventdays/" + start + "/" + end + ".json",
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {

                    for (var i = 0; i < data.response.length; i++) {
                        data.response[i] = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i]);
                    }
                    callback({ result: true, days: data.response });
                    return true;
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        })

    }

    this.DoRequestToTodo = function (params, callback) {
        callbackFunc = callback;
        
        switch (params.action) {
            case 1:
                _controller.CreateTodo(
                    params.sourceId,
                    params.title,
                    params.description,
                    params.start,
                    params.priority,
                    params.completed
                );
                break;
            case 2:
                _controller.UpdateTodo(
                    params.sourceId,
                    params.title,
                    params.description,
                    params.start,
                    params.priority,
                    params.completed,
                    params.objectId
                );
                break;
            case 3:
                _controller.DeleteTodo(
                    params.sourceId,
                    params.title,
                    params.description,
                    params.start,
                    params.priority,
                    params.completed,
                    params.objectId
                );
                break;
        default:
        }
    }
    this.DoRequestToEvent = function(params, callback) {
        callbackFunc = callback;
        //permissions
        var sharingOptions = new Array();

        if (params.action === 1 || params.action === 2) {
            if (params.permissions != undefined && params.permissions.data != undefined) {
                for (var i = 0; i < params.permissions.data.items.length; i++) {
                    var item = params.permissions.data.items[i];
                    if (item.selectedAction.id == 'owner')
                        continue;

                    sharingOptions.push({ actionId: item.selectedAction.id, itemId: item.id, isGroup: item.isGroup });
                }
            }
        }

        //create
        if (params.action === 1) {
            edited = params.action;
            _controller.CreateEvent(
                params.newSourceId,
                params.title,
                params.description,
                params.start,
                params.end,
                params.repeatRule.ToiCalString(),
                params.alert.type,
                params.allDay,
                params.newTimeZone,
                sharingOptions,
                params.location,
                params.organizer,
                params.attendees,
                params.status
            );
        }

        //update
        else if (params.action === 2) {
            edited = params.action;
            var tz = params.source.timeZone;
            if (params.source.objectId != params.newSourceId)
                tz = params.newTimeZone;
            _controller.UpdateEvent(
                params.newSourceId,
                params.objectId,
                params.title,
                params.description,
                params.start,
                params.end,
                params.repeatRule.ToiCalString(),
                params.alert.type,
                params.allDay,
                tz,
                sharingOptions,
                params.location,
                params.organizer,
                params.attendees,
                params.status
            );
        }

        //delete
        else if (params.action === 3)
            _controller.RemoveEvent(params.objectId, params.type, params.date);

        //unsubscribe
        else if (params.action === 4) {
            edited = params.action;
            _controller.UnsubscribeEvent(params.objectId);
        }
        //cancel
        else if (params.action === 5) {
            _controller.CancelEditDialog(params.permissions);
        }

    }

    this.CancelEditDialog = function(permissions) {

        if (oldPermissions != null && edited != null && permissions) {
            permissions.data = oldPermissions.data;
            permissions.users = oldPermissions.users;
        }
        oldPermissions = null;
        edited = null;
    }

    //unsubscribe
    this.UnsubscribeEvent = function(eventId) {

        jq.ajax({ type: 'delete',
            url: _controller.ApiUrl + "/events/" + eventId + "/unsubscribe.json",
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                oldPermissions = null;
                if (data.status === 0) {
                    callbackFunc({ result: true });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.RemoveEvent = function(eventId, type, date) {

        //all series
        if (type == 2) {
            jq.ajax({ type: 'delete',
                url: _controller.ApiUrl + "/events/" + eventId + ".json",
                complete: function(d) {
                    var data = jq.evalJSON(d.responseText);
                    if (data.status === 0) {
                        callbackFunc({ result: true, event: undefined });
                    }
                    else {
                        callbackFunc({ result: false });
                        AlertError(data.error.message);
                    }
                }
            });
        }
        else {
            jq.ajax({ type: 'delete',
                url: _controller.ApiUrl + "/events/" + eventId + "/custom.json",
                data: {
                    type: type,
                    date: ASC.Api.TypeConverter.ClientTimeToServer(date, 0)
                },
                complete: function(d) {
                    var data = jq.evalJSON(d.responseText);
                    if (data.status === 0) {

                        for (var i = 0; i < data.response.length; i++) {
                            data.response[i].start = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].start);
                            data.response[i].end = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].end);
                            data.response[i].repeatRule = ASC.Api.iCal.ParseRRuleFromString(data.response[i].repeatRule);
                        }
                        callbackFunc({ result: true, event: data.response });
                    }
                    else {
                        callbackFunc({ result: false });
                        AlertError(data.error.message);
                    }
                }
            });
        }
    }

    this.CreateEvent = function(calendarId, name, description, startDate, endDate, repeatType, alertType, isAllDayLong, timeZone, shareOptions, location, organizer, attendees, status) {

        action = null; //wtf???

        var url = _controller.ApiUrl + "/icsevent.json";
        var postData = getEventData(calendarId, name, description, startDate, endDate, repeatType, alertType, isAllDayLong, timeZone, shareOptions, location, organizer, attendees, status);

        jq.ajax({ type: 'post',
            url: url,
            data: postData,
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {

                    for (var i = 0; i < data.response.length; i++) {
                        data.response[i].start = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].start);
                        data.response[i].end = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].end);
                        data.response[i].repeatRule = ASC.Api.iCal.ParseRRuleFromString(data.response[i].repeatRule);
                    }
                    callbackFunc({ result: true, event: data.response });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.UpdateEvent = function (calendarId, eventId, name, description, startDate, endDate, repeatType, alertType, isAllDayLong, timeZone, shareOptions, location, organizer, attendees, status) {

        action = null; //wtf???

        var url = _controller.ApiUrl + "/icsevent.json";
        var putData = getEventData(calendarId, name, description, startDate, endDate, repeatType, alertType, isAllDayLong, timeZone, shareOptions, location, organizer, attendees, status);
        putData.eventId = eventId;

        jq.ajax({ type: 'put',
            url: url,
            data: putData,
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                oldPermissions = null;
                if (data.status === 0) {
                    for (var i = 0; i < data.response.length; i++) {
                        data.response[i].start = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].start);
                        data.response[i].end = ASC.Api.TypeConverter.ServerTimeToClient(data.response[i].end);
                        data.response[i].repeatRule = ASC.Api.iCal.ParseRRuleFromString(data.response[i].repeatRule);
                    }
                    callbackFunc({ result: true, event: data.response });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }


    this.CreateTodo = function (calendarId, name, description, startDate, endDate, priority) {
        
        var url = _controller.ApiUrl + "/icstodo.json";

        startDate = startDate ? new Date(startDate) : startDate;
    
        var postData = getTodoData(calendarId, name, description, startDate, priority, false);
        
        jq.ajax({ type: 'post',
            url: url,
            data: postData,
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
               
                if (data.status === 0) {
                    callbackFunc({ result: true, todo: data.response });
                }
                else {
                    callbackFunc({ result: false });
                }
            }
        });
    }
    
    this.UpdateTodo = function (calendarId, name, description, startDate, priority, completed, todoId) {

        var url = _controller.ApiUrl + "/icstodo.json";

        var putData = getTodoData(calendarId, name, description, startDate, priority, completed);
        putData.todoId = todoId;

        jq.ajax({
            type: 'put',
            url: url,
            data: putData,
            complete: function (d) {
                var data = jq.evalJSON(d.responseText);

                if (data.status === 0) {
                    callbackFunc({ result: true, todo: data.response });
                }
                else {
                    callbackFunc({ result: false });
                }
            }
        });
    }

    this.DeleteTodo = function(calendarId, name, description, startDate, priority, completed, todoId) {
        jq.ajax({
            type: 'delete',
            url: _controller.ApiUrl + "/todos/" + todoId + ".json",
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true, event: undefined });
                } else {
                    callbackFunc({ result: false });
                }
            }
        });
    };
    //permissions
    var SetAccessForCalendar = function(sharingData) {
        var permissions = new Array();
        for (var i = 0; i < sharingData.items.length; i++) {
            var item = sharingData.items[i];
            if (item.selectedAction.id == 'owner')
                continue;

            permissions.push({ objectId: item.id, name: item.name });
        }
        callbackFunc({ result: true, permissions: { users: permissions, data: sharingData} });
    }

    var oldPermissions = null;
    this.RemovePermissions = function(params, callback) {

        edited = params.userId;
        if (params.permissions != undefined && params.permissions.data != undefined) {

            if (oldPermissions == null)
                oldPermissions = clone(params.permissions);

            for (var i = 0; i < params.permissions.data.items.length; i++) {
                var item = params.permissions.data.items[i];
                if (item.selectedAction.id == 'owner')
                    continue;

                if (item.id == params.userId) {
                    params.permissions.data.items.splice(i, 1);

                    for (var k = 0; k < params.permissions.users.length; k++) {
                        if (params.permissions.users[k].objectId == params.userId) {
                            params.permissions.users.splice(k, 1);
                            break;
                        }
                    }

                    callback({ result: true, permissions: params.permissions });
                    return;
                }
            }
        }

        callback({ result: false });
    }

    var wrapperShowDialog = function (param) {
        if (param.permissions.data == undefined) {
            jq.ajax({
                type: 'get',
                url: _controller.ApiUrl + "/sharing.json",
                complete: function (d) {
                    var data = jq.evalJSON(d.responseText);
                    if (data.status === 0) {
                        sharingManager.OnSave = SetAccessForCalendar;
                        sharingManager.UpdateSharingData(data.response);
                        sharingManager.ShowDialog();
                        //shareUserSelector.HideUser(ASC.Resources.Master.ApiResponsesMyProfile.response.id, true);
                        return;
                    }
                    else {
                        callbackFunc({ result: false });
                        AlertError(data.error.message);
                    }
                }
            });
        }
        else {
            sharingManager.OnSave = SetAccessForCalendar;
            sharingManager.UpdateSharingData(param.permissions.data);
            sharingManager.ShowDialog();
            //shareUserSelector.HideUser(ASC.Resources.Master.ApiResponsesMyProfile.response.id, true);
            return;
        }
    }

    this.EditPermissions = function (param, callback) {
        if (!sharingManager)
            sharingManager = new SharingSettingsManager(undefined, null);

        callbackFunc = callback;
        oldPermissions = param.permissions;
        //oldPermissions = clone(param.permissions);

        setTimeout(function () {
            wrapperShowDialog(param);
        }, 0);
    }
    
    this.GetPermissions = function (param, callback) {

        if (param.permissions.data == undefined) {

            jq.ajax({
                type: 'get',
                url: _controller.ApiUrl + "/sharing.json",
                complete: function (d) {
                    var data = jq.evalJSON(d.responseText);
                    if (data.status === 0) {
                        var users = new Array();

                        for (var i = 0; i < data.response.items.length; i++) {
                            var item = data.response.items[i];
                            if (item.selectedAction.id == 'owner')
                                continue;
                            users.push({ objectId: item.id, name: item.name });
                        }

                        callback({ result: data.status === 0, permissions: { users: users, data: data.response } });
                        return;
                    } else {
                        callbackFunc({ result: false });
                        if (data.statusCode == 403) window.location = window.location.origin;
                        else AlertError(data.error.message);
                    }
                }
            });
            
        } else {
            callback({ result: true, permissions: param.permissions });
            return;
        }
        
    }

    // ViewChangedHandler
    this.ViewChangedHandler = function(currentViewName, currentDate) {
        if (currentViewName != '') {
            try {
                ASC.Controls.AnchorController.safemove(currentViewName + '/' + currentDate.getTime());
            }
            catch (e) { }
        }
    }


    //subscriptions
    this.LoadSubscriptions = function(callback) {

        callbackFunc = callback;
        jq.ajax({ type: 'get',
            url: _controller.ApiUrl + "/subscriptions.json",
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0)
                    callbackFunc({ result: true, subscriptions: data.response });
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.ManageSubscriptions = function(params, callback) {
        callbackFunc = callback;
        var states = new Array();
        for (var i = 0; i < params.length; i++) {
            states.push({ id: params[i].objectId, isAccepted: (params[i].action == 1) });
        }

        jq.ajax({ type: 'put',
            url: _controller.ApiUrl + "/subscriptions/manage.json",
            data: { states: states },
            complete: function(d) {
                var data = jq.evalJSON(d.responseText);
                if (data.status === 0) {
                    callbackFunc({ result: true });
                }
                else {
                    callbackFunc({ result: false });
                    AlertError(data.error.message);
                }
            }
        });
    }

    this.ReplaceSpecCharacter = function (str) {
        return (str || "").trim().replace(ASC.CalendarController.characterRegExp, "_");
    };

    this.displayInfoPanel = function (str, warn) {
        if (str === "" || typeof str === "undefined") {
            return;
        }
        if (warn === true) {
            toastr.error(str);
        } else {
            toastr.success(str);
        }
    };
}