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
using static System.Globalization.CultureInfo;

namespace SupportFunctions.Utilities
{
    internal static class StringFunctions
    {
        public static bool EqualsIgnoreCase(this string input, string compareTo) =>
            input.Trim().Equals(compareTo, StringComparison.OrdinalIgnoreCase);

        public static string FillIn(this string input, object objectToInsert) => 
            string.Format(InvariantCulture, input, objectToInsert);

        public static string Formatted(this DateTime input, string format) => 
            input.ToString(format, InvariantCulture);

        #region Table Table Output Interface

        public static string Fail(this object message) => $"fail:{message}";

        public static string Pass(this object message) => $"pass:{message}";

        public static string Report(this object message) => $"report:{message}";

        #endregion
    }
}
