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
    public class Address
    {
        public Address()
        {
            //
            // TODO : ajoutez ici la logique du constructeur
            //
        }
        private string _extAddress,_strtAddress,_locality,_region,_postalCode,_country;
        private int _poBox;
        private bool _isDomestic,_isInternational,_isPostal,_isParcel,_isWork,_isHome,_isPrefered = false;

        public string ExtendedAddress
        {
            get
            {
                return this._extAddress;
            }
            set
            {
                this._extAddress = value;
            }
        }
        public string StreetAddress
        {
            get
            {
                return this._strtAddress;
            }
            set
            {
                this._strtAddress = value;
            }
        }
        public string Locality
        {
            get
            {
                return this._locality;
            }
            set
            {
                this._locality = value;
            }
        }
        public string Region
        {
            get
            {
                return this._region;
            }
            set
            {
                this._region = value;
            }
        }
        public string PostalCode
        {
            get
            {
                return this._postalCode;
            }
            set
            {
                this._postalCode = value;
            }
        }
        public string Country
        {
            get
            {
                return this._country;
            }
            set
            {
                this._country = value;
            }
        }
        public int POBox
        {
            get
            {
                return this._poBox;
            }
            set
            {
                this._poBox = value;
            }
        }
        public bool IsDomestic
        {
            get
            {
                return this._isDomestic;
            }
            set
            {
                this._isDomestic = value;
            }
        }
        public bool IsInternational
        {
            get
            {
                return this._isInternational;
            }
            set
            {
                this._isInternational = value;
            }
        }
        public bool IsPostal
        {
            get
            {
                return this._isPostal;
            }
            set
            {
                this._isPostal = value;
            }
        }
        public bool IsParcel
        {
            get
            {
                return this._isParcel;
            }
            set
            {
                this._isParcel = value;
            }
        }
        public bool IsWork
        {
            get
            {
                return this._isWork;
            }
            set
            {
                this._isWork = value;
            }
        }
        public bool IsHome
        {
            get
            {
                return this._isHome;
            }
            set
            {
                this._isHome = value;
            }
        }
        public bool IsPrefered
        {
            get
            {
                return this._isPrefered;
            }
            set
            {
                this._isPrefered = value;
            }
        }
        public string GetFormattedLine()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("ADR;");
            if(this.IsDomestic || this.IsHome || this.IsParcel || this.IsPostal || this.IsPrefered || this.IsWork) sb.Append("TYPE=");
            if(this.IsDomestic) sb.Append("dom,");
            if(this.IsHome) sb.Append("home,");
            if(this.IsParcel) sb.Append("parcel,");
            if(this.IsPostal) sb.Append("postal,");
            if(this.IsPrefered) sb.Append("pref,");
            if(this.IsWork) sb.Append("work,");
            sb.Remove(sb.Length-1,1);
            sb.Append(":");
            if(this.POBox!=-1) sb.Append(this.POBox.ToString()+";");
            else sb.Append(";");
            if(this.ExtendedAddress!=null) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(this.ExtendedAddress)+";");
            else sb.Append(";");
            if(this.StreetAddress!=null) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(this.StreetAddress)+";");
            else sb.Append(";");
            if(this.Locality!=null) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Locality)+";");
            else sb.Append(";");
            if(this.Region!=null) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Region)+";");
            else sb.Append(";");
            if(this.PostalCode!=null) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(this.PostalCode)+";");
            else sb.Append(";");
            if(this.Country!=null) sb.Append(ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Country)+";");
            else sb.Append(";");
            sb.Remove(sb.Length-1,1);
            return sb.ToString();
        }
    }
}
