/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

    /*
	 * <x xmlns='jabber:x:data'
		type='{form-type}'>
		<title/>
		<instructions/>
		<field var='field-name'
				type='{field-type}'
				label='description'>
			<desc/>
			<required/>
			<value>field-value</value>
			<option label='option-label'><value>option-value</value></option>
			<option label='option-label'><value>option-value</value></option>
		</field>
		</x>
	*/

    /// <summary>
    /// </summary>
    public class Field : Element
    {
        #region Constructor

        /// <summary>
        /// </summary>
        public Field()
        {
            TagName = "field";
            Namespace = Uri.X_DATA;
        }

        /// <summary>
        /// </summary>
        /// <param name="type"> </param>
        public Field(FieldType type) : this()
        {
            Type = type;
        }

        /// <summary>
        /// </summary>
        /// <param name="var"> </param>
        /// <param name="label"> </param>
        /// <param name="type"> </param>
        public Field(string var, string label, FieldType type) : this()
        {
            Type = type;
            Var = var;
            Label = label;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public string Description
        {
            get { return GetTag("desc"); }

            set { SetTag("desc", value); }
        }

        /// <summary>
        ///   Is this field a required field?
        /// </summary>
        public bool IsRequired
        {
            get { return HasTag("required"); }

            set
            {
                if (value)
                {
                    SetTag("required");
                }
                else
                {
                    RemoveTag("required");
                }
            }
        }

        /// <summary>
        /// </summary>
        public string Label
        {
            get { return GetAttribute("label"); }

            set { SetAttribute("label", value); }
        }

        /// <summary>
        /// </summary>
        public FieldType Type
        {
            get
            {
                switch (GetAttribute("type"))
                {
                    case "boolean":
                        return FieldType.Boolean;
                    case "fixed":
                        return FieldType.Fixed;
                    case "hidden":
                        return FieldType.Hidden;
                    case "jid-multi":
                        return FieldType.Jid_Multi;
                    case "jid-single":
                        return FieldType.Jid_Single;
                    case "list-multi":
                        return FieldType.List_Multi;
                    case "list-single":
                        return FieldType.List_Single;
                    case "text-multi":
                        return FieldType.Text_Multi;
                    case "text-private":
                        return FieldType.Text_Private;
                    case "text-single":
                        return FieldType.Text_Single;
                    default:
                        return FieldType.Unknown;
                }
            }

            set
            {
                switch (value)
                {
                    case FieldType.Boolean:
                        SetAttribute("type", "boolean");
                        break;
                    case FieldType.Fixed:
                        SetAttribute("type", "fixed");
                        break;
                    case FieldType.Hidden:
                        SetAttribute("type", "hidden");
                        break;
                    case FieldType.Jid_Multi:
                        SetAttribute("type", "jid-multi");
                        break;
                    case FieldType.Jid_Single:
                        SetAttribute("type", "jid-single");
                        break;
                    case FieldType.List_Multi:
                        SetAttribute("type", "list-multi");
                        break;
                    case FieldType.List_Single:
                        SetAttribute("type", "list-single");
                        break;
                    case FieldType.Text_Multi:
                        SetAttribute("type", "text-multi");
                        break;
                    case FieldType.Text_Private:
                        SetAttribute("type", "text-private");
                        break;
                    case FieldType.Text_Single:
                        SetAttribute("type", "text-single");
                        break;
                    default:
                        RemoveAttribute("type");
                        break;
                }
            }
        }

        /// <summary>
        /// </summary>
        public string Var
        {
            get { return GetAttribute("var"); }

            set { SetAttribute("var", value); }
        }

        #endregion

        #region Methods

        public string FieldValue
        {
            set { SetValue(value); }
            get { return GetValue(); }
        }

        /// <summary>
        ///   The value of the field.
        /// </summary>
        /// <returns> </returns>
        public string GetValue()
        {
            return GetTag(typeof (Value));

            // return GetTag("value");			
        }

        /// <summary>
        /// </summary>
        /// <param name="val"> </param>
        /// <returns> </returns>
        public bool HasValue(string val)
        {
            foreach (string s in GetValues())
            {
                if (s == val)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="val"> </param>
        public void SetValue(string val)
        {
            SetTag(typeof (Value), val);
        }

        /// <summary>
        ///   Set the value of boolean fields
        /// </summary>
        /// <param name="val"> </param>
        public void SetValueBool(bool val)
        {
            SetValue(val ? "1" : "0");
        }

        /// <summary>
        ///   Get the value of boolean fields
        /// </summary>
        /// <returns> </returns>
        public bool GetValueBool()
        {
            // only "0" and "1" are valid. We dont care about other buggy implementations
            string val = GetValue();
            if (val == null || val == "0")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        ///   Returns the value as Jif for the Jid fields. Or null when the value is not a valid Jid.
        /// </summary>
        /// <returns> </returns>
        public Jid GetValueJid()
        {
            try
            {
                return new Jid(GetValue());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///   Adds a value
        /// </summary>
        /// <remarks>
        ///   you can call this function multiple times to add values to "multi" fields
        /// </remarks>
        /// <param name="val"> </param>
        public void AddValue(string val)
        {
            AddChild(new Value(val));

            // AddTag("value", val);
        }

        /// <summary>
        ///   Adds multiple values to the already existing values from a string array
        /// </summary>
        /// <param name="vals"> </param>
        public void AddValues(string[] vals)
        {
            if (vals.Length > 0)
            {
                foreach (string s in vals)
                {
                    AddValue(s);
                }
            }
        }

        /// <summary>
        ///   Adds multiple values. All already existing values will be removed
        /// </summary>
        /// <param name="vals"> </param>
        public void SetValues(string[] vals)
        {
            ElementList nl = SelectElements(typeof (Value));

            foreach (Element e in nl)
            {
                e.Remove();
            }

            AddValues(vals);
        }

        /// <summary>
        ///   Gets all values for multi fields as Array
        /// </summary>
        /// <returns> string Array that contains all the values </returns>
        public string[] GetValues()
        {
            ElementList nl = SelectElements(typeof (Value));
            var values = new string[nl.Count];
            int i = 0;
            foreach (Element val in nl)
            {
                values[i] = val.Value;
                i++;
            }

            return values;
        }

        /// <summary>
        /// </summary>
        /// <param name="label"> </param>
        /// <param name="val"> </param>
        /// <returns> </returns>
        public Option AddOption(string label, string val)
        {
            var opt = new Option(label, val);
            AddChild(opt);
            return opt;
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public Option AddOption()
        {
            var opt = new Option();
            AddChild(opt);
            return opt;
        }

        /// <summary>
        /// </summary>
        /// <param name="opt"> </param>
        public void AddOption(Option opt)
        {
            AddChild(opt);
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public Option[] GetOptions()
        {
            ElementList nl = SelectElements(typeof (Option));
            int i = 0;
            var result = new Option[nl.Count];
            foreach (Option o in nl)
            {
                result[i] = o;
                i++;
            }

            return result;
        }

        #endregion
    }
}