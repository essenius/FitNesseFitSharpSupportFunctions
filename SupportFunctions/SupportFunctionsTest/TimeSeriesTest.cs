// Copyright 2017-2024 Rik Essenius
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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class TimeSeriesTest
    {
        private static TimeSeries CreateTimeSeriesViaDecisionTableInterface(Date timestamp, string value)
        {
            var timeSeries = new TimeSeries(null);
            timeSeries.BeginTable();
            timeSeries.Timestamp = timestamp;
            timeSeries.Value = value;
            timeSeries.Reset(); // sets IsGood to true
            timeSeries.Execute();
            return timeSeries;
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("test1.csv", "test1.csv", "timestamp", "value", @"isgood", "TimeSeriesDefault")]
        [DataRow(@"test2.csv#tijd#waarde#goed", "test2.csv", @"tijd", @"waarde", @"goed", "TimeSeriesCustom")]
        [DataRow(@"test3.csv##waarde", "test3.csv", "timestamp", @"waarde", @"isgood", "TimeSeriesPartlyCustom")]
        public void TimeSeriesConstructorTest(string spec, string path, string timestampHeader, string valueHeader, string isGoodHeader,
            string testCase)
        {
            var timeSeries = new TimeSeries(spec);
            Assert.AreEqual(path, timeSeries.Path, testCase);
            Assert.AreEqual(timestampHeader, timeSeries.TimestampColumn, testCase);
            Assert.AreEqual(valueHeader, timeSeries.ValueColumn, testCase);
            Assert.AreEqual(isGoodHeader, timeSeries.IsGoodColumn, testCase);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("0-100", 10, "0", "100", 100)]
        [DataRow("-50-100", 12, "-50", "100", 150)]
        [DataRow("-1e10-1e10", 14, "-1e10", "1e10", 2e10)]
        [DataRow("-0.2-1.6", 16, "-0.2", "1.6", 1.8)]
        [DataRow("empty", 0, "", "", 0)]
        [DataRow("bool", 2, "false", "true", 0)]
        [DataRow("string", 2, "hi1", "hi2", 0)]
        public void TimeSeriesDataRangeTest(string testCase, int measurementCount, string lowValue, string highValue, double expectedRange)
        {
            var timeSeries = new TimeSeries();

            for (var i = 0; i < measurementCount; i++)
            {
                var measurement = new Measurement
                {
                    Value = i < measurementCount / 2 ? lowValue : highValue,
                    Timestamp = DateTime.Now,
                    IsGood = true
                };
                timeSeries.AddMeasurement(measurement);
            }

            var metadata = new TimeSeriesMetadata<Measurement>(timeSeries.Measurements, p => p.Value);
            Assert.AreEqual(expectedRange, metadata.Range, $"Test {testCase}");
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("Double-Int-50", 50, "12.5", "13", "System.Double")]
        [DataRow("Double-Bool-10", 10, "12.5", "true", "System.String")]
        [DataRow("Double-Long-4", 4, "1e10", "2147483648", "System.Double")]
        [DataRow("Double-String-3", 3, "12.5", "Hi1", "System.String")]
        [DataRow("Double-Double-7", 7, "12.5", "13.2", "System.Double")]
        [DataRow("Int-Long-25", 25, "12", "2147483648", "System.Int64")]
        [DataRow("Int-Double-12", 12, "12", "12.1", "System.Double")]
        [DataRow("Int-Bool-2", 2, "12", "True", "System.String")]
        [DataRow("Int-String-5", 5, "12", "Hi1", "System.String")]
        [DataRow("Int-Int-6", 6, "12", "13", "System.Int32")]
        [DataRow("Long-Int-8", 8, "2147483648", "12", "System.Int64")]
        [DataRow("Long-Long-9000", 9000, "2147483648", "2147483648", "System.Int64")]
        [DataRow("Long-Double-11", 11, "2147483648", "12.5", "System.Double")]
        [DataRow("Long-Bool-13", 13, "2147483648", "False", "System.String")]
        [DataRow("Long-String-14", 14, "2147483648", "Hi1", "System.String")]
        [DataRow("Bool-String-15", 15, "False", "Hi1", "System.String")]
        [DataRow("Bool-Bool-16", 16, "False", "True", "System.Boolean")]
        [DataRow("Bool-Double-17", 17, "False", "12.5", "System.String")]
        [DataRow("String-Double-18", 18, "Hi1", "1e10", "System.String")]
        [DataRow("Double-Inf-NaN-2", 2, "Infinity", "NaN", "System.Double")]
        [DataRow("Double-InfSym-NaN-2", 19, "∞", "-∞", "System.Double")]
        public void TimeSeriesDataTypeTest(string testCase, int measurementCount, string value1, string value2, string expectedType)
        {
            var timeSeries = new TimeSeries();

            for (var i = 0; i < measurementCount; i++)
            {
                var measurement = new Measurement
                {
                    Value = i < measurementCount / 2 ? value1 : value2,
                    Timestamp = DateTime.Now,
                    IsGood = true
                };
                timeSeries.AddMeasurement(measurement);
            }

            var metadata = new TimeSeriesMetadata<Measurement>(timeSeries.Measurements, p => p.Value);
            Assert.AreEqual(expectedType, metadata.DataType.ToString(), $"Test {testCase}");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TimeSeriesDecisionTableTest()
        {
            // emulate a FitNesse decision table with one record

            const string timestampString = "1965-02-23T16:10:00.5000000";
            const string value = "123.4";
            var timestamp = Date.Parse(timestampString);
            var timeseries = CreateTimeSeriesViaDecisionTableInterface(timestamp, value);
            Assert.AreEqual(1, timeseries.Measurements.Count, "Count == 1");
            Assert.AreEqual(timestamp.DateTime, timeseries.Measurements[0].Timestamp, "timestamp OK");
            Assert.AreEqual(value, timeseries.Measurements[0].Value, "Value OK");
            Assert.IsTrue(timeseries.Measurements[0].IsGood, "IsGood OK");
        }

        [TestMethod]
        [TestCategory("Integration")]
        [DeploymentItem("TestData\\Dbl02_0.1Tol_NoFixed_Application_Variables_OUTFLOW1.SP_ExpectedRaw.csv")]
        [DeploymentItem("TestData\\1stOrderExpected.csv")]
        public void TimeSeriesLoadTest()
        {
            const string file = "Dbl02_0.1Tol_NoFixed_Application_Variables_OUTFLOW1.SP_ExpectedRaw.csv";
            var timeSeries = TimeSeries.Parse(file);
            timeSeries.Load();
            Assert.AreEqual(127, timeSeries.Measurements.Count);
            Assert.AreEqual("50", timeSeries.Measurements[0].Value, "First row value 1");
            Assert.AreEqual("49.96429546", timeSeries.Measurements[126].Value, "Last row value 1");

            const string file2 = @"1stOrderExpected.csv#tijd#waarde#kwaliteit";
            var timeSeries2 = TimeSeries.Parse(file2);
            timeSeries2.Load();
            Assert.AreEqual(34, timeSeries2.Measurements.Count);
            Assert.AreEqual("0", timeSeries2.Measurements[0].Value, "First row value 2");
            Assert.AreEqual("0.999999887", timeSeries2.Measurements[32].Value, "One but last row value 2");
            Assert.AreEqual(double.NaN, timeSeries2.Measurements[33].Value.To<double>(), "Last row value 2");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void TimeSeriesSaveTest()
        {
            const string timestampString = "1965-02-23T16:10:00.5000000";
            const string value = "Hi1,Hi2";
            var timestamp = Date.Parse(timestampString);
            var timeSeries = CreateTimeSeriesViaDecisionTableInterface(timestamp, value);
            var tempFileName = Path.GetTempFileName();
            timeSeries.SaveTo(tempFileName);
            var timeSeries2 = TimeSeries.Parse(tempFileName);
            timeSeries2.Load();
            Assert.AreEqual(1, timeSeries2.Measurements.Count);
            Assert.AreEqual(timeSeries2.Measurements[0].Timestamp, timestamp.DateTime);
            Assert.AreEqual(timeSeries2.Measurements[0].Value, value);
            Assert.AreEqual(timeSeries2.Measurements[0].IsGood, true);
            File.Delete(tempFileName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TimeSeriesToStringTest()
        {
            var timeSeries = new TimeSeries();
            Assert.AreEqual("TimeSeries", timeSeries.ToString());
            Assert.IsNull(timeSeries.FileName);
            timeSeries.Path = "Path.csv#1#2#3#";
            Assert.AreEqual("Path.csv", timeSeries.ToString());
        }
    }
}
