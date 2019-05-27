/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.LdapMatchingRuleUseSchema.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using System.Text;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents the definition of a specific matching rule use in the
    ///     directory schema.
    ///     The LdapMatchingRuleUseSchema class represents the definition of a
    ///     matching rule use.  It is used to discover or modify which attributes are
    ///     suitable for use with an extensible matching rule. It contains the name and
    ///     identifier of a matching rule, and a list of attributes which
    ///     it applies to.
    /// </summary>
    /// <seealso cref="LdapAttributeSchema">
    /// </seealso>
    /// <seealso cref="LdapSchemaElement">
    /// </seealso>
    /// <seealso cref="LdapSchema">
    /// </seealso>
    public class LdapMatchingRuleUseSchema : LdapSchemaElement
    {
        /// <summary>
        ///     Returns an array of all the attributes which this matching rule
        ///     applies to.
        /// </summary>
        /// <returns>
        ///     An array of all the attributes which this matching rule applies to.
        /// </returns>
        public virtual string[] Attributes
        {
            get { return attributes; }
        }

        private readonly string[] attributes;

        /// <summary>
        ///     Constructs a matching rule use definition for adding to or deleting
        ///     from the schema.
        /// </summary>
        /// <param name="names">
        ///     Name(s) of the matching rule.
        /// </param>
        /// <param name="oid">
        ///     Object Identifier of the the matching rule
        ///     in dotted-decimal format.
        /// </param>
        /// <param name="description">
        ///     Optional description of the matching rule use.
        /// </param>
        /// <param name="obsolete">
        ///     True if the matching rule use is obsolete.
        /// </param>
        /// <param name="attributes">
        ///     List of attributes that this matching rule
        ///     applies to. These values may be either the
        ///     names or numeric oids of the attributes.
        /// </param>
        public LdapMatchingRuleUseSchema(string[] names, string oid, string description, bool obsolete,
            string[] attributes) : base(LdapSchema.schemaTypeNames[LdapSchema.MATCHING_USE])
        {
            this.names = new string[names.Length];
            names.CopyTo(this.names, 0);
            this.oid = oid;
            this.description = description;
            this.obsolete = obsolete;
            this.attributes = new string[attributes.Length];
            attributes.CopyTo(this.attributes, 0);
            Value = formatString();
        }


        /// <summary>
        ///     Constructs a matching rule use definition from the raw string value
        ///     returned on a schema query for matchingRuleUse.
        /// </summary>
        /// <param name="raw">
        ///     The raw string value returned on a schema
        ///     query for matchingRuleUse.
        /// </param>
        public LdapMatchingRuleUseSchema(string raw) : base(LdapSchema.schemaTypeNames[LdapSchema.MATCHING_USE])
        {
            try
            {
                var matchParser = new SchemaParser(raw);
                names = new string[matchParser.Names.Length];
                matchParser.Names.CopyTo(names, 0);
                oid = matchParser.ID;
                description = matchParser.Description;
                obsolete = matchParser.Obsolete;
                attributes = matchParser.Applies;
                Value = formatString();
            }
            catch (IOException ex)
            {
                Logger.Log.LogWarning("Exception swallowed", ex);
            }
        }

        /// <summary>
        ///     Returns a string in a format suitable for directly adding to a
        ///     directory, as a value of the particular schema element attribute.
        /// </summary>
        /// <returns>
        ///     A string representation of the attribute's definition.
        /// </returns>
        protected internal override string formatString()
        {
            var valueBuffer = new StringBuilder("( ");
            string token;
            string[] strArray;

            if ((object) (token = ID) != null)
            {
                valueBuffer.Append(token);
            }
            strArray = Names;
            if (strArray != null)
            {
                valueBuffer.Append(" NAME ");
                if (strArray.Length == 1)
                {
                    valueBuffer.Append("'" + strArray[0] + "'");
                }
                else
                {
                    valueBuffer.Append("( ");

                    for (var i = 0; i < strArray.Length; i++)
                    {
                        valueBuffer.Append(" '" + strArray[i] + "'");
                    }
                    valueBuffer.Append(" )");
                }
            }
            if ((object) (token = Description) != null)
            {
                valueBuffer.Append(" DESC ");
                valueBuffer.Append("'" + token + "'");
            }
            if (Obsolete)
            {
                valueBuffer.Append(" OBSOLETE");
            }
            if ((strArray = Attributes) != null)
            {
                valueBuffer.Append(" APPLIES ");
                if (strArray.Length > 1)
                    valueBuffer.Append("( ");
                for (var i = 0; i < strArray.Length; i++)
                {
                    if (i > 0)
                        valueBuffer.Append(" $ ");
                    valueBuffer.Append(strArray[i]);
                }
                if (strArray.Length > 1)
                    valueBuffer.Append(" )");
            }
            valueBuffer.Append(" )");
            return valueBuffer.ToString();
        }
    }
}