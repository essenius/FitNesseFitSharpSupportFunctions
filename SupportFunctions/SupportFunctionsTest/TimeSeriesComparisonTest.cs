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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class TimeSeriesComparisonTest
    {
        private static void AddIfExistInTestSpec(IDictionary<string, string> result, DataRow entry, string key)
        {
            var value = ColumnWithDefault(entry, key, null)?.ToString();
            // value returns empty string if not specified, so we need to check for that too.
            if (!string.IsNullOrEmpty(value))
            {
                result.Add(key, value);
            }
        }

        private static void AssertIssue(string expectedIssue, IReadOnlyList<object> row)
        {
            Assert.IsNotNull(row);
            var issue = row[5] as Collection<string>;
            Assert.IsNotNull(issue);
            Assert.AreEqual(expectedIssue, issue[1]);
        }

        private static object ColumnWithDefault(DataRow row, string columnName, object defaultValue)
        {
            return row.Table.Columns.Cast<DataColumn>().Any(column => column.ColumnName == columnName)
                ? row[columnName]
                : defaultValue;
        }

        private static Measurement CreateMeasurementFrom(DataRow row, object defaultValue, object defaultIsGood,
            string valueField, string isGoodField)
        {
            var value = ValueWithDefault(row[valueField], defaultValue);
            var isGood = ValueWithDefault(row[isGoodField], defaultIsGood);
            //($"Creating {valueField}: {row["timestamp"]}, {value}, {isGood}").Log();
            return new Measurement(row["timestamp"], value, isGood);
        }

        private static string GetTimestamp(string cell)
        {
            // remove the pass/fail indicator by keeping everything after the first colon
            var timestamp = cell.Split(new[] {':'}, 2)[1];
            if (string.IsNullOrEmpty(timestamp))
            {
                return null;
            }
            if (timestamp.StartsWith("["))
            {
                timestamp = timestamp.Substring(1, timestamp.IndexOf("]", StringComparison.Ordinal) - 1);
            }
            return timestamp;
        }

        private static object ValueWithDefault(object value, object defaultValue) =>
            string.IsNullOrEmpty(value.ToString()) ? defaultValue : value;

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "False positive")]
        public TestContext TestContext { get; set; }

        private void ArrangeDataForDoTable(IEnumerable<DataRow> data, TimeSeries expectedSeries, TimeSeries actualSeries,
            IDictionary<string, Dictionary<string, string>> expectedResults)
        {
            var defaultValue = ColumnWithDefault(TestContext.DataRow, "value", null);
            var defaultIsGood = ColumnWithDefault(TestContext.DataRow, "isGood", null);
            foreach (var entry in data)
            {
                var exists = ColumnWithDefault(entry, "exists", "both").ToString();

                // if exists == expected, we don't want an actual record
                if (exists != "expected")
                {
                    actualSeries.AddMeasurement(
                        CreateMeasurementFrom(entry, defaultValue, defaultIsGood, "actualValue", "actualIsGood"));
                }
                // if exists == actual, we don't want an expected record
                if (exists != "actual")
                {
                    expectedSeries.AddMeasurement(
                        CreateMeasurementFrom(entry, defaultValue, defaultIsGood, "expectedValue", "expectedIsGood"));
                }
                var result = new Dictionary<string, string>();
                AddIfExistInTestSpec(result, entry, "timeStampOut");
                AddIfExistInTestSpec(result, entry, "value");
                AddIfExistInTestSpec(result, entry, "delta");
                AddIfExistInTestSpec(result, entry, "deltaPercentage");
                AddIfExistInTestSpec(result, entry, "isGood");

                expectedResults.Add(entry["timestamp"].ToString(), result);
            }
        }

        [TestMethod, TestCategory("Unit"), ExpectedExceptionWithMessage(typeof(ArgumentException),
             "Duplicate timestamp in expected: 2004-03-24T00:00:00.0000000")]
        public void TimeSeriesComparisonDoTableExceptionTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("2004-03-24");
            expected.AddMeasurement(new Measurement {Timestamp = timestamp});
            expected.AddMeasurement(new Measurement {Timestamp = timestamp});
            var actual = new TimeSeries();
            var timeSeriesComparison = new TimeSeriesComparison(expected, actual);
            timeSeriesComparison.DoTable(null);
        }

        [TestMethod, TestCategory("Unit"), DeploymentItem("TestData.xml"),
         DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\TestData.xml",
             "TimeSeriesComparisonDoTable", DataAccessMethod.Sequential)]
        public void TimeSeriesComparisonDoTableTest()
        {
            var testCase = TestContext.DataRow["testcase"].ToString();
            var actualSeries = new TimeSeries();
            var expectedSeries = new TimeSeries();
            var expectedResults = new Dictionary<string, Dictionary<string, string>>();
            var data = TestContext.DataRow.GetChildRows("TimeSeriesComparisonDoTable_data");
            ArrangeDataForDoTable(data, expectedSeries, actualSeries, expectedResults);

            var toleranceString = ColumnWithDefault(TestContext.DataRow, "tolerance", null)?.ToString();
            var timeSeriesComparison = string.IsNullOrEmpty(toleranceString)
                ? new TimeSeriesComparison(expectedSeries, actualSeries)
                : new TimeSeriesComparison(expectedSeries, actualSeries, Tolerance.Parse(toleranceString));
            var displayTolerance = ColumnWithDefault(TestContext.DataRow, "displayTolerance", string.Empty)?.ToString();
            var result = timeSeriesComparison.DoTable(null);
            const string fail = "fail:";

            Assert.IsTrue(result.Count > 0, $"Result count = 0 for {testCase}");
            var header = result[0] as Collection<object>;
            Assert.IsNotNull(header, $"header null for {testCase}");
            Assert.AreEqual("report:Timestamp", header[0]);
            Assert.AreEqual("report:Value", header[1], $"Value Header for {testCase}"); //+ displayTolerance
            Assert.AreEqual("report:Delta", header[2], $"Delta Header for {testCase}");
            Assert.AreEqual("report:Delta %", header[3], $"Delta % Header for {testCase}");
            Assert.AreEqual("report:Is Good", header[4], $"Is Good Header for {testCase}");

            // check the data rows only - we already did the header
            foreach (Collection<object> row in result.Skip(1))
            {
                var timestamp = GetTimestamp(row[0].ToString());
                Assert.IsFalse(string.IsNullOrEmpty(timestamp),
                    $"Timestamp not found in actual result [{row[0]}] for {testCase}");
                Assert.IsTrue(expectedResults.ContainsKey(timestamp),
                    $"Timestamp [{timestamp}] not found in expected result for {testCase}");
                var expectedResult = expectedResults[timestamp];
                var actualResult = new Dictionary<string, object>
                {
                    {"pass", (!row.Any(cell => cell.ToString().StartsWith(fail))).ToString()},
                    {"timestampOut", row[0]},
                    {"value", row[1]},
                    {"delta", row[2]},
                    {"deltaPercentage", row[3]},
                    {"isGood", row[4]}
                };

                // only test the ones we have specified in the test data
                foreach (var entry in expectedResult)
                {
                    Assert.AreEqual(entry.Value, actualResult[entry.Key], $"{entry.Key}@{testCase}[{timestamp}]");
                }
            }
            Assert.AreEqual(TestContext.DataRow["failures"].To<long>(), timeSeriesComparison.FailureCount, $"FailureCount {testCase}");
            Assert.AreEqual(TestContext.DataRow["datapoints"].To<long>(), timeSeriesComparison.PointCount, $"PointCount {testCase}");
            Assert.AreEqual(TestContext.DataRow["usedTolerance"].ToString(), timeSeriesComparison.UsedTolerance,
                $"UsedTolerance {testCase}");
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeSeriesComparisonFailureCountTest()
        {
            var comparison = new TimeSeriesComparison(null, null);
            Assert.AreEqual(0, comparison.FailureCount);
        }

        [TestMethod, TestCategory("Integration"), DeploymentItem("TestData\\Base64ResultMissingExpected.html")]
        public void TimeSeriesComparisonGraphEmptyExpectedNumericalTest()
        {
            var expected = new TimeSeries();
            var actual = new TimeSeries();
            var timestamp = DateTime.Parse("2000-11-24");
            actual.AddMeasurement(new Measurement
            {
                Timestamp = timestamp,
                Value = "0"
            });
            var timeseriesComparison = new TimeSeriesComparison(expected, actual);
            var parameters = new Dictionary<string, string>
            {
                {"Width", "320"},
                {@"heIght", "200"}
            };
            var result = timeseriesComparison.Graph(parameters);
            var expectedResult = File.ReadAllText("Base64ResultMissingExpected.html");
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeSeriesComparisonGraphNoDataTest()
        {
            var comparison = new TimeSeriesComparison(null, null);
            var result = comparison.Graph();
            Assert.AreEqual(result, "no data");
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeSeriesComparisonGraphNoNumericalDataTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("2000-11-24");
            expected.AddMeasurement(new Measurement
            {
                Timestamp = timestamp,
                Value = "hi"
            });
            var timeseriesComparison = new TimeSeriesComparison(expected, expected);
            var result = timeseriesComparison.Graph();
            Assert.AreEqual(result, "no numeric comparison");
            Assert.AreEqual(timestamp, timeseriesComparison.BaseTimestamp.DateTime);
            Assert.IsTrue(timeseriesComparison.TimeSpanSeconds.IsZero());
        }

        [TestMethod, TestCategory("Integration"), DeploymentItem("TestData\\Base64AllBelowXAxis.html")]
        public void TimeSeriesComparisonGraphNumericalAllBelowXAxisTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("1969-08-28");
            expected.AddMeasurement(new Measurement
            {
                Timestamp = timestamp,
                Value = "-1.2"
            });
            expected.AddMeasurement(new Measurement
            {
                Timestamp = timestamp.Date.AddSeconds(1),
                Value = "-2.3"
            });
            var actual = new TimeSeries();
            actual.AddMeasurement(new Measurement
            {
                Timestamp = timestamp,
                Value = "-1.2"
            });
            actual.AddMeasurement(new Measurement
            {
                Timestamp = timestamp.Date.AddSeconds(2),
                Value = "-2.6"
            });
            var timeseriesComparison = new TimeSeriesComparison(expected, actual);
            var result = timeseriesComparison.GraphX(640, 480);
            var expectedResult = File.ReadAllText("Base64AllBelowXAxis.html");
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod, TestCategory("Integration"), DeploymentItem("TestData\\Base64ResultWithMissingValues.html")]
        public void TimeSeriesComparisonGraphNumericalWithMissingValuesTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("2000-11-24");
            expected.AddMeasurement(new Measurement
            {
                Timestamp = timestamp,
                Value = "1"
            });
            expected.AddMeasurement(new Measurement
            {
                Timestamp = timestamp.Date.AddSeconds(1),
                Value = "2"
            });
            var actual = new TimeSeries();
            actual.AddMeasurement(new Measurement
            {
                Timestamp = timestamp,
                Value = "1"
            });
            actual.AddMeasurement(new Measurement
            {
                Timestamp = timestamp.Date.AddSeconds(2),
                Value = "-1"
            });
            var timeSeriesComparison = new TimeSeriesComparison(expected, actual);
            var result = timeSeriesComparison.GraphX(640, 480);
            var expectedResult = File.ReadAllText("Base64ResultWithMissingValues.html");
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod, TestCategory("Unit"), DeploymentItem("TestData\\Base64SecondOrderResponseLimitedY.html"),
         DeploymentItem("TestData\\Base64SecondOrderResponse.html")]
        public void TimeSeriesComparisonGraphSecondOrderResponseTest()
        {
            const double zetaExpected = 0.205;
            const double zetaActual = 0.2;
            const double omega = 1;
            const double theta = Math.PI / 2;
            var timestampBase = DateTime.Today;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            var expected = new TimeSeries();
            var actual = new TimeSeries();

            for (double time = 0; time <= 31; time += 0.5)
            {
                var timestamp = timestampBase.AddSeconds(time);
                var expectedValue = TimeSeriesChartTest.SecondOrderResponse(time, zetaExpected, omega, theta);
                var actualValue = TimeSeriesChartTest.SecondOrderResponse(time, zetaActual, omega, theta);
                if (actualValue > maxY)
                {
                    maxY = actualValue;
                }
                if (actualValue < minY)
                {
                    minY = actualValue;
                }

                var okValue = time < 12 || time > 16 && time < 25 || time > 25.5;

                actual.AddMeasurement(new Measurement
                {
                    Timestamp = timestamp,
                    Value = actualValue.To<string>(),
                    IsGood = okValue
                });
                expected.AddMeasurement(new Measurement
                {
                    Timestamp = timestamp,
                    Value = expectedValue.To<string>(),
                    IsGood = true
                });
            }

            var parameters = new Dictionary<string, string>
            {
                {"Min Value", "0.7"},
                {"Max Value", "1.7"},
                {"start timestamp", timestampBase.ToRoundTripFormat()},
                {"end timestamp", timestampBase.AddSeconds(30).ToRoundTripFormat()}
            };
            var comparison = new TimeSeriesComparison(expected, actual, Tolerance.Parse("1.0"));
            var result = comparison.Graph(parameters);
            var expectedResult = File.ReadAllText("Base64SecondOrderResponseLimitedY.html");
            Assert.AreEqual(expectedResult, result);
            parameters.Remove("Min Value");
            parameters.Remove("Max Value");
            var result2 = new TimeSeriesComparison(expected, actual, Tolerance.Parse("1.0")).Graph(parameters);
            var expectedResult2 = File.ReadAllText("Base64SecondOrderResponse.html");
            Assert.AreEqual(expectedResult2, result2);
        }

        [TestMethod, TestCategory("Integration"), DeploymentItem("TestData\\Base64SimpleResult.html")]
        public void TimeSeriesComparisonGraphSimpleNumericalTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("2000-11-24");
            expected.AddMeasurement(new Measurement
            {
                Timestamp = timestamp,
                Value = "1"
            });
            expected.AddMeasurement(new Measurement
            {
                Timestamp = timestamp.Date.AddSeconds(1),
                Value = "2"
            });
            var timeseriesComparison = new TimeSeriesComparison(expected, expected);
            var result = timeseriesComparison.GraphX(320, 200);
            var expectedResult = File.ReadAllText("Base64SimpleResult.html");
            Assert.AreEqual(expectedResult, result);
            var parameters = new Dictionary<string, string>
            {
                {"start timestamp", "2004-03-24"}
            };
            var result2 = timeseriesComparison.Graph(parameters);
            Assert.AreEqual("no data", result2);
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeSeriesComparisonQueryTest()
        {
            var timestamp = DateTime.Today;
            var expected = new TimeSeries();
            var actual = new TimeSeries();
            expected.AddMeasurement(new Measurement(timestamp, "ok", true));
            actual.AddMeasurement(new Measurement(timestamp, "ok", true));
            expected.AddMeasurement(new Measurement(timestamp.AddSeconds(1), "ok", true));
            actual.AddMeasurement(new Measurement(timestamp.AddSeconds(1), "wrong", true));
            expected.AddMeasurement(new Measurement(timestamp.AddSeconds(2), "ok", true));
            actual.AddMeasurement(new Measurement(timestamp.AddSeconds(3), "ok", true));
            var response = new TimeSeriesComparison(expected, actual);
            var actualResult = response.Query();
            Assert.AreEqual(3, actualResult.Count);
            AssertIssue("ValueIssue", actualResult[0] as Collection<object>);
            AssertIssue("Missing", actualResult[1] as Collection<object>);
            AssertIssue("Surplus", actualResult[2] as Collection<object>);
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeSeriesComparisonSelectDataTest()
        {
            var table = new MeasurementComparisonDictionary();

            var timestampBase = DateTime.Today;

            for (double time = -1; time <= 31; time += 0.5)
            {
                var timestamp = timestampBase.AddSeconds(time);
                var timeString = time.To<string>();
                table.Add(timestamp, new MeasurementComparisonMock(timeString, timeString, CompareOutcome.None));
            }
            var subset = table.Subset(timestampBase, timestampBase.AddSeconds(10));
            Assert.AreEqual(21, subset.Count);
            Assert.AreEqual("0", subset.First().Value.Value.ActualValueOut);
            Assert.AreEqual("10", subset.Last().Value.Value.ActualValueOut);
        }
    }
}