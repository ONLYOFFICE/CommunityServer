using System;
using System.IO;
using System.Xml.Linq;

namespace AppLimit.CloudComputing.SharpBox.Common.Extensions
{
    public static class SharpBoxExtensions
    {
        private const int BufferSize = 2048;

        public static void CopyTo(this Stream src, Stream dst)
        {
            if (src == null || dst == null)
                return;

            var buffer = new byte[BufferSize];
            int readed;
            while ((readed = src.Read(buffer, 0, BufferSize)) > 0)
            {
                dst.Write(buffer, 0, readed);
            }
        }

        public static string ReplaceFirst(this string src, string replace, string replaceWith)
        {
            var ind = src.IndexOf(replace, StringComparison.InvariantCulture);

            if (ind != -1)
            {
                src = src.Remove(ind, replace.Length).Insert(ind, replaceWith);
            }

            return src;
        }

        public static string ReplaceLast(this string src, string replace, string replaceWith)
        {
            var ind = src.LastIndexOf(replace, StringComparison.InvariantCulture);

            if (ind != -1)
            {
                src = src.Remove(ind, replace.Length).Insert(ind, replaceWith);
            }

            return src;
        }

        public static XAttribute AttributeOrNull(this XElement el, string attr)
        {
            return el != null ? el.Attribute(attr) : null;
        }

        public static string ValueOrEmpty(this XElement el)
        {
            return el != null ? el.Value : string.Empty;
        }

        public static string ValueOrEmpty(this XAttribute attr)
        {
            return attr != null ? attr.Value : string.Empty;
        }
    }
}