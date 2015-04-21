using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BasecampRestAPI
{
	/*
		<company>
			<id type="integer">1776</id>
			<name>Peter Griffin Brewery</name>
			<address-one>123 Main Street</address-one>
			<address-two>Suite 456</address-two>
			<city>Anytown</city>
			<state>OH</state>
			<zip>65445</zip>
			<country>USA</country>
			<web-address>http://petergriffinbrewery.example.com</web-address>
			<phone-number-office>343-555-1212</phone-number-office>
			<phone-number-fax>343-555-1212</phone-number-fax>
			<time-zone-id>PST</time-zone-id>
			<can-see-private type="boolean">true</can-see-private>

			<!-- for non-client companies -->
			<url-name>http://petergriffinbrewery.example.com</url-name>
		</company>
	 */
	public class Company : ICompany
	{
		#region Implementation of ICompany
		public int Id { get; private set; }
		public string Name { get; private set; }
		public string Address1 { get; private set; }
		public string Address2 { get; private set; }
		public string City { get; private set; }
		public string State { get; private set; }
		public string ZipCode { get; private set; }
		public string Country { get; private set; }
		public string WebAddress { get; private set; }
		public string PhoneNumberOffice { get; private set; }
		public string PhoneNumberFax { get; private set; }
		public string TimeZoneId { get; private set; }
		public bool CanSeePrivate { get; private set; }
		public string UrlName { get; private set; }
		public IPerson[] People
		{
			get
			{
				var requestPath = string.Format("projects/{0}/people.json", Id);
				var people = new List<IPerson>();
				foreach (JObject node in _service.GetRequestResponseElement(requestPath))
				{
					people.Add(Person.GetInstance(_camp, node));
				}
				return people.ToArray();
			}
		}

		#endregion

		public static ICompany GetInstance(IBaseCamp camp, JObject node)
		{
			return new Company(camp, camp.Service, node);
		}
        private Company(IBaseCamp camp, IRestWebService service, JObject node)
		{
			_camp = camp;
			_service = service;
			Id = node["id"].ToObject<int>();
			Name = node["name"].ToObject<string>();
            Address1 = node["address-one"].ToObject<string>();
            Address2 = node["address-two"].ToObject<string>();
            City = node["city"].ToObject<string>();
            State = node["state"].ToObject<string>();
            ZipCode = node["zip"].ToObject<string>();
            Country = node["country"].ToObject<string>();
            WebAddress = node["web-address"].ToObject<string>();
            PhoneNumberOffice = node["phone-number-office"].ToObject<string>();
            PhoneNumberFax = node["phone-number-fax"].ToObject<string>();
            TimeZoneId = node["time-zone-id"].ToObject<string>();
            CanSeePrivate = node["can-see-private"].ToObject<bool>();
            UrlName = node["url-name"].ToObject<string>();
		}
		private readonly IBaseCamp _camp;
		private readonly IRestWebService _service;

		public static string PathForId(int id)
		{
			return string.Format("companies/{0}.xml", id);
		}
	}
}
