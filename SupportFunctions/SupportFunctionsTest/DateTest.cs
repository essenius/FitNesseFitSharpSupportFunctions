// Copyright 2015-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class DateTest
    {
        [TestMethod, TestCategory("Unit")]
        public void DateAddDaysToTest()
        {
            Date.ResetDefaultFormat();
            var date1 = Date.Parse("23-Feb-2016 16:10");
            var date2 = date1.AddDays(-365);
            Assert.AreEqual("2015-02-23T16:10:00", date2.ToString());
        }

        [TestMethod, TestCategory("Unit")]
        public void DateAddHoursToTest()
        {
            Date.ResetDefaultFormat();
            var date1 = Date.Parse("23-Feb-2016 16:10");
            var date2 = date1.AddHours(2.5);
            Assert.AreEqual("2016-02-23T18:40:00", date2.ToString());
        }

        [TestMethod, TestCategory("Unit")]
        public void DateFormattedTest()
        {
            var date1 = Date.Parse("23-Feb-2016 16:10");
            Assert.AreEqual(date1.Formatted("yyyy-MM-dd"), "2016-02-23");
        }

        [TestMethod, TestCategory("Unit")]
        public void DateLocalFormatTest()
        {
            var date = new Date("1-1-2017 1:02:03pm");
            Assert.AreEqual("01-Jan-2017 13:02:03", date.ToLocalFormat);
        }

        [TestMethod, TestCategory("Unit")]
        public void DateParseNowTest()
        {
            var nowBefore = UniqueDateTime.NowTicks;
            var now = Date.Parse("Now").Ticks;
            var nowAfter = UniqueDateTime.NowTicks;
            Assert.IsTrue(now > nowBefore);
            Assert.IsTrue(nowAfter > now);
        }

        [TestMethod, TestCategory("Unit")]
        public void DateParseStringTest()
        {
            var date1 = Date.Parse("23-Feb-2016 16:10").Ticks;
            var date2 = new DateTime(2016, 2, 23, 16, 10, 0).Ticks;
            Assert.AreEqual(date1, date2);
        }

        [TestMethod, TestCategory("Unit")]
        public void DateParseTicksTest()
        {
            var now1 = DateTime.Today.Ticks;
            var now = Date.Parse(now1.To<string>()).Ticks;
            Assert.AreEqual(now1, now);
        }

        [TestMethod, TestCategory("Unit")]
        public void DateParseTodayTest()
        {
            var now1 = DateTime.Today.Ticks;
            var now = Date.Parse("Today").Ticks;
            Assert.AreEqual(now1, now);
        }

        [TestMethod, TestCategory("Unit")]
        public void DateParseUtcNowTest()
        {
            // check whether the UtcNow ticks are unique and increasing
            var nowBefore = UniqueDateTime.UtcNowTicks;
            var now = Date.Parse("UtcNow").Ticks;
            var nowAfter = UniqueDateTime.UtcNowTicks;
            Assert.IsTrue(now > nowBefore);
            Assert.IsTrue(nowAfter > now);

            // now check if the UTC ticks gives the right value - i.e. the right offset with the local time ticks
            var ticksDifference = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Ticks;
            var ticksRealDifference = UniqueDateTime.NowTicks - UniqueDateTime.UtcNowTicks;
            Assert.IsTrue(Math.Abs(ticksDifference - ticksRealDifference) < 10000);
        }

        [TestMethod, TestCategory("Unit")]
        public void DateParseUtcTodayTest()
        {
            var now1 = DateTime.UtcNow.Date.Ticks;
            var now = Date.Parse("UtcToday").Ticks;
            Assert.AreEqual(now1, now);
        }

        [TestMethod, TestCategory("Unit")]
        public void DateParseWithFormatTest()
        {
            var ticksExpected = new DateTime(2018, 1, 11, 0, 0, 0).Ticks;
            Assert.AreEqual(ticksExpected, Date.ParseFormatted("11/1/2018", "d/M/yyyy").Ticks);
            Assert.AreEqual(ticksExpected, Date.ParseFormatted("1/11/2018", "M/d/yyyy").Ticks);
            Assert.AreEqual(ticksExpected, Date.ParseFormatted("11-01-2018", "dd-MM-yyyy").Ticks);
            Assert.AreEqual(ticksExpected, Date.ParseFormatted("01-11-2018", "MM-dd-yyyy").Ticks);
            Assert.AreEqual(ticksExpected, Date.ParseFormatted("11-Jan-18", "dd-MMM-yy").Ticks);

            var ticksExpected1 = new DateTime(2018, 1, 11, 13, 14, 15).Ticks;
            Assert.AreEqual(ticksExpected1, Date.ParseFormatted("11/1/2018 1:14:15 pm", "d/M/yyyy h:mm:ss tt").Ticks);
            Assert.AreEqual(ticksExpected1, Date.ParseFormatted("11-Jan-18 13:14:15", "dd-MMM-yy HH:mm:ss").Ticks);
        }

        [TestMethod, TestCategory("Integration")]
        public void DateShortDateFormatTest()
        {
            var format = Date.ShortDateFormat;
            Console.WriteLine("Short date format: " + format);
            Assert.IsTrue(format.Contains("d"), "days found");
            Assert.IsTrue(format.Contains("yy"), "years found");
        }

        [TestMethod, TestCategory("Integration")]

        public void DateTimeFormatTest()
        {
            var format = Date.TimeFormat;
            Console.WriteLine("Time format: " + format);
            Assert.IsTrue(format.Contains("m"), "minutes found");
            Assert.IsTrue(format.Contains("h") || format.Contains("H"), "hours found");
        }

        [TestMethod, TestCategory("Unit")]
        public void DateToStringTest()
        {
            Date.ResetDefaultFormat();
            var date1 = Date.Parse("23-Feb-2016 16:10");
            Assert.AreEqual("2016-02-23T16:10:00", date1.ToString());
            Date.DefaultFormat = "MM/dd/yyyy";
            Assert.AreEqual("02/23/2016", date1.ToString());
        }
    }
}
