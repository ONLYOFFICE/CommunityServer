using System;

namespace BasecampRestAPI
{
    public interface IPerson
    {
        int ID { get; }
        string EmailAddress { get; }
        string UserName { get; }
        string AvatarUrl { get; }
    }
}
