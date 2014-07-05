using System;
using System.Xml;

namespace BasecampRestAPI
{
	/*
<account>
  <id type="integer">1</id>
  <name>Your Company</name>
  <account-holder-id type="integer">1</account-holder-id>
  <ssl-enabled type="boolean">true</ssl-enabled>
  <email-notification-enabled type="boolean">true</email-notification-enabled>
  <time-tracking-enabled type="boolean">true</time-tracking-enabled>
  <updated-at type="datetime">2009-10-09T17:52:46Z</updated-at>
</account>
	 */
	public class Account : IAccount
	{
		private readonly IBaseCamp _camp;

		public static IAccount GetInstance(IBaseCamp camp, XmlNode node)
		{
			return new Account(camp, node);
		}
		private Account(IBaseCamp camp, XmlNode node)
		{
			_camp = camp;
			Id = XmlHelpers.ParseInteger(node, "id");
			Name = XmlHelpers.ChildNodeText(node, "name");
			AccountHolderId = XmlHelpers.ParseInteger(node, "account-holder-id");
			SSLEnabled = XmlHelpers.ParseBool(node, "ssl-enabled");
			EmailNotificationEnabled = XmlHelpers.ParseBool(node, "email-notification-enabled");
			TimeTrackingEnabled = XmlHelpers.ParseBool(node, "time-tracking-enabled");
			UpdatedAt = XmlHelpers.ParseDateTime(node, "updated-at", DateTime.MinValue);
		}

		#region Implementation of IAccount

		public int Id { get; private set; }
		public string Name { get; private set; }
		public int AccountHolderId { get; private set; }
		public bool SSLEnabled { get; private set; }
		public bool EmailNotificationEnabled { get; private set; }
		public bool TimeTrackingEnabled { get; private set; }
		public DateTime UpdatedAt { get; private set; }

		#endregion
	}
}