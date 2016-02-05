/*
 * Utility.cs
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
 * 
 * MS	05-12-20	changed AjaxSettins access, now thread safe for web farms
 * MS	06-04-05	fixed sessionID on ASP.NET 2.0, new static method GetSessionUri
 * MS	06-04-12	added useAssemblyQualifiedName
 * MS	06-04-25	fixed forms authentication cookieless configuration
 * MS	06-04-26	added ProfileBaseConverter for ASP.NET 2.0
 * MS	06-04-27	fixed RegisterClientScriptBlock memory leak because complete page object 
 *					has been used as key (see Google group: [ajaxpro] Huge Memory Leak in AJAX.NET Pro)
 *					fixed page_PreRender calls, event has been added several times
 * MS	06-04-29	added new property AjaxID which will map to Constant.AjaxID
 * MS	06-05-09	fixed missing page.PreRender event (see 06-04-27) if Server.Transfer
 *					if NET20 and type,App_Code.xyz use shortcut type,App_Code without ".xyz", can be
 *					disabled with oldStyle/appCodeQualifiedFullName, see web.config
 * MS	06-05-17	added old HashtableConverter, IDictionaryConverter does not support Hashtable any more
 * MS	06-05-30	changed to new converter dictionary
 *					added ms.ashx to the common JavaScript includes
 *					removed HtmlControlConverter from default
 * MS	06-06-07	removed Obsolete(true) for RegisterConverterForAjax
 * MS	06-06-09	fixed check if converter is already in list
 * MS	06-09-15	fixed IDictionary bug, wrong sequence for converters
 * MS	07-04-24	fixed Ajax token
 *					using new AjaxSecurityProvider
 * MS   09-02-17    fixed memory problem
 * 
 * 
 * 
 */
using System;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.Caching;

namespace AjaxPro
{
    /// <summary>
    /// Provides methods to register Ajax methods.
    /// </summary>
    public sealed class Utility
    {
#if(!JSONLIB)
        /// <summary>
        /// Set the HandlerExtension configured in web.config/configuration/system.web/httpHandlers/add/@path:
        /// </summary>
        public static string HandlerExtension = ".ashx";

        /// <summary>
        /// Set the HandlerPath configured in web.config/configuration/system.web/httpHandlers/add/@path:
        /// </summary>
        public static string HandlerPath = "ajaxpro";
#endif
        private static AjaxSettings m_Settings = null;
        private static object m_SettingsLock = new object();
        internal static bool ConverterRegistered = false;

        /// <summary>
        /// Gets the ajax ID.
        /// </summary>
        /// <value>The ajax ID.</value>
        public static string AjaxID
        {
            get
            {
                return Constant.AjaxID;
            }
        }
#if(!JSONLIB)
        /// <summary>
        /// Returns the session identifier.
        /// </summary>
        /// <returns>
        /// Returns the URL part for the session identifier.
        /// </returns>
        internal static string GetSessionUri()
        {
            string cookieUri = "";

            if ((System.Web.HttpContext.Current.Session != null && System.Web.HttpContext.Current.Session.IsCookieless)
#if(NET20)
 || (!System.Web.Security.FormsAuthentication.CookiesSupported && System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
#endif
)
            {
#if(NET20)
                cookieUri = "(" + System.Web.HttpContext.Current.Request.ServerVariables["HTTP_ASPFILTERSESSIONID"] + ")";
#else
				cookieUri = "(" + System.Web.HttpContext.Current.Session.SessionID + ")";
#endif
            }

            if (cookieUri != null && cookieUri.Length != 0)
                cookieUri += "/";

            return cookieUri;
        }

        /// <summary>
        /// Returns the name of the class and method the AjaxMethod will be rendered to the client-side JavaScript.
        /// </summary>
        /// <param name="method">The method you want to call.</param>
        /// <returns>
        /// Returns a string separated by a comma, i.e. "MyNamespace.MyClass,MyMethod"
        /// </returns>
        [Obsolete("The recommended alternative is AjaxPro.ClientMethod.FromMethodInfo.", false)]
        public static string GetClientMethod(MethodInfo method)
        {
            ClientMethod cm = ClientMethod.FromMethodInfo(method);

            if (cm == null)
                return null;

            return cm.ClassName + "," + cm.MethodName;
        }

        /// <summary>
        /// Writes an enum representation to the current page.
        /// </summary>
        /// <param name="type">The type of the enum.</param>
        public static void RegisterEnumForAjax(Type type)
        {
            System.Web.UI.Page page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;

            RegisterEnumForAjax(type, page);
        }

        /// <summary>
        /// Writes an enum representation to the current page.
        /// </summary>
        /// <param name="type">The type of the enum.</param>
        /// <param name="page">The page where the JavaScript shoult be rendered in.</param>
        public static void RegisterEnumForAjax(Type type, System.Web.UI.Page page)
        {
            RegisterCommonAjax(page);

            RegisterClientScriptBlock(page, Constant.AjaxID + ".AjaxEnum." + type.FullName,
                "<script type=\"text/javascript\">\r\n" + JavaScriptUtil.GetEnumRepresentation(type) + "\r\n</script>");
        }


        /// <summary>
        /// Register the specified type (class) for the current page. This will also add the common JavaScript file.
        /// </summary>
        /// <param name="type">The tpye to register i.e. RegisterTypeForAjax(typeof(WebApplication1.WebForm1));</param>
        public static void RegisterTypeForAjax(Type type)
        {
            System.Web.UI.Page page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;

            RegisterTypeForAjax(type, page);
        }

        /// <summary>
        /// Register the specified type (class) for the current page. This will also add the common JavaScript file.
        /// </summary>
        /// <param name="type">The tpye to register i.e. RegisterTypeForAjax(typeof(WebApplication1.WebForm1));</param>
        /// <param name="page">The Page the script should rendered on.</param>
        public static void RegisterTypeForAjax(Type type, System.Web.UI.Page page)
        {
            //RegisterCommonAjax(page);
            var data = ProcessRequest(HttpContext.Current, type);
            RegisterClientScriptBlock(page, "AjaxType." + type.FullName, data);
        }
#endif
        /// <summary>
        /// Register the specified converter to be used with Ajax.NET.
        /// </summary>
        /// <param name="converter">The IJavaScriptConverter.</param>
        [Obsolete("The recommended alternative is to add the converter type to ajaxNet/ajaxSettings/jsonConverters.", false)]
        public static void RegisterConverterForAjax(IJavaScriptConverter converter)
        {
            Utility.AddConverter(Utility.Settings, converter);
        }

        #region Internal Members

        /// <summary>
        /// Adds the default converter.
        /// </summary>
        /// <param name="settings">The settings.</param>
        internal static void AddDefaultConverter(AjaxSettings settings)
        {
            #region Default Converters

            AddConverter(settings, new StringConverter());
            AddConverter(settings, new PrimitiveConverter());
            AddConverter(settings, new GuidConverter());
            AddConverter(settings, new ExceptionConverter());
            AddConverter(settings, new EnumConverter());
            AddConverter(settings, new DecimalConverter());

            AddConverter(settings, new NameValueCollectionConverter());

            AddConverter(settings, new DateTimeConverter());

            AddConverter(settings, new DataSetConverter());
            AddConverter(settings, new DataTableConverter());
            AddConverter(settings, new DataViewConverter());

            AddConverter(settings, new IJavaScriptObjectConverter());

#if(NET20)
            AddConverter(settings, new ProfileBaseConverter());
#endif

            AddConverter(settings, new IDictionaryConverter());
            AddConverter(settings, new IListConverter());
            AddConverter(settings, new HashtableConverter());
            AddConverter(settings, new IEnumerableConverter());

            AddConverter(settings, new DataRowConverter());
            AddConverter(settings, new HtmlControlConverter());

            #endregion
        }

        /// <summary>
        /// Removes the converter.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="t">The t.</param>
        internal static void RemoveConverter(AjaxSettings settings, Type t)
        {
            Type key;
            bool removed = false;
            IEnumerator m = settings.SerializableConverters.Keys.GetEnumerator();

            while (!removed && m.MoveNext())
            {
                key = (Type)m.Current;
                if (settings.SerializableConverters[key].GetType() == t)
                {
                    settings.SerializableConverters.Remove(key);
                    removed = true;
                }
            }

            removed = false;
            m = settings.DeserializableConverters.Keys.GetEnumerator();

            while (!removed && m.MoveNext())
            {
                key = (Type)m.Current;
                if (settings.DeserializableConverters[key].GetType() == t)
                {
                    settings.DeserializableConverters.Remove(key);
                    removed = true;
                }
            }
        }

        /// <summary>
        /// Adds the converter.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="converter">The converter.</param>
        internal static void AddConverter(AjaxSettings settings, IJavaScriptConverter converter)
        {
            AddConverter(settings, converter, false);
        }

        /// <summary>
        /// Adds the converter.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="converter">The converter.</param>
        /// <param name="replace">if set to <c>true</c> [replace].</param>
        internal static void AddConverter(AjaxSettings settings, IJavaScriptConverter converter, bool replace)
        {
            Type t;

            for (int i = 0; i < converter.SerializableTypes.Length; i++)
            {
                t = converter.SerializableTypes[i];

                if (settings.SerializableConverters.ContainsKey(t))
                {
                    if (replace)
                        settings.SerializableConverters[t] = converter;
                    continue;
                }

                settings.SerializableConverters.Add(t, converter);
            }

            for (int i = 0; i < converter.DeserializableTypes.Length; i++)
            {
                t = converter.DeserializableTypes[i];

                if (settings.DeserializableConverters.ContainsKey(t))
                {
                    if (replace)
                        settings.DeserializableConverters[t] = converter;
                    continue;
                }

                settings.DeserializableConverters.Add(t, converter);
            }
        }

        /// <summary>
        /// Get the settings configured in web.config.
        /// </summary>
        /// <value>The settings.</value>
        internal static AjaxSettings Settings
        {
            get
            {
                if (m_Settings != null)
                    return m_Settings;

                lock (m_SettingsLock)
                {
                    if (m_Settings != null)
                        return m_Settings;      // Ok, one other thread has already initialized this value.

                    AjaxSettings settings = null;

                    try
                    {
#if(NET20)
                        settings = (AjaxSettings)System.Configuration.ConfigurationManager.GetSection("ajaxNet/ajaxSettings");
#else
						settings = (AjaxSettings)System.Configuration.ConfigurationSettings.GetConfig("ajaxNet/ajaxSettings");
#endif

                    }
#if(NET20)
                    catch (System.Configuration.ConfigurationErrorsException)
#else
					catch (Exception)
#endif
                    { }

                    if (settings == null)
                    {
                        settings = new AjaxSettings();
                        AddDefaultConverter(settings);
                    }

                    // now make the setting visible to all threads
                    m_Settings = settings;

                    return m_Settings;
                }
            }
        }
#if(!JSONLIB)
        /// <summary>
        /// Gets the current ajax token.
        /// </summary>
        /// <value>The current ajax token.</value>
        internal static string CurrentAjaxToken
        {
            get
            {
                if (Utility.Settings == null || Utility.Settings.Security == null || Utility.Settings.Security.SecurityProvider == null)
                    return null;

                string token = null;

                try
                {
                    token = Utility.Settings.Security.SecurityProvider.GetAjaxToken(Utility.Settings.TokenSitePassword);
                }
                catch (Exception)
                {
                }

                return token;
            }
        }

        /// <summary>
        /// Register the common JavaScript to the current handler.
        /// </summary>
        internal static void RegisterCommonAjax()
        {
            RegisterCommonAjax((System.Web.UI.Page)System.Web.HttpContext.Current.Handler);
        }

        /// <summary>
        /// Register the common JavaScript file for the specified page.
        /// </summary>
        /// <param name="page">The Page the client script should be rendered to.</param>
        internal static void RegisterCommonAjax(System.Web.UI.Page page)
        {
            if (page == null)
                return;

            // If there is a configuration for this fileName in
            // web.config AjaxPro section scriptReplacements we will
            // redirect to this file.

            string rootFolder = System.Web.HttpContext.Current.Request.ApplicationPath + (System.Web.HttpContext.Current.Request.ApplicationPath.EndsWith("/") ? "" : "/");

            string prototypeJs = rootFolder + Utility.HandlerPath + "/" + Utility.GetSessionUri() + "prototype" + Utility.HandlerExtension;
            string coreJs = rootFolder + Utility.HandlerPath + "/" + Utility.GetSessionUri() + "core" + Utility.HandlerExtension;
            string corePrototypeConverterJs = rootFolder + Utility.HandlerPath + "/" + Utility.GetSessionUri() + "core-prototype-converter" + Utility.HandlerExtension;
            string convertersJs = rootFolder + Utility.HandlerPath + "/" + Utility.GetSessionUri() + "converter" + Utility.HandlerExtension;

            if (Utility.Settings != null)
            {
                if (Utility.Settings.ScriptReplacements.ContainsKey("prototype"))
                {
                    prototypeJs = Utility.Settings.ScriptReplacements["prototype"];
                    if (prototypeJs.Length > 0 && prototypeJs.StartsWith("~/"))
                        prototypeJs = rootFolder + prototypeJs.Substring(2);
                }
                if (Utility.Settings.ScriptReplacements.ContainsKey("core"))
                {
                    coreJs = Utility.Settings.ScriptReplacements["core"];
                    if (coreJs.Length > 0 && coreJs.StartsWith("~/"))
                        coreJs = rootFolder + coreJs.Substring(2);
                }

                if (Utility.Settings.ScriptReplacements.ContainsKey("converter"))
                {
                    convertersJs = Utility.Settings.ScriptReplacements["converter"];
                    if (convertersJs.Length > 0 && convertersJs.StartsWith("~/"))
                        convertersJs = rootFolder + convertersJs.Substring(2);
                }
            }

            if (prototypeJs.Length > 0 && coreJs.Length > 0 && convertersJs.Length > 0)
            {
                RegisterClientScriptBlock(page, Constant.AjaxID + ".core-prototype-converter",
                                          "<script type=\"text/javascript\" src=\"" + corePrototypeConverterJs + "\"></script>");
            }
            else
            {
                if (prototypeJs.Length > 0)
                    RegisterClientScriptBlock(page, Constant.AjaxID + ".prototype",
                        "<script type=\"text/javascript\" src=\"" + prototypeJs + "\"></script>");

                if (coreJs.Length > 0)
                    RegisterClientScriptBlock(page, Constant.AjaxID + ".core",
                        "<script type=\"text/javascript\" src=\"" + coreJs + "\"></script>");

                if (convertersJs.Length > 0)
                    RegisterClientScriptBlock(page, Constant.AjaxID + ".converters",
                        "<script type=\"text/javascript\" src=\"" + convertersJs + "\"></script>");
            }



            if (Utility.Settings.OldStyle.Contains("includeMsPrototype"))
            {
                string msJs = rootFolder + Utility.HandlerPath + "/" + Utility.GetSessionUri() + "ms" + Utility.HandlerExtension;

                RegisterClientScriptBlock(page, Constant.AjaxID + ".ms",
                    "<script type=\"text/javascript\" src=\"" + msJs + "\"></script>");
            }

            StringBuilder sb = new StringBuilder();

            if (Settings.Security != null && Settings.Security.SecurityProvider != null && Settings.Security.SecurityProvider.AjaxTokenEnabled)
                sb.Append("AjaxPro.token = \"" + CurrentAjaxToken + "\";\r\n");

            if (Settings.OldStyle.Contains("noUtcTime") && coreJs.Length > 0)
                sb.Append("if(typeof AjaxPro != \"undefined\") AjaxPro.noUtcTime = true;\r\n");

            if (sb.Length > 0)
            {
                RegisterClientScriptBlock(page, Constant.AjaxID + ".ajaxproinit",
                    "<script type=\"text/javascript\">\r\n" + sb.ToString() + "</script>\r\n");
            }
        }

        /// <summary>
        /// Gets the scripts.
        /// </summary>
        /// <returns></returns>
        internal static ListDictionary GetScripts()
        {
            ListDictionary scripts = null;

            if (System.Web.HttpContext.Current.Items.Contains(Constant.AjaxID + ".pagescripts"))
            {
                scripts = System.Web.HttpContext.Current.Items[Constant.AjaxID + ".pagescripts"] as ListDictionary;
            }

            if (scripts == null)
            {
                scripts = new ListDictionary();
                System.Web.HttpContext.Current.Items[Constant.AjaxID + ".pagescripts"] = scripts;
            }

            return scripts;
        }

        /// <summary>
        /// Registers the client script block.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="key">The key.</param>
        /// <param name="script">The script.</param>
        internal static void RegisterClientScriptBlock(System.Web.UI.Page page, string key, string script)
        {
            if (!System.Web.HttpContext.Current.Items.Contains(Constant.AjaxID + ".pageID"))
            {
                System.Web.HttpContext.Current.Items.Add(Constant.AjaxID + ".pageID", "mypage");
                //page.PreRender += new EventHandler(page_PreRender);
            }

            var scripts = GetScripts();

            if (scripts.Contains(Constant.AjaxID + ".script." + key))
                return;

            scripts.Add(Constant.AjaxID + ".script." + key, script);
        }

        /// <summary>
        /// Handles the PreRender event of the page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void page_PreRender(object sender, EventArgs e)
        {
            ListDictionary scripts = GetScripts();

            if (scripts == null)
                return;

            System.Web.UI.Page page = (System.Web.UI.Page)sender;
            if (page != null)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("\r\n");

                foreach(string key in scripts.Keys)
                {
                    sb.Append(scripts[key].ToString());
                    sb.Append("\r\n");
                }

#if(NET20)
                // TODO: replace with new .NET 2.0 method
                // page.ClientScript.RegisterClientScriptInclude("name", "file.js");
                // we have to put only the filename to the list
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), Constant.AjaxID + ".javascript", sb.ToString());

#else
			page.RegisterClientScriptBlock(Constant.AjaxID + ".javascript", sb.ToString());
#endif
            }
        }

#endif
        #endregion

        public static string ProcessRequest(HttpContext context, Type type)
        {
            if (HttpRuntime.Cache.Get(type.FullName) != null)
                return HttpRuntime.Cache.Get(type.FullName).ToString();

            var path = type.FullName + "," + type.Assembly.FullName.Split(',')[0];
            if (type.Assembly.GetName().Name.StartsWith("App_Web"))
            {
                var baseType = type.BaseType;
                path = baseType.FullName + "," + baseType.Assembly.FullName.Split(',')[0];
            }
            if (Settings.UseAssemblyQualifiedName) path = type.AssemblyQualifiedName;

            if (Settings != null && Settings.UrlNamespaceMappings.ContainsValue(path))
            {
                foreach (string key in Settings.UrlNamespaceMappings.Keys)
                {
                    if (Settings.UrlNamespaceMappings[key].ToString() == path)
                    {
                        path = key;
                        break;
                    }
                }
            }

            var url = context.Request.ApplicationPath + (context.Request.ApplicationPath.EndsWith("/") ? "" : "/") + HandlerPath + "/" + GetSessionUri() + path + HandlerExtension;

            var mi = type.GetMethods();
            var methodList = new ArrayList();

            for (var y = 0; y < mi.Length; y++)
            {
                var method = mi[y];

                if (!method.IsPublic)
                    continue;

                var ma = (AjaxMethodAttribute[])method.GetCustomAttributes(typeof(AjaxMethodAttribute), true);

                if (ma.Length == 0)
                    continue;

                var ppa = (PrincipalPermissionAttribute[])method.GetCustomAttributes(typeof(PrincipalPermissionAttribute), true);
                if (ppa.Length > 0)
                {
                    var permissionDenied = true;
                    for (var p = 0; p < ppa.Length && permissionDenied; p++)
                    {
                        if (!string.IsNullOrEmpty(ppa[p].Role) && context.User != null && context.User.Identity.IsAuthenticated && !context.User.IsInRole(ppa[p].Role))
                            continue;

                        permissionDenied = false;
                    }

                    if (permissionDenied)
                        continue;
                }

                //methods[mc++] = method;
                methodList.Add(method);
            }

            var methods = (MethodInfo[])methodList.ToArray(typeof(MethodInfo));

            // render client-side proxy file

            var sb = new StringBuilder();
            TypeJavaScriptProvider jsp = null;

            if (Settings.TypeJavaScriptProvider != null)
            {
                try
                {
                    var jspt = Type.GetType(Settings.TypeJavaScriptProvider);
                    if (jspt != null && typeof(TypeJavaScriptProvider).IsAssignableFrom(jspt))
                    {
                        jsp = (TypeJavaScriptProvider)Activator.CreateInstance(jspt, new object[] { type, url, sb });
                    }
                }
                catch (Exception)
                {
                }
            }

            if (jsp == null)
            {
                jsp = new TypeJavaScriptProvider(type, url, sb);
            }


            jsp.RenderNamespace();
            jsp.RenderClassBegin();
            jsp.RenderMethods(methods);
            jsp.RenderClassEnd();
			
            HttpRuntime.Cache.Remove(type.FullName);
            HttpRuntime.Cache.Insert(type.FullName, sb.ToString(), null, DateTime.UtcNow.AddDays(1), Cache.NoSlidingExpiration);

            return sb.ToString();
        }
    }
}