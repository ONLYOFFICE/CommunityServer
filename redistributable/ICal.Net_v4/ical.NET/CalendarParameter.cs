using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.Collections.Interfaces;

namespace Ical.Net
{
    [DebuggerDisplay("{Name}={string.Join(\",\", Values)}")]
    public class CalendarParameter : CalendarObject, IValueObject<string>
    {
        private HashSet<string> _values;

        public CalendarParameter()
        {
            Initialize();
        }

        public CalendarParameter(string name) : base(name)
        {
            Initialize();
        }

        public CalendarParameter(string name, string value) : base(name)
        {
            Initialize();
            AddValue(value);
        }

        public CalendarParameter(string name, IEnumerable<string> values) : base(name)
        {
            Initialize();
            foreach (var v in values)
            {
                AddValue(v);
            }
        }

        private void Initialize()
        {
            _values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void CopyFrom(ICopyable c)
        {
            base.CopyFrom(c);

            var p = c as CalendarParameter;
            if (p?.Values == null)
            {
                return;
            }

            _values = new HashSet<string>(p.Values.Where(IsValidValue), StringComparer.OrdinalIgnoreCase);
        }

        public virtual IEnumerable<string> Values => _values;

        public virtual bool ContainsValue(string value) => _values.Contains(value);

        public virtual int ValueCount => _values?.Count ?? 0;

        public virtual void SetValue(string value)
        {
            _values.Add(value);
        }

        public virtual void SetValue(IEnumerable<string> values)
        {
            // Remove all previous values
            _values.Clear();
            _values.UnionWith(values.Where(IsValidValue));
        }

        private bool IsValidValue(string value) => !string.IsNullOrWhiteSpace(value);

        public virtual void AddValue(string value)
        {
            if (!IsValidValue(value))
            {
                return;
            }
            _values.Add(value);
        }

        public virtual void RemoveValue(string value)
        {
            _values.Remove(value);
        }

        public virtual string Value
        {
            get => Values?.FirstOrDefault();
            set => SetValue(value);
        }
    }
}