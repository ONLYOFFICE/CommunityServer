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








namespace ActiveUp.Net.Groupware.vCalendar
{
    #if !PocketPC
    [System.Serializable]
    #endif
    public class Duration
    {
        public Duration()
        {
            
        }
        private int _years,_months,_weeks,_days,_hours,_minutes,_seconds;

        public int Years
        {
            get
            {
                return this._years;
            }
            set
            {
                this._years = value;
            }
        }
        public int Months
        {
            get
            {
                return this._months;
            }
            set
            {
                this._months = value;
            }
        }
        public int Weeks
        {
            get
            {
                return this._weeks;
            }
            set
            {
                this._weeks = value;
            }
        }
        public int Days
        {
            get
            {
                return this._days;
            }
            set
            {
                this._days = value;
            }
        }
        public int Hours
        {
            get
            {
                return this._hours;
            }
            set
            {
                this._hours = value;
            }
        }
        public int Minutes
        {
            get
            {
                return this._minutes;
            }
            set
            {
                this._minutes = value;
            }
        }
        public int Seconds
        {
            get
            {
                return this._seconds;
            }
            set
            {
                this._seconds = value;
            }
        }

        
    }

}