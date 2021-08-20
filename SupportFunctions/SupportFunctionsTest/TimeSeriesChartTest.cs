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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class TimeSeriesChartTest
    {
#if NET5_0
        public const string ChartFolder = "TestDataLive\\";
#else
        public const string ChartFolder = "TestDataWebUI\\";
#endif
        private const string ChartOutputFolder = "TestOutput-" + ChartFolder;

        [ClassInitialize, SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "False positive")]
        public static void ClassInitialize(TestContext testContext)
        {
            Directory.CreateDirectory(ChartOutputFolder);
        }

        private static MeasurementComparisonDictionary SecondOrderGraphWithValueIssues(DateTime startTimestamp)
        {
            var table = new MeasurementComparisonDictionary();
            const double zetaExpected = 0.205;
            const double zetaActual = 0.2;
            const double omega = 1;
            const double theta = Math.PI / 2;
            var minY = double.MaxValue;
            var maxY = double.MinValue;
            for (double time = 0; time <= 30; time += 0.5)
            {
                var timestamp = startTimestamp.AddSeconds(time);
                var expectedValue = SecondOrderResponse(time, zetaExpected, omega, theta);
                var actualValue = SecondOrderResponse(time, zetaActual, omega, theta);
                if (actualValue > maxY) maxY = actualValue;
                if (actualValue < minY) minY = actualValue;

                var okValue = time < 12 || time > 16 && time < 25 || time > 25.5;

                table.Add(timestamp, new MeasurementComparisonMock(
                    expectedValue.To<string>(),
                    actualValue.To<string>(),
                    okValue ? CompareOutcome.None : CompareOutcome.ValueIssue));
            }
            return table;
        }

        public static double SecondOrderResponse(double time, double zeta, double omega, double theta) =>
            1 - Math.Exp(-zeta * omega * time) / Math.Sqrt(1 - Math.Pow(zeta, 2)) *
            Math.Sin(Math.Sqrt(1 - Math.Pow(zeta, 2)) * omega * time + theta);

        public static void AssertChartImage(string actual, string fileName, string testName)
        {
            var outputFile = Path.Combine(ChartOutputFolder, fileName);
            File.WriteAllText(outputFile, actual);
            var expected = File.ReadAllText(fileName);
            Debug.Print($"OutputFile: {Path.GetFullPath(outputFile)}");
            Assert.AreEqual(expected, actual, $"Test: {testName}, file: {fileName}");
        }

        private const string MinutesTimeRangeExpectedFile = "Base64MinutesTimeRange.html";

        [TestMethod, TestCategory("Unit"), DeploymentItem(ChartFolder + MinutesTimeRangeExpectedFile)]
        public void TimeSeriesChartMinutesTimeTest()
        {
            var table = new MeasurementComparisonDictionary();

            var startTimestamp = DateTime.Today;
            const double minY = -1;
            const double maxY = 50;
            table.Add(startTimestamp, new MeasurementComparisonMock("49.95", "49.95", CompareOutcome.None));
            table.Add(startTimestamp.AddSeconds(9660), new MeasurementComparisonMock("-0.50", "-0.50", CompareOutcome.None));
            table.Add(startTimestamp.AddSeconds(19320), new MeasurementComparisonMock("3.00", "3.20", CompareOutcome.OutsideToleranceIssue));

            var endTimestamp = table.Last().Key;
            var chart = new TimeSeriesChart();
            var base64Result = chart.ChartInHtmlFor(table,
                new AxisLimits(startTimestamp, endTimestamp, new Dimension(minY, maxY)),
                new Size(800, 600));
            AssertChartImage(base64Result, MinutesTimeRangeExpectedFile, nameof(TimeSeriesChartMinutesTimeTest));
        }

        private const string SecondOrderResponseLimitedYFile = "Base64SecondOrderResponseLimitedY.html";

        [TestMethod, TestCategory("Unit"), DeploymentItem(ChartFolder + SecondOrderResponseLimitedYFile)]
        public void TimeSeriesChartSecondOrderResponseTest()
        {
            var startTimestamp = DateTime.Today;
            var table = SecondOrderGraphWithValueIssues(startTimestamp);
            var endTimestamp = table.Last().Key;
            var timeSeriesChart = new TimeSeriesChart();
            var result = timeSeriesChart.ChartDataFor(table,
                new AxisLimits(startTimestamp, endTimestamp, new Dimension(0.7, 1.6)),
                new Size(800, 600));
            result = WebFunctions.AsImg(result);
            AssertChartImage(result, SecondOrderResponseLimitedYFile, nameof(TimeSeriesChartSecondOrderResponseTest));
        }

        private const string SmallRangeFile = "Base64SmallRange.html";

        [TestMethod, TestCategory("Unit"), DeploymentItem(ChartFolder + SmallRangeFile)]
        public void TimeSeriesChartVerySmallRangeTest()
        {
            var table = new MeasurementComparisonDictionary();

            var startTimestamp = DateTime.Today;
            const double minY = 49.93;
            const double maxY = 50.02;
            table.Add(startTimestamp, new MeasurementComparisonMock("49.95", "49.95", CompareOutcome.None));
            table.Add(startTimestamp.AddSeconds(1), new MeasurementComparisonMock("50.0", "50.0", CompareOutcome.None));
            // Ading a double NaN to check whether that is ignored
            table.Add(startTimestamp.AddSeconds(2), new MeasurementComparisonMock("NaN", "NaN", CompareOutcome.None));
            var chart = new TimeSeriesChart();
            var base64Result = chart.ChartInHtmlFor(table,
                new AxisLimits(startTimestamp, startTimestamp.AddSeconds(1), new Dimension(minY, maxY)),
                new Size(800, 600));
            AssertChartImage(base64Result, SmallRangeFile, nameof(TimeSeriesChartVerySmallRangeTest));
        }
    }
}
