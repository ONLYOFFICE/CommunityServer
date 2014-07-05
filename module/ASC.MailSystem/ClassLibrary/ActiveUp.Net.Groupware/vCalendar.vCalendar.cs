// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.IO;
using System;
namespace ActiveUp.Net.Groupware.vCalendar
{
    #if !PocketPC
    [System.Serializable]
    #endif
    public class vCalendar
    {
        public vCalendar()
        {
        
        }
        private string _version,_prodId,_timeZone;
        private GeographicalPosition _geo = new GeographicalPosition();
        private DayLightSavings _daylight = new DayLightSavings();
        private vEventCollection _events;
        private vTodoCollection _todos;
        
        public string Version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }
        public GeographicalPosition GeographicalPosition
        {
            get
            {
                return this._geo;
            }
            set
            {
                this._geo = value;
            }
        }
        public DayLightSavings DayLightSavings
        {
            get
            {
                return this._daylight;
            }
            set
            {
                this._daylight = value;
            }
        }
        public string GeneratorId
        {
            get
            {
                return this._prodId;
            }
            set
            {
                this._prodId = value;
            }
        }
        public string TimeZone
        {
            get
            {
                return this._timeZone;
            }
            set
            {
                this._timeZone = value;
            }
        }
        public vEventCollection Events
        {
            get
            {
                if (this._events == null)
                    this._events = new vEventCollection();
                return this._events;
            }
            set
            {
                this._events = value;
            }
        }
        public vTodoCollection Todos
        {
            get
            {
                return this._todos;
            }
            set
            {
                this._todos = value;
            }
        }

        public static vCalendar LoadFromFile(string filename)
        {
            StreamReader streamReader = new StreamReader(filename);
            string content = streamReader.ReadToEnd();
            streamReader.Close();
            return Parser.Parse(content);
        }

        public void SaveToFile(string filename)
        {
            StreamWriter streamWriter = new StreamWriter(filename);
            streamWriter.Write(this.GetData());
            streamWriter.Close();
        }

        private string GetUniversalDate(DateTime date)
        {
            return string.Format("{0}{1}{2}T183000Z", date.Year, date.Month.ToString().PadRight(2, '0'), date.Day.ToString().PadRight(2, '0'));
        }
        public string GetData()
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            sw.WriteLine("BEGIN:VCALENDAR");
            sw.WriteLine("VERSION:2.0");
            foreach (vEvent ev in this.Events)
            {
                sw.WriteLine("BEGIN:VEVENT");

                foreach (Attendee attendee in ev.Attendees)
                {
                    sw.WriteLine(string.Format("ATTENDEE;CN={0};ROLE={1};RSVP={2};MAILTO:{3}", attendee.Contact.Name, attendee.Role, "True", attendee.Contact.Email));
                }

                sw.Write("DTSTART:"); sw.WriteLine(GetUniversalDate(ev.Start));
                sw.Write("DTEND:"); sw.WriteLine(GetUniversalDate(ev.End));

                sw.Write("LOCATION:"); sw.WriteLine(ev.Location);

                //sw.WriteLine("TRANSP:OPAQUE");
                //sw.WriteLine("SEQUENCE");
                sw.Write("UID:"); sw.WriteLine(ev.Uid);
                
                //sw.WriteLine("DTSTAMP");

                sw.Write("DESCRIPTION:"); sw.WriteLine(ev.Description);

                sw.Write("SUMMARY:"); sw.WriteLine(ev.Summary);

                sw.Write("PRIORITY:"); sw.WriteLine(ev.Priority.ToString());

                sw.Write("CLASS:"); sw.WriteLine(ev.Classification.ToString());

                // Alarms
                //foreach(
                
                sw.WriteLine("END:VEVENT");
            }

            sw.WriteLine("END:VCALENDAR");

            return sw.ToString();
            //TODO : CRLFSPACE 
            //System.IO.StringWriter sw = new System.IO.StringWriter();
            //sw.WriteLine("BEGIN:VCARD");
            //sw.WriteLine("VERSION:3.0");
            //if (this.FullName != null) sw.WriteLine("FN:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.FullName));
            //if (this.DisplayName != null) sw.WriteLine("NAME:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.DisplayName));
            //if (this.GeneratorId != null) sw.WriteLine("PRODID:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.GeneratorId));
            //if (this.Mailer != null) sw.WriteLine("MAILER:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Mailer));
            //if (this.Nickname != null) sw.WriteLine("NICKNAME:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Nickname));
            //if (this.Note != null) sw.WriteLine("NOTE:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Note));
            //if (this.Role != null) sw.WriteLine("ROLE:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Role));
            //if (this.SortText != null) sw.WriteLine("SORT-STRING:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.SortText));
            //if (this.Source != null) sw.WriteLine("SOURCE:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Source));
            //if (this.TimeZone != null) sw.WriteLine("TZ:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.TimeZone));
            //if (this.Title != null) sw.WriteLine("TITLE:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Title));
            //if (this.AccessClass != null) sw.WriteLine("CLASS:" + ActiveUp.Net.Groupware.vCard.Parser.Escape(this.AccessClass));
            //if (this.Addresses.Count > 0) foreach (ActiveUp.Net.Groupware.vCard.Address address in this.Addresses) sw.WriteLine(address.GetFormattedLine());
            //if (this.EmailAddresses.Count > 0) foreach (ActiveUp.Net.Groupware.vCard.EmailAddress email in this.EmailAddresses) sw.WriteLine(email.GetFormattedLine());
            //if (this.Labels.Count > 0) foreach (ActiveUp.Net.Groupware.vCard.Label label in this.Labels) sw.WriteLine(label.GetFormattedLine());
            //if (this.TelephoneNumbers.Count > 0) foreach (ActiveUp.Net.Groupware.vCard.TelephoneNumber number in this.TelephoneNumbers) sw.WriteLine(number.GetFormattedLine());
            //if (this.Name != null) sw.WriteLine(this.Name.GetFormattedLine());
            //if (this.Birthday != System.DateTime.MinValue) sw.WriteLine("BDAY:" + this.Birthday.ToString("yyyy-MM-dd"));
            //if (this.GeographicalPosition != null && (this.GeographicalPosition.Latitude != 0 && this.GeographicalPosition.Longitude != 0)) sw.WriteLine("GEO:" + this.GeographicalPosition.Latitude.ToString() + ";" + this.GeographicalPosition.Longitude.ToString());
            ////if(this.Organization!=null && this.Organization.Length>0)
            //if (this.Organization != null && this.Organization.Count > 0)
            //{
            //    string organization = "ORG:";
            //    foreach (string str in this.Organization) organization += ActiveUp.Net.Groupware.vCard.Parser.Escape(str) + ",";
            //    organization = organization.TrimEnd(',');
            //    sw.WriteLine(organization);
            //}
            ////if (this.Categories != null && this.Categories.Length > 0)
            //if (this.Categories != null && this.Categories.Count > 0)
            //{
            //    string categories = "CATEGORIES:";
            //    foreach (string str in this.Categories) categories += ActiveUp.Net.Groupware.vCard.Parser.Escape(str) + ",";
            //    categories = categories.TrimEnd(',');
            //    sw.WriteLine(categories);
            //}
            //if (this.Photo != null && this.Photo.Length > 0)
            //{
            //    string binary = "PHOTO:" + System.Convert.ToBase64String(this.Photo);
            //    sw.WriteLine(binary);
            //}
            //sw.WriteLine("REV:" + System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            //sw.WriteLine("END:VCARD");
            //return ActiveUp.Net.Groupware.vCard.Parser.Fold(sw.ToString());
        }
    }
}
