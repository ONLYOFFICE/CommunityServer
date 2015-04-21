namespace BasecampRestAPI
{
	public interface ICompany
	{
		int Id { get; }
		string Name { get;  }
		string Address1 { get; }
		string Address2 { get; }
		string City { get; }
		string State { get; }
		string ZipCode { get; }
		string Country { get; }
		string WebAddress { get; }
		string PhoneNumberOffice { get; }
		string PhoneNumberFax { get; }
		string TimeZoneId { get; }
		bool CanSeePrivate { get; }
		string UrlName { get; }
		IPerson[] People { get; }
	}
}
