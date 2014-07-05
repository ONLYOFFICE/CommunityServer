using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using System.Diagnostics;

namespace MSBuild.Community.Tasks {

	/// <summary>
	/// This class references an interface that looks like FtpWebRequest
	/// in order to support unit testing without an actual FTP Server.
	/// </summary>
	public interface IFtpWebRequest {

		/// <summary>
		/// Sets the ContentLength property of the FtpWebRequest.
		/// </summary>
		/// <param name="length"></param>
		void SetContentLength(long length);

		/// <summary>
		/// Calls GetRequestStream on the FtpWebRequest.
		/// </summary>
		/// <returns></returns>
		Stream GetRequestStream();

		/// <summary>
		/// Gets the StatusDescription property of the response, then closes the response
		/// on the FtpWebRequest.
		/// </summary>
		/// <returns></returns>
		string GetStatusDescriptionAndCloseResponse();

		/// <summary>
		/// Gets the response from the FTP server and closes it.
		/// </summary>
		void GetAndCloseResponse();

		/// <summary>
		/// Gets the response stream from the FtpWebRequest.
		/// </summary>
		/// <returns></returns>
		Stream GetResponseStream();

	}
}
