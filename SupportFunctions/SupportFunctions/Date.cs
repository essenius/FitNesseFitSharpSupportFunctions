// Copyright 2015-2019 Rik Essenius
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
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Entry point for FitSharp"),
     Documentation("Date class assignable to a FitNesse symbol")]
    public class Date
    {
        public Date(long ticks) => DateTime = new DateTime(ticks);

        public Date(DateTime date) => DateTime = date;

        public Date(string input) => DateTime = input.To<DateTime>();

        [Documentation("Return the underlying DateTime object")]
        public DateTime DateTime { get; }

        [Documentation("The default date/time format. If not set, SortableDateTimePattern is taken.")]
        public static string DefaultFormat { get; set; } = DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern;

        [Documentation("Return the current short date format")]
        public static string ShortDateFormat => new RegistryWrapper().ShortDateFormat;

        [Documentation("Return the Ticks representation")]
        public long Ticks => DateTime.Ticks;

        [Documentation("Return the time format")]
        public static string TimeFormat => new RegistryWrapper().TimeFormat;

        [Documentation("Return the date in the user's default formatting")]
        public string ToLocalFormat => DateTime.Formatted(new RegistryWrapper().DateTimeFormat);

        [Documentation("Add a number of days. Can be negative and/or contain fractions")]
        public Date AddDays(double days) => new Date(DateTime.AddDays(days));

        [Documentation("Add a number of hours. Can be negative and/or contain fractions")]
        public Date AddHours(double hours) => new Date(DateTime.AddHours(hours));

        [Documentation("Return the date in the specified format (.Net specification)")]
        public string Formatted(string format) => DateTime.Formatted(format);

        [Documentation("Parse a string into a date object. FitNesse calls this when using as a parameter")]
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

        [Documentation("Parse a string into a date object using a specific date format.")]
        public static Date ParseFormatted(string input, string format)
        {
            Debug.Assert(input != null, "input != null");
            Debug.Assert(format != null, "format != null");
            var dateTime = DateTime.ParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            return new Date(dateTime);
        }

        [Documentation("Reset the default date/time format to SortableDateTimePattern")]
        public static void ResetDefaultFormat()
        {
            DefaultFormat = DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern;
        }

        [Documentation("Return the date converted to local time")]
        public Date ToLocal() => new Date(DateTime.ToLocalTime());

        public override string ToString() => DateTime.Formatted(DefaultFormat);

        [Documentation("Return the date converted to UTC")]
        public Date ToUtc() => new Date(DateTime.ToUniversalTime());
    }
}