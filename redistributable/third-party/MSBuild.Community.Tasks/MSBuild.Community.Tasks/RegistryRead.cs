#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

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
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Win32;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Reads a value from the Registry
    /// </summary>
    /// <example>Read .NET Framework install root from Registry.
    /// <code><![CDATA[
    /// <RegistryRead 
    ///     KeyName="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework" 
    ///     ValueName="InstallRoot">
    ///     <Output TaskParameter="Value" PropertyName="InstallRoot" />
    /// </RegistryRead>
    /// <Message Text="InstallRoot: $(InstallRoot)"/>
    /// ]]></code>
    /// </example>
    /// <remarks>The <see cref="Value"/> parameter is set according to the following rules:
    /// <list type="table"><item><description>If a <see cref="DefaultValue"/> is provided, it will be used if the <see cref="KeyName"/> or <see cref="ValueName"/> does not exist.</description></item>
    /// <item><description>If a <see cref="DefaultValue"/> is not provided, the <see cref="KeyName"/> exists, but the <see cref="ValueName"/> does not exist, <see cref="Value"/> will be set to an empty string.</description></item>
    /// <item><description>If a <see cref="DefaultValue"/> is not provided, and the <see cref="KeyName"/> does not exist, the task will fail.</description></item></list></remarks>
    public class RegistryRead : Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:RegistryRead"/> class.
        /// </summary>
        public RegistryRead()
        {
        }

        #region Properties

        private string _keyName;

        /// <summary>
        /// Gets or sets the full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".
        /// </summary>
        /// <value>The name of the key.</value>
        [Required]
        public string KeyName
        {
            get { return _keyName; }
            set { _keyName = value; }
        }

        private string _valueName;

        /// <summary>
        /// Gets or sets the name of the name/value pair.
        /// </summary>
        /// <value>The name of the value.</value>
        public string ValueName
        {
            get { return _valueName; }
            set { _valueName = value; }
        }

        private string _defaultValue;

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>The default value.</value>
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }


        private string _value;

        /// <summary>
        /// Gets the stored value.
        /// </summary>
        /// <value>The value.</value>
        [Output]
        public string Value
        {
            get { return _value; }
        }

        #endregion

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            Log.LogMessage(Properties.Resources.RegistryRead);
            string defaultValueForCall = _defaultValue ?? String.Empty;
            object key = Registry.GetValue(_keyName, _valueName, defaultValueForCall);
            if (key == null)
            {
                _value = _defaultValue;
                Log.LogWarning("Failed to read registry key '{0}'.", _keyName);
                return true;
            }
            _value = key.ToString() ?? string.Empty;

            Log.LogMessage(MessageImportance.Low, "[{0}]", _keyName);
            if(string.IsNullOrEmpty(_valueName))
                Log.LogMessage(MessageImportance.Low, "@=\"{0}\"", _value);
            else
                Log.LogMessage(MessageImportance.Low, "\"{0}\"=\"{1}\"", _valueName, _value);

            return true;
        }
    }
}
