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
// Novell.Directory.Ldap.LdapAttributeSchema.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.IO;
using System.Text;

namespace Novell.Directory.Ldap.Utilclass
{
    public class SchemaParser
    {
        private void InitBlock()
        {
            usage = LdapAttributeSchema.USER_APPLICATIONS;
            qualifiers = new ArrayList();
        }

        public virtual string RawString
        {
            get { return rawString; }

            set { rawString = value; }
        }

        public virtual string[] Names
        {
            get { return names; }
        }

        public virtual IEnumerator Qualifiers
        {
            get { return new ArrayEnumeration(qualifiers.ToArray()); }
        }

        public virtual string ID
        {
            get { return id; }
        }

        public virtual string Description
        {
            get { return description; }
        }

        public virtual string Syntax
        {
            get { return syntax; }
        }

        public virtual string Superior
        {
            get { return superior; }
        }

        public virtual bool Single
        {
            get { return single; }
        }

        public virtual bool Obsolete
        {
            get { return obsolete; }
        }

        public virtual string Equality
        {
            get { return equality; }
        }

        public virtual string Ordering
        {
            get { return ordering; }
        }

        public virtual string Substring
        {
            get { return substring; }
        }

        public virtual bool Collective
        {
            get { return collective; }
        }

        public virtual bool UserMod
        {
            get { return userMod; }
        }

        public virtual int Usage
        {
            get { return usage; }
        }

        public virtual int Type
        {
            get { return type; }
        }

        public virtual string[] Superiors
        {
            get { return superiors; }
        }

        public virtual string[] Required
        {
            get { return required; }
        }

        public virtual string[] Optional
        {
            get { return optional; }
        }

        public virtual string[] Auxiliary
        {
            get { return auxiliary; }
        }

        public virtual string[] Precluded
        {
            get { return precluded; }
        }

        public virtual string[] Applies
        {
            get { return applies; }
        }

        public virtual string NameForm
        {
            get { return nameForm; }
        }

        public virtual string ObjectClass
        {
            get { return nameForm; }
        }

        internal string rawString;
        internal string[] names;
        internal string id;
        internal string description;
        internal string syntax;
        internal string superior;
        internal string nameForm;
        internal string objectClass;
        internal string[] superiors;
        internal string[] required;
        internal string[] optional;
        internal string[] auxiliary;
        internal string[] precluded;
        internal string[] applies;
        internal bool single;
        internal bool obsolete;
        internal string equality;
        internal string ordering;
        internal string substring;
        internal bool collective;
        internal bool userMod = true;
        internal int usage;
        internal int type = -1;
        internal int result;
        internal ArrayList qualifiers;

        public SchemaParser(string aString)
        {
            InitBlock();

            int index;

            if ((index = aString.IndexOf('\\')) != -1)
            {
                /*
                * Unless we escape the slash, StreamTokenizer will interpret the
                * single slash and convert it assuming octal values.
                * Two successive back slashes are intrepreted as one backslash.
                */
                var newString = new StringBuilder(aString.Substring(0, index - 0));
                for (var i = index; i < aString.Length; i++)
                {
                    newString.Append(aString[i]);
                    if (aString[i] == '\\')
                    {
                        newString.Append('\\');
                    }
                }
                rawString = newString.ToString();
            }
            else
            {
                rawString = aString;
            }

            var st2 = new SchemaTokenCreator(new StringReader(rawString));
            st2.OrdinaryCharacter('.');
            st2.OrdinaryCharacters('0', '9');
            st2.OrdinaryCharacter('{');
            st2.OrdinaryCharacter('}');
            st2.OrdinaryCharacter('_');
            st2.OrdinaryCharacter(';');
            st2.WordCharacters('.', '9');
            st2.WordCharacters('{', '}');
            st2.WordCharacters('_', '_');
            st2.WordCharacters(';', ';');
            //First parse out the OID
            try
            {
                string currName;
                if ((int) TokenTypes.EOF != st2.nextToken())
                {
                    if (st2.lastttype == '(')
                    {
                        if ((int) TokenTypes.WORD == st2.nextToken())
                        {
                            id = st2.StringValue;
                        }
                        while ((int) TokenTypes.EOF != st2.nextToken())
                        {
                            if (st2.lastttype == (int) TokenTypes.WORD)
                            {
                                if (st2.StringValue.ToUpper().Equals("NAME".ToUpper()))
                                {
                                    if (st2.nextToken() == '\'')
                                    {
                                        names = new string[1];
                                        names[0] = st2.StringValue;
                                    }
                                    else
                                    {
                                        if (st2.lastttype == '(')
                                        {
                                            var nameList = new ArrayList();
                                            while (st2.nextToken() == '\'')
                                            {
                                                if ((object) st2.StringValue != null)
                                                {
                                                    nameList.Add(st2.StringValue);
                                                }
                                            }
                                            if (nameList.Count > 0)
                                            {
                                                names = new string[nameList.Count];
                                                SupportClass.ArrayListSupport.ToArray(nameList, names);
                                            }
                                        }
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("DESC".ToUpper()))
                                {
                                    if (st2.nextToken() == '\'')
                                    {
                                        description = st2.StringValue;
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("SYNTAX".ToUpper()))
                                {
                                    result = st2.nextToken();
                                    if (result == (int) TokenTypes.WORD || result == '\'')
                                        //Test for non-standard schema
                                    {
                                        syntax = st2.StringValue;
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("EQUALITY".ToUpper()))
                                {
                                    if (st2.nextToken() == (int) TokenTypes.WORD)
                                    {
                                        equality = st2.StringValue;
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("ORDERING".ToUpper()))
                                {
                                    if (st2.nextToken() == (int) TokenTypes.WORD)
                                    {
                                        ordering = st2.StringValue;
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("SUBSTR".ToUpper()))
                                {
                                    if (st2.nextToken() == (int) TokenTypes.WORD)
                                    {
                                        substring = st2.StringValue;
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("FORM".ToUpper()))
                                {
                                    if (st2.nextToken() == (int) TokenTypes.WORD)
                                    {
                                        nameForm = st2.StringValue;
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("OC".ToUpper()))
                                {
                                    if (st2.nextToken() == (int) TokenTypes.WORD)
                                    {
                                        objectClass = st2.StringValue;
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("SUP".ToUpper()))
                                {
                                    var values = new ArrayList();
                                    st2.nextToken();
                                    if (st2.lastttype == '(')
                                    {
                                        st2.nextToken();
                                        while (st2.lastttype != ')')
                                        {
                                            if (st2.lastttype != '$')
                                            {
                                                values.Add(st2.StringValue);
                                            }
                                            st2.nextToken();
                                        }
                                    }
                                    else
                                    {
                                        values.Add(st2.StringValue);
                                        superior = st2.StringValue;
                                    }
                                    if (values.Count > 0)
                                    {
                                        superiors = new string[values.Count];
                                        SupportClass.ArrayListSupport.ToArray(values, superiors);
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("SINGLE-VALUE".ToUpper()))
                                {
                                    single = true;
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("OBSOLETE".ToUpper()))
                                {
                                    obsolete = true;
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("COLLECTIVE".ToUpper()))
                                {
                                    collective = true;
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("NO-USER-MODIFICATION".ToUpper()))
                                {
                                    userMod = false;
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("MUST".ToUpper()))
                                {
                                    var values = new ArrayList();
                                    st2.nextToken();
                                    if (st2.lastttype == '(')
                                    {
                                        st2.nextToken();
                                        while (st2.lastttype != ')')
                                        {
                                            if (st2.lastttype != '$')
                                            {
                                                values.Add(st2.StringValue);
                                            }
                                            st2.nextToken();
                                        }
                                    }
                                    else
                                    {
                                        values.Add(st2.StringValue);
                                    }
                                    if (values.Count > 0)
                                    {
                                        required = new string[values.Count];
                                        SupportClass.ArrayListSupport.ToArray(values, required);
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("MAY".ToUpper()))
                                {
                                    var values = new ArrayList();
                                    st2.nextToken();
                                    if (st2.lastttype == '(')
                                    {
                                        st2.nextToken();
                                        while (st2.lastttype != ')')
                                        {
                                            if (st2.lastttype != '$')
                                            {
                                                values.Add(st2.StringValue);
                                            }
                                            st2.nextToken();
                                        }
                                    }
                                    else
                                    {
                                        values.Add(st2.StringValue);
                                    }
                                    if (values.Count > 0)
                                    {
                                        optional = new string[values.Count];
                                        SupportClass.ArrayListSupport.ToArray(values, optional);
                                    }
                                    continue;
                                }

                                if (st2.StringValue.ToUpper().Equals("NOT".ToUpper()))
                                {
                                    var values = new ArrayList();
                                    st2.nextToken();
                                    if (st2.lastttype == '(')
                                    {
                                        st2.nextToken();
                                        while (st2.lastttype != ')')
                                        {
                                            if (st2.lastttype != '$')
                                            {
                                                values.Add(st2.StringValue);
                                            }
                                            st2.nextToken();
                                        }
                                    }
                                    else
                                    {
                                        values.Add(st2.StringValue);
                                    }
                                    if (values.Count > 0)
                                    {
                                        precluded = new string[values.Count];
                                        SupportClass.ArrayListSupport.ToArray(values, precluded);
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("AUX".ToUpper()))
                                {
                                    var values = new ArrayList();
                                    st2.nextToken();
                                    if (st2.lastttype == '(')
                                    {
                                        st2.nextToken();
                                        while (st2.lastttype != ')')
                                        {
                                            if (st2.lastttype != '$')
                                            {
                                                values.Add(st2.StringValue);
                                            }
                                            st2.nextToken();
                                        }
                                    }
                                    else
                                    {
                                        values.Add(st2.StringValue);
                                    }
                                    if (values.Count > 0)
                                    {
                                        auxiliary = new string[values.Count];
                                        SupportClass.ArrayListSupport.ToArray(values, auxiliary);
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("ABSTRACT".ToUpper()))
                                {
                                    type = LdapObjectClassSchema.ABSTRACT;
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("STRUCTURAL".ToUpper()))
                                {
                                    type = LdapObjectClassSchema.STRUCTURAL;
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("AUXILIARY".ToUpper()))
                                {
                                    type = LdapObjectClassSchema.AUXILIARY;
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("USAGE".ToUpper()))
                                {
                                    if (st2.nextToken() == (int) TokenTypes.WORD)
                                    {
                                        currName = st2.StringValue;
                                        if (currName.ToUpper().Equals("directoryOperation".ToUpper()))
                                        {
                                            usage = LdapAttributeSchema.DIRECTORY_OPERATION;
                                        }
                                        else if (currName.ToUpper().Equals("distributedOperation".ToUpper()))
                                        {
                                            usage = LdapAttributeSchema.DISTRIBUTED_OPERATION;
                                        }
                                        else if (currName.ToUpper().Equals("dSAOperation".ToUpper()))
                                        {
                                            usage = LdapAttributeSchema.DSA_OPERATION;
                                        }
                                        else if (currName.ToUpper().Equals("userApplications".ToUpper()))
                                        {
                                            usage = LdapAttributeSchema.USER_APPLICATIONS;
                                        }
                                    }
                                    continue;
                                }
                                if (st2.StringValue.ToUpper().Equals("APPLIES".ToUpper()))
                                {
                                    var values = new ArrayList();
                                    st2.nextToken();
                                    if (st2.lastttype == '(')
                                    {
                                        st2.nextToken();
                                        while (st2.lastttype != ')')
                                        {
                                            if (st2.lastttype != '$')
                                            {
                                                values.Add(st2.StringValue);
                                            }
                                            st2.nextToken();
                                        }
                                    }
                                    else
                                    {
                                        values.Add(st2.StringValue);
                                    }
                                    if (values.Count > 0)
                                    {
                                        applies = new string[values.Count];
                                        SupportClass.ArrayListSupport.ToArray(values, applies);
                                    }
                                    continue;
                                }
                                currName = st2.StringValue;
                                var q = parseQualifier(st2, currName);
                                if (q != null)
                                {
                                    qualifiers.Add(q);
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        private AttributeQualifier parseQualifier(SchemaTokenCreator st, string name)
        {
            var values = new ArrayList(5);
            try
            {
                if (st.nextToken() == '\'')
                {
                    values.Add(st.StringValue);
                }
                else
                {
                    if (st.lastttype == '(')
                    {
                        while (st.nextToken() == '\'')
                        {
                            values.Add(st.StringValue);
                        }
                    }
                }
            }
            catch (IOException e)
            {
                throw e;
            }
            var valArray = new string[values.Count];
            valArray = (string[]) SupportClass.ArrayListSupport.ToArray(values, valArray);
            return new AttributeQualifier(name, valArray);
        }
    }
}