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


using XmppData = ASC.Xmpp.Core.protocol.x.data;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ASC.Xmpp.Server.Utils
{
    public class XDataFromsExample
	{
		[XDataUtils.XDataAnyOf("any", "all", "one of")]
		public string[] many { get; set; }

		[XDataUtils.XDataOneOf("any", "all", "one of")]
		public string oneof { get; set; }

		[XDataUtils.XDataFixed]
		public string data1 { get; set; }

		[XDataUtils.XDataMultiline]
		public string data2 { get; set; }

		[XDataUtils.XDataPassword]
		public string data3 { get; set; }

		[XDataUtils.XDataDescription("Label test")]
		public string data4 { get; set; }

		public XDataFromsExample()
		{
			many = new string[] { "any" };
			oneof = "all";
			data1 = "fixed!";
		}

	}



	public class XDataUtils
	{
		public class XDataDescriptionAttribute : Attribute
		{
			public string Description { get; set; }

			public XDataDescriptionAttribute(string description)
			{
				Description = description;
			}
		}

		public class XDataOneOfAttribute : Attribute
		{
			public string[] Variants { get; set; }

			public XDataOneOfAttribute(params string[] variants)
			{
				Variants = variants;
			}
		}

		public class XDataAnyOfAttribute : Attribute
		{
			public string[] Variants { get; set; }

			public XDataAnyOfAttribute(params string[] variants)
			{
				Variants = variants;
			}
		}

		public class XDataMultiline : Attribute
		{
		}

		public class XDataFixed : Attribute
		{
		}

		public class XDataPassword : Attribute
		{
		}

        public static void FillDataTo(object dataForm, string prefix, XmppData.Data data)
		{
            if (data.Type == XmppData.XDataFormType.submit)
			{
				//Gen prop map
				PropertyInfo[] props =
					dataForm.GetType().GetProperties(BindingFlags.Instance | BindingFlags.SetProperty |
													 BindingFlags.Public);
				Dictionary<string, PropertyInfo> propsVar = new Dictionary<string, PropertyInfo>();
				foreach (PropertyInfo prop in props)
				{
					if (prop.CanWrite)
					{
						propsVar.Add(string.Format("{0}#{1}", prefix, prop.Name), prop);
					}
				}

                XmppData.Field[] fields = data.GetFields();
				foreach (var field in fields)
				{
					if (propsVar.ContainsKey(field.Var))
					{
						PropertyInfo prop = propsVar[field.Var];
						if (prop.PropertyType == typeof(bool))
						{
							string val = field.GetValue();
							if (!string.IsNullOrEmpty(val))
							{
								prop.SetValue(dataForm, val == "1", null);
							}
						}
						else if (prop.PropertyType == typeof(string))
						{
							string val = field.GetValue();
							if (!string.IsNullOrEmpty(val))
							{
								prop.SetValue(dataForm, val, null);
							}
						}
						else if (prop.PropertyType == typeof(string[]))
						{
							string[] val = field.GetValues();
							if (val != null)
							{
								prop.SetValue(dataForm, val, null);
							}
						}

					}
				}
			}
		}

        public static XmppData.Data GetDataForm(object dataForm, string prefix)
		{
            XmppData.Data data = new XmppData.Data(XmppData.XDataFormType.form);

			//Go through public vars
			PropertyInfo[] props = dataForm.GetType().GetProperties(BindingFlags.Instance | BindingFlags.SetProperty |
											 BindingFlags.Public);
			foreach (PropertyInfo prop in props)
			{
				if (prop.CanRead)
				{
                    XmppData.Field field = new XmppData.Field(XmppData.FieldType.Unknown);

					field.Var = string.Format("{0}#{1}", prefix, prop.Name);
					object propValue = prop.GetValue(dataForm, null);

					foreach (var attribute in prop.GetCustomAttributes(false))
					{
						if (attribute is XDataDescriptionAttribute)
						{
							field.Label = (attribute as XDataDescriptionAttribute).Description;
						}
						else if (attribute is XDataOneOfAttribute)
						{
                            field.Type = XmppData.FieldType.List_Single;
							field.FieldValue = (string)propValue;
							foreach (var vars in (attribute as XDataOneOfAttribute).Variants)
							{
								field.AddOption(vars, vars);
							}
						}
						else if (attribute is XDataAnyOfAttribute)
						{
                            field.Type = XmppData.FieldType.List_Multi;
							field.AddValues((string[])propValue);
							foreach (var vars in (attribute as XDataAnyOfAttribute).Variants)
							{
								field.AddOption(vars, vars);
							}
						}
						else if (attribute is XDataMultiline)
						{
                            field.Type = XmppData.FieldType.Text_Multi;
							field.FieldValue = (string)propValue;
						}
						else if (attribute is XDataPassword)
						{
                            field.Type = XmppData.FieldType.Text_Private;
							field.FieldValue = (string)propValue;
						}
						else if (attribute is XDataFixed)
						{
                            field.Type = XmppData.FieldType.Fixed;
							field.FieldValue = (string)propValue;
						}
					}
                    if (field.Type == XmppData.FieldType.Unknown)
					{
						if (prop.PropertyType == typeof(bool))
						{
                            field.Type = XmppData.FieldType.Boolean;
							field.FieldValue = (bool)propValue ? "1" : "0";
						}
						else if (prop.PropertyType == typeof(string))
						{
                            field.Type = XmppData.FieldType.Text_Single;
							field.FieldValue = (string)propValue;
						}
					}
					if (field.Label == null)
					{
						field.Label = prop.Name;
					}
					data.AddField(field);
				}
			}

			return data;
		}
	}
}