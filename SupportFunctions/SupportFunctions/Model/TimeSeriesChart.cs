// Copyright 2015-2020 Rik Essenius
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
using System.Web.UI.DataVisualization.Charting;
using SupportFunctions.Utilities;
using static System.FormattableString;

namespace SupportFunctions.Model
{
    internal sealed class TimeSeriesChart : IDisposable
    {
        private const string ActualCaption = "Actual";
        private const string ChartAreaName = "Default";
        private const string ChartLegend = "Legend";
        private const string ExpectedCaption = "Expected";
        private const string FailCaption = "Fail";
        private const string XAxisTitleTemplate = "Time ({0})";
        private const string YAxisTitle = "Value";
        private static readonly Color AxisColor = Color.DarkSlateGray;
        private static readonly Color MajorGridColor = Color.DarkGray;
        private static readonly Color MinorGridColor = Color.Silver;

        private Series _actualSeries;
        private ChartArea _area;
        private Chart _chart;
        private Series _expectedSeries;
        private Series _failSeries;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static void AddPoint(Series series, double x, double? y)
        {
            Debug.Assert(series.Points != null, "series.Points != null");
            if (y == null) return;
            using (var point = new DataPoint())
            {
                point.XValue = x;
                point.YValues = new[] {y.Value};
                series.Points.Add(point);
            }
        }

        private string AsBase64String(ChartImageFormat chartImageFormat)
        {
            Debug.Assert(_chart != null, "_chart != null");
            using (var ms = new MemoryStream())
            {
                _chart.SaveImage(ms, chartImageFormat);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        internal string ChartDataFor(MeasurementComparisonDictionary sourceData, AxisLimits limits, Size size)
        {
            InitChart(size);
            var timeUnit = limits.TimeUnit;
            limits.EnsureNonZeroRanges();
            Debug.Assert(limits.StartTimestamp != null, "limits.StartTimestamp != null");
            InitSeries(sourceData, limits.StartTimestamp, timeUnit);
            InitChartArea();
            SetAxisDimensions(_area.AxisX, XAxisTitleTemplate.FillIn(timeUnit.Caption), limits.X);
            SetAxisDimensions(_area.AxisY, YAxisTitle, limits.Y);
            _area.AxisY.Crossing = 0.0;
            _chart.ChartAreas.Add(_area);
            _chart.Legends.Add(new Legend(ChartLegend));
            _chart.Legends[ChartLegend].DockedToChartArea = ChartAreaName;
            return AsBase64String(ChartImageFormat.Png);
        }

        internal string ChartInHtmlFor(MeasurementComparisonDictionary sourceData, AxisLimits limits, Size size)
        {
            //TODO: calculate Y values dynamically
            var chartInBase64 = ChartDataFor(sourceData, limits, size);
            return Invariant($"<img src=\"data:image/png;base64,{chartInBase64}\"/>");
        }

        private static T ChooseValue<T>(SeriesType seriesType, T expectedValue, T actualValue, T failValue)
        {
            if (seriesType == SeriesType.Actual) return actualValue;
            return seriesType == SeriesType.Expected ? expectedValue : failValue;
        }

        [SuppressMessage("ReSharper", "UseNullPropagation", Justification = "Conflicts with CA2213")]
        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_chart != null) _chart.Dispose();
            if (_actualSeries != null) _actualSeries.Dispose();
            if (_expectedSeries != null) _expectedSeries.Dispose();
            if (_failSeries != null) _failSeries.Dispose();
            if (_area != null) _area.Dispose();
        }

        ~TimeSeriesChart() => Dispose(false);

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer", Justification = "conflicts with CA2000"),
         SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "conflicts with CA2000")]
        private void InitChart(Size size)
        {
            _chart = new Chart();
            _chart.AntiAliasing = AntiAliasingStyles.All;
            _chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
            _chart.Width = size.Width;
            _chart.Height = size.Height;
        }

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer", Justification = "conflicts with CA2000"),
         SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "conflicts with CA2000")]
        private void InitChartArea()
        {
            _area = new ChartArea(ChartAreaName);
            _area.BackColor = Color.WhiteSmoke;
            _area.BackSecondaryColor = Color.White;
            _area.BackGradientStyle = GradientStyle.TopBottom;
        }

        private void InitSeries(MeasurementComparisonDictionary sourceData, DateTime baseTimestamp, TimeUnitForDisplay timeUnit)
        {
            _actualSeries = new Series(ActualCaption);
            _expectedSeries = new Series(ExpectedCaption);
            _failSeries = new Series(FailCaption);

            var hasFailure = false;
            foreach (var key in sourceData.Keys)
            {
                var xValue = timeUnit.ConvertFromSeconds((key - baseTimestamp).TotalSeconds);
                var value = sourceData[key].Value;
                var expectedY = value.ExpectedValueOut?.To<double>();
                // if we have a non-convertible value, return null, so it is skipped
                var actualY = value.ActualValueOut?.ToWithDefault<double?>(null);
                // ensure that IsGood issues are also marked as failures.
                var pass = sourceData[key].IsOk();

                var failValue = actualY ?? expectedY;
                AddPoint(_expectedSeries, xValue, expectedY);
                AddPoint(_actualSeries, xValue, actualY);
                if (pass) continue;
                AddPoint(_failSeries, xValue, failValue);
                hasFailure = true;
            }

            SetSeriesStyle(_expectedSeries, SeriesType.Expected, hasFailure);
            SetSeriesStyle(_actualSeries, SeriesType.Actual, hasFailure);
            SetSeriesStyle(_failSeries, SeriesType.Fail, hasFailure);

            _chart.Series.Add(_expectedSeries);
            _chart.Series.Add(_actualSeries);
            _chart.Series.Add(_failSeries);
        }

        private static void SetAxisDimensions(Axis axis, string title, Dimension dimension)
        {
            axis.Minimum = dimension.GridlineMin;
            axis.Maximum = dimension.SnapToGrid ? dimension.GridlineMax : dimension.Max;
            axis.Interval = dimension.GridlineInterval;
            axis.IsMarksNextToAxis = false;
            axis.Title = title;
            axis.LineColor = AxisColor;
            axis.MajorTickMark.Enabled = true;
            axis.MajorTickMark.LineColor = MajorGridColor;
            axis.MinorTickMark.Enabled = true;
            axis.MinorTickMark.LineColor = MinorGridColor;
            axis.LabelStyle.IsEndLabelVisible = true;
            axis.MajorGrid.LineColor = MajorGridColor;
            axis.MajorGrid.LineWidth = 1;
            axis.LineWidth = 2;
        }

        private static void SetSeriesStyle(Series series, SeriesType seriesType, bool hasFailures)
        {
            series.ChartType = ChooseValue(seriesType, SeriesChartType.FastLine, SeriesChartType.FastLine, SeriesChartType.Line);
            series.Color = ChooseValue(seriesType, Color.Chocolate, Color.SteelBlue, Color.Crimson);
            series.BorderWidth = ChooseValue(seriesType, 3, 2, 0);
            series.MarkerSize = ChooseValue(seriesType, 0, 0, 8);
            series.MarkerStyle = ChooseValue(seriesType, MarkerStyle.None, MarkerStyle.None, MarkerStyle.Circle);
            series.IsVisibleInLegend = ChooseValue(seriesType, true, true, hasFailures);
        }

        private enum SeriesType
        {
            Actual,
            Expected,
            Fail
        }
    }
}
