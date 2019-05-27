using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class AttachmentSerializer : EncodableDataTypeSerializer
    {
        public AttachmentSerializer() { }

        public AttachmentSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (Attachment);

        public override string SerializeToString(object obj)
        {
            var a = obj as Attachment;
            if (a == null)
            {
                return null;
            }

            if (a.Uri != null)
            {
                if (a.Parameters.ContainsKey("VALUE"))
                {
                    // Ensure no VALUE type is provided
                    a.Parameters.Remove("VALUE");
                }

                return Encode(a, a.Uri.OriginalString);
            }
            if (a.Data == null)
            {
                return null;
            }

            // Ensure the VALUE type is set to BINARY
            a.SetValueType("BINARY");

            // BASE64 encoding for BINARY inline attachments.
            a.Parameters.Set("ENCODING", "BASE64");

            return Encode(a, a.Data);
        }

        public Attachment Deserialize(string attachment)
        {
            try
            {
                var a = CreateAndAssociate() as Attachment;
                // Decode the value, if necessary
                var data = DecodeData(a, attachment);

                // Get the currently-used encoding off the encoding stack.
                var encodingStack = GetService<EncodingStack>();
                a.ValueEncoding = encodingStack.Current;

                // Get the format of the attachment
                var valueType = a.GetValueType();
                if (valueType == typeof(byte[]))
                {
                    // If the VALUE type is specifically set to BINARY,
                    // then set the Data property instead.                    
                    return new Attachment(data) {ValueEncoding = a.ValueEncoding};
                }

                // The default VALUE type for attachments is URI.  So, let's
                // grab the URI by default.
                var uriValue = Decode(a, attachment);
                a.Uri = new Uri(uriValue);

                return a;
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public override object Deserialize(TextReader tr) => Deserialize(tr.ReadToEnd());
    }
}