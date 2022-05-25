using System.Extensions;
using Xunit;

namespace OpenStack.Extensions
{
    public class TypeExtensionsTests
    {
        public class A
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Number { get; set; }
        }

        public class B
        {
            public Identifier Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void CopyProperties_SameObjectType()
        {
            var src = new A {Id = "{id}", Name = "{name}", Number = 1};
            var dest = new A();
            src.CopyProperties(dest);
            Assert.Equal(src.Id, dest.Id);
            Assert.Equal(src.Name, dest.Name);
            Assert.Equal(src.Number, dest.Number);
        }

        [Fact]
        public void CopyProperties_DifferentObjectType()
        {
            var src = new A { Id = "{id}", Name = "{name}", Number = 1 };
            var dest = new B();
            src.CopyProperties(dest);
            Assert.Equal(src.Name, dest.Name);
        }
    }
}
