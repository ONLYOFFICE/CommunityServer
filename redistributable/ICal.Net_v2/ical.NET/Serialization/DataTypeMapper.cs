using System;
using System.Collections.Generic;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization
{
    public delegate Type TypeResolverDelegate(object context);

    public class DataTypeMapper : IDataTypeMapper
    {
        private class PropertyMapping
        {
            public Type ObjectType { get; set; }
            public TypeResolverDelegate Resolver { get; set; }
            public bool AllowsMultipleValuesPerProperty { get; set; }
        }

        private readonly IDictionary<string, PropertyMapping> _propertyMap = new Dictionary<string, PropertyMapping>(StringComparer.OrdinalIgnoreCase);

        public DataTypeMapper()
        {
            AddPropertyMapping("ACTION", typeof (AlarmAction), false);
            AddPropertyMapping("ATTACH", typeof (IAttachment), false);
            AddPropertyMapping("ATTENDEE", typeof (IAttendee), false);
            AddPropertyMapping("CATEGORIES", typeof (string), true);
            AddPropertyMapping("COMMENT", typeof (string), false);
            AddPropertyMapping("COMPLETED", typeof (IDateTime), false);
            AddPropertyMapping("CONTACT", typeof (string), false);
            AddPropertyMapping("CREATED", typeof (IDateTime), false);
            AddPropertyMapping("DTEND", typeof (IDateTime), false);
            AddPropertyMapping("DTSTAMP", typeof (IDateTime), false);
            AddPropertyMapping("DTSTART", typeof (IDateTime), false);
            AddPropertyMapping("DUE", typeof (IDateTime), false);
            AddPropertyMapping("DURATION", typeof (TimeSpan), false);
            AddPropertyMapping("EXDATE", typeof (IPeriodList), false);
            AddPropertyMapping("EXRULE", typeof (IRecurrencePattern), false);
            AddPropertyMapping("FREEBUSY", typeof (IFreeBusyEntry), true);
            AddPropertyMapping("GEO", typeof (IGeographicLocation), false);
            AddPropertyMapping("LAST-MODIFIED", typeof (IDateTime), false);
            AddPropertyMapping("ORGANIZER", typeof (IOrganizer), false);
            AddPropertyMapping("PERCENT-COMPLETE", typeof (int), false);
            AddPropertyMapping("PRIORITY", typeof (int), false);
            AddPropertyMapping("RDATE", typeof (IPeriodList), false);
            AddPropertyMapping("RECURRENCE-ID", typeof (IDateTime), false);
            AddPropertyMapping("RELATED-TO", typeof (string), false);
            AddPropertyMapping("REQUEST-STATUS", typeof (IRequestStatus), false);
            AddPropertyMapping("REPEAT", typeof (int), false);
            AddPropertyMapping("RESOURCES", typeof (string), true);
            AddPropertyMapping("RRULE", typeof (IRecurrencePattern), false);
            AddPropertyMapping("SEQUENCE", typeof (int), false);
            AddPropertyMapping("STATUS", ResolveStatusProperty, false);
            AddPropertyMapping("TRANSP", typeof (TransparencyType), false);
            AddPropertyMapping("TRIGGER", typeof (ITrigger), false);
            AddPropertyMapping("TZNAME", typeof (string), false);
            AddPropertyMapping("TZOFFSETFROM", typeof (IUtcOffset), false);
            AddPropertyMapping("TZOFFSETTO", typeof (IUtcOffset), false);
            AddPropertyMapping("TZURL", typeof (Uri), false);
            AddPropertyMapping("URL", typeof (Uri), false);
        }

        protected Type ResolveStatusProperty(object context)
        {
            var obj = context as ICalendarObject;
            if (obj != null)
            {
                if (obj.Parent is IEvent)
                {
                    return typeof (EventStatus);
                }
                if (obj.Parent is ITodo)
                {
                    return typeof (TodoStatus);
                }
                if (obj.Parent is IJournal)
                {
                    return typeof (JournalStatus);
                }
            }

            return null;
        }

        public void AddPropertyMapping(string name, Type objectType, bool allowsMultipleValues)
        {
            if (name != null && objectType != null)
            {
                var m = new PropertyMapping
                {
                    ObjectType = objectType,
                    AllowsMultipleValuesPerProperty = allowsMultipleValues
                };

                _propertyMap[name] = m;
            }
        }

        public void AddPropertyMapping(string name, TypeResolverDelegate resolver, bool allowsMultipleValues)
        {
            if (name != null && resolver != null)
            {
                var m = new PropertyMapping
                {
                    Resolver = resolver,
                    AllowsMultipleValuesPerProperty = allowsMultipleValues
                };

                _propertyMap[name] = m;
            }
        }

        public void RemovePropertyMapping(string name)
        {
            if (name != null && _propertyMap.ContainsKey(name))
            {
                _propertyMap.Remove(name);
            }
        }

        public virtual bool GetPropertyAllowsMultipleValues(object obj)
        {
            var p = obj as ICalendarProperty;
            PropertyMapping m;
            return p?.Name != null && _propertyMap.TryGetValue(p.Name, out m) && m.AllowsMultipleValuesPerProperty;
        }

        public virtual Type GetPropertyMapping(object obj)
        {
            var p = obj as ICalendarProperty;
            if (p?.Name == null)
            {
                return null;
            }

            PropertyMapping m;
            if (!_propertyMap.TryGetValue(p.Name, out m))
            {
                return null;
            }

            return m.Resolver == null
                ? m.ObjectType
                : m.Resolver(p);
        }
    }
}