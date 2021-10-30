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

// Legend construction for Live Charts 2, which currently only supports legends for UI components

#if NET5_0
using System;
using System.Diagnostics;
using System.Linq;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SupportFunctions.Model
{
    internal class TimeSeriesLegend : IDisposable
    {
        private SKCanvas _canvas;
        private SKImageInfo _imageInfo;
        private SKSurface _surface;
        private int _textHeight;
        public int Height { get; private set; }
        public int MaxHeight { get; private set; }
        private int MaxWidth { get; set; }
        public SKImage Result { get; private set; }
        private int Width { get; set; }

        public void Dispose()
        {
            Result?.Dispose();
            _canvas?.Dispose();
            _surface?.Dispose();
        }

        private SKPaint CreateLinePaint(SKColor color, float width = 0f, bool isStroke = true)
        {
            Debug.Assert(_textHeight > 0, "Measure happend");
            return new SKPaint
            {
                TextSize = _textHeight,
                IsStroke = isStroke,
                IsAntialias = true,
                StrokeWidth = width,
                Color = color
            };
        }

        private void CreateResizedImage()
        {
            using var image = _surface.Snapshot();
            var boundary = SKRectI.Create(0, 0, Width, Height);
            Result = image.Subset(boundary);
        }

        private SKPaint CreateStrokePaint(IStrokedAndFilled<SkiaSharpDrawingContext> series)
        {
            var strokePaint = new SKPaint
            {
                TextSize = _textHeight,
                IsAntialias = true,
                IsStroke = series.Stroke != null
            };
            if (series.Stroke is Paint lineStroke)
            {
                strokePaint.Color = lineStroke.Color;
                strokePaint.StrokeWidth = lineStroke.StrokeThickness;
                if (lineStroke.PathEffect == null) return strokePaint;
                // using null is a shortcut, as the function doesn't use the drawing context.
                lineStroke.PathEffect.CreateEffect(null);
                strokePaint.PathEffect = lineStroke.PathEffect.SKPathEffect;
            }
            else
            {
                strokePaint.Color = SKColors.Transparent;
                strokePaint.StrokeWidth = 0;
            }
            return strokePaint;
        }

        public void Draw(SKCartesianChart chart)
        {
            Initialize(chart);

            var borderPaint = CreateLinePaint(SKColors.Black);
            var textPaint = borderPaint.Clone();
            textPaint.IsStroke = false;

            var primaryXAxis = chart.XAxes.First();
            var xAxisTitleWidth = MeasureTextWidth(primaryXAxis.Name, textPaint);

            MaxWidth -= xAxisTitleWidth / 2;
            var cursor = new LegendCursor(MaxWidth, _textHeight);

            foreach (var chartSeries in chart.Series)
            {
                if (!(chartSeries is ILineSeries<SkiaSharpDrawingContext> entry)) continue;
                cursor.CheckNextLine();
                var strokePaint = CreateStrokePaint(entry);

                _canvas.DrawLine(cursor.X, cursor.LineY, cursor.LineEndX, cursor.LineY, strokePaint);
                _canvas.DrawText(entry.Name, cursor.TextStartX, cursor.TextY, textPaint);

                // This is a bit of a shortcut, all geometries become circles. Not an issue right now
                // since we only use a circle. Still need to figure out how to use geometries more effectively.
                if (entry.GeometrySize > 0 && entry.GeometryFill is Paint geometryFill)
                {
                    DrawCircle(
                        new SKPoint(cursor.GeometryX, cursor.LineY), 
                        entry.GeometrySize, 
                        geometryFill.Color);
                }
                var seriesNameWidth = MeasureTextWidth(entry.Name, textPaint);
                cursor.TextWidth = seriesNameWidth;
                cursor.NextSeries();
            }

            Width = (int) Math.Round(Math.Min(cursor.MaxX, MaxWidth));
            Height = (int) Math.Round(Math.Min(cursor.MaxY, MaxHeight));
            DrawBorder(borderPaint);
            CreateResizedImage();
        }

        private void DrawBorder(SKPaint borderPaint)
        {
            var border = SKRect.Create(new SKPoint(0, 0), new SKSize(Width - 1, Height - 1));
            _canvas.DrawRect(border, borderPaint);
        }

        private void DrawCircle(SKPoint position, double size, SKColor color)
        {
            Debug.Assert(_canvas != null, "_canvas initialized");
            var geometryPaint = new SKPaint
            {
                IsStroke = false,
                IsAntialias = true,
                Color = color
            };
            var radius = (float) size / 2;
            _canvas.DrawCircle(position, radius, geometryPaint);
        }

        private void Initialize(SKCartesianChart chart)
        {
            var primaryXAxis = chart.XAxes.First();
            _textHeight = (int) primaryXAxis.NameTextSize;
            MaxWidth = (int) (chart.Core.DrawMarginSize.Width / 2);
            MaxHeight = (int) (chart.Height - chart.Core.DrawMarginSize.Height) / 2;

            _imageInfo = new SKImageInfo(MaxWidth, MaxHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(_imageInfo);
            _canvas = _surface.Canvas;
            _canvas.Clear(SKColors.Transparent);
        }

        private static int MeasureTextWidth(string text, SKPaint paint)
        {
            var boundingRectangle = new SKRect();
            paint.MeasureText(text, ref boundingRectangle);
            return (int) boundingRectangle.Width;
        }
    }
}
#endif
