//-----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

using LinkedIn.ServiceEntities;

namespace LinkedIn.Utility
{
  /// <summary>
  /// A utilities class
  /// </summary>
  public static class Utilities
  {
    /// <summary>
    /// Generate the timestamp of the provided <see cref="DateTime"/>.      
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> object.</param>
    /// <returns>A timestamp.</returns>
    public static string GenerateTimestamp(DateTime dateTime)
    {
      TimeSpan t = (dateTime - new DateTime(1970, 1, 1));

      return ((long)t.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Serialize an object (of type T) to a string representation.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <typeparam name="T">Type of object to serialize.</typeparam>
    /// <returns>A xml string representing the object.</returns>
    public static string SerializeToXml<T>(T obj)
    {
      XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
      xmlSerializerNamespaces.Add(string.Empty, string.Empty);

      var xmlSerializer = new XmlSerializer(typeof(T));
      var memoryStream = new MemoryStream();
      xmlSerializer.Serialize(memoryStream, obj, xmlSerializerNamespaces);
      return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
    }

    /// <summary>
    /// Deserialize the provided string to it's xml representation.
    /// </summary>
    /// <param name="value">The xml string to deserialize.</param>
    /// <typeparam name="T">Type of object to which the xml string should be deserialized.</typeparam>
    /// <returns>An object (of type T) representing the xml.</returns>
    /// <exception cref="LinkedInException">Thrown if the xml couldn't be deserialized.</exception>
    public static T DeserializeXml<T>(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return default(T);
      }

      var xs = new XmlSerializer(typeof(T));
      var memoryStream = new MemoryStream(StringToUTF8ByteArray(value));

      try
      {
        return (T)xs.Deserialize(memoryStream);
      }
      catch (InvalidOperationException e)
      {
        throw new LinkedInException("Could not deserialize the data.", e);
      }
    }

    /// <summary>
    /// Convert a string to a Byte Array in UTF8 encoding.
    /// </summary>
    /// <param name="value">The xml string to convert.</param>
    /// <returns>A byte Array of the string.</returns>
    public static byte[] StringToUTF8ByteArray(string value)
    {
      UTF8Encoding encoding = new UTF8Encoding();
      byte[] byteArray = encoding.GetBytes(value);
      return byteArray;
    }

    /// <summary>
    /// Checks if response has any error and throws an exception if it does.
    /// </summary>
    /// <param name="response">The xml response of the API.</param>
    public static void ParseException(string response)
    {
      try
      {
        XDocument doc = XDocument.Load(new StringReader(response));
        if (doc.Elements().FirstOrDefault().Name.LocalName == "error")
        {
          var error = Utilities.DeserializeXml<LinkedInApiError>(response);
          if (error != null)
          {
            if (error.Status == 401)
            {
              throw new LinkedInNotAuthorizedException(error.Status, error.ErrorCode, error.Message);
            }
            else
            {
              throw new LinkedInException(error.Status, error.ErrorCode, error.Message);
            }
          }
        }
      }
      catch (LinkedInException)
      {
        // Rethrow deserialized LinkedIn Exception, ignore others
        throw;
      }
      catch (Exception)
      {
      }
    }
  }
}