//-----------------------------------------------------------------------
// <copyright file="LinkedInService.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using LinkedIn.FieldSelectorConverters;
using LinkedIn.Properties;
using LinkedIn.ServiceEntities;
using LinkedIn.Utility;

namespace LinkedIn
{
  /// <summary>
  /// A service to access the LinkedIn API's.
  /// </summary>
  public class LinkedInService
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInService"/> class.
    /// </summary>
    /// <param name="authorization">The object that can send authorized requests.</param>
    public LinkedInService(ILinkedInAuthorization authorization)
    {
      this.Authorization = authorization;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the object that can send authorized requests.
    /// </summary>
    private ILinkedInAuthorization Authorization
    {
      get;
      set;
    }
    #endregion

    #region Profile API
    /// <summary>
    /// Retrieve the current user his profile.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1002
    /// </summary>
    /// <param name="profileType">The type of profile to retrieve.</param>
    /// <returns>A <see cref="Person"/> representing the current user.</returns>
    public Person GetCurrentUser(ProfileType profileType)
    {
      return GetCurrentUser(profileType, new List<ProfileField>());
    }

    /// <summary>
    /// Retrieve the current user his profile.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1002
    /// </summary>
    /// <param name="profileType">The type of profile to retrieve.</param>
    /// <param name="profileFields">A list of profile fields to retrieve.</param>
    /// <returns>A <see cref="Person"/> representing the current user.</returns>
    /// <exception cref="ArgumentException">When <paramref name="profileType"/> is Standard and 
    /// <paramref name="profileFields"/> contains SitePublicProfileRequestUrl.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="profileFields"/> is null.</exception>    
    public Person GetCurrentUser(ProfileType profileType, List<ProfileField> profileFields)
    {
      if (profileFields == null)
      {
        throw new ArgumentNullException("profileFields", string.Format(Resources.NotNullMessageFormat, "profileFields"));
      }

      if (profileType == ProfileType.Standard && profileFields.Contains(ProfileField.SitePublicProfileRequestUrl))
      {
        throw new ArgumentException(Resources.ProfileFieldsContainsSitePublicProfileRequest, "profileFields");
      }

      UriBuilder locationBaseUri = BuildApiUrlForCurrentUser(profileType);

      return GetProfile(locationBaseUri, profileFields);
    }

    /// <summary>
    /// Retrieve a profile for a member.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1002
    /// </summary>
    /// <param name="memberId">The identifier for the member.</param>
    /// <returns>A <see cref="Person"/> representing the member.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="memberId"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="memberId"/> is an empty string.</exception>
    /// <remarks>You cannot use a member id to get a public profile.</remarks>
    public Person GetProfileByMemberId(string memberId)
    {
      return GetProfileByMemberId(memberId, new List<ProfileField>());
    }

    /// <summary>
    /// Retrieve a profile for a member.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1002
    /// </summary>
    /// <param name="memberId">The identifier for the member.</param>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <returns>A <see cref="Person"/> representing the member.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="memberId"/> is null. -or-
    /// when <paramref name="profileFields"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="profileType"/> is Standard and 
    /// <paramref name="profileFields"/> contains SitePublicProfileRequestUrl. -or-
    /// when <paramref name="memberId"/> is an empty string.</exception>
    /// <remarks>You cannot use a member id to get a public profile.</remarks>
    public Person GetProfileByMemberId(string memberId, List<ProfileField> profileFields)
    {
      if (memberId == null)
      {
        throw new ArgumentNullException("memberId", string.Format(Resources.NotNullMessageFormat, "memberId"));
      }

      if (memberId == string.Empty)
      {
        throw new ArgumentException(string.Format(Resources.NotEmptyStringMessageFormat, "memberId"), "memberId");
      }

      if (profileFields == null)
      {
        throw new ArgumentNullException("profileFields", string.Format(Resources.NotNullMessageFormat, "profileFields"));
      }

      UriBuilder location = BuildApiUrlByMemberId(memberId);
      return GetProfile(location, profileFields);
    }

    /// <summary>
    /// Retrieve a collection of profiles for a list of member identifiers.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1002
    /// </summary>
    /// <param name="memberIds">The list of identifiers for the members.</param>
    /// <returns>A <see cref="People"/> object representing a collection of profiles.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="memberIds"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="memberIds"/> is an empty list.</exception>
    /// <remarks>You cannot use a member id to get a public profile.</remarks>
    public People GetProfilesByMemberIds(List<string> memberIds)
    {
      if (memberIds == null)
      {
        throw new ArgumentNullException("memberIds", string.Format(Resources.NotNullMessageFormat, "memberIds"));
      }

      if (memberIds.Count == 0)
      {
        throw new ArgumentException("memberIds", string.Format(Resources.NotEmptyMessageFormat, "memberIds"));
      }

      StringBuilder sb = new StringBuilder();
      foreach (string memberId in memberIds)
      {
        sb.AppendFormat(CultureInfo.InvariantCulture, Constants.MemberIdIdentifierFormat, memberId);
        sb.Append(",");
      }

      sb.Length--;

      UriBuilder location = BuildApiUrl(Constants.PeopleResourceName);
      location.Path = string.Format(CultureInfo.InvariantCulture, "{0}::({1})", location.Path, sb.ToString());
      
      return GetProfiles(location, new List<ProfileField>());
    }

    /// <summary>
    /// Retrieve a profile for a member.
    /// </summary>
    /// <param name="location">The uri represented by a <see cref="UriBuilder"/> object to append to.</param>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <returns>A <see cref="Person"/> representing the member.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="profileFields"/> is null.</exception>
    private Person GetProfile(UriBuilder location, List<ProfileField> profileFields)
    {
      if (profileFields == null)
      {
        throw new ArgumentNullException("profileFields", string.Format(Resources.NotNullMessageFormat, "profileFields"));
      }

      if (profileFields.Count > 0)
      {
        PeopleFieldSelectorConverter peopleFieldSelectorConverter = new PeopleFieldSelectorConverter(true);
        string listOfFields = peopleFieldSelectorConverter.ConvertToString(profileFields);

        location.Path = string.Format(CultureInfo.InvariantCulture, "{0}:({1})", location.Path, listOfFields);
      }

      WebRequest webRequest = this.Authorization.InitializeGetRequest(location.Uri);
      string xmlResponse = ProcessResponse(SendRequest(webRequest));
      return Utilities.DeserializeXml<Person>(xmlResponse);
    }

    /// <summary>
    /// Retrieve a collection of profiles for a list of member identifiers.
    /// </summary>
    /// <param name="location">The uri represented by a <see cref="UriBuilder"/> object to append to.</param>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <returns>A <see cref="People"/> object representing a collection of profiles.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="profileFields"/> is null.</exception>
    private People GetProfiles(UriBuilder location, List<ProfileField> profileFields)
    {
      if (profileFields == null)
      {
        throw new ArgumentNullException("profileFields", string.Format(Resources.NotNullMessageFormat, "profileFields"));
      }

      if (profileFields.Count > 0)
      {
        PeopleFieldSelectorConverter peopleFieldSelectorConverter = new PeopleFieldSelectorConverter(true);
        string listOfFields = peopleFieldSelectorConverter.ConvertToString(profileFields);

        location.Path = string.Format(CultureInfo.InvariantCulture, "{0}:({1})", location.Path, listOfFields);
      }

      WebRequest webRequest = this.Authorization.InitializeGetRequest(location.Uri);
      string xmlResponse = ProcessResponse(SendRequest(webRequest));
      return Utilities.DeserializeXml<People>(xmlResponse);
    }
    #endregion

    #region Connections API
    /// <summary>
    /// Retrieve the connections for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1004
    /// </summary>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    public Connections GetConnectionsForCurrentUser()
    {
      return GetConnectionsForCurrentUser(new List<ProfileField>(), -1, -1, Modified.Updated, 1);
    }

    /// <summary>
    /// Retrieve the connections for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1004
    /// </summary>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <param name="modified">Which kind of modification shoud be returned.</param>
    /// <param name="modifiedSince">Time since the connections are modified (in milliseconds since epoch).</param>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="modifiedSince"/> is not a valid timestamp.</exception>
    public Connections GetConnectionsForCurrentUser(List<ProfileField> profileFields, Modified modified, int modifiedSince)
    {
      if (modifiedSince <= 0)
      {
        throw new ArgumentOutOfRangeException("modifiedSince", string.Format(Resources.TimeStampOutOfRangeMessageFormat, "modifiedSince"));
      }

      UriBuilder location = BuildApiUrlForCurrentUser(Constants.ConnectionsResourceName);
      return GetConnections(location, profileFields, -1, -1, modified, modifiedSince);
    }

    /// <summary>
    /// Retrieve the connections for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1004
    /// </summary>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="count">Number of results to return.</param>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    public Connections GetConnectionsForCurrentUser(List<ProfileField> profileFields, int start, int count)
    {
      UriBuilder location = BuildApiUrlForCurrentUser(Constants.ConnectionsResourceName);
      return GetConnections(location, profileFields, start, count, Modified.All, 1);
    }

    /// <summary>
    /// Retrieve the connections for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1004
    /// </summary>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="count">Number of results to return.</param>
    /// <param name="modified">Which kind of modification shoud be returned.</param>
    /// <param name="modifiedSince">Time since the connections are modified (in milliseconds since epoch).</param>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="modifiedSince"/> is not a valid timestamp.</exception>
    public Connections GetConnectionsForCurrentUser(List<ProfileField> profileFields, int start, int count, Modified modified, int modifiedSince)
    {
      if (modifiedSince <= 0)
      {
        throw new ArgumentOutOfRangeException("modifiedSince", string.Format(Resources.TimeStampOutOfRangeMessageFormat, "modifiedSince"));
      }

      UriBuilder location = BuildApiUrlForCurrentUser(Constants.ConnectionsResourceName);
      return GetConnections(location, profileFields, start, count, modified, modifiedSince);
    }

    /// <summary>
    /// Retrieve the connections for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1004
    /// </summary>
    /// <param name="memberId">The identifier for the member.</param>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="memberId"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="memberId"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="modifiedSince"/> is not a valid timestamp.</exception>
    public Connections GetConnectionsByMemberId(string memberId)
    {
      return GetConnectionsByMemberId(memberId, null, -1, -1);
    }

    /// <summary>
    /// Retrieve the connections for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1004
    /// </summary>
    /// <param name="memberId">The identifier for the member.</param>
    /// <param name="modified">Which kind of modification shoud be returned.</param>
    /// <param name="modifiedSince">Time since the connections are modified (in milliseconds since epoch).</param>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="memberId"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="memberId"/> is an empty string.</exception>
    public Connections GetConnectionsByMemberId(string memberId, Modified modified, int modifiedSince)
    {
      if (modifiedSince <= 0)
      {
        throw new ArgumentOutOfRangeException("modifiedSince", string.Format(Resources.TimeStampOutOfRangeMessageFormat, "modifiedSince"));
      }

      return GetConnectionsByMemberId(memberId, null, -1, -1, modified, modifiedSince);
    }

    /// <summary>
    /// Retrieve the connections a member.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1004
    /// </summary>
    /// <param name="memberId">The identifier for the member.</param>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="count">Number of results to return.</param>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="memberId"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="memberId"/> is an empty string.</exception>
    public Connections GetConnectionsByMemberId(string memberId, List<ProfileField> profileFields, int start, int count)
    {
      return GetConnectionsByMemberId(memberId, profileFields, start, count, Modified.All, 1);
    }

    /// <summary>
    /// Retrieve the connections a member.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1004
    /// </summary>
    /// <param name="memberId">The identifier for the member.</param>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="count">Number of results to return.</param>
    /// <param name="modified">Which kind of modification shoud be returned.</param>
    /// <param name="modifiedSince">Time since the connections are modified (in milliseconds since epoch).</param>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="memberId"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="memberId"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="modifiedSince"/> is not a valid timestamp.</exception>
    public Connections GetConnectionsByMemberId(string memberId, List<ProfileField> profileFields, int start, int count, Modified modified, int modifiedSince)
    {
      if (memberId == null)
      {
        throw new ArgumentNullException("memberId", string.Format(Resources.NotNullMessageFormat, "memberId"));
      }

      if (memberId == string.Empty)
      {
        throw new ArgumentException(string.Format(Resources.NotEmptyStringMessageFormat, "memberId"), "memberId");
      }

      if (modifiedSince <= 0)
      {
        throw new ArgumentOutOfRangeException("modifiedSince", string.Format(Resources.TimeStampOutOfRangeMessageFormat, "modifiedSince"));
      }

      UriBuilder location = BuildApiUrlByMemberId(memberId, Constants.ConnectionsResourceName);
      return GetConnections(location, profileFields, start, count, modified, modifiedSince);
    }

    /// <summary>
    /// Retrieve the connections a member.
    /// </summary>
    /// <param name="location">The uri represented by a <see cref="UriBuilder"/> object to append to.</param>
    /// <param name="profileFields">A list of Profile fields to retrieve.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="count">Number of results to return.</param>
    /// <param name="modified">Which kind of modification shoud be returned.</param>
    /// <param name="modifiedSince">Time since the connections are modified (in milliseconds since epoch).</param>
    /// <returns>A <see cref="Connections"/> object representing the connections.</returns>
    /// <exception cref="ArgumentException">When <paramref name="profileFields"/> contains SitePublicProfileRequestUrl.</exception>
    private Connections GetConnections(UriBuilder location, List<ProfileField> profileFields, int start, int count, Modified modified, int modifiedSince)
    {
      if (profileFields.Contains(ProfileField.SitePublicProfileRequestUrl))
      {
        throw new ArgumentException(Resources.ProfileFieldsContainsSitePublicProfileRequest, "profileFields");
      }

      if (profileFields.Contains(ProfileField.Associations) ||
        profileFields.Contains(ProfileField.Connections) ||
        profileFields.Contains(ProfileField.Honors) ||
        profileFields.Contains(ProfileField.ProposalComments) ||
        profileFields.Contains(ProfileField.Specialties) ||
        profileFields.Contains(ProfileField.Summary) ||
        profileFields.Contains(ProfileField.RelationToViewerDistance) ||
        profileFields.Contains(ProfileField.RelationToViewerNumberOfRelatedConnections) ||
        profileFields.Contains(ProfileField.RelationToViewerRelatedConnections))
      {
        throw new ArgumentException(Resources.ProfileFieldsContainsInvalidCollectionFields, "profileFields");
      }

      if (profileFields != null && profileFields.Count > 0)
      {
        PeopleFieldSelectorConverter peopleFieldSelectorConverter = new PeopleFieldSelectorConverter(true);
        string listOfFields = peopleFieldSelectorConverter.ConvertToString(profileFields);

        location.Path = string.Format(CultureInfo.InvariantCulture, "{0}:({1})", location.Path, listOfFields);
      }

      QueryStringParameters queryStringParameters = new QueryStringParameters();
      queryStringParameters.Add(Constants.StartParam, start);
      queryStringParameters.Add(Constants.CountParam, count);
      if (modified != Modified.All)
      {
        queryStringParameters.Add(Constants.ModifiedParam, EnumHelper.GetDescription(modified));
        queryStringParameters.Add(Constants.ModifiedSinceParam, modifiedSince);
      }

      location = queryStringParameters.AppendToUri(location);

      WebRequest webRequest = this.Authorization.InitializeGetRequest(location.Uri);
      string xmlResponse = this.ProcessResponse(SendRequest(webRequest));
      return Utilities.DeserializeXml<Connections>(xmlResponse);
    }
    #endregion

    #region Out of Network Profiles
    /// <summary>
    /// Get a 'out of network' profile.
    /// <para/>
    /// For more info see: http://developer.linkedin.com/docs/DOC-1160
    /// </summary>
    /// <param name="apiRequest">The api information for the request.</param>
    /// <returns>A <see cref="Person"/> representing the profile.</returns>
    /// <exception cref="ArgumentException">When the url in <paramref name="apiRequest"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="apiRequest"/> is null.</exception>
    public Person GetOutOfNetworkProfile(ApiRequest apiRequest)
    {
      if (apiRequest == null)
      {
        throw new ArgumentNullException("apiRequest", string.Format(Resources.NotNullMessageFormat, "apiRequest"));
      }

      if (string.IsNullOrEmpty(apiRequest.Url))
      {
        throw new ArgumentException("apiRequest", Resources.InvalidUrlApiRequestMessage);
      }

      Uri requestUri = null;
      try
      {
        requestUri = new Uri(apiRequest.Url);
      }
      catch (UriFormatException)
      {
        throw new ArgumentException("apiRequest", Resources.InvalidUrlApiRequestMessage);
      }

      return GetOutOfNetworkProfile(requestUri, apiRequest.Headers);
    }

    /// <summary>
    /// Get a 'out of network' profile.
    /// <para/>
    /// For more info see: http://developer.linkedin.com/docs/DOC-1160
    /// </summary>
    /// <param name="requestUri">The request uri for this profile.</param>
    /// <param name="httpHeaders">The request headers for this profile.</param>
    /// <returns>A <see cref="Person"/> representing the profile.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="requestUri"/> is null. -or-
    /// when <paramref name="httpHeaders"/> is null.</exception>
    public Person GetOutOfNetworkProfile(Uri requestUri, IEnumerable<HttpHeader> httpHeaders)
    {
      if (requestUri == null)
      {
        throw new ArgumentNullException("requestUri", string.Format(Resources.NotNullMessageFormat, "requestUri"));
      }

      if (httpHeaders == null)
      {
        throw new ArgumentNullException("httpHeaders", string.Format(Resources.NotNullMessageFormat, "httpHeaders"));
      }

      WebRequest webRequest = this.Authorization.InitializeGetRequest(requestUri);
      foreach (HttpHeader httpHeader in httpHeaders)
      {
        webRequest.Headers.Add(httpHeader.Name, httpHeader.Value);
      }

      string xmlResponse = ProcessResponse(SendRequest(webRequest));
      return Utilities.DeserializeXml<Person>(xmlResponse);
    }
    #endregion

    #region Network Updates API
    /// <summary>
    /// Retrieve the Network Updates for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1006
    /// </summary>
    /// <param name="updateType">The type of Network Updates to retrieve.</param>
    /// <returns>A <see cref="Network"/> object representing the Network Updates.</returns>
    public Updates GetNetworkUpdates(NetworkUpdateTypes updateType)
    {
      return GetNetworkUpdates(updateType, Constants.MaxNumberOfNetworkUpdates, 0, DateTime.MinValue, DateTime.MinValue, false);
    }

    /// <summary>
    /// Retrieve the Network Updates for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1006
    /// </summary>
    /// <param name="updateType">The type of Network Updates to retrieve.</param>
    /// <param name="scope">The scope of the network updates (the current user his updates or the aggregated network updates).</param>
    /// <returns>A <see cref="Network"/> object representing the Network Updates.</returns>
    public Updates GetNetworkUpdates(NetworkUpdateTypes updateType, Scope scope)
    {
      return GetNetworkUpdates(updateType, Constants.MaxNumberOfNetworkUpdates, 0, DateTime.MinValue, DateTime.MinValue, false, scope);
    }

    /// <summary>
    /// Retrieve the Network Updates for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1006
    /// </summary>
    /// <param name="updateType">The type of Network Updates to retrieve.</param>
    /// <param name="after">The <see cref="DateTime"/> after which to retrieve updates for.</param>
    /// <param name="before">The <see cref="DateTime"/> before which to retrieve updates for.</param>
    /// <returns>A <see cref="Network"/> object representing the Network Updates.</returns>
    public Updates GetNetworkUpdates(NetworkUpdateTypes updateType, DateTime after, DateTime before)
    {
      return GetNetworkUpdates(updateType, Constants.MaxNumberOfNetworkUpdates, 0, after, before, false);
    }

    /// <summary>
    /// Retrieve the Network Updates for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1006
    /// </summary>
    /// <param name="updateTypes">The type(s) of Network Updates to retrieve.</param>
    /// <param name="count">Number of results to return.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="after">The <see cref="DateTime"/> after which to retrieve updates for.</param>
    /// <param name="before">The <see cref="DateTime"/> before which to retrieve updates for.</param>
    /// <returns>A <see cref="Network"/> object representing the Network Updates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is smaller then zero. -or-
    /// when <paramref name="start"/> is smaller then zero.</exception>
    public Updates GetNetworkUpdates(NetworkUpdateTypes updateTypes, int count, int start, DateTime after, DateTime before)
    {
      return GetNetworkUpdates(updateTypes, count, start, after, before, false);
    }

    /// <summary>
    /// Retrieve the Network Updates for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1006
    /// </summary>
    /// <param name="updateTypes">The type(s) of Network Updates to retrieve.</param>
    /// <param name="count">Number of results to return.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="after">The <see cref="DateTime"/> after which to retrieve updates for.</param>
    /// <param name="before">The <see cref="DateTime"/> before which to retrieve updates for.</param>
    /// <param name="showHiddenMembers">Whether to display updates from people the member has chosen to "hide" from their update stream.</param>
    /// <returns>A <see cref="Network"/> object representing the Network Updates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is smaller then zero. -or-
    /// when <paramref name="start"/> is smaller then zero.</exception>
    public Updates GetNetworkUpdates(NetworkUpdateTypes updateTypes, int count, int start, DateTime after, DateTime before, bool showHiddenMembers)
    {
      return GetNetworkUpdates(updateTypes, count, start, after, before, showHiddenMembers, Scope.Connections);
    }

    /// <summary>
    /// Retrieve the Network Updates for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1006
    /// </summary>
    /// <param name="updateTypes">The type(s) of Network Updates to retrieve.</param>
    /// <param name="count">Number of results to return.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="after">The <see cref="DateTime"/> after which to retrieve updates for.</param>
    /// <param name="before">The <see cref="DateTime"/> before which to retrieve updates for.</param>
    /// <param name="showHiddenMembers">Whether to display updates from people the member has chosen to "hide" from their update stream.</param>
    /// <param name="scope">The scope of the network updates (the current user his updates or the aggregated network updates).</param>
    /// <returns>A <see cref="Network"/> object representing the Network Updates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is smaller then zero. -or-
    /// when <paramref name="start"/> is smaller then zero.</exception>
    public Updates GetNetworkUpdates(NetworkUpdateTypes updateTypes, int count, int start, DateTime after, DateTime before, bool showHiddenMembers, Scope scope)
    {
      if (count < 0)
      {
        throw new ArgumentOutOfRangeException("count", Resources.CountOutOfRangeMessage);
      }

      if (start < 0)
      {
        throw new ArgumentOutOfRangeException("start", Resources.StartOutOfRangeMessage);
      }

      QueryStringParameters parameters = new QueryStringParameters();

      if (EnumHelper.HasFlag<NetworkUpdateTypes>(updateTypes, NetworkUpdateTypes.All) == false)
      {
        var networkUpdateTypeValues = Enum.GetValues(typeof(NetworkUpdateTypes));
        foreach (NetworkUpdateTypes value in networkUpdateTypeValues)
        {
          if (EnumHelper.HasFlag(updateTypes, value))
          {
            parameters.Add(Constants.TypeParam, EnumHelper.GetDescription(value));
          }
        }
      }

      parameters.Add(Constants.CountParam, count);
      parameters.Add(Constants.StartParam, start);
      if (before != DateTime.MinValue)
      {
        parameters.Add(Constants.BeforeParam, Utilities.GenerateTimestamp(before));
      }

      if (after != DateTime.MinValue)
      {
        parameters.Add(Constants.AfterParam, Utilities.GenerateTimestamp(after));
      }

      parameters.Add(Constants.ShowHiddenMembersParam, showHiddenMembers);

      if (scope == Scope.Self)
      {
        parameters.Add(Constants.ScopeParam, EnumHelper.GetDescription(scope));
      }

      Collection<Resource> resources = new Collection<Resource>
      {
        new Resource { Name = Constants.NetworkResourceName },
        new Resource { Name = Constants.UpdatesResourceName }
      };
      UriBuilder location = BuildApiUrlForCurrentUser(
        ProfileType.Standard,
        resources,
        parameters);

      WebRequest webRequest = this.Authorization.InitializeGetRequest(location.Uri);
      string xmlResponse = ProcessResponse(SendRequest(webRequest));
      return Utilities.DeserializeXml<Updates>(xmlResponse);
    }

    /// <summary>
    /// Retrieve network statistics (e.g. how many connections they have one and two degrees away).
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1006
    /// </summary>
    /// <returns>A <see cref="NetworkStats"/> object representing the network statistics.</returns>
    public NetworkStats GetNetworkStatistics()
    {
      Collection<Resource> resources = new Collection<Resource>
      {
        new Resource { Name = Constants.NetworkResourceName },
        new Resource { Name = Constants.NetworkStatsResourceName }
      };
      UriBuilder location = BuildApiUrlForCurrentUser(
        ProfileType.Standard,
        resources,
        null);

      WebRequest webRequest = this.Authorization.InitializeGetRequest(location.Uri);
      string xmlResponse = ProcessResponse(SendRequest(webRequest));
      return Utilities.DeserializeXml<NetworkStats>(xmlResponse);
    }

    /// <summary>
    /// Comment on a network update.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1006
    /// </summary>
    /// <param name="updateKey">The identifier of a network update.</param>
    /// <param name="comment">The actual comment.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="updateKey"/> is null. -or-
    /// when <paramref name="comment"/> is null.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="updateKey"/> is an empty string. -or-
    /// when <paramref name="comment"/> is an empty string.</exception>
    public bool CommentOnNetworkUpdate(string updateKey, string comment)
    {
      if (updateKey == null)
      {
        throw new ArgumentNullException("updateKey", string.Format(Resources.NotNullMessageFormat, "updateKey"));
      }

      if (updateKey == string.Empty)
      {
        throw new ArgumentException(string.Format(Resources.NotEmptyStringMessageFormat, "updateKey"), "updateKey");
      }

      if (comment == null)
      {
        throw new ArgumentNullException("comment", string.Format(Resources.NotNullMessageFormat, "comment"));
      }

      if (comment == string.Empty)
      {
        throw new ArgumentException(string.Format(Resources.NotEmptyStringMessageFormat, "comment"), "comment");
      }

      if (comment.Length > Constants.MaxShareCommentLength)
      {
        throw new ArgumentOutOfRangeException("comment", string.Format(Resources.MaxLengthMessageFormat, "Comment", Constants.MaxShareCommentLength));
      }

      UpdateComment updateComment = new UpdateComment
      {
        Comment = comment
      };

      string identifier = string.Format(Constants.NetworkUpdateIdentifierFormat, updateKey);

      UriBuilder location = BuildApiUrlForCurrentUser(new Collection<Resource> 
        {
          new Resource { Name = Constants.NetworkResourceName },
          new Resource { Name = Constants.UpdatesResourceName, Identifier = identifier },
          new Resource { Name = Constants.UpdateCommentsResourceName }
        });

      WebRequest webRequest = this.Authorization.InitializePostRequest(location.Uri);
      webRequest = InitializeRequest<UpdateComment>(webRequest, updateComment);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);
      ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    /// Like a network update.
    /// </summary>
    /// <param name="updateKey">The identifier of a network update.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="updateKey"/> is null.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="updateKey"/> is an empty string.</exception>
    public bool LikeNetworkUpdate(string updateKey)
    {
      if (updateKey == null)
      {
        throw new ArgumentNullException("updateKey", string.Format(Resources.NotNullMessageFormat, "updateKey"));
      }

      if (updateKey == string.Empty)
      {
        throw new ArgumentException(string.Format(Resources.NotEmptyStringMessageFormat, "updateKey"), "updateKey");
      }

      IsLiked isLiked = new IsLiked
      {
        IsNetworkUpdateLiked = true
      };

      string identifier = string.Format(Constants.NetworkUpdateIdentifierFormat, updateKey);

      UriBuilder location = BuildApiUrlForCurrentUser(new Collection<Resource> 
        {
          new Resource { Name = Constants.NetworkResourceName },
          new Resource { Name = Constants.UpdatesResourceName, Identifier = identifier },
          new Resource { Name = Constants.IsLikedResourceName }
        });

      WebRequest webRequest = this.Authorization.InitializePutRequest(location.Uri);
      webRequest = InitializeRequest<IsLiked>(webRequest, isLiked);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);
      ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    /// Unlike a network update.
    /// </summary>
    /// <param name="updateKey">The identifier of a network update.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="updateKey"/> is null.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="updateKey"/> is an empty string.</exception>
    public bool UnlikeNetworkUpdate(string updateKey)
    {
      if (updateKey == null)
      {
        throw new ArgumentNullException("updateKey", string.Format(Resources.NotNullMessageFormat, "updateKey"));
      }

      if (updateKey == string.Empty)
      {
        throw new ArgumentException(string.Format(Resources.NotEmptyStringMessageFormat, "updateKey"), "updateKey");
      }

      IsLiked isLiked = new IsLiked
      {
        IsNetworkUpdateLiked = false
      };

      string identifier = string.Format(Constants.NetworkUpdateIdentifierFormat, updateKey);

      UriBuilder location = BuildApiUrlForCurrentUser(new Collection<Resource> 
        {
          new Resource { Name = Constants.NetworkResourceName },
          new Resource { Name = Constants.UpdatesResourceName, Identifier = identifier },
          new Resource { Name = Constants.IsLikedResourceName }
        });

      WebRequest webRequest = this.Authorization.InitializePutRequest(location.Uri);
      webRequest = InitializeRequest<IsLiked>(webRequest, isLiked);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);
      ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.Created;
    }
    #endregion

    #region Search API
    /// <summary>
    /// Search for members.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1191
    /// </summary>
    /// <param name="keywords">The keywords to search for.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="count">Number of results to return.</param>
    /// <returns>A <see cref="People"/> object representing the search result.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is smaller then zero or larger then 25.</exception>
    public PeopleSearch Search(string keywords, int start, int count)
    {
      return Search(
        keywords,
        string.Empty,
        string.Empty,
        string.Empty,
        false,
        string.Empty,
        false,
        string.Empty,
        false,
        string.Empty,
        string.Empty,
        -1,
        SortCriteria.Connections,
        start,
        count,
        null,
        null,
        null);
    }

    /// <summary>
    /// Search for members.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1191
    /// </summary>
    /// <param name="keywords">The keywords to search for.</param>
    /// <param name="firstName">The first name to search for.</param>
    /// <param name="lastName">The last name to search for.</param>
    /// <param name="companyName">The company name to search for.</param>
    /// <param name="currentCompany">Whether the company to search for is the current company.</param>
    /// <param name="sortCriteria">The sort criteria for the search results.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="count">Number of results to return.</param>
    /// <param name="profileFields">A list of profile fields to retrieve.</param>
    /// <returns>A <see cref="People"/> object representing the search result.</returns>
    /// <exception cref="ArgumentException">When <paramref name="profileFields"/> contains SitePublicProfileRequestUrl.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is smaller then zero or larger then 25.</exception>
    public PeopleSearch Search(
      string keywords,
      string firstName,
      string lastName,
      string companyName,
      bool currentCompany,
      SortCriteria sortCriteria,
      int start,
      int count,
      Collection<ProfileField> profileFields)
    {
      return Search(
        keywords,
        firstName,
        lastName,
        companyName,
        currentCompany,
        string.Empty,
        false,
        string.Empty,
        false,
        string.Empty,
        string.Empty,
        -1,
        sortCriteria,
        start,
        count,
        profileFields,
        null,
        null);
    }

    /// <summary>
    /// Search for members.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1191
    /// </summary>
    /// <param name="keywords">The keywords to search for.</param>
    /// <param name="firstName">The first name to search for.</param>
    /// <param name="lastName">The last name to search for.</param>
    /// <param name="companyName">The company name to search for.</param>
    /// <param name="currentCompany">Whether the company to search for is the current company.</param>
    /// <param name="title">The title to search for.</param>
    /// <param name="currentTitle">Whether the title to search for is the current title.</param>
    /// <param name="schoolName">The school name to search for.</param>
    /// <param name="currentSchool">Whether the school to search for is the current school.</param>
    /// <param name="countryCode">The country code to search for.</param>
    /// <param name="postalCode">The postal code to search for. (Not supported for all countries.)</param>
    /// <param name="distance">The distrance from a central point in miles. (This is best used in combination with both countryCode and postalCode.)</param>
    /// <param name="sortCriteria">The sort criteria for the search results.</param>
    /// <param name="start">Starting location within the result set for paginated returns.</param>
    /// <param name="count">Number of results to return.</param>
    /// <param name="profileFields">A list of profile fields to retrieve.</param>
    /// <param name="facetFields">A list of facet fields to retrieve.</param>
    /// <param name="facets">A list of facet to refine the search results.</param>
    /// <returns>A list of <see cref="Person"/> objects representing the members.</returns>
    /// <exception cref="ArgumentException">When <paramref name="postalCode"/> is provided and 
    /// <paramref name="countryCode"/> is null or empty. -or-
    /// when <paramref name="profileFields"/> contains SitePublicProfileRequestUrl.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is smaller then zero or larger then 25.</exception>
    public PeopleSearch Search(
      string keywords,
      string firstName,
      string lastName,
      string companyName,
      bool currentCompany,
      string title,
      bool currentTitle,
      string schoolName,
      bool currentSchool,
      string countryCode,
      string postalCode,
      int distance,
      SortCriteria sortCriteria,
      int start,
      int count,
      Collection<ProfileField> profileFields,
      Collection<FacetField> facetFields,
      Dictionary<FacetCode, Collection<string>> facets)
    {
      // TODO: Country code and postal code validation
      if (string.IsNullOrEmpty(postalCode) == false && string.IsNullOrEmpty(countryCode))
      {
        throw new ArgumentException(Resources.CountryCodeArgumentMessage, "countryCode");
      }

      if (count < 1 || count > 25)
      {
        throw new ArgumentOutOfRangeException("count", Resources.SearchCountOutOfRangeMessage);
      }

      if (profileFields.Contains(ProfileField.SitePublicProfileRequestUrl))
      {
        throw new ArgumentException(Resources.ProfileFieldsContainsSitePublicProfileRequest, "profileFields");
      }

      if (string.IsNullOrEmpty(countryCode) == false)
      {
        countryCode = countryCode.ToLowerInvariant();
      }

      QueryStringParameters parameters = new QueryStringParameters();
      parameters.Add(Constants.KeywordsParam, keywords.Replace(" ", "+"));
      parameters.Add(Constants.FirstNameParam, firstName.Replace(" ", "+"));
      parameters.Add(Constants.LastNameParam, lastName.Replace(" ", "+"));
      if (string.IsNullOrEmpty(companyName) == false)
      {
        parameters.Add(Constants.CompanyNameParam, companyName);
        parameters.Add(Constants.CurrentCompanyParam, currentCompany);
      }

      if (string.IsNullOrEmpty(title) == false)
      {
        parameters.Add(Constants.TitleParam, title);
        parameters.Add(Constants.CurrentTitleParam, currentTitle);
      }

      if (string.IsNullOrEmpty(title) == false)
      {
        parameters.Add(Constants.SchoolNameParam, schoolName);
        parameters.Add(Constants.CurrentSchoolParam, currentSchool);
      }

      parameters.Add(Constants.CountryCodeParam, countryCode);
      parameters.Add(Constants.PostalCodeParam, postalCode);
      parameters.Add(Constants.DistanceParam, distance);

      parameters.Add(Constants.SortParam, EnumHelper.GetDescription(sortCriteria));
      parameters.Add(Constants.StartParam, start);
      parameters.Add(Constants.CountParam, count);

      if (facets != null && facets.Count > 0)
      {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<FacetCode, Collection<string>> pair in facets)
        {
          sb.Append(EnumHelper.GetDescription(pair.Key));
          sb.Append(",");

          // Only search for a facet is the value isn't empty
          if (pair.Value != null && pair.Value.Count > 0)
          {
            StringBuilder facetValue = new StringBuilder();
            foreach (string value in pair.Value)
            {
              facetValue.Append(value);
              facetValue.Append(",");
            }

            facetValue.Length--;

            string facetParameterValue = string.Format("{0},{1}", EnumHelper.GetDescription(pair.Key), facetValue.ToString());
            parameters.Add(Constants.FacetParam, facetParameterValue);
          }
        }

        sb.Length--;

        parameters.Add(Constants.FacetsParam, sb.ToString());
      }

      UriBuilder location = BuildApiUrl(Constants.PeopleSearchResourceName, parameters);

      if (profileFields != null)
      {
        PeopleFieldSelectorConverter peopleFieldSelectorConverter = new PeopleFieldSelectorConverter();
        string listOfFields = peopleFieldSelectorConverter.ConvertToString(profileFields);

        if (facetFields != null)
        {
            FacetFieldSelectorConverter ffsc = new FacetFieldSelectorConverter();
            string listOfFacetFields = ffsc.ConvertToString(facetFields);
            if (string.IsNullOrEmpty(listOfFacetFields) == false)
            {
                listOfFields += string.Format(",{0}", listOfFacetFields);
            }
        }
        location.Path = string.Format(CultureInfo.InvariantCulture, "{0}:({1})", location.Path, listOfFields);
      }

      WebRequest webRequest = this.Authorization.InitializeGetRequest(location.Uri);
      string xmlResponse = ProcessResponse(SendRequest(webRequest));
      return Utilities.DeserializeXml<PeopleSearch>(xmlResponse);
    }
    #endregion

    #region Status Update API
    /// <summary>
    /// Update the status of the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1007
    /// </summary>
    /// <param name="status">The new status.</param>
    [Obsolete("Please use the Share API")]
    public void UpdateStatus(string status)
    {
      if (string.IsNullOrEmpty(status))
      {
        DeleteStatus();
      }
      else if (status.Length > Constants.MaxStatusLength)
      {
        throw new ArgumentOutOfRangeException("status", Resources.StatusOutOfRangeMessage);
      }
      else
      {
        UriBuilder location = BuildApiUrlForCurrentUser(Constants.CurrentStatusResourceName);

        CurrentStatus currentStatus = new CurrentStatus();
        currentStatus.Status = status;

        WebRequest webRequest = this.Authorization.InitializePutRequest(location.Uri);
        webRequest = InitializeRequest<CurrentStatus>(webRequest, currentStatus);
        string xmlResponse = ProcessResponse(SendRequest(webRequest));
      }
    }

    /// <summary>
    /// Delete the current user his status.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1007
    /// </summary>
    [Obsolete("Please use the Share API")]
    public void DeleteStatus()
    {
      UriBuilder location = BuildApiUrlForCurrentUser(Constants.CurrentStatusResourceName);
      WebRequest webRequest = this.Authorization.InitializeDeleteRequest(location.Uri);
      string xmlResponse = ProcessResponse(SendRequest(webRequest));
    }
    #endregion

    #region Share API
    /// <summary>
    /// Create a new share for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1212
    /// </summary>
    /// <param name="comment">The comment to share.</param>
    /// <param name="visibility">The visibility of the new share.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="comment"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="comment"/> is an empty string. -or-
    /// when <paramref name="visibility" /> is unknown.</exception>
    public bool CreateShare(string comment, VisibilityCode visibility)
    {
      if (comment == null)
      {
        throw new ArgumentNullException("comment", string.Format(Resources.NotNullMessageFormat, "comment"));
      }

      if (comment == string.Empty)
      {
        throw new ArgumentException(string.Format(Resources.NotEmptyStringMessageFormat, "comment"), "comment");
      }

      return CreateShare(comment, string.Empty, string.Empty, null, null, visibility, false);
    }

    /// <summary>
    /// Create a new share for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1212
    /// </summary>
    /// <param name="comment">The comment to share.</param>
    /// <param name="title">The title of the share.</param>
    /// <param name="description">The description of the share.</param>
    /// <param name="uri">The uri to share.</param>
    /// <param name="imageUri">The uri of the image to share.</param>
    /// <param name="visibility">The visibility of the new share.</param>
    /// <param name="postOnTwitter">Whether to post this share on Twitter.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="comment"/> and <paramref name=""/>
    /// a combination of a title and submitted url is null or empty. -or-
    /// when <paramref name="visibility" /> is unknown.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="comment"/> is longer then 700 characters. -or-
    /// when <paramref name="title"/> is longer then 200 characters. -or-
    /// when <paramref name="description"/> is longer then 400 characters.</exception>
    public bool CreateShare(string comment, string title, string description, Uri uri, Uri imageUri, VisibilityCode visibility, bool postOnTwitter)
    {
      if (string.IsNullOrEmpty(comment) &&
        (string.IsNullOrEmpty(title) || uri == null))
      {
        throw new ArgumentException(Resources.InvalidCreateShareArguments);
      }

      if (comment != null && comment.Length > Constants.MaxShareCommentLength)
      {
        throw new ArgumentOutOfRangeException("comment", string.Format(Resources.MaxLengthMessageFormat, "Comment", Constants.MaxShareCommentLength));
      }

      if (title != null && title.Length > Constants.MaxShareTitleLength)
      {
        throw new ArgumentOutOfRangeException("title", string.Format(Resources.MaxLengthMessageFormat, "title", Constants.MaxShareTitleLength));
      }

      if (description != null && description.Length > Constants.MaxShareDescriptionLength)
      {
        throw new ArgumentOutOfRangeException("description", string.Format(Resources.MaxLengthMessageFormat, "description", Constants.MaxShareDescriptionLength));
      }

      if (visibility == VisibilityCode.Unknown)
      {
        throw new ArgumentException(Resources.InvalidVisibilityCode, "visibilityCode");
      }

      Share share = new Share(comment, title, description, visibility);

      if (uri != null)
      {
        share.Content.SubmittedUrl = uri.ToString();
      }

      if (imageUri != null)
      {
        share.Content.SubmittedImageUrl = imageUri.ToString();
      }
      
      QueryStringParameters queryStringParameters = new QueryStringParameters();
      if (postOnTwitter)
      {
        queryStringParameters.Add(Constants.PostTwitterParam, Boolean.TrueString.ToLowerInvariant());
      }

      UriBuilder location = BuildApiUrlForCurrentUser(Constants.SharesResourceName, queryStringParameters);

      WebRequest webRequest = this.Authorization.InitializePostRequest(location.Uri);
      webRequest = InitializeRequest<Share>(webRequest, share);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);
      ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    /// Create a rehare for the current user.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1212
    /// </summary>
    /// <param name="comment">The comment to include with the reshare.</param>
    /// <param name="shareId">The identifier of the share to reshare.</param>
    /// <param name="visibility">The visibility of the reshare.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="shareId"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="shareId"/> is an empty string. -or-
    /// When <paramref name="visibility" /> is unknown.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="comment"/> is longer then 700 characters.</exception>
    public bool CreateReShare(string comment, string shareId, VisibilityCode visibility)
    {
      if (comment != null && comment.Length > Constants.MaxShareCommentLength)
      {
        throw new ArgumentOutOfRangeException("comment", string.Format(Resources.MaxLengthMessageFormat, "Comment", Constants.MaxShareCommentLength));
      }

      if (shareId == null)
      {
        throw new ArgumentNullException(string.Format(Resources.NotNullMessageFormat, "shareId"), "shareId");
      }

      if (shareId == string.Empty)
      {
        throw new ArgumentException("shareId", string.Format(Resources.NotEmptyStringMessageFormat, "shareId"));
      }

      if (visibility == VisibilityCode.Unknown)
      {
        throw new ArgumentException(Resources.InvalidVisibilityCode, "visibilityCode");
      }

      ReShare reshare = new ReShare
      {
        Comment = comment,
        Attribution = new Attribution
        {
          Share = new Share
          {
            Id = shareId
          },
        },
        Visibility = new Visibility
        {
          Code = visibility
        }
      };

      UriBuilder location = BuildApiUrlForCurrentUser(Constants.SharesResourceName);

      WebRequest webRequest = this.Authorization.InitializePostRequest(location.Uri);
      webRequest = InitializeRequest<ReShare>(webRequest, reshare);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);
      ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.Created;
    }
    #endregion

    #region Messaging And Invitation API
    /// <summary>
    /// Invite a person to the current user his network.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1012
    /// </summary>
    /// <param name="personId">The identifier of the person to invite.</param>
    /// <param name="subject">The subject of the message that will be sent to the recipient.</param>
    /// <param name="body">The body of the message that will be sent to the recipient.</param>
    /// <param name="connectionType">The type of connection to invite the person to.</param>
    /// <param name="apiRequest">A <see cref="ApiRequest"/> object used to authorize the recipient.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="personId"/> is an empty string. -or-
    /// when <paramref name="subject"/> is an empty string. -or-
    /// when <paramref name="body"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="personId"/> is null. -or-
    /// when <paramref name="subject"/> is null. -or-
    /// when <paramref name="body"/> is null.</exception>
    public bool InvitePerson(string personId, string subject, string body, ConnectionType connectionType, ApiRequest apiRequest)
    {
      if (personId == null)
      {
        throw new ArgumentNullException("personId", string.Format(Resources.NotNullMessageFormat, "personId"));
      }

      if (personId == string.Empty)
      {
        throw new ArgumentException("personId", string.Format(Resources.NotEmptyStringMessageFormat, "personId"));
      }

      if (subject == null)
      {
        throw new ArgumentNullException("subject", string.Format(Resources.NotNullMessageFormat, "subject"));
      }

      if (subject == string.Empty)
      {
        throw new ArgumentException("subject", string.Format(Resources.NotEmptyStringMessageFormat, "subject"));
      }

      if (body == null)
      {
        throw new ArgumentNullException("body", string.Format(Resources.NotNullMessageFormat, "body"));
      }

      if (body == string.Empty)
      {
        throw new ArgumentException("body", string.Format(Resources.NotEmptyStringMessageFormat, "body"));
      }

      if (apiRequest == null)
      {
        throw new ArgumentNullException("apiRequest");
      }

      string path = string.Format(CultureInfo.InvariantCulture, "/people/id={0}", personId);
      List<Recipient> recipients = new List<Recipient>
      {
        new Recipient(path)
      };

      MailboxItem mailboxItem = new MailboxItem(recipients);
      mailboxItem.Subject = subject;
      mailboxItem.Body = body;

      string headerValue = apiRequest.Headers[0].Value;
      string[] authorizationParams = headerValue.Split(":".ToCharArray());
      mailboxItem.ItemContent = new Invitation
      {
        ConnectType = connectionType,
        Authorization = new KeyValuePair<string, string>(authorizationParams[0], authorizationParams[1])
      };

      return InvitePerson(mailboxItem);
    }

    /// <summary>
    /// Invite a person to the current user his network.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1012
    /// </summary>
    /// <param name="emailAddress">The e-mail address of the person to invite.</param>
    /// <param name="firstName">The first name of the person to invite.</param>
    /// <param name="lastName">The last name of the person to invite.</param>
    /// <param name="subject">The subject of the message that will be sent to the recipient.</param>
    /// <param name="body">The body of the message that will be sent to the recipient.</param>
    /// <param name="connectionType">The type of connection to invite the person to.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="emailAddress"/> is an empty string. -or-
    /// when <paramref name="firstName"/> is an empty string. -or-
    /// when <paramref name="lastName"/> is an empty string. -or-
    /// when <paramref name="subject"/> is an empty string. -or-
    /// when <paramref name="body"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="emailAddress"/> is null. -or-
    /// when <paramref name="firstName"/> is null. -or-
    /// when <paramref name="lastName"/> is null. -or-
    /// when <paramref name="subject"/> is null. -or-
    /// when <paramref name="body"/> is null.</exception>
    public bool InvitePerson(string emailAddress, string firstName, string lastName, string subject, string body, ConnectionType connectionType)
    {
      if (emailAddress == null)
      {
        throw new ArgumentNullException("emailAddress", string.Format(Resources.NotNullMessageFormat, "emailAddress"));
      }

      if (emailAddress == string.Empty)
      {
        throw new ArgumentException("emailAddress", string.Format(Resources.NotEmptyStringMessageFormat, "emailAddress"));
      }

      if (firstName == null)
      {
        throw new ArgumentNullException("firstName", string.Format(Resources.NotNullMessageFormat, "firstName"));
      }

      if (firstName == string.Empty)
      {
        throw new ArgumentException("firstName", string.Format(Resources.NotEmptyStringMessageFormat, "firstName"));
      }

      if (lastName == null)
      {
        throw new ArgumentNullException("lastName", string.Format(Resources.NotNullMessageFormat, "lastName"));
      }

      if (lastName == string.Empty)
      {
        throw new ArgumentException("lastName", string.Format(Resources.NotEmptyStringMessageFormat, "lastName"));
      }

      if (subject == null)
      {
        throw new ArgumentNullException("subject", string.Format(Resources.NotNullMessageFormat, "subject"));
      }

      if (subject == string.Empty)
      {
        throw new ArgumentException("subject", string.Format(Resources.NotEmptyStringMessageFormat, "subject"));
      }

      if (body == null)
      {
        throw new ArgumentNullException("body", string.Format(Resources.NotNullMessageFormat, "body"));
      }

      if (body == string.Empty)
      {
        throw new ArgumentException("body", string.Format(Resources.NotEmptyStringMessageFormat, "body"));
      }

      string path = string.Format(CultureInfo.InvariantCulture, "/people/email={0}", emailAddress);
      List<Recipient> recipients = new List<Recipient>
      {
        new Recipient(path)
      };

      MailboxItem mailboxItem = new MailboxItem(recipients);
      mailboxItem.Subject = subject;
      mailboxItem.Body = body;

      mailboxItem.ItemContent = new Invitation
      {
        ConnectType = connectionType
      };

      return InvitePerson(mailboxItem);
    }

    /// <summary>
    /// Invite a person to the current user his network.
    /// </summary>
    /// <param name="mailboxItem">A <see cref="MailboxItem"/> representing the invitation.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    private bool InvitePerson(MailboxItem mailboxItem)
    {
      UriBuilder location = BuildApiUrlForCurrentUser(Constants.MailboxResourceName);

      WebRequest webRequest = this.Authorization.InitializePostRequest(location.Uri);
      webRequest = InitializeRequest<MailboxItem>(webRequest, mailboxItem);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);

      ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    /// Send a message to one or more persons.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1012
    /// </summary>
    /// <param name="subject">The subject of the message.</param>
    /// <param name="body">The body of the message.</param>
    /// <param name="memberIds">A list of member identifiers.</param>
    /// <param name="includeCurrentUser">Whether to send the message to the current user.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    public bool SendMessage(string subject, string body, IEnumerable<string> memberIds, bool includeCurrentUser)
    {
      if (memberIds == null)
      {
        if (includeCurrentUser == false)
        {
          throw new ArgumentNullException("memberIds", string.Format(Resources.NotNullMessageFormat, "memberIds"));
        }
        else
        {
          memberIds = new List<string>();
        }
      }

      List<Recipient> recipients = new List<Recipient>();
      foreach (string recipient in memberIds)
      {
        recipients.Add(new Recipient { Path = string.Format(CultureInfo.InvariantCulture, "/people/{0}", recipient) });
      }

      if (includeCurrentUser)
      {
        recipients.Add(new Recipient { Path = string.Format(CultureInfo.InvariantCulture, "/people/{0}", Constants.CurrentUserIdentifier) });
      }

      MailboxItem mailboxItem = new MailboxItem(recipients);
      mailboxItem.Subject = subject;
      mailboxItem.Body = body;

      UriBuilder location = BuildApiUrlForCurrentUser(Constants.MailboxResourceName);

      WebRequest webRequest = this.Authorization.InitializePostRequest(location.Uri);
      webRequest = InitializeRequest<MailboxItem>(webRequest, mailboxItem);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);

      string xmlResponse = ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.Created;
    }
    #endregion

    #region Post Network Update API
    /// <summary>
    /// Post a Network Update.
    /// <para />
    /// For more info see: http://developer.linkedin.com/docs/DOC-1009
    /// </summary>
    /// <param name="cultureName">A culture name indicating the language of the update.</param>
    /// <param name="body">The actual content of the update. You can use HTML to include links to the user name and the content the user created. Other HTML tags are not supported. All body text should be HTML entity escaped and UTF-8 compliant.</param>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="body"/> is null.</exception>
    /// <exception cref="ArgumentException">When <paramref name="body"/> is an empty string.</exception>
    public bool PostNetworkUpdate(string cultureName, string body)
    {
      if (body == null)
      {
        throw new ArgumentNullException("body", string.Format(Resources.NotNullMessageFormat, "body"));
      }

      if (body == string.Empty)
      {
        throw new ArgumentException(string.Format(Resources.NotEmptyStringMessageFormat, "body"), "body");
      }

      if (string.IsNullOrEmpty(cultureName))
      {
        cultureName = "en-US";
      }

      Activity activity = new Activity();
      activity.CultureName = cultureName;
      activity.Body = body;

      UriBuilder location = BuildApiUrlForCurrentUser(Constants.PersonActivitiesResourceName);

      WebRequest webRequest = this.Authorization.InitializePostRequest(location.Uri);
      webRequest = InitializeRequest<Activity>(webRequest, activity);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);

      string xmlResponse = ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.Created;
    }
    #endregion

    #region Authorization Tokens
    /// <summary>
    /// Invalidate the currently used OAuth token.
    /// </summary>
    /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
    public bool InvalidateToken()
    {
      UriBuilder location = new UriBuilder(string.Concat(Constants.ApiOAuthBaseUrl, Constants.InvalidateTokenMethod));
      WebRequest webRequest = this.Authorization.InitializeGetRequest(location.Uri);
      HttpWebResponse webResponse = (HttpWebResponse)SendRequest(webRequest);
      ProcessResponse(webResponse);

      return webResponse.StatusCode == HttpStatusCode.OK;
    }
    #endregion

    #region Private methods
    #region BuildApiUrl
    #region BuildApiUrlForCurrentUser
    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="profileType">Indicate the profile type.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(ProfileType profileType)
    {
      return BuildApiUrlForCurrentUser(profileType, null, null);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(string resourceName)
    {
      return BuildApiUrlForCurrentUser(new Resource { Name = resourceName });
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(QueryStringParameters parameters)
    {
      return BuildApiUrlForCurrentUser(ProfileType.Standard, null, parameters);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resource">The API resource.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(Resource resource)
    {
      return BuildApiUrlForCurrentUser(resource, null);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resources">A list of API resources.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(Collection<Resource> resources)
    {
      return BuildApiUrlForCurrentUser(ProfileType.Standard, resources, null);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="profileType">Indicate the profile type.</param>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(ProfileType profileType, string resourceName)
    {
      return BuildApiUrlForCurrentUser(profileType, new Resource { Name = resourceName });
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="profileType">Indicate the profile type.</param>
    /// <param name="resource">The API resource.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(ProfileType profileType, Resource resource)
    {
      return BuildApiUrlForCurrentUser(ProfileType.Standard, new Collection<Resource> { resource }, null);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(string resourceName, QueryStringParameters parameters)
    {
      return BuildApiUrlForCurrentUser(new Resource { Name = resourceName }, parameters);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resource">The API resource.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(Resource resource, QueryStringParameters parameters)
    {
      return BuildApiUrlForCurrentUser(ProfileType.Standard, new Collection<Resource> { resource }, parameters);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="profileType">Indicate the profile type.</param>
    /// <param name="resources">A list of API resources.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForCurrentUser(ProfileType profileType, Collection<Resource> resources, QueryStringParameters parameters)
    {
      string identifier = Constants.CurrentUserIdentifier;
      if (profileType == ProfileType.Public)
      {
        identifier = string.Concat(identifier, Constants.PublicProfileIdentifier);
      }

      return BuildApiUrlForMember(identifier, resources, parameters);
    }
    #endregion

    #region BuildApiUrlForMember
    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="memberId">The identifier for a member.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlByMemberId(string memberId)
    {
      return BuildApiUrlByMemberId(memberId, string.Empty);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="memberId">The identifier for a member.</param>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlByMemberId(string memberId, string resourceName)
    {
      string identifier = string.Format(CultureInfo.InvariantCulture, Constants.MemberIdIdentifierFormat, memberId);

      return BuildApiUrlForMember(identifier, resourceName, null);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="identifier">The identifier for a member.</param>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForMember(string identifier, string resourceName, QueryStringParameters parameters)
    {
      return BuildApiUrlForMember(identifier, new Resource { Name = resourceName }, parameters);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="identifier">The identifier for a member.</param>
    /// <param name="resource">The API resource.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForMember(string identifier, Resource resource, QueryStringParameters parameters)
    {
      return BuildApiUrl(
        new Collection<Resource> 
        { 
          new Resource { Name = Constants.PeopleResourceName, Identifier = identifier },
          resource 
        },
        parameters);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="identifier">The identifier of the member.</param>
    /// <param name="resources">A list of API resources.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrlForMember(string identifier, Collection<Resource> resources, QueryStringParameters parameters)
    {
      if (resources == null)
      {
        resources = new Collection<Resource>();
      }

      resources.Insert(0, new Resource { Name = Constants.PeopleResourceName, Identifier = identifier });

      return BuildApiUrl(resources, parameters);
    }
    #endregion

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>A <see cref="UriBuilder"/> representing the API url.</returns>
    private UriBuilder BuildApiUrl(string resourceName)
    {
      return BuildApiUrl(new Resource { Name = resourceName });
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resource">The API resource.</param>
    /// <returns>A <see cref="UriBuilder"/> representing the API url.</returns>
    private UriBuilder BuildApiUrl(Resource resource)
    {
      return BuildApiUrl(new Collection<Resource> { resource }, null);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resources">A list of API resources.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrl(Collection<Resource> resources)
    {
      return BuildApiUrl(resources, null);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrl(string resourceName, QueryStringParameters parameters)
    {
      return BuildApiUrl(new Resource { Name = resourceName }, parameters);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resource">The API resource.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrl(Resource resource, QueryStringParameters parameters)
    {
      return BuildApiUrl(new Collection<Resource> { resource }, parameters);
    }

    /// <summary>
    /// Initialize the url of the API.
    /// </summary>
    /// <param name="resources">A list of API resources.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <returns>A <see cref="UriBuilder"/> object representing the url.</returns>
    private UriBuilder BuildApiUrl(Collection<Resource> resources, QueryStringParameters parameters)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(Constants.ApiBaseUrl);
      foreach (Resource resource in resources)
      {
        if (string.IsNullOrEmpty(resource.Name) == false)
        {
          sb.Append("/");
          sb.Append(resource.Name);
          if (string.IsNullOrEmpty(resource.Identifier) == false)
          {
            sb.Append("/");
            sb.Append(resource.Identifier);
          }
        }
      }
      
      UriBuilder uri = new UriBuilder(sb.ToString());

      if (parameters != null)
      {
        return parameters.AppendToUri(uri);
      }
      else
      {
        return uri;
      }
    }
    #endregion
    
    /// <summary>
    /// Initialize the API request.
    /// </summary>
    /// <typeparam name="T">Type of the request data.</typeparam>
    /// <param name="webRequest">The <see cref="WebRequest" /> to initialize.</param>
    /// <param name="requestData">The request data to send.</param>
    /// <returns>A <see cref="WebRequest"/> representing the API request.</returns>
    private WebRequest InitializeRequest<T>(WebRequest webRequest, T requestData)
    {
      byte[] requestBytes = Encoding.UTF8.GetBytes(Utilities.SerializeToXml<T>(requestData));

      webRequest.ContentType = "text/xml";
      webRequest.ContentLength = requestBytes.Length;

      using (Stream requestStream = webRequest.GetRequestStream())
      {
        requestStream.Write(requestBytes, 0, requestBytes.Length);
      }

      return webRequest;
    }

    /// <summary>
    /// Send a <see cref="WebRequest"/> and retrieve the response.
    /// </summary>
    /// <param name="webRequest">The web request to send.</param>
    /// <exception cref="WebException">Thrown in case of a connect failure, name resolution failure, send failure or timeout of the WebException.</exception>
    /// <returns>A <see cref="WebResponse"/> object representing the API response.</returns>
    private WebResponse SendRequest(WebRequest webRequest)
    {
      HttpWebResponse webResponse = null;
      try
      {
        webResponse = (HttpWebResponse)webRequest.GetResponse();
      }
      catch (WebException wex)
      {
        if (wex.Status == WebExceptionStatus.ConnectFailure ||
          wex.Status == WebExceptionStatus.NameResolutionFailure ||
          wex.Status == WebExceptionStatus.SendFailure ||
          wex.Status == WebExceptionStatus.Timeout)
        {
          throw;
        }

        webResponse = (HttpWebResponse)wex.Response;
      }

      return webResponse;
    }

    /// <summary>
    /// Process the API response.
    /// </summary>
    /// <param name="webResponse">The <see cref="WebResponse"/> to process.</param>
    /// <returns>A xml string returned by the API.</returns>
    private string ProcessResponse(WebResponse webResponse)
    {
      string xmlResponse = string.Empty;
      using (var streamReader = new StreamReader(webResponse.GetResponseStream()))
      {
        xmlResponse = streamReader.ReadToEnd();
      }

      Utilities.ParseException(xmlResponse);
      return xmlResponse;
    }
    #endregion
  }
}
