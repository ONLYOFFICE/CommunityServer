

using System;
using System.Collections.Generic;

namespace MSBuild.Community.Tasks.Ftp
{
    /// <summary>
    /// Represents an remote file or directory on a FTP server.
    /// </summary>
    public class FtpEntry
    {
        /// <summary>
        /// Indicates whether this instance represents a directory.
        /// </summary>
        private bool _isDirectory;

        /// <summary>
        /// Represents the file or directory name.
        /// </summary>
        private String _name;

        /// <summary>
        /// Gets or sets a value indicating whether this instance represents a directory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance represents a directory; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory
        {
            get
            {
                return _isDirectory;
            }
            set
            {
                _isDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpEntry"/> class.
        /// </summary>
        private FtpEntry()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpEntry"/> class.
        /// </summary>
        /// <param name="isDirectory">if set to <c>true</c> this instance represents a directory; otherwise, <c>false</c>.</param>
        /// <param name="name">The name.</param>
        public FtpEntry( bool isDirectory, String name )
        {
            _isDirectory = isDirectory;
            _name = name;
        }

        private static FtpEntry ParseDosDirLine( string entryLine )
        {
            FtpEntry entry = new FtpEntry();

            try
            {
                string[] parsed = new string[3];
                int index = 0;
                int position = 0;

                // Parse out the elements
                position = entryLine.IndexOf( ' ' );
                while(index < parsed.Length)
                {
                    parsed[index] = entryLine.Substring( 0, position );
                    entryLine = entryLine.Substring( position );
                    entryLine = entryLine.Trim();
                    index++;
                    position = entryLine.IndexOf( ' ' );
                }
                entry.Name = entryLine;
                entry.IsDirectory = parsed[2] == "<DIR>";
            }
            catch
            {
                entry = null;
            }
            return entry;
        }

        private static FtpEntry ParseUnixDirLine( string entryLine )
        {
            FtpEntry entry = new FtpEntry();

            try
            {
                string[] parsed = new string[8];
                int index = 0;
                int position;

                // Parse out the elements
                position = entryLine.IndexOf( ' ' );
                while(index < parsed.Length)
                {
                    parsed[index] = entryLine.Substring( 0, position );
                    entryLine = entryLine.Substring( position );
                    entryLine = entryLine.Trim();
                    index++;
                    position = entryLine.IndexOf( ' ' );
                }
                entry.Name = entryLine;
                entry.IsDirectory = parsed[0][0] == 'd';
            }
            catch
            {
                entry = null;
            }
            return entry;
        }

        /// <summary>
        /// Parses the dir list.
        /// </summary>
        /// <param name="entryLines">The entry lines.</param>
        /// <returns></returns>
        public static FtpEntry[] ParseDirList( string[] entryLines )
        {
            List<FtpEntry> files = new List<FtpEntry>( entryLines.Length );

            int autodetect = 0;

            foreach(string entryLine in entryLines)
            {
                FtpEntry entry = null;
                if(autodetect == 0)
                {
                    entry = ParseDosDirLine( entryLine );
                    if(entry == null)
                    {
                        entry = ParseUnixDirLine( entryLine );
                        autodetect = 2;
                    }
                    else
                        autodetect = 1;
                }
                else
                    if(autodetect == 1)
                        entry = ParseDosDirLine( entryLine );
                    else
                        if(autodetect == 2)
                            entry = ParseUnixDirLine( entryLine );

                if(entry != null)
                {
                    files.Add( entry );
                }
            }

            return files.ToArray();
        }
    }
}
