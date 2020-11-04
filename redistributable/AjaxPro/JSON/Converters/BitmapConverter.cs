/*
 * BitmapConverter.cs
 * 
 * Copyright © 2007 Michael Schwarz (http://www.ajaxpro.info).
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
/*
 * MS	06-06-01	initial version
 * MS	06-06-09	removed addNamespace use
 * MS	06-09-22	fixed disposing Bitmap after removed from cache
 * MS	06-09-26	improved performance using StringBuilder
 * 
 */
using System;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize a Bitmap object.
	/// </summary>
	public class BitmapConverter : IJavaScriptConverter
	{
		private string clientType = "Ajax.Web.Bitmap";
		private string mimeType = "image/jpeg";
		private long quality = 100L;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapConverter"/> class.
        /// </summary>
		public BitmapConverter()
			: base()
		{
			m_serializableTypes = new Type[] {
				typeof(Bitmap)
			};
		}

        /// <summary>
        /// Initializes the converter. This method will be called when the application is starting and
        /// any converter is loaded.
        /// </summary>
        /// <param name="d"></param>
		public override void Initialize(System.Collections.Specialized.StringDictionary d)
		{
			if (d.ContainsKey("mimeType"))
			{
				mimeType = d["mimeType"];

				if (d.ContainsKey("quality"))
				{
					long i = 0;
#if(NET20)
					if (long.TryParse(d["quality"], out i))
						quality = i;
#else
					try
					{
						i = long.Parse(d["quality"]);
						quality = i;
					}
					catch(Exception)
					{
					}
#endif
					
				}
			}
		}

        /// <summary>
        /// Render the JavaScript code for prototypes or any other JavaScript method needed from this converter
        /// on the client-side.
        /// </summary>
        /// <returns>Returns JavaScript code.</returns>
		public override string GetClientScript()
		{
			string appPath = System.Web.HttpContext.Current.Request.ApplicationPath;
			if(appPath != "/") appPath += "/";

			return JavaScriptUtil.GetClientNamespaceRepresentation(clientType) + @"
" + clientType + @" = function(id) {
	this.src = '" + appPath + @"ajaximage/' + id + '.ashx';
}

Object.extend(" + clientType + @".prototype,  {
	getImage: function() {
		var i = new Image();
		i.src = this.src;
		return i;
	}
}, false);
";
		}

        /// <summary>
        /// Converts a .NET object into a JSON string.
        /// </summary>
        /// <param name="o">The object to convert.</param>
        /// <returns>Returns a JSON string.</returns>
		public override string Serialize(object o)
		{
			StringBuilder sb = new StringBuilder();
			Serialize(o, sb);
			return sb.ToString();
		}

        /// <summary>
        /// Serializes the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="sb">The sb.</param>
		public override void Serialize(object o, StringBuilder sb)
		{
			string id = Guid.NewGuid().ToString();

			AjaxBitmap b = new AjaxBitmap();
			b.bmp = o as Bitmap;
			b.mimeType = this.mimeType;
			b.quality = this.quality;

			System.Web.HttpContext.Current.Cache.Add(id, b, 
				null, 
				System.Web.Caching.Cache.NoAbsoluteExpiration,
				TimeSpan.FromSeconds(30),
				System.Web.Caching.CacheItemPriority.BelowNormal,
#if(NET20)
				RemoveBitmapFromCache
#else
				new System.Web.Caching.CacheItemRemovedCallback(RemoveBitmapFromCache)
#endif
				);

			sb.Append("new ");
			sb.Append(clientType);
			sb.Append("('");
			sb.Append(id);
			sb.Append("')");
		}

        /// <summary>
        /// Removes the bitmap from cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="o">The o.</param>
        /// <param name="reason">The reason.</param>
		public static void RemoveBitmapFromCache(string key, object o, System.Web.Caching.CacheItemRemovedReason reason)
		{
			if(o != null)
			{
				AjaxBitmap b = o as AjaxBitmap;
				if (b != null && b.bmp != null) b.bmp.Dispose();
			}
		}
	}

	public class AjaxBitmap
	{
		public Bitmap bmp = null;
		public string mimeType = "image/jpeg";
		public long quality = 100L;
	}

	public class AjaxBitmapHttpHandler : IHttpHandler
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxBitmapHttpHandler"/> class.
        /// </summary>
		public AjaxBitmapHttpHandler()
			: base()
		{
		}

		#region IHttpHandler Members

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context)
		{
			string id = System.IO.Path.GetFileNameWithoutExtension(context.Request.FilePath);

			AjaxBitmap b = null;

			try
			{
				Guid guid = new Guid(id);
				b = context.Cache[id] as AjaxBitmap;
			}
			catch (Exception) { }

			if (b == null || b.bmp == null)
			{
				b.bmp = new Bitmap(10, 20);
				Graphics g = Graphics.FromImage(b.bmp);
				g.FillRectangle(new SolidBrush(Color.White), 0, 0, 10, 20);
				g.DrawString("?", new Font("Arial", 10), new SolidBrush(Color.Red), 0, 0);
			}

			context.Response.ContentType = b.mimeType;

			if (b.mimeType.ToLower() == "image/jpeg")
			{
				ImageCodecInfo[] enc = ImageCodecInfo.GetImageEncoders();

				EncoderParameters ep = new EncoderParameters(1);
				ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, b.quality);

				b.bmp.Save(context.Response.OutputStream, enc[1], ep);
				return;
			}
			else
			{
				switch (b.mimeType.ToLower())
				{
					case "image/gif":
						b.bmp.Save(context.Response.OutputStream, ImageFormat.Gif);
						return;

					case "image/png":
						b.bmp.Save(context.Response.OutputStream, ImageFormat.Png);
						return;
				}
			}

			throw new NotSupportedException("'" + b.mimeType + "' is not supported.");
		}

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"></see> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
		public bool IsReusable
		{
			get
			{
				// TODO:  Add AjaxAsyncHttpHandler.IsReusable getter implementation
				return false;
			}
		}

		#endregion
	}
}
