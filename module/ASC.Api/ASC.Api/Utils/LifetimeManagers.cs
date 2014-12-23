/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using System;
using System.Threading;
using System.Web;
using System.Web.Caching;

namespace ASC.Api.Utils
{
    public class HttpContextLifetimeManager : LifetimeManager, IDisposable
    {
        private readonly Type _type;

        public HttpContextLifetimeManager(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            _type = type;
        }

        #region IDisposable Members

        public void Dispose()
        {
            RemoveValue();
        }

        #endregion

        public override object GetValue()
        {
            return HttpContext.Current.Items[_type.AssemblyQualifiedName];
        }

        public override void RemoveValue()
        {
            HttpContext.Current.Items.Remove(_type.AssemblyQualifiedName);
        }

        public override void SetValue(object newValue)
        {
            HttpContext.Current.Items[_type.AssemblyQualifiedName]
                = newValue;
        }
    }

    public class HttpContextLifetimeManager2 : LifetimeManager, IDisposable
    {
        private readonly HttpContextBase _context;
        private readonly string _type;

        public HttpContextLifetimeManager2() : this(new HttpContextWrapper(HttpContext.Current))
        {
        }

        public HttpContextLifetimeManager2(HttpContextBase context)
        {
            _context = context;
            _type = Guid.NewGuid().ToString();
        }

        #region IDisposable Members

        public void Dispose()
        {
            RemoveValue();
        }

        #endregion

        public override object GetValue()
        {
            return _context.Items[_type];
        }

        public override void RemoveValue()
        {
            _context.Items.Remove(_type);
        }

        public override void SetValue(object newValue)
        {
            _context.Items[_type] = newValue;
        }
    }

    public class NewInstanceLifetimeManager : LifetimeManager, IDisposable
    {
        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public override object GetValue()
        {
            return null;
        }

        public override void SetValue(object newValue)
        {
        }

        public override void RemoveValue()
        {
        }
    }

    public class SingletonLifetimeManager : LifetimeManager, IDisposable, IRequiresRecovery
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private object _value;

        protected virtual object SynchronizedGetValue()
        {
            return _value;
        }

        protected virtual void SynchronizedSetValue(object newValue)
        {
            _value = newValue;
        }

        public override object GetValue()
        {
            try
            {
                _lock.EnterReadLock();
                return SynchronizedGetValue();
            }
            finally 
            {
                _lock.ExitReadLock();
            }
        }

        public override void SetValue(object newValue)
        {
            try
            {
                _lock.EnterWriteLock();
                SynchronizedSetValue(newValue);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void RemoveValue()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_value == null)
                return;
            var disposable = _value as IDisposable;
            if (disposable != null)
                (disposable).Dispose();
            _value = null;
        }

        public void Recover()
        {
        }
    }
}