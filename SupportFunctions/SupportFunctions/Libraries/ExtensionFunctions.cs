﻿// Copyright 2015-2019 Rik Essenius
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
using System.Globalization;
using System.Linq;

namespace SupportFunctions.Libraries
{
    internal static class ExtensionFunctions
    {
        internal static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        private static readonly Dictionary<string, Type> TypeDictionary = new Dictionary<string, Type>
        {
            {"BOOL", typeof(bool)},
            {"BOOLEAN", typeof(bool)},
            {"DATE", typeof(Date)},
            {"DECIMAL", typeof(decimal)},
            {"DOUBLE", typeof(double)},
            {"INT", typeof(int)},
            {"LONG", typeof(long)},
            {"STRING", typeof(string)}
        };


        public static void AddWithCheck<T>(this IDictionary<DateTime, T> dictionary, DateTime key, T value, string id)
        {
            if (dictionary.ContainsKey(key))
            {
                throw new ArgumentException(FormattableString.Invariant($"Duplicate timestamp in {id}: {key.ToRoundTripFormat()}"));
            }
            dictionary.Add(key, value);
        }


        private static bool ArePair(Type type1, Type type2, Type expected1, Type expected2)
        {
            return type1 == expected1 && type2 == expected2 || type1 == expected2 && type2 == expected1;
        }


        /// <summary>
        /// Infer from the input string what type it is, and return the field as that type (int, long, decimal, double, bool or string)
        /// </summary>
        /// <param name="input">the input sting</param>
        /// <returns>the value cast to the type it could be parsed into</returns>
        public static object CastToInferredType(this string input)
        {
            if (int.TryParse(input, out int intValue)) return intValue;
            if (long.TryParse(input, out long longValue)) return longValue;
            if (decimal.TryParse(input, out decimal decimalValue)) return decimalValue;
            if (double.TryParse(input, out double doubleValue)) return doubleValue;
            if (bool.TryParse(input, out bool boolValue)) return boolValue;
            return input;
        }



        /// <summary>
        /// Get the type that the input value can be parsed into (int, long, double, bool, string)
        /// </summary>
        /// <param name="value">the value to be examined</param>
        /// <returns>The type it was succesfully parsed into</returns>
        public static Type InferType(this object value)
        {
            var stringValue = value?.ToString();
            if (int.TryParse(stringValue, out int intValue)) return typeof(int);
            if (long.TryParse(stringValue, out long longValue)) return typeof(long);
            if (double.TryParse(stringValue, out double doubleValue)) return typeof(double);
            return bool.TryParse(stringValue, out bool boolValue) ? typeof(bool) : typeof(string);
        }


        /// <summary>
        /// Get the type that the input value can be parsed into which is compatible with current type.
        /// </summary>
        /// <param name="value">the value to be examined</param>
        /// <param name="currentType">the type it needs to be compatible with</param>
        /// <returns>The type it was succesfully parsed into</returns>
        public static Type InferType(this object value, Type currentType)
        {
            var newType = value.InferType();
            if (currentType == null || currentType == newType) return newType;
            // if one is long and the other int, we move to long
            if (ArePair(currentType, newType, typeof(long), typeof(int))) return typeof(long);
            // if one is double and the other int or long, we move to double
            if (ArePair(currentType, newType, typeof(double), typeof(long))) return typeof(double);
            if (ArePair(currentType, newType, typeof(double), typeof(int))) return typeof(double);
            // in all other cases (including bool/numerical pairs) we move to string
            return typeof(string);
        }


        public static bool IsWithinTimeRange(this DateTime pointInTime, DateTime? startTimestamp, DateTime? endTimestamp)
        {
            return (startTimestamp == null || pointInTime >= startTimestamp) &&
                   (endTimestamp == null || pointInTime <= endTimestamp);
        }


        public static void Log(this object info)
        {
            Console.WriteLine(info.To<string>());
        }


        /// <summary>
        /// Convert a value to a specified type
        /// </summary>
        /// <param name="value">the value to convert</param>
        /// <param name="targetType">The type to convert to</param>
        /// <returns>the converted value</returns>
        public static object To(this object value, Type targetType)
        {
            try
            {
                var type = Nullable.GetUnderlyingType(targetType);
                return Convert.ChangeType(value, type ?? targetType, Culture);
            }
            catch (FormatException)
            {
                return null;
            }
        }


        /// <summary>
        /// Convert a value to the specified types
        /// </summary>
        /// <typeparam name="T">The type to convert to (can be nullable)</typeparam>
        /// <param name="value">The value to convert</param>
        /// <returns>the converted value</returns>
        /// <exception cref="FormatException">if the conversion fails</exception>
        public static T To<T>(this object value)
        {
            try
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(T));
                // if underlyingType is null, the original type wasn't nullable, so take the type itself
                return (T) Convert.ChangeType(value, underlyingType ?? typeof(T), Culture);
            }
            catch (Exception e) when (e is FormatException || e is InvalidCastException)
            {
                throw new FormatException(FormattableString.Invariant($"Could not convert '{value}' to {typeof(T).Name}"), e);
            }
        }


        public static Dictionary<string, string> ToDictionary(this IEnumerable<string> keyValuePairStrings)
        {
            return keyValuePairStrings.Select(entry => entry.Split('='))
                .Where(entry => entry.Length == 2)
                .ToDictionary(entry => entry[0], entry => entry[1]);
        }


        public static string ToExcelColumnName(this int columnNumber)
        {
            // we start at column# 0, and the algorithm assumes 1
            var dividend = columnNumber + 1;
            var columnName = string.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }


        public static string ToRoundTripFormat(this DateTime value)
        {
            return value.ToString("o", Culture);
        }


        public static string ToTitleCase(this string input)
        {
            return Culture.TextInfo.ToTitleCase(input);
        }


        public static Type ToType(this string type)
        {
            var typeKey = type.ToUpperInvariant();
            if (TypeDictionary.ContainsKey(typeKey)) return TypeDictionary[typeKey];
            // none of the standard types, check if we can map it
            var evalType = Type.GetType(type);
            if (evalType != null) return evalType;
            // don't know what this is, give up.
            throw new ArgumentException(FormattableString.Invariant($"Type '{type}' not recognized."), nameof(type));
        }


        /// <summary>
        /// Convert a value to the specified type, and return a default value if the conversion fails
        /// </summary>
        /// <typeparam name="T">The type to convert to (can be nullable)</typeparam>
        /// <param name="value">The value to convert</param>
        /// <param name="defaultValue">the default value to return if the conversion fails</param>
        /// <returns>the converted value</returns>
        public static T ToWithDefault<T>(this object value, T defaultValue)
        {
            try
            {
                return To<T>(value);
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }


        public static T ValueOrDefault<T>(this Dictionary<string, string> dictionary, string key, T defaultValue)
        {
            return dictionary.ContainsKey(key) && dictionary[key] != null ? dictionary[key].To<T>() : defaultValue;
        }
    }
}