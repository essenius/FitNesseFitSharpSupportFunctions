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
using System.Collections.Generic;
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
        public Date(long ticks) => DateTime = new DateTime(ticks);

        public Date(DateTime date) => DateTime = date;

        public Date(string input) => DateTime = input.To<DateTime>();
        public static string DefaultFormat { get; set; } = DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern;

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Date class assignable to a FitNesse symbol"},
            {nameof(AddDays), "Add a number of days. Can be negative and/or contain fractions"},
            {nameof(AddHours), "Add a number of hours. Can be negative and/or contain fractions"},
            {nameof(DateTime), "Return the underlying DateTime object"},
            {nameof(DefaultFormat), "The default date/time format. If not set, SortableDateTimePattern is taken."},
            {"set_" + nameof(DefaultFormat), "Set the default date/time format (.Net specification)"},
            {nameof(Formatted), "Return the date in the specified format (.Net specification)"},
            {nameof(Parse), "Parse a string into a date object. FitNesse calls this when using as a parameter"},
            {nameof(ParseFormatted), "Parse a string into a date object using a specific date format."},
            {nameof(ResetDefaultFormat), "Reset the default date/time format to SortableDateTimePattern"},
            {nameof(Ticks), "Return the Ticks representation"},
            {nameof(TimeFormat), "Return the time format"},
            {nameof(ToLocal), "Return the date converted to local time"},
            {nameof(ToUtc), "Return the date converted to UTC"},
            {nameof(ToLocalFormat), "Return the date in the user's default formatting"},
            {nameof(ShortDateFormat), "Return the current short date format"}
        };

        public static string ShortDateFormat => new RegistryWrapper().ShortDateFormat;

        public static string TimeFormat => new RegistryWrapper().TimeFormat;

        public DateTime DateTime { get; }

        public long Ticks => DateTime.Ticks;

        public string ToLocalFormat => DateTime.Formatted(new RegistryWrapper().DateTimeFormat);

        /// <summary>
        ///     The function that FitNesse calls when it parses Date parameters
        /// </summary>
        /// <param name="input">the input to be parsed</param>
        /// <returns>a new Date object based in the input</returns>
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

        public static Date ParseFormatted(string input, string format)
        {
            Debug.Assert(input != null, "input != null");
            Debug.Assert(format != null, "format != null");
            var dateTime = DateTime.ParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            return new Date(dateTime);
        }

        public static void ResetDefaultFormat()
        {
            DefaultFormat = DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern;
        }

        public Date AddDays(double days) => new Date(DateTime.AddDays(days));

        public Date AddHours(double hours) => new Date(DateTime.AddHours(hours));

        public string Formatted(string format) => DateTime.Formatted(format);

        public Date ToLocal() => new Date(DateTime.ToLocalTime());

        public override string ToString() => DateTime.Formatted(DefaultFormat);

        public Date ToUtc() => new Date(DateTime.ToUniversalTime());
    }
}