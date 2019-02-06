// Copyright 2017-2019 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("Unit")]
        public void TimeSeriesConstructorTest()
        {
            var timeSeriesDefault = new TimeSeries("test1.csv");
            Assert.AreEqual("test1.csv", timeSeriesDefault.Path, "timeSeriesDefault");
            Assert.AreEqual("timestamp", timeSeriesDefault.TimestampColumn, "timeSeriesDefault");
            Assert.AreEqual("value", timeSeriesDefault.ValueColumn, "timeSeriesDefault");
            Assert.AreEqual(@"isgood", timeSeriesDefault.IsGoodColumn, "timeSeriesDefault");

            var timeSeriesCustom = new TimeSeries("test2.csv#tijd#waarde#goed");
            Assert.AreEqual("test2.csv", timeSeriesCustom.Path, "timeSeriesCustom");
            Assert.AreEqual(@"tijd", timeSeriesCustom.TimestampColumn, "timeSeriesCustom");
            Assert.AreEqual(@"waarde", timeSeriesCustom.ValueColumn, "timeSeriesCustom");
            Assert.AreEqual(@"goed", timeSeriesCustom.IsGoodColumn, "timeSeriesCustom");

            var timeSeriesPartlyCustom = new TimeSeries("test2.csv##waarde");
            Assert.AreEqual("test2.csv", timeSeriesPartlyCustom.Path, "timeSeriesCustom");
            Assert.AreEqual("timestamp", timeSeriesPartlyCustom.TimestampColumn, "timeSeriesCustom");
            Assert.AreEqual(@"waarde", timeSeriesPartlyCustom.ValueColumn, "timeSeriesCustom");
            Assert.AreEqual(@"isgood", timeSeriesPartlyCustom.IsGoodColumn, "timeSeriesCustom");
        }

        [TestMethod, TestCategory("Unit"), DeploymentItem("SupportFunctionsTest\\TestData.xml"),
         DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "TimeSeriesDataRange", DataAccessMethod.Sequential)]
        public void TimeSeriesDataRangeTest()
        {
            var timeSeries = new TimeSeries();

            var measurementCount = Convert.ToInt32(TestContext.DataRow["count"]);
            for (var i = 0; i < measurementCount; i++)
            {
                var measurement = new Measurement
                {
                    Value = TestContext.DataRow[i < measurementCount / 2 ? "lowValue" : "highValue"].ToString(),
                    Timestamp = DateTime.Now,
                    IsGood = true
                };
                timeSeries.AddMeasurement(measurement);
            }
            var metadata = new TimeSeriesMetadata<Measurement>(timeSeries.Measurements, p => p.Value);
            Assert.AreEqual(Convert.ToDouble(TestContext.DataRow["expectedRange"].ToString()),
                metadata.Range, $"Test {TestContext.DataRow["testCase"]}");
        }

        [TestMethod, TestCategory("Unit"), DeploymentItem("SupportFunctionsTest\\TestData.xml"),
         DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "TimeSeriesDataType", DataAccessMethod.Sequential)]
        public void TimeSeriesDataTypeTest()
        {
            var timeSeries = new TimeSeries();

            var measurementCount = Convert.ToInt32(TestContext.DataRow["count"]);
            for (var i = 0; i < measurementCount; i++)
            {
                var measurement = new Measurement
                {
                    Value = TestContext.DataRow[i < measurementCount / 2 ? "value1" : "value2"].ToString(),
                    Timestamp = DateTime.Now,
                    IsGood = true
                };
                timeSeries.AddMeasurement(measurement);
            }
            var metadata = new TimeSeriesMetadata<Measurement>(timeSeries.Measurements, p => p.Value);
            Assert.AreEqual(TestContext.DataRow["expectedType"].ToString(), metadata.DataType.ToString(),
                $"Test {TestContext.DataRow["testCase"]}");
        }

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Integration"),
         DeploymentItem("TestData\\Dbl02_0.1Tol_NoFixed_Application_Variables_OUTFLOW1.SP_ExpectedRaw.csv"),
         DeploymentItem("TestData\\1stOrderExpected.csv")]
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

        [TestMethod, TestCategory("Integration")]
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

        [TestMethod, TestCategory("Unit")]
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