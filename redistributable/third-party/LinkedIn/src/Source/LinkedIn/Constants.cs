//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;

namespace LinkedIn
{
  /// <summary>
  /// A Constants class.
  /// </summary>
  internal static class Constants
  {
    /// <summary>
    /// The base url for all the OAuth calls.
    /// </summary>
    public static readonly string ApiOAuthBaseUrl = "https://api.linkedin.com/uas/oauth/";

    /// <summary>
    /// The base url for all the API calls.
    /// </summary>
    public static readonly string ApiBaseUrl = "http://api.linkedin.com/v1";

    /// <summary>
    /// The name of the people resource.
    /// </summary>
    public static readonly string PeopleResourceName = "people";

    /// <summary>
    /// The name of the request token resource.
    /// </summary>
    public static readonly string RequestTokenResourceName = "requestToken";

    /// <summary>
    /// The name of the authorize method.
    /// </summary>
    public static readonly string AuthorizeTokenMethod = "authorize";

    /// <summary>
    /// The name of the authenticate method.
    /// </summary>
    public static readonly string AuthenticateMethod = "authenticate";

    /// <summary>
    /// The name of the access token resource.
    /// </summary>
    public static readonly string AccessTokenResourceName = "accessToken";

    /// <summary>
    /// The name of the invalidate token method.
    /// </summary>
    public static readonly string InvalidateTokenMethod = "invalidateToken";

    /// <summary>
    /// The name of the connections resource.
    /// </summary>
    public static readonly string ConnectionsResourceName = "connections";

    /// <summary>
    /// The name of the network resource.
    /// </summary>
    public static readonly string NetworkResourceName = "network";

    /// <summary>
    /// The name of the updates resource.
    /// </summary>
    public static readonly string UpdatesResourceName = "updates";

    /// <summary>
    /// The name of the network stats resource.
    /// </summary>
    public static readonly string NetworkStatsResourceName = "network-stats";
    
    /// <summary>
    /// The name of the current status resource.
    /// </summary>
    public static readonly string CurrentStatusResourceName = "current-status";

    /// <summary>
    /// The name of the Share resource.
    /// </summary>
    public static readonly string SharesResourceName = "shares";

    /// <summary>
    /// The name of the mailbox resource.
    /// </summary>
    public static readonly string MailboxResourceName = "mailbox";

    /// <summary>
    /// The name of the person activities resource.
    /// </summary>
    public static readonly string PersonActivitiesResourceName = "person-activities";

    /// <summary>
    /// The name of the people search resource.
    /// </summary>
    public static readonly string PeopleSearchResourceName = "people-search";

    /// <summary>
    /// The identifier for the current user.
    /// </summary>
    public static readonly string CurrentUserIdentifier = "~";

    /// <summary>
    /// The identifier for the a member.
    /// </summary>
    public static readonly string MemberIdIdentifierFormat = "id={0}";

    /// <summary>
    /// The identifier for a public profile.
    /// </summary>
    public static readonly string PublicProfileIdentifier = ":public";

    /// <summary>
    /// The identifier format for a network update.
    /// </summary>
    public static readonly string NetworkUpdateIdentifierFormat = "key={0}";

    /// <summary>
    /// The name of the update comments resource.
    /// </summary>
    public static readonly string UpdateCommentsResourceName = "update-comments";

    /// <summary>
    /// The name of the likes resource.
    /// </summary>
    public static readonly string LikesResourceName = "likes";

    /// <summary>
    /// The name of the is liked resource.
    /// </summary>
    public static readonly string IsLikedResourceName = "is-liked";

    /// <summary>
    /// The maximum length of a status.
    /// </summary>
    public static readonly int MaxStatusLength = 140;

    /// <summary>
    /// The content type for LinkedIn html.
    /// </summary>
    public static readonly string LinkedInHtmlContentType = "linkedin-html";

    /// <summary>
    /// The maximum number of network updates to return.
    /// </summary>
    public static readonly int MaxNumberOfNetworkUpdates = 1000;

    /// <summary>
    /// The maximum number of connections to return.
    /// </summary>
    public static readonly int MaxNumberOfConnections = 5000;

    /// <summary>
    /// The maximum length of a comment in a share.
    /// </summary>
    public static readonly int MaxShareCommentLength = 700;

    /// <summary>
    /// The maximum length of a title in a share.
    /// </summary>
    public static readonly int MaxShareTitleLength = 200;

    /// <summary>
    /// The maximum length of a description in a share.
    /// </summary>
    public static readonly int MaxShareDescriptionLength = 400;

    #region Parameters
    /// <summary>
    /// The count parameter name.
    /// </summary>
    public static readonly string CountParam = "count";

    /// <summary>
    /// The start parameter name.
    /// </summary>
    public static readonly string StartParam = "start";

    #region Connections parameters
    /// <summary>
    /// The modified parameter name.
    /// </summary>
    public static readonly string ModifiedParam = "modified";

    /// <summary>
    /// The modified since parameter name.
    /// </summary>
    public static readonly string ModifiedSinceParam = "modified-since";
    #endregion

    #region Search parameters
    /// <summary>
    /// The keywords parameter name.
    /// </summary>
    public static readonly string KeywordsParam = "keywords";

    /// <summary>
    /// The first name parameter name.
    /// </summary>
    public static readonly string FirstNameParam = "first-name";

    /// <summary>
    /// The last name parameter name.
    /// </summary>
    public static readonly string LastNameParam = "last-name";

    /// <summary>
    /// The company name parameter name.
    /// </summary>
    public static readonly string CompanyNameParam = "company-name";

    /// <summary>
    /// The current company parameter name.
    /// </summary>
    public static readonly string CurrentCompanyParam = "current-company";

    /// <summary>
    /// The title parameter name.
    /// </summary>
    public static readonly string TitleParam = "title";

    /// <summary>
    /// The current title parameter name.
    /// </summary>
    public static readonly string CurrentTitleParam = "current-title";

    /// <summary>
    /// The school name parameter name.
    /// </summary>
    public static readonly string SchoolNameParam = "school-name";

    /// <summary>
    /// The current school parameter name.
    /// </summary>
    public static readonly string CurrentSchoolParam = "current-school";

    /// <summary>
    /// The search location type parameter name.
    /// </summary>
    public static readonly string SearchLocationTypeParam = "search-location-type";

    /// <summary>
    /// The country code parameter name.
    /// </summary>
    public static readonly string CountryCodeParam = "country-code";

    /// <summary>
    /// The postal code parameter name.
    /// </summary>
    public static readonly string PostalCodeParam = "postal-code";

    /// <summary>
    /// The distance parameter name.
    /// </summary>
    public static readonly string DistanceParam = "distance";

    /// <summary>
    /// The sort parameter name.
    /// </summary>
    public static readonly string SortParam = "sort";

    /// <summary>
    /// The facet parameter name.
    /// </summary>
    public static readonly string FacetParam = "facet";

    /// <summary>
    /// The facets parameter name.
    /// </summary>
    public static readonly string FacetsParam = "facets";
    #endregion

    #region Share parameters
    /// <summary>
    /// The post twitter parameter name.
    /// </summary>
    public static readonly string PostTwitterParam = "twitter-post";
    #endregion

    #region Network Updates
    /// <summary>
    /// The type parameter name.
    /// </summary>
    public static readonly string TypeParam = "type";

    /// <summary>
    /// The after parameter name.
    /// </summary>
    public static readonly string AfterParam = "after";

    /// <summary>
    /// The before parameter name.
    /// </summary>
    public static readonly string BeforeParam = "before";

    /// <summary>
    /// The show hidden members parameter name.
    /// </summary>
    public static readonly string ShowHiddenMembersParam = "show-hidden-members";

    /// <summary>
    /// The scope parameter name.
    /// </summary>
    public static readonly string ScopeParam = "scope";
    #endregion
    #endregion

    #region Field Selectors
    /// <summary>
    /// The people field selector name.
    /// </summary>
    public static readonly string PeopleFieldSelector = "people";

    /// <summary>
    /// The location field selector name.
    /// </summary>
    public static readonly string LocationFieldSelector = "location";

    /// <summary>
    /// The relation to viewer field selector name.
    /// </summary>
    public static readonly string RelationToViewerFieldSelector = "relation-to-viewer";

    /// <summary>
    /// The member url resources field selector name.
    /// </summary>
    public static readonly string MemberUrlResourcesFieldSelector = "member-url-resources";

    /// <summary>
    /// The member url field selector name.
    /// </summary>
    public static readonly string MemberUrlFieldSelector = "member-url";

    /// <summary>
    /// The api standard profile request field selector name.
    /// </summary>
    public static readonly string ApiStandardProfileRequestFieldSelector = "api-standard-profile-request";

    /// <summary>
    /// The positions field selector name.
    /// </summary>
    public static readonly string PositionsFieldSelector = "positions";

    /// <summary>
    /// The educations field selector name.
    /// </summary>
    public static readonly string EducationsFieldSelector = "educations";

    /// <summary>
    /// The recommendations received field selector name.
    /// </summary>
    public static readonly string RecommendationsReceivedFieldSelector = "recommendations-received";

    /// <summary>
    /// The facets field selector name.
    /// </summary>
    public static readonly string FacetsFieldSelector = "facets";

    /// <summary>
    /// The buckets field selector name.
    /// </summary>
    public static readonly string BucketsFieldSelector = "buckets";
    #endregion
  }
}
