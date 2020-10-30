<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Import Namespace="ASC.Core" %>

<%@ Import Namespace="ASC.Data.Storage" %>
<script id="categoriesDialogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_cal_editor">
	        <div class="header">
		        <div class="inner">
			        <span class="new-label">${dialogHeaderAdd}</span>
                    <span class="edit-label">${dialogHeaderEdit}</span>
                    <span class="import-label">${dialogHeaderImport}</span>
			        <div class="close-btn">&times;</div>
		        </div>
	        </div>
            <div class="choose_event_source">
                <span id="events_link" class="active"><%=Resources.CalendarJSResource.calendarExportEvents_byLink%></span>
                <span id="events_file"><%=Resources.CalendarJSResource.calendarExportEvents_fromFile%></span>
            </div>
            
	        <!-- create iCal-calendar -->
	        <div class="ical-url-input">
		        <div class="ical-label">${dialogInputiCalLabel}</div>
		        <input type="text" value=""/>
	        </div>
            <div class="sync-with-calendar inline-block cbx-container">
                <div>
                    <input type="checkbox"/>
                    <label>
                        <%=Resources.CalendarJSResource.calendarExportEvents_syncByLink%>
                    </label>
                </div>
			</div>
            <!-- get/set iCal stream-->
	        <div class="ical">
		        
		        <div class="ical-selectors">
			        
			        <div class="ical-import">
				        <span id="ical-browse-btn" class="ical-link">${dialogImportLabelNew}</span>
				        <span class="ical-file-selected">${fileNotSelected}</span>
                        <span class="ical-file-del">x</span>
			        </div>
		        </div>
	        </div>
            <div class="botttom-indent clearFix calendar">
                <div class="halfwidth">
                    <div class="label"><%=Resources.CalendarJSResource.calendarImportEvents_calendarLabel%></div>
                    <div style="position: relative;">
				        <div class="bullet"></div>
				        <select></select>
			        </div>
                </div>
		    </div>
            <div class="title">
		        <div class="bullet"></div>
		        <input type="text" value="${defaultCalendarName}" maxlength="${maxlength}"/>
	        </div>
	        <div class="color">
		        <span class="label">${dialogBgColorLabel}</span>
		        <span class="outer">
                    <span class="inner">&nbsp;</span>
		        </span>
		        <span class="label-for-text">${dialogFontColorLabel}</span>
		        <span class="outer">
                    <span class="inner-for-text">&nbsp;</span>
		        </span>
	        </div>
	        <div class="row">
		        <div class="alert">
			        <div class="label">${dialogAlertLabel}</div>
			        <select>
                        {{each(index, alertOption) alertOptions}}
                            <option value="${alertOption.value}">${alertOption.text}</option>    
                        {{/each}}
			        </select>
		        </div>
		        <div class="timezone">
			        <div class="label">${dialogTimezoneLabel}</div>
			        <select></select>
		        </div>
	        </div>
	        <div class="shared-list"/>
            <div class="export">
				<span class="export-link link">${dialogExportLink}</span>
	        </div>
	
	        
	        <div class="buttons">
		        <a class="button blue middle save-btn" href="#">${dialogButtonSave}</a>
		        <a class="button gray middle cancel-btn" href="#">${dialogButtonCancel}</a>
		        <a class="button gray middle delete-btn" href="#">${dialogButtonDelete}</a>
	        </div>
        </div>
    </div>
</script>

<script id="categoriesSubscriptionsDialogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_subscription_dlg">
	        <div class="header">
		        <div class="inner">
			        <span>${dialogHeader}</span>
			        <div class="close-btn">&times;</div>
		        </div>
	        </div>
	        <div class="title">
		        <div class="bullet"></div>
		        <input type="text" value="" maxlength="${maxlength}"/>
	        </div>
	        <div class="color">
		        <span class="label">${dialogBgColorLabel}</span>
		        <span class="outer"><span class="inner">&nbsp;</span></span>
		        <span class="label-for-text">${dialogFontColorLabel}</span>
		        <span class="outer"><span class="inner-for-text">&nbsp;</span></span>
	        </div>
	        <div class="row">
		        <div class="alert">
			        <div class="label">${dialogAlertLabel}</div>
			        <select>
                        {{each(index, alertOption) alertOptions}}
                            <option value="${alertOption.value}">${alertOption.text}</option>    
                        {{/each}}
			        </select>
		        </div>
		        <div class="timezone">
			        <div class="label">${dialogTimezoneLabel}</div>
			        <select></select>
		        </div>
		        <div class="timezone-read-only">
			        <div class="label">${dialogTimezoneLabel}</div>
			        <span class="timezone-desc" />
		        </div>
	        </div>
	        <div class="owner">
		        <div class="label">${dialogOwnerLabel}</div>
		        <div>
                    <span class="icon">&nbsp;</span>
                    <span class="name"></span>
		        </div>
	        </div>

	        <div class="shared-list"/>
			<div class="ical-export">
				<span class="ical-link">${dialogExportLink}</span>
			</div>

	        <div class="buttons">
		        <a class="button blue middle save-btn" href="#">${dialogButtonSave}</a>
		        <a class="button gray middle cancel-btn" href="#">${dialogButtonCancel}</a>
		        <a class="button gray middle delete-btn" href="#">${dialogButtonDelete}</a>
		        <a class="button gray middle unsubs-btn" href="#">${dialogButtonUnsubscribe}</a>
	        </div>
        </div>
    </div>
</script>

<script id="categoriesSubscriptionsManageDialogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_subscr_editor">
	        <div class="header">
		        <div class="inner">
			        <span class="title">${dialogTitle}</span>
			        <div class="close-btn">&times;</div>
		        </div>
	        </div>
	        <div class="qsearch">
		        <input type="text" value="${dialogSearchText}" maxlength="${maxlength}"/>
		        <div class="clean-btn">&nbsp;</div>
	        </div>
	        <div class="groups"/>
	        <div class="buttons">
		        <a id="fc_subscr_save" class="button blue middle" href="#">${dialogButtonSave}</a>
                <span class="splitter">&nbsp;</span>
		        <a id="fc_subscr_cancel" class="button gray middle" href="#">${dialogButtonCancel}</a>
	        </div>
        </div>
    </div>
</script>

<script id="eventEditorDialogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_event_editor" class="event-editor">
	        <div class="start-point"></div>
            
            <br/>
	        <div class="header">
		        <div class="inner">
			        <span>${dialogHeaderAdd}</span>
			        <div class="close-btn">&times;</div>
		        </div>
	        </div>
	        <div class="viewer">
                <div class="title big"></div>
                <div class="info-text">
                    <%=Resources.CalendarJSResource.cancelledEventInfoText%>
                </div>
                <div class="location"></div>
                <div class="date-time">
			        <div>
				        <span class="from-date"></span>
                        <span class="from-time"></span>
			        </div>
			        <div class="right">
				        <span class="to-date"></span>
                        <span class="to-time"></span>
			        </div>
		        </div>
                <div class="all-day">
			        <span class="no-label">${dialogAllDayNoText}</span>
			        <span class="yes-label">${dialogAllDayYesText}</span>
		        </div>
                <div class="owner">
                    <span class="icon">&nbsp;</span>
                    <span class="name"></span>
		        </div>
                <div class="attendees">
                    <%=Resources.CalendarJSResource.GuestsCount%> <span class="guests-count"></span>&nbsp;
                    <%=Resources.CalendarJSResource.AcceptedCount%> <span class="accepted-count"></span>&nbsp;
                    <%=Resources.CalendarJSResource.DeclinedCount%> <span class="declined-count"></span>
                </div>
                <div class="reply-buttons">
                    <%=Resources.CalendarJSResource.replyLabelText%>
                    <span class="reply-button accepted"><%=Resources.CalendarJSResource.replyAnswerYes%></span>&nbsp;
                    <span class="reply-button tentative"><%=Resources.CalendarJSResource.replyAnswerMaybe%></span>&nbsp;
                    <span class="reply-button declined"><%=Resources.CalendarJSResource.replyAnswerNo%></span>
                </div>
		        

		        <div class="repeat-alert" style="display: none !important;">
			        <div>
				        <div class="label">${dialogAlertLabel}</div>
				        <span class="alert"></span>
			        </div>
			        <div class="right">
				        <div class="label">${dialogRepeatLabel}</div>
				        <span class="repeat"></span>
			        </div>
		        </div>

		        <div class="calendar-status">
			        <div class="calendar" style="width: 100%; padding: 0;">
                        <div class="label">${dialogCalendarLabel}</div>
			            <div style="position: relative">
                            <span class="bullet"></span>
                            <span class="name"></span>
			            </div>
                    </div>
			        <div class="right" style="display: none !important;">
				        <div class="label">${dialogStatusLabel}</div>
				        <span class="status"></span>
			        </div>
		        </div>
		        <div class="description" style="display: none !important;">
			        <div class="label">${dialogDescriptionLabel}</div>
			        <div class="text"></div>
		        </div>
	        </div>
	        <div class="editor">
                <div class="title">
			        <div class="label">${dialogTodoNameLabel}</div>
			        <input type="text" value="${defaultEventSummary}" maxlength="${maxlength}"/>
		        </div>
                <div class="buttonGroup">
                    <span class="active event">${eventButton}</span>
                    <span class="todo">${todoButton}</span>
                </div>

                <div class="location">
			        <div class="label">${dialogLocationLabel}</div>
			        <input type="text" value="" maxlength="${maxlength}"/>
		        </div>
                <div class="date-time">
			        <div>
				        <div class="label">${dialogFromLabel}</div>
				        <div class="wrapper">
					        <input class="from-date" type="text" value=""/>
                            <div class="from cal-icon"></div>
					        <input class="from-time" type="text" value=""/>
				        </div>
			        </div>
			        <div class="right">
				        <div class="label">${dialogToLabel}</div>
				        <div class="wrapper">
					        <input class="to-date" type="text" value=""/>
                            <div class="to cal-icon"></div>
					        <input class="to-time" type="text" value=""/>
				        </div>
			        </div>
		        </div>
                <div class="all-day">
			        <input type="checkbox" class="cb"/>
                    <span class="label">${dialogAllDayLabel}</span>
		        </div>

                <div class="attendees" style="display: none !important;">
			        <div class="label">${dialogAttendeesLabel}</div>
			        <div id="emailSelector" class="emailselector">
                        <input type="text" class="emailselector-input" autocomplete="off">
                        <pre class="emailSelector-input-buffer"></pre>
                    </div>
		        </div>
		        <div class="repeat-alert" style="display: none !important;">
			        <div>
				        <span class="label">${dialogAlertLabel}</span>&nbsp;
                        <br/>
				        <span class="fc-view-alert">
					        <span class="fc-selector-link"></span>
					        <span class="fc-dropdown">&nbsp;</span>
				        </span>
			        </div>
			        <div class="right">
				        <span class="label">${dialogRepeatLabel}</span>&nbsp;
                        <br/>
				        <span class="fc-view-repeat">
					        <span class="fc-selector-link"></span>
					        <span class="fc-dropdown">&nbsp;</span>
				        </span>
			        </div>
		        </div>

		        <div class="calendar-status">
			        <div class="calendar" style="width: 46%; padding: 0;">
                        <div class="label">${dialogCalendarLabel}</div>
			            <div class="wrapper" style="position: relative">
				            <div class="bullet"></div>
				            <select></select>
			            </div>
                    </div>
			        <div class="right" style="display: none !important;">
				        <div class="label">${dialogStatusLabel}</div>
				        <div class="wrapper">
					        <select class="status">
					            <option value="0">${dialogStatusOptionTentative}</option>
					            <option value="1">${dialogStatusOptionConfirmed}</option>
                                <option value="2">${dialogStatusOptionCancelled}</option>
				            </select>
				        </div>
			        </div>
		        </div>
		        <div class="description" style="display: none !important;">
			        <div class="label">${dialogDescriptionLabel}</div>
			        <textarea cols="3" rows="3"></textarea>
		        </div>
                
                <div class="todo_editor">
                    <div class="date-time">
			            <div>
				            <div class="label">${dialogTodoDate}</div>
				            <div class="wrapper">
					            <input class="date" type="text" value=""/>
                                <div class="cal-icon"></div>
                                <input class="time textEdit time" style="display: none!important" type="text" value=""/>
	        </div>
			            </div>
		            </div>
                    <div class="description">
			            <div class="label">${dialogDescriptionLabel}</div>
			            <textarea cols="3" rows="3"/>
		            </div>
                </div>
	        </div>
	
	        <!-- Repeat settings block -->
	        <div class="repeat-settings" style="display: none !important;">
		
		        <!-- Start date -->
		        <div class="fc-start-date">
			        <div class="date-time">
				        <div>
					        <div>${repeatFromLabel}</div>
					        <div class="wrapper">
						        <input class="from-date" type="text" value=""/>
                                <div class="from cal-icon"></div>
						        <input class="from-time hidden" type="text" value=""/>
					        </div>
				        </div>
			        </div>
		        </div>
		
		        <!-- Day/week/month selector -->
		        <div>
			        <span>${repeatRepeatOnLabel}</span>&nbsp;
			        <span class="fc-dwm-selector">
				        <span class="fc-selector-link">${repeatRepeatOnDays}</span>
				        <span class="fc-dropdown">&nbsp;</span>
			        </span>
		        </div>
		
		        <!-- Interval selector -->
		        <div>
			        <span>${repeatEachLabel}</span>&nbsp;
			        <select class="fc-interval-selector"></select>&nbsp;
			        <span class="fc-interval-label">${repeatIntervalOptionDay}</span>
		        </div>
		
		        <!-- Days of week -->
		        <div class="fc-days-week"></div>
		
		        <!-- Radio selector -->
		        <div class="fc-month-radio"></div>
		
		        <!-- End of repeat -->
		        <div>
			        <span>${repeatToLabel}</span>&nbsp;
			        <span class="fc-endrepeat-selector">
				        <span class="fc-selector-link">${repeatOptionNever}</span>
				        <span class="fc-dropdown">&nbsp;</span>
			        </span>
		        </div>
		
		        <!-- Count of cycles -->
		        <div class="fc-repeat-cycles">
			        <span>${repeatAfterLabel}</span>&nbsp;
			        <input class="fc-cycle-times" type="text" value="1">&nbsp;
			        <span>${repeatTimesLabel}</span>
		        </div>
		
		        <!-- End date -->
		        <div class="fc-end-date">
			        <div class="date-time">
				        <div>
					        <div class="wrapper">
						        <input class="to-date" type="text" value=""/>
                                <div class="to cal-icon"></div>
						        <input class="to-time hidden" type="text" value=""/>
					        </div>
				        </div>
			        </div>
		        </div>
	        </div>
	        <div class="buttons clearFix">
		        <a class="edit-btn button blue middle">${dialogButtonEdit}</a>
		        <a class="save-btn button blue middle">${dialogButtonSave}</a>
		        <a class="close-btn button blue middle">${dialogButtonMoreDetails}</a>
		        <a class="delete-btn button gray middle">${dialogButtonDelete}</a>
		        <a class="unsubs-btn button gray middle">${dialogButtonUnsubscribe}</a>
                <span class="view-details">${dialogButtonDetails}</span>
	        </div>
	        <div class="end-point"></div>
        </div>
    </div>
</script>

<script id="delete_todo_icon" type="text/x-jquery-tmpl">
    <span class="menu-item-icon userforum"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/documents-icons.svg#documentsIconstrash"></use></svg></span>
</script>
<script id="edit_todo_icon" type="text/x-jquery-tmpl">
    <span class="menu-item-icon userforum"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/community-icons.svg#communityIconsblogs"></use></svg></span>
</script>
<script id="settings_todo_icon" type="text/x-jquery-tmpl">
    <span class="menu-item-icon userforum"><svg class="menu-item-svg"><use base="<%= WebPath.GetPath("/")%>" href="/skins/default/images/svg/top-studio-menu.svg#svgTopStudioMenusettings"></use></svg></span>
</script>

<script id="todoViewDialogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_todo_viewer">
            <div class="viewer">
                <div class="title big"></div>   
                <div class="date-time">
	                <div>
		                <span class="date"></span>
	                </div>
                </div>
                <div class="description">
	                <div class="text"></div>
                </div>
                <div class="buttons">
		            <a class="mark-btn button blue middle" href="#">${dialogButtonMarkOn}</a>
		            <a class="edit-btn button gray middle" href="#">${dialogButtonEdit}</a>
		            <a class="delete-btn button gray middle" href="#">${dialogButtonDelete}</a>
	            </div>
            </div>
        </div>
    </div>
</script>
<script id="todoEditorDialogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_todo_editor">
            <div class="header">
		        <div class="inner">
			        <span class="title">${dialogTitle}</span>
			        <div class="close-btn">&times;</div>
		        </div>
	        </div>
            <div class="title">
                <div class="label">${dialogNameLabel}</div>
		        <input id="fc_todo_title" type="text" value="" maxlength="150"/>
	        </div>
            <div class="date-time">
			    <div>
				    <div class="label">${dialogDate}</div>
				    <div class="wrapper">
					    <input class="date" id="fc_todo_start_date" type="text" value=""/>
                        <div class="cal-icon"></div>
				    </div>
			    </div>
		    </div>
            <div class="description">
			    <div class="label">${dialogDescriptionLabel}</div>
			    <textarea id="fc_todo_description" cols="3" rows="3"/>
		    </div>
            <div class="buttons">
		        <a class="save-btn button blue middle" id="fc_todo_ok" href="#">${dialogButtonSave}</a>
		        <a class="cancel-btn button gray middle" id="fc_todo_cancel" href="#">${dialogButtonCancel}</a>
	        </div>
        </div>
    </div>
</script>
<script id="deleteTodoDalogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_delete_todo">
	        <div class="header">
		        <div class="inner">
                    <span class="single-content">${dialogHeader}</span>
			        <div class="close-btn">&times;</div>
		        </div>
	        </div>
            <div class="delete-text single-content">${dialogSingleBody}</div>
	        <div class="buttons">
		        <a class="save-btn button blue middle" href="#">${dialogButton_apply}</a>
		        <a class="cancel-btn button gray middle" href="#">${dialogButton_cancel}</a>
	        </div>
        </div>
    </div>
</script>
<script id="todoMenuTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_todo_menu">
            <div class="todo-in-calendar">
                <label><input type="checkbox" id="todo-in-cal-check" class="bullet"/><span></span></label>
                <label class="label" for="todo-in-cal-check">${menuTodoInCalendar}</label>
            </div>
            <div class="delete-marked-todo">
                <label><input type="checkbox" id="del-mark-td-check" class="bullet"/><span></span></label>
                <label class="label" for="del-mark-td-check">${menuDeleteMarkedTodo}</label>
            </div>
            <div class="sync-links">
                <label class="label" id="sync-lnk">${menuSyncLinks}</label>
            </div>
        </div>
    </div>
</script>
<script id="deleteSettingsDalogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_delete_settings">
	        <div class="header">
		        <div class="inner">
			        <span class="repeat-content">${dialogHeader}</span>
                    <span class="single-content">${dialogSingleHeader}</span>
			        <div class="close-btn">&times;</div>
		        </div>
	        </div>
	        <div class="delete-selector repeat-content">
		        <input class="delete-this" type="radio" name="delete-radio" value="" checked>&nbsp;
                <span class="delete-this-label">${dialogDeleteOnlyThisLabel}</span>
                <br/>
		        <input class="delete-following" type="radio" name="delete-radio" value="">&nbsp;
                <span class="delete-following-label">${dialogDeleteFollowingLabel}</span>
                <br/>
		        <input class="delete-all" type="radio" name="delete-radio" value="">&nbsp;
                <span class="delete-all-label">${dialogDeleteAllLabel}</span>
	        </div>
            <div class="delete-text single-content">${dialogSingleBody}</div>
	        <div class="buttons">
		        <a class="save-btn button blue middle" href="#">${dialogButtonSave}</a>
		        <a class="cancel-btn button gray middle" href="#">${dialogButtonCancel}</a>
	        </div>
        </div>
    </div>
</script>

<script id="icalStreamDialogTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="fc_ical_stream">
	        <div class="header">
		        <div class="inner">
			        <span>${dialogHeader}</span>
			        <div class="close-btn">&times;</div>
		        </div>
	        </div>
	        <div class="ical-description">
		        <span>${dialogDescription}</span>
	        </div>
	        <div class="saved-url-link"></div>
            <div class="url-link caldav" style="display: none">
                <div class="title">${dialogExportCalDav}</div>
                <span class="control">
                    <div class="button copy">
                        <span>${dialogCopyButton}</span>
                    </div>
                    <div class="button try-again">
                        <span>${dialogTryAgainButton}</span>
                    </div>
                    <span class="textinput__box"></span>
                    <input type='text' readonly/>
                </span>
            </div>
             <% if (!string.IsNullOrEmpty(ASC.Web.Studio.Utility.CommonLinkUtility.GetHelpLink()))
                   { %>
                    <div class="caldav-help" style="display: none">
                        <span>${dialogCaldavHelp} </span>
                        <a href="<%= ASC.Web.Studio.Utility.CommonLinkUtility.GetHelpLink() + "/tipstricks/export-calendars-to-devices.aspx" %>" target="_blank">${dialogHelpCenter}</a>
                    </div>
                <% } %>
            <div class="url-link ical" style="display: none">
                <div class="title">${dialogExportIcal}</div>
                <span class="control">
                    <div class="button copy">
                        <span>${dialogCopyButton}</span>
                    </div>
                    <span class="textinput__box"></span>
                    <input type='text' readonly/>
                </span>
            </div>
	        <div class="buttons">
		        <a class="cancel-btn button gray middle" href="#">${dialogButtonClose}</a>
	        </div>
        </div>
    </div>
</script>




<script id="eventPageTemplate" type="text/x-jquery-tmpl">
    
    <div class="event-editor">

        <div>
            <div class="clearFix">
                <div class="halfwidth left">
                    <div class="inner">
                        <h1 class="event-header">
                            <a class="header-back-link"></a>
                            <span></span>
                        </h1>
                        <div class="info-text"><%=Resources.CalendarJSResource.cancelledEventInfoText%></div>
                    </div>
                </div>
                <div class="halfwidth right">
                    <div class="inner">
                        <div class="reply-buttons">
			                <span class="label"><%=Resources.CalendarJSResource.replyLabelText%></span>
                            <ul>
                                <li class="reply-radio-container">
                                    <label>
                                        <input class="reply-radio accepted" type="radio" name="replyradio" data-value="ACCEPTED"/>
                                        <%=Resources.CalendarJSResource.replyAnswerYes%>
                                    </label>
                                </li>
                                <li class="reply-radio-container">
                                    <label>
                                        <input class="reply-radio tentative" type="radio" name="replyradio" data-value="TENTATIVE"/>
                                        <%=Resources.CalendarJSResource.replyAnswerMaybe%>
                                    </label>
                                </li>
                                <li class="reply-radio-container">
                                    <label>
                                        <input class="reply-radio declined" type="radio" name="replyradio" data-value="DECLINED"/>
                                        <%=Resources.CalendarJSResource.replyAnswerNo%>
                                    </label>
                                </li>
                            </ul>
		                </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="viewer botttom-indent">
            <div class="clearFix">
                <div class="halfwidth left">
                    <div class="inner">
                        <div class="botttom-indent title">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_summaryLabel%></div>
			                <div class="text"></div>
		                </div>

                        <div class="botttom-indent location">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_locationLabel%></div>
			                <div class="text"></div>
		                </div>
                
                        <div class="botttom-indent date-time from-to">
			                <div class="inline-block">
				                <div class="wrapper">
					                <span class="from-date"></span>
                                    <span class="from-time"></span>
				                </div>
			                </div>
                            <div class="inline-block">
				                <div class="wrapper">
					                <span class="to-date"></span>
                                    <span class="to-time"></span>
				                </div>
			                </div>
                            <div class="inline-block cbx-container all-day">
                                <span class="no-label"><%=Resources.CalendarJSResource.calendarEventEditor_notAllDayEvent%></span>
			                    <span class="yes-label"><%=Resources.CalendarJSResource.calendarEventEditor_allDayEvent%></span>
			                </div>
		                </div>
                
                        <div class="botttom-indent clearFix calendar">
                            <div class="halfwidth">
                                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_calendarLabel%></div>
                                <div class="wrapper">
				                    <span class="bullet"></span>
                                    <span class="name"></span>
			                    </div>
                            </div>
		                </div>

                        <div class="botttom-indent owner">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_organizerLabel%></div>
                            <div class="name"></div>
		                </div>

                        <div class="botttom-indent attendees">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_attendeesLabel%></div>
                            <div class="users-list attendees-user-list"></div>
		                </div>

		                <div class="botttom-indent clearFix repeat-alert">
			                <div class="halfwidth">
				                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_repeatLabel%></div>
                                <span class="repeat"></span>
			                </div>
                            <div class="halfwidth">
                                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_alertLabel%></div>
                                <span class="alert"></span>
			                </div>
		                </div>
                    </div>
                </div>
                <div class="halfwidth right">
                    <div class="inner">
	                    <div class="botttom-indent description">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_descriptionLabel%></div>
			                <div class="text"></div>
		                </div>

                        <div class="botttom-indent">
                            <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_sharedUsersLabel%></div>
                            <div class="users-list shared-user-list"></div>
                        </div>
                    </div>
                </div>
            </div>
	    </div>

	    <div class="editor botttom-indent">
            <div class="clearFix">
                <div class="halfwidth left">
                    <div class="inner">
                        <div class="botttom-indent title">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_summaryLabel%></div>
			                <input type="text" class="textEdit fullwidth" value="" maxlength="${maxlength}"/>
		                </div>

                        <div class="botttom-indent date-time from-to">
			                <div class="inline-block">
				                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_timeLabel%></div>
				                <div class="wrapper">
					                <input class="from-date textEdit date" type="text" value=""/>
                                    <div class="from cal-icon"></div>
					                <input class="from-time textEdit time" type="text" value=""/>
				                </div>
			                </div>
                            <div class="inline-block">
				               <!-- <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_toLabel%></div> -->
				                <div class="wrapper">
					                <input class="to-time textEdit time" type="text" value=""/>
					                <input class="to-date textEdit date" type="text" value=""/>
                                    <div class="to cal-icon"></div>
				                </div>
			                </div>
                            
		                </div>
                        <div class="botttom-indent clearFix repeat-alert">
                            <div class="inline-block cbx-container all-day">
                                <div>
                                    <label>
                                        <input class="allday cb" type="checkbox"/>
                                        <%=Resources.CalendarJSResource.calendarEventEditor_allDayLabel%>
                                    </label>
                                </div>
			                </div>
			                <div class="inline-block">
                                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_repeatLabel%></div>
                                <span class="fc-view-repeat">
					                <span class="fc-selector-link"></span>
					                <span class="fc-dropdown">&nbsp;</span>
				                </span>
			                </div>
                            <div class="inline-block">
				                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_alertLabel%></div>
                                <span class="fc-view-alert">
					                <span class="fc-selector-link"></span>
					                <span class="fc-dropdown">&nbsp;</span>
				                </span>
			                </div>
		                </div>
                        
                        <table class="repeat-settings hidden">
                            <tr class="fc-start-date">
                                <td><%=Resources.CalendarJSResource.repeatSetting_fromLabel%></td>
                                <td class="date-time">
                                    <div class="wrapper">
						                <input class="from-date textEdit date" type="text" value=""/>
                                        <div class="from cal-icon"></div>
						                <input class="from-time textEdit time hidden" type="text" value=""/>
					                </div>
                                </td>
                            </tr>
                            <tr>
                                <td><%=Resources.CalendarJSResource.repeatSettings_onLabel%></td>
                                <td>
                                    <div class="fc-dwm-selector">
				                        <span class="fc-selector-link"></span>
				                        <span class="fc-dropdown">&nbsp;</span>
			                        </div>
                                </td>
                            </tr>
                            <tr>
                                <td><%=Resources.CalendarJSResource.repeatSettings_eachLabel%></td>
                                <td>
                                    <select class="fc-interval-selector"></select>&nbsp;
			                        <span class="fc-interval-label"><%=Resources.CalendarJSResource.repeatSettings_intervalDay%></span>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td class="fc-days-week"></td>
                            </tr>
		                    <tr>
                                <td></td>
                                <td class="fc-month-radio"></td>
                            </tr>
                            <tr>
                                <td><%=Resources.CalendarJSResource.repeatSettings_toLabel%></td>
                                <td>
                                    <div class="fc-endrepeat-selector">
				                        <span class="fc-selector-link"></span>
				                        <span class="fc-dropdown">&nbsp;</span>
			                        </div>
                                </td>
                            </tr>
                            <tr class="fc-repeat-cycles">
                                <td></td>
                                <td>
                                    <%=Resources.CalendarJSResource.repeatSettings_afterLabel%>&nbsp;
                                    <input class="fc-cycle-times" type="text" value="1">&nbsp;
			                        <%=Resources.CalendarJSResource.repeatSettings_timesLabel%>
                                </td>
                            </tr>
                            <tr class="fc-end-date">
                                <td></td>
                                <td class="date-time">
                                    <div class="wrapper">
						                <input class="to-date textEdit date" type="text" value=""/>
                                        <div class="to cal-icon"></div>
						                <input class="to-time textEdit time hidden" type="text" value=""/>
					                </div>
                                </td>
                            </tr>
	                    </table>
                        
                        <div class="botttom-indent clearFix calendar">
                            <div class="halfwidth">
                                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_calendarLabel%></div>
                                <div class="wrapper" style="position: relative">
				                    <div class="bullet"></div>
				                    <select></select>
                    </div>
                </div>
		                </div>

                        <div class="botttom-indent location">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_locationLabel%></div>
			                <input type="text" class="textEdit fullwidth" value="" maxlength="${maxlength}"/>
		                </div>

                        <div class="botttom-indent owner">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_organizerLabel%></div>
                            <div class="name"></div>
                            <div class="selector">
                                <select></select>
                            </div>
		                </div>
                        
                        <div class="botttom-indent description">
			                <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_descriptionLabel%></div>
			                <textarea class="textEdit fullwidth"></textarea>
		                </div>
                    </div>
                </div>
                <div class="halfwidth right">
                    <div class="inner">
                        <div class="botttom-indent attendees">
			                <div class="label">
                                <%=Resources.CalendarJSResource.calendarEventEditor_attendeesLabel%>
                                <span id="attendeesHelpSwitcher" class="HelpCenterSwitcher"></span>
                                <div id="attendeesHelpInfo" class="popup_helper">
                                    <%=CoreContext.Configuration.CustomMode ? ASC.Web.Studio.PublicResources.CustomModeResource.calendarEventEditor_attendeesLabelHelpInfoCustomMode : Resources.CalendarJSResource.calendarEventEditor_attendeesLabelHelpInfo%>
                                </div>
			                </div>
                            <div class="clearFix input-container">
                                <div class="btn-container">
                                    <a class="button gray"><%=Resources.CalendarJSResource.confirmPopup_ButtonOk%></a>
                                </div>
                                <div class="text-container">
                                    <input type="text" class="textEdit fullwidth" value=""/>
                                </div>
                            </div>
                            <div class="users-list attendees-user-list"></div>
                            <div class="attendees-noaccount">
                                <a href="<%= VirtualPathUtility.ToAbsolute("~/addons/mail/Default.aspx") %>">
                                    <%=Resources.CalendarJSResource.attendeesNoAccountLink%>
                                </a>
                                <%=Resources.CalendarJSResource.attendeesNoAccountText%>
                            </div>
                            <div class="inline-block cbx-container sent-invitations">
                                <div>
                                    <label>
                                        <input class="sent-invitations cb" type="checkbox"/>
                                        <%=Resources.CalendarJSResource.calendarEventEditor_sentInvitationsLabel%>
                                    </label>
                                </div>
                            </div>
                        </div>
                        <div class="botttom-indent">
                            <div class="label"><%=Resources.CalendarJSResource.calendarEventEditor_sharedUsersLabel%></div>
                            <div>
                                <span class="addUserLink">
                                    <a class="link dotline"><%=Resources.CalendarJSResource.calendarEventEditor_sharedUsersLink%></a>
                                    <span class="sort-down-black"></span>
                                </span>
                            </div>
                            <div class="users-list shared-user-list"></div>
                        </div>
                    </div>
                </div>
            </div>
	    </div>

	    <div class="buttons clearFix">
		    <a class="save-btn button blue big"><%=Resources.CalendarJSResource.calendarEventEditor_saveButton%></a>
		    <a class="close-btn button gray big"><%=Resources.CalendarJSResource.calendarEventEditor_closeButton%></a>
		    <a class="cancel-btn button gray big"><%=Resources.CalendarJSResource.calendarEventEditor_cancelButton%></a>
		    <a class="delete-btn button gray big"><%=Resources.CalendarJSResource.calendarEventEditor_deleteButton%></a>
		    <a class="unsubs-btn button gray big"><%=Resources.CalendarJSResource.calendarEventEditor_unsubscribeButton%></a>
	    </div>

    </div>
    
    <div class="fc-modal" style="display: none;"></div>

</script>




<script id="sharingUserTemplate" type="text/x-jquery-tmpl">
    {{each(i, item) items}}     
        <div id="sharing_event_item_${item.id}" class="sharingItem borderBase clearFix">        
            
        {{if item.isGroup}}
            <div class="name" title="${item.name}">
                ${item.name}
            </div>
        {{else}}
            <div class="name">
                <span class="userLink" title="${item.name}">${item.name}</span>   
            </div>             
        {{/if}}
            
            <div class="remove">
                {{if item.canEdit & !item.hideRemove}}
                    <img class="removeItem" data="${item.id}" border="0" align="absmiddle"
                        src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("trash_16.png")%>" alt="<%=Resources.Resource.DeleteButton%>"/>
                {{else}}
                    &nbsp;
                {{/if}}
            </div>
            
            <div class="action">
                {{if item.canEdit}}
                    <select data="${item.id}" id="select_${item.id}">
                        {{each(j, action) actions}}
                            {{if !action.disabled | action.id == item.selectedAction.id}}
                                <option value="${action.id}"
                                    {{if action.id == item.selectedAction.id}}selected="selected"{{/if}} 
                                    {{if action.disabled}}disabled="disabled"{{/if}} >${action.name}</option>
                            {{/if}}
                        {{/each}}
                    </select>
                {{else}}
                    ${item.selectedAction.name}
                {{/if}}
            </div>
        </div>
    {{/each}}
</script>

<script id="attendeeTemplate" type="text/x-jquery-tmpl">
    <div class="attendee-item sharingItem borderBase clearFix">
        <div class="status ${status.toLowerCase()}"></div>  
        <div class="name">
            <span class="userLink" title="${email}">{{if name }}${name}{{else}}${email}{{/if}}</span>   
        </div>         
        <div class="remove">
            {{if canEdit}}  
                <img class="removeItem" border="0" align="absmiddle" src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("trash_16.png")%>" alt="<%=Resources.Resource.DeleteButton%>"/>
            {{else}}
                &nbsp;
            {{/if}}
        </div>
        <div class="action">
            {{if canEdit}} 
            <select>
                <option value="REQ-PARTICIPANT"><%=Resources.CalendarJSResource.calendarEventEditor_requiredParticipant%></option>
                <option value="OPT-PARTICIPANT"><%=Resources.CalendarJSResource.calendarEventEditor_optionalParticipant%></option>
            </select>
            {{else}}
                {{if role == "REQ-PARTICIPANT"}}<%=Resources.CalendarJSResource.calendarEventEditor_requiredParticipant%>{{/if}}
                {{if role == "OPT-PARTICIPANT"}}<%=Resources.CalendarJSResource.calendarEventEditor_optionalParticipant%>{{/if}}
            {{/if}}
        </div>
    </div>
</script>

<script id="attendeeTemplateAutocomplete" type="text/x-jquery-tmpl">
    <li>
        <a>{{if name }}${name},&nbsp;{{/if}}{{if email }}${email}{{/if}}</a>
    </li>
</script>



<script id="attendeeConfirmNotificationTemplate" type="text/x-jquery-tmpl">
    <div>
        <div id="attendeeConfirmNotification">
	        <div class="header">
		        <div class="inner">
			        <span class="title">${dialogHeader}</span>
                    <div class="close-btn">&times;</div>
		        </div>
	        </div>
	        <div class="body">${dialogBody}</div>
	        <div class="buttons">
		        <a class="button blue middle send-btn">${dialogButtonSend}</a>
                <a class="button blue middle send-customs-btn">${dialogButtonSendCustoms}</a>
                <a class="button gray middle send-everyone-btn">${dialogButtonSendEveryone}</a>
		        <a class="button gray middle dont-send-btn">${dialogButtonDontSend}</a>
	        </div>
        </div>
    </div>
</script>
