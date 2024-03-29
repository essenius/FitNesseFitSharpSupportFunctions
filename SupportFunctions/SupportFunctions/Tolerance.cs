﻿// Copyright 2016-2023 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using SupportFunctions.Model;
using SupportFunctions.Utilities;
using static System.FormattableString;

namespace SupportFunctions
{
    /// <summary>Tolerance for floating point time series comparisons</summary>
    public class Tolerance
    {
        private readonly List<ToleranceValue> _toleranceValues = new List<ToleranceValue>();
        private double? _dataRange;
        private bool _isDirty = true;
        private int? _precision;
        private double _value;

        /// <summary>The range that is the basis for relative tolerances</summary>
        public double? DataRange
        {
            get => _dataRange;
            internal set
            {
                _dataRange = value;
                _isDirty = true;
            }
        }

        /// <summary>The calculated precision for the comparison results</summary>
        public int? Precision
        {
            get
            {
                UpdateValues();
                return _precision;
            }
        }

        /// <summary>The tolerance value that is applied in the comparison</summary>
        public double Value
        {
            get
            {
                UpdateValues();
                return _value;
            }
        }

        private void AddToleranceValue(ToleranceValue toleranceValue)
        {
            _toleranceValues.Add(toleranceValue);
            _isDirty = true;
        }

        /// <summary>
        ///     Parse desired tolerance. Format: one or more tolerance specs separated by semicolon.
        ///     Double for absolutes. percentage for relative compared to expected data range. Example: 0.001;0.1%
        /// </summary>
        /// <param name="input">
        ///     one or more tolerance specs separated by semicolon. Double for absolutes.
        ///     percentage for relative compared expected data range. Example: 0.001;0.1%
        /// </param>
        public static Tolerance Parse(string input)
        {
            Requires.NotNull(input, nameof(input));
            var returnValue = new Tolerance();
            foreach (var toleranceSpec in input.Split(';'))
            {
                var toleranceValue = ToleranceValue.Parse(toleranceSpec);
                returnValue.AddToleranceValue(toleranceValue);
            }

            return returnValue;
        }

        /// <returns>the absolute and relative tolerance in format 'absolute (relative %)'</returns>
        public override string ToString()
        {
            UpdateValues();
            var percentage = Value / DataRange;
            var percentageText = percentage != null && !double.IsInfinity(percentage.Value)
                ? Invariant($" ({percentage:P1})")
                : string.Empty;
            return Value > 0
                ? Invariant($"{Value}{percentageText}")
                : string.Empty;
        }

        private void UpdateValues()
        {
            if (!_isDirty) return;
            var maxTolerance = 0.0;
            int? precision = null;
            foreach (var toleranceValue in _toleranceValues)
            {
                toleranceValue.DataRange = DataRange;
                var newTolerance = toleranceValue.AppliedValue;
                if (!(newTolerance > maxTolerance)) continue;
                maxTolerance = newTolerance;
                precision = toleranceValue.Precision;
            }

            _value = maxTolerance;
            _precision = precision;
            _isDirty = false;
        }
    }
}
