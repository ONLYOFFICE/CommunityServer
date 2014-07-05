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
    public abstract class Entity
    {
        private ActiveUp.Net.Groupware.vCalendar.AttachmentCollection _attachments;
        private AttendeeCollection _attendees;
        private AudioReminder _audio = new AudioReminder();
        private string[] _categories,_ressources;
        private Classification _classification;
        private string _description,_exceptionRule,_recurrenceRule,_summary,_url,_uid,_location;
        private System.DateTime _lastModified,_start;
        private System.DateTime[] _exceptions;
        private System.DateTime[] _recurrence;
        private int _occurences,_priority,_revisions;
        private EntityCollection _relatedEntities = new EntityCollection();
        private Status _status;
        private bool _blocksTime;

        public ActiveUp.Net.Groupware.vCalendar.AttachmentCollection Attachments
        {
            get
            {
                if (this._attachments == null)
                    this._attachments = new AttachmentCollection();
                return this._attachments;
            }
            set
            {
                this._attachments = value;
            }
        }
        public AttendeeCollection Attendees
        {
            get
            {
                if (this._attendees == null)
                    this._attendees = new AttendeeCollection();
                return this._attendees;
            }
            set
            {
                this._attendees = value;
            }
        }
        public AudioReminder AudioReminder
        {
            get
            {
                return this._audio;
            }
            set
            {
                this._audio = value;
            }
        }
        public string[] Categories
        {
            get
            {
                return this._categories;
            }
            set
            {
                this._categories = value;
            }
        }
        public Classification Classification
        {
            get
            {
                return this._classification;
            }
            set
            {
                this._classification = value;
            }
        }
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }
        public System.DateTime[] Exceptions
        {
            get
            {
                return this._exceptions;
            }
            set
            {
                this._exceptions = value;
            }
        }
        public string ExceptionRule
        {
            get
            {
                return this._exceptionRule;
            }
            set
            {
                this._exceptionRule = value;
            }
        }
        public System.DateTime LastModified
        {
            get
            {
                return this._lastModified;
            }
            set
            {
                this._lastModified = value;
            }
        }
        public int Occurences
        {
            get
            {
                return this._occurences;
            }
            set
            {
                this._occurences = value;
            }
        }
        public int Priority
        {
            get
            {
                return this._priority;
            }
            set
            {
                this._priority = value;
            }
        }
        public EntityCollection RelatedEntities
        {
            get
            {
                return this._relatedEntities;
            }
            set
            {
                this._relatedEntities = value;
            }
        }
        public string RecurrenceRule
        {
            get
            {
                return this._recurrenceRule;
            }
            set
            {
                this._recurrenceRule = value;
            }
        }
        public System.DateTime[] RecurrenceDates
        {
            get
            {
                return this._recurrence;
            }
            set
            {
                this._recurrence = value;
            }
        }
        public string[] Ressources
        {
            get
            {
                return this._ressources;
            }
            set
            {
                this._ressources = value;
            }
        }
        public int Revisions
        {
            get
            {
                return this._revisions;
            }
            set
            {
                this._revisions = value;
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
        public string Summary
        {
            get
            {
                return this._summary;
            }
            set
            {
                this._summary = value;
            }
        }
        public string Location
        {
            get
            {
                return this._location;
            }
            set
            {
                this._location = value;
            }
        }
        public bool BlocksTime
        {
            get
            {
                return this._blocksTime;
            }
            set
            {
                this._blocksTime = value;
            }
        }
        public string Url
        {
            get
            {
                return this._url;
            }
            set
            {
                this._url = value;
            }
        }
        public string Uid
        {
            get
            {
                return this._uid;
            }
            set
            {
                this._uid = value;
            }
        }
    }
}