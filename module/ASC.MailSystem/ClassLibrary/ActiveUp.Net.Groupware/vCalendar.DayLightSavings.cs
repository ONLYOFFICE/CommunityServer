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
    public class DayLightSavings
    {
        public DayLightSavings()
        {
            
        }
        private bool _isObserved;
        private int _offset;
        private string _standard,_savings;
        private System.DateTime _start,_end;

        public string StandardTimeDesignation
        {
            get
            {
                return this._standard;
            }
            set
            {
                this._standard = value;
            }
        }
        public string Designation
        {
            get
            {
                return this._savings;
            }
            set
            {
                this._savings = value;
            }
        }
        public int Offset
        {
            get
            {
                return this._offset;
            }
            set
            {
                this._offset = value;
            }
        }
        public System.DateTime Start
        {
            get
            {
                return this._start;
            }
            set
            {
                this._start = value;
            }
        }
        public System.DateTime End
        {
            get
            {
                return this._end;
            }
            set
            {
                this._end = value;
            }
        }
        public bool IsObserved
        {
            get
            {
                return this._isObserved;
            }
            set
            {
                this._isObserved = value;
            }
        }
    }
}
