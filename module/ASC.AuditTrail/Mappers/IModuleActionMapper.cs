using System.Collections.Generic;

using ASC.AuditTrail.Types;
using ASC.MessagingSystem;

namespace ASC.AuditTrail.Mappers
{
    public interface IModuleActionMapper
    {
        ModuleType Module { get; }
        IDictionary<MessageAction, MessageMaps> Actions { get; }
    }
}
