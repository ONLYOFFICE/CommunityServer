using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace OpenStack.Serialization
{
    public class RootWrapperConverterTests
    {
        [JsonConverterWithConstructor(typeof(RootWrapperConverter), "thing")]
        class Thing
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        [JsonConverterWithConstructor(typeof(RootWrapperConverter), "things")]
        class ThingCollection : List<Thing>
        {
        }
        
        [Fact]
        public void Serialize()
        {
            var json = OpenStackNet.Serialize(new Thing());
            
            var jsonObj = JObject.Parse(json);
            JProperty rootProperty = jsonObj.Properties().FirstOrDefault();
            Assert.NotNull(rootProperty);
            Assert.Equal("thing", rootProperty.Name);
        }

        [Fact]
        public void Deserialize()
        {
            var json = OpenStackNet.Serialize(new Thing {Id = "thing-id"});
            var thing = OpenStackNet.Deserialize<Thing>(json);
            Assert.Equal("thing-id", thing.Id);
        }

        [Fact]
        public void SerializeWhenNotRoot()
        {
            var json = OpenStackNet.Serialize(new List<Thing>{ new Thing() });

            Assert.DoesNotContain("\"thing\"", json);
        }

        [Fact]
        public void SerializeWhenNested()
        {
            var json = OpenStackNet.Serialize(new ThingCollection { new Thing() });

            Assert.DoesNotContain("\"thing\"", json);
        }

        [Fact]
        public void DeserializeWhenNotRoot()
        {
            var json = JArray.Parse("[{'id':'thing-id'}]").ToString();
            var things = OpenStackNet.Deserialize<List<Thing>>(json);
            Assert.Single(things);
            Assert.Equal("thing-id", things[0].Id);
        }

        [Fact]
        public void DeserializeWhenNested()
        {
            var json = JObject.Parse("{'things':[{'id':'thing-id'}]}").ToString();
            var things = OpenStackNet.Deserialize<ThingCollection>(json);
            Assert.NotNull(things);
            Assert.Single(things);
            Assert.Equal("thing-id", things[0].Id);
        }

        [Fact]
        public void ShouldIgnoreUnexpectedRootProperties()
        {
            var json = JObject.Parse("{'links': [{'name': 'next', 'link': 'http://nextlink'}], 'thing': {'id': 'thing-id'}}").ToString();
            var thing = OpenStackNet.Deserialize<Thing>(json);
            Assert.NotNull(thing);
            Assert.Equal("thing-id", thing.Id);
        }
    }
}