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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using SupportFunctions.Utilities;
using static System.FormattableString;

namespace SupportFunctions
{
    /// <summary>Frequently needed functions for FitNesse tests. Useful as library</summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Required for FitSharp"),
     SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Required for FitSharp"),
     SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global", Justification = "Required for FitSharp")]
    public class CommonFunctions
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
        public static Date AddDaysTo(double days, Date date) => date.AddDays(days);

        /// <summary>Add a number of hours to a date. Value can contain fractions and be negative</summary>
        public static Date AddHoursTo(double hours, Date date) => date.AddHours(hours);

        /// <summary>Calculate the result of a numerical expression. Shorthand for Evaluate As with type double.</summary>
        public static object Calculate(string expression) => Evaluate(expression, typeof(double), null);

        /// <summary>Alias for Echo. Required by FitNesse to enable use of symbols in decision table results.</summary>
        public static object CloneSymbol(object symbol) => symbol;

        [Obsolete("Use Concatenate instead")]
#pragma warning disable 1591 // We don't need XML documentation for obsolete functions
        public static string Concat(string[] input) => Concatenate(input);
#pragma warning restore 1591

        /// <summary>Concatenate a list of values into a single string value</summary>
        public static string Concatenate(string[] input) => input.Aggregate(string.Empty, (current, entry) => current + entry);

        /// <returns>a date using a specific format (using standard .Net convention)</returns>
        public static string DateFormatted(Date date, string format)
        {
            Debug.Assert(date != null, "date != null");
            return date.Formatted(format);
        }

        /// <summary>Experimental - do not use</summary>
        public static object Do(string function) => DoOnWithParams(function, null);

        /// <summary>Experimental - do not use</summary>
        public static object DoOn(string function, string input) => DoOnWithParams(function, input);

        /// <summary>Experimental - do not use</summary>
        public static object DoOnWithParam(string function, string input, object parameter) => DoOnWithParams(function, input, parameter);

        /// <summary>Experimental - do not use</summary>
        public static object DoOnWithParams(string function, string input, params object[] parameters)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            // if we are asking for a property, the input could be null
            var convertedInput = input?.CastToInferredType();
            var inputType = convertedInput?.GetType();

            //we use a list here to make it easier to insert the input value for static calls
            var convertedParams = parameters.Select(p => ((string) p).CastToInferredType()).ToList();

            // assume it is a static call (e.g. Math) if there is a dot in the name
            // Look for the last dot; the class could be fully specified (and also contain dots)
            if (function.Contains(".")) return StaticFunctionCall(function, convertedInput, convertedParams);

            // No static call, so try if it is a property or method on the object
            var types = convertedParams.Select(t => t.GetType()).ToArray();
            if (TryGetPropertyFieldOrMethod(inputType, function, convertedInput, types, convertedParams.ToArray(), out var outValue))
            {
                return outValue;
            }

            // final fallback: try string
            if (TryGetPropertyFieldOrMethod(typeof(string), function, input, types, convertedParams.ToArray(), out outValue))
            {
                return outValue;
            }
            throw new MissingMethodException(MissingPropertyFieldOrMethodMessage(function, inputType));
        }

        /// <returns>the input parameter. Useful for initializing symbols</returns>
        public static object Echo(object input) => input;

        private static object Evaluate(string expression, Type type, IEnumerable<string> parameters)
        {
            // making use of the fact that DataTables come with a handy eval function
            using (var dataTable = new DataTable {Locale = CultureInfo.InvariantCulture})
            {
                // expect that all parameters have the same type as the expression
                // otherwise the specification becomes too complex (params can also contain expressions)
                var columnDictionary = parameters?.ToDictionary() ?? new Dictionary<string, string>();
                columnDictionary.Add("Eval", expression);
                foreach (var entry in columnDictionary)
                {
                    var paramColumn = new DataColumn(entry.Key, type, entry.Value);
                    dataTable.Columns.Add(paramColumn);
                }
                // all columns are expressions so no need to add data. We just need a row to refer to.
                dataTable.Rows.Add(dataTable.NewRow());
                return dataTable.Rows[0]["Eval"];
            }
        }

        /// <summary>Evaluate an expression</summary>
        /// <param name="expression">
        ///     the expression to evaluate. Supported operations: addition (+), subtraction (-), multiplication (*),
        ///     division (/), and modulo (%).
        ///     Supported functions: LEN(expression), ISNULL(expression, replacement), IIF(expression, trueValue, falseValue),
        ///     TRIM(expression), SUBSTRING(expression, start, length)
        /// </param>
        /// <param name="type">
        ///     the type to return. Supported types are bool, date, decimal, double, int, long, string,
        ///     and the full names of standard .Net types such as System.String, System.Int32, System.DateTime
        /// </param>
        /// <returns>the result as a specified type</returns>
        public static object EvaluateAs(string expression, string type) => EvaluateAsWithParams(expression, type, null);

        /// <summary>Experimental - do not use</summary>
        public static object EvaluateAsWithParams(string expression, string rawType, string[] parameters)
        {
            Debug.Assert(rawType != null, "type != null");
            var type = rawType.ToType();
            return type == typeof(Date)
                ? Date.Parse(Evaluate(expression, typeof(DateTime), parameters).To<string>())
                : Evaluate(expression, type, parameters);
        }

        private static Type FindStaticClass(string className)
        {
            var type = Type.GetType(className) ?? Type.GetType("System." + className.ToTitleCase());
            if (type == null) throw new TypeLoadException(Invariant($"Could not find static class '{className}'"));
            return type;
        }

        /// <summary>Evaluate a Boolean expression. Shorthand for Evaluate As with type bool</summary>
        /// <param name="expression">
        ///     The expression to evaluate. Supported operations are AND, OR and NOT.
        ///     Comparisons can be &gt;, &lt;, &gt;=, &lt;=, =, &lt;&gt;, IN, LIKE. For LIKE, wildcards (%, *) can be used at the beginning
        ///     and/or end of the pattern. For IN, specify the values between parentheses, e.g. ′a′ IN (′a′, ′b′, ′c′)
        /// </param>
        /// <returns>whether the expression evaluated to True</returns>
        public static bool IsTrue(string expression) => (bool) EvaluateAs(expression, "bool");

        /// <returns>the leftmost characters of a string</returns>
        public static string LeftmostOf(int length, string input)
        {
            Debug.Assert(input != null, "input != null");
            return input.Length < length ? input : input.Substring(0, length);
        }

        private static string MissingPropertyFieldOrMethodMessage(string function, Type inputType) =>
            Invariant($"Could not find property, field or method '{function}' for type ") +
            (inputType != null && inputType.Name != "String" ? Invariant($"'{inputType.Name}' or ") : "") + "'String'";

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
            Debug.Assert(input != null, "input != null");
            return input.Length < length ? input : input.Substring(input.Length - length, length);
        }

        private static object StaticFunctionCall(string function, object convertedInput, List<object> convertedParams)
        {
            var index = function.LastIndexOf(".", StringComparison.Ordinal);
            var className = function.Substring(0, index).Trim();
            var methodName = function.Substring(index + 1).Trim();
            var type = FindStaticClass(className);

            // insert the input  to the parameter arrays if not null - it is the first parameter for static calls
            if (convertedInput != null) convertedParams.Insert(0, convertedInput);
            var types = convertedParams.Select(t => t.GetType()).ToArray();
            if (TryGetPropertyFieldOrMethod(type, methodName, null, types, convertedParams.ToArray(), out var outValue))
            {
                return outValue;
            }
            throw new MissingMethodException(Invariant($"Could not find static property, field or method '{function}'"));
        }

        /// <returns>the difference in ticks between two input dates</returns>
        /// <remarks>If the start date is after the end date, the result will be negative</remarks>
        /// <param name="dateFrom">start date</param>
        /// <param name="dateTo">end date</param>
        public static long TicksBetweenAnd(Date dateFrom, Date dateTo)
        {
            Debug.Assert(dateFrom != null, "dateFrom != null");
            Debug.Assert(dateTo != null, "dateTo != null");
            return dateTo.Ticks - dateFrom.Ticks;
        }

        /// <returns>the difference in ticks between the input date and the current time. Shorthand for Ticks Between And Now.</returns>
        public static long TicksSince(Date date)
        {
            Debug.Assert(date != null, "date != null");
            var startTicks = date.Ticks;
            var elapsedTicks = Ticks - startTicks;
            return elapsedTicks;
        }

        /// <returns>the local date for a date in UTC.</returns>
        public static Date ToLocal(Date date) => date.ToLocal();

        /// <returns>ticks value of the input date.</returns>
        public static long ToTicks(Date date)
        {
            Debug.Assert(date != null, "date != null");
            return date.Ticks;
        }

        /// <returns>the UTC date for a local date</returns>
        public static Date ToUtc(Date date) => date.ToUtc();

        /// <returns>string without leading or trailing whitespace.</returns>
        public static string Trim(string input)
        {
            Debug.Assert(input != null, "input != null");
            return input.Trim();
        }

        private static bool TryGetPropertyFieldOrMethod(Type type, string function, object input,
            Type[] types, object[] parameters, out object output)
        {
            output = null;
            if (type == null) return false;
            var property = type.GetProperty(function);
            if (property != null)
            {
                output = property.GetValue(input, null);
                return true;
            }
            var field = type.GetField(function);
            if (field != null)
            {
                output = field.GetValue(input);
                return true;
            }
            var method = type.GetMethod(function, types);
            if (method == null)
            {
                // if we had decimal parameters, retry with double (those are more common)
                for (var i = 0; i < types.Length; i++)
                {
                    if (types[i] == typeof(decimal)) types[i] = typeof(double);
                }
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is decimal) parameters[i] = parameters[i].To<double>();
                }
                method = type.GetMethod(function, types);
            }
            if (method == null) return false;
            output = method.Invoke(input, parameters);
            return true;
        }

        /// <summary>Wait the specified number of seconds</summary>
        public static void WaitSeconds(double seconds) => Thread.Sleep(TimeSpan.FromSeconds(seconds));
    }
}
