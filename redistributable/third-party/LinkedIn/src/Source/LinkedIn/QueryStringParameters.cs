using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Web;

namespace LinkedIn
{
  /// <summary>
  /// Represents a collection of query string parameters.
  /// </summary>
  public class QueryStringParameters : NameValueCollection
  {
    /// <summary>
    /// Add a parameter to a list of parameters.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    public override void Add(string name, string value)
    {
      if (string.IsNullOrEmpty(value) == false)
      {
        base.Add(name, value);
      }
    }
    
    /// <summary>
    /// Add a parameter to a list of parameters.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    public void Add(string name, int value)
    {
      if (value > -1)
      {
        base.Add(name, value.ToString(CultureInfo.InvariantCulture));
      }
    }

    /// <summary>
    /// Add a parameter to a list of parameters.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    public void Add(string name, bool value)
    {
      base.Add(name, value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Append a the parameters to the query string of an uri.
    /// </summary>
    /// <param name="location">The uri represented by a <see cref="UriBuilder"/> object to append to.</param>
    /// <returns>A uri represented by a <see cref="UriBuilder"/> object containing the query string parameters.</returns>
    internal UriBuilder AppendToUri(UriBuilder location)
    {
      if (this.Count == 0)
      {
        return location;
      }

      StringBuilder sb = new StringBuilder();
      if (string.IsNullOrEmpty(location.Query) == false)
      {
        sb.Append(location.Query.Substring(1));
      }

      foreach (string key in this.Keys)
      {
        sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(this[key]));
      }

      sb.Length--;

      location.Query = sb.ToString();
      return location;
    }
  }
}
