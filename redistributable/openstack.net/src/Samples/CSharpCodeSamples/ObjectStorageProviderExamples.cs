namespace CSharpCodeSamples
{
    using System;
    using System.Collections.Generic;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Providers;
    using net.openstack.Providers.Rackspace;
    using Stream = System.IO.Stream;

    public class ObjectStorageProviderExamples
    {
        #region ListObjectsInContainer
        public void ListObjects(IObjectStorageProvider provider, string containerName)
        {
            Console.WriteLine("Objects in container {0}", containerName);
            foreach (ContainerObject containerObject in ListAllObjects(provider, containerName))
                Console.WriteLine("    {0}", containerObject.Name);
        }

        private static IEnumerable<ContainerObject> ListAllObjects(
            IObjectStorageProvider provider,
            string containerName,
            int? blockSize = null,
            string prefix = null,
            string region = null,
            bool useInternalUrl = false,
            CloudIdentity identity = null)
        {
            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException("blockSize");

            ContainerObject lastContainerObject = null;

            do
            {
                string marker = lastContainerObject != null ? lastContainerObject.Name : null;
                IEnumerable<ContainerObject> containerObjects =
                    provider.ListObjects(containerName, blockSize, marker, null, prefix, region, useInternalUrl, identity);
                lastContainerObject = null;
                foreach (ContainerObject containerObject in containerObjects)
                {
                    lastContainerObject = containerObject;
                    yield return containerObject;
                }
            } while (lastContainerObject != null);
        }
        #endregion ListObjectsInContainer

        #region CreateObjectFromFileWithMetadata
        public void CreateObjectWithMetadata(
            IObjectStorageProvider provider,
            string containerName,
            string objectName,
            string filePath)
        {
            // Method 1: Set metadata when the object is created
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { CloudFilesProvider.ObjectMetaDataPrefix + "Key", "Value" }
            };
            provider.CreateObjectFromFile(containerName, filePath, objectName, headers: headers);

            // Method 2: Set metadata in a separate call after the object is created
            provider.CreateObjectFromFile(containerName, filePath, objectName);
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Key", "Value" }
            };
            provider.UpdateObjectMetadata(containerName, objectName, metadata);
        }
        #endregion CreateObjectWithMetadata

        #region CreateObjectWithMetadata
        public void CreateObjectWithMetadata(
            IObjectStorageProvider provider,
            string containerName,
            string objectName,
            Stream data)
        {
            // Method 1: Set metadata when the object is created
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { CloudFilesProvider.ObjectMetaDataPrefix + "Key", "Value" }
            };
            provider.CreateObject(containerName, data, objectName, headers: headers);

            // Method 2: Set metadata in a separate call after the object is created
            provider.CreateObject(containerName, data, objectName);
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Key", "Value" }
            };
            provider.UpdateObjectMetadata(containerName, objectName, metadata);
        }
        #endregion CreateObjectWithMetadata
    }
}
