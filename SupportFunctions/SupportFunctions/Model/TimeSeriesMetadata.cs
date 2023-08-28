// Copyright 2015-2023 Rik Essenius
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
    internal class TimeSeriesMetadata<T>
    {
        public TimeSeriesMetadata(ICollection<T> measurements, params Func<T, string>[] getValue)
        {
            DataType = null;

            if (measurements.Count == 0)
            {
                MinValue = double.NaN;
                MaxValue = double.NaN;
                return;
            }

            MinValue = double.MaxValue;
            MaxValue = double.MinValue;

            foreach (var entry in measurements)
            {
                var expected = getValue[0](entry);
                if (!string.IsNullOrEmpty(expected))
                {
                    DataType = getValue[0](entry).InferType(DataType);
                    // string is an end state, no need to dig further
                    if (DataType == typeof(string)) break;
                    // we dont need min and max for booleans, but we could need to switch to string later
                    if (DataType == typeof(bool)) continue;
                    // now we know the primary (expected) value is numerical (double, int, long)
                    // So it makes sense to determine the extremes, using secundary (actual) too
                }

                UpdateExtremes(entry, getValue);
            }

            if (DataType == null || DataType.IsNumeric()) return;

            MaxValue = double.NaN;
            MinValue = double.NaN;
        }

        public Type DataType { get; }
        public double MaxValue { get; private set; }
        public double MinValue { get; private set; }

        // if MaxValue is NaN, MinValue is too. So no need to check explicitly
        public double Range => double.IsNaN(MaxValue) ? 0 : MaxValue - MinValue;

        private void UpdateExtremes(T entry, IEnumerable<Func<T, string>> getValue)
        {
            foreach (var valueGetter in getValue)
            {
                // actual values could be non-numerical if the data set is wrong.
                // since that's what we are here to determine, we need to handle it gracefully
                if (!valueGetter(entry).InferType().IsNumeric()) continue;
                var value = valueGetter(entry).To<double>();
                if (double.IsNaN(value) || double.IsInfinity(value)) continue;
                if (value > MaxValue) MaxValue = value;
                if (value < MinValue) MinValue = value;
            }
        }
    }
}
