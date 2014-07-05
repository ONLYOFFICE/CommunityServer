/*
 * AjaxSettingsSectionHandler.cs
 * 
 * Copyright © 2007 Michael Schwarz (http://www.ajaxpro.info).
 * All Rights Reserved.
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without 
 * restriction, including without limitation the rights to use, 
 * copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
/*
 * MS	06-04-04	added DebugEnabled web.config property <debug enabled="true"/>
 * MS	06-04-05	added oldStyle section in web.config
 * MS	06-04-12	added urlNamespaceMappings/@useAssemblyQualifiedName
 * MS	06-04-25	using String.IsNullOrEmpty
 * MS	06-05-09	added all child nodes to oldStyle settings
 * MS	06-05-29	fixed wrong jsonConverters handling
 * MS	06-06-07	added urlNamespaceMappings/allowListOnly
 * MS	07-04-24	added new settings (oldStyle == configuration)
 *					added provider settings
 *					added includeTypeProperty
 * 
 * 
 */
using System;
using System.Xml;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
#if(NET20)
using System.Collections.Generic;
#endif

namespace AjaxPro
{
	internal class AjaxSettingsSectionHandler : IConfigurationSectionHandler
	{
		#region IConfigurationSectionHandler Members

        /// <summary>
        /// Creates a configuration section handler.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext">Configuration context object.</param>
        /// <param name="section"></param>
        /// <returns>The created section handler object.</returns>
		public object Create(object parent, object configContext, System.Xml.XmlNode section)
		{
			AjaxSettings settings = new AjaxSettings();
			Utility.AddDefaultConverter(settings);

			foreach(XmlNode n in section.ChildNodes)
			{
#if(!JSONLIB)
				if(n.Name == "coreScript")
				{
					if(n.InnerText != null && n.InnerText.Length > 0) 
					{
						settings.ScriptReplacements.Add("core", n.InnerText);
					}
				}
				else if(n.Name == "scriptReplacements")
				{
					foreach(XmlNode file in n.SelectNodes("file"))
					{
						string name = "";
						string path = "";

						if(file.Attributes["name"] != null)
						{
							name = file.Attributes["name"].InnerText;
							if(file.Attributes["path"] != null) path = file.Attributes["path"].InnerText;

							if(settings.ScriptReplacements.ContainsKey(name))
								settings.ScriptReplacements[name] = path;
							else
								settings.ScriptReplacements.Add(name, path);
						}
					}
				}
				else if(n.Name == "urlNamespaceMappings")
				{
					settings.OnlyAllowTypesInList = n.SelectSingleNode("@allowListOnly[.='true']") != null;
					settings.UseAssemblyQualifiedName = n.SelectSingleNode("@useAssemblyQualifiedName[.='true']") != null;

					XmlNode ns, url;

					foreach(XmlNode e in n.SelectNodes("add"))
					{
						ns = e.SelectSingleNode("@type");
						url = e.SelectSingleNode("@path");
#if(NET20)
						if(ns == null || String.IsNullOrEmpty(ns.InnerText) || url == null || String.IsNullOrEmpty(url.InnerText))
#else
						if(ns == null || ns.InnerText.Length == 0 || url == null || url.InnerText.Length == 0)
#endif
							continue;

						if(settings.UrlNamespaceMappings.Contains(url.InnerText))
							throw new Exception("Duplicate namespace mapping '" + url.InnerText + "'.");

						settings.UrlNamespaceMappings.Add(url.InnerText, ns.InnerText);
					}
				}
				else if (n.Name == "providers" || n.Name == "provider")
				{
					foreach (XmlNode p in n.ChildNodes)
					{
						if (p.Name == "securityProvider")
						{
							if (p.SelectSingleNode("@type") != null)
							{
								string securityProviderType = p.SelectSingleNode("@type").InnerText;

								AjaxSecurity sec = new AjaxSecurity(securityProviderType);

								if (sec.Init())
								{
									settings.Security = sec;
								}
							}
						}
						else if (p.Name == "typeJavaScriptProvider")
						{
							if (p.SelectSingleNode("@type") != null)
							{
								settings.TypeJavaScriptProvider = p.SelectSingleNode("@type").InnerText;
							}
						}
					}
				}
				else if (n.Name == "token")
				{
					// settings.TokenEnabled = n.SelectSingleNode("@enabled") != null && n.SelectSingleNode("@enabled").InnerText == "true";
					settings.TokenSitePassword = n.SelectSingleNode("@sitePassword") != null ? n.SelectSingleNode("@sitePassword").InnerText : settings.TokenSitePassword;
				}
				else if (n.Name == "debug")
				{
					if (n.SelectSingleNode("@enabled") != null && n.SelectSingleNode("@enabled").InnerText == "true")
						settings.DebugEnabled = true;
				}
				else if (n.Name == "oldStyle" || n.Name == "configuration")
				{
					foreach (XmlNode sn in n.ChildNodes)
					{
						switch (sn.Name)
						{
							case "useSimpleObjectNaming":
								settings.UseSimpleObjectNaming = true;
								break;

							default:
								settings.OldStyle.Add(sn.Name);
								break;
						}
					}
				}
				else
#endif
					if (n.Name == "jsonConverters")
					{
						if (n.SelectSingleNode("@includeTypeProperty") != null && n.SelectSingleNode("@includeTypeProperty").InnerText == "true")
						{
							settings.IncludeTypeProperty = true;
						}

						XmlNodeList jsonConverters = n.SelectNodes("add");

						foreach (XmlNode j in jsonConverters)
						{
							XmlNode t = j.SelectSingleNode("@type");

							if (t == null)
								continue;

							Type type = Type.GetType(t.InnerText);

							if (type == null)
							{
								// throw new ArgumentException("Could not find type " + t.InnerText + ".");
								continue;
							}

							if (!typeof(IJavaScriptConverter).IsAssignableFrom(type))
							{
								// throw new ArgumentException("Type " + t.InnerText + " does not inherit from JavaScriptObjectConverter.");
								continue;
							}

							StringDictionary d = new StringDictionary();
							foreach (XmlAttribute a in j.Attributes)
							{
								if (d.ContainsKey(a.Name)) continue;
								d.Add(a.Name, a.Value);
							}

							IJavaScriptConverter c = (IJavaScriptConverter)Activator.CreateInstance(type);
							c.Initialize(d);

							Utility.AddConverter(settings, c, true);
						}


						jsonConverters = n.SelectNodes("remove");

						foreach (XmlNode j in jsonConverters)
						{
							XmlNode t = j.SelectSingleNode("@type");

							if (t == null)
								continue;

							Type type = Type.GetType(t.InnerText);

							if (type == null)
							{
								// throw new ArgumentException("Could not find type " + t.InnerText + ".");
								continue;
							}

							Utility.RemoveConverter(settings, type);
						}
					}
			}

			return settings;
		}

		#endregion
	}
}
