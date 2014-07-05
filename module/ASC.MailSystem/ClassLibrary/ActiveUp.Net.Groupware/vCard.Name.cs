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

namespace ActiveUp.Net.Groupware.vCard
{
    /// <summary>
    /// Description résumée de vCard.
    /// </summary>
    #if !PocketPC
    [System.Serializable]
    #endif
    public class Name
    {
        public Name()
        {
            //
            // TODO : ajoutez ici la logique du constructeur
            //
        }

        private string _family,_given;
        private string[] _additional,_prefix,_suffix;

        public string FamilyName
        {
            get
            {
                return this._family;
            }
            set
            {
                this._family = value;
            }
        }
        public string GivenName
        {
            get
            {
                return this._given;
            }
            set
            {
                this._given = value;
            }
        }
        public string[] AdditionalNames
        {
            get
            {
                return this._additional;
            }
            set
            {
                this._additional = value;
            }
        }
        public string[] Prefixes
        {
            get
            {
                return this._prefix;
            }
            set
            {
                this._prefix = value;
            }
        }
        public string[] Suffixes
        {
            get
            {
                return this._suffix;
            }
            set
            {
                this._suffix = value;
            }
        }

        public string GetFormattedLine()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("N:");
            if(this.FamilyName!=null) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(this.FamilyName)+";");
            else sb.Append(";");
            if(this.GivenName!=null) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(this.GivenName)+";");
            else sb.Append(";");
            if(this.AdditionalNames!=null && this.AdditionalNames.Length>0)
            {
                foreach(string str in this.AdditionalNames) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(str)+",");
                sb.Remove(sb.Length-1,1);
                sb.Append(";");
            }
            if(this.Prefixes!=null && this.Prefixes.Length>0)
            {
                foreach(string str in this.Prefixes) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(str)+",");
                sb.Remove(sb.Length-1,1);
                sb.Append(";");
            }
            if(this.Suffixes!=null && this.Suffixes.Length>0)
            {
                foreach(string str in this.Suffixes) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(str)+",");
                sb.Remove(sb.Length-1,1);
            }
            return sb.ToString();
        }
    }
}
