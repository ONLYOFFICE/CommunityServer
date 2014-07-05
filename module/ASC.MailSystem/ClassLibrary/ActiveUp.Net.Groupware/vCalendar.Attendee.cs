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
    public class Attendee : Property
    {
        public Attendee()
        {

        }
        private Role _role;
        private Status _status;
        private bool _reply;
        private Expectation _expectation;
        private Address _contact;

        public Role Role
        {
            get
            {
                return this._role;
            }
            set
            {
                this._role = value;
            }
        }
        public Status Status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = value;
            }
        }
        public bool ReplyRequested
        {
            get
            {
                return this._reply;
            }
            set
            {
                this._reply = value;
            }
        }
        public Expectation Expectation
        {
            get
            {
                return this._expectation;
            }
            set
            {
                this._expectation = value;
            }
        }
        public Address Contact
        {
            get
            {
                return this._contact;
            }
            set
            {
                this._contact = value;
            }
        }
    }

}