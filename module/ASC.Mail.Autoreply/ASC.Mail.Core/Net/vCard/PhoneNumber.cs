/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    /// <summary>
    /// vCard phone number implementation.
    /// </summary>
    public class PhoneNumber
    {
        #region Members

        private readonly Item m_pItem;
        private string m_Number = "";
        private PhoneNumberType_enum m_Type = PhoneNumberType_enum.Voice;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="item">Owner vCard item.</param>
        /// <param name="type">Phone number type. Note: This value can be flagged value !</param>
        /// <param name="number">Phone number.</param>
        internal PhoneNumber(Item item, PhoneNumberType_enum type, string number)
        {
            m_pItem = item;
            m_Type = type;
            m_Number = number;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets underlaying vCrad item.
        /// </summary>
        public Item Item
        {
            get { return m_pItem; }
        }

        /// <summary>
        /// Gets or sets phone number.
        /// </summary>
        public string Number
        {
            get { return m_Number; }

            set
            {
                m_Number = value;
                Changed();
            }
        }

        /// <summary>
        /// Gets or sets phone number type. Note: This property can be flagged value !
        /// </summary>
        public PhoneNumberType_enum NumberType
        {
            get { return m_Type; }

            set
            {
                m_Type = value;
                Changed();
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses phone from vCard TEL structure string.
        /// </summary>
        /// <param name="item">vCard TEL item.</param>
        internal static PhoneNumber Parse(Item item)
        {
            PhoneNumberType_enum type = PhoneNumberType_enum.NotSpecified;
            if (item.ParametersString.ToUpper().IndexOf("PREF") != -1)
            {
                type |= PhoneNumberType_enum.Preferred;
            }
            if (item.ParametersString.ToUpper().IndexOf("HOME") != -1)
            {
                type |= PhoneNumberType_enum.Home;
            }
            if (item.ParametersString.ToUpper().IndexOf("MSG") != -1)
            {
                type |= PhoneNumberType_enum.Msg;
            }
            if (item.ParametersString.ToUpper().IndexOf("WORK") != -1)
            {
                type |= PhoneNumberType_enum.Work;
            }
            if (item.ParametersString.ToUpper().IndexOf("VOICE") != -1)
            {
                type |= PhoneNumberType_enum.Voice;
            }
            if (item.ParametersString.ToUpper().IndexOf("FAX") != -1)
            {
                type |= PhoneNumberType_enum.Fax;
            }
            if (item.ParametersString.ToUpper().IndexOf("CELL") != -1)
            {
                type |= PhoneNumberType_enum.Cellular;
            }
            if (item.ParametersString.ToUpper().IndexOf("VIDEO") != -1)
            {
                type |= PhoneNumberType_enum.Video;
            }
            if (item.ParametersString.ToUpper().IndexOf("PAGER") != -1)
            {
                type |= PhoneNumberType_enum.Pager;
            }
            if (item.ParametersString.ToUpper().IndexOf("BBS") != -1)
            {
                type |= PhoneNumberType_enum.BBS;
            }
            if (item.ParametersString.ToUpper().IndexOf("MODEM") != -1)
            {
                type |= PhoneNumberType_enum.Modem;
            }
            if (item.ParametersString.ToUpper().IndexOf("CAR") != -1)
            {
                type |= PhoneNumberType_enum.Car;
            }
            if (item.ParametersString.ToUpper().IndexOf("ISDN") != -1)
            {
                type |= PhoneNumberType_enum.ISDN;
            }
            if (item.ParametersString.ToUpper().IndexOf("PCS") != -1)
            {
                type |= PhoneNumberType_enum.PCS;
            }

            return new PhoneNumber(item, type, item.Value);
        }

        /// <summary>
        /// Converts PhoneNumberType_enum to vCard item parameters string.
        /// </summary>
        /// <param name="type">Value to convert.</param>
        /// <returns></returns>
        internal static string PhoneTypeToString(PhoneNumberType_enum type)
        {
            string retVal = "";
            if ((type & PhoneNumberType_enum.BBS) != 0)
            {
                retVal += "BBS,";
            }
            if ((type & PhoneNumberType_enum.Car) != 0)
            {
                retVal += "CAR,";
            }
            if ((type & PhoneNumberType_enum.Cellular) != 0)
            {
                retVal += "CELL,";
            }
            if ((type & PhoneNumberType_enum.Fax) != 0)
            {
                retVal += "FAX,";
            }
            if ((type & PhoneNumberType_enum.Home) != 0)
            {
                retVal += "HOME,";
            }
            if ((type & PhoneNumberType_enum.ISDN) != 0)
            {
                retVal += "ISDN,";
            }
            if ((type & PhoneNumberType_enum.Modem) != 0)
            {
                retVal += "MODEM,";
            }
            if ((type & PhoneNumberType_enum.Msg) != 0)
            {
                retVal += "MSG,";
            }
            if ((type & PhoneNumberType_enum.Pager) != 0)
            {
                retVal += "PAGER,";
            }
            if ((type & PhoneNumberType_enum.PCS) != 0)
            {
                retVal += "PCS,";
            }
            if ((type & PhoneNumberType_enum.Preferred) != 0)
            {
                retVal += "PREF,";
            }
            if ((type & PhoneNumberType_enum.Video) != 0)
            {
                retVal += "VIDEO,";
            }
            if ((type & PhoneNumberType_enum.Voice) != 0)
            {
                retVal += "VOICE,";
            }
            if ((type & PhoneNumberType_enum.Work) != 0)
            {
                retVal += "WORK,";
            }
            if (retVal.EndsWith(","))
            {
                retVal = retVal.Substring(0, retVal.Length - 1);
            }

            return retVal;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// This method is called when some property has changed, wee need to update underlaying vCard item.
        /// </summary>
        private void Changed()
        {
            m_pItem.ParametersString = PhoneTypeToString(m_Type);
            m_pItem.Value = m_Number;
        }

        #endregion
    }
}