using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.Evaluation;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An iCalendar list of recurring dates (or date exclusions)
    /// </summary>
    public class PeriodList : EncodableDataType, IPeriodList, IList<IPeriod>
    {
        public string TzId { get; set; }
        
        public int Count => Periods.Count;
        public bool IsReadOnly => Periods.IsReadOnly;

        protected IList<IPeriod> Periods { get; set; } = new List<IPeriod>();

        public PeriodList()
        {
            SetService(new PeriodListEvaluator(this));
        }

        public PeriodList(string value) : this()
        {
            var serializer = new PeriodListSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            var list = obj as IPeriodList;
            if (list == null)
            {
                return;
            }

            foreach (var p in list)
            {
                Add(p);
            }
        }

        public override string ToString() => new PeriodListSerializer().SerializeToString(this);

        public void Add(IDateTime dt) => Periods.Add(new Period(dt));

        public static Dictionary<string, List<Period>> GetGroupedPeriods(IList<IPeriodList> periodLists)
        {
            // In order to know if two events are equal, a semantic understanding of exdates, rdates, rrules, and exrules is required. This could be done by
            // computing the complete recurrence set (expensive) while being time-zone sensitive, or by comparing each List<Period> in each IPeriodList.

            // For example, events containing these rules generate the same recurrence set, including having the same time zone for each occurrence, so
            // they're the same:
            // Event A:
            // RDATE:20170302T060000Z,20170303T060000Z
            // Event B:
            // RDATE:20170302T060000Z
            // RDATE:20170303T060000Z

            var grouped = new Dictionary<string, HashSet<Period>>(StringComparer.OrdinalIgnoreCase);
            foreach (var periodList in periodLists)
            {
                var defaultBucket = string.IsNullOrWhiteSpace(periodList.TzId) ? "" : periodList.TzId;

                foreach (var period in periodList.Cast<Period>())
                {
                    var actualBucket = string.IsNullOrWhiteSpace(period.StartTime.TzId) ? defaultBucket : period.StartTime.TzId;

                    if (!grouped.ContainsKey(actualBucket))
                    {
                        grouped.Add(actualBucket, new HashSet<Period>());
                    }
                    grouped[actualBucket].Add(period);
                }
            }
            return grouped.ToDictionary(k => k.Key, v => v.Value.OrderBy(d => d.StartTime).ToList());
        }

        protected bool Equals(PeriodList other)
        {
            return string.Equals(TzId, other.TzId, StringComparison.OrdinalIgnoreCase)
                   && CollectionHelpers.Equals(Periods, other.Periods);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PeriodList)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = string.IsNullOrWhiteSpace(TzId) ? 0 : TzId.GetHashCode();
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Periods);
                return hashCode;
            }
        }

        public IPeriod this[int index]
        {
            get => Periods[index];
            set => Periods[index] = value;
        }

        public bool Remove(IPeriod item) => Periods.Remove(item);
        public int IndexOf(IPeriod item) => Periods.IndexOf(item);
        public void Insert(int index, IPeriod item) => Periods.Insert(index, item);
        public void RemoveAt(int index) => Periods.RemoveAt(index);
        public void Add(IPeriod item) => Periods.Add(item);
        public void Clear() => Periods.Clear();
        public bool Contains(IPeriod item) => Periods.Contains(item);
        public void CopyTo(IPeriod[] array, int arrayIndex) => Periods.CopyTo(array, arrayIndex);
        public IEnumerator<IPeriod> GetEnumerator() => Periods.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Periods.GetEnumerator();
    }
}