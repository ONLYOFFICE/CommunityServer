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
    public enum TelephoneNumberSingleType
    {
        Home,
        Message,
        Work,
        Voice,
        Fax,
        Prefered,
        Cellular,
        Video,
        Pager,
        BulletinBoard,
        Modem,
        Car,
        ISDN,
        PersonalCommunication
    }
    /// <summary>
    /// Description résumée de vCard.
    /// </summary>
    #if !PocketPC
    [System.Serializable]
    #endif
    public class TelephoneNumber
    {
        public TelephoneNumber()
        {
            //
            // TODO : ajoutez ici la logique du constructeur
            //
        }
        public TelephoneNumber(string number)
        {
            _number = number;
        }
        public TelephoneNumber(string number, TelephoneNumberSingleType type)
        {
            _number = number;

            switch (type)
            {
                case TelephoneNumberSingleType.BulletinBoard: _isBulletinBoard = true; break;
                case TelephoneNumberSingleType.Car: _isCar = true; break;
                case TelephoneNumberSingleType.Cellular: _isCellular = true; break;
                case TelephoneNumberSingleType.Fax: _isFax = true; break;
                case TelephoneNumberSingleType.Home: _isHome = true; break;
                case TelephoneNumberSingleType.ISDN: _isISDN = true; break;
                case TelephoneNumberSingleType.Message: _isMessage = true; break;
                case TelephoneNumberSingleType.Modem: _isModem = true; break;
                case TelephoneNumberSingleType.Pager: _isPager = true; break;
                case TelephoneNumberSingleType.PersonalCommunication: _isPersonalCommunication = true; break;
                case TelephoneNumberSingleType.Prefered: _isPrefered = true; break;
                case TelephoneNumberSingleType.Video: _isVideo = true; break;
                case TelephoneNumberSingleType.Voice: _isVoice = true; break;
                case TelephoneNumberSingleType.Work: _isWork = true; break;
            }
        }

        private string _number;
        private bool _isHome,_isMessage,_isWork,_isVoice,_isFax,_isPrefered,_isCellular,_isVideo,_isPager,_isBulletinBoard,_isModem,_isCar,_isISDN,_isPersonalCommunication;

        public string Number
        {
            get
            {
                return this._number;
            }
            set
            {
                this._number = value;
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
        public bool IsMessage
        {
            get
            {
                return this._isMessage;
            }
            set
            {
                this._isMessage = value;
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
        public bool IsVoice
        {
            get
            {
                return this._isVoice;
            }
            set
            {
                this._isVoice = value;
            }
        }
        public bool IsFax
        {
            get
            {
                return this._isFax;
            }
            set
            {
                this._isFax = value;
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
        public bool IsCellular
        {
            get
            {
                return this._isCellular;
            }
            set
            {
                this._isCellular = value;
            }
        }
        public bool IsVideo
        {
            get
            {
                return this._isVideo;
            }
            set
            {
                this._isVideo = value;
            }
        }
        public bool IsPager
        {
            get
            {
                return this._isPager;
            }
            set
            {
                this._isPager = value;
            }
        }
        public bool IsBulletinBoard
        {
            get
            {
                return this._isBulletinBoard;
            }
            set
            {
                this._isBulletinBoard = value;
            }
        }
        public bool IsModem
        {
            get
            {
                return this._isModem;
            }
            set
            {
                this._isModem = value;
            }
        }
        public bool IsCar
        {
            get
            {
                return this._isCar;
            }
            set
            {
                this._isCar = value;
            }
        }
        public bool IsISDN
        {
            get
            {
                return this._isISDN;
            }
            set
            {
                this._isISDN = value;
            }
        }
        public bool IsPersonalCommunication
        {
            get
            {
                return this._isPersonalCommunication;
            }
            set
            {
                this._isPersonalCommunication = value;
            }
        }

        public string GetFormattedLine()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("TEL;");
            if(this.IsBulletinBoard || this.IsCar || this.IsCellular || this.IsFax || this.IsHome || this.IsISDN || this.IsMessage || this.IsModem || this.IsPager || this.IsPersonalCommunication || this.IsPrefered || this.IsVoice || this.IsWork) sb.Append("TYPE=");
            if(this.IsBulletinBoard) sb.Append("bbs,");
            if(this.IsCar) sb.Append("car,");
            if(this.IsCellular) sb.Append("cell,");
            if(this.IsFax) sb.Append("fax,");
            if(this.IsHome) sb.Append("home,");
            if(this.IsMessage) sb.Append("msg,");
            if(this.IsModem) sb.Append("modem,");
            if(this.IsPager) sb.Append("pager,");
            if(this.IsPersonalCommunication) sb.Append("pcs,");
            if(this.IsPrefered) sb.Append("pref,");
            if(this.IsVideo) sb.Append("video,");
            if(this.IsVoice) sb.Append("voice,");
            if(this.IsWork) sb.Append("work,");
            sb.Remove(sb.Length-1,1);
            sb.Append(":");
            if(this.Number!=null) sb.Append(this.Number);
            return sb.ToString();
        }
    }
}
