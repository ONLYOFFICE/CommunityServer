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