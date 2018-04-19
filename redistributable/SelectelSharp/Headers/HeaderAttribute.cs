using System;

namespace SelectelSharp.Headers
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class HeaderAttribute : Attribute
    {
        readonly string headerKey;

        public HeaderAttribute(string headerKey = null)
        {
            this.headerKey = headerKey;            
        }

        public string HeaderKey
        {
            get { return headerKey; }
        }

        public bool CustomHeaders { get; set; }
        public bool CORSHeaders { get; set; }
    }
}
