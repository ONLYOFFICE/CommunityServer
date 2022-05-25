using System;
using System.Linq;
using net.openstack.Core;
using net.openstack.Core.Domain;
using net.openstack.Core.Domain.Mapping;
using net.openstack.Providers.Rackspace.Objects.Response;

namespace net.openstack.Providers.Rackspace.Objects.Mapping
{
    internal class BulkDeletionResultMapper : IObjectMapper<BulkDeleteResponse, BulkDeletionResults>
    {
        private readonly IStatusParser _statusParser;

        public BulkDeletionResultMapper(IStatusParser statusParser)
        {
            _statusParser = statusParser;
        }

        /// <inheritdoc/>
        public BulkDeletionResults Map(BulkDeleteResponse from)
        {
            var successfulObjects = from.AllItems.Where(i => !from.IsItemError(i));
            var failedObjects = from.Errors.Select(e =>
            {
                var eParts = e.ToArray();
                Status errorStatus;
                string errorItem;

                if (eParts.Length != 2)
                {
                    errorStatus = new Status(0, "Unknown");
                    errorItem = string.Format("The error array has an unexpected length. Array: {0}", string.Join("||", eParts));
                }
                else
                {
                    errorItem = eParts[1];
                    if (!_statusParser.TryParse(eParts[0], out errorStatus))
                    {
                        errorItem = eParts[0];
                        if (!_statusParser.TryParse(eParts[1], out errorStatus))
                        {
                            errorStatus = new Status(0, "Unknown");
                            errorItem = string.Format("The error array is in an unknown format. Array: {0}", string.Join("||", eParts));
                        }
                    }
                }

                return new BulkDeletionFailedObject(errorItem, errorStatus);
            });

            return new BulkDeletionResults(successfulObjects, failedObjects);
        }

        /// <inheritdoc/>
        public BulkDeleteResponse Map(BulkDeletionResults to)
        {
            throw new NotImplementedException();
        }
    }
}
