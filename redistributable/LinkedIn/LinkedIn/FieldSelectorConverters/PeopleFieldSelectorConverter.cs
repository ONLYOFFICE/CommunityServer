//-----------------------------------------------------------------------
// <copyright file="PeopleFieldSelectorConverter.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Text;
using LinkedIn.ServiceEntities;
using LinkedIn.Utility;

namespace LinkedIn.FieldSelectorConverters
{
  /// <summary>
  /// Converter for people fields.
  /// </summary>
  internal class PeopleFieldSelectorConverter : FieldSelectorConverter<ProfileField>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PeopleFieldSelectorConverter"/> class.
    /// </summary>
    internal PeopleFieldSelectorConverter()
      : this(false)
    {
    }

    internal PeopleFieldSelectorConverter(bool onlyChildFieldSelectors)
      : base(onlyChildFieldSelectors, Constants.PeopleFieldSelector)
    {
      this.FieldSelectorConverters.Add(new LocationFieldSelectorConverter());
      this.FieldSelectorConverters.Add(new RelationToViewerFieldSelectorConverter());
      this.FieldSelectorConverters.Add(new MemberUrlResourcesFieldSelectorConverter());
      this.FieldSelectorConverters.Add(new ApiStandardProfileRequestFieldSelectorConverter());
      this.FieldSelectorConverters.Add(new PositionFieldSelectorConverter());
      this.FieldSelectorConverters.Add(new EducationFieldSelectorConverter());
      this.FieldSelectorConverters.Add(new RecommendationFieldSelectorConverter());
    }

    /// <summary>
    /// Whether or not a profile field is a valid field for this converter.
    /// </summary>
    /// <param name="profileField">The profile field to check.</param>
    /// <returns><b>True</b> if the profile field is valid; otherwise <b>false</b>.</returns>
    internal override bool IsValidField(ProfileField profileField)
    {
      return profileField == ProfileField.PersonId ||
        profileField == ProfileField.FirstName ||
        profileField == ProfileField.LastName ||
        profileField == ProfileField.Headline ||
        profileField == ProfileField.Distance ||
        profileField == ProfileField.CurrentShare ||
        profileField == ProfileField.CurrentStatus ||
        profileField == ProfileField.CurrentStatusTimestamp ||
        profileField == ProfileField.Connections ||
        profileField == ProfileField.NumberOfConnections ||
        profileField == ProfileField.NumberOfConnectionsCapped ||
        profileField == ProfileField.Summary ||
        profileField == ProfileField.Specialties ||
        profileField == ProfileField.ProposalComments ||
        profileField == ProfileField.Associations ||
        profileField == ProfileField.Honors ||
        profileField == ProfileField.ThreeCurrentPositions ||
        profileField == ProfileField.ThreePastPositions ||
        profileField == ProfileField.NumberOfRecommenders ||
        profileField == ProfileField.PhoneNumbers ||
        profileField == ProfileField.IMAccounts ||
        profileField == ProfileField.TwitterAccounts ||
        profileField == ProfileField.DateOfBirth ||
        profileField == ProfileField.MainAddress ||
        profileField == ProfileField.PictureUrl ||
        profileField == ProfileField.SiteStandardProfileRequestUrl ||
        profileField == ProfileField.ApiPublicProfileRequestUrl ||
        profileField == ProfileField.SitePublicProfileRequestUrl ||
        profileField == ProfileField.PublicProfileUrl;
    }
  }
}
