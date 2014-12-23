namespace SlowCheetah.Xdt {
    using Microsoft.Web.XmlTransform;
    using System;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class AttributeRegexReplace : Transform {
        private string pattern;
        private string replacement;
        private string attributeName;

        protected string AttributeName {
            get {
                if (this.attributeName == null) {
                    this.attributeName = this.GetArgumentValue("Attribute");
                }
                return this.attributeName;
            }
        }
        protected string Pattern {
            get {
                if (this.pattern == null) {
                    this.pattern = this.GetArgumentValue("Pattern");
                }

                return pattern;
            }
        }

        protected string Replacement {
            get {
                if (this.replacement == null) {
                    this.replacement = this.GetArgumentValue("Replacement");
                }

                return replacement;
            }
        }

        protected string GetArgumentValue(string name) {
            // this extracts a value from the arguments provided
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentNullException("name"); }

            string result = null;
            if (this.Arguments != null && this.Arguments.Count > 0) {
                foreach (string arg in this.Arguments) {
                    if (!string.IsNullOrWhiteSpace(arg)) {
                        string trimmedArg = arg.Trim();
                        if (trimmedArg.ToUpperInvariant().StartsWith(name.ToUpperInvariant())) {
                            int start = arg.IndexOf('\'');
                            int last = arg.LastIndexOf('\'');
                            if (start <= 0 || last <= 0 || last <= 0) {
                                throw new ArgumentException("Expected two ['] characters");
                            }

                            string value = trimmedArg.Substring(start, last - start);
                            if (value != null) {
                                // remove any leading or trailing '
                                value = value.Trim().TrimStart('\'').TrimStart('\'');
                            }
                            result = value;
                        }
                    }
                }
            }
            return result;
        }

        protected override void Apply() {
            foreach (XmlAttribute att in this.TargetNode.Attributes) {
                if (string.Compare(att.Name, this.AttributeName, StringComparison.InvariantCultureIgnoreCase) == 0) {
                    // get current value, perform the Regex
                    att.Value = Regex.Replace(att.Value, this.Pattern, this.Replacement);
                }
            }
        }
    }
}
