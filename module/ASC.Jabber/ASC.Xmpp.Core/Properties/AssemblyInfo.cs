#region using

using System.Reflection;
using System.Runtime.InteropServices;

#endregion

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//

[assembly: AssemblyTitle("ASC.Xmpp.Core")]
[assembly: AssemblyDescription("ASC.Xmpp.Core library")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ascensio System SIA")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Copyright © 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

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

[assembly: AssemblyVersion("1.1.0")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified - the assembly cannot be signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. 
//   (*) If the key file and a key name attributes are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP - that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the file is installed into the CSP and used.
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//

[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

// There was no need yet for a .NET 1.1 Condition. So use MONOSSL here
// because its only used with .NET 1.1
#if CF || MONOSSL
[assembly: AssemblyKeyFile(@"..\..\key.snk")]
#endif
//[assembly: AssemblyKeyName("")]

[assembly: ComVisible(false)]