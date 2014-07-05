

namespace MSBuild.Community.Tasks.Oracle
{
    /// <summary>
    /// Contains information about a TNS definition
    /// </summary>
    /// <exclude />
    public class TnsEntry
    {
        private int startPosition;
        private int length;

        /// <summary>
        /// Creates a new instance of a TnsEntry
        /// </summary>
        /// <param name="startPosition">The position of the entry within a TNSNAMES.ORA file</param>
        /// <param name="length">The length of the entry definition within the TNSNAMES.ORA file</param>
        public TnsEntry(int startPosition, int length)
        {
            this.startPosition = startPosition;
            this.length = length;
        }

        /// <summary>
        /// The position of the entry within a TNSNAMES.ORA file
        /// </summary>
        public int StartPosition
        {
            get { return startPosition; }
        }

        /// <summary>
        /// The length of the entry definition within the TNSNAMES.ORA file
        /// </summary>
        public int Length
        {
            get { return length; }
        }
    }
}