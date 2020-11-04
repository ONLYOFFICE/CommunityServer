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


/**
 * @preserve
 * FullCalendar v1.5.1
 * http://arshaw.com/fullcalendar/
 *
 * Use fullcalendar.css for basic styling.
 * For event drag & drop, requires jQuery UI draggable.
 * For event resizing, requires jQuery UI resizable.
 *
 * Copyright (c) 2011 Adam Shaw
 * Dual licensed under the MIT and GPL licenses, located in
 * MIT-LICENSE.txt and GPL-LICENSE.txt respectively.
 *
 * Date: Sat Apr 9 14:09:51 2011 -0700
 *
 */

(function($, undefined) {

// such structure of code is used only for code folding in NetBeans IDE
var defaults = function defaultsModule() {return {
		
		// display
		defaultView: "month",
		aspectRatio: 1.35,
		minHeight:   500,
		header: {
		    left:   "prev,next,title",
			center: "",
			right:  ""
		},
		weekends: true,
		weekMode: "fixed",

		// editing
		editable: true,
		//disableDragging: false,
		//disableResizing: false,

		allDayDefault: true,
		ignoreTimezone: true,

		// event ajax
		lazyFetching: true,
		startParam: 'start',
		endParam: 'end',

		// time formats
		titleFormat: {
			month: "MMMM yyyy",
			week:  "MMM d",
			day:   "ddd, MMM d, yyyy"
		},
		columnFormat: {
			month: "dddd",
			week:  "'<div style=\"white-space: nowrap;\"><div class=\"fc-head-col-name\">'dddd'</div>' '<span class=\"number fc-head-col-number\">'dd'</span></div>'",
			day:   "'<div class=\"center\">'dddd'&nbsp;&nbsp;'d'</div>'"
		},
		timeFormat: { // for event elements
			"":     "h(:mm)t", // default
			agenda: "h:mm{ - h:mm}"
		},

		allDaySlot: true,
		allDayText: "all-day",
		firstHour: 6,
		slotMinutes: 30,
		defaultEventMinutes: 1,
		axisFormat: "h(:mm)tt",
		dragOpacity: {
			agenda: .5
		},
		minTime: 0,
		maxTime: 24,

		// locale
		isRTL: false,
		firstDay: 0,
		monthNames: ['January','February','March','April','May','June','July','August','September','October','November','December'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
		dayNames: ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],
		dayNamesShort: ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'],
		buttonText: {
			prev: '<span class="triangle-w">&nbsp;</span>',
			next: '<span class="triangle-e">&nbsp;</span>',
			prevYear: '&nbsp;&lt;&lt;&nbsp;',
			nextYear: '&nbsp;&gt;&gt;&nbsp;',
			today: 'Today',
			month: 'Month',
			week: 'Week',
			day: 'Day',
			//list: 'Period',
			list: 'List',
			todo: "Todo"
			
		},
		defaultTimeZone: {name: "UTC:00", id: "UTC", offset: 0},

		// jquery-ui theming
		theme: false,
		buttonIcons: {
			prev: 'circle-triangle-w',
			next: 'circle-triangle-e'
		},

		selectable: true,
		selectHelper: true,
		unselectAuto: true,

		dropAccept: '*',

		eventTextColor:       "#000",
		eventBackgroundColor: "#87CEFA",
		eventBorderColor:     "#297FB4",
		eventBg2BorderRatio:  0.8,
		eventMaxTitleLength:  250,

		todayLabel:           "Today",
		moreEventsLabel:      "events",
		addNewEventLabel:     "Add event",
		addNewLabel:          "Create",

		modes: {
			calendarViewLabel: "Calendar view",
			listViewLabel:     "List view"
		},		

		sharedList: {
			title:    "Shared with:",
			moreLink: "Show %d more users",
			addLink:  "Add users for sharing",
			shortListLength: 5
		},

		categories: {
			width: 201,
			// list items
			itemBullet:      "&#9632;",
			inactiveColor:   "#aaa",
			defaultTitle:    "New calendar",
			
			// list of calendars
			title:                                  "My Calendars",
			addNewCategoryLabel:                    "Add",
			dialogTemplate:                         "",
			dialogHeader_add:                       "Add new calendar",
			dialogHeader_edit:                      "Edit calendar",
			dialogColor_label:                      "Color:",
			dialogTextColor_label:                  "Text color:",
			dialogTimezoneLabel:                    "Timezone:",
			dialogButton_save:                      "Save",
			dialogButton_cancel:                    "Cancel",
            dialogButton_delete:                    "Delete",

            dialogErrorMassageSpecCharacter:        "The title cannot contain any of the following characters: {0}",
            dialogCopyMessage:                      "Link has been copied to the clipboard",
			// list of subscriptions
			subscriptionsTitle:                     "Subscriptions",
			subscriptionsDialog:                    "",
			subscriptionsDialogHeader:              "Edit subscription",
			subscriptionsDialogOwnerLabel:          "Owner:",
			subscriptionsDialogButton_unsubscribe:  "Unsubscribe",
			// subscriptions dialog
			subscriptionsManageLabel:               "Manage",
			subscriptionsManageDialog:              "",
			subscriptionsManageDialog_title:        "Manage subscriptions",
			subscriptionsManageDialog_qsearchText:  "Quick search",
			subscriptionsManageDialogButton_save:   "Save",
			subscriptionsManageDialogButton_cancel: "Cancel",
			// datepicker
			datepickerHideLabel: 'Hide mini calendar',
			datepickerShowLabel: 'Show mini calendar',
			// date box
			dayFormat: 'd',
			monthFormat: 'MMMM',
			yearFormat: 'yyyy'
		},

		todoList: {
			title:         'To-do',
			hideLabel:     'Hide ToDo List',
			showLabel:     'Show ToDo List',
			addTodoLabel:  'New todo',
			newTodoTitle: 'new todo',
			
			overdue: 'Overdue',
			noDueDate: 'No due date',

			menuTemplate: "",
			deleteIcon: "",
			editIcon: "",
			settingsIcon:"",

			menu: {
				hideColmpletedTodos: {hide: 'Hide completed items', show: 'Show completed items'},
				deleteCompletedTodos: 'Delete completed items'
			},
			sortByCalendarLabel: 'Sort by calendar',
			sortByPriorityLabel: 'Sort by priority',
			sortAlphabeticallyLabel: 'Sort alphabetically',
			
			menuTodoInCalendar: 'Show to-do in calendar',
			menuDeleteMarkedTodo: 'Hide to-do marked as done on page load',
			menuSyncLinks: 'Export and Sync'
		},
        deleteTodoDialog: {
            dialogTemplate: "",
            dialogHeader: "Delete todo",

            dialogSingleBody: "The deleted todo cannot be restored. Are you sure you want to continue?",

            dialogButton_apply: "Apply",
            dialogButton_cancel: "Cancel",
        },
		todoEditor: {
		    dialogTemplate: "",
		    
		    dialogHeader_add: "Create todo",
		    dialogHeader_edit: "Edit todo",
		    
		    dialogDateLabel: "Date",
		    
		    titleLabel: "Title",
		    descriptionLabel: "Description",

		    dialogButton_save: "Save",
		    dialogButton_cancel: "Cancel",
		},
		todoViewer:{
		    dialogTemplate: "",

		    dialogButton_mark_on: "Mark as done",
		    dialogButton_mark_off: "Mark as not done",
            dialogButton_edit: "Edit",
            dialogButton_delete: "Delete"
		},
		
		eventEditor: {
			dateFormat:                  "yyyy-MM-dd",
			timeFormat:                  "HH:mm",
			newEventTitle:               "New event",
			
			// dialog
			dialogTemplate:              "",
			dialogHeader_add:            "Add new event",
			dialogSummaryLabel:          "Event name:",
			eventButton:                 "Event",
			todoButton:                  "Todo",
			dialogLocationLabel:         "Where:",
			dialogAttendeesLabel:        "Guests:",
			dialogOwnerLabel:            "Owner:",
			dialogOrganizerLabel:        "Organizer:",
			dialogAllDayLabel:           "All-day event",
			dialogSentInvitations:       "Sent invitations",
			dialogAllDay_no:             "This event is not all-day.",
			dialogAllDay_yes:            "This is all-day event.",
			dialogFromLabel:             "From:",
			dialogToLabel:               "To:",
			dialogRepeatLabel:           "Repeat:",
			dialogStatusLabel:           "Status:",
			dialogRepeatOption_never:    "never",
			dialogRepeatOption_day:      "every day",
			dialogRepeatOption_week:     "every week",
			dialogRepeatOption_month:    "every month",
			dialogRepeatOption_year:     "every year",
			dialogAlertLabel:            "Alert:",
			dialogAlertOption_default:   "default",
			dialogAlertOption_never:     "never",
			dialogAlertOption_5minutes:  "5 minutes",
			dialogAlertOption_15minutes: "15 minutes",
			dialogAlertOption_30minutes: "half an hour",
			dialogAlertOption_hour:      "an hour",
			dialogAlertOption_2hours:    "2 hours",
			dialogAlertOption_day:       "a day",
			dialogSharing_no:            "This event is not shared.",
			dialogCalendarLabel:         "Calendar:",
			dialogDescriptionLabel:      "Description:",
			dialogButton_edit:           "Edit",
			dialogButton_save:           "Save",
			dialogButton_close:          "Close",
			dialogButton_cancel:         "Cancel",
			dialogButton_delete:         "Delete",
			dialogButton_unsubscribe:    "Unsubscribe",
			dialogButton_moreDetails:    "More details",
		    dialogButton_details:        "Details",
			
			// new option
			dialogRepeatOption_custom:   "настройка",
			dialogHeader_createEvent: "Create new event",
			dialogHeader_editEvent: "Edit event",
			dialogHeader_viewEvent: "View event",
		},		
		
		repeatSettings: {
			dateFormat:                   "yyyy-MM-dd",
			timeFormat:                   "HH:mm",
			
			// dialog
			dialogTemplate:               "",
			dialogHeader:                 "Recurrence setup",
			
			// start date
			dialogFromLabel:              "starting with",
			
			// end of repeat
			dialogToLabel:                "end",
			dialogOptionNever:            "never",
			dialogOptionDate:             "date",
			dialogOptionCount:            "number of times",
			
			dialogAfterLabel:             "after",
			dialogTimesLabel:             "times",
			
			// repeat by 
			dialogRepeatOnLabel:          "by",
			dialogRepeatOn_days:          "days",
			dialogRepeatOn_weeks:         "weeks",
			dialogRepeatOn_months:        "months",
			dialogRepeatOn_years:         "years",
			
			// interval
			dialogEachLabel:              "every",
			dialogAliasLabel:             "every",
			dialogIntervalOption_day:     "day",
			dialogIntervalOption_week:    "week",
			dialogIntervalOption_month:   "month",
			dialogIntervalOption_year:    "yesr",
			
			dayNames:                     ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
			
			dayIndexResponse: {
									su: 0,
									mo: 1,
									tu: 2,
									we: 3,
									th: 4,
									fr: 5,
									sa: 6
			},
			dayNamesShort:                ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
			dayAliasNames:                ['first','second','third','last but one','last'],
			
			// buttons
			dialogButton_save:            "Save",
			dialogButton_cancel:          "Cancel"
		},
		
		deleteSettings: {
			// dialog
			dialogTemplate:               "",
			dialogHeader:                 "Delete recurring event",
			dialogSingleHeader:           "Delete event",
			dialogSingleBody:             "The deleted event cannot be restored. Are you sure you want to continue?",
			
			dialogDeleteOnlyThisLabel:    "This event only",
			dialogDeleteFollowingLabel:   "This event and all that follow",
			dialogDeleteAllLabel:         "All events in the series",
			
			// buttons
			dialogButton_save:            "Apply",
			dialogButton_cancel:          "Cancel"
		},
		
		confirmPopup: {
		    // dialog
		    dialogTemplate:               "",
		    dialogAddEventHeader:         "Sending invitations",
		    dialogUpdateEventHeader:      "Sending updates",
		    dialogDeleteEventHeader:      "Sending cancellations",
		    dialogAddEventBody:           "Would you like to send invitations to guests?",
		    dialogUpdateEventBody:        "Would you like to send updates to existing guests?",
		    dialogUpdateGuestsBody:       "Would you like to send updates only to new guests and cancellations or just everyone?",
		    dialogDeleteEventBody:        "Would you like to send cancellations to existing guests?",
		    
		    dialogSuccessToastText:       "Notifications successfully sent",
		    dialogErrorToastText:         "An error occurred while sending notifications",
		    
		    // buttons
		    dialogButtonSend:            "Send",
		    dialogButtonSendCustoms:     "New guests and cancellations",
		    dialogButtonSendEveryone:    "Everyone",
		    dialogButtonDontSend:        "Don't Send"
		},

		icalStream: {
			// dialog
			dialogTemplate:                       "",
			newiCalTitle:                         "Calendar from iCal feed",
			importEventsTitle:                    "Import events",

			dialogHeader:                         "Calendar event export",
			dialogDescription:                    "Use the following address to access your calendar from other applications. You can copy and enter this information into any calendar supporting iCal format. You can save the data into a file in iCal format and export the events into another calendar or application.",
            dialogCaldavHelp:                     "Connection instructions in the ",
            dialogHelpCenter:                     "Help Center",
			dialogTodoDescription:                "To configure the export of todos  to another calendar, copy the corresponding link and add it to the required calendar.",


			dialogImportExportLabel:              "Import/Export",
			dialogStreamLink:                     "Export events from ONLYOFFICE calendar",
			dialogImportLabel:                    "Import events into ONLYOFFICE calendar",
			dialogImportLabelNew:                 "Select iCal file",
			dialogExportLink:                     "Export and Sync",
			dialogButton_fileSelected:            "file is selected",
			dialogButton_fileNotSelected:         "no file is selected",
			dialog_incorrectFormat:               "Wrong format of the iCal file",
			
			dialogInputiCalLabel:                 "Enter the iCal feed link",
			dialogSavediCalLabel:                 "iCal feed link:",
			
			// buttons
			dialogButton_close:                   "Close",
			dialogButton_browse:                  "Select file"
		},

		dayView: {
			dateFormat: {
				topText:    "dddd",
				bigText:    "d",
				bottomText: "MMMM,'<br/>' yyyy"
			},
			todayTasksTitle:  "Today's tasks",
			alldayTasksTitle: "All-day tasks"
		},

		taskListTimeFormat: 'HH:mm{[ - HH:mm]}',

		listView: {
			headerDateFormat: 'MM/dd/yy',
			monthTitleFormat: 'MMMM yyyy',
			dayTitleFormat:   'ddd, MMM d, yyyy',
			timeFormat:       'HH:mm',
			noEventsMessage:  'There are no events to show.'
		},

		popupCellFormat: "dd.MM.yy"

	};
}();

// right-to-left defaults
// such structure of code is used only for code folding in NetBeans IDE
var rtlDefaults = function rtlDefaultsModule() {return {

		header: {
			left:   '',
			center: 'next,title,prev',
			right:  ''
		},
		buttonText: {
			prev: '&nbsp;&#9658;&nbsp;',
			next: '&nbsp;&#9668;&nbsp;',
			prevYear: '&nbsp;&gt;&gt;&nbsp;',
			nextYear: '&nbsp;&lt;&lt;&nbsp;'
		},
		buttonIcons: {
			prev: 'circle-triangle-e',
			next: 'circle-triangle-w'
		}

	};
}();


var fc = $.fullCalendar = {version: "2.0.1"};

var fcColors = function fcColorModule()
{
var _this = {};
_this.DefaultPicker = [
					"#F48454", "#FFB45E", "#FFD267", "#B7D269", "#6BBD72", "#77CF9A", "#6AC6DD",
					"#4682B6", "#6A9AD2", "#8A98D8", "#7E6EB2", "#B58FD6", "#D28CC8", "#E795C1",
					"#F2A9BE", "#DF7895"];
_this.TextPicker = ["#FFF","#000"];					
	return _this;

}();

var fcViews = fc.views = {};

var fcMenus = function fcMenusModule() {

	var _this = {};

	_this.hideMenus = function(m) {
		for (var i in this) if (this.hasOwnProperty(i)) {
			if (typeof this[i] == "function") {continue;}
			if ((/.+Menu$/i).test(i)) {
				if (m !== undefined && m == this[i]) {continue;}
				this[i].popupMenu("close");
			}
		}
	};

	_this.createHeaderYearMenu = function(calendar) {
		var year = calendar.getDate().getFullYear();
		if (!this.headerYearMenu || this.headerYearMenu.length < 1) {
			this.headerYearMenu = $("<div id='fc_header_year_menu'/>");
		} else {
			this.headerYearMenu.popupMenu("close");
			this.headerYearMenu.popupMenu("destroy");
		}
		this.headerYearMenu.popupMenu({
			anchor: "left,bottom",
			direction: "right,down",
			arrow: "up",
			arrowPosition: "50%",
			showArrow: false,
			cssClassName: "asc-popupmenu",
            items: [
                (year + 3).toString(), (year + 2).toString(), (year + 1).toString(),
                "divider",
                (year - 1).toString(), (year - 2).toString(), (year - 3).toString()
			],
			itemClick: function(event, data) {
				var d = calendar.getDate();
				d.setFullYear(parseInt(data.item, 10));
				calendar.gotoDate(d);
			}
		});
	};

	_this.createHeaderMonthMenu = function(calendar) {
		if (!this.headerMonthMenu || this.headerMonthMenu.length < 1) {
			this.headerMonthMenu = $("<div id='fc_header_month_menu'/>");
		} else {
			this.headerMonthMenu.popupMenu("close");
			this.headerMonthMenu.popupMenu("destroy");
		}
		this.headerMonthMenu.popupMenu({
			anchor: "left,bottom",
			direction: "right,down",
			arrow: "up",
			arrowPosition: "50%",
			showArrow: false,
			cssClassName: "asc-popupmenu",
			items: calendar.options.monthNames,
			itemClick: function(event, data) {
				var d = calendar.getDate();
				d.setMonth(data.itemIndex);
				calendar.gotoDate(d);
			}
		});
	};

	_this.buildTitleMenus = function(calendar, title) {
		var self = this;

		var monthLabel = undefined;
		title.find(".month").click(
				function() {
					if (!monthLabel || monthLabel != this) {
						monthLabel = this;
						self.createHeaderMonthMenu(calendar);
					}
					self.hideMenus(self.headerMonthMenu);
					self.headerMonthMenu.popupMenu("open", this);
				});

		var yearLabel = undefined;
		title.find(".year").click(
				function() {
					if (!yearLabel || yearLabel != this) {
						yearLabel = this;
						self.createHeaderYearMenu(calendar);
					}
					self.hideMenus(self.headerYearMenu);
					self.headerYearMenu.popupMenu("open", this);
				});
	};

	return _this;

}();

var fcUtil = function fcUtilModule() {

	var _this = {};

	/**
	 * Converts an RGB color value to HSV. Conversion formula
	 * adapted from http://en.wikipedia.org/wiki/HSV_color_space.
	 * Assumes r, g, and b are contained in the set [0, 255] and
	 * returns h, s, and v in the set [0, 1].
	 *
	 * @param   r       (Number) The red color value
	 * @param   g       (Number) The green color value
	 * @param   b       (Number) The blue color value
	 * @return  Array   The HSV representation
	 */
	_this.rgbToHsv = function(r, g, b){
			r = r/255, g = g/255, b = b/255;
			var max = Math.max(r, g, b), min = Math.min(r, g, b);
			var h, s, v = max;

			var d = max - min;
			s = max == 0 ? 0 : d / max;

			if(max == min){
					h = 0; // achromatic
			}else{
					switch(max){
							case r:h = (g - b) / d + (g < b ? 6 : 0);break;
							case g:h = (b - r) / d + 2;break;
							case b:h = (r - g) / d + 4;break;
					}
					h /= 6;
			}

			return [h, s, v];
	};

	/**
	 * Converts an HSV color value to RGB. Conversion formula
	 * adapted from http://en.wikipedia.org/wiki/HSV_color_space.
	 * Assumes h, s, and v are contained in the set [0, 1] and
	 * returns r, g, and b in the set [0, 255].
	 *
	 * @param   h       (Number) The hue
	 * @param   s       (Number) The saturation
	 * @param   v       (Number) The value
	 * @return  Array   The RGB representation
	 */
	_this.hsvToRgb = function(h, s, v){
			var r, g, b;

			var i = Math.floor(h * 6);
			var f = h * 6 - i;
			var p = v * (1 - s);
			var q = v * (1 - f * s);
			var t = v * (1 - (1 - f) * s);

			switch(i % 6){
					case 0:r = v, g = t, b = p;break;
					case 1:r = q, g = v, b = p;break;
					case 2:r = p, g = v, b = t;break;
					case 3:r = p, g = q, b = v;break;
					case 4:r = t, g = p, b = v;break;
					case 5:r = v, g = p, b = q;break;
			}

			return [Math.floor(r * 255), Math.floor(g * 255), Math.floor(b * 255)];
	};

	/**
	 * Converts an RGB color value to HSL. Conversion formula
	 * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
	 * Assumes r, g, and b are contained in the set [0, 255] and
	 * returns h, s, and l in the set [0, 1].
	 *
	 * @param   r       (Number) The red color value
	 * @param   g       (Number) The green color value
	 * @param   b       (Number) The blue color value
	 * @return  Array   The HSL representation
	 */
	_this.rgbToHsl = function(r, g, b) {
			r /= 255, g /= 255, b /= 255;
			var max = Math.max(r, g, b), min = Math.min(r, g, b);
			var h = 0, s = 0, l = (max + min) / 2;

			if(max == min){
					h = s = 0; // achromatic
			}else{
					var d = max - min;
					s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
					switch(max){
							case r:h = (g - b) / d + (g < b ? 6 : 0);break;
							case g:h = (b - r) / d + 2;break;
							case b:h = (r - g) / d + 4;break;
					}
					h /= 6;
			}

			return [h, s, l];
	};

	/**
	 * Converts an HSL color value to RGB. Conversion formula
	 * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
	 * Assumes h, s, and l are contained in the set [0, 1] and
	 * returns r, g, and b in the set [0, 255].
	 *
	 * @param   h       (Number) The hue
	 * @param   s       (Number) The saturation
	 * @param   l       (Number) The lightness
	 * @return  Array   The RGB representation
	 */
	_this.hslToRgb = function(h, s, l) {
			var r, g, b;

			if(s == 0){
					r = g = b = l; // achromatic
			}else{
					function hue2rgb(p, q, t){
							if(t < 0) t += 1;
							if(t > 1) t -= 1;
							if(t < 1/6) return p + (q - p) * 6 * t;
							if(t < 1/2) return q;
							if(t < 2/3) return p + (q - p) * (2/3 - t) * 6;
							return p;
					}

					var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
					var p = 2 * l - q;
					r = hue2rgb(p, q, h + 1/3);
					g = hue2rgb(p, q, h);
					b = hue2rgb(p, q, h - 1/3);
			}

			return [Math.floor(r * 255), Math.floor(g * 255), Math.floor(b * 255)];
	};

    _this.randomHex = function (isFont) {
        if(isFont)
            return fcColors.TextPicker[1];

        return fcColors.DefaultPicker[Math.floor(Math.random() * fcColors.DefaultPicker.length)];
    };

	_this.parseCssColor = function(c) {
		var reColor =
				/\s*#([0-9a-f]{6}\s*$|[0-9a-f]{3}\s*$)|\s*rgba?\(\s*([0-9]+\s*,\s*[0-9]+\s*,\s*[0-9]+\s*(?:,\s*(?:[0-9]+(?:\.[0-9]+)?|[0-9]*\.[0-9]+))?)\s*\)/i;
		var m = reColor.exec(c);
		if (!m) {return null;}

		var reRgb =
				/^([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})$|^([0-9a-f])([0-9a-f])([0-9a-f])$|^([0-9]{1,3}),\s*([0-9]{1,3}),\s*([0-9]{1,3})(?:,\s*([0-9.]+))?$/i;
		var x = reRgb.exec(m[1] || m[2]);

		var type = (typeof x[1] === "string" || typeof x[4] === "string") ? 0 :
				(typeof x[10] === "string" ? 2 : 1);

		var r = x[1] || x[4];
		var g = x[2] || x[5];
		var b = x[3] || x[6];
		r = r ? parseInt(r.length > 1 ? r : r + r, 16) : parseInt(x[7], 10);
		g = g ? parseInt(g.length > 1 ? g : g + g, 16) : parseInt(x[8], 10);
		b = b ? parseInt(b.length > 1 ? b : b + b, 16) : parseInt(x[9], 10);

		var a = x[10];
		a = a ? (Math.round(parseFloat(a) * 100) * 0.01) : 1;

		var color2 = ((r << 16) | (g << 8) | b).toString(16);
		while (color2.length < 6) {color2 = "0" + color2;}
		color2 = "#" + color2;

		var rgb1 = r + ", " + g + ", " + b;
		var rgb2 = "rgb(" + rgb1 + ")";
		var rgba2 = "rgba(" + rgb1 + ", " + a + ")";

		return {
			type: type,
			r: r,
			g: g,
			b: b,
			a: a,
			rgb: rgb2,
			rgba: rgba2,
			color: color2,
			origColor: type === 0 ? color2 : (type === 1 ? rgb2 : rgba2)
		};
	};

	_this.changeColorLightness = function(r, g, b, ratio) {
		//var h = _this.rgbToHsv(r, g, b);
		//var rgb = _this.hslToRgb(h[0], h[1], h[2] * ratio);
		var rgb = [Math.round(r * ratio), Math.round(g * ratio), Math.round(b * ratio)];
		var bc = ((rgb[0] << 16) | (rgb[1] << 8) | rgb[2]).toString(16);
		while (bc.length < 6) {bc = "0" + bc;}
		return {
			r: rgb[0],
			g: rgb[1],
			b: rgb[2],
			color: "#" + bc
		};
	};

	_this.EventSourcesIterator = function(eventSources, arrayName) {
		var _curSource = 0;
		var _curIndex= 0;

		function _next() {
			for (; eventSources && eventSources.length > 0 &&
			       _curSource < eventSources.length; ++_curSource) {
				var source = eventSources[_curSource];
				for (; source && source[arrayName] && source[arrayName].length > 0 &&
				       _curIndex < source[arrayName].length; ++_curIndex) {
					if (source[arrayName][_curIndex] != undefined) {
						return source[arrayName][_curIndex++];
					}
				}
				_curIndex = 0;
			}
			return undefined;
		}

		function _prev() {
			if (eventSources && eventSources.length > 0) {
				if (_curSource == undefined) {
					_curSource = eventSources.length - 1;
				}
				for (; _curSource >= 0; --_curSource) {
					var source = eventSources[_curSource];
					if (source && source[arrayName] && source[arrayName].length > 0) {
						if (_curIndex == undefined) {
							_curIndex = source[arrayName].length - 1;
						}
						for (; _curIndex >= 0; --_curIndex) {
							if (source[arrayName][_curIndex] != undefined) {
								return source[arrayName][_curIndex--];
							}
						}
					}
					_curIndex = undefined;
				}
			}
			return undefined;
		}

		this.first = function() {
			_curSource = 0;
			_curIndex = 0;
			return _next.call(this);
		};

		this.next = function() {
			return _next.call(this);
		};

		this.last = function() {
			_curSource = undefined;
			_curIndex = undefined;
			return _prev.call(this);
		};

		this.prev = function() {
			return _prev.call(this);
		};
	};

	_this.objectIsValid = function(obj) {
		return obj !== undefined && obj !== null &&
		       obj.objectId !== undefined && obj.objectId !== null;
	};

	_this.objectIsEditable = function(obj) {
		return obj !== undefined && obj !== null && obj.isEditable;
	};

	_this.eventIsEditable = function(event) {
		return event.source === undefined || event.source === null ||
		       _this.objectIsEditable(event) ||
		       _this.objectIsEditable(event.source) ||
		       event.source && !event.source.isSubscription;
	};

	_this.getElementRect = function(elem) {
		var o = elem.offset();
		return {
			left:   o.left,
			top:    o.top,
			right:  o.left + elem.outerWidth(),
			bottom: o.top + elem.outerHeight()
		};
	};

	_this.isLeapYear = function(y) {
		return (y % 4 == 0) && ((y % 100 != 0) || (y % 400 == 0));
	};

	_this.validateDateString = function(s) {
		var m = s.replace(/^\s+|\s+$/g, "").match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/i);
		if (!m) {return null;}
		var mon = parseInt(m[2], 10);
		if (mon < 1 || mon > 12) {return null;}
		var leap = _this.isLeapYear(parseInt(m[1], 10));
		var day = parseInt(m[3], 10);
		var maxDay = [31,(leap?29:28),31,30,31,30,31,31,30,31,30,31];
		if (day < 1 || day > maxDay[mon-1]) {return null;}
		return m[1] + "-" +
				(m[2].length > 1 ? m[2] : "0" + m[2]) + "-" +
				(m[3].length > 1 ? m[3] : "0" + m[3]);
	};

	_this.validateTimeString = function(s) {
		var s1 = s.replace(/^\s+|\s+$/g, "");
		if (s1.length < 1) {return "00:00";}
		var m = s1.match(/^(\d{1,2}):(\d{1,2})$/i);
		if (!m) {return null;}
		var hh = parseInt(m[1], 10);
		if (hh < 0 || hh > 23) {return null;}
		var mm = parseInt(m[2], 10);
		if (mm < 0 || mm > 59) {return null;}
		return (m[1].length > 1 ? m[1] : "0" + m[1]) + ":" +
				(m[2].length > 1 ? m[2] : "0" + m[2]);
	};

	_this.validateNonemptyString = function(s) {
		var r = s.replace(/^\s+|\s+$/g, "");
		return r.length > 0 ? r : null;
	};

	_this.validateInput = function (elem, fn, result) {
		var inp = $(elem);
		var res = fn(inp.val());

		if (null == res) {
			inp.css("color", "#cc3300").css("border-color", "#cc3300");
			return false;
		}
	    
		inp.css("color", "").css("border-color", "");

		if (result) {
			result.value = res;
		}
		return true;
	};
	
	_this.validateInputHttp = function(elem, fn, result) {
		var inp = $(elem);
		var res = fn(inp.val());
		if (null === res) {
			inp.css("color", "#cc3300").css("border-color", "#cc3300");
			return false;
		}
		else {
			if (res.indexOf("http://") && res.indexOf("https://") && res.indexOf("webcal://")) {
				inp.css("color", "#cc3300").css("border-color", "#cc3300");
				return false;
			}
		}
	    
		inp.css("color", "").css("border-color", "");
	    
		if (result) {result.value = res;}
		return true;
	};

	_this.makeBullet = function(isActive, activeColor, inactiveColor, isShared) {
		var bg = htmlEscape(activeColor);
		var bullet =
				'<div class="bullet" style="border: 1px solid ' + bg + ';' +
						(isActive ? "background-color:" + bg : "") + '">' +
					(isShared ? '<div class="shared"/>' : '') +
				'</div>';
		return bullet;
	};

	_this.updateBullet = function(elem, isActive, activeColor, inactiveColor, isShared) {
		var bg = htmlEscape(activeColor),
		    s = elem.find(".shared");
		elem.css("background-color", isActive ? bg : "");
		elem.next(".label").css("color", isActive ? "" : inactiveColor);
		if (isShared && s.length < 1) {
			elem.append('<div class="shared"/>');
		} else if (!isShared && s.length > 0) {
			s.remove();
		}
	};

	return _this;

}();

var fcColorPicker = function fcCPModule() {
	var _this = {};

	var _cp;
	var _oldElem;

	function _init(colors) {
		//if (!_cp || _cp.length < 1) {
			_cp = $("<div id='fc_color_picker'/>").colorPicker({
			    arrowPosition:  "50%",
			    showArrow: false,
				anchorToBorder: true,
				showModal:      true,
				colors: {1: colors},
				colorSet: 1});
		//}
	}

	_this.open = function(iconElem, onColorSelect, colors) {
		_init(colors);
		if (_oldElem != iconElem) {
			_cp.colorPicker("close");
		}
		_oldElem = iconElem;
		var icon = $(iconElem);

		// to round all float numbers to 2nd digit after point do not use directly css(...)
		var rgb = fcUtil.parseCssColor(icon.css("background-color"));
		_cp.colorPicker("option", "selectedColor", rgb.origColor);

		_cp.colorPicker("option", "select", function(ev, c) {
			if ($.isFunction(onColorSelect)) {onColorSelect(c);}
		});
		_cp.colorPicker("open", icon.parent());
	};

	_this.close = function() {
		if (_cp && _cp.length > 0) {
			_cp.colorPicker("close");
			_oldElem = undefined;
		}
	};

	return _this;
}();

var fcDatepicker = function fcDPModule() {
	var _this = {};

	var _frame;
	var _dp;

// show calendar
	function _init(calendar) {
		if (!_frame || _frame.length < 1) {
			_frame = $(
				'<div id="fc_common_dp">' +
					'<div class="asc-datepicker"/>' +
				'</div>')
				.popupFrame({
						anchor: "left,bottom",
						direction: "right,down",
						arrow: "up",
						arrowPosition: "50%",
						showArrow: false,
						showModal: true});
			_dp = _frame.find(".asc-datepicker").datepicker({
					firstDay: calendar.options.firstDay});
		}
	}

	function _fixDatepickerUI() {
		setTimeout(
				function() {
					_dp.find(".ui-datepicker-prev .ui-icon")
							.removeClass("ui-icon-circle-triangle-w")
							.addClass("ui-icon-carat-1-w");
					_dp.find(".ui-datepicker-next .ui-icon")
							.removeClass("ui-icon-circle-triangle-e")
							.addClass("ui-icon-carat-1-e");
				},
				0);
	}

	_this.open = function(calendar, label, onShow, onSelect) {
		_init(calendar);
		_dp.datepicker('option', 'onSelect', function(){onSelect.call(_this, label, _dp);});
		_dp.datepicker('option', 'onChangeMonthYear', _fixDatepickerUI);
		onShow.call(_this, _dp);
		_fixDatepickerUI();
		_frame.popupFrame("open", label);
	};

	_this.close = function() {
		isOpenCommonDatePicker = false;
		if (_frame && _frame.length > 0) {
			_frame.popupFrame("close");
		}
	};

	return _this;
}();

var fcDebugMode = 0;

function debugOutput(channel) {
	if (fcDebugMode && console && $.isFunction(console[channel])) {
		console[channel].apply(this, Array.prototype.slice.call(arguments, 1));
	}
}


var kCalendarAddAction        = 1;
var kCalendarChangeAction     = 2;
var kCalendarDeleteAction     = 3;
var kCalendarHideAction       = 4;
var kCalendarCancelAction     = 5;

var kEventAddAction           = 1;
var kEventChangeAction        = 2;
var kEventDeleteAction        = 3;
var kEventUnsubscribeAction   = 4;
var kEventCancelAction        = 5;
    
var kTodoAddAction            = 1;
var kTodoChangeAction         = 2;
var kTodoDeleteAction         = 3;
var kTodoUnsubscribeAction    = 4;
var kTodoCancelAction         = 5;

var kCalendarPermissions      = 1;
var kEventPermissions         = 2;

var kSubscriptionAddAction    = 1;
var kSubscriptionChangeAction = 2;
var kSubscriptionRemoveAction = 3;

var kAlertDefault             = -1;
var kAlertNever               = 0;

var kRepeatNever              = 0;

var AjaxUploader = undefined;

function initTemplates(options) {

	var c = options.categories;
	var e = options.eventEditor;
	var t = options.todoEditor;
	var tv = options.todoViewer;
	var dt = options.deleteTodoDialog;
	var tl = options.todoList;
	var rs = options.repeatSettings;
	var ds = options.deleteSettings;
	var ic = options.icalStream;
	var cp = options.confirmPopup;

    var templateData = {
        dialogHeaderAdd: htmlEscape(c.dialogHeader_add),
        dialogHeaderImport: htmlEscape(c.dialogHeader_import),
        dialogHeaderEdit: htmlEscape(c.dialogHeader_edit),
        defaultCalendarName: htmlEscape(c.defaultTitle),
        dialogBgColorLabel: htmlEscape(c.dialogColor_label),
        dialogFontColorLabel: htmlEscape(c.dialogTextColor_label),
        dialogAlertLabel: e.dialogAlertLabel,
        alertOptions: [
							{ value:kAlertNever, text:e.dialogAlertOption_never },
							{ value:1, text:e.dialogAlertOption_5minutes },
							{ value:2, text:e.dialogAlertOption_15minutes },
							{ value:3, text:e.dialogAlertOption_30minutes },
							{ value:4, text:e.dialogAlertOption_hour },
							{ value:5, text:e.dialogAlertOption_2hours },
							{ value:6, text:e.dialogAlertOption_day }
        ],
        dialogTimezoneLabel: c.dialogTimezoneLabel,
        dialogInputiCalLabel: ic.dialogInputiCalLabel,
        dialogImportExportLabel: ic.dialogImportExportLabel,
        dialogImportLabel: ic.dialogImportLabel,
        dialogImportLabelNew: ic.dialogImportLabelNew,
        fileNotSelected: ic.dialogButton_fileNotSelected,
        dialogStreamLink: ic.dialogStreamLink,
        dialogExportLink: ic.dialogExportLink,
        dialogButtonSave: c.dialogButton_save,
        dialogButtonCancel: c.dialogButton_cancel,
        dialogButtonDelete: c.dialogButton_delete,
        maxlength: defaults.eventMaxTitleLength
    };

    options.categories.dialogTemplate = $("#categoriesDialogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");

    templateData = {
        dialogHeader: htmlEscape(c.subscriptionsDialogHeader),
        dialogBgColorLabel: htmlEscape(c.dialogColor_label),
        dialogFontColorLabel: htmlEscape(c.dialogTextColor_label),
        dialogAlertLabel: htmlEscape(e.dialogAlertLabel),
        alertOptions: [
							{ value:kAlertNever, text:e.dialogAlertOption_never },
							{ value:1, text:e.dialogAlertOption_5minutes },
							{ value:2, text:e.dialogAlertOption_15minutes },
							{ value:3, text:e.dialogAlertOption_30minutes },
							{ value:4, text:e.dialogAlertOption_hour },
							{ value:5, text:e.dialogAlertOption_2hours },
							{ value:6, text:e.dialogAlertOption_day }
        ],
        dialogTimezoneLabel: htmlEscape(c.dialogTimezoneLabel),
        dialogOwnerLabel: c.subscriptionsDialogOwnerLabel,
        dialogSavediCalLabel: htmlEscape(ic.dialogSavediCalLabel),
        dialogImportExportLabel: htmlEscape(ic.dialogImportExportLabel),
        dialogImportLabel: htmlEscape(ic.dialogImportLabel),
        dialogImportLabelNew: htmlEscape(ic.dialogImportLabelNew),
        fileNotSelected: htmlEscape(ic.dialogButton_fileNotSelected),
        dialogStreamLink: htmlEscape(ic.dialogStreamLink),
        dialogExportLink: htmlEscape(ic.dialogExportLink),
        dialogButtonSave: c.dialogButton_save,
        dialogButtonCancel: c.dialogButton_cancel,
        dialogButtonDelete: c.dialogButton_delete,
        dialogButtonUnsubscribe: c.subscriptionsDialogButton_unsubscribe,
        maxlength: defaults.eventMaxTitleLength
    };

    options.categories.subscriptionsDialog = $("#categoriesSubscriptionsDialogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");

    templateData = {
        dialogTitle: htmlEscape(c.subscriptionsManageDialog_title),
        dialogSearchText: htmlEscape(c.subscriptionsManageDialog_qsearchText),
        dialogButtonSave: htmlEscape(c.subscriptionsManageDialogButton_save),
        dialogButtonCancel: htmlEscape(c.subscriptionsManageDialogButton_cancel),
        maxlength: defaults.eventMaxTitleLength
    };

    options.categories.subscriptionsManageDialog = $("#categoriesSubscriptionsManageDialogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");

    templateData = {
        dialogHeaderAdd: htmlEscape(e.dialogHeader_add),
        defaultEventSummary: htmlEscape(e.newEventTitle),
        eventButton: htmlEscape(e.eventButton),
        todoButton: htmlEscape(e.todoButton),
        dialogLocationLabel: htmlEscape(e.dialogLocationLabel),
        dialogAttendeesLabel: htmlEscape(e.dialogAttendeesLabel),
        dialogOwnerLabel: htmlEscape(e.dialogOwnerLabel),
        dialogOrganizerLabel: htmlEscape(e.dialogOrganizerLabel),
        dialogAllDayNoText: htmlEscape(e.dialogAllDay_no),
        dialogAllDayYesText: htmlEscape(e.dialogAllDay_yes),
        dialogFromLabel: htmlEscape(e.dialogFromLabel),
        dialogToLabel: htmlEscape(e.dialogToLabel),
        dialogAlertLabel: htmlEscape(e.dialogAlertLabel),
        dialogRepeatLabel: htmlEscape(e.dialogRepeatLabel),
        dialogSharingNoText: htmlEscape(e.dialogSharing_no),
        dialogSharingYesText: htmlEscape(options.sharedList.title),
        dialogCalendarLabel: htmlEscape(e.dialogCalendarLabel),
        dialogDescriptionLabel: htmlEscape(e.dialogDescriptionLabel),
        dialogSummaryLabel: htmlEscape(e.dialogSummaryLabel),
        dialogAllDayLabel: htmlEscape(e.dialogAllDayLabel),
        dialogSentInvitations: htmlEscape(e.dialogSentInvitations),
        dialogAlertOptionDefault: htmlEscape(e.dialogAlertOption_default),
        dialogRepeatOptionNever: htmlEscape(e.dialogRepeatOption_never),
        dialogStatusLabel: htmlEscape(e.dialogStatusLabel),
        dialogStatusOptionTentative: htmlEscape(e.dialogStatusOption_tentative),
        dialogStatusOptionConfirmed: htmlEscape(e.dialogStatusOption_confirmed),
        dialogStatusOptionCancelled: htmlEscape(e.dialogStatusOption_cancelled),
        repeatFromLabel: htmlEscape(rs.dialogFromLabel),
        repeatRepeatOnLabel: htmlEscape(rs.dialogRepeatOnLabel),
        repeatRepeatOnDays: htmlEscape(rs.dialogRepeatOn_days),
        repeatEachLabel: htmlEscape(rs.dialogEachLabel),
        repeatIntervalOptionDay: htmlEscape(rs.dialogIntervalOption_day),
        repeatToLabel: htmlEscape(rs.dialogToLabel),
        repeatOptionNever: htmlEscape(rs.dialogOptionNever),
        repeatAfterLabel: htmlEscape(rs.dialogAfterLabel),
        repeatTimesLabel: htmlEscape(rs.dialogTimesLabel),
        dialogButtonEdit: htmlEscape(e.dialogButton_edit),
        dialogButtonSave: htmlEscape(e.dialogButton_save),
        dialogButtonClose: htmlEscape(e.dialogButton_close),
        dialogButtonCancel: htmlEscape(e.dialogButton_cancel),
        dialogButtonDelete: htmlEscape(e.dialogButton_delete),
        dialogButtonUnsubscribe: htmlEscape(e.dialogButton_unsubscribe),
        dialogButtonMoreDetails: htmlEscape(e.dialogButton_moreDetails),
        dialogButtonDetails: htmlEscape(e.dialogButton_details),
        maxlength: defaults.eventMaxTitleLength,
        
        dialogTodoDate: htmlEscape(t.dialogDateLabel),
        dialogTodoNameLabel: htmlEscape(t.titleLabel),
        
    };

    options.eventEditor.dialogTemplate = $("#eventEditorDialogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");
	
    templateData = {
        dialogHeader: htmlEscape(ds.dialogHeader),
        dialogSingleHeader: htmlEscape(ds.dialogSingleHeader),
        dialogSingleBody: htmlEscape(ds.dialogSingleBody),
        dialogDeleteOnlyThisLabel: htmlEscape(ds.dialogDeleteOnlyThisLabel),
        dialogDeleteFollowingLabel: htmlEscape(ds.dialogDeleteFollowingLabel),
        dialogDeleteAllLabel: htmlEscape(ds.dialogDeleteAllLabel),
        dialogButtonSave: htmlEscape(ds.dialogButton_save),
        dialogButtonCancel: htmlEscape(ds.dialogButton_cancel)
    };

    options.deleteSettings.dialogTemplate = $("#deleteSettingsDalogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");

    templateData = {
        dialogHeader: htmlEscape(ic.dialogHeader),
        
        dialogTodoDescription: htmlEscape(ic.dialogTodoDescription),
        
        dialogDescription: htmlEscape(ic.dialogDescription),
        dialogCaldavHelp: htmlEscape(ic.dialogCaldavHelp),
        dialogHelpCenter: htmlEscape(ic.dialogHelpCenter),
        dialogPreparingMessage: htmlEscape(ic.dialogPreparingMessage),
        dialogPreparingErrorMessage: htmlEscape(ic.dialogPreparingErrorMessage),
        dialogButtonClose: htmlEscape(ic.dialogButton_close),
        dialogExportCalDav: htmlEscape(ic.dialogExportCalDav),
        dialogCopyButton: htmlEscape(ic.dialogCopyButton),
        dialogTryAgainButton: htmlEscape(ic.dialogTryAgainButton),
        dialogExportIcal: htmlEscape(ic.dialogExportIcal)
    };

    options.icalStream.dialogTemplate = $("#icalStreamDialogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");
    
    templateData = {
        dialogHeader: htmlEscape(cp.dialogAddEventHeader),
        dialogBody: htmlEscape(cp.dialogAddEventBody),
        dialogButtonSend: htmlEscape(cp.dialogButtonSend),
        dialogButtonSendCustoms: htmlEscape(cp.dialogButtonSendCustoms),
        dialogButtonSendEveryone: htmlEscape(cp.dialogButtonSendEveryone),
        dialogButtonDontSend: htmlEscape(cp.dialogButtonDontSend)
    };

    options.confirmPopup.dialogTemplate = $("#attendeeConfirmNotificationTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");
    
    templateData = {
        dialogTitle: htmlEscape(t.dialogHeader_add),
        dialogDate: htmlEscape(t.dialogDateLabel),
        
        dialogNameLabel: htmlEscape(t.titleLabel),
        dialogDescriptionLabel: htmlEscape(t.descriptionLabel),
        
        dialogButtonSave: htmlEscape(t.dialogButton_save),
        dialogButtonCancel: htmlEscape(t.dialogButton_cancel),
    };
    options.todoEditor.dialogTemplate = $("#todoEditorDialogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");
    
    templateData = {
        dialogButtonMarkOn: htmlEscape(tv.dialogButton_mark_on),
        dialogButtonMarkOff: htmlEscape(tv.dialogButton_mark_off),
        dialogButtonEdit: htmlEscape(tv.dialogButton_edit),
        dialogButtonDelete: htmlEscape(tv.dialogButton_delete),
    };
    options.todoViewer.dialogTemplate = $("#todoViewDialogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");
    
    templateData = {
        
        dialogHeader: htmlEscape(dt.dialogHeader),
        dialogText: htmlEscape(dt.deleleDialogText),
        
        dialogSingleBody: htmlEscape(dt.dialogSingleBody),
        
        dialogButton_apply: htmlEscape(dt.dialogButton_apply),
        dialogButton_cancel: htmlEscape(dt.dialogButton_cancel),

    };
    options.deleteTodoDialog.dialogTemplate = $("#deleteTodoDalogTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");
    
    templateData = {
        menuTodoInCalendar: htmlEscape(tl.menuTodoInCalendar),
        menuDeleteMarkedTodo: htmlEscape(tl.menuDeleteMarkedTodo),
        menuSyncLinks: htmlEscape(tl.menuSyncLinks)
    };
    options.todoList.menuTemplate = $("#todoMenuTemplate").tmpl(templateData).html().replace(/\>\s+\</g, "><");
    options.todoList.deleteIcon = $("#delete_todo_icon").tmpl(templateData).html().replace(/\>\s+\</g, "><");
    options.todoList.editIcon = $("#edit_todo_icon").tmpl(templateData).html().replace(/\>\s+\</g, "><");
    options.todoList.settingsIcon = $("#settings_todo_icon").tmpl(templateData).html().replace(/\>\s+\</g, "><");

}


$.fn.fullCalendar = function(options) {


	// method calling
	if (typeof options == 'string') {
		var args = Array.prototype.slice.call(arguments, 1);
		var res = undefined;
		this.each(function() {
			var calendar = $.data(this, 'fullCalendar');
			if (calendar && $.isFunction(calendar[options])) {
				var r = calendar[options].apply(calendar, args);
				if (res === undefined) {
					res = r;
				}
				if (options == 'destroy') {
					$.removeData(this, 'fullCalendar');
				}
			}
		});
		return res !== undefined ? res : this;
	}


	// would like to have this logic in EventManager, but needs to happen before options are recursively extended
	var eventSources = options.eventSources || [];
	delete options.eventSources;
	if (options.events) {
		eventSources.push(options.events);
		delete options.events;
	}


	options = $.extend(true, {},
		defaults,
		(options.isRTL || options.isRTL===undefined && defaults.isRTL) ? rtlDefaults : {},
		window.g_fcOptions ? window.g_fcOptions : {},
		options
	);

	initTemplates(options);

	if ($.isFunction(options.loadEvents)) {
		fc.sourceFetchers[0] = options.loadEvents;
	}

	options.eventResize = options.eventResize ||
			function(event, dayDelta, minuteDelta, revertFunc, jsEvent, ui, view) {
				if (!fcUtil.eventIsEditable(event)) {
					revertFunc();
				}
			};

	options.eventDrop = options.eventDrop ||
			function(event, dayDelta, minuteDelta, allDay, revertFunc, jsEvent, ui, view) {
				if (!fcUtil.eventIsEditable(event)) {
					revertFunc();
				}
			};


	this.each(function(i, _element) {
		var element = $(_element);
		var calendar = new Calendar(element, options, eventSources);
		element.data('fullCalendar', calendar); // TODO: look into memory leak implications
		calendar.render();
	});


	return this;

};



// function for adding/overriding defaults
function setDefaults(d) {
	$.extend(true, defaults, d);
}



function Calendar(element, options, eventSources) {
	var t = this;

	// exports
	t.options = options;
	t.render = render;
	t.destroy = destroy;
	t.refetchEvents = refetchEvents;
	t.reportEvents = reportEvents;
	t.reportEventChange = reportEventChange;
	t.rerenderEvents = rerenderEvents;
	t.rerenderCategories = rerenderCategories;
	t.changeView = changeView;
	t.openTodoList = openTodoList;
	t.changeViewAndMode = changeViewAndMode;
	t.select = select;
	t.unselect = unselect;
	t.prev = prev;
	t.next = next;
	t.prevYear = prevYear;
	t.nextYear = nextYear;
	t.today = today;
	t.gotoDate = gotoDate;
	t.incrementDate = incrementDate;
	t.formatDate = function(date, format) {return formatDate(date, format, options);};
	t.formatDates = function(date1, date2, format) {return formatDates(date1, date2, format, options);};
	t.getDate = getDate;
	t.getView = getView;
	t.option = option;
	t.trigger = trigger;
	t.updateSize = updateSize;
	t.updateEventWin = updateEventWin;
	t.editEvent = editEvent;
	t.addAndEditEvent = addAndEditEvent;
	t.addNewCalendar = addNewCalendar;
	t.addiCalCalendar = addiCalCalendar;
	t.isEditingEvent = isEditingEvent;
	t.showTodo = showTodo;
	t.showEventPageEditor = showEventPageEditor;
	t.showEventPageViewer = showEventPageViewer;


	// imports
	EventManager.call(t, options, eventSources);
	var isFetchNeeded = t.isFetchNeeded;
	var fetchEvents = t.fetchEvents;


	// locals
	var _element = element[0];
	var catlist;
	var todolist;
	var eventEditor;
    
	var eventPage;

	var header;
	var headerElement;
	var content;
	var tm; // for making theme classes
	var currentView;
	var viewInstances = {};
	var elementOuterWidth;
	var suggestedViewHeight;
	var absoluteViewElement;
	var resizeUID = 0;
	var ignoreWindowResize = 0;
	var date = new Date();
	var events = [];
	var _dragElement;
	var _confirmPopup;



	/* Main Rendering
	-----------------------------------------------------------------------------*/

	setYMD(date, options.year, options.month, options.date);


	function render(inc) {
		if (!content) {
			initialRender();
		}else{
			calcSize();
			markSizesDirty();
			markEventsDirty();
			renderView(inc);
		}
	}


	function initialRender() {
		tm = options.theme ? 'ui' : 'fc';
		element.addClass('fc');
		if (options.isRTL) {
			element.addClass('fc-rtl');
		}
		if (options.theme) {
			element.addClass('ui-widget');
		}

		var fc_container = $(
				'<div id="fc_container">' +
					'<table cellpadding="0" cellspacing="0" width="100%" class="fc_table"><tbody><tr>' +
						'<td class="fc-main"/>' +
				       // '<td class="_fc-todo-list"/>' +
					'</tr></tbody></table>'+
				    '<div class="fc-todo-list-container"></div>'+
				'</div>'
				
		);
		var fc_todo_container = fc_container.find(".fc-todo-list-container");
	    
		fc_todo_container.on('transitionend MSTransitionEnd webkitTransitionEnd oTransitionEnd', function (event) {
		    updateSize();
		});

		header = new Header(t, options);
		headerElement = header.render();
		if (headerElement && headerElement.length > 0) {
			fc_container.prepend(headerElement);
		}

		catlist = new CategoriesList(t);
		$("#calendarSidePanelContent").append(catlist.render());

		todolist = new TodoList(t);
		t.todolist = todolist;
	    fc_todo_container.append(todolist.render());

	    content = $(
	        "<div class='fc-content' style='position:relative'>" +
	            "<div class='fc-modal'/>" +
	            "</div>")
	        .appendTo(fc_container.find("td.fc-main"));

		eventEditor = new EventEditor(t, content.children(".fc-modal"));

		fc_container.appendTo(element);

		var targetView = defaults.defaultView;

		if (options.defaultView) {
			targetView = options.defaultView;
		} else {
			var savedView = $.cookie('fc_current_view');
			if(savedView && savedView.length > 0 && savedView.search(/\w/) >= 0)
				targetView = savedView;
		}

		changeView(targetView);

		$(window).resize(windowResize);

		catlist.showMiniCalendar(parseInt($.cookie('fc_show_minicalendar'), 10) !== 0);

		// needed for IE in a 0x0 iframe, b/c when it is resized, never triggers a windowResize
		if (!bodyVisible()) {
			lateRender();
		}
	    
		if (options.targetEventId) {
		    displayTargetEvent();
		}

	    function displayTargetEvent() {
	        if (!ASC.Mail.Initialized) {
	            setTimeout(displayTargetEvent, 500);
	        } else {
	            prepare();
	        }
	        
	        function prepare() {
	            var targetEvent = t.clientEvents(options.targetEventId)[0];
	            
                if (!targetEvent) return;
                    
	            if (targetEvent && !isNaN(targetEvent.objectId) && targetEvent.uniqueId) {
	                window.Teamlab.getCalendarEventById({},
                        targetEvent.objectId,
                        {
                            success: function(p, eventInfo) {
                                if (eventInfo.eventUid === targetEvent.uniqueId && eventInfo.mergedIcs) {
                                    console.info(eventInfo);
                                    var evt = parseIcs(eventInfo.mergedIcs);
                                    targetEvent = jq.extend(targetEvent, evt || {});
                                    show();
                                }
                            },
                            error: function(p, e) {
                                console.error(e);
                            },
                            max_request_attempts: 1
                        });
	            } else {
	                show();
	            }
	            
	            function show() {
	                var canDelete = fcUtil.objectIsEditable(targetEvent) || fcUtil.objectIsEditable(targetEvent.source) || targetEvent.source && !targetEvent.source.isSubscription;
	                var canEdit = targetEvent.source == undefined || canDelete;
	                showEventPageViewer(canEdit, null, targetEvent);
	            }
            }
	    }
	}


	// called when we know the calendar couldn't be rendered when it was initialized,
	// but we think it's ready now
	function lateRender() {
		setTimeout(function() { // IE7 needs this so dimensions are calculated correctly
			if (!currentView.start && bodyVisible()) { // !currentView.start makes sure this never happens more than once
				renderView();
			}
		},0);
	}


	function destroy() {
		$(window).unbind('resize', windowResize);
		header.destroy();
		content.remove();
		element.removeClass('fc fc-rtl ui-widget');
	}


	function elementVisible() {
		return _element.style.display != "none";
	}


	function bodyVisible() {
		return $('body')[0].offsetWidth !== 0;
	}

	function openTodoList() {
	    currentView.clearEvents();
	    var fc_container = $('#fc_container');
	    fc_container.toggleClass('open-todo-list');
	    fc_container.find('.fc-button-todo').toggleClass('fc-state-active');
	}


	/* View Rendering
	-----------------------------------------------------------------------------*/

	// TODO: improve view switching (still weird transition in IE, and FF has whiteout problem)

	function changeView(newViewName) {
		if (!currentView || newViewName != currentView.name) {
			ignoreWindowResize++; // because setMinHeight might change the height before render (and subsequently setSize) is reached

			unselect();

			var oldView = currentView;
			var newViewElement;

			if (oldView) {
				(oldView.beforeHide || noop)(); // called before changing min-height. if called after, scroll state is reset (in Opera)
				setMinHeight(content, content.height());
				oldView.element.hide();
			}else{
				setMinHeight(content, 1); // needs to be 1 (not 0) for IE7, or else view dimensions miscalculated
			}
			content.css('overflow', 'hidden');

			currentView = viewInstances[newViewName];
			if (currentView) {
				currentView.element.show();
			}else{
				currentView = viewInstances[newViewName] = new fcViews[newViewName](
					newViewElement =
						$("<div class='fc-view fc-view-" + newViewName + "'/>")
							.appendTo(content),
					t // the calendar object
				);
			}
			$.cookie('fc_current_view', newViewName + "", {expires: 365});

			if (oldView) {
				header.deactivateButton(oldView.name);
			}
			header.activateButton(newViewName);
			header.resize();

			renderView();

			content.css('overflow', '');
			if (oldView) {
				setMinHeight(content, 1);
			}

			if (!newViewElement) {
				(currentView.afterShow || noop)(); // called after setting min-height/overflow, so in final scroll state (for Opera)
			}

			ignoreWindowResize--;
		}
	}


	function changeViewAndMode(viewName, mode) {
		header.clickButton(viewName, mode);
	}


	function renderView(inc) {
		if (elementVisible()) {
			ignoreWindowResize++; // because renderEvents might temporarily change the height before setSize is reached

			unselect();

			if (suggestedViewHeight === undefined) {
				calcSize();
			}

			var forceEventRender = false;
			if (!currentView.start || inc || date < currentView.start || date >= currentView.end) {
				// view must render an entire new date range (and refetch/render events)
				currentView.render(date, inc || 0); // responsible for clearing events
				setSize(true);
				forceEventRender = true;
				
				// opera fix
				var unsel = jq(document).find(".fc-main");
				if ((unsel != undefined) && (unsel.length > 0)) {
					preventSelection.call(this, unsel[0]);
				}
			}
			else if (currentView.sizeDirty) {
				// view must resize (and rerender events)
				currentView.clearEvents();
				setSize();
				forceEventRender = true;
			}
			else if (currentView.eventsDirty) {
				currentView.clearEvents();
				forceEventRender = true;
			}
			currentView.sizeDirty = false;
			currentView.eventsDirty = false;
			updateEvents(forceEventRender);

			elementOuterWidth = element.outerWidth();

			header.updateTitle(currentView.title, currentView.name);
			var today = new Date();
			if (today >= currentView.start && today < currentView.end) {
				header.disableButton('today');
			}else{
				header.enableButton('today');
			}

			catlist.updateDatepicker(date);

			ignoreWindowResize--;
			currentView.trigger('viewDisplay', _element);

			trigger("viewChanged", currentView, currentView.name, getDate());
		}
	}



	/* Resizing
	-----------------------------------------------------------------------------*/
	function updateEventWin() {
	    var eventObj = eventEditor.getEventObj();
	    var todoObj = t.todolist.getTodoElem();
	    if (t.openEventWin) {
	        if (!t.openEventWin.isTodo && eventObj != undefined) {
	            if (!eventObj.objectId) {
	                eventEditor.closePopupEvent(true);
	                eventEditor.addEvent(eventObj.start, eventObj.end, eventObj.allDay, true);
	            } else {
	                var eventOpenElement = jQuery('.fc-event-open');
	                if (eventOpenElement.length > 0) {
	                    eventEditor.openEvent(eventOpenElement, eventObj);
	                }
	            }
	        } else if (t.openEventWin.objectId != null && t.openEventWin.isTodo && todoObj != undefined) {
	            var todoOpenElement = jQuery('.fc-todo-open');
	            if (todoOpenElement.length > 0) {
	                eventEditor.openEvent(todoOpenElement, todoObj);
	            }
	        }
	    }
	    
    }
	function updateSize() {
		if (currentView.name == "agendaWeek") {
		    currentView.updateHeader();
		}
		header.resize();
		markSizesDirty();
		if (elementVisible()) {
			currentView.clearEvents();
			calcSize();
			setSize();
			unselect();
			currentView.renderEvents(events);
			currentView.sizeDirty = false;
			updateFcHeader()
		}
	}


	function markSizesDirty() {
		$.each(viewInstances, function(i, inst) {
			inst.sizeDirty = true;
		});
	}


	function calcSize() {
		if (options.contentHeight) {
			suggestedViewHeight = Math.max(options.contentHeight, options.minHeight || 0);
		}
		else if (options.height) {
			if ($.isFunction(options.onHeightChange)) {options.onHeightChange();}
			suggestedViewHeight = Math.max(options.height, options.minHeight || 0) - ASC.CalendarSizeManager.cache.fcHeaderHeight;
		}
		else {
			suggestedViewHeight = Math.max(
					Math.round(content.width() / Math.max(options.aspectRatio, .5)), options.minHeight || 0);
		}
	}


	function setSize(dateChanged, bResize) { // todo: dateChanged?
		ignoreWindowResize++;
		currentView.setHeight(suggestedViewHeight, dateChanged);

		var oldWidth = content.width();

		currentView.setWidth(oldWidth, dateChanged);
		ignoreWindowResize--;

		var paddingStudioPageContent = jq("#studioPageContent .mainPageLayout:not(.studio-top-panel)").outerHeight(true) - jq("#studioPageContent .mainPageLayout:not(.studio-top-panel)").height();
		var paddingmainPageTable = jq(".mainPageTable .mainPageTableSidePanel.ui-resizable").outerHeight(true) - jq(".mainPageTable .mainPageTableSidePanel.ui-resizable").height();
		var smallChatDownPanel = jq('.small_chat_down_panel').length > 0 ? jq('.small_chat_down_panel').height() : 0;

		catlist.resize(suggestedViewHeight - paddingStudioPageContent - paddingmainPageTable - smallChatDownPanel);

		if (todolist && todolist.length > 0) {todolist.resize(suggestedViewHeight);}
		var newWidth = content.width();
		if (oldWidth != newWidth) {
			if (!bResize) {calcSize();}
			setSize(false, oldWidth > newWidth);
		}
	}

	function updateFcHeader() {
		var fcHeader = document.querySelector('.fc-header');

		if (!fcHeader || window.innerWidth > 1200) return;

		var left = fcHeader.querySelector('.fc-header-left');
		var center = fcHeader.querySelector('.fc-header-center');
		var right = fcHeader.querySelector('.fc-header-right');
		var diff = fcHeader.clientWidth - left.clientWidth - right.clientWidth;
		center.style.width = diff + "px";
	}

    function windowResize() {
        if (!ignoreWindowResize) {
            //eventEditor.closePopupEvent();
            if (currentView.start) { // view has already been rendered
                var uid = ++resizeUID;
                setTimeout(function () { // add a delay
                    if (uid == resizeUID && !ignoreWindowResize && elementVisible()) {
                        if (elementOuterWidth != (elementOuterWidth = element.outerWidth())) {
                            ignoreWindowResize++; // in case the windowResize callback changes the height
                            updateSize();
                            currentView.trigger('windowResize', _element);
                            setTimeout(function () {
                                updateEventWin();
                                ignoreWindowResize--;
                            }, 200);
                        } else {
                            updateEventWin();
                        }
                    }
                }, 200);
            } else {
                // calendar must have been initialized in a 0x0 iframe that has just been resized
                lateRender();
            }
        } else {
            setTimeout(function () {
                $(window).resize();
            }, 200);
        }
	}



	/* Event Fetching/Rendering
	-----------------------------------------------------------------------------*/

	// fetches events if necessary, rerenders events if necessary (or if forced)
	function updateEvents(forceRender) {
		if (!options.lazyFetching || isFetchNeeded(currentView.visStart, currentView.visEnd)) {
			refetchEvents();
		}
		else if (forceRender) {
			rerenderEvents();
		}
	}


	function refetchEvents() {
		fetchEvents(currentView.visStart, currentView.visEnd); // will call reportEvents
	}


	// called when event data arrives
	function reportEvents(_events) {
		events = _events;
		rerenderEvents();
	}


	// called when a single event's data has been changed
	function reportEventChange(eventID, event) {
		
	    if (event && !isNaN(event.objectId) && event.uniqueId) {
	        Teamlab.getCalendarEventById({},
	            event.objectId,
	            {
	                success: function(p, eventInfo) {
	                    if (eventInfo.eventUid === event.uniqueId && eventInfo.mergedIcs) {
	                        console.info(eventInfo);
	                        var evt = parseIcs(eventInfo.mergedIcs);
	                        event = jq.extend(event, evt || {});

	                        var isOrganizer = false;
	                        var organizerEmail = event.organizer ? event.organizer[3].replace(new RegExp("mailto:", "ig"), "").toLowerCase() : "";

	                        jq.each(ASC.Mail.Accounts, function(index, account) {
	                            if (account.enabled && organizerEmail == account.email.toLowerCase()) {
	                                isOrganizer = true;
	                                return false;
	                            }
	                            return true;
	                        });

	                        var attendees = [];
	                        jq.each(event.attendees || [], function(index, attendeeObj) {
	                            var attendeeEmail = attendeeObj[3].replace(new RegExp("mailto:", "ig"), "");
	                            if (attendeeEmail.toLowerCase() != organizerEmail)
	                                attendees.push(attendeeEmail);
	                        });

	                        if (isOrganizer && attendees.length && !event.source.isSubscription)
	                            openConfirmPopup(event);
	                        else
	                            edit(event, false);
	                    }
	                },
	                error: function(p, e) {
	                    console.error(e);
	                },
	                max_request_attempts: 1
	            });
	    } else {
	        edit(event, false);
	    }
	    
	    function createConfirmPopup(eventObj) {

	        if (!_confirmPopup) {
	            _confirmPopup = $(options.confirmPopup.dialogTemplate)
	                .addClass("asc-dialog")
	                .popupFrame({
	                    anchor: "right,top",
	                    direction: "right,down",
	                    offset: "0,0",
	                    showArrow: false
	                });

	            _confirmPopup.find(".title").text(options.confirmPopup.dialogUpdateEventHeader);
	            _confirmPopup.find(".body").text(options.confirmPopup.dialogUpdateEventBody);
	            _confirmPopup.find(".send-customs-btn, .send-everyone-btn").remove();
	        }
	        
	        _confirmPopup.find(".buttons .send-btn").unbind("click").bind("click", function() {
	            edit(eventObj, true);
	        });

	        _confirmPopup.find(".buttons .dont-send-btn").unbind("click").bind("click", function() {
	            edit(eventObj, false);
	        });

	    }

	    function openConfirmPopup(eventObj) {

	        createConfirmPopup(eventObj);
	        
	        var uiBlocker = jq(".fc-content .fc-modal");
            
            _confirmPopup.popupFrame("close");
            uiBlocker.removeClass("fc-modal-transparent");;
		
	        _confirmPopup.popupFrame("open", { pageX: "center", pageY: "center" });
	        _confirmPopup.css("position","fixed");

	        if (_confirmPopup.popupFrame("isVisible")) {
	            uiBlocker.show();
	        } else {
	            uiBlocker.hide();
	        }
	    }

	    function closeConfirmPopup() {
	        if(_confirmPopup)
	            _confirmPopup.popupFrame("close");
	        jq(".fc-content .fc-modal").hide();
	    }

	    function sendGuestsNotification(eventObj) {

	        var attendeesEmails = jq.map(eventObj.attendees, function (attendee) {
	            return attendee[3].replace(new RegExp("mailto:", "ig"), "");
	        });

	        if (attendeesEmails.length) {
	            ASC.CalendarController.Busy = true;
	            window.LoadingBanner.displayLoading();
	        
	            ASC.Mail.Utility.SendCalendarUpdate(eventObj.sourceId, eventObj.uniqueId, attendeesEmails)
                    .done(function() {
                        console.log(options.confirmPopup.dialogSuccessToastText, arguments);
                    })
                    .fail(function() {
                        toastr.error(options.confirmPopup.dialogErrorToastText);
                        console.error(options.confirmPopup.dialogErrorToastText, arguments);
                    })
	                .always(function () {
	                    ASC.CalendarController.Busy = false;
	                    window.LoadingBanner.hideLoading();
	                });
	        }
	    }

	    function edit(eventObj, send) {
	        closeConfirmPopup();
	        
	        if (eventObj && eventObj.source) {
	            // update event
	            eventObj.newSourceId = eventObj.sourceId;
	            trigger("editEvent", t,
                        $.extend(
                                {
                                    action: kEventChangeAction,
                                    sourceId: eventObj.source.objectId,
                                    newSourceId: eventObj.source.objectId
                                },
                                eventObj),
                        function(response) {
                            if (!response.result) {return;}
                            
                             var cache = t.getCache();
                             for (var j = 0; j < cache.length; ++j) {
                                 if (cache[j].objectId == eventObj.eventId) {
                                     cache.splice(j, 1);
                                 }
                             }

                            cache.concat(response.event);
                            t.refetchEvents();
                            rerenderEvents();

                            if (send)
                                sendGuestsNotification(eventObj);
                        });
	            return;
	        }
	        rerenderEvents(eventID);
	    }
	}


	// attempts to rerenderEvents
	function rerenderEvents(modifiedEventID) {
		markEventsDirty();
		if (elementVisible()) {
			currentView.clearEvents();
			currentView.renderEvents(events, modifiedEventID);
			currentView.eventsDirty = false;
			//catlist.updateDatepickerCells();
		}
	}


	function rerenderCategories(resolveTodoList) {
		catlist.rerenderList();
		catlist.updateDatepickerSize();
		todolist.rerenderList(resolveTodoList);
		//if (todolist && todolist.length > 0) {todolist.rerenderList();}
	}


	function markEventsDirty() {
		$.each(viewInstances, function(i, inst) {
			inst.eventsDirty = true;
		});
	}


	function editEvent(elem, event) {
		eventEditor.openEvent(elem, event);
	}


	function addAndEditEvent(startDate, endDate, allDay) {
		eventEditor.addEvent(startDate, endDate, allDay);
	}
	
	function addiCalCalendar(anchor) {
		catlist.addiCalCalendar(anchor);
	}
	
	function addNewCalendar(anchor) {
		catlist.addNewCalendar(anchor);
	}


	function isEditingEvent() {
		return eventEditor.isVisible();
	}


	function showTodo(show) {
		if (todolist && todolist.length > 0) {
			return show === false ? todolist.hide() : todolist.show();
		}
		return false;
	}

	function initEventPage() {
		if (eventPage) return;
		eventPage = new EventPage(t);
	}

	function showEventPageEditor() {
		initEventPage();
		if (jq("#asc_event .event-editor .editor").is(":visible")) {
			if(confirm(ASC.Resources.Master.Resource.WarningMessageBeforeUnload))
				eventPage.addEvent();
		} else {
			eventPage.addEvent();
		}
	}

	function showEventPageViewer(canEdit, elem, event) {
		initEventPage();
		eventPage.openEvent(canEdit, elem, event);
	}


	/* Selection
	-----------------------------------------------------------------------------*/

	function select(start, end, allDay) {
		currentView.select(start, end, allDay===undefined ? true : allDay);
	}


	function unselect() { // safe to be called before renderView
		if (currentView) {
			currentView.unselect();
		}
	}



	/* Date
	-----------------------------------------------------------------------------*/

	function prev(inc) {
		renderView(undefined === inc ? -1: inc);
	}


	function next(inc) {
		renderView(undefined === inc ? 1: inc);
	}


	function prevYear() {
		addYears(date, -1);
		renderView();
	}


	function nextYear() {
		addYears(date, 1);
		renderView();
	}


	function today() {
		date = new Date();
		renderView();
	}


	function gotoDate(year, month, dateOfMonth) {
		if (year instanceof Date) {
			date = cloneDate(year); // provided 1 argument, a Date
		}else{
			setYMD(date, year, month, dateOfMonth);
		}
		renderView();
	}


	function incrementDate(years, months, days) {
		if (years !== undefined) {
			addYears(date, years);
		}
		if (months !== undefined) {
			addMonths(date, months);
		}
		if (days !== undefined) {
			addDays(date, days);
		}
		renderView();
	}


	function getDate() {
		return cloneDate(date);
	}



	/* Misc
	-----------------------------------------------------------------------------*/

	function getView() {
		return currentView;
	}


	function option(name, value) {
		if (value === undefined) {
			return options[name];
		}
		if (name == 'height' || name == 'contentHeight' || name == 'aspectRatio') {
			options[name] = value;
			updateSize();
		}
		return value;
	}


	function trigger(name, thisObj) {
		if (options[name]) {
			return options[name].apply(
				thisObj || _element,
				Array.prototype.slice.call(arguments, 2)
			);
		}
		return undefined;
	}



	/* External Dragging
	------------------------------------------------------------------------*/

	if (options.droppable) {
		$(document)
			.bind('dragstart', function(ev, ui) {
				var _e = ev.target;
				var e = $(_e);
				if (!e.parents('.fc').length) { // not already inside a calendar
					var accept = options.dropAccept;
					if ($.isFunction(accept) ? accept.call(_e, e) : e.is(accept)) {
						_dragElement = _e;
						currentView.dragStart(_dragElement, ev, ui);
					}
				}
			})
			.bind('dragstop', function(ev, ui) {
				if (_dragElement) {
					currentView.dragStop(_dragElement, ev, ui);
					_dragElement = null;
				}
			});
	}


}


function Header(calendar, options) {
	var _this = this;

	// locals
	var element = $([]);
	var tm;

	var _viewSelectorLabel;
	var _viewSelectorIcon;
	var _labels = [];
	var _modes = [];
	var _classes = [];
	var _switchTable = [];
	var _nameToMode = {};
	var _activeButtonName;
	var _activeMode;


	var formatDate = calendar.formatDate;
	var formatDates = calendar.formatDates;


	function _renderButton(buttonName) {
		var buttonClick;
		var button;

		if (calendar[buttonName]) {
			buttonClick = calendar[buttonName]; // calendar method
		}
		else if (fcViews[buttonName]) {
			buttonClick = function() {
				button.removeClass(tm + '-state-hover'); // forget why
				calendar.changeView(buttonName);
			};
		}else if (buttonName == 'todo') {
		    buttonClick = function() {
		        calendar.openTodoList();
		    };
		}

		if (buttonClick) {
			var icon = options.theme ? smartProperty(options.buttonIcons, buttonName) : null; // why are we using smartProperty here?
			var text = smartProperty(options.buttonText, buttonName); // why are we using smartProperty here?
			button = $(
				"<span class='fc-button fc-button-" + buttonName + " " + tm + "-state-default'>" +
					"<span class='fc-button-inner'>" +
						"<span class='fc-button-content'>" +
							(icon ?
								"<span class='fc-icon-wrap'>" +
									"<span class='ui-icon ui-icon-" + icon + "'/>" +
								"</span>" :
								text
								) +
						"</span>" +
					"</span>" +
				"</span>"
			);
			if (button) {
				button
					.click(function() {
						if (!button.hasClass(tm + '-state-disabled')) {
							buttonClick();
						}
					})
					.mousedown(function() {
						button
							.not('.' + tm + '-state-active')
							.not('.' + tm + '-state-disabled')
							.addClass(tm + '-state-down');
					})
					.mouseup(function() {
						button.removeClass(tm + '-state-down');
					})
					.hover(
						function() {
							button
								.not('.' + tm + '-state-active')
								.not('.' + tm + '-state-disabled')
								.addClass(tm + '-state-hover');
						},
						function() {
							button
								.removeClass(tm + '-state-hover')
								.removeClass(tm + '-state-down');
						}
					)
					.addClass(tm + '-corner-left')
					.addClass(tm + '-corner-right');
			}
		}
		return button;
	}

	function _renderSection(position) {
		var e = $("<div class='fc-header-" + position + "'/>");
		var buttonStr = options.header[position];
		if (buttonStr) {
			$.each(buttonStr.split(' '), function(i) {
				if (i > 0) {
					e.append("<span class='fc-header-space'/>");
				}
				$.each(this.split(','), function(j, buttonName) {
					if (buttonName == 'title') {
						e.append("<span class='fc-header-title'>&nbsp;</span>");
					}
					else {
						var button = _renderButton.call(_this, buttonName);
						if (button) {
							button.appendTo(e);
						}
					}
				});
			});
		}
		return e;
	}

	function _renderTodoLabel(elem) {
		var visible = true;
		$("<span class='todo-label'>" +
				htmlEscape(options.todoList.hideLabel) +
			"</span>")
			.click(function() {
				if (visible) {
					$(this).text(options.todoList.showLabel);
				} else {
					$(this).text(options.todoList.hideLabel);
				}
				visible = !visible;
				calendar.showTodo(visible);
			})
			.appendTo(elem.find(".fc-header-right"));
	}

	function _renderNewSelectorLabel(elem) {
	    var mainButtonText = htmlEscape(calendar.options.addNewLabel);

	    $('<ul class="menu-actions">' +
        '<li class="menu-main-button without-separator big" title="' + mainButtonText + '">' +
            '<span class="main-button-text">' + mainButtonText + '</span>' +
            '<span class="white-combobox">&nbsp;</span>'+
        '</li>'+
        '</ul>').appendTo(elem);

		var _addSelectorLabel = elem.find(".menu-main-button");

		_addSelectorLabel.on("click", function (event) {
			fcMenus.hideMenus(fcMenus.modeMenuAddNew);
			fcMenus.modeMenuAddNew.popupMenu("open", _addSelectorLabel);
			event.stopPropagation();
		});

		if (!fcMenus.modeMenuAddNew || fcMenus.modeMenuAddNew.length < 1) {
			fcMenus.modeMenuAddNew = $('<div id="fc_mode_menu_add_new"/>');
		} else {
			fcMenus.modeMenuAddNew.popupMenu("close");
			fcMenus.modeMenuAddNew.popupMenu("destroy");
		}

		fcMenus.modeMenuAddNew.popupMenu({
			anchor: "left,bottom",
			direction: "right,down",
			arrow: "up",
			showArrow: false,
			closeTimeout: -1,
			cssClassName: "asc-popupmenu",
			items: [
				{
					label: calendar.options.eventEditor.newEventTitle,
					click: function() {
					    calendar.showEventPageEditor();
					}
				},
				{
					label: calendar.options.categories.defaultTitle,
					click: function() {
						calendar.addNewCalendar( {pageX: "center", pageY: "center"} );
					}
				},
			    {
					label: '',
					cssClass: "dropdown-item-seporator"
				},
				{
					label: calendar.options.icalStream.importEventsTitle,
					click: function() {
					    calendar.addiCalCalendar( {pageX: "center", pageY: "center"} );
					},
				}
			]
		});
	}

	function _renderToday(elem) {
		var op = calendar.options.categories;
		var today = new Date();
		var day;
	    
		var daynames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
		if (window.moment) {
		    day = window.moment(today).calendar(null, {
		        lastWeek: 'dddd DD.MM.YYYY',
		        sameElse: 'dddd DD.MM.YYYY',
		        nextWeek: 'dddd DD.MM.YYYY'
		    }).split(' ')[0];
		  
		} else {
		    day = daynames[today.getDay()];
		}
	    
		var r = $(
				'<div class="fc-header-today">' +
					'<div class="date-box">' +
				        '<div class="today">' + day + ', ' + '</div>' +
						'<div class="today-day">' + htmlEscape(formatDate(today, op.dayFormat)) + '</div>' +
						'<div class="today-month">' + htmlEscape(formatDate(today, 'MMM')) + '</div>' +
						//'<div class="today-year">' + htmlEscape(formatDate(today, op.yearFormat)) + '</div>' +
					'</div>' +
				'</div>');
		r.find(".date-box").children().click(function() {calendar.today();});
		r.appendTo(elem.find(".fc-header-center"));
	}

	function _updateViewSelector(mode) {
		_viewSelectorLabel.text(_labels[mode]);
		_viewSelectorIcon
				.removeClass()
				.addClass("icon")
				.addClass(_classes[mode]);
		for (var i = 0; i < _modes.length; ++i) {
			_modes[i].css("display", i != mode ? "none" : "inline-block");
		}
		_activeMode = mode;
	    _updateFcHeader();

	}

	function _updateFcHeader() {
	    var header = document.querySelector('.fc-header');

	    if (!header || window.innerWidth > 1200) return;

	    setTimeout(function () {
	        var left = header.querySelector('.fc-header-left');
	        var center = header.querySelector('.fc-header-center');
	        var right = header.querySelector('.fc-header-right');
	        var diff = header.clientWidth - left.clientWidth - right.clientWidth;
	        center.style.width = diff + "px";
	    },100);
	}

	function _setCurrentView(mode) {
		_updateViewSelector(mode);
		if (_activeButtonName && _activeButtonName.length > 0) {
			_this.clickButton(_activeButtonName, mode);
		}
	}

	function _renderViewSelectors(elem) {
		elem.find(".fc-header-right").append(
				/*'<span class="fc-view-selector">' +
					'<span class="icon">&nbsp;</span>' +
					'<span class="label">' + htmlEscape(options.modes.calendarViewLabel) + '</span>' +
					'<span class="fc-dropdown">&nbsp;</span>' +
				'</span>' + */
				'<span class="fc-calendar-buttons"/>' +
				'<span class="fc-list-buttons"/>');
		_modes[0] = elem.find(".fc-calendar-buttons")
				.append(_renderButton.call(_this, "agendaDay"))
				.append(_renderButton.call(_this, "agendaWeek"))
				.append(_renderButton.call(_this, "month"))
                .append(_renderButton.call(_this, "list"))
                .append(_renderButton.call(_this, "todo"));
		_modes[1] = elem.find(".fc-list-buttons")
				.append(_renderButton.call(_this, "listDay"))
				.append(_renderButton.call(_this, "listWeek"))
				.append(_renderButton.call(_this, "listMonth"))
				.append(_renderButton.call(_this, "list"));
		_labels[0]  = options.modes.calendarViewLabel;
		_labels[1]  = options.modes.listViewLabel;
		_classes[0] = "calendar";
		_classes[1] = "list";
		_switchTable[0] = {
		    listDay:    "agendaDay",
		    todoList:   "todo",
			listWeek:   "agendaWeek",
			listMonth:  "month",
			list:       "month",
			day:        "agendaDay"
		};
		_switchTable[1] = {
		    agendaDay:  "listDay",
		    todo:       "todoList",
			agendaWeek: "listWeek",
			month:      "listMonth",
			day:        "listDay"
		};
		_nameToMode = {
			agendaDay:  0,
			agendaWeek: 0,
			month:      0,
		    todo:       0,
			listDay:    1,
			listWeek:   1,
			listMonth:  1,
			list:       0
		};

		_viewSelectorLabel = elem.find(".fc-view-selector .label");
		_viewSelectorIcon = elem.find(".fc-view-selector .icon");

		elem.find(".fc-view-selector .label, .fc-view-selector .fc-dropdown").click(
			function (event) {
				fcMenus.hideMenus(fcMenus.modeMenu);
				fcMenus.modeMenu.popupMenu("open", _viewSelectorLabel);
				event.stopPropagation();
			});

		if (!fcMenus.modeMenu || fcMenus.modeMenu.length < 1) {
			fcMenus.modeMenu = $('<div id="fc_mode_menu"/>');
		} else {
			fcMenus.modeMenu.popupMenu("close");
			fcMenus.modeMenu.popupMenu("destroy");
		}
		fcMenus.modeMenu.popupMenu({
			anchor: "left,bottom",
			direction: "right,down",
			arrow: "up",
			showArrow: false,
			closeTimeout: -1,
			cssClassName: "asc-popupmenu",
			items: [
				{
					label: _labels[0],
					click: function(event, data) {_setCurrentView.call(_this, 0);}
				},
				{
					label: _labels[1],
					click: function(event, data) {_setCurrentView.call(_this, 1);}
				}
			]
		});

		_setCurrentView.call(_this, 0);
	}



	this.render = function() {
		tm = options.theme ? 'ui' : 'fc';
		var sections = options.header;
		if (sections) {
			element = $('<div class="fc-header-outer"/>')
					.append(
						$('<div class="fc-header"/>')
							.append(_renderSection.call(this, "left"))
							.append(_renderSection.call(this, "center"))
							.append(_renderSection.call(this, "right"))
					);
			// temporary desabled
			//_renderTodoLabel.call(this, element);
			_renderViewSelectors.call(this, element);
			_renderNewSelectorLabel.call(this, $("#calendarSidePanelActions"));
			_renderToday.call(this,element);
			return element;
		}
		return null;
	};


	this.destroy = function() {
		element.remove();
	};

	this.updateTitle = function(newTitle, viewName) {
		var title = element.find('span.fc-header-title').empty();
		if (typeof(newTitle) == "string") {
			title.append($("<h2>" + htmlEscape(newTitle) + "</h2>"));
		} else {
			var nt = $(newTitle).clone(true);
			title.append(nt.children());
		}
		title.removeClass().addClass("fc-header-title");
		var parts = viewName.split(/(?=[A-Z])/);
		var c = parts.pop();
		if (c != undefined) {
			title.addClass(c.toLowerCase());
		}
	};

	this.activateButton = function(buttonName) {
		_activeButtonName = buttonName;
		_updateViewSelector(_nameToMode[buttonName] !== undefined ? _nameToMode[buttonName] : 0);
		element.find('span.fc-button-' + buttonName)
			.addClass(tm + '-state-active');
	};

	this.deactivateButton = function(buttonName) {
		element.find('span.fc-button-' + buttonName)
				.removeClass(tm + '-state-active');
	};

	this.disableButton = function(buttonName) {
		element.find('span.fc-button-' + buttonName)
			.addClass(tm + '-state-disabled');
	};

	this.enableButton = function(buttonName) {
		element.find('span.fc-button-' + buttonName)
			.removeClass(tm + '-state-disabled');
	};

	this.clickButton = function(buttonName, mode) {
		element.find(".fc-button-" +
				_switchTable[mode != undefined ? mode : _activeMode][buttonName]).click();
	};

	this.resize = function() {
		element.find(".fc-header-left")[0].style.width = "";
		element.find(".fc-header-center")[0].style.width = "";
	};

}


function PermissionsList(calendar, container, linkText) {
	var _this = this;

	// Trigger self events:
	//   onChange(permissions)
	//   onResize()
	//
	// Trigger calendar's events:
	//   editPermissions(param)
	//   removePermissions(param)

	var _handlers = {};

	function _trigger(eventName) {
		if (_handlers && $.isFunction(_handlers[eventName])) {
			_handlers[eventName].apply(
					_handlers.context != undefined ? _handlers.context : window,
					Array.prototype.slice.call(arguments, 1));
		}
	}

	this.setEventsHandlers = function(handlers) {
		if (handlers != undefined) {
			$.extend(_handlers, handlers);
		}
	};


	var _userList;
	var _moreText;
	var _moreLink;
	var _shortListLen;

	var _permissions;
	var _type;
	var _objectId;


	(_create());

	function _create() {
		var sl = calendar.options.sharedList;
		container.append(
				'<div class="empty-list">' +
					'<span class="add-icon"/>' +
					'<span class="link">' + htmlEscape(linkText) + '</span>' +
				'</div>' +
				'<div class="users-list">' +
					'<div class="label">' + htmlEscape(sl.title) + '</div>' +
					'<div class="short-list"/>' +
					'<div class="more-users">' +
						'<span class="link">' + htmlEscape(sl.moreLink) + '</span>' +
					'</div>' +
					'<div class="add-users">' +
						'<span class="add-icon"/>' +
						'<span class="link">' + htmlEscape(sl.addLink) + '</span>' +
					'</div>' +
				'</div>');
		_userList = container.find(".users-list .short-list");
		_moreLink = container.find(".more-users .link");
		_moreText = sl.moreLink;
		_shortListLen = sl.shortListLength > 0 ? sl.shortListLength : 5;
		container
				.find(".empty-list .link, .add-users .link, .add-users .add-icon")
				.click(_changePermissions);
	}

	function _changePermissions() {
		calendar.trigger("editPermissions", _this,
				{
					type:       _type,
					objectId:   _objectId,
					permissions: _permissions
				},
				_updatePermissions);
	}

	function _deletePermission() {
		var i = $(this).parent().index();
		calendar.trigger("removePermissions", _this,
				{
					type:        _type,
					objectId:    _objectId,
					userId:      _permissions.users[i].objectId,
					permissions: _permissions
				},
				_updatePermissions);
	}

	function _updatePermissions(response) {
		if (response.result) {
			_trigger.call(_this, "onChange", response);
		}
	}


	function _renderUser(user) {
		var item =
					'<span class="user">' +
						'<span class="icon">&nbsp;</span>' +
						'<span class="name">' + htmlEscape(user.name) + '</span>' +
						'<span class="remove-btn">&nbsp;</span>' +
					'</span> ';
		return item;
	}

	this.render = function(permissions, type, objectId) {
		_permissions = permissions;
		_type = type;
		_objectId = objectId;

		container.empty();
		_create();

		container.removeClass("has-users more-users");
		_userList.find(".user").remove();
		_moreLink.unbind("click");

		if (permissions && permissions.users && permissions.users.length > 0) {
			container.show();
			container.addClass("has-users");
			var list = "";
			var otherList = "";
			for (var i = 0; i < permissions.users.length; ++i) {
				var item = _renderUser.call(_this, permissions.users[i]);
				if (i < _shortListLen) {
					list += item;
				} else {
					otherList += item;
				}
			}
			_userList.append(list);
			_userList.find(".user .remove-btn").click(_deletePermission);
			var moreCount = permissions.users.length - _shortListLen;
			if (moreCount > 0) {
				container.addClass("more-users");
				_moreLink
						.text(_moreText.replace("%d", moreCount))
						.click(
								function(ev) {
									$(this).unbind("click");
									container.removeClass("more-users");
									_userList.append(otherList);
									_userList.find(".user .remove-btn").click(_deletePermission);
									_trigger.call(_this, "onResize");
								});
			}
		}
	};

}


function CategoryDialog(calendar) {
	var _this = this;

	// Trigger dialog's events:
	//   onClose(elem, source, changed)
	//   onDelete(source)

	// Trigger calendar's events:
	//   editPermissions(param)
	//   removePermissions(param)

	var _handlers = {};

	function _trigger(eventName) {
		if (_handlers && $.isFunction(_handlers[eventName])) {
			_handlers[eventName].apply(
					_handlers.context != undefined ? _handlers.context : window,
					Array.prototype.slice.call(arguments, 1));
		}
	}

	this.setEventsHandlers = function(handlers) {
		if (handlers != undefined) {
			$.extend(_handlers, handlers);
		}
	};

	var _dialog;
	
	var _icalStream;
	var ic = calendar.options.icalStream;
	
	var _permissionsList;

	var _anchor;
	var _elem;
	var _source;
	var _permissions;

	var kCreateMode    = 1;
	var kEditMode      = 2;
	var kCreateUrlMode = 3;
	var kEditUrlMode = 4;
	var kCreateUrlWithoutSyncMode = 5;
    var kEditModeFile = 6;

    var configuration = {
        isSyncWithCalendar: 0,
        isNewCalendar: 0,
        isExportLink: 0,
        isExportFile: 0
    };

	var currentMode;

	var kTitle        = 1;
	var kColorBox     = 11;
	var kTextColorBox = 12;
	
	(function _create() {
		_dialog = $(calendar.options.categories.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("fc-shadow")
				.popupFrame({
					anchor: "left,bottom",
					direction: "right,down",
					showArrow: false,
					showModal: true,
					beforeClose: function() {_close.call(_this, false);return false;}
				});
	    _updateConfiguration();
		_dialog.find(".ical-export .ical-link").click(function() {
			_openIcalStream.call(_this, {pageX: "center", pageY: "center"});
		});
		_dialog.find(".export .export-link").click(function () {
            _openExportStream.call(_this, { pageX: "center", pageY: "center" });
		    _dialog.popupFrame('hide');
		});
       
	
		_dialog.find(".buttons .save-btn").click(function() {
		    if (jq(this).hasClass("disable"))
		        return;
		    
		    _close.call(_this, true);
		});
		_dialog.find(".buttons .cancel-btn, .header .close-btn").click(function() {
			_close.call(_this, false);
		});
		_dialog.find(".buttons .delete-btn").click(function() {
			_trigger.call(_this, "onDelete", _source);
			_close.call(_this, false, true);
		});
		//
        _getControl(kTitle).keyup(function (val) {
            fcUtil.validateInput(this, fcUtil.validateNonemptyString);
            var str = jq(this).val();
            if (str.search(ASC.CalendarController.characterRegExp) != -1) {
                jq(this).val(ASC.CalendarController.ReplaceSpecCharacter(str));
                ASC.CalendarController.displayInfoPanel(calendar.options.categories.dialogErrorMassageSpecCharacter.format(ASC.CalendarController.characterString), true);
            }
        });
		_dialog.find(".ical-url-input input").keyup(function() {
			fcUtil.validateInputHttp(this, fcUtil.validateNonemptyString);
		});
		
		_getControl(kColorBox).click(function() {fcColorPicker.open(this, _setColor, fcColors.DefaultPicker);});
		_getControl(kTextColorBox).click(function() {fcColorPicker.open(this, _setTextColor,fcColors.TextPicker);});
		//
		_permissionsList = new PermissionsList(calendar, _dialog.find(".shared-list"),
				calendar.options.sharedList.addLink);
		_permissionsList.setEventsHandlers({
					context:   _this,
					onResize: function() {_dialog.popupFrame("updatePosition", _anchor);},
					onChange: _updatePermissions
				});
		
	}());
	
	(function _createIcal() {
		_icalStream = $(ic.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("add-popup")
				.popupFrame({
					anchor: "right,top",
					direction: "right,down",
					offset: "0,0",
					showArrow: false
				});
		
		_icalStream.find(".buttons .cancel-btn, .header .close-btn").click(function () {
			_closeIcalStream.call(_this, false);
		});
        _icalStream.find((".url-link .control input")).click(function () {
            $(this).select();
        });
        _icalStream.find((".url-link .control .button.copy")).click(function () {
            var control = $($(this)[0].parentNode);
            if (!control.hasClass('disabled')) {
                control.find('input').select();
                try {
                    document.execCommand('copy');
                    ASC.CalendarController.displayInfoPanel(calendar.options.categories.dialogCopyMessage, false);
                } catch (err) { }
            }
        });
        _icalStream.find((".url-link .control .button.try-again")).click(function () {
            getCaldavLink();
        });
	}());

	function _getControl(controlId) {
		switch (controlId) {
			case kTitle:return _dialog.find(".title input");
			case kColorBox:return _dialog.find(".color .inner");
			case kTextColorBox:return _dialog.find(".color .inner-for-text");
		}
		return undefined;
	}

	function _setIconColor(newColor, icon) {
		var c = fcUtil.parseCssColor(newColor);
		var bc = ((Math.round(c.r * 0.9) << 16) | (Math.round(c.g * 0.9) << 8) | Math.round(c.b * 0.9)).toString(16);
		while (bc.length < 6) {bc = "0" + bc;}
		icon.css("background-color", newColor).css("border-color", "#" + bc);
		icon.parent().css("border-color", "#" + bc);
	}

	function _setColor(newColor) {
	    _setIconColor(newColor, _getControl(kColorBox));
		_dialog.find(".title .bullet").css("background", newColor);
	}

	function _setTextColor(newColor) {
		_setIconColor(newColor, _getControl(kTextColorBox));
	}

	function _setMode(mode, isImport) {
		
		currentMode = mode;
		
		switch (mode) {
			case kCreateMode:
			case kCreateUrlMode:
				_dialog.removeClass("edit-mode"); break;
			
			case kEditMode:
				_dialog.addClass("edit-mode"); break;
		}
		if (isImport) {
		    _dialog.addClass("import");
		} else {
		    _dialog.removeClass("import");
		}
	}


	function _updatePermissions(response) {
		if (response.result) {
			_permissions = response.permissions;
			if (response.isShared != undefined) {
				_source.isShared = response.isShared;
			}
			_permissionsList.render(_permissions, kCalendarPermissions, _source.objectId);
			_dialog.popupFrame("updatePosition", _anchor);
		}
	}

	function _updateConfiguration() {
	    var configurationCode = '';
	  
	    _dialog.find(".choose_event_source").removeClass("hidden");
	    _dialog.find(".calendar").removeClass("hidden");
	    
	    for (key in configuration) {
	        configurationCode += configuration[key];
	    }
	    switch (configurationCode) {
	        case "1110":
	            _dialog.find(".title").removeClass("hidden");
	            _dialog.find(".color").removeClass("hidden");
	            _dialog.find(".shared-list").addClass("hidden"); 
	            _dialog.find(".row").addClass("hidden");
	            _dialog.find(".ical").addClass("hidden");
	            _dialog.find(".sync-with-calendar").removeClass("hidden");
	            _dialog.find(".ical-url-input").removeClass("hidden");
	            
	            _dialog.find(".calendar select").val(-1);
	            _dialog.find(".calendar select").prop('disabled', true);
	            _dialog.find(".calendar .bullet").css("color", "transparent");
	            
	            _setMode.call(_this, kCreateUrlMode, true);
	            break;
	        case "0110":
	            _dialog.find(".title").removeClass("hidden");
	            _dialog.find(".color").removeClass("hidden");
	            _dialog.find(".shared-list").removeClass("hidden");
	            _dialog.find(".row").removeClass("hidden");
	            _dialog.find(".sync-with-calendar").removeClass("hidden");
	            _dialog.find(".ical-url-input").removeClass("hidden");
	            _dialog.find(".ical").addClass("hidden");
	            
	            _dialog.find(".calendar select").prop('disabled', false);
	            
	            _setMode.call(_this, kCreateUrlWithoutSyncMode, true);
	            break;
	        case "0010":
	            _dialog.find(".title").addClass("hidden");
	            _dialog.find(".color").addClass("hidden");
	            _dialog.find(".shared-list").addClass("hidden");
	            _dialog.find(".row").addClass("hidden");
	            _dialog.find(".ical").addClass("hidden");
	            _dialog.find(".sync-with-calendar").removeClass("hidden");
	            _dialog.find(".ical-url-input").removeClass("hidden");
	            
	            _dialog.find(".calendar select").prop('disabled', false);

	            _setMode.call(_this, kEditUrlMode, true);
	            break;
	        case "0101":
	            _dialog.find(".title").removeClass("hidden");
	            _dialog.find(".color").removeClass("hidden");
	            _dialog.find(".shared-list").removeClass("hidden");
	            _dialog.find(".row").removeClass("hidden");
	            _dialog.find(".sync-with-calendar").addClass("hidden");
	            _dialog.find(".ical-url-input").addClass("hidden");
	            _dialog.find(".ical").removeClass("hidden");
	            
	            _dialog.find(".calendar select").prop('disabled', false);

	            _setMode.call(_this, kCreateMode, true);
	            break;
	        case "0001":
	            _dialog.find(".title").addClass("hidden");
	            _dialog.find(".color").addClass("hidden");
	            _dialog.find(".shared-list").addClass("hidden");
	            _dialog.find(".row").addClass("hidden");
	            _dialog.find(".sync-with-calendar").addClass("hidden");
	            _dialog.find(".ical-url-input").addClass("hidden");
	            _dialog.find(".ical").removeClass("hidden");
	            
	            _dialog.find(".calendar select").prop('disabled', false);

	            _setMode.call(_this, kEditModeFile, true);
	            break;
	        case "0000":
	            _dialog.find(".title").removeClass("hidden");
	            _dialog.find(".color").removeClass("hidden");
	            _dialog.find(".shared-list").removeClass("hidden");
	            _dialog.find(".row").removeClass("hidden");
	            _dialog.find(".choose_event_source").addClass("hidden");
	            _dialog.find(".ical").addClass("hidden");
	            _dialog.find(".calendar").addClass("hidden");
	            _dialog.find(".sync-with-calendar").addClass("hidden");
	            break;
	        default:
	           
	    }
    }

	function _open(anchor) {
		$(document).bind("keyup", _checkEscKey);
		_dialog.popupFrame("open", anchor);
	    
		_dialog.find('#ical-browse-btn').show();
		_dialog.find(".ical-file-selected").hide();
		_dialog.find(".ical-file-del").hide();
	    
		_dialog.find(".ical-url-input input").val("");

		_dialog.find(".choose_event_source span").removeClass('active');
		_dialog.find("#events_link").addClass('active');
		_dialog.find(".buttons .save-btn").removeClass("disable");

		_dialog.find(".sync-with-calendar input").prop('checked', false);

		_dialog.find(".sync-with-calendar input").click(function() {
		    configuration.isSyncWithCalendar = +(_dialog.find(".sync-with-calendar input").is(":checked"));
		    configuration.isNewCalendar = 1;
		    
		    _updateConfiguration();
		});
		_dialog.find(".choose_event_source span").click(function () {
		    $(".choose_event_source span").removeClass('active');
		    $(this).addClass('active');
		    if ($(this)[0].id == "events_link") {
		        _setMode.call(_this, kCreateUrlMode);
		        configuration.isExportLink = 1;
		        configuration.isExportFile = 0;
		        _dialog.find(".buttons .save-btn").removeClass("disable");
		    } else {
		        _setMode.call(_this, kEditModeFile);
		        configuration.isExportLink = 0;
		        configuration.isExportFile = 1;
		        configuration.isSyncWithCalendar = 0;
                
		        if (!AjaxUploader.isChanged) _dialog.find(".buttons .save-btn").addClass("disable");
		        
		        _dialog.find(".sync-with-calendar input").prop('checked', false);
		    };
		    _updateConfiguration();
		});
	    
		_dialog.find(".calendar select").change(function (ev) {
		    var v = $(this).val();
		    var s = calendar.getEventSources();
		   
		    for (var i = 0; i < s.length; ++i) {
		        if (s[i].objectId != v) {
		            if (v == -1) {
		                _dialog.find(".calendar .bullet").css("background", "transparent");
		                _dialog.find(".title input").focus();
		                
		                configuration.isNewCalendar = 1;
		                var opt = calendar.options;
		                var categories = opt.categories;
		                var newColor = fcUtil.parseCssColor(fcUtil.randomHex(true));
		                var newBg = fcUtil.parseCssColor(fcUtil.randomHex(false));
		                var newBor = fcUtil.changeColorLightness(newBg.r, newBg.g, newBg.b, opt.eventBg2BorderRatio);

		                _elem = undefined;
		                _source = {
		                    title: categories.defaultTitle,
		                    textColor: newColor.origColor,
		                    backgroundColor: newBg.origColor,
		                    borderColor: newBor.color,
		                    isHidden: false,
		                    events: [],
		                    todos: [],
		                    permissions: { users: [] },
		                    defaultAlert: { type: 0 },
		                    timeZone: $.extend({}, opt.defaultTimeZone)
		                };
		                _anchor = anchor;
		            };
		            _updateConfiguration();
		            continue;
		        }
		        configuration.isNewCalendar = 0;
		        _source = calendar.getEventSources()[i];
                _elem = s[1].isTodo === 0 ? $('.categories .content-li')[(i - 1)] : $('.categories .content-li')[(i - 2)];
		        _updateConfiguration();
		        _dialog.find(".calendar .bullet").css("background", s[i].backgroundColor);
                return;
		    }
		});
		// check length
		var titleMaxLen = defaults.eventMaxTitleLength;
		var inputTxt = _dialog.find(".title input");
		if (inputTxt.length > 0)
		{
			inputTxt.keyup(function(){
				if (inputTxt.val().length > titleMaxLen){
					inputTxt.val(inputTxt.val().substr(0, titleMaxLen));
				}
			});
		}
		//

	    inputTxt.focus().select();

		// upload ajax
		AjaxUploader = new AjaxUpload('#ical-browse-btn', {
			action: 'upload.php',
			autoSubmit: false,
			//responseType: "xml",
			onChange: function(file, extension) {
				
				if ( !(extension && ( /^(ics)$/.test(extension) || /^(txt)$/.test(extension) )) ){
					_dialog.find(".ical-file-selected").html(ic.dialog_incorrectFormat).css("color", "#cc3300");
					return;
				} 
				this.isChanged = true;
			    _dialog.find(".buttons .save-btn").removeClass("disable");
				_dialog.find('#ical-browse-btn').hide();
			    _dialog.find(".ical-file-del").show();
				_dialog.find(".ical-file-selected").show().html(file).css("color", "grey");
			},
			onSubmit: function (file, extension) {
				LoadingBanner.displayLoading(); 
			},
			onComplete: function (file, response) {
				LoadingBanner.hideLoading(true);
				calendar.refetchEvents();
			}
		});

	    _dialog.find(".ical-file-del").click(function() {
	        AjaxUploader.isChanged = false;
	        _dialog.find(".buttons .save-btn").addClass("disable");
	        _dialog.find('#ical-browse-btn').show();
	        _dialog.find(".ical-file-del").hide();
	        _dialog.find(".ical-file-selected").hide().html('');
	    });
	}
	
	function _openIcalStream(anchor) {
		$(document).bind("keyup", _checkEscKeyIcalStream);
		
		calendar.trigger("getiCalUrl", _this, _source.objectId, function(response){
			if (response.result) {
				var link = "<a href=" + response.url + " target='_blank'>" + response.url + "</a>";
				_icalStream.find(".saved-url-link").html(link);
			}
		});
		
		_icalStream.width(_dialog.width() + 160);
		_icalStream.popupFrame("open", anchor);
	}

    var timerId;
    function getCaldavLink() {
        clearInterval(timerId);
        var linkContainer = _icalStream.find(".url-link.caldav");
        var helpLink = _icalStream.find(".caldav-help");

        linkContainer.find(".control").removeClass('success').removeClass('failure').addClass('disabled').addClass('processing');
        linkContainer.find(".control input").attr('disabled', 'disabled');

        linkContainer.show();
        helpLink.show();

        var counter = 0, preparingMessage = ic.dialogPreparingMessage, preparingErrorMessage = ic.dialogPreparingErrorMessage;
        timerId = setInterval(function () {
            preparingMessage = preparingMessage + ".";
            if (counter == 4) {
                preparingMessage = ic.dialogPreparingMessage;
                counter = 0;
            }
            linkContainer.find(".control input").val(preparingMessage);
            counter++;
        }, 500);

        calendar.trigger("getCaldavUrl", _this, _source.objectId, function (response) {
            clearInterval(timerId);
            if (response.result) {
                if (response.url != "") {
                    linkContainer.find(".control input").val(response.url);
                    linkContainer.find(".control input").removeAttr("disabled");
                    linkContainer.find(".control").removeClass("disabled").removeClass('processing').addClass('success');
                } else {
                    linkContainer.find(".control input").val(preparingErrorMessage);
                    linkContainer.find(".control").removeClass('processing').addClass('failure');
                }
            }
        });
    }

    function _openExportStream(anchor) {
        $(document).bind("keyup", _checkEscKeyIcalStream);
        
        calendar.trigger("getiCalUrl", _this, _source.objectId, function (response) {
            if (response.result) {
                var linkContainer = _icalStream.find(".url-link.ical");
                linkContainer.show();
                linkContainer.find(".control input").val(response.url);
                getCaldavLink();
            }
        });
        
        _icalStream.width(_dialog.width());
        _icalStream.popupFrame("open", anchor);
    }

    function _close(changed, deleted) {
		fcColorPicker.close();
		if (changed && false == _doDDX.call(_this, true)) {return;}
		$(document).unbind("keyup", _checkEscKey);
		_dialog.popupFrame("close");
		_icalStream.popupFrame("close");


		var elem = _elem;
		var source = _source;
		if (configuration.isNewCalendar == 1 && configuration.isExportLink == 1 && configuration.isSyncWithCalendar == 1) {
			var opt = calendar.options;
			var categories = opt.categories;
			var newColor = fcUtil.parseCssColor(fcUtil.randomHex(true));
			var newBg = fcUtil.parseCssColor(fcUtil.randomHex(false));
			var newBor = fcUtil.changeColorLightness(newBg.r, newBg.g, newBg.b, opt.eventBg2BorderRatio);

			elem = undefined;
			source = {
				title: _source.title,
				textColor: _source.textColor,
				backgroundColor: _source.backgroundColor,
				borderColor: _source.borderColor,
				isHidden: false,
				events: [],
				todos: [],
				permissions: { users: [] },
				defaultAlert: { type: 0 },
				timeZone: $.extend({}, opt.defaultTimeZone),
				iCalUrl: _source.iCalUrl
			};
		}

        _trigger.call(
            _this,
            "onClose",
            elem,
            source,
            changed,
            deleted);
	}
	
	function _closeIcalStream() {
	    $(document).unbind("keyup", _checkEscKeyIcalStream);
	    _icalStream.popupFrame("close");
	    if (_dialog.popupFrame('isVisible')) _dialog.popupFrame('show');
	}

	function _checkEscKey(ev) {
		if (ev.which == 27) {
			_close.call(_this, false);
		}
	}
	
	function _checkEscKeyIcalStream(ev) {
		if (ev.which == 27) {
			_closeIcalStream.call(_this);
		}
	}

	function _doDDX(saveData) {
		var opt = calendar.options,
				dlg = {
					title:     _getControl(kTitle),
					bgColor:   _getControl(kColorBox),
					textColor: _getControl(kTextColorBox),
					alert:     _dialog.find(".alert select"),
					timezone:  _dialog.find(".timezone select"),
					iCalUrl:  _dialog.find(".ical-url-input input"),
					calendar: _dialog.find(".calendar select"),
					calendar_b: _dialog.find(".calendar .bullet")
				},
				i;

		if (saveData) {     // ------------- SAVE data -------------
		   
		    if (currentMode == kEditModeFile) { return true; }
		    
		    if (false == fcUtil.validateInput(dlg.title, fcUtil.validateNonemptyString)) { return false; }
		    
		    if (currentMode == kCreateUrlMode || currentMode == kEditUrlMode || currentMode == kCreateUrlWithoutSyncMode) {
				if (false == fcUtil.validateInputHttp(dlg.iCalUrl, fcUtil.validateNonemptyString)) {return false;}
		    }
		    
		    // iCal url
		    if (currentMode == kCreateUrlMode || currentMode == kEditUrlMode || currentMode == kCreateUrlWithoutSyncMode) {
		        _source.iCalUrl = dlg.iCalUrl.val();
		    }
		    
		    if (currentMode == kCreateUrlWithoutSyncMode) {
		        _source.withoutSync = true;
		    }
		    if (currentMode == kEditUrlMode) { return true; }
		    
			_source.title = $.trim(dlg.title.val());
			_source.title = _source.title.substr(0, Math.min(
					calendar.options.eventMaxTitleLength, _source.title.length));

			var newBg = fcUtil.parseCssColor(dlg.bgColor.css("background-color"));
			var newBor;
			if (newBg) {
				newBor = fcUtil.changeColorLightness(newBg.r, newBg.g, newBg.b, calendar.options.eventBg2BorderRatio);
				_source.backgroundColor = newBg.origColor;
				_source.borderColor = newBor.color;
			}
			_source.textColor = dlg.textColor.css("background-color");

			_source.defaultAlert.type = parseInt(dlg.alert.val(), 10);

			//if (_source.canEditTimeZone) 
			{
				_source.timeZone.id = dlg.timezone.val();
				for (i = 0; i < opt.timeZones.length; ++i) {
					if (opt.timeZones[i].id == _source.timeZone.id) {
						_source.timeZone.name = opt.timeZones[i].name;
						_source.timeZone.delta = opt.timeZones[i].offset - _source.timeZone.offset;
						_source.timeZone.offset = opt.timeZones[i].offset;
						break;
					}
				}
			}

			_source.permissions = _permissions;
			
		} else {            // ------------- LOAD data -------------

			dlg.title.val(_source.title);
			dlg.title.css("color", "").css("border-color", "");

			// to round all float numbers to 2nd digit after point
			var rgb = fcUtil.parseCssColor(_source.backgroundColor);
			_setColor.call(_this, rgb.origColor);
			rgb = fcUtil.parseCssColor(_source.textColor);
			_setTextColor.call(_this, rgb.origColor);

			dlg.alert.val(_source.defaultAlert.type);

			//if (_source.canEditTimeZone) 
			{
				var options = '';
				var tz, tzT;
				for (i = 0; i < opt.timeZones.length; ++i) {
					tz = opt.timeZones[i];
					tzT = htmlEscape(tz.name);
					options += '<option value="' + htmlEscape(tz.id) + '" ' +
									'title="' + tzT + '">' + tzT + '</option>';
				}
				dlg.timezone.html(options);
				dlg.timezone.val(_source.timeZone.id);
				_dialog.find(".timezone").removeClass("hidden");
			} /*else {
				_dialog.find(".timezone").addClass("hidden");
			}*/

			_permissions = _source.permissions;
			_permissionsList.render(_permissions, kCalendarPermissions, _source.objectId);

			var options = '<option value="-1" title="' + htmlEscape(calendar.options.newLabel) + '" >&nbsp;&nbsp;&nbsp;&nbsp;' + htmlEscape(calendar.options.newLabel) + '</option><option disabled="disabled"></option>';
			var calT;
			var sources = calendar.getEventSources();
			for (i = 0; i < sources.length; ++i) {
			    if ((sources[i].objectId != undefined) &&
						(sources[i].isEditable || !sources[i].isSubscription) && sources[i].isTodo != 1) {
			        calT = htmlEscape(sources[i].title);
			        options += '<option value="' + htmlEscape(sources[i].objectId) + '" ' +
							'title="' + calT + '">' +
							'&nbsp;&nbsp;&nbsp;&nbsp;' +  // for select elem padding does not work in safari
							calT + '</option>';
			    }
			}
			dlg.calendar.removeAttr("disabled");
			
			dlg.calendar.html(options);
			dlg.calendar_b.css("background", "transparent");

			var calCount = 0;
			$.each(calendar.getEventSources(), function(i,src) {
			    if (src.objectId != undefined && !src.isSubscription && src.isTodo != 1) {++calCount;}
			});

			if (calCount > 1 && !jq("#asc_event .event-editor .editor").is(":visible")) {
				_dialog.find(".buttons").removeClass("read-only");
			} else {
				_dialog.find(".buttons").addClass("read-only");
			}
		}
		return true;
	}
	function _getDefaultSource() {
	    var sources = calendar.getEventSources();
	    var s1;
	    for (var i = 0; i < sources.length; ++i) {
	        if (fcUtil.objectIsValid(sources[i]) && (
			    fcUtil.objectIsEditable(sources[i]) || !sources[i].isSubscription)) {
	            if (s1 == undefined) { s1 = sources[i]; }
	            if (!sources[i].isHidden) { return sources[i]; }
	        }
	    }
	    return s1;
	}


	this.edit = function(elem, anchor) {
		if (elem != _elem) {
			_close.call(_this, false);
			_elem = elem;
			_source = calendar.getEventSources()[$(_elem).data("sourceIndex")];
		}
		_anchor = anchor;
		_doDDX.call(_this);
		_setMode.call(_this, kEditMode);
		
		_dialog.find(".choose_event_source").addClass("hidden");
		_dialog.find(".sync-with-calendar").addClass("hidden");
		_dialog.find(".calendar").addClass("hidden");

		_dialog.find(".ical").addClass("hidden");
		_dialog.find(".export").removeClass("hidden");
		_dialog.find(".ical-import").removeClass("hidden");
		
		_dialog.find(".title").removeClass("hidden");
		_dialog.find(".row").removeClass("hidden");
		_dialog.find(".shared-list").removeClass("hidden");
		_dialog.find(".export").removeClass("hidden");
		_dialog.find(".ical-url-input").addClass("hidden");
		
		_open.call(_this, anchor);
	};

	this.addNew = function (anchor) {

	    configuration.isExportFile = 0;
	    configuration.isExportLink = 0;
	    configuration.isNewCalendar = 0;
	    configuration.isSyncWithCalendar = 0;
	    _updateConfiguration();

		var opt = calendar.options;
		var categories = opt.categories;
		var newColor = fcUtil.parseCssColor(fcUtil.randomHex(true));
		var newBg = fcUtil.parseCssColor(fcUtil.randomHex(false));
		var newBor = fcUtil.changeColorLightness(newBg.r, newBg.g, newBg.b, opt.eventBg2BorderRatio);

		_elem = undefined;
		_source = {
			title: categories.defaultTitle,
			textColor: newColor.origColor,
			backgroundColor: newBg.origColor,
			borderColor: newBor.color,
			isHidden: false,
			events: [],
			todos: [],
			permissions: {users:[]},
			defaultAlert: {type:0},
			timeZone: $.extend( {}, opt.defaultTimeZone )
		};
		_anchor = anchor;
		_doDDX.call(_this);
		_setMode.call(_this, kCreateMode);
		
		_dialog.find(".export").addClass("hidden");

		/*_dialog.find(".choose_event_source").addClass("hidden");
		_dialog.find(".ical").addClass("hidden");
		_dialog.find(".calendar").addClass("hidden");
		_dialog.find(".sync-with-calendar").addClass("hidden");*/

		//_dialog.find(".ical-import").removeClass("hidden");
		
		_dialog.find(".row").removeClass("hidden");
		_dialog.find(".shared-list").removeClass("hidden");
		_dialog.find(".ical-url-input").addClass("hidden");
		
		_open.call(_this, anchor);
	};
	
	this.addiCalCalendar = function (anchor) {

	    configuration.isExportLink = 1;
	    configuration.isExportFile = 0;
	    configuration.isNewCalendar = 1;
	    configuration.isSyncWithCalendar = 0;
	    _updateConfiguration();

		var opt = calendar.options;
		var categories = opt.categories;
		var newColor = fcUtil.parseCssColor(fcUtil.randomHex(true));
		var newBg = fcUtil.parseCssColor(fcUtil.randomHex(false));
		var newBor = fcUtil.changeColorLightness(newBg.r, newBg.g, newBg.b, opt.eventBg2BorderRatio);

		_elem = undefined;
		_source = {
			title: categories.defaultTitle,
			textColor: newColor.origColor,
			backgroundColor: newBg.origColor,
			borderColor: newBor.color,
			isHidden: false,
			events: [],
			todos: [],
			permissions: {users:[]},
			defaultAlert: {type:0},
			timeZone: $.extend( {}, opt.defaultTimeZone )
		};
		_anchor = anchor;
		if (configuration.isSyncWithCalendar == 0) {
		    _setMode.call(_this, kCreateUrlWithoutSyncMode, true);
		} else {
		    _setMode.call(_this, kCreateUrlMode, true);
		}

		_doDDX.call(_this);
		
		_dialog.find(".export").addClass("hidden");
		
		_open.call(_this, anchor);
	};

	this.close = function(changed) {
		_close.call(_this, changed);
	};

}


function SubscriptionDialog(calendar) {
	var _this = this;

	// Trigger dialog's events:
	//   onClose(elem, source, changed)
	//   onUnsubscribe(source)

	var _handlers = {};

	function _trigger(eventName) {
		if (_handlers && $.isFunction(_handlers[eventName])) {
			_handlers[eventName].apply(
					_handlers.context != undefined ? _handlers.context : window,
					Array.prototype.slice.call(arguments, 1));
		}
	}

	this.setEventsHandlers = function(handlers) {
		if (handlers != undefined) {
			$.extend(_handlers, handlers);
		}
	};


	var _dialog;
	var _permissionsList;
	
	var _icalStream;
	var ic = calendar.options.icalStream;

	var _elem;
	var _source;
	var _anchor;
	var _permissions;

	var kTitle        = 1;
	var kColorBox     = 11;
	var kTextColorBox = 12;

	(function _create() {
		_dialog = $(calendar.options.categories.subscriptionsDialog)
				.addClass("asc-dialog")
				.addClass("fc-shadow")
				.popupFrame({
					anchor: "left,bottom",
					direction: "right,down",
					showArrow: false,
					showModal: true,
					beforeClose: function() {_close.call(_this, false);return false;}
				});
		
		_dialog.find(".ical-export .ical-link").click(function() {
		    _openIcalStream.call(_this, { pageX: "center", pageY: "center" });
		    _dialog.popupFrame('hide');
		});
				
		_dialog.find(".buttons .save-btn").click(function() {
		    if (jq(this).hasClass("disable"))
		        return;
		    
		    _close.call(_this, true);
		});
		_dialog.find(".buttons .cancel-btn, .header .close-btn").click(function() {
			_close.call(_this, false);
		});
		_dialog.find(".buttons .unsubs-btn").click(function() {
			_trigger.call(_this, "onUnsubscribe", _source);
			_close.call(_this, false);
		});
		_dialog.find(".buttons .delete-btn").click(function() {
			_trigger.call(_this, "onDelete", _source);
			_close.call(_this, false, true);
		});
		//
		_getControl(kTitle).keyup(function() {fcUtil.validateInput(this, fcUtil.validateNonemptyString);});
		_getControl(kColorBox).click(function() {fcColorPicker.open(this, _setColor,fcColors.DefaultPicker);});
		_getControl(kTextColorBox).click(function() {fcColorPicker.open(this, _setTextColor,fcColors.TextPicker);});
		//
		_permissionsList = new PermissionsList(calendar, _dialog.find(".shared-list"),
				calendar.options.sharedList.addLink);
		_permissionsList.setEventsHandlers({
					context:   _this,
					onResize: function() {_dialog.popupFrame("updatePosition", _anchor);},
					onChange: _updatePermissions
				});
	}());
	
	(function _createIcal() {
		_icalStream = $(ic.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("add-popup")
				.popupFrame({
					anchor: "right,top",
					direction: "right,down",
					offset: "0,0",
					showArrow: false
				});
		
		_icalStream.find(".buttons .cancel-btn, .header .close-btn").click(function () {
		    _closeIcalStream.call(_this, false);
		    _icalStream.popupFrame("close");
		    if (_dialog.popupFrame('isVisible')) _dialog.popupFrame('show');
		});
	    

		_icalStream.find((".url-link .control input")).click(function () {
		    $(this).select();
		});
		_icalStream.find((".url-link .control .button.copy")).click(function () {
		    var control = $($(this)[0].parentNode);
		    control.find('input').select();
		    try {
		        document.execCommand('copy');
                ASC.CalendarController.displayInfoPanel(calendar.options.categories.dialogCopyMessage, false);
		    } catch (err) { }
		});
		
	}());

	function _getControl(controlId) {
		switch (controlId) {
			case kTitle:return _dialog.find(".title input");
			case kColorBox:return _dialog.find(".color .inner");
			case kTextColorBox:return _dialog.find(".color .inner-for-text");
		}
		return undefined;
	}

	function _setIconColor(newColor, icon) {
		var c = fcUtil.parseCssColor(newColor);
		var bc = ((Math.round(c.r * 0.9) << 16) | (Math.round(c.g * 0.9) << 8) | Math.round(c.b * 0.9)).toString(16);
		while (bc.length < 6) {bc = "0" + bc;}
		icon.css("background-color", newColor).css("border-color", "#" + bc);
		icon.parent().css("border-color", "#" + bc);
	}

	function _setColor(newColor) {
	    _setIconColor(newColor, _getControl(kColorBox));
		_dialog.find(".title .bullet").css("background", newColor);
	}

	function _setTextColor(newColor) {
		_setIconColor(newColor, _getControl(kTextColorBox));
	}


	function _updatePermissions(response) {
		if (response.result) {
			_permissions = response.permissions;
			if (response.isShared != undefined) {
				_source.isShared = response.isShared;
			}
			_permissionsList.render(_permissions, kCalendarPermissions, _source.objectId);
			_dialog.popupFrame("updatePosition", _anchor);
		}
	}


	function _open(anchor) {
		$(document).bind("keyup", _checkEscKey);
		
		// check length
		var titleMaxLen = defaults.eventMaxTitleLength;
		var inputTxt = _dialog.find(".title input");
		if (inputTxt.length > 0)
		{
			inputTxt.keyup(function(){
				if (inputTxt.val().length > titleMaxLen){
					inputTxt.val(inputTxt.val().substr(0, titleMaxLen));
				}
			});
		}
		//
		
		if (_source.isiCalStream) {
			_dialog.find(".ical").addClass("hidden");
			_dialog.find(".buttons").find(".delete-btn").removeClass("hidden");
			
			if ((_source.iCalUrl != undefined) && (_source.iCalUrl != "")) {
				_dialog.find(".ical-saved-url").removeClass("hidden");
				
				var link = "<a href=" + _source.iCalUrl + " target='_blank'>" + _source.iCalUrl + "</a>";
				_dialog.find(".saved-url-link").html(link);
			}
		}
		else {
			_dialog.find(".ical").removeClass("hidden");
			_dialog.find(".export").removeClass("hidden");
			
			_dialog.find(".ical-saved-url").addClass("hidden");
			_dialog.find(".buttons").find(".delete-btn").addClass("hidden");
			_dialog.find(".saved-url-link").html("");
		}
		
		if (_source.isEditable && !_source.isSubscription) {
			_dialog.find(".ical-import").removeClass("hidden");
		}
		else {
			_dialog.find(".ical-import").addClass("hidden");
		}
		_dialog.popupFrame("open", anchor);
		
		_dialog.find(".ical-file-selected").html(ic.dialogButton_fileNotSelected);
		
		// upload ajax
		AjaxUploader = new AjaxUpload('#ical-browse-btn-subs', {
			action: 'upload.php',
			autoSubmit: false,
			//responseType: "xml",
			onChange: function(file, extension) {
				
				if ( !(extension && ( /^(ics)$/.test(extension) || /^(txt)$/.test(extension) )) ){
					_dialog.find(".ical-file-selected").html(ic.dialog_incorrectFormat).css("color", "#cc3300");
					return;
				}
				
				this.isChanged = true; 
				_dialog.find(".ical-file-selected").html(ic.dialogButton_fileSelected).css("color", "grey");
				},
			onSubmit: function(file, extension) {
				LoadingBanner.displayLoading(); 
				},
			onComplete: function(file, response) { 
				LoadingBanner.hideLoading();
				calendar.refetchEvents();
				}
			});
	}
	var timerId;
	function getCaldavLink() {
	    clearInterval(timerId);
	    var linkContainer = _icalStream.find(".url-link.caldav");
	    linkContainer.find(".control").removeClass('success').removeClass('failure').addClass('disabled').addClass('processing');
	    linkContainer.find(".control input").attr('disabled', 'disabled');

	    linkContainer.show();

	    var counter = 0, preparingMessage = ic.dialogPreparingMessage, preparingErrorMessage = ic.dialogPreparingErrorMessage;
	    timerId = setInterval(function () {
	        preparingMessage = preparingMessage + ".";
	        if (counter == 4) {
	            preparingMessage = ic.dialogPreparingMessage;
	            counter = 0;
	        }
	        linkContainer.find(".control input").val(preparingMessage);
	        counter++;
	    }, 500);

	    calendar.trigger("getCaldavUrl", _this, _source.objectId, function (response) {
	        clearInterval(timerId);
	        if (response.result) {
	            if (response.url != "") {
	                linkContainer.find(".control input").val(response.url);
	                linkContainer.find(".control input").removeAttr("disabled");
	                linkContainer.find(".control").removeClass("disabled").removeClass('processing').addClass('success');
	            } else {
	                linkContainer.find(".control input").val(preparingErrorMessage);
	                linkContainer.find(".control").removeClass('processing').addClass('failure');
	            }
	        }
	    });
	}
	function _openIcalStream(anchor) {
		$(document).bind("keyup", _checkEscKeyIcalStream);

		calendar.trigger("getiCalUrl", _this, _source.objectId, function (response) {
		    if (response.result) {
		        var linkContainer = _icalStream.find(".url-link.ical");
		        linkContainer.show();
		        linkContainer.find(".control input").val(response.url);
		        getCaldavLink();
		    }
		});
		
		_icalStream.width(_dialog.width());
		_icalStream.popupFrame("open", anchor);
	}

	function _close(changed, deleted) {
		fcColorPicker.close();
		if (changed && false == _doDDX.call(_this, true)) {return;}
		$(document).unbind("keyup", _checkEscKey);
		_dialog.popupFrame("close");
		_icalStream.popupFrame("close");
		_trigger.call(_this, "onClose", _elem, _source, changed, deleted);
	}
	
	function _closeIcalStream() {
		$(document).unbind("keyup", _checkEscKeyIcalStream);
		_icalStream.popupFrame("close");
	}

	function _checkEscKey(ev) {
		if (ev.which == 27) {
			_close.call(_this, false);
		}
	}
	
	function _checkEscKeyIcalStream(ev) {
		if (ev.which == 27) {
			_closeIcalStream.call(_this);
		}
	}

	function _doDDX(saveData) {
		var opt = calendar.options,
				dlg = {
					title:                  _getControl(kTitle),
					bgColor:                _getControl(kColorBox),
					textColor:              _getControl(kTextColorBox),
					alert:                  _dialog.find(".alert select"),
					timezone:               _dialog.find(".timezone select"),
					owner:                  _dialog.find(".owner .name")
				},
				i;
		var isEditable = _source.isEditable;
		var canChangeAlert = _source.canAlertModify === undefined || _source.canAlertModify;
		var canChangeTimezone = _source.canEditTimeZone;

		if (saveData) {     // ------------- SAVE data -------------

			if (false == fcUtil.validateInput(dlg.title, fcUtil.validateNonemptyString)) {return false;}
			_source.title = $.trim(dlg.title.val());
			_source.title = _source.title.substr(0,
					Math.min(calendar.options.eventMaxTitleLength, _source.title.length));

			var newBg = fcUtil.parseCssColor(dlg.bgColor.css("background-color"));
			var newBor;
			if (newBg) {
				newBor = fcUtil.changeColorLightness(newBg.r, newBg.g, newBg.b, calendar.options.eventBg2BorderRatio);
				_source.backgroundColor = newBg.origColor;
				_source.borderColor = newBor.color;
			}
			_source.textColor = dlg.textColor.css("background-color");

			if (canChangeAlert) {
				_source.defaultAlert.type = parseInt(dlg.alert.val(), 10);
			}

			if (canChangeTimezone) {
				_source.timeZone.id = dlg.timezone.val();
				for (i = 0; i < opt.timeZones.length; ++i) {
					if (opt.timeZones[i].id == _source.timeZone.id) {
						_source.timeZone.name = opt.timeZones[i].name;
						_source.timeZone.delta = opt.timeZones[i].offset - _source.timeZone.offset;
						_source.timeZone.offset = opt.timeZones[i].offset;
						break;
					}
				}
			}

			if (isEditable) {
				_source.permissions = _permissions;
			}

		} else {            // ------------- LOAD data -------------

			dlg.title.val(_source.title);
			dlg.title.css("color", "").css("border-color", "");

			// to round all float numbers to 2nd digit after point
			var rgb = fcUtil.parseCssColor(_source.backgroundColor);
			_setColor.call(_this, rgb.origColor);
			rgb = fcUtil.parseCssColor(_source.textColor);
			_setTextColor.call(_this, rgb.origColor);

			if (canChangeAlert && !_source.isiCalStream) {
				dlg.alert.val(_source.defaultAlert.type);
				_dialog.find(".alert").removeClass("hidden");
			} 
			else {
				dlg.alert.val(kAlertNever);
				_dialog.find(".alert").addClass("hidden");
			}

			if (canChangeTimezone && !_source.isiCalStream) {
				var options = '';
				var tz, tzT;
				for (i = 0; i < opt.timeZones.length; ++i) {
					tz = opt.timeZones[i];
					tzT = htmlEscape(tz.name);
					options += '<option value="' + htmlEscape(tz.id) + '" ' +
									'title="' + tzT + '">' + tzT + '</option>';
				}
				dlg.timezone.html(options);
				dlg.timezone.val(_source.timeZone.id);
				_dialog.find(".timezone").removeClass("hidden");
				_dialog.find(".timezone-read-only").addClass("hidden");
			} 
			else {
				_dialog.find(".timezone").addClass("hidden");
				_dialog.find(".timezone-read-only").removeClass("hidden");
				_dialog.find(".timezone-desc").html(htmlEscape(_source.timeZone.name));
			}

			if (_source.objectId !== "shared_events" && _source.owner && _source.owner.name && _source.owner.name.length > 0) {
				dlg.owner.html(htmlEscape(_source.owner.name));
				_dialog.find(".owner").removeClass("hidden");
			} else {
				dlg.owner.html("");
				_dialog.find(".owner").addClass("hidden");
			}

			_permissions = _source.permissions || {};
			_permissions.users = _permissions.users || [];
			if (isEditable) {
				_permissionsList.render(_permissions, kCalendarPermissions, _source.objectId);
			} else {
				_dialog.find(".shared-list").removeClass("has-users").empty();
				var list = "";
				for (var i = 0; i < _permissions.users.length; i++) {
					if (i > 0) {list += ", ";}
					list += _permissions.users[i].name;
				}
				if (list.length > 0) {
                    _dialog.find(".shared-list").show();
					_dialog.find(".shared-list").addClass("has-users").append(
							'<div class="users-list">' +
								'<div class="label">' + htmlEscape(calendar.options.sharedList.title) + '</div>' +
								'<div class="short-list">' + htmlEscape(list) + '</div>' +
							'</div>');
				} else {
				    _dialog.find(".shared-list").hide();
				}
			}

		}
		return true;
	}


	this.edit = function(elem, anchor) {
		if (elem != _elem) {
			_close.call(_this, false);
			_elem = elem;
			_source = calendar.getEventSources()[$(_elem).data("sourceIndex")];
		}
		_anchor = anchor;
		_doDDX.call(_this);
		_open.call(_this, anchor);
	};

	this.close = function(changed) {
		_close.call(_this, changed);
	};

}


function ManageSubscriptionsDialog(calendar) {
	var _this = this;

	// Triggers dialog's events:
	//   onClose(changed)

	// Triggers calendar's events:
	//   loadSubscriptions(callback)
	//   manageSubscriptions(param, callback)

	var _handlers = {};

	function _trigger(eventName) {
		if (_handlers && $.isFunction(_handlers[eventName])) {
			_handlers[eventName].apply(
					_handlers.context != undefined ? _handlers.context : window,
					Array.prototype.slice.call(arguments, 1));
		}
	}

	this.setEventsHandlers = function(handlers) {
		if (handlers != undefined) {
			$.extend(_handlers, handlers);
		}
	};


	var _dialog;
	var _anchor;
	var _subscriptions;

	(function _create() {
		_dialog = $(calendar.options.categories.subscriptionsManageDialog)
				.addClass("asc-dialog")
				.addClass("fc-shadow")
				.popupFrame({
					anchor: "left,bottom",
					direction: "right,down",
					showArrow: false,
					showModal: true,
					beforeClose: function() {_close.call(_this, false);return false;}
				});
		_dialog.find("#fc_subscr_save").click(function() {
			_close.call(_this, true);
		});
		_dialog.find("#fc_subscr_cancel, .header .close-btn").click(function() {
			_close.call(_this, false);
		});
		//
		var qtext = calendar.options.categories.subscriptionsManageDialog_qsearchText;
		_dialog.find(".qsearch input")
				.focus(function() {
					var q = $(this);
					if (q.val().toLowerCase() == qtext.toLowerCase()) {q.val("");}
				})
				.blur(function() {
					var q = $(this);
					if (q.val() == "") {q.val(qtext);}
				})
				.keyup(function() {
					var qval = $(this).val().toLowerCase();
					if (qval.length > 2) {
						_dialog.find("ul.groups li li")
								.filter(function() {return $(this).text().toLowerCase().indexOf(qval) == -1;})
								.hide();
						_dialog.find("ul.groups li li")
								.filter(function() {return $(this).text().toLowerCase().indexOf(qval) != -1;})
								.show();
					} else {
						_dialog.find(".groups li").show();
					}
					_updateScrollArea();
				});
		_dialog.find(".qsearch .clean-btn").click(function() {
			_cleanQSearch();
			_dialog.find(".groups li").show();
		});
	}());

	function _cleanQSearch() {
		_dialog.find(".qsearch input")
				.val(calendar.options.categories.subscriptionsManageDialog_qsearchText);
	}

	function _updateScrollArea() {
		_dialog.find(".groups .scroll-area").data("jsp").reinitialise();
	}

	function _close(changed) {
		$(document).unbind("keyup", _checkEscKey);
		_dialog.popupFrame("close");
		if (changed) {_doDDX.call(_this, true);}
		_trigger.call(_this, "onClose", changed);
	}

	function _checkEscKey(ev) {
		if (ev.which == 27) {
			_close.call(_this, false);
		}
	}

	function _doDDX(saveData) {
		var dlg = {
			list: _dialog.find("div.groups")
		};

		if (saveData) {     // ------------- SAVE data -------------

			var param = [];

			_dialog.find("ul.groups li li input").each(function() {
				var cb = $(this);
				param.push({
					action:   cb.is(":checked") ? kSubscriptionAddAction : kSubscriptionRemoveAction,
					objectId: _subscriptions[parseInt(cb.val(), 10)].objectId,
					isNew:    false
				});
			});

			//debugOutput("debug", "manageSubscriptions: %o", param);
			calendar.trigger("manageSubscriptions", _this, param, function(response) {
				//debugOutput("debug", "manageSubscriptions -> callback: response: %o", response);
				if (!response.result) {return;}
				calendar.refetchEvents();
			});

		} else {            // ------------- LOAD data -------------

			_cleanQSearch();

			dlg.list.empty();
			dlg.list.append('<div class="scroll-area"><ul class="groups"/></div>');

			var sList = {};
			for (var i = 0; i < _subscriptions.length; ++i) {
				var group = _subscriptions[i].group != undefined && _subscriptions[i].group != null ?
						_subscriptions[i].group : "Unknown";
				if (!$.isArray(sList[group])) {sList[group] = [];}
				sList[group].push({subscription: _subscriptions[i], index: i});
			}

			var groupList = dlg.list.find("ul.groups");
			var groupItem, sublist;
			for (group in sList) if (sList.hasOwnProperty(group)) {
				groupItem = $('<li><span class="group-bullet"/><span>' + group + '</span></li>');
				sublist = $('<ul/>');
				sList[group]
					.sort(function(l,r) {
							return l.subscription.title.toLowerCase() < r.subscription.title.toLowerCase() ? -1 : 1;
					})
					.sort(function(l,r) {
						return l.subscription.isNew ? -1 : (r.subscription.isNew ? 1 : -1);
					});
				for (i = 0; i < sList[group].length; ++i) {
					var subscription = sList[group][i].subscription;
					$('<li><label><input type="checkbox" name="checkedSubscriptions"' +
							' value="' + sList[group][i].index + '"'
							+ (subscription.isSubscribed ? ' checked="checked"' : '') + '/> '
							+ htmlEscape(subscription.title) + '</label></li>')
						.addClass(subscription.isNew ? "new-item" : "")
						.appendTo(sublist);
				}
				if (sublist.length > 0) {groupItem.append(sublist);}
				groupList.append(groupItem);
			}

			groupList.find(".group-bullet").click(function() {
				var b = $(this);
				if (b.hasClass("folded")) {
					b.removeClass("folded");
					b.parent().children("ul").slideDown(_updateScrollArea);
				} else {
					b.addClass("folded");
					b.parent().children("ul").slideUp(_updateScrollArea);
				}
			});

			//_dialog.find(".groups .scroll-area").jScrollPane();

		}
	}


	this.open = function(anchor) {
		_anchor = anchor;
		calendar.trigger("loadSubscriptions", _this,
				function(response) {
					if (!response.result || !$.isArray(response.subscriptions)) {return;}
						_subscriptions = response.subscriptions;
						_dialog.popupFrame("open", _anchor);
						_doDDX.call(_this);
						$(document).bind("keyup", _checkEscKey);
						_dialog.popupFrame("updatePosition", _anchor);
				});
	};

	this.close = function(changed) {
		_close.call(_this, changed);
	};

}


function CategoriesList(calendar) {
	var _this = this;

	var _content;
	var _list;

	var _categoryDialog;
	var _subscriptionDialog;
	var _manageSubscriptionsDialog;

	var _datepicker;
	var _dpVisibleDate;
	var _dpCommonVisibleDate = new Date();
	var isOpenCommonDatePicker = false;
	var _dpickerIsVisible = true;
	var _dpIgnoreUpdate = false;
	var _dpSettingDate = false;
	var _dpYearMenuCaller;
	var _dpPrevHeight = 0;

	var _clickStart;
	var _clickCount;

	var kWholeList         = 0;
	var kCategoriesOnly    = 1;
	var kSubscriptionsOnly = 2;


	var formatDate = calendar.formatDate;
	var formatDates = calendar.formatDates;


	function _changeTime(d, offset) {
		return new Date( d.getTime() + offset * 60000 );
	}

	function _updateTimeZone(events, offset) {
		for (var i = 0; i < events.length; ++i) {
			if (events[i].allDay === undefined || !events[i].allDay) {
				events[i].start = _changeTime(events[i].start, offset);
				if (events[i].end instanceof Date) {
					events[i].end = _changeTime(events[i].end, offset);
				}
			}
		}
	}

	// Categories

	function _addNewCategory() {
		_categoryDialog.addNew({pageX:"center",pageY:"center"});
	}

	function _editCategory(categoryElem) {
	    ASC.CalendarController.CancelEditDialog();
		_categoryDialog.edit(categoryElem, {pageX:"center",pageY:"center"});
	}

    function _deleteCategory(source) {
        calendar.trigger("editCalendar", _this,
            $.extend({ action: kCalendarDeleteAction }, source),
            function(response) {
                if (!response.result) {
                    return;
                }
                calendar.removeEventSource(source);
                calendar.rerenderEvents();
                _rerenderList.call(_this);

                if (jq("#asc_event").is(":visible")) {
                    jq("#asc_event").find(".editor .calendar select option[value=" + source.objectId + "]").remove();
                    jq("#asc_event").find(".editor .calendar select").change();
                }

            });
    }

    function _updateCategory(elem, source, changed, deleted) {
		if (changed) {
			if (elem) {
				// Change existing category
				calendar.trigger("editCalendar", _this,
						$.extend({action: kCalendarChangeAction}, source),
						function(response) {
							if (!response.result) {return;}

							_syncCategory(source, response);
							_updateListItem.call(_this, elem);
							_ellipsisLongItem.call(elem);
							if (source.timeZone !== undefined) {
								_updateTimeZone(source.events, source.timeZone.delta);
							}
							
							if (AjaxUploader.isChanged != undefined) {
								AjaxUploader._settings.action = response.importUrl;
								AjaxUploader.submit();
							}
							calendar.addEventSource(response.source);
							_rerenderList.call(_this, kSubscriptionsOnly, true);
							calendar.refetchEvents();

						});
			} else {
				// Add new category
			    calendar.trigger("editCalendar", _this,
			        $.extend({ action: kCalendarAddAction }, source),
			        function(response) {
			            if (!response.result) {
			                return;
			            }

			            if (AjaxUploader.isChanged != undefined) {
			                AjaxUploader._settings.action = response.importUrl;
			                AjaxUploader.submit();
			            }

			            calendar.addEventSource(response.source);
			            _rerenderList.call(_this, kCategoriesOnly, true);
			            _rerenderList.call(_this, kSubscriptionsOnly, true);
			            calendar.refetchEvents();

			            if (jq("#asc_event").is(":visible")) {
			                jq("#asc_event")
			                    .find(".editor .calendar select")
			                    .append(jq("<option></option>")
			                        .attr("value", response.source.objectId)
			                        .attr("title", response.source.title)
			                        .html("&nbsp;&nbsp;&nbsp;&nbsp;" + htmlEscape(response.source.title)));
			            }

			        });
			}
		}
		else if (!deleted) {
			calendar.trigger("editCalendar", _this,
						$.extend({action: kCalendarCancelAction}, source),
						function(response) {
							if (!response.result) {return;}
						});
		}
	}

	function _syncCategory(source, response){
		if (response.source.objectId === source.objectId){
			source.isShared = response.source.isShared;
			// ...
		}
	}


	// Subscriptions

	function _editSubscription(elem) {
		_subscriptionDialog.edit(elem, {pageX:"center",pageY:"center"});
	}

	function _updateSubscription(elem, source, changed, deleted) {
		if (changed) {
			if (elem) {
				calendar.trigger("editCalendar", _this,
						$.extend({action: kCalendarChangeAction}, source),
						function(response) {
							if (!response.result) {return;}
							_updateListItem.call(_this, elem);
							_ellipsisLongItem.call(elem);
							if (source.timeZone !== undefined) {
								_updateTimeZone(source.events, source.timeZone.delta);
							}
							
							if (AjaxUploader.isChanged != undefined) {
								AjaxUploader._settings.action = response.importUrl;
								AjaxUploader.submit();
							}
							else {
								calendar.refetchEvents();
							}
						});
			}
		}
		else if (!deleted) {
			calendar.trigger("editCalendar", _this,
						$.extend({action: kCalendarCancelAction}, source),
						function(response) {
							if (!response.result) {return;}
							calendar.refetchEvents();
						});
		}
	}

	function _removeSubscription(source) {
		calendar.trigger("manageSubscriptions", _this,
				[{
					action:   kSubscriptionRemoveAction,
					objectId: source.objectId,
					isNew:    false
				}],
				function(response){
					if (response.result) {calendar.removeEventSource(source);}
				});
	}

	function _manageSubscriptions() {
		_manageSubscriptionsDialog.open({pageX:"center",pageY:"center"});
	}


	// Render functions

	function _renderListItem(index, item) {
		return $(
				'<div class="content-li">' +
					fcUtil.makeBullet(!item.isHidden, item.backgroundColor,
					                  calendar.options.categories.inactiveColor,
					                  item.isSubscription ? false : item.isShared) +
					'<div class="label" style="'+ (item.isHidden ? ("color:" + calendar.options.categories.inactiveColor) : "") +'">' +
						htmlEscape(item.title ? item.title : 'Unknown') + '</div>' +
					'<div class="edit-icon" title="' + htmlEscape(calendar.options.categories.dialogHeader_edit) + '"/>' +
				'</div>')
				.data("sourceIndex", index);
	}

	function _ellipsisLongItem() {
        var li = $(this);
        var label = li.find(".label");
        label.attr("title", label.text());

        //TODO: remove this function! use css!
        return;
        label.css("right", "")
            .removeClass("show-tooltip")
            .unbind("mouseenter");
		var dots = li.find(".dots");
		if (label.position().left + label.width() >= li.width() - 3/*paddind*/) {
			if (dots.length < 1) {dots = $('<div class="dots">&#8230;</div>').appendTo(li);}
			label
					.css("right", (li.width() - dots.position().left) + "px")
					.addClass("show-tooltip")
					.mouseenter(
							function() {
								var id = "tt_" + (new Date()).getTime();
								var hideTT = function(){
									$("#" + id).remove();};
								var label = $(this);
								$('<div class="fc-label-tooltip" id="' + id + '">' + htmlEscape(label.text()) + '</div>')
										.appendTo("body")
										.offset(label.offset())
										.mouseleave(function() {setTimeout(hideTT, 0);});
								setTimeout(hideTT, 2000);
							});
		} else {
			if (dots.length > 0) {dots.remove();}
		}
	}

	function _updateListItem(item) {
		var elem = $(item);
		var source = calendar.getEventSources()[elem.data("sourceIndex")];
		fcUtil.updateBullet(
				elem.find(".bullet"),
				!source.isHidden,
				source.backgroundColor,
				calendar.options.categories.inactiveColor,
				source.isSubscription ? false : source.isShared);
		elem.find(".label").text(source.title);
	}

	function _toggleListItem(item) {
		var elem = $(item);
		var source = calendar.getEventSources()[elem.data("sourceIndex")];
		var hidden = source.isHidden;
		source.isHidden = !hidden;
		calendar.trigger("editCalendar", _this,
				$.extend({action: kCalendarHideAction}, source),
				function(response) {
					if (!response.result) {source.isHidden = hidden;return;}
					//calendar.refetchEvents();
					_updateListItem.call(_this, elem);
					calendar.rerenderEvents();
				});
	}

	function _renderCategories(list) {
	    var sources = calendar.getEventSources();
	    
	    //list.find(".categories").removeClass('hidden');
	    list.find(".my_calendars").removeClass('active');
	    
	    

		var categ = list.find(".categories");
		categ.empty();
		
			for (var i = 0; i < sources.length; ++i) {
				var cal = sources[i];

				if (cal.isTodo == 1) continue;

				if (!cal.title || cal.title.length < 1 || cal.isSubscription) { continue; }
				cal.isHidden = cal.isHidden == true ? true : false; // convert to boolean

				var li = _renderListItem.call(_this, i, cal);
				li.find(".bullet, .label").click(function () {
					_toggleListItem.call(_this, $(this).parent().get(0));
				});
				li.find(".edit-icon").click(function () {
					_editCategory.call(_this, $(this).parent().get(0));
				});
				li.appendTo(categ);
			}
		

		
	}

	function _renderSubscriptions(list) {
		var sources = calendar.getEventSources();
        if (!list.find(".subscr").length) {
            return;
        }
	    
        //list.find(".subscr").removeClass('hidden');
        list.find(".other_calendars").removeClass('active');
	    
		
		var subscr = list.find(".subscr");
		subscr.empty();
		
			for (var i = 0; i < sources.length; ++i) {
				var sub = sources[i];
				if (!sub.title || sub.title.length < 1 || !sub.isSubscription) { continue; }
				sub.isHidden = sub.isHidden == true ? true : false; // convert to boolean

				var li = _renderListItem.call(_this, i, sub);
				li.find(".bullet, .label").click(function () {
					_toggleListItem.call(_this, $(this).parent().get(0));
				});
				li.find(".edit-icon").click(function () {
					_editSubscription.call(_this, $(this).parent().get(0));
				});
				li.appendTo(subscr);
			}
		
		
	}

	function _renderList() {
		var list = $("div.fc-catlist .content");
		if (list.length < 1) {
			list = $(
				'<div class="content">' +
					'<div class="content-h first my_calendars">' +
						'<span class="main-label">' + htmlEscape(calendar.options.categories.title) + '</span>' +
						//<!--'<span class="add-label" title="' + htmlEscape(calendar.options.categories.addNewCategoryLabel) + '"/>' + -->
					'</div>' +
					'<div class="categories"/>' +
                    (calendar.options.isPersonal ? "" :
					'<div class="content-h other_calendars">' +
						'<span class="main-label">' + htmlEscape(calendar.options.categories.subscriptionsTitle) + '</span>' +
						'<span class="manage-label" title="' + htmlEscape(calendar.options.categories.subscriptionsManageLabel) + '"/>' +
					'</div>' +
					'<div class="subscr"/>' +
				'</div>'));
			//list.find(".add-label").click(function() {_addNewCategory.call(_this);});
			list.find(".manage-label").click(function(event) {
			    event.preventDefault();
			    _manageSubscriptions.call(_this);
			    
			    list.find(".content-h.other_calendars,.content-h.my_calendars").removeClass('active');
			    list.find(".subscr,.categories").removeClass('hidden');
			    _resizeList(true);
			    
			    return false;
			});
			//list.find(".categories, .subscr").jScrollPane();

			list.find(".content-h.my_calendars").click(function () {
			    $(this).toggleClass('active');
				list.find(".categories").toggleClass('hidden');
				_resizeList(true);
			});
			list.find(".content-h.other_calendars").click(function () {
			    $(this).toggleClass('active');
			    var categoriesHeight = list.find(".categories").height();
				list.find(".subscr").toggleClass('hidden');
				_resizeList(true, categoriesHeight);
			});
		}
		_renderCategories.call(_this, list);
		_renderSubscriptions.call(_this, list);
		return list;
	}

	function _resizeList(scrollToEnd, categHeight) {
	    if (_list.length < 1) {return;}

		var categ = _list.find(".categories");
		var subscr = _list.find(".subscr");

	    var isCategoriesHidden = categ.hasClass('hidden'),
	        isSubscrHidden = subscr.hasClass('hidden');
	    


		_list.find(".content-li").each(_ellipsisLongItem);

		_list.css("visibility", "hidden");
		_list.removeClass("fixed");

		// find lists
		
		var categItems = categ.find(".content-li");
		
		var subscrItems = subscr.find(".content-li");

		// restore heights of lists
		var cih = categItems.eq(0).length ? categItems.eq(0).outerHeight(true) : 22;
		var categH = categItems.length * cih;
		var minCategH = categItems.length > 1 ? 2 * cih : cih;
		var sih = subscrItems.eq(0).length ? subscrItems.eq(0).outerHeight(true) : 22;
		var subscrH = subscrItems.length * sih;
		var minSubscrH = subscrItems.length > 1 ? 2 * sih : sih;

		categ.height(categH);
		subscr.height(subscrH);

		// make datepicker sticky
		var pickerDiv = $('#calendarSidePanelCalendar').find(".fc-catlist-picker").removeClass("fixed");
		var makeSticky =
				pickerDiv.position().top + pickerDiv.outerHeight(true) > _content.height();
		if (makeSticky) {
			pickerDiv.addClass("fixed");
			_list.addClass("fixed");
			_list.css("bottom", pickerDiv.outerHeight(true) + "px");
		}

		// calc new heights
		var listH = _list.height();
		var otherH = (_list.find(".date-box").length ? _list.find(".date-box").outerHeight(true) : 0) +
				(_list.find(".content-h").length ? _list.find(".content-h").outerHeight(true) * 2 : 0);
		var scrollH = listH - otherH;
		var categSpace = categ.outerHeight(true) - categ.height();
		var subscrSpace = subscr.outerHeight(true) - subscr.height();
		var newCategH = Math.max(minCategH, Math.round(scrollH * categH / (categH + subscrH)) || 0);
		var newSubscrH = Math.max(minSubscrH, Math.round(scrollH * subscrH / (categH + subscrH)) || 0);
		var delta = newCategH + newSubscrH - scrollH;
		if (delta > 0) {
			if (newCategH > newSubscrH) {newCategH = newCategH - delta;}
			else                        {newSubscrH = newSubscrH - delta;}
		}
		newCategH = isSubscrHidden ? categItems.length * (cih + 1) : newCategH;
		if (isSubscrHidden && categHeight) {
		    var resultCategH = categHeight ? categHeight : newCategH - categSpace;
		    categ.css("height", resultCategH + "px");
		} else {
			if ((newCategH + _list.find(".content-h").outerHeight(true))> _list.height()) {
				newCategH = _list.height() - subscr.outerHeight(true) - _list.find(".content-h").outerHeight(true);
			}
		    categ.css("height", (newCategH - categSpace) + "px");
		}
	   
		subscr.css("height", (newSubscrH - subscrSpace) + "px");

		//var categJSP = categ.data("jsp");
		//categJSP.reinitialise();
		//if (subscr.length) subscr.data("jsp").reinitialise();

		//if (scrollToEnd) {
		//	categJSP.scrollToBottom();
		//}

		_list.css("visibility", "visible");
	    
		if (isCategoriesHidden) {
		    categ.addClass('hidden');
		    _list.find(".my_calendars").addClass('active');
		}
		if (isSubscrHidden) {
		    subscr.addClass('hidden');
		    _list.find(".other_calendars").addClass('active');
		}
	}

	function _rerenderList(flag, scrollToEnd) {
		if (flag == undefined || flag == kWholeList || flag == kCategoriesOnly) {
			_renderCategories.call(_this, _list);
		}
		if (flag == undefined || flag == kWholeList || flag == kSubscriptionsOnly) {
			_renderSubscriptions.call(_this, _list);
		}
		_resizeList.call(_this, scrollToEnd);
	}


	function _renderDatepicker() {
		var result = $(
				'<div class="fc-catlist-picker">' +
					'<div>' +
						'<div id="fc_datepicker" class="asc-datepicker"/>' +
						'<div class="hide">' +
							'<span>' +
								htmlEscape(calendar.options.categories.datepickerHideLabel) +
							'</span>' +
						'</div>' +
					'</div>' +
				'</div>');
		result.find(".hide span").click(function() {_showDatepicker.call(_this, !_dpickerIsVisible);});

		_datepicker = result.find("#fc_datepicker").datepicker({
			firstDay: calendar.options.firstDay,
			onChangeMonthYear: function(year, month, inst) {
				if (_dpSettingDate == true) {return;}
				_dpVisibleDate = new Date(year, month - 1, 1);
				setTimeout(function() {_updateDatepickerCells(_dpVisibleDate);}, 0);
			},
			onSelect: function(dateText, inst) {

			    var showDay = false;

			    if (jq("#asc_event .event-editor .editor").is(":visible")) {
			        if (confirm(ASC.Resources.Master.Resource.WarningMessageBeforeUnload)) {
			            window.toastr.remove();
			            jq("#asc_event").hide();
			            jq("#asc_calendar").show();
			            calendar.updateSize();
			            showDay = true;
			        }
			    } else {
			        window.toastr.remove();
			        jq("#asc_event").hide();
			        jq("#asc_calendar").show();
			        calendar.updateSize();
			        showDay = true;
			    }

			    if (showDay) {
			        _dpIgnoreUpdate = true;
			        _dpVisibleDate = _datepicker.datepicker("getDate");
			        calendar.gotoDate(_dpVisibleDate);
			        calendar.changeViewAndMode("day");
			        _dpIgnoreUpdate = false;
			        //
			        setTimeout(_updateDatepickerCells, 0);
			    }
			}
		});

        function initMonthMenu(directionUp) {
            fcMenus.dpMonthMenu = $("#fc_datepicker_mmenu").length ? $("#fc_datepicker_mmenu") : $("<div id='fc_datepicker_mmenu'/>");
            if (isOpenCommonDatePicker) {
                fcMenus.dpMonthMenu.css('z-index', 2001);
            }

			fcMenus.dpMonthMenu.popupMenu({
				anchor: directionUp ? "left,top" : "right,bottom",
				direction: directionUp ? "right,up" : "left,down",
				arrow: directionUp ? "down" : "up",
				arrowPosition: "50%",
				showArrow: false,
				cssClassName: "asc-popupmenu",
				items: calendar.options.monthNames,
                itemClick: function (event, data) {
                    _dpCommonVisibleDate = isOpenCommonDatePicker ? $('#fc_common_dp .asc-datepicker').datepicker("getDate") !== null ? $('#fc_common_dp .asc-datepicker').datepicker("getDate") : _dpCommonVisibleDate : _dpCommonVisibleDate;
                    var delta = isOpenCommonDatePicker ? data.itemIndex - _dpCommonVisibleDate.getMonth() : data.itemIndex - _dpVisibleDate.getMonth();
					adjustDatepickerDate(delta, "M");
				}
			});
		}

        function initYearMenu(y, directionUp) {
            if (isOpenCommonDatePicker) {
                fcMenus.dpYearMenu.css('z-index', 2001);
            }
			fcMenus.dpYearMenu.popupMenu({
				anchor: directionUp ? "left,top" : "right,bottom",
				direction: directionUp ? "right,up" : "left,down",
				arrow: directionUp ? "down" : "up",
				arrowPosition: "50%",
				showArrow: false,
				cssClassName: "asc-popupmenu",
                items: [
                    (y + 3).toString(),
                    (y + 2).toString(),
                    (y + 1).toString(),
                    "divider",
                    (y - 1).toString(),
                    (y - 2).toString(),
                    (y - 3).toString()
				],
                itemClick: function (event, data) {
                    _dpCommonVisibleDate = isOpenCommonDatePicker ? $('#fc_common_dp .asc-datepicker').datepicker("getDate") !== null ? $('#fc_common_dp .asc-datepicker').datepicker("getDate") : _dpCommonVisibleDate : _dpCommonVisibleDate;
                    var delta = isOpenCommonDatePicker ? parseInt(data.item, 10) - _dpCommonVisibleDate.getFullYear()  : parseInt(data.item, 10) - y;
					adjustDatepickerDate(delta, "Y");
				}
			});
		}

        function adjustDatepickerDate(delta, period) {
            if (delta != 0) {
                if (isOpenCommonDatePicker) {
                    if (period === "M") {
                        _dpCommonVisibleDate = addMonths(_dpCommonVisibleDate, delta);
                    } else if (period === "Y") {
                        _dpCommonVisibleDate = addYears(_dpCommonVisibleDate, delta);
                    } else {
                        return;
                    }
                    $('#fc_common_dp .asc-datepicker').datepicker("setDate", _dpCommonVisibleDate);
                } else {
                    if (period === "M") {
                        _dpVisibleDate = addMonths(_dpVisibleDate, delta);
                    } else if (period === "Y") {
                        _dpVisibleDate = addYears(_dpVisibleDate, delta);
                    } else {
                        return;
                    }
                    _dpSettingDate = true;
                    $.datepicker._adjustDate("#fc_datepicker", delta, period);
                    _dpSettingDate = false;
                    _updateDatepickerCells.call(_this, _dpVisibleDate);
                }
			}
		}

		initMonthMenu(true);

        $(document).on("click", "#fc_datepicker .ui-datepicker-title , #fc_common_dp .ui-datepicker-title", function(e) {
            var directionUp = $(e.target).offset().top > fcMenus.dpMonthMenu.outerHeight(true) + 10;

            isOpenCommonDatePicker = $("div").is("#fc_common_dp") && $("#fc_common_dp").is(':visible');

            if (e.target.className == "ui-datepicker-month") {
				initMonthMenu(directionUp);
				fcMenus.hideMenus(fcMenus.dpMonthMenu);
				fcMenus.dpMonthMenu.popupMenu("open", e.target);
				return;
			}
			if (e.target.className == "ui-datepicker-year") {
				if (!_dpYearMenuCaller || _dpYearMenuCaller != e.target) {
                    _dpYearMenuCaller = e.target;
                    _dpCommonVisibleDate = isOpenCommonDatePicker ? $('#fc_common_dp .asc-datepicker').datepicker("getDate") !== null ? $('#fc_common_dp .asc-datepicker').datepicker("getDate") : _dpCommonVisibleDate : _dpCommonVisibleDate;
					var y = isOpenCommonDatePicker ? _dpCommonVisibleDate.getFullYear() : _dpVisibleDate.getFullYear();
					if (!fcMenus.dpYearMenu || fcMenus.dpYearMenu.length < 1) {
						fcMenus.dpYearMenu = $("#fc_datepicker_ymenu").length ? $("#fc_datepicker_ymenu") : $("<div id='fc_datepicker_ymenu'/>");
					} else {
						fcMenus.dpYearMenu.popupMenu("close");
						fcMenus.dpYearMenu.popupMenu("destroy");
					}
					initYearMenu(y, directionUp);
				}
				fcMenus.hideMenus(fcMenus.dpYearMenu);
				fcMenus.dpYearMenu.popupMenu("open", e.target);
				return;
			}
		});
		return result;
	}

	function _showDatepicker(show) {
		_showVersion.call(_this);

		$.cookie('fc_show_minicalendar', !show ? "0" : "1", {expires: 365});

		var pickerDiv = $("#calendarSidePanelCalendar .fc-catlist-picker");
		var label = pickerDiv.find(".hide span");
		if (show == false) {
			if (_dpickerIsVisible) {
				_datepicker.hide();
				label.text(calendar.options.categories.datepickerShowLabel);
				_dpickerIsVisible = false;
			}
		} else {
			if (!_dpickerIsVisible) {
				_datepicker.show();
				label.text(calendar.options.categories.datepickerHideLabel);
				_dpickerIsVisible = true;
			}
		}

		var pickerH = $("#calendarSidePanelCalendar .fc-catlist-picker").outerHeight(true);
		_list.css("bottom", pickerH + "px");
		_resizeList.call(_this);
	}

	function _showVersion() {
		if (_clickStart === undefined) {_clickStart = (new Date()).getTime();}
		_clickCount = _clickCount ? _clickCount + 1 : 1;
		if (_clickCount >= 6) {
			if ((new Date()).getTime() - _clickStart <= 5000) {
				var id = "ver_" + (new Date()).getTime();
				$('<div class="fc-label-tooltip" style="padding:2em;" id="' + id + '">' +
						htmlEscape('FullCalendar v' + $.fullCalendar.version) + '</div>')
						.appendTo("body")
						.offset(calendar.getView().getViewRect());
				setTimeout(function() {$("#" + id).remove();}, 3000);
			}
			_clickStart = undefined;
			_clickCount = 0;
		}
	}

	function _updateDatepickerCells(startDate) {
		_datepicker.find(".ui-datepicker-prev .ui-icon")
				.removeClass("ui-icon-circle-triangle-w")
				.addClass("ui-icon-carat-1-w");
		_datepicker.find(".ui-datepicker-next .ui-icon")
				.removeClass("ui-icon-circle-triangle-e")
				.addClass("ui-icon-carat-1-e");
		// find visible date range
		var date = startDate instanceof Date ? startDate : calendar.getDate();
		var visStart = new Date(date.getFullYear(), date.getMonth(), 1);
		var days = visStart.getDay() - calendar.options.firstDay;
		addDays(visStart, -(days >= 0 ? days : days + 7));
		// get all events
		var events = calendar.clientEvents();
		
		var sd = new Date(visStart.getFullYear(), visStart.getMonth() - 1, 1);
		var ed = new Date(visStart.getFullYear(), visStart.getMonth() + 2, 1);
		
		//if (!events.length) {
			calendar.trigger("getMonthEvents", _this,
					$.extend({startDate: sd, endDate: ed}),
					function(response) {
						if (response.result) {
							var daysArr = response.days;
							
							// mark days with events
							_datepicker.find(".ui-datepicker-calendar tbody td").each(function(i, _cell) {
								var cell = $(_cell);
								
								for (var j = 0; j < daysArr.length; ++j) {
									
									var eventDate = clearTime(new Date(daysArr[j].getFullYear(), daysArr[j].getMonth(), daysArr[j].getDate() + days)).toString();
									var cellDate = clearTime(new Date(visStart.getFullYear(), visStart.getMonth(), visStart.getDate() + i + days)).toString();
									
									if (eventDate === cellDate) {
										cell.addClass("has-events");
										return;
									}
								}
							});
						}
					}
				);
			//}
		
		// mark days with events
		_datepicker.find(".ui-datepicker-calendar tbody td").each(function(i, _cell) {
			var cell = $(_cell);
			var date = addDays(cloneDate(visStart), i);
			for (var j = 0; j < events.length; ++j) {
				var ev = events[j];
				var start = clearTime(cloneDate(ev.start));
				var end = clearTime(cloneDate(ev.end || ev.start));
				if (start <= date && date <= end && (!ev.source || !ev.source.isHidden)) {
					//cell.addClass("has-events");
					return;
				}
			}
		});
	}

	function _updateDatepickerSize() {
		var newH = _content.find(".fc-catlist-picker").outerHeight(true);
		if (newH != _dpPrevHeight) {
			_resizeList.call(_this);
			_dpPrevHeight = newH;
		}
	}
	
	


	// Public Interface

	this.render = function() {
		_list = _renderList.call(_this);
		_content = $('<div class="fc-catlist asc-dialog"><div class="fc-modal"/></div>')
				.append(_list);
	    
		$('#calendarSidePanelCalendar').append(_renderDatepicker.call(_this));

		_categoryDialog = new CategoryDialog(calendar);
		_categoryDialog.setEventsHandlers({
			context:    _this,
			onClose:    _updateCategory,
			onDelete:   _deleteCategory
		});

		_subscriptionDialog = new SubscriptionDialog(calendar);
		_subscriptionDialog.setEventsHandlers({
			context:          _this,
			onClose:          _updateSubscription,
			onUnsubscribe:    _removeSubscription,
			onDelete:         _deleteCategory
		});

		_manageSubscriptionsDialog = new ManageSubscriptionsDialog(calendar);

		return _content;
	};

	this.rerenderList = function() {
		//if (!_content.is(":visible")) {return;}
		_rerenderList.call(_this);
	};

	this.resize = function(newHeight) {
		//if (!_content.is(":visible")) {return;}
		_content.css("height", newHeight + "px");
		_resizeList.call(_this);
	};

	this.updateDatepicker = function(newDate) {
		//if (!_content.is(":visible")) {return;}
		if (_datepicker.length < 1 || _dpIgnoreUpdate) {return;}

		_dpSettingDate = true;
		_dpVisibleDate = newDate;
		_datepicker.datepicker("setDate", newDate);
		_dpSettingDate = false;

		_updateDatepickerCells.call(_this);
	};

	this.updateDatepickerSize = function() {
		//if (!_content.is(":visible")) {return;}
		_updateDatepickerSize.call(_this);
	};

	this.updateDatepickerCells = function() {
		//if (!_content.is(":visible")) {return;}
		_updateDatepickerCells.call(_this);
	};

	this.showMiniCalendar = function(show) {
		//if (!_content.is(":visible")) {return;}
		_showDatepicker.call(_this, show);
	};
	
	this.addNewCalendar = function(anchor) {
		_categoryDialog.addNew(anchor);
	}

	this.addiCalCalendar = function(anchor) {
		_categoryDialog.addiCalCalendar(anchor);
	}
}


function TodoList(calendar) {
	var _this = this;

	var _list;
    var _listStates = {};
	var _editor;
	var _todo_viewer;
    var _anchor;
    var _curTodoElem;
    var _curTodo;
	var _sortType = 0;
	var _sortLabel;
	var _uiBlocker;
	var _date = null;
	var _deleteDialog;
	var _icalStream;
    var _minTime = -62135596800000;
    var ic = calendar.options.icalStream;

    function _parseDateTime(inputD, result) {
	    var dateStr = inputD.val();
	    var dateStrIsEmpty = dateStr.search(/\S/) < 0;
	    var date, time, r;
	    if (!dateStrIsEmpty) {
	        date = fcUtil.validateDateString(dateStr);
	        r = (date === null) ? 0 : 1;
	    } else {
	        r = -1;
	        date = null;
	        time = null;
	    }
	    result.date = { isEmpty: dateStrIsEmpty, isValid: date !== null, value: date };
	    return r;
	}
    
	function _doDDX(save, isViewer) {
		var todo = {
			title:       _editor.find("#fc_todo_title"),
			start_d:     _editor.find("#fc_todo_start_date"),
			start_t:     _editor.find("#fc_todo_start_time"),
			completed:   _editor.find("#fc_todo_completed"),
			priority:    _editor.find("#fc_todo_priority"),
			cal:         _editor.find("#fc_todo_cal"),
			description: _editor.find("#fc_todo_description"),
			ok:          _editor.find("#fc_todo_ok")
		};
	    var viewTodo = {
	        title: _todo_viewer.find(".title")[0],
	        start_d: _todo_viewer.find(".date-time .date")[0],
	        description: _todo_viewer.find(".description .text")[0],
	        mark_btn: _todo_viewer.find(".buttons .mark-btn")[0]
	    };
		var elem = $(_curTodoElem);
		var sources = calendar.getEventSources();
		var src = sources[elem.data("sourceIndex")];
		var newSource;
		var curTodo = src.todos[elem.data("todoId")];

		if (save) {    // ------------- SAVE data -------------

		    curTodo.title = $.trim(todo.title.val());
		   
		    if (false == fcUtil.validateInput(todo.title, fcUtil.validateNonemptyString)) { return false; }

			curTodo.completed = todo.completed.is(":checked");
			curTodo.description = $.trim(todo.description.val());
			curTodo.priority = todo.priority.val();
		    curTodo.completed = todo.completed == true ? true : false;

		    var date = {};
		    var validateDateResult = _validateDateFields(date);
		    
		    if (!validateDateResult && !date.data.date.isEmpty) { return false; }

		    curTodo.start = parseDate(date.data.date.value + "T23:59:59");

			var cal = todo.cal.val();
			if (cal != src.title) {
				for (var i = 0; i < sources.length; ++i) {
					if (sources[i].title != cal) {continue;}
					newSource = sources[i];
					newSource.todos = newSource.todos || [];
					newSource.todos.push(curTodo);
					src.todos =
							$.grep(src.todos, function(a){return a != curTodo;});
					break;
				}
			}
			_updateTodo(curTodo);
			_curTodoElem = undefined;
			_renderList.call(_this);
			_closeEditor.call(_this);

		} else {       // ------------- LOAD data -------------

			todo.title.val(curTodo.title.trim());
			todo.title.css("color", "").css("border-color", "");
			todo.start_d.css("color", "").css("border-color", "");
			todo.start_t.css("color", "").css("border-color", "");

			_editor.find(".header .title").text(curTodo.objectId ? window.g_fcOptions.todoEditor.dialogHeader_edit : window.g_fcOptions.todoEditor.dialogHeader_add);

			if (curTodo.completed == true) {
				todo.completed.prop("checked", true);
			} else {
				todo.completed.removeAttr("checked");
			}

		    var minDate = new Date(1, 0, 1);
		    if ((curTodo.start instanceof Date) && !(curTodo.start.getFullYear() <= minDate.getFullYear())) {
			    todo.start_d.val(formatDate(curTodo.start, calendar.options.eventEditor.dateFormat));
			    todo.start_t.val(formatDate(curTodo.start, calendar.options.eventEditor.timeFormat));
			} else {
		        todo.start_d.val(_date != null ? (formatDate(_date, calendar.options.eventEditor.dateFormat)) : "");
		        todo.start_t.val(_date != null ? "00:00" : "");
		        _date = null;
		    }

			todo.priority.val(curTodo.priority != undefined ? curTodo.priority : "2");

			var options = "";
			for (i = 0; i < sources.length; ++i) {
				var e = sources[i];
				options += "<option>" + htmlEscape(e.title) + "</option>";
			}
			todo.cal.html(options);
			todo.cal.val(src.title);

			todo.description.val(curTodo.description != undefined ? curTodo.description : "");
		    
			if (isViewer != undefined) {
			    
			    viewTodo.mark_btn.innerText = curTodo.completed ? calendar.options.todoViewer.dialogButton_mark_off : calendar.options.todoViewer.dialogButton_mark_on;
			        
			    viewTodo.title.innerText = curTodo.title;
			    viewTodo.description.innerText = curTodo.description != undefined ? curTodo.description : "";
			    if ((curTodo.start instanceof Date) && !(minDate.getTime() == curTodo.start.getTime())) {
			        viewTodo.start_d.innerText = formatDate(curTodo.start, calendar.options.eventEditor.dateFormat);
			    } else {
			        viewTodo.start_d.innerText = _date != null ? (formatDate(_date, calendar.options.eventEditor.dateFormat)) : "";
			        _date = null;
			    }
			}
		    _curTodo = curTodo;
		}
	}
	(function _createIcal() {
	    _icalStream = $(ic.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("add-popup")
				.popupFrame({
				    anchor: "right,top",
				    direction: "right,down",
				    offset: "0,0",
				    showArrow: false
				});

	    _icalStream.find(".buttons .cancel-btn, .header .close-btn").click(function () {
	        _closeTodoExportStream.call(_this);
	    });
	    _icalStream.find(".ical-description span")[0].innerHTML = ic.dialogTodoDescription;
	    
	    _icalStream.find((".url-link .control input")).click(function () {
	        $(this).select();
	    });
	    _icalStream.find((".url-link .control .button.copy")).click(function () {
	        var control = $($(this)[0].parentNode);
	        control.find('input').select();
	        try {
                document.execCommand('copy');
                ASC.CalendarController.displayInfoPanel(calendar.options.categories.dialogCopyMessage, false);
	        } catch (err) { }
	    });

	}());
    

	function _updateTodo(todo, resolve) {
	    var action = kTodoAddAction;
	    if (todo.objectId) {
	        action = kTodoChangeAction;
	    }
	    var isExistTodo = false;
	    calendar.trigger("editTodo", _this,
                $.extend({ action: action },todo),
                function (response) {
                    if (response.result) {
                        var sources = calendar.getEventSources();
                        var cache = calendar.getCache();
                        for (var i = 0; i < sources.length; ++i) {
                            var source = sources[i];
                            if (response.todo.length > 0) {
                                if (source.todos && source.objectId == response.todo[0].sourceId) {
                                      
                                    response.todo[0].completed = new Date(response.todo[0].completed).getTime() == _minTime ? false : true;
                                    response.todo[0].start = ASC.Api.TypeConverter.ServerTimeToClient(response.todo[0].start);
                                    response.todo[0].source = source;
                                    
                                    for (var k = 0; k < cache.length; k++) {
                                        if (cache[k].objectId == response.todo[0].objectId) {

                                            calendar.fetchEvents(calendar.getView().visStart, calendar.getView().visEnd, resolve);
                                            isExistTodo = true;
                                        }
                                    }
                                    if (!isExistTodo) {
                                        source.todos = source.todos.concat(response.todo[0]);
                                    }
                                    
                                }
                            }
                        }
                        if (!isExistTodo && response.todo[0]) {

                            calendar.fetchEvents(calendar.getView().visStart, calendar.getView().visEnd, resolve);
                        }
                    }

                });
    }
	function _openTodoContainer(_this) {
	    var $container = $(($(_this)[0].parentNode));
	    var date = $container.data('date') == null ? new Date(1, 0, 1, 23, 59, 59).getTime() : $container.data('date');
	    _listStates[date] = !_listStates[date];
	    $container.toggleClass('close');
	}
	function _adjustPopupPosition() {
	    var vr = calendar.getView().getViewRect();                               // current view's rect
	    var ar = fcUtil.getElementRect(_anchor);                                 // anchor element's rect
	    var pw = _todo_viewer.outerWidth(true);// + _settings.outerWidth(true) + 30;     // popup's width
	    var ph = _todo_viewer.outerHeight(true);                                      // popip's height

	    // adjust popup's ahchor, direction & arrow
	    if (ar.right + pw <= (vr.right - 250) || ar.left - pw < vr.left) {
	        _todo_viewer.popupFrame("option", "anchor", "right,top");
	        _todo_viewer.popupFrame("option", "direction", "right,down");
	        _todo_viewer.popupFrame("option", "arrow", "left");
	        _todo_viewer.popupFrame("option", "offset", "-4px,0");
	        _todo_viewer.popupFrame("updatePosition", _anchor);
	        pw = _todo_viewer.outerWidth(true);// + _settings.outerWidth(true) + 30;
	        var offs = ar.right + pw - vr.right;
	        offs = offs < 0 ? -4 : -offs;
	        _todo_viewer.popupFrame("option", "offset", offs + "px,0");
	    } else {
	        _todo_viewer.popupFrame("option", "anchor", "left,top");
	        _todo_viewer.popupFrame("option", "direction", "left,down");
	        _todo_viewer.popupFrame("option", "arrow", "right");
	        _todo_viewer.popupFrame("option", "offset", "2px,0");
	    }

	    _todo_viewer.popupFrame("updatePosition", _anchor);

	    // adjust popup's top & arrow position
	    var skipArrowPos = false;
	    if (ar.top < vr.top || ar.bottom > vr.bottom) {
	       // calendar.getView().resetScroll(_eventObj.start.getHours());
	        ar = fcUtil.getElementRect(_anchor);
	        if (ar.top < vr.top) {
	            skipArrowPos = true;
	            var m = _todo_viewer.popupFrame("option", "offset").match(/^([+-]?\d+(\.\d+)?)(px)?,([+-]?\d+(\.\d+)?)(px)?/i);
	            var newOffs = m[1] + (m[3] ? m[3] : "") + "," +
						(vr.top - ar.top + parseInt(m[4], 10)) + (m[6] ? m[6] : "");
	            _todo_viewer.popupFrame("option", "offset", newOffs);
	        }
	    }
	    var pt = (Math.max(ar.top, vr.top) + Math.min(ar.bottom, vr.bottom) - ph) * 0.5;
	    var ap = 50;
	    if (!skipArrowPos) {
	        if (pt < vr.top) {
	            ap = Math.round((ph * 0.5 - vr.top + pt) * 100 / ph);
	        } else if (pt + ph > vr.bottom) {
	            ap = Math.round((ph * 0.5 + pt + ph - vr.bottom) * 100 / ph);
	        }
	    }
	    _todo_viewer.popupFrame("option", "arrowPosition", ap + "%");
	}
	function _openViewer(todoElem, elem) {
	    _list.addClass("modal");
	    _uiBlocker.show();
	    _todo_viewer.popupFrame("option", "showArrow", true);
	    _anchor = elem;
	    _adjustPopupPosition.call(_this);
	    _todo_viewer.popupFrame("close");
	    _todo_viewer.popupFrame("open", elem);
	    _uiBlocker.addClass("fc-modal-transparent");
	    if (todoElem != _curTodoElem) {
	        _editor.popupFrame("close");
	        _curTodoElem = todoElem;
	        _doDDX.call(_this, false, true);
	    }
	}

	function _openEditor(todoElem) {
		if (todoElem != _curTodoElem) {
			_editor.popupFrame("close");
			_curTodoElem = todoElem;
			_doDDX.call(_this);
		}
		_list.addClass("modal");
		_uiBlocker.show();
		_editor.popupFrame("open", { pageX: "center", pageY: "center" }, todoElem);
		_editor.find("#fc_todo_title").focus();
	}

	function _updateTodoList() {
	    
        var sources = calendar.getEventSources();
        for (var i = 0; i < sources.length; ++i) {
            var source = sources[i];
            if (source.todos && source.todos.length > 0) {
                source.todos = source.todos.filter(function (todo) {
                    return !!todo.objectId;
                });
            }
        }
        _renderList.call(_this);
	    
    }

	function _closeDeleteDialog() {
	    _deleteDialog.popupFrame("close");
	    _uiBlocker.hide();
	    _curTodoElem = null;
	}

	function _closeEditor() {
		_editor.popupFrame("close");
		_uiBlocker.hide();
		_list.removeClass("modal");

		_updateTodoList();
		calendar.openEventWin = null;
	}
	function _closeViewer() {
	    _todo_viewer.popupFrame("close");
	    _uiBlocker.hide();
	    _uiBlocker.removeClass("fc-modal-transparent");
	    _list.removeClass("modal");
	    calendar.openEventWin = null;
	}

	function _compileTodos(_sources) {
	    var todos = [];
	    var todosObj = {};
	    var sources = _sources ? _sources : calendar.getEventSources();
		for (var i = 0; i < sources.length; ++i) {
		    var source = sources[i];
			if (source.todos && source.todos.length > 0) {
				for (var j = 0; j < source.todos.length; ++j) {
					source.todos[j].source = source;
					source.todos[j].sourceIndex = i;
					source.todos[j].id = j;

				    todosObj[source.todos[j].objectId] = source.todos[j];
				}
				
			}
		}
	    
		for (var k in todosObj) {
		    todos.push(todosObj[k]);
		}
		return todos;
	}
    
	function _openDelDialog(todoElem) {
	    
	    _list.addClass("modal");
	    _uiBlocker.show();
	    _curTodoElem = todoElem;
	    _deleteDialog.popupFrame("open", { pageX: "center", pageY: "center" });

	}

	var _formatDate = function (d, fmt, monthnames, daynames) {
	    var
          c = '',
          r = [],
          escape = false,
          hours = d.getHours(),
          isam = hours < 12,
          day = '',
	      dateName = '';
	    monthnames = monthnames || ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
	    daynames = daynames || ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

	    if (fmt.search(/%p|%P/) !== -1) {
	        if (hours > 12) {
	            hours = hours - 12;
	        } else if (hours == 0) {
	            hours = 12;
	        }
	    }
	    if (window.moment) {
	        day = window.moment(d).calendar(null, {
	            lastWeek: 'dddd DD.MM.YYYY',
	            sameElse: 'dddd DD.MM.YYYY',
	            nextWeek: 'dddd DD.MM.YYYY'
	        }).split(' ')[0];
	        dateName = window.moment(d).calendar(null, {
	            sameDay: 'D MMMM',
	            nextDay: 'D MMMM',
	            nextWeek: 'D MMMM',
	            lastDay: 'D MMMM',
	            lastWeek: 'D MMMM',
	            sameElse: 'D MMMM'
	        }).split(' ')[1];
	    } else {
	        day = daynames[d.getDay()];
	    }
	    for (var i = 0, n = fmt.length; i < n; ++i) {
	        c = fmt.charAt(i);
	        if (escape) {
	            switch (c) {
	                case 'h': c = '' + hours; break;
	                case 'H': c = leftPad(hours); break;
	                case 'M': c = leftPad(d.getMinutes()); break;
	                case 'S': c = leftPad(d.getSeconds()); break;
	                case 'd': c = '' + d.getDate(); break;
	                case 'm': c = '' + (d.getMonth() + 1); break;
	                case 'y': c = '' + d.getFullYear(); break;
	                case 'b': c = '' + window.moment ? dateName : monthnames[d.getMonth()]; break;
	                case 'p': c = (isam) ? ('' + 'am') : ('' + 'pm'); break;
	                case 'P': c = (isam) ? ('' + 'AM') : ('' + 'PM'); break;
	                case 'D': c = '' + day; break;
	            }
	            r.push(c);
	            escape = false;
	        } else {
	            if (c == '%') {
	                escape = true;
	            } else {
	                r.push(c);
	            }
	        }
	    }
	    return r.join('');
	};
	var getDisplayDate = function (date) {
	    var displaydate = _formatDate(date, '%d %b, %D', ASC.Resources.Master.MonthNamesFull, ASC.Resources.Master.DayNamesFull);
	    return displaydate !== '' ? displaydate : date.toLocaleTimeString();
	};
	var leftPad = function (n) {
	    n = '' + n;
	    return n.length === 1 ? '0' + n : n;
	};

	var resolveNewTodoItemList = function (resolve) {
	    var containerDate = resolve.containerDate;
	    newTodoItemList(containerDate);
    };
    var newTodoItemList = function (date) {
        var _date = Date.parse(date) != 0 ? date : null;
        var id = _addNewTodo.call(_this, _date);
        if (id) {
            _curTodoElem = undefined;
            _renderList.call(_this);
	                    
            var newElem = _findTodoElement.call(_this, id.sourceIndex, id.todoId);
            newElem.find(".label").val('');
            newElem.find(".label").click();
        }
    }
	var changeTodoTitle = function () {
	    
	    var elem = $($(this).parent());
	    var sources = calendar.getEventSources();
	    var src = sources[elem.data("sourceIndex")];
	    var curTodo = src.todos[elem.data("todoId")];

	    var $lbl = $(this), text = $lbl.text(),
	    $txt = $('<input type="text" class="editable-label-text" maxlength="150" value="' + text + '" />');

	    var container = $($(elem).parent()[0]);
	    var containerDate = container.data("date") != null ? new Date(container.data("date")) : null;
	    
	    elem.addClass('edit');

        $lbl.replaceWith($txt);
        $txt.focus();

	    $txt.blur(function () {
	        var newText = $(this).val();
	        if (false != fcUtil.validateInput($(this), fcUtil.validateNonemptyString)) {
	            $lbl.text(newText);
	            curTodo.title = newText;
	            _updateTodo(curTodo);
	            elem.removeClass('edit');
	            $txt.replaceWith($lbl);
	            $lbl.click(changeTodoTitle);
	            _updateTodoList();
	        } else {
	            if (curTodo.objectId == undefined) {
	                calendar.cleanEmptyEventSources();
	                elem.detach();
	            }
	        }
          
        })
        .keydown(function (evt) {
            if (evt.keyCode == 13) {
                var newText = $(this).val();
                if (false != fcUtil.validateInput($(this), fcUtil.validateNonemptyString)) {
                    $lbl.text(newText);
                    curTodo.title = newText;

                    _updateTodo(curTodo, { resolve: true, containerDate: containerDate });

                    $lbl.click(changeTodoTitle);
                    $txt.replaceWith($lbl);
                    elem.removeClass('edit');
                    _curTodoElem = undefined;
                }
                else {
                    if (curTodo.objectId == undefined) {
                        calendar.cleanEmptyEventSources();
                        $txt.blur();
                    }
                }
            }
        });
    }

	function unique(arr) {
	    var obj = {};
	    var result = [];

	    for (var i = 0; i < arr.length; i++) {
	        var str = arr[i].objectId;
	        obj[str] = arr[i]; 
	    }
	    for (var j in obj) {
	        result.push(obj[j]);
	    }

	    return result;
	}

	function _renderList(resolveTodoList) {
		var items = _list.find(".content .items");
		items.empty();
		var todos = _compileTodos.call(_this);
		var sortTodos = todos.sort(function (a, b) {
		    return a.start - b.start;
		});
		var minDate = new Date(1, 0, 1, 23, 59, 59);

	    var now = new Date();
	    var today = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59).valueOf();
	   
	    var lastTodoDate;
	    var container = null;
	    var counter = 0;

	    for (var j = 0; j < sortTodos.length; j++) {
	        var todo = sortTodos[j];

	        var todoDate = todo.start ? new Date(todo.start.getFullYear(), todo.start.getMonth(), todo.start.getDate(), 23,59,59).valueOf() : minDate.valueOf();

	        var containerClass = "";
	        var containerTitle = "";
	        
	        var day = todo.start ? getDisplayDate(todo.start).split(' ') : '';

	        counter++;
	        if (counter == sortTodos.length) {
	            if (container != null) container.appendTo(items);
	        }
	        if (todo.completed && calendar.hideCompletedTodos) {
	            continue;
	        }
	        
	        var li = $(
                    '<div class="content-li">' +
                        '<label><input type="checkbox"' + (todo.completed ? 'checked="checked"' : '') + ' class="bullet"/><span></span></label>' +
                        '<span class="label">' +
                            htmlEscape(todo.title ? todo.title : 'Unknown') + '</span> ' +
                         '<span class="description">' +
                            htmlEscape(todo.description ? todo.description : '') + '</span>' +
                        '<div class="edit-todo-button" title="' + htmlEscape(calendar.options.todoEditor.dialogHeader_edit) + '">' + calendar.options.todoList.editIcon + '</div>' +
                        '<div class="del-todo-button" title="' + htmlEscape(calendar.options.deleteTodoDialog.dialogHeader) + '">' + calendar.options.todoList.deleteIcon + '</div>' +
                    '</div>');


	        li.find(".bullet").change(function () {
	            _completeTodo.call(_this, $($(this).parent()).parent());
	        });
	        li.find(".label").click(changeTodoTitle);
	        
	        li.find(".edit-todo-button").click(function() {
	            _openEditor.call(_this, $(this).parent());
	        });
	        li.find(".del-todo-button").click(function () {
	            _openDelDialog.call(_this, $(this).parent());
	        });
	        
	        li.find(".description").click(function () {
                _openEditor.call(_this, $(this).parent());
	        });
	        if (todo.completed) {
	            li.addClass("completed");
	        }
	        li.data({ "sourceIndex": todo.sourceIndex, "todoId": todo.id });

	        if (lastTodoDate == todoDate) {
	            li.appendTo(container);
	        } else {
	            if (todoDate == minDate.valueOf()) {
	                containerClass = "no_due_date";
	                containerTitle = htmlEscape(calendar.options.todoList.noDueDate);
	            } else if ((todoDate < today - 86400000) || (todoDate < today)) {
	                if (container != null && container.hasClass('overdue')) {
	                    li.appendTo(container);
	                    continue;
	                } else {
	                    containerClass = "overdue";
	                    containerTitle = htmlEscape(calendar.options.todoList.overdue);
	                }
	               
	            } else if (todoDate == today) {
	                containerClass = "today";
	                containerTitle = day.length > 0 ? day[day.length - 1] : '';
	            } else if (86400000 + today == todoDate) {
	                containerClass = "tomorrow";
	                containerTitle = day.length > 0 ? day[day.length - 1] : '';
	            } else {
	                containerClass = "future";
	                containerTitle = getDisplayDate(todo.start);
	            }

                if (container == null) {
                    container = $('<div class="todo-container ' + containerClass + '"><span class="title">' + containerTitle + '</span><span class="add-todo"></span></div>');
                    container.data({ "date": containerClass == "no_due_date" ? 0 : containerClass == "overdue" ? -1 : todoDate });
                    container.appendTo(items);
                } else {
                    container.appendTo(items);
                    container = $('<div class="todo-container ' + containerClass + '"><span class="title">' + containerTitle + '</span><span class="add-todo"></span></div>');
                    container.data({ "date": containerClass == "no_due_date" ? 0 : containerClass == "overdue" ? -1 : todoDate });
                }

                if (containerClass == "no_due_date" && _listStates['0'] != undefined) {
                    container.addClass(_listStates['0'] ? 'close' : '');
                } else if (containerClass == "overdue" && _listStates['-1'] != undefined) {
                    container.addClass(_listStates['-1'] ? 'close' : '');
                } else if (_listStates[todoDate] != undefined) {
                    container.addClass(_listStates[todoDate] ? 'close' : '');
                }
                _listStates[todoDate] = container.hasClass('close');
                container.find(".title").click(function () {
                    _openTodoContainer(this);
                });

                container.find(".add-todo").click(function () {
                    var containerDate = $($(this).parent()[0]).data("date");

                    _date = containerDate != null && containerDate != 0 && containerDate != -1 ? new Date(containerDate) : null;
                    var id = _addNewTodo.call(_this, _date);
                    if (id) {
                        _curTodoElem = undefined;
                        _renderList.call(_this);

                        var todoContainers = _list.find(".todo-container");
                        for (var i = 0; i < todoContainers.length; i++) {
                            if ($(todoContainers[i]).data("date") == containerDate) {
                                if ($(todoContainers[i]).hasClass("close")) _openTodoContainer($(todoContainers[i]).find(".title"));
                            }
                        }
                        var newElem = _findTodoElement.call(_this, id.sourceIndex, id.todoId);
                        newElem.find(".label").val('');
                        newElem.find(".label").click();
                    }
	            });

	            li.appendTo(container);
	        }
	        lastTodoDate = todoDate;
	        if (counter == sortTodos.length) {
	            container.appendTo(items);
	        }
	    }
	    if (resolveTodoList != undefined && typeof resolveTodoList == "object" && resolveTodoList.resolve) {
	        resolveNewTodoItemList(resolveTodoList);
	    } 
	}

	function _renderMenu() {
		if (!fcMenus.todoMenu || fcMenus.todoMenu.length < 1) {
		    //fcMenus.todoMenu = $("<div id='fc_todo_menu'/>");
		    fcMenus.todoMenu = $(calendar.options.todoList.menuTemplate); 
		} else {
			fcMenus.todoMenu.popupMenu("close");
			fcMenus.todoMenu.popupMenu("destroy");
		}
		fcMenus.todoMenu.popupMenu({
		    anchor: "left,bottom",
		    direction: "right,down",
			arrow: "up",
			showArrow: false,
			cssClassName: "asc-popupmenu",
		    closeTimeout: -1
		});
	    
		fcMenus.todoMenu.find('#todo-in-cal-check').prop('checked', localStorageManager.getItem("showTodosInCalendar"));
		fcMenus.todoMenu.find('#todo-in-cal-check').change(function () {
		    _hideTodosInCalendar.call(_this); 
		});
	    
		fcMenus.todoMenu.find('#del-mark-td-check').prop('checked', localStorageManager.getItem("hideCompletedTodos"));
		fcMenus.todoMenu.find('#del-mark-td-check').change(function () {
		    _hideCompletedTodos.call(_this);
		});
		fcMenus.todoMenu.find('#sync-lnk').click(function () {
		    _openTodoExportStream.call(_this, { pageX: "center", pageY: "center" });
		});
		
	}

	function _getTodoCalendar() {
        var sources = calendar.getEventSources();
        for (var i = 0; i < sources.length; i++) {
            if (sources[i].isTodo == 1) {
                return sources[i];
            }
        }
        return null;
    }

	function _closeTodoExportStream() {
	    _icalStream.popupFrame("close");
	    _uiBlocker.hide();
	}

    var timerId;

    function updateTodoExportStream() {
        var todoCalendar = _getTodoCalendar();

        var linkContainer = _icalStream.find(".url-link.caldav");
        linkContainer.show();
        linkContainer.find(".control").removeClass('success').removeClass('failure').addClass('disabled').addClass('processing');
        linkContainer.find(".control input").attr('disabled', 'disabled');
            
        var counter = 0, preparingMessage = ic.dialogPreparingMessage;
        timerId = setInterval(function() {
            preparingMessage = preparingMessage + ".";
            if (counter == 4) {
                preparingMessage = ic.dialogPreparingMessage;
                counter = 0;
            }
            linkContainer.find(".control input").val(preparingMessage);
            counter++;
        }, 500);

        calendar.trigger("getCaldavUrl", _this, todoCalendar != null ? todoCalendar.objectId : 'todo_calendar', function (response) {
            clearInterval(timerId);
            if (response.result) {
                var preparingErrorMessage = ic.dialogPreparingErrorMessage;
                if (response.url != "" && linkContainer) {
                    linkContainer.find(".control input").val(response.url);
                    linkContainer.find(".control input").removeAttr("disabled");
                    linkContainer.find(".control").removeClass("disabled").removeClass('processing').addClass('success');
                } else {
                    linkContainer.find(".control input").val(preparingErrorMessage);
                    linkContainer.find(".control").removeClass('processing').addClass('failure');
                }
            }
        });

    }

    function _openTodoExportStream(anchor) {

        _icalStream.find((".url-link .control .button.try-again")).click(function () {
            clearInterval(timerId);
            updateTodoExportStream();
        });

        updateTodoExportStream();
        _uiBlocker.show();
        _icalStream.width(372);
        _icalStream.popupFrame("open", anchor);

    }

	function _renderSortMenu() {
		if (!fcMenus.sortMenu || fcMenus.sortMenu.length < 1) {
			fcMenus.sortMenu = $("<div id='fc_todo_sort_menu'/>");
		} else {
			fcMenus.sortMenu.popupMenu("close");
			fcMenus.sortMenu.popupMenu("destroy");
		}
		fcMenus.sortMenu.popupMenu({
			anchor: "right,top",
			direction: "left,up",
			arrow: "down",
			cssClassName: "asc-popupmenu",
			items: [
				{
					label: calendar.options.todoList.sortByCalendarLabel,
					click: function(event, data) {
						_sortType = 0;
						_sortLabel.text(calendar.options.todoList.sortByCalendarLabel);
						_renderList.call(_this);
					}
				},
				{
					label: calendar.options.todoList.sortByPriorityLabel,
					click: function(event, data) {
						_sortType = 1;
						_sortLabel.text(calendar.options.todoList.sortByPriorityLabel);
						_renderList.call(_this);
					}
				},
				{
					label: calendar.options.todoList.sortAlphabeticallyLabel,
					click: function(event, data) {
						_sortType = 2;
						_sortLabel.text(calendar.options.todoList.sortAlphabeticallyLabel);
						_renderList.call(_this);
					}
				}
			]
		});
	}
	function _validateDateFields(result) {
	    var dlg = {
	        date: _editor.find(".date")
	    };

	    var frDate = {}, fr = _parseDateTime(dlg.date, frDate);
	    frDate.dateTime = fr == 1 ? parseDate(frDate.date.value + "T23:59:59") : null;

	    var frc, r;
	    if (fr == 1) {
	        r = true;
	        frc = "";
	    } else {
	        r = false;
	        frc = frDate.date.isValid ? "" : "#cc3300";
	    }
	    dlg.date.css("color", "").css("border-color", frc);

	    if (result != undefined) {
	        result.data = frDate;
	    }
	    return r;
	}
	function _renderTodoEditor() {
		
	    _editor = $(calendar.options.todoEditor.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("fc-shadow")
				.popupFrame({
					anchor: "left,bottom",
					direction: "right,down",
					showArrow: false,
					showModal: true,
				});
	    _todo_viewer = $(calendar.options.todoViewer.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("fc-shadow")
				.popupFrame({
				    anchor: "right,top",
				    direction: "right,down",
				    offset: "-2px,0",
				    arrow: "left",
				    arrowPosition: "50%",
				    showArrow: false
				});
	    _deleteDialog = $(calendar.options.deleteTodoDialog.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("fc-shadow")
				.popupFrame({
				    anchor: "left,bottom",
				    direction: "right,down",
				    showArrow: false,
				    showModal: true,
				});
	    
	    var date = _editor.find(".date");
	    

	    function loadDate(input, dp, defDate) {
	        var d = parseISO8601(input.val());
	        dp.datepicker("setDate", (d instanceof Date) ? d : defDate);
	    }
	    function saveDate(input, dp) {
	        input.val(formatDate(
					dp.datepicker("getDate"),
					calendar.options.eventEditor.dateFormat));
	        input.change();
	    }
	    _editor.find(".cal-icon").click(function () {
	        fcDatepicker.open(calendar, this,
					function (dp) { loadDate(date, dp, null); },
					function (elem, dp) {
					    this.close();
					    saveDate(date, dp);
					});
	    });
	    
	    function getCaret(el) {
	        if (el.selectionStart) {
	            return el.selectionStart;
	        } else if (document.selection) {
	            el.focus();
 
	            var r = document.selection.createRange();
	            if (r == null) {
	                return 0;
	            }
	            var re = el.createTextRange(),
                        rc = re.duplicate();
	            re.moveToBookmark(r.getBookmark());
	            rc.setEndPoint('EndToStart', re);
 
	            return rc.text.length;
	        }
	        return 0;
	    }
	    _editor.keypress(function (e) {
	       if ((event.keyCode == 10 || event.keyCode == 13) && event.ctrlKey) {
	            var $fcTodoDescription = _editor.find('#fc_todo_description');
	            if($fcTodoDescription.is(":focus"))
	            {
	                var caret = getCaret(document.getElementById('fc_todo_description'));
	                var str = $fcTodoDescription.val();
	                var fersPat = str.substr(0, caret);
	                var secondPat = str.substr(caret);
	                $fcTodoDescription.val(fersPat + '\n' + secondPat);
	                document.getElementById('fc_todo_description').setSelectionRange(caret + 1, caret + 1);
	            }
	       }else if(e.which == 13) {
	           _doDDX.call(_this, true);
	       }
	    });
	    _editor.find(".time").mask("00:00");
	    _editor.find(".time, .date")
				.bind("keyup change", function(result) {
					if (!!$(this).val()) _validateDateFields(result);
					else $(this).css("border-color", "");
				});

	    _editor.find("#fc_todo_title").keyup(function () { fcUtil.validateInput(this, fcUtil.validateNonemptyString); });

		_editor.find("#fc_todo_ok").click(function() {
			_doDDX.call(_this, true);
		});
		_editor.find(".close-btn").click(function () {
		    _closeEditor.call(_this);
		});
		_editor.find("#fc_todo_cancel").click(function() {
			_closeEditor.call(_this);
		});
	    

		_deleteDialog.find(".save-btn").click(function () {
		    _deleteTodo.call(_this, _curTodoElem);
		});
		_deleteDialog.find(".close-btn").click(function () {
		    _closeDeleteDialog.call(_this);
		});
		_deleteDialog.find(".cancel-btn").click(function () {
		    _closeDeleteDialog.call(_this);
		});
	    
		_todo_viewer.find(".mark-btn").click(function () {
		    _completeTodo.call(_this, _curTodoElem);
		    _closeViewer.call(_this);
		});
		_todo_viewer.find(".edit-btn").click(function () {
		    _closeViewer.call(_this);
		    _openEditor.call(_this, _curTodoElem);
		});
		_todo_viewer.find(".delete-btn").click(function () {
		    _closeViewer.call(_this);
		    _openDelDialog.call(_this, _curTodoElem);
		});
				
	}

	function _addNewTodo(todaDate) {
		var sources = calendar.getEventSources();
		if (sources.length < 1) {return null;}
		var todo = {
			title: " ",
			completed: false,
			start: todaDate ? todaDate : null
		};
		sources[0].todos.push(todo);
		return {sourceIndex: 0, todoId: sources[0].todos.length - 1};
	}

	function _findTodoElement(sourceIndex, todoId) {
		var e = _list.find(".content .items .content-li").filter(function() {
					var li = $(this);
					return li.data("sourceIndex") == sourceIndex && li.data("todoId") == todoId;
				});
		return e.length > 0 ? e : null;
	}

	function _deleteCompletedTodos() {
		var sources = calendar.getEventSources();
		for (var i = 0; i < sources.length; ++i) {
			var src = sources[i];
			if (!src.todos || src.todos.length < 1) {continue;}
			for (var j = 0; j < src.todos.length; ++j) {
				if (src.todos[j].completed) {
					src.todos.splice(j, 1);
				}
			}
		}
		_renderList.call(_this);
	}

	function _hideTodosInCalendar() {
	    calendar.showTodosInCalendar = !calendar.showTodosInCalendar;
	    localStorageManager.setItem("showTodosInCalendar", calendar.showTodosInCalendar);
	    calendar.fetchEvents(calendar.getView().visStart, calendar.getView().visEnd);
	}
	function _hideCompletedTodos() {
	    calendar.hideCompletedTodos = !calendar.hideCompletedTodos;
        localStorageManager.setItem("hideCompletedTodos", calendar.hideCompletedTodos);
	    
	    _renderList.call(_this);
	    calendar.fetchEvents(calendar.getView().visStart, calendar.getView().visEnd);
	    
	}

	function _completeTodo(todoElem) {
		var elem = $(todoElem);
		var source = calendar.getEventSources()[elem.data("sourceIndex")];
		var todo = source.todos[elem.data("todoId")];
		todo.completed = !todo.completed;

		var _elem = $(todoElem);
		var sources = calendar.getEventSources();
		var src = sources[_elem.data("sourceIndex")];
		
		var curTodo = src.todos[elem.data("todoId")];
		_updateTodo(curTodo);
		_curTodoElem = undefined;
		if (todo.completed) _elem.addClass("completed");
		else _elem.removeClass("completed");
		//_renderList.call(_this);
	}

	function _deleteTodo(todoElem) {
	    var elem = $(todoElem);
	    var sources = calendar.getEventSources();
	    var todo = sources[elem.data("sourceIndex")].todos[elem.data("todoId")];
	    
	    calendar.trigger("editTodo", _this,
                 $.extend(
                         { action: kTodoDeleteAction },
                         todo),
                 function (response) {
                     if (response.result) {
                         
                         var cache = calendar.getCache();
                         for (var i = 0; i < sources.length; ++i) {
                             if (sources[i].todos && sources[i].todos.length > 0) {
                                 sources[i].todos = sources[i].todos.filter(function (_todo) {
                                     return _todo.objectId != todo.objectId;
                                 });
                             }
                         }
                         for (var j = 0; j < cache.length; ++j) {
                             if (cache[j].objectId == todo.objectId) {
                                 cache.splice(j, 1);
                                 break;
                             }
                         }
                         calendar.reportEvents(cache);
                         _renderList.call(_this);
                         _closeDeleteDialog.call(_this);
                     }

                 });
    }

	this.render = function() {
		var result = $(
				"<div>" +
					"<div class='fc-todo-list'>" +
						"<div class='content'>" +
							"<div class='content-h'>" +
								"<span class='label'>" + htmlEscape(calendar.options.todoList.title) + "</span>" +
								//"<span class='fc-dropdown'>&nbsp;</span>" +
								"<span class='fc-settings' title='" + htmlEscape(calendar.options.categories.subscriptionsManageLabel) + "'>" + calendar.options.todoList.settingsIcon + "</span>" +
                                "<div class='close-btn' title='" + htmlEscape(calendar.options.categories.dialogButton_cancel) + "'>&times;</div>" +
							"</div>" +
							"<div class='items'/>" +
				            "<span class='add-btn'>" +
				                htmlEscape(calendar.options.todoList.addTodoLabel) +
				            "<span class='add-todo'></span>" +
							"</span>" +
						/*	"<div class='sort-label'>" +
								"<span class='label'>" +
									htmlEscape(calendar.options.todoList.sortByCalendarLabel) + "</span>" +
								"<span class='fc-dropdown'>&nbsp;</span>" +
							"</div>" +*/
						"</div>" +
						"<div class='fc-modal'/>" +
					"</div>" +
				"</div>"
				);

		_list = result.find(".fc-todo-list");
		_sortLabel = result.find(".sort-label .label");

		result.find(".add-btn").click(function() {
		    _this.addNewTodo();
		});

		result.find(".content-h .close-btn").click(function () {
		    
		    var fc_container = $('#fc_container');
		    fc_container.toggleClass('open-todo-list');
		    fc_container.find('.fc-button-todo').toggleClass('fc-state-active');
		   
		});

		result.find(".content-h .label, .content-h .fc-settings").click(function() {
			fcMenus.hideMenus(fcMenus.todoMenu);
			fcMenus.todoMenu.popupMenu("open", $(this).parent().find(".label"));
		});
		result.find(".sort-label .label, .sort-label .fc-dropdown").click(function() {
			fcMenus.hideMenus(fcMenus.sortMenu);
			fcMenus.sortMenu.popupMenu("open", $(this).parent().find(".label"));
		});
		_uiBlocker = result.find(".fc-modal").click(function () {
            _closeEditor.call(_this);
            _closeDeleteDialog.call(_this);
            _closeTodoExportStream.call(_this);
            _closeViewer.call(_this);
		});

		_renderList.call(this);
		_renderMenu.call(this);
		_renderSortMenu.call(this);
		_renderTodoEditor.call(this);

		return result.children();
	};

	this.createTodo = function (todo) {
	    _updateTodo(todo);
	    _curTodoElem = undefined;
	    _renderList.call(_this);
	    _closeEditor.call(_this);
	};
    
	this.openTodoEditor = function (todo, elem) {
	    var todos = $('.fc-todo-list .content-li');
	    for (var i = 0; i < todos.length; i++) {
	        if ($(todos[i]).data("todoId") == todo.id) {
			    _openViewer.call(_this, todos[i], elem);
			}
		}
		
	};
    this.updateTodo = function(todo) {
        _updateTodo(todo);
    };

    this.getTodoElem = function() {
        return _curTodo;
    };
	

    this.rerenderList = function (resolveTodoList) {
        _renderList.call(this, resolveTodoList); 
	};

	this.resize = function(newHeight) {
		_list.css("height", newHeight + "px");
	};

	this.addNewTodo = function() {
		var id = _addNewTodo.call(_this);
		if (id) {
			_curTodoElem = undefined;
			_renderList.call(_this);
			var newElem = _findTodoElement.call(_this, id.sourceIndex, id.todoId);
			if (newElem) {
				_openEditor.call(_this, newElem[0]);
			}
		}
	};

	this.show = function() {
		_list.parent().removeClass("hidden");
		_list.show();
		calendar.updateSize();
	};

	this.hide = function() {
		_list.hide();
		_list.parent().addClass("hidden");
		calendar.updateSize();
	};

}


function EventEditor(calendar, uiBlocker) {
	var _this = this;
	
	// repeat settings options
	var rs = calendar.options.repeatSettings;
	var ds = calendar.options.deleteSettings;
	var cp = calendar.options.confirmPopup;

	var _modes = {
	    view:                       _viewMode,
	    todo:                       _todoMode,
	    edit:                       _editMode,
		addDialog:					_addDialogMode,
		addPopup:					_addPopupMode,
		addPopupDelSettings:		_addPopupDelSettings
	};
	
	var dwm = {
		day:     0,
		week:    1,
		month:   2,
		year:    3
	};

    var deleteMode = {
        single: 0,
        allFollowing: 1,
        allSeries: 2
    };
	
    var confirmSettings = {
        viewMode: {
            none: 0,
            addEvent: 1,
            updateEvent: 2,
            updateGuests: 3,
            deleteEvent: 4
        },
        notificationMode: {
            nobody: 0,
            partially: 1,
            everybody: 2
        },
        notificationType: {
            request: 1,
            update: 2,
            cancel: 4
        },
        notificationUsers: {
            newGuests: [],
            removedGuests: [],
            updatedGuests: []
        },
        selectedViewMode: 0,
        selectedNotificationMode: 0,
        selectedDeleteMode: 0
    };

	var _dialog;

	var alertType = kAlertDefault;								// number
    var repeatRule = ASC.Api.iCal.RecurrenceRule.EveryDay;	// standart or custom object rules
	var dwm_current = dwm.day;
	var dayRuleObject = undefined;
	
	var _settings;
	var _delSettings;
	var _confirmPopup;
	var _dialogMode;
	var _permissionsList;

	var _anchor;
	var _eventObj;
	var _todoObj;
	var _canEdit;
	var _canDelete;
	var _canChangeSource;
	var _canUnsubscribe;

	var formatDate = calendar.formatDate;
	var formatDates = calendar.formatDates;

    var _editModes = {
        event: 0,
        todo: 1
    };
    var _editMode = _editModes.event;

	(function _create() {
		_dialog = $(calendar.options.eventEditor.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("fc-shadow")
				.popupFrame({
					anchor: "right,top",
					direction: "right,down",
					offset: "-2px,0",
					arrow: "left",
					arrowPosition: "50%",
					showArrow: false
				});
		
		_dialog.find(".buttons .edit-btn").click(function() {
			//if (_canEdit) {
			//	_resetMode.call(_this);
			//	_dialogMode = false;
			//	_dialog.addClass("edit-popup");
			//	_dialog.popupFrame("updatePosition", _anchor);
				
			//	_enableRepeatSettings.call(_this);
			//	_setRepeatSettingsHeight.call(_this);
		    //}
		    _close.call(_this, false);
		    calendar.showEventPageViewer(_canEdit, _anchor, _eventObj);
		});

		function updateMode(currentBtn) {

		    _dialog.find(".buttonGroup span.active").removeClass('active');
		    currentBtn.addClass('active');
		    
			var mode;
			var s = calendar.getEventSources();
		    if (currentBtn.hasClass('event')) {
		        _editMode = _editModes.event;
				mode = 'edit';
				var calendarId = _dialog.find(".editor .calendar select").val();
				for (var i = 0; i < s.length; ++i) {
					if (s[i].objectId == calendarId) {
						_anchor.find(".fc-event-skin, .fc-event-skin-day")
							.css("border-color", s[i].backgroundColor)
							.css("background-color", s[i].backgroundColor)
							.css("color", s[i].textColor);
						$(_anchor.find(".fc-event-skin, .fc-event-skin-day")).parent()
							.css("border-color", s[i].backgroundColor)
							.css("background-color", s[i].backgroundColor)
							.css("color", s[i].textColor);
					}
				}
		    } else {
		        _editMode = _editModes.todo;
				mode = 'todo';
				for (var i = 0; i < s.length; ++i) {
					if (s[i].isTodo == 1) {
						_anchor.find(".fc-event-skin, .fc-event-skin-day")
							.css("border-color", s[i].backgroundColor)
							.css("background-color", s[i].backgroundColor)
							.css("color", s[i].textColor);
						$(_anchor.find(".fc-event-skin, .fc-event-skin-day")).parent()
							.css("border-color", s[i].backgroundColor)
							.css("background-color", s[i].backgroundColor)
							.css("color", s[i].textColor);
                    }
				}
		    }
		    _modes[mode].call(_this);
	    }

		_dialog.find(".buttonGroup span").click(function () {
			updateMode($(this));
		});
		_dialog.find(".buttonGroup span").on('touchstart', function (e) {
		    
		    updateMode($(this));
		    
		    e.preventDefault();
		    
		    _dialog.find('.title input').click().focus();
		});
		_dialog.find(".buttons .save-btn").click(function() {
		    if (jq(this).hasClass("disable"))
		        return;
		    
		    if (_editMode == _editModes.event) {
		        var hasSettings = !_settings.hasClass("hidden");
		        if (hasSettings) {
		            if (!_validateDateFieldsSettings.call(_this)) {
		                return;
		            }
		            _closeSettings.call(_this, hasSettings);
		        }
		    }
		    _close.call(_this, true);
		   
		});
		_dialog.find(".buttons .close-btn").click(function() {
		    _close.call(_this, false);
		    calendar.showEventPageViewer(_canEdit, _anchor, _eventObj);
		});
		_dialog.find(".buttons .delete-btn").click(function() {
			if (_canDelete) {
			    _openDelSettings.call(_this, "addPopupDelSettings", _eventObj.repeatRule.Freq == ASC.Api.iCal.Frequency.Never);
			}
		});
		_dialog.find(".buttons .unsubs-btn").click(function() {
			if (_canUnsubscribe) {
			    _unsubscribeEvent.call(_this);
			}
		});
	    
		_dialog.find(".buttons .view-details").click(function() {
		    _close.call(_this, false);
		    _doDDX.call(_this, true, true);
		    calendar.showEventPageViewer(_canEdit, _anchor, _eventObj);
		});
		
		_renderRepeatAlertList.call(_this);
		
		//
		_permissionsList = new PermissionsList(calendar, _dialog.find(".editor .shared-list"),
				calendar.options.sharedList.addLink);
		_permissionsList.setEventsHandlers({
					context:   _this,
					onResize: _resizePopup,
					onChange: _updatePermissions
				});
	    //
	    
		_dialog.find(".viewer .reply-buttons .accepted").click(function() {
		    if(!jq(this).hasClass("active"))
		        _eventReply(true);
		});
		_dialog.find(".viewer .reply-buttons .tentative").click(function() {
		    if(!jq(this).hasClass("active"))
		        _eventReply();
		});
		_dialog.find(".viewer .reply-buttons .declined").click(function() {
		    if(!jq(this).hasClass("active"))
		        _eventReply(false);
		});

		_dialog.find(".editor .title input").keyup(function() {fcUtil.validateInput(this, fcUtil.validateNonemptyString);});
		_dialog.find(".editor .all-day input").click(_handleAllDayClick);
		
		_dialog.find(".editor .all-day .label").css("cursor", "pointer").click(function() {
			_dialog.find(".editor .all-day .cb").trigger("click");
			_handleAllDayClick.call(_this);
		});
		//
		function loadDate(input, dp, defDate) {
			var d = parseISO8601(input.val());
			dp.datepicker("setDate", (d instanceof Date) ? d : defDate);
		}
		function saveDate(input, inputT, dp) {
			input.val(formatDate(
					dp.datepicker("getDate"),
					calendar.options.eventEditor.dateFormat));
			if (!inputT.is(":disabled") && $.trim(inputT.val()).length < 1) {
				inputT.val("00:00");
			}
			input.change();
		}
		var fromD = _dialog.find(".editor .from-date");
		var fromT = _dialog.find(".editor .from-time");
		
		var toD   = _dialog.find(".editor .to-date");
		var toT   = _dialog.find(".editor .to-time");
	    
		var todoD = _dialog.find(".editor .todo_editor .date");
		var todoT = _dialog.find(".editor .todo_editor .time");
	    
		_dialog.find(".editor .todo_editor .cal-icon").click(function () {
		    fcDatepicker.open(calendar, this,
					function (dp) { loadDate(todoD, dp, _eventObj.start); },
					function (elem, dp) {
					    this.close();
					    saveDate(todoD, todoT, dp);

					});
		});
	    
		
		_dialog.find(".editor .from.cal-icon").click(function() {
			fcDatepicker.open(calendar, this,
					function(dp) {loadDate(fromD, dp, _eventObj.start);},
					function(elem, dp) {
						this.close();
						saveDate(fromD, fromT, dp);
						
						var fromD_settings = _settings.find(".from-date");
						var fromT_settings = _settings.find(".from-time");
						saveDate(fromD_settings, fromT_settings, dp);
					});
		});
		_dialog.find(".editor .to.cal-icon").click(function() {
			fcDatepicker.open(calendar, this,
					function(dp) {loadDate(toD, dp, _eventObj.end||_eventObj.start);},
					function(elem, dp) {
						this.close();
						saveDate(toD, toT, dp);
					});
		});
		//
		_dialog.find(".editor .from-time, .editor .to-time").mask("00:00");
		_dialog
			.find(".editor .from-date, .editor .from-time, .editor .to-date, .editor .to-time")
			.bind("keyup change", function (result) {
				var r = _validateDateFields(result)
				if (r && !($(result.currentTarget).hasClass('from-time') || $(result.currentTarget).hasClass('to-time'))) {
					updateEditorView();
				}
			});
	    //
	    
		_dialog.find(".editor .todo_editor .time").mask("00:00");
		_dialog
				.find(".editor .todo_editor .date, .editor .todo_editor .time")
				.bind("keyup change", _validateTodoDateFields);
	    //
		_dialog.find(".editor .calendar select").change(function (ev) {
			var v = $(this).val();
			var s = calendar.getEventSources();
			for (var i = 0; i < s.length; ++i) {
				if (s[i].objectId != v) { continue; }
				if (s[i].isTodo == 1) { continue; }
				_dialog.find(".editor .calendar .bullet").css("background", s[i].backgroundColor);

			    if (_anchor.length) {
			        _anchor
			            .css("border-color", s[i].backgroundColor)
			            .css("background-color", s[i].backgroundColor)
						.css("color", s[i].textColor);
			        _anchor.find(".fc-event-skin, .fc-event-skin-day")
			            .css("border-color", s[i].backgroundColor)
			            .css("background-color", s[i].backgroundColor)
			            .css("color", s[i].textColor);
			    }

			    return;
			}
		});
		//
		if (uiBlocker && uiBlocker.length > 0) {
		    uiBlocker.click(function() {
		        if(_dialog.is(":visible")) {
		            _close.call(_this, false);
		            _closeSettings.call(_this, false);
		        }
		    });
		}
	}());

	function updateEditorView(isTodo) {
		_close.call(_this, false);

		var startStr = _dialog.find(".editor .from-date").val() + (_dialog.find(".editor .from-time").val() ? "T" + _dialog.find(".editor .from-time").val() : "T00:00:00");
		var endStr = _dialog.find(".editor .to-date").val() + (_dialog.find(".editor .to-time").val() ? "T" + _dialog.find(".editor .to-time").val() : "T00:00:00");

		var start = new Date(startStr);
		var end = new Date(endStr);
		var allDay = _dialog.find(".editor .all-day input").is(":checked");

		_eventObj.title = _dialog.find('.title input').val();

		_eventObj.start = start;
		_eventObj._start = start;
		_eventObj.end = isTodo ? start : end;
		_eventObj._end =  isTodo ? start : end;

		_eventObj.allDay = !isTodo ? allDay : true;
		_eventObj.location = _dialog.find(".editor .location input").val();

		var s = calendar.getEventSources();
		if (_dialog.find(".editor .calendar select").val() == null && !isTodo) {
			_eventObj.afretResizeSource = s[2];
			_eventObj.textColor = s[2].textColor;
			_eventObj.backgroundColor = s[2].backgroundColor;
			_eventObj.borderColor = s[2].borderColor;
		} else {
			for (var i = 0; i < s.length; ++i) {
				if (s[i].objectId == _dialog.find(".editor .calendar select").val() && !isTodo) {
					_eventObj.afretResizeSource = s[i];
					_eventObj.textColor = s[i].textColor;
					_eventObj.backgroundColor = s[i].backgroundColor;
					_eventObj.borderColor = s[i].borderColor;
				} else if (isTodo && s[i].isTodo == 1) {
					_eventObj.afretResizeSource = s[i];
					_eventObj.textColor = s[i].textColor;
					_eventObj.backgroundColor = s[i].backgroundColor;
					_eventObj.borderColor = s[i].borderColor;
				}
			}
        }
		

		calendar.renderEvent(_eventObj);
		_open.call(_this, "addPopup", calendar.getView().getEventElement(_eventObj), _eventObj);
	}

	function _showDaySections(){
		_settings.find(".fc-days-week").addClass("hidden");
		_settings.find(".fc-month-radio").addClass("hidden");
	}
		
	function _showWeekSections(){
		_settings.find(".fc-month-radio").addClass("hidden");
		_settings.find(".fc-days-week").removeClass("hidden");
	}
		
	function _showMonthSections(){
		_settings.find(".fc-days-week").addClass("hidden");
		_settings.find(".fc-month-radio").removeClass("hidden");
	}
	
	function _showEndSectionsNever(){
		_settings.find(".fc-repeat-cycles").addClass("hidden");
		_settings.find(".fc-end-date").addClass("hidden");
	}
	
	function _showEndSectionsCycles(){
		_settings.find(".fc-end-date").addClass("hidden");
		_settings.find(".fc-repeat-cycles").removeClass("hidden");
		_validateDateFieldsSettings.call(_this);
	}
	
	function _showEndSectionsDate(){
		_settings.find(".fc-repeat-cycles").addClass("hidden");
		_settings.find(".fc-end-date").removeClass("hidden");
		_validateDateFieldsSettings.call(_this);
	}
	
	function _getEndDate(mode) {
		var startDate = parseISO8601(_settings.find(".from-date").val());
		var interval = parseInt(_settings.find(".fc-interval-selector").find("option:selected").text(), 10);
		
		switch (mode) {
			case dwm.day: 
				return calendar.formatDate(new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate() + interval + 1, 0, 0, 0), rs.dateFormat);
			case dwm.week: 
				return calendar.formatDate(new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate() + interval*7 + 1, 0, 0, 0), rs.dateFormat);
			case dwm.month: 
				return calendar.formatDate(new Date(startDate.getFullYear(), startDate.getMonth() + interval, startDate.getDate() + 1, 0, 0, 0), rs.dateFormat);
			case dwm.year: 
				return calendar.formatDate(new Date(startDate.getFullYear() + interval, startDate.getMonth(), startDate.getDate() + 1, 0, 0, 0), rs.dateFormat);
			}
	}
	
	// create settings dialog
	(function _createSettings() {
		_settings = _dialog.find(".repeat-settings");
		
		_settings.find(".buttons .save-btn").click(function() {
		    if (jq(this).hasClass("disable"))
		        return;
		    
		    _closeSettings.call(_this, true);
		});
		_settings.find(".buttons .cancel-btn, .buttons .close-btn, .header .close-btn").click(function() {
			_closeSettings.call(_this, false);
		});
		
		// day/week/month selector
		var _DWMSelectorLabel = _settings.find(".fc-dwm-selector");
		
		_settings.find(".fc-dwm-selector").click(
			function (event) {
				
				if ($(this).find(".fc-selector-link").hasClass("not-active")) {
					return false;
				}
				
				fcMenus.hideMenus(fcMenus.modeMenuDWM);
				fcMenus.modeMenuDWM.popupMenu("open", _DWMSelectorLabel);
				event.stopPropagation();
			});
			
			if (!fcMenus.modeMenuDWM || fcMenus.modeMenuDWM.length < 1) {
				fcMenus.modeMenuDWM = $('<div id="fc_mode_menu_dwm"/>');
			} else {
				fcMenus.modeMenuDWM.popupMenu("close");
				fcMenus.modeMenuDWM.popupMenu("destroy");
			}
			fcMenus.modeMenuDWM.popupMenu({
				anchor: "left,bottom",
				direction: "right,down",
				arrow: "up",
				showArrow: false,
				closeTimeout: -1,
				cssClassName: "asc-popupmenu",
				items: [
					{
						label: rs.dialogRepeatOn_days,
						click: function() {
							_DWMSelectorLabel.find(".fc-selector-link").text(rs.dialogRepeatOn_days);
							_settings.find(".fc-interval-label").text(rs.dialogIntervalOption_day);
							dwm_current = dwm.day;
							_showDaySections.call(_this);
							_settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
							}
					},
					{
						label: calendar.options.repeatSettings.dialogRepeatOn_weeks,
						click: function() {
							_DWMSelectorLabel.find(".fc-selector-link").text(rs.dialogRepeatOn_weeks);
							_settings.find(".fc-interval-label").text(rs.dialogIntervalOption_week);
							dwm_current = dwm.week;
							_showWeekSections.call(_this);
							_settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
							}
					},
					{
						label: calendar.options.repeatSettings.dialogRepeatOn_months,
						click: function() {
							_DWMSelectorLabel.find(".fc-selector-link").text(rs.dialogRepeatOn_months);
							_settings.find(".fc-interval-label").text(rs.dialogIntervalOption_month);
							dwm_current = dwm.month;
							_showMonthSections.call(_this);
							_settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
							}
					},
					{
						label: calendar.options.repeatSettings.dialogRepeatOn_years,
						click: function() {
							_DWMSelectorLabel.find(".fc-selector-link").text(rs.dialogRepeatOn_years);
							_settings.find(".fc-interval-label").text(rs.dialogIntervalOption_year);
							dwm_current = dwm.year;
							_showDaySections.call(_this);
							_settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
							}
					}
				]
			});
			
		// end of repeat selector
		var _endRepeatSelectorLabel = _settings.find(".fc-endrepeat-selector");
		
		_settings.find(".fc-endrepeat-selector").click(
			function (event) {
				
				if ($(this).find(".fc-selector-link").hasClass("not-active")) {
					return false;
				}
				
				fcMenus.hideMenus(fcMenus.modeMenuEndRepeat);
				fcMenus.modeMenuEndRepeat.popupMenu("open", _endRepeatSelectorLabel);
				event.stopPropagation();
			});
			
			if (!fcMenus.modeMenuEndRepeat || fcMenus.modeMenuEndRepeat.length < 1) {
				fcMenus.modeMenuEndRepeat = $('<div id="fc_mode_menu_end_repeat"/>');
			} else {
				fcMenus.modeMenuEndRepeat.popupMenu("close");
				fcMenus.modeMenuEndRepeat.popupMenu("destroy");
			}
			fcMenus.modeMenuEndRepeat.popupMenu({
				anchor: "left,bottom",
				direction: "right,down",
				arrow: "up",
				showArrow: false,
				closeTimeout: -1,
				cssClassName: "asc-popupmenu",
				items: [
					{
						label: calendar.options.repeatSettings.dialogOptionNever,
						click: function() {
							_endRepeatSelectorLabel.find(".fc-selector-link").text(rs.dialogOptionNever);
							_showEndSectionsNever.call(_this);
							}
					},
					{
						label: calendar.options.repeatSettings.dialogOptionCount,
						click: function() {
							_endRepeatSelectorLabel.find(".fc-selector-link").text(rs.dialogOptionCount);
							_settings.find(".fc-cycle-times").val(3);
							_showEndSectionsCycles.call(_this);
							}
					},
					{
						label: calendar.options.repeatSettings.dialogOptionDate,
						click: function() {
							_endRepeatSelectorLabel.find(".fc-selector-link").text(rs.dialogOptionDate);
							_settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
							_showEndSectionsDate.call(_this);
							}
					}
				]
			});
		
		//
		function loadDate(input, dp, defDate) {
			var d = parseISO8601(input.val());
			dp.datepicker("setDate", (d instanceof Date) ? d : defDate);
		}
		function saveDate(input, inputT, dp) {
			input.val(formatDate(
					dp.datepicker("getDate"),
					rs.dateFormat));
			if (!inputT.is(":disabled") && $.trim(inputT.val()).length < 1) {
				inputT.val("00:00");
			}
			input.change();
		}
		
		var fromD = _settings.find(".from-date");
		var fromT = _settings.find(".from-time");
		var toD = _settings.find(".to-date");
		var toT = _settings.find(".to-time");
		
		_settings.find(".from.cal-icon").click(function() {
			
			if (_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").hasClass("not-active")) {
				return false;
			}
			
			fcDatepicker.open(calendar, this,
					function(dp) {loadDate(fromD, dp, _eventObj.start);},
					function(elem, dp) {
						this.close();
						saveDate(fromD, fromT, dp);
						
						var fromD_parent = _dialog.find(".from-date");
						var fromT_parent = _dialog.find(".from-time");
						saveDate(fromD_parent, fromT_parent, dp);
					});
		});
		
		_settings.find(".to.cal-icon").click(function() {
			
			if (_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").hasClass("not-active")) {
				return false;
			}
			
			fcDatepicker.open(calendar, this,
					function(dp) {loadDate(toD, dp, _eventObj.end||_eventObj.start);},
					function(elem, dp) {this.close();saveDate(toD, toT, dp);});
		});
		
		//
		_settings
				.find(".from-date, .to-date, .fc-cycle-times")
				.bind("keyup change", _validateDateFieldsSettings);
		//
		_settings.find(".calendar select").change(function (ev) {
			var v = $(this).val();
			var s = calendar.getEventSources();
			for (var i = 0; i < s.length; ++i) {
			    if (s[i].objectId != v) { continue; }
			    _settings.find(".calendar .bullet").css("color", s[i].backgroundColor);
				return;
			}
		});
		
	}());
	
	// create settings dialog
	(function _createDelSettings() {
		_delSettings = $(ds.dialogTemplate)
				.addClass("asc-dialog")
				.popupFrame({
					anchor: "right,top",
					direction: "right,down",
					offset: "0,0",
					showArrow: false
				});
				
		// radio
		function _setCheckedAttrValue(element, checked) {
			if ((element != undefined) && (element.length != 0)) {
                element.prop("checked", checked);
			}
		}
		
		_delSettings.find(".delete-selector .delete-this-label").click(function() {
			_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-this"), true);
		});
		
		_delSettings.find(".delete-selector .delete-following-label").click(function() {
			_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-following"), true);
		});
		
		_delSettings.find(".delete-selector .delete-all-label").click(function() {
			_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-all"), true);
		});
		
		_delSettings.find(".buttons .save-btn").click(function() {
		    if (jq(this).hasClass("disable"))
		        return;
		    
		    var delType = deleteMode.single;

		    if(!_delSettings.hasClass("single")) {
		        if (_delSettings.find(".delete-following").is(":checked")) {
				    delType = deleteMode.allFollowing;
			    }
			    else if (_delSettings.find(".delete-all").is(":checked")) {
				    delType = deleteMode.allSeries;
			    }
		    }
		    
			_closeDelSettings.call(_this, true);

			if (_getConfirmViewMode.call(_this)) {
			    confirmSettings.selectedDeleteMode = delType;
			    _openConfirmPopup.call(_this);
			} else {
			    _deleteEvent.call(_this, delType, _eventObj._start);
			    _close.call(_this, false, true);
			}

		});
		_delSettings.find(".buttons .cancel-btn, .buttons .close-btn, .header .close-btn").click(function() {
			_closeDelSettings.call(_this, false);
		});
		
	}());

    // create confirm dialog
	(function _createConfirmPopup() {
	    _confirmPopup = $(cp.dialogTemplate)
				.addClass("asc-dialog")
				.popupFrame({
				    anchor: "right,top",
				    direction: "right,down",
				    offset: "0,0",
				    showArrow: false
				});

	    _confirmPopup.find(".buttons .send-btn, .buttons .send-everyone-btn").click(function() {
	        _sendGuestsNotification.call(_this, confirmSettings.notificationMode.everybody);
	    });

	    _confirmPopup.find(".buttons .send-customs-btn").click(function() {
	        _sendGuestsNotification.call(_this, confirmSettings.notificationMode.partially);
	    });

	    _confirmPopup.find(".buttons .dont-send-btn").click(function() {
	        _sendGuestsNotification.call(_this, confirmSettings.notificationMode.nobody);
	    });
	    _confirmPopup.find(".header .close-btn").click(function() {
	        _closeConfirmPopup.call(_this, false);
	    });

	}());


	function _resizePopup() {
		if (!_dialogMode) {_adjustPopupPosition.call(_this);}
		_dialog.popupFrame("updatePosition", _anchor);
	}

	function _adjustPopupPosition() {
		var vr = calendar.getView().getViewRect();                               // current view's rect
		var ar = fcUtil.getElementRect(_anchor);                                 // anchor element's rect
		var pw = _dialog.outerWidth(true);// + _settings.outerWidth(true) + 30;     // popup's width
		var ph = _dialog.outerHeight(true);                                      // popip's height

		// adjust popup's ahchor, direction & arrow
		if (ar.right + pw <= (vr.right - _settings.outerWidth(true)) || ar.left - pw < vr.left) {
			_dialog.popupFrame("option", "anchor", "right,top");
			_dialog.popupFrame("option", "direction", "right,down");
			_dialog.popupFrame("option", "arrow", "left");
			_dialog.popupFrame("option", "offset", "-4px,0");
			_dialog.popupFrame("updatePosition", _anchor);
			pw = _dialog.outerWidth(true);// + _settings.outerWidth(true) + 30;
			var offs = ar.right + pw - vr.right;
			offs = offs < 0 ? -4 : -offs;
			_dialog.popupFrame("option", "offset", offs + "px,0");
		} else {
			_dialog.popupFrame("option", "anchor", "left,top");
			_dialog.popupFrame("option", "direction", "left,down");
			_dialog.popupFrame("option", "arrow", "right");
			_dialog.popupFrame("option", "offset", "2px,0");
		}

		_dialog.popupFrame("updatePosition", _anchor);

		// adjust popup's top & arrow position
		var skipArrowPos = false;
		if (ar.top < vr.top || ar.bottom > vr.bottom) {
			calendar.getView().resetScroll(_eventObj.start.getHours());
			ar = fcUtil.getElementRect(_anchor);
			if (ar.top < vr.top) {
				skipArrowPos = true;
				var m = _dialog.popupFrame("option", "offset").match(/^([+-]?\d+(\.\d+)?)(px)?,([+-]?\d+(\.\d+)?)(px)?/i);
				var newOffs = m[1] + (m[3] ? m[3] : "") + "," +
						(vr.top - ar.top + parseInt(m[4], 10)) + (m[6] ? m[6] : "");
				_dialog.popupFrame("option", "offset", newOffs);
			}
		}
		var pt = (Math.max(ar.top, vr.top) + Math.min(ar.bottom, vr.bottom) - ph) * 0.5;
		var ap = 50;
		if (!skipArrowPos) {
			if (pt < vr.top) {
				ap = Math.round((ph * 0.5 - vr.top + pt) * 100 / ph);
			} else if (pt + ph > vr.bottom) {
				ap = Math.round((ph * 0.5 + pt + ph - vr.bottom) * 100 / ph);
			}
		}
		_dialog.popupFrame("option", "arrowPosition", ap + "%");
	}

	function _resetMode() {
	    _dialog.removeClass("add-dialog add-popup edit-popup todo-popup");
		_dialogMode = undefined;
	}

	function _viewMode(elem, eventObj) {
		_resetMode.call(_this);
		_dialogMode = false;
		_dialog.popupFrame("option", "showArrow", true);
		_adjustPopupPosition.call(_this);
		_dialog.popupFrame("open", elem);
		uiBlocker.addClass("fc-modal-transparent");
	}
	
	function _editMode() {
		_resetMode.call(_this);
		updateEditorView(false);
	    _dialog.addClass("edit-popup");
	    _dialog.find('.title input').focus();
	}

	function _todoMode() {
		_resetMode.call(_this);
		updateEditorView(true);
	    _dialog.addClass("edit-popup todo-popup");
	    _dialog.find('.title input').focus();
	}

    function _getAlertLabel(type) {
		if (type != undefined) {
			switch (type) {
				case kAlertDefault: 		return calendar.options.eventEditor.dialogAlertOption_default;
				case kAlertNever:			return calendar.options.eventEditor.dialogAlertOption_never;
				case 1: 					return calendar.options.eventEditor.dialogAlertOption_5minutes;
				case 2: 					return calendar.options.eventEditor.dialogAlertOption_15minutes;
				case 3: 					return calendar.options.eventEditor.dialogAlertOption_30minutes;
				case 4: 					return calendar.options.eventEditor.dialogAlertOption_hour;
				case 5: 					return calendar.options.eventEditor.dialogAlertOption_2hours;
				case 6: 					return calendar.options.eventEditor.dialogAlertOption_day;
				default: 					return "Unknown";
			}
		}
	}
	
	function _getRepeatLabel(rule) {
		if (rule != undefined) {
			if (rule.Equals(ASC.Api.iCal.RecurrenceRule.Never)) {
				return calendar.options.eventEditor.dialogRepeatOption_never;
			}
			else if (rule.Equals(ASC.Api.iCal.RecurrenceRule.EveryDay)) {
				return calendar.options.eventEditor.dialogRepeatOption_day;
			}
			else if (rule.Equals(ASC.Api.iCal.RecurrenceRule.EveryWeek)) {
				return calendar.options.eventEditor.dialogRepeatOption_week;
			}
			else if (rule.Equals(ASC.Api.iCal.RecurrenceRule.EveryMonth)) {
				return calendar.options.eventEditor.dialogRepeatOption_month;
			}
			else if (rule.Equals(ASC.Api.iCal.RecurrenceRule.EveryYear)) {
				return calendar.options.eventEditor.dialogRepeatOption_year;
			}
			else {
				return calendar.options.eventEditor.dialogRepeatOption_custom;
			}
		}
	}
	
	function _disableRepeatSettings() {
		_settings.find(".fc-interval-selector").attr("disabled", "disabled");
		_settings.find("input").each(function(){
			el = $(this);
			el.attr("disabled", "disabled");
		});
		
		_settings.find(".fc-dwm-selector").find(".fc-selector-link").addClass("not-active");
		_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").addClass("not-active");
	}
	
	function _enableRepeatSettings() {
		_settings.find(".fc-interval-selector").removeAttr("disabled");
		_settings.find("input").each(function(){
			el = $(this);
			el.removeAttr("disabled");
		});
		
		_settings.find(".fc-dwm-selector").find(".fc-selector-link").removeClass("not-active");
		_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").removeClass("not-active");
	}
	
	function _setRepeatBackground(mode) {
		if (!mode) {		// old style
			_dialog.find(".repeat-alert").find(".right").css("background-color", "transparent");
		}
		else {				// new style
			_dialog.find(".repeat-alert").find(".right").css("background-color", "#f5f5f5");
		}
	}
	
	function _renderRepeatAlertList() {
		// repeat list
		var _repeatSelectorLabel = _dialog.find(".fc-view-repeat");
		
		_dialog.find(".fc-view-repeat").click(
			function (event) {
				fcMenus.hideMenus(fcMenus.modeMenuRepeat);
				fcMenus.modeMenuRepeat.popupMenu("open", _repeatSelectorLabel);
				event.stopPropagation();
			});
			
			if (!fcMenus.modeMenuRepeat || fcMenus.modeMenuRepeat.length < 1) {
				fcMenus.modeMenuRepeat = $('<div id="fc_mode_menu_repeat"/>');
			} else {
				fcMenus.modeMenuRepeat.popupMenu("close");
				fcMenus.modeMenuRepeat.popupMenu("destroy");
			}
			fcMenus.modeMenuRepeat.popupMenu({
				anchor: "left,bottom",
				direction: "right,down",
				arrow: "up",
				showArrow: false,
				closeTimeout: -1,
				cssClassName: "asc-popupmenu",
				items: [
					{
						label: calendar.options.eventEditor.dialogRepeatOption_never,
						click: function() {
							repeatRule = ASC.Api.iCal.RecurrenceRule.Never;
							_repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
							_closeSettings.call(_this, false);
							_dialog.find(".repeat-settings").addClass("hidden");
							_setRepeatBackground.call(_this, 0);
							_adjustPopupPosition.call(_this);
						}
					},
					{
						label: calendar.options.eventEditor.dialogRepeatOption_day,
						click: function() {
							repeatRule = ASC.Api.iCal.RecurrenceRule.EveryDay;
							_repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
							_closeSettings.call(_this, false);
							_dialog.find(".repeat-settings").addClass("hidden");
							_setRepeatBackground.call(_this, 0);
							_adjustPopupPosition.call(_this);
						}
					},
					{
						label: calendar.options.eventEditor.dialogRepeatOption_week,
						click: function() {
							repeatRule = ASC.Api.iCal.RecurrenceRule.EveryWeek ;
							_repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
							_closeSettings.call(_this, false);
							_dialog.find(".repeat-settings").addClass("hidden");
							_setRepeatBackground.call(_this, 0);
							_adjustPopupPosition.call(_this);
						}
					},
					{
						label: calendar.options.eventEditor.dialogRepeatOption_month,
						click: function() {
							repeatRule = ASC.Api.iCal.RecurrenceRule.EveryMonth;
							_repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
							_closeSettings.call(_this, false);
							_dialog.find(".repeat-settings").addClass("hidden");
							_setRepeatBackground.call(_this, 0);
							_adjustPopupPosition.call(_this);
						}
					},
					{
						label: calendar.options.eventEditor.dialogRepeatOption_year,
						click: function() {
							repeatRule = ASC.Api.iCal.RecurrenceRule.EveryYear;
							_repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
							_closeSettings.call(_this, false);
							_dialog.find(".repeat-settings").addClass("hidden");
							_setRepeatBackground.call(_this, 0);
							_adjustPopupPosition.call(_this);
						}
					},
					{
						label: calendar.options.eventEditor.dialogRepeatOption_custom,
						click: function() {
							_repeatSelectorLabel.find(".fc-selector-link").text(calendar.options.eventEditor.dialogRepeatOption_custom);
							_openSettings.call(_this);
							_dialog.find(".repeat-settings").removeClass("hidden");
							
							_enableRepeatSettings.call(_this);
							_setRepeatBackground.call(_this, 1);
							_adjustPopupPosition.call(_this);
							_setRepeatSettingsHeight.call(_this);
						}
					}
				]
			});
		
		// alert list  
		var _alertSelectorLabel = _dialog.find(".fc-view-alert");
		
		_dialog.find(".fc-view-alert").click(
			function (event) {
				fcMenus.hideMenus(fcMenus.modeMenuAlert);
				fcMenus.modeMenuAlert.popupMenu("open", _alertSelectorLabel);
				event.stopPropagation();
			});

			if (!fcMenus.modeMenuAlert || fcMenus.modeMenuAlert.length < 1) {
				fcMenus.modeMenuAlert = $('<div id="fc_mode_menu_alert"/>');
			} else {
				fcMenus.modeMenuAlert.popupMenu("close");
				fcMenus.modeMenuAlert.popupMenu("destroy");
			}
			fcMenus.modeMenuAlert.popupMenu({
				anchor: "left,bottom",
				direction: "right,down",
				arrow: "up",
				showArrow: false,
				closeTimeout: -1,
				cssClassName: "asc-popupmenu",
				
				items: [
					{
						label: calendar.options.eventEditor.dialogAlertOption_default,
						click: function() {
							alertType = kAlertDefault;
							_alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
						}
					},
					{
						label: calendar.options.eventEditor.dialogAlertOption_never,
						click: function() {
							alertType = kAlertNever;
							_alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
						}
					},
					{
						label: calendar.options.eventEditor.dialogAlertOption_5minutes,
						click: function() {
							alertType = 1;
							_alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
						}
					},
					{
						label: calendar.options.eventEditor.dialogAlertOption_15minutes,
						click: function() {
							alertType = 2;
							_alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
						}
					},
					{
						label: calendar.options.eventEditor.dialogAlertOption_30minutes,
						click: function() {
							alertType = 3;
							_alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
						}
					},
					{
						label: calendar.options.eventEditor.dialogAlertOption_hour,
						click: function() {
							alertType = 4;
							_alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
						}
					},
					{
						label: calendar.options.eventEditor.dialogAlertOption_2hours,
						click: function() {
							alertType = 5;
							_alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
						}
					},
					{
						label: calendar.options.eventEditor.dialogAlertOption_day,
						click: function() {
							alertType = 6;
							_alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
						}
					}
				]
			});
	}

	function _addDialogMode(elem, eventObj) {
		_anchor = {pageX:"center",pageY:"center"};
		_resetMode.call(_this);
		_dialogMode = true;
		_dialog.addClass("add-dialog");
		_dialog.popupFrame("option", "showArrow", false);
		_dialog.popupFrame("open", _anchor);
		uiBlocker.removeClass("fc-modal-transparent");
	}

	function _addPopupMode(elem, eventObj) {
		_resetMode.call(_this);
		_dialogMode = false;
		_dialog.addClass("add-popup");
		_dialog.popupFrame("option", "showArrow", true);
		_adjustPopupPosition.call(_this);
		_dialog.popupFrame("open", elem);
		uiBlocker.addClass("fc-modal-transparent");
	}
	
	function _addPopupDelSettings() {
		_anchor = {pageX:"center",pageY:"center"};
		_delSettings.popupFrame("open", _anchor);
		_delSettings.addClass("add-popup");
		_delSettings.addClass("fc-shadow");
		uiBlocker.removeClass("fc-modal-transparent");
	}

	function _renderPermissions() {
		_eventObj.permissions = _eventObj.permissions || {};
		_eventObj.permissions.users = _eventObj.permissions.users || [];

		var list = "";
		for (var i = 0; i < _eventObj.permissions.users.length; i++) {
			if (i > 0) {list += ", ";}
			list += _eventObj.permissions.users[i].name;
		}

	    if (calendar.options.isPersonal) {
	        _dialog.find(".shared-list").remove();
	    } else {
	        _dialog.find(".viewer .shared-list .users-list").html(htmlEscape(list));

	        if (list.length > 0) {
	            _dialog.find(".viewer .shared-list").addClass("has-users");
	        } else {
	            _dialog.find(".viewer .shared-list").removeClass("has-users");
	        }
	    }

		_permissionsList.render(_eventObj.permissions, kEventPermissions, _eventObj.objectId);
	}

	function _updatePermissions(response) {
		if (response.result) {
			_eventObj.permissions = response.permissions;
			if (response.isShared != undefined) {
				_eventObj.isShared = response.isShared;
			}
			_renderPermissions.call(_this);
			_resizePopup();
			_setRepeatSettingsHeight.call(_this);
		}
	}


	function _open(mode, elem, eventObj) {
		_dialog.popupFrame("close");
		uiBlocker.hide();
		
		_dialog.find(".repeat-settings").addClass("hidden");
		_setRepeatBackground.call(_this, 0);
		
		// check length
		var titleMaxLen = defaults.eventMaxTitleLength;
		var inputTxt = _dialog.find(".editor .title input, .editor .location input");
		if (inputTxt.length > 0)
		{
			inputTxt.keyup(function(){
				if (inputTxt.val().length > titleMaxLen){
					inputTxt.val(inputTxt.val().substr(0, titleMaxLen));
				}
			});
		}
		//

		_eventObj = eventObj;
		_eventObj.sourceId = _eventObj.sourceId || (_eventObj.source ? _eventObj.source.objectId : undefined);
		
		repeatRule = _eventObj.repeatRule;

		_canChangeSource = _eventObj.source == undefined ||
				_eventObj.source && !_eventObj.source.isSubscription;
		_canDelete = fcUtil.objectIsEditable(_eventObj) ||
				fcUtil.objectIsEditable(_eventObj.source) ||
				_eventObj.source && !_eventObj.source.isSubscription;
		_canEdit = _eventObj.source == undefined || _canDelete;

		_canUnsubscribe = _eventObj && _eventObj.canUnsubscribe &&
				_eventObj.source && _eventObj.source.isSubscription;

		if (_canEdit) {
			_dialog.find(".buttons").addClass("editable");
		} else {
			_dialog.find(".buttons").removeClass("editable");
		}
		if (_canDelete) {
			_dialog.find(".buttons").addClass("erasable");
		} else {
			_dialog.find(".buttons").removeClass("erasable");
		}
		if (_canEdit || _canDelete) {
			_dialog.find(".buttons").removeClass("readonly");
		} else {
			_dialog.find(".buttons").addClass("readonly");
		}
		if (_canUnsubscribe) {
			_dialog.find(".buttons").addClass("shared");
		} else {
			_dialog.find(".buttons").removeClass("shared");
		}
		if (eventObj.allDay && (eventObj.start == eventObj.end || eventObj.end == null)) {
			_dialog.find(".viewer .date-time .right").hide();
		} else {
			_dialog.find(".viewer .date-time .right").show();
		}

		_doDDX.call(_this);
		
		_anchor = elem;
		_modes[mode].call(_this, elem, eventObj);
		_resizePopup();
	    _dialog.find(".editor .title input").focus().select();

		_setRepeatSettingsHeight.call(_this);
		
		$(document).bind("keyup", _checkEscKey);

		if (_dialog.popupFrame("isVisible")) {
			uiBlocker.show();
		} else {
			uiBlocker.hide();
		}
	}
	
	function _setRepeatSettingsHeight() {
		_settings.height("auto");
		_settings.height( _dialog.find(".end-point").offset().top - _dialog.find(".start-point").offset().top - 
																	(_dialog.hasClass("add-dialog") ? _dialog.find(".header").outerHeight() : 0) );
	}

	// repeat interval
	function _setRepeatInterval(selectedItem, count) {
		if (selectedItem <= count) {
			var intervals = "";
			for (var i = 1; i < count; i++) {
				intervals += "<option" + ((selectedItem == i) ? " selected" : "") + " value='" + i + "'>" + i + "</option>";
			}
			
			var html = $(intervals);
			_settings.find(".fc-interval-selector").html(html);
			
			_settings.find(".fc-interval-selector").change(function() {
				_settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
			});
		}
	}
	
	// repeat days of week
	function _setSelectedDays(indexArr) {
		var daysSection = "";

	    var dayNamesInCurCulture;
	    var beforeFirstDay = [];
	    var afterFirstDay = [];

	    for (var i = 0; i < rs.dayNamesShort.length; i++) {
	        var item = { index: i, name: rs.dayNamesShort[i] };
	        if(i < g_fcOptions.firstDay){
	            beforeFirstDay.push(item);
	        } else {
	            afterFirstDay.push(item);
	        }
	    }
	    dayNamesInCurCulture = afterFirstDay.concat(beforeFirstDay);

	    for (var i = 0; i < dayNamesInCurCulture.length; i++) {
			var isSelected = false;
			for (var j = 0; j < indexArr.length; j++) {
				if (dayNamesInCurCulture[i].index == indexArr[j]) {
					isSelected = true;
					break;
				}
			}
			
			daysSection += 	"<div class='checkbox-day'>" +
								"<input class='repeat-day-" + dayNamesInCurCulture[i].index + "' type='checkbox' " + ((isSelected == true) ? "checked" : "") + ">" +
								"<div>" + htmlEscape(dayNamesInCurCulture[i].name) + "</div>" +
							"</div>";
		}
		daysSection += "<div class='clear'></div>";
		var html = $(daysSection);
		_settings.find(".fc-days-week").html(html);
	}
	function _getSelectedDays() {
		indexArr = new Array();
		if (_settings.find(".fc-days-week.hidden").length == 0) {
			for (var i = 0; i < rs.dayNamesShort.length; i++) {
				var checkedClass = ".repeat-day-" + i;
				var checkedItem = _settings.find(checkedClass).is(":checked");
				if (checkedItem == true) {
					indexArr.push(i);
				}
			}
		}
		return indexArr;
	}
	
	function _getRepeatDayByIndex(index) {
		switch (index) {
			case 0: return ASC.Api.iCal.DayOfWeek.Sunday;
			case 1: return ASC.Api.iCal.DayOfWeek.Monday;
			case 2: return ASC.Api.iCal.DayOfWeek.Tuesday;
			case 3: return ASC.Api.iCal.DayOfWeek.Wednesday;
			case 4: return ASC.Api.iCal.DayOfWeek.Thursday;
			case 5: return ASC.Api.iCal.DayOfWeek.Friday;
			case 6: return ASC.Api.iCal.DayOfWeek.Saturday;
		}
	}
	
	function _openSettings()
	{
		var fromD = _dialog.find(".editor .from-date");
		_settings.find(".from-date").val(fromD.val());
		_validateDateFieldsSettings.call(_this);
		
		var date = parseISO8601(fromD.val());
		var endDate = undefined;
		
		var maxDayCount = 32 - new Date(date.getFullYear(), date.getMonth(), 32).getDate();
		var dayOfWeek = date.getDay();
		var dayOfMonth = date.getDate();
		
		var indexDay = Math.floor((dayOfMonth - 1) / 7);
		var maxIndexDay = indexDay + Math.floor((maxDayCount - dayOfMonth) / 7);
		
		var dayName = calendar.options.repeatSettings.dayNames[dayOfWeek];
		var dayAlias = "";
		
		switch (indexDay) {
			case 0:
			case 1:
			case 2:
			case 4: dayAlias = rs.dayAliasNames[indexDay]; break;
			case 3: (indexDay != maxIndexDay) ? dayAlias = rs.dayAliasNames[indexDay] : dayAlias = rs.dayAliasNames[indexDay + 1]; break;
		}
		
		var html = $("<div>" + 
						"<div style='margin-bottom: 0.5em;'>" +
						"<input class='only-day' type='radio' name='day-radio' value='" + dayOfMonth + "' />&nbsp;" + 
							"<span class='only-day-label'>" + rs.dialogEachLabel + " " + dayOfMonth + " " + rs.dialogIntervalOption_day + "</span></div>" +
					 	"<input class='each-day' type='radio' name='day-radio' value='" + dayName + "'/>&nbsp;" + 
					 		"<span class='each-day-label'>" + rs.dialogAliasLabel + " " + dayAlias + " " + dayName + "</span>" +
					 "</div>");
					 
		_settings.find(".fc-month-radio").html(html);
		_settings.find(".only-day").attr({checked: "checked"});
		
		_settings.find(".only-day-label").click(function() {
			_settings.find(".only-day").trigger("click");
		});
		
		_settings.find(".each-day-label").click(function() {
			_settings.find(".each-day").trigger("click");
		});
		
		dayRuleObject = undefined;
		_settings.find(".each-day").click(function () {
			dayRuleObject = _getRepeatDayByIndex.call(_this, dayOfWeek);
			var dayRuleIndex = 0;
			
			switch (indexDay) {
				case 0:
				case 1:
				case 2: dayRuleIndex  = indexDay + 1; break;
				case 3: dayRuleIndex  = -2; break;
				case 4: dayRuleIndex  = -1; break;
			}
			if (indexDay == maxIndexDay) {
				dayRuleIndex = -1;
			}
			
			dayRuleObject.Order = dayRuleIndex;
		});
		
		_settings.find(".only-day").click(function () {
			dayRuleObject = undefined;
		});
		
		_setRepeatInterval.call(this, repeatRule.Interval, 31);
		_settings.find(".fc-cycle-times").val((repeatRule.Count > 0) ? repeatRule.Count : 3);
		
		var dayIndexArr = new Array();
		for (var i = 0; i < repeatRule.ByDay.length; i++) {
			dayIndexArr.push(rs.dayIndexResponse[repeatRule.ByDay[i].Id]);
		}
		
		_setSelectedDays.call(this, dayIndexArr.length ? dayIndexArr : [dayOfWeek]);
		
		// set default view
		if ((repeatRule.Freq == ASC.Api.iCal.Frequency.Never) || (repeatRule.Freq == ASC.Api.iCal.Frequency.Daily)) {
			_showDaySections.call(_this);
			dwm_current = dwm.day;
			_settings.find(".fc-dwm-selector").find(".fc-selector-link").text(rs.dialogRepeatOn_days);
			_settings.find(".fc-interval-label").text(rs.dialogIntervalOption_day);
		}
		else if (repeatRule.Freq == ASC.Api.iCal.Frequency.Weekly) {
			_showWeekSections.call(_this);
			dwm_current = dwm.week;
			_settings.find(".fc-dwm-selector").find(".fc-selector-link").text(rs.dialogRepeatOn_weeks);
			_settings.find(".fc-interval-label").text(rs.dialogIntervalOption_week);
		}
		else if (repeatRule.Freq == ASC.Api.iCal.Frequency.Monthly) {
			_showMonthSections.call(_this);
			dwm_current = dwm.month;
			_settings.find(".fc-dwm-selector").find(".fc-selector-link").text(rs.dialogRepeatOn_months);
			_settings.find(".fc-interval-label").text(rs.dialogIntervalOption_month);
			
			if ((repeatRule.ByDay.length == 1) && (repeatRule.ByDay[0].Order != 0)) {
				_settings.find(".only-day").attr({checked: ""});
				_settings.find(".each-day").attr({checked: "checked"}).click();
			}
			else {
				_settings.find(".each-day").attr({checked: ""});
				_settings.find(".only-day").attr({checked: "checked"});
			}
		} else if (repeatRule.Freq == ASC.Api.iCal.Frequency.Yearly) {
			_showDaySections.call(_this);
			dwm_current = dwm.year;
			_settings.find(".fc-dwm-selector").find(".fc-selector-link").text(rs.dialogRepeatOn_years);
			_settings.find(".fc-interval-label").text(rs.dialogIntervalOption_year);
		}
		
		_settings.find(".to-date").val(repeatRule.Until ? calendar.formatDate(repeatRule.Until, rs.dateFormat) : 
																	_getEndDate.call(_this, dwm_current));
		
		// end repeat
		if (repeatRule.Until) {
			_showEndSectionsDate.call(_this);
			_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").text(rs.dialogOptionDate);
		}
		else if (repeatRule.Count > 0) {
			_showEndSectionsCycles.call(_this);
			_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").text(rs.dialogOptionCount);
		}
		else {
			_showEndSectionsNever.call(_this);
			_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").text(rs.dialogOptionNever);
		}
	}
	
	function _openDelSettings(mode, single)
	{
	    _dialog.popupFrame("close");
	    $(document).unbind("keyup", _checkEscKey);

	    if (single)
	        _delSettings.addClass("single");
	    else
	        _delSettings.removeClass("single");

	    _delSettings.popupFrame("close");
		_delSettings.find("input[type=radio]:first").prop("checked", true);
		uiBlocker.hide();
		
		_modes[mode].call(_this);
		
		$(document).bind("keyup", _checkEscKeyDelSettings);

		if (_delSettings.popupFrame("isVisible")) {
			uiBlocker.show();
		} else {
			uiBlocker.hide();
		}
	}

	function _close(changed, deleted) {
	    calendar.openEventWin = null;
	    if (changed) {
	        if (_editMode == _editModes.event) {
	            if (!_canEdit || false == _doDDX.call(_this, true)) {
	                return;
	            }
	            _updateEvent.call(_this);
	        } else {
	            if (false == _doDDX.call(_this, true)) {
	                return;
	            }
	            _createTodo.call(_this);
	        }
	        return;
	    }
	    if (_eventObj.source == undefined && _eventObj._id != undefined) {
	        calendar.removeEvents(_eventObj._id);
	    }

	    if ((_eventObj.source != undefined) && !changed && !deleted)
	        _updateEvent.call(_this, true);
	    $(document).unbind("keyup", _checkEscKey);
	    _closeDialog.call(_this);
	    
	}
	
	function _closeSettings(changed) {
		if (changed && _canEdit) {
			_prepareRepeatRule.call(_this);
		}
	}
	
	function _closeDelSettings(changed) {
		if (changed) {
			if (!_canEdit) {return;}
			_closeDialogDelSettings.call(_this);
			return;
		}
		
		$(document).unbind("keyup", _checkEscKeyDelSettings);
		_closeDialogDelSettings.call(_this);
	}

	function _checkEscKey(ev) {
		if (ev.which == 27) {
			_close.call(_this, false);
		}
	}
	
	function _checkEscKeyDelSettings(ev) {
		if (ev.which == 27) {
			_closeDelSettings.call(_this, false);
		}
	}

	function _closeDialog() {
		fcDatepicker.close();
		_dialog.popupFrame("close");
		uiBlocker.hide();
	}
    
	function _disableDialogBtns(disable) {
	    if (disable){
	        _dialog.find(".buttons .save-btn").addClass("disable");
	    } else {
	        _dialog.find(".buttons .save-btn").removeClass("disable");
	    }
	}
	
	function _closeDialogDelSettings() {
	    _delSettings.popupFrame("close");
	    uiBlocker.hide();
	}


	function _getConfirmViewMode() {

	    confirmSettings.notificationUsers.newGuests = [];
	    confirmSettings.notificationUsers.updatedGuests = [];
	    confirmSettings.notificationUsers.removedGuests = [];
	    confirmSettings.selectedViewMode = confirmSettings.viewMode.none;
	    confirmSettings.selectedNotificationMode = confirmSettings.notificationMode.nobody;

	    if(_eventObj.status == 2 || (_eventObj.source && _eventObj.source.isSubscription))
	        return confirmSettings.selectedViewMode;

	    var isOrganizer = false;
	    var organizerEmail = _eventObj.organizer ? _eventObj.organizer[3].replace(new RegExp("mailto:", "ig"), "").toLowerCase() : "";

	    jq.each(ASC.Mail.Accounts, function(index, account) {
	        if (account.enabled && organizerEmail == account.email.toLowerCase()) {
	            isOrganizer = true;
	            return false;
	        }
	        return true;
	    });

	    if(!isOrganizer)
	        return confirmSettings.selectedViewMode;

	    var guests = [];
	    jq.each(_eventObj.attendees || [], function(index, attendeeObj) {
	        var attendeeEmail = attendeeObj[3].replace(new RegExp("mailto:", "ig"), "");
	        if (attendeeEmail.toLowerCase() != organizerEmail)
	            guests.push(attendeeEmail);
	    });

	    if (guests.length) {
	        confirmSettings.selectedViewMode = confirmSettings.viewMode.deleteEvent;
	        confirmSettings.notificationUsers.removedGuests = guests;
	        confirmSettings.notificationUsers.updatedGuests = guests;
	    }
	    
	    return confirmSettings.selectedViewMode;
	}

	function _openConfirmPopup()
	{
	    switch (confirmSettings.selectedViewMode) {
	        case confirmSettings.viewMode.deleteEvent:
	            _confirmPopup.find(".title").text(calendar.options.confirmPopup.dialogDeleteEventHeader);
	            _confirmPopup.find(".body").text(calendar.options.confirmPopup.dialogDeleteEventBody);
	            _confirmPopup.find(".send-btn").show();
	            _confirmPopup.find(".send-customs-btn, .send-everyone-btn").hide();
	            break;
	        default:
	            break;
	    }

	    _dialog.popupFrame("close");
	    $(document).unbind("keyup", _checkEscKey);

	    _delSettings.popupFrame("close");
	    $(document).unbind("keyup", _checkEscKeyDelSettings);

	    _confirmPopup.popupFrame("close");

	    _confirmPopup.popupFrame("open", { pageX: "center", pageY: "center" });
	    _confirmPopup.css("position","fixed");
	    uiBlocker.removeClass("fc-modal-transparent");

	    if (_confirmPopup.popupFrame("isVisible")) {
	        uiBlocker.show();
	    } else {
	        uiBlocker.hide();
	    }
	}

	function _closeConfirmPopup() {
	    _confirmPopup.popupFrame("close");
	    uiBlocker.hide();
	}

	function _sendGuestsNotification(mode) {
	    confirmSettings.selectedNotificationMode = mode;
	    _deleteEvent.call(_this, confirmSettings.selectedDeleteMode, _eventObj._start);
	    _close.call(_this, false, true);
	    _closeConfirmPopup.call(_this);
	}

	function _sendNotification(sourceId, uniqueId, type, callback) {
	    var attendeesEmails = [];
	    var method = null;

	    switch (type) {
	        case confirmSettings.notificationType.cancel:
	            switch (confirmSettings.selectedNotificationMode) {
	                case confirmSettings.notificationMode.everybody:
	                case confirmSettings.notificationMode.partially:
	                    attendeesEmails = confirmSettings.notificationUsers.removedGuests;
	                    method = ASC.Mail.Utility.SendCalendarCancel;
	                    break;
	                default:
	                    break;
	            }
	            break;
	        case confirmSettings.notificationType.update:
	            switch (confirmSettings.selectedNotificationMode) {
	                case confirmSettings.notificationMode.everybody:
	                case confirmSettings.notificationMode.partially:
	                    attendeesEmails = confirmSettings.notificationUsers.updatedGuests;
	                    method = ASC.Mail.Utility.SendCalendarUpdate;
	                    break;
	                default:
	                    break;
	            }
	            break;
	        default:
	            break;
	    }

	    if (attendeesEmails.length && method) {
	        ASC.CalendarController.Busy = true;
	        window.LoadingBanner.displayLoading();
	        
	        method.call(this, sourceId, uniqueId, attendeesEmails)
                .done(function() {
                    //toastr.success(calendar.options.confirmPopup.dialogSuccessToastText);
                    console.log(calendar.options.confirmPopup.dialogSuccessToastText, arguments);
                    if (callback)
                        callback();
                })
                .fail(function() {
                    toastr.error(calendar.options.confirmPopup.dialogErrorToastText);
                    console.error(calendar.options.confirmPopup.dialogErrorToastText, arguments);
                })
	            .always(function () {
	                ASC.CalendarController.Busy = false;
	                window.LoadingBanner.hideLoading();
	            });
	    } else {
	        if (callback)
	            callback();
	    }
	}


	function _parseDateTime(inputD, inputT, result) {
		var dateStr = inputD.val();
		var timeStr = inputT.val();
		var dateStrIsEmpty = dateStr.search(/\S/) < 0;
		var timeStrIsEmpty = timeStr.search(/\S/) < 0;
		var date, time, r;
		if (!dateStrIsEmpty || !timeStrIsEmpty) {
			date = fcUtil.validateDateString(dateStr);
			time = fcUtil.validateTimeString(timeStr);
			r = (date === null || !timeStrIsEmpty && time === null) ? 0 : 1;
		} else {
			r = -1;
			date = null;
			time = null;
		}
		result.date = {isEmpty: dateStrIsEmpty, isValid: date !== null, value: date};
		result.time = {isEmpty: timeStrIsEmpty, isValid: time !== null, value: time};
		return r;
	}
	function _validateTodoDateFields(result) {
	    var dlg = {
	        date: _dialog.find(".editor .todo_editor .date"),
	        time: _dialog.find(".editor .todo_editor .time"),
	    };
	    var todoDate = {}, fr = _parseDateTime(dlg.date, dlg.time, todoDate);
	    
	    todoDate.dateTime = fr == 1 ? parseDate(todoDate.date.value + "T23:59:59") : null;

	    var frc, r;
	    
	    if (fr == 1) {
	        r = true;
	        frc = "";
	    } else {
	        r = false;
            frc = todoDate.date.isValid ? "" : dlg.date.val() === "" ? "" : "#cc3300";
	    }
	    dlg.date.css("color", "").css("border-color", frc);
	 
	    if (result != undefined) {
	        result.todoDate = todoDate;
	    }

	    return r;
	}
	var changeTimeFrom = null;
    
	function _validateDateFields(result) {
		var allday = _dialog.find(".editor .all-day input").is(":checked");
		var dlg = {
			from:      _dialog.find(".editor .from-date"),
			from_t:    _dialog.find(".editor .from-time"),
			to:        _dialog.find(".editor .to-date"),
			to_t:      _dialog.find(".editor .to-time")
		};
		var frDate = {}, fr = _parseDateTime(dlg.from, dlg.from_t, frDate);
		var toDate = {}, to = _parseDateTime(dlg.to, dlg.to_t, toDate);
		frDate.dateTime = fr == 1 ? parseDate(frDate.date.value + "T" + frDate.time.value) : null;
		toDate.dateTime = to == 1 ? parseDate(toDate.date.value + "T" + toDate.time.value) : null;

		var frc, frtc, toc, totc, r, delta;
		if (!allday && fr == 1 && to == 1 && (delta = toDate.dateTime - frDate.dateTime > 0) ||
			allday && fr == 1 && to == -1 ||
			allday && fr == 1 && to == 1 && (delta = toDate.dateTime - frDate.dateTime >= 0)) {
			r = true;
			frc = frtc = toc = totc = "";
		} else {
			r = false;
			frc  = frDate.date.isValid ? "" : "#cc3300";
			frtc = frDate.time.isValid ? "" : "#cc3300";
			toc  = toDate.date.isValid && delta ? "" : "#cc3300";
			totc = toDate.time.isValid && delta ? "" : "#cc3300";
		}
		dlg.from.css("color", "").css("border-color", frc);
		dlg.from_t.css("color", "").css("border-color", frtc);
		dlg.to.css("color", "").css("border-color", toc);
		dlg.to_t.css("color", "").css("border-color", totc);

		if (result) {
			if ($(result.currentTarget).hasClass('from-time') || $(result.currentTarget).hasClass('from-date')) {
				var oldTime = new Date(Date.parse(_eventObj._start ? _eventObj._start : _eventObj.start));
				var newTime = new Date(Date.parse(frDate.dateTime));

				if (changeTimeFrom != null) {
					oldTime = changeTimeFrom;
				}

				var diff = newTime.getTime() - oldTime.getTime();
				if (diff != NaN && diff != 0) {

					var date = new Date();
					date.setTime(Date.parse(toDate.dateTime) + diff);

					if (date != 'Invalid Date') {
						toDate.dateTime.setTime(Date.parse(toDate.dateTime) + diff);

						changeTimeFrom = newTime;

						if ($(result.currentTarget).hasClass('from-time')) {
							var time =
								(toDate.dateTime.getHours() < 10 ? '0' + toDate.dateTime.getHours() : toDate.dateTime.getHours())
								+ ":" +
								(toDate.dateTime.getMinutes() < 10 ? '0' + toDate.dateTime.getMinutes() : toDate.dateTime.getMinutes());
							var newDate = date.getFullYear() + '-'
								+ ((date.getMonth() + 1) < 10 ? '0' + (date.getMonth() + 1) : (date.getMonth() + 1)) + '-'
								+ (date.getDate() < 10 ? '0' + date.getDate() : date.getDate());
							toDate.date.value = newDate;
							toDate.time.value = time;

							dlg.to[0].value = newDate;
							dlg.to_t[0].value = time;

						} else if ($(result.currentTarget).hasClass('from-date')) {

							var newDate = date.getFullYear() + '-'
								+ ((date.getMonth() + 1) < 10 ? '0' + (date.getMonth() + 1) : (date.getMonth() + 1)) + '-'
								+ (date.getDate() < 10 ? '0' + date.getDate() : date.getDate());

							toDate.date.value = newDate;
							dlg.to[0].value = newDate;
						}
						_validateDateFields.call(_this);
					}
				}
            }
        }
		

		if (result != undefined) {
			result.fromDate = frDate;
			result.toDate   = toDate;
		}
		
		return r;
	}
	
	function _validateDateFieldsSettings(result) {
		
		var r = true;
		var allday = _dialog.find(".editor .all-day input").is(":checked");
		var dlg = {
			from:              _settings.find(".from-date"),
			from_t:            _settings.find(".from-time"),
			to:                _settings.find(".to-date"),
			to_t:              _settings.find(".to-time"),
			cycles:            _settings.find(".fc-cycle-times")
		};
		
		if (!_settings.find(".fc-end-date").hasClass("hidden")) {
			
			var frDate = {}, fr = _parseDateTime(dlg.from, dlg.from_t, frDate);
			var toDate = {}, to = _parseDateTime(dlg.to, dlg.to_t, toDate);
			frDate.dateTime = fr == 1 ? parseDate(frDate.date.value + "T" + frDate.time.value) : null;
			toDate.dateTime = to == 1 ? parseDate(toDate.date.value + "T" + toDate.time.value) : null;
	
			var frc, frtc, toc, totc, delta;
			if (!allday && fr == 1 && to == 1 && (delta = toDate.dateTime - frDate.dateTime > 0) ||
			    allday && fr == 1 && to == -1 ||
			    allday && fr == 1 && to == 1 && (delta = toDate.dateTime - frDate.dateTime >= 0)) {
				r = true;
				frc = frtc = toc = totc = "";
			} else {
				r = false;
				frc  = frDate.date.isValid ? "" : "#cc3300";
				frtc = frDate.time.isValid ? "" : "#cc3300";
				toc  = toDate.date.isValid && delta ? "" : "#cc3300";
				totc = toDate.time.isValid && delta ? "" : "#cc3300";
			}
			dlg.from.css("color", "").css("border-color", frc);
			dlg.from_t.css("color", "").css("border-color", frtc);
			dlg.to.css("color", "").css("border-color", toc);
			dlg.to_t.css("color", "").css("border-color", totc);
			if (result != undefined) {
				result.fromDate = frDate;
				result.toDate   = toDate;
			}
		}
		
		if (!_settings.find(".fc-repeat-cycles").hasClass("hidden")) {
			var cb = "";
			r = true;
			var isNum = dlg.cycles.val().match('^[0-9]+$');
			if ( (dlg.cycles.find(".hidden").length == 0) && !isNum ) { 
				r = false;
				cb  = !r || (r && (dlg.cycles.val() < 0)) ? "#cc3300" : "";
			}
			dlg.cycles.css("color", "").css("border-color", cb);
		}
		
		return r;
	}

	function _handleAllDayClick() {
		var allday = _dialog.find(".editor .all-day input").is(":checked");
		var dlg = {
				from:  _dialog.find(".editor .from-date"),
				from_t:  _dialog.find(".editor .from-time"),
				to:      _dialog.find(".editor .to-date"),
				to_t:    _dialog.find(".editor .to-time")
		};
		if (allday) {
			//dlg.to.val("");
			dlg.from_t.val("").attr("disabled", "disabled");
			dlg.to_t.val("").attr("disabled", "disabled");
		} else {
			var defaultEndDate = new Date(_eventObj.start.getFullYear(), _eventObj.start.getMonth(), _eventObj.start.getDate(), _eventObj.start.getHours(), _eventObj.start.getMinutes() + 30);
			
			if(!dlg.from.val())
			    dlg.from.val(formatDate(_eventObj.start, calendar.options.eventEditor.dateFormat));

			dlg.from_t
					.val(formatDate(_eventObj.start, calendar.options.eventEditor.timeFormat))
					.removeAttr("disabled");
			dlg.to_t
					.val(_eventObj.end instanceof Date ?
							formatDate(_eventObj.end, calendar.options.eventEditor.timeFormat) : formatDate(defaultEndDate, calendar.options.eventEditor.timeFormat))
					.removeAttr("disabled");
		    
			if(!dlg.to.val())
                dlg.to.val(_eventObj.end instanceof Date ?
					formatDate(_eventObj.end, calendar.options.eventEditor.dateFormat) : formatDate(defaultEndDate, calendar.options.eventEditor.dateFormat));
		}
		_validateDateFields();
	}

	function _prepareRepeatRule() {
		if (repeatRule) {
			repeatRule = undefined;
		}
		
		// get all values		
		var dlg = {
			from:        _settings.find(".from-date"),
			to:          _settings.find(".to-date"),
			interval:    _settings.find(".fc-interval-selector"),
			cycles:      _settings.find(".fc-cycle-times")
		};
		
		repeatRule = ASC.Api.iCal.RecurrenceRule.EveryDay;
		switch (dwm_current) {
			case dwm.day: repeatRule = ASC.Api.iCal.ParseRRuleFromString("FREQ=DAILY"); break;
			case dwm.week: repeatRule = ASC.Api.iCal.ParseRRuleFromString("FREQ=WEEKLY"); break;
			case dwm.month: repeatRule = ASC.Api.iCal.ParseRRuleFromString("FREQ=MONTHLY"); break;
			case dwm.year: repeatRule = ASC.Api.iCal.ParseRRuleFromString("FREQ=YEARLY"); break;
		}
		
		repeatRule.Interval = dlg.interval.find("option:selected").text();
		repeatRule.Count = _settings.find(".fc-repeat-cycles.hidden").length ? repeatRule.Count : dlg.cycles.val();
		
		var dayIndexArr = _getSelectedDays.call(_this);
		for (var i = 0; i < dayIndexArr.length; i++) {
			repeatRule.ByDay.push(_getRepeatDayByIndex.call(this, dayIndexArr[i]));
		}
		
		if ((_settings.find(".fc-month-radio.hidden").length == 0) && (dayRuleObject != undefined)) {
			repeatRule.ByDay.push(dayRuleObject);
		}
		
		if (_settings.find(".fc-end-date.hidden").length == 0) {
			var dates = {};
			if (!_validateDateFieldsSettings(dates)) {return false;}
			repeatRule.Until = dates.toDate.dateTime;
		}
		
		_eventObj.repeatRule = repeatRule;
		
		return (repeatRule != undefined);
	}

	function _doDDX(saveData,toViewDetails) {
		var sources = calendar.getEventSources();
		var sourceIsValid = fcUtil.objectIsValid(_eventObj.source);
		var canChangeAlert = sourceIsValid && (
				_eventObj.source.canAlertModify != undefined && _eventObj.source.canAlertModify ||
				_eventObj.source.canAlertModify == undefined) ||
				!sourceIsValid;

		var dlg = {
			viewer: {
			    title:       _dialog.find(".viewer .title"),
			    location:    _dialog.find(".viewer .location"),
			    attendees:   _dialog.find(".viewer .attendees"),
			    replybuttons:_dialog.find(".viewer .reply-buttons"),
				owner:       _dialog.find(".viewer .owner .name"),
				allday:      _dialog.find(".viewer .all-day"),
				from:        _dialog.find(".viewer .from-date"),
				from_t:      _dialog.find(".viewer .from-time"),
				to:          _dialog.find(".viewer .to-date"),
				to_t:        _dialog.find(".viewer .to-time"),
				repeat:      _dialog.find(".viewer .repeat"),
				alert:       _dialog.find(".viewer .alert"),
				calendar:    _dialog.find(".viewer .calendar .name"),
				calendar_b:  _dialog.find(".viewer .calendar .bullet"),
				description: _dialog.find(".viewer .description .text"),
				status:      _dialog.find(".viewer .status")
			},
			editor: {
			    title:       _dialog.find(".editor .title input"),
			    location:    _dialog.find(".editor .location input"),
			    attendees:   _dialog.find(".editor .attendees input"),
				allday:      _dialog.find(".editor .all-day input"),
				from:        _dialog.find(".editor .from-date"),
				from_t:      _dialog.find(".editor .from-time"),
				to:          _dialog.find(".editor .to-date"),
				to_t:        _dialog.find(".editor .to-time"),
				repeat:      _dialog.find(".editor .fc-view-repeat .fc-selector-link"),
				alert:       _dialog.find(".editor .fc-view-alert .fc-selector-link"),
				calendar:    _dialog.find(".editor .calendar select"),
				calendar_b:  _dialog.find(".editor .calendar .bullet"),
				description: _dialog.find(".editor .description textarea"),
				status:      _dialog.find(".editor .status")
			},
		    todo: {
		        title: _dialog.find(".editor .title input"),
		        date: _dialog.find(".editor .todo_editor .date"),
		        time: _dialog.find(".editor .todo_editor .time"),
		        description: _dialog.find(".editor .todo_editor .description textarea"),
		    },
			infoText:    _dialog.find(".info-text")
		};
		var i;

		if (saveData) {			// ------------- SAVE data -------------

			if (!_canEdit) {return false;}

			if (_editMode == _editModes.event) {

				if (false == fcUtil.validateInput(dlg.editor.title, fcUtil.validateNonemptyString) && !toViewDetails) {
                    return false;
                }
                _eventObj.title = $.trim(dlg.editor.title.val());
                _eventObj.title = _eventObj.title.substr(0,
                    Math.min(calendar.options.eventMaxTitleLength, _eventObj.title.length));

                _eventObj.location = $.trim(dlg.editor.location.val());

                _eventObj.allDay = dlg.editor.allday.is(":checked");

                var dates = {};
                if (!_validateDateFields(dates)) {
                    return false;
                }
                _eventObj.start = dates.fromDate.dateTime;
                _eventObj.end = dates.toDate.dateTime;

                if (canChangeAlert) {
                    //_eventObj.repeat.type = parseInt(dlg.editor.repeat.val(), 10);
                    //_eventObj.alert.type = parseInt(dlg.editor.alert.val(), 10);

                    _eventObj.repeatRule = repeatRule;
                    _eventObj.alert.type = alertType;
                }

                _eventObj.newSourceId = dlg.editor.calendar.val();

                _eventObj.status = $.trim(dlg.editor.status.val());

                _eventObj.attendees = [];

                if (!_eventObj.organizer && ASC.Mail.DefaultAccount) {
                    var organizerObj = new ICAL.Property("organizer");
                    organizerObj.setParameter("cn", ASC.Mail.DefaultAccount.name);
                    organizerObj.setValue("mailto:" + ASC.Mail.DefaultAccount.email);
                    _eventObj.organizer = organizerObj.jCal;
                }

                var src;
                $.each(sources, function() {
                    if (this.objectId == _eventObj.newSourceId) {
                        src = this;
                        return false;
                    }
                    return true;
                });
                if (src) {
                    _eventObj.newTimeZone = $.extend({}, src.timeZone);
                }

                var description = $.trim(dlg.editor.description.val());
                /*if (description.length > 0)*/
                {
                    _eventObj.description = description;
                }

                delete _eventObj.textColor;
                delete _eventObj.backgroundColor;
                delete _eventObj.borderColor;
                calendar.normalizeEvent(_eventObj);
            } else {
                _todoObj = {
                    title: calendar.options.todoList.newTodoTitle,
                    completed: false,
                    start: null
                };
                if (false == fcUtil.validateInput(dlg.todo.title, fcUtil.validateNonemptyString)) {
                    return false;
                }
                _todoObj.title = $.trim(dlg.editor.title.val());
                _todoObj.title = _todoObj.title.substr(0,
                    Math.min(calendar.options.eventMaxTitleLength, _todoObj.title.length));
                
                var dates = {};
                if (!_validateTodoDateFields(dates) && !dates.todoDate.date.isEmpty ) {
                    return false;
                }
                _todoObj.start = dates.todoDate.dateTime;
                
                _todoObj.description = $.trim(dlg.todo.description.val());
                
            }
			
		} else {					// ------------- LOAD data -------------
			
			//tite
		    dlg.editor.title.css("color", "").css("border-color", "").val(_eventObj.title);
			dlg.viewer.title.text(_eventObj.title);

			//infotext
			if (_eventObj.status == 2) {
			    dlg.infoText.show();
			} else {
			    dlg.infoText.hide();
			}

            //location
			dlg.editor.location.val(_eventObj.location || "");
			dlg.viewer.location.text(_eventObj.location || "");

			if (_eventObj.location)
			    dlg.viewer.location.show();
			else {
			    dlg.viewer.location.hide();
			}

		    //attendees
			if(_eventObj.attendees && _eventObj.attendees.length) {
			    var statuses = {
			        needsAction: "NEEDS-ACTION", needsActionCount: 0,
			        accepted: "ACCEPTED", acceptedCount: 0,
			        declined: "DECLINED", declinedCount: 0,
			        tentative: "TENTATIVE", tentativeCount: 0
			    };
			    
			    var showReplyButtons = false;

			    jq(_eventObj.attendees).each(function (index, attendee) {
			        
			        var attendeeEmail = attendee[3].replace(new RegExp("mailto:", "ig"), "");
			        var attendeePartstat = attendee[1].partstat.toUpperCase();

			        if (!showReplyButtons) {
			            jq(ASC.Mail.Accounts).each(function(j, account) {
			                if (account.enabled && attendeeEmail.toLowerCase() == account.email.toLowerCase()) {
			                    showReplyButtons = true;
			                    dlg.viewer.replybuttons.find(".reply-button").removeClass("active");
			                    dlg.viewer.replybuttons.find(".reply-button." + attendeePartstat.toLowerCase()).addClass("active");
			                    return false;
			                }
			                return true;
			            });
			        }

			        switch (attendeePartstat) {
			            case statuses.accepted:
			                statuses.acceptedCount++;
			                break;
			            case statuses.declined:
			                statuses.declinedCount++;
			                break;
			            case statuses.tentative:
			                statuses.tentativeCount++;
			                break;
			            case statuses.needsAction:
			                statuses.needsActionCount++;
			                break;
			            default:
			                break;
			        }
			        
			    });

			    dlg.viewer.attendees.find(".guests-count").text(_eventObj.attendees.length);
			    dlg.viewer.attendees.find(".accepted-count").text(statuses.acceptedCount);
			    dlg.viewer.attendees.find(".declined-count").text(statuses.declinedCount);
			    dlg.viewer.attendees.show();

			    if (_canEdit && showReplyButtons && _eventObj.status != 2 && !_eventObj.source.isSubscription)
			        dlg.viewer.replybuttons.show();
			    else
			        dlg.viewer.replybuttons.hide();

			} else {
			    dlg.viewer.replybuttons.hide();
			    dlg.viewer.attendees.hide();
			}

			var organizerName = _eventObj.organizer && _eventObj.organizer.length > 1 ? _eventObj.organizer[1].cn : "";
			organizerName = organizerName ? organizerName : _eventObj.owner && _eventObj.owner.name ? _eventObj.owner.name : "";

			if (organizerName) {
			    dlg.viewer.owner.html(htmlEscape(organizerName));
				_dialog.find(".viewer .owner").removeClass("hidden");
			} else {
				dlg.viewer.owner.html("");
				_dialog.find(".viewer .owner").addClass("hidden");
			}

            //all day
			if (_eventObj.allDay == true) {
				dlg.editor.allday.prop("checked", true);
				dlg.viewer.allday.addClass("yes");
			} else {
				dlg.editor.allday.prop("checked", false);
				dlg.viewer.allday.removeClass("yes");
			}

            //editor from
			dlg.editor.from.css("color", "").css("border-color", "");
			dlg.editor.from_t.css("color", "").css("border-color", "");

			dlg.todo.date.css("color", "").css("border-color", "");
			dlg.todo.time.css("color", "").css("border-color", "");
			dlg.todo.date.val(formatDate(_eventObj.start, calendar.options.eventEditor.dateFormat));
			dlg.todo.time.val('00:00');
		    
			if (_eventObj.start instanceof Date) {
				dlg.editor.from.val(formatDate(_eventObj.start, calendar.options.eventEditor.dateFormat));
				if (_eventObj.allDay == false) {
					dlg.editor.from_t.val(formatDate(_eventObj.start, calendar.options.eventEditor.timeFormat));
				} else {
					dlg.editor.from_t.val("");
				}
			} else {
				dlg.editor.from.val("");
				dlg.editor.from_t.val("");
			}

            //editor to
			dlg.editor.to.css("color", "").css("border-color", "");
			dlg.editor.to_t.css("color", "").css("border-color", "");
		    
			if (_eventObj.end instanceof Date) {
				dlg.editor.to.val(formatDate(_eventObj.end, calendar.options.eventEditor.dateFormat));
				if (_eventObj.allDay == false) {
					dlg.editor.to_t.val(formatDate(_eventObj.end, calendar.options.eventEditor.timeFormat));
				} else {
					dlg.editor.to_t.val("");
				}
			} else {
			    if (_eventObj.allDay && _eventObj.start instanceof Date) {
			        dlg.editor.to.val(formatDate(_eventObj.start, calendar.options.eventEditor.dateFormat));
			    } else {
			        dlg.editor.to.val("");
			        dlg.editor.to_t.val("");
			    }
			}
		    
            //viewer from
			dlg.viewer.from.text(dlg.editor.from.val());
			
			if( (_eventObj.start instanceof Date && !_eventObj.allDay) || (_eventObj.start instanceof Date && _eventObj.allDay && _eventObj.isSegmEvent) ){
				dlg.viewer.from_t.text(formatDate(_eventObj.start, calendar.options.axisFormat));
			} else {
				dlg.viewer.from_t.text("");
			}
			
		    //viewer to
			dlg.viewer.to.text(dlg.editor.to.val());

			if ((_eventObj.end instanceof Date && !_eventObj.allDay) || (_eventObj.end instanceof Date && _eventObj.allDay && _eventObj.isSegmEvent)) {
				dlg.viewer.to_t.text(formatDate(_eventObj.end, calendar.options.axisFormat));
			} else {
				dlg.viewer.to_t.text("");
			}

			var defaultSource = _getDefaultSource();
			var afretResizeSourceIsValid = fcUtil.objectIsValid(_eventObj.afretResizeSource);
			var calSource = sourceIsValid ? _eventObj.source : afretResizeSourceIsValid ? _eventObj.afretResizeSource : defaultSource;

			dlg.editor.repeat.text(function() {
				if (!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.Never) && 
					!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.EveryDay) &&
					!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.EveryWeek) &&
					!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.EveryMonth) &&
					!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.EveryYear)) {
						_openSettings.call(_this);
						_settings.removeClass("hidden");
						
						_disableRepeatSettings.call(_this);
						_setRepeatBackground.call(_this, 1);
					}
				
				return _getRepeatLabel.call(_this, _eventObj.repeatRule);
			});
			dlg.editor.alert.text(sourceIsValid ? _getAlertLabel.call(_this, _eventObj.alert.type) : _getAlertLabel.call(_this, kAlertDefault));

			if (canChangeAlert) {
				_dialog.find(".viewer .repeat-alert").removeClass("hidden");
				_dialog.find(".editor .repeat-alert").removeClass("hidden");
				
				//_dialog.find(".viewer .fc-dropdown").removeClass("hidden");
				//_dialog.find(".editor .fc-dropdown").removeClass("hidden");
			} else {
				dlg.editor.repeat.text(ASC.Api.iCal.RecurrenceRule.Never);
				dlg.editor.alert.text(kAlertNever);
				_dialog.find(".viewer .repeat-alert").addClass("hidden");
				_dialog.find(".editor .repeat-alert").addClass("hidden");
				
				//_dialog.find(".viewer .fc-dropdown").removeClass("hidden");
				//_dialog.find(".editor .fc-dropdown").removeClass("hidden");
			}
			dlg.viewer.repeat.text(dlg.editor.repeat.text());
			dlg.viewer.alert.text(dlg.editor.alert.text());

			var options = '';
			var calT;
			var calVal;
			var calColor;
			if (_canChangeSource) {
				for (i = 0; i < sources.length; ++i) {
					if ((sources[i].objectId != undefined) &&
							(sources[i].isEditable || !sources[i].isSubscription) && sources[i].isTodo != 1) {
						calT = htmlEscape(sources[i].title);
						options += '<option value="' + htmlEscape(sources[i].objectId) + '" ' +
								'title="' + calT + '">' +
								'&nbsp;&nbsp;&nbsp;&nbsp;' +  // for select elem padding does not work in safari
								calT + '</option>';
					}
				}
				calVal = calSource.objectId;
				calColor = calSource.backgroundColor;
				dlg.editor.calendar.removeAttr("disabled");
			} else {
				calVal = sourceIsValid ?
						_eventObj.source.objectId : -1;
				calColor = sourceIsValid ?
						_eventObj.source.backgroundColor : calendar.options.eventBackgroundColor;
				calT = htmlEscape(sourceIsValid ? _eventObj.source.title : "");
				options = '<option value="' + htmlEscape(calVal) + '" title="' + calT + '">' +
						'&nbsp;&nbsp;&nbsp;&nbsp;' +      // for select elem padding does not work in safari
						calT + '</option>';
				dlg.editor.calendar.attr("disabled", "disabled");
			}

			dlg.editor.calendar.html(options);
			dlg.editor.calendar.val(calVal);
			dlg.viewer.calendar.text(sourceIsValid ?
					_eventObj.source.title :
					dlg.editor.calendar.find("option:selected").text());

			calColor = htmlEscape(calColor);
			dlg.editor.calendar_b.css("background", calColor);
			dlg.viewer.calendar_b.css("background", calColor);

			dlg.editor.status.val(_eventObj.status);
			dlg.viewer.status.text(dlg.editor.status.find("option:selected").text());

			dlg.editor.description.val(_eventObj.description != undefined ? _eventObj.description : "");
			setTimeout(function() {     // jScrollPane must be init for visible element
				dlg.viewer.description.empty();
				dlg.viewer.description.append(
						'<div><div class="inner">' + htmlEscape(dlg.editor.description.val()) + '</div></div>');
				var inner = dlg.viewer.description.find(".inner");
				inner.css("width", inner.width() + "px");
				dlg.viewer.description.children("div").jScrollPane();
				inner.css("width", "");
				
				//hide description field title in Birthday event
				if (_eventObj.sourceId == "users_birthdays") {
					dlg.viewer.description.prev().hide();
				} else {
					dlg.viewer.description.prev().show();
				}
				
				_resizePopup();
			}, 0);

			_renderPermissions.call(_this);
			_handleAllDayClick.call(_this);
			changeTimeFrom = null;
		}
		return true;
	}


	function _getDefaultSource() {
		var sources = calendar.getEventSources();
		var s1;
		for (var i = 0; i < sources.length; ++i) {
			if (fcUtil.objectIsValid(sources[i]) && (
			    fcUtil.objectIsEditable(sources[i]) || !sources[i].isSubscription) && sources[i].isTodo != 1) {
				if (s1 == undefined) {s1 = sources[i];}
				if (!sources[i].isHidden) {return sources[i];}
			}
		}
		return s1;
	}

	function _createEvent(startDate, endDate, allDay) {
		var evt = {
			title:           "",
			description:     "",
			allDay:          allDay,
			start:           startDate,
			end:             endDate,
			repeatRule:      ASC.Api.iCal.RecurrenceRule.Never,
			alert:           {type:kAlertDefault},
			isShared:        false,
			permissions:     {users:[]},
			attendees:       []
		};
	    

		if (ASC.Mail.DefaultAccount) {
		    var organizerObj = new ICAL.Property("organizer");
		    organizerObj.setParameter("cn", ASC.Mail.DefaultAccount.name);
		    organizerObj.setValue("mailto:" + ASC.Mail.DefaultAccount.email);
		    evt.organizer = organizerObj.jCal;
		}

	    return evt;
	}

	function _deleteEvent(deleteType, eventDate) {
		if (!_canDelete ||
		    !fcUtil.objectIsValid(_eventObj) ||
		    !fcUtil.objectIsValid(_eventObj.source)) {return;}

		if (_eventObj.sourceId && _eventObj.uniqueId && (_eventObj.repeatRule.Freq == ASC.Api.iCal.Frequency.Never || confirmSettings.selectedDeleteMode == deleteMode.allSeries))
	        _sendNotification(_eventObj.sourceId, _eventObj.uniqueId, confirmSettings.notificationType.cancel, deleteEvt);
	    else
	        deleteEvt();

	    function deleteEvt() {
	        var id = _eventObj._id;
	        calendar.trigger("editEvent", _this,
	            $.extend(
	                { action: kEventDeleteAction, sourceId: _eventObj.source.objectId, type: deleteType, date: eventDate },
	                _eventObj),
	            function(response) {
	                if (!response.result) {
	                    return;
	                }
	                calendar.removeEvents(id);

	                //
	                if (response.event != undefined) {
	                    if (!response.event.length) {
	                        if (_eventObj.repeatRule.Freq != ASC.Api.iCal.Frequency.Never && confirmSettings.selectedDeleteMode != deleteMode.allSeries) {
	                            _sendNotification(_eventObj.sourceId, _eventObj.uniqueId, confirmSettings.notificationType.cancel, function() {
	                                ASC.CalendarController.RemoveEvent(id, deleteMode.allSeries, eventDate);
	                            });
	                        }
	                        return;
	                    }
	                    //
	                    var sources = calendar.getEventSources();
	                    var j = 0;
	                    while (j < response.event.length) {
	                        _setEventSource(response.event[j], sources);
	                        j++;
	                    }
	                    calendar.addEvents(response.event);

	                    if(_eventObj.repeatRule.Freq != ASC.Api.iCal.Frequency.Never && confirmSettings.selectedDeleteMode != deleteMode.allSeries)
	                        _sendNotification(_eventObj.sourceId, _eventObj.uniqueId, confirmSettings.notificationType.update);
	                }
	            });
	    }
	}

	function _unsubscribeEvent() {
		if (!_canUnsubscribe ||
		    !fcUtil.objectIsValid(_eventObj) ||
		    !fcUtil.objectIsValid(_eventObj.source)) {return;}
		//
		var id = _eventObj._id;
		calendar.trigger("editEvent", _this,
				$.extend(
						{action: kEventUnsubscribeAction, sourceId: _eventObj.source.objectId},
						_eventObj),
				function(response) {
					if (!response.result) {return;}
					calendar.removeEvents(id);
				    _close.call(_this, false);
				});
	}

	function _setEventSource(event, sources) {
		if (event.sourceId == undefined) {
			event.sourceId = event.newSourceId;
		}
		if (!event.source && event.sourceId != undefined) {
			for (var i = 0; i < sources.length; ++i) {
				if (sources[i].objectId != undefined &&
						sources[i].objectId == event.sourceId) {
					event.source = sources[i];
					break;
				}
			}
		}
	}
	function _createTodo() {
	    calendar.todolist.createTodo(_todoObj);
	    _dialog.popupFrame("close");
	    uiBlocker.hide();
	}
	function _updateEvent(isCancel) {
		if (!_canEdit) {return;}
		
		if(isCancel)
		{
			calendar.trigger("editEvent", _this, $.extend( {action: kEventCancelAction, sourceId: _eventObj.source.objectId}, _eventObj), function(response){} );
			return;
		}

		if (_eventObj.source) {
		    _disableDialogBtns(true);
		    // update event
		    calendar.trigger("editEvent", _this,
					$.extend(
							{action: kEventChangeAction, sourceId: _eventObj.source.objectId},
							_eventObj),
					function(response) {
					    _disableDialogBtns(false);
					    if (!response.result) {return;}
						_closeDialog.call(_this);
						if (response.event.length < 1) {return;}
						//
						var sources = calendar.getEventSources();
						if (response.event[0])
							calendar.removeEvents(response.event[0].objectId);
						for (var j = 0; j < response.event.length; ++j) {
							_setEventSource(response.event[j], sources);
						}
						calendar.addEvents(response.event);
					});
		} else {
		    _disableDialogBtns(true);
		    var id = _eventObj._id;
			// create new event
			calendar.trigger("editEvent", _this,
					$.extend({action: kEventAddAction}, _eventObj),
					function(response) {
					    _disableDialogBtns(false);
					    if (!response.result) {return;}
						_closeDialog.call(_this);
						calendar.removeEvents(id);
						if (response.event.length < 1) {return;}
						//
						var sources = calendar.getEventSources();
						if (response.event[0])
							calendar.removeEvents(response.event[0].objectId);
						for (var j = 0; j < response.event.length; ++j) {
							_setEventSource(response.event[j], sources);
						}
						calendar.addEvents(response.event);
					});
		}
	}

    function _eventReply(decision) {

        if (!_eventObj.attendees || !_eventObj.attendees.length) return;

        var partstat = typeof decision === "boolean" ? decision ? "ACCEPTED" : "DECLINED" : "TENTATIVE";

        var replyEmail = null;

        jq(_eventObj.attendees).each(function(i, attendee) {

            var attendeeEmail = attendee[3].replace(new RegExp("mailto:", "ig"), "");
            var attendeePartstat = attendee[1].partstat.toUpperCase();

            jq(ASC.Mail.Accounts).each(function(j, account) {
                if (account.enabled && attendeeEmail.toLowerCase() == account.email.toLowerCase() && attendeePartstat != partstat) {
                    replyEmail = attendeeEmail;
                    return false;
                }
                return true;
            });

            return replyEmail == null;
        });

        if (_eventObj.sourceId && _eventObj.uniqueId && replyEmail) {
            _doReply(_eventObj.sourceId, _eventObj.uniqueId, replyEmail, partstat);
        }
    }

    function _doReply(calendarId, eventUid, attendeeEmail, partstat) {
        _dialog.find(".viewer .reply-button").removeClass("active");

        ASC.CalendarController.Busy = true;
        window.LoadingBanner.displayLoading();

        ASC.Mail.Utility.SendCalendarReply(calendarId, eventUid, attendeeEmail, partstat)
            .done(function () {
                _doReplyCallback(attendeeEmail, partstat);
            })
            .fail(function () {
                toastr.error(calendar.options.confirmPopup.dialogErrorToastText);
                console.error(calendar.options.confirmPopup.dialogErrorToastText, arguments);
            })
            .always(function () {
                ASC.CalendarController.Busy = false;
                window.LoadingBanner.hideLoading();
            });
    }
    
    function _doReplyCallback(attendeeEmail, partstat) {
        _dialog.find(".viewer .reply-buttons ." + partstat.toLowerCase()).addClass("active");
        
        jq(_eventObj.attendees).each(function (index, item) {
            var itemEmail = item[3].replace(new RegExp("mailto:", "ig"), "");
            if (itemEmail.toLowerCase() == attendeeEmail.toLowerCase()) {
                item[1].partstat = partstat;
                item[1].rsvp = "FALSE";
                return false;
            }
            return true;
        });
        
        var statuses = {
            n: "NEEDS-ACTION", nc: 0,
            a: "ACCEPTED", ac: 0,
            d: "DECLINED", dc: 0,
            t: "TENTATIVE", tc: 0
        };

        jq(_eventObj.attendees).each(function (index, attendee) {
			        
            var attendeePartstat = attendee[1].partstat.toUpperCase();
			        
            switch (attendeePartstat) {
                case statuses.a:
                    statuses.ac++;
                    break;
                case statuses.d:
                    statuses.dc++;
                    break;
                case statuses.t:
                    statuses.tc++;
                    break;
                case statuses.n:
                    statuses.nc++;
                    break;
                default:
                    break;
            }
			        
        });

        _dialog.find(".attendees .guests-count").text(_eventObj.attendees.length);
        _dialog.find(".attendees .accepted-count").text(statuses.ac);
        _dialog.find(".attendees .declined-count").text(statuses.dc);

    }

    // Public interface

    this.openEvent = function (elem, event) {
        calendar.openEventWin = { objectId: event.objectId, isTodo: event.isTodo };
	    if (ASC.CalendarController.Busy) {
	        console.log("ASC.CalendarController.Busy");
	        return false;
	    }

	    ASC.CalendarController.CancelEditDialog();

		if (event && !isNaN(event.objectId) && event.uniqueId && !event.isTodo) {
	        Teamlab.getCalendarEventById({},
                event.objectId,
                {
                    success: function(p, eventInfo) {
                        if (eventInfo.eventUid === event.uniqueId && eventInfo.mergedIcs) {
                            console.info(eventInfo);

                            var evt = parseIcs(eventInfo.mergedIcs);

                            event = jq.extend(event, evt || {});

                            _open.call(_this, "view", elem, event);
                        }
                    },
                    error: function(p, e) {
                        console.error(e);
                    },
                    max_request_attempts: 1
                });
		} if (event.isTodo) {
			calendar.todolist.openTodoEditor(event, elem);
		} else {
	        _open.call(_this, "view", elem, event);
	    }
	};

	this.addEvent = function(startDate, endDate, allDay , isAfterResize) {
	    if (ASC.CalendarController.Busy) {
	        console.log("ASC.CalendarController.Busy");
	        return false;
	    }

	    calendar.openEventWin = { objectId: null, isTodo: false };
	    
	    ASC.CalendarController.CancelEditDialog();

	    // Protect from addind event in nonexistent category
		var defaultSource = _getDefaultSource();
		if (defaultSource == undefined) return false;

		_dialog.find(".buttonGroup span.active").removeClass('active');
		_dialog.find(".buttonGroup span.event").addClass('active');
	    _editMode = _editModes.event;

		var ev;
		if (startDate != undefined) {
		    ev = _createEvent(startDate, endDate, allDay);
		    if (isAfterResize && _eventObj) {
		        ev.alert = _eventObj.alert;
		        ev.allDay = _eventObj.allDay;
		        ev.location = _eventObj.location;
		        ev.start = _eventObj.start;
		        ev.end = _eventObj.end;
		        ev.title = _eventObj.title; 
		        
		        var s = calendar.getEventSources();
		        for (var i = 0; i < s.length; ++i) {
		            if (_eventObj.newSourceId != undefined && _eventObj.newSourceId == s[i].objectId) {
		                ev.afretResizeSource = s[i];
		            }
		        }

		        ev.textColor = defaultSource.textColor;
		        ev.backgroundColor = defaultSource.backgroundColor;
		        ev.borderColor = defaultSource.borderColor;
		    } else {
		        // add event via clicking calendar cell
		        
		        ev.textColor = defaultSource.textColor;
		        ev.backgroundColor = defaultSource.backgroundColor;
		        ev.borderColor = defaultSource.borderColor;
		    }
			
		    calendar.renderEvent(ev);
			_open.call(_this, "addPopup", calendar.getView().getEventElement(ev), ev);
		} else {
			// add event via header menu
			ev = _createEvent(
					new Date(),
					addMinutes(new Date(), calendar.options.defaultEventMinutes),
					false);
			_open.call(_this, "addDialog", undefined, ev);
		}
	};

	this.isVisible = function() {
		return _dialog.popupFrame("isVisible");
	};
    
	this.closePopupEvent = function (isResize) {
	    if (isResize) {
	        _doDDX.call(_this, true);
	        calendar.openEventWin = { objectId: null, isTodo: false};
	    }
	    if (this.isVisible() && !_dialogMode) {
	        _close.call(_this, false);
	    }
	};

    this.getEventObj = function() {
        return _eventObj;
    };
    
}


function EventPage(calendar) {
    var _this = this;
	
    var rs = calendar.options.repeatSettings;
    var ds = calendar.options.deleteSettings;
    var cp = calendar.options.confirmPopup;

    var dwm = {
        day:     0,
        week:    1,
        month:   2,
        year:    3
    };

    var deleteMode = {
        single: 0,
        allFollowing: 1,
        allSeries: 2
    };

    var confirmSettings = {
        viewMode: {
            none: 0,
            addEvent: 1,
            updateEvent: 2,
            updateGuests: 3,
            deleteEvent: 4
        },
        notificationMode: {
            nobody: 0,
            partially: 1,
            everybody: 2
        },
        notificationType: {
            request: 1,
            update: 2,
            cancel: 4
        },
        notificationUsers: {
            newGuests: [],
            removedGuests: [],
            updatedGuests: []
        },
        selectedViewMode: 0,
        selectedNotificationMode: 0,
        selectedDeleteMode: 0,
    };

    var replyDecisionSettings = {
        sendReply: true,
        email: null,
        decision: null
    };
	
    var _dialog;
    var _settings;
    var _delSettings;
    var _confirmPopup;
    var uiBlocker;
    var alertType = kAlertDefault;
    var repeatRule = ASC.Api.iCal.RecurrenceRule.EveryDay;
    var dwm_current = dwm.day;
    var dayRuleObject = undefined;
	

    var _eventObj;
    var _canEdit;
    var _canDelete;
    var _canChangeSource;
    var _canUnsubscribe;

    var formatDate = calendar.formatDate;
    var formatDates = calendar.formatDates;


    (function _createPage() {
        _dialog = jq("#asc_event").empty();
        
        jq("#eventPageTemplate").tmpl({ maxlength: defaults.eventMaxTitleLength }).appendTo(_dialog);

        var selector = jq("#asc_event .editor .owner select");
        jq.each(ASC.Mail.Accounts, function (index, item) {
            var name = item.name ? Encoder.htmlEncode(item.name) : Teamlab.profile.displayName;
            var option = jq("<option/>").attr("value", item.email).attr("data-name", Encoder.htmlDecode(name)).html("{0} &lt;{1}&gt;".format(name, item.email));
            selector.append(option);
        });

        selector.tlcombobox({ align: "left" });

        uiBlocker = _dialog.find(".fc-modal");

        _dialog.find(".buttons .save-btn").click(function() {
            if (jq(this).hasClass("disable"))
                return;
		    
            var hasSettings = !_settings.hasClass("hidden");
            if (hasSettings) {
                if (!_validateDateFieldsSettings.call(_this)) {
                    return;
                }
                _closeSettings.call(_this, hasSettings);
            }
            var oldEventObj = Object.assign({}, _eventObj),
                compareResult = {},
                sentInvitations = _dialog.find(".editor .sent-invitations input").is(":checked"),
                confirmViewMode = _getConfirmViewMode.call(_this);
            
            if (sentInvitations) {
                if (oldEventObj.sourceId != undefined) { //it's not new event
                    
                    _doDDX.call(_this, true);
                    var newEventObj = Object.assign({}, _eventObj);
                    _eventObj = Object.assign({}, oldEventObj);

                    compareResult = deepEventsCompare(oldEventObj, newEventObj);

                    if (!compareResult.isCompare) {
                        if (compareResult.differentsList.length == 1 && compareResult.differentsList.indexOf("attendees") != -1) {
                            if (confirmViewMode) {
                                _openConfirmPopup.call(_this);
                            } else {
                                _close.call(_this, true);
                            }
                        } else if (compareResult.differentsList.length >= 1) {
                            if (confirmViewMode) {
                                _sendGuestsNotification.call(_this, confirmSettings.notificationMode.everybody);
                            } else {
                                _close.call(_this, true);
                            }
                        }
                    } else {
                        _close.call(_this, true);
                    }
                } else {
                    if (confirmViewMode) {
                        _sendGuestsNotification.call(_this, confirmSettings.notificationMode.everybody);
                    } else {
                        _close.call(_this, true);
                    }
                }
            } else {
                _close.call(_this, true);
            }
        });
       
        function deepEventsCompare () {
            var i, l, leftChain, rightChain;

            var result = {
                isCompare: true,
                differentsList: []
            };

            function compare2Objects (x, y, notPush) {
                var p;

                if (isNaN(x) && isNaN(y) && typeof x === 'number' && typeof y === 'number') {
                    return result;
                }

                if (x === y) {
                    return result;
                }

                if ((typeof x === 'function' && typeof y === 'function') ||
                   (x instanceof Date && y instanceof Date) ||
                   (x instanceof RegExp && y instanceof RegExp) ||
                   (x instanceof String && y instanceof String) ||
                   (x instanceof Number && y instanceof Number)) {
                    return x.toString() === y.toString();
                }

                if (!(x instanceof Object && y instanceof Object)) {
                    result.isCompare = false;
                    return result;
                }

                if (x.isPrototypeOf(y) || y.isPrototypeOf(x)) {
                    result.isCompare = false;
                    return result;
                }

                if (x.constructor !== y.constructor) {
                    result.isCompare = false;
                    return result;
                }

                if (x.prototype !== y.prototype) {
                    result.isCompare = false;
                    return result;
                }

                if (leftChain.indexOf(x) > -1 || rightChain.indexOf(y) > -1) {
                    result.isCompare = false;
                    return result;
                }
                

                for (p in y) {
                    if (y.hasOwnProperty(p) !== x.hasOwnProperty(p) && (p != "newSourceId" && p != "newTimeZone")) {
                        result.isCompare = false;
                        if(!notPush) result.differentsList.push(p);
                    }
                    else if (typeof y[p] !== typeof x[p] && (p != "newSourceId" && p != "newTimeZone")) {
                        result.isCompare = false;
                        if(!notPush) result.differentsList.push(p);
                    }
                }

                for (p in x) {
                    if (y.hasOwnProperty(p) !== x.hasOwnProperty(p)) {
                        result.isCompare = false;
                        if(!notPush) result.differentsList.push(p);
                    }
                    else if (typeof y[p] !== typeof x[p]) {
                        result.isCompare = false;
                        if(!notPush) result.differentsList.push(p);
                    }
                    if (x[p] == null && y[p] != null) {
                        result.differentsList.push(p);
                    } else {
                        switch (typeof (x[p])) {
                            case 'object':
                            case 'function':

                                leftChain.push(x);
                                rightChain.push(y);

                                if (p == "attendees") {
                                    var t = Object.assign({}, result);
                                    result = { isCompare: true, differentsList: [] };
                                    var tmpResult = compare2Objects(x[p], y[p], true);
                                
                                    if (!tmpResult.isCompare) {
                                        result.isCompare = false;
                                        result.differentsList.push(p);
                                    }
                                    result.isCompare = !result.isCompare ? result.isCompare : t.isCompare;
                                    result.differentsList = result.differentsList.concat(t.differentsList);
                                    tmpResult = t = null;

                                }else if (!compare2Objects (x[p], y[p])) {
                                    result.isCompare = false;
                                    if(!notPush) result.differentsList.push(p);
                                }

                                leftChain.pop();
                                rightChain.pop();
                                break;

                            default:
                                if (p == "sourceId") {
                                    if(x[p] !== y["sourceId"] || x[p] !== y["newSourceId"]){
                                        result.isCompare = false;
                                        if(!notPush) result.differentsList.push(p);
                                    }
                                }else if (x[p] !== y[p]) {
                                    result.isCompare = false;
                                    if(!notPush) result.differentsList.push(p);
                                }
                                break;
                        }
                    }
                    
                }
               
                return result;
            }

            if (arguments.length < 1) {
                return result; 
            }

            for (i = 1, l = arguments.length; i < l; i++) {

                leftChain = [];
                rightChain = [];

                compare2Objects(arguments[0], arguments[i]);

            }

            return result;
        }
       
        _dialog.find(".event-header .header-back-link, .buttons .cancel-btn, .buttons .close-btn").click(function() {
            _close.call(_this, false);
        });
        
        _dialog.find(".buttons .delete-btn").click(function() {
            if (_canDelete) {
                _openDelSettings.call(_this, _eventObj.repeatRule.Freq == ASC.Api.iCal.Frequency.Never);
            }
        });
        
        _dialog.find(".buttons .unsubs-btn").click(function() {
            if (_canUnsubscribe) {
                _unsubscribeEvent.call(_this);
            }
        });

        _renderRepeatAlertList.call(_this);
		
        _dialog.find(".editor .title input").keyup(function() {
            fcUtil.validateInput(this, fcUtil.validateNonemptyString);
        });
        
        _dialog.find(".editor .attendees input.textEdit").AttendeesSelector("init", {
            isInPopup: false,
            items: [],
            container: _dialog.find(".editor .attendees .attendees-user-list"),
            organizer: _dialog.find(".editor .owner select"),
        });

        _dialog.find(".editor .addUserLink").ShareUsersSelector("init", {
            permissions: { data: { actions: [], items: [] }, users: []},
            container:  _dialog.find(".editor .shared-user-list")
        });

        _dialog.find(".editor .owner select").on("change", function() {
            var email = jq(this).val();
            var attendeeSelector = _dialog.find(".editor .attendees input.textEdit");
            var attendees = attendeeSelector.AttendeesSelector("get");
            var redraw = false;

            jq.each(attendees, function(index, item) {
                if(Boolean(item[1]["x-organizer"])){
                    item[3] = "mailto:" + email;
                    redraw = true;
                }
            });

            if (redraw) attendeeSelector.AttendeesSelector("set", attendees);
        });

        _dialog.find(".editor .all-day input").click(_handleAllDayClick);
		
        _dialog.find(".editor .all-day .label").css("cursor", "pointer").click(function() {
            _dialog.find(".editor .all-day .cb").trigger("click");
            _handleAllDayClick.call(_this);
        });

        function loadDate(input, dp, defDate) {
            var d = parseISO8601(input.val());
            dp.datepicker("setDate", (d instanceof Date) ? d : defDate);
        }
        
        function saveDate(input, inputT, dp) {
            input.val(formatDate(
					dp.datepicker("getDate"),
					calendar.options.eventEditor.dateFormat));
            if (!inputT.is(":disabled") && $.trim(inputT.val()).length < 1) {
                inputT.val("00:00");
            }
            input.change();
        }
        
        var fromD = _dialog.find(".editor .from-to .from-date");
        var fromT = _dialog.find(".editor .from-to .from-time");
		
        var toD   = _dialog.find(".editor .from-to .to-date");
        var toT   = _dialog.find(".editor .from-to .to-time");
		
        _dialog.find(".editor .from-to .from.cal-icon").click(function() {
            fcDatepicker.open(calendar, this,
					function(dp) {loadDate(fromD, dp, _eventObj.start);},
					function(elem, dp) {
					    this.close();
					    saveDate(fromD, fromT, dp);
						
					    var fromD_settings = _settings.find(".from-date");
					    var fromT_settings = _settings.find(".from-time");
					    saveDate(fromD_settings, fromT_settings, dp);
					});
        });
        
        _dialog.find(".editor .from-to .to.cal-icon").click(function() {
            fcDatepicker.open(calendar, this,
					function(dp) {loadDate(toD, dp, _eventObj.end||_eventObj.start);},
					function(elem, dp) {
					    this.close();
					    saveDate(toD, toT, dp);
					});
        });

        _dialog.find(".editor .from-to .from-time, .editor .from-to .to-time").mask("00:00");
        
        _dialog.find(".editor .from-to .from-date, .editor .from-to .from-time, .editor .from-to .to-date, .editor .from-to .to-time")
            .bind("keyup change", _validateDateFields);

        _dialog.find(".editor .calendar select").change(function (ev) {
            var v = $(this).val();
            var s = calendar.getEventSources();
            for (var i = 0; i < s.length; ++i) {
                if (s[i].objectId != v || s[i].isTodo == 1) { continue; }
                _dialog.find(".editor .calendar .bullet").css("background", s[i].backgroundColor);
                
                if (s[i].isSubscription) {
                    _dialog.find(".editor .attendees").hide();
                } else {
                    if (!_dialog.find(".editor .attendees .input-container").hasClass("display-none") ||
                        !_dialog.find(".editor .attendees .attendees-user-list").hasClass("display-none") ||
                        !_dialog.find(".editor .attendees .attendees-noaccount").hasClass("display-none")) {
                        _dialog.find(".editor .attendees").show();
                    } else {
                        _dialog.find(".editor .attendees").hide();
                    }
                }

                return;
            }
        });
        
        jq("#attendeesHelpSwitcher").on("click", function () {
            jq(this).helper({ BlockHelperID: 'attendeesHelpInfo' });
        });
        
        if (window.onbeforeunload == null) {
                window.onbeforeunload = function () {
                    if(jq("#asc_event .event-editor .editor").is(":visible")) {
                        return ASC.Resources.Master.Resource.WarningMessageBeforeUnload;
                    }
                };
            }
    }());
	
    (function _createSettings() {
        _settings = _dialog.find(".repeat-settings");

        // day/week/month selector
        var _DWMSelectorLabel = _settings.find(".fc-dwm-selector");
		
        _settings.find(".fc-dwm-selector").click(
			function (event) {
			    
			    if ($(this).find(".fc-selector-link").hasClass("not-active")) {
			        return;
			    }
				
			    fcMenus.hideMenus(fcMenus.modeMenuDWMEventPage);
			    fcMenus.modeMenuDWMEventPage.popupMenu("open", _DWMSelectorLabel);
			    event.stopPropagation();
			});
			
        if (!fcMenus.modeMenuDWMEventPage || fcMenus.modeMenuDWMEventPage.length < 1) {
            fcMenus.modeMenuDWMEventPage = $('<div id="fc_mode_menu_dwm_eventpage"/>');
        } else {
            fcMenus.modeMenuDWMEventPage.popupMenu("close");
            fcMenus.modeMenuDWMEventPage.popupMenu("destroy");
        }
        
        fcMenus.modeMenuDWMEventPage.popupMenu({
            anchor: "left,bottom",
            direction: "right,down",
            arrow: "up",
            showArrow: false,
            closeTimeout: -1,
            cssClassName: "asc-popupmenu",
            items: [
                {
                    label: rs.dialogRepeatOn_days,
                    click: function() {
                        _DWMSelectorLabel.find(".fc-selector-link").text(rs.dialogRepeatOn_days);
                        _settings.find(".fc-interval-label").text(rs.dialogIntervalOption_day);
                        dwm_current = dwm.day;
                        _showDaySections.call(_this);
                        _settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
                    }
                },
                {
                    label: calendar.options.repeatSettings.dialogRepeatOn_weeks,
                    click: function() {
                        _DWMSelectorLabel.find(".fc-selector-link").text(rs.dialogRepeatOn_weeks);
                        _settings.find(".fc-interval-label").text(rs.dialogIntervalOption_week);
                        dwm_current = dwm.week;
                        _showWeekSections.call(_this);
                        _settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
                    }
                },
                {
                    label: calendar.options.repeatSettings.dialogRepeatOn_months,
                    click: function() {
                        _DWMSelectorLabel.find(".fc-selector-link").text(rs.dialogRepeatOn_months);
                        _settings.find(".fc-interval-label").text(rs.dialogIntervalOption_month);
                        dwm_current = dwm.month;
                        _showMonthSections.call(_this);
                        _settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
                    }
                },
                {
                    label: calendar.options.repeatSettings.dialogRepeatOn_years,
                    click: function() {
                        _DWMSelectorLabel.find(".fc-selector-link").text(rs.dialogRepeatOn_years);
                        _settings.find(".fc-interval-label").text(rs.dialogIntervalOption_year);
                        dwm_current = dwm.year;
                        _showDaySections.call(_this);
                        _settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
                    }
                }
            ]
        });
			
        // end of repeat selector
        var _endRepeatSelectorLabel = _settings.find(".fc-endrepeat-selector");
		
        _settings.find(".fc-endrepeat-selector").click(
			function (event) {
				
			    if ($(this).find(".fc-selector-link").hasClass("not-active")) {
			        return false;
			    }
				
			    fcMenus.hideMenus(fcMenus.modeMenuEndRepeatEventPage);
			    fcMenus.modeMenuEndRepeatEventPage.popupMenu("open", _endRepeatSelectorLabel);
			    event.stopPropagation();
			});
			
        if (!fcMenus.modeMenuEndRepeatEventPage || fcMenus.modeMenuEndRepeatEventPage.length < 1) {
            fcMenus.modeMenuEndRepeatEventPage = $('<div id="fc_mode_menu_end_reapeat_eventpage"/>');
        } else {
            fcMenus.modeMenuEndRepeatEventPage.popupMenu("close");
            fcMenus.modeMenuEndRepeatEventPage.popupMenu("destroy");
        }
        fcMenus.modeMenuEndRepeatEventPage.popupMenu({
            anchor: "left,bottom",
            direction: "right,down",
            arrow: "up",
            showArrow: false,
            closeTimeout: -1,
            cssClassName: "asc-popupmenu",
            items: [
                {
                    label: calendar.options.repeatSettings.dialogOptionNever,
                    click: function() {
                        _endRepeatSelectorLabel.find(".fc-selector-link").text(rs.dialogOptionNever);
                        _showEndSectionsNever.call(_this);
                    }
                },
                {
                    label: calendar.options.repeatSettings.dialogOptionCount,
                    click: function() {
                        _endRepeatSelectorLabel.find(".fc-selector-link").text(rs.dialogOptionCount);
                        _settings.find(".fc-cycle-times").val(3);
                        _showEndSectionsCycles.call(_this);
                    }
                },
                {
                    label: calendar.options.repeatSettings.dialogOptionDate,
                    click: function() {
                        _endRepeatSelectorLabel.find(".fc-selector-link").text(rs.dialogOptionDate);
                        _settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
                        _showEndSectionsDate.call(_this);
                    }
                }
            ]
        });
		
        //
        function loadDate(input, dp, defDate) {
            var d = parseISO8601(input.val());
            dp.datepicker("setDate", (d instanceof Date) ? d : defDate);
        }
        function saveDate(input, inputT, dp) {
            input.val(formatDate(
					dp.datepicker("getDate"),
					rs.dateFormat));
            if (!inputT.is(":disabled") && $.trim(inputT.val()).length < 1) {
                inputT.val("00:00");
            }
            input.change();
        }
		
        var fromD = _settings.find(".from-date");
        var fromT = _settings.find(".from-time");
        var toD = _settings.find(".to-date");
        var toT = _settings.find(".to-time");
		
        _settings.find(".from.cal-icon").click(function() {
			
            if (_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").hasClass("not-active")) {
                return false;
            }
			
            fcDatepicker.open(calendar, this,
					function(dp) {loadDate(fromD, dp, _eventObj.start);},
					function(elem, dp) {
					    this.close();
					    saveDate(fromD, fromT, dp);
						
					    var fromD_parent = _dialog.find(".from-date");
					    var fromT_parent = _dialog.find(".from-time");
					    saveDate(fromD_parent, fromT_parent, dp);
					});
        });
		
        _settings.find(".to.cal-icon").click(function() {
			
            if (_settings.find(".fc-endrepeat-selector").find(".fc-selector-link").hasClass("not-active")) {
                return false;
            }
			
            fcDatepicker.open(calendar, this,
					function(dp) {loadDate(toD, dp, _eventObj.end||_eventObj.start);},
					function(elem, dp) {this.close();saveDate(toD, toT, dp);});
        });
		
        //
        _settings
				.find(".from-date, .to-date, .fc-cycle-times")
				.bind("keyup change", _validateDateFieldsSettings);
        //
        _settings.find(".calendar select").change(function (ev) {
            var v = $(this).val();
            var s = calendar.getEventSources();
            for (var i = 0; i < s.length; ++i) {
                if (s[i].objectId != v || s[i].isTodo == 1) { continue; }
                _settings.find(".calendar .bullet").css("color", s[i].backgroundColor);
                return;
            }
        });
		
    }());
	
    (function _createDelSettings() {
        _delSettings = $(ds.dialogTemplate)
				.addClass("asc-dialog")
				.popupFrame({
				    anchor: "right,top",
				    direction: "right,down",
				    offset: "0,0",
				    showArrow: false
				});
				
        function setCheckedAttrValue(element, checked) {
            if ((element != undefined) && (element.length != 0)) {
                element.prop("checked", checked);
            }
        }
		
        _delSettings.find(".delete-selector .delete-this-label").click(function() {
            setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-this"), true);
        });
		
        _delSettings.find(".delete-selector .delete-following-label").click(function() {
            setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-following"), true);
        });
		
        _delSettings.find(".delete-selector .delete-all-label").click(function() {
            setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-all"), true);
        });
		
        _delSettings.find(".buttons .save-btn").click(function() {
            if (jq(this).hasClass("disable"))
                return;
		    
            var delType = deleteMode.single;

            if (!_delSettings.hasClass("single")) {
                if (_delSettings.find(".delete-following").is(":checked")) {
                    delType = deleteMode.allFollowing;
                } else if (_delSettings.find(".delete-all").is(":checked")) {
                    delType = deleteMode.allSeries;
                }
            }
            
            _closeDelSettings.call(_this, true);

            if (_getConfirmViewMode.call(_this, true)) {
                confirmSettings.selectedDeleteMode = delType;
                _openConfirmPopup.call(_this);
            } else {
                _deleteEvent.call(_this, delType, _eventObj._start);
                _close.call(_this, false, true);
            }
        });
        
        _delSettings.find(".buttons .cancel-btn, .buttons .close-btn, .header .close-btn").click(function() {
            _closeDelSettings.call(_this, false);
        });

    }());

    (function _createConfirmPopup() {
        _confirmPopup = $(cp.dialogTemplate)
				.addClass("asc-dialog")
				.popupFrame({
				    anchor: "right,top",
				    direction: "right,down",
				    offset: "0,0",
				    showArrow: false
				});

        _confirmPopup.find(".buttons .send-btn, .buttons .send-everyone-btn").click(function() {
            _sendGuestsNotification.call(_this, confirmSettings.notificationMode.everybody);
        });

        _confirmPopup.find(".buttons .send-customs-btn").click(function() {
            _sendGuestsNotification.call(_this, confirmSettings.notificationMode.partially);
        });

        _confirmPopup.find(".buttons .dont-send-btn").click(function() {
            _sendGuestsNotification.call(_this, confirmSettings.notificationMode.nobody);
        });
        
        _confirmPopup.find(".header .close-btn").click(function() {
            _closeConfirmPopup.call(_this, false);
        });

        
    }());


    function _showDaySections(){
        _settings.find(".fc-days-week").addClass("hidden").parent().addClass("hidden");
        _settings.find(".fc-month-radio").addClass("hidden").parent().addClass("hidden");
    }
		
    function _showWeekSections(){
        _settings.find(".fc-month-radio").addClass("hidden").parent().addClass("hidden");
        _settings.find(".fc-days-week").removeClass("hidden").parent().removeClass("hidden");
    }
		
    function _showMonthSections(){
        _settings.find(".fc-days-week").addClass("hidden").parent().addClass("hidden");
        _settings.find(".fc-month-radio").removeClass("hidden").parent().removeClass("hidden");
    }
	
    function _showEndSectionsNever(){
        _settings.find(".fc-repeat-cycles").addClass("hidden");
        _settings.find(".fc-end-date").addClass("hidden");
    }
	
    function _showEndSectionsCycles(){
        _settings.find(".fc-end-date").addClass("hidden");
        _settings.find(".fc-repeat-cycles").removeClass("hidden");
        _validateDateFieldsSettings.call(_this);
    }
	
    function _showEndSectionsDate(){
        _settings.find(".fc-repeat-cycles").addClass("hidden");
        _settings.find(".fc-end-date").removeClass("hidden");
        _validateDateFieldsSettings.call(_this);
    }
	
    function _getEndDate(mode) {
        var startDate = parseISO8601(_settings.find(".from-date").val());
        var interval = parseInt(_settings.find(".fc-interval-selector").find("option:selected").text(), 10);
		
        switch (mode) {
            case dwm.day: 
                return calendar.formatDate(new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate() + interval + 1, 0, 0, 0), rs.dateFormat);
            case dwm.week: 
                return calendar.formatDate(new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate() + interval*7 + 1, 0, 0, 0), rs.dateFormat);
            case dwm.month: 
                return calendar.formatDate(new Date(startDate.getFullYear(), startDate.getMonth() + interval, startDate.getDate() + 1, 0, 0, 0), rs.dateFormat);
            case dwm.year: 
                return calendar.formatDate(new Date(startDate.getFullYear() + interval, startDate.getMonth(), startDate.getDate() + 1, 0, 0, 0), rs.dateFormat);
        }
    }

	
    function _getAlertLabel(type) {
        if (type != undefined) {
            switch (type) {
                case kAlertDefault: 		return calendar.options.eventEditor.dialogAlertOption_default;
                case kAlertNever:			return calendar.options.eventEditor.dialogAlertOption_never;
                case 1: 					return calendar.options.eventEditor.dialogAlertOption_5minutes;
                case 2: 					return calendar.options.eventEditor.dialogAlertOption_15minutes;
                case 3: 					return calendar.options.eventEditor.dialogAlertOption_30minutes;
                case 4: 					return calendar.options.eventEditor.dialogAlertOption_hour;
                case 5: 					return calendar.options.eventEditor.dialogAlertOption_2hours;
                case 6: 					return calendar.options.eventEditor.dialogAlertOption_day;
                default: 					return "Unknown";
            }
        }
    }
	
    function _getRepeatLabel(rule) {
        if (rule != undefined) {
            if (rule.Equals(ASC.Api.iCal.RecurrenceRule.Never)) {
                return calendar.options.eventEditor.dialogRepeatOption_never;
            }
            else if (rule.Equals(ASC.Api.iCal.RecurrenceRule.EveryDay)) {
                return calendar.options.eventEditor.dialogRepeatOption_day;
            }
            else if (rule.Equals(ASC.Api.iCal.RecurrenceRule.EveryWeek)) {
                return calendar.options.eventEditor.dialogRepeatOption_week;
            }
            else if (rule.Equals(ASC.Api.iCal.RecurrenceRule.EveryMonth)) {
                return calendar.options.eventEditor.dialogRepeatOption_month;
            }
            else if (rule.Equals(ASC.Api.iCal.RecurrenceRule.EveryYear)) {
                return calendar.options.eventEditor.dialogRepeatOption_year;
            }
            else {
                return calendar.options.eventEditor.dialogRepeatOption_custom;
            }
        }
    }
	
    function _disableRepeatSettings() {
        _settings.find(".fc-interval-selector").attr("disabled", "disabled");
        _settings.find("input").attr("disabled", "disabled");
        _settings.find(".fc-dwm-selector").find(".fc-selector-link").addClass("not-active");
        _settings.find(".fc-endrepeat-selector").find(".fc-selector-link").addClass("not-active");
    }
	
    function _enableRepeatSettings() {
        _settings.find(".fc-interval-selector").removeAttr("disabled");
        _settings.find("input").removeAttr("disabled");
        _settings.find(".fc-dwm-selector").find(".fc-selector-link").removeClass("not-active");
        _settings.find(".fc-endrepeat-selector").find(".fc-selector-link").removeClass("not-active");
    }
	
    function _renderRepeatAlertList() {
        // repeat list
        var _repeatSelectorLabel = _dialog.find(".fc-view-repeat");
		
        _dialog.find(".fc-view-repeat").click(
			function (event) {
			    fcMenus.hideMenus(fcMenus.modeMenuRepeatEventPage);
			    fcMenus.modeMenuRepeatEventPage.popupMenu("open", _repeatSelectorLabel);
			    event.stopPropagation();
			});
			
        if (!fcMenus.modeMenuRepeatEventPage || fcMenus.modeMenuRepeatEventPage.length < 1) {
            fcMenus.modeMenuRepeatEventPage = $('<div id="fc_mode_menu_repeat_eventpage"/>');
        } else {
            fcMenus.modeMenuRepeatEventPage.popupMenu("close");
            fcMenus.modeMenuRepeatEventPage.popupMenu("destroy");
        }
        fcMenus.modeMenuRepeatEventPage.popupMenu({
            anchor: "left,bottom",
            direction: "right,down",
            arrow: "up",
            showArrow: false,
            closeTimeout: -1,
            cssClassName: "asc-popupmenu",
            items: [
                {
                    label: calendar.options.eventEditor.dialogRepeatOption_never,
                    click: function() {
                        repeatRule = ASC.Api.iCal.RecurrenceRule.Never;
                        _repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
                        _closeSettings.call(_this, false);
                        _dialog.find(".repeat-settings").addClass("hidden");
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogRepeatOption_day,
                    click: function() {
                        repeatRule = ASC.Api.iCal.RecurrenceRule.EveryDay;
                        _repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
                        _closeSettings.call(_this, false);
                        _dialog.find(".repeat-settings").addClass("hidden");
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogRepeatOption_week,
                    click: function() {
                        repeatRule = ASC.Api.iCal.RecurrenceRule.EveryWeek ;
                        _repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
                        _closeSettings.call(_this, false);
                        _dialog.find(".repeat-settings").addClass("hidden");
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogRepeatOption_month,
                    click: function() {
                        repeatRule = ASC.Api.iCal.RecurrenceRule.EveryMonth;
                        _repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
                        _closeSettings.call(_this, false);
                        _dialog.find(".repeat-settings").addClass("hidden");
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogRepeatOption_year,
                    click: function() {
                        repeatRule = ASC.Api.iCal.RecurrenceRule.EveryYear;
                        _repeatSelectorLabel.find(".fc-selector-link").text(_getRepeatLabel.call(_this, repeatRule));
                        _closeSettings.call(_this, false);
                        _dialog.find(".repeat-settings").addClass("hidden");
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogRepeatOption_custom,
                    click: function() {
                        _repeatSelectorLabel.find(".fc-selector-link").text(calendar.options.eventEditor.dialogRepeatOption_custom);
                        _openSettings.call(_this);
                        _dialog.find(".repeat-settings").removeClass("hidden");
                    }
                }
            ]
        });
		
        // alert list  
        var _alertSelectorLabel = _dialog.find(".fc-view-alert");
		
        _dialog.find(".fc-view-alert").click(
			function (event) {
			    fcMenus.hideMenus(fcMenus.modeMenuAlertEventPage);
			    fcMenus.modeMenuAlertEventPage.popupMenu("open", _alertSelectorLabel);
			    event.stopPropagation();
			});

        if (!fcMenus.modeMenuAlertEventPage || fcMenus.modeMenuAlertEventPage.length < 1) {
            fcMenus.modeMenuAlertEventPage = $('<div id="fc_mode_menu_alert_eventpage"/>');
        } else {
            fcMenus.modeMenuAlertEventPage.popupMenu("close");
            fcMenus.modeMenuAlertEventPage.popupMenu("destroy");
        }
        fcMenus.modeMenuAlertEventPage.popupMenu({
            anchor: "left,bottom",
            direction: "right,down",
            arrow: "up",
            showArrow: false,
            closeTimeout: -1,
            cssClassName: "asc-popupmenu",
				
            items: [
                {
                    label: calendar.options.eventEditor.dialogAlertOption_default,
                    click: function() {
                        alertType = kAlertDefault;
                        _alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogAlertOption_never,
                    click: function() {
                        alertType = kAlertNever;
                        _alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogAlertOption_5minutes,
                    click: function() {
                        alertType = 1;
                        _alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogAlertOption_15minutes,
                    click: function() {
                        alertType = 2;
                        _alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogAlertOption_30minutes,
                    click: function() {
                        alertType = 3;
                        _alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogAlertOption_hour,
                    click: function() {
                        alertType = 4;
                        _alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogAlertOption_2hours,
                    click: function() {
                        alertType = 5;
                        _alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
                    }
                },
                {
                    label: calendar.options.eventEditor.dialogAlertOption_day,
                    click: function() {
                        alertType = 6;
                        _alertSelectorLabel.find(".fc-selector-link").text(_getAlertLabel.call(_this, alertType));
                    }
                }
            ]
        });
    }


    function _open(mode, elem, eventObj) {
        calendar.trigger("getPermissions", _this,
            {
                type: kEventPermissions,
                objectId: eventObj.objectId,
                permissions: eventObj.permissions
            },
            function(res) {
                if (res.result) {
                    eventObj.permissions = res.permissions;
                    if (res.isShared != undefined) {
                        eventObj.isShared = res.isShared;
                    }
                }
                _openPage(mode, elem, eventObj);
            }
        );
    }

    function _openPage(mode, elem, eventObj) {

        _dialog.find(".repeat-settings").addClass("hidden");

        // check length
        var titleMaxLen = defaults.eventMaxTitleLength;
        var inputTxt = _dialog.find(".editor .title input, .editor .location input");
        if (inputTxt.length > 0)
        {
            inputTxt.keyup(function(){
                if (inputTxt.val().length > titleMaxLen){
                    inputTxt.val(inputTxt.val().substr(0, titleMaxLen));
                }
            });
        }
        //

        _eventObj = eventObj;
        _eventObj.sourceId = _eventObj.sourceId || (_eventObj.source ? _eventObj.source.objectId : undefined);
		
        repeatRule = _eventObj.repeatRule;

        _canChangeSource = _eventObj.source == undefined ||
				_eventObj.source && !_eventObj.source.isSubscription;
        _canDelete = fcUtil.objectIsEditable(_eventObj) ||
				fcUtil.objectIsEditable(_eventObj.source) ||
				_eventObj.source && !_eventObj.source.isSubscription;
        _canEdit = _eventObj.source == undefined || _canDelete;

        _canUnsubscribe = _eventObj && _eventObj.canUnsubscribe &&
				_eventObj.source && _eventObj.source.isSubscription;

        if (_canEdit) {
            _dialog.find(".buttons").addClass("editable");
        } else {
            _dialog.find(".buttons").removeClass("editable");
        }
        if (_canDelete) {
            _dialog.find(".buttons").addClass("erasable");
        } else {
            _dialog.find(".buttons").removeClass("erasable");
        }
        if (_canEdit || _canDelete) {
            _dialog.find(".buttons").removeClass("readonly");
        } else {
            _dialog.find(".buttons").addClass("readonly");
        }
        if (_canUnsubscribe) {
            _dialog.find(".buttons").addClass("shared");
        } else {
            _dialog.find(".buttons").removeClass("shared");
        }

        _doDDX.call(_this);

        _dialog.find(".viewer .repeat-settings").remove();

        if (mode == "edit") {
            _dialog.find(".event-header span").text(_eventObj.objectId ? window.g_fcOptions.eventEditor.dialogHeader_editEvent : window.g_fcOptions.eventEditor.dialogHeader_createEvent);
            _dialog.find(".event-editor").addClass("edit-popup");
            _enableRepeatSettings.call(_this);
        } else {
            _dialog.find(".event-header span").text(window.g_fcOptions.eventEditor.dialogHeader_viewEvent);
            _dialog.find(".event-editor").removeClass("edit-popup");
            _disableRepeatSettings.call(_this);
            _dialog.find(".editor .repeat-settings").clone().insertAfter(_dialog.find(".viewer .repeat-alert"));
        }

        jq("#asc_calendar").hide();
        jq("#asc_event").show();

        _dialog.find(".editor .title input").focus().select();
    }


    // repeat interval
    function _setRepeatInterval(selectedItem, count) {
        if (selectedItem <= count) {
            var intervals = "";
            for (var i = 1; i < count; i++) {
                intervals += "<option" + ((selectedItem == i) ? " selected" : "") + " value='" + i + "'>" + i + "</option>";
            }
			
            var html = $(intervals);
            _settings.find(".fc-interval-selector").html(html);
			
            _settings.find(".fc-interval-selector").change(function() {
                _settings.find(".to-date").val(_getEndDate.call(_this, dwm_current));
            });
        }
    }
	
    // repeat days of week
    function _setSelectedDays(indexArr) {
        var daysSection = "";

        var dayNamesInCurCulture;
        var beforeFirstDay = [];
        var afterFirstDay = [];

        for (var i = 0; i < rs.dayNamesShort.length; i++) {
            var item = { index: i, name: rs.dayNamesShort[i] };
            if(i < g_fcOptions.firstDay){
                beforeFirstDay.push(item);
            } else {
                afterFirstDay.push(item);
            }
        }
        dayNamesInCurCulture = afterFirstDay.concat(beforeFirstDay);

        for (var i = 0; i < dayNamesInCurCulture.length; i++) {
            var isSelected = false;
            for (var j = 0; j < indexArr.length; j++) {
                if (dayNamesInCurCulture[i].index == indexArr[j]) {
                    isSelected = true;
                    break;
                }
            }
			
            daysSection += 	"<div class='checkbox-day'>" +
								"<input class='repeat-day-" + dayNamesInCurCulture[i].index + "' type='checkbox' " + ((isSelected == true) ? "checked" : "") + ">" +
								"<div>" + htmlEscape(dayNamesInCurCulture[i].name) + "</div>" +
							"</div>";
        }
        daysSection += "<div class='clear'></div>";
        var html = $(daysSection);
        _settings.find(".fc-days-week").html(html);
    }
    
    function _getSelectedDays() {
        indexArr = new Array();
        if (_settings.find(".fc-days-week.hidden").length == 0) {
            for (var i = 0; i < rs.dayNamesShort.length; i++) {
                var checkedClass = ".repeat-day-" + i;
                var checkedItem = _settings.find(checkedClass).is(":checked");
                if (checkedItem == true) {
                    indexArr.push(i);
                }
            }
        }
        return indexArr;
    }
	
    function _getRepeatDayByIndex(index) {
        switch (index) {
            case 0: return ASC.Api.iCal.DayOfWeek.Sunday;
            case 1: return ASC.Api.iCal.DayOfWeek.Monday;
            case 2: return ASC.Api.iCal.DayOfWeek.Tuesday;
            case 3: return ASC.Api.iCal.DayOfWeek.Wednesday;
            case 4: return ASC.Api.iCal.DayOfWeek.Thursday;
            case 5: return ASC.Api.iCal.DayOfWeek.Friday;
            case 6: return ASC.Api.iCal.DayOfWeek.Saturday;
        }
    }
	
    function _openSettings()
    {
        var fromD = _dialog.find(".editor .from-date");
        _settings.find(".from-date").val(fromD.val());
        _validateDateFieldsSettings.call(_this);
		
        var date = parseISO8601(fromD.val());
        var endDate = undefined;
		
        var maxDayCount = 32 - new Date(date.getFullYear(), date.getMonth(), 32).getDate();
        var dayOfWeek = date.getDay();
        var dayOfMonth = date.getDate();
		
        var indexDay = Math.floor((dayOfMonth - 1) / 7);
        var maxIndexDay = indexDay + Math.floor((maxDayCount - dayOfMonth) / 7);
		
        var dayName = calendar.options.repeatSettings.dayNames[dayOfWeek];
        var dayAlias = "";
		
        switch (indexDay) {
            case 0:
            case 1:
            case 2:
            case 4: dayAlias = rs.dayAliasNames[indexDay]; break;
            case 3: (indexDay != maxIndexDay) ? dayAlias = rs.dayAliasNames[indexDay] : dayAlias = rs.dayAliasNames[indexDay + 1]; break;
        }
		
        var html = $("<div>" + 
						"<div style='margin-bottom: 0.5em;'>" +
						"<input class='only-day' type='radio' name='day-radio' value='" + dayOfMonth + "' />&nbsp;" + 
							"<span class='only-day-label'>" + rs.dialogEachLabel + " " + dayOfMonth + " " + rs.dialogIntervalOption_day + "</span></div>" +
					 	"<input class='each-day' type='radio' name='day-radio' value='" + dayName + "'/>&nbsp;" + 
					 		"<span class='each-day-label'>" + rs.dialogAliasLabel + " " + dayAlias + " " + dayName + "</span>" +
					 "</div>");
					 
        _settings.find(".fc-month-radio").html(html);
        _settings.find(".only-day").attr({checked: "checked"});
		
        _settings.find(".only-day-label").click(function() {
            _settings.find(".only-day").trigger("click");
        });
		
        _settings.find(".each-day-label").click(function() {
            _settings.find(".each-day").trigger("click");
        });
		
        dayRuleObject = undefined;
        _settings.find(".each-day").click(function () {
            dayRuleObject = _getRepeatDayByIndex.call(_this, dayOfWeek);
            var dayRuleIndex = 0;
			
            switch (indexDay) {
                case 0:
                case 1:
                case 2: dayRuleIndex  = indexDay + 1; break;
                case 3: dayRuleIndex  = -2; break;
                case 4: dayRuleIndex  = -1; break;
            }
            if (indexDay == maxIndexDay) {
                dayRuleIndex = -1;
            }
			
            dayRuleObject.Order = dayRuleIndex;
        });
		
        _settings.find(".only-day").click(function () {
            dayRuleObject = undefined;
        });
		
        _setRepeatInterval.call(this, repeatRule.Interval, 31);
        _settings.find(".fc-cycle-times").val((repeatRule.Count > 0) ? repeatRule.Count : 3);
		
        var dayIndexArr = new Array();
        for (var i = 0; i < repeatRule.ByDay.length; i++) {
            dayIndexArr.push(rs.dayIndexResponse[repeatRule.ByDay[i].Id]);
        }
		
        _setSelectedDays.call(this, dayIndexArr.length ? dayIndexArr : [dayOfWeek]);
		
        // set default view
        if ((repeatRule.Freq == ASC.Api.iCal.Frequency.Never) || (repeatRule.Freq == ASC.Api.iCal.Frequency.Daily)) {
            _showDaySections.call(_this);
            dwm_current = dwm.day;
            _settings.find(".fc-dwm-selector").find(".fc-selector-link").text(rs.dialogRepeatOn_days);
            _settings.find(".fc-interval-label").text(rs.dialogIntervalOption_day);
        }
        else if (repeatRule.Freq == ASC.Api.iCal.Frequency.Weekly) {
            _showWeekSections.call(_this);
            dwm_current = dwm.week;
            _settings.find(".fc-dwm-selector").find(".fc-selector-link").text(rs.dialogRepeatOn_weeks);
            _settings.find(".fc-interval-label").text(rs.dialogIntervalOption_week);
        }
        else if (repeatRule.Freq == ASC.Api.iCal.Frequency.Monthly) {
            _showMonthSections.call(_this);
            dwm_current = dwm.month;
            _settings.find(".fc-dwm-selector").find(".fc-selector-link").text(rs.dialogRepeatOn_months);
            _settings.find(".fc-interval-label").text(rs.dialogIntervalOption_month);
			
            if ((repeatRule.ByDay.length == 1) && (repeatRule.ByDay[0].Order != 0)) {
                _settings.find(".only-day").attr({checked: ""});
                _settings.find(".each-day").attr({checked: "checked"}).click();
            }
            else {
                _settings.find(".each-day").attr({checked: ""});
                _settings.find(".only-day").attr({checked: "checked"});
            }
        } else if (repeatRule.Freq == ASC.Api.iCal.Frequency.Yearly) {
            _showDaySections.call(_this);
            dwm_current = dwm.year;
            _settings.find(".fc-dwm-selector").find(".fc-selector-link").text(rs.dialogRepeatOn_years);
            _settings.find(".fc-interval-label").text(rs.dialogIntervalOption_year);
        }
		
        _settings.find(".to-date").val(repeatRule.Until ? calendar.formatDate(repeatRule.Until, rs.dateFormat) : 
																	_getEndDate.call(_this, dwm_current));
		
        // end repeat
        if (repeatRule.Until) {
            _showEndSectionsDate.call(_this);
            _settings.find(".fc-endrepeat-selector").find(".fc-selector-link").text(rs.dialogOptionDate);
        }
        else if (repeatRule.Count > 0) {
            _showEndSectionsCycles.call(_this);
            _settings.find(".fc-endrepeat-selector").find(".fc-selector-link").text(rs.dialogOptionCount);
        }
        else {
            _showEndSectionsNever.call(_this);
            _settings.find(".fc-endrepeat-selector").find(".fc-selector-link").text(rs.dialogOptionNever);
        }
    }
	
    function _openDelSettings(single)
    {
        _delSettings.popupFrame("close");

        if (single)
            _delSettings.addClass("single");
        else
            _delSettings.removeClass("single");

        _delSettings.find("input[type=radio]:first").prop("checked", true);

        uiBlocker.hide();
		
        _delSettings.popupFrame("open", { pageX: "center", pageY: "center" });
        _delSettings.addClass("add-popup");
        _delSettings.addClass("fc-shadow");
        _delSettings.css("position","fixed");
		
        $(document).bind("keyup", _checkEscKeyDelSettings);

        if (_delSettings.popupFrame("isVisible")) {
            uiBlocker.show();
        } else {
            uiBlocker.hide();
        }
    }

    function _close(changed, deleted) {
        if (changed) {
            if (!_canEdit || false == _doDDX.call(_this, true)) {return;}
            _updateEvent.call(_this);
            return;
        }
        if (_eventObj.source == undefined && _eventObj._id != undefined) {
            calendar.removeEvents(_eventObj._id);
        }
		
        if ((_eventObj.source != undefined) && !changed && !deleted)
            _updateEvent.call(_this, true);

        _closeDialog.call(_this);
    }
	
    function _closeSettings(changed) {
        if (changed && _canEdit) {
            _prepareRepeatRule.call(_this);
        }
    }
	
    function _closeDelSettings(changed) {
        if (changed) {
            if (!_canEdit) {return;}
            _closeDialogDelSettings.call(_this);
            return;
        }
		
        $(document).unbind("keyup", _checkEscKeyDelSettings);
        
        _closeDialogDelSettings.call(_this);
    }

    function _checkEscKeyDelSettings(ev) {
        if (ev.which == 27) {
            _closeDelSettings.call(_this, false);
        }
    }


    function _getConfirmViewMode(checkDelete) {
        
        confirmSettings.notificationUsers.newGuests = [];
        confirmSettings.notificationUsers.updatedGuests = [];
        confirmSettings.notificationUsers.removedGuests = [];
        confirmSettings.selectedViewMode = confirmSettings.viewMode.none;
        confirmSettings.selectedNotificationMode = confirmSettings.notificationMode.nobody;

        if(_eventObj.status == 2 || (_eventObj.source && _eventObj.source.isSubscription))
            return confirmSettings.selectedViewMode;

        var oldEvent = jq.extend({}, _eventObj);

        if (!checkDelete) {
            var validData = _doDDX.call(_this, true);
            if (!validData)
                return confirmSettings.selectedViewMode;
        }

        var isOrganizer = false;
        var organizerEmail = _eventObj.organizer ? _eventObj.organizer[3].replace(new RegExp("mailto:", "ig"), "").toLowerCase() : "";

        jq.each(ASC.Mail.Accounts, function(index, account) {
            if (account.enabled && organizerEmail == account.email.toLowerCase()) {
                isOrganizer = true;
                return false;
            }
            return true;
        });

        if(!isOrganizer)
            return confirmSettings.selectedViewMode;

        if (checkDelete) {
            var removedGuests = [];
            jq.each(_eventObj.attendees || [], function(index, attendeeObj) {
                var attendeeEmail = attendeeObj[3].replace(new RegExp("mailto:", "ig"), "");
                if (attendeeEmail.toLowerCase() != organizerEmail)
                    removedGuests.push(attendeeEmail);
            });

            if (removedGuests.length) {
                confirmSettings.selectedViewMode = confirmSettings.viewMode.deleteEvent;
                confirmSettings.notificationUsers.removedGuests = removedGuests;
            }

            return confirmSettings.selectedViewMode;
        }

        var oldAttendees = [];
        jq.each(oldEvent.attendees || [], function(index, attendeeObj) {
            var attendeeEmail = attendeeObj[3].replace(new RegExp("mailto:", "ig"), "");
            if (attendeeEmail.toLowerCase() != organizerEmail)
                oldAttendees.push(attendeeEmail);
        });

        var newAttendees = [];
        jq.each(_eventObj.attendees || [], function(index, attendeeObj) {
            var attendeeEmail = attendeeObj[3].replace(new RegExp("mailto:", "ig"), "");
            if (attendeeEmail.toLowerCase() != organizerEmail)
                newAttendees.push(attendeeEmail);
        });

        if (oldAttendees.length == 0 && newAttendees.length == 0)
            return confirmSettings.selectedViewMode;

        if (!oldEvent.objectId) {
            if (newAttendees.length) {
                confirmSettings.selectedViewMode = confirmSettings.viewMode.addEvent;
                confirmSettings.notificationUsers.newGuests = newAttendees;
            }

            return confirmSettings.selectedViewMode;
        }

        var oldOrganizerEmail = oldEvent.organizer ? oldEvent.organizer[3].replace(new RegExp("mailto:", "ig"), "").toLowerCase() : "";
        var organizerChanged = oldOrganizerEmail && oldOrganizerEmail != organizerEmail;

        var oldStartData = oldEvent.start ? oldEvent.start.getTime() : null;
        var newStartData = _eventObj.start ? _eventObj.start.getTime() : null;
        var oldEndData = oldEvent.end ? oldEvent.end.getTime() : null;
        var newEndData = _eventObj.end ? _eventObj.end.getTime() : null;

        if (organizerChanged ||
            oldEvent.title != _eventObj.title ||
            oldEvent.location != _eventObj.location ||
            oldEvent.description != _eventObj.description ||
            oldStartData != newStartData ||
            oldEndData != newEndData ||
            oldEvent.allDay != _eventObj.allDay ||
            oldEvent.sourceId != _eventObj.newSourceId ||
            oldEvent.repeatRule.ToiCalString() != _eventObj.repeatRule.ToiCalString())
            confirmSettings.selectedViewMode  = confirmSettings.viewMode.updateEvent;

        jq.each(newAttendees, function (index, newAttendee) {
            if (jq.inArray(newAttendee, oldAttendees) < 0)
                confirmSettings.notificationUsers.newGuests.push(newAttendee);
            else
                confirmSettings.notificationUsers.updatedGuests.push(newAttendee);
        });
        
        confirmSettings.notificationUsers.removedGuests = jq.grep(oldAttendees, function (oldAttendee) {
            return jq.inArray(oldAttendee, newAttendees) < 0;
        });

        if (confirmSettings.notificationUsers.newGuests.length || confirmSettings.notificationUsers.removedGuests.length)
            confirmSettings.selectedViewMode  = confirmSettings.viewMode.updateGuests;

        return confirmSettings.selectedViewMode;
    }

    function _openConfirmPopup()
    {
        switch (confirmSettings.selectedViewMode) {
            case confirmSettings.viewMode.addEvent:
                _confirmPopup.find(".title").text(calendar.options.confirmPopup.dialogAddEventHeader);
                _confirmPopup.find(".body").text(calendar.options.confirmPopup.dialogAddEventBody);
                _confirmPopup.find(".send-btn").show();
                _confirmPopup.find(".send-customs-btn, .send-everyone-btn").hide();
                break;
            case confirmSettings.viewMode.updateEvent:
                _confirmPopup.find(".title").text(calendar.options.confirmPopup.dialogUpdateEventHeader);
                _confirmPopup.find(".body").text(calendar.options.confirmPopup.dialogUpdateEventBody);
                _confirmPopup.find(".send-btn").show();
                _confirmPopup.find(".send-customs-btn, .send-everyone-btn").hide();
                break;
            case confirmSettings.viewMode.updateGuests:
                _confirmPopup.find(".title").text(calendar.options.confirmPopup.dialogUpdateEventHeader);
                _confirmPopup.find(".body").text(calendar.options.confirmPopup.dialogUpdateGuestsBody);
                _confirmPopup.find(".send-btn").hide();
                _confirmPopup.find(".dont-send-btn").hide();
                _confirmPopup.find(".send-customs-btn, .send-everyone-btn").show();
                break;
            case confirmSettings.viewMode.deleteEvent:
                _confirmPopup.find(".title").text(calendar.options.confirmPopup.dialogDeleteEventHeader);
                _confirmPopup.find(".body").text(calendar.options.confirmPopup.dialogDeleteEventBody);
                _confirmPopup.find(".send-btn").show();
                _confirmPopup.find(".send-customs-btn, .send-everyone-btn").hide();
                break;
            default:
                break;
        }

        _delSettings.popupFrame("close");
        $(document).unbind("keyup", _checkEscKeyDelSettings);
        
        _confirmPopup.popupFrame("close");
        uiBlocker.hide();
		
        _confirmPopup.popupFrame("open", { pageX: "center", pageY: "center" });
        _confirmPopup.css("position","fixed");

        if (_confirmPopup.popupFrame("isVisible")) {
            uiBlocker.show();
        } else {
            uiBlocker.hide();
        }
    }

    function _closeConfirmPopup() {
        _confirmPopup.popupFrame("close");
        uiBlocker.hide();
    }

    function _sendGuestsNotification(mode) {
        confirmSettings.selectedNotificationMode = mode;
        if (confirmSettings.selectedViewMode == confirmSettings.viewMode.deleteEvent) {
            _deleteEvent.call(_this, confirmSettings.selectedDeleteMode, _eventObj._start);
            _close.call(_this, false, true);
        } else {
            _close.call(_this, true);
        }
        _closeConfirmPopup.call(_this);
    }

    function _sendNotification(sourceId, uniqueId, type, callback) {
        var attendeesEmails = [];
        var method = null;

        switch (type) {
        case confirmSettings.notificationType.request:
            switch (confirmSettings.selectedNotificationMode) {
            case confirmSettings.notificationMode.everybody:
            case confirmSettings.notificationMode.partially:
                attendeesEmails = confirmSettings.notificationUsers.newGuests;
                method = ASC.Mail.Utility.SendCalendarRequest;
                break;
            default:
                break;
            }
            break;
        case confirmSettings.notificationType.update:
            switch (confirmSettings.selectedNotificationMode) {
            case confirmSettings.notificationMode.everybody:
                attendeesEmails = confirmSettings.notificationUsers.updatedGuests;
                method = ASC.Mail.Utility.SendCalendarUpdate;
                break;
            default:
                break;
            }
            break;
        case confirmSettings.notificationType.cancel:
            switch (confirmSettings.selectedNotificationMode) {
            case confirmSettings.notificationMode.everybody:
            case confirmSettings.notificationMode.partially:
                attendeesEmails = confirmSettings.notificationUsers.removedGuests;
                method = ASC.Mail.Utility.SendCalendarCancel;
                break;
            default:
                break;
            }
            break;
        default:
            break;
        }

        if (attendeesEmails.length && method) {
            ASC.CalendarController.Busy = true;
            window.LoadingBanner.displayLoading();
            
            method.call(this, sourceId, uniqueId, attendeesEmails)
                .done(function() {
                    //toastr.success(calendar.options.confirmPopup.dialogSuccessToastText);
                    console.log(calendar.options.confirmPopup.dialogSuccessToastText, arguments);
                    if (callback)
                        callback();
                })
                .fail(function() {
                    toastr.error(calendar.options.confirmPopup.dialogErrorToastText);
                    console.error(calendar.options.confirmPopup.dialogErrorToastText, arguments);
                })
                .always(function () {
                    ASC.CalendarController.Busy = false;
                    window.LoadingBanner.hideLoading();
                });
        } else {
            if(callback)
                callback();
        }
    }

    function _sendReply(calendarId, eventUid) {
        if (!replyDecisionSettings.sendReply || !replyDecisionSettings.email || !replyDecisionSettings.decision)
            return;

        ASC.CalendarController.Busy = true;
        window.LoadingBanner.displayLoading();
        
        ASC.Mail.Utility.SendCalendarReply(calendarId, eventUid, replyDecisionSettings.email, replyDecisionSettings.decision)
            .done(function () {
                console.log(calendar.options.confirmPopup.dialogSuccessToastText, arguments);
            })
            .fail(function () {
                toastr.error(calendar.options.confirmPopup.dialogErrorToastText);
                console.error(calendar.options.confirmPopup.dialogErrorToastText, arguments);
            })
            .always(function () {
                ASC.CalendarController.Busy = false;
                window.LoadingBanner.hideLoading();
            });
    }


    function _closeDialog() {
        window.toastr.remove();
        jq("#asc_event").hide();
        jq("#asc_calendar").show();
        calendar.updateSize();
        uiBlocker.hide();
    }
    
    function _disableDialogBtns(disable) {
        if (disable){
            _dialog.find(".buttons .save-btn").addClass("disable");
        } else {
            _dialog.find(".buttons .save-btn").removeClass("disable");
        }
    }
	
    function _closeDialogDelSettings() {
        _delSettings.popupFrame("close");
        uiBlocker.hide();
    }

    function _parseDateTime(inputD, inputT, result) {
        var dateStr = inputD.val();
        var timeStr = inputT.val();
        var dateStrIsEmpty = dateStr.search(/\S/) < 0;
        var timeStrIsEmpty = timeStr.search(/\S/) < 0;
        var date, time, r;
        if (!dateStrIsEmpty || !timeStrIsEmpty) {
            date = fcUtil.validateDateString(dateStr);
            time = fcUtil.validateTimeString(timeStr);
            r = (date === null || !timeStrIsEmpty && time === null) ? 0 : 1;
        } else {
            r = -1;
            date = null;
            time = null;
        }
        result.date = {isEmpty: dateStrIsEmpty, isValid: date !== null, value: date};
        result.time = {isEmpty: timeStrIsEmpty, isValid: time !== null, value: time};
        return r;
    }

    var changeTimeFrom = null;
    
    function _validateDateFields(result) {
        var allday = _dialog.find(".editor .all-day input").is(":checked");
        var dlg = {
            from:      _dialog.find(".editor .from-date"),
            from_t:    _dialog.find(".editor .from-time"),
            to:        _dialog.find(".editor .to-date"),
            to_t:      _dialog.find(".editor .to-time")
        };
        var frDate = {}, fr = _parseDateTime(dlg.from, dlg.from_t, frDate);
        var toDate = {}, to = _parseDateTime(dlg.to, dlg.to_t, toDate);
        frDate.dateTime = fr == 1 ? parseDate(frDate.date.value + "T" + frDate.time.value) : null;
        toDate.dateTime = to == 1 ? parseDate(toDate.date.value + "T" + toDate.time.value) : null;

        var frc, frtc, toc, totc, r, delta;
        if (!allday && fr == 1 && to == 1 && (delta = toDate.dateTime - frDate.dateTime > 0) ||
			allday && fr == 1 && to == -1 ||
			allday && fr == 1 && to == 1 && (delta = toDate.dateTime - frDate.dateTime >= 0)) {
            r = true;
            frc = frtc = toc = totc = "";
        } else {
            r = false;
            frc  = frDate.date.isValid ? "" : "#cc3300";
            frtc = frDate.time.isValid ? "" : "#cc3300";
            toc  = toDate.date.isValid && delta ? "" : "#cc3300";
            totc = toDate.time.isValid && delta ? "" : "#cc3300";
        }
        dlg.from.css("color", "").css("border-color", frc);
        dlg.from_t.css("color", "").css("border-color", frtc);
        dlg.to.css("color", "").css("border-color", toc);
        dlg.to_t.css("color", "").css("border-color", totc);

        if ($(this).hasClass('from-time') || $(this).hasClass('from-date')) {

            var oldTime = new Date(Date.parse(_eventObj._start ? _eventObj._start : _eventObj.start));
            var newTime = new Date(Date.parse(frDate.dateTime));
            
            if (changeTimeFrom != null) {
                oldTime = changeTimeFrom;
            }
            
            var diff = newTime.getTime() - oldTime.getTime();
            if (diff != NaN && diff != 0) {

                var date = new Date();
                date.setTime(Date.parse(toDate.dateTime) + diff);

                if (date != 'Invalid Date') {
                    toDate.dateTime.setTime(Date.parse(toDate.dateTime) + diff);

                    changeTimeFrom = newTime;

                    if ($(this).hasClass('from-time')) {
                        var time =
                            (toDate.dateTime.getHours() < 10 ? '0' + toDate.dateTime.getHours() : toDate.dateTime.getHours())
                            + ":" +
                            (toDate.dateTime.getMinutes() < 10 ? '0' + toDate.dateTime.getMinutes() : toDate.dateTime.getMinutes());

                        toDate.time.value = time;
                        $('input.to-time').val(time);
                        
                    } else if ($(this).hasClass('from-date')) {
                        
                        var newDate = date.getFullYear() + '-'
                           + ((date.getMonth() + 1) < 10 ? '0' + (date.getMonth() + 1) : (date.getMonth() + 1)) + '-'
                           + (date.getDate() < 10 ? '0' + date.getDate() : date.getDate());
                       
                        toDate.date.value = newDate;
                        $('input.to-date')[0].value = newDate;
                    }
                    _validateDateFields.call(_this);
                }
            }
        }

        if (result != undefined) {
            result.fromDate = frDate;
            result.toDate   = toDate;
        }
		
        return r;
    }
	
    function _validateDateFieldsSettings(result) {
		
        var r = true;
        var allday = _dialog.find(".editor .all-day input").is(":checked");
        var dlg = {
            from:              _settings.find(".from-date"),
            from_t:            _settings.find(".from-time"),
            to:                _settings.find(".to-date"),
            to_t:              _settings.find(".to-time"),
            cycles:            _settings.find(".fc-cycle-times")
        };
		
        if (!_settings.find(".fc-end-date").hasClass("hidden")) {
			
            var frDate = {}, fr = _parseDateTime(dlg.from, dlg.from_t, frDate);
            var toDate = {}, to = _parseDateTime(dlg.to, dlg.to_t, toDate);
            frDate.dateTime = fr == 1 ? parseDate(frDate.date.value + "T" + frDate.time.value) : null;
            toDate.dateTime = to == 1 ? parseDate(toDate.date.value + "T" + toDate.time.value) : null;
	
            var frc, frtc, toc, totc, delta;
            if (!allday && fr == 1 && to == 1 && (delta = toDate.dateTime - frDate.dateTime > 0) ||
			    allday && fr == 1 && to == -1 ||
			    allday && fr == 1 && to == 1 && (delta = toDate.dateTime - frDate.dateTime >= 0)) {
                r = true;
                frc = frtc = toc = totc = "";
            } else {
                r = false;
                frc  = frDate.date.isValid ? "" : "#cc3300";
                frtc = frDate.time.isValid ? "" : "#cc3300";
                toc  = toDate.date.isValid && delta ? "" : "#cc3300";
                totc = toDate.time.isValid && delta ? "" : "#cc3300";
            }
            dlg.from.css("color", "").css("border-color", frc);
            dlg.from_t.css("color", "").css("border-color", frtc);
            dlg.to.css("color", "").css("border-color", toc);
            dlg.to_t.css("color", "").css("border-color", totc);
            if (result != undefined) {
                result.fromDate = frDate;
                result.toDate   = toDate;
            }
        }
		
        if (!_settings.find(".fc-repeat-cycles").hasClass("hidden")) {
            var cb = "";
            r = true;
            var isNum = dlg.cycles.val().match('^[0-9]+$');
            if ( (dlg.cycles.find(".hidden").length == 0) && !isNum ) { 
                r = false;
                cb  = !r || (r && (dlg.cycles.val() < 0)) ? "#cc3300" : "";
            }
            dlg.cycles.css("color", "").css("border-color", cb);
        }
		
        return r;
    }

    function _handleAllDayClick() {
        var allday = _dialog.find(".editor .all-day input").is(":checked");
        var dlg = {
            from:  _dialog.find(".editor .from-date"),
            from_t:  _dialog.find(".editor .from-time"),
            to:      _dialog.find(".editor .to-date"),
            to_t:    _dialog.find(".editor .to-time")
        };
        if (allday) {
            //dlg.to.val("");
            dlg.from_t.val("").attr("disabled", "disabled");
            dlg.to_t.val("").attr("disabled", "disabled");
        } else {
            var defaultEndDate = new Date(_eventObj.start.getFullYear(), _eventObj.start.getMonth(), _eventObj.start.getDate(), _eventObj.start.getHours(), _eventObj.start.getMinutes() + 30);
			
            if(!dlg.from.val())
                dlg.from.val(formatDate(_eventObj.start, calendar.options.eventEditor.dateFormat));
            
            dlg.from_t
					.val(formatDate(_eventObj.start, calendar.options.eventEditor.timeFormat))
					.removeAttr("disabled");
            if (_eventObj.end == _eventObj.start) {
                _eventObj.end = new Date(_eventObj.end.getFullYear(), _eventObj.end.getMonth(), _eventObj.end.getDate(), _eventObj.end.getHours(), _eventObj.end.getMinutes() + 30);
            }
            dlg.to_t
					.val(_eventObj.end instanceof Date ?
							formatDate(_eventObj.end, calendar.options.eventEditor.timeFormat) : formatDate(defaultEndDate, calendar.options.eventEditor.timeFormat))
					.removeAttr("disabled");
            
            if(!dlg.to.val())
                dlg.to.val(_eventObj.end instanceof Date ?
					formatDate(_eventObj.end, calendar.options.eventEditor.dateFormat) : formatDate(defaultEndDate, calendar.options.eventEditor.dateFormat));
        }
        _validateDateFields();
    }

    function _prepareRepeatRule() {
        if (repeatRule) {
            repeatRule = undefined;
        }
		
        // get all values		
        var dlg = {
            from:        _settings.find(".from-date"),
            to:          _settings.find(".to-date"),
            interval:    _settings.find(".fc-interval-selector"),
            cycles:      _settings.find(".fc-cycle-times")
        };
		
        repeatRule = ASC.Api.iCal.RecurrenceRule.EveryDay;
        switch (dwm_current) {
            case dwm.day: repeatRule = ASC.Api.iCal.ParseRRuleFromString("FREQ=DAILY"); break;
            case dwm.week: repeatRule = ASC.Api.iCal.ParseRRuleFromString("FREQ=WEEKLY"); break;
            case dwm.month: repeatRule = ASC.Api.iCal.ParseRRuleFromString("FREQ=MONTHLY"); break;
            case dwm.year: repeatRule = ASC.Api.iCal.ParseRRuleFromString("FREQ=YEARLY"); break;
        }
		
        repeatRule.Interval = dlg.interval.find("option:selected").text();
        repeatRule.Count = _settings.find(".fc-repeat-cycles.hidden").length ? repeatRule.Count : dlg.cycles.val();
		
        var dayIndexArr = _getSelectedDays.call(_this);
        for (var i = 0; i < dayIndexArr.length; i++) {
            repeatRule.ByDay.push(_getRepeatDayByIndex.call(this, dayIndexArr[i]));
        }
		
        if ((_settings.find(".fc-month-radio.hidden").length == 0) && (dayRuleObject != undefined)) {
            repeatRule.ByDay.push(dayRuleObject);
        }
		
        if (_settings.find(".fc-end-date.hidden").length == 0) {
            var dates = {};
            if (!_validateDateFieldsSettings(dates)) {return false;}
            repeatRule.Until = dates.toDate.dateTime;
        }
		
        return (repeatRule != undefined);
    }

    function _doDDX(saveData) {
        var sources = calendar.getEventSources();
        var sourceIsValid = fcUtil.objectIsValid(_eventObj.source);
        var canChangeAlert = sourceIsValid && (
				_eventObj.source.canAlertModify != undefined && _eventObj.source.canAlertModify ||
				_eventObj.source.canAlertModify == undefined) ||
				!sourceIsValid;

        var dlg = {
            viewer: {
                title:       _dialog.find(".viewer .title .text"),
                location:    _dialog.find(".viewer .location .text"),
                attendees:   _dialog.find(".viewer .attendees-user-list"),
                owner:       _dialog.find(".viewer .owner .name"),
                allday:      _dialog.find(".viewer .all-day"),
                from:        _dialog.find(".viewer .from-date"),
                from_t:      _dialog.find(".viewer .from-time"),
                to:          _dialog.find(".viewer .to-date"),
                to_t:        _dialog.find(".viewer .to-time"),
                repeat:      _dialog.find(".viewer .repeat"),
                alert:       _dialog.find(".viewer .alert"),
                calendar:    _dialog.find(".viewer .calendar .name"),
                calendar_b:  _dialog.find(".viewer .calendar .bullet"),
                users:       _dialog.find(".viewer .shared-user-list"),
                description: _dialog.find(".viewer .description .text")
            },
            editor: {
                replybuttons:_dialog.find(".reply-buttons"),
                title:       _dialog.find(".editor .title input"),
                location:    _dialog.find(".editor .location input"),
                owner:       _dialog.find(".editor .owner .name"),
                ownerSelector: _dialog.find(".editor .owner select"),
                attendees:   _dialog.find(".editor .attendees input"),
                attendeesInputBtn:   _dialog.find(".editor .attendees .btn-container .button"),
                attendeesInputContainer:   _dialog.find(".editor .attendees .input-container"),
                attendeesList:_dialog.find(".editor .attendees .attendees-user-list"),
                attendeesNoAccount:_dialog.find(".editor .attendees .attendees-noaccount"),
                attendeesHelpSwitcher: _dialog.find("#attendeesHelpSwitcher"),
                allday:      _dialog.find(".editor .all-day input"),
                from:        _dialog.find(".editor .from-date"),
                from_t:      _dialog.find(".editor .from-time"),
                to:          _dialog.find(".editor .to-date"),
                to_t:        _dialog.find(".editor .to-time"),
                repeat:      _dialog.find(".editor .fc-view-repeat .fc-selector-link"),
                alert:       _dialog.find(".editor .fc-view-alert .fc-selector-link"),
                calendar:    _dialog.find(".editor .calendar select"),
                calendar_b:  _dialog.find(".editor .calendar .bullet"),
                users:       _dialog.find(".editor .addUserLink"),
                usersList:   _dialog.find(".editor .shared-user-list"),
                description: _dialog.find(".editor .description textarea"),
                
                sent_invitations: _dialog.find(".editor .sent-invitations input")
            },
            infoText:    _dialog.find(".info-text")
        };

        if (saveData) {
            return _doDDXSaveData.call(_this, dlg, sources, sourceIsValid, canChangeAlert);
        } else { 
            return _doDDXLoadData.call(_this, dlg, sources, sourceIsValid, canChangeAlert);
        }
        
    }

    function _doDDXSaveData(dlg, sources, sourceIsValid, canChangeAlert) {

        if (!_canEdit) {return false;}

        if (false == fcUtil.validateInput(dlg.editor.title, fcUtil.validateNonemptyString)) {return false;}
            
        _eventObj.title = $.trim(dlg.editor.title.val());
        _eventObj.title = _eventObj.title.substr(0, Math.min(calendar.options.eventMaxTitleLength, _eventObj.title.length));
        _eventObj.location = $.trim(dlg.editor.location.val());
        _eventObj.description = $.trim(dlg.editor.description.val());
        _eventObj.allDay = dlg.editor.allday.is(":checked");

        var dates = {};
            
        if (!_validateDateFields(dates)) {return false;}
            
        _eventObj.start = dates.fromDate.dateTime;
        _eventObj.end   = dates.toDate.dateTime;

        if (canChangeAlert) {
            _eventObj.repeatRule = repeatRule;
            _eventObj.alert.type = alertType;
        }

        _eventObj.newSourceId = dlg.editor.calendar.val();
        
        if (!_eventObj.sourceId && _getDefaultSource(_eventObj.newSourceId).isSubscription) {
            _eventObj.attendees = [];
        } else {
            dlg.editor.attendeesInputBtn.click(); //add guests for users who are too lazy to click on the button
            _eventObj.attendees =  dlg.editor.attendees.AttendeesSelector("get");
        }
        
        var replyDecision = null;
        var replyEmail = null;
        var selectedReplyDecision = dlg.editor.replybuttons.find(".reply-radio:checked").attr("data-value");
        
        jq(_eventObj.attendees).each(function(i, attendee) {

            var attendeeEmail = attendee[3].replace(new RegExp("mailto:", "ig"), "");
            var attendeePartstat = attendee[1].partstat.toUpperCase();
            var nextStep = true;

            jq(ASC.Mail.Accounts).each(function(j, account) {
                if (account.enabled && attendeeEmail.toLowerCase() == account.email.toLowerCase()) {
                    nextStep = false;
                    if (selectedReplyDecision && selectedReplyDecision != attendeePartstat) {
                        replyEmail = account.email;
                        replyDecision = selectedReplyDecision;
                    }
                    return nextStep;
                }
                return nextStep;
            });

            return nextStep;
        });

        if (replyDecisionSettings.sendReply && replyEmail && replyDecision) {
            replyDecisionSettings.email = replyEmail;
            replyDecisionSettings.decision = replyDecision;
        } else {
            replyDecisionSettings.email = null;
            replyDecisionSettings.decision = null;
            replyDecisionSettings.sendReply = false;
        }

        _eventObj.permissions = dlg.editor.users.ShareUsersSelector("get");

        var organizerObj = new ICAL.Property("organizer");
        var selectedAccountObj = dlg.editor.ownerSelector.find("option:selected");
        var selectedAccount = selectedAccountObj.length ? { email: selectedAccountObj.attr("value"), name: Encoder.htmlDecode(selectedAccountObj.attr("data-name")) } : ASC.Mail.DefaultAccount;

        if (!_eventObj.sourceId) {
            if (selectedAccount) {
                organizerObj.setParameter("cn", selectedAccount.name);
                organizerObj.setValue("mailto:" + selectedAccount.email);
                _eventObj.organizer = organizerObj.jCal;
            }
        } else {
            var isOrganizer = false;
            if (_eventObj.organizer && _eventObj.organizer[3]) {
                var organizerEmail = _eventObj.organizer[3].replace(new RegExp("mailto:", "ig"), "").toLowerCase();
                jq(ASC.Mail.Accounts).each(function(index, account) {
                    if (organizerEmail == account.email.toLowerCase()) {
                        isOrganizer = true;
                        return false;
                    }
                    return true;
                });
            }
            if (isOrganizer && selectedAccount) {
                organizerObj.setParameter("cn", selectedAccount.name);
                organizerObj.setValue("mailto:" + selectedAccount.email);
                _eventObj.organizer = organizerObj.jCal;
            }
        }

        var src;
            
        $.each(sources, function() {
            if (this.objectId == _eventObj.newSourceId) {
                src = this;
                return false;
            }
            return true;
        });
            
        if (src) {
            _eventObj.newTimeZone = $.extend({}, src.timeZone);
        }

        delete _eventObj.textColor;
        delete _eventObj.backgroundColor;
        delete _eventObj.borderColor;
            
        calendar.normalizeEvent(_eventObj);

        return true;
    }

    function _doDDXLoadData(dlg, sources, sourceIsValid, canChangeAlert) {

        dlg.editor.title.css("color", "").css("border-color", "");
        dlg.editor.title.val(_eventObj.title || "");
        dlg.viewer.title.text(_eventObj.title || "");
        dlg.editor.attendeesInputContainer.find('input')[0].value = "";

        dlg.editor.sent_invitations.prop("checked", true);

        if (_eventObj.status == 2) {
            dlg.infoText.show();
        } else {
            dlg.infoText.hide();
        }

        dlg.editor.location.val(_eventObj.location || "");
        dlg.viewer.location.text(_eventObj.location || "");

        if (!_eventObj.location) {
            dlg.viewer.location.parent().hide();
        } else {
            dlg.viewer.location.parent().show();
        }

        var organizerName = _eventObj.organizer && _eventObj.organizer.length > 1 ? _eventObj.organizer[1].cn : "";
        organizerName = organizerName ? organizerName : _eventObj.owner ? _eventObj.owner.name : "";
            
        if (organizerName) {
            dlg.editor.owner.html(htmlEscape(organizerName));
            dlg.viewer.owner.html(htmlEscape(organizerName));
            _dialog.find(".owner").show();
        } else {
            dlg.editor.owner.html("");
            dlg.viewer.owner.html("");
            _dialog.find(".owner").hide();
        }

        var isNewEvent = !_eventObj.sourceId;
        var hasAccounts = ASC.Mail.Accounts.length > 0;
        
        var isOrganizer = false;
        var canEditOrganizer = false;
        var canEditAttendees = false;
        var showNoAccountsLink = false;

        if (isNewEvent) {
            isOrganizer = true;
            if (hasAccounts) {
                canEditOrganizer = true;
                canEditAttendees = true;
                dlg.editor.ownerSelector.val(ASC.Mail.DefaultAccount.email).change();
            } else {
                showNoAccountsLink = true;
            }
        } else {
            if (_eventObj.organizer && _eventObj.organizer[3]) {
                var organizerEmail = _eventObj.organizer[3].replace(new RegExp("mailto:", "ig"), "").toLowerCase();
                jq(ASC.Mail.Accounts).each(function(index, account) {
                    if (organizerEmail == account.email.toLowerCase()) {
                        isOrganizer = true;
                        dlg.editor.ownerSelector.val(account.email).change();
                        return false;
                    }
                    return true;
                });
            } else if (_eventObj.owner && _eventObj.owner.objectId === Teamlab.profile.id) {
                isOrganizer = true;
            }
            
            if (hasAccounts) {
                canEditOrganizer = isOrganizer;
                canEditAttendees = isOrganizer;
            } else {
                showNoAccountsLink = isOrganizer;
            }
        }

        if (canEditOrganizer) {
            dlg.editor.ownerSelector.parents(".selector").show();
            dlg.editor.owner.hide();
        } else {
            dlg.editor.ownerSelector.parents(".selector").hide();
            dlg.editor.owner.show();
        }

        var attendees = _eventObj.attendees || [];

        if (_canEdit && attendees.length) {
            var text;

            if (_eventObj.source.isSubscription)
                text = calendar.options.confirmPopup.editorInfoTextSubscription;
            else if(!isOrganizer)
                text = calendar.options.confirmPopup.editorInfoText;
            
            if(text)
                window.toastr.warning(text, "", { "closeButton": false, "timeOut": "0", "extendedTimeOut": "0" });
        }

        if (ASC.Mail.Enabled && showNoAccountsLink) {
            dlg.editor.attendeesNoAccount.removeClass("display-none");
        } else {
            dlg.editor.attendeesNoAccount.addClass("display-none");
        }

        if (canEditAttendees) {
            dlg.editor.attendeesHelpSwitcher.show();
        } else {
            dlg.editor.attendeesHelpSwitcher.hide();
        }

        dlg.editor.attendees.AttendeesSelector("set", attendees, canEditAttendees);
        
        if (_eventObj.attendees && _eventObj.attendees.length) {
                
            dlg.viewer.attendees.empty().parent().show();
            
            var replyDecision = null;

            jq.each(_eventObj.attendees, function (index, item) {
                
                var attendeeEmail = item[3].replace(new RegExp("mailto:", "ig"), "");
                var attendeePartstat = item[1].partstat.toUpperCase();
                var attendeeRole = item[1].role;
                var attendeeCommonName = item[1].cn;
			    
                var attendeeData = { canEdit: false, status: attendeePartstat, email: attendeeEmail, name: attendeeCommonName || "", role: attendeeRole };
                jq("#attendeeTemplate").tmpl(attendeeData).appendTo(dlg.viewer.attendees);

                if (!replyDecision) {
                    jq(ASC.Mail.Accounts).each(function(j, account) {
                        if (attendeeEmail.toLowerCase() == account.email.toLowerCase()) {
                            replyDecision = attendeePartstat.toLowerCase();
                            return false;
                        }
                        return true;
                    });
                }
                
            });

            if(dlg.editor.attendeesList.hasClass("scrollable"))
                dlg.viewer.attendees.addClass("scrollable");
            else
                dlg.viewer.attendees.removeClass("scrollable");

            dlg.editor.replybuttons.hide().find(".reply-radio").removeAttr("checked");
            
            if (_canEdit && replyDecision && _eventObj.status != 2 && !_eventObj.source.isSubscription) {
                dlg.editor.replybuttons.find(".reply-radio." + replyDecision).prop("checked", true);
                dlg.editor.replybuttons.show();
                replyDecisionSettings.sendReply = true;
            }

        } else {
            dlg.viewer.attendees.empty().parent().hide();
            dlg.editor.replybuttons.hide().find(".reply-radio").removeAttr("checked");
            replyDecisionSettings.sendReply = false;
        }

        dlg.editor.users.ShareUsersSelector("set", _eventObj.permissions, true);
            
        if (_eventObj.permissions && _eventObj.permissions.users && _eventObj.permissions.users.length) {
            var data = jq.extend(true, {}, _eventObj.permissions.data);

            jq(data.items).each(function(index, item) {
                item.canEdit = false;
            });

            dlg.viewer.users.empty().parent().show();
            jq("#sharingUserTemplate").tmpl(data).appendTo(dlg.viewer.users);

            if(dlg.editor.usersList.hasClass("scrollable"))
                dlg.viewer.users.addClass("scrollable");
            else
                dlg.viewer.users.removeClass("scrollable");

        } else {
            dlg.viewer.users.empty().parent().hide();
        }

        if (_eventObj.allDay == true) {
            dlg.editor.allday.prop("checked", true);
            dlg.viewer.allday.addClass("yes");
        } else {
            dlg.editor.allday.prop("checked", false);
            dlg.viewer.allday.removeClass("yes");
        }

        dlg.editor.from.css("color", "").css("border-color", "");
        dlg.editor.from_t.css("color", "").css("border-color", "");
            
        if (_eventObj.start instanceof Date) {
            dlg.editor.from.val(formatDate(_eventObj.start, calendar.options.eventEditor.dateFormat));
            dlg.editor.from_t.val(_eventObj.allDay ? "" : formatDate(_eventObj.start, calendar.options.eventEditor.timeFormat));
        } else {
            dlg.editor.from.val("");
            dlg.editor.from_t.val("");
        }
            
        dlg.viewer.from.text(dlg.editor.from.val());
        dlg.viewer.from_t.text(dlg.editor.from_t.val());

        dlg.editor.to.css("color", "").css("border-color", "");
        dlg.editor.to_t.css("color", "").css("border-color", "");
            
        if (_eventObj.end instanceof Date) {
            dlg.editor.to.val(formatDate(_eventObj.end, calendar.options.eventEditor.dateFormat));
            dlg.editor.to_t.val(_eventObj.allDay ? "" : formatDate(_eventObj.end, calendar.options.eventEditor.timeFormat));
        } else {
            if (_eventObj.allDay && _eventObj.start instanceof Date) {
                dlg.editor.to.val(formatDate(_eventObj.start, calendar.options.eventEditor.dateFormat));
            } else {
                dlg.editor.to.val("");
                dlg.editor.to_t.val("");
            }
        }
            
        dlg.viewer.to.text(dlg.editor.to.val());
        dlg.viewer.to_t.text(dlg.editor.to_t.val());
			

        if (_eventObj.start instanceof Date) {
            dlg.viewer.from_t.text(_eventObj.allDay ? "" : formatDate(_eventObj.start, calendar.options.axisFormat));
        } else {
            dlg.viewer.from_t.text("");
        }
			
        if (_eventObj.end instanceof Date) {
            dlg.viewer.to_t.text(_eventObj.allDay ? "" : formatDate(_eventObj.end, calendar.options.axisFormat));
        } else {
            dlg.viewer.to_t.text("");
        }

        if (!dlg.viewer.to.text())
            dlg.viewer.to.parent(".wrapper").hide();
        else
            dlg.viewer.to.parent(".wrapper").show();

        var defaultSource = _getDefaultSource(isNewEvent ? _eventObj.newSourceId : null);
        var calSource = sourceIsValid ? _eventObj.source : defaultSource;

        if (isOrganizer && calSource.isSubscription) {
            _dialog.find(".editor .attendees").hide();
        } else {
            if (!dlg.editor.attendeesInputContainer.hasClass("display-none") ||
                !dlg.editor.attendeesList.hasClass("display-none") ||
                !dlg.editor.attendeesNoAccount.hasClass("display-none")) {
                _dialog.find(".editor .attendees").show();
            } else {
                _dialog.find(".editor .attendees").hide();
            }
        }

        dlg.editor.repeat.text(function() {
            if (!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.Never) && 
				!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.EveryDay) &&
				!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.EveryWeek) &&
				!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.EveryMonth) &&
				!_eventObj.repeatRule.Equals(ASC.Api.iCal.RecurrenceRule.EveryYear)) {
                _openSettings.call(_this);
                _settings.removeClass("hidden");
            }
				
            return _getRepeatLabel.call(_this, _eventObj.repeatRule);
        });
            
        dlg.editor.alert.text(sourceIsValid ? _getAlertLabel.call(_this, _eventObj.alert.type) : _getAlertLabel.call(_this, kAlertDefault));

        if (canChangeAlert) {
            _dialog.find(".repeat-alert").show();
        } else {
            dlg.editor.repeat.text(ASC.Api.iCal.RecurrenceRule.Never);
            dlg.editor.alert.text(kAlertNever);
            _dialog.find(".repeat-alert").hide();
        }
            
        dlg.viewer.repeat.text(dlg.editor.repeat.text());
        dlg.viewer.alert.text(dlg.editor.alert.text());

        var options = '';
        var calT;
        var calVal;
        var calColor;
        if (_canChangeSource) {
            for (var i = 0; i < sources.length; ++i) {
                if(!isNewEvent && sources[i].isSubscription)
                    continue;
                if (sources[i].isTodo == 1) continue;

                if ((sources[i].objectId != undefined) && (sources[i].isEditable || !sources[i].isSubscription)) {
                    calT = htmlEscape(sources[i].title);
                    options += '<option value="' + htmlEscape(sources[i].objectId) + '" ' +
							'title="' + calT + '">' +
							'&nbsp;&nbsp;&nbsp;&nbsp;' +  // for select elem padding does not work in safari
							calT + '</option>';
                }
            }
            calVal = calSource.objectId;
            calColor = calSource.backgroundColor;
            dlg.editor.calendar.removeAttr("disabled");
        } else {
            calVal = sourceIsValid ?
					_eventObj.source.objectId : -1;
            calColor = sourceIsValid ?
					_eventObj.source.backgroundColor : calendar.options.eventBackgroundColor;
            calT = htmlEscape(sourceIsValid ? _eventObj.source.title : "");
            options = '<option value="' + htmlEscape(calVal) + '" title="' + calT + '">' +
					'&nbsp;&nbsp;&nbsp;&nbsp;' +      // for select elem padding does not work in safari
					calT + '</option>';
			dlg.editor.calendar.attr("disabled", "disabled");

			var wrapper = $(dlg.viewer.calendar[0].parentElement)
			if (wrapper) {
				var halfwidth = $(wrapper)[0].parentElement;
				if (halfwidth) {
					$(halfwidth).addClass("readonly")
				}
			}

        }

        dlg.editor.calendar.html(options);
        dlg.editor.calendar.val(calVal);
        dlg.viewer.calendar.text(sourceIsValid ?_eventObj.source.title : dlg.editor.calendar.find("option:selected").text());

        calColor = htmlEscape(calColor);
        dlg.editor.calendar_b.css("background", calColor);
        dlg.viewer.calendar_b.css("background", calColor);

        dlg.editor.description.val(_eventObj.description || "");
        dlg.viewer.description.text(_eventObj.description || "");

        if (_eventObj.sourceId == "users_birthdays" || !_eventObj.description) {
            dlg.viewer.description.parent().hide();
        } else {
            dlg.viewer.description.parent().show();
        }

        _handleAllDayClick.call(_this);
       
        return true;
    }



    function _getDefaultSource(objectId) {
        var sources = calendar.getEventSources();
        var validSources = [];

        for (var i = 0; i < sources.length; ++i) {
            if (fcUtil.objectIsValid(sources[i]) && (fcUtil.objectIsEditable(sources[i]) || !sources[i].isSubscription) && sources[i].isTodo != 1) {
                validSources.push(sources[i]);
            }
        }

        for (var j = 0; j < validSources.length; ++j) {
            if (objectId) {
                if (validSources[j].objectId == objectId)
                    return validSources[j];
            } else {
                if (!validSources[j].isHidden)
                    return validSources[j];
            }
        }

        return validSources.length ? validSources[0] : null;
    }

    function _createEvent(startDate, endDate, allDay) {
        changeTimeFrom = null;
        var evt = {
            title:           "",
            description:     "",
            allDay:          allDay,
            start:           startDate,
            end:             endDate,
            repeatRule:      ASC.Api.iCal.RecurrenceRule.Never,
            alert:           {type:kAlertDefault},
            isShared:        false,
            permissions:     {users:[]},
            attendees:       []
        };
        
        if (ASC.Mail.DefaultAccount) {
            var organizerObj = new ICAL.Property("organizer");
            organizerObj.setParameter("cn", ASC.Mail.DefaultAccount.name);
            organizerObj.setValue("mailto:" + ASC.Mail.DefaultAccount.email);
            evt.organizer = organizerObj.jCal;
        }

        return evt;
    }

    function _deleteEvent(deleteType, eventDate) {
        if (!_canDelete ||
		    !fcUtil.objectIsValid(_eventObj) ||
		    !fcUtil.objectIsValid(_eventObj.source)) {return;}

        if (_eventObj.sourceId && _eventObj.uniqueId && (_eventObj.repeatRule.Freq == ASC.Api.iCal.Frequency.Never || confirmSettings.selectedDeleteMode == deleteMode.allSeries))
            _sendNotification(_eventObj.sourceId, _eventObj.uniqueId, confirmSettings.notificationType.cancel, deleteEvt);
        else
            deleteEvt();

        function deleteEvt() {
            var id = _eventObj._id;
            calendar.trigger("editEvent", _this,
                $.extend(
                    { action: kEventDeleteAction, sourceId: _eventObj.source.objectId, type: deleteType, date: eventDate },
                    _eventObj),
                function(response) {
                    if (!response.result) {
                        return;
                    }
                    calendar.removeEvents(id);

                    //
                    if (response.event != undefined) {
                        if (!response.event.length) {
                            if (_eventObj.repeatRule.Freq != ASC.Api.iCal.Frequency.Never && confirmSettings.selectedDeleteMode != deleteMode.allSeries) {
                                _sendNotification(_eventObj.sourceId, _eventObj.uniqueId, confirmSettings.notificationType.cancel, function() {
                                    ASC.CalendarController.RemoveEvent(id, deleteMode.allSeries, eventDate);
                                });
                            }
                            return;
                        }
                        //
                        var sources = calendar.getEventSources();
                        var j = 0;
                        while (j < response.event.length) {
                            _setEventSource(response.event[j], sources);
                            j++;
                        }
                        calendar.addEvents(response.event);

                        if (_eventObj.repeatRule.Freq != ASC.Api.iCal.Frequency.Never && confirmSettings.selectedDeleteMode != deleteMode.allSeries) {
                            confirmSettings.notificationUsers.updatedGuests = confirmSettings.notificationUsers.removedGuests;
                            _sendNotification(_eventObj.sourceId, _eventObj.uniqueId, confirmSettings.notificationType.update);
                        }
                    }
                });
        }
    }

    function _unsubscribeEvent() {
        if (!_canUnsubscribe ||
		    !fcUtil.objectIsValid(_eventObj) ||
		    !fcUtil.objectIsValid(_eventObj.source)) {return;}
        //
        var id = _eventObj._id;
        calendar.trigger("editEvent", _this,
				$.extend(
						{action: kEventUnsubscribeAction, sourceId: _eventObj.source.objectId},
						_eventObj),
				function(response) {
				    if (!response.result) {return;}
				    _close.call(_this, false);
				    calendar.removeEvents(id);
				});
    }

    function _setEventSource(event, sources) {
        if (event.sourceId == undefined) {
            event.sourceId = event.newSourceId;
        }
        if (!event.source && event.sourceId != undefined) {
            for (var i = 0; i < sources.length; ++i) {
                if (sources[i].objectId != undefined && sources[i].objectId == event.sourceId) {
                    event.source = sources[i];
                    break;
                }
            }
        }
    }

    function _updateEvent(isCancel) {
        if (!_canEdit) {return;}
		
        if(isCancel)
        {
            calendar.trigger("editEvent", _this, $.extend( {action: kEventCancelAction, sourceId: _eventObj.source.objectId}, _eventObj), function(response){} );
            return;
        }

        if (_eventObj.source) {
            _disableDialogBtns(true);

            if (_eventObj.sourceId && _eventObj.uniqueId)
                _sendNotification(_eventObj.sourceId, _eventObj.uniqueId, confirmSettings.notificationType.cancel, editEvt);
            else
                editEvt();

        } else {
            _disableDialogBtns(true);
            var id = _eventObj._id;
            // create new event
            calendar.trigger("editEvent", _this,
					$.extend({action: kEventAddAction}, _eventObj),
					function(response) {
					    _disableDialogBtns(false);
					    if (!response.result) {return;}
					    _closeDialog.call(_this);
					    calendar.removeEvents(id);
					    if (response.event.length < 1) {return;}
					    //
					    var sources = calendar.getEventSources();
					    calendar.removeEvents(response.event[0].objectId);
					    for (var j = 0; j < response.event.length; ++j) {
					        _setEventSource(response.event[j], sources);
					    }
					    calendar.addEvents(response.event);
					    
					    _sendNotification(response.event[0].sourceId, response.event[0].uniqueId, confirmSettings.notificationType.request);
					});
        }
        
        function editEvt() {
            calendar.trigger("editEvent", _this,
                $.extend(
                    { action: kEventChangeAction, sourceId: _eventObj.source.objectId },
                    _eventObj),
                function(response) {
                    _disableDialogBtns(false);
                    if (!response.result) {
                        return;
                    }
                    _closeDialog.call(_this);
                    if (response.event.length < 1) {
                        return;
                    }
                    //
                    var sources = calendar.getEventSources();
                    calendar.removeEvents(response.event[0].objectId);
                    for (var j = 0; j < response.event.length; ++j) {
                        _setEventSource(response.event[j], sources);
                    }
                    calendar.addEvents(response.event);

                    _sendNotification(response.event[0].sourceId, response.event[0].uniqueId, confirmSettings.notificationType.request);
                    _sendNotification(response.event[0].sourceId, response.event[0].uniqueId, confirmSettings.notificationType.update);
                    _sendReply(response.event[0].sourceId, response.event[0].uniqueId);
                });
        }
    }

    // Public interface

    this.openEvent = function (canEdit, elem, event) {
        changeTimeFrom = null;
        _open.call(_this, canEdit ? "edit" : "view", elem, event);
    };

    this.addEvent = function(startDate, endDate, allDay) {
        // Protect from addind event in nonexistent category
        var defaultSource = _getDefaultSource();
        if (defaultSource == undefined) return;

        var ev;
        if (startDate != undefined) {
            // add event via clicking calendar cell
            ev = _createEvent(startDate, endDate, allDay);
            ev.textColor = defaultSource.textColor;
            ev.backgroundColor = defaultSource.backgroundColor;
            ev.borderColor = defaultSource.borderColor;
            calendar.renderEvent(ev);
            _open.call(_this, "edit", calendar.getView().getEventElement(ev), ev);
        } else {
            // add event via header menu
            var curDate = new Date();
            var targetDate = new Date(curDate.getFullYear(), curDate.getMonth(), curDate.getDate());
            ev = _createEvent(targetDate, targetDate, true);
            _open.call(_this, "edit", undefined, ev);
        }
    };

    this.isVisible = function() {
        return _dialog.is(":visible");
    };
}


fc.sourceNormalizers = [];
fc.sourceFetchers = [];

var ajaxDefaults = {
	dataType: 'json',
	cache: false
};

var eventGUID = 1;


function EventManager(options, _sources) {
	var t = this;


	if (localStorageManager.getItem("showTodosInCalendar") == undefined) {
	    localStorageManager.setItem("showTodosInCalendar", true);
	}

	// exports
	t.isFetchNeeded = isFetchNeeded;
	t.fetchEvents = fetchEvents;
	t.addEvents = addEvents;
	t.addEventSource = addEventSource;
	t.removeEventSource = removeEventSource;
	t.getEventSources = getEventSources;
	t.cleanEmptyEventSources = cleanEmptyEventSources;
	t.updateEvent = updateEvent;
	t.renderEvent = renderEvent;
	t.removeEvents = removeEvents;
	t.clientEvents = clientEvents;
	t.normalizeEvent = normalizeEvent;
    t.getCache = getCache;
    t.showTodosInCalendar = localStorageManager.getItem("showTodosInCalendar");
    t.hideCompletedTodos = localStorageManager.getItem("hideCompletedTodos");


	// imports
	var trigger = t.trigger;
	var getView = t.getView;
	var reportEvents = t.reportEvents;


	// locals
	var stickySource = { events: [], todos: [] };
	var sources = [ stickySource ];
	var rangeStart, rangeEnd;
	var currentFetchID = 0;
	var pendingSourceCnt = 0;
	var loadingLevel = 0;
	var cache = [];



	/* Fetching
	-----------------------------------------------------------------------------*/


	function isFetchNeeded(start, end) {
		return !rangeStart || start < rangeStart || end > rangeEnd;
	}

	function fetchEvents(start, end, resolve) {
		rangeStart = function(d) {
			var d1 = cloneDate(d, true);
			if (d1.getDate() != 1) {d1.setDate(1);}
			return d1;
		}(start);
		rangeEnd = function(d) {
			var d1 = cloneDate(d, true);
			var m1 = d1.getMonth();
			var y1 = d1.getFullYear();
			m1 = d1.getDate() == 1 ? m1 : (m1 < 11 ? m1 + 1 : (++y1, 0));
			d1.setFullYear(y1, m1, 1);
			return d1;
		}(end);
		cache = [];
		var fetchID = ++currentFetchID;

		trigger("loadEventSources", t, rangeStart, rangeEnd, function(response){
			if (response.result && $.isArray(response.eventSources)) {
				sources.splice(1, sources.length - 1);
				for (var i = 0; i < response.eventSources.length; ++i) {
					_addEventSource(response.eventSources[i]);
				}
				var len = sources.length;
				pendingSourceCnt = len;
				for (i = 0; i < len; ++i) {
					fetchEventSource(sources[i], fetchID);
				}
				t.rerenderCategories(resolve);
			}
		});
	}

	function fetchEventSource(source, fetchID) {
		_fetchEventSource(source, function(response) {
			if (fetchID == currentFetchID) {
				if (response.events) {
				    for (var i = 0; i < response.events.length; i++) {
				        response.events[i].source = source;
				        response.events[i].false = true;
				        normalizeEvent(response.events[i]);
				    }
				    cache = cache.concat(response.events);
				}
				if (response.todos && t.showTodosInCalendar) {
				    var todos = [];
				    for (var j = 0; j < response.todos.length; j++) {
				        if (!t.hideCompletedTodos || (t.hideCompletedTodos && response.todos[j].completed == false)) {
				            response.todos[j].source = source;
				            response.todos[j].isTodo = true;
				            todos.push(response.todos[j]);
				            normalizeEvent(todos[todos.length-1]);
				        } 
				    }
				    if (source.objectId != undefined) cache = cache.concat(todos);

				}
				pendingSourceCnt--;
				if (!pendingSourceCnt) {
					reportEvents(cache);
				}
			}
		});
	}

	function _fetchEventSource(source, callback) {
		var i;
		var fetchers = fc.sourceFetchers;
		var res;
		for (i=0; i<fetchers.length; i++) {
			res = fetchers[i](source, rangeStart, rangeEnd, callback);
			if (res === true) {
				// the fetcher is in charge. made its own async request
				return;
			}
			else if (typeof res == 'object') {
				// the fetcher returned a new source. process it
				_fetchEventSource(res, callback);
				return;
			}
		}
		var events = source.events;
		var todos = source.todos;
		
		if (events) {
			if ($.isFunction(events)) {
				pushLoading();
				events(cloneDate(rangeStart), cloneDate(rangeEnd), function(events) {
				    callback({ result: true, events: events, todos: todos });
					popLoading();
				});
			}
			else if ($.isArray(events)) {
				callback({result:true,events:events, todos: todos});
			}
			else {
			    callback({ result: false, events: [], todos: [] });
			}
		}
		else {
			var url = source.url;
			if (url) {
				var success = source.success;
				var error = source.error;
				var complete = source.complete;
				var data = $.extend({}, source.data || {});
				var startParam = firstDefined(source.startParam, options.startParam);
				var endParam = firstDefined(source.endParam, options.endParam);
				if (startParam) {
					data[startParam] = Math.round(+rangeStart / 1000);
				}
				if (endParam) {
					data[endParam] = Math.round(+rangeEnd / 1000);
				}
				pushLoading();
				$.ajax($.extend({}, ajaxDefaults, source, {
					data: data,
					success: function(events) {
						events = events || [];
						var res = applyAll(success, this, arguments);
						if ($.isArray(res)) {
							events = res;
						}
						callback({result:true,events:events});
					},
					error: function() {
						applyAll(error, this, arguments);
						callback({result:false,events:[]});
					},
					complete: function() {
						applyAll(complete, this, arguments);
						popLoading();
					}
				}));
			}else{
				callback({result:false,events:[]});
			}
		}
	}


	/* Sources
	-----------------------------------------------------------------------------*/

	function addEventSource(source) {
		source = _addEventSource(source);
		if (source) {
			pendingSourceCnt++;
			fetchEventSource(source, currentFetchID); // will eventually call reportEvents
		}
	}

	function _addEventSource(source) {
		if ($.isFunction(source) || $.isArray(source)) {
			source = {events: source};
		}
		else if (typeof source == 'string') {
			source = {url: source};
		}
		if (typeof source == 'object') {
			normalizeSource(source);
			sources.push(source);
			return source;
		}
		return undefined;
	}

	function removeEventSource(source) {
		sources = $.grep(sources, function(src) {
			return !isSourcesEqual(src, source);
		});
		// remove all client events from that source
		cache = $.grep(cache, function(e) {
			return !isSourcesEqual(e.source, source);
		});
		reportEvents(cache);
		t.rerenderCategories();
	}

	function getCache() {
	    return cache;
	}
	function getEventSources() {
		return sources;
	}
	function cleanEmptyEventSources() {
	    for (var i = 0; i < sources.length; i++) {
	        if (sources[i].isTodo != 1) {
	            sources[i].todos = [];
	        }
	    }
	}


	/* Manipulation
	-----------------------------------------------------------------------------*/

	function updateEvent(event) { // update an existing event
		normalizeEvent(event);
		var i, len = cache.length, e,
			defaultEventEnd = getView().defaultEventEnd, // getView???
			startDelta = event.start - event._start,
			endDelta = event.end ?
				(event.end - (event._end || defaultEventEnd(event))) // event._end would be null if event.end
				: 0;                                                 // was null and event was just resized
		for (i=0; i<len; i++) {
			e = cache[i];
			if (e._id == event._id && e != event) {
				e.start = new Date(+e.start + startDelta);
				if (event.end) {
					if (e.end) {
						e.end = new Date(+e.end + endDelta);
					}else{
						e.end = new Date(+defaultEventEnd(e) + endDelta);
					}
				}else{
					e.end = null;
				}
				if (event.title           != undefined) {e.title = event.title;}
				if (event.url             != undefined) {e.url = event.url;}
				if (event.allDay          != undefined) {e.allDay = event.allDay;}
				if (event.className       != undefined) {e.className = event.className;}
				if (event.editable        != undefined) {e.editable = event.editable;}
				if (event.color           != undefined) {e.color = event.color;}
				if (event.backgroundColor != undefined) {e.backgroundColor = event.backgroundColor;}
				if (event.borderColor     != undefined) {e.borderColor = event.borderColor;}
				if (event.textColor       != undefined) {e.textColor = event.textColor;}
				if (event.description     != undefined) {e.description = event.description;}
				if (event.repeat          != undefined) {e.repeat = event.repeat;}
				if (event.alert           != undefined) {e.alert = event.alert;}
				if (event.objectId        != undefined) {e.objectId = event.objectId;}
				if (event.sourceId        != undefined) {e.sourceId = event.sourceId;}
				if (event.source          != undefined) {e.source = event.source;}
				if (event.isShared        != undefined) {e.isShared = event.isShared;}
				if (event.permissions     != undefined) {e.permissions = event.permissions;}
				if (event.owner           != undefined) {e.owner = event.owner;}
				normalizeEvent(e);
			}
		}
		reportEvents(cache);
	}

	function renderEvent(event, stick) {
		normalizeEvent(event);
		if (!event.source) {
			if (stick) {
				stickySource.events.push(event);
				event.source = stickySource;
			}
			cache.push(event);
		}
		reportEvents(cache);
	}

	function addEvents(events) {
		var i=0;
		while(i < events.length) {
			normalizeEvent(events[i]);
			i++;
		}
		cache = cache.concat(events);
		reportEvents(cache);
	}

	function removeEvents(filter) {
		if (!filter) { // remove all
			cache = [];
			// clear all array sources
			for (var i=0; i<sources.length; i++) {
				if ($.isArray(sources[i].events)) {
					sources[i].events = [];
				}
			}
		}else{
			if (!$.isFunction(filter)) { // an event ID
				var id = filter + '';
				filter = function(e) {
					return e._id == id;
				};
			}
			cache = $.grep(cache, filter, true);
			// remove events from array sources
			for (i=0; i<sources.length; i++) {
				if ($.isArray(sources[i].events)) {
					sources[i].events = $.grep(sources[i].events, filter, true);
				}
			}
		}
		reportEvents(cache);
	}

	function clientEvents(filter) {
		if ($.isFunction(filter)) {
			return $.grep(cache, filter);
		}
		else if (filter) { // an event ID
			filter += '';
			return $.grep(cache, function(e) {
				return e._id == filter;
			});
		}
		return cache; // else, return all
	}
	
	/* Loading State
	-----------------------------------------------------------------------------*/

	function pushLoading() {
		if (!loadingLevel++) {
			trigger('loading', null, true);
		}
	}

	function popLoading() {
		if (!--loadingLevel) {
			trigger('loading', null, false);
		}
	}


	/* Event Normalization
	-----------------------------------------------------------------------------*/

	function normalizeEvent(event) {
		var source = event.source || {};
		var ignoreTimezone = firstDefined(source.ignoreTimezone, options.ignoreTimezone);
		event._id = event.objectId !== undefined ?
				event.objectId + '' :
				(event._id !== undefined ?
						event._id :
						(event.id !== undefined ? event.id + '' : '_fc' + eventGUID++));
		if (event.date) {
			if (!event.start) {
				event.start = event.date;
			}
			delete event.date;
		}
		event._start = cloneDate(event.start = parseDate(event.start, ignoreTimezone));
		event.end = parseDate(event.end, ignoreTimezone);
		if (event.end && event.end <= event.start) {
			event.end = null;
		}
		event._end = event.end ? cloneDate(event.end) : null;
		if (event.allDay === undefined) {
		    if (event.isTodo) {
		        event.allDay = true;
		    } else {
		        event.allDay = firstDefined(source.allDayDefault, options.allDayDefault);
		    }
			
		}
		if (event.className) {
			if (typeof event.className == 'string') {
				event.className = event.className.split(/\s+/);
			}
		}else{
			event.className = [];
		}
		// TODO: if there is no start date, return false to indicate an invalid event
	}


	/* Utils
	------------------------------------------------------------------------------*/

	function normalizeSource(source) {
		if (source.backgroundColor && !source.borderColor) {
			var bg = fcUtil.parseCssColor(source.backgroundColor);
			var bor;
			if (bg) {
				bor = fcUtil.changeColorLightness(bg.r, bg.g, bg.b, options.eventBg2BorderRatio);
				source.borderColor = bor.color;
			}
		}

		if (source.className) {
			// TODO: repeat code, same code for event classNames
			if (typeof source.className == 'string') {
				source.className = source.className.split(/\s+/);
			}
		}else{
			source.className = [];
		}
		var normalizers = fc.sourceNormalizers;
		for (var i=0; i<normalizers.length; i++) {
			normalizers[i](source);
		}
	}

	function isSourcesEqual(source1, source2) {
		return source1 && source2 && getSourcePrimitive(source1) == getSourcePrimitive(source2);
	}

	function getSourcePrimitive(source) {
		return ((typeof source == 'object') ? (source.events || source.url) : '') || source;
	}

}



fc.addDays = addDays;
fc.cloneDate = cloneDate;
fc.parseDate = parseDate;
fc.parseISO8601 = parseISO8601;
fc.parseTime = parseTime;
fc.formatDate = formatDate;
fc.formatDates = formatDates;



/* Date Math
-----------------------------------------------------------------------------*/

var dayIDs = ['sun', 'mon', 'tue', 'wed', 'thu', 'fri', 'sat'],
	DAY_MS = 86400000,
	HOUR_MS = 3600000,
	MINUTE_MS = 60000;


function addYears(d, n, keepTime) {
	d.setFullYear(d.getFullYear() + n);
	if (!keepTime) {
		clearTime(d);
	}
	return d;
}

function addMonths(d, n, keepTime) { // prevents day overflow/underflow
	if (+d) { // prevent infinite looping on invalid dates
		var m = d.getMonth() + n,
			check = cloneDate(d);
		check.setDate(1);
		check.setMonth(m);
		d.setMonth(m);
		if (!keepTime) {
			clearTime(d);
		}
		while (d.getMonth() != check.getMonth()) {
			d.setDate(d.getDate() + (d < check ? 1 : -1));
		}
	}
	return d;
}

function addDays(d, n, keepTime) { // deals with daylight savings
	if (+d) {
		var dd = d.getDate() + n,
			check = cloneDate(d);
		check.setHours(9); // set to middle of day
		check.setDate(dd);
		d.setDate(dd);
		if (!keepTime) {
			clearTime(d);
		}
		fixDate(d, check);
	}
	return d;
}

function fixDate(d, check) { // force d to be on check's YMD, for daylight savings purposes
	if (+d) { // prevent infinite looping on invalid dates
		while (d.getDate() != check.getDate()) {
			d.setTime(+d + (d < check ? 1 : -1) * HOUR_MS);
		}
	}
}

function addMinutes(d, n) {
	d.setMinutes(d.getMinutes() + n);
	return d;
}

function clearTime(d) {
	d.setHours(0, 0, 0, 0);
	return d;
}

function cloneDate(d, dontKeepTime) {
	if (dontKeepTime) {
		return clearTime(new Date(+d));
	}
	return new Date(+d);
}

function zeroDate() { // returns a Date with time 00:00:00 and dateOfMonth=1
	var i=0, d;
	do {
		d = new Date(1970, i++, 1);
	} while (d.getHours()); // != 0
	return d;
}

function skipWeekend(date, inc, excl) {
	inc = inc || 1;
	while (!date.getDay() || (excl && date.getDay()==1 || !excl && date.getDay()==6)) {
		addDays(date, inc);
	}
	return date;
}

function dayDiff(d1, d2) { // d1 - d2
	return Math.round((cloneDate(d1, true) - cloneDate(d2, true)) / DAY_MS);
}

function setYMD(date, y, m, d) {
	if (y !== undefined && y != date.getFullYear()) {
		date.setDate(1);
		date.setMonth(0);
		date.setFullYear(y);
	}
	if (m !== undefined && m != date.getMonth()) {
		date.setDate(1);
		date.setMonth(m);
	}
	if (d !== undefined) {
		date.setDate(d);
	}
}


/* ICS Parsing
-----------------------------------------------------------------------------*/

function parseIcs(icsFormat) {
    if (!icsFormat) return null;

    try {
        var jCalData = window.ICAL.parse(icsFormat);

        var comp = new window.ICAL.Component(jCalData);
        if (comp.name !== "vcalendar")
            return null;

        var vevent = comp.getFirstSubcomponent("vevent");
        if (!vevent)
            return null;

        var event = new window.ICAL.Event(vevent);

        var organizer = vevent.getFirstProperty("organizer");

        var attendees = [];

        jq.each(event.attendees, function(index, attendee) {
            attendees.push(attendee.jCal);
        });

        return {
            location: event.location,
            attendees: attendees,
            organizer: organizer == null ? null : organizer.jCal
        };
    } catch(e) {
        console.error(e);
        return null;
    }
}


/* Date Parsing
-----------------------------------------------------------------------------*/

function parseDate(s, ignoreTimezone) { // ignoreTimezone defaults to true
	if (typeof s == 'object') { // already a Date object
		return s;
	}
	if (typeof s == 'number') { // a UNIX timestamp
		return new Date(s * 1000);
	}
	if (typeof s == 'string') {
		if (s.match(/^\d+(\.\d+)?$/)) { // a UNIX timestamp
			return new Date(parseFloat(s) * 1000);
		}
		if (ignoreTimezone === undefined) {
			ignoreTimezone = true;
		}
		return parseISO8601(s, ignoreTimezone) || (isNaN(Date.parse(s)) ? null : new Date(s));
	}
	// TODO: never return invalid dates (like from new Date(<string>)), return null instead
	return null;
}

function parseISO8601(s, ignoreTimezone) { // ignoreTimezone defaults to false
	// derived from http://delete.me.uk/2005/03/iso8601.html
	// TODO: for a know glitch/feature, read tests/issue_206_parseDate_dst.html
	var m = s.match(/^([0-9]{4})(-([0-9]{2})(-([0-9]{2})([T ]([0-9]{2}):([0-9]{2})(:([0-9]{2})(\.([0-9]+))?)?(Z|(([-+])([0-9]{2})(:?([0-9]{2}))?))?)?)?)?$/);
	if (!m) {
		return null;
	}
	var date = new Date(m[1], 0, 1, 1); // TODO hack for Russia TZ Daylight Time. remove for chrome 29 ???
	if (ignoreTimezone || !m[14]) {
		var check = new Date(m[1], 0, 1, 9, 0);
		if (m[3]) {
			date.setMonth(m[3] - 1);
			check.setMonth(m[3] - 1);
		}
		if (m[5]) {
			date.setDate(m[5]);
			check.setDate(m[5]);
		}
		fixDate(date, check);
		if (m[7]) {
			date.setHours(m[7]);
		}
		if (m[8]) {
			date.setMinutes(m[8]);
		}
		if (m[10]) {
			date.setSeconds(m[10]);
		}
		if (m[12]) {
			date.setMilliseconds(Number("0." + m[12]) * 1000);
		}
		fixDate(date, check);
	}else{
		date.setUTCFullYear(
			m[1],
			m[3] ? m[3] - 1 : 0,
			m[5] || 1
		);
		date.setUTCHours(
			m[7] || 0,
			m[8] || 0,
			m[10] || 0,
			m[12] ? Number("0." + m[12]) * 1000 : 0
		);
		var offset = Number(m[16]) * 60 + (m[18] ? Number(m[18]) : 0);
		offset *= m[15] == '-' ? 1 : -1;
		date = new Date(+date + (offset * 60 * 1000));
	}
	return date;
}

function parseTime(s) { // returns minutes since start of day
	if (typeof s == 'number') { // an hour
		return s * 60;
	}
	if (typeof s == 'object') { // a Date object
		return s.getHours() * 60 + s.getMinutes();
	}
	var m = s.match(/(\d+)(?::(\d+))?\s*(\w+)?/);
	if (m) {
		var h = parseInt(m[1], 10);
		if (m[3]) {
			h %= 12;
			if (m[3].toLowerCase().charAt(0) == 'p') {
				h += 12;
			}
		}
		return h * 60 + (m[2] ? parseInt(m[2], 10) : 0);
	}
	return 0;
}



/* Date Formatting
-----------------------------------------------------------------------------*/
// TODO: use same function formatDate(date, [date2], format, [options])

function formatDate(date, format, options) {
	return formatDates(date, null, format, options);
}

function formatDates(date1, date2, format, options) {
	options = options || defaults;
	var date = date1,
		otherDate = date2,
		i, len = format.length, c,
		i2, formatter,
		res = '', subres;
	for (i=0; i<len; i++) {
		c = format.charAt(i);
		if (c == "'") {
			for (i2=i+1; i2<len; i2++) {
				if (format.charAt(i2) == "'") {
					if (date) {
						if (i2 == i+1) {
							res += "'";
						}else{
							res += format.substring(i+1, i2);
						}
						i = i2;
					}
					break;
				}
			}
		}
		else if (c == '(') {
			for (i2=i+1; i2<len; i2++) {
				if (format.charAt(i2) == ')') {
					subres = formatDate(date, format.substring(i+1, i2), options);
					if (parseInt(subres.replace(/\D/, ''), 10)) {
						res += subres;
					}
					i = i2;
					break;
				}
			}
		}
		else if (c == '[') {
			for (i2=i+1; i2<len; i2++) {
				if (format.charAt(i2) == ']') {
					var subformat = format.substring(i+1, i2);
					subres = formatDate(date, subformat, options);
					if (subres != formatDate(otherDate, subformat, options)) {
						res += subres;
					}
					i = i2;
					break;
				}
			}
		}
		else if (c == '{') {
			date = date2;
			otherDate = date1;
		}
		else if (c == '}') {
			date = date1;
			otherDate = date2;
		}
		else {
			for (i2=len; i2>i; i2--) {
				formatter = dateFormatters[format.substring(i, i2)];
				if (formatter) {
					if (date) {
						res += formatter(date, options);
					}
					i = i2 - 1;
					break;
				}
			}
			if (i2 == i) {
				if (date) {
					res += c;
				}
			}
		}
	}
	return res;
}


var dateFormatters = {
	s  : function(d)  {return d.getSeconds();},
	ss  : function(d)  {return zeroPad(d.getSeconds());},
	m  : function(d)  {return d.getMinutes();},
	mm  : function(d)  {return zeroPad(d.getMinutes());},
	h  : function(d)  {return d.getHours() % 12 || 12;},
	hh  : function(d)  {return zeroPad(d.getHours() % 12 || 12);},
	H  : function(d)  {return d.getHours();},
	HH  : function(d)  {return zeroPad(d.getHours());},
	d  : function(d)  {return d.getDate();},
	dd  : function(d)  {return zeroPad(d.getDate());},
	ddd  : function(d,o)  {return o.dayNamesShort[d.getDay()];},
	dddd: function(d,o)  {return o.dayNames[d.getDay()];},
	M  : function(d)  {return d.getMonth() + 1;},
	MM  : function(d)  {return zeroPad(d.getMonth() + 1);},
	MMM  : function(d,o)  {return o.monthNamesShort[d.getMonth()];},
	MMMM: function(d,o)  {return o.monthNames[d.getMonth()];},
	yy  : function(d)  {return (d.getFullYear()+'').substring(2);},
	yyyy: function(d)  {return d.getFullYear();},
	t  : function(d)  {return d.getHours() < 12 ? 'a' : 'p';},
	tt  : function(d)  {return d.getHours() < 12 ? 'am' : 'pm';},
	T  : function(d)  {return d.getHours() < 12 ? 'A' : 'P';},
	TT  : function(d)  {return d.getHours() < 12 ? 'AM' : 'PM';},
	u  : function(d)  {return formatDate(d, "yyyy-MM-dd'T'HH:mm:ss'Z'");},
	S  : function(d)  {
		var date = d.getDate();
		if (date > 10 && date < 20) {
			return 'th';
		}
		return ['st', 'nd', 'rd'][date%10-1] || 'th';
	}
};



fc.applyAll = applyAll;


/* Event Date Math
-----------------------------------------------------------------------------*/

function exclEndDay(event) {
	if (event.end) {
		return _exclEndDay(event.end, event.allDay);
	}else{
		return addDays(cloneDate(event.start), 1);
	}
}

function _exclEndDay(end, allDay) {
	end = cloneDate(end);
	return allDay || end.getHours() || end.getMinutes() ? addDays(end, 1) : clearTime(end);
}

function segCmp(a, b) {
	return a.event.source === undefined ? -1 : (b.event.source === undefined ? 1 : (
			(b.msLength - a.msLength) * 100 + (a.event.start - b.event.start)));
}

function segsCollide(seg1, seg2) {
	return seg1.end > seg2.start && seg1.start < seg2.end;
}



/* Event Sorting
-----------------------------------------------------------------------------*/

// event rendering utilities
function sliceSegs(events, fixEventEndFunc, start, end) {
	var segs = [],
		i, len=events.length, event,
		eventStart, eventEnd,
		segStart, segEnd,
		isStart, isEnd;
	for (i=0; i<len; i++) {
		event = events[i];
		eventStart = event.start;
		eventEnd = fixEventEndFunc(event);
		if ((!event.source || !event.source.isHidden) && eventEnd > start && eventStart < end && ASC.Api.iCal.RecurrenceRule.isVisibleYearlyEvent(event)) {
			if (eventStart < start) {
				segStart = cloneDate(start);
				isStart = false;
			}else{
				segStart = eventStart;
				isStart = true;
			}
			if (eventEnd > end) {
				segEnd = cloneDate(end);
				isEnd = false;
			}else{
				segEnd = eventEnd;
				isEnd = true;
			}
			segs.push({
				event: event,
				start: segStart,
				end: segEnd,
				isStart: isStart,
				isEnd: isEnd,
				msLength: segEnd - segStart
			});
		}
	}
	return segs.sort(segCmp);
}

// event rendering calculation utilities
function stackSegs(segs) {
	var levels = [],
		i, len = segs.length, seg,
		j, collide, k;
	for (i=0; i<len; i++) {
		seg = segs[i];
		j = 0; // the level index where seg should belong
		while (true) {
			collide = false;
			if (levels[j]) {
				for (k=0; k<levels[j].length; k++) {
					if (segsCollide(levels[j][k], seg)) {
						collide = true;
						break;
					}
				}
			}
			if (collide) {
				j++;
			}else{
				break;
			}
		}
		if (levels[j]) {
			levels[j].push(seg);
		}else{
			levels[j] = [seg];
		}
	}
	return levels;
}



/* Event Element Binding
-----------------------------------------------------------------------------*/

function lazySegBind(container, segs, bindHandlers) {
	container.unbind('mouseover').mouseover(function(ev) {
		var e = ev.target, i, seg;
		while (e && e != this) {
			if (e._fci !== undefined) break;
			e = e.parentNode;
		}
		if ((i = e._fci) !== undefined) {
			e._fci = undefined;
			seg = segs[i];
			bindHandlers(seg.event, seg.element, seg);
			$(ev.target).trigger(ev);
		}
		ev.stopPropagation();
	});
}



/* Element Dimensions
-----------------------------------------------------------------------------*/

function setOuterWidth(element, width, includeMargins) {
	for (var i=0, e; i<element.length; i++) {
		e = $(element[i]);
		e.width(Math.max(0, width - hsides(e, includeMargins)));
	}
}

function setOuterHeight(element, height, includeMargins) {
	for (var i=0, e; i<element.length; i++) {
		e = $(element[i]);
		e.height(Math.max(0, height - vsides(e, includeMargins)));
	}
}

// TODO: curCSS has been deprecated (jQuery 1.4.3 - 10/16/2010)

function hsides(element, includeMargins) {
    var styles = window.getComputedStyle(element[0]);
    return hpadding(styles) + hborders(styles) + (includeMargins ? hmargins(styles) : 0);
}

function hpadding(styles) {
    return (parseFloat(styles.paddingLeft) || 0) + (parseFloat(styles.paddingRight) || 0);
}

function hmargins(styles) {
    return (parseFloat(styles.marginLeft) || 0) + (parseFloat(styles.marginRight) || 0);
}

function hborders(styles) {
    return (parseFloat(styles.borderLeftWidth) || 0) + (parseFloat(styles.borderRightWidth) || 0);
}

function vsides(element, includeMargins) {
    var styles = window.getComputedStyle(element[0]);
    return vpadding(styles) + vborders(styles) + (includeMargins ? vmargins(styles) : 0);
}

function vpadding(styles) {
    return (parseFloat(styles.paddingTop) || 0) + (parseFloat(styles.paddingBottom) || 0);
}

function vmargins(styles) {
    return (parseFloat(styles.marginTop) || 0) + (parseFloat(styles.marginBottom) || 0);
}

function vborders(styles) {
    return (parseFloat(styles.borderTopWidth) || 0) + (parseFloat(styles.borderBottomWidth) || 0);
}

function setMinHeight(element, height, bSetHeight) {
	height = (typeof height == 'number' ? height + 'px' : height);
	element.each(function(i, _element) {
		_element.style.cssText += ';min-height:' + height +
				(bSetHeight ? ';height:' + height: '') +
				';_height:' + height + ';';
	});
}

function setMinWidth(element, width) {
	width = (typeof width == 'number' ? width + 'px' : width);
	element.each(function(i, _element) {
		_element.style.cssText += ';min-width:' + width + ';width:' + width + ';_width:' + width + ';';
	});
}


/* Misc Utils
-----------------------------------------------------------------------------*/


//TODO: arraySlice
//TODO: isFunction, grep ?


function noop() { }

function cmp(a, b) {
	return a - b;
}

function arrayMax(a) {
	return Math.max.apply(Math, a);
}

function zeroPad(n) {
	return (n < 10 ? '0' : '') + n;
}

function smartProperty(obj, name) { // get a camel-cased/namespaced property of an object
	if (obj[name] !== undefined) {
		return obj[name];
	}
	var parts = name.split(/(?=[A-Z])/),
		i=parts.length-1, res;
	for (; i>=0; i--) {
		res = obj[parts[i].toLowerCase()];
		if (res !== undefined) {
			return res;
		}
	}
	return obj[''];
}

function htmlEscape(s) {
	if (typeof s !== "string") {
		return s;
	}
	return s.replace(/&(\s|[^#\w]|\w+(?:[^;\w]|$))/g, '&amp;$1')
		.replace(/</g, '&lt;')
		.replace(/>/g, '&gt;')
		.replace(/'/g, '&#039;')
		.replace(/"/g, '&quot;')
		.replace(/\n/g, '<br/>');
}

function cssKey(_element) {
	return _element.id + '/' + _element.className + '/' + _element.style.cssText.replace(/(^|;)\s*(top|left|width|height)\s*:[^;]*/ig, '');
}

function disableTextSelection(element) {
	element
		.attr('unselectable', 'on')
		.css('MozUserSelect', 'none')
		.bind('selectstart.ui', function() {return false;});
}

/*
function enableTextSelection(element) {
	element
		.attr('unselectable', 'off')
		.css('MozUserSelect', '')
		.unbind('selectstart.ui');
}
*/

function markFirstLast(e) {
	e.children()
		.removeClass('fc-first fc-last')
		.filter(':first-child')
			.addClass('fc-first')
		.end()
		.filter(':last-child')
			.addClass('fc-last');
}

function setDayID(cell, date) {
	cell.className = cell.className.replace(/^fc-\w*/, 'fc-' + dayIDs[date.getDay()]);
	// TODO: make a way that doesn't rely on order of classes
}

function getSkinCss(event, opt, curView) {
	var source = event.source || {};
	var eventColor = event.color;
	var sourceColor = source.color;
	var optionColor = opt('eventColor');
	var isShort = curView === 'month' && !event.allDay &&
			(!(event.end instanceof Date) ||
			event.end.getFullYear() == event.start.getFullYear() &&
			event.end.getMonth() == event.start.getMonth() &&
			((event.end.getDate() == event.start.getDate()) || (event.end.getDate() - event.start.getDate() == 1 && event.end.getHours() == 0 && event.end.getMinutes() == 0)));

	var backgroundColor = isShort ? 'transparent' : (
		event.backgroundColor ||
		eventColor ||
		source.backgroundColor ||
		sourceColor ||
		opt('eventBackgroundColor') ||
		optionColor);
	var borderColor = isShort ? 'transparent' : (
		event.backgroundColor ||
		eventColor ||
		source.backgroundColor ||
		sourceColor ||
		opt('eventBorderColor') ||
		optionColor);
	var textColor = isShort ? opt('eventTextColor') : (
		event.textColor ||
		source.textColor ||
		opt('eventTextColor'));
	var statements = [];
	if (backgroundColor) {
		statements.push('background-color:' + htmlEscape(backgroundColor));
	}
	if (borderColor) {
		statements.push('border-color:' + htmlEscape(borderColor));
	}
	if (textColor) {
		statements.push('color:' + htmlEscape(textColor));
	}
	return statements.join(';');
}

function applyAll(functions, thisObj, args) {
	if ($.isFunction(functions)) {
		functions = [ functions ];
	}
	if (functions) {
		var i;
		var ret;
		for (i=0; i<functions.length; i++) {
			ret = functions[i].apply(thisObj, args) || ret;
		}
		return ret;
	}
	return false;
}

function firstDefined() {
	for (var i=0; i<arguments.length; i++) {
		if (arguments[i] !== undefined) {
			return arguments[i];
		}
	}
	return false;
}



function TitleRender(calendar) {

	var _renders = {
		month: _renderMonthTitle,
		week:  _renderWeekTitle,
		day:   _renderDayTitle
	};


	var formatDate = calendar.formatDate;
	var formatDates = calendar.formatDates;


	this.render = function(viewName, startDate, endDate) {
		var method = smartProperty(_renders, viewName);
		return method ? method.call(this, startDate, endDate) : $('<span/>');
	};


	function _parseFormatString(fmt) {
		var result = " " + fmt + " ";
		result = result.replace(/([^A-Za-z])(M+)([^A-Za-z])/g, "$1'<span class=\"month\">'$2'</span>'$3");
		result = result.replace(/([^A-Za-z])(y+)([^A-Za-z])/g, "$1'<span class=\"year\">'$2'</span>'$3");
		return result.replace(/^\s+|\s+$/g, "");
	}

	function _renderMonthTitle(startDate, endDate) {
		var fmt = _parseFormatString(calendar.options.titleFormat.month);
		//var m = startDate.getMonth();
		//var prev1 = (m + 12 - 1) % 12;
		//var next1 = (m + 1) % 12;
		var title = $(
				'<span>' +
					//'<span class="link">' +
					//	'<span class="inner left">' +
					//		calendar.options.monthNames[prev1] +
					//	'</span>' +
					//'</span>' +
					'<span class="title-text">' +
						'<span class="h2">' +
							formatDate(startDate, fmt) +
						'</span>' +
					'</span>' +
					//'<span class="link">' +
					//	'<span class="inner right">' +
					//		calendar.options.monthNames[next1] +
					//	'</span>' +
					//'</span>' +
				'</span>');
		fcMenus.buildTitleMenus(calendar, title);
		//title.find(".link .left").click(function() {calendar.prev(-1);});
		//title.find(".link .right").click(function() {calendar.next(+1);});
		return title;
	}

	function _renderWeekTitle(startDate, endDate) {
		var wf = calendar.options.titleFormat.week.replace(/MMMM/g, "MMM");
		var fmt = wf + "[, yyyy]{ '&#8212;' " + wf + ", " + _parseFormatString("yyyy}");
		var tc = formatDates(startDate, endDate, fmt);
		//var fmt2 = wf + "{ '&#8212;' " + wf + "}";
		//var tl = formatDates(addDays(cloneDate(startDate), -7), addDays(cloneDate(endDate), -7), fmt2);
		//var tr = formatDates(addDays(cloneDate(startDate), +7), addDays(cloneDate(endDate), +7), fmt2);
		var title = $(
				'<span>' +
					//'<span class="link">' +
					//	'<span class="inner left">' + tl + '</span>' +
					//'</span>' +
					'<span class="title-text">' +
						'<span class="h2">' + tc + '</span>' +
					'</span>' +
					//'<span class="link">' +
					//	'<span class="inner right">' + tr + '</span>' +
					//'</span>' +
				'</span>');
		fcMenus.buildTitleMenus(calendar, title);
		//title.find(".link .left").click(function() {calendar.prev(-1);});
		//title.find(".link .right").click(function() {calendar.next(+1);});
		return title;
	}

	function _renderDayTitle(startDate, endDate) {
		var tmp = (calendar.options.titleFormat.day.indexOf("ddd") < 0 &&
				calendar.options.titleFormat.day.indexOf("dddd") < 0 ? "dddd, " : "") +
				calendar.options.titleFormat.day
		var fmt = tmp.replace(/dddd/g, "ddd");
		var title = $(
				'<span>' +
					//'<span class="link">' +
					//	'<span class="inner left">' +
					//		formatDate(addDays(cloneDate(startDate), -2), 'd') + '</span>' +
					//'</span>' +
					//'<span class="link">' +
					//	'<span class="inner left">' +
					//		formatDate(addDays(cloneDate(startDate), -1), 'd') + '</span>' +
					//'</span>' +
					'<span class="title-text">' +
						'<span class="h2">' +
							formatDate(startDate, fmt) +
						'</span>' +
					'</span>' +
					//'<span class="link">' +
					//	'<span class="inner right">' +
					//		formatDate(addDays(cloneDate(startDate), +1), 'd') + '</span>' +
					//'</span>' +
					//'<span class="link">' +
					//	'<span class="inner right">' +
					//		formatDate(addDays(cloneDate(startDate), +2), 'd') + '</span>' +
					//'</span>' +
				'</span>');
		//fcMenus.buildTitleMenus(calendar, title);

		//var left = title.find(".link .left");
		//left.eq(0).parent().click(function() {calendar.prev(-2);});
		//left.eq(1).parent().click(function() {calendar.prev(-1);});

		//var right = title.find(".link .right");
		//right.eq(0).parent().click(function() {calendar.next(+1);});
		//right.eq(1).parent().click(function() {calendar.next(+2);});

		title.find(".title-text .h2").click(function() {
			fcDatepicker.open(calendar, this,
					function(dp) {dp.datepicker("setDate", calendar.getDate());},
					function(elem, dp) {this.close();calendar.gotoDate(dp.datepicker("getDate"));});
		});

		return title;
	}

}



fcViews.month = MonthView;

function MonthView(element, calendar) {
	var t = this;


	// exports
	t.render = render;


	// imports
	BasicView.call(t, element, calendar, 'month');
	var opt = t.opt;
	var renderBasic = t.renderBasic;
	var formatDate = calendar.formatDate;



	function render(date, delta) {
		if (delta) {
			addMonths(date, delta);
			date.setDate(1);
		}
		var start = cloneDate(date, true);
		start.setDate(1);
		var end = addMonths(cloneDate(start), 1);
		var visStart = cloneDate(start);
		var visEnd = cloneDate(end);
		var firstDay = opt('firstDay');
		var nwe = opt('weekends') ? 0 : 1;
		if (nwe) {
			skipWeekend(visStart);
			skipWeekend(visEnd, -1, true);
		}
		addDays(visStart, -((visStart.getDay() - Math.max(firstDay, nwe) + 7) % 7));
		addDays(visEnd, (7 - visEnd.getDay() + Math.max(firstDay, nwe)) % 7);
		var rowCnt = Math.round((visEnd - visStart) / (DAY_MS * 7));
		if (opt('weekMode') == 'fixed') {
			addDays(visEnd, (6 - rowCnt) * 7);
			rowCnt = 6;
		}
		var titleRender = new TitleRender(calendar);
		t.title = titleRender.render(t.name, start, end);
		t.start = start;
		t.end = end;
		t.visStart = visStart;
		t.visEnd = visEnd;
		renderBasic(6, rowCnt, nwe ? 5 : 7, true);
	}


	this._afterRenderDaySegs = function(segs) {
		//--diabled big day numbers--
		//t.markEmptyCells(segs);
	};

}



fcViews.basicWeek = BasicWeekView;

function BasicWeekView(element, calendar) {
	var t = this;


	// exports
	t.render = render;


	// imports
	BasicView.call(t, element, calendar, 'basicWeek');
	var opt = t.opt;
	var renderBasic = t.renderBasic;
	var formatDates = calendar.formatDates;



	function render(date, delta) {
		if (delta) {
			addDays(date, delta * 7);
		}
		var start = addDays(cloneDate(date), -((date.getDay() - opt('firstDay') + 7) % 7));
		var end = addDays(cloneDate(start), 7);
		var visStart = cloneDate(start);
		var visEnd = cloneDate(end);
		var weekends = opt('weekends');
		if (!weekends) {
			skipWeekend(visStart);
			skipWeekend(visEnd, -1, true);
		}
		t.title = formatDates(
			visStart,
			addDays(cloneDate(visEnd), -1),
			opt('titleFormat')
		);
		t.start = start;
		t.end = end;
		t.visStart = visStart;
		t.visEnd = visEnd;
		renderBasic(1, 1, weekends ? 7 : 5, false);
	}


}



fcViews.basicDay = BasicDayView;

//TODO: when calendar's date starts out on a weekend, shouldn't happen

function BasicDayView(element, calendar) {
	var t = this;


	// exports
	t.render = render;


	// imports
	BasicView.call(t, element, calendar, 'basicDay');
	var opt = t.opt;
	var renderBasic = t.renderBasic;
	var formatDate = calendar.formatDate;



	function render(date, delta) {
		if (delta) {
			addDays(date, delta);
			if (!opt('weekends')) {
				skipWeekend(date, delta < 0 ? -1 : 1);
			}
		}
		t.title = formatDate(date, opt('titleFormat'));
		t.start = t.visStart = cloneDate(date, true);
		t.end = t.visEnd = addDays(cloneDate(t.start), 1);
		renderBasic(1, 1, 1, false);
	}


}



function BasicView(element, calendar, viewName) {
	var t = this;


	// exports
	t.renderBasic = renderBasic;
	t.setHeight = setHeight;
	t.setWidth = setWidth;
	t.renderDayOverlay = renderDayOverlay;
	t.defaultSelectionEnd = defaultSelectionEnd;
	t.renderSelection = renderSelection;
	t.clearSelection = clearSelection;
	t.reportDayClick = reportDayClick; // for selection (kinda hacky)
	t.dragStart = dragStart;
	t.dragStop = dragStop;
	t.defaultEventEnd = defaultEventEnd;
	t.getHoverListener = function() {return hoverListener;};
	t.colContentLeft = colContentLeft;
	t.colContentRight = colContentRight;
	t.dayOfWeekCol = dayOfWeekCol;
	t.dateCell = dateCell;
	t.cellDate = cellDate;
	t.cellIsAllDay = function() {return true;};
	t.allDayRow = allDayRow;
	t.allDayBounds = allDayBounds;
	t.getRowCnt = function() {return rowCnt;};
	t.getColCnt = function() {return colCnt;};
	t.getColWidth = function() {return colWidth;};
	t.getDaySegmentContainer = function() {return daySegmentContainer;};
	t.getCellBounds = getCellBounds;
	//--diabled big day numbers--
	//t.markEmptyCells = markEmptyCells;


	// imports
	View.call(t, element, calendar, viewName);
	OverlayManager.call(t);
	SelectionManager.call(t);
	BasicEventRenderer.call(t);
	var opt = t.opt;
	var trigger = t.trigger;
	var clearEvents = t.clearEvents;
	var renderOverlay = t.renderOverlay;
	var clearOverlays = t.clearOverlays;
	var daySelectionMousedown = t.daySelectionMousedown;
	var formatDate = calendar.formatDate;


	// locals

	var head;
	var headCells;
	var body;
	var bodyRows;
	var bodyCells;
	var bodyFirstCells;
	var bodyCellTopInners;
	var daySegmentContainer;

	var viewWidth;
	var viewHeight;
	var colWidth;

	var rowCnt, colCnt;
	var coordinateGrid;
	var hoverListener;
	var colContentPositions;

	var rtl, dis, dit;
	var firstDay;
	var nwe;
	var tm;
	var colFormat;



	/* Rendering
	------------------------------------------------------------*/


	disableTextSelection(element.addClass('fc-grid'));


	function renderBasic(maxr, r, c, showNumbers) {
		rowCnt = r;
		colCnt = c;
		updateOptions();
		var firstTime = !body;
		if (firstTime) {
			buildSkeleton(maxr, showNumbers);
		}else{
			clearEvents();
		}
		updateCells(firstTime);
	}


	function updateOptions() {
		rtl = opt('isRTL');
		if (rtl) {
			dis = -1;
			dit = colCnt - 1;
		}else{
			dis = 1;
			dit = 0;
		}
		firstDay = opt('firstDay');
		nwe = opt('weekends') ? 0 : 1;
		tm = opt('theme') ? 'ui' : 'fc';
		colFormat = opt('columnFormat');
	}


	function buildSkeleton(maxRowCnt, showNumbers) {
		var s;
		var headerClass = tm + "-widget-header";
		var contentClass = tm + "-widget-content";
		var i, j;
		var table;

		s =
			"<table class='fc-border-separate' style='width:100%' cellspacing='0'>" +
			"<thead>" +
			"<tr>";
		for (i=0; i<colCnt; i++) {
			s +=
				"<th class='fc- " + headerClass + "'/>"; // need fc- for setDayID
		}
		s +=
			"</tr>" +
			"</thead>" +
			"<tbody>";
		for (i=0; i<maxRowCnt; i++) {
			s +=
				"<tr class='fc-week" + i + "'>";
			for (j=0; j<colCnt; j++) {
				s +=
					"<td class='fc- " + contentClass + " fc-day" + (i*colCnt+j) + "'>" + // need fc- for setDayID
						"<div>" +
							(showNumbers ?
								"<div class='fc-day-number'/>" :
								''
								) +
							"<div class='fc-day-content'>" +
								"<div style='position:relative;height:100%;'>&nbsp;</div>" +
							"</div>" +
							//--diabled big day numbers--
							//"<div class='fc-day-number-big'/>" +
						"</div>" +
					"</td>";
			}
			s +=
				"</tr>";
		}
		s +=
			"</tbody>" +
			"</table>";
		table = $(s).appendTo(element);

		head = table.find('thead');
		headCells = head.find('th');
		body = table.find('tbody');
		bodyRows = body.find('tr');
		bodyCells = body.find('td');
		bodyFirstCells = bodyCells.filter(':first-child');
		bodyCellTopInners = bodyRows.eq(0).find('div.fc-day-content div');

		markFirstLast(head.add(head.find('tr'))); // marks first+last tr/th's
		markFirstLast(bodyRows); // marks first+last td's
		bodyRows.eq(0).addClass('fc-first'); // fc-last is done in updateCells

		dayBind(bodyCells);

		daySegmentContainer =
			$("<div style='position:absolute;z-index:8;top:0;left:0'/>")
				.appendTo(element);
	}


	function updateCells(firstTime) {
		var dowDirty = firstTime || rowCnt == 1; // could the cells' day-of-weeks need updating?
		var month = t.start.getMonth();
		var today = clearTime(new Date());
		var date;
		var todayIndex;
		var topRow;
		var leftCell;

		if (dowDirty) {
			headCells.each(function(i, cell) {
				date = indexDate(i);
				cell.innerHTML = formatDate(date, colFormat);
			    setDayID(cell, date);
			});
		}

		bodyCells.each(function(i, cell) {
			date = indexDate(i);
			if (date.getMonth() == month) {
				cell.classList.remove('fc-other-month');
			} else {
				cell.classList.add('fc-other-month');
			}
			if (+date == +today) {
				cell.classList.add(tm + '-state-highlight', 'fc-today');
				leftCell = cell.previousSibling;
				if (leftCell) {
					leftCell.classList.add("fc-today-left");
				}
				topRow = cell.parentElement.previousSibling;
				if (topRow) {
					topRow.childNodes[cell.cellIndex].classList.add("fc-today-top");
				}
				todayIndex = i;
			} else {
				cell.classList.remove(tm + '-state-highlight', 'fc-today', 'fc-today-left', 'fc-today-top');
			}
			cell.querySelector(".fc-day-number").innerHTML ='<span>' + date.getDate() + '</span>';
			if (dowDirty) {
			    setDayID(cell, date);
			}
		});

		bodyRows.each(function(i, row) {
			if (i < rowCnt) {
				if (i == rowCnt-1) {
					row.classList.add('fc-last');
				}else{
					row.classList.remove('fc-last');
				}
			}else{
				row.style.display = 'none';
			}
		});

		if (todayIndex < 7) {
			bodyCellTopInners = bodyRows.eq(1).find('div.fc-day-content div');
		}

		bodyCells.find(".fc-day-number > span").mousedown(
				function (ev) {
					if (ev.which != 1) {return;}
					var d = calendar.getDate();
					calendar.gotoDate(d.getFullYear(), d.getMonth(), $(this).text());
					calendar.changeView("agendaDay");
				});
	}


	//--diabled big day numbers--
	//function markEmptyCells(segs) {
	//	bodyCells.each(function(i, _cell) {
	//		var cell = $(_cell);
	//		var r = Math.floor(i / colCnt);
	//		var c = i % colCnt;
	//		for (var j = 0; j < segs.length; ++j) {
	//			if (segs[j].row === r &&
	//			    segs[j].startCol <= c && c < segs[j].endCol) {
	//				cell.removeClass("no-events");
	//				return;
	//			}
	//		}
	//		cell.addClass("no-events");
	//	});
	//}


	function setHeight(height) {
		viewHeight = height;

		var sizeManager = ASC.CalendarSizeManager.cache;
		var bodyHeight = viewHeight - sizeManager.fcMonthContentTheadHeight;
		var rowHeight;
		var rowHeightLast;

		if (opt('weekMode') == 'variable') {
			rowHeight = rowHeightLast = Math.floor(bodyHeight / (rowCnt==1 ? 2 : 6));
		}else{
			rowHeight = Math.floor(bodyHeight / rowCnt);
			rowHeightLast = bodyHeight - rowHeight * (rowCnt-1);
		}

		bodyCells.each(function(i, cell) {
			var h = (Math.floor(i / 7) == rowCnt - 1 ? rowHeightLast : rowHeight) - sizeManager.fcMonthContentCellVSides + 1;
			var innerDiv = cell.querySelector("div");
			innerDiv.style.cssText = ';min-height:' + h + 'px;height:' + h + 'px;';
			var dc = innerDiv.querySelector(".fc-day-content");
			h = h - sizeManager.fcMonthContentCellNumberHeight - sizeManager.fcMonthContentCellNumberVSides;
			dc.style.cssText += ';min-height:' + h + 'px;height:' + h + 'px;';
		});

	}


	function setWidth(width) {
		viewWidth = width;
		colContentPositions.clear();
		colWidth = Math.floor(viewWidth / colCnt) - hsides($(headCells[0]));
		for (var i = 0, e; i < headCells.length; i++) {
		    headCells[i].style.width = colWidth + "px";
		}
	}



	/* Day clicking and binding
	-----------------------------------------------------------*/


	function dayBind(days) {
		days.click(dayClick)
			.mousedown(daySelectionMousedown);
	}


	function dayClick(ev) {
		if (!opt('selectable')) { // if selectable, SelectionManager will worry about dayClick
			var index = parseInt(this.className.match(/fc\-day(\d+)/)[1]); // TODO: maybe use .data
			var date = indexDate(index);
			trigger('dayClick', this, date, true, ev);
		}
	}



	/* Semi-transparent Overlay Helpers
	------------------------------------------------------*/


	function renderDayOverlay(overlayStart, overlayEnd, refreshCoordinateGrid) { // overlayEnd is exclusive
		if (refreshCoordinateGrid) {
			coordinateGrid.build();
		}
		var rowStart = cloneDate(t.visStart);
		var rowEnd = addDays(cloneDate(rowStart), colCnt);
		for (var i=0; i<rowCnt; i++) {
			var stretchStart = new Date(Math.max(rowStart, overlayStart));
			var stretchEnd = new Date(Math.min(rowEnd, overlayEnd));
			if (stretchStart < stretchEnd) {
				var colStart, colEnd;
				if (rtl) {
					colStart = dayDiff(stretchEnd, rowStart)*dis+dit+1;
					colEnd = dayDiff(stretchStart, rowStart)*dis+dit+1;
				}else{
					colStart = dayDiff(stretchStart, rowStart);
					colEnd = dayDiff(stretchEnd, rowStart);
				}
				dayBind(
					renderCellOverlay(i, colStart, i, colEnd-1)
				);
			}
			addDays(rowStart, 7);
			addDays(rowEnd, 7);
		}
	}


	function renderCellOverlay(row0, col0, row1, col1) { // row1,col1 is inclusive
		var rect = coordinateGrid.rect(row0, col0, row1, col1, element);
		return renderOverlay(rect, element);
	}



	/* Selection
	-----------------------------------------------------------------------*/


	function defaultSelectionEnd(startDate, allDay) {
		return cloneDate(startDate);
	}


	function renderSelection(startDate, endDate, allDay) {
		renderDayOverlay(startDate, addDays(cloneDate(endDate), 1), true); // rebuild every time???
	}


	function clearSelection() {
		clearOverlays();
	}


	function reportDayClick(date, allDay, ev) {
		var cell = dateCell(date);
		var _element = bodyCells[cell.row*colCnt + cell.col];
		trigger('dayClick', _element, date, allDay, ev);
	}



	/* External Dragging
	-----------------------------------------------------------------------*/


	function dragStart(_dragElement, ev, ui) {
		hoverListener.start(function(cell) {
			clearOverlays();
			if (cell) {
				renderCellOverlay(cell.row, cell.col, cell.row, cell.col);
			}
		}, ev);
	}


	function dragStop(_dragElement, ev, ui) {
		var cell = hoverListener.stop();
		clearOverlays();
		if (cell) {
			var d = cellDate(cell);
			trigger('drop', _dragElement, d, true, ev, ui);
		}
	}



	/* Utilities
	--------------------------------------------------------*/


	function defaultEventEnd(event) {
		return cloneDate(event.start);
	}


	coordinateGrid = new CoordinateGrid(function(rows, cols) {
		var e, n, p;
		headCells.each(function(i, _e) {
			e = $(_e);
			n = e.offset().left;
			if (i) {
				p[1] = n;
			}
			p = [n];
			cols[i] = p;
		});
		p[1] = n + e.outerWidth();
		bodyRows.each(function(i, _e) {
			if (i < rowCnt) {
				e = $(_e);
				n = e.offset().top;
				if (i) {
					p[1] = n;
				}
				p = [n];
				rows[i] = p;
			}
		});
		p[1] = n + e.outerHeight();
	});


	hoverListener = new HoverListener(coordinateGrid);


	colContentPositions = new ElementsPositionCache(function(col) {
		return bodyCellTopInners.eq(col);
	});


	function colContentLeft(col) {
		return colContentPositions.left(col);
	}


	function colContentRight(col) {
		return colContentPositions.right(col);
	}




	function dateCell(date) {
		return {
			row: Math.floor(dayDiff(date, t.visStart) / 7),
			col: dayOfWeekCol(date.getDay())
		};
	}


	function cellDate(cell) {
		return _cellDate(cell.row, cell.col);
	}


	function _cellDate(row, col) {
		return addDays(cloneDate(t.visStart), row*7 + col*dis+dit);
		// what about weekends in middle of week?
	}


	function indexDate(index) {
		return _cellDate(Math.floor(index/colCnt), index%colCnt);
	}


	function dayOfWeekCol(dayOfWeek) {
		return ((dayOfWeek - Math.max(firstDay, nwe) + colCnt) % colCnt) * dis + dit;
	}




	function allDayRow(i) {
		return bodyRows.eq(i);
	}


	function allDayBounds(i) {
		return {
			left: 0,
			right: viewWidth
		};
	}




	function getCellBounds(row, col) {
		var c = bodyRows.eq(row).find("td:eq(" + col + ")");
		if (c.length > 0) {
			return {
					left:   c[0].offsetLeft,
					top:    c[0].offsetTop,
					right:  c[0].offsetLeft + c[0].offsetWidth,
					bottom: c[0].offsetTop + c[0].offsetHeight};
		}
		return {top:0, right:0, bottom:0, left:0};
	}


}

function BasicEventRenderer() {
	var t = this;


	// exports
	t.renderEvents = renderEvents;
	t.compileDaySegs = compileSegs; // for DayEventRenderer
	t.clearEvents = clearEvents;
	t.bindDaySeg = bindDaySeg;


	// imports
	DayEventRenderer.call(t);
	var opt = t.opt;
	var trigger = t.trigger;
	//var setOverflowHidden = t.setOverflowHidden;
	var isEventDraggable = t.isEventDraggable;
	var isEventResizable = t.isEventResizable;
	var reportEvents = t.reportEvents;
	var reportEventClear = t.reportEventClear;
	var eventElementHandlers = t.eventElementHandlers;
	var showEvents = t.showEvents;
	var hideEvents = t.hideEvents;
	var eventDrop = t.eventDrop;
	var getDaySegmentContainer = t.getDaySegmentContainer;
	var getHoverListener = t.getHoverListener;
	var renderDayOverlay = t.renderDayOverlay;
	var clearOverlays = t.clearOverlays;
	var getRowCnt = t.getRowCnt;
	var getColCnt = t.getColCnt;
	var renderDaySegs = t.renderDaySegs;
	var resizableDayEvent = t.resizableDayEvent;



	/* Rendering
	--------------------------------------------------------------------*/


	function renderEvents(events, modifiedEventId) {
		reportEvents(events);
		renderDaySegs(compileSegs(events), modifiedEventId);
	}


	function clearEvents() {
		reportEventClear();
		var node = getDaySegmentContainer()[0];
		var last;
		while (last = node.lastChild)
			node.removeChild(last);
	}


	function compileSegs(events) {
		var rowCnt = getRowCnt(),
			colCnt = getColCnt(),
			d1 = cloneDate(t.visStart),
			d2 = addDays(cloneDate(d1), colCnt),
			i, row,
			j, level,
			k, seg,
			segs=[];
		for (i=0; i<rowCnt; i++) {
			row = stackSegs(sliceSegs(events, exclEndDay, d1, d2));
			for (j=0; j<row.length; j++) {
				level = row[j];
				for (k=0; k<level.length; k++) {
					seg = level[k];
					seg.row = i;
					seg.level = j; // not needed anymore
					segs.push(seg);
				}
			}
			addDays(d1, 7);
			addDays(d2, 7);
		}
		return segs;
	}


	function bindDaySeg(event, eventElement, seg) {
		if (isEventDraggable(event)) {
			draggableDayEvent(event, eventElement);
		}
		if (seg.isEnd && isEventResizable(event)) {
			resizableDayEvent(event, eventElement, seg);
		}
		eventElementHandlers(event, eventElement);
			// needs to be after, because resizableDayEvent might stopImmediatePropagation on click
	}



	/* Dragging
	----------------------------------------------------------------------------*/


function draggableDayEvent(event, eventElement) {
		var hoverListener = getHoverListener();
		var dayDelta;
		eventElement.draggable({
			zIndex: 9,
			delay: 50,
			opacity: opt('dragOpacity'),
			revertDuration: opt('dragRevertDuration'),
			start: function(ev, ui) {
			
				trigger('eventDragStart', eventElement, event, ev, ui);
				hideEvents(event, eventElement);
				hoverListener.start(function(cell, origCell, rowDelta, colDelta) {
					eventElement.draggable('option', 'revert', !cell || !rowDelta && !colDelta);
					clearOverlays();
					if (cell) {
						//setOverflowHidden(true);
						dayDelta = rowDelta*7 + colDelta * (opt('isRTL') ? -1 : 1);
						renderDayOverlay(
							addDays(cloneDate(event.start), dayDelta),
							addDays(exclEndDay(event), dayDelta)
						);
					}else{
						//setOverflowHidden(false);
						dayDelta = 0;
					}
				}, ev, 'drag');
			},
			 stop: function(ev, ui) {
                    hoverListener.stop();
                    clearOverlays();
                    trigger('eventDragStop', eventElement, event, ev, ui);
                    if (dayDelta) {
                        eventDrop(this, event, dayDelta, 0, event.allDay, ev, ui);
                    } else {
                        eventElement.css('filter', ''); // clear IE opacity side-effects
                        showEvents(event, eventElement);
                    }
                    //setOverflowHidden(false);
                }
		});
	}


}


fcViews.agendaWeek = AgendaWeekView;

function AgendaWeekView(element, calendar) {
	var t = this;


	// imports
	AgendaView.call(t, element, calendar, 'agendaWeek');
	var opt = t.opt;
	var renderAgenda = t.renderAgenda;
	var formatDates = calendar.formatDates;



	this.render = function(date, delta) {
		if (delta) {
			addDays(date, delta * 7);
		}
		var start = addDays(cloneDate(date), -((date.getDay() - opt('firstDay') + 7) % 7));
		var end = addDays(cloneDate(start), 7);
		var visStart = cloneDate(start);
		var visEnd = cloneDate(end);
		var weekends = opt('weekends');
		if (!weekends) {
			skipWeekend(visStart);
			skipWeekend(visEnd, -1, true);
		}
		var titleRender = new TitleRender(calendar);
		t.title = titleRender.render(t.name, start, addDays(cloneDate(start), 6));
		t.start = start;
		t.end = end;
		t.visStart = visStart;
		t.visEnd = visEnd;
		renderAgenda(weekends ? 7 : 5);
	}

}



fcViews.agendaDay = AgendaDayView;

function AgendaDayView(element, calendar) {
	var t = this;


	// exports
	t.render = render;


	// imports
	AgendaView.call(t, element, calendar, 'agendaDay');
	var opt = t.opt;
	var renderAgenda = t.renderAgenda;
	var formatDate = calendar.formatDate;
	var getDayList = t.getDayList;



	function render(date, delta) {
		if (delta) {
			addDays(date, delta);
			if (!opt('weekends')) {
				skipWeekend(date, delta < 0 ? -1 : 1);
			}
		}
		var start = cloneDate(date, true);
		var end = addDays(cloneDate(start), 1);
		var titleRender = new TitleRender(calendar);
		t.title = titleRender.render(t.name, start, end);
		t.start = t.visStart = start;
		t.end = t.visEnd = end;
		renderAgenda(1, false);
		_renderDayList.call(t);
	}

	function _renderDayList() {
		var dayList = getDayList();
		if (!dayList) {return;}

		var textFormat = calendar.options.dayView.dateFormat;
		var alldayTitle = calendar.options.dayView.alldayTasksTitle;
		var todayTitle = calendar.options.dayView.todayTasksTitle;

		dayList.html(
				'<div class="big-date-box">' +
					'<div class="top-text">' +
							htmlEscape(formatDates(t.visStart, t.visEnd, textFormat.topText)) + '</div>' +
					'<div class="big-text">' +
							htmlEscape(formatDates(t.visStart, t.visEnd, textFormat.bigText)) + '</div>' +
					'<div class="bottom-text">' +
							htmlEscape(formatDates(t.visStart, t.visEnd, textFormat.bottomText)) + '</div>' +
					'<div class="today-label">' +
						'<div class="inner">' + opt('todayLabel') + '</div>' +
					'</div>' +
				'</div>' +
				'<div class="allday-box">' +
					'<div class="allday-tasks-title">' + htmlEscape(alldayTitle) + '</div>' +
					'<div class="allday-tasks-scroller">' +
						'<table class="allday-tasks-list" border="0" cellspacing="0" cellpadding="0"/>' +
					'</div>' +
				'</div>' +
				'<div class="tasks-title">' + htmlEscape(todayTitle) + '</div>' +
				'<div class="tasks-scroller">' +
					'<table class="tasks-list" border="0" cellspacing="0" cellpadding="0"/>' +
				'</div>');
	}

	this._onSetHeight = function(height) {
		var dayList = getDayList();
		if (!dayList) {return;}
		//
		var title = dayList.find(".tasks-title");
		var pos = title.position();
		pos.top += title.outerHeight(true);
		dayList.find(".tasks-scroller")
				.css("top", pos.top + "px")
				.css("height", (height - pos.top) + "px");
		//
		var dateBox = dayList.find(".big-date-box");
		dayList.find(".allday-box")
				.css("top", dateBox.position().top + "px")
				.css("height", dateBox.outerHeight(true) + "px");
		title = dayList.find(".allday-tasks-title");
		dayList.find(".allday-tasks-scroller")
				.css("top", (title.position().top + title.outerHeight(true)) + "px");
	};

	this._onSetWidth = function(width) {
		var dayList = getDayList();
		if (!dayList) {return;}
		//
		var title = dayList.find(".tasks-title");
		var pos = title.position();
		dayList.find(".tasks-scroller")
				.css("left", pos.left + "px")
				.css("width", title.outerWidth(true));
		//
		var dateBox = dayList.find(".big-date-box");
		var dateBoxWidth = dateBox.outerWidth(true);
		dayList.find(".allday-box")
				.css("left", (dateBox.position().left + dateBoxWidth) + "px")
				.css("width", (dayList.width() - dateBoxWidth) + "px");
	};

	this._afterRenderSlotSegs = function(segs) {
		var dayList = getDayList();
		if (!dayList) {return;}

		var segCnt = segs.length;
		var tasksList = dayList.find('table.tasks-list');
		var i;
		var event;
		var tbody = '';

		segs.sort(function(l,r) {
			var res =
					l.event.start < r.event.start ? -1 :
					(l.event.start > r.event.start ? 1 :
					(l.event.title.toLowerCase() < r.event.title.toLowerCase() ? -1 : 1));
			return res;
		});

		for (i = 0; i < segCnt; ++i) {
			event = segs[i].event;
			tbody +=
					'<tr>' +
						'<td class="task-img">' +
							'<span class="bullet" style="color:' + htmlEscape(event.source.isHidden ?
									calendar.options.categories.inactiveColor : event.source.backgroundColor) +
									';">' +
								htmlEscape(calendar.options.categories.itemBullet) + '&nbsp;</span>' + '</td>' +
						'<td class="task-title">' +
							'<div class="task-title">' + htmlEscape(event.title) + '</div>' +
							(event.description && event.description.length > 0 ?
									('<div class="task-note">' + htmlEscape(event.description) + '</div>') : '') + '</td>' +
						'<td class="task-time">' +
							'<div class="task-time">' +
								htmlEscape(formatDates(event.start, event.end || event.start,
										opt('taskListTimeFormat'))) + '</div>' + '</td>' +
					'</tr>';
		}

		tasksList.html('<tbody>' + tbody + '</tbody>');
		tasksList.find("tr:last").addClass("last");
	};

	this._afterRenderDaySegs = function(segs) {
		var dayList = getDayList();
		if (!dayList) {return;}

		var segCnt = segs.length;
		var tasksList = dayList.find('table.allday-tasks-list');
		var i;
		var event;
		var tbody = '';

		segs.sort(function(l,r) {
			var res =
					l.event.title.toLowerCase() < r.event.title.toLowerCase() ? -1 : 1;
			return res;
		});

		for (i = 0; i < segCnt; ++i) {
			event = segs[i].event;
			if (!event) {continue;}
			tbody +=
					'<tr>' +
						'<td class="task-img">' +
							'<span class="bullet" style="color:' + htmlEscape(event.source.isHidden ?
									calendar.options.categories.inactiveColor : event.source.backgroundColor) +
									';">' +
								htmlEscape(calendar.options.categories.itemBullet) + '&nbsp;</span>' + '</td>' +
						'<td class="task-title">' +
							'<div class="task-title">' + htmlEscape(event.title) + '</div>' + '</td>' +
					'</tr>';
		}

		tasksList.html('<tbody>' + tbody + '</tbody>');
		tasksList.find("tr:last").addClass("last");
	};

}



// TODO: make it work in quirks mode (event corners, all-day height)
// TODO: test liquid width, especially in IE6

function AgendaView(element, calendar, viewName) {
	var t = this;


	// exports
	t.renderAgenda = renderAgenda;
	t.setWidth = setWidth;
	t.setHeight = setHeight;
	t.beforeHide = beforeHide;
	t.afterShow = afterShow;
	t.defaultEventEnd = defaultEventEnd;
	t.timePosition = timePosition;
	t.dayOfWeekCol = dayOfWeekCol;
	t.dateCell = dateCell;
	t.cellDate = cellDate;
	t.cellIsAllDay = cellIsAllDay;
	t.allDayRow = getAllDayRow;
	t.allDayBounds = allDayBounds;
	t.getHoverListener = function() {return hoverListener;};
	t.colContentLeft = colContentLeft;
	t.colContentRight = colContentRight;
	t.getDaySegmentContainer = function() {return daySegmentContainer;};
	t.getSlotSegmentContainer = function() {return slotSegmentContainer;};
	t.getMinMinute = function() {return minMinute;};
	t.getMaxMinute = function() {return maxMinute;};
	t.getBodyContent = function() {return slotContent;}; // !!??
	t.getRowCnt = function() {return 1;};
	t.getColCnt = function() {return colCnt;};
	t.getColWidth = function() {return colWidth;};
	t.getSlotHeight = function() {return slotHeight;};
	t.defaultSelectionEnd = defaultSelectionEnd;
	t.renderDayOverlay = renderDayOverlay;
	t.renderSelection = renderSelection;
	t.clearSelection = clearSelection;
	t.reportDayClick = reportDayClick; // selection mousedown hack
	t.dragStart = dragStart;
	t.dragStop = dragStop;
	t.getDayList = function() {return dayList;};
	t.resetScroll = resetScroll;
    t.getCellBounds = getCellBounds;
    t.updateHeader = updateHeader;


	// imports
	View.call(t, element, calendar, viewName);
	OverlayManager.call(t);
	SelectionManager.call(t);
	AgendaEventRenderer.call(t);
	var opt = t.opt;
	var trigger = t.trigger;
	var clearEvents = t.clearEvents;
	var renderOverlay = t.renderOverlay;
	var clearOverlays = t.clearOverlays;
	var reportSelection = t.reportSelection;
	var unselect = t.unselect;
	var daySelectionMousedown = t.daySelectionMousedown;
	var slotSegHtml = t.slotSegHtml;
	var formatDate = calendar.formatDate;


	// locals

	var mainContainer;
	var dayList;

	var dayTable;
	var dayHead;
	var dayHeadCells;
	var dayBody;
	var dayBodyCells;
	var dayBodyCellInners;
	var dayBodyFirstCell;
	var dayBodyFirstCellStretcher;
	var slotLayer;
	var daySegmentContainer;
	var allDayTable;
	var allDayRow;
	var slotScroller;
	var slotContent;
	var slotSegmentContainer;
	var slotTable;
	var axisFirstCells;
	var gutterCells;
	var selectionHelper;

	var viewWidth;
	var viewHeight;
	var axisWidth;
	var colWidth;
	var gutterWidth;
	var slotHeight; // TODO: what if slotHeight changes? (see issue 650)
	var savedScrollTop;

	var colCnt;
	var slotCnt;
	var coordinateGrid;
	var hoverListener;
	var colContentPositions;
	var slotTopCache = {};

	var tm;
	var firstDay;
	var nwe;            // no weekends (int)
	var rtl, dis, dit;  // day index sign / translate
	var minMinute, maxMinute;
	var colFormat;

	var marker;
	var markerColumn;
	var markerId;



	/* Rendering
	-----------------------------------------------------------------------------*/


	disableTextSelection(element.addClass('fc-agenda'));


	function renderAgenda(c, renderDayList) {
		colCnt = c;
		updateOptions();
		if (!dayTable) {
			buildSkeleton(renderDayList);
		}else{
			clearEvents();
		}
		updateCells();
	}

    function updateHeader() {
        updateOptions();
        updateCells();
    }

	function updateOptions() {
		tm = opt('theme') ? 'ui' : 'fc';
		nwe = opt('weekends') ? 0 : 1;
		firstDay = opt('firstDay');
		rtl = opt('isRTL');
		if (rtl) {
			dis = -1;
			dit = colCnt - 1;
		}else{
			dis = 1;
			dit = 0;
		}
		minMinute = parseTime(opt('minTime'));
		maxMinute = parseTime(opt('maxTime'));
		colFormat = opt('columnFormat');

		if (t.name == 'agendaWeek') {
		    if (window.innerWidth < 1270) {
		        colFormat = colFormat.replace("dddd", "ddd");
		    } else {
		        if (colFormat.indexOf("dddd") < 0 && colFormat.indexOf("ddd") < 0) {
		            colFormat = "dddd, " + colFormat;
		        }
		    }
		} else {
		    if (colFormat.indexOf("dddd") < 0 && colFormat.indexOf("ddd") < 0) {
		        colFormat = "dddd, " + colFormat;
		    }
		}
		if (colCnt < 2) {
			colFormat = "'<div class=\"center\">'" + colFormat + "'</div>'";
		}
	}

	function calcSlotHeight() {
		slotHeight = slotTable.find("tr:eq(1)").outerHeight(true);
	}

	function buildSkeleton(renderDayList) {
		var headerClass = tm + "-widget-header";
		var contentClass = tm + "-widget-content";
		var s;
		var i;
		var d;
		var maxd;
		var minutes;
		var slotNormal = opt('slotMinutes') % 15 == 0;

		var cont = $(
				"<table class='fc-agenda-inner' cellspacing='0'>" +
					"<tbody>" +
						"<tr>" +
							(colCnt < 2 && renderDayList ?
									"<td class='td-day-list'><div class='day-list'>&nbsp;</div></td>" : "") +
							"<td><div class='main-container'/></td>" +
						"</tr>" +
					"</tbody>" +
				"</table>")
				.appendTo(element);
		dayList = colCnt < 2 && renderDayList ? cont.find(".day-list") : undefined;
		mainContainer = cont.find(".main-container");

		s =
			"<table style='width:100%' class='fc-agenda-days fc-border-separate' cellspacing='0'>" +
			"<thead>" +
			"<tr>" +
			"<th class='fc-agenda-axis " + headerClass + "'>&nbsp;</th>";
		for (i=0; i<colCnt; i++) {
			s +=
				"<th class='fc- fc-col" + i + ' ' + headerClass + "'/>"; // fc- needed for setDayID
		}
		s +=
			"<th class='fc-agenda-gutter " + headerClass + "'>&nbsp;</th>" +
			"</tr>" +
			"</thead>" +
			"<tbody>" +
			"<tr>" +
			"<th class='fc-agenda-axis " + headerClass + "'>&nbsp;</th>";
		for (i=0; i<colCnt; i++) {
			s +=
				"<td class='fc- fc-col" + i + ' ' + contentClass + "'>" + // fc- needed for setDayID
				"<div>" +
				"<div class='fc-day-content'>" +
				"<div style='position:relative;max-height: 150px;'>&nbsp;</div>" +
				"</div>" +
				"</div>" +
				"</td>";
		}
		s +=
			"<td class='fc-agenda-gutter " + contentClass + "'>&nbsp;</td>" +
			"</tr>" +
			"</tbody>" +
			"</table>";
		dayTable = $(s).appendTo(mainContainer);
		dayHead = dayTable.find('thead');
		dayHeadCells = dayHead.find('th').slice(1, -1);
		dayBody = dayTable.find('tbody');
		dayBodyCells = dayBody.find('td').slice(0, -1);
		dayBodyCellInners = dayBodyCells.find('div.fc-day-content div');
		dayBodyFirstCell = dayBodyCells.eq(0);
		dayBodyFirstCellStretcher = dayBodyFirstCell.find('> div');

		markFirstLast(dayHead.add(dayHead.find('tr')));
		markFirstLast(dayBody.add(dayBody.find('tr')));

		axisFirstCells = dayHead.find('th:first');
		gutterCells = dayTable.find('.fc-agenda-gutter');

		slotLayer =
			$("<div style='position:absolute;z-index:2;left:0;width:100%'/>")
				.appendTo(mainContainer);

		if (opt('allDaySlot')) {

			daySegmentContainer =
				$("<div style='position:absolute;z-index:9;top:0;left:0'/>")
					.appendTo(slotLayer);

			s =
				"<table style='width:100%' class='fc-agenda-allday' cellspacing='0'>" +
				"<tr>" +
				"<th class='" + headerClass + " fc-agenda-axis'>" + opt('allDayText') + "</th>" +
				"<td>" +
				"<div class='fc-day-content'><div style='position:relative;max-height: 150px;'/></div>" +
				"</td>" +
				"<th class='" + headerClass + " fc-agenda-gutter'>&nbsp;</th>" +
				"</tr>" +
				"</table>";
			allDayTable = $(s).appendTo(slotLayer);
			allDayRow = allDayTable.find('tr');

			dayBind(allDayRow.find('td'));

			axisFirstCells = axisFirstCells.add(allDayTable.find('th:first'));
			gutterCells = gutterCells.add(allDayTable.find('th.fc-agenda-gutter'));

			slotLayer.append(
				"<div class='fc-agenda-divider " + headerClass + "'>" +
				"<div class='fc-agenda-divider-inner'/>" +
				"</div>"
			);

		}else{

			daySegmentContainer = $([]); // in jQuery 1.4, we can just do $()

		}

		slotScroller =
			$("<div style='position:absolute;width:100%;overflow-x:hidden;overflow-y:auto'/>")
				.appendTo(slotLayer);

		slotContent =
			$("<div style='position:relative;width:100%;overflow:hidden'/>")
				.appendTo(slotScroller);

		slotSegmentContainer =
			$("<div style='position:absolute;z-index:8;top:0;left:0'/>")
				.appendTo(slotContent);

		s =
			"<table class='fc-agenda-slots' style='width:100%' cellspacing='0'>" +
			"<tbody>";
		d = zeroDate();
		maxd = addMinutes(cloneDate(d), maxMinute);
		addMinutes(d, minMinute);
		slotCnt = 0;
		for (i=0; d < maxd; i++) {
			minutes = d.getMinutes();
			s +=
				"<tr class='fc-slot" + i + ' ' + (!minutes ? '' : 'fc-minor') + "'>" +
					"<th class='fc-agenda-axis " + headerClass + "'>" +
						((!slotNormal || !minutes) ?
								"<div" + (i==0?' style="margin-top:0;"':'') + ">" + htmlEscape(formatDate(d, opt('axisFormat'))) + "</div>&nbsp;" :
								'&nbsp;') +
					"</th>" +
					"<td class='" + contentClass + "'>" +
						"<div style='position:relative'>&nbsp;</div>" +
					"</td>" +
				"</tr>";
			addMinutes(d, opt('slotMinutes'));
			slotCnt++;
		}
		s +=
			"</tbody>" +
			"</table>";
		slotTable = $(s).appendTo(slotContent);

		calcSlotHeight();

		slotBind(slotTable.find('td'));

		axisFirstCells = axisFirstCells.add(slotTable.find('th:first'));
	}

	function createTimeMarker(col) {
	    if (!marker || marker.length < 1) {
			marker = $(
					"<div class='fc-time-marker'>" +
					    "<span class='title'/>" +
						"<div class='left-side'/>" +
						"<div class='center-line'/>" +
						"<div class='right-side'/>" +
					"</div>")
					.appendTo(slotContent);
			markerColumn = col;
		}
		updateTimeMarker();
	}

	function updateTimeMarker() {
		if (markerId != undefined) {
			clearTimeout(markerId);
		}
		if (marker) {
			if (colCnt > 0) {
				var padding = 3;

				var localDate = new Date();
				var utcDate = new Date(localDate.getTime() + localDate.getTimezoneOffset() * 60000);
				var tenantDate = new Date(utcDate.getTime() + ASC.Resources.Master.CurrentTenantTimeZone.UtcOffset * 60000);

				var time = localDate.getHours() + ":" + (localDate.getMinutes() < 10 ? "0" + localDate.getMinutes() : localDate.getMinutes());

				var top = timePosition(tenantDate, tenantDate);
				var left = colContentLeft(0) - padding;
				var width = colContentRight(colCnt - 1) + padding - left;
				var l = marker.find(".left-side").outerWidth(true);
				var h = marker.find(".left-side").outerHeight(true);
				marker.css("left", (left - l) + "px");
				marker.css("top", Math.round(top - 0.5 * h) + "px");
				marker.find(".center-line").css("width", width + "px");
				marker.find(".title")[0].innerText = time;
				marker.css("visibility", "visible");
				markerId = setTimeout(updateTimeMarker, 1000 * 60);
			} else {
				marker.css("visibility", "hidden");
			}
		}
	}

	function updateCells() {
		var i;
		var todayCol;
		var headCell;
		var bodyCell;
		var leftCell;
		var date;
		var today = clearTime(new Date());
		dayBodyCells.eq(0).prev().removeClass('fc-today-left');
		for (i=0; i<colCnt; i++) {
			date = colDate(i);
			headCell = dayHeadCells.eq(i);
		    headCell.html((date.getFullYear() == today.getFullYear()) && colFormat.indexOf("yyyy") != -1? formatDate(date, colFormat.substring(0, colFormat.indexOf("yyyy"))) : formatDate(date, colFormat));
			(function(d, hc) {
				var num = hc.find(".number");
				if (num.length > 0) {
					num.click(function() {
						calendar.gotoDate(d);
						calendar.changeView("agendaDay");
					});
				}
			}(date, headCell));
			bodyCell = dayBodyCells.eq(i);
			if (+date == +today) {
				bodyCell.addClass(tm + '-state-highlight fc-today');
				headCell.addClass('fc-today');
				todayCol = i;
				leftCell = bodyCell.prev();
				if (leftCell.length > 0) {
					leftCell.addClass("fc-today-left");
				}
			}else{
				bodyCell.removeClass(tm + '-state-highlight fc-today');
				bodyCell.removeClass('fc-today-left');
				headCell.removeClass('fc-today');
			}
			setDayID(headCell[0], date);
			setDayID(bodyCell[0], date);
		}

		if (dayList != undefined) {
			headCell = dayHeadCells.eq(0);
			dayList.removeClass("fc-sat fc-sun fc-today");
			if (headCell.hasClass("fc-sat")) {
				dayList.addClass("fc-sat");
			} else if (headCell.hasClass("fc-sun")) {
				dayList.addClass("fc-sun");
			}
			if (todayCol != undefined) {
				dayList.addClass("fc-today");
			}
		}

		if (marker && marker.length > 0) {
			markerColumn = todayCol;
			updateTimeMarker();
		}else {
			if (todayCol !== undefined) {
				createTimeMarker(todayCol)
			}
		}
	}

	function setHeight(height, dateChanged) {
		if (height === undefined) {
			height = viewHeight;
		}
		viewHeight = height;
		slotTopCache = {};

		var headHeight = dayBody.position().top;
		var allDayHeight = slotScroller.position().top; // including divider
		var bodyHeight = Math.min( // total body height, including borders
			height - headHeight,   // when scrollbars
			slotTable.height() + allDayHeight + 1 // when no scrollbars. +1 for bottom border
		);

		dayBodyFirstCellStretcher
			.height(bodyHeight - vsides(dayBodyFirstCell));

		slotLayer.css('top', headHeight);

		slotScroller.height(bodyHeight - allDayHeight - 1);

		calcSlotHeight();

		if (dateChanged) {
			resetScroll();
		}
		updateTimeMarker();

		if ($.isFunction(t._onSetHeight)) {t._onSetHeight(height);}
	}

	function setWidth(width) {
		viewWidth = width;
		colContentPositions.clear();

		axisWidth = 0;
		setOuterWidth(
			axisFirstCells
				.width('')
				.each(function(i, _cell) {
					axisWidth = Math.max(axisWidth, $(_cell).outerWidth());
				}),
			axisWidth
		);

		var slotTableWidth = slotScroller[0].clientWidth; // needs to be done after axisWidth (for IE7)
		//slotTable.width(slotTableWidth);

		gutterWidth = slotScroller.width() - slotTableWidth;
		if (gutterWidth) {
			setOuterWidth(gutterCells, gutterWidth);
			gutterCells
				.show()
				.prev()
				.removeClass('fc-last');
		}else{
			gutterCells
				.hide()
				.prev()
				.addClass('fc-last');
		}

		var tableW = slotTableWidth - axisWidth;
		var rem = tableW % colCnt;
		colWidth = Math.floor(tableW / colCnt);
		if (rem == 0) {
			setOuterWidth(dayHeadCells, colWidth);
		} else {
			setOuterWidth(dayHeadCells.filter(":even"), colWidth);
			setOuterWidth(dayHeadCells.filter(":odd"), colWidth + 1);
			setOuterWidth(dayHeadCells.last(), tableW - colWidth * (colCnt-1) - Math.floor(colCnt/2));
		}
		updateTimeMarker();

		if ($.isFunction(t._onSetWidth)) {t._onSetWidth(width);}
	}

	function resetScroll(hour) {
		var d0 = zeroDate();
		var scrollDate = cloneDate(d0);
		scrollDate.setHours(hour !== undefined ? hour : opt('firstHour'));
		var top = timePosition(d0, scrollDate) + 1; // +1 for the border
		function scroll() {
			slotScroller.scrollTop(top);
		}
		scroll();
		setTimeout(scroll, 0); // overrides any previous scroll state made by the browser
	}

	function beforeHide() {
		savedScrollTop = slotScroller.scrollTop();
	}

	function afterShow() {
		slotScroller.scrollTop(savedScrollTop);
	}



	/* Slot/Day clicking and binding
	-----------------------------------------------------------------------*/


	function dayBind(cells) {
		cells.click(slotClick)
			.mousedown(daySelectionMousedown);
	}

	function slotBind(cells) {
		cells.click(slotClick)
			.mousedown(slotSelectionMousedown);
	}

	function slotClick(ev) {
		if (!opt('selectable')) { // if selectable, SelectionManager will worry about dayClick
			var col = Math.min(colCnt-1, Math.floor((ev.pageX - dayTable.offset().left - axisWidth) / colWidth));
			var date = colDate(col);
			var rowMatch = this.parentNode.className.match(/fc-slot(\d+)/); // TODO: maybe use data
			if (rowMatch) {
				var mins = parseInt(rowMatch[1]) * opt('slotMinutes');
				var hours = Math.floor(mins/60);
				date.setHours(hours);
				date.setMinutes(mins%60 + minMinute);
				trigger('dayClick', dayBodyCells[col], date, false, ev);
			}else{
				trigger('dayClick', dayBodyCells[col], date, true, ev);
			}
		}
	}



	/* Semi-transparent Overlay Helpers
	-----------------------------------------------------*/


	function renderDayOverlay(startDate, endDate, refreshCoordinateGrid) { // endDate is exclusive
		if (refreshCoordinateGrid) {
			coordinateGrid.build();
		}
		var visStart = cloneDate(t.visStart);
		var startCol, endCol;
		if (rtl) {
			startCol = dayDiff(endDate, visStart)*dis+dit+1;
			endCol = dayDiff(startDate, visStart)*dis+dit+1;
		}else{
			startCol = dayDiff(startDate, visStart);
			endCol = dayDiff(endDate, visStart);
		}
		startCol = Math.max(0, startCol);
		endCol = Math.min(colCnt, endCol);
		if (startCol < endCol) {
			dayBind(
				renderCellOverlay(0, startCol, 0, endCol-1)
			);
		}
	}

	function renderCellOverlay(row0, col0, row1, col1) { // only for all-day?
		var rect = coordinateGrid.rect(row0, col0, row1, col1, slotLayer);
		return renderOverlay(rect, slotLayer);
	}

	function renderSlotOverlay(overlayStart, overlayEnd) {
		var dayStart = cloneDate(t.visStart);
		var dayEnd = addDays(cloneDate(dayStart), 1);
		for (var i=0; i<colCnt; i++) {
			var stretchStart = new Date(Math.max(dayStart, overlayStart));
			var stretchEnd = new Date(Math.min(dayEnd, overlayEnd));
			if (stretchStart < stretchEnd) {
				var col = i*dis+dit;
				var rect = coordinateGrid.rect(0, col, 0, col, slotContent); // only use it for horizontal coords
				var top = timePosition(dayStart, stretchStart);
				var bottom = timePosition(dayStart, stretchEnd);
				rect.top = top;
				rect.height = bottom - top;
				slotBind(
					renderOverlay(rect, slotContent)
				);
			}
			addDays(dayStart, 1);
			addDays(dayEnd, 1);
		}
	}



	/* Coordinate Utilities
	-----------------------------------------------------------------------------*/


	coordinateGrid = new CoordinateGrid(function(rows, cols) {
		var e, n, p;
		dayHeadCells.each(function(i, _e) {
			e = $(_e);
			n = e.offset().left;
			if (i) {
				p[1] = n;
			}
			p = [n];
			cols[i] = p;
		});
		p[1] = n + e.outerWidth();
		if (opt('allDaySlot')) {
			e = allDayRow;
			n = e.offset().top;
			rows[0] = [n, n+e.outerHeight()];
		}
		var slotTableTop = slotContent.offset().top;
		var slotScrollerTop = slotScroller.offset().top;
		var slotScrollerBottom = slotScrollerTop + slotScroller.outerHeight();
		var slotTableTh = slotTable.find("th");
		function constrain(n) {
			return Math.max(slotScrollerTop, Math.min(slotScrollerBottom, n));
		}
		for (var i=0; i<slotTableTh.length; i++) {
			var h = slotTableTh.eq(i).outerHeight(true);
			rows.push([
				constrain(slotTableTop),
				constrain(slotTableTop + h)
			]);
			slotTableTop += h;
		}
	});


	hoverListener = new HoverListener(coordinateGrid);


	colContentPositions = new ElementsPositionCache(function(col) {
		return dayBodyCellInners.eq(col);
	});


	function colContentLeft(col) {
		return colContentPositions.left(col);
	}

	function colContentRight(col) {
		return colContentPositions.right(col);
	}




	function dateCell(date) { // "cell" terminology is now confusing
		return {
			row: Math.floor(dayDiff(date, t.visStart) / 7),
			col: dayOfWeekCol(date.getDay())
		};
	}

	function cellDate(cell) {
		var d = colDate(cell.col);
		var slotIndex = cell.row;
		if (opt('allDaySlot')) {
			slotIndex--;
		}
		if (slotIndex >= 0) {
			addMinutes(d, minMinute + slotIndex * opt('slotMinutes'));
		}
		return d;
	}

	function colDate(col) { // returns dates with 00:00:00
		return addDays(cloneDate(t.visStart), col*dis+dit);
	}

	function cellIsAllDay(cell) {
		return opt('allDaySlot') && !cell.row;
	}

	function dayOfWeekCol(dayOfWeek) {
		return ((dayOfWeek - Math.max(firstDay, nwe) + colCnt) % colCnt)*dis+dit;
	}




	// get the Y coordinate of the given time on the given day (both Date objects)
	function timePosition(day, time) { // both date objects. day holds 00:00 of current day
		day = cloneDate(day, true);
		if (time < addMinutes(cloneDate(day), minMinute)) {
			return 0;
		}
		if (time >= addMinutes(cloneDate(day), maxMinute)) {
			return slotTable.height();
		}
		var slotMinutes = opt('slotMinutes'),
			minutes = time.getHours()*60 + time.getMinutes() - minMinute,
			slotI = Math.floor(minutes / slotMinutes),
			slotTop = slotTopCache[slotI];
		if (slotTop === undefined) {
			slotTop = slotTopCache[slotI] = slotTable.find('tr:eq(' + slotI + ') td div')[0].offsetTop; //.position().top; // need this optimization???
		}
		return Math.max(0, Math.round(
			slotTop - 1 + slotHeight * ((minutes % slotMinutes) / slotMinutes)
		));
	}

	function allDayBounds() {
		return {
			left: axisWidth,
			right: allDayTable.width() - 1 - gutterWidth
		};
	}

	function getAllDayRow(index) {
		return allDayRow;
	}

	function defaultEventEnd(event) {
		var start = cloneDate(event.start);
		if (event.allDay) {
			return start;
		}
		return addMinutes(start, opt('defaultEventMinutes'));
	}



	/* Selection
	---------------------------------------------------------------------------------*/


	function defaultSelectionEnd(startDate, allDay) {
		if (allDay) {
			return cloneDate(startDate);
		}
		return addMinutes(cloneDate(startDate), opt('slotMinutes'));
	}

	function renderSelection(startDate, endDate, allDay) { // only for all-day
		if (allDay) {
			if (opt('allDaySlot')) {
				renderDayOverlay(startDate, addDays(cloneDate(endDate), 1), true);
			}
		}else{
			renderSlotSelection(startDate, endDate);
		}
	}

	function renderSlotSelection(startDate, endDate) {
		var helperOption = opt('selectHelper');
		coordinateGrid.build();
		if (helperOption) {
			var col = dayDiff(startDate, t.visStart) * dis + dit;
			if (col >= 0 && col < colCnt) { // only works when times are on same day
				var rect = coordinateGrid.rect(0, col, 0, col, slotContent); // only for horizontal coords
				var top = timePosition(startDate, startDate);
				var bottom = timePosition(startDate, endDate);
				if (bottom > top) { // protect against selections that are entirely before or after visible range
					rect.top = top;
					rect.height = bottom - top;
					rect.left += 2;
					rect.width -= 5;
					if ($.isFunction(helperOption)) {
						var helperRes = helperOption(startDate, endDate);
						if (helperRes) {
							rect.position = 'absolute';
							rect.zIndex = 8;
							selectionHelper = $(helperRes)
								.css(rect)
								.appendTo(slotContent);
						}
					}else{
						rect.isStart = true; // conside rect a "seg" now
						rect.isEnd = true;   //
						selectionHelper = $(slotSegHtml(
							{
								title: '',
								start: startDate,
								end: endDate,
								className: ['fc-select-helper'],
								editable: false
							},
							rect
						));
						selectionHelper.css('opacity', opt('dragOpacity'));
					}
					if (selectionHelper) {
						slotBind(selectionHelper);
						slotContent.append(selectionHelper);
						setOuterWidth(selectionHelper, rect.width, true); // needs to be after appended
						setOuterHeight(selectionHelper, rect.height, true);
					}
				}
			}
		}else{
			renderSlotOverlay(startDate, endDate);
		}
	}

	function clearSelection() {
		clearOverlays();
		if (selectionHelper) {
			selectionHelper.remove();
			selectionHelper = null;
		}
	}

	function slotSelectionMousedown(ev) {
		if (ev.which == 1 && opt('selectable')) { // ev.which==1 means left mouse button
			unselect(ev);
			var dates;
			hoverListener.start(function(cell, origCell) {
				clearSelection();
				if (cell && cell.col == origCell.col && !cellIsAllDay(cell)) {
					var d1 = cellDate(origCell);
					var d2 = cellDate(cell);
					dates = [
						d1,
						addMinutes(cloneDate(d1), opt('slotMinutes')),
						d2,
						addMinutes(cloneDate(d2), opt('slotMinutes'))
					].sort(cmp);
					renderSlotSelection(dates[0], dates[3]);
				}else{
					dates = null;
				}
			}, ev);
			$(document).one('mouseup', function(ev) {
				hoverListener.stop();
				if (dates) {
					if (+dates[0] == +dates[1]) {
						reportDayClick(dates[0], false, ev);
					}
					reportSelection(dates[0], dates[3], false, ev);
				}
			});
		}
	}

	function reportDayClick(date, allDay, ev) {
		trigger('dayClick', dayBodyCells[dayOfWeekCol(date.getDay())], date, allDay, ev);
	}



	/* External Dragging
	--------------------------------------------------------------------------------*/


	function dragStart(_dragElement, ev, ui) {
		hoverListener.start(function(cell) {
			clearOverlays();
			if (cell) {
				if (cellIsAllDay(cell)) {
					renderCellOverlay(cell.row, cell.col, cell.row, cell.col);
				}else{
					var d1 = cellDate(cell);
					var d2 = addMinutes(cloneDate(d1), opt('defaultEventMinutes'));
					renderSlotOverlay(d1, d2);
				}
			}
		}, ev);
	}

	function dragStop(_dragElement, ev, ui) {
		var cell = hoverListener.stop();
		clearOverlays();
		if (cell) {
			trigger('drop', _dragElement, cellDate(cell), cellIsAllDay(cell), ev, ui);
		}
	}

    function getCellBounds(row, col) {
        var isWeekView = dayBodyCells.length > 1;
        var allDayContainer = $(".fc-agenda-allday .fc-day-content:visible")[0];
        var cellContainer = isWeekView ? dayBodyCells[col] : allDayContainer;

        if (allDayContainer && cellContainer) {
            return {
                left: cellContainer.offsetLeft,
                top: allDayContainer.offsetTop,
                right: cellContainer.offsetLeft + cellContainer.offsetWidth - (isWeekView ? 0 : 325),
                bottom: allDayContainer.offsetTop + allDayContainer.offsetHeight
            };
        }
        return {top:0, right:0, bottom:0, left:0};
	}

}

function AgendaEventRenderer() {
	var t = this;


	// exports
	t.renderEvents = renderEvents;
	t.compileDaySegs = compileDaySegs; // for DayEventRenderer
	t.clearEvents = clearEvents;
	t.slotSegHtml = slotSegHtml;
	t.bindDaySeg = bindDaySeg;


	// imports
	DayEventRenderer.call(t);
	var opt = t.opt;
	var trigger = t.trigger;
	//var setOverflowHidden = t.setOverflowHidden;
	var isEventDraggable = t.isEventDraggable;
	var isEventResizable = t.isEventResizable;
	var eventEnd = t.eventEnd;
	var reportEvents = t.reportEvents;
	var reportEventClear = t.reportEventClear;
	var eventElementHandlers = t.eventElementHandlers;
	var setHeight = t.setHeight;
	var getDaySegmentContainer = t.getDaySegmentContainer;
	var getSlotSegmentContainer = t.getSlotSegmentContainer;
	var getHoverListener = t.getHoverListener;
	var getMaxMinute = t.getMaxMinute;
	var getMinMinute = t.getMinMinute;
	var timePosition = t.timePosition;
	var colContentLeft = t.colContentLeft;
	var colContentRight = t.colContentRight;
	var renderDaySegs = t.renderDaySegs;
	var resizableDayEvent = t.resizableDayEvent; // TODO: streamline binding architecture
	var getColCnt = t.getColCnt;
	var getColWidth = t.getColWidth;
	var getSlotHeight = t.getSlotHeight;
	var getBodyContent = t.getBodyContent;
	var reportEventElement = t.reportEventElement;
	var showEvents = t.showEvents;
	var hideEvents = t.hideEvents;
	var eventDrop = t.eventDrop;
	var eventResize = t.eventResize;
	var renderDayOverlay = t.renderDayOverlay;
	var clearOverlays = t.clearOverlays;
	var calendar = t.calendar;
	var formatDate = calendar.formatDate;
	var formatDates = calendar.formatDates;



	/* Rendering
	----------------------------------------------------------------------------*/


	function renderEvents(events, modifiedEventId) {
		reportEvents(events);
		var i, len=events.length,
			dayEvents=[],
			slotEvents=[];
		for (i=0; i<len; i++) {
			if (events[i].allDay) {
				dayEvents.push(events[i]);
			}else{
				slotEvents.push(events[i]);
			}
		}
		if (opt('allDaySlot')) {
		    var compileSlotSeg = compileSlotSegs(slotEvents);
		    if (t.name == "agendaDay") {
		        if (compileSlotSeg.length > 0) {
		            for (var j = 0; j < compileSlotSeg.length; j++) {
		                var startSegm = new Date(compileSlotSeg[j].start);
		                startSegm.setDate(startSegm.getDate() + 1);
		                if(startSegm.getTime() == cloneDate(compileSlotSeg[j].end, true).getTime()){
		                    var _clone = {};
		                    Object.assign(_clone, compileSlotSeg[j].event);
		                    _clone._start = _clone.start = compileSlotSeg[j].event.start;
		                    _clone._end = _clone.end = compileSlotSeg[j].event.end;
		                    _clone.isEditable = false;
		                    _clone.isSegmEvent = true;
		                    dayEvents.push(_clone);
		                }
		            }
		        }
		    }else if (t.name == "agendaWeek") {
		        if (compileSlotSeg.length > 0) {
		            for (var j = 0; j < compileSlotSeg.length; j++) {
		                var startSegm = new Date(compileSlotSeg[j].start);
		                startSegm.setDate(startSegm.getDate() + 1);
		                if(startSegm.getTime() == cloneDate(compileSlotSeg[j].end, true).getTime()){
		                    var _clone = {};
		                    Object.assign(_clone, compileSlotSeg[j].event);
		                    _clone._start = _clone.start = compileSlotSeg[j].event.start;
		                    _clone._end = _clone.end = compileSlotSeg[j].event.end;
		                    compileSlotSeg[j].event.isSegmEventWeek = true;
		                    if (dayEvents.find(function (x) { return x.objectId === _clone.objectId;}) == undefined) dayEvents.push(_clone);
		                }
		            }
		        }
		    }
			renderDaySegs(compileDaySegs(dayEvents), modifiedEventId);
			setHeight(); // no params means set to viewHeight
		}
		renderSlotSegs(compileSlotSegs(slotEvents), modifiedEventId);
	}


	function clearEvents() {
		reportEventClear();
		getDaySegmentContainer().empty();
		getSlotSegmentContainer().empty();
	}


	function compileDaySegs(events) {
		var levels = stackSegs(sliceSegs(events, exclEndDay, t.visStart, t.visEnd)),
			i, levelCnt=levels.length, level,
			j, seg,
			segs=[];
		for (i=0; i<levelCnt; i++) {
			level = levels[i];
			for (j=0; j<level.length; j++) {
				seg = level[j];
				seg.row = 0;
				seg.level = i; // not needed anymore
				segs.push(seg);
			}
		}
		return segs;
	}


	function countForwardSegs(levels) {
		var i, j, k, level, segForward, segBack;
		for (i=levels.length-1; i>0; i--) {
			level = levels[i];
			for (j=0; j<level.length; j++) {
				segForward = level[j];
				for (k=0; k<levels[i-1].length; k++) {
					segBack = levels[i-1][k];
					if (segsCollide(segForward, segBack)) {
						segBack.forward = Math.max(segBack.forward||0, (segForward.forward||0)+1);
					}
				}
			}
		}
	}


	function compileSlotSegs(events) {
		var colCnt = getColCnt(),
			minMinute = getMinMinute(),
			maxMinute = getMaxMinute(),
			d = addMinutes(cloneDate(t.visStart), minMinute),
			i, col,
			j, level,
			k, seg,
			segs=[];
		for (i=0; i<colCnt; i++) {
		    col = stackSegs(sliceSegs(events, slotEventEnd, d, addMinutes(cloneDate(d), maxMinute-minMinute)));
			countForwardSegs(col);
			for (j=0; j<col.length; j++) {
				level = col[j];
				for (k=0; k<level.length; k++) {
					seg = level[k];
					seg.col = i;
					seg.level = j;
					segs.push(seg);
				}
			}
			addDays(d, 1, true);
		}
		return segs;
	}


	function slotEventEnd(event) {
		if (event.end) {
			return cloneDate(event.end);
		}else{
			return addMinutes(cloneDate(event.start), opt('defaultEventMinutes'));
		}
	}


	// renders events in the 'time slots' at the bottom

	function renderSlotSegs(segs, modifiedEventId) {

		var i, segCnt=segs.length, seg,
			event,
			top, bottom,
			colI, levelI, forward,
			leftmost,
			availWidth,
			outerWidth,
			left,
			html='',
			eventElements,
			eventElement,
			titleElement,
			timeElement,
			triggerRes,
			vsideCache={},
			hsideCache={},
			key, val,
			contentElement,
			width, height, cheight,
			slotSegmentContainer = getSlotSegmentContainer(),
			rtl, dis, dit,
			colCnt = getColCnt();

		rtl = opt('isRTL');
		if (rtl) {
			dis = -1;
			dit = colCnt - 1;
		}else{
			dis = 1;
			dit = 0;
		}

		// calculate position/dimensions, create html
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			event = seg.event;
			top = timePosition(seg.start, seg.start);
			bottom = timePosition(seg.start, seg.end);
			colI = seg.col;
			levelI = seg.level;
			forward = seg.forward || 0;
			leftmost = colContentLeft(colI*dis + dit);
			availWidth = colContentRight(colI*dis + dit) - leftmost;
			availWidth = Math.min(availWidth-6, availWidth*.95); // TODO: move this to CSS
			if (levelI) {
				// indented and thin
				outerWidth = availWidth / (levelI + forward + 1);
			}else{
				if (forward) {
					// moderately wide, aligned left still
					outerWidth = ((availWidth / (forward + 1)) - (12/2)) * 2; // 12 is the predicted width of resizer =
				}else{
					// can be entire width, aligned left
					outerWidth = availWidth;
				}
			}
			left = leftmost +                                  // leftmost possible
				(availWidth / (levelI + forward + 1) * levelI) // indentation
				* dis + (rtl ? availWidth - outerWidth : 0);   // rtl
			seg.top = top;
			seg.left = left;
			seg.outerWidth = outerWidth;
			seg.outerHeight = bottom - top;
			var slotHtml = slotSegHtml(event, seg);
			var slot = $(slotHtml);
			if (t.name == "agendaDay") {
			    var segStart = new Date(seg.start);
			    segStart.setDate(segStart.getDate() + 1);
			    if(segStart.getTime() == cloneDate(seg.end, true).getTime()) {
			        slot.addClass('hidden');
			        slotHtml = $('<div>').append(slot.clone()).html();
			    }
			}else if (t.name == "agendaWeek") {
			    if (seg.event.isSegmEventWeek) {
			        slot.addClass('hidden');
			        slotHtml = $('<div>').append(slot.clone()).html();
			    }
			}
		    html += slotHtml;
		}
		slotSegmentContainer[0].innerHTML = html; // faster than html()
		eventElements = slotSegmentContainer.children();

		// retrieve elements, run through eventRender callback, bind event handlers
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			event = seg.event;
			eventElement = $(eventElements[i]); // faster than eq()
			triggerRes = trigger('eventRender', event, event, eventElement);
			if (triggerRes === false) {
				eventElement.remove();
			}else{
				if (triggerRes && triggerRes !== true) {
					eventElement.remove();
					eventElement = $(triggerRes)
						.css({
							position: 'absolute',
							top: seg.top,
							left: seg.left
						})
						.appendTo(slotSegmentContainer);
				}
				seg.element = eventElement;
				if (event._id === modifiedEventId) {
					bindSlotSeg(event, eventElement, seg);
				}else{
					eventElement[0]._fci = i; // for lazySegBind
				}
				reportEventElement(event, eventElement);
			}
		}

		lazySegBind(slotSegmentContainer, segs, bindSlotSeg);

		// record event sides and title positions
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			eventElement = seg.element;
			if (eventElement) {
				val = vsideCache[key = seg.key = cssKey(eventElement[0])];
				seg.vsides = val === undefined ? (vsideCache[key] = vsides(eventElement, true)) : val;
				val = hsideCache[key];
				seg.hsides = val === undefined ? (hsideCache[key] = hsides(eventElement, true)) : val;
				contentElement = eventElement.find('div.fc-event-content');
				if (contentElement.length) {
					seg.contentTop = contentElement[0].offsetTop;
				}
			}
		}

		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			eventElement = seg.element;
			if (eventElement) {
				width = Math.max(0, seg.outerWidth - seg.hsides);
				height = Math.max(0, seg.outerHeight - seg.vsides);
				eventElement[0].style.width = width + 'px';
				eventElement[0].style.height = height > 0 ? height + 'px' : 'auto';
				// set height of event-content
				contentElement = eventElement.find('.fc-event-content');
				cheight = height -
					eventElement.find(".fc-event-head").outerHeight(true);
				contentElement.css("height", cheight + "px");
				titleElement = eventElement.find(".fc-event-title");
				timeElement = eventElement.find('.fc-event-time');
				event = seg.event;
				if (seg.contentTop !== undefined && height - seg.contentTop < 10) {
					// not enough room for title, put it in the time header
					timeElement
						.html(timeElement.html() + ' ' + htmlEscape(titleElement.text()));

					if (titleElement.hasClass("fc-event-cancelled"))
						timeElement.addClass("fc-event-cancelled");
					else
						timeElement.removeClass("fc-event-cancelled");

					titleElement
						.remove();
				}
				trigger('eventAfterRender', event, event, eventElement);
			}
		}

		if ($.isFunction(t._afterRenderSlotSegs)) {t._afterRenderSlotSegs(segs);}
	}


	function slotSegHtml(event, seg) {
		var html = '<';
		var url = event.url;
		var skinCss = getSkinCss(event, opt);
		var skinCssAttr = (skinCss ? ' style="' + skinCss + '"' : "");
		var classes = ["fc-event", "fc-event-skin", "fc-event-vert"];
		if (isEventDraggable(event)) {
			classes.push("fc-event-draggable");
		}
		if (seg.isStart) {
			classes.push("fc-corner-top");
		}
		if (seg.isEnd) {
			classes.push("fc-corner-bottom");
		}
		classes = classes.concat(event.className);
		if (event.source) {
			classes = classes.concat(event.source.className || []);
		}
		//if (url) {
		//	html += 'a href="' + htmlEscape(event.url) + '"';
		//}else{
			html += 'div';
		//}

		var title = htmlEscape(event.title);

		html +=
			' class="' + classes.join(" ") + '"' +
			' style="position:absolute;z-index:8;top:' + seg.top + 'px;left:' + seg.left + 'px;' + skinCss + '"' +
			'>' +
			'<div class="fc-event-inner fc-event-skin"' + skinCssAttr + '>' +
				'<div class="fc-event-head fc-event-skin"' + skinCssAttr + '>' +
					'<div class="fc-event-time">' +
						htmlEscape(formatDates(event.start, event.end, opt('timeFormat'))) +
						(event.repeat && event.repeat.type > kRepeatNever ?
								'<span class="fc-event-repeat"></span>' : '') +
						(event.alert && (event.alert.type > kAlertNever || event.alert.type == kAlertDefault &&
								event.source && event.source.defaultAlert.type > kAlertNever) ?
								'<span class="fc-event-alert"></span>' : '') +
					'</div>' +
				'</div>' +
				'<div class="fc-event-content" title="' + title + '">' +
					'<div class="fc-event-title' + (event.status == 2 ? " fc-event-cancelled" : "") + '">' +
						title +
						//'<div class="fc-event-tooltip">' + htmlEscape(event.title) + '</div>' +
					'</div>' +
				'</div>' +
				'<div class="fc-event-bg"></div>' +
			'</div>'; // close inner
		if (seg.isEnd && isEventResizable(event)) {
			html += '<div class="ui-resizable-handle ui-resizable-s">&nbsp;</div>';
		}
		html +=
			'</' + /*(url ? 'a' :*/ 'div'/*)*/ + '>';
		return html;
	}


	function bindDaySeg(event, eventElement, seg) {
		if (isEventDraggable(event)) {
			draggableDayEvent(event, eventElement, seg.isStart);
		}
		if (seg.isEnd && isEventResizable(event)) {
			resizableDayEvent(event, eventElement, seg);
		}
		eventElementHandlers(event, eventElement);
			// needs to be after, because resizableDayEvent might stopImmediatePropagation on click
	}


	function bindSlotSeg(event, eventElement, seg) {
		var timeElement = eventElement.find('div.fc-event-time');
		if (isEventDraggable(event)) {
			draggableSlotEvent(event, eventElement, timeElement);
		}
		if (seg.isEnd && isEventResizable(event)) {
			resizableSlotEvent(event, eventElement, timeElement);
		}
		eventElementHandlers(event, eventElement);
	}



	/* Dragging
	-----------------------------------------------------------------------------------*/


	// when event starts out FULL-DAY

	function draggableDayEvent(event, eventElement, isStart) {
		var origWidth;
		var revert;
		var allDay=true;
		var dayDelta;
		var dis = opt('isRTL') ? -1 : 1;
		var hoverListener = getHoverListener();
		var colWidth = getColWidth();
		var slotHeight = getSlotHeight();
		var minMinute = getMinMinute();
		eventElement.draggable({
			zIndex: 9,
			opacity: opt('dragOpacity', 'month'), // use whatever the month view was using
			revertDuration: opt('dragRevertDuration'),
			start: function(ev, ui) {
			
				trigger('eventDragStart', eventElement, event, ev, ui);
				hideEvents(event, eventElement);
				origWidth = eventElement.width();
				hoverListener.start(function(cell, origCell, rowDelta, colDelta) {
					clearOverlays();
					if (cell) {
						//setOverflowHidden(true);
						revert = false;
						dayDelta = colDelta * dis;
						if (!cell.row) {
							// on full-days
							renderDayOverlay(
								addDays(cloneDate(event.start), dayDelta),
								addDays(exclEndDay(event), dayDelta)
							);
							resetElement();
						}else{
							// mouse is over bottom slots
							if (isStart) {
								if (allDay) {
									// convert event to temporary slot-event
									eventElement.width(colWidth - 10); // don't use entire width
									setOuterHeight(
										eventElement,
										slotHeight * Math.round(
											(event.end ? ((event.end - event.start) / MINUTE_MS) : opt('defaultEventMinutes'))
											/ opt('slotMinutes')
										)
									);
									eventElement.draggable('option', 'grid', [colWidth, 1]);
									allDay = false;
								}
							}else{
								revert = true;
							}
						}
						revert = revert || (allDay && !dayDelta);
					}else{
						resetElement();
						//setOverflowHidden(false);
						revert = true;
					}
					eventElement.draggable('option', 'revert', revert);
				}, ev, 'drag');
			},
			stop: function(ev, ui) {
				hoverListener.stop();
				clearOverlays();
				
				trigger('eventDragStop', eventElement, event, ev, ui);
				if (revert) {
					// hasn't moved or is out of bounds (draggable has already reverted)
					resetElement();
					eventElement.css('filter', ''); // clear IE opacity side-effects
					showEvents(event, eventElement);
				}else{
					// changed!
					var minuteDelta = 0;
					if (!allDay) {
						minuteDelta = Math.round((eventElement.offset().top - getBodyContent().offset().top) / slotHeight)
							* opt('slotMinutes')
							+ minMinute
							- (event.start.getHours() * 60 + event.start.getMinutes());
					}
					eventDrop(this, event, dayDelta, minuteDelta, allDay, ev, ui);
				}
				//setOverflowHidden(false);
			}
		});
		function resetElement() {
			if (!allDay) {
				eventElement
					.width(origWidth)
					.height('')
					.draggable('option', 'grid', null);
				allDay = true;
			}
		}
	}


	// when event starts out IN TIMESLOTS

	function draggableSlotEvent(event, eventElement, timeElement) {
		var origPosition;
		var allDay=false;
		var dayDelta;
		var minuteDelta;
		var prevMinuteDelta;
		var dis = opt('isRTL') ? -1 : 1;
		var hoverListener = getHoverListener();
		var colCnt = getColCnt();
		var colWidth = getColWidth();
		var slotHeight = getSlotHeight();
		eventElement.draggable({
			zIndex: 9,
			scroll: false,
			grid: [colWidth, slotHeight],
			axis: colCnt==1 ? 'y' : false,
			opacity: opt('dragOpacity'),
			revertDuration: opt('dragRevertDuration'),
			start: function(ev, ui) {
				trigger('eventDragStart', eventElement, event, ev, ui);
				hideEvents(event, eventElement);
				origPosition = eventElement.position();
				minuteDelta = prevMinuteDelta = 0;
				hoverListener.start(function(cell, origCell, rowDelta, colDelta) {
					eventElement.draggable('option', 'revert', !cell);
					clearOverlays();
					if (cell) {
						dayDelta = colDelta * dis;
						if (opt('allDaySlot') && !cell.row) {
							// over full days
							if (!allDay) {
								// convert to temporary all-day event
								allDay = true;
								timeElement.hide();
								eventElement.draggable('option', 'grid', null);
							}
							renderDayOverlay(
								addDays(cloneDate(event.start), dayDelta),
								addDays(exclEndDay(event), dayDelta)
							);
						}else{
							// on slots
							resetElement();
						}
					}
				}, ev, 'drag');
			},
			drag: function(ev, ui) {
				minuteDelta = Math.round((ui.position.top - origPosition.top) / slotHeight) * opt('slotMinutes');
				if (minuteDelta != prevMinuteDelta) {
					if (!allDay) {
						updateTimeText(minuteDelta);
					}
					prevMinuteDelta = minuteDelta;
				}
			},
			stop: function(ev, ui) {
				var cell = hoverListener.stop();
				clearOverlays();
				trigger('eventDragStop', eventElement, event, ev, ui);
				if (cell && (dayDelta || minuteDelta || allDay)) {
					// changed!
					eventDrop(this, event, dayDelta, allDay ? 0 : minuteDelta, allDay, ev, ui);
				}else{
					// either no change or out-of-bounds (draggable has already reverted)
					resetElement();
					eventElement.css('filter', ''); // clear IE opacity side-effects
					eventElement.css(origPosition); // sometimes fast drags make event revert to wrong position
					updateTimeText(0);
					showEvents(event, eventElement);
				}
			}
		});
		function updateTimeText(minuteDelta) {
			var newStart = addMinutes(cloneDate(event.start), minuteDelta);
			var newEnd;
			if (event.end) {
				newEnd = addMinutes(cloneDate(event.end), minuteDelta);
			}
			timeElement.text(formatDates(newStart, newEnd, opt('timeFormat')));
		}
		function resetElement() {
			// convert back to original slot-event
			if (allDay) {
				timeElement.css('display', ''); // show() was causing display=inline
				eventElement.draggable('option', 'grid', [colWidth, slotHeight]);
				allDay = false;
			}
		}
	}



	/* Resizing
	--------------------------------------------------------------------------------------*/


	function resizableSlotEvent(event, eventElement, timeElement) {
		var slotDelta, prevSlotDelta;
		var slotHeight = getSlotHeight();
		eventElement.resizable({
			handles: {
				s: 'div.ui-resizable-s'
			},
			grid: slotHeight,
			start: function(ev, ui) {
				slotDelta = prevSlotDelta = 0;
				hideEvents(event, eventElement);
				eventElement.css('z-index', 9);
				trigger('eventResizeStart', this, event, ev, ui);
			},
			resize: function(ev, ui) {
				// don't rely on ui.size.height, doesn't take grid into account
				slotDelta = Math.round((Math.max(slotHeight, eventElement.height()) - ui.originalSize.height) / slotHeight);
				if (slotDelta != prevSlotDelta) {
					timeElement.text(
						formatDates(
							event.start,
							(!slotDelta && !event.end) ? null : // no change, so don't display time range
								addMinutes(eventEnd(event), opt('slotMinutes')*slotDelta),
							opt('timeFormat')
						)
					);
					prevSlotDelta = slotDelta;
				}
			},
			stop: function(ev, ui) {
				trigger('eventResizeStop', this, event, ev, ui);
				if (slotDelta) {
					eventResize(this, event, 0, opt('slotMinutes')*slotDelta, ev, ui);
				}else{
					eventElement.css('z-index', 8);
					showEvents(event, eventElement);
					// BUG: if event was really short, need to put title back in span
				}
				ev.stopImmediatePropagation();
			}
		});
	}


}



function BasicListView(element, calendar, viewName) {
	var _this = this;

	// exports
	_this.renderBasic = _renderBasic;
	_this.renderEvents = _renderEvents;
	_this.clearEvents = _clearEvents;
	_this.setWidth = _setWidth;
	_this.setHeight = _setHeight;
	_this.beforeHide = _beforeHide;
	_this.afterShow = _afterShow;
	_this.defaultEventEnd = _defaultEventEnd;
	_this.select = _select;
	_this.unselect = _unselect;

	_this.showMonthTitle = false;

	// imports
	View.call(_this, element, calendar, viewName);


	var formatDate = calendar.formatDate;
	var formatDates = calendar.formatDates;


	var _eventsList;


	function _renderBasic() {
		if (_eventsList == undefined) {
			_eventsList = $('<div class="fc-lv-scroller"/>').appendTo(element);
		}
	}

	function _compileSegs(events) {
	    return sliceSegs(events, exclEndDay, _this.start, _this.end).sort(
				function(left, right) {
					var d1 = cloneDate(left.event.start, left.event.allDay);
					var d2 = cloneDate(right.event.start, right.event.allDay);
					var res = d1 < d2 ? -1 : (d1 > d2 ? 1 :
							(left.event.title.toLowerCase() < right.event.title.toLowerCase() ? -1 : 1));
					return res;
				});
	}

	function _renderEvents(events, modifiedEventId) {
		_clearEvents.call(_this);

		var lv = calendar.options.listView;
		var now = new Date();
		var i, j, k;
		var list, monthTitle, dayTitle, mt, dt;
		var seg, event, start, end, eventTitle, eventNote, eventRow;
		var html;
		var first;
		var timeFormat = lv.timeFormat;
		var monthFormat = lv.monthTitleFormat;
		var dayFormat = (lv.dayTitleFormat.indexOf("dddd") < 0 &&
				lv.dayTitleFormat.indexOf("ddd") < 0 ? "dddd, " : "") + lv.dayTitleFormat;

		var segs = _compileSegs.call(_this, events);

		list = {};
		for (i = 0; i < segs.length; ++i) {
		    seg = segs[i];
		    event = seg.event;
			start = cloneDate(seg.start, true);
			end = seg.end != undefined ? cloneDate(seg.end, true) : cloneDate(start);

			eventTitle =
					typeof event.title == "string" ? event.title.replace(/(.+)\.?\s*$/i, "$1") : "";
			eventNote =
					typeof event.description == "string" ? event.description.replace(/(.+)\.?\s*$/i, "$1") : "";
			eventRow =
					'<tr class="event-row" id="event_' + event._id + '">' +
						'<td class="time first">' +
							'<span class="time">' +
								htmlEscape(!event.allDay ?
										formatDates(event.start, event.end || event.start, timeFormat) :
										calendar.options.allDayText) + '</span>' + '</td>' +
						'<td class="bullet">' +
							'<span class="bullet" style="color:' + htmlEscape(event.source.isHidden ?
									calendar.options.categories.inactiveColor : event.source.backgroundColor) +
									';">' +
								htmlEscape(calendar.options.categories.itemBullet) + '&nbsp;</span>' + '</td>' +
						'<td class="title last' + (event.status == 2 ? " fc-event-cancelled" : "") +'">' +
							'<span class="title '+((event.isTodo == true && event.completed == true) ? 'completed':'')+'">' + htmlEscape(eventTitle) + '</span>' +
							(eventNote && eventNote.length > 0 ?
									('<span class="note">' + htmlEscape(eventNote) + '</span>') : '') + '</td>' +
					'</tr>';

			var oneDay = 86400000;
			var startTime = start.getTime();
		    var startEventDay = !event.allDay && event.start != null ? new Date(event.start.getFullYear(), event.start.getMonth(), event.start.getDate()) : null,
                        endEventDay = !event.allDay && event.end != null ? new Date(event.end.getFullYear(), event.end.getMonth(), event.end.getDate()) : null;

            for (; start < end; addDays(start, +1)) {

                var newEventRow = null;
                if (!event.allDay && startEventDay != null && endEventDay != null) {

                    var $eventRow = $(eventRow),
                        spanTime = $eventRow[0].firstChild.firstChild;

                    var startDiff = startTime - startEventDay.getTime(),
                        endDiff = endEventDay.getTime() - startTime;

                    if (startDiff == 0 && endDiff == 0) {
                        spanTime.innerHTML = formatDate(event.start, timeFormat) + "<span class='dash'></span>" + formatDate(event.end, timeFormat);    //one day
                        
                    } else if (startDiff == 0 && endDiff != 0) {
                        spanTime.innerHTML = formatDate(event.start, timeFormat) + "<span class='dash'></span><span>...</span>";                        //first day of N
                        
                    } else if (startDiff != 0 && endDiff == 0) {
                        spanTime.innerHTML = "<span>...</span><span class='dash'></span>" + formatDate(event.end, timeFormat);                          //last day

                    } else if (startDiff != 0 && endDiff != 0) {
                        spanTime.innerHTML = calendar.options.allDayText;
                    }
                    newEventRow = $eventRow[0].outerHTML;
                    startTime += oneDay;
		        }

				monthTitle = formatDate(start, monthFormat);
				dayTitle = start.getFullYear() == now.getFullYear() && dayFormat.indexOf("yyyy") != -1 ? formatDate(start, dayFormat.substring(0, dayFormat.indexOf("yyyy"))) : formatDate(start, dayFormat);
				mt = "_" + encodeURIComponent(monthTitle).replace(/[^a-z0-9_]/gi, "_");
				dt = "_" + encodeURIComponent(dayTitle).replace(/[^a-z0-9_]/gi, "_");

				if (list[mt] === undefined) {
					list[mt] =
							{
								title: '<div class="fc-lv-month-title">' + htmlEscape(monthTitle) + '</div>'
							};
				}
				if (list[mt][dt] === undefined) {
					list[mt][dt] =
							{
							    title: '<tr><th colspan="3"><span>' + htmlEscape(dayTitle.split(',')[0]) + '</span><span>' + htmlEscape(dayTitle.split(',')[1]) + '</span></th></tr>',
								today: now.getDate() == start.getDate() &&
											 now.getMonth() == start.getMonth() &&
											 now.getFullYear() == start.getFullYear(),
								events: []
							};
				}
				list[mt][dt].events.push(newEventRow!=null ? newEventRow : eventRow); 
			}
		}

		first = true;
		html = '';
		// iterate through months
		for (i in list) {
			if (list[i].title === undefined) {
				continue;
			}
			if (_this.showMonthTitle) {
				html += list[i].title;
				first = true;
			}
			// iterate through days
			for (j in list[i]) {
				if (list[i][j].title === undefined) {continue;}
				html += '<table class="fc-lv-events' +
						(first ? (first = false, ' first') : '') +
						(list[i][j].today ? ' today' : '') +
						'" border="0" cellspacing="0" cellpadding="0"><tbody><td style="width: 25%"><table><tbody>' + list[i][j].title + '</tbody></table></td><td style="width: 75%"><table style="width: 100%"><tbody>';
			    // iterate through events
				for (k = 0; k < list[i][j].events.length; ++k) {
					html += list[i][j].events[k];
				}
				html += '</tbody></table></td></tbody></table>';
			}
		}

		if (html.length > 0) {
		    var w = _eventsList[0].clientWidth - 6/* padding in .fc-lv-scroller */;
			_eventsList.html('<div">' + html + '</div>');
			_eventsList.find("table.fc-lv-events").find("tr:last").addClass("last");
			_eventsList.find("tr.event-row").click(function(ev){
				var tr = $(this);
				var id = tr.attr("id");
				var event = calendar.clientEvents(id.substring(id.search("event_") + 6));
				if (event.length > 0 && event[0]) {
					var eventElement = tr.find(".title .title");
					calendar.editEvent(eventElement, event[0]);
					calendar.trigger('eventClick', this, event[0], ev);
				}
			});
		} else {
			_eventsList.html(
					'<div class="fc-lv-no-events">' +
						htmlEscape(calendar.options.listView.noEventsMessage) +
					'</div>');
		}
	}

	function _clearEvents() {
		var node = _eventsList[0];
		var last;
		while (last = node.lastChild)
			node.removeChild(last);
	}


	function _setHeight(height, dateChanged) {
		_eventsList.height(height);
	}

	function _setWidth(width) {
	}

	function _beforeHide() {
	}

	function _afterShow() {
	}


	function _defaultEventEnd(event) {
		var start = cloneDate(event.start);
		if (event.allDay) {
			return start;
		}
		return addMinutes(start, _this.opt('defaultEventMinutes'));
	}


	function _select(startDate, endDate, allDay) {
	}

	function _unselect(ev) {
	}

}


fcViews.list = ListView;

function ListView(element, calendar) {
	var _this = this;

	// imports
	BasicListView.call(_this, element, calendar, "list");
	//_this.showMonthTitle = true;


	var formatDate = calendar.formatDate;
	var formatDates = calendar.formatDates;


	var _initialized;


	this.render = function(date, delta) {
		if (delta) {
			addDays(date, delta);
		}

		var diff;
		if (_initialized) {
			diff = dayDiff(_this.end, _this.start);
		}
		if (!_initialized) {
			_this.start = _this.visStart = cloneDate(date, true);
			_this.end = _this.visEnd = addMonths(cloneDate(_this.start), 3);
		} else {
			if (date < _this.visStart || date >= _this.visEnd) {
				_this.start = _this.visStart = cloneDate(date, true);
			} else {
				_this.start = cloneDate(date, true);
			}
			var d = addDays(cloneDate(_this.start), diff);
			if (d < _this.visStart || d >= _this.visEnd) {
				_this.end = _this.visEnd = d;
			} else {
				_this.end = d;
			}
		}

		_this.title = _renderTitle.call(_this, _this.start, addDays(cloneDate(_this.end), -1));

		_this.renderBasic();
		if (!_initialized) {
			_this.afterShow();
			_initialized = true;
		}
	}

	this.beforeHide = function() {
		var h = $("#fc_container .fc-header");
		h.find(".fc-button-next, .fc-button-prev").show();
	};

	this.afterShow = function() {
		var h = $("#fc_container .fc-header");
		h.find(".fc-button-next, .fc-button-prev").hide();
	};


	function _changeFromDate(label, dp) {
		this.close();

		var date = dp.datepicker("getDate");
		_changeFromLabel.call(_this, label, date);

		if (_this.visStart <= date && date < _this.visEnd) {
			var diff = dayDiff(_this.end, _this.start);
			_this.start = date;
			var d = addDays(cloneDate(_this.start), diff);
			if (d >= _this.visEnd) {
				_this.end = _this.visEnd = d;
				calendar.refetchEvents();
			} else {
				_this.end = d;
				_this.eventsDirty = true;
			}
			_changeToLabel.call(_this, undefined, d);
		}
		calendar.gotoDate(date);
	}

	function _changeToDate(label, dp) {
		this.close();

		var date = dp.datepicker("getDate");
		var date1 = addDays(cloneDate(date), 1);
		_changeToLabel.call(_this, label, date1);

		if (date < _this.visStart) {
			var diff = dayDiff(_this.end, _this.start);
			calendar.gotoDate(addDays(date, -diff));
		} else if (date1 >= _this.visEnd) {
			_this.end = _this.visEnd = date1;
			calendar.refetchEvents();
		} else {
			_this.end = date1;
			calendar.rerenderEvents();
		}
	}

	function _changeFromLabel(elem, date) {
		var label = $(elem);
		var newLabel = formatDate(date, calendar.options.listView.headerDateFormat);
		// update visible label
		if (label != undefined && label.length > 0) {
			label.find(".inner").text(newLabel);
		}
		// update label in unattached DOM Node
		_this.title.find(".from.link .inner").text(newLabel);
	}

	function _changeToLabel(elem, date) {
		var label = $(elem);
		var newLabel = formatDate(
				addDays(cloneDate(date), -1), calendar.options.listView.headerDateFormat);
		// update visible label
		if (label != undefined && label.length > 0) {
			label.find(".inner").text(newLabel);
		}
		// update label in unattached DOM Node
		_this.title.find(".to.link .inner").text(newLabel);
	}

	function _renderTitle(start, end) {
		var lv = calendar.options.listView;
		var title = $(
				'<span>' +
					'<span class="from link">' +
						'<span class="inner">' +
							htmlEscape(formatDate(start, lv.headerDateFormat)) + '</span>' +
					'</span>' +
					'<span class="delimiter">&#8212;</span>' +
					'<span class="to link">' +
						'<span class="inner">' +
							htmlEscape(formatDate(end, lv.headerDateFormat)) + '</span>' +
					'</span>' +
				'</span>');
		title.find(".from.link").click(function() {
			fcDatepicker.open(calendar, this,
					function(dp) {dp.datepicker("setDate", _this.start);},
					_changeFromDate);
		});
		title.find(".to.link").click(function() {
			fcDatepicker.open(calendar, this,
					function(dp) {dp.datepicker("setDate", addDays(cloneDate(_this.end), -1));},
					_changeToDate);
		});
		//
		return title;
	}

}


fcViews.listMonth = ListMonthView;

function ListMonthView(element, calendar) {
	var _this = this;

	BasicListView.call(_this, element, calendar, 'listMonth');


	this.render = function(date, delta) {
		if (delta) {
			addMonths(date, delta);
			date.setDate(1);
		}
		_this.start = _this.visStart = cloneDate(date, true);
		_this.start.setDate(1);
		_this.end = _this.visEnd = addDays(addMonths(cloneDate(_this.start), 1), 0);

		var titleRender = new TitleRender(calendar);
		_this.title = titleRender.render(_this.name, _this.start, _this.end);

		_this.renderBasic();
	};

}


fcViews.listWeek = ListWeekView;

function ListWeekView(element, calendar) {
	var _this = this;

	BasicListView.call(_this, element, calendar, 'listWeek');


	this.render = function(date, delta) {
		if (delta) {
			addDays(date, delta * 7);
		}
		var start = addDays(cloneDate(date), -((date.getDay() - _this.opt('firstDay') + 7) % 7));
		var end = addDays(cloneDate(start), 7);

		_this.start = _this.visStart = start;
		_this.end = _this.visEnd = end;

		var titleRender = new TitleRender(calendar);
		_this.title = titleRender.render(_this.name, _this.start, addDays(cloneDate(_this.end), -1));

		_this.renderBasic();
	};

}


fcViews.listDay = ListDayView;

function ListDayView(element, calendar) {
	var _this = this;

	BasicListView.call(_this, element, calendar, 'listDay');


	this.render = function(date, delta) {
		if (delta) {
			addDays(date, delta);
		}

		_this.start = _this.visStart = cloneDate(date, true);
		_this.end = _this.visEnd = addDays(cloneDate(date, true), 1);

		var titleRender = new TitleRender(calendar);
		_this.title = titleRender.render(_this.name, _this.start, addDays(cloneDate(_this.end), -1));

		_this.renderBasic();
	};

}



function View(element, calendar, viewName) {
	var t = this;


	// exports
	t.element = element;
	t.calendar = calendar;
	t.name = viewName;
	t.opt = opt;
	t.trigger = trigger;
	//t.setOverflowHidden = setOverflowHidden;
	t.isEventDraggable = isEventDraggable;
	t.isEventResizable = isEventResizable;
	t.reportEvents = reportEvents;
	t.eventEnd = eventEnd;
	t.reportEventElement = reportEventElement;
	t.reportEventClear = reportEventClear;
	t.eventElementHandlers = eventElementHandlers;
	t.showEvents = showEvents;
	t.hideEvents = hideEvents;
	t.eventDrop = eventDrop;
	t.eventResize = eventResize;
	t.getEventElement = getEventElement;
	t.getViewRect = _getViewRect;
	// t.title
	// t.start, t.end
	// t.visStart, t.visEnd


	// imports
	var defaultEventEnd = t.defaultEventEnd;
	var normalizeEvent = calendar.normalizeEvent; // in EventManager
	var reportEventChange = calendar.reportEventChange;


	// locals
	var eventsByID = {};
	var eventElements = [];
	var eventElementsByID = {};
	var options = calendar.options;



	function _getViewRect() {
		return fcUtil.getElementRect(element);
	}


	function opt(name, viewNameOverride) {
		var v = options[name];
		if (typeof v == 'object') {
			return smartProperty(v, viewNameOverride || viewName);
		}
		return v;
	}


	function trigger(name, thisObj) {
		return calendar.trigger.apply(
			calendar,
			[name, thisObj || t].concat(Array.prototype.slice.call(arguments, 2), [t])
		);
	}


	/*
	function setOverflowHidden(bool) {
		element.css('overflow', bool ? 'hidden' : '');
	}
	*/


	function isEventDraggable(event) {
		return isEventEditable(event) && !opt('disableDragging');
	}


	function isEventResizable(event) { // but also need to make sure the seg.isEnd == true
		return isEventEditable(event) && !opt('disableResizing');
	}


	function isEventEditable(event) {
		return firstDefined(event.editable, (event.source || {}).editable, opt('editable')) && (
				event.source == undefined ||
				fcUtil.objectIsEditable(event) ||
				fcUtil.objectIsEditable(event.source) ||
				event.source && !event.source.isSubscription);
	}



	/* Event Data
	------------------------------------------------------------------------------*/


	// report when view receives new events
	function reportEvents(events) { // events are already normalized at this point
		eventsByID = {};
		var i, len=events.length, event;
		for (i=0; i<len; i++) {
			event = events[i];
			if (eventsByID[event._id]) {
				eventsByID[event._id].push(event);
			}else{
				eventsByID[event._id] = [event];
			}
		}
	}


	// returns a Date object for an event's end
	function eventEnd(event) {
		return event.end ? cloneDate(event.end) : defaultEventEnd(event);
	}



	/* Event Elements
	------------------------------------------------------------------------------*/


	// report when view creates an element for an event
	function reportEventElement(event, element) {
		eventElements.push(element);
		if (eventElementsByID[event._id]) {
			eventElementsByID[event._id].push(element);
		}else{
			eventElementsByID[event._id] = [element];
		}
	}


	function reportEventClear() {
		eventElements = [];
		eventElementsByID = {};
	}


	// attaches eventClick, eventMouseover, eventMouseout
	function eventElementHandlers(event, eventElement) {
		// in IE onclick triggered if, after resizing the cursor stays on the variable object
		if($.browser.msie) {
			eventElement.mouseup(function (ev) {
				if (!eventElement.hasClass('ui-draggable-dragging') &&
					!eventElement.hasClass('ui-resizable-resizing') &&
					!$(ev.target).hasClass('ui-resizable-handle')) {
					calendar.editEvent(eventElement, event);
					return trigger('eventClick', this, event, ev);
				}
				return undefined;
			});
		} else {
			eventElement.click(function (ev) {
				if (!eventElement.hasClass('ui-draggable-dragging') &&
					!eventElement.hasClass('ui-resizable-resizing') &&
					!$(ev.target).hasClass('ui-resizable-handle')) {
					calendar.editEvent(eventElement, event);
					return trigger('eventClick', this, event, ev);
				}
				return undefined;
			});
		}
		eventElement
			.hover(
				function(ev) {
					trigger('eventMouseover', this, event, ev);
				},
				function(ev) {
					trigger('eventMouseout', this, event, ev);
				}
			);
		// TODO: don't fire eventMouseover/eventMouseout *while* dragging is occuring (on subject element)
		// TODO: same for resizing
	}


	function showEvents(event, exceptElement) {
		eachEventElement(event, exceptElement, 'show');
	}


	function hideEvents(event, exceptElement) {
		eachEventElement(event, exceptElement, 'hide');
	}


	function eachEventElement(event, exceptElement, funcName) {
		var elements = eventElementsByID[event._id],
			i, len = elements.length;
		for (i=0; i<len; i++) {
			if (!exceptElement || elements[i][0] != exceptElement[0]) {
				elements[i][funcName]();
			}
		}
	}


	function getEventElement(event) {
		if (event._id != undefined) {
			return eventElementsByID[event._id][0];
		}
		return null;
	}



	/* Event Modification Reporting
	---------------------------------------------------------------------------------*/


	function eventDrop(e, event, dayDelta, minuteDelta, allDay, ev, ui) {
		var oldAllDay = event.allDay;
		var eventId = event._id;
		
		if (!event.isTodo) {
		    moveEvents(eventsByID[eventId], dayDelta, minuteDelta, allDay);
		    trigger(
		        'eventDrop',
		        e,
		        event,
		        dayDelta,
		        minuteDelta,
		        allDay,
		        function() {
		            // TODO: investigate cases where this inverse technique might not work
		            moveEvents(eventsByID[eventId], -dayDelta, -minuteDelta, oldAllDay);
		            reportEventChange(eventId, event);
		        },
		        ev,
		        ui
		    );
		    reportEventChange(eventId, event);
		} else {
		    moveEvents(eventsByID[eventId], dayDelta, minuteDelta, allDay);
		    calendar.todolist.updateTodo(event);

		}
	}


	function eventResize(e, event, dayDelta, minuteDelta, ev, ui) {
		var eventId = event._id;
		elongateEvents(eventsByID[eventId], dayDelta, minuteDelta);
		trigger(
			'eventResize',
			e,
			event,
			dayDelta,
			minuteDelta,
			function() {
				// TODO: investigate cases where this inverse technique might not work
				elongateEvents(eventsByID[eventId], -dayDelta, -minuteDelta);
				reportEventChange(eventId, event);
			},
			ev,
			ui
		);
		reportEventChange(eventId, event);
	}



	/* Event Modification Math
	---------------------------------------------------------------------------------*/


	function moveEvents(events, dayDelta, minuteDelta, allDay) {
		minuteDelta = minuteDelta || 0;
		for (var e, len=events.length, i=0; i<len; i++) {
			e = events[i];
			if (allDay !== undefined) {
				e.allDay = allDay;
			}
			addMinutes(addDays(e.start, dayDelta, true), minuteDelta);
			if (e.end) {
				e.end = addMinutes(addDays(e.end, dayDelta, true), minuteDelta);
			}else if (!allDay) {
			    e.end =  new Date(e.start.getFullYear(), e.start.getMonth(), e.start.getDate(), e.start.getHours(), e.start.getMinutes() + 30);
			}
			normalizeEvent(e, options);
		}
	}


	function elongateEvents(events, dayDelta, minuteDelta) {
		minuteDelta = minuteDelta || 0;
		for (var e, len=events.length, i=0; i<len; i++) {
			e = events[i];
			e.end = addMinutes(addDays(eventEnd(e), dayDelta, true), minuteDelta);
			normalizeEvent(e, options);
		}
	}


}

function DayEventRenderer() {
	var t = this;


	// exports
	t.renderDaySegs = renderDaySegs;
	t.resizableDayEvent = resizableDayEvent;


	// imports
	var opt = t.opt;
	var trigger = t.trigger;
	var isEventDraggable = t.isEventDraggable;
	var isEventResizable = t.isEventResizable;
	var eventEnd = t.eventEnd;
	var reportEventElement = t.reportEventElement;
	var showEvents = t.showEvents;
	var hideEvents = t.hideEvents;
	var eventResize = t.eventResize;
	var getRowCnt = t.getRowCnt;
	var getColCnt = t.getColCnt;
	var getColWidth = t.getColWidth;
	var allDayRow = t.allDayRow;
	var allDayBounds = t.allDayBounds;
	var colContentLeft = t.colContentLeft;
	var colContentRight = t.colContentRight;
	var dayOfWeekCol = t.dayOfWeekCol;
	var dateCell = t.dateCell;
	var cellDate = t.cellDate;
	var compileDaySegs = t.compileDaySegs;
	var getDaySegmentContainer = t.getDaySegmentContainer;
	var bindDaySeg = t.bindDaySeg; //TODO: streamline this
	var formatDates = t.calendar.formatDates;
	var renderDayOverlay = t.renderDayOverlay;
	var clearOverlays = t.clearOverlays;
	var clearSelection = t.clearSelection;
	var getCellBounds = t.getCellBounds;



	/* Rendering
	-----------------------------------------------------------------------------*/

	function calcDaySegs(segs, segmentContainer, modifiedEventId) {
		var i,
			l = segs.length,
			seg,
			element,
			elements = segmentContainer.children();

		for (i = 0; i < l; i++) {
			seg = segs[i];
			element = $(elements[i]);
			daySegElementResolve(seg, element);
			daySegElementReport(seg);
			daySegHandlers(seg, i, modifiedEventId);

			trigger('eventAfterRender', seg.event, seg.event, element);
		}

		for (i = 0, l = elements.length; i < l; i++) {
			element = elements[i];
			if (element.className != "fc-event-info") continue;
			var array = element.id.split('_');
			$(element).click({ row: array[1], col: array[2], segs: segs }, showHiddenEvents);
		}

		lazySegBind(segmentContainer, segs, bindDaySeg);
	}

	function renderDaySegs(segs, modifiedEventId) {
		var segmentContainer = getDaySegmentContainer();

		segmentContainer[0].innerHTML = daySegHTML(segs); // faster than .html()

		calcDaySegs(segs, segmentContainer, modifiedEventId);

		if ($.isFunction(t._afterRenderDaySegs)) {
			t._afterRenderDaySegs(segs);
		}
	}


	function calcTempDaySegs(segs, segmentContainer, adjustRow, adjustTop) {
		var i,
			l = segs.length,
			seg,
		    element,
			elements = segmentContainer.children(),
			result = [];

		for (i = 0; i < l; i++) {
			seg = segs[i];
			element = $(elements[i]);
			daySegElementResolve(seg, element);

			trigger('eventAfterRender', seg.event, seg.event, element);

			element = segs[i].element;
			if (element) {
				if (segs[i].row === adjustRow) {
					element.css('top', adjustTop);
				}
				result.push(element[0]);
			}
		}

		return $(result);
	}

	function renderTempDaySegs(segs, adjustRow, adjustTop) {
		var segmentContainer = getDaySegmentContainer();

		segmentContainer[0].innerHTML = daySegHTML(segs); // faster than .html()

		return calcTempDaySegs(segs, segmentContainer, adjustRow, adjustTop);
	}

	function daySegHTML(segs) { // also sets seg.left and seg.outerWidth
		var rtl = opt('isRTL');
		var i;
		var segCnt=segs.length;
		var seg;
		var event;
		var classes;
		var bounds = allDayBounds();
		var minLeft = bounds.left;
		var maxLeft = bounds.right;
		var leftCol;
		var rightCol;
		var left;
		var right;
		var skinCss;
		var html = '';

		var sizeManager = ASC.CalendarSizeManager.cache;
		var widgetHeaderHeight = 0;
		var dayNumberElementHeight = 0;
		var dayContentElement;
		var dayContentElementHeight;
		var dayContentElementVsides = sizeManager.fcMonthContentCellContentVSides;
		var segHsides = sizeManager.fcMonthContentCellContentSegmentHSides;
		var segHeight = sizeManager.fcMonthContentCellContentSegmentHeight + segHsides;
		var infoBoxes = {};
		var maxLevel = 0;

		if (t.name == "month") {
			widgetHeaderHeight = sizeManager.fcMonthContentTheadHeight;
			dayNumberElementHeight = sizeManager.fcMonthContentCellNumberHeight;
			dayContentElement = document.querySelector(".fc-content .fc-view-month .fc-day-content");
			dayContentElementHeight = parseFloat(dayContentElement.style.height) + dayContentElementVsides;
		} else if (t.name == "agendaWeek") {
			dayContentElement = document.querySelector(".fc-content .fc-view-agendaWeek .fc-agenda-allday .fc-day-content");
			dayContentElementHeight = sizeManager.fcMonthContentCellContentMaxHeight;
		} else if (t.name == "agendaDay") {
			dayContentElement = document.querySelector(".fc-content .fc-view-agendaDay .fc-agenda-allday .fc-day-content");
			dayContentElementHeight = sizeManager.fcMonthContentCellContentMaxHeight;
		}


		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			event = seg.event;

			var isOpened = false;
			if (t.calendar.openEventWin) {
				if (event.objectId == t.calendar.openEventWin.objectId) {
					isOpened = true;
				}
			}

			classes = [
				'fc-event',
				event.allDay ? 'fc-event-skin-day' : 'fc-event-skin',
				event.isTodo && event.completed ? 'fc-todo-completed' : '',
				isOpened ? event.isTodo ? 'fc-todo-open' : 'fc-event-open' : '',
				'fc-event-hori',
				isEventDraggable(event) ? 'fc-event-draggable' : ''
			];

			if (rtl) {
				if (seg.isStart) {
					classes.push('fc-corner-right');
				}
				if (seg.isEnd) {
					classes.push('fc-corner-left');
				}
				leftCol = dayOfWeekCol(seg.end.getDay()-1);
				rightCol = dayOfWeekCol(seg.start.getDay());
				left = seg.isEnd ? colContentLeft(leftCol) : minLeft;
				right = seg.isStart ? colContentRight(rightCol) : maxLeft;
			}else{
				if (seg.isStart) {
					classes.push('fc-corner-left');
				}
				if (seg.isEnd) {
					classes.push('fc-corner-right');
				}
				leftCol = dayOfWeekCol(seg.start.getDay());
				rightCol = dayOfWeekCol(seg.end.getDay()-1);
				left = seg.isStart ? colContentLeft(leftCol) : minLeft;
				right = seg.isEnd ? colContentRight(rightCol) : maxLeft;
			}

			classes = classes.concat(event.className);
			if (event.source) {
				classes = classes.concat(event.source.className || []);
			}

			seg.left = left;
			seg.outerWidth = right - left;
			seg.startCol = leftCol;
			seg.endCol = rightCol + 1; // needs to be exclusive
			seg.hsides = segHsides;
			seg.outerHeight = segHeight;
			seg.top = (seg.outerHeight * seg.level) + (seg.row * (dayNumberElementHeight + dayContentElementHeight) + dayNumberElementHeight + sizeManager.fcMonthContentCellVBorders * seg.row) + widgetHeaderHeight;

			if (maxLevel < seg.level)
				maxLevel = seg.level;

			skinCss = getSkinCss(event, opt, t.name);

			var styles = "";

			if (seg.isStart == false && seg.isEnd == true) {
				classes.push('fc-segm-right');
				styles = 'width:' + (Math.max(0, seg.outerWidth - seg.hsides) - 10) + 'px;';
			} else if (seg.isStart == true && seg.isEnd == false) {
				classes.push('fc-segm-left');
				styles = 'width:' + (Math.max(0, seg.outerWidth - seg.hsides) - 10) + 'px;';
			} else if (seg.isStart == false && seg.isEnd == false) {
				classes.push('fc-segm-right');
				classes.push('fc-segm-left');
				styles = 'width:' + (Math.max(0, seg.outerWidth - seg.hsides) - 20) + 'px;';
			} else {
				styles = 'width:' + Math.max(0, seg.outerWidth - seg.hsides) + 'px;';
			}

			styles += 'top:' + seg.top + 'px;';

			var rowBottom;
			if (t.name == "month") {
				rowBottom = (seg.row + 1) * (dayNumberElementHeight + dayContentElementHeight) + widgetHeaderHeight;
			} else if (t.name == "agendaWeek") {
				rowBottom = dayContentElementHeight + widgetHeaderHeight;
			} else if (t.name == "agendaDay") {
				rowBottom = dayContentElementHeight + widgetHeaderHeight;
			}

			if (seg.top + seg.outerHeight > rowBottom - seg.outerHeight) {

				styles += "display: none;";

				var cellId = "cell_" + seg.row + "_" + seg.startCol;

				if (!infoBoxes[cellId])
					infoBoxes[cellId] = { seg: seg, text: "" };

				var s = infoBoxes[cellId].text.match(/(\d+)/);
				var cnt = parseInt((s ? s[1] : 0), 10) || 0;
				infoBoxes[cellId].text = t.calendar.options.moreEventsLabel.replace("%d", cnt + 1);
			}

			html +=
				"<div class='" + classes.join(' ') + "'" +
				" style='position:absolute;z-index:7;left:" + left + "px;" + styles + skinCss + "'" +
				">" +
				"<div" + (event.allDay ?
					" class='fc-event-inner fc-event-skin-day'" :
					" class='fc-event-inner fc-event-skin'") +
					(skinCss ? " style='" + skinCss + "'" : '') +
				">";

			if (!event.allDay && seg.isStart) {
				html +=
					"<span class='fc-event-time'>" +
						htmlEscape(formatDates(event.start, event.end, opt('timeFormat'))) +
						"&nbsp;" +
					"</span>";

				html += "<span class='bullet' style='color:" + htmlEscape(event.source ?
						event.source.backgroundColor : t.calendar.options.eventBackgroundColor) +
						";'>" + htmlEscape(t.calendar.options.categories.itemBullet) + "&nbsp;</span>";
			} else {
				html += '<span>&nbsp;</span>';      // to prevent collapsing
			}

			var title = htmlEscape(event.title);

			html +=
				"<span class='fc-event-title" + (event.status == 2 ? " fc-event-cancelled" : "") + "' title='" + title + "'>" + title + "</span>" +
				"</div>";
			if (seg.isEnd && isEventResizable(event) && event.allDay) {
				html +=  event.isTodo == true ? "" :
					"<div class='ui-resizable-handle ui-resizable-" + (rtl ? 'w' : 'e') + "'>" +
					"&nbsp;&nbsp;&nbsp;" + // makes hit area a lot better for IE6/7
					"</div>";
			}
			html += "</div>";
		}

		for (var name in infoBoxes) {
			var item = infoBoxes[name];
			html += "<div id='" + name + "' style='width:" + item.seg.outerWidth + "px;top:" + item.seg.top + "px;left:" + item.seg.left + "px;height:" + item.seg.outerHeight + "px;line-height:" + item.seg.outerHeight + "px;' class='fc-event-info'>" + item.text + "</div>";
		}

		if (t.name == "agendaWeek" || t.name == "agendaDay") {
			dayContentElement.style.height = Math.min(segHeight * (maxLevel + 2), dayContentElementHeight) + "px";
		}

		return html;
	}


    function daySegElementResolve(seg, element) { // sets seg.element
        var event = seg.event;
        var triggerRes = trigger('eventRender', event, event, element);

        if (triggerRes === false) {
            element.remove();
        } else {
            if (triggerRes && triggerRes !== true) {
                triggerRes = $(triggerRes)
                    .css({
                        position: 'absolute',
                        left: seg.left
                    });
                element.replaceWith(triggerRes);
                element = triggerRes;
            }
            seg.element = element;
        }
    }


    function daySegElementReport(seg) {
        if (seg.element) {
            reportEventElement(seg.event, seg.element);
        }
    }


    function daySegHandlers(seg, index, modifiedEventId) {
        var element = seg.element;

        if (element) {
            var event = seg.event;
            if (event._id === modifiedEventId) {
                bindDaySeg(event, element, seg);
            } else {
                element[0]._fci = index; // for lazySegBind
            }
        }
    }


	function showHiddenEvents(evData) {
		var co = t.calendar.options;
		var evInfo = $(this);
		var parent = evInfo.parent();
		var popup = parent.find("#cell_popup");
		
		if (popup.length < 1) {
			popup = $("<div id='cell_popup' class='fc-event-popup'/>");
            $(document).mouseup(function (e) {
                if (popup.has(e.target).length === 0 && $(e.target)[0].id != popup[0].id) {
                    if (!t.calendar.isEditingEvent()) {
                        popup.hide();
                    }
                }
            });
            document.onkeydown = function (e) {
                e = e || window.event;
                if (e.keyCode === 27) {
                    if (popup.has(e.target).length === 0 && $(e.target)[0].id !== popup[0].id) {
                        if (!t.calendar.isEditingEvent()) {
                            popup.hide();
                        }
                    }
                }
            };
            popup.appendTo(parent);
		}
		
		var cb = getCellBounds(evData.data.row, evData.data.col);
		var cw = cb.right - cb.left;
		var ch = cb.bottom - cb.top;
		var wratio = 1.4;
		var hratio = 1.4;
		
		popup.width(Math.floor(cw * wratio));
		popup.height("auto");

		var celld = cellDate(evData.data);

		popup.empty();
		popup.append($(
				"<div class='fc-day-number'>" +
					htmlEscape(co.dayNames[celld.getDay()] + ", " +
							formatDates(celld, null, t.calendar.options.popupCellFormat)) +
				"</div>"));
		popup.show();

		for (var i = 0; i < evData.data.segs.length; ++i) {
			var seg = evData.data.segs[i];
			if (seg.row == evData.data.row && seg.startCol == evData.data.col) {
				var el = seg.element.clone();
				el.css("display", "block")
					.css("left", "")
					.css("top", "")
					.css("position", "")
					.css("width", "auto")
					.show();
				t.eventElementHandlers(seg.event, el);
				popup.append(el);
			}
		}
		popup.find(".ui-resizable-e").remove();

		var h1 = Math.floor(ch * hratio);
		var h2 = popup.height();
		var h = h1 > h2 ? h1 : h2;
		popup.height(h);

		var pw = popup.outerWidth(true);
		var left = evData.data.col == 0 ?
				cb.left + 1 :
				(evData.data.col == getColCnt() - 1 ?
						Math.floor(cb.left - (pw - cw)) - 1 :
						Math.floor(cb.left - (pw - cw) * 0.5));
						
		var ph = popup.outerHeight(true);
		if (ph > 190) {
			popup.css("overflow-y", "auto").css("overflow-x", "hidden").css("max-height", "190px");
			ph = 190;
		}
		else {
			popup.css("overflow-y", "hidden");
		}
		
		var top = evData.data.row == 0 ?
				cb.top + 1 :
				(evData.data.row == getRowCnt() - 1 ?
						Math.floor(cb.top - (ph - ch)) - 1 :
						Math.floor(cb.top - (ph - ch) * 0.5));
		
		
		popup.css("left", left + "px");
		popup.css("top", top + "px");
	}



	/* Resizing
	-----------------------------------------------------------------------------------*/


	function resizableDayEvent(event, element, seg) {
		var rtl = opt('isRTL');
		var direction = rtl ? 'w' : 'e';
		var handle = element.find('div.ui-resizable-' + direction);
		var isResizing = false;

		// TODO: look into using jquery-ui mouse widget for this stuff
		disableTextSelection(element); // prevent native <a> selection for IE
		element
			.mousedown(function(ev) { // prevent native <a> selection for others
				ev.preventDefault();
			})
			.click(function(ev) {
				if (isResizing) {
					ev.preventDefault(); // prevent link from being visited (only method that worked in IE6)
					ev.stopImmediatePropagation(); // prevent fullcalendar eventClick handler from being called
					                               // (eventElementHandlers needs to be bound after resizableDayEvent)
				}
			});

		handle.mousedown(function(ev) {
			if (ev.which != 1) {
				return; // needs to be left mouse button
			}
			isResizing = true;
			var hoverListener = t.getHoverListener();
			var rowCnt = getRowCnt();
			var colCnt = getColCnt();
			var dis = rtl ? -1 : 1;
			var dit = rtl ? colCnt-1 : 0;
			var elementTop = element.css('top');
			var dayDelta;
			var helpers;
			var eventCopy = $.extend({}, event);
			var minCell = dateCell(event.start);
			clearSelection();
			$('body')
				.css('cursor', direction + '-resize')
				.one('mouseup', mouseup);
			trigger('eventResizeStart', this, event, ev);
			hoverListener.start(function(cell, origCell) {
				if (cell) {
					var r = Math.max(minCell.row, cell.row);
					var c = cell.col;
					if (rowCnt == 1) {
						r = 0; // hack for all-day area in agenda views
					}
					if (r == minCell.row) {
						if (rtl) {
							c = Math.min(minCell.col, c);
						}else{
							c = Math.max(minCell.col, c);
						}
					}
					dayDelta = (r*7 + c*dis+dit) - (origCell.row*7 + origCell.col*dis+dit);
					var newEnd = addDays(eventEnd(event), dayDelta, true);
					if (dayDelta) {
						eventCopy.end = newEnd;
						var oldHelpers = helpers;
						helpers = renderTempDaySegs(compileDaySegs([eventCopy]), seg.row, elementTop);
						helpers.find('*').css('cursor', direction + '-resize');
						if (oldHelpers) {
							oldHelpers.remove();
						}
						hideEvents(event);
					}else{
						if (helpers) {
							showEvents(event);
							helpers.remove();
							helpers = null;
						}
					}
					clearOverlays();
					renderDayOverlay(event.start, addDays(cloneDate(newEnd), 1)); // coordinate grid already rebuild at hoverListener.start
				}
			}, ev);

			function mouseup(ev) {
				trigger('eventResizeStop', this, event, ev);
				$('body').css('cursor', '');
				hoverListener.stop();
				clearOverlays();
				if (dayDelta) {
					eventResize(this, event, dayDelta, 0, ev);
					// event redraw will clear helpers
				}
				// otherwise, the drag handler already restored the old events

				setTimeout(function() { // make this happen after the element's click event
					isResizing = false;
				},0);
			}

		});
	}


}

//BUG: unselect needs to be triggered when events are dragged+dropped

function SelectionManager() {
	var t = this;


	// exports
	t.select = select;
	t.unselect = unselect;
	t.reportSelection = reportSelection;
	t.daySelectionMousedown = daySelectionMousedown;


	// imports
	var opt = t.opt;
	var trigger = t.trigger;
	var defaultSelectionEnd = t.defaultSelectionEnd;
	var renderSelection = t.renderSelection;
	var clearSelection = t.clearSelection;


	// locals
	var selected = false;



	// unselectAuto
	if (opt('selectable') && opt('unselectAuto')) {
		$(document).mousedown(function(ev) {
			var ignore = opt('unselectCancel');
			if (ignore) {
				if ($(ev.target).parents(ignore).length) { // could be optimized to stop after first match
					return;
				}
			}
			unselect(ev);
		});
	}


	function select(startDate, endDate, allDay) {
		unselect();
		if (!endDate) {
			endDate = defaultSelectionEnd(startDate, allDay);
		}
		renderSelection(startDate, endDate, allDay);
		//reportSelection(startDate, endDate, allDay);
	}


	function unselect(ev) {
		if (selected) {
			selected = false;
			clearSelection();
			trigger('unselect', null, ev);
		}
	}


	function reportSelection(startDate, endDate, allDay, ev) {
		selected = true;

		t.calendar.addAndEditEvent(startDate, endDate, allDay);

		trigger('select', null, startDate, endDate, allDay, ev);
		t.calendar.unselect();
	}


	function daySelectionMousedown(ev) { // not really a generic manager method, oh well
		var cellDate = t.cellDate;
		var cellIsAllDay = t.cellIsAllDay;
		var hoverListener = t.getHoverListener();
		var reportDayClick = t.reportDayClick; // this is hacky and sort of weird
		if (ev.which == 1 && opt('selectable')) { // which==1 means left mouse button
			unselect(ev);
			var _mousedownElement = this;
			var dates;
			hoverListener.start(function(cell, origCell) { // TODO: maybe put cellDate/cellIsAllDay info in cell
				clearSelection();
				if (cell && cellIsAllDay(cell)) {
					dates = [ cellDate(origCell), cellDate(cell) ].sort(cmp);
					renderSelection(dates[0], dates[1], true);
				}else{
					dates = null;
				}
			}, ev);
			$(document).one('mouseup', function(ev) {
				hoverListener.stop();
				if (dates) {
					if (+dates[0] == +dates[1]) {
						reportDayClick(dates[0], true, ev);
					}
					reportSelection(dates[0], dates[1], true, ev);
				}
			});
		}
	}


}

function OverlayManager() {
	var t = this;


	// exports
	t.renderOverlay = renderOverlay;
	t.clearOverlays = clearOverlays;


	// locals
	var usedOverlays = [];
	var unusedOverlays = [];


	function renderOverlay(rect, parent) {
		var e = unusedOverlays.shift();
		if (!e) {
			e = $("<div class='fc-cell-overlay' style='position:absolute;z-index:3'/>");
		}
		if (e[0].parentNode != parent[0]) {
			e.appendTo(parent);
		}
		usedOverlays.push(e.css(rect).show());
		return e;
	}


	function clearOverlays() {
		var e;
		while (e = usedOverlays.shift()) {
			unusedOverlays.push(e.hide().unbind());
		}
	}


}

function CoordinateGrid(buildFunc) {

	var t = this;
	var rows;
	var cols;


	this.build = function() {
		rows = [];
		cols = [];
		buildFunc(rows, cols);
	};


	this.cell = function(x, y) {
		var rowCnt = rows.length;
		var colCnt = cols.length;
		var i, r=-1, c=-1;
		for (i=0; i<rowCnt; i++) {
			if (y >= rows[i][0] && y < rows[i][1]) {
				r = i;
				break;
			}
		}
		for (i=0; i<colCnt; i++) {
			if (x >= cols[i][0] && x < cols[i][1]) {
				c = i;
				break;
			}
		}
		return (r>=0 && c>=0) ? {row:r, col:c} : null;
	};


	this.rect = function(row0, col0, row1, col1, originElement) { // row1,col1 is inclusive
		var origin = originElement.offset();
		return {
			top: rows[row0][0] - origin.top,
			left: cols[col0][0] - origin.left,
			width: cols[col1][1] - cols[col0][0],
			height: rows[row1][1] - rows[row0][0]
		};
	};

}

function HoverListener(coordinateGrid) {


	var t = this;
	var bindType;
	var change;
	var firstCell;
	var cell;


	t.start = function(_change, ev, _bindType) {
		change = _change;
		firstCell = cell = null;
		coordinateGrid.build();
		mouse(ev);
		bindType = _bindType || 'mousemove';
		$(document).bind(bindType, mouse);
	};


	function mouse(ev) {
	_fixUIEvent(ev);
		var newCell = coordinateGrid.cell(ev.pageX, ev.pageY);
		if (!newCell != !cell || newCell && (newCell.row != cell.row || newCell.col != cell.col)) {
			if (newCell) {
				if (!firstCell) {
					firstCell = newCell;
				}
				change(newCell, firstCell, newCell.row-firstCell.row, newCell.col-firstCell.col);
			}else{
				change(newCell, firstCell);
			}
			cell = newCell;
		}
	}


	t.stop = function() {
		$(document).unbind(bindType, mouse);
		return cell;
	};


}

function ElementsPositionCache(getElement) {

	var t = this;
	var elements = {};
	var lefts = {};
	var rights = {};

	function e(i) {
		return elements[i] = elements[i] || getElement(i);
	}

	this.left = function(i) {
		return lefts[i] = lefts[i] === undefined ? e(i).position().left : lefts[i];
	};

	this.right = function(i) {
		return rights[i] = rights[i] === undefined ? t.left(i) + e(i).width() : rights[i];
	};

	this.clear = function() {
		elements = {};
		lefts = {};
		rights = {};
	};

}

// select elements
function preventSelection(element){
  var preventSelection = false;

  function addHandler(element, event, handler){
    if (element.attachEvent) 
      element.attachEvent('on' + event, handler);
    else 
      if (element.addEventListener) 
        element.addEventListener(event, handler, false);
  }
  function removeSelection(){
    if (window.getSelection) { window.getSelection().removeAllRanges(); }
    else if (document.selection && document.selection.clear)
      document.selection.clear();
  }
  function killCtrlA(event){
    var event = event || window.event;
    var sender = event.target || event.srcElement;

    if (sender.tagName.match(/INPUT|TEXTAREA/i))
      return;

    var key = event.keyCode || event.which;
    if (event.ctrlKey && key == 'A'.charCodeAt(0))  // 'A'.charCodeAt(0) or 65
    {
      removeSelection();

      if (event.preventDefault) 
        event.preventDefault();
      else
        event.returnValue = false;
    }
  }

  // mouse selection
  addHandler(element, 'mousemove', function(){
    if(preventSelection)
      removeSelection();
  });
  addHandler(element, 'mousedown', function(event){
    var event = event || window.event;
    var sender = event.target || event.srcElement;
    preventSelection = !sender.tagName.match(/INPUT|TEXTAREA/i);
  });

  // dblclick  
  addHandler(element, 'mouseup', function(){
    if (preventSelection)
      removeSelection();
    preventSelection = false;
  });

  // ctrl+A  
  addHandler(element, 'keydown', killCtrlA);
  addHandler(element, 'keyup', killCtrlA);
}

  // this fix was only necessary for jQuery UI 1.8.16 (and jQuery 1.7 or 1.7.1)
    // upgrading to jQuery UI 1.8.17 (and using either jQuery 1.7 or 1.7.1) fixed the problem
    // but keep this in here for 1.8.16 users
    // and maybe remove it down the line

    function _fixUIEvent(event) { // for issue 1168
        if (event.pageX === undefined) {
            event.pageX = event.originalEvent.pageX;
            event.pageY = event.originalEvent.pageY;
        }
    }
    function HorizontalPositionCache(getElement) {

        var t = this,
		elements = {},
		lefts = {},
		rights = {};

        function e(i) {
            return elements[i] = elements[i] || getElement(i);
        }

        t.left = function(i) {
            return lefts[i] = lefts[i] === undefined ? e(i).position().left : lefts[i];
        };

        t.right = function(i) {
            return rights[i] = rights[i] === undefined ? t.left(i) + e(i).width() : rights[i];
        };

        t.clear = function() {
            elements = {};
            lefts = {};
            rights = {};
        };

    }

})(jQuery);