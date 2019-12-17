using System.Runtime.Serialization;

namespace ASC.Files.Core
{
    [DataContract(Namespace = "")]
    public enum ForcesaveType
    {
        [EnumMember] None = 0,

        [EnumMember] Command = 1,

        [EnumMember] User = 2,

        [EnumMember] Timer = 3
    }
}