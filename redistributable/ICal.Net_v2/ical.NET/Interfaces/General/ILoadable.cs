using System;

namespace Ical.Net.Interfaces.General
{
    public interface ILoadable
    {
        /// <summary>
        /// Gets whether or not the object has been loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// An event that fires when the object has been loaded.
        /// </summary>
        event EventHandler Loaded;

        /// <summary>
        /// Fires the Loaded event.
        /// </summary>
        void OnLoaded();
    }
}