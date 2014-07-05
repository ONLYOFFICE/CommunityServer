//-----------------------------------------------------------------------
// <copyright file="Sound.cs" company="MSBuild Community Tasks Project">
//     Copyright © 2008 Ignaz Kohlbecker
// </copyright>
//-----------------------------------------------------------------------


namespace MSBuild.Community.Tasks
{
	using System;
	using System.IO;
	using System.Media;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// A task to play a sound from a .wav file path or URL.
	/// </summary>
	/// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="Sound"]/*'/>
	public class Sound : Task
	{
		#region Fields

		private SoundPlayer soundPlayer = new SoundPlayer();
		private bool sync = true;

		#endregion Fields

		#region Input Parameters

		/// <summary>
		/// Gets or sets the file path or URL of the .wav file to load.
		/// </summary>
		public string SoundLocation
		{
			get
			{
				return this.soundPlayer.SoundLocation;
			}

			set
			{
				this.soundPlayer.SoundLocation = value;
			}
		}

		/// <summary>
		/// Sets the file path of the .wav file to load
		/// as a relative path to <see cref="Environment.SystemDirectory"/>.
		/// </summary>
		/// <example>
		/// For example, on a Windows XP platform, you can call
		/// <code><![CDATA[<Sound SystemSoundFile="..\Media\Windows XP Startup.wav" />]]></code>
		/// </example>
		public string SystemSoundFile
		{
			set
			{
				this.soundPlayer.SoundLocation = Path.Combine(Environment.SystemDirectory, value);
			}
		}

		/// <summary>
		/// Sets the file path of the .wav file to load
		/// as a relative path to <see cref="Environment.SpecialFolder.MyMusic"/>.
		/// </summary>
		public string MyMusicFile
		{
			set
			{
				this.soundPlayer.SoundLocation = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
					value);
			}
		}

		/// <summary>
		/// Gets or sets the time, in milliseconds, in which the .wav file must load.
		/// </summary>
		/// <value>The number of milliseconds to wait. The default is 10000 (10 seconds).</value>
		/// <remarks>
		/// After this time has expired, the loading is canceled and the task execution fails.
		/// </remarks>
		public int LoadTimeout
		{
			get
			{
				return this.soundPlayer.LoadTimeout;
			}

			set
			{
				this.soundPlayer.LoadTimeout = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to play the sound synchronously.
		/// </summary>
		/// <value>
		/// 	<see langword="true"/> if playing the sound using the caller's thread (default);
		/// otherwise, <see langword="false"/> if playing the sound using a new thread.
		/// </value>
		public bool Synchron
		{
			get
			{
				return this.sync;
			}

			set
			{
				this.sync = value;
			}
		}

		#endregion Input Parameters

		#region Task overrides

		/// <summary>
		/// Loads the .wav file given by <see cref="SoundLocation"/>
		/// and plays the sound using a new thread.
		/// </summary>
		/// <returns>
		/// Returns <see langword="true"/> if the .wav file can successfully be played;
		/// otherwise, returns <see langword="false"/>.
		/// </returns>
		public override bool Execute()
		{
			bool result = false;
			try
			{
				if (this.Synchron)
				{
					this.soundPlayer.PlaySync();
				}
				else
				{
					this.soundPlayer.Play();
				}

				result = true;
			}
			catch (TimeoutException)
			{
			}
			catch (FileNotFoundException)
			{
			}
			catch (InvalidOperationException)
			{
			}

			return result;
		}

		#endregion Task overrides
	}
}