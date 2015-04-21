/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Utils;
using ASC.Web.Core.Calendars;
using System.Globalization;

namespace ASC.Api.Calendar.iCalParser
{
    public class iCalendarEmitter : IEmitter
    {   
      

        private iCalendar _curCalendar;
        private iCalEvent _curEvent;

        private Stack<Token> _component= new Stack<Token>();
        private Token _curPropToken = null;

        public Parser VParser { get; set; }

        public iCalendar GetCalendar()
        {
            return _curCalendar;
        }

        public void doIntro(){}
        public void doOutro()
        {
        }
        public void doComponent() { }
      
        public void doResource(Token t) { }
        public void emit(string val) { }

        public void doEnd(Token t)
        {
            _curPropToken = null;
        }

        public void doResourceBegin(Token t)
        {
            _curPropToken = t;
        }

        public void doBegin(Token t)
        {            
        }

        public void doEndComponent() 
        {
            _component.Pop();
        }

        public void doComponentBegin(Token t)
        {  
            _component.Push(t);

            switch (t.TokenVal)
            {
                case TokenValue.Tvcalendar:                    
                    _curCalendar = new iCalendar();
                    break;

                case TokenValue.Tvevent:
                case TokenValue.Tvjournal:                    
                    _curEvent = new iCalEvent();
                    _curCalendar.Events.Add(_curEvent);
                    _curEvent.CalendarId = _curCalendar.Id;
                    break;
            }
        }       

        public void doID(Token t)
        {
            _curPropToken = t;            
        }

        public void doSymbolic(Token t)
        {           
        }

        public void doURIResource(Token t)
        {         
        }

        public void doMailto(Token t)
        {
        }

        public void doValueProperty(Token t, Token iprop)
        {
            var dateTime = DateTime.MinValue;
            bool isAllDay = true;
            bool isUTC = true;

            if (_curPropToken.TokenVal == TokenValue.Tdtstart 
                || _curPropToken.TokenVal == TokenValue.Tdtend
                || _curPropToken.TokenVal == TokenValue.Texdate)
            {

                if (iprop != null && iprop.TokenText.ToLower() == "date")
                    dateTime = Token.ParseDate(t.TokenText);

                else
                    dateTime = Token.ParseDateTime(t.TokenText, out isAllDay, out isUTC);
            }

            if (_component.Count > 0)
            {
                switch (_component.Peek().TokenVal)
                {
                    case TokenValue.Tvevent:

                        if (_curPropToken.TokenVal == TokenValue.Tdtstart)
                        {
                            _curEvent.AllDayLong = isAllDay;
                            _curEvent.OriginalStartDate = dateTime;

                            if (!isAllDay && !isUTC && _curCalendar.TimeZone != null)
                                _curEvent.UtcStartDate = dateTime.AddMinutes((-1) * (int)_curCalendar.TimeZone.BaseUtcOffset.TotalMinutes);
                            else
                                _curEvent.UtcStartDate = dateTime;

                        }

                        else if (_curPropToken.TokenVal == TokenValue.Tdtend)
                        {
                            _curEvent.OriginalEndDate = dateTime;
                            if (!isAllDay && !isUTC && _curCalendar.TimeZone != null)
                                _curEvent.UtcEndDate = dateTime.AddMinutes((-1) * (int)_curCalendar.TimeZone.BaseUtcOffset.TotalMinutes);
                            else if (isAllDay)
                                _curEvent.UtcEndDate = dateTime.AddDays(-1);
                            else
                                _curEvent.UtcEndDate = dateTime;
                        }
                        else if (_curPropToken.TokenVal == TokenValue.Texdate)
                        {
                            _curEvent.RecurrenceRule.ExDates.Add(new RecurrenceRule.ExDate() { Date = dateTime, isDateTime = !isAllDay });
                        }

                        break;
                }
            }
        }

        public void doIprop(Token t, Token iprop)
        {         
        }

        public void doRest(Token t, Token id)
        {
            _curPropToken = null;

            if (_component.Count <= 0)
                return;

            switch (_component.Peek().TokenVal)
            {
                case TokenValue.Tvcalendar:
                case TokenValue.Tvtimezone:
                    switch (id.TokenText.ToLower())
                    {
                        case "tzid":
                            _curCalendar.TZID = t.TokenText;
                            break;

                        case "x:wrtimezone":
                            _curCalendar.xTimeZone = t.TokenText;
                            break;
        
                        case "x:wrcalname":
                            _curCalendar.Name = t.TokenText;
                            break;

                        case "x:wrcaldesc":
                            _curCalendar.Description = t.TokenText;
                            break;
                    }
                    break;

                case TokenValue.Tvevent:
                    switch (id.TokenText.ToLower())
                    {
                        case "description":
                            _curEvent.Description = t.TokenText;
                            break;

                        case "summary":
                            _curEvent.Name = t.TokenText;
                            break;

                        case "uid":
                            _curEvent.Id = t.TokenText;
                            break;
                    }
                    break;
            }
        }

        public void doAttribute(Token key, Token val)
        {
            if (_component.Count <= 0)
                return;

            //event timezone
            if ((_curPropToken.TokenVal == TokenValue.Tdtstart || _curPropToken.TokenVal == TokenValue.Tdtend) && _component.Peek().TokenVal == TokenValue.Tvevent)
            {
                switch (key.TokenText.ToLower())
                {
                    case "tzid":

                        var tz = TimeZoneConverter.GetTimeZone(val.TokenText);
                        if (_curPropToken.TokenVal == TokenValue.Tdtstart)
                            _curEvent.UtcStartDate = _curEvent.OriginalStartDate.AddMinutes((-1) * (int)tz.BaseUtcOffset.TotalMinutes);
                        
                        else if (_curPropToken.TokenVal == TokenValue.Tdtend)
                            _curEvent.UtcEndDate = _curEvent.OriginalEndDate.AddMinutes((-1) * (int)tz.BaseUtcOffset.TotalMinutes);

                        break;
                }
            }

            //event rrule
            if (_curPropToken.TokenVal == TokenValue.Trrule && _component.Peek().TokenVal == TokenValue.Tvevent)
            {
                switch(key.TokenText.ToLower())
                {
                    case "freq":
                        _curEvent.RecurrenceRule.Freq = RecurrenceRule.ParseFrequency(val.TokenText);
                        break;

                    case "until":
                        bool isDate, isUTC;
                        _curEvent.RecurrenceRule.Until = Token.ParseDateTime(val.TokenText, out isDate, out isUTC);
                        break;

                    case "count":
                        _curEvent.RecurrenceRule.Count = Convert.ToInt32(val.TokenText);
                        break;

                    case "interval":
                        _curEvent.RecurrenceRule.Interval = Convert.ToInt32(val.TokenText);
                        break;

                    case "bysecond":
                        _curEvent.RecurrenceRule.BySecond = val.TokenText.Split(',').Select(v => Convert.ToInt32(v)).ToArray();
                        break;

                    case "byminute":
                        _curEvent.RecurrenceRule.ByMinute= val.TokenText.Split(',').Select(v => Convert.ToInt32(v)).ToArray();
                        break;

                    case "byhour":
                        _curEvent.RecurrenceRule.ByHour = val.TokenText.Split(',').Select(v => Convert.ToInt32(v)).ToArray();
                        break;

                    case "byday":
                        _curEvent.RecurrenceRule.ByDay = val.TokenText.Split(',').Select(v => RecurrenceRule.WeekDay.Parse(v)).ToArray();
                        break;

                    case "bymonthday":
                        _curEvent.RecurrenceRule.ByMonthDay = val.TokenText.Split(',').Select(v => Convert.ToInt32(v)).ToArray();
                        break;

                    case "byyearday":
                        _curEvent.RecurrenceRule.ByYearDay = val.TokenText.Split(',').Select(v => Convert.ToInt32(v)).ToArray();
                        break;

                    case "byweekno":
                        _curEvent.RecurrenceRule.ByWeekNo = val.TokenText.Split(',').Select(v => Convert.ToInt32(v)).ToArray();
                        break;

                    case "bymonth":
                        _curEvent.RecurrenceRule.ByMonth = val.TokenText.Split(',').Select(v => Convert.ToInt32(v)).ToArray();
                        break;

                    case "bysetpos":
                        _curEvent.RecurrenceRule.BySetPos= val.TokenText.Split(',').Select(v => Convert.ToInt32(v)).ToArray();
                        break;

                    case "wkst":
                        _curEvent.RecurrenceRule.WKST = RecurrenceRule.WeekDay.Parse(val.TokenText);
                        break;
                }
            }
        }
    }    
}
