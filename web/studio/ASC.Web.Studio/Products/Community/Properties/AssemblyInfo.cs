using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ASC.Web.Community.Birthdays;
using ASC.Web.Community.Blogs.Common;
using ASC.Web.Community.Bookmarking;
using ASC.Web.Community.Forum.Common;
using ASC.Web.Community.News;
using ASC.Web.Community.Product;
using ASC.Web.Community.Wiki;
using ASC.Web.Core;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ASC.Web.Community")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ascensio System SIA")]
[assembly: AssemblyProduct("ASC.Web.Community")]
[assembly: AssemblyCopyright("(c) Ascensio System SIA. All rights reserved")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3d5900ae-111a-45be-96b3-d9e4606ca793")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: Product(typeof(CommunityProduct))]
[assembly: Product(typeof(BirthdaysModule))]
[assembly: Product(typeof(BlogsModule))]
[assembly: Product(typeof(BookmarkingModule))]
[assembly: Product(typeof(ForumModule))]
[assembly: Product(typeof(NewsModule))]
[assembly: Product(typeof(WikiModule))]