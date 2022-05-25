namespace net.openstack.Core.Domain.Converters
{
    using System.Net.NetworkInformation;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    /// <summary>
    /// This implementation of <see cref="JsonConverter"/> allows for JSON serialization
    /// and deserialization of <see cref="PhysicalAddress"/> objects using a simple string
    /// representation. Serialization produces an IEEE 802 representation of the form
    /// <literal>00:11:22:33:44:55</literal>, and deserialization supports representations
    /// using hyphens or colons, along with bare strings containing 12 hexadecimal digits.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class PhysicalAddressSimpleConverter : SimpleStringJsonConverter<PhysicalAddress>
    {
        private static readonly Regex Ieee802Expression =
            new Regex(@"^[a-fA-F0-9]{2}(?:[\-\:][a-fA-F0-9]{2}){5}$", RegexOptions.Compiled);

        /// <remarks>
        /// If <paramref name="str"/> is an empty string, this method returns <see langword="null"/>.
        /// Otherwise, this method converts IEEE 802 addresses containing hyphens or colons
        /// to a bare representation, and then uses <see cref="PhysicalAddress.Parse"/> for
        /// deserialization. As a result, this converter can handle physical addresses in
        /// any of the following forms:
        ///
        /// <list type="bullet">
        /// <item><literal>01-23-45-67-89-ab</literal></item>
        /// <item><literal>01:23:45:67:89:ab</literal></item>
        /// <item><literal>0123456789ab</literal></item>
        /// </list>
        /// </remarks>
        /// <inheritdoc/>
        protected override PhysicalAddress ConvertToObject(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            str = str.Trim();
            if (str.Length > 12 && Ieee802Expression.IsMatch(str))
                str = str.Replace("-", string.Empty).Replace(":", string.Empty);

            return PhysicalAddress.Parse(str);
        }
    }
}
