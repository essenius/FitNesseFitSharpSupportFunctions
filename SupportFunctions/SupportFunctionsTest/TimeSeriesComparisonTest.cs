// Copyright 2017-2021 Rik Essenius
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
using System.Globalization;
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
        private const string AllBelowXAxisFile = "Base64AllBelowXAxis.html";

        private const string MissingExpectedFile = "Base64ResultMissingExpected.html";

        private const string MissingValuesFile = "Base64ResultWithMissingValues.html";

        private const string SecondOrderResponseFile = "Base64SecondOrderResponse.html";
        private const string SecondOrderResponseLimitedYFile = "Base64SecondOrderResponseLimitedY.html";

        private const string SimpleResultFile = "Base64SimpleResult.html";

        private static void ArrangeDataForDoTable(IEnumerable<object> data, TimeSeries expectedSeries, TimeSeries actualSeries,
            IDictionary<string, Dictionary<string, string>> expectedResults, bool? defaultIsGood)
        {
            foreach (var dataRow in data)
            {
                var entry = dataRow as object[];
                var timestamp = GetEntry(entry, 0) as string;
                var exists = GetEntry(entry, 1) as string;
                var expectedValue = GetEntry(entry, 2) as string;
                var actualValue = GetEntry(entry, 3) as string;
                var value = GetEntry(entry, 5) as string;
                var delta = GetEntry(entry, 6) as string;
                var deltaPercentage = GetEntry(entry, 7) as string;
                var expectedlIsGood = GetEntry(entry, 9) as string;
                var actualIsGood = GetEntry(entry, 10) as string;

                // if exists == expected, we don't want an actual record
                if (exists != "expected")
                {
                    actualSeries.AddMeasurement(new Measurement(timestamp, actualValue, actualIsGood ?? defaultIsGood.ToString()));
                }
                // if exists == actual, we don't want an expected record
                if (exists != "actual")
                {
                    expectedSeries.AddMeasurement(new Measurement(timestamp, expectedValue, expectedlIsGood ?? defaultIsGood.ToString()));
                }
                var result = new Dictionary<string, string>
                {
                    { "value", value }, { "delta", delta }, { "deltaPercentage", deltaPercentage }
                };
                if (GetEntry(entry, 11) is string isGood) result.Add("isGood", isGood);
                if (GetEntry(entry, 8) is string timestampOut) result.Add("timestampOut", timestampOut);
                expectedResults.Add(timestamp, result);
            }
        }

        private static void AssertIssue(string expectedIssue, IReadOnlyList<object> row)
        {
            Assert.IsNotNull(row);
            var issue = row[5] as Collection<string>;
            Assert.IsNotNull(issue);
            Assert.AreEqual(expectedIssue, issue[1]);
        }

        private static void CheckDataRows(IReadOnlyDictionary<string, Dictionary<string, string>> expectedResults, IEnumerable<object> result,
            string testCase)
        {
            const string fail = "fail:";
            // check the data rows only - we already did the header
            foreach (Collection<object> row in result.Skip(1))
            {
                var timestamp = GetTimestamp(row[0].ToString());
                Assert.IsFalse(string.IsNullOrEmpty(timestamp), $"Timestamp not found in actual result [{row[0]}] for {testCase}");
                Assert.IsTrue(expectedResults.ContainsKey(timestamp), $"Timestamp [{timestamp}] not found in expected result for {testCase}");
                var expectedResult = expectedResults[timestamp];
                var actualResult = new Dictionary<string, object>
                {
                    { "pass", (!row.Any(cell => cell.ToString().StartsWith(fail, StringComparison.Ordinal))).ToString() },
                    { "timestampOut", row[0] },
                    { "value", row[1] },
                    { "delta", row[2] },
                    { "deltaPercentage", row[3] },
                    { "isGood", row[4] }
                };

                // only test the ones we have specified in the test data
                foreach (var entry in expectedResult)
                {
                    Assert.AreEqual(entry.Value, actualResult[entry.Key], $"{entry.Key}@{testCase}[{timestamp}]");
                }
            }
        }

        // Needed in case this gets done before TimeSeriesChartTest
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            TimeSeriesChartTest.ClassInitialize(testContext);
        }

        private static object GetEntry(IReadOnlyList<object> entry, int index) => index >= entry.Count ? null : entry[index];

        private static string GetTimestamp(string cell)
        {
            // remove the pass/fail indicator by keeping everything after the first colon
            var timestamp = cell.Split(new[] { ':' }, 2)[1];
            if (string.IsNullOrEmpty(timestamp)) return null;
            if (timestamp.StartsWith("[", StringComparison.Ordinal))
            {
                timestamp = timestamp.Substring(1, timestamp.IndexOf("]", StringComparison.Ordinal) - 1);
            }
            return timestamp;
        }

        [TestMethod, TestCategory("Unit"),
         ExpectedExceptionWithMessage(typeof(ArgumentException), "Duplicate timestamp in expected: 2004-03-24T00:00:00.0000000")]
        public void TimeSeriesComparisonDoTableExceptionTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("2004-03-24", CultureInfo.InvariantCulture);
            expected.AddMeasurement(new Measurement { Timestamp = timestamp });
            expected.AddMeasurement(new Measurement { Timestamp = timestamp });
            var actual = new TimeSeries();
            var timeSeriesComparison = new TimeSeriesComparison(expected, actual);
            timeSeriesComparison.DoTable(null);
        }

        [DataTestMethod, TestCategory("Unit"), DataRow("double_0.1%_0.001", "0.1%;0.001", "0.001 (1.9 %)", true, 4, 7,
             new object[]
             {
                 new object[] { "1980-01-01T00:00:00.0000000", "both", "50", "50", true, "pass:50", "report:", "report:" },
                 new object[]
                 {
                     "1980-01-01T00:03:00.0000000", "both", "50.0000000414593", "50", "True", "pass:50 ~= 50.0000000414593", "pass:4.14593E-08",
                     "pass:0.0 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:06:00.0000000", "both", "49.9752835096486", "49.9739350539787", "False",
                     "fail:49.9739350539787 != 49.9752835096486", "fail:0.0013484556699", "fail:2.6 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:09:00.0000000", "both", "49.9480367888638", "49.9469639682952", "False",
                     "fail:49.9469639682952 != 49.9480367888638", "fail:0.0010728205686", "fail:2.1 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:12:00.0000000", "both", "49.9540729336722", "49.9547465725331", "True",
                     "pass:49.9547465725331 ~= 49.9540729336722", "pass:0.0006736388609", "pass:1.3 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:15:00.0000000", "both", "49.9626680632779", "49.964159146503", "False",
                     "fail:49.964159146503 != 49.9626680632779", "fail:0.0014910832251", "fail:2.9 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:18:00.0000000", "both", "49.9626680632779", "False", "False", "fail:[False] expected [49.9626680632779]",
                     "report:", "report:"
                 }
             }
         ), DataRow("double_0.001_rounded", "0.1%:4;0.001:4", "0.001 (1.9 %)", true, 4, 7,
             new object[]
             {
                 new object[] { "1980-01-01T00:00:00.0000000", "both", "50", "50", true, "pass:50", "report:", "report:" },
                 new object[] { "1980-01-01T00:03:00.0000000", "both", "50.0000000414593", "50", "True", "pass:50 ~= 50", "report:", "report:" },
                 new object[]
                 {
                     "1980-01-01T00:06:00.0000000", "both", "49.9752835096486", "49.9739350539787", "False", "fail:49.973935 != 49.975284",
                     "fail:0.001348", "fail:2.6 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:09:00.0000000", "both", "49.9480367888638", "49.9469639682952", "False", "fail:49.946964 != 49.948037",
                     "fail:0.001073", "fail:2.1 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:12:00.0000000", "both", "49.9540729336722", "49.9547465725331", "True", "pass:49.954747 ~= 49.954073",
                     "pass:0.000674", "pass:1.3 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:15:00.0000000", "both", "49.9626680632779", "49.964159146503", "False", "fail:49.964159 != 49.962668",
                     "fail:0.001491", "fail:2.9 %"
                 },
                 new object[]
                 {
                     "1980-01-01T00:18:00.0000000", "both", "49.9626680632779", "False", "False", "fail:[False] expected [49.962668]", "report:",
                     "report:"
                 }
             }
         ), DataRow("int_0.1%_missing_surplus", "0.1%", "2 (0.1 %)", true, 3, 5,
             new object[]
             {
                 new object[] { "1990-01-01T00:00:00.0000000", "both", "0", "0", true, "pass:0", "report:", "report:" },
                 new object[] { "1990-01-01T00:03:00.0000000", "both", "2000", "1999", "True", "pass:1999 ~= 2000", "pass:1", "pass:0.1 %" },
                 new object[]
                 {
                     "1990-01-01T00:06:00.0000000", "expected", "1000", null, "False", "fail:[1000] missing", "report:", "report:",
                     "fail:[1990-01-01T00:06:00.0000000] missing"
                 },
                 new object[]
                 {
                     "1990-01-01T00:09:00.0000000", "actual", null, "500", "False", "fail:[500] surplus", "report:", "report:",
                     "fail:[1990-01-01T00:09:00.0000000] surplus"
                 },
                 new object[] { "1990-01-01T00:12:00.0000000", "both", "500", "503", "False", "fail:503 != 500", "fail:3", "fail:0.2 %" }
             }
         ), DataRow("string", null, "", true, 1, 2,
             new object[]
             {
                 new object[] { "2000-01-01T00:00:00.0000000", "both", "hi1", "hi1", true, "pass:hi1", "report:", "report:" },
                 new object[] { "2000-01-01T00:03:00.0000000", "both", "hi2", "hi3", false, "fail:[hi3] expected [hi2]", "report:", "report:" }
             }
         ), DataRow("bool", null, "", true, 5, 6,
             new object[]
             {
                 new object[] { "2010-01-01T00:00:00.0000000", "both", "True", "True", true, "pass:True", "report:", "report:" },
                 new object[] { "2010-01-01T00:03:00.0000000", "both", "True", "False", false, "fail:[False] expected [True]", "report:", "report:" },
                 new object[] { "2010-01-01T00:06:00.0000000", "both", "True", "hi3", false, "fail:[hi3] expected [True]", "report:", "report:" },
                 new object[]
                 {
                     "2010-01-01T00:09:00.0000000", "both", "True", "True", false, "pass:True", "report:", "report:", null, "True", "False",
                     "fail:[False] expected [True]"
                 },
                 new object[]
                 {
                     "2010-01-01T00:12:00.0000000", "expected", "True", null, false, "fail:[True] missing", "report:", "report:",
                     "fail:[2010-01-01T00:12:00.0000000] missing"
                 },
                 new object[]
                 {
                     "2010-01-01T00:15:00.0000000", "actual", null, "True", false, "fail:[True] surplus", "report:", "report:",
                     "fail:[2010-01-01T00:15:00.0000000] surplus", null, null, "fail:[True] surplus"
                 }
             }
         ), DataRow("allgood", null, "", true, 0, 4,
             new object[]
             {
                 new object[] { "2020-01-01T00:00:00.0000000", "both", "10", "10", true, "pass:10", "report:", "report:" },
                 new object[] { "2020-01-01T00:03:00.0000000", "both", "12", "12", true, "pass:12", "report:", "report:" },
                 new object[] { "2020-01-01T00:06:00.0000000", "both", "13", "13", true, "pass:13", "report:", "report:" },
                 new object[] { "2020-01-01T00:09:00.0000000", "both", "14", "14", true, "pass:14", "report:", "report:" }
             }
         )]
        public void TimeSeriesComparisonDoTableTest(string testCase, string toleranceString, string usedTolerance, bool isGood, long failures,
            long dataPoints,
            object[] data)
        {
            var actualSeries = new TimeSeries();
            var expectedSeries = new TimeSeries();
            var expectedResults = new Dictionary<string, Dictionary<string, string>>();
            ArrangeDataForDoTable(data, expectedSeries, actualSeries, expectedResults, isGood);

            var timeSeriesComparison = string.IsNullOrEmpty(toleranceString)
                ? new TimeSeriesComparison(expectedSeries, actualSeries)
                : new TimeSeriesComparison(expectedSeries, actualSeries, Tolerance.Parse(toleranceString));
            var result = timeSeriesComparison.DoTable(null);

            Assert.IsTrue(result.Count > 0, $"Result count = 0 for {testCase}");
            var header = result[0] as Collection<object>;
            Assert.IsNotNull(header, $"header null for {testCase}");
            Assert.AreEqual("report:Timestamp", header[0]);
            Assert.AreEqual("report:Value", header[1], $"Value Header for {testCase}"); //+ displayTolerance
            Assert.AreEqual("report:Delta", header[2], $"Delta Header for {testCase}");
            Assert.AreEqual("report:Delta %", header[3], $"Delta % Header for {testCase}");
            Assert.AreEqual("report:Is Good", header[4], $"Is Good Header for {testCase}");

            CheckDataRows(expectedResults, result, testCase);

            Assert.AreEqual(failures, timeSeriesComparison.FailureCount, $"FailureCount {testCase}");
            Assert.AreEqual(dataPoints, timeSeriesComparison.PointCount, $"PointCount {testCase}");
            Assert.AreEqual(usedTolerance, timeSeriesComparison.UsedTolerance, $"UsedTolerance {testCase}");
        }

        [TestMethod, TestCategory("Unit")]
        public void TimeSeriesComparisonFailureCountTest()
        {
            var comparison = new TimeSeriesComparison(null, null);
            Assert.AreEqual(0, comparison.FailureCount);
        }

        [TestMethod, TestCategory("Integration"), DeploymentItem(TimeSeriesChartTest.ChartFolder + MissingExpectedFile)]
        public void TimeSeriesComparisonGraphEmptyExpectedNumericalTest()
        {
            var expected = new TimeSeries();
            var actual = new TimeSeries();
            var timestamp = DateTime.Parse("2000-11-24", CultureInfo.InvariantCulture);
            actual.AddMeasurement(new Measurement
            {
                Timestamp = timestamp,
                Value = "0"
            });
            var timeseriesComparison = new TimeSeriesComparison(expected, actual);
            var parameters = new Dictionary<string, string>
            {
                { "Width", "320" },
                { @"heIght", "200" }
            };
            var result = timeseriesComparison.Graph(parameters);
            TimeSeriesChartTest.AssertChartImage(result, MissingExpectedFile, nameof(TimeSeriesComparisonGraphEmptyExpectedNumericalTest));
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
            var timestamp = DateTime.Parse("2000-11-24", CultureInfo.InvariantCulture);
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

        [TestMethod, TestCategory("Integration"), DeploymentItem(TimeSeriesChartTest.ChartFolder + AllBelowXAxisFile)]
        public void TimeSeriesComparisonGraphNumericalAllBelowXAxisTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("1969-08-28", CultureInfo.InvariantCulture);
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
            TimeSeriesChartTest.AssertChartImage(result, AllBelowXAxisFile, nameof(TimeSeriesComparisonGraphNumericalAllBelowXAxisTest));
        }

        [TestMethod, TestCategory("Integration"), DeploymentItem(TimeSeriesChartTest.ChartFolder + MissingValuesFile)]
        public void TimeSeriesComparisonGraphNumericalWithMissingValuesTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("2000-11-24", CultureInfo.InvariantCulture);
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
            var result = timeSeriesComparison.GraphX(480, 320);
            TimeSeriesChartTest.AssertChartImage(result, MissingValuesFile, nameof(TimeSeriesComparisonGraphNumericalWithMissingValuesTest));
        }

        [TestMethod, TestCategory("Unit"), DeploymentItem(TimeSeriesChartTest.ChartFolder + SecondOrderResponseLimitedYFile),
         DeploymentItem(TimeSeriesChartTest.ChartFolder + SecondOrderResponseFile)]
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
                if (actualValue > maxY) maxY = actualValue;
                if (actualValue < minY) minY = actualValue;

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
                { "Min Value", "0.7" },
                { "Max Value", "1.6" },
                { "start timestamp", timestampBase.ToRoundTripFormat() },
                { "end timestamp", timestampBase.AddSeconds(30).ToRoundTripFormat() }
            };
            var comparison = new TimeSeriesComparison(expected, actual, Tolerance.Parse("1.0"));
            var result = comparison.Graph(parameters);
            TimeSeriesChartTest.AssertChartImage(result, SecondOrderResponseLimitedYFile, nameof(TimeSeriesComparisonGraphSecondOrderResponseTest));
            parameters.Remove("Min Value");
            parameters.Remove("Max Value");
            var result2 = new TimeSeriesComparison(expected, actual, Tolerance.Parse("1.0")).Graph(parameters);
            TimeSeriesChartTest.AssertChartImage(result2, SecondOrderResponseFile, nameof(TimeSeriesComparisonGraphSecondOrderResponseTest));
        }

        [TestMethod, TestCategory("Integration"), DeploymentItem(TimeSeriesChartTest.ChartFolder + SimpleResultFile)]
        public void TimeSeriesComparisonGraphSimpleNumericalTest()
        {
            var expected = new TimeSeries();
            var timestamp = DateTime.Parse("2000-11-24", CultureInfo.InvariantCulture);
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
            TimeSeriesChartTest.AssertChartImage(result, SimpleResultFile, nameof(TimeSeriesComparisonGraphSimpleNumericalTest));
            var parameters = new Dictionary<string, string> { { "start timestamp", "2004-03-24" } };
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
