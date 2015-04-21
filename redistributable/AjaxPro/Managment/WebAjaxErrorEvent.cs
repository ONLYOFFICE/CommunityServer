/*
 * WebAjaxErrorEvent.cs
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
 * MS	06-05-03	initial version (thanks to Albert)
 * MS	06-05-09	added new constructor
 * 
 */
#if(WEBEVENT)
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Management;
using System.Reflection;
using System.Web;
using AjaxPro;

namespace AjaxPro.Management
{
    public class WebAjaxErrorEvent : WebBaseErrorEvent
    {
        IAjaxProcessor ajaxProcessor;
        object[] parameters;
        Exception exception;

		public WebAjaxErrorEvent(string message, int eventCode, Exception exception)
			: base(message, null, eventCode, exception)
		{
			this.exception = exception;
		}

		public WebAjaxErrorEvent(string message, IAjaxProcessor ajaxProcessor, int eventCode, Exception exception)
			: base(message, ajaxProcessor, eventCode, exception)
		{
			this.ajaxProcessor = ajaxProcessor;
			this.exception = exception;
		}

		public WebAjaxErrorEvent(string message, IAjaxProcessor ajaxProcessor, object[] parameters, int eventCode, Exception exception) 
			: base(message, ajaxProcessor, eventCode, exception)
        {
            this.ajaxProcessor = ajaxProcessor;
            this.parameters = parameters;
            this.exception = exception;
        }

        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
			if (ajaxProcessor != null)
			{
				MethodInfo methodInfo = ajaxProcessor.AjaxMethod;
				object[] paremeters = ajaxProcessor.RetreiveParameters();
				string str = string.Format("TargetSite: {0}", exception.TargetSite);

				formatter.AppendLine(str);
				formatter.AppendLine("");

				if (System.Web.HttpContext.Current.Items[Constant.AjaxID + ".JSON"] != null)
				{
					formatter.AppendLine("JSON: " + System.Web.HttpContext.Current.Items[Constant.AjaxID + ".JSON"].ToString());
					formatter.AppendLine("");
				}

				str = string.Format(Constant.AjaxID + " Method: {0}.{1}", methodInfo.ReflectedType.ToString(), methodInfo.Name);

				formatter.AppendLine(str);
				formatter.AppendLine("");

				if (this.parameters != null)
				{
					foreach (object o in this.parameters)
					{
						formatter.AppendLine(string.Format("Parameter: {0}", o));
					}

					formatter.AppendLine("");
				}

				formatter.AppendLine("StrackTrace:");
				formatter.AppendLine(exception.StackTrace);
			}
			else
			{
				string str = string.Format("TargetSite: {0}", exception.TargetSite);

				formatter.AppendLine(str);
				formatter.AppendLine("");

				if (System.Web.HttpContext.Current.Items[Constant.AjaxID + ".JSON"] != null)
				{
					formatter.AppendLine("JSON: " + System.Web.HttpContext.Current.Items[Constant.AjaxID + ".JSON"].ToString());
					formatter.AppendLine("");
				}

				formatter.AppendLine("StrackTrace:");
				formatter.AppendLine(exception.StackTrace);
			}

            base.FormatCustomEventDetails(formatter);
        }
    }
}
#endif