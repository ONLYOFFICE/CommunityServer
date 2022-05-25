using System;
using Newtonsoft.Json;
using Xunit;

namespace OpenStack
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

            var result = OpenStackNet.Serialize(id);

            Assert.Equal(string.Format("\"{0}\"", rawId.ToString("D")), result);
        }

        [Fact]
        public void Serialize_WithinAClass()
        {
            var thing = new Thing {Id = Guid.NewGuid()};

            var result = OpenStackNet.Serialize(thing);

            Assert.Equal(string.Format("{{\"Id\":\"{0}\"}}", thing.Id), result);
        }

        [Fact]
        public void Deserialize()
        {
            var id = new Identifier(Guid.NewGuid());

            var json = OpenStackNet.Serialize(id);
            var result = OpenStackNet.Deserialize<Identifier>(json);

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
            Assert.Equal(rawId, id);
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