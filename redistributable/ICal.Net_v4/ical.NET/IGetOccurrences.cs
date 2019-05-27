using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net
{
    public interface IGetOccurrences
    {
        /// <summary>
        /// Clears a previous evaluation, usually because one of the 
        /// key elements used for evaluation has changed 
        /// (Start, End, Duration, recurrence rules, exceptions, etc.).
        /// </summary>
        void ClearEvaluation();

        /// <summary>
        /// Returns all occurrences of this component that start on the date provided.
        /// All components starting between 12:00:00AM and 11:59:59 PM will be
        /// returned.
        /// <note>
        /// This will first Evaluate() the date range required in order to
        /// determine the occurrences for the date provided, and then return
        /// the occurrences.
        /// </note>
        /// </summary>
        /// <param name="dt">The date for which to return occurrences.</param>
        /// <returns>A list of Periods representing the occurrences of this object.</returns>
        HashSet<Occurrence> GetOccurrences(IDateTime dt);

        HashSet<Occurrence> GetOccurrences(DateTime dt);

        /// <summary>
        /// Returns all occurrences of this component that overlap with the date range provided.
        /// All components that overlap with the time range between <paramref name="startTime"/> and <paramref name="endTime"/> will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime);

        HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime);
    }

    public interface IGetOccurrencesTyped : IGetOccurrences
    {
        /// <summary>
        /// Returns all occurrences of components of type T that start on the date provided.
        /// All components starting between 12:00:00AM and 11:59:59 PM will be
        /// returned.
        /// <note>
        /// This will first Evaluate() the date range required in order to
        /// determine the occurrences for the date provided, and then return
        /// the occurrences.
        /// </note>
        /// </summary>
        /// <param name="dt">The date for which to return occurrences.</param>
        /// <returns>A list of Periods representing the occurrences of this object.</returns>
        HashSet<Occurrence> GetOccurrences<T>(IDateTime dt) where T : IRecurringComponent;

        HashSet<Occurrence> GetOccurrences<T>(DateTime dt) where T : IRecurringComponent;

        /// <summary>
        /// Returns all occurrences of components of type T that start within the date range provided.
        /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// will be returned.
        /// </summary>
        /// <param name="startTime">The starting date range</param>
        /// <param name="endTime">The ending date range</param>
        HashSet<Occurrence> GetOccurrences<T>(IDateTime startTime, IDateTime endTime) where T : IRecurringComponent;

        HashSet<Occurrence> GetOccurrences<T>(DateTime startTime, DateTime endTime) where T : IRecurringComponent;
    }
}