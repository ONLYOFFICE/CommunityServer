/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.Mail.Net.Mime.vCard
{
    #region usings

    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// Rfc 2426 vCard implementation.
    /// </summary>
    public class vCard
    {
        #region Members

        private readonly ItemCollection m_pItems;
        private DeliveryAddressCollection m_pAddresses;
        private EmailAddressCollection m_pEmailAddresses;
        private PhoneNumberCollection m_pPhoneNumbers;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public vCard()
        {
            m_pItems = new ItemCollection();
            Version = "3.0";
            UID = Guid.NewGuid().ToString();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets addresses collection.
        /// </summary>
        public DeliveryAddressCollection Addresses
        {
            get
            {
                // Delay collection creation, create it when needed.
                if (m_pAddresses == null)
                {
                    m_pAddresses = new DeliveryAddressCollection(this);
                }

                return m_pAddresses;
            }
        }

        /// <summary>
        /// Gets or sets birth date. Returns DateTime.MinValue if not set.
        /// </summary>
        public DateTime BirthDate
        {
            get
            {
                Item item = m_pItems.GetFirst("BDAY");
                if (item != null)
                {
                    string date = item.DecodedValue.Replace("-", "");
                    string[] dateFormats = new[] {"yyyyMMdd", "yyyyMMddz"};
                    return DateTime.ParseExact(date,
                                               dateFormats,
                                               DateTimeFormatInfo.InvariantInfo,
                                               DateTimeStyles.None);
                }
                else
                {
                    return DateTime.MinValue;
                }
            }

            set
            {
                if (value != DateTime.MinValue)
                {
                    m_pItems.SetValue("BDAY", value.ToString("yyyyMMdd"));
                }
                else
                {
                    m_pItems.SetValue("BDAY", null);
                }
            }
        }

        /// <summary>
        /// Gets email addresses collection.
        /// </summary>
        public EmailAddressCollection EmailAddresses
        {
            get
            {
                // Delay collection creation, create it when needed.
                if (m_pEmailAddresses == null)
                {
                    m_pEmailAddresses = new EmailAddressCollection(this);
                }

                return m_pEmailAddresses;
            }
        }

        /// <summary>
        /// Gets or sets formatted(Display name) name.  Returns null if FN: item doesn't exist.
        /// </summary>
        public string FormattedName
        {
            get
            {
                Item item = m_pItems.GetFirst("FN");
                if (item != null)
                {
                    return item.DecodedValue;
                }
                else
                {
                    return null;
                }
            }

            set { m_pItems.SetDecodedValue("FN", value); }
        }

        /// <summary>
        /// Gets or sets vCard home URL.
        /// </summary>
        public string HomeURL
        {
            get
            {
                Item[] items = m_pItems.Get("URL");
                foreach (Item item in items)
                {
                    if (item.ParametersString == "" || item.ParametersString.ToUpper().IndexOf("HOME") > -1)
                    {
                        return item.DecodedValue;
                    }
                }

                return null;
            }

            set
            {
                Item[] items = m_pItems.Get("URL");
                foreach (Item item in items)
                {
                    if (item.ParametersString.ToUpper().IndexOf("HOME") > -1)
                    {
                        if (value != null)
                        {
                            item.Value = value;
                        }
                        else
                        {
                            m_pItems.Remove(item);
                        }
                        return;
                    }
                }

                if (value != null)
                {
                    // If we reach here, URL;Work  doesn't exist, add it.
                    m_pItems.Add("URL", "HOME", value);
                }
            }
        }

        /// <summary>
        /// Gets reference to vCard items.
        /// </summary>
        public ItemCollection Items
        {
            get { return m_pItems; }
        }

        /// <summary>
        /// Gets or sets name info.  Returns null if N: item doesn't exist.
        /// </summary>
        public Name Name
        {
            get
            {
                Item item = m_pItems.GetFirst("N");
                if (item != null)
                {
                    return Name.Parse(item);
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    m_pItems.SetDecodedValue("N", value.ToValueString());
                }
                else
                {
                    m_pItems.SetDecodedValue("N", null);
                }
            }
        }

        /// <summary>
        /// Gets or sets nick name. Returns null if NICKNAME: item doesn't exist.
        /// </summary>
        public string NickName
        {
            get
            {
                Item item = m_pItems.GetFirst("NICKNAME");
                if (item != null)
                {
                    return item.DecodedValue;
                }
                else
                {
                    return null;
                }
            }

            set { m_pItems.SetDecodedValue("NICKNAME", value); }
        }

        /// <summary>
        /// Gets or sets note text. Returns null if NOTE: item doesn't exist.
        /// </summary>
        public string NoteText
        {
            get
            {
                Item item = m_pItems.GetFirst("NOTE");
                if (item != null)
                {
                    return item.DecodedValue;
                }
                else
                {
                    return null;
                }
            }

            set { m_pItems.SetDecodedValue("NOTE", value); }
        }

        /// <summary>
        /// Gets or sets organization name. Usually this value is: comapny;department;office. Returns null if ORG: item doesn't exist.
        /// </summary>
        public string Organization
        {
            get
            {
                Item item = m_pItems.GetFirst("ORG");
                if (item != null)
                {
                    return item.DecodedValue;
                }
                else
                {
                    return null;
                }
            }

            set { m_pItems.SetDecodedValue("ORG", value); }
        }

        /// <summary>
        /// Gets phone number collection.
        /// </summary>
        public PhoneNumberCollection PhoneNumbers
        {
            get
            {
                // Delay collection creation, create it when needed.
                if (m_pPhoneNumbers == null)
                {
                    m_pPhoneNumbers = new PhoneNumberCollection(this);
                }

                return m_pPhoneNumbers;
            }
        }

        /// <summary>
        /// Gets or sets person photo. Returns null if PHOTO: item doesn't exist.
        /// </summary>
        public Image Photo
        {
            get
            {
                Item item = m_pItems.GetFirst("PHOTO");
                if (item != null)
                {
                    return Image.FromStream(new MemoryStream(Encoding.Default.GetBytes(item.DecodedValue)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    MemoryStream ms = new MemoryStream();
                    value.Save(ms, ImageFormat.Jpeg);

                    m_pItems.SetValue("PHOTO", "ENCODING=b;TYPE=JPEG", Convert.ToBase64String(ms.ToArray()));
                }
                else
                {
                    m_pItems.SetValue("PHOTO", null);
                }
            }
        }

        /// <summary>
        /// Gets or sets role. Returns null if ROLE: item doesn't exist.
        /// </summary>
        public string Role
        {
            get
            {
                Item item = m_pItems.GetFirst("ROLE");
                if (item != null)
                {
                    return item.DecodedValue;
                }
                else
                {
                    return null;
                }
            }

            set { m_pItems.SetDecodedValue("ROLE", value); }
        }

        /// <summary>
        /// Gets or sets job title. Returns null if TITLE: item doesn't exist.
        /// </summary>
        public string Title
        {
            get
            {
                Item item = m_pItems.GetFirst("TITLE");
                if (item != null)
                {
                    return item.DecodedValue;
                }
                else
                {
                    return null;
                }
            }

            set { m_pItems.SetDecodedValue("TITLE", value); }
        }

        /// <summary>
        /// Gets or sets vCard unique ID. Returns null if UID: item doesn't exist.
        /// </summary>
        public string UID
        {
            get
            {
                Item item = m_pItems.GetFirst("UID");
                if (item != null)
                {
                    return item.DecodedValue;
                }
                else
                {
                    return null;
                }
            }

            set { m_pItems.SetDecodedValue("UID", value); }
        }

        /// <summary>
        /// Gets or sets vCard version. Returns null if VERSION: item doesn't exist.
        /// </summary>
        public string Version
        {
            get
            {
                Item item = m_pItems.GetFirst("VERSION");
                if (item != null)
                {
                    return item.DecodedValue;
                }
                else
                {
                    return null;
                }
            }

            set { m_pItems.SetDecodedValue("VERSION", value); }
        }

        /// <summary>
        /// Gets or sets vCard Work URL.
        /// </summary>
        public string WorkURL
        {
            get
            {
                Item[] items = m_pItems.Get("URL");
                foreach (Item item in items)
                {
                    if (item.ParametersString.ToUpper().IndexOf("WORK") > -1)
                    {
                        return item.DecodedValue;
                    }
                }

                return null;
            }

            set
            {
                Item[] items = m_pItems.Get("URL");
                foreach (Item item in items)
                {
                    if (item.ParametersString.ToUpper().IndexOf("WORK") > -1)
                    {
                        if (value != null)
                        {
                            item.Value = value;
                        }
                        else
                        {
                            m_pItems.Remove(item);
                        }
                        return;
                    }
                }

                if (value != null)
                {
                    // If we reach here, URL;Work  doesn't exist, add it.
                    m_pItems.Add("URL", "WORK", value);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Stores vCard structure to byte[].
        /// </summary>
        /// <returns></returns>
        public byte[] ToByte()
        {
            MemoryStream ms = new MemoryStream();
            ToStream(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Stores vCard to the specified file.
        /// </summary>
        /// <param name="file">File name with path where to store vCard.</param>
        public void ToFile(string file)
        {
            using (FileStream fs = File.Create(file))
            {
                ToStream(fs);
            }
        }

        /// <summary>
        /// Stores vCard structure to the specified stream.
        /// </summary>
        /// <param name="stream">Stream where to store vCard structure.</param>
        public void ToStream(Stream stream)
        {
            /* 
                BEGIN:VCARD<CRLF>
                ....
                END:VCARD<CRLF>
            */

            StringBuilder retVal = new StringBuilder();
            retVal.Append("BEGIN:VCARD\r\n");
            foreach (Item item in m_pItems)
            {
                retVal.Append(item.ToItemString() + "\r\n");
            }
            retVal.Append("END:VCARD\r\n");

            byte[] data = Encoding.UTF8.GetBytes(retVal.ToString());
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Parses vCard from the specified file.
        /// </summary>
        /// <param name="file">vCard file with path.</param>
        public void Parse(string file)
        {
            using (FileStream fs = File.OpenRead(file))
            {
                Parse(fs);
            }
        }

        /// <summary>
        /// Parses vCard from the specified stream.
        /// </summary>
        /// <param name="stream">Stream what contains vCard.</param>
        public void Parse(Stream stream)
        {
            m_pItems.Clear();
            m_pPhoneNumbers = null;
            m_pEmailAddresses = null;

            TextReader r = new StreamReader(stream, Encoding.Default);
            string line = r.ReadLine();
            // Find row BEGIN:VCARD
            while (line != null && line.ToUpper() != "BEGIN:VCARD")
            {
                line = r.ReadLine();
            }
            // Read frist vCard line after BEGIN:VCARD
            line = r.ReadLine();
            while (line != null && line.ToUpper() != "END:VCARD")
            {
                StringBuilder item = new StringBuilder();
                item.Append(line);
                // Get next line, see if item continues (folded line).
                line = r.ReadLine();
                while (line != null && (line.StartsWith("\t") || line.StartsWith(" ")))
                {
                    item.Append(line.Substring(1));
                    line = r.ReadLine();
                }

                string[] name_value = item.ToString().Split(new[] {':'}, 2);

                // Item syntax: name[*(;parameter)]:value
                string[] name_params = name_value[0].Split(new[] {';'}, 2);
                string name = name_params[0];
                string parameters = "";
                if (name_params.Length == 2)
                {
                    parameters = name_params[1];
                }
                string value = "";
                if (name_value.Length == 2)
                {
                    value = name_value[1];
                }
                m_pItems.Add(name, parameters, value);
            }
        }

        #endregion
    }
}