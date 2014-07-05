using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkedIn
{
  public class Resource
  {
    /// <summary>
    /// Gets or sets the name of the resource.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the (optional) identifier for a specific element of the resource collection.
    /// </summary>
    public string Identifier { get; set; }
  }
}
