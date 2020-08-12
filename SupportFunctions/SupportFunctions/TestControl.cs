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

using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

namespace SupportFunctions
{
    /// <summary>Control stopping tests/suites from within a test</summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Required for FitNesse"),
     SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Used by FitSharp")]
    public class TestControl
    {
        /// <summary>Throw a StopSuiteException if argument is true</summary>
        public static void StopSuiteIf(string condition) => ThrowStopIf(condition, true, true);

        /// <summary>Throw a StopSuiteException if argument is false</summary>
        public static void StopSuiteIfNot(string condition) => ThrowStopIf(condition, true, false);

        /// <summary>Throw a StopTestException if argument is true</summary>
        public static void StopTestIf(string condition) => ThrowStopIf(condition, false, true);

        /// <summary>Throw a StopTestException if argument is false</summary>
        public static void StopTestIfNot(string condition) => ThrowStopIf(condition, false, false);

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "intentional precondition check")]
        private static void ThrowStopIf(string conditionString, bool stopSuite, bool expectedValue)
        {
            var canParse = bool.TryParse(conditionString, out var condition);
            if (canParse && condition != expectedValue) return;
            var message = canParse
                ? Invariant($"'{conditionString}' evaluates to stop condition '{condition}'")
                : Invariant($"Could not parse '{conditionString}' to boolean");
            if (stopSuite) throw new StopSuiteException(message);
            throw new StopTestException(message);
        }
    }
}