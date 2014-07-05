<%@ Assembly Name="ASC.Web.Calendar" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CalendarResources.ascx.cs" Inherits="ASC.Web.Calendar.UserControls.CalendarResources" %>

<script language="javascript" type="text/javascript">
var g_fcOptions = {

		isRTL:           <%=(System.Globalization.CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "true" : "false")%>,

		monthNames:      ['<%=System.String.Join("', '", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames, 0, 12)%>'],
		monthNamesShort: ['<%=System.String.Join("', '", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedMonthNames, 0, 12)%>'],

		dayNames:        ['<%=System.String.Join("', '", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.DayNames, 0, 7)%>'],
		dayNamesShort:   ['<%=System.String.Join("', '", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedDayNames, 0, 7)%>'],
		firstDay:        <%=(int)System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek%>,

		titleFormat: {
			month:        "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.YearMonthPattern%>",
			week:         "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.MonthDayPattern%>",
			day:          "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern%>"
		},
		columnFormat: {
			day:          "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern%>"
		},
		timeFormat: {
			"":           "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern%>",
			agenda:       "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern%>{ - <%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern%>}"
		},

		buttonText: {
			today:        "<%=Resources.CalendarJSResource.calendarButtonText_today%>",
			month:        "<%=Resources.CalendarJSResource.calendarButtonText_month%>",
			week:         "<%=Resources.CalendarJSResource.calendarButtonText_week%>",
			day:          "<%=Resources.CalendarJSResource.calendarButtonText_day%>",
			list:         "<%=Resources.CalendarJSResource.calendarButtonText_list%>"
		},

		allDayText:          "<%=Resources.CalendarJSResource.calendarAllDayText%>",
		axisFormat:          "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern%>",
		popupCellFormat:     "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.MonthDayPattern%>",

		todayLabel:          "<%=Resources.CalendarJSResource.calendarTodayLabel%>",
		moreEventsLabel:     "<%=Resources.CalendarJSResource.calendarMoreEventsLabel%>",
		addNewEventLabel:    "<%=Resources.CalendarJSResource.calendarAddNewEventLabel%>",
		addNewLabel:         "<%=Resources.CalendarJSResource.calendarAddNewLabel%>",

		modes: {
			calendarViewLabel: "<%=Resources.CalendarJSResource.calendarViewLabelText_calendar%>",
			listViewLabel:     "<%=Resources.CalendarJSResource.calendarViewLabelText_list%>"
		},

		sharedList: {
			title:             "<%=Resources.CalendarJSResource.calendarSharedList_title%>",
			moreLink:          "<%=Resources.CalendarJSResource.calendarSharedList_moreLink%>",
			addLink:           "<%=Resources.CalendarJSResource.calendarSharedList_addLink%>"
		},

		categories: {
			// list items
			defaultTitle:                           "<%=Resources.CalendarJSResource.calendar_newCalendarTitle%>",
			// list of calendars
			title:                                  "<%=Resources.CalendarJSResource.calendarCategoriesList_calendarsHeader%>",
			addNewCategoryLabel:                    "<%=Resources.CalendarJSResource.calendarCategoriesList_addNewCategoryLabel%>",
			dialogHeader_add:                       "<%=Resources.CalendarJSResource.calendarCategoriesDialog_addHeader%>",
			dialogHeader_edit:                      "<%=Resources.CalendarJSResource.calendarCategoriesDialog_editHeader%>",
			dialogColor_label:                      "<%=Resources.CalendarJSResource.calendarCategoriesDialog_colorLabel%>",
			dialogTextColor_label:                  "<%=Resources.CalendarJSResource.calendarCategoriesDialog_textColorLabel%>",
			dialogTimezoneLabel:                    "<%=Resources.CalendarJSResource.calendarCategoriesDialog_timezoneLabel%>",
			dialogButton_save:                      "<%=Resources.CalendarJSResource.calendarCategoriesDialog_saveButton%>",
			dialogButton_cancel:                    "<%=Resources.CalendarJSResource.calendarCategoriesDialog_cancelButton%>",
			dialogButton_delete:                    "<%=Resources.CalendarJSResource.calendarCategoriesDialog_deleteButton%>",
			// list of subscriptions
			subscriptionsTitle:                     "<%=Resources.CalendarJSResource.calendarCategoriesList_subscriptionsHeader%>",
			//
			subscriptionsDialogHeader:              "<%=Resources.CalendarJSResource.calendarSubscriptionsDialog_header%>",
			subscriptionsDialogOwnerLabel:          "<%=Resources.CalendarJSResource.calendarSubscriptionsDialog_ownerLabel%>",
			subscriptionsDialogButton_unsubscribe:  "<%=Resources.CalendarJSResource.calendarSubscriptionsDialog_unsubscribeButton%>",
			//
			subscriptionsManageLabel:               "<%=Resources.CalendarJSResource.calendarCategoriesList_subscriptionsManageLabel%>",
			subscriptionsManageDialog_title:        "<%=Resources.CalendarJSResource.calendarSubscriptionsManageDialog_title%>",
			subscriptionsManageDialog_qsearchText:  "<%=Resources.CalendarJSResource.calendarSubscriptionsManageDialog_qSearchText%>",
			subscriptionsManageDialogButton_save:   "<%=Resources.CalendarJSResource.calendarSubscriptionsManageDialog_saveButton%>",
			subscriptionsManageDialogButton_cancel: "<%=Resources.CalendarJSResource.calendarSubscriptionsManageDialog_cancelButton%>",
			// datepicker
			datepickerHideLabel:                    "<%=Resources.CalendarJSResource.calendarMiniCalendar_hideLabel%>",
			datepickerShowLabel:                    "<%=Resources.CalendarJSResource.calendarMiniCalendar_showLabel%>"
		},

		todoList: {
			title: 'ToDo List',
			hideLabel: 'Hide ToDo List',
			showLabel: 'Show ToDo List',
			addTodoLabel: 'New todo',
			todoEditorUrl: './fullcalendar/tmpl/todo.editor.tmpl',
			newTodoTitle: 'New ToDo item',
			menu: {
				hideColmpletedTodos: {hide: 'Hide completed items', show: 'Show completed items'},
				deleteCompletedTodos: 'Delete completed items'
			},
			sortByCalendarLabel: 'Sort by calendar',
			sortByPriorityLabel: 'Sort by priority',
			sortAlphabeticallyLabel: 'Sort alphabetically'
		},

		eventEditor: {
			newEventTitle:               "<%=Resources.CalendarJSResource.calendar_newEventTitle%>",
			// dialog
			dialogHeader_add:            "<%=Resources.CalendarJSResource.calendarEventEditor_addHeader%>",
            dialogOwnerLabel:            "<%=Resources.CalendarJSResource.calendarEventEditor_ownerLabel%>",
			dialogAllDayLabel:           "<%=Resources.CalendarJSResource.calendarEventEditor_allDayLabel%>",
			dialogAllDay_no:             "<%=Resources.CalendarJSResource.calendarEventEditor_notAllDayEvent%>",
			dialogAllDay_yes:            "<%=Resources.CalendarJSResource.calendarEventEditor_allDayEvent%>",
			dialogFromLabel:             "<%=Resources.CalendarJSResource.calendarEventEditor_fromLabel%>",
			dialogToLabel:               "<%=Resources.CalendarJSResource.calendarEventEditor_toLabel%>",
			dialogRepeatLabel:           "<%=Resources.CalendarJSResource.calendarEventEditor_repeatLabel%>",
			dialogRepeatOption_never:    "<%=Resources.CalendarJSResource.calendarEventEditor_repeatNever%>",
			dialogRepeatOption_day:      "<%=Resources.CalendarJSResource.calendarEventEditor_repeatDaily%>",
			dialogRepeatOption_week:     "<%=Resources.CalendarJSResource.calendarEventEditor_repeatWeekly%>",
			dialogRepeatOption_month:    "<%=Resources.CalendarJSResource.calendarEventEditor_repeatMonthly%>",
			dialogRepeatOption_year:     "<%=Resources.CalendarJSResource.calendarEventEditor_repeatYearly%>",
			dialogRepeatOption_custom:   "<%=Resources.CalendarJSResource.calendarEventEditor_repeatCustom%>",
			dialogAlertLabel:            "<%=Resources.CalendarJSResource.calendarEventEditor_alertLabel%>",
			dialogAlertOption_default:   "<%=Resources.CalendarJSResource.calendarEventEditor_alertDefault%>",
			dialogAlertOption_never:     "<%=Resources.CalendarJSResource.calendarEventEditor_alertNever%>",
			dialogAlertOption_5minutes:  "<%=Resources.CalendarJSResource.calendarEventEditor_alert5Minutes%>",
			dialogAlertOption_15minutes: "<%=Resources.CalendarJSResource.calendarEventEditor_alert15Minutes%>",
			dialogAlertOption_30minutes: "<%=Resources.CalendarJSResource.calendarEventEditor_alert30Minutes%>",
			dialogAlertOption_hour:      "<%=Resources.CalendarJSResource.calendarEventEditor_alertHour%>",
			dialogAlertOption_2hours:    "<%=Resources.CalendarJSResource.calendarEventEditor_alert2Hours%>",
			dialogAlertOption_day:       "<%=Resources.CalendarJSResource.calendarEventEditor_alertDay%>",
			dialogSharing_no:            "<%=Resources.CalendarJSResource.calendarEventEditor_sharingNoLabel%>",
			dialogCalendarLabel:         "<%=Resources.CalendarJSResource.calendarEventEditor_calendarLabel%>",
			dialogDescriptionLabel:      "<%=Resources.CalendarJSResource.calendarEventEditor_descriptionLabel%>",
			dialogButton_edit:           "<%=Resources.CalendarJSResource.calendarEventEditor_editButton%>",
			dialogButton_save:           "<%=Resources.CalendarJSResource.calendarEventEditor_saveButton%>",
			dialogButton_close:          "<%=Resources.CalendarJSResource.calendarEventEditor_closeButton%>",
			dialogButton_cancel:         "<%=Resources.CalendarJSResource.calendarEventEditor_cancelButton%>",
			dialogButton_delete:         "<%=Resources.CalendarJSResource.calendarEventEditor_deleteButton%>",
			dialogButton_unsubscribe:    "<%=Resources.CalendarJSResource.calendarEventEditor_unsubscribeButton%>"
		},
		
		repeatSettings: {
            // dialog
            dialogHeader:                 "<%=Resources.CalendarJSResource.repeatSettings_header%>",
            
            // start date
            dialogFromLabel:              "<%=Resources.CalendarJSResource.repeatSetting_fromLabel%>",
            
            // end of repeat
            dialogToLabel:                "<%=Resources.CalendarJSResource.repeatSettings_toLabel%>",
            dialogOptionNever:            "<%=Resources.CalendarJSResource.repeatSettings_optionNever%>",
            dialogOptionDate:             "<%=Resources.CalendarJSResource.repeatSettings_optionDate%>",
            dialogOptionCount:            "<%=Resources.CalendarJSResource.repeatSettings_optionCount%>",
            
            dialogAfterLabel:             "<%=Resources.CalendarJSResource.repeatSettings_afterLabel%>",
            dialogTimesLabel:             "<%=Resources.CalendarJSResource.repeatSettings_timesLabel%>",
            
            // repeat by 
            dialogRepeatOnLabel:          "<%=Resources.CalendarJSResource.repeatSettings_onLabel%>",
            dialogRepeatOn_days:          "<%=Resources.CalendarJSResource.repeatSettings_onDays%>",
            dialogRepeatOn_weeks:         "<%=Resources.CalendarJSResource.repeatSettings_onWeeks%>",
            dialogRepeatOn_months:        "<%=Resources.CalendarJSResource.repeatSettings_onMonths%>",
            dialogRepeatOn_years:         "<%=Resources.CalendarJSResource.repeatSettings_onYears%>",
            
            // interval
            dialogEachLabel:              "<%=Resources.CalendarJSResource.repeatSettings_eachLabel%>",
            dialogAliasLabel:             "<%=Resources.CalendarJSResource.repeatSettings_aliasLabel%>",
            dialogIntervalOption_day:     "<%=Resources.CalendarJSResource.repeatSettings_intervalDay%>",
            dialogIntervalOption_week:    "<%=Resources.CalendarJSResource.repeatSettings_intervalWeek%>",
            dialogIntervalOption_month:   "<%=Resources.CalendarJSResource.repeatSettings_intervalMonth%>",
            dialogIntervalOption_year:    "<%=Resources.CalendarJSResource.repeatSettings_intervalYear%>",
            
            dayNames:                     ['<%=System.String.Join("', '", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.DayNames, 0, 7)%>'],            
            dayNamesShort:                ['<%=System.String.Join("', '", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedDayNames, 0, 7)%>'],
            
            dayAliasNames:                ["<%=Resources.CalendarJSResource.repeatSettings_firstLabel%>",
                                           "<%=Resources.CalendarJSResource.repeatSettings_secondLabel%>",
                                           "<%=Resources.CalendarJSResource.repeatSettings_thirdLabel%>",
                                           "<%=Resources.CalendarJSResource.repeatSettings_penultimateLabel%>",
                                           "<%=Resources.CalendarJSResource.repeatSettings_lastLabel%>"],
            
            // buttons
            dialogButton_save:            "<%=Resources.CalendarJSResource.repeatSettings_saveBtn%>",
            dialogButton_cancel:          "<%=Resources.CalendarJSResource.repeatSettings_cancelBtn%>"
        },
        
        deleteSettings: {
            // dialog            
            dialogHeader:                 "<%=Resources.CalendarJSResource.deleteSettings_header%>",
            
            dialogDeleteOnlyThisLabel:    "<%=Resources.CalendarJSResource.deleteSettings_thisLabel%>",
            dialogDeleteFollowingLabel:   "<%=Resources.CalendarJSResource.deleteSettings_followingLabel%>",
            dialogDeleteAllLabel:         "<%=Resources.CalendarJSResource.deleteSettings_allLabel%>",
            
            // buttons
            dialogButton_save:            "<%=Resources.CalendarJSResource.deleteSettings_applyBtn%>",
            dialogButton_cancel:          "<%=Resources.CalendarJSResource.deleteSettings_cancelBtn%>"
        },
        
        icalStream: {
            // dialog            
            newiCalTitle:                         "<%=Resources.CalendarJSResource.icalStream_newLabel%>",
            
            dialogHeader:                         "<%=Resources.CalendarJSResource.icalStream_header%>",
            dialogDescription:                    "<%=Resources.CalendarJSResource.icalStream_description%>",
            
            dialogImportExportLabel:              "<%=Resources.CalendarJSResource.icalStream_importExport%>",
            dialogStreamLink:                     "<%=Resources.CalendarJSResource.icalStream_exportLink%>",
            dialogImportLabel:                    "<%=Resources.CalendarJSResource.icalStream_importtLink%>",
            
            dialogButton_fileSelected:            "<%=Resources.CalendarJSResource.icalStream_fileSelected%>",
            dialogButton_fileNotSelected:         "<%=Resources.CalendarJSResource.icalStream_fileNotSelected%>",
            dialog_incorrectFormat:               "<%=Resources.CalendarJSResource.icalStream_incorrectFormat%>",
            
            dialogInputiCalLabel:                 "<%=Resources.CalendarJSResource.icalStream_inputLabel%>",
            dialogSavediCalLabel:                 "<%=Resources.CalendarJSResource.icalStream_savedLinkLabel%>",
            
            // buttons
            dialogButton_close:                   "<%=Resources.CalendarJSResource.icalStream_closeBtn%>",
            dialogButton_browse:                  "<%=Resources.CalendarJSResource.icalStream_browseBtn%>"
        },

		listView: {
			headerDateFormat: "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern%>",
			monthTitleFormat: "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.YearMonthPattern%>",
			dayTitleFormat:   "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern%>",
			timeFormat:       "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern%>",
			noEventsMessage:  "<%=Resources.CalendarJSResource.calendarListView_noEventsMessage%>"
		}

};
</script>