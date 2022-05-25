using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// The exception that is thrown when a <see cref="IContentDeliveryNetworkService"/> service operation (Create, Delete, Update) fails.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [Serializable]
    public sealed class ServiceOperationFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOperationFailedException"/> class.
        /// </summary>
        public ServiceOperationFailedException(IEnumerable<ServiceError> errors)
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOperationFailedException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        private ServiceOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Errors = OpenStackNet.Deserialize<IEnumerable<ServiceError>>(info.GetString("service_errors"));
        }

        /// <summary>
        /// Errors generated during the previous service operation.
        /// </summary>
        public IEnumerable<ServiceError> Errors { get; private set; }


        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // ReSharper disable once ExceptionNotDocumented
            info.AddValue("service_errors", OpenStackNet.Serialize(Errors));
            base.GetObjectData(info, context);
        }
    }
}
