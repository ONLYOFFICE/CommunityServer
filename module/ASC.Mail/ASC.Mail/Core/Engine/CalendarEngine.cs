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


using System;
using System.IO;
using System.Linq;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Utils;

namespace ASC.Mail.Core.Engine
{
    public class CalendarEngine
    {
        public ILog Log { get; private set; }

        public CalendarEngine(ILog log = null)
        {
            Log = log ?? LogManager.GetLogger("ASC.Mail.CalendarEngine");
        }

        public void UploadIcsToCalendar(MailBoxData mailBoxData, int calendarId, string calendarEventUid, string calendarIcs,
            string calendarCharset, string calendarContentType, string calendarEventReceiveEmail, string httpContextScheme)
        {
            try
            {
                if (string.IsNullOrEmpty(calendarEventUid) ||
                    string.IsNullOrEmpty(calendarIcs) ||
                    calendarContentType != "text/calendar")
                    return;

                var calendar = MailUtil.ParseValidCalendar(calendarIcs, Log);

                if (calendar == null)
                    return;

                var alienEvent = true;

                var organizer = calendar.Events[0].Organizer;

                if (organizer != null)
                {
                    var orgEmail = calendar.Events[0].Organizer.Value.ToString()
                        .ToLowerInvariant()
                        .Replace("mailto:", "");

                    if (orgEmail.Equals(calendarEventReceiveEmail))
                        alienEvent = false;
                }
                else
                {
                    throw new ArgumentException("calendarIcs.organizer is null");
                }

                if (alienEvent)
                {
                    if (calendar.Events[0].Attendees.Any(
                        a =>
                            a.Value.ToString()
                                .ToLowerInvariant()
                                .Replace("mailto:", "")
                                .Equals(calendarEventReceiveEmail)))
                    {
                        alienEvent = false;
                    }
                }

                if (alienEvent)
                    return;

                CoreContext.TenantManager.SetCurrentTenant(mailBoxData.TenantId);
                SecurityContext.AuthenticateMe(new Guid(mailBoxData.UserId));

                using (var ms = new MemoryStream(EncodingTools.GetEncodingByCodepageName(calendarCharset).GetBytes(calendarIcs)))
                {
                    var apiHelper = new ApiHelper(httpContextScheme, Log);
                    apiHelper.UploadIcsToCalendar(calendarId, ms, "calendar.ics", calendarContentType);
                }

                Log.Info("CalendarEngine->UploadIcsToCalendar() has been succeeded");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("CalendarEngine->UploadIcsToCalendar with \r\n" +
                          "calendarId: {0}\r\n" +
                          "calendarEventUid: '{1}'\r\n" +
                          "calendarIcs: '{2}'\r\n" +
                          "calendarCharset: '{3}'\r\n" +
                          "calendarContentType: '{4}'\r\n" +
                          "calendarEventReceiveEmail: '{5}'\r\n" +
                          "Exception:\r\n{6}\r\n",
                    calendarId, calendarEventUid, calendarIcs, calendarCharset, calendarContentType,
                    calendarEventReceiveEmail, ex.ToString());
            }
        }
    }
}