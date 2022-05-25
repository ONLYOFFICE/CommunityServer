using System;
using Newtonsoft.Json;
using Xunit;

namespace Rackspace
{
    public class IdentifierTests
    {
        public class Thing
        {
            public Identifier Id { get; set; }
        }

        [Fact]
        public void Serialize()
        {
            var rawId = Guid.NewGuid();
            var id = (Identifier)rawId;

            var result = JsonConvert.SerializeObject(id);

            Assert.Equal($"\"{rawId.ToString("D")}\"", result);
        }

        [Fact]
        public void Serialize_WithinAClass()
        {
            var thing = new Thing {Id = Guid.NewGuid()};

            var result = JsonConvert.SerializeObject(thing, Formatting.None);

            Assert.Equal($"{{\"Id\":\"{thing.Id}\"}}", result);
        }

        [Fact]
        public void Deserialize()
        {
            var id = new Identifier(Guid.NewGuid());

            var json = JsonConvert.SerializeObject(id);
            var result = JsonConvert.DeserializeObject<Identifier>(json);

            Assert.Equal(id, result);
        }

        [Fact]
        public void CanCompareWithString()
        {
            var rawId = Guid.NewGuid().ToString("D");
            var id = new Identifier(rawId);

            Assert.Equal(rawId, id);
            Assert.Equal(id, rawId);
            Assert.True(id == rawId);
            Assert.True(rawId == id);
            Assert.True(id.Equals(rawId));
            Assert.True(rawId.Equals(id));
        }

        [Fact]
        public void CanCompareWithGuid()
        {
            var rawId = Guid.NewGuid();
            var id = new Identifier(rawId);

            Assert.Equal(rawId, id);
            Assert.Equal(id, rawId);
            Assert.True(id == rawId);
            Assert.True(rawId == id);
            Assert.True(id.Equals(rawId));
        }
    }
}