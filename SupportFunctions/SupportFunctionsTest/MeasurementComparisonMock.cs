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

using System;
using SupportFunctions.Model;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    internal class MeasurementComparisonMock : IMeasurementComparison
    {
        private readonly CompareOutcome _outcome;

        public MeasurementComparisonMock(object expected, object actual, CompareOutcome outcome)
        {
            var timestamp = new DateTime().ToRoundTripFormat();
            Timestamp = new ValueComparisonMock(timestamp, timestamp, outcome);
            Value = new ValueComparisonMock(expected, actual, outcome);
            IsGood = new ValueComparisonMock(true, true, CompareOutcome.None);
            _outcome = outcome;
        }

        public IValueComparison IsGood { get; }

        public bool IsOk() => ValueComparison.IsOk(_outcome);

        public string OutcomeMessage => _outcome.ToString();

        public object TableResult(string message) => string.Empty;

        public IValueComparison Timestamp { get; }

        public IValueComparison Value { get; }
    }
}
