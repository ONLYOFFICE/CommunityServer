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
    /// FolderItemV2
    /// </summary>
    [DataContract]
    public partial class FolderItemV2 :  IEquatable<FolderItemV2>, IValidatableObject
    {
        public FolderItemV2()
        {
            // Empty Constructor
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderItemV2" /> class.
        /// </summary>
        /// <param name="CompletedDateTime">Specifies the date and time this item was completed..</param>
        /// <param name="CreatedDateTime">Indicates the date and time the item was created..</param>
        /// <param name="EnvelopeId">The envelope ID of the envelope status that failed to post..</param>
        /// <param name="EnvelopeUri">Contains a URI for an endpoint that you can use to retrieve the envelope or envelopes..</param>
        /// <param name="ExpireDateTime">The date and time the envelope is set to expire..</param>
        /// <param name="FolderId">.</param>
        /// <param name="FolderUri">.</param>
        /// <param name="Is21CFRPart11">When set to **true**, indicates that this module is enabled on the account..</param>
        /// <param name="IsSignatureProviderEnvelope">.</param>
        /// <param name="LastModifiedDateTime">The date and time the item was last modified..</param>
        /// <param name="OwnerName">.</param>
        /// <param name="Recipients">Recipients.</param>
        /// <param name="RecipientsUri">Contains a URI for an endpoint that you can use to retrieve the recipients..</param>
        /// <param name="SenderCompany">.</param>
        /// <param name="SenderEmail">.</param>
        /// <param name="SenderName">.</param>
        /// <param name="SenderUserId">.</param>
        /// <param name="SentDateTime">The date and time the envelope was sent..</param>
        /// <param name="Status">Indicates the envelope status. Valid values are:  * sent - The envelope is sent to the recipients.  * created - The envelope is saved as a draft and can be modified and sent later..</param>
        /// <param name="Subject">.</param>
        public FolderItemV2(string CompletedDateTime = default(string), string CreatedDateTime = default(string), string EnvelopeId = default(string), string EnvelopeUri = default(string), string ExpireDateTime = default(string), string FolderId = default(string), string FolderUri = default(string), string Is21CFRPart11 = default(string), string IsSignatureProviderEnvelope = default(string), string LastModifiedDateTime = default(string), string OwnerName = default(string), Recipients Recipients = default(Recipients), string RecipientsUri = default(string), string SenderCompany = default(string), string SenderEmail = default(string), string SenderName = default(string), string SenderUserId = default(string), string SentDateTime = default(string), string Status = default(string), string Subject = default(string))
        {
            this.CompletedDateTime = CompletedDateTime;
            this.CreatedDateTime = CreatedDateTime;
            this.EnvelopeId = EnvelopeId;
            this.EnvelopeUri = EnvelopeUri;
            this.ExpireDateTime = ExpireDateTime;
            this.FolderId = FolderId;
            this.FolderUri = FolderUri;
            this.Is21CFRPart11 = Is21CFRPart11;
            this.IsSignatureProviderEnvelope = IsSignatureProviderEnvelope;
            this.LastModifiedDateTime = LastModifiedDateTime;
            this.OwnerName = OwnerName;
            this.Recipients = Recipients;
            this.RecipientsUri = RecipientsUri;
            this.SenderCompany = SenderCompany;
            this.SenderEmail = SenderEmail;
            this.SenderName = SenderName;
            this.SenderUserId = SenderUserId;
            this.SentDateTime = SentDateTime;
            this.Status = Status;
            this.Subject = Subject;
        }
        
        /// <summary>
        /// Specifies the date and time this item was completed.
        /// </summary>
        /// <value>Specifies the date and time this item was completed.</value>
        [DataMember(Name="completedDateTime", EmitDefaultValue=false)]
        public string CompletedDateTime { get; set; }
        /// <summary>
        /// Indicates the date and time the item was created.
        /// </summary>
        /// <value>Indicates the date and time the item was created.</value>
        [DataMember(Name="createdDateTime", EmitDefaultValue=false)]
        public string CreatedDateTime { get; set; }
        /// <summary>
        /// The envelope ID of the envelope status that failed to post.
        /// </summary>
        /// <value>The envelope ID of the envelope status that failed to post.</value>
        [DataMember(Name="envelopeId", EmitDefaultValue=false)]
        public string EnvelopeId { get; set; }
        /// <summary>
        /// Contains a URI for an endpoint that you can use to retrieve the envelope or envelopes.
        /// </summary>
        /// <value>Contains a URI for an endpoint that you can use to retrieve the envelope or envelopes.</value>
        [DataMember(Name="envelopeUri", EmitDefaultValue=false)]
        public string EnvelopeUri { get; set; }
        /// <summary>
        /// The date and time the envelope is set to expire.
        /// </summary>
        /// <value>The date and time the envelope is set to expire.</value>
        [DataMember(Name="expireDateTime", EmitDefaultValue=false)]
        public string ExpireDateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="folderId", EmitDefaultValue=false)]
        public string FolderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="folderUri", EmitDefaultValue=false)]
        public string FolderUri { get; set; }
        /// <summary>
        /// When set to **true**, indicates that this module is enabled on the account.
        /// </summary>
        /// <value>When set to **true**, indicates that this module is enabled on the account.</value>
        [DataMember(Name="is21CFRPart11", EmitDefaultValue=false)]
        public string Is21CFRPart11 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="isSignatureProviderEnvelope", EmitDefaultValue=false)]
        public string IsSignatureProviderEnvelope { get; set; }
        /// <summary>
        /// The date and time the item was last modified.
        /// </summary>
        /// <value>The date and time the item was last modified.</value>
        [DataMember(Name="lastModifiedDateTime", EmitDefaultValue=false)]
        public string LastModifiedDateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="ownerName", EmitDefaultValue=false)]
        public string OwnerName { get; set; }
        /// <summary>
        /// Gets or Sets Recipients
        /// </summary>
        [DataMember(Name="recipients", EmitDefaultValue=false)]
        public Recipients Recipients { get; set; }
        /// <summary>
        /// Contains a URI for an endpoint that you can use to retrieve the recipients.
        /// </summary>
        /// <value>Contains a URI for an endpoint that you can use to retrieve the recipients.</value>
        [DataMember(Name="recipientsUri", EmitDefaultValue=false)]
        public string RecipientsUri { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="senderCompany", EmitDefaultValue=false)]
        public string SenderCompany { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="senderEmail", EmitDefaultValue=false)]
        public string SenderEmail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="senderName", EmitDefaultValue=false)]
        public string SenderName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="senderUserId", EmitDefaultValue=false)]
        public string SenderUserId { get; set; }
        /// <summary>
        /// The date and time the envelope was sent.
        /// </summary>
        /// <value>The date and time the envelope was sent.</value>
        [DataMember(Name="sentDateTime", EmitDefaultValue=false)]
        public string SentDateTime { get; set; }
        /// <summary>
        /// Indicates the envelope status. Valid values are:  * sent - The envelope is sent to the recipients.  * created - The envelope is saved as a draft and can be modified and sent later.
        /// </summary>
        /// <value>Indicates the envelope status. Valid values are:  * sent - The envelope is sent to the recipients.  * created - The envelope is saved as a draft and can be modified and sent later.</value>
        [DataMember(Name="status", EmitDefaultValue=false)]
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [DataMember(Name="subject", EmitDefaultValue=false)]
        public string Subject { get; set; }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class FolderItemV2 {\n");
            sb.Append("  CompletedDateTime: ").Append(CompletedDateTime).Append("\n");
            sb.Append("  CreatedDateTime: ").Append(CreatedDateTime).Append("\n");
            sb.Append("  EnvelopeId: ").Append(EnvelopeId).Append("\n");
            sb.Append("  EnvelopeUri: ").Append(EnvelopeUri).Append("\n");
            sb.Append("  ExpireDateTime: ").Append(ExpireDateTime).Append("\n");
            sb.Append("  FolderId: ").Append(FolderId).Append("\n");
            sb.Append("  FolderUri: ").Append(FolderUri).Append("\n");
            sb.Append("  Is21CFRPart11: ").Append(Is21CFRPart11).Append("\n");
            sb.Append("  IsSignatureProviderEnvelope: ").Append(IsSignatureProviderEnvelope).Append("\n");
            sb.Append("  LastModifiedDateTime: ").Append(LastModifiedDateTime).Append("\n");
            sb.Append("  OwnerName: ").Append(OwnerName).Append("\n");
            sb.Append("  Recipients: ").Append(Recipients).Append("\n");
            sb.Append("  RecipientsUri: ").Append(RecipientsUri).Append("\n");
            sb.Append("  SenderCompany: ").Append(SenderCompany).Append("\n");
            sb.Append("  SenderEmail: ").Append(SenderEmail).Append("\n");
            sb.Append("  SenderName: ").Append(SenderName).Append("\n");
            sb.Append("  SenderUserId: ").Append(SenderUserId).Append("\n");
            sb.Append("  SentDateTime: ").Append(SentDateTime).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  Subject: ").Append(Subject).Append("\n");
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
            return this.Equals(obj as FolderItemV2);
        }

        /// <summary>
        /// Returns true if FolderItemV2 instances are equal
        /// </summary>
        /// <param name="other">Instance of FolderItemV2 to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(FolderItemV2 other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.CompletedDateTime == other.CompletedDateTime ||
                    this.CompletedDateTime != null &&
                    this.CompletedDateTime.Equals(other.CompletedDateTime)
                ) && 
                (
                    this.CreatedDateTime == other.CreatedDateTime ||
                    this.CreatedDateTime != null &&
                    this.CreatedDateTime.Equals(other.CreatedDateTime)
                ) && 
                (
                    this.EnvelopeId == other.EnvelopeId ||
                    this.EnvelopeId != null &&
                    this.EnvelopeId.Equals(other.EnvelopeId)
                ) && 
                (
                    this.EnvelopeUri == other.EnvelopeUri ||
                    this.EnvelopeUri != null &&
                    this.EnvelopeUri.Equals(other.EnvelopeUri)
                ) && 
                (
                    this.ExpireDateTime == other.ExpireDateTime ||
                    this.ExpireDateTime != null &&
                    this.ExpireDateTime.Equals(other.ExpireDateTime)
                ) && 
                (
                    this.FolderId == other.FolderId ||
                    this.FolderId != null &&
                    this.FolderId.Equals(other.FolderId)
                ) && 
                (
                    this.FolderUri == other.FolderUri ||
                    this.FolderUri != null &&
                    this.FolderUri.Equals(other.FolderUri)
                ) && 
                (
                    this.Is21CFRPart11 == other.Is21CFRPart11 ||
                    this.Is21CFRPart11 != null &&
                    this.Is21CFRPart11.Equals(other.Is21CFRPart11)
                ) && 
                (
                    this.IsSignatureProviderEnvelope == other.IsSignatureProviderEnvelope ||
                    this.IsSignatureProviderEnvelope != null &&
                    this.IsSignatureProviderEnvelope.Equals(other.IsSignatureProviderEnvelope)
                ) && 
                (
                    this.LastModifiedDateTime == other.LastModifiedDateTime ||
                    this.LastModifiedDateTime != null &&
                    this.LastModifiedDateTime.Equals(other.LastModifiedDateTime)
                ) && 
                (
                    this.OwnerName == other.OwnerName ||
                    this.OwnerName != null &&
                    this.OwnerName.Equals(other.OwnerName)
                ) && 
                (
                    this.Recipients == other.Recipients ||
                    this.Recipients != null &&
                    this.Recipients.Equals(other.Recipients)
                ) && 
                (
                    this.RecipientsUri == other.RecipientsUri ||
                    this.RecipientsUri != null &&
                    this.RecipientsUri.Equals(other.RecipientsUri)
                ) && 
                (
                    this.SenderCompany == other.SenderCompany ||
                    this.SenderCompany != null &&
                    this.SenderCompany.Equals(other.SenderCompany)
                ) && 
                (
                    this.SenderEmail == other.SenderEmail ||
                    this.SenderEmail != null &&
                    this.SenderEmail.Equals(other.SenderEmail)
                ) && 
                (
                    this.SenderName == other.SenderName ||
                    this.SenderName != null &&
                    this.SenderName.Equals(other.SenderName)
                ) && 
                (
                    this.SenderUserId == other.SenderUserId ||
                    this.SenderUserId != null &&
                    this.SenderUserId.Equals(other.SenderUserId)
                ) && 
                (
                    this.SentDateTime == other.SentDateTime ||
                    this.SentDateTime != null &&
                    this.SentDateTime.Equals(other.SentDateTime)
                ) && 
                (
                    this.Status == other.Status ||
                    this.Status != null &&
                    this.Status.Equals(other.Status)
                ) && 
                (
                    this.Subject == other.Subject ||
                    this.Subject != null &&
                    this.Subject.Equals(other.Subject)
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
                if (this.CompletedDateTime != null)
                    hash = hash * 59 + this.CompletedDateTime.GetHashCode();
                if (this.CreatedDateTime != null)
                    hash = hash * 59 + this.CreatedDateTime.GetHashCode();
                if (this.EnvelopeId != null)
                    hash = hash * 59 + this.EnvelopeId.GetHashCode();
                if (this.EnvelopeUri != null)
                    hash = hash * 59 + this.EnvelopeUri.GetHashCode();
                if (this.ExpireDateTime != null)
                    hash = hash * 59 + this.ExpireDateTime.GetHashCode();
                if (this.FolderId != null)
                    hash = hash * 59 + this.FolderId.GetHashCode();
                if (this.FolderUri != null)
                    hash = hash * 59 + this.FolderUri.GetHashCode();
                if (this.Is21CFRPart11 != null)
                    hash = hash * 59 + this.Is21CFRPart11.GetHashCode();
                if (this.IsSignatureProviderEnvelope != null)
                    hash = hash * 59 + this.IsSignatureProviderEnvelope.GetHashCode();
                if (this.LastModifiedDateTime != null)
                    hash = hash * 59 + this.LastModifiedDateTime.GetHashCode();
                if (this.OwnerName != null)
                    hash = hash * 59 + this.OwnerName.GetHashCode();
                if (this.Recipients != null)
                    hash = hash * 59 + this.Recipients.GetHashCode();
                if (this.RecipientsUri != null)
                    hash = hash * 59 + this.RecipientsUri.GetHashCode();
                if (this.SenderCompany != null)
                    hash = hash * 59 + this.SenderCompany.GetHashCode();
                if (this.SenderEmail != null)
                    hash = hash * 59 + this.SenderEmail.GetHashCode();
                if (this.SenderName != null)
                    hash = hash * 59 + this.SenderName.GetHashCode();
                if (this.SenderUserId != null)
                    hash = hash * 59 + this.SenderUserId.GetHashCode();
                if (this.SentDateTime != null)
                    hash = hash * 59 + this.SentDateTime.GetHashCode();
                if (this.Status != null)
                    hash = hash * 59 + this.Status.GetHashCode();
                if (this.Subject != null)
                    hash = hash * 59 + this.Subject.GetHashCode();
                return hash;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        { 
            yield break;
        }
    }

}
