/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

namespace ASC.Web.Core.Security
{
    public class AntiXssModule : IHttpModule
    {
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.
        }

        private static readonly List<string> SkipFields = new List<string>() { "__VIEWSTATE", "__EVENTVALIDATION", "__EVENTTARGET" };

        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequestHandler;
        }

        private static void BeginRequestHandler(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.Form.Count>0)
            {
                //HACK: Unlock form for writing. Bad hack
                var writableMethod = HttpContext.Current.Request.Form.GetType().GetMethod("MakeReadWrite", BindingFlags.NonPublic | BindingFlags.Instance);
                var readOnlyMethod = HttpContext.Current.Request.Form.GetType().GetMethod("MakeReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
                var formField = HttpContext.Current.Request.GetType().GetField("_form", BindingFlags.NonPublic | BindingFlags.Instance);
                writableMethod.Invoke(HttpContext.Current.Request.Form, null);
                //Filter form values
                foreach (string param in HttpContext.Current.Request.Form.AllKeys)
                {
                    if (!SkipFields.Contains(param))
                    {
                        var paramvalue = HttpContext.Current.Request.Form.Get(param);
                        //Clean it. Collection is read only
                        HttpContext.Current.Request.Form.Set(param,HtmlSanitizer.Sanitize(paramvalue));
                    }
                }
                formField.SetValue(HttpContext.Current.Request, HttpContext.Current.Request.Form);
                readOnlyMethod.Invoke(HttpContext.Current.Request.Form, null);
            }


        }

        #endregion

    }
}
