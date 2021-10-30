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

        private static readonly HashSet<Type> UnsignedTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(uint),
            typeof(ulong),
            typeof(ushort)
        };

        private static object ConvertToSmallestSignedType(ulong input, int length)
        {
            if (length <= 2) return unchecked((sbyte)input);
            if (length <= 4) return unchecked((short)input);
            if (length <= 8) return unchecked((int)input);
            return unchecked((long)input);
        }

        private static object ConvertToSmallestUnsignedType(ulong input, int length)
        {
            if (length <= 2) return (byte)input;
            if (length <= 4) return (ushort)input;
            if (length <= 8) return (uint)input;
            return (long)input;
        }

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

        public static bool IsFloatingPoint(this Type type) => 
            type == typeof(double) || type == typeof(float) || type == typeof(decimal);

        public static bool IsNumeric(this Type myType) => 
            NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);

        public static bool IsNumeric(this object expression)
        {
            if (expression == null) return false;
            // signed and unsigned have the same hex representation, so just checking for unsigned is OK
            if (TryParseHex(expression.ToString(), false, out _)) return true;
            // somewhat tricky. The invariant culture understands Infinity and the current culture ∞.
            return double.TryParse(Convert.ToString(expression, InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out _) ||
                   double.TryParse(Convert.ToString(expression, CurrentCulture), NumberStyles.Any, NumberFormatInfo.CurrentInfo, out _);
        }

        public static bool IsSigned(this Type type) => !UnsignedTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);

        public static bool IsZero(this double value) => value.HasMinimalDifferenceWith(0D);

        public static object RoundedTo(this object value, int? fractionDigits)
        {
            // if we can't perform the rounding, just take the original value
            if (fractionDigits == null || !value.IsNumeric() || fractionDigits.Value < 0 || fractionDigits.Value > 15) return value;
            return Math.Round(value.To<double>(), fractionDigits.Value);
        }

        public static bool TryParseHex(string input, bool isSigned, out object output)
        {
            output = null;
            if (input == null) return false;
            if (!input.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return false;
            var potentialHexNumber = input.Substring(2).Trim();
            var inputBytes = potentialHexNumber.Length;

            if (!ulong.TryParse(potentialHexNumber, NumberStyles.HexNumber, InvariantCulture, out var ulongOutput)) return false;
            output = isSigned ? ConvertToSmallestSignedType(ulongOutput, inputBytes) : ConvertToSmallestUnsignedType(ulongOutput, inputBytes);
            return true;
        }
    }
}
