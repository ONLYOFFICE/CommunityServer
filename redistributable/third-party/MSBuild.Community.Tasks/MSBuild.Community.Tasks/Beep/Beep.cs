//-----------------------------------------------------------------------
// <copyright file="Beep.cs" company="MSBuild Community Tasks Project">
//     Copyright © 2008 Ignaz Kohlbecker
// </copyright>
//-----------------------------------------------------------------------


namespace MSBuild.Community.Tasks
{
	using System;
	using System.Security;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// A task to play the sound of a beep through the console speaker.
	/// </summary>
	/// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="Beep"]/*'/>
	public class Beep : Task
	{
		#region Fields

		private int frequency = 800;
		private int duration = 200;

		#endregion Fields

		#region Input Parameters

		/// <summary>
		/// Gets or sets the frequency of the beep, ranging from 37 to 32767 hertz.
		/// Defaults to 800 hertz.
		/// </summary>
		public int Frequency
		{
			get { return this.frequency; }
			set { this.frequency = value; }
		}

		/// <summary>
		/// Gets or sets the of the beep measured in milliseconds.
		/// Defaults to 200 milliseconds.
		/// </summary>
		public int Duration
		{
			get { return this.duration; }
			set { this.duration = value; }
		}

		#endregion Input Parameters

		#region Task overrides

		/// <summary>
		/// Plays the sound of a beep 
		/// at the given <see cref="Frequency"/> and for the given <see cref="Duration"/> 
		/// through the console speaker.
		/// </summary>
		/// <returns>
		/// Always returns <see langword="true"/>, even when the sound could not be played.
		/// </returns>
		public override bool Execute()
		{
			try
			{
				Console.Beep(this.Frequency, this.Duration);
			}
			catch (ArgumentOutOfRangeException)
			{
			}
			catch (HostProtectionException)
			{
			}

			return true;
		}

		#endregion Task overrides
	}
}