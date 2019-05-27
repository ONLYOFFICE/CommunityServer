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
using System.Net;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using System.Diagnostics;
using System.Threading;

// $Id: FtpUpload.cs 127 2006-03-15 02:52:23Z pwelter34 $

namespace MSBuild.Community.Tasks {



	/// <summary>
	/// Uploads a group of files using File Transfer Protocol (FTP).
	/// </summary>
	/// <remarks>
	/// Set either LocalFiles or LocalFile but not both.
	/// </remarks>
	/// <example>Upload a file.
	/// <code><![CDATA[
	/// <FtpUpload 
	///     LocalFile="MSBuild.Community.Tasks.zip" 
	///     RemoteUri="ftp://localhost/" />
	/// ]]></code>
	/// 
	/// Upload all the files in an ItemGroup:
	/// <code><![CDATA[
	/// <FtpUpload
	///     Username="username"
	///     Password="password"
	///     UsePassive="true"
	///     RemoteUri="ftp://webserver.com/httpdocs/"
	///     LocalFiles="@(FilesToUpload)"
	///     RemoteFiles="@(FilesToUpload->'%(RecursiveDir)%(Filename)%(Extension)')" />
	/// ]]></code>
	/// </example>
	public class FtpUpload : Task, IFtpWebRequestCreator {

		/// <summary>
		/// Initializes a new instance of the <see cref="T:FtpUpload"/> class.
		/// </summary>
		public FtpUpload() {
			_username = "anonymous";
			_password = string.Empty;
            _timeout = 7000;
            _keepAlive = false;
		}

		/// <summary>
		/// Initializes a new instance that will inject the specified dependency.
		/// </summary>
		/// <param name="requestCreator"></param>
		public FtpUpload(IFtpWebRequestCreator requestCreator) : this() {
			_requestCreator = requestCreator;
		}

		private IFtpWebRequestCreator _requestCreator;
		private ITaskItem[] _localFiles;

		/// <summary>
		/// Gets or sets the single file to upload.  Use
		/// this or LocalFiles, but not both.
		/// </summary>
		public string LocalFile {
			get {
				if (LocalFiles == null) {
					return null;
				}
				switch (LocalFiles.Length) {
					case 0: return null;
					case 1: return LocalFiles[0].ItemSpec;
					default: throw new InvalidOperationException();
				}
			}
			set {
				LocalFiles = new ITaskItem[] { new TaskItem(value) };
				string fileNameOnly = Path.GetFileName(value);
				RemoteFiles = new ITaskItem[] { new TaskItem { ItemSpec = fileNameOnly } };
			}
		}

		/// <summary>
		/// Gets or sets the local files to upload.  Use this
		/// or LocalFile, but not both.
		/// </summary>
		/// <value>The local file.</value>
		public ITaskItem[] LocalFiles {
			get { return _localFiles; }
			set { _localFiles = value; }
		}

		private ITaskItem[] _remoteFiles;

		/// <summary>
		/// Gets or sets the remote files to upload.
		/// Each item in this list should have a corresponding item in LocalFiles.
		/// </summary>
		[Required]
		public ITaskItem[] RemoteFiles {
			get { return _remoteFiles; }
			set { _remoteFiles = value; }
		}


		private string _remoteUri;

		/// <summary>
		/// Gets or sets the remote URI to upload.
		/// </summary>
		/// <value>The remote URI.</value>
		[Required]
		public string RemoteUri {
			get { return _remoteUri; }
			set { _remoteUri = value; }
		}

		private string _username;

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>The username.</value>
		public string Username {
			get { return _username; }
			set { _username = value; }
		}

		private string _password;

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
		public string Password {
			get { return _password; }
			set { _password = value; }
		}

		private bool _usePassive;

		/// <summary>
		/// Gets or sets the behavior of a client application's data transfer process.
		/// </summary>
		/// <value><c>true</c> if [use passive]; otherwise, <c>false</c>.</value>
		public bool UsePassive {
			get { return _usePassive; }
			set { _usePassive = value; }
		}

        private bool _keepAlive;

        /// <summary>
        /// Gets or sets a value that indicates whether to make a persistent connection to the Internet resource.
        /// </summary>        
        public bool KeepAlive
        {
            get { return _keepAlive; }
            set { _keepAlive = value; }
        }

        private int _timeout;

        /// <summary>
        /// Gets or sets the time-out value in milliseconds
        /// </summary>  
        /// <value>The number of milliseconds to wait before the request times out. The default value is 7000 milliseconds (7 seconds).</value>
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// true if the task successfully executed; otherwise, false.
		/// </returns>
		public override bool Execute() {
			List<string> createdRemoteFolders = new List<string>();
			bool allSuccess = true;
			EnsureDirectoriesCreated();
			int MAX_RETRIES = 3;
			for (int i = 0; i < _localFiles.Length; i++) {
				for (int retry = 0; retry <= MAX_RETRIES; retry++) {
					bool success = UploadFile(_localFiles[i].ItemSpec, _remoteFiles[i].ItemSpec, i < MAX_RETRIES);
					if (success) {
						break;
					}
					if (retry < MAX_RETRIES) {
						Log.LogMessage("File failed to upload: {0}", _localFiles[i].ItemSpec);
						if (retry > 1) {
							Log.LogMessage("Taking a break for a second.");
							Thread.Sleep(TimeSpan.FromSeconds(1.5));
						}
						Log.LogMessage("Starting retry {0} of {1}", retry + 1, MAX_RETRIES);
					} else {
						Log.LogError("Failed all retries try on {0}.", _localFiles[i].ItemSpec);
						allSuccess = false;
					}
				}
			}
			return allSuccess;
		}

		private void EnsureDirectoriesCreated() {
			List<string> directories = GetUniqueDirectories(_remoteFiles);
			Dictionary<string, List<string>> directoryCache = new Dictionary<string, List<string>>();
			directories.Sort(delegate(string a, string b) { return a.Length.CompareTo(b.Length); });
			foreach (string dir in directories) {
				Log.LogMessage("Checking if {0} exists.", dir);
				if (!DirectoryExists(directoryCache, dir)) {
					Log.LogMessage("Creating {0}.", dir);
					CreateDirectory(dir);
				}
			}
		}

		private void CreateDirectory(string dir) {
			IFtpWebRequest rq = CreateRequest(FtpPath(dir), WebRequestMethods.Ftp.MakeDirectory);
			rq.GetAndCloseResponse();
		}

		bool DirectoryExists(Dictionary<string, List<string>> directoryCache, string dirString) {
			if (dirString == null || dirString.Length == 0) {
				return true;
			}
			string parentDir = ParentPath(dirString);
			if (!directoryCache.ContainsKey(parentDir)) {
				IFtpWebRequest request = CreateRequest(FtpPath(parentDir), WebRequestMethods.Ftp.ListDirectory);
				StreamReader response = new StreamReader(request.GetResponseStream());
				directoryCache.Add(parentDir, new List<string>());
				while (!response.EndOfStream) {
					string line = response.ReadLine();
					directoryCache[parentDir].Add(line);
				}
				response.Close();
			}
			string dirName = NamePath(dirString);
			return directoryCache[parentDir].Exists(delegate(string listLine) {
				return listLine == dirName
					|| listLine.EndsWith("/" + dirName)
					|| listLine.EndsWith("\\" + dirName);
			});
		}

		private Uri FtpPath(string dir) {
			string uriString = (RemoteUri + dir).Replace("\\", "/");
			Uri ftpUri;
			if (!Uri.TryCreate(uriString, UriKind.Absolute, out ftpUri)) {
				Log.LogError(Properties.Resources.FtpUriInvalid, uriString);
				return null;
			}
			return ftpUri;
		}

		private List<string> GetUniqueDirectories(ITaskItem[] files) {
			List<string> dirs = new List<string>();
			foreach (ITaskItem file in files) {
				for (string path = ParentPath(file.ItemSpec);
					path.Length > 0;
					path = ParentPath(path)) {
					if (!dirs.Contains(path)) {
						dirs.Add(path);
					}
				}
			}
			return dirs;
		}

		private static readonly char[] PathSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
		private string ParentPath(string path) {
			if (string.IsNullOrEmpty(path)) {
				throw new Exception("Cannot get the parent path of a blank string.");
			}
			int lastIdx = path.LastIndexOfAny(PathSeparators);
			if (lastIdx == -1) {
				return string.Empty;
			} else {
				return path.Substring(0, lastIdx);
			}
		}

		private string NamePath(string path) {
			if (string.IsNullOrEmpty(path)) {
				return path;
			}
			if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString())) {
				path = path.Substring(0, path.Length - 1);
			}
			int lastIdx = path.LastIndexOfAny(PathSeparators);
			if (lastIdx == -1) {
				return path;
			} else {
				return path.Substring(lastIdx + 1);
			}
		}

		private bool UploadFile(string localFilePath, string remoteFilePath, bool errorOnFailure) {
			Uri uri = FtpPath(remoteFilePath);
			FileInfo localFile = new FileInfo(localFilePath);
			if (!localFile.Exists) {
				Log.LogError(Properties.Resources.FtpLocalNotFound, localFile);
				return false;
			}

			Log.LogMessage(Properties.Resources.FtpUploading, localFilePath, uri);

			IFtpWebRequest request = CreateRequest(uri, WebRequestMethods.Ftp.UploadFile);
			request.SetContentLength(localFile.Length);

			const int bufferLength = 2048;
			byte[] buffer = new byte[bufferLength];
			int readBytes = 0;
			long totalBytes = localFile.Length;
			long progressUpdated = 0;
			long wroteBytes = 0;

			try {
				Stopwatch watch = Stopwatch.StartNew();
				using (Stream fileStream = localFile.OpenRead(),
							requestStream = request.GetRequestStream()) {
					do {
						readBytes = fileStream.Read(buffer, 0, bufferLength);
						requestStream.Write(buffer, 0, readBytes);
						wroteBytes += readBytes;

						// log progress every 5 seconds
						if (watch.ElapsedMilliseconds - progressUpdated > 5000) {
							progressUpdated = watch.ElapsedMilliseconds;
							Log.LogMessage(MessageImportance.Normal,
								Properties.Resources.FtpPercentComplete,
								wroteBytes * 100 / totalBytes,
								ByteDescriptions.FormatBytesPerSecond(wroteBytes, watch.Elapsed.TotalSeconds, 1));
						}
					}
					while (readBytes != 0);
				}
				watch.Stop();

				string statusDescription = request.GetStatusDescriptionAndCloseResponse();
				Log.LogMessage(MessageImportance.Low, Properties.Resources.FtpUploadComplete, statusDescription);
				Log.LogMessage(Properties.Resources.FtpTransfered,
					ByteDescriptions.FormatByte(totalBytes, 1),
					ByteDescriptions.FormatBytesPerSecond(totalBytes, watch.Elapsed.TotalSeconds, 1),
					watch.Elapsed.ToString());
			} catch (Exception ex) {
				if (errorOnFailure) {
					Log.LogErrorFromException(ex);
				} else {
					Log.LogMessage("Exception occurred trying to upload: " + ex.Message);
				}
				return false;
			}

			return true;
		}

		private IFtpWebRequest CreateRequest(Uri uri, string method) {
			Log.LogMessage("{0}: {1}", method, uri);

			IFtpWebRequestCreator creator = _requestCreator;
			if (creator == null) {
				creator = this;
			}

			return creator.Create(uri, method);
		}

		IFtpWebRequest IFtpWebRequestCreator.Create(Uri uri, string method) {
			FtpWebRequest rq = (FtpWebRequest)WebRequest.Create(uri);
			rq.Method = method;
			rq.UsePassive = _usePassive;
			rq.UseBinary = true;
			rq.Timeout = _timeout;
			rq.KeepAlive = _keepAlive;
			if (!string.IsNullOrEmpty(_username)) {
				rq.Credentials = new NetworkCredential(_username, _password);
			}
			return new RealFtpWebRequest(rq);
		}

	}

}

