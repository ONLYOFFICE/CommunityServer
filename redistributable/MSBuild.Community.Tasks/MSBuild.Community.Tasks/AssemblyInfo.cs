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
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Generates an AssemblyInfo files
    /// </summary>
    /// <example>
    /// <para>Generates a common version file.</para>
    /// <code><![CDATA[
    /// <AssemblyInfo CodeLanguage="CS"  
    ///     OutputFile="VersionInfo.cs" 
    ///     AssemblyVersion="1.0.0.0" 
    ///     AssemblyFileVersion="1.0.0.0" />
    /// ]]></code>
    /// <para>Generates a complete version file.</para>
    /// <code><![CDATA[
    /// <AssemblyInfo CodeLanguage="CS"  
    ///     OutputFile="$(MSBuildProjectDirectory)\Test\GlobalInfo.cs" 
    ///     AssemblyTitle="AssemblyInfoTask" 
    ///     AssemblyDescription="AssemblyInfo Description"
    ///     AssemblyConfiguration=""
    ///     AssemblyCompany="Company Name, LLC"
    ///     AssemblyProduct="AssemblyInfoTask"
    ///     AssemblyCopyright="Copyright (c) Company Name, LLC 2006"
    ///     AssemblyTrademark=""
    ///     ComVisible="false"
    ///     CLSCompliant="true"
    ///     Guid="d038566a-1937-478a-b5c5-b79c4afb253d"
    ///     AssemblyVersion="1.0.0.0" 
    ///     AssemblyFileVersion="1.0.0.0" />
    /// ]]></code>
    /// <para>Generates a complete version file for C++/CLI.</para>
    /// <code><![CDATA[
    /// <AssemblyInfo CodeLanguage="CPP"  
    ///     OutputFile="$(MSBuildProjectDirectory)\Properties\AssemblyInfo.cpp"
    ///     AssemblyTitle="MyAssembly" 
    ///     AssemblyDescription="MyAssembly Description"
    ///     AssemblyConfiguration="$(Configuration)"
    ///     AssemblyCompany="Company Name, LLC"
    ///     AssemblyProduct="MyAssembly"
    ///     AssemblyCopyright="Copyright (c) Company Name, LLC 2008"
    ///     AssemblyTrademark=""
    ///     ComVisible="false"
    ///     CLSCompliant="true"
    ///     Guid="d038566a-1937-478a-b5c5-b79c4afb253d"
    ///     AssemblyVersion="1.0.0.0" 
    ///     AssemblyFileVersion="1.0.0.0"
    ///     UnmanagedCode="true" />
    /// ]]></code>
    /// </example>
    public class AssemblyInfo : Task
    {
        #region Constants

        /// <summary>
        /// The default value of <see cref="OutputFile"/>.
        /// The value is <c>"AssemblyInfo.cs"</c>.
        /// </summary>
        public const string DEFAULT_OUTPUT_FILE = @"AssemblyInfo.cs";

        private const string CSharp = "CS";
        private const string VisualBasic = "VB";
        private const string CPP = "CPP";
        private const string FSharp_cl = "FS";

        private const string CppCodeProviderAssembly = "CppCodeProvider, "+
                                                       "Version=8.0.0.0, " +
                                                       "Culture=neutral, " +
                                                       "PublicKeyToken=b03f5f7f11d50a3a, " +
                                                       "processorArchitecture=MSIL";

        private const string CppCodeProviderType = "Microsoft.VisualC.CppCodeProvider";

        private const string CLSCompliantName = "CLSCompliant";
        private const string ComVisibleName = "ComVisible";
        private const string GuidName = "Guid";
        private const string AssemblyTitleName = "AssemblyTitle";
        private const string AssemblyDescriptionName = "AssemblyDescription";
        private const string AssemblyConfigurationName = "AssemblyConfiguration";
        private const string AssemblyCompanyName = "AssemblyCompany";
        private const string AssemblyProductName = "AssemblyProduct";
        private const string AssemblyCopyrightName = "AssemblyCopyright";
        private const string AssemblyTrademarkName = "AssemblyTrademark";
        private const string AssemblyCultureName = "AssemblyCulture";
        private const string AssemblyVersionName = "AssemblyVersion";
        private const string AssemblyFileVersionName = "AssemblyFileVersion";
        private const string AssemblyInformationalVersionName = "AssemblyInformationalVersion";
        private const string AssemblyKeyFileName = "AssemblyKeyFile";
        private const string AssemblyKeyNameName = "AssemblyKeyName";
        private const string AssemblyDelaySignName = "AssemblyDelaySign";
        private const string SkipVerificationName = "SkipVerification";
        private const string UnmanagedCodeName = "UnmanagedCode";
        private const string InternalsVisibleToName = "InternalsVisibleTo";
        private const string AllowPartiallyTrustedCallersName = "AllowPartiallyTrustedCallers";

        #endregion Constants

        #region Type Constructor

        static AssemblyInfo()
        {
            // C#
            _codeLangMapping["csharp"] = CSharp;
            _codeLangMapping["cs"] = CSharp;
            _codeLangMapping["c#"] = CSharp;
            _codeLangMapping["c sharp"] = CSharp;

            // Visual Basic
            _codeLangMapping["vb"] = VisualBasic;
            _codeLangMapping["visualbasic"] = VisualBasic;
            _codeLangMapping["visual basic"] = VisualBasic;

            // F#
            _codeLangMapping["fsharp"] = FSharp_cl;
            _codeLangMapping["fs"] = FSharp_cl;
            _codeLangMapping["f#"] = FSharp_cl;
            _codeLangMapping["f sharp"] = FSharp_cl;

            // C++/CLI
            _codeLangMapping["cpp"] = CPP;
            _codeLangMapping["c++"] = CPP;
            _codeLangMapping["c++/cli"] = CPP;
            _codeLangMapping["c plus plus"] = CPP;

            _attributeNamespaces[CLSCompliantName] = "System";
            _attributeNamespaces[ComVisibleName] = "System.Runtime.InteropServices";
            _attributeNamespaces[GuidName] = "System.Runtime.InteropServices";
            _attributeNamespaces[AssemblyTitleName] = "System.Reflection";
            _attributeNamespaces[AssemblyDescriptionName] = "System.Reflection";
            _attributeNamespaces[AssemblyConfigurationName] = "System.Reflection";
            _attributeNamespaces[AssemblyCompanyName] = "System.Reflection";
            _attributeNamespaces[AssemblyProductName] = "System.Reflection";
            _attributeNamespaces[AssemblyCopyrightName] = "System.Reflection";
            _attributeNamespaces[AssemblyTrademarkName] = "System.Reflection";
            _attributeNamespaces[AssemblyCultureName] = "System.Reflection";
            _attributeNamespaces[AssemblyVersionName] = "System.Reflection";
            _attributeNamespaces[AssemblyFileVersionName] = "System.Reflection";
            _attributeNamespaces[AssemblyInformationalVersionName] = "System.Reflection";
            _attributeNamespaces[AssemblyKeyFileName] = "System.Reflection";
            _attributeNamespaces[AssemblyKeyNameName] = "System.Reflection";
            _attributeNamespaces[AssemblyDelaySignName] = "System.Reflection";
            _attributeNamespaces[InternalsVisibleToName] = "System.Runtime.CompilerServices";
            _attributeNamespaces[AllowPartiallyTrustedCallersName] = "System.Security";
        }
        #endregion

        #region Type Fields

        private readonly static Dictionary<string, string> _codeLangMapping
            = new Dictionary<string, string>();

        private readonly static Dictionary<string, string> _attributeNamespaces
            = new Dictionary<string, string>();

        private static readonly string[] booleanAttributes = { 
                                                                 CLSCompliantName, 
                                                                 AssemblyDelaySignName, 
                                                                 ComVisibleName 
                                                             };
        private static readonly string[] securityAttributes = {
                                                                  SkipVerificationName, 
                                                                  UnmanagedCodeName
                                                              };
        private static readonly string[] nonGeneratedClassAttributes = {
                                                                           CLSCompliantName,
                                                                           AssemblyDelaySignName,
                                                                           ComVisibleName, 
                                                                           AssemblyKeyFileName,
                                                                           InternalsVisibleToName
                                                                       };
        private static readonly string[] markerAttributes = {
                                                                  AllowPartiallyTrustedCallersName 
                                                            };
        #endregion

        #region Fields
        private readonly Dictionary<string, string> _attributes;
        private string _outputFile;

        #endregion Fields

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo"/> class.
        /// </summary>
        public AssemblyInfo()
        {
            _attributes = new Dictionary<string, string>();
            _outputFile = DEFAULT_OUTPUT_FILE;
        }

        #endregion Constructor

        #region Input Parameters

        /// <summary>
        /// Gets or sets the code language.
        /// </summary>
        /// <value>The code language.</value>
        [Required]
        public string CodeLanguage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [COMVisible].
        /// </summary>
        /// <value><c>true</c> if [COMVisible]; otherwise, <c>false</c>.</value>
        public bool ComVisible
        {
            get { return ReadBooleanAttribute(ComVisibleName); }
            set { _attributes[ComVisibleName] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [CLSCompliant].
        /// </summary>
        /// <value><c>true</c> if [CLSCompliant]; otherwise, <c>false</c>.</value>
        public bool CLSCompliant
        {
            get { return ReadBooleanAttribute(CLSCompliantName); }
            set { _attributes[CLSCompliantName] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>The GUID.</value>
        public string Guid
        {
            get { return ReadAttribute(GuidName); }
            set { _attributes[GuidName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly title.
        /// </summary>
        /// <value>The assembly title.</value>
        public string AssemblyTitle
        {
            get { return ReadAttribute(AssemblyTitleName); }
            set { _attributes[AssemblyTitleName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly description.
        /// </summary>
        /// <value>The assembly description.</value>
        public string AssemblyDescription
        {
            get { return ReadAttribute(AssemblyDescriptionName); }
            set { _attributes[AssemblyDescriptionName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly configuration.
        /// </summary>
        /// <value>The assembly configuration.</value>
        public string AssemblyConfiguration
        {
            get { return ReadAttribute(AssemblyConfigurationName); }
            set { _attributes[AssemblyConfigurationName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly company.
        /// </summary>
        /// <value>The assembly company.</value>
        public string AssemblyCompany
        {
            get { return ReadAttribute(AssemblyCompanyName); }
            set { _attributes[AssemblyCompanyName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly product.
        /// </summary>
        /// <value>The assembly product.</value>
        public string AssemblyProduct
        {
            get { return ReadAttribute(AssemblyProductName); }
            set { _attributes[AssemblyProductName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly copyright.
        /// </summary>
        /// <value>The assembly copyright.</value>
        public string AssemblyCopyright
        {
            get { return ReadAttribute(AssemblyCopyrightName); }
            set { _attributes[AssemblyCopyrightName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly trademark.
        /// </summary>
        /// <value>The assembly trademark.</value>
        public string AssemblyTrademark
        {
            get { return ReadAttribute(AssemblyTrademarkName); }
            set { _attributes[AssemblyTrademarkName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly culture.
        /// </summary>
        /// <value>The assembly culture.</value>
        public string AssemblyCulture
        {
            get { return ReadAttribute(AssemblyCultureName); }
            set { _attributes[AssemblyCultureName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        /// <value>The assembly version.</value>
        public string AssemblyVersion
        {
            get { return ReadAttribute(AssemblyVersionName); }
            set { _attributes[AssemblyVersionName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly file version.
        /// </summary>
        /// <value>The assembly file version.</value>
        public string AssemblyFileVersion
        {
            get { return ReadAttribute(AssemblyFileVersionName); }
            set { _attributes[AssemblyFileVersionName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly informational version.
        /// </summary>
        /// <value>The assembly informational version.</value>
        public string AssemblyInformationalVersion
        {
            get { return ReadAttribute(AssemblyInformationalVersionName); }
            set { _attributes[AssemblyInformationalVersionName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly key file.
        /// </summary>
        public string AssemblyKeyFile
        {
            get { return ReadAttribute(AssemblyKeyFileName); }
            set { _attributes[AssemblyKeyFileName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly key name.
        /// </summary>
        public string AssemblyKeyName
        {
            get { return ReadAttribute(AssemblyKeyNameName); }
            set { _attributes[AssemblyKeyNameName] = value; }
        }

        /// <summary>
        /// Gets or sets the assembly delay sign value.
        /// </summary>
        public bool AssemblyDelaySign
        {
            get { return ReadBooleanAttribute(AssemblyDelaySignName); }
            set { _attributes[AssemblyDelaySignName] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the assembly delay sign value.
        /// </summary>
        public bool SkipVerification
        {
            get { return ReadBooleanAttribute(SkipVerificationName); }
            set { _attributes[SkipVerificationName] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the assembly delay sign value.
        /// </summary>
        public bool UnmanagedCode
        {
            get { return ReadBooleanAttribute(UnmanagedCodeName); }
            set { _attributes[UnmanagedCodeName] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to generate the ThisAssmebly class.
        /// </summary>
        public bool GenerateClass { get; set; }

        /// <summary>
        /// Gets or sets the neutral language which is used as a fallback language configuration 
        /// if the locale on the computer isn't supported. Example is setting this to "en-US".
        /// </summary>
        public string NeutralResourcesLanguage { get; set; }

        /// <summary>
        /// Gets or sets the ultimate resource fallback location.
        /// </summary>
        /// <value>The ultimate resource fallback location.</value>
        public string UltimateResourceFallbackLocation { get; set; }

        /// <summary>
        /// Makes it possible to make certain assemblies able to use constructs marked as internal.
        /// Example might be setting this value to "UnitTests" assembly. The typical use case might 
        /// be constructors in classes which shouldn't be available to other assemblies, but the unit
        /// tests should be able to use them.
        /// </summary>
        public string InternalsVisibleTo
        {
            get { return ReadAttribute(InternalsVisibleToName); }
            set { _attributes[InternalsVisibleToName] = value; }
        }

        /// <summary>
        /// Gets or sets whether to allow strong-named assemblies to be called by partially trusted code.
        /// </summary>
        public bool AllowPartiallyTrustedCallers
        {
            get { return ReadBooleanAttribute(AllowPartiallyTrustedCallersName); }
            set { _attributes[AllowPartiallyTrustedCallersName] = value.ToString(); }
        }
        
        #endregion Input Parameters

        #region Input/Output Parameters

        /// <summary>
        /// Gets or sets the output file.
        /// </summary>
        /// <value>The output file.</value>
        [Output]
        public string OutputFile
        {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        #endregion Input/Output Parameters

        #region Task Overrides
        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (_attributes.Count == 0)
            {
                Log.LogError("No assembly parameter were set for file \"{0}\".", _outputFile);
                return false;
            }

            Encoding utf8WithSignature = new UTF8Encoding(true);

            using (StreamWriter writer = new StreamWriter(_outputFile, false, utf8WithSignature))
            {
                GenerateFile(writer);
                writer.Flush();
                writer.Close();
                Log.LogMessage("Created AssemblyInfo file \"{0}\".", _outputFile);
            }

            return true;
        }

        #endregion Task Overrides

        #region Private Methods
        private void GenerateFile(TextWriter writer)
        {
            CodeLanguage = (CodeLanguage ?? String.Empty).ToLower();

            // Get the chosen provider and rename the output file's extension
            CodeDomProvider provider = GetProviderAndSetExtension(CodeLanguage, ref _outputFile);

            SetDefaultsForLanguage(CodeLanguage);

            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            CodeNamespace codeNamespace = new CodeNamespace();
            codeCompileUnit.Namespaces.Add(codeNamespace);

            // Add each configured attribute
            foreach (var attribute in _attributes)
            {
                String name = attribute.Key;
                String value = attribute.Value;

                if (Array.Exists(booleanAttributes, name.Equals))
                {
                    // Add the boolean attribute with a strongly typed value
                    AddBooleanAssemblyAttribute(codeCompileUnit, name, value);
                }
                else if (Array.Exists(securityAttributes, name.Equals))
                {
                    // Add the security permission request attribute for the given action
                    AddSecurityPermissionAssemblyAttribute(codeCompileUnit, name, value);
                }
                else if (Array.Exists(markerAttributes, name.Equals))
                {
                    // Add the security permission request attribute for the given action
                    AddMarkerAssemblyAttribute(codeCompileUnit, name, value);
                }
                else
                {
                    // Add the attribute with the text value
                    AddAttributeToCodeDom(codeCompileUnit, name, value);
                }
            }

            // Add an assembly language code attribute to determine the neutral culture
            if (NeutralResourcesLanguage != null)
            {
                AddAssemblyLanguageCodeAttribute(codeCompileUnit);
            }

            // Generate an internally accessible class which has the version information 
            // available as properties
            if (GenerateClass)
            {
                GenerateThisAssemblyClass(codeNamespace);
            }

            // Generate code with the chosen provider
            provider.GenerateCodeFromCompileUnit(codeCompileUnit, writer, new CodeGeneratorOptions());
        }

        private CodeDomProvider GetProviderAndSetExtension(string codeLanguage, ref string outputFile)
        {
            String codeLang;

            if (!_codeLangMapping.TryGetValue(codeLanguage, out codeLang))
            {
                throw new NotSupportedException("The specified code language is not supported: '" +
                                                CodeLanguage +
                                                "'");
            }

            CodeDomProvider provider;

            switch (codeLang)
            {
                case CSharp:
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    outputFile = Path.ChangeExtension(outputFile, ".cs");
                    break;
                case VisualBasic:
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    outputFile = Path.ChangeExtension(outputFile, ".vb");
                    break;
                case FSharp_cl:
                    provider = new FSharp.Compiler.CodeDom.FSharpCodeProvider();
                    outputFile = Path.ChangeExtension(outputFile, ".fs");
                    break;
                case CPP:
                    // We load the CppCodeProvider via reflection since a hard reference would 
                    // require client machines to have the provider installed just to run the task.
                    // This way relieves us of the dependency.
                    try
                    {
                        Assembly cppCodeProvider = Assembly.Load(CppCodeProviderAssembly);
                        provider = cppCodeProvider.CreateInstance(CppCodeProviderType) 
                                        as CodeDomProvider;
                    }
                    catch(FileLoadException fileLoadEx)
                    {
                        String fusionMessage = String.IsNullOrEmpty(fileLoadEx.FusionLog)
                                                   ? "Turn on fusion logging to diagnose the problem further. " +
                                                     "(Check http://blogs.msdn.com/suzcook/archive"+
                                                     "/2003/05/29/57120.aspx for more info)"
                                                   : "Check fusion log: " + fileLoadEx.FusionLog;
                        Log.LogError("The C++/CLI code provider could not be loaded. " +
                                     fileLoadEx.Message +
                                     (fileLoadEx.InnerException?.Message ?? "") +
                                     fusionMessage);
                        provider = null;
                    }
                    catch (FileNotFoundException)
                    {
                        Log.LogError("The C++/CLI code provider wasn't found. "+
                                     "Make sure you have Visual C++ installed.");
                        provider = null;
                    }

                    outputFile = Path.ChangeExtension(outputFile, ".cpp");
                    break;
                default:
                    throw new InvalidOperationException("Shouldn't reach here.");
            }

            return provider;
        }

        private void SetDefaultsForLanguage(string codeLanguage)
        {
            switch (codeLanguage)
            {
                case CSharp:
                    break;
                case VisualBasic:
                    break;
                case CPP:
                    if (!_attributes.ContainsKey(UnmanagedCodeName))
                    {
                        UnmanagedCode = true;
                    }
                    break;
            }
        }

        private static void AddAttributeToCodeDom(CodeCompileUnit codeCompileUnit, string name, object value)
        {
            var valueExpression = new CodePrimitiveExpression(value);
            var codeAttributeDeclaration = new CodeAttributeDeclaration(_attributeNamespaces[name] + "." + name);
            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(valueExpression));
            // add assembly-level argument to code compile unit
            codeCompileUnit.AssemblyCustomAttributes.Add(codeAttributeDeclaration);
        }

        private static void AddMarkerAttributeToCodeDom(CodeCompileUnit codeCompileUnit, string name)
        {
            var codeAttributeDeclaration = new CodeAttributeDeclaration(_attributeNamespaces[name] + "." + name);
            // add assembly-level argument to code compile unit
            codeCompileUnit.AssemblyCustomAttributes.Add(codeAttributeDeclaration);
        }
        
        private static void AddBooleanAssemblyAttribute(CodeCompileUnit codeCompileUnit, String name, String value)
        {
            bool typedValue;

            if (!bool.TryParse(value, out typedValue))
            {
                throw new InvalidOperationException("Boolean attribute " + name + "is not boolean: " +
                                                    value);
            }

            AddAttributeToCodeDom(codeCompileUnit, name, typedValue);
        }

        private static void AddSecurityPermissionAssemblyAttribute(CodeCompileUnit codeCompileUnit, String name, String value)
        {

            var codeAttributeDeclaration = new CodeAttributeDeclaration("System.Security.Permissions.SecurityPermissionAttribute");

            var requestMinimum = new CodeAttributeArgument(
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(typeof(SecurityAction)), "RequestMinimum"));

            codeAttributeDeclaration.Arguments.Add(requestMinimum);

            bool typedValue;
            if (!bool.TryParse(value, out typedValue))
            {
                throw new InvalidOperationException("Boolean security attribute " + name +
                                                    "is not boolean: " + value);
            }

            CodePrimitiveExpression valueExpression = new CodePrimitiveExpression(typedValue);

            codeAttributeDeclaration.Arguments.Add(new CodeAttributeArgument(name, valueExpression));

            // add assembly-level argument to code compile unit
            codeCompileUnit.AssemblyCustomAttributes.Add(codeAttributeDeclaration);
        }

        private static void AddMarkerAssemblyAttribute(CodeCompileUnit codeCompileUnit, String name, String value)
        {
            bool typedValue;

            if (!bool.TryParse(value, out typedValue))
            {
                throw new InvalidOperationException("Boolean attribute " + name + "is not boolean: " +
                                                    value);
            }
            if (typedValue)
            {
                AddMarkerAttributeToCodeDom(codeCompileUnit, name);
            }
        }
        
        private void AddAssemblyLanguageCodeAttribute(CodeCompileUnit codeCompileUnit)
        {
            var codeAttributeDeclaration = new CodeAttributeDeclaration("System.Resources.NeutralResourcesLanguage");
            codeAttributeDeclaration.Arguments.Add(
                new CodeAttributeArgument(
                    new CodePrimitiveExpression(NeutralResourcesLanguage)));
            
            if (! string.IsNullOrEmpty(UltimateResourceFallbackLocation))
                codeAttributeDeclaration.Arguments.Add(
                    new CodeAttributeArgument(
                        new CodeTypeReferenceExpression(UltimateResourceFallbackLocation)));

            codeCompileUnit.AssemblyCustomAttributes.Add(codeAttributeDeclaration);
        }

        private void GenerateThisAssemblyClass(CodeNamespace codeNamespace)
        {
            //Create Class Declaration
            CodeTypeDeclaration thisAssemblyType = new CodeTypeDeclaration("ThisAssembly")
            {
                IsClass = true,
                IsPartial = true,
                TypeAttributes = TypeAttributes.NotPublic |
                                 TypeAttributes.Sealed
            };

            CodeConstructor privateConstructor = new CodeConstructor
            {
                Attributes = MemberAttributes.Private
            };

            thisAssemblyType.Members.Add(privateConstructor);

            foreach (var assemblyAttribute in _attributes)
            {
                String name = assemblyAttribute.Key;
                String value = assemblyAttribute.Value;

                if (Array.Exists(nonGeneratedClassAttributes, name.Equals))
                {
                    continue;
                }

                CodeMemberField field = new CodeMemberField(typeof(string), name)
                {
                    Attributes = MemberAttributes.Assembly |
                                 MemberAttributes.Const,
                    InitExpression = new CodePrimitiveExpression(value)
                };

                thisAssemblyType.Members.Add(field);
            }

            codeNamespace.Types.Add(thisAssemblyType);
        }

        private string ReadAttribute(string key)
        {
            string value;
            _attributes.TryGetValue(key, out value);
            return value;
        }

        private bool ReadBooleanAttribute(string key)
        {
            string value;
            bool result;

            if (!_attributes.TryGetValue(key, out value))
                return false;
            if (!bool.TryParse(value, out result))
                return false;

            return result;
        }

        #endregion Private Methods

    }
}
