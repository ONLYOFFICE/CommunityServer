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
		
		// personal version
		personal: false,
		
		// display
		defaultView: "month",
		aspectRatio: 1.35,
		minHeight:   500,
		header: {
			left:   "",
			center: "prev,title,next",
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
			list: 'Period'
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
		eventMaxTitleLength:  100,

		todayLabel:           "Today",
		moreEventsLabel:      "events",
		addNewEventLabel:     "Add event",
		addNewLabel:          "Добавить",

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
			title:         'ToDo List',
			hideLabel:     'Hide ToDo List',
			showLabel:     'Show ToDo List',
			addTodoLabel:  'New todo',
			todoEditorUrl: './fullcalendar/tmpl/todo.editor.tmpl',
			newTodoTitle:  'New ToDo item',
			menu: {
				hideColmpletedTodos: {hide: 'Hide completed items', show: 'Show completed items'},
				deleteCompletedTodos: 'Delete completed items'
			},
			sortByCalendarLabel: 'Sort by calendar',
			sortByPriorityLabel: 'Sort by priority',
			sortAlphabeticallyLabel: 'Sort alphabetically'
		},

		eventEditor: {
			dateFormat:                  "yyyy-MM-dd",
			timeFormat:                  "HH:mm",
			newEventTitle:               "New event",
			
			// dialog
			dialogTemplate:              "",
			dialogHeader_add:            "Add new event",
			dialogOwnerLabel:            "Owner:",
			dialogAllDayLabel:           "All-day event",
			dialogAllDay_no:             "This event is not all-day.",
			dialogAllDay_yes:            "This is all-day event.",
			dialogFromLabel:             "From:",
			dialogToLabel:               "To:",
			dialogRepeatLabel:           "Repeat:",
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
			
			// new option
			dialogRepeatOption_custom:   "настройка"
		},		
		
		repeatSettings: {
			dateFormat:                   "yyyy-MM-dd",
			timeFormat:                   "HH:mm",
			
			// dialog
			dialogTemplate:               "",
			dialogHeader:                 "Настройка повторения",
			
			// start date
			dialogFromLabel:              "начиная с",
			
			// end of repeat
			dialogToLabel:                "окончание",
			dialogOptionNever:            "никогда",
			dialogOptionDate:             "дата",
			dialogOptionCount:            "к-во циклов",
			
			dialogAfterLabel:             "после",
			dialogTimesLabel:             "раз",
			
			// repeat by 
			dialogRepeatOnLabel:          "по",
			dialogRepeatOn_days:          "дням",
			dialogRepeatOn_weeks:         "неделям",
			dialogRepeatOn_months:        "месяцам",
			dialogRepeatOn_years:         "годам",
			
			// interval
			dialogEachLabel:              "каждый",
			dialogAliasLabel:             "каждый",
			dialogIntervalOption_day:     "дн",
			dialogIntervalOption_week:    "нед",
			dialogIntervalOption_month:   "мес",
			dialogIntervalOption_year:    "год",
			
			dayNames:                     ['воскресенье','понедельник','вторник','среда','четверг','пятница','суббота'],
			
			dayIndexResponse: {
									su: 0,
									mo: 1,
									tu: 2,
									we: 3,
									th: 4,
									fr: 5,
									sa: 6
			},
			dayNamesShort:                ['вс','пн','вт','ср','чт','пт','сб'],
			dayAliasNames:                ['первый','второй','третий','предпоследний','последний'],
			
			// buttons
			dialogButton_save:            "Сохранить",
			dialogButton_cancel:          "Отмена"
		},
		
		deleteSettings: {
			// dialog
			dialogTemplate:               "",
			dialogHeader:                 "Удалить повторяющееся событие",
			
			dialogDeleteOnlyThisLabel:    "Только это событие",
			dialogDeleteFollowingLabel:   "Это событие и все следующие",
			dialogDeleteAllLabel:         "Все события серии",
			
			// buttons
			dialogButton_save:            "Применить",
			dialogButton_cancel:          "Отмена"
		},
		
		icalStream: {
			// dialog
			dialogTemplate:                       "",
			newiCalTitle:                         "Новый iCal поток",
			
			dialogHeader:                         "Экспорт событий календаря",
			dialogDescription:                    "Используйте следующий адрес для доступа к своему календарю из других приложений.\n\n" + 
												"Можно скопировать и вставить эту информацию в любой календарь, поддерживающий формат iCal.\n\n" + 
												"Можно сохранить эти данные как файл в формате iCal и экспортировать события в другой календарь или приложение.",
			
			dialogImportExportLabel:              "Импорт/Экспорт",
			dialogStreamLink:                     "Экспорт событий из календаря",
			dialogImportLabel:                    "Импорт событий из Google календаря",
			
			dialogButton_fileSelected:            "файл выбран",
			dialogButton_fileNotSelected:         "файл не выбран",
			dialog_incorrectFormat:               "неверный формат",
			
			dialogInputiCalLabel:                 "Введите ссылку iCal-потока",
			dialogSavediCalLabel:                 "Ссылка iCal-потока:",
			
			// buttons
			dialogButton_close:                   "Закрыть",
			dialogButton_browse:                  "Выбрать файл"
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
			cssClassName: "asc-popupmenu",
			items: [
				(year - 3).toString(), (year - 2).toString(), (year - 1).toString(),
				"divider",
				(year + 1).toString(), (year + 2).toString(), (year + 3).toString()
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
			inp.css("color", "red").css("border-color", "red");
			return false;
		}
		inp.css("color", "black");//.css("border-color", "");;
		if (!($.browser.msie && $.browser.version))
			inp.css("border-color", "black");

		if (result) {
			result.value = res;
		}
		return true;
	};
	
	_this.validateInputHttp = function(elem, fn, result) {
		var inp = $(elem);
		var res = fn(inp.val());
		if (null === res) {
			inp.css("color", "red").css("border-color", "red");
			return false;
		}
		else {
			if (res.indexOf("http://") && res.indexOf("https://") && res.indexOf("webcal://")) {
				inp.css("color", "red").css("border-color", "red");
				return false;
			}
		}
		inp.css("color", "black");//.css("border-color", "");
		if (!($.browser.msie && $.browser.version))
			inp.css("border-color", "black");
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
var kEventCancelAction   	  = 5;

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
	var rs = options.repeatSettings;
	var ds = options.deleteSettings;
	var ic = options.icalStream;

	options.categories.dialogTemplate = ('\
<div id="fc_cal_editor">\
	<div class="header">\
		<div class="inner">\
			<span class="new-label">'+htmlEscape(c.dialogHeader_add)+'</span><span class="edit-label">'+htmlEscape(c.dialogHeader_edit)+'</span>\
			<div class="close-btn">&nbsp;</div>\
		</div>\
	</div>\
	<div class="title">\
		<div class="bullet">&#9632;</div>\
		<input type="text" value="'+htmlEscape(c.defaultTitle)+'"/>\
	</div>\
	<div class="color">\
		<span class="label">'+htmlEscape(c.dialogColor_label)+'</span>\
		<span class="outer"><span class="inner">&nbsp;</span></span>\
		<span class="label-for-text">'+htmlEscape(c.dialogTextColor_label)+'</span>\
		<span class="outer"><span class="inner-for-text">&nbsp;</span></span>\
	</div>\
	<div class="row">\
		<div class="alert">\
			<div class="label">'+htmlEscape(e.dialogAlertLabel)+'</div>\
			<select>\
				<option value="'+htmlEscape(kAlertNever)+'">'+htmlEscape(e.dialogAlertOption_never)+'</option>\
				<option value="1">'+htmlEscape(e.dialogAlertOption_5minutes)+'</option>\
				<option value="2">'+htmlEscape(e.dialogAlertOption_15minutes)+'</option>\
				<option value="3">'+htmlEscape(e.dialogAlertOption_30minutes)+'</option>\
				<option value="4">'+htmlEscape(e.dialogAlertOption_hour)+'</option>\
				<option value="5">'+htmlEscape(e.dialogAlertOption_2hours)+'</option>\
				<option value="6">'+htmlEscape(e.dialogAlertOption_day)+'</option>\
			</select>\
		</div>\
		<div class="timezone">\
			<div class="label">'+htmlEscape(c.dialogTimezoneLabel)+'</div>\
			<select/>\
		</div>\
	</div>\
	<div class="shared-list"/>\
	\
	<!-- create iCal-calendar -->\
	<div class="ical-url-input">\
		<div class="ical-label">'+htmlEscape(ic.dialogInputiCalLabel)+'</div>\
		<input type="text" value=""/>\
	</div>\
	\
	<!-- get/set iCal stream -->\
	<div class="ical">\
		<div class="ical-logo">\
		</div>\
		<div class="ical-selectors">\
			<span class="ical-label">'+htmlEscape(ic.dialogImportExportLabel)+'</span>\
			<div class="ical-import">\
				<span id="ical-browse-btn" class="ical-link">'+htmlEscape(ic.dialogImportLabel)+'</span>&nbsp;&nbsp;\
				<span class="ical-file-selected">'+htmlEscape(ic.dialogButton_fileNotSelected)+'</span>\
			</div>\
			<div class="ical-export">\
				<span class="ical-link">'+htmlEscape(ic.dialogStreamLink)+'</span>\
			</div>\
		</div>\
	</div>\
	<div class="buttons">\
		<a class="button blue middle save-btn" href="#">'+htmlEscape(c.dialogButton_save)+'</a>\
		<a class="button gray middle cancel-btn" href="#">'+htmlEscape(c.dialogButton_cancel)+'</a>\
		<a class="button gray middle delete-btn" href="#">'+htmlEscape(c.dialogButton_delete)+'</a>\
	</div>\
</div>').replace(/\>\s+\</g, "><");

	options.categories.subscriptionsDialog = ('\
<div id="fc_subscription_dlg">\
	<div class="header">\
		<div class="inner">\
			<span class="label">'+htmlEscape(c.subscriptionsDialogHeader)+'</span>\
			<div class="close-btn">&nbsp;</div>\
		</div>\
	</div>\
	<div class="title">\
		<div class="bullet">&#9632;</div>\
		<input type="text" value=""/>\
	</div>\
	<div class="color">\
		<span class="label">'+htmlEscape(c.dialogColor_label)+'</span>\
		<span class="outer"><span class="inner">&nbsp;</span></span>\
		<span class="label-for-text">'+htmlEscape(c.dialogTextColor_label)+'</span>\
		<span class="outer"><span class="inner-for-text">&nbsp;</span></span>\
	</div>\
	<div class="row">\
		<div class="alert">\
			<div class="label">'+htmlEscape(e.dialogAlertLabel)+'</div>\
			<select>\
				<option value="'+htmlEscape(kAlertNever)+'">'+htmlEscape(e.dialogAlertOption_never)+'</option>\
				<option value="1">'+htmlEscape(e.dialogAlertOption_5minutes)+'</option>\
				<option value="2">'+htmlEscape(e.dialogAlertOption_15minutes)+'</option>\
				<option value="3">'+htmlEscape(e.dialogAlertOption_30minutes)+'</option>\
				<option value="4">'+htmlEscape(e.dialogAlertOption_hour)+'</option>\
				<option value="5">'+htmlEscape(e.dialogAlertOption_2hours)+'</option>\
				<option value="6">'+htmlEscape(e.dialogAlertOption_day)+'</option>\
			</select>\
		</div>\
		<div class="timezone">\
			<div class="label">'+htmlEscape(c.dialogTimezoneLabel)+'</div>\
			<select/>\
		</div>\
		<div class="timezone-read-only">\
			<div class="label">'+htmlEscape(c.dialogTimezoneLabel)+'</div>\
			<span class="timezone-desc" />\
		</div>\
	</div>\
	<div class="owner">\
		<div class="label">'+htmlEscape(c.subscriptionsDialogOwnerLabel)+'</div>\
		<div><span class="icon">&nbsp;</span><span class="name">Joe Black</span></div>\
	</div>\
	\
	<!-- show iCal url -->\
	<div class="ical-saved-url">\
		<div class="ical-label">'+htmlEscape(ic.dialogSavediCalLabel)+'</div>\
		<span class="saved-url-link "></span>\
	</div>\
	<div class="shared-list"/>\
	\
	<!-- get/set iCal stream -->\
	<div class="ical">\
		<div class="ical-logo">\
		</div>\
		<div class="ical-selectors">\
			<span class="ical-label">'+htmlEscape(ic.dialogImportExportLabel)+'</span>\
			<div class="ical-import">\
				<span id="ical-browse-btn-subs" class="ical-link">'+htmlEscape(ic.dialogImportLabel)+'</span>&nbsp;&nbsp;\
				<span class="ical-file-selected">'+htmlEscape(ic.dialogButton_fileNotSelected)+'</span>\
			</div>\
			<div class="ical-export">\
				<span class="ical-link">'+htmlEscape(ic.dialogStreamLink)+'</span>\
			</div>\
		</div>\
	</div>\
	<div class="buttons">\
		<a class="button blue middle save-btn" href="#">'+htmlEscape(c.dialogButton_save)+'</a>\
		<a class="button gray middle cancel-btn" href="#">'+htmlEscape(c.dialogButton_cancel)+'</a>\
		<a class="button gray middle delete-btn" href="#">'+htmlEscape(c.dialogButton_delete)+'</a>\
		<a class="button gray middle unsubs-btn" href="#">'+htmlEscape(c.subscriptionsDialogButton_unsubscribe)+'</a>\
	</div>\
</div>').replace(/\>\s+\</g, "><");

	options.categories.subscriptionsManageDialog = ('\
<div id="fc_subscr_editor">\
	<div class="header">\
		<div class="inner">\
			<span class="title">'+htmlEscape(c.subscriptionsManageDialog_title)+'</span>\
			<div class="close-btn">&nbsp;</div>\
		</div>\
	</div>\
	<div class="qsearch">\
		<input type="text" value="'+htmlEscape(c.subscriptionsManageDialog_qsearchText)+'"/>\
		<div class="clean-btn">&nbsp;</div>\
	</div>\
	<div class="groups"/>\
	<div class="buttons">\
		<a id="fc_subscr_save" class="button blue middle" href="#">'+htmlEscape(c.subscriptionsManageDialogButton_save)+'</a><span class="splitter">&nbsp;</span>\
		<a id="fc_subscr_cancel" class="button gray middle" href="#">'+htmlEscape(c.subscriptionsManageDialogButton_cancel)+'</a>\
	</div>\
</div>').replace(/\>\s+\</g, "><");

	options.eventEditor.dialogTemplate = ('\
<div id="fc_event_editor">\
	<div class="start-point"></div>\
	<div class="header">\
		<div class="inner">\
			<span>'+htmlEscape(e.dialogHeader_add)+'</span>\
			<div class="close-btn">&nbsp;</div>\
		</div>\
	</div>\
	<div class="viewer">\
		<div class="title">'+htmlEscape(e.newEventTitle)+'</div>\
		<div class="owner">\
			<div class="label">'+htmlEscape(e.dialogOwnerLabel)+'</div>\
			<div><span class="icon">&nbsp;</span><span class="name">Joe Black</span></div>\
		</div>\
		<div class="all-day">\
			<span class="no-label">'+htmlEscape(e.dialogAllDay_no)+'</span>\
			<span class="yes-label">'+htmlEscape(e.dialogAllDay_yes)+'</span>\
		</div>\
		<div class="date-time">\
			<div>\
				<div class="label">'+htmlEscape(e.dialogFromLabel)+'</div>\
				<span class="from-date">8.03.2011</span><span class="from-time">8:00</span>\
			</div>\
			<div class="right">\
				<div class="label">'+htmlEscape(e.dialogToLabel)+'</div>\
				<span class="to-date">8.03.2011</span><span class="to-time">12:00</span>\
			</div>\
		</div>\
		<div class="repeat-alert">\
			<div>\
				<div class="label">'+htmlEscape(e.dialogAlertLabel)+'</div>\
				<span class="alert">8.03.2011</span>\
			</div>\
			<div class="right">\
				<div class="label">'+htmlEscape(e.dialogRepeatLabel)+'</div>\
				<span class="repeat">8.03.2011</span>\
			</div>\
		</div>\
		<div class="shared-list">\
			<div class="no-label">'+htmlEscape(e.dialogSharing_no)+'</div>\
			<div class="yes-label">'+htmlEscape(options.sharedList.title)+'</div>\
			<div class="users-list"/>\
		</div>\
		<div class="calendar">\
			<div class="label">'+htmlEscape(e.dialogCalendarLabel)+'</div>\
			<div><span class="bullet">&#9632;</span><span class="name">Project CRM</span></div>\
		</div>\
		<div class="description">\
			<div class="label">'+htmlEscape(e.dialogDescriptionLabel)+'</div>\
			<div class="text">A decription of an event.</div>\
		</div>\
	</div>\
	<div class="editor">\
		<div class="title"><input type="text" value="New event"/></div>\
		<div class="all-day">\
			<input type="checkbox" class="cb"/><span class="label">'+htmlEscape(e.dialogAllDayLabel)+'</span>\
		</div>\
		<div class="date-time">\
			<div>\
				<div class="label">'+htmlEscape(e.dialogFromLabel)+'</div>\
				<div class="wrapper">\
					<input class="from-date" type="text" value="8.03.2011"/><div class="from cal-icon"/>\
					<input class="from-time" type="text" value="8:00"/>\
				</div>\
			</div>\
			<div class="right">\
				<div class="label">'+htmlEscape(e.dialogToLabel)+'</div>\
				<div class="wrapper">\
					<input class="to-date" type="text" value="8.03.2011"/><div class="to cal-icon"/>\
					<input class="to-time" type="text" value="12:00"/>\
				</div>\
			</div>\
		</div>\
		<div class="repeat-alert">\
			<div>\
				<span class="label">'+htmlEscape(e.dialogAlertLabel)+'</span>&nbsp;<br\>\
				<span class="fc-view-alert">\
					<span class="fc-selector-link">' + htmlEscape(e.dialogAlertOption_default) + '</span>\
					<span class="fc-dropdown">&nbsp;</span>\
				</span>\
			</div>\
			<div class="right">\
				<span class="label">'+htmlEscape(e.dialogRepeatLabel)+'</span>&nbsp;<br\>\
				<span class="fc-view-repeat">\
					<span class="fc-selector-link">' + htmlEscape(e.dialogRepeatOption_never) + '</span>\
					<span class="fc-dropdown">&nbsp;</span>\
				</span>\
			</div>\
		</div>\
		<div class="shared-list"/>\
		<div class="calendar">\
			<div class="label">'+htmlEscape(e.dialogCalendarLabel)+'</div>\
			<div class="wrapper">\
				<div class="bullet">&#9632;</div>\
				<select>\
					<option>Project CRM</option>\
					<option>My calendar</option>\
				</select>\
			</div>\
		</div>\
		<div class="description">\
			<div class="label">'+htmlEscape(e.dialogDescriptionLabel)+'</div>\
			<textarea cols="3" rows="3">A decription of an event</textarea>\
		</div>\
	</div>\
	\
	<!-- Repeat settings block -->\
	<div class="repeat-settings">\
		\
		<!-- Start date -->\
		<div class="fc-start-date">\
			<div class="date-time">\
				<div>\
					<div>'+htmlEscape(rs.dialogFromLabel)+'</div>\
					<div class="wrapper">\
						<input class="from-date" type="text" value="8.03.2011"/><div class="from cal-icon"/>\
						<input class="from-time hidden" type="text" value="00:00"/>\
					</div>\
				</div>\
			</div>\
		</div>\
		\
		<!-- Day/week/month selector -->\
		<div>\
			<span>'+htmlEscape(rs.dialogRepeatOnLabel)+'</span>&nbsp;\
			<span class="fc-dwm-selector">\
				<span class="fc-selector-link">'+htmlEscape(rs.dialogRepeatOn_days)+'</span>\
				<span class="fc-dropdown">&nbsp;</span>\
			</span>\
		</div>\
		\
		<!-- Interval selector -->\
		<div>\
			<span>'+htmlEscape(rs.dialogEachLabel)+'</span>&nbsp;\
			<select class="fc-interval-selector"></select>&nbsp;\
			<span class="fc-interval-label">'+htmlEscape(rs.dialogIntervalOption_day)+'</span>\
		</div>\
		\
		<!-- Days of week -->\
		<div class="fc-days-week">\
		</div>\
		\
		<!-- Radio selector -->\
		<div class="fc-month-radio">\
		</div>\
		\
		<!-- End of repeat -->\
		<div>\
			<span>'+htmlEscape(rs.dialogToLabel)+'</span>&nbsp;\
			<span class="fc-endrepeat-selector">\
				<span class="fc-selector-link">' + htmlEscape(rs.dialogOptionNever) + '</span>\
				<span class="fc-dropdown">&nbsp;</span>\
			</span>\
		</div>\
		\
		<!-- Count of cycles -->\
		<div class="fc-repeat-cycles">\
			<span>'+htmlEscape(rs.dialogAfterLabel)+'</span>&nbsp;\
			<input class="fc-cycle-times" type="text" value="1">&nbsp;\
			<span>'+htmlEscape(rs.dialogTimesLabel)+'</span>\
		</div>\
		\
		<!-- End date -->\
		<div class="fc-end-date">\
			<div class="date-time">\
				<div>\
					<div class="wrapper">\
						<input class="to-date" type="text" value="8.03.2011"/><div class="to cal-icon"/>\
						<input class="to-time hidden" type="text" value="00:00"/>\
					</div>\
				</div>\
			</div>\
		</div>\
	</div>\
	<div class="buttons">\
		<a class="edit-btn button blue middle" href="#">'+htmlEscape(e.dialogButton_edit)+'</a>\
		<a class="save-btn button blue middle" href="#">'+htmlEscape(e.dialogButton_save)+'</a>\
		<a class="close-btn button blue middle" href="#">'+htmlEscape(e.dialogButton_close)+'</a>\
		<a class="cancel-btn button gray middle" href="#">'+htmlEscape(e.dialogButton_cancel)+'</a>\
		<a class="delete-btn button gray middle" href="#">'+htmlEscape(e.dialogButton_delete)+'</a>\
		<a class="unsubs-btn button gray middle" href="#">'+htmlEscape(e.dialogButton_unsubscribe)+'</a>\
	</div>\
	<div class="end-point"></div>\
</div>').replace(/\>\s+\</g, "><");
	
	options.deleteSettings.dialogTemplate = ('\
<div id="fc_delete_settings">\
	<div class="header">\
		<div class="inner">\
			<span>'+htmlEscape(ds.dialogHeader)+'</span>\
			<div class="close-btn">&nbsp;</div>\
		</div>\
	</div>\
	<div class="delete-selector">\
		<input class="delete-this" type="radio" name="delete-radio" value="" checked>&nbsp;<span class="delete-this-label">'+htmlEscape(ds.dialogDeleteOnlyThisLabel)+'</span><br/>\
		<input class="delete-following" type="radio" name="delete-radio" value="">&nbsp;<span class="delete-following-label">'+htmlEscape(ds.dialogDeleteFollowingLabel)+'</span><br/>\
		<input class="delete-all" type="radio" name="delete-radio" value="">&nbsp;<span class="delete-all-label">'+htmlEscape(ds.dialogDeleteAllLabel)+'</span>\
	</div>\
	<div class="buttons">\
		<a class="save-btn button blue middle" href="#">'+htmlEscape(ds.dialogButton_save)+'</a>\
		<a class="cancel-btn button gray middle" href="#">'+htmlEscape(ds.dialogButton_cancel)+'</a>\
	</div>\
</div>').replace(/\>\s+\</g, "><");

	options.icalStream.dialogTemplate = ('\
<div id="fc_ical_stream">\
	<div class="header">\
		<div class="inner">\
			<span>'+htmlEscape(ic.dialogHeader)+'</span>\
			<div class="close-btn">&nbsp;</div>\
		</div>\
	</div>\
	<div class="ical-description">\
		<span>'+htmlEscape(ic.dialogDescription)+'</span>\
	</div>\
	<div class="saved-url-link">\
	</div>\
	<div class="buttons">\
		<a class="cancel-btn button gray middle" href="#">'+htmlEscape(ic.dialogButton_close)+'</a>\
	</div>\
</div>').replace(/\>\s+\</g, "><");

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
	t.editEvent = editEvent;
	t.addAndEditEvent = addAndEditEvent;
	t.addNewCalendar = addNewCalendar;
	t.addiCalCalendar = addiCalCalendar;
	t.isEditingEvent = isEditingEvent;
	t.showTodo = showTodo;


	// imports
	EventManager.call(t, options, eventSources);
	var isFetchNeeded = t.isFetchNeeded;
	var fetchEvents = t.fetchEvents;


	// locals
	var _element = element[0];
	var catlist;
	var todolist;
	var eventEditor;
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
						'<td class="fc-catlist"/>' +
						'<td class="fc-catlist-sp"><div>&nbsp;</div></td>' +
						'<td class="fc-main"/>' +
						// temporary desabled
						//'<td class="fc-todo-list"/>' +
					'</tr></tbody></table>' +
				'</div>'
		);

		header = new Header(t, options);
		headerElement = header.render();
		if (headerElement && headerElement.length > 0) {
			fc_container.prepend(headerElement);
		}

		catlist = new CategoriesList(t);
		fc_container.find("td.fc-catlist").append(catlist.render());

		var anim = false;
		fc_container.find("td.fc-catlist-sp").click(function() {
			if (anim) {return;}
			var spl       = $(this);
			var panel     = fc_container.find(".fc-catlist");
			var td        = panel.filter("td");
			var div       = panel.filter("div");
			var isVisible = !td.hasClass("hidden");
			function onToggle() {
				td.toggleClass("hidden").css("width", "").css("display", "");
				div.css("margin-left", "");
				updateSize();
				anim = false;
			}
			spl.toggleClass("hidden");
			td.css("width", "auto");
			if (!isVisible) {
				div.css("margin-left", "-" + options.categories.width + "px");
				td.css("display", "table-cell");
			}
			anim = true;
			div.animate(
					{"margin-left": isVisible ? "-" + options.categories.width + "px" : "0"},
					{"duration": 300, "complete": onToggle/*, "step": updateSize*/});
		});


		// temporary desabled
		//todolist = new TodoList(t);
		//fc_container.find("td.fc-todo-list").append(todolist.render());

		content = $(
				"<div class='fc-content' style='position:relative'>" +
					"<div class='fc-modal'/>" +
				"</div>")
				.appendTo(fc_container.find("td.fc-main"));

		eventEditor = new EventEditor(t, content.children(".fc-modal"));

		fc_container.appendTo(element);

		var savedView = $.cookie('fc_current_view');
		changeView(savedView && savedView.length > 0 && savedView.search(/\w/) >= 0 ?
				savedView : options.defaultView);

		$(window).resize(windowResize);

		catlist.showMiniCalendar(parseInt($.cookie('fc_show_minicalendar'), 10) !== 0);

		// needed for IE in a 0x0 iframe, b/c when it is resized, never triggers a windowResize
		if (!bodyVisible()) {
			lateRender();
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
		return _element.offsetWidth !== 0;
	}


	function bodyVisible() {
		return $('body')[0].offsetWidth !== 0;
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
					newViewElement = absoluteViewElement =
						$("<div class='fc-view fc-view-" + newViewName + "' style='position:absolute'/>")
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

			renderView(); // after height has been set, will make absoluteViewElement's position=relative, then set to null

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

	function updateSize() {
		header.resize();
		markSizesDirty();
		if (elementVisible()) {
			currentView.clearEvents();
			calcSize();
			setSize();
			unselect();
			currentView.renderEvents(events);
			currentView.sizeDirty = false;
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
			suggestedViewHeight = Math.max(options.height, options.minHeight || 0) -
					(headerElement ? headerElement.height() : 0) - vsides(content);
		}
		else {
			suggestedViewHeight = Math.max(
					Math.round(content.width() / Math.max(options.aspectRatio, .5)), options.minHeight || 0);
		}
	}


	function setSize(dateChanged, bResize) { // todo: dateChanged?
		ignoreWindowResize++;
		currentView.setHeight(suggestedViewHeight, dateChanged);
		if (absoluteViewElement) {
			absoluteViewElement.css('position', 'relative');
			absoluteViewElement = null;
		}
		currentView.setWidth(content.width(), dateChanged);
		ignoreWindowResize--;

		var oldWidth = content.width();
		catlist.resize(suggestedViewHeight);
		if (todolist && todolist.length > 0) {todolist.resize(suggestedViewHeight);}
		var newWidth = content.width();
		if (oldWidth != newWidth) {
			if (!bResize) {calcSize();}
			setSize(false, oldWidth > newWidth);
		}
	}


	function windowResize() {
		if (!ignoreWindowResize) {
			if (currentView.start) { // view has already been rendered
				var uid = ++resizeUID;
				setTimeout(function() { // add a delay
					if (uid == resizeUID && !ignoreWindowResize && elementVisible()) {
						if (elementOuterWidth != (elementOuterWidth = element.outerWidth())) {
							ignoreWindowResize++; // in case the windowResize callback changes the height
							updateSize();
							currentView.trigger('windowResize', _element);
							ignoreWindowResize--;
						}
					}
				}, 200);
			}else{
				// calendar must have been initialized in a 0x0 iframe that has just been resized
				lateRender();
			}
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
		if (event && event.source) {
			// update event
			event.newSourceId = event.sourceId;
			trigger("editEvent", t,
					$.extend(
							{
								action: kEventChangeAction,
								sourceId: event.source.objectId,
								newSourceId: event.source.objectId
							},
							event),
					function(response) {
						if (!response.result) {return;}
						rerenderEvents();
					});
			return;
		}
		rerenderEvents(eventID);
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


	function rerenderCategories() {
		catlist.rerenderList();
		catlist.updateDatepickerSize();
		if (todolist && todolist.length > 0) {todolist.rerenderList();}
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
		var l = $(
		'<div class="page-menu">'+
        '<ul class="menu-actions">'+
        '<li class="menu-main-button with-separator middle" title="">'+
            '<span class="main-button-text override new-event">'+ htmlEscape(options.addNewLabel) + '</span>'+
            '<span class="white-combobox">&nbsp;</span>'+
        '</li>'+
        '</ul>'+
        '</div>').appendTo(elem.find(".fc-header-navigate"));
        
		// add new event / add new iCal-calendar
		_addSelectorLabel = elem.find(".white-combobox");
		elem.find(".white-combobox").on("click",
		                    
			function (event) {
				fcMenus.hideMenus(fcMenus.modeMenuAddNew);
				fcMenus.modeMenuAddNew.popupMenu("open", _addSelectorLabel);
				event.stopPropagation();
			});
			
	    elem.find(".new-event").on("click",
			 function(event){
			    calendar.addAndEditEvent();event.stopPropagation();
			 });
			
			if (!fcMenus.modeMenuAddNew || fcMenus.modeMenuAddNew.length < 1) {
				fcMenus.modeMenuAddNew = $('<div id="fc_mode_menu"/>');
			} else {
				fcMenus.modeMenuAddNew.popupMenu("close");
				fcMenus.modeMenuAddNew.popupMenu("destroy");
			}
			fcMenus.modeMenuAddNew.popupMenu({
				anchor: "right,bottom",
				direction: "left,down",
				arrow: "up",
				//showArrow: false,
				closeTimeout: -1,
				cssClassName: "asc-popupmenu asc-popup-wide",
				items: [
					{
						label: calendar.options.eventEditor.newEventTitle,
						click: function() {
							calendar.addAndEditEvent();
						}
					},
					{
						label: calendar.options.categories.defaultTitle,
						click: function() {
							calendar.addNewCalendar( {pageX: "center", pageY: "center"} );
						}
					},
					{
						label: calendar.options.icalStream.newiCalTitle,
						click: function() {
							calendar.addiCalCalendar( {pageX: "center", pageY: "center"} );
						}
					}
				]
			});
	}

	function _renderToday(elem) {
		var op = calendar.options.categories;
		var today = new Date();
		var r = $(
				'<div class="fc-header-today">' +
					'<div class="date-box">' +
						'<div class="today-day">' + htmlEscape(formatDate(today, op.dayFormat)) + '</div>' +
						'<div class="today-month">' + htmlEscape(formatDate(today, op.monthFormat)) + '</div>' +
						'<div class="today-year">' + htmlEscape(formatDate(today, op.yearFormat)) + '</div>' +
					'</div>' +
				'</div>');
		r.find(".date-box").children().click(function() {calendar.today();});
		r.appendTo(elem.find(".fc-header-left"));
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
	}

	function _setCurrentView(mode) {
		_updateViewSelector(mode);
		if (_activeButtonName && _activeButtonName.length > 0) {
			_this.clickButton(_activeButtonName, mode);
		}
	}

	function _renderViewSelectors(elem) {
		elem.find(".fc-header-right").append(
				'<span class="fc-view-selector">' +
					'<span class="icon">&nbsp;</span>' +
					'<span class="label">' + htmlEscape(options.modes.calendarViewLabel) + '</span>' +
					'<span class="fc-dropdown">&nbsp;</span>' +
				'</span>' +
				'<span class="fc-calendar-buttons"/>' +
				'<span class="fc-list-buttons"/>');
		_modes[0] = elem.find(".fc-calendar-buttons")
				.append(_renderButton.call(_this, "agendaDay"))
				.append(_renderButton.call(_this, "agendaWeek"))
				.append(_renderButton.call(_this, "month"));
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
			listWeek:   "agendaWeek",
			listMonth:  "month",
			list:       "month",
			day:        "agendaDay"
		};
		_switchTable[1] = {
			agendaDay:  "listDay",
			agendaWeek: "listWeek",
			month:      "listMonth",
			day:        "listDay"
		};
		_nameToMode = {
			agendaDay:  0,
			agendaWeek: 0,
			month:      0,
			listDay:    1,
			listWeek:   1,
			listMonth:  1,
			list:       1
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
			        .append('<div class="fc-header-navigate" />')
					.append(
						$('<div class="fc-header"/>')
							.append(_renderSection.call(this, "left"))
							.append(_renderSection.call(this, "center"))
							.append(_renderSection.call(this, "right"))
					);
			// temporary desabled
			//_renderTodoLabel.call(this, element);
			_renderViewSelectors.call(this, element);
			_renderNewSelectorLabel.call(this, element);
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
		var h = element.find(".fc-header"),
				hw = h.width(),
				l = element.find(".fc-header-left").css("width", ""),
				lo = l.offset(),
				c = element.find(".fc-header-center").css("width", ""),
				co = c.offset(),
				r = element.find(".fc-header-right"),
				ro = r.offset();
		if (co.left + c.outerWidth(true) > ro.left) {
			l.css( "width", "auto" );
			c.css( "width",
			       Math.round( 100 * (ro.left - lo.left - l.outerWidth(true)) / hw ) + "%" );
		}
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
				
		_dialog.find(".ical-export .ical-link").click(function() {
			_openIcalStream.call(_this, {pageX: "center", pageY: "center"});
		});
	
		_dialog.find(".buttons .save-btn").click(function() {
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
		_getControl(kTitle).keyup(function() {fcUtil.validateInput(this, fcUtil.validateNonemptyString);});
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
				.addClass("dark-border")
				.popupFrame({
					anchor: "right,top",
					direction: "right,down",
					offset: "0,0",
					showArrow: false
				});
		
		_icalStream.find(".buttons .cancel-btn, .header .close-btn").click(function() {
			_closeIcalStream.call(_this, false);
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
		_dialog.find(".title .bullet").css("color", newColor);
	}

	function _setTextColor(newColor) {
		_setIconColor(newColor, _getControl(kTextColorBox));
	}

	function _setMode(mode) {
		
		currentMode = mode;
		
		switch (mode) {
			case kCreateMode:
			case kCreateUrlMode:
				_dialog.removeClass("edit-mode"); break;
			
			case kEditMode:
				_dialog.addClass("edit-mode"); break;
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


	function _open(anchor) {
		$(document).bind("keyup", _checkEscKey);
		_dialog.popupFrame("open", anchor);
		_dialog.find(".ical-file-selected").html(ic.dialogButton_fileNotSelected);
		_dialog.find(".ical-url-input input").val("");
		
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
		
		// upload ajax
		AjaxUploader = new AjaxUpload('#ical-browse-btn', {
			action: 'upload.php',
			autoSubmit: false,
			//responseType: "xml",
			onChange: function(file, extension) {
				
				if ( !(extension && ( /^(ics)$/.test(extension) || /^(txt)$/.test(extension) )) ){
					_dialog.find(".ical-file-selected").html(ic.dialog_incorrectFormat).css("color", "red");
					return;
				} 
				
				this.isChanged = true; 
				_dialog.find(".ical-file-selected").html(ic.dialogButton_fileSelected).css("color", "grey");
				},
			onSubmit: function(file, extension) {
				LoadingBanner.displayLoading(true); 
				},
			onComplete: function(file, response) { 
				LoadingBanner.hideLoading(true);
				calendar.refetchEvents();
				}
			});
		//
		
		// personal version
		_setPersonalMode.call(_this);
	}
	
	function _setPersonalMode() {
		var shared_list = _dialog.find(".shared-list");
		if (calendar.options.personal == true) {
			shared_list.addClass("hidden");
		}
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
					title:     _getControl(kTitle),
					bgColor:   _getControl(kColorBox),
					textColor: _getControl(kTextColorBox),
					alert:     _dialog.find(".alert select"),
					timezone:  _dialog.find(".timezone select"),
					iCalUrl:   _dialog.find(".ical-url-input input")
				},
				i;

		if (saveData) {     // ------------- SAVE data -------------

			if (false == fcUtil.validateInput(dlg.title, fcUtil.validateNonemptyString)) {return false;}
			
			if (currentMode == kCreateUrlMode) {
				if (false == fcUtil.validateInputHttp(dlg.iCalUrl, fcUtil.validateNonemptyString)) {return false;}
			}
			
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
			
			// iCal url
			if (currentMode == kCreateUrlMode) {
				_source.iCalUrl = dlg.iCalUrl.val();
			}

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

			var calCount = 0;
			$.each(calendar.getEventSources(), function(i,src) {
				if (src.objectId != undefined && !src.isSubscription) {++calCount;}
			});
			if (calCount > 1) {
				_dialog.find(".buttons").removeClass("read-only");
			} else {
				_dialog.find(".buttons").addClass("read-only");
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
		_setMode.call(_this, kEditMode);
		
		_dialog.find(".ical").removeClass("hidden");
		_dialog.find(".ical-export").removeClass("hidden");
		_dialog.find(".ical-import").removeClass("hidden");
		
		_dialog.find(".row").removeClass("hidden");
		_dialog.find(".shared-list").removeClass("hidden");
		_dialog.find(".ical-url-input").addClass("hidden");
		
		_open.call(_this, anchor);
	};

	this.addNew = function(anchor) {
		var opt = calendar.options;
		var categories = opt.categories;
		var newColor = fcUtil.parseCssColor(opt.eventTextColor);
		var newBg = fcUtil.parseCssColor(opt.eventBackgroundColor);
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
		
		_dialog.find(".ical").removeClass("hidden");
		_dialog.find(".ical-export").addClass("hidden");
		_dialog.find(".ical-import").removeClass("hidden");
		
		_dialog.find(".row").removeClass("hidden");
		_dialog.find(".shared-list").removeClass("hidden");
		_dialog.find(".ical-url-input").addClass("hidden");
		
		_open.call(_this, anchor);
	};
	
	this.addiCalCalendar = function(anchor) {
		var opt = calendar.options;
		var categories = opt.categories;
		var newColor = fcUtil.parseCssColor(opt.eventTextColor);
		var newBg = fcUtil.parseCssColor(opt.eventBackgroundColor);
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
		
		_setMode.call(_this, kCreateUrlMode);
		_doDDX.call(_this);
		
		_dialog.find(".ical").addClass("hidden");
		
		_dialog.find(".row").addClass("hidden");
		_dialog.find(".shared-list").addClass("hidden");
		_dialog.find(".ical-url-input").removeClass("hidden");
		
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
			_openIcalStream.call(_this, {pageX: "center", pageY: "center"});
		});
				
		_dialog.find(".buttons .save-btn").click(function() {
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
				.addClass("dark-border")
				.popupFrame({
					anchor: "right,top",
					direction: "right,down",
					offset: "0,0",
					showArrow: false
				});
		
		_icalStream.find(".buttons .cancel-btn, .header .close-btn").click(function() {
			_closeIcalStream.call(_this, false);
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
		_dialog.find(".title .bullet").css("color", newColor);
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
			_dialog.find(".ical-export").removeClass("hidden");
			
			_dialog.find(".ical-saved-url").addClass("hidden");
			_dialog.find(".buttons").find(".delete-btn").addClass("hidden");
			_dialog.find(".saved-url-link").html("");
		}
		
		if (_source.isEditable) {
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
					_dialog.find(".ical-file-selected").html(ic.dialog_incorrectFormat).css("color", "red");
					return;
				}
				
				this.isChanged = true; 
				_dialog.find(".ical-file-selected").html(ic.dialogButton_fileSelected).css("color", "grey");
				},
			onSubmit: function(file, extension) {
				LoadingBanner.displayLoading(true); 
				},
			onComplete: function(file, response) { 
				LoadingBanner.hideLoading(true);
				calendar.refetchEvents();
				}
			});
		//
		
		// personal version
		_setPersonalMode.call(_this);
	}
	
	function _setPersonalMode() {
		var owner = _dialog.find(".owner");
		var shared_list = _dialog.find(".shared-list");
		if (calendar.options.personal == true) {
			owner.addClass("hidden");
			shared_list.addClass("hidden");
		}
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

			if (_source.owner && _source.owner.name && _source.owner.name.length > 0) {
				dlg.owner.html(htmlEscape(_source.owner.name));
				_dialog.find(".owner").removeClass("hidden");
			} else {
				dlg.owner.text("");
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
					_dialog.find(".shared-list").addClass("has-users").append(
							'<div class="users-list">' +
								'<div class="label">' + htmlEscape(calendar.options.sharedList.title) + '</div>' +
								'<div class="short-list">' + htmlEscape(list) + '</div>' +
							'</div>');
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
					$('<li><input type="checkbox" name="checkedSubscriptions"' +
							' value="' + sList[group][i].index + '"'
							+ (subscription.isSubscribed ? ' checked="checked"' : '') + '/> '
							+ htmlEscape(subscription.title) + '</li>')
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

			_dialog.find(".groups .scroll-area").jScrollPane();

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
				$.extend({action: kCalendarDeleteAction}, source),
				function(response) {
					if (!response.result) {
						return;
					}
					calendar.removeEventSource(source);
					calendar.rerenderEvents();
					_rerenderList.call(_this);
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
							
							calendar.rerenderEvents();
						});
			} else {
				// Add new category
				calendar.trigger("editCalendar", _this,
						$.extend({action: kCalendarAddAction}, source),
						function(response) {
							if (!response.result) {return;}
							
							if (AjaxUploader.isChanged != undefined) {
								AjaxUploader._settings.action = response.importUrl;
								AjaxUploader.submit();
							}
							
							calendar.addEventSource(response.source);
							_rerenderList.call(_this, kCategoriesOnly, true);
							_rerenderList.call(_this, kSubscriptionsOnly, true);
							calendar.refetchEvents();
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
					'<div class="label">' +
						htmlEscape(item.title ? item.title : 'Unknown') + '</div>' +
					'<div class="edit-icon"/>' +
				'</div>')
				.data("sourceIndex", index);
	}

	function _ellipsisLongItem() {
		var li = $(this);
		var label = li.find(".label")
				.css("right", "")
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
		var jsp = list.find(".categories").data("jsp");
		var categ = jsp.getContentPane();
		categ.empty();

		for (var i = 0; i < sources.length; ++i) {
			var cal = sources[i];
			if (!cal.title || cal.title.length < 1 || cal.isSubscription) {continue;}
			cal.isHidden = cal.isHidden == true ? true : false; // convert to boolean

			var li = _renderListItem.call(_this, i, cal);
			li.find(".bullet").click(function() {
				_toggleListItem.call(_this, $(this).parent().get(0));
			});
			li.find(".edit-icon").click(function() {
				_editCategory.call(_this, $(this).parent().get(0));
			});
			li.appendTo(categ);
		}

		jsp.reinitialise();
	}

	function _renderSubscriptions(list) {
		var sources = calendar.getEventSources();
		var jsp = list.find(".subscr").data("jsp");
		var subscr = jsp.getContentPane();
		subscr.empty();

		for (var i = 0; i < sources.length; ++i) {
			var sub = sources[i];
			if (!sub.title || sub.title.length < 1 || !sub.isSubscription) {continue;}
			sub.isHidden = sub.isHidden == true ? true : false; // convert to boolean

			var li = _renderListItem.call(_this, i, sub);
			li.find(".bullet").click(function() {
				_toggleListItem.call(_this, $(this).parent().get(0));
			});
			li.find(".edit-icon").click(function() {
				_editSubscription.call(_this, $(this).parent().get(0));
			});
			li.appendTo(subscr);
		}

		jsp.reinitialise();
	}

	function _renderList() {
		var list = $("div.fc-catlist .content");
		if (list.length < 1) {
			list = $(
				'<div class="content">' +
					'<div class="content-h first">' +
						'<span class="main-label">' + htmlEscape(calendar.options.categories.title) + '</span>' +
						<!--'<span class="add-label" title="' + htmlEscape(calendar.options.categories.addNewCategoryLabel) + '"/>' + -->
					'</div>' +
					'<div class="categories"/>' +
					'<div class="content-h">' +
						'<span class="main-label">' + htmlEscape(calendar.options.categories.subscriptionsTitle) + '</span>' +
						'<span class="manage-label" title="' + htmlEscape(calendar.options.categories.subscriptionsManageLabel) + '"/>' +
					'</div>' +
					'<div class="subscr"/>' +
				'</div>');
			//list.find(".add-label").click(function() {_addNewCategory.call(_this);});
			list.find(".manage-label").click(function() {_manageSubscriptions.call(_this);});
			list.find(".categories, .subscr").jScrollPane().css("padding", "");
		}
		_renderCategories.call(_this, list);
		_renderSubscriptions.call(_this, list);
		return list;
	}

	function _resizeList(scrollToEnd) {
		if (_list.length < 1) {return;}

		_list.find(".content-li").each(_ellipsisLongItem);

		_list.css("visibility", "hidden");
		_list.removeClass("fixed");

		// find lists
		var categ = _list.find(".categories");
		var categItems = categ.find(".content-li");
		var subscr = _list.find(".subscr");
		var subscrItems = subscr.find(".content-li");

		// restore heights of lists
		var cih = categItems.eq(0).outerHeight(true);
		var categH = categItems.length * cih;
		var minCategH = categItems.length > 1 ? 2 * cih : cih;
		var sih = subscrItems.eq(0).outerHeight(true);
		var subscrH = subscrItems.length * sih;
		var minSubscrH = subscrItems.length > 1 ? 2 * sih : sih;

		categ.height(categH);
		subscr.height(subscrH);

		// make datepicker sticky
		var pickerDiv = _content.find(".fc-catlist-picker").removeClass("fixed");
		var makeSticky =
				pickerDiv.position().top + pickerDiv.outerHeight(true) > _content.height();
		if (makeSticky) {
			pickerDiv.addClass("fixed");
			_list.addClass("fixed");
			_list.css("bottom", pickerDiv.outerHeight(true) + "px");
		}

		// calc new heights
		var listH = _list.height();
		var otherH = _list.find(".date-box").outerHeight(true) +
				_list.find(".content-h").outerHeight(true) * 2;
		var scrollH = listH - otherH;
		var categSpace = categ.outerHeight(true) - categ.height();
		var subscrSpace = subscr.outerHeight(true) - subscr.height();
		var newCategH = Math.max(minCategH, Math.round(scrollH * categH / (categH + subscrH)));
		var newSubscrH = Math.max(minSubscrH, Math.round(scrollH * subscrH / (categH + subscrH)));
		var delta = newCategH + newSubscrH - scrollH;
		if (delta > 0) {
			if (newCategH > newSubscrH) {newCategH = newCategH - delta;}
			else                        {newSubscrH = newSubscrH - delta;}
		}

		categ.css("height", (newCategH - categSpace) + "px");
		subscr.css("height", (newSubscrH - subscrSpace) + "px");

		var categJSP = categ.data("jsp");
		categJSP.reinitialise();
		subscr.data("jsp").reinitialise();

		if (scrollToEnd) {
			categJSP.scrollToBottom();
		}

		_list.css("visibility", "visible");
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
				_dpIgnoreUpdate = true;
				_dpVisibleDate = _datepicker.datepicker("getDate");
				calendar.gotoDate(_dpVisibleDate);
				calendar.changeViewAndMode("day");
				_dpIgnoreUpdate = false;
				//
				setTimeout(_updateDatepickerCells, 0);
			}
		});

		function initMonthMenu (directionUp) {
		    fcMenus.dpMonthMenu = $("#fc_datepicker_mmenu").length ? $("#fc_datepicker_mmenu") : $("<div id='fc_datepicker_mmenu'/>");
			fcMenus.dpMonthMenu.popupMenu({
				anchor: directionUp ? "left,top" : "right,bottom",
				direction: directionUp ? "right,up" : "left,down",
				arrow: directionUp ? "down" : "up",
				arrowPosition: "50%",
				cssClassName: "asc-popupmenu",
				items: calendar.options.monthNames,
				itemClick: function (event, data) {
					var delta = data.itemIndex - _dpVisibleDate.getMonth();
					adjustDatepickerDate(delta, "M");
				}
			});
		}

		function initYearMenu (y, directionUp) {
			fcMenus.dpYearMenu.popupMenu({
				anchor: directionUp ? "left,top" : "right,bottom",
				direction: directionUp ? "right,up" : "left,down",
				arrow: directionUp ? "down" : "up",
				arrowPosition: "50%",
				cssClassName: "asc-popupmenu",
				items: [
					(y - 2).toString(),
					(y - 1).toString(),
					"divider",
					(y + 1).toString(),
					(y + 2).toString()
				],
				itemClick: function(event, data) {
					var delta = parseInt(data.item, 10) - y;
					adjustDatepickerDate(delta, "Y");
				}
			});
		}

		function adjustDatepickerDate(delta, period) {
			if (delta != 0) {
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

		initMonthMenu(true);

		$(document).on("click", "#fc_datepicker .ui-datepicker-title", function(e) {
			var directionUp = $(e.target).offset().top > fcMenus.dpMonthMenu.outerHeight(true) + 10;
			if (e.target.className == "ui-datepicker-month") {
				initMonthMenu(directionUp);
				fcMenus.hideMenus(fcMenus.dpMonthMenu);
				fcMenus.dpMonthMenu.popupMenu("open", e.target);
				return;
			}
			if (e.target.className == "ui-datepicker-year") {
				//always render YearMenu
				//if (!_dpYearMenuCaller || _dpYearMenuCaller != e.target) {
					_dpYearMenuCaller = e.target;
					var y = _dpVisibleDate.getFullYear();
					if (!fcMenus.dpYearMenu || fcMenus.dpYearMenu.length < 1) {
						fcMenus.dpYearMenu = $("#fc_datepicker_ymenu").length ? $("#fc_datepicker_ymenu") : $("<div id='fc_datepicker_ymenu'/>");
					} else {
						fcMenus.dpYearMenu.popupMenu("close");
						fcMenus.dpYearMenu.popupMenu("destroy");
					}
					initYearMenu(y, directionUp);
				//}
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

		var pickerDiv = _content.find(".fc-catlist-picker");
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

		var pickerH = _content.find(".fc-catlist-picker").outerHeight(true);
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
				.append(_list)
				.append(_renderDatepicker.call(_this));

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
		if (!_content.is(":visible")) {return;}
		_rerenderList.call(_this);
	};

	this.resize = function(newHeight) {
		if (!_content.is(":visible")) {return;}
		_content.css("height", newHeight + "px");
		_resizeList.call(_this);
	};

	this.updateDatepicker = function(newDate) {
		if (!_content.is(":visible")) {return;}
		if (_datepicker.length < 1 || _dpIgnoreUpdate) {return;}

		_dpSettingDate = true;
		_dpVisibleDate = newDate;
		_datepicker.datepicker("setDate", newDate);
		_dpSettingDate = false;

		_updateDatepickerCells.call(_this);
	};

	this.updateDatepickerSize = function() {
		if (!_content.is(":visible")) {return;}
		_updateDatepickerSize.call(_this);
	};

	this.updateDatepickerCells = function() {
		if (!_content.is(":visible")) {return;}
		_updateDatepickerCells.call(_this);
	};

	this.showMiniCalendar = function(show) {
		if (!_content.is(":visible")) {return;}
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
	var _editor;
	var _curTodoElem;
	var _hideCompleted = false;
	var _sortType = 0;
	var _sortLabel;
	var _uiBlocker;

	function _doDDX(save) {
		var todo = {
			title:       _editor.find("#fc_todo_title"),
			completed:   _editor.find("#fc_todo_completed"),
			priority:    _editor.find("#fc_todo_priority"),
			cal:         _editor.find("#fc_todo_cal"),
			description: _editor.find("#fc_todo_description"),
			ok:          _editor.find("#fc_todo_ok")
		};
		var elem = $(_curTodoElem);
		var sources = calendar.getEventSources();
		var src = sources[elem.data("sourceIndex")];
		var newSource;
		var curTodo = src.todos[elem.data("todoId")];

		if (save) {    // ------------- SAVE data -------------

			curTodo.title = $.trim(todo.title.val());
			curTodo.completed = todo.completed.is(":checked");
			curTodo.description = $.trim(todo.description.val());
			curTodo.priority = todo.priority.val();

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

			_curTodoElem = undefined;
			_renderList.call(_this);

		} else {       // ------------- LOAD data -------------

			todo.title.val(curTodo.title);

			if (curTodo.completed == true) {
				todo.completed.prop("checked", true);
			} else {
				todo.completed.removeAttr("checked");
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
		_editor.popupFrame("open", todoElem);
	}

	function _closeEditor() {
		_editor.popupFrame("close");
		_uiBlocker.hide();
		_list.removeClass("modal");
	}

	function _compileTodos() {
		var todos = [];
		var sources = calendar.getEventSources();
		for (var i = 0; i < sources.length; ++i) {
			var source = sources[i];
			if (source.todos && source.todos.length > 0) {
				for (var j = 0; j < source.todos.length; ++j) {
					source.todos[j].source = source;
					source.todos[j].sourceIndex = i;
					source.todos[j].id = j;
				}
				todos = todos.concat(source.todos);
			}
		}
		todos.sort(function(left, right) {
					var res = 0;
					if (_sortType === 0) {          // sort by calendar
						res = left.source.title.toLowerCase() < right.source.title.toLowerCase() ? -1 : 1;
					} else if (_sortType === 1) {   // sort by priority
						var p1 = left.priority != undefined ? left.priority : 2;
						var p2 = right.priority != undefined ? right.priority : 2;
						res = p1 < p2 ? -1 : 1;
					} else if (_sortType === 2) {   // sort alphabetically
						res = left.title.toLowerCase() < right.title.toLowerCase() ? -1 : 1;
					}
					return res;
				});
		return todos;
	}

	function _renderList() {
		var items = _list.find(".content .items");
		items.empty();
		//
		var todos = _compileTodos.call(_this);
		for (var j = 0; j < todos.length; ++j) {
			var todo = todos[j];
			if (todo.completed && _hideCompleted) {continue;}
			var li = $(
					'<div class="content-li">' +
						'<span class="bullet" style="color:' + htmlEscape(todo.source.isHidden ?
								calendar.options.categories.inactiveColor : todo.source.backgroundColor) +
								';">' +
							htmlEscape(calendar.options.categories.itemBullet) + '&nbsp;</span>' +
						'<span class="label">' +
							htmlEscape(todo.title ? todo.title : 'Unknown') + '</span>' +
					'</div>');
			li.find(".label").click(function() {
				_openEditor.call(_this, $(this).parent());
			});
			li.find(".bullet").click(function() {
				_completeTodo.call(_this, $(this).parent());
			});
			if (todo.completed) {
				li.addClass("completed");
			}
			li.data({"sourceIndex": todo.sourceIndex, "todoId": todo.id});
			li.appendTo(items);
		}
	}

	function _renderMenu() {
		if (!fcMenus.todoMenu || fcMenus.todoMenu.length < 1) {
			fcMenus.todoMenu = $("<div id='fc_todo_menu'/>");
		} else {
			fcMenus.todoMenu.popupMenu("close");
			fcMenus.todoMenu.popupMenu("destroy");
		}
		fcMenus.todoMenu.popupMenu({
			anchor: "right,bottom",
			direction: "left,down",
			arrow: "up",
			cssClassName: "asc-popupmenu",
			items: [
				{
					label: calendar.options.todoList.menu.hideColmpletedTodos.hide,
					click: function(event, data) {
						_hideCompletedTodos.call(_this);
					},
					state: 1,
					toggle: function(event, data) {
						if (data.item.state == 1) {
							data.item.label = calendar.options.todoList.menu.hideColmpletedTodos.show;
							data.item.state = 0;
						} else {
							data.item.label = calendar.options.todoList.menu.hideColmpletedTodos.hide;
							data.item.state = 1;
						}
					}
				},
				{
					label: calendar.options.todoList.menu.deleteCompletedTodos,
					click: function(event, data) {
						_deleteCompletedTodos.call(_this);
					}
				}
			]
		});
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

	function _renderTodoEditor() {
		$.get(
				calendar.options.todoList.todoEditorUrl,
				function(tmpl) {
					_editor = $(tmpl)
							.addClass("asc-dialog")
							.addClass("fc-shadow")
							.popupFrame({
									anchor: "left,top",
									direction: "left,down",
									arrow: "right",
									arrowPosition: "50%"});
					_editor.find("#fc_todo_ok").button().click(function() {
						_doDDX.call(_this, true);
						_closeEditor.call(_this);
					});
					_editor.find("#fc_todo_cancel").button().click(function() {
						_closeEditor.call(_this);
					});
				});
	}

	function _addNewTodo() {
		var sources = calendar.getEventSources();
		if (sources.length < 1) {return null;}
		var todo = {
			title: calendar.options.todoList.newTodoTitle,
			completed: false
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

	function _hideCompletedTodos() {
		_hideCompleted = !_hideCompleted;
		_renderList.call(_this);
	}

	function _completeTodo(todoElem) {
		var elem = $(todoElem);
		var source = calendar.getEventSources()[elem.data("sourceIndex")];
		var todo = source.todos[elem.data("todoId")];
		todo.completed = !todo.completed;
		_curTodoElem = undefined;
		_renderList.call(_this);
	}


	// Public Interface

	this.render = function() {
		var result = $(
				"<div>" +
					"<div class='fc-todo-list'>" +
						"<div class='content'>" +
							"<div class='content-h'>" +
								"<span class='label'>" + htmlEscape(calendar.options.todoList.title) + "</span>" +
								"<span class='fc-dropdown'>&nbsp;</span>" +
								"<span class='add-btn'>" +
									htmlEscape(calendar.options.todoList.addTodoLabel) +
								"</span>" +
							"</div>" +
							"<div class='items'/>" +
							"<div class='sort-label'>" +
								"<span class='label'>" +
									htmlEscape(calendar.options.todoList.sortByCalendarLabel) + "</span>" +
								"<span class='fc-dropdown'>&nbsp;</span>" +
							"</div>" +
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
		result.find(".content-h .label, .content-h .fc-dropdown").click(function() {
					fcMenus.hideMenus(fcMenus.todoMenu);
					fcMenus.todoMenu.popupMenu("open", $(this).parent().find(".label"));
				});
		result.find(".sort-label .label, .sort-label .fc-dropdown").click(function() {
					fcMenus.hideMenus(fcMenus.sortMenu);
					fcMenus.sortMenu.popupMenu("open", $(this).parent().find(".label"));
				});
		_uiBlocker = result.find(".fc-modal").click(function() {
					_closeEditor.call(_this);
				});

		_renderList.call(this);
		_renderMenu.call(this);
		_renderSortMenu.call(this);
		_renderTodoEditor.call(this);

		return result.children();
	};

	this.rerenderList = function() {
		_renderList.call(this);
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

	var _modes = {
		view:						_viewMode,
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
		single:          0,
		allFollowing:    1,
		allSeries:       2
	}
	
	var _dialog;

	var alertType = kAlertDefault;								// number
	var repeatRule = ASC.Api.iCal.RecurrenceRule.EveryDay		// standart or custom object rules
	var dwm_current = dwm.day;
	var dayRuleObject = undefined;
	
	var _settings;
	var _delSettings;

	var _dialogMode;
	var _permissionsList;

	var _anchor;
	var _eventObj;
	var _canEdit;
	var _canDelete;
	var _canChangeSource;
	var _canUnsubscribe;

	var formatDate = calendar.formatDate;
	var formatDates = calendar.formatDates;

	(function _create() {
		_dialog = $(calendar.options.eventEditor.dialogTemplate)
				.addClass("asc-dialog")
				.addClass("fc-shadow")
				.popupFrame({
					anchor: "right,top",
					direction: "right,down",
					offset: "-2px,0",
					arrow: "left",
					arrowPosition: "50%"
				});
		
		_dialog.find(".buttons .edit-btn").click(function() {
			if (_canEdit) {
				_resetMode.call(_this);
				_dialogMode = false;
				_dialog.addClass("edit-popup");
				_dialog.popupFrame("updatePosition", _anchor);
				
				_enableRepeatSettings.call(_this);
				_setRepeatSettingsHeight.call(_this);
			}
		});
		_dialog.find(".buttons .save-btn").click(function() {
			var hasSettings = !_settings.hasClass("hidden");
			if (hasSettings) {
				if (!_validateDateFieldsSettings.call(_this)) {
					return;
				}
				_closeSettings.call(_this, hasSettings);
			}
			
			_close.call(_this, true);
		});
		_dialog.find(".buttons .cancel-btn, .buttons .close-btn, .header .close-btn").click(function() {
			_close.call(_this, false);
		});
		_dialog.find(".buttons .delete-btn").click(function() {
			if (_canDelete) {
				if (_eventObj.repeatRule.Freq == ASC.Api.iCal.Frequency.Never) {
					_deleteEvent.call(_this, deleteMode.single, _eventObj._start);
					_close.call(_this, false, true);
				}
				else {
					_openDelSettings.call(_this, "addPopupDelSettings");
				}
			}
		});
		_dialog.find(".buttons .unsubs-btn").click(function() {
			if (_canUnsubscribe) {
			    _unsubscribeEvent.call(_this);
			}
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
		_dialog.find(".editor .from-time, .editor .to-time").keyup(function(ev) {
			if (ev.keyCode == 16 || ev.keyCode == 17 || ev.keyCode >= 33 && ev.keyCode <= 40) {return;}
			var inp = $(this);
			var inpV = inp.val().replace(/\s+|[^:\d]+/g, "");
			var ci = inpV.indexOf(":");
			if (ci >= 0) {
				inpV = inpV.substring(0, ci + 1) + inpV.substring(ci + 1).replace(/:/g, "");
				if (inpV.search(/^\d{1,2}:\d{1,2}$/) < 0) {
					inpV = inpV.replace(/:/g, "").replace(/^(\d\d|\d)(\d*)/, "$1:$2");
				}
			} else if (inpV.length > 1) {
				inpV = inpV.substring(0, 2) + ":" + inpV.substring(2);
			}
			if (inpV.length > 5) {
				inpV = inpV.substring(0, 5);
			}
			if (inp.val() != inpV) {inp.val(inpV);}
		});
		_dialog
				.find(".editor .from-date, .editor .from-time, .editor .to-date, .editor .to-time")
				.bind("keyup change", _validateDateFields);
		//
		_dialog.find(".editor .calendar select").change(function (ev) {
			var v = $(this).val();
			var s = calendar.getEventSources();
			for (var i = 0; i < s.length; ++i) {
				if (s[i].objectId != v) {continue;}
				_dialog.find(".editor .calendar .bullet").css("color", s[i].backgroundColor);
				return;
			}
		});
		//
		if (uiBlocker && uiBlocker.length > 0) {
			uiBlocker.click(function() {
				_close.call(_this, false);
				_closeSettings.call(_this, false);
				_closeDelSettings.call(_this, false);
				});
		}
	}());
	
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
			_closeSettings.call(_this, true);
		});
		_settings.find(".buttons .cancel-btn, .buttons .close-btn, .header .close-btn").click(function() {
			_closeSettings.call(_this, false);
		});
		
		// day/week/month selector
		_DWMSelectorLabel = _settings.find(".fc-dwm-selector");
		
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
				fcMenus.modeMenuDWM = $('<div id="fc_mode_menu"/>');
			} else {
				fcMenus.modeMenuDWM.popupMenu("close");
				fcMenus.modeMenuDWM.popupMenu("destroy");
			}
			fcMenus.modeMenuDWM.popupMenu({
				anchor: "left,bottom",
				direction: "right,down",
				arrow: "up",
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
		_endRepeatSelectorLabel = _settings.find(".fc-endrepeat-selector");
		
		_settings.find(".fc-endrepeat-selector").click(
			function (elem) {
				
				if ($(this).find(".fc-selector-link").hasClass("not-active")) {
					return false;
				}
				
				fcMenus.hideMenus(fcMenus.modeMenuEndRepeat);
				fcMenus.modeMenuEndRepeat.popupMenu("open", _endRepeatSelectorLabel);
				event.stopPropagation();
			});
			
			if (!fcMenus.modeMenuEndRepeat || fcMenus.modeMenuEndRepeat.length < 1) {
				fcMenus.modeMenuEndRepeat = $('<div id="fc_mode_menu"/>');
			} else {
				fcMenus.modeMenuEndRepeat.popupMenu("close");
				fcMenus.modeMenuEndRepeat.popupMenu("destroy");
			}
			fcMenus.modeMenuEndRepeat.popupMenu({
				anchor: "left,bottom",
				direction: "right,down",
				arrow: "up",
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
				if (s[i].objectId != v) {continue;}
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
			//_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-following"), false);
			//_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-all"), false);
		});
		
		_delSettings.find(".delete-selector .delete-following-label").click(function() {
			//_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-this"), false);
			_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-following"), true);
			//_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-all"), false);
		});
		
		_delSettings.find(".delete-selector .delete-all-label").click(function() {
			//_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-this"), false);
			//_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-following"), false);
			_setCheckedAttrValue.call(_this, _delSettings.find(".delete-selector .delete-all"), true);
		});
		//
		
		_delSettings.find(".buttons .save-btn").click(function() {
			var delType = deleteMode.single;
			if (_delSettings.find(".delete-following").is(":checked")) {
				delType = deleteMode.allFollowing;
			}
			else if (_delSettings.find(".delete-all").is(":checked")) {
				delType = deleteMode.allSeries;
			}
			
			_deleteEvent.call(_this, delType, _eventObj._start);
			_closeDelSettings.call(_this, true);
			_close.call(_this, false, true);
		});
		_delSettings.find(".buttons .cancel-btn, .buttons .close-btn, .header .close-btn").click(function() {
			_closeDelSettings.call(_this, false);
		});
		
		
		//
		if (uiBlocker && uiBlocker.length > 0) {
			uiBlocker.click(function() {_close.call(_this, false);});
		}
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
				var m = _dialog.popupFrame("option", "offset").match(/^([+-]?\d+)(px)?,([+-]?\d+)(px)?/i);
				var newOffs = m[1] + (m[2] ? m[2] : "") + "," +
						(vr.top - ar.top + parseInt(m[3], 10)) + (m[4] ? m[4] : "");
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
		_dialog.removeClass("add-dialog add-popup edit-popup");
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
		_repeatSelectorLabel = _dialog.find(".fc-view-repeat");
		
		_dialog.find(".fc-view-repeat").click(
			function (event) {
				fcMenus.hideMenus(fcMenus.modeMenuRepeat);
				fcMenus.modeMenuRepeat.popupMenu("open", _repeatSelectorLabel);
				event.stopPropagation();
			});
			
			if (!fcMenus.modeMenuRepeat || fcMenus.modeMenuRepeat.length < 1) {
				fcMenus.modeMenuRepeat = $('<div id="fc_mode_menu"/>');
			} else {
				fcMenus.modeMenuRepeat.popupMenu("close");
				fcMenus.modeMenuRepeat.popupMenu("destroy");
			}
			fcMenus.modeMenuRepeat.popupMenu({
				anchor: "left,bottom",
				direction: "right,down",
				arrow: "up",
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
		_alertSelectorLabel = _dialog.find(".fc-view-alert");
		
		_dialog.find(".fc-view-alert").click(
			function (event) {
				fcMenus.hideMenus(fcMenus.modeMenuAlert);
				fcMenus.modeMenuAlert.popupMenu("open", _alertSelectorLabel);
				event.stopPropagation();
			});

			if (!fcMenus.modeMenuAlert || fcMenus.modeMenuAlert.length < 1) {
				fcMenus.modeMenuAlert = $('<div id="fc_mode_menu"/>');
			} else {
				fcMenus.modeMenuAlert.popupMenu("close");
				fcMenus.modeMenuAlert.popupMenu("destroy");
			}
			fcMenus.modeMenuAlert.popupMenu({
				anchor: "left,bottom",
				direction: "right,down",
				arrow: "up",
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
	}

	function _renderPermissions() {
		_eventObj.permissions = _eventObj.permissions || {};
		_eventObj.permissions.users = _eventObj.permissions.users || [];

		var list = "";
		for (var i = 0; i < _eventObj.permissions.users.length; i++) {
			if (i > 0) {list += ", ";}
			list += _eventObj.permissions.users[i].name;
		}

		_dialog.find(".viewer .shared-list .users-list").html(htmlEscape(list));

		if (list.length > 0) {
			_dialog.find(".viewer .shared-list").addClass("has-users");
		} else {
			_dialog.find(".viewer .shared-list").removeClass("has-users");
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
		var inputTxt = _dialog.find(".editor .title input");
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
		
		// personal version
		_setPersonalMode.call(_this);
		
		_anchor = elem;
		_modes[mode].call(_this, elem, eventObj);
		
		_setRepeatSettingsHeight.call(_this);
		
		$(document).bind("keyup", _checkEscKey);

		if (_dialog.popupFrame("isVisible")) {
			uiBlocker.show();
		} else {
			uiBlocker.hide();
		}
	}
	
	function _setPersonalMode() {
		var dlg = {
			viewer: {
				owner:				_dialog.find(".viewer .owner"),
				shared_list:		_dialog.find(".viewer .shared-list")
			},
			editor: {
				shared_list:		_dialog.find(".editor .shared-list")
			}
		};
		
		// personal version
		if (calendar.options.personal == true) {
			dlg.viewer.owner.addClass("hidden");
			dlg.viewer.shared_list.addClass("hidden");
			dlg.editor.shared_list.addClass("hidden");
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
	
	function _openDelSettings(mode)
	{
		_delSettings.popupFrame("close");
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
		_dialog.popupFrame("close");
		uiBlocker.hide();
	}
	
	function _closeDialogDelSettings() {
		_delSettings.popupFrame("close");
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
			frc  = frDate.date.isValid ? "" : "red";
			frtc = frDate.time.isValid ? "" : "red";
			toc  = toDate.date.isValid && delta ? "" : "red";
			totc = toDate.time.isValid && delta ? "" : "red";
		}
		dlg.from.css("color", "").css("border-color", frc);
		dlg.from_t.css("color", "").css("border-color", frtc);
		dlg.to.css("color", "").css("border-color", toc);
		dlg.to_t.css("color", "").css("border-color", totc);
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
				frc  = frDate.date.isValid ? "" : "red";
				frtc = frDate.time.isValid ? "" : "red";
				toc  = toDate.date.isValid && delta ? "" : "red";
				totc = toDate.time.isValid && delta ? "" : "red";
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
				cb  = !r || (r && (dlg.cycles.val() < 0)) ? "red" : "";
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
			var defaultEndDate = new Date(_eventObj.start.getFullYear(), _eventObj.start.getMonth(), _eventObj.start.getDate(), _eventObj.start.getHours() + 1, _eventObj.start.getMinutes())
			
			dlg.from_t
					.val(formatDate(_eventObj.start, calendar.options.eventEditor.timeFormat))
					.removeAttr("disabled");
			dlg.to_t
					.val(_eventObj.end instanceof Date ?
							formatDate(_eventObj.end, calendar.options.eventEditor.timeFormat) : formatDate(defaultEndDate, calendar.options.eventEditor.timeFormat))
					.removeAttr("disabled");
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

	function _doDDX(saveData) {
		var sources = calendar.getEventSources();
		var sourceIsValid = fcUtil.objectIsValid(_eventObj.source);
		var canChangeAlert = sourceIsValid && (
				_eventObj.source.canAlertModify != undefined && _eventObj.source.canAlertModify ||
				_eventObj.source.canAlertModify == undefined) ||
				!sourceIsValid;

		var dlg = {
			viewer: {
				title:       _dialog.find(".viewer .title"),
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
				description: _dialog.find(".viewer .description .text")
			},
			editor: {
				title:       _dialog.find(".editor .title input"),
				allday:      _dialog.find(".editor .all-day input"),
				from:        _dialog.find(".editor .from-date"),
				from_t:      _dialog.find(".editor .from-time"),
				to:          _dialog.find(".editor .to-date"),
				to_t:        _dialog.find(".editor .to-time"),
				repeat:      _dialog.find(".editor .fc-view-repeat .fc-selector-link"),
				alert:       _dialog.find(".editor .fc-view-alert .fc-selector-link"),
				calendar:    _dialog.find(".editor .calendar select"),
				calendar_b:  _dialog.find(".editor .calendar .bullet"),
				description: _dialog.find(".editor .description textarea")
			}
		};
		var i;

		if (saveData) {			// ------------- SAVE data -------------

			if (!_canEdit) {return false;}

			if (false == fcUtil.validateInput(dlg.editor.title, fcUtil.validateNonemptyString)) {return false;}
			_eventObj.title = $.trim(dlg.editor.title.val());
			_eventObj.title = _eventObj.title.substr(0,
					Math.min(calendar.options.eventMaxTitleLength, _eventObj.title.length));

			_eventObj.allDay = dlg.editor.allday.is(":checked");

			var dates = {};
			if (!_validateDateFields(dates)) {return false;}
			_eventObj.start = dates.fromDate.dateTime;
			_eventObj.end   = dates.toDate.dateTime;

			if (canChangeAlert) {
				//_eventObj.repeat.type = parseInt(dlg.editor.repeat.val(), 10);
				//_eventObj.alert.type = parseInt(dlg.editor.alert.val(), 10);
				
				_eventObj.repeatRule = repeatRule;
				_eventObj.alert.type = alertType;
			}

			_eventObj.newSourceId = dlg.editor.calendar.val();

			var src;
			$.each(sources, function(){if (this.objectId == _eventObj.newSourceId) {src = this;return false;}return true;});
			if (src) {
				_eventObj.newTimeZone = $.extend({}, src.timeZone);
			}

			var description = $.trim(dlg.editor.description.val());
			/*if (description.length > 0)*/ {
				_eventObj.description = description;
			}

			delete _eventObj.textColor;
			delete _eventObj.backgroundColor;
			delete _eventObj.borderColor;
			calendar.normalizeEvent(_eventObj);
			
		} else {					// ------------- LOAD data -------------
			
			dlg.editor.title.css("color", "").css("border-color", "");
			dlg.editor.title.val(_eventObj.title);
			dlg.viewer.title.html(htmlEscape(_eventObj.title));

			if (_eventObj.owner && _eventObj.owner.name && _eventObj.owner.name.length > 0) {
				dlg.viewer.owner.html(htmlEscape(_eventObj.owner.name));
				_dialog.find(".viewer .owner").removeClass("hidden");
			} else {
				dlg.viewer.owner.text("");
				_dialog.find(".viewer .owner").addClass("hidden");
			}

			if (_eventObj.allDay == true) {
				dlg.editor.allday.prop("checked", true);
				dlg.viewer.allday.addClass("yes");
			} else {
				dlg.editor.allday.removeAttr("checked");
				dlg.viewer.allday.removeClass("yes");
			}

			dlg.editor.from.css("color", "").css("border-color", "");
			dlg.editor.from_t.css("color", "").css("border-color", "");
			if (_eventObj.start instanceof Date) {
				dlg.editor.from.val(
						formatDate(_eventObj.start, calendar.options.eventEditor.dateFormat));
						
				if (_eventObj.allDay == false) {
					dlg.editor.from_t.val(formatDate(_eventObj.start, calendar.options.eventEditor.timeFormat));
				}
				else {
					dlg.editor.from_t.val("");
				}
			} 
			else {
				dlg.editor.from.val("");
				dlg.editor.from_t.val("");
			}
			dlg.viewer.from.text(dlg.editor.from.val());
			//dlg.viewer.from_t.text(dlg.editor.from_t.val());

			dlg.editor.to.css("color", "").css("border-color", "");
			dlg.editor.to_t.css("color", "").css("border-color", "");
			if (_eventObj.end instanceof Date) {
				dlg.editor.to.val(
						formatDate(_eventObj.end, calendar.options.eventEditor.dateFormat));
						
				if (_eventObj.allDay == false) {
					dlg.editor.to_t.val(formatDate(_eventObj.end, calendar.options.eventEditor.timeFormat));
				}
				else {
					dlg.editor.to_t.val("");
				}
			} 
			else {
				dlg.editor.to.val("");
				dlg.editor.to_t.val("");
			}
			dlg.viewer.to.text(dlg.editor.to.val());
			//dlg.viewer.to_t.text(dlg.editor.to_t.val());
			
			// from time
			if (_eventObj.start instanceof Date) {
				if (_eventObj.allDay == false) {
					dlg.viewer.from_t.text(formatDate(_eventObj.start, calendar.options.axisFormat));
				}
				else {
					dlg.viewer.from_t.text("");
				}
			} 
			else {
				dlg.viewer.from_t.text("");
			}
			
			// to time
			if (_eventObj.end instanceof Date) {
				if (_eventObj.allDay == false) {
					dlg.viewer.to_t.text(formatDate(_eventObj.end, calendar.options.axisFormat));
				}
				else {
					dlg.viewer.to_t.text("");
				}
				dlg.viewer.from.prev().show();
				dlg.viewer.to.prev().show();
			}
			else {
				dlg.viewer.to_t.text("");
				dlg.viewer.from.prev().hide();
				dlg.viewer.to.prev().hide();
			}
			//

			var defaultSource = _getDefaultSource();
			var calSource = sourceIsValid ? _eventObj.source : defaultSource;

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
							(sources[i].isEditable || !sources[i].isSubscription)) {
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
			dlg.editor.calendar_b.css("color", calColor);
			dlg.viewer.calendar_b.css("color", calColor);

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
		}
		return true;
	}


	function _getDefaultSource() {
		var sources = calendar.getEventSources();
		var s1;
		for (var i = 0; i < sources.length; ++i) {
			if (fcUtil.objectIsValid(sources[i]) && (
			    fcUtil.objectIsEditable(sources[i]) || !sources[i].isSubscription)) {
				if (s1 == undefined) {s1 = sources[i];}
				if (!sources[i].isHidden) {return sources[i];}
			}
		}
		return s1;
	}

	function _createEvent(startDate, endDate, allDay) {
		return {
			title:           calendar.options.eventEditor.newEventTitle,
			description:     "",
			allDay:          allDay,
			start:           startDate,
			end:             endDate,
			repeatRule:      ASC.Api.iCal.RecurrenceRule.Never,
			alert:           {type:kAlertDefault},
			isShared:        false,
			permissions:     {users:[]}
		};
	}

	function _deleteEvent(deleteType, eventDate) {
		if (!_canDelete ||
		    !fcUtil.objectIsValid(_eventObj) ||
		    !fcUtil.objectIsValid(_eventObj.source)) {return;}
		//
		var id = _eventObj._id;
		calendar.trigger("editEvent", _this,
				$.extend(
						{action: kEventDeleteAction, sourceId: _eventObj.source.objectId, type: deleteType, date: eventDate},
						_eventObj),
				function(response) {
					if (!response.result) {return;}
					calendar.removeEvents(id);
					
					//
					if (response.event != undefined) {
						if (response.event.length < 1) {return;}
						//
						var sources = calendar.getEventSources();
						var j = 0;
						while (j < response.event.length) {
							_setEventSource(response.event[j], sources);
							j++;
						}
						calendar.addEvents(response.event);
					}
				});
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

	function _updateEvent(isCancel) {
		if (!_canEdit) {return;}
		
		if(isCancel)
		{
			calendar.trigger("editEvent", _this, $.extend( {action: kEventCancelAction, sourceId: _eventObj.source.objectId}, _eventObj), function(response){} );
			return;
		}

		if (_eventObj.source) {
			// update event
			calendar.trigger("editEvent", _this,
					$.extend(
							{action: kEventChangeAction, sourceId: _eventObj.source.objectId},
							_eventObj),
					function(response) {
						if (!response.result) {return;}
						_closeDialog.call(_this);
						if (response.event.length < 1) {return;}
						//
						var sources = calendar.getEventSources();
						calendar.removeEvents(response.event[0].objectId);
						for (var j = 0; j < response.event.length; ++j) {
							_setEventSource(response.event[j], sources);
						}
						calendar.addEvents(response.event);
					});
		} else {
			var id = _eventObj._id;
			// create new event
			calendar.trigger("editEvent", _this,
					$.extend({action: kEventAddAction}, _eventObj),
					function(response) {
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
					});
		}
	}


	// Public interface

	this.openEvent = function(elem, event) {
		_open.call(_this, "view", elem, event);
	};

	this.addEvent = function(startDate, endDate, allDay) {
		// Protect from addind event in nonexistent category
		var defaultSource = _getDefaultSource();
		if (defaultSource == undefined) {return;}

		var ev;
		if (startDate != undefined) {
			// add event via clicking calendar cell
			ev = _createEvent(startDate, endDate, allDay);
			ev.textColor = defaultSource.textColor;
			ev.backgroundColor = defaultSource.backgroundColor;
			ev.borderColor = defaultSource.borderColor;
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


	// exports
	t.isFetchNeeded = isFetchNeeded;
	t.fetchEvents = fetchEvents;
	t.addEvents = addEvents;
	t.addEventSource = addEventSource;
	t.removeEventSource = removeEventSource;
	t.getEventSources = getEventSources;
	t.updateEvent = updateEvent;
	t.renderEvent = renderEvent;
	t.removeEvents = removeEvents;
	t.clientEvents = clientEvents;
	t.normalizeEvent = normalizeEvent;


	// imports
	var trigger = t.trigger;
	var getView = t.getView;
	var reportEvents = t.reportEvents;


	// locals
	var stickySource = {events: []};
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

	function fetchEvents(start, end) {
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
				t.rerenderCategories();
			}
		});
	}

	function fetchEventSource(source, fetchID) {
		_fetchEventSource(source, function(response) {
			if (fetchID == currentFetchID) {
				if (response.events) {
					for (var i=0; i<response.events.length; i++) {
						response.events[i].source = source;
						normalizeEvent(response.events[i]);
					}
					cache = cache.concat(response.events);
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
		if (events) {
			if ($.isFunction(events)) {
				pushLoading();
				events(cloneDate(rangeStart), cloneDate(rangeEnd), function(events) {
					callback({result:true,events:events});
					popLoading();
				});
			}
			else if ($.isArray(events)) {
				callback({result:true,events:events});
			}
			else {
				callback({result:false,events:[]});
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

	function getEventSources() {
		return sources;
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
			event.allDay = firstDefined(source.allDayDefault, options.allDayDefault);
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
	d.setHours(0);
	d.setMinutes(0);
	d.setSeconds(0);
	d.setMilliseconds(0);
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
	var date = new Date(m[1], 0, 1);
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
function sliceSegs(events, visEventEnds, start, end) {
	var segs = [],
		i, len=events.length, event,
		eventStart, eventEnd,
		segStart, segEnd,
		isStart, isEnd;
	for (i=0; i<len; i++) {
		event = events[i];
		eventStart = event.start;
		eventEnd = visEventEnds[i];
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
	return hpadding(element) + hborders(element) + (includeMargins ? hmargins(element) : 0);
}

function hpadding(element) {
	return (parseFloat($(element[0]).css('padding-left')) || 0) +
				 (parseFloat($(element[0]).css('padding-right')) || 0);
}

function hmargins(element) {
	return (parseFloat($(element[0]).css('margin-left')) || 0) +
				 (parseFloat($(element[0]).css('margin-right')) || 0);
}

function hborders(element) {
	return (parseFloat($(element[0]).css('border-left-width')) || 0) +
				 (parseFloat($(element[0]).css('border-right-width')) || 0);
}

function vsides(element, includeMargins) {
	return vpadding(element) +  vborders(element) + (includeMargins ? vmargins(element) : 0);
}

function vpadding(element) {
	return (parseFloat($(element[0]).css('padding-top')) || 0) +
				 (parseFloat($(element[0]).css('padding-bottom')) || 0);
}

function vmargins(element) {
	return (parseFloat($(element[0]).css('margin-top')) || 0) +
				 (parseFloat($(element[0]).css('margin-bottom')) || 0);
}

function vborders(element) {
	return (parseFloat($(element[0]).css('border-top-width')) || 0) +
				 (parseFloat($(element[0]).css('border-bottom-width')) || 0);
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
	cell.each(function(i, _cell) {
		_cell.className = _cell.className.replace(/^fc-\w*/, 'fc-' + dayIDs[date.getDay()]);
		// TODO: make a way that doesn't rely on order of classes
	});
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
			event.end.getDate() == event.start.getDate());
	var backgroundColor = isShort ? 'transparent' : (
		event.backgroundColor ||
		eventColor ||
		source.backgroundColor ||
		sourceColor ||
		opt('eventBackgroundColor') ||
		optionColor);
	var borderColor = isShort ? 'transparent' : (
		event.borderColor ||
		eventColor ||
		source.borderColor ||
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
		var cell;
		var date;
		var row;
		var todayIndex;
		var topRow;
		var leftCell;

		if (dowDirty) {
			headCells.each(function(i, _cell) {
				cell = $(_cell);
				date = indexDate(i);
				cell.html(formatDate(date, colFormat));
				setDayID(cell, date);
			});
		}

		bodyCells.each(function(i, _cell) {
			cell = $(_cell);
			date = indexDate(i);
			if (date.getMonth() == month) {
				cell.removeClass('fc-other-month');
			} else {
				cell.addClass('fc-other-month');
			}
			if (+date == +today) {
				cell.addClass(tm + '-state-highlight fc-today');
				leftCell = cell.prev();
				if (leftCell.length > 0) {
					leftCell.addClass("fc-today-left");
				}
				topRow = cell.closest("tr").prev();
				if (topRow.length > 0) {
					topRow.children().eq(cell.index()).addClass("fc-today-top");
				}
				todayIndex = i;
			} else {
				cell.removeClass(tm + '-state-highlight fc-today');
				cell.removeClass('fc-today-left');
				cell.removeClass('fc-today-top');
			}
			cell.find('div.fc-day-number').html('<span>' + date.getDate() + '</span>');
			//--diabled big day numbers--
			//cell.find('div.fc-day-number-big').text(date.getDate());
			if (dowDirty) {
				setDayID(cell, date);
			}
		});

		bodyRows.each(function(i, _row) {
			row = $(_row);
			if (i < rowCnt) {
				row.show();
				if (i == rowCnt-1) {
					row.addClass('fc-last');
				}else{
					row.removeClass('fc-last');
				}
			}else{
				row.hide();
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

		var bodyHeight = viewHeight - head.height();
		var rowHeight;
		var rowHeightLast;

		if (opt('weekMode') == 'variable') {
			rowHeight = rowHeightLast = Math.floor(bodyHeight / (rowCnt==1 ? 2 : 6));
		}else{
			rowHeight = Math.floor(bodyHeight / rowCnt);
			rowHeightLast = bodyHeight - rowHeight * (rowCnt-1);
		}

		bodyCells.each(function(i, _cell) {
			var cell = $(_cell);
			var h = (Math.floor(i/7) == rowCnt-1 ? rowHeightLast : rowHeight) - vsides(cell);
			var innerDiv = cell.find('> div');
			setMinHeight(innerDiv, h, true);
			var dc = cell.find(".fc-day-content");
			var hdc = h - cell.find(".fc-day-number").outerHeight();
			setMinHeight(dc, hdc, true);
			var hspace = dc.outerHeight() - hdc;
			if (hspace > 0) {
				setMinHeight(dc, hdc - hspace, true);
			}
			var bigDayNum = cell.find(".fc-day-number-big")
					.css("top", innerDiv.position().top)
					.css("line-height", h + "px");
			setMinHeight(bigDayNum, h, true);
		});

	}


	function setWidth(width) {
		viewWidth = width;
		colContentPositions.clear();
		var rem = viewWidth % colCnt;
		colWidth = Math.floor(viewWidth / colCnt);
		if (rem == 0) {
			setOuterWidth(headCells, colWidth);
		} else {
			setOuterWidth(headCells.filter(":even"), colWidth);
			setOuterWidth(headCells.filter(":odd"), colWidth + 1);
			setOuterWidth(headCells.last(), viewWidth - colWidth * (colCnt-1) - Math.floor(colCnt/2));
		}
		bodyCells.each(function(i, _cell) {
			var cell = $(_cell);
			var innerDiv = cell.find('> div');
			var bigDayNum = cell.find(".fc-day-number-big")
					.css("left", innerDiv.position().left)
			setOuterWidth(bigDayNum, innerDiv.outerWidth());
		});
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
		getDaySegmentContainer().empty();
	}


	function compileSegs(events) {
		var rowCnt = getRowCnt(),
			colCnt = getColCnt(),
			d1 = cloneDate(t.visStart),
			d2 = addDays(cloneDate(d1), colCnt),
			visEventsEnds = $.map(events, exclEndDay),
			i, row,
			j, level,
			k, seg,
			segs=[];
		for (i=0; i<rowCnt; i++) {
			row = stackSegs(sliceSegs(events, visEventsEnds, d1, d2));
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
                    if (-1) {
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
		if (colFormat.indexOf("dddd") < 0 && colFormat.indexOf("ddd") < 0) {
			colFormat = "dddd, " + colFormat;
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
				$("<div style='position:absolute;z-index:8;top:0;left:0'/>")
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
				var now = new Date();
				var top = timePosition(now, now);
				var left = colContentLeft(0) - padding;
				var width = colContentRight(colCnt - 1) + padding - left;
				var l = marker.find(".left-side").outerWidth(true);
				var h = marker.find(".left-side").outerHeight(true);
				marker.css("left", (left - l) + "px");
				marker.css("top", Math.round(top - 0.5 * h) + "px");
				marker.find(".center-line").css("width", width + "px");
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
			headCell.html(formatDate(date, colFormat));
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
			setDayID(headCell.add(bodyCell), date);
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
        var c = dayBodyCells.length > 1 ? dayBodyCells[col] : $(".fc-agenda-allday .fc-day-content:visible")[0];
		if (c) {
			return {
					left:   c.offsetLeft,
					top:    c.offsetTop,
					right:  c.offsetLeft + c.offsetWidth - (dayBodyCells.length > 1 ? 0 : 325),
					bottom: c.offsetTop + c.offsetHeight};
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
		var levels = stackSegs(sliceSegs(events, $.map(events, exclEndDay), t.visStart, t.visEnd)),
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
			visEventEnds = $.map(events, slotEventEnd),
			i, col,
			j, level,
			k, seg,
			segs=[];
		for (i=0; i<colCnt; i++) {
			col = stackSegs(sliceSegs(events, visEventEnds, d, addMinutes(cloneDate(d), maxMinute-minMinute)));
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
			classes,
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
			headElement,
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
			html += slotSegHtml(event, seg);
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

		// set all positions/dimensions at once
		var vr = t.getViewRect();
		function activateTooltip(elem, maxWidth, maxHeight, elemContainer, elemContainerH) {
			elem.css("position", "absolute");
			var elemW = elem.width();
			var elemH = elem.height();
			if (maxWidth < elemW || maxHeight > 0 && maxHeight < elemH) {
				var to = elem.offset();
				if (to.left + elemW > vr.right - 20 &&
						to.left + maxWidth - elemW >= vr.left) {
					elemContainer.addClass("fc-tooltip right");
				} else {
					elemContainer.addClass("fc-tooltip");
				}
				if (elemContainerH != undefined) {elemContainer.css("height", hh + "px");}
			} else {
				elemContainer.removeClass("fc-tooltip right");
				if (elemContainerH != undefined) {elemContainer.css("height", "");}
			}
			elem.css("position", "");
		}
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			eventElement = seg.element
			if (eventElement) {
				width = Math.max(0, seg.outerWidth - seg.hsides);
				height = Math.max(0, seg.outerHeight - seg.vsides);
				eventElement[0].style.width = width + 'px';
				eventElement[0].style.height = height + 'px';
				// set height of event-content
				contentElement = eventElement.find('.fc-event-content');
				cheight = height -
						eventElement.find(".fc-event-head").outerHeight(true) -
						eventElement.find(".ui-resizable-s").outerHeight(true);
				contentElement.css("height", cheight + "px");
				// activate tooltip for event-content
				titleElement = eventElement.find(".fc-event-title");
				activateTooltip(titleElement, width, cheight, contentElement);
				// activate tooltip for event-time
				headElement = eventElement.find('.fc-event-head');
				var hh = headElement.outerHeight(true);
				timeElement = eventElement.find('.fc-event-time');
				activateTooltip(timeElement, width, 0, headElement, hh);
				//
				event = seg.event;
				if (seg.contentTop !== undefined && height - seg.contentTop < 10) {
					// not enough room for title, put it in the time header
					timeElement
						.html(timeElement.html() + ' ' + htmlEscape( titleElement.text() ));
					titleElement
						.remove();
					activateTooltip(timeElement, width, 0, headElement, hh);
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
				'<div class="fc-event-content">' +
					'<div class="fc-event-title">' +
						htmlEscape(event.title) +
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
		var visEventEnds = $.map(events, exclEndDay);
		return sliceSegs(events, visEventEnds, _this.start, _this.end).sort(
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
		var event, start, end, eventTitle, eventNote, eventRow;
		var html;
		var first;
		var timeFormat = lv.timeFormat;
		var monthFormat = lv.monthTitleFormat;
		var dayFormat = (lv.dayTitleFormat.indexOf("dddd") < 0 &&
				lv.dayTitleFormat.indexOf("ddd") < 0 ? "dddd, " : "") + lv.dayTitleFormat;

		var segs = _compileSegs.call(_this, events);

		list = {};
		for (i = 0; i < segs.length; ++i) {
			event = segs[i].event;
			start = cloneDate(segs[i].start, true);
			end = segs[i].end != undefined ? cloneDate(segs[i].end, true) : cloneDate(start);

			eventTitle =
					typeof event.title == "string" ? event.title.replace(/(.+)\.?\s*$/i, "$1.") : "";
			eventNote =
					typeof event.description == "string" ? event.description.replace(/(.+)\.?\s*$/i, "$1.") : "";
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
						'<td class="title last">' +
							'<span class="title">' + htmlEscape(eventTitle) + '</span>' +
							(eventNote && eventNote.length > 0 ?
									('<span class="note">' + htmlEscape(eventNote) + '</span>') : '') + '</td>' +
					'</tr>';

			for (; start < end; addDays(start, +1)) {
				monthTitle = formatDate(start, monthFormat);
				dayTitle = formatDate(start, dayFormat);
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
								title: '<tr><th colspan="3">' + htmlEscape(dayTitle) + '</th></tr>',
								today: now.getDate() == start.getDate() &&
											 now.getMonth() == start.getMonth() &&
											 now.getFullYear() == start.getFullYear(),
								events: []
							};
				}
				list[mt][dt].events.push(eventRow);
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
						'" border="0" cellspacing="0" cellpadding="0"><tbody>' + list[i][j].title;
				// iterate through events
				for (k = 0; k < list[i][j].events.length; ++k) {
					html += list[i][j].events[k];
				}
				html += '</tbody></table>';
			}
		}

		if (html.length > 0) {
			var w = _eventsList[0].clientWidth - 6/* padding in .fc-lv-scroller */;
			_eventsList.html('<div style="width:' + w + 'px;">' + html + '</div>');
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
		_eventsList.empty();
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
	t.ellipsisEventsTitles = ellipsisEventsTitles;


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


	function ellipsisEventsTitles(eventElem) {
		eventElem.find(".fc-event-title").each(function() {
			var title = $(this);
			if (!title.is(":visible")) {return;}

			var inner = title.find(".inner");
			var text = inner.length > 0 ? inner.text() : title.text();

			title.css("right", "auto");
			title.html( htmlEscape(text) );

			var elem  = title.parent();
			var elemW = elem.width();
			var tm    = elem.find(".fc-event-time");
			var img   = elem.find(".bullet");
			var left  = (tm.length > 0 ? tm.width() : 0) +
					(img.length > 0 ? img.width() : 0);
			var right = 0;
			var tw    = title.outerWidth(true);
			var overlapped = left + tw >= elemW;

			title
					.css("left", left + "px")
					.css("right", right + "px");

			if (overlapped) {
				title.html('<div class="inner">' + htmlEscape(text) + '</div>' +
				           '<div class="dots">&#8230;</div>');
				var bgc = elem.css("background-color");
				title.find(".dots").css(
						"background-color",
						(bgc == "transparent" || bgc.indexOf("rgba") != -1)? "" : bgc);
				var vr = t.getViewRect();
				var to = title.offset();
				if (to.left + tw > vr.right &&
				    to.left + title.outerWidth(true) - tw >= vr.left) {
					title.find(".inner").addClass("tooltip-right");
				} else {
					title.find(".inner").removeClass("tooltip-right");
				}
			}
		});
	}


	function renderDaySegs(segs, modifiedEventId) {
		var segmentContainer = getDaySegmentContainer();
		var rowDivs;
		var rowCnt = getRowCnt();
		var colCnt = getColCnt();
		var i = 0;
		var rowI;
		var levelI;
		var colHeights;
		var j;
		var segCnt = segs.length;
		var seg;
		var top;
		var k;
		segmentContainer[0].innerHTML = daySegHTML(segs); // faster than .html()
		daySegElementResolve(segs, segmentContainer.children());
		daySegElementReport(segs);
		daySegHandlers(segs, segmentContainer, modifiedEventId);
		daySegCalcHSides(segs);
		daySegSetWidths(segs);
		daySegCalcHeights(segs);
		rowDivs = getRowDivs();
		// set row heights, calculate event tops (in relation to row top)
		for (rowI=0; rowI<rowCnt; rowI++) {
			levelI = 0;
			colHeights = [];
			for (j=0; j<colCnt; j++) {colHeights[j] = 0;}
			while (i<segCnt && (seg = segs[i]).row == rowI) {
				// loop through segs in a row
				top = arrayMax(colHeights.slice(seg.startCol, seg.endCol));
				seg.top = top;
				top += seg.outerHeight;
				for (k=seg.startCol; k<seg.endCol; k++) {
					colHeights[k] = top;
				}
				i++;
			}
			if (t.name != "month") {
				rowDivs[rowI].height(arrayMax(colHeights));
			}
		}
		daySegSetTops(segs, getRowTops(rowDivs));
		ellipsisEventsTitles(getDaySegmentContainer());

		if ($.isFunction(t._afterRenderDaySegs)) {
		    t._afterRenderDaySegs(segs);
		}
	}


	function renderTempDaySegs(segs, adjustRow, adjustTop) {
		var tempContainer = $("<div/>");
		var elements;
		var segmentContainer = getDaySegmentContainer();
		var i;
		var segCnt = segs.length;
		var element;
		tempContainer[0].innerHTML = daySegHTML(segs); // faster than .html()
		elements = tempContainer.children();
		segmentContainer.append(elements);
		daySegElementResolve(segs, elements);
		daySegCalcHSides(segs);
		daySegSetWidths(segs);
		daySegCalcHeights(segs);
		daySegSetTops(segs, getRowTops(getRowDivs()));
		elements = [];
		for (i=0; i<segCnt; i++) {
			element = segs[i].element;
			if (element) {
				if (segs[i].row === adjustRow) {
					element.css('top', adjustTop);
				}
				elements.push(element[0]);
			}
		}
		return $(elements);
	}


	function daySegHTML(segs) { // also sets seg.left and seg.outerWidth
		var rtl = opt('isRTL');
		var i;
		var segCnt=segs.length;
		var seg;
		var event;
		var url;
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
		// calculate desired position/dimensions, create html
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			event = seg.event;
			classes = [
					'fc-event',
					event.allDay ? 'fc-event-skin-day' : 'fc-event-skin',
					'fc-event-hori'];
			if (isEventDraggable(event)) {
				classes.push('fc-event-draggable');
			}
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
			skinCss = getSkinCss(event, opt, t.name);
			
			// tag <a> is not supported now
			//url = event.url;
			//if (url) {
			//	html += "<a href='" + htmlEscape(url) + "'";
			//}else{
				html += "<div";
			//}
			
			html +=
				" class='" + classes.join(' ') + "'" +
				" style='position:absolute;z-index:8;left:"+left+"px;" + skinCss + "'" +
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
			}
			if (seg.isStart && !event.allDay) {
				html += '<span class="bullet" style="color:' + htmlEscape(event.source ?
						event.source.backgroundColor : t.calendar.options.eventBackgroundColor) +
						';">' + htmlEscape(t.calendar.options.categories.itemBullet) + '&nbsp;</span>';
			} else {
				html += '<span>&nbsp;</span>';      // to prevent collapsing
			}
			html +=
				"<div class='fc-event-title'>" + htmlEscape(event.title) + "</div>" +
				"</div>";
			if (seg.isEnd && isEventResizable(event) && event.allDay) {
				html +=
					"<div class='ui-resizable-handle ui-resizable-" + (rtl ? 'w' : 'e') + "'>" +
					"&nbsp;&nbsp;&nbsp;" + // makes hit area a lot better for IE6/7
					"</div>";
			}
			html +=
				"</div>";//"</" + (url ? "a" : "div" ) + ">";
			seg.left = left;
			seg.outerWidth = right - left;
			seg.startCol = leftCol;
			seg.endCol = rightCol + 1; // needs to be exclusive
		}
		return html;
		
	}


	function daySegElementResolve(segs, elements) { // sets seg.element
		var i;
		var segCnt = segs.length;
		var seg;
		var event;
		var element;
		var triggerRes;
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			event = seg.event;
			element = $(elements[i]); // faster than .eq()
			triggerRes = trigger('eventRender', event, event, element);
			if (triggerRes === false) {
				element.remove();
			}else{
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
	}


	function daySegElementReport(segs) {
		var i;
		var segCnt = segs.length;
		var seg;
		var element;
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			element = seg.element;
			if (element) {
				reportEventElement(seg.event, element);
			}
		}
	}


	function daySegHandlers(segs, segmentContainer, modifiedEventId) {
		var i;
		var segCnt = segs.length;
		var seg;
		var element;
		var event;
		// retrieve elements, run through eventRender callback, bind handlers
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			element = seg.element;
			if (element) {
				event = seg.event;
				if (event._id === modifiedEventId) {
					bindDaySeg(event, element, seg);
				}else{
					element[0]._fci = i; // for lazySegBind
				}
			}
		}
		lazySegBind(segmentContainer, segs, bindDaySeg);
	}


	function daySegCalcHSides(segs) { // also sets seg.key
		var i;
		var segCnt = segs.length;
		var seg;
		var element;
		var key, val;
		var hsideCache = {};
		// record event horizontal sides
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			element = seg.element;
			if (element) {
				key = seg.key = cssKey(element[0]);
				val = hsideCache[key];
				if (val === undefined) {
					val = hsideCache[key] = hsides(element, true);
				}
				seg.hsides = val;
			}
		}
	}


	function daySegSetWidths(segs) {
		var i;
		var segCnt = segs.length;
		var seg;
		var element;
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			element = seg.element;
			if (element) {
				element[0].style.width = Math.max(0, seg.outerWidth - seg.hsides) + 'px';
			}
		}
	}


	function daySegCalcHeights(segs) {
		var i;
		var segCnt = segs.length;
		var seg;
		var element;
		var key, val;
		var vmarginCache = {};
		// record event heights
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			element = seg.element;
			if (element) {
				key = seg.key; // created in daySegCalcHSides
				val = vmarginCache[key];
				if (val === undefined) {
					val = vmarginCache[key] = vmargins(element);
				}
				seg.outerHeight = element[0].scrollHeight + val;
			}
		}
	}


	function getRowDivs() {
		var i;
		var rowCnt = getRowCnt();
		var rowDivs = [];
		for (i=0; i<rowCnt; i++) {
			rowDivs[i] = allDayRow(i)
				.find('td:not(.fc-today):first div.fc-day-content > div'); // optimal selector?
		}
		return rowDivs;
	}


	function getRowTops(rowDivs) {
		var i;
		var rowCnt = rowDivs.length;
		var tops = [];
		if (rowCnt == 1 && t.name != "month") {
			rowDivs[0] = rowDivs[0].closest('div.fc-day-content');
		}
		for (i=0; i<rowCnt; i++) {
			tops[i] = {};
			tops[i].top = rowDivs[i][0].offsetTop; // !!?? but this means the element needs position:relative if in a table cell!!!!
			tops[i].bottom = tops[i].top + rowDivs[i][0].offsetHeight - 1;
		}
		return tops;
	}


	function daySegSetTops(segs, rowTops) { // also triggers eventAfterRender
		var co = t.calendar.options;
		var segmentContainer = getDaySegmentContainer();
		var i;
		var segCnt = segs.length;
		var seg;
		var element;
		var event;
		var tmpInfo = $("<div id='tmp_info' class='fc-event-info'>1</div>").appendTo(segmentContainer);
		var infoH = tmpInfo.outerHeight();
		tmpInfo.remove();
		for (i=0; i<segCnt; i++) {
			seg = segs[i];
			element = seg.element;
			if (element) {
				var elTop = rowTops[seg.row].top + (seg.top||0);
				element[0].style.top = elTop + 'px';
				if (true) {
					if (elTop + seg.outerHeight > rowTops[seg.row].bottom - infoH) {
						element.hide();
						var cellId = "cell_" + seg.row + "_" + seg.startCol;
						var info = segmentContainer.find("#" + cellId);
						if (info.length < 1) {
							info = $("<div id='" + cellId + "' class='fc-event-info'/>").appendTo(segmentContainer);
						}
						var s = info.text().match(/(\d+)/);
						var cnt = parseInt((s ? s[1] : 0), 10) || 0;
						info
								.text(co.moreEventsLabel.replace("%d", cnt + 1))
								.css("left", (seg.left+ Math.round(seg.outerWidth/(2*(seg.endCol-seg.startCol)) - info.width()/2)-2/*parent's left padding*/) + "px")
								.css("bottom", 0 - rowTops[seg.row].bottom + "px")
								.unbind()
								.click({row: seg.row, col: seg.startCol, segs: segs}, showHiddenEvents);
					} else {
						element.show();
					}
				}
				event = seg.event;
				trigger('eventAfterRender', event, event, element);
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
			popup.mouseleave(function() {
				if (!t.calendar.isEditingEvent()){
					$(this).hide();}
			});
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
		ellipsisEventsTitles(popup);

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
			popup.css("overflow-y", "scroll").css("overflow-x", "hidden").css("max-height", "190px");
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