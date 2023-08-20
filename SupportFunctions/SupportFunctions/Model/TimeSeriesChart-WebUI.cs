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

// TimeSeriesChart using Web.UI.DataVisualization (which is standard in .NET Framework)

#if NET6_0 == false
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Web.UI.DataVisualization.Charting;
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal sealed class TimeSeriesChart : IDisposable
    {
        private const string ActualCaption = "Actual";
        private const string ChartAreaName = "Default";
        private const string ChartLegend = "Legend";
        private const string ExpectedCaption = "Expected";
        private const string FailCaption = "Fail";
        private const int LineThickness = 3;
        private const int MaxTextSize = 10;
        private const int MinTextSize = 7;
        private const int TextScalingFactor = 50;
        private const string XAxisTitleTemplate = "Time ({0})";
        private const string YAxisTitle = "Value";

        private static readonly Color ActualColor = Color.MidnightBlue;
        private static readonly Color AxisColor = Color.DarkSlateGray;
        private static readonly Color ChartColor = Color.WhiteSmoke;
        private static readonly Color ExpectedColor = Color.MediumAquamarine;
        private static readonly Color FailColor = Color.Crimson;
        private static readonly Color MajorGridColor = Color.DarkGray;
        private static readonly Color MinorGridColor = Color.Silver;

        private Series _actualSeries;
        private ChartArea _area;
        private Chart _chart;
        private Series _expectedSeries;
        private Series _failSeries;
        private int _textSize;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static void AddPoint(Series series, double x, double? y)
        {
            Requires.NotNull(series.Points, $"{nameof(series)}.{nameof(series.Points)}");
            if (y == null) return;
            using var point = new DataPoint
            {
                XValue = x,
                YValues = new[] { y.Value }
            };
            series.Points.Add(point);
        }

        private string AsBase64String(ChartImageFormat chartImageFormat)
        {
            Requires.NotNull(_chart, nameof(_chart));
            using var ms = new MemoryStream();
            _chart.SaveImage(ms, chartImageFormat);
            return Convert.ToBase64String(ms.ToArray());
        }

        internal string ChartDataFor(
            MeasurementComparisonDictionary sourceData, 
            AxisLimits limits, 
            Size size)
        {
            InitChart(size);
            var timeUnit = limits.TimeUnit;
            limits.EnsureNonZeroRanges();
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
            var chartInBase64 = ChartDataFor(sourceData, limits, size);
            return WebFunctions.AsImg(chartInBase64);
        }

        private static T ChooseValue<T>(SeriesType seriesType, T expectedValue, T actualValue, T failValue)
        {
            if (seriesType == SeriesType.Actual) return actualValue;
            return seriesType == SeriesType.Expected ? expectedValue : failValue;
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            _chart?.Dispose();
            _actualSeries?.Dispose();
            _expectedSeries?.Dispose();
            _failSeries?.Dispose();
            _area?.Dispose();
        }

        ~TimeSeriesChart() => Dispose(false);

        private void InitChart(Size size)
        {
            _chart = new Chart
            {
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High,
                Width = size.Width,
                Height = size.Height
            };
            _textSize = Math.Max(Math.Min(size.Width / TextScalingFactor, MaxTextSize), MinTextSize);
        }

        private void InitChartArea()
        {
            _area = new ChartArea(ChartAreaName)
            {
                BackColor = ChartColor,
                BackSecondaryColor = Color.White,
                BackGradientStyle = GradientStyle.TopBottom,
                BorderColor = MinorGridColor,
                BorderWidth = 1,
                BorderDashStyle = ChartDashStyle.Solid
            };
        }

        private void InitSeries(
            MeasurementComparisonDictionary sourceData, 
            DateTime baseTimestamp, 
            TimeUnitForDisplay timeUnit)
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

            SetSeriesStyle(_actualSeries, SeriesType.Actual, hasFailure);
            SetSeriesStyle(_expectedSeries, SeriesType.Expected, hasFailure);
            SetSeriesStyle(_failSeries, SeriesType.Fail, hasFailure);

            _chart.Series.Add(_actualSeries);
            _chart.Series.Add(_expectedSeries);
            _chart.Series.Add(_failSeries);
        }

        private void SetAxisDimensions(Axis axis, string title, Dimension dimension)
        {
            axis.Minimum = dimension.SnapToGrid ? dimension.GridlineMin : dimension.Min;
            axis.Maximum = dimension.SnapToGrid ? dimension.GridlineMax : dimension.Max;
            axis.Interval = dimension.GridlineInterval;
            axis.IsMarksNextToAxis = false;
            axis.Title = title;
            axis.TitleFont = new Font(axis.TitleFont.FontFamily, _textSize, axis.TitleFont.Style);
            Debug.Print($"Text size: {_textSize}");
            axis.IsLabelAutoFit = false;
            axis.LabelStyle.Font = axis.TitleFont;
            axis.LineColor = AxisColor;
            axis.MajorTickMark.Enabled = false;
            axis.MinorTickMark.Enabled = true;
            axis.MinorTickMark.LineColor = MinorGridColor;
            axis.LabelStyle.IsEndLabelVisible = true;
            axis.MajorGrid.LineColor = MajorGridColor;
            axis.MajorGrid.LineWidth = 1;
            axis.LineWidth = 2;
            axis.IntervalOffset = dimension.SnapToGrid ? 0 : dimension.GridlineMin - dimension.Min;
            axis.MinorTickMark.IntervalOffset = axis.IntervalOffset;
        }

        private static void SetSeriesStyle(Series series, SeriesType seriesType, bool hasFailures)
        {
            series.ChartType = ChooseValue(
                seriesType, SeriesChartType.FastLine, SeriesChartType.FastLine, SeriesChartType.Line);
            series.Color = ChooseValue(seriesType, ExpectedColor, ActualColor, FailColor);
            series.BorderWidth = ChooseValue(seriesType, LineThickness, LineThickness + 1, 0);
            series.MarkerSize = ChooseValue(seriesType, 0, 0, 8);
            series.MarkerStyle = ChooseValue(seriesType, MarkerStyle.None, MarkerStyle.None, MarkerStyle.Circle);
            series.BorderDashStyle = ChooseValue(
                seriesType, ChartDashStyle.Dot, ChartDashStyle.Solid, ChartDashStyle.NotSet
            );
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
#endif
