//-----------------------------------------------------------------------
// <copyright file="EducationFieldSelectorConverter.cs" company="Beemway">
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
  /// Converter for education fields.
  /// </summary>
  internal class EducationFieldSelectorConverter : FieldSelectorConverter<ProfileField>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="EducationFieldSelectorConverter"/> class.
    /// </summary>
    internal EducationFieldSelectorConverter()
      : base(Constants.EducationsFieldSelector)
    {
    }

    /// <summary>
    /// Whether or not a profile field is a valid field for this converter.
    /// </summary>
    /// <param name="profileField">The profile field to check.</param>
    /// <returns><b>True</b> if the profile field is valid; otherwise <b>false</b>.</returns>
    internal override bool IsValidField(ProfileField profileField)
    {
      return profileField == ProfileField.EducationId ||
        profileField == ProfileField.EducationSchoolName ||
        profileField == ProfileField.EducationFieldOfStudy ||
        profileField == ProfileField.EducationStartDate ||
        profileField == ProfileField.EducationEndDate ||
        profileField == ProfileField.EducationDegree ||
        profileField == ProfileField.EducationActivities;
    }
  }
}
