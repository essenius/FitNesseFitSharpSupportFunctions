// Copyright 2017-2020 Rik Essenius
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
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class Dimension
    {
        public Dimension(double min, double max, bool snapToGrid = true)
        {
            Min = min;
            Max = max;
            SnapToGrid = snapToGrid;
        }

        public double GridlineInterval
        {
            get
            {
                if (Range.IsZero()) return 0;
                var orderOfMagnitude = OrderOfMagnitude(Range);
                var normalValue = Normalized(Range, orderOfMagnitude);
                Requires.Condition(normalValue > 0.1, $"{nameof(normalValue)} > 0.1");
                Requires.Condition(normalValue <= 1, $"{nameof(normalValue)} <= 1");
                var roundedNormalValue =
                    normalValue > 0.5
                        ? 1.0
                        : normalValue > 0.25
                            ? 0.5
                            : normalValue > 0.2
                                ? 0.25
                                : 0.2;
                return Denormalized(roundedNormalValue, orderOfMagnitude - 1);
            }
        }

        // These two are a bit tricky. Max/GridlineInterval shoud become integer, but floats sometimes have precision errors. 
        // So we round the value first. If we round it to 5 digits, the difference wouldn't be visible anyway.
        // Then the ceiling or floor kicks in to drag the outer gridline up or down
        public double GridlineMax => GridlineInterval.IsZero() ? 0D : Math.Ceiling(Math.Round(Max / GridlineInterval, 5)) * GridlineInterval;
        public double GridlineMin => GridlineInterval.IsZero() ? 0D : Math.Floor(Math.Round(Min / GridlineInterval, 5)) * GridlineInterval;

        public double Max { get; private set; }
        public double Min { get; private set; }

        private double Range => Math.Abs(Max - Min);

        public bool SnapToGrid { get; }
        private static double Denormalized(double value, int orderOfMagnitude) => value * Math.Pow(10, orderOfMagnitude);

        public void EnsureNonZeroRange(double deltaMin, double deltaPlus)
        {
            if (!Range.IsZero()) return;
            Min -= deltaMin;
            Max += deltaPlus;
        }

        public static Dimension GetExtremeValues(ICollection<IMeasurementComparison> values, double? minValue,
            double? maxValue)
        {
            if (minValue == null || maxValue == null)
            {
                var metadata = new TimeSeriesMetadata<IMeasurementComparison>(values,
                    p => p.Value.ExpectedValueOut, p => p.Value.ActualValueOut);
                minValue = minValue ?? metadata.MinValue;
                maxValue = maxValue ?? metadata.MaxValue;
            }
            Requires.NotNull(minValue, nameof(minValue));
            Requires.NotNull(maxValue, nameof(maxValue));
            return new Dimension(minValue.Value, maxValue.Value);
        }

        private static double Normalized(double value, double orderOfMagnitude) => value * Math.Pow(10, -orderOfMagnitude);

        private static int OrderOfMagnitude(double value)
        {
            Requires.Condition(!value.IsZero(), $"{nameof(value)} is non-zero");
            return Math.Ceiling(Math.Log10(value)).To<int>();
        }
    }
}
