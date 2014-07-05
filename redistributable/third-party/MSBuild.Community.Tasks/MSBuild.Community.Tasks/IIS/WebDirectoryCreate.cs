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
using System.DirectoryServices;

namespace MSBuild.Community.Tasks.IIS
{
	/// <summary>
	/// Creates a new web directory on a local or remote machine with IIS installed.  The default is 
	/// to create the new web directory on the local machine.  The physical path is required to already exist
	/// on the target machine.  If connecting to a remote machine, you can specify the <see cref="WebBase.Username"/>
	/// and <see cref="WebBase.Password"/> for the task to run under.
	/// </summary>
	/// <example>Create a new web directory on the local machine.
	/// <code><![CDATA[
	/// <WebDirectoryCreate VirtualDirectoryName="MyVirDir"
	///     VirtualDirectoryPhysicalPath="C:\Inetpub\MyWebDir" />
	/// ]]></code>
	/// </example>
	public class WebDirectoryCreate : WebBase
	{
		#region Fields

		private string mVirtualDirectoryName;
		private string mVirtualDirectoryPhysicalPath;
		private bool mAccessExecute = false;
		private bool mAccessNoRemoteExecute = false;
		private bool mAccessNoRemoteRead = false;
		private bool mAccessNoRemoteScript = false;
		private bool mAccessNoRemoteWrite = false;
		private bool mAccessRead = true;
		private bool mAccessSource = false;
		private bool mAccessScript = true;
		private bool mAccessSsl = false;
		private bool mAccessSsl128 = false;
		private bool mAccessSslMapCert = false;
		private bool mAccessSslNegotiateCert = false;
		private bool mAccessSslRequireCert = false;
		private bool mAccessWrite = false;
		private bool mAnonymousPasswordSync = true;
		private bool mAppAllowClientDebug = false;
		private bool mAppAllowDebugging = false;
		private bool mAspAllowSessionState = true;
		private bool mAspBufferingOn = true;
		private bool mAspEnableApplicationRestart = true;
		private bool mAspEnableAspHtmlFallback = false;
		private bool mAspEnableChunkedEncoding = true;
		private bool mAspErrorsToNTLog = false;
		private bool mAspEnableParentPaths = true;
		private bool mAspEnableTypelibCache = true;
		private bool mAspExceptionCatchEnable = true;
		private bool mAspLogErrorRequests = true;
		private bool mAspScriptErrorSentToBrowser = true;
		private bool mAspTrackThreadingModel = false;
		private bool mAuthAnonymous = true;
		private bool mAuthBasic = false;
		private bool mAuthNtlm = false;
		private bool mAuthPersistSingleRequest = false;
		private bool mAuthPersistSingleRequestIfProxy = true;
		private bool mAuthPersistSingleRequestAlwaysIfProxy = false;
		private bool mCacheControlNoCache = false;
		private bool mCacheIsapi = true;
		private bool mContentIndexed = true;
		private bool mCpuAppEnabled = true;
		private bool mCpuCgiEnabled = true;
		private bool mCreateCgiWithNewConsole = false;
		private bool mCreateProcessAsUser = true;
		private bool mDirBrowseShowDate = true;
		private bool mDirBrowseShowExtension = true;
		private bool mDirBrowseShowLongDate = true;
		private bool mDirBrowseShowSize = true;
		private bool mDirBrowseShowTime = true;
		private bool mDontLog = false;
		private bool mEnableDefaultDoc = true;
		private bool mEnableDirBrowsing = false;
		private bool mEnableDocFooter = false;
		private bool mEnableReverseDns = false;
		private bool mSsiExecDisable = false;
		private bool mUncAuthenticationPassthrough = false;
		private string mAspScriptErrorMessage = "An error occurred on the server when processing the URL.  Please contact the system administrator.";
		private string mDefaultDoc = "Default.htm, Default.asp, index.htm, iisstart.asp, Default.aspx";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the virtual directory.
		/// </summary>
		/// <value>The name of the virtual directory.</value>
		[Required]
		public string VirtualDirectoryName
		{
			get
			{
				return mVirtualDirectoryName;
			}
			set
			{
				mVirtualDirectoryName = value;
			}
		}

		/// <summary>
		/// Gets or sets the virtual directory physical path.  The physical directory must
		/// exist before this task executes.
		/// </summary>
		/// <value>The virtual directory physical path.</value>
		[Required]
		public string VirtualDirectoryPhysicalPath
		{
			get
			{
				return mVirtualDirectoryPhysicalPath;
			}
			set
			{
				mVirtualDirectoryPhysicalPath = value;
			}
		}

		/// <summary>
		/// Gets or sets a value that indicates if the file
		/// or the contents of the folder may be executed, regardless of file type.
		/// </summary>
		/// <value>A value indicating if AccessExecute is enabled or disabled.</value>
		public bool AccessExecute
		{
			get
			{
				return mAccessExecute;
			}
			set
			{
				mAccessExecute = value;
			}
		}

		/// <summary>
		/// A value of true indicates that remote requests to execute applications
		/// are denied; only requests from the same computer as the IIS server succeed
		/// if the AccessExecute property is set to true. You cannot set 
		/// AccessNoRemoteExecute to false to enable remote requests, and set
		/// <see cref="AccessExecute"/> to false to disable local requests.
		/// </summary>
		/// <value>Value indicating if AccessNoRemoteExecute is enabled or disabled.</value>
		public bool AccessNoRemoteExecute
		{
			get
			{
				return mAccessNoRemoteExecute;
			}
			set
			{
				mAccessNoRemoteExecute = value;
			}
		}

		/// <summary>
		/// A value of true indicates that remote requests to view files are denied; only
		/// requests from the same computer as the IIS server succeed if the <see cref="AccessRead"/>
		/// property is set to true. You cannot set <see cref="AccessNoRemoteRead"/> to false to enable
		/// remote requests, and set <see cref="AccessRead"/> to false to disable local requests.
		/// </summary>
		/// <value>Value indicating if AccessNoRemoteRead is enabled or disabled.</value>
		public bool AccessNoRemoteRead
		{
			get
			{
				return mAccessNoRemoteRead;
			}
			set
			{
				mAccessNoRemoteRead = value;
			}
		}

		/// <summary>
		/// A value of true indicates that remote requests to view dynamic content are denied; only
		/// requests from the same computer as the IIS server succeed if the <see cref="AccessScript"/> property
		/// is set to true. You cannot set AccessNoRemoteScript to false to enable remote requests, 
		/// and set <see cref="AccessScript"/> to false to disable local requests.
		/// </summary>
		/// <value>Value indicating if AccessNoRemoteScript is enabled or disabled.</value>
		public bool AccessNoRemoteScript
		{
			get
			{
				return mAccessNoRemoteScript;
			}
			set
			{
				mAccessNoRemoteScript = value;
			}
		}

		/// <summary>
		/// A value of true indicates that remote requests to create or change files are denied; only
		/// requests from the same computer as the IIS server succeed if the <see cref="AccessWrite"/> property
		/// is set to true. You cannot set AccessNoRemoteWrite to false to enable remote requests, 
		/// and set <see cref="AccessWrite"/> to false to disable local requests.
		/// </summary>
		/// <value>Value indicating if AccessNoRemoteWrite is enabled or disabled.</value>
		public bool AccessNoRemoteWrite
		{
			get
			{
				return mAccessNoRemoteWrite;
			}
			set
			{
				mAccessNoRemoteWrite = value;
			}
		}

		/// <summary>
		/// A value of true indicates that the file or the contents of the folder may be read
		/// through Microsoft Internet Explorer.
		/// </summary>
		/// <value>Value indicating if AccessRead is enabled or disabled.</value>
		public bool AccessRead
		{
			get
			{
				return mAccessRead;
			}
			set
			{
				mAccessRead = value;
			}
		}

		/// <summary>
		/// A value of true indicates that users are allowed to access source code if either
		/// Read or Write permissions are set. Source code includes scripts in Microsoft ® Active
		/// Server Pages (ASP) applications.
		/// </summary>
		/// <value>Value indicating if AccessSource is enabled or disabled.</value>
		public bool AccessSource
		{
			get
			{
				return mAccessSource;
			}
			set
			{
				mAccessSource = value;
			}
		}

		/// <summary>
		/// A value of true indicates that the file or the contents of the folder may be executed
		/// if they are script files or static content. A value of false only allows static files, 
		/// such as HTML files, to be served.
		/// </summary>
		/// <value>Value indicating if AccessScript is enabled or disabled.</value>
		public bool AccessScript
		{
			get
			{
				return mAccessScript;
			}
			set
			{
				mAccessScript = value;
			}
		}

		/// <summary>
		/// A value of true indicates that file access requires SSL file permission processing, with
		/// or without a client certificate.
		/// </summary>
		/// <value>Value indicating if AccessSsl is enabled or disabled.</value>
		public bool AccessSsl
		{
			get
			{
				return mAccessSsl;
			}
			set
			{
				mAccessSsl = value;
			}
		}

		/// <summary>
		/// A value of true indicates that file access requires SSL file permission processing
		/// with a minimum key size of 128 bits, with or without a client certificate.
		/// </summary>
		/// <value>Value indicating if AccessSsl128 is enabled or disabled.</value>
		public bool AccessSsl128
		{
			get
			{
				return mAccessSsl128;
			}
			set
			{
				mAccessSsl128 = value;
			}
		}

		/// <summary>
		/// A value of true indicates that SSL file permission processing maps a client certificate
		/// to a Microsoft Windows ® operating system user-account. The <see cref="AccessSslNegotiateCert"/> property
		/// must also be set to true for the mapping to occur.
		/// </summary>
		/// <value>Value indicating if AccessSslMapCert is enabled or disabled.</value>
		public bool AccessSslMapCert
		{
			get
			{
				return mAccessSslMapCert;
			}
			set
			{
				mAccessSslMapCert = value;
			}
		}

		/// <summary>
		/// A value of true indicates that SSL file access processing requests a certificate from
		/// the client. A value of false indicates that access continues if the client does not have
		/// a certificate. Some versions of Internet Explorer will close the connection if the server
		/// requests a certificate and a certificate is not available (even if <see cref="AccessSslRequireCert"/> 
		/// is also set to true).
		/// </summary>
		/// <value>Value indicating if AccessSslNegotiateCert is enabled or disabled.</value>
		public bool AccessSslNegotiateCert
		{
			get
			{
				return mAccessSslNegotiateCert;
			}
			set
			{
				mAccessSslNegotiateCert = value;
			}
		}

		/// <summary>
		/// A value of true indicates that SSL file access processing requests a certificate from the
		/// client. If the client provides no certificate, the connection is closed. <see cref="AccessSslNegotiateCert"/> 
		/// must also be set to true when using AccessSSLRequireCert.
		/// </summary>
		/// <value>Value indicating if AccessSslRequireCert is enabled or disabled.</value>
		public bool AccessSslRequireCert
		{
			get
			{
				return mAccessSslRequireCert;
			}
			set
			{
				mAccessSslRequireCert = value;
			}
		}

		/// <summary>
		/// A value of true indicates that users are allowed to upload files and their associated
		/// properties to the enabled directory on your server or to change content in a Write-enabled
		/// file. Write can be implemented only with a browser that supports the PUT feature of
		/// the HTTP 1.1 protocol standard.
		/// </summary>
		/// <value>Value indicating if AccessWrite is enabled or disabled.</value>
		public bool AccessWrite
		{
			get
			{
				return mAccessWrite;
			}
			set
			{
				mAccessWrite = value;
			}
		}

		/// <summary>
		/// The AnonymousPasswordSync property indicates whether IIS should handle the user password
		/// for anonymous users attempting to access resources.
		/// </summary>
		/// <value>Value indicating if AnonymousPasswordSync is enabled or disabled.</value>
		public bool AnonymousPasswordSync
		{
			get
			{
				return mAnonymousPasswordSync;
			}
			set
			{
				mAnonymousPasswordSync = value;
			}
		}

		/// <summary>
		/// The AppAllowClientDebug property specifies whether ASP client-side debugging
		/// is enabled. This property is independent of <see cref="AppAllowDebugging"/>, which
		/// applies to server-side debugging.
		/// </summary>
		/// <value>Value indicating if AppAllowClientDebug is enabled or disabled.</value>
		public bool AppAllowClientDebug
		{
			get
			{
				return mAppAllowClientDebug;
			}
			set
			{
				mAppAllowClientDebug = value;
			}
		}

		/// <summary>
		/// The AppAllowDebugging property specifies whether ASP debugging is enabled on
		/// the server. This property is independent of the <see cref="AppAllowClientDebug"/> property, 
		/// which applies to client-side debugging.
		/// </summary>
		/// <value>Value indicating if AppAllowDebugging is enabled or disabled.</value>
		public bool AppAllowDebugging
		{
			get
			{
				return mAppAllowDebugging;
			}
			set
			{
				mAppAllowDebugging = value;
			}
		}

		/// <summary>
		/// The AspAllowSessionState property enables session state persistence for the ASP application.
		/// </summary>
		/// <value>Value indicating if the AspAllowSessionState is enabled or disabled.</value>
		public bool AspAllowSessionState
		{
			get
			{
				return mAspAllowSessionState;
			}
			set
			{
				mAspAllowSessionState = value;
			}
		}

		/// <summary>
		/// The AspBufferingOn property specifies whether output from an ASP application will be buffered.
		/// </summary>
		/// <value>Value indicating if the AspBufferingOn is enabled or disabled.</value>
		public bool AspBufferingOn
		{
			get
			{
				return mAspBufferingOn;
			}
			set
			{
				mAspBufferingOn = value;
			}
		}

		/// <summary>
		/// The AspEnableApplicationRestart determines whether an ASP application can be
		/// automatically restarted. When changes are made to Global.asa or metabase properties
		/// that affect an application, the application will not restart unless the
		/// AspEnableApplicationRestart property is set to true.
		/// </summary>
		/// <value>Value indicating if AspEnableApplicationRestart is enabled or disabled.</value>
		public bool AspEnableApplicationRestart
		{
			get
			{
				return mAspEnableApplicationRestart;
			}
			set
			{
				mAspEnableApplicationRestart = value;
			}
		}

		/// <summary>
		/// The AspEnableAspHtmlFallback property controls the behavior of ASP when a
		/// new request is to be rejected due to a full request queue.
		/// </summary>
		/// <value>Value indicating if AspEnableAspHtmlFallback is enabled or disabled.</value>
		public bool AspEnableAspHtmlFallback
		{
			get
			{
				return mAspEnableAspHtmlFallback;
			}
			set
			{
				mAspEnableAspHtmlFallback = value;
			}
		}

		/// <summary>
		/// The AspEnableChunkedEncoding property specifies whether HTTP 1.1 chunked
		/// transfer encoding is enabled for the World Wide Web Publishing Service (WWW service).
		/// </summary>
		/// <value>Value indicating if AspEnableChunkedEncoding is enabled or disabled.</value>
		public bool AspEnableChunkedEncoding
		{
			get
			{
				return mAspEnableChunkedEncoding;
			}
			set
			{
				mAspEnableChunkedEncoding = value;
			}
		}

		/// <summary>
		/// The AspErrorsToNTLog property specifies which ASP errors are written to the
		/// Windows event log. ASP errors are written to the client browser and to the IIS
		/// log files by default.
		/// </summary>
		/// <value>Value indicating if AspErrorsToNTLog is enabled or disabled.</value>
		public bool AspErrorsToNTLog
		{
			get
			{
				return mAspErrorsToNTLog;
			}
			set
			{
				mAspErrorsToNTLog = value;
			}
		}

		/// <summary>
		/// The AspEnableParentPaths property specifies whether an ASP page allows paths
		/// relative to the current directory (using the ..\ notation) or above the current directory.
		/// </summary>
		/// <value>Value indicating if AspEnableParentPaths is enabled or disabled.</value>
		public bool AspEnableParentPaths
		{
			get
			{
				return mAspEnableParentPaths;
			}
			set
			{
				mAspEnableParentPaths = value;
			}
		}

		/// <summary>
		/// The AspEnableTypelibCache property specifies whether type libraries are cached
		/// on the server. The World Wide Web Publishing Service (WWW service) setting for
		/// this property is applicable to all in-process and pooled out-of-process application
		/// nodes, at all levels. Metabase settings at the Web server level or lower are ignored
		/// for in-process and pooled out-of-process applications. However, settings at the Web
		/// server level or lower are used if that node is an isolated out-of-process application.
		/// </summary>
		/// <value>Value indicating if AspEnableTypelibCache is enabled or disabled.</value>
		public bool AspEnableTypelibCache
		{
			get
			{
				return mAspEnableTypelibCache;
			}
			set
			{
				mAspEnableTypelibCache = value;
			}
		}

		/// <summary>
		/// The AspExceptionCatchEnable property specifies whether ASP pages trap exceptions
		/// thrown by components.
		/// </summary>
		/// <value>Value indicating if AspExceptionCatchEnable is enabled or disabled.</value>
		public bool AspExceptionCatchEnable
		{
			get
			{
				return mAspExceptionCatchEnable;
			}
			set
			{
				mAspExceptionCatchEnable = value;
			}
		}

		/// <summary>
		/// The AspLogErrorRequests property controls whether the Web server writes ASP errors
		/// to the application section of the Windows event log. ASP errors are written to the
		/// client browser and to the IIS log files by default.
		/// </summary>
		/// <value>Value indicating if AspLogErrorRequests is enabled or disabled.</value>
		public bool AspLogErrorRequests
		{
			get
			{
				return mAspLogErrorRequests;
			}
			set
			{
				mAspLogErrorRequests = value;
			}
		}

		/// <summary>
		/// The AspScriptErrorSentToBrowser property specifies whether the Web server writes
		/// debugging specifics (file name, error, line number, description) to the client
		/// browser, in addition to logging them to the Windows Event Log. The <see cref="AspScriptErrorMessage"/> 
		/// property provides the error message to be sent if this property is set to false.
		/// </summary>
		/// <value>Value indicating if AspScriptErrorSentToBrowser is enabled or disabled.</value>
		public bool AspScriptErrorSentToBrowser
		{
			get
			{
				return mAspScriptErrorSentToBrowser;
			}
			set
			{
				mAspScriptErrorSentToBrowser = value;
			}
		}

		/// <summary>
		/// The AspTrackThreadingModel property specifies whether IIS checks the threading model
		/// of any components (COM objects) that your application creates. The preferred setting
		/// of this metabase property is false.
		/// </summary>
		/// <value>Value indicating if AspTrackThreadingModel is enabled or disabled.</value>
		public bool AspTrackThreadingModel
		{
			get
			{
				return mAspTrackThreadingModel;
			}
			set
			{
				mAspTrackThreadingModel = value;
			}
		}

		/// <summary>
		/// Specifies Anonymous authentication as one of the possible Windows authentication
		/// schemes returned to clients as being available.
		/// </summary>
		/// <value>Value indicating if AuthAnonymous is enabled or disabled.</value>
		public bool AuthAnonymous
		{
			get
			{
				return mAuthAnonymous;
			}
			set
			{
				mAuthAnonymous = value;
			}
		}

		/// <summary>
		/// Specifies Basic authentication as one of the possible Windows authentication
		/// schemes returned to clients as being available.
		/// </summary>
		/// <value>Value indicating if AuthBasic is enabled or disabled.</value>
		public bool AuthBasic
		{
			get
			{
				return mAuthBasic;
			}
			set
			{
				mAuthBasic = value;
			}
		}

		/// <summary>
		/// Specifies Integrated Windows authentication (also known as Challenge/Response or
		/// NTLM authentication) as one of the possible Windows authentication schemes
		/// returned to clients as being available.
		/// </summary>
		/// <value>Value indicating if AuthNtlm is enabled or disabled.</value>
		public bool AuthNtlm
		{
			get
			{
				return mAuthNtlm;
			}
			set
			{
				mAuthNtlm = value;
			}
		}

		/// <summary>
		/// Setting this flag to true specifies that authentication persists only for a single
		/// request on a connection. IIS resets the authentication at the end of each request, and
		/// forces re-authentication on the next request of the session.
		/// </summary>
		/// <value>Value indicating if AuthPersistSingleRequest is enabled or disabled.</value>
		public bool AuthPersistSingleRequest
		{
			get
			{
				return mAuthPersistSingleRequest;
			}
			set
			{
				mAuthPersistSingleRequest = value;
			}
		}

		/// <summary>
		/// Setting this flag to true specifies authentication will persist only across single
		/// requests on a connection if the connection is by proxy. IIS will reset the authentication
		/// at the end of the request if the current authenticated request is by proxy and it is
		/// not the special case where IIS is running MSPROXY.
		/// </summary>
		/// <value>Value indicating if AuthPersistSingleRequestIfProxy is enabled or disabled.</value>
		public bool AuthPersistSingleRequestIfProxy
		{
			get
			{
				return mAuthPersistSingleRequestIfProxy;
			}
			set
			{
				mAuthPersistSingleRequestIfProxy = value;
			}
		}

		/// <summary>
		/// Setting this flag to true specifies that authentication is valid for a single request if
		/// by proxy. IIS will reset the authentication at the end of the request and force
		/// re-authentication on the next request if the current authenticated request is by
		/// proxy of any type.
		/// </summary>
		/// <value>Value indicating if AuthPersistSingleRequestAlwaysIfProxy is enabled or disabled.</value>
		public bool AuthPersistSingleRequestAlwaysIfProxy
		{
			get
			{
				return mAuthPersistSingleRequestAlwaysIfProxy;
			}
			set
			{
				mAuthPersistSingleRequestAlwaysIfProxy = value;
			}
		}

		/// <summary>
		/// The CacheControlNoCache property specifies the HTTP 1.1 directive to prevent caching of content.
		/// </summary>
		/// <value>Value indicating if CacheControlNoCache is enabled or disabled.</value>
		public bool CacheControlNoCache
		{
			get
			{
				return mCacheControlNoCache;
			}
			set
			{
				mCacheControlNoCache = value;
			}
		}

		/// <summary>
		/// The CacheISAPI property indicates whether ISAPI extensions are cached in memory after first use.
		/// </summary>
		/// <value>Value indicating if CacheIsapi is enabled or disabled.</value>
		public bool CacheIsapi
		{
			get
			{
				return mCacheIsapi;
			}
			set
			{
				mCacheIsapi = value;
			}
		}

		/// <summary>
		/// The ContentIndexed property specifies whether the installed content indexer should
		/// index content under this directory tree.
		/// </summary>
		/// <value>Value indicating if ContentIndexed is enabled or disabled.</value>
		public bool ContentIndexed
		{
			get
			{
				return mContentIndexed;
			}
			set
			{
				mContentIndexed = value;
			}
		}

		/// <summary>
		/// This property specifies whether process accounting and throttling should be performed
		/// for ISAPI extensions and ASP applications. To perform process accounting on CGI
		/// applications, use the property <see cref="CpuCgiEnabled"/>.
		/// </summary>
		/// <value>Value indicating if CpuAppEnabled is enabled or disabled.</value>
		public bool CpuAppEnabled
		{
			get
			{
				return mCpuAppEnabled;
			}
			set
			{
				mCpuAppEnabled = value;
			}
		}

		/// <summary>
		/// This property indicates whether IIS should perform process accounting for CGI
		/// applications. To effectively throttle CGI applications, use the CgiTimeout
		/// property. To use process accounting for ISAPI and ASP applications, use <see cref="CpuAppEnabled"/>.
		/// </summary>
		/// <value>Value indicating if CpuCgiEnabled is enabled or disabled.</value>
		public bool CpuCgiEnabled
		{
			get
			{
				return mCpuCgiEnabled;
			}
			set
			{
				mCpuCgiEnabled = value;
			}
		}

		/// <summary>
		/// The CreateCGIWithNewConsole property indicates whether a CGI application runs in its own console.
		/// </summary>
		/// <value>Value indicating if CreateCgiWithNewConsole is enabled or disabled.</value>
		public bool CreateCgiWithNewConsole
		{
			get
			{
				return mCreateCgiWithNewConsole;
			}
			set
			{
				mCreateCgiWithNewConsole = value;
			}
		}

		/// <summary>
		/// The CreateProcessAsUser property specifies whether a CGI process is created in the system context or in the context of the requesting user.
		/// </summary>
		/// <value>Value indicating if CreateProcessAsUser is enabled or disabled.</value>
		public bool CreateProcessAsUser
		{
			get
			{
				return mCreateProcessAsUser;
			}
			set
			{
				mCreateProcessAsUser = value;
			}
		}

		/// <summary>
		/// When set to true, date information is displayed when browsing directories.
		/// </summary>
		/// <value>Value indicating if DirBrowseShowDate is enabled or disabled.</value>
		public bool DirBrowseShowDate
		{
			get
			{
				return mDirBrowseShowDate;
			}
			set
			{
				mDirBrowseShowDate = value;
			}
		}

		/// <summary>
		/// When set to true, file name extensions are displayed when browsing directories.
		/// </summary>
		/// <value>Value indicating if DirBrowseShowExtension is enabled or disabled.</value>
		public bool DirBrowseShowExtension
		{
			get
			{
				return mDirBrowseShowExtension;
			}
			set
			{
				mDirBrowseShowExtension = value;
			}
		}

		/// <summary>
		/// When set to true, date information is displayed in extended format when displaying directories.
		/// </summary>
		/// <value>Value indicating if DirBrowseShowLongDate is enabled or disabled.</value>
		public bool DirBrowseShowLongDate
		{
			get
			{
				return mDirBrowseShowLongDate;
			}
			set
			{
				mDirBrowseShowLongDate = value;
			}
		}

		/// <summary>
		/// When set to true, file size information is displayed when browsing directories.
		/// </summary>
		/// <value>Value indicating if DirBrowseShowSize is enabled or disabled.</value>
		public bool DirBrowseShowSize
		{
			get
			{
				return mDirBrowseShowSize;
			}
			set
			{
				mDirBrowseShowSize = value;
			}
		}

		/// <summary>
		/// When set to true, file time information is displayed when displaying directories.
		/// </summary>
		/// <value>Value indicating if DirBrowseShowTime is enabled or disabled.</value>
		public bool DirBrowseShowTime
		{
			get
			{
				return mDirBrowseShowTime;
			}
			set
			{
				mDirBrowseShowTime = value;
			}
		}

		/// <summary>
		/// The DontLog property specifies whether client requests are written to the IIS log files.
		/// </summary>
		/// <value>Value indicating if DontLog is enabled or disabled.</value>
		public bool DontLog
		{
			get
			{
				return mDontLog;
			}
			set
			{
				mDontLog = value;
			}
		}

		/// <summary>
		/// When set to true, the default document (specified by the <see cref="DefaultDoc"/> property) for
		/// a directory is loaded when the directory is browsed.
		/// </summary>
		/// <value>Value indicating if EnableDefaultDoc is enabled or disabled.</value>
		public bool EnableDefaultDoc
		{
			get
			{
				return mEnableDefaultDoc;
			}
			set
			{
				mEnableDefaultDoc = value;
			}
		}

		/// <summary>
		/// When set to true, directory browsing is enabled.
		/// </summary>
		/// <value>Value indicating if EnableDirBrowsing is enabled or disabled.</value>
		public bool EnableDirBrowsing
		{
			get
			{
				return mEnableDirBrowsing;
			}
			set
			{
				mEnableDirBrowsing = value;
			}
		}

		/// <summary>
		/// The EnableDocFooter property enables or disables custom footers specified by
		/// the DefaultDocFooter property.
		/// </summary>
		/// <value>Value indicating if EnableDocFooter is enabled or disabled.</value>
		public bool EnableDocFooter
		{
			get
			{
				return mEnableDocFooter;
			}
			set
			{
				mEnableDocFooter = value;
			}
		}

		/// <summary>
		/// The EnableReverseDns property enables or disables reverse Domain Name Server (DNS) lookups
		/// for the World Wide Web Publishing Service (WWW service). Reverse lookups involve looking
		/// up the domain name when the IP address is known. Reverse DNS lookups can use significant
		/// resources and time.
		/// </summary>
		/// <value>Value indicating if EnableReverseDns is enabled or disabled.</value>
		public bool EnableReverseDns
		{
			get
			{
				return mEnableReverseDns;
			}
			set
			{
				mEnableReverseDns = value;
			}
		}

		/// <summary>
		/// The SSIExecDisable property specifies whether server-side include (SSI) #exec directives
		/// are disabled under this path.
		/// </summary>
		/// <value>Value indicating if SsiExecDisable is enabled or disabled.</value>
		public bool SsiExecDisable
		{
			get
			{
				return mSsiExecDisable;
			}
			set
			{
				mSsiExecDisable = value;
			}
		}

		/// <summary>
		/// The UNCAuthenticationPassthrough property enables user authentication passthrough
		/// for Universal Naming Convention (UNC) virtual root access (for authentication schemes
		/// that support delegation).
		/// </summary>
		/// <value>Value indicating if UncAuthenticationPassthrough is enabled or disabled.</value>
		public bool UncAuthenticationPassthrough
		{
			get
			{
				return mUncAuthenticationPassthrough;
			}
			set
			{
				mUncAuthenticationPassthrough = value;
			}
		}

		/// <summary>
		/// The AspScriptErrorMessage property specifies the error message to send to the browser
		/// if specific debugging errors are not sent to the client (if <see cref="AspScriptErrorSentToBrowser"/> 
		/// is set to false).
		/// </summary>
		/// <value>Value indicating if AspScriptErrorMessage is enabled or disabled.</value>
		public string AspScriptErrorMessage
		{
			get
			{
				return mAspScriptErrorMessage;
			}
			set
			{
				mAspScriptErrorMessage = value;
			}
		}

		/// <summary>
		/// The DefaultDoc contains one or more file names of default documents that will be returned
		/// to the client if no file name is included in the client's request. The default document
		/// will be returned if the <see cref="EnableDefaultDoc"/> flag of the DirBrowseFlags property
		/// is set to true for the directory. This property can contain a list of default document
		/// file names separated by a comma and a space, for example Default.htm, Default.asp.
		/// </summary>
		/// <value>Listing of the default documents for the web application.</value>
		public string DefaultDoc
		{
			get
			{
				return mDefaultDoc;
			}
			set
			{
				mDefaultDoc = value;
			}
		}

		#endregion

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// True if the task successfully executed; otherwise, false.
		/// </returns>
		public override bool Execute()
        {
			mIISVersion = GetIISVersion();

			if (CreateVirtualDirectory())
			{
				return true;
			}
			else
			{
				return false;
			}
        }

		#region Private Methods

		private bool CreateVirtualDirectory()
		{
            const string WebDirectoryClassName = "IIsWebDirectory";
            const string VirtualDirectoryClassName = "IIsWebVirtualDir";
            bool bSuccess = false;

			try
			{
				Log.LogMessage(MessageImportance.Normal, "Creating virtual directory named {0} on {1}:", VirtualDirectoryName, ServerName);
				VerifyIISRoot();
				
				DirectoryEntry siteRoot = new DirectoryEntry(IISServerPath);
				siteRoot.RefreshCache();
				DirectoryEntry newVirDir = null;

                try
                {
                    // If a Web Directory with this name already exists, delete it.
                    DirectoryEntry existingWebDir = siteRoot.Children.Find(VirtualDirectoryName, WebDirectoryClassName);
                    if (existingWebDir != null)
                    {
                        existingWebDir.DeleteTree();
                    }
                }
                catch (System.IO.DirectoryNotFoundException) { /* Web Directory does not exist - that's good */}
                try
                {
                    // If a Virtual Directory with this name already exists, use it. Otherwise, add it.
                    DirectoryEntry existingVirDir = siteRoot.Children.Find(VirtualDirectoryName, VirtualDirectoryClassName);
                    newVirDir = existingVirDir;
                }
                catch (System.IO.DirectoryNotFoundException) { }
                if (newVirDir == null)
                {
                    // Create the virtual directory
                    newVirDir = siteRoot.Children.Add(VirtualDirectoryName, VirtualDirectoryClassName);
                    newVirDir.CommitChanges();
                }

				// Set Required Properties
				newVirDir.Properties["Path"].Value = VirtualDirectoryPhysicalPath;
				newVirDir.Properties["AppFriendlyName"].Value = VirtualDirectoryName;
				newVirDir.Properties["AppRoot"].Value = string.Format("{0}/{1}", IISApplicationPath, VirtualDirectoryName);
				newVirDir.Properties["AppIsolated"][0] = 2;

				// Set Optional Properties
				if (mIISVersion == IISVersion.Four)
				{
					newVirDir.Properties["CpuAppEnabled"][0] = CpuAppEnabled;
					newVirDir.Properties["CpuCgiEnabled"][0] = CpuCgiEnabled;
				}

				if (mIISVersion == IISVersion.Five)
				{
					newVirDir.Properties["AspTrackThreadingModel"][0] = AspTrackThreadingModel;
					newVirDir.Properties["AuthPersistSingleRequestIfProxy"][0] = AuthPersistSingleRequestIfProxy;
					newVirDir.Properties["AuthPersistSingleRequestAlwaysIfProxy"][0] = AuthPersistSingleRequestAlwaysIfProxy;
					newVirDir.Properties["UNCAuthenticationPassthrough"][0] = UncAuthenticationPassthrough;
				}

				newVirDir.Properties["AccessExecute"][0] = AccessExecute;
				newVirDir.Properties["AccessNoRemoteExecute"][0] = AccessNoRemoteExecute;
				newVirDir.Properties["AccessNoRemoteRead"][0] = AccessNoRemoteRead;
				newVirDir.Properties["AccessNoRemoteScript"][0] = AccessNoRemoteScript;
				newVirDir.Properties["AccessNoRemoteWrite"][0] = AccessNoRemoteWrite;
				newVirDir.Properties["AccessRead"][0] = AccessRead;
				newVirDir.Properties["AccessSource"][0] = AccessSource;
				newVirDir.Properties["AccessScript"][0] = AccessScript;
				newVirDir.Properties["AccessSSL"][0] = AccessSsl;
				newVirDir.Properties["AccessSSL128"][0] = AccessSsl128;
				newVirDir.Properties["AccessSSLMapCert"][0] = AccessSslMapCert;
				newVirDir.Properties["AccessSSLNegotiateCert"][0] = AccessSslNegotiateCert;
				newVirDir.Properties["AccessSSLRequireCert"][0] = AccessSslRequireCert;
				newVirDir.Properties["AccessWrite"][0] = AccessWrite;
				newVirDir.Properties["AnonymousPasswordSync"][0] = AnonymousPasswordSync;
				newVirDir.Properties["AppAllowClientDebug"][0] = AppAllowClientDebug;
				newVirDir.Properties["AppAllowDebugging"][0] = AppAllowDebugging;
				newVirDir.Properties["AspBufferingOn"][0] = AspBufferingOn;
				newVirDir.Properties["AspEnableApplicationRestart"][0] = AspEnableApplicationRestart;
				newVirDir.Properties["AspEnableAspHtmlFallback"][0] = AspEnableAspHtmlFallback;
				newVirDir.Properties["AspEnableChunkedEncoding"][0] = AspEnableChunkedEncoding;
				newVirDir.Properties["AspErrorsToNTLog"][0] = AspErrorsToNTLog;
				newVirDir.Properties["AspEnableParentPaths"][0] = AspEnableParentPaths;
				newVirDir.Properties["AspEnableTypelibCache"][0] = AspEnableTypelibCache;
				newVirDir.Properties["AspExceptionCatchEnable"][0] = AspExceptionCatchEnable;
				newVirDir.Properties["AspLogErrorRequests"][0] = AspLogErrorRequests;
				newVirDir.Properties["AspScriptErrorSentToBrowser"][0] = AspScriptErrorSentToBrowser;
				newVirDir.Properties["AuthAnonymous"][0] = AuthAnonymous;
				newVirDir.Properties["AuthBasic"][0] = AuthBasic;
				newVirDir.Properties["AuthNTLM"][0] = AuthNtlm;
				newVirDir.Properties["AuthPersistSingleRequest"][0] = AuthPersistSingleRequest;
				newVirDir.Properties["CacheControlNoCache"][0] = CacheControlNoCache;
				newVirDir.Properties["CacheISAPI"][0] = CacheIsapi;
				newVirDir.Properties["ContentIndexed"][0] = ContentIndexed;
				newVirDir.Properties["CreateCGIWithNewConsole"][0] = CreateCgiWithNewConsole;
				newVirDir.Properties["CreateProcessAsUser"][0] = CreateProcessAsUser;
				newVirDir.Properties["DirBrowseShowDate"][0] = DirBrowseShowDate;
				newVirDir.Properties["DirBrowseShowExtension"][0] = DirBrowseShowExtension;
				newVirDir.Properties["DirBrowseShowLongDate"][0] = DirBrowseShowLongDate;
				newVirDir.Properties["DirBrowseShowSize"][0] = DirBrowseShowSize;
				newVirDir.Properties["DirBrowseShowTime"][0] = DirBrowseShowTime;
				newVirDir.Properties["DontLog"][0] = DontLog;
				newVirDir.Properties["EnableDefaultDoc"][0] = EnableDefaultDoc;
				newVirDir.Properties["EnableDirBrowsing"][0] = EnableDirBrowsing;
				newVirDir.Properties["EnableDocFooter"][0] = EnableDocFooter;
				newVirDir.Properties["EnableReverseDns"][0] = EnableReverseDns;
				newVirDir.Properties["SSIExecDisable"][0] = SsiExecDisable;
				newVirDir.Properties["AspScriptErrorMessage"][0] = AspScriptErrorMessage;
				newVirDir.Properties["DefaultDoc"][0] = DefaultDoc;

				// Commit the changes
				newVirDir.CommitChanges();
				siteRoot.CommitChanges();
				newVirDir.Close();
				siteRoot.Close();

				bSuccess = true;
				Log.LogMessage(MessageImportance.Normal, "Done.");
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
			}

			return bSuccess;
		}

		#endregion Private Methods
	}
}
