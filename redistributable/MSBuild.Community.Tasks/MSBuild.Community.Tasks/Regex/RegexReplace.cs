#region Copyright © 2006 Andy Johns. All rights reserved.
/*
Copyright © 2006 Andy Johns. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion



using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Task to replace portions of strings within the Input list
    /// Output list contains all the elements of the Input list after
    /// performing the Regex Replace.
    /// </summary>
    /// <example>
    /// 1st example replaces first occurance of "foo." with empty string
    /// 2nd example replaces occurance of "foo." after character 6 with "oop." string
    /// <code><![CDATA[
    /// <ItemGroup>
    ///    <TestGroup Include="foo.my.foo.foo.test.o" />
    ///    <TestGroup Include="foo.my.faa.foo.test.a" />
    ///    <TestGroup Include="foo.my.fbb.foo.test.b" />
    ///    <TestGroup Include="foo.my.fcc.foo.test.c" />
    ///    <TestGroup Include="foo.my.fdd.foo.test.d" />
    ///    <TestGroup Include="foo.my.fee.foo.test.e" />
    ///    <TestGroup Include="foo.my.fff.foo.test.f" />
    /// </ItemGroup>
    /// <Target Name="Test">
    ///    <Message Text="Input:&#xA;@(TestGroup, '&#xA;')"/>
    ///    <!-- Replaces first occurance of "foo." with empty string-->
    ///    <RegexReplace Input="@(TestGroup)" Expression="foo\." Replacement="" Count="1">
    ///       <Output ItemName ="ReplaceReturn1" TaskParameter="Output" />
    ///    </RegexReplace>
    ///    <Message Text="&#xA;Output Replace 1:&#xA;@(ReplaceReturn1, '&#xA;')" />
    ///    <!-- Replaces occurance of "foo." after character 6 with "oop." string-->
    ///    <RegexReplace Input="@(TestGroup)" Expression="foo\." Replacement="oop" Startat="6">
    ///       <Output ItemName ="ReplaceReturn2" TaskParameter="Output" />
    ///    </RegexReplace>
    ///    <Message Text="&#xA;Output Replace 2:&#xA;@(ReplaceReturn2, '&#xA;')" />
    /// </Target>
    /// ]]></code>
    /// </example>
    public class RegexReplace : RegexBase
    {
        private ITaskItem replacement = null;
        private int count = -1;
        private int startat = 0;

        /// <summary>
        /// String replacing matching expression strings in input list.
        /// If left empty matches in the input list are removed (replaced with empty string)
        /// </summary>
        public ITaskItem Replacement
        {
            get { return replacement; }
            set { replacement = value; }
        }

        /// <summary>
        /// Number of matches to allow on each input item.
        /// -1 indicates to perform matches on all matches within input item
        /// </summary>
        public ITaskItem Count
        {
            get { return new TaskItem(count.ToString()); }
            set { count = int.Parse(value.ItemSpec); }
        }

        /// <summary>
        /// Position within the input item to start matching
        /// </summary>
        public ITaskItem StartAt
        {
            get { return new TaskItem(startat.ToString()); }
            set { startat = int.Parse(value.ItemSpec); }
        }

        /// <summary>
        /// Performs the Replace task
        /// </summary>
        /// <returns><see langword="true"/> if the task ran successfully; 
        /// otherwise <see langword="false"/>.</returns>
        public override bool Execute()
        {
            var expressionOptions = ExpressionOptions;

            var regex = new Regex(Expression.ItemSpec, expressionOptions);
            if (Path.DirectorySeparatorChar == '/')
            {
                regex = new Regex(Expression.ItemSpec.Replace('/', '\\'), expressionOptions);
            }

            Output = new TaskItem[Input.Length];
            string replaceValue = "";
            if (replacement != null) replaceValue = replacement.ItemSpec;

            for(int ndx = 0; ndx < Input.Length; ndx++)
            {

                if ((expressionOptions & RegexOptions.RightToLeft) == RegexOptions.RightToLeft)
                    startat = Input[ndx].ItemSpec.Length;

                Output[ndx] = new TaskItem(Input[ndx]);
                Output[ndx].ItemSpec = regex.Replace(Input[ndx].ItemSpec, replaceValue, count, startat);
            }

            return !Log.HasLoggedErrors;
        }
    }
}
