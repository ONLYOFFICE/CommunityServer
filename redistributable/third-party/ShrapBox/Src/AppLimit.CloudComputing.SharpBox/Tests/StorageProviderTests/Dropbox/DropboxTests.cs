using NUnit.Framework;

namespace AppLimit.CloudComputing.SharpBox.Tests.StorageProviderTests.Dropbox
{
    [TestFixture]
    public class DropboxTests : StorageProviderTestsBase
    {
        protected override CloudStorage CreateStorage()
        {
            var storage = new CloudStorage();
            var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox);
            storage.Open(config, GetAccessToken());
            return storage;
        }

        protected override ICloudStorageAccessToken GetAccessToken()
        {
            var token = new CloudStorage().DeserializeSecurityTokenFromBase64("PEFycmF5T2ZLZXlWYWx1ZU9mc3RyaW5nc3RyaW5nIHhtbG5zPSJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tLzIwMDMvMTAvU2VyaWFsaXphdGlvbi9BcnJheXMiIHhtbG5zOmk9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIj48S2V5VmFsdWVPZnN0cmluZ3N0cmluZz48S2V5PlRva2VuUHJvdkNvbmZpZ1R5cGU8L0tleT48VmFsdWU+QXBwTGltaXQuQ2xvdWRDb21wdXRpbmcuU2hhcnBCb3guU3RvcmFnZVByb3ZpZGVyLkRyb3BCb3guRHJvcEJveENvbmZpZ3VyYXRpb248L1ZhbHVlPjwvS2V5VmFsdWVPZnN0cmluZ3N0cmluZz48S2V5VmFsdWVPZnN0cmluZ3N0cmluZz48S2V5PlRva2VuQ3JlZFR5cGU8L0tleT48VmFsdWU+QXBwTGltaXQuQ2xvdWRDb21wdXRpbmcuU2hhcnBCb3guU3RvcmFnZVByb3ZpZGVyLkRyb3BCb3guRHJvcEJveFRva2VuPC9WYWx1ZT48L0tleVZhbHVlT2ZzdHJpbmdzdHJpbmc+PEtleVZhbHVlT2ZzdHJpbmdzdHJpbmc+PEtleT5Ub2tlbkRyb3BCb3hQYXNzd29yZDwvS2V5PjxWYWx1ZT43endvOXJhMm5uZXh0Z2E8L1ZhbHVlPjwvS2V5VmFsdWVPZnN0cmluZ3N0cmluZz48S2V5VmFsdWVPZnN0cmluZ3N0cmluZz48S2V5PlRva2VuRHJvcEJveFVzZXJuYW1lPC9LZXk+PFZhbHVlPmJ0emhuN3Q3d3c5b3RjMjwvVmFsdWU+PC9LZXlWYWx1ZU9mc3RyaW5nc3RyaW5nPjxLZXlWYWx1ZU9mc3RyaW5nc3RyaW5nPjxLZXk+VG9rZW5Ecm9wQm94QXBwS2V5PC9LZXk+PFZhbHVlPmIzcGRvYjFwcDMyeWpyajwvVmFsdWU+PC9LZXlWYWx1ZU9mc3RyaW5nc3RyaW5nPjxLZXlWYWx1ZU9mc3RyaW5nc3RyaW5nPjxLZXk+VG9rZW5Ecm9wQm94QXBwU2VjcmV0PC9LZXk+PFZhbHVlPm5yeXp6NXR3dWRvcWxwczwvVmFsdWU+PC9LZXlWYWx1ZU9mc3RyaW5nc3RyaW5nPjwvQXJyYXlPZktleVZhbHVlT2ZzdHJpbmdzdHJpbmc+");
            return token;
        }
    }
}
