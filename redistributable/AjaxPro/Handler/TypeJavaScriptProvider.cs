/*
 * TypeJavaScriptProvider.cs
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
 * MS	07-04-24	initial version
 * TB	07-07-31	added Ext JS framework
 * MS	08-03-24	patch V1p3r, work item 12114
 * 
 * 
 * 
 */
using System;
using System.Text;
using System.Reflection;
#if(NET20)
using System.Collections.Generic;
#endif

namespace AjaxPro
{
	public class TypeJavaScriptProvider
	{
		protected Type m_Type;
		protected string m_URL;
		protected StringBuilder sb;

		public TypeJavaScriptProvider(Type type, string url, StringBuilder sb)
		{
			m_Type = type;
			m_URL = url;
			this.sb = sb;
		}

		#region Protected Methods

		protected string GetClientNamespace()
		{
			AjaxNamespaceAttribute[] cma = (AjaxNamespaceAttribute[])m_Type.GetCustomAttributes(typeof(AjaxNamespaceAttribute), true);
			string clientNS = m_Type.FullName;

			if (Utility.Settings.UseSimpleObjectNaming)
				clientNS = m_Type.Name;

			if (cma.Length > 0 && cma[0].ClientNamespace != null)
				clientNS = cma[0].ClientNamespace;

			return clientNS;
		}

		protected string GetClientMethodName(MethodInfo method)
		{
			AjaxNamespaceAttribute[] cmam = (AjaxNamespaceAttribute[])method.GetCustomAttributes(typeof(AjaxNamespaceAttribute), true);
			if (cmam.Length > 0)
				return cmam[0].ClientNamespace;

			return method.Name;
		}

		#endregion

		#region Private Methods

		private void RenderXHR(string methodName, ParameterInfo[] pi)
		{
			sb.Append("\t\treturn this.invoke(\"");
			sb.Append(methodName);
			sb.Append("\", {");

			for (int p = 0; p < pi.Length; p++)
			{
				sb.Append("\"");
				sb.Append(pi[p].Name);
				sb.Append("\":");
				sb.Append(pi[p].Name);

				if (p < pi.Length - 1)
					sb.Append(", ");
			}

			sb.Append("}, this.");
			sb.Append(methodName);
			sb.Append(".getArguments().slice(");
			sb.Append(pi.Length.ToString());
			sb.Append("));\r\n");
		}

		private void RenderMethodParameters(ParameterInfo[] pi)
		{
			for (int p = 0; p < pi.Length; p++)
			{
				sb.Append(pi[p].Name);

				if (p < pi.Length - 1)
					sb.Append(", ");
			}
		}

		#endregion

		public virtual void RenderNamespace()
		{
			string clientNS = GetClientNamespace();

			sb.Append(JavaScriptUtil.GetClientNamespaceRepresentation(clientNS + "_class"));
		}

		public virtual void RenderClassBegin()
		{
			string clientNS = GetClientNamespace();

			sb.Append(clientNS);
			sb.Append("_class = function() {};\r\n");

			sb.Append("Object.extend(");
			sb.Append(clientNS);
			sb.Append("_class.prototype, Object.extend(new AjaxPro.AjaxClass(), {\r\n");
		}

		public virtual void RenderClassEnd()
		{
			string clientNS = GetClientNamespace();

			sb.Append("\turl: '");
			sb.Append(m_URL);
			sb.Append("'\r\n");

			sb.Append("}));\r\n");

			// create a new instance of the JavaScript class

			sb.Append(clientNS);
			sb.Append(" = new ");
			sb.Append(clientNS);
			sb.Append("_class();\r\n");
		}

		public virtual void RenderMethods(MethodInfo[] methods)
		{
			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i] == null)
				{
					break;
				}
				MethodInfo method = methods[i];
				string methodName = GetClientMethodName(method);
				ParameterInfo[] pi = method.GetParameters();

				sb.Append("\t");
				sb.Append(methodName);
				sb.Append(": function(");

				RenderMethodParameters(pi);

				sb.Append(") {\r\n");


				RenderXHR(methodName, pi);


				sb.Append("\t},\r\n");

				//if (i < methods.Count - 1)
				//    sb.Append(",");
			}
		}
	}

	public class jQueryTypeJavaScriptProvider : TypeJavaScriptProvider
	{
		public jQueryTypeJavaScriptProvider(Type type, string url, StringBuilder sb)
			: base(type, url, sb)
		{
		}

		public override void RenderClassBegin()
		{
			string clientNS = GetClientNamespace();

			sb.Append(clientNS);
			sb.Append("_class = function() {};\r\n");

			sb.Append(clientNS);
			sb.Append("_class.prototype.url = '" + m_URL + "';\r\n");
		}

		public override void RenderClassEnd()
		{
			string clientNS = GetClientNamespace();

			sb.Append("var ");
			sb.Append(clientNS);
			sb.Append(" = new ");
			sb.Append(clientNS);
			sb.Append("_class();\r\n\r\n");

		}

		public override void RenderMethods(MethodInfo[] methods)
		{
			string clientNS = GetClientNamespace();

			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i] == null)
				{
					break;
				}
				MethodInfo method = methods[i];
				string methodName = GetClientMethodName(method);
				ParameterInfo[] pi = method.GetParameters();


				sb.Append(clientNS + "_class.prototype.");
				sb.Append(methodName);
				sb.Append(" = function(");

				for (int p = 0; p < pi.Length; p++)
				{
					sb.Append(pi[p].Name);
					sb.Append(", ");
				}

				sb.Append("onsuccess, onerror) {\r\n");

				sb.Append(@"
	return jQuery.ajax({
		type: ""POST"",
		url: this.url,
		data: {");


				for (int p = 0; p < pi.Length; p++)
				{
					sb.Append("\"");
					sb.Append(pi[p].Name);
					sb.Append("\":");
					sb.Append(pi[p].Name);

					if (p < pi.Length - 1)
						sb.Append(", ");
				}


				sb.Append(@"}.toJSONString(),
		beforeSend: function(xhr) {
			xhr.setRequestHeader(""X-AjaxPro-Method"", """ + methodName + @""");
			if(typeof AjaxPro !== 'undefined' && AjaxPro.token !== null)
				xhr.setRequestHeader(""X-AjaxPro-Token"", AjaxPro.token);
		},
		success: function(s) {
			var o = null;
			eval(""o = "" + s + "";"");
			if(o != null) {
				if(typeof o.value != ""undefined"" && typeof onsuccess == ""function"") {
					onsuccess(o.value);
					return;
				} else if(typeof o.error != ""undefined"" && typeof onerror == ""function"") {
					onerror(o.error);
					return;
				}
			}
			if(typeof onerror == ""function"") {
				onerror({""Message"":""Failed.""});
			}
		}
	}); 
");

				sb.Append("};\r\n");

				//if (i < methods.Count - 1)
				//    sb.Append(",");
			}
		}
	}

	public class ExtTypeJavaScriptProvider : TypeJavaScriptProvider
	{
		public ExtTypeJavaScriptProvider(Type type, string url, StringBuilder sb)
			: base(type, url, sb)
		{
		}

		public override void RenderClassBegin()
		{
			string clientNS = GetClientNamespace();

			sb.Append(clientNS);
			sb.Append("_class = function() {");

			sb.Append(@"
    this.connection = new Ext.data.AjaxProConnection({
        url: """ + m_URL + @""",
        listeners: {
            requestcomplete: function(connection, response, options) {
                var o = Ext.decode(response.responseText);
                var onsuccess = options.onsuccess;
                var onerror = options.onerror;
                if(o != null) {
                    if (typeof o.value != ""undefined"" && typeof onsuccess == ""function"") {
                        onsuccess(o.value);
                        return;
                    } else if(typeof o.error != ""undefined"" && typeof onerror == ""function"") {
                        onerror(o.error);
                        return;
                    }
                }
                if(typeof onerror == ""function"") {
                    onerror({""Message"": ""Failed.""});
                }
            },
            requestexception: function(connection, response, options, e) {
                var onerror = options.onerror;
                if(typeof onerror == ""function"") {
                    onerror({""Message"": ""Failed.""});
                }
            }
        }
    });
};");

			sb.Append("\r\n\r\n");
			sb.Append(clientNS);
			sb.Append("_class.prototype = {\r\n");
		}

		public override void RenderClassEnd()
		{
			string clientNS = GetClientNamespace();

			sb.Append("var ");
			sb.Append(clientNS);
			sb.Append(" = new ");
			sb.Append(clientNS);
			sb.Append("_class();\r\n\r\n");
		}

		public override void RenderMethods(MethodInfo[] methods)
		{
			string clientNS = GetClientNamespace();

			for (int i = 0; i < methods.Length; i++)
			{
				if (methods[i] == null)
				{
					break;
				}
				MethodInfo method = methods[i];
				string methodName = GetClientMethodName(method);
				ParameterInfo[] pi = method.GetParameters();

				sb.Append("    ");
				sb.Append(methodName);
				sb.Append(": function(");

				for (int p = 0; p < pi.Length; p++)
				{
					sb.Append(pi[p].Name);
					sb.Append(", ");
				}

				sb.Append("onsuccess, onerror) {");

				sb.Append(@"
        return this.connection.request({
            ajaxProMethod: """ + methodName + @""",
            ajaxProToken: (typeof AjaxPro !== ""undefined"" && AjaxPro.token !== null) ? AjaxPro.token : """",
            params: {");

				for (int p = 0; p < pi.Length; p++)
				{
					sb.Append("\"");
					sb.Append(pi[p].Name);
					sb.Append("\": ");
					sb.Append(pi[p].Name);

					if (p < pi.Length - 1)
						sb.Append(", ");
				}

				sb.Append(@"},
            onsuccess: onsuccess,
            onerror: onerror
        });
    }");

				if (i < methods.Length - 1)
					sb.Append(",\r\n");
			}

			sb.Append("\r\n};\r\n\r\n");
		}
	}
}
