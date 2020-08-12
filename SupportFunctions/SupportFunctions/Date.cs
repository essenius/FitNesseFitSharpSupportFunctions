// Copyright 2015-2020 Rik Essenius
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctions
{
    /// <summary>
    ///     Date class assignable to a FitNesse symbol
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date",
         Justification = "Date is more natural for use in FitNesse"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Entry point for FitSharp")]
    public class Date
    {
        /// <summary>Initialize new Date object</summary>
        /// <param name="ticks">date/time in ticks</param>
        public Date(long ticks) => DateTime = new DateTime(ticks);

        /// <summary>Initialize date with a date type</summary>
        public Date(DateTime date) => DateTime = date;

        /// <summary>Initialize Date with a string to be parsed</summary>
        public Date(string input) => DateTime = input.To<DateTime>();

        /// <returns>the underlying DateTime object</returns>
        public DateTime DateTime { get; }

        /// <summary>The default date/time format. If not set, SortableDateTimePattern is taken.</summary>
        public static string DefaultFormat { get; set; } = DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern;

        /// <returns>the current short date format</returns>
        public static string ShortDateFormat => new RegistryWrapper().ShortDateFormat;

        /// <returns>the Ticks representation</returns>
        public long Ticks => DateTime.Ticks;

        /// <returns>the time format</returns>
        public static string TimeFormat => new RegistryWrapper().TimeFormat;

        /// <returns>the date in the user's default formatting</returns>
        public string ToLocalFormat => DateTime.Formatted(new RegistryWrapper().DateTimeFormat);

        /// <summary>Add a number of days. Can be negative and/or contain fractions</summary>
        public Date AddDays(double days) => new Date(DateTime.AddDays(days));

        /// <summary>Add a number of hours. Can be negative and/or contain fractions</summary>
        public Date AddHours(double hours) => new Date(DateTime.AddHours(hours));

        /// <returns>the date in the specified format (.Net specification)</returns>
        public string Formatted(string format) => DateTime.Formatted(format);

        /// <summary>Parse a string into a date object. FitNesse calls this when using as a parameter</summary>
        public static Date Parse(string input)
        {
            Debug.Assert(input != null, "input != null");
            switch (input.ToUpperInvariant())
            {
                case "TODAY":
                    return new Date(DateTime.Today);
                case "NOW":
                    return new Date(UniqueDateTime.NowTicks);
                case @"UTCTODAY":
                    return new Date(DateTime.UtcNow.Date);
                case @"UTCNOW":
                    return new Date(UniqueDateTime.UtcNowTicks);
                default:
                    // if input is a long, assume it represents ticks. Otherwise, assume it is a date string
                    return long.TryParse(input, out var ticks) ? new Date(ticks) : new Date(input);
            }
        }

        /// <summary>Parse a string into a date object using a specific date format.</summary>
        public static Date ParseFormatted(string input, string format)
        {
            Debug.Assert(input != null, "input != null");
            Debug.Assert(format != null, "format != null");
            var dateTime = DateTime.ParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            return new Date(dateTime);
        }

        /// <summary>Reset the default date/time format to SortableDateTimePattern</summary>
        public static void ResetDefaultFormat()
        {
            DefaultFormat = DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern;
        }

        /// <returns>the date converted to local time</returns>
        public Date ToLocal() => new Date(DateTime.ToLocalTime());

        /// <returns>the date in default format</returns>
        public override string ToString() => DateTime.Formatted(DefaultFormat);

        /// <returns>the date converted to UTC</returns>
        public Date ToUtc() => new Date(DateTime.ToUniversalTime());
    }
}
