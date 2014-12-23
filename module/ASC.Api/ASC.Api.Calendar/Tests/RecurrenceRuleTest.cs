/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

#if DEBUG
namespace ASC.Api.Calendar.Tests
{
    using ASC.Web.Core.Calendars;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class RecurrenceRuleTest
    {
        private DateTime _toDate = new DateTime(2013, 6, 10, 9, 0, 0);
        private DateTime _fromDate = new DateTime(1990, 6, 10, 9, 0, 0);

        [TestMethod]
        public void YearlyRules()
        {           

            //test 1
            var r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Count = 10,
                ByMonth = new int[] { 6, 7 }
            };

            var dates = r1.GetDates(new DateTime(1997, 6, 10, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){new DateTime(1997, 6, 10, 9, 0, 0), new DateTime(1997, 7, 10, 9, 0, 0),
            new DateTime(1998, 6, 10, 9, 0, 0),new DateTime(1998, 7, 10, 9, 0, 0),
            new DateTime(1999, 6, 10, 9, 0, 0),new DateTime(1999, 7, 10, 9, 0, 0),
            new DateTime(2000, 6, 10, 9, 0, 0),new DateTime(2000, 7, 10, 9, 0, 0),
            new DateTime(2001, 6, 10, 9, 0, 0),new DateTime(2001, 7, 10, 9, 0, 0)}, dates);


            //test 2
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Interval = 2,
                Count = 10,
                ByMonth = new int[] { 1, 2,3 }
            };
            dates = r1.GetDates(new DateTime(1997, 3, 10, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){new DateTime(1997, 3, 10, 9, 0, 0),
                new DateTime(1999, 1, 10, 9, 0, 0),new DateTime(1999, 2, 10, 9, 0, 0), new DateTime(1999, 3, 10, 9, 0, 0),
                new DateTime(2001, 1, 10, 9, 0, 0),new DateTime(2001, 2, 10, 9, 0, 0), new DateTime(2001, 3, 10, 9, 0, 0),
                new DateTime(2003, 1, 10, 9, 0, 0),new DateTime(2003, 2, 10, 9, 0, 0), new DateTime(2003, 3, 10, 9, 0, 0),
            }, dates);


            //test 3
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Interval = 3,
                Count = 10,
                ByYearDay = new int[] { 1, 100, 200 }
            };
            dates = r1.GetDates(new DateTime(1997, 1, 1, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
                new DateTime(1997, 1, 1, 9, 0, 0),new DateTime(1997, 4, 10, 9, 0, 0),new DateTime(1997, 7, 19, 9, 0, 0), 
                new DateTime(2000, 1, 1, 9, 0, 0),new DateTime(2000, 4, 9, 9, 0, 0),new DateTime(2000, 7, 18, 9, 0, 0), 
                new DateTime(2003, 1, 1, 9, 0, 0),new DateTime(2003, 4, 10, 9, 0, 0),new DateTime(2003, 7, 19, 9, 0, 0), 
                new DateTime(2006, 1, 1, 9, 0, 0)
            }, dates);


            //test 4
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Count = 3,
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("20MO") }
            };
            dates = r1.GetDates(new DateTime(1997, 5, 19, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
                new DateTime(1997, 5, 19, 9, 0, 0),new DateTime(1998, 5, 18, 9, 0, 0),new DateTime(1999, 5, 17, 9, 0, 0)
            }, dates);


            //test 5
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Count = 3,
                ByWeekNo = new int[]{20},
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("MO") }
            };
            dates = r1.GetDates(new DateTime(1997, 5, 12, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
                new DateTime(1997, 5, 12, 9, 0, 0),new DateTime(1998, 5, 11, 9, 0, 0),new DateTime(1999, 5, 17, 9, 0, 0)
            }, dates);

            //test 6
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Count = 11,
                ByMonth = new int[] { 3 },
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("TH") }
            };
            dates = r1.GetDates(new DateTime(1997, 3, 13, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
               new DateTime(1997, 3, 13, 9, 0, 0), new DateTime(1997, 3, 20, 9, 0, 0),new DateTime(1997, 3, 27, 9, 0, 0),
               new DateTime(1998, 3, 5, 9, 0, 0), new DateTime(1998, 3, 12, 9, 0, 0),new DateTime(1998, 3, 19, 9, 0, 0),new DateTime(1998, 3, 26, 9, 0, 0),
               new DateTime(1999, 3, 4, 9, 0, 0),new DateTime(1999, 3, 11, 9, 0, 0),new DateTime(1999, 3, 18, 9, 0, 0),new DateTime(1999, 3, 25, 9, 0, 0)
            }, dates);


            //test 7
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Count = 13,
                ByMonth = new int[] { 6,7,8 },
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("TH") }
            };
            dates = r1.GetDates(new DateTime(1997, 6, 5, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
               new DateTime(1997, 6, 5, 9, 0, 0), new DateTime(1997, 6, 12, 9, 0, 0),new DateTime(1997, 6, 19, 9, 0, 0),new DateTime(1997, 6, 26, 9, 0, 0),
               new DateTime(1997, 7, 3, 9, 0, 0), new DateTime(1997, 7, 10, 9, 0, 0),new DateTime(1997, 7, 17, 9, 0, 0),new DateTime(1997, 7, 24, 9, 0, 0),new DateTime(1997, 7, 31, 9, 0, 0),
               new DateTime(1997, 8, 7, 9, 0, 0), new DateTime(1997, 8, 14, 9, 0, 0),new DateTime(1997, 8, 21, 9, 0, 0),new DateTime(1997, 8, 28, 9, 0, 0)
               
            }, dates);

            //test 8
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Interval = 4,
                Count = 3,
                ByMonth = new int[] { 11 },
                ByMonthDay = new int[] { 2,3,4,5,6,7,8 },
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("TU") }
            };
            dates = r1.GetDates(new DateTime(1996, 11, 5, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
               new DateTime(1996, 11, 5, 9, 0, 0), 
               new DateTime(2000, 11, 7, 9, 0, 0),
               new DateTime(2004, 11, 2, 9, 0, 0)
            }, dates);


            //friday 13
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Yearly,
                Until = new DateTime(2013,1,1),
                ByMonthDay = new int[] { 13 },
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("fr") }
            };
            dates = r1.GetDates(new DateTime(2012, 1, 1, 0, 0, 0), _fromDate, new DateTime(2014, 1, 1, 0, 0, 0));
           


        }

        [TestMethod]
        public void MonthlyRules()
        {
            //test 1
            var r1 = new RecurrenceRule()
            {
                Freq = Frequency.Monthly,
                Count = 10,
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("1FR") }
            };

            var dates = r1.GetDates(new DateTime(1997, 9, 5, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 5, 9, 0, 0), new DateTime(1997, 10, 3, 9, 0, 0),
            new DateTime(1997, 11, 7, 9, 0, 0),new DateTime(1997, 12, 5, 9, 0, 0),
            new DateTime(1998, 1, 2, 9, 0, 0),new DateTime(1998, 2, 6, 9, 0, 0),
            new DateTime(1998, 3, 6, 9, 0, 0),new DateTime(1998, 4, 3, 9, 0, 0),
            new DateTime(1998, 5, 1, 9, 0, 0), new DateTime(1998, 6, 5, 9, 0, 0)}, dates);

            //test 2
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Monthly,
                Until = new DateTime(1997, 12, 24),
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("1FR") }
            };

            dates = r1.GetDates(new DateTime(1997, 9, 5, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 5, 9, 0, 0), new DateTime(1997, 10, 3, 9, 0, 0),
            new DateTime(1997, 11, 7, 9, 0, 0),new DateTime(1997, 12, 5, 9, 0, 0)}, dates);


            //test 3
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Monthly,
                Count = 10,
                Interval = 2,
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("1SU"), RecurrenceRule.WeekDay.Parse("-1SU") }
            };

            dates = r1.GetDates(new DateTime(1997, 9, 7, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 7, 9, 0, 0), new DateTime(1997, 9, 28, 9, 0, 0),
            new DateTime(1997, 11, 2, 9, 0, 0),new DateTime(1997, 11, 30, 9, 0, 0),
            new DateTime(1998, 1, 4, 9, 0, 0),new DateTime(1998, 1, 25, 9, 0, 0),
            new DateTime(1998, 3, 1, 9, 0, 0),new DateTime(1998, 3, 29, 9, 0, 0),
            new DateTime(1998, 5, 3, 9, 0, 0),new DateTime(1998, 5, 31, 9, 0, 0)
            }, dates);


            //test 4
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Monthly,
                Count = 6,                
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("-2MO")}
            };

            dates = r1.GetDates(new DateTime(1997, 9, 22, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 22, 9, 0, 0), new DateTime(1997, 10, 20, 9, 0, 0),
            new DateTime(1997, 11, 17, 9, 0, 0),new DateTime(1997, 12, 22, 9, 0, 0),
            new DateTime(1998, 1, 19, 9, 0, 0),new DateTime(1998, 2, 16, 9, 0, 0)
            
            }, dates);

            //test 5
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Monthly,
                Interval = 18,
                Count = 10,
                ByMonthDay = new int[] {10,11,12,13,14,15 }
            };

            dates = r1.GetDates(new DateTime(1997, 9, 10, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 10, 9, 0, 0), new DateTime(1997, 9, 11, 9, 0, 0),
            new DateTime(1997, 9, 12, 9, 0, 0),new DateTime(1997, 9, 13, 9, 0, 0),
            new DateTime(1997, 9, 14, 9, 0, 0),new DateTime(1997, 9, 15, 9, 0, 0),
            new DateTime(1999, 3, 10, 9, 0, 0),new DateTime(1999, 3, 11, 9, 0, 0),
            new DateTime(1999, 3, 12, 9, 0, 0),new DateTime(1999, 3, 13, 9, 0, 0)
            
            }, dates);

        }

        [TestMethod]
        public void WeeklyRules()
        {
            //test 1
            var r1 = new RecurrenceRule()
            {
                Freq = Frequency.Weekly,
                Count = 10                
            };

            var dates = r1.GetDates(new DateTime(1997, 9, 2, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 2, 9, 0, 0), new DateTime(1997, 9, 9, 9, 0, 0),
            new DateTime(1997, 9, 16, 9, 0, 0),new DateTime(1997, 9, 23, 9, 0, 0),
            new DateTime(1997, 9, 30, 9, 0, 0),new DateTime(1997, 10, 7, 9, 0, 0),
            new DateTime(1997, 10, 14, 9, 0, 0),new DateTime(1997, 10, 21, 9, 0, 0),
            new DateTime(1997, 10, 28, 9, 0, 0), new DateTime(1997, 11, 4, 9, 0, 0)}, dates);


            //test 2
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Weekly,
                Interval = 2,
                WKST = RecurrenceRule.WeekDay.Parse("su"),
                Count = 10
            };

            dates = r1.GetDates(new DateTime(1997, 9, 2, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 2, 9, 0, 0), new DateTime(1997, 9, 16, 9, 0, 0),
            new DateTime(1997, 9, 30, 9, 0, 0),new DateTime(1997, 10, 14, 9, 0, 0),
            new DateTime(1997, 10, 28, 9, 0, 0),new DateTime(1997, 11, 11, 9, 0, 0),
            new DateTime(1997, 11, 25, 9, 0, 0),new DateTime(1997, 12, 9, 9, 0, 0),
            new DateTime(1997, 12, 23, 9, 0, 0), new DateTime(1998, 1, 6, 9, 0, 0)}, dates);

            //test 3
            r1 = new RecurrenceRule()
            {
                Freq = Frequency.Weekly,
                Interval = 2,
                WKST = RecurrenceRule.WeekDay.Parse("su"),
                Count = 8,
                ByDay = new RecurrenceRule.WeekDay[] { RecurrenceRule.WeekDay.Parse("tu"), RecurrenceRule.WeekDay.Parse("th") }
            };

            dates = r1.GetDates(new DateTime(1997, 9, 2, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 2, 9, 0, 0), new DateTime(1997, 9, 4, 9, 0, 0),
            new DateTime(1997, 9, 16, 9, 0, 0),new DateTime(1997, 9, 18, 9, 0, 0),
            new DateTime(1997, 9, 30, 9, 0, 0),new DateTime(1997, 10, 2, 9, 0, 0),
            new DateTime(1997, 10, 14, 9, 0, 0),new DateTime(1997, 10, 16, 9, 0, 0)}, dates);


            r1 = RecurrenceRule.Parse("freq=weekly;count=3;interval=2;byday=mo");
            dates = r1.GetDates(new DateTime(2012, 1, 2, 0, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(2012, 1, 2, 0, 0, 0), new DateTime(2012, 1, 16, 0, 0, 0),
            new DateTime(2012, 1, 30, 0, 0, 0)}, dates);



        }


        [TestMethod]
        public void DailyRules()
        {
            //test 1
            var r1 = new RecurrenceRule()
            {
                Freq = Frequency.Daily,
                Count = 5,
                Interval = 10
            };

            var dates = r1.GetDates(new DateTime(1997, 9, 2, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 2, 9, 0, 0), new DateTime(1997, 9, 12, 9, 0, 0),
            new DateTime(1997, 9, 22, 9, 0, 0),new DateTime(1997, 10, 2, 9, 0, 0),
            new DateTime(1997, 10, 12, 9, 0, 0)}, dates);

        }

        [TestMethod]
        public void HorlyRules()
        {
            //test 1
            var r1 = new RecurrenceRule()
            {
                Freq = Frequency.Hourly,
                Until = new DateTime(1997,9,2,17,0,0),
                Interval = 3
            };

            var dates = r1.GetDates(new DateTime(1997, 9, 2, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 2, 9, 0, 0), new DateTime(1997, 9, 2, 12, 0, 0),            
            new DateTime(1997, 9, 2, 15, 0, 0)}, dates);
        }

        [TestMethod]
        public void MinutelyRules()
        {
            //test 1
            var r1 = new RecurrenceRule()
            {
                Freq = Frequency.Minutely,
                ByHour = new int[]{9,10,11,12,13,14,15,16},
                Count = 5,
                Interval = 20
            };

            var dates = r1.GetDates(new DateTime(1997, 9, 2, 9, 0, 0), _fromDate, _toDate);

            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 2, 9, 0, 0), new DateTime(1997, 9, 2, 9, 20, 0),            
            new DateTime(1997, 9, 2, 9, 40, 0),new DateTime(1997, 9, 2, 10, 0, 0),new DateTime(1997, 9, 2, 10, 20, 0)}, dates);
        }

        [TestMethod]
        public void BySetPosRules()
        {
            //test 1
            var r1 = RecurrenceRule.Parse("FREQ=MONTHLY;BYDAY=MO,TU,WE,TH,FR;BYSETPOS=-2;count=7");
            var dates = r1.GetDates(new DateTime(1997, 9, 29, 9, 0, 0), _fromDate, _toDate);
            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 29, 9, 0, 0), new DateTime(1997, 10, 30, 9, 0, 0),            
            new DateTime(1997, 11, 27, 9, 0, 0),new DateTime(1997, 12, 30, 9, 0, 0),new DateTime(1998, 1, 29, 9, 0, 0),
            new DateTime(1998, 2, 26, 9, 0, 0),new DateTime(1998, 3, 30, 9, 0, 0)}, dates);


            //test 2
            r1 = RecurrenceRule.Parse("FREQ=MONTHLY;COUNT=3;BYDAY=TU,WE,TH;BYSETPOS=3");
            dates = r1.GetDates(new DateTime(1997, 9, 4, 9, 0, 0), _fromDate, _toDate);
            CollectionAssert.AreEqual(new List<DateTime>(){
            new DateTime(1997, 9, 4, 9, 0, 0), new DateTime(1997, 10, 7, 9, 0, 0),            
            new DateTime(1997, 11, 6, 9, 0, 0)}, dates);
        }

        [TestMethod]
        public void ParseFromStringTest()
        {
            //tets
            var r1 = RecurrenceRule.Parse("FREQ=WEEKLY;INTERVAL=2;UNTIL=19971224T000000Z;WKST=SU");
            Assert.AreEqual( new DateTime(1997, 12, 24, 0, 0, 0),r1.Until);
        }


    }
}
#endif
