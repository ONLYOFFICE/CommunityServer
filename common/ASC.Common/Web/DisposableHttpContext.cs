/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


#region usings

using System;
using System.Collections.Generic;
using System.Web;

#endregion

namespace ASC.Common.Web
{
    public class DisposableHttpContext : IDisposable
    {
        private const string key = "disposable.key";
        private readonly HttpContext ctx;

        public DisposableHttpContext(HttpContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException();
            this.ctx = ctx;
        }

        public static DisposableHttpContext Current
        {
            get
            {
                if (HttpContext.Current == null) throw new NotSupportedException("Avaliable in web request only.");
                return new DisposableHttpContext(HttpContext.Current);
            }
        }

        public object this[string key]
        {
            get { return Items.ContainsKey(key) ? Items[key] : null; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (!(value is IDisposable)) throw new ArgumentException("Only IDisposable may be added!");
                Items[key] = (IDisposable) value;
            }
        }

        private Dictionary<string, IDisposable> Items
        {
            get
            {
                var table = (Dictionary<string, IDisposable>) ctx.Items[key];
                if (table == null)
                {
                    table = new Dictionary<string, IDisposable>(1);
                    ctx.Items.Add(key, table);
                }
                return table;
            }
        }

        #region IDisposable Members

        private bool _isDisposed;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                foreach (IDisposable item in Items.Values)
                {
                    try
                    {
                        item.Dispose();
                    }
                    catch
                    {
                    }
                }
                _isDisposed = true;
            }
        }

        #endregion
    }
}