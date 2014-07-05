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

using System;
namespace ActiveUp.Net.Groupware.vCard
{
    /// <summary>
    /// Parses text to vCard objects.
    /// </summary>
    public abstract class Parser
    {
        public static ActiveUp.Net.Groupware.vCard.vCard Parse(string data)
        {
            ActiveUp.Net.Groupware.vCard.vCard card = new ActiveUp.Net.Groupware.vCard.vCard();
            data = ActiveUp.Net.Groupware.vCard.Parser.Unfold(data);
            data = data.Replace("\\,","²²²COMMA²²²");
            data = data.Replace("\\;","²²²SEMICOLON²²²");
            foreach(string line in System.Text.RegularExpressions.Regex.Split(data,"\r\n"))
            {
                string fulltype = line.Split(':')[0];
                string type = fulltype.Split(';')[0].ToUpper();
                switch(type)
                {
                    case "NAME": Parser.SetDisplayName(card,line);
                        break;
                    case "FN": Parser.SetFullName(card,line);
                        break;
                    case "N": Parser.SetName(card,line);
                        break;
                    case "NICKNAME": Parser.SetNickName(card,line);
                        break;
                    case "SOURCE": Parser.SetSource(card,line);
                        break;
                    case "MAILER": Parser.SetMailer(card,line);
                        break;
                    case "TZ": Parser.SetTimeZone(card,line);
                        break;
                    case "TITLE": Parser.SetTitle(card,line);
                        break;
                    case "ROLE": Parser.SetRole(card,line);
                        break;
                    case "NOTE": Parser.SetNote(card,line);
                        break;
                    case "PRODID": Parser.SetGeneratorId(card,line);
                        break;
                    case "SORT-STRING": Parser.SetSortText(card,line);
                        break;
                    case "UID": Parser.SetUid(card,line);
                        break;
                    case "URL": Parser.SetUrl(card,line);
                        break;
                    case "VERSION": Parser.SetVersion(card,line);
                        break;
                    case "CLASS": Parser.SetAccessClass(card,line);
                        break;
                    case "ADR": Parser.AddAddress(card,line);
                        break;
                    case "TEL": Parser.AddTelephoneNumber(card,line);
                        break;
                    case "LABEL": Parser.AddLabel(card,line);
                        break;
                    case "EMAIL": Parser.AddEmail(card,line);
                        break;
                    case "BDAY": Parser.SetBirthday(card,line);
                        break;
                    case "REV": Parser.SetRevision(card,line);
                        break;
                    case "ORG": Parser.SetOrganization(card,line);
                        break;
                    case "CATEGORIES": Parser.SetCategories(card,line);
                        break;
                    case "PHOTO": Parser.SetPhoto(card,line);
                        break;
                    case "SOUND": Parser.SetSound(card,line);
                        break;
                    case "LOGO": Parser.SetLogo(card,line);
                        break;
                    case "KEY": Parser.SetKey(card,line);
                        break;
                    case "GEO": Parser.SetGeo(card,line);
                        break;
                }
            }
            return card;
        }
        public static ActiveUp.Net.Groupware.vCard.vCard Parse(byte[] data)
        {
            return ActiveUp.Net.Groupware.vCard.Parser.Parse(System.Text.Encoding.UTF7.GetString(data,0,data.Length));
        }
        public static System.DateTime ParseDate(string input)
        {
            try { return System.DateTime.Parse(input); }
            catch 
            {
                if(input.Length==8)
                {
                    input = input.Insert(4,"-");
                    input = input.Insert(7,"-");
                }
                else if(input.Length==16)
                {
                    input = input.Insert(4,"-");
                    input = input.Insert(7,"-");
                    input = input.Insert(13,":");
                    input = input.Insert(16,":");
                }
            }
            return System.DateTime.Parse(input);
        }
        private static void AddAddress(vCard card, string line)
        {
            string val = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
            string type = line.Split(':')[0].ToUpper();
            if(type.IndexOf("ENCODING=QUOTED-PRINTABLE")!=-1) val = FromQuotedPrintable(val,"utf-8");
            if (type.IndexOf("ENCODING=B") != -1)
            {
                byte[] data = System.Convert.FromBase64String(val);
                val = System.Text.Encoding.UTF8.GetString(data,0,data.Length);
            }
            string[] values = val.Split(';');
            ActiveUp.Net.Groupware.vCard.Address adr = new ActiveUp.Net.Groupware.vCard.Address();
            if (values.Length > 0 && values[0].Length > 0) adr.POBox = System.Convert.ToInt32(values[0]);
            if (values.Length > 1 && values[1].Length > 0) adr.ExtendedAddress = values[1];
            if (values.Length > 2 && values[2].Length>0) adr.StreetAddress = values[2];
            if (values.Length > 3 && values[3].Length > 0) adr.Locality = values[3];
            if (values.Length > 4 && values[4].Length > 0) adr.Region = values[4];
            if (values.Length > 5 && values[5].Length > 0) adr.PostalCode = values[5];
            if (values.Length > 6 && values[6].Length > 0) adr.Country = values[6];
            string parameters = line.Split(':')[0].ToUpper();
            if(parameters.IndexOf("DOM")!=-1) adr.IsDomestic = true;
            if(parameters.IndexOf("INTL")!=-1) adr.IsInternational = true;
            if(parameters.IndexOf("POSTAL")!=-1) adr.IsPostal = true;
            if(parameters.IndexOf("PARCEL")!=-1) adr.IsParcel = true;
            if(parameters.IndexOf("HOME")!=-1) adr.IsHome = true;
            if(parameters.IndexOf("WORK")!=-1) adr.IsWork = true;
            if(parameters.IndexOf("PREF")!=-1) adr.IsPrefered = true;
            card.Addresses.Add(adr);
        }
        private static void AddTelephoneNumber(vCard card, string line)
        {
            string val = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
            ActiveUp.Net.Groupware.vCard.TelephoneNumber tel = new ActiveUp.Net.Groupware.vCard.TelephoneNumber();
            tel.Number = val;
            string parameters = line.Split(':')[0].ToUpper();
            if(parameters.IndexOf("HOME")!=-1) tel.IsHome = true;
            if(parameters.IndexOf("MSG")!=-1) tel.IsMessage = true;
            if(parameters.IndexOf("WORK")!=-1) tel.IsWork = true;
            if(parameters.IndexOf("VOICE")!=-1) tel.IsVoice = true;
            if(parameters.IndexOf("FAX")!=-1) tel.IsFax = true;
            if(parameters.IndexOf("PREF")!=-1) tel.IsPrefered = true;
            if(parameters.IndexOf("CELL")!=-1) tel.IsCellular = true;
            if(parameters.IndexOf("VIDEO")!=-1) tel.IsVideo = true;
            if(parameters.IndexOf("PAGER")!=-1) tel.IsPager = true;
            if(parameters.IndexOf("BBS")!=-1) tel.IsBulletinBoard = true;
            if(parameters.IndexOf("MODEM")!=-1) tel.IsModem = true;
            if(parameters.IndexOf("CAR")!=-1) tel.IsCar = true;
            if(parameters.IndexOf("ISDN")!=-1) tel.IsISDN = true;
            if(parameters.IndexOf("PCS")!=-1) tel.IsPersonalCommunication = true;
            card.TelephoneNumbers.Add(tel);
        }
        private static void AddLabel(vCard card, string line)
        {
            string val = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
            string type = line.Split(':')[0].ToUpper();
            if(type.IndexOf("ENCODING=QUOTED-PRINTABLE")!=-1) val = FromQuotedPrintable(val,"utf-8");
            if (type.IndexOf("ENCODING=B") != -1)
            {
                byte[] data = System.Convert.FromBase64String(val);
                val = System.Text.Encoding.UTF8.GetString(data,0,data.Length);
            }
            ActiveUp.Net.Groupware.vCard.Label label = new ActiveUp.Net.Groupware.vCard.Label();
            label.Value = val;
            string parameters = line.Split(':')[0].ToUpper();
            if(parameters.IndexOf("DOM")!=-1) label.IsDomestic = true;
            if(parameters.IndexOf("INTL")!=-1) label.IsInternational = true;
            if(parameters.IndexOf("POSTAL")!=-1) label.IsPostal = true;
            if(parameters.IndexOf("PARCEL")!=-1) label.IsParcel = true;
            if(parameters.IndexOf("HOME")!=-1) label.IsHome = true;
            if(parameters.IndexOf("WORK")!=-1) label.IsWork = true;
            if(parameters.IndexOf("PREF")!=-1) label.IsPrefered = true;
            card.Labels.Add(label);
        }
        private static void AddEmail(vCard card, string line)
        {
            string val = line.Split(':')[1];
            ActiveUp.Net.Groupware.vCard.EmailAddress email = new ActiveUp.Net.Groupware.vCard.EmailAddress();
            email.Address = val;
            string parameters = line.Split(':')[0].ToUpper();
            if(parameters.IndexOf("INTERNET")!=-1) email.IsInternet = true;
            if(parameters.IndexOf("X400")!=-1) email.IsX400 = true;
            if(parameters.IndexOf("PREF")!=-1) email.IsPrefered = true;
            card.EmailAddresses.Add(email);
        }
        private static void SetFullName(vCard card, string line)
        {
            card.FullName = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetDisplayName(vCard card, string line)
        {
            card.DisplayName = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetName(vCard card, string line)
        {
            string val = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
            string[] values = val.Split(';');
            if(values.Length==0) return;
            if(values.Length>0) card.Name.FamilyName = ActiveUp.Net.Groupware.vCard.Parser.Unescape(values[0]);
            if(values.Length>1) card.Name.GivenName = ActiveUp.Net.Groupware.vCard.Parser.Unescape(values[1]);
            if(values.Length>2) card.Name.AdditionalNames = ActiveUp.Net.Groupware.vCard.Parser.UnescapeArray(values[2].Split(','));
            if(values.Length>3) card.Name.Prefixes = ActiveUp.Net.Groupware.vCard.Parser.UnescapeArray(values[3].Split(','));
            if(values.Length>4) card.Name.Suffixes = ActiveUp.Net.Groupware.vCard.Parser.UnescapeArray(values[4].Split(','));
        }
        private static void SetNickName(vCard card, string line)
        {
            card.Nickname = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetSource(vCard card, string line)
        {
            card.Source = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetMailer(vCard card, string line)
        {
            card.Mailer = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetTimeZone(vCard card, string line)
        {
            card.TimeZone = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetTitle(vCard card, string line)
        {
            card.Title = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetRole(vCard card, string line)
        {
            card.Role = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetNote(vCard card, string line)
        {
            card.Note = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetGeneratorId(vCard card, string line)
        {
            card.GeneratorId = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetSortText(vCard card, string line)
        {
            card.SortText = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetUid(vCard card, string line)
        {
            card.Uid = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetUrl(vCard card, string line)
        {
            card.Url = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetVersion(vCard card, string line)
        {
            card.Version = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetAccessClass(vCard card, string line)
        {
            card.AccessClass = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetBirthday(vCard card, string line)
        {
            card.Birthday = Parser.ParseDate(ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":","")));
        }
        private static void SetRevision(vCard card, string line)
        {
            card.Revision = Parser.ParseDate(ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":","")));
        }
        private static void SetOrganization(vCard card, string line)
        {
            foreach (string organisation in Parser.UnescapeArray(line.Replace(line.Split(':')[0] + ":", "").Split(',')))
                card.Organization.Add(organisation);// = Parser.UnescapeArray(line.Replace(line.Split(':')[0]+":","").Split(','));
        }
        private static void SetCategories(vCard card, string line)
        {
            foreach(string category in Parser.UnescapeArray(line.Replace(line.Split(':')[0]+":","").Split(',')))
                card.Categories.Add(category);// = Parser.UnescapeArray(line.Replace(line.Split(':')[0]+":","").Split(','));
        }
        private static void SetPhoto(vCard card, string line)
        {
            card.Photo = System.Convert.FromBase64String(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetLogo(vCard card, string line)
        {
            card.Logo = System.Convert.FromBase64String(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetSound(vCard card, string line)
        {
            card.Sound = System.Convert.FromBase64String(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetKey(vCard card, string line)
        {
            card.Key = System.Convert.FromBase64String(line.Replace(line.Split(':')[0]+":",""));
        }
        private static void SetGeo(vCard card, string line)
        {
            GeographicalPosition geo = new GeographicalPosition();
            string val = ActiveUp.Net.Groupware.vCard.Parser.Unescape(line.Replace(line.Split(':')[0]+":",""));
            string[] values = val.Split(';');
            geo.Latitude = System.Convert.ToDecimal(values[0]);
            geo.Longitude = System.Convert.ToDecimal(values[1]);
            card.GeographicalPosition = geo;
        }
        public static string Escape(string input)
        {
            input = input.Replace(",","\\,");
            input = input.Replace(";","\\;");
            input = input.Replace("\\n","\\\\n");
            return input;
        }
        public static string Unescape(string input)
        {
            input = input.Replace("\\,",",");
            input = input.Replace("\\;",";");
            input = input.Replace("\\\\n","\\n");
            input = input.Replace("\\\\N","\\n");
            input = input.Replace("²²²COMMA²²²",",");
            input = input.Replace("²²²SEMICOLON²²²",";");
            return input;
        }
        public static string[] UnescapeArray(string[] input)
        {
            for(int i=0;i<input.Length;i++) input[i] = ActiveUp.Net.Groupware.vCard.Parser.Unescape(input[i]);
            return input;
        }
        public static string Fold(string input)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int i=0;
            for(i=0;i<input.Length-72;i+=72) sb.Append(input.Substring(i,72)+"\r\n ");
            sb.Append(input.Substring(i));
            return sb.ToString();
        }
        public static string Unfold(string input)
        {
            input = input.Replace("\r\n ","");
            return input;
        }
        public static string FromQuotedPrintable(string input, string toCharset)
        {
            try
            {
                input = input.Replace("=\r\n", "") + "=3D=3D";
                System.Collections.ArrayList arr = new System.Collections.ArrayList();
                int i = 0;
                byte[] decoded = new byte[0];
                while (true)
                {
                    if (i <= (input.Length) - 3)
                    {
                        if (input[i] == '=' && input[i + 1] != '=')
                        {
                            arr.Add(System.Convert.ToByte(System.Int32.Parse(String.Concat((char)input[i + 1], (char)input[i + 2]), System.Globalization.NumberStyles.HexNumber)));
                            i += 3;
                        }
                        else
                        {
                            arr.Add((byte)input[i]);
                            i++;
                        }
                    }
                    else break;
                }
                decoded = new byte[arr.Count];
                for (int j = 0; j < arr.Count; j++) decoded[j] = (byte)arr[j];
                return System.Text.Encoding.GetEncoding(toCharset).GetString(decoded,0,decoded.Length).TrimEnd('=');
            }
            catch { return input; }
        }
    }
}