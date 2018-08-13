/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Mail.Aggregator.Common.Imap
{
    public class ImapIntervals
    {
        public ImapIntervals(IList<int> indexes)
        {
            if (indexes.Count < 2)
                throw new ArgumentException("ImapIntervals: indexes argument should contains at least two items");

            if (indexes.Count % 2 != 0)
                throw new ArgumentException("ImapIntervals: indexes number should be even");

            _unhandledIntervals = new List<UidInterval>();

            for (var i = indexes.Count / 2; i > 0; i--)
            {
                _unhandledIntervals.Add(new UidInterval(indexes[2 * i - 2], indexes[2 * i - 1]));
            }
        }

        public IList<int> ToIndexes()
        {
            var res = new List<int>();

            foreach (var interval in _unhandledIntervals)
            {
                res.Add(interval.To);
                res.Add(interval.From);
            }

            res.Reverse();
            return res;
        }

        public IList<UidInterval> GetUnhandledIntervalsCopy()
        {
            return new List<UidInterval>(_unhandledIntervals);
        }

        public void AddHandledInterval(UidInterval newInterval)
        {
            var newIntervals = new List<UidInterval>();
            foreach (var interval in _unhandledIntervals)
            {
                if (interval.To < newInterval.From || interval.From > newInterval.To)
                    newIntervals.Add(interval);
                else
                {
                    if (interval.To > newInterval.To)
                        newIntervals.Add(new UidInterval(newInterval.To + 1, interval.To));
                    if (interval.From < newInterval.From)
                        newIntervals.Add(new UidInterval(interval.From, newInterval.From - 1));
                }
            }
            _unhandledIntervals = newIntervals;
        }

        public void AddUnhandledInterval(UidInterval newInterval)
        {
            var newIntervals = new List<UidInterval>();

            var intersectionMinFrom = newInterval.From;
            var intersectionMaxTo = newInterval.To;
            var intersectionWasAddedFlag = false;

            foreach (var interval in _unhandledIntervals)
            {
                if (interval.From > newInterval.To)
                    newIntervals.Add(interval);
                else if (interval.To < newInterval.From)
                {
                    if (!intersectionWasAddedFlag)
                    {
                        intersectionWasAddedFlag = true;
                        newIntervals.Add(new UidInterval(intersectionMinFrom, intersectionMaxTo));
                    }
                    newIntervals.Add(interval);
                }
                else
                {
                    intersectionMinFrom = Math.Min(intersectionMinFrom, interval.From);
                    intersectionMaxTo = Math.Max(intersectionMaxTo, interval.To);
                }
            }

            if (!intersectionWasAddedFlag)
                newIntervals.Add(new UidInterval(intersectionMinFrom, intersectionMaxTo));

            _unhandledIntervals = newIntervals;
        }

        public void SetBeginIndex(int index)
        {
            var newIntervals = (from interval in _unhandledIntervals
                                where interval.To == 0 || (interval.To >= index && interval.To != index)
                                select interval.From > index ? interval : new UidInterval(index + 1, interval.To))
                .ToList();
            _unhandledIntervals = newIntervals;
        }

        // uid intervals of unhandled messages
        private IList<UidInterval> _unhandledIntervals;
    }
}
