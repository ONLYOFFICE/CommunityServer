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
 * 
 * James Higgs (james.higgs@interesource.com)
*/
#endregion



using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks
{
	/// <summary>
	/// Executes code contained within the task.
	/// </summary>
    /// <include file='AdditionalDocumentation.xml' path='docs/task[@name="Script"]/*'/>
    public class Script : Task
	{
		#region Fields
		private static readonly string[] _defaultNamespaces = {
			"System",
			"System.Collections.Generic",
			"System.IO",
			"System.Text",
			"System.Text.RegularExpressions"
		};
		private string _rootClassName = "msbc" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

		#endregion Fields

		#region Input Parameters
		private ITaskItem[] _references;

		/// <summary>
		/// The required references
		/// </summary>
		public ITaskItem[] References
		{
            get { return _references; }
			set { _references = value; }
		}

		private ITaskItem[] _imports;

		/// <summary>
		/// The namespaces to import.
		/// </summary>
		public ITaskItem[] Imports
		{
			get { return _imports; }
			set { _imports = value; }
		}

		string _language = "C#";

		/// <summary>
		/// The language of the script block (defaults to C#).
		/// </summary>
		/// <remarks><para>The supported languages are:</para>
		/// <list type="bullet">
		/// <item><description>Visual Basic.NET (VB, vb, VISUALBASIC)</description></item>
		/// <item><description>C# (C#, c#, CSHARP)</description></item>
		/// <item><description>JavaScript (JS, js, JSCRIPT)</description></item>
		/// <item><description>J# (VJS, vjs, JSHARP)</description></item>
		/// </list> or, proviude the fully-qualified name for a class implementing 
		/// <see cref="System.CodeDom.Compiler.CodeDomProvider" />.</remarks>
		[Required]
		public string Language
		{
			get { return _language; }
			set { _language = value; }
		}

		private string _mainClass = string.Empty;

		/// <summary>
		/// The name of the main class containing the static <c>ScriptMain</c> 
		/// entry point. 
		/// </summary>
		public string MainClass
		{
			get { return _mainClass; }
			set { _mainClass = value; }
		}

		private string _code;

		/// <summary>
		/// The code to compile and execute
		/// </summary>
        /// <remarks>
        /// The code must include a static (Shared in VB) method named ScriptMain.
        /// It cannot accept any parameters. If you define the method return a <see cref="String"/>,
        /// the returned value will be available in the <see cref="ReturnValue"/> output property.
        /// </remarks>
		public string Code
		{
			get { return _code; }
			set { _code = value; }
		}

		#endregion Input Parameters

        string _returnValue;
        
        /// <summary>
        /// The string returned from the custom ScriptMain method.
        /// </summary>
        [Output]
        public string ReturnValue
        {
            get { return _returnValue; }
        }


		#region Task Overrides
		/// <summary>
		/// Executes the task.
		/// </summary>
		/// <returns><see langword="true"/> if the task ran successfully; 
		/// otherwise <see langword="false"/>.</returns>
		public override bool Execute()
		{
			// create compiler info for user-specified language
			CompilerInfo compilerInfo = CreateCompilerInfo(Language);

			CodeDomProvider provider = compilerInfo.Provider;
			CompilerParameters options = new CompilerParameters();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.MainClass = MainClass;

			// add all available assemblies.
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					if (!string.IsNullOrEmpty(asm.Location))
					{
						options.ReferencedAssemblies.Add(asm.Location);
					}
				}
				catch (NotSupportedException)
				{
					// Ignore - this error is sometimes thrown by asm.Location 
					// for certain dynamic assemblies
				}
			}

			// add (and load) assemblies specified by user
			if (References != null)
			{
				foreach (ITaskItem item in References)
				{
					string assemblyFile = item.ItemSpec;
					// load assemblies into current AppDomain to ensure assemblies
					// are available when executing the emitted assembly
                    Assembly loadedAssembly;
                    if (isAssemblyFilePath(assemblyFile))
                    {
                        loadedAssembly = Assembly.LoadFrom(assemblyFile);
                    }
                    else
                    {
                        loadedAssembly = Assembly.Load(assemblyFile);
                    }

					// make assembly available to compiler
					options.ReferencedAssemblies.Add(loadedAssembly.Location);
				}
			}

			// generate the code
			CodeCompileUnit compileUnit = compilerInfo.GenerateCode(_rootClassName,
				Code, _imports);

			StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

			compilerInfo.Provider.GenerateCodeFromCompileUnit(compileUnit, sw, null);
			string code = sw.ToString();

			Log.LogMessage(MessageImportance.Low, "Generated code:\n{0}", code);

			CompilerResults results = provider.CompileAssemblyFromDom(options, compileUnit);

			Assembly compiled = null;
			if (results.Errors.Count > 0)
			{
				string errors = "There were compiler errors:" + Environment.NewLine;
				foreach (CompilerError err in results.Errors)
				{
					errors += err.ToString() + Environment.NewLine;
				}
				errors += code;
				throw new Exception(errors);
			}
			else
			{
				compiled = results.CompiledAssembly;
			}

			string mainClass = _rootClassName;
			if (!string.IsNullOrEmpty(MainClass))
			{
				mainClass += "+" + MainClass;
			}

			Type mainType = compiled.GetType(mainClass);
			if (mainType == null)
			{
				//TODO:
				throw new Exception("Failed - something to do with MainClass");
			}

			MethodInfo entry = mainType.GetMethod("ScriptMain");
			// check for task or function definitions.
			if (entry == null)
			{
				throw new Exception("Could not find an entry point. You must create a static (Shared) method named ScriptMain.");
			}

			if (!entry.IsStatic)
			{
				throw new Exception("Could not find a static (Shared) entry point");
			}

			ParameterInfo[] entryParams = entry.GetParameters();

			if (entryParams.Length != 0)
			{
				throw new Exception("Invalid signatiure for ScriptMain");
			}

			/* 
			 * TODO: if we need to change the sig of the entry point...
			if (entryParams[0].ParameterType.FullName != typeof(Project).FullName)
			{
				throw new Exception("The entry point has the wrong signature");
			}
			 * */

			// invoke Main method
            object invokeResult = null;
            try
            {
                invokeResult = entry.Invoke(null, new object[] { });
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
            _returnValue = invokeResult as string;

			return true;
		}

        private static bool isAssemblyFilePath(string assemblyFile)
        {
            if (assemblyFile.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) ||
                assemblyFile.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

		#endregion Task Overrides

		#region Private Static Methods

		private static CodeDomProvider CreateCodeDomProvider(string typeName, string assemblyName)
		{
			Assembly providerAssembly = Assembly.Load(assemblyName);
			if (providerAssembly == null)
			{
				//TODO:
				throw new Exception("Invalid CodeDomProvider");
			}

			Type providerType = providerAssembly.GetType(typeName, true, true);
			return CreateCodeDomProvider(providerType);
		}

		private static CodeDomProvider CreateCodeDomProvider(string assemblyQualifiedTypeName)
		{
			Type providerType = Type.GetType(assemblyQualifiedTypeName, true, true);
			return CreateCodeDomProvider(providerType);
		}

		private static CodeDomProvider CreateCodeDomProvider(Type providerType)
		{
			object provider = Activator.CreateInstance(providerType);
			if (!(provider is CodeDomProvider))
			{
				//TODO:
				throw new Exception("Invalid CodeDomProvider");
			}
			return (CodeDomProvider)provider;
		}

		#endregion Private Static Methods

		#region Private Methods
		private CompilerInfo CreateCompilerInfo(string language)
		{
			CodeDomProvider provider = null;
			LanguageId languageId;

			switch (language)
			{
				case "vb":
				case "VB":
				case "VISUALBASIC":
					languageId = LanguageId.VisualBasic;
					provider = CreateCodeDomProvider(typeof(Microsoft.VisualBasic.VBCodeProvider));
					break;
				case "c#":
				case "C#":
				case "CSHARP":
					languageId = LanguageId.CSharp;
					provider = CreateCodeDomProvider(typeof(Microsoft.CSharp.CSharpCodeProvider));
					break;
				case "js":
				case "JS":
				case "JSCRIPT":
					languageId = LanguageId.JScript;
					provider = CreateCodeDomProvider(
						"Microsoft.JScript.JScriptCodeProvider",
						"Microsoft.JScript, Culture=neutral");
					break;
				case "vjs":
				case "VJS":
				case "JSHARP":
					languageId = LanguageId.JSharp;
					provider = CreateCodeDomProvider(
						"Microsoft.VJSharp.VJSharpCodeProvider",
						"VJSharpCodeProvider, Culture=neutral");
					break;
				default:
					// if its not one of the above then it must be a fully 
					// qualified provider class name
					languageId = LanguageId.Other;
					provider = CreateCodeDomProvider(language);
					break;
			}

			return new CompilerInfo(languageId, provider);
		}

		#endregion Private Methods

		#region Enums
		internal enum LanguageId : int
		{
			CSharp = 1,
			VisualBasic = 2,
			JScript = 3,
			JSharp = 4,
			Other = 5
		}

		#endregion Enums

		#region Nested Internal Class
		internal class CompilerInfo
		{
			private LanguageId _lang;
			public readonly CodeDomProvider Provider;

			public CompilerInfo(LanguageId languageId, CodeDomProvider provider)
			{
				_lang = languageId;
				Provider = provider;

			}


			public CodeCompileUnit GenerateCode(string typeName, string codeBody,
				ITaskItem[] imports)
			{
				CodeCompileUnit compileUnit = new CodeCompileUnit();

				CodeTypeDeclaration typeDecl = new CodeTypeDeclaration(typeName);
				typeDecl.IsClass = true;
				typeDecl.TypeAttributes = TypeAttributes.Public;

				// pump in the user specified code as a snippet
				CodeSnippetTypeMember literalMember =
					new CodeSnippetTypeMember(codeBody);
				typeDecl.Members.Add(literalMember);

				CodeNamespace nspace = new CodeNamespace();

				//Add default imports
				foreach (string nameSpace in Script._defaultNamespaces)
				{
					nspace.Imports.Add(new CodeNamespaceImport(nameSpace));
				}
				if (imports != null)
				{
					foreach (ITaskItem item in imports)
					{
						string nameSpace = item.ItemSpec;
						nspace.Imports.Add(new CodeNamespaceImport(nameSpace));
					}
				}
				compileUnit.Namespaces.Add(nspace);
				nspace.Types.Add(typeDecl);

				return compileUnit;
			}
		}

		#endregion Nested Internal Class
	}
}
