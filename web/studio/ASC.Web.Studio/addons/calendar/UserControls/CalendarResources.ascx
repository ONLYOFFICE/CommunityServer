<%@ Assembly Name="ASC.Web.Calendar" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CalendarResources.ascx.cs" Inherits="ASC.Web.Calendar.UserControls.CalendarResources" %>

<%@ Import Namespace="ASC.Web.Calendar.Resources" %>
<%@ Import Namespace="ASC.Web.Calendar.Handlers" %>
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
			today:        "<%=CalendarJSResource.calendarButtonText_today%>",
			month:        "<%=CalendarJSResource.calendarButtonText_month%>",
			week:         "<%=CalendarJSResource.calendarButtonText_week%>",
			day:          "<%=CalendarJSResource.calendarButtonText_day%>",
		    //list:         "<%=CalendarJSResource.calendarButtonText_list%>",
		    list:         "<%=CalendarJSResource.calendarButtonNewText_list%>",
		    todo:         "<%=CalendarJSResource.calendarButtonNewText_todo%>"
		},

		allDayText:          "<%=CalendarJSResource.calendarAllDayText%>",
		axisFormat:          "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern%>",
		popupCellFormat:     "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.MonthDayPattern%>",

		todayLabel:          "<%=CalendarJSResource.calendarTodayLabel%>",
		moreEventsLabel:     "<%=CalendarJSResource.calendarMoreEventsLabel%>",
		addNewEventLabel:    "<%=CalendarJSResource.calendarAddNewEventLabel%>",
		addNewLabel:         "<%=CalendarJSResource.calendarAddNewLabel%>",

        newLabel:            "<%=CalendarJSResource.calendarNewLabel%>",

		modes: {
			calendarViewLabel: "<%=CalendarJSResource.calendarViewLabelText_calendar%>",
			listViewLabel:     "<%=CalendarJSResource.calendarViewLabelText_list%>"
		},

		sharedList: {
			title:             "<%=CalendarJSResource.calendarSharedList_title%>",
			moreLink:          "<%=CalendarJSResource.calendarSharedList_moreLink%>",
			addLink:           "<%=CalendarJSResource.calendarSharedList_addLink%>"
		},

		categories: {
			// list items
			defaultTitle:                           "<%=CalendarJSResource.calendar_newCalendarTitle%>",
			// list of calendars
			title:                                  "<%=CalendarJSResource.calendarCategoriesList_calendarsHeader%>",
			addNewCategoryLabel:                    "<%=CalendarJSResource.calendarCategoriesList_addNewCategoryLabel%>",
			dialogHeader_add:                       "<%=CalendarJSResource.calendarCategoriesDialog_addHeader%>",
			dialogHeader_import:                    "<%=CalendarJSResource.calendarCategoriesDialog_importHeader%>",
			dialogHeader_edit:                      "<%=CalendarJSResource.calendarCategoriesDialog_editHeader%>",
			dialogColor_label:                      "<%=CalendarJSResource.calendarCategoriesDialog_colorLabel%>",
			dialogTextColor_label:                  "<%=CalendarJSResource.calendarCategoriesDialog_textColorLabel%>",
			dialogTimezoneLabel:                    "<%=CalendarJSResource.calendarCategoriesDialog_timezoneLabel%>",
			dialogButton_save:                      "<%=CalendarJSResource.calendarCategoriesDialog_saveButton%>",
			dialogButton_cancel:                    "<%=CalendarJSResource.calendarCategoriesDialog_cancelButton%>",
            dialogButton_delete:                    "<%=CalendarJSResource.calendarCategoriesDialog_deleteButton%>",

            dialogErrorMassageSpecCharacter:        "<%=CalendarJSResource.dialogErrorMassageSpecCharacter%>",
            dialogCopyMessage:                      "<%=CalendarJSResource.dialogCopyMessage%>",
			// list of subscriptions
			subscriptionsTitle:                     "<%=CalendarJSResource.calendarCategoriesList_subscriptionsHeader%>",
			//
			subscriptionsDialogHeader:              "<%=CalendarJSResource.calendarSubscriptionsDialog_header%>",
			subscriptionsDialogOwnerLabel:          "<%=CalendarJSResource.calendarSubscriptionsDialog_ownerLabel%>",
			subscriptionsDialogButton_unsubscribe:  "<%=CalendarJSResource.calendarSubscriptionsDialog_unsubscribeButton%>",
			//
			subscriptionsManageLabel:               "<%=CalendarJSResource.calendarCategoriesList_subscriptionsManageLabel%>",
			subscriptionsManageDialog_title:        "<%=CalendarJSResource.calendarSubscriptionsManageDialog_title%>",
			subscriptionsManageDialog_qsearchText:  "<%=CalendarJSResource.calendarSubscriptionsManageDialog_qSearchText%>",
			subscriptionsManageDialogButton_save:   "<%=CalendarJSResource.calendarSubscriptionsManageDialog_saveButton%>",
			subscriptionsManageDialogButton_cancel: "<%=CalendarJSResource.calendarSubscriptionsManageDialog_cancelButton%>",
			// datepicker
			datepickerHideLabel:                    "<%=CalendarJSResource.calendarMiniCalendar_hideLabel%>",
			datepickerShowLabel:                    "<%=CalendarJSResource.calendarMiniCalendar_showLabel%>"
		},

		todoList: {
		    title: "<%=CalendarJSResource.calendarTodoList_title%>",
			hideLabel: 'Hide ToDo List',
			showLabel: 'Show ToDo List',
			addTodoLabel: "<%=CalendarJSResource.calendarTodoList_addTodoLabel%>",
			
		    overdue:  "<%=CalendarJSResource.calendarTodoList_Overdue%>",
		    noDueDate:  "<%=CalendarJSResource.calendarTodoList_noDueDate%>",

            newTodoTitle: "<%=CalendarJSResource.calendarTodoList_newTodoTitle%>",
			menu: {
				hideColmpletedTodos: {hide: 'Hide completed items', show: 'Show completed items'},
				deleteCompletedTodos: 'Delete completed items'
			},
			sortByCalendarLabel: 'Sort by calendar',
			sortByPriorityLabel: 'Sort by priority',
			sortAlphabeticallyLabel: 'Sort alphabetically',
		    
			menuTodoInCalendar: "<%=CalendarJSResource.menuTodoInCalendar%>",
		    menuDeleteMarkedTodo: "<%=CalendarJSResource.menuDeleteMarkedTodo%>",
		    menuSyncLinks: "<%=CalendarJSResource.menuSyncLinks%>",
		},
        deleteTodoDialog: {
            dialogTemplate: "",
            
            dialogHeader: "<%=CalendarJSResource.calendarTodoDeleteDialog_dialogHeader%>",
        
            dialogSingleBody: "<%=CalendarJSResource.calendarTodoDeleteDialog_dialogSingleBody%>",
            
            dialogButton_apply: "<%=CalendarJSResource.calendarTodoDeleteDialog_dialogButton_apply%>",
            dialogButton_cancel: "<%=CalendarJSResource.calendarTodoDeleteDialog_dialogButton_cancel%>",
        },


        todoEditor: {
            dialogHeader_add:  "<%=CalendarJSResource.calendarTodoEditor_dialogHeader_add%>",
            dialogHeader_edit: "<%=CalendarJSResource.calendarTodoEditor_dialogHeader_edit%>",

            dialogDateLabel:  "<%=CalendarJSResource.calendarTodoEditor_dialogDateLabel%>",
		    
            titleLabel:  "<%=CalendarJSResource.calendarTodoEditor_titleLabel%>",
            descriptionLabel:  "<%=CalendarJSResource.calendarTodoEditor_descriptionLabel%>",

            dialogButton_save:  "<%=CalendarJSResource.calendarEventEditor_saveButton%>",
            dialogButton_cancel:  "<%=CalendarJSResource.calendarEventEditor_cancelButton%>",
            
        },

        todoViewer:{
            dialogButton_mark_on: "<%=CalendarJSResource.todoViewer_dialogButton_mark_on%>",
            dialogButton_mark_off: "<%=CalendarJSResource.todoViewer_dialogButton_mark_off%>",
            dialogButton_edit: "<%=CalendarJSResource.todoViewer_dialogButton_edit%>",
            dialogButton_delete: "<%=CalendarJSResource.todoViewer_dialogButton_delete%>"
        },
        
        eventEditor: {
			newEventTitle:               "<%=CalendarJSResource.calendar_newEventTitle%>",
			// dialog
			dialogHeader_add:            "<%=CalendarJSResource.calendarEventEditor_addHeader%>",
			dialogSummaryLabel:          "<%=CalendarJSResource.calendarEventEditor_summaryLabel%>",

            eventButton:                 "<%=CalendarJSResource.calendarEventEditor_eventButton%>",
            todoButton:                  "<%=CalendarJSResource.calendarEventEditor_todoButton%>",

			dialogLocationLabel:         "<%=CalendarJSResource.calendarEventEditor_locationLabel%>",
			dialogAttendeesLabel:        "<%=CalendarJSResource.calendarEventEditor_attendeesLabel%>",
			dialogOwnerLabel:            "<%=CalendarJSResource.calendarEventEditor_ownerLabel%>",
			dialogOrganizerLabel:        "<%=CalendarJSResource.calendarEventEditor_organizerLabel%>",
			dialogAllDayLabel:           "<%=CalendarJSResource.calendarEventEditor_allDayLabel%>",
			dialogSentInvitations:       "<%=CalendarJSResource.calendarEventEditor_sentInvitationsLabel%>",
			dialogAllDay_no:             "<%=CalendarJSResource.calendarEventEditor_notAllDayEvent%>",
			dialogAllDay_yes:            "<%=CalendarJSResource.calendarEventEditor_allDayEvent%>",
			dialogFromLabel:             "<%=CalendarJSResource.calendarEventEditor_fromLabel%>",
			dialogToLabel:               "<%=CalendarJSResource.calendarEventEditor_toLabel%>",
			dialogRepeatLabel:           "<%=CalendarJSResource.calendarEventEditor_repeatLabel%>",
			dialogStatusLabel:           "<%=CalendarJSResource.calendarEventEditor_statusLabel%>",
			dialogStatusOption_tentative:"<%=CalendarJSResource.calendarEventEditor_statusTentative%>",
			dialogStatusOption_confirmed:"<%=CalendarJSResource.calendarEventEditor_statusConfirmed%>",
			dialogStatusOption_cancelled:"<%=CalendarJSResource.calendarEventEditor_statusCancelled%>",
			dialogRepeatOption_never:    "<%=CalendarJSResource.calendarEventEditor_repeatNever%>",
			dialogRepeatOption_day:      "<%=CalendarJSResource.calendarEventEditor_repeatDaily%>",
			dialogRepeatOption_week:     "<%=CalendarJSResource.calendarEventEditor_repeatWeekly%>",
			dialogRepeatOption_month:    "<%=CalendarJSResource.calendarEventEditor_repeatMonthly%>",
			dialogRepeatOption_year:     "<%=CalendarJSResource.calendarEventEditor_repeatYearly%>",
			dialogRepeatOption_custom:   "<%=CalendarJSResource.calendarEventEditor_repeatCustom%>",
			dialogAlertLabel:            "<%=CalendarJSResource.calendarEventEditor_alertLabel%>",
			dialogAlertOption_default:   "<%=CalendarJSResource.calendarEventEditor_alertDefault%>",
			dialogAlertOption_never:     "<%=CalendarJSResource.calendarEventEditor_alertNever%>",
			dialogAlertOption_5minutes:  "<%=CalendarJSResource.calendarEventEditor_alert5Minutes%>",
			dialogAlertOption_15minutes: "<%=CalendarJSResource.calendarEventEditor_alert15Minutes%>",
			dialogAlertOption_30minutes: "<%=CalendarJSResource.calendarEventEditor_alert30Minutes%>",
			dialogAlertOption_hour:      "<%=CalendarJSResource.calendarEventEditor_alertHour%>",
			dialogAlertOption_2hours:    "<%=CalendarJSResource.calendarEventEditor_alert2Hours%>",
			dialogAlertOption_day:       "<%=CalendarJSResource.calendarEventEditor_alertDay%>",
			dialogSharing_no:            "<%=CalendarJSResource.calendarEventEditor_sharingNoLabel%>",
			dialogCalendarLabel:         "<%=CalendarJSResource.calendarEventEditor_calendarLabel%>",
			dialogDescriptionLabel:      "<%=CalendarJSResource.calendarEventEditor_descriptionLabel%>",
			dialogButton_edit:           "<%=CalendarJSResource.calendarEventEditor_editButton%>",
			dialogButton_save:           "<%=CalendarJSResource.calendarEventEditor_saveButton%>",
			dialogButton_close:          "<%=CalendarJSResource.calendarEventEditor_closeButton%>",
			dialogButton_cancel:         "<%=CalendarJSResource.calendarEventEditor_cancelButton%>",
			dialogButton_delete:         "<%=CalendarJSResource.calendarEventEditor_deleteButton%>",
		    dialogButton_unsubscribe:    "<%=CalendarJSResource.calendarEventEditor_unsubscribeButton%>",
		    dialogHeader_createEvent:    "<%=CalendarJSResource.calendarEventEditor_headerCreate%>",
		    dialogHeader_editEvent:      "<%=CalendarJSResource.calendarEventEditor_headerEdit%>",
		    dialogHeader_viewEvent:      "<%=CalendarJSResource.calendarEventEditor_headerView%>",
		    dialogButton_moreDetails:    "<%=CalendarJSResource.calendarEventEditor_moreDetailsButtons%>",
            dialogButton_details:        "<%=CalendarJSResource.calendarEventEditor_detailsButton%>"
		},
		
		repeatSettings: {
            // dialog
            dialogHeader:                 "<%=CalendarJSResource.repeatSettings_header%>",
            
            // start date
            dialogFromLabel:              "<%=CalendarJSResource.repeatSetting_fromLabel%>",
            
            // end of repeat
            dialogToLabel:                "<%=CalendarJSResource.repeatSettings_toLabel%>",
            dialogOptionNever:            "<%=CalendarJSResource.repeatSettings_optionNever%>",
            dialogOptionDate:             "<%=CalendarJSResource.repeatSettings_optionDate%>",
            dialogOptionCount:            "<%=CalendarJSResource.repeatSettings_optionCount%>",
            
            dialogAfterLabel:             "<%=CalendarJSResource.repeatSettings_afterLabel%>",
            dialogTimesLabel:             "<%=CalendarJSResource.repeatSettings_timesLabel%>",
            
            // repeat by 
            dialogRepeatOnLabel:          "<%=CalendarJSResource.repeatSettings_onLabel%>",
            dialogRepeatOn_days:          "<%=CalendarJSResource.repeatSettings_onDays%>",
            dialogRepeatOn_weeks:         "<%=CalendarJSResource.repeatSettings_onWeeks%>",
            dialogRepeatOn_months:        "<%=CalendarJSResource.repeatSettings_onMonths%>",
            dialogRepeatOn_years:         "<%=CalendarJSResource.repeatSettings_onYears%>",
            
            // interval
            dialogEachLabel:              "<%=CalendarJSResource.repeatSettings_eachLabel%>",
            dialogAliasLabel:             "<%=CalendarJSResource.repeatSettings_aliasLabel%>",
            dialogIntervalOption_day:     "<%=CalendarJSResource.repeatSettings_intervalDay%>",
            dialogIntervalOption_week:    "<%=CalendarJSResource.repeatSettings_intervalWeek%>",
            dialogIntervalOption_month:   "<%=CalendarJSResource.repeatSettings_intervalMonth%>",
            dialogIntervalOption_year:    "<%=CalendarJSResource.repeatSettings_intervalYear%>",
            
            dayNames:                     ["<%=String.Join("\", \"", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.DayNames, 0, 7)%>"],            
            dayNamesShort:                ["<%=String.Join("\", \"", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedDayNames, 0, 7)%>"],
            
            dayAliasNames:                ["<%=CalendarJSResource.repeatSettings_firstLabel%>",
                                           "<%=CalendarJSResource.repeatSettings_secondLabel%>",
                                           "<%=CalendarJSResource.repeatSettings_thirdLabel%>",
                                           "<%=CalendarJSResource.repeatSettings_penultimateLabel%>",
                                           "<%=CalendarJSResource.repeatSettings_lastLabel%>"],
            
            // buttons
            dialogButton_save:            "<%=CalendarJSResource.repeatSettings_saveBtn%>",
            dialogButton_cancel:          "<%=CalendarJSResource.repeatSettings_cancelBtn%>"
        },
        
        deleteSettings: {
            // dialog            
            dialogHeader:                 "<%=CalendarJSResource.deleteSettings_header%>",
            dialogSingleHeader:           "<%=CalendarJSResource.deleteSettings_singleHeader%>",
            dialogSingleBody:             "<%=CalendarJSResource.deleteSettings_singleBody%>",
            
            dialogDeleteOnlyThisLabel:    "<%=CalendarJSResource.deleteSettings_thisLabel%>",
            dialogDeleteFollowingLabel:   "<%=CalendarJSResource.deleteSettings_followingLabel%>",
            dialogDeleteAllLabel:         "<%=CalendarJSResource.deleteSettings_allLabel%>",
            
            // buttons
            dialogButton_save:            "<%=CalendarJSResource.deleteSettings_applyBtn%>",
            dialogButton_cancel:          "<%=CalendarJSResource.deleteSettings_cancelBtn%>"
        },
        
        confirmPopup: {
            // dialog
            dialogAddEventHeader:         "<%=CalendarJSResource.confirmPopup_AddEventHeader%>",
            dialogUpdateEventHeader:      "<%=CalendarJSResource.confirmPopup_UpdateEventHeader%>",
            dialogDeleteEventHeader:      "<%=CalendarJSResource.confirmPopup_DeleteEventHeader%>",
            dialogAddEventBody:           "<%=CalendarJSResource.confirmPopup_AddEventBody%>",
            dialogUpdateEventBody:        "<%=CalendarJSResource.confirmPopup_UpdateEventBody%>",
            dialogUpdateGuestsBody:       "<%=CalendarJSResource.confirmPopup_UpdateGuestsBody%>",
            dialogDeleteEventBody:        "<%=CalendarJSResource.confirmPopup_DeleteEventBody%>",
            
            dialogSuccessToastText:       "<%=CalendarJSResource.confirmPopup_SuccessToastText%>",
            dialogErrorToastText:         "<%=CalendarJSResource.confirmPopup_ErrorToastText%>",

            // buttons
            dialogButtonSend:            "<%=CalendarJSResource.confirmPopup_ButtonSend%>",
            dialogButtonSendCustoms:     "<%=CalendarJSResource.confirmPopup_ButtonSendCustoms%>",
            dialogButtonSendEveryone:    "<%=CalendarJSResource.confirmPopup_ButtonSendEveryone%>",
            dialogButtonDontSend:        "<%=CalendarJSResource.confirmPopup_ButtonDontSend%>",

            // infotext
            editorInfoText:              "<%=CalendarJSResource.editorInfoText%>",
            editorInfoTextSubscription:  "<%=CalendarJSResource.editorInfoTextSubscription%>"
        },

        icalStream: {
            // dialog            
            newiCalTitle:                         "<%=CalendarJSResource.icalStream_newLabel%>",
            importEventsTitle:                    "<%=CalendarJSResource.icalStream_importEventsLabel%>",

            dialogHeader:                         "<%=CalendarJSResource.icalStream_export_header%>",
            dialogTodoDescription:                "<%=CalendarJSResource.icalStream_export_todo_description%>",
            dialogDescription:                    "<%=CalendarJSResource.icalStream_export_description%>",
            dialogCaldavHelp:                     "<%=CalendarJSResource.icalStream_export_help%>",
            dialogHelpCenter:                     "<%=CalendarJSResource.icalStream_export_help_center%>",
            dialogPreparingMessage:               "<%=CalendarJSResource.icalStream_preparing_message%>",
            dialogPreparingErrorMessage:          "<%=CalendarJSResource.icalStream_preparing_error_message%>",

            dialogCopyButton:                    "<%=CalendarJSResource.icalStream_export_copy_button%>",
            dialogTryAgainButton:                "<%=CalendarJSResource.icalStream_export_try_again_button%>",
            dialogExportCalDav:                  "<%=CalendarJSResource.icalStream_export_caldav%>",
            dialogExportIcal:                    "<%=CalendarJSResource.icalStream_export_ical%>",

            dialogImportExportLabel:              "<%=CalendarJSResource.icalStream_importExport%>",
            dialogStreamLink:                     "<%: CalendarJSResource.icalStream_exportLink%>",
            dialogImportLabel:                    "<%: CalendarJSResource.icalStream_importtLink%>",
            dialogImportLabelNew:                 "<%: CalendarJSResource.icalStream_importLinkNew%>",

            dialogButton_fileSelected:            "<%=CalendarJSResource.icalStream_fileSelected%>",
            dialogButton_fileNotSelected:         "<%=CalendarJSResource.icalStream_fileNotSelected%>",
            dialog_incorrectFormat:               "<%=CalendarJSResource.icalStream_incorrectFormat%>",
            
            dialogInputiCalLabel:                 "<%=CalendarJSResource.icalStream_inputLabel%>",
            dialogSavediCalLabel:                 "<%=CalendarJSResource.icalStream_savedLinkLabel%>",

            dialogExportLink:                     "<%=CalendarJSResource.icalStream_export_header%>",
            // buttons
            dialogButton_close:                   "<%=CalendarJSResource.icalStream_closeBtn%>",
            dialogButton_browse:                  "<%=CalendarJSResource.icalStream_browseBtn%>"
        },

		listView: {
			headerDateFormat: "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern%>",
			monthTitleFormat: "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.YearMonthPattern%>",
			dayTitleFormat:   "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern%>",
			timeFormat:       "<%=System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern%>",
			noEventsMessage:  "<%=CalendarJSResource.calendarListView_noEventsMessage%>"
		},

		attachments: {
			bytes: "<%=CalendarJSResource.calendarEventAttachments_bytes%>",
			kilobytes: "<%=CalendarJSResource.calendarEventAttachments_kilobytes%>",
            megabytes: "<%=CalendarJSResource.calendarEventAttachments_megabytes%>",

            maxFileSizeInMegaBytes: <%=FilesUploader.MaxFileSizeInMegabytes%>,

            attachDeleteAllLabel: "<%=CalendarJSResource.calendarEventAttachments_attachDeleteAllLabel%>",
            attachFilesFromDocuments: "<%=CalendarJSResource.calendarEventAttachments_attachFilesFromDocuments%>",
            attachmentsLabelHelpInfo: "<%=CalendarJSResource.calendarEventAttachments_attachmentsLabelHelpInfo%>",
            copyFileToMyDocumentsFolderErrorMsg: "<%=CalendarJSResource.calendarEventAttachments_copyFileToMyDocumentsFolderErrorMsg%>",
            copyFilesToMyDocumentsBtn: "<%=CalendarJSResource.calendarEventAttachments_copyFilesToMyDocumentsBtn%>",
            copyingToMyDocumentsLabel: "<%=CalendarJSResource.calendarEventAttachments_copyingToMyDocumentsLabel%>",
            documentAccessDeniedError: "<%=CalendarJSResource.calendarEventAttachments_documentAccessDeniedError%>",
            emptyFileNotSupportedError: "<%=CalendarJSResource.calendarEventAttachments_emptyFileNotSupportedError%>",
            executableWarning: "<%=CalendarJSResource.calendarEventAttachments_executableWarning%>",
            fileSizeError: "<%=CalendarJSResource.calendarEventAttachments_fileSizeError%>",
            insertedViaLink: "<%=CalendarJSResource.calendarEventAttachments_insertedViaLink%>",
            limitLabel: "<%=CalendarJSResource.calendarEventAttachments_limitLabel%>",
            uploadFile: "<%=CalendarJSResource.calendarEventAttachments_uploadFile%>",
            uploadedLabel: "<%=CalendarJSResource.calendarEventAttachments_uploadedLabel%>",
            uploadingLabel: "<%=CalendarJSResource.calendarEventAttachments_uploadingLabel%>",
            warningLabel: "<%=CalendarJSResource.calendarEventAttachments_warningLabel%>"
		}

};
</script>