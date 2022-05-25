using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace OpenStack.Serialization
{
    public class EmptyEnumerableTests
    {
        private class ExampleThing
        {
            public ExampleThing()
            {
                Messages = new List<string>();
            }

            [JsonProperty("messages")]
            public IEnumerable<string> Messages { get; set; } 
        }

        [Fact]
        public void WhenDeserializingNullCollection_ItShouldUseAnEmptyCollection()
        {
            var thing = new ExampleThing{Messages = null};
            string json = OpenStackNet.Serialize(thing);
            Assert.DoesNotContain("messages", json);

            var result = OpenStackNet.Deserialize<ExampleThing>(json);

            Assert.NotNull(result.Messages);
            Assert.Empty(result.Messages);
        }

        [Fact]
        public void WhenSerializingEmptyCollection_ItShouldBeIgnored()
        {
            var thing = new ExampleThing { Messages = new List<string>() };

            string json = OpenStackNet.Serialize(thing);

            Assert.DoesNotContain("messages", json);
        }
    }
}
