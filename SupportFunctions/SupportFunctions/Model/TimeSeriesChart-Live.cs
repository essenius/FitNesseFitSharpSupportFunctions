// Copyright 2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

// TimeSeriesChart using LiveCharts 2 which supports .NET 5.0
// It's still a beta, so that's why this SupportFunctions version is a beta too.

#if NET5_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal sealed class TimeSeriesChart
    {
        private const string ActualCaption = "Actual";
        private const string ExpectedCaption = "Expected";
        private const string FailCaption = "Fail";
        private const int LineThickness = 3;
        private const string XAxisTitleTemplate = "Time ({0})";
        private const string YAxisTitle = "Value";
        private const int MinTextSize = 10;
        private const int MaxTextSize = 16;
        private const int TextScalingFactor = 40;
        private static readonly SKColor ActualColor = SKColors.MidnightBlue;
        private static readonly SKColor AxisColor = SKColors.DarkSlateGray;
        private static readonly SKColor ChartColor = SKColors.GhostWhite;
        private static readonly SKColor FailColor = SKColors.Crimson;
        private static readonly SKColor ExpectedColor = SKColors.MediumAquamarine; // wheat SpringGreen Gold LightCoral
        private static readonly SKColor MajorGridColor = SKColors.DarkGray;

        private ISeries _actualSeries;
        private SKCartesianChart _chart;
        private ISeries _expectedSeries;
        private ISeries _failSeries;
        private int _textSize;


        private static void AddPoint(ISeries series, double x, double? y)
        {
            var values = series.Values as List<ObservablePoint>;
            Debug.Assert(values != null, nameof(values) + " != null");
            if (y == null) return;
            var point = new ObservablePoint(x, y);
            values.Add(point);
        }

        
        private static SKImage CombinedImage(SKImage graph, SKImage legend, int bottomMargin, int rightMargin)
        {
            // Draw the legend in the bottom right of the graph. 
            // We do this by drawing both images on a new canvas, and taking a snapshot.
            // offset by right margin so the legend aligns with the graph.
            var imageSize = new SKSize(graph.Width, graph.Height);
            using var surface = SKSurface.Create(new SKImageInfo(graph.Width, graph.Height));
            using var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawImage(graph, SKRect.Create(new SKPoint(0, 0), imageSize));
            var legendPosition = new SKPoint(graph.Width - legend.Width - rightMargin - 1, graph.Height - legend.Height -bottomMargin - 1);
            canvas.DrawImage(legend, SKRect.Create(legendPosition, new SKSize(legend.Width, legend.Height)));
            return surface.Snapshot();
        }

        private string AsBase64PngString()
        {
            Requires.NotNull(_chart, nameof(_chart));
            using var  legend = new TimeSeriesLegend();
            using var image = _chart.GetImage();
            legend.Draw(_chart);
            using var legendImage = legend.Result;
            var rightMargin = (int)(_chart.Width - _chart.Core.DrawMarginSize.Width - _chart.Core.DrawMarginLocation.X);
            using var combinedImage = CombinedImage(image, legendImage, (legend.MaxHeight - legend.Height)/2,rightMargin);
            using var encodedImage = combinedImage.Encode();
            return Convert.ToBase64String(encodedImage.AsSpan());
        }

        internal string ChartDataFor(MeasurementComparisonDictionary sourceData, AxisLimits limits, Size size)
        {
            InitChart(size);
            var axisPaintTask = new SolidColorPaintTask(AxisColor) {StrokeThickness = LineThickness};
            var timeUnit = limits.TimeUnit;
            limits.EnsureNonZeroRanges();
            Requires.NotNull(limits.StartTimestamp, $"{nameof(limits)}.{nameof(limits.StartTimestamp)}");
            InitSeries(sourceData, limits.StartTimestamp, timeUnit);
            _chart.Sections = new List<RectangularSection>()
                .Append(new RectangularSection {Xi = 0, Xj = 0, Stroke = axisPaintTask})
                .Append(new RectangularSection {Yi = 0, Yj = 0, Stroke = axisPaintTask});
            _chart.XAxes = CreateAxes(limits.X, XAxisTitleTemplate.FillIn(timeUnit.Caption));

            _chart.YAxes = CreateAxes(limits.Y, YAxisTitle);
            return AsBase64PngString();
        }

        internal string ChartInHtmlFor(MeasurementComparisonDictionary sourceData, AxisLimits limits, Size size)
        {
            var chartInBase64 = ChartDataFor(sourceData, limits, size);
            return WebFunctions.AsImg(chartInBase64);
//            return Invariant($"<img alt=\"Time series chart\" src=\"data:image/png;base64,{chartInBase64}\"/>");
        }

        private IEnumerable<IAxis> CreateAxes(Dimension limits, string name)
        {
            return new IAxis[]
            {
                new Axis
                {
                    MinLimit = limits.GridlineMin,
                    MaxLimit = limits.SnapToGrid ? limits.GridlineMax : limits.Max,
                    MinStep = limits.GridlineInterval,

                    SeparatorsPaint = new SolidColorPaintTask(MajorGridColor),
                    ShowSeparatorLines = true,
                    Name = name,
                    TextSize = _textSize,
                    NameTextSize = _textSize
                }
            };
        }

        private void InitChart(Size size)
        {
            _chart = new SKCartesianChart
            {
                Width = size.Width, Height = size.Height, Background = SKColors.White
            };
            _textSize = Math.Max(Math.Min(size.Width / TextScalingFactor, MaxTextSize),MinTextSize);
        }

        private void InitSeries(MeasurementComparisonDictionary sourceData, DateTime baseTimestamp, TimeUnitForDisplay timeUnit)
        {
            _expectedSeries = new LineSeries<ObservablePoint>
            {
                Values = new List<ObservablePoint>(),
                Fill = null,
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaintTask(ExpectedColor) {StrokeThickness = LineThickness, PathEffect = new DashEffect(new[] {5f, 5f})},
                Name = ExpectedCaption
            };
            _actualSeries = new LineSeries<ObservablePoint>
            {
                Values = new List<ObservablePoint>(),
                Fill = null,
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaintTask(ActualColor) {StrokeThickness = LineThickness + 1},
                Name = ActualCaption
            };
            var errorPaintTask = new SolidColorPaintTask(FailColor);
            _failSeries = new LineSeries<ObservablePoint, CircleGeometry>
            {
                GeometrySize = 9,
                Values = new List<ObservablePoint>(),
                Fill = null,
                Stroke = null,
                GeometryStroke = errorPaintTask,
                GeometryFill = errorPaintTask,
                LineSmoothness = 0,
                Name = FailCaption
            };

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
            }

            var chartSeries = new List<ISeries> {_actualSeries, _expectedSeries};
            var errors = _failSeries.Values as List<ObservablePoint>;
            Debug.Assert(errors != null, nameof(errors) + " != null");
            if (errors.Any())
            {
                chartSeries.Add(_failSeries);
            }
            _chart.Series = chartSeries;
            _chart.DrawMarginFrame = new DrawMarginFrame
            {
                Fill = new SolidColorPaintTask(ChartColor),
                Stroke = new SolidColorPaintTask(MajorGridColor)
            };
        }
    }
}
#endif