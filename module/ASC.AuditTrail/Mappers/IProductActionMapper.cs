using System.Collections.Generic;

using ASC.AuditTrail.Types;

namespace ASC.AuditTrail.Mappers
{
    public interface IProductActionMapper
    {
        ProductType Product { get; }
        List<IModuleActionMapper> Mappers { get; }
    }
}
