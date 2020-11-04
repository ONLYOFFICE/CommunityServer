// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

[assembly: CLSCompliant(true)]
#if DEBUG
// Assembly marked as compliant.
[assembly: AssemblyTitle("Html Agility Pack - Debug")] //Description
#else // release
#if TRACE
[assembly: AssemblyTitle("Html Agility Pack - ReleaseTrace")] //Description
#else
[assembly: AssemblyTitle("Html Agility Pack - Release")] //Description
#endif
#endif
[assembly: InternalsVisibleTo("HtmlAgilityPack.Tests, PublicKey=002400000480000094000000060200000024000052534131000400000100010027dc71d8e0b968c7324238e18a4cee4a367f1bf50c9d7a52d91ed46c6a1a584b9142c1d4234c4011d25437c909924079660c434eebe6d2c46412f30520a276e7ca8d8fa7075bb8b9e1c7502ef0e50423b32d469ba750012823fde16989ab42d8428ca5fdd0b06b801788a17239b78e0f75900012a50c5038ab93abbe2ac0d6ee")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ZZZ Projects Inc.")]
[assembly: AssemblyProduct("Html Agility Pack")]
[assembly: AssemblyCopyright("Copyright © ZZZ Projects Inc. 2014 - 2017")]
[assembly: AssemblyTrademark("SQL & .NET Tools")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(true)]
[assembly: Guid("643622ea-d2aa-4572-a2b2-6202b7fcd83f")]
[assembly: AssemblyVersion("1.6.0.1")]
#if !PocketPC
[assembly: AssemblyFileVersion("1.6.0.1")]
[assembly: AssemblyInformationalVersion("1.6.0.1")]
#if !SILVERLIGHT
[assembly: AllowPartiallyTrustedCallers]
#endif
[assembly: AssemblyDelaySign(false)]
#endif
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