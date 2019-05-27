using System;
using System.Runtime.Serialization;
using Ical.Net.Proxies;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An abstract class from which all iCalendar data types inherit.
    /// </summary>
    public abstract class CalendarDataType : ICalendarDataType
    {
        private IParameterCollection _parameters;
        private ParameterCollectionProxy _proxy;
        private ServiceProvider _serviceProvider;

        protected ICalendarObject _AssociatedObject;

        protected CalendarDataType()
        {
            Initialize();
        }

        private void Initialize()
        {
            _parameters = new ParameterList();
            _proxy = new ParameterCollectionProxy(_parameters);
            _serviceProvider = new ServiceProvider();
        }

        [OnDeserializing]
        internal void DeserializingInternal(StreamingContext context)
        {
            OnDeserializing(context);
        }

        [OnDeserialized]
        internal void DeserializedInternal(StreamingContext context)
        {
            OnDeserialized(context);
        }

        protected virtual void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }

        protected virtual void OnDeserialized(StreamingContext context) {}

        public virtual Type GetValueType()
        {
            // See RFC 5545 Section 3.2.20.
            if (_proxy != null && _proxy.ContainsKey("VALUE"))
            {
                switch (_proxy.Get("VALUE"))
                {
                    case "BINARY":
                        return typeof (byte[]);
                    case "BOOLEAN":
                        return typeof (bool);
                    case "CAL-ADDRESS":
                        return typeof (Uri);
                    case "DATE":
                        return typeof (IDateTime);
                    case "DATE-TIME":
                        return typeof (IDateTime);
                    case "DURATION":
                        return typeof (TimeSpan);
                    case "FLOAT":
                        return typeof (double);
                    case "INTEGER":
                        return typeof (int);
                    case "PERIOD":
                        return typeof (Period);
                    case "RECUR":
                        return typeof (RecurrencePattern);
                    case "TEXT":
                        return typeof (string);
                    case "TIME":
                        // FIXME: implement ISO.8601.2004
                        throw new NotImplementedException();
                    case "URI":
                        return typeof (Uri);
                    case "UTC-OFFSET":
                        return typeof (UtcOffset);
                    default:
                        return null;
                }
            }
            return null;
        }

        public virtual void SetValueType(string type)
        {
            _proxy?.Set("VALUE", type ?? type.ToUpper());
        }

        public virtual ICalendarObject AssociatedObject
        {
            get => _AssociatedObject;
            set
            {
                if (Equals(_AssociatedObject, value))
                {
                    return;
                }

                _AssociatedObject = value;
                if (_AssociatedObject != null)
                {
                    _proxy.SetParent(_AssociatedObject);
                    if (_AssociatedObject is ICalendarParameterCollectionContainer)
                    {
                        _proxy.SetProxiedObject(((ICalendarParameterCollectionContainer) _AssociatedObject).Parameters);
                    }
                }
                else
                {
                    _proxy.SetParent(null);
                    _proxy.SetProxiedObject(_parameters);
                }
            }
        }

        public virtual Calendar Calendar => _AssociatedObject?.Calendar;

        public virtual string Language
        {
            get => Parameters.Get("LANGUAGE");
            set => Parameters.Set("LANGUAGE", value);
        }

        /// <summary>
        /// Copies values from the target object to the
        /// current object.
        /// </summary>
        public virtual void CopyFrom(ICopyable obj)
        {
            if (!(obj is ICalendarDataType))
            {
                return;
            }

            var dt = (ICalendarDataType) obj;
            _AssociatedObject = dt.AssociatedObject;
            _proxy.SetParent(_AssociatedObject);
            _proxy.SetProxiedObject(dt.Parameters);
        }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <returns>The copy of the object.</returns>
        public virtual T Copy<T>()
        {
            var type = GetType();
            var obj = Activator.CreateInstance(type) as ICopyable;

            // Duplicate our values
            if (obj is T)
            {
                obj.CopyFrom(this);
                return (T) obj;
            }
            return default(T);
        }

        public virtual IParameterCollection Parameters => _proxy;

        public virtual object GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

        public object GetService(string name) => _serviceProvider.GetService(name);

        public T GetService<T>() => _serviceProvider.GetService<T>();

        public T GetService<T>(string name) => _serviceProvider.GetService<T>(name);

        public void SetService(string name, object obj) => _serviceProvider.SetService(name, obj);

        public void SetService(object obj) => _serviceProvider.SetService(obj);

        public void RemoveService(Type type) => _serviceProvider.RemoveService(type);

        public void RemoveService(string name) => _serviceProvider.RemoveService(name);
    }
}