// Copyright 2016-2020 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using SupportFunctions.Model;

namespace SupportFunctionsTest
{
    internal class ValueComparisonMock : IValueComparison
    {
        public ValueComparisonMock(object expected, object actual, CompareOutcome outcome)
        {
            ExpectedValueOut = expected.ToString();
            ActualValueOut = actual.ToString();
            Outcome = outcome;
        }

        public string ActualValueOut { get; }

        public string DeltaMessage => string.Empty;

        public string DeltaOut => string.Empty;

        public string DeltaPercentageMessage => string.Empty;

        public string ExpectedValueOut { get; }

        public bool IsOk() => ValueComparison.IsOk(Outcome);

        public CompareOutcome Outcome { get; set; }

        public string TableResult(string message) => string.Empty;

        public string ValueMessage => string.Empty;
    }
}
