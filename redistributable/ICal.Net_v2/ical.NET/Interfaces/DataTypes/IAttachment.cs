using System;
using System.Text;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IAttachment : IEncodableDataType
    {
        /// <summary>
        /// The URI where the attachment information can be located.
        /// </summary>
        Uri Uri { get; set; }

        /// <summary>
        /// A binary representation of the data that was loaded.
        /// </summary>
        byte[] Data { get; set; }

        /// <summary>
        /// To specify the content type of a referenced object.
        /// This optional value should be an IANA-registered
        /// MIME type, if specified.
        /// </summary>
        string FormatType { get; set; }

        /// <summary>
        /// Gets/sets the encoding used to store the value.
        /// </summary>
        Encoding ValueEncoding { get; set; }
    }
}