namespace BasecampRestAPI
{
	public interface IBaseCamp
	{
		IRestWebService Service { get; }
		IProject[] Projects { get; }
		IPerson[] People { get; }
		IComment[] CommentsForResource(string resource, int id, int projectid);
	}
}