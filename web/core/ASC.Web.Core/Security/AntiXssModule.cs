/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
