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
    public class vTodo : Entity
    {
        public vTodo()
        {

        }
        
        private System.DateTime _created,_completed,_due;

        public System.DateTime Created
        {
            get
            {
                return this._created;
            }
            set
            {
                this._created = value;
            }
        }
        public System.DateTime Completed
        {
            get
            {
                return this._completed;
            }
            set
            {
                this._completed = value;
            }
        }
        public System.DateTime Due
        {
            get
            {
                return this._due;
            }
            set
            {
                this._due = value;
            }
        }
    }
}