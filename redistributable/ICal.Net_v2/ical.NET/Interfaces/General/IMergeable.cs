namespace Ical.Net.Interfaces.General
{
    public interface IMergeable
    {
        /// <summary>
        /// Merges this object with another.
        /// </summary>
        void MergeWith(IMergeable obj);
    }
}