// Copyright 2016-2019 Rik Essenius
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
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class ToleranceValue
    {
        private const StringComparison Ordinal = StringComparison.Ordinal;

        private double _appliedValue;

        public double AppliedValue
        {
            get
            {
                UpdateValues();
                return Precision == null ? _appliedValue : _appliedValue.RoundedTo(Precision).To<double>();
            }
        }

        public double? DataRange { get; set; }
        private bool IsAbsolute { get; set; }
        public int? Precision { get; private set; }
        private int? SignificantDigits { get; set; }
        private double Value { get; set; }

        public static ToleranceValue Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new ToleranceValue
                {
                    Value = 0.0,
                    IsAbsolute = true,
                    SignificantDigits = null
                };
            }
            var rawTolerance = input;
            int? significantDigits = null;
            if (input.Contains(":"))
            {
                var split = input.Split(':');
                rawTolerance = split[0];
                significantDigits = split[1].To<int>();
            }
            if (rawTolerance.EqualsIgnoreCase("epsilon"))
            {
                return new ToleranceValue
                {
                    Value = double.Epsilon,
                    IsAbsolute = true,
                    SignificantDigits = significantDigits
                };
            }
            var isAbsolute = !rawTolerance.EndsWith("%", Ordinal);

            var tolerance = Math.Abs(
                isAbsolute
                    ? Math.Abs(rawTolerance.To<double>())
                    : rawTolerance.Substring(0, rawTolerance.IndexOf("%", Ordinal)).To<double>() / 100.0
            );

            return new ToleranceValue
            {
                Value = tolerance,
                IsAbsolute = isAbsolute,
                SignificantDigits = significantDigits
            };
        }

        private void UpdateValues()
        {
            _appliedValue = IsAbsolute ? Value : Math.Abs(DataRange ?? 0) * Value;
            if (SignificantDigits == null || _appliedValue.IsZero())
            {
                return;
            }
            var desiredPrecision = Math.Max(Math.Ceiling(-Math.Log10(_appliedValue)) + SignificantDigits.Value - 1, 0);
            Precision = Convert.ToInt32(desiredPrecision);
        }
    }
}