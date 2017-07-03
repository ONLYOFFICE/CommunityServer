using System;
using System.Collections.Generic;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Interfaces.Components
{
    public interface ITodo : IRecurringComponent
    {
        /// <summary>
        /// The date/time the todo was completed.
        /// </summary>
        IDateTime Completed { get; set; }

        /// <summary>
        /// The due date of the todo item.
        /// </summary>
        IDateTime Due { get; set; }

        /// <summary>
        /// The duration of the todo item.
        /// </summary>
        // NOTE: Duration is not supported by all systems,
        // (i.e. iPhone 1st gen) and cannot co-exist with Due.
        // RFC 5545 states:
        //
        //      ; either 'due' or 'duration' may appear in
        //      ; a 'todoprop', but 'due' and 'duration'
        //      ; MUST NOT occur in the same 'todoprop'
        //
        // Therefore, Duration should not be serialized, as 
        // it can be extrapolated from the Due date.
        TimeSpan Duration { get; set; }

        /// <summary>
        /// The geographic location (lat/long) of the todo item.
        /// </summary>
        IGeographicLocation GeographicLocation { get; set; }

        /// <summary>
        /// The location of the todo item.
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// A number between 0 and 100 that represents
        /// the percentage of completion of this item.
        /// </summary>
        int PercentComplete { get; set; }

        /// <summary>
        /// A list of resources associated with this todo item.
        /// </summary>
        IList<string> Resources { get; set; }

        /// <summary>
        /// The current status of the todo item.
        /// </summary>
        TodoStatus Status { get; set; }

        /// <summary>
        /// Use this method to determine if a todo item has been completed.
        /// This takes into account recurrence items and the previous date
        /// of completion, if any.        
        /// <note>
        /// This method evaluates the recurrence pattern for this TODO
        /// as necessary to ensure all relevant information is taken
        /// into account to give the most accurate result possible.
        /// </note>
        /// </summary>
        /// <param name="currDt">The date and time to test.</param>
        /// <returns>True if the todo item has been completed</returns>
        bool IsCompleted(IDateTime currDt);

        /// <summary>
        /// Returns 'True' if the todo item is Active as of <paramref name="currDt"/>.
        /// An item is Active if it requires action of some sort.
        /// </summary>
        /// <param name="currDt">The date and time to test.</param>
        /// <returns>True if the item is Active as of <paramref name="currDt"/>, False otherwise.</returns>
        bool IsActive(IDateTime currDt);

        /// <summary>
        /// Returns True if the todo item was cancelled.
        /// </summary>
        /// <returns>True if the todo was cancelled, False otherwise.</returns>
        bool IsCancelled();
    }
}