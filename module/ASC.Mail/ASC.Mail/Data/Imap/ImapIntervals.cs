/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Mail.Data.Imap
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
