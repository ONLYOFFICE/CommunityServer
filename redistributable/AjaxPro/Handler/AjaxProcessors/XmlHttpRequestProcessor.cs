/*
 * XmlHttpRequestProcessor.cs
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
 * MS	06-04-26	renamed property Method to AjaxMethod to be CLSCompliant
 * MS	06-04-28	changed http header names to AjaxPro-Method
 * MS	06-04-29	added context item for JSON request
 * MS	06-05-03	set encoding to UTF-8
 * MS	06-05-04	fixed bug on Express Web Developer when method does not have
 *					any argument
 * MS	06-05-09	implemented CanHandleRequest
 *					fixed return value of AjaxMethod if type equals null
 *					fixed if there are ambiguous methods
 * MS	06-05-22	using inherited.GetMethodInfo for fixed AjaxNamespace usage for method
 * MS	06-05-30	changed method value name to X-AjaxPro-Method
 * MS	06-06-06	ContentType moved to inherited class
 * MS	06-06-08	fixed missing /* if used from cache
 * MS	06-07-19	fixed if method argument is from type IJavaScriptObject
 * MS	06-07-20	removed the fix above and put it to JavaScriptConverter
 * MS	06-10-03	fixed bug with CryptProvider
 * MS	07-04-24	fixed Ajax token
 *					using new AjaxSecurityProvider
 *					added optional x-ajaxpro-method to querystring
 * 
 * 
 * 
 * 
 */
using System;
using System.Web;
using System.Reflection;
using System.IO;

namespace AjaxPro
{
	internal class XmlHttpRequestProcessor : IAjaxProcessor
	{
		private int hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlHttpRequestProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">The type.</param>
		internal XmlHttpRequestProcessor(HttpContext context, Type type) : base(context, type)
		{
		}

		#region IAjaxProcessor Members

        /// <summary>
        /// Retreives the parameters.
        /// </summary>
        /// <returns></returns>
		public override object[] RetreiveParameters()
		{
			context.Request.ContentEncoding = System.Text.UTF8Encoding.UTF8;

			ParameterInfo[] pi = method.GetParameters();

			object[] args = new object[pi.Length];

			// initialize default values
			for(int i=0; i<pi.Length; i++)
				args[i] = pi[i].DefaultValue;

			byte[] b = new byte[context.Request.InputStream.Length];

			if(context.Request.InputStream.Read(b, 0, b.Length) == 0)
				return null;

			StreamReader sr = new StreamReader(new MemoryStream(b), System.Text.UTF8Encoding.UTF8);

			string v = null;
			try
			{
				v = sr.ReadToEnd();
			}
			finally
			{
				sr.Close();
			}

			// If something went wrong or there are no arguments
			// we can return with the empty argument array.

			if(v == null || pi.Length == 0 || v == "{}")
				return args;

			hashCode = v.GetHashCode();

			// check if we have to decrypt the JSON string.
			if(Utility.Settings != null && Utility.Settings.Security != null)
				v = Utility.Settings.Security.SecurityProvider.Decrypt(v);

			context.Items.Add(Constant.AjaxID + ".JSON", v);

			JavaScriptObject jso = (JavaScriptObject)JavaScriptDeserializer.DeserializeFromJson(v, typeof(JavaScriptObject));
			for(int i=0; i<pi.Length; i++)
			{
				if (jso.Contains(pi[i].Name))
				{
					//if(pi[i].ParameterType.IsAssignableFrom(jso[pi[i].Name].GetType()))
					//{
					//    args[i] = jso[pi[i].Name];
					//}
					//else
					{
						args[i] = JavaScriptDeserializer.Deserialize((IJavaScriptObject)jso[pi[i].Name], pi[i].ParameterType);
					}
				}
			}
			
			return args;
		}

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
		public override string SerializeObject(object o)
		{
			// On the client we want to have a real object.
			// For more details visit: http://www.crockford.com/JSON/index.html
			// 
			// JSON is built on two structures:
			//   - A collection of name/value pairs. In various languages, 
			//     this is realized as an object, record, struct, dictionary, 
			//     hash table, keyed list, or associative array. 
			//   - An ordered list of values. In most languages, this is realized 
			//     as an array. 

			string res;
			if (o is Exception)
				res = "{\"error\":" + JavaScriptSerializer.Serialize(o) + "}";
			else
				res = "{\"value\":" + JavaScriptSerializer.Serialize(o) + "}";

			// check if we have to encrypt the JSON string.
			if (Utility.Settings != null && Utility.Settings.Security != null)
				res = Utility.Settings.Security.SecurityProvider.Encrypt(res);

			context.Response.Write(res);

			return res;
		}

        /// <summary>
        /// Gets a value indicating whether this instance can handle request.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can handle request; otherwise, <c>false</c>.
        /// </value>
		internal override bool CanHandleRequest
		{
			get
			{
				if (context.Request.Headers["X-" + Constant.AjaxID + "-Method"] != null)
					return true;

				if (context.Request["X-" + Constant.AjaxID + "-Method"] != null)
					return true;

				return false;
			}
		}

	    private MethodInfo m_MethodInfo = null;

        /// <summary>
        /// Gets the ajax method.
        /// </summary>
        /// <value>The ajax method.</value>
		public override MethodInfo AjaxMethod
		{
			get
			{
                if (m_MethodInfo!=null)
                {
                    return m_MethodInfo;
                }

				string m = context.Request.Headers["X-" + Constant.AjaxID + "-Method"];

				if (string.IsNullOrEmpty(m))
					m = context.Request["X-" + Constant.AjaxID + "-Method"];

                if (!string.IsNullOrEmpty(m))
                {
                    m_MethodInfo = GetMethodInfo(m);
                    return m_MethodInfo;
                }

			    return null;
			}
		}

        /// <summary>
        /// Gets a value indicating whether this instance is encryption able.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is encryption able; otherwise, <c>false</c>.
        /// </value>
		public override bool IsEncryptionAble
		{
			get
			{
				return true;
			}
		}

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
		public override int GetHashCode()
		{
			return hashCode;
		}


		#endregion

	}

}
