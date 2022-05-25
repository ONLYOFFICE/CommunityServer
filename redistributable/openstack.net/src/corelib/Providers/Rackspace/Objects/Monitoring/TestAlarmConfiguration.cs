namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a request to test an alarm
    /// in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.TestAlarmAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class TestAlarmConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Criteria"/> property.
        /// </summary>
        [JsonProperty("criteria")]
        private string _criteria;

        /// <summary>
        /// This is the backing field for the <see cref="CheckData"/> property.
        /// </summary>
        [JsonProperty("check_data")]
        private CheckData[] _checkData;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAlarmConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected TestAlarmConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAlarmConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="criteria">The <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/alerts-language.html">alarm DSL</see> for describing alerting conditions and their output states.</param>
        /// <param name="checkData">A collection of <see cref="CheckData"/> objects describing the result of running a check.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="criteria"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="checkData"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="criteria"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="checkData"/> is contains any <see langword="null"/> values.</para>
        /// </exception>
        public TestAlarmConfiguration(string criteria, IEnumerable<CheckData> checkData)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria");
            if (checkData == null)
                throw new ArgumentNullException("checkData");
            if (string.IsNullOrEmpty(criteria))
                throw new ArgumentException("criteria cannot be empty");
            if (checkData.Contains(null))
                throw new ArgumentException("checkData cannot contain any null values", "checkData");

            _criteria = criteria;
            _checkData = checkData.ToArray();
        }

        /// <summary>
        /// Gets the alarm DSL describing alerting conditions and their output states.
        /// </summary>
        public string Criteria
        {
            get
            {
                return _criteria;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="CheckData"/> instances describing the results
        /// of running a check.
        /// </summary>
        public ReadOnlyCollection<CheckData> CheckData
        {
            get
            {
                if (_checkData == null)
                    return null;

                return new ReadOnlyCollection<CheckData>(_checkData);
            }
        }
    }
}
