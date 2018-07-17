/* 
 * DocuSign REST API
 *
 * The DocuSign REST API provides you with a powerful, convenient, and simple Web services API for interacting with DocuSign.
 *
 * OpenAPI spec version: v2
 * Contact: devcenter@docusign.com
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace DocuSign.eSign.Model
{
    /// <summary>
    /// Contains information about custom fields.
    /// </summary>
    [DataContract]
    public partial class CustomFields :  IEquatable<CustomFields>, IValidatableObject
    {
        public CustomFields()
        {
            // Empty Constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFields" /> class.
        /// </summary>
        /// <param name="ListCustomFields">An array of list custom fields..</param>
        /// <param name="TextCustomFields">An array of text custom fields..</param>
        public CustomFields(List<ListCustomField> ListCustomFields = default(List<ListCustomField>), List<TextCustomField> TextCustomFields = default(List<TextCustomField>))
        {
            this.ListCustomFields = ListCustomFields;
            this.TextCustomFields = TextCustomFields;
        }
        
        /// <summary>
        /// An array of list custom fields.
        /// </summary>
        /// <value>An array of list custom fields.</value>
        [DataMember(Name="listCustomFields", EmitDefaultValue=false)]
        public List<ListCustomField> ListCustomFields { get; set; }
        /// <summary>
        /// An array of text custom fields.
        /// </summary>
        /// <value>An array of text custom fields.</value>
        [DataMember(Name="textCustomFields", EmitDefaultValue=false)]
        public List<TextCustomField> TextCustomFields { get; set; }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CustomFields {\n");
            sb.Append("  ListCustomFields: ").Append(ListCustomFields).Append("\n");
            sb.Append("  TextCustomFields: ").Append(TextCustomFields).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return this.Equals(obj as CustomFields);
        }

        /// <summary>
        /// Returns true if CustomFields instances are equal
        /// </summary>
        /// <param name="other">Instance of CustomFields to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CustomFields other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.ListCustomFields == other.ListCustomFields ||
                    this.ListCustomFields != null &&
                    this.ListCustomFields.SequenceEqual(other.ListCustomFields)
                ) && 
                (
                    this.TextCustomFields == other.TextCustomFields ||
                    this.TextCustomFields != null &&
                    this.TextCustomFields.SequenceEqual(other.TextCustomFields)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = 41;
                // Suitable nullity checks etc, of course :)
                if (this.ListCustomFields != null)
                    hash = hash * 59 + this.ListCustomFields.GetHashCode();
                if (this.TextCustomFields != null)
                    hash = hash * 59 + this.TextCustomFields.GetHashCode();
                return hash;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        { 
            yield break;
        }
    }

}
