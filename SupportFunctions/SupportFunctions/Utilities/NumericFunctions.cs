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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using static System.Globalization.CultureInfo;

namespace SupportFunctions.Utilities
{
    internal static class NumericFunctions
    {
        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(double),
            typeof(decimal),
            typeof(long),
            typeof(short),
            typeof(sbyte),
            typeof(byte),
            typeof(ulong),
            typeof(ushort),
            typeof(uint),
            typeof(float)
        };

        public static int FractionalDigits(this object value)
        {
            if (!value.IsNumeric()) return 0;
            var splitE = value.ToString().Split('E', 'e');
            var eValue = splitE.Length > 1 ? -splitE[1].To<int>() : 0;
            var splitPoint = splitE[0].Split('.');
            var fractionDigits = Math.Max((splitPoint.Length == 1 ? 0 : splitPoint[1].Length) + eValue, 0);
            return fractionDigits;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Done intentionally")]
        public static bool HasMinimalDifferenceWith(this double value1, double value2, int units = 1)
        {
            var lValue1 = BitConverter.DoubleToInt64Bits(value1);
            var lValue2 = BitConverter.DoubleToInt64Bits(value2);

            // If the signs are different, return false except for +0 and -0.
            if (lValue1 >> 63 != lValue2 >> 63) return value1 == value2;

            var diff = Math.Abs(lValue1 - lValue2);
            return diff <= units;
        }

        public static bool IsFloatingPoint(this Type type) => type == typeof(double) || type == typeof(float) || type == typeof(decimal);

        public static bool IsNumeric(this Type myType) => NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);

        public static bool IsNumeric(this object expression)
        {
            if (expression == null) return false;

            // somewhat tricky. The invariant culture understands Infinity and the current culture ∞.
            if (double.TryParse(Convert.ToString(expression, InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out _)) return true;
            return double.TryParse(Convert.ToString(expression, CurrentCulture), NumberStyles.Any, NumberFormatInfo.CurrentInfo, out _);
        }

        public static bool IsZero(this double value) => value.HasMinimalDifferenceWith(0D);

        public static object RoundedTo(this object value, int? fractionDigits)
        {
            // if we can't perform the rounding, just take the original value
            if (fractionDigits == null || !value.IsNumeric() || fractionDigits.Value < 0 || fractionDigits.Value > 15) return value;
            return Math.Round(Convert.ToDouble(value, InvariantCulture), fractionDigits.Value);
        }
    }
}