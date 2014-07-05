//-----------------------------------------------------------------------
// <copyright file="Visibility.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LinkedIn.Utility;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents the visibility in LinkedIn.
  /// </summary>
  [XmlRoot("visibility")]
  public class Visibility : IXmlSerializable
  {
    #region Private members
    private readonly string RootElementName = "visibility";
    private readonly string CodeElementName = "code";
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Visibility"/> class.
    /// </summary>
    public Visibility()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the visibility code.
    /// </summary>
    /// <remarks>Possible values are: anyone, connections-only.</remarks>
    public VisibilityCode Code
    {
      get;
      set;
    }
    #endregion

    #region Methods
    public void WriteContractBody(System.Xml.XmlWriter writer, bool isRoot)
    {
      if (isRoot == false)
      {
        writer.WriteStartElement(RootElementName);
      }

      writer.WriteElementString(CodeElementName, EnumHelper.GetDescription(this.Code));

      if (isRoot == false)
      {
        writer.WriteEndElement();
      }
    }
    #endregion

    #region IXmlSerializable Members
    /// <summary>
    /// This method is reserved and should not be used.
    /// </summary>
    /// <returns>This should always returns <b>null</b> (<b>Nothing</b> in Visual Basic).</returns>
    public System.Xml.Schema.XmlSchema GetSchema()
    {
      return null;
    }

    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader" /> stream from which the object is deserialized.</param>
    public void ReadXml(System.Xml.XmlReader reader)
    {
      // Read the opening tag of the encapsulating element
      reader.ReadStartElement();

      reader.ReadStartElement(CodeElementName);
      string codeString = reader.ReadString();
      switch (codeString)
      {
        case "anyone":
          this.Code = VisibilityCode.Anyone;
          break;
        case "connections-only":
          this.Code = VisibilityCode.ConnectionsOnly;
          break;
      }

      reader.ReadEndElement();

      // Read the end tag of the encapsulating element
      reader.ReadEndElement();
    }

    /// <summary>
    /// Converts an object into its XML representation. 
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter" /> stream to which the object is serialized.</param>
    public void WriteXml(System.Xml.XmlWriter writer)
    {
      WriteContractBody(writer, true);
    }
    #endregion
  }
}
