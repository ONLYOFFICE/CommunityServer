/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System.Collections.Generic;
using System.Linq;
using ASC.Mail.Aggregator.Common.Imap;
using NUnit.Framework;

namespace ASC.Mail.Aggregator.Tests.Common
{
    [TestFixture]
    class ImapIntervalsTest
    {
        [Test]
        [ExpectedException("System.ArgumentException")]
        public void CreateIntervalsFromEmptyIndexesList()
        {
            var intervals = new ImapIntervals(new List<int>());
            Assert.IsNotNull(intervals);
        }

        [Test]
        [ExpectedException("System.ArgumentException")]
        public void CreateIntervalsFromSingleItemList()
        {
            var intervals = new ImapIntervals(new List<int> { 1 });
            Assert.IsNotNull(intervals);
        }

        [Test]
        [ExpectedException("System.ArgumentException")]
        public void CreateIntervalsFromUnevenList()
        {
            var intervals = new ImapIntervals(new List<int> { 10, 20, 30 });
            Assert.IsNotNull(intervals);
        }

        [Test]
        public void CreateIntervalsFromZeroItemsList()
        {
            var imapIntervals = new ImapIntervals(new List<int> {1, int.MaxValue});
            var uidIntervals = imapIntervals.GetUnhandledIntervalsCopy();
            Assert.IsTrue(uidIntervals.Contains(new UidInterval(1, int.MaxValue)));
            Assert.IsTrue(uidIntervals.Count == 1);
        }

        [Test]
        public void CreateIntervalsFromEvenList1()
        {
            var imapIntervals = new ImapIntervals(new List<int> { 1, 10, 20, int.MaxValue });
            var uidIntervals = imapIntervals.GetUnhandledIntervalsCopy();
            Assert.IsTrue(uidIntervals.Contains(new UidInterval(1, 10)));
            Assert.IsTrue(uidIntervals.Contains(new UidInterval(20, int.MaxValue)));
        }

        [Test]
        public void CreateIntervalsFromEvenList2()
        {
            var imapIntervals = new ImapIntervals(new List<int> { 10, 20, 30, 40, 50, int.MaxValue });
            var uidIntervals = imapIntervals.GetUnhandledIntervalsCopy();
            Assert.IsTrue(uidIntervals.Contains(new UidInterval(10, 20)));
            Assert.IsTrue(uidIntervals.Contains(new UidInterval(30, 40)));
            Assert.IsTrue(uidIntervals.Contains(new UidInterval(50, int.MaxValue)));
        }

        [Test]
        public void CreateIntervalsFromEvenList3()
        {
            var imapIntervals = new ImapIntervals(new List<int> { 10, int.MaxValue });
            var uidIntervals = imapIntervals.GetUnhandledIntervalsCopy();
            Assert.IsTrue(uidIntervals.Contains(new UidInterval(10, int.MaxValue)));
        }

        public void ToIndexesWithoutModificationTestBase(int[] indexesArr)
        {
            var indexesList = new List<int>(indexesArr);
            var imapIntervals = new ImapIntervals(indexesList);
            Assert.IsTrue(indexesList.SequenceEqual(imapIntervals.ToIndexes()));
        }

        [Test]
        public void ToIndexesWithoutModificationTest1()
        {
            ToIndexesWithoutModificationTestBase(new[] { 1, int.MaxValue });
        }

        [Test]
        public void ToIndexesWithoutModificationTest2()
        {
            ToIndexesWithoutModificationTestBase(new[] { 1, 10, 20, int.MaxValue });
        }

        [Test]
        public void ToIndexesWithoutModificationTest3()
        {
            ToIndexesWithoutModificationTestBase(new[] { 10, int.MaxValue });
        }

        public void SetBeginIndexForSeveralIntervalsBase(int[] indexes, int beginIndex, int[] indexesMody, UidInterval[] intervals)
        {
            var imapIntervals = new ImapIntervals(new List<int>(indexes));
            imapIntervals.SetBeginIndex(beginIndex);
            var uidIntervals = imapIntervals.GetUnhandledIntervalsCopy();
            foreach (var interval in intervals)
            {
                Assert.IsTrue(uidIntervals.Contains(interval));
            }

            var indexesListMody = new List<int>(indexesMody);
            Assert.IsTrue(indexesListMody.SequenceEqual(imapIntervals.ToIndexes()));
        }

        [Test]
        public void SetBeginIndexForSeveralIntervals1()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(6, 10),
                new UidInterval(20, 30),
                new UidInterval(40, int.MaxValue)
            };
            SetBeginIndexForSeveralIntervalsBase(new[] { 1, 10, 20, 30, 40, int.MaxValue }, 5, new[] { 6, 10, 20, 30, 40, int.MaxValue }, intervals.ToArray());
        }

        [Test]
        public void SetBeginIndexForSeveralIntervals2()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(20, 30),
                new UidInterval(40, int.MaxValue)
            };
            SetBeginIndexForSeveralIntervalsBase(new[] { 1, 10, 20, 30, 40, int.MaxValue }, 10, new[] { 20, 30, 40, int.MaxValue }, intervals.ToArray());
        }

        [Test]
        public void SetBeginIndexForSeveralIntervals3()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(11, int.MaxValue)
            };
            SetBeginIndexForSeveralIntervalsBase(new[] { 1, int.MaxValue }, 10, new[] { 11, int.MaxValue }, intervals.ToArray());
        }

        public void AddHandledIntervalBaseTest(int[] indexes, UidInterval newInterval, int[] indexesMody, UidInterval[] intervals)
        {
            var imapIntervals = new ImapIntervals(new List<int>(indexes));
            imapIntervals.AddHandledInterval(newInterval);
            var uidIntervals = imapIntervals.GetUnhandledIntervalsCopy();
            Assert.IsTrue(uidIntervals.Count == intervals.Count());
            foreach (var interval in intervals)
            {
                Assert.IsTrue(uidIntervals.Contains(interval));
            }

            var indexesListMody = new List<int>(indexesMody);
            Assert.IsTrue(indexesListMody.SequenceEqual(imapIntervals.ToIndexes()));
        }

        [Test]
        public void AddHandledIntervalToEmptyIntervals()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(1, 9),
                new UidInterval(21, int.MaxValue)
            };
            AddHandledIntervalBaseTest(new[] { 1, int.MaxValue }, new UidInterval(10, 20), new[] { 1, 9, 21, int.MaxValue }, intervals.ToArray());
        }

        [Test]
        public void AddHandledIntervalToIntervals1()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(1, 4),
                new UidInterval(20, int.MaxValue)
            };
            AddHandledIntervalBaseTest(new[] { 1, 10, 20, int.MaxValue }, new UidInterval(5, 10), new[] { 1, 4, 20, int.MaxValue }, intervals.ToArray());
        }

        [Test]
        public void AddHandledIntervalToIntervals2()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(20, int.MaxValue)
            };
            AddHandledIntervalBaseTest(new[] { 1, 10, 20, int.MaxValue }, new UidInterval(1, 10), new[] { 20, int.MaxValue }, intervals.ToArray());
        }

        [Test]
        public void AddHandledIntervalToIntervals3()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(1, 10),
                new UidInterval(20, 24),
                new UidInterval(31, int.MaxValue)
            };
            AddHandledIntervalBaseTest(new[] { 1, 10, 20, int.MaxValue }, new UidInterval(25, 30), new[] { 1, 10, 20, 24, 31, int.MaxValue }, intervals.ToArray());
        }

        public void AddUnhandledIntervalBaseTest(int[] indexes, UidInterval newInterval, int[] indexesMody, UidInterval[] intervals)
        {
            var imapIntervals = new ImapIntervals(new List<int>(indexes));
            imapIntervals.AddUnhandledInterval(newInterval);
            var uidIntervals = imapIntervals.GetUnhandledIntervalsCopy();
            Assert.IsTrue(uidIntervals.Count == intervals.Count());
            foreach (var interval in intervals)
            {
                Assert.IsTrue(uidIntervals.Contains(interval));
            }

            var indexesListMody = new List<int>(indexesMody);
            Assert.IsTrue(indexesListMody.SequenceEqual(imapIntervals.ToIndexes()));
        }

        [Test]
        public void AddUnhandledIntervalToIntervals1()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(1, 10),
                new UidInterval(20, int.MaxValue)
            };
            AddUnhandledIntervalBaseTest(new[] { 1, 10, 20, int.MaxValue }, new UidInterval(5, 10), new[] { 1, 10, 20, int.MaxValue }, intervals.ToArray());
        }

        [Test]
        public void AddUnhandledIntervalToIntervals2()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(1, int.MaxValue)
            };
            AddUnhandledIntervalBaseTest(new[] { 1, 10, 20, int.MaxValue }, new UidInterval(1, 20), new[] { 1, int.MaxValue }, intervals.ToArray());
        }

        [Test]
        public void AddUnhandledIntervalToIntervals3()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(1, 15),
                new UidInterval(20, int.MaxValue)
            };
            AddUnhandledIntervalBaseTest(new[] { 1, 10, 20, int.MaxValue }, new UidInterval(1, 15), new[] { 1, 15, 20, int.MaxValue }, intervals.ToArray());
        }

        [Test]
        public void AddUnhandledIntervalToIntervals4()
        {
            var intervals = new List<UidInterval>
            {
                new UidInterval(1, 15),
                new UidInterval(20, int.MaxValue)
            };
            AddUnhandledIntervalBaseTest(new[] { 20, int.MaxValue }, new UidInterval(1, 15), new[] { 1, 15, 20, int.MaxValue }, intervals.ToArray());
        }
    }
}
