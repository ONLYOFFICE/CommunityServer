using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace OpenStack.Networking.v2
{
    public class DHCPOptionConverterTests
    {
        [Fact]
        public void Serialize()
        {
            var input = new PortCreateDefinition(null)
            {
                DHCPOptions =
                {
                    {"a", "stuff"},
                    {"b", "things"}
                }
            };

            string result = OpenStackNet.Serialize(input);

            string expectedJson = JObject.Parse("{'port':{'extra_dhcp_opts':[{'opt_name':'a','opt_value':'stuff'},{'opt_name':'b','opt_value':'things'}]}}").ToString(Formatting.None);
            Assert.Equal(expectedJson, result);
        }

        [Fact]
        public void Deserialize()
        {
            string json = JObject.Parse("{'port':{'extra_dhcp_opts':[{'opt_name':'a','opt_value':'stuff'},{'opt_name':'b','opt_value':'things'}]}}").ToString(Formatting.None);

            var result = OpenStackNet.Deserialize<PortCreateDefinition>(json).DHCPOptions;

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
            Assert.Contains("a", result.Keys);
            Assert.Contains("b", result.Keys);
            Assert.Equal("stuff", result["a"]);
            Assert.Equal("things", result["b"]);
        }

        [Fact]
        public void OpenStackNet_UsesDHCPOptionConverter()
        {
            var port = new Port
            {
                DHCPOptions = new Dictionary<string, string>
                {
                    {"a", "stuff"}
                }
            };

            var json = OpenStackNet.Serialize(port);
            var result = OpenStackNet.Deserialize<Port>(json);

            Assert.NotNull(result.DHCPOptions);
            Assert.Equal(1, result.DHCPOptions.Count);
        }
    }
}
