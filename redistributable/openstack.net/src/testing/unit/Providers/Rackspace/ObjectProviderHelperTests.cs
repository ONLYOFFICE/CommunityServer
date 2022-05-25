using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using net.openstack.Core;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Validators;
using net.openstack.Providers.Rackspace;
using net.openstack.Providers.Rackspace.Validators;

namespace OpenStackNet.Testing.Unit.Providers.Rackspace
{
    [TestClass]
    public class ObjectProviderHelperTests
    {
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Pass_Validation_For_Container_Name()
        {
            const string containerName = "DarkKnight";
            var validatorMock = new Mock<IObjectStorageValidator>();

            validatorMock.Setup(v => v.ValidateContainerName(containerName));

            var objectStoreValidator = CloudFilesValidator.Default;
            objectStoreValidator.ValidateContainerName(containerName);

        }

        //[ExpectedException(typeof(ArgumentNullException),"ERROR: Container Name cannot be Null.")]
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Throw_Exception_When_Passing_Empty_Container_Name()
        {
            const string containerName = "";
            var validatorMock = new Mock<IObjectStorageValidator>();

            validatorMock.Setup(v => v.ValidateContainerName(containerName));

            try
            {
                var objectStoreValidator = CloudFilesValidator.Default;
                objectStoreValidator.ValidateContainerName(containerName);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("ERROR: Container Name cannot be empty." + Environment.NewLine + "Parameter name: containerName", ex.Message);
            }
        }

        [TestMethod] 
        [TestCategory(TestCategories.Unit)]
        public void Should_Throw_Exception_When_Passing_Null_Container_Name()
        {
            const string containerName = null;
            var validatorMock = new Mock<IObjectStorageValidator>();

            validatorMock.Setup(v => v.ValidateContainerName(containerName));

            try
            {
                var objectStoreValidator = CloudFilesValidator.Default;
                objectStoreValidator.ValidateContainerName(containerName);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Value cannot be null." + Environment.NewLine + "Parameter name: containerName", ex.Message);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Throw_Exception_When_Passing_256_Characters_In_Container_Name()
        {
            string containerName = "AaAaAaAaAa";

            while (containerName.Length <= 256)
            {
                containerName += containerName;
            }
            var validatorMock = new Mock<IObjectStorageValidator>();

            validatorMock.Setup(v => v.ValidateContainerName(containerName));

            try
            {
                var objectStoreValidator = CloudFilesValidator.Default;
                objectStoreValidator.ValidateContainerName(containerName);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (ContainerNameException ex)
            {

                Assert.AreEqual(string.Format("ERROR: encoded URL Length greater than 256 char's. Container Name:[{0}]",containerName), ex.Message);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void Should_Throw_Exception_When_Passing_Forwar_Slash_In_Container_Name()
        {
            const string containerName = "/";

            var validatorMock = new Mock<IObjectStorageValidator>();

            validatorMock.Setup(v => v.ValidateContainerName(containerName));

            try
            {
                var objectStoreValidator = CloudFilesValidator.Default;
                objectStoreValidator.ValidateContainerName(containerName);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (ContainerNameException ex)
            {

                Assert.AreEqual(string.Format("ERROR: Container Name contains a /. Container Name:[{0}]", containerName), ex.Message);
            }
        }


    }
}
