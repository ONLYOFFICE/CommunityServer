//-----------------------------------------------------------------------
// <copyright file="Xslt.cs" company="MSBuild Community Tasks Project">
//     Copyright � 2006 Ignaz Kohlbecker
// </copyright>
//-----------------------------------------------------------------------


namespace MSBuild.Community.Tasks
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Xml.Xsl;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using MSBuild.Community.Tasks.Properties;

	/// <summary>
	/// A task to merge and transform a set of xml files.
	/// </summary>
	/// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="Xslt"]/*'/>
	public class Xslt : Task
	{
		#region Constants

		/// <summary>
		/// The name of the default attribute
		/// of the <see cref="RootTag"/>.
		/// The value is <c>"created"</c>,
		/// and the attribute will contain a local time stamp.
		/// </summary>
		public const string CreatedAttributeName = @"created";

		/// <summary>
		/// The prefix of XSLT parameters created from single XML input metadata.
		/// <para>The value is <c>"input_"</c>.</para>
		/// </summary>
		public const string InputMetadataArgumentPrefix = @"input_";

		#endregion Constants

		#region Fields
		private ITaskItem[] inputs;
		private string rootTag;
		private string rootAttributes;
		private bool useTrusted;
		private ITaskItem xsl;
		private string output;
		#endregion Fields

		#region Input Parameters

		/// <summary>
		/// Gets or sets the xml input files.
		/// </summary>
		[Required]
		public ITaskItem[] Inputs
		{
			get
			{
				return this.inputs;
			}

			set
			{
				this.inputs = value;
			}
		}

		/// <summary>
		/// Gets or sets the xml tag name
		/// of the root tag wrapped
		/// around the merged xml input files.
		/// </summary>
		public string RootTag
		{
			get
			{
				return this.rootTag;
			}

			set
			{
				this.rootTag = value;
			}
		}

		/// <summary>
		/// Gets or sets the list of
		/// semicolon-delimited name/value pairs
		/// of the <see cref="RootTag"/>.
		/// For example: <code>RootAttributes="foo=bar;date=$(buildDate)"</code>
		/// </summary>
		public string RootAttributes
		{
			get
			{
				return this.rootAttributes;
			}

			set
			{
				this.rootAttributes = value;
			}
		}
		
		/// <summary>
		/// Enables a Trusted XSLT processor. Sepcifically enables scripts 
		/// in the xsl transformation file
		/// For example: <code>UseTrusted="true"</code>
		/// </summary>
		public bool UseTrusted
		{
			get
			{
				return useTrusted;
			}
			set
			{
				useTrusted = value;
			}
		}

		/// <summary>
		/// Gets or sets the path of the
		/// xsl transformation file to apply.
		/// </summary>
		/// <remarks>
		/// The property can be given any number of metadata,
		/// which will be handed to the xsl transformation
		/// as parameters.
		/// </remarks>
		[Required]
		public ITaskItem Xsl
		{
			get
			{
				return this.xsl;
			}

			set
			{
				this.xsl = value;
			}
		}

		/// <summary>
		/// Gets or sets the path of the output file.
		/// </summary>
		[Required]
		public string Output
		{
			get
			{
				return this.output;
			}

			set
			{
				this.output = value;
			}
		}

		#endregion Input Parameters

		#region Task overrides
		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if the task successfully executed; otherwise, <c>false</c>.
		/// </returns>
		public override bool Execute()
		{
			#region Sanity checks
			if ((this.Inputs == null) || (this.Inputs.Length == 0))
			{
				Log.LogError(Resources.XsltNoInputFiles);
				return false;
			}
			#endregion Sanity checks

			#region Create and fill xml working document
			// The working document
			XmlDocument doc = new XmlDocument();

			try
			{
				if ((this.inputs.Length == 1) && string.IsNullOrEmpty(this.rootTag))
				{
					Log.LogMessage(MessageImportance.Low, Resources.XsltNoRootTag);
					doc.Load(this.inputs[0].ItemSpec);
				}
				else
				{
					this.CreateRootNode(doc);

					#region Populate root node
					foreach (ITaskItem input in this.inputs)
					{
						// create and load a xml input file
						XmlDocument inputDocument = new XmlDocument();
						inputDocument.Load(input.ItemSpec);

						// import the root node of the xml input file
						// into the working file
						XmlNode importNode = doc.ImportNode(inputDocument.DocumentElement, true);
						doc.DocumentElement.AppendChild(importNode);
					}

					#endregion Populate root node
				}
			}
			catch (XmlException ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}
			catch (ArgumentException ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}
			catch (InvalidOperationException ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}

			#endregion Create and fill xml working document

			#region Create and execute the transform
			XmlWriter xmlWriter = null;

			XslCompiledTransform transform = new XslCompiledTransform();

			#region Parameters from Metadata

			XsltArgumentList argumentList = new XsltArgumentList();

			// add metadata from xsl
			foreach (string metadataName in this.xsl.MetadataNames)
			{
				AddParameter(metadataName, this.xsl.GetMetadata(metadataName), argumentList);
			}

			// add metadata from single xml
			if (this.inputs.Length == 1)
			{
				ITaskItem input = this.inputs[0];
				foreach (string metadataName in input.MetadataNames)
				{
					// add two times by design, but note that the first parameter might be ignored.
					AddParameter(metadataName, input.GetMetadata(metadataName), argumentList);
					AddParameter(InputMetadataArgumentPrefix + metadataName, input.GetMetadata(metadataName),
						argumentList);
				}
			}
			#endregion Parameters from Metadata

			try
			{
                if(useTrusted) 
                {
                    transform.Load(xsl.ItemSpec, XsltSettings.TrustedXslt, null);
                }
                else
                {   
                    transform.Load(xsl.ItemSpec, XsltSettings.Default, new XmlUrlResolver());
                }

				xmlWriter = XmlWriter.Create(this.output, transform.OutputSettings);

				transform.Transform(doc.DocumentElement, argumentList, xmlWriter);
			}
			catch (XsltException ex)
			{
				Log.LogErrorFromException(ex, false, true, new Uri(ex.SourceUri).LocalPath + '(' + ex.LineNumber + ',' + ex.LinePosition + ')');
				return false;
			}
			catch (FileNotFoundException ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}
			catch (DirectoryNotFoundException ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}
			catch (XmlException ex)
			{
				Log.LogErrorFromException(ex, false, true, new Uri(ex.SourceUri).LocalPath + '(' + ex.LineNumber + ',' + ex.LinePosition + ')');
				return false;
			}
			finally
			{
				if (xmlWriter != null)
				{
					xmlWriter.Close();
				}
			}

			#endregion Create and execute the transform

			return true;
		}

		#endregion Task overrides

		#region Private Methods

		private void CreateRootNode(XmlDocument doc)
		{
			// create the root element
			Log.LogMessage(MessageImportance.Normal, Resources.XsltCreatingRootTag, this.rootTag);
			XmlElement rootElement = doc.CreateElement(this.rootTag);

			if (this.rootAttributes == null)
			{
				// add the timestamp attribute to the root element
				string timestamp = DateTime.Now.ToString();
				Log.LogMessage(
					MessageImportance.Normal,
					Resources.XsltAddingRootAttribute,
					CreatedAttributeName,
					timestamp);
				rootElement.SetAttribute(CreatedAttributeName, timestamp);
			}
			else
			{
				foreach (string rootAttribute in this.rootAttributes.Split(';'))
				{
					string[] keyValuePair = rootAttribute.Split('=');

					Log.LogMessage(
						MessageImportance.Normal,
						Resources.XsltAddingRootAttribute,
						keyValuePair[0],
						keyValuePair[1]);

					rootElement.SetAttribute(keyValuePair[0], keyValuePair[1]);
				}
			}

			// insert the root element to the document
			doc.AppendChild(rootElement);
		}

		/// <summary>
		/// Adds a new xsl parameter with to the specified argument list.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <param name="parameters">The parameter list.</param>
		/// <returns>Whether the parameter was added.</returns>
		/// <remarks>Does not add the parameter
		/// when a parameter with the same name is already part of the <paramref name="parameters"/>.</remarks>
		private bool AddParameter(string name, string value, XsltArgumentList parameters)
		{
			bool result;

			if (parameters.GetParam(name, string.Empty) == null)
			{
				parameters.AddParam(name, string.Empty, value);

				Log.LogMessage(MessageImportance.Low, Resources.XsltAddingParameter, name, value);

				result = true;
			}
			else
			{
				// don't add the parameter when already provided by other xml metadata
				result = false;
			}
			return result;
		}

		#endregion Private Methods
	}
}