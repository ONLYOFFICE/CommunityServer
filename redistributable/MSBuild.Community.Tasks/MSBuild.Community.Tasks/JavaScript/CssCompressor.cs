using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MSBuild.Community.Tasks.JavaScript
{
    /// <summary>
    /// Work in progress ...
    /// </summary>
    /// <remarks>
    /// Based on http://iceyboard.no-ip.org/projects/css_compressor
    /// </remarks>
    internal sealed class CssCompressor
    {
        public static string Compress(string source)
        {
            StringWriter writer = new StringWriter();
            StringReader reader = new StringReader(source);
            Compress(reader, writer);
            return writer.ToString();
        }

        public static void Compress(TextReader reader, TextWriter writer)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (writer == null)
                throw new ArgumentNullException("writer");

        }

        private string cssContent;

        public CssCompressor(string css)
        {
            cssContent = css;
        }

        public string Compress()
        {
            // temporarily change semicolons in web links
            cssContent = cssContent.Replace("://", "[!semi-colon!]//");
            // remove html and css comments    
            RemoveComments();
            // trim whitespace from the start and end
            cssContent = cssContent.Trim();
            return cssContent;
        }

        private void RemoveComments()
        {
            cssContent = cssContent.Replace("<!--", string.Empty);
            cssContent = cssContent.Replace("-->", string.Empty);

            cssContent = cssContent.Replace("://", "[!semi-colon!]//");

        }

        private void Rgb2Hex()
        {
        }

        private void LongColorToHex()
        {

        }

        private void LongHexToColor()
        {

        }

        private void RemoveZeroMeasurements()
        {

        }

        private void SortCss()
        {

        }

        private void FontWeightTextToNumbers()
        {

        }

        private void ParseRules()
        {

        }

        private void RemoveSpace()
        {

        }

        private void CombineIdenticalSelectors()
        {

        }

        private void RemoveOverwrittenProperties()
        {

        }

        private void CombinePropsList()
        {

        }

        private void CombineProps()
        {

        }

        private void ReduceProp()
        {

        }

        private void ShortHex()
        {

        }
        private void CompressPaddingAndMargins()
        {

        }
        private void RemoveEmptyRules()
        {

        }

        private void CombineIdenticalRules()
        {

        }

    }
}
