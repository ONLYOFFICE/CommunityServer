using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    public class ServiceOperationFailedExceptionTests
    {
        [Fact]
        public void SerializeCustomProperties()
        {
            var errors = new[] {new ServiceError{Message = "oops!"}};
            var ex = new ServiceOperationFailedException(errors);

            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, ex);
                ms.Seek(0, 0);
                ex = (ServiceOperationFailedException)formatter.Deserialize(ms);
            }

            Assert.NotEmpty(ex.Errors);
            Assert.Equal("oops!", ex.Errors.First().Message);
        }
    }
}
