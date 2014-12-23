using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Jurassic")]
[assembly: AssemblyDescription("Mono port of the Jurassic JS Engine")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Paul Bartrum")]
[assembly: AssemblyProduct("Jurassic")]
[assembly: AssemblyCopyright("Copyright © Paul Bartrum 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("2.1.1.0")]

// The AllowPartiallyTrustedCallersAttribute requires the assembly to be signed with a strong name
// key.
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityRules(SecurityRuleSet.Level1)]

// Unit tests and performance tests need access to internal members.
[assembly: InternalsVisibleTo("Unit Tests")]
//[assembly: InternalsVisibleTo("Performance")]
//[assembly: InternalsVisibleTo("JavaScript")]
//[assembly: InternalsVisibleTo("DebuggerTest")]