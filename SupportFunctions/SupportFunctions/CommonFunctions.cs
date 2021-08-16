// Copyright 2015-2021 Rik Essenius
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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using SupportFunctions.Utilities;

namespace SupportFunctions
{
    /// <summary>Frequently needed functions for FitNesse tests. Useful as library</summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "FitSharp entry point")]
    [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global", Justification = "FitSharp would not see it")]
    public sealed class CommonFunctions
    {
        /// <summary>Get or set the default date format. Formatting follows the standard .Net conventions</summary>
        public static string DateFormat
        {
            get => Date.DefaultFormat;
            set => Date.DefaultFormat = value;
        }

        /// <summary>
        ///     Return the current ticks value. Ticks are the number of 100-nanosecond intervals that have elapsed since midnight,
        ///     January 1, 0001 (0:00:00 UTC) in the Gregorian calendar. It will always return a unique higher value
        /// </summary>
        public static long Ticks => UniqueDateTime.NowTicks;

        /// <summary>Add a number of days to a date. Value can contain fractions and be negative</summary>
        public static Date AddDaysTo(double days, Date date)
        {
            Requires.NotNull(date, nameof(date));
            return date.AddDays(days);
        }

        /// <summary>Add a number of hours to a date. Value can contain fractions and be negative</summary>
        public static Date AddHoursTo(double hours, Date date)
        {
            Requires.NotNull(date, nameof(date));
            return date.AddHours(hours);
        }

        /// <summary>Calculate the result of a numerical expression. Shorthand for Evaluate As with type 'object'.</summary>
        public static object Calculate(string expression) => EvaluateExpression(expression, typeof(object), null);

        /// <summary>Alias for Echo. Required by FitNesse to enable use of symbols in decision table results.</summary>
        public static object CloneSymbol(object symbol) => symbol;

        [Obsolete("Use Concatenate instead")]
        public static string Concat(string[] input) => Concatenate(input);

        /// <summary>Concatenate a list of values into a single string value</summary>
        public static string Concatenate(string[] input) => input.Aggregate(string.Empty, (current, entry) => current + entry);

        /// <returns>a date using a specific format (using standard .Net convention)</returns>
        public static string DateFormatted(Date date, string format)
        {
            Requires.NotNull(date, nameof(date));
            return date.Formatted(format);
        }

        /// <returns>the input parameter. Useful for initializing symbols</returns>
        public static object Echo(object input) => input;

        private static object EvaluateExpression(string expression, Type type, IEnumerable<string> parameters)
        {
            // making use of the fact that DataTables come with a handy eval function
            using var dataTable = new DataTable {Locale = CultureInfo.InvariantCulture};
            var columnDictionary = parameters?.ToDictionary() ?? new Dictionary<string, string>();
            columnDictionary.Add("Eval", expression);
            foreach (var entry in columnDictionary)
            {
                // we take the object type for all parameters, and the specified type for the evaluation result
                var paramColumn = new DataColumn(entry.Key, entry.Key == "Eval" ? type : typeof(object), entry.Value);
                dataTable.Columns.Add(paramColumn);
            }

            // all columns are expressions so no need to add data. We just need a row to refer to.
            dataTable.Rows.Add(dataTable.NewRow());
            return dataTable.Rows[0]["Eval"];
        }

        /// <summary>Evaluate an expression</summary>
        /// <param name="expression">
        ///     the expression to evaluate. Supported operations: addition (+), subtraction (-), multiplication (*),
        ///     division (/), and modulo (%).
        ///     Supported functions: LEN(expression), ISNULL(expression, replacement), IIF(expression, trueValue, falseValue),
        ///     TRIM(expression), SUBSTRING(expression, start, length)
        /// </param>
        /// <remarks>
        ///     See <a href="https://docs.microsoft.com/en-us/dotnet/api/system.data.datacolumn.expression">DataColumn documentation</a>
        ///     for more information
        /// </remarks>
        /// <returns>the result as a suitable type</returns>
        public static object Evaluate(string expression) => EvaluateExpression(expression, typeof(object), null);

        /// <summary>Evaluate an expression</summary>
        /// <param name="expression">the expression to evaluate (see <see cref="Evaluate" />)</param>
        /// <param name="type">
        ///     the type to return. Supported types are bool, date, decimal, double, int, object, long, string,
        ///     and the full names of standard .Net types such as System.String, System.Int32, System.DateTime
        /// </param>
        /// <returns>the result as the specified type</returns>
        public static object EvaluateAs(string expression, string type) => EvaluateAsWithParams(expression, type, null);

        /// <summary>Experimental - do not use</summary>
        public static object EvaluateAsWithParams(string expression, string type, string[] parameters)
        {
            Requires.NotNull(type, nameof(type));
            var returnType = type.ToType();
            // Date is a special case. It can be used as a return value type, but EvaluateExpression only knows standard types. 
            // So use DateTime in the evaluation, and map it to a Date afterwards.
            return returnType == typeof(Date)
                ? Date.Parse(EvaluateExpression(expression, typeof(DateTime), parameters).To<string>())
                : EvaluateExpression(expression, returnType, parameters);
        }

        /// <summary>Experimental - do not use</summary>
        public static object EvaluateWithParams(string expression, string[] parameters) => EvaluateExpression(expression, typeof(object), parameters);

        /// <summary>EvaluateExpression a Boolean expression. Shorthand for EvaluateExpression As with type bool</summary>
        /// <param name="expression">
        ///     The expression to evaluate. Supported operations are AND, OR and NOT.
        ///     Comparisons can be &gt;, &lt;, &gt;=, &lt;=, =, &lt;&gt;, IN, LIKE. For LIKE, wildcards (%, *) can be used at the beginning
        ///     and/or end of the pattern. For IN, specify the values between parentheses, e.g. ′a′ IN (′a′, ′b′, ′c′)
        /// </param>
        /// <returns>whether the expression evaluated to True</returns>
        public static bool IsTrue(string expression) => (bool) EvaluateExpression(expression, typeof(bool), null);

        /// <returns>the leftmost characters of a string</returns>
        public static string LeftmostOf(int length, string input)
        {
            Requires.NotNull(input, nameof(input));
            return input.Length < length ? input : input.Substring(0, length);
        }

        /// <summary>Parse a string value into a Date object. </summary>
        /// <param name="date">
        ///     today will return the current date (0:00 hours); now will return current date and time.
        ///     If the input can be converted into a long, the input is assumed to be in Ticks.
        ///     Otherwise, the date should be an unambiguous date format. Safest is to use the sortable format (as per default)
        /// </param>
        /// <returns>the parsed Date object</returns>
        public static Date Parse(string date) => Date.Parse(date);

        /// <summary>Parse a string value into a Date object using a specific date format (.Net style)</summary>
        public static Date ParseFormatted(string data, string format) => Date.ParseFormatted(data, format);

        /// <summary>Escape a string for literal use in Regex expression (converts characters that need escaping)</summary>
        public static string RegexEscape(string input) => Regex.Escape(input);

        /// <summary>Unescape a Regex string (converts escaped characters)</summary>
        public static string RegexUnescape(string input) => Regex.Unescape(input);

        /// <returns>the rightmost characters of a string</returns>
        public static string RightmostOf(int length, string input)
        {
            Requires.NotNull(input, nameof(input));
            return input.Length < length ? input : input.Substring(input.Length - length, length);
        }

        /// <returns>the difference in ticks between two input dates</returns>
        /// <remarks>If the start date is after the end date, the result will be negative</remarks>
        /// <param name="dateFrom">start date</param>
        /// <param name="dateTo">end date</param>
        public static long TicksBetweenAnd(Date dateFrom, Date dateTo)
        {
            Requires.NotNull(dateFrom, nameof(dateFrom));
            Requires.NotNull(dateTo, nameof(dateTo));
            return dateTo.Ticks - dateFrom.Ticks;
        }

        /// <returns>the difference in ticks between the input date and the current time. Shorthand for Ticks Between And Now.</returns>
        public static long TicksSince(Date date)
        {
            Requires.NotNull(date, nameof(date));
            var startTicks = date.Ticks;
            var elapsedTicks = Ticks - startTicks;
            return elapsedTicks;
        }

        /// <returns>the local date for a date in UTC.</returns>
        public static Date ToLocal(Date date)
        {
            Requires.NotNull(date, nameof(date));
            return date.ToLocal();
        }

        /// <returns>ticks value of the input date.</returns>
        public static long ToTicks(Date date)
        {
            Requires.NotNull(date, nameof(date));
            return date.Ticks;
        }

        /// <returns>the UTC date for a local date</returns>
        public static Date ToUtc(Date date)
        {
            Requires.NotNull(date, nameof(date));
            return date.ToUtc();
        }

        /// <returns>string without leading or trailing whitespace.</returns>
        public static string Trim(string input)
        {
            Requires.NotEmpty(input, nameof(input));
            return input.Trim();
        }

        /// <summary>Wait the specified number of seconds</summary>
        public static void WaitSeconds(double seconds) => Thread.Sleep(TimeSpan.FromSeconds(seconds));
    }
}
