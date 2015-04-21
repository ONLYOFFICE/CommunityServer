#region Copyright © 2008 MSBuild Community Task Project. All rights reserved.
/*
Copyright © 2008 MSBuild Community Task Project. All rights reserved.

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
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Compiles regular expressions and saves them to disk in an assembly.
    /// </summary>
    /// <include file='AdditionalDocumentation.xml' path='docs/task[@name="RegexCompiler"]/*'/>
    public class RegexCompiler : Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegexCompiler"/> class.
        /// </summary>
        public RegexCompiler()
        {
            AssemblyVersion = "1.0.0.0";
            IsPublic = true;
        }

        #region Properties
        /// <summary>
        /// Gets or sets the name of the assembly to be created by the regex compiler.
        /// </summary>
        /// <value>The name of the assembly.</value>
        [Required]
        public string AssemblyName { get; set; }
        /// <summary>
        /// Gets or sets the assembly title.
        /// </summary>
        /// <value>The assembly title.</value>
        public string AssemblyTitle { get; set; }
        /// <summary>
        /// Gets or sets the assembly description.
        /// </summary>
        /// <value>The assembly description.</value>
        public string AssemblyDescription { get; set; }
        /// <summary>
        /// Gets or sets the assembly company.
        /// </summary>
        /// <value>The assembly company.</value>
        public string AssemblyCompany { get; set; }
        /// <summary>
        /// Gets or sets the assembly product.
        /// </summary>
        /// <value>The assembly product.</value>
        public string AssemblyProduct { get; set; }
        /// <summary>
        /// Gets or sets the assembly copyright.
        /// </summary>
        /// <value>The assembly copyright.</value>
        public string AssemblyCopyright { get; set; }
        /// <summary>
        /// Gets or sets the assembly culture.
        /// </summary>
        /// <value>The assembly culture.</value>
        public string AssemblyCulture { get; set; }
        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        /// <value>The assembly version.</value>
        public string AssemblyVersion { get; set; }
        /// <summary>
        /// Gets or sets the assembly file version.
        /// </summary>
        /// <value>The assembly file version.</value>
        public string AssemblyFileVersion { get; set; }
        /// <summary>
        /// Gets or sets the assembly informational version.
        /// </summary>
        /// <value>The assembly informational version.</value>
        public string AssemblyInformationalVersion { get; set; }
        /// <summary>
        /// Gets or sets the assembly key file.
        /// </summary>
        /// <value>The assembly key file.</value>
        public string AssemblyKeyFile { get; set; }
        /// <summary>
        /// Gets or sets the directory where the assembly will be saved.
        /// </summary>
        /// <value>The output directory.</value>
        [Required]
        public string OutputDirectory { get; set; }
        /// <summary>
        /// Gets or sets the output file.
        /// </summary>
        /// <value>The output file.</value>
        [Output]
        public ITaskItem OutputFile { get; set; }
        /// <summary>
        /// Gets or sets the regular expressions.
        /// </summary>
        /// <value>The regular expressions.</value>
        public ITaskItem[] RegularExpressions { get; set; }
        /// <summary>
        /// Gets or sets the file defining the regular expressions.
        /// </summary>
        /// <value>The regular expressions file.</value>
        public ITaskItem RegularExpressionsFile { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the default value is public for regular expression instances.
        /// </summary>
        /// <value><c>true</c> if regular expression instance is public; otherwise, <c>false</c>.</value>
        public bool IsPublic { set; get; }
        /// <summary>
        /// Gets or sets the default namespace for regular expression instances.
        /// </summary>
        /// <value>The namespace for regular expression instances.</value>
        public string Namespace { set; get; }
        /// <summary>
        /// Gets or sets the default regular expression options.
        /// </summary>
        /// <value>The default regular expression options.</value>
        public string Options { set; get; }

        #endregion

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {

            try
            {
                Validate();

                var regexCompilations = new List<RegexCompilationInfo>();
                regexCompilations.AddRange(GetRegexCompilation());
                regexCompilations.AddRange(GetRegexCompilationFile());

                RegexCompilationInfo[] infoArray = regexCompilations.ToArray();
                AssemblyName name = GetAssemblyName();
                CustomAttributeBuilder[] attributes = GetAttributes();

                //changing the enviroment directory because CompileToAssembly 
                //does not provide a way to specify an output directory.
                string currentDirectory = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = OutputDirectory;
                    System.Text.RegularExpressions.Regex.CompileToAssembly(infoArray, name, attributes);
                }
                finally
                {
                    Environment.CurrentDirectory = currentDirectory;
                }

                OutputFile = new TaskItem(Path.Combine(OutputDirectory, Path.GetFileName(AssemblyName)));

                Log.LogMessage("RegexCompiler created assembly '{0}'.", name.Name);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }

        private void Validate()
        {
            string extension = Path.GetExtension(AssemblyName);
            if (!string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
                AssemblyName += ".dll";

            AssemblyVersion = ValidateVersion(AssemblyVersion, "1.0.0.0");
            AssemblyFileVersion = ValidateVersion(AssemblyFileVersion, AssemblyVersion);
            AssemblyInformationalVersion = ValidateVersion(AssemblyInformationalVersion, AssemblyFileVersion);
        }

        private string ValidateVersion(string version, string defaultVersion)
        {
            if (string.IsNullOrEmpty(version))
                return defaultVersion;

            try
            {
                var v = new System.Version(version);
                return v.ToString();
            }
            catch (Exception ex)
            {
                Log.LogWarningFromException(ex, false);
                return defaultVersion;
            }
        }

        private AssemblyName GetAssemblyName()
        {
            var version = new System.Version(AssemblyVersion);

            var name = new AssemblyName();
            name.Name = Path.GetFileNameWithoutExtension(AssemblyName);
            name.Version = version;

            if (string.IsNullOrEmpty(AssemblyKeyFile) || !File.Exists(AssemblyKeyFile)) 
                return name;

            using (var fs = File.OpenRead(AssemblyKeyFile))
            {
                var keyPair = new StrongNameKeyPair(fs);
                name.KeyPair = keyPair;
            }

            return name;
        }

        private static CustomAttributeBuilder GetAttribute<T>(string value) where T : Attribute
        {
            var constructor = typeof(T).GetConstructor(new[] { typeof(string) });
            var builder = new CustomAttributeBuilder(constructor, new object[] { value ?? string.Empty });
            return builder;
        }

        private CustomAttributeBuilder[] GetAttributes()
        {
            var attributes = new List<CustomAttributeBuilder>();

            attributes.Add(GetAttribute<AssemblyTitleAttribute>(AssemblyTitle));
            attributes.Add(GetAttribute<AssemblyDescriptionAttribute>(AssemblyDescription));
            attributes.Add(GetAttribute<AssemblyCompanyAttribute>(AssemblyCompany));
            attributes.Add(GetAttribute<AssemblyProductAttribute>(AssemblyProduct));
            attributes.Add(GetAttribute<AssemblyCopyrightAttribute>(AssemblyCopyright));

            attributes.Add(GetAttribute<AssemblyCultureAttribute>(AssemblyCulture));

            attributes.Add(GetAttribute<AssemblyVersionAttribute>(AssemblyVersion));
            attributes.Add(GetAttribute<AssemblyFileVersionAttribute>(AssemblyFileVersion));
            attributes.Add(GetAttribute<AssemblyInformationalVersionAttribute>(AssemblyInformationalVersion));

            return attributes.ToArray();
        }

        private List<RegexCompilationInfo> GetRegexCompilation()
        {
            RegexOptions defaultRegexOptions = GetRegexOptions(Options);

            var regexList = new List<RegexCompilationInfo>();
            if (RegularExpressions == null)
                return regexList;

            foreach (ITaskItem expression in RegularExpressions)
            {
                string name = expression.ItemSpec;

                string pattern = expression.GetMetadata("Pattern");
                string nspace = expression.GetMetadata("Namespace");
                string options = expression.GetMetadata("Options");
                string pub = expression.GetMetadata("IsPublic");

                if (string.IsNullOrEmpty(pattern))
                    throw new InvalidOperationException(string.Format(
                        "The regular expression '{0}' is missing the Pattern metadata.", name));

                if (string.IsNullOrEmpty(nspace))
                    nspace = Namespace;
                if (string.IsNullOrEmpty(nspace))
                    nspace = Path.GetFileNameWithoutExtension(AssemblyName);

                RegexOptions regexOptions = string.IsNullOrEmpty(options)
                                                ? defaultRegexOptions
                                                : GetRegexOptions(options);

                bool isPublic;

                if (!bool.TryParse(pub, out isPublic))
                    isPublic = IsPublic;

                var info = new RegexCompilationInfo(pattern, regexOptions, name, nspace, isPublic);
                regexList.Add(info);
            }

            return regexList;
        }

        private List<RegexCompilationInfo> GetRegexCompilationFile()
        {
            RegexOptions defaultRegexOptions = GetRegexOptions(Options);
            string defaultNamespace = Namespace;
            if (string.IsNullOrEmpty(defaultNamespace))
                defaultNamespace = Path.GetFileNameWithoutExtension(AssemblyName);

            var regexList = new List<RegexCompilationInfo>();
            if (RegularExpressionsFile == null)
                return null;

            if (!File.Exists(RegularExpressionsFile.ItemSpec))
            {
                Log.LogWarning("The expressions file '{0}' does not exists.", RegularExpressionsFile.ItemSpec);
                return null;
            }
            Log.LogMessage(MessageImportance.Low,  "Loading expressions file '{0}'.", RegularExpressionsFile.ItemSpec);

            using (FileStream fs = File.OpenRead(RegularExpressionsFile.ItemSpec))
            using (XmlReader reader = XmlReader.Create(fs))
            {
                RegexCompilationInfo current = null;

                while (reader.Read())
                {
                    if ((reader.NodeType != XmlNodeType.Element))
                        continue;

                    switch (reader.Name)
                    {
                        case "Regex":
                            string name = reader.GetAttribute("Name");
                            current = new RegexCompilationInfo("", defaultRegexOptions, name, defaultNamespace, IsPublic);
                            regexList.Add(current);
                            break;
                        case "Pattern":
                            if (current == null)
                                break;

                            string pattern = reader.ReadElementContentAsString();
                            if (string.IsNullOrEmpty(pattern))
                                throw new InvalidOperationException(string.Format(
                                    "The regular expression '{0}' is missing the Pattern metadata.", current.Name));

                            current.Pattern = pattern;

                            break;
                        case "Namespace":
                            if (current == null)
                                break;

                            string nspace = reader.ReadElementContentAsString();
                            if (string.IsNullOrEmpty(nspace))
                                break;

                            current.Namespace = nspace;
                            break;
                        case "Options":
                            if (current == null)
                                break;

                            string options = reader.ReadElementContentAsString();
                            if (string.IsNullOrEmpty(options))
                                break;

                            RegexOptions regexOptions = GetRegexOptions(options);
                            current.Options = regexOptions;

                            break;
                        case "IsPublic":
                            if (current == null)
                                break;

                            bool isPublic;
                            string pub = reader.ReadElementContentAsString();
                            if (!bool.TryParse(pub, out isPublic))
                                break;

                            current.IsPublic = isPublic;
                            break;
                    } // switch
                } // whild read
            } // using

            return regexList;
        }

        private RegexOptions GetRegexOptions(string options)
        {
            if (string.IsNullOrEmpty(options))
                return RegexOptions.None;

            RegexOptions regexOptions = RegexOptions.None;

            //remove RegexOptions prefix
            string cleaned = options.Replace("RegexOptions.", "");

            string[] names = cleaned.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string name in names)
            {
                try
                {
                    regexOptions |= (RegexOptions)Enum.Parse(typeof(RegexOptions), name.Trim(), true);
                }
                catch (Exception)
                {
                    Log.LogWarning("'{0}' is not an option in RegexOptions.", name);
                }
            }

            return regexOptions;
        }
    }
}
