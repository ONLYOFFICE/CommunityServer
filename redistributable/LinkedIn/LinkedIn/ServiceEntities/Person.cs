//-----------------------------------------------------------------------
// <copyright file="Person.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents a person.
  /// </summary>
  [XmlRoot("person")]
  [XmlType("person")]
  public class Person
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Person"/> class.
    /// </summary>
    public Person()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the person.
    /// </summary>
    [XmlElement("id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the first name of the person.
    /// </summary>
    [XmlElement("first-name")]
    public string FirstName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the last name of the person.
    /// </summary>
    [XmlElement("last-name")]
    public string LastName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the full name of the person.
    /// </summary>
    public string Name
    {
      get
      {
        string name = string.Empty;
        if (string.IsNullOrEmpty(FirstName) == false)
        {
          name = string.Format("{0} ", FirstName);
        }

        if (string.IsNullOrEmpty(LastName) == false && LastName.Equals("Private") == false)
        {
          name += LastName;
        }

        return name;
      }
    }

    /// <summary>
    /// Gets or sets the headline of the person.
    /// </summary>
    /// <remarks>
    /// Often "Job Title at Company
    /// </remarks>
    [XmlElement("headline")]
    public string Headline
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the location of the person.
    /// </summary>
    [XmlElement("location")]
    public Location Location
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the industry the person belongs to.
    /// </summary>
    [XmlElement("industry")]
    public string Industry
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the number of connections of the person.
    /// </summary>
    [XmlElement("num-connections")]
    public int NumberOfConnections
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the number of connections are capped.
    /// </summary>
    [XmlElement("num-connections-capped")]
    public bool NumberOfConnectionsCapped
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the number of recommenders of the person.
    /// </summary>
    [XmlElement("num-recommenders")]
    public int NumberOfRecommenders
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the degree distance of the person from the member who fetched the person.
    /// </summary>
    [XmlElement("distance")]
    public int Distance
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the degree distance of the person from the member who fetched the person.
    /// </summary>
    public string DisplayDistance
    {
      get
      {
        if (Distance > 0)
        {
          return Distance.ToString();
        }
        else
        {
          return string.Empty;
        }
      }
    }

    /// <summary>
    /// Gets or sets a <see cref="Relation" /> object describing the relation ship of the person to the member who fetched the person.
    /// </summary>
    [XmlElement("relation-to-viewer")]
    public Relation RelationToViewer
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the current status of the person.
    /// </summary>
    [XmlElement("current-status")]
    public string CurrentStatus
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the timestamp when the status of the person was last set.
    /// </summary>
    [XmlElement("current-status-timestamp")]
    public long CurrentStatusTimestamp
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the connections of the person.
    /// </summary>
    [XmlElement("connections")]
    public Connections Connections
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the summary of the person.
    /// </summary>
    [XmlElement("summary")]
    public string Summary
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the specialties of the person.
    /// </summary>
    [XmlElement("specialties")]
    public string Specialties
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the associations of the person.
    /// </summary>
    [XmlElement("associations")]
    public string Associations
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the honors that the person may have.
    /// </summary>
    [XmlElement("honors")]
    public string Honors
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of positions the person has had.
    /// </summary>
    [XmlArray("positions")]
    [XmlArrayItem("position")]
    public Collection<Position> Positions
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of educations the person has attended.
    /// </summary>
    [XmlArray("educations")]
    [XmlArrayItem("education")]
    public Collection<Education> Educations
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of the three current positions the person has.
    /// </summary>
    [XmlArray("three-current-positions")]
    [XmlArrayItem("position")]
    public Collection<Position> ThreeCurrentPositions
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of the three past positions the person has had.
    /// </summary>
    [XmlArray("three-past-positions")]
    [XmlArrayItem("position")]
    public Collection<Position> ThreePastPositions
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of urls the person shares.
    /// </summary>
    [XmlArray("member-url-resources")]
    [XmlArrayItem("member-url")]
    public Collection<MemberUrl> MemberUrls
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of member groups the person belongs to.
    /// </summary>
    [XmlArray("member-groups")]
    [XmlArrayItem("member-group")]
    public Collection<MemberGroup> MemberGroups
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of activites the person has done.
    /// </summary>
    [XmlArray("person-activities")]
    [XmlArrayItem("activity")]
    public Collection<Activity> Activities
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of recommendations the person has received.
    /// </summary>
    [XmlArray("recommendations-received")]
    [XmlArrayItem("recommendation")]
    public Collection<Recommendation> RecommendationsReceived
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of phone numbers the person has.
    /// </summary>
    [XmlArray("phone-numbers")]
    [XmlArrayItem("phone-number")]
    public Collection<PhoneNumber> PhoneNumbers
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of IM accounts the person has.
    /// </summary>
    [XmlArray("im-accounts")]
    [XmlArrayItem("im-account")]
    public Collection<IMAccount> IMAccounts
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of Twitter accounts the person has.
    /// </summary>
    [XmlArray("twitter-accounts")]
    [XmlArrayItem("twitter-account")]
    public Collection<TwitterAccount> TwitterAccounts
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the date of birth of the person.
    /// </summary>
    [XmlElement("date-of-birth")]
    public Date DateOfBirth
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the main address of the person.
    /// </summary>
    [XmlElement("main-address")]
    public string MainAddress
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the url of the person his/her profile picture.
    /// </summary>
    [XmlElement("picture-url")]
    public string PictureUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the current share of the person.
    /// </summary>
    [XmlElement("current-share")]
    public Share CurrentShare
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the public url of the person his/her profile.
    /// </summary>
    [XmlElement("public-profile-url")]
    public string PublicProfileUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the request for the standard profile using the API.
    /// </summary>
    [XmlElement("api-standard-profile-request")]
    public ApiRequest ApiStandardProfileRequest
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the request for the standard profile on the site.
    /// </summary>
    [XmlElement("site-standard-profile-request")]
    public SiteRequest SiteStandardProfileUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the request for the public profile on the site.
    /// </summary>
    [XmlElement("site-public-profile-request")]
    public SiteRequest SitePublicProfileUrl
    {
      get;
      set;
    }
    #endregion
  }
}
