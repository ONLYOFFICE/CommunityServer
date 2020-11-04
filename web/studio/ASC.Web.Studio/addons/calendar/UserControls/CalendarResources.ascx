<%@ Assembly Name="ASC.Web.Calendar" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CalendarResources.ascx.cs" Inherits="ASC.Web.Calendar.UserControls.CalendarResources" %>

<script language="javascript" type="text/javascript">
var g_fcOptions = {
        isPersonal: "<%= ASC.Core.CoreContext.Configuration.Personal %>" == "True",

		isRTL:           <%=(System.Globalization.CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "true" : "false")%>,

		monthNames:      ["<%=String.Join("\", \"", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.MonthNames, 0, 12)%>"],
		monthNamesShort: ["<%=String.Join("\", \"", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedMonthNames, 0, 12)%>"],

		dayNames:        ["<%=String.Join("\", \"", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.DayNames, 0, 7)%>"],
		dayNamesShort:   ["<%=String.Join("\", \"", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedDayNames, 0, 7)%>"],
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
		    //list:         "<%=Resources.CalendarJSResource.calendarButtonText_list%>",
		    list:         "<%=Resources.CalendarJSResource.calendarButtonNewText_list%>",
		    todo:         "<%=Resources.CalendarJSResource.calendarButtonNewText_todo%>"
		},

		allDayText:          "<%=Resources.CalendarJSResource.calendarAllDayText%>",
		axisFormat:          "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern%>",
		popupCellFormat:     "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.MonthDayPattern%>",

		todayLabel:          "<%=Resources.CalendarJSResource.calendarTodayLabel%>",
		moreEventsLabel:     "<%=Resources.CalendarJSResource.calendarMoreEventsLabel%>",
		addNewEventLabel:    "<%=Resources.CalendarJSResource.calendarAddNewEventLabel%>",
		addNewLabel:         "<%=Resources.CalendarJSResource.calendarAddNewLabel%>",

        newLabel:            "<%=Resources.CalendarJSResource.calendarNewLabel%>",

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
			dialogHeader_import:                    "<%=Resources.CalendarJSResource.calendarCategoriesDialog_importHeader%>",
			dialogHeader_edit:                      "<%=Resources.CalendarJSResource.calendarCategoriesDialog_editHeader%>",
			dialogColor_label:                      "<%=Resources.CalendarJSResource.calendarCategoriesDialog_colorLabel%>",
			dialogTextColor_label:                  "<%=Resources.CalendarJSResource.calendarCategoriesDialog_textColorLabel%>",
			dialogTimezoneLabel:                    "<%=Resources.CalendarJSResource.calendarCategoriesDialog_timezoneLabel%>",
			dialogButton_save:                      "<%=Resources.CalendarJSResource.calendarCategoriesDialog_saveButton%>",
			dialogButton_cancel:                    "<%=Resources.CalendarJSResource.calendarCategoriesDialog_cancelButton%>",
            dialogButton_delete:                    "<%=Resources.CalendarJSResource.calendarCategoriesDialog_deleteButton%>",

            dialogErrorMassageSpecCharacter:        "<%=Resources.CalendarJSResource.dialogErrorMassageSpecCharacter%>",
            dialogCopyMessage:                      "<%=Resources.CalendarJSResource.dialogCopyMessage%>",
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
		    title: "<%=Resources.CalendarJSResource.calendarTodoList_title%>",
			hideLabel: 'Hide ToDo List',
			showLabel: 'Show ToDo List',
			addTodoLabel: "<%=Resources.CalendarJSResource.calendarTodoList_addTodoLabel%>",
			
		    overdue:  "<%=Resources.CalendarJSResource.calendarTodoList_Overdue%>",
		    noDueDate:  "<%=Resources.CalendarJSResource.calendarTodoList_noDueDate%>",

            newTodoTitle: "<%=Resources.CalendarJSResource.calendarTodoList_newTodoTitle%>",
			menu: {
				hideColmpletedTodos: {hide: 'Hide completed items', show: 'Show completed items'},
				deleteCompletedTodos: 'Delete completed items'
			},
			sortByCalendarLabel: 'Sort by calendar',
			sortByPriorityLabel: 'Sort by priority',
			sortAlphabeticallyLabel: 'Sort alphabetically',
		    
			menuTodoInCalendar: "<%=Resources.CalendarJSResource.menuTodoInCalendar%>",
		    menuDeleteMarkedTodo: "<%=Resources.CalendarJSResource.menuDeleteMarkedTodo%>",
		    menuSyncLinks: "<%=Resources.CalendarJSResource.menuSyncLinks%>",
		},
        deleteTodoDialog: {
            dialogTemplate: "",
            
            dialogHeader: "<%=Resources.CalendarJSResource.calendarTodoDeleteDialog_dialogHeader%>",
        
            dialogSingleBody: "<%=Resources.CalendarJSResource.calendarTodoDeleteDialog_dialogSingleBody%>",
            
            dialogButton_apply: "<%=Resources.CalendarJSResource.calendarTodoDeleteDialog_dialogButton_apply%>",
            dialogButton_cancel: "<%=Resources.CalendarJSResource.calendarTodoDeleteDialog_dialogButton_cancel%>",
        },


        todoEditor: {
            dialogHeader_add:  "<%=Resources.CalendarJSResource.calendarTodoEditor_dialogHeader_add%>",
            dialogHeader_edit: "<%=Resources.CalendarJSResource.calendarTodoEditor_dialogHeader_edit%>",

            dialogDateLabel:  "<%=Resources.CalendarJSResource.calendarTodoEditor_dialogDateLabel%>",
		    
            titleLabel:  "<%=Resources.CalendarJSResource.calendarTodoEditor_titleLabel%>",
            descriptionLabel:  "<%=Resources.CalendarJSResource.calendarTodoEditor_descriptionLabel%>",

            dialogButton_save:  "<%=Resources.CalendarJSResource.calendarEventEditor_saveButton%>",
            dialogButton_cancel:  "<%=Resources.CalendarJSResource.calendarEventEditor_cancelButton%>",
            
        },

        todoViewer:{
            dialogButton_mark_on: "<%=Resources.CalendarJSResource.todoViewer_dialogButton_mark_on%>",
            dialogButton_mark_off: "<%=Resources.CalendarJSResource.todoViewer_dialogButton_mark_off%>",
            dialogButton_edit: "<%=Resources.CalendarJSResource.todoViewer_dialogButton_edit%>",
            dialogButton_delete: "<%=Resources.CalendarJSResource.todoViewer_dialogButton_delete%>"
        },
        
        eventEditor: {
			newEventTitle:               "<%=Resources.CalendarJSResource.calendar_newEventTitle%>",
			// dialog
			dialogHeader_add:            "<%=Resources.CalendarJSResource.calendarEventEditor_addHeader%>",
			dialogSummaryLabel:          "<%=Resources.CalendarJSResource.calendarEventEditor_summaryLabel%>",

            eventButton:                 "<%=Resources.CalendarJSResource.calendarEventEditor_eventButton%>",
            todoButton:                  "<%=Resources.CalendarJSResource.calendarEventEditor_todoButton%>",

			dialogLocationLabel:         "<%=Resources.CalendarJSResource.calendarEventEditor_locationLabel%>",
			dialogAttendeesLabel:        "<%=Resources.CalendarJSResource.calendarEventEditor_attendeesLabel%>",
			dialogOwnerLabel:            "<%=Resources.CalendarJSResource.calendarEventEditor_ownerLabel%>",
			dialogOrganizerLabel:        "<%=Resources.CalendarJSResource.calendarEventEditor_organizerLabel%>",
			dialogAllDayLabel:           "<%=Resources.CalendarJSResource.calendarEventEditor_allDayLabel%>",
			dialogSentInvitations:       "<%=Resources.CalendarJSResource.calendarEventEditor_sentInvitationsLabel%>",
			dialogAllDay_no:             "<%=Resources.CalendarJSResource.calendarEventEditor_notAllDayEvent%>",
			dialogAllDay_yes:            "<%=Resources.CalendarJSResource.calendarEventEditor_allDayEvent%>",
			dialogFromLabel:             "<%=Resources.CalendarJSResource.calendarEventEditor_fromLabel%>",
			dialogToLabel:               "<%=Resources.CalendarJSResource.calendarEventEditor_toLabel%>",
			dialogRepeatLabel:           "<%=Resources.CalendarJSResource.calendarEventEditor_repeatLabel%>",
			dialogStatusLabel:           "<%=Resources.CalendarJSResource.calendarEventEditor_statusLabel%>",
			dialogStatusOption_tentative:"<%=Resources.CalendarJSResource.calendarEventEditor_statusTentative%>",
			dialogStatusOption_confirmed:"<%=Resources.CalendarJSResource.calendarEventEditor_statusConfirmed%>",
			dialogStatusOption_cancelled:"<%=Resources.CalendarJSResource.calendarEventEditor_statusCancelled%>",
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
		    dialogButton_unsubscribe:    "<%=Resources.CalendarJSResource.calendarEventEditor_unsubscribeButton%>",
		    dialogHeader_createEvent:    "<%=Resources.CalendarJSResource.calendarEventEditor_headerCreate%>",
		    dialogHeader_editEvent:      "<%=Resources.CalendarJSResource.calendarEventEditor_headerEdit%>",
		    dialogHeader_viewEvent:      "<%=Resources.CalendarJSResource.calendarEventEditor_headerView%>",
		    dialogButton_moreDetails:    "<%=Resources.CalendarJSResource.calendarEventEditor_moreDetailsButtons%>",
            dialogButton_details:        "<%=Resources.CalendarJSResource.calendarEventEditor_detailsButton%>"
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
            
            dayNames:                     ["<%=String.Join("\", \"", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.DayNames, 0, 7)%>"],            
            dayNamesShort:                ["<%=String.Join("\", \"", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedDayNames, 0, 7)%>"],
            
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
            dialogSingleHeader:           "<%=Resources.CalendarJSResource.deleteSettings_singleHeader%>",
            dialogSingleBody:             "<%=Resources.CalendarJSResource.deleteSettings_singleBody%>",
            
            dialogDeleteOnlyThisLabel:    "<%=Resources.CalendarJSResource.deleteSettings_thisLabel%>",
            dialogDeleteFollowingLabel:   "<%=Resources.CalendarJSResource.deleteSettings_followingLabel%>",
            dialogDeleteAllLabel:         "<%=Resources.CalendarJSResource.deleteSettings_allLabel%>",
            
            // buttons
            dialogButton_save:            "<%=Resources.CalendarJSResource.deleteSettings_applyBtn%>",
            dialogButton_cancel:          "<%=Resources.CalendarJSResource.deleteSettings_cancelBtn%>"
        },
        
        confirmPopup: {
            // dialog
            dialogAddEventHeader:         "<%=Resources.CalendarJSResource.confirmPopup_AddEventHeader%>",
            dialogUpdateEventHeader:      "<%=Resources.CalendarJSResource.confirmPopup_UpdateEventHeader%>",
            dialogDeleteEventHeader:      "<%=Resources.CalendarJSResource.confirmPopup_DeleteEventHeader%>",
            dialogAddEventBody:           "<%=Resources.CalendarJSResource.confirmPopup_AddEventBody%>",
            dialogUpdateEventBody:        "<%=Resources.CalendarJSResource.confirmPopup_UpdateEventBody%>",
            dialogUpdateGuestsBody:       "<%=Resources.CalendarJSResource.confirmPopup_UpdateGuestsBody%>",
            dialogDeleteEventBody:        "<%=Resources.CalendarJSResource.confirmPopup_DeleteEventBody%>",
            
            dialogSuccessToastText:       "<%=Resources.CalendarJSResource.confirmPopup_SuccessToastText%>",
            dialogErrorToastText:         "<%=Resources.CalendarJSResource.confirmPopup_ErrorToastText%>",

            // buttons
            dialogButtonSend:            "<%=Resources.CalendarJSResource.confirmPopup_ButtonSend%>",
            dialogButtonSendCustoms:     "<%=Resources.CalendarJSResource.confirmPopup_ButtonSendCustoms%>",
            dialogButtonSendEveryone:    "<%=Resources.CalendarJSResource.confirmPopup_ButtonSendEveryone%>",
            dialogButtonDontSend:        "<%=Resources.CalendarJSResource.confirmPopup_ButtonDontSend%>",

            // infotext
            editorInfoText:              "<%=Resources.CalendarJSResource.editorInfoText%>",
            editorInfoTextSubscription:  "<%=Resources.CalendarJSResource.editorInfoTextSubscription%>"
        },

        icalStream: {
            // dialog            
            newiCalTitle:                         "<%=Resources.CalendarJSResource.icalStream_newLabel%>",
            importEventsTitle:                    "<%=Resources.CalendarJSResource.icalStream_importEventsLabel%>",

            dialogHeader:                         "<%=Resources.CalendarJSResource.icalStream_export_header%>",
            dialogTodoDescription:                "<%=Resources.CalendarJSResource.icalStream_export_todo_description%>",
            dialogDescription:                    "<%=Resources.CalendarJSResource.icalStream_export_description%>",
            dialogCaldavHelp:                     "<%=Resources.CalendarJSResource.icalStream_export_help%>",
            dialogHelpCenter:                     "<%=Resources.CalendarJSResource.icalStream_export_help_center%>",
            dialogPreparingMessage:               "<%=Resources.CalendarJSResource.icalStream_preparing_message%>",
            dialogPreparingErrorMessage:          "<%=Resources.CalendarJSResource.icalStream_preparing_error_message%>",

            dialogCopyButton:                    "<%=Resources.CalendarJSResource.icalStream_export_copy_button%>",
            dialogTryAgainButton:                "<%=Resources.CalendarJSResource.icalStream_export_try_again_button%>",
            dialogExportCalDav:                  "<%=Resources.CalendarJSResource.icalStream_export_caldav%>",
            dialogExportIcal:                    "<%=Resources.CalendarJSResource.icalStream_export_ical%>",

            dialogImportExportLabel:              "<%=Resources.CalendarJSResource.icalStream_importExport%>",
            dialogStreamLink:                     "<%: Resources.CalendarJSResource.icalStream_exportLink%>",
            dialogImportLabel:                    "<%: Resources.CalendarJSResource.icalStream_importtLink%>",
            dialogImportLabelNew:                 "<%: Resources.CalendarJSResource.icalStream_importLinkNew%>",

            dialogButton_fileSelected:            "<%=Resources.CalendarJSResource.icalStream_fileSelected%>",
            dialogButton_fileNotSelected:         "<%=Resources.CalendarJSResource.icalStream_fileNotSelected%>",
            dialog_incorrectFormat:               "<%=Resources.CalendarJSResource.icalStream_incorrectFormat%>",
            
            dialogInputiCalLabel:                 "<%=Resources.CalendarJSResource.icalStream_inputLabel%>",
            dialogSavediCalLabel:                 "<%=Resources.CalendarJSResource.icalStream_savedLinkLabel%>",

            dialogExportLink:                     "<%=Resources.CalendarJSResource.icalStream_export_header%>",
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