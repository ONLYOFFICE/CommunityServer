using System;
namespace BasecampRestAPI
{
	public interface IAttachment
	{
        int ByteSize { get; }
        string DownloadUrl {get;}
        string ID { get; }
        string Name { get; }
        int OwnerID { get; }
        string OwnerType {get;}
        int AuthorID { get; }
        DateTime CreatedOn { get; }        
	}
}