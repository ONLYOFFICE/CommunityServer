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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASC.Api.Calendar.Resources;
using ASC.Web.Core.Calendars;
using System.IO;
using System.Net;
using ASC.Core;

namespace ASC.Api.Calendar.iCalParser
{
    public class iCalendar : BaseCalendar
    {
        public static iCalendar GetFromStream(TextReader reader)
        {
            var emitter = new iCalendarEmitter();
            var parser = new Parser(reader, emitter);
            parser.Parse();
            return emitter.GetCalendar();
        }

        public static iCalendar GetFromUrl(string url)
        {
            return GetFromUrl(url, null);
        }

        public static iCalendar GetFromUrl(string url, string calendarId)
        {
            var cache = new iCalendarCache();
            iCalendar calendar = null;
            if (calendarId != null)
                calendar = cache.GetCalendarFromCache(calendarId);

            if (calendar == null)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                WebResponse resp = req.GetResponse();

                var ms = new MemoryStream();
                resp.GetResponseStream().StreamCopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                using (var tempReader = new StreamReader(ms))
                {
                    var reader = new StringReader(tempReader.ReadToEnd());
                    calendar = GetFromStream(reader);
                    
                    if (calendar != null && calendarId != null)
                    {
                        tempReader.BaseStream.Seek(0, SeekOrigin.Begin);
                        cache.UpdateCalendarCache(calendarId, tempReader);
                    }                    
                }
            }

            if (calendar == null)
                throw new Exception(CalendarApiResource.WrongiCalFeedLink);

            return calendar;
        }


        public List<iCalEvent> Events { get; set; }

        public iCalendar()
        {
            this.Context.CanChangeAlertType = false;
            this.Context.CanChangeTimeZone = false;
            this.Context.GetGroupMethod = delegate() { return Resources.CalendarApiResource.iCalCalendarsGroup; };

            this.EventAlertType = EventAlertType.Never;
            this.Events = new List<iCalEvent>();            
        }

        public bool isEmptyName
        {
            get { return String.IsNullOrEmpty(_name);}
        }

        private string _name;
        public override string Name
        {
            get
            {
                if (String.IsNullOrEmpty(_name))
                    return Resources.CalendarApiResource.NoNameCalendar;

                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private TimeZoneInfo _timeZone;
        public override TimeZoneInfo TimeZone
        {
            get
            {
                if (_timeZone != null)
                    return _timeZone;

                if (!String.IsNullOrEmpty(xTimeZone))
                {
                    _timeZone = OlsenTimeZoneConverter.OlsonTZId2TimeZoneInfo(xTimeZone);
                    return _timeZone;
                }

                if (String.IsNullOrEmpty(TZID))
                {
                    _timeZone = CoreContext.TenantManager.GetCurrentTenant().TimeZone;
                    return _timeZone;
                }


                _timeZone = OlsenTimeZoneConverter.OlsonTZId2TimeZoneInfo(TZID);
                return _timeZone;
            }
            set
            {
                _timeZone = value;
            }
        }
        
        public string TZID { get; set; }

        public string xTimeZone { get; set; }
       
        public override List<IEvent> LoadEvents(Guid userId, DateTime utcStartDate, DateTime utcEndDate)
        {
            return Events.Cast<IEvent>().ToList();
        }       
    }
}
