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
	/// An adapter to make the real FtpWebRequest look like
	/// an IFtpWebRequest.
	/// </summary>
	class RealFtpWebRequest : IFtpWebRequest {

		private FtpWebRequest realRequest;

		/// <summary>
		/// Initializes a new instance of the RealFtpWebRequest class.
		/// </summary>
		public RealFtpWebRequest(FtpWebRequest realRequest) {
			this.realRequest = realRequest;
		}

		public void SetContentLength(long length) {
			realRequest.ContentLength = length;
		}

		public Stream GetRequestStream() {
			return realRequest.GetRequestStream();
		}

		public string GetStatusDescriptionAndCloseResponse() {
			string status;
			using (FtpWebResponse response = (FtpWebResponse)realRequest.GetResponse()) {
				status = response.StatusDescription;
				response.Close();
			}
			return status;
		}

		public void GetAndCloseResponse() {
			realRequest.GetResponse().Close();
		}

		public Stream GetResponseStream() {
			return realRequest.GetResponse().GetResponseStream();
		}

	}

}
