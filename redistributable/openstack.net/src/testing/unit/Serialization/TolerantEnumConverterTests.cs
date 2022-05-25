using System.Runtime.Serialization;
using Newtonsoft.Json;
using Xunit;

namespace OpenStack.Serialization
{
    public class TolerantEnumConverterTests
    {
        [JsonConverter(typeof(TolerantEnumConverter))]
        enum ThingStatus
        {
            Active,
            Unknown,
            [EnumMember(Value = "REMOVE_FAILED")]
            RemoveFailed
        }

        [JsonConverter(typeof(TolerantEnumConverter))]
        enum StuffStatus
        {
            Missing,
            Present
        }

        [Fact]
        public void WhenValueIsRecognized_MatchToValue()
        {
            var result = OpenStackNet.Deserialize<ThingStatus>("\"ACTIVE\"");

            Assert.Equal(ThingStatus.Active, result);
        }

        [Fact]
        public void WhenAttributedValueIsRecognized_MatchToValue()
        {
            var result = OpenStackNet.Deserialize<ThingStatus>("\"REMOVE_FAILED\"");

            Assert.Equal(ThingStatus.RemoveFailed, result);
        }

        [Fact]
        public void WhenValueIsUnrecognized_MatchToUnknownValue()
        {
            var result = OpenStackNet.Deserialize<ThingStatus>("\"bad-enum-value\"");

            Assert.Equal(ThingStatus.Unknown, result);
        }

        [Fact]
        public void WhenValueIsUnrecognized_AndUnknownIsNotPresent_MatchToFirstValue()
        {
            var result = OpenStackNet.Deserialize<StuffStatus>("\"bad-enum-value\"");

            Assert.Equal(StuffStatus.Missing, result);
        }

        [Fact]
        public void WhenValueIsUnrecognized_AndDestinationIsNullable_UseNull()
        {
            var result = OpenStackNet.Deserialize<StuffStatus?>("\"bad-enum-value\"");

            Assert.Null(result);
        }
    }
}
