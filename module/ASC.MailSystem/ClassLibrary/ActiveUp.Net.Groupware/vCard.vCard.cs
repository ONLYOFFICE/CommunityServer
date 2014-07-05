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

using System.Collections;
using System.IO;
namespace ActiveUp.Net.Groupware.vCard
{
    #if !PocketPC
    [System.Serializable]
    #endif
    public class vCard
    {
        public vCard()
        {
            
        }

        public static vCard LoadFromFile(string filename)
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

        private string _name,_source,_fn,_nickname,_mailer,_timeZone,_title,_role,_note,_prodId,_sort,_uid,_url,_version,_accessClass;
        private System.DateTime _bday,_revision;
        private ArrayList _org, _categories;
        //TODO _photo,_logo,_sound,_key
        private ActiveUp.Net.Groupware.vCard.AddressCollection _addresses = new ActiveUp.Net.Groupware.vCard.AddressCollection();
        private ActiveUp.Net.Groupware.vCard.EmailAddressCollection _emails = new ActiveUp.Net.Groupware.vCard.EmailAddressCollection();
        //private ActiveUp.Net.Groupware.vCard.vCard _agent = new ActiveUp.Net.Groupware.vCard.vCard();
        private ActiveUp.Net.Groupware.vCard.LabelCollection _labels = new ActiveUp.Net.Groupware.vCard.LabelCollection();
        private ActiveUp.Net.Groupware.vCard.TelephoneNumberCollection _telnums = new ActiveUp.Net.Groupware.vCard.TelephoneNumberCollection();
        private ActiveUp.Net.Groupware.vCard.GeographicalPosition _geo = new ActiveUp.Net.Groupware.vCard.GeographicalPosition();
        private ActiveUp.Net.Groupware.vCard.Name _n = new ActiveUp.Net.Groupware.vCard.Name();
        private byte[] _photo,_sound,_logo,_key;
        
        /// <summary>
        /// The displayable, presentation text associated with the source for the vCard, as specified in the SOURCE property.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
        /// <summary>
        /// To specify the formatted text corresponding to the name of the object the vCard represents.
        /// </summary>
        public string FullName
        {
            get
            {
                return this._fn;
            }
            set
            {
                this._fn = value;
            }
        }
        /// <summary>
        /// Information    how to find the source for the vCard.
        /// </summary>
        public string Source
        {
            get
            {
                return this._source;
            }
            set
            {
                this._source = value;
            }
        }
        /// <summary>
        /// Components of the name of the object the vCard represents.
        /// </summary>
        public ActiveUp.Net.Groupware.vCard.Name Name
        {
            get
            {
                return this._n;
            }
            set
            {
                this._n = value;
            }
        }
        /// <summary>
        /// The nickname of    the object the vCard represents.
        /// </summary>
        public string Nickname
        {
            get
            {
                return this._nickname;
            }
            set
            {
                this._nickname = value;
            }
        }
        /// <summary>
        /// Electronic mail software that is used by the individual associated with the vCard.
        /// </summary>
        public string Mailer
        {
            get
            {
                return this._mailer;
            }
            set
            {
                this._mailer = value;
            }
        }
        /// <summary>
        /// Information related to the time zone (UTC offset) of the object the vCard represents.
        /// </summary>
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
        /// <summary>
        /// The job title, functional position or function of the object the vCard represents.
        /// </summary>
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
            }
        }
        /// <summary>
        /// Information concerning the role, occupation, or business category of the object the vCard represents.
        /// </summary>
        public string Role
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
        /// <summary>
        /// Supplemental information or a comment that is associated with the vCard.
        /// </summary>
        public string Note
        {
            get
            {
                return this._note;
            }
            set
            {
                this._note = value;
            }
        }
        /// <summary>
        /// The identifier for the product that created    the vCard object. (ISO 9070)
        /// </summary>
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
        /// <summary>
        /// Specify the family name or given name text to be used for national-language-specific sorting of the Full Name and Name properties.
        /// </summary>
        public string SortText
        {
            get
            {
                return this._sort;
            }
            set
            {
                this._sort = value;
            }
        }
        /// <summary>
        /// A value that represents a globally unique identifier corresponding to the individual or resource associated with the vCard.
        /// </summary>
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
        /// <summary>
        /// Specify a uniform resource locator associated with the object that the vCard refers to.
        /// </summary>
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
        /// <summary>
        /// The version of the vCard specification used to format this vCard.
        /// </summary>
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
        /// <summary>
        /// The access classification for a vCard object. (i.e. PRIVATE, PUBLIC, CONFIDENTIAL)
        /// </summary>
        public string AccessClass
        {
            get
            {
                return this._accessClass;
            }
            set
            {
                this._accessClass = value;
            }
        }
        /// <summary>
        /// The birth date of the object the vCard represents.
        /// </summary>
        public System.DateTime Birthday
        {
            get
            {
                return this._bday;
            }
            set
            {
                this._bday = value;
            }
        }
        /// <summary>
        /// Revision information about the current vCard.
        /// The value distinguishes the current revision of    the information in this vCard for other renditions of the information.
        /// </summary>
        public System.DateTime Revision
        {
            get
            {
                return this._revision;
            }
            set
            {
                this._revision = value;
            }
        }
        /// <summary>
        /// The organizational name and units associated with the vCard.
        /// </summary>
        public ArrayList Organization
        {
            get
            {
                if (this._org == null)
                    this._org = new ArrayList();
                return this._org;
            }
            set
            {
                this._org = value;
            }
        }
        /// <summary>
        /// Application categories information about the vCard.
        /// </summary>
        public ArrayList Categories
        {
            get
            {
                if (this._categories == null)
                    this._categories = new ArrayList();
                return this._categories;
            }
            set
            {
                this._categories = value;
            }
        }
        /// <summary>
        /// The components of the delivery addresses for the vCard object.
        /// </summary>
        public ActiveUp.Net.Groupware.vCard.AddressCollection Addresses
        {
            get
            {
                return this._addresses;
            }
            set
            {
                this._addresses = value;
            }
        }
        /// <summary>
        /// The telephone numbers for telephony communication with the object the vCard represents.
        /// </summary>
        public ActiveUp.Net.Groupware.vCard.TelephoneNumberCollection TelephoneNumbers
        {
            get
            {
                return this._telnums;
            }
            set
            {
                this._telnums = value;
            }
        }
        /*/// <summary>
        /// Information about another person who will act on behalf of the individual or resource associated with the vCard.
        /// </summary>
        public ActiveUp.Net.Groupware.vCard.vCard Agent
        {
            get
            {
                return this._agent;
            }
            set
            {
                this._agent = value;
            }
        }*/
        /// <summary>
        /// The formatted text corresponding to delivery addresses of the object the vCard represents.
        /// </summary>
        public ActiveUp.Net.Groupware.vCard.LabelCollection Labels
        {
            get
            {
                return this._labels;
            }
            set
            {
                this._labels = value;
            }
        }
        /// <summary>
        /// Information related to the global positioning of the object the vCard represents.
        /// </summary>
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
        /// <summary>
        /// The electronic mail addresses for communication with the object the vCard represents.
        /// </summary>
        public ActiveUp.Net.Groupware.vCard.EmailAddressCollection EmailAddresses
        {
            get
            {
                return this._emails;
            }
            set
            {
                this._emails = value;
            }
        }
        /// <summary>
        /// An image or photograph information that    annotates some aspect of the object the vCard represents.
        /// </summary>
        public byte[] Photo
        {
            get
            {
                return this._photo;
            }
            set
            {
                this._photo = value;
            }
        }
        /// <summary>
        /// A graphic image of a logo associated with the object the vCard represents.
        /// </summary>
        public byte[] Logo
        {
            get
            {
                return this._logo;
            }
            set
            {
                this._logo = value;
            }
        }
        /// <summary>
        /// A digital sound content information that annotates some aspect of the vCard. By default this type is used to specify the proper pronunciation of the name type value of the vCard.
        /// </summary>
        public byte[] Sound
        {
            get
            {
                return this._sound;
            }
            set
            {
                this._sound = value;
            }
        }
        /// <summary>
        /// A public key or authentication certificate associated with the object that the vCard represents.
        /// </summary>
        public byte[] Key
        {
            get
            {
                return this._key;
            }
            set
            {
                this._key = value;
            }
        }

        public string GetData()
        {

            //TODO : CRLFSPACE 
            System.IO.StringWriter sw = new System.IO.StringWriter();
            sw.WriteLine("BEGIN:VCARD");
            sw.WriteLine("VERSION:3.0");
            if(this.FullName!=null) sw.WriteLine("FN:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.FullName));
            if(this.DisplayName!=null) sw.WriteLine("NAME:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.DisplayName));
            if(this.GeneratorId!=null) sw.WriteLine("PRODID:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.GeneratorId));
            if(this.Mailer!=null) sw.WriteLine("MAILER:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Mailer));
            if(this.Nickname!=null) sw.WriteLine("NICKNAME:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Nickname));
            if(this.Note!=null) sw.WriteLine("NOTE:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Note));
            if(this.Role!=null) sw.WriteLine("ROLE:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Role));
            if(this.SortText!=null) sw.WriteLine("SORT-STRING:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.SortText));
            if(this.Source!=null) sw.WriteLine("SOURCE:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Source));
            if(this.TimeZone!=null) sw.WriteLine("TZ:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.TimeZone));
            if(this.Title!=null) sw.WriteLine("TITLE:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.Title));
            if(this.AccessClass!=null) sw.WriteLine("CLASS:"+ActiveUp.Net.Groupware.vCard.Parser.Escape(this.AccessClass));
            if(this.Addresses.Count>0) foreach(ActiveUp.Net.Groupware.vCard.Address address in this.Addresses) sw.WriteLine(address.GetFormattedLine());
            if(this.EmailAddresses.Count>0) foreach(ActiveUp.Net.Groupware.vCard.EmailAddress email in this.EmailAddresses) sw.WriteLine(email.GetFormattedLine());
            if(this.Labels.Count>0) foreach(ActiveUp.Net.Groupware.vCard.Label label in this.Labels) sw.WriteLine(label.GetFormattedLine());
            if(this.TelephoneNumbers.Count>0) foreach(ActiveUp.Net.Groupware.vCard.TelephoneNumber number in this.TelephoneNumbers) sw.WriteLine(number.GetFormattedLine());
            if(this.Name!=null) sw.WriteLine(this.Name.GetFormattedLine());
            if(this.Birthday!=System.DateTime.MinValue) sw.WriteLine("BDAY:"+this.Birthday.ToString("yyyy-MM-dd"));
            if(this.GeographicalPosition!=null && (this.GeographicalPosition.Latitude!=0 && this.GeographicalPosition.Longitude!=0)) sw.WriteLine("GEO:"+this.GeographicalPosition.Latitude.ToString()+";"+this.GeographicalPosition.Longitude.ToString());
            //if(this.Organization!=null && this.Organization.Length>0)
            if(this.Organization!=null && this.Organization.Count>0)
            {
                string organization = "ORG:";
                foreach(string str in this.Organization) organization += ActiveUp.Net.Groupware.vCard.Parser.Escape(str)+",";
                organization = organization.TrimEnd(',');
                sw.WriteLine(organization);
            }
            //if (this.Categories != null && this.Categories.Length > 0)
            if (this.Categories != null && this.Categories.Count > 0)
            {
                string categories = "CATEGORIES:";
                foreach(string str in this.Categories) categories += ActiveUp.Net.Groupware.vCard.Parser.Escape(str)+",";
                categories = categories.TrimEnd(',');
                sw.WriteLine(categories);
            }
            if(this.Photo!=null && this.Photo.Length>0)
            {
                string binary = "PHOTO:"+System.Convert.ToBase64String(this.Photo);
                sw.WriteLine(binary);
            }
            sw.WriteLine("REV:"+System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            sw.WriteLine("END:VCARD");
            return ActiveUp.Net.Groupware.vCard.Parser.Fold(sw.ToString());
        }
    }
}
