using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Serialization.iCalendar.Serializers.Other;

namespace Ical.Net.Serialization.iCalendar.Serializers.DataTypes
{
    public class RequestStatusSerializer : StringSerializer
    {
        public RequestStatusSerializer() { }

        public RequestStatusSerializer(ISerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (RequestStatus);

        public override string SerializeToString(object obj)
        {
            try
            {
                var rs = obj as IRequestStatus;
                if (rs == null)
                {
                    return null;
                }

                // Push the object onto the serialization stack
                SerializationContext.Push(rs);

                try
                {
                    var factory = GetService<ISerializerFactory>();
                    var serializer = factory?.Build(typeof (IStatusCode), SerializationContext) as IStringSerializer;
                    if (serializer == null)
                    {
                        return null;
                    }

                    var builder = new StringBuilder(256);
                    builder.Append(Escape(serializer.SerializeToString(rs.StatusCode)));
                    builder.Append(";");
                    builder.Append(Escape(rs.Description));
                    if (!string.IsNullOrWhiteSpace(rs.ExtraData))
                    {
                        builder.Append(";");
                        builder.Append(Escape(rs.ExtraData));
                    }
                    return Encode(rs, builder.ToString());
                }
                finally
                {
                    // Pop the object off the serialization stack
                    SerializationContext.Pop();
                }
            }
            catch
            {
                return null;
            }
        }

        internal static readonly Regex NarrowRequestMatch = new Regex(@"(.*?[^\\]);(.*?[^\\]);(.+)", RegexOptions.Compiled);
        internal static readonly Regex BroadRequestMatch = new Regex(@"(.*?[^\\]);(.+)", RegexOptions.Compiled);

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            var rs = CreateAndAssociate() as IRequestStatus;
            if (rs == null)
            {
                return null;
            }

            // Decode the value as needed
            value = Decode(rs, value);

            // Push the object onto the serialization stack
            SerializationContext.Push(rs);

            try
            {
                var factory = GetService<ISerializerFactory>();
                if (factory == null)
                {
                    return null;
                }

                var match = NarrowRequestMatch.Match(value);
                if (!match.Success)
                {
                    match = BroadRequestMatch.Match(value);
                }

                if (match.Success)
                {
                    var serializer = factory.Build(typeof(IStatusCode), SerializationContext) as IStringSerializer;
                    if (serializer == null)
                    {
                        return null;
                    }

                    rs.StatusCode = serializer.Deserialize(new StringReader(Unescape(match.Groups[1].Value))) as IStatusCode;
                    rs.Description = Unescape(match.Groups[2].Value);
                    if (match.Groups.Count == 4)
                    {
                        rs.ExtraData = Unescape(match.Groups[3].Value);
                    }

                    return rs;
                }
            }
            finally
            {
                // Pop the object off the serialization stack
                SerializationContext.Pop();
            }
            return null;
        }
    }
}