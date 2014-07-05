/*
 * AssemblyInfo.cs
 * 
 * Copyright © 2009 Michael Schwarz (http://www.ajaxpro.info).
 * All Rights Reserved.
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without 
 * restriction, including without limitation the rights to use, 
 * copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
/* Developers of Ajax.NET Professional (AjaxPro)
 * MS	Michael Schwarz		info@schwarz-interactive.de
 * TB	Tim Byng			
 * MR	Matthew Raymer
 * 
 * 
 * 
 * 
 * MS	06-04-03	fixed missing http error status code in core.js
 * MS	06-04-04	added AjaxPro.onError, onTimeout, onStateChanged, onLoading to core.js
 * MS	06-04-05	removed Object.prototype.extend from prototype.js and all othere files using this
 *					method
 * MS	06-04-11	fixed core.js bug if http status != 200
 * MS	06-04-25	added ComVisible and CLSCompliant attribute
 * MS	06-06-11	added ReflectionPermission attribute
 * MS	06-07-19	removed ReflectionPermission attribute (why did we add it?)
 * 
 * 
 */
using System;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
#if(JSONLIB)
	#if(NET20)
	[assembly: AssemblyTitle("Ajax.NET Professional JSON library for Microsoft.NET 2.0")]
	#else
	[assembly: AssemblyTitle("Ajax.NET Professional JSON library for Microsoft.NET 1.1")]
	#endif
#else
	#if(NET20)
	[assembly: AssemblyTitle("Ajax.NET Professional for Microsoft.NET 2.0")]
	#else
	[assembly: AssemblyTitle("Ajax.NET Professional for Microsoft.NET 1.1")]
	#endif
#endif

[assembly: AssemblyDescription(".NET Library that provides AJAX related methods to simplify the communication between server and client.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Michael Schwarz")]
[assembly: AssemblyProduct("Ajax.NET Professional")]
[assembly: AssemblyCopyright("2009, Michael Schwarz")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]	
[assembly: System.Security.AllowPartiallyTrustedCallersAttribute()]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.CLSCompliant(true)]

// [assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution=true)]
// [assembly: ReflectionPermission(SecurityAction.RequestMinimum, MemberAccess=true)]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly:				AssemblyVersion("9.2.17.1")]		// do not remove the blanks!!!!

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]

#if(STRONGNAME)
	#if(NET20)
	[assembly: AssemblyKeyFile("michael.schwarz.snk")]
	#else
	[assembly: AssemblyKeyFile("michael.schwarz.snk")]
	#endif
#endif