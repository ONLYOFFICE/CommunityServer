// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

#if DEBUG

[assembly: AssemblyTitle("Html Agility Pack - Debug")] //Description
#else // release
#if TRACE
[assembly: AssemblyTitle("Html Agility Pack - ReleaseTrace")] //Description
#else
[assembly: AssemblyTitle("Html Agility Pack - Release")] //Description
#endif
#endif

[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("Html Agility Pack")]
[assembly:
    AssemblyCopyright(
        "Copyright (C) 2003-2009 Simon Mourier <simon underscore mourier at hotmail dot com> All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("643622ea-d2aa-4572-a2b2-6202b7fcd83f")]
[assembly: AssemblyVersion("1.3.9.2")]
[assembly: AssemblyFileVersion("1.3.9.2")]
[assembly: AssemblyInformationalVersion("1.3.9.2")]
[assembly: AllowPartiallyTrustedCallers]
[assembly: AssemblyDelaySign(false)]

// 
// Welcome to the HTML Agility Pack!
// As you may have noticed, there is no HtmlAgilityPack file provided.
// You need to build one using SN.EXE utility provided with the .NET Framework
//
// The command to use is something like:
//      SN.EXE -k HtmlAgilityPack.snk
//
// Simon.
//

[assembly: AssemblyKeyName("")]