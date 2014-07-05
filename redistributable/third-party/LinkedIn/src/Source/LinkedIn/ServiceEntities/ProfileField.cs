//-----------------------------------------------------------------------
// <copyright file="ProfileField.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// The possible profile fields.
  /// </summary>
  public enum ProfileField
  {
    /// <summary>
    /// A unique identifier token for a member
    /// </summary>
    [Description("id")]
    PersonId = 0,

    /// <summary>
    /// The first name of a member.
    /// </summary>
    [Description("first-name")]
    FirstName = 1,

    /// <summary>
    /// The last name of a member.
    /// </summary>
    [Description("last-name")]
    LastName = 2,

    /// <summary>
    /// The headline of a member.
    /// </summary>
    [Description("headline")]
    Headline = 3,

    /// <summary>
    /// The industry this member belongs to.
    /// </summary>
    [Description("industry")]
    Industry = 4,

    /// <summary>
    /// The degree distance of the fetched profile from the member who fetched the profile.
    /// </summary>
    [Description("distance")]
    Distance = 5,

    /// <summary>
    /// The current status of a member.
    /// </summary>
    [Description("current-status")]
    CurrentStatus = 6,

    /// <summary>
    /// The timestamp, in milliseconds, when the member's status was last set.
    /// </summary>
    [Description("current-status-timestamp")]
    CurrentStatusTimestamp = 7,

    /// <summary>
    /// The connections of a member.
    /// </summary>
    [Description("connections")]
    Connections = 8,

    /// <summary>
    /// The number of connections a member has.
    /// </summary>
    [Description("num-connections")]
    NumberOfConnections = 9,

    /// <summary>
    /// Whether the number of connections has been capped.
    /// </summary>
    [Description("num-connections-capped")]
    NumberOfConnectionsCapped = 10,

    /// <summary>
    /// The summary of a member.
    /// </summary>
    [Description("summary")]
    Summary = 11,

    /// <summary>
    /// The specialties of a member.
    /// </summary>
    [Description("specialties")]
    Specialties = 12,

    /// <summary>
    /// How a member approaches proposals.
    /// </summary>
    [Description("proposal-comments")]
    ProposalComments = 13,

    /// <summary>
    /// The associations a member has.
    /// </summary>
    [Description("associations")]
    Associations = 14,

    /// <summary>
    /// The honors a member may have.
    /// </summary>
    [Description("honors")]
    Honors = 15,

    /// <summary>
    /// The current positions a member has, limited to three.
    /// </summary>
    [Description("three-current-positions")]
    ThreeCurrentPositions = 16,

    /// <summary>
    /// The past positions a member has had, limited to three.
    /// </summary>
    [Description("three-past-positions")]
    ThreePastPositions = 17,

    /// <summary>
    /// The number of recommenders a member has.
    /// </summary>
    [Description("num-recommenders")]
    NumberOfRecommenders = 18,

    /// <summary>
    /// The phone numbers of a member.
    /// </summary>
    [Description("phone-numbers")]
    PhoneNumbers = 19,

    /// <summary>
    /// The instant messenger accounts of a member.
    /// </summary>
    [Description("im-accounts")]
    IMAccounts = 20,

    /// <summary>
    /// The twitter accounts of a member.
    /// </summary>
    [Description("twitter-accounts")]
    TwitterAccounts = 21,

    /// <summary>
    /// The date of birth of a member.
    /// </summary>
    [Description("date-of-birth")]
    DateOfBirth = 22,

    /// <summary>
    /// The main address of a member.
    /// </summary>
    [Description("main-address")]
    MainAddress = 23,

    /// <summary>
    /// The picture url of a member.
    /// </summary>
    [Description("picture-url")]
    PictureUrl = 24,

    /// <summary>
    /// The URL to the member's authenticated profile on LinkedIn.
    /// </summary>
    [Description("site-standard-profile-request:(url)")]
    SiteStandardProfileRequestUrl = 25,

    /// <summary>
    /// The URL representing the resource you would request for programmatic access to the member's public profile.
    /// </summary>
    [Description("api-public-profile-request:(url)")]
    ApiPublicProfileRequestUrl = 26,

    /// <summary>
    /// The public profile URL for the member on the LinkedIn.com website. 
    /// </summary>
    [Description("site-public-profile-request:(url)")]
    SitePublicProfileRequestUrl = 27,

    /// <summary>
    /// The URL to the member's public profile.
    /// </summary>
    [Description("public-profile-url")]
    PublicProfileUrl = 28,

    /// <summary>
    /// The current share of the member.
    /// </summary>
    [Description("current-share")]
    CurrentShare = 29,
    
    /// <summary>
    /// The generic name of a location.
    /// </summary>
    [Description("name")]
    LocationName = 101,

    /// <summary>
    /// The country code of a location.
    /// </summary>
    [Description("country:(code)")]
    LocationCountryCode = 102,

    /// <summary>
    /// The degree distance of the fetched profile from the member who fetched the profile.
    /// </summary>
    [Description("distance")]
    RelationToViewerDistance = 201,

    /// <summary>
    /// The number of profiles that link the fetching member to the fetched profile.
    /// </summary>
    [Description("num-related-connections")]
    RelationToViewerNumberOfRelatedConnections = 202,

    /// <summary>
    /// The related connections of a member.
    /// </summary>
    [Description("related-connections")]
    RelationToViewerRelatedConnections = 203,
    
    /// <summary>
    /// The fully-qualified URL being shared.
    /// </summary>
    [Description("url")]
    MemberUrlUrl = 301,

    /// <summary>
    /// The label given to the URL by the member.
    /// </summary>
    [Description("name")]
    MemberUrlName = 302,

    /// <summary>
    /// The URL representing the resource you would request for programmatic access to the member's profile.
    /// </summary>
    [Description("url")]
    ApiStandardProfileRequestUrl = 401,

    /// <summary>
    /// A collection of fields that can be re-used as HTTP headers to request an out of network profile programmatically.
    /// </summary>
    [Description("headers")]
    ApiStandardProfileRequestHeaders = 402,

    /// <summary>
    /// The unique identifier for a position.
    /// </summary>
    [Description("id")]
    PositionId = 501,

    /// <summary>
    /// The title of a position.
    /// </summary>
    [Description("title")]
    PositionTitle = 502,

    /// <summary>
    /// The summary of a position.
    /// </summary>
    [Description("summary")]
    PositionSummary = 503,

    /// <summary>
    /// The start date when a position began.
    /// </summary>
    [Description("start-date")]
    PositionStartDate = 504,

    /// <summary>
    /// The end date when a position ended.
    /// </summary>
    [Description("end-date")]
    PositionEndDate = 505,

    /// <summary>
    /// Whether a position is the current.
    /// </summary>
    [Description("is-current")]
    PositionIsCurrent = 506,

    /// <summary>
    /// The company name the member works/worked for.
    /// </summary>
    [Description("company:(name)")]
    PositionCompanyName = 507,

    /// <summary>
    /// The unique identifier for a education entry.
    /// </summary>
    [Description("id")]
    EducationId = 601,

    /// <summary>
    /// The school name of a education entry.
    /// </summary>
    [Description("school-name")]
    EducationSchoolName = 602,

    /// <summary>
    /// The field of study at a school.
    /// </summary>
    [Description("field-of-study")]
    EducationFieldOfStudy = 603,

    /// <summary>
    /// The start date when a education began.
    /// </summary>
    [Description("start-date")]
    EducationStartDate = 604,

    /// <summary>
    /// The end date when a education ended.
    /// </summary>
    [Description("end-date")]
    EducationEndDate = 605,

    /// <summary>
    /// The degree, if any, received a the school.
    /// </summary>
    [Description("degree")]
    EducationDegree = 606,

    /// <summary>
    /// The activities a member was involved in during his education.
    /// </summary>
    [Description("activities")]
    EducationActivities = 607,

    /// <summary>
    /// The unique identifier for a recommendation.
    /// </summary>
    [Description("id")]
    RecommendationId = 701,

    /// <summary>
    /// The type of a recommendation.
    /// </summary>
    [Description("recommendation-type")]
    RecommendationRecommendationType = 702,

    /// <summary>
    /// The person who made the recommendation.
    /// </summary>
    [Description("recommender")]
    RecommendationRecommender = 703
  }
}
