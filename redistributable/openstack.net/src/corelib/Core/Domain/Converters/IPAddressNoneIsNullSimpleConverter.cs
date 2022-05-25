namespace net.openstack.Core.Domain.Converters
{
    using System;
    using System.Net;
    using Newtonsoft.Json;

    /// <summary>
    /// This implementation of <see cref="JsonConverter"/> extends the behavior of
    /// <see cref="IPAddressSimpleConverter"/> by treating null values in the JSON representation as
    /// <see cref="IPAddress.None"/> instead of <see langword="null"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class IPAddressNoneIsNullSimpleConverter : IPAddressSimpleConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object result = base.ReadJson(reader, objectType, existingValue, serializer);
            return result ?? IPAddress.None;
        }

        /// <inheritdoc/>
        protected override string ConvertToString(IPAddress obj)
        {
            if (IPAddress.None.Equals(obj))
                return null;

            return base.ConvertToString(obj);
        }
    }
}
