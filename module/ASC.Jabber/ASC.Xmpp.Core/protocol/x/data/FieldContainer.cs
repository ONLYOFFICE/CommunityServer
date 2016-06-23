/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.x.data
{

    #region usings

    #endregion

    /// <summary>
    ///   Bass class for all xdata classes that contain xData fields
    /// </summary>
    public abstract class FieldContainer : Element
    {
        #region Methods

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public Field AddField()
        {
            var f = new Field();
            AddChild(f);
            return f;
        }

        /// <summary>
        /// </summary>
        /// <param name="field"> </param>
        public Field AddField(Field field)
        {
            AddChild(field);
            return field;
        }

        /// <summary>
        ///   Retrieve a field with the given "var"
        /// </summary>
        /// <param name="var"> </param>
        /// <returns> </returns>
        public Field GetField(string var)
        {
            ElementList nl = SelectElements(typeof (Field));
            foreach (Element e in nl)
            {
                var f = e as Field;
                if (f.Var == var)
                {
                    return f;
                }
            }

            return null;
        }

        /// <summary>
        ///   Gets a list of all form fields
        /// </summary>
        /// <returns> </returns>
        public Field[] GetFields()
        {
            ElementList nl = SelectElements(typeof (Field));
            var fields = new Field[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                fields[i] = (Field) e;
                i++;
            }

            return fields;
        }

        #endregion
    }
}